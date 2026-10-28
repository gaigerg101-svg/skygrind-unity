// NIGHT74 builder L (pads74): the pad regions of one pan74 sweep. For every rendered frame, for every jump pad that is active in the scene, the screen rectangle of the pad volume
// (the halo, the stream and the chevrons: UnityTuning.N74PadRegionHalfR x the pad's radius wide, N74PadRegionHeightM high) and of the stream column, projected through the world camera
// at the moment the frame is captured, and whether the pad is in view (in the picture, no box corner behind the camera, not hidden by the level's boxes on both sight lines). The text
// file Pan74PadsMath documents goes beside the sweep's stack file; Tools/night/n74-flick.py `pads` counts the flagged blocks inside those rectangles for two runs. Reel only: the game
// never builds it, and it reads the scene and the camera, never writes either.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    sealed class Pan74Pads
    {
        readonly List<string> lines = new List<string>();
        readonly JumpPadView27[] views;
        readonly Camera cam;
        readonly List<double[]> solids;
        readonly double radius;
        readonly double[] corners = new double[24];
        readonly double[] sx = new double[8], sy = new double[8];
        readonly bool[] behind = new bool[8];
        readonly int[] vol = new int[4], stream = new int[4];
        int w, h;
        /// <summary>Frames in which at least one pad was in view, the pads built in the scene, and the frames written.</summary>
        public int framesWithPad, padCount, frames;
        /// <summary>The first frame's note (what the file holds), for the reel's line.</summary>
        public string note = "no frames";
        public string error;

        public Pan74Pads(SurvivalGame g, List<double[]> solids)
        {
            cam = g != null ? g.cam : null;
            this.solids = solids ?? new List<double[]>();
            radius = g != null && g.sim != null ? g.sim.cfg.Num("jumpPad.radius") : 1.0;
            views = Object.FindObjectsByType<JumpPadView27>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            padCount = views.Length;
        }

        /// <summary>Adds the frame (call it once the frame has been captured: the camera is where that frame drew it).</summary>
        public void Add(int frame)
        {
            if (cam == null) { error = "no world camera"; return; }
            if (w == 0) { w = cam.pixelWidth; h = cam.pixelHeight; }
            bool any = false;
            Vector3 eyeU = cam.transform.position;
            Vec3 eye = WebSpace.FromUnity(eyeU);
            foreach (JumpPadView27 v in views)
            {
                if (v == null || !v.gameObject.activeInHierarchy) continue;
                Vector3 c = v.transform.position;
                Vec3 cw = WebSpace.FromUnity(c);
                bool seen = Project(c, UnityTuning.N74PadRegionHalfR * radius, 0.0, UnityTuning.N74PadRegionHeightM, vol)
                    && Project(c, UnityTuning.N74PadStreamRegionHalfR * radius, 0.0, UnityTuning.N74PadStreamRegionTopM, stream)
                    && !Pan74PadsMath.Hidden(eye.x, eye.y, eye.z, cw.x, cw.y, cw.z, UnityTuning.N74PadSightLiftM, UnityTuning.N74PadRegionHeightM - 0.1, solids, UnityTuning.N74PadSightEps);
                lines.Add(Pan74PadsMath.Line(frame, v.index, seen, vol, stream));
                any |= seen;
            }
            if (any) framesWithPad++;
            frames++;
            if (frames == 1) note = padCount + " pads built, " + w + "x" + h;
        }

        bool Project(Vector3 c, double hw, double y0, double y1, int[] rect)
        {
            Pan74PadsMath.Corners(c.x, c.y, c.z, hw, y0, y1, corners);
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = cam.WorldToScreenPoint(new Vector3((float)corners[i * 3], (float)corners[i * 3 + 1], (float)corners[i * 3 + 2]));
                sx[i] = p.x;
                sy[i] = p.y;
                behind[i] = p.z <= 0.05f;
            }
            return Pan74PadsMath.Rect(sx, sy, behind, w, h, rect);
        }

        /// <summary>Writes the file (whatever frames were added); false with `error` set when it cannot.</summary>
        public bool Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var all = new List<string>(lines.Count + 1) { Pan74PadsMath.Header(w, h) };
                all.AddRange(lines);
                File.WriteAllLines(path, all);
                return true;
            }
            catch (System.Exception e)
            {
                error = e.GetType().Name + ": " + e.Message;
                return false;
            }
        }
    }
}
