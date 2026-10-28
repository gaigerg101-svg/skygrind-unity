// NIGHT43 builder U1: the Player scenario `ui43` (NIGHT43.md U1's proof): the health bar (hud43), the counter (counter43), the centre card (teach43),
// the boss bar's states (bossbar43), the campaign results and the shop over the blurred game (screens43). The dispatch: FeelReel.N43.cs ("ui43").
// Not run by its author (no Player in the worktree): the lead runs it.
// Boot: reel.sh + -skygrindMap <a campaign level with a next level, e.g. n42-foundry> + -skygrindReelOnly ui43 (L-062). BEFORE: the same with the
// rows off (or -skygrindOff43); AFTER: hud43 / counter43 / teach43 / bossbar43 / screens43 on.
// Setup: a CAMPAIGN run (N24StartCampaign; the shop forced on in memory only, as shop33's reel: Shop33NoSave, the wallet emptied), the player kept at
// SafeSpot (the autopilot's goal is the spot), husks killed and frozen, God mode except in the health part.
// The counter and the boss bar are driven through their reel-only inputs (Counter43Probe.Force, Bar43Probe.ForceState): these are the VIEW's frames of
// every state; the real counts / states are E1's rooms43 reel (a locked room to its door) and B1's wick43 reel (THE WICK).
// Frames (shot-N30-<name>.png), each with its probe line:
//   U43-h-rest, h-hit (+0.03 s after a 30 % hit: the HOT after-image, the split, the bracket red-orange), h-after (+0.12: still held), h-drain (+0.7:
//   drained; the rule clears a 30 % chunk at ~0.43 s), h-heal (+0.05 after a 25 % heal: the cyan glow), h-low (20 % health +0.4: red-orange, breathing)
//   U43-c-3 (+0.3 after it opens), c-2 / c-1 (+0.03 after a tick: the punch), c-0 (CLEAR), c-close (+0.05 after -1), c-gone (+0.3)
//   U43-t-open (+0.08 s: mid-pop), t-hold (+0.7), t-close (+0.05 after its time: closing)
//   U43-b-norm, b-hit (+0.05 after a 8 % hit: the after-image chunk), b-locked, b-open (+0.5: breathing, the timer), b-leg (+0.05 after leg 2: the punch)
//   U43-s-res0 (+0.06 s after RESULTS open: the entrance mid-way over the blur), s-res (+1.2: settled), s-shop0 (+0.06 after the shop opens), s-shop
//   (+0.6: settled), s-shop-sel (the focus moved right +0.1)
//   round 3 (the menus): U43-m-pause0 (+0.06 s after PAUSED: the blur in, the entrance), m-pause (+0.5), m-pause-sel (SETTINGS focused: the 1.08 grow,
//   the plate, the cyan edge), m-title (+1.2 after TITLE), m-levels (+1.2 after PLAY on CAMPAIGN: the level select's panel and rows)
// GREEN (rows on): every frame saved and each row's own check (the line names it); with every row off the frames are the BEFORE (GREEN when saved).
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
        static partial void N43Ui(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Ui43Scenario(g, dir, res, offs, pre);

        static IEnumerator Ui43Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            bool hudRow = N43View.RowOn(g.cfg, "hud43"), counterRow = N43View.RowOn(g.cfg, "counter43"), teachRow = N43View.RowOn(g.cfg, "teach43"),
                barRow = N43View.RowOn(g.cfg, "bossbar43"), screensRow = N43View.RowOn(g.cfg, "screens43");
            bool any = hudRow || counterRow || teachRow || barRow || screensRow;
            string why = N24CampaignGate(g);
            bool campaign = why == null;
            var lines = new List<string>();
            var fails = new List<string>();
            int saved = 0, want = 0;
            double wallEnd = Time.realtimeSinceStartup + 200;

            // the shop in memory only (shop33's reel's setup)
            SurvivalGame.Shop33NoSave = true;
            SurvivalGame.Shop33ReelForce = true;
            SurvivalGame.SkipSceneReload25 = true;
            N33Blocks? n33Was = g.sim.n33Override;
            N33Blocks n33 = N33Blocks.ForceOff ? N33Blocks.Off : N33Blocks.From(g.cfg);
            if (!N33Blocks.ForceOff) n33.shop33 = true;
            g.sim.n33Override = n33;
            offs.Add(() =>
            {
                SurvivalGame.Shop33NoSave = false;
                SurvivalGame.Shop33ReelForce = false;
                SurvivalGame.SkipSceneReload25 = false;
                SurvivalGame.PendingMapId = null;
                g.sim.n33Override = n33Was;
                if (g.Shop33Ui != null) g.Shop33Ui.Close();
                Counter43Probe.Force = int.MinValue;
                Bar43Probe.ForceOn = false;
                g.husks.frozen = false;
            });
            if (g.Shop33Store != null)
            {
                g.Shop33Store.bank33 = 0;
                g.Shop33Store.shop33 = 0;
                g.Shop33Store.banked33.Clear();
            }

            AutopilotInput ap = campaign ? N24StartCampaign(g, offs) : null;
            yield return Frames(30);
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            if (ap != null) ap.GoalProvider = () => spot; // the bot stands on the spot
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId}" + (campaign ? "" : " | " + why);
            double maxHp = g.health.max > 0 && g.health.max < 1e5 ? g.health.max : 100;
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "ui43", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            bool god = true;
            Action keep = () => { if (god) God(g); };
            IEnumerator Wait(double s) => Wait27(g, s, wallEnd, keep);
            IEnumerator Shot(string name, string line)
            {
                want++;
                lines.Add(name + ": " + line);
                yield return Shot30(dir, name);
                saved++;
            }
            string HudLine() => Hud43.Live
                ? $"hp {g.health.hp.ToString("0", ci)}/{g.health.max.ToString("0", ci)} view {Hud43.NumberShown} cells {Hud43.LitCells} after {Hud43.AfterCells} glow {Hud43.GlowCells} split {Hud43.SplitNow.ToString("0.00", ci)} afterLv {Hud43.After.ToString("0.00", ci)}"
                : $"hp {g.health.hp.ToString("0", ci)} (hud43 off)";
            string CounterLine() => $"force {Counter43Probe.Force} drawn {Counter43Probe.Drawn} shown {Counter43Probe.Shown} ticks {Counter43Probe.Ticks} mismatch {Counter43Probe.Mismatch}";
            string TeachLine() => $"card {Teach43Probe.Shown} '{Teach43Probe.Head}' / '{Teach43Probe.Sub}' open {Teach43Probe.OpenSeconds.ToString("0.000", ci)} s, bottom {Teach43Probe.BottomAboveCentre.ToString("0", ci)} px over the centre, w {Teach43Probe.Width.ToString("0", ci)} | text36 slot '{Ui.Hud36TeachNow}'";
            string BarLine() => $"bar43 {Bar43Probe.Shown} {Bar43Probe.Mode} fill {Bar43Probe.Fill.ToString("0.000", ci)} after {Bar43Probe.After.ToString("0.000", ci)} tag '{Bar43Probe.Tag}' leg {Bar43Probe.Leg}/{Bar43Probe.Legs} stagger {Bar43Probe.Stagger.ToString("0.00", ci)} hint '{Bar43Probe.Hint}' | bars41 {BossBar41Probe.Shown}";

            // ---- hud43: a hit, the after-image, a heal, low ----
            god = false;
            g.health.max = maxHp;
            g.health.hp = maxHp;
            yield return Wait(0.6);
            Hud43.ResetCounters();
            yield return Shot("U43-h-rest", HudLine());
            g.health.Damage(maxHp * 0.3, "reel");
            yield return Wait(0.03);
            int afterAtHit = Hud43.AfterCells;
            yield return Shot("U43-h-hit", HudLine());
            yield return Wait(0.09);
            int afterHeld = Hud43.AfterCells;
            yield return Shot("U43-h-after", HudLine());
            yield return Wait(0.58); // round 2: the drain rule (0.15 s wait, then gap x 5 + 0.5 per s) clears a 30 % chunk at ~0.43 s: +0.4 was early (n43-r1: 1 cell left)
            int afterGone = Hud43.AfterCells;
            yield return Shot("U43-h-drain", HudLine());
            g.health.Heal(maxHp * 0.25);
            yield return Wait(0.05);
            int glow = Hud43.GlowCells;
            yield return Shot("U43-h-heal", HudLine());
            g.health.hp = maxHp * 0.2;
            yield return Wait(0.4);
            yield return Shot("U43-h-low", HudLine());
            g.health.hp = maxHp;
            // round 2: E1's fallReturn43 blink (the event by name, as the campaign emits it; the real fall is E1's / L1's GANTRY frames)
            int fades0 = Fall43Probe.Fades;
            yield return Wait(0.4);
            // E1's payload type when this build has it (another listener may read its fields), else none
            Type fallType = typeof(Campaign24).Assembly.GetType("Skygrind.Core.FallReturn43Event");
            g.events.Emit("fallReturn43", fallType != null ? Activator.CreateInstance(fallType) : null);
            yield return Wait(0.12);
            float blink = Fall43Probe.AlphaNow;
            yield return Shot("U43-f-blink", HudLine() + $" | fall blink alpha {blink.ToString("0.00", ci)}");
            yield return Wait(0.5);
            float back = Fall43Probe.AlphaNow;
            yield return Shot("U43-f-back", HudLine() + $" | fall blink alpha {back.ToString("0.00", ci)}");
            bool fallOk = !hudRow || (Fall43Probe.Fades > fades0 && blink > 0.5f && back == 0f);
            lines.Add($"fall blink (hud43 row {hudRow}): fades {Fall43Probe.Fades - fades0}, alpha +0.12 {blink.ToString("0.00", ci)} / +0.62 {back.ToString("0.00", ci)} -> {(fallOk ? "ok" : "NOT ok")}");
            if (!fallOk) fails.Add("fall blink");
            god = true;
            bool hudOk = !hudRow || (Hud43.Live && Hud43.Hits >= 1 && afterAtHit > 0 && afterHeld > 0 && afterGone == 0 && Hud43.Heals >= 1 && glow > 0);
            lines.Add($"hud43 row {hudRow}: hits {Hud43.Hits}, after cells +0.03 {afterAtHit} / +0.12 {afterHeld} / +0.7 {afterGone}, heals {Hud43.Heals}, glow {glow} -> {(hudOk ? "ok" : "NOT ok")}");
            if (!hudOk) fails.Add("hud43");

            // ---- round 3: the style meter as blade cells (hud43 + style42): a chain of named bonuses through the bus (ui42's path) ----
            for (int b = 0; b < 6 && Time.realtimeSinceStartup < wallEnd; b++)
            {
                g.events.Emit("styleBonus", new StyleBonusEvent { owner = "player", name = Ui42Bonus[b % Ui42Bonus.Length], points = 40 });
                yield return Wait(0.25);
            }
            yield return Shot("U43-st-meter", $"style {(g.style != null ? g.style.rank : -1)} '{Style42.Letter}' line {Style42.LineShown.ToString("0.00", ci)} x{Style42.Combo} | points gold (hud43 {hudRow})");

            // ---- counter43: 3 -> 2 -> 1 -> 0 -> gone ----
            Counter43Probe.ResetCounters();
            Counter43Probe.Force = 3;
            yield return Wait(0.3);
            yield return Shot("U43-c-3", CounterLine());
            for (int n = 2; n >= 0; n--)
            {
                Counter43Probe.Force = n;
                yield return Wait(0.03);
                yield return Shot("U43-c-" + n, CounterLine());
                yield return Wait(0.4);
            }
            Counter43Probe.Force = -1;
            yield return Wait(0.05);
            yield return Shot("U43-c-close", CounterLine());
            yield return Wait(0.3);
            bool gone = !Counter43Probe.Shown;
            yield return Shot("U43-c-gone", CounterLine());
            Counter43Probe.Force = int.MinValue;
            bool counterOk = !counterRow || (Counter43Probe.Opens >= 1 && Counter43Probe.Ticks >= 3 && Counter43Probe.Mismatch == 0 && gone);
            lines.Add($"counter43 row {counterRow}: opens {Counter43Probe.Opens}, ticks {Counter43Probe.Ticks}, mismatch frames {Counter43Probe.Mismatch}, gone at -1 {gone} -> {(counterOk ? "ok" : "NOT ok")}");
            if (!counterOk) fails.Add("counter43");

            // ---- teach43: one teaching line through teach35's path (the bus) ----
            int opens0 = Teach43Probe.Opens;
            g.events.Emit("teach35", new Teach35Event { id = "ui43", trigger = "ui43", seconds = 3, text = "HOLD {jump} ON A WALL. Ride it, then let go to jump off." });
            double tw = 0;
            while (!Teach43Probe.Shown && !(Ui.Hud36TeachNow.Length > 0 && !teachRow) && tw < 3 && Time.realtimeSinceStartup < wallEnd) { keep(); yield return null; tw += Time.unscaledDeltaTime; }
            yield return Wait(0.08);
            yield return Shot("U43-t-open", TeachLine());
            yield return Wait(0.62);
            yield return Shot("U43-t-hold", TeachLine());
            double tw2 = 0;
            while (Teach43Probe.Closes == 0 && teachRow && tw2 < 4 && Time.realtimeSinceStartup < wallEnd) { keep(); yield return null; tw2 += Time.unscaledDeltaTime; }
            yield return Wait(0.05);
            yield return Shot("U43-t-close", TeachLine());
            yield return Wait(0.4);
            double openS = Teach43Probe.OpenSeconds;
            bool teachOk = !teachRow || (Teach43Probe.Opens > opens0 && openS > 0.1 && openS < 0.25 && Teach43Probe.BottomAboveCentre >= 80);
            lines.Add($"teach43 row {teachRow}: opens {Teach43Probe.Opens - opens0}, open {openS.ToString("0.000", ci)} s (0.10-0.25), bottom {Teach43Probe.BottomAboveCentre.ToString("0", ci)} px over the crosshair (>= 80), closes {Teach43Probe.Closes} -> {(teachOk ? "ok" : "NOT ok")}");
            if (!teachOk) fails.Add("teach43");

            // ---- bossbar43: each state ----
            var st = new Boss43State { valid = true, name = "THE WICK", hp01 = 0.82, lockAt = 0.6, stagger01 = 0.35, leg = 1, legs = 3, hint = "CRACK A PLATE: RETURN AN EMBER" };
            Bar43Probe.ForceState = st;
            Bar43Probe.ForceOn = true;
            int legs0 = Bar43Probe.LegChanges;
            var modes = new HashSet<Ui43Math.BossMode>();
            yield return Wait(0.4);
            modes.Add(Bar43Probe.Mode);
            yield return Shot("U43-b-norm", BarLine());
            st.hp01 = 0.74;
            Bar43Probe.ForceState = st;
            yield return Wait(0.05);
            bool chunk = Bar43Probe.After > Bar43Probe.Fill + 0.01f;
            yield return Shot("U43-b-hit", BarLine());
            yield return Wait(0.6);
            st.hp01 = 0.6;
            st.locked = true;
            st.hint = "RIDE, HOLD {fire}: SHOOT THE CRACK";
            Bar43Probe.ForceState = st;
            yield return Wait(0.4);
            modes.Add(Bar43Probe.Mode);
            yield return Shot("U43-b-locked", BarLine());
            st.open = true;
            st.openLeft = 2.0;
            double t0 = g.sim.SimTime;
            while (g.sim.SimTime - t0 < 0.5 && Time.realtimeSinceStartup < wallEnd)
            {
                st.openLeft = 2.0 - (g.sim.SimTime - t0);
                Bar43Probe.ForceState = st;
                keep();
                yield return null;
            }
            modes.Add(Bar43Probe.Mode);
            yield return Shot("U43-b-open", BarLine());
            st.open = st.locked = false;
            st.leg = 2;
            st.hp01 = 0.58;
            st.lockAt = 0.3;
            st.hint = "CRACK A PLATE: RETURN AN EMBER";
            Bar43Probe.ForceState = st;
            yield return Wait(0.05);
            yield return Shot("U43-b-leg", BarLine());
            yield return Wait(0.3);
            Bar43Probe.ForceOn = false;
            bool barOk = !barRow || (modes.Contains(Ui43Math.BossMode.Normal) && modes.Contains(Ui43Math.BossMode.Locked) && modes.Contains(Ui43Math.BossMode.Open) && chunk && Bar43Probe.LegChanges > legs0);
            lines.Add($"bossbar43 row {barRow}: modes {string.Join("/", modes)}, after-image chunk at +0.05 {chunk}, leg changes {Bar43Probe.LegChanges - legs0} -> {(barOk ? "ok" : "NOT ok")}");
            if (!barOk) fails.Add("bossbar43");

            // ---- round 3: the menus under screens43: PAUSED (its blur, its panel, the focused item's grow and edge) ----
            string MenuLine() => $"menus43 built {Menus43Probe.Built}, pause backdrop {Menus43Probe.PauseBackdrop}, opens {Menus43Probe.PauseOpens}, focus '{Menus43Probe.FocusNow}' scale {Menus43Probe.FocusScale.ToString("0.00", ci)} | flow {g.flow}";
            int pOpens0 = Menus43Probe.PauseOpens;
            g.Pause();
            yield return Frames(Math.Max(1, Ms(60)));
            yield return Shot("U43-m-pause0", MenuLine());
            yield return Frames(Ms(500));
            yield return Shot("U43-m-pause", MenuLine());
            bool moved = g.UiView != null && g.UiView.Menus43Select("SETTINGS");
            yield return Frames(Ms(300));
            float pauseScale = Menus43Probe.FocusScale;
            string pauseFocus = Menus43Probe.FocusNow;
            yield return Shot("U43-m-pause-sel", MenuLine() + $" | moved to SETTINGS {moved}");
            bool pauseBack = Menus43Probe.PauseBackdrop;
            g.Resume();
            yield return Frames(Ms(300));
            bool pauseOk = !screensRow || (Menus43Probe.Built && Menus43Probe.PauseOpens > pOpens0 && pauseBack && pauseFocus == "SETTINGS" && pauseScale > 1.05f);
            lines.Add($"menus43 PAUSED (screens43 row {screensRow}): opens {Menus43Probe.PauseOpens - pOpens0}, backdrop {pauseBack}, focus after the move '{pauseFocus}' at {pauseScale.ToString("0.00", ci)} -> {(pauseOk ? "ok" : "NOT ok")}");
            if (!pauseOk) fails.Add("menus43 pause");

            // ---- screens43: the campaign results, then the shop (CONTINUE) ----
            bool screensOk = true;
            if (campaign && g.sim.campaignLevel != null && g.CampaignNextLevel25() != null)
            {
                if (g.sim.economy.earned < 3000) g.sim.economy.Grant(3000 - g.sim.economy.earned, "reel");
                g.sim.events.Emit("layerClear", new RoomEvent { room = g.sim.campaign != null ? g.sim.campaign.room : 0, id = g.sim.campaignLevel.id, name = g.sim.campaignLevel.name });
                if (g.sim.campaign != null) g.sim.campaign.finished = true;
                int k = 0;
                while (g.flow != Flow.Results && k++ < Ms(5000)) yield return null;
                if (g.flow == Flow.Results)
                {
                    yield return Frames(Math.Max(1, Ms(60)));
                    yield return Shot("U43-s-res0", ScreensLine(ci));
                    for (int w = 0; w < Ms(1200); w++) yield return null;
                    yield return Shot("U43-s-res", ScreensLine(ci));
                    if (g.UiView != null) g.UiView.CampRes25PressPrimary();
                    for (int w = 0; w < Ms(1000) && (g.Shop33Ui == null || !g.Shop33Ui.IsOpen); w++) yield return null;
                    if (g.Shop33Ui != null && g.Shop33Ui.IsOpen)
                    {
                        yield return Frames(Math.Max(1, Ms(60)));
                        yield return Shot("U43-s-shop0", ScreensLine(ci));
                        yield return Frames(Ms(600));
                        yield return Shot("U43-s-shop", ScreensLine(ci));
                        g.Shop33Ui.Move(1, 0);
                        yield return Frames(Ms(100));
                        yield return Shot("U43-s-shop-sel", ScreensLine(ci));
                        g.Shop33Ui.Close();
                    }
                    else fails.Add("CONTINUE did not open the shop");
                }
                else fails.Add($"no RESULTS after the clear (flow {g.flow})");
                screensOk = !screensRow || (Screens43Probe.ResultsBackdrop && Screens43Probe.ShopBackdrop && Screens43Probe.ResultsEnterDone > 0 && Screens43Probe.ShopEnterDone > 0);
            }
            else lines.Add("screens43: SKIPPED (not a campaign level with a next level)");
            lines.Add($"screens43 row {screensRow}: {ScreensLine(ci)} -> {(screensOk ? "ok" : "NOT ok")}");
            if (!screensOk) fails.Add("screens43");

            // ---- round 3: TITLE and the LEVEL SELECT (the hud33 reel's path: ToTitle, PLAY on CAMPAIGN) ----
            g.ToTitle();
            yield return Frames(Ms(1200));
            yield return Shot("U43-m-title", MenuLine());
            string titleFocus = Menus43Probe.FocusNow;
            string modeWas = g.setup.mode;
            g.setup.mode = RunSetup.Campaign;
            bool levels = g.UiView != null && g.UiView.ReelPressPlay26();
            yield return Frames(Ms(1200));
            yield return Shot("U43-m-levels", MenuLine() + $" | level select pressed {levels}, open {Ui.LevelSel26Open}");
            g.UiView?.CloseMeta();
            g.setup.mode = modeWas;
            yield return Frames(Ms(300));
            lines.Add($"menus43 TITLE / LEVEL SELECT: title focus '{titleFocus}', level select open {levels}");

            yield return Stop();
            bool ok = saved == want && fails.Count == 0;
            res[0] = $"{(ok ? "GREEN" : "RED")} ui43 {(any ? "AFTER (rows on: hud43 " + hudRow + ", counter43 " + counterRow + ", teach43 " + teachRow + ", bossbar43 " + barRow + ", screens43 " + screensRow + ")" : "BEFORE (every NIGHT43 UI row off)")}"
                + $" | frames {saved}/{want}{(fails.Count > 0 ? " | NOT ok: " + string.Join(", ", fails) : "")} || {string.Join(" || ", lines)} | {pc}";
        }

        static string ScreensLine(CultureInfo ci) =>
            $"results built {Screens43Probe.ResultsBuilt} backdrop {Screens43Probe.ResultsBackdrop} settled {Screens43Probe.ResultsEnterDone.ToString("0.00", ci)} s | shop built {Screens43Probe.ShopBuilt} backdrop {Screens43Probe.ShopBackdrop} settled {Screens43Probe.ShopEnterDone.ToString("0.00", ci)} s focus '{Screens43Probe.ShopFocus}'"
            + $" | blur: shader {UiBlur43.ShaderFound}, captures {UiBlur43.Captures}, {UiBlur43.LastW}x{UiBlur43.LastH}, passes {UiBlur43.Passes}, {UiBlur43.LastMs.ToString("0.0", ci)} ms";
    }
}
