// NIGHT47 builder B: the Player scenarios a247 (level 2: the CHARGER, THE STAMPEDE) and a347 (level 3: THE KILN WYRM's tells and burrow),
// dispatched by FeelReel.N47.cs (the lead's routing). Not run by its author (no Player in a builder worktree). Rows: the reel forces the sim
// rows it shows (sim.rowsOverride before the run starts; put back after); view rows (chargerLook47 ...) are the build copy's (the lead's
// n46-build-rows.cjs). The player is God; input by forced frames only (L-046). Frames: dir/shot-N29-<name>.png.
//   a247: boot -skygrindMap n47-iceline -skygrindReelOnly a247 (rows charger47 + brawl47)
//     B47-charger-windup-start   g1, from its spawn: one CHARGER placed 14 m ahead, the frame its wind-up starts (the line follows)
//     B47-charger-windup-last    the wind-up's last 0.2 s (the line locked: bright, steady, white-edged with chargerLook47 on)
//     B47-charger-charge         mid-charge after a late sidestep (moveX 1 for 0.4 s, started in the lock)
//     B47-charger-stun           the charger stunned on the entry wall, the camera turned to its back (the yellow plate)
//     B47-stampede-lip           g8 THE STAMPEDE from its spawn: the columns, the pit, the gallery, the turntable gate
//     B47-stampede-mid           8 s later (the first waves in)
//     B47-brawl-armour / B47-brawl-<attack>-tell (each attack forced once, 70 % into its wind-up) / -phase2 / -phase3-herd   x9 THE BRAWL
//     B47-charger-stun-side-6m / -10m / -20m (the body at distance); B47-stampede47-herd-staged (round 4: a staged herd in g8)
//   a347: boot -skygrindMap n47-kiln (or n46-kiln: no beats) -skygrindReelOnly a347 (rows walkin46 + wyrm46 + wyrm47)
//     B47-beat-glimpse / -pass / -cameo-tell-start / -cameo-tell-last / -cameo-breach   G35's beats (n47-kiln only)
//     B47-wyrm-crack-start       the frame a "crack" tell starts (the floor crack where it dives, 1.2 s with wyrm47)
//     B47-wyrm-crack-last        ~0.2 s before it dives
//     B47-wyrm-aggro-start       the frame a charge's tell starts (0.5 s: wyrm47 keeps it; see B-cards)
//     B47-wyrm-mid               ~6 s into the fight
//     B47-wyrm-emerge / -breach / -trail   wyrmLook47 (the emerge, the dive's exit, the burrow trail on the floor: trail47)
// Line: GREEN when the frames were taken and the charger went tell -> charge -> stunned (a247) / a crack tell was seen (a347).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N47Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Brawl47Reel(g, dir, res, offs, pre);

        static partial void N47Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Wyrm47Reel(g, dir, res, offs, pre);

        static IEnumerator Brawl47Reel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            NightRowState before = sim.rowsOverride;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.sim.rowsOverride = before; });
            try { sim.rowsOverride = sim.rowsRegistry.Defaults().Set("charger47", true).Set("brawl47", true); }
            catch (Exception e) { res[0] = "RED a247: the build lacks the rows charger47 / brawl47 (" + e.Message + ")"; yield break; }
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a247: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            if (sim.campaignLevel == null || sim.campaignLevel.id != "n47-iceline")
            {
                res[0] = $"RED a247: the run is on {sim.campaignLevel?.id ?? "?"}, not n47-iceline (boot -skygrindMap n47-iceline through reel.sh, L-062)";
                yield break;
            }
            g.forcedCombat = IdleCombat;
            God(g);
            var st = new double[4];
            var shots = new List<string>();
            Charger mine = null;
            void Look(double y, double p, double moveX = 0)
            {
                g.input.SetLook(y, p);
                g.forcedMove = new InputFrame { yaw = y, pitch = p, moveX = moveX };
            }
            void OnlyMine() { foreach (Enemy e in sim.enemies.all) if (e.alive && e != mine) e.alive = false; }
            IEnumerator Hold(float seconds, bool only) { for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { God(g); if (only) OnlyMine(); yield return null; } }
            IEnumerator Shot(string name) { Wyrm46ClearCards(g); yield return null; Wyrm46ClearCards(g); yield return Shot29(dir, name, st); shots.Add(name); }
            Start(g, "a247", "camera", dir);
            yield return Frames(pre);

            // 1. g1: one charger, its wind-up (following, then locked), a late sidestep, the stun on the entry wall
            RoomDef g1 = sim.campaignLevel.rooms.Find(r => r.id == "g1");
            if (g1 == null) { res[0] = "RED a247: no room g1"; yield break; }
            g.Respawn(g1.spawn, 0);
            Look(0, 0.02);
            yield return Hold(0.5f, true);
            Husk h = sim.husks.Spawn("charger", new Vec3(g1.spawn.x, g1.spawn.y, g1.spawn.z - 14), sim.mover.position);
            mine = h as Charger;
            if (mine == null) { res[0] = "RED a247: no charger placed in g1"; yield break; }
            bool told = false, locked = false, charged = false, stunned = false;
            for (float t = 0; t < 8f && !stunned; t += Time.unscaledDeltaTime)
            {
                God(g);
                OnlyMine();
                if (!told && mine.state == "tell") { told = true; yield return Shot("B47-charger-windup-start"); }
                if (told && !locked && mine.state == "tell" && mine.stateSteps <= MovementMath.StepsFor(0.2, g.cfg.Dt))
                {
                    locked = true;
                    yield return Shot("B47-charger-windup-last");
                    Look(0, 0.02, 1); // the late sidestep (0.4 s)
                }
                if (locked && !charged && mine.state == "charge")
                {
                    yield return Hold(0.15f, true);
                    Look(0, 0.02);
                    charged = true;
                    yield return Shot("B47-charger-charge");
                }
                if (charged && mine.state == "stunned")
                {
                    stunned = true;
                    Vec3 d = mine.position - sim.mover.position;
                    double back = Math.Atan2(-d.x, -d.z);
                    Look(back, -0.05);
                    yield return Hold(0.2f, true);
                    yield return Shot("B47-charger-stun");
                    // r1e note: one frame at 6 m, side-on, stunned with its vents open (the body's read)
                    double fx = -Math.Sin(mine.yaw), fz = -Math.Cos(mine.yaw); // its facing (web space)
                    g.Respawn(new Vec3(mine.position.x + fz * 6, mine.position.y, mine.position.z - fx * 6), 0);
                    yield return null;
                    Vec3 sd = mine.position + new Vec3(0, 1.0, 0) - (sim.mover.position + new Vec3(0, 1.6, 0));
                    Look(Math.Atan2(-sd.x, -sd.z), Math.Atan2(sd.y, Math.Sqrt(sd.x * sd.x + sd.z * sd.z)));
                    if (mine.state == "stunned") yield return Shot("B47-charger-stun-side-6m");
                    // the lead's round-4 list: the body reading at 10 and 20 m (front 3/4 view)
                    foreach (double dist in new[] { 10.0, 20.0 })
                    {
                        double fx2 = -Math.Sin(mine.yaw), fz2 = -Math.Cos(mine.yaw);
                        double ox = (fx2 * 0.8 + fz2 * 0.6), oz = (fz2 * 0.8 - fx2 * 0.6);
                        g.Respawn(new Vec3(mine.position.x + ox * dist, mine.position.y, mine.position.z + oz * dist), 0);
                        yield return null;
                        Vec3 dd = mine.position + new Vec3(0, 1.0, 0) - (sim.mover.position + new Vec3(0, 1.6, 0));
                        Look(Math.Atan2(-dd.x, -dd.z), Math.Atan2(dd.y, Math.Sqrt(dd.x * dd.x + dd.z * dd.z)));
                        yield return Hold(0.1f, true);
                        yield return Shot("B47-charger-" + (int)dist + "m");
                    }
                }
                yield return null;
            }
            mine.alive = false;
            mine = null;

            // 2. g8 THE STAMPEDE: the lip view, then the fight 8 s on
            RoomDef g8 = sim.campaignLevel.rooms.Find(r => r.id == "g8");
            if (g8 != null)
            {
                g.Respawn(g8.spawn, 0);
                Look(0, 0.04);
                yield return Hold(1.0f, false);
                yield return Shot("B47-stampede-lip");
                yield return Hold(8f, false);
                yield return Shot("B47-stampede-mid");
            }
            // 3. THE ICEBREAKER (x9 THE BRAWL; brawl47 forced, brawlLook47 the build copy's): the armour, each attack's wind-up (forced once
            // each), a window, phase 2 and phase 3 with its chargers (the bar written down to each mark: a reel-only cheat, printed)
            int brawlShots = 0;
            RoomDef x9 = sim.campaignLevel.rooms.Find(r => r.id == "x9");
            if (x9 != null)
            {
                g.Respawn(x9.spawn, 0);
                for (int f = 0; f < 600 && (sim.brawl40 == null || !sim.brawl40.Alive); f++)
                {
                    God(g);
                    g.forcedMove = new InputFrame { yaw = 0, pitch = 0, moveZ = 1 };
                    yield return null;
                }
                Brawl40 b = sim.brawl40;
                if (b != null && b.Alive)
                {
                    void Face()
                    {
                        // r1e note: 8-10 m from the body, and never outside the arena (from outside the camera looks through its pale
                        // boundary wall: the "translucent box with stripes"): the stand point is pulled toward the arena's centre
                        Vec3 d = b.Centre - (sim.mover.position + new Vec3(0, 1.6, 0));
                        double hz = Math.Sqrt(d.x * d.x + d.z * d.z);
                        Vec3 ac = sim.brawl40Centre ?? b.Centre;
                        if ((hz < 7.5 || hz > 11) && hz > 1e-3)
                        {
                            Vec3 at = new Vec3(b.Centre.x - d.x / hz * 9, 0, b.Centre.z - d.z / hz * 9);
                            double ox = at.x - ac.x, oz = at.z - ac.z, ol = Math.Sqrt(ox * ox + oz * oz);
                            if (ol > 14) { at = new Vec3(ac.x + ox / ol * 14, 0, ac.z + oz / ol * 14); }
                            g.Respawn(new Vec3(at.x, ac.y, at.z), 0);
                        }
                        d = b.Centre - (sim.mover.position + new Vec3(0, 1.6, 0));
                        double y = Math.Atan2(-d.x, -d.z), p = Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z));
                        g.input.SetLook(y, p);
                        g.forcedMove = new InputFrame { yaw = y, pitch = p };
                    }
                    for (float t = 0; t < 2f; t += Time.unscaledDeltaTime) { God(g); Face(); yield return null; }
                    yield return Shot("B47-brawl-armour"); brawlShots++;
                    var seenAtk = new HashSet<string>();
                    var queue = new List<string> { Brawl40.Haymaker, Brawl40.Slam, Brawl40.Block, Brawl40.Rush, Brawl40.Sweep, Brawl40.Leap };
                    for (float t = 0; t < 40f && b.Alive && seenAtk.Count < queue.Count; t += Time.unscaledDeltaTime)
                    {
                        God(g);
                        Face();
                        if (b.state == Brawl40State.Idle && !b.ForcePending) foreach (string q in queue) if (!seenAtk.Contains(q)) { b.Force(q); break; }
                        if (b.state == Brawl40State.Tell && b.attack != null && !seenAtk.Contains(b.attack) && b.Progress > 0.7)
                        {
                            seenAtk.Add(b.attack);
                            yield return Shot("B47-brawl-" + b.attack + "-tell"); brawlShots++;
                        }
                        yield return null;
                    }
                    double[] marks = { 0.64, 0.31 };
                    string[] names = { "B47-brawl-phase2", "B47-brawl-phase3-herd" };
                    for (int m = 0; m < 2 && b.Alive; m++)
                    {
                        b.body.hp = b.body.maxHp * (marks[m] + 0.02);
                        for (int k = 0; k < 40 && b.Alive && b.HpFrac > marks[m]; k++)
                        {
                            sim.loadout.Damage(b.body, b.body.maxHp * 0.03, false, "gauntlet", b.Centre); // a real hit through the fight's filter, so its phase rule runs
                            yield return null;
                        }
                        for (float t = 0; t < 2.5f; t += Time.unscaledDeltaTime) { God(g); Face(); yield return null; }
                        yield return Shot(names[m]); brawlShots++;
                    }
                }
            }
            // 4. round 4 stampede47 (a SURVIVAL event: no SURVIVAL boot in this reel): a STAGED herd of four chargers placed together at g8's
            // far end, one frame as their wind-ups light (what the event looks like when it comes in; the event itself is n47b stampede's)
            RoomDef g8h = sim.campaignLevel.rooms.Find(r => r.id == "g8");
            if (g8h != null)
            {
                foreach (Enemy e in sim.enemies.all) if (e.alive) e.alive = false;
                g.Respawn(g8h.spawn, 0);
                yield return null;
                for (int k = 0; k < 4; k++) sim.husks.Spawn("charger", new Vec3(-4.5 + 3 * k, g8h.spawn.y, g8h.spawn.z - 22), sim.mover.position);
                for (float t = 0; t < 2.5f; t += Time.unscaledDeltaTime)
                {
                    God(g);
                    Look(0, 0.02);
                    bool anyTell = false;
                    foreach (Enemy e in sim.enemies.all) if (e is Charger hc && hc.alive && hc.state == "tell") anyTell = true;
                    if (anyTell) break;
                    yield return null;
                }
                yield return Shot("B47-stampede47-herd-staged");
            }
            bool green = told && charged && stunned && shots.Count >= 4;
            res[0] = (green ? "GREEN" : "RED") + $" a247: n47-iceline; charger told {told}, locked frame {locked}, charged {charged}, stunned {stunned}; ICEBREAKER frames {brawlShots}; frames {shots.Count} ({string.Join(", ", shots)})";
        }

        static IEnumerator Wyrm47Reel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            NightRowState before = sim.rowsOverride;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.sim.rowsOverride = before; });
            try { sim.rowsOverride = sim.rowsRegistry.Defaults().Set("walkin46", true).Set("wyrm46", true).Set("wyrm47", true); }
            catch (Exception e) { res[0] = "RED a347: the build lacks the rows walkin46 / wyrm46 / wyrm47 (" + e.Message + ")"; yield break; }
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a347: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            RoomDef kb4 = sim.campaignLevel?.rooms.Find(r => r.id == "kb4");
            if (kb4 == null) { res[0] = $"RED a347: the run is on {sim.campaignLevel?.id ?? "?"} with no kb4 (boot -skygrindMap n46-kiln, L-062)"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            var st = new double[4];
            var shots = new List<string>();
            IEnumerator Hold(float seconds) { for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { God(g); yield return null; } }
            IEnumerator Shot(string name) { Wyrm46ClearCards(g); yield return null; Wyrm46ClearCards(g); yield return Shot29(dir, name, st); shots.Add(name); }
            Start(g, "a347", "camera", dir);
            yield return Frames(pre);
            // 0. G35's beats (only when booted on n47-kiln: -skygrindMap n47-kiln): the glimpse from kb1's top, the pass in kb2, the cameo in kb3
            int beatShots = 0;
            if (sim.campaignLevel.id == "n47-kiln")
            {
                void LookAt(Vec3 at)
                {
                    Vec3 d = at - (sim.mover.position + new Vec3(0, 1.6, 0));
                    double y = Math.Atan2(-d.x, -d.z), p = Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z));
                    g.input.SetLook(y, p);
                    g.forcedMove = new InputFrame { yaw = y, pitch = p };
                }
                RoomDef kb1 = sim.campaignLevel.rooms.Find(r => r.id == "kb1"), kb2 = sim.campaignLevel.rooms.Find(r => r.id == "kb2"), kb3 = sim.campaignLevel.rooms.Find(r => r.id == "kb3");
                if (kb1 != null)
                {
                    g.Respawn(new Vec3(kb1.spawn.x, kb1.spawn.y + 9.2, kb1.spawn.z - 3), 0);
                    for (int f = 0; f < 120 && sim.Glimpse47Steps < 0; f++) { God(g); yield return null; }
                    // r2 note: the frame waited 0.6 s and caught no worm: wait until the arc's head is near its top, look at the apex
                    double gl = g.cfg.Num("design.n47.wyrm47.numbers.glimpseS");
                    for (int f = 0; f < 600 && sim.Glimpse47Steps >= 0 && sim.Glimpse47Steps < (int)(0.38 * gl * 120); f++) { God(g); LookAt(sim.Glimpse47At + new Vec3(0, 8, 6)); yield return null; }
                    LookAt(sim.Glimpse47At + new Vec3(0, 8, 6));
                    yield return Shot("B47-beat-glimpse"); beatShots++;
                }
                if (kb2 != null)
                {
                    g.Respawn(kb2.spawn, 0);
                    for (int f = 0; f < 1200 && sim.Pass47Steps < 0; f++) { God(g); g.forcedMove = new InputFrame { yaw = 0, pitch = -0.35 }; g.input.SetLook(0, -0.35); yield return null; }
                    for (float t = 0; t < 1.0f; t += Time.unscaledDeltaTime) { God(g); LookAt((sim.Pass47From + sim.Pass47To) * 0.5); yield return null; }
                    yield return Shot("B47-beat-pass"); beatShots++;
                }
                if (kb3 != null)
                {
                    g.Respawn(kb3.spawn, 0);
                    for (int f = 0; f < 1200 && !sim.Cameo47Telling; f++) { God(g); yield return null; }
                    // r1e note: never from inside the glow: wait for the circle to lock (it follows the player for 60 % of its tell), then stand
                    // 8 m to its side and look at it low, from the side (the crack on the floor, the eruption beside you)
                    double track = g.cfg.Num("design.n47.wyrm47.numbers.cameoTrack");
                    for (int f = 0; f < 600 && sim.Cameo47Telling && sim.Cameo47Progress < track + 0.02; f++) { God(g); yield return null; }
                    Vec3 c0 = sim.Cameo47At;
                    g.Respawn(new Vec3(c0.x + 8, c0.y, c0.z), 0);
                    yield return null;
                    LookAt(c0 + new Vec3(0, -1.0, 0));
                    yield return Shot("B47-beat-cameo-tell-locked"); beatShots++;
                    for (int f = 0; f < 600 && sim.Cameo47Telling && sim.Cameo47Progress < 0.83; f++) { God(g); LookAt(c0 + new Vec3(0, -1.0, 0)); yield return null; }
                    yield return Shot("B47-beat-cameo-tell-last"); beatShots++;
                    for (int f = 0; f < 600 && sim.Cameo47BreachSteps < 0; f++) { God(g); LookAt(c0); yield return null; }
                    for (float t = 0; t < 0.35f; t += Time.unscaledDeltaTime) { God(g); LookAt(c0 + new Vec3(0, 1.5, 0)); yield return null; }
                    yield return Shot("B47-beat-cameo-breach"); beatShots++;
                }
            }

            // walk in through d8 (walkin46), then face the worm
            g.Respawn(kb4.spawn, 0);
            g.input.SetLook(0, 0.02);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0.02, moveZ = 1 };
            for (int f = 0; f < 600 && (sim.wyrm24 == null || !sim.wyrm24.Alive); f++) { God(g); yield return null; }
            Wyrm w = sim.wyrm24;
            if (w == null || !w.Alive) { res[0] = "RED a347: the worm's fight did not begin"; yield break; }
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0.02 };
            // the lead's round-4 list (wyrmLook47's frames: emerge / coil / breach): the emerge 0.8 s into the fight, facing the head
            for (float t = 0; t < 0.8f; t += Time.unscaledDeltaTime)
            {
                God(g);
                Vec3 ed = w.boxes[0].c - (sim.mover.position + new Vec3(0, 1.6, 0));
                double ey = Math.Atan2(-ed.x, -ed.z), ep = Math.Atan2(ed.y, Math.Sqrt(ed.x * ed.x + ed.z * ed.z));
                g.input.SetLook(ey, ep);
                g.forcedMove = new InputFrame { yaw = ey, pitch = ep };
                yield return null;
            }
            yield return Shot("B47-wyrm-emerge");
            bool breachShot = false;
            int seen = w.log.Count;
            bool crack = false, crackLast = false, aggro = false;
            Vec3? aimCrack = null;
            bool trailShot = false;
            for (float t = 0; t < 90f && w.Alive && !(crackLast && aggro); t += Time.unscaledDeltaTime)
            {
                God(g);
                // round 4 trail47 / wyrmLook47: once pieces are buried, one frame of the glowing trail on the floor, seen from 10 m, low
                if (!trailShot)
                {
                    int bi = -1;
                    for (int i = 0; i < w.PieceCount; i++) if (w.buried[i]) { bi = i; break; }
                    if (bi >= 0)
                    {
                        trailShot = true;
                        Vec3 tp = w.boxes[bi].c;
                        double floorY = w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0;
                        g.Respawn(new Vec3(tp.x + 10, floorY, tp.z), 0);
                        yield return null;
                        Vec3 td = new Vec3(tp.x, floorY, tp.z) - (sim.mover.position + new Vec3(0, 1.6, 0));
                        double ty = Math.Atan2(-td.x, -td.z), tpch = Math.Atan2(td.y, Math.Sqrt(td.x * td.x + td.z * td.z));
                        g.input.SetLook(ty, tpch);
                        g.forcedMove = new InputFrame { yaw = ty, pitch = tpch };
                        yield return null;
                        yield return Shot("B47-wyrm-trail");
                    }
                }
                // keep 10-15 m from the head (the camera faces it): step back when nearer than 10 m (a reel-only cheat)
                Vec3 hd = w.boxes[0].c - sim.mover.position;
                double hl = Math.Sqrt(hd.x * hd.x + hd.z * hd.z);
                if (hl < 10 && hl > 1e-3) g.Respawn(new Vec3(w.boxes[0].c.x - hd.x / hl * 12, 0, w.boxes[0].c.z - hd.z / hl * 12), 0);
                Vec3 d = (aimCrack ?? w.boxes[0].c) - (sim.mover.position + new Vec3(0, 1.6, 0));
                double yaw = Math.Atan2(-d.x, -d.z), pitch = Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z));
                g.input.SetLook(yaw, pitch);
                g.forcedMove = new InputFrame { yaw = yaw, pitch = pitch };
                for (; seen < w.log.Count; seen++)
                {
                    WyrmLog e = w.log[seen];
                    if (e.resolve) continue;
                    if (!crack && e.name == "crack")
                    {
                        // the lead's r0e note: aim at the crack on the floor, from 12 m (the player put there, God; a reel-only cheat)
                        WyrmMarker mk = null;
                        foreach (WyrmMarker m in w.markers) if (m.kind == "diveCrack") mk = m;
                        if (mk != null)
                        {
                            Vec3 me = sim.mover.position, to = mk.pos - me;
                            double len = Math.Sqrt(to.x * to.x + to.z * to.z);
                            if (len > 1e-3) g.Respawn(new Vec3(mk.pos.x - to.x / len * 12, 0, mk.pos.z - to.z / len * 12), 0);
                            Vec3 eye = sim.mover.position + new Vec3(0, 1.6, 0), dd = mk.pos - eye;
                            double cy = Math.Atan2(-dd.x, -dd.z), cp = Math.Atan2(dd.y, Math.Sqrt(dd.x * dd.x + dd.z * dd.z));
                            g.input.SetLook(cy, cp);
                            g.forcedMove = new InputFrame { yaw = cy, pitch = cp };
                            aimCrack = mk.pos;
                        }
                        crack = true;
                        yield return Shot("B47-wyrm-crack-start");
                        yield return Hold(Math.Max(0.05f, (float)e.seconds - 0.2f));
                        crackLast = true;
                        yield return Shot("B47-wyrm-crack-last");
                        aimCrack = null;
                    }
                    else if (!aggro && e.name == "aggro")
                    {
                        aggro = true;
                        yield return Shot("B47-wyrm-aggro-start");
                    }
                    else if (!breachShot && e.name == "dive")
                    {
                        // the breach out of the wall (the dive's exit tell): wait for it, then the frame (the head is faced each step)
                        breachShot = true;
                        yield return Hold((float)e.seconds + 0.15f);
                        yield return Shot("B47-wyrm-breach");
                    }
                }
                yield return null;
            }
            yield return Shot("B47-wyrm-mid");
            res[0] = (crack ? "GREEN" : "RED") + $" a347: {sim.campaignLevel.id}; crack tell seen {crack}, its last frame {crackLast}, aggro tell {aggro}; beat frames {beatShots}; frames {shots.Count} ({string.Join(", ", shots)})";
        }
    }
}
