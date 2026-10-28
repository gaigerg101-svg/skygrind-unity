// NIGHT34 builder B (B1, design.n34.bend34 and its split, bendSplit34): THE GRAPPLE BEND in the sim. The rule is Core/Bend34.cs; this file feeds it
// the step's latches and kills and hands its outputs to the loop (Bend34Share), the view (Bend34Depth01, BendWorldAlpha) and the loadout (the
// instant swap, Loadout.bend34Instant). The hooks (declared in SurvivalSim.N34.cs, called by the lead's Step in SurvivalSim.Run.cs):
// - N34BendStartRun: the numbers once (Bend34Rules), the meter full, no bend, the clock in the mode's whole steps.
// - N34BendBeginStep (the top of Step, after simStep++): the bend's clock advances one step; then n34HoldWorld (split: inside a bend the world block
//   runs on 1 step in N, N = round(1 / scale)) and loadout.bend34Instant (inside a bend, for this step's loadout.Step).
// - N34BendStep (every step, after the NIGHT33 hooks): this step's latch (the Mover's RopeAttach: a node, a wall, an enemy zip the whip bit; the
//   ARCLINE's yank of a small enemy, arclinePull), then this step's player kills (the kill event, style rank at the kill), then the eased share.
//   A kill after N34BendStep in the step (N25CombatStep) counts on the next step.
// Events (new, GG's sound hooks; PORT-STATUS's list): bend34Start {tier "short" | "full", meter, kills 0} at a bend's start and at a SHORT bend's
// upgrade to FULL; bend34Kill {tier, kills (inside this bend so far), extended} per player kill inside a bend; bend34End {tier, kills, meter} when the
// hold ends (the snap back begins).
// THE SPLIT TABLE (NIGHT34.md B1, the lead's SurvivalSim.N34.cs header; proven by Tools/SimHarness/Harness.N34.Bend.cs `bend34` split checks):
//   WORLD (skipped on a held step, `if (!n34HoldWorld)` in Step): core?.Step, N33DevicesMove, brain.Step, N20RoutesStep, projectiles.Step (the player's
//   rockets slow too: DECIDE34 B-3), husks.Step, N19EnemiesStep, N20DirectorStep, N21Deadline / Stamp / GrindStep, N22Directed / DuskStep,
//   N24Campaign / WyrmStep, N26Boss / LevelStep, N27Boss / LevelStep, N29Systems / BossStep, N30BossStep, N31BossStep, N32Balance / Boss / Arena /
//   Span / Finale / CrownStep, N33Ai / Spawns / Boss / DevicesStep, N25Wyrm / LevelStep, brain.Separate, N33AfterCrowd, traps.Step, StepMode.
//   PLAYER (every step): input and the N31 / N32 / N33 grapple frames, mover.Step, health.Tick, pads, assist, loadout.Step, N19 / N20 / N21 arsenal
//   hooks, regen, buyables, READY, N22Trick / HapticsStep, N24FistStep, N26Weapons / SurfaceStep, N27WeaponsStep, N30SystemsStep, N31Grapple /
//   WeaponsStep, N32GrappleStep, N33WhipStep, N33PierceStep, N34BendStep, N25CombatStep, style, powerUps, moments, contracts.
//   One mixed call: N30SystemsStep's Lava30.Step also runs the ENEMIES' burn clocks; Tools/night/n34b-lava-hold-patch.cjs (the lead's) holds that part
//   on a held step (Lava30.holdEnemies34). Left on the player's clock by reading: powerUps.Step (a drop's age), box?.Step (the box's timers).
// Off (the row, --off34 / -skygrindOff34 and every older off switch): nothing here changes a field the sim reads; n34HoldWorld stays false, the
// share 1, the loadout's swap NIGHT33's.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>bend34Start / bend34Kill / bend34End's payload (reused: read it inside the handler).</summary>
    public sealed class Bend34Event
    {
        public string tier;
        public int kills;
        public double meter;
        public bool extended;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT34: design.n34.bend34's numbers (null until a run starts with the row on).</summary>
        public Bend34Rules bend34Rules;

        /// <summary>NIGHT34: the bend's state this run (the view, the reel and the harness read it; Reset at each run start).</summary>
        public readonly Bend34 bend34 = new Bend34();

        /// <summary>NIGHT34: is the bend row on for this run?</summary>
        public bool Bend34On => n34.bend34 && bend34Rules != null;

        /// <summary>NIGHT34: the bend's tier now (None outside a bend; the Out counts as the tier it had).</summary>
        public Bend34Tier Bend34TierNow => Bend34On ? bend34.tier : Bend34Tier.None;

        /// <summary>NIGHT34: the BEND METER 0 .. 1 (1 with the row off: nothing to show).</summary>
        public double Bend34Meter => Bend34On ? bend34.meter : 1;

        /// <summary>NIGHT34: inside a bend (In or Hold) now?</summary>
        public bool Bend34Inside => Bend34On && bend34.Inside;

        /// <summary>NIGHT34 (split): the world's render interpolation alpha for this frame. The world steps 1 in N inside a split bend, so the enemies,
        /// projectiles and devices must be interpolated between their last two WORLD steps' positions with this alpha (frameAlpha = the loop's usual
        /// accumulator / dt alpha). Outside a split bend it is frameAlpha (the world steps every step).</summary>
        public double BendWorldAlpha(double frameAlpha)
        {
            if (!Bend34On) return frameAlpha;
            int n = bend34.WorldN;
            return n <= 1 ? frameAlpha : (bend34.WorldPhase + frameAlpha) / n;
        }

        /// <summary>NIGHT34 (split): did the world block run on the last Step (false on a held step)? The render keeps the world's previous
        /// positions only on steps where this is true.</summary>
        public bool Bend34WorldStepped => !n34HoldWorld;

        bool b34Wired, b34Latch;
        readonly List<int> b34KillRanks = new List<int>(32);
        readonly List<string> b34KillWeapons = new List<string>(32); // NIGHT34 B leftover 3: the kills' weapons (combo34)
        readonly Bend34Event b34Event = new Bend34Event();

        partial void N34BendStartRun()
        {
            if (!b34Wired)
            {
                mover.Events += B34OnMover;
                events.On("arclinePull", B34OnYank);
                events.On<KillEvent>("kill", B34OnKill);
                b34Wired = true;
            }
            b34Latch = false;
            b34KillRanks.Clear();
            b34KillWeapons.Clear();
            loadout.bend34Instant = false;
            loadout.bend34InstantSwaps = 0;
            Air34StartRun(); // NIGHT34 B leftover 1: the air multiplier (SurvivalSim.Air34.cs; its own row)
            Combo34StartRun(); // NIGHT34 B leftover 3: the named bend combo (SurvivalSim.Combo34.cs; bit combo34)
            if (!n34.bend34) return;
            bend34Rules ??= Bend34Rules.From(cfg);
            bend34.Reset(bend34Rules, n34.bendSplit34, step);
        }

        /// <summary>NIGHT34: a latch on this step from outside the grapple (the harness's swap check; nothing in the game calls it).</summary>
        public void Bend34LatchForCheck()
        {
            if (Bend34On) b34Latch = true;
        }

        void B34OnMover(MoverEvent e)
        {
            if (e.type == MoverEventType.RopeAttach && Bend34On) b34Latch = true; // a node, a wall, an enemy zip (the whip)
        }

        void B34OnYank(object _)
        {
            if (Bend34On) b34Latch = true; // the ARCLINE yanks a small enemy to you: the whip on an enemy
        }

        void B34OnKill(KillEvent e)
        {
            if (!Bend34On || e.owner != "player") return;
            b34KillRanks.Add(style.rank);
            b34KillWeapons.Add(e.weapon);
        }

        partial void N34BendBeginStep()
        {
            Air34Step(); // NIGHT34 B leftover 1: the air multiplier, before anything moves (SurvivalSim.Air34.cs; its own row)
            if (!Bend34On) return;
            if (bend34.BeginStep()) B34Emit("bend34End", false);
            n34HoldWorld = bend34.HoldWorld;
            loadout.bend34Instant = bend34.Inside;
        }

        partial void N34BendStep(double dt)
        {
            if (!Bend34On) return;
            var r = new Bend34Step();
            if (b34Latch)
            {
                b34Latch = false;
                bend34.Latch(ref r);
                if (r.started) Combo34BendStarted(); // a new bend: the combo starts empty (an upgrade keeps it)
                if (r.started || r.upgraded) B34Emit("bend34Start", false);
            }
            for (int i = 0; i < b34KillRanks.Count; i++)
            {
                bool inside = bend34.Inside;
                int ext = r.killsExtended;
                bend34.Kill(b34KillRanks[i], ref r);
                if (inside) B34Emit("bend34Kill", r.killsExtended > ext);
                if (inside) Combo34Kill(b34KillWeapons[i]);
            }
            b34KillRanks.Clear();
            b34KillWeapons.Clear();
            bend34.UpdateShare();
            Bend34Share = bend34.split ? 1 : bend34.share;
            Bend34Depth01 = bend34.Depth01;
        }

        void B34Emit(string name, bool extended)
        {
            b34Event.tier = bend34.tier == Bend34Tier.Full ? "full" : "short";
            b34Event.kills = bend34.kills;
            b34Event.meter = bend34.meter;
            b34Event.extended = extended;
            events.Emit(name, b34Event);
        }
    }
}
