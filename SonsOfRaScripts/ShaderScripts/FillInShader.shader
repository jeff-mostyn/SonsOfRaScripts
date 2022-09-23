Shader "Custom/FillInShader"
{
    Properties
    {
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_NormMap("Normal Map", 2D) = "bump" {}

		[Header(Fill In Values)]
		_ForeColor("Foreground Color", Color) = (1,1,1,1)
		_BackColor("Background Color", Color) = (1,1,1,1)

		_CutoffX("X Cutoff for FG Color", Range(0,1)) = 0
		[Toggle] _FlipXCut("Flip X Cutoff", float) = 0
		_OffsetX("X Offset", float) = 0

		_CutoffY("Y Cutoff for FG Color", Range(0,1)) = 0
		[Toggle] _FlipYCut("Flip Y Cutoff", float) = 0
		_OffsetY("Y Offset", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _NormMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;

		fixed4 _ForeColor;
        fixed4 _BackColor;

		float _CutoffX;
		float _FlipXCut;
		float _OffsetX;

		float _CutoffY;
		float _FlipYCut;
		float _OffsetY;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			fixed4 back = tex2D(_MainTex, IN.uv_MainTex) * _BackColor;

			//adjust x and y uvs for if result should be inverted and any offset
			float newXuv = IN.uv_MainTex.x * (1 - _FlipXCut) + (1 - IN.uv_MainTex.x) * _FlipXCut + _OffsetX;
			float newYuv = IN.uv_MainTex.y * (1 - _FlipYCut) + (1 - IN.uv_MainTex.y) * _FlipYCut + _OffsetY;
			//correct if offset pushes uv above 1 or below 0
			newXuv = newXuv > 1 ? newXuv % 1 : newXuv < 0 ? 1 - (abs(newXuv) % 1) : newXuv;
			newYuv = newYuv > 1 ? newYuv % 1 : newYuv < 0 ? 1 - (abs(newYuv) % 1) : newYuv;

			//ternary checking if uv is greater than Y or X cutoff show background color, otherwise show foreground color
			fixed4 c = newXuv > _CutoffX ? back : newYuv > _CutoffY ? back : tex2D(_MainTex, IN.uv_MainTex) * _ForeColor;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			o.Normal = UnpackNormal(tex2D(_NormMap, IN.uv_MainTex));
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
