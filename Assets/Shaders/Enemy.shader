// NIGHT12 U3-25.3: the enemies' one shader (every part of the husk, swarmer and spitter views; GG's models and colours
// kept as they are). The body is the web's MeshBasicMaterial (an unlit _BaseColor, fogged as URP Unlit does: MixFog)
// plus the web's own looks:
// - the hit flash: _Flash lerps the colour to white (the views set 1 while the enemy's flash time runs, husk.js: WHITE);
// - the rim (fx/rimGlow.js): rgb += _RimColor * pow(1 - saturate(dot(N, V)), _RimPower), before the fog; _RimColor is
//   fx.enemyRimColor (linear) x fx.enemyRimStrength, and grows with distance (_RimBoost: near m, far m, max x; the
//   port's Valorant rule, UnityTuning). _RimOn 0 turns it off for a part (the spitter's sac: rim: false);
// - the death dissolve (fx/dissolve.js on a dead enemy's frozen pose): 2.5 cm cells (_DissolveCells) of a noise texture
//   made in code at load; a cell whose noise x (1 - band) is over _Dissolve is discarded, one inside the band below it
//   shows _EdgeColor (after the fog, as dissolve.js writes it last). _Dissolve 1 = whole (no cost but one branch);
// - the x-ray (fx/xray.js): a second pass drawn where the body is hidden (ZTest Greater, ZWrite Off, polygon offset
//   -2 / -8), red-orange at aim.xray.alpha with a brighter fresnel rim, fading out from fadeStart to fadeEnd m, x the
//   elite boost, alpha capped at 0.9; normal blending, no fog. It lives on a second material (the transparent queue,
//   after every wall wrote depth) with its other passes disabled; the body material has the x-ray pass disabled.
// Each pass is named after its LightMode tag, so Material.SetShaderPassEnabled finds it by either.
// The depth passes clip the dissolve too, so the depth and normals textures (SSAO) match what is drawn.
// - NIGHT63 V C2b nearFade63 (NearFade63.N63V.cs; Logs/n63/verdict-C2.md section 6): a live part the eye is at fades through a SCREEN-DOOR of
//   its own (_NearFade, 1 whole .. 0 gone; 1 returns before any cost): a 4 x 4 ordered (Bayer) dither in screen pixels, _NearFade63.x px a
//   cell at 1080p, so the near body reads as an even see-through screen, never the death's sheared 3D cells. The body keeps its rim (N.V under
//   _NearFade63.y stays: you still read where it is); the ink hull is cut at the same pixel as the body (its far side never shows through a
//   hole: C2's black flakes) and thins to nothing at _NearFade63.z; the depth passes cut the same; the x-ray pass is untouched.
Shader "SKYGRIND/Enemy"
{
    Properties
    {
        [HDR] _BaseColor ("Color", Color) = (1, 0.42, 0.21, 1)
        _Flash ("Hit flash (1 = white)", Float) = 0
        _RimOn ("Rim on (0 / 1)", Float) = 1
        [HDR] _RimColor ("Rim (linear, x strength)", Color) = (1.6, 0.53, 0.14, 1)
        _RimPower ("Rim power", Float) = 2.2
        _RimBoost ("Rim distance boost (near m, far m, max x)", Vector) = (15, 60, 1.6, 0)
        _Dissolve ("Dissolve (1 whole, 0 gone)", Float) = 1
        [HDR] _EdgeColor ("Dissolve edge (linear)", Color) = (3, 1, 0.4, 1)
        _DissolveCells ("Dissolve cells per m", Float) = 40
        _DissolveBand ("Dissolve edge band", Float) = 0.12
        [NoScaleOffset] _NoiseTex ("Dissolve noise", 2D) = "gray" {}
        _NoiseSize ("Noise texels a side", Float) = 64
        [HDR] _XrayColor ("X-ray (linear)", Color) = (1, 0.07, 0.01, 1)
        _XrayAlpha ("X-ray alpha", Float) = 0.35
        _XrayEdge ("X-ray edge", Float) = 1.4
        _XrayFade ("X-ray fade (start m, end m)", Vector) = (45, 60, 0, 0)
        _XrayBoost ("X-ray boost (elites)", Float) = 1
        _Form ("models43 form shade (0 = flat)", Float) = 0
        _NearFade ("Near fade (NIGHT63 nearFade63: 1 whole, 0 gone; a screen-door, never the death's dissolve)", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _Flash;
        float _RimOn;
        float4 _RimColor;
        float _RimPower;
        float4 _RimBoost;
        float _Dissolve;
        float4 _EdgeColor;
        float _DissolveCells;
        float _DissolveBand;
        float _NoiseSize;
        float4 _XrayColor;
        float _XrayAlpha;
        float _XrayEdge;
        float4 _XrayFade;
        float _XrayBoost;
        float _Form;
        float _NearFade;
        CBUFFER_END

        float _Flash51Hdr; // NIGHT51 (Night51.Apply): a global, outside the per-material buffer
        // NIGHT52 skin52 (Skin52.Apply; globals, outside the per-material buffer; unset = 0 = off: every pass draws exactly as NIGHT51):
        //   _Skin52       x on (0 / 1), y the rim's near m, z its far m, w the rim's share at the near m (the close-up "jelly" cut)
        //   _Skin52Rim    x the rim's power at the near m (a thin edge light close up; the material's _RimPower from the far m on); y the m the
        //                 ramp is full to, z the m it is flat from (round 2: the far read stays NIGHT51's bright body)
        //   _Skin52Key    the ramp's key light: x up, y toward the camera (flat), z along the camera's right (weights of a direction)
        //   _Skin52Ramp   x the terminator (N.L), y its half-softness, z the lit side's gain
        //   _Skin52Shade  rgb the shadow side's multiplier (linear; a cool dusk shadow, the body's hue kept)
        float4 _Skin52, _Skin52Rim, _Skin52Key, _Skin52Ramp, _Skin52Shade;
        // NIGHT63 V C2b nearFade63 (EnemyLook.Frame: a global, outside the per-material buffer; unset = 0): x the screen-door's cell (px at
        // 1080p; under 1: 1), y the rim kept (a pixel whose N.V is under it is never cut), z the fade the ink outline is gone at
        float4 _NearFade63;

        TEXTURE2D(_NoiseTex);
        SAMPLER(sampler_NoiseTex);

        // dissolve.js dissolveHash: one value per cell (floor(p * uCell)). The cell's three coordinates fold into the
        // noise texture (point filtered, repeating): y shifts x and z by two primes so stacked cells differ.
        float DissolveNoise(float3 positionWS)
        {
            float3 c = floor(positionWS * _DissolveCells);
            float2 t = c.xz + c.y * float2(37, 59);
            float2 uv = (frac(t / _NoiseSize) * _NoiseSize + 0.5) / _NoiseSize;
            return SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, uv, 0).r;
        }

        // dissolve.js: n = hash * (1 - uBand); n > uDissolve is discarded; n > uDissolve - uBand is the edge (returns 1).
        float DissolveClip(float3 positionWS)
        {
            if (_Dissolve >= 1) return 0;
            float n = DissolveNoise(positionWS) * (1 - _DissolveBand);
            clip(_Dissolve - n);
            return n > _Dissolve - _DissolveBand ? 1 : 0;
        }

        // NIGHT63 V C2b: the screen-door's threshold for this pixel: the 4 x 4 ordered (Bayer) matrix over cells of _NearFade63.x px at 1080p,
        // (m + 0.5) / 16 with m = 4 b(x0, y0) + b(x1, y1), b(x, y) = 2 (x xor y) + y over the cell's low bit (x0, y0) and next bit (x1, y1).
        // NearFade63.Bayer is this formula in C# (N63ScreenDoorTests: each threshold once a tile, no clumps).
        float NearDither(float2 pixel)
        {
            float cell = max(1, round(max(_NearFade63.x, 1) * _ScreenParams.y / 1080.0));
            float2 q = floor(pixel / cell);
            float2 h = floor(q / 2);
            float2 a = q - 2 * h;
            float2 b = h - 2 * floor(h / 2);
            float m = 4 * (2 * abs(a.x - a.y) + a.y) + (2 * abs(b.x - b.y) + b.y);
            return (m + 0.5) / 16;
        }

        // NIGHT63 V C2b: a fading part's pixel is kept when its threshold is at or under _NearFade (clip keeps 0) or, with keepRim, when its
        // surface is at the silhouette (N.V under _NearFade63.y: the rim stays). _NearFade 1 (every part far, or the row off): no cost but a branch.
        void NearClip(float4 positionCS, float ndv, float keepRim)
        {
            if (_NearFade >= 1) return;
            if (keepRim > 0 && ndv < _NearFade63.y) return;
            clip(_NearFade - NearDither(positionCS.xy));
        }

        // NIGHT63 V C2b: the ink outline's share of its width on a fading part: 1 whole, 0 at or under _NearFade63.z (the near body has no outline).
        float InkNearK()
        {
            if (_NearFade >= 1) return 1;
            return saturate((_NearFade - _NearFade63.z) / max(1 - _NearFade63.z, 1e-4));
        }

        // NIGHT63 V C2b: N.V at a fragment (the rim keep in the depth passes, as the body's).
        float NearNdv(float3 positionWS, float3 normalWS)
        {
            float3 toCam = GetCameraPositionWS() - positionWS;
            return saturate(dot(normalize(normalWS), toCam / max(length(toCam), 1e-4)));
        }

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float fogFactor : TEXCOORD2;
        };

        Varyings EnemyVertex(Attributes v)
        {
            Varyings o;
            VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
            o.positionCS = p.positionCS;
            o.positionWS = p.positionWS;
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.fogFactor = ComputeFogFactor(p.positionCS.z);
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
            #pragma target 3.0 // the noise's explicit LOD sample (target 2.5 has none)
            #pragma vertex EnemyVertex
            #pragma fragment frag
            #pragma multi_compile_fog

            half4 frag(Varyings i) : SV_Target
            {
                float edge = DissolveClip(i.positionWS);
                float3 c;
                float3 toCam = GetCameraPositionWS() - i.positionWS;
                float dist = max(length(toCam), 1e-4);
                float3 nWS = normalize(i.normalWS);
                float ndv = saturate(dot(nWS, toCam / dist));
                NearClip(i.positionCS, ndv, 1); // NIGHT63 V C2b: the near screen-door, the rim kept
                float boost = lerp(1, _RimBoost.z, saturate((dist - _RimBoost.x) / max(_RimBoost.y - _RimBoost.x, 1e-4)));
                if (_Skin52.x > 0.5)
                {
                    // NIGHT52 skin52: a ramp-lit body (NiloCat's toon lit, MIT: smoothstep(mid - soft, mid + soft, N.L) between a shadow colour
                    // and the lit one) under a key light fixed to the view (from above, toward the camera, from its left: every enemy reads as a
                    // volume in any room's light); the shadow side keeps the body's hue, darker and cooler. Then the flash (white x _Flash51Hdr).
                    float3 flatTo = float3(toCam.x, 0, toCam.z);
                    flatTo = flatTo / max(length(flatTo), 1e-4);
                    float3 key = normalize(float3(0, _Skin52Key.x, 0) + flatTo * _Skin52Key.y + UNITY_MATRIX_V[0].xyz * _Skin52Key.z);
                    float litK = smoothstep(_Skin52Ramp.x - _Skin52Ramp.y, _Skin52Ramp.x + _Skin52Ramp.y, dot(nWS, key));
                    // round 2: the ramp fades out with distance (full to _Skin52Rim.y m, flat from _Skin52Rim.z m): up close a lit volume, at range
                    // the bright NIGHT51 body (the far read: the colour, the rim and the ink carry it)
                    float rampK = 1 - saturate((dist - _Skin52Rim.y) / max(_Skin52Rim.z - _Skin52Rim.y, 1e-4));
                    float3 shadeC = lerp(_Skin52Ramp.zzz, _Skin52Shade.rgb, rampK);
                    c = _BaseColor.rgb * lerp(shadeC, _Skin52Ramp.zzz, litK);
                    c = lerp(c, max(_Flash51Hdr, 1).xxx, _Flash);
                    // the rim cut close up: at _Skin52.y m and nearer, _Skin52.w of its strength at a higher power (a thin edge light, not a
                    // glowing gel); the material's rim from _Skin52.z m on (and the far boost past _RimBoost.x, unchanged)
                    float farK = saturate((dist - _Skin52.y) / max(_Skin52.z - _Skin52.y, 1e-4));
                    c += _RimColor.rgb * (pow(1 - ndv, lerp(_Skin52Rim.x, _RimPower, farK)) * boost * _RimOn * lerp(_Skin52.w, 1, farK));
                }
                else
                {
                    // NIGHT51 crunch51: the flash's white x _Flash51Hdr (a global; 2.4 on, 1 off or unset): over the bloom threshold, a hit glows
                    c = lerp(_BaseColor.rgb, max(_Flash51Hdr, 1).xxx, _Flash);
                    // NIGHT43 A1 models43: the form shade (the sky from above: the underside 0.62, a side 0.87, the top 1.12) so the smooth meshes read as
                    // volumes; _Form 0 (every enemy with the row off) leaves the colour exactly as it was (x 1)
                    c *= lerp(1, 0.62 + 0.5 * saturate(nWS.y * 0.5 + 0.5), _Form);
                    c += _RimColor.rgb * (pow(1 - ndv, _RimPower) * boost * _RimOn);
                }
                c = MixFog(c, i.fogFactor);
                if (edge > 0) c = _EdgeColor.rgb;
                return half4(c, 1);
            }
            ENDHLSL
        }

        // NIGHT51 ink51: a dark outline (an inverted hull pushed out a constant _Ink51.x px at 1080p in clip space), so an enemy's silhouette
        // reads against the warm floors and walls at any range. Drawn by URP's forward list with the body (another LightMode, so both
        // draw); the x-ray materials turn it off; width 0 (the global unset, or -skygrindOff51) draws nothing.
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite On
            Cull Front
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex InkVertex
            #pragma fragment frag
            #pragma multi_compile_fog

            float4 _Ink51; // x the width (px at 1080p), yzw the ink colour (linear)

            // NIGHT52 skin52: the hull's push direction from the mesh's smoothed normals (QuickOutline's technique, MIT: the normals of the
            // vertices that share a position averaged, baked into UV channel 3 by Skin52.Bake), so a flat-shaded corner's split vertices move
            // together and the outline has no gap there. A mesh without the channel (or skin52 off) pushes along its own normal, as NIGHT51.
            struct AttributesInk
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 smoothOS : TEXCOORD3;
            };

            Varyings InkVertex(AttributesInk vi)
            {
                Attributes v;
                v.positionOS = vi.positionOS;
                v.normalOS = vi.normalOS;
                Varyings o = EnemyVertex(v);
                float3 nW = o.normalWS;
                if (_Skin52.x > 0.5 && dot(vi.smoothOS.xyz, vi.smoothOS.xyz) > 0.25) nW = TransformObjectToWorldNormal(vi.smoothOS.xyz);
                float3 nCS = mul((float3x3)UNITY_MATRIX_VP, nW);
                float2 dir = nCS.xy;
                float len = length(dir);
                dir = len > 1e-5 ? dir / len : float2(0, 0);
                float px = _Ink51.x * InkNearK() * _ScreenParams.y / 1080.0; // NIGHT63 V C2b: thinner as a near part fades (InkNearK 1 when whole)
                o.positionCS.xy += dir * (px * 2.0 / _ScreenParams.xy) * o.positionCS.w;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                clip(_Ink51.x - 0.01);
                clip(InkNearK() - 0.01); // NIGHT63 V C2b: no outline on a near part faded to _NearFade63.z (from inside the hull it is all far side)
                DissolveClip(i.positionWS);
                NearClip(i.positionCS, 1, 0); // NIGHT63 V C2b: cut at the same pixel as the body, so its dark far side never shows through a hole
                return half4(MixFog(_Ink51.yzw, i.fogFactor), 1);
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
            #pragma vertex EnemyVertex
            #pragma fragment frag

            half frag(Varyings i) : SV_Target
            {
                DissolveClip(i.positionWS);
                NearClip(i.positionCS, NearNdv(i.positionWS, i.normalWS), 1); // NIGHT63 V C2b: the depth as the body draws it
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
            #pragma vertex EnemyVertex
            #pragma fragment frag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            // URP's UnlitDepthNormalsPass output (the world normal, octahedron-packed when the renderer asks).
            half4 frag(Varyings i) : SV_Target
            {
                DissolveClip(i.positionWS);
                NearClip(i.positionCS, NearNdv(i.positionWS, i.normalWS), 1); // NIGHT63 V C2b: the normals as the body draws it
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

        Pass
        {
            Name "SRPDefaultUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            ZWrite Off
            ZTest Greater
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            Offset -2, -8
            HLSLPROGRAM
            #pragma vertex EnemyVertex
            #pragma fragment frag

            // xray.js FRAGMENT, line for line (vTint is white: the x-ray mesh has no instance colours).
            half4 frag(Varyings i) : SV_Target
            {
                float3 toCam = GetCameraPositionWS() - i.positionWS;
                float dist = max(length(toCam), 1e-4);
                float rim = pow(1 - saturate(abs(dot(normalize(i.normalWS), toCam / dist))), 2.0);
                float fade = 1 - smoothstep(_XrayFade.x, _XrayFade.y, dist);
                float a = clamp((_XrayAlpha + rim * _XrayEdge * _XrayAlpha) * fade * _XrayBoost, 0, 0.9);
                return half4(_XrayColor.rgb * (1 + rim * _XrayEdge * 0.6), a);
            }
            ENDHLSL
        }
    }
}
