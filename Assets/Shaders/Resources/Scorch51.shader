// NIGHT51 scorch51 (EnemyViews.Scorch51.cs; UnityTuning.N51.cs): the mark a kill leaves on the floor. A horizontal quad (uv -1..1) drawn
// premultiplied: a dark burnt blotch in the enemy's colour (its edge wobbling by angle, per mark's _Seed) darkens the floor under it, and its
// core glows hot (HDR, the enemy's colour x _Heat) for the first second, then cools; _Fade takes the whole mark out at the end of its life.
// Fogged (MixFog) so far marks sink into the dusk with the floor. No texture.
Shader "SKYGRIND/Scorch51"
{
    Properties
    {
        _Tint ("Tint (the enemy's colour, linear)", Color) = (1, 0.42, 0.21, 1)
        _Heat ("Heat (0..1)", Float) = 0
        _Fade ("Fade (1 whole, 0 gone)", Float) = 1
        _Seed ("Seed", Float) = 0
        _Dark ("Dark (the blotch's colour share)", Float) = 0.12
        _Alpha ("Alpha (the blotch's cover)", Float) = 0.7
        _Glow ("Glow (the hot core's gain)", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-50" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Scorch51"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend One OneMinusSrcAlpha
            Offset -1, -4
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;
            float _Heat, _Fade, _Seed, _Dark, _Alpha, _Glow;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float fogFactor : TEXCOORD1; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.uv = v.uv;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float r = length(i.uv);
                float a = atan2(i.uv.y, i.uv.x);
                float edge = 0.72 + 0.14 * sin(a * 5 + _Seed) + 0.08 * sin(a * 11 + _Seed * 2.3) + 0.05 * sin(a * 23 + _Seed * 5.1);
                float rr = r / edge;
                float blot = 1 - smoothstep(0.75, 1.0, rr);
                // a few drops thrown past the edge (the splash), by angle
                float drops = step(0.93, frac(sin(floor(a * 7 + _Seed) * 91.7) * 4375.5)) * (1 - smoothstep(0.08, 0.13, abs(rr - 1.25)));
                float cover = saturate(blot + drops) * _Alpha * _Fade;
                float core = (1 - smoothstep(0.0, 0.55, rr)) * _Heat;
                float3 c = _Tint.rgb * _Dark * cover + _Tint.rgb * core * _Glow * _Fade;
                c = MixFog(c, i.fogFactor);
                return half4(c, cover);
            }
            ENDHLSL
        }
    }
}
