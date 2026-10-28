// NIGHT43 builder W1 (the weapons): the sim side of gauntlet43, parry43 and fusion43 (N43Blocks; design.n43.*), through the lead's partial hooks
// (SurvivalSim.N43.cs). N43StartRun runs right after N42StartRun (SurvivalSim.Run.cs), so a NIGHT43 row here overrides what NIGHT42's
// N42WeaponsStartRun handed the same objects; each object keeps its own per-run state. Off (a row off, a tape without the bit, --off43 and
// older): nothing below changes a field NIGHT42 set, and NIGHT42's game runs bit for bit.
// - gauntlet43 (G17, GG: "Experiment with an attack interval around 1.25 to 1.3 seconds"; DECIDE43 D-6): the PULSE swing's cycle
//   design.n43.gauntlet43.swingSeconds (1.25 s) in PulseCharge27.swingSeconds42 (gauntlet42's 1.8 s and the n27 row's 2.5 s untouched).
// - parry43 (G18, GG: "the player currently has to wait until an incoming projectile is extremely close"): the DRONE orb is caught by
//   parry42's body volume, a catch sphere ahead of the eye, or a time-to-impact window, with parry42's mercy and an orb buffer of 0.15 s that
//   the punch's and the gauntlet swing's misses open (Core/Parry43.cs, Core/Parry42.cs; design.n43.parry43 numbers, parry42's row untouched).
// - fusion43 (G19, GG: "If the player performs a high-speed dash impact or gauntlet attack, create a powerful special feedback response"):
//   a gauntlet / punch hit at speed, in or just after a dash, or just after a wall jump: more damage, a knock, a stagger, a hitstop request
//   ("fusion43", ImpactRules' Fusion43 tier) and N43Events.Fusion once per hit (Core/Fusion43.cs; F1's Fusion43View draws it).
// - slug43 (G20, GG: "Charged attacks, especially charged shotgun attacks, should produce more dramatic ... explosions, enemy reactions"):
//   the charged slug throws and stuns what it strikes and blasts what is near its end; `slug43` once an impact (Core/Slug43.cs).
// - torch43 (G20, GG: "The flamethrower currently feels weak"): the TORCH ignites (a burn over seconds, `burn43`, Loadout.Burning43) and its
//   reach ramps in over the first 0.2 s of a spray (Core/Torch43.cs).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N43WeaponsStartRun()
        {
            // G17 gauntlet43: the swing's cycle (PulseCharge27.SwingSteps reads it: Combat's cooldown, the HUD's punch look, the brawl pilot)
            if (pulse27 != null && n43.gauntlet43) pulse27.swingSeconds42 = cfg.Num("design.n43.gauntlet43.swingSeconds");
            // G18 parry43: parry42's volume and mercy with parry43's numbers, the catch sphere ahead, the time window, the 0.15 s buffer
            // (Core/Parry43.cs; DroneOrbs.TryReflect asks NearestHostile43; Parry42Step runs the buffer, N42WeaponsStep)
            DroneOrbs orbs = husks.orbs;
            if (n43.parry43 && orbs != null)
            {
                const string pp = "design.n43.parry43.";
                orbs.parry42 = true;
                orbs.reach42 = cfg.Num("orb.reflectRange") * cfg.Num(pp + "reachMult");
                orbs.mercySteps42 = MovementMath.StepsFor(cfg.Num(pp + "mercyS"), cfg.Dt);
                orbs.mercyHeld42 = orbs.mercyParried42 = 0;
                orbs.Parry43Set(true, cfg.Num(pp + "catchRadius"), cfg.Num(pp + "catchAhead"), MovementMath.StepsFor(cfg.Num(pp + "windowS"), cfg.Dt), cfg.movement.body.eyeHeight);
                loadout.Parry42Reset(MovementMath.StepsFor(cfg.Num(pp + "bufferS"), cfg.Dt));
            }
            else if (orbs != null) orbs.Parry43Set(false, 0, 0, 0, 0);
            // G19 fusion43: a high-speed melee hit is a FUSION (Core/Fusion43.cs; the one-line hooks in Combat.cs read loadout.fusion43)
            if (n43.fusion43)
            {
                if (loadout.fusion43 == null) loadout.fusion43 = new Fusion43(events); // once a sim (its `hit` handler is made here)
                loadout.fusion43.Set(true, cfg);
                if (!fusion43Hooked)
                {
                    fusion43Hooked = true;
                    mover.Events += e => { if (e.type == MoverEventType.WallJump) loadout.fusion43?.WallJump(); }; // once: a wall jump (off a ride or a wall)
                }
            }
            else if (loadout.fusion43 != null) loadout.fusion43.Set(false, cfg);
            // G20 slug43: the charged THUNDERCLAP slug throws, stuns and blasts (Core/Slug43.cs; ChargeShots27.FireSlug31's one-line hook)
            if (n43.slug43)
            {
                if (loadout.slug43 == null) loadout.slug43 = new Slug43();
                loadout.slug43.Set(true, cfg);
            }
            else if (loadout.slug43 != null) loadout.slug43.Set(false, cfg);
            // G20 torch43: the TORCH ignites what it hits, its reach ramps in (Core/Torch43.cs; Torch.cs's two one-line hooks)
            if (n43.torch43)
            {
                if (loadout.torch43 == null) loadout.torch43 = new Torch43();
                loadout.torch43.Set(true, cfg);
            }
            else if (loadout.torch43 != null) loadout.torch43.Set(false, cfg);
        }

        bool fusion43Hooked;

        // parry43 round 2 (DESIGN-RULES C-5): the bosses' fast parryable projectiles get the orb's TIME window: THE ICEBREAKER's BLOCK (20 m/s,
        // caught within 3 m + its radius of the eye) and THE LOOM's spool (22 m/s, 3.5 m) are caught as soon as they would touch within
        // design.n43.parry43.windowS: their parry reach becomes max(reach, capsuleRadius + speed x windowS) (5.35 m / 5.85 m), set once on each
        // fight's own params (a fight's boss object is made at its room; NIGHT42's numbers are the starting point). THE WICK's EMBER (9 m/s, 3 m)
        // already reaches past it (0.35 + 9 x 0.25 = 2.6 m): untouched. Off: nothing.
        Brawl40 parry43Brawl;
        Loom37 parry43Loom;
        public double parry43BlockReach, parry43SpoolReach; // the reaches set (the harness prints them)

        void Parry43Bosses()
        {
            if (brawl40 != null && brawl40 != parry43Brawl)
            {
                parry43Brawl = brawl40;
                double w = cfg.Num("design.n43.parry43.windowS"), r = cfg.movement.body.capsuleRadius;
                brawl40.p.blockParryReach = System.Math.Max(brawl40.p.blockParryReach, r + brawl40.p.blockSpeed * w);
                parry43BlockReach = brawl40.p.blockParryReach;
            }
            if (loom37 != null && loom37 != parry43Loom)
            {
                parry43Loom = loom37;
                double w = cfg.Num("design.n43.parry43.windowS"), r = cfg.movement.body.capsuleRadius;
                loom37.p.parryReach = System.Math.Max(loom37.p.parryReach, r + loom37.p.spoolSpeed * w);
                parry43SpoolReach = loom37.p.parryReach;
            }
        }

        /// <summary>NIGHT43 W1: the proof reel's switch (FeelReel.N43.Weapons.cs, the `after` pass): W1's three rows on for the rest of this
        /// run, set up as a run's start would (a tape never records this; harness runs use n43Override). The `before` pass boots with
        /// -skygrindOff43 and never calls it.</summary>
        public void N43WeaponsForceOn()
        {
            n43.gauntlet43 = n43.parry43 = n43.fusion43 = n43.slug43 = n43.torch43 = true;
            N43WeaponsStartRun();
        }

        partial void N43WeaponsStep(double dt)
        {
            if (loadout.fusion43 != null)
            {
                if (Active) loadout.Fusion43HoldStep(dt); // G19 fusion43: the held swing lands when a body comes into reach (off: returns at once)
                loadout.fusion43.Step(mover.DashStepsLeft > 0); // G19 fusion43: the dash's and the wall jump's age (off: returns at once)
            }
            if (Active && loadout.torch43 != null) loadout.Torch43Step(); // G20 torch43: the burns tick (off: returns at once)
            if (n43.parry43) Parry43Bosses(); // G18 parry43 round 2: the bosses' fast projectiles (two reference compares a step)
        }
    }
}
