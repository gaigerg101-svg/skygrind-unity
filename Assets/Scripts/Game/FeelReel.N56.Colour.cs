// NIGHT56 builder V: the a356 phase (FeelReel.N56.cs routes it here), the colour reel: the frames n56-colour.py measures for V1 colour56 (each
// level's key and accent), V2 rim56 (the enemies the warmest and brightest) and the yellow on the world (V3's own frames are verbs37 x5, any reel).
// Not run by its author (builders never run a Player). One level a reel (L-062), the row(s) on in the build copy:
//   bash Tools/night/n56-reels.sh <tag> v1on,n52-foundry,a356     and   v1off,n52-foundry,a356,-skygrindOff56      (each of levels 1-5)
// a452's run (FeelReel.N52.Rooms.cs: the campaign, DOORFREE, God, nothing pressed; every room with a frames52 object, in room order): the ENTRY frame
// (the camera at frames52.entry once the room's first wave is up, + 0.5 s) and the MID frame (frames52.mid after 1.5 s more of the fight). Each room
// records its own telemetry (dir/<room>/: FeelProbe's 1800-frame ring cut a452's iceline at its 6th room), so every shot has its line.
// Each shot is dir/<room>/shot-N56-<level>-<room>-<entry|mid>.png with a SIDECAR shot-N56-....json that the meter reads: the telemetry frame index
// of the shot ("f": its HUD ui[] rects, the enemies' rects, the gun's rect), the horizon row (the camera's level forward projected) and the SKY GRID
// (one ray per 20 x 20 px cell through the C# world: no hit within Colour56SkyRayM = open sky; upper walls and ceilings with a collider stay world).
// One line per shot, exactly `[reel] a356: <room> <entry|mid> <file>`, and `done: ...`: GREEN when every framed room gave its two shots and
// sidecars with the campaign in that room, the screen is 1920x1080, and the rows' input happened (colour56 on: the frieze built and the accent
// colour56's; with -skygrindOff56 the line says OFF and stays RED: the comparison side); the numbers are n56-colour.py's on these folders:
//   python -I Tools/night/n56-colour.py Logs/feel/<tag>/v1on/a356 --kinds entry,mid      (and the off side's folder)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N56Colour(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = C56Scenario(g, dir, res, offs, pre);

        static IEnumerator C56Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot the level with n56-reels.sh <tag> <side>,<level>,a356 (reel.sh + -skygrindMap <id> -skygrindReelOnly a356), the rows on in the build copy (L-062)";
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; });
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "done: RED a356: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + "; " + boot + ")"; yield break; }
            LevelDef lv = sim.campaignLevel;
            string id = lv?.id ?? "?";
            God(g);
            g.forcedCombat = IdleCombat;
            var rows = Levels35Rows(g);
            var shots = new List<string>();
            var unframed = new List<string>();
            int rooms = 0, framed = 0, want = 0, wrongRoom = 0, noWave = 0, sidecars = 0;
            string ended = null;
            foreach (RoomDef r in lv.rooms)
            {
                double[] fe = K52Frame(rows, r.id, "entry"), fm = K52Frame(rows, r.id, "mid");
                if (fe == null || fm == null) { if (r.kind == "room") unframed.Add(r.id); continue; }
                if (r.kind == "boss") continue;
                rooms++;
                want += 2;
                g.husks.KillAll();
                yield return Frames(4);
                string rdir = Path.Combine(dir, r.id);
                Start(g, "a356", "camera", rdir);
                yield return Frames(pre);
                FeelProbe.MarkInput();
                K52Place(g, fe);
                float end = Time.realtimeSinceStartup + 6;
                while (K52Room(g) != r.id && Time.realtimeSinceStartup < end) { God(g); yield return null; }
                double t0 = sim.SimTime;
                while (sim.enemies.AliveCount == 0 && sim.SimTime - t0 < 8 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (sim.enemies.AliveCount == 0) noWave++;
                double t1 = sim.SimTime;
                while (sim.SimTime - t1 < 0.5 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                if (ended != null) { yield return Stop(); break; }
                g.UiView?.ClearBanners41();
                if (K52Room(g) != r.id) wrongRoom++;
                for (int k = 0; k < 2 && ended == null; k++)
                {
                    if (k == 1)
                    {
                        K52Place(g, fm);
                        double t2 = sim.SimTime;
                        while (sim.SimTime - t2 < 1.5 && (ended = Ended30(g)) == null) { God(g); yield return null; }
                        if (ended != null) break;
                    }
                    God(g);
                    string kind = k == 0 ? "entry" : "mid";
                    string name = $"N56-{id}-{r.id}-{kind}";
                    var side = new bool[1];
                    yield return C56Shot(g, rdir, name, id, r.id, kind, side);
                    framed++;
                    if (side[0]) sidecars++;
                    Debug.Log($"[reel] a356: {r.id} {kind} shot-{name}.png");
                    K52Count(g, (k == 0 ? fe : fm)[3], out int alive, out int inView);
                    shots.Add($"{r.id} {kind}: campaign room {K52Room(g)}, alive {alive}, in view {inView}");
                }
                yield return Stop();
                if (ended != null) break;
            }
            g.husks.KillAll();
            g.forcedMove = null;
            bool screen = Screen.width == 1920 && Screen.height == 1080;
            bool c56 = Colour56.On(g.cfg);
            bool input = c56 && Colour56.Built && Colour56.Segments > 0;
            bool green = rooms > 0 && framed == want && sidecars == framed && wrongRoom == 0 && noWave == 0 && screen && ended == null && input;
            res[0] = $"done: {(green ? "GREEN" : "RED")} a356 | map {g.MapId}, level {id} | rows: colour56 {(c56 ? "ON" : "OFF")}, rim56 {(Rim56.On(g.cfg) ? "ON" : "OFF")} (skin52's globals written this frame: {(Rim56.Live ? "yes" : "no")}; the enemy material's far rim lifted: {(Rim56.RimLifted ? "yes" : "no")}), yellow56 {(Yellow56.On(g.cfg) ? "ON" : "OFF")} (gun plates moved off the yellow arc {Yellow56.GunsMoved}{(Yellow56.LastGun.Length > 0 ? ": last " + Yellow56.LastGun : "")}; floor vertices moved {Yellow56Floors.Moved} of {Yellow56Floors.Seen} in {Yellow56Floors.Passes} pass(es))"
                + $" | the input: {Colour56.Line} || {Colour56.Look49Line} || {Decor55.Line}"
                + $" | rooms framed {rooms}, shots {framed}/{want}, sidecars {sidecars}, entry frames in the wrong room {wrongRoom}, entries with no wave {noWave}"
                + $" | screen {Screen.width}x{Screen.height}{(screen ? "" : " (NOT 1920x1080)")}{(ended != null ? " | ended: " + ended : "")}"
                + $" | fight rooms without frames52 (not framed): {(unframed.Count > 0 ? string.Join(", ", unframed) : "none")}"
                + " | the numbers: python -I Tools/night/n56-colour.py <this side's a356 folder> --kinds entry,mid || " + string.Join(" || ", shots);
        }

        /// <summary>The shot (PNG, as Shot29) and its sidecar: the telemetry frame index, the horizon row, the sky grid, the enemies' rects now.</summary>
        static IEnumerator C56Shot(SurvivalGame g, string dir, string name, string level, string room, string kind, bool[] wrote)
        {
            yield return new WaitForEndOfFrame();
            int f = FeelProbe.FrameCount - 1; // FeelProbe.EndFrame recorded this frame in RenderFrame; the capture below is the same frame
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-" + name + ".png"), t.EncodeToPNG());
                int w = t.width, h = t.height;
                var sb = new StringBuilder(8192);
                CultureInfo ci = CultureInfo.InvariantCulture;
                sb.Append("{\"level\":\"").Append(level).Append("\",\"room\":\"").Append(room).Append("\",\"kind\":\"").Append(kind).Append("\",\"f\":").Append(f);
                sb.Append(",\"w\":").Append(w).Append(",\"h\":").Append(h);
                Camera cam = g.cam;
                if (cam != null)
                {
                    Vector3 fw = cam.transform.forward;
                    fw.y = 0;
                    if (fw.sqrMagnitude > 1e-6f)
                    {
                        Vector3 s = cam.WorldToScreenPoint(cam.transform.position + fw.normalized * 1000f);
                        if (s.z > 0) sb.Append(",\"horizonY\":").Append(((double)h - s.y * h / Screen.height).ToString("0.0", ci));
                    }
                    int cols = UnityTuning.Colour56SkyCols, rws = UnityTuning.Colour56SkyRows, open = 0;
                    var bits = new StringBuilder(cols * rws);
                    for (int cy = 0; cy < rws; cy++)
                        for (int cx = 0; cx < cols; cx++)
                        {
                            // the cell's centre (image rows top-down; Unity's screen y runs from the bottom)
                            float sx = (cx + 0.5f) * Screen.width / cols, sy = Screen.height - (cy + 0.5f) * Screen.height / rws;
                            Ray ray = cam.ScreenPointToRay(new Vector3(sx, sy, 0));
                            bool hit = g.world.RayIntersect(WebSpace.FromUnity(ray.origin), WebSpace.FromUnity(ray.direction), out RayHit rh) && rh.distance < UnityTuning.Colour56SkyRayM;
                            bits.Append(hit ? '0' : '1');
                            if (!hit) open++;
                        }
                    sb.Append(",\"skyGrid\":{\"cols\":").Append(cols).Append(",\"rows\":").Append(rws).Append(",\"open\":").Append(open).Append(",\"bits\":\"").Append(bits).Append("\"}");
                }
                sb.Append(",\"colour56\":").Append(Colour56.On(g.cfg) ? "true" : "false").Append('}');
                File.WriteAllText(Path.Combine(dir, "shot-" + name + ".json"), sb.ToString());
                wrote[0] = true;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
            // round 2: the frieze's A/B (colour56 on): the same view one frame later with the frieze hidden (shot-<name>-nofrieze.png); the meter reads
            // the frieze's exact pixels by the difference (no role pass): its coverage, its rendered hue and saturation
            if (Colour56.Built)
            {
                Colour56.SetFriezeVisible(false);
                yield return null;
                yield return new WaitForEndOfFrame();
                Texture2D t2 = null;
                try
                {
                    t2 = ScreenCapture.CaptureScreenshotAsTexture();
                    if (t2 != null) File.WriteAllBytes(Path.Combine(dir, "shot-" + name + "-nofrieze.png"), t2.EncodeToPNG());
                }
                finally
                {
                    if (t2 != null) UnityEngine.Object.Destroy(t2);
                    Colour56.SetFriezeVisible(true);
                }
            }
            // NIGHT57 V (a357, FeelReel.N57.Colour.cs): while colour56's world saturation cap is live, the -nocap twin (nothing when it is off)
            yield return C57NoCapTwin(dir, name);
        }
    }
}
