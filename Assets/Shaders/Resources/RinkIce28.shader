// NIGHT28 (builder A2): THE GLACIER rink's ONE ice material (GG: "you can't even make the entire floor that skating texture"). Every floor-grade surface of the rink (lane, banks, lip, island top, door floor)
// uses this one material, so the skating texture is the same on every square metre. Opaque, unlit (a fake normal shade so the banks read), in WORLD space only (no UVs, no textures):
//   - static scratches: faint pale lines in two fixed directions, always on, so a standing player still sees skated ice;
//   - SKATE STREAKS over the WHOLE floor: thin dashed lines along the player's velocity that scroll with the player's speed (the same layered idea as SKYGRIND/SkateFloor27, but on the ice itself,
//     not a patch round the feet). SkateFx27 sets the globals _Rink28Dir (xz travel direction), _Rink28Scroll (metres travelled) and _Rink28Strength (0..1); strength 0 = no streaks.
// Pale violet-white (the boundary tone) for the lines, never red-orange. Cheap: a handful of ALU per pixel.
Shader "SKYGRIND/RinkIce28"
{
    Properties
    {
        _DeepColor ("Ice, shadow side", Color) = (0.10, 0.12, 0.25, 1)
        _BaseColor ("Ice, lit side", Color) = (0.32, 0.38, 0.62, 1)
        [HDR] _Sheen ("Line colour", Color) = (0.85, 0.82, 1, 1)
        _LinesPerM ("Streak lines per metre", Float) = 2.2
        _Width ("Streak line width (m)", Float) = 0.04
        _Gain ("Streak gain", Float) = 0.9
        _ScratchGain ("Static scratch gain", Float) = 0.18
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "RinkIce28"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _DeepColor;
            float4 _BaseColor;
            float4 _Sheen;
            float _LinesPerM, _Width, _Gain, _ScratchGain;
            CBUFFER_END
            // globals written by SkateFx27 each frame (outside the material's constant buffer)
            float4 _Rink28Dir;
            float _Rink28Scroll;
            float _Rink28Strength;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 worldPos : TEXCOORD0; float3 normalWS : TEXCOORD1; float tint : TEXCOORD2; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.worldPos = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.tint = v.color.r * 1.25; // the vertex colour tints the zones (bank lighter, island darker, door floor like the lane); 1.25 undoes the 0.8 of the lane's tint
                return o;
            }

            // One layer of dashed lines along `dir` (the coordinate across it picks the line); each line scrolls with its own phase: streaks, not a grid.
            float Layer(float2 xz, float2 dir, float2 side, float density, float scroll, float width)
            {
                float across = dot(xz, side) * density;
                float along = dot(xz, dir) * density * 0.5 - scroll * density * 0.5;
                float cell = frac(across);
                float cw = width * density;
                float ln = 1 - smoothstep(cw * 0.5, cw, abs(cell - 0.5) * 2.0 * 0.5);
                float id = floor(across);
                float dash = frac(along + frac(sin(id * 12.9898) * 43758.5453));
                float on = smoothstep(0.0, 0.08, dash) * (1 - smoothstep(0.35, 0.5, dash));
                return ln * on;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float shade = saturate(dot(n, normalize(float3(0.35, 0.8, 0.45))) * 0.5 + 0.5);
                float3 col = lerp(_DeepColor.rgb, _BaseColor.rgb, shade) * i.tint;
                float2 xz = i.worldPos.xz;
                // static scratches in two fixed directions
                float sc = Layer(xz, float2(0.866, 0.5), float2(-0.5, 0.866), 1.3, 0.0, 0.03) + Layer(xz, float2(-0.5, 0.866), float2(-0.866, -0.5), 0.9, 0.0, 0.03) * 0.7;
                col += _Sheen.rgb * sc * _ScratchGain;
                // the skate streaks, everywhere
                float2 dir = normalize(_Rink28Dir.xz + float2(1e-5, 0));
                float2 side = float2(-dir.y, dir.x);
                float a = Layer(xz, dir, side, _LinesPerM, _Rink28Scroll, _Width);
                float b = Layer(xz, dir, side, _LinesPerM * 0.6, _Rink28Scroll * 0.55, _Width * 1.4) * 0.5;
                col += _Sheen.rgb * (a + b) * _Rink28Strength * _Gain;
                return half4(col, 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex vertD
            #pragma fragment fragD
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct AttributesD { float4 positionOS : POSITION; };
            struct VaryingsD { float4 positionCS : SV_POSITION; };
            VaryingsD vertD(AttributesD v) { VaryingsD o; o.positionCS = TransformObjectToHClip(v.positionOS.xyz); return o; }
            half4 fragD(VaryingsD i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
