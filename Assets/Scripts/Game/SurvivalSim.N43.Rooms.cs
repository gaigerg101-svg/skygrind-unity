// NIGHT43 builder E1 (design.n43.rooms43; NIGHT43.md E1, GG G5 / G22; CODE-FACTS-A B9, CODE-FACTS-B "G5 door"): the sim's side of rooms43.
// With the row on (n43.rooms43, read at the run's start; Campaign24.rooms43 is set from it when the campaign starts):
//   - every body a room spawns (its waves, its top-ups: CampaignWorld.Spawn) is that room's; the door counts ONLY those (Campaign24 asks
//     ICampaignWorld.RoomAlive43), not every enemy in the level (NIGHT42: enemies.AliveCount); a stand-in (a lesson's example target) is
//     tracked but never holds the door (its lesson does);
//   - a room's body (not a stand-in) that stays out of its room's bounds (the rect and the nodes' box + outMarginM) or under its floor (the lowest
//     of its spawn and nodes, minus pitDropM: a dry pit, a fall through geometry) for endS ends: alive false, no kill event, no points (the kill
//     plane's own way);
//   - the kind "heavy" (levels 3 and 5's rosters: a plain 40 HP husk until tonight, CODE-FACTS-B) spawns as heavyKind (the TANK);
//   - N43RoomLeftCount answers U1's counter (Campaign24.RoomLeft43).
// Off: none of this runs and CampaignWorld answers as NIGHT42. No allocation per step (the records are structs in a list kept for the sim's life).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        const string Rooms43Root = "design.n43.rooms43.";

        struct Body43
        {
            public Husk h;
            public int id, room, outSteps;
            public bool standIn;
            public double ax, az; // G22: the still spell's anchor
            public int stillSteps, tellAt;
        }

        /// <summary>The harness's counts this run (G22): the sim's unstick tells and steps (a room body still near the player whatever the brain
        /// thinks: a swarmer hung off the ground, a dazed charger).</summary>
        public int rooms43Unsticks, rooms43UnstickTells;

        readonly List<Body43> rm43Bodies = new List<Body43>();
        bool rm43On;
        int rm43EndSteps;
        double rm43PitDrop, rm43OutMargin;
        string rm43Heavy;
        LevelDef rm43BoundsFor;
        double[] rm43MinX, rm43MaxX, rm43MinZ, rm43MaxZ, rm43Floor;

        /// <summary>The harness's counts this run: bodies ended out of their room / under its floor; bodies a room spawned.</summary>
        public int rooms43Ended, rooms43Tracked;

        /// <summary>Is rooms43 on for this run (the campaign's door rules)?</summary>
        public bool Rooms43On => rm43On;

        partial void N43RoomsStartRun()
        {
            rm43Bodies.Clear();
            rooms43Ended = rooms43Tracked = rooms43Unsticks = rooms43UnstickTells = 0;
            rm43BoundsFor = null;
            rm43On = n43.rooms43;
            if (!rm43On) return;
            rm43EndSteps = System.Math.Max(1, MovementMath.StepsFor(cfg.Num(Rooms43Root + "endS"), step));
            rm43PitDrop = cfg.Num(Rooms43Root + "pitDropM");
            rm43OutMargin = cfg.Num(Rooms43Root + "outMarginM");
            rm43Heavy = cfg.Has(Rooms43Root + "heavyKind") ? cfg.Str(Rooms43Root + "heavyKind") : "husk";
            rm43WatchS = cfg.Num(Rooms43Root + "onScreenRelocateS");
            rm43TellSteps = System.Math.Max(1, MovementMath.StepsFor(cfg.Num(Rooms43Root + "onScreenTellS"), step));
            rm43StepMin = cfg.Num(Rooms43Root + "onScreenStepMinM");
            rm43StillS = cfg.Num(Rooms43Root + "stillNudgeS");
            rm43StillMove = cfg.Num(Rooms43Root + "stillMoveM");
            rm43StillRange = cfg.Num(Rooms43Root + "stillRangeM");
            rm43UnstickSteps = System.Math.Max(1, MovementMath.StepsFor(cfg.Num(Rooms43Root + "unstickS"), step));
        }

        int rm43UnstickSteps;

        double rm43StillS, rm43StillMove, rm43StillRange;

        /// <summary>The harness's control (`n43rooms still --nowatch`): rooms43's door rules on, its on-screen watchdog off.</summary>
        public bool rooms43NoWatch;

        /// <summary>The harness's control (`n43rooms idle --nocommit`): rooms43 on without its round-2 commit gate and stand-in rule.</summary>
        public bool rooms43CommitOff;

        double rm43WatchS, rm43StepMin;
        int rm43TellSteps;

        /// <summary>The kind a room's spawn asks for, as rooms43 spawns it ("heavy" -> heavyKind).</summary>
        string Rooms43Kind(string kind) => rm43On && kind == "heavy" ? rm43Heavy : kind;

        /// <summary>A body the current room spawned (a wave, a top-up, a stand-in).</summary>
        void Rooms43Track(Husk h, bool standIn)
        {
            if (!rm43On || h == null || campaign == null) return;
            for (int i = rm43Bodies.Count - 1; i >= 0; i--)
                if (rm43Bodies[i].h == h) rm43Bodies.RemoveAt(i); // a reused pool body: its old record goes
            rm43Bodies.Add(new Body43 { h = h, id = h.id, room = campaign.room, standIn = standIn });
            rooms43Tracked++;
        }

        bool Rooms43Live(in Body43 b) => b.h.alive && b.h.id == b.id;

        /// <summary>The bodies alive that room `room` spawned.</summary>
        int Rooms43Alive(int room)
        {
            int n = 0;
            for (int i = 0; i < rm43Bodies.Count; i++)
            {
                Body43 b = rm43Bodies[i];
                if (b.room == room && !b.standIn && Rooms43Live(b)) n++; // a stand-in is a lesson's target, never the door's (the lesson gates it)
            }
            return n;
        }

        /// <summary>The stand-ins alive that room `room` placed.</summary>
        int Rooms43StandIns(int room)
        {
            int n = 0;
            for (int i = 0; i < rm43Bodies.Count; i++)
            {
                Body43 b = rm43Bodies[i];
                if (b.room == room && b.standIn && Rooms43Live(b)) n++;
            }
            return n;
        }

        /// <summary>Each room's bounds: the rect and its nodes' box (+ outMarginM), the floor (the lowest of the spawn and the nodes).</summary>
        void Rooms43Bounds()
        {
            if (rm43BoundsFor == campaignLevel) return;
            rm43BoundsFor = campaignLevel;
            int n = campaignLevel.rooms.Count;
            rm43MinX = new double[n]; rm43MaxX = new double[n]; rm43MinZ = new double[n]; rm43MaxZ = new double[n]; rm43Floor = new double[n];
            for (int i = 0; i < n; i++)
            {
                RoomDef r = campaignLevel.rooms[i];
                bool rect = r.x1 > r.x0 || r.z1 > r.z0;
                double x0 = rect ? System.Math.Min(r.x0, r.x1) : r.spawn.x, x1 = rect ? System.Math.Max(r.x0, r.x1) : r.spawn.x;
                double z0 = rect ? System.Math.Min(r.z0, r.z1) : r.spawn.z, z1 = rect ? System.Math.Max(r.z0, r.z1) : r.spawn.z;
                double fl = r.spawn.y;
                foreach (Vec3 v in r.nodes)
                {
                    x0 = System.Math.Min(x0, v.x); x1 = System.Math.Max(x1, v.x);
                    z0 = System.Math.Min(z0, v.z); z1 = System.Math.Max(z1, v.z);
                    fl = System.Math.Min(fl, v.y);
                }
                rm43MinX[i] = x0 - rm43OutMargin; rm43MaxX[i] = x1 + rm43OutMargin;
                rm43MinZ[i] = z0 - rm43OutMargin; rm43MaxZ[i] = z1 + rm43OutMargin;
                rm43Floor[i] = fl;
            }
        }

        partial void N43RoomsStep(double dt)
        {
            // G22: the watchdog's on-screen relocation, in a campaign run with rooms43 on (0 = the web's watchdog: survival, the row off)
            bool watch = rm43On && campaignOn && !rooms43NoWatch;
            brain.onScreenRelocateAt43 = watch ? rm43WatchS : 0;
            brain.onScreenTellSteps43 = rm43TellSteps;
            brain.onScreenStepMin43 = rm43StepMin;
            brain.stillNudgeS43 = watch ? rm43StillS : 0; // and the ring waiters re-place when they stood still near the player
            brain.stillMoveM43 = rm43StillMove;
            brain.stillRangeM43 = rm43StillRange;
            if (!rm43On || campaign == null || campaignLevel == null || rm43Bodies.Count == 0) return;
            Rooms43Bounds();
            for (int i = rm43Bodies.Count - 1; i >= 0; i--)
            {
                Body43 b = rm43Bodies[i];
                if (!Rooms43Live(b)) { rm43Bodies.RemoveAt(i); continue; }
                if (b.standIn || b.room < 0 || b.room >= rm43Floor.Length) continue;
                Vec3 p = b.h.position;
                bool outside = p.x < rm43MinX[b.room] || p.x > rm43MaxX[b.room] || p.z < rm43MinZ[b.room] || p.z > rm43MaxZ[b.room] || p.y < rm43Floor[b.room] - rm43PitDrop;
                b.outSteps = outside ? b.outSteps + 1 : 0;
                if (b.outSteps >= rm43EndSteps)
                {
                    b.h.alive = false; // ended: counted dead for the door, no kill event, no points (the kill plane's way, Husks.StepPool)
                    rooms43Ended++;
                    rm43Bodies.RemoveAt(i);
                    continue;
                }
                if (watch) Rooms43Unstick(ref b);
                rm43Bodies[i] = b;
            }
        }

        /// <summary>G22: a room body that moved under stillMoveM for unstickS within stillRangeM of the player, not in an attack, gets the tell
        /// (navTell43, its id) and onScreenTellS later steps to the nearest free node onScreenStepMinM away that reaches the player (navRelocate43).
        /// The brain's own watchdog and Still43 come first (2 s); this answers what they cannot see (a body off the ground, a dazed charger).</summary>
        void Rooms43Unstick(ref Body43 b)
        {
            Husk h = b.h;
            Vec3 p = h.position, me = mover.position;
            double mv = JsMath.Hypot(p.x - b.ax, p.z - b.az), d = JsMath.Hypot(p.x - me.x, p.z - me.z);
            if (b.tellAt > 0)
            {
                if (--b.tellAt > 0) return;
                if (nav.Relocate(p.x, p.z, 0, rm43StepMin, 180, h.kind != "charger", out Vec3 spot))
                {
                    h.Teleport(spot);
                    rooms43Unsticks++;
                    events.Emit("navRelocate43", h.id);
                }
                b.ax = h.position.x;
                b.az = h.position.z;
                b.stillSteps = 0;
                return;
            }
            if (mv > rm43StillMove || h.committed || d > rm43StillRange)
            {
                b.ax = p.x;
                b.az = p.z;
                b.stillSteps = 0;
                return;
            }
            if (++b.stillSteps < rm43UnstickSteps) return;
            b.tellAt = rm43TellSteps;
            rooms43UnstickTells++;
            events.Emit("navTell43", h.id);
        }

        partial void N43RoomLeftCount(ref int n)
        {
            if (rm43On && campaignOn && campaign != null) n = campaign.RoomLeft43();
        }
    }
}
