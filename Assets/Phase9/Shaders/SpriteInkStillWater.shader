Shader "Director/Phase9/Sprite Ink Still Water"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Water Tint", Color) = (0.76, 0.95, 0.88, 0.82)
        _HighlightColor ("Soft Highlight", Color) = (1, 0.98, 0.86, 0.35)
        _BreathSpeed ("Breath Speed", Range(0, 0.2)) = 0.022
        _RippleScale ("Ripple Scale", Range(1, 32)) = 11
        _RippleStrength ("Ripple Strength", Range(0, 0.06)) = 0.014
        _HighlightIntensity ("Highlight Intensity", Range(0, 1)) = 0.16
        _InkWashStrength ("Ink Wash Strength", Range(0, 0.25)) = 0.055
        _Stillness ("Stillness", Range(0, 1)) = 0.82
        _AlphaSoftness ("Alpha Softness", Range(0, 1)) = 0.94
        _PhaseOffset ("Phase Offset", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _HighlightColor;
            float _BreathSpeed;
            float _RippleScale;
            float _RippleStrength;
            float _HighlightIntensity;
            float _InkWashStrength;
            float _Stillness;
            float _AlphaSoftness;
            float _PhaseOffset;

            float Hash21(float2 p)
            {
                p = frac(p * float2(181.27, 427.13));
                p += dot(p, p + 23.41);
                return frac(p.x * p.y);
            }

            float StillNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.uv = v.texcoord;
                o.color = v.color * _Color;

                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float calm = saturate(_Stillness);
                float motion = 1.0 - calm * 0.72;
                float t = _Time.y * _BreathSpeed + _PhaseOffset;

                float2 slowDriftA = float2(sin(t * 0.73), cos(t * 0.61)) * 0.11;
                float2 slowDriftB = float2(cos(t * 0.43), sin(t * 0.51)) * 0.07;
                float nA = StillNoise(i.uv * _RippleScale + slowDriftA);
                float nB = StillNoise(i.uv * (_RippleScale * 0.58) + slowDriftB + 9.2);

                float2 distortion = (float2(nA, nB) - 0.5) * _RippleStrength * motion;
                fixed4 baseCol = tex2D(_MainTex, i.texcoord + distortion) * i.color;

                float inkWash = ((nA + nB) * 0.5 - 0.5) * _InkWashStrength * motion;
                float breath = sin(t * 6.28318 + nB * 1.7) * 0.5 + 0.5;
                baseCol.rgb += inkWash + (breath - 0.5) * _InkWashStrength * 0.48;

                float ringWave = sin((i.uv.x * 1.7 + i.uv.y * 1.13 + nA * 0.24) * 24.0 + t * 4.0);
                float crossWave = sin((i.uv.x * -1.05 + i.uv.y * 1.85 + nB * 0.18) * 18.0 - t * 3.2);
                float highlight = smoothstep(0.68, 0.965, ringWave * 0.5 + 0.5);
                highlight += smoothstep(0.76, 0.985, crossWave * 0.5 + 0.5) * 0.55;
                highlight = saturate(highlight);
                highlight *= smoothstep(0.12, 0.68, nB) * _HighlightIntensity * motion;
                baseCol.rgb = lerp(baseCol.rgb, _HighlightColor.rgb, highlight * _HighlightColor.a);

                baseCol.a *= _AlphaSoftness;
                return baseCol;
            }
            ENDCG
        }
    }
}
