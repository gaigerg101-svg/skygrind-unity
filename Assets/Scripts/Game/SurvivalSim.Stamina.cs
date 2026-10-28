// NIGHT17 U3-30.4 (builder M owns this file): one bar, a sprint and a partial boost (Core/Stamina.cs) in the sim.
// design.stamina is read once (StaminaInit, in the constructor); each run sets the player's Mover rules from design.stamina's
// flags (no LAB rows tonight: the design flags are the switches; automation reads the same defaults), or, for a tape's
// replay, the flags it recorded (staminaOverride; a format 1-2 tape: every flag off, the web's pips). The rules, the bar and
// its clocks live in the Mover (Mover.stamina), so the run snapshot saves them with the mover (MoverState.Fields) and a
// resumed run keeps the flags it started with. The rewards: the bus's `kill` (the player's) adds rewards.kill, `parry`
// rewards.parry (the slam's landing is the Mover's own). The mover's port-only events reach the bus for GG's sounds:
// `staminaFull`, `sprintStart`, `sprintEnd` (payload: the owner id "player") and `boostPartial` (payload: an int, the boost's
// percent of a full boost, 25 .. 89). No allocation per step (the handlers are made once; the payloads are cached boxes).
// NIGHT19 U3-33.7 (builder E owns this file tonight; design.stamina.n19, Stamina.N19Params): with the run's stamina block
// on (n19.stamina, set before StaminaStartRun), the sprint drains n19.drain instead of sprint.drain; a player's kill
// refills by its distance from the player's eye (Stamina.KillReward: killNear within nearRange, killFar beyond farRange,
// rewards.kill between); with slamOnHit the Mover's own landing reward is 0 and the slam's landing refills rewards.slam only
// when an enemy is inside the slam's shockwave (the web's `within`: an aim point within shockwaveRadius + its radius). With
// the block off every rule is NIGHT18's (the handlers read the run's flag; no number is touched).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>design.stamina, read once (Params.Neutral when the design config has no stamina block: the web's pips).</summary>
        Stamina.Params staminaDesign;

        /// <summary>A tape's replay: the five flags its run read (bar, boost, sprint, refill, rewards); null: the design defaults.</summary>
        public bool[] staminaOverride;

        /// <summary>NIGHT19: design.stamina.n19's numbers, read once; applied only while staminaN19On.</summary>
        Stamina.N19Params staminaN19;

        /// <summary>NIGHT19: this run plays design.stamina.n19 (its stamina block on, with the bar).</summary>
        bool staminaN19On;

        /// <summary>NIGHT19: the slam's refill with slamOnHit (the run's rewards.slam; the Mover's own is 0 then).</summary>
        double staminaSlamOnHit;

        /// <summary>The flags this run read (bar, boost, sprint, refill, rewards): what a tape records.</summary>
        public bool StaminaFlag(int i) => mover.stamina.rules.Flag(i);

        partial void StaminaInit()
        {
            staminaDesign = Stamina.Params.From(cfg);
            staminaN19 = Stamina.N19Params.From(cfg);
            mover.Events += StaminaOnMover;
            events.On("kill", StaminaOnKill);
            events.On("parry", StaminaOnParry);
        }

        void StaminaOnMover(MoverEvent e)
        {
            switch (e.type)
            {
                case MoverEventType.StaminaFull: events.Emit(Stamina.FullEvent, "player"); break;
                case MoverEventType.SprintStart: events.Emit(Stamina.SprintStartEvent, "player"); break;
                case MoverEventType.SprintEnd: events.Emit(Stamina.SprintEndEvent, "player"); break;
                case MoverEventType.BoostPartial: events.Emit(Stamina.BoostPartialEvent, (int)System.Math.Round(e.fraction * 100)); break;
                case MoverEventType.Slam:
                    // NIGHT19 slamOnHit: the landing refills only when the shockwave reaches an enemy.
                    if (staminaN19On && staminaN19.slamOnHit && StaminaSlamHits(e.position)) mover.StaminaReward(staminaSlamOnHit);
                    break;
            }
        }

        /// <summary>NIGHT19: a live enemy inside the slam's shockwave at `at` (weapons.js slamShockwave's `within`: an aim point
        /// within shockwaveRadius + the enemy's radius).</summary>
        bool StaminaSlamHits(Vec3 at)
        {
            double r = staminaN19.slamRadius;
            for (int i = 0; i < enemies.all.Count; i++)
            {
                Enemy e = enemies.all[i];
                if (e.alive && Vec3.DistanceTo(e.AimPoint, at) <= r + e.Radius) return true;
            }
            return false;
        }

        void StaminaOnKill(object payload)
        {
            if (!(payload is KillEvent k) || k.owner != "player") return;
            double amount = mover.stamina.rules.kill;
            if (staminaN19On)
            {
                // NIGHT19: the refill by the kill's distance from the player's eye
                Vec3 p = mover.position;
                double dx = k.position.x - p.x, dy = k.position.y - (p.y + cfg.movement.body.eyeHeight), dz = k.position.z - p.z;
                amount = Stamina.KillReward(staminaN19, amount, JsMath.Hypot(dx, dy, dz));
            }
            mover.StaminaReward(amount);
        }

        void StaminaOnParry(object payload)
        {
            if (payload is HitEvent h && h.owner == "player") mover.StaminaReward(mover.stamina.rules.parry);
        }

        partial void StaminaStartRun(bool fromSettings)
        {
            Stamina.Params d = staminaDesign;
            staminaN19On = false;
            if (d.max <= 0)
            {
                mover.SetStamina(Stamina.Params.Neutral); // no design.stamina block: the web's pips whatever a tape says
                return;
            }
            if (staminaOverride != null) d = d.With(staminaOverride[0], staminaOverride[1], staminaOverride[2], staminaOverride[3], staminaOverride[4]);
            // NIGHT19 (design.stamina.n19, the run's stamina block): the drain, and the slam's refill moved to its hit.
            staminaN19On = n19.stamina && d.Any && staminaN19.drain > 0;
            if (staminaN19On)
            {
                d.drain = staminaN19.drain;
                staminaSlamOnHit = d.slam;
                if (staminaN19.slamOnHit) d.slam = 0;
            }
            mover.SetStamina(d);
        }
    }
}
