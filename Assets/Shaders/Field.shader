// The force fields (gates of locked zones and the boundary), after skygrind/src/fx/barrier.js (the parts ported
// tonight): an additive emissive sheet in the field's color (config.barrier.gateColor: red-orange, danger; the
// boundary is pale violet-white), diagonal hazard stripes (stripeWidth), stronger near the viewer
// (strongWithin / clearWithin, farAlpha) and fading out with height (fadeTop / fadeLength). Ripples on touch and the
// dissolve are later.
// NIGHT13 U3-26.11 S3 (the locked gate painted an orange striped wall 16 m ahead of spawn): the doorway look, on by
// default (_Doorway 1) so the gate material needs no new value. The stripes keep to the doorway, the walkable band under
// _DoorTop (world m; every ground-level gate stands on y 0), and come in as you walk up (full within _DoorNear, gone from
// _DoorFar); above the band and far away the sheet dims (_DoorFarDim); seen edge-on it fades and loses its stripes, as
// barrier.js's hex lines fade at grazing angles (onFace: smoothstep(0.06, 0.4, facing)). A plain sheet (_Stripe 100 m or
// more: TelegraphView's beams, RunShapeDesignViews' fields) keeps the old look whatever _Doorway says. The boundary shares
// this shader: _Doorway 0 on its material keeps it exactly as before. Unity-only numbers (barrier.js has no stripes).
Shader "SKYGRIND/Field"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 0.36, 0.15, 1)
        _FarAlpha ("Far alpha", Float) = 0.5
        _StrongWithin ("Strong within (m)", Float) = 3
        _ClearWithin ("Clear within (m)", Float) = 14
        _FadeTop ("Fade top (m)", Float) = 30
        _FadeLength ("Fade length (m)", Float) = 24
        _Stripe ("Stripe width (m)", Float) = 0.7
        _Doorway ("Doorway look (1 the gate's, 0 as before)", Float) = 1
        _DoorTop ("Doorway top (world y, m)", Float) = 3.5
        _DoorFeather ("Doorway top's soft edge (m)", Float) = 1
        _DoorNear ("Stripes full within (m)", Float) = 4
        _DoorFar ("Stripes gone from (m)", Float) = 12
        _DoorFarDim ("Sheet far away (x)", Float) = 0.45
        _EdgeOnDim ("Sheet edge-on (x)", Float) = 0.15
        _EdgeOnFrom ("Edge-on below facing", Float) = 0.06
        _EdgeOnTo ("Face-on above facing", Float) = 0.4
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        Pass
        {
            Name "Field"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _FarAlpha, _StrongWithin, _ClearWithin, _FadeTop, _FadeLength, _Stripe;
            float _Doorway, _DoorTop, _DoorFeather, _DoorNear, _DoorFar, _DoorFarDim, _EdgeOnDim, _EdgeOnFrom, _EdgeOnTo;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float fogFactor : TEXCOORD1; float3 normalWS : TEXCOORD2; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS, false);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 toEye = GetCameraPositionWS() - i.positionWS;
                float dist = length(toEye);
                float near = 1 - smoothstep(_StrongWithin, _ClearWithin, dist);
                float a = lerp(_FarAlpha * 0.3, 0.75, near);
                float s = (i.positionWS.x + i.positionWS.y + i.positionWS.z) / max(_Stripe * 2, 1e-3);
                float stripe = step(0.82, frac(s)); // thin hazard lines, not a filled sheet
                // NIGHT13 S3: the doorway look (a plain sheet, _Stripe >= 100 m, never takes it). A mesh without normals
                // reads as edge-on (no NaN).
                float doorway = saturate(_Doorway) * step(_Stripe, 100.0);
                float nLen = max(length(i.normalWS), 1e-4);
                float facing = abs(dot(toEye / max(dist, 1e-4), i.normalWS / nLen));
                float onFace = smoothstep(_EdgeOnFrom, _EdgeOnTo, facing);
                float door = 1 - smoothstep(_DoorTop - _DoorFeather, _DoorTop, i.positionWS.y);
                float approach = 1 - smoothstep(_DoorNear, _DoorFar, dist);
                stripe = lerp(stripe, stripe * door * approach * onFace, doorway);
                a *= lerp(1, lerp(_DoorFarDim, 1, approach) * lerp(_EdgeOnDim, 1, onFace), doorway);
                a *= 0.35 + 0.65 * stripe;
                a *= 1 - saturate((i.positionWS.y - (_FadeTop - _FadeLength)) / max(_FadeLength, 1e-3));
                float fog = 1;
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                fog = ComputeFogIntensity(i.fogFactor);
                #endif
                return half4(_Color.rgb, saturate(a * 0.6) * lerp(0.25, 1, fog));
            }
            ENDHLSL
        }
    }
}
