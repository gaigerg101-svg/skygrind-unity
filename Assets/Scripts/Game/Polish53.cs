// NIGHT53 builder U: design.n53.polish53 (a view row; NIGHT53.md section 4: NIGHT52's SHORT polish items and a kill-confirm HUD pop). Its switch:
// NightRows.ViewOn(cfg, "polish53") and -skygrindOff53 (or any older off switch) read from the command line too (the boot and the map are built
// before PlayerChecks applies the switch, as look52 does). Each item has its own numbers object (design.n53.polish53.numbers.<item>) with an
// "on" (0 / 1): an item at 0 is NIGHT52's look for that item alone (its fallback), the others unchanged. Off: NIGHT52's game exactly.
// Item 1 (T-122, the first-shot hitch; FirstShot53 below): the Player is Mono (MonoBleedingEdge in the build), so a method runs its JIT the first
// time it is called; the shot's path (the loadout's fire, the cone, the husks' hit, the view's fire handlers: the star, the tracers, the flare,
// the light, the viewmodel's kick, the impact, the decals, the hit react) is first called on the run's first shot, inside that frame. The
// materials and the GPU states of the star, the lines and the kill blocks were made at load since NIGHT17 (WeaponFx.WarmDraws, Chunks.Warm), so
// the JIT is what is left on the first shot's frame; a recorded GraphicsStateCollection (Warmup.cs) is still not shipped ("[warmup] none" in
// every n52 Player log). With firstShot.on the fire path's types (UnityTuning.Polish53JitTypes) are compiled at boot, once per process, on the
// main thread (static constructors may run there: never on a worker), within Polish53JitBudgetMs. Nothing runs: PrepareMethod compiles only.
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Debug = UnityEngine.Debug;

namespace Skygrind.Game
{
    public static class Polish53
    {
        const string Row = "design.n53.polish53.numbers.";

        /// <summary>The reels' override (null: the config and the command line).</summary>
        public static bool? Force;

        static GameConfig readFor;
        static bool on;

        /// <summary>polish53 draws: its row on and no -skygrindOffNN at or under 53.</summary>
        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor))
            {
                readFor = cfg;
                bool offSwitch = NightRows.OffFrom(Environment.GetCommandLineArgs(), "-skygrindOff") <= 53;
                on = NightRows.ViewOn(cfg, "polish53") && !offSwitch;
            }
            return on;
        }

        static GameConfig numsFor;
        static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, double>> nums =
            new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, double>>();

        /// <summary>An item's number (design.n53.polish53.numbers.&lt;item&gt;.&lt;key&gt;), `missing` when the key is not there. Read once per
        /// config and kept (the per-frame readers allocate nothing after the first read).</summary>
        public static double Num(GameConfig cfg, string item, string key, double missing)
        {
            if (cfg == null) return missing;
            if (!ReferenceEquals(cfg, numsFor)) { nums.Clear(); numsFor = cfg; }
            if (!nums.TryGetValue(item, out var keys)) { keys = new System.Collections.Generic.Dictionary<string, double>(); nums[item] = keys; }
            if (keys.TryGetValue(key, out double v)) return double.IsNaN(v) ? missing : v;
            string k = Row + item + "." + key;
            v = double.NaN;
            if (cfg.Has(k))
            {
                try { v = cfg.Num(k); }
                catch (Exception) { v = double.NaN; }
            }
            keys[key] = v;
            return double.IsNaN(v) ? missing : v;
        }

        /// <summary>The item draws: polish53 on and its own "on" not 0 (NIGHT71 L4: or, for killWash, design.n71.wash71 on: Wash71.cs).</summary>
        public static bool Item(GameConfig cfg, string item) => On(cfg) && (Num(cfg, item, "on", 0) != 0 || item == "killWash" && Wash71.On(cfg));
    }

    /// <summary>Item 2 (T-111, the kill wash's f3-f4 source). NIGHT52's frames (Logs/feel/n52-freeze/u2on/a252/S52-wash-on-f0..f8-box, read
    /// frame by frame): on f1-f4 the kill's blocks are WHITE (K's cap: Fx49KillWhiteFrames 4 drawn frames, x KillWhiteHdr, the hold scale) and,
    /// flying apart, cover a third of the wash box where the living husk's body covered less (f1 +0.07, f3 +0.32, f4 +0.27 over f-2); the fx49
    /// kit's kill ring (12 deg, its band through the box's sides) adds on f2-f5; on f6 the blocks are the enemy's colour (+0.20, the cap's
    /// frames). So the f3-f4 source is the white blocks' last two white frames, which K's f6-f8 cap kept. With killWash.on a gun kill's blocks
    /// are white for killWash.whiteFrames drawn frames (the kill's flash on its first frames, then the enemy's colour as they spread); a punch
    /// kill and NIGHT17's chunks (FeelAB off) unchanged. Off: K's cap (or none) exactly.</summary>
    public static class KillWash53
    {
        /// <summary>The reel's probe: kills whose white frames this item set.</summary>
        public static int Capped;

        /// <summary>n53-r1 (ON peak +0.380 f3 foundry / +0.343 iceline, the white frames alone not enough): the f3-f4 window is the kill burst's
        /// FADE frames (KillBurstFullFrames 2 + KillBurstFadeFrames 2), when NIGHT18's white-hot silhouette (additive, x KillSilhouetteHdr, a touch
        /// bigger than the held body) still draws over the crush disc, and the fx49 kit's kill burst (its ring's band through the box). With
        /// killWash.on: the silhouette only on its first killWash.silhouetteFrames held frames (the burst's full frames: the impact stays), and a
        /// kill's fx49 burst inside the wash box x killWash.burstK. Off: `frames` / 1 (NIGHT52's).</summary>
        public static int SilFrames(GameConfig cfg, int frames)
        {
            if (!Polish53.Item(cfg, "killWash")) return frames;
            int n = (int)Polish53.Num(cfg, "killWash", "silhouetteFrames", -1);
            return n >= 0 ? Math.Min(frames, n) : frames;
        }

        public static float BurstK(GameConfig cfg)
        {
            if (!Polish53.Item(cfg, "killWash")) return 1f;
            double k = Polish53.Num(cfg, "killWash", "burstK", 1);
            return k >= 0 && k <= 1 ? (float)k : 1f;
        }

        /// <summary>The reel's probe: kill rings not drawn in the wash box under ringInWash 0.</summary>
        public static int RingsSkipped;

        /// <summary>EXT53 B: a kill burst inside the wash box draws its fx49 ring (true: NIGHT52's / U's rule, and every case with the item off or the
        /// key missing); false under killWash.on with killWash.ringInWash 0 (n53-r2's f3-f4 frames: the ring, white on screen at any burstK, was the
        /// box's brightest area).</summary>
        public static bool RingInWash(GameConfig cfg)
        {
            if (!Polish53.Item(cfg, "killWash") || Polish53.Num(cfg, "killWash", "ringInWash", 1) != 0) return true;
            RingsSkipped++;
            return false;
        }

        public static int WhiteCap(GameConfig cfg, int cap, bool n18, bool melee)
        {
            if (!n18 || melee || !Polish53.Item(cfg, "killWash")) return cap;
            int wf = (int)Polish53.Num(cfg, "killWash", "whiteFrames", -1);
            if (wf < 1) return cap;
            Capped++;
            return cap >= 0 ? Math.Min(cap, wf) : wf;
        }
    }

    /// <summary>Item 3 (T-120, the scorch core). NIGHT52 shipped design.n52.skin52.numbers.scorchGlow 0 (D-22: the a252 core box read darker
    /// than bare floor). n52-r15's ON frame (Logs/feel/n52-r15/u2on/a252/S52-scorch-on-crop.png) shows the core drawn hot (an orange disc at the
    /// star's middle, its colour 1.00 / 0.26 / 0.08 on the block) but a white band crossed the box in that frame and its "before" read 0.794
    /// (the OFF side's 0.416 on the same floor kind): the before / after measure, not the core, was dark. With scorchCore.on the core is drawn
    /// with scorchCore.glow (skin52's hot-core path, EnemyViews.Scorch52.cs) while skin52 is live; the a153 reel judges it as a pair on ONE mark
    /// in one pose, a frame apart (polish53 forced off, then on), so anything else in the box is in both frames. Off: NIGHT52's mark exactly.</summary>
    public static class Scorch53
    {
        /// <summary>The reel's probe: mark frames drawn with this item's glow.</summary>
        public static int Drawn;

        /// <summary>The core's glow under scorchCore.on, else 0 (skin52's own rule).</summary>
        public static float Glow(GameConfig cfg) => Polish53.Item(cfg, "scorchCore") ? (float)Math.Max(0, Polish53.Num(cfg, "scorchCore", "glow", 0)) : 0f;
    }

    /// <summary>Item 4 (T-116, iceline's grade to 4/4 on the look meter). NIGHT52's a349 combat ON frames on n52-iceline: the walls' lift
    /// (Look52Lift: the rim and the SH probe's equator in the level hue) at x1.12 gave dark 42 / mid 54 / saturated 10 (under the 10-30 band:
    /// "no") / hue bins 4 = 3/4 (n52-r15); at x1.5, 29 / 67 / 20 / 4 = 2/4 (n52-r12); D-22 shipped gradeLevels [] (no lift, the grade off on
    /// iceline). Between the two runs mid rises ~0.34 points and saturated ~0.26 points per 0.01 of lift (linear between the two measured
    /// lifts: an estimate), so x1.14 lands ~54.7 / ~10.5: inside both bands, with dark ~41. With icelineGrade.on look52's grade list takes
    /// UnityTuning.Polish53IcelineLevels (iceline's ids) and iceline's slot the lift icelineGrade.lift; look52 off or -skygrindOff53: NIGHT52's
    /// (no grade on iceline). The meter is a349's (the lead runs it on n52-iceline); a153 prints the lift applied and its own frame's meter.</summary>
    public static class IcelineGrade53
    {
        /// <summary>The lift on V50 slot `slot` (iceline: UnityTuning.Polish53IcelineSlot) under icelineGrade.on, else 0 (NIGHT52's rule).</summary>
        public static float Lift(GameConfig cfg, int slot)
        {
            if (slot != UnityTuning.Polish53IcelineSlot || !Polish53.Item(cfg, "icelineGrade")) return 0f;
            double k = Polish53.Num(cfg, "icelineGrade", "lift", 0);
            return k > 0 ? (float)k : 0f;
        }

        /// <summary>Into look52's grade list (lower case ids): iceline's ids under icelineGrade.on.</summary>
        public static void AddLevels(GameConfig cfg, System.Collections.Generic.List<string> ids)
        {
            if (!Polish53.Item(cfg, "icelineGrade")) return;
            foreach (string id in UnityTuning.Polish53IcelineLevels) if (!ids.Contains(id)) ids.Add(id);
        }
    }

    /// <summary>T-122: the fire path compiled ahead at boot (see the file's header).</summary>
    public static class FirstShot53
    {
        /// <summary>The reel's probe: the warm ran, its methods, the ones that could not be compiled, the types found, its ms.</summary>
        public static bool Ran;
        public static int Methods, Skipped, Types, Missing;
        public static double Ms;
        public static bool OverBudget;

        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        /// <summary>The fx49 kit's sprites were built at this game's boot (n53-r1: the THUNDERCLAP's first shot still hitched 19.7 / 26.1 ms on
        /// a 4-5 ms median: its cone / boost is the run's first Fx49Kit.Spawn, which built the kit then: 4 Resources.Load and a material and a
        /// dynamic mesh per sprite x owner tone x brightness level).</summary>
        public static bool KitPrewarmed;
        public static double KitMs;

        /// <summary>EXT53 B (T-122, the THUNDERCLAP's first shot after n53-r2: 15.8-25.4 ms with the kit and the JIT warm): fx47's three sprite
        /// sheets (Fx47View.Prewarm53) built at the game's first frame with fx47 on, not by the THUNDERCLAP's first `fire` (OnFire47: under fx49
        /// the first call that builds that view; about 240 additive materials and dynamic meshes and ~2.7 MB of quad arrays). The reel prints it.</summary>
        public static bool Fx47Sheets;
        public static double Fx47Ms;

        /// <summary>From SurvivalGame's boot (after Warmup.Run), with firstShot.on: the fx49 kit's sprites built (every game: a level's boot makes
        /// a new kit), then, once per process, the listed types' methods compiled.</summary>
        public static void Boot(SurvivalGame g)
        {
            GameConfig cfg = g != null ? g.cfg : null;
            if (!Polish53.Item(cfg, "firstShot")) return;
            KitPrewarmed = false;
            if (Fx49.On(cfg))
            {
                var kw = Stopwatch.StartNew();
                Fx49Kit kit = Fx49Kit.Of(g);
                kit?.Prewarm53();
                KitPrewarmed = kit != null && kit.Built53;
                KitMs = kw.Elapsed.TotalMilliseconds;
                Debug.Log($"[polish53] first shot (T-122): the fx49 kit's sprites built at boot {(KitPrewarmed ? "yes" : "NO")} in {KitMs:0.0} ms");
            }
            if (Ran) return;
            Ran = true;
            var watch = Stopwatch.StartNew();
            double budget = UnityTuning.Polish53JitBudgetMs;
            Assembly game = typeof(FirstShot53).Assembly, core = typeof(Loadout).Assembly;
            foreach (string name in UnityTuning.Polish53JitTypes)
            {
                if (watch.Elapsed.TotalMilliseconds > budget) { OverBudget = true; break; }
                Type t = game.GetType(name) ?? core.GetType(name);
                if (t == null) { Missing++; continue; }
                Types++;
                Prepare(t, watch, budget, 0);
            }
            Ms = watch.Elapsed.TotalMilliseconds;
            Debug.Log($"[polish53] first shot (T-122): {Methods} methods of {Types} types compiled ahead ({Skipped} skipped, {Missing} names not found{(OverBudget ? ", the budget ran out" : "")}) in {Ms:0.0} ms");
        }

        static void Prepare(Type t, Stopwatch watch, double budget, int depth)
        {
            if (t.ContainsGenericParameters || depth > 3) return;
            foreach (MethodInfo m in t.GetMethods(All))
            {
                if (watch.Elapsed.TotalMilliseconds > budget) { OverBudget = true; return; } // a big type (the HUD) stops at the budget too
                if (m.IsAbstract || m.ContainsGenericParameters || (m.MethodImplementationFlags & MethodImplAttributes.InternalCall) != 0 || (m.Attributes & MethodAttributes.PinvokeImpl) != 0) continue;
                Compile(m);
            }
            foreach (ConstructorInfo c in t.GetConstructors(All))
                if (!c.IsStatic) Compile(c);
            foreach (Type n in t.GetNestedTypes(All))
            {
                if (watch.Elapsed.TotalMilliseconds > budget) { OverBudget = true; return; }
                Prepare(n, watch, budget, depth + 1); // the lambdas' closures, the iterators, the local types
            }
        }

        static void Compile(MethodBase m)
        {
            try
            {
                RuntimeHelpers.PrepareMethod(m.MethodHandle);
                m.MethodHandle.GetFunctionPointer(); // Mono: the native code's address (compiles the method when PrepareMethod did not)
                Methods++;
            }
            catch (Exception) { Skipped++; }
        }
    }
}
