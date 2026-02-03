Shader "Unlit/GlassShader"
{
    Properties
    {
        _Tint("Tint Color", Color) = (1,1,1,0.05)
        _FresnelPower("Fresnel Power", Range(1, 5)) = 4
        _FresnelIntensity("Fresnel Intensity", Range(0, 1)) = 0.4
        _RefractionStrength("Refraction Strength", Range(0, 0.1)) = 0.02
        _EdgeSoftness("Edge Softness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 50

        Pass
        {
            Name "FORWARD"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _FresnelPower;
                float _FresnelIntensity;
                float _RefractionStrength;
                float _EdgeSoftness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 viewDir     : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                return o;
            }

            float2 RefractUV(float2 uv, float3 normal, float strength)
            {
                return uv + normal.xy * strength;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                float3 V = normalize(i.viewDir);

                // Fresnel para bordes
                float fresnel = pow(1 - saturate(dot(N,V)), _FresnelPower) * _FresnelIntensity;

                // UV de pantalla
                float2 screenUV = i.positionHCS.xy / i.positionHCS.w * 0.5 + 0.5;

                // Desplazamiento UV según normal para refracción
                float2 refractedUV = RefractUV(screenUV, N, _RefractionStrength);

                // Tomar color de fondo
                float4 bg = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, refractedUV);

                // Mezcla con color del cristal y Fresnel
                float3 finalColor = lerp(_Tint.rgb, bg.rgb, 0.9) + fresnel * _Tint.rgb;

                // Alfa dinámica según ángulo y bordes
                float edgeFactor = saturate(1.0 - pow(dot(N,V), _EdgeSoftness));
                float alpha = saturate(_Tint.a + fresnel * edgeFactor);

                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
