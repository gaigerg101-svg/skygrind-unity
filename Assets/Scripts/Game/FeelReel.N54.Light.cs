// NIGHT54 builder L (design.n54.light54, a VIEW row; Light54.cs): the reel a154, THE FLICKER COUNT (FeelReel.N54.cs routes it here). Run it per
// level in a Player (reel.sh + -skygrindMap <id> + -skygrindReelOnly a154, L-062), at 1920x1080: n52-foundry (level 1, THE WICK: both spots),
// n52-iceline and n52-kiln (the lit room only). A campaign run, DOORFREE, god mode, the room's wave out, the camera shake setting 0 (the shot's
// kick off: a still camera), the camera held still by the reel's forced look.
//   Spot ROOM: the current room's spawn, facing a wall UnityTuning.L54ReelWallMin..Max m away (the impacts' pops and the fixtures in the frame).
//   Spot STACK (a level with THE STACK): THE WICK's fight begun (Wick37Begin), the camera on the shaft's floor between the entry and the core,
//     aimed L54ReelWickSideDeg off the core at the wall L54ReelWickWallUp m up (the core's tells in frame, the shots on the wall).
// At each spot four passes: OFF (Light54.Force false: NIGHT53's lights), ON (Force true), OFF, ON, each L54ReelSettle frames to settle and
// L54ReelFrames / 2 frames read, the gun pressed every L54ReelFireEvery frames. Each frame read: the screen downsampled to L54ReelW x L54ReelH
// (a box average), its read region (L54ReelU0..U1 x V0..V1: clear of the viewmodel, the reticle, the HUD) as luminance; per frame against the
// last: a FLICKER EVENT when the region's mean luminance moves more than L54ReelJump (3 %), the share of pixels whose luminance moves more than
// L54ReelPixelDelta; per pass: the mean luminance and the near-white share (> L54ReelWhite: the exposure).
// Prints the input (shots fired, frames read, pops / impacts the pop light took, pops the FLASH light took, fixture slot moves / holds), the
// numbers per spot and side, and: GREEN when the row is ON in this run's config (Force null), every side fired >= L54ReelMinShots and read every
// frame, each spot's ON flicker events <= L54ReelPassShare x its OFF events (or both 0), and at THE STACK the ON near-white share <=
// L54ReelWhiteShare x OFF's (or under L54ReelWhiteFloor); RED otherwise (always RED with the row off / -skygrindOff54: the OFF control, with
// the same numbers). Frames: shot-L54-<spot>-<side>.png (the first round of each side) and the capture's own frames.
// Not run by its author (no Player in the worktree).
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
        static partial void N54Light(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = Light54Scenario(g, dir, res, offs, pre);
        }

        sealed class Side54
        {
            public int frames, pairs, shots, events, pops, impacts, flashPops, moves, held;
            public double lum, changed, changedMax, white, jumpMax, wideLum, wideWhite;
            public readonly HashSet<string> attacks = new HashSet<string>();
            public readonly List<string> eventsAt = new List<string>();
            public string albedo = "";
        }

        static readonly CultureInfo Ci54 = CultureInfo.InvariantCulture;

        /// <summary>ScreenCapture.CaptureScreenshotIntoRenderTexture's rows are top-down where the graphics API's UVs start at the top (D3D): the
        /// read rows are then flipped (the narrow region is symmetric, the wide exposure read's viewmodel box is not). Printed in the line.</summary>
        static bool Flip54 => SystemInfo.graphicsUVStartsAtTop;
        static RenderTexture full54;
        static Texture2D tex54;

        /// <summary>At the frame's end: the screen, halved down to L54ReelW x L54ReelH, as luminance (0..1, bottom-up rows) into `lum`.</summary>
        static bool Grab54(float[] lum)
        {
            int sw = Screen.width, sh = Screen.height, W = UnityTuning.L54ReelW, H = UnityTuning.L54ReelH;
            if (sw < W || sh < H) return false;
            if (full54 == null || full54.width != sw || full54.height != sh)
            {
                if (full54 != null) full54.Release();
                full54 = new RenderTexture(sw, sh, 0, RenderTextureFormat.ARGB32);
            }
            ScreenCapture.CaptureScreenshotIntoRenderTexture(full54);
            var temps = new List<RenderTexture>();
            RenderTexture src = full54;
            int w = sw, h = sh;
            while (w / 2 >= W && h / 2 >= H)
            {
                w /= 2;
                h /= 2;
                RenderTexture t = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(src, t);
                temps.Add(t);
                src = t;
            }
            RenderTexture fin = RenderTexture.GetTemporary(W, H, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, fin);
            temps.Add(fin);
            if (tex54 == null) tex54 = new Texture2D(W, H, TextureFormat.RGBA32, false);
            RenderTexture was = RenderTexture.active;
            RenderTexture.active = fin;
            tex54.ReadPixels(new Rect(0, 0, W, H), 0, 0, false);
            tex54.Apply(false);
            RenderTexture.active = was;
            foreach (RenderTexture t in temps) RenderTexture.ReleaseTemporary(t);
            Color32[] px = tex54.GetPixels32();
            for (int i = 0; i < px.Length && i < lum.Length; i++) lum[i] = (0.2126f * px[i].r + 0.7152f * px[i].g + 0.0722f * px[i].b) / 255f;
            return true;
        }

        /// <summary>One pass at a still camera: the switch set, settled, then `frames` frames read with the gun pressed.</summary>
        static IEnumerator Pass54(SurvivalGame g, Side54 s, bool on, double yaw, double pitch, int frames, int[] fires, string dir, string shot)
        {
            Light54.Force = on;
            for (int i = 0; i < UnityTuning.L54ReelSettle; i++)
            {
                God(g);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                g.forcedCombat = IdleCombat;
                yield return null;
            }
            int W = UnityTuning.L54ReelW, H = UnityTuning.L54ReelH;
            int u0 = (int)(W * UnityTuning.L54ReelU0), u1 = (int)(W * UnityTuning.L54ReelU1), v0 = (int)(H * UnityTuning.L54ReelV0), v1 = (int)(H * UnityTuning.L54ReelV1);
            int n = Math.Max(1, (u1 - u0) * (v1 - v0));
            float[] cur = new float[W * H], prev = new float[W * H];
            bool havePrev = false;
            double prevMean = 0;
            int f0 = fires[0], pops0 = LightPools.Pops54, imp0 = LightPools.Impacts54, fp0 = LightPools.FlashPops54, mv0 = LightKit29.Moves54, hd0 = LightKit29.Held54;
            var end = new WaitForEndOfFrame();
            for (int i = 0; i < frames; i++)
            {
                God(g);
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                g.forcedCombat = i % UnityTuning.L54ReelFireEvery == 0 ? Fire : IdleCombat;
                yield return null;
                yield return end;
                if (!Grab54(cur)) continue;
                s.frames++;
                double sum = 0, white = 0, changed = 0;
                for (int y = v0; y < v1; y++)
                    for (int x = u0; x < u1; x++)
                    {
                        int k = y * W + x;
                        float l = cur[k];
                        sum += l;
                        if (l > UnityTuning.L54ReelWhite) white++;
                        if (havePrev && Math.Abs(l - prev[k]) > UnityTuning.L54ReelPixelDelta) changed++;
                    }
                double mean = sum / n;
                s.lum += mean;
                s.white += white / n;
                // the exposure over the WIDE frame (the lead's a147 read: the walls, the floor and the boss), minus the HUD's bands and the viewmodel's box
                double wSum = 0, wWhite = 0;
                int wn = 0;
                for (int y = (int)(H * 0.15f); y < (int)(H * 0.85f); y++)
                    for (int x = (int)(W * 0.04f); x < (int)(W * 0.96f); x++)
                    {
                        if (x > W * UnityTuning.L54ReelVmU && (Flip54 ? y > H * (1 - UnityTuning.L54ReelVmV) : y < H * UnityTuning.L54ReelVmV)) continue; // the viewmodel (rows bottom-up, or top-down where the capture is flipped)
                        float l = cur[y * W + x];
                        wSum += l;
                        if (l > UnityTuning.L54ReelWhite) wWhite++;
                        wn++;
                    }
                if (wn > 0) { s.wideLum += wSum / wn; s.wideWhite += wWhite / wn; }
                if (havePrev)
                {
                    s.pairs++;
                    double jump = Math.Abs(mean - prevMean) / Math.Max(prevMean, 0.02);
                    if (jump > UnityTuning.L54ReelJump)
                    {
                        s.events++;
                        if (s.eventsAt.Count < UnityTuning.L54ReelEventsListed)
                        {
                            // what was live on the event's frame (the a154 STACK's events attributed: the lead's 15:12 ask)
                            Wick37 we = g.sim != null ? g.sim.wick37 : null;
                            int embers = 0;
                            if (we != null) foreach (Wick37Ember e in we.embers) if (e.alive) embers++;
                            int burns = g.sim != null ? g.sim.wick37HotBurns : 0;
                            string st = we != null && we.Alive && we.brain != null ? $"{we.AttackId ?? "none"}:{we.brain.state}{(we.brain.Open ? " OPEN" : "")}" : "-";
                            s.eventsAt.Add($"f{i} {(mean > prevMean ? "+" : "-")}{(100 * jump).ToString("0.0", Ci54)} % (wick {st}, embers {embers}, burns {burns}, fired {(i % UnityTuning.L54ReelFireEvery == 0 ? "this frame" : (i % UnityTuning.L54ReelFireEvery) + " frames ago")})");
                        }
                    }
                    s.jumpMax = Math.Max(s.jumpMax, jump);
                    s.changed += changed / n;
                    s.changedMax = Math.Max(s.changedMax, changed / n);
                }
                float[] sw = prev; prev = cur; cur = sw;
                prevMean = mean;
                havePrev = true;
                Wick37 w = g.sim != null ? g.sim.wick37 : null;
                if (w != null && w.Alive && w.brain != null) s.attacks.Add((w.AttackId ?? "none") + ":" + w.brain.state);
            }
            g.forcedCombat = IdleCombat;
            s.shots += fires[0] - f0;
            s.pops += LightPools.Pops54 - pops0;
            s.impacts += LightPools.Impacts54 - imp0;
            s.flashPops += LightPools.FlashPops54 - fp0;
            s.moves += LightKit29.Moves54 - mv0;
            s.held += LightKit29.Held54 - hd0;
            if (g.sim != null && g.sim.wick37Stack != null && Wick37View.Current != null) s.albedo += (Wick37View.Current.Albedo54Built ? "grey" : "WHITE") + " "; // the stack's albedo this pass (light54 rebuilds it on a flip)
            if (shot != null) yield return Shot29(dir, shot, new double[4]);
        }

        static string Line54(string side, Side54 s)
        {
            int rf = Math.Max(1, s.frames), rp = Math.Max(1, s.pairs);
            return $"{side}: shots fired {s.shots}, frames read {s.frames} ({s.pairs} pairs); FLICKER EVENTS {s.events} (mean luminance jump > {(100 * UnityTuning.L54ReelJump).ToString("0", Ci54)} %; worst {(100 * s.jumpMax).ToString("0.0", Ci54)} %); "
                + $"pixels changed > {UnityTuning.L54ReelPixelDelta.ToString("0.00", Ci54)}: mean {(100 * s.changed / rp).ToString("0.00", Ci54)} %, worst {(100 * s.changedMax).ToString("0.0", Ci54)} %; "
                + $"mean luminance {(s.lum / rf).ToString("0.000", Ci54)}, near-white share {(100 * s.white / rf).ToString("0.00", Ci54)} %; "
                + $"WIDE frame (exposure): mean luminance {(s.wideLum / rf).ToString("0.000", Ci54)}, near-white share {(100 * s.wideWhite / rf).ToString("0.00", Ci54)} %; "
                + $"the pop light took {s.pops} kills/blasts and {s.impacts} impacts, the FLASH light took {s.flashPops} pop frames; fixture slot moves {s.moves}, held by hysteresis {s.held}"
                + (s.albedo.Length > 0 ? "; THE STACK's albedo per pass: " + s.albedo.Trim() : "")
                + (s.attacks.Count > 0 ? "; THE WICK's states seen: " + string.Join(" ", s.attacks) : "")
                + (s.eventsAt.Count > 0 ? "; EVENTS: " + string.Join("; ", s.eventsAt) : "");
        }

        /// <summary>The input happened on a side: its shots (or, row on, the impacts the pop light took) and every frame read.</summary>
        static bool In54(Side54 s, int half) => (s.shots >= UnityTuning.L54ReelMinShots || s.impacts >= UnityTuning.L54ReelMinShots) && s.frames >= 2 * half - 2;

        static bool FlickerOk54(Side54 off, Side54 on) => off.events == 0 ? on.events == 0 : on.events <= off.events * UnityTuning.L54ReelPassShare;

        static IEnumerator Light54Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            Light54.Force = null;
            bool rowOn = Light54.On(g.cfg);
            SurvivalSim sim = g.sim;
            double shake0 = g.settings != null ? g.settings.Num(UnityTuning.CameraShakeSetting) : 1;
            offs.Add(() =>
            {
                Light54.Force = null;
                g.forcedMove = null;
                g.forcedCombat = IdleCombat;
                if (g.husks != null) g.husks.frozen = false;
                if (g.settings != null) g.settings.Set(UnityTuning.CameraShakeSetting, shake0);
                Quiet(g);
                g.setup.mode = RunSetup.Survival;
            });
            if (g.settings != null) g.settings.Set(UnityTuning.CameraShakeSetting, 0.0);
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(240);
            sim = g.sim;
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED a154: the run did not start as a CAMPAIGN (boot with reel.sh + -skygrindMap <id> + -skygrindReelOnly a154)"; yield break; }
            DoorFree41(g);
            g.forcedCombat = IdleCombat;
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            int[] fires = new int[1];
            offs.Add(g.events.On("fire", o => fires[0]++));
            LevelDef lv = sim.campaignLevel;
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, map {g.MapId}, light54 in the config {(rowOn ? "ON" : "off")}, camera shake setting 0 (was {shake0.ToString("0.##", Ci54)}), gun {g.loadout.Weapon}, screen {Screen.width}x{Screen.height}";
            Start(g, "a154-light54", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            bool inputOk = true, flickerOk = true, exposureOk = true;
            int half = UnityTuning.L54ReelFrames / 2;

            // ---- spot ROOM: the current room's spawn, a wall 4..14 m ahead ----
            Campaign24 c = sim.campaign;
            RoomDef room = c != null && c.room >= 0 && c.room < lv.rooms.Count ? lv.rooms[c.room] : lv.rooms[0];
            g.Respawn(room.spawn, room.spawnYawDeg);
            for (int i = 0; i < 20; i++) { God(g); yield return null; }
            double yawA = room.spawnYawDeg * Math.PI / 180, wallA = ClearAhead(g, yawA);
            foreach (double y in Yaws(g))
            {
                double d = ClearAhead(g, y);
                if (d >= UnityTuning.L54ReelWallMin && d <= UnityTuning.L54ReelWallMax) { yawA = y; wallA = d; break; }
            }
            var aOff = new Side54();
            var aOn = new Side54();
            for (int r = 0; r < 2; r++)
            {
                yield return Pass54(g, aOff, false, yawA, -0.05, half, fires, dir, r == 0 ? "L54-room-off" : null);
                yield return Pass54(g, aOn, true, yawA, -0.05, half, fires, dir, r == 0 ? "L54-room-on" : null);
            }
            bool aIn = In54(aOff, half) && In54(aOn, half);
            bool aOk = FlickerOk54(aOff, aOn);
            inputOk &= aIn;
            flickerOk &= aOk;
            lines.Add($"ROOM {room.id} (its spawn, a wall {wallA.ToString("0.0", Ci54)} m ahead, yaw {(yawA * 180 / Math.PI).ToString("0", Ci54)} deg): {(aOk ? "flicker down" : "FLICKER NOT DOWN")}{(aIn ? "" : " (INPUT SHORT)")} || {Line54("OFF", aOff)} || {Line54("ON", aOn)}");

            // ---- spot STACK: THE WICK's fight, on the shaft's floor ----
            bool wickLevel = sim.wick37Centre.HasValue || (g.MapId ?? "").Contains("foundry");
            if (wickLevel)
            {
                if (!sim.wick37Centre.HasValue) sim.Wick37Place(new Vec3(2600, 0, 0));
                if (!sim.wick37Centre.HasValue || sim.wick37Stack == null) { lines.Add("STACK: no stack (the wick37 row off?)"); inputOk = false; }
                else
                {
                    Vec3 o = sim.wick37Centre.Value;
                    Wick37Stack st = sim.wick37Stack;
                    g.Respawn(o + new Vec3(0, 0.05, st.p.radius + 1.0), 0);
                    g.input.SetLook(0, 0);
                    g.forcedMove = new InputFrame();
                    yield return Frames(10);
                    bool begun = sim.Wick37Begin(out _, out _);
                    double t0 = sim.SimTime;
                    while (sim.SimTime - t0 < 1.5) { God(g); yield return null; }
                    Wick37 w = sim.wick37;
                    Vec3 feet = o + new Vec3(0, 0.05, st.p.radius * 0.55);
                    Vec3 core = w != null ? w.Centre : o + new Vec3(0, 3, 0);
                    double dx = core.x - feet.x, dz = core.z - feet.z;
                    double yawB = Math.Atan2(-dx, -dz) + UnityTuning.L54ReelWickSideDeg * Math.PI / 180;
                    g.Respawn(feet, yawB * 180 / Math.PI);
                    for (int i = 0; i < 20; i++) { God(g); g.input.SetLook(yawB, 0); yield return null; }
                    double wallB = ClearAhead(g, yawB);
                    double pitchB = Math.Atan2(UnityTuning.L54ReelWickWallUp - 1.6, Math.Max(1, wallB));
                    var bOff = new Side54();
                    var bOn = new Side54();
                    for (int r = 0; r < 2; r++)
                    {
                        g.Respawn(feet, yawB * 180 / Math.PI); // a slam or a burn may have moved the player: the same spot each pass
                        yield return Pass54(g, bOff, false, yawB, pitchB, half, fires, dir, r == 0 ? "L54-stack-off" : null);
                        g.Respawn(feet, yawB * 180 / Math.PI);
                        yield return Pass54(g, bOn, true, yawB, pitchB, half, fires, dir, r == 0 ? "L54-stack-on" : null);
                    }
                    bool bIn = begun && In54(bOff, half) && In54(bOn, half);
                    bool bOk = FlickerOk54(bOff, bOn);
                    double wOff = bOff.wideWhite / Math.Max(1, bOff.frames), wOn = bOn.wideWhite / Math.Max(1, bOn.frames); // the WIDE frame's near-white share (the exposure)
                    double lumOn = bOn.wideLum / Math.Max(1, bOn.frames);
                    bool bExp = (wOn < UnityTuning.L54ReelWhiteFloor || wOn <= wOff * UnityTuning.L54ReelWhiteShare) && lumOn <= UnityTuning.L54ReelStackLumMax; // the lead (15:12): THE STACK's base toward 0.40-0.45
                    inputOk &= bIn;
                    flickerOk &= bOk;
                    exposureOk &= bExp;
                    lines.Add($"STACK (THE WICK's fight begun {(begun ? "yes" : "NO")}; the floor {(st.p.radius * 0.55).ToString("0.0", Ci54)} m in from the axis, {UnityTuning.L54ReelWickSideDeg.ToString("0", Ci54)} deg off the core, the wall {wallB.ToString("0.0", Ci54)} m ahead; in the stack (bloom trim) {Wick37View.InStack54}): "
                        + $"{(bOk ? "flicker down" : "FLICKER NOT DOWN")}, exposure {(bExp ? "down" : "NOT DOWN")} (the wide frame's near-white {(100 * wOff).ToString("0.00", Ci54)} % -> {(100 * wOn).ToString("0.00", Ci54)} %, mean luminance {(bOff.wideLum / Math.Max(1, bOff.frames)).ToString("0.000", Ci54)} -> {lumOn.ToString("0.000", Ci54)} (ON at most {UnityTuning.L54ReelStackLumMax.ToString("0.00", Ci54)}: the un-graded base the lead asked for, 0.40-0.45); capture rows flipped {Flip54}){(bIn ? "" : " (INPUT SHORT)")} || {Line54("OFF", bOff)} || {Line54("ON", bOn)}");
                }
            }
            else lines.Add("STACK: not on this level (THE WICK is level 1, n52-foundry)");
            Light54.Force = null;
            yield return Frames(5);
            yield return Stop();
            bool green = rowOn && inputOk && flickerOk && exposureOk;
            res[0] = $"{(green ? "GREEN" : "RED")} a154 light54 (row {(rowOn ? "ON" : "OFF: the OFF control")}; input {(inputOk ? "ok" : "SHORT")}, flicker {(flickerOk ? "down at every spot" : "NOT DOWN")}, exposure {(exposureOk ? "ok" : "NOT DOWN")}): "
                + string.Join(" |||| ", lines) + " | " + pc;
        }
    }
}
