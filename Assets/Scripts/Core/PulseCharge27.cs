// NIGHT27 (builder W; design.n27.pulse27): the PULSE GAUNTLET, slower and heavier. Unity-only (the web had no charge).
// GG 2026-10-09: "the pulse needs to attack much slower ... after you charge it, after say killing five enemies, then you unlock the left trigger".
// - The click swing (Loadout.StepFire path, Combat.cs) has a 2.5 s cooldown (300 steps) and hits for gauntlet damage x damageMult.
// - Five pips on the glove: a KILL of yours fills one (any weapon; hits never do; PulseCharge27.AddKill is the entry other systems call, the boss
//   sentries and the GLACIER). The store is capped at one full glove (5 pips). The pips are spent by the blast.
// - LT with the gauntlet in hand: the blast, ONLY with a full glove and the blast cooldown (15 s or longer) run out. Then the glove empties, the bodies in
//   front take a hit and a push, and a 0.4 s self-boost adds speed along the aim (equal parts over the boost's steps, through Mover.Impulse).
//   LT not ready: nothing is spent and a "pulse27Denied" event says why (the HUD shows it: UiWeapons.N27.cs).
// - Whole steps everywhere; everything resets at a run's start. Off (design.n27.pulse27 off, --off27): NIGHT26's pulseLT exactly (this object is null).
// Events (new names): pulse27Pip {pips, kills}, pulse27Blast {hits, boostSteps}, pulse27Denied {why: pips | cooldown}.
using System;

namespace Skygrind.Core
{
    /// <summary>design.n27.pulse27's numbers.</summary>
    public sealed class Pulse27Params
    {
        public double swingSeconds, damageMult, blastCooldownSeconds, boostSeconds, boostSpeed, blastRange, blastRadius, blastDamageMult;
        public int pips;

        public static Pulse27Params From(GameConfig cfg)
        {
            const string p = "design.n27.pulse27.";
            return new Pulse27Params
            {
                swingSeconds = cfg.Num(p + "swingSeconds"),
                damageMult = cfg.Num(p + "damageMult"),
                pips = cfg.Int(p + "pips"),
                blastCooldownSeconds = cfg.Num(p + "blastCooldownSeconds"),
                boostSeconds = cfg.Num(p + "boostSeconds"),
                boostSpeed = cfg.Num(p + "boostSpeed"),
                blastRange = cfg.Num(p + "blastRange"),
                blastRadius = cfg.Num(p + "blastRadius"),
                blastDamageMult = cfg.Num(p + "blastDamageMult"),
            };
        }
    }

    public sealed class Pulse27Event
    {
        public int pips, kills, hits, boostSteps;
        public string why;
    }

    /// <summary>The glove's pips, the blast's cooldown and the boost (the rule; the blast's geometry is Loadout.StepPulse27 below).</summary>
    public sealed partial class PulseCharge27 // NIGHT47 F: partial (pulse47's fields: Core/Guns/Blast47.cs)
    {
        public readonly Pulse27Params p;
        readonly EventBus events;
        readonly double dt;
        readonly Pulse27Event ev = new Pulse27Event();

        /// <summary>The row is on for this run (the pips fill, the blast is gated); off: a dead object.</summary>
        public bool active;

        /// <summary>Pips lit now, 0..p.pips.</summary>
        public int pips;

        /// <summary>Steps of the blast's cooldown and of the boost left; the boost's per-step speed and direction.</summary>
        public int cooldownSteps, boostSteps;
        public Vec3 boostDir;
        public double boostPerStep;

        /// <summary>Set while the blast deals its damage: its own kills do not refill the glove.</summary>
        public bool feedLocked;

        /// <summary>The run's counters (the harness reads them).</summary>
        public int kills, blasts, denied, hitsIgnored;

        /// <summary>The last LT refusal ("pips" | "cooldown" | null) and the step it happened (the HUD).</summary>
        public string deniedWhy;
        public int deniedAtStep;

        /// <summary>LT's edge detector (LT pressed this step = held now, not last step).</summary>
        public bool prevLt;

        public int stepNow;

        public PulseCharge27(GameConfig cfg, EventBus events)
        {
            p = Pulse27Params.From(cfg);
            this.events = events;
            dt = cfg.Dt;
            events.On<KillEvent>("kill", OnKill);
            events.On<HitEvent>("hit", OnHit);
        }

        /// <summary>A run's start: an empty glove, no cooldown, no boost.</summary>
        public void Reset(bool on)
        {
            active = on;
            pips = 0;
            cooldownSteps = boostSteps = 0;
            boostDir = default;
            boostPerStep = 0;
            feedLocked = false;
            kills = blasts = denied = hitsIgnored = 0;
            deniedWhy = null;
            deniedAtStep = -100000;
            prevLt = false;
            stepNow = 0;
        }

        /// <summary>NIGHT42 W1 gauntlet42 (N42Blocks.gauntlet42, design.n42.gauntlet42.swingSeconds; set by the sim at a run's start,
        /// SurvivalSim.N42.Weapons.cs): the swing's cycle in seconds while the row is on (GG 2026-10-14: "0.7 seconds faster", 2.5 -> 1.8 s);
        /// 0 = design.n27.pulse27.swingSeconds (the n27 row untouched; off: NIGHT41 bit for bit). Not reset by Reset: the sim sets it each run.</summary>
        public double swingSeconds42;

        /// <summary>The click swing's cooldown in whole steps (2.5 s = 300 steps at 120 Hz; gauntlet42: 1.8 s = 216).</summary>
        public int SwingSteps => MovementMath.StepsFor(swingSeconds42 > 0 ? swingSeconds42 : p.swingSeconds, dt);

        public int BlastCooldownSteps => MovementMath.StepsFor(p.blastCooldownSeconds, dt);

        public bool Full => pips >= p.pips;

        /// <summary>The glove is full and the blast's cooldown has run out: LT blasts.</summary>
        public bool Ready => active && Full && cooldownSteps <= 0;

        /// <summary>The share of the blast's cooldown left, 0..1 (the HUD's ring).</summary>
        public double CooldownShare => cooldownSteps <= 0 ? 0 : Math.Min(1.0, cooldownSteps / (double)BlastCooldownSteps);

        void OnKill(KillEvent e)
        {
            if (!active || feedLocked || e.owner != "player") return;
            AddKill();
        }

        // A hit never fills a pip: counted so the harness can show the hits that were ignored.
        void OnHit(HitEvent e)
        {
            if (active && e.owner == "player" && e.weapon == "gauntlet" && !e.killed) hitsIgnored++;
        }

        /// <summary>One kill fills one pip (capped at one full glove). The entry other systems call (boss sentries, the GLACIER). True: a pip was added.</summary>
        public bool AddKill()
        {
            if (!active) return false;
            kills++;
            if (pips >= p.pips) return false;
            pips++;
            ev.pips = pips;
            ev.kills = kills;
            ev.why = "kill";
            events.Emit("pulse27Pip", ev);
            return true;
        }

        /// <summary>One fixed step: the blast's cooldown and the boost's clock run.</summary>
        public void Step()
        {
            stepNow++;
            if (!active) return;
            if (cooldownSteps > 0) cooldownSteps--;
            if (boostSteps > 0) boostSteps--;
        }

        /// <summary>Spend the glove and start the cooldown and the boost (the Loadout calls it when the blast goes).</summary>
        public void Spend(Vec3 dir)
        {
            pips = 0;
            cooldownSteps = BlastCooldownSteps;
            boostSteps = MovementMath.StepsFor(p.boostSeconds, dt);
            boostDir = dir;
            boostPerStep = p.boostSpeed / boostSteps;
            blasts++;
        }

        public void Deny(string why)
        {
            denied++;
            deniedWhy = why;
            deniedAtStep = stepNow;
            ev.why = why;
            ev.pips = pips;
            events.Emit("pulse27Denied", ev);
        }

        public void Announce(int hits)
        {
            ev.hits = hits;
            ev.boostSteps = boostSteps;
            ev.pips = pips;
            ev.why = "blast";
            events.Emit("pulse27Blast", ev);
        }
    }

    public sealed partial class Loadout
    {
        /// <summary>NIGHT27 pulse27: the glove (null with the row off: NIGHT26's pulseLT).</summary>
        public PulseCharge27 pulse27;

        /// <summary>The run's pulse27 hook: the sim hands the glove over (or null) at a run's start.</summary>
        public bool Pulse27Live => pulse27 != null && pulse27.active;

        /// <summary>One step after the loadout's own step (the sim's N27WeaponsStep): the boost, and LT's press: the blast, or why not.</summary>
        public void StepPulse27(CombatInput input, double dt)
        {
            PulseCharge27 pc = pulse27;
            if (pc == null || !pc.active) return;
            pc.Step();
            if (pc.boostSteps > 0) mover.Impulse(pc.boostDir.x * pc.boostPerStep, pc.boostDir.y * pc.boostPerStep, pc.boostDir.z * pc.boostPerStep);
            bool lt = input.altFire.held;
            bool press = lt && !pc.prevLt;
            pc.prevLt = lt;
            if (!press || Weapon != "gauntlet") return; // another gun's LT is its own (zoom, alt)
            if (rules30 != null && (rules30.gunAction > 0 || rules30.queuedSwitch >= 0)) // NIGHT30 rules: one action at a time; a queued switch goes first
            {
                rules30.droppedFireSteps++;
                if (lt31 != null && lt31.active) Deny31("BUSY"); // NIGHT31 ltAlways: the press says why on the slot (the swing or a switch goes first)
                return;
            }
            if (!pc.Full || pc.cooldownSteps > 0)
            {
                if (lt31 != null && lt31.active)
                {
                    LtParry31(); // NIGHT31 ltAlways (R6): no BLAST yet, or it cools: LT is the parry (Core/Guns/LtAlways31.cs)
                    return;
                }
                pc.Deny(!pc.Full ? "pips" : "cooldown");
                return;
            }
            var (origin, aim) = getAim();
            double len = aim.Length;
            Vec3 dir = len > 1e-6 ? aim / len : aim;
            Pulse27Params p = pc.p;
            pc.Spend(dir);
            // The blast: bodies within blastRadius (their own radius added) of the aim line, out to blastRange, once each: a hit and a push.
            int hits = 0;
            pc.feedLocked = true;
            try
            {
                if (pc.blast47 != null) hits = Blast47(pc, origin, dir, dt); // NIGHT47 F pulse47 (Core/Guns/Blast47.cs); null: the loop below, NIGHT46's
                else
                for (int i = 0; i < enemies.all.Count; i++)
                {
                    Enemy e = enemies.all[i];
                    if (!e.alive) continue;
                    Vec3 rel = e.AimPoint - origin;
                    double t = Vec3.Dot(rel, dir);
                    if (t < 0 || t > p.blastRange) continue;
                    Vec3 off = rel - dir * t;
                    if (off.Length > p.blastRadius + e.Radius + BossMargin34(e)) continue; // NIGHT34 reach34: + the boss margin (Pulse25.cs; 0 off: the same sum)
                    e.Knockback(dir.x, dir.z, gKnock, gKnockTime, dt);
                    Pushed20(e);
                    Damage(e, gDamage * p.blastDamageMult, false, "gauntlet", origin, false);
                    hits++;
                }
            }
            finally { pc.feedLocked = false; }
            pc.Announce(hits);
        }
    }
}
