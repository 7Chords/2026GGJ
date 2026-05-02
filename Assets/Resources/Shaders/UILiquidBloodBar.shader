Shader "UI/LiquidBloodBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Fill ("Fill Amount", Range(0,1)) = 1
        [ToggleUI] _FillFromRight ("Fill From Right (Mirror X)", Float) = 0

        _WaveAmplitude ("Wave Amplitude", Range(0,0.2)) = 0.035
        _WaveFrequency ("Wave Frequency", Range(1,40)) = 12
        _WaveSpeed ("Wave Speed", Range(0,8)) = 2

        _EdgeSoftness ("Surface Edge Softness", Range(0.001,0.08)) = 0.018

        _BloodDeep ("Blood Deep", Color) = (0.18,0.02,0.04,1)
        _BloodBright ("Blood Surface", Color) = (0.95,0.12,0.1,1)

        _HighlightColor ("Surface Highlight", Color) = (1,0.55,0.45,1)
        _HighlightWidth ("Highlight Width", Range(0.002,0.06)) = 0.012

        _NoiseStrength ("Surface Noise", Range(0,0.05)) = 0.012

        _WaveEndAttenuation ("Wave Fade Near Empty Full", Range(0,1)) = 1

        _LiquidOpacity ("Body Opacity", Range(0.15,1)) = 0.58
        _DepthAbsorb ("Depth Darken (translucent body)", Range(0,0.45)) = 0.18
        _RimLight ("Back / edge light", Range(0,0.6)) = 0.12

        _BubbleDensity ("Bubble Density", Range(4, 42)) = 20
        _BubbleSize ("Bubble Max Size", Range(0.02,0.2)) = 0.08
        _BubbleSpeed ("Bubble Rise Speed", Range(0,4)) = 1.1
        _BubbleWobble ("Bubble Side Wobble", Range(0,0.05)) = 0.012
        _BubbleColor ("Bubble Tint", Color) = (0.95,0.75,0.8,1)
        _BubbleRim ("Bubble Edge Bright", Range(0,1.2)) = 0.55
        _BubbleCore ("Bubble Core Bright", Range(0,0.8)) = 0.22

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _TextureSampleAdd;
            fixed4 _Color;
            float4 _ClipRect;

            float _Fill;
            float _FillFromRight;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _EdgeSoftness;
            fixed4 _BloodDeep;
            fixed4 _BloodBright;
            fixed4 _HighlightColor;
            float _HighlightWidth;
            float _NoiseStrength;
            float _WaveEndAttenuation;

            float _LiquidOpacity;
            float _DepthAbsorb;
            float _RimLight;
            float _BubbleDensity;
            float _BubbleSize;
            float _BubbleSpeed;
            float _BubbleWobble;
            fixed4 _BubbleColor;
            float _BubbleRim;
            float _BubbleCore;

            float hash11(float p)
            {
                return frac(sin(p * 127.1) * 43758.5453);
            }

            float2 hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            float noise1(float x, float t)
            {
                float i = floor(x);
                float f = frac(x);
                float u = f * f * (3.0 - 2.0 * f);
                float a = hash11(i + t * 0.01);
                float b = hash11(i + 1.0 + t * 0.01);
                return lerp(a, b, u) * 2.0 - 1.0;
            }

            float bubbleLayer(float2 liqUv, float time, float density, float size, float wobbleAmt)
            {
                float2 uv = liqUv;
                float sp = _BubbleSpeed;
                uv.y += time * sp * 0.11;
                uv.x += sin(liqUv.y * 16.0 + time * 2.2) * wobbleAmt;
                uv.x += cos(liqUv.x * 9.0 - time * 1.1) * wobbleAmt * 0.5;

                float2 g = uv * density;
                float2 cell = floor(g);
                float2 f0 = frac(g) - 0.5;

                float acc = 0.0;
                float accRim = 0.0;
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        float2 b = float2((float)i, (float)j);
                        float2 cid = cell + b;
                        float2 h = hash22(cid);
                        float2 h2 = hash22(cid * 1.7 + 3.1);
                        float r = size * (0.45 + 0.7 * h.x);
                        float ph = h2.x * 6.28318;
                        float rise = time * sp * (0.18 + 0.22 * h2.y) + ph;
                        float2 offs = (h - 0.5) * 0.88;
                        offs.y += frac(rise * 0.35) - 0.5;
                        float2 c = b + offs - f0;
                        float d = length(c);
                        float fill = 1.0 - smoothstep(r * 0.75, r, d);
                        float edge = smoothstep(r * 0.55, r * 0.88, d) * (1.0 - smoothstep(r * 0.88, r * 1.05, d));
                        acc = max(acc, fill);
                        accRim = max(accRim, edge);
                    }
                }
                return acc * _BubbleCore + accRim * _BubbleRim;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float tx = uv.x;
                if (_FillFromRight > 0.5)
                    tx = 1.0 - tx;

                float ty = uv.y;
                float t = _Time.y;
                float wf = _WaveFrequency;

                float fRaw = saturate(_Fill);
                float fPlot = lerp(0.3, 0.7, fRaw);

                float endFade = lerp(1.0, sin(fRaw * 3.14159265), saturate(_WaveEndAttenuation));

                float n = noise1(ty * 14.0 + tx * 3.0, t * _WaveSpeed) * _NoiseStrength * endFade;

                float wobble = _WaveAmplitude * endFade * (
                    sin(ty * wf + t * _WaveSpeed)
                    + 0.48 * sin(ty * wf * 2.13 - t * _WaveSpeed * 0.92)
                    + 0.22 * sin(ty * wf * 4.9 + t * _WaveSpeed * 1.15)
                ) + n;

                float surfaceX = saturate(fPlot + wobble);

                float edge = max(_EdgeSoftness, 1e-4);
                float liquidMask = 1.0 - smoothstep(surfaceX - edge, surfaceX + edge, tx);

                float denom = max(surfaceX, 1e-4);
                float depthGrad = saturate(tx / denom);

                fixed3 blood = lerp(_BloodDeep.rgb, _BloodBright.rgb, depthGrad * 0.92 + 0.08);
                blood *= 1.0 - _DepthAbsorb * (1.0 - depthGrad);

                float distSurf = abs(tx - surfaceX);
                float hi = (1.0 - smoothstep(0.0, _HighlightWidth, distSurf))
                    * smoothstep(surfaceX - edge * 2.0, surfaceX + edge * 0.5, tx);
                blood += _HighlightColor.rgb * hi * liquidMask;

                float rim = (1.0 - depthGrad) * (1.0 - ty);
                rim *= liquidMask;
                blood += _BloodBright.rgb * _RimLight * rim;

                float2 liqUv = float2(tx, ty);
                float bubAtten = lerp(1.0, 0.35 + 0.65 * sin(fRaw * 3.14159265), saturate(_WaveEndAttenuation));
                float bub = bubbleLayer(liqUv, t, _BubbleDensity, _BubbleSize, _BubbleWobble) * bubAtten;
                bub *= liquidMask;
                float bubSmall = bubbleLayer(liqUv * 1.73 + float2(0.13, 0.07), t * 1.13 + 2.1, _BubbleDensity * 1.45, _BubbleSize * 0.62, _BubbleWobble * 0.8) * bubAtten;
                bubSmall *= liquidMask;
                float bubMix = saturate(bub + bubSmall * 0.65);

                fixed3 bubRgb = _BubbleColor.rgb;
                blood += bubRgb * bubMix;
                blood += (fixed3)1.0 * bubSmall * _BubbleCore * 0.35 * liquidMask;

                fixed4 tex = tex2D(_MainTex, uv) + _TextureSampleAdd;
                fixed3 col = blood * tex.rgb * IN.color.rgb;

                float alphaBody = tex.a * IN.color.a * liquidMask * _LiquidOpacity;
                float alphaBubble = bubMix * liquidMask * tex.a * IN.color.a * (0.35 + _BubbleRim * 0.25);
                fixed alpha = saturate(alphaBody + alphaBubble);

                fixed4 c = fixed4(col, alpha);

                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a - 0.001);
                #endif

                return c;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
