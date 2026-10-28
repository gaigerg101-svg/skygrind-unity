// NIGHT24 (the shared commit, the lead owns this file): the nine NIGHT24 design blocks in the sim (Core/N24Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n24Override), --off24 / -skygrindOff24 (and --off22,
// --off21, --off20, --off19) take every block off (N24Blocks.ForceOff), otherwise the design defaults. The builders hook in through
// the partial methods below (nothing runs without a body); each is called once at the run's start or once per step, from the
// lead's call sites (SurvivalSim.Run.cs).
// - L (levels) implements N24CampaignStartRun / N24CampaignStep (Game/SurvivalSim.Campaign24.cs) and fills campaign24 (the tape's state).
// - F (fist, rewards) implements N24FistStartRun / N24FistStep and N24BigPickStartRun (Game/SurvivalSim.Fist24.cs).
// - B (the worm) implements N24WyrmStartRun / N24WyrmStep (Game/SurvivalSim.Wyrm24.cs).
// - R (removals, pad, shotgun) implements N24ShotgunStartRun (Game/SurvivalSim.Shotgun24.cs).
namespace Skygrind.Game
{
    using System.Collections.Generic;
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT24 blocks, read once.</summary>
        N24Blocks n24Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N24Blocks? n24Override;

        /// <summary>The NIGHT24 blocks this run read (what a tape records).</summary>
        public N24Blocks n24;

        /// <summary>The campaign's run state (what a tape, format 8, records): builder L's partial fills it; SURVIVAL and THE GRIND leave it empty.</summary>
        public Campaign24State campaign24 = Campaign24State.None;

        /// <summary>A tape's replay: the campaign state its run read (null: none).</summary>
        public Campaign24State? campaign24Override;

        void N24Init() => n24Design = N24Blocks.From(cfg);

        void N24StartRun()
        {
            n24 = n24Override ?? (N24Blocks.ForceOff ? N24Blocks.Off : n24Design);
            campaign24 = campaign24Override ?? Campaign24State.None;
            N24ShotgunStartRun();
            Hitch24.Mark("n24:shotgun");
            N24FistStartRun();
            Hitch24.Mark("n24:fist");
            N24BigPickStartRun();
            Hitch24.Mark("n24:bigpick");
            N24CampaignStartRun();
            Hitch24.Mark("n24:campaign");
            N24WyrmStartRun();
            Hitch24.Mark("n24:wyrm");
        }

        partial void N24Sections(List<(string name, IRunState state)> list);
        partial void N24ShotgunStartRun();
        partial void N24FistStartRun();
        partial void N24FistStep(double dt);
        partial void N24BigPickStartRun();
        partial void N24CampaignStartRun();
        partial void N24CampaignStep(double dt);
        partial void N24WyrmStartRun();
        partial void N24WyrmStep(double dt);
    }
}
