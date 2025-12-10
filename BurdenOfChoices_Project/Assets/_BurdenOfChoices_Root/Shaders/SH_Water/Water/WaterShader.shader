Shader "Unlit/WaterShader"
{
    Properties
    {
        _BaseTex ("Base Texture", 2D) = "white" {}
        _Mask ("Charco Mask", 2D) = "white" {}
        _WaterColor ("Water Color", Color) = (0.15,0.2,0.25,0.5)
        _Size ("Charco Size", Vector) = (1,1,1,0)
        _Offset ("Charco Offset", Vector) = (0,0,0,0)
        _Distortion ("Distortion Amount", Range(0,0.05)) = 0.02
        _EdgeFalloff ("Edge Falloff", Range(0.1,3)) = 1.2
        _DepthFalloff ("Depth Falloff", Range(0,3)) = 1.2
        _WaterAlpha ("Water Opacity", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BaseTex;
            sampler2D _Mask;

            float4 _WaterColor;
            float3 _Size;
            float3 _Offset;
            float _Distortion;
            float _EdgeFalloff;
            float _DepthFalloff;
            float _WaterAlpha;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 baseUV : TEXCOORD0;
                float2 maskUV : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                float3 localPos = v.vertex.xyz;

                o.pos = UnityObjectToClipPos(float4(localPos,1));
                o.baseUV = v.uv;

                // UV del charco centrada y escalable
                o.maskUV = (v.uv - 0.5) * _Size.xy + 0.5 + _Offset.xy;

                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // Evitar fuera de rango
                if(i.maskUV.x < 0 || i.maskUV.x > 1 || i.maskUV.y < 0 || i.maskUV.y > 1)
                    return tex2D(_BaseTex, i.baseUV);

                // Máscara del charco
                float mask = tex2D(_Mask, i.maskUV).r;
                if(mask <= 0.01) return tex2D(_BaseTex, i.baseUV);

                // Gradiente radial para bordes y distorsión
                float2 center = float2(0.5,0.5);
                float2 toCenter = i.maskUV - center;
                float dist = length(toCenter);
                float edge = smoothstep(1.0, 0.0, dist);
                edge = pow(edge, _EdgeFalloff);
                float depth = pow(1.0 - dist, _DepthFalloff);

                // Micro-distorsión contenida dentro del charco
                float2 distort = float2(
                    sin(_Time.y*0.5 + i.maskUV.x*10),
                    cos(_Time.y*0.5 + i.maskUV.y*10)
                ) * _Distortion * mask * edge;

                // Color del charco
                float3 waterCol = _WaterColor.rgb * mask * depth;

                // Mezclar con textura base (sin escalar)
                float3 baseCol = tex2D(_BaseTex, i.baseUV + distort).rgb;

                float alpha = _WaterAlpha * mask * depth * edge;
                float3 final = lerp(baseCol, waterCol, alpha);

                return float4(final, 1);
            }

            ENDHLSL
        }
    }
}
