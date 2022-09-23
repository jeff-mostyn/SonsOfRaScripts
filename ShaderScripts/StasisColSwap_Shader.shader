Shader "Custom/StasisColSwap_Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormMap("Normal Map",2D) = "bump" {}
		_Metal("Metallic Map", 2D) = "black" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_OverText("Overlay Details", 2D) = "black" {}

		_RGBInd ("RGB Index", Range(0,2)) = 0
		_ShadMap ("Shadow Mask", 2D) = "white" {}
		[HDR]_EmisColor ("Emissive Color", Color) = (1,1,1,1)
		_EmisMap ("Emissive Mask", 2D) = "black" {}

		[Header(Color Swap Properties)]
		_PalCol1("Palette Color 1", Color) = (1,1,1,1)
		_PalCol2("Palette Color 2", Color) = (1,1,1,1)
		[Toggle] _SwapPalCols("Swap Placement of Palette Colors 1 & 2?", float) = 0
		_PalMask12("Palette Masks 1(R) & 2(G)", 2D) = "black"{}
		// R, G, & B channels are treated as seperate masks (multiple masks in one texture)

		_ExtrCol1("Extra Color 1", Color) = (1,1,1,1)
		_ExtrCol2("Extra Color 2", Color) = (1,1,1,1)
		_ExtrCol3("Extra Color 3", Color) = (1,1,1,1)
		_ExtrMask123("Extra Masks 1(R), 2(G), & 3(B)", 2D) = "black"{}

		_ExtrCol4("Extra Color 4", Color) = (1,1,1,1)
		_ExtrCol5("Extra Color 5", Color) = (1,1,1,1)
		_ExtrCol6("Extra Color 6", Color) = (1,1,1,1)
		_ExtrMask456("Extra Masks 4(R), 3(G), & 6(B)", 2D) = "black"{}
    }

	SubShader
    {
				//"RenderType"="Opaque"
		Tags {"RenderType" = "Opaque"}
        LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

        sampler2D _MainTex;
		fixed _RGBInd;
		sampler2D _ShadMap;
		fixed4 _EmisColor;
		sampler2D _EmisMap;

        struct Input
        {
            float2 uv_MainTex;
        };

		sampler2D _OverText;
		sampler2D _NormMap;
		sampler2D _Metal;
		half _Glossiness;
        fixed4 _Color;

		//col swap variables
		fixed4 _PalCol1;
		fixed4 _PalCol2;
		float _SwapPalCols;
		sampler2D _PalMask12;

		fixed4 _ExtrCol1;
		fixed4 _ExtrCol2;
		fixed4 _ExtrCol3;
		sampler2D _ExtrMask123;
		fixed4 _ExtrCol4;
		fixed4 _ExtrCol5;
		fixed4 _ExtrCol6;
		sampler2D _ExtrMask456;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			fixed4 e;

			// Swapping colors from maintex
			fixed4 c = lerp(tex2D(_MainTex, IN.uv_MainTex) * _Color, _PalCol1 * (1 - _SwapPalCols) + _PalCol2 * _SwapPalCols, tex2D(_PalMask12, IN.uv_MainTex).r); //lerp between regular texture and new color (based on mask)
			c = lerp(c, _PalCol2 * (1 - _SwapPalCols) + _PalCol1 * _SwapPalCols, tex2D(_PalMask12, IN.uv_MainTex).g); //_SwapPalCols added; if 1, normal color will be 0 and other color will be at 1
			//because PalCol2 is added second, any overlapping areas will display PalCol2
			c = lerp(c, _ExtrCol1, tex2D(_ExtrMask123, IN.uv_MainTex).r);
			c = lerp(c, _ExtrCol2, tex2D(_ExtrMask123, IN.uv_MainTex).g);
			c = lerp(c, _ExtrCol3, tex2D(_ExtrMask123, IN.uv_MainTex).b);
			c = lerp(c, _ExtrCol4, tex2D(_ExtrMask456, IN.uv_MainTex).r);
			c = lerp(c, _ExtrCol5, tex2D(_ExtrMask456, IN.uv_MainTex).g);
			c = lerp(c, _ExtrCol6, tex2D(_ExtrMask456, IN.uv_MainTex).b);

			//overlay details
			c = lerp(c, tex2D(_OverText, IN.uv_MainTex), tex2D(_OverText, IN.uv_MainTex).a);


			//for cycling through different RGB values
            if (_RGBInd >= 0 && _RGBInd < 1) { 
				c = c * tex2D (_ShadMap, IN.uv_MainTex).r;
				e = tex2D (_EmisMap, IN.uv_MainTex).r * _EmisColor;
			} else if (_RGBInd >= 1 && _RGBInd < 2) { 
				c = c * tex2D (_ShadMap, IN.uv_MainTex).g;
				e = tex2D (_EmisMap, IN.uv_MainTex).g * _EmisColor;
			} else {
				c = c * tex2D (_ShadMap, IN.uv_MainTex).b;
				e = tex2D (_EmisMap, IN.uv_MainTex).b * _EmisColor;
			}

            o.Albedo = c.rgb + e.rgb;
            // Metallic and smoothness come from slider variables
			o.Metallic = tex2D(_Metal, IN.uv_MainTex).r;
			o.Smoothness = _Glossiness;
			o.Normal = UnpackNormal(tex2D(_NormMap, IN.uv_MainTex));
            o.Alpha = c.a;
			o.Emission = e.rgb;
			//o.Alpha = 1;
        }
        ENDCG

    }
    FallBack "VertexLit"
}
