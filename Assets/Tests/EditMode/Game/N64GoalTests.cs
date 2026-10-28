// NIGHT64 builder D (slate C4, THE EXIT THAT READS DANGER; view row design.n64.goal64, OFF; Goal64Math.N64D.cs, Route47View.cs). gg-proxy
// (Logs/n63/gg-proxy.md #4): "a red-orange vertical line drops from a ceiling lamp down the cyan door" on 5 of 5 spire frames: THE CROWN's goal
// column (Route47View, red-gold #FF8A3A, in the danger band; CLAUDE.md rule 8: red-orange is enemies and danger only). Pinned:
//   1. the column's colour is the colour table's goal route (design.n63.colours63.numbers.routes.goal47: the boundary's pale violet-white) at the
//      red-gold's own luminance, read with Colours63Math.From(cfg) (it reads the table whatever colours63's enabled), never typed, never danger
//   2. never Colours63.Route: it returns its input while colours63 is off (no Colours63 instance here: the red team's point, redteam.md)
//   3. no table, or a table with no goal route: false and the input unchanged (the view then draws the red-gold, as today)
//   4. THE SPLIT (the red team: Route47View.cs:58 gave the column and the window shafts one colour): the column takes the goal colour while
//      the row is on; the shafts keep theirs (colours63's route, the red-gold while colours63 is off: GG's call, DECIDE64)
//   5. the switch: off under -skygrindOff64 (or an older night's) and -skygrindNoGoal64 (any case); live every frame through NightRows.Live,
//      D1's reel override first (the same-run toggle's held off frame)
//   6. the proof line (L-089) names the input that fired: the live value, the role, the column's and the shafts' colours, the frame
// Written test first (NIGHT64.md section 4): RED on its commit (Goal64Math's placeholders answer nothing).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.Colours63Math;

namespace Skygrind.Game.Tests
{
    public class N64GoalTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static readonly Color RedGold = Look.Hex(UnityTuning.CLook47RedGoldHex);

        [Test]
        public void GoalColour_IsTheTablesGoalRoute_TheBoundary_AtTheRedGoldsLuminance()
        {
            M.Table t = M.From(Cfg());
            Assert.That(t, Is.Not.Null, "design.n63.colours63.numbers reads as a table");
            Assert.That(M.RoleOf(t, RedGold.r, RedGold.g, RedGold.b), Is.EqualTo("danger"), "the red: THE CROWN's red-gold column is in the danger band");
            bool known = Goal64Math.GoalColour(t, RedGold.r, RedGold.g, RedGold.b, out double r, out double g, out double b, out string role);
            Assert.That(known, Is.True, "the table has the goal route");
            Assert.That(role, Is.EqualTo(t.Route(Goal64Math.Route)), "the role is the table's own route for goal47, never typed");
            Assert.That(role, Is.EqualTo("boundary"));
            M.ToRole(t, role, RedGold.r, RedGold.g, RedGold.b, out double er, out double eg, out double eb);
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { er, eg, eb }).Within(1e-12), "the table's ToRole at the red-gold's own luminance");
            Assert.That(M.RoleOf(t, r, g, b), Is.EqualTo("boundary"), "the column reads as the boundary's pale violet-white, not danger");
        }

        [Test]
        public void GoalColour_ReadsTheTable_WhileColours63IsOff_NeverColours63Route()
        {
            GameConfig c = Cfg();
            M.Table t = M.From(c);
            bool known = Goal64Math.GoalColour(t, RedGold.r, RedGold.g, RedGold.b, out double r, out double g, out double b, out _);
            Assert.That(known, Is.True, "From reads the table whatever design.n63.colours63.enabled says (" + c.Design("n63.colours63.enabled") + ")");
            Color viaRoute = Colours63.Route(Goal64Math.Route, RedGold);
            Assert.That(viaRoute, Is.EqualTo(RedGold), "Colours63.Route returns its input with no live colours63 (why the column must not use it)");
            Assert.That(M.RoleOf(t, r, g, b), Is.Not.EqualTo(M.RoleOf(t, viaRoute.r, viaRoute.g, viaRoute.b)), "the goal colour differs from what Route gives with colours63 off");
        }

        [Test]
        public void GoalColour_NoTableOrNoRoute_FalseAndUnchanged()
        {
            Assert.That(Goal64Math.GoalColour(null, 1, 0.5, 0.25, out double r, out double g, out double b, out string role), Is.False);
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { 1, 0.5, 0.25 }));
            Assert.That(role, Is.Null);
            var empty = new M.Table(); // no routes
            Assert.That(Goal64Math.GoalColour(empty, 1, 0.5, 0.25, out r, out g, out b, out role), Is.False);
            Assert.That(new[] { r, g, b }, Is.EqualTo(new[] { 1, 0.5, 0.25 }));
        }

        [Test]
        public void Column_SplitsTheColumnFromTheShafts()
        {
            var goal = new Color(0.6f, 0.55f, 0.8f, 1f);
            Color shafts = RedGold;
            Assert.That(Goal64Math.Column(true, true, goal, shafts), Is.EqualTo(goal), "row on: the column in the goal role");
            Assert.That(Goal64Math.Column(false, true, goal, shafts), Is.EqualTo(shafts), "row off: the column as today (the shafts' colour)");
            Assert.That(Goal64Math.Column(true, false, goal, shafts), Is.EqualTo(shafts), "no goal colour known: as today");
            Assert.That(Colours63.Route(Goal64Math.Route, RedGold), Is.EqualTo(RedGold), "the shafts (colours63's route) keep their red-gold while colours63 is off (GG's call, DECIDE64)");
        }

        [Test]
        public void Switches_Off64_OlderNights_OwnSwitch_AnyCase()
        {
            Assert.That(Goal64Math.Off(null), Is.False);
            Assert.That(Goal64Math.Off(new string[0]), Is.False);
            Assert.That(Goal64Math.Off(new[] { "-skygrindOff64" }), Is.True);
            Assert.That(Goal64Math.Off(new[] { "-skygrindOff57" }), Is.True, "an older night's switch covers it");
            Assert.That(Goal64Math.Off(new[] { "-skygrindOff65" }), Is.False);
            Assert.That(Goal64Math.Off(new[] { "-skygrindNoGoal64" }), Is.True);
            Assert.That(Goal64Math.Off(new[] { "-SKYGRINDNOGOAL64" }), Is.True, "case ignored (NIGHT63 R1 N5: the one rule)");
            Assert.That(Goal64Math.Off(new[] { "-skygrindNoGoal63" }), Is.False);
            Assert.That(Goal64Math.NoSwitch, Is.EqualTo("-skygrindNoGoal64"));
            Assert.That(Goal64Math.Row, Is.EqualTo("goal64"));
        }

        [Test]
        public void Live_ReadsTheRowEveryFrame_TheReelOverrideFirst()
        {
            GameConfig c = Cfg();
            bool design = NightRows.ViewOn(c, Goal64Math.Row);
            try
            {
                NightRows.ReelOverride.Clear();
                Assert.That(Goal64Math.Live(c, new string[0]), Is.EqualTo(design), "no override: the row's own value");
                Assert.That(Goal64Math.Live(c, new[] { "-skygrindNoGoal64" }), Is.False, "its own switch");
                Assert.That(Goal64Math.Live(c, new[] { "-skygrindOff64" }), Is.False, "the night's switch");
                NightRows.ReelOverride[Goal64Math.Row] = true;
                Assert.That(Goal64Math.Live(c, new string[0]), Is.True, "D1's override on");
                Assert.That(Goal64Math.Live(c, new[] { "-skygrindNoGoal64" }), Is.True, "D1's override wins over the switches (NightRows.Live's contract)");
                NightRows.ReelOverride[Goal64Math.Row] = false;
                Assert.That(Goal64Math.Live(c, new string[0]), Is.False, "D1's held off frame");
            }
            finally { NightRows.ReelOverride.Remove(Goal64Math.Row); }
        }

        [Test]
        public void LiveLine_NamesTheInputThatFired()
        {
            var goal = new Color(0.633f, 0.592f, 0.803f, 1f);
            string on = Goal64Math.LiveLine(true, "boundary", RedGold, goal, RedGold, true, 1234, 5.5);
            StringAssert.StartsWith("[goal64] column live on", on);
            StringAssert.Contains("boundary", on);
            StringAssert.Contains("#FF8A3A", on, "the red-gold it came from");
            StringAssert.Contains("shafts #FF8A3A", on);
            StringAssert.Contains("frame 1234", on);
            StringAssert.Contains("column built yes", on);
            string off = Goal64Math.LiveLine(false, "boundary", RedGold, RedGold, RedGold, false, 7, 0);
            StringAssert.StartsWith("[goal64] column live off", off);
            StringAssert.Contains("column built NO", off);
        }

        [Test]
        public void Route47View_ColumnReadsGoal64_ShaftsKeepColours63sRoute()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "Route47View.cs"));
            StringAssert.Contains("Goal64Math.Live(", src, "the row read every frame");
            StringAssert.Contains("Colours63Math.From(", src, "the table read with From, never Colours63.Route, for the column");
            StringAssert.Contains("Colours63.Route(\"goal47\"", src, "the shafts keep colours63's route (NIGHT63 C1), untouched");
        }
    }
}
