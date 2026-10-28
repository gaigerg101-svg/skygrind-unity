// NIGHT33 L (design.n33.look33): the Player scenario of "every light has a fixture" (the lead's FeelReel.N27.cs dispatches PhaseN33L to N33LookBody).
//   look33   boot -skygrindMap n32-foundry | n32-iceline | n32-kiln | n32-span | n32-spire (a CAMPAIGN level). God mode, DOORFREE, enemies frozen. For each named room of
//            the booted level (the rooms where NIGHT32 had floating lights: lava trenches, pool sockets; Look33Rooms): respawn at the room's entry facing in and take
//            frame -a at eye height (pitch 0) and frame -b looking up at the ceiling (pitch Look33UpPitch: the fixtures, panels, strips). Per frame: the mean Rec.709
//            luminance of the whole frame, % pixels at 0.98 or more, % dark pixels (under 0.08), and the orange share of saturated pixels (S over 0.4) with the
//            cyan share beside it. The frame checks (L1 section 5 / DESIGN33 L): mean 0.10-0.30, <= 3 % at 0.98+, >= 15 % dark, orange <= 35 % in a lava room.
//            Run it twice: GREEN with design.n33.look33 on, RED with -skygrindOff33 (NIGHT32's look). The line says which.
// Shots go to <dir>/shot-N33-look33-<room>-a/b.png. Every result line starts with PASS or RED. Not run by its author (no Player in the worktree).
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
        static partial void N33LookBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Look33Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The rooms look33 frames per level: lava trenches and the rooms with the most pool sockets (NIGHT32's floating pool lights and shafts).</summary>
        static readonly Dictionary<string, string[]> Look33Rooms = new Dictionary<string, string[]>
        {
            { "n32-foundry", new[] { "f2", "f9b", "f10" } },
            { "n32-iceline", new[] { "i6", "i6b" } },
            { "n32-kiln", new[] { "k2", "k2b" } },
            { "n32-span", new[] { "s4", "s7" } },
            { "n32-spire", new[] { "v2", "v8" } },
        };

        const double Look33UpPitch = 0.55; // rad: frame -b looks up at the ceiling (positive pitch is up: AssistMath.LookDir)

        /// <summary>Frame numbers: [0] mean luminance, [1] share at 0.98+, [2] share under 0.08, [3] orange share of saturated, [4] cyan share of saturated, [5] saturated share.</summary>
        static IEnumerator Shot33(string dir, string name, double[] st)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N33-" + name + ".png"), t.EncodeToPNG());
                int w = t.width, h = t.height, n = 0, hi = 0, dark = 0, sat = 0, orange = 0, cyan = 0;
                double sum = 0;
                for (int y = 0; y < h; y += 4)
                    for (int x = 0; x < w; x += 4)
                    {
                        Color c = t.GetPixel(x, y);
                        double l = 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
                        sum += l; n++;
                        if (l >= 0.98) hi++;
                        if (l < 0.08) dark++;
                        Color.RGBToHSV(c, out float hu, out float sa, out float va);
                        if (sa > 0.4f && va > 0.15f)
                        {
                            sat++;
                            float deg = hu * 360f;
                            if (deg >= 8f && deg <= 45f) orange++; // red-orange to amber (the danger hues and the lava)
                            else if (deg >= 170f && deg <= 200f) cyan++;
                        }
                    }
                st[0] = n > 0 ? sum / n : 0;
                st[1] = n > 0 ? hi / (double)n : 0;
                st[2] = n > 0 ? dark / (double)n : 0;
                st[3] = sat > 0 ? orange / (double)sat : 0;
                st[4] = sat > 0 ? cyan / (double)sat : 0;
                st[5] = n > 0 ? sat / (double)n : 0;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        /// <summary>The ambient probe evaluated straight up (linear): what Stone.shader's SampleSH gives an up-facing floor (the stone's real ambient).</summary>
        static string ProbeUp33()
        {
            var res = new Color[1];
            RenderSettings.ambientProbe.Evaluate(new[] { Vector3.up }, res);
            return $"({res[0].r:0.000}, {res[0].g:0.000}, {res[0].b:0.000})";
        }

        static string Fails33(double[] st, bool lava, CultureInfo ci)
        {
            var f = new List<string>();
            if (st[0] < 0.10 || st[0] > 0.30) f.Add($"mean {st[0].ToString("0.000", ci)} outside 0.10-0.30");
            if (st[1] > 0.03) f.Add($"{(100 * st[1]).ToString("0.0", ci)} % at 0.98+ (max 3)");
            if (st[2] < 0.15) f.Add($"{(100 * st[2]).ToString("0.0", ci)} % dark (min 15)");
            if (lava && st[3] > 0.35) f.Add($"orange {(100 * st[3]).ToString("0.0", ci)} % of saturated (max 35)");
            return string.Join(", ", f);
        }

        static IEnumerator Look33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (boot -skygrindMap n32-foundry | n32-iceline | n32-kiln | n32-span | n32-spire)"; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; g.husks.frozen = false; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (boot -skygrindMap n32-foundry | ...)"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            g.husks.frozen = true;
            LevelDef lv = sim.campaignLevel;
            if (!Look33Rooms.TryGetValue(lv.id ?? "", out string[] ids)) Look33Rooms.TryGetValue((lv.id ?? "").Replace("n33-", "n32-"), out ids); // kit33's n32 -> n33 level swap keeps the room ids
            if (ids == null) { res[0] = $"RED: level '{lv.id}' has no look33 room list (boot -skygrindMap n32-foundry | n32-iceline | n32-kiln | n32-span | n32-spire)"; yield break; }
            bool on = N33Fixes.ViewRowOn(g.cfg, "look33");
            CampaignLighting24 cl = UnityEngine.Object.FindAnyObjectByType<CampaignLighting24>();
            LightKit29 lk = UnityEngine.Object.FindAnyObjectByType<LightKit29>();
            string side = on ? "GREEN (look33 on)" : "RED (look33 off: NIGHT32's look)";
            string counts = $"look33 {on}: trims {cl?.trims33 ?? -1}, panels {cl?.panels33 ?? -1}, frames {cl?.frames33 ?? -1}, guide lamps {cl?.lamps33 ?? -1}, sockets {lk?.sockets33 ?? -1}, lava lights {lk?.lava33 ?? -1}, shafts {lk?.beams33 ?? -1}, lenses {cl?.lenses33 ?? -1}, bodies {cl?.bodies33 ?? -1}, theme {LightPlan33.Theme(sim.map.json)}, vfx embers {cl?.vfx33?.embers ?? -1} / dust {cl?.vfx33?.motes ?? -1} / sparks {cl?.vfx33?.sparks ?? -1}";
            PostStack ps = Skygrind.Game.Look.Post;
            if (ps != null) counts += $" | post: tonemap {ps.tone.mode.value}, bloom threshold {ps.bloom.threshold.value.ToString("0.00", ci)} intensity {ps.bloom.intensity.value.ToString("0.00", ci)} clamp {ps.bloom.clamp.value.ToString("0", ci)}, exposure {ps.color.postExposure.value.ToString("0.00", ci)}, saturation {ps.color.saturation.value.ToString("0", ci)}";
            counts += $" | fog {RenderSettings.fogMode} {RenderSettings.fogDensity.ToString("0.000", ci)}, ambient sky {RenderSettings.ambientSkyColor}, probe up {ProbeUp33()}, rim {Shader.GetGlobalColor("_RimColor")}";
            var lavaRooms = new HashSet<string>();
            if (sim.map.json != null && sim.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("pits", out object po) && po is List<object> pits)
                foreach (object o in pits)
                    if (o is Dictionary<string, object> p && p.TryGetValue("lava", out object lo) && lo is bool lb && lb && p.TryGetValue("zone", out object zo) && zo is string zs) lavaRooms.Add(zs);
            Start(g, "look33", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            var fails = new List<string>();
            var st = new double[6];
            int frames = 0;
            string ended = null;
            foreach (string id in ids)
            {
                RoomDef r = lv.rooms.Find(q => q.id == id);
                if (r == null) { fails.Add($"room {id} not in {lv.id}"); continue; }
                bool lava = lavaRooms.Contains(id);
                double yaw = r.spawnYawDeg * Math.PI / 180;
                foreach (var (tag, pitch) in new[] { ("a", 0.0), ("b", Look33UpPitch) })
                {
                    if (ended != null) break;
                    g.Respawn(r.spawn, r.spawnYawDeg);
                    g.input.SetLook(yaw, pitch);
                    yield return Settle31(g, 0.6, () => (ended = Ended30(g)) != null);
                    g.input.SetLook(yaw, pitch);
                    yield return null;
                    if (ended != null) break;
                    yield return Shot33(dir, $"look33-{id}-{tag}", st);
                    frames++;
                    string f = Fails33(st, lava, ci);
                    if (f != "") fails.Add($"{id}-{tag}: {f}");
                    lines.Add($"{id}-{tag}{(lava ? " (lava)" : "")}: mean {st[0].ToString("0.000", ci)}, 0.98+ {(100 * st[1]).ToString("0.0", ci)} %, dark {(100 * st[2]).ToString("0.0", ci)} %, saturated {(100 * st[5]).ToString("0.0", ci)} % (orange {(100 * st[3]).ToString("0.0", ci)} %, cyan {(100 * st[4]).ToString("0.0", ci)} %) {(f == "" ? "ok" : "OUT")}");
                }
            }
            yield return Stop();
            bool ok = fails.Count == 0 && frames == 2 * ids.Length && ended == null;
            res[0] = $"{(ok ? "PASS" : "RED")}: {side} on {lv.id}, {frames} of {2 * ids.Length} frames (shot-N33-look33-<room>-a eye height / -b the ceiling){(ended != null ? " | ended: " + ended : "")}{(fails.Count > 0 ? " | out: " + string.Join("; ", fails) : "")} | {counts} | "
                + "LOOK: -a no glowing bar over lava or a pit, the room lit from fixtures you can see; -b every light under a framed panel / lamp / strip flush on the room's own roof, no shaft standing in mid-air || "
                + string.Join(" || ", lines);
        }
    }
}
