Shader "Ultimate/ForceFieldPickableEmission"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.1, 0.8, 1, 0.5)
        _FresnelPower ("Fresnel Power", Range(0.5,5)) = 2.0
        _EmissionStrength ("Emission Strength", Range(0,5)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            fixed4 _Color;
            half _FresnelPower;
            half _EmissionStrength;

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                o.pos = UnityWorldToClipPos(worldPos);
                o.worldPos = worldPos;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Fresnel visible desde cualquier ángulo
                float fresnel = pow(1.0 - saturate(dot(i.normal, i.viewDir)), _FresnelPower);
                fresnel = saturate(fresnel + 0.3);

                fixed4 col = _Color;

                // Alpha proporcional a Fresnel
                col.a *= fresnel;

                // Emission: color más brillante en bordes
                fixed3 emission = col.rgb * fresnel * _EmissionStrength;

                col.rgb += emission; // sumamos el brillo

                return saturate(col);
            }
            ENDCG
        }
    }
    FallBack Off
}
