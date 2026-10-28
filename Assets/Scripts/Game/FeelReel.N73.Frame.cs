// NIGHT73 builder B (the lead's second job: "frame the boss kill so finish55 can be judged"). finish55 (Finish55View.cs: the 6 deg kick, the 8 deg FOV punch over 0.8 s, the letterbox) read NOTHING OBVIOUS twice on
// bosskill72 strips: at the wyrm's dying beat (frame ~525 on n57-kiln) the camera was already on the wyrm's head (Look(a.weak)), but the head is a 3 m box seen 25 m or more away at the far end of a lit chamber,
// 100 px in a frame of 1080, and the room (an orange wall, a lit box) is what the strip shows. This is the reel's side of it, used by BossKill72Scenario (FeelReel.N72.B.cs) for THE KILN WYRM only (the other four
// bosses keep their old framing): a vantage in the arena, a gate that holds the killing taps until the body is a third of the frame, the same decision on both sides of a pair (it reads the sim's positions and
// the config's field of view, never the camera), and the numbers of what was framed. Nothing in the game changes: the reel moves its own player (as FeelReel.N48.Wyrm.cs does), aims, and holds its own trigger.
//   THE WYRM: 26 oriented boxes on a trail (head 3 x 3 x 4 m, 24 pieces 2.5 m tapering), 22 m/s on its coil, which is a ring of radius 19 m round the arena's centre at 2.5-5 m (Design/bosses/wyrm.json), the
//   arena 60 m across with four pillars; its dying beat (design.n24.wyrm.speed dieTime 1.2 s) slows the head to a stop over about a second and sinks it. It cannot be held still for the beat (Wyrm.Frozen stops the
//   dying count too), so the gate arms the kill at a moment the head is in the band; the beat then moves it on by up to a dozen metres while the camera tracks it, and the numbers say where it was at the beat's
//   start, half a second in, and at bossDown.
//   THE AIM: the centre of the wyrm's second box (the first body piece behind the head), the middle of the head and its next two pieces (UnityTuning.Fin73Pieces); the reticle on it is also where the taps land.
//   THE DISTANCE: the three pieces' bounding sphere (radius rho) subtends Fin73FillShare of the vertical field of view at D = rho / sin(share x fov / 2); the gate opens within Fin73BandLo..Fin73BandHi of that.
//   THE VANTAGE: Fin73VantageM from the arena's centre toward the room's entry (inside the pillars' ring on the entry side), set once when the framing begins and again only when something shoved the player away
//   and the gate is still shut; the gate also needs a clear sight line (no pillar in it) that misses the arena's centre by Fin73ClearM (the "core beam in the middle" that filled the n48-r3 frames), the head over
//   the floor, and all of that for Fin73HoldFrames frames in a row; at Fin73GateMaxS s from the forced health it arms anyway and the line says so.
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>The wyrm's first Fin73Pieces boxes on the screen at one moment: the clipped pixel box, how many of them have their centre in the frame, and the eye-to-aim distance (m).</summary>
        struct Fin73Box
        {
            public bool have;
            public int x0, y0, x1, y1, inFrame, counted, frame;
            public double dist;
            public int W => have ? x1 - x0 : 0;
            public int H => have ? y1 - y0 : 0;
        }

        /// <summary>The framing's state through one bosskill72 fight (the wyrm's).</summary>
        sealed class Fin73
        {
            public bool begun, armed, timedOut, noCentre;
            public int teleports, sinceTp = 1000, holdN, bandFrames, gateFrames, armedFrame = -1;
            public double armedMs = -1, floorY, targetD, rho, fovDeg, lastD, lastMiss;
            public Vec3 centre, vantage;
            public string why = "";
            public Fin73Box atArm, atDying, atHalf, atDown;
            public int sinceDying = -1;
        }

        /// <summary>The aim: the centre of the second box (the first body piece), the head when there is no second.</summary>
        static Vec3 Fin73Aim(Wyrm w) => w.PieceCount > 1 ? w.boxes[1].c : w.HeadPosition;

        /// <summary>The bounding sphere of the first k boxes about `at`: the farthest centre plus its box's largest half size.</summary>
        static double Fin73Rho(Wyrm w, Vec3 at, int k)
        {
            double r = 0;
            for (int i = 0; i < k; i++)
            {
                WyrmBox b = w.boxes[i];
                r = Math.Max(r, Vec3.DistanceTo(b.c, at) + Math.Max(b.hr, Math.Max(b.hu, b.hf)));
            }
            return r;
        }

        /// <summary>The first k boxes' eight corners each, projected by the real camera: the clipped pixel box. have false when nothing is in front of it.</summary>
        static Fin73Box Fin73Measure(SurvivalGame g, Wyrm w, int frame)
        {
            var o = new Fin73Box { frame = frame };
            Camera cam = g.cam;
            if (cam == null || w == null) return o;
            int k = Math.Min(UnityTuning.Fin73Pieces, w.PieceCount);
            int x0 = int.MaxValue, y0 = int.MaxValue, x1 = int.MinValue, y1 = int.MinValue;
            for (int i = 0; i < k; i++)
            {
                WyrmBox b = w.boxes[i];
                o.counted++;
                Vector3 cp = cam.WorldToScreenPoint(WebSpace.ToUnity(b.c));
                if (cp.z > 0.1f && cp.x >= 0 && cp.x <= cam.pixelWidth && cp.y >= 0 && cp.y <= cam.pixelHeight) o.inFrame++;
                for (int sx = -1; sx <= 1; sx += 2)
                    for (int sy = -1; sy <= 1; sy += 2)
                        for (int sz = -1; sz <= 1; sz += 2)
                        {
                            Vec3 corner = b.c + b.r * (sx * b.hr) + b.u * (sy * b.hu) + b.f * (sz * b.hf);
                            Vector3 sp = cam.WorldToScreenPoint(WebSpace.ToUnity(corner));
                            if (sp.z <= 0.1f) continue;
                            x0 = Math.Min(x0, (int)sp.x);
                            y0 = Math.Min(y0, (int)sp.y);
                            x1 = Math.Max(x1, (int)sp.x);
                            y1 = Math.Max(y1, (int)sp.y);
                        }
            }
            if (x1 <= x0 || y1 <= y0) return o;
            o.have = true;
            o.x0 = Math.Max(0, x0);
            o.y0 = Math.Max(0, y0);
            o.x1 = Math.Min(cam.pixelWidth, x1);
            o.y1 = Math.Min(cam.pixelHeight, y1);
            o.dist = Vec3.DistanceTo(g.Eye, Fin73Aim(w));
            return o;
        }

        static string Fin73BoxText(SurvivalGame g, Fin73Box b)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            if (!b.have) return "not on screen";
            Camera cam = g.cam;
            int pw = cam != null ? cam.pixelWidth : 1920, ph = cam != null ? cam.pixelHeight : 1080;
            return $"{b.dist.ToString("0.0", ci)} m, box x {b.x0}..{b.x1} y {b.y0}..{b.y1} = {b.W} x {b.H} px of {pw} x {ph} ({(100.0 * b.W / pw).ToString("0", ci)} % of the width, {(100.0 * b.H / ph).ToString("0", ci)} % of the height; {b.inFrame} of {b.counted} pieces' centres in the frame)";
        }

        /// <summary>One fight frame before the kill, after the health is forced: where to look, and whether the taps may fire. The first call places the player at the vantage; the gate arms the kill (tapOk).</summary>
        static Vec3 Fin73Step(SurvivalGame g, SurvivalSim sim, Fin73 s, Wyrm w, RoomDef room, double ms, double forcedAtMs, int frame, out bool tapOk)
        {
            Vec3 aim = Fin73Aim(w);
            tapOk = true;
            int k = Math.Min(UnityTuning.Fin73Pieces, w.PieceCount);
            void Place()
            {
                g.Respawn(s.vantage, 0);
                sim.mover.invulnerable = true; // Respawn clears it (Mover.cs): the lava floor and the worm's shoves do not matter to a frame
                s.teleports++;
                s.sinceTp = 0;
            }
            if (!s.begun)
            {
                s.begun = true;
                if (!w.arenaCenter.HasValue) { s.noCentre = true; s.why = "no arena centre on the wyrm: not framed"; s.armed = true; s.armedMs = ms; s.armedFrame = frame; return aim; }
                s.centre = w.arenaCenter.Value;
                s.floorY = s.centre.y;
                double ux = room.spawn.x - s.centre.x, uz = room.spawn.z - s.centre.z, ul = Math.Sqrt(ux * ux + uz * uz);
                if (ul < 1e-3) { ux = 0; uz = 1; ul = 1; }
                s.vantage = new Vec3(s.centre.x + ux / ul * UnityTuning.Fin73VantageM, s.floorY, s.centre.z + uz / ul * UnityTuning.Fin73VantageM);
                s.fovDeg = g.cfg.Num("camera.fovDeg");
                s.rho = Fin73Rho(w, aim, k);
                s.targetD = s.rho / Math.Sin(UnityTuning.Fin73FillShare * s.fovDeg * Math.PI / 360.0);
                Place();
            }
            if (s.armed) return aim;
            s.sinceTp++;
            s.gateFrames++;
            Vec3 me = sim.mover.position;
            if (s.sinceTp >= UnityTuning.Fin73DriftFrames && Math.Sqrt((me.x - s.vantage.x) * (me.x - s.vantage.x) + (me.z - s.vantage.z) * (me.z - s.vantage.z)) > UnityTuning.Fin73DriftM) Place();
            Vec3 eye = g.Eye;
            double d = Vec3.DistanceTo(eye, aim);
            s.lastD = d;
            bool up = true;
            for (int i = 0; i < k; i++) if (w.buried[i] || w.boxes[i].c.y - s.floorY < UnityTuning.Fin73AboveM) up = false;
            bool inBand = d >= UnityTuning.Fin73BandLo * s.targetD && d <= UnityTuning.Fin73BandHi * s.targetD;
            bool sight = d > 1e-3 && (!g.world.RayIntersect(eye, (aim - eye) * (1.0 / d), out RayHit hit) || hit.distance >= d - 1.0);
            double sx = aim.x - eye.x, sz = aim.z - eye.z, sl = sx * sx + sz * sz;
            double t = sl > 1e-9 ? Math.Max(0, Math.Min(1, ((s.centre.x - eye.x) * sx + (s.centre.z - eye.z) * sz) / sl)) : 0;
            double mx = eye.x + sx * t - s.centre.x, mz = eye.z + sz * t - s.centre.z, miss = Math.Sqrt(mx * mx + mz * mz);
            s.lastMiss = miss;
            bool ok = inBand && up && sight && miss >= UnityTuning.Fin73ClearM;
            s.holdN = ok ? s.holdN + 1 : 0;
            if (ok) s.bandFrames++;
            bool timeout = ms - forcedAtMs >= UnityTuning.Fin73GateMaxS * 1000;
            if (s.holdN >= UnityTuning.Fin73HoldFrames || timeout)
            {
                s.armed = true;
                s.armedMs = ms;
                s.armedFrame = frame;
                s.timedOut = !(s.holdN >= UnityTuning.Fin73HoldFrames);
                if (s.timedOut) s.why = $"the gate never held {UnityTuning.Fin73HoldFrames} frames in {UnityTuning.Fin73GateMaxS.ToString("0", CultureInfo.InvariantCulture)} s (last distance {d.ToString("0.0", CultureInfo.InvariantCulture)} m against {s.targetD.ToString("0.0", CultureInfo.InvariantCulture)}, sight {(sight ? "clear" : "blocked")}, miss {miss.ToString("0.0", CultureInfo.InvariantCulture)} m): armed anyway";
                s.atArm = Fin73Measure(g, w, frame);
            }
            tapOk = s.armed;
            return aim;
        }
    }
}
