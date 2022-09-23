Shader "Unlit/ScrollingParticleObscureShader"
{
    Properties
    {
		[HDR] _Color("Color", Color) = (1,1,1,1)
		_MainTex ("Particle Texture 1", 2D) = "white" {}
		_MainStr ("Texture 1 Strength", Range (0, 1)) = 1 
		_MainTex2 ("Particle Texture 2", 2D) = "white" {}
		_Main2Str ("Texture 2 Strength", Range(0, 1)) = 0
		_MainTex3 ("Particle Texture 3", 2D) = "white" {}
		_Main3Str ("Texture 3 Strength", Range(0, 1)) = 0

		[Toggle] _UseValAsAlp ("Use Texture Value as Alpha", float) = 0

		[Header(Scrolling Properties)] 
		_ScrollSpd("T1 Scroll Speed", Vector) = (0,0,0,0)
		_ScrollSpd2("T2 Scroll Speed", Vector) = (0,0,0,0)
		_ScrollSpd3("T3 Scroll Speed", Vector) = (0,0,0,0)

		_MaskTex("Mask Texture (B&W)", 2D) = "white" {}
		[Toggle] _MaskUseMainSize("Mask copies Tiling/Offset of Main", float) = 0
		[Toggle] _MaskApplySpeed("Mask follows speed of Main", float) = 0

		[Header(Dissolve Properties)]
		_DissTex("Dissolve Texture", 2D) = "white" {}
		_DissVal("Value of Dissolve", float) = 0
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		//Cull back
		ZWrite On
		//ColorMask 0
		//Cull Off //makes it double sides on meshes
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			ZWrite On
			ColorMask 0
		}

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
                float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float4 custom1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD3; //uvs for texture 2
				float2 uv3 : TEXCOORD4; //uvs for texture 3
				float2 altuv : TEXCOORD5; //alternate UVs for mask
				float2 dissuv : TEXCOORD2;
				UNITY_FOG_COORDS(6)	// changed to 6 because of duplicate texcoord error on others 
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 custom1 : TEXCOORD1;
            };

			fixed4 _Color;
            sampler2D _MainTex;
			float _MainStr;
			sampler2D _MainTex2;
			float _Main2Str;
			sampler2D _MainTex3;
			float _Main3Str;

			float _UseValAsAlp;
			fixed4 _ScrollSpd;
			fixed4 _ScrollSpd2;
			fixed4 _ScrollSpd3;
            float4 _MainTex_ST;
			float4 _MainTex2_ST;
			float4 _MainTex3_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float _MaskUseMainSize;
			float _MaskApplySpeed;

			sampler2D _DissTex;
			float4 _DissTex_ST;
			float _DissVal;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex).xy + frac(_Time.y * float2(_ScrollSpd.x, _ScrollSpd.y));
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				
				//uvs for texture 2 + custom offset
				o.uv2 = TRANSFORM_TEX(v.uv, _MainTex2).xy + frac(_Time.y * float2(_ScrollSpd2.x, _ScrollSpd2.y)) + frac(float2(v.custom1.x, v.custom1.y));
				//uvs for texture 3 + custom offset
				o.uv3 = TRANSFORM_TEX(v.uv, _MainTex3).xy + frac(_Time.y * float2(_ScrollSpd3.x, _ScrollSpd3.y)) + frac(float2(v.custom1.z, v.custom1.w));

				//alternate "o" uvs for mask
				//if not using mainTex size, calculate Mask UVs; else, recalculate Main UVs
				o.altuv = (1 - _MaskUseMainSize) * TRANSFORM_TEX(v.uv, _MaskTex).xy + _MaskUseMainSize * TRANSFORM_TEX(v.uv, _MainTex).xy 
				//if applySpeed is enabled, add motion
				+ _MaskApplySpeed * frac(_Time.y * float2(_ScrollSpd.x, _ScrollSpd.y));

				//uvs for dissolve
				o.dissuv = TRANSFORM_TEX(v.uv, _DissTex).xy;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//combine main 1 and 2 adjusted based on strength
				//fixed4 newMain = (tex2D(_MainTex, i.uv) * (_MainStr / (_MainStr + _Main2Str))) + (tex2D(_MainTex2, i.uv2) * (_Main2Str / (_MainStr + _Main2Str)));
				fixed4 newMain = ((1 - _MainStr + (_MainStr * tex2D(_MainTex, i.uv)) ) * (1 - _Main2Str + (_Main2Str * tex2D(_MainTex2, i.uv2)) * (1+_Main2Str))) 
				* (1 - _Main3Str + (_Main3Str * tex2D(_MainTex3, i.uv3)) * (1 + _Main3Str));

				newMain = clamp(newMain, 0, 1);

                // get main texture and mask texture,
				//lerp mainTex between itself and black based on mask, then multiply by color value
                fixed4 col = lerp(0, newMain, tex2D(_MaskTex, i.altuv).r) * _Color * i.color;

				//generate alpha from texture value or col alpha
				col.a = col.a * (1 - _UseValAsAlp) + newMain.r * _Color.a * i.color.a * _UseValAsAlp * tex2D(_MaskTex, i.altuv).r; //maskTex added so it also affects alpha

				//apply dissolve
				fixed4 noiseStep = step(tex2D(_DissTex, i.dissuv), _DissVal);
				col.a = col.a * (1 - noiseStep);

				//col = clamp(col, 0, 1);
	
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
