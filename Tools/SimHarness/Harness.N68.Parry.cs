// NIGHT68 builder T, slice I2's fix round (Logs/n68/verdict-I2.md: kb3's 40 hp husks died to 43-53 PARRY punches, and a parry never launches,
// Combat.cs `launch = launch36 != null && !parry && Wants`): would a punch on this body right now be a parry? The game's own two tests, read
// here without changing anything: the body is winding up (CombatMath.IsParry(windupSteps), Combat.cs's `parry`), or parry36's early buffer
// would take it (Parry36.cs Imminent36: a plain Husk, not winding up, not staggered or knocked, grounded, not leaping, not a stand-in, its brain
// letting it attack, its attack cooldown at or under parry36's bufferSteps, the player inside its meleeRange). The launch pilot and the launch
// probe skip such a target at the trigger and hold the punch while it holds (Harness.N65.VerbPilot.cs, Harness.N65.Verb.cs V65Launch).
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>Why a punch on `e` now would be a parry (no launch), or null: "winding up" or "attack-ready inside parry36's early window".</summary>
    static string V68ParryWhy(SurvivalSim sim, Enemy e)
    {
        if (e == null || !e.alive) return null;
        if (CombatMath.IsParry(e.windupSteps)) return "winding up";
        Parry36 p36 = sim.loadout.parry36;
        if (p36 == null || e.GetType() != typeof(Husk)) return null;
        if (e.staggerSteps > 0 || e.knockSteps > 0) return null;
        var h = (Husk)e;
        if (!h.grounded || h.leaping || h.standIn29 || (h.ai != null && !h.ai.attack)) return null;
        if (h.cooldownSteps > p36.bufferSteps) return null;
        Vec3 me = sim.mover.position;
        if (JsMath.Hypot(me.x - h.position.x, me.y - h.position.y, me.z - h.position.z) > h.Params.meleeRange) return null;
        return "attack-ready inside parry36's early window";
    }

    /// <summary>The punch hits the nearest body in its sweep (Combat: SweepTargets over punch.range and coneHalfAngleDeg), not always the
    /// target (kb3 seed 3: the punch went to another husk, which was winding up: a parry). Why a punch now along (yaw, pitch) would be a parry on
    /// ANY body it could pick (the target or another inside the reach + 0.5 m and the cone + 20 deg, a probe margin), or null.</summary>
    static string V68PunchParryWhy(SurvivalSim sim, Enemy target, double yaw, double pitch)
    {
        string w = V68ParryWhy(sim, target);
        if (w != null) return "the target " + w;
        double range = sim.cfg.Num("punch.range") + 0.5, half = (sim.cfg.Num("punch.coneHalfAngleDeg") + 20) * System.Math.PI / 180;
        Vec3 eye = sim.Eye;
        double ax = -System.Math.Sin(yaw) * System.Math.Cos(pitch), ay = System.Math.Sin(pitch), az = -System.Math.Cos(yaw) * System.Math.Cos(pitch);
        foreach (Enemy e in sim.enemies.all)
        {
            if (!e.alive || e == target || e.standIn29) continue;
            Vec3 d = e.AimPoint - eye;
            double len = d.Length;
            if (len - 0.5 > range || len < 1e-6) continue;
            double cos = (d.x * ax + d.y * ay + d.z * az) / len;
            if (System.Math.Acos(System.Math.Max(-1, System.Math.Min(1, cos))) > half) continue;
            string we = V68ParryWhy(sim, e);
            if (we != null) return $"another body in the punch's reach ({e.kind} #{e.id}) {we}";
        }
        return null;
    }
}
