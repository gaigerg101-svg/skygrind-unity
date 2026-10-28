// NIGHT46 builder B1 (NIGHT46.md card B1; docs/ROOMS.md "NIGHT46 identity cards", k5 THE KILN'S HEART, walked into): the Player scenario `a446`
// (dispatched by FeelReel.N46.cs, the shared hook). Not run by its author (no Player in a builder worktree).
// Boot level 3 as n46-kiln through reel.sh: -skygrindMap n46-kiln -skygrindReelOnly a446 (L-062). The reel forces walkin46 and wyrm46 on for its
// run (sim.rowsOverride, before the run starts; put back after). The player is God; input by forced frames only (L-046).
// Frames (dir/shot-N29-<name>.png; before each, level 3's teach card and the banners are cleared off the frame: Wyrm46ClearCards):
//   B1-wyrm46-kb1b-door-panel / -kb3b-door-panel   from the link's door (1 m in), looking between the cliff and the east wall: the cyan ride
//                              panel running into the climb (kiln46's run-up)
//   B1-wyrm46-kb1b-riding-face / -kb3b-riding-face  riding the panel (forward + toward the wall, jump held), 1.5 m short of the cliff's face
//   B1-wyrm46-door-kilnmouth-cone  from 3.4 REST (kb4's spawn), looking down d8's axis before the fight: the door's view of the ring with THE KILN
//                              MOUTH in the entry cone (the lead's gate line; the ring drawn before the fight by V1's `|| sim.Wyrm46ArenaHere`)
//   B1-wyrm46-lip-kilnmouth    1.5 m outside d8 (REST's door end), the same axis: the ring fills the view
//   B1-wyrm46-step-in          the first frame after the fight begins, walked in at a run through d8 (no teleport): the speed kept is printed
//   B1-wyrm46-mid              ~6 s into the fight from where the player walked in: the worm on its loop
//   B1-wyrm46-crumble-tell     the bar set just under half (a cheat, printed): the 13 m perches' crack tell
//   B1-wyrm46-after-crumble    0.5 s after they fall
// Line: GREEN when the run is on n46-kiln, the fight began with Boss36WalkIn true, the worm's centre is k5's (walkIn46.origin), no step of the
// walk moved more than 1 m, the speed on the begin's frame is >= 90 % of the last walk frame's, and the crumble ran (tell, then 2 fallen).
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
        static partial void N46Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Wyrm46Scenario(g, dir, res, offs, pre);

        static IEnumerator Wyrm46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            NightRowState before = sim.rowsOverride;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.husks.frozen = false; g.sim.rowsOverride = before; });
            try { sim.rowsOverride = sim.rowsRegistry.Defaults().Set("walkin46", true).Set("wyrm46", true); }
            catch (Exception e) { res[0] = "RED a446: the build lacks the rows walkin46 / wyrm46 (" + e.Message + ")"; yield break; }
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a446: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            if (sim.campaignLevel == null || sim.campaignLevel.id != "n46-kiln")
            {
                res[0] = $"RED a446: the run is on {sim.campaignLevel?.id ?? "?"}, not n46-kiln (boot -skygrindMap n46-kiln through reel.sh, L-062)";
                yield break;
            }
            g.forcedCombat = IdleCombat;
            God(g);
            RoomDef kb4 = sim.campaignLevel.rooms.Find(r => r.id == "kb4");
            if (kb4 == null) { res[0] = "RED a446: no room kb4 on this level"; yield break; }
            double Yaw(double dx, double dz) => Math.Atan2(-dx, -dz);
            double yaw = 0, pitch = 0.05;
            void Look(double y, double p, double moveZ = 0)
            {
                yaw = y;
                pitch = p;
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch, moveZ = moveZ };
            }
            IEnumerator Hold(float seconds) { for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { God(g); yield return null; } }

            var st = new double[4];
            var shots = new List<string>();
            Wyrm46Cleared = 0;
            IEnumerator Shot(string name)
            {
                Wyrm46ClearCards(g); // round 3: level 3's stale kiln43 card ("STEP 1 ...") and any banner off the frame (reel-only)
                yield return null;
                Wyrm46ClearCards(g);
                yield return Shot29(dir, name, st);
                shots.Add(name);
            }
            Start(g, "a446", "camera", dir);
            yield return Frames(pre);

            // 0. the cliff links' run-ups (kiln46): from the door with the cyan panel in view, then riding it at the cliff's face
            var rideNotes = new List<string>();
            foreach (var (id, face) in new[] { ("kb1b", -57.0), ("kb3b", -260.5) })
            {
                RoomDef lr = sim.campaignLevel.rooms.Find(r => r.id == id);
                if (lr == null) { rideNotes.Add(id + " missing"); continue; }
                g.Respawn(new Vec3(0, 0, lr.z1 - 1), 0);
                Look(Yaw(7.4, face - (lr.z1 - 1)) * 0.5, 0.12); // half way between the cliff ahead and the panel's end (east, at the face)
                yield return Hold(0.6f);
                yield return Shot("B1-wyrm46-" + id + "-door-panel");
                g.Respawn(new Vec3(6.6, 0, lr.z1 - 2.5), 0);
                Look(0, 0.05);
                yield return Hold(0.4f);
                bool rodeAtFace = false;
                double sp = 0;
                for (int f = 0; f < 300; f++)
                {
                    God(g);
                    g.input.SetLook(0, 0.05);
                    g.forcedMove = new InputFrame { yaw = 0, pitch = 0.05, moveZ = 1, moveX = 0.4, jump = new Button { held = true, pressed = f == 0 } };
                    yield return null;
                    if (sim.mover.position.z <= face + 1.5) { rodeAtFace = sim.mover.state == MoveState.WallRide; sp = sim.mover.Speed; break; }
                }
                g.forcedMove = new InputFrame { yaw = 0, pitch = 0.05, moveZ = 1, moveX = 0.4, jump = new Button { held = true } };
                yield return Shot("B1-wyrm46-" + id + "-riding-face");
                rideNotes.Add($"{id} riding at the face {rodeAtFace}, {sp.ToString("0.0", ci)} m/s");
                Look(0, 0.05);
                yield return Hold(0.3f);
            }

            // 1. the door's view from REST (kb4's spawn) and from d8's lip (1.5 m outside the door: the ring fills the view), before the fight
            g.Respawn(kb4.spawn, 0);
            Look(0, 0.02);
            yield return Hold(1.0f);
            yield return Shot("B1-wyrm46-door-kilnmouth-cone");
            g.Respawn(new Vec3(0, 0, kb4.z0 + 1.5), 0);
            Look(0, 0.04);
            yield return Hold(0.6f);
            yield return Shot("B1-wyrm46-lip-kilnmouth");
            bool arenaHere = sim.Wyrm46ArenaHere;
            g.Respawn(kb4.spawn, 0);
            Look(0, 0.02);
            yield return Hold(0.3f);

            // 2. walk in at a run, no stop at the door
            Vec3 last = sim.mover.position;
            double maxStep = 0, speedBefore = 0, speedAfter = 0;
            int frames = 0;
            Look(0, 0.02, 1);
            while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && frames++ < 600)
            {
                God(g);
                speedBefore = sim.mover.Speed;
                yield return null;
                maxStep = Math.Max(maxStep, (sim.mover.position - last).Length);
                last = sim.mover.position;
            }
            speedAfter = sim.mover.Speed;
            bool begun = sim.wyrm24 != null && sim.wyrm24.Alive;
            bool walkIn = sim.Boss36WalkIn;
            Vec3 centre = begun && sim.wyrm24.arenaCenter.HasValue ? sim.wyrm24.arenaCenter.Value : default;
            yield return Shot("B1-wyrm46-step-in");
            Look(0, 0.02);

            // 3. mid-fight
            if (begun)
            {
                yield return Hold(6f);
                yield return Shot("B1-wyrm46-mid");
            }

            // 4. the crumble (as a345's: a 6 m step resets the still armor, the bar written just under the mark and held there)
            int tellFrames = 0, fallen = 0;
            Wyrm w = sim.wyrm24;
            WyrmArena arena = sim.wyrm24Arena;
            if (begun && w.Alive && sim.wyrm31 != null && arena != null)
            {
                Vec3 o = sim.Wyrm24Spawn - arena.spawn;
                Vec3 perch = o;
                if (sim.wyrm31CrumbleBoxes.Count > 0 && sim.wyrm31CrumbleBoxes[0] < arena.boxes.Count)
                {
                    var (pmin, pmax, _) = arena.boxes[sim.wyrm31CrumbleBoxes[0]];
                    perch = o + new Vec3((pmin.x + pmax.x) * 0.5, pmax.y, (pmin.z + pmax.z) * 0.5);
                }
                Vec3 at = sim.mover.position;
                g.Respawn(new Vec3(at.x, at.y, at.z - 6), 0);
                for (int k = 0; k < 2; k++) { God(g); yield return null; }
                Vec3 eye = sim.mover.position + new Vec3(0, 1.6, 0), d = perch - eye;
                Look(Yaw(d.x, d.z), Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z)));
                double mark = Math.Min(0.49, sim.wyrm31.p.crumbleAt - 0.01);
                w.body.hp = w.body.maxHp * mark;
                void HoldBar() { if (w.Alive && w.HpFrac > mark) w.body.hp = w.body.maxHp * mark; }
                bool shotTell = false;
                for (float t = 0; t < 6f && sim.wyrm31CrumbleState != 2; t += Time.unscaledDeltaTime)
                {
                    God(g);
                    HoldBar();
                    if (sim.wyrm31CrumbleState == 1) tellFrames++;
                    if (!shotTell && sim.wyrm31CrumbleState == 1 && sim.Wyrm31CrumbleProgress > 0.5)
                    {
                        shotTell = true;
                        yield return Shot("B1-wyrm46-crumble-tell");
                    }
                    yield return null;
                }
                if (sim.wyrm31CrumbleState == 2)
                {
                    for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime) { God(g); HoldBar(); yield return null; }
                    for (int i = 0; i < arena.boxes.Count; i++) if (sim.Wyrm31Fallen(i)) fallen++;
                    yield return Shot("B1-wyrm46-after-crumble");
                }
            }
            yield return Stop();
            bool keep = speedBefore > 0 && speedAfter >= 0.9 * speedBefore;
            bool green = begun && walkIn && maxStep <= 1 && keep && Math.Abs(centre.z - (-327.5)) < 0.01 && tellFrames > 0 && fallen == 2;
            res[0] = $"{(green ? "GREEN" : "RED")} a446 | map {g.MapId}, level {sim.campaignLevel?.id}, arena {sim.wyrm39ArenaBuilt} | walked in: begun {begun} after {frames} frames, BossWalkIn {walkIn}, Wyrm46ArenaHere {arenaHere}, speed {speedBefore.ToString("0.00", ci)} -> {speedAfter.ToString("0.00", ci)} m/s (want >= 90 %), largest frame move {maxStep.ToString("0.00", ci)} m (want <= 1), the worm's centre ({centre.x.ToString("0.0", ci)}, {centre.z.ToString("0.0", ci)}) (want (0, -327.5)) | the crumble: tell frames {tellFrames}, fallen {fallen} (want 2; the bar was set: the reel's cheat) | run-ups: {string.Join("; ", rideNotes)} | cards cleared {Wyrm46Cleared} | frames {string.Join(", ", shots)}";
        }

        /// <summary>Round 3 (the lead: r2's door frame still showed level 3's "STEP 1" card): the cards and banners off a frame (reel-only). The banners by
        /// Ui.ClearBanners41 (the reels' own); the teach card (UiTeach35: the kiln43 chain's lines) by reflection on Ui's private queue / current card, as
        /// FeelReel.N41 reads its probes: no Ui file changes. Wyrm46Cleared counts the frames it found a card up.</summary>
        static int Wyrm46Cleared;

        static void Wyrm46ClearCards(SurvivalGame g)
        {
            Ui ui = g.UiView;
            if (ui == null) return;
            ui.ClearBanners41();
            const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            Type t = typeof(Ui);
            object q = t.GetField("t35Queue", F)?.GetValue(ui);
            object now = t.GetField("t35Now", F)?.GetValue(ui);
            if (now != null || (q is System.Collections.IList ql && ql.Count > 0)) Wyrm46Cleared++;
            (q as System.Collections.IList)?.Clear();
            t.GetField("t35Now", F)?.SetValue(ui, null);
            t.GetField("t35Text", F)?.SetValue(ui, null);
            Ui.Teach35TextNow = Ui.Teach35IdNow = "";
        }
    }
}
