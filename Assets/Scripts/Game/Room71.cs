// NIGHT71 builder B (leftover L1 "room71", design.n71.room71, a VIEW row: no tape bit; a play-build-only tool). GG judges the rebuilt rooms by
// playing (GG 2026-10-26: "Bots guard, I judge"), and a room deep in a level is minutes of play away. With the row on:
//   - THE LEVEL SELECT: left / right on a level's row picks where its run starts: the level's start (as always) or one of its rooms (the level
//     file's campaign rooms, links left out), named on the select's own hint line (the same label, font, size and colour; only its words);
//   - THE START: a run begun on a picked room holds DOORFREE through the player's own input path (N29Fixes.ForceDoorFreeHold, the hold RB 3 s /
//     F8 feeds) until the sim reads it on, then respawns the player at that room's spawn (SurvivalGame.Respawn, the reels' and DOORFREE's own
//     move); the campaign enters the room by its zone (Campaign24's DOORFREE rule) and its waves start as usual. The run is unranked (DOORFREE).
//   - `-skygrindStartRoom <room id>` (the proof; with -skygrindMap <level>): the same from the command line: the campaign run starts 1.5 s
//     after boot and goes to that room.
// Off (the row off, -skygrindOff71 or older, -skygrindNoRoom71): the select's left / right do nothing, its hint is NIGHT70's, the switch is
// ignored (said once). The sim's rules are untouched: nothing here runs inside a step. Proof lines (Player.log):
//   [room71] start room <id> '<name>' on <level>: DOORFREE on after <s> s, respawned at its spawn
//   [room71] room entered <id> '<name>' <s> s after the run began (unranked: DOORFREE)
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Room71
    {
        public const string Row = "room71";
        public const string NoSwitch = "-skygrindNoRoom71";
        public const string StartSwitch = "-skygrindStartRoom";
        public const int Night = 71;
        const string RowOff = "design.n71." + Row + " off";

        static string[] args;

        public static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Why the row is off (null: on): the row (NightRows.ViewOn), then the off switches.</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            return null;
        }

        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>The room the next campaign run starts in (null: the level's start). Static: it outlives the level select's scene reload.</summary>
        public static string PendingRoom;

        /// <summary>A level's rooms a run may start in: (id, name) of every campaign room of the level file's JSON that is not a link, in order.</summary>
        public static List<KeyValuePair<string, string>> RoomsOf(Dictionary<string, object> levelJson)
        {
            var list = new List<KeyValuePair<string, string>>();
            if (levelJson == null || !levelJson.TryGetValue("campaign", out object c) || !(c is Dictionary<string, object> camp)
                || !camp.TryGetValue("rooms", out object r) || !(r is List<object> rooms)) return list;
            foreach (object o in rooms)
            {
                if (!(o is Dictionary<string, object> room) || !room.TryGetValue("id", out object id) || !(id is string sid) || sid.Length == 0) continue;
                string kind = room.TryGetValue("kind", out object k) && k is string sk ? sk : "room";
                if (kind == "link") continue;
                string name = room.TryGetValue("name", out object n) && n is string sn ? sn : sid;
                list.Add(new KeyValuePair<string, string>(sid, name));
            }
            return list;
        }

        /// <summary>The pick after a left (-1) / right (+1) press: -1 is the level's start, 0..count-1 a room; it wraps.</summary>
        public static int Step(int pick, int dir, int count)
        {
            if (count <= 0) return -1;
            int span = count + 1; // the start and the rooms
            int at = ((pick + 1 + dir) % span + span) % span;
            return at - 1;
        }

        /// <summary>The select's hint line with the pick (`room` null: the level's start) in front of `hint` (the select's own words).</summary>
        public static string Hint(string hint, string roomId, string roomName) =>
            "<  >  START: " + (roomId == null ? "THE LEVEL'S START" : (roomName ?? roomId).ToUpperInvariant() + " (" + roomId + ")") + "      " + hint;

        public static string Seconds(double s) => s.ToString("0.00", CultureInfo.InvariantCulture);

        /// <summary>NIGHT71 (builder B's finding): the night-71 rows' own off switches, `-skygrindNo&lt;Row&gt;71` (voice71, words71, wyrm71, cyan71
        /// and every other design.n71 row), are the play card's A / B for GG, so they boot like a player's game, as -skygrindOff46 and later do
        /// (N29Fixes.IsAutomationArg). Before, any of them made the boot automation: N26DefaultCampaign skipped the CAMPAIGN default and BootMap
        /// ignored the level select's PendingMapId, so a play build started with one stayed on the default map (read from the code; not checked
        /// in a Player). The reels are unaffected: they pass -skygrindMap / -skygrindReel, which stay automation.</summary>
        public static bool PlayerSwitch71(string a) =>
            a != null && a.Length > 13 && a.StartsWith("-skygrindNo", StringComparison.OrdinalIgnoreCase) && a.EndsWith("71", StringComparison.Ordinal);
    }
}
