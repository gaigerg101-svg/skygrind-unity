// NIGHT16 U3-29.5 (builder I): the impact layer's rules, pure and tested (Tests/EditMode/ImpactRulesTests.cs). From a moment
// (a kill, a headshot, a punch hit, a parry ...) to a freeze: the tier (design.impact.freeze), the longest request wins
// (the web's hitstop), each tier's cooldown (design.impact.cooldown), the budget (design.impact.budget: at most perSecond s
// frozen in any rolling second, at most perMinute of real time over a rolling minute; a request over it is cut to what is
// left, or skipped when less than a sim step is left, and logged; parries exempt; no single freeze over budget.maxFreeze,
// FEEL-DESIGN.md section 2: Smash's 0.5 s cap), and the slow-mos (design.impact.slowMo:
// never stacked, the strongest wins, at most one start in any minGap s; none while DOWN, paused or in a menu).
// Real time in, decisions out: no UnityEngine, no sim state. The same object runs in SurvivalGame's loop
// (SurvivalGame.Impact.cs through Game/ImpactFrames.cs) and in the headless ledger (Tools/SimHarness/Harness.Impact.cs):
//   - Watch(bus) once: the bus's moments (hit, kill, parry, roundClear and the sim's own `hitstop`) become this step's
//     requests (no allocation per call: a tier bit mask, a few fields);
//   - BeginFrame(frameDt, blocked) once a rendered frame while the sim runs: the real clock, the freeze counted down (its
//     last frame runs the remainder, so the frozen time is exactly the freeze granted), the slow-mo's scale; it returns the
//     share of the frame's time the sim gets (0 frozen, under 1 a slow-mo);
//   - EndStep(blocked, simHitstop) after each sim step: the step's requests resolved (true: a freeze begins now).
// The clock is real time while the sim runs (the loop doesn't call BeginFrame on a pause or a screen), so the budget's
// windows and the slow-mo's hold don't run on under a menu.
// A moment's tier, read from the web's payloads (Core/Economy.cs HitEvent / KillEvent):
//   - a punch or gauntlet hit: punchHit (punchKill when it killed);
//   - else a headshot: headshot (headshotKill);
//   - else a heavy body hit: the rail; a rocket's direct hit (the blast point within husk.aimHeight of the struck body's aim
//     point: on the body, not the floor beside it; HitEvent.distance is measured from the blast); a shotgun hit dealing at
//     least freeze.heavyPellets x weapons.shotgun.pelletDamage (the shot's pellets are summed per body before the hit
//     event: Combat.cs FireShotgun; the momentum and tag multipliers ride on top, so this is "heavyPellets pellets' worth");
//   - a kill (any of the above that killed, or the KillEvent; the sim's own feel.hitstopTime request maps here too);
//   - the sim's momentum melee hitstop (feel.momentumHitstop): momentum, the punch tier's floor (it has no cooldown);
//   - a parry: parry / perfectParry (the sim's own parry request is covered by these: the longer wins); inside their
//     cooldown (a chain), parryRepeat: the web's punch.parryHitstop, budgeted (NIGHT16 review finding 3);
//   - the headshot and heavy-hit tiers (not the kills) go to a shot's first hit only, and every budgeted freeze starts at
//     least design.impact.cooldown.any after the last freeze's start (NIGHT16 review finding 1: no 3-hitch chop);
//   - any other sim hitstop cause: sim, at the sim's own length.
// A wave's share (HitEvent.shock) earns no freeze of its own, but its kills count toward a multi-kill.
// Slow-mos: the round's last kill (a roundClear in the same step as a kill of yours), a PERFECT PARRY's tail (after its
// freeze), a multi-kill (slowMo.multiKill.count kills sharing one shot id: one trigger pull or blast).
// NIGHT18 U3-31.3 (builder X): the freeze tiers come from a table (ImpactRules(cfg, table), UseFreezeTable): design.impact.freeze
// (the default; NIGHT18.md M4's ladder) or design.impact.n17.freeze (NIGHT17's, the Game's FeelAB off side). No rule changed.
using System;

namespace Skygrind.Core
{
    public enum ImpactTier { None, Kill, HeavyHit, Headshot, HeadshotKill, PunchHit, PunchKill, Parry, PerfectParry, Momentum, Sim, ParryRepeat, BodyHit, Gauntlet42, Fusion43 }

    public enum SlowMoCause { None, LastKill, PerfectParry, MultiKill, CorePunch } // NIGHT47 F: CorePunch (fx47; the Game requests it on wick47CorePunch)

    public enum HitFreezeMode { Off, Light, Full }

    public sealed class ImpactRules
    {
        public const int TierCount = 15, SlowMoCount = 5;

        /// <summary>The `impactFreeze` cause names (FeelKeys.ImpactFreezeEvent.cause), by tier.</summary>
        public static readonly string[] TierNames = { "none", "kill", "heavyHit", "headshot", "headshotKill", "punchHit", "punchKill", "parry", "perfectParry", "momentum", "sim", "parryRepeat", "bodyHit", "gauntlet42", "fusion43" };

        // --- NIGHT43 builder W1 fusion43 (design.n43.fusion43; G19 "a powerful special feedback response"; additive) -------------------------------
        // The sim asks a FUSION's freeze through its own Hitstop with the cause "fusion43" (Core/Fusion43.cs: hitstopS 0.22 s, bossHitstopS 0.28 s
        // on a boss body; the sim emits it only with the row on). That request becomes the Fusion43 tier at the sim's length (EndStep's
        // simHitstop), in place of the unknown-cause Sim tier: out of the shared gap and the shared minute like Gauntlet42, inside a rolling-minute
        // share of its own (design.n43.fusion43.perMinute) counted together with the gauntlet's freezes (FrozenIn which 3). Off: never asked.
        bool fusion43Asked;
        double f43PerMinute;

        // --- NIGHT42 builder F1 impact42 (design.n42.impact42; G9 "hits and kills need to feel more satifying", G15 "a greater or mor epowerful
        // effect when hitting it or any other enemy iwth gaughtlent"; hitfeel-web.md's "Ten concrete numbers") ------------------------------------
        // With `impact42` on (the Game sets it from the run's n42.impact42, as crunch36): a gauntlet hit (the sim's `gauntletHit42` event,
        // Game/SurvivalSim.N42.Impact.cs: the enemy's class small / big / boss) requests Gauntlet42 at its class's length (a kill: at least
        // gauntletKillS), in place of the punch tiers; its own cooldown and its own rolling-minute share (gauntletPerMinute), out of the shared
        // gap and the shared minute (like crunch36's body hits), cut to what is left of its share or skipped under a sim step. A gun's body hit
        // freezes by weapon (bodyHitByWeapon; else crunch36's bodyHitS) inside crunch36's body budget; the heavy hit, the kills and the headshot
        // kill freeze longer (heavyHitS, killS, headshotKillS, punchKillS, killCapS). Off (or no design.n42.impact42 numbers): the rules as before,
        // bit for bit (the tier is never requested; the ring never holds a gauntlet freeze).
        public bool impact42;
        bool has42;
        double g42HitS, g42BigS, g42BossS, g42KillS, g42PerMinute, heavy42S, kill42S, headKill42S, punchKill42S, killCap42S;
        System.Collections.Generic.Dictionary<string, double> body42ByWeapon;
        double gauntletPending42, bodyPending42; // this step's longest gauntlet / body-hit request (s)

        /// <summary>NIGHT42 F1: the gauntlet's freeze seconds for a class (GauntletHit42Event.Small / Big / Boss) and a kill (0 with no row).</summary>
        public double Gauntlet42Seconds(int cls, bool killed)
        {
            double s = cls == GauntletHit42Event.Boss ? g42BossS : cls == GauntletHit42Event.Big ? g42BigS : g42HitS;
            return killed && g42KillS > s ? g42KillS : s;
        }

        // --- NIGHT36 builder F crunch36 (design.n36.crunch36; REVIEW.md root cause 3: a KESTREL body hit had no freeze) ------------------------------------
        // With `crunch36` on (the Game sets it from the run's n36.crunch36): a gun's non-kill body hit (a shot's first hit, not melee, not a wave's share)
        // requests BodyHit (bodyHitS, its own cooldown bodyHitCooldownS); a punch hit freezes punchHitS (ULTRAKILL's 0.1 s) instead of the table's. A body-hit
        // freeze never starts the shared gap (cooldown.any) nor counts toward the rolling minute (perMinute), so it can't starve the kill / headshot / punch
        // freezes; it takes at most bodyBudgetPerSecond of any rolling second and fits the total perSecond with everything else (skipped, never cut).
        // Off: no BodyHit request and the table's punchHit: the rules as before, bit for bit.
        public bool crunch36;
        readonly double bodyHitS36, bodyShare36, punchHitS36;
        readonly bool has36;

        /// <summary>The `slowMo` cause names (FeelKeys.SlowMoEvent.cause), by cause.</summary>
        public static readonly string[] SlowMoNames = { "none", "lastKill", "perfectParry", "multiKill", "corePunch" };

        // --- settings (the Game sets them every frame from SettingsView; the automation keeps the design defaults) -------
        /// <summary>HIT FREEZE: off freezes nothing (the sim's own requests included), light scales every freeze.</summary>
        public HitFreezeMode hitFreeze;

        /// <summary>SLOW-MO on and REDUCED MOTION off.</summary>
        public bool slowMoOn;

        // --- the last EndStep's outcome (the director reads it; reset by every EndStep) ----------------------------------
        public ImpactTier granted;
        public double grantedSeconds;
        /// <summary>The freeze was cut to the budget left.</summary>
        public bool cut;
        /// <summary>The tiers skipped this step (1 &lt;&lt; tier): inside their cooldown, or over the budget.</summary>
        public int skippedMask;
        public SlowMoCause slowMoStarted;
        public double slowMoScale, slowMoHold;
        /// <summary>A slow-mo was asked for and refused (inside minGap, or blocked: DOWN, a pause, a menu).</summary>
        public bool slowMoSkipped;
        public SlowMoCause slowMoSkippedCause;

        // --- the ledger (counts since NewRun) ------------------------------------------------------------------------------
        public readonly int[] freezes = new int[TierCount], skips = new int[TierCount];
        public readonly double[] frozenSeconds = new double[TierCount];
        public readonly int[] slowMos = new int[SlowMoCount], slowMoSkips = new int[SlowMoCount];
        public int cuts;

        // --- config, read once --------------------------------------------------------------------------------------------
        readonly double[] tierSeconds = new double[TierCount], cooldown = new double[TierCount];
        readonly double perSecond, perMinute, maxFreeze, lightScale, minCut, rocketDirectReach, minGap, anyGap;
        double heavyShotgunDamage; // NIGHT18: from the freeze table (UseFreezeTable)
        readonly HitFreezeMode defaultHitFreeze;
        readonly bool defaultSlowMo;
        readonly bool parryExempt;
        readonly double[] slowScale = new double[SlowMoCount], slowHoldS = new double[SlowMoCount], slowEase = new double[SlowMoCount];
        readonly int multiCount;

        // --- state --------------------------------------------------------------------------------------------------------
        double clock, freezeLeft, scale = 1, lastFreezeAt, prevFreezeAt;
        int lastFreezeRank; // TierRank of the last freeze granted (the gap's bypass)

        /// <summary>
        /// NIGHT16 (the lead's refinement of the shared gap): each tier's rank for the bypass: kill &lt; heavyHit &lt; headshot &lt;
        /// headshotKill &lt; punchHit &lt; punchKill, the parries above all (exempt: nothing bypasses after one); the momentum
        /// floor, an unknown sim request and a parry repeat rank 0 (they never bypass).
        /// </summary>
        static readonly int[] TierRank = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 6, 6 }; // NIGHT42 F1: Gauntlet42 ranks as a punch kill; NIGHT43 W1: Fusion43 as the gauntlet

        /// <summary>At most two freeze starts in any rolling second (the budget's own window): s.</summary>
        const double BypassWindow = 1;
        int lastHitShot = -1; // the shot id of the last hit: a headshot / heavy-hit tier only for a shot's first hit
        readonly double[] lastAt = new double[TierCount];
        int requestMask;
        bool simUnknown, stepKill;
        int stepKills; // NIGHT17 round 5: the player's kills this step (the kill freeze grows with them)
        double killPerExtra, killCap; // NIGHT17 round 5: design.impact.freeze.killPerExtra / killCap (0: the old rule); NIGHT18: the table's
        SlowMoCause slowPending;
        int multiShot = -1, multiKills;
        bool multiDone;
        // The slow-mo running: its start on the clock (after a freeze granted with it), its scale, hold and ease.
        SlowMoCause slowCause;
        double slowStart, slowS, slowHold, slowEaseNow, lastSlowStart;

        // The budgeted freezes, oldest first (a ring): [start, start + seconds] on the clock. Freezes never overlap (no step
        // runs during one, so no request can arrive). 1024: the minute's budget over the shortest freeze (a sim step) with
        // room to spare.
        const int RingSize = 1024;
        readonly double[] ringStart = new double[RingSize], ringSeconds = new double[RingSize];
        readonly bool[] ringBody = new bool[RingSize]; // NIGHT36 crunch36: a body-hit freeze (out of the minute's budget)
        readonly bool[] ringGauntlet = new bool[RingSize]; // NIGHT42 F1 impact42: a gauntlet freeze (its own minute share)
        int ringHead, ringCount;

        public ImpactRules(GameConfig cfg) : this(cfg, FreezeTable)
        {
        }

        /// <summary>NIGHT18: the rules with their freeze tiers from `freezeTable` (a design.impact.freeze-shaped block's prefix).</summary>
        public ImpactRules(GameConfig cfg, string freezeTable)
        {
            const string d = "design.impact.";
            UseFreezeTable(cfg, freezeTable); // NIGHT18 U3-31.3: the tiers, the kill's growth and the heavy shotgun line
            tierSeconds[(int)ImpactTier.Momentum] = cfg.Num("feel.momentumHitstop"); // the web's melee floor (the sim requests it)
            cooldown[(int)ImpactTier.Kill] = cfg.Num(d + "cooldown.kill");
            cooldown[(int)ImpactTier.HeavyHit] = cfg.Num(d + "cooldown.heavyHit");
            cooldown[(int)ImpactTier.Headshot] = cfg.Num(d + "cooldown.headshot");
            cooldown[(int)ImpactTier.HeadshotKill] = cfg.Num(d + "cooldown.headshotKill");
            cooldown[(int)ImpactTier.PunchHit] = cfg.Num(d + "cooldown.punchHit");
            cooldown[(int)ImpactTier.PunchKill] = cfg.Num(d + "cooldown.punchKill");
            cooldown[(int)ImpactTier.Parry] = cfg.Num(d + "cooldown.parry");
            cooldown[(int)ImpactTier.PerfectParry] = cfg.Num(d + "cooldown.perfectParry");
            perSecond = cfg.Num(d + "budget.perSecond");
            maxFreeze = cfg.Num(d + "budget.maxFreeze");
            perMinute = cfg.Num(d + "budget.perMinute") * 60;
            parryExempt = cfg.Bool(d + "budget.parryExempt");
            lightScale = cfg.Num(d + "settings.lightScale");
            minCut = cfg.Dt; // a freeze cut below one sim step isn't a freeze: skipped instead
            rocketDirectReach = cfg.Num("husk.aimHeight");
            SlowFrom(cfg, SlowMoCause.LastKill, d + "slowMo.lastKill.");
            SlowFrom(cfg, SlowMoCause.PerfectParry, d + "slowMo.perfectParry.");
            SlowFrom(cfg, SlowMoCause.MultiKill, d + "slowMo.multiKill.");
            const string core47 = "design.n47.fx47.numbers.corePunchSlow."; // NIGHT47 F (G7, W's hand-off): THE WICK's core punch (absent: scale 1, never requested)
            if (cfg.Has(core47 + "scale")) SlowFrom(cfg, SlowMoCause.CorePunch, core47);
            else slowScale[(int)SlowMoCause.CorePunch] = 1;
            multiCount = (int)cfg.Num(d + "slowMo.multiKill.count");
            minGap = cfg.Num(d + "slowMo.minGap");
            anyGap = cfg.Num(d + "cooldown.any");
            tierSeconds[(int)ImpactTier.ParryRepeat] = cfg.Num("punch.parryHitstop"); // the web's own parry hitstop (before design.ultra)
            hitFreeze = defaultHitFreeze = ParseHitFreeze(cfg.Str(d + "settings.hitFreezeDefault"));
            slowMoOn = defaultSlowMo = cfg.Bool(d + "settings.slowMoDefault");
            const string c36 = "design.n36.crunch36.";
            has36 = cfg.Has(c36 + "bodyHitS");
            if (has36)
            {
                bodyHitS36 = cfg.Num(c36 + "bodyHitS");
                bodyShare36 = cfg.Num(c36 + "bodyBudgetPerSecond");
                punchHitS36 = cfg.Num(c36 + "punchHitS");
                tierSeconds[(int)ImpactTier.BodyHit] = bodyHitS36;
                cooldown[(int)ImpactTier.BodyHit] = cfg.Num(c36 + "bodyHitCooldownS");
            }
            Impact42From(cfg);
            f43PerMinute = cfg.Has("design.n43.fusion43.perMinute") ? cfg.Num("design.n43.fusion43.perMinute") * 60 : 0; // NIGHT43 W1 fusion43
            NewRun();
        }

        /// <summary>NIGHT42 F1: design.n42.impact42's numbers, read once (has42 false when the row holds none: the tier is never asked).</summary>
        void Impact42From(GameConfig cfg)
        {
            const string r = "design.n42.impact42.";
            has42 = cfg.Has(r + "gauntletHitS");
            if (!has42) return;
            g42HitS = cfg.Num(r + "gauntletHitS");
            g42BigS = cfg.Num(r + "gauntletBigS");
            g42BossS = cfg.Num(r + "gauntletBossS");
            g42KillS = cfg.Num(r + "gauntletKillS");
            g42PerMinute = cfg.Num(r + "gauntletPerMinute") * 60;
            cooldown[(int)ImpactTier.Gauntlet42] = cfg.Num(r + "gauntletCooldownS");
            heavy42S = cfg.Num(r + "heavyHitS");
            kill42S = cfg.Num(r + "killS");
            headKill42S = cfg.Num(r + "headshotKillS");
            punchKill42S = cfg.Num(r + "punchKillS");
            killCap42S = cfg.Num(r + "killCapS");
            body42ByWeapon = new System.Collections.Generic.Dictionary<string, double>();
            foreach (var kv in cfg.Obj(r + "bodyHitByWeapon"))
                if (!kv.Key.StartsWith("_", StringComparison.Ordinal) && !kv.Key.EndsWith("_why", StringComparison.Ordinal)) body42ByWeapon[kv.Key] = PortData.Num(kv.Value);
        }

        bool On42 => impact42 && has42;

        /// <summary>NIGHT18 U3-31.3: the freeze table the default constructor reads (design.impact.freeze).</summary>
        public const string FreezeTable = "design.impact.freeze.";

        /// <summary>
        /// NIGHT18 U3-31.3 (builder X): the freeze tiers from `table` (a design.impact.freeze-shaped block's prefix, e.g.
        /// design.impact.n17.freeze. for NIGHT17's), in place: the Game's FeelAB flip (Game/ImpactFrames.cs) swaps tables live;
        /// the clock, the cooldowns, the budget, the slow-mo and the ledger stay. Reads the config (strings): never per frame.
        /// </summary>
        public void UseFreezeTable(GameConfig cfg, string table)
        {
            tierSeconds[(int)ImpactTier.Kill] = cfg.Num(table + "kill");
            killPerExtra = cfg.Has(table + "killPerExtra") ? cfg.Num(table + "killPerExtra") : 0; // NIGHT17 round 5
            killCap = cfg.Has(table + "killCap") ? cfg.Num(table + "killCap") : 0;
            tierSeconds[(int)ImpactTier.HeavyHit] = cfg.Num(table + "heavyHit");
            tierSeconds[(int)ImpactTier.Headshot] = cfg.Num(table + "headshot");
            tierSeconds[(int)ImpactTier.HeadshotKill] = cfg.Num(table + "headshotKill");
            tierSeconds[(int)ImpactTier.PunchHit] = cfg.Num(table + "punchHit");
            tierSeconds[(int)ImpactTier.PunchKill] = cfg.Num(table + "punchKill");
            tierSeconds[(int)ImpactTier.Parry] = cfg.Num(table + "parry");
            tierSeconds[(int)ImpactTier.PerfectParry] = cfg.Num(table + "perfectParry");
            heavyShotgunDamage = cfg.Num(table + "heavyPellets") * cfg.Num("weapons.shotgun.pelletDamage");
        }

        void SlowFrom(GameConfig cfg, SlowMoCause c, string at)
        {
            slowScale[(int)c] = cfg.Num(at + "scale");
            slowHoldS[(int)c] = cfg.Num(at + "hold");
            slowEase[(int)c] = cfg.Num(at + "ease");
        }

        /// <summary>The HIT FREEZE setting's value: "off" | "light" | "full" (anything else, unset included: full).</summary>
        public static HitFreezeMode ParseHitFreeze(string s) =>
            s == FeelKeys.HitFreezeOff ? HitFreezeMode.Off : s == FeelKeys.HitFreezeLight ? HitFreezeMode.Light : HitFreezeMode.Full;

        /// <summary>Real seconds while the sim runs (BeginFrame's sum).</summary>
        public double Clock => clock;

        /// <summary>The freeze left (real s).</summary>
        public double FreezeLeft => freezeLeft;

        /// <summary>The last BeginFrame's share: 0 frozen, under 1 a slow-mo (or a freeze's last frame), 1 real time.</summary>
        public double Scale => scale;

        public bool SlowMoActive => slowCause != SlowMoCause.None;

        public SlowMoCause SlowMoRunning => slowCause;

        /// <summary>The slow-mo's own time scale now (1: none), without the freeze.</summary>
        public double SlowScale => SlowAt(clock);

        /// <summary>
        /// NIGHT16 review (finding 2): the settings, once a frame. `automation` (the smoke, the AUTOPILOT, the farm, the strips:
        /// noLab and not the attract) plays design.impact.settings' defaults whatever the player saved, REDUCED MOTION
        /// ignored, so the gates always exercise the layer; a player (and the attract's viewer) plays HIT FREEZE and SLOW-MO as
        /// saved, the slow-mo off with REDUCED MOTION.
        /// </summary>
        public void ApplySettings(bool automation, string hitFreezeSetting, bool slowMoSetting, bool reducedMotion)
        {
            if (automation)
            {
                hitFreeze = defaultHitFreeze;
                slowMoOn = defaultSlowMo;
                return;
            }
            hitFreeze = ParseHitFreeze(hitFreezeSetting);
            slowMoOn = slowMoSetting && !reducedMotion;
        }

        /// <summary>NIGHT16 review (finding 5): the way to TITLE: the freeze and the slow-mo end now (the cooldowns and the budget
        /// stay until the next run's NewRun).</summary>
        public void Halt()
        {
            freezeLeft = 0;
            scale = 1;
            slowCause = SlowMoCause.None;
            ClearPending();
        }

        /// <summary>A new run (or a new composition): no freeze, no slow-mo, no cooldown, no budget spent, the ledger zeroed.</summary>
        public void NewRun()
        {
            clock = 0;
            freezeLeft = 0;
            scale = 1;
            lastFreezeAt = prevFreezeAt = double.NegativeInfinity;
            lastFreezeRank = 0;
            lastHitShot = -1;
            for (int i = 0; i < TierCount; i++)
            {
                lastAt[i] = double.NegativeInfinity;
                freezes[i] = skips[i] = 0;
                frozenSeconds[i] = 0;
            }
            for (int i = 0; i < SlowMoCount; i++) slowMos[i] = slowMoSkips[i] = 0;
            cuts = 0;
            ringHead = ringCount = 0;
            slowCause = SlowMoCause.None;
            lastSlowStart = double.NegativeInfinity;
            multiShot = -1;
            multiKills = 0;
            multiDone = false;
            ClearPending();
            ClearOutcome();
        }

        /// <summary>Forget this step's requests (the loop calls it before stepping: a moment outside the loop's steps earns nothing).</summary>
        public void ClearPending()
        {
            requestMask = 0;
            simUnknown = false;
            stepKill = false;
            stepKills = 0;
            slowPending = SlowMoCause.None;
            gauntletPending42 = bodyPending42 = 0; // NIGHT42 F1
            fusion43Asked = false; // NIGHT43 W1
        }

        void ClearOutcome()
        {
            granted = ImpactTier.None;
            grantedSeconds = 0;
            cut = false;
            skippedMask = 0;
            slowMoStarted = SlowMoCause.None;
            slowMoScale = slowMoHold = 0;
            slowMoSkipped = false;
            slowMoSkippedCause = SlowMoCause.None;
        }

        /// <summary>
        /// Once a rendered frame while the sim runs (Playing or DOWN): the clock, the freeze, the slow-mo. `blocked` (DOWN):
        /// a running slow-mo ends at once. Returns the share of the frame the sim gets: 0 while frozen, the remainder on a
        /// freeze's last frame (so the frozen time is exactly the freeze), x the slow-mo's scale.
        /// </summary>
        public double BeginFrame(double frameDt, bool blocked)
        {
            if (frameDt <= 0) return scale = freezeLeft > 0 ? 0 : SlowAt(clock);
            clock += frameDt;
            double run = 1;
            if (freezeLeft > 0)
            {
                if (freezeLeft >= frameDt)
                {
                    freezeLeft -= frameDt;
                    return scale = 0;
                }
                run = (frameDt - freezeLeft) / frameDt;
                freezeLeft = 0;
            }
            if (blocked) slowCause = SlowMoCause.None;
            return scale = run * SlowAt(clock);
        }

        /// <summary>The slow-mo's scale at clock time `t`: held, then an ease back (smoothstep) to 1; it ends there.</summary>
        double SlowAt(double t)
        {
            if (slowCause == SlowMoCause.None) return 1;
            double u = t - slowStart;
            if (u < slowHold) return slowS;
            if (u < slowHold + slowEaseNow)
            {
                double k = (u - slowHold) / slowEaseNow;
                return slowS + (1 - slowS) * k * k * (3 - 2 * k);
            }
            slowCause = SlowMoCause.None;
            return 1;
        }

        /// <summary>
        /// NIGHT17 round 5 (builder X): the kill tier's freeze for a step with `kills` kills: base + perExtra x (kills - 1),
        /// capped at `cap` (never under the base; perExtra 0: the base, the old rule).
        /// </summary>
        public static double KillFreezeSeconds(double baseS, int kills, double perExtra, double cap)
        {
            if (kills <= 1 || !(perExtra > 0)) return baseS;
            return Math.Max(baseS, Math.Min(cap, baseS + perExtra * (kills - 1)));
        }

        /// <summary>A freeze request for this step, at its tier's length (design.impact.freeze).</summary>
        public void Request(ImpactTier tier) => requestMask |= 1 << (int)tier;

        /// <summary>A slow-mo request for this step: the strongest (lowest scale) of the step's requests is the one asked.</summary>
        public void RequestSlowMo(SlowMoCause cause)
        {
            if (cause == SlowMoCause.None) return;
            if (slowPending == SlowMoCause.None || slowScale[(int)cause] < slowScale[(int)slowPending]) slowPending = cause;
        }

        /// <summary>
        /// After each sim step: the step's requests resolved. `blocked`: the run isn't Playing (DOWN; the loop doesn't step on
        /// a pause or a screen), so no slow-mo starts. `simHitstop`: the sim's own request this step (sim.hitstopLeft when
        /// sim.hitstopRequested), the length of a cause this class doesn't know. True when a freeze begins now.
        /// </summary>
        public bool EndStep(bool blocked, double simHitstop = 0)
        {
            ClearOutcome();
            if (fusion43Asked && simHitstop > 0)
            {
                // NIGHT43 W1 fusion43: the sim's request is a FUSION's (the longest of the step's sim requests: it covers a heavy41 one too)
                tierSeconds[(int)ImpactTier.Fusion43] = simHitstop;
                Request(ImpactTier.Fusion43);
            }
            else if (simUnknown && simHitstop > 0)
            {
                tierSeconds[(int)ImpactTier.Sim] = simHitstop;
                Request(ImpactTier.Sim);
            }
            if (requestMask != 0 && hitFreeze != HitFreezeMode.Off) ResolveFreeze();
            if (slowPending != SlowMoCause.None) ResolveSlowMo(blocked);
            ClearPending();
            return granted != ImpactTier.None;
        }

        void ResolveFreeze()
        {
            ImpactTier best = ImpactTier.None;
            double bestS = 0;
            int served = 0;
            for (int t = 1; t < TierCount; t++)
            {
                int bit = 1 << t;
                if ((requestMask & bit) == 0) continue;
                if (cooldown[t] > 0 && clock - lastAt[t] < cooldown[t])
                {
                    skippedMask |= bit;
                    skips[t]++;
                    // NIGHT16 review (finding 3): a parry inside its cooldown (a chain at the punch's cycle) gets the web's own
                    // short parry hitstop instead, budgeted (ParryRepeat is the last tier: this loop reaches it).
                    if (t == (int)ImpactTier.Parry || t == (int)ImpactTier.PerfectParry) requestMask |= 1 << (int)ImpactTier.ParryRepeat;
                    continue;
                }
                served |= bit;
                // NIGHT17 round 5: the kill tier grows with the step's kills (a blast's horde freezes longer than one kill)
                double ts = t == (int)ImpactTier.Kill ? KillFreezeSeconds(tierSeconds[t], stepKills, killPerExtra, killCap)
                    : t == (int)ImpactTier.PunchHit && crunch36 && has36 ? punchHitS36 : tierSeconds[t]; // NIGHT36 crunch36: ULTRAKILL's 0.1 s punch
                if (On42) ts = Tier42Seconds(t, ts); // NIGHT42 F1 impact42: the longer freezes (off: ts as before)
                if (ts > bestS || (t == (int)ImpactTier.Fusion43 && ts > 0 && ts >= bestS)) // NIGHT43 W1: a FUSION wins a tie (the gauntlet kill's 0.22 s)
                {
                    best = (ImpactTier)t;
                    bestS = ts;
                }
            }
            if (best == ImpactTier.None) return;
            double s = Math.Min(bestS * (hitFreeze == HitFreezeMode.Light ? lightScale : 1), maxFreeze); // the hard cap, parries included
            if (best == ImpactTier.Gauntlet42 || best == ImpactTier.Fusion43)
            {
                // NIGHT42 F1 impact42: no shared gap, out of the shared minute; its own rolling-minute share, cut to what is left of it
                // (NIGHT43 W1 fusion43: the FUSION's share, the gauntlet's and the FUSION's freezes counted together)
                double room = (best == ImpactTier.Fusion43 ? f43PerMinute : g42PerMinute) - FrozenIn(clock - 60, clock, 3);
                if (s > room)
                {
                    if (room >= minCut)
                    {
                        s = room;
                        cut = true;
                        cuts++;
                    }
                    else
                    {
                        skippedMask |= 1 << (int)best;
                        skips[(int)best]++;
                        return;
                    }
                }
                ringStart[ringHead] = clock;
                ringSeconds[ringHead] = s;
                ringBody[ringHead] = false;
                ringGauntlet[ringHead] = true;
                ringHead = (ringHead + 1) % RingSize;
                if (ringCount < RingSize) ringCount++;
                granted = best;
                grantedSeconds = s;
                prevFreezeAt = lastFreezeAt;
                lastFreezeAt = clock;
                lastFreezeRank = TierRank[(int)best];
                freezes[(int)best]++;
                frozenSeconds[(int)best] += s;
                for (int t = 1; t < TierCount; t++)
                    if ((served & (1 << t)) != 0) lastAt[t] = clock;
                if (s > freezeLeft) freezeLeft = s;
                return;
            }
            if (best == ImpactTier.BodyHit)
            {
                // NIGHT36 crunch36: no shared gap, no rolling minute; its own share of the second and the total second, else skipped (never cut)
                double room = Math.Min(perSecond - FrozenIn(clock - 1, clock, 0), bodyShare36 - FrozenIn(clock - 1, clock, 2));
                if (s > room + 1e-12)
                {
                    skippedMask |= 1 << (int)best;
                    skips[(int)best]++;
                    return;
                }
                ringStart[ringHead] = clock;
                ringSeconds[ringHead] = s;
                ringBody[ringHead] = true;
                ringGauntlet[ringHead] = false;
                ringHead = (ringHead + 1) % RingSize;
                if (ringCount < RingSize) ringCount++;
                granted = best;
                grantedSeconds = s;
                freezes[(int)best]++;
                frozenSeconds[(int)best] += s;
                lastAt[(int)best] = clock;
                if (s > freezeLeft) freezeLeft = s;
                return;
            }
            bool exempt = parryExempt && (best == ImpactTier.Parry || best == ImpactTier.PerfectParry);
            if (!exempt)
            {
                // NIGHT16 review (finding 1): the shared gap: at least cooldown.any between the starts of any two freezes (a
                // budgeted one never follows another, or a parry's, sooner), so a horde or automatic headshots never chop.
                // NIGHT16 (the lead's refinement): a strictly higher tier than the last freeze (TierRank) may start inside the gap,
                // so a horde's ordinary kills don't swallow the best moments; it restarts the gap. Every budgeted start (a bypass
                // or not) also needs the freeze before the last one a whole second back (BypassWindow): at most two starts in
                // any rolling second, so one bypass per gap at most. Same or lower tiers inside the gap are skipped.
                bool inGap = clock - lastFreezeAt < anyGap;
                bool bypass = inGap && TierRank[(int)best] > lastFreezeRank;
                if ((inGap && !bypass) || clock - prevFreezeAt < BypassWindow)
                {
                    skippedMask |= 1 << (int)best;
                    skips[(int)best]++;
                    return;
                }
                double left =Math.Min(perSecond - FrozenBetween(clock - 1, clock), perMinute - FrozenIn(clock - 60, clock, 1)); // NIGHT36: body hits out of the minute
                if (s > left)
                {
                    if (left >= minCut)
                    {
                        s = left;
                        cut = true;
                        cuts++;
                    }
                    else
                    {
                        skippedMask |= 1 << (int)best;
                        skips[(int)best]++;
                        return;
                    }
                }
                ringStart[ringHead] = clock;
                ringSeconds[ringHead] = s;
                ringBody[ringHead] = false;
                ringGauntlet[ringHead] = false;
                ringHead = (ringHead + 1) % RingSize;
                if (ringCount < RingSize) ringCount++;
            }
            granted = best;
            grantedSeconds = s;
            prevFreezeAt = lastFreezeAt;
            lastFreezeAt = clock;
            lastFreezeRank = TierRank[(int)best];
            freezes[(int)best]++;
            frozenSeconds[(int)best] += s;
            for (int t = 1; t < TierCount; t++)
                if ((served & (1 << t)) != 0) lastAt[t] = clock; // every tier this freeze served starts its cooldown
            if (s > freezeLeft) freezeLeft = s; // the longest wins
        }

        void ResolveSlowMo(bool blocked)
        {
            SlowMoCause c = slowPending;
            if (!slowMoOn) return; // SLOW-MO off or REDUCED MOTION: nothing asked, nothing logged
            if (blocked || clock - lastSlowStart < minGap)
            {
                slowMoSkipped = true;
                slowMoSkippedCause = c;
                slowMoSkips[(int)c]++;
                return;
            }
            slowCause = c;
            slowStart = clock + (granted != ImpactTier.None ? grantedSeconds : 0); // after the freeze granted with it
            lastSlowStart = clock;
            slowS = slowScale[(int)c];
            slowHold = slowHoldS[(int)c];
            slowEaseNow = slowEase[(int)c];
            slowMoStarted = c;
            slowMoScale = slowS;
            slowMoHold = slowHold;
            slowMos[(int)c]++;
        }

        /// <summary>NIGHT42 F1 impact42: a tier's length with the row on (`ts`: the length the rules as before give it).</summary>
        double Tier42Seconds(int t, double ts)
        {
            switch ((ImpactTier)t)
            {
                case ImpactTier.Gauntlet42: return gauntletPending42;
                case ImpactTier.BodyHit: return bodyPending42 > 0 ? bodyPending42 : ts;
                case ImpactTier.HeavyHit: return Math.Max(ts, heavy42S);
                case ImpactTier.Kill: return Math.Max(ts, KillFreezeSeconds(kill42S, stepKills, killPerExtra, killCap42S));
                case ImpactTier.HeadshotKill: return Math.Max(ts, headKill42S);
                case ImpactTier.PunchKill: return Math.Max(ts, punchKill42S);
                default: return ts;
            }
        }

        /// <summary>The budgeted freezes' real seconds inside [from, to] on the clock (parries exempt aren't counted).</summary>
        public double FrozenBetween(double from, double to) => FrozenIn(from, to, 0);

        /// <summary>NIGHT36 crunch36: the budgeted freezes' seconds inside [from, to]: `which` 0 all, 1 all but body hits, 2 body hits only.</summary>
        public double FrozenIn(double from, double to, int which)
        {
            double sum = 0;
            for (int k = 0; k < ringCount; k++)
            {
                int i = (ringHead - 1 - k + RingSize) % RingSize;
                double a = ringStart[i], b = a + ringSeconds[i];
                if (b <= from) break; // older ones end earlier still
                if ((which == 1 && (ringBody[i] || ringGauntlet[i])) || (which == 2 && !ringBody[i]) || (which == 3 && !ringGauntlet[i])) continue; // NIGHT42 F1: 3 the gauntlet's only; its freezes are out of the shared minute
                double lo = a > from ? a : from, hi = b < to ? b : to;
                if (hi > lo) sum += hi - lo;
            }
            return sum;
        }

        // --- the bus ------------------------------------------------------------------------------------------------------

        /// <summary>Listen to the sim's bus (once: the handlers are allocated here, never per call).</summary>
        public void Watch(EventBus events)
        {
            events.On<HitEvent>("hit", OnHit);
            events.On<KillEvent>("kill", OnKill);
            events.On<HitEvent>("parry", OnParry);
            events.On("roundClear", OnRoundClearObj);
            events.On("hitstop", OnSimHitstopObj);
            events.On<GauntletHit42Event>(GauntletHit42Event.Name, OnGauntletHit42); // NIGHT42 F1 (emitted only with impact42 on)
        }

        /// <summary>NIGHT42 F1 impact42: the sim's `gauntletHit42` (one per gauntlet hit of the player's): its class's freeze, the step's longest.</summary>
        public void OnGauntletHit42(GauntletHit42Event e)
        {
            if (!On42 || e.owner != "player") return;
            double s = Gauntlet42Seconds(e.cls, e.killed);
            if (s > gauntletPending42) gauntletPending42 = s;
            Request(ImpactTier.Gauntlet42);
        }

        void OnRoundClearObj(object _) => OnRoundClear();

        void OnSimHitstopObj(object cause) => OnSimHitstop(cause as string);

        /// <summary>The web's `hit`: the tier it earns, and the multi-kill count by shot.</summary>
        public void OnHit(HitEvent e)
        {
            if (e.owner != "player") return;
            if (e.killed && e.shot > 0)
            {
                if (e.shot == multiShot) multiKills++;
                else
                {
                    multiShot = e.shot;
                    multiKills = 1;
                    multiDone = false;
                }
                if (!multiDone && multiKills >= multiCount)
                {
                    multiDone = true;
                    RequestSlowMo(SlowMoCause.MultiKill);
                }
            }
            if (e.shock) return; // a wave's share: no freeze of its own
            // NIGHT16 review (finding 1): the non-kill headshot and heavy-hit tiers go to a shot's first hit only (a rail through
            // a line, a shotgun's bodies, a blast: one moment, not one per body).
            bool first = e.shot != lastHitShot;
            lastHitShot = e.shot;
            string w = e.weapon;
            if (w == "gauntlet" && On42) { } // NIGHT42 F1 impact42: the gauntlet's freeze is its own tier (OnGauntletHit42), not a punch's
            else if (w == "punch" || w == "gauntlet") Request(e.killed ? ImpactTier.PunchKill : ImpactTier.PunchHit);
            else if (e.headshot)
            {
                if (e.killed) Request(ImpactTier.HeadshotKill);
                else if (first) Request(ImpactTier.Headshot);
            }
            else if (first && (w == "rail" || (w == "rocket" && e.distance <= rocketDirectReach) || (w == "shotgun" && e.damage >= heavyShotgunDamage))) Request(ImpactTier.HeavyHit);
            else if ((crunch36 || On42) && has36 && first && !e.killed && !e.melee)
            {
                // NIGHT36 crunch36: every gun's body hit catches; NIGHT42 F1 impact42: at its weapon's length (bodyHitByWeapon; else bodyHitS)
                if (On42 && w != null && body42ByWeapon.TryGetValue(w, out double bs) && bs > bodyPending42) bodyPending42 = bs;
                Request(ImpactTier.BodyHit);
            }
            if (e.killed) Request(ImpactTier.Kill);
        }

        /// <summary>The web's `kill`: a kill of yours this step (the round's last kill reads it).</summary>
        public void OnKill(KillEvent e)
        {
            if (e.owner != "player") return;
            stepKill = true;
            stepKills++;
            Request(ImpactTier.Kill);
        }

        /// <summary>The `parry` (HitEvent; perfect: PERFECT PARRY, design.parry): its tier, and the perfect one's slow-mo tail.</summary>
        public void OnParry(HitEvent e)
        {
            if (e.owner != "player") return;
            Request(e.perfect ? ImpactTier.PerfectParry : ImpactTier.Parry);
            if (e.perfect) RequestSlowMo(SlowMoCause.PerfectParry);
        }

        /// <summary>`roundClear`: with a kill of yours in the same step, that kill was the round's last.</summary>
        public void OnRoundClear()
        {
            if (stepKill) RequestSlowMo(SlowMoCause.LastKill);
        }

        /// <summary>The sim's own `hitstop` (SurvivalSim.Hitstop: its cause): kill and momentum map to their tiers; a parry's is
        /// covered by the parry tiers; any other cause keeps the sim's length (EndStep's simHitstop).</summary>
        public void OnSimHitstop(string cause)
        {
            if (cause == "kill") Request(ImpactTier.Kill);
            else if (cause == "momentum") Request(ImpactTier.Momentum);
            else if (cause == N43Events.Fusion) fusion43Asked = true; // NIGHT43 W1 fusion43: its own tier (EndStep)
            else if (cause != "parry") simUnknown = true;
        }
    }

    /// <summary>
    /// NIGHT42 F1 impact42: `gauntletHit42`, one per gauntlet hit of the player's (not a wave's share), emitted by the sim
    /// (Game/SurvivalSim.N42.Impact.cs) only with impact42 on; a reused payload (read it in the handler, never keep it). Its class: Small (any
    /// body), Big (design.n41.heavy41.kinds: the TANK, the WARDEN, the CHARGER), Boss (every boss body, THE ICEBREAKER's included:
    /// Loadout.IsBoss34). The impact layer freezes on it (ImpactRules.OnGauntletHit42); the views draw the shockwave ring and the camera kick
    /// (Game/KillFeel42.cs); GG's sounds may hang on the name (no audio here).
    /// </summary>
    public sealed class GauntletHit42Event
    {
        public const string Name = "gauntletHit42";
        public const int Small = 0, Big = 1, Boss = 2;
        public string owner = "player";
        public int enemy, cls, shot;
        public string kind;
        public bool killed;
        public double damage;
        public Vec3 position; // the struck body's aim point
    }

    /// <summary>
    /// NIGHT42 F1 impact42: `killTier42`, one per kill of the player's (not a wave's share), emitted by the sim only with impact42 on; a reused
    /// payload. The tier, for U1's style and F1's kill view: Plain 1 (a gun's body kill), Heavy 2 (a headshot, the rail, a rocket's direct hit,
    /// a heavy shotgun shot), Melee 3 (the gauntlet or a punch), Big 4 (a boss body, a big kind with the gauntlet, or the
    /// design.impact.slowMo.multiKill.count-th kill of one shot).
    /// </summary>
    public sealed class KillTier42Event
    {
        public const string Name = "killTier42";
        public const int Plain = 1, Heavy = 2, Melee = 3, Big = 4;
        public string owner = "player";
        public int tier, enemy, shot;
        public string kind, weapon;
        public Vec3 position;
    }
}
