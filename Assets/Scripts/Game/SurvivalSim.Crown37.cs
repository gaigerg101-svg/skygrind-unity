// NIGHT37 builder C (crown37; NIGHT37.md C1, BOSSES.md "L5 THE CROWN"): THE CROWN's descent in the sim. Implements the lead's partials
// (SurvivalSim.N37.cs):
//   N37CrownStartRun  with the crown37 row on, a level whose rooms carry `spire37` / `fall37` / `hoop37` rows (n37-spire: Tools/night/
//                     gen-n37-spire-boss.cjs) gets its moving and dropping pieces as world layer "crown37Spire" (Core/Orbit37.cs), the ring
//                     targets of room v10, and boss37WalkInHere (the fight is where the player walked in: the spire's bridge).
//   N37CrownStep      every step after the Mover (N37Step: after N36BossStep): the pieces move and carry their rider, the ring targets test
//                     passes, the descent's fall rule (feet under the leg's floor: back to the leg's checkpoint, -fallDamage), the Crown.
//   N37CrownBegin     BossRoomBegin("crown37"): the Crown's walk-in (Crown37Begin, SurvivalSim.Crown37.Boss.cs).
//   N37CrownSystem    BossRoomSystem("crown37") is the Crown while it owns the fight.
// Off (row off, --off37 / -skygrindOff37, a tape without the bit): no layer, nothing runs (n33-spire is the level then). No UnityEngine.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>A ring target (room v10's hoop37 row): its centre, facing, passes and the step of the last pass (the view's flash).</summary>
    public sealed class Hoop37Target
    {
        public Vec3 at, normal;
        public string room;
        public int passes, lastPassStep = -100000;
    }

    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT37 C: the level's moving and dropping pieces (null with crown37 off or a level without them).</summary>
        public Orbit37 orbit37;
        /// <summary>NIGHT37 C: room v10's ring targets (empty without them).</summary>
        public readonly List<Hoop37Target> hoops37 = new List<Hoop37Target>();
        /// <summary>NIGHT37 C: the spire room's row (axis, legs' checkpoints and floors); null on a level without it.</summary>
        public Dictionary<string, object> spire37Row;
        public Vec3 spire37Axis;
        public readonly Vec3[] spire37Checkpoint = new Vec3[3];
        public readonly double[] spire37CheckpointYaw = new double[3], spire37Floor = new double[3];
        /// <summary>NIGHT40 B5: the spire room's rect (x0, z0, x1, z1), [0] NaN: none.</summary>
        public readonly double[] spire37Rect = { double.NaN, 0, 0, 0 };
        /// <summary>Steps the sim has run this run (the pass flash clock) and the player's chest a step ago (the pass test's segment).</summary>
        public int crown37Steps;
        Vec3 crown37PrevChest;
        bool crown37HavePrev;

        partial void N37CrownStartRun()
        {
            hoops37.Clear();
            spire37Row = null;
            crown37Steps = 0;
            crown37HavePrev = false;
            Crown37ClearBoss();
            if (!n37.crown37)
            {
                if (orbit37 != null) world?.SetLayer("crown37Spire", null);
                orbit37 = null;
                return;
            }
            orbit37 ??= new Orbit37(Orbit37Params.From(cfg), step);
            orbit37.Load(map?.json);
            orbit37.Reset();
            world?.SetLayer("crown37Spire", orbit37.Count > 0 ? orbit37.layer : null, true);
            Crown37ReadRows();
            if (spire37Row != null) boss37WalkInHere = true;
        }

        void Crown37ReadRows()
        {
            if (map?.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return;
            foreach (object r in rooms)
            {
                var room = PortData.Obj(r);
                string rid = room.TryGetValue("id", out object id) ? id as string : null;
                if (room.TryGetValue("hoop37", out object ho) && ho is List<object> hl)
                    foreach (object h in hl)
                    {
                        var row = PortData.Obj(h);
                        Vec3 n = PortData.V3(row["normal"]);
                        hoops37.Add(new Hoop37Target { at = PortData.V3(row["at"]), normal = Vec3.Normalize(n), room = rid });
                    }
                if (room.TryGetValue("spire37", out object so) && so is Dictionary<string, object> spire)
                {
                    spire37Row = spire;
                    spire37Axis = PortData.V3(spire["axis"]);
                    // NIGHT40 B5: the boss room's rect (crown40's clamp); NaN when the room has none
                    spire37Rect[0] = double.NaN;
                    if (room.TryGetValue("rect", out object rc) && rc is List<object> rl && rl.Count == 4)
                        for (int k = 0; k < 4; k++) spire37Rect[k] = PortData.Num(rl[k]);
                    var cps = PortData.Arr(spire["checkpoints"]);
                    var floors = PortData.Arr(spire["legFloors"]);
                    for (int k = 0; k < 3; k++)
                    {
                        var c = PortData.Arr(cps[k]);
                        spire37Checkpoint[k] = new Vec3(PortData.Num(c[0]), PortData.Num(c[1]), PortData.Num(c[2]));
                        spire37CheckpointYaw[k] = c.Count > 3 ? PortData.Num(c[3]) : 0;
                        spire37Floor[k] = PortData.Num(floors[k]);
                    }
                }
            }
        }

        partial void N37CrownStep(double dt)
        {
            if (orbit37 == null) return;
            crown37Steps++;
            orbit37.Step(mover, events);
            Vec3 chest = new Vec3(mover.position.x, mover.position.y + 1.0, mover.position.z);
            if (crown37HavePrev && hoops37.Count > 0)
            {
                double r = cfg.Num("design.n37.crown37.crown.passRadius");
                foreach (Hoop37Target h in hoops37)
                {
                    if (!Hoop37.Passed(crown37PrevChest, chest, h.at, h.normal, r, out Vec3 at)) continue;
                    h.passes++;
                    h.lastPassStep = crown37Steps;
                    events.Emit("crown37Pass", new Crown37Event { stage = "hoop", at = at });
                }
            }
            Crown37BossStep(dt, chest);
            crown37PrevChest = new Vec3(mover.position.x, mover.position.y + 1.0, mover.position.z); // after a fall's respawn: the checkpoint's chest
            crown37HavePrev = true;
        }

        /// <summary>The harness / reel: this run's pieces with the row forced on (or off), as a level row would set them at the run's start.</summary>
        public void Crown37Force(bool on)
        {
            n37.crown37 = on;
            N37CrownStartRun();
        }

        partial void N37CrownBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg)
        {
            if (id != "crown37" || !n37.crown37 || spire37Row == null) return;
            handled = true;
            ok = Crown37Begin(out spawn, out yawDeg);
        }

        partial void N37CrownSystem(string id, ref IBossSystem boss)
        {
            if (id == "crown37" && crown37 != null && crown37Owns) boss = crown37;
        }
    }
}
