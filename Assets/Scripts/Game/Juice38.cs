// NIGHT38 builder H (design.n38.juice38, a view row: no sim bit; PLAN.md section 4; GG 2026-10-13: "the screen to shake ... when he first enters ...
// the worm boss digging"; "a static TV" flicker when you take damage). One Juice38 per CameraFeel (wired with it: CameraFeel.Wire / Apply). With the
// row on (and not --off38 / -skygrindOff38):
//   - A BOSS ENTRANCE (every boss's `bossRise`, THE KILN WYRM's included; or `boss38Entrance` on the bus for an explicit call): a hit-stop
//     (SurvivalGame.Hitstop, the impact layer's Sim tier: its budget and HIT FREEZE apply), a shake (ULTRAKILL's CameraShake: the bigger replaces,
//     linear decay 3 / s, the view turning min(shake, 1) of Juice38*Deg), a full-screen flicker through Crt29's pass (Juice38Crt: two brightness pulses,
//     deeper scanlines, a tear, a colour split) and the card (the boss's name on hud33's layer: Ui.RenderCard38). Event boss38Entrance (names only).
//   - A BIG BOSS MOVE (design.n38.juice38.moves: THE WICK's dive, THE STAG's crash, THE LOOM's fall, THE CROWN's shatter; THE KILN WYRM's dig and erupt
//     from its bossTell `crack` / `dive`, landing the tell's seconds of world time later): a shake and a one-pulse flicker. Event boss38Move.
//   - DAMAGE (`hurt`, the player): a full-screen TV static / RGB-split burst at the hit's level (0.5 .. 0.8 by the damage: ULTRAKILL's hurt screen,
//     whose alpha IS the i-frame time) falling 1 per second, and a small shake (damage / 20). Event hurt38Static. UiTv35's HUD glitch is unchanged.
//     NIGHT39 V: a damage-over-time tick (Juice39Hurt: a DoT source, a burn's repeat, or inside the last burst's 1 s) gets only a small static
//     (Juice38TickStatic) and no shake; event hurt38Tick.
//   - feel.trauma.boss (the web's number, read by nothing until now) is added to the web trauma shake on feel.trauma.bossEvents.
// Safety: the brightness pulses go through the game's FlashGate (three a second, every flash of the game) and scale with FLASH INTENSITY; HUD FLICKER
// off (look30's ACCESS switch) stills the flicker and the static (the shake stays); REDUCED MOTION stills the shake and the tear (the static stays);
// CAMERA SHAKE % and EFFECTS INTENSITY scale the shake as every other shake. View only: no sim path reads anything here.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>What juice38 did (the reels read it; all 0 with the row off) and GG's sound hook.</summary>
    public static class Juice38Probe
    {
        public static int Entrances, Moves, Hurts, Hitstops, FlickerPulses, FlickersRefused;
        /// <summary>NIGHT43 B1 (wick43): THE WICK's entrances drawn the wick43 way (no split, no hit-stop).</summary>
        public static int Wick43Entrances;
        /// <summary>NIGHT39 V: of the Hurts, the full bursts and the damage ticks (Juice39Hurt).</summary>
        public static int HurtBursts, HurtTicks;
        public static double Shake; // the shake now (ULTRAKILL units)
        public static float Static, Flicker; // the static's level now, the flicker's brightness gain now
        public static string LastCue = "", LastBoss = "";
        /// <summary>The card: its serial (a new card when it changes), the name, the subtitle.</summary>
        public static int CardSerial;
        public static string CardName = "", CardSub = "";

        /// <summary>A cue fired (boss38Entrance / boss38Move / hurt38Static): GG's sound hook (no audio here: CLAUDE.md rule 7).</summary>
        public static event Action<string> Cue;
        internal static void Raise(string name)
        {
            LastCue = name;
            FeelProbe.Event(name);
            Cue?.Invoke(name);
        }

        public static void Clear()
        {
            Entrances = Moves = Hurts = Hitstops = FlickerPulses = FlickersRefused = HurtBursts = HurtTicks = Wick43Entrances = 0;
            Shake = 0;
            Static = Flicker = 0;
            LastCue = LastBoss = "";
        }
    }

    /// <summary>This frame's adds to Crt29's pass (Crt29Pass reads them; Crt29Driver keeps the pass on while they are live).</summary>
    public static class Juice38Crt
    {
        public static float Grain, SplitUv, Tear, Scan, Sweep, SweepHalf;
        public static uint TearStep;
        public static int Frame = -1;

        /// <summary>Set this frame (or the last: the render may run before this frame's camera) with anything to add.</summary>
        public static bool Live => Frame >= Time.frameCount - 1 && (Grain > UnityTuning.Juice38LiveEpsilon || SplitUv > 0 || Tear > UnityTuning.Juice38LiveEpsilon || Scan > UnityTuning.Juice38LiveEpsilon || Sweep > UnityTuning.Juice38LiveEpsilon);
    }

    /// <summary>NIGHT38 juice38's rules (pure; EditMode tests: Juice38Tests).</summary>
    public static class Juice38Math
    {
        /// <summary>ULTRAKILL's CameraShake: a new shake replaces the old one only when bigger (never adds).</summary>
        public static double Replace(double now, double amount) => amount > now ? amount : now;

        /// <summary>A linear fall of `rate` per second (the shake 3 / s, the static 1 / s).</summary>
        public static double Decay(double v, double rate, double dt) => Math.Max(0, v - rate * dt);

        /// <summary>The share of the full turn the view moves at a shake (ULTRAKILL: above 1 the offset no longer grows: a bigger shake only lasts longer).</summary>
        public static double Amplitude(double shake) => shake <= 0 ? 0 : Math.Min(1, shake);

        /// <summary>A hit's shake: damage / per, between min and max (0 for no damage).</summary>
        public static double HurtShake(double damage, double per, double min, double max) => damage <= 0 || per <= 0 ? 0 : Math.Max(min, Math.Min(max, damage / per));

        /// <summary>A hit's static level: min .. max by the damage's share of heavyAt (ULTRAKILL: 0.5, or 0.8 at 50+).</summary>
        public static double HurtLevel(double damage, double min, double max, double heavyAt) =>
            damage <= 0 ? 0 : heavyAt <= 0 ? max : min + (max - min) * Math.Min(1, damage / heavyAt);

        /// <summary>The flicker's brightness (0..1) `age` s into a flicker of `total` s with `pulses` evenly spaced pulses, each falling as (1 - t)^2.</summary>
        public static double Pulse(double age, double total, int pulses)
        {
            if (age < 0 || total <= 0 || age >= total || pulses <= 0) return 0;
            double seg = total / pulses;
            double t = (age - Math.Floor(age / seg) * seg) / seg;
            return (1 - t) * (1 - t);
        }

        /// <summary>The flicker's envelope (1 at its start, 0 at its end, linear).</summary>
        public static double Envelope(double age, double total) => age < 0 || total <= 0 || age >= total ? 0 : 1 - age / total;

        /// <summary>A move's spec "event:stage" (or "event"): does this payload's stage match?</summary>
        public static bool StageMatches(string want, string stage) => string.IsNullOrEmpty(want) || want == stage;
    }

    /// <summary>The row's live state (one per CameraFeel).</summary>
    public sealed class Juice38
    {
        bool on;
        GameConfig cfg;
        double entranceShake, entranceHitstop, entranceFlicker, entranceGap, moveShake, moveFlicker, shakeDecay;
        double hurtPer, hurtMin, hurtMax, staticMin, staticMax, heavyAt, staticDecay;
        readonly HashSet<string> wyrmTells = new HashSet<string>();
        double shake, shakeTime, staticLevel, flickerAge = double.MaxValue, flickerTotal;
        int flickerPulses;
        bool flickerBright;
        bool noSplit43; // NIGHT43 B1 (wick43): the current entrance flicker draws no red / blue split
        double realClock, worldClock, lastEntrance = double.NegativeInfinity;
        readonly List<double> pendingMoves = new List<double>(); // world-clock times of THE KILN WYRM's dig / erupt still to land
        readonly Juice39Hurt hurts = new Juice39Hurt(); // NIGHT39 V: a hit's burst or a damage tick's small cue

        public bool On => on;

        /// <summary>From CameraFeel.Wire: the row's numbers and its listeners (nothing at all with the row off).</summary>
        public void Wire(EventBus events, GameConfig config)
        {
            cfg = config;
            on = Hud38.RowOn(config, "juice38") || N41View.RowOn(config, "juice41On"); // NIGHT41 (the lead's hook): or design.n41.juice41On (the n38 row untouched; off with --off41)
            if (!on) return;
            const string p = "design.n38.juice38.";
            // NIGHT41 J2 (design.n41.juice41, a view row): the iteration after T2's frames: a number present in design.n41.juice41 overrides
            // design.n38.juice38's (never edited in place); with juice41 off (or --off41) every number is NIGHT38's.
            bool iter = N41View.RowOn(config, "juice41");
            double J(string k) => iter && config.Has("design.n41.juice41." + k) ? config.Num("design.n41.juice41." + k) : config.Num(p + k);
            entranceShake = J("entranceShake");
            entranceHitstop = J("entranceHitstop");
            entranceFlicker = J("entranceFlicker");
            entranceGap = J("entranceGap");
            moveShake = J("moveShake");
            moveFlicker = J("moveFlicker");
            shakeDecay = J("shakeDecay");
            hurtPer = J("hurtShakePer");
            hurtMin = J("hurtShakeMin");
            hurtMax = J("hurtShakeMax");
            staticMin = J("hurtStaticMin");
            staticMax = J("hurtStaticMax");
            heavyAt = J("hurtHeavyAt");
            staticDecay = J("staticDecay");
            foreach (object o in config.Arr(p + "wyrmTells")) wyrmTells.Add((string)o);

            events.On<BossEvent>("bossRise", e => Entrance(e != null ? e.name ?? (e.boss ?? "").ToUpperInvariant() : ""));
            events.On(UnityTuning.Juice38EntranceEvent, o => Entrance(o is BossEvent b ? b.name : o as string));
            events.On<BossEvent>("bossIntro", e =>
            {
                // the kit's card follows the rise (NIGHT36 / 37 bosses: walkin36.cardSeconds later): its subtitle joins the card when it is the same boss
                // (THE KILN WYRM's rise says "WYRM"; its card, kit.Start's displayName, names it whole: the card takes the intro's name too)
                if (e == null || realClock - lastEntrance > UnityTuning.Juice38CardHold) return;
                if (!string.IsNullOrEmpty(e.name)) Juice38Probe.CardName = e.name;
                if (!string.IsNullOrEmpty(e.subtitle)) Juice38Probe.CardSub = e.subtitle;
            });
            events.On<BossEvent>("bossTell", e =>
            {
                if (e == null || e.boss != "wyrm" || e.name == null || !wyrmTells.Contains(e.name)) return;
                pendingMoves.Add(worldClock + Math.Max(0, e.seconds)); // the dig / the erupt lands when its tell resolves
            });
            foreach (object o in config.Arr(p + "moves"))
            {
                string spec = (string)o;
                int c = spec.IndexOf(':');
                string name = c < 0 ? spec : spec.Substring(0, c), stage = c < 0 ? null : spec.Substring(c + 1);
                events.On(name, payload =>
                {
                    if (Juice38Math.StageMatches(stage, StageOf(payload))) Move();
                });
            }
            events.On<HurtEvent>("hurt", e =>
            {
                if (e == null || e.owner != "player" || !(e.amount > 0)) return;
                Hurt(e.amount, e.source);
            });
            // feel.trauma.boss on the web's boss events, into the web's trauma shake (SurvivalGame.trauma, capped at feel.trauma.cap)
            double tb = config.Num("feel.trauma.boss"), cap = config.Num("feel.trauma.cap");
            foreach (object o in config.Arr("feel.trauma.bossEvents"))
                events.On((string)o, _ => { SurvivalGame g = SurvivalGame.Instance; if (g != null) g.trauma.Add(tb, cap); });
            Juice38Probe.Clear();
        }

        static string StageOf(object payload)
        {
            switch (payload)
            {
                case Wick37Fx w: return w.stage;
                case Stag37Event s: return s.stage;
                case Loom37Event l: return l.stage;
                case Crown37Event c: return c.stage;
                default: return null;
            }
        }

        void Entrance(string name)
        {
            if (realClock - lastEntrance < entranceGap) return; // the same entrance (a resume start, a rise and an explicit call on one beat)
            lastEntrance = realClock;
            SurvivalGame g = SurvivalGame.Instance;
            // NIGHT43 B1 (wick43, G10: "a strange pink effect", "runs into ... a brick wall" at THE STACK's door): THE WICK's entrance with the sim's
            // wick43 on keeps its flicker's pulses and scanlines but no red / blue split (the magenta fringe) and no hit-stop (the reveal's letterbox
            // and look ease frame it instead: Wick37View.N43.cs), and a smaller shake
            bool wick43 = Wick37View.On43(g) && g.sim.wick37 != null && name == g.sim.wick37.p.name;
            noSplit43 = wick43;
            if (wick43) Juice38Probe.Wick43Entrances++;
            shake = Juice38Math.Replace(shake, wick43 ? entranceShake * UnityTuning.Wick43EntranceShakeMul : entranceShake);
            Flicker(entranceFlicker, UnityTuning.Juice38EntrancePulses);
            if (!wick43 && g != null && entranceHitstop > 0 && g.flow == Flow.Playing) { g.Hitstop(entranceHitstop, UnityTuning.Juice38EntranceEvent); Juice38Probe.Hitstops++; }
            Juice38Probe.Entrances++;
            Juice38Probe.LastBoss = name ?? "";
            Juice38Probe.CardName = name ?? "";
            Juice38Probe.CardSub = "";
            Juice38Probe.CardSerial++;
            Juice38Probe.Raise(UnityTuning.Juice38EntranceEvent);
        }

        void Move()
        {
            shake = Juice38Math.Replace(shake, moveShake);
            if (flickerAge >= flickerTotal) Flicker(moveFlicker, UnityTuning.Juice38MovePulses); // never cut an entrance's flicker short
            Juice38Probe.Moves++;
            Juice38Probe.Raise(UnityTuning.Juice38MoveEvent);
        }

        void Hurt(double amount, string source)
        {
            Juice38Probe.Hurts++;
            if (!hurts.Hit(realClock, source))
            {
                // NIGHT39 V: a damage tick (a burn's cadence, a DoT source, or inside the last burst's cooldown): a small cue, never the full burst
                shake = Juice38Math.Replace(shake, UnityTuning.Juice38TickShake);
                if (FlickerAllowed()) staticLevel = Math.Max(staticLevel, UnityTuning.Juice38TickStatic);
                Juice38Probe.HurtTicks++;
                Juice38Probe.Raise(UnityTuning.Juice38TickEvent);
                return;
            }
            shake = Juice38Math.Replace(shake, Juice38Math.HurtShake(amount, hurtPer, hurtMin, hurtMax));
            if (FlickerAllowed()) staticLevel = Math.Max(staticLevel, Juice38Math.HurtLevel(amount, staticMin, staticMax, heavyAt));
            Juice38Probe.HurtBursts++;
            Juice38Probe.Raise(UnityTuning.Juice38HurtEvent);
        }

        void Flicker(double seconds, int pulses)
        {
            if (!FlickerAllowed() || seconds <= 0) return;
            flickerAge = 0;
            flickerTotal = seconds;
            flickerPulses = Light54.On(cfg) ? System.Math.Min(pulses, (int)Light54.Num(cfg, "juicePulses", 1)) : pulses; // NIGHT54 light54: one clean flash, never a repeated flicker
            // the brightness pulses are flashes: the game's gate (three a second, every flash) and FLASH INTENSITY > 0; refused, the rest still plays
            SurvivalGame g = SurvivalGame.Instance;
            flickerBright = g != null && g.settings != null && g.settings.Num("flashIntensity") > 0 && g.flashGate.TryFlash(SurvivalGame.RealNow);
            if (flickerBright) Juice38Probe.FlickerPulses += pulses;
            else Juice38Probe.FlickersRefused++;
        }

        /// <summary>HUD FLICKER (look30's ACCESS switch, UnityTuning.HudFx30Setting) on: the flicker and the static may play.</summary>
        static bool FlickerAllowed()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.settings == null) return true;
            return g.settings.store.Item(UnityTuning.HudFx30Setting) == null || g.settings.Bool(UnityTuning.HudFx30Setting);
        }

        /// <summary>
        /// Once a frame from CameraFeel.Apply: the clocks, the wyrm's landed moves, the decays, this frame's CRT adds (Juice38Crt) and the shake (deg:
        /// pitch, yaw, roll) x `scale` (EFFECTS INTENSITY x CAMERA SHAKE %, 0 with REDUCED MOTION: CameraFeel's own composition).
        /// </summary>
        public (double pitch, double yaw, double roll) Step(double frameDt, double scale)
        {
            if (!on) return (0, 0, 0);
            realClock += frameDt;
            SurvivalGame g = SurvivalGame.Instance;
            worldClock += g != null ? g.WorldDt : frameDt;
            for (int i = pendingMoves.Count - 1; i >= 0; i--)
            {
                if (worldClock < pendingMoves[i]) continue;
                pendingMoves.RemoveAt(i);
                Move();
            }
            // the shake: sampled, then it falls (ULTRAKILL decays it on unscaled time: frame time here)
            double amp = Juice38Math.Amplitude(shake) * scale;
            (double, double, double) deg = (0, 0, 0);
            if (amp > 0)
            {
                double t = shakeTime * UnityTuning.Juice38NoiseHz;
                deg = (UnityTuning.Juice38PitchDeg * amp * Cosmetic.ValueNoise(t, UnityTuning.Juice38NoiseSalt),
                    UnityTuning.Juice38YawDeg * amp * Cosmetic.ValueNoise(t, UnityTuning.Juice38NoiseSalt + 1),
                    UnityTuning.Juice38RollDeg * amp * Cosmetic.ValueNoise(t, UnityTuning.Juice38NoiseSalt + 2));
            }
            shakeTime = shake > 0 ? shakeTime + frameDt : 0;
            shake = Juice38Math.Decay(shake, shakeDecay, frameDt);
            Juice38Probe.Shake = shake;
            Crt(frameDt, g);
            return deg;
        }

        void Crt(double frameDt, SurvivalGame g)
        {
            bool calm = cfg != null && cfg.movement.settings.reducedMotion;
            double flash = g != null && g.settings != null ? Math.Max(0, Math.Min(1, g.settings.Num("flashIntensity"))) : 1;
            double env = 0, pulse = 0;
            if (flickerAge < flickerTotal)
            {
                env = Juice38Math.Envelope(flickerAge, flickerTotal);
                pulse = flickerBright ? Juice38Math.Pulse(flickerAge, flickerTotal, flickerPulses) * flash : 0;
                flickerAge += frameDt;
            }
            else flickerAge = double.MaxValue;
            double st = staticLevel;
            staticLevel = Juice38Math.Decay(staticLevel, staticDecay, frameDt);
            if (Light54.On(cfg)) env *= Light54.Num(cfg, "juiceEnv", 0); // NIGHT54 light54: the flicker's tear / split / scan / grain off (its one brightness pulse stays; the damage static unchanged)
            Juice38Crt.Grain = (float)(st * UnityTuning.Juice38StaticGrain + env * UnityTuning.Juice38FlickerGrain);
            Juice38Crt.SplitUv = (float)(st * UnityTuning.Juice38StaticSplitUv + (noSplit43 ? 0 : env * UnityTuning.Juice38FlickerSplitUv)); // NIGHT43 B1: THE WICK's wick43 entrance, no colour split
            Juice38Crt.Tear = calm ? 0f : (float)Math.Max(st * UnityTuning.Juice38StaticTear, env * UnityTuning.Juice38FlickerTear);
            Juice38Crt.Scan = (float)(st * UnityTuning.Juice38StaticScan + env * UnityTuning.Juice38FlickerScan);
            Juice38Crt.Sweep = (float)(pulse * UnityTuning.Juice38FlickerGain);
            Juice38Crt.SweepHalf = UnityTuning.Juice38SweepHalf;
            Juice38Crt.TearStep = (uint)Math.Floor(realClock * UnityTuning.Juice38TearHz) + 3801u;
            Juice38Crt.Frame = Time.frameCount;
            Juice38Probe.Static = (float)st;
            Juice38Probe.Flicker = Juice38Crt.Sweep;
        }

        /// <summary>A respawn / a new run: nothing carries over.</summary>
        public void Clear()
        {
            shake = staticLevel = 0;
            flickerAge = double.MaxValue;
            hurts.Clear();
            pendingMoves.Clear();
            Juice38Crt.Grain = Juice38Crt.SplitUv = Juice38Crt.Tear = Juice38Crt.Scan = Juice38Crt.Sweep = 0;
        }
    }
}
