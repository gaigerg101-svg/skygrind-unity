// NIGHT30 builder V (item 3; design.n30.lava): the LAVA SURFACE. GG: "replacing those gaps with actual lava"; DESIGN30 B: "a scrolling emissive red-orange
// with a gold rim on its edge"; E: "danger reads on the surface". Opaque and unlit (it is its own light; bloom carries the glow), world-space coordinates only
// (no texture: any rect, any size, no seams between rects): two value-noise layers scroll in different directions (the crust drifts), a third, slower layer
// opens hot gold seams; the colour runs from a dark crust red through the danger red-orange to gold; _Pulse (the C# view's 0.8 Hz breath, 1 with REDUCED
// MOTION) and _Gain scale the emission. The gold rim is the view's own geometry (LavaView30), not this shader. Red-orange is danger only (CLAUDE.md rule 8).
// Fog: the scene's (MixFog), so far lava sinks into the dusk like the rest. Cheap: three noise lookups per pixel.
Shader "SKYGRIND/Lava30"
{
    Properties
    {
        [HDR] _Hot ("Hot (danger red-orange)", Color) = (1, 0.35, 0.12, 1)
        [HDR] _Gold ("Seams (gold)", Color) = (1, 0.72, 0.28, 1)
        _Crust ("Crust (dark red)", Color) = (0.32, 0.05, 0.03, 1)
        _Gain ("Gain", Float) = 1.6
        _Pulse ("Pulse (0.8 Hz breath, set by the view)", Float) = 1
        _Scale ("Noise cells per metre", Float) = 0.55
        _Flow ("Flow speed (m/s)", Float) = 0.35
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "Lava30"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Hot, _Gold, _Crust;
            float _Gain, _Pulse, _Scale, _Flow;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 worldPos : TEXCOORD0; float fogFactor : TEXCOORD1; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.worldPos = p.positionWS;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash(i), b = Hash(i + float2(1, 0)), c = Hash(i + float2(0, 1)), d = Hash(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float t = _Time.y * _Flow;
                float2 p = i.worldPos.xz * _Scale;
                float a = Noise(p + float2(t, t * 0.6));
                float b = Noise(p * 2.1 + float2(-t * 1.3, t * 0.9));
                float heat = saturate(a * 0.65 + b * 0.35); // 0 crust .. 1 molten
                float seam = Noise(p * 0.7 + float2(t * 0.25, -t * 0.2));
                seam = smoothstep(0.62, 0.78, seam) * (1 - smoothstep(0.78, 0.9, seam)); // thin gold veins
                float3 c = lerp(_Crust.rgb, _Hot.rgb, smoothstep(0.25, 0.7, heat));
                c = lerp(c, _Gold.rgb, seam * 0.8);
                c *= _Gain * _Pulse;
                c = MixFog(c, i.fogFactor);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}
