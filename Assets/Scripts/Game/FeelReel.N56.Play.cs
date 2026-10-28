// NIGHT56 builder G: the a256 phase (FeelReel.N56.cs routes it here): G1 rank56's Player proof. Not run by its author (builders never run a
// Player). A campaign level (L-062):
//   bash Tools/night/n56-reels.sh <tag> g1on,n52-foundry,a256                    (the build copy's config with design.n56.rank56 on: GREEN)
//   bash Tools/night/n56-reels.sh <tag> g1off,n52-foundry,a256,-skygrindOff56    (the row off, NIGHT55's payoffs: RED on the heal line)
// The CAMPAIGN on the booted map, DOORFREE, the level's husks killed and frozen, the KESTREL in hand, standing at a non-boss room's spawn facing
// its clearest line (a355's spot finder), the style meter put at DULL (StyleCore.DebugSetRank(0)). Then a COMBAT: UnityTuning.Rank56ReelHusks
// husks (1 hp, held in place) on an arc Rank56ReelRadius m ahead (inside heal36's 4 m radius: every kill is a close kill), shot one after another
// at the KESTREL's own rate (a press / release every other frame: a355's G2 input), the health held at Rank56ReelHp of 100 before each kill so the
// heal shows. Printed: each kill's rank (the rank SHOWN when it landed, rank56's index) and heal36's heal (its event), the frame the rank first left
// DULL with husks still alive (shot-N56-G-rank.jpg: the meter's letter on a combat frame), the top rank, the heal at DULL and at the top rank
// and their ratio, rank56's own counters (Rank56Heals by rank).
// GREEN: the row on, every kill happened, the rank left DULL on a frame with a husk alive, and the heal at a rank above DULL is over the heal at
// DULL by the row's multiplier (x numbers.mult[rank], within 1e-6). RED otherwise (with the row off the heal is the same at every rank: RED on
// "pays"). One line "GREEN a256 PLAY | ..." (or RED), the full one in the log as "[a256] ...".
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
        static partial void N56Play(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Rank56Scenario(g, dir, res, offs, pre);

        /// <summary>Saves the screen to dir/shot-N56-G-name.jpg. Yields to the frame's end.</summary>
        static IEnumerator G56Shot(string dir, string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = ScreenCapture.CaptureScreenshotAsTexture();
            if (t == null) yield break;
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "shot-N56-G-" + name + ".jpg"), t.EncodeToJPG(85));
            UnityEngine.Object.Destroy(t);
        }

        static IEnumerator Rank56Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string F2(double v) => v.ToString("0.00", ci);
            res[0] = "RED a256 PLAY: began, the combat not reached";
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            var spawned = new List<Husk>();
            offs.Add(() =>
            {
                foreach (Husk h in spawned) if (h != null) h.alive = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.forcedMove = null;
            });
            if (!sim.campaignOn) { res[0] = "RED a256 PLAY: the run did not start as a CAMPAIGN"; yield break; }
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
            bool on = sim.Rank56On;
            string map = PostFx.LevelIdOf(g.map.json);

            // ---- the spot: a non-boss room's spawn with the clearest heading (a355's) ----
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
                    if (bestLen >= UnityTuning.Rank56ReelRadius + 3) break;
                }
            if (bestLen < UnityTuning.Rank56ReelRadius + 1) { res[0] = $"RED a256 PLAY: no clear line of {UnityTuning.Rank56ReelRadius + 1} m from any room's spawn on {map}"; yield break; }
            double yawU = Math.Atan2(-u.x, -u.z);
            yield return Equip27(g, "revolver"); // the KESTREL
            g.forcedCombat = IdleCombat;
            string pc = Preconditions(g, f0);
            g.Respawn(new Vec3(f0.x, f0.y + 0.05, f0.z), yawU * 180 / Math.PI);
            for (int i = 0; i < 40; i++) { God(g); g.forcedMove = new InputFrame(); g.input.SetLook(yawU, 0); yield return null; }

            // the husks on an arc ahead (1 hp), held in place
            var arc = new List<(Husk h, Vec3 at)>();
            int n = UnityTuning.Rank56ReelHusks;
            for (int k = 0; k < n; k++)
            {
                double a = yawU + (k - (n - 1) * 0.5) * UnityTuning.Rank56ReelStepDeg * Math.PI / 180;
                double x = f0.x - Math.Sin(a) * UnityTuning.Rank56ReelRadius, z = f0.z - Math.Cos(a) * UnityTuning.Rank56ReelRadius;
                double y = Move54Floor(g.world, x, z, f0.y + 2, out double fy, out _) ? fy : f0.y;
                Husk h = null;
                try { h = sim.husks.Spawn("husk", new Vec3(x, y, z), f0); } catch (Exception) { }
                if (h == null) continue;
                spawned.Add(h);
                h.hp = h.maxHp = 1;
                arc.Add((h, new Vec3(x, y, z)));
            }
            void Hold() { foreach (var p in arc) if (p.h.alive) { p.h.position = p.h.previous = p.at; } }
            for (int i = 0; i < 60; i++) { Hold(); God(g); g.input.SetLook(yawU, 0); yield return null; } // their spawn tells out
            if (arc.Count < 3) { res[0] = $"RED a256 PLAY: only {arc.Count} husks placed on {map}"; yield break; }

            // the meter at DULL, the heal's events
            sim.style.core.DebugSetRank(0);
            var kills = new List<(int rank, double heal, int frame)>();
            double healNow = 0;
            int healRank = 0;
            int frame = 0;
            bool killedNow = false;
            offs.Add(g.events.On<Heal36Event>(Heal36.Event, e => { healNow += e.amount; healRank = sim.style.rank; })); // the rank shown when the heal lands (rank56 reads the same)
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") killedNow = true; }));
            int[] heals0 = (int[])sim.Rank56Heals.Clone();
            Start(g, "a256-rank", "kill", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();

            // ---- the combat ----
            int leftAt = -1, top = 0;
            bool pressedLast = false;
            for (int i = 0; i < 900 && kills.Count < arc.Count; i++)
            {
                Hold();
                // the health under the max before each kill so heal36's heal shows (God's 1e6 would cap nothing: held at Rank56ReelHp of 100)
                g.health.max = 100;
                if (g.health.hp > UnityTuning.Rank56ReelHp) g.health.hp = UnityTuning.Rank56ReelHp;
                Husk target = null;
                foreach (var p in arc) if (p.h.alive) { target = p.h; break; }
                if (target != null) { RideKill53Look(sim.Eye, target.AimPoint, out double yw, out double pt); g.input.SetLook(yw, pt); }
                bool tap = target != null && !pressedLast;
                pressedLast = tap;
                g.forcedCombat = tap ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } } : IdleCombat;
                healRank = sim.style.rank;
                healNow = 0;
                killedNow = false;
                yield return null;
                frame++;
                if (killedNow) kills.Add((healRank, healNow, frame));
                int rk = sim.style.rank;
                if (rk > top) top = rk;
                bool alive = false;
                foreach (var p in arc) if (p.h.alive) alive = true;
                if (leftAt < 0 && rk > 0 && alive) { leftAt = frame; yield return G56Shot(dir, "rank"); }
            }
            g.forcedCombat = IdleCombat;
            yield return G56Shot(dir, "after");
            God(g);
            yield return Frames(5);
            yield return Stop();

            // ---- the numbers ----
            double healDull = -1, healUp = -1;
            int rankUp = -1;
            var line = new StringBuilder();
            for (int k = 0; k < kills.Count; k++)
            {
                var kk = kills[k];
                line.Append($"{(k > 0 ? ", " : "")}kill {k + 1}: {sim.style.Params.ranks[Math.Max(0, Math.Min(5, kk.rank))]} heal {F2(kk.heal)} HP");
                if (kk.rank == 0 && healDull < 0 && kk.heal > 0) healDull = kk.heal;
                if (kk.rank > 0 && kk.rank >= rankUp && kk.heal > 0) { rankUp = kk.rank; healUp = kk.heal; }
            }
            double want = rankUp > 0 ? Rank56.Mult(rankUp, Rank56Params.From(g.cfg)) : double.NaN;
            bool pays = on && healDull > 0 && healUp > 0 && Math.Abs(healUp / healDull - want) < 1e-6;
            bool left = leftAt >= 0;
            var counts = new StringBuilder();
            for (int r = 0; r < 6; r++) counts.Append($"{(r > 0 ? " " : "")}{sim.Rank56Heals[r] - heals0[r]}");
            string input = $"input: kills {kills.Count} of {arc.Count} husks placed (KESTREL, {UnityTuning.Rank56ReelRadius.ToString("0.0", ci)} m: close kills), the meter set to DULL first";
            string nums = $"{line} | the rank left DULL on frame {(left ? leftAt.ToString(ci) : "never")} with a husk alive ({(left ? "shot-N56-G-rank.jpg" : "no frame")}), top rank {sim.style.Params.ranks[top]} | heal at DULL {F2(healDull)} HP, at {(rankUp > 0 ? sim.style.Params.ranks[rankUp] : "-")} {F2(healUp)} HP: x{(healDull > 0 && healUp > 0 ? (healUp / healDull).ToString("0.00", ci) : "-")} (rank56 wants x{(double.IsNaN(want) ? "-" : want.ToString("0.00", ci))}); rank56's heals by rank (DULL..SKYGRIND) {counts}";
            bool ok = on && kills.Count == arc.Count && left && pays;
            string verdict = $"left DULL {(left ? "ok" : "NO")}, pays {(pays ? "ok" : "NO")}, kills {(kills.Count == arc.Count ? "ok" : "NO")}";
            Debug.Log($"[a256] rank56 on {on}; map {map}; {input}; {nums}; {verdict}; {pc}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a256 PLAY | rank56 on {on} (map {map}) | {input} | {nums} | {verdict} | {pc}";
        }
    }
}
