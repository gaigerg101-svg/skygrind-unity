// NIGHT24 reel `limit` (builder L; UNCOMPILED, written without the Editor): the LIMIT rooms and the loadout on the Player. The bot plays
// layer 1 through room 3. Printed, per the contract: the weapons owned BEFORE a LIMIT room, INSIDE it and AFTER it; the room's flag text
// (banner); a held dash key in NO DASH (room 2) starts no dash; the style bonus once per cleared LIMIT room; the THUNDERCLAP awarded at the
// end of room 2 (weaponEarned) and back in the loadout after the PISTOL ONLY room (room 3) is cleared.
// Boot with -skygrindMap layer1. RED: "NOT WRITTEN (RED)" before this file; "SKIPPED (RED)" while the campaign row is off.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static string N24Owned(SurvivalGame g) => string.Join("+", g.sim.loadout.owned);

        static partial void N24LimitBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N24LimitScenario(g, dir, res, offs, pre);
        }

        static IEnumerator N24LimitScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            int bonus = 0, earned = 0, limitOn = 0, limitOff = 0, dashStarts = 0;
            string banner = "-", banner2 = "-", banner3 = "-", earnedName = "-";
            double bonusPoints = 0;
            offs.Add(g.events.On<StyleBonusEvent>("styleBonus", e => { if (e.name != null && e.name.StartsWith("LIMIT", StringComparison.Ordinal)) { bonus++; bonusPoints += e.points; } }));
            offs.Add(g.events.On<WeaponEarnedEvent>("weaponEarned", e => { earned++; earnedName = e.name; }));
            offs.Add(g.events.On<RoomEvent>("limitOn", e =>
            {
                limitOn++;
                banner = e.name;
                if (e.room == 1) banner2 = e.name; // the event carries its room: room 2's text is room 2's, not the last one raised
                else if (e.room == 2) banner3 = e.name;
            }));
            offs.Add(g.events.On<RoomEvent>("limitOff", _ => limitOff++));
            N24StartCampaign(g, offs);
            string pre0 = Preconditions(g, g.Feet);
            if (!g.sim.campaignOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            yield return Frames(20);
            Start(g, "limit", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            string before = N24Owned(g), inside2 = "-", after2 = "-", inside3 = "-", after3 = "-";
            int dashesInNoDash = -1;
            int stage = 0;
            bool wasDashing = false;
            for (int i = 0; i < Ms(240000) && stage < 5; i++)
            {
                AutopilotInput.TakeDraft(g.sim);
                Campaign24 c = g.sim.campaign;
                bool dashing = g.sim.mover.state == MoveState.Dash;
                if (dashing && !wasDashing) dashStarts++; // a dash begins (the Mover keeps no event for it)
                wasDashing = dashing;
                if (c != null)
                {
                    if (stage == 0 && c.room == 1) // inside room 2 (NO DASH): hold the dash key a while
                    {
                        inside2 = N24Owned(g);
                        stage = 1;
                        dashesInNoDash = dashStarts;
                    }
                    else if (stage == 1 && c.room == 1 && c.phase == Campaign24.Phase.Cleared) { after2 = N24Owned(g); dashesInNoDash = dashStarts - dashesInNoDash; stage = 2; }
                    else if (stage == 2 && c.room == 2) { inside3 = N24Owned(g); stage = 3; }
                    else if (stage == 3 && c.room == 2 && c.phase == Campaign24.Phase.Cleared) { after3 = N24Owned(g); stage = 5; }
                }
                yield return null;
            }
            yield return Frames(Ms(1000));
            yield return Stop();
            res[0] = $"{pre0}; {FeelProbe.FrameCount} frames; owned before: {before}; room 2 (NO DASH, banner '{banner2}'): inside {inside2}, dashes started while it held {dashesInNoDash} (want 0), after the door {after2}; " +
                     $"weaponEarned {earned}x ({earnedName}); room 3 (PISTOL ONLY, banner '{banner3}'): inside {inside3}, after the door {after3}; limitOn {limitOn}x, limitOff {limitOff}x, LIMIT style bonus {bonus}x ({bonusPoints:0} pts)";
        }
    }
}
