Shader "Unlit/UnlitFill"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (1,1,1,1)
		_MainTex("Particle Texture", 2D) = "white" {}

		[Toggle] _UseValAsAlp("Use Texture Value as Alpha", float) = 0
		[Toggle] _UseCustom("Use Custom Data as Fill", float) = 0

		[Header(Cutoff Properties)]
		_fill("Fill Amount", Range(0, 1)) = 0
		_falloff("Falloff", Range(0, 1)) = 0

	}
		SubShader
		{
			Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100

			ZWrite Off
			Cull Off //makes it double sides on meshes
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always //makes it always on top of meshes

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float3 uv : TEXCOORD0;
					fixed4 color : COLOR;
				};

				struct v2f
				{
					float3 uv : TEXCOORD0;
					fixed4 color : COLOR;
					UNITY_FOG_COORDS(6)	// changed to 6 because of duplicate texcoord error on others 
					float4 vertex : SV_POSITION;
				};

				fixed4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;

				float _fill;
				float _falloff;
				float _UseValAsAlp;
				float _UseCustom;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex).xy;
					o.color = v.color;
					o.uv.z = v.uv.z;
					UNITY_TRANSFER_FOG(o,o.vertex);

					return o;
				}

				float fillWithFalloff(v2f i){
					//https://www.shadertoy.com/view/3sjGWD

					float fillVal = _UseCustom > 0 ? i.uv.z : _fill;
					//edge of falloff based on fill and fallout amnt
					float falloffShift = _falloff * fillVal;
					return min(1.0, max(0.0, (fillVal - i.uv.x + falloffShift) / _falloff));
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 newMain = tex2D(_MainTex, i.uv);
					fixed4 col = newMain * _Color * i.color;
					//col = clamp(col, 0, 1);

					//generate alpha from texture value or col alpha
					col.a = col.a * (1 - _UseValAsAlp) + newMain.r * _Color.a * i.color * _UseValAsAlp;

					//apply cutoff
					col.a = col.a * fillWithFalloff(i);

					// apply fog
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG

					//getting close
					//but SIMILAR SEE THROUGH OBJECT ISSUE, INVESTIGATE 
			}
		}
}
