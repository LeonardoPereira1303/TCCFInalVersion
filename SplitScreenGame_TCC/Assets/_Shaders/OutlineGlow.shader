Shader "Custom/OutlineGlow_Stronger"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth("Outline Width", Range(0.0, 0.2)) = 0.05   // Aumentei o limite máximo

        _GlowColor("Glow Color", Color) = (0,1,1,1)
        _GlowIntensity("Glow Intensity", Range(0,10)) = 3.0       // Intensidade maior
        _GlowPower("Glow Power", Range(0.1,10)) = 1.5             // Menor power → transição mais suave
        _GlowSharpness("Glow Sharpness", Range(0.1,10)) = 3.0     // Novo controle para o decaimento
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            fixed4 _OutlineColor;
            float _OutlineWidth;

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowPower;
            float _GlowSharpness;

            v2f vert (appdata v)
            {
                v2f o;

                // Expande o modelo no sentido da normal — outline visivelmente mais espesso
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz + worldNormal * _OutlineWidth * 1.5;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos,1.0));
                o.worldNormal = worldNormal;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel ajustado para brilho mais intenso e suave
                float fresnel = pow(1.0 - saturate(dot(normalize(i.worldNormal), normalize(i.viewDir))), _GlowPower);
                float glow = pow(fresnel, _GlowSharpness) * _GlowIntensity;

                fixed4 col = _OutlineColor;
                col.rgb = lerp(col.rgb, _GlowColor.rgb, glow); // mistura suave entre cor e glow
                col.a = saturate(_OutlineColor.a + glow * 0.7);

                return col;
            }
            ENDCG
        }
    }
}
