// NIGHT47 (the lead's helper, DECIDE47 D-5; NIGHT47.md section 4 "The human-model pilot", section 3 "the human band"): `n47human`, the one
// boss pilot every boss builder measures with tonight. Hooked without a Program.cs line: a module initializer runs the command and exits when
// argv[1] is "n47human" (Harness.N46.Flow.cs's head); it applies --off47 / --off46 itself (NightRows.ForceOffFrom: it runs before Program.Main).
//   n47human --boss <wick|brawl|wyrm|loom|crown|all|a,b> [--seeds 40] [--first 1] [--rows r1,r2] [--off47] [--cap 300] [--raw] [--no-melee]
//            [--no-trigger] [--per-seed] [--level id]
//     Each boss's EXISTING fight setup and EXISTING bot pilot, with the config's rows as they ship (no block overrides: the ON game; --rows forces
//     rows on through sim.rowsOverride = Defaults().Set(r, true)), wrapped in a HUMAN MODEL applied to the pilot's input every step:
//       wick   Wick37Pilot (skilled) in THE WICK's stack at (2600, 0, 0) on the survival map (n43wick band's setup; mortal-counted: a 'death' is
//              100 hp lost, healed, never played out, as n37 / n43wick band). The live game's WICK is n46-foundry's boss room: not this file.
//       brawl  Brawl40Pilot (Mixed) in THE ICEBREAKER's arena at (3000, 0, 0) on the survival map (n42brawl band's setup; a death retries).
//       wyrm   WyrmPilot31 in k5 of --level (default n46-kiln, today's level 3; n46wyrm fight's setup: DOORFREE, k5's spawn, a death retries).
//       loom   Loom37Pilot (stay) on n40-span from the "start" ledge (n41loom sweep's setup; a death respawns at the last ledge, the leg restarts).
//       crown  Crown37Pilot (skilled) on n40-spire from (-10, 0.02, -267) (n40crown defects' setup; a death retries).
//     The human model (H47Human; its own Mulberry32 per run, seeded from the seed: never the sim's stream, so the sim stays deterministic per seed):
//       aim    a shot is fated hit or miss before it fires: hit with p, steered so the MEASURED hit share (hitscan shotResult with an enemy hit /
//              'fire' events) sits at 45 % hip fire and 65 % zoomed (loadout.adsActive at the shot). A 'miss' shot turns the look off to a side
//              and high by missDeg (starts 20 deg, x1.5 each time a miss-fated shot hits anyway, max 90: big close bosses) on the steps the fire
//              button is down; the movement is re-projected so the feet go where the pilot meant. Punch steps are never perturbed. Projectile
//              guns count as shots, not as hits. The game's aim assist stays on (it is the game).
//       trigger the human fires consistently (GG: "even while firing consistently"): when the pilot has left the trigger alone for 30 steps
//              (no fire, no alt, no swap), is not punching, holds a gun (not the gauntlet) and the boss's aim point is within 20 deg of the look,
//              a click every 15 steps (8 Hz). --no-trigger: off.
//       react  a tell (the boss's brain entering Tell: Boss37State.Tell for WICK / LOOM / CROWN, Brawl40State.Tell, the worm's charge start) makes
//              the movement (move, jump, dash, slide, grapple, reel, sprint) AND the punch (a parry is a reaction) run 250 ms late for 1.0 s: the
//              pre-tell frames first, then the pilot's own frames shifted 30 steps; the presses skipped at the resync are merged into the resync
//              frame. One tell in five is a further 200 ms late (54 steps). No pilot has a separable dodge: the whole movement input is delayed in
//              the reaction window. The climb is a planned sequence, not a reaction: a tell opens the window only with the feet on the ground and
//              no rope, and a wall ride or a rope ends a window at once (the first cut delayed the WICK's climb and the pilot never reached the
//              top: 0 / 3 killed in 300 s). Look and fire are never delayed (the human keeps shooting while he reacts).
//     --raw: the bare bot (no human model), for the before / after line (it reproduces the boss harness's own numbers: wyrm seed 1 75.3 s =
//     n46wyrm fight). --no-melee: no punch / parry / gauntlet (GG's "firing consistently" play, the bound for a player who has not found the
//     melee). --per-seed: a line per seed. --level: the wyrm's level file (default n46-kiln).
//   Reported per boss: kill time median / p90 / min / max over every seed (a fight alive at the cap counts as the cap, and is counted), damage
//   taken, deaths, windows (N43BossState.open rising edges) and window hits (hitscan hits + punches while open), the boss's damage share dealt
//   inside windows, verbs per minute (mover events, 'fire', 'punch', 'parry', 'bend34Start'), and the measured hit share hip / zoomed.
//   The band (NIGHT47.md section 3): median 90-150 s, p90 <= 180 s, min >= 60 s over 40 seeds.
//
// THE HOOK (boss builders: add your boss's policy in YOUR harness file, never edit this one): declare in `public static partial class Harness`
//     static void H47Policy<Anything>(H47Ctx ctx, ref InputFrame move, ref CombatInput combat) { if (ctx.boss != "wick") return; ... }
//   Every static method of Harness whose name starts with "H47Policy" and has that signature is found by reflection at the run's start (no
//   initializer order to rely on) and called each step AFTER the pilot and BEFORE the human model (your policy shapes what the player means to do;
//   the human limits then apply). ctx: boss id, sim, step (from the fight's begin), rng (the human's own stream: never the sim's), and
//   `tell` (set it true on the step one of YOUR attacks starts its tell: the reaction delay then applies to it too; a new attack with its own
//   tell state is the case), `reacting` (read: the movement is in its late window), `missFated` (read: the next shot is fated to miss).
//   ctx.state is a per-run Dictionary<string, object> for the policy's own memory.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void H47HumanInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47human") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = H47Main(args);
        }
        catch (Exception e) { Console.WriteLine("n47human: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    /// <summary>The context a boss builder's H47Policy* method gets each step.</summary>
    public sealed class H47Ctx
    {
        public string boss;
        public SurvivalSim sim;
        public int step;
        public Mulberry32 rng;
        public bool tell;
        public bool reacting, missFated;
        public readonly Dictionary<string, object> state = new Dictionary<string, object>();
    }

    public delegate void H47PolicyFn(H47Ctx ctx, ref InputFrame move, ref CombatInput combat);

    static List<H47PolicyFn> H47Policies;

    static List<H47PolicyFn> H47FindPolicies()
    {
        var list = new List<H47PolicyFn>();
        foreach (MethodInfo m in typeof(Harness).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            if (!m.Name.StartsWith("H47Policy", StringComparison.Ordinal)) continue;
            ParameterInfo[] ps = m.GetParameters();
            if (ps.Length != 3 || ps[0].ParameterType != typeof(H47Ctx) || ps[1].ParameterType != typeof(InputFrame).MakeByRefType() || ps[2].ParameterType != typeof(CombatInput).MakeByRefType()) continue;
            list.Add((H47PolicyFn)Delegate.CreateDelegate(typeof(H47PolicyFn), m));
        }
        return list;
    }

    static string H47Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static string[] H47Rows = new string[0];
    static string H47Level = "n46-kiln";
    static bool H47Raw, H47PerSeed, H47NoTrigger, H47NoMelee;

    static void H47ApplyRows(SurvivalSim sim)
    {
        if (W48HumanLips(sim, ref H47Rows)) { } // NIGHT48 W (Harness.N48.Wick.cs): --rows climbTop48 forces the WICK stack's lips (a level row: no sim bit) and is taken out of the list
        if (H47Rows.Length == 0) return;
        NightRowState st = sim.rowsRegistry.Defaults();
        foreach (string r in H47Rows) st = st.Set(r, true);
        sim.rowsOverride = st;
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------
    // The human model
    // ------------------------------------------------------------------------------------------------------------------------------------------

    public const double H47HipShare = 0.45, H47ZoomShare = 0.65; // NIGHT47.md section 4: the hit share caps
    const int H47ReactSteps = 30, H47LateSteps = 54, H47WindowSteps = 120; // 250 ms; 250 + 200 ms; the late window 1.0 s (120 Hz)
    const int H47LateEvery = 5; // one tell in five is late

    sealed class H47Human
    {
        public bool enabled = true;
        public Mulberry32 rng;
        // aim
        public int hipShots, hipHits, zoomShots, zoomHits, projShots;
        public int fate = -1; // -1 undecided, 0 miss, 1 hit (the next shot)
        public double missDeg = 20, missYaw, missPitch;
        public bool trigger = true; // the human fires consistently (GG: "even while firing consistently"): a click every 15 steps (8 Hz) when the pilot neither fires nor punches
        public int clicks, lastPilotTrigger = -1000;
        public int missHitAnyway;
        public bool offThisStep, adsThisStep;
        // react
        public int tells, lateTells, skippedTells, aborted;
        public int delay, windowEnd = -1; // the current delay (steps) and the step the late window ends
        public readonly InputFrame[] ring = new InputFrame[256];
        public readonly CombatInput[] cring = new CombatInput[256];
        public int count;
        public InputFrame lastOut;
        public bool wasTelling;
        public int reactSteps;

        public bool Reacting(int step) => windowEnd >= 0 && step < windowEnd;

        static double Wrap(double a) { while (a > Math.PI) a -= 2 * Math.PI; while (a < -Math.PI) a += 2 * Math.PI; return a; }

        /// <summary>Re-project a frame's move (meant under its own yaw) onto another yaw: the same world wish.</summary>
        static void Reproject(ref InputFrame f, double fromYaw, double toYaw)
        {
            if (Math.Abs(Wrap(toYaw - fromYaw)) < 1e-12) return;
            double s = JsMath.Sin(fromYaw), c = JsMath.Cos(fromYaw);
            double wx = -s * f.moveZ + c * f.moveX, wz = -c * f.moveZ - s * f.moveX;
            double s2 = JsMath.Sin(toYaw), c2 = JsMath.Cos(toYaw);
            f.moveZ = -s2 * wx - c2 * wz;
            f.moveX = c2 * wx - s2 * wz;
        }

        static Button Held(Button b) => new Button { held = b.held };
        static Button Or(Button a, Button b) => new Button { held = a.held || b.held, pressed = a.pressed || b.pressed };

        public void Apply(SurvivalSim sim, int step, bool tellingNow, bool onTarget, ref InputFrame move, ref CombatInput combat)
        {
            InputFrame pilot = move;
            ring[step & 255] = pilot;
            cring[step & 255] = combat;
            count = step + 1;
            bool rising = tellingNow && !wasTelling;
            if (!enabled) { if (rising) tells++; wasTelling = tellingNow; lastOut = move; return; }
            // the climb is a planned sequence, not a reaction: a tell opens the late window only on the ground (feet down, no rope), and a
            // window ends at once (the resync below) when the player is on a wall ride or a rope (H47.md: the WICK's climb broke at 250 ms)
            bool traversing = sim.mover.state == MoveState.WallRide || sim.mover.rope.phase != RopePhase.None;
            if (Reacting(step) && traversing) { windowEnd = step; aborted++; }
            // react: a tell's rising edge opens a late window
            if (rising)
            {
                tells++;
                bool late = rng.Next() < 1.0 / H47LateEvery;
                if (!sim.mover.grounded || traversing) { skippedTells++; late = false; }
                else if (!Reacting(step))
                {
                    delay = late ? H47LateSteps : H47ReactSteps;
                    windowEnd = step + delay + H47WindowSteps;
                }
                else windowEnd = Math.Max(windowEnd, step + delay + H47WindowSteps);
                if (late) lateTells++;
            }
            wasTelling = tellingNow;
            InputFrame outF = pilot;
            if (Reacting(step))
            {
                reactSteps++;
                int src = step - delay;
                InputFrame old = src >= 0 && step - src < 256 ? ring[src & 255] : pilot;
                outF = old;
                Reproject(ref outF, old.yaw, pilot.yaw);
                outF.yaw = pilot.yaw; outF.pitch = pilot.pitch; // the look is never delayed
                combat.punch = src >= 0 && step - src < 256 ? cring[src & 255].punch : combat.punch; // a parry / punch is a reaction too: late with the feet
            }
            else if (windowEnd >= 0 && step == windowEnd)
            {
                // the resync: the pilot's presses the late window never played (its last `delay` frames) merge into this frame
                for (int k = step - delay; k < step; k++)
                {
                    if (k < 0) continue;
                    InputFrame d = ring[k & 255];
                    outF.jump = Or(outF.jump, d.jump.pressed ? new Button { pressed = true, held = outF.jump.held || d.jump.pressed } : default);
                    outF.dash = Or(outF.dash, d.dash.pressed ? new Button { pressed = true, held = true } : default);
                    outF.slide = Or(outF.slide, d.slide.pressed ? new Button { pressed = true, held = outF.slide.held || true } : default);
                    outF.grapple = Or(outF.grapple, d.grapple.pressed ? new Button { pressed = true, held = true } : default);
                    if (cring[k & 255].punch.pressed) combat.punch = new Button { pressed = true, held = true };
                }
                windowEnd = -1;
            }
            // the trigger habit: the pilot idle on the trigger and not punching -> a click (8 Hz)
            bool punching = combat.punch.held || combat.punch.pressed;
            if (combat.fire.held || combat.fire.pressed || combat.altFire.held || combat.weapon >= 0) lastPilotTrigger = step;
            if (trigger && onTarget && step - lastPilotTrigger > 30 && !punching && sim.loadout.Weapon != "gauntlet" && step % 15 == 0)
            {
                combat.fire = new Button { pressed = true, held = true };
                clicks++;
            }
            // aim: the next shot's fate
            bool fireDown = combat.fire.held || combat.fire.pressed;
            adsThisStep = sim.loadout.adsActive || combat.altFire.held && sim.loadout.Weapon == "revolver";
            if (fate < 0 && fireDown)
            {
                double target = adsThisStep ? H47ZoomShare : H47HipShare;
                int sh = adsThisStep ? zoomShots : hipShots, hi = adsThisStep ? zoomHits : hipHits;
                double share = (hi + target * 4) / (sh + 4.0);
                double p = Math.Max(0.02, Math.Min(1, target + 1.5 * (target - share)));
                fate = rng.Next() < p ? 1 : 0;
                if (fate == 0)
                {
                    double mag = missDeg * (0.8 + 0.4 * rng.Next()) * Math.PI / 180;
                    missYaw = (rng.Next() < 0.5 ? -1 : 1) * 0.7 * mag; // a miss: off to a side and high (over the shoulder of a close, wide boss)
                    missPitch = (0.5 + 0.5 * rng.Next()) * 0.7 * mag;
                }
            }
            offThisStep = fate == 0 && fireDown && !punching;
            if (offThisStep)
            {
                double y0 = outF.yaw;
                outF.yaw = y0 + missYaw;
                outF.pitch = Math.Max(-1.45, Math.Min(1.45, outF.pitch + missPitch));
                Reproject(ref outF, y0, outF.yaw);
            }
            move = outF;
            lastOut = outF;
        }

        /// <summary>A 'fire' event this step (the shot the fate was drawn for).</summary>
        public void OnFire(bool ads, bool hitscanHit, bool hitscan)
        {
            if (!hitscan) projShots++;
            if (ads) { zoomShots++; if (hitscanHit) zoomHits++; }
            else { hipShots++; if (hitscanHit) hipHits++; }
            if (enabled && fate == 0 && hitscanHit) { missHitAnyway++; missDeg = Math.Min(90, missDeg * 1.5); }
            fate = -1;
        }
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------
    // One fight, any boss
    // ------------------------------------------------------------------------------------------------------------------------------------------

    sealed class H47Fight
    {
        public string boss;
        public SurvivalSim sim;
        public int reveal; // steps from the begin before the clock starts
        public Func<int, (InputFrame, CombatInput)> pilot;
        public Func<bool> alive, telling;
        public Func<double> frac; // the boss's health share
        public Func<Vec3> at; // the boss's aim point (the trigger habit fires only with it inside 20 deg of the look)
        public Action retry; // a death: the harness's retry (null: mortal-counted, healed in place)
        public bool mortalCounted;
        public string note;
    }

    sealed class H47Run
    {
        public bool begun, killed;
        public double seconds, taken, dealt, dealtOpen, hipShare = double.NaN, zoomShare = double.NaN;
        public int deaths, windows, windowHits, tells, lateTells, skippedTells, aborted, hipShots, hipHits, zoomShots, zoomHits, projShots, missHitAnyway, steps;
        public readonly SortedDictionary<string, int> verbs = new SortedDictionary<string, int>(StringComparer.Ordinal);
        public string note;
    }

    /// <summary>The boss's aim point inside 20 degrees of the pilot's look (the human fires at what he sees, not at walls).</summary>
    static bool H47OnTarget(SurvivalSim sim, InputFrame mv, Func<Vec3> at)
    {
        if (at == null) return true;
        Vec3 d = at() - sim.Eye;
        double len = d.Length;
        if (len < 1e-6) return true;
        double cp = JsMath.Cos(mv.pitch);
        double lx = -JsMath.Sin(mv.yaw) * cp, ly = JsMath.Sin(mv.pitch), lz = -JsMath.Cos(mv.yaw) * cp;
        return (lx * d.x + ly * d.y + lz * d.z) / len >= 0.9396926; // cos 20 deg
    }

    /// <summary>--no-melee: GG's first fights as he describes them ("firing consistently"): no punch, no parry, no gauntlet swing (a swap to
    /// the gauntlet is refused; the gauntlet in hand never fires). The bound for a player who has not found the melee yet.</summary>
    static void H47StripMelee(SurvivalSim sim, ref CombatInput cb)
    {
        cb.punch = default;
        int gi = sim.loadout.owned.IndexOf("gauntlet");
        if (gi >= 0 && cb.weapon == gi) cb.weapon = -1;
        if (sim.loadout.Weapon == "gauntlet")
        {
            cb.fire = default;
            int ri = sim.loadout.owned.IndexOf("revolver");
            if (ri >= 0) cb.weapon = ri;
        }
    }

    static H47Run H47Play(H47Fight f, uint seed, double cap)
    {
        var r = new H47Run { begun = f != null };
        if (f == null) return r;
        r.note = f.note;
        SurvivalSim sim = f.sim;
        var si = (ScriptedInput)sim.input;
        var human = new H47Human { enabled = !H47Raw, trigger = !H47NoTrigger, rng = new Mulberry32(seed * 2654435761u ^ 0x4847u) };
        var ctx = new H47Ctx { boss = f.boss, sim = sim, rng = human.rng };
        void V(string k) { r.verbs.TryGetValue(k, out int n); r.verbs[k] = n + 1; }
        sim.mover.Events += e =>
        {
            switch (e.type)
            {
                case MoverEventType.Jump: V("jump"); break;
                case MoverEventType.DoubleJump: V("jump2"); break;
                case MoverEventType.DashJump: V("dashJump"); break;
                case MoverEventType.BoostJump: V("boost"); break;
                case MoverEventType.WallJump: V("wallJump"); break;
                case MoverEventType.Dash: V("dash"); break;
                case MoverEventType.Slam: V("slam"); break;
                case MoverEventType.SlideStart: V("slide"); break;
                case MoverEventType.WallRideStart: V("wallRide"); break;
            }
        };
        bool shotThisStep = false, shotHit = false, shotHitscan = false, open = false;
        sim.events.On("fire", _ => { V("shot"); shotThisStep = true; });
        sim.events.On<ShotEvent>("shotResult", e => { if (e.owner != "player") return; shotHitscan = true; if (e.surface == "enemy" || e.hitCount > 0) shotHit = true; });
        sim.events.On("punch", _ => { V("punch"); if (open) r.windowHits++; });
        sim.events.On("parry", _ => V("parry"));
        sim.events.On("bend34Start", _ => V("bend"));
        double dt = sim.cfg.Dt;
        int capSteps = f.reveal + (int)(cap / dt), step = 0;
        double prevHp = sim.health.hp, prevFrac = f.frac(), maxHp = sim.health.max;
        bool wasOpen = false;
        while (step < capSteps && f.alive())
        {
            var (mv, cb) = f.pilot(step);
            ctx.step = step; ctx.tell = false; ctx.reacting = human.Reacting(step); ctx.missFated = human.fate == 0;
            foreach (H47PolicyFn p in H47Policies) p(ctx, ref mv, ref cb);
            if (H47NoMelee) H47StripMelee(sim, ref cb);
            bool tellNow = f.telling() || ctx.tell;
            human.Apply(sim, step, tellNow, H47OnTarget(sim, mv, f.at), ref mv, ref cb);
            bool ads = human.adsThisStep;
            si.SetLook(mv.yaw, mv.pitch);
            si.Set(mv, cb);
            shotThisStep = shotHit = shotHitscan = false;
            Boss43State bs = sim.N43BossState;
            open = bs.valid && bs.open;
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (shotThisStep)
            {
                human.OnFire(ads || sim.loadout.adsActive, shotHit, shotHitscan);
                if (shotHit && open) r.windowHits++;
            }
            if (open && !wasOpen) r.windows++;
            wasOpen = open;
            double fr = f.frac();
            if (fr < prevFrac) { r.dealt += prevFrac - fr; if (open) r.dealtOpen += prevFrac - fr; }
            prevFrac = fr;
            if (sim.health.hp < prevHp) r.taken += prevHp - sim.health.hp;
            if (f.mortalCounted)
            {
                if (sim.health.hp < sim.health.max - 100) { r.deaths++; sim.health.hp = sim.health.max; }
            }
            else if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                f.retry?.Invoke();
                prevFrac = f.frac();
            }
            prevHp = sim.health.hp;
        }
        r.killed = !f.alive();
        r.steps = step;
        r.seconds = Math.Max(0, step - f.reveal) * dt;
        r.tells = human.tells; r.lateTells = human.lateTells; r.skippedTells = human.skippedTells; r.aborted = human.aborted;
        r.hipShots = human.hipShots; r.hipHits = human.hipHits; r.zoomShots = human.zoomShots; r.zoomHits = human.zoomHits; r.projShots = human.projShots;
        r.missHitAnyway = human.missHitAnyway;
        r.hipShare = human.hipShots > 0 ? human.hipHits / (double)human.hipShots : double.NaN;
        r.zoomShare = human.zoomShots > 0 ? human.zoomHits / (double)human.zoomShots : double.NaN;
        return r;
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------
    // The five setups (each copied from its boss's harness, H47-prefixed; no older harness file edited)
    // ------------------------------------------------------------------------------------------------------------------------------------------

    static H47Fight H47Wick(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.wick37CentreOverride = Stack37;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        if (sim.wick37Stack == null) return null;
        sim.Respawn(Stack37 + new Vec3(0, 0, sim.wick37Stack.p.radius + 1.5), 0);
        if (!sim.BossRoomBegin("wick", out _, out _)) return null;
        Wick37 w = sim.wick37;
        var pilot = new Wick37Pilot { hand = new Mulberry32(seed * 7919u + 13u) };
        double yaw = 0;
        sim.health.max = sim.health.hp = 1e6;
        return new H47Fight
        {
            boss = "wick", sim = sim, reveal = MovementMath.StepsFor(w.p.revealSeconds, sim.cfg.Dt), mortalCounted = true,
            pilot = i => { pilot.Step(sim, yaw, i); yaw = pilot.Move.yaw; return (pilot.Move, pilot.Combat); },
            alive = () => w.Alive || w.brain.state == Boss37State.Dying,
            telling = () => w.brain.state == Boss37State.Tell, at = () => w.Centre,
            frac = () => w.HpFrac,
            note = "survival map stack (2600, 0, 0), Wick37Pilot skilled, deaths mortal-counted (100 hp)",
        };
    }

    static H47Fight H47Brawl(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalSim.Brawl40Dir = MapGenRepo;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.Brawl40Place(Brawl40At)) return null;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        Vec3 spawn = default;
        double yaw0 = 0;
        if (!sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw0)) return null;
        Brawl40 g = sim.brawl40;
        var pilot = new Brawl40Pilot { hand = new Mulberry32(seed * 7919u + 40u), style = Brawl40Pilot.Style.Mixed };
        double yawL = 0;
        return new H47Fight
        {
            boss = "brawl", sim = sim, reveal = MovementMath.StepsFor(g.p.revealSeconds, sim.cfg.Dt),
            pilot = i => { pilot.Step(sim, yawL, i); yawL = pilot.Move.yaw; return (pilot.Move, pilot.Combat); },
            alive = () => g.Alive,
            telling = () => g.state == Brawl40State.Tell, at = () => g.Centre,
            frac = () => g.HpFrac,
            retry = () => { sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw0); sim.Respawn(spawn, yaw0); },
            note = "survival map arena (3000, 0, 0), Brawl40Pilot Mixed",
        };
    }

    static H47Fight H47Wyrm(uint seed)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", H47Level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = Campaign24.Mode;
        H47ApplyRows(sim);
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        for (int i = 0; i < 10; i++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        RoomDef k5 = sim.campaignLevel.rooms.Find(r => r.id == "k5");
        if (k5 == null) return null;
        foreach (string gun in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        sim.Respawn(k5.spawn, k5.spawnYawDeg);
        int wait = 0;
        while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && wait++ < 600) sim.Step();
        if (sim.wyrm24 == null || !sim.wyrm24.Alive) return null;
        Wyrm w = sim.wyrm24;
        var wp = new WyrmPilot31();
        Vec3 retryAt = k5.spawn;
        return new H47Fight
        {
            boss = "wyrm", sim = sim, reveal = 0,
            pilot = i => { wp.Step(sim, i); return (wp.Move, wp.Combat); },
            alive = () => w.Alive,
            telling = () => w.Charging30, at = () => w.HeadPosition,
            frac = () => w.HpFrac,
            retry = () => { if (sim.BossRoomBegin("wyrm", out Vec3 sp, out _)) retryAt = sp; sim.Respawn(retryAt, 0); },
            note = $"{H47Level} k5 (DOORFREE, k5's spawn), WyrmPilot31; the tell = the worm's charge start (Charging30)",
        };
    }

    static H47Fight H47Loom(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Loom40Level(true))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return null;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!sim.BossRoomBegin("loom37", out _, out _)) return null;
        Loom37 L = sim.loom37;
        if (L == null || !L.Alive) return null;
        var pilot = new Loom37Pilot();
        return new H47Fight
        {
            boss = "loom", sim = sim, reveal = 0,
            pilot = i => { pilot.Step(sim, i); return (pilot.Move, pilot.Combat); },
            alive = () => L.Alive && !L.Falling,
            telling = () => L.brain.state == Boss37State.Tell, at = () => L.centre,
            frac = () => L.HpFrac,
            retry = () =>
            {
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
            },
            note = "n40-span from the start ledge, Loom37Pilot stay",
        };
    }

    static H47Fight H47Crown(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path40C("n40"))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(-10, 0.02, -267), 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        if (!sim.BossRoomBegin("crown37", out Vec3 spawn, out double yaw0)) return null;
        Crown37 c = sim.crown37;
        if (c == null) return null;
        var pilot = new Crown37Pilot { hand = new Mulberry32(seed * 7919u + 37u) };
        double yawL = 0;
        return new H47Fight
        {
            boss = "crown", sim = sim, reveal = MovementMath.StepsFor(c.p.revealSeconds, sim.step),
            pilot = i => { pilot.Step(sim, yawL, i); yawL = pilot.Move.yaw; return (pilot.Move, pilot.Combat); },
            alive = () => c.Alive && !c.Dying,
            telling = () => c.brain.state == Boss37State.Tell, at = () => c.centre,
            frac = () => c.body.maxHp > 0 ? Math.Max(0, c.body.hp) / c.body.maxHp : 0,
            retry = () => { sim.BossRoomBegin("crown37", out spawn, out yaw0); sim.Respawn(spawn, yaw0); },
            note = "n40-spire from (-10, 0.02, -267), Crown37Pilot skilled",
        };
    }

    // ------------------------------------------------------------------------------------------------------------------------------------------
    // The command
    // ------------------------------------------------------------------------------------------------------------------------------------------

    static string H47F(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "n/a" : v.ToString("0.0", CultureInfo.InvariantCulture);
    static string H47Pc(double v) => double.IsNaN(v) ? "n/a" : (v * 100).ToString("0", CultureInfo.InvariantCulture) + " %";

    static int H47Main(string[] args)
    {
        string boss = H47Arg(args, "--boss", "all");
        int seeds = int.Parse(H47Arg(args, "--seeds", "40"), CultureInfo.InvariantCulture);
        int first = int.Parse(H47Arg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(H47Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        H47Rows = H47Arg(args, "--rows", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        H47Level = H47Arg(args, "--level", "n46-kiln");
        H47Raw = Array.IndexOf(args, "--raw") >= 0;
        H47PerSeed = Array.IndexOf(args, "--per-seed") >= 0;
        H47NoTrigger = Array.IndexOf(args, "--no-trigger") >= 0;
        H47NoMelee = Array.IndexOf(args, "--no-melee") >= 0;
        H47Policies = H47FindPolicies();
        string[] bosses = boss == "all" ? new[] { "wick", "brawl", "wyrm", "loom", "crown" } : boss.Split(',', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"n47human: bosses [{string.Join(",", bosses)}], seeds {first}..{first + seeds - 1}, cap {cap:0} s, rows [{string.Join(",", H47Rows)}], off from {NightRows.ForceOffFrom}, model {(H47Raw ? "OFF (--raw: the bare bot)" : "ON")}, policies [{string.Join(",", H47Policies.Select(p => p.Method.Name))}]");
        var lines = new List<string>();
        int fails = 0;
        foreach (string b in bosses)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var runs = new List<H47Run>();
            for (int s = first; s < first + seeds; s++)
            {
                H47Fight f;
                try
                {
                    f = b == "wick" ? H47Wick((uint)s) : b == "brawl" ? H47Brawl((uint)s) : b == "wyrm" ? H47Wyrm((uint)s) : b == "loom" ? H47Loom((uint)s) : b == "crown" ? H47Crown((uint)s) : null;
                }
                finally { }
                H47Run r = H47Play(f, (uint)s, cap);
                SurvivalSim.Brawl40Dir = null; SurvivalSim.Wyrm24Dir = null;
                runs.Add(r);
                if (H47PerSeed || !r.begun)
                    Console.WriteLine($"  {b} seed {s}: {(r.begun ? "" : "NOT BEGUN ")}{(r.killed ? "killed" : "CAP")} {H47F(r.seconds)} s, deaths {r.deaths}, taken {r.taken:0}, windows {r.windows} (hits {r.windowHits}), dealt in windows {H47Pc(r.dealt > 0 ? r.dealtOpen / r.dealt : double.NaN)}, tells {r.tells} (late {r.lateTells}, not on the ground {r.skippedTells}, windows cut by a ride or rope {r.aborted}), hit share hip {H47Pc(r.hipShare)} ({r.hipHits}/{r.hipShots}) zoom {H47Pc(r.zoomShare)} ({r.zoomHits}/{r.zoomShots}), projectile shots {r.projShots}, miss-fated hits {r.missHitAnyway}; verbs {string.Join(" ", r.verbs.Select(kv => kv.Key + " " + kv.Value))}");
            }
            var begun = runs.Where(x => x.begun).ToList();
            if (begun.Count == 0) { Console.WriteLine($"n47human {b}: NO FIGHT began"); fails++; continue; }
            var t = begun.Select(x => x.killed ? x.seconds : cap).OrderBy(x => x).ToList();
            double Q(List<double> l, double q) => l.Count == 0 ? double.NaN : l[Math.Min(l.Count - 1, (int)Math.Floor(q * (l.Count - 1) + 0.5))];
            double Med(IEnumerable<double> e) { var l = e.OrderBy(x => x).ToList(); return Q(l, 0.5); }
            int killed = begun.Count(x => x.killed);
            double minutes = begun.Sum(x => x.steps) / 120.0 / 60.0;
            var verbs = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var x in begun) foreach (var kv in x.verbs) { verbs.TryGetValue(kv.Key, out int n); verbs[kv.Key] = n + kv.Value; }
            int hs = begun.Sum(x => x.hipShots), hh = begun.Sum(x => x.hipHits), zs = begun.Sum(x => x.zoomShots), zh = begun.Sum(x => x.zoomHits);
            double med = Q(t, 0.5), p90 = Q(t, 0.9), mn = t[0], mx = t[^1];
            bool band = med >= 90 && med <= 150 && p90 <= 180 && mn >= 60 && killed == begun.Count;
            string verbLine = string.Join(", ", verbs.Select(kv => $"{kv.Key} {kv.Value / Math.Max(1e-9, minutes):0.0}"));
            Console.WriteLine($"n47human {b} ({begun[0].note}): begun {begun.Count}/{seeds}, killed {killed}/{begun.Count} (caps {begun.Count - killed}); kill time median {H47F(med)} s, p90 {H47F(p90)} s, min {H47F(mn)} s, max {H47F(mx)} s (band 90-150 / p90 <= 180 / min >= 60: {(band ? "IN" : "OUT")}); damage taken median {Med(begun.Select(x => x.taken)):0}; deaths median {Med(begun.Select(x => (double)x.deaths)):0} (total {begun.Sum(x => x.deaths)}); windows median {Med(begun.Select(x => (double)x.windows)):0}, window hits median {Med(begun.Select(x => (double)x.windowHits)):0}, dealt in windows median {H47Pc(Med(begun.Where(x => x.dealt > 0).Select(x => x.dealtOpen / x.dealt)))}; tells {begun.Sum(x => x.tells)} (late {begun.Sum(x => x.lateTells)}, not on the ground {begun.Sum(x => x.skippedTells)}, cut {begun.Sum(x => x.aborted)}); hit share hip {H47Pc(hs > 0 ? hh / (double)hs : double.NaN)} ({hh}/{hs}), zoom {H47Pc(zs > 0 ? zh / (double)zs : double.NaN)} ({zh}/{zs}), projectile shots {begun.Sum(x => x.projShots)}; verbs / min: {verbLine}; {sw.Elapsed.TotalSeconds:0} s wall");
            lines.Add($"| {b} | {killed}/{begun.Count} | {H47F(med)} | {H47F(p90)} | {H47F(mn)} | {H47F(mx)} | {Med(begun.Select(x => x.taken)):0} | {begun.Sum(x => x.deaths)} | {Med(begun.Select(x => (double)x.windows)):0} / {Med(begun.Select(x => (double)x.windowHits)):0} | {H47Pc(hs > 0 ? hh / (double)hs : double.NaN)} / {H47Pc(zs > 0 ? zh / (double)zs : double.NaN)} | {verbLine} | {(band ? "IN" : "OUT")} |");
        }
        Console.WriteLine("| boss | killed | median s | p90 s | min s | max s | taken (median) | deaths (total) | windows / window hits (median) | hit share hip / zoom | verbs / min | band |");
        Console.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (string l in lines) Console.WriteLine(l);
        return fails == 0 ? 0 : 1;
    }
}
