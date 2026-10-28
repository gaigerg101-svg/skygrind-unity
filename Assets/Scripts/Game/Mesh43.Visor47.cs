// NIGHT47 builder F (design.n47.visor47, a view row; NIGHT47.md G13; GG 2026-10-18: "the visors on enemies should be redesigned to look sharper and
// more angular ... and they should be slightly thicker and more like futuristic glasses"). An original design (F-cards 5b; not a copy of any
// character): the same visor band family on every class, built like Mesh43.Patch but:
//   - Visor47HeightMul x today's height, wrapping a little further round the head (Visor47WrapMul x the half-width);
//   - TEMPLE POINTS: over the last Visor47TipShare of each side the band's top and bottom converge to a tip that sweeps back and up;
//   - a V DIP at the bridge: the top edge drops Visor47DipTop x the half-height at the centre line, the bottom edge Visor47DipBottom;
//   - TWO FACETED PLANES meeting at a horizontal crease (a ridge across the band instead of a pillow: the crease catches a highlight line);
//   - thicker (Visor47ThickMul x today's raise).
// The class read stays (DESIGN-RULES E-2): the slit's proportions on the husk, two bands on the tank, the chevron's deep V on the charger, the
// warden's tall plate; the round lenses (swarmer, spitter, drone) are not patches and keep their shape here. Off: today's meshes (Mesh43.Get).
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>G13's meter (F-cards 5b: "the visor's projected height in px at 30 m at 1080p"): while a reel records, every drawn models43 VISOR
    /// part's world bounds this frame by kind (Enemy40Parts.Pose notes them); the reel projects them. Nothing is kept outside a recording.</summary>
    public static class Visor47Probe
    {
        public static bool Recording;
        static int frame = -1;
        public static readonly System.Collections.Generic.List<(string kind, Bounds b)> Seen = new System.Collections.Generic.List<(string, Bounds)>();

        public static void Note(string kind, Bounds b)
        {
            if (frame != Time.frameCount) { frame = Time.frameCount; Seen.Clear(); }
            Seen.Add((kind, b));
        }

        /// <summary>The visor of `kind` nearest `near` (world) drawn on the last frame: its height on screen in px at 1080p (8 corners projected), or -1.</summary>
        public static float PxNear(Camera cam, string kind, Vector3 near)
        {
            if (cam == null) return -1;
            float best = float.MaxValue, px = -1;
            foreach (var (k, b) in Seen)
            {
                if (k != kind) continue;
                float d = (b.center - near).sqrMagnitude;
                if (d >= best) continue;
                float lo = float.MaxValue, hi = float.MinValue;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 s = cam.WorldToScreenPoint(c);
                    if (s.z <= 0) continue;
                    lo = Mathf.Min(lo, s.y);
                    hi = Mathf.Max(hi, s.y);
                }
                if (hi < lo) continue;
                best = d;
                px = (hi - lo) * 1080f / Mathf.Max(1, Screen.height);
            }
            return px;
        }
    }

    /// <summary>visor47's switch (Enemy40Parts swaps its VISOR parts' meshes when it changes; the a847 reel forces it per pass).</summary>
    public static class Visor47
    {
        public static bool? Force;
        static Skygrind.Core.GameConfig readFor;
        static bool on;

        public static bool On(Skygrind.Core.GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = Skygrind.Core.NightRows.ViewOn(cfg, "visor47"); }
            return on;
        }
    }

    public static partial class Mesh43
    {
        static readonly Mesh[] cache47 = new Mesh[Count];

        /// <summary>The visor47 version of visor mesh `id` (null: `id` is not a band visor, keep the mesh).</summary>
        public static Mesh Visor47Of(int id)
        {
            if (id < 0 || id >= Count) return null;
            if (id != VisorSlit && id != VisorChevron && id != VisorPlate && id != VisorSlit2) return null;
            Mesh m = cache47[id];
            if (m == null)
            {
                m = Build47(id);
                m.name = "mesh43 visor47 " + id;
                cache47[id] = m;
            }
            return m;
        }

        static Mesh Build47(int id)
        {
            float hm = UnityTuning.Visor47HeightMul, wm = UnityTuning.Visor47WrapMul, tm = UnityTuning.Visor47ThickMul;
            switch (id)
            {
                case VisorSlit: return Patch47(HelmetE1, HelmetE2, 0.04f, 0.05f * hm, 1.15f * wm, 0f, 0.07f * tm, UnityTuning.Visor47DipTop, UnityTuning.Visor47DipBottom);
                case VisorChevron: return Patch47(BrowE1, BrowE2, -0.16f, 0.13f * hm, 1.0f * wm, 0.42f, 0.09f * tm, UnityTuning.Visor47DipTop * 2f, UnityTuning.Visor47DipBottom * 2f); // the charger: the V deepened (its chevron brow)
                case VisorPlate: return Patch47(HelmetE1, HelmetE2, -0.02f, 0.36f * 1.0f, 0.82f * wm, 0f, 0.06f * tm, UnityTuning.Visor47DipTop * 0.5f, UnityTuning.Visor47DipBottom * 0.5f); // the warden: already tall
                default:
                    return Combine("", (Patch47(HelmetE1, HelmetE2, 0.2f, 0.07f * hm, 1.05f * wm, 0f, 0.06f * tm, UnityTuning.Visor47DipTop, UnityTuning.Visor47DipBottom), Matrix4x4.identity),
                        (Patch47(HelmetE1, HelmetE2, -0.08f, 0.07f * hm, 1.05f * wm, 0f, 0.06f * tm, UnityTuning.Visor47DipTop, UnityTuning.Visor47DipBottom), Matrix4x4.identity));
            }
        }

        /// <summary>Patch's band with temple points, a V dip at the bridge and a creased (two-plane) section.</summary>
        static Mesh Patch47(float e1, float e2, float lat0, float halfLat, float halfLon, float vee, float thick, float dipTop, float dipBottom)
        {
            const int nu = 36, nv = 8;
            var pos = new Vector3[nu + 1, nv + 1];
            var refN = new Vector3[nu + 1, nv + 1];
            float tipShare = UnityTuning.Visor47TipShare;
            for (int i = 0; i <= nu; i++)
            {
                float su = -1 + 2f * i / nu; // -1..1 across
                float au = Mathf.Abs(su);
                float lon = Mathf.PI / 2 + su * halfLon;
                // the temple point: the band narrows to a tip over the last tipShare, its centre sweeping up by Visor47TipRise x halfLat
                float taper = au < 1 - tipShare ? 1f : Mathf.Max(0f, (1 - au) / tipShare);
                float rise = au < 1 - tipShare ? 0f : (1 - taper) * UnityTuning.Visor47TipRise * halfLat;
                // the V dip at the bridge (|su| < Visor47DipWidth)
                float dipK = Mathf.Clamp01(1 - au / UnityTuning.Visor47DipWidth);
                dipK = dipK * dipK * (3 - 2 * dipK);
                for (int j = 0; j <= nv; j++)
                {
                    float sv = -1 + 2f * j / nv; // -1..1 up
                    float edgeDrop = sv > 0 ? dipTop * sv : dipBottom * -sv;
                    float lat = lat0 + vee * Mathf.Abs(su * halfLon) + rise + sv * halfLat * taper - dipK * edgeDrop * halfLat;
                    Vector3 p = SuperPoint(e1, e2, lat, lon);
                    Vector3 nn = SuperNormal(e1, e2, lat, lon);
                    float crease = 1f - Mathf.Abs(sv); // two planes meeting at the middle: a ridge, not a pillow
                    float bump = crease * Mathf.Pow(Mathf.Max(0, 1 - Mathf.Pow(au, 8)), 0.5f) * (0.35f + 0.65f * taper);
                    pos[i, j] = p + nn * (0.012f + thick * bump);
                    refN[i, j] = nn;
                }
            }
            return Grid(pos, refN, nu, nv);
        }
    }
}
