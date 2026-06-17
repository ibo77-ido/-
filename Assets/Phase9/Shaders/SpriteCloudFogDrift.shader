Shader "Director/Phase9/Sprite Cloud Fog Drift"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _DriftStrength ("Drift Strength", Range(0, 1)) = 0.18
        _DriftSpeed ("Drift Speed", Range(0, 5)) = 0.45
        _VerticalFloat ("Vertical Float", Range(0, 0.5)) = 0.04
        _DistortionStrength ("Soft Distortion", Range(0, 0.25)) = 0.045
        _AlphaPulse ("Alpha Pulse", Range(0, 0.5)) = 0.12
        _NoiseScale ("Noise Scale", Range(1, 30)) = 9
        _PhaseOffset ("Phase Offset", Float) = 0
        _LayerOffset ("Layer Offset", Float) = 0
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
                float pulse : TEXCOORD2;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _DriftStrength;
            float _DriftSpeed;
            float _VerticalFloat;
            float _DistortionStrength;
            float _AlphaPulse;
            float _NoiseScale;
            float _PhaseOffset;
            float _LayerOffset;

            float Hash21(float2 p)
            {
                p = frac(p * float2(234.34, 435.35));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            float SmoothNoise(float2 uv)
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

                float t = _Time.y * _DriftSpeed + _PhaseOffset;
                float softWave = sin(t + v.texcoord.y * 3.7 + _LayerOffset);
                float slowGust = sin(t * 0.47 + v.texcoord.x * 5.1 + _LayerOffset);

                float2 localDrift;
                localDrift.x = (softWave * 0.65 + slowGust * 0.35) * _DriftStrength;
                localDrift.y = sin(t * 0.83 + v.texcoord.x * 4.0 + _LayerOffset) * _VerticalFloat;

                v.vertex.xy += localDrift;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.uv = v.texcoord;
                o.texcoord = v.texcoord;
                o.pulse = sin(t * 1.2 + _LayerOffset) * 0.5 + 0.5;

                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y * _DriftSpeed + _PhaseOffset;
                float2 noiseUV = i.uv * _NoiseScale + float2(t * 0.18 + _LayerOffset, t * 0.07);
                float noiseA = SmoothNoise(noiseUV);
                float noiseB = SmoothNoise(noiseUV + 13.17);
                float2 distortion = (float2(noiseA, noiseB) - 0.5) * _DistortionStrength;

                fixed4 c = tex2D(_MainTex, i.texcoord + distortion) * i.color;
                float alphaBreath = 1.0 - _AlphaPulse + _AlphaPulse * i.pulse;
                float edgeMist = smoothstep(0.02, 0.2, c.a);
                c.a *= lerp(alphaBreath * 0.85, alphaBreath, edgeMist);
                return c;
            }
            ENDCG
        }
    }
}
