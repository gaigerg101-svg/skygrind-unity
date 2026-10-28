// NIGHT60 builder V (B8, the exit lamp; design.n60.lamp60): a157's CLEAR SHOT, a reel option behind `-skygrindClearShots60` (without the flag a157
// is unchanged; it changes nothing in the game). For each fight room a157 frames, after its entry (and spawn) shot: the room's exit (the gated
// campaign door whose `between` starts at this room, not into a boss room, as Exit58 picks them) is found, the room is cleared (every wave killed
// until the campaign, in this room, reads phase Cleared, or UnityTuning.Lamp60ClearWaitS: a157 boots DOORFREE, so the doors are open from the start
// and only the room's own clear says it is cleared), the lamp's rise (Exit58FadeS) and Lamp60SettleS pass, the camera stands at the room's entry
// spot looking level at the door's centre, and the frame is saved as shot-N29-L57-<level>-<room>-clear.png with a sidecar .json: the door's
// screen rect (the opening and the lamp frame's box, image px from the top-left, [x, y, w, h]), opened, cleared, alive, the lamp's line.
// One more a157 line per room:
//   [reel] a157: <room> clear shot-N29-L57-<level>-<room>-clear.png | room cleared <yes|NO> (phase <p>, alive <n>) | door <id> opened <yes|NO>
//   | rect <x,y,w,h> | lamp <Exit58.Line>
// The numbers: python -I Tools/night/n60-v-lamp.py <on a157 folder> <off a157 folder> (lamp60 on vs -skygrindNoExit58, the same build).
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
        /// <summary>NIGHT60 (B8): `-skygrindClearShots60` adds each room's clear frame (its exit door lit or not) to a157.</summary>
        static bool ClearShots60 => Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindClearShots60") >= 0;

        /// <summary>The room's exit as Exit58 lamps it: a gated campaign door leaving `room`, not into a boss room. Its rect (web x0, z0, x1, z1) and height.</summary>
        static bool L60ExitDoor(SurvivalGame g, string room, out string id, out double[] rect, out double h)
        {
            id = null; rect = null; h = 4;
            if (g.map?.json == null || !g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)
                || !camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors)) return false;
            var rooms = Doors38Math.Rooms(camp);
            foreach (object o in doors)
            {
                if (!(o is Dictionary<string, object> d) || !d.TryGetValue("id", out object io) || !(io is string did)) continue;
                if (!d.TryGetValue("between", out object bo) || !(bo is List<object> bl) || bl.Count < 2 || bl[0] as string != room) continue;
                if (g.map.ZoneIndex(did) < 0) continue;
                if (bl[1] is string b && rooms.TryGetValue(b, out var rb) && rb.kind == "boss") continue;
                double[] r = d.TryGetValue("rect", out object ro) ? Doors38Math.Rect(ro) : null;
                if (r == null || r[2] <= r[0] || r[3] <= r[1]) continue;
                id = did;
                rect = r;
                h = d.TryGetValue("height", out object ho) ? PortData.Num(ho) : 4;
                return true;
            }
            return false;
        }

        /// <summary>A world box (web x0, z0, x1, z1 x 0..top) on the screen as an image rect [x, y, w, h] from the top-left; null when behind the camera.</summary>
        static double[] L60ScreenRect(double[] r, double grow, double top)
        {
            Vector3 a = WebSpace.ToUnity(new Vec3(r[0] - grow, 0, r[1] - grow)), b = WebSpace.ToUnity(new Vec3(r[2] + grow, top, r[3] + grow));
            var bounds = new Bounds((a + b) * 0.5f, new Vector3(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y), Mathf.Abs(b.z - a.z)));
            if (!FeelProbe.ScreenRect(bounds, out Rect s)) return null;
            return new double[] { s.xMin, Screen.height - s.yMax, s.width, s.height };
        }

        /// <summary>a157's clear shot for room `r` (after its entry shot; the camera back at the entry spot `fe`).</summary>
        static IEnumerator L60ClearShot(SurvivalGame g, RoomDef r, double[] fe, string level, string dir, double[] st, List<string> lines)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            if (!L60ExitDoor(g, r.id, out string door, out double[] rect, out double h))
            {
                string none = $"{r.id} clear: no gated exit door leaving this room (Exit58 lamps none here)";
                Debug.Log("[reel] a157: " + none);
                lines.Add(none);
                yield break;
            }
            Zones zones = g.sim.zones;
            // B8 attempt 2: a157 boots DOORFREE (Campaign41's doorFree: every door opens at the start, Exit58's lamps lit at frame ~272), so "until the
            // door opens" ended at once and the shot caught the wave alive ("15 LEFT"): the room's own clear (the campaign in this room, phase Cleared) is
            // the condition now, every wave killed until it
            Campaign24 camp = g.sim.campaign;
            int ri = g.sim.campaignLevel != null ? g.sim.campaignLevel.rooms.IndexOf(r) : -1;
            float end = Time.realtimeSinceStartup + UnityTuning.Lamp60ClearWaitS;
            while (!(camp != null && camp.room == ri && camp.phase == Campaign24.Phase.Cleared) && Time.realtimeSinceStartup < end && Ended30(g) == null)
            {
                g.husks.KillAll();
                God(g);
                yield return null;
            }
            bool cleared = camp != null && camp.room == ri && camp.phase == Campaign24.Phase.Cleared;
            bool opened = zones == null || zones.IsOpen(door);
            float settle = Time.realtimeSinceStartup + UnityTuning.Exit58FadeS + UnityTuning.Lamp60SettleS;
            while (Time.realtimeSinceStartup < settle) { God(g); yield return null; }
            double cx = (rect[0] + rect[2]) / 2, cz = (rect[1] + rect[3]) / 2, dx = cx - fe[0], dz = cz - fe[2];
            K52Place(g, new[] { fe[0], fe[1], fe[2], Math.Atan2(-dx, -dz) * 180 / Math.PI, 0.0 }); // web yaw: 0 faces -z, + turns toward -x
            yield return null; // the respawn's camera cut lands
            yield return null; // one rendered frame at the spot
            God(g);
            g.UiView?.ClearBanners41();
            double grow = UnityTuning.Exit58FrameOut + UnityTuning.Exit58FrameW;
            double[] box = L60ScreenRect(rect, grow, h + grow);
            double[] opening = L60ScreenRect(rect, 0, h);
            string name = $"L57-{level}-{r.id}-clear";
            yield return Shot29(dir, name, st);
            Func<double[], string> f = q => q == null ? "null" : "[" + string.Join(",", Array.ConvertAll(q, v => v.ToString("0.0", ci))) + "]";
            int alive = g.sim.enemies.AliveCount;
            string json = "{\"room\":\"" + r.id + "\",\"door\":\"" + door + "\",\"opened\":" + (opened ? "true" : "false") + ",\"cleared\":" + (cleared ? "true" : "false")
                + ",\"alive\":" + alive + ",\"box\":" + f(box) + ",\"opening\":" + f(opening)
                + ",\"w\":" + Screen.width + ",\"h\":" + Screen.height + ",\"lamp\":\"" + Exit58.Line.Replace("\"", "'") + "\"}";
            File.WriteAllText(Path.Combine(dir, "shot-N29-" + name + ".json"), json);
            string line = $"{r.id} clear shot-N29-{name}.png | room cleared {(cleared ? "yes" : "NO")} (phase {camp?.phase.ToString() ?? "none"}, alive {alive}) | door {door} opened {(opened ? "yes" : "NO")} | rect {f(box)} | lamp {Exit58.Line}";
            Debug.Log("[reel] a157: " + line);
            lines.Add(line);
        }
    }
}
