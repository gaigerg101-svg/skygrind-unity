// NIGHT48 builder F (foes48 with visor47, view rows; NIGHT48.md G13 / G14). visor47 (NIGHT47) gave the four BAND visors (the husk's slit,
// the tank's double slit, the charger's chevron, the warden's plate) the angular glasses with temple points; the three LENS visors (the
// swarmer's three lenses, the spitter's round lens, the drone's ring of lenses) "keep their shape here" (Mesh43.Visor47.cs), so G13's "the
// angular visor band with temple points on every kind" was not true for three kinds of seven. With foes48 on (and visor47's band on):
//   - the SWARMER: its three lenses set in a narrow angular band of three segments (the bridge segment and two side segments ending in
//     swept temple points; F-cards 5b "a narrow band with three slits cut in it"), built on its head's superellipsoid as Patch47 builds;
//   - the SPITTER: its round lens kept, set in a band that wraps its head to two temple points (the lens over the band's middle: "a round
//     lens inside a band frame with temple points");
//   - the DRONE: each of its three rim lenses set in a short angular brow round the rim, pointed at both ends (the flyer's lenses read as
//     three pointed visors round the saucer).
// One mesh per VISOR part (the part's one renderer: Models43Tests' budget kept); the band is built in the part's ANCHOR space (the head) and
// carried into the part's own space by the inverse of the part's rest transform. And G14's gait (F-cards 5c "legs on a 2-phase gait cycle tied
// to its speed (so it scuttles instead of sliding)"): Legs48(i), the swarmer's four leg spikes (Legs47's layout) in Legs48Phases poses of a
// diagonal-pairs gait (front-left + back-right swing forward and lift while the other pair swings back), picked by the distance it walked.
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class Mesh43
    {
        static readonly Dictionary<string, Mesh> cache48 = new Dictionary<string, Mesh>();

        /// <summary>The foes48 version of the lens visor `id` on `kind` (null: not a lens visor: keep visor47's / the base mesh). `partRest`: the
        /// part's rest transform in its anchor's space (Enemy40Parts: localPosition, rest rotation, rest scale).</summary>
        public static Mesh Visor48Of(int id, string kind, Matrix4x4 partRest)
        {
            // round 5 (the blind pair, DECIDE48 D-33: the drone OLD won 3-0, "pale overlapping patches that read as two meshes"): the drone's brows
            // (Brows48: 0.6 m arcs, 0.33 m tall, at the saucer's rim radius) stood half inside the saucer and flat beside it; the drone keeps
            // its ring of round lenses (the flyer's read: F-cards 5b "spitter / drone keep their round lens")
            if (id == Lenses3 && !UnityTuning.Visor48DroneBrows) return null;
            if (id != Eyes3 && id != Ball && id != Lenses3) return null;
            string key = kind + "/" + id;
            if (cache48.TryGetValue(key, out Mesh m)) return m;
            Matrix4x4 inv = partRest.inverse;
            float hm = UnityTuning.Visor47HeightMul, wm = UnityTuning.Visor47WrapMul, tm = UnityTuning.Visor47ThickMul;
            if (id == Eyes3)
                m = Combine("mesh43 visor48 " + kind, (Get(Eyes3), Matrix4x4.identity),
                    (Band48(HelmetE1, HelmetE2, UnityTuning.Visor48SwarmerLat, 0.05f * hm, 1.0f * wm, 0.06f * tm, -1f, -UnityTuning.Visor48SwarmerGapOut, true, false), inv),
                    (Band48(HelmetE1, HelmetE2, UnityTuning.Visor48SwarmerLat, 0.05f * hm, 1.0f * wm, 0.06f * tm, -UnityTuning.Visor48SwarmerGapIn, UnityTuning.Visor48SwarmerGapIn, false, false), inv),
                    (Band48(HelmetE1, HelmetE2, UnityTuning.Visor48SwarmerLat, 0.05f * hm, 1.0f * wm, 0.06f * tm, UnityTuning.Visor48SwarmerGapOut, 1f, false, true), inv));
            else if (id == Ball)
                m = Combine("mesh43 visor48 " + kind, (Get(Ball), Matrix4x4.identity),
                    (Band48(HelmetE1, HelmetE2, UnityTuning.Visor48SpitterLat, 0.06f * hm, 1.15f * wm, 0.07f * tm, -1f, 1f, true, true), inv));
            else
                m = Combine("mesh43 visor48 " + kind, (Get(Lenses3), Matrix4x4.identity), (Brows48(), Matrix4x4.identity));
            m.name = "mesh43 visor48 " + kind;
            cache48[key] = m;
            return m;
        }

        /// <summary>Patch47's band (the same temple point, V dip and crease rules) over su0..su1 of the band's width only (-1..1), with a temple
        /// point at its left (su0) and / or right (su1) end; an end without a point is cut square (a slit between segments).</summary>
        static Mesh Band48(float e1, float e2, float lat0, float halfLat, float halfLon, float thick, float su0, float su1, bool tipL, bool tipR)
        {
            const int nu = 16, nv = 8;
            var pos = new Vector3[nu + 1, nv + 1];
            var refN = new Vector3[nu + 1, nv + 1];
            float tipShare = UnityTuning.Visor47TipShare * 2f / Mathf.Max(0.1f, su1 - su0); // the point's share of THIS segment (the whole band's share in su)
            for (int i = 0; i <= nu; i++)
            {
                float su = Mathf.Lerp(su0, su1, (float)i / nu);
                float s01 = (float)i / nu; // 0 at su0, 1 at su1
                float lon = Mathf.PI / 2 + su * halfLon;
                float taper = 1f;
                if (tipL && s01 < tipShare) taper = Mathf.Min(taper, s01 / tipShare);
                if (tipR && 1f - s01 < tipShare) taper = Mathf.Min(taper, (1f - s01) / tipShare);
                float rise = (1 - taper) * UnityTuning.Visor47TipRise * halfLat;
                float au = Mathf.Abs(su);
                float dipK = Mathf.Clamp01(1 - au / UnityTuning.Visor47DipWidth);
                dipK = dipK * dipK * (3 - 2 * dipK);
                for (int j = 0; j <= nv; j++)
                {
                    float sv = -1 + 2f * j / nv;
                    float edgeDrop = sv > 0 ? UnityTuning.Visor47DipTop * sv : UnityTuning.Visor47DipBottom * -sv;
                    float lat = lat0 + rise + sv * halfLat * Mathf.Max(0.02f, taper) - dipK * edgeDrop * halfLat;
                    Vector3 p = SuperPoint(e1, e2, lat, lon);
                    Vector3 nn = SuperNormal(e1, e2, lat, lon);
                    float crease = 1f - Mathf.Abs(sv);
                    pos[i, j] = p + nn * (0.012f + thick * crease * (0.35f + 0.65f * taper));
                    refN[i, j] = nn;
                }
            }
            return Grid(pos, refN, nu, nv);
        }

        /// <summary>The drone's three brows: round the rim (Lenses3's space: lenses at radius 0.5, every 120 deg), each an arc of
        /// +-Visor48BrowSpanRad at radius Visor48BrowR, Visor48BrowHalfH tall, pointed at both ends (the ends sweep up), its top edge dipping at
        /// the lens (a V over each lens).</summary>
        static Mesh Brows48()
        {
            const int nu = 14, nv = 6;
            var parts = new (Mesh, Matrix4x4)[3];
            float span = UnityTuning.Visor48BrowSpanRad, R = UnityTuning.Visor48BrowR, hh = UnityTuning.Visor48BrowHalfH;
            for (int k = 0; k < 3; k++)
            {
                float a0 = k * 120 * Mathf.Deg2Rad;
                var pos = new Vector3[nu + 1, nv + 1];
                var refN = new Vector3[nu + 1, nv + 1];
                for (int i = 0; i <= nu; i++)
                {
                    float su = -1 + 2f * i / nu;
                    float a = a0 + su * span;
                    float taper = Mathf.Clamp01((1 - Mathf.Abs(su)) / 0.35f);
                    float rise = (1 - taper) * hh * 0.8f;
                    float dip = Mathf.Clamp01(1 - Mathf.Abs(su) / 0.3f);
                    for (int j = 0; j <= nv; j++)
                    {
                        float sv = -1 + 2f * j / nv;
                        float y = rise + sv * hh * Mathf.Max(0.03f, taper) - (sv > 0 ? dip * 0.35f * hh * sv : 0);
                        float rr = R + 0.02f * (1f - Mathf.Abs(sv)); // the crease: a ridge along the brow
                        var n = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
                        pos[i, j] = new Vector3(n.x * rr, y, n.z * rr);
                        refN[i, j] = n;
                    }
                }
                parts[k] = (Grid(pos, refN, nu, nv), Matrix4x4.identity);
            }
            return Combine("", parts);
        }

        // ---- G14: the swarmer's gait (foes48) ----
        static Mesh[] legs48;

        /// <summary>One of Legs48Phases poses of the four-leg gait (0 = all four at rest, Legs47's own pose).</summary>
        public static Mesh Legs48(int phase)
        {
            int n = UnityTuning.Legs48Phases;
            if (legs48 == null) legs48 = new Mesh[n];
            phase = ((phase % n) + n) % n;
            if (legs48[phase] != null) return legs48[phase];
            Mesh spike = Get(Spike);
            var parts = new (Mesh, Matrix4x4)[4];
            int k = 0;
            float p = (float)phase / n * 2f * Mathf.PI;
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                {
                    // the diagonal pairs: (left, front) + (right, back) in one phase, the other two half a cycle later
                    bool pairA = (sx < 0) == (sz > 0);
                    float w = Mathf.Sin(p + (pairA ? 0f : Mathf.PI));
                    float swing = UnityTuning.Legs48SwingDeg * w; // + forward (+z) at the tip
                    float lift = Mathf.Max(0f, w) * UnityTuning.Legs48Lift; // the leg swinging forward is off the ground
                    var corner = new Vector3(sx * UnityTuning.Swarmer47LegX, lift, sz * UnityTuning.Swarmer47LegZ);
                    Quaternion r = Quaternion.Euler(-sz * UnityTuning.Swarmer47LegSplayDeg, 0, sx * UnityTuning.Swarmer47LegSplayDeg) * Quaternion.Euler(180, 0, 0);
                    // the swing about the leg's root (the spike's base, +0.5 up in its tip-down space) across the body's x axis: the tip moves along z
                    var root = new Vector3(corner.x, corner.y + 0.5f, corner.z);
                    Matrix4x4 m = Matrix4x4.Translate(root) * Matrix4x4.Rotate(Quaternion.Euler(-swing, 0, 0)) * Matrix4x4.Translate(-root)
                        * Matrix4x4.TRS(corner, r, new Vector3(UnityTuning.Swarmer47LegW, 1f, UnityTuning.Swarmer47LegW));
                    parts[k++] = (spike, m);
                }
            legs48[phase] = Combine("mesh43 swarmer legs 48 " + phase, parts);
            return legs48[phase];
        }

        /// <summary>Is `m` one of the gait's poses (Legs47 keeps its put-back mesh clean of them).</summary>
        public static bool IsLegs48(Mesh m)
        {
            if (m == null || legs48 == null) return false;
            foreach (Mesh l in legs48) if (l == m) return true;
            return false;
        }
    }
}
