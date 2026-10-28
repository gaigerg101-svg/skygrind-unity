// NIGHT74 builder L (L2; NIGHT74.md section 6; slate L2): the Player phase `reveal74`, reveal74's proof (gg-proxy-close #1: foundry-b1 reads "11 LEFT" with no enemy in the frame). It enters a campaign room as tour65 does (the card's
// first pose, the first wave up and settled), freezes the enemies where the wave put them, turns the camera to a heading from which no live enemy is on screen (the 36 headings, the one farthest from every enemy's bearing first,
// confirmed by the reels' L56Count: viewport and line of sight), and HOLDS it for UnityTuning.N74RevealHoldS (4.5 s on the fixed 60 fps clock) while Reveal74's view runs. At UnityTuning.N74RevealShotAtS (4.0 s) it takes the
// FACING-AWAY frame (the pair's frame: the chevron ON, nothing OFF); then it turns the camera straight at the nearest enemy and counts the frames until the chevron is gone (the slate: hidden within 0.2 s), and takes the FACING frame.
//   boot one Player per level (L-062):  bash Tools/night/n74-reels.sh <tag> <side>,n52-foundry,reveal74[,<extra args>]
//   the ON side:  ...,reveal74,-skygrindRows72,reveal74:1      the OFF side: plain (the row is OFF in the config)      another room: ...,reveal74,-skygrindReveal74Room=a2
// The phase prints its input (L-014 / L-115): the room, the pose, the enemies alive, the heading and how many were on screen at it (0), the row's state; the view prints `[reveal74] shown dir=<deg> ...` once it draws and
// `[reveal74] hidden <s> s after an enemy came on screen` once it is gone (nothing at all with the row OFF). The line: `done: GREEN reveal74 | ...` when the row is ON, the chevron came up between 2.9 s and 3.6 s into the hold
// (the 3 s delay plus the placement frames), the frame at 4 s has it drawn, and it was gone within 0.2 s of the camera turning to face an enemy; RED otherwise (RED with the row OFF: nothing came up). A reel phase only:
// it changes nothing in the game.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>The room switch: `-skygrindReveal74Room=a2` (default b1, else the first fight room with a wave).</summary>
        public const string RevealRoomSwitch74 = "-skygrindReveal74Room";

        static partial void N74RevealBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Reveal74Scenario(g, dir, res, offs, pre);

        static string Reveal74Room()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i].StartsWith(RevealRoomSwitch74 + "=", StringComparison.OrdinalIgnoreCase)) return args[i].Substring(RevealRoomSwitch74.Length + 1).Trim();
                if (string.Equals(args[i], RevealRoomSwitch74, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) return args[i + 1].Trim();
            }
            return "b1";
        }

        static double WrapRad(double a)
        {
            while (a > Math.PI) a -= 2 * Math.PI;
            while (a < -Math.PI) a += 2 * Math.PI;
            return a;
        }

        static IEnumerator Reveal74Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with reel.sh + -skygrindMap <id> (bash Tools/night/n74-reels.sh <tag> r1,n52-foundry,reveal74; L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            FeelClock.Begin(); // the fixed 60 fps clock: every frame is 1 / 60 s of game time
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED reveal74: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            yield return Frames(pre);
            bool rowOn = NightRows.ViewOn(g.cfg, Reveal74Math.Row);
            string wantRoom = Reveal74Room();
            RoomDef room = null;
            foreach (RoomDef r in lv.rooms) if (r.id == wantRoom) { room = r; break; }
            if (room == null)
                foreach (RoomDef r in lv.rooms) if (r.kind == "room" && r.WaveCount > 0) { room = r; break; }
            if (room == null) { res[0] = $"done: RED reveal74: level {id} has no room '{wantRoom}' and no fight room with a wave ({boot})"; yield break; }
            var rows = Levels35Rows(g);
            rows.TryGetValue(room.id, out var row);
            List<Tour65.Pose> poses = Tour65.Poses(row, out string source);
            if (poses.Count == 0) { res[0] = $"done: RED reveal74: room {room.id} has no card pose (frames65 / frames36.mid) to stand at ({boot})"; yield break; }
            Tour65.Pose p = poses[0];
            Debug.Log("[reel] reveal74: input: room " + room.id + " of level " + id + ", pose " + p.name + " (" + p.source + ") feet (" + p.x.ToString("0.0", ci) + ", " + p.y.ToString("0.0", ci) + ", " + p.z.ToString("0.0", ci)
                + "), hold " + UnityTuning.N74RevealHoldS.ToString("0.0", ci) + " s facing away from every enemy, the frame at " + UnityTuning.N74RevealShotAtS.ToString("0.0", ci) + " s, then turn to face the nearest enemy; delay " + UnityTuning.N74RevealDelayS.ToString("0.0", ci)
                + " s, reveal74 " + (rowOn ? "ON" : "off") + " (-skygrindRows72 reveal74:1 turns it on)");
            Reveal74Probe.Reset();

            // enter the room as tour65 does: the first wave up and settled
            g.husks.frozen = false;
            g.husks.KillAll();
            yield return Frames(4);
            K52Place(g, p.Spot);
            float end = Time.realtimeSinceStartup + UnityTuning.L56RoomWaitS;
            while (K52Room(g) != room.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
            double t0 = sim.SimTime;
            while (room.WaveCount > 0 && sim.enemies.AliveCount == 0 && sim.SimTime - t0 < UnityTuning.L56WaveWaitS && Ended30(g) == null) { God(g); yield return null; }
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < UnityTuning.L56SettleS && Ended30(g) == null) { God(g); yield return null; }
            string inRoom = K52Room(g);
            var live = new List<Enemy>();
            foreach (Enemy e in sim.enemies.all) if (e.alive && !e.standIn29) live.Add(e);
            if (live.Count == 0 || Ended30(g) != null)
            {
                g.husks.KillAll();
                g.forcedMove = null;
                yield return Stop();
                res[0] = $"done: RED reveal74: room {room.id} has no live enemy to point at (alive {live.Count}, campaign room {inRoom}{(Ended30(g) != null ? ", ended: " + Ended30(g) : "")})";
                yield break;
            }

            // the heading with no enemy on screen: the 36 headings, the farthest from every enemy's bearing first
            Vec3 feet = g.Feet;
            var cands = new List<(double yaw, double sep)>();
            for (int k = 0; k < 36; k++)
            {
                double yaw = k * Math.PI * 2 / 36, sep = double.MaxValue;
                foreach (Enemy e in live)
                {
                    double yawE = Math.Atan2(-(e.position.x - feet.x), -(e.position.z - feet.z));
                    sep = Math.Min(sep, Math.Abs(WrapRad(yaw - yawE)));
                }
                cands.Add((yaw, sep));
            }
            cands.Sort((a, b) => b.sep.CompareTo(a.sep));
            double awayYaw = double.NaN, awaySep = 0;
            int alive = 0, seenAway = -1, tried = 0;
            for (int c = 0; c < Math.Min(8, cands.Count) && double.IsNaN(awayYaw); c++)
            {
                tried++;
                g.husks.frozen = false;
                K52Place(g, new[] { p.x, p.y, p.z, cands[c].yaw * 180 / Math.PI, 0.0 });
                yield return null; // the respawn's camera cut lands
                yield return null;
                g.husks.frozen = true; // the enemies stand where the wave put them
                God(g);
                L56Count(g, out alive, out seenAway, out _);
                if (seenAway == 0) { awayYaw = cands[c].yaw; awaySep = cands[c].sep; }
            }
            if (double.IsNaN(awayYaw))
            {
                g.husks.KillAll();
                g.forcedMove = null;
                yield return Stop();
                res[0] = $"done: RED reveal74: no heading hides all {alive} enemies of room {room.id} (tried {tried}, the last saw {seenAway}): the room is too open for the proof; try -skygrindReveal74Room=<another room>";
                yield break;
            }
            Debug.Log("[reel] reveal74: heading " + (awayYaw * 180 / Math.PI).ToString("0", ci) + " deg, " + (awaySep * 180 / Math.PI).ToString("0", ci) + " deg from the nearest enemy's bearing, alive " + alive + ", on screen " + seenAway + " (tried " + tried + ")");

            // hold facing away: the chevron should come up after the delay; the frame at 4 s is the pair's frame
            var st = new double[4];
            int holdFrames = (int)Math.Round(UnityTuning.N74RevealHoldS * UnityTuning.ReelFps), shotFrame = (int)Math.Round(UnityTuning.N74RevealShotAtS * UnityTuning.ReelFps);
            int shows0 = Reveal74Probe.Shows;
            double shownAt = -1, shotAlpha = 0, shotAngle = 0, shotDir = 0;
            bool shotVisible = false;
            string left = "?";
            for (int k = 0; k < holdFrames; k++)
            {
                God(g);
                g.UiView?.ClearBanners41();
                yield return null;
                if (shownAt < 0 && Reveal74Probe.Shows > shows0) shownAt = (k + 1) / (double)UnityTuning.ReelFps;
                if (k + 1 == shotFrame)
                {
                    shotVisible = Reveal74Probe.Visible;
                    shotAlpha = Reveal74Probe.Alpha;
                    shotAngle = Reveal74Probe.AngleDeg;
                    shotDir = Reveal74Probe.DirDeg;
                    left = Reveal74Probe.Left.ToString(ci);
                    yield return Shot29(dir, $"R74-{id}-{room.id}-away", st);
                }
            }
            Debug.Log("[reel] reveal74: " + Reveal74Probe.Line());

            // turn to face the nearest enemy: the chevron should go within 0.2 s
            Enemy nearest = null;
            double nd = double.MaxValue;
            Vec3 eye = g.Eye;
            foreach (Enemy e in live)
            {
                double d = (e.position + new Vec3(0, UnityTuning.L56BodyH, 0) - eye).Length;
                if (e.alive && d < nd) { nd = d; nearest = e; }
            }
            double goneS = -1;
            int seenFacing = 0, hides0 = Reveal74Probe.Hides;
            if (nearest != null)
            {
                Vec3 to = nearest.position + new Vec3(0, UnityTuning.L56BodyH, 0) - eye;
                double fyaw = Math.Atan2(-to.x, -to.z), fpitch = Math.Atan2(to.y, Math.Sqrt(to.x * to.x + to.z * to.z));
                g.input.SetLook(fyaw, fpitch);
                g.forcedMove = new InputFrame { yaw = fyaw, pitch = fpitch };
                int frames = 0;
                for (int k = 0; k < UnityTuning.N74RevealTurnFrames; k++)
                {
                    God(g);
                    yield return null;
                    frames++;
                    if (Reveal74Probe.Hides > hides0) break;
                }
                goneS = Reveal74Probe.Hides > hides0 ? frames / (double)UnityTuning.ReelFps : -1;
                yield return Frames(8);
                L56Count(g, out _, out seenFacing, out _);
                yield return Shot29(dir, $"R74-{id}-{room.id}-facing", st);
            }
            Debug.Log("[reel] reveal74: " + Reveal74Probe.Line());
            g.husks.frozen = false;
            g.husks.KillAll();
            g.forcedMove = null;
            yield return Stop();

            bool timing = shownAt >= UnityTuning.N74RevealDelayS - 0.1 && shownAt <= UnityTuning.N74RevealDelayS + 0.6;
            bool gone = goneS >= 0 && goneS <= 0.2 && Reveal74Probe.GoneAfterS >= 0 && Reveal74Probe.GoneAfterS <= 0.2;
            bool green = rowOn && timing && shotVisible && shotAlpha > 0.9 && gone && seenFacing > 0 && inRoom == room.id;
            string why = !rowOn ? "the row is OFF: nothing is drawn" : shownAt < 0 ? "the chevron never came up" : !timing ? $"it came up at {shownAt.ToString("0.00", ci)} s, not within the delay's window" : !shotVisible ? "it was not drawn at 4 s" : seenFacing <= 0 ? "no enemy on screen after the turn" : !gone ? "it was not gone within 0.2 s of the turn" : inRoom != room.id ? "the camera was in the wrong room" : "";
            res[0] = $"done: {(green ? "GREEN" : "RED")} reveal74 | map {g.MapId}, level {id}, room {room.id} (campaign {inRoom}) | row {(rowOn ? "ON" : "off")} | alive {alive}, on screen facing away {seenAway} (heading {(awayYaw * 180 / Math.PI).ToString("0", ci)} deg, tried {tried})"
                + $" | counter N LEFT {left} | shown at {(shownAt >= 0 ? shownAt.ToString("0.00", ci) + " s" : "never")} into the hold (delay {UnityTuning.N74RevealDelayS.ToString("0.0", ci)} s) | at 4 s: drawn {shotVisible}, alpha {shotAlpha.ToString("0.00", ci)}, dir {shotDir.ToString("0", ci)} deg, chevron angle {shotAngle.ToString("0", ci)} deg"
                + $" | turned to face the nearest enemy ({nd.ToString("0.0", ci)} m): on screen {seenFacing}, gone after {(goneS >= 0 ? goneS.ToString("0.000", ci) + " s" : "never")} (the view says {Reveal74Probe.GoneAfterS.ToString("0.000", ci)} s)"
                + (why.Length > 0 ? " | " + why : "")
                + $" | shows {Reveal74Probe.Shows}, hides {Reveal74Probe.Hides} | frames R74-{id}-{room.id}-away / -facing";
        }
    }
}
