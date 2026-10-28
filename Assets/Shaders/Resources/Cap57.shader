// NIGHT57 builder V (slate V1-colour56: "cap the world first"; Game/Render/Cap57Pass.cs drives it; Tools/night/n57-v-cap.py and Cap57Math run the
// same maths for the tests and the simulation). Valve's Left 4 Dead saturation threshold (research/design.md lesson 7; colors.md method 3): every
// pixel whose hue is OUTSIDE the role families (_Cap57A / _Cap57B: danger + yellow 350-70, cyan 150-200, magenta / FUSION 285-335) and whose
// saturation (HSV, on the sRGB value the frame shows) is over the level's cap moves to the cap toward the grey of its own luminance: hue and
// luminance kept, so it is never a tint and the greyscale frame (E-3's check) does not change; a pixel under the cap, or in a role family, is
// returned untouched (the family's edge feathered over _Cap57.y deg inside it). NIGHT57 V4 (yellow56): the same pass also holds a DIM yellow
// (_Cap57Y / _Cap57YH) under its own cap, by value, so the bright parry tells stay the only saturated yellow. The blit plumbing (Blit.hlsl's Vert,
// _BlitTexture) as Resources/Crt29.shader. In a Resources folder so Shader.Find
// has it in a Player. No keywords (one variant).
Shader "SKYGRIND/Cap57"
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
            Name "Cap57"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _Cap57;  // x the cap (HSV saturation, 0..1; >= 1 moves nothing), y the feather (deg), z test in sRGB (1) or as stored (0), w the
                            // keep bound's value floor (NIGHT59 V2, dimYellow58.sKeepVMin; 0 = none)
            float4 _Cap57A; // the protected arcs (deg; lo > hi wraps through 0): x lo0, y hi0, z lo1, w hi1
            float4 _Cap57B; // x lo2, y hi2, zw unused
            float4 _Cap57Y; // yellow56 (NIGHT57 V4): x the dim yellow's S cap, y the value (sRGB max channel) at or under which it is whole, z the value at
                            // or over which it is none (smoothstep between), w 1 = on
            float4 _Cap57YH; // x the yellow arc's lo, y its hi (deg), z its feather (deg), w the keep bound (NIGHT59 V2, dimYellow58.sKeep: a
                             // yellow at or over this saturation is untouched whatever its value; <= 0 = none, NIGHT58's rule)

            // sRGB <-> linear by the standard curves (as Crt29.shader's ToSrgb35 / ToLin35): the meter reads S on the sRGB frame
            float3 ToSrgb57(float3 c)
            {
                c = max(c, 0.0);
                return lerp(1.055 * pow(c, 1.0 / 2.4) - 0.055, c * 12.92, step(c, 0.0031308));
            }
            float3 ToLin57(float3 c)
            {
                c = max(c, 0.0);
                return lerp(pow((c + 0.055) / 1.055, 2.4), c / 12.92, step(c, 0.04045));
            }

            // the HSV hue in degrees, 0..360 (n56-colour.py's hsv())
            float Hue57(float3 c, float mx, float d)
            {
                float h;
                if (c.r >= mx) h = (c.g - c.b) / d;
                else if (c.g >= mx) h = 2.0 + (c.b - c.r) / d;
                else h = 4.0 + (c.r - c.g) / d;
                return frac(h / 6.0) * 360.0;
            }

            // 1 deep inside the arc [lo, hi] (wrapping), 0 outside, a smoothstep over f deg inside each edge (n57-v-cap.py's arc_weight)
            float ArcWeight57(float h, float lo, float hi, float f)
            {
                float span = fmod(hi - lo + 720.0, 360.0);
                float d = fmod(h - lo + 720.0, 360.0);
                if (d > span) return 0.0;
                float t = saturate(min(d, span - d) / max(f, 1e-4));
                return t * t * (3.0 - 2.0 * t);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float4 src = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, i.texcoord, 0);
                float cap = _Cap57.x;
                bool srgb = _Cap57.z > 0.5;
                float3 c = srgb ? ToSrgb57(src.rgb) : max(src.rgb, 0.0);
                float mx = max(c.r, max(c.g, c.b));
                float mn = min(c.r, min(c.g, c.b));
                float d = mx - mn;
                bool yOn = _Cap57Y.w > 0.5;
                float lo = min(cap, yOn ? _Cap57Y.x : 1.0);
                if (lo >= 1.0 || mx <= 1e-6 || d <= 1e-6) return (half4)src;
                float s = d / mx;
                if (s <= lo) return (half4)src;
                float h = Hue57(c, mx, d);
                float st = s;
                if (s > cap)
                {
                    float f = _Cap57.y;
                    float p = max(ArcWeight57(h, _Cap57A.x, _Cap57A.y, f), max(ArcWeight57(h, _Cap57A.z, _Cap57A.w, f), ArcWeight57(h, _Cap57B.x, _Cap57B.y, f)));
                    st = s - (1.0 - p) * (s - cap);
                }
                if (yOn && st > _Cap57Y.x && !(_Cap57YH.w > 0.0 && s >= _Cap57YH.w && mx >= _Cap57.w))
                {
                    // yellow56 (NIGHT57 V4): a DIM yellow (the arc _Cap57YH, value under _Cap57Y.z) toward _Cap57Y.x: yellow means parry, and the parry
                    // tells are bright emissives (V >= ~0.85): the mustard walls and ochre floors (V 0.2-0.5) lose the parry colour, the tells keep it.
                    // NIGHT59 V2: a yellow at or over the keep bound's saturation (_Cap57YH.w) is a tell whatever its value (the foundry a5 husk's
                    // parry halo S 0.99 at V 0.84, on dimYellow58.vFull; the lava edge S 0.64 and the column plate S 0.56 stay under the cap), when
                    // its value is at least _Cap57.w (dimYellow58.sKeepVMin: spire's near-black olive reads S 1.0 at V 0.18-0.19 and stays capped)
                    float wy = ArcWeight57(h, _Cap57YH.x, _Cap57YH.y, _Cap57YH.z) * (1.0 - smoothstep(_Cap57Y.y, _Cap57Y.z, mx));
                    st = st - wy * (st - _Cap57Y.x);
                }
                if (st >= s) return (half4)src;
                // toward the grey of the same luminance (Rec. 709 weights on the sRGB value, as the meter's greyscale): the greyscale frame is
                // unchanged and the hue kept; k solves HSV S(L + k (c - L)) = st (k = 1 when st = s, 0 < k < 1 below it)
                float L = dot(c, float3(0.2126, 0.7152, 0.0722));
                float k = saturate(st * L / max(d - st * (mx - L), 1e-6));
                float3 o = L + k * (c - L);
                if (srgb) o = ToLin57(o);
                return half4((half3)o, (half)src.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
