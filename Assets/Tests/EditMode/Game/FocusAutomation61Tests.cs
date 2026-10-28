// NIGHT61 builder D (B1, T-146; the NIGHT60 code review's S3, Research/2026-10-21-wave60/review-n60-code.md): the view row focus60
// (design.n60.focus60, ON) pauses a Playing run when the window loses the focus, but FocusPause60On (SurvivalGame.N60L.cs) kept only
// -skygrindBench out. Every other run PlayerChecks.Attach drives starts a live run too (the smoke; the capture tours -skygrindShots,
// -skygrindTile, -skygrindStrip, -skygrindHitch; the NIGHT29 checks -skygrindDoorFree29, -skygrindLevelSelect29), so a window taking the
// foreground mid-capture paused it (FeelReel.Running shields only the reel's own scenarios). Pinned: SurvivalGame.MayPauseOnFocusLoss(args),
// the one automation predicate FocusPause60On reads (its -skygrindOff60 / -skygrindNoFocus60 / -skygrindBench rules stay as NIGHT60 wrote them):
//   1. false for every PlayerChecks mode, each launched as Tools/ launches it (its options with it), the focus probe's excepted
//   2. true for a player's game (no flag, or only switches and options that start no mode) and for the focus probe's own run, planted or
//      not: the probe measures this very pause (Tools/night/n60-focus-probe.sh: row on True, -skygrindNoFocus60 False, planted False)
//   3. Attach's order: the NIGHT29 checks run before the probe (with one of them the probe never starts), the probe before every other mode
//   4. every "-skygrind..." literal PlayerChecks.cs reads is classified: a row switch (-skygrindOff...), an option of a mode, or a mode the
//      predicate keeps from pausing (a mode added to Attach fails here until the predicate knows it)
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FocusAutomation61Tests
    {
        /// <summary>A Player command line: the exe, then the flags (split on spaces: no flag or value here holds one).</summary>
        static string[] Line(string flags) => ("SKYGRIND.exe " + flags).Trim().Split(' ');

        /// <summary>The options PlayerChecks.Attach reads only after its mode's flag (alone they start nothing).</summary>
        static readonly string[] Options = { "-skygrindPadShot", "-skygrindShotsUi", "-skygrindReelOnly", "-skygrindReelPad", "-skygrindFpsCap", "-skygrindProfile", "-skygrindVsync" }; // NIGHT71 F1: -skygrindVsync, the bench's VSYNC arm

        [TestCase("-skygrindDoorFree29 -logFile doorfree.log")]
        [TestCase("-skygrindDoorFree29 -skygrindNoHold29")]
        [TestCase("-skygrindLevelSelect29 -logFile levelselect.log")]
        [TestCase("-skygrindLevelSelect29 -skygrindOff29")]
        [TestCase("-skygrindSmoke -logFile smoke.log")]
        [TestCase("-skygrindSmoke -skygrindPadShot pad.png")]
        [TestCase("-skygrindShots C:/shots -skygrindMap n57-kiln")]
        [TestCase("-skygrindShots C:/shots -skygrindShotsUi")]
        [TestCase("-skygrindReel C:/reel -skygrindMap n57-kiln -skygrindReelOnly a157 -skygrindClearShots60 -skygrindNoExit58")]
        [TestCase("-skygrindReel C:/reel -skygrindMap n52-foundry -skygrindReelOnly pad41 -skygrindReelPad")]
        [TestCase("-skygrindTile C:/tile")]
        [TestCase("-skygrindStrip C:/strips slide")]
        [TestCase("-skygrindHitch -logFile hitch.log")]
        [TestCase("-skygrindGolden")]
        [TestCase("-skygrindFingerprint")]
        [TestCase("-skygrindFarm 100 normal 1 farm.jsonl")]
        [TestCase("-skygrindReplay tape.jsonl")]
        [TestCase("-skygrindBench 60 play")]
        [TestCase("-skygrindBench 60 -skygrindFpsCap 144")]
        [TestCase("-skygrindBench 60 -skygrindProfile 300 bench.raw")]
        [TestCase("-skygrindBench 60 play -skygrindVsync 1")]
        [TestCase("-skygrindBench 60 play -skygrindVsync 0 -skygrindNoLoopSplit71")]
        public void AutomationRun_NeverPausesOnFocusLoss(string flags)
        {
            Assert.IsFalse(SurvivalGame.MayPauseOnFocusLoss(Line(flags)), flags + ": a run PlayerChecks drives must not pause on a focus loss");
        }

        [TestCase("")]
        [TestCase("-logFile player.log -screen-fullscreen 0 -screen-width 1280 -screen-height 720")]
        [TestCase("-skygrindOff61")]
        [TestCase("-skygrindNoExit58")]
        [TestCase("-skygrindMap n57-kiln")]
        [TestCase("-skygrindClearShots60")]
        [TestCase("-skygrindReelOnly a157")]
        [TestCase("-skygrindDev")]
        [TestCase("-skygrindKiosk")]
        public void PlayersGame_StillPausesOnFocusLoss(string flags)
        {
            Assert.IsTrue(SurvivalGame.MayPauseOnFocusLoss(Line(flags)), "'" + flags + "' starts no PlayerChecks mode: a player's game still pauses");
        }

        [TestCase("-skygrindFocusProbe60 -screen-fullscreen 0 -logFile focus.log")]
        [TestCase("-skygrindFocusProbe60 -skygrindFocusProbe60NoPause")]
        [TestCase("-skygrindFocusProbe60NoPause")]
        public void FocusProbe_StillPausesOnFocusLoss(string flags)
        {
            Assert.IsTrue(SurvivalGame.MayPauseOnFocusLoss(Line(flags)), flags + ": the focus probe's own run measures this very pause");
        }

        [Test]
        public void AttachOrder_TheNight29ChecksRunBeforeTheProbe()
        {
            Assert.IsFalse(SurvivalGame.MayPauseOnFocusLoss(Line("-skygrindDoorFree29 -skygrindFocusProbe60")), "DoorFree29Check runs, the probe never starts");
            Assert.IsFalse(SurvivalGame.MayPauseOnFocusLoss(Line("-skygrindLevelSelect29 -skygrindFocusProbe60")), "LevelSelect29Check runs, the probe never starts");
        }

        [Test]
        public void AttachOrder_TheProbeRunsBeforeTheOtherModes()
        {
            Assert.IsTrue(SurvivalGame.MayPauseOnFocusLoss(Line("-skygrindFocusProbe60 -skygrindSmoke")), "the probe runs, the smoke never starts");
        }

        [Test]
        public void NoCommandLine_IsAPlayersGame()
        {
            Assert.IsTrue(SurvivalGame.MayPauseOnFocusLoss(null), "no arguments");
            Assert.IsTrue(SurvivalGame.MayPauseOnFocusLoss(new string[0]), "an empty command line");
        }

        [Test]
        public void EveryPlayerChecksFlag_IsClassified()
        {
            string path = Path.Combine(Application.dataPath, "Scripts", "Game", "PlayerChecks.cs");
            Assert.IsTrue(File.Exists(path), "PlayerChecks.cs not found: " + path);
            var seen = new SortedSet<string>(StringComparer.Ordinal);
            foreach (Match m in Regex.Matches(File.ReadAllText(path), "\"(-skygrind[A-Za-z0-9]+)\"")) seen.Add(m.Groups[1].Value);
            int modes = 0;
            foreach (string f in seen)
            {
                if (f.StartsWith("-skygrindOff", StringComparison.Ordinal) || Array.IndexOf(Options, f) >= 0) continue; // a row switch (FocusPause60On's own -skygrindOffNN rule) or a mode's option
                modes++;
                Assert.IsFalse(SurvivalGame.MayPauseOnFocusLoss(Line(f)), f + " (a flag PlayerChecks.cs reads) starts a mode: it must not pause on a focus loss");
            }
            Assert.That(modes, Is.GreaterThan(0), "no mode flag found in PlayerChecks.cs: " + string.Join(" ", seen));
        }
    }
}
