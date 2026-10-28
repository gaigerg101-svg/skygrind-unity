// The spitter's acid puddles (spitter.js puddleMesh: a MeshBasicMaterial in spitter.acidColor x 1.3, transparent,
// opacity 0.55, no depth write): an unlit alpha-blended colour. The colour and its alpha come from _BaseColor
// (set per puddle through a MaterialPropertyBlock, like the neon material's).
Shader "SKYGRIND/Acid"
{
    Properties
    {
        [HDR] _BaseColor ("Color", Color) = (1, 0.69, 0.13, 0.55)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            Name "Acid"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float fogFactor : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 c = _BaseColor;
                c.rgb = MixFog(c.rgb, i.fogFactor);
                return c;
            }
            ENDHLSL
        }
    }
}
