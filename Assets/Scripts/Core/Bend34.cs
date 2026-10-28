// NIGHT34 builder B (B1, design.n34.bend34): THE GRAPPLE BEND, the pure rule (no engine, no sim types: Game/SurvivalSim.Bend34.cs feeds it).
// GG (2026-10-11): the grapple triggers "a dramatic slow-motion effect" so players "switch weapons in real time ... as the world bends around you".
// - Two tiers. Every latch (a node, a wall, the whip on an enemy) gives a SHORT bend (shortS, free: the grapple's charges are its limit). A latch with
//   the BEND METER full gives the FULL bend instead (holdS, +killExtendS per kill inside it, capped at maxS) and spends the meter.
// - The meter (0 .. 1) starts full at every run start; only kills refill it (refillPerKill, x rankDoubleMult at style rank rankDoubleLetter or higher),
//   never time. A kill inside a FULL bend extends it instead of refilling (its reward is the time).
// - A latch inside a bend: a SHORT bend with the meter full turns into a FULL one; a SHORT bend otherwise runs at least shortS more; a FULL bend
//   is unchanged. A latch during the ease out starts a new bend from the share it has reached.
// - Clocks: every time above is REAL seconds, converted once per run into whole steps of the mode's clock (CLAUDE.md rule 4): global (the whole
//   sim slowed by the loop's share) StepsFor(s x scale) sim steps; split (the player steps every step, the world 1 in N) StepsFor(s) player steps.
//   The ramps (inS, outS) run while the share moves, so in global mode they use the ramp's mean share (1 + scale) / 2.
// - The phases: In (the share eases 1 -> scale, smoothstep), Hold (scale), Out (scale -> 1, smoothstep). "Inside a bend" (the instant swap, the
//   split's held world steps, kills counted) is In + Hold; Out is the snap back. Depth01 = (1 - share) / (1 - scale) in both modes.
// - Hitstop sits above the bend (the loop takes the smaller share; 0 stays 0) and is no business of this class.
// The web has no slow motion of its own (its killSlowMo is a view effect), so every number here is Unity-side (design.n34.bend34, each with a _why).
namespace Skygrind.Core
{
    /// <summary>design.n34.bend34's numbers.</summary>
    public sealed class Bend34Rules
    {
        public double scale, inS, outS, shortS, holdS, killExtendS, maxS, refillPerKill, rankDoubleMult;

        /// <summary>The style rank index (config style.ranks) from which a kill refills x rankDoubleMult (-1: never).</summary>
        public int doubleRank = -1;

        public static Bend34Rules From(GameConfig cfg)
        {
            const string p = "design.n34.bend34.";
            var r = new Bend34Rules
            {
                scale = cfg.Num(p + "scale"),
                inS = cfg.Num(p + "inS"),
                outS = cfg.Num(p + "outS"),
                shortS = cfg.Num(p + "shortS"),
                holdS = cfg.Num(p + "holdS"),
                killExtendS = cfg.Num(p + "killExtendS"),
                maxS = cfg.Num(p + "maxS"),
                refillPerKill = cfg.Num(p + "refillPerKill"),
                rankDoubleMult = cfg.Num(p + "rankDoubleMult"),
            };
            string letter = cfg.Str(p + "rankDoubleLetter");
            var ranks = cfg.Arr("style.ranks");
            for (int i = 0; i < ranks.Count; i++)
                if ((string)PortData.Obj(ranks[i])["letter"] == letter) r.doubleRank = i;
            return r;
        }
    }

    public enum Bend34Tier { None = 0, Short = 1, Full = 2 }

    public enum Bend34Phase { None = 0, In = 1, Hold = 2, Out = 3 }

    /// <summary>What one step of the bend did (the sim turns these into events).</summary>
    public struct Bend34Step
    {
        public bool started, upgraded;
        public int killsExtended;
    }

    /// <summary>The bend's state and its whole-step clock (one per run).</summary>
    public sealed class Bend34
    {
        public Bend34Rules rules;
        public bool split;

        /// <summary>The run's whole-step lengths in the mode's clock.</summary>
        public int inSteps, outSteps, shortSteps, holdSteps, extendSteps, maxSteps;

        /// <summary>Split: the world steps once every N steps while bending (N = round(1 / scale), at least 1).</summary>
        public int worldEvery = 1;

        public Bend34Tier tier;
        public Bend34Phase phase;

        /// <summary>The meter, 0 .. 1 (starts full).</summary>
        public double meter = 1;

        /// <summary>Clock steps since this bend began (0 on the latch's step); `length` = its In + Hold length; `outT` = steps into the Out.</summary>
        public int t, length, outT;

        /// <summary>Kills inside this bend (FULL: each extended it).</summary>
        public int kills;

        /// <summary>The share the current In started from (1, or the Out's share at a latch during the Out).</summary>
        public double fromShare = 1;

        /// <summary>The world's eased share now (1 outside a bend); the global path's loop share.</summary>
        public double share = 1;

        /// <summary>Run counters (the harness).</summary>
        public int shortBends, fullBends, upgrades, latches, killExtends, refills;

        /// <summary>Is the player inside a bend (In or Hold): the instant swap, the split's held world, kills counted?</summary>
        public bool Inside => phase == Bend34Phase.In || phase == Bend34Phase.Hold;

        /// <summary>The depth 0 .. 1 (the overlay's input).</summary>
        public double Depth01 => rules == null || rules.scale >= 1 ? 0 : JsMath.Max(0, JsMath.Min(1, (1 - share) / (1 - rules.scale)));

        /// <summary>A new run: the meter full, no bend, the lengths in the mode's whole steps.</summary>
        public void Reset(Bend34Rules r, bool splitMode, double dt)
        {
            rules = r;
            split = splitMode;
            double k = split ? 1 : r.scale, kr = split ? 1 : (1 + r.scale) / 2;
            inSteps = MovementMath.StepsFor(r.inS * kr, dt);
            outSteps = MovementMath.StepsFor(r.outS * kr, dt);
            shortSteps = MovementMath.StepsFor(r.shortS * k, dt);
            holdSteps = MovementMath.StepsFor(r.holdS * k, dt);
            extendSteps = MovementMath.StepsFor(r.killExtendS * k, dt);
            maxSteps = MovementMath.StepsFor(r.maxS * k, dt);
            worldEvery = r.scale > 0 ? (int)JsMath.Max(1, JsMath.Round(1 / r.scale)) : 1;
            tier = Bend34Tier.None;
            phase = Bend34Phase.None;
            meter = 1;
            t = length = outT = kills = 0;
            fromShare = share = 1;
            shortBends = fullBends = upgrades = latches = killExtends = refills = 0;
        }

        /// <summary>Is the meter full (the next latch pays for a FULL bend)?</summary>
        public bool MeterFull => meter >= 1 - 1e-9;

        /// <summary>The top of a step (before anything moves): advance the clock of a bend in progress. True when the Hold ended on this step.</summary>
        public bool BeginStep()
        {
            if (phase == Bend34Phase.None) return false;
            if (phase == Bend34Phase.Out)
            {
                outT++;
                if (outT >= outSteps)
                {
                    phase = Bend34Phase.None;
                    tier = Bend34Tier.None;
                    outT = 0;
                }
                return false;
            }
            t++;
            if (phase == Bend34Phase.In && t >= inSteps) phase = Bend34Phase.Hold;
            if (t >= length)
            {
                phase = Bend34Phase.Out;
                outT = 0;
                return true;
            }
            return false;
        }

        /// <summary>Split: is the world held on this step (inside a bend, and not the 1-in-N world step)? Call after BeginStep.</summary>
        public bool HoldWorld => split && Inside && worldEvery > 1 && t % worldEvery != 0;

        /// <summary>Split: steps since the world last stepped inside this bend (0 .. N-1; 0 outside a bend). The render's world alpha is
        /// (WorldPhase + frameAlpha) / WorldN between the last two world steps.</summary>
        public int WorldPhase => split && Inside && worldEvery > 1 ? t % worldEvery : 0;

        /// <summary>Split: N while the world is stepping 1 in N, else 1.</summary>
        public int WorldN => split && Inside ? worldEvery : 1;

        /// <summary>A latch on this step (r: what it did).</summary>
        public void Latch(ref Bend34Step r)
        {
            latches++;
            if (Inside)
            {
                if (tier == Bend34Tier.Full) return;
                if (MeterFull)
                {
                    tier = Bend34Tier.Full;
                    meter = 0;
                    length = (int)JsMath.Min(maxSteps, t + holdSteps);
                    kills = 0;
                    fullBends++;
                    upgrades++;
                    r.upgraded = true;
                    return;
                }
                if (length < t + shortSteps) length = t + shortSteps;
                return;
            }
            fromShare = share; // 1, or the Out's share
            t = 0;
            outT = 0;
            kills = 0;
            phase = inSteps > 0 ? Bend34Phase.In : Bend34Phase.Hold;
            if (MeterFull)
            {
                tier = Bend34Tier.Full;
                meter = 0;
                length = (int)JsMath.Min(maxSteps, holdSteps);
                fullBends++;
            }
            else
            {
                tier = Bend34Tier.Short;
                length = shortSteps;
                shortBends++;
            }
            r.started = true;
        }

        /// <summary>A player kill on this step at style rank `rank`.</summary>
        public void Kill(int rank, ref Bend34Step r)
        {
            if (Inside) kills++;
            if (Inside && tier == Bend34Tier.Full)
            {
                int was = length;
                length = (int)JsMath.Min(maxSteps, length + extendSteps);
                if (length > was)
                {
                    killExtends++;
                    r.killsExtended++;
                }
                return;
            }
            double add = rules.refillPerKill * (rules.doubleRank >= 0 && rank >= rules.doubleRank ? rules.rankDoubleMult : 1);
            if (add <= 0 || MeterFull) return;
            meter = JsMath.Min(1, meter + add);
            refills++;
        }

        /// <summary>The end of a step: the eased share for this step's phase.</summary>
        public void UpdateShare()
        {
            double s = rules == null ? 1 : rules.scale;
            switch (phase)
            {
                case Bend34Phase.In:
                    share = fromShare + (s - fromShare) * Smooth(inSteps > 0 ? (double)t / inSteps : 1);
                    break;
                case Bend34Phase.Hold:
                    share = s;
                    break;
                case Bend34Phase.Out:
                    share = s + (1 - s) * Smooth(outSteps > 0 ? (double)outT / outSteps : 1);
                    break;
                default:
                    share = 1;
                    break;
            }
        }

        static double Smooth(double k)
        {
            k = JsMath.Max(0, JsMath.Min(1, k));
            return k * k * (3 - 2 * k);
        }
    }
}
