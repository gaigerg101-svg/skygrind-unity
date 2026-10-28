// NIGHT63 builder D (slate B3, BOSS COLOURS; view row design.n63.bossColours63, OFF; the parts: BossColours63Math.N63D.cs; the colours: C1's
// table, design.n63.colours63.numbers). With the row on, each boss part named in the row's numbers wears its role, read every frame:
//   WEAK (the player's cyan, at the site's own luminance: dim while closed, bright when OPEN, one hue on every boss): THE WICK's idle core,
//     THE CROWN's gems and halos, THE LOOM's knees (rest, seams, nails), THE KILN WYRM's punchable head, THE ICEBREAKER's stuck fists / dazed rims;
//   DANGER (red-orange): THE WICK's ember-lit plate edges, THE CROWN's gold clap and ring edge and its flame's yellow.
//   NIGHT63 B3b (the wyrm's pairs on n63-b5's b3k changed 0 / 0 / 0 / 6 px): THE KILN WYRM's head LIT while it is OPEN (staggered or its core
//   exposed) as the punch window's head, weak (Lit: today's dark head with the row off); its exposed core weak; its seams and spit / shards danger.
// Hooks: one wrapper per colour site (BossColours63.Part(cfg, "<part>", colour)) in Wick37View.cs, Crown37View.Crown38.cs / .Crown40.cs /
// .Look47.cs / .N47.cs, Loom37View.cs / .N47.cs, WyrmView.cs, Brawl40View.N47.cs. The parry tells keep their yellow (THE WICK's yellow core
// flash and embers, THE ICEBREAKER's block, THE LOOM's yellow spool tell: DECIDE63 D-1); G's danger column (bossTell63) is untouched.
// READ EVERY FRAME (D1's contract): NightRows.Live(decided, "bossColours63") at each call; the views keep their own state (the colour is chosen
// per frame), so the held off frame shows each part's own colour and the next frame its role again. Off (the row off, -skygrindOff63 or older,
// -skygrindNoBossColours63, or C1's table missing): every call returns its colour unchanged. The proof (L-089): one Player log line per part the
// first time it is routed, "[bossColours63] <part>: <role> #from -> #to", and BossColours63.Routed (the calls that routed a colour, ever).
// View only: no sim read, no tape bit.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using B = Skygrind.Game.BossColours63Math;

namespace Skygrind.Game
{
    public static class BossColours63
    {
        static GameConfig cfgFor;
        static bool decided;
        static Colours63Math.Table table;
        static B.Parts parts;
        static readonly HashSet<string> logged = new HashSet<string>(StringComparer.Ordinal);
        static string[] args;

        /// <summary>For a reel: the calls that routed a colour (ever), and the state line.</summary>
        public static int Routed;
        public static string Line = "bossColours63: not read";

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        static bool Ready(GameConfig cfg)
        {
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, cfgFor))
            {
                cfgFor = cfg;
                bool rowOn = NightRows.ForceOffFrom > B.Night && cfg.Design("n63." + B.Row + ".enabled"); // the config's own answer (D1's override is per call)
                decided = B.Decide(rowOn, Args());
                table = decided ? Colours63Math.From(cfg) : null;
                parts = decided ? B.From(cfg) : null;
                aims = decided ? WeakAim64Math.From(cfg) : null; // NIGHT64 B3: the weak aims (design.n63.bossColours63.numbers.weakAim)
                if (decided) Debug.Log(WeakAim64Math.Line(aims));
                Line = !decided ? "bossColours63 OFF" : table == null ? "bossColours63 ON but C1's table (design.n63.colours63.numbers) is missing: nothing routed"
                    : parts == null ? "bossColours63 ON but its parts (design.n63.bossColours63.numbers.parts) are missing: nothing routed" : "bossColours63 ON: " + parts.Count + " parts";
                if (decided) Debug.Log("[bossColours63] " + Line);
            }
            return decided && table != null && parts != null;
        }

        /// <summary>A boss part's colour this frame: through its role while the row is on (this frame), else unchanged. Allocation-free after the
        /// part's first routed call (its first-routed proof line; NIGHT63 B3c: the per-shot line is FrameLine).</summary>
        public static Color Part(GameConfig cfg, string part, Color c)
        {
            if (!Routes(cfg, part, out string role)) return c;
            Color o = Route(role, c, part);
            Note(part, role, c, o);
            return o;
        }

        /// <summary>NIGHT63 B3b: a boss part LIT only with the row on: this frame, with the row on and the part in the table, `lit` through the part's
        /// role (at lit's own brightness); else `off` (today's colour) unchanged. THE KILN WYRM's head while OPEN (wyrm.open): today it keeps its dark
        /// ash and jaw heat; with the row on it is lit as the punch window's head, in the weak role. Allocation-free after the first routed call.</summary>
        public static Color Lit(GameConfig cfg, string part, Color off, Color lit)
        {
            if (!Routes(cfg, part, out _)) return off;
            return Part(cfg, part, lit);
        }

        // --- NIGHT63 B3c (B3's GATE FAIL, Logs/n63/verdict-B3.md; one fix round) ---

        /// <summary>NIGHT63 B3c: a part at a site that multiplies its colour by `gain` (Tint(r, c, gain)): returns the colour to draw at gain 1. With
        /// the row on, the gain first, then the route, so a danger route is clip-safe on what the screen gets (THE CROWN's red tell at x2.8 read
        /// amber); off, c * gain exactly as the site computed it (Unity's Color * float, alpha included).</summary>
        public static Color Hot(GameConfig cfg, string part, Color c, float gain)
        {
            if (!Routes(cfg, part, out string role)) return c * gain;
            if (role == Cyan71.Role) { Color k = Cyan71.ToBone(cfg, c * gain); Note(part, role, c * gain, k); return k; } // NIGHT71 B cyan71: the gain first, then the bone
            if (role == Weak74.Role) { Color k = Weak74.Closed(cfg, part, c * gain); Note(part, role, c * gain, k); return k; } // NIGHT74 B weak74: the gain first, then the boss's own identity colour
            B.Hot(table, role, c.r, c.g, c.b, gain, out double r, out double g, out double b);
            var o = new Color((float)r, (float)g, (float)b, c.a * gain);
            Note(part, role, c * gain, o);
            return o;
        }

        /// <summary>NIGHT63 B3c: a weak part through its role and then the fight's grade rim (BossGrade54.RimAt: the player's cyan in this fight,
        /// pre-divided by the grade's colour filter, at the routed colour's brightness; unchanged where the boss has no grade). THE KILN WYRM's grade
        /// cuts blue to 0.78 and tones the highlights sulphur (its routed cyan read lime on every boss63 shot); THE LOOM's cuts red and green to
        /// 0.86 (its routed knees read hue 224, blue, on b5). Row off: c unchanged.</summary>
        public static Color PartRim(GameConfig cfg, string part, Color c)
        {
            if (!Routes(cfg, part, out string role)) return c;
            Color o = role == Cyan71.Role || role == Weak74.Role ? Route(role, c, part) : Weak(part, Route(role, c, part)); // NIGHT71 B cyan71: bone takes no cyan rim; NIGHT74 B weak74: nor does the identity colour
            Note(part, role, c, o);
            return o;
        }

        static Dictionary<string, WeakAim64Math.Aim> aims;

        /// <summary>NIGHT64 B3 (builder D; WeakAim64Math.N64D.cs): a weak part's routed cyan in a graded fight: the boss's own weak aim
        /// (design.n63.bossColours63.numbers.weakAim.&lt;id&gt;: aim x its brightest channel x scale) where it has one, under RimAt's own gate
        /// (BossGrade54.RimBoss); else RimAt (NIGHT63 B3c). THE KILN WYRM's head read hue 158.5 through RimAt: its HDR green clipped before the
        /// grade's blue cut. Allocation-free.</summary>
        static Color Weak(string part, Color routed)
        {
            string id = BossGrade54.RimBoss();
            if (id != null && aims != null && aims.TryGetValue(id, out WeakAim64Math.Aim a) // NIGHT64 B3 round 2: the part's own aim (.For(part)), else the boss's
                && WeakAim64Math.At(a.For(part), routed.r, routed.g, routed.b, out double r, out double g, out double b))
                return new Color((float)r, (float)g, (float)b, routed.a);
            return BossGrade54.RimAt(routed);
        }

        /// <summary>NIGHT63 B3c: Lit through the fight's grade rim (THE KILN WYRM's open head). Row off (or the part not in the table): `off`.</summary>
        public static Color LitRim(GameConfig cfg, string part, Color off, Color lit)
        {
            if (!Routes(cfg, part, out _)) return off;
            return PartRim(cfg, part, lit);
        }

        /// <summary>NIGHT63 B3c: BossMarks32's additive shell colour `c` x `glow` (alpha 1, as its Glow): with the row on, `c` through the part's role
        /// and the fight's grade rim first (THE KILN WYRM's open-window shell over its head: cyan-white, it read lime). Row off: exactly Glow's.</summary>
        public static Color ShellRim(GameConfig cfg, string part, Color c, float glow)
        {
            if (!Routes(cfg, part, out string role)) return new Color(c.r * glow, c.g * glow, c.b * glow, 1f);
            Color k = role == Cyan71.Role || role == Weak74.Role ? Route(role, c, part) : Weak(part, Route(role, c, part)); // NIGHT64 B3: the part's (or the boss's) weak aim, else RimAt (NIGHT71 cyan71: bone takes no rim; NIGHT74 weak74: nor does the identity colour)
            var o = new Color(k.r * glow, k.g * glow, k.b * glow, 1f);
            Note(part, role, new Color(c.r * glow, c.g * glow, c.b * glow, 1f), o);
            return o;
        }

        /// <summary>NIGHT63 B3c (B3's GATE: the first-routed line named another draw path's colour than the judged frame's): one line for a shot,
        /// naming per part the colour that frame used (the last call that frame, and how many calls it had). `frame` is Time.frameCount of the
        /// captured frame.</summary>
        public static string FrameLine(string file, int frame)
        {
            var list = new List<B.FramePart>();
            foreach (KeyValuePair<string, Rec> kv in recs)
            {
                if (kv.Value.frame != frame) continue;
                Rec r = kv.Value;
                list.Add(new B.FramePart { part = kv.Key, role = r.role, fr = r.from.r, fg = r.from.g, fb = r.from.b, tr = r.to.r, tg = r.to.g, tb = r.to.b, calls = r.calls });
            }
            list.Sort((a, b) => string.CompareOrdinal(a.part, b.part));
            return B.FrameLine(file, frame, list);
        }

        struct Rec
        {
            public int frame, calls;
            public Color from, to;
            public string role;
        }

        static readonly Dictionary<string, Rec> recs = new Dictionary<string, Rec>(StringComparer.Ordinal);

        static bool Routes(GameConfig cfg, string part, out string role)
        {
            role = null;
            if (!Ready(cfg) || !NightRows.Live(true, B.Row)) return false;
            role = parts.RoleOf(part);
            if (role == "weak") { Cyan71.Wire(SurvivalGame.Instance); role = Weak74.WeakRole(cfg, part); } // NIGHT74 B weak74 (Weak74.cs): "id74" while its boss is closed (the row on), "weak" while open; with the row off, NIGHT71 B cyan71's answer (Cyan71.WeakRole): "bone" while closed (the row on), "weak" as before
            return role != null;
        }

        static Color Route(string role, Color c, string part)
        {
            if (role == Weak74.Role) return Weak74.Closed(cfgFor, part, c); // NIGHT74 B weak74: a closed weak part in its boss's own identity colour at its own luminance
            if (role == Cyan71.Role) return Cyan71.ToBone(cfgFor, c); // NIGHT71 B cyan71: a closed weak part in bone at its own luminance
            B.Route(table, role, c.r, c.g, c.b, out double r, out double g, out double b);
            return new Color((float)r, (float)g, (float)b, c.a);
        }

        /// <summary>The proof: the first routed call per part (once), and per frame the last call (FrameLine). No allocation after a part's first call.</summary>
        static void Note(string part, string role, Color from, Color to)
        {
            Routed++;
            if (logged.Add(part)) Debug.Log("[bossColours63] " + part + ": first routed " + role + " " + Hex(from) + " -> " + Hex(to) + " (per shot: the frame lines)");
            int f = Time.frameCount;
            recs.TryGetValue(part, out Rec r);
            r.calls = r.frame == f ? r.calls + 1 : 1;
            r.frame = f;
            r.from = from;
            r.to = to;
            r.role = role;
            recs[part] = r;
        }

        static string Hex(Color c)
        {
            float m = Mathf.Max(1f, c.maxColorComponent);
            CultureInfo ci = CultureInfo.InvariantCulture;
            int r = Mathf.Clamp(Mathf.RoundToInt(c.r / m * 255f), 0, 255), g = Mathf.Clamp(Mathf.RoundToInt(c.g / m * 255f), 0, 255), b = Mathf.Clamp(Mathf.RoundToInt(c.b / m * 255f), 0, 255);
            return "#" + r.ToString("X2", ci) + g.ToString("X2", ci) + b.ToString("X2", ci) + (m > 1f ? " x" + m.ToString("0.0", ci) : "");
        }
    }
}
