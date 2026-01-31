Shader "Unlit/CustomColorTint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            sampler2D _MainTex;
            float4 _OverlayColor;
            float _Intensity;
            float _VignetteWidth;
            float _ScanlinePower;
            float _CycleIndex;
            float _TransparencyFactor;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * 2.0f - 1.0f;
                float2 offset = uv.yx / _Intensity;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5f + 0.5f;

                float4 col = tex2D(_MainTex, uv);
                if (uv.x <= 0.0f || 1.0f <= uv.x || uv.y <= 0.0f || 1.0f <= uv.y)
                    col = 0;

                uv = uv * 2.0f - 1.0f;
                float2 vignette = _VignetteWidth / _ScreenParams.xy;
                vignette = smoothstep(0.0f, vignette, 1.0f - abs(uv));
                vignette = saturate(vignette);

                col.g *= (sin(i.uv.y * _ScreenParams.y * _CycleIndex*1.2) + 1.0f) *_ScanlinePower + 1.0f;
                col.rb *= (cos(i.uv.y * _ScreenParams.y * _CycleIndex*1.2)+ 1.0f) *_ScanlinePower + 1.0f;
                
                // float mask=step(0,sin(i.uv.y  * _ScreenParams.y*_CycleIndex*1.2));
                // col *= (mask+_TransparencyFactor);

                return  saturate(col)* vignette.x * vignette.y;
            }
            ENDHLSL
        }
    }
}
