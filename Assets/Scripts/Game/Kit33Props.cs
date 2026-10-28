// NIGHT33 builder K (design.n33.kit33; K1-arena-kit.md section 6): the arena kit's view. PropsView29 hands it every prop row whose kind is a
// kit kind (the dressing pass Tools/night/dress33.cjs writes them, `decor: true`, no collider) or, with the kit33 view row on, one of the four
// kinds NIGHT32's plans used that PropsView29 drew as crates (heatVent, checkpoint, plinth, slot: A3 section 5). Everything is built from boxes
// in the row's own frame (`at` the centre, `size` [w, h, d], `yawDeg`) and stays INSIDE that box, so the plan's AABB (the collider of a cover
// piece, the checker's extents) is the truth. Two layers: the stone (the Stone shader: vertex colours with a dark-at-the-base, light-at-the-top
// gradient, so a wall piece never reads as one flat colour) and the glows (the neon material, one per colour: lamps, lintels, edge lines,
// inlays, the danger tone on hazard stripes and lava channels). No light here: builder L mounts the lights (lamp posts, lamps, kiln mouths).
// Colours: the theme's dusk tones (UnityTuning.Kit33*), never cyan (player-usable) except the checkpoint's band, red-orange only on hazards.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT33 round 3: a vertical interval cut by clear bands, in one bounded pass (pure; Kit33Props.S and Kit33CutTests).</summary>
    public static class Kit33Cut
    {
        /// <summary>The parts of [b0, b1] outside every band [lo, hi] of `bands` ([lo, hi, lo, hi, ...]; a pair with hi &lt;= lo is ignored), each at
        /// least `minLen` long, bottom to top. A band that only touches the interval (within Kit33CutTouch) leaves it whole, so a part whose edge
        /// lies on a band by float rounding is never cut again. Each band visits each current part once: at most (bands / 2 + 1) parts, no recursion.</summary>
        public static List<(float lo, float hi)> Parts(float b0, float b1, float[] bands, float minLen)
        {
            var parts = new List<(float lo, float hi)> { (b0, b1) };
            if (bands == null) return parts;
            float touch = UnityTuning.Kit33CutTouch;
            for (int k = 0; k + 1 < bands.Length; k += 2)
            {
                float lo = bands[k], hi = bands[k + 1];
                if (!(hi > lo)) continue;
                var next = new List<(float lo, float hi)>(parts.Count + 1);
                foreach (var (a, b) in parts)
                {
                    if (hi <= a + touch || lo >= b - touch) { next.Add((a, b)); continue; } // the band misses (or only touches) this part
                    if (lo - a >= minLen) next.Add((a, lo));
                    if (b - hi >= minLen) next.Add((hi, b));
                }
                parts = next;
            }
            return parts;
        }
    }

    public sealed class Kit33Props
    {
        /// <summary>The kit's own kinds (K1's catalogue as dress33 names them).</summary>
        public static readonly HashSet<string> Kinds = new HashSet<string>
        {
            "skirt", "cornice", "pilaster", "rib", "doorFrame", "lintelLight", "truss", "pipes", "chain", "lamp", "wallPipe", "recess", "wash",
            "inlay", "grate", "hazardStripe", "chimney", "banner", "kilnArch", "ingots", "lavaChannel", "stalactite", "crystal", "drift",
            "pylon", "span", "lampPost", "halo", "fin", "hero", "debris", "scorch", "edgeLight", "crateStack", "lowWall", "coverPillar",
        };

        /// <summary>The kinds NIGHT32's plans used that PropsView29 drew as crates (A3): drawn as themselves with the kit33 view row on.</summary>
        public static readonly HashSet<string> Legacy = new HashSet<string> { "heatVent", "checkpoint", "plinth", "slot" };

        // round 2 (the lead: draw cost): one stone mesh per ROOM and one glow mesh per room + colour, so a room behind the camera is culled whole;
        // a level's decor costs (rooms x (1 + its glow colours)) draws, never one per row
        readonly Dictionary<string, MeshKit> stones = new Dictionary<string, MeshKit>();
        readonly Dictionary<string, (MeshKit kit, string name, Color color, float scale)> glows = new Dictionary<string, (MeshKit, string, Color, float)>();
        MeshKit stone;
        string room = "";
        float[] gaps = new float[0]; // the row's clear bands [lo, hi, lo, hi, ...] (y, absolute): no box covers them (a wall-ride strip, L's sockets)
        public int objects, vertices, boxes; // what Build made (the kit33 reel prints them)
        readonly Color dark, mid, light, metal, crystal, snow, lamp, accent, danger, usable, decal;
        public readonly string theme;
        public int drawn;

        public Kit33Props(GameConfig cfg, string levelId)
        {
            theme = ThemeOf(levelId);
            uint d0, m0, l0, t0;
            switch (theme)
            {
                case "kiln": d0 = UnityTuning.Kit33KilnDark; m0 = UnityTuning.Kit33KilnMid; l0 = UnityTuning.Kit33KilnLight; t0 = UnityTuning.Kit33KilnMetal; break;
                case "ice": d0 = UnityTuning.Kit33IceDark; m0 = UnityTuning.Kit33IceMid; l0 = UnityTuning.Kit33IceLight; t0 = UnityTuning.Kit33IceMetal; break;
                case "span": d0 = UnityTuning.Kit33SpanDark; m0 = UnityTuning.Kit33SpanMid; l0 = UnityTuning.Kit33SpanLight; t0 = UnityTuning.Kit33SpanMetal; break;
                case "spire": d0 = UnityTuning.Kit33SpireDark; m0 = UnityTuning.Kit33SpireMid; l0 = UnityTuning.Kit33SpireLight; t0 = UnityTuning.Kit33SpireMetal; break;
                default: d0 = UnityTuning.Kit33FoundryDark; m0 = UnityTuning.Kit33FoundryMid; l0 = UnityTuning.Kit33FoundryLight; t0 = UnityTuning.Kit33FoundryMetal; break;
            }
            dark = Look.Hex(d0); mid = Look.Hex(m0); light = Look.Hex(l0); metal = Look.Hex(t0);
            crystal = Look.Hex(UnityTuning.Kit33IceCrystalHex); snow = Look.Hex(UnityTuning.Kit33SnowHex);
            lamp = Look.Hex(UnityTuning.Kit33LampHex); accent = Look.Hex(UnityTuning.Kit33AccentHex);
            danger = Look.C(cfg, "colors.enemy"); usable = Look.C(cfg, "colors.usable"); decal = Look.C(cfg, "colors.decal");
            // NIGHT34 A (design.n34.calm34): the calm filter (Calm34): a dress33 row whose kind is not kept is consumed and not drawn
            calmKeep = N34Fixes.ViewRowOn(cfg, "calm34") ? Calm34.KeepKinds(cfg) : null;
        }

        /// <summary>NIGHT34 A: design.n34.calm34's keep set (null: the row off, every row drawn as NIGHT33).</summary>
        readonly HashSet<string> calmKeep;

        /// <summary>NIGHT34 A: dress33 rows calm34 cut (consumed: PropsView29 neither draws them nor falls back to a crate).</summary>
        public int calmCut;

        /// <summary>The level's theme from its id (n32-foundry, n33-kiln, ...): foundry, kiln, ice, span, spire.</summary>
        public static string ThemeOf(string id)
        {
            id = id ?? "";
            if (id.Contains("kiln")) return "kiln";
            if (id.Contains("ice")) return "ice";
            if (id.Contains("span")) return "span";
            if (id.Contains("spire")) return "spire";
            return "foundry";
        }

        /// <summary>A stable per-row number (FNV-1a of the id): the view's only variation (debris turns, crystal tilts), the same every boot.</summary>
        public static uint Seed(string id)
        {
            uint h = 2166136261;
            foreach (char ch in id ?? "") { h ^= ch; h *= 16777619; }
            return h;
        }

        static float Unit(uint s, int k) { uint x = s ^ (uint)(k * 0x9E3779B9); x ^= x >> 15; x *= 0x2C1B3C6D; x ^= x >> 12; x *= 0x297A2D39; x ^= x >> 15; return (x & 0xFFFFFF) / 16777216f; }

        // the row's frame while drawing
        Vector3 o, R, F;
        Quaternion q;

        MeshKit Glow(string name, Color c, float scale)
        {
            string key = room + "|" + name;
            if (!glows.TryGetValue(key, out var g)) glows[key] = g = (new MeshKit(), name, c, scale);
            return g.kit;
        }

        /// <summary>A box in the row's frame: centre (x, y, z) local, half sizes; a vertical gradient on the sides (dark base, light top), a lifted lid.</summary>
        void S(float x, float y, float z, float hx, float hy, float hz, Color c, float turnDeg = 0)
        {
            Quaternion t = turnDeg != 0 ? q * Quaternion.Euler(0, turnDeg, 0) : q;
            Vector3 r = t * Vector3.right, f = t * Vector3.forward, u = Vector3.up;
            Vector3 cc = o + R * x + Vector3.up * y + F * z;
            if (gaps.Length >= 2)
            {
                // NIGHT33 round 3: the clear bands cut this box in ONE pass, no recursion (n33-r4 overflowed the stack when a cut part's edge landed on a
                // band by float rounding and re-entered S forever; the lead's 5 mm hotfix de73295b). Kit33Cut.Parts returns at most bands + 1 pieces.
                float b0 = cc.y - hy, b1 = cc.y + hy;
                List<(float lo, float hi)> parts = Kit33Cut.Parts(b0, b1, gaps, UnityTuning.Kit33CutMin);
                if (!(parts.Count == 1 && parts[0].lo == b0 && parts[0].hi == b1))
                {
                    foreach (var (plo, phi) in parts) Emit(cc + Vector3.up * ((plo + phi) * 0.5f - cc.y), r, f, u, hx, (phi - plo) * 0.5f, hz, c);
                    return;
                }
            }
            Emit(cc, r, f, u, hx, hy, hz, c);
        }

        /// <summary>One box (no band test): world centre, its axes, half sizes; the vertical gradient and the lifted lid.</summary>
        void Emit(Vector3 cc, Vector3 r, Vector3 f, Vector3 u, float hx, float hy, float hz, Color c)
        {
            boxes++;
            Color lo = c * UnityTuning.Kit33GradBottom, hi = c * UnityTuning.Kit33GradTop;
            lo.a = hi.a = 1;
            Color lid = Color.Lerp(hi, Color.white, UnityTuning.Kit33TopLift);
            Vector3 P(int sx, int sy, int sz) => cc + r * (sx * hx) + u * (sy * hy) + f * (sz * hz);
            stone.Quad(P(-1, 1, -1), P(1, 1, -1), P(1, 1, 1), P(-1, 1, 1), u, lid);
            stone.Quad(P(-1, -1, -1), P(-1, -1, 1), P(1, -1, 1), P(1, -1, -1), -u, lo);
            stone.QuadGradient(P(-1, -1, -1), P(1, -1, -1), P(1, 1, -1), P(-1, 1, -1), -f, lo, lo, hi, hi);
            stone.QuadGradient(P(-1, -1, 1), P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), f, lo, lo, hi, hi);
            stone.QuadGradient(P(-1, -1, -1), P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), -r, lo, lo, hi, hi);
            stone.QuadGradient(P(1, -1, -1), P(1, -1, 1), P(1, 1, 1), P(1, 1, -1), r, lo, lo, hi, hi);
        }

        /// <summary>A glowing box in the row's frame (one neon material per name).</summary>
        void G(string name, Color c, float scale, float x, float y, float z, float hx, float hy, float hz)
        {
            Vector3 cc = o + R * x + Vector3.up * y + F * z;
            for (int k = 0; k + 1 < gaps.Length; k += 2) if (gaps[k + 1] > gaps[k] && cc.y - hy < gaps[k + 1] && cc.y + hy > gaps[k]) return; // a glow never crosses a clear band
            boxes++;
            LightKit29.OrientedBox(Glow(name, c, scale), cc, R, Vector3.up, F, hx, hy, hz, Color.white);
        }

        /// <summary>Draws one kit row (a kit kind, or a legacy kind with the kit33 row on). False: not the kit's (PropsView29 draws it).</summary>
        public bool Draw(string id, string kind, Vector3 at, Quaternion yaw, float w, float h, float d) => Draw(id, kind, at, yaw, w, h, d, "", null);

        /// <summary>Round 2: with the row's room (its own meshes) and its clear bands [lo, hi, lo, hi, ...] (y; null: none).</summary>
        public bool Draw(string id, string kind, Vector3 at, Quaternion yaw, float w, float h, float d, string roomId, float[] bands)
        {
            if (!Kinds.Contains(kind) && !Legacy.Contains(kind)) return false;
            if (!Calm34.Keep(calmKeep, id, kind)) { calmCut++; return true; } // NIGHT34 A calm34: cut (only dress33's decor rows; never a collider)
            room = roomId ?? "";
            if (!stones.TryGetValue(room, out stone)) stones[room] = stone = new MeshKit();
            gaps = bands ?? new float[0];
            o = at; q = yaw; R = yaw * Vector3.right; F = yaw * Vector3.forward;
            float hw = w * 0.5f, hh = h * 0.5f, hd = d * 0.5f;
            uint seed = Seed(id);
            switch (kind)
            {
                case "skirt": S(0, 0, 0, hw, hh, hd, dark); break;
                case "rib": S(0, 0, 0, hw, hh, hd, mid); S(0, hh - 0.2f, 0, hw, 0.15f, hd, light); break;
                case "pilaster":
                    S(0, 0, 0, hw * 0.85f, hh, hd * 0.85f, mid);
                    S(0, -hh + 0.3f, 0, hw, 0.3f, hd, dark);              // a base
                    S(0, hh - 0.25f, 0, hw, 0.25f, hd, light);            // a capital
                    break;
                case "wallPipe": S(0, 0, 0, hw, hh, hd, metal); for (float x = -hw + 1f; x < hw; x += 2.5f) S(x, 0, 0, 0.08f, hh * 1.0f, hd, light); break;
                case "pylon": S(0, 0, 0, hw, hh, hd, metal); S(0, hh - 0.3f, 0, hw, 0.3f, hd, light); break;
                case "fin": S(0, 0, 0, hw, hh, hd, light); break;
                case "recess":
                    S(0, 0, 0, hw, hh, hd, dark);                          // the inset slab (darker than the wall)
                    S(0, hh - 0.06f, 0, hw, 0.06f, hd * 1.0f, light);      // a lit top edge
                    break;
                case "wash": S(0, 0, 0, hw, hh, hd, mid); break;             // a full-height gradient skin (dark at the floor, light up high)
                case "cornice":
                {
                    float up = h * UnityTuning.Kit33CorniceStep;
                    S(0, -hh + (h - up) * 0.5f, 0, hw, (h - up) * 0.5f, hd * 0.6f, mid); // centred: the row's frame has no "wall side"
                    S(0, hh - up * 0.5f, 0, hw, up * 0.5f, hd, light);
                    break;
                }
                case "doorFrame":
                {
                    float j = Mathf.Min(UnityTuning.Kit33DoorJamb, hw * 0.4f);
                    S(-hw + j * 0.5f, 0, 0, j * 0.5f, hh, hd, mid);
                    S(hw - j * 0.5f, 0, 0, j * 0.5f, hh, hd, mid);
                    S(0, hh - j * 0.5f, 0, hw, j * 0.5f, hd, light);
                    break;
                }
                case "lintelLight": G("kit lamp", lamp, UnityTuning.Kit33EmissiveScale, 0, 0, 0, hw, hh, hd); break;
                case "edgeLight": G("kit lamp", lamp, UnityTuning.Kit33EmissiveScale, 0, 0, 0, hw, hh, hd); break;
                case "inlay": G("kit inlay", accent, UnityTuning.Kit33InlayScale, 0, 0, 0, hw, hh, hd); break;
                case "lavaChannel": G("kit danger", danger, UnityTuning.Kit33DangerScale, 0, 0, 0, hw, hh, hd); break;
                case "truss":
                {
                    float c = Mathf.Min(UnityTuning.Kit33TrussChord * 0.5f, hh * 0.3f);
                    S(0, hh - c, hd - c, hw, c, c, metal); S(0, hh - c, -hd + c, hw, c, c, metal);
                    S(0, -hh + c, 0, hw, c, c, metal);
                    for (float x = -hw + c; x <= hw - c + 1e-3f; x += UnityTuning.Kit33TrussBay)
                    {
                        S(x, 0, 0, c * 0.7f, hh, c * 0.7f, dark);
                        S(x, hh - c, 0, c * 0.7f, c * 0.7f, hd, dark);
                    }
                    break;
                }
                case "pipes":
                    S(0, hh * 0.5f, -hd * 0.6f, hw, hh * 0.45f, hd * 0.35f, metal);
                    S(0, -hh * 0.4f, 0, hw, hh * 0.5f, hd * 0.3f, mid);
                    S(0, hh * 0.3f, hd * 0.65f, hw, hh * 0.3f, hd * 0.3f, light);
                    for (float x = -hw + 1.5f; x < hw; x += 3f) S(x, 0, 0, 0.1f, hh, hd, dark); // the clamps
                    break;
                case "chain":
                {
                    float link = UnityTuning.Kit33ChainLink;
                    int n = Mathf.Max(1, Mathf.FloorToInt(h / link));
                    for (int i = 0; i < n; i++)
                    {
                        float y = hh - (i + 0.5f) * link;
                        bool turn = (i & 1) == 1;
                        S(0, y, 0, turn ? hw * 0.25f : hw, link * 0.45f, turn ? hd : hd * 0.25f, metal);
                    }
                    break;
                }
                case "lamp":
                    S(0, hh * 0.4f, 0, hw, hh * 0.6f, hd, metal);                                  // the housing
                    G("kit lamp", lamp, UnityTuning.Kit33EmissiveScale, 0, -hh * 0.4f, 0, hw * 0.7f, hh * 0.4f, hd * 0.7f); // the core (L hangs the light)
                    break;
                case "grate":
                {
                    S(0, -hh * 0.5f, 0, hw, hh * 0.5f, hd, Color.Lerp(dark, Color.black, 0.5f));
                    float bar = UnityTuning.Kit33GrateBar * 0.5f;
                    for (float z = -hd + bar; z <= hd - bar + 1e-3f; z += UnityTuning.Kit33GrateGap) S(0, hh * 0.5f, z, hw, hh * 0.5f, bar, metal);
                    break;
                }
                case "hazardStripe":
                {
                    float p = UnityTuning.Kit33StripePitch;
                    int n = Mathf.Max(1, Mathf.RoundToInt(w / p));
                    float bw = w / n;
                    for (int i = 0; i < n; i++)
                    {
                        float x = -hw + (i + 0.5f) * bw;
                        if ((i & 1) == 0) G("kit danger", danger, UnityTuning.Kit33DangerScale, x, 0, 0, bw * 0.5f, hh, hd);
                        else S(x, 0, 0, bw * 0.5f, hh, hd, dark);
                    }
                    break;
                }
                case "chimney":
                {
                    float cap = Mathf.Min(UnityTuning.Kit33CapLift, h * 0.2f);
                    S(0, -cap * 0.5f, 0, hw * 0.85f, hh - cap * 0.5f, hd * 0.85f, dark);
                    S(0, hh - cap * 0.5f, 0, hw, cap * 0.5f, hd, metal);
                    S(0, hh * 0.2f, 0, hw * 0.9f, 0.2f, hd * 0.9f, mid); // a band
                    break;
                }
                case "banner":
                    S(0, hh - 0.05f, 0, hw, 0.05f, Mathf.Max(hd, 0.05f), metal);                 // the rod
                    S(0, -0.05f, 0, hw * 0.9f, hh - 0.05f, hd * 0.6f, Color.Lerp(mid, accent, 0.35f));
                    S(0, -hh * 0.3f, 0, hw * 0.92f, 0.12f, hd * 0.7f, accent * 0.8f);            // a stripe
                    break;
                case "kilnArch":
                {
                    float jw = w * 0.22f, top = h * 0.25f;
                    S(-hw + jw * 0.5f, 0, 0, jw * 0.5f, hh, hd, mid);
                    S(hw - jw * 0.5f, 0, 0, jw * 0.5f, hh, hd, mid);
                    S(0, hh - top * 0.5f, 0, hw, top * 0.5f, hd, light);
                    S(0, -top * 0.5f, 0, hw - jw, hh - top * 0.5f, hd * 0.3f, Color.Lerp(dark, Color.black, 0.4f)); // the mouth's back
                    G("kit mouth", Look.Hex(UnityTuning.FixtureAmberHex), UnityTuning.Kit33DangerScale, 0, -hh + 0.15f, 0, hw - jw, 0.12f, hd * 0.35f); // embers at its sill (amber, not red-orange)
                    break;
                }
                case "ingots":
                {
                    float iw = 0.4f, ih = 0.2f, id2 = 0.2f;
                    int nx = Mathf.Max(1, Mathf.FloorToInt(w / iw)), ny = Mathf.Max(1, Mathf.FloorToInt(h / ih));
                    Color brass = Color.Lerp(metal, accent, 0.45f);
                    for (int y = 0; y < ny; y++)
                        for (int x = 0; x < nx - y; x++)
                        {
                            float cx = -hw + (x + 0.5f + y * 0.5f) * iw;
                            for (float z = -hd + id2 * 0.5f; z <= hd - id2 * 0.5f + 1e-3f; z += id2 * 1.4f)
                                S(cx, -hh + (y + 0.5f) * ih, z, iw * 0.45f, ih * 0.45f, id2 * 0.45f, (x + y) % 2 == 0 ? brass : metal);
                        }
                    break;
                }
                case "stalactite":
                {
                    int n = 4;
                    float y = hh;
                    for (int i = 0; i < n; i++)
                    {
                        float k = 1f - i / (float)n;
                        float sh = h * (i == n - 1 ? 0.34f : 0.22f);
                        S(0, y - sh * 0.5f, 0, hw * k, sh * 0.5f, hd * k, crystal, Unit(seed, i) * 30f);
                        y -= sh;
                    }
                    break;
                }
                case "crystal":
                {
                    int n = 4 + (int)(Unit(seed, 0) * 3);
                    for (int i = 0; i < n; i++)
                    {
                        float bx = (Unit(seed, i + 1) - 0.5f) * w * 0.6f, bz = (Unit(seed, i + 9) - 0.5f) * d * 0.6f;
                        float sh = h * (0.4f + 0.6f * Unit(seed, i + 17)), s = Mathf.Min(hw, hd) * (0.25f + 0.2f * Unit(seed, i + 23));
                        S(bx, -hh + sh * 0.5f, bz, s, sh * 0.5f, s, crystal, Unit(seed, i + 31) * 90f);
                    }
                    break;
                }
                case "drift":
                    for (int i = 0; i < 3; i++) { float k = 1f - i * 0.3f, sh = h / 3f; S(-hw * (1 - k) * 0.5f, -hh + (i + 0.5f) * sh, -hd * (1 - k), hw * k, sh * 0.5f, hd * k, snow); }
                    break;
                case "span": S(0, 0, 0, hw, hh, hd, metal); break; // a suspension cable or tie (thin box)
                case "lampPost":
                    S(0, -0.25f, 0, Mathf.Min(0.08f, hw), hh - 0.25f, Mathf.Min(0.08f, hd), metal);
                    S(0, hh - 0.2f, 0, hw, 0.2f, hd, dark);
                    G("kit lamp", lamp, UnityTuning.Kit33EmissiveScale, 0, hh - 0.45f, 0, hw * 0.6f, 0.08f, hd * 0.6f); // the head's lens (L mounts the light)
                    S(0, -hh + 0.15f, 0, hw * 0.8f, 0.15f, hd * 0.8f, mid);                         // the foot
                    break;
                case "halo":
                {
                    float rad = Mathf.Min(hw, hd) - 0.2f;
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        float seg = 2f * Mathf.PI * rad / 8f * 0.38f;
                        S(Mathf.Cos(a) * rad, 0, Mathf.Sin(a) * rad, seg, hh, 0.15f, light, -a * Mathf.Rad2Deg + 90f);
                    }
                    break;
                }
                case "hero": Hero(seed, w, h, d); break;
                case "debris":
                {
                    int n = 3 + (int)(Unit(seed, 0) * 3);
                    for (int i = 0; i < n; i++)
                    {
                        float s = 0.1f + 0.2f * Unit(seed, i + 3);
                        float bx = (Unit(seed, i + 7) - 0.5f) * (w - 2 * s), bz = (Unit(seed, i + 13) - 0.5f) * (d - 2 * s);
                        S(bx, -hh + s * 0.6f, bz, s, s * 0.6f, s * 0.8f, i % 2 == 0 ? dark : mid, Unit(seed, i + 19) * 90f);
                    }
                    break;
                }
                case "scorch": S(0, 0, 0, hw, hh, hd, decal); break;
                case "crateStack":
                {
                    int n = Mathf.Max(1, Mathf.RoundToInt(h / 1.2f));
                    float ch = h / n;
                    for (int i = 0; i < n; i++)
                    {
                        float k = i == 0 ? 1f : 0.85f;
                        S(0, -hh + (i + 0.5f) * ch, 0, hw * k, ch * 0.5f, hd * k, i % 2 == 0 ? mid : metal, i == 0 ? 0 : (Unit(seed, i) - 0.5f) * 10f);
                        S(0, -hh + (i + 0.5f) * ch, 0, hw * k + 0.01f, 0.06f, hd * k + 0.01f, light, i == 0 ? 0 : (Unit(seed, i) - 0.5f) * 10f); // a band
                    }
                    break;
                }
                case "lowWall": S(0, -0.06f, 0, hw * 0.96f, hh - 0.06f, hd * 0.9f, mid); S(0, hh - 0.06f, 0, hw, 0.06f, hd, light); break;
                case "coverPillar":
                    S(0, 0, 0, hw * 0.85f, hh, hd * 0.85f, mid);
                    S(0, -hh + 0.2f, 0, hw, 0.2f, hd, dark);
                    S(0, hh - 0.15f, 0, hw, 0.15f, hd, light);
                    break;
                // --- NIGHT32's kinds drawn as themselves (A3: they fell to the crate default) ---
                case "heatVent":
                {
                    S(0, -hh * 0.3f, 0, hw, hh * 0.7f, hd, metal);                              // the frame
                    G("kit danger", danger, UnityTuning.Kit33DangerScale, 0, hh * 0.45f, 0, hw * 0.8f, hh * 0.05f, hd * 0.8f); // the throat glow (a hazard: red-orange)
                    float bar = UnityTuning.Kit33GrateBar * 0.5f;
                    for (float z = -hd + 0.2f; z <= hd - 0.2f + 1e-3f; z += UnityTuning.Kit33GrateGap) S(0, hh - 0.05f, z, hw * 0.85f, 0.05f, bar, dark);
                    break;
                }
                case "checkpoint":
                    S(0, -hh * 0.2f, 0, hw * 0.5f, hh * 0.8f, hd * 0.5f, mid);                       // a post
                    G("kit usable", usable, UnityTuning.Kit33EmissiveScale, 0, hh * 0.75f, 0, hw * 0.55f, hh * 0.08f, hd * 0.55f); // the band (player-usable: cyan)
                    S(0, -hh + 0.1f, 0, hw, 0.1f, hd, dark);
                    break;
                case "plinth":
                    S(0, -hh * 0.5f, 0, hw, hh * 0.5f, hd, dark);
                    S(0, hh * 0.5f, 0, hw * 0.8f, hh * 0.5f, hd * 0.8f, light);
                    break;
                case "slot":
                    S(0, 0, 0, hw, hh, hd, Color.Lerp(dark, Color.black, 0.4f));
                    G("kit lamp", lamp, UnityTuning.Kit33EmissiveScale * 0.6f, 0, hh - 0.02f, 0, hw, 0.02f, hd);
                    break;
            }
            drawn++;
            return true;
        }

        /// <summary>The room's landmark (K1 #29): stacked tiers that narrow upward, a brass band, the theme's crown. 6-20 boxes inside the row's box.</summary>
        void Hero(uint seed, float w, float h, float d)
        {
            float hw = w * 0.5f, hh = h * 0.5f, hd = d * 0.5f;
            if (h < UnityTuning.Kit33FriezeBelow)
            {
                // round 2: a low area's landmark is a frieze under its cornice (above 3 m): a long slab, three emblem blocks standing proud, a brass line
                S(0, 0, -hd * 0.3f, hw, hh, hd * 0.7f, mid);
                for (int i = -1; i <= 1; i++) S(i * hw * 0.62f, 0, 0, Mathf.Min(0.5f, hw * 0.12f), hh * 0.92f, hd, theme == "ice" ? crystal : light);
                G("kit accent", accent, UnityTuning.Kit33EmissiveScale, 0, -hh + 0.06f, hd * 0.45f, hw * 0.96f, 0.04f, hd * 0.3f);
                return;
            }
            int tiers = 5 + (int)(Unit(seed, 0) * 3);
            float y = -hh, th = h * 0.85f / tiers;
            for (int i = 0; i < tiers; i++)
            {
                float k = 1f - 0.55f * i / tiers;
                float turn = theme == "ice" || theme == "spire" ? (Unit(seed, i + 1) - 0.5f) * 20f : 0f;
                S(0, y + th * 0.5f, 0, hw * k * (theme == "ice" ? 0.7f : 1f), th * 0.5f, hd * k * (theme == "ice" ? 0.7f : 1f), theme == "ice" ? crystal : (i % 2 == 0 ? mid : light), turn);
                y += th;
            }
            float crown = h * 0.15f;
            switch (theme)
            {
                case "kiln": G("kit mouth", Look.Hex(UnityTuning.FixtureAmberHex), UnityTuning.Kit33EmissiveScale, 0, y + crown * 0.3f, 0, hw * 0.35f, crown * 0.3f, hd * 0.35f); break;
                case "spire":
                    for (int i = 0; i < 4; i++) { float a = i * Mathf.PI / 2f; S(Mathf.Cos(a) * hw * 0.4f, y + crown * 0.5f, Mathf.Sin(a) * hd * 0.4f, 0.12f, crown * 0.5f, 0.12f, light); }
                    break;
                default: S(0, y + crown * 0.5f, 0, hw * 0.2f, crown * 0.5f, hd * 0.2f, metal); break;
            }
            G("kit accent", accent, UnityTuning.Kit33EmissiveScale, 0, -hh + h * 0.6f, 0, hw * 0.75f + 0.02f, 0.08f, hd * 0.75f + 0.02f); // the band
        }

        /// <summary>Emits the stone mesh and one object per glow colour under `parent`.</summary>
        public void Build(Transform parent, SurvivalMaterials mats, float bright)
        {
            foreach (var kv in stones)
            {
                if (kv.Value.Vertices == 0) continue;
                vertices += kv.Value.Vertices;
                objects++;
                MeshKit.Object("Kit33 stone " + kv.Key, parent, kv.Value.Build("kit33 stone " + kv.Key), mats.stone, shadows: true);
            }
            var shared = new Dictionary<string, Material>(); // one material per glow colour (the SRP batcher batches the rooms' draws)
            foreach (var kv in glows)
            {
                if (!shared.TryGetValue(kv.Value.name, out Material m))
                {
                    shared[kv.Value.name] = m = new Material(mats.neon);
                    m.SetColor("_BaseColor", kv.Value.color * bright * kv.Value.scale);
                }
                vertices += kv.Value.kit.Vertices;
                objects++;
                MeshKit.Object("Kit33 " + kv.Key, parent, kv.Value.kit.Build("kit33 " + kv.Key), m, shadows: false);
            }
        }
    }
}
