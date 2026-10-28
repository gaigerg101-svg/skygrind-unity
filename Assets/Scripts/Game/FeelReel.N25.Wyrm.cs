// NIGHT25 builder W: the reel scenarios for the worm's three bugs and the aim pass (GG's screenshots 2026-10-08: the worm over an empty sky, no bar, it
// does not die). Each result line starts with the scenario's PRECONDITIONS: the boot path (map, mode, campaign on), the room, where the player stands.
//   wyrmvoid  the Player line per boot path: on a path with no campaign boss (SURVIVAL, THE GRIND, -skygrindMap arena) the worm must not start; on a CAMPAIGN
//             path the room is drawn (WyrmRoomView: boxes, light), the player stands on its floor with walls round it, the SURVIVAL round line is hidden
//             (WyrmFightLive). The lead boots the Player each way (TITLE > PLAY SURVIVAL; TITLE > MODE > CAMPAIGN; -skygrindMap layer1) and runs this scenario in each.
//   wyrmbar   the boss bar: the threat read is valid for the worm, name KILN WYRM, the HP fraction falls with a hit, the phase pips fall with the beats.
// Written before the fix and seen RED: the shared commit's hook answers "NOT WRITTEN (RED)" until this file exists; a boot where the room view is absent reads
// "no WyrmRoomView". Not run by its author (no Editor, no Player: the lead runs it).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static string WyrmPath(SurvivalGame g)
        {
            SurvivalSim s = g.sim;
            return $"boot map '{g.MapId}', mode '{s.setup.mode}', campaign run {(s.campaignOn ? "ON" : "off")}, design.n24.wyrm {(s.n24.wyrm ? "on" : "OFF")}, design.n25.wyrmAim {(s.n25.wyrmAim ? "on" : "off")}, worm alive at boot {(s.wyrm24 != null && s.wyrm24.Alive ? "YES" : "no")}";
        }

        static partial void N25WyrmvoidBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = WyrmVoidScenario(g, dir, res, pre);
        }

        static IEnumerator WyrmVoidScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            string path = WyrmPath(g);
            if (g.wyrmRoomView == null)
            {
                res[0] = path + " | RED: no WyrmRoomView (SurvivalGame.Wyrm24InitView does not build it)";
                yield break;
            }
            SurvivalSim sim = g.sim;
            if (sim.wyrm24 != null && sim.wyrm24.Alive)
            {
                res[0] = path + " | RED: the worm is alive at boot, before any campaign boss room";
                yield break;
            }
            God(g);
            yield return Frames(Ms(1500));
            if (!sim.campaignOn)
            {
                // a path with no campaign boss: nothing may start the worm in a few seconds of play, and the round line is the survival run's
                bool alive = sim.wyrm24 != null && sim.wyrm24.Alive;
                res[0] = $"{path}; {Preconditions(g, sim.mover.position)}; 1.5 s of play; worm alive {(alive ? "YES" : "no")}, WyrmFightLive {g.WyrmFightLive}, room view shown {g.wyrmRoomView.Shown} | "
                    + (alive || g.WyrmFightLive || g.wyrmRoomView.Shown ? "FAIL (RED): the worm started outside the campaign" : "PASS: no worm outside the campaign");
                yield break;
            }
            // a campaign path: the boss room as Campaign24.Enter does it (Wyrm24Begin, then the player to the room's spawn)
            if (!sim.Wyrm24Begin())
            {
                res[0] = path + " | RED: Wyrm24Begin failed (the files were not found, or no BossKit)";
                yield break;
            }
            Vec3 spawn = sim.Wyrm24Spawn;
            g.husks.frozen = true;
            g.Respawn(spawn, sim.wyrm24Arena.spawnYawDeg);
            g.input.SetLook(0, 0.1);
            for (int i = 0; i < Ms(1000); i++) { God(g); yield return null; }
            Start(g, "wyrmvoid", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            Vec3 feet = sim.mover.position;
            Vec3 eye = new Vec3(feet.x, feet.y + 1.6, feet.z);
            bool floor = sim.world.RayIntersect(eye, new Vec3(0, -1, 0), out RayHit down) && down.distance < 3;
            // walls: the four horizontal rays from the room's middle (2 m up) must each hit within the room's half width + a margin
            Vec3 mid = new Vec3(sim.wyrm24Origin.x, sim.wyrm24Origin.y + 2, sim.wyrm24Origin.z);
            double half = sim.wyrm24Arena.half + 3;
            int walls = 0;
            foreach (Vec3 d in new[] { new Vec3(1, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, 1), new Vec3(0, 0, -1) })
                if (sim.world.RayIntersect(mid, d, out RayHit h) && h.distance <= half) walls++;
            bool shown = g.wyrmRoomView.Shown;
            int boxes = g.wyrmRoomView.Boxes;
            bool live = g.WyrmFightLive;
            // the pixel check (the lead: the frame read backed by a number): the same view with the room hidden (the void: the sky and the horizon) and shown;
            // the median luminance (0..1) of the frame's lower third and upper third. A floor is visible when the lower third of the room frame differs from the
            // void's lower third by >= 0.03 AND from the void's sky (its upper third) by >= 0.02; the direction is printed (a dusk stone floor may be darker than the sky).
            var vo = new double[2];
            var rm = new double[2];
            g.wyrmRoomView.Suppress = true;
            yield return Frames(4);
            yield return LumaThirds(vo);
            g.wyrmRoomView.Suppress = false;
            yield return Frames(4);
            yield return LumaThirds(rm);
            double dFloor = Math.Abs(rm[0] - vo[0]), dSky = Math.Abs(rm[0] - vo[1]);
            if (dFloor < 0.03) fails.Add($"the lower third barely changes with the room ({vo[0]:0.000} void vs {rm[0]:0.000} room)");
            if (dSky < 0.02) fails.Add($"the room's lower third reads as the sky ({rm[0]:0.000} vs sky {vo[1]:0.000})");
            string px = $"median luminance lower third: void {vo[0]:0.000}, room {rm[0]:0.000} ({(rm[0] > vo[1] ? ">" : "<")} the void's sky {vo[1]:0.000}); upper third: void {vo[1]:0.000}, room {rm[1]:0.000}";
            if (!floor) fails.Add("no floor under the player");
            if (walls < 4) fails.Add($"only {walls} of 4 walls hit");
            if (!shown) fails.Add("the room view is not shown while the worm fights");
            if (boxes < 20) fails.Add($"the room mesh has {boxes} boxes");
            if (!live) fails.Add("WyrmFightLive is false while the worm is alive");
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {path}; {Preconditions(g, feet)}; room origin ({sim.wyrm24Origin.x:0}, {sim.wyrm24Origin.z:0}); floor under the player {floor}, walls hit {walls}/4, room view shown {shown} with {boxes} boxes, WyrmFightLive {live} (the lead hides the round line on it); {px} | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

        /// <summary>The median luminance (0..1) of the screen's lower third (o[0]) and upper third (o[1]), read at the frame's end (every 4th pixel).</summary>
        static IEnumerator LumaThirds(double[] o)
        {
            yield return new UnityEngine.WaitForEndOfFrame();
            UnityEngine.Texture2D tex = UnityEngine.ScreenCapture.CaptureScreenshotAsTexture();
            UnityEngine.Color32[] px = tex.GetPixels32();
            int w = tex.width, h = tex.height;
            UnityEngine.Object.Destroy(tex);
            if (o.Length > 2)
            {
                int mag = 0; // magenta (the RIVAL's only colour, rule 8): r high, g low, b high
                for (int i = 0; i < px.Length; i += 4)
                    if (px[i].r > 190 && px[i].b > 150 && px[i].g < 100) mag++;
                o[2] = mag;
            }
            for (int third = 0; third < 2; third++)
            {
                var hist = new int[256];
                int n = 0, y0 = third == 0 ? 0 : h * 2 / 3, y1 = third == 0 ? h / 3 : h; // texture row 0 is the screen's bottom
                for (int y = y0; y < y1; y += 4)
                    for (int x = 0; x < w; x += 4)
                    {
                        UnityEngine.Color32 c = px[y * w + x];
                        hist[(int)Math.Round(0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b)]++;
                        n++;
                    }
                int acc = 0, med = 0;
                for (; med < 255 && acc + hist[med] < n / 2; med++) acc += hist[med];
                o[third] = med / 255.0;
            }
        }

        static void AimAtWorm(SurvivalGame g, Wyrm w)
        {
            Vec3 f = g.Feet;
            double ex = f.x, ey = f.y + 1.6, ez = f.z;
            int pick = 0;
            for (int i = 0; i < w.PieceCount; i++)
                if (!w.buried[i]) { pick = i; break; }
            Vec3 t = w.boxes[pick].c;
            double dx = t.x - ex, dy = t.y - ey, dz = t.z - ez;
            g.input.SetLook(Math.Atan2(-dx, -dz), Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz))); // yaw 0 looks toward web -z
            g.forcedCombat = Fire;
        }

        static void AimAtPoint(SurvivalGame g, Vec3 t)
        {
            Vec3 f = g.Feet;
            double dx = t.x - f.x, dy = t.y - (f.y + 1.6), dz = t.z - f.z;
            g.input.SetLook(Math.Atan2(-dx, -dz), Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)));
            g.forcedCombat = Fire;
        }

        // --- `bosses`: every campaign boss room on a Player (builder W; the lead adds "bosses" to PhaseH and `case "bosses": N25BossesBody(...)` to N25Scenario) ---------
        static partial void N25BossesBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        static partial void N25BossesBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = BossesScenario(g, dir, res, pre);
        }

        static Vec3 BossAimPoint(SurvivalSim s, string id)
        {
            if (id == "monolith") return s.monolith24.CoreWorld();
            if (id == "bulwark") return s.bulwark.body.AimPoint;
            if (id == "rival") return s.rival25.body.AimPoint;
            Wyrm w = s.wyrm24;
            for (int i = 0; i < w.PieceCount; i++)
                if (!w.buried[i]) return w.boxes[i].c;
            return w.HeadPosition;
        }

        static double BossFrac(SurvivalSim s, string id) => id == "rival" ? s.rival25.HpFrac : id == "bulwark" ? s.bulwark.HpFrac : id == "monolith" ? s.monolith24.HpFrac : s.wyrm24.HpFrac;
        static string BossPhase(SurvivalSim s, string id) => id == "rival" ? s.rival25.phase : id == "bulwark" ? s.bulwark.phase : id == "monolith" ? s.monolith24.phase : s.wyrm24.phase;

        static void BossCut(SurvivalSim s, string id, double frac)
        {
            if (id == "rival") { s.rival25.body.hp = s.rival25.body.maxHp * frac; s.rival25.Apply(0); }
            else if (id == "bulwark") { s.bulwark.body.hp = s.bulwark.body.maxHp * frac; s.bulwark.Apply(0); }
            else if (id == "monolith") { s.monolith24.body.hp = s.monolith24.body.maxHp * frac; s.monolith24.Apply(0); }
            else { s.wyrm24.body.hp = s.wyrm24.body.maxHp * frac; s.wyrm24.Apply(0); }
        }

        static IEnumerator BossesScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            SurvivalSim sim = g.sim;
            bool? was = sim.bosses25Override;
            sim.rival25Override = true; // RIVAL PRIME has its own row: the scenario forces it for this run too
            sim.bosses25Override = true; // the scenario forces design.n25.bosses on for this run (the config row stays the lead's)
            string path = WyrmPath(g);
            var lines = new List<string>();
            var allFails = new List<string>();
            g.husks.frozen = true;
            foreach (string id in new[] { "wyrm", "bulwark", "monolith", "final", "rival" })
            {
                var fails = new List<string>();
                if (id == "rival")
                {
                    sim.rival25Override = false; // its own row OFF: no duel may start (RED)
                    bool offRival = sim.BossRoomBegin("rival", out _, out _);
                    sim.rival25Override = true;
                    lines.Add($"rival with design.n25.bosses.rival off: {(offRival ? "STARTED (FAIL)" : "no duel (RED, as it must)")}");
                    if (offRival) allFails.Add("the rival started with its row off");
                }
                if (id == "final")
                {
                    sim.bosses25Override = false; // the variant OFF: no ELDER WYRM may start (RED)
                    bool offBegan = sim.BossRoomBegin("final", out _, out _);
                    sim.bosses25Override = true;
                    lines.Add($"final with design.n25.bosses off: {(offBegan ? "STARTED (FAIL)" : "no fight (RED, as it must)")}");
                    if (offBegan) allFails.Add("final started with the row off");
                }
                if (!sim.BossRoomBegin(id, out Vec3 spawn, out double yaw)) { lines.Add($"{id}: RED: BossRoomBegin failed (row off, files missing or no BossKit)"); allFails.Add(id); continue; }
                g.Respawn(spawn, yaw);
                God(g);
                WyrmArena arena = sim.BossRoomArena(id, out Vec3 origin);
                IBossSystem boss = sim.BossRoomSystem(id);
                for (int i = 0; i < Ms(1500); i++) { God(g); yield return null; }
                // the bar
                var t = new HudReads20.Threat();
                HudReads20.ThreatNow(g, ref t);
                bool bar = t.valid && t.boss;
                string name = t.name;
                if (!bar) fails.Add("no boss bar");
                // the room: floor under the player, and the void-versus-room pixel check
                Vec3 feet = g.Feet;
                bool floor = sim.world.RayIntersect(new Vec3(feet.x, feet.y + 1.6, feet.z), new Vec3(0, -1, 0), out RayHit down) && down.distance < 3;
                bool shown = g.wyrmRoomView != null && g.wyrmRoomView.ShownFor(id);
                var vo = new double[3];
                var rm = new double[3];
                g.wyrmRoomView.Suppress = true;
                yield return Frames(4);
                yield return LumaThirds(vo);
                g.wyrmRoomView.Suppress = false;
                yield return Frames(4);
                yield return LumaThirds(rm);
                if (id != "rival" && (vo[2] > 0 || rm[2] > 0)) fails.Add($"magenta pixels ({vo[2]} / {rm[2]}) outside the rival");
                if (!floor) fails.Add("no floor under the player");
                if (!shown) fails.Add("room not drawn");
                if (Math.Abs(rm[0] - vo[0]) < 0.03) fails.Add($"lower third barely changes with the room ({vo[0]:0.000} vs {rm[0]:0.000})");
                // the fight: the real gun, the HP cut through the phases, a death in the last phase (the retry), then the kill
                int t0 = g.run.steps;
                int guard = 0;
                while (BossPhase(sim, id) == "intro" && guard++ < Ms(12000)) { God(g); yield return null; }
                for (int k = 0; k < Ms(8000); k++) { God(g); AimAtPoint(g, BossAimPoint(sim, id)); yield return null; }
                g.forcedCombat = null;
                if (id == "final") { foreach (double cut in new[] { 0.7, 0.45, 0.2 }) { BossCut(sim, id, cut); for (int k = 0; k < Ms(4000) && sim.wyrm24.phase == "beat"; k++) { God(g); yield return null; } for (int k = 0; k < Ms(300); k++) { God(g); yield return null; } } }
                else                 if (id == "bulwark" || id == "rival") { BossCut(sim, id, 0.45); for (int k = 0; k < Ms(5000) && BossPhase(sim, id) != "p2"; k++) { God(g); yield return null; } }
                else
                {
                    BossCut(sim, id, 0.6);
                    for (int k = 0; k < Ms(4000) && BossPhase(sim, id) != "p2"; k++) { God(g); yield return null; }
                    BossCut(sim, id, 0.3);
                    for (int k = 0; k < Ms(4000) && BossPhase(sim, id) != "p3"; k++) { God(g); yield return null; }
                }
                for (int k = 0; k < Ms(4000); k++) { God(g); AimAtPoint(g, BossAimPoint(sim, id)); yield return null; }
                g.forcedCombat = null;
                double before = BossFrac(sim, id);
                string phaseAtDeath = BossPhase(sim, id);
                int r0 = sim.bossRoom25Retries + sim.wyrm24Retries;
                sim.BossRoomBegin(id, out spawn, out yaw); // what Campaign24.Retry does after a player death: a live boss goes back to its phase start
                g.Respawn(spawn, yaw);
                double after = BossFrac(sim, id);
                int retries = sim.bossRoom25Retries + sim.wyrm24Retries - r0;
                if (retries < 1) fails.Add("the retry was not counted");
                if (!(after < 0.999)) fails.Add($"the retry restored full HP ({after:0.00})");
                for (int k = 0; k < Ms(150000) && boss.Alive; k++)
                {
                    God(g);
                    if (BossPhase(sim, id) != "dying") AimAtPoint(g, BossAimPoint(sim, id));
                    yield return null;
                }
                g.forcedCombat = null;
                bool dead = !boss.Alive;
                double secs = (g.run.steps - t0) * sim.cfg.Dt;
                if (!dead) fails.Add("not killed in 150 s");
                if (!g.events.Emitted.ContainsKey("bossDown")) fails.Add("no bossDown");
                yield return Frames(Ms(800));
                bool roomGone = g.wyrmRoomView != null && !g.wyrmRoomView.ShownFor(id);
                if (!roomGone) fails.Add("room still drawn after the kill");
                lines.Add($"{id}: bar {bar} '{name}', floor {floor}, room drawn {shown} (lower-third luminance void {vo[0]:0.000} vs room {rm[0]:0.000}, magenta px {rm[2]:0}), killed {dead} in {secs:0} s of game time with the real gun after the HP cuts, retry in {phaseAtDeath}: HP {before:0.00} -> {after:0.00} ({retries} counted) | {(fails.Count == 0 ? "PASS" : "FAIL: " + string.Join("; ", fails))}");
                allFails.AddRange(fails);
            }
            sim.bosses25Override = was;
            sim.rival25Override = null;
            res[0] = $"{path}; {string.Join(" || ", lines)} | {(allFails.Count == 0 ? "PASS" : "FAIL (RED)")}";
        }

        // --- `wyrmaim`: the aimed worm on a Player, bot-less (the player stands in the room, the real HP, topped up under 30): over a held phase-2 fight the aimed vs plain
        // spits, the lunge locks and re-aims, the player's hits per minute. RED while design.n25.wyrmAim is off, or when no spit / lock / re-aim is seen.
        static partial void N25WyrmaimBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = WyrmAimScenario(g, dir, res, pre);
        }

        static IEnumerator WyrmAimScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            SurvivalSim sim = g.sim;
            string path = WyrmPath(g);
            if (!sim.n24.wyrm || !sim.n25.wyrmAim || g.wyrmView == null)
            {
                res[0] = path + " | RED: the worm or design.n25.wyrmAim is off (or no view)";
                yield break;
            }
            if (!sim.Wyrm24Begin()) { res[0] = path + " | RED: Wyrm24Begin failed"; yield break; }
            Wyrm w = sim.wyrm24;
            g.husks.frozen = true;
            g.Respawn(sim.Wyrm24Spawn, sim.wyrm24Arena.spawnYawDeg);
            g.input.SetLook(0, 0.1);
            God(g);
            for (int k = 0; k < Ms(12000) && w.phase == "intro"; k++) { God(g); yield return null; }
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            for (int k = 0; k < Ms(4000) && w.phase != "p2"; k++) { God(g); yield return null; }
            g.health.max = g.health.hp = 100;
            string pc = Preconditions(g, g.Feet);
            Start(g, "wyrmaim", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var seen = new HashSet<WyrmShot>();
            int aimed = 0, plain = 0, shards = 0;
            int aims0 = w.stats["lungeAims"], reaims0 = w.stats["lungeReaims"], hit0 = w.stats["shotsHit"], c0 = w.stats["contactsBody"] + w.stats["contactsHead"] + w.stats["contactsLunge"];
            int frames = 0;
            double lost = 0, prev = g.health.hp;
            for (; frames < Ms(45000) && w.Alive && w.phase == "p2"; frames++)
            {
                foreach (WyrmShot s in w.shots)
                    if (seen.Add(s))
                    {
                        if (s.shard) shards++;
                        else if (s.aimed) aimed++;
                        else plain++;
                    }
                if (g.health.hp < prev) lost += prev - g.health.hp;
                if (g.health.hp < 30) g.health.hp = 100;
                prev = g.health.hp;
                g.input.SetLook(0, 0.1);
                yield return null;
            }
            double minutes = frames / (double)Ms(60000);
            int aims = w.stats["lungeAims"] - aims0, reaims = w.stats["lungeReaims"] - reaims0;
            int hits = (w.stats["shotsHit"] - hit0) + (w.stats["contactsBody"] + w.stats["contactsHead"] + w.stats["contactsLunge"] - c0);
            var fails = new List<string>();
            if (aimed == 0) fails.Add("no aimed spit in the window");
            if (aims == 0) fails.Add("no lunge lock in the window (the cycle did not reach a lunge)");
            if (reaims == 0) fails.Add("no lunge re-aim in the window");
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {path}; {pc}; held phase 2 for {frames / (double)Ms(1000):0.0} s standing in the room: spits aimed {aimed} / plain {plain} (aimedEvery {w.p.aimSpitEvery}), shards {shards}, lunge locks {aims}, midpoint re-aims {reaims}, player hits {hits} = {hits / Math.Max(1e-9, minutes):0.0} per minute (HP lost {lost:0}) | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

        static partial void N25WyrmdiesBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = WyrmDiesScenario(g, dir, res, offs, pre);
        }

        static IEnumerator WyrmDiesScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            bool campaign = N24CampaignGate(g) == null;
            if (campaign) N24StartCampaign(g, offs);
            string path = WyrmPath(g);
            int roomRetries = 0, layerClears = 0;
            offs.Add(g.events.On<RoomEvent>("roomRetry", _ => roomRetries++));
            offs.Add(g.events.On<RoomEvent>("layerClear", _ => layerClears++));
            yield return Frames(5);
            if (!sim.n24.wyrm || g.wyrmView == null)
            {
                res[0] = path + " | RED: the worm row is off or the view is missing";
                yield break;
            }
            var fails = new List<string>();
            LevelDef lv = sim.campaignLevel;
            int bossIndex = campaign && lv != null ? lv.rooms.FindIndex(r => r.kind == "boss") : -1;
            if (campaign && sim.campaignOn)
            {
                // the whole way to THE FURNACE by the bot (as the layer reel), then the fight by hand
                for (int i = 0; i < Ms(300000); i++)
                {
                    AutopilotInput.TakeDraft(sim);
                    Campaign24 c0 = sim.campaign;
                    if (c0 != null && (c0.finished || (bossIndex >= 0 && c0.room >= bossIndex))) break;
                    if (g.flow == Flow.Results) break;
                    yield return null;
                }
                for (int i = 0; i < Ms(1500) && (sim.wyrm24 == null || !sim.wyrm24.Alive); i++) yield return null;
            }
            else
            {
                if (!sim.Wyrm24Begin()) { res[0] = path + " | RED: Wyrm24Begin failed"; yield break; }
                g.husks.frozen = true;
                g.Respawn(sim.Wyrm24Spawn, 0);
            }
            Wyrm w = sim.wyrm24;
            if (w == null || !w.Alive) { res[0] = path + " | RED: no live worm to fight (the boss room was not reached)"; yield break; }
            string pc = Preconditions(g, g.Feet);
            Start(g, "wyrmdies", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int hits0 = g.events.Emitted.TryGetValue("bossHit", out int h0) ? h0 : 0;
            double hp0 = w.HpFrac;
            // 1. a real fight: the real gun at the worm for up to 25 s (the intro blocks damage: it ends by itself)
            int guard = 0;
            while ((w.phase == "intro" || guard < Ms(12000)) && guard++ < Ms(40000)) { God(g); AimAtWorm(g, w); yield return null; }
            g.forcedCombat = null;
            int realHits = (g.events.Emitted.TryGetValue("bossHit", out int h1) ? h1 : 0) - hits0;
            double hp1 = w.HpFrac;
            if (realHits <= 0) fails.Add("the real gun landed no hit");
            // 2. to the last phase (cut), then 6 s of real fire in it
            w.body.hp = w.body.maxHp * 0.6; w.Apply(0);
            for (int k = 0; k < Ms(3000) && w.phase != "p2"; k++) { God(g); yield return null; }
            w.body.hp = w.body.maxHp * 0.3; w.Apply(0);
            for (int k = 0; k < Ms(3000) && w.phase != "p3"; k++) { God(g); yield return null; }
            for (int k = 0; k < Ms(6000); k++) { God(g); AimAtWorm(g, w); yield return null; }
            g.forcedCombat = null;
            double hpBefore = w.HpFrac;
            // 3. the player dies in phase 3: the worm goes back to the START of the phase, not to full HP
            int retries0 = sim.wyrm24Retries;
            if (campaign && sim.campaignOn && sim.campaign != null && sim.campaign.room == bossIndex)
            {
                int rr0 = roomRetries; // roomRetries counts the bot's deaths in rooms 1-5 too: wait for a NEW one
                g.health.hp = 1;
                g.health.max = 100;
                g.health.Damage(1e6, "boss");
                for (int k = 0; k < Ms(8000) && roomRetries == rr0; k++) yield return null;
                yield return Frames(3); // Campaign24 emits roomRetry after BeginBoss and the respawn
            }
            else
            {
                sim.Wyrm24Begin(); // what Campaign24.Retry does: BeginBoss again while the worm lives
                g.Respawn(sim.Wyrm24Spawn, 0);
            }
            double hpAfter = w.HpFrac;
            int retries = sim.wyrm24Retries - retries0;
            if (retries < 1) fails.Add("no phase-start retry counted");
            if (sim.n25.wyrmAim && Math.Abs(hpAfter - w.p.phase3At) > 1e-6) fails.Add($"HP after the retry {hpAfter:0.000} is not phase 3's threshold {w.p.phase3At:0.00}");
            if (!w.Alive || w.phase != "p3") fails.Add($"the worm after the retry: alive {w.Alive}, phase {w.phase}");
            // 4. the kill from the real gun, to bossDown
            guard = 0;
            for (int k = 0; k < Ms(150000) && w.Alive; k++) { God(g); if (w.phase != "dying") AimAtWorm(g, w); yield return null; }
            g.forcedCombat = null;
            bool down = g.events.Emitted.ContainsKey("bossDown");
            if (w.Alive) fails.Add($"the worm did not die in 150 s (phase {w.phase}, HP {w.HpFrac:0.00})");
            if (!down) fails.Add("no bossDown event");
            yield return Frames(Ms(800));
            bool piecesGone = !g.wyrmView.Shown;
            bool roomGone = !g.wyrmRoomView.Shown;
            if (!piecesGone) fails.Add("the worm's pieces are still drawn after it died");
            string adv = "not a campaign boot (run it on -skygrindMap layer1 for the layer's end)";
            if (campaign && sim.campaignOn)
            {
                Campaign24 cb = sim.campaign;
                for (int k = 0; k < Ms(10000) && !(cb != null && cb.finished) && g.flow != Flow.Results; k++) { God(g); yield return null; }
                bool done = (cb != null && cb.finished) || g.flow == Flow.Results;
                adv = $"campaign {(cb != null && cb.finished ? "LAYER CLEARED" : "not finished")}, flow {g.flow}, layerClear x{layerClears}, room retries {roomRetries}";
                if (!done) fails.Add("the campaign did not advance after bossDown");
            }
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {path}; {pc}; real gun: {realHits} bossHit events, HP {hp0:0.00} -> {hp1:0.00} before the cut; phase-3 retry: HP {hpBefore:0.00} -> {hpAfter:0.00} (threshold {w.p.phase3At:0.00}), retries counted {retries}; bossDown {down}, pieces drawn after {(!piecesGone ? "YES" : "no")}, room still drawn {(!roomGone ? "yes" : "no")}; {adv} | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }

        static partial void N25WyrmbarBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = WyrmBarScenario(g, dir, res, pre);
        }

        static IEnumerator WyrmBarScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            SurvivalSim sim = g.sim;
            string path = WyrmPath(g);
            if (!sim.n24.wyrm || g.wyrmView == null || !sim.Wyrm24Begin())
            {
                res[0] = path + " | RED: the worm cannot start (row off, no view, or no files)";
                yield break;
            }
            Wyrm w = sim.wyrm24;
            g.husks.frozen = true;
            g.Respawn(sim.Wyrm24Spawn, sim.wyrm24Arena.spawnYawDeg);
            g.input.SetLook(0, 0.28);
            God(g);
            yield return Frames(Ms(500));
            Start(g, "wyrmbar", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            var t = new HudReads20.Threat();
            int guard = 0;
            while (w.phase == "intro" && guard++ < Ms(9000)) { God(g); yield return null; }
            HudReads20.ThreatNow(g, ref t);
            bool valid = t.valid && t.boss;
            string name = t.name;
            double f0 = t.maxHp > 0 ? t.hp / t.maxHp : 0;
            int pips0 = t.pips, on0 = t.pipsOn;
            if (!valid) fails.Add("no boss threat read while the worm fights");
            if (name != "KILN WYRM") fails.Add($"name '{name}'");
            // a hit: the fraction falls
            w.Apply(w.body.maxHp * 0.05);
            yield return Frames(Ms(300));
            HudReads20.ThreatNow(g, ref t);
            double f1 = t.maxHp > 0 ? t.hp / t.maxHp : 0;
            if (!(f1 < f0)) fails.Add($"the fraction did not fall ({f0:0.00} -> {f1:0.00})");
            // phase 2: the beat, one pip out
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            guard = 0;
            while (w.phase != "p2" && guard++ < Ms(4000)) { God(g); yield return null; }
            yield return Frames(Ms(600));
            HudReads20.ThreatNow(g, ref t);
            int on2 = t.pipsOn;
            if (!(on2 < on0)) fails.Add($"the phase pips did not fall at phase 2 ({on0} -> {on2})");
            float nameH = UiTheme.Px(UiTheme.Size.S24);
            if (nameH < 18) fails.Add($"name text {nameH} px (< 18)");
            double mark = t.phaseAt;
            yield return Frames(Ms(400));
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {path}; {Preconditions(g, sim.mover.position)}; bar valid {valid}, name '{name}', HP {f0:0.00} -> {f1:0.00} after a 5 % hit, phase mark {mark:0.00}, pips {pips0}, lit {on0} -> {on2} at phase 2, name text {nameH:0} px (red-orange only on the fill and the lit pips: the BULWARK's bar) | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
