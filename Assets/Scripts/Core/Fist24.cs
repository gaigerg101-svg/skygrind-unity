// NIGHT24 (builder F, contract charge-fist; design.n24.chargeFist): the hit-fed fist and its flick. Unity-only (the web had neither).
// GG: "charge it up by hitting enemies over time ... flick air kind of movement boost ... only how many enemies you hit or kill".
// - The charge (0..1) rises from fist hits (design hit), fist kills (fistKill) and kills by any other weapon (otherKill); it NEVER
//   rises with time and the sim never ticks it. A hit on you takes hurtLoss of it.
// - Three tiers at tiers[] (33 / 66 / 100 %). A punch (the PULSE GAUNTLET's meleeSwing, or the punch key's punch) with a charge of
//   tier 1 or more FLICKS the player along the aim by jumpMult[tier-1] x J metres (J = jumpDistance, the horizontal reach of a jump
//   measured on the Mover by the harness `fist`), spends ALL the charge, and starts a lockout (the CHARGE is the long cooldown,
//   DECIDE24 D-13; the lockout only stops a double punch). One flick in the air until the player is grounded or touches a wall.
// - The flick is an IMPULSE through the existing Mover.Impulse (Mover.cs is untouched). Before it, a ray along the flick on the
//   collider (ICollisionWorld) caps the distance: aim into a wall and the flick shortens to the room there is, or does not happen
//   (and spends nothing) when there is less than minRoom metres: no launch through geometry.
// Events (new names): fistCharge {charge, tier, gain, why}, fistFlick {tier, spent, distance, speed, air}, fistFlickBlocked {room}.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n24.chargeFist's numbers.</summary>
    public sealed class Fist24Rules
    {
        public double hit, fistKill, otherKill, hurtLoss, lockout;
        public double jumpDistance, wallMargin, minRoom, bodyHeight;
        public readonly double[] tiers = new double[3];
        public readonly double[] jumpMult = new double[3];
        /// <summary>The impulse in m/s for each notch (design speed): tuned so the measured reach meets jumpMult x J (the ground scrubs a push).</summary>
        public readonly double[] speed = new double[3];

        public static Fist24Rules From(GameConfig cfg)
        {
            const string p = "design.n24.chargeFist.";
            var r = new Fist24Rules
            {
                hit = cfg.Num(p + "gain.hit"),
                fistKill = cfg.Num(p + "gain.fistKill"),
                otherKill = cfg.Num(p + "gain.otherKill"),
                hurtLoss = cfg.Num(p + "hurtLoss"),
                lockout = cfg.Num(p + "lockout"),
                jumpDistance = cfg.Num(p + "jumpDistance"),
                wallMargin = cfg.Num(p + "wallMargin"),
                minRoom = cfg.Num(p + "minRoom"),
                bodyHeight = cfg.Num(p + "bodyHeight"),
            };
            double[] t = cfg.Nums(p + "tiers"), j = cfg.Nums(p + "jumpMult"), v = cfg.Nums(p + "speed");
            for (int i = 0; i < 3; i++)
            {
                r.tiers[i] = t[i];
                r.jumpMult[i] = j[i];
                r.speed[i] = v[i];
            }
            return r;
        }
    }

    public sealed class Fist24ChargeEvent
    {
        public double charge, gain;
        public int tier;
        public string why; // hit, fistKill, otherKill, hurt
    }

    public sealed class Fist24FlickEvent
    {
        public int tier;
        public double spent, distance, speed;
        public bool air;
    }

    public sealed class Fist24
    {
        public readonly Fist24Rules rules;
        readonly EventBus events;
        readonly Mover mover;
        readonly ICollisionWorld world;
        readonly Func<Vec3> aimDir;
        readonly double dt;
        readonly Fist24ChargeEvent chargeEvent = new Fist24ChargeEvent();
        readonly Fist24FlickEvent flickEvent = new Fist24FlickEvent();
        bool active, airFlicked;

        /// <summary>The harness bot banks charge: punches do not flick (never set in the game).</summary>
        public bool noFlick;
        int lockSteps;

        /// <summary>NIGHT25 pulseCharge: the PULSE GAUNTLET owns the charge. Only its own hits feed it (pulseHit each: no kill rate, no other gun's
        /// kill), the punch key and the swing no longer flick (Loadout.StepPulse25 flicks on the release of the left click), time never feeds it.
        /// Set by the sim's hook; off: NIGHT24's rules unchanged.</summary>
        public bool pulseMode;
        public double pulseHit;

        /// <summary>Set while the launch deals its damage, so that damage is not a hit that feeds the charge.</summary>
        public bool feedLocked;

        /// <summary>Pulse mode on / off for this run (after Reset).</summary>
        public void SetPulse(bool on, double hitGain)
        {
            pulseMode = on;
            pulseHit = hitGain;
            feedLocked = false;
        }

        /// <summary>The charge, 0..1.</summary>
        public double charge;

        /// <summary>The run's counters (the harness and the reel read them).</summary>
        public int flicks, blocked, hitsFed, fistKillsFed, otherKillsFed;

        /// <summary>The distance the last flick was asked for / allowed after the collider's cap (the harness).</summary>
        public double lastWanted, lastAllowed;

        public Fist24(GameConfig cfg, EventBus events, Mover mover, ICollisionWorld world, Func<Vec3> aimDir)
        {
            rules = Fist24Rules.From(cfg);
            this.events = events;
            this.mover = mover;
            this.world = world;
            this.aimDir = aimDir;
            dt = 1 / cfg.movement.loop.simHz;
            events.On<HitEvent>("hit", OnHit);
            events.On<KillEvent>("kill", OnKill);
            events.On<HurtEvent>("hurt", OnHurt);
            events.On<MeleeEvent>("meleeSwing", OnPunch);
            events.On<MeleeEvent>("punch", OnPunch);
        }

        /// <summary>A run's start: no charge, no lockout (the block off wires nothing: the plain punch).</summary>
        public void Reset(bool on)
        {
            active = on;
            charge = 0;
            lockSteps = 0;
            airFlicked = false;
            flicks = blocked = hitsFed = fistKillsFed = otherKillsFed = 0;
            lastWanted = lastAllowed = 0;
        }

        public bool Active => active;

        /// <summary>The run snapshot (NIGHT24 audit f): the charge, the lockout, the air flick used.</summary>
        public void SaveState(StateWriter w)
        {
            w.D("charge", charge);
            w.I("lockSteps", lockSteps);
            w.B("airFlicked", airFlicked);
        }

        public void LoadState(StateReader r)
        {
            charge = r.D("charge");
            lockSteps = r.Int("lockSteps");
            airFlicked = r.B("airFlicked");
        }

        /// <summary>0 below the first notch, else 1..3.</summary>
        public int Tier => TierOf(charge);

        public int TierOf(double c)
        {
            if (c >= rules.tiers[2] - 1e-9) return 3;
            if (c >= rules.tiers[1]) return 2;
            return c >= rules.tiers[0] ? 1 : 0;
        }

        /// <summary>Steps of the lockout left.</summary>
        public int LockSteps => lockSteps;

        /// <summary>Is a flick possible right now (a tier, no lockout, the air flick not spent)?</summary>
        public bool CanFlick => active && Tier > 0 && lockSteps <= 0 && (mover.grounded || !airFlicked);

        /// <summary>One fixed step. NOTHING here adds charge: the lockout runs, and the air flick is given back on the ground or a wall.</summary>
        public void Step()
        {
            if (!active) return;
            if (lockSteps > 0) lockSteps--;
            if (mover.grounded || mover.touchingWall) airFlicked = false;
        }

        void Add(double amount, string why)
        {
            if (!active || amount <= 0) return;
            double before = charge;
            charge = Math.Min(1, charge + amount);
            Emit(charge - before, why);
        }

        /// <summary>A reward fills the charge (the BIG PICK's FULL FIST CHARGE).</summary>
        public void Fill()
        {
            if (!active) return;
            double before = charge;
            charge = 1;
            Emit(charge - before, "reward");
        }

        void Emit(double gain, string why)
        {
            chargeEvent.charge = charge;
            chargeEvent.gain = gain;
            chargeEvent.tier = Tier;
            chargeEvent.why = why;
            events.Emit("fistCharge", chargeEvent);
        }

        void OnHit(HitEvent e)
        {
            if (pulseMode)
            {
                // NIGHT25: the PULSE's own hits, a killing blow included (a hit is a hit; a kill pays nothing more)
                if (!active || feedLocked || e.owner != "player" || !e.melee || e.weapon != "gauntlet") return;
                hitsFed++;
                Add(pulseHit, "hit");
                return;
            }
            if (!active || e.owner != "player" || !e.melee || e.killed) return; // a killing blow pays the kill's rate, not both
            hitsFed++;
            Add(rules.hit, "hit");
        }

        void OnKill(KillEvent e)
        {
            if (!active || pulseMode || e.owner != "player") return; // NIGHT25: no kill feeds the pulse
            if (e.melee)
            {
                fistKillsFed++;
                Add(rules.fistKill, "fistKill");
            }
            else
            {
                otherKillsFed++;
                Add(rules.otherKill, "otherKill");
            }
        }

        void OnHurt(HurtEvent e)
        {
            if (!active || e.owner != "player" || e.amount <= 0 || charge <= 0) return;
            double before = charge;
            charge = Math.Max(0, charge * (1 - rules.hurtLoss));
            Emit(charge - before, "hurt");
        }

        /// <summary>A punch with a charge flicks the player along the aim. Runs inside the loadout's step (the swing's event).</summary>
        void OnPunch(MeleeEvent e)
        {
            if (!active || noFlick || pulseMode || e.owner != "player" || !CanFlick) return; // NIGHT25: the pulse flicks on the release
            Flick();
        }

        /// <summary>The flick: the tier's reach (jumpMult x J) along the aim, capped on the collider, spent in full, as an impulse.</summary>
        public bool Flick()
        {
            int tier = Tier;
            Vec3 dir = aimDir();
            bool air = !mover.grounded;
            if (!air && dir.y < 0) dir = new Vec3(dir.x, 0, dir.z); // on the ground the floor eats a downward push: flick along it
            double len = dir.Length;
            if (len < 1e-6) return false;
            dir = dir / len;
            double wanted = rules.jumpMult[tier - 1] * rules.jumpDistance;
            double allowed = wanted;
            // The collider's cap: a ray from the body's middle along the flick; the player's radius and a margin stay clear of the surface.
            Vec3 from = new Vec3(mover.position.x, mover.position.y + rules.bodyHeight, mover.position.z);
            if (world.RayIntersect(from, dir, out RayHit hit))
            {
                double room = hit.distance - mover.Config.body.capsuleRadius - rules.wallMargin;
                if (room < allowed) allowed = Math.Max(0, room);
            }
            lastWanted = wanted;
            lastAllowed = allowed;
            if (allowed < rules.minRoom)
            {
                blocked++;
                events.Emit("fistFlickBlocked", allowed);
                return false;
            }
            double speed = rules.speed[tier - 1] * (wanted > 0 ? allowed / wanted : 0); // a flick cut by a wall is slower in proportion
            mover.Impulse(dir.x * speed, dir.y * speed, dir.z * speed);
            flickEvent.tier = tier;
            flickEvent.spent = charge;
            flickEvent.distance = allowed;
            flickEvent.speed = speed;
            flickEvent.air = air;
            charge = 0;
            lockSteps = MovementMath.StepsFor(rules.lockout, dt);
            if (air) airFlicked = true;
            flicks++;
            events.Emit("fistFlick", flickEvent);
            Emit(-flickEvent.spent, "flick");
            return true;
        }
    }
}
