// NIGHT29 builder B (boss1): THE LANTERN reworked (GG 2026-10-09: "I'm not able to attack the first boss ... floor is lava ... exposed ... slow motion barrier"). A wrapper on Lantern27
// (which stays the OFF path, bit for bit): Lantern27.Filter used to return 0 unless the shot hit the face that looks at the ridden wall; with this rule on (design.n29.boss1) ANY gun
// anywhere lands `lightDamage` of a hit, and the full hit lands only while the core is EXPOSED (the floor is live) AND the shooter is WALL RIDING (a platform does not count).
// FLOOR IS LAVA, a cycle of `cycleSeconds` in whole steps: rest, then a TELL (tellSeconds: the arena glow shifts, the core opens, no sound), then the floor LIVE (liveSeconds: the floor
// hurts damagePerSecond in ticks of the hazardFloor27 tick, a wall rider is safe; the hazardFloor27 mechanism). While live the world slows to slowScale with a slowEaseS ease: this class
// only EXPOSES the eased value (SlowScale); the world's time share is driven by the lead (SurvivalGame). Nothing here kills in under 0.5 s (8 damage per second in 0.25 s ticks).
// Fixed 120 Hz, timers in whole steps, no randomness, no audio, no UnityEngine.
using System;

namespace Skygrind.Core
{
    /// <summary>The hit rule Lantern27 asks when a rule is attached (NIGHT29).</summary>
    public interface ILanternRule29
    {
        /// <summary>The share of a normal hit that lands now (1 = full).</summary>
        double HitScale();
    }

    public enum Lantern29Stage { Rest, Tell, Live }

    /// <summary>The payload of the "lantern29Stage" event (the view's glow and the HUD's tag).</summary>
    public sealed class Lantern29Event
    {
        public Lantern29Stage stage;
    }

    /// <summary>design.n29.boss1 (and the hazardFloor27 tick and stand height), read once.</summary>
    public sealed class Lantern29Params
    {
        public double attackTellMin, lightDamage, cycleSeconds, tellSeconds, liveSeconds, damagePerSecond, slowScale, slowEaseS, tickSeconds, standHeight;

        public static Lantern29Params From(GameConfig c)
        {
            const string g = "design.n29.boss1.";
            return new Lantern29Params
            {
                attackTellMin = c.Num(g + "attackTellMinSeconds"), lightDamage = c.Num(g + "lightDamage"), cycleSeconds = c.Num(g + "cycleSeconds"), tellSeconds = c.Num(g + "tellSeconds"), liveSeconds = c.Num(g + "liveSeconds"),
                damagePerSecond = c.Num(g + "damagePerSecond"), slowScale = c.Num(g + "slowScale"), slowEaseS = c.Num(g + "slowEaseS"),
                tickSeconds = c.Num("design.n27.hazardFloor27.tickSeconds"), standHeight = c.Num("design.n27.hazardFloor27.standHeight"),
            };
        }
    }

    public sealed class Lantern29 : ILanternRule29
    {
        public readonly Lantern29Params p;
        public readonly Lantern27 inner;
        readonly EventBus events;
        readonly BossTarget target;
        readonly double dt0;
        int cycleSteps, tellSteps, liveSteps, tickSteps, pos, tick, easeSteps, lastPhaseSteps = int.MaxValue;
        double scale = 1;

        /// <summary>The sim hands this in: is the player wall riding right now (the Mover's own state).</summary>
        public Func<bool> playerRiding;
        public Lantern29Stage Stage { get; private set; } = Lantern29Stage.Rest;
        /// <summary>Damage the live floor dealt this fight (the harness line's number).</summary>
        public double floorDamage;
        public int floorTicks;
        public int fullHits, lightHits;

        public Lantern29(GameConfig cfg, Lantern27 inner, EventBus events, BossTarget target)
        {
            p = Lantern29Params.From(cfg);
            this.inner = inner;
            this.events = events;
            this.target = target;
            dt0 = cfg.Dt;
            inner.tellMin29 = p.attackTellMin;
            cycleSteps = S(p.cycleSeconds);
            tellSteps = S(p.tellSeconds);
            liveSteps = S(p.liveSeconds);
            tickSteps = Math.Max(1, S(p.tickSeconds));
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        /// <summary>NIGHT30 (design.n30.lantern30): the floor's tell in seconds (0 or less: design.n29.boss1.tellSeconds, NIGHT29's). The cycle's rest takes up the difference.</summary>
        public void Tell30(double sec) => tellSteps = S(sec > 0 ? sec : p.tellSeconds);

        /// <summary>The floor is live (the core is open): the world slows, the floor hurts.</summary>
        public bool FloorLive => Stage == Lantern29Stage.Live;
        /// <summary>The core is exposed: a wall rider's shots land in full.</summary>
        public bool CoreExposed => Stage == Lantern29Stage.Live;
        /// <summary>The world's time share the view reads (1 = normal; slowScale while the floor is live; eased over slowEaseS).</summary>
        public double SlowScale => scale;
        /// <summary>0..1 through the current tell (the glow's shift).</summary>
        public double TellProgress => Stage == Lantern29Stage.Tell ? (double)(pos - RestSteps) / tellSteps : 0;
        /// <summary>0..1 through the live phase (the HUD's countdown), 0 outside it.</summary>
        public double LiveProgress => Stage == Lantern29Stage.Live ? (double)(pos - RestSteps - tellSteps) / liveSteps : 0;
        int RestSteps => Math.Max(0, cycleSteps - tellSteps - liveSteps);

        /// <summary>The share of a hit that lands: full only with the core exposed and the shooter wall riding, lightDamage anywhere else.</summary>
        public double HitScale()
        {
            bool full = CoreExposed && playerRiding != null && playerRiding();
            if (full) fullHits++; else lightHits++;
            return full ? 1.0 : p.lightDamage;
        }

        public void Reset()
        {
            pos = 0;
            tick = 0;
            scale = 1;
            lastPhaseSteps = int.MaxValue;
            SetStage(Lantern29Stage.Rest);
            floorDamage = 0;
            floorTicks = 0;
            fullHits = lightHits = 0;
        }

        void SetStage(Lantern29Stage s)
        {
            if (s == Stage) return;
            Stage = s;
            tick = 0;
            events.Emit("lantern29Stage", new Lantern29Event { stage = s });
        }

        public void Step(double dt)
        {
            bool fighting = inner.Alive && (inner.phase == "p1" || inner.phase == "p2" || inner.phase == "p3");
            // a retry restarts a phase (phaseSteps goes back): the cycle starts over with its rest
            if (inner.phaseSteps < lastPhaseSteps) { pos = 0; tick = 0; }
            lastPhaseSteps = inner.phaseSteps;
            double want = 1;
            if (fighting)
            {
                int rest = RestSteps;
                SetStage(pos < rest ? Lantern29Stage.Rest : pos < rest + tellSteps ? Lantern29Stage.Tell : Lantern29Stage.Live);
                if (++pos >= cycleSteps) pos = 0;
                if (Stage == Lantern29Stage.Live) { want = p.slowScale; StepFloor(); }
            }
            else SetStage(Lantern29Stage.Rest);
            // the world's slow share eases at (1 - slowScale) per slowEaseS
            double rate = (1 - p.slowScale) / Math.Max(1, S(p.slowEaseS));
            scale = scale < want ? Math.Min(want, scale + rate) : Math.Max(want, scale - rate);
        }

        /// <summary>The hazardFloor27 mechanism over the whole floor: feet within standHeight of the floor and not wall riding take damagePerSecond in ticks.</summary>
        void StepFloor()
        {
            Vec3 f = target.position();
            bool riding = playerRiding != null && playerRiding();
            if (riding || f.y > inner.OriginWorld.y + p.standHeight) { tick = 0; return; }
            if (++tick < tickSteps) return;
            tick = 0;
            double amount = p.damagePerSecond * tickSteps * dt0;
            Health h = target.health;
            if (h != null && !h.Dead && h.Damage(amount, "hazard", f))
            {
                floorDamage += amount;
                floorTicks++;
            }
        }
    }
}
