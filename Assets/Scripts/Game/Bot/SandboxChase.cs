// NIGHT19 U3-33.2 (the lead owns this file; builders never edit it): THE CHASE. Can a player who only runs survive? The
// Survival map, the director's own waves from round `round` on (its spawn rules and pacing as configured: the run skips to
// the round, DebugSkipTo, the first break, then the wave), real HP, a 300 s limit, seeds 1..n. Three bots:
// - KITE: never fires or punches. It circles the open floor (a 20 m loop around Sandbox.OpenSpot, steered every 6 steps
//   by the collider's clearance and away from the enemies within 25 m: KiteInput.Steer) holding the sprint (the
//   boost key held after a press, NIGHT18's sprint, and the sprint key, the NIGHT19 rule when it's on), boosts along its
//   heading when an enemy is within 6 m, slides every 2.5 s, and uses the gust (the KESTREL's LT, looking at the nearest
//   enemy for that step) when an enemy is within 4 m and the gust block is on. NIGHT20 (the lead, before the baseline):
//   with design.shockwave live the KITE holds the THUNDERCLAP (both sides of `--gust off`: the same gun) and presses its
//   LT (the shockwave; inside its cooldown nothing fires) under the same rule; `--gust off` turns off both LT tools.
// - STAND: the AUTOPILOT with its movement zeroed (it fires, never moves).
// - FIGHT: the AUTOPILOT as it is.
// The measures (S8, S9, S11): the survival time from the round's start to the first death (or the limit), the damage per
// minute, the time to the first hit, the hits by source, the time on an empty bar.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class SandboxChase
    {
        public sealed class Result
        {
            public string bot;
            public int round;
            public uint seed;
            public double survival, hurt, dpm, firstHit = -1, emptyBar;
            public bool died;
            public readonly SortedDictionary<string, int> hitsBy = new SortedDictionary<string, int>(StringComparer.Ordinal);
        }

        /// <summary>KITE: the scripted runner (an IPlayerInput the sim samples once a step).</summary>
        sealed class KiteInput : IPlayerInput
        {
            readonly SurvivalSim sim;
            readonly Vec3 center;
            readonly double radius;
            double yaw, pitch;
            int step;

            /// <summary>NIGHT20: the KITE holds the THUNDERCLAP (design.shockwave in the config and not forced off).</summary>
            public bool holdsShotgun;

            public KiteInput(SurvivalSim sim, Vec3 center, double radius)
            {
                this.sim = sim;
                this.center = center;
                this.radius = radius;
            }

            public double Yaw => yaw;
            public double Pitch => pitch;
            public bool Captured => true;

            public void SetLook(double y, double p)
            {
                yaw = y;
                pitch = p;
            }

            Enemy Nearest(out double dist)
            {
                Enemy best = null;
                dist = double.PositiveInfinity;
                Vec3 me = sim.mover.position;
                foreach (Enemy e in sim.enemies.all)
                {
                    if (!e.alive) continue;
                    double dx = e.position.x - me.x, dz = e.position.z - me.z;
                    double d = Math.Sqrt(dx * dx + dz * dz);
                    if (d < dist)
                    {
                        dist = d;
                        best = e;
                    }
                }
                return best;
            }

            double heading = double.NaN;

            /// <summary>The heading: of 16 (22.5 deg apart) with at least 3 m clear, the best by 1.0 x the flee direction
            /// (away from every enemy within 25 m, weighted 1 / d^2), 0.6 x the tangent of a 20 m loop around the open spot
            /// (counter-clockwise, pulled back to the loop), 0.3 x the last heading, + 0.03 x the clearance (at most 20 m).</summary>
            void Steer(Vec3 me)
            {
                double fx = 0, fz = 0;
                foreach (Enemy e in sim.enemies.all)
                {
                    if (!e.alive) continue;
                    double dx = me.x - e.position.x, dz = me.z - e.position.z;
                    double d2 = dx * dx + dz * dz;
                    if (d2 > 25 * 25 || d2 < 1e-6) continue;
                    fx += dx / d2;
                    fz += dz / d2;
                }
                double fl = Math.Sqrt(fx * fx + fz * fz);
                if (fl > 0)
                {
                    fx /= fl;
                    fz /= fl;
                }
                double ox = me.x - center.x, oz = me.z - center.z;
                double r = Math.Max(1e-6, Math.Sqrt(ox * ox + oz * oz));
                double pull = Math.Max(-1, Math.Min(1, (r - radius) / radius));
                double tx = -oz / r - ox / r * pull, tz = ox / r - oz / r * pull;
                double tl = Math.Max(1e-6, Math.Sqrt(tx * tx + tz * tz));
                tx /= tl;
                tz /= tl;
                double best = double.NegativeInfinity, pick = double.IsNaN(heading) ? JsMath.Atan2(-tx, -tz) : heading;
                for (int h = 0; h < 16; h++)
                {
                    double y = h * Math.PI / 8;
                    double clear = Sandbox.Clear(sim, me, y);
                    if (clear < 3) continue;
                    double hx = -JsMath.Sin(y), hz = -JsMath.Cos(y);
                    double score = 1.0 * (hx * fx + hz * fz) + 0.6 * (hx * tx + hz * tz) + 0.03 * Math.Min(20, clear);
                    if (!double.IsNaN(heading)) score += 0.3 * (hx * -JsMath.Sin(heading) + hz * -JsMath.Cos(heading));
                    if (score > best)
                    {
                        best = score;
                        pick = y;
                    }
                }
                heading = pick;
                yaw = heading;
            }

            public InputFrame SampleMove(bool active)
            {
                step++;
                Vec3 me = sim.mover.position;
                if (step % 6 == 1) Steer(me); // a decision every 6 steps (20 a second), the heading held between
                pitch = 0;
                Nearest(out double near);
                int t = step;
                bool boost = near <= 6 && t % 30 == 0; // a fresh press at most 4 a second while something is close
                var f = new InputFrame
                {
                    moveZ = 1, yaw = yaw, pitch = pitch,
                    dash = new Button { held = true, pressed = boost || t == 1 }, // held: the boost key held after a press sprints (NIGHT18)
                    sprint = new Button { held = true, pressed = t == 1 },
                    slide = new Button { pressed = t % 300 == 150, held = t % 300 >= 150 && t % 300 < 198 },
                };
                return f;
            }

            public CombatInput SampleCombat(bool active)
            {
                var c = new CombatInput { weapon = -1 };
                if (holdsShotgun && sim.loadout.Weapon != "shotgun")
                {
                    c.weapon = sim.loadout.owned.IndexOf("shotgun"); // NIGHT20: take the THUNDERCLAP (the shockwave's gun)
                    return c;
                }
                // NIGHT20 round 2 (a judge fix, logged): only while the shockwave can fire (it never turns into the enemy inside the cooldown)
                bool tool = sim.n25.shockwaveOff ? false : sim.n20.shockwave ? sim.loadout.Weapon == "shotgun" && sim.loadout.shockwave != null && sim.loadout.shockwave.Ready : sim.n19.gust && sim.loadout.Weapon == "revolver";
                if (!tool) return c;
                Enemy e = Nearest(out double near);
                if (e == null || near > 4) return c;
                Sandbox.Look(sim.Eye, e.AimPoint, out double y, out double p);
                yaw = y; // the gust pushes you away from what you look at: look at it for this step
                pitch = p;
                c.altFire = new Button { held = true, pressed = true };
                return c;
            }
        }

        /// <summary>STAND: the AUTOPILOT's look and combat, its movement zeroed.</summary>
        sealed class StandInput : IPlayerInput
        {
            readonly AutopilotInput ap;
            public StandInput(AutopilotInput ap) => this.ap = ap;
            public double Yaw => ap.Yaw;
            public double Pitch => ap.Pitch;
            public bool Captured => ap.Captured;
            public void SetLook(double y, double p) => ap.SetLook(y, p);

            public InputFrame SampleMove(bool active)
            {
                InputFrame f = ap.SampleMove(active);
                f.moveX = f.moveZ = 0;
                f.jump = f.dash = f.slide = f.grapple = f.reel = f.sprint = default;
                return f;
            }

            public CombatInput SampleCombat(bool active) => ap.SampleCombat(active);
        }

        public const double Limit = 300;

        /// <summary>`chase --gust off`: the run's NIGHT19 blocks as configured but the gust's (S10: the KITE with and without it).</summary>
        public static bool GustOff;

        /// <summary>NIGHT21 U3-35.6 (`chase --twist deadline`): the round under test is the DEADLINE twist (design.n21.deadline taken on for the
        /// run, the twist forced on that round); null: the round as the design draws it.</summary>
        public static string Twist;

        /// <summary>The hand check's log (null: none): the round's start, every hit on the player (time, amount, HP after,
        /// source) and the death.</summary>
        public static Action<string> Trace;

        /// <summary>One run: `bot` (kite | stand | fight) from round `round`, seed `seed`, at most Limit s from the round's start.</summary>
        public static Result Run(GameConfig cfg, SurvivalMap map, string bot, int round, uint seed) => Run(cfg, map, bot, round, seed, null);

        /// <summary>NIGHT20 (THE CLIMB's perch test): the same run with the player respawned at `at` (null: the map's spawn)
        /// after the skip to the round.</summary>
        public static Result Run(GameConfig cfg, SurvivalMap map, string bot, int round, uint seed, Vec3? at)
        {
            if (BotNav.CacheDir == null) BotNav.CacheDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "skygrind-botnav");
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            if (GustOff)
            {
                N19Blocks b = N19Blocks.ForceOff ? N19Blocks.Off : N19Blocks.From(cfg);
                b.gust = false;
                sim.n19Override = b;
                N20Blocks b20 = N20Blocks.ForceOff ? N20Blocks.Off : N20Blocks.From(cfg); // NIGHT20: and the shockwave
                b20.shockwave = false;
                sim.n20Override = b20;
            }
            if (Twist == "deadline")
            {
                N21Blocks b21 = N21Blocks.ForceOff ? N21Blocks.Off : N21Blocks.From(cfg);
                b21.deadline = !N21Blocks.ForceOff;
                sim.n21Override = b21;
            }
            var r = new Result { bot = bot, round = round, seed = seed };
            AutopilotInput ap = null;
            Vec3? kiteSpot = null;
            KiteInput kite = null;
            if (bot == "kite")
            {
                var (spot, _, clear) = Sandbox.OpenSpot(sim);
                kiteSpot = spot;
                kite = new KiteInput(sim, spot, 20) { holdsShotgun = !N20Blocks.ForceOff && N20Blocks.From(cfg).shockwave };
                sim.UseInput(kite); // a 20 m loop around the open spot (Steer keeps it off the walls)
                sim.noRecords = sim.noLab = true;
                sim.metaSilent = true;
                sim.assistModeFor = _ => "off";
            }
            else
            {
                ap = new AutopilotInput(sim, "normal", seed);
                sim.UseInput(bot == "stand" ? new StandInput(ap) : (IPlayerInput)ap);
            }
            double t0 = -1, now = 0, deathAt = -1;
            bool dead = false;
            sim.events.On<RoundEvent>("roundBegin", e =>
            {
                if (t0 < 0 && e.number >= round)
                {
                    t0 = now;
                    Trace?.Invoke($"t 0.000 s: round {e.number} begins (HP {sim.health.hp:0.##} / {sim.health.max:0.##})");
                }
            });
            sim.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player" || t0 < 0 || dead) return;
                r.hurt += e.amount;
                if (r.firstHit < 0) r.firstHit = now - t0;
                string k = e.source ?? "?";
                r.hitsBy[k] = (r.hitsBy.TryGetValue(k, out int n) ? n : 0) + 1;
                Trace?.Invoke($"t {now - t0:0.000} s: hurt {e.amount:0.##} by {k}, HP {e.hp:0.##}");
            });
            sim.events.On<DeathEvent>("death", e =>
            {
                if (e.owner == "player" && t0 >= 0)
                {
                    dead = true;
                    deathAt = now - t0;
                    Trace?.Invoke($"t {now - t0:0.000} s: DEATH by {e.source}");
                }
            });
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            if (Twist == "deadline" && sim.specials != null) sim.specials.ForceTwist(round, Deadline21.TwistId); // NIGHT21: the round under test
            sim.zones.OpenAll(); // the whole map, as the bench plays it (a round-10 player has opened gates; the judge opens all)
            if (kiteSpot.HasValue) sim.Respawn(kiteSpot.Value, 0);
            if (kite != null && kite.holdsShotgun) sim.loadout.AddWeapon("shotgun"); // NIGHT20: the THUNDERCLAP owned (the KITE takes it on its first step)
            sim.run.DebugSkipTo(round);
            if (at.HasValue) sim.Respawn(at.Value, 0); // NIGHT20: THE CLIMB's perch
            double dt = sim.step;
            int max = MovementMath.StepsFor(Limit + cfg.Num("survival.firstBreakTime") + 1, dt);
            int emptySteps = 0;
            for (int i = 0; i < max && !dead; i++)
            {
                if (ap != null) AutopilotInput.TakeDraft(sim);
                sim.Step();
                now += dt;
                if (t0 >= 0)
                {
                    if (sim.mover.stamina.rules.Any && sim.mover.stamina.bar <= 1e-9) emptySteps++;
                    if (now - t0 >= Limit) break;
                }
                if (sim.flow == Flow.Results) break;
            }
            r.died = dead;
            r.survival = t0 < 0 ? 0 : dead ? deathAt : Math.Min(Limit, now - t0); // the death's own time (the hit list's clock)
            r.dpm = r.survival > 0 ? r.hurt / (r.survival / 60) : 0;
            r.emptyBar = emptySteps * dt;
            ap?.Detach();
            return r;
        }
    }
}
