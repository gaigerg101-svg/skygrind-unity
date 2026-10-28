// NIGHT36 builder P (the core loop; NIGHT36.md card P1): the sim side of tells36, parry36, launch36 and heal36, through the lead's hooks
// (SurvivalSim.N36.cs: N36CoreStartRun at a run's start, N36CoreStep every step after F's). Each row is its own N36Blocks bit; every
// object below is made on the first run that needs it, and a run with the row off leaves the old rule in place bit for bit:
// - tells36 (Core/Tell36.cs): the wind-ups of the ordinary enemies stretch to design.n36.tells36's (husk melee, spitter volley, swarmer
//   crouch; the web's numbers come back at the next run with the row off) and every wind-up raises a tell in `tells36` (YELLOW: the husk's
//   melee and the spitter's volley, both parryable; RED: the swarmer's pounce). Builder B's bosses raise theirs through the same registry.
// - parry36 (Core/Parry36.cs): the loadout's early buffer and the glob's return to its shooter with a burst; every "parry36" freezes the
//   screen design.n36.parry36.freezeS (the sim's hitstop, cause "parry36").
// - launch36 (Core/Launch36.cs): a moving punch launches; flights meet bodies or walls here, after the enemies moved.
// - heal36 (Core/Heal36.cs): a kill of the player's within the radius heals.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT36 P tells36: the run's live tells (never null after the first run start; Raise does nothing with the row off).</summary>
        public Tells36 tells36;

        /// <summary>NIGHT36 P: the rows' objects (null until a run needs them; the harness and the reels read their counters).</summary>
        public Parry36 parry36Core;
        public Launch36 launch36Core;
        public Heal36 heal36Core;

        // tells36: the web's wind-ups, saved at the first run start (restored with the row off)
        bool core36Saved, core36Wired;
        double huskWindupWeb, spitterTellWeb, swarmerTellWeb, droneChargeWeb, chargerTellWeb;

        /// <summary>The scan's attack ids (reference-compared: the scan ends only its own tells, never a boss's).</summary>
        public const string Tell36HuskMelee = "huskMelee", Tell36SpitterVolley = "spitterVolley", Tell36SwarmerPounce = "swarmerPounce",
            Tell36DroneShot = "droneShot", Tell36ChargerCharge = "chargerCharge", Tell36LancerBeam = "lancerBeam";

        partial void N36CoreStartRun()
        {
            tells36 ??= new Tells36(events);
            tells36.Reset();
            tells36.enabled = n36.tells36;
            if (!core36Saved)
            {
                huskWindupWeb = husks.p.windupTime;
                spitterTellWeb = husks.acid.p.tellTime;
                swarmerTellWeb = husks.swarmer.tellTime;
                droneChargeWeb = husks.drone.chargeTime;
                chargerTellWeb = husks.chargerAttack.tellTime;
                core36Saved = true;
            }
            husks.p.windupTime = n36.tells36 ? cfg.Num("design.n36.tells36.huskWindupS") : huskWindupWeb;
            husks.acid.p.tellTime = n36.tells36 ? cfg.Num("design.n36.tells36.spitterTellS") : spitterTellWeb;
            husks.swarmer.tellTime = n36.tells36 ? cfg.Num("design.n36.tells36.swarmerTellS") : swarmerTellWeb;
            husks.drone.chargeTime = n36.tells36 ? cfg.Num("design.n36.tells36.droneChargeS") : droneChargeWeb;
            husks.chargerAttack.tellTime = n36.tells36 ? cfg.Num("design.n36.tells36.chargerTellS") : chargerTellWeb;

            if (n36.parry36)
            {
                parry36Core ??= new Parry36(cfg);
                parry36Core.Reset();
                loadout.parry36 = parry36Core;
                husks.acid.onReflectEnd36 = loadout.Parry36Burst;
                if (husks.orbs != null) husks.orbs.onReflectEnd36 = loadout.Parry36Burst;
            }
            else
            {
                loadout.parry36 = null;
                husks.acid.onReflectEnd36 = null;
                if (husks.orbs != null) husks.orbs.onReflectEnd36 = null;
            }

            if (n36.launch36)
            {
                launch36Core ??= new Launch36(cfg);
                launch36Core.Reset();
                loadout.launch36 = launch36Core;
            }
            else loadout.launch36 = null;

            if (n36.heal36)
            {
                heal36Core ??= new Heal36(cfg);
                heal36Core.Reset();
            }

            if (!core36Wired)
            {
                core36Wired = true;
                events.On<KillEvent>("kill", e =>
                {
                    if (!n36.heal36 || heal36Core == null || e.owner != "player" || health.Dead) return;
                    Vec3 p = mover.position;
                    heal36Core.OnKill(e.position, new Vec3(p.x, p.y + cfg.movement.body.eyeHeight, p.z), health, events, e.owner, Rank56HealScale()); // NIGHT56 G1 rank56 (1 off)
                });
                events.On<HitEvent>(Parry36.Event, e =>
                {
                    if (n36.parry36 && parry36Core != null && e.owner == "player") Hitstop(parry36Core.freezeS, "parry36");
                });
            }
        }

        partial void N36CoreStep(double dt)
        {
            if (n36.tells36) Tells36Scan();
            if (n36.parry36) loadout.Parry36Step(dt);
            if (n36.launch36)
            {
                launch36Core.Track(mover);
                launch36Core.Step(world, enemies, loadout.Launch36Damage, events);
            }
            if (n36.tells36) tells36.Step();
        }

        /// <summary>tells36: every ordinary enemy's wind-up this step raises (or keeps in step) its tell; a wind-up that stopped ends it.</summary>
        void Tells36Scan()
        {
            var all = enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Enemy e = all[i];
                string attack = null;
                Tell36Kind kind = Tell36Kind.None;
                int left = 0, total = 0;
                if (e.alive)
                {
                    if (e is Spitter s)
                    {
                        if (s.state == "tell")
                        {
                            attack = Tell36SpitterVolley;
                            kind = Tell36Kind.Yellow;
                            left = s.stateSteps;
                            total = s.tellSteps;
                        }
                    }
                    else if (e is Swarmer w)
                    {
                        if (w.state == "crouch")
                        {
                            attack = Tell36SwarmerPounce;
                            kind = Tell36Kind.Red;
                            left = w.stateSteps;
                            total = MovementMath.StepsFor(husks.swarmer.tellTime, step);
                        }
                    }
                    else if (e is Drone dr)
                    {
                        if (dr.chargeSteps > 0)
                        {
                            attack = Tell36DroneShot; // its orb is parryable: YELLOW
                            kind = Tell36Kind.Yellow;
                            left = dr.chargeSteps;
                            total = MovementMath.StepsFor(husks.drone.chargeTime, step);
                        }
                    }
                    else if (e is Charger ch)
                    {
                        if (ch.state == "tell")
                        {
                            attack = Tell36ChargerCharge; // a 150 hp body at 22 m/s: dodge it (RED)
                            kind = Tell36Kind.Red;
                            left = ch.stateSteps;
                            total = MovementMath.StepsFor(husks.chargerAttack.tellTime, step);
                        }
                    }
                    else if (e is Lancer ln)
                    {
                        if (ln.state == Lancer.State.Aim)
                        {
                            attack = Tell36LancerBeam; // a hitscan beam from a far perch: no punch reaches it (RED: step off the line)
                            kind = Tell36Kind.Red;
                            left = ln.stateSteps;
                            total = MovementMath.StepsFor(husks.lancer.aimTime, step);
                        }
                    }
                    else if (e.GetType() == typeof(Husk) && e.windupSteps > 0)
                    {
                        attack = Tell36HuskMelee;
                        kind = Tell36Kind.Yellow;
                        left = e.windupSteps;
                        total = MovementMath.StepsFor(husks.p.windupTime, step);
                    }
                }
                Tell36 t = tells36.Of(e.id);
                if (attack == null)
                {
                    if (t != null && IsScanTell(t.attack)) tells36.End(e.id, t.stepsLeft > 2); // a wind-up that ran out landed: not an early end
                    continue;
                }
                if (t == null || (object)t.attack != attack)
                {
                    t = tells36.Raise(e.id, attack, kind, total > left ? total : left, e.AimPoint);
                    if (t == null) continue;
                }
                t.stepsLeft = left + 1; // the registry's Step below counts this step off
                t.at = e.AimPoint;
            }
        }

        public static bool IsScanTell(string a) => (object)a == Tell36HuskMelee || (object)a == Tell36SpitterVolley || (object)a == Tell36SwarmerPounce
            || (object)a == Tell36DroneShot || (object)a == Tell36ChargerCharge || (object)a == Tell36LancerBeam;
    }
}
