// NIGHT24 (builder F): the reel scenario `cue`, the campaign's door cue and ROOM banner (UiCampaignCue24.cs). UNRUN when written. It needs a CAMPAIGN
// run (design.n24.campaign on, mode campaign): without one the line says SKIPPED (not a pass, not a fail) and why. The story:
//   clear   room 1's enemies are killed each frame until Campaign24 reads Cleared: the cue reads "DOOR OPEN | <n> m · <side>" within 1 s
//   reach   the player put at the room's exit: the cue hides (within directed.hideWithin m)
//   banner  the player put at room 2's spawn (a LIMIT room, NO DASH): the banner reads "ROOM 2 · NO DASH", at 18 px or more
// Registered by the lead (FeelReel.N24.cs owns the phase list): see the proposal in the commit message.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        /// <summary>Called by the lead's switch in FeelReel.N24.cs (one `case "cue"` line).</summary>
        public static IEnumerator N24CueScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot);
            if (!g.sim.campaignOn && N24CampaignGate(g) == null) { N24StartCampaign(g, offs); for (int w = 0; w < 5; w++) yield return null; } // NIGHT24: start its own run (the room scenario's boot)
            Campaign24 c = g.sim.campaignOn ? g.sim.campaign : null;
            if (!g.sim.campaignOn || !Ui.CampaignCue24Built)
            {
                res[0] = pc + " | SKIPPED: no CAMPAIGN run (" + (!g.sim.campaignOn ? "design.n24.campaign off or the mode is not campaign" : "the cue was not built: Ui.BuildN24Hud") + ")";
                yield break;
            }
            int guard0 = 0;
            while (g.sim.campaign == null && guard0++ < 600) yield return null;
            c = g.sim.campaign;
            if (c == null)
            {
                res[0] = pc + " | SKIPPED: the campaign object never started in 10 s";
                yield break;
            }
            var fails = new List<string>();
            Start(g, "cue", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // --- clear: room 1 killed until Cleared ----------------------------------------------------------------------------------
            int n = 0;
            while (c.phase != Campaign24.Phase.Cleared && n++ < Ms(90000))
            {
                God(g);
                if (c.wave >= c.Room.WaveCount) g.husks.KillAll();
                else if (g.sim.enemies.AliveCount > 0) g.husks.KillAll();
                yield return null;
            }
            if (c.phase != Campaign24.Phase.Cleared) fails.Add("room 1 was not cleared in 90 s");
            for (int w = 0; w < Ms(4000) && !Ui.Cue24TextNow.StartsWith("DOOR OPEN"); w++) yield return null; // the cue waits for a banner, at most UnityTuning.Cue25MaxWait
            yield return Frames(Ms(300));
            string cue = Ui.Cue24TextNow;
            if (!cue.StartsWith("DOOR OPEN")) fails.Add($"the cue read '{cue}', not DOOR OPEN ...");

            // --- reach: at the exit the cue hides ----------------------------------------------------------------------------------------
            RoomDef r1 = c.Level.rooms[0];
            if (g.sim.input is AutopilotInput) g.sim.UseInput(g.playerInput); // the bot would walk the player away from the exit during the check
            g.Respawn(new Vec3(r1.exit.x, r1.exit.y, r1.exit.z), 0);
            yield return Frames(Ms(600));
            string atExit = Ui.Cue24TextNow;
            if (atExit != "") fails.Add($"at the exit the cue still read '{atExit}'");

            // --- banner: room 2 (NO DASH) -----------------------------------------------------------------------------------------------
            RoomDef r2 = c.Level.rooms.Count > 1 ? c.Level.rooms[1] : null;
            string banner = "";
            if (r2 == null || r2.limits == 0) fails.Add("room 2 has no LIMIT in this plan");
            else
            {
                g.Respawn(r2.spawn, r2.spawnYawDeg);
                yield return Frames(Ms(800));
                banner = Ui.Banner24TextNow;
                if (!banner.StartsWith("ROOM 2")) fails.Add($"the banner read '{banner}', not ROOM 2 · <limit>");
                if (Ui.Banner24MinFontPx < 18f) fails.Add($"the banner text is {Ui.Banner24MinFontPx.ToString("0", ci)} px, under 18");
            }
            yield return Frames(Ms(300));
            yield return Stop();
            string overlap = HudOverlap25.Report(dir, out bool noOverlap); // NIGHT25: no two HUD text rects overlap at any frame (the cue sat on the room banner)
            if (!noOverlap) fails.Add(overlap);
            res[0] = pc + $" | {FeelProbe.FrameCount} frames; {overlap}; cue after the clear '{cue}'; at the exit '{atExit}'; banner '{banner}' ({Ui.Banner24MinFontPx.ToString("0", ci)} px) | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
