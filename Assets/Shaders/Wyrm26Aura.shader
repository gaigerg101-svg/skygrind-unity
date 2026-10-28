// NIGHT26 builder V: the KILN WYRM's aura (Game/WyrmVisual26.cs): additive red-orange, three uses of one shader, instanced:
//   _Mode 0  the shell: the piece's own mesh pushed out along its normal by _Shell (m, world space), a fresnel rim: the glow round the worm;
//   _Mode 1  the halo: a sphere, brightest at its middle and fading to its edge (a soft light halo round the coil);
//   _Mode 2  flat: a solid colour (the embers: small cubes).
// Blend One One, no depth write: nothing here darkens the scene, it only adds light. Per instance (MaterialPropertyBlock): _Glow (the strength, 0 draws nothing)
// and _Hue (0 red-orange .. 1 amber). Red-orange only (CLAUDE.md rule 8). Not compiled by its author (no Editor).
Shader "SKYGRIND/Wyrm26Aura"
{
    Properties
    {
        [HDR] _GlowColor ("Glow (red-orange, linear)", Color) = (1, 0.07, 0.005, 1)
        [HDR] _AmberColor ("Glow at hue 1 (amber, linear)", Color) = (1, 0.32, 0.02, 1)
        _Mode ("0 shell, 1 halo, 2 flat", Float) = 0
        _Shell ("Shell offset (m)", Float) = 0.28
        _Power ("Fresnel / falloff power", Float) = 2.2
        _Gain ("HDR gain", Float) = 3.2
        _Glow ("Strength (set per draw)", Float) = 0
        _Hue ("Hue (set per draw)", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull (the halo is two-sided: the camera can stand inside the coil)", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "UniversalForwardOnly"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _GlowColor;
            float4 _AmberColor;
            float _Mode;
            float _Shell;
            float _Power;
            float _Gain;
            float _Glow;
            float _Hue;
            CBUFFER_END


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 nWS = normalize(TransformObjectToWorldNormal(v.normalOS));
                posWS += nWS * (_Mode < 0.5 ? _Shell : 0.0);
                o.positionCS = TransformWorldToHClip(posWS);
                o.normalWS = nWS;
                o.viewWS = GetCameraPositionWS() - posWS;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float glow = _Glow, hue = _Hue;
                float ndv = saturate(dot(normalize(i.normalWS), normalize(i.viewWS)));
                float shape = _Mode < 0.5 ? pow(1.0 - ndv, _Power) : (_Mode < 1.5 ? pow(abs(dot(normalize(i.normalWS), normalize(i.viewWS))), _Power) : 1.0);
                float3 c = lerp(_GlowColor.rgb, _AmberColor.rgb, saturate(hue)) * (glow * shape * _Gain);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
