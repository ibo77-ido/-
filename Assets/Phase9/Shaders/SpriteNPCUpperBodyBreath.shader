Shader "Director/Phase9/Sprite NPC Upper Body Breath"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BreathAmount ("Breath Amount", Range(0, 0.08)) = 0.02
        _BreathValue ("Breath Value", Range(-1, 1)) = 0
        _FootLock ("Foot Lock", Range(0, 0.6)) = 0.22
        _UpperBodyStart ("Upper Body Start", Range(0, 1)) = 0.38
        _ChestCenterX ("Chest Center X", Range(0, 1)) = 0.5
        _VerticalStretch ("Vertical Stretch", Range(0, 1)) = 0.6
        _HorizontalStretch ("Horizontal Stretch", Range(0, 1)) = 1
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
            float _BreathAmount;
            float _BreathValue;
            float _FootLock;
            float _UpperBodyStart;
            float _ChestCenterX;
            float _VerticalStretch;
            float _HorizontalStretch;

            v2f vert(appdata_t v)
            {
                v2f o;
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
                float upperRange = max(0.0001, 1.0 - _UpperBodyStart);
                float upperHeight = saturate((i.texcoord.y - _UpperBodyStart) / upperRange);
                float footMask = smoothstep(_FootLock, _UpperBodyStart, i.texcoord.y);
                float breathWeight = footMask * upperHeight * upperHeight;
                float breath = _BreathValue * _BreathAmount;

                float horizontalScale = 1.0 + breath * _HorizontalStretch * breathWeight;
                float verticalScale = 1.0 + breath * _VerticalStretch * breathWeight;

                float2 sampleUV = i.texcoord;
                sampleUV.x = _ChestCenterX + (sampleUV.x - _ChestCenterX) / max(0.0001, horizontalScale);
                sampleUV.y = _FootLock + (sampleUV.y - _FootLock) / max(0.0001, verticalScale);

                sampleUV = saturate(sampleUV);
                fixed4 c = tex2D(_MainTex, sampleUV) * i.color;
                return c;
            }
            ENDCG
        }
    }
}
