Shader "UI/RetroScanlines"
{
    Properties
    {
        // Required by Unity UI / CanvasRenderer.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _LineColor ("Line Color", Color) = (0, 0, 0, 1)
        _LineCount ("Line Count", Range(20, 500)) = 180
        _Thickness ("Line Thickness", Range(0.01, 0.95)) = 0.35
        _Opacity ("Opacity", Range(0, 1)) = 0.16
        _ScrollSpeed ("Scroll Speed", Range(-2, 2)) = 0
        _FlickerStrength ("Flicker Strength", Range(0, 0.2)) = 0.025
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;

            float4 _LineColor;
            float _LineCount;
            float _Thickness;
            float _Opacity;
            float _ScrollSpeed;
            float _FlickerStrength;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            fixed4 frag(Varyings input) : SV_Target
            {
                // Allows the UI Image's sprite/alpha to work normally.
                float textureAlpha = tex2D(_MainTex, input.uv).a;

                float linePosition = frac(
                    input.uv.y * _LineCount +
                    _Time.y * _ScrollSpeed
                );

                float scanline = smoothstep(
                    1.0 - _Thickness,
                    1.0,
                    linePosition
                );

                float flicker =
                    1.0 - _FlickerStrength +
                    (sin(_Time.y * 70.0) * 0.5 + 0.5)
                    * _FlickerStrength;

                float alpha =
                    scanline *
                    _Opacity *
                    _LineColor.a *
                    flicker *
                    input.color.a *
                    textureAlpha;

                return float4(_LineColor.rgb, alpha);
            }

            ENDCG
        }
    }
}