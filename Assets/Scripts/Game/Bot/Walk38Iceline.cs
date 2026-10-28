// NIGHT38 builder S2 (NIGHT38-BRIEF card S2; the walk bot through every room, L-041): the walk pilot's script for n38-iceline's flight room j3 2.3 THE RUN
// (Design/levels/n38/iceline.beats.md; the room row's run38 map, written by Tools/night/gen-n37-iceline-boss.cjs --n38). The other new rooms (j1 THE
// GAUNTLET, j2 THE RINK, j4 REST) and NIGHT37's s rooms are the autopilot's (fights) and Walk29Pilot's as before.
//   j3: on the lane: pump dashes on the ice (no sprint: it drains the dash's stamina), the jump at the lip, the dash 0.4 s and the double 0.7 s after it
//       (harness iceline38 gates: lands on D1); on D1: the jump at its lip beside the G2 wall, held with the stick into it (the ride), let go past D2's
//       near edge; on D2: the chain (W1, E1, W2: released 3 m before each wall's north end with the stick away, the jump pressed again 0.25 s later
//       with the stick toward the next wall); on D3: the autopilot to the exit. In a void: the stair at its south end, hopping, back to the takeoff.
// Two ways in (the lead's call): the ONE dispatch line in Walk29Pilot.SampleMove, after the NIGHT36 line:
//       if (Ice38(out InputFrame f38i)) return Keep(f38i); // NIGHT38 S2: n38-iceline's THE RUN (Walk38Iceline.cs)
//   or, without it, the wrapper Walk38IcelineInput (the harness `iceline38 walk` uses it). Ice38 is idempotent within a step (a second call in the same
//   step returns the same frame), so both together do no harm.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Walk29Pilot
    {
        // the run38 map (world coordinates), read once from the level JSON
        bool run38Read;
        Vec3 r38Lane, r38Lip1, r38D1Lip, r38D2Lip, r38D3Exit;
        double[] r38D1, r38D2, r38D3;
        readonly List<(double planeX, int side, double zN, double zS)> r38Chain = new List<(double, int, double, double)>();
        readonly List<Vec3> r38Stairs = new List<Vec3>();
        // the flight in progress: 0 none, 1 the skate jump, 2 the G2 ride, 3 the chain; its step counters
        int f38Mode, f38Jump = -1, f38Released = -1, f38Wall, f38DashCool, f38LastStep = -1;
        InputFrame f38Last;

        /// <summary>The harness reads them: steps spent climbing out of a void; the flight now (0 none, 1 skate, 2 ride, 3 chain).</summary>
        internal int Fell38;
        internal int Mode38 => f38Mode;

        bool Level38Iceline => sim.campaignLevel != null && sim.campaignLevel.id == "n38-iceline";

        bool Run38Map()
        {
            if (run38Read) return r38D1 != null;
            run38Read = true;
            if (sim.map.json == null || !sim.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp) || !camp.TryGetValue("rooms", out object ro)) return false;
            foreach (object o in PortData.Arr(ro))
            {
                var rd = PortData.Obj(o);
                if (!rd.TryGetValue("run38", out object rr)) continue;
                var r = PortData.Obj(rr);
                double[] P(string k) { var a = PortData.Arr(r[k]); return new[] { PortData.Num(a[0]), PortData.Num(a[1]) }; }
                r38Lane = PortData.V3(r["laneStart"]); r38Lip1 = PortData.V3(r["lip1"]); r38D1Lip = PortData.V3(r["d1Lip"]); r38D2Lip = PortData.V3(r["d2Lip"]);
                r38D1 = P("d1"); r38D2 = P("d2"); r38D3 = P("d3");
                foreach (object w in PortData.Arr(r["chain"]))
                {
                    var wd = PortData.Obj(w);
                    var z = PortData.Arr(wd["z"]);
                    r38Chain.Add((PortData.Num(wd["planeX"]), (int)PortData.Num(wd["side"]), PortData.Num(z[0]), PortData.Num(z[1])));
                }
                foreach (object s in PortData.Arr(r["stairs"])) r38Stairs.Add(PortData.V3(s));
            }
            return r38D1 != null;
        }

        /// <summary>NIGHT38 S2: the frame for n38-iceline's j3 THE RUN, or false (the pilot's own frame). Idempotent within a step.</summary>
        internal bool Ice38(out InputFrame f)
        {
            f = default;
            if (!Level38Iceline || sim.campaign == null || sim.campaign.phase == Campaign24.Phase.None) return false;
            RoomDef rd = Room();
            if (rd != null && rd.id == "s2")
            {
                // 2.9 SLIPSTREAM (NIGHT37's, kept): no waves, its lesson is `reached` at the far end (round 2: the autopilot stood at the spawn), and on its
                // ice the autopilot slid into the door's frame (2 of 5 seeds): skate north on the door's line, slow while off it
                Vec3 q = sim.mover.position;
                bool onLine = Math.Abs(q.x - rd.exit.x) < 0.6;
                f = new InputFrame { moveZ = onLine || q.z - rd.exit.z > 12 ? 1 : 0.3, yaw = onLine ? Toward(q, rd.exit.x, q.z - 10) : Toward(q, rd.exit.x, Math.Min(q.z - 2, rd.exit.z)) };
                return true;
            }
            // round 3: j3 by POSITION (after its clear the campaign's room is already j4, and the autopilot walked off D3 into the void: 3 of 5 seeds)
            RoomDef j3 = sim.campaignLevel.rooms.Find(q => q.id == "j3");
            Vec3 at = sim.mover.position;
            bool inJ3 = j3 != null && at.x >= j3.x0 && at.x <= j3.x1 && at.z >= j3.z0 && at.z <= j3.z1;
            if (!inJ3 || !Run38Map()) { f38Mode = 0; return false; }
            r38D3Exit = new Vec3(j3.exit.x, 0, j3.z0 - 3); // through j3's north door
            if (f38LastStep == step) { f = f38Last; return true; }
            bool ok = Ice38Frame(sim.mover.position, sim.mover.grounded, out f);
            f38LastStep = step;
            f38Last = f;
            return ok;
        }

        bool f38Back;

        /// <summary>The run at a ride gate's lip on the ride line: onto the line first (slow); too close to the lip off the line (a landing that carried on),
        /// back 6 m south first. True while approaching; false = jump now.</summary>
        bool Approach38(Vec3 p, Vec3 lip, ref InputFrame f)
        {
            double dx = Math.Abs(p.x - lip.x), d = p.z - lip.z;
            if (!f38Back && d < 5 && dx > 0.3) f38Back = true;
            if (f38Back)
            {
                f.yaw = Toward(p, lip.x, lip.z + 6);
                if (JsMath.Hypot(p.x - lip.x, p.z - (lip.z + 6)) < 1.0) f38Back = false;
                return true;
            }
            f.yaw = Toward(p, lip.x, p.z - 2);
            if (dx > 0.3) f.moveZ = 0.25;
            return d > 0.4;
        }

        bool Ice38Frame(Vec3 p, bool grounded, out InputFrame f)
        {
            f = new InputFrame { moveZ = 1 };
            double dt = sim.step;
            int S(double s) => MovementMath.StepsFor(s, dt);
            bool riding = sim.mover.state == MoveState.WallRide;
            // in a void: the stair at its south end, then up it (south) onto the takeoff, hopping
            if (p.y < -0.2 && grounded)
            {
                f38Mode = 0;
                Vec3 st = p.z > r38D1[1] ? r38Stairs[0] : p.z > r38D2[1] ? r38Stairs[1] : r38Stairs[2];
                bool atStair = Math.Abs(p.x - st.x) < 1.2;
                // off the stair's line: across to it north of its foot (the steps' sides are walls); on it: south up the steps, hopping
                f.yaw = atStair ? Toward(p, st.x, st.z + 10) : Toward(p, st.x, Math.Min(p.z, st.z - 2.5));
                f.jump = Press(true, step % 12 == 0);
                Fell38++;
                return true;
            }
            if (f38Mode == 0 && grounded)
            {
                if (p.z <= r38D3[1] + 0.5) { f.yaw = Toward(p, r38D3Exit.x, r38D3Exit.z); return true; } // on the exit deck: straight to the door
                if (p.z > r38Lip1.z) f38Mode = 1;
                else if (p.z > r38D1[0] - 0.5 && p.z <= r38D1[1] + 0.5) f38Mode = 2;
                else if (p.z > r38D2[0] - 0.5 && p.z <= r38D2[1] + 0.5) f38Mode = 3;
                else return false;
                f38Jump = f38Released = -1;
                f38Back = false;
                f38Wall = 0;
            }
            if (f38Mode == 0) return false; // in the air from a script of no one's: leave it
            if (f38DashCool > 0) f38DashCool--;
            if (f38Mode == 1) // the skate gap
            {
                if (f38Jump < 0)
                {
                    f.yaw = Toward(p, r38Lip1.x, p.z - 10);
                    if (p.z - r38Lip1.z > 12 && grounded && f38DashCool == 0 && sim.mover.pips >= 1) { f.dash = Press(true, true); f38DashCool = S(0.35); }
                    if (p.z - r38Lip1.z <= 0.4) { f38Jump = step; f.jump = Press(true, true); }
                    return true;
                }
                int k = step - f38Jump;
                f.yaw = 0;
                f.jump = Press(true, k == S(0.7));
                if (k == S(0.4)) f.dash = Press(true, true);
                if (k > 6 && grounded) f38Mode = 0;
                return true;
            }
            if (f38Mode == 2) // the G2 ride: jump at D1's lip beside the wall, hold into it, let go past D2's near edge
            {
                if (f38Jump < 0)
                {
                    if (!Approach38(p, r38D1Lip, ref f)) { f38Jump = step; f.jump = Press(true, true); f.moveX = -0.6; }
                    return true;
                }
                f.yaw = 0;
                bool past = p.z < r38D2[1] - 1;
                f.moveX = past ? 0 : -0.6;
                f.jump = Press(!past, false);
                if (step - f38Jump > 6 && grounded) f38Mode = 0;
                return true;
            }
            // the chain (Harness.N38.Iceline.cs Run38Chain's rule)
            var w = r38Chain[Math.Min(f38Wall, r38Chain.Count - 1)];
            f.yaw = 0;
            bool held = true, press = false;
            double stick = w.side * 0.6;
            if (f38Jump < 0)
            {
                if (!Approach38(p, r38D2Lip, ref f)) { f38Jump = step; press = true; }
                else { held = false; stick = 0; }
            }
            else if (f38Released < 0)
            {
                if (riding && f38Wall < r38Chain.Count - 1 && p.z - w.zN <= 3) { f38Released = step; held = false; stick = -w.side * 0.7; }
            }
            else
            {
                int since = step - f38Released;
                var next = r38Chain[f38Wall + 1];
                if (since < S(0.25)) { held = false; stick = -w.side * 0.7; }
                else
                {
                    press = since == S(0.25);
                    stick = next.side * 0.6;
                    if (riding && since > S(0.3)) { f38Wall++; f38Released = -1; }
                }
            }
            f.moveX = stick;
            f.jump = Press(held || press, press);
            if (f38Jump >= 0 && step - f38Jump > 6 && grounded) f38Mode = 0;
            return true;
        }
    }

    /// <summary>NIGHT38 S2: Walk29Pilot with n38-iceline's THE RUN scripted (Walk29Pilot.Ice38) while the dispatch line is not in Walk29Pilot.SampleMove:
    /// the pilot steps as always, and in j3 its frame is replaced by the script's.</summary>
    public sealed class Walk38IcelineInput : IPlayerInput
    {
        public readonly Walk29Pilot pilot;
        double yaw, pitch;
        bool mine;

        public Walk38IcelineInput(Walk29Pilot pilot) { this.pilot = pilot; }

        public double Yaw => mine ? yaw : pilot.Yaw;
        public double Pitch => mine ? pitch : pilot.Pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; pilot.SetLook(y, p); }
        public bool Captured => pilot.Captured;

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = pilot.SampleMove(active);
            if (active && pilot.Ice38(out InputFrame g))
            {
                mine = true;
                yaw = g.yaw;
                pitch = g.pitch;
                return g;
            }
            mine = false;
            return f;
        }

        public CombatInput SampleCombat(bool active) => pilot.SampleCombat(active);
    }
}
