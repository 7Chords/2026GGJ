Shader "UI/TVSwitchTransition"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ScreenTex ("Screen", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _CloseAmount ("Close Amount", Range(0,1)) = 0
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

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
            sampler2D _ScreenTex;
            float4 _ScreenTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _CloseAmount;

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
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

            fixed4 frag(v2f i) : SV_Target
            {
                float t = saturate(_CloseAmount);
                if (t < 0.0001)
                    return fixed4(0, 0, 0, 0);

                float time = _Time.y * 2.1;
                float2 uv0 = i.texcoord;
                float vy = uv0.y;
                float vx = uv0.x;

                float2 uv = uv0;
                #if UNITY_UV_STARTS_AT_TOP
                if (_ScreenTex_TexelSize.y < 0)
                    uv.y = 1.0 - uv.y;
                #endif

                float tt = t;

                // Mild vertical roll (reference is more horizontal-glitch than heavy roll)
                uv.y = frac(uv.y + time * 0.38 * tt);

                // --- Large horizontal block displacement (chunky glitch, jagged vertical edges) ---
                float blockY = floor(vy * 88.0);
                float blockTick = floor(time * 38.0);
                float blockOff = (hash12(float2(blockY, blockTick)) - 0.5) * 0.14 * tt;
                blockOff += (hash12(float2(blockY * 1.7 + 3.0, blockTick + 9.0)) - 0.5) * 0.07 * tt;
                float bigGlitch = step(0.82, hash12(float2(blockY, blockTick * 0.31)));
                blockOff += (hash12(float2(blockY + 22.0, blockTick)) - 0.5) * 0.22 * tt * bigGlitch;
                uv.x += blockOff;

                // Finer per-line jitter
                float lineId = floor(vy * 820.0);
                float hJit = (hash12(float2(lineId, floor(time * 95.0)))) - 0.5;
                uv.x += hJit * 0.042 * tt;

                // Horizontal wave (signal wobble)
                uv.x += sin(vy * 105.0 + time * 24.0) * 0.022 * tt;
                uv.x += sin(vy * 240.0 - time * 36.0) * 0.01 * tt;

                uv = saturate(uv);

                // Subtle RGB split (fringe on contrast)
                float mid = 4.0 * tt * (1.0 - tt);
                float2 ca = float2(0.0028 + 0.014 * mid, 0.0012 + 0.005 * mid) * tt;
                float r = tex2D(_ScreenTex, uv + float2(ca.x, ca.y)).r;
                float g = tex2D(_ScreenTex, uv).g;
                float b = tex2D(_ScreenTex, uv - float2(ca.x, ca.y)).b;

                float3 col = float3(r, g, b);

                // Prominent scanlines — scroll upward (bottom -> top)
                float scanScroll = time * 0.2 * tt;
                float scan = sin((vy - scanScroll) * 960.0 * 3.14159265) * 0.5 + 0.5;
                col *= 0.48 + 0.52 * scan;

                // Rolling dark sync band — sweeps bottom -> top (rollY increases with time; vy=0 bottom, vy=1 top)
                float rollSpeed = (0.32 + 0.06 * sin(time * 1.8)) * max(tt, 0.15);
                float rollY = frac(time * rollSpeed);
                float db = abs(vy - rollY);
                db = min(db, 1.0 - db);
                float bandDark = smoothstep(0.058, 0.0, db);
                col *= lerp(1.0, 0.26, bandDark * tt);

                // Secondary thinner band, same upward sweep
                float roll2 = frac(time * 0.21 * tt + 0.37);
                float db2 = abs(vy - roll2);
                db2 = min(db2, 1.0 - db2);
                float band2 = smoothstep(0.022, 0.0, db2);
                col *= lerp(1.0, 0.45, band2 * tt * 0.65);

                // Grain / static (zero-mean)
                float snA = hash12(uv0 * float2(1301.0, 801.0) + time * 220.0);
                float snB = hash12(uv0 * float2(601.0, 1001.0) + time * 310.0);
                float snC = hash12(uv0 * float2(901.0, 501.0) - time * 160.0);
                float snowAmt = mid * 0.95 + tt * 0.38;
                float3 sn = float3(snA, snB, snC) - 0.5;
                col += sn * snowAmt * 0.48;
                col = saturate(col);

                // Cyan / teal grade (reference palette)
                col *= lerp(float3(1, 1, 1), float3(0.72, 0.88, 0.98), tt * 0.92);

                // Darkening flicker only (no white lift)
                col *= 1.0 - 0.1 * (0.5 + 0.5 * sin(time * 58.0 + vx * 32.0)) * tt;

                float overlayA = saturate(t * 0.46);

                fixed4 c = fixed4(col, overlayA);
                c *= i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a - 0.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
