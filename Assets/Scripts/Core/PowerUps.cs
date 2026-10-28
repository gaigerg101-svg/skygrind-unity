// NIGHT8 U2-12: kill drops and power-ups. Ports of survival/powerUpMath.js (pure: the style drop: a kill chain or an air
// multikill drops one, then a cooldown; killDropChance: none inside minGap, the base chance x eliteMult for an elite,
// x2 after `rise` s without a drop and x4 after `rise2` (Killing Floor 2's Zed Time shape), capped) and
// survival/powerUps.js: a pool of config.powerUps.pool obelisks where an enemy fell, powerUps.life s to grab (blinking
// over the last blinkFrom s), walk into one (grabRadius, grabHeight): DOUBLE POINTS (x mult combat points for its time),
// OVERCLOCK (weapons stay FRESH for its time: here the style meter's freshness, the arsenal isn't ported), SHOCKWAVE
// (every non-elite enemy dies now, as your kills), STASIS (every enemy frozen for its time; they still take hits),
// REFUND (the next purchase at mult x its price). Every player kill feeds the style drop; an elite kill rolls
// eliteChance; else every kill rolls killDropChance on the run's seeded stream (powerUps.killDrop.seedSalt).
// Randomness: the web's elite roll and a style drop's type use Math.random(); here a second seeded stream per run.
// Events (the web's names): powerUpSpawn {type, position}, powerUpGrab {type, time}, powerUpEnd {type},
// powerUpStyleDrop {reason, position}, powerShockwave {kills, position}.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class PowerUpMath
    {
        public sealed class StyleDropState
        {
            public readonly List<double> kills = new List<double>(), air = new List<double>();
            public double readyAt = double.NegativeInfinity;
        }

        public struct StyleDrop
        {
            public double chainKills, chainWindow, airKills, airWindow, cooldown;
        }

        public struct KillDrop
        {
            public bool enabled;
            public double chance, eliteMult, rise, rise2, minGap, maxChance;
        }

        /// <summary>styleDropOnKill: a player kill at `t` s; "chain" or "air" when it earns a drop (windows clear, the cooldown starts), else null.</summary>
        public static string StyleDropOnKill(StyleDropState state, double t, bool airborne, StyleDrop c)
        {
            while (state.kills.Count > 0 && t - state.kills[0] > c.chainWindow) state.kills.RemoveAt(0);
            state.kills.Add(t);
            if (airborne)
            {
                while (state.air.Count > 0 && t - state.air[0] > c.airWindow) state.air.RemoveAt(0);
                state.air.Add(t);
            }
            else state.air.Clear();
            if (t < state.readyAt) return null;
            string reason = state.kills.Count >= c.chainKills ? "chain" : state.air.Count >= c.airKills ? "air" : null;
            if (reason == null) return null;
            state.kills.Clear();
            state.air.Clear();
            state.readyAt = t + c.cooldown;
            return reason;
        }

        /// <summary>killDropChance: a kill's drop chance `sinceDrop` fixed-step s after the last drop of any kind.</summary>
        public static double KillDropChance(double sinceDrop, KillDrop k, bool elite = false, double mult = 1)
        {
            if (!k.enabled || sinceDrop < k.minGap) return 0;
            double ramp = sinceDrop >= k.rise2 ? 4 : sinceDrop >= k.rise ? 2 : 1;
            return Math.Min(k.maxChance, k.chance * (elite ? k.eliteMult : 1) * mult * ramp);
        }
    }

    public sealed class PowerUpEvent
    {
        public string type, reason;
        public Vec3 position;
        public double time;
        public int kills;
    }

    public sealed class PowerUps : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: every drop slot, the timed effects (their effects on the economy's boost, the
        // arsenal's OVERCLOCK and the husks' STASIS are those systems' fields), a pending REFUND, the style drop's
        // windows, the clock and the pity ramp's last drop, both streams, the counters.
        public void Save(StateWriter w)
        {
            var active = new List<bool>(drops.Length);
            var types = new List<string>(drops.Length);
            var steps = new List<int>(drops.Length);
            var nums = new List<double>(drops.Length * 4);
            foreach (Drop d in drops)
            {
                active.Add(d.active);
                types.Add(d.type);
                steps.Add(d.steps);
                nums.AddRange(new[] { d.position.x, d.position.y, d.position.z, d.age });
            }
            w.Bools("dropActive", active);
            w.Strings("dropType", types);
            w.Ints("dropSteps", steps);
            w.Doubles("dropNums", nums);
            w.SI("timers", timers);
            w.B("refund", refund);
            w.Doubles("styleKills", styleDrop.kills);
            w.Doubles("styleAir", styleDrop.air);
            w.D("styleReadyAt", styleDrop.readyAt);
            w.I("clock", clock);
            w.I("lastDropAt", lastDropAt);
            w.Rng("killRng", killRng);
            w.Rng("rng", rng);
            w.Ints("counters", new[] { spawned, grabbed, expired, shockwaveKills, styleDrops, killDrops });
        }

        public void Load(StateReader r)
        {
            List<bool> active = r.Bools("dropActive");
            List<string> types = r.Strings("dropType");
            List<int> steps = r.Ints("dropSteps"), counters = r.Ints("counters");
            List<double> nums = r.Doubles("dropNums");
            if (active.Count != drops.Length || types.Count != drops.Length || steps.Count != drops.Length || nums.Count != drops.Length * 4 || counters.Count != 6)
                throw new StateException("snapshot: powerUps: the drop pool's size differs");
            var sd = new PowerUpMath.StyleDropState { readyAt = r.D("styleReadyAt") };
            sd.kills.AddRange(r.Doubles("styleKills"));
            sd.air.AddRange(r.Doubles("styleAir"));
            for (int i = 0; i < drops.Length; i++)
            {
                Drop d = drops[i];
                d.active = active[i];
                d.type = types[i];
                d.steps = steps[i];
                d.position = new Vec3(nums[i * 4], nums[i * 4 + 1], nums[i * 4 + 2]);
                d.age = nums[i * 4 + 3];
            }
            r.SI("timers", timers);
            refund = r.B("refund");
            styleDrop = sd;
            clock = r.Int("clock");
            lastDropAt = r.Int("lastDropAt");
            killRng = r.Rng("killRng");
            rng = r.Rng("rng");
            spawned = counters[0];
            grabbed = counters[1];
            expired = counters[2];
            shockwaveKills = counters[3];
            styleDrops = counters[4];
            killDrops = counters[5];
        }

        public static readonly string[] Types = { "double", "overclock", "shockwave", "stasis", "refund" }; // POWER_UPS

        public sealed class Drop
        {
            public bool active;
            public string type;
            public Vec3 position;
            public int steps;
            public double age; // s (visual)
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly double simHz, grabRadius, grabHeight;
        readonly int lifeSteps;
        readonly HashSet<string> eliteKinds = new HashSet<string>();
        readonly double eliteChance;
        readonly PowerUpMath.StyleDrop styleDropCfg;
        readonly PowerUpMath.KillDrop killDropCfg;
        readonly uint seedSalt;
        public readonly Drop[] drops;
        public readonly Dictionary<string, int> timers = new Dictionary<string, int>(); // the timed ones: steps left
        public bool refund;
        PowerUpMath.StyleDropState styleDrop = new PowerUpMath.StyleDropState();
        int clock, lastDropAt;
        Mulberry32 killRng = new Mulberry32(1), rng = new Mulberry32(2);
        public int spawned, grabbed, expired, shockwaveKills, styleDrops, killDrops;

        // The owner's hooks
        public Func<Vec3> playerPos = () => default;
        public Func<bool> playerGrounded = () => true;
        public Func<bool> isLive = () => true;
        public Func<Vec3, double> floorUnder = p => p.y;
        public Economy economy;
        public Action<Enemy, double, Vec3> kill = (e, d, at) => { }; // SHOCKWAVE's damage (the player's loadout)
        public EnemyRegistry enemies;
        public Action<bool> setStasis = _ => { }; // freeze / thaw every regular enemy
        public Action<bool> setOverclock = _ => { };
        public Func<double> dropMult = () => 1; // SCAVENGER (main.js getDropMult: mods.dropMult())

        public PowerUps(GameConfig cfg, EventBus events)
        {
            this.cfg = cfg;
            this.events = events;
            simHz = cfg.movement.loop.simHz;
            grabRadius = cfg.Num("powerUps.grabRadius");
            grabHeight = cfg.Num("powerUps.grabHeight");
            lifeSteps = cfg.Steps("powerUps.life");
            foreach (string k in cfg.Strs("powerUps.eliteKinds")) eliteKinds.Add(k);
            eliteChance = cfg.Num("powerUps.eliteChance");
            styleDropCfg = new PowerUpMath.StyleDrop
            {
                chainKills = cfg.Num("powerUps.styleDrop.chainKills"), chainWindow = cfg.Num("powerUps.styleDrop.chainWindow"), airKills = cfg.Num("powerUps.styleDrop.airKills"),
                airWindow = cfg.Num("powerUps.styleDrop.airWindow"), cooldown = cfg.Num("powerUps.styleDrop.cooldown"),
            };
            killDropCfg = new PowerUpMath.KillDrop
            {
                enabled = cfg.Bool("powerUps.killDrop.enabled"), chance = cfg.Num("powerUps.killDrop.chance"), eliteMult = cfg.Num("powerUps.killDrop.eliteMult"),
                rise = cfg.Num("powerUps.killDrop.rise"), rise2 = cfg.Num("powerUps.killDrop.rise2"), minGap = cfg.Num("powerUps.killDrop.minGap"),
                maxChance = cfg.Num("powerUps.killDrop.maxChance"),
            };
            seedSalt = (uint)cfg.Num("powerUps.killDrop.seedSalt");
            drops = new Drop[cfg.Int("powerUps.pool")];
            for (int i = 0; i < drops.Length; i++) drops[i] = new Drop();
            events.On<KillEvent>("kill", OnKill);
            events.On("pointsSpent", _ =>
            {
                if (!refund) return;
                refund = false; // the discount went into that purchase
                economy?.SetDiscount(1);
                events.Emit("powerUpEnd", new PowerUpEvent { type = "refund" });
            });
        }

        void OnKill(KillEvent e)
        {
            if (e.owner != "player" || !isLive()) return;
            // Every kill counts toward the style drop; an elite rolls its own chance first.
            string reason = PowerUpMath.StyleDropOnKill(styleDrop, clock / simHz, !playerGrounded(), styleDropCfg);
            bool elite = eliteKinds.Contains(e.kind);
            if (elite && rng.Next() < eliteChance) Spawn(e.position);
            else if (reason != null)
            {
                styleDrops++;
                Spawn(e.position);
                events.Emit("powerUpStyleDrop", new PowerUpEvent { reason = reason, position = e.position });
            }
            else
            {
                // NIGHT6 B4: every kill rolls (the run's seeded stream: the drop and its type), the chance rising while none drops.
                double chance = PowerUpMath.KillDropChance((clock - lastDropAt) / simHz, killDropCfg, elite, dropMult());
                if (chance > 0 && killRng.Next() < chance)
                {
                    killDrops++;
                    Spawn(e.position, Types[(int)Math.Floor(killRng.Next() * Types.Length)]);
                }
            }
        }

        /// <summary>Drop `type` (from the run's stream when null) at a point, settled on the floor under it.</summary>
        public Drop Spawn(Vec3 at, string type = null)
        {
            Drop d = null;
            foreach (Drop x in drops) if (!x.active) { d = x; break; }
            if (d == null)
            {
                d = drops[0];
                foreach (Drop x in drops) if (x.steps < d.steps) d = x;
            }
            d.type = type ?? Types[(int)Math.Floor(rng.Next() * Types.Length)];
            d.active = true;
            d.steps = lifeSteps;
            d.age = 0;
            d.position = new Vec3(at.x, floorUnder(at), at.z);
            spawned++;
            lastDropAt = clock;
            events.Emit("powerUpSpawn", new PowerUpEvent { type = d.type, position = d.position });
            return d;
        }

        void Grab(Drop d)
        {
            d.active = false;
            grabbed++;
            string type = d.type;
            double time = cfg.Has($"powerUps.types.{type}.time") ? cfg.Num($"powerUps.types.{type}.time") : 0;
            if (type == "shockwave") Shockwave();
            else if (type == "refund")
            {
                refund = true;
                economy?.SetDiscount(cfg.Num("powerUps.types.refund.mult"));
            }
            else
            {
                timers[type] = MovementMath.StepsFor(time, 1 / simHz);
                Apply(type, true);
            }
            events.Emit("powerUpGrab", new PowerUpEvent { type = type, time = time });
        }

        void Apply(string type, bool on)
        {
            if (type == "double") economy?.SetBoost("double", on ? cfg.Num("powerUps.types.double.mult") : 1);
            else if (type == "overclock") setOverclock(on);
            else if (type == "stasis") setStasis(on);
        }

        /// <summary>Every non-elite enemy dies (points and kills as yours).</summary>
        void Shockwave()
        {
            Vec3 p = playerPos();
            var doomed = new List<Enemy>();
            if (enemies != null) foreach (Enemy e in enemies.all) if (e.alive && !eliteKinds.Contains(e.kind)) doomed.Add(e);
            double damage = cfg.Num("powerUps.types.shockwave.damage");
            foreach (Enemy e in doomed)
            {
                kill(e, damage, p);
                shockwaveKills++;
            }
            events.Emit("powerShockwave", new PowerUpEvent { kills = doomed.Count, position = p });
        }

        /// <summary>A new run's kill-drop stream (powerUps.killDrop.seedSalt) and the other rolls' stream; the clock rises from now.</summary>
        public void SeedRun(uint seed)
        {
            killRng = new Mulberry32((seed ^ seedSalt) >> 0);
            rng = new Mulberry32((seed ^ 0x9e37u) >> 0);
            lastDropAt = clock;
        }

        /// <summary>A new run: no drops, no effects.</summary>
        public void Clear()
        {
            foreach (Drop d in drops) d.active = false;
            foreach (string type in Types)
            {
                if (timers.TryGetValue(type, out int left) && left > 0) Apply(type, false);
            }
            timers.Clear();
            setStasis(false);
            if (refund) economy?.SetDiscount(1);
            refund = false;
            styleDrop = new PowerUpMath.StyleDropState();
        }

        /// <summary>The running effects for the HUD (type, s left; REFUND: -1, until the next buy).</summary>
        public double Left(string type) => type == "refund" ? (refund ? -1 : 0) : timers.TryGetValue(type, out int s) ? s / simHz : 0;

        /// <summary>One fixed step: drops age out or are grabbed; the timed effects run out.</summary>
        public void Step()
        {
            clock++;
            Vec3 p = playerPos();
            foreach (Drop d in drops)
            {
                if (!d.active) continue;
                if (--d.steps <= 0)
                {
                    d.active = false;
                    expired++;
                    continue;
                }
                if (JsMath.Hypot(p.x - d.position.x, p.z - d.position.z) <= grabRadius && Math.Abs(p.y - d.position.y) < grabHeight) Grab(d);
            }
            foreach (string type in Types)
            {
                if (!timers.TryGetValue(type, out int left) || left <= 0) continue;
                timers[type] = --left;
                if (left == 0)
                {
                    Apply(type, false);
                    events.Emit("powerUpEnd", new PowerUpEvent { type = type });
                }
            }
        }
    }
}
