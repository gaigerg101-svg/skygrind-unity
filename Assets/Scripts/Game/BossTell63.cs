// NIGHT63 builder G (slate B2 "Boss attacks read in time", design.n63.bossTell63, a VIEW row: no tape bit). GG 2026-10-12: bosses "hard to
// understand what to do", "an understanding of parry mechanics"; GG 2026-10-22: "boss animations are too rigid". With the row on, every attack
// on B2's FIX LIST (NIGHT63 G B2b: design.n63.bossTell63.numbers.fix, computed from the boss63 before measure by Tools/night/n63-b2-fixlist.cjs:
// the attacks whose cue is short or off the danger hue; the slate: "attacks already fine are untouched") shows, through its whole wind-up, its
// own warm off-hue glows pulled to the danger hue (BossTell63Late, the late pass) and a DANGER COLUMN: an open tube of red-orange light (Tell36RedHex, the danger
// hue, CLAUDE.md rule 8) around the boss, BossTell63Size per boss, its level rising from BossTell63From at the wind-up's first step to 1 at the
// hit (BossTell63.Level), gone on the strike. Driven by BossAttackClock63 (V's read-only clock: the sim's own tell, so the cue's lead is the
// wind-up, config's 0.55-1.5 s, over the slate's 300 ms); nothing in the sim is read for writing, no damage frame moves.
// NOT drawn for a parryable attack (the sim's tell kind yellow: THE WICK's ember, THE LOOM's spool, THE CROWN's gem shard, THE ICEBREAKER's
// block) nor the wyrm's punch window (aggro, aggroLean: its head goes yellow) or roar (UnityTuning.BossTell63Skip): their parry tells are
// untouched, on or off. Drawn with the views' own glow path (FxDraw.Additive on the neon material, a MaterialPropertyBlock _BaseColor: what
// Brawl40View's fists use) under the boss view's own root, so the boss63 reel's cue measure (FeelReel.N63G.cs) reads exactly what is drawn.
// Hooks: one line per boss view (Wick37View.Render, Brawl40View.Render, WyrmView.Render, Loom37View.Render, Crown37View.LateUpdate):
// `BossTell63.Draw(game, root, "<boss id>");`. THE SWITCH is read every frame (NightRows.ViewOn: D1's same-run toggle flips it); off (the row
// off, -skygrindOff63 or older, -skygrindNoBossTell63): nothing is built, or the built column stays hidden. Proof (L-089): Player.log
// "[tell63] cue <boss> <attack> | level <l> at wind-up <w>" once per attack shown, "[tell63] skip <boss> <attack> (<why>)" once per attack
// left alone. EditMode: N63BossTellTests.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>NIGHT63 G (bossTell63): what the danger column did (a reel may print it).</summary>
    public static class BossTell63Probe
    {
        public static string Boss = "", Attack = "";
        public static int Shown, Skipped, Frame = -1;
        /// <summary>NIGHT63 G B2b: renderers and lights the late pass drew in the danger hue this frame.</summary>
        public static int Recoloured;
        public static float Level;
    }

    public static class BossTell63
    {
        public const string Row = "bossTell63";
        public const string NoSwitch = "-skygrindNoBossTell63";
        public const int Night = 63;
        /// <summary>The column's GameObject name (the boss63 cue line prints it as the strongest riser).</summary>
        public const string CueName = "tell63 cue";

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Why the row is off for this config and command line (null: on).</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return "design.n63." + Row + " off";
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff63 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch; // NIGHT63 R1 N5: the one rule (case ignored, as before)
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it (D1's same-run toggle flips ViewOn).</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>Does this attack get the danger column: an attack, not parryable (kind yellow), not in UnityTuning.BossTell63Skip.</summary>
        public static bool Wants(string boss, string attack, string kind) =>
            !string.IsNullOrEmpty(attack) && kind != "yellow" && Array.IndexOf(UnityTuning.BossTell63Skip, (boss ?? "") + ":" + attack) < 0;

        // ---- NIGHT63 G B2b (the slate: "attacks already fine are untouched"): only the attacks the before measure lists get the fix ----

        static GameConfig fixFor;
        static IList<string> fixList = new string[0];

        /// <summary>B2's fix list ("boss:attack") from design.n63.bossTell63.numbers.fix: computed from the boss63 before logs by
        /// Tools/night/n63-b2-fixlist.cjs with BossCue63.Verdict's rule (read once per config).</summary>
        public static IList<string> FixList(GameConfig cfg)
        {
            if (cfg == null) return new string[0];
            if (ReferenceEquals(cfg, fixFor)) return fixList;
            fixFor = cfg;
            var l = new List<string>();
            if (cfg.Has("design.n63.bossTell63.numbers.fix") && cfg.Get("design.n63.bossTell63.numbers.fix") is List<object> raw)
                foreach (object o in raw)
                    if (o is string s && s.Length > 0) l.Add(s);
            fixList = l;
            return fixList;
        }

        /// <summary>Does this attack get the fix: on the list and an eligible attack (Wants: an attack, not parryable, not in BossTell63Skip).</summary>
        public static bool Wants(string boss, string attack, string kind, IList<string> fix) =>
            fix != null && Wants(boss, attack, kind) && fix.Contains((boss ?? "") + ":" + attack);

        /// <summary>A warm off-hue glow (saturated, a hue strictly between the colour table's danger band and its parry band: the crown's point
        /// edge at 39 deg) pulled to the danger red-orange (Tell36RedHex's hue) at the same brightness (max channel) and alpha. False: leave it
        /// (already danger: the wyrm's amber 31.8 is, in the table; the parry role: a parry or punch tell in yellow stays yellow, review S7;
        /// cyan, pale, dark; no table). B2d: also a WHITE-HOT warm glow (pale, so no hue the meter reads, but warm: from the danger band's start to
        /// the parry band's start, tinted at least BossTell63WarmMinSat, and HDR-hot: level >= BossTell63HotLevel): the crown's clapper at its fire,
        /// Crown47WhiteHex x 2.4, which outshines the column (n63-b9: crownFire's riser hue 41, saturation 0.14, off-hue after the fix).</summary>
        public static bool ToDanger(Color c, out Color d)
        {
            d = c;
            BossCue63.Hsv(c.r, c.g, c.b, out double h, out double s, out double v);
            if (v < UnityTuning.Boss63CueMinLevel) return false;
            BossCue63.Bands63 b = BossCue63.Bands;
            if (!b.ok) return false;
            if (s >= UnityTuning.Boss63CueMinSat)
            {
                if (BossCue63.DangerHue(h, s) || BossCue63.ParryHue(h, s)) return false;
                if (!BossCue63.InArc(h, b.dangerHi, b.parryLo)) return false; // between the two bands, on the warm side (35-45 in the table)
            }
            else
            {
                if (v < UnityTuning.BossTell63HotLevel || s < UnityTuning.BossTell63WarmMinSat) return false; // a surface, or a pure white
                if (!BossCue63.InArc(h, b.dangerLo, b.parryLo) || h == b.parryLo) return false; // warm, never a pale parry yellow
            }
            Color red = Look.Hex(UnityTuning.Tell36RedHex);
            float k = (float)v / Mathf.Max(red.r, Mathf.Max(red.g, red.b));
            d = new Color(red.r * k, red.g * k, red.b * k, c.a);
            return true;
        }

        /// <summary>The fix drawing this frame (the late pass reads it): the view's root, the boss, the attack, the frame.</summary>
        public struct Fix63
        {
            public Transform root;
            public string boss, attack;
            public int frame;
        }

        public static Fix63 ActiveFix;

        /// <summary>The column's level at `windup01` of the wind-up (0 its first step, 1 the hit): BossTell63From eased to 1.</summary>
        public static float Level(double windup01)
        {
            float k = Mathf.Clamp01((float)windup01);
            k = k * k * (3f - 2f * k);
            return UnityTuning.BossTell63From + (1f - UnityTuning.BossTell63From) * k;
        }

        /// <summary>The column's drawn colour at `level`: the danger red-orange (Tell36RedHex) x BossTell63Gain x level.</summary>
        public static Color Colour(float level) => Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.BossTell63Gain * Mathf.Max(0f, level));

        /// <summary>The sim's tell colour for the attack now: a Boss37Brain attack's kind (THE WICK, THE LOOM, THE CROWN); THE ICEBREAKER's block
        /// yellow, its others red; the wyrm "?" (its log has no kind). Reads only.</summary>
        public static string Kind(SurvivalSim sim, string boss, string attack)
        {
            if (sim == null) return "?";
            Boss37Brain b = boss == BossAttackClock63.Wick ? sim.wick37?.brain : boss == BossAttackClock63.Loom ? sim.loom37?.brain : boss == BossAttackClock63.Crown ? sim.crown37?.brain : null;
            if (b != null) { Boss37Attack k = b.Current; return k == null ? "?" : k.kind == Tell36Kind.Yellow ? "yellow" : k.kind == Tell36Kind.Red ? "red" : "?"; }
            if (boss == BossAttackClock63.Brawl) return attack == Brawl40.Block ? "yellow" : attack == null ? "?" : "red";
            return "?";
        }

        // ---- the drawing (one column per boss view root, built the first frame the row is on) ----

        sealed class Cue
        {
            public Transform t;
            public MeshRenderer r;
            public string said; // the attack last said (shown or skipped), so each attack is said once
        }

        static readonly Dictionary<Transform, Cue> cues = new Dictionary<Transform, Cue>();
        /// <summary>The drawing's engine objects, in their own class so the pure rules above never start the engine (EditMode, the tests).</summary>
        static class Gfx
        {
            internal static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
            internal static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        }

        static Material mat;
        static Mesh tube;

        /// <summary>A boss view's hook (its Render / LateUpdate, every frame): the column for `boss` under the view's `root`.</summary>
        public static void Draw(SurvivalGame game, Transform root, string boss)
        {
            if (game == null || root == null) return;
            BossCue63.Use(game.cfg); // review S3: the colour table's bands for the late pull (a reference check when unchanged)
            bool on = On(game.cfg) && game.sim != null;
            cues.TryGetValue(root, out Cue cue);
            if (cue == null || cue.t == null)
            {
                if (!on) return; // nothing is built while the row is off: the off game draws NIGHT62's boss
                cue = Build(game, root);
                if (cue == null) return;
                cues[root] = cue;
            }
            BossAttack63 a = on ? BossAttackClock63.Read(game.sim, boss) : default;
            bool windup = on && a.phase == BossPhase63.Windup && !string.IsNullOrEmpty(a.attack);
            string kind = windup ? Kind(game.sim, boss, a.attack) : null;
            IList<string> fix = FixList(game.cfg);
            bool show = windup && Wants(boss, a.attack, kind, fix); // NIGHT63 G B2b: listed attacks only (the rest already read in time and in the danger hue)
            if (windup && !show && cue.said != a.attack)
            {
                cue.said = a.attack;
                BossTell63Probe.Skipped++;
                Debug.Log($"[tell63] skip {boss} {a.attack} ({(kind == "yellow" ? "parryable: its own yellow tell" : !Wants(boss, a.attack, kind) ? "UnityTuning.BossTell63Skip" : "not on the fix list: already in time and in the danger hue")})");
            }
            if (!windup) cue.said = null;
            if (cue.r.enabled != show) cue.r.enabled = show;
            if (!show && ReferenceEquals(ActiveFix.root, root)) ActiveFix = default; // the late pass restores this boss's glows at once (the row off, the wind-up over)
            if (!show) return;
            float level = Level(a.Windup01);
            float[] size = UnityTuning.BossTell63Size.TryGetValue(boss ?? "", out float[] s) ? s : new[] { 3.5f, 8f };
            float colR = size[0], colH = size[1];
            // NIGHT72 B2 bossfx72 (a view row, read every frame): THE CROWN's column is a share of its size (UnityTuning.Fx72ColumnShare: about a third of the frame at 6.6 m, gg-proxy #3)
            if (NightRows.ViewOn(game.cfg, "bossfx72") && UnityTuning.Fx72ColumnShare.TryGetValue(boss ?? "", out float[] share)) { colR *= share[0]; colH *= share[1]; }
            Vector3 ls = root.lossyScale;
            cue.t.position = new Vector3((float)a.centre.x, (float)a.centre.y, (float)-a.centre.z); // web z is Unity -z
            cue.t.rotation = Quaternion.identity;
            cue.t.localScale = new Vector3(colR / Nz(ls.x), colH / Nz(ls.y), colR / Nz(ls.z));
            Gfx.block.SetColor(Gfx.BaseColor, Colour(level));
            cue.r.SetPropertyBlock(Gfx.block);
            BossTell63Probe.Boss = boss ?? "";
            BossTell63Probe.Attack = a.attack;
            BossTell63Probe.Level = level;
            BossTell63Probe.Frame = Time.frameCount;
            ActiveFix = new Fix63 { root = root, boss = boss, attack = a.attack, frame = Time.frameCount }; // the late pass pulls this boss's own warm glows (BossTell63Late)
            if (cue.said != a.attack)
            {
                cue.said = a.attack;
                BossTell63Probe.Shown++;
                Debug.Log($"[tell63] cue {boss} {a.attack} | level {level:0.00} at wind-up {a.Windup01:0.00} ({a.stepsToHit} steps to the hit), radius {colR:0.0} m, height {colH:0.0} m");
            }
        }

        static float Nz(float v) => Mathf.Abs(v) < 1e-4f ? 1f : v;

        static Cue Build(SurvivalGame game, Transform root)
        {
            if (game.mats == null || game.mats.neon == null) return null;
            if (mat == null) mat = FxDraw.Additive(game.mats.neon, "BossTell63 column");
            if (tube == null) tube = Tube(Math.Max(8, UnityTuning.BossTell63Segments));
            var go = new GameObject(CueName);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = tube;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return new Cue { t = go.transform, r = r };
        }

        /// <summary>An open tube of radius 1 and height 1 centred on the origin (y -0.5 .. 0.5), both faces.</summary>
        static Mesh Tube(int n)
        {
            var v = new Vector3[n * 2];
            var tri = new int[n * 12];
            for (int i = 0; i < n; i++)
            {
                float ang = i * Mathf.PI * 2 / n;
                float x = Mathf.Cos(ang), z = Mathf.Sin(ang);
                v[i * 2] = new Vector3(x, -0.5f, z);
                v[i * 2 + 1] = new Vector3(x, 0.5f, z);
                int j = (i + 1) % n, k = i * 12;
                int a0 = i * 2, a1 = i * 2 + 1, b0 = j * 2, b1 = j * 2 + 1;
                tri[k] = a0; tri[k + 1] = a1; tri[k + 2] = b0;
                tri[k + 3] = b0; tri[k + 4] = a1; tri[k + 5] = b1;
                tri[k + 6] = a0; tri[k + 7] = b0; tri[k + 8] = a1;
                tri[k + 9] = b0; tri[k + 10] = b1; tri[k + 11] = a1;
            }
            var m = new Mesh { name = "BossTell63 tube", vertices = v, triangles = tri };
            m.RecalculateBounds();
            return m;
        }
    }
}
