Shader "Unlit/MeshFlipBookShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
		[HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)
		_FlipValue ("Value in flip Sequence", Range (0.0, 1.5)) = 1.5 //Effective Range is 0-1, but needs 1.5 for _DisplayRange
		_DisplayRange ("Wave Power in Y Dir", Range (0.0, 0.5)) = 0.125
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

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
				float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 vertexColor : COLOR;
            };

			//sampler2D _MainTex;
            //float4 _MainTex_ST;
			float4 _EmissionColor;
			float _FlipValue;
			float _DisplayRange;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertexColor = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				//fixed4 col = i.vertexColor;
                fixed4 col = _EmissionColor;
				col.a = (step(i.vertexColor.r, _FlipValue) - step(i.vertexColor.r, (_FlipValue - _DisplayRange)));
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col.rgba);
				UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
