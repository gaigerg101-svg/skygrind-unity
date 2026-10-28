// NIGHT29 item 4 (builder V): the CRT overlay (Game/Render/Crt29Pass.cs; Game/Crt29Math.cs is the same maths in C# for
// EditMode tests). Order: pixel step -> warp -> sample with the chromatic split -> scanlines -> grain -> vignette.
// The grain is an integer hash of the pixel and the flicker step (a fixed function: no free-running clock, no Random).
// The blit plumbing (Blit.hlsl's Vert, _BlitTexture) as Resources/TravelBlur.shader. In a Resources folder so Shader.Find has
// it in a Player without the Always Included list.
// NIGHT35 V (design.n35.look35 / ridelook35; Research/2026-10-11-wave35 RESEARCH.md: ULTRAKILL's 480-line downscale and 32-level colour compression):
// _Crt4 / _Crt5 add a point-upscaled virtual resolution, a flat chroma split, the quantise in sRGB on the virtual-pixel grid, a band tear and a
// sweeping scanline band. With _Crt4 = _Crt5 = 0 every new branch is skipped and the output is NIGHT30's. No keywords (one variant).
Shader "SKYGRIND/Crt29"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off
        Pass
        {
            Name "Crt29"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _Crt0; // x warp k (corner pull), y scanline intensity, z scanline count, w chroma uv at the corner
            float4 _Crt1; // x grain, y vignette, z grain step, w pixel block (px, 0 = off)
            float4 _Crt2; // xy the target's size in px
            float4 _Crt3; // NIGHT30 item 8 (THE LOOK): x levels, y dither cell (px), z quantise strength (0 off), w grain halved in the dark (0 off)
            // NIGHT35 V (look35 / ridelook35): every value 0 = NIGHT30's path exactly (no branch below runs; the flat split adds 0).
            float4 _Crt4; // x virtual lines (point upscale; 0 = off), y flat chroma split (uv x, the whole screen), z quantise in sRGB (1) or as stored (0), w glitch amount (0 off)
            float4 _Crt5; // x the glitch's step (a fixed hash seed, no clock), y the sweep band's centre (uv y), z the sweep's strength (0 off), w the sweep's half height (uv)
            // NIGHT72 lit72 (Game/Lit72.cs; Tools/night/n72-lit-sim.py is the same maths on the existing frames): the boss arenas' and level 5's shadow lift.
            float4 _Fix72; // NIGHT72 fix72 (Game/Fix72.cs): x 1 = the grain is added in display (sRGB) space (0 = linear light, as before); yzw unused
            float4 _Lit72; // x display luma added at black (A x the weight; 0 = off, the block below is skipped), y l0 (whole under it), z l1 (untouched from it), w gain cap

            // NIGHT35 look35: sRGB <-> linear by the standard curves (inline: Color.hlsl is not included here), so 32 levels step evenly to the eye
            float3 ToSrgb35(float3 c)
            {
                c = max(c, 0.0);
                return lerp(1.055 * pow(c, 1.0 / 2.4) - 0.055, c * 12.92, step(c, 0.0031308));
            }
            float3 ToLin35(float3 c)
            {
                c = max(c, 0.0);
                return lerp(pow((c + 0.055) / 1.055, 2.4), c / 12.92, step(c, 0.04045));
            }

            // NIGHT30: the ordered 4x4 Bayer matrix (Game/Look30Math.cs Bayer4, row-major)
            static const float Bayer4[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

            // NIGHT30: Look30Math.Quantise per channel: floor(c x (levels - 1) + 0.5 + dither) / (levels - 1), blended by the strength
            half3 Quantise30(half3 c, float2 px)
            {
                float s = max(1.0, round(_Crt3.y));
                uint2 cell = (uint2)floor(px / s) & 3u;
                float d = (Bayer4[cell.y * 4 + cell.x] + 0.5) / 16.0 - 0.5;
                float n = max(2.0, _Crt3.x) - 1.0;
                float3 q = saturate(floor(saturate(c) * n + 0.5 + d) / n);
                return (half3)lerp(c, q, _Crt3.z);
            }

            // NIGHT30: Look30Math.GrainScale: the grain's share at a luminance (0.5 under 0.075, whole from 0.15, smooth between)
            float GrainScale30(float luma)
            {
                float t = saturate((luma - 0.075) / 0.075);
                t = t * t * (3.0 - 2.0 * t);
                return lerp(0.5, 1.0, t);
            }

            uint CrtHash(uint x)
            {
                x ^= x >> 16;
                x *= 0x7feb352du;
                x ^= x >> 15;
                x *= 0x846ca68bu;
                x ^= x >> 16;
                return x;
            }

            float Noise(uint2 p, uint stepIndex)
            {
                uint h = CrtHash(p.x ^ CrtHash(p.y ^ CrtHash(stepIndex)));
                return (h & 0xFFFFFFu) / 16777216.0;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 res = _Crt2.xy;
                float2 uv0 = i.texcoord;
                float2 uv = uv0;
                float2 px = uv0 * res; // the dither / grain grid (screen px; look35: virtual px)
                if (_Crt4.x > 0.5)
                {
                    // NIGHT35 look35: draw at about _Crt4.x lines and upscale by point (each virtual pixel samples its centre once)
                    float2 vres = float2(max(1.0, round(res.x * _Crt4.x / max(res.y, 1.0))), _Crt4.x);
                    px = floor(uv0 * vres);
                    uv = (px + 0.5) / vres;
                }
                else if (_Crt1.w > 0.5)
                {
                    float2 b = _Crt1.w;
                    uv = (floor(uv * res / b) + 0.5) * b / res;
                }
                if (_Crt4.w > 0.0)
                {
                    // NIGHT35 ridelook35: a faint TV tear: about one band in six (of 48) slides sideways this step, a fixed hash of (band, step)
                    uint band = (uint)floor(uv0.y * 48.0);
                    float hb = Noise(uint2(band, 17u), (uint)_Crt5.x);
                    float tear = saturate((hb - 0.83) / 0.17);
                    uv.x += (Noise(uint2(band, 29u), (uint)_Crt5.x) - 0.5) * 0.03 * _Crt4.w * tear;
                }
                float2 c = uv * 2.0 - 1.0;
                float r2 = dot(c, c);
                float k = _Crt0.x;
                float2 cw = c * ((1.0 + k * r2 * 0.5) / (1.0 + k));
                float2 uvw = cw * 0.5 + 0.5;
                float2 off = cw * (_Crt0.w * dot(cw, cw) * 0.5) + float2(_Crt4.y, 0.0); // NIGHT35: + the flat split (0 = NIGHT29's)
                half3 col;
                col.r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uvw + off, 0).r;
                half4 mid = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uvw, 0);
                col.g = mid.g;
                col.b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uvw - off, 0).b;
                float s = 0.5 + 0.5 * sin(uvw.y * _Crt0.z * 6.2831853);
                col *= (1.0 - _Crt0.y * (1.0 - s)) / (1.0 - _Crt0.y * 0.5);
                if (_Crt5.z > 0.0)
                {
                    // NIGHT35 ridelook35: a bright scanline band sweeping down the screen (the C# side moves _Crt5.y)
                    float d = abs(uv0.y - _Crt5.y) / max(_Crt5.w, 1e-4);
                    col *= 1.0 + _Crt5.z * saturate(1.0 - d) * (0.6 + 0.4 * s);
                }
                if (_Lit72.x > 0.0)
                {
                    // NIGHT72 lit72: after the scanlines, BEFORE the quantise (the 32 levels stay even steps) and the grain (so the grain is not scaled up in the
                    // shadows): each pixel's display luma rises by A x w in its OWN hue (the colour is scaled, never replaced): w = 1 under l0, 0 from l1; the
                    // gain is capped; a lit pixel is left alone.
                    float3 dl = ToSrgb35(col);
                    float ly = dot(dl, float3(0.299, 0.587, 0.114));
                    float wl = 1.0 - smoothstep(_Lit72.y, _Lit72.z, ly);
                    float kl = min(_Lit72.w, (ly + _Lit72.x * wl) / max(ly, 0.0001));
                    if (kl > 1.0001) col = (half3)ToLin35(saturate(dl * kl));
                }
                if (_Crt3.z > 0.0)
                {
                    // NIGHT30 THE LOOK: after the scanlines, before the grain. NIGHT35 look35 (_Crt4.z): in sRGB, so the levels are even steps to the eye
                    if (_Crt4.z > 0.5) col = (half3)ToLin35(Quantise30((half3)ToSrgb35(col), px));
                    else col = Quantise30(col, px);
                }
                float n = Noise(uint2(px), (uint)_Crt1.z);
                float gs = _Crt3.w > 0.5 ? GrainScale30(dot(col, half3(0.2126, 0.7152, 0.0722))) : 1.0; // NIGHT30: half the grain in the dark
                float grainAdd = (n - 0.5) * _Crt1.x * gs;
                // NIGHT72 fix72: the grain was added in LINEAR light, where the sRGB curve turns +-0.006 into +-6..10 levels in a dark pixel (slope 6 at linear 0.01)
                // and +-1.5 on a bright one: a flicker that lives in the shadows (24 Hz re-seed). With _Fix72.x it is added in display space: the same +-1.5 levels
                // everywhere. _Fix72.x = 0: NIGHT30's linear add exactly.
                if (_Fix72.x > 0.5) col = (half3)ToLin35(ToSrgb35(col) + grainAdd);
                else col += grainAdd;
                float t = saturate((length(c) - 0.4) / (1.41 - 0.4));
                col *= 1.0 - _Crt1.y * (t * t * (3.0 - 2.0 * t));
                return half4(max(col, 0), mid.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
