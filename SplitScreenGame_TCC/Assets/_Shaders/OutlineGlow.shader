Shader "Custom/OutlineGlow"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth("Outline Width", Range(0.0, 0.1)) = 0.02

        _GlowColor("Glow Color", Color) = (0,1,1,1)
        _GlowIntensity("Glow Intensity", Range(0, 5)) = 1.5
        _GlowPower("Glow Power", Range(0.5, 8)) = 2.0
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

            v2f vert (appdata v)
            {
                v2f o;

                // posição + expansão do contorno
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz + worldNormal * _OutlineWidth;

                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos,1.0));
                o.worldNormal = worldNormal;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fresnel para glow
                float fresnel = pow(1.0 - saturate(dot(normalize(i.worldNormal), normalize(i.viewDir))), _GlowPower);
                float glow = fresnel * _GlowIntensity;

                fixed4 col = _OutlineColor;
                col.rgb += _GlowColor.rgb * glow;
                col.a = saturate(_OutlineColor.a + glow * 0.5);

                return col;
            }
            ENDCG
        }
    }
}
