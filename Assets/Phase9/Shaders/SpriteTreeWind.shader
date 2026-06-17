Shader "Director/Phase9/Sprite Tree Wind"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.15
        _WindSpeed ("Wind Speed", Range(0, 10)) = 1.2
        _NoiseStrength ("Irregular Noise", Range(0, 1)) = 0.35
        _WaveFrequency ("Wave Frequency", Range(0, 30)) = 4
        _RootUVY ("Root UV Y", Range(0, 1)) = 0
        _TopUVY ("Top UV Y", Range(0, 1)) = 1
        _RootLock ("Root Lock", Range(0, 0.6)) = 0.35
        _PhaseOffset ("Phase Offset", Float) = 0
        _VerticalSway ("Vertical Sway", Range(0, 0.5)) = 0.08
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
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _WindStrength;
            float _WindSpeed;
            float _NoiseStrength;
            float _WaveFrequency;
            float _RootUVY;
            float _TopUVY;
            float _RootLock;
            float _PhaseOffset;
            float _VerticalSway;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;

                float heightRange = max(0.0001, _TopUVY - _RootUVY);
                float height = saturate((v.texcoord.y - _RootUVY) / heightRange);
                float rootMask = smoothstep(_RootLock, 1.0, height);
                float bendWeight = rootMask * height * height;

                float t = _Time.y * _WindSpeed + _PhaseOffset;

                // 基于 UV.x 的相位偏移 — 同一棵树不同水平位置异步摆动
                float xPhase = v.texcoord.x * 6.28;

                // 水平摆动 (x轴) — 主方向
                float gust = sin(t + v.texcoord.y * _WaveFrequency + xPhase);
                float leafFlutter = sin(t * 1.73 + v.texcoord.y * (_WaveFrequency * 1.9) + v.texcoord.x * 17.0) * 0.45;
                float irregular = (Hash21(floor(v.texcoord * 16.0) + floor(t * 2.0)) - 0.5) * 2.0;
                float wind = gust + leafFlutter + irregular * _NoiseStrength;

                // 水平偏移
                v.vertex.x += wind * _WindStrength * bendWeight;

                // 垂直抖动 (y轴) — 不同 UV.x 位置不同方向，产生"不同方向摆"的错觉
                // 使用 UV.x 作为相位，让树叶左右侧交替上下抖动
                float vertWave = sin(t * 1.27 + v.texcoord.x * 12.57 + v.texcoord.y * 3.14) * 0.5;
                v.vertex.y += vertWave * _VerticalSway * bendWeight;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;

                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                return c;
            }
            ENDCG
        }
    }
}
