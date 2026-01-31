Shader "Unlit/CRTFilter"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    // 新增：暴露曲率参数，默认值2.0（值越小曲率越明显）
    _Curvature("CRT Curvature", Float) = 2.0
        // 新增：暴露暗角宽度参数，默认值0.5（值越大暗角范围越宽）
        _VignetteWidth("Vignette Width", Float) = 0.5
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // 声明属性变量（必须和Properties中的名称一致）
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Curvature;    // 曲率变量
            float _VignetteWidth; // 暗角宽度变量

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
               float2 uv = i.uv * 2.0f - 1.0f;
                float2 offset = uv.yx / _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5f + 0.5f;

                fixed4 col = tex2D(_MainTex, uv);
                if (uv.x <= 0.0f || 1.0f <= uv.x || uv.y <= 0.0f || 1.0f <= uv.y)
                    col = 0;

                uv = uv * 2.0f - 1.0f;
                float2 vignette = _VignetteWidth / _ScreenParams.xy;
                vignette = smoothstep(0.0f, vignette, 1.0f - abs(uv));
                vignette = saturate(vignette);

                col.g *= (sin(i.uv.y * _ScreenParams.y * 2.0f) + 1.0f) * 0.15f + 1.0f;
                col.rb *= (cos(i.uv.y * _ScreenParams.y * 2.0f) + 1.0f) * 0.135f + 1.0f;

                return saturate(col) * vignette.x * vignette.y;
            }
            ENDCG
        }
    }
}