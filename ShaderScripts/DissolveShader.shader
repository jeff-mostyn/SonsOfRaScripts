Shader "Custom/DissolveShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Background Albedo", 2D) = "white" {}
		_OverTex1 ("Overlay Image 1",2D) = "white" {}
		_OverTex2 ("Overlay Image 2",2D) = "white" {}
		_NoiseTex ("Dissolve Noise", 2D) = "white" {} //have script generated this
		_SwitVal ("Switch Value", float) = 0
		[HDR]_GlowColor ("Glow Color", Color) = (1,1,1,1)
		_GlowThick ("Glow Thickness", Range(0,0.25)) = 0.03
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", 2D) = "white" {}
		_BackNorpMap ("Background Normal Map",2D) = "bump" {}
		_NormMap1 ("Normal Map 1",2D) = "bump" {}
		_NormAlpha1 ("Normal Alpha Map 1",2D) = "white" {}
		_NormMap2 ("Normal Map 2",2D) = "bump" {}
		_NormAlpha2 ("Normal Alpha Map 2",2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" } //shader graph had it on transparent
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _OverTex1;
		sampler2D _OverTex2;
		sampler2D _NoiseTex;
		sampler2D _Metallic;
		sampler2D _BackNorpMap;
		sampler2D _NormMap1;
		sampler2D _NormAlpha1;
		sampler2D _NormMap2;
		sampler2D _NormAlpha2;

		struct Input {
			float2 uv_MainTex;
			//float2 uv_BackNorpMap;
		};

		half _Glossiness;
		float _SwitVal;
		fixed4 _Color;
		fixed4 _GlowColor;
		float _GlowThick;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			//all the variable declarations
			// Albedo comes from a texture tinted by color
			fixed4 back = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 imag1 = tex2D (_OverTex1, IN.uv_MainTex);
			fixed4 imag2 = tex2D (_OverTex2, IN.uv_MainTex);
			fixed4 noise = tex2D (_NoiseTex, IN.uv_MainTex);
			//the two blends are the back texture combined with deity image based on the alpha of deity image
			fixed4 blendText1 = lerp (back,imag1,imag1.a);
			fixed4 blendText2 = lerp (back,imag2,imag2.a);
			//using step to make noise texture into pure black and white texture
			fixed4 noiseStep = step(noise,_SwitVal);
			//lerp between two blends based on step version of noise map
			o.Albedo = lerp(blendText1,blendText2,noiseStep.r).rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = tex2D(_Metallic,IN.uv_MainTex).rgb;
			o.Smoothness = tex2D(_Metallic,IN.uv_MainTex).rgb;
			o.Alpha = back.a;

			//blending normal map (similar method)
			fixed4 backNorm = tex2D (_BackNorpMap, IN.uv_MainTex);
			fixed4 norm1 = tex2D (_NormMap1, IN.uv_MainTex);
			//normal maps read alphas differently, need another source of Alpha
			fixed4 normAlpha1 = tex2D (_NormAlpha1, IN.uv_MainTex);
			fixed4 norm2 = tex2D (_NormMap2, IN.uv_MainTex);
			fixed4 normAlpha2 = tex2D (_NormAlpha2, IN.uv_MainTex);
			fixed4 blendNorm1 = lerp (backNorm,norm1,normAlpha1.a);
			fixed4 blendNorm2 = lerp (backNorm,norm2,normAlpha2.a);
			fixed4 finalNorm = lerp(blendNorm1,blendNorm2,noiseStep.r).rgba;
			finalNorm.a = backNorm.a;
			
			o.Normal = UnpackNormal (finalNorm);
			//o.Normal = UnpackNormal (tex2D (_BackNorpMap, IN.uv_MainTex));

			/*fixed workingEmis = lerp(step(noise,_SwitVal - _GlowThick),1, noiseStep);//(noiseStep - step(noise,_SwitVal - _GlowThick));// * _GlowColor;
			o.Emission = lerp(0,workingEmis,imag1.a + imag2.a);*/
			o.Emission = lerp(0,(noiseStep - step(noise,(_SwitVal - _GlowThick))) * _GlowColor,imag1.a + imag2.a);//lerp(noiseStep,0, step(noise,_SwitVal - _GlowThick));

			/*
			Comments to fix:
			Give bigger noise on carving so its less splotchy
			no highlight on carving version
			full black on face, less elsewhere (keep face defined)
			Joe likes no outlines (especially at bottom cuz it reads more as an indentation), need to edit areas to either fix normal or add outlines to make more like indentation
			*/
		}
		ENDCG
	}
	FallBack "Diffuse"
}
/*
SHADER GRAPH TEST SHADER

Shader "PBR Master"
{
    Properties
    {
        [NoScaleOffset] Texture2D_93BEDE2F("mainAlbedo", 2D) = "white" {}
[NoScaleOffset] Texture2D_6626C0F3("image1", 2D) = "white" {}
[NoScaleOffset] Texture2D_47F3001D("image2", 2D) = "white" {}
Vector1_131186C1("switchFactor", Range(0, 1)) = 0

    }
    SubShader
    {
        Tags{ "RenderPipeline" = "LightweightPipeline"}
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Pass
        {
        	Tags{"LightMode" = "LightweightForward"}

        	// Material options generated by graph

            Blend SrcAlpha OneMinusSrcAlpha

            Cull Back

            ZTest LEqual

            ZWrite Off

        	HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

        	// -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _VERTEX_LIGHTS
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            #pragma multi_compile _ _SHADOWS_ENABLED
            #pragma multi_compile _ _LOCAL_SHADOWS_ENABLED
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _SHADOWS_CASCADE
            
        	// -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
        	#pragma fragment frag

        	// Defines generated by graph

        	#include "LWRP/ShaderLibrary/Core.hlsl"
        	#include "LWRP/ShaderLibrary/Lighting.hlsl"
        	#include "CoreRP/ShaderLibrary/Color.hlsl"
        	#include "CoreRP/ShaderLibrary/UnityInstancing.hlsl"
        	#include "ShaderGraphLibrary/Functions.hlsl"

            TEXTURE2D(Texture2D_93BEDE2F); SAMPLER(samplerTexture2D_93BEDE2F);
            TEXTURE2D(Texture2D_6626C0F3); SAMPLER(samplerTexture2D_6626C0F3);
            TEXTURE2D(Texture2D_47F3001D); SAMPLER(samplerTexture2D_47F3001D);
            float Vector1_131186C1;

            struct VertexDescriptionInputs
            {
                float3 ObjectSpacePosition;
            };

            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                half4 uv0;
            };



        inline float unity_noise_randomValue (float2 uv)
        {
            return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
        }

        inline float unity_noise_interpolate (float a, float b, float t)
        {
            return (1.0-t)*a + (t*b);
        }


        inline float unity_valueNoise (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);

            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0 = unity_noise_randomValue(c0);
            float r1 = unity_noise_randomValue(c1);
            float r2 = unity_noise_randomValue(c2);
            float r3 = unity_noise_randomValue(c3);

            float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
            float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
            float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
            void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
            {
                float t = 0.0;
                for(int i = 0; i < 3; i++)
                {
                    float freq = pow(2.0, float(i));
                    float amp = pow(0.5, float(3-i));
                    t += unity_valueNoise(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;
                }
                Out = t;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
				//Out = 0 + (x - 0) * (1 - 0)/(1-0) = x? wut
            }

            void Unity_Step_float(float Edge, float In, out float Out)
            {
                Out = step(Edge, In);
            }

            void Unity_InvertColors_float(float In, float InvertColors, out float Out)
            {
                Out = abs(InvertColors - In);
            }

            void Unity_Comparison_Equal_float(float A, float B, out float Out)
            {
                Out = A == B ? 1 : 0;
            }

            void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
            {
                Out = lerp(A, B, T);
            }

            void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
            {
                Out = lerp(False, True, Predicate);
            }

            void Unity_Add_float(float A, float B, out float Out)
            {
                Out = A + B;
            }

            void Unity_Subtract_float(float A, float B, out float Out)
            {
                Out = A - B;
            }

            void Unity_Multiply_float (float4 A, float4 B, out float4 Out)
            {
                Out = A * B;
            }

            struct VertexDescription
            {
                float3 Position;
            };

            VertexDescription PopulateVertexData(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                description.Position = IN.ObjectSpacePosition;
                return description;
            }

            struct SurfaceDescription
            {
                float3 Albedo;
                float3 Normal;
                float3 Emission;
                float Metallic;
                float Smoothness;
                float Occlusion;
                float Alpha;
                float AlphaClipThreshold;
            };

            SurfaceDescription PopulateSurfaceData(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float _SimpleNoise_164866EF_Out;
                Unity_SimpleNoise_float(IN.uv0.xy, 30, _SimpleNoise_164866EF_Out);
                float _Remap_2951D8F8_Out;
                Unity_Remap_float(_SinTime.w, float2 (0,1), float2 (0,1), _Remap_2951D8F8_Out);
                float _Step_8DD7F9E_Out;
                Unity_Step_float(_SimpleNoise_164866EF_Out, _Remap_2951D8F8_Out, _Step_8DD7F9E_Out);
                float _InvertColors_BD43A28A_Out;
                float _InvertColors_BD43A28A_InvertColors = float (1
                );Unity_InvertColors_float(_Step_8DD7F9E_Out, _InvertColors_BD43A28A_InvertColors, _InvertColors_BD43A28A_Out);

                float _Comparison_EAFD6E9_Out;
                Unity_Comparison_Equal_float(_InvertColors_BD43A28A_Out, 1, _Comparison_EAFD6E9_Out);
                float4 _SampleTexture2D_8E03CAB5_RGBA = SAMPLE_TEXTURE2D(Texture2D_93BEDE2F, samplerTexture2D_93BEDE2F, IN.uv0.xy);
                float _SampleTexture2D_8E03CAB5_R = _SampleTexture2D_8E03CAB5_RGBA.r;
                float _SampleTexture2D_8E03CAB5_G = _SampleTexture2D_8E03CAB5_RGBA.g;
                float _SampleTexture2D_8E03CAB5_B = _SampleTexture2D_8E03CAB5_RGBA.b;
                float _SampleTexture2D_8E03CAB5_A = _SampleTexture2D_8E03CAB5_RGBA.a;
                float4 _SampleTexture2D_1FA5167E_RGBA = SAMPLE_TEXTURE2D(Texture2D_6626C0F3, samplerTexture2D_6626C0F3, IN.uv0.xy);
                float _SampleTexture2D_1FA5167E_R = _SampleTexture2D_1FA5167E_RGBA.r;
                float _SampleTexture2D_1FA5167E_G = _SampleTexture2D_1FA5167E_RGBA.g;
                float _SampleTexture2D_1FA5167E_B = _SampleTexture2D_1FA5167E_RGBA.b;
                float _SampleTexture2D_1FA5167E_A = _SampleTexture2D_1FA5167E_RGBA.a;
                float4 _Lerp_196F5DA2_Out;
                Unity_Lerp_float4(_SampleTexture2D_8E03CAB5_RGBA, _SampleTexture2D_1FA5167E_RGBA, (_SampleTexture2D_1FA5167E_A.xxxx), _Lerp_196F5DA2_Out);
                float4 _SampleTexture2D_C5899FB3_RGBA = SAMPLE_TEXTURE2D(Texture2D_47F3001D, samplerTexture2D_47F3001D, IN.uv0.xy);
                float _SampleTexture2D_C5899FB3_R = _SampleTexture2D_C5899FB3_RGBA.r;
                float _SampleTexture2D_C5899FB3_G = _SampleTexture2D_C5899FB3_RGBA.g;
                float _SampleTexture2D_C5899FB3_B = _SampleTexture2D_C5899FB3_RGBA.b;
                float _SampleTexture2D_C5899FB3_A = _SampleTexture2D_C5899FB3_RGBA.a;
                float4 _Lerp_22C1D44A_Out;
                Unity_Lerp_float4(_SampleTexture2D_8E03CAB5_RGBA, _SampleTexture2D_C5899FB3_RGBA, (_SampleTexture2D_C5899FB3_A.xxxx), _Lerp_22C1D44A_Out);
                float4 _Branch_BED2BF1_Out;
                Unity_Branch_float4(_Comparison_EAFD6E9_Out, _Lerp_196F5DA2_Out, _Lerp_22C1D44A_Out, _Branch_BED2BF1_Out);
                float4 Color_592831CE = IsGammaSpace() ? float4(0, 11.85882, 16, 0) : float4(SRGBToLinear(float3(0, 11.85882, 16)), 0);
                float _Add_4E850CEB_Out;
                Unity_Add_float(0.01, _Remap_2951D8F8_Out, _Add_4E850CEB_Out);
                float _Step_C425E841_Out;
                Unity_Step_float(_SimpleNoise_164866EF_Out, _Add_4E850CEB_Out, _Step_C425E841_Out);
                float _Subtract_1636FD1D_Out;
                Unity_Subtract_float(_Step_C425E841_Out, _Step_8DD7F9E_Out, _Subtract_1636FD1D_Out);
                float4 _Multiply_D1E336EB_Out;
                Unity_Multiply_float(Color_592831CE, (_Subtract_1636FD1D_Out.xxxx), _Multiply_D1E336EB_Out);

                float4 Color_5BD1A5E3 = IsGammaSpace() ? float4(0, 0, 0, 0) : float4(SRGBToLinear(float3(0, 0, 0)), 0);
                float _Add_541CC2C9_Out;
                Unity_Add_float(_SampleTexture2D_1FA5167E_A, _SampleTexture2D_C5899FB3_A, _Add_541CC2C9_Out);
                float _Subtract_2E602DD0_Out;
                Unity_Subtract_float(1, _Add_541CC2C9_Out, _Subtract_2E602DD0_Out);
                float4 _Lerp_832B2A5D_Out;
                Unity_Lerp_float4(_Multiply_D1E336EB_Out, Color_5BD1A5E3, (_Subtract_2E602DD0_Out.xxxx), _Lerp_832B2A5D_Out);
                surface.Albedo = (_Branch_BED2BF1_Out.xyz);
                surface.Normal = IN.TangentSpaceNormal;
                surface.Emission = (_Lerp_832B2A5D_Out.xyz);
                surface.Metallic = 0;
                surface.Smoothness = 0.5;
                surface.Occlusion = 1;
                surface.Alpha = 1;
                surface.AlphaClipThreshold = 0;
                return surface;
            }

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


        	struct GraphVertexOutput
            {
                float4 clipPos                : SV_POSITION;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 0);
        		half4 fogFactorAndVertexLight : TEXCOORD1; // x: fogFactor, yzw: vertex light
            	float4 shadowCoord            : TEXCOORD2;

        		// Interpolators defined by graph
                float3 WorldSpacePosition : TEXCOORD3;
                float3 WorldSpaceNormal : TEXCOORD4;
                float3 WorldSpaceTangent : TEXCOORD5;
                float3 WorldSpaceBiTangent : TEXCOORD6;
                float3 WorldSpaceViewDirection : TEXCOORD7;
                half4 uv0 : TEXCOORD8;
                half4 uv1 : TEXCOORD9;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            	UNITY_VERTEX_OUTPUT_STEREO
            };

            GraphVertexOutput vert (GraphVertexInput v)
        	{
        		GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
            	UNITY_TRANSFER_INSTANCE_ID(v, o);
        		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        		// Vertex transformations performed by graph
                float3 WorldSpacePosition = mul(UNITY_MATRIX_M,v.vertex);
                float3 WorldSpaceNormal = mul(v.normal,(float3x3)UNITY_MATRIX_I_M);
                float3 WorldSpaceTangent = mul((float3x3)UNITY_MATRIX_M,v.tangent.xyz);
                float3 WorldSpaceBiTangent = normalize(cross(WorldSpaceNormal, WorldSpaceTangent.xyz) * v.tangent.w);
                float3 WorldSpaceViewDirection = SafeNormalize(_WorldSpaceCameraPos.xyz - mul(GetObjectToWorldMatrix(), float4(v.vertex.xyz, 1.0)).xyz);
                float4 uv0 = v.texcoord0;
                float4 uv1 = v.texcoord1;
                float3 ObjectSpacePosition = mul(UNITY_MATRIX_I_M,float4(WorldSpacePosition,1.0));

        		VertexDescriptionInputs vdi = (VertexDescriptionInputs)0;

        		// Vertex description inputs defined by graph
                vdi.ObjectSpacePosition = ObjectSpacePosition;

        	    VertexDescription vd = PopulateVertexData(vdi);
        		v.vertex.xyz = vd.Position;

        		// Vertex shader outputs defined by graph
                o.WorldSpacePosition = WorldSpacePosition;
                o.WorldSpaceNormal = WorldSpaceNormal;
                o.WorldSpaceTangent = WorldSpaceTangent;
                o.WorldSpaceBiTangent = WorldSpaceBiTangent;
                o.WorldSpaceViewDirection = WorldSpaceViewDirection;
                o.uv0 = uv0;
                o.uv1 = uv1;

        		float3 lwWNormal = TransformObjectToWorldNormal(v.normal);
        		float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
        		float4 clipPos = TransformWorldToHClip(lwWorldPos);

         		// We either sample GI from lightmap or SH.
        	    // Lightmap UV and vertex SH coefficients use the same interpolator ("float2 lightmapUV" for lightmap or "half3 vertexSH" for SH)
                // see DECLARE_LIGHTMAP_OR_SH macro.
        	    // The following funcions initialize the correct variable with correct data
        	    OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUV);
        	    OUTPUT_SH(lwWNormal, o.vertexSH);

        	    half3 vertexLight = VertexLighting(lwWorldPos, lwWNormal);
        	    half fogFactor = ComputeFogFactor(clipPos.z);
        	    o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
        	    o.clipPos = clipPos;

        	#ifdef _SHADOWS_ENABLED
        	#if SHADOWS_SCREEN
        		o.shadowCoord = ComputeShadowCoord(clipPos);
        	#else
        		o.shadowCoord = TransformWorldToShadowCoord(lwWorldPos);
        	#endif
        	#endif
        		return o;
        	}

        	half4 frag (GraphVertexOutput IN ) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(IN);

        		// Pixel transformations performed by graph
                float3 WorldSpacePosition = IN.WorldSpacePosition;
                float3 WorldSpaceNormal = normalize(IN.WorldSpaceNormal);
                float3 WorldSpaceTangent = IN.WorldSpaceTangent;
                float3 WorldSpaceBiTangent = IN.WorldSpaceBiTangent;
                float3 WorldSpaceViewDirection = normalize(IN.WorldSpaceViewDirection);
                float3x3 tangentSpaceTransform = float3x3(WorldSpaceTangent,WorldSpaceBiTangent,WorldSpaceNormal);
                float4 uv0 = IN.uv0;
                float4 uv1 = IN.uv1;
                float3 TangentSpaceNormal = mul(WorldSpaceNormal,(float3x3)tangentSpaceTransform);

                SurfaceDescriptionInputs surfaceInput = (SurfaceDescriptionInputs)0;

        		// Surface description inputs defined by graph
                surfaceInput.TangentSpaceNormal = TangentSpaceNormal;
                surfaceInput.uv0 = uv0;

                SurfaceDescription surf = PopulateSurfaceData(surfaceInput);

        		float3 Albedo = float3(0.5, 0.5, 0.5);
        		float3 Specular = float3(0, 0, 0);
        		float Metallic = 1;
        		float3 Normal = float3(0, 0, 1);
        		float3 Emission = 0;
        		float Smoothness = 0.5;
        		float Occlusion = 1;
        		float Alpha = 1;
        		float AlphaClipThreshold = 0;

        		// Surface description remap performed by graph
                Albedo = surf.Albedo;
                Normal = surf.Normal;
                Emission = surf.Emission;
                Metallic = surf.Metallic;
                Smoothness = surf.Smoothness;
                Occlusion = surf.Occlusion;
                Alpha = surf.Alpha;
                AlphaClipThreshold = surf.AlphaClipThreshold;

        		InputData inputData;
        		inputData.positionWS = WorldSpacePosition;

        #ifdef _NORMALMAP
        	    inputData.normalWS = TangentToWorldNormal(Normal, WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal);
        #else
            #if !SHADER_HINT_NICE_QUALITY
                inputData.normalWS = WorldSpaceNormal;
            #else
        	    inputData.normalWS = normalize(WorldSpaceNormal);
            #endif
        #endif

        #if !SHADER_HINT_NICE_QUALITY
        	    // viewDirection should be normalized here, but we avoid doing it as it's close enough and we save some ALU.
        	    inputData.viewDirectionWS = WorldSpaceViewDirection;
        #else
        	    inputData.viewDirectionWS = normalize(WorldSpaceViewDirection);
        #endif

        	    inputData.shadowCoord = IN.shadowCoord;

        	    inputData.fogCoord = IN.fogFactorAndVertexLight.x;
        	    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
        	    inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);

        		half4 color = LightweightFragmentPBR(
        			inputData, 
        			Albedo, 
        			Metallic, 
        			Specular, 
        			Smoothness, 
        			Occlusion, 
        			Emission, 
        			Alpha);

        		// Computes fog factor per-vertex
            	ApplyFog(color.rgb, IN.fogFactorAndVertexLight.x);

        #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif
        		return color;
            }

        	ENDHLSL
        }
        Pass
        {
        	Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On ZTest LEqual

            // Material options generated by graph
            Cull Back

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // Defines generated by graph

            #include "LWRP/ShaderLibrary/Core.hlsl"
            #include "LWRP/ShaderLibrary/Lighting.hlsl"
            #include "ShaderGraphLibrary/Functions.hlsl"
            #include "CoreRP/ShaderLibrary/Color.hlsl"

            TEXTURE2D(Texture2D_93BEDE2F); SAMPLER(samplerTexture2D_93BEDE2F);
            TEXTURE2D(Texture2D_6626C0F3); SAMPLER(samplerTexture2D_6626C0F3);
            TEXTURE2D(Texture2D_47F3001D); SAMPLER(samplerTexture2D_47F3001D);
            float Vector1_131186C1;

            struct VertexDescriptionInputs
            {
                float3 ObjectSpacePosition;
            };


            struct VertexDescription
            {
                float3 Position;
            };

            VertexDescription PopulateVertexData(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                description.Position = IN.ObjectSpacePosition;
                return description;
            }

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


        	struct VertexOutput
        	{
        	    float2 uv           : TEXCOORD0;
        	    float4 clipPos      : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

            // x: global clip space bias, y: normal world space bias
            float4 _ShadowBias;
            float3 _LightDirection;

            VertexOutput ShadowPassVertex(GraphVertexInput v)
        	{
        	    VertexOutput o;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Vertex transformations performed by graph
                float3 WorldSpacePosition = mul(UNITY_MATRIX_M,v.vertex);
                float3 WorldSpaceNormal = mul(v.normal,(float3x3)UNITY_MATRIX_I_M);
                float4 uv1 = v.texcoord1;
                float3 ObjectSpacePosition = mul(UNITY_MATRIX_I_M,float4(WorldSpacePosition,1.0));

        		VertexDescriptionInputs vdi = (VertexDescriptionInputs)0;

                // Vertex description inputs defined by graph
                vdi.ObjectSpacePosition = ObjectSpacePosition;

        	    VertexDescription vd = PopulateVertexData(vdi);
                v.vertex.xyz = vd.Position;

        	    o.uv = uv1;
        	    
        	    float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldDir(v.normal);

                float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                float scale = invNdotL * _ShadowBias.y;

                // normal bias is negative since we want to apply an inset normal offset
                positionWS = normalWS * scale.xxx + positionWS;
                float4 clipPos = TransformWorldToHClip(positionWS);

                // _ShadowBias.x sign depens on if platform has reversed z buffer
                clipPos.z += _ShadowBias.x;

        	#if UNITY_REVERSED_Z
        	    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        	#else
        	    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        	#endif
                o.clipPos = clipPos;

        	    return o;
        	}

            half4 ShadowPassFragment(VertexOutput IN) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                return 0;
            }

            ENDHLSL
        }

        Pass
        {
        	Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            // Material options generated by graph
            Cull Back

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            // Defines generated by graph

            #include "LWRP/ShaderLibrary/Core.hlsl"
            #include "LWRP/ShaderLibrary/Lighting.hlsl"
            #include "ShaderGraphLibrary/Functions.hlsl"
            #include "CoreRP/ShaderLibrary/Color.hlsl"

            TEXTURE2D(Texture2D_93BEDE2F); SAMPLER(samplerTexture2D_93BEDE2F);
            TEXTURE2D(Texture2D_6626C0F3); SAMPLER(samplerTexture2D_6626C0F3);
            TEXTURE2D(Texture2D_47F3001D); SAMPLER(samplerTexture2D_47F3001D);
            float Vector1_131186C1;

            struct VertexDescriptionInputs
            {
                float3 ObjectSpacePosition;
            };


            struct VertexDescription
            {
                float3 Position;
            };

            VertexDescription PopulateVertexData(VertexDescriptionInputs IN)
            {
                VertexDescription description = (VertexDescription)0;
                description.Position = IN.ObjectSpacePosition;
                return description;
            }

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


        	struct VertexOutput
        	{
        	    float2 uv           : TEXCOORD0;
        	    float4 clipPos      : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

            VertexOutput vert(GraphVertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

        	    // Vertex transformations performed by graph
                float3 WorldSpacePosition = mul(UNITY_MATRIX_M,v.vertex);
                float3 WorldSpaceNormal = mul(v.normal,(float3x3)UNITY_MATRIX_I_M);
                float4 uv1 = v.texcoord1;
                float3 ObjectSpacePosition = mul(UNITY_MATRIX_I_M,float4(WorldSpacePosition,1.0));

        		VertexDescriptionInputs vdi = (VertexDescriptionInputs)0;

                // Vertex description inputs defined by graph
                vdi.ObjectSpacePosition = ObjectSpacePosition;

        	    VertexDescription vd = PopulateVertexData(vdi);
                v.vertex.xyz = vd.Position;

        	    o.uv = uv1;
        	    o.clipPos = TransformObjectToHClip(v.vertex.xyz);
        	    return o;
            }

            half4 frag(VertexOutput IN) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                return 0;
            }
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
        	Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex LightweightVertexMeta
            #pragma fragment LightweightFragmentMeta

            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION

            #pragma shader_feature _SPECGLOSSMAP

            #include "LWRP/ShaderLibrary/InputSurfacePBR.hlsl"
            #include "LWRP/ShaderLibrary/LightweightPassMetaPBR.hlsl"
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
*/