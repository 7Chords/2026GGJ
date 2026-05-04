Shader "UI/DefeatFace"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Defeat)]
        _Progress ("Defeat Progress", Range(0,1)) = 0
        _DeathTone ("Death Tone (multiply grade)", Color) = (0.55, 0.62, 0.78, 1)
        _ToneBlend ("Tone Blend At Full Progress", Range(0,1)) = 0.55
        _DesatAtFull ("Desaturate At Full Progress", Range(0,1)) = 0.72
        _VignettePower ("Vignette Power", Range(0,3)) = 1.25
        _VignetteRoundness ("Vignette Roundness", Range(0.2,4)) = 1.6
        _Chromatic ("Chromatic Aberration", Range(0,0.025)) = 0.008
        _ScanIntensity ("Scanline Intensity", Range(0,0.35)) = 0.12
        _NoiseIntensity ("Static Noise", Range(0,0.25)) = 0.08
        _PulseDarken ("Signal Pulse Darken", Range(0,0.35)) = 0.12

        [Header(Dissolve to invisible)]
        _DissolveFromProgress ("Dissolve Begins At Progress", Range(0,0.95)) = 0.38
        _DissolveSoft ("Dissolve Edge Softness", Range(0.02,0.28)) = 0.11
        _DissolveNoiseScale ("Dissolve Pattern Scale", Range(8,220)) = 72
        _FinalFadeFrom ("Final Alpha Fade From Progress", Range(0.5,1)) = 0.88

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

            float _Progress;
            fixed4 _DeathTone;
            float _ToneBlend;
            float _DesatAtFull;
            float _VignettePower;
            float _VignetteRoundness;
            float _Chromatic;
            float _ScanIntensity;
            float _NoiseIntensity;
            float _PulseDarken;
            float _DissolveFromProgress;
            float _DissolveSoft;
            float _DissolveNoiseScale;
            float _FinalFadeFrom;

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
                float p = saturate(_Progress);
                float time = _Time.y;

                float2 uv = i.texcoord;
                float2 cuv = uv - 0.5;
                float r = length(pow(abs(cuv), _VignetteRoundness.xx));
                float vig = smoothstep(0.35, 1.15, r);
                float vigAmt = vig * _VignettePower * p;

                float chrom = _Chromatic * p;
                float2 offR = float2(chrom, -chrom * 0.35);
                float2 offB = float2(-chrom, chrom * 0.35);

                fixed4 sR = tex2D(_MainTex, uv + offR) + _TextureSampleAdd;
                fixed4 sG = tex2D(_MainTex, uv) + _TextureSampleAdd;
                fixed4 sB = tex2D(_MainTex, uv + offB) + _TextureSampleAdd;
                float3 col = float3(sR.r, sG.g, sB.b);
                float a = (sR.a + sG.a + sB.a) / 3.0;

                float lum = dot(col, float3(0.299, 0.587, 0.114));
                float desat = _DesatAtFull * p;
                col = lerp(col, lum.xxx, desat);

                float toneMix = _ToneBlend * p;
                col *= lerp(float3(1, 1, 1), _DeathTone.rgb, toneMix);

                float scan = sin((uv.y - time * 0.11 * p) * 900.0 * 3.14159265) * 0.5 + 0.5;
                col *= 1.0 - _ScanIntensity * p * (1.0 - scan);

                float n1 = hash12(uv * float2(1200.0, 800.0) + time * 180.0);
                float n2 = hash12(uv.yx * float2(900.0, 600.0) - time * 220.0);
                col += (n1 + n2 - 1.0) * _NoiseIntensity * p;

                float roll = frac(time * (0.18 + 0.06 * sin(time * 1.4)) * (0.25 + p));
                float band = abs(uv.y - roll);
                band = min(band, 1.0 - band);
                float bandDark = smoothstep(0.05, 0.0, band);
                col *= 1.0 - bandDark * _PulseDarken * p;

                col *= 1.0 - vigAmt;

                float d0 = saturate(_DissolveFromProgress);
                float dissolvePhase = saturate((p - d0) / max(1e-4, 1.0 - d0));
                float2 dUv = uv * _DissolveNoiseScale + float2(time * 1.6, -time * 1.1) * p;
                float dn = hash12(dUv);
                float dn2 = hash12(dUv.yx * 1.73 + float2(31.0, 9.0));
                float nm = dn * 0.62 + dn2 * 0.38;
                float soft = max(0.02, _DissolveSoft);
                float dissolveMask = smoothstep(dissolvePhase, dissolvePhase + soft, nm);

                float ff0 = saturate(_FinalFadeFrom);
                float finalFade = 1.0 - smoothstep(ff0, 1.0, p);

                fixed4 c = fixed4(saturate(col), a);
                c.a *= dissolveMask * finalFade;
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
