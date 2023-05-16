Shader "Ultimate 10+ Shaders/Plasma" {
    Properties{
        [HDR] _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Normal("Normal map", 2D) = "bump" {}

        _NoiseTex("Noise", 2D) = "white" {}
        _MovementDirection("Movement Direction", Vector) = (0, -1, 0, 1)

        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

        SubShader{
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100
            Cull[_Cull]
            ZWrite On

            HLSLPROGRAM
            // Required for URP
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone vulkan metal switch

            // URP specific include
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float3 positionOS : POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_Normal : TEXCOORD1;
                float2 uv_NoiseTex : TEXCOORD2;
            };

            struct Varyings {
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_Normal : TEXCOORD1;
                float2 uv_NoiseTex : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _Normal;
            sampler2D _NoiseTex;
            float4 _MovementDirection;

            fixed4 _Color;

            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.uv_MainTex = input.uv_MainTex;
                output.uv_Normal = input.uv_Normal;
                output.uv_NoiseTex = input.uv_NoiseTex;

                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                input.uv_NoiseTex += _MovementDirection.xy * _Time.y / 2.0;
                input.uv_MainTex += _MovementDirection.xy * _Time.y;
                input.uv_Normal += _MovementDirection.xy * _Time.y / 2.0;

                half4 alphaPixel = tex2D(_NoiseTex, input.uv_NoiseTex);
                half4 pixel = tex2D(_MainTex, input.uv_MainTex) * _Color * alphaPixel.r;

                half3 normal = UnpackNormal(tex2D(_Normal, input.uv_Normal));

                // Use URP provided Blend modes and premultiply alpha
                half4 finalColor = PremultiplyAlpha(half4(pixel.rgb, alphaPixel.r));
                return finalColor;
            }
        }