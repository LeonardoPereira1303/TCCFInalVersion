Shader "Custom/Hologram"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Hologram Color", Color) = (0, 1, 1, 1)
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _ScanSpeed ("Scanline Speed", Range(0, 20)) = 5
        _ScanFreq ("Scanline Frequency", Range(1, 100)) = 50
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.05
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Transparency;
            float _ScanSpeed;
            float _ScanFreq;
            float _DistortionStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Scanline animation
                float scan = sin(i.worldPos.y * _ScanFreq + _Time.y * _ScanSpeed);

                // Vertical distortion (wave-like)
                float distortion = sin(i.worldPos.y * 10 + _Time.y * 2) * _DistortionStrength;

                float2 uv = i.uv + distortion;

                fixed4 tex = tex2D(_MainTex, uv);

                // Combine with color and scan effect
                float intensity = (scan + 1) * 0.5;
                fixed4 col = tex * _Color;
                col.rgb *= intensity;
                col.a *= (1 - _Transparency);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
