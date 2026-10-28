// NIGHT32 builder F (DESIGN32 card F, row design.n32.level5): the Player scenario of LEVEL 5 THE SPIRE (the name is FeelReel.N27.cs PhaseN32F, the body is the
// partial N32FinaleBody). Not run by its author (no Player in the worktree).
// Boot with -skygrindMap n32-spire (the CAMPAIGN level) and design.n32.level5 on; DOORFREE is set (a room is entered from any room), God on.
//   level5   1. the TOUR: for every room of the booted level the player is respawned at its entry and, after 1.0 s, an eye-height frame is saved
//               (shot-N32-level5-<room>.png); one line per room: entered, the lesson on screen, enemies alive, the owned weapons, the feet.
//            2. the SELECT: back to TITLE, PLAY with the mode on CAMPAIGN opens the level select; frame shot-N32-level5-select.png; the line: the rows, slot 5's
//               text, the playable count and the LOCKED count.
//            PASS when every room was entered and framed, the owned weapons at v1 are the four (revolver, gauntlet, shotgun, nailgun), and the select's row 5
//            names THE SPIRE and is playable (LevelSel26.Params playable >= 5). RED with -skygrindOff32 (no slot 5 THE SPIRE; playable 4).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N32FinaleBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "level5") body = Level5Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The weapons LEVEL 5 starts with (design.n31.unlocks row n 5: the earns of levels 1-4 plus the KESTREL).</summary>
        static readonly string[] Level5Weapons = { "revolver", "gauntlet", "shotgun", "nailgun" };
        static IEnumerator Level5Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; Ui.NoRankSave26 = false; SurvivalGame.SkipSceneReload25 = false; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN"; yield break; }
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            LevelDef lv = sim.campaignLevel;
            if (lv.id != "n32-spire") { res[0] = $"RED: the booted level is '{lv.id}', not n32-spire (boot -skygrindMap n32-spire)"; yield break; }
            string ownedAtStart = sim.loadout != null ? string.Join("+", sim.loadout.owned) : "?";
            bool fourGuns = sim.loadout != null && Array.TrueForAll(Level5Weapons, w => sim.loadout.owned.Contains(w));
            string pc = Preconditions(g, g.Feet) + $" | level {lv.id}, design.n32.level5 {g.cfg.Design("n32.level5.enabled")}, boss5 {g.cfg.Design("n32.boss5.enabled")}, owned at start {ownedAtStart}";
            Start(g, "level5", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string>();
            int entered = 0, shots = 0;
            string ended = null;
            foreach (RoomDef r in lv.rooms)
            {
                if (ended != null) break;
                g.Respawn(r.spawn, r.spawnYawDeg);
                yield return Settle31(g, 1.0, () => (ended = Ended30(g)) != null);
                Vec3 ft = g.Feet;
                bool inRoom = sim.campaign != null && sim.campaign.room == lv.rooms.IndexOf(r) || ft.x >= r.x0 && ft.x <= r.x1 && ft.z >= r.z0 && ft.z <= r.z1 || r.kind == "boss" && g.MapId != lv.id;
                if (inRoom) entered++;
                if (ended == null) { yield return Shot30(dir, $"level5-{r.id}"); shots++; }
                string lesson = Ui.Lesson25Visible && !string.IsNullOrEmpty(Ui.Lesson25TextNow) ? Ui.Lesson25TextNow : "(none)";
                lines.Add($"{r.id} '{r.name}' [{r.kind}] entered {inRoom}: lesson '{lesson}', enemies alive {Alive30(sim)}, health {sim.health.hp.ToString("0", ci)}, feet ({ft.x.ToString("0.0", ci)}, {ft.y.ToString("0.0", ci)}, {ft.z.ToString("0.0", ci)}){(ended != null ? " | then " + ended : "")}");
            }
            // the SELECT (FeelReel.N26.Level.cs's way: TITLE, the mode on CAMPAIGN, PLAY)
            Ui.NoRankSave26 = true;
            SurvivalGame.SkipSceneReload25 = true;
            g.ToTitle();
            yield return Frames(60);
            g.setup.mode = RunSetup.Campaign;
            bool pressed = g.UiView != null && g.UiView.ReelPressPlay26();
            yield return Frames(Ms(800));
            bool open = Ui.LevelSel26Open;
            if (open) { yield return Shot30(dir, "level5-select"); shots++; }
            int playable = LevelSel26.Params(g.cfg).playable;
            string row5 = Ui.LevelSel26RowsNow.Count >= 5 ? Ui.LevelSel26RowsNow[4] : "(no row 5)";
            bool row5Spire = row5.IndexOf("SPIRE", StringComparison.Ordinal) >= 0;
            string rows = string.Join(" ;; ", Ui.LevelSel26RowsNow.GetRange(0, Math.Min(6, Ui.LevelSel26RowsNow.Count)));
            yield return Stop();
            bool lastResults = ended != null && ended.StartsWith("Results");
            bool ok = entered == lv.rooms.Count && (ended == null || lastResults) && fourGuns && pressed && open && row5Spire && playable >= 5;
            res[0] = $"{(ok ? "PASS" : "RED")}: {entered} of {lv.rooms.Count} rooms entered by respawn, {shots} frames (shot-N32-level5-<room>.png, -select.png); four guns at the start {fourGuns}; select open {open} (PLAY pressed {pressed}), row 5 '{row5}', playable {playable}, LOCKED {Ui.LevelSel26Locked}{(ended != null ? " | ended: " + ended : "")} | {pc} || rows [{rows}] || " + string.Join(" || ", lines);
        }
    }
}
