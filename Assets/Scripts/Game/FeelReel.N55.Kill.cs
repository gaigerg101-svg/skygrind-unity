// NIGHT55 builder K: the a355 phase (FeelReel.N55.cs routes it here): kill55's Player proof. Not run by its author (builders never run a
// Player). A campaign level (L-062):
//   bash Tools/night/n47-reels.sh <tag> k1on,n52-foundry,a355                    (the build copy's config with design.n55.kill55 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> k1off,n52-foundry,a355,-skygrindOff55    (the row off, NIGHT54's kills: RED)
// The CAMPAIGN on the booted map, DOORFREE, God, the level's husks killed and frozen, the KESTREL in hand, standing at a non-boss room's spawn.
//   1. CHAIN: UnityTuning.Kill55ReelChain husks (1 hp, held) in an arc Kill55ReelAhead m ahead, shot one after another at the KESTREL's own
//      rate (NIGHT56 G2: a press / release every other frame, the release inside the interval queued to its end: 58 steps = 0.483 s, the live
//      interval 0.4 x design.n39.p39.intervalMult 1.2): each kill's play time, real time, chain index, the rules' freeze, kill55's extra and the
//      frames held after it (HeldThisFrame; Kill55ReelChainTail frames after the last) are printed, and kills 1 -> 3 in play seconds (the slate:
//      <= 1 s). shot-N55-K-chain.jpg on the 3rd kill's first held frame.
//   2. DEBRIS: Kill55ReelDebrisWait frames (1.5 s) after the chain's last kill: each kill's shards alive and in view (Kill55Debris), the chunks'
//      blocks alive (Chunks.Live, NIGHT54's debris: all gone by ChunkLife 1.45 s). shot-N55-K-debris.jpg.
//   3. SPACED: four husks shot Kill55ReelSpacedEvery frames apart (off a chain): their held frames and seconds (NIGHT54's 0.05 s each).
//   4. DAMAGE: one husk at full health (40), held 5 m ahead; the frame before a KESTREL body shot (25: 15 / 40 left, a hole), the next frame and
//      the frame Kill55ReelDamageWait frames later (the white hit flash over): the share of the body's pixels that changed against the before
//      frame. shot-N55-K-hit-before.jpg / -next.jpg / -after.jpg.
// GREEN: the row on, the chain's 1st kill held <= 0.06 s and every 3rd-or-later kill >= 0.08 s (frames held x the fixed frame time, and the
// layer's own record), each <= 0.12 s; every chain kill >= 8 shards alive at +1.5 s; the spaced kills' held time <= 0.05 s + a frame each; the
// damage share at +0.5 s >= 5 %. NIGHT57 G (G3): the damage line is GREEN on the NEXT frame's marks (changed and dark: >= 5 % of the body's
// pixels, Kill55ReelMarkNextShare; the lead's MAIN); the +0.5 s numbers are printed, not required. One line "GREEN a355 KILL | ..." (or RED),
// the full one in the log as "[a355] ...".
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N55Kill(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Kill55Scenario(g, dir, res, offs, pre);

        /// <summary>Saves the screen to dir/shot-N55-K-name.jpg and returns the texture (the caller destroys it). Yields to the frame's end.</summary>
        static IEnumerator K55Shot(string dir, string name, Texture2D[] keep)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = ScreenCapture.CaptureScreenshotAsTexture();
            if (t == null) yield break;
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "shot-N55-K-" + name + ".jpg"), t.EncodeToJPG(85));
            if (keep != null) keep[0] = t;
            else UnityEngine.Object.Destroy(t);
        }

        /// <summary>The husk's body rect on screen (pixels, bottom-left origin): its feet to its head, its width.</summary>
        static RectInt K55BodyRect(SurvivalGame g, Husk h, int w, int hgt)
        {
            Camera cam = g.cam;
            Vector3 feet = WebSpace.ToUnity(h.position);
            Vector3 a = cam.WorldToScreenPoint(feet + Vector3.up * 0.15f), b = cam.WorldToScreenPoint(feet + Vector3.up * (float)(h.Height * 0.8));
            Vector3 c = cam.WorldToScreenPoint(feet + Vector3.up * (float)(h.Height * 0.5) + cam.transform.right * (float)h.Radius);
            float cx = (a.x + b.x) * 0.5f, half = Mathf.Abs(c.x - cx);
            int x0 = Mathf.Clamp((int)(cx - half), 0, w - 1), x1 = Mathf.Clamp((int)(cx + half), 0, w - 1);
            int y0 = Mathf.Clamp((int)Mathf.Min(a.y, b.y), 0, hgt - 1), y1 = Mathf.Clamp((int)Mathf.Max(a.y, b.y), 0, hgt - 1);
            return new RectInt(x0, y0, Mathf.Max(1, x1 - x0), Mathf.Max(1, y1 - y0));
        }

        /// <summary>Of the before frame's body pixels (warm and saturated: the enemy's red-orange) inside `r`, the share whose colour moved by more
        /// than Kill55ReelPixelDelta in `after`; `n` the body pixels counted.</summary>
        static double K55Changed(Texture2D before, Texture2D after, RectInt r, out int n)
        {
            n = 0;
            int changed = 0;
            for (int y = r.yMin; y < r.yMax; y += 2)
                for (int x = r.xMin; x < r.xMax; x += 2)
                {
                    Color p = before.GetPixel(x, y);
                    Color.RGBToHSV(p, out float hh, out float s, out float v);
                    bool warm = s > 0.35f && v > 0.15f && (hh < 0.12f || hh > 0.94f);
                    if (!warm) continue;
                    n++;
                    Color q = after.GetPixel(x, y);
                    if (Mathf.Max(Mathf.Abs(p.r - q.r), Mathf.Max(Mathf.Abs(p.g - q.g), Mathf.Abs(p.b - q.b))) > UnityTuning.Kill55ReelPixelDelta) changed++;
                }
            return n > 0 ? changed / (double)n : 0;
        }

        /// <summary>NIGHT56 G (G2): of the before frame's warm body pixels inside `r`, the share that changed and are dark (V under Kill55ReelMarkV)
        /// in `after` (the crack / hole: the mark itself, not the flash or a lean), and the median hue / S / V of the changed pixels.</summary>
        static double K55Marks(Texture2D before, Texture2D after, RectInt r, out float medH, out float medS, out float medV)
        {
            int n = 0, dark = 0;
            var hs = new List<float>();
            var ss = new List<float>();
            var vs = new List<float>();
            for (int y = r.yMin; y < r.yMax; y += 2)
                for (int x = r.xMin; x < r.xMax; x += 2)
                {
                    Color p = before.GetPixel(x, y);
                    Color.RGBToHSV(p, out float hh, out float s, out float v);
                    if (!(s > 0.35f && v > 0.15f && (hh < 0.12f || hh > 0.94f))) continue;
                    n++;
                    Color q = after.GetPixel(x, y);
                    if (Mathf.Max(Mathf.Abs(p.r - q.r), Mathf.Max(Mathf.Abs(p.g - q.g), Mathf.Abs(p.b - q.b))) <= UnityTuning.Kill55ReelPixelDelta) continue;
                    Color.RGBToHSV(q, out float h2, out float s2, out float v2);
                    hs.Add(h2 * 360f);
                    ss.Add(s2);
                    vs.Add(v2);
                    if (v2 < UnityTuning.Kill55ReelMarkV) dark++;
                }
            hs.Sort(); ss.Sort(); vs.Sort();
            medH = hs.Count > 0 ? hs[hs.Count / 2] : -1;
            medS = ss.Count > 0 ? ss[ss.Count / 2] : -1;
            medV = vs.Count > 0 ? vs[vs.Count / 2] : -1;
            return n > 0 ? dark / (double)n : 0;
        }

        /// <summary>NIGHT56 G (G2): a hex colour's HSV hue (deg), S and V, for the line (the marks' own colours, UnityTuning.N55K.cs).</summary>
        static string K55Hsv(uint hex)
        {
            Color c = Skygrind.Game.Look.Hex(hex);
            Color.RGBToHSV(c, out float h, out float s, out float v);
            return $"#{hex:X6} H {h * 360f:0} S {s:0.00} V {v:0.00}";
        }

        static IEnumerator Kill55Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string F3(double v) => v.ToString("0.000", ci);
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            var killFrames = new List<int>();
            var killIds = new List<int>();
            int frame = 0;
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") { killFrames.Add(frame); killIds.Add(e.enemy); } }));
            offs.Add(() =>
            {
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a355 KILL: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 240;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 220) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            sim.husks.KillAll();
            bool on = Kill55.Live(g.cfg);
            string map = PostFx.LevelIdOf(g.map.json);

            // ---- the spot: a non-boss room's spawn with the clearest heading (Move54's line finder) ----
            LevelDef lv = sim.campaignLevel;
            Vec3 f0 = g.Feet, u = new Vec3(0, 0, -1);
            double bestLen = -1;
            if (lv != null)
                foreach (RoomDef r in lv.rooms)
                {
                    if (r.kind == "boss") continue;
                    Vec3 s0 = r.spawn;
                    if (!Move54Floor(g.world, s0.x, s0.z, s0.y + 1.5, out double sy, out _)) continue;
                    s0 = new Vec3(s0.x, sy, s0.z);
                    for (int k = 0; k < UnityTuning.Move54ReelHeadings; k++)
                    {
                        double a = 2 * Math.PI * k / UnityTuning.Move54ReelHeadings;
                        var uk = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                        double len = Move54Line(g.world, s0, uk, out _, out _);
                        if (len > bestLen) { bestLen = len; f0 = s0; u = uk; }
                    }
                    if (bestLen >= UnityTuning.Kill55ReelAhead + 3) break;
                }
            if (bestLen < UnityTuning.Kill55ReelAhead) { res[0] = $"RED a355 KILL: no clear line of {UnityTuning.Kill55ReelAhead} m from any room's spawn on {map}"; yield break; }
            double yawU = Math.Atan2(-u.x, -u.z);
            var side = new Vec3(u.z, 0, -u.x);
            yield return Equip27(g, "revolver"); // the KESTREL
            g.forcedCombat = IdleCombat;
            string pc = Preconditions(g, f0);
            g.Respawn(new Vec3(f0.x, f0.y + 0.05, f0.z), yawU * 180 / Math.PI);
            for (int i = 0; i < 40; i++) { God(g); g.forcedMove = new InputFrame(); g.input.SetLook(yawU, 0); yield return null; }

            // spawns n husks in an arc ahead (1 hp unless full), held in place by the caller's loop
            List<(Husk h, Vec3 at)> Place(int n, double ahead, bool full)
            {
                var list = new List<(Husk, Vec3)>();
                for (int k = 0; k < n; k++)
                {
                    double off = (k - (n - 1) * 0.5) * UnityTuning.Kill55ReelAcross;
                    double x = f0.x + u.x * ahead + side.x * off, z = f0.z + u.z * ahead + side.z * off;
                    double y = Move54Floor(g.world, x, z, f0.y + 2, out double fy, out _) ? fy : f0.y;
                    var at = new Vec3(x, y, z);
                    Husk h = null;
                    try { h = sim.husks.Spawn("husk", at, f0); } catch (Exception) { }
                    if (h == null) continue;
                    spawned.Add(h);
                    if (!full) h.hp = h.maxHp = 1;
                    list.Add((h, at));
                }
                return list;
            }
            void Hold(List<(Husk h, Vec3 at)> list)
            {
                foreach (var p in list) if (p.h.alive) { p.h.position = p.h.previous = p.at; }
            }

            Start(g, "a355-kill", "kill", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int HeldAfter(int f0r, int[] heldLog, int max)
            {
                int n = 0;
                for (int k = f0r; k < Math.Min(max, heldLog.Length); k++) { if (heldLog[k] == 1) n++; else if (n > 0) break; }
                return n;
            }

            // ---- 1. CHAIN ----
            var chain = Place(UnityTuning.Kill55ReelChain, UnityTuning.Kill55ReelAhead, false);
            for (int i = 0; i < 60; i++) { Hold(chain); God(g); g.input.SetLook(yawU, 0); yield return null; } // their spawn tells out
            var held = new int[4000];
            int recs0 = g.N55KHolds != null ? g.N55KHolds.RecCount : 0;
            int kills0 = killFrames.Count;
            int chainStart = frame;
            bool shotChain = false, pressedLast = false;
            for (int i = 0; i < 600 && killFrames.Count - kills0 < chain.Count; i++)
            {
                Hold(chain);
                God(g);
                Husk target = null;
                foreach (var p in chain) if (p.h.alive) { target = p.h; break; }
                if (target != null) { RideKill53Look(sim.Eye, target.AimPoint, out double yw, out double pt); g.input.SetLook(yw, pt); }
                // r1: fire HELD charged the KESTREL (a charged shot every ~4 s); a TAP the moment it read ready, released the next frame, landed the
                // kills 61-62 steps apart (0.508-0.517 s: n56-r1, and `n56 kestrel --scheme tap` headless), over the KESTREL's live interval.
                // NIGHT56 G (G2): a press / release every other frame while a husk lives (a thumb hammering the trigger): a release inside the
                // interval is queued and fires on the step it ends (charge35 / p39), so the kills land at the KESTREL's own rate, 58 steps =
                // 0.483 s (weapons.revolver.fireInterval 0.4 x design.n39.p39.intervalMult 1.2; `n56 kestrel --scheme alt`: kills 1 -> 3 in 0.967 s)
                bool tap = target != null && !pressedLast;
                pressedLast = tap;
                g.forcedCombat = tap ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                yield return null;
                frame++;
                if (frame < held.Length) held[frame] = g.HeldThisFrame ? 1 : 0;
                if (!shotChain && killFrames.Count - kills0 >= 3 && g.HeldThisFrame) { shotChain = true; yield return K55Shot(dir, "chain", null); }
            }
            g.forcedCombat = IdleCombat;
            // NIGHT56 G (G2): the chain's last kill's hold is recorded too (r1: the loop ended on its frame, so kill 6 read "held 0 frames" on both
            // sides): Kill55ReelChainTail frames more, held frames logged
            for (int i = 0; i < UnityTuning.Kill55ReelChainTail; i++)
            {
                Hold(chain);
                God(g);
                yield return null;
                frame++;
                if (frame < held.Length) held[frame] = g.HeldThisFrame ? 1 : 0;
            }
            int chainKills = killFrames.Count - kills0;
            var chainLine = new StringBuilder();
            bool ladderOk = chainKills >= 3, capOk = true;
            // NIGHT56 G (G2): the slate's "chain >= 3 kills inside 1 s": kills 1 -> 3 of the chain, in play seconds (the kill55 probe's sim time;
            // the reel's own frame count x the fixed frame time with the row off)
            double k13 = double.NaN;
            if (chainKills >= 3)
            {
                Kill55Hold kp = g.N55KHolds;
                if (kp != null && recs0 + 2 < kp.RecCount) k13 = kp.ProbeAt(recs0 + 2).simT - kp.ProbeAt(recs0).simT;
                else k13 = (killFrames[kills0 + 2] - killFrames[kills0]) * FeelClock.Fixed;
            }
            bool threeInOne = chainKills >= 3 && k13 <= 1.0 + 1e-6;
            double dtF = FeelClock.Fixed;
            var chainTags = new List<int>();
            Kill55Hold kh = g.N55KHolds;
            for (int k = 0; k < chainKills; k++)
            {
                int kf = killFrames[kills0 + k];
                int nh = HeldAfter(kf, held, kf + 20);
                double heldS = nh * dtF;
                string rec = "row off";
                double total = heldS;
                if (kh != null && recs0 + k < kh.RecCount)
                {
                    Kill55Hold.KillRec r = kh.ProbeAt(recs0 + k);
                    total = Math.Max(r.rulesS + r.extraS, 0);
                    rec = $"chain {r.chain}, play {F3(r.simT)} s, real {F3(r.realT)} s, rules {r.cause} {F3(r.rulesS)} + kill55 {F3(r.extraS)} = {F3(total)} s ({r.note})";
                }
                chainLine.Append($" | kill {k + 1}: held {nh} frames ({F3(heldS)} s), {rec}");
                double seen = Math.Max(heldS, total);
                if (k == 0 && seen > 0.06 + 1e-6) ladderOk = false;
                if (k >= 2 && seen < 0.08 - 1e-6) ladderOk = false;
                if (seen > 0.12 + dtF + 1e-6) capOk = false;
            }

            // ---- 2. DEBRIS (1.5 s after the chain's last kill) ----
            int lastTag = g.N55KLastKillTag;
            for (int i = 0; i < UnityTuning.Kill55ReelDebrisWait; i++) { God(g); yield return null; frame++; }
            yield return K55Shot(dir, "debris", null);
            var debrisLine = new StringBuilder();
            bool debrisOk = chainKills > 0;
            Plane[] planes = g.cam != null ? GeometryUtility.CalculateFrustumPlanes(g.cam) : null;
            int chunksLive = EnemyLook.For(g).chunks.Live;
            Kill55Debris kd = g.N55KDebris;
            for (int k = 0; k < chainKills; k++)
            {
                int tag = lastTag - (chainKills - 1 - k);
                int alive = kd != null ? kd.CountOf(tag) : 0, inView = kd != null ? kd.CountOf(tag, planes) : 0, resting = kd != null ? kd.RestingOf(tag) : 0;
                debrisLine.Append($"{(k > 0 ? ", " : "")}kill {k + 1}: {alive} alive ({inView} in view, {resting} resting)");
                if (alive < 8) debrisOk = false;
            }
            string debrisNums = $"debris at +{F3(UnityTuning.Kill55ReelDebrisWait * dtF)} s: {debrisLine}; the chunks' blocks alive {chunksLive}; shards alive in all {(kd != null ? kd.Live : 0)}";

            // ---- 3. SPACED (off a chain) ----
            var spaced = Place(4, UnityTuning.Kill55ReelAhead, false);
            for (int i = 0; i < 60; i++) { Hold(spaced); God(g); g.input.SetLook(yawU, 0); yield return null; frame++; }
            int kills1 = killFrames.Count;
            for (int i = 0; i < 600 && killFrames.Count - kills1 < spaced.Count; i++)
            {
                Hold(spaced);
                God(g);
                Husk target = null;
                foreach (var p in spaced) if (p.h.alive) { target = p.h; break; }
                if (target != null) { RideKill53Look(sim.Eye, target.AimPoint, out double yw, out double pt); g.input.SetLook(yw, pt); }
                bool press = target != null && i % UnityTuning.Kill55ReelSpacedEvery == 0;
                g.forcedCombat = press ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                yield return null;
                frame++;
                if (frame < held.Length) held[frame] = g.HeldThisFrame ? 1 : 0;
            }
            g.forcedCombat = IdleCombat;
            int spacedKills = killFrames.Count - kills1;
            double spacedTotal = 0;
            var spacedLine = new StringBuilder();
            bool spacedOk = spacedKills > 0;
            for (int k = 0; k < spacedKills; k++)
            {
                int kf = killFrames[kills1 + k];
                int nh = HeldAfter(kf, held, kf + 20);
                spacedTotal += nh * dtF;
                spacedLine.Append($"{(k > 0 ? ", " : "")}{nh} frames");
                if (nh * dtF > 0.05 + dtF + 1e-6) spacedOk = false;
            }

            // ---- 4. DAMAGE ----
            var victim = Place(1, 5, true);
            string damageNums = "no victim placed";
            bool damageOk = false;
            if (victim.Count == 1)
            {
                Husk h = victim[0].h;
                for (int i = 0; i < 60; i++) { Hold(victim); God(g); RideKill53Look(sim.Eye, h.AimPoint, out double yw, out double pt); g.input.SetLook(yw, pt); yield return null; frame++; }
                var tb = new Texture2D[1];
                var tn = new Texture2D[1];
                var ta = new Texture2D[1];
                yield return K55Shot(dir, "hit-before", tb);
                double hp0 = h.hp;
                // r1: the shot landed after the 30-frame wait (the next frame was taken before the hit): a tap when the gun is ready, up to 3 s
                bool tapped = false;
                for (int i = 0; i < 180 && h.hp >= hp0 && h.alive; i++)
                {
                    bool tap = !tapped && g.loadout.CooldownFraction(g.cfg.Dt) <= 0;
                    if (tap) tapped = true;
                    else if (tapped && i % 30 == 0) tapped = false; // a tap that missed: again
                    g.forcedCombat = tap ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                    Hold(victim);
                    God(g);
                    RideKill53Look(sim.Eye, h.AimPoint, out double yw2, out double pt2);
                    g.input.SetLook(yw2, pt2);
                    yield return null;
                    frame++;
                }
                g.forcedCombat = IdleCombat;
                bool hit = h.hp < hp0;
                yield return K55Shot(dir, "hit-next", tn);
                if (k58Mode) { IEnumerator k58 = K58FlashEnd(g, h, hit, tb[0], dir, () => { Hold(victim); God(g); }); while (k58.MoveNext()) yield return k58.Current; } // NIGHT58 G (G1): a258 waits for the flash's end (FeelReel.N58.Play.cs)
                for (int i = 0; i < UnityTuning.Kill55ReelDamageWait; i++) { Hold(victim); God(g); yield return null; frame++; }
                yield return K55Shot(dir, "hit-after", ta);
                if (tb[0] != null && tn[0] != null && ta[0] != null)
                {
                    RectInt rect = K55BodyRect(g, h, tb[0].width, tb[0].height);
                    double next = K55Changed(tb[0], tn[0], rect, out int nb), after = K55Changed(tb[0], ta[0], rect, out _);
                    double marks = K55Marks(tb[0], ta[0], rect, out float mh, out float ms, out float mv);
                    // NIGHT57 G (G3; the lead's MAIN, NIGHT57.md section 0 rule 5): the hit marks on the NEXT frame: body pixels (warm before) that
                    // changed and are dark (V < Kill55ReelMarkV) on the hit's next frame, where the hit flash turns the rest of the body white, so a
                    // dark pixel there is the mark (the OFF side: the white flash alone, ~0 %)
                    double marksNext = K55Marks(tb[0], tn[0], rect, out float nh, out float ns, out float nv);
                    Kill55Damage dm = g.N55KDamage;
                    // the slate: >= 5 % on the NEXT frame (the hit's own frame: the mark and the white hit flash); and >= 5 % at +0.5 s, the flash
                    // over, so the change is the mark's (RED off: the flash alone passes the first, never the second)
                    // NIGHT56 G (G2): n56-r1's OFF run passed both (the lean and the flash change the body too): the mark itself is now measured, the
                    // changed body pixels that are dark at +0.5 s (the crack / hole), and the changed pixels' median hue is printed (the critic's ask)
                    // NIGHT57 G (G3): GREEN on the MAIN, the next frame's marks (the +0.5 s lines are printed as before, not required)
                    damageOk = hit && h.alive && marksNext >= UnityTuning.Kill55ReelMarkNextShare;
                    damageNums = $"damage: hit {(hit ? "yes" : "NO")} ({F3(hp0)} -> {F3(h.hp)} hp, alive {h.alive}), body rect {rect.width}x{rect.height} px ({nb} body pixels sampled), " +
                                 $"MARKS NEXT FRAME (changed, V < {UnityTuning.Kill55ReelMarkV.ToString("0.00", ci)}) {(marksNext * 100).ToString("0.00", ci)} % of the body (want >= {(UnityTuning.Kill55ReelMarkNextShare * 100).ToString("0", ci)} %; the changed pixels' median H {nh.ToString("0", ci)} S {ns.ToString("0.00", ci)} V {nv.ToString("0.00", ci)}), " +
                                 $"changed next frame {(next * 100).ToString("0.0", ci)} %, at +{F3(UnityTuning.Kill55ReelDamageWait * dtF)} s {(after * 100).ToString("0.0", ci)} % (NIGHT56's line: >= {(UnityTuning.Kill55ReelPixelShare * 100).ToString("0", ci)} %), " +
                                 $"mark pixels (changed, V < {UnityTuning.Kill55ReelMarkV.ToString("0.00", ci)}) at +{F3(UnityTuning.Kill55ReelDamageWait * dtF)} s {(marks * 100).ToString("0.00", ci)} % (want >= {(UnityTuning.Kill55ReelMarkShare * 100).ToString("0.0", ci)} %), the changed pixels' median H {mh.ToString("0", ci)} S {ms.ToString("0.00", ci)} V {mv.ToString("0.00", ci)}; " +
                                 $"the marks' colours: crack {K55Hsv(UnityTuning.Kill55CrackHex)}, hole {K55Hsv(UnityTuning.Kill55HoleHex)}, hot {K55Hsv(UnityTuning.Kill55MarkHotHex)} cooling to {K55Hsv(UnityTuning.Kill55MarkEmberHex)} (the enemy family); marks laid {(dm != null ? dm.Laid : 0)}, holes {(dm != null ? dm.Holes : 0)}, drawn {(dm != null ? dm.DrawnLastFrame : 0)}";
                }
                foreach (Texture2D t in new[] { tb[0], tn[0], ta[0] }) if (t != null) UnityEngine.Object.Destroy(t);
            }
            yield return Frames(5);
            yield return Stop();

            string input = $"input: chain kills {chainKills} of {chain.Count} (from frame {chainStart}), spaced kills {spacedKills} of {spaced.Count}, kill55 records {(kh != null ? kh.RecCount - recs0 : 0)} (extended {(kh != null ? kh.Extended : 0)}, held {(kh != null ? kh.Rescued : 0)}, skipped {(kh != null ? kh.Skipped : 0)}, running {(kh != null ? kh.RunSkipped : 0)})";
            string nums = $"chain{chainLine} | kills 1 -> 3 in {F3(k13)} s of play (want <= 1 s) | {debrisNums} | spaced (off a chain): {spacedLine} = {F3(spacedTotal)} s held over {spacedKills} kills | {damageNums}";
            bool ok = on && ladderOk && capOk && threeInOne && debrisOk && spacedOk && damageOk;
            k58Ladder = ladderOk; // NIGHT58 G (G1): a258 reads the ladder
            string verdict = $"ladder {(ladderOk ? "ok" : "NO")}, cap {(capOk ? "ok" : "NO")}, 3 in 1 s {(threeInOne ? "ok" : "NO")}, debris {(debrisOk ? "ok" : "NO")}, spaced {(spacedOk ? "ok" : "NO")}, damage {(damageOk ? "ok" : "NO")}";
            Debug.Log($"[a355] kill55 on {on}; map {map}; {input}; {nums}; {verdict}; {pc}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a355 KILL | kill55 on {on} (map {map}) | {input} | {nums} | {verdict} | {pc}";
        }
    }
}
