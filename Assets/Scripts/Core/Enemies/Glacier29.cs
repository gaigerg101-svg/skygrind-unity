// NIGHT29 builder B (boss2): THE GLACIER reworked (GG 2026-10-09: "for some reason I still can't damage it ... speed skating ... both of you trying not to be caught"). A wrapper on Glacier28
// (the lap chase on the banked rink, which stays the OFF path bit for bit): Glacier28.Filter let only a PULSE hit inside the window land, and never through the glaze, so a pistol-only player could not
// move the bar. With design.n29.boss2 on (this rule attached as Glacier28.rule29):
//   OTHER GUNS   any gun hit lands `otherGunFraction` of a gauntlet hit (a third of the pool) at any time, but cannot take the bar below `otherGunFloor` of the pool: a camper never finishes it.
//   THE CATCH-UP the player within `catchUpMetres` of the boss ALONG its flee direction and in the SAME skate lane opens an EXPOSED window of `exposedSeconds` (the plates crack): the gauntlet lands a full
//                hit (a third of the pool), a gun `exposedGunFraction` of one, and the floor does not apply. After the window the boss surges ahead (Glacier28's window bookkeeping).
//   THE LANES    the boss skates the lane lines (SkateLane29: `count` lines `spacing` apart on the rink's flat lane): every `laneEverySeconds` it slews to an adjacent line at `laneSlew` m/s (Mulberry32).
//                The player's own rule is SkateLane29 (applied after the Mover). The island and doors of the rink are unchanged.
//   ATTACKS      each with a 0.8 s tell and a decal (Glacier28's decals), none lethal under 0.5 s: P1 ICE SHARD VOLLEY (Glacier28's shard trail, every `volley.everySeconds`: a sideways dash dodges),
//                P2 + FROST TRAIL (a slow field laid ahead in the player's lane, speed x(1 - frostSlow) inside, a lane switch avoids it), P3 + LANE CHARGE (Glacier28's roller along the lane: a jump).
//   PATIENCE     V2's gauge: a gap above `patienceMetres` for `patienceSeconds` makes the volleys `patienceVolleyMult` x faster until the gap closes under `patienceCloseMetres`.
//   HUD DATA     Gap, Exposed / ExposedFrac, Enraged, PatienceFrac (0..1 toward the enrage), FrostZones, the player's lane (the view reads them).
// Fixed 120 Hz, timers in whole steps, Mulberry32 seeded per run, JsMath, no UnityEngine, no audio.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The reworked hit rule Glacier28 asks when a rule is attached (NIGHT29).</summary>
    public interface IGlacierRule29
    {
        double Filter(Glacier28 g, double amount, string weapon, Vec3 from, out bool blocked);
        bool Launch(Glacier28 g);
        void Apply(Glacier28 g, double dmg);
        bool WantWindow(Glacier28 g);
        double WindowSeconds { get; }
    }

    /// <summary>One slow field of the FROST TRAIL: an interval of the rink's arc and a lane band; live after its tell.</summary>
    public sealed class Glacier29Frost
    {
        public double s, hl, lat0, lat1, x, z, tx, tz;
        public int tellLeft, lifeLeft, tellTotal;
        public bool Live => tellLeft <= 0 && lifeLeft > 0;
    }

    public sealed class Glacier29Params
    {
        public double otherGunFraction, otherGunFloor, exposedSeconds, catchUpMetres, frostSlow, patienceSeconds, patienceMetres, patienceCloseMetres, patienceVolleyMult, exposedGunFraction;
        public double speedMult, attackTellMin;
        public double laneEvery, laneSlew, volleyEvery, frostEvery, frostTell, frostLife, frostLength, frostLead, frostMinGap, chargeEvery;

        public static Glacier29Params From(GameConfig c)
        {
            const string g = "design.n29.boss2.";
            return new Glacier29Params
            {
                otherGunFraction = c.Num(g + "otherGunFraction"), otherGunFloor = c.Num(g + "otherGunFloor"), exposedSeconds = c.Num(g + "exposedSeconds"), catchUpMetres = c.Num(g + "catchUpMetres"),
                frostSlow = c.Num(g + "frostSlow"), patienceSeconds = c.Num(g + "patienceSeconds"), patienceMetres = c.Num(g + "patienceMetres"), patienceCloseMetres = c.Num(g + "patienceCloseMetres"),
                patienceVolleyMult = c.Num(g + "patienceVolleyMult"), exposedGunFraction = c.Num(g + "exposedGunFraction"),
                speedMult = c.Num(g + "boss.speedMult"), attackTellMin = c.Num(g + "attackTellMinSeconds"), laneEvery = c.Num(g + "boss.laneEverySeconds"), laneSlew = c.Num(g + "boss.laneSlew"), volleyEvery = c.Num(g + "volley.everySeconds"),
                frostEvery = c.Num(g + "frost.everySeconds"), frostTell = c.Num(g + "frost.tellSeconds"), frostLife = c.Num(g + "frost.lifeSeconds"), frostLength = c.Num(g + "frost.length"),
                frostLead = c.Num(g + "frost.leadMetres"), frostMinGap = c.Num(g + "frost.minGap"), chargeEvery = c.Num(g + "charge.everySeconds"),
            };
        }
    }

    public sealed class Glacier29 : IGlacierRule29
    {
        public readonly Glacier29Params p;
        public readonly Glacier28 g;
        public readonly SkateLane29 lane;
        readonly EventBus events;
        readonly double dt0;
        Mulberry32 rng = new Mulberry32(1);
        readonly BossEvent said = new BossEvent();
        public readonly List<Glacier29Frost> frost = new List<Glacier29Frost>();

        double pendingFloor;
        int laneTimer, patienceSteps, frostFrameSteps;
        double volleyLeft, frostLeft, chargeLeft;
        int bossLane = 1;
        double frostEntrySpeed;
        bool inFrost;
        /// <summary>The volleys run faster (V2's patience gauge tripped).</summary>
        public bool Enraged { get; private set; }
        public int frostTells, frostSlowSteps, volleys, charges, exposedWindows, exposedHits, lightHits;

        public Glacier29(GameConfig cfg, Glacier28 inner, EventBus events)
        {
            p = Glacier29Params.From(cfg);
            g = inner;
            this.events = events;
            lane = SkateLane29.From(cfg);
            dt0 = cfg.Dt;
            bossLane = lane.count / 2;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public void NewRun(uint seed) => rng = new Mulberry32(seed ^ 0x6A09E667u);

        public void Reset()
        {
            frost.Clear();
            lane.Reset();
            patienceSteps = 0;
            Enraged = false;
            volleyLeft = S(p.volleyEvery);
            frostLeft = S(p.frostEvery);
            chargeLeft = S(p.chargeEvery);
            laneTimer = S(p.laneEvery);
            bossLane = lane.count / 2;
            g.bossLat29 = lane.LineLat(bossLane);
            g.speedMult29 = p.speedMult;
            g.tellMin29 = p.attackTellMin;
            inFrost = false;
            frostTells = frostSlowSteps = volleys = charges = exposedWindows = 0;
        }

        /// <summary>The boss is gone: nothing runs.</summary>
        public void Idle()
        {
            frost.Clear();
            Enraged = false;
            g.bossLat29 = 0;
            g.speedMult29 = 1;
            g.tellMin29 = 0;
        }

        // --- the HUD's data ------------------------------------------------------------------------------------------------------------

        /// <summary>The gap to the boss in metres along its flee direction (the HUD's gap bar).</summary>
        public double Gap => g.gap;
        public bool Exposed => g.WindowOpen;
        /// <summary>0..1 of the exposed window left.</summary>
        public double ExposedFrac => g.WindowOpen ? 1.0 : 0;
        /// <summary>0..1 toward the enrage (a gap above patienceMetres for patienceSeconds); 1 while enraged.</summary>
        public double PatienceFrac => Enraged ? 1.0 : Math.Min(1.0, (double)patienceSteps / Math.Max(1, S(p.patienceSeconds)));
        public int BossLane => bossLane;
        public int PlayerLane => lane.NearestLine(g.playerLat);

        // --- the hit rule --------------------------------------------------------------------------------------------------------------

        double Third(Glacier28 gl) => gl.body.maxHp / gl.p.hitsToKill;

        public double Filter(Glacier28 gl, double amount, string weapon, Vec3 from, out bool blocked)
        {
            blocked = false;
            if (!gl.IsPlaying) { blocked = true; return 0; }
            bool exposed = gl.WindowOpen;
            pendingFloor = exposed ? 0 : p.otherGunFloor;
            if (exposed) exposedHits++; else lightHits++;
            if (weapon == "gauntlet" && exposed) return Third(gl) * exposedScale30;
            double frac = exposed ? p.exposedGunFraction : p.otherGunFraction;
            return exposed ? Third(gl) * frac * exposedScale30 : Third(gl) * frac;
        }

        /// <summary>NIGHT30 (design.n30.glacier30): the exposed window's damage share (1: NIGHT29) and its length in seconds (0 or less: exposedSeconds, NIGHT29).</summary>
        public double exposedScale30 = 1, windowSeconds30;

        public void Apply(Glacier28 gl, double dmg)
        {
            gl.Hit29(dmg, pendingFloor, false);
        }

        /// <summary>The PULSE blast landed on the boss (in reach, in the cone): full in the exposed window, the light share anywhere else.</summary>
        public bool Launch(Glacier28 gl)
        {
            if (gl.WindowOpen) gl.Hit29(Third(gl) * exposedScale30, 0, true);
            else gl.Hit29(Third(gl) * p.otherGunFraction, p.otherGunFloor, true);
            return true;
        }

        public bool WantWindow(Glacier28 gl)
        {
            if (!gl.IsPlaying) return false;
            if (gl.gap > p.catchUpMetres || gl.gap < -1.5) return false;
            bool same = Math.Abs(gl.playerLat - gl.bossLat29) <= lane.spacing * 0.75; // side by side: within three quarters of a lane of the boss (the boss slews between lines, so no index flicker)
            if (same) exposedWindows++;
            return same;
        }

        public double WindowSeconds => windowSeconds30 > 0 ? windowSeconds30 : p.exposedSeconds;

        /// <summary>NIGHT30 (design.n30.glacier30): SkateLane29's snap is off for the player (the glide floor replaces it); false: NIGHT29, bit for bit.</summary>
        public bool noLane30;

        // --- the step (after the Mover and the boss's own step) -----------------------------------------------------------------------

        public void Step(double dt, Mover mover, bool onRink)
        {
            if (!g.Alive) { Idle(); return; }
            bool play = g.IsPlaying;
            // the skate lane rule on the player (after the Mover)
            lane.Step(mover, g.rink, g.p.laneHalf, onRink && play && !noLane30, dt); // NIGHT30 (glacier30): no snap lanes on the rebuilt ice (noLane30 false: NIGHT29)
            if (!play) { frost.Clear(); inFrost = false; return; }
            StepBossLane(dt);
            StepPatience();
            StepAttacks();
            StepFrost(mover);
        }

        void StepBossLane(double dt)
        {
            if (--laneTimer <= 0)
            {
                laneTimer = S(p.laneEvery);
                int to = bossLane + (rng.Next() < 0.5 ? -1 : 1);
                if (to < 0 || to >= lane.count) to = bossLane + (to < 0 ? 1 : -1);
                if (to >= 0 && to < lane.count) bossLane = to;
            }
            double want = lane.LineLat(bossLane), stp = p.laneSlew * dt;
            g.bossLat29 += Math.Max(-stp, Math.Min(stp, want - g.bossLat29));
        }

        void StepPatience()
        {
            double gap = g.gap;
            if (Enraged)
            {
                if (gap < p.patienceCloseMetres) { Enraged = false; patienceSteps = 0; }
                return;
            }
            if (gap > p.patienceMetres) { if (++patienceSteps >= S(p.patienceSeconds)) Enraged = true; }
            else patienceSteps = 0;
        }

        bool AnyFrostTell()
        {
            foreach (Glacier29Frost f in frost) if (f.tellLeft > 0) return true;
            return false;
        }

        void StepAttacks()
        {
            string ph = g.phase;
            bool busy = g.AnyTelling29 || AnyFrostTell();
            double rate = Enraged ? p.patienceVolleyMult : 1.0;
            // P1 on: the ice shard volley (a sideways dash); patience speeds the volleys
            if (volleyLeft > 0) volleyLeft -= rate;
            if (volleyLeft <= 0 && !busy)
            {
                if (g.SpawnShards29()) { volleyLeft = S(p.volleyEvery); volleys++; }
                else volleyLeft = S(0.5);
                busy = true;
            }
            // P2 on: the frost trail (a lane switch)
            if (ph == "p1" || ph == "p2" || ph == "p3") // NIGHT29 round 4: the frost trail fires from P1 (a fast fight never reached P2 in the Player)
            {
                if (frostLeft > 0) frostLeft--;
                if (frostLeft <= 0 && !busy)
                {
                    if (SpawnFrost()) frostLeft = S(p.frostEvery);
                    else frostLeft = S(0.5);
                    busy = true;
                }
            }
            // P3: the lane charge (a jump)
            if (ph == "p2" || ph == "p3") // NIGHT29 round 4: the lane charge from P2 (it was P3 only)
            {
                if (chargeLeft > 0) chargeLeft--;
                if (chargeLeft <= 0 && !busy)
                {
                    if (g.SpawnRoller29()) { chargeLeft = S(p.chargeEvery); charges++; }
                    else chargeLeft = S(0.5);
                }
            }
        }

        /// <summary>A slow field laid ahead of the player along the line to the boss, in the player's lane: the decal shows for frostTell s, then it stands for frostLife s.</summary>
        bool SpawnFrost()
        {
            if (g.gap < p.frostMinGap) return false;
            Vec3 v = g.playerVelocity != null ? g.playerVelocity() : default;
            double speed = JsMath.Hypot(v.x, v.z);
            double d = p.frostLead + speed * p.frostTell + p.frostLength * 0.5;
            if (d > g.gap - 2) return false;
            int pl = lane.NearestLine(g.playerLat);
            double lc = lane.LineLat(pl), hw = lane.spacing * 0.5;
            var f = new Glacier29Frost { s = g.rink.Wrap(g.playerS + g.dir * d), hl = p.frostLength * 0.5, lat0 = lc - hw, lat1 = lc + hw, tellLeft = S(p.frostTell), tellTotal = S(p.frostTell), lifeLeft = S(p.frostLife) };
            g.rink.PointAt(f.s, lc, out f.x, out f.z, out f.tx, out f.tz);
            frost.Add(f);
            frostTells++;
            said.Reset("glacier");
            said.name = "frost";
            said.seconds = p.frostTell;
            events.Emit("bossTell", said);
            return true;
        }

        void StepFrost(Mover mover)
        {
            bool now = false;
            for (int i = frost.Count - 1; i >= 0; i--)
            {
                Glacier29Frost f = frost[i];
                if (f.tellLeft > 0) { f.tellLeft--; continue; }
                if (--f.lifeLeft <= 0) { frost.RemoveAt(i); continue; }
                if (!now && mover.grounded && Math.Abs(g.rink.SignedGap(f.s, g.playerS)) < f.hl && g.playerLat > f.lat0 && g.playerLat < f.lat1) now = true;
            }
            if (now)
            {
                Vec3 v = mover.velocity;
                double speed = JsMath.Hypot(v.x, v.z);
                if (!inFrost) { inFrost = true; frostEntrySpeed = speed; }
                double cap = frostEntrySpeed * (1.0 - p.frostSlow);
                if (speed > cap && speed > 1e-6)
                {
                    double k = cap / speed;
                    mover.velocity = new Vec3(v.x * k, v.y, v.z * k);
                    frostSlowSteps++;
                }
            }
            else inFrost = false;
        }
    }
}
