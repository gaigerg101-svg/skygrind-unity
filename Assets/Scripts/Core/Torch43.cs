// NIGHT43 builder W1 torch43 (N43Blocks.torch43, design.n43.torch43; GG 2026-10-16 G20: "The flamethrower currently feels weak"; combat-web.md
// C11: DOOM Eternal's Flame Belch works because burning enemies change state; C12 (d): "a ramp-up: the first 0.2 s of fire grows reach ... from
// 50 % to 100 %, so holding it feels like building power"). The sim's half of the TORCH (Guns/Torch.cs Flame), with the row on:
//   - IGNITE: a body a flame tick hits (and that lives) burns for burnS s (a new hit refreshes it); the first ignite of a burn stuns it
//     igniteStaggerS (it flinches) and emits `burn43` (Torch43.Event, payload Burn43Event: the body, where, the seconds);
//   - BURN: every burnTick s a burning body takes burnDps x burnTick (Combat.Damage, weapon "burn", shock = true: no freeze, no momentum;
//     its own shot id); a burn ends after burnS without a new flame, or when the body dies;
//   - THE RAMP: the flame's reach grows from rampFrom x to 1 x over the first rampS s of a continuous spray (a pause of more than 1.5 flame
//     ticks starts it again).
// The views read Loadout.Burning43(enemy) (bool) and Loadout.BurnLeft43(enemy) (0..1 of the burn left); F1 / A1 draw the burning look.
// Off (torch43 null or not on): Torch.cs's flame as NIGHT42 has it, bit for bit (Torch43Reach is exactly 1, nothing ignites).
namespace Skygrind.Core
{
    /// <summary>torch43's numbers, the burning bodies (a fixed pool: no allocation per step) and the run's counters.</summary>
    public sealed class Torch43
    {
        /// <summary>A body caught fire (payload Burn43Event, reused). GG's sound hook too (CLAUDE.md rule 7).</summary>
        public const string Event = "burn43";

        public bool on;
        public double burnDps, burnTickS, igniteStaggerS, rampFrom;
        public int burnSteps, tickSteps, rampSteps;

        const int Max = 48;
        internal readonly Enemy[] who = new Enemy[Max];
        internal readonly int[] left = new int[Max], tickLeft = new int[Max];
        internal int count;
        internal int now, lastFlame = -1000000, rampStart;

        /// <summary>This run's counters (the harness reads them).</summary>
        public int ignites, burnTicks, burnKills;
        public double burnDamage;

        public readonly Burn43Event payload = new Burn43Event();

        public void Set(bool on, GameConfig cfg)
        {
            this.on = on;
            for (int i = 0; i < Max; i++) who[i] = null;
            count = 0;
            now = 0;
            lastFlame = -1000000;
            rampStart = 0;
            ignites = burnTicks = burnKills = 0;
            burnDamage = 0;
            if (!on) return;
            const string r = "design.n43.torch43.";
            double dt = cfg.Dt;
            burnSteps = MovementMath.StepsFor(cfg.Num(r + "burnS"), dt);
            burnDps = cfg.Num(r + "burnDps");
            burnTickS = cfg.Num(r + "burnTickS");
            tickSteps = System.Math.Max(1, MovementMath.StepsFor(burnTickS, dt));
            igniteStaggerS = cfg.Num(r + "igniteStaggerS");
            rampSteps = MovementMath.StepsFor(cfg.Num(r + "rampS"), dt);
            rampFrom = cfg.Num(r + "rampFrom");
        }

        internal int Slot(Enemy e)
        {
            for (int i = 0; i < count; i++) if (who[i] == e) return i;
            return -1;
        }

        internal void RemoveAt(int i)
        {
            count--;
            who[i] = who[count];
            left[i] = left[count];
            tickLeft[i] = tickLeft[count];
            who[count] = null;
        }
    }

    /// <summary>A body caught fire: its id, where (its aim point), the burn's seconds.</summary>
    public sealed class Burn43Event
    {
        public string owner = "player";
        public int enemy;
        public Vec3 position;
        public double seconds;
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT43 W1 torch43 (null or not on: off; the sim sets it, SurvivalSim.N43.Weapons.cs).</summary>
        public Torch43 torch43;

        bool T43 => torch43 != null && torch43.on;

        /// <summary>torch43: is this body burning now? (the views; false with the row off)</summary>
        public bool Burning43(Enemy e) => T43 && e != null && torch43.Slot(e) >= 0;

        /// <summary>torch43: the share of this body's burn left, 0..1 (0: not burning or the row off).</summary>
        public double BurnLeft43(Enemy e)
        {
            if (!T43 || e == null) return 0;
            int i = torch43.Slot(e);
            return i < 0 || torch43.burnSteps <= 0 ? 0 : (double)torch43.left[i] / torch43.burnSteps;
        }

        /// <summary>torch43, in Flame's range: the ramp's reach factor for this flame tick (exactly 1 with the row off).</summary>
        double Torch43Reach()
        {
            if (!T43) return 1;
            Torch43 t = torch43;
            if (t.now - t.lastFlame > MovementMath.StepsFor(tTick * 1.5, dt26)) t.rampStart = t.now; // a new spray
            t.lastFlame = t.now;
            if (t.rampSteps <= 0) return 1;
            double k = System.Math.Min(1.0, (double)(t.now - t.rampStart) / t.rampSteps);
            return t.rampFrom + (1 - t.rampFrom) * k;
        }

        /// <summary>torch43, after a flame tick's Damage on a body: it burns (a new burn flinches it and emits `burn43`).</summary>
        void Torch43Ignite(Enemy e)
        {
            if (!T43 || !e.alive) return;
            Torch43 t = torch43;
            int i = t.Slot(e);
            if (i >= 0)
            {
                t.left[i] = t.burnSteps; // refreshed
                return;
            }
            if (t.count >= t.who.Length) return;
            i = t.count++;
            t.who[i] = e;
            t.left[i] = t.burnSteps;
            t.tickLeft[i] = t.tickSteps;
            t.ignites++;
            e.Stun(t.igniteStaggerS, dt26);
            Burn43Event ev = t.payload;
            ev.owner = owner;
            ev.enemy = e.id;
            ev.position = e.AimPoint;
            ev.seconds = t.burnSteps * dt26;
            events.Emit(Torch43.Event, ev);
        }

        /// <summary>torch43, every sim step (SurvivalSim.N43WeaponsStep): the clock, the burns' ticks and ends.</summary>
        public void Torch43Step()
        {
            if (!T43) return;
            Torch43 t = torch43;
            t.now++;
            int shot = -1;
            for (int i = t.count - 1; i >= 0; i--)
            {
                Enemy e = t.who[i];
                if (e == null || !e.alive)
                {
                    t.RemoveAt(i);
                    continue;
                }
                if (--t.tickLeft[i] <= 0)
                {
                    t.tickLeft[i] = t.tickSteps;
                    if (shot < 0) shot = NewShot();
                    double amount = t.burnDps * t.burnTickS;
                    t.burnTicks++;
                    t.burnDamage += amount;
                    if (Damage(e, amount, false, "burn", e.AimPoint, false, tPointScale, shot, true)) t.burnKills++;
                }
                if (--t.left[i] <= 0 || !e.alive) t.RemoveAt(i);
            }
        }
    }
}
