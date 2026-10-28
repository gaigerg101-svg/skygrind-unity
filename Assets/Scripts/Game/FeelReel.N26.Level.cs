// NIGHT26 (builder L): the reel scenarios `levelselect` and `ranks` (contracts level-select, ranks, campaign-default; hooks of FeelReel.N26.cs).
//   levelselect  TITLE's PLAY button is pressed in CAMPAIGN mode with a SEEDED profile (the first level cleared with an A, the second with a B): the level select opens; it
//                prints its rows (number | name | overall | time | kills | style | unlock), the cursor (the first level not cleared), the LOCKED count (levels 4+ and the
//                empty slots), the smallest text in px (18 or more), the first row's alpha on its first drawn frame (under 0.95: no pop-in), the focused row, and the press:
//                A on the cursor row records the level it loads (PendingMapId: the scene reload is skipped for the reel). RED: design.n26.levelSelect off (PLAY would start a
//                run: it is not pressed), the screen not drawn, no rows, a text under 18 px, a pop-in, the cursor not on the next level, a playable level 4, the press not
//                naming the cursor's level.
//   ranks        the best per level: a seeded P (S in all three, no restart) and an A that had one restart (14 points, never P) go through the profile's JSON (saved and read
//                back), the select shows the P and the A and a dash for the unplayed level; the numbers of each Rank26.Compute are printed. RED: the block off, or a letter
//                that is not the one expected.
// The profile the scenario seeds is snapshotted and restored when it ends (Ui.NoRankSave26 true: nothing is saved to disk). Every result line starts with the
// scenario's preconditions (what the profile held, the booted map, the order). Not run in the Editor when written.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N26LevelselectBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = LevelSelect26Scenario(g, dir, res, offs, pre, false);

        static partial void N26RanksBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = LevelSelect26Scenario(g, dir, res, offs, pre, true);

        static IEnumerator LevelSelect26Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, bool ranks)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string name = ranks ? "ranks" : "levelselect";
            // the levels the select would list (playable ones that have a map), in the curriculum's order
            N25Blocks n25 = N25Blocks.ForceOff ? N25Blocks.Off : N25Blocks.From(g.cfg);
            var ids = new List<string>();
            foreach (string id in Curriculum25.Order(g.cfg, n25)) if (g.Maps.Count > 0 && g.Maps.ToList26().Exists(m => m.campaignOnly && m.id == id)) ids.Add(id);
            int playable = Math.Min(ids.Count, LevelSel26.Params(g.cfg).playable);
            string pc = $"booted map '{g.MapId}', mode '{g.setup.mode}', levels in order [{string.Join(",", ids)}], {playable} playable, profile holds {g.profile.levelBest.Count} level card(s), design.n26.levelSelect {(LevelSel26.On(g) ? "on" : "off")}";
            if (!LevelSel26.On(g))
            {
                res[0] = pc + " | FAIL (RED): design.n26.levelSelect is off, so PLAY on CAMPAIGN starts a run (not pressed) and there is no select";
                yield break;
            }
            if (playable < 2)
            {
                res[0] = pc + " | FAIL (RED): fewer than two playable levels with a map file";
                yield break;
            }
            var fails = new List<string>();
            Profile snap = Profile.Read(g.profile.ToJson());
            offs.Add(() => { g.profile.CopyFrom(snap); Ui.NoRankSave26 = false; SurvivalGame.SkipSceneReload25 = false; SurvivalGame.PendingMapId = null; SurvivalGame.PendingMode = null; SurvivalGame.AutoStartCampaign25 = false; });
            Ui.NoRankSave26 = true;
            SurvivalGame.SkipSceneReload25 = true;
            Rank26Params rp = Rank26Params.From(g.cfg);
            RankTable26 table = rp.DefaultsFor(100); // par sum 100: time cuts 100 / 150 / 250 / 400 s
            // seeded cards: levels[0] and levels[1] (a third level stays unplayed)
            LevelRank26 first = ranks ? Rank26.Compute(table, rp, 90, 100, 5000, 0) : Rank26.Compute(table, rp, 150, 85, 2500, 0);
            LevelRank26 second = ranks ? Rank26.Compute(table, rp, 90, 100, 5000, 1) : Rank26.Compute(table, rp, 240, 70, 1200, 0);
            g.profile.levelBest.Clear();
            g.profile.SubmitLevel(ids[0], first);
            g.profile.SubmitLevel(ids[1], second);
            if (ranks) // through the file's JSON, as a save and a load do
            {
                Profile back = Profile.Read(g.profile.ToJson());
                g.profile.CopyFrom(back);
            }
            string expectFirst = ranks ? "P" : "A", expectSecond = ranks ? "A" : "B";
            if (first.aggregate.ToString() != expectFirst) fails.Add($"the first seeded run ranks '{first.aggregate}', expected {expectFirst} ({first.Line}, {first.points} pts)");
            if (second.aggregate.ToString() != expectSecond) fails.Add($"the second seeded run ranks '{second.aggregate}', expected {expectSecond} ({second.Line}, {second.points} pts, {second.restarts} restart)");
            if (ranks && second.p) fails.Add("a run with a restart came out P");
            g.ToTitle();
            yield return Frames(60);
            g.setup.mode = RunSetup.Campaign; // CAMPAIGN is the mode (the default-mode line is the lead's boot; the reel sets it as the MODE row would)
            Start(g, name, "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            bool pressed = g.UiView != null && g.UiView.ReelPressPlay26();
            if (!pressed) fails.Add("TITLE's PLAY button could not be pressed (not on TITLE)");
            yield return Frames(Ms(500));
            if (!Ui.LevelSel26Open) fails.Add("PLAY on CAMPAIGN did not open the level select (Ui.LevelSel26Open false)");
            if (Ui.LevelSel26RowsNow.Count != LevelSel26.Params(g.cfg).slots) fails.Add($"{Ui.LevelSel26RowsNow.Count} rows, not {LevelSel26.Params(g.cfg).slots}");
            if (Ui.LevelSel26MinPx < 18f) fails.Add($"the smallest text is {Ui.LevelSel26MinPx.ToString("0", ci)} px, under 18");
            if (Ui.LevelSel26FirstAlpha < 0 || Ui.LevelSel26FirstAlpha >= 0.95f) fails.Add($"the first row's first drawn frame is at alpha {Ui.LevelSel26FirstAlpha.ToString("0.00", ci)} (a pop-in)");
            int expectLocked = LevelSel26.Params(g.cfg).slots - playable;
            if (Ui.LevelSel26Locked != expectLocked) fails.Add($"{Ui.LevelSel26Locked} LOCKED slots, expected {expectLocked} (levels past the {playable} playable ones)");
            if (Ui.LevelSel26RowsNow.Count > 1)
            {
                if (!Ui.LevelSel26RowsNow[0].Contains("| " + expectFirst + " |")) fails.Add($"row 1 does not show its {expectFirst}: '{Ui.LevelSel26RowsNow[0]}'");
                if (!Ui.LevelSel26RowsNow[1].Contains("| " + expectSecond + " |")) fails.Add($"row 2 does not show its {expectSecond}: '{Ui.LevelSel26RowsNow[1]}'");
            }
            string expectCursor = playable >= 3 ? ids[2] : ids[playable - 1];
            for (int w = 0; w < Ms(1500) && g.UiView != null && g.UiView.ReelFocusedRow26() == ""; w++) yield return null;
            string focused = g.UiView != null ? g.UiView.ReelFocusedRow26() : "";
            if (Ui.LevelSel26CursorNow != expectCursor) fails.Add($"the cursor is on '{Ui.LevelSel26CursorNow}', expected '{expectCursor}' (the first level not cleared)");
            if (focused != "Level " + (ids.IndexOf(expectCursor) + 1)) fails.Add($"the focused row is '{focused}', expected 'Level {ids.IndexOf(expectCursor) + 1}'");
            string snapRows = string.Join(" ;; ", Ui.LevelSel26RowsNow.GetRange(0, Math.Min(4, Ui.LevelSel26RowsNow.Count)));
            string went = "";
            if (!ranks)
            {
                SurvivalGame.PendingMapId = null;
                FeelProbe.Event("levelPress");
                string pressedId = g.UiView != null ? g.UiView.ReelPressLevel26() : "";
                yield return Frames(Ms(300));
                went = SurvivalGame.PendingMapId;
                if (went != expectCursor) fails.Add($"A on the cursor went to '{went}', expected '{expectCursor}' (press returned '{pressedId}')");
            }
            yield return Stop();
            string tail = ranks
                ? $"first {first.Line} ({first.points} pts, {first.restarts} restarts, P {first.p}); second {second.Line} ({second.points} pts, {second.restarts} restart, P {second.p}); after the JSON round trip the profile holds {g.profile.levelBest.Count} card(s)"
                : $"CONTINUE would go to '{expectCursor}'; A went to '{went}'";
            res[0] = pc + $" | {FeelProbe.FrameCount} frames; rows 1-4 [{snapRows}]; cursor '{Ui.LevelSel26CursorNow}', focused '{focused}' (hold ended by '{Ui.LevelSel26HeldBy}'); slots [{Ui.LevelSel26SlotsNow}]; LOCKED {Ui.LevelSel26Locked}; smallest text {Ui.LevelSel26MinPx.ToString("0", ci)} px; first row alpha {Ui.LevelSel26FirstAlpha.ToString("0.00", ci)}; {tail} | "
                     + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

        static List<SurvivalGame.MapEntry> ToList26(this IReadOnlyList<SurvivalGame.MapEntry> maps)
        {
            var l = new List<SurvivalGame.MapEntry>(maps.Count);
            foreach (SurvivalGame.MapEntry m in maps) l.Add(m);
            return l;
        }
    }
}
