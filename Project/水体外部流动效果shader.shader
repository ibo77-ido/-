Shader "Director/Phase9/Sprite Ink Water Flow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Water Tint", Color) = (0.78, 0.96, 0.90, 0.78)
        _FoamColor ("Foam Tint", Color) = (1, 0.98, 0.88, 0.72)
        _FlowDirection ("Flow Direction XY", Vector) = (1, 0.28, 0, 0)
        _FlowSpeed ("Flow Speed", Range(0, 0.25)) = 0.055
        _RippleScale ("Ripple Scale", Range(1, 48)) = 18
        _RippleStrength ("Ripple Strength", Range(0, 0.12)) = 0.035
        _FoamIntensity ("Foam Intensity", Range(0, 1)) = 0.42
        _FoamThinness ("Foam Thinness", Range(0, 1)) = 0.62
        _InkWashStrength ("Ink Wash Strength", Range(0, 0.35)) = 0.12
        _AlphaSoftness ("Alpha Softness", Range(0, 1)) = 0.92
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
            fixed4 _FoamColor;
            float4 _FlowDirection;
            float _FlowSpeed;
            float _RippleScale;
            float _RippleStrength;
            float _FoamIntensity;
            float _FoamThinness;
            float _InkWashStrength;
            float _AlphaSoftness;
            float _PhaseOffset;

            float Hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
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

            float FlowLine(float2 uv, float t)
            {
                float n1 = SmoothNoise(uv * _RippleScale + float2(t, t * 0.31));
                float n2 = SmoothNoise(uv * (_RippleScale * 0.53) - float2(t * 0.37, t * 0.21));
                float wave = sin((uv.x + n1 * 0.28 + n2 * 0.16) * 42.0 + t * 9.0);
                float foamLine = smoothstep(0.92 - _FoamThinness * 0.18, 0.995, wave * 0.5 + 0.5);
                return foamLine * smoothstep(0.18, 0.82, n1);
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
                float2 flowDir = normalize(_FlowDirection.xy + float2(0.0001, 0.0001));
                float t = _Time.y * _FlowSpeed + _PhaseOffset;
                float2 flowUV = i.uv + flowDir * t;

                float nA = SmoothNoise(flowUV * _RippleScale);
                float nB = SmoothNoise(flowUV * (_RippleScale * 0.47) + 17.3);
                float2 distortion = (float2(nA, nB) - 0.5) * _RippleStrength;

                fixed4 baseCol = tex2D(_MainTex, i.texcoord + distortion) * i.color;

                float wash = (nA - 0.5) * _InkWashStrength;
                baseCol.rgb += wash;

                float foam = FlowLine(flowUV, t);
                float edgeFromAlpha = 1.0 - smoothstep(0.08, 0.42, baseCol.a);
                float edgeFoam = foam * lerp(0.45, 1.0, edgeFromAlpha) * _FoamIntensity;

                baseCol.rgb = lerp(baseCol.rgb, _FoamColor.rgb, edgeFoam * _FoamColor.a);
                baseCol.a *= _AlphaSoftness;
                baseCol.a = saturate(baseCol.a + edgeFoam * 0.18);

                return baseCol;
            }
            ENDCG
        }
    }
}
