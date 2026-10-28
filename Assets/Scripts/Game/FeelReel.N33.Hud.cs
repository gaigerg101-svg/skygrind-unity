// NIGHT33 builder U: the Player scenario of the HUD row (FeelReel.N27.cs PhaseN33U "hud33"; the body is the partial N33HudBody). Not run by its author
// (no Player in the worktree): the lead runs it.
//   hud33   any map, SURVIVAL (a fresh run), at 1920 x 1080 (recorded) and then 1280 x 720 (stills only), each pass:
//           rest     0.6 s idle; the HUD at rest                                        shot hud33-<res>-rest
//           charge   RT held 0.9 s on the KESTREL (the charge arcs close)              shot hud33-<res>-charge
//           hit/kill a player hit, then a kill, emitted on the bus (view check only)    shots hud33-<res>-hit, -kill (the ticks flashed / turned 45)
//           switch   the THUNDERCLAP / GAUNTLET / NAILGUN given, a switch pressed      shot hud33-<res>-switch (the names list up, the name sliding)
//           slot     three banners in a row (one line at a time)                        shots hud33-<res>-slot-1 / -2
//           boss     THE STAG begun (BossRoomBegin glacier)                              shot hud33-<res>-boss
//           At each shot every visible text on the HUD canvas (effective alpha >= 0.3: CanvasGroups included, so the hidden NIGHT32 layers do not count when
//           the row is on) and HUD33's bars are checked by Hud33Layout.Check (overlap, >= 16 px, 54 px margin, <= 360 px wide except the boss bar and the
//           slot, a label once). The front screens (title / level select / results) come with round 3.
//   PASS: the row on, every shot clean, the reticle's grammar seen (a hit flash, a kill turn, the charge >= 0.9, a switch list). With -skygrindOff33 the
//   same run prints the NIGHT32 HUD's violations (the RED half: the weapon card wider than 360, the LT row's repeated words, ...).
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
        static partial void N33HudBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "hud33") body = Hud33Scenario(g, dir, res, offs, pre);
        }

        /// <summary>The live HUD as checker items (canvas units: 1080 tall): every visible text under the "UI" canvas (shadow twins and "(offset)" copies
        /// skipped), and HUD33's bars and tick rows by their rects.</summary>
        static List<Hud33Layout.Item> Hud33LiveItems(out double cw, bool front = false)
        {
            var items = new List<Hud33Layout.Item>();
            float k = Screen.height > 0 ? 1080f / Screen.height : 1f;
            cw = Screen.width * (double)k;
            GameObject ui = GameObject.Find("UI");
            if (ui == null) return items;
            var corners = new Vector3[4];
            var texts = new List<TextMeshProUGUI>(ui.GetComponentsInChildren<TextMeshProUGUI>(false));
            // NIGHT33 round 3: Hud29's DOORS OPEN tag (its own canvas) is checked with the HUD
            if (Hud29.TagTextNow != null && Hud29.TagTextNow.gameObject.activeInHierarchy && Hud29.TagTextNow.canvas != null && Hud29.TagTextNow.canvas.enabled) texts.Add(Hud29.TagTextNow);
            foreach (TextMeshProUGUI t in texts)
            {
                // a label's shadow twin, an "(offset)" copy and the wordmark's hero shadow ("Wordmark (back)": shadow-coloured with hud33) are one label with it
                if (t.name == "Shadow" || t.name.EndsWith("(offset)", StringComparison.Ordinal) || t.name.EndsWith("(back)", StringComparison.Ordinal) || string.IsNullOrEmpty(t.text)) continue;
                float a = t.color.a;
                for (Transform p = t.transform; p != null && a > 0; p = p.parent)
                {
                    CanvasGroup cg = p.GetComponent<CanvasGroup>();
                    if (cg != null) { a *= cg.alpha; if (cg.ignoreParentGroups) break; }
                }
                if (a < 0.3f) continue;
                t.ForceMeshUpdate();
                Bounds b = t.textBounds;
                if (b.size.x <= 0 || b.size.y <= 0) continue;
                Vector3 lo = t.rectTransform.TransformPoint(b.min), hi = t.rectTransform.TransformPoint(b.max);
                string name = t.name == "Text" && t.transform.parent != null ? t.transform.parent.name : t.name;
                if (front && t.transform.parent != null) name = t.transform.parent.name + "/" + t.name; // menus: many items' texts share a name ("Label")
                items.Add(new Hud33Layout.Item
                {
                    name = name, text = t.text, px = t.fontSize,
                    x = Math.Min(lo.x, hi.x) * k, y = (Screen.height - Math.Max(lo.y, hi.y)) * k, w = Math.Abs(hi.x - lo.x) * k, h = Math.Abs(hi.y - lo.y) * k,
                    slot = name == Hud33Layout.SlotName,
                    bossBar = name == Hud33Layout.BossNameName,
                    world = name.StartsWith("Door marker", StringComparison.Ordinal),
                });
            }
            if (Ui.Hud33Root != null && Ui.Hud33Live)
                foreach (string n in new[] { "Health ticks", "Stamina", "Weapon pips", "Rank line", "Boss/" + Hud33Layout.BossBarName })
                {
                    Transform tr = Ui.Hud33Root.Find(n);
                    if (tr == null || !tr.gameObject.activeInHierarchy) continue;
                    ((RectTransform)tr).GetWorldCorners(corners);
                    items.Add(new Hud33Layout.Item
                    {
                        name = n, x = corners[0].x * k, y = (Screen.height - corners[1].y) * k, w = (corners[2].x - corners[0].x) * k, h = (corners[1].y - corners[0].y) * k,
                        bossBar = n.EndsWith(Hud33Layout.BossBarName, StringComparison.Ordinal),
                    });
                }
            return items;
        }

        /// <summary>One shot and its check line ("name: clean" or "name: N problem(s): ...").</summary>
        static IEnumerator Hud33Shot(string dir, string name, List<string> lines, int[] bad, bool front = false)
        {
            yield return Shot30(dir, name);
            List<Hud33Layout.Item> items = Hud33LiveItems(out double cw, front);
            List<string> found = Hud33Layout.Check(items, cw, 1080);
            // a menu screen: its rows are wider than the HUD's 360 px and repeat column values ("-"): the overlap, text size and margin rules only
            if (front) found.RemoveAll(s => s.StartsWith("wide:", StringComparison.Ordinal) || s.StartsWith("tall:", StringComparison.Ordinal) || s.StartsWith("repeated label:", StringComparison.Ordinal));
            if (found.Count > 0) bad[0] += found.Count;
            int texts = 0;
            foreach (Hud33Layout.Item it in items) if (!string.IsNullOrEmpty(it.text)) texts++;
            lines.Add(found.Count == 0 ? $"{name}: clean ({texts} texts, f{FeelProbe.FrameCount:0000})" : $"{name}: {found.Count} problem(s) ({texts} texts, f{FeelProbe.FrameCount:0000}): {string.Join(" ;; ", found.GetRange(0, Math.Min(6, found.Count)))}");
        }

        /// <summary>Waits `seconds` of SIM time (god mode on), calling `each` per frame, ending early when `until` holds. The 720 pass is not recorded
        /// (no FeelClock: frames run at the display rate), so frame counts there were a fraction of the time they are in the recorded 1080 pass (the lead's
        /// n33-r1: charge / kill / switch missed at 720); sim time is the same in both. A 10 s wall guard.</summary>
        static IEnumerator Sim33(SurvivalGame g, double seconds, Action each = null, Func<bool> until = null)
        {
            double t0 = g.sim.SimTime;
            float wall = Time.realtimeSinceStartup;
            while (g.sim.SimTime - t0 < seconds && Time.realtimeSinceStartup - wall < 10f)
            {
                God(g);
                yield return null;
                each?.Invoke();
                if (until != null && until()) break;
            }
        }

        static IEnumerator Hud33Pass(SurvivalGame g, string dir, string tag, List<string> lines, int[] bad, int[] seen)
        {
            SurvivalSim sim = g.sim;
            // rest
            g.forcedCombat = IdleCombat;
            yield return Sim33(g, 0.6);
            yield return Hud33Shot(dir, $"hud33-{tag}-rest", lines, bad);
            lines.Add($"rest {tag}: run state {g.run.state}, the slot '{Ui.Hud33SlotNow}' (a break shows its countdown here)");
            // charge: RT held on the KESTREL
            int rev = g.loadout.owned.IndexOf("revolver");
            if (rev >= 0)
            {
                g.forcedCombat = new CombatInput { weapon = rev };
                yield return Frames(3);
            }
            g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { held = true, pressed = true } };
            yield return null;
            g.forcedCombat = new CombatInput { weapon = -1, fire = new Btn { held = true, pressed = false } };
            float maxCharge = 0;
            yield return Sim33(g, 1.0, () => maxCharge = Math.Max(maxCharge, Ui.Hud33ChargeNow));
            if (maxCharge >= 0.9f) seen[0]++;
            yield return Hud33Shot(dir, $"hud33-{tag}-charge", lines, bad);
            g.forcedCombat = IdleCombat;
            yield return Sim33(g, 0.3);
            // a hit, then a kill (emitted: the view's grammar, not the sim's)
            int hits0 = Ui.Hud33HitFlashes, kills0 = Ui.Hud33KillTurns;
            g.events.Emit("hit", new HitEvent { owner = "player", weapon = "revolver", kind = "husk", damage = 1 });
            yield return null;
            yield return Hud33Shot(dir, $"hud33-{tag}-hit", lines, bad);
            yield return Sim33(g, 0.3);
            g.events.Emit("kill", new KillEvent { owner = "player", weapon = "revolver", kind = "husk" });
            float maxTurn = 0;
            // the turn runs on the HUD's frame time: wait (sim time, so the unrecorded 720 pass waits as long as the recorded one) until it stands at 45
            yield return Sim33(g, 0.16, () => maxTurn = Math.Max(maxTurn, Ui.Hud33KillAngleNow), () => Ui.Hud33KillAngleNow >= 44f);
            yield return Hud33Shot(dir, $"hud33-{tag}-kill", lines, bad);
            if (Ui.Hud33HitFlashes > hits0) seen[1]++;
            if (Ui.Hud33KillTurns > kills0 && maxTurn >= 40f) seen[2]++;
            yield return Sim33(g, 0.4);
            // a switch with four guns owned
            foreach (string w in new[] { "shotgun", "gauntlet", "nailgun" })
                if (g.loadout.owned.IndexOf(w) < 0 && g.cfg.Has($"weapons.catalog.{w}")) g.loadout.AddWeapon(w);
            yield return Sim33(g, 0.2);
            int sg = g.loadout.owned.IndexOf("shotgun");
            int sw0 = Ui.Hud33Switches;
            g.forcedCombat = new CombatInput { weapon = sg >= 0 ? sg : 1 };
            yield return Frames(2);
            g.forcedCombat = IdleCombat;
            bool listSeen = false;
            yield return Sim33(g, 0.15, () => listSeen |= Ui.Hud33SwitchListShown);
            yield return Hud33Shot(dir, $"hud33-{tag}-switch", lines, bad);
            if (Ui.Hud33Switches > sw0 && listSeen) seen[3]++;
            yield return Sim33(g, 0.9);
            yield return Hud33Shot(dir, $"hud33-{tag}-switched", lines, bad);
            // the DOORS OPEN tag (Hud29, forced by its reel switch; the chase gap bar comes with it and the slot drops under that)
            Hud29.ReelForce = true;
            yield return Sim33(g, 0.4);
            yield return Hud33Shot(dir, $"hud33-{tag}-doors-tag", lines, bad);
            Hud29.ReelForce = false;
            Hud29.BossGapMetres = -1;
            // the message slot: a lesson line asked for and three banners in the same frame (A3's pile-up: before NIGHT33 they drew on top of each other).
            // The slot must show them one at a time, the banners (room titles) first, each held >= 1.2 s, then the lesson.
            Ui.Hud33SlotMinHeldSeen = double.MaxValue;
            g.UiView?.ShowLesson25("{jump} TO CLIMB");
            g.events.Emit("banner", new BannerEvent { text = "DOOR OPEN" });
            g.events.Emit("banner", new BannerEvent { text = "1.3 DASH" });
            g.events.Emit("banner", new BannerEvent { text = "HOLD THE LINE" });
            var seq = new List<string>();
            string lastLine = null;
            double tSeq = sim.SimTime;
            float wallSeq = Time.realtimeSinceStartup;
            bool shot1 = false, shot2 = false;
            int maxTop = 0;
            while (sim.SimTime - tSeq < 9.0 && Time.realtimeSinceStartup - wallSeq < 30f)
            {
                God(g);
                yield return null;
                string now = Ui.Hud33SlotNow ?? "";
                if (now != lastLine)
                {
                    lastLine = now;
                    if (seq.Count < 12) seq.Add($"{(sim.SimTime - tSeq).ToString("0.00", CultureInfo.InvariantCulture)}:{(now.Length > 0 ? now : "-")}");
                }
                maxTop = Math.Max(maxTop, TopCentreTexts33());
                double t = sim.SimTime - tSeq;
                if (!shot1 && t > 0.5) { shot1 = true; yield return Hud33Shot(dir, $"hud33-{tag}-slot-1", lines, bad); }
                else if (!shot2 && t > 3.0) { shot2 = true; yield return Hud33Shot(dir, $"hud33-{tag}-slot-2", lines, bad); }
            }
            yield return Hud33Shot(dir, $"hud33-{tag}-slot-3", lines, bad);
            g.UiView?.HideLesson25();
            double minHeld = Ui.Hud33SlotMinHeldSeen;
            lines.Add($"slot sequence {tag}: [{string.Join(" ", seq)}], shortest hold before a replace {(minHeld < double.MaxValue ? minHeld.ToString("0.00", CultureInfo.InvariantCulture) + " s" : "none")}, most top-centre texts in one frame {maxTop}, posts dropped {Ui.Hud33SlotDropped}");
            if (seq.Count >= 3 && maxTop <= 1 && (minHeld == double.MaxValue || minHeld >= UnityTuning.Hud33SlotMinHold - 0.05)) seen[4]++;
            // a boss fight: the bar and its name, then its teaching line in the slot under the bar
            if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            sim.flow = Flow.Playing;
            if (sim.BossRoomBegin("glacier", out Vec3 sp, out double syaw) && sim.stag31 != null && sim.stag31.Alive)
            {
                g.Respawn(sp, syaw);
                yield return Sim33(g, 1.5);
                yield return Hud33Shot(dir, $"hud33-{tag}-boss", lines, bad);
                bool bar = Ui.Hud33BossBarShown && Ui.Hud33BossNameNow.Length > 0;
                g.events.Emit("lessonBegin", new LessonEvent { id = "boss", prompt = "WAIT FOR THE CHARGE" });
                yield return Sim33(g, 0.7);
                bool lineUnder = Ui.Hud33SlotPrioNow == MessageSlot33.Boss;
                yield return Hud33Shot(dir, $"hud33-{tag}-boss-line", lines, bad);
                lines.Add($"boss {tag}: bar {(bar ? "shown '" + Ui.Hud33BossNameNow + "'" : "NOT shown")}, its line in the slot (boss channel) {lineUnder} ('{Ui.Hud33SlotNow}')");
                if (bar) seen[5]++;
                g.UiView?.HideLesson25();
                if (sim.stag31 != null && sim.stag31.Alive) sim.stag31.Clear();
            }
            else lines.Add($"hud33-{tag}-boss: not shot (BossRoomBegin(glacier) did not start THE STAG)");
        }

        /// <summary>Waits `seconds` of real time (a menu: the sim does not step under TITLE / RESULTS).</summary>
        static IEnumerator Real33(double seconds)
        {
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < seconds) yield return null;
        }

        /// <summary>The front screens and the campaign's door, one pass (seen: [0] campaign door marker, [1] campaign results with the banked line,
        /// [2] TITLE drawn, [3] LEVEL SELECT opened). The campaign part needs a campaign level booted (-skygrindMap n33-foundry).</summary>
        static IEnumerator Front33Pass(SurvivalGame g, string dir, string tag, List<string> lines, int[] bad, int[] seen, List<Action> offs)
        {
            if (N24CampaignGate(g) == null)
            {
                yield return StartCampaign30(g, offs);
                SurvivalSim sim = g.sim;
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c != null && lv != null && lv.rooms.Count > 0)
                {
                    // the door: room 1 cleared (as the `lit` reel does), the player at its spawn looking at its exit
                    RoomDef room = lv.rooms[0];
                    c.room = 0;
                    c.phase = Campaign24.Phase.Cleared;
                    g.Respawn(room.spawn, room.spawnYawDeg);
                    var exitAt = new Vec3(room.exit.x, room.exit.y + UnityTuning.Hud33DoorMarkUp, room.exit.z);
                    bool marked = false;
                    yield return Sim33(g, 1.2, () =>
                    {
                        var (yw, pt) = LookAt31(g, exitAt);
                        g.input.SetLook(yw, pt);
                        marked |= Ui.Hud33DoorMarkShown;
                    });
                    yield return Hud33Shot(dir, $"hud33-{tag}-camp-door", lines, bad);
                    lines.Add($"campaign {tag} ({lv.id}): the door marker {(Ui.Hud33DoorMarkShown ? "drawn '" + Ui.Hud33DoorMetres + " m'" : "NOT drawn")}, the slot '{Ui.Hud33SlotNow}'");
                    if (marked) seen[0]++;
                    // the campaign results (the level marked finished: the screen, not the run, is checked); POINTS BANKED: P's number, or a stand-in
                    // 1234 when the shop row banked nothing yet (said in the line)
                    bool stand = Shop33.LastBanked < 0;
                    if (stand) Shop33.LastBanked = 1234;
                    c.finished = true;
                    g.flow = Flow.Results;
                    yield return Real33(UnityTuning.CampRes25ButtonsAt + 1.0);
                    yield return Hud33Shot(dir, $"hud33-{tag}-camp-results", lines, bad, true);
                    lines.Add($"campaign results {tag}: drawn {Ui.CampRes25Open}, '{Ui.Front33BankedNow}'{(stand ? " (LastBanked stand-in 1234: the shop row banked nothing)" : "")}, focus '{Ui.Front33FocusNow}'");
                    if (Ui.CampRes25Open && Ui.Front33BankedNow.Length > 0) seen[1]++;
                    if (stand) Shop33.LastBanked = -1;
                }
                else lines.Add($"campaign {tag}: not run (no campaign level or room)");
            }
            else lines.Add($"campaign {tag}: not run (boot -skygrindMap n33-foundry): {N24CampaignGate(g)}");
            // TITLE
            g.ToTitle();
            yield return Real33(1.2);
            yield return Hud33Shot(dir, $"hud33-{tag}-title", lines, bad, true);
            lines.Add($"title {tag}: focus '{Ui.Front33FocusNow}', front33 {Ui.Front33On}");
            if (g.flow == Flow.Title) seen[2]++;
            // LEVEL SELECT (PLAY on CAMPAIGN)
            string mode0 = g.setup.mode;
            g.setup.mode = RunSetup.Campaign;
            bool pressed = g.UiView != null && g.UiView.ReelPressPlay26();
            yield return Real33(1.2);
            yield return Hud33Shot(dir, $"hud33-{tag}-levels", lines, bad, true);
            lines.Add($"level select {tag}: pressed {pressed}, open {Ui.LevelSel26Open}, focus '{Ui.Front33FocusNow}'");
            if (Ui.LevelSel26Open) seen[3]++;
            g.UiView?.CloseMeta();
            g.setup.mode = mode0;
            yield return Real33(0.3);
        }

        /// <summary>Visible texts whose centre is in the top-centre column (the slot's place): 1 at most when the slot works.</summary>
        static int TopCentreTexts33()
        {
            int n = 0;
            List<Hud33Layout.Item> items = Hud33LiveItems(out double cw);
            foreach (Hud33Layout.Item it in items)
            {
                if (string.IsNullOrEmpty(it.text) || it.world || it.name == Hud33Layout.BossNameName) continue;
                double cx = it.x + it.w / 2, cy = it.y + it.h / 2;
                if (Math.Abs(cx - cw / 2) < UnityTuning.Hud33SlotW / 2 && cy < 300) n++;
            }
            return n;
        }

        static IEnumerator Hud33Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            bool row = N33Fixes.ViewRowOn(g.cfg, "hud33");
            int w0 = Screen.width, h0 = Screen.height;
            FullScreenMode mode0 = Screen.fullScreenMode;
            offs.Add(() => Quiet(g));
            offs.Add(() => { if (Screen.width != w0 || Screen.height != h0) Screen.SetResolution(w0, h0, mode0); });
            offs.Add(() => { Hud29.ReelForce = false; Hud29.BossGapMetres = -1; });
            string setup0 = g.setup.mode;
            offs.Add(() => g.setup.mode = setup0);
            g.setup.mode = RunSetup.Survival; // the HUD passes on a SURVIVAL run (its rounds, countdown and bosses); the front pass makes its own campaign run
            Fresh(g);
            g.husks.frozen = true;
            string pc = Preconditions(g, g.Feet) + $" | hud33 row {row}, screen {w0}x{h0} {mode0}, map {g.MapId}";
            var lines = new List<string>();
            int[] bad = { 0 }, seen = { 0, 0, 0, 0, 0, 0 }, front = { 0, 0, 0, 0 };
            // 1080p, recorded
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && Screen.height != 1080; i++) yield return null;
            string r1 = $"{Screen.width}x{Screen.height}";
            Start(g, "hud33", "ui", dir);
            yield return Frames(pre);
            yield return Hud33Pass(g, dir, "1080", lines, bad, seen);
            yield return Stop();
            yield return Front33Pass(g, dir, "1080", lines, bad, front, offs);
            // 720p, stills
            g.setup.mode = RunSetup.Survival;
            Fresh(g);
            g.husks.frozen = true;
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            for (int i = 0; i < 60 && Screen.height != 720; i++) yield return null;
            string r2 = $"{Screen.width}x{Screen.height}";
            yield return Frames(10);
            yield return Hud33Pass(g, dir, "720", lines, bad, seen);
            yield return Front33Pass(g, dir, "720", lines, bad, front, offs);
            Screen.SetResolution(w0, h0, mode0);
            yield return Frames(10);
            g.setup.mode = setup0;
            Quiet(g);
            bool grammar = seen[0] == 2 && seen[1] == 2 && seen[2] == 2 && seen[3] == 2 && seen[4] == 2 && seen[5] == 2;
            bool fronts = front[0] == 2 && front[1] == 2 && front[2] == 2 && front[3] == 2;
            bool ok = row && bad[0] == 0 && grammar && fronts && r1 == "1920x1080" && r2 == "1280x720";
            res[0] = $"{(ok ? "PASS" : "RED")}: {(row ? "hud33 ON" : "hud33 OFF (-skygrindOff33 or the row off: the NIGHT32 HUD, the RED half)")}; passes {r1} and {r2}; " +
                     $"HUD / screen rule problems {bad[0]}; per pass, of 2: charge >= 0.9 {seen[0]}, hit flash {seen[1]}, kill turn >= 40 deg {seen[2]}, switch list {seen[3]}, slot one-at-a-time and held {seen[4]}, boss bar {seen[5]}, " +
                     $"campaign door marker {front[0]}, campaign results + POINTS BANKED {front[1]}, title {front[2]}, level select {front[3]} | " +
                     string.Join(" | ", lines) + " | shots shot-N30-hud33-<1080|720>-<rest|charge|hit|kill|switch|switched|doors-tag|slot-1|slot-2|slot-3|boss|boss-line|camp-door|camp-results|title|levels>.png | " + pc;
        }
    }
}
