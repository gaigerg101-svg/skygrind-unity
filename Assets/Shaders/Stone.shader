// The map's stone, after skygrind/src/world/arena.js colorStone + worldMaterial.js (the parts ported so far): vertex
// colors (config.colors.pillarTop on tops, pillarSide on sides), a world-space 1 m grid with 4 m plates
// (config.world.gridMinor / gridMajor), Lambert from the main light with its shadows, sky ambient (SH), a violet rim
// from a second direction (globals _RimDir / _RimColor, like the web's rim light), and the fog. The web's trim sheet,
// AO volume, floor sheen and zone grey-out are later.
// NIGHT12 U3-25.2 (light and lens):
// - URP's SSAO reaches the stone (_SCREEN_SPACE_OCCLUSION; the renderer's SSAO runs before the opaques, After Opaque off,
//   from the DepthNormals prepass the Lit DepthNormals pass below writes): the indirect AO darkens the ambient, the
//   direct AO (the feature's Direct Lighting Strength) the key, the rim and every point light, as URP's Lit does.
// - The point lights (lightPool.js's pools and blasts, effects.js's muzzle flash and pops: LightPools.cs) light it:
//   the Forward+ cluster loop (_CLUSTER_LIGHT_LOOP, LIGHT_LOOP_BEGIN / END) or the forward per-object list
//   (_ADDITIONAL_LIGHTS), Lambert x the lights' distance attenuation (URP's 1 / d^2 with the smooth range window: three's
//   decay-2 PointLight with a cutoff distance, the same curve).
// - The web's fog (worldMaterial.js FRAGMENT_FOG) when Look.Apply set the _SkyFog* globals: smoothstep(near, far, view
//   depth) x mix(1, fog.heightMin, smoothstep(0, fog.heightFade, y)): it falls away with height, so high ledges stay
//   clear while the low far map sinks into haze, coloured as the sky behind it along the view ray (SkyAt: Sky.shader's
//   gradient and glow without the disc), so the far world fades into the dome and nothing pops. Without the globals
//   (another scene): URP's linear fog, as before.
Shader "SKYGRIND/Stone"
{
    Properties
    {
        _GridMinor ("Grid minor (darken)", Float) = 0.05
        _GridMajor ("Grid major (darken)", Float) = 0.11
        _Plate ("Plate size (m)", Float) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _GridMinor;
            float _GridMajor;
            float _Plate;
            CBUFFER_END
            float4 _RimDir;
            float4 _RimColor;
            // Look.Apply (NIGHT12): fog near, far, heightFade, heightMin; the sky's colours; the sun direction + its glow.
            float4 _SkyFogParams;
            float4 _SkyFogZenith;
            float4 _SkyFogMid;
            float4 _SkyFogHorizon;
            float4 _SkyFogSun;
            float _SkyFogOn;
            // NIGHT51 stone51 (UnityTuning.Stone51.cs, set by Look.Apply): A = wall panel w, h, floor tile, bevel (m); B = seam (m), seam
            // dark, bevel light, panel tone; C = grime height (m), grime dark, rise light, on (0 / 1).
            float4 _Stone51A;
            float4 _Stone51B;
            float4 _Stone51C;
            float4 _Stone51D; // round 2: x bevel light added, y the lip's light added (in albedo: a dark wall's edges show too)
            // NIGHT74 crown74 (Game/Crown74.cs): THE CROWN's arena's stone lifted with the same cool fill as its ceiling (Ceiling27.shader's block). Globals the C# side sets (zero = off: the block in frag is skipped).
            float4 _Crown74;  // rgb the fill in display (sRGB) values 0-1, a the weight 0-1
            float4 _Crown74B; // x display luma under which a pixel takes the fill whole, y display luma from which it takes none

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float4 face : TEXCOORD3; // NIGHT51: MeshKit's face coordinates (u, v, width, height in m); 0 on other meshes
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : COLOR;
                float fogFactor : TEXCOORD2;
                float4 face : TEXCOORD3;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.color = v.color;
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                o.face = v.face;
                return o;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // NIGHT51 stone51: the face's panels (sized to divide it evenly), seams, a lit bevel on its edges (a wall's foot excepted),
            // grime at a wall's foot, a rise of light up the wall, a tone per panel. Returns the albedo's multiplier (x; 1 = unchanged) and
            // the light added to it (y; round 2: the bevel and the lip under a wall's horizontal seams).
            float2 Stone51(float4 face, float3 n, float3 positionWS)
            {
                float2 uv = face.xy, size = face.zw;
                bool wall = abs(n.y) < 0.5;
                float2 target = wall ? _Stone51A.xy : _Stone51A.zz;
                float2 count = max(1, round(size / target));
                float2 cell = size / count;
                float2 local = uv / cell;
                float2 idx = floor(local);
                float2 fr = frac(local);
                float aa = max(max(fwidth(uv.x), fwidth(uv.y)), 1e-4);
                // seams: interior panel lines only (the face's own edges get the bevel)
                float2 dl = min(fr, 1 - fr) * cell;
                float2 inner = float2(idx.x > 0 || fr.x > 0.5 ? 1 : 0, idx.y > 0 || fr.y > 0.5 ? 1 : 0);
                inner *= float2(idx.x < count.x - 1 || fr.x < 0.5 ? 1 : 0, idx.y < count.y - 1 || fr.y < 0.5 ? 1 : 0);
                float2 seamD = lerp(1e3, dl, inner);
                float seam = 1 - smoothstep(_Stone51B.x * 0.5 - aa, _Stone51B.x * 0.5 + aa, min(seamD.x, seamD.y));
                // the bevel: distance to the face's edges (a wall's foot is a floor joint: no bevel there)
                float4 ed = float4(uv.x, size.x - uv.x, wall ? 1e3 : uv.y, size.y - uv.y);
                float e = min(min(ed.x, ed.y), min(ed.z, ed.w));
                // round 5: sized to the face (the guns, pickups and boss blocks share this material: a 6 cm bevel lit a 4 cm gun face whole)
                float bw = min(_Stone51A.w, 0.15 * min(size.x, size.y));
                float bevel = 1 - smoothstep(bw - aa, bw + aa, e);
                // round 2: the lip, a band under each interior horizontal seam on a wall (the lower panel's top edge, lit from above)
                float lipD = (1 - fr.y) * cell.y; // m below the seam over this panel
                float lip = wall && idx.y < count.y - 1 ? (1 - smoothstep(_Stone51B.x * 0.5, _Stone51B.x * 0.5 + 2.5 * _Stone51B.x, lipD)) * smoothstep(_Stone51B.x * 0.5 - aa, _Stone51B.x * 0.5 + aa, lipD) : 0;
                float add = _Stone51D.x * bevel + _Stone51D.y * lip * (1 - bevel);
                float k = 1 - _Stone51B.y * seam * (1 - bevel);
                k *= 1 + _Stone51B.z * bevel;
                // a tone per panel (the face's plane offset tells faces apart)
                float faceId = dot(positionWS, n);
                k *= 1 + _Stone51B.w * saturate(min(size.x, size.y) / 0.5) * (Hash21(idx + floor(faceId * 3.1) * 0.37) * 2 - 1); // none on a small part
                if (wall)
                {
                    k *= 1 - _Stone51C.y * (1 - smoothstep(0, min(_Stone51C.x, 0.35 * size.y), uv.y)); // round 5: a small face's grime is its own foot only
                    k *= 1 + _Stone51C.z * saturate(uv.y / max(size.y, 1));
                }
                return float2(k, add);
            }

            // An antialiased line on every integer of uv (1 = on the line).
            float GridLine(float2 uv)
            {
                float2 w = max(fwidth(uv), 1e-4);
                float2 d = abs(frac(uv - 0.5) - 0.5) / w;
                return 1 - saturate(min(d.x, d.y) - 0.5);
            }

            // The sky's colour along a unit direction: Sky.shader's frag (its literals) without the sun disc.
            float3 SkyAt(float3 d)
            {
                float h = d.y;
                float3 c = h > 0 ? lerp(_SkyFogMid.rgb, _SkyFogZenith.rgb, saturate(pow(max(h, 1e-5), 0.6))) : _SkyFogMid.rgb;
                c = lerp(c, _SkyFogHorizon.rgb, exp(-abs(h) * 7));
                c += _SkyFogHorizon.rgb * _SkyFogSun.w * pow(saturate(dot(d, normalize(_SkyFogSun.xyz))), 8) * 0.8;
                if (h < 0) c = lerp(c, _SkyFogMid.rgb * 0.55, saturate(-h * 4));
                return c;
            }

            // worldMaterial.js FRAGMENT_FOG (USE_FOG): height fog in the sky's colour; URP's fog without Look's globals.
            float3 ApplyFog(float3 lit, float3 positionWS, float fogFactor)
            {
                if (_SkyFogOn < 0.5) return MixFog(lit, fogFactor);
                float depth = -TransformWorldToView(positionWS).z;
                float fogF = smoothstep(_SkyFogParams.x, _SkyFogParams.y, depth);
                fogF *= lerp(1.0, _SkyFogParams.w, smoothstep(0.0, _SkyFogParams.z, positionWS.y));
                float3 skyFog = SkyAt(normalize(positionWS - _WorldSpaceCameraPos.xyz));
                return lerp(lit, skyFog, fogF);
            }

            // One additional light's Lambert (its colour already carries the direct AO: GetAdditionalLight with aoFactor).
            float3 StoneLambert(Light l, float3 n)
            {
                return l.color * (l.distanceAttenuation * l.shadowAttenuation * saturate(dot(n, l.direction)));
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 an = abs(n);
                float2 uv = an.y > 0.5 ? i.positionWS.xz : (an.x > an.z ? i.positionWS.zy : i.positionWS.xy);
                float minor = GridLine(uv);
                float major = GridLine(uv / _Plate);
                float3 albedo;
                if (_Stone51C.w > 0.5 && i.face.z > 0.01 && i.face.w > 0.01)
                {
                    // NIGHT51: the face's panels replace the world grid (a top keeps the faint 1 m minor grid: the floor's speed cue)
                    float keepMinor = an.y > 0.5 ? 1 : 0;
                    float2 s51 = Stone51(i.face, n, i.positionWS);
                    albedo = i.color.rgb * (1 - _GridMinor * minor * keepMinor) * s51.x + s51.y;
                }
                else albedo = i.color.rgb * (1 - _GridMinor * minor) * (1 - _GridMajor * major);

                float2 screenUV = GetNormalizedScreenSpaceUV(i.positionCS);
                AmbientOcclusionFactor ao = GetScreenSpaceAmbientOcclusion(screenUV);
                Light l = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float ndl = saturate(dot(n, l.direction));
                float rim = saturate(dot(n, normalize(_RimDir.xyz)));
                float3 direct = (l.color * ndl * l.shadowAttenuation + _RimColor.rgb * rim) * ao.directAmbientOcclusion;

            #if defined(_ADDITIONAL_LIGHTS)
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = n;
                inputData.normalizedScreenSpaceUV = screenUV;
                half4 shadowMask = half4(1, 1, 1, 1);
              #if USE_CLUSTER_LIGHT_LOOP
                // Forward+: additional directional lights come first, outside the clusters (URP Lit's loop).
                [loop] for (uint dirIndex = 0; dirIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); dirIndex++)
                {
                    direct += StoneLambert(GetAdditionalLight(dirIndex, inputData, shadowMask, ao), n);
                }
              #endif
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    direct += StoneLambert(GetAdditionalLight(lightIndex, inputData, shadowMask, ao), n);
                LIGHT_LOOP_END
            #endif

                float3 lit = albedo * (SampleSH(n) * ao.indirectAmbientOcclusion + direct);
                float3 outc = ApplyFog(lit, i.positionWS, i.fogFactor);
                if (_Crown74.a > 0.0)
                {
                    // NIGHT74 crown74 (the retake, n74-t2: the ceiling alone lifted a third of what its region needed: the arena's walls are half the region): the fill is added in
                    // display space (the sRGB curves, inline as Crt29.shader's), strongest on the darkest pixels (w = 1 under _Crown74B.x, 0 from _Crown74B.y); a lit face is left alone.
                    float3 dsp = max(outc, 0.0);
                    dsp = lerp(1.055 * pow(dsp, 1.0 / 2.4) - 0.055, dsp * 12.92, step(dsp, 0.0031308));
                    float lum = dot(dsp, float3(0.299, 0.587, 0.114));
                    float wc = 1.0 - smoothstep(_Crown74B.x, _Crown74B.y, lum);
                    dsp = saturate(dsp + _Crown74.rgb * (_Crown74.a * wc));
                    outc = lerp(pow((dsp + 0.055) / 1.055, 2.4), dsp / 12.92, step(dsp, 0.04045));
                }
                return half4(outc, 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}
