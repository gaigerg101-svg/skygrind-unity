// NIGHT47 builder F (design.n47.pulse47, a sim row; NIGHT47.md G20; GG 2026-10-18: "The left-trigger special attack needs to be significantly more
// powerful, with a heavier feel ... right now it feels like a weak gust of wind with no weight"). The PULSE gauntlet's LT blast (PulseCharge27) with
// the row on: every body it hits is LAUNCHED (launch36's flight and burst: a launched body that meets another body or a wall bursts), not shoved
// 1.4 m; a second ring round the player catches what stands beside you (Doom Eternal's Blood Punch: an inner hit and an outer shockwave, R9 in
// Research/2026-10-18-wave47/F.md); more damage; a sim hitstop. Bodies that ignore a launch (not husk-shaped: bosses, the bulwark, the burrower)
// take the damage and a stun. The pips, the 15 s cooldown and the boost are PulseCharge27's, unchanged (CORE: a crowd-control verb needs 15 s+).
// Numbers: design.n47.pulse47.numbers (Blast47Params.From; never here). Off (PulseCharge27.blast47 null): NIGHT46's blast loop bit for bit.
// Event (new name, for GG's sounds): pulse47Blast {hits, launched, ring}; pulse27Blast still fires after it (PulseCharge27.Announce).
using System;

namespace Skygrind.Core
{
    /// <summary>design.n47.pulse47.numbers.</summary>
    public sealed class Blast47Params
    {
        public double launchSpeed, lift, flightS, range, radius, damageMult, ringRadius, ringDamageShare, hitstopS, staggerS;

        public static Blast47Params From(GameConfig cfg)
        {
            const string p = "design.n47.pulse47.numbers.";
            return new Blast47Params
            {
                launchSpeed = cfg.Num(p + "launchSpeed"),
                lift = cfg.Num(p + "lift"),
                flightS = cfg.Num(p + "flightS"),
                range = cfg.Num(p + "range"),
                radius = cfg.Num(p + "radius"),
                damageMult = cfg.Num(p + "damageMult"),
                ringRadius = cfg.Num(p + "ringRadius"),
                ringDamageShare = cfg.Num(p + "ringDamageShare"),
                hitstopS = cfg.Num(p + "hitstopS"),
                staggerS = cfg.Num(p + "staggerS"),
            };
        }
    }

    public sealed class Blast47Event
    {
        public int hits, launched, ring;
    }

    public sealed partial class PulseCharge27
    {
        /// <summary>NIGHT47 F pulse47: the blast's numbers while the row is on (set by the sim at a run's start, SurvivalSim.N47.Feel.cs); null =
        /// NIGHT46's blast. Not reset by Reset (the sim sets it each run).</summary>
        public Blast47Params blast47;

        /// <summary>The run's pulse47 counters (the harness reads them).</summary>
        public int blast47Hits, blast47Launched, blast47Ring;
    }

    public sealed partial class Loadout
    {
        readonly Blast47Event blast47Event = new Blast47Event();

        /// <summary>NIGHT47 pulse47: the blast from `origin` along the unit `dir` (StepPulse27 calls it after Spend, instead of NIGHT46's loop).
        /// Returns the bodies hit (the count PulseCharge27.Announce reports).</summary>
        int Blast47(PulseCharge27 pc, Vec3 origin, Vec3 dir, double dt)
        {
            Blast47Params b = pc.blast47;
            int hits = 0, launched = 0, ring = 0;
            double full = gDamage * b.damageMult;
            for (int i = 0; i < enemies.all.Count; i++)
            {
                Enemy e = enemies.all[i];
                if (!e.alive) continue;
                Vec3 rel = e.AimPoint - origin;
                double t = Vec3.Dot(rel, dir);
                bool onLine = false;
                if (t >= 0 && t <= b.range)
                {
                    Vec3 off = rel - dir * t;
                    onLine = off.Length <= b.radius + e.Radius + BossMargin34(e);
                }
                bool inRing = false;
                double ringDx = 0, ringDz = 0;
                if (!onLine)
                {
                    ringDx = e.position.x - origin.x;
                    ringDz = e.position.z - origin.z;
                    inRing = JsMath.Hypot(ringDx, ringDz) <= b.ringRadius + e.Radius;
                }
                if (!onLine && !inRing) continue;
                double lx = onLine ? dir.x : ringDx, lz = onLine ? dir.z : ringDz;
                if (JsMath.Hypot(lx, lz) < 1e-6) { lx = dir.x; lz = dir.z; } // straight up / down: along the aim's flat part
                if (JsMath.Hypot(lx, lz) < 1e-6) { lx = 0; lz = -1; }
                Pushed20(e);
                Damage(e, onLine ? full : full * b.ringDamageShare, false, "gauntlet", origin, false);
                hits++;
                if (!onLine) ring++;
                if (!e.alive) continue;
                if (e is Husk h && !h.standIn29 && !h.tank29)
                {
                    if (launch36 != null) launch36.Launch(h, lx, lz, dt, events); // the flight and its burst (launch36's), then this row's throw
                    h.Knockback(lx, lz, b.launchSpeed, b.flightS, dt);
                    h.Launch(b.lift);
                    launched++;
                }
                else if (e is Husk)
                {
                    e.Knockback(lx, lz, b.launchSpeed, b.flightS * 0.5, dt); // a tank / stand-in: a hard shove, no flight
                }
                else e.Stun(b.staggerS, dt); // a body that ignores a launch (a boss, the bulwark, the burrower): it reels instead
            }
            if (hits > 0 && b.hitstopS > 0 && hitstop41 != null) hitstop41(b.hitstopS, "pulse47");
            pc.blast47Hits += hits;
            pc.blast47Launched += launched;
            pc.blast47Ring += ring;
            blast47Event.hits = hits;
            blast47Event.launched = launched;
            blast47Event.ring = ring;
            events.Emit("pulse47Blast", blast47Event);
            return hits;
        }
    }
}
