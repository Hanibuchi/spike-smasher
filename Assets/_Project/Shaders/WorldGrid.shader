Shader "Custom/WorldGrid"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.1, 0.12, 1)
        _GridColor1 ("Grid Color 1u", Color) = (0.3, 0.3, 0.35, 1)
        _GridColor10 ("Grid Color 10u", Color) = (0.5, 0.5, 0.6, 1)
        _GridColor100 ("Grid Color 100u", Color) = (1.0, 1.0, 1.0, 1)
        
        _Thickness1 ("Thickness 1u", Float) = 0.02
        _Thickness10 ("Thickness 10u", Float) = 0.08
        _Thickness100 ("Thickness 100u", Float) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _GridColor1;
                half4 _GridColor10;
                half4 _GridColor100;
                float _Thickness1;
                float _Thickness10;
                float _Thickness100;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            float GetGridPattern(float2 pos, float stepSize, float thickness)
            {
                // stepSizeの単位で繰り返す
                float2 modPos = fmod(abs(pos), stepSize);
                // 線の中心（0またはstepSize）からの距離
                float2 dist = min(modPos, stepSize - modPos);
                
                // antialiasingのためのfade幅
                float2 fw = fwidth(pos);
                float fade = length(fw);
                
                // 距離がthicknessの半分以下なら線を描画する。境界を少しぼかして滑らかにする
                float2 weight = 1.0 - smoothstep(thickness * 0.5 - fade, thickness * 0.5 + fade, dist);
                return max(weight.x, weight.y);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // ワールド空間のXとZ座標を使用
                float2 pos = IN.positionWS.xz;

                // 1u, 10u, 100u ごとのグリッド線の強さを計算
                float grid1 = GetGridPattern(pos, 1.0, _Thickness1);
                float grid10 = GetGridPattern(pos, 10.0, _Thickness10);
                float grid100 = GetGridPattern(pos, 100.0, _Thickness100);

                // ベースカラーにグリッドの色を順番に重ねていく
                half4 finalColor = _BaseColor;
                finalColor = lerp(finalColor, _GridColor1, grid1);
                finalColor = lerp(finalColor, _GridColor10, grid10);
                finalColor = lerp(finalColor, _GridColor100, grid100);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
