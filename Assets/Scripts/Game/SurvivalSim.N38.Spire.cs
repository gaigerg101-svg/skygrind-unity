// NIGHT38 builder C2 (spire38, N38Blocks bit 5; NIGHT38.md L5, PLAN.md section 2 "a limited dash meter at each gate, so the dash-jump can't be
// spammed gap to gap"): THE GATE METER of level 5 rebuilt for flight (n38-spire: Tools/night/gen-n38-spire.cjs). It runs only with the spire38 block on
// and only in a level whose campaign rooms carry `flight38` / `meter38` rows (none before NIGHT38: every older level is untouched; --off38 /
// -skygrindOff38 take the block off, and with it the swap to n38-spire). The rules (numbers: design.n38.spire38.meter, each with its _why):
//   - a PAD (a room's meter38 rect, the feet standing on it, grounded and not dashing, within padReach m of its height): the stamina bar is HELD AT
//     the pad's jump-dashes x one jump-dash's cost (design.stamina.boost.cost + dashJumpExtra = 0.75 of the 1.0 bar; the web's pips without the bar:
//     dash.pipCost + dashJump.extraPipCost) + slack (a sprint's drain on the takeoff step), never above the bar's size; the event spire38Meter
//     {gate, x, y, z, bar} on the step the feet arrive on it (GG's sound);
//   - a FLIGHT ROOM (a room row with flight38: true, the feet inside its rect at any height), off its pads: the bar never rises by itself (holdRefill):
//     a rise over the last step's value is taken back, so only a pad fills it; spending works as ever;
//   - ONE DASH PER PAD (oneDash): in a flight room, once a dash begun since the feet left the pad has ended (its jump-dash, or the dash run out), the
//     meter is spent: the bar to 0 and the double jump gone until the feet stand on a pad or the ground again (the Mover gives the double back on
//     landing). Measured (harness spire38 gates): under the gate's ceiling a jump + a late dash + the double jump, or two dashes + the double,
//     reach 18-21 m, as far as the jump-dash: the meter is what makes the jump-dash the only way across;
//   - in a flight room the grapple is off (noHook: Mover.ropeEnabled false while the feet are in it; the value it had comes back at the room's edge):
//     grapple35's pull goes to any surface within 40 m, so it would cross every gate.
// It fills the shared partials N38SpireStartRun / N38SpireStep (SurvivalSim.N38.cs: N38Step after N37Step, in simulate() order after the Mover
// and N37's systems); the rows load on the run's first step (the run's map is set by then). No randomness; no timers.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>spire38Meter's payload: the pad's gate, where the feet are, the bar it set.</summary>
    public sealed class Spire38Event
    {
        public string gate = "";
        public double x, y, z, bar;
    }

    public sealed partial class SurvivalSim
    {
        struct Spire38Pad
        {
            public string gate;
            public double x0, z0, x1, z1, y, jumps;
        }

        struct Spire38Rect
        {
            public double x0, z0, x1, z1;
        }

        readonly List<Spire38Pad> spire38Pads = new List<Spire38Pad>();
        readonly List<Spire38Rect> spire38Rooms = new List<Spire38Rect>();
        bool spire38Loaded;
        double spire38Held = -1; // the bar (or pips) at the end of the last step inside a flight room; -1 outside one
        int spire38Pad = -1; // the pad the feet were on last step (-1: none)
        bool spire38RopeOff, spire38RopeWas;
        bool spire38Dashed; // a dash has begun since the feet last stood on a pad (or the ground outside a flight room)

        /// <summary>The harness's counts: pads that filled the bar, steps a refill was taken back, steps the hook was held off.</summary>
        public int spire38Fills, spire38Holds, spire38HookSteps, spire38Spent;

        /// <summary>Is the gate meter live this run (the block on and the level has flight rooms)? Valid after the run's first step.</summary>
        public bool Spire38Live => spire38Rooms.Count > 0 || spire38Pads.Count > 0;

        partial void N38SpireStartRun()
        {
            spire38Pads.Clear();
            spire38Rooms.Clear();
            spire38Loaded = false;
            spire38Held = -1;
            spire38Pad = -1;
            spire38RopeOff = false; // Arcline21.Reset at the run's start sets the rope's ownership afresh
            spire38Fills = spire38Holds = spire38HookSteps = spire38Spent = 0;
            spire38Dashed = false;
        }

        void Spire38Load()
        {
            spire38Loaded = true;
            if (!n38.spire38) return;
            if (map?.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return;
            double perGate = cfg.Num("design.n38.spire38.meter.perGate");
            foreach (object r in rooms)
            {
                var room = PortData.Obj(r);
                if (room.TryGetValue("flight38", out object fl) && fl is bool f && f && room.TryGetValue("rect", out object rc))
                {
                    var q = PortData.Arr(rc);
                    spire38Rooms.Add(new Spire38Rect { x0 = PortData.Num(q[0]), z0 = PortData.Num(q[1]), x1 = PortData.Num(q[2]), z1 = PortData.Num(q[3]) });
                }
                if (room.TryGetValue("meter38", out object mo) && mo is List<object> pads)
                    foreach (object po in pads)
                    {
                        var pad = PortData.Obj(po);
                        var q = PortData.Arr(pad["rect"]);
                        spire38Pads.Add(new Spire38Pad
                        {
                            gate = pad.TryGetValue("gate", out object g) ? g as string ?? "" : "",
                            x0 = PortData.Num(q[0]), z0 = PortData.Num(q[1]), x1 = PortData.Num(q[2]), z1 = PortData.Num(q[3]),
                            y = pad.TryGetValue("y", out object py) ? PortData.Num(py) : 0,
                            jumps = pad.TryGetValue("dashJumps", out object dj) ? PortData.Num(dj) : perGate,
                        });
                    }
            }
        }

        /// <summary>One jump-dash's cost in the Mover's own units: bar units with design.stamina's bar on, the web's pips without.</summary>
        double Spire38Unit() => mover.stamina.rules.Any
            ? mover.stamina.rules.cost + mover.stamina.rules.dashJumpExtra
            : cfg.movement.dash.pipCost + cfg.movement.dashJump.extraPipCost;

        double Spire38Level
        {
            get => mover.stamina.rules.Any ? mover.stamina.bar : mover.pips;
            set
            {
                if (mover.stamina.rules.Any) mover.stamina.bar = value;
                else mover.pips = value;
            }
        }

        double Spire38Max => mover.stamina.rules.Any ? mover.StaminaMax : mover.MaxPips;

        /// <summary>Every step, after the Mover (N38Step, after N38StagStep).</summary>
        partial void N38SpireStep(double dt)
        {
            if (!spire38Loaded) Spire38Load();
            if (!Spire38Live) return;
            Vec3 p = mover.position;
            bool inFlight = false;
            foreach (Spire38Rect r in spire38Rooms)
                if (p.x >= r.x0 && p.x <= r.x1 && p.z >= r.z0 && p.z <= r.z1) { inFlight = true; break; }
            int pad = -1;
            double reach = cfg.Num("design.n38.spire38.meter.padReach");
            for (int i = 0; i < spire38Pads.Count; i++)
            {
                Spire38Pad q = spire38Pads[i];
                if (p.x >= q.x0 && p.x <= q.x1 && p.z >= q.z0 && p.z <= q.z1 && p.y >= q.y - reach && p.y <= q.y + reach) { pad = i; break; }
            }
            // STANDING on a pad (grounded, not in a dash): the bar is held at the pad's meter, up or down (a sprint on the pad does not drain it);
            // a dash begun on the pad spends it as ever (no top-up while dashing, none once the jump has left the ground)
            bool standing = pad >= 0 && mover.grounded && mover.state != MoveState.Dash;
            if (standing || mover.grounded && !inFlight) spire38Dashed = false;
            else if (inFlight && mover.state == MoveState.Dash) spire38Dashed = true;
            else if (inFlight && spire38Dashed && cfg.Design("n38.spire38.meter.oneDash"))
            {
                // the dash is over: the pad's one jump-dash is spent (no second dash, no double jump) until a pad
                if (Spire38Level > 0) Spire38Level = 0;
                if (mover.doubleJumpsLeft > 0) { mover.doubleJumpsLeft = 0; spire38Spent++; }
            }
            if (standing)
            {
                Spire38Pad q = spire38Pads[pad];
                double level = JsMath.Min(Spire38Max, q.jumps * Spire38Unit() + cfg.Num("design.n38.spire38.meter.slack"));
                Spire38Level = level;
                if (pad != spire38Pad)
                {
                    spire38Fills++;
                    events.Emit("spire38Meter", new Spire38Event { gate = q.gate, x = p.x, y = p.y, z = p.z, bar = level });
                }
            }
            else if ((inFlight || pad >= 0) && spire38Held >= 0 && cfg.Design("n38.spire38.meter.holdRefill") && Spire38Level > spire38Held)
            {
                Spire38Level = spire38Held; // no refill by itself in a flight room: only a pad fills the meter
                spire38Holds++;
            }
            spire38Pad = standing ? pad : pad >= 0 ? spire38Pad : -1; // the arrival edge: standing on a pad it was not standing on
            spire38Held = inFlight || pad >= 0 ? Spire38Level : -1;

            bool hookOff = inFlight && cfg.Design("n38.spire38.meter.noHook");
            if (hookOff)
            {
                if (!spire38RopeOff)
                {
                    spire38RopeWas = mover.ropeEnabled;
                    spire38RopeOff = true;
                }
                else if (mover.ropeEnabled) spire38RopeWas = true; // owned meanwhile (a pickup): it comes back at the edge
                mover.ropeEnabled = false;
                spire38HookSteps++;
            }
            else if (spire38RopeOff)
            {
                mover.ropeEnabled = spire38RopeWas;
                spire38RopeOff = false;
            }
        }
    }
}
