// NIGHT30 builder S: the Player scenarios of the systems rows (the names are FeelReel.N27.cs PhaseN30S; the body is the partial N30SystemsBody). Each writes
// PASS or RED lines into res, with the numbers, using the player's own inputs (g.forcedCombat / g.forcedMove go through the same reader the pad and the
// keyboard feed). Not run by its author (no Player in the worktree). Not yet written here: lava30, glide30 (their sims land later tonight).
//   rules30    SURVIVAL (any map). The THUNDERCLAP fired, a switch to the KESTREL asked on the next frame: the switch must run within the ACTION (design.n30.rules.
//              actionSeconds, 42 steps) of the shot, not the pump's whole cooldown; the THUNDERCLAP's slot reads COOLING while holstered and its seconds fall; the
//              gauntlet's heavy swing (2.5 s) then a switch: within the action too; Gauntlet30Now's PUNCH reads COOLING about 2.5 s. RED with -skygrindOff30
//              (NIGHT29's lockout: the switch waits the whole cooldown).
//   lesson30   a CAMPAIGN level (boot -skygrindMap n30-iceline, or any level): DOORFREE set, every room entered in order by a respawn (2.5 s each); 0.6 s after each
//              roomEnter the lesson widget (Ui.Lesson25Visible / Lesson25TextNow) must show no line begun in another room, and none in a boss room unless it is a boss
//              line; the boss-bar gate (sim.BossBarHidden30) hidden outside the boss room.
//   hudfx30    P0 a: a TMP text on the HUD canvas gets the letter reveal (HudFx20.Reveal), is DESTROYED mid-reveal (what a scene reload does), and the HUD ticks
//              for 2 s: 0 exceptions logged (Application.logMessageReceived). RED with -skygrindOff30 (one NullReferenceException per motion).
//   arcline30  a CAMPAIGN level with grapple nodes (boot -skygrindMap n30-span): the ARCLINE owned at 1 s, a grapple press at the nearest node attaches ON the node
//              (ropeAttach node) and the swing's cooldown after the let-go is arcline.cooldown.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using Btn = Skygrind.Core.Button;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N30SystemsBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "rules30") body = Rules30Scenario(g, dir, res, offs, pre);
            else if (s == "lesson30") body = Lesson30Scenario(g, dir, res, offs, pre);
            else if (s == "hudfx30") body = HudFx30Scenario(g, dir, res, offs, pre);
            else if (s == "arcline30") body = Arcline30Scenario(g, dir, res, offs, pre);
            else if (s == "lava30") body = Lava30Scenario(g, dir, res, offs, pre);
            else if (s == "glide30") body = Glide30Scenario(g, dir, res, offs, pre);
        }

        /// <summary>A CAMPAIGN run on the booted level, DOORFREE on (any room by a respawn); null when it did not start.</summary>
        static IEnumerator StartCampaign30(SurvivalGame g, List<Action> offs)
        {
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            for (int k = 0; k < 120 && g.sim.campaign == null; k++) yield return null;
            g.sim.campaign?.SetDoorFree29(true);
            God(g);
        }

        // ======================================================= lava30 =======================================================

        static IEnumerator Lava30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            yield return StartCampaign30(g, offs);
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet) + $" | level {sim.campaignLevel?.id}, n30.lava {sim.n30.lava}";
            if (sim.lava30 == null) { res[0] = "RED: design.n30.lava is off in this run | " + pc; yield break; }
            if (sim.lava30.zones.Count == 0) { res[0] = $"RED: the level '{g.MapId}' has no lava array (L's lava pits not in this map) | " + pc; yield break; }
            Lava30Zone z = sim.lava30.zones[0];
            double cx = (z.x0 + z.x1) / 2, cz = (z.z0 + z.z1) / 2;
            int burns = 0, enemyBurns = 0, enemyDeaths = 0;
            offs.Add(g.events.On<Lava30Event>("lavaBurn", e => burns++));
            offs.Add(g.events.On<Lava30Event>("lavaEnemyBurn", e => enemyBurns++));
            offs.Add(g.events.On<Lava30Event>("lavaEnemyDeath", e => enemyDeaths++));
            // into the lava's room first (its zone opens the campaign room), then the drop from 1.3 m above the surface
            RoomDef room = sim.campaignLevel.rooms.Find(r => r.id == z.room);
            if (room != null) { g.Respawn(room.spawn, room.spawnYawDeg); double t1 = sim.SimTime; while (sim.SimTime - t1 < 0.5) { God(g); yield return null; } }
            g.health.max = sim.cfg.Num("health.max");
            g.health.hp = g.health.max;
            double max = g.health.max, hp0 = g.health.hp;
            int respawns0 = sim.campaign != null ? sim.campaign.stageRespawns : 0;
            Start(g, "lava30", "camera", dir);
            yield return Frames(pre);
            g.Respawn(new Vec3(cx, z.y + 1.3, cz), room != null ? room.spawnYawDeg : 0);
            double hpFirst = -1, maxY = double.MinValue, t0 = sim.SimTime;
            bool stayed = true;
            while (sim.SimTime - t0 < 2.0)
            {
                if (burns > 0 && hpFirst < 0) hpFirst = g.health.hp;
                if (burns > 0) maxY = Math.Max(maxY, g.Feet.y);
                Vec3 p = g.Feet;
                if (p.x < z.x0 - 1.5 || p.x > z.x1 + 1.5 || p.z < z.z0 - 1.5 || p.z > z.z1 + 1.5) stayed = false;
                if (g.health.hp < max * 0.3) g.health.hp = max;
                yield return null;
            }
            int respawns = (sim.campaign != null ? sim.campaign.stageRespawns : 0) - respawns0;
            // an enemy dropped in
            God(g);
            if (room != null) g.Respawn(room.spawn, room.spawnYawDeg);
            Husk h = sim.husks.Spawn("husk", new Vec3(cx, z.y + 0.02, cz), g.Feet);
            double t2 = sim.SimTime;
            while (sim.SimTime - t2 < 2.0 && h != null && h.alive) { God(g); yield return null; }
            yield return Stop();
            double frac = hpFirst >= 0 ? 1 - hpFirst / hp0 : 0;
            double want = sim.cfg.Num("design.n30.lava.damageFraction");
            bool ok = burns > 0 && Math.Abs(frac - want) < 0.01 && maxY > z.y + 0.6 && respawns == 0 && stayed && enemyBurns > 0 && enemyDeaths > 0;
            res[0] = $"{(ok ? "PASS" : "RED")}: lava '{z.room}' surface y {z.y.ToString("0.00", ci)}: burns {burns} in 2 s, the first cost {(100 * frac).ToString("0.0", ci)} % (want {(100 * want).ToString("0", ci)} %), the knock lifted the feet to y {maxY.ToString("0.00", ci)}, stage respawns {respawns}, stayed over the lava {stayed}; a husk dropped in: burn {enemyBurns}, death {enemyDeaths} | {pc}";
        }

        // ======================================================= glide30 =======================================================

        static IEnumerator Glide30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            yield return StartCampaign30(g, offs);
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet) + $" | level {sim.campaignLevel?.id}, n30.glide {sim.n30.glide}, surfaces {(sim.surfaces26 != null ? sim.surfaces26.zones.Count : 0)}";
            if (sim.glide30 == null) { res[0] = "RED: design.n30.glide is off in this run | " + pc; yield break; }
            SurfaceZone ice = null;
            double bestLen = 0;
            if (sim.surfaces26 != null)
                foreach (SurfaceZone zz in sim.surfaces26.zones)
                    if (zz.kind == SurfaceKind.Ice && Math.Max(zz.x1 - zz.x0, zz.z1 - zz.z0) > bestLen) { bestLen = Math.Max(zz.x1 - zz.x0, zz.z1 - zz.z0); ice = zz; }
            if (ice == null) { res[0] = $"RED: no ICE surface in '{g.MapId}' (boot -skygrindMap n30-iceline) | " + pc; yield break; }
            bool alongZ = ice.z1 - ice.z0 >= ice.x1 - ice.x0;
            double cx = (ice.x0 + ice.x1) / 2, cz = (ice.z0 + ice.z1) / 2;
            // NIGHT30 round 3 fix (the n30-r2 RED was the reel's): its centre line ran the dash into i6's 1.2 m pillar at z -164.4 (speed 0 after it, with glide on or
            // off; harness glide30 --level found it). The clearest line across the sheet now: the offset whose rays at knee and chest height run furthest.
            double bestRun = -1;
            for (double o = 0.8; o <= (alongZ ? ice.x1 - ice.x0 : ice.z1 - ice.z0) - 0.8 + 1e-9; o += 0.25)
            {
                double run = 60;
                Vec3 from = alongZ ? new Vec3(ice.x0 + o, Math.Max(0, ice.minY + 0.5), ice.z1 - 1) : new Vec3(ice.x1 - 1, Math.Max(0, ice.minY + 0.5), ice.z0 + o);
                Vec3 dirv = alongZ ? new Vec3(0, 0, -1) : new Vec3(-1, 0, 0);
                foreach (double hgt in new[] { 0.4, 1.4 })
                    if (sim.world.RayIntersect(new Vec3(from.x, from.y + hgt, from.z), dirv, out RayHit rh)) run = Math.Min(run, rh.distance);
                if (run > bestRun + 1e-9) { bestRun = run; if (alongZ) cx = from.x; else cz = from.z; }
            }
            // start at the +z (or +x) end, skate toward -z (or -x)
            Vec3 start = alongZ ? new Vec3(cx, Math.Max(0, ice.minY + 0.5), ice.z1 - 1) : new Vec3(ice.x1 - 1, Math.Max(0, ice.minY + 0.5), cz);
            double yaw = alongZ ? 0 : Math.PI / 2; // yaw 0 looks toward -z, PI/2 toward -x
            g.Respawn(start, yaw * 180 / Math.PI);
            Start(g, "glide30", "camera", dir);
            yield return Frames(pre);
            int dashes = sim.glide30.dashAdds;
            // run up 0.8 s, then one dash, then glide with no input
            double t0 = sim.SimTime, before = 0, after = -1, top = 0;
            bool dashed = false;
            while (sim.SimTime - t0 < 2.6)
            {
                God(g);
                sim.husks.KillAll(); // the room's wave (DOORFREE spawned it) would stand on the line
                double t = sim.SimTime - t0;
                Vec3 v = sim.mover.velocity;
                double sp = Math.Sqrt(v.x * v.x + v.z * v.z);
                g.input.SetLook(yaw, 0);
                if (t < 0.8) g.forcedMove = new InputFrame { yaw = yaw, moveZ = 1 };
                else if (!dashed) { before = sp; dashed = true; g.forcedMove = new InputFrame { yaw = yaw, moveZ = 1, dash = new Btn { held = true, pressed = true } }; }
                else g.forcedMove = new InputFrame { yaw = yaw };
                if (dashed && sim.mover.state != MoveState.Dash && t > 1.4 && after < 0) after = sp;
                top = Math.Max(top, sp);
                yield return null;
            }
            g.forcedMove = new InputFrame();
            yield return Stop();
            double gain = after > 0 && before > 0 ? after / before - 1 : 0;
            bool ok = gain >= 0.2 && sim.glide30.dashAdds > dashes;
            res[0] = $"{(ok ? "PASS" : "RED")}: on the {bestLen.ToString("0", ci)} m ice sheet: speed before the dash {before.ToString("0.0", ci)} m/s, 0.35 s after it {after.ToString("0.0", ci)} m/s ({(100 * gain).ToString("+0;-0", ci)} %), top {top.ToString("0.0", ci)}, glide dash adds {sim.glide30.dashAdds - dashes} | {pc}";
        }

        /// <summary>Hold `c` as the player's combat input until `until` reads true or `maxFrames` pass.</summary>
        static IEnumerator Hold30(SurvivalGame g, CombatInput c, Func<bool> until, int maxFrames)
        {
            for (int i = 0; i < maxFrames && !until(); i++)
            {
                g.forcedCombat = c;
                yield return null;
            }
            g.forcedCombat = IdleCombat;
        }

        // ======================================================= rules30 =======================================================

        static IEnumerator Rules30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            offs.Add(() => Quiet(g));
            Quiet(g);
            God(g);
            Loadout lo = sim.loadout;
            foreach (string w in new[] { "revolver", "shotgun", "gauntlet" }) if (!lo.owned.Contains(w)) lo.AddWeapon(w);
            string pc = Preconditions(g, g.Feet) + $" | n30.rules {sim.n30.rules}, n29.lockout {sim.n29.lockout}, pulse27 {lo.Pulse27Live}";
            int A = MovementMath.StepsFor(g.cfg.Num("design.n30.rules.actionSeconds"), sim.step);
            int iRev = lo.owned.IndexOf("revolver"), iSg = lo.owned.IndexOf("shotgun"), iG = lo.owned.IndexOf("gauntlet");
            int fireStep = -1, swingStep = -1, swapStep = -1;
            string swapTo = null;
            offs.Add(g.events.On("fire", o => { if (o as string == "shotgun" && fireStep < 0) fireStep = sim.simStep; }));
            offs.Add(g.events.On<MeleeEvent>("meleeSwing", e => { if (swingStep < 0) swingStep = sim.simStep; }));
            offs.Add(g.events.On("weaponSwap", o => { swapStep = sim.simStep; swapTo = o as string; }));
            Start(g, "rules30", "ui", dir);
            yield return Frames(pre);
            // 1. the THUNDERCLAP in hand, fired once, then a switch to the KESTREL on the next frame
            yield return Hold30(g, new CombatInput { weapon = iSg }, () => lo.Weapon == "shotgun", 2);
            yield return Frames(90);
            fireStep = -1;
            yield return Hold30(g, new CombatInput { weapon = -1, fire = new Btn { held = true, pressed = true } }, () => fireStep >= 0, 120);
            g.forcedCombat = IdleCombat;
            yield return null;
            int askStep = sim.simStep, sgCool = lo.CooldownStepsOf("shotgun");
            swapStep = -1;
            yield return Hold30(g, new CombatInput { weapon = iRev }, () => swapStep >= 0 || lo.Switching30 >= 0, 30); // the ask, held until the sim took it (queued or swapping)
            for (int i = 0; i < 600 && swapStep < 0; i++) yield return null;
            int wait1 = swapStep >= 0 ? swapStep - fireStep : -1;
            // the holstered THUNDERCLAP: COOLING, its seconds falling
            yield return Frames(10);
            SlotRead30 a = lo.Slot30(iSg);
            yield return Frames(30);
            SlotRead30 b = lo.Slot30(iSg);
            bool ticks = a.state == WeaponState30.Cooling && (b.seconds < a.seconds || b.state == WeaponState30.Ready);
            // 2. the gauntlet's heavy swing, then a switch
            swapStep = -1;
            yield return Hold30(g, new CombatInput { weapon = iG }, () => swapStep >= 0, 30);
            yield return Frames(90);
            swingStep = -1;
            yield return Hold30(g, new CombatInput { weapon = -1, fire = new Btn { held = true, pressed = true } }, () => swingStep >= 0, 400);
            g.forcedCombat = IdleCombat;
            yield return null;
            Gauntlet30 gs = lo.Gauntlet30Now();
            swapStep = -1;
            yield return Hold30(g, new CombatInput { weapon = iRev }, () => swapStep >= 0 || lo.Switching30 >= 0, 30); // the ask, held until the sim took it (queued or swapping)
            for (int i = 0; i < 900 && swapStep < 0; i++) yield return null;
            int wait2 = swapStep >= 0 && swingStep >= 0 ? swapStep - swingStep : -1;
            yield return Stop();
            double hz = 1 / sim.step;
            bool ok1 = wait1 >= 0 && wait1 <= A + 3, ok2 = wait2 >= 0 && wait2 <= A + 3;
            bool all = ok1 && ticks && ok2;
            res[0] = $"{(all ? "PASS" : "RED")}: [{(ok1 ? "ok" : "RED")}] THUNDERCLAP shot -> switch ran {wait1} steps ({(wait1 / hz).ToString("0.00", ci)} s) after the shot; the action cap {A} steps, the pump's cooldown {sgCool} steps ({(sgCool / hz).ToString("0.00", ci)} s)"
                + $" || [{(ticks ? "ok" : "RED")}] the holstered THUNDERCLAP read {a.state} {a.seconds.ToString("0.00", ci)} s, 30 frames later {b.state} {b.seconds.ToString("0.00", ci)} s"
                + $" || [{(ok2 ? "ok" : "RED")}] heavy swing -> switch ran {wait2} steps ({(wait2 / hz).ToString("0.00", ci)} s) after the swing; PUNCH read {gs.punch} {gs.punchSeconds.ToString("0.00", ci)} s, BLAST {gs.blast} pips {gs.pips}/{gs.pipsMax} | {pc}";
        }

        // ======================================================= lesson30 =======================================================

        static IEnumerator Lesson30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            // NIGHT30 round 3 fix (the reel's RED in n30-r2 was the reel's own): the listeners go on BEFORE the run starts. The first room's line begins at the
            // campaign's first live step, inside the 30 frames below; listening after them, the reel never saw it begin (lineRoom stayed -1, printed "room 0")
            // and called the first room's own line stale.
            int lineRoom = -1;
            string lineId = null;
            offs.Add(g.events.On<LessonEvent>("lessonBegin", e => { lineRoom = e.room; lineId = e.id; }));
            offs.Add(g.events.On<LessonEvent>("lessonClear", e => { lineRoom = -1; lineId = null; }));
            g.StartRun(Seed);
            yield return Frames(30);
            God(g);
            if (!sim.campaignOn || sim.campaignLevel == null) { res[0] = "RED: the run did not start as a CAMPAIGN"; yield break; }
            string pc = Preconditions(g, g.Feet) + $" | level {sim.campaignLevel.id}, n30.teach {sim.n30.teach}, N30Fixes {N30Fixes.On}";
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            Start(g, "lesson30", "ui", dir);
            yield return Frames(pre);
            LevelDef lv = sim.campaignLevel;
            int stale = 0, bossLine = 0, barWrong = 0, checkedRooms = 0, shown = 0;
            var notes = new List<string>();
            for (int k = 0; k < lv.rooms.Count; k++)
            {
                RoomDef r = lv.rooms[k];
                g.Respawn(r.spawn, r.spawnYawDeg);
                double t0 = sim.SimTime;
                bool sampled = false;
                while (sim.SimTime - t0 < 2.5)
                {
                    God(g);
                    if (!sampled && sim.SimTime - t0 >= 0.6 && sim.campaign != null && sim.campaign.room == k)
                    {
                        sampled = true;
                        checkedRooms++;
                        bool up = Ui.Lesson25Visible && !string.IsNullOrEmpty(Ui.Lesson25TextNow);
                        if (up) shown++;
                        if (up && lineRoom != k && lineId != "boss") { stale++; if (notes.Count < 4) notes.Add($"{r.id}: '{Ui.Lesson25TextNow}' ({(lineRoom < 0 ? "no lessonBegin seen" : "begun in room " + (lineRoom + 1) + " " + lv.rooms[Math.Min(lineRoom, lv.rooms.Count - 1)].id)})"); }
                        if (up && r.kind == "boss" && lineId != "boss") bossLine++;
                        if (sim.BossBarHidden30 == (r.kind == "boss")) barWrong++;
                    }
                    yield return null;
                }
            }
            yield return Stop();
            bool ok = stale == 0 && bossLine == 0 && barWrong == 0 && checkedRooms > 0;
            res[0] = $"{(ok ? "PASS" : "RED")}: {checkedRooms} rooms checked 0.6 s after entry; a line up in {shown}; STALE lines (begun in another room) {stale}; room lines in a boss room {bossLine}; boss-bar gate wrong {barWrong} | {pc}";
            if (notes.Count > 0) res[0] += " || stale: " + string.Join("; ", notes);
        }

        // ======================================================= hudfx30 =======================================================

        static IEnumerator HudFx30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            if (!HudFx20.On || HudFx20.Reveal == null) { res[0] = "RED: HudFx20 is not installed or switched off (UnityTuning.Hud20FxOn)"; yield break; }
            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { res[0] = "RED: no HUD canvas"; yield break; }
            int nre = 0, other = 0;
            string first = null;
            Application.LogCallback cb = (msg, stack, type) =>
            {
                if (type != LogType.Exception && type != LogType.Error) return;
                if (msg.Contains("NullReferenceException") || msg.Contains("MissingReferenceException")) { nre++; first ??= msg + " @ " + (stack.Length > 160 ? stack.Substring(0, 160) : stack); }
                else other++;
            };
            Application.logMessageReceived += cb;
            offs.Add(() => Application.logMessageReceived -= cb);
            Start(g, "hudfx30", "ui", dir);
            yield return Frames(pre);
            int rounds = 3;
            for (int k = 0; k < rounds; k++)
            {
                var go = new GameObject("HudFx30Probe");
                go.transform.SetParent(canvas.transform, false);
                TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
                t.fontSize = 24;
                t.text = "HUDFX30 REVEAL PROBE " + k;
                HudFx20.Reveal(t); // the announcer's reveal on this text
                yield return Frames(4); // the reveal is mid-way (its letters stagger in)
                UnityEngine.Object.Destroy(go); // what a scene reload does to the HUD's texts
                float until = Time.realtimeSinceStartup + 2f;
                while (Time.realtimeSinceStartup < until) yield return null; // the HUD keeps ticking HudFx20 (Ui.Render)
            }
            yield return Stop();
            res[0] = $"{(nre == 0 ? "PASS" : "RED")}: {rounds} reveals destroyed mid-way, 2 s of HUD ticks each: NullReference / MissingReference exceptions logged {nre}, other errors {other}; N30Fixes {N30Fixes.On}{(first != null ? " | first: " + first : "")}";
        }

        // ======================================================= arcline30 =======================================================

        static IEnumerator Arcline30Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            SurvivalSim sim = g.sim;
            if (sim.map.grappleNodes.Count == 0) { res[0] = $"RED: the booted map '{g.MapId}' has no grapple nodes (boot -skygrindMap n30-span)"; yield break; }
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            double t0 = sim.SimTime;
            while (sim.SimTime - t0 < 1.0) { God(g); yield return null; }
            bool owned = sim.arcline21 != null && sim.arcline21.Owned;
            string pc = Preconditions(g, g.Feet) + $" | n30.span {sim.n30.span}, n21.arcline {sim.n21.arcline}, nodes {sim.map.grappleNodes.Count}";
            if (!owned) { res[0] = "RED: the ARCLINE is not owned 1 s into the level | " + pc; yield break; }
            int attachNode = 0, releaseStep = -1, cdAfter = -1;
            offs.Add(g.events.On<Arcline21RopeEvent>("ropeAttach", e => { if (e.node) attachNode++; }));
            // the cooldown is read in the release itself (the Mover sets it before it emits ropeRelease): r5 read it a frame later (47 of 48)
            offs.Add(g.events.On<Arcline21RopeEvent>("ropeRelease", e => { if (releaseStep < 0) { releaseStep = sim.simStep; cdAfter = sim.mover.rope.cooldownSteps; } }));
            // round 8 (r5's RED: from s1's spawn every node is ~30 m off and the hook bit a surface): the first room in the level's order that holds a node; the
            // player stands 4.5 m before that node (toward +z: the plans run toward -z) on its room's floor, DOORFREE so its zone is open, as span30 / the pilot do
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            sim.campaign?.SetDoorFree29(true);
            Vec3 node = sim.map.grappleNodes[0];
            RoomDef nodeRoom = null;
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                foreach (Vec3 n in sim.map.grappleNodes)
                    if (n.x >= r.x0 - 1 && n.x <= r.x1 + 1 && n.z >= r.z0 - 1 && n.z <= r.z1 + 1) { node = n; nodeRoom = r; break; }
                if (nodeRoom != null) break;
            }
            var stand = new Vec3(node.x, nodeRoom != null ? nodeRoom.spawn.y : g.Feet.y, node.z + 4.5);
            g.Respawn(stand, 0);
            double t1 = sim.SimTime;
            while (sim.SimTime - t1 < 0.4) { God(g); yield return null; }
            pc += $" | node ({node.x.ToString("0.0", ci)}, {node.y.ToString("0.0", ci)}, {node.z.ToString("0.0", ci)}) in {(nodeRoom != null ? nodeRoom.id : "?")}, stood at z {stand.z.ToString("0.0", ci)}";
            Start(g, "arcline30", "camera", dir);
            yield return Frames(pre);
            bool fired = false;
            for (int i = 0; i < 240 && attachNode == 0; i++)
            {
                God(g);
                Vec3 eye = sim.Eye, d = node - eye;
                double yaw = Math.Atan2(-d.x, -d.z), pitch = Math.Asin(d.y / Math.Max(1e-6, d.Length));
                g.input.SetLook(yaw, pitch);
                if (sim.mover.rope.phase != RopePhase.None) fired = true;
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch, grapple = new Btn { held = true, pressed = !fired } }; // pressed until the hook is out (a frame may hold no sim step)
                yield return null;
            }
            double tHang = sim.SimTime;
            while (sim.SimTime - tHang < 1.0) // the swing: held 1 s
            {
                God(g);
                g.forcedMove = new InputFrame { yaw = 0, grapple = new Btn { held = true, pressed = false } };
                yield return null;
            }
            g.forcedMove = new InputFrame();
            for (int i = 0; i < 60 && releaseStep < 0; i++) yield return null;
            yield return Stop();
            int want = MovementMath.StepsFor(g.cfg.movement.arcline.cooldown, sim.step);
            bool ok = attachNode > 0 && releaseStep >= 0 && cdAfter == want;
            res[0] = $"{(ok ? "PASS" : "RED")}: owned at 1 s {owned}; attached on a node {attachNode}; released {(releaseStep >= 0)}; swing cooldown at the let-go {cdAfter} steps (want {want} = {g.cfg.movement.arcline.cooldown.ToString("0.00", ci)} s) | {pc}";
        }
    }
}
