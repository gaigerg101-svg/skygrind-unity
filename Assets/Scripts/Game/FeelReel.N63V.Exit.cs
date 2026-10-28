// NIGHT63 builder V (exit63's SPOT pose; Exit63.N63V.cs): a157's EXIT SHOT, a reel option behind `-skygrindExitShots63 [room]` that runs only
// beside `-skygrindClearShots60` (without the switch a157 is unchanged; it changes nothing in the game). Why: rule 8's SPOT needs >= 41,472 px
// changed, and a157's clear shots stand at each room's entry, 18-27 m from the exit on n57-kiln, where the door's whole box is 15,004-37,810 px
// on b3 (shot-N29-L57-n57-kiln-*-clear.json): no change inside the door can reach the rule there.
// After a room's clear shot (its exit found as L60ExitDoor finds it): the feet on the door's axis UnityTuning.Exit63ShotM into the room from
// its face, the look level at the door's centre, and the frame shot-N29-L57-<level>-<room>-exit63.png with a sidecar .json in the clear
// shot's format (the door's box and opening, image px from the top-left; its name does not end in -clear, so n60-v-lamp.py's clear-shot MAIN
// never reads it). With `-skygrindSameRun63 n63.exit63` the shot's row-off twin lands beside it (Shot29's SameRun63). One more a157 line:
//   [reel] a157: <room> exit63 shot-N29-L57-<level>-<room>-exit63.png | door <id> opened <yes|NO> | spot (<x>, <z>) <m> m out on its axis,
//   yaw <deg> | rect <x,y,w,h> | exit63 <Exit63.Line>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>a157's exit shot for room `r` (after its clear shot): see the file's head.</summary>
        static IEnumerator L63ExitShot(SurvivalGame g, RoomDef r, string level, string dir, double[] st, List<string> lines)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string want = Exit63Math.ShotRoom(Environment.GetCommandLineArgs());
            if (want == null || (want.Length > 0 && want != r.id)) yield break;
            if (!L60ExitDoor(g, r.id, out string door, out double[] rect, out double h)) yield break; // the clear shot said so already
            if (!g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) yield break;
            var rooms = Doors38Math.Rooms(camp);
            string b = L63RoomB(camp, door);
            double[] ra = rooms.TryGetValue(r.id, out var rowA) ? rowA.rect : null;
            double[] rb = b != null && rooms.TryGetValue(b, out var rowB) ? rowB.rect : null;
            bool spansX = Doors38Math.SpansX(rect, ra, rb);
            var (ax, az) = Doors38Math.Ahead(rect, rb, spansX);
            ExitFrame61Math.Door d = ExitFrame61Math.DoorOf(rect, h, ax, az, spansX);
            Vec3 at = Exit63Math.ShotSpot(d, UnityTuning.Exit63ShotM);
            double yaw = Exit63Math.ShotYawDeg(d, at);
            K52Place(g, new[] { at.x, at.y, at.z, yaw, 0.0 });
            yield return null; // the respawn's camera cut lands
            yield return null; // one rendered frame at the spot
            God(g);
            g.UiView?.ClearBanners41();
            Zones zones = g.sim.zones;
            bool opened = zones == null || zones.IsOpen(door);
            double grow = UnityTuning.Exit58FrameOut + UnityTuning.Exit58FrameW;
            double[] box = L60ScreenRect(rect, grow, h + grow);
            double[] opening = L60ScreenRect(rect, 0, h);
            string name = $"L57-{level}-{r.id}-exit63";
            yield return Shot29(dir, name, st);
            Func<double[], string> f = q => q == null ? "null" : "[" + string.Join(",", Array.ConvertAll(q, v => v.ToString("0.0", ci))) + "]";
            string spot = "(" + at.x.ToString("0.0", ci) + ", " + at.z.ToString("0.0", ci) + ")";
            string json = "{\"room\":\"" + r.id + "\",\"door\":\"" + door + "\",\"opened\":" + (opened ? "true" : "false") + ",\"spot\":[" + at.x.ToString("0.00", ci)
                + "," + at.z.ToString("0.00", ci) + "],\"distM\":" + UnityTuning.Exit63ShotM.ToString("0.0", ci) + ",\"yawDeg\":" + yaw.ToString("0.0", ci)
                + ",\"box\":" + f(box) + ",\"opening\":" + f(opening) + ",\"w\":" + Screen.width + ",\"h\":" + Screen.height
                + ",\"exit63\":\"" + Exit63.Line.Replace("\"", "'") + "\"}";
            File.WriteAllText(Path.Combine(dir, "shot-N29-" + name + ".json"), json);
            string line = $"{r.id} exit63 shot-N29-{name}.png | door {door} opened {(opened ? "yes" : "NO")} | spot {spot} {UnityTuning.Exit63ShotM.ToString("0", ci)} m out on its axis,"
                + $" yaw {yaw.ToString("0", ci)} | rect {f(box)} | exit63 {Exit63.Line}";
            Debug.Log("[reel] a157: " + line);
            lines.Add(line);
        }

        /// <summary>The room a campaign door leads into (its between[1]), null when the door or the field is missing.</summary>
        static string L63RoomB(Dictionary<string, object> camp, string door)
        {
            if (!camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors)) return null;
            foreach (object o in doors)
                if (o is Dictionary<string, object> d && d.TryGetValue("id", out object io) && io as string == door
                    && d.TryGetValue("between", out object bo) && bo is List<object> bl && bl.Count >= 2)
                    return bl[1] as string;
            return null;
        }
    }
}
