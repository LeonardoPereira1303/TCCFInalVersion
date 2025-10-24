Shader "Custom/GlowRim"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (1,0.6,0.2,1)
        _GlowIntensity ("Glow Intensity", Range(0,10)) = 2.0
        _GlowPower ("Glow Power (sharpness)", Range(0.1,5.0)) = 1.5
        _RimOffset ("Rim Offset", Range(-1,1)) = 0.0

        _Cutout ("Alpha Cutoff", Range(0,1)) = 0.0
        _Mode ("Render Mode (0 Opaque, 1 Transparent)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        ZWrite On

        // Transparent mode adjustments
        Pass
        {
            Name "BASE"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowPower;
            float _RimOffset;
            float _Cutout;
            float _Mode;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = _WorldSpaceCameraPos - worldPos;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * _Color;

                // Alpha cutout support
                if (baseCol.a < _Cutout)
                    discard;

                float3 n = normalize(i.normal);
                float3 vdir = normalize(i.viewDir);

                // Fresnel-like rim: 1 - dot(view, normal) gives bright at edges
                float fresnel = saturate(1.0 - dot(vdir, n) + _RimOffset);
                float rim = pow(fresnel, _GlowPower) * _GlowIntensity;

                fixed3 emission = _GlowColor.rgb * rim;

                // Compose final color: base + emission
                fixed3 outCol = baseCol.rgb + emission;

                // If in transparent mode, output alpha; else alpha = 1
                float outAlpha = (_Mode >= 0.5) ? baseCol.a : 1.0;

                return fixed4(outCol, outAlpha);
            }
            ENDCG
        }
    }

    // If transparent mode requested, override tags / blending
    FallBack "Diffuse"
    CustomEditor "UnityEditor.MaterialEditor"
}
