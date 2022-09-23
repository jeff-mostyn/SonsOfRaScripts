// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FlagShaderUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		[Header(3D Wave Properties)]
		_WavePowerX ("Wave Power (X Dir)", Range (0.0, 1.0)) = 0.0
		_WaveFreqX("Wave Frequency (X Dir)", Range(0.0, 5.0)) = 1.0

		_WavePowerY ("Wave Power (Y Dir)", Range (0.0, 1.0)) = 0.0
		_WaveFreqY("Wave Frequency (Y Dir)", Range(0.0, 5.0)) = 1.0

		_WavePowerZ("Wave Power (Z Dir)", Range(0.0, 1.0)) = 0.0
		_WaveFreqZ("Wave Frequency (Z Dir)", Range(0.0, 5.0)) = 1.0

		_WaveSpeed ("Wave Speed", Range (0.0, 200.0)) = 1.0

		[Header(Color Swap Properties)]
		_PalCol1("Palette Color 1", Color) = (1,1,1,1)
		_PalCol2("Palette Color 2", Color) = (1,1,1,1)
		[Toggle] _SwapPalCols("Swap Placement of Palette Colors 1 & 2?", float) = 0
		_PalMask12("Palette Masks 1(R) & 2(G)", 2D) = "black"{}
		// R, G, & B channels are treated as seperate masks (multiple masks in one texture)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
				float4 color : COLOR; // defines vertex color (I think?)
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			//sampler2D _WaveInfluence;
			float _WavePowerX;
			float _WavePowerY;
			float _WavePowerZ;
			float _WaveSpeed;
			float _WaveFreqX;
			float _WaveFreqY;
			float _WaveFreqZ;

			//color swap variables
			fixed4 _PalCol1;
			fixed4 _PalCol2;
			float _SwapPalCols;
			sampler2D _PalMask12;


            v2f vert (appdata v)
            {
                v2f o;

				float4 objectOrigin = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));
				float combOrigin = objectOrigin.x + objectOrigin.y + objectOrigin.z;

				float4 vertColor = v.color;
				float t = _Time * _WaveSpeed;

				//based on how close to static "base" apply sine wave based on power, modify sine input based on WaveFreq and object position (make it different for each)
				v.vertex.x += vertColor.r * _WavePowerX * sin(t + (vertColor.r * _WaveFreqX) + combOrigin);
				v.vertex.y += vertColor.r * _WavePowerY * sin(t + (vertColor.r * _WaveFreqY) + combOrigin);
				v.vertex.z += vertColor.r * _WavePowerZ * sin(t + (vertColor.r * _WaveFreqZ) + combOrigin);

				o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				//color swap
				col	= lerp(col, _PalCol1 * (1 - _SwapPalCols) + _PalCol2 * _SwapPalCols, tex2D(_PalMask12, i.uv).r); //lerp between regular texture and new color (based on mask)
				col = lerp(col, _PalCol2 * (1 - _SwapPalCols) + _PalCol1 * _SwapPalCols, tex2D(_PalMask12, i.uv).g); //_SwapPalCols added; if 1, normal color will be 0 and other color will be at 1

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
