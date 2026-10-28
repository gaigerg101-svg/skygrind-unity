// NIGHT48 builder B: the Player scenario a348 (level 3: THE KILN WYRM), dispatched by FeelReel.N48.cs (the lead's routing). Not run by its
// author (no Player in a builder worktree). Boot: -skygrindMap n47-kiln -skygrindReelOnly a348 (reel.sh, L-062). Forced sim rows: walkin46,
// wyrm46, wyrm47 + B's NIGHT48 rows wyrmTell48, wyrmDive48, kilnBoost48 (sim.rowsOverride before the run; put back after); view rows
// (wyrmLook47) are the build copy's: take the reel once with wyrmLook47 ON and once OFF for G32's pair. The player is God; input by forced
// frames only (L-046; this reel presses no button: no sim.UseInput). Frames: dir/shot-N29-<name>.png.
//   G35  B48-beat-glimpse-top    the glimpse's head at its arc's top (Wyrm47BeatsView: s = 1.6 gs, the top at s 0.5), seen from kb1's top
//        B48-beat-glimpse-back   0.25 s later: the molten back across the arc (the body's segments at their tops)
//        B48-beat-pass / B48-beat-cameo-breach   the other two beats, re-taken
//   G32  B48-wyrm-emerge-body / B48-wyrm-coil-body / B48-wyrm-damage-flash-body / B48-wyrm-dive-breach-body   the molten worm's body framed
//        (n48-r3: the unburied pieces' centroid, the camera set back to fit them; wyrmLook47 on vs off: two boots)
//   G31  B48-wyrm-tell-start / -tell-mid / -tell-last   one head-charge tell (1.0 s with wyrmTell48) at its start, half way and 0.2 s before
//        the charge: the head yellow (Wyrm30Punchable) through the WHOLE tell; the line prints the share of tell frames with it punchable
//   G34  B48-wyrm-dive-crack / B48-wyrm-dive-breach   a stagger forced (a reel-only cheat: Stagger30 + Wyrm31.OnStagger, then the STAGGERED
//        window ended after 1.5 s), then the dive wyrmDive48 starts: its crack tell and its exit
//   G33  B48-kiln-island        standing on a slag island (hp 40, the pickup taken: the line prints the heal), the worm in view
// Line: GREEN when the tell frames were punchable >= 90 % of the tell, a dive followed the forced stagger and the island healed.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N48Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Wyrm48Reel(g, dir, res, offs, pre);

        static IEnumerator Wyrm48Reel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            NightRowState before = sim.rowsOverride;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.sim.rowsOverride = before; });
            try
            {
                sim.rowsOverride = sim.rowsRegistry.Defaults().Set("walkin46", true).Set("wyrm46", true).Set("wyrm47", true)
                    .Set("wyrmTell48", true).Set("wyrmDive48", true).Set("kilnBoost48", true);
            }
            catch (Exception e) { res[0] = "RED a348: the build lacks a row (" + e.Message + ")"; yield break; }
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a348: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            RoomDef kb4 = sim.campaignLevel?.rooms.Find(r => r.id == "kb4");
            if (kb4 == null || sim.campaignLevel.id != "n47-kiln") { res[0] = $"RED a348: the run is on {sim.campaignLevel?.id ?? "?"} (boot -skygrindMap n47-kiln through reel.sh, L-062)"; yield break; }
            g.forcedCombat = IdleCombat;
            God(g);
            var st = new double[4];
            var shots = new List<string>();
            IEnumerator Hold(float seconds) { for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { God(g); yield return null; } }
            IEnumerator Shot(string name) { Wyrm46ClearCards(g); yield return null; Wyrm46ClearCards(g); yield return Shot29(dir, name, st); shots.Add(name); }
            void LookAt(Vec3 at)
            {
                Vec3 d = at - (sim.mover.position + new Vec3(0, 1.6, 0));
                double y = Math.Atan2(-d.x, -d.z), p = Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z));
                g.input.SetLook(y, p);
                g.forcedMove = new InputFrame { yaw = y, pitch = p };
            }
            Start(g, "a348", "camera", dir);
            yield return Frames(pre);

            // G35: the beats (n47-kiln)
            RoomDef kb1 = sim.campaignLevel.rooms.Find(r => r.id == "kb1"), kb2 = sim.campaignLevel.rooms.Find(r => r.id == "kb2"), kb3 = sim.campaignLevel.rooms.Find(r => r.id == "kb3");
            if (kb1 != null)
            {
                g.Respawn(new Vec3(kb1.spawn.x, kb1.spawn.y + 9.2, kb1.spawn.z - 3), 0);
                for (int f = 0; f < 120 && sim.Glimpse47Steps < 0; f++) { God(g); yield return null; }
                double gl = g.cfg.Num("design.n47.wyrm47.numbers.glimpseS");
                // the head's arc point (Wyrm47BeatsView): s = 1.6 gs; x from at.x - 7 to + 7, y at.y + 3 + 5 sin(pi s), z at.z + 6; the top at s 0.5
                Vec3 Arc(double s) => new Vec3(sim.Glimpse47At.x - 7 + 14 * s, sim.Glimpse47At.y + 3 + 5 * Math.Sin(Math.PI * s), sim.Glimpse47At.z + 6.0);
                int topSteps = (int)(0.5 / 1.6 * gl * 120);
                for (int f = 0; f < 600 && sim.Glimpse47Steps >= 0 && sim.Glimpse47Steps < topSteps - 2; f++) { God(g); LookAt(Arc(0.5)); yield return null; }
                LookAt(Arc(0.5));
                yield return Shot("B48-beat-glimpse-top");
                int backSteps = topSteps + 30;
                for (int f = 0; f < 600 && sim.Glimpse47Steps >= 0 && sim.Glimpse47Steps < backSteps; f++) { God(g); LookAt(Arc(0.45)); yield return null; }
                yield return Shot("B48-beat-glimpse-back");
            }
            if (kb2 != null)
            {
                g.Respawn(kb2.spawn, 0);
                for (int f = 0; f < 1200 && sim.Pass47Steps < 0; f++) { God(g); g.forcedMove = new InputFrame { yaw = 0, pitch = -0.35 }; g.input.SetLook(0, -0.35); yield return null; }
                for (float t = 0; t < 1.0f; t += Time.unscaledDeltaTime) { God(g); LookAt((sim.Pass47From + sim.Pass47To) * 0.5); yield return null; }
                yield return Shot("B48-beat-pass");
            }
            if (kb3 != null)
            {
                g.Respawn(kb3.spawn, 0);
                for (int f = 0; f < 1200 && !sim.Cameo47Telling; f++) { God(g); yield return null; }
                for (int f = 0; f < 600 && sim.Cameo47Telling; f++) { God(g); yield return null; }
                Vec3 c0 = sim.Cameo47At;
                g.Respawn(new Vec3(c0.x + 8, c0.y, c0.z), 0);
                for (float t = 0; t < 0.35f; t += Time.unscaledDeltaTime) { God(g); LookAt(c0 + new Vec3(0, 1.5, 0)); yield return null; }
                yield return Shot("B48-beat-cameo-breach");
            }

            // the fight: walk in through d8, then face the worm
            g.Respawn(kb4.spawn, 0);
            g.input.SetLook(0, 0.02);
            g.forcedMove = new InputFrame { yaw = 0, pitch = 0.02, moveZ = 1 };
            for (int f = 0; f < 600 && (sim.wyrm24 == null || !sim.wyrm24.Alive); f++) { God(g); yield return null; }
            Wyrm w = sim.wyrm24;
            if (w == null || !w.Alive) { res[0] = "RED a348: the worm's fight did not begin"; yield break; }
            void FaceHead(double far)
            {
                Vec3 hd = w.boxes[0].c - sim.mover.position;
                double hl = Math.Sqrt(hd.x * hd.x + hd.z * hd.z);
                if ((hl < far - 2 || hl > far + 4) && hl > 1e-3) g.Respawn(new Vec3(w.boxes[0].c.x - hd.x / hl * far, w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0, w.boxes[0].c.z - hd.z / hl * far), 0);
                LookAt(w.boxes[0].c);
            }
            // n48-r2 (the lead): the emerge / coil / damage-flash frames showed the lava floor and the core beam, not the worm (the camera
            // looked at the head's box from the walk-in, often under the floor). Now each frame waits for the body to be out (>= 4 unburied
            // pieces, the head 1.5 m over the floor) and frames the unburied pieces: the camera on the arena's side of their centroid, set
            // back to fit their spread (12 m or more), kept inside the arena, the reticle on the centroid
            double floorY = w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0;
            // n48-r3 (the lead): the -body frames still showed the core beam and the lava floor. Two faults: the sim's "unburied" is y over
            // buriedY (-1 m), so pieces half inside the lava floor counted and pulled the centroid into the floor; and the camera stood on the
            // arena's centre side, so the core beam in the middle filled the view. Now only pieces whose centre is 0.6 m over the floor count;
            // the camera takes, of 8 bearings round the body, the spot whose sight line passes farthest from the arena's centre (the beam), set
            // back until every counted piece projects inside the frame (the real camera, g.cam), and each frame is named with the body's
            // pixel height on screen
            bool Above(int i) => !w.buried[i] && w.boxes[i].c.y - floorY > 0.6;
            int Out() { int n = 0; for (int i = 0; i < w.PieceCount; i++) if (Above(i)) n++; return n; }
            bool BodyUp() => Out() >= 5;
            double back = 1;
            void FrameBody()
            {
                double cx = 0, cy = 0, cz = 0;
                int n = 0;
                for (int i = 0; i < w.PieceCount; i++) { if (!Above(i)) continue; cx += w.boxes[i].c.x; cy += w.boxes[i].c.y; cz += w.boxes[i].c.z; n++; }
                if (n == 0) { LookAt(w.boxes[0].c); return; }
                var c = new Vec3(cx / n, cy / n, cz / n);
                double ext = 0;
                for (int i = 0; i < w.PieceCount; i++) { if (!Above(i)) continue; double dx = w.boxes[i].c.x - c.x, dz = w.boxes[i].c.z - c.z; ext = Math.Max(ext, Math.Sqrt(dx * dx + dz * dz)); }
                double dist = Math.Max(12, ext * 1.6 + 6) * back;
                Vec3 ac = w.arenaCenter ?? c;
                Vec3 best = default;
                double bestClear = -1;
                for (int k = 0; k < 8; k++)
                {
                    double a = k * Math.PI / 4;
                    var at = new Vec3(c.x + Math.Cos(a) * dist, floorY, c.z + Math.Sin(a) * dist);
                    double ox = at.x - ac.x, oz = at.z - ac.z, ol = Math.Sqrt(ox * ox + oz * oz);
                    if (ol > 15) at = new Vec3(ac.x + ox / ol * 15, floorY, ac.z + oz / ol * 15);
                    double sx = c.x - at.x, sz = c.z - at.z, sl = sx * sx + sz * sz;
                    if (sl < 64) continue; // closer than 8 m: the body would overflow the frame
                    double tt = Math.Max(0, Math.Min(1, ((ac.x - at.x) * sx + (ac.z - at.z) * sz) / sl));
                    double px = at.x + sx * tt - ac.x, pz = at.z + sz * tt - ac.z, clear = Math.Sqrt(px * px + pz * pz);
                    if (clear > bestClear) { bestClear = clear; best = at; }
                }
                if (bestClear < 0) best = new Vec3(c.x + dist, floorY, c.z);
                Vec3 me = sim.mover.position;
                if (Math.Abs(me.x - best.x) + Math.Abs(me.z - best.z) > 1.5) g.Respawn(best, 0);
                sim.mover.invulnerable = true;
                LookAt(new Vec3(c.x, c.y + 0.5, c.z));
            }
            // the body on screen: the counted pieces projected by the camera (each a 1.3 m sphere); in = pieces inside the frame
            int BodyPx(out int inFrame, out int counted)
            {
                inFrame = counted = 0;
                Camera cam = g.cam;
                if (cam == null) return 0;
                float y0 = float.MaxValue, y1 = float.MinValue;
                for (int i = 0; i < w.PieceCount; i++)
                {
                    if (!Above(i)) continue;
                    counted++;
                    Vector3 u = WebSpace.ToUnity(w.boxes[i].c);
                    Vector3 sp = cam.WorldToScreenPoint(u);
                    if (sp.z <= 0.5f) continue;
                    float r = Mathf.Abs(cam.WorldToScreenPoint(u + cam.transform.up * 1.3f).y - sp.y);
                    if (sp.x >= 0 && sp.x <= cam.pixelWidth && sp.y >= 0 && sp.y <= cam.pixelHeight) inFrame++;
                    y0 = Mathf.Min(y0, sp.y - r);
                    y1 = Mathf.Max(y1, sp.y + r);
                }
                return y1 > y0 ? (int)(Mathf.Min(cam.pixelHeight, y1) - Mathf.Max(0, y0)) : 0;
            }
            var bodyLines = new List<string>();
            IEnumerator BodyShot(string name, float maxWait)
            {
                back = 1;
                int h = 0, inF = 0, cnt = 0;
                for (float t = 0; t < maxWait; t += Time.unscaledDeltaTime)
                {
                    God(g);
                    FrameBody();
                    yield return null;
                    if (!BodyUp()) continue;
                    h = BodyPx(out inF, out cnt);
                    if (inF < cnt && back < 2.2) { back *= 1.12; continue; } // a piece outside the frame: set back
                    if (inF == cnt && h >= 60) break;
                }
                h = BodyPx(out inF, out cnt);
                string full = $"{name}-{h}px";
                yield return Shot(full);
                bodyLines.Add($"{full} ({inF}/{cnt} pieces in frame)");
            }
            yield return BodyShot("B48-wyrm-emerge-body", 8f);
            for (float t = 0; t < 4f; t += Time.unscaledDeltaTime) { God(g); FrameBody(); yield return null; }
            yield return BodyShot("B48-wyrm-coil-body", 8f);
            sim.loadout.Damage(w.body, 40, false, "revolver", sim.mover.position + new Vec3(0, 1.6, 0)); // a real hit: the damage flash
            yield return null;
            yield return BodyShot("B48-wyrm-damage-flash-body", 0.05f);
            int bodyOut = Out();

            // G31: one head-charge tell, the player 9 m from the head (inside the aggro radius), the head faced
            int tellFrames = 0, punchFrames = 0;
            bool tellSeen = false;
            for (float t = 0; t < 40f && w.Alive && !tellSeen; t += Time.unscaledDeltaTime)
            {
                God(g);
                if (!w.AggroTelling) { FaceHead(9); yield return null; continue; }
                tellSeen = true;
                double tellS = w.p.tells["aggro"];
                float t0 = Time.unscaledTime;
                yield return Shot("B48-wyrm-tell-start");
                bool mid = false, last = false;
                while (w.Alive && w.AggroTelling)
                {
                    God(g);
                    LookAt(w.boxes[0].c);
                    tellFrames++;
                    if (sim.Wyrm30Punchable) punchFrames++;
                    float el = Time.unscaledTime - t0;
                    if (!mid && el >= tellS * 0.5) { mid = true; yield return Shot("B48-wyrm-tell-mid"); }
                    if (!last && el >= tellS - 0.2) { last = true; yield return Shot("B48-wyrm-tell-last"); }
                    yield return null;
                }
            }

            // G34: a forced stagger (reel-only), its window ended after 1.5 s, then wyrmDive48's dive
            int divesBefore = sim.dives48;
            bool dived = false;
            if (w.Alive && sim.wyrm31 != null)
            {
                Vec3 me = sim.mover.position, head = w.boxes[0].c;
                if (w.Stagger30(g.cfg.Num("design.n30.wyrm30.recoilSeconds"), g.cfg.Num("design.n30.wyrm30.recoilBack"), g.cfg.Num("design.n30.wyrm30.recoilUp"), head.x - me.x, head.z - me.z))
                    sim.wyrm31.OnStagger();
                for (float t = 0; t < 1.5f; t += Time.unscaledDeltaTime) { God(g); FaceHead(12); yield return null; }
                sim.wyrm31.EndStagger47();
                for (int f = 0; f < 240 && sim.dives48 == divesBefore; f++) { God(g); FaceHead(12); yield return null; }
                dived = sim.dives48 > divesBefore;
                if (dived)
                {
                    yield return Hold(0.3f);
                    WyrmMarker mk = null;
                    foreach (WyrmMarker m in w.markers) if (m.kind == "diveCrack") mk = m;
                    if (mk != null) LookAt(mk.pos); else LookAt(w.boxes[0].c);
                    yield return Shot("B48-wyrm-dive-crack");
                    for (float t = 0; t < 6f && w.PatternName == "dive"; t += Time.unscaledDeltaTime) { God(g); LookAt(w.boxes[0].c); yield return null; }
                    yield return BodyShot("B48-wyrm-dive-breach-body", 4f);
                }
            }

            // G33: on a slag island, hp 40: the pickup
            int healsBefore = sim.heals48;
            if (w.Alive && w.arenaCenter.HasValue)
            {
                Vec3 c = w.arenaCenter.Value;
                double a = g.cfg.Num("design.n48.kilnBoost48.numbers.islandFirstDeg") * Math.PI / 180, r = g.cfg.Num("design.n48.kilnBoost48.numbers.islandR");
                g.Respawn(new Vec3(c.x + r * Math.Cos(a), c.y + g.cfg.Num("design.n48.kilnBoost48.numbers.islandTop") + 0.05, c.z + r * Math.Sin(a)), 0);
                // n48-r1: God (max = hp = 1e6 each frame) left hp at max, so the pickup (hp under max) never fired: here max is the game's and hp 40,
                // the player kept alive by the mover's invulnerable flag, until the heal fires (or 2 s)
                for (float t = 0; t < 2.0f && sim.heals48 == healsBefore; t += Time.unscaledDeltaTime)
                {
                    sim.health.max = g.cfg.Num("health.max");
                    if (sim.heals48 == healsBefore) sim.health.hp = Math.Min(sim.health.hp, 40);
                    sim.mover.invulnerable = true;
                    LookAt(w.boxes[0].c);
                    yield return null;
                }
                yield return Shot("B48-kiln-island");
                God(g);
            }
            bool healed = sim.heals48 > healsBefore;
            double share = tellFrames > 0 ? punchFrames / (double)tellFrames : 0;
            bool green = tellSeen && share >= 0.9 && dived && healed;
            res[0] = (green ? "GREEN" : "RED") + $" a348: {sim.campaignLevel.id}; tell seen {tellSeen}, punchable {punchFrames}/{tellFrames} tell frames ({share:P0}); dive after the stagger {dived}; island heal {healed}; body pieces out at the damage flash {bodyOut}/{w.PieceCount}; body frames: {string.Join("; ", bodyLines)}; frames {shots.Count} ({string.Join(", ", shots)})";
        }
    }
}
