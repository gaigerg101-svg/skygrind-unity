// NIGHT31 builder B (design.n31.wyrm31; DESIGN31 C3 + the reviewer's R5): THE KILN WYRM, rebuilt so it cannot be camped. Keeps NIGHT30's head-punch stagger (design.n30.wyrm30). Adds:
//   FEINT        one aggro charge in feintEvery stops feintShort m short (Wyrm.feintNext31; its pattern "feint31" is not "aggro": the head does not glow cyan, no punch window), then a
//                TAIL SWEEP where it stopped: a sweepTellSeconds tell (event wyrm31 "sweepTell"), then anyone grounded within sweepRadius is hit (sweepDamage, a knock). A jump dodges it.
//   GUNS         every hit lands gunScale (Wyrm.filter31) except the PULSE gauntlet's punch; while STAGGERED (staggerSeconds after a "wyrmStagger") the THUNDERCLAP lands in full,
//                its slug (a weapon id naming "slug") slugMult x; other guns stay at gunScale (R5).
//   SPIT         a player who has not moved for stillSeconds, at any range (R5), meets a led spit volley every spitEverySeconds from the head.
//   (cut: the k-room platforms crumbling at half health; see the report.)
// Fixed 120 Hz, whole steps, no randomness, JsMath, no audio, no UnityEngine.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Wyrm31Params
    {
        public double gunScale, staggerSeconds, slugMult, feintShort, sweepTellSeconds, sweepRadius, sweepDamage, sweepKnock, sweepUp, stillSpeed, stillSeconds;
        public double spitEverySeconds, spitSpread, spitSpeed, spitDamage, spitRadius, spitLife;
        public int feintEvery, spitCount;
        public double aggroCooldown, crumbleAt, crumbleTellSeconds;
        public double[] crumbleBoxes;

        public static Wyrm31Params From(GameConfig c)
        {
            const string g = "design.n31.wyrm31.";
            return new Wyrm31Params
            {
                gunScale = c.Num(g + "gunScale"), staggerSeconds = c.Num(g + "staggerSeconds"), slugMult = c.Num(g + "slugMult"), feintEvery = (int)c.Num(g + "feintEvery"), feintShort = c.Num(g + "feintShort"),
                sweepTellSeconds = c.Num(g + "sweepTellSeconds"), sweepRadius = c.Num(g + "sweepRadius"), sweepDamage = c.Num(g + "sweepDamage"), sweepKnock = c.Num(g + "sweepKnock"), sweepUp = c.Num(g + "sweepUp"),
                stillSpeed = c.Num(g + "stillSpeed"), stillSeconds = c.Num(g + "stillSeconds"), spitEverySeconds = c.Num(g + "spitEverySeconds"), spitCount = (int)c.Num(g + "spitCount"),
                aggroCooldown = c.Num(g + "aggroCooldown"), crumbleAt = c.Num(g + "crumbleAt"), crumbleTellSeconds = c.Num(g + "crumbleTellSeconds"), crumbleBoxes = c.Nums(g + "crumbleBoxes"),
                spitSpread = c.Num(g + "spitSpread"), spitSpeed = c.Num(g + "spitSpeed"), spitDamage = c.Num(g + "spitDamage"), spitRadius = c.Num(g + "spitRadius"), spitLife = c.Num(g + "spitLife"),
            };
        }

        Wyrm31Params orig32;

        /// <summary>NIGHT32 B (design.n32.bosses32.wyrm): the row's overrides on (the gun chip, a longer stagger, charges more often), or NIGHT31's values back.</summary>
        public void Set32(GameConfig c, bool on)
        {
            orig32 ??= (Wyrm31Params)MemberwiseClone();
            gunScale = on ? Boss32Cfg.Num(c, "wyrm", "gunScale", orig32.gunScale) : orig32.gunScale;
            staggerSeconds = on ? Boss32Cfg.Num(c, "wyrm", "staggerSeconds", orig32.staggerSeconds) : orig32.staggerSeconds;
            aggroCooldown = on ? Boss32Cfg.Num(c, "wyrm", "aggroCooldown", orig32.aggroCooldown) : orig32.aggroCooldown;
            // the anti-camp (the lead 20:13): the spit meets a still player sooner, more often and harder
            stillSeconds = on ? Boss32Cfg.Num(c, "wyrm", "stillSeconds", orig32.stillSeconds) : orig32.stillSeconds;
            spitEverySeconds = on ? Boss32Cfg.Num(c, "wyrm", "spitEverySeconds", orig32.spitEverySeconds) : orig32.spitEverySeconds;
            spitDamage = on ? Boss32Cfg.Num(c, "wyrm", "spitDamage", orig32.spitDamage) : orig32.spitDamage;
        }
    }

    /// <summary>The payload of "wyrm31": stage feint / sweepTell / sweep / spit; where.</summary>
    public sealed class Wyrm31Event
    {
        public string stage;
        public Vec3 at;
        public double seconds;
    }

    public sealed class Wyrm31
    {
        public readonly Wyrm31Params p;
        readonly EventBus events;
        readonly BossTarget target;
        readonly double dt0;
        readonly Wyrm31Event ev = new Wyrm31Event();
        public Func<Vec3> playerVelocity;
        public Func<bool> playerGrounded;
        /// <summary>The sim hands this in: the head is punchable now (SurvivalSim.Wyrm30Punchable: a real charge inside the window).</summary>
        public Func<bool> punchable;
        int aggroCount, staggerLeft, sweepTell, spitTimer;
        double stillTime;
        string lastPat;
        public Vec3 sweepAt;
        public readonly List<Stag31Shard> spits = new List<Stag31Shard>();
        // the harness's counters (this fight)
        public int feints, sweeps, sweepHits, spitVolleys, spitHits, gunHits, fullHits;

        /// <summary>NIGHT32 B (design.n32.bosses32.wyrm; GG 2026-10-10 "you can barely hit them"): outside a stagger every hit lands p.gunScale (the row's: at least 25 % on the
        /// body after its x0.5 part share); while staggered EVERY weapon lands open32 x the part, and the THUNDERCLAP's slug (it fires as weapon "shotgun", ChargeShots27.FireSlug31:
        /// one hit of the whole volley, so a shotgun hit of at least slugMin32 after the part share is the slug) lands slugMult more (NIGHT31's slug x2 never applied: A1).
        /// slugHits32: the slug hits seen (the harness's; never reset).</summary>
        public bool on32;
        public double open32 = 1.25, slugMin32 = 200;
        public int slugHits32;
        /// <summary>NIGHT32 B: one staggered hit takes at most maxHit32 of the pool (the sim sets maxHp32 = the worm's max each step): the slug x2 x the head's x2 x open was
        /// 3120 on a 4700 pool, so one stagger decided the fight.</summary>
        public double maxHit32, maxHp32;

        /// <summary>NIGHT32 B: the row on (its numbers) or off (NIGHT31's).</summary>
        public void Set32(GameConfig c, bool on)
        {
            p.Set32(c, on);
            on32 = on;
            open32 = Boss32Cfg.Num(c, "wyrm", "open", 1.25);
            slugMin32 = Boss32Cfg.Num(c, "wyrm", "slugMinDamage", 200);
            maxHit32 = on ? Boss32Cfg.Num(c, "wyrm", "maxHitFrac", 0) : 0;
        }

        double Filter32(double dmg, string weapon)
        {
            if (weapon == Stag31.ParryWeapon) { fullHits++; return dmg * open32; }
            if (staggerLeft > 0)
            {
                fullHits++;
                double m = open32;
                if (weapon != null && weapon.StartsWith("shotgun", StringComparison.Ordinal) && dmg >= slugMin32) { slugHits32++; m *= p.slugMult; }
                return maxHp32 > 0 && maxHit32 > 0 ? Math.Min(dmg * m, maxHit32 * maxHp32) : dmg * m;
            }
            gunHits++;
            return dmg * p.gunScale;
        }

        public Wyrm31(GameConfig cfg, Wyrm31Params p, EventBus events, BossTarget target)
        {
            this.p = p;
            this.events = events;
            this.target = target;
            dt0 = cfg.Dt;
        }

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Staggered => staggerLeft > 0;
        public bool SweepTelling => sweepTell > 0;
        public double SweepTellProgress => sweepTell > 0 ? 1.0 - (double)sweepTell / Math.Max(1, S(p.sweepTellSeconds)) : 0;

        public void Reset()
        {
            aggroCount = staggerLeft = sweepTell = 0;
            spitTimer = S(p.spitEverySeconds);
            stillTime = 0;
            lastPat = null;
            spits.Clear();
            feints = sweeps = sweepHits = spitVolleys = spitHits = gunHits = fullHits = 0;
        }

        /// <summary>Wyrm.feintNext31: every feintEvery-th aggro charge is a feint.</summary>
        public bool FeintNext()
        {
            aggroCount++;
            bool f = p.feintEvery > 0 && aggroCount % p.feintEvery == 0;
            if (f) feints++;
            return f;
        }

        /// <summary>A head punch staggered the worm (the sim's "wyrmStagger").</summary>
        public void OnStagger() => staggerLeft = S(p.staggerSeconds);
        /// <summary>NIGHT47 B wyrm47 (SurvivalSim.N47.Wyrm.cs; nothing calls it with the row off): the STAGGER ends now (its share of the bar spent).</summary>
        public void EndStagger47() => staggerLeft = 0;

        /// <summary>Wyrm.filter31: the weapon rule over the part's multiplier.</summary>
        public double Filter(double dmg, string weapon, string part)
        {
            if (on32) return Filter32(dmg, weapon); // NIGHT32 B (design.n32.bosses32)
            if (weapon == Stag31.ParryWeapon) { fullHits++; return dmg; } // the lead (12:18): a parried projectile always lands in full
            if (weapon == "gauntlet" && staggerLeft > 0) { fullHits++; return dmg; } // punches while staggered; the punch that staggers lands its bonus through the stagger (SurvivalSim.Wyrm30OnHit, design.n30.wyrm30.bonusMult), so a head punch that does not stagger is a 5 % chip
            if (staggerLeft > 0 && weapon != null && weapon.StartsWith("shotgun", StringComparison.Ordinal))
            {
                fullHits++;
                return weapon.IndexOf("slug", StringComparison.Ordinal) >= 0 ? dmg * p.slugMult : dmg;
            }
            gunHits++;
            return dmg * p.gunScale;
        }

        public void Step(Wyrm w, double dt)
        {
            if (staggerLeft > 0) staggerLeft--;
            Vec3 me = target.position();
            Vec3 v = playerVelocity != null ? playerVelocity() : default;
            stillTime = JsMath.Hypot(v.x, v.z) < p.stillSpeed ? stillTime + dt : 0;
            bool fighting = w.Alive && (w.phase == "p1" || w.phase == "p2" || w.phase == "p3" || w.phase == "p4");
            // the tail sweep after a feint ends
            string pat = w.PatternName;
            if (lastPat == "feint31" && pat != "feint31" && fighting)
            {
                sweepAt = w.boxes[0].c;
                sweepTell = Math.Max(1, S(p.sweepTellSeconds));
                sweeps++;
                Say("sweepTell", sweepAt, p.sweepTellSeconds);
            }
            if (pat == "feint31" && lastPat != "feint31") Say("feint", w.boxes[0].c, 0);
            lastPat = pat;
            if (sweepTell > 0 && --sweepTell == 0)
            {
                Say("sweep", sweepAt, 0);
                double dx = me.x - sweepAt.x, dz = me.z - sweepAt.z, d = JsMath.Hypot(dx, dz);
                bool grounded = playerGrounded == null || playerGrounded();
                if (d <= p.sweepRadius && grounded)
                {
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.sweepDamage, "boss")) sweepHits++;
                    if (d < 1e-6) { dx = 1; d = 1; }
                    target.impulse?.Invoke(dx / d * p.sweepKnock, p.sweepUp, dz / d * p.sweepKnock);
                }
            }
            // the spit at a still player (any range)
            if (spitTimer > 0) spitTimer--;
            if (fighting && spitTimer == 0 && stillTime >= p.stillSeconds)
            {
                Volley(w.boxes[0].c, me, v);
                spitTimer = S(p.spitEverySeconds);
            }
            StepSpits(dt, me);
        }

        void Volley(Vec3 from, Vec3 me, Vec3 v)
        {
            spitVolleys++;
            Vec3 chest = new Vec3(me.x, me.y + 1.0, me.z);
            double tFly = Vec3.DistanceTo(from, chest) / Math.Max(1e-6, p.spitSpeed);
            Vec3 led = new Vec3(chest.x + v.x * tFly, chest.y, chest.z + v.z * tFly);
            Vec3 aim = Vec3.Normalize(led - from);
            double sl = JsMath.Hypot(aim.x, aim.z);
            Vec3 side = sl < 1e-9 ? new Vec3(1, 0, 0) : new Vec3(-aim.z / sl, 0, aim.x / sl);
            for (int i = 0; i < p.spitCount; i++)
            {
                double off = i == 0 ? 0 : (i % 2 == 1 ? 1 : -1) * p.spitSpread * ((i + 1) / 2);
                Vec3 d = Vec3.Normalize(led + side * off - from);
                spits.Add(new Stag31Shard { pos = from, vel = d * p.spitSpeed, life = S(p.spitLife), alive = true });
            }
            Say("spit", from, 0);
        }

        void StepSpits(double dt, Vec3 me)
        {
            Vec3 chest = new Vec3(me.x, me.y + 1.0, me.z);
            for (int i = spits.Count - 1; i >= 0; i--)
            {
                Stag31Shard s = spits[i];
                s.pos = s.pos + s.vel * dt;
                if (--s.life <= 0) { spits.RemoveAt(i); continue; }
                if (Vec3.DistanceTo(s.pos, chest) <= p.spitRadius)
                {
                    Health h = target.health;
                    if (h != null && !h.Dead && h.Damage(p.spitDamage, "boss")) spitHits++;
                    spits.RemoveAt(i);
                }
            }
        }

        void Say(string stage, Vec3 at, double sec)
        {
            ev.stage = stage;
            ev.at = at;
            ev.seconds = sec;
            events.Emit("wyrm31", ev);
        }
    }
}
