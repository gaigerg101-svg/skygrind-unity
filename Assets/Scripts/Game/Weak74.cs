// NIGHT74 builder B (slate B1, design.n74.weak74, a VIEW row: NightRows.ViewOn(cfg, "weak74") EVERY frame, never `enabled` written here; off: -skygrindRows72 weak74:0, Rows72.cs).
// RULE W (FORGE74 D-4, the lead's call on T-153, DECIDE73's question aged three waves): on a boss, cyan means "shoot here" and nothing else. gg-proxy's NIGHT73 close #2: THE CROWN's ring
// and spokes are cyan (crown37-idle 560-1520 x 345-1080) beside the player's cyan TORCH, THE WICK's core is a cyan cylinder, THE LOOM's knees are cyan spheres (bosses-d.png). GG
// 2026-10-20: "each boss should have its own distinct color palette"; 2026-10-22: "effect colores everything doenst feel alligned".
// What the row does (read from the code; cyan sites listed by file:line in Research/2026-10-27-wave74/B/card-weak74.md):
//   - bossColours63's weak parts (design.n63.bossColours63.numbers.parts: wick.core, crown.gems, crown.halo, loom.knees, loom.seams, loom.nails; THE KILN WYRM's and THE ICEBREAKER's are
//     left as they were) are routed to the boss's own identity colour (UnityTuning.N74*IdentityHex) at the site's own luminance while their boss is CLOSED, and keep the player's cyan while
//     it is OPEN (the boss bar's OPEN: Cyan71.Open, plus THE WICK's stalled core; THE LOOM's parts are never the weak point, so they stay in the identity colour in its window too). This is
//     NIGHT71's cyan71 (bone, OFF) with a colour per boss. Hook: BossColours63.N63D.cs (Routes: the role "id74"; Route / Hot / PartRim / ShellRim take it like bone).
//   - THE CROWN's ring is not in that table (crown40's rule: "cyan while it may be passed"; crown72's: cyan through a red tell): Crown37View.Weak74.cs reads the ring's blades, rim, halo,
//     gem crosses and gem halos back after the last tint of the frame and takes every part still cyan to the ivory identity (RecolourIfCyan), and the Crown's point light with it. Skipped while
//     the window is OPEN (the whole ring is the weak point: OPEN: SHOOT IT) and through a pass's flash (the hit's confirmation).
//   - nothing else changes: THE ICEBREAKER draws cyan only on its stuck / dazed / reeling parts (open), THE LOOM's live pair of knees (the grapple node and the weak point) and the arena's own
//     ride trims stay the player's usable cyan (CLAUDE.md rule 8: the player rides them).
// THE PROOF (L-014 / L-115: the input fired): [weak74] lines. Per boss63 shot (FeelReel.N74.B.cs): the open weak point's screen box and the boss's, the parts still cyan, the row on or off;
// and the totals at a new run and the quit. With the row OFF the same line reads the cyan parts the row would have taken.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The pure maths (no Unity object): the colour at the site's luminance, and the cyan test. EditMode: N74BWeakTests.</summary>
    public static class Weak74Math
    {
        /// <summary>`id` (r, g, b in 0..1) at the luminance of (r, g, b), never past the site's brightest channel (Cyan71.ToBone's rule: an HDR site keeps its glow).</summary>
        public static void ToIdentity(double[] id, double r, double g, double b, out double R, out double G, out double B) => Cyan71.ToBone(id, r, g, b, out R, out G, out B);

        /// <summary>The hue (degrees, 0..360) and saturation of a colour (brightest channel as value); 0 / 0 for black or grey.</summary>
        public static double HueOf(double r, double g, double b, out double sat)
        {
            double m = Math.Max(r, Math.Max(g, b)), n = Math.Min(r, Math.Min(g, b)), d = m - n;
            sat = m > 1e-9 ? d / m : 0;
            if (d < 1e-9) return 0;
            double h;
            if (m == r) h = 60 * (((g - b) / d) % 6);
            else if (m == g) h = 60 * ((b - r) / d + 2);
            else h = 60 * ((r - g) / d + 4);
            return h < 0 ? h + 360 : h;
        }

        /// <summary>Is (r, g, b) the player's cyan: its hue in [hueLo, hueHi], its saturation at least satMin, and not black. HDR values are fine (hue and saturation are scale free).</summary>
        public static bool IsCyanish(double r, double g, double b, double hueLo, double hueHi, double satMin)
        {
            if (Math.Max(r, Math.Max(g, b)) < 1e-3) return false;
            double h = HueOf(r, g, b, out double s);
            return s >= satMin && h >= hueLo && h <= hueHi;
        }

        /// <summary>The direction word of a point the camera does not see: BEHIND when it is behind the camera, else ABOVE / BELOW / LEFT / RIGHT by its larger overshoot of the viewport (0..1);
        /// null when it is on screen. `vz` is the point's depth (viewport z).</summary>
        public static string Direction(double vx, double vy, double vz)
        {
            if (vz <= 0) return "BEHIND";
            double ox = vx < 0 ? -vx : vx > 1 ? vx - 1 : 0, oy = vy < 0 ? -vy : vy > 1 ? vy - 1 : 0;
            if (ox <= 0 && oy <= 0) return null;
            if (oy >= ox) return vy > 1 ? "ABOVE" : "BELOW";
            return vx < 0 ? "LEFT" : "RIGHT";
        }
    }

    public static class Weak74
    {
        public const string Row = "weak74";
        /// <summary>The pseudo role a closed weak part takes in BossColours63 (like Cyan71.Role "bone", with the boss's own colour).</summary>
        public const string Role = "id74";
        /// <summary>The boss indexes: Cyan71.Bosses' order (the prefix of a part's name in bossColours63's table).</summary>
        public const int Wick = 0, Crown = 1, Loom = 2, Wyrm = 3, Brawl = 4;

        static int onFrame = -1;
        static bool onNow;
        static GameConfig onCfg;

        /// <summary>The row, uncached (tests; the row's own answer through NightRows.ViewOn, the reel's live override first).</summary>
        public static bool Decide(GameConfig cfg) => cfg != null && NightRows.ViewOn(cfg, Row);

        /// <summary>Is the row on this frame? Read every frame (D1's same-run toggle flips ViewOn between frames); once a frame per config, the colour sites ask many times.</summary>
        public static bool On(GameConfig cfg)
        {
            int f = Time.frameCount;
            if (f != onFrame || !ReferenceEquals(cfg, onCfg)) { onFrame = f; onCfg = cfg; onNow = Decide(cfg); }
            return onNow;
        }

        // ---- the identity colours ----
        static readonly double[][] ids = BuildIds();

        static double[][] BuildIds()
        {
            uint[] hex = { UnityTuning.N74WickIdentityHex, UnityTuning.N74CrownIdentityHex, UnityTuning.N74LoomIdentityHex, UnityTuning.N74CrownIdentityHex, UnityTuning.N74CrownIdentityHex };
            var o = new double[5][];
            for (int i = 0; i < 5; i++)
            {
                var (r, g, b) = GameConfig.Hex(hex[i]);
                o[i] = new double[] { r, g, b };
            }
            return o;
        }

        /// <summary>A boss's identity colour as a Color (0..1, alpha 1). THE KILN WYRM and THE ICEBREAKER use the crown's ivory (no part of theirs is routed here).</summary>
        public static Color IdentityColour(int boss)
        {
            double[] d = ids[boss < 0 || boss > 4 ? Crown : boss];
            return new Color((float)d[0], (float)d[1], (float)d[2], 1f);
        }

        /// <summary>`c` in the boss's identity colour at c's own luminance (alpha kept). Allocation-free.</summary>
        public static Color ClosedBoss(int boss, Color c)
        {
            Weak74Math.ToIdentity(ids[boss < 0 || boss > 4 ? Crown : boss], c.r, c.g, c.b, out double r, out double g, out double b);
            return new Color((float)r, (float)g, (float)b, c.a);
        }

        /// <summary>A routed part's colour while its boss is closed (BossColours63 Route / Hot / PartRim / ShellRim with the role "id74").</summary>
        public static Color Closed(GameConfig cfg, string part, Color c) => ClosedBoss(Cyan71.BossOf(part), c);

        // ---- open or closed ----

        /// <summary>Is boss i open now? Cyan71.Open (the boss bar's OPEN; THE WICK's window), plus THE WICK's stalled core (its view draws that as the open cyan too).</summary>
        public static bool Open(int i)
        {
            if (i < 0 || i > 4) return false;
            if (Cyan71.Open(i)) return true;
            if (i != Wick) return false;
            SurvivalSim s = SurvivalGame.Instance != null ? SurvivalGame.Instance.sim : null;
            return s != null && s.wick37 != null && s.wick37.stalled;
        }

        /// <summary>A weak part's role this frame (BossColours63.Routes calls it for every "weak" part): "id74" while its boss is closed and the row is on, "weak" while it is open; a part of
        /// THE KILN WYRM, of no boss, or the row off: NIGHT71's answer exactly (Cyan71.WeakRole: "weak", or "bone" with cyan71 on).</summary>
        public static string WeakRole(GameConfig cfg, string part)
        {
            int i = Cyan71.BossOf(part);
            if (i < 0 || i == Wyrm || !On(cfg)) return Cyan71.WeakRole(cfg, part);
            bool open = Open(i);
            Frame(i, open);
            // THE LOOM's routed parts (knees, nails, seams) are never the weak point: its live pair of knees is drawn cyan by the view itself (unrouted), and its window's weak point is the
            // spinneret (Boss37Glow's cyan), so an open Loom's idle knees stay in the identity colour too
            return open && i != Loom ? "weak" : Role;
        }

        // ---- the counters (L-089: the input fired) ----
        public static readonly int[] ClosedFrames = new int[5], OpenFrames = new int[5], RingRecoloured = new int[5];
        static readonly int[] seen = { -1, -1, -1, -1, -1 };
        static bool dirty, wired;

        static void Frame(int i, bool open)
        {
            int f = Time.frameCount;
            if (seen[i] == f || FeelClock.Holding) return;
            seen[i] = f;
            dirty = true;
            Wire();
            if (open) OpenFrames[i]++;
            else ClosedFrames[i]++;
        }

        /// <summary>A view's pass for this frame (the Crown's ring): the frame counts once per boss, closed or open, and `recoloured` parts were taken to the identity colour.</summary>
        public static void NoteFrame(int boss, bool open, int recoloured)
        {
            if (boss < 0 || boss > 4) return;
            Frame(boss, open);
            if (recoloured > 0 && !FeelClock.Holding) { RingRecoloured[boss] += recoloured; dirty = true; }
        }

        static void Wire()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (wired || g == null) return;
            wired = true;
            Application.quitting += () => { if (dirty) { Debug.Log(TotalsLine("the quit")); dirty = false; } };
            g.OnNewRun += () => { if (dirty) { Debug.Log(TotalsLine("a new run")); dirty = false; } Array.Clear(ClosedFrames, 0, 5); Array.Clear(OpenFrames, 0, 5); Array.Clear(RingRecoloured, 0, 5); };
        }

        public static string TotalsLine(string why)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var sb = new System.Text.StringBuilder("[weak74] boss-frames in the identity colour (closed) / in cyan (open) / ring parts recoloured:");
            string[] names = Cyan71.Bosses;
            for (int i = 0; i < 5; i++)
            {
                if (ClosedFrames[i] + OpenFrames[i] + RingRecoloured[i] == 0) continue;
                sb.Append(' ').Append(names[i]).Append(' ').Append(ClosedFrames[i].ToString(ci)).Append(" / ").Append(OpenFrames[i].ToString(ci)).Append(" / ").Append(RingRecoloured[i].ToString(ci)).Append(';');
            }
            sb.Append(" | ").Append(why);
            return sb.ToString();
        }

        // ---- the readback pass (renderers that the views tinted this frame; THE CROWN's ring) ----
        static MaterialPropertyBlock blk;
        static int baseColorId;

        static MaterialPropertyBlock Blk()
        {
            if (blk == null) { blk = new MaterialPropertyBlock(); baseColorId = Shader.PropertyToID("_BaseColor"); }
            return blk;
        }

        /// <summary>Does the renderer's drawn _BaseColor (what the views set this frame, read back) read as the player's cyan? Counts nothing.</summary>
        public static bool IsCyanRenderer(Renderer r)
        {
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) return false;
            MaterialPropertyBlock b = Blk();
            b.Clear();
            r.GetPropertyBlock(b);
            Color c = b.GetColor(baseColorId);
            return Weak74Math.IsCyanish(c.r, c.g, c.b, UnityTuning.N74CyanHueLo, UnityTuning.N74CyanHueHi, UnityTuning.N74CyanSatMin);
        }

        /// <summary>The renderer's drawn colour, when it reads as the player's cyan, taken to boss's identity at its own luminance (alpha kept); true when it was taken. Run after the
        /// frame's last tint, with the row on and the boss closed.</summary>
        public static bool RecolourIfCyan(Renderer r, int boss)
        {
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) return false;
            MaterialPropertyBlock b = Blk();
            b.Clear();
            r.GetPropertyBlock(b);
            Color c = b.GetColor(baseColorId);
            if (!Weak74Math.IsCyanish(c.r, c.g, c.b, UnityTuning.N74CyanHueLo, UnityTuning.N74CyanHueHi, UnityTuning.N74CyanSatMin)) return false;
            b.SetColor(baseColorId, ClosedBoss(boss, c));
            r.SetPropertyBlock(b);
            return true;
        }

        /// <summary>A light's colour (0..1), when it reads as the player's cyan, taken to the identity at its luminance. True when it was taken.</summary>
        public static bool RecolourLight(Light l, int boss)
        {
            if (l == null) return false;
            Color c = l.color;
            if (!Weak74Math.IsCyanish(c.r, c.g, c.b, UnityTuning.N74CyanHueLo, UnityTuning.N74CyanHueHi, UnityTuning.N74CyanSatMin)) return false;
            Color k = ClosedBoss(boss, c);
            l.color = new Color(Mathf.Clamp01(k.r), Mathf.Clamp01(k.g), Mathf.Clamp01(k.b), 1f);
            return true;
        }

        /// <summary>One boss's cyan parts and the probe's world boxes for this frame: the view calls Begin, adds its renderers, and the reel reads the line at the frame's end.</summary>
        public static class Probe
        {
            public static int Frame = -1, Boss = -1, CyanParts, Parts;
            public static bool Open, HasWeak, HasBody;
            public static Bounds Weak, Body;

            public static void Begin(int boss, bool open)
            {
                Frame = Time.frameCount;
                Boss = boss;
                Open = open;
                HasWeak = HasBody = false;
                CyanParts = Parts = 0;
            }

            /// <summary>A part of the boss's body (its bounds join the body box); `weak`: also the open weak point's box (only meaningful while open). Counts the part if it reads cyan now.</summary>
            public static void Add(Renderer r, bool weak)
            {
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) return;
                Parts++;
                if (IsCyanRenderer(r)) CyanParts++;
                Bounds b = r.bounds;
                if (!HasBody) { Body = b; HasBody = true; } else Body.Encapsulate(b);
                if (!weak) return;
                if (!HasWeak) { Weak = b; HasWeak = true; } else Weak.Encapsulate(b);
            }

            /// <summary>A box with no renderer (THE LOOM's spinneret in its window): joins the body box, and the weak box when `weak`.</summary>
            public static void AddBox(Bounds b, bool weak)
            {
                if (!HasBody) { Body = b; HasBody = true; } else Body.Encapsulate(b);
                if (!weak) return;
                if (!HasWeak) { Weak = b; HasWeak = true; } else Weak.Encapsulate(b);
            }

            /// <summary>The probe is this boss's, drawn this frame or the one before (a LateUpdate view is read at the frame's end; an Update view the frame after).</summary>
            public static bool Fresh(int boss) => Boss == boss && Frame >= Time.frameCount - 1;
        }

        /// <summary>A world box as "x0,y0,x1,y1" in image pixels (rows from the top; the captured frame's own size), "none" (no part), "behind" (every corner behind the camera), "off" (in front,
        /// outside the frame) or "partial:x0,y0,x1,y1" (some corners behind the camera: the box spans the whole frame, a conservative answer).</summary>
        public static string BoxText(Camera cam, bool has, Bounds b)
        {
            if (!has || cam == null) return "none";
            int w = cam.pixelWidth, h = cam.pixelHeight, behind = 0;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            Vector3 c = b.center, e = b.extents;
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = cam.WorldToScreenPoint(c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z));
                if (p.z <= 0f) { behind++; continue; }
                x0 = Mathf.Min(x0, p.x); x1 = Mathf.Max(x1, p.x); y0 = Mathf.Min(y0, p.y); y1 = Mathf.Max(y1, p.y);
            }
            if (behind == 8) return "behind";
            bool partial = behind > 0;
            if (partial) { x0 = 0; y0 = 0; x1 = w; y1 = h; }
            if (x1 < 0 || y1 < 0 || x0 > w || y0 > h) return "off";
            x0 = Mathf.Clamp(x0, 0, w); x1 = Mathf.Clamp(x1, 0, w); y0 = Mathf.Clamp(y0, 0, h); y1 = Mathf.Clamp(y1, 0, h);
            CultureInfo ci = CultureInfo.InvariantCulture;
            return (partial ? "partial:" : "") + Mathf.RoundToInt(x0).ToString(ci) + "," + Mathf.RoundToInt(h - y1).ToString(ci) + "," + Mathf.RoundToInt(x1).ToString(ci) + "," + Mathf.RoundToInt(h - y0).ToString(ci);
        }

        /// <summary>The boss's index from a boss63 id (BossAttackClock63: wick, crown37, loom37, wyrm, brawl40); -1 for none.</summary>
        public static int IndexOf(string boss)
        {
            switch (boss)
            {
                case BossAttackClock63.Wick: return Wick;
                case BossAttackClock63.Crown: return Crown;
                case BossAttackClock63.Loom: return Loom;
                case BossAttackClock63.Wyrm: return Wyrm;
                case BossAttackClock63.Brawl: return Brawl;
            }
            return -1;
        }

        /// <summary>The proof line for one boss63 shot (FeelReel.N74.B.cs calls it at the captured frame's end): `[weak74] <file> | boss <id> | row on|off | open yes|no | weak box <x0,y0,x1,y1|...> | body box
        /// <...> | frame WxH | parts cyan <n> of <m> | identity #RRGGBB | <totals>`. The row OFF reads the parts the row would take (cyan now); ON, the parts still cyan (0 while closed).</summary>
        public static string ShotLine(SurvivalGame g, string file, string boss)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            int i = IndexOf(boss);
            bool on = g != null && Decide(g.cfg);
            bool fresh = Probe.Fresh(i);
            Camera cam = g != null ? g.cam : null;
            string weak = fresh && Probe.Open ? BoxText(cam, Probe.HasWeak, Probe.Weak) : "none";
            string body = fresh ? BoxText(cam, Probe.HasBody, Probe.Body) : "none";
            Color idc = IdentityColour(i);
            string hex = "#" + Mathf.RoundToInt(idc.r * 255f).ToString("X2", ci) + Mathf.RoundToInt(idc.g * 255f).ToString("X2", ci) + Mathf.RoundToInt(idc.b * 255f).ToString("X2", ci);
            return "[weak74] " + file + " | boss " + (boss ?? "?") + " | row " + (on ? "on" : "off") + " | open " + (fresh ? (Probe.Open ? "yes" : "no") : "not drawn")
                + " | weak box " + weak + " | body box " + body + " | frame " + (cam != null ? cam.pixelWidth.ToString(ci) + "x" + cam.pixelHeight.ToString(ci) : "?")
                + " | parts cyan " + (fresh ? Probe.CyanParts.ToString(ci) + " of " + Probe.Parts.ToString(ci) : "n/a") + " | identity " + hex
                + " | target74 row " + (g != null && Target74.Decide(g.cfg) ? "on" : "off") + ", weak point " + Target74.Status(g) + " | kill74 row " + (g != null && Kill74.Decide(g.cfg) ? "on" : "off")
                + " | crownroom74 row " + (g != null && CrownRoom74.Decide(g.cfg) ? "on" : "off") + ", sky blend " + CrownSky74.Blend.ToString("0.00", ci) + " (applied " + CrownSky74.AppliedFrames + " frames)"
                + " | " + TotalsLine("this shot");
        }
    }
}
