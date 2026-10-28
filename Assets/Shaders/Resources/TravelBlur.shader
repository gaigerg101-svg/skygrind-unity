// NIGHT20 U3-34.7 step 3 (builder V): the travel blur's pass (Game/Render/TravelBlurFeature.cs).
// Ported from Bauxitedev/godot-motion-blur (MIT, Copyright (c) 2018 Bauxitedev), motion_blur/moblur_shader.shader: the
// pixel's position from the depth texture, moved by the eye's travel, projected back; the image averaged along that
// difference, `pixel_diff_ndc * (i / n - 0.5)`. Changes: the linear velocity only (no angular term: a turn never smears
// the aim), the pixel rebuilt in view space from its linear eye depth and the view's half-angle tangents (x right, y up,
// z ahead; uv.y is up on every platform for a URP render-graph blit), the middle kept sharp (smoothstep about the
// crosshair), the smear clamped to a share of the width, six taps. The blit plumbing (Blit.hlsl's Vert, _BlitTexture) as
// daniel-ilett/blur-pro-urp's RadialBlur.shader (MIT, Copyright (c) 2026 Daniel Ilett).
// In a Resources folder so Shader.Find has it in a Player without the Always Included list.
Shader "SKYGRIND/TravelBlur"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off
        Pass
        {
            Name "TravelBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _TravelDisp; // xyz the eye's travel over the shutter (view space, m), w the strength 0..1
            float4 _TravelParams; // x tanH, y tanV, z the sharp middle's rho, w the whole blur's rho
            float4 _TravelClamp; // x the longest smear (a share of the width), y the aspect (w / h)

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 uv = i.texcoord;
                float2 ndc = uv * 2.0 - 1.0;
                float mask = smoothstep(_TravelParams.z, _TravelParams.w, length(ndc));
                float z = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float3 p = float3(ndc.x * _TravelParams.x * z, ndc.y * _TravelParams.y * z, z);
                float3 q = p + _TravelDisp.xyz;
                float2 off = 0;
                if (q.z > 0.05)
                {
                    float2 nq = float2(q.x / (q.z * _TravelParams.x), q.y / (q.z * _TravelParams.y));
                    off = (nq - ndc) * 0.5 * _TravelDisp.w * mask; // uv units
                }
                float2 w = float2(off.x, off.y / _TravelClamp.y); // in widths
                float len = length(w);
                if (len > _TravelClamp.x) off *= _TravelClamp.x / len;
                half4 centre = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0);
                half3 col = 0;
                UNITY_UNROLL
                for (int k = 0; k < 6; k++)
                {
                    float2 o = off * (k / 5.0 - 0.5);
                    col += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + o, 0).rgb;
                }
                return half4(col / 6.0, centre.a);
            }
            ENDHLSL
        }
    }
}
