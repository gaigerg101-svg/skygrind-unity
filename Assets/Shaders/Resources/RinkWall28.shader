// NIGHT28 (builder A2): the rink's WALL material (the rim wall, the island's sides, the door walls). A flat vertex colour read as a flat band in the first Player frames, so this draws the wall as a wall:
// the stone tint (vertex colour) shaded by the normal, with a faint world-space GRID (horizontal rings every _Step m of height, vertical seams every _Step m of x and of z) in the boundary tone,
// like the other levels' stone. Opaque, unlit, no textures, no UVs.
Shader "SKYGRIND/RinkWall28"
{
    Properties
    {
        _GridColor ("Grid colour", Color) = (0.85, 0.82, 1, 1)
        _GridGain ("Grid gain", Float) = 0.28
        _Step ("Grid step (m)", Float) = 1
        _LineWidth ("Grid line width (m)", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "RinkWall28"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _GridColor;
            float _GridGain, _Step, _LineWidth;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 worldPos : TEXCOORD0; float3 normalWS : TEXCOORD1; float4 color : COLOR; };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.worldPos = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.color = v.color;
                return o;
            }

            float Line(float v, float width)
            {
                float f = abs(frac(v / _Step + 0.5) - 0.5) * _Step; // metres from the nearest multiple of _Step
                return 1 - smoothstep(width * 0.5, width, f);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float shade = lerp(0.55, 1.0, saturate(dot(n, normalize(float3(0.35, 0.5, 0.45))) * 0.5 + 0.5));
                float3 col = i.color.rgb * shade;
                float g = max(Line(i.worldPos.y, _LineWidth), max(Line(i.worldPos.x, _LineWidth), Line(i.worldPos.z, _LineWidth)));
                col += _GridColor.rgb * g * _GridGain;
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
