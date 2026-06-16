Shader "Director/Phase9/Sprite Bamboo Wind"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.12
        _WindSpeed ("Wind Speed", Range(0, 10)) = 1.8
        _NoiseStrength ("Irregular Noise", Range(0, 1)) = 0.35
        _WaveFrequency ("Wave Frequency", Range(0, 30)) = 8
        _RootUVY ("Root UV Y", Range(0, 1)) = 0
        _TopUVY ("Top UV Y", Range(0, 1)) = 1
        _RootLock ("Root Lock", Range(0, 0.6)) = 0.08
        _PhaseOffset ("Phase Offset", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
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
                float gust = sin(t + v.texcoord.y * _WaveFrequency + v.texcoord.x * 5.7);
                float leafFlutter = sin(t * 1.73 + v.texcoord.y * (_WaveFrequency * 1.9) + v.texcoord.x * 17.0) * 0.45;
                float irregular = (Hash21(floor(v.texcoord * 16.0) + floor(t * 2.0)) - 0.5) * 2.0;

                float wind = gust + leafFlutter + irregular * _NoiseStrength;
                v.vertex.x += wind * _WindStrength * bendWeight;

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
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
