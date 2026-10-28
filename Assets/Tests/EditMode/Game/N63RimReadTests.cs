// NIGHT63 builder V (leftover 3: T-146's rim56 item; the NIGHT60 code review's S2, Research/2026-10-21-wave60/review-n60-code.md): rim56's row
// is read ONCE a frame. Rim56.LateUpdate read it at its top (On(g.cfg)) and again through Rim60.On(g.cfg) for the lit gain (Rim60.On calls Rim56.On:
// NightRows.ViewOn and GameConfig.Design build strings each call, about 0.5 KB of garbage a frame, estimated). Pinned:
//   1. Rim60.LitOn(rim56On): the lit gain's rule given rim56's own read of this frame: false when rim56 is off, else neither -skygrindOff60 nor
//      -skygrindOff58 (the process switch ForceOffFrom 60 or 58: false); Rim60.On(cfg) answers LitOn(Rim56.On(cfg)) (the band's caller unchanged)
//   2. Rim56.cs reads the row once: one On(g.cfg) and no Rim60.On (the lit gain takes LitOn of the frame's read)
// The row is still read every frame (D1's same-run toggle re-reads view rows each frame): one read, not a cache. Written test first (RED on its
// commit: LitOn is a placeholder answering false and Rim56.cs still calls Rim60.On); the Editor run is the lead's.
using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63RimReadTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        [Test]
        public void LitOn_TakesRim56sOwnReadOfTheFrame()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                bool procOff = Rim60.Off60() || Rim58.Off58(); // this process's command line (an Editor's carries neither switch)
                Assert.That(Rim60.LitOn(false), Is.False, "rim56 off: NIGHT59's lit gain");
                Assert.That(Rim60.LitOn(true), Is.EqualTo(!procOff), "rim56 on, no -skygrindOff60 / -skygrindOff58: NIGHT60's lit gain");
                NightRows.ForceOffFrom = 60;
                Assert.That(Rim60.LitOn(true), Is.False, "-skygrindOff60: NIGHT59's rim56");
                NightRows.ForceOffFrom = 58;
                Assert.That(Rim60.LitOn(true), Is.False, "-skygrindOff58: NIGHT57's rim56");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Rim56_ReadsItsRowOnceAFrame()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Rim56.cs"));
            Assert.That(Regex.Matches(src, @"On\(g\.cfg\)").Count, Is.EqualTo(1), "Rim56.cs reads the row once (LateUpdate's own On(g.cfg))");
            Assert.That(src.Contains("Rim60.On("), Is.False, "the lit gain takes the frame's read (Rim60.LitOn), not a second Rim60.On -> Rim56.On");
            Assert.That(src.Contains("Rim60.LitOn("), Is.True, "the lit gain's hook");
        }
    }
}
