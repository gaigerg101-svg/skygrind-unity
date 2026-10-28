// NIGHT27 builder V (skate27): the FLOOR STREAK on ice. A flat quad under the player (SkateFx27 lays it, in world space, over the ice rect around
// the feet) draws thin lines along the velocity direction: world-space coordinates only, no textures. Across the travel direction the lines repeat
// _LinesPerM per metre, _Width of the cell wide; they scroll along travel at speed x _ScrollK so the ice slides under the feet; two parallax layers
// (the second at a different density, speed and a dimmer gain) give depth. _Strength (0 at 30 % of top speed to 1 at 100 %) scales the whole thing.
// Additive, no depth write, unlit: pale violet-white (the boundary tone), never red-orange. Cheap: a handful of ALU per pixel.
Shader "SKYGRIND/SkateFloor27"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.85, 0.82, 1, 1)
        _Dir ("Travel direction (xz)", Vector) = (0, 1, 0, 0)
        _Scroll ("Scroll (metres travelled along dir)", Float) = 0
        _LinesPerM ("Lines per metre", Float) = 2.5
        _Width ("Line width (m)", Float) = 0.04
        _Strength ("Strength", Float) = 0
        _Gain ("Gain", Float) = 0.9
        _Center ("Fade centre (xz)", Vector) = (0, 0, 0, 0)
        _Radius ("Fade radius (m)", Float) = 9
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Skate27"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _Dir;
            float _Scroll, _LinesPerM, _Width, _Strength, _Gain, _Radius;
            float4 _Center;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 worldPos : TEXCOORD0; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.worldPos = p.positionWS;
                return o;
            }

            // One layer: lines along the travel direction (so the coordinate across it picks the line), dashes along it that scroll.
            float Layer(float2 xz, float2 dir, float2 side, float density, float scroll, float width)
            {
                float across = dot(xz, side) * density;
                float along = dot(xz, dir) * density * 0.5 - scroll * density * 0.5;
                float cell = frac(across);
                float cw = width * density; // line width as a share of the cell
                float ln = 1 - smoothstep(cw * 0.5, cw, abs(cell - 0.5) * 2.0 * 0.5);
                float id = floor(across);
                float dash = frac(along + frac(sin(id * 12.9898) * 43758.5453)); // each line scrolls with its own phase: streaks, not a grid
                float on = smoothstep(0.0, 0.08, dash) * (1 - smoothstep(0.35, 0.5, dash));
                return ln * on;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 dir = normalize(_Dir.xz + float2(1e-5, 0));
                float2 side = float2(-dir.y, dir.x);
                float a = Layer(i.worldPos.xz, dir, side, _LinesPerM, _Scroll, _Width);
                float b = Layer(i.worldPos.xz, dir, side, _LinesPerM * 0.6, _Scroll * 0.55, _Width * 1.4) * 0.5; // the far layer: sparser, slower, dimmer
                float fade = 1 - smoothstep(_Radius * 0.35, _Radius, distance(i.worldPos.xz, _Center.xz));
                float v = (a + b) * fade * _Strength * _Gain;
                return half4(_Color.rgb * v, 1);
            }
            ENDHLSL
        }
    }
}
