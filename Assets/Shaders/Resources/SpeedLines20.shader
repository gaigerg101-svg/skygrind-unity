// NIGHT20 U3-34.7 (builder V): the AIR streaks (Game/SpeedLines20.cs). One dynamic mesh of camera-facing tapered quads
// in world space (drawn with the identity matrix): uv.x across the quad (-1..1), uv.y along it (0 the head, 1 the tail);
// the vertex colour's a is the streak's alpha, its r the burst's share (the burst's colour).
// - KvantStream's line (keijiro/KvantStream, MIT, Copyright (C) 2015 Keijiro Takahashi; Line.shader `color.a *= sw`):
//   whole at the head, 0 at the tail, so the line reads as travel, not as a stick.
// - The central clear zone per pixel: rho, the elliptical radius about the crosshair (the screen's middle; 0 there, 1 at
//   the edge midpoints) from the pixel's own position: 0 inside _ClearIn, a smoothstep to whole by _ClearFull, and never
//   over _CapAlpha inside _ClearFull (the cap rising to 1 by _CapFull). rho is symmetric, so a flipped y doesn't matter.
// - A soft edge across (_EdgeSharp: the quad's half width over the feather, both in px).
// Additive (three's AdditiveBlending, premultiplied here), depth-tested (an enemy or a wall in front hides it), no depth
// write, both faces. In a Resources folder so Shader.Find has it in a Player without the Always Included list.
Shader "SKYGRIND/SpeedLines20"
{
    Properties
    {
        _FlowColor ("Flow colour", Color) = (0.6, 0.95, 1, 1)
        _BurstColor ("Burst colour", Color) = (0.4, 0.9, 1, 1)
        _ClearIn ("Clear inside (rho)", Float) = 0.33
        _ClearFull ("Whole from (rho)", Float) = 0.55
        _CapAlpha ("Alpha cap inside the whole radius", Float) = 0.29
        _CapFull ("Cap whole at (rho)", Float) = 0.7
        _EdgeSharp ("Edge sharpness", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "SpeedLines20"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _FlowColor;
            float4 _BurstColor;
            float _ClearIn;
            float _ClearFull;
            float _CapAlpha;
            float _CapFull;
            float _EdgeSharp;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 ndc = i.positionCS.xy / _ScreenParams.xy * 2.0 - 1.0;
                float rho = length(ndc);
                float clearShare = rho <= _ClearIn ? 0.0 : smoothstep(_ClearIn, _ClearFull, rho);
                float cap = lerp(_CapAlpha, 1.0, smoothstep(_ClearFull, _CapFull, rho));
                float across = saturate((1.0 - abs(i.uv.x)) * _EdgeSharp);
                float along = saturate(1.0 - i.uv.y); // KvantStream: whole at the head, 0 at the tail
                float a = min(i.color.a * along * across * clearShare, cap);
                float3 c = lerp(_FlowColor.rgb, _BurstColor.rgb, i.color.r);
                return half4(c * a, a);
            }
            ENDHLSL
        }
    }
}
