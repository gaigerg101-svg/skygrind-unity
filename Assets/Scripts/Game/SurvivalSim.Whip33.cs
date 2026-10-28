// NIGHT33 builder W (design.n33.whip33, DESIGN33 W, D-3): THE WHIP. GG (2026-10-11): "grapple you towards the enemy ... you don't go through it ...
// a little damage". A4 (c): an enemy zip reeled you at 44 m/s, the Mover let go at arcline.zip.arrive (2.6 m chest to aim point) with the speed kept
// x1.08, so you left through the body at ~47 m/s and hit nothing. The Mover is frozen (CLAUDE.md rule 3): every rule here works from OUTSIDE it.
// - The pick (Core/Arcline21.cs, whip33 on): a small enemy (arcline.pullKinds: husk, swarmer, drone) is yanked to you as before (the web's yank and
//   its 15 s crowd-control cooldown); while that cooldown runs it is a zip target instead of being skipped, so a TAP on an enemy always latches it.
//   A boss body gives way to a node in the cone (rings and boss joints stay pull targets).
// - The stop (N33WhipFrame, before mover.Step, after the NIGHT31 latch and the NIGHT32 charges): while an enemy zip is out, the Mover's own
//   distance release is off (config.arcline.zip.arrive 0 for that pull, the web's value back the step it ends; the zip's maxTime still applies);
//   when one more reel step would bring the gap (chest to the enemy's aim point, less its Radius and the player's capsule radius) under
//   stopGap + stopMargin, the rope is let go on this frame (grapple.held false: the Mover's letGo) with the velocity set to stopKeep toward the enemy, flat.
//   So you stop between stopGap and stopGap + one reel step short of the hit volume, never inside.
// - The arrival (N33WhipStep, after the Mover): a hop (vy at least hopSpeed), a hit of `damage` through Loadout.Damage (weapon "whip33": kill credit,
//   style, points, the kill event), hitstopSteps whole steps of hitstop.
// - Cancels: a jump in the air, a dash (the Mover's own releases) or a second press (the NIGHT31 latch) let go before the stop with the momentum kept
//   (NIGHT32's cancelKeepMax clamp still applies). A dead target lets go at stopKeep.
// - Charges stay (grapple32): a whip hook spends one like any hook; a kill by the whip's own hit gives NO charge back (SurvivalSim.Grapple32.cs
//   G32OnKill; A4: husks have 40 hp, so a refill would chain forever).
// Events (new, PORT-STATUS's list): whip33Latch {kind, enemy, gap} when the hook bites an enemy; whip33Arrive {kind, enemy, gap, damage, killed}
// at the stop. Off (the row, --off33 and every older off switch): nothing here touches a frame, the Mover or the config: NIGHT32's grapple.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>design.n33.whip33's numbers (Unity-only: the web's enemy zip had no stop and no hit).</summary>
    public sealed class Whip33Rules
    {
        public double stopGap, stopMargin, stopKeep, hopSpeed, damage;
        public int hitstopSteps;

        public static Whip33Rules From(GameConfig cfg)
        {
            const string p = "design.n33.whip33.";
            return new Whip33Rules
            {
                stopGap = cfg.Num(p + "stopGap"),
                stopMargin = cfg.Num(p + "stopMargin"),
                stopKeep = cfg.Num(p + "stopKeep"),
                hopSpeed = cfg.Num(p + "hopSpeed"),
                damage = cfg.Num(p + "damage"),
                hitstopSteps = cfg.Int(p + "hitstopSteps"),
            };
        }
    }

    /// <summary>whip33Latch / whip33Arrive's payload.</summary>
    public sealed class Whip33Event
    {
        public string kind;
        public int enemy;
        public double gap, damage;
        public bool killed;
        public Vec3 at;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>The whip's hit's weapon id (Loadout.Damage; the kill event carries it, G32OnKill reads it).</summary>
        public const string Whip33Weapon = "whip33";

        /// <summary>NIGHT33: design.n33.whip33's numbers (null until a run starts with the row on).</summary>
        public Whip33Rules whip33Rules;

        /// <summary>NIGHT33 counters this run (the harness): enemy hooks bitten, stops (arrivals), kills by the whip's hit, pulls whose enemy died
        /// first, kills that gave no charge back.</summary>
        public int whip33Latches, whip33Arrivals, whip33Kills, whip33Lost, whip33NoRefill;

        /// <summary>NIGHT33: the gap (m) at the last stop (the harness and the reel).</summary>
        public double whip33LastGap;

        /// <summary>NIGHT33: the enemy the whip is on now (null: none); the rope view reads it.</summary>
        public Enemy Whip33Target => w33Target;

        double? w33WebArrive;
        Enemy w33Target, w33ArriveEnemy;
        bool w33Bitten;
        readonly Whip33Event w33Event = new Whip33Event();

        partial void N33WhipStartRun()
        {
            ArclineConfig a = cfg.movement.arcline;
            if (w33WebArrive.HasValue) a.zip.arrive = w33WebArrive.Value; // a run that ended mid-pull: the web's arrival back
            w33Target = w33ArriveEnemy = null;
            w33Bitten = false;
            whip33Latches = whip33Arrivals = whip33Kills = whip33Lost = whip33NoRefill = 0;
            whip33LastGap = 0;
            if (!n33.whip33)
            {
                w33WebArrive = null;
                if (arcline21 != null)
                {
                    arcline21.whip33 = false;
                    arcline21.whip33BossKind = null;
                }
                return;
            }
            whip33Rules ??= Whip33Rules.From(cfg);
            w33WebArrive = a.zip.arrive;
            if (arcline21 != null)
            {
                arcline21.whip33 = true;
                arcline21.whip33BossKind = Whip33IsBoss;
            }
        }

        bool Whip33IsBoss(string kind)
        {
            string[] bosses = grapple32Rules != null ? grapple32Rules.bossKinds : null;
            return bosses != null ? Array.IndexOf(bosses, kind) >= 0 : kind == "boss";
        }

        /// <summary>The gap (m) between the player and an enemy's hit volume along the rope: the enemy's own hit shapes (Enemy.RayHit) on the line from
        /// the chest to its aim point, less the player's capsule radius; when that ray misses (the chest inside a shape, or a shape the ray skirts), the
        /// distance to the aim point less the enemy's Radius and the capsule radius. Round 3: the hit shapes, not Radius (a boss's Radius is a box's guess).</summary>
        public double Whip33Gap(Enemy e)
        {
            // round 4: a zip the crosshair's ray took (Arcline21.WhipRay33: THE KILN WYRM) measures along the rope to its anchor, a point on the
            // enemy's hit shapes (so the fallback is that distance, not less a Radius)
            bool onRope = arcline21 != null && arcline21.ZipByRay33 && arcline21.ZipEnemy == e && mover.zipAnchor != null && mover.rope.phase != RopePhase.None;
            Vec3 from = mover.RopePoint, d = (onRope ? mover.rope.anchor : e.AimPoint) - from;
            double len = d.Length, cr = cfg.movement.body.capsuleRadius;
            if (len > 1e-9)
            {
                double hit = e.RayHit(from, d / len, out _);
                if (hit >= 0) return hit - cr;
            }
            return len - (onRope ? 0 : e.Radius) - cr;
        }

        /// <summary>NIGHT33 (the harness's boss rigs build their run first): turn the whip on or off for the run in progress.</summary>
        public void Whip33Arm(bool on)
        {
            n33.whip33 = on;
            N33WhipStartRun();
        }

        /// <summary>Before mover.Step (after N31GrappleFrame and N32GrappleFrame): the whip's stop.</summary>
        partial void N33WhipFrame(ref InputFrame frame)
        {
            if (!n33.whip33 || arcline21 == null) return;
            ArclineConfig a = cfg.movement.arcline;
            Rope r = mover.rope;
            Enemy e = r.phase != RopePhase.None && r.zip && mover.zipAnchor != null ? arcline21.ZipEnemy : null;
            if (e == null)
            {
                a.zip.arrive = w33WebArrive.Value; // no enemy zip: the Mover's own arrival (nodes, surfaces) as before
                w33Target = null;
                w33Bitten = false;
                return;
            }
            a.zip.arrive = 0; // an enemy zip: the Mover never lets go by distance; this hook stops it short
            w33Target = e;
            if (r.phase != RopePhase.Attached) return; // the hook is still flying: the player has not moved yet
            if (!w33Bitten)
            {
                w33Bitten = true;
                whip33Latches++;
                Whip33Emit("whip33Latch", e, Whip33Gap(e), 0, false);
            }
            // a cancel on this step (a second press: the latch already put the button up; a jump in the air; a dash): the Mover lets go, momentum kept
            if (!frame.grapple.held || (frame.jump.pressed && !mover.grounded) || frame.dash.pressed) return;
            if (!e.alive)
            {
                Whip33Let(ref frame, e, whip33Rules.stopKeep);
                whip33Lost++;
                return;
            }
            double gap = Whip33Gap(e);
            if (gap - a.zip.reelSpeed * step > whip33Rules.stopGap + whip33Rules.stopMargin) return; // one more reel step stays outside the stop gap (+ the margin): reel on
            Whip33Let(ref frame, e, whip33Rules.stopKeep);
            w33ArriveEnemy = e;
            whip33LastGap = gap;
        }

        /// <summary>Let go of the whip on this frame (the Mover's letGo) with `keep` m/s flat toward the enemy.</summary>
        void Whip33Let(ref InputFrame frame, Enemy e, double keep)
        {
            frame.grapple.held = false;
            frame.grapple.pressed = false;
            Vec3 to = e.AimPoint - mover.RopePoint;
            double h = JsMath.Hypot(to.x, to.z);
            mover.velocity = h > 1e-9 ? new Vec3(to.x / h * keep, 0, to.z / h * keep) : new Vec3(0, 0, 0);
        }

        /// <summary>After the Mover's step: the arrival's hop, hit and hitstop.</summary>
        partial void N33WhipStep(double dt)
        {
            if (w33ArriveEnemy == null) return;
            Enemy e = w33ArriveEnemy;
            w33ArriveEnemy = null;
            if (!n33.whip33) return;
            Whip33Rules w = whip33Rules;
            mover.velocity = new Vec3(mover.velocity.x, JsMath.Max(mover.velocity.y, w.hopSpeed), mover.velocity.z);
            bool killed = e.alive && loadout.Damage(e, w.damage, false, Whip33Weapon, Eye);
            whip33Arrivals++;
            if (killed) whip33Kills++;
            if (w.hitstopSteps > 0) Hitstop(w.hitstopSteps * cfg.Dt, "whip33");
            Whip33Emit("whip33Arrive", e, whip33LastGap, w.damage, killed);
        }

        void Whip33Emit(string name, Enemy e, double gap, double damage, bool killed)
        {
            w33Event.kind = e.kind;
            w33Event.enemy = e.id;
            w33Event.gap = gap;
            w33Event.damage = damage;
            w33Event.killed = killed;
            w33Event.at = e.AimPoint;
            events.Emit(name, w33Event);
        }
    }
}
