// NIGHT71 builder B (leftover L1 room71, design.n71.room71; Game/Room71.cs, the select's half Game/UiRoom71.cs, the run's half
// Game/Room71Runner.cs). Pinned: the switch; the pick's wrap (the level's start, then each room, back to the start); a level file's rooms
// (links left out, in order); the hint line keeps the select's own words. Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using R = Skygrind.Game.Room71;

namespace Skygrind.Game.Tests
{
    public class N71RoomTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>A fresh config (nothing read yet, so nothing cached) with design.n71.room71 set to `on`.</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var n71 = (Dictionary<string, object>)((Dictionary<string, object>)c.root["design"])["n71"];
            ((Dictionary<string, object>)n71[R.Row])["enabled"] = on;
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                // one config per state: GameConfig.Get caches each dotted path per instance (GameConfig.cs:66-73), so a flag flipped on a config
                // already read keeps its first answer (the main tree's run: this assert read the cached false)
                Assert.That(R.OffReason(WithRow(false), new string[0]), Is.Not.Null, "the repo: off");
                GameConfig c = WithRow(true);
                Assert.That(R.OffReason(c, new string[0]), Is.Null);
                Assert.That(R.OffReason(c, new[] { "-skygrindNoRoom71" }), Is.Not.Null);
                Assert.That(R.OffReason(c, new[] { "-skygrindOff65" }), Is.Not.Null);
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Step_TheStartThenEachRoomThenBack()
        {
            Assert.That(R.Step(-1, 1, 3), Is.EqualTo(0));
            Assert.That(R.Step(0, 1, 3), Is.EqualTo(1));
            Assert.That(R.Step(2, 1, 3), Is.EqualTo(-1), "past the last room: the level's start");
            Assert.That(R.Step(-1, -1, 3), Is.EqualTo(2), "left from the start: the last room");
            Assert.That(R.Step(0, -1, 3), Is.EqualTo(-1));
            Assert.That(R.Step(-1, 1, 0), Is.EqualTo(-1), "no rooms: always the start");
        }

        [Test]
        public void Rooms_ALevelFilesRoomsWithoutItsLinks()
        {
            string file = Path.Combine(Repo(), "Assets", "StreamingAssets", "Design", "levels", "n52-kiln.json");
            if (!File.Exists(file)) Assert.Ignore("no n52-kiln.json in this tree");
            var rooms = R.RoomsOf(PortData.LoadJson(file) as Dictionary<string, object>);
            var ids = rooms.ConvertAll(p => p.Key);
            Assert.That(ids, Does.Contain("kb3"), "the slate's proof room");
            Assert.That(ids, Does.Not.Contain("kb1"), "a link is not offered");
            Assert.That(R.RoomsOf(null), Is.Empty);
            Assert.That(R.RoomsOf(new Dictionary<string, object>()), Is.Empty);
        }

        [Test]
        public void Switches71_BootLikeAPlayersGame_TheReelsStayAutomation()
        {
            foreach (string a in new[] { "-skygrindNoVoice71", "-skygrindNoWords71", "-skygrindNoWyrm71", "-skygrindNoCyan71", "-skygrindNoRoom71", "-skygrindnocyan71" })
                Assert.That(N29Fixes.IsAutomationArg(a), Is.False, a + ": GG's A / B boots his campaign (the level select can load a level)");
            foreach (string a in new[] { "-skygrindMap", "-skygrindReel", "-skygrindReelOnly", "-skygrindStartRoom", "-skygrindNoOrder64", "-skygrindSmoke" })
                Assert.That(N29Fixes.IsAutomationArg(a), Is.True, a + ": still automation");
            Assert.That(N29Fixes.IsAutomationArg("-skygrindOff65"), Is.False, "NIGHT46's rule kept");
        }

        [Test]
        public void Hint_KeepsTheSelectsOwnWords()
        {
            const string own = "ENTER / A  START      ESC / B  BACK";
            Assert.That(R.Hint(own, null, null), Does.EndWith(own).And.Contain("THE LEVEL'S START"));
            Assert.That(R.Hint(own, "kb3", "3.3 Casting Floor"), Does.Contain("3.3 CASTING FLOOR (kb3)").And.EndWith(own));
        }
    }
}
