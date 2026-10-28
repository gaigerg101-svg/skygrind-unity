// NIGHT26 builder V: the KILN WYRM's armour (Game/WyrmVisual26.cs draws the three Tripo pieces with it, instanced: one draw call per piece kind).
// Not compiled by its author (no Editor): written to URP 17's include paths as Enemy.shader is.
// - The body is the Tripo albedo, lit softly (the dusk's main light as a half-lambert plus the ambient SH): the armour reads dark.
// - The emission is read from the albedo itself: a texel's redness (r - 1.6 g) between _MaskLo and _MaskHi is the seams, eyes and orbs
//   (the Tripo textures paint them saturated red; the yellow patches and the grey armour fall under the mask). It is HDR, so URP bloom picks it up.
// - Per instance (MaterialPropertyBlock arrays, UNITY_INSTANCING_BUFFER): _Glow (0 off .. 1 full, over 1 for a flash), _Jaw (extra glow on the head's
//   jaws: the baked vertex colour red channel), _Hue (0 red-orange .. 1 amber: the tell's colour), _Hot (white-hot flash, the damage), _Burn (0 .. 1: the
//   piece burns out to ash and its emission goes out).
// Red-orange only (CLAUDE.md rule 8): _GlowColor and _AmberColor are both in the danger family; the only white is _Hot.
Shader "SKYGRIND/Wyrm26"
{
    Properties
    {
        [NoScaleOffset] _BaseMap ("Albedo (Tripo)", 2D) = "gray" {}
        [HDR] _GlowColor ("Glow (red-orange, linear)", Color) = (1, 0.07, 0.005, 1)
        [HDR] _AmberColor ("Glow at hue 1 (amber, linear)", Color) = (1, 0.32, 0.02, 1)
        [HDR] _HotColor ("White-hot (linear)", Color) = (1, 0.88, 0.67, 1)
        [HDR] _AshColor ("Ash (linear)", Color) = (0.01, 0.006, 0.005, 1)
        _Emission ("Emission gain (HDR)", Float) = 3.2
        _MaskLo ("Redness mask low", Float) = 0.22
        _MaskHi ("Redness mask high", Float) = 0.58
        _Wash ("Glow wash on the armour", Float) = 0.12
        _Tint ("Armour tint", Color) = (1, 1, 1, 1)
        _Ambient ("Lit floor (the dusk scene has almost no ambient light: the other enemies are unlit)", Float) = 0.55
        _Rim ("Red-orange rim on the silhouette", Float) = 0.8
        _RimPower ("Rim power", Float) = 2.5
        _Glow ("Glow (set per draw)", Float) = 0
        _Jaw ("Jaw glow (set per draw)", Float) = 0
        _Hue ("Hue (set per draw)", Float) = 0
        _Hot ("White-hot (set per draw)", Float) = 0
        _Burn ("Burn-out (set per draw)", Float) = 0
        _Len ("Mesh length (object space, m)", Float) = 1
        _SeamCount ("Seam bands per mesh length", Float) = 4
        _SeamOn ("Procedural seams", Float) = 0
        _EyeA ("Eye (abs x, y, z, radius) in object space", Vector) = (0.14, 0.1, 0.3, 0.09)
        _EyeOn ("Procedural eyes", Float) = 0
        _OrbY ("Orbs above this object-space height", Float) = 0.3
        _OrbOn ("Procedural orbs", Float) = 0
        _Proc ("Procedural mask strength", Float) = 0.85
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
        float4 _GlowColor;
        float4 _AmberColor;
        float4 _HotColor;
        float4 _AshColor;
        float _Emission;
        float _MaskLo;
        float _MaskHi;
        float _Wash;
        float4 _Tint;
        float _Ambient;
        float _Rim;
        float _RimPower;
        float _Glow;
        float _Jaw;
        float _Hue;
        float _Hot;
        float _Burn;
        float _Len;
        float _SeamCount;
        float _SeamOn;
        float4 _EyeA;
        float _EyeOn;
        float _OrbY;
        float _OrbOn;
        float _Proc;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            float4 color : COLOR; // r: the jaw mask baked by WormBossSetup (head only)
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 normalWS : TEXCOORD0;
            float2 uv : TEXCOORD1;
            float jawMask : TEXCOORD2;
            float fogFactor : TEXCOORD3;
            float3 viewWS : TEXCOORD4;
            float3 posOS : TEXCOORD5;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        Varyings WyrmVertex(Attributes v)
        {
            Varyings o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
            o.positionCS = p.positionCS;
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.uv = v.uv;
            o.jawMask = v.color.r;
            o.fogFactor = ComputeFogFactor(p.positionCS.z);
            o.viewWS = GetCameraPositionWS() - p.positionWS;
            o.posOS = v.positionOS.xyz;
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "UniversalForwardOnly"
            Tags { "LightMode" = "UniversalForwardOnly" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex WyrmVertex
            #pragma fragment frag
            #pragma multi_compile_fog

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float glow = _Glow, jaw = _Jaw, hue = _Hue, hot = _Hot, burn = _Burn;

                float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _Tint.rgb;
                float3 n = normalize(i.normalWS);
                Light l = GetMainLight();
                float lambert = saturate(dot(n, l.direction) * 0.5 + 0.5);
                float3 lit = albedo * (max(SampleSH(n), _Ambient.xxx) + l.color * lambert * 0.6);

                // the emission mask: how red the texel is
                float mask = smoothstep(_MaskLo, _MaskHi, albedo.r - 1.6 * albedo.g);
                // procedural emission that never depends on the texture's colours: seams every 1/_SeamCount of the mesh length, two eyes, orbs on the top
                float f = frac(i.posOS.z / max(_Len, 1e-3) * _SeamCount + 0.5);
                float seam = (1.0 - smoothstep(0.02, 0.08, abs(f - 0.5))) * _SeamOn;
                float eye = (1.0 - smoothstep(_EyeA.w * 0.6, _EyeA.w, length(float3(abs(i.posOS.x), i.posOS.y, i.posOS.z) - _EyeA.xyz))) * _EyeOn;
                float orb = smoothstep(_OrbY, _OrbY + 0.08, i.posOS.y) * _OrbOn;
                mask = max(mask, max(seam, max(eye, orb)) * _Proc);
                float3 glowColor = lerp(_GlowColor.rgb, _AmberColor.rgb, saturate(hue));
                float level = glow + jaw * i.jawMask;
                float3 emit = glowColor * (mask * level * _Emission);
                float3 wash = glowColor * (level * _Wash * dot(albedo, float3(0.3, 0.5, 0.2)));

                // a red-orange rim on the silhouette (the enemies' rim language, danger colour): the dark armour still reads against the dusk
                float ndv = saturate(dot(n, normalize(i.viewWS)));
                float3 rim = glowColor * (pow(1.0 - ndv, _RimPower) * _Rim * (0.4 + level));
                float3 c = lit + emit + wash + rim;
                // the damage flash: white-hot over the piece, the emission mask hottest
                c = lerp(c, _HotColor.rgb * (1.2 + mask * _Emission), saturate(hot));
                // the burn-out: the armour goes to ash and the glow goes out
                c = lerp(c, _AshColor.rgb + lit * 0.12, saturate(burn));
                c = MixFog(c, i.fogFactor);
                return half4(c, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex WyrmVertex
            #pragma fragment frag
            #pragma multi_compile_instancing

            half frag(Varyings i) : SV_Target
            {
                return i.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormalsOnly"
            Tags { "LightMode" = "DepthNormalsOnly" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex WyrmVertex
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            half4 frag(Varyings i) : SV_Target
            {
                #if defined(_GBUFFER_NORMALS_OCT)
                    float3 n = normalize(i.normalWS);
                    float2 oct = PackNormalOctQuadEncode(n);
                    float2 remapped = saturate(oct * 0.5 + 0.5);
                    return half4(PackFloat2To888(remapped), 0);
                #else
                    return half4(NormalizeNormalPerPixel(i.normalWS), 0);
                #endif
            }
            ENDHLSL
        }
    }
    FallBack Off
}
