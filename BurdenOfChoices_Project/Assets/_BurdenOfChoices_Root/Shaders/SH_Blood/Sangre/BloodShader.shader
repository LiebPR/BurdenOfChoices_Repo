Shader "Custom/BloodShader"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}

        _BloodTex1("Blood Texture 1", 2D) = "white" {}
        _BloodTex2("Blood Texture 2", 2D) = "white" {}
        _BloodTex3("Blood Texture 3", 2D) = "white" {}
        _BloodTex4("Blood Texture 4", 2D) = "white" {}
        _BloodTex5("Blood Texture 5", 2D) = "white" {}
        _BloodTex6("Blood Texture 6", 2D) = "white" {}

        _BloodColor1("Blood Color 1", Color) = (1,0,0,1)
        _BloodColor2("Blood Color 2", Color) = (1,0,0,1)
        _BloodColor3("Blood Color 3", Color) = (1,0,0,1)
        _BloodColor4("Blood Color 4", Color) = (1,0,0,1)
        _BloodColor5("Blood Color 5", Color) = (1,0,0,1)
        _BloodColor6("Blood Color 6", Color) = (1,0,0,1)

        _BloodAmount("Blood Amount", Range(0,1)) = 0
        _BloodCount("Blood Count per Texture", Range(1,10)) = 5
        _MinScale("Min Scale", Range(0.05,0.5)) = 0.08
        _MaxScale("Max Scale", Range(0.1,1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // -------- STRUCTS --------
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv        : TEXCOORD0;
                float3 normalWS  : TEXCOORD1;
                float3 posWS     : TEXCOORD2;
                float4 vertex    : SV_POSITION;
            };

            // -------- TEXTURES --------
            sampler2D _MainTex;

            sampler2D _BloodTex1;
            sampler2D _BloodTex2;
            sampler2D _BloodTex3;
            sampler2D _BloodTex4;
            sampler2D _BloodTex5;
            sampler2D _BloodTex6;

            // -------- BLOOD COLORS --------
            float4 _BloodColor1;
            float4 _BloodColor2;
            float4 _BloodColor3;
            float4 _BloodColor4;
            float4 _BloodColor5;
            float4 _BloodColor6;

            float _BloodAmount;
            int   _BloodCount;
            float _MinScale;
            float _MaxScale;

            // -------- HASH --------
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // -------- VERT --------
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normalWS = normalize(UnityObjectToWorldNormal(v.normal));
                o.posWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            // -------- BLOOD BLENDING --------
            fixed4 BlendBlood(fixed4 current, fixed4 sample)
            {
                return current + sample * sample.a * (1 - current.a);
            }

            fixed4 SampleBlood(sampler2D tex, float4 color, float2 uv, int seed)
            {
                fixed4 blood = fixed4(0,0,0,0);

                for (int i = 0; i < _BloodCount; i++)
                {
                    float2 offset = float2(Hash21(float2(i, seed)), Hash21(float2(i + seed, i)));
                    offset = frac(offset);

                    float scale = lerp(_MinScale, _MaxScale, Hash21(float2(i + seed, i * 2)));

                    float2 bloodUV = (uv - offset) / scale + offset;
                    fixed4 sample = tex2D(tex, bloodUV) * color;

                    if (Hash21(offset + i + seed + 3) < _BloodAmount)
                        blood = BlendBlood(blood, sample);
                }

                return blood;
            }

            // -------- FRAG --------
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);

                // BLOOD MIX
                fixed4 bloodMix = fixed4(0,0,0,0);
                bloodMix = BlendBlood(bloodMix, SampleBlood(_BloodTex1, _BloodColor1, i.uv, 1));
                bloodMix = BlendBlood(bloodMix, SampleBlood(_BloodTex2, _BloodColor2, i.uv, 10));
                bloodMix = BlendBlood(bloodMix, SampleBlood(_BloodTex3, _BloodColor3, i.uv, 20));
                bloodMix = BlendBlood(bloodMix, SampleBlood(_BloodTex4, _BloodColor4, i.uv, 30));
                bloodMix = BlendBlood(bloodMix, SampleBlood(_BloodTex5, _BloodColor5, i.uv, 40));
                bloodMix = BlendBlood(bloodMix, SampleBlood(_BloodTex6, _BloodColor6, i.uv, 50));

                fixed4 col = lerp(baseCol, bloodMix, bloodMix.a);

                // ILUMINACIÓN
                float3 N = normalize(i.normalWS);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float diff = saturate(dot(N, L));

                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

                col.rgb *= ambient + diff * _LightColor0.rgb;
                col.a = 1;
                return col;
            }

            ENDCG
        }
    }
}
