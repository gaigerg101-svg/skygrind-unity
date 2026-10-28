// NIGHT46 builder V1, the leftover row shift46 (design.n46.shift46; Shift46View.cs): its proof frames, through V1's a546 partial (no new
// phase: FeelReel.N46.cs untouched). Run when the Player is started with -skygrindShift46 (Shift46View.ReelFlag); one Player per level:
//   reel.sh + -skygrindMap n46-foundry + -skygrindReelOnly a546 + -skygrindShift46   (b1: the ladle's lamp)
//   reel.sh + -skygrindMap n46-spire   + -skygrindReelOnly a546 + -skygrindShift46   (v2: the BELL's lamp; v6: the organ's)
//   reel.sh + -skygrindMap n46-kiln    + -skygrindReelOnly a546 + -skygrindShift46   (k5: the crumble on the KILN MOUTH)
//   (n46-iceline's g7 and n46-span's p3 the same way, when there is time.)
// The reel starts the CAMPAIGN on the booted map (N24StartCampaign: -skygrindMap alone boots SURVIVAL, L-062), DOORFREE, God, enemies frozen.
// Per room of UnityTuning.Shift46Fixtures the level has: the player at the room's spawn, the look held on the fixture; then at the SAME
// spot and look:
//   S46-<room>-fight          shift46 on, the campaign's own state (the room's fight: no change, the fixture its own colour)
//   S46-<room>-last-off/-on   the last wave's change: off = today's look; on = the state forced LastWave (hot red-orange, pulsing)
//   S46-<room>-clear-off/-on  the room cleared: off = today's; on = the state forced Cleared (steady cyan)
// k5 (THE KILN'S HEART, walked in with walkin46): at its spawn looking at the KILN MOUTH's light: S46-k5-crumble-off / -on (the crumble forced).
// The state is forced (Shift46View.ForceRoom / ForceState) so each pair is the same moment; the line also prints the campaign's real state
// there (its rule, Shift46Rule, reads the campaign every frame). Files: dir/shot-N30-<name>.png (Shot30). GREEN when every room with a fixture
// gave its five frames (k5 its two) and the on frames drew the change (Shift46View.StateNow / BlendNow at the shot).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static IEnumerator Shift46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            AutopilotInput ap = N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            bool? forceWas = Shift46View.Force;
            offs.Add(() =>
            {
                Shift46View.Force = forceWas;
                Shift46View.ForceRoom = null;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
            });
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED shift46: the run did not start as a CAMPAIGN"; yield break; }
            LevelDef lv = sim.campaignLevel;
            float wallEnd = Time.realtimeSinceStartup + 300;
            while ((sim.campaign == null || sim.campaign.room < 0 || sim.campaign.phase == Campaign24.Phase.None) && Time.realtimeSinceStartup < wallEnd - 280)
            {
                AutopilotInput.TakeDraft(sim);
                yield return null;
            }
            yield return Frames(20);
            DoorFree41(g);
            Shift46View.Force = true; // builds the level's fixtures (FixtureAt)
            Shift46View.ForceRoom = null;
            yield return Frames(3);
            string pc = Preconditions(g, g.Feet);
            Start(g, "a546-shift46", "camera", dir);
            yield return Frames(pre);
            var lines = new List<string> { Shift46View.Line };
            int frames = 0, wanted = 0, roomsDone = 0, roomsWanted = 0, changed = 0, changesWanted = 0;

            string Real(string room) =>
                sim.campaign != null && sim.campaign.room >= 0 && sim.campaign.room < lv.rooms.Count
                    ? $"campaign room {lv.rooms[sim.campaign.room].id} phase {sim.campaign.phase} wave {sim.campaign.wave}/{lv.rooms[sim.campaign.room].WaveCount}, rule {Shift46Rule.RoomState(sim.campaign, lv, room)}"
                    : "campaign not in a room";

            // the look held on a Unity-space point from the eye (web: yaw 0 looks toward -z, + pitch up)
            void Hold(Vector3 target)
            {
                Vec3 eye = g.Eye;
                double dx = target.x - eye.x, dy = target.y - eye.y, dz = -target.z - eye.z;
                double yaw = Math.Atan2(-dx, -dz), pitch = Math.Max(-1.2, Math.Min(1.2, Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz))));
                sim.UseInput(new A1HoldLook45(yaw, pitch));
                g.input.SetLook(yaw, pitch);
            }

            IEnumerator Shot(string name, bool on, string room, int state, System.Text.StringBuilder sb)
            {
                wanted++;
                Shift46View.Force = on;
                Shift46View.ForceRoom = state >= 0 ? room : null;
                Shift46View.ForceState = Math.Max(0, state);
                yield return Wait27(g, Math.Max(UnityTuning.Shift46EaseIn, UnityTuning.Shift46EaseOut) + 0.35, wallEnd, () => God(g)); // a change eased fully in, or fully back out
                yield return Shot30(dir, name);
                frames++;
                sb.Append($" {name}: shown {Shift46View.RoomNow} state {Shift46View.StateNow} blend {Shift46View.BlendNow.ToString("0.00", ci)};");
                if (on && state > 0)
                {
                    changesWanted++;
                    if (Shift46View.StateNow == state && Shift46View.BlendNow > 0.9f) changed++;
                }
            }

            for (int i = 0; i < UnityTuning.Shift46Fixtures.GetLength(0); i++)
            {
                string room = UnityTuning.Shift46Fixtures[i, 0];
                RoomDef rd = lv.rooms.Find(r => r.id == room);
                if (rd == null) continue;
                if (!Shift46View.FixtureAt.TryGetValue(room, out Vector3 fixAt)) { lines.Add($"S46-{room}: the level has the room but not its fixture {UnityTuning.Shift46Fixtures[i, 1]} (skipped)"); continue; }
                roomsWanted++;
                Shift46View.Force = true;
                Shift46View.ForceRoom = null;
                g.Respawn(rd.spawn, rd.spawnYawDeg);
                float placeEnd = Time.realtimeSinceStartup + 5;
                while ((sim.campaign == null || sim.campaign.room < 0 || lv.rooms[sim.campaign.room].id != room) && Time.realtimeSinceStartup < placeEnd) yield return null;
                g.husks.frozen = true;
                God(g);
                Hold(fixAt);
                yield return Frames(10);
                var sb = new System.Text.StringBuilder($"S46-{room} ({Real(room)}):");
                yield return Shot($"S46-{room}-fight", true, room, -1, sb);
                yield return Shot($"S46-{room}-last-off", false, room, Shift46Rule.LastWave, sb);
                yield return Shot($"S46-{room}-last-on", true, room, Shift46Rule.LastWave, sb);
                yield return Shot($"S46-{room}-clear-off", false, room, Shift46Rule.Cleared, sb);
                yield return Shot($"S46-{room}-clear-on", true, room, Shift46Rule.Cleared, sb);
                lines.Add(sb.ToString());
                roomsDone++;
                Shift46View.ForceRoom = null;
            }

            RoomDef k5 = lv.rooms.Find(r => r.id == "k5");
            if (k5 != null)
            {
                roomsWanted++;
                Shift46View.Force = true;
                Shift46View.ForceRoom = null;
                g.Respawn(k5.spawn, k5.spawnYawDeg);
                yield return Wait27(g, 1.5, wallEnd, () => God(g)); // the worm's arena draws (walkin46: from the door on)
                GameObject mouth = GameObject.Find(UnityTuning.Shift46CrumbleLights[0]);
                if (mouth != null) Hold(mouth.transform.position);
                yield return Frames(10);
                var sb = new System.Text.StringBuilder($"S46-k5 (crumble state {sim.wyrm31CrumbleState}, KILN MOUTH light {(mouth != null ? "found" : "NOT FOUND")}):");
                yield return Shot("S46-k5-crumble-off", false, "k5", Shift46Rule.Crumble, sb);
                yield return Shot("S46-k5-crumble-on", true, "k5", Shift46Rule.Crumble, sb);
                lines.Add(sb.ToString());
                if (mouth != null) roomsDone++;
                Shift46View.ForceRoom = null;
            }
            yield return Stop();
            bool ok = roomsWanted > 0 && roomsDone == roomsWanted && frames == wanted && changed == changesWanted;
            res[0] = $"{(ok ? "GREEN" : "RED")} a546 shift46 | level {lv.id} (BaseId {NightRows.BaseId(g.cfg, lv.id)}), rooms {roomsDone}/{roomsWanted}, frames {frames}/{wanted}, changes drawn {changed}/{changesWanted} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
