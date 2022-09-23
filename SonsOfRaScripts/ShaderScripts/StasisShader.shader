Shader "Custom/StasisShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
		_BackNorpMap ("Normal Map",2D) = "bump" {}
		_MetalSmooth ("Metallic Smoothness Map", 2D) = "white" {}

		_RGBInd ("RGB Index", Range(0,2)) = 0
		_ShadMap ("Shadow Mask", 2D) = "white" {}
		[HDR]_EmisColor ("Emissive Color", Color) = (1,1,1,1)
		_EmisMap ("Emissive Mask", 2D) = "black" {}


		// Blending state
		[HideInInspector] _Mode ("__mode", Float) = 3
        [HideInInspector] _SrcBlend ("__src", Float) = 0
        [HideInInspector] _DstBlend ("__dst", Float) = 1
        [HideInInspector] _ZWrite ("__zw", Float) = 0
    }

	SubShader
    {
				//"RenderType"="Opaque"
		Tags {"RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 200

		// From Mark: Don't ask me how this works, I likely understand it less than you

		//Blend [__SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend] //causes the issue when Opaque, how to fix?
		//Blend SrcAlpha OneMinusSrcAlpha
        //ColorMask RGB

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows keepalpha


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

		sampler2D _MetalSmooth;
		sampler2D _BackNorpMap;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			fixed4 c;
			fixed4 e;
			//for cycling through different RGB values
            if (_RGBInd >= 0 && _RGBInd < 1) { 
				c = tex2D (_MainTex, IN.uv_MainTex) * _Color * tex2D (_ShadMap, IN.uv_MainTex).r;
				e = tex2D (_EmisMap, IN.uv_MainTex).r * _EmisColor;
			} else if (_RGBInd >= 1 && _RGBInd < 2) { 
				c = tex2D (_MainTex, IN.uv_MainTex) * _Color * tex2D (_ShadMap, IN.uv_MainTex).g;
				e = tex2D (_EmisMap, IN.uv_MainTex).g * _EmisColor;
			} else {
				c = tex2D (_MainTex, IN.uv_MainTex) * _Color * tex2D (_ShadMap, IN.uv_MainTex).b;
				e = tex2D (_EmisMap, IN.uv_MainTex).b * _EmisColor;
			}

            o.Albedo = c.rgb + e.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = tex2D(_MetalSmooth, IN.uv_MainTex).r;
            o.Smoothness =  tex2D(_MetalSmooth, IN.uv_MainTex).r;
			o.Normal = UnpackNormal (tex2D(_BackNorpMap, IN.uv_MainTex));
            o.Alpha = c.a;
			o.Emission = e.rgb;
			//o.Alpha = 1;
        }
        ENDCG

		Pass{
			Name "ShadowCaster"
			Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
			
			sampler2D _MainTex;
			fixed _SrcBlend;
			struct Input
			{
				float2 uv_MainTex;
			};

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                //fixed4 color = tex2D(_MainTex, i.uv);
				//clip(color.a - _Cutoff);

				//Temp fix clip out shadows when transparent (_SrcBlend = 0)
				clip(_SrcBlend - 0.1);
				SHADOW_CASTER_FRAGMENT(i)
            }

            ENDCG
		}
    }
	//UsePass "StasisShader/ShadowCaster"​
    FallBack "VertexLit"
}
