// NIGHT19 U3-33.5 (builder W, design.gust): the KESTREL's wind gust. GG 2026-10-06: "for the first weapon if you use the
// left trigger ... it'll be like kind of more of a wind gust, it's more of a movement option". With design.gust on, the
// KESTREL's alt-fire (LT, right mouse; the SPECIAL key too) is the gust and the zoom goes (Loadout.StepAds):
// - the player is pushed opposite the aim through Mover.Impulse (mover.js impulse: a dash keeps its own velocity, the
//   no-stacking rule). A gust is not fired at all during a dash, so no charge is spent there;
// - light enemies (design.gust.lightKinds) in a cone ahead are pushed and staggered: the web's THUNDERCLAP variant cone
//   wave (weapons.js coneWave, :640-656, with weapons.specials.shotgun.shock: a 30 deg cone, 8 m, push 12, stagger 1),
//   ported line for line in ConeWave below; its push time (the web's literal 0.25 s there) is design.gust.pushTime, 0.3 s
//   (the web's THUNDERCLAP double-blast push time: 0.25 left SWARMERs under 3 m back, GustTests);
// - one charge per airtime plus one on the ground: on the ground a gust fires once design.gust.cooldown s have passed
//   since the last one; in the air one gust fires (after the same cooldown) and that air charge refills on landing.
// Emits N19Events.Gust (a reused GustEvent) for the views (V: the air, the kick) and GG's sounds. No damage, no
// freshness: it is a movement tool, not a gun. Allocation-free (a reused list and payload).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.gust, plus the web's cone (weapons.specials.shotgun.shock), read once. Null when the design config
    /// has no gust numbers (the web's config alone: the golden sets), so the gust never fires there.</summary>
    public sealed class GustParams
    {
        public double selfPush, cooldown, pushTime, staminaCost, hipSpreadDeg;
        public double coneDeg, range, push, stagger; // the web's weapons.specials.shotgun.shock
        public readonly List<string> lightKinds = new List<string>();

        public static GustParams From(GameConfig cfg)
        {
            if (!cfg.Has("design.gust.selfPush")) return null;
            var p = new GustParams
            {
                selfPush = cfg.Num("design.gust.selfPush"),
                cooldown = cfg.Num("design.gust.cooldown"),
                pushTime = cfg.Num("design.gust.pushTime"),
                staminaCost = cfg.Num("design.gust.staminaCost"),
                hipSpreadDeg = cfg.Num("design.gust.kestrelHipSpreadDeg"),
                coneDeg = cfg.Num("weapons.specials.shotgun.shock.coneDeg"),
                range = cfg.Num("weapons.specials.shotgun.shock.range"),
                push = cfg.Num("weapons.specials.shotgun.shock.push"),
                stagger = cfg.Num("weapons.specials.shotgun.shock.stagger"),
            };
            p.lightKinds.AddRange(cfg.Strs("design.gust.lightKinds"));
            return p;
        }
    }

    /// <summary>The gust's charges and its fire (one per loadout; Loadout owns it).</summary>
    public sealed partial class KestrelGust
    {
        public readonly GustParams p;
        int cooldownSteps;
        bool airUsed;
        /// <summary>Gusts fired this run (the stats, the tests).</summary>
        public int fired;
        readonly GustEvent gustEvent = new GustEvent();
        readonly List<Enemy> near = new List<Enemy>();

        public KestrelGust(GustParams p) => this.p = p;

        /// <summary>A new run: both charges ready.</summary>
        public void Reset()
        {
            cooldownSteps = 0;
            airUsed = false;
            fired = 0;
        }

        /// <summary>One fixed step (after the mover's): the cooldown runs, and landing refills the air charge.</summary>
        public void Step(Mover mover)
        {
            if (cooldownSteps > 0) cooldownSteps--;
            if (mover.grounded) airUsed = false;
        }

        /// <summary>Can a gust fire now? Never in a dash; on the ground after the cooldown; in the air once per airtime.</summary>
        public bool Ready(Mover mover) => p != null && mover.state != MoveState.Dash && cooldownSteps == 0 && (mover.grounded || !airUsed);

        /// <summary>Is the air charge spent (the HUD's pip, the bot)?</summary>
        public bool AirUsed => airUsed;

        /// <summary>NIGHT20 (builder G, the HUD's alt meter): the cooldown's share refilled, 0..1 (1: ready).</summary>
        public double Refilled(double dt) => p == null || cooldownSteps <= 0 ? 1 : 1 - cooldownSteps / (double)MovementMath.StepsFor(p.cooldown, dt);

        /// <summary>NIGHT20 (the HUD's alt meter): the charges ready now (the ground's after the cooldown, the airtime's while unspent).</summary>
        public int ChargesReady => (cooldownSteps == 0 ? 1 : 0) + (airUsed ? 0 : 1);

        /// <summary>
        /// Fire a gust from `origin` along the unit aim `dir`: the self push opposite the aim, the cone wave, the event.
        /// False (nothing happens) when it isn't ready.
        /// </summary>
        public bool Fire(Mover mover, EnemyRegistry enemies, Vec3 origin, Vec3 dir, Func<Vec3, Vec3, bool> hasLos, EventBus events, string owner, double dt)
        {
            if (!Ready(mover)) return false;
            bool air = !mover.grounded;
            if (air) airUsed = true;
            cooldownSteps = MovementMath.StepsFor(p.cooldown, dt);
            // The push opposite the aim (mover.js impulse: an upward share lifts you off the ground, no apex hang).
            mover.Impulse(-dir.x * p.selfPush, -dir.y * p.selfPush, -dir.z * p.selfPush);
            int pushed = ConeWave(enemies, origin, dir, hasLos, dt);
            fired++;
            gustEvent.owner = owner;
            gustEvent.origin = origin;
            gustEvent.dir = dir;
            gustEvent.selfPush = p.selfPush;
            gustEvent.pushed = pushed;
            gustEvent.airborne = air;
            events.Emit(N19Events.Gust, gustEvent);
            return true;
        }

        /// <summary>
        /// weapons.js coneWave (the Thunderclap variant's cone shockwave): every body in a cone ahead is pushed and
        /// staggered. Port-only: only design.gust.lightKinds are moved (the heavy kinds stand). Returns how many.
        /// </summary>
        int ConeWave(EnemyRegistry enemies, Vec3 origin, Vec3 dir, Func<Vec3, Vec3, bool> hasLos, double dt)
        {
            double cos = JsMath.Cos((p.coneDeg * Math.PI) / 180);
            int hit = 0;
            enemies.Within(origin, p.range, near);
            for (int i = 0; i < near.Count; i++)
            {
                Enemy enemy = near[i];
                if (!p.lightKinds.Contains(enemy.kind)) continue;
                Vec3 aim = enemy.AimPoint;
                Vec3 coneDir = aim - origin;
                double d = coneDir.Length;
                if (d > p.range || Vec3.Dot(coneDir, dir) / (d != 0 ? d : 1) < cos || !hasLos(origin, aim)) continue;
                enemy.Knockback(enemy.position.x - origin.x, enemy.position.z - origin.z, p.push, p.pushTime, dt);
                enemy.Stun(p.stagger, dt);
                hit++;
            }
            return hit;
        }
    }
}
