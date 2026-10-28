// NIGHT31 builder W (design.n31.ltAlways; DESIGN31 A + R6 / R8; GG 2026-10-10: LT must do something on every weapon). Unity-only (the web's LT
// is the zoom or the special). Every campaign weapon's LT always does something, or the slot says why not:
// - KESTREL: the zoom (the web's ads.weapons.revolver.zoom 1.5), whatever the gust says.
// - THUNDERCLAP: the zoom shotgunZoom (1.25, NIGHT25's; NO shockwave: GG removed it in NIGHT25).
// - NAILGUN: the zoom nailgunZoom (1.25) while no nail is stuck in a live body; DETONATE (the SPECIAL path) once one is (R6). Label ZOOM / DETONATE.
// - PULSE gauntlet: the BLAST (PulseCharge27) when the five pips are full and its cooldown is ready; otherwise a PARRY: a parryWindowSeconds (0.25 s)
//   window in which a spitter glob or a drone orb within the punch's reflect reach goes back at its shooter (the web's punch reflect: x2 damage,
//   orb.reflectDamageMult), then parryCooldownSeconds (1.0 s) shown as COOLING with seconds (R6). With the gauntlet in hand LT is its only parry:
//   AltUse.ParryWanted(Loadout, ...) skips the bots' punch parry (R8). A parry moves no enemy: no crowd control, no 15 s rule.
// Per run: reset at a run's start. Off: null on the loadout (NIGHT30's LT). Events (new names): ltParry31Open {weapon}, ltParry31Return {weapon},
// ltDeny31 {weapon, reason}.
namespace Skygrind.Core
{
    public sealed class LtAlways31Params
    {
        public double parryWindowSeconds, parryCooldownSeconds, nailgunZoom, shotgunZoom, wordSeconds;

        public static LtAlways31Params From(GameConfig cfg)
        {
            const string p = "design.n31.ltAlways.";
            return new LtAlways31Params
            {
                parryWindowSeconds = cfg.Num(p + "parryWindowSeconds"),
                parryCooldownSeconds = cfg.Num(p + "parryCooldownSeconds"),
                nailgunZoom = cfg.Num(p + "nailgunZoom"),
                shotgunZoom = cfg.Num(p + "shotgunZoom"),
                wordSeconds = cfg.Num(p + "wordSeconds"),
            };
        }
    }

    public sealed class LtAlways31Event
    {
        public string weapon, reason;
    }

    public sealed class LtAlways31
    {
        public readonly LtAlways31Params p;
        readonly double dt;
        public bool active;
        /// <summary>Steps of the parry window left, of the parry's cooldown left, and of the HUD word (`word`: PARRY / COOLING / why not) left.</summary>
        public int parrySteps, parryCooldown, wordSteps;
        public string word = "";
        /// <summary>The run's counters (the harness reads them).</summary>
        public int parriesOpened, parriesReturned, denies, bossReturns;

        public LtAlways31(GameConfig cfg)
        {
            p = LtAlways31Params.From(cfg);
            dt = cfg.Dt;
        }

        public void Reset(bool on)
        {
            active = on;
            parrySteps = parryCooldown = wordSteps = 0;
            word = "";
            parriesOpened = parriesReturned = denies = bossReturns = 0;
        }

        public int WindowSteps => MovementMath.StepsFor(p.parryWindowSeconds, dt);
        public int CooldownSteps => MovementMath.StepsFor(p.parryCooldownSeconds, dt);

        public void Say(string w)
        {
            word = w;
            wordSteps = MovementMath.StepsFor(p.wordSeconds, dt);
        }

        public void Step()
        {
            if (parryCooldown > 0) parryCooldown--;
            if (wordSteps > 0 && --wordSteps == 0) word = "";
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT31 ltAlways (null with the row off).</summary>
        public LtAlways31 lt31;

        readonly LtAlways31Event lt31Ev = new LtAlways31Event();

        bool Lt31On => lt31 != null && lt31.active;

        /// <summary>NIGHT31 round 2: the sim's boss-projectile parry (from, reach m): true when one boss projectile within reach was sent back (the sim removes
        /// it and hurts its boss). Null: no boss projectiles (a test, a survival run without bosses).</summary>
        public System.Func<Vec3, double, bool> bossParry31;

        /// <summary>The parry's reach (the web's orb.reflectRange: the punch's reflect reach).</summary>
        double parryReach31 => cfg.Has("orb.reflectRange") ? cfg.Num("orb.reflectRange") : 2;

        /// <summary>A nail of this loadout's stuck in a live body (the NAILGUN's LT is DETONATE then, R6). Read from the projectiles themselves:
        /// Enemy.ConductiveNails is counted only on Sandbox19's path.</summary>
        public bool NailsStuck31
        {
            get
            {
                if (projectiles == null || projectiles.StuckCount(this) == 0) return false;
                foreach (Enemy e in enemies.all) if (e.alive && projectiles.StuckOn(e, this) > 0) return true;
                return false;
            }
        }

        /// <summary>The LT zoom under ltAlways (-1: not ltAlways's to say).</summary>
        double ZoomGun31(string id)
        {
            if (!Lt31On) return -1;
            if (id == "revolver") return AdsZoom(id);
            if (id == "shotgun") return lt31.p.shotgunZoom;
            if (id == "nailgun") return NailsStuck31 ? 0 : lt31.p.nailgunZoom;
            return -1;
        }

        /// <summary>The slot's LT word (16 px+ on the HUD): ZOOM, DETONATE, BLAST, PARRY or COOLING (empty: not ltAlways's weapon or the row off).</summary>
        public string LtLabel31(string id)
        {
            if (!Lt31On) return "";
            switch (id)
            {
                case "revolver":
                case "shotgun": return "ZOOM";
                case "nailgun": return NailsStuck31 ? "DETONATE" : "ZOOM";
                case "gauntlet":
                    if (pulse27 != null && pulse27.active && pulse27.Full && pulse27.cooldownSteps <= 0) return "BLAST";
                    return lt31.parryCooldown > 0 ? "COOLING" : "PARRY";
                default: return "";
            }
        }

        /// <summary>Seconds of the gauntlet's parry cooldown left (the slot's COOLING seconds).</summary>
        public double LtParryCoolSeconds31 => Lt31On ? lt31.parryCooldown * cfg.Dt : 0;

        void Deny31(string reason)
        {
            lt31.denies++;
            lt31.Say(reason);
            lt31Ev.weapon = Weapon;
            lt31Ev.reason = reason;
            events.Emit("ltDeny31", lt31Ev);
        }

        /// <summary>The gauntlet's LT when the BLAST cannot go (pips not full or its cooldown running): the parry, or COOLING.</summary>
        void LtParry31()
        {
            if (lt31.parryCooldown > 0)
            {
                Deny31("COOLING");
                return;
            }
            lt31.parrySteps = lt31.WindowSteps;
            lt31.parryCooldown = lt31.CooldownSteps;
            lt31.parriesOpened++;
            lt31.Say("PARRY");
            lt31Ev.weapon = "gauntlet";
            lt31Ev.reason = null;
            events.Emit("ltParry31Open", lt31Ev);
        }

        /// <summary>Per step after the loadout's own step (the sim's N31WeaponsStep): the clocks, the gauntlet's LT when pulse27 does not run it, the open
        /// parry window (a glob / orb in reach goes back: the punch's reflect).</summary>
        public void StepLt31(CombatInput input)
        {
            if (!Lt31On) return;
            lt31.Step();
            if (Weapon != "gauntlet") lt31.parrySteps = 0;
            if (!Pulse27Live && Weapon == "gauntlet" && input.altFire.pressed) LtParry31(); // no glove blast this run: LT is the parry alone
            if (lt31.parrySteps <= 0) return;
            lt31.parrySteps--;
            var (origin, dir) = getAim();
            SpitterAcid.Glob glob = acid?.TryReflect(origin, dir, enemies);
            if (glob == null)
            {
                // NIGHT31 round 2: a boss projectile in reach (the Lantern's orbs, the Wyrm's spit, the Stag's shards; the sim's hook) goes back at its boss
                if (bossParry31 == null || !bossParry31(origin, parryReach31)) return;
                lt31.parrySteps = 0;
                lt31.parriesReturned++;
                lt31.bossReturns++;
                lt31Ev.weapon = "gauntlet";
                lt31Ev.reason = "boss";
                events.Emit("ltParry31Return", lt31Ev);
                return;
            }
            lt31.parrySteps = 0;
            lt31.parriesReturned++;
            lt31Ev.weapon = "gauntlet";
            lt31Ev.reason = null;
            events.Emit("ltParry31Return", lt31Ev);
            events.Emit("parry", new HitEvent { owner = owner, weapon = "gauntlet", kind = "orb", enemy = -1, damage = 0, position = glob.position });
        }
    }
}
