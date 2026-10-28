// NIGHT63 builder D (slate D1, same-run SPOT pairs): the reel side's pure parts (SameRun63Math.N63D.cs; the reel in FeelReel.N63Same.cs).
// Pinned:
//   1. the option: `-skygrindSameRun63 <night.row>` names one row by its night and name (n61.exitFrame61); absent = no option; a value missing,
//      without its night, with another night than the name's, or another switch is refused with a reason (the reel prints it, never guesses)
//   2. the rows it claims: re-read every frame (exitFrame61, bossMotion63, bossTell63, bossColours63, colours63), judged by their UI root hidden
//      (bossRule61), planted (noop63, plant63); any other row is unclaimed (the pair proves nothing about it, and the line says so)
//   3. the off captures' folder: <side>-off/ with the shot's path under the reel's root kept (so n60-v-lamp.py <side>/a157 <side>-off/a157,
//      n62-gun-yellow.py --pair and n63-spot-pair.cjs run unchanged); a folder outside the root (an Editor eval) gets <dir>-off
//   4. the hold clock: frozen from the hold's start to its end (frame and unscaled time; delta 0), continuous after (the held span skipped)
//   5. the planted square: exactly UnityTuning.Same63PlantPx square pixels at UnityTuning.Same63PlantAt from the top-left, inside the screen
//   6. the proof line (L-089): one "[same63] toggle fired" line per shot with the file, the row, its kind, the sim held or not, the off path
//   7. NIGHT63 review S4 (Logs/n63/review-n63.md): ONE NAME, ONE FRAME. The held ON frame no longer replaces the shot's PNG (Shot29's stats,
//      lamp60's door box and every other sidecar a shot helper wrote describe the shot's own frame): it goes to <side>-on/ under the same name,
//      beside <side>-off/, so the pair is <side>-on/X against <side>-off/X (both held frames, the same draw but for the row) and <side>/X is
//      the reel's own frame, as in a run without the toggle; the proof line names both halves. Written test first: RED on its commit.
// Written test first (NIGHT63.md section 4): RED on its commit (SameRun63Math's placeholders answer nothing); the Editor run is the lead's.
using System.IO;
using NUnit.Framework;
using S = Skygrind.Game.SameRun63Math;

namespace Skygrind.Game.Tests
{
    public class N63SameRunGameTests
    {
        [Test]
        public void Option_NamesOneRowByItsNightAndName()
        {
            Assert.That(S.Row(new[] { "x.exe", "-skygrindSameRun63", "n61.exitFrame61" }, out string why), Is.EqualTo("exitFrame61"));
            Assert.That(why, Is.Null);
            Assert.That(S.Row(new[] { "-skygrindSameRun63", "n63.noop63" }, out _), Is.EqualTo("noop63"));
            Assert.That(S.Row(new[] { "-skygrindsamerun63", "n61.bossRule61" }, out _), Is.EqualTo("bossRule61"), "the switch ignores case");
            Assert.That(S.Row(new[] { "x.exe", "-skygrindReel", "C:/r" }, out why), Is.Null, "absent: no option");
            Assert.That(why, Is.Null, "absent: nothing to refuse");
            foreach (string[] bad in new[]
            {
                new[] { "-skygrindSameRun63" }, new[] { "-skygrindSameRun63", "exitFrame61" }, new[] { "-skygrindSameRun63", "n62.exitFrame61" },
                new[] { "-skygrindSameRun63", "-skygrindClearShots60" }, new[] { "-skygrindSameRun63", "n61." }, new[] { "-skygrindSameRun63", "n61.exit.Frame61" },
            })
            {
                Assert.That(S.Row(bad, out why), Is.Null, string.Join(" ", bad));
                Assert.That(why, Is.Not.Null.And.Not.Empty, "a reason: " + string.Join(" ", bad));
            }
        }

        [Test]
        public void Kinds_TheRowsTheToggleClaims()
        {
            foreach (string r in new[] { "exitFrame61", "bossMotion63", "bossTell63", "bossColours63", "colours63" }) Assert.That(S.KindOf(r), Is.EqualTo(S.Kind.Frame), r);
            Assert.That(S.KindOf("bossRule61"), Is.EqualTo(S.Kind.UiRoot));
            Assert.That(S.KindOf("noop63"), Is.EqualTo(S.Kind.Planted));
            Assert.That(S.KindOf("plant63"), Is.EqualTo(S.Kind.Planted));
            Assert.That(S.KindOf("gunYellow61"), Is.EqualTo(S.Kind.Unclaimed), "a row whose view reads it once");
            Assert.That(S.KindOf(null), Is.EqualTo(S.Kind.Unclaimed));
        }

        [Test]
        public void ArgAfter_TheValueOfASwitch()
        {
            Assert.That(S.ArgAfter(new[] { "x.exe", "-skygrindReel", "C:/Logs/feel/t/s1" }, "-skygrindReel"), Is.EqualTo("C:/Logs/feel/t/s1"));
            Assert.That(S.ArgAfter(new[] { "x.exe", "-skygrindReel" }, "-skygrindReel"), Is.Null, "no value");
            Assert.That(S.ArgAfter(new[] { "-skygrindReel", "-skygrindMap" }, "-skygrindReel"), Is.Null, "another switch is no value");
            Assert.That(S.ArgAfter(new[] { "x.exe" }, "-skygrindReel"), Is.Null);
            Assert.That(S.ArgAfter(null, "-skygrindReel"), Is.Null);
        }

        static string F(string p) => Path.GetFullPath(p);

        [Test]
        public void OffDir_SideOffWithTheSamePathUnderTheRoot()
        {
            string root = "C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on";
            Assert.That(S.OffDir(root, Path.Combine(root, "a157")), Is.EqualTo(F("C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on-off/a157")), "a scenario folder");
            Assert.That(S.OffDir(root + "/", root + "/boss61/x"), Is.EqualTo(F("C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on-off/boss61/x")), "a trailing slash, a deeper folder");
            Assert.That(S.OffDir(root, root), Is.EqualTo(F(root + "-off")), "the root itself");
            Assert.That(S.OffDir("C:/r/s1", "C:/r/s1on/a157"), Is.EqualTo(F("C:/r/s1on/a157-off")), "a sibling that only starts with the root's name is not under it");
            Assert.That(S.OffDir(null, "C:/eval/shots"), Is.EqualTo(F("C:/eval/shots-off")), "no reel root (an Editor eval)");
            Assert.That(S.OffDir("C:/r/s1", "D:/other/a157"), Is.EqualTo(F("D:/other/a157-off")), "outside the root");
        }

        [Test]
        public void OnDir_SideOnWithTheSamePathUnderTheRoot()
        {
            string root = "C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on";
            Assert.That(S.OnDir(root, Path.Combine(root, "a157")), Is.EqualTo(F("C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on-on/a157")), "a scenario folder");
            Assert.That(S.OnDir(root + "/", root + "/boss61/x"), Is.EqualTo(F("C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on-on/boss61/x")), "a trailing slash, a deeper folder");
            Assert.That(S.OnDir(root, root), Is.EqualTo(F(root + "-on")), "the root itself");
            Assert.That(S.OnDir("C:/r/s1", "C:/r/s1on/a157"), Is.EqualTo(F("C:/r/s1on/a157-on")), "a sibling that only starts with the root's name is not under it");
            Assert.That(S.OnDir(null, "C:/eval/shots"), Is.EqualTo(F("C:/eval/shots-on")), "no reel root (an Editor eval)");
            Assert.That(S.OffDir(root, Path.Combine(root, "a157")), Is.EqualTo(F("C:/Users/gg/dev/SKYGRIND-unity/Logs/feel/n63-b1/s1on-off/a157")), "the off half's folder unchanged");
        }

        [Test]
        public void TheHeldOnFrame_BesideTheShot_TheShotsOwnPngStays()
        {
            string root;
            try { root = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..")); }
            catch (System.Exception) { root = Path.GetFullPath("."); }
            string src = File.ReadAllText(Path.Combine(root, "Assets", "Scripts", "Game", "FeelReel.N63Same.cs"));
            Assert.That(src, Does.Not.Contain("string onPath = Path.Combine(dir, file);"), "the held ON frame never written over the shot's own PNG");
            Assert.That(src, Does.Contain("string onDir = SameRun63Math.OnDir(same63Root, dir), onPath = Path.Combine(onDir, file);"), "<side>-on/, the same name");
            Assert.That(src, Does.Contain("onPx = Grab63(onDir, onPath);"), "the held ON frame written there");
            Assert.That(src, Does.Contain("SameRun63Math.ToggleLine(file, same63NightRow, same63Kind, held, state, wroteOn ? onPath : \"NOT WRITTEN\", wrote ? offPath : \"NOT WRITTEN\", same63Shots)"), "the proof line names both halves");
        }

        [Test]
        public void ToggleLine_NamesBothHalves()
        {
            string l = S.ToggleLine("a.png", "n61.exitFrame61", S.Kind.Frame, true, "x", "C:/r/s1-on/a157/a.png", "C:/r/s1-off/a157/a.png", 1);
            StringAssert.Contains("| on C:/r/s1-on/a157/a.png | off C:/r/s1-off/a157/a.png", l);
            StringAssert.StartsWith("[same63] toggle fired: shot 1 a.png | row n61.exitFrame61 on -> off -> on", l);
        }

        [Test]
        public void HoldClock_FrozenDuringAHold_ContinuousAfter()
        {
            var h = new S.HoldClock();
            Assert.That(h.Holding, Is.False);
            Assert.That(h.Frame(100), Is.EqualTo(100), "no hold yet: the frame");
            Assert.That(h.Time(10.0), Is.EqualTo(10.0), "no hold yet: the time");
            Assert.That(h.Delta(0.016), Is.EqualTo(0.016));
            h.Begin(100, 10.0); // the end of frame 100: its on capture written
            Assert.That(h.Holding, Is.True);
            Assert.That(h.Frame(101), Is.EqualTo(100), "the held frame reads as frame 100");
            Assert.That(h.Time(10.25), Is.EqualTo(10.0), "and its time as frame 100's");
            Assert.That(h.Delta(0.25), Is.EqualTo(0.0), "no time passes");
            h.End(101, 10.25); // the end of frame 101: its off capture written
            Assert.That(h.Holding, Is.False);
            Assert.That(h.Frame(102), Is.EqualTo(101), "the next frame follows frame 100");
            Assert.That(h.Time(10.27), Is.EqualTo(10.02).Within(1e-12), "its time follows frame 100's by its own delta");
            Assert.That(h.Delta(0.02), Is.EqualTo(0.02));
            Assert.That(h.HeldFrames, Is.EqualTo(1));
            Assert.That(h.HeldTime, Is.EqualTo(0.25).Within(1e-12));
            h.Begin(200, 20.0);
            h.Begin(201, 20.5); // a second Begin inside a hold changes nothing
            Assert.That(h.Frame(201), Is.EqualTo(200 - 1));
            h.End(201, 20.1);
            h.End(202, 30.0); // a second End outside a hold changes nothing
            Assert.That(h.HeldFrames, Is.EqualTo(2), "two holds, one frame each");
            Assert.That(h.Frame(203), Is.EqualTo(201));
            Assert.That(h.Time(20.2), Is.EqualTo(20.2 - 0.25 - 0.1).Within(1e-12));
        }

        [Test]
        public void PlantRect_ExactlyTheSquareInsideTheScreen()
        {
            int px = UnityTuning.Same63PlantPx, at = UnityTuning.Same63PlantAt;
            Assert.That(px * px, Is.EqualTo(100), "the doneWhen's planted change: 10 x 10 px");
            Assert.That(S.PlantRect(1920, 1080, at, px), Is.EqualTo(new[] { at, at, px, px }));
            Assert.That(S.PlantRect(1280, 720, at, px), Is.EqualTo(new[] { at, at, px, px }));
            Assert.That(S.PlantRect(at + 4, at + 4, at, px), Is.EqualTo(new[] { at + 4 - px, at + 4 - px, px, px }), "a tiny screen: pushed inside");
        }

        [Test]
        public void ProofLine_OnePerShot()
        {
            string l = S.ToggleLine("shot-N29-L57-n57-kiln-kb3-clear.png", "n61.exitFrame61", S.Kind.Frame, true, "step 4120 feet (1.0, 0.0, 2.0) alive 0", "C:/r/s1-off/a157/shot-N29-L57-n57-kiln-kb3-clear.png", 1);
            StringAssert.StartsWith("[same63] toggle fired", l);
            StringAssert.Contains("shot-N29-L57-n57-kiln-kb3-clear.png", l);
            StringAssert.Contains("n61.exitFrame61", l);
            StringAssert.Contains("frame", l);
            StringAssert.Contains("sim held yes", l);
            StringAssert.Contains("s1-off", l);
            StringAssert.Contains("sim held NO", S.ToggleLine("a.png", "n63.noop63", S.Kind.Planted, false, "x", "y", 1));
            StringAssert.Contains("unclaimed", S.ToggleLine("a.png", "n61.gunYellow61", S.Kind.Unclaimed, true, "x", "y", 1));
        }
    }
}
