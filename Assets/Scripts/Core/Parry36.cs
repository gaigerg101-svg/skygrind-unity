// NIGHT36 builder P (parry36; NIGHT36.md card P1, PLAN.md section 1 "the parry is the centre"): the parry's payoff, on top of the web's parry
// (punch.js: a punch in a wind-up staggers the attacker, heals punch.parryHeal; NIGHT8: a punch reflects a spitter's glob; design.ultra: the
// 0.25 s freeze and the dash refill). With the row on:
// - THE EARLY BUFFER: a punch up to bufferS before a parry opens still parries (ULTRAKILL Enemy.cs:2698-2842: parryFramesLeft = 6 physics
//   frames, Punch.cs:524; rea-ultrakill-combat.md section 3). A punch that reflects nothing keeps a glob buffer: a glob that comes into reach
//   within bufferS is parried then. A punch that hits a body out of its wind-up keeps an enemy buffer: if that body opens a wind-up within
//   bufferS, it is parried then. (A husk knocked back by the punch can't start a wind-up, so the enemy buffer mostly serves the bosses.)
// - THE RETURN: a parried glob flies back at its own shooter (alive), not the nearest body, and BURSTS where it ends (on a body or a wall):
//   returnBurstDamage to every enemy within returnBurstRadius (never the player). Drone orbs keep their own return (DroneOrbs).
// - "parry36": one event per parry (melee or projectile; a boss's sim emits it for its own projectiles), a HitEvent (owner, kind, enemy,
//   position; weapon "parry36buffer" when the buffer made it). SurvivalSim.Core36 freezes the screen freezeS on it (the sim's hitstop).
// Off (Loadout.parry36 null, SpitterAcid.onReflectEnd36 null): the web's parry bit for bit.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Parry36
    {
        public const string Event = "parry36";

        public readonly int bufferSteps;
        public readonly double freezeS, returnBurstDamage, returnBurstRadius;

        /// <summary>The enemy buffer: a body the last punch hit out of its wind-up, and the steps it may still open one in.</summary>
        public Enemy bufferEnemy;
        public int bufferEnemySteps;

        /// <summary>The glob buffer: steps a punch that reflected nothing may still catch a glob.</summary>
        public int bufferGlobSteps;

        /// <summary>Counters (the harness, the reels).</summary>
        public int parries, buffered, globReturns, orbReturns, bursts, burstHits, burstKills;

        /// <summary>The last burst's point (the harness, the view).</summary>
        public Vec3 lastBurst;

        public Parry36(GameConfig cfg)
        {
            const string p = "design.n36.parry36.";
            bufferSteps = MovementMath.StepsFor(cfg.Num(p + "bufferS"), cfg.Dt);
            freezeS = cfg.Num(p + "freezeS");
            returnBurstDamage = cfg.Num(p + "returnBurstDamage");
            returnBurstRadius = cfg.Num(p + "returnBurstRadius");
        }

        public void Reset()
        {
            bufferEnemy = null;
            bufferEnemySteps = bufferGlobSteps = 0;
            parries = buffered = globReturns = orbReturns = bursts = burstHits = burstKills = 0;
            lastBurst = default;
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT36 P parry36 (SurvivalSim.Core36 sets it at a run's start; null: off, the web's parry).</summary>
        public Parry36 parry36;

        readonly HitEvent parry36Event = new HitEvent();
        readonly List<Enemy> burst36Scratch = new List<Enemy>();

        /// <summary>parry36: a parry happened (the web's own Parried / glob reflect already healed, refilled and emitted "parry").</summary>
        void Parry36Emit(string kind, int enemyId, Vec3 at, bool buffered)
        {
            parry36.parries++;
            if (buffered) parry36.buffered++;
            parry36Event.owner = owner;
            parry36Event.weapon = buffered ? "parry36buffer" : "parry36";
            parry36Event.kind = kind;
            parry36Event.enemy = enemyId;
            parry36Event.position = at;
            parry36Event.damage = 0;
            events.Emit(Parry36.Event, parry36Event);
        }

        /// <summary>parry36: a glob the punch reflected (a real glob has a shooter; the drone orb's stand-in has none) goes back at its shooter.</summary>
        void Parry36Glob(SpitterAcid.Glob glob, bool buffered)
        {
            if (acid.IsOrbStandIn(glob))
            {
                // a drone orb: back at its own drone (DroneOrbs homes a reflected orb on its target), and it bursts where it ends
                DroneOrbs.Orb orb = acid.lastOrb36;
                Husk shooter = orb?.shooter;
                if (orb != null && shooter != null && shooter.alive) orb.target = shooter;
                parry36.orbReturns++;
                Parry36Emit("orb", shooter != null ? shooter.id : -1, glob.position, buffered);
            }
            else if (glob.shooter != null)
            {
                if (glob.shooter.alive) glob.target = glob.shooter;
                parry36.globReturns++;
                Parry36Emit("glob", glob.shooter.id, glob.position, buffered);
            }
            else Parry36Emit("glob", -1, glob.position, buffered);
        }

        /// <summary>
        /// parry36's early buffer for a melee body that has NOT started its wind-up yet: a plain husk standing in its melee reach of you,
        /// grounded, free (no stagger, no knock, no leap), allowed to attack, whose cooldown ends within bufferS, starts its wind-up in its
        /// own step within the buffer (Husk.Step: it attacks the first step it can). Here the punch's knockback would cancel that wind-up
        /// before it opened, so the parry is resolved at the punch (ULTRAKILL's punch never stops the attack; its parryFramesLeft fires the
        /// parry when the window opens: Enemy.cs:2698-2842).
        /// </summary>
        bool Imminent36(Enemy e)
        {
            if (e.GetType() != typeof(Husk) || e.windupSteps > 0 || e.staggerSteps > 0 || e.knockSteps > 0) return false;
            var h = (Husk)e;
            if (!h.grounded || h.leaping || h.standIn29 || (h.ai != null && !h.ai.attack)) return false;
            if (h.cooldownSteps > parry36.bufferSteps) return false;
            Vec3 me = mover.position;
            return JsMath.Hypot(me.x - h.position.x, me.y - h.position.y, me.z - h.position.z) <= h.Params.meleeRange;
        }

        /// <summary>The web's glob parry (StepPunch's: heal, the dash, "punch" and "parry"), shared by the buffer.</summary>
        void ParryGlobWeb(SpitterAcid.Glob glob)
        {
            events.Emit("punch", new MeleeEvent { owner = owner, weapon = "punch", hits = 1, parry = true });
            double healed = health.Heal(pParryHeal);
            if (parryRefillsDash) mover.RestoreStamina();
            events.Emit("parry", new HitEvent { owner = owner, weapon = "punch", kind = "orb", enemy = -1, damage = healed, position = glob.position });
        }

        /// <summary>parry36's per-step buffers (SurvivalSim.Core36, after the enemies stepped): a glob now in reach, a body now winding up.</summary>
        public void Parry36Step(double dt)
        {
            Parry36 p = parry36;
            if (p == null) return;
            if (p.bufferGlobSteps > 0)
            {
                p.bufferGlobSteps--;
                if (acid != null)
                {
                    var (origin, dir) = getAim();
                    SpitterAcid.Glob glob = acid.TryReflect(origin, dir, enemies);
                    if (glob != null)
                    {
                        p.bufferGlobSteps = 0;
                        p.bufferEnemy = null;
                        p.bufferEnemySteps = 0;
                        ParryGlobWeb(glob);
                        Parry36Glob(glob, true);
                        return;
                    }
                }
            }
            if (p.bufferEnemySteps > 0)
            {
                p.bufferEnemySteps--;
                Enemy e = p.bufferEnemy;
                if (e == null || !e.alive) p.bufferEnemySteps = 0;
                else if (e.windupSteps > 0)
                {
                    p.bufferEnemySteps = 0;
                    p.bufferGlobSteps = 0;
                    p.bufferEnemy = null;
                    Stagger(e, pStagger, dt);
                    Parried(e);
                    Parry36Emit(e.kind, e.id, e.AimPoint, true);
                }
                if (p.bufferEnemySteps == 0) p.bufferEnemy = null;
            }
        }

        /// <summary>parry36's return burst (SpitterAcid.onReflectEnd36): every enemy within the radius takes the burst ("orb": secondary damage, no momentum).</summary>
        public void Parry36Burst(Vec3 at)
        {
            Parry36 p = parry36;
            if (p == null) return;
            p.bursts++;
            p.lastBurst = at;
            enemies.Within(at, p.returnBurstRadius, burst36Scratch);
            int hits = 0, kills = 0;
            for (int i = 0; i < burst36Scratch.Count; i++)
            {
                Enemy e = burst36Scratch[i];
                if (!e.alive) continue;
                hits++;
                if (Damage(e, p.returnBurstDamage, false, "orb", at)) kills++;
            }
            burst36Scratch.Clear();
            p.burstHits += hits;
            p.burstKills += kills;
            events.Emit("parry36Burst", new Launch36BurstEvent { position = at, hits = hits, kills = kills });
        }
    }
}
