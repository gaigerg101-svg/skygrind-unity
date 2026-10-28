// NIGHT43 builder W1 fusion43 (N43Blocks.fusion43, design.n43.fusion43; GG 2026-10-16 G19: "If the player performs a high-speed dash impact or
// gauntlet attack, create a powerful special feedback response"; combat-web.md D "the empowered impact"). The sim's half: the rule, the damage,
// the knock, the stagger, the hitstop request and the event; F1 draws it (Game/Fusion43View.cs) on N43Events.Fusion.
// A gauntlet swing's hit or a punch's hit is a FUSION when, at the hit, the player
//   - moves at speedMin m/s or more (horizontal), or
//   - is dashing, or ended a dash at most dashAfterS ago, or
//   - wall-jumped (off a ride or a wall) at most rideJumpS ago,
// and no FUSION landed in the last cooldownS. Then: the hit's damage x damageMult (x bossDamageMult on a boss body: Loadout.IsBoss34), a knock of
// knockSpeed m/s for knockTime s away from the player, Enemy.Stun(staggerS) (boss bodies ignore both: their Knockback / Stun are empty), a
// hitstop request of hitstopS (bossHitstopS on a boss) through the sim's Hitstop with the cause "fusion43" (ImpactRules' Fusion43 tier), and
// ONE N43Events.Fusion per hit with the reused Fusion43Event. While a FUSION is ready the swing and the punch also reach reachBonus m farther
// when nothing is in their normal reach (a dash INTO a body lands). Every hook in Combat.cs is one line; with the row off (fusion43 null or
// not on) Fusion43Pre returns exactly 1 and nothing else runs: NIGHT42's melee bit for bit.
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>fusion43's numbers and per-run state (the sim makes it once, SurvivalSim.N43.Weapons.cs; Set at each run's start).</summary>
    public sealed class Fusion43
    {
        public bool on;
        public double speedMin, damageMult, bossDamageMult, knockSpeed, knockTime, staggerS, hitstopS, bossHitstopS, reachBonus;
        public int dashAfterSteps, rideJumpSteps, cooldownSteps;

        /// <summary>THE HELD SWING (round 2): a gauntlet swing that found nothing while a FUSION was ready waits up to holdSteps for a body
        /// to come into its reach and then lands (holdLeft: the steps left; heldSwings: the run's held swings that landed).</summary>
        public int holdSteps, holdLeft, heldSwings, heldArmed;
        public bool inHold;

        /// <summary>Steps since the last dash ended / the last wall jump (large: none this run), steps left of the cooldown.</summary>
        public int sinceDash = Never, sinceWallJump = Never, cooldownLeft;
        const int Never = 1 << 28;

        /// <summary>The hit being judged (Fusion43Pre to Fusion43Post): live, why ("speed" | "dash" | "wallJump"), the speed, the damage the
        /// web's `hit` reported for it (-1: no hit event, e.g. a shield blocked it).</summary>
        public bool live;
        public string why;
        public double speedAt, dealt = -1;
        public bool killedSeen;

        /// <summary>This run's counters (the harness reads them): FUSIONs by why, melee hits judged, extra-reach picks.</summary>
        public int fusions, bySpeed, byDash, byWallJump, judged, reachPicks;

        public readonly Fusion43Event payload = new Fusion43Event();

        public Fusion43(EventBus events)
        {
            events.On<HitEvent>("hit", OnHit); // once: the handler is allocated here, never per hit
        }

        void OnHit(HitEvent e)
        {
            if (!live || e.owner != "player" || e.shock) return;
            dealt = e.damage;
            killedSeen = e.killed;
        }

        /// <summary>A run's start: the numbers (on false: everything off).</summary>
        public void Set(bool on, GameConfig cfg)
        {
            this.on = on;
            live = false;
            sinceDash = sinceWallJump = Never;
            cooldownLeft = 0;
            fusions = bySpeed = byDash = byWallJump = judged = reachPicks = 0;
            holdLeft = heldSwings = heldArmed = 0;
            if (!on) return;
            const string r = "design.n43.fusion43.";
            double dt = cfg.Dt;
            speedMin = cfg.Num(r + "speedMin");
            dashAfterSteps = MovementMath.StepsFor(cfg.Num(r + "dashAfterS"), dt);
            rideJumpSteps = MovementMath.StepsFor(cfg.Num(r + "rideJumpS"), dt);
            damageMult = cfg.Num(r + "damageMult");
            bossDamageMult = cfg.Num(r + "bossDamageMult");
            knockSpeed = cfg.Num(r + "knockSpeed");
            knockTime = cfg.Num(r + "knockTime");
            staggerS = cfg.Num(r + "staggerS");
            hitstopS = cfg.Num(r + "hitstopS");
            bossHitstopS = cfg.Num(r + "bossHitstopS");
            reachBonus = cfg.Num(r + "reachBonus");
            cooldownSteps = MovementMath.StepsFor(cfg.Num(r + "cooldownS"), dt);
            holdSteps = cfg.Has(r + "holdS") ? MovementMath.StepsFor(cfg.Num(r + "holdS"), dt) : 0;
        }

        /// <summary>The sim's mover event: a wall jump (off a ride or a wall).</summary>
        public void WallJump() => sinceWallJump = 0;

        /// <summary>Every step after combat (SurvivalSim.N43WeaponsStep): the dash's and the wall jump's age, the cooldown.</summary>
        public void Step(bool dashing)
        {
            if (!on) return;
            if (dashing) sinceDash = 0;
            else if (sinceDash < Never) sinceDash++;
            if (sinceWallJump < Never) sinceWallJump++;
            if (cooldownLeft > 0) cooldownLeft--;
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT43 W1 fusion43 (null or not on: off; the sim sets it, SurvivalSim.N43.Weapons.cs).</summary>
        public Fusion43 fusion43;

        bool F43 => fusion43 != null && fusion43.on;

        /// <summary>fusion43: would a melee hit now be a FUSION? (why: its reason).</summary>
        public bool Fusion43Ready(out string why, out double speed)
        {
            why = null;
            speed = JsMath.Hypot(mover.velocity.x, mover.velocity.z);
            if (!F43 || fusion43.cooldownLeft > 0) return false;
            if (mover.DashStepsLeft > 0 || fusion43.sinceDash <= fusion43.dashAfterSteps) why = "dash";
            else if (fusion43.sinceWallJump <= fusion43.rideJumpSteps) why = "wallJump";
            else if (speed >= fusion43.speedMin) why = "speed";
            return why != null;
        }

        /// <summary>fusion43's reach: nothing in the swing's / punch's own reach and a FUSION ready: the first body within reachBonus m more
        /// (the gauntlet's sweep with its line-of-sight test; the punch's pick). Null otherwise (off: always null).</summary>
        TargetPick? Fusion43Reach(Vec3 origin, Vec3 dir, List<BodyTarget> targets, double range, double coneDeg, bool gauntlet)
        {
            if (!F43 || !Fusion43Ready(out _, out _)) return null;
            double reach = range + fusion43.reachBonus, half = coneDeg * System.Math.PI / 180;
            TargetPick? pick = null;
            if (gauntlet)
            {
                foreach (TargetPick p in CombatMath.SweepTargets(origin, dir, targets, reach, half, found))
                {
                    if (HasLos(origin, ((Enemy)p.target.enemy).AimPoint))
                    {
                        pick = p;
                        break;
                    }
                }
            }
            else pick = CombatMath.PickPunchTarget(origin, dir, targets, reach, half);
            if (pick.HasValue) fusion43.reachPicks++;
            return pick;
        }

        /// <summary>fusion43's HELD SWING: the gauntlet's swing found no body while a FUSION is ready: it waits holdSteps for one (SwingGauntlet's
        /// one-line hook; nothing off, or with no hold configured).</summary>
        void Fusion43Arm()
        {
            if (!F43 || fusion43.holdSteps <= 0 || fusion43.inHold || !Fusion43Ready(out _, out _)) return;
            fusion43.holdLeft = fusion43.holdSteps;
            fusion43.heldArmed++;
        }

        /// <summary>fusion43's HELD SWING, every sim step (SurvivalSim.N43WeaponsStep): a body now in the FUSION reach lands the swing (the whole
        /// gauntlet swing runs again at that step: its reflect test, its hit, its `meleeSwing` with hits 1; its cooldown is the press's).</summary>
        public void Fusion43HoldStep(double dt)
        {
            if (!F43 || fusion43.holdLeft <= 0) return;
            fusion43.holdLeft--;
            if (Weapon != "gauntlet" || !ready) { fusion43.holdLeft = 0; return; }
            var (origin, dir) = getAim();
            if (!Fusion43Reach(origin, dir, GauntletTargets34(), gRange, gCone, true).HasValue) return;
            fusion43.reachPicks--; // the probe is not a pick: SwingGauntlet counts its own
            fusion43.holdLeft = 0;
            fusion43.heldSwings++;
            fusion43.inHold = true; // a held swing never arms another
            SwingGauntlet(dt);
            fusion43.inHold = false;
        }

        /// <summary>fusion43, inside the melee hit's damage argument: x damageMult (x bossDamageMult on a boss) when the hit is a FUSION
        /// (it is then judged live until Fusion43Post); exactly 1 otherwise.</summary>
        double Fusion43Pre(Enemy enemy)
        {
            if (!F43) return 1;
            fusion43.judged++;
            if (!Fusion43Ready(out string why, out double speed)) return 1;
            fusion43.live = true;
            fusion43.why = why;
            fusion43.speedAt = speed;
            fusion43.dealt = -1;
            fusion43.killedSeen = false;
            return IsBoss34(enemy) ? fusion43.bossDamageMult : fusion43.damageMult;
        }

        /// <summary>fusion43, right after the melee hit's Damage: the knock, the stagger, the hitstop, the event (nothing unless Pre judged
        /// a FUSION and the hit landed: a blocked hit is no FUSION).</summary>
        void Fusion43Post(Enemy enemy, string weapon, Vec3 dir, double dt)
        {
            if (!F43 || !fusion43.live) return;
            Fusion43 f = fusion43;
            f.live = false;
            if (f.dealt < 0) return; // no `hit`: blocked (a shield, a stand-in)
            f.cooldownLeft = f.cooldownSteps;
            f.fusions++;
            if (f.why == "dash") f.byDash++;
            else if (f.why == "wallJump") f.byWallJump++;
            else f.bySpeed++;
            bool boss = IsBoss34(enemy);
            if (enemy.alive)
            {
                MeleeKnock40(enemy, enemy.position.x - mover.position.x, enemy.position.z - mover.position.z, f.knockSpeed, f.knockTime, dt);
                enemy.Stun(f.staggerS, dt);
            }
            hitstop41?.Invoke(boss ? f.bossHitstopS : f.hitstopS, N43Events.Fusion);
            Fusion43Event e = f.payload;
            e.owner = owner;
            e.position = enemy.AimPoint;
            e.dir = dir;
            e.speed = f.speedAt;
            e.weapon = weapon;
            e.enemy = enemy.id;
            e.kind = enemy.kind;
            e.killed = f.killedSeen || !enemy.alive;
            e.damage = f.dealt;
            events.Emit(N43Events.Fusion, e);
        }
    }
}
