// NIGHT30 builder B (glacier30; DESIGN30 C2; GG 2026-10-09: "it's far too easy to just run outside the area into the slope section where it literally can't attack you", "too small",
// "I don't like how the rails currently 'stick' you"). THE GLACIER REBUILT, on top of Glacier29's rules (the other-gun floor, the catch-up window, the shard trail, the frost trail, the charge) and
// Glacier28's lap chase, both untouched with the row off. The sim builds the fight on a BIGGER closed rink (design.n30.glacier30.rink over design.n28.rink: about 44 x 70 m, the banks inside it)
// and turns SkateLane29's snap off on this path (Glacier29.noLane30); ice is the glide floor (builder S's Glide30 when it lands). This class adds the REACH:
//   PATIENCE      the player farther than farMeters (straight line) or out of the boss's sight (the world's ray from the boss to his chest blocked) for hideSeconds fills the gauge over fillSeconds;
//                 in reach it drains at drainMult x that rate. Full: a HOMING SHARD VOLLEY: a volleyTellSeconds tell (event glacierVolley "tell"), then volleyCount shards from the boss that turn
//                 toward the player at turnDegPerSecond (a sideways dash out-turns them), volleyDamage each. The gauge empties. GaugeFrac is the gap bar's patience fill.
//   FROST SWEEP   every sweepEverySeconds a band sweepWidth wide across the player's lateral position (lane, bank or lip: nowhere is out of reach), sweepHalfLength m either side of him along
//                 the lap, marked sweepTellSeconds (event glacierSweep "tell"), live sweepLiveSeconds: sweepDamage once and the frost slow (Glacier29's frostSlow) to a player inside it.
// OutOfReach (far or hidden with nothing aimed at him) is the harness's measure. Fixed 120 Hz, whole steps, Mulberry32-free (no randomness), JsMath, no audio, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Glacier30Shard
    {
        public Vec3 pos, prev, vel;
        public int life;
    }

    /// <summary>The payload of "glacierVolley" / "glacierSweep": stage "tell" / "fire" (volley) or "tell" / "live" / "end" (sweep).</summary>
    public sealed class Glacier30Event
    {
        public string stage;
        public double seconds;
    }

    /// <summary>design.n30.glacier30 (every number Unity-only; each has its _why).</summary>
    public sealed class Glacier30Params
    {
        public double farMeters, hideSeconds, fillSeconds, drainMult, volleyTellSeconds, volleySpeed, turnDegPerSecond, volleyLife, volleyDamage, volleyRadius, volleySpreadDeg;
        public int volleyCount;
        public double sweepEverySeconds, sweepFirstSeconds, sweepTellSeconds, sweepLiveSeconds, sweepWidth, sweepHalfLength, sweepDamage, sweepHeight, speedMult;

        public static Glacier30Params From(GameConfig c)
        {
            const string g = "design.n30.glacier30.";
            return new Glacier30Params
            {
                farMeters = c.Num(g + "farMeters"), hideSeconds = c.Num(g + "hideSeconds"), fillSeconds = c.Num(g + "fillSeconds"), drainMult = c.Num(g + "drainMult"),
                volleyTellSeconds = c.Num(g + "volleyTellSeconds"), volleyCount = c.Int(g + "volleyCount"), volleySpeed = c.Num(g + "volleySpeed"), turnDegPerSecond = c.Num(g + "turnDegPerSecond"),
                volleyLife = c.Num(g + "volleyLife"), volleyDamage = c.Num(g + "volleyDamage"), volleyRadius = c.Num(g + "volleyRadius"), volleySpreadDeg = c.Num(g + "volleySpreadDeg"),
                sweepEverySeconds = c.Num(g + "sweepEverySeconds"), sweepFirstSeconds = c.Num(g + "sweepFirstSeconds"), sweepTellSeconds = c.Num(g + "sweepTellSeconds"), sweepLiveSeconds = c.Num(g + "sweepLiveSeconds"),
                sweepWidth = c.Num(g + "sweepWidth"), sweepHalfLength = c.Num(g + "sweepHalfLength"), sweepDamage = c.Num(g + "sweepDamage"), sweepHeight = c.Num(g + "sweepHeight"), speedMult = c.Num(g + "speedMult"),
            };
        }
    }

    public sealed class Glacier30
    {
        public readonly Glacier30Params p;
        public readonly Glacier28 g;
        public readonly Glacier29 rule;
        readonly EventBus events;
        readonly BossTarget target;
        readonly double dt0, frostSlow;
        readonly int hideSteps, fillSteps, volleyTellSteps, sweepEverySteps, sweepFirstSteps, sweepTellSteps, sweepLiveSteps, shardLife;
        readonly Glacier30Event said = new Glacier30Event();
        /// <summary>The sim hands these in: the player's capsule, and is the line from a to b open (the world's walls and the island).</summary>
        public Func<Capsule> playerBody;
        public Func<Vec3, Vec3, bool> lineOpen;
        /// <summary>The player's mover (the sweep's frost bite); null: no slow.</summary>
        public Mover mover;

        double gauge;
        int hiddenSteps, volleyTell, sweepTimer, sweepTell, sweepLive;
        bool sweepHit;
        public readonly List<Glacier30Shard> shards = new List<Glacier30Shard>();

        /// <summary>0..1 the patience gauge (the gap bar's fill); the volley's tell; the sweep's band (lap s, half length, lateral from..to) while it tells or is live.</summary>
        public double GaugeFrac => gauge;
        public bool VolleyTelling => volleyTell > 0;
        public bool SweepTelling => sweepTell > 0;
        public bool SweepLive => sweepLive > 0;
        public double SweepS { get; private set; }
        public double SweepLat0 { get; private set; }
        public double SweepLat1 { get; private set; }
        public double SweepTellProgress => sweepTell > 0 ? 1.0 - (double)sweepTell / sweepTellSteps : 0;
        /// <summary>The player is far or out of sight (the reach rule's input) and the boss cannot see him now.</summary>
        public bool Far { get; private set; }
        public bool Hidden { get; private set; }
        /// <summary>Far or hidden with nothing aimed at him (no volley told or flying, no sweep told or live): the harness's out-of-reach measure.</summary>
        public bool OutOfReach => (Far || Hidden) && volleyTell == 0 && shards.Count == 0 && sweepTell == 0 && sweepLive == 0;

        // the harness's counters (this fight)
        public int volleys, shardHits, sweeps, sweepHits;

        public Glacier30(GameConfig cfg, Glacier28 inner, Glacier29 rule, EventBus events, BossTarget target)
        {
            p = Glacier30Params.From(cfg);
            g = inner;
            this.rule = rule;
            this.events = events;
            this.target = target;
            dt0 = cfg.Dt;
            frostSlow = cfg.Num("design.n29.boss2.frostSlow");
            hideSteps = S(p.hideSeconds);
            fillSteps = Math.Max(1, S(p.fillSeconds));
            volleyTellSteps = Math.Max(1, S(p.volleyTellSeconds));
            sweepEverySteps = S(p.sweepEverySeconds);
            sweepFirstSteps = S(p.sweepFirstSeconds);
            sweepTellSteps = Math.Max(1, S(p.sweepTellSeconds));
            sweepLiveSteps = Math.Max(1, S(p.sweepLiveSeconds));
            shardLife = S(p.volleyLife);
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public void Reset()
        {
            gauge = 0;
            hiddenSteps = volleyTell = sweepTell = sweepLive = 0;
            sweepTimer = sweepFirstSteps;
            sweepHit = false;
            shards.Clear();
            Far = Hidden = false;
            volleys = shardHits = sweeps = sweepHits = 0;
        }

        Capsule Body()
        {
            if (playerBody != null) return playerBody();
            Vec3 f = target.position();
            return new Capsule { start = new Vec3(f.x, f.y + 0.35, f.z), end = new Vec3(f.x, f.y + 1.45, f.z), radius = 0.35 };
        }

        static Vec3 Mid(Capsule c) => new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);

        Vec3 BossChest() => g.box.c;

        void Say(string ev, string stage, double sec)
        {
            said.stage = stage;
            said.seconds = sec;
            events.Emit(ev, said);
        }

        /// <summary>One fixed step, after Glacier28 and Glacier29 have stepped.</summary>
        public void Step(double dt)
        {
            if (!g.Alive || !g.IsPlaying)
            {
                volleyTell = sweepTell = sweepLive = 0;
                shards.Clear();
                gauge = 0;
                hiddenSteps = 0;
                Far = Hidden = false;
                return;
            }
            if (rule != null) g.speedMult29 = rule.p.speedMult * p.speedMult; // the bigger lap's pace (Glacier29 sets its own share at its reset)
            Capsule c = Body();
            Vec3 me = Mid(c), boss = BossChest();
            Far = Vec3.DistanceTo(me, boss) > p.farMeters;
            bool blind = lineOpen != null && !lineOpen(boss, me);
            hiddenSteps = blind ? hiddenSteps + 1 : 0;
            Hidden = blind && hiddenSteps >= hideSteps;
            StepGauge();
            StepShards(c, dt);
            StepSweep(me);
        }

        void StepGauge()
        {
            if (volleyTell > 0)
            {
                if (--volleyTell == 0) FireVolley();
                return;
            }
            if (Far || Hidden) gauge = Math.Min(1.0, gauge + 1.0 / fillSteps);
            else gauge = Math.Max(0.0, gauge - p.drainMult / fillSteps);
            if (gauge >= 1.0)
            {
                gauge = 0;
                volleyTell = volleyTellSteps;
                Say("glacierVolley", "tell", p.volleyTellSeconds);
            }
        }

        void FireVolley()
        {
            Vec3 boss = BossChest(), me = Mid(Body());
            double ax = me.x - boss.x, az = me.z - boss.z, l = JsMath.Hypot(ax, az);
            if (l < 1e-6) { ax = 1; az = 0; l = 1; }
            double baseA = JsMath.Atan2(az, ax), spread = p.volleySpreadDeg * Math.PI / 180;
            for (int k = 0; k < p.volleyCount; k++)
            {
                double f = p.volleyCount == 1 ? 0 : (double)k / (p.volleyCount - 1) * 2 - 1;
                double a = baseA + f * spread;
                var v = new Vec3(JsMath.Cos(a) * p.volleySpeed, 0, JsMath.Sin(a) * p.volleySpeed);
                shards.Add(new Glacier30Shard { pos = boss, prev = boss, vel = v, life = shardLife });
            }
            volleys++;
            Say("glacierVolley", "fire", p.volleyLife);
        }

        void StepShards(Capsule c, double dt)
        {
            Vec3 me = Mid(c);
            double maxTurn = p.turnDegPerSecond * Math.PI / 180 * dt;
            for (int i = shards.Count - 1; i >= 0; i--)
            {
                Glacier30Shard s = shards[i];
                // turn toward the player (horizontal) by at most maxTurn; height follows the player's chest
                double vh = JsMath.Hypot(s.vel.x, s.vel.z);
                double have = JsMath.Atan2(s.vel.z, s.vel.x), want = JsMath.Atan2(me.z - s.pos.z, me.x - s.pos.x), d = want - have;
                while (d > Math.PI) d -= 2 * Math.PI;
                while (d < -Math.PI) d += 2 * Math.PI;
                double a = have + Math.Max(-maxTurn, Math.Min(maxTurn, d));
                double dy = me.y - s.pos.y;
                s.vel = new Vec3(JsMath.Cos(a) * vh, Math.Max(-4, Math.Min(4, dy * 2)), JsMath.Sin(a) * vh);
                s.prev = s.pos;
                s.pos = s.pos + s.vel * dt;
                bool dead = --s.life <= 0 || (lineOpen != null && !lineOpen(s.prev, s.pos));
                if (!dead && SegDist(s.pos, c.start, c.end) < p.volleyRadius + c.radius)
                {
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.volleyDamage, "boss")) shardHits++;
                    dead = true;
                }
                if (dead) shards.RemoveAt(i);
            }
        }

        static double SegDist(Vec3 pt, Vec3 a, Vec3 b)
        {
            Vec3 ab = b - a, ap = pt - a;
            double len2 = Vec3.Dot(ab, ab), t = len2 < 1e-12 ? 0 : Math.Max(0, Math.Min(1, Vec3.Dot(ap, ab) / len2));
            return Vec3.DistanceTo(pt, a + ab * t);
        }

        void StepSweep(Vec3 me)
        {
            if (sweepTell > 0)
            {
                if (--sweepTell == 0) { sweepLive = sweepLiveSteps; sweepHit = false; Say("glacierSweep", "live", p.sweepLiveSeconds); }
                return;
            }
            if (sweepLive > 0)
            {
                g.rink.Project(me.x, me.z, out double ps, out double pl);
                bool inside = Math.Abs(g.rink.SignedGap(SweepS, ps)) <= p.sweepHalfLength && pl >= SweepLat0 && pl <= SweepLat1 && me.y - g.origin.y - g.FloorAt(pl) <= p.sweepHeight;
                if (inside && !sweepHit)
                {
                    // once per sweep: the damage and the frost's bite (the horizontal speed x (1 - frostSlow), Glacier29's number)
                    sweepHit = true;
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.sweepDamage, "boss")) sweepHits++;
                    if (mover != null) mover.velocity = new Vec3(mover.velocity.x * (1 - frostSlow), mover.velocity.y, mover.velocity.z * (1 - frostSlow));
                }
                if (--sweepLive == 0) { Say("glacierSweep", "end", 0); sweepTimer = sweepEverySteps; }
                return;
            }
            if (--sweepTimer > 0) return;
            // aim at the player: a band across his lateral position, either side of him along the lap
            g.rink.Project(me.x, me.z, out double s0, out double lat);
            SweepS = s0;
            SweepLat0 = lat - p.sweepWidth * 0.5;
            SweepLat1 = lat + p.sweepWidth * 0.5;
            sweepTell = sweepTellSteps;
            sweeps++;
            Say("glacierSweep", "tell", p.sweepTellSeconds);
        }
    }
}
