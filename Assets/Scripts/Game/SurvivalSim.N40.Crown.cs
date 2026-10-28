// NIGHT40 builder B5 (crown40; NIGHT40.md B5, <boss_rules> 5; GG 2026-10-13: "players have no idea what to do, and even I don't know how to
// damage it"): THE CROWN taught. Fills the lead's partials (SurvivalSim.N40.cs):
//   N40CrownStartRun  a run's start: the fight's line state back to none.
//   N40CrownStep      every step after N39Step (so after N37CrownStep's Crown37BossStep): while THE CROWN owns a fight, its entry line
//                     (design.n40.crown40.line.entry: "FLY THROUGH THE RING: {dash} DASH + {jump} JUMP") shows on the fight's first step
//                     and once more after the first fall or death (line.maxShows), through BossLine40 (Core/BossLine40.cs: the lesson
//                     widget resolves {dash} / {jump} to the bound buttons); the line goes when the fight ends.
// Off (crown40 off, --off40 / -skygrindOff40, a tape without the bit): nothing runs, nothing is emitted (NIGHT39's fight). No UnityEngine:
// SimHarness compiles this file. No audio: event "bossLine40" for GG.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT40 B5: THE CROWN's boss line (null until crown40's first step).</summary>
        public BossLine40 crownLine40;
        bool crown40Fight;
        int crown40FallsSeen, crown40RetriesSeen;
        string crown40Entry, crown40EntryPad;
        double crown40LineSeconds;
        int crown40LineMax;

        partial void N40CrownStartRun()
        {
            crownLine40?.Clear();
            crown40Fight = false;
            crown40FallsSeen = crown40RetriesSeen = 0;
            if (!n40.crown40) return;
            const string g = "design.n40.crown40.line.";
            crown40Entry = cfg.Str(g + "entry");
            crown40EntryPad = cfg.Has(g + "entryPad") ? cfg.Str(g + "entryPad") : crown40Entry;
            crown40LineSeconds = cfg.Num(g + "seconds");
            crown40LineMax = cfg.Int(g + "maxShows");
        }

        partial void N40CrownStep(double dt)
        {
            if (!n40.crown40) return;
            crownLine40 ??= new BossLine40(events, step);
            bool live = crown37 != null && crown37Owns && crown37.Alive && !crown37.Dying;
            if (!live)
            {
                if (crown40Fight) { crownLine40.Clear(); crown40Fight = false; }
                return;
            }
            if (!crown40Fight)
            {
                crown40Fight = true;
                string roomId = null;
                int room = -1;
                if (campaign != null && campaignLevel != null && campaign.room >= 0 && campaign.room < campaignLevel.rooms.Count)
                {
                    room = campaign.room;
                    roomId = campaignLevel.rooms[room].id;
                }
                crownLine40.Begin("crown37", roomId, room);
                crown40FallsSeen = crown37Falls;
                crown40RetriesSeen = crown37Retries;
                crownLine40.Say("entry", crown40Entry, crown40LineSeconds, crown40EntryPad, crown40LineMax);
            }
            else if (crown37Falls > crown40FallsSeen || crown37Retries > crown40RetriesSeen)
            {
                crown40FallsSeen = crown37Falls;
                crown40RetriesSeen = crown37Retries;
                crownLine40.Say("entry", crown40Entry, crown40LineSeconds, crown40EntryPad, crown40LineMax);
            }
            crownLine40.Step();
        }
    }
}
