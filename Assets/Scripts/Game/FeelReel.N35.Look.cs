// NIGHT35 builder V (NIGHT35.md V1; rows design.n35.look35 / lights35 / tv35 / ridelook35): the Player scenario `look35` (the lead's FeelReel.N27.cs
// dispatches PhaseN35V to N35LookBody). Not run by its author (no Player in a worktree): the lead runs it, one Player per run (L-046). Input is the
// reel's own (g.forcedMove / g.forcedCombat / g.input.SetLook), never a device.
//   look35  boot -skygrindMap <level 1, e.g. n34-foundry> (GREEN) and again with -skygrindOff35 (RED: NIGHT34's look). A campaign run, DOORFREE, god
//           mode, the window set to 1920 x 1080. Frames (shot-N35-look35-<name>.png in the reel dir, each with its sampled numbers):
//     room      the level's first fight room from its spawn, facing in (the PS2 pass on a level-1 room)
//     lit       the room with the most fixture lights, from its spawn, aimed at its key light (the fixtures, the bloom, the sky above an open room)
//     hud-hit-1 a hit taken (12 damage from 3 m ahead), 2 frames after it; hud-hit-2 the same hit 8 frames after it (the HUD's jiggle / bounce / red)
//     ride      a REAL wall ride + zoom: stood 0.9 m off the room's longest clear wall, run along it, jump into it, LT (zoom) held while riding; the shot
//               once the focus look is full (SlowAim27.Focus35Now >= 0.95). No ride within 3 tries: the look is FORCED (SlowAim27.Force35) for a
//               ride-forced frame and the line says so (the look checked, the trigger not).
//   Per frame a blockiness number: the share of equal horizontal neighbour pixels in the frame's middle (point upscale: high; NIGHT34: low).
//   One `[reel] look35:` line: PASS when every V row that is ON shows its effect (look35: 480-ish lines, 32 levels at full strength, scanlines >= 0.25;
//   lights35: 0 fixtures in the sky of an open room; tv35: the hit kicked the HUD; ridelook35: the focus look full in the ride frame), else RED.
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
        static partial void N35LookBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "look35") body = Look35Scenario(g, dir, res, offs, pre);
        }

        /// <summary>A frame (shot-N35-look35-name.png) and its numbers: [0] blockiness (equal horizontal neighbours, middle 60 %), [1] mid luminance,
        /// [2] blue share (b > r + 0.12 and b > g), [3] distinct green values / 256 in the middle.</summary>
        static IEnumerator Shot35V(string dir, string name, double[] st, List<string> lines, CultureInfo ci, string note = "")
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) { lines.Add(name + ": no capture"); yield break; }
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N35-look35-" + name + ".png"), t.EncodeToPNG());
                Color32[] px = t.GetPixels32();
                int w = t.width, h = t.height, pairs = 0, eq = 0, n = 0, blue = 0;
                double lum = 0;
                var greens = new bool[256];
                for (int y = (int)(h * 0.2); y < h * 0.8; y += 5)
                    for (int x = (int)(w * 0.2); x < w * 0.8 - 1; x += 3)
                    {
                        Color32 a = px[y * w + x], b = px[y * w + x + 1];
                        pairs++;
                        if (a.r == b.r && a.g == b.g && a.b == b.b) eq++;
                        n++;
                        lum += (0.2126 * a.r + 0.7152 * a.g + 0.0722 * a.b) / 255.0;
                        if (a.b > a.r + 30 && a.b > a.g) blue++;
                        greens[a.g] = true;
                    }
                int distinct = 0;
                foreach (bool gv in greens) if (gv) distinct++;
                st[0] = pairs > 0 ? (double)eq / pairs : -1;
                st[1] = n > 0 ? lum / n : -1;
                st[2] = n > 0 ? (double)blue / n : -1;
                st[3] = distinct / 256.0;
                lines.Add($"{name}{(note.Length > 0 ? " (" + note + ")" : "")}: {w}x{h}, equal neighbours {(100 * st[0]).ToString("0.0", ci)} %, mid luminance {st[1].ToString("0.000", ci)}, blue {(100 * st[2]).ToString("0.0", ci)} %, green values {distinct}");
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
        }

        /// <summary>The room's longest clear wall from its spawn: the stand point 0.9 m off it, the run's yaw along it (degrees) and the strafe into it.</summary>
        static bool RideWall35(SurvivalGame g, RoomDef r, int skip, out Vec3 stand, out double yawDeg, out double into)
        {
            stand = default; yawDeg = 0; into = 0;
            Vec3 eye0 = new Vec3(r.spawn.x, r.spawn.y + g.cfg.movement.body.eyeHeight, r.spawn.z);
            var cands = new List<(double len, Vec3 st, double yaw, double into)>();
            for (int i = 0; i < 24; i++)
            {
                double a = i * Math.PI / 12;
                var d = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                if (!g.world.RayIntersect(eye0, d, out RayHit hit) || hit.distance < 2 || hit.distance > 24 || Math.Abs(hit.normal.y) > 0.2) continue;
                var nrm = new Vec3(hit.normal.x, 0, hit.normal.z);
                double nl = nrm.Length;
                if (nl < 0.5) continue;
                nrm = new Vec3(nrm.x / nl, 0, nrm.z / nl);
                Vec3 p = eye0 + d * hit.distance + nrm * 0.7; // r2: 0.7 m off (the verbs35 reel's stand-off)
                foreach (double sgn in new[] { 1.0, -1.0 })
                {
                    var along = new Vec3(nrm.z * sgn, 0, -nrm.x * sgn);
                    double clear = g.world.RayIntersect(p, along, out RayHit ah) ? ah.distance : 40;
                    if (clear < 10) continue;
                    bool wallAll = true; // the wall goes on beside the run (3, 6, 9 m along)
                    for (int k = 1; k <= 3 && wallAll; k++)
                        if (!g.world.RayIntersect(p + along * (3 * k), new Vec3(-nrm.x, 0, -nrm.z), out RayHit wh) || wh.distance > 1.6) wallAll = false;
                    if (!wallAll) continue;
                    double yaw = Math.Atan2(-along.x, -along.z); // forward (-sin yaw, -cos yaw) = along
                    Vec3 right = MovementMath.WishDirection(1, 0, yaw);
                    double side = right.x * nrm.x + right.z * nrm.z > 0 ? -1 : 1; // strafe toward the wall
                    cands.Add((clear, new Vec3(p.x, r.spawn.y, p.z), yaw * 180 / Math.PI, side));
                }
            }
            cands.Sort((x, y) => y.len.CompareTo(x.len));
            if (cands.Count <= skip) return false;
            stand = cands[skip].st; yawDeg = cands[skip].yaw; into = cands[skip].into;
            return true;
        }

        static IEnumerator Look35Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            const string boot = "boot -skygrindMap n34-foundry (or tonight's level 1) for GREEN, the same with -skygrindOff35 for RED";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why + " (" + boot + ")"; yield break; }
            SurvivalSim sim = g.sim;
            bool look = N35Fixes.ViewRowOn(g.cfg, "look35"), lights = N35Fixes.ViewRowOn(g.cfg, "lights35"), tv = N35Fixes.ViewRowOn(g.cfg, "tv35"), ride = N35Fixes.ViewRowOn(g.cfg, "ridelook35");
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            UiTv35.ReelMotion = true; // r2: this reel shows the TV HUD's motion (every other reel's HUD holds still)
            offs.Add(() => { SlowAim27.Force35 = false; UiTv35.ReelMotion = false; Quiet(g); g.setup.mode = RunSetup.Survival; });
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && Screen.height != 1080; i++) yield return null;
            string reso = $"{Screen.width}x{Screen.height}";
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN (" + boot + ")"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            var lines = new List<string>();
            var shots = new List<string>();
            var st = new double[4];
            string ended = null;
            Start(g, "look35", "camera", dir);
            yield return Frames(pre);

            // --- room: the first fight room, from its spawn ---
            RoomDef first = null;
            foreach (RoomDef r in lv.rooms) if (r.kind != "boss" && r.waves.Count > 0) { first = r; break; }
            first = first ?? lv.rooms[0];
            g.Respawn(first.spawn, first.spawnYawDeg);
            g.input.SetLook(first.spawnYawDeg * Math.PI / 180, -0.04);
            yield return Settle31(g, 0.8, () => (ended = Ended30(g)) != null);
            g.husks.KillAll();
            yield return Frames(10);
            yield return Shot35V(dir, "room", st, shots, ci);
            double blockRoom = st[0];
            string pass35 = $"look35 {look}: lines {Crt29.Lines35.ToString("0", ci)}, levels {Crt29.QuantLevels.ToString("0", ci)} at {Crt29.QuantStrength.ToString("0.00", ci)} (sRGB {Crt29.Srgb35 > 0.5f}, dither cell {Crt29.QuantDitherScale.ToString("0", ci)}), scanlines {Crt29.ScanCount.ToString("0", ci)} at {Crt29.ScanIntensity.ToString("0.00", ci)}, corner split {Crt29.ChromaUv.ToString("0.0000", ci)} + flat {Crt29.FlatChroma35.ToString("0.00000", ci)} uv, grain {Crt29.Grain.ToString("0.000", ci)}, CRT strength {Crt29.Strength.ToString("0.00", ci)}, pixel step {Crt29.PixelStep.ToString("0.0", ci)}";
            bool lookOk = look && Crt29.Lines35 >= 400 && Crt29.Lines35 <= 480 && Crt29.QuantLevels == 32 && Crt29.QuantStrength >= 0.99f && Crt29.ScanIntensity >= 0.25f;

            // --- lit: the room with the most fixture lights, aimed at its key ---
            LightPlan33.Plan plan = LightPlan33.GreenFor(g.map.json, g.cfg, N30Fixes.ViewRowOn(g.cfg, "look30"), out LightPlan33.Level lpl);
            RoomDef litRoom = first;
            int best = -1, skyFix = 0, skyRooms = 0;
            if (plan != null && lpl != null)
            {
                foreach (RoomDef r in lv.rooms)
                {
                    int c = 0;
                    foreach (LightPlan33.Lit l in plan.lights) if (l.room == r.id && (l.kind == "socket" || l.kind == "pool" || l.kind == "room")) c++;
                    if (r.kind != "boss" && c > best) { best = c; litRoom = r; }
                }
                skyFix = LightPlan33.SkyFixtures(lpl, plan, out skyRooms);
            }
            // r4 (n35-r3: f4 showed no pool): the room key and the fill follow the CAMPAIGN's current room (CampaignLighting24), not where the reel stands, so the
            // lit frame is that room; stood about 7 m from the floor under its key (toward the spawn), aimed at that floor point: the pool and the far fill
            if (sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < lv.rooms.Count && lv.rooms[sim.campaign.room].kind != "boss") litRoom = lv.rooms[sim.campaign.room];
            Vec3 key = new Vec3((litRoom.x0 + litRoom.x1) / 2, litRoom.spawn.y + 6, (litRoom.z0 + litRoom.z1) / 2);
            if (plan != null) foreach (LightPlan33.Lit l in plan.lights) if (l.room == litRoom.id && l.kind == "room") { key = new Vec3(l.x, l.y, l.z); break; }
            var pool = new Vec3(key.x, litRoom.spawn.y, key.z);
            Vec3 from = litRoom.spawn;
            double dx = from.x - pool.x, dz = from.z - pool.z, dl = Math.Sqrt(dx * dx + dz * dz);
            if (dl > 8) from = new Vec3(pool.x + dx / dl * 7, from.y, pool.z + dz / dl * 7);
            else if (dl < 4) from = new Vec3(Math.Max(litRoom.x0 + 1, Math.Min(litRoom.x1 - 1, pool.x)), from.y, Math.Max(litRoom.z0 + 1, Math.Min(litRoom.z1 - 1, pool.z + ((litRoom.z1 - litRoom.z0) > (litRoom.x1 - litRoom.x0) ? 7 : 0))));
            g.Respawn(from, YawTo33(from, pool));
            yield return Settle31(g, 0.4, () => (ended = Ended30(g)) != null);
            (double ky, double kp) = LookAt31(g, pool);
            kp = Math.Max(-0.7, Math.Min(0.3, kp));
            g.input.SetLook(ky, kp);
            g.forcedMove = new InputFrame { yaw = ky, pitch = kp };
            yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
            g.husks.KillAll();
            yield return Frames(8);
            yield return Shot35V(dir, "lit", st, shots, ci, $"room {litRoom.id} (the campaign's current room), key light {(CampaignLighting24.Key35Now.Length > 0 ? CampaignLighting24.Key35Now : "not lights35")}, aimed at the floor under the key from {(from - pool).Length.ToString("0.0", ci)} m; key ({key.x.ToString("0.0", ci)}, {key.y.ToString("0.0", ci)}, {key.z.ToString("0.0", ci)})");
            string passLights = $"lights35 {lights}: plan fixtures in the sky of an open room {skyFix} (open rooms {skyRooms})";
            bool lightsOk = lights && plan != null && skyFix == 0;
            g.forcedMove = new InputFrame();

            // --- hud-hit: a hit taken, two frames ---
            g.Respawn(first.spawn, first.spawnYawDeg);
            g.input.SetLook(first.spawnYawDeg * Math.PI / 180, -0.04);
            yield return Settle31(g, 0.5, () => (ended = Ended30(g)) != null);
            g.health.max = g.health.hp = 100;
            yield return Frames(20);
            int kicks0 = UiTv35Probe.Kicks, glitch0 = UiTv35Probe.Glitches;
            Vec3 f0 = g.Feet;
            double yr = first.spawnYawDeg * Math.PI / 180;
            g.health.Damage(12, "husk", new Vec3(f0.x - Math.Sin(yr) * 3, f0.y, f0.z - Math.Cos(yr) * 3));
            yield return Frames(2);
            float off1 = UiTv35Probe.OffsetPx;
            yield return Shot35V(dir, "hud-hit-1", st, shots, ci, $"HUD offset {off1.ToString("0.0", ci)} px");
            yield return Frames(6);
            float off2 = UiTv35Probe.OffsetPx;
            yield return Shot35V(dir, "hud-hit-2", st, shots, ci, $"HUD offset {off2.ToString("0.0", ci)} px");
            God(g);
            string passTv = $"tv35 {tv}: the hit kicked the HUD {UiTv35Probe.Kicks - kicks0} time(s), glitches {UiTv35Probe.Glitches - glitch0}, offset {off1.ToString("0.0", ci)} -> {off2.ToString("0.0", ci)} px, text min {UiTv35Probe.MinTextPx.ToString("0", ci)} px";
            bool tvOk = tv && UiTv35Probe.Kicks > kicks0 && Math.Abs(off1) > 0.5f;

            // --- ride: a real wall ride + zoom (3 tries), else the look forced ---
            int rideFrames = 0, focusFrames = 0, tries = 0;
            bool shotRide = false;
            float focusMax = 0;
            for (int attempt = 0; attempt < 3 && !shotRide && ended == null; attempt++)
            {
                if (!RideWall35(g, first, attempt, out Vec3 stand, out double yd, out double into)) break;
                tries++;
                g.Respawn(stand, yd);
                double yaw = yd * Math.PI / 180;
                g.input.SetLook(yaw, 0);
                yield return Settle31(g, 0.3, () => (ended = Ended30(g)) != null);
                double t0 = sim.SimTime;
                bool wasAds = false;
                // r1: 0 ride frames with a run-up + strafe. Now as the verbs35 reel enters a ride (wallride35: a HOLD ride while jump stays held): already
                // running along the wall at 11 m/s with a lean into it, jump pressed at once and HELD, the stick into the wall
                Vec3 alongV = MovementMath.WishDirection(0, 1, yaw), rightV = MovementMath.WishDirection(1, 0, yaw);
                var intoV = new Vec3(rightV.x * into, 0, rightV.z * into);
                while (sim.SimTime - t0 < 3.0 && !shotRide && (ended = Ended30(g)) == null)
                {
                    God(g);
                    double t = sim.SimTime - t0;
                    if (t < 0.05) sim.mover.velocity = new Vec3(alongV.x * 11 + intoV.x * 0.5, sim.mover.velocity.y, alongV.z * 11 + intoV.z * 0.5);
                    bool riding = sim.mover.state == MoveState.WallRide;
                    var mv = new InputFrame { moveZ = 1, moveX = into * 0.5, yaw = yaw, pitch = 0 };
                    mv.jump = new Skygrind.Core.Button { held = true, pressed = t < 0.02 };
                    g.input.SetLook(yaw, 0);
                    g.forcedMove = mv;
                    g.forcedCombat = riding ? new CombatInput { weapon = -1, altFire = new Skygrind.Core.Button { held = true, pressed = !wasAds && !sim.loadout.adsActive } } : IdleCombat;
                    wasAds = sim.loadout.adsActive;
                    yield return null;
                    if (sim.mover.state == MoveState.WallRide) rideFrames++;
                    focusMax = Math.Max(focusMax, SlowAim27.Focus35Now);
                    if (sim.mover.state == MoveState.WallRide && sim.loadout.adsActive) focusFrames++;
                    if (sim.mover.state == MoveState.WallRide && sim.loadout.adsActive && (SlowAim27.Focus35Now >= 0.95f || (!ride && focusFrames >= 12)))
                    {
                        yield return Shot35V(dir, "ride", st, shots, ci, $"try {attempt + 1}, riding + zoomed, focus {SlowAim27.Focus35Now.ToString("0.00", ci)}, slow-aim {g.SlowAim01.ToString("0.00", ci)}");
                        shotRide = true;
                    }
                }
                g.forcedMove = new InputFrame();
                g.forcedCombat = IdleCombat;
                yield return Frames(10);
            }
            bool forced = false;
            if (!shotRide && ride)
            {
                SlowAim27.Force35 = true;
                yield return Frames(20);
                focusMax = Math.Max(focusMax, SlowAim27.Focus35Now);
                yield return Shot35V(dir, "ride-forced", st, shots, ci, $"no ride in {tries} tries: the look FORCED, focus {SlowAim27.Focus35Now.ToString("0.00", ci)}");
                SlowAim27.Force35 = false;
                forced = true;
                yield return Frames(20);
            }
            string passRide = $"ridelook35 {ride}: {tries} tries, ride frames {rideFrames}, riding + zoomed frames {focusFrames}, focus max {focusMax.ToString("0.00", ci)}{(forced ? " (FORCED: the trigger not seen)" : "")}, entries {SlowAim27.Focus35Entries}";
            bool rideOk = ride && shotRide && focusMax >= 0.95f;

            yield return Stop();
            Screen.SetResolution(w0, h0, mode0);
            yield return Frames(10);
            Quiet(g);
            g.setup.mode = RunSetup.Survival;
            var verdicts = new List<string>();
            bool ok = look && ended == null;
            if (look) verdicts.Add("look35 " + (lookOk ? "GREEN" : "RED"));
            if (lights) verdicts.Add("lights35 " + (lightsOk ? "GREEN" : "RED"));
            if (tv) verdicts.Add("tv35 " + (tvOk ? "GREEN" : "RED"));
            if (ride) verdicts.Add("ridelook35 " + (rideOk ? "GREEN" : forced ? "LOOK ONLY (forced)" : "RED"));
            ok = ok && (!look || lookOk) && (!lights || lightsOk) && (!tv || tvOk) && (!ride || rideOk);
            res[0] = $"{(ok ? "PASS" : "RED")}: level {lv.id} at {reso}; rows look35 {look} lights35 {lights} tv35 {tv} ridelook35 {ride} -> {(verdicts.Count > 0 ? string.Join(", ", verdicts) : "all off (-skygrindOff35: the RED half)")}; "
                + $"room equal neighbours {(100 * blockRoom).ToString("0.0", ci)} % (point upscale: high){(ended != null ? " | ended: " + ended : "")} || {pass35} || {passLights} || {passTv} || {passRide} || "
                + string.Join(" || ", shots) + " || shots shot-N35-look35-<room|lit|hud-hit-1|hud-hit-2|ride|ride-forced>.png | " + Preconditions(g, g.Feet);
        }
    }
}
