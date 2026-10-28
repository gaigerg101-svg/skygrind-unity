// NIGHT47 builder L (crownLook47, a VIEW row; NIGHT47.md G42 "the goal visible from the start and from each room"; the lead's round-4 item 3:
// "a view-only object drawn after the opaque roofs with a depth test against the sky"): THE CROWN'S LIGHT, the 300 m red-gold column over the
// spire's shaft (Game/Route47View.cs), drawn in the transparent queue after every roof wrote depth, in two passes:
//   UniversalForwardOnly  ZTest LEqual:  where nothing stands in front of it (the open sky, the v11 shaft), the column at full strength;
//   SRPDefaultUnlit       ZTest Greater: where a roof or a wall stands in front of it, the same column at _Through of the strength (the light
//                          seen through the roof: Enemy.shader's x-ray rule, ZTest Greater, applied to the level's goal instead of a body).
// Blend One One, no depth write, two-sided: it only adds light, never darkens. A soft core (pow(|N.V|, _Power): bright where the cylinder faces you,
// gone at its silhouette), brightest low, fading over its top _TopFade and its bottom _BottomFade (object space: Unity's cylinder is y -1..1).
// Colour per draw (_BaseColor through a MaterialPropertyBlock: the red-gold of crownLook47, x the pulse). No fog: the light is the far wienie.
// In Resources/ so Shader.Find finds it in a Player. Not compiled by its author (no Editor): Route47View falls back to no column when
// Shader.Find returns null or the shader is not supported.
Shader "SKYGRIND/Column47"
{
    Properties
    {
        [HDR] _BaseColor ("Colour (linear, set per draw)", Color) = (1, 0.35, 0.08, 1)
        _Through ("Share drawn behind a roof or wall (ZTest Greater pass)", Float) = 0.35
        _Power ("Core softness (pow of |N.V|)", Float) = 2.0
        _TopFade ("Fade over the top share of the height", Float) = 0.35
        _BottomFade ("Fade over the bottom share of the height", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+100" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Through;
        float _Power;
        float _TopFade;
        float _BottomFade;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS : TEXCOORD0;
            float3 viewWS : TEXCOORD1;
            float h : TEXCOORD2;
        };

        Varyings ColumnVertex(Attributes v)
        {
            Varyings o;
            float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
            o.positionCS = TransformWorldToHClip(posWS);
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.viewWS = GetCameraPositionWS() - posWS;
            o.h = saturate(v.positionOS.y * 0.5 + 0.5);
            return o;
        }

        float3 ColumnLight(Varyings i)
        {
            float ndv = abs(dot(normalize(i.normalWS), normalize(i.viewWS)));
            float core = pow(ndv, _Power);
            float top = saturate((1.0 - i.h) / max(1e-4, _TopFade));
            float bottom = saturate(i.h / max(1e-4, _BottomFade));
            float low = lerp(1.0, 0.45, i.h);
            return _BaseColor.rgb * (core * top * bottom * low);
        }
        ENDHLSL

        Pass
        {
            Name "UniversalForwardOnly"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ColumnVertex
            #pragma fragment frag
            half4 frag(Varyings i) : SV_Target
            {
                return half4(ColumnLight(i), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "SRPDefaultUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Blend One One
            ZWrite Off
            ZTest Greater
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ColumnVertex
            #pragma fragment frag
            half4 frag(Varyings i) : SV_Target
            {
                return half4(ColumnLight(i) * _Through, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
