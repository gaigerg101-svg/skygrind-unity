// NIGHT19 U3-33.2 (the lead owns this file; builders never edit it): THE RANGE. A gun (or a pair, or a rotation) against
// a cell's real enemies on the Survival map's open floor (Sandbox.OpenSpot), the player scripted per cell (stand, strafe
// +/-4 m, back-pedal, boost every 2 s), the aim model of Sandbox.cs, the punch always available and used as a parry when
// an enemy within 2 m winds up, the alt-fire by the catalog's own rule (AltUse: builder W's AutopilotGuns.AltUse, the rule
// the bot reads too; until W lands it, never). The enemies spawn as the director spawns them (Director.SpawnGroup:
// husks.Spawn(kind), the round's HP mult rounded, brain.Attach) at the cell's round (Director.StartWave sets the mult;
// its own squads are cleared), and the run's clock is held (SurvivalSim.holdRunClock: no break end, no wave) while the
// cell plays, in a FIGHT state (kills pay points as in a round). The player can't die (1e6 HP); the damage taken is
// counted from the bus's `hurt`. One episode's score: the time to clear in s (capped at capSeconds) + damageWeight x the
// HP taken. A kind with no port (Director.Available) or an affix with no hook makes the cell n/a.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class SandboxRange
    {
        /// <summary>One episode's numbers.</summary>
        public sealed class Result
        {
            public string cell, guns;
            public uint seed;
            public bool na, cleared;
            public string why;
            public double clear, hurt, score, points, seconds;
            public int kills, steps;
        }

        /// <summary>The alt-fire rule (builder W's AutopilotGuns.AltUse once it lands, the bot's own): (sim, gun in hand, the
        /// target, its distance, enemies within 4 m) =&gt; hold the alt this step. Null: never.</summary>
        public static Func<SurvivalSim, string, Enemy, double, int, bool> AltUse;

        /// <summary>NIGHT20: the SPECIAL rule (builder G's AutopilotGuns.SpecialHeld: the STITCHER's detonate moved to SPECIAL
        /// with design.zoom), the same arguments as AltUse; null: never.</summary>
        public static Func<SurvivalSim, string, Enemy, double, int, bool> SpecialUse;

        /// <summary>A cell affix's hook (builder E's SHIELDED once it lands): (the spawned enemy, the affix) =&gt; applied.
        /// Null, or false: the cell is n/a.</summary>
        public static Func<Enemy, string, bool> ApplyAffix;

        public readonly SurvivalSim sim;
        public readonly SandboxSpec spec;
        public readonly Vec3 spot;
        public readonly double spotYaw, spotClear;
        readonly ScriptedInput script = new ScriptedInput();
        readonly List<Enemy> mine = new List<Enemy>();
        double hurt;

        public SandboxRange(GameConfig cfg, SurvivalMap map, SandboxSpec spec)
        {
            this.spec = spec;
            sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.metaSilent = true;
            sim.assistModeFor = _ => "off"; // the judge's own aim model, no assist (the AUTOPILOT's rule 6)
            sim.UseInput(script);
            sim.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player") hurt += e.amount;
            });
            (spot, spotYaw, spotClear) = Sandbox.OpenSpot(sim);
            ApplyAffix = sim.RangeAffix; // NIGHT20 (builder E): the ported affixes (C10 shielded) on a cell's spawns
        }

        /// <summary>Why a cell can't run on this build (a kind with no port, an affix with no hook), or null.</summary>
        public static string NotAvailable(SandboxCell cell)
        {
            foreach (SandboxGroup g in cell.groups)
            {
                if (!Director.Ported(g.kind)) return $"no {g.kind.ToUpperInvariant()} port on this build";
                if (g.affix != null && ApplyAffix == null) return $"no {g.affix.ToUpperInvariant()} affix on this build";
            }
            return null;
        }

        /// <summary>One episode: `guns` owned (slot order), the schedule picks the slot by time (null: slot 0 throughout).</summary>
        public Result Run(SandboxCell cell, List<string> guns, uint seed, Func<double, int> slotAt = null)
        {
            var r = new Result { cell = cell.id, guns = string.Join("+", guns), seed = seed };
            string na = NotAvailable(cell);
            if (na != null)
            {
                r.na = true;
                r.why = na;
                return r;
            }
            GameConfig cfg = sim.cfg;
            double dt = sim.step;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            sim.zones.OpenAll(); // the whole map (Sandbox.OpenSpot)
            sim.holdRunClock = true;
            sim.director.StartWave(cell.round, seed); // the round's HP mult (DirectorMath.RoundHpMult)
            double hpMult = sim.director.HpMult;
            sim.director.Clear(); // its own squads and telegraphs: the cell spawns its own
            sim.run.state = SurvivalRun.State.Fight;
            sim.health.max = sim.health.hp = 1e6;
            sim.loadout.Reset(new List<string>(guns), seed);
            if (cell.forged) foreach (string g in guns) sim.forge.Give(g);
            sim.Respawn(spot, spotYaw * 180 / Math.PI);
            script.SetLook(spotYaw, 0);
            mine.Clear();
            foreach (SandboxGroup g in cell.groups)
            {
                for (int i = 0; i < g.count; i++)
                {
                    double ahead = g.line > 0 ? g.distance + i * g.line : g.distance;
                    double side = g.line > 0 || g.count == 1 ? 0 : -g.spread / 2 + g.spread * i / (g.count - 1);
                    Vec3 at = Sandbox.Along(spot, spotYaw, ahead, side);
                    Husk h = sim.husks.Spawn(g.kind, at, spot);
                    if (h == null) continue;
                    h.hp = JsMath.Round(h.hp * hpMult); // main.js onSpawn: rounded, maxHp stays the kind's
                    sim.brain.Attach(h);
                    if (g.affix != null && !ApplyAffix(h, g.affix))
                    {
                        r.na = true;
                        r.why = $"the {g.affix} affix refused";
                        return r;
                    }
                    mine.Add(h);
                }
            }
            hurt = 0;
            double earned0 = sim.economy.earned;
            int kills0 = sim.run.stats.kills;
            var rng = new Mulberry32(seed ^ spec.aimSalt);
            int cap = MovementMath.StepsFor(spec.capSeconds, dt);
            int boostEvery = MovementMath.StepsFor(spec.boostEvery, dt);
            bool moving = cell.script != "stand";
            double sigma = (moving ? spec.sigmaMoveDeg : spec.sigmaStandDeg) * Math.PI / 180;
            double strafeDir = 1;
            int step = 0, lastSlot = -1;
            for (; step < cap; step++)
            {
                Enemy target = null;
                double best = double.PositiveInfinity;
                int near4 = 0;
                bool windup = false;
                Vec3 me = sim.mover.position;
                foreach (Enemy e in mine)
                {
                    if (!e.alive) continue;
                    double dx = e.position.x - me.x, dz = e.position.z - me.z;
                    double d = Math.Sqrt(dx * dx + dz * dz);
                    if (d < best)
                    {
                        best = d;
                        target = e;
                    }
                    if (d <= 4) near4++;
                    if (d <= spec.parryRange && e.windupSteps > 0) windup = true;
                }
                if (target == null) break; // cleared
                Vec3 eye = sim.Eye;
                string weapon = sim.loadout.Weapon;
                Sandbox.Look(eye, Sandbox.AimPoint(cfg, weapon, target, eye), out double yaw, out double pitch);
                yaw += Sandbox.Gauss(rng) * sigma;
                pitch += Sandbox.Gauss(rng) * sigma;
                script.SetLook(yaw, pitch);
                var move = new InputFrame { yaw = yaw, pitch = pitch };
                if (cell.script == "strafe" || cell.script == "boost")
                {
                    // +/- strafeMeters across the spot's heading (the side offset from the spot)
                    Vec3 off = new Vec3(me.x - spot.x, 0, me.z - spot.z);
                    double side = off.x * JsMath.Cos(spotYaw) - off.z * JsMath.Sin(spotYaw); // the spot's right vector (-fz, fx)
                    if (side >= spec.strafeMeters) strafeDir = -1;
                    else if (side <= -spec.strafeMeters) strafeDir = 1;
                    // the stick is relative to the look: the spot's right vector in the look's frame
                    double rel = spotYaw - yaw;
                    move.moveX = strafeDir * JsMath.Cos(rel);
                    move.moveZ = strafeDir * JsMath.Sin(rel);
                    if (cell.script == "boost" && step > 0 && step % boostEvery == 0) move.dash = new Button { held = true, pressed = true };
                }
                else if (cell.script == "backpedal")
                {
                    move.moveZ = -1;
                }
                int slot = slotAt != null ? slotAt(step * dt) : 0;
                var combat = new CombatInput
                {
                    weapon = slot != lastSlot ? slot : -1,
                    fire = Charge27Tap(sim, weapon, step, Sandbox.Trigger(sim, weapon)),
                    // NIGHT31 W (R8, design.n31.ltAlways): with the gauntlet in hand its LT is the only parry: AltUse.ParryWanted(Loadout, ...) skips it; off: NIGHT30's punch
                    punch = windup && !(sim.loadout.lt31 != null && sim.loadout.lt31.active && !Skygrind.Core.AltUse.ParryWanted(sim.loadout, me, sim.loadout.AltParams ?? new AltUseParams { parryRange = spec.parryRange })) ? new Button { held = true, pressed = true } : default,
                };
                lastSlot = slot;
                if (AltUse != null && AltUse(sim, weapon, target, best, near4)) combat.altFire = new Button { held = true, pressed = true };
                if (SpecialUse != null && SpecialUse(sim, weapon, target, best, near4)) combat.special = new Button { held = true, pressed = true }; // NIGHT20
                script.Set(move, combat);
                sim.Step();
            }
            r.steps = step;
            r.seconds = step * dt;
            r.cleared = step < cap;
            r.clear = r.cleared ? r.seconds : spec.capSeconds;
            r.hurt = hurt;
            r.score = r.clear + spec.damageWeight * hurt;
            r.points = sim.economy.earned - earned0;
            r.kills = sim.run.stats.kills - kills0;
            sim.holdRunClock = false;
            return r;
        }

        /// <summary>NIGHT27 chargeShots27: the bot TAPS the pistol and the shotgun (the trigger is let go every other step) so a held trigger never reaches the 0.4 s
        /// charge threshold: the table measures the normal shot (the charged one is a 10 s / 14 s cooldown move the table does not model). Off: the trigger as it was.</summary>
        static Button Charge27Tap(SurvivalSim sim, string weapon, int step, Button trigger)
        {
            if (sim.charge27 != null && sim.charge27.active && trigger.held && (weapon == "revolver" || weapon == "shotgun") && (step & 1) == 1) return default;
            return trigger;
        }

        /// <summary>A cell's median score over spec.seeds seeds (seed 1 .. n), and the episodes.</summary>
        public double Median(SandboxCell cell, List<string> guns, List<Result> episodes, Func<double, int> slotAt = null)
        {
            var scores = new List<double>();
            for (int s = 1; s <= spec.seeds; s++)
            {
                Result r = Run(cell, guns, (uint)s, slotAt);
                episodes?.Add(r);
                if (r.na) return double.NaN;
                scores.Add(r.score);
            }
            return Sandbox.Median(scores);
        }

        // --- the gust probes (S10: THE RANGE's half) ---------------------------------------------------------------------------

        /// <summary>The peak height (m over the floor) of a jump from the spot standing still, with `alt` (the KESTREL's LT, held
        /// one step) or a COMET shot at the feet (`rocket`) fired `after` s into the jump, looking straight down. 0 / 0: a plain jump.</summary>
        public double JumpPeak(string gun, bool alt, double after, uint seed = 1)
        {
            GameConfig cfg = sim.cfg;
            double dt = sim.step;
            sim.StartRun(seed, null, null);
            sim.zones.OpenAll(); // the whole map (Sandbox.OpenSpot)
            sim.holdRunClock = true;
            sim.director.Clear();
            sim.run.state = SurvivalRun.State.Fight;
            sim.health.max = sim.health.hp = 1e6;
            sim.loadout.Reset(new List<string> { gun }, seed);
            sim.Respawn(spot, spotYaw * 180 / Math.PI);
            double floor = spot.y, peak = 0;
            int fireAt = MovementMath.StepsFor(after, dt);
            for (int i = 0; i < MovementMath.StepsFor(2.5, dt); i++)
            {
                script.SetLook(spotYaw, -Math.PI / 2 + 0.01);
                var move = new InputFrame { yaw = spotYaw, pitch = -Math.PI / 2 + 0.01, jump = new Button { held = i < 10, pressed = i == 0 } };
                var combat = new CombatInput { weapon = -1 };
                if (after > 0 && i == fireAt)
                {
                    if (alt) combat.altFire = new Button { held = true, pressed = true };
                    else combat.fire = new Button { held = true, pressed = true };
                }
                script.Set(move, combat);
                sim.Step();
                peak = Math.Max(peak, sim.mover.position.y - floor);
            }
            sim.holdRunClock = false;
            return peak;
        }

        /// <summary>C3's swarm with the KESTREL: the gust (LT, one press) when the nearest SWARMER is within `at` m; how many of
        /// the 8 were pushed back at least `back` m (their distance to the player 0.5 s later minus before), and whether a gust
        /// fired at all (an `N19Events.Gust` on the bus).</summary>
        public (int pushed, int of, bool fired) GustSwarm(SandboxCell c3, double at, double back, uint seed = 1, string gun = "revolver")
        {
            int fired = 0;
            Action<object> onGust = _ => fired++;
            sim.events.On(N19Events.Gust, onGust);
            sim.events.On(N20Events.Shockwave, onGust); // NIGHT20: the THUNDERCLAP's shockwave (gun "shotgun")
            GameConfig cfg = sim.cfg;
            double dt = sim.step;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed, null, null);
            sim.zones.OpenAll(); // the whole map (Sandbox.OpenSpot)
            sim.holdRunClock = true;
            sim.director.StartWave(c3.round, seed);
            double hpMult = sim.director.HpMult;
            sim.director.Clear();
            sim.run.state = SurvivalRun.State.Fight;
            sim.health.max = sim.health.hp = 1e6;
            sim.loadout.Reset(new List<string> { gun }, seed);
            sim.Respawn(spot, spotYaw * 180 / Math.PI);
            mine.Clear();
            SandboxGroup g = c3.groups[0];
            for (int i = 0; i < g.count; i++)
            {
                double side = g.count == 1 ? 0 : -g.spread / 2 + g.spread * i / (g.count - 1);
                Husk h = sim.husks.Spawn(g.kind, Sandbox.Along(spot, spotYaw, g.distance, side), spot);
                if (h == null) continue;
                h.hp = JsMath.Round(h.hp * hpMult);
                sim.brain.Attach(h);
                mine.Add(h);
            }
            var before = new double[mine.Count];
            int firedAt = -1, pushed = 0;
            int after = MovementMath.StepsFor(0.5, dt);
            for (int i = 0; i < MovementMath.StepsFor(8, dt); i++)
            {
                Vec3 me = sim.mover.position;
                double nearest = double.PositiveInfinity;
                foreach (Enemy e in mine) if (e.alive) nearest = Math.Min(nearest, Dist(e.position, me));
                var combat = new CombatInput { weapon = -1 };
                if (firedAt < 0 && nearest <= at)
                {
                    firedAt = i;
                    for (int k = 0; k < mine.Count; k++) before[k] = Dist(mine[k].position, me);
                    combat.altFire = new Button { held = true, pressed = true };
                }
                script.SetLook(spotYaw, 0);
                script.Set(new InputFrame { yaw = spotYaw }, combat);
                sim.Step();
                if (firedAt >= 0 && i == firedAt + after)
                {
                    Vec3 p = sim.mover.position;
                    for (int k = 0; k < mine.Count; k++) if (Dist(mine[k].position, p) - before[k] >= back) pushed++;
                    break;
                }
            }
            sim.events.Off(N19Events.Gust, onGust);
            sim.events.Off(N20Events.Shockwave, onGust);
            sim.holdRunClock = false;
            return (pushed, mine.Count, fired > 0);
        }

        static double Dist(Vec3 a, Vec3 b)
        {
            double dx = a.x - b.x, dz = a.z - b.z;
            return Math.Sqrt(dx * dx + dz * dz);
        }
    }
}
