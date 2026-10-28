// NIGHT27 builder V (roof27): baked ambient occlusion as gradient vertex-colour strips laid along inside corners and edges (RoofView27). Unlit,
// alpha blended (black with the vertex alpha = 1 - AO factor), no depth write, pulled toward the camera a hair so it never z-fights the stone.
// No textures. Fogged like URP Unlit (MixFog is skipped: black multiplied by fog would lighten it, so the alpha is faded by the fog factor instead).
Shader "SKYGRIND/Ao27"
{
    Properties { }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-10" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Ao27"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 color : COLOR; float fogFactor : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.color = v.color;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half fade = saturate(i.fogFactor);
                return half4(0, 0, 0, i.color.a * fade);
            }
            ENDHLSL
        }
    }
}
