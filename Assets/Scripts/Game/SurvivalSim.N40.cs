// NIGHT40 (the shared commit, the lead owns this file): the ten sim-changing NIGHT40 design blocks in the sim (Core/N40Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n40Override), --off40 / -skygrindOff40 (and every older off switch) take
// every block off (N40Blocks.ForceOff), otherwise the design defaults. The sim hooks a builder may fill in its OWN partial file (a partial
// with no body costs nothing; the call sites are the lead's, in SurvivalSim.Run.cs):
// - S1 (start40, kiln40, cut40): N40StartStartRun / N40StartStep; Campaign24.waveReady40 is S1's to set (the first-wave hold).
// - W1 (charge40): N40ChargeStartRun. The charge rule itself lives in Core/Guns/Charge35.cs (W1's file tonight).
// - R2 (ride40): N40RideStartRun. The numbers are read inside Core/Mover.cs's wallride35 branch (R2's).
// - L2 (iceline40): N40IcelineStartRun.   - B2 (brawl40): N40BrawlStartRun / N40BrawlStep.
// - B4 (loom40): N40LoomStartRun / N40LoomStep.   - B5 (crown40): N40CrownStartRun / N40CrownStep.   - E3 (foes40): N40FoesStartRun / N40FoesStep.
// The step hooks run right after N39Step (SurvivalSim.Run.cs).
// The level rows (kiln40, iceline40, loom40, crown40) each swap their `levelFrom` for their `levelTo` in the campaign order
// (Curriculum25.N40Levels). Their weapons are data (no builder code): `startSet` (the weapons a run of levelTo starts with, replacing the
// unlock table's), `awards` ([{room, weapon}]: given on entering that room of levelTo), and loom40's `dropWeapons` (out of every campaign
// start set and award while loom40 is on: the STITCHER). A new level id reads the unlock table through its levelFrom (N40TableId).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N40Blocks n40Design;

        /// <summary>A replay's or a harness's NIGHT40 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N40Blocks? n40Override;

        /// <summary>The NIGHT40 blocks the current run reads (set at StartRun).</summary>
        public N40Blocks n40;

        /// <summary>The level rows: bit index in N40Blocks, levelFrom, levelTo (read once at Init; null when the row has none).</summary>
        static readonly int[] N40LevelBits = { 1, 5, 7, 8 }; // kiln40, iceline40, loom40, crown40
        readonly string[] n40From = new string[4], n40To = new string[4];
        readonly List<string> n40Drops = new List<string>();
        readonly List<string> n40AwardRooms = new List<string>(), n40AwardWeapons = new List<string>();

        void N40Init()
        {
            n40Design = N40Blocks.From(cfg);
            for (int k = 0; k < N40LevelBits.Length; k++)
            {
                string row = "design.n40." + N40Blocks.Names[N40LevelBits[k]];
                n40From[k] = cfg.Has(row + ".levelFrom") ? cfg.Get(row + ".levelFrom") as string : null;
                n40To[k] = cfg.Has(row + ".levelTo") ? cfg.Get(row + ".levelTo") as string : null;
            }
        }

        void N40StartRun()
        {
            n40 = n40Override ?? (N40Blocks.ForceOff ? N40Blocks.Off : n40Design);
            n40Drops.Clear();
            if (n40.loom40 && cfg.Has("design.n40.loom40.dropWeapons")) n40Drops.AddRange(cfg.Strs("design.n40.loom40.dropWeapons"));
            n40AwardRooms.Clear();
            n40AwardWeapons.Clear();
            N40StartStartRun();
            N40ChargeStartRun();
            N40RideStartRun();
            N40IcelineStartRun();
            N40BrawlStartRun();
            N40LoomStartRun();
            N40CrownStartRun();
            N40FoesStartRun();
        }

        partial void N40StartStartRun(); // NIGHT40 S1
        partial void N40StartStep(double dt); // NIGHT40 S1: every step, after N39Step
        partial void N40ChargeStartRun(); // NIGHT40 W1
        partial void N40RideStartRun(); // NIGHT40 R2
        partial void N40IcelineStartRun(); // NIGHT40 L2
        partial void N40BrawlStartRun(); // NIGHT40 B2
        partial void N40BrawlStep(double dt); // NIGHT40 B2
        partial void N40LoomStartRun(); // NIGHT40 B4
        partial void N40LoomStep(double dt); // NIGHT40 B4
        partial void N40CrownStartRun(); // NIGHT40 B5
        partial void N40CrownStep(double dt); // NIGHT40 B5
        partial void N40FoesStartRun(); // NIGHT40 E3
        partial void N40FoesStep(double dt); // NIGHT40 E3

        /// <summary>NIGHT40: every step, after N39Step (SurvivalSim.Run.cs, the lead's line).</summary>
        void N40Step(double dt)
        {
            N40StartStep(dt);
            N40BrawlStep(dt);
            N40LoomStep(dt);
            N40CrownStep(dt);
            N40FoesStep(dt);
        }

        /// <summary>The level row (index into N40LevelBits) whose levelTo is this level and which is on this run; -1: none.</summary>
        int N40RowOf(string levelId)
        {
            if (levelId == null) return -1;
            for (int k = 0; k < N40LevelBits.Length; k++)
                if (n40To[k] == levelId && n40.Flag(N40LevelBits[k])) return k;
            return -1;
        }

        /// <summary>The unlock table's id for this level: a NIGHT40 level reads its slot's older id (levelFrom); any other id is itself.</summary>
        string N40TableId(string levelId)
        {
            int k = N40RowOf(levelId);
            return k >= 0 && n40From[k] != null ? n40From[k] : levelId;
        }

        /// <summary>Is this weapon taken out of the campaign this run (loom40's dropWeapons)?</summary>
        bool N40Dropped(string weapon) => n40Drops.Count > 0 && n40Drops.Contains(weapon);

        /// <summary>At a campaign level's start (once per campaign, not per step): the row's start set replaces the table's, the dropped
        /// weapons leave it, and the row's room awards are cached. With every NIGHT40 block off: the start set unchanged, no awards.</summary>
        List<string> N40UnlocksBegin(string levelId, List<string> start)
        {
            n40AwardRooms.Clear();
            n40AwardWeapons.Clear();
            int k = N40RowOf(levelId);
            if (k >= 0)
            {
                string row = "design.n40." + N40Blocks.Names[N40LevelBits[k]];
                if (start != null && cfg.Has(row + ".startSet")) start = new List<string>(cfg.Strs(row + ".startSet"));
                if (cfg.Has(row + ".awards"))
                    foreach (object o in cfg.Arr(row + ".awards"))
                    {
                        var d = PortData.Obj(o);
                        if (d.TryGetValue("room", out object r) && r is string rs && d.TryGetValue("weapon", out object w) && w is string ws)
                        {
                            n40AwardRooms.Add(rs);
                            n40AwardWeapons.Add(ws);
                        }
                    }
            }
            if (start != null && n40Drops.Count > 0)
                for (int i = start.Count - 1; i >= 0; i--)
                    if (n40Drops.Contains(start[i])) start.RemoveAt(i);
            return start;
        }

        /// <summary>Every step of a campaign level: the row's room awards (given on entering the room, once, as the table's RoomAward is).</summary>
        void N40RoomAwardsStep()
        {
            if (n40AwardRooms.Count == 0 || campaign.room < 0 || campaign.room >= campaignLevel.rooms.Count) return;
            string id = campaignLevel.rooms[campaign.room].id;
            for (int i = 0; i < n40AwardRooms.Count; i++)
            {
                if (n40AwardRooms[i] != id) continue;
                string weapon = n40AwardWeapons[i];
                if (N40Dropped(weapon) || campaign.Given31(weapon)) continue;
                if (loadout.owned.Contains(weapon)) campaign.MarkGiven31(weapon);
                else campaign.Award31(weapon);
            }
        }
    }
}
