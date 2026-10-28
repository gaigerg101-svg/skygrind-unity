// NIGHT55 builder C ssao55: a255's SSAO part (FeelReel.N55.Look.cs calls it after the STEP part). Not run by its author.
// A still camera in two rooms (the current room's spawn, then the next non-boss room's), each facing the first wall Ssao55ReelWallMin-Max m
// away, looking Ssao55ReelPitch down (the floor-wall corner, where the AO lives). Three sides in one run by Ssao55.Force (the config's row
// is not needed for the numbers; the verdict needs it): OFF (NIGHT54's SSAO, the noise as the pass makes it), ON (the noise held), FLOOR
// (the AO's intensity 0: what changes on a still camera without SSAO's noise: motes, lights). Each side settles Ssao55ReelSettle frames, then
// Ssao55ReelFrames frames are read at full resolution in the read region (Ssao55ReelU0..V1); per frame pair the share of pixels whose
// luminance moves more than Ssao55ReelDelta (|dL| > 2 %), per side its mean, the mean luminance and the median frame time (ms, the reads'
// cost included, the same on each side) and the GPU frame time when FrameTimingManager reports one. Then a slow yaw (Ssao55ReelYawRate) OFF
// and ON in the first room. Prints the input (the rooms, the walls, frames read, Ssao55.Frozen / Floored / MaterialsFound: the freeze ran),
// then per room: FLOOR, OFF, ON. GREEN: the row on in this run's config, OFF over Ssao55ReelDefectOverFloor x FLOOR in a room (the defect
// exists), and in every room ON's changed share <= Ssao55ReelOnOverOff x OFF's with ON's mean luminance within Ssao55ReelLumTol of OFF's.
// "no defect measured" when no room's OFF is over 2x its FLOOR. Frames: shot-N29-N55-ssao-<room>-<side>.png (1920x1080).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Unity.Collections;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        sealed class Ssao55Side
        {
            public int frames, pairs;
            public double changed, lum;
            public readonly List<float> ms = new List<float>();
            public readonly List<double> gpu = new List<double>();
        }

        static RenderTexture full55;
        static Texture2D crop55;
        static readonly FrameTiming[] timing55 = new FrameTiming[1];

        /// <summary>At the frame's end: the read region at full resolution as luminance (0..1) into `lum` (its length w x h).</summary>
        static bool Grab55(float[] lum, out int w, out int h)
        {
            int sw = Screen.width, sh = Screen.height;
            int x0 = (int)(sw * UnityTuning.Ssao55ReelU0), x1 = (int)(sw * UnityTuning.Ssao55ReelU1), y0 = (int)(sh * UnityTuning.Ssao55ReelV0), y1 = (int)(sh * UnityTuning.Ssao55ReelV1);
            w = x1 - x0;
            h = y1 - y0;
            if (w <= 0 || h <= 0 || lum.Length < w * h) return false;
            if (full55 == null || full55.width != sw || full55.height != sh)
            {
                if (full55 != null) full55.Release();
                full55 = new RenderTexture(sw, sh, 0, RenderTextureFormat.ARGB32);
            }
            ScreenCapture.CaptureScreenshotIntoRenderTexture(full55);
            if (crop55 == null || crop55.width != w || crop55.height != h) crop55 = new Texture2D(w, h, TextureFormat.RGBA32, false);
            RenderTexture was = RenderTexture.active;
            RenderTexture.active = full55;
            crop55.ReadPixels(new Rect(x0, y0, w, h), 0, 0, false); // the region is symmetric about the middle row: a flipped capture reads the same rows
            crop55.Apply(false);
            RenderTexture.active = was;
            NativeArray<Color32> px = crop55.GetPixelData<Color32>(0);
            for (int i = 0; i < w * h; i++) lum[i] = (0.2126f * px[i].r + 0.7152f * px[i].g + 0.0722f * px[i].b) / 255f;
            return true;
        }

        static IEnumerator Ssao55Pass(SurvivalGame g, Ssao55Side s, Ssao55.Mode mode, double yaw, double pitch, double yawRate, string dir, string shot)
        {
            Ssao55.Force = mode;
            double y = yaw;
            for (int i = 0; i < UnityTuning.Ssao55ReelSettle; i++)
            {
                God(g);
                g.input.SetLook(y, pitch);
                g.forcedMove = new InputFrame { yaw = y, pitch = pitch };
                g.forcedCombat = IdleCombat;
                yield return null;
            }
            float[] cur = new float[Screen.width * Screen.height], prev = new float[Screen.width * Screen.height];
            bool havePrev = false;
            var end = new WaitForEndOfFrame();
            for (int i = 0; i < UnityTuning.Ssao55ReelFrames; i++)
            {
                y += yawRate;
                God(g);
                g.input.SetLook(y, pitch);
                g.forcedMove = new InputFrame { yaw = y, pitch = pitch };
                g.forcedCombat = IdleCombat;
                yield return null;
                s.ms.Add(Time.unscaledDeltaTime * 1000f);
                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, timing55) > 0 && timing55[0].gpuFrameTime > 0) s.gpu.Add(timing55[0].gpuFrameTime);
                yield return end;
                if (!Grab55(cur, out int w, out int h)) continue;
                int n = w * h;
                double sum = 0, changed = 0;
                for (int k = 0; k < n; k++)
                {
                    sum += cur[k];
                    if (havePrev && Math.Abs(cur[k] - prev[k]) > UnityTuning.Ssao55ReelDelta) changed++;
                }
                s.frames++;
                s.lum += sum / n;
                if (havePrev) { s.pairs++; s.changed += changed / n; }
                float[] t = prev; prev = cur; cur = t;
                havePrev = true;
            }
            if (shot != null) yield return Shot29(dir, shot, new double[4]);
            if (mode == Ssao55.Mode.Floor) Ssao55.EndFloor();
            Ssao55.Force = Ssao55.Mode.Row;
        }

        static double Median55(List<float> v)
        {
            if (v.Count == 0) return double.NaN;
            var c = new List<float>(v);
            c.Sort();
            return c[c.Count / 2];
        }

        static double Median55(List<double> v)
        {
            if (v.Count == 0) return double.NaN;
            var c = new List<double>(v);
            c.Sort();
            return c[c.Count / 2];
        }

        /// <summary>The SSAO part (see the file's header). out[0] the part's text, out[1] "GREEN" / "RED".</summary>
        static IEnumerator N55SsaoPart(SurvivalGame g, string dir, string[] output)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string P(double v) => (100 * v).ToString("0.00", ci) + " %";
            string F3(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.000", ci);
            string F2(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.00", ci);
            SurvivalSim sim = g.sim;
            bool on = Ssao55.RowOn;
            LevelDef lv = sim.campaignLevel;
            var spots = new List<RoomDef>();
            if (lv != null)
            {
                if (sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < lv.rooms.Count && lv.rooms[sim.campaign.room].kind != "boss") spots.Add(lv.rooms[sim.campaign.room]);
                foreach (RoomDef r in lv.rooms) if (spots.Count < 2 && r.kind != "boss" && !spots.Contains(r)) spots.Add(r);
            }
            if (spots.Count == 0) { output[0] = "SSAO: no room to stand in"; output[1] = "RED"; yield break; }
            int frozen0 = Ssao55.Frozen, floored0 = Ssao55.Floored;
            var lines = new List<string>();
            bool defectAny = false, allOk = true;
            string map = PostFx.LevelIdOf(g.map.json);
            for (int si = 0; si < spots.Count; si++)
            {
                RoomDef r = spots[si];
                Vec3 at = r.spawn;
                if (Move55Lines.Floor(g.world, at.x, at.z, at.y + 1.5, out double fy, out _)) at = new Vec3(at.x, fy, at.z);
                g.Respawn(new Vec3(at.x, at.y + 0.05, at.z), r.spawnYawDeg);
                for (int i = 0; i < 30; i++) { God(g); yield return null; }
                // the first heading (from the spawn's yaw round) with a wall in range at the eye
                double yaw = r.spawnYawDeg * Math.PI / 180, wallAt = -1;
                Vec3 eye = sim.Eye;
                for (int k = 0; k < 24; k++)
                {
                    double a = r.spawnYawDeg * Math.PI / 180 + 2 * Math.PI * k / 24;
                    var dirv = new Vec3(-Math.Sin(a), 0, -Math.Cos(a)); // the look's forward at yaw a (RideKill53Stick's convention)
                    if (g.world.RayIntersect(eye, dirv, out RayHit hit) && hit.distance >= UnityTuning.Ssao55ReelWallMin && hit.distance <= UnityTuning.Ssao55ReelWallMax) { yaw = a; wallAt = hit.distance; break; }
                }
                var off = new Ssao55Side();
                var onS = new Ssao55Side();
                var floor = new Ssao55Side();
                string tag = $"N55-ssao-{map}-{r.id}";
                yield return Ssao55Pass(g, off, Ssao55.Mode.Off, yaw, UnityTuning.Ssao55ReelPitch, 0, dir, tag + "-off");
                yield return Ssao55Pass(g, onS, Ssao55.Mode.On, yaw, UnityTuning.Ssao55ReelPitch, 0, dir, tag + "-on");
                yield return Ssao55Pass(g, floor, Ssao55.Mode.Floor, yaw, UnityTuning.Ssao55ReelPitch, 0, dir, tag + "-floor");
                double cOff = off.pairs > 0 ? off.changed / off.pairs : double.NaN, cOn = onS.pairs > 0 ? onS.changed / onS.pairs : double.NaN, cFl = floor.pairs > 0 ? floor.changed / floor.pairs : double.NaN;
                double lOff = off.frames > 0 ? off.lum / off.frames : double.NaN, lOn = onS.frames > 0 ? onS.lum / onS.frames : double.NaN, lFl = floor.frames > 0 ? floor.lum / floor.frames : double.NaN;
                bool defect = cOff > UnityTuning.Ssao55ReelDefectOverFloor * cFl;
                bool ok = cOn <= UnityTuning.Ssao55ReelOnOverOff * cOff && Math.Abs(lOn - lOff) <= UnityTuning.Ssao55ReelLumTol;
                defectAny |= defect;
                allOk &= ok;
                string room = $"ROOM {r.id} (spawn, a wall {(wallAt < 0 ? "NOT FOUND (the spawn's yaw)" : wallAt.ToString("0.0", ci) + " m")} ahead, yaw {(yaw * 180 / Math.PI).ToString("0", ci)} deg, pitch {(UnityTuning.Ssao55ReelPitch * 180 / Math.PI).ToString("0", ci)} deg): "
                    + $"changed |dL| > {(100 * UnityTuning.Ssao55ReelDelta).ToString("0", ci)} %: FLOOR {P(cFl)}, OFF {P(cOff)}, ON {P(cOn)} (ON x{F2(cOn / cOff)} of OFF, want <= x{F2(UnityTuning.Ssao55ReelOnOverOff)}; OFF x{F2(cOff / Math.Max(cFl, 1e-9))} of FLOOR: {(defect ? "a defect" : "NO DEFECT")}); "
                    + $"mean luminance FLOOR {F3(lFl)}, OFF {F3(lOff)}, ON {F3(lOn)} (dON-OFF {F3(lOn - lOff)}, want within {F2(UnityTuning.Ssao55ReelLumTol)}); "
                    + $"median frame ms OFF {F2(Median55(off.ms))}, ON {F2(Median55(onS.ms))}, FLOOR {F2(Median55(floor.ms))}; GPU ms OFF {F2(Median55(off.gpu))}, ON {F2(Median55(onS.gpu))}; frames read {off.frames}/{onS.frames}/{floor.frames}";
                if (si == 0)
                {
                    var yOff = new Ssao55Side();
                    var yOn = new Ssao55Side();
                    yield return Ssao55Pass(g, yOff, Ssao55.Mode.Off, yaw, UnityTuning.Ssao55ReelPitch, UnityTuning.Ssao55ReelYawRate, dir, null);
                    yield return Ssao55Pass(g, yOn, Ssao55.Mode.On, yaw, UnityTuning.Ssao55ReelPitch, UnityTuning.Ssao55ReelYawRate, dir, null);
                    double a = yOff.pairs > 0 ? yOff.changed / yOff.pairs : double.NaN, b = yOn.pairs > 0 ? yOn.changed / yOn.pairs : double.NaN;
                    room += $"; SLOW YAW ({(UnityTuning.Ssao55ReelYawRate * 60 * 180 / Math.PI).ToString("0.0", ci)} deg/s): OFF {P(a)}, ON {P(b)} (x{F2(b / a)})";
                }
                lines.Add(room);
            }
            Ssao55.Force = Ssao55.Mode.Row;
            int frozen = Ssao55.Frozen - frozen0, floored = Ssao55.Floored - floored0;
            bool green = on && defectAny && allOk;
            output[0] = $"SSAO ssao55 on {on} | input: {spots.Count} rooms, the freeze ran {frozen} times (materials {Ssao55.MaterialsFound}), the floor {floored} | {string.Join(" | ", lines)} | "
                + (defectAny ? (allOk ? "ON <= half of OFF in every room" : "ON NOT under half of OFF (or its luminance moved)") : "no defect measured (OFF under 2x FLOOR in every room)");
            output[1] = green ? "GREEN" : "RED";
        }
    }
}
