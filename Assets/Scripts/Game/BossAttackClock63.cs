// NIGHT63 builder V (slate B1's first deliverable; Logs/n63/slate.json B1 doneWhen 1; NIGHT63.md section 3 V): BossAttackClock63, a READ-ONLY view
// of a campaign boss's attack state from its sim: which attack, which phase (the wind-up, the strike, the recover, a stagger ...) and the whole sim
// steps to the hit. Nothing here writes to the sim: it reads fields and properties only (no Step, no Force, no setter), so the sim, the hitboxes,
// the damage frames and every fingerprint are untouched. Its users: the boss63 reel (FeelReel.N63V.cs: the [boss63] attack / flinch / stagger
// lines), BossMotion63 (the wind-up pose) and builder G's B2 (each attack's cue lead).
// THE FIVE (their bossRise ids):
//   wick     THE WICK (Wick37: Boss37Brain)        weak point: the flame core (the cage's centre)
//   brawl40  THE ICEBREAKER (Brawl40: its states)  weak point: the torso (the gauntlet's target)
//   wyrm     THE KILN WYRM (Wyrm: its attack log) weak point: the head (the exposed core while it is open)
//   loom37   THE LOOM (Loom37: Boss37Brain)       weak point: a live knee of the leg's pair (the spinneret in the window)
//   crown37  THE CROWN (Crown37: Boss37Brain)     weak point: the ring's top blade (its centre is empty: a shot there flies through)
// THE HIT is the step the attack's strike begins: Boss37Brain and Brawl40 go TELL -> ACTIVE when the tell's steps left run out (stepsToHit = the
// state's steps left: Boss37Brain.Step and Brawl40.Step decrement, then switch); the wyrm has no active state: a tell logged at step s announces
// `seconds` and hits at s + StepsFor(seconds) (its lunge, dive and spit resolve in that step), read as a Strike for UnityTuning.Boss63WyrmStrikeS.
// Phases: Reveal (the walk-in), Idle (rest, walk, between attacks), Windup (a tell), Strike, Recover, Stagger (a window, the Icebreaker stuck or
// dazed, the wyrm staggered: `why` names it), Change (a leg or phase change), Dying; None = no fight.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public enum BossPhase63 : byte { None = 0, Reveal, Idle, Windup, Strike, Recover, Stagger, Change, Dying }

    /// <summary>One read of a campaign boss's attack state (see BossAttackClock63's head).</summary>
    public struct BossAttack63
    {
        /// <summary>The boss's bossRise id (null: no fight), the attack in its wind-up / strike / recover (null: none), the stagger's kind.</summary>
        public string boss, attack, why;
        public BossPhase63 phase;
        /// <summary>Windup: whole sim steps until the strike begins; Strike: 0; else -1.</summary>
        public int stepsToHit;
        /// <summary>The wind-up's full length in steps (0: no wind-up now), the steps spent in the current phase.</summary>
        public int windupSteps, phaseSteps;
        /// <summary>The boss's centre and its weak point (web space; Read fills them, the From* mappings leave them zero).</summary>
        public Vec3 centre, weak;
        /// <summary>The boss's health now and its pool (Read fills them).</summary>
        public double hp, maxHp;

        public bool Live => boss != null && phase != BossPhase63.None;

        /// <summary>0 at the wind-up's first step, 1 at the strike (0 outside a wind-up).</summary>
        public double Windup01 => phase == BossPhase63.Windup && windupSteps > 0 ? Math.Max(0, Math.Min(1, 1 - stepsToHit / (double)windupSteps)) : 0;
    }

    public static class BossAttackClock63
    {
        public const string Wick = "wick", Brawl = "brawl40", Wyrm = "wyrm", Loom = "loom37", Crown = "crown37";

        /// <summary>The five campaign bosses by their bossRise ids (BossRule61's ids).</summary>
        public static readonly string[] Bosses = { Wick, Brawl, Wyrm, Loom, Crown };

        /// <summary>The named boss's state now (null: the first of the five with a live fight). Not live: phase None.</summary>
        public static BossAttack63 Read(SurvivalSim sim, string boss = null)
        {
            if (sim == null) return default;
            if (boss == null)
            {
                foreach (string b in Bosses)
                {
                    BossAttack63 r = Read(sim, b);
                    if (r.Live) return r;
                }
                return default;
            }
            BossAttack63 a = default;
            switch (boss)
            {
                case Wick:
                {
                    Wick37 w = sim.wick37;
                    if (w == null || !w.Alive || w.brain == null) return default;
                    a = FromBrain(Wick, w.brain);
                    a.centre = w.Centre;
                    a.weak = w.Centre;
                    a.hp = w.body.hp;
                    a.maxHp = w.body.maxHp;
                    break;
                }
                case Brawl:
                {
                    Brawl40 g = sim.brawl40;
                    if (g == null || !(g.Alive || g.state == Brawl40State.Dying)) return default;
                    a = FromBrawl(g.state, g.attack, g.stateLeft, g.stateTotal, g.stateSteps);
                    a.centre = g.Centre;
                    a.weak = g.Centre;
                    a.hp = g.body.hp;
                    a.maxHp = g.body.maxHp;
                    break;
                }
                case Wyrm:
                {
                    Wyrm w = sim.wyrm24;
                    if (w == null || !w.Alive) return default;
                    a = FromWyrm(w.log, w.stepNo, w.phase, sim.wyrm31 != null && sim.wyrm31.Staggered, sim.step);
                    a.centre = w.HeadPosition;
                    a.weak = w.PieceCount > 0 ? w.boxes[0].c : w.HeadPosition;
                    if (w.CoreOpen && w.PieceCount > 0) a.weak = w.boxes[0].c + w.boxes[0].u * w.p.coreUp - w.boxes[0].f * w.p.coreBack;
                    a.hp = w.body.hp;
                    a.maxHp = w.body.maxHp;
                    break;
                }
                case Loom:
                {
                    Loom37 L = sim.loom37;
                    if (L == null || !L.Alive || !sim.loom37Owns) return default;
                    a = FromBrain(Loom, L.brain);
                    a.centre = L.centre;
                    a.weak = L.Open ? L.Spinneret : L.centre;
                    if (!L.Open)
                        for (int k = 0; k < L.knees.Length; k++)
                            if (L.knees[k] != null && L.ActiveKnee(k)) { a.weak = L.knees[k].at; break; }
                    a.hp = L.body.hp;
                    a.maxHp = L.body.maxHp;
                    break;
                }
                case Crown:
                {
                    Crown37 c = sim.crown37;
                    if (c == null || !c.Alive) return default;
                    a = FromBrain(Crown, c.brain);
                    a.centre = c.centre;
                    a.weak = c.centre + InPlaneUp(c.normal) * c.p.ringRadius;
                    a.hp = c.body.hp;
                    a.maxHp = c.body.maxHp;
                    break;
                }
                default:
                    return default;
            }
            return a;
        }

        /// <summary>World up projected into the plane whose normal is n (unit), else any unit vector in it: the ring's top.</summary>
        static Vec3 InPlaneUp(Vec3 n)
        {
            double len = n.Length;
            if (len < 1e-9) return new Vec3(0, 1, 0);
            Vec3 u = n * (1 / len);
            Vec3 up = new Vec3(0, 1, 0) - u * u.y;
            double l = up.Length;
            if (l < 1e-6) { up = new Vec3(1, 0, 0) - u * u.x; l = up.Length; }
            return l < 1e-9 ? new Vec3(0, 1, 0) : up * (1 / l);
        }

        /// <summary>A Boss37Brain boss (THE WICK, THE LOOM, THE CROWN): its state mapped (see the file's head). Reads only.</summary>
        public static BossAttack63 FromBrain(string boss, Boss37Brain b)
        {
            var a = new BossAttack63 { boss = boss, stepsToHit = -1 };
            if (b == null) { a.phase = BossPhase63.None; return a; }
            a.phaseSteps = b.stateSteps;
            switch (b.state)
            {
                case Boss37State.Reveal: a.phase = BossPhase63.Reveal; break;
                case Boss37State.Rest: a.phase = BossPhase63.Idle; break;
                case Boss37State.Tell:
                    a.phase = BossPhase63.Windup;
                    a.attack = b.Current?.id;
                    a.stepsToHit = Math.Max(0, b.stateLeft);
                    a.windupSteps = Math.Max(1, b.stateTotal);
                    break;
                case Boss37State.Active: a.phase = BossPhase63.Strike; a.attack = b.Current?.id; a.stepsToHit = 0; break;
                case Boss37State.Recover: a.phase = BossPhase63.Recover; a.attack = b.Current?.id; break;
                case Boss37State.Window: a.phase = BossPhase63.Stagger; a.why = "window"; break;
                case Boss37State.LegChange: a.phase = BossPhase63.Change; break;
                case Boss37State.Dying: a.phase = BossPhase63.Dying; break;
                default: a.phase = BossPhase63.None; break; // Off, Dead
            }
            return a;
        }

        /// <summary>THE ICEBREAKER's own states mapped (see the file's head). Pure.</summary>
        public static BossAttack63 FromBrawl(Brawl40State st, string attack, int stateLeft, int stateTotal, int stateSteps)
        {
            var a = new BossAttack63 { boss = Brawl, stepsToHit = -1, phaseSteps = stateSteps };
            switch (st)
            {
                case Brawl40State.Reveal: a.phase = BossPhase63.Reveal; break;
                case Brawl40State.Idle: a.phase = BossPhase63.Idle; break;
                case Brawl40State.Tell:
                    a.phase = BossPhase63.Windup;
                    a.attack = attack;
                    a.stepsToHit = Math.Max(0, stateLeft);
                    a.windupSteps = Math.Max(1, stateTotal);
                    break;
                case Brawl40State.Active: a.phase = BossPhase63.Strike; a.attack = attack; a.stepsToHit = 0; break;
                case Brawl40State.Stuck: a.phase = BossPhase63.Stagger; a.attack = attack; a.why = attack == Brawl40.Rush ? "dazed" : "stuck"; break;
                case Brawl40State.Recover: a.phase = BossPhase63.Recover; a.attack = attack; break;
                case Brawl40State.Window: a.phase = BossPhase63.Stagger; a.why = "window"; break;
                case Brawl40State.PhaseChange: a.phase = BossPhase63.Change; break;
                case Brawl40State.Dying: a.phase = BossPhase63.Dying; break;
                default: a.phase = BossPhase63.None; break; // Off, Dead
            }
            return a;
        }

        /// <summary>THE KILN WYRM from its attack log (see the file's head): the phase strings ("intro", "beat", "dying", "dead") first; then a hit's
        /// strike for Boss63WyrmStrikeS after it (only Boss63WyrmStrikeOverS when another tell is already pending); then the pending tell whose hit
        /// is nearest (its wind-up); then the stagger (Wyrm31.Staggered); else idle. NIGHT63 fix round: the worm tells while staggered (n63-b2's
        /// spit inside an 18.0-24.3 s stagger), so a pending tell outranks the stagger, else its wind-up hides until the stagger ends. Pure.</summary>
        public static BossAttack63 FromWyrm(IList<WyrmLog> log, int stepNo, string phase, bool staggered, double dt)
        {
            var a = new BossAttack63 { boss = Wyrm, stepsToHit = -1 };
            if (phase == null || phase == "dead") { a.phase = BossPhase63.None; return a; }
            if (phase == "dying") { a.phase = BossPhase63.Dying; return a; }
            if (phase == "intro") { a.phase = BossPhase63.Reveal; return a; }
            if (phase == "beat") { a.phase = BossPhase63.Change; return a; }
            a.phase = staggered ? BossPhase63.Stagger : BossPhase63.Idle;
            if (staggered) a.why = "staggered";
            if (log == null || log.Count == 0) return a;
            int strike = Math.Max(1, MovementMath.StepsFor(UnityTuning.Boss63WyrmStrikeS, dt));
            int over = Math.Max(1, MovementMath.StepsFor(UnityTuning.Boss63WyrmStrikeOverS, dt));
            int bestAhead = int.MaxValue, bestStruck = int.MaxValue;
            string aheadId = null, struckId = null;
            int aheadLen = 0, aheadTell = 0;
            // the log grows through the fight: the last 24 entries hold every tell still ahead (a tell is 0.5-1.5 s; the log's entries come a few a second)
            for (int i = log.Count - 1, seen = 0; i >= 0 && seen < 24; i--, seen++)
            {
                WyrmLog e = log[i];
                if (e.resolve || e.seconds <= 0 || e.step > stepNo) continue; // a resolve, or a tell not given yet
                int len = MovementMath.StepsFor(e.seconds, dt), hit = e.step + len;
                if (hit > stepNo)
                {
                    if (hit < bestAhead) { bestAhead = hit; aheadId = e.name; aheadLen = len; aheadTell = e.step; }
                }
                else if (stepNo - hit < strike && stepNo - hit < bestStruck) { bestStruck = stepNo - hit; struckId = e.name; }
            }
            if (struckId != null && (aheadId == null || bestStruck < over))
            {
                a.phase = BossPhase63.Strike;
                a.attack = struckId;
                a.stepsToHit = 0;
                a.phaseSteps = bestStruck;
                a.why = null;
                return a;
            }
            if (aheadId != null)
            {
                a.phase = BossPhase63.Windup;
                a.attack = aheadId;
                a.stepsToHit = bestAhead - stepNo;
                a.windupSteps = Math.Max(1, aheadLen);
                a.phaseSteps = stepNo - aheadTell;
                a.why = null;
            }
            return a;
        }

        // --- the boss63 reel's proof lines (L-089: each prints that its input fired; ms on the reel's fixed clock, from the boss's rise) ---

        static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

        static string Ms(double ms) => Math.Round(ms, MidpointRounding.AwayFromZero).ToString("0", Ci);

        /// <summary>"[boss63] attack &lt;id&gt; windup &lt;ms&gt; hit &lt;ms&gt; lead &lt;ms&gt; pose &lt;ms | off&gt;": the wind-up's first step, the strike's step,
        /// the lead between them, and how long before the hit the drawn wind-up pose was visible (BossMotion63; poseMs &lt; 0: off / never).</summary>
        public static string AttackLine(string id, double windupMs, double hitMs, double poseMs) =>
            "[boss63] attack " + (id ?? "?") + " windup " + Ms(windupMs) + " hit " + Ms(hitMs) + " lead " + Ms(hitMs - windupMs)
            + " pose " + (poseMs < 0 ? "off" : Ms(hitMs - poseMs));

        /// <summary>"[boss63] flinch &lt;ms&gt; hp &lt;before&gt; -&gt; &lt;after&gt; drawn +&lt;n&gt; frames | off": a damage event and the frames until the drawn flinch.</summary>
        public static string FlinchLine(double ms, double hpBefore, double hpAfter, int drawnFrames) =>
            "[boss63] flinch " + Ms(ms) + " hp " + hpBefore.ToString("0.0", Ci) + " -> " + hpAfter.ToString("0.0", Ci) + " drawn " + (drawnFrames < 0 ? "off" : "+" + drawnFrames + " frames");

        /// <summary>"[boss63] stagger &lt;ms&gt; &lt;why&gt;[ FORCED]": a stagger began (FORCED: the reel emptied the boss's meter itself).</summary>
        public static string StaggerLine(double ms, string why, bool forced) => "[boss63] stagger " + Ms(ms) + " " + (why ?? "?") + (forced ? " FORCED" : "");

        /// <summary>"[boss63] stagger end &lt;ms&gt; after &lt;ms&gt;".</summary>
        public static string StaggerEndLine(double ms, double lastedMs) => "[boss63] stagger end " + Ms(ms) + " after " + Ms(lastedMs);

        /// <summary>"[boss63] rms &lt;boss&gt; s&lt;n&gt; &lt;m&gt; m parts &lt;n&gt; max &lt;m&gt; m limit &lt;share&gt;": second n's rms offset of the drawn parts from their rigid
        /// pose (BossMotion63Probe), the parts moved, the largest pivot offset, and the largest share of a part's config limit used.</summary>
        public static string RmsLine(string boss, int second, double rms, int parts, double max, double limitShare) =>
            "[boss63] rms " + (boss ?? "?") + " s" + second + " " + rms.ToString("0.0000", Ci) + " m parts " + parts + " max " + max.ToString("0.000", Ci) + " m limit " + limitShare.ToString("0.00", Ci);
    }
}
