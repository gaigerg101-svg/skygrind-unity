// NIGHT63 builder V (B1e, a measurement; DECIDE63 D-12: THE WICK runs at its full 1.53 m cap in its OPEN window, its hit sphere 1.7 m, and "a
// shot finds what you see"): the boss63 reel's reach book, pure (N63ReachTests). Over the OPEN window's frames the layer drew (the reel adds
// them), the distance from the DRAWN weak point to the sim's hit sphere centre (BossAttackClock63's `weak`) and the sphere's radius (the size the
// guard scales by: the boss's collider radius). Its line: the max, the p95 (nearest rank) and the frames whose drawn weak point lies outside
// the sim's sphere: a shot at the drawn centre there finds nothing.
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class ReachBook63
    {
        static readonly CultureInfo Ci = CultureInfo.InvariantCulture;
        readonly List<double> dists = new List<double>();
        double radius;

        /// <summary>Open frames measured, and those whose drawn weak point lay outside the sim's sphere.</summary>
        public int Frames, Outside;
        /// <summary>The largest distance (m).</summary>
        public double Max;

        /// <summary>The 95th percentile distance by nearest rank (m; 0 with no frame).</summary>
        public double P95
        {
            get
            {
                if (dists.Count == 0) return 0;
                var s = new List<double>(dists);
                s.Sort();
                int rank = (int)Math.Ceiling(0.95 * s.Count);
                return s[Math.Max(0, Math.Min(s.Count - 1, rank - 1))];
            }
        }

        /// <summary>An open frame from the layer's motion: the drawn weak point (BossMotion63Math.DrawnPoint) against the sim's (radius m).</summary>
        public void Add(Vector3 pivot, Motion63 m, Vector3 simWeak, float radius) =>
            AddDistance((BossMotion63Math.DrawnPoint(pivot, m, simWeak) - simWeak).magnitude, radius);

        /// <summary>An open frame: the drawn weak point `dist` m from the sim's hit sphere centre, the sphere `radius` m.</summary>
        public void AddDistance(double dist, double radius)
        {
            Frames++;
            dists.Add(dist);
            if (dist > Max) Max = dist;
            if (dist > radius) Outside++;
            this.radius = radius;
        }

        /// <summary>"[boss63] reach &lt;boss&gt; open frames &lt;n&gt; | drawn weak point to the sim's hit sphere centre max &lt;m&gt; m, p95 &lt;m&gt; m | outside its
        /// &lt;r&gt; m sphere &lt;k&gt; frames (&lt;share&gt; %)".</summary>
        public string Line(string boss)
        {
            string b = boss ?? "?";
            if (Frames == 0) return "[boss63] reach " + b + " open frames 0 | no open frame drawn (not measured)";
            return "[boss63] reach " + b + " open frames " + Frames + " | drawn weak point to the sim's hit sphere centre max " + Max.ToString("0.00", Ci)
                + " m, p95 " + P95.ToString("0.00", Ci) + " m | outside its " + radius.ToString("0.00", Ci) + " m sphere " + Outside + " frames ("
                + (100.0 * Outside / Frames).ToString("0.0", Ci) + " %)";
        }
    }
}
