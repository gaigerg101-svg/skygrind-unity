// NIGHT59 builder G (G1, kill55's marks after the flash): the marks' own unlit shader (Kill55Damage.Ready, kill55 on and no -skygrindOldMarks59).
// The cause (Tools/night/n59-g-probe.py on n58-freezek's a258 frames): at the mark's drawn centre the frame is the SAME on and off (mean V
// 0.52 / 0.52 at the flash end, 0.64 / 0.64 at +0.5 s), so the hole is hidden there, not lifted: the decal sits on the pill's profile
// (Kill55Damage.ShellR + Kill55MarkLift) and the model's parts in front of the torso win the depth test; only the crack strips past them
// show (3.3 % of the rect, median V 0.28). So:
// - the depth pull: each vertex moves toward the camera ALONG ITS OWN VIEW RAY by _Pull59.x m (clamped to stay _Pull59.y m past the camera),
//   so the mark keeps its exact screen footprint and size but wins the depth test against the husk's own parts; anything nearer than the
//   pull (debris, another enemy, a wall) still covers it;
// - the facing cut: a mark whose shell normal turns away from the camera (dot under _Pull59.z) is not drawn (a mark on the far side never
//   shows through the body's edge);
// - no fog (MixFog is not called: the mark is drawn after the fog's reach; fog.near is 45 m, so this changes nothing at the a258 range) and
//   no flash, no rim, no ink, no x-ray: the colour is _BaseColor (linear), the decal's own near-black / ember.
// Drawn by URP's forward lists (LightMode UniversalForwardOnly), as Graphics.RenderMesh submits it. No shadow, no depth-only pass.
// NIGHT60 V (B2): the materials override the queue below to Transparent + UnityTuning.EnemyXrayQueueOffset + Kill60MarkQueueAfterXray
// (Kill55Damage.Params59): the enemy x-ray's ZTest Greater pass tinted every mark in front of the body (Tools/night/n60-v-mark.py).
Shader "SKYGRIND/Kill59Mark"
{
    Properties
    {
        [HDR] _BaseColor ("Color (linear)", Color) = (0.03, 0.01, 0.01, 1)
        _Pull59 ("x pull toward the camera (m), y nearest to the camera (m), z facing cut (dot)", Vector) = (0.3, 0.35, 0.05, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" "IgnoreProjector" = "True" }

        Pass
        {
            Name "UniversalForwardOnly"
            Tags { "LightMode" = "UniversalForwardOnly" }
            ZWrite On
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _Pull59;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float facing : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 p = TransformObjectToWorld(v.positionOS.xyz);
                float3 n = normalize(TransformObjectToWorldNormal(v.normalOS));
                float3 toCam = GetCameraPositionWS() - p;
                float dist = max(length(toCam), 1e-4);
                float3 dir = toCam / dist;
                o.facing = dot(n, dir);
                // along the vertex's own view ray: the same pixel, a nearer depth
                p += dir * min(_Pull59.x, max(dist - _Pull59.y, 0));
                o.positionCS = TransformWorldToHClip(p);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                clip(i.facing - _Pull59.z);
                return half4(_BaseColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}
