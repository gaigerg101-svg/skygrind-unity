// NIGHT37 builder O (NIGHT37.md O1; Research/2026-10-12-wave37/BOSSES.md "L4 THE LOOM"): THE LOOM's level side in the sim, behind design.n37.loom37
// (sim.n37.loom37) and only on its level (design.n37.loom37.levelTo, n37-span: Tools/night/gen-n37-span-boss.cjs). Implements the lead's
// declaration-only hooks N37LoomStartRun / N37LoomStep / N37LoomBegin / N37LoomSystem (SurvivalSim.N37.cs).
// The level side (this part):
// - THE CANYON's anchor ledges (the room row's loom37.ledges): the last one the player stood on is the leg checkpoint; feet under
//   design.n37.loom37.fallY inside the canyon = a fall: back on that ledge for fallCostHp (BOSSES.md: "fall = back to the last anchor ledge, -20 HP").
// - The ride refill (design.n37.loom37.rideRefillS, NEW: the web has no grapple charges): one wall ride of that long inside a loom37 room gives one
//   grapple charge back (design.n32.grapple32's G32Give), once per ride: "pull, ride, pull" over a void with no enemies to refill on kills.
// - o2's SWING ANCHOR: the room row's loomSwing37 node moves east-west (swingAmpM, swingPeriodS); a rope hooked on it keeps its anchor on it
//   (as THE LOOM's joints in SurvivalSim.Boss30.cs).
// Off (row off, --off37, or another level): nothing here runs; the walk-in flag stays false.
// No UnityEngine: SimHarness compiles this file.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>One anchor ledge of THE CANYON (a tower top, the start ledge, the end ledge): a fall comes back to the last one stood on.</summary>
        public sealed class Loom37Ledge
        {
            public string id;
            public Vec3 at;
            public double x0, z0, x1, z1, y;
            public int pair;
            public bool Under(Vec3 feet) => feet.x >= x0 - 0.3 && feet.x <= x1 + 0.3 && feet.z >= z0 - 0.3 && feet.z <= z1 + 0.3 && System.Math.Abs(feet.y - y) < 0.6;
        }

        /// <summary>One thread anchor of THE CANYON: a level grapple node (index `node` in map.grappleNodes), the tower pair it leads to, its ledge.</summary>
        public sealed class Loom37Anchor
        {
            public string id, ledge;
            public int node, pair;
            public Vec3 at;
            /// <summary>Steps until it goes dark (-1: not cut); pulsing RED in its last cutPulse steps; dark: parked out of reach.</summary>
            public int cutLeft = -1;
            public bool dark;
        }

        /// <summary>NIGHT37 O: the run is on THE LOOM's level with design.n37.loom37 on (the level side runs).</summary>
        public bool Loom37Level;

        public readonly List<Loom37Ledge> loom37Ledges = new List<Loom37Ledge>();
        public readonly List<Loom37Anchor> loom37Anchors = new List<Loom37Anchor>();

        /// <summary>The last anchor ledge stood on (null before the canyon).</summary>
        public Loom37Ledge loom37LastLedge;

        /// <summary>THE CANYON's floor plan [x0, z0, x1, z1] (world) and its room id.</summary>
        public double loom37CanyonX0, loom37CanyonZ0, loom37CanyonX1, loom37CanyonZ1;
        public string loom37CanyonId;

        /// <summary>The swing anchor's node index (-1: none) and its rest point.</summary>
        public int loom37SwingNode = -1;
        public Vec3 loom37SwingRest;

        /// <summary>Counters this run (the harness): canyon falls, ride refills.</summary>
        public int loom37Falls, loom37RideRefills;

        readonly HashSet<string> loom37Rooms = new HashSet<string>();
        readonly List<(double x0, double z0, double x1, double z1)> loom37RoomRects = new List<(double, double, double, double)>();
        double l37FallY, l37FallCost, l37SwingAmp, l37SwingPeriod;
        int l37RideRefillSteps, l37RideSteps, l37Steps;
        bool l37RideGiven, l37ArriveHooked, l37Arrived;
        double l37ArriveUpMax;
        /// <summary>Counters this run: pull arrivals whose upward speed was clamped (design.n37.loom37.arriveUpMax).</summary>
        public int loom37ArriveClamps;

        partial void N37LoomStartRun()
        {
            Loom37Level = false;
            loom37Ledges.Clear();
            loom37Anchors.Clear();
            loom37Rooms.Clear();
            loom37RoomRects.Clear();
            loom37LastLedge = null;
            loom37SwingNode = -1;
            loom37CanyonId = null;
            loom37Falls = loom37RideRefills = 0;
            l37RideSteps = l37Steps = 0;
            l37RideGiven = false;
            l37Arrived = false;
            loom37ArriveClamps = 0;
            if (!l37ArriveHooked)
            {
                l37ArriveHooked = true;
                events.On("grappleArrive", o => { if (Loom37Level) l37Arrived = true; });
            }
            Loom37BossStartRun();
            if (!n37.loom37 || map?.json == null || !map.json.ContainsKey("campaign")) return;
            string want = cfg.Has("design.n37.loom37.levelTo") ? cfg.Str("design.n37.loom37.levelTo") : "n37-span";
            string levelId = N45Levels.BaseId(cfg, LevelDef.From(map.json).id); // NIGHT46 (n46-shared.cjs): n46-span is n40-span here (loom40's o1-o3); an n46 id is only loaded when its row swapped it in
            if (levelId != want && !Loom37OnSpan38(levelId)) return;
            Loom37Level = true;
            boss37WalkInHere = true; // THE CANYON is where the player walks in: Campaign24 keeps them there (no Respawn into a far arena)
            const string R = "design.n37.loom37.";
            l37FallY = cfg.Num(R + "fallY");
            l37FallCost = cfg.Num(R + "fallCostHp");
            l37RideRefillSteps = MovementMath.StepsFor(cfg.Num(R + "rideRefillS"), step);
            l37SwingAmp = cfg.Num(R + "swingAmpM");
            l37SwingPeriod = cfg.Num(R + "swingPeriodS");
            l37ArriveUpMax = cfg.Num(R + "arriveUpMax");
            var camp = PortData.Obj(map.json["campaign"]);
            int nodeBase = 0;
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var r = PortData.Obj(ro);
                string id = r.ContainsKey("id") ? (string)r["id"] : "";
                int nodes = r.ContainsKey("grappleNodes") ? PortData.Arr(r["grappleNodes"]).Count : 0;
                var rect = PortData.Arr(r["rect"]);
                double x0 = PortData.Num(rect[0]), z0 = PortData.Num(rect[1]), x1 = PortData.Num(rect[2]), z1 = PortData.Num(rect[3]);
                if (r.ContainsKey("loomRoom37"))
                {
                    loom37Rooms.Add(id);
                    loom37RoomRects.Add((x0, z0, x1, z1));
                }
                if (r.ContainsKey("loomSwing37"))
                {
                    var sw = PortData.Obj(r["loomSwing37"]);
                    loom37SwingNode = nodeBase + (int)PortData.Num(sw["node"]);
                    loom37SwingRest = PortData.V3(sw["rest"]);
                }
                if (r.ContainsKey("loom37"))
                {
                    loom37CanyonId = id;
                    loom37CanyonX0 = x0; loom37CanyonZ0 = z0; loom37CanyonX1 = x1; loom37CanyonZ1 = z1;
                    var lr = PortData.Obj(r["loom37"]);
                    foreach (object lo in PortData.Arr(lr["ledges"]))
                    {
                        var l = PortData.Obj(lo);
                        var lrect = PortData.Arr(l["rect"]);
                        loom37Ledges.Add(new Loom37Ledge
                        {
                            id = (string)l["id"], at = PortData.V3(l["at"]), y = PortData.Num(l["y"]), pair = (int)PortData.Num(l["pair"]),
                            x0 = PortData.Num(lrect[0]), z0 = PortData.Num(lrect[1]), x1 = PortData.Num(lrect[2]), z1 = PortData.Num(lrect[3]),
                        });
                    }
                    foreach (object ao in PortData.Arr(lr["anchors"]))
                    {
                        var a = PortData.Obj(ao);
                        loom37Anchors.Add(new Loom37Anchor
                        {
                            id = (string)a["id"], node = nodeBase + (int)PortData.Num(a["node"]), at = PortData.V3(a["at"]), pair = (int)PortData.Num(a["pair"]),
                            ledge = a.ContainsKey("ledge") && a["ledge"] is string s ? s : null,
                        });
                    }
                    Loom37BossLevel(lr);
                }
                nodeBase += nodes;
            }
            if (loom37SwingNode >= map.grappleNodes.Count) loom37SwingNode = -1;
            // the run before may have parked a cut anchor: every canyon node back at its own place
            foreach (Loom37Anchor a in loom37Anchors) if (a.node < map.grappleNodes.Count) map.grappleNodes[a.node] = a.at;
        }

        partial void N37LoomStep(double dt)
        {
            if (!Loom37Level) return;
            l37Steps++;
            Vec3 feet = mover.position;
            // a pull's arrival in a loom37 room stops there: the upward speed the pull carried is clamped (grapple35 clamps only the horizontal)
            if (l37Arrived)
            {
                l37Arrived = false;
                if (Loom37InRoom(feet) && mover.velocity.y > l37ArriveUpMax)
                {
                    mover.velocity = new Vec3(mover.velocity.x, l37ArriveUpMax, mover.velocity.z);
                    loom37ArriveClamps++;
                }
            }
            // o2's swing anchor
            if (loom37SwingNode >= 0 && l37SwingPeriod > 0)
            {
                double ph = 2 * System.Math.PI * (l37Steps * step) / l37SwingPeriod;
                Loom37MoveNode(loom37SwingNode, new Vec3(loom37SwingRest.x + l37SwingAmp * JsMath.Sin(ph), loom37SwingRest.y, loom37SwingRest.z));
            }
            // the anchor ledges: the last one stood on
            if (loom37Ledges.Count > 0 && mover.grounded)
                foreach (Loom37Ledge l in loom37Ledges)
                    if (l.Under(feet))
                    {
                        if (loom37LastLedge != l) events.Emit("loom37Ledge", l.id);
                        loom37LastLedge = l;
                        break;
                    }
            Loom47Net(feet); // NIGHT47 L (loom47): THE WARP NET catches a fall above the fall line and throws it back up (SurvivalSim.N47.Loom.cs)
            // a fall in THE CANYON: back on the last anchor ledge, for its health cost
            if (loom37CanyonId != null && feet.y < l37FallY && feet.x >= loom37CanyonX0 && feet.x <= loom37CanyonX1 && feet.z >= loom37CanyonZ0 && feet.z <= loom37CanyonZ1)
            {
                Loom37Ledge back = loom37LastLedge ?? (loom37Ledges.Count > 0 ? loom37Ledges[0] : null);
                if (back != null)
                {
                    loom37Falls++;
                    health.hp = System.Math.Max(1, health.hp - Loom47FallCost(l37FallCost)); // NIGHT47 L (loom47): the fight's fall costs numbers.fallHp
                    Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                    events.Emit("loom37Fall", back.id);
                    Loom37BossFell(back);
                }
            }
            // the ride refill: one wall ride of rideRefillS inside a loom37 room gives one grapple charge back
            if (mover.state == MoveState.WallRide && Loom37InRoom(feet))
            {
                if (++l37RideSteps >= l37RideRefillSteps && !l37RideGiven && l37RideRefillSteps > 0 && Grapple32On)
                {
                    l37RideGiven = true;
                    int before = loom37RideRefills;
                    G32Give(ref loom37RideRefills);
                    if (loom37RideRefills != before) events.Emit("loom37RideRefill", grappleCharges32);
                }
            }
            else
            {
                l37RideSteps = 0;
                l37RideGiven = false;
            }
            Loom37BossStep(dt);
        }

        /// <summary>NIGHT38 O2 (design.n38.span38, N38Blocks bit 4): level 4 rebuilt as n38-span keeps THE LOOM's rooms o1-o3 (Tools/night/gen-n38-span.cjs):
        /// the level side runs there too while the row is on. N37StartRun runs before N38StartRun (SurvivalSim.Run.cs), so the run's bit is taken here
        /// the way N38StartRun takes it (a tape's override, else off when forced off, else the design's). Row off: n37-span only, as NIGHT37.</summary>
        bool Loom37OnSpan38(string levelId)
        {
            if (Loom37OnSpan40(levelId)) return true; // NIGHT40 B4: n40-span (loom40) carries THE CANYON too (SurvivalSim.N40.Loom.cs)
            bool on = n38Override.HasValue ? n38Override.Value.span38 : !N38Blocks.ForceOff && n38Design.span38;
            if (!on) return false;
            string to = cfg.Has("design.n38.span38.levelTo") ? cfg.Str("design.n38.span38.levelTo") : "";
            return to.Length > 0 && levelId == to;
        }

        /// <summary>Is the point inside a loom37 room's floor plan?</summary>
        public bool Loom37InRoom(Vec3 p)
        {
            foreach (var r in loom37RoomRects) if (p.x >= r.x0 && p.x <= r.x1 && p.z >= r.z0 && p.z <= r.z1) return true;
            return false;
        }

        /// <summary>Moves a grapple node; a rope hooked on it keeps its anchor on it.</summary>
        void Loom37MoveNode(int i, Vec3 to)
        {
            if (i < 0 || i >= map.grappleNodes.Count) return;
            bool onIt = mover.rope.phase != RopePhase.None && Vec3.DistanceTo(mover.rope.anchor, map.grappleNodes[i]) < 1e-6;
            if (onIt && to.y < -500)
            {
                // the thread snaps (a cut anchor, a torn knee): the rope lets go, you drop with half your carry (as the old LOOM's CUT)
                mover.Launch(System.Math.Min(mover.velocity.y, 0), mover.velocity.x * 0.5, mover.velocity.z * 0.5);
                events.Emit("loom37Snap", i);
            }
            else if (onIt && mover.rope.phase == RopePhase.Attached) mover.rope.anchor = to;
            map.grappleNodes[i] = to;
        }

        partial void N37LoomBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg)
        {
            if (id != "loom37") return;
            handled = true; // the old LOOM never takes this id; with the row off (or off this level) the room has no boss and clears
            ok = false;
            if (!Loom37Level) return;
            Loom37BossBegin(ref ok, ref spawn, ref yawDeg);
        }

        partial void N37LoomSystem(string id, ref IBossSystem boss)
        {
            if (id != "loom37" || !Loom37Level) return;
            Loom37BossSystem(ref boss);
        }

        partial void Loom37BossBegin(ref bool ok, ref Vec3 spawn, ref double yawDeg);
        partial void Loom37BossSystem(ref IBossSystem boss);
        partial void Loom37BossStartRun();
        partial void Loom37BossLevel(Dictionary<string, object> canyon);
        partial void Loom37BossStep(double dt);
        partial void Loom37BossFell(Loom37Ledge back);
    }
}
