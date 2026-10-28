// NIGHT52 builder U: the a152 phase (FeelReel.N52.cs routes it here): hud52's proof (design.n52.hud52, UiHud52.cs). Run on any campaign level:
//   bash Tools/night/n47-reels.sh <tag> u1on,n48-foundry,a152                 (the build copy's config with design.n52.hud52 on: GREEN)
//   bash Tools/night/n47-reels.sh <tag> u1off,n48-foundry,a152,-skygrindOff52 (the same build, the row off by the switch: RED, NIGHT51's HUD)
// The CAMPAIGN on the booted map (N24StartCampaign, L-062), DOORFREE, enemies frozen, the health HELD at each share (max 100) as hud47's a647 does.
// With the row built, every frame is a PAIR in the same Player and pose: Hud52Probe.ForceOff draws NIGHT51's HUD (the "before"), then hud52 (the
// "after"). Frames (shot-N30-U52-<name>-<off|on>.png; with the row not built: -off52, the one side there is):
//   hp100 / hp60 / hp29 / hp10 / over130     the health block at each share (bone; red-orange below 30 %; the overheal cyan)
//   hit                                      2 frames after a 100 -> 70 hit (the shake, the rule red-orange, the ghost)
//   heal                                     5 frames after a 40 -> 80 heal (the pop, the cyan segment)
//   kestrel / thunderclap / swap             the weapon block with each gun; swap: 2 frames after the THUNDERCLAP's swap began (the kick)
// Printed: per share the fill's colour (hex, OKLab hue and chroma), the number's and the rule's; the hit's peak shake (px at 1080p) and its trauma
// after 0.75 s (back to rest); the heal's peak scale; the swap's peak kick; the panels' rects and their distance to the reticle's centre box.
// GREEN when hud52 is built, the fill is bone (OKLab chroma < 0.06) at 100 / 60 % and red-orange (hue within 25 deg of the danger token) at 29 /
// 10 %, no fill is green (hue 100-170 deg with chroma > 0.08), the hit shook >= 3 px and settled (trauma 0), the heal popped >= 1.03, the swap
// kicked >= 6 px, and no frame shows magenta (a missing shader).
// Round 9 (the lesson card): its 2 s / 8 s frames are timed by the card's own clock (Teach52Probe.Age, the HUD's time since the line opened), not
// real seconds (the HUD's clock steps 1/60 s per recorded frame: n52-r12 showed no chip at 8 real s, why '', on a run 42.0 s long against r9's
// 34.6 s; a HUD clock short of 6 s there is the likely cause, not checked). The line prints both clocks
// and how often the line re-opened between the frames.
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
        static partial void N52Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Hud52Scenario(g, dir, res, offs, pre);

        static float Chroma52(Color c)
        {
            Vector3 lab = Hud47Math.ToOklab(c);
            return Mathf.Sqrt(lab.y * lab.y + lab.z * lab.z);
        }

        static string Col52(Color c, CultureInfo ci) => $"#{ColorUtility.ToHtmlStringRGB(c)} (h {Hud47Math.HueDeg(c).ToString("0", ci)} C {Chroma52(c).ToString("0.000", ci)})";

        static float HueGap52(float a, float b)
        {
            float d = Mathf.Abs(a - b) % 360f;
            return d > 180f ? 360f - d : d;
        }

        static IEnumerator Hud52Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() =>
            {
                Hud52Probe.ForceOff = false;
                g.husks.frozen = false;
                g.forcedCombat = IdleCombat;
                g.health.over = 0;
            });
            if (!sim.campaignOn) { res[0] = "RED a152: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 300;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 280) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            g.husks.frozen = true;
            N55T125InputBack(g, sim); // NIGHT55 S (T-125, FeelReel.N55.Smoke.cs): the sim read the campaign bot, never g.forcedCombat; the reel's input back, as a252 does
            string pc = Preconditions(g, g.Feet);
            Start(g, "a152-hud52", "camera", dir);
            yield return Frames(pre);
            bool built = Hud52Probe.Built;
            float ss = (float)CameraFeel.ShakeScale(g.settings);
            var lines = new List<string> { $"hud52 built {(built ? "yes (pairs: ForceOff = NIGHT51's HUD, then hud52)" : "NO (the row is off in this build or -skygrindOff52: these are NIGHT51's frames, the RED side)")}; frame {Screen.width}x{Screen.height}; CAMERA SHAKE setting x{ss.ToString("0.00", ci)}" };
            int[] mag = new int[1];
            int magentaFrames = 0;
            double hpNow = 1, overNow = 0;
            void Hold() { g.health.max = 100; g.health.hp = Math.Max(0.5, hpNow * 100); g.health.over = overNow; }
            IEnumerator HoldFor(int n) { for (int i = 0; i < n; i++) { Hold(); yield return null; } }
            IEnumerator Shot(string name)
            {
                yield return Capture47(dir, "U52-" + name, mag);
                if (mag[0] > 0) magentaFrames++;
            }
            string[] sides = built ? new[] { "off", "on" } : new[] { "off52" };

            yield return Equip27(g, "revolver");
            g.forcedCombat = IdleCombat;

            // ---- 1. the health block at each share ----
            var sb = new System.Text.StringBuilder("health (the 'on' side):");
            bool boneOk = true, dangerOk = true, noGreen = true;
            float dangerHue = Hud47Math.HueDeg(UiStyle33.Danger);
            bool overOn = sim.Overheal47On && sim.Overheal47Max > 0;
            var shares = new List<(string name, double hp, double over)> { ("hp100", 1, 0), ("hp60", 0.6, 0), ("hp29", 0.29, 0), ("hp10", 0.1, 0) };
            if (overOn) shares.Add(("over130", 1, Math.Min(30, sim.Overheal47Max)));
            foreach (var s in shares)
            {
                hpNow = s.hp;
                overNow = s.over;
                foreach (string side in sides)
                {
                    Hud52Probe.ForceOff = side == "off";
                    yield return HoldFor(40); // the ghost drained, the number eased
                    yield return Shot($"{s.name}-{side}");
                    if (side != "on") continue;
                    Color f = Hud52Probe.FillColor;
                    sb.Append($" {s.name}: fill {Col52(f, ci)}, number {Col52(Hud52Probe.NumberColor, ci)}, rule {Col52(Hud52Probe.EdgeColor, ci)};");
                    float hue = Hud47Math.HueDeg(f), ch = Chroma52(f);
                    if (hue > 100f && hue < 170f && ch > 0.08f) noGreen = false;
                    if (s.hp >= 0.5 && s.over <= 0 && ch >= 0.06f) boneOk = false;
                    if (s.hp < UnityTuning.Hud52LowFrac && HueGap52(hue, dangerHue) > 25f) dangerOk = false;
                }
            }
            overNow = 0;
            lines.Add(overOn ? sb.ToString() : sb + " (overheal47 off in this build: no over130 frame)");

            // ---- 2. a hit: 100 -> 70 at once ----
            float peakShake = -1f, trauma075 = -1f;
            int hits = -1;
            foreach (string side in sides)
            {
                Hud52Probe.ForceOff = side == "off";
                hpNow = 1;
                yield return HoldFor(45);
                Hud52Probe.ResetPeaks();
                int h0 = Hud52Probe.Hits;
                hpNow = 0.7;
                yield return HoldFor(2);
                yield return Shot($"hit-{side}");
                yield return HoldFor(45); // 0.75 s at 60 fps
                if (side == "on") { peakShake = Hud52Probe.PeakShakePx; trauma075 = Hud52Probe.Trauma; hits = Hud52Probe.Hits - h0; }
            }
            lines.Add(built ? $"hit 100 -> 70: hits seen {hits}, peak shake {peakShake.ToString("0.0", ci)} px (at 1080p, x the setting), trauma 0.75 s later {trauma075.ToString("0.000", ci)}; the rule now {Col52(Hud52Probe.EdgeColor, ci)}" : "hit 100 -> 70: frames only (hud52 not built)");

            // ---- 3. a heal: 40 -> 80 ----
            float peakScale = -1f;
            int heals = -1;
            foreach (string side in sides)
            {
                Hud52Probe.ForceOff = side == "off";
                hpNow = 0.4;
                yield return HoldFor(45);
                Hud52Probe.ResetPeaks();
                int e0 = Hud52Probe.Heals;
                hpNow = 0.8;
                yield return HoldFor(5);
                yield return Shot($"heal-{side}");
                yield return HoldFor(40);
                if (side == "on") { peakScale = Hud52Probe.PeakScale; heals = Hud52Probe.Heals - e0; }
            }
            lines.Add(built ? $"heal 40 -> 80: heals seen {heals}, peak scale {peakScale.ToString("0.000", ci)}" : "heal 40 -> 80: frames only (hud52 not built)");
            hpNow = 1;

            // ---- 4. the weapon block: KESTREL, the swap's kick, THUNDERCLAP ----
            foreach (string side in sides)
            {
                Hud52Probe.ForceOff = side == "off";
                yield return HoldFor(30);
                yield return Shot($"kestrel-{side}");
            }
            float peakKick = -1f;
            int swaps = -1;
            if (g.loadout.owned.IndexOf("shotgun") < 0) g.loadout.AddWeapon("shotgun");
            Hud52Probe.ForceOff = false;
            Hud52Probe.ResetPeaks();
            int w0 = Hud52Probe.Swaps;
            int slot = g.loadout.owned.IndexOf("shotgun");
            for (int i = 0; i < 30 && Hud52Probe.Swaps == w0; i++) { g.forcedCombat = new CombatInput { weapon = slot }; Hold(); yield return null; }
            g.forcedCombat = IdleCombat;
            yield return HoldFor(2);
            yield return Shot(built ? "swap-on" : "swap-off52");
            yield return Equip27(g, "shotgun");
            g.forcedCombat = IdleCombat;
            peakKick = Hud52Probe.PeakKickPx;
            swaps = Hud52Probe.Swaps - w0;
            foreach (string side in sides)
            {
                Hud52Probe.ForceOff = side == "off";
                yield return HoldFor(30);
                yield return Shot($"thunderclap-{side}");
            }
            yield return N55T125Taps(g, sim, lines, "a152 (the THUNDERCLAP in hand after Equip27)", Hold, null); // NIGHT55 S (T-125): six taps, the fire events counted and the fix named
            Hud52Probe.ForceOff = false;
            Rect hp = Hud52Probe.HealthPanel, wp = Hud52Probe.WeaponPanel;
            float cw = (float)Hud33Layout.CanvasW(Screen.width, Screen.height);
            // the panels' nearest corner to the screen's centre (canvas units: the reticle's 18 px box is +-9 round it)
            float hpGap = Vector2.Distance(new Vector2(hp.xMax, hp.yMax), new Vector2(cw * 0.5f, 540f));
            float wpGap = Vector2.Distance(new Vector2(cw + wp.xMin, wp.yMax), new Vector2(cw * 0.5f, 540f));
            lines.Add(built ? $"weapon: swaps seen {swaps}, peak kick {peakKick.ToString("0.0", ci)} px, the rule {Col52(Hud52Probe.WeaponEdgeColor, ci)}; panels (canvas units at 1080p, from their corner): health x {hp.x.ToString("0", ci)} y {hp.y.ToString("0", ci)} w {hp.width.ToString("0", ci)} h {hp.height.ToString("0", ci)}, weapon x {wp.x.ToString("0", ci)} y {wp.y.ToString("0", ci)} w {wp.width.ToString("0", ci)} h {wp.height.ToString("0", ci)}; nearest corner to the centre {Mathf.Min(hpGap, wpGap).ToString("0", ci)} px (the reticle's box: 9)" : "weapon: frames only (hud52 not built)");
            // ---- 5. round 3: the room's lesson card as a chip after 6 s or at the first kill (UiTeach52.cs) ----
            hpNow = 1;
            float CardH()
            {
                GameObject card = GameObject.Find(Ui.Teach43Name);
                if (card == null) return 0f;
                var rt = (RectTransform)card.transform;
                return rt.rect.height * rt.localScale.y; // canvas units (px at 1080p): the plate's height x its pop / chip scale
            }
            float CardHeadPx() => built ? Teach52Probe.HeadPx : Teach43Probe.HeadPx;
            // round 4 (rule 8): each line the card DRAWS now, read from the card's own objects (not hud52's probe): "Verb" and "Rule" (UiTeach43's
            // labels), drawn = active in the hierarchy with a visible colour; its px on screen = font size x the card's scale (px at 1080p)
            string CardLines(out bool allReadable)
            {
                allReadable = true;
                GameObject card = GameObject.Find(Ui.Teach43Name);
                if (card == null) return "no card";
                var sbl = new System.Text.StringBuilder();
                float sc = card.transform.localScale.y;
                foreach (string ln in new[] { "Verb", "Rule" })
                {
                    Transform t = card.transform.Find(ln + "/Text");
                    var tmp = t != null ? t.GetComponent<TMPro.TextMeshProUGUI>() : null;
                    bool drawn = tmp != null && tmp.gameObject.activeInHierarchy && tmp.color.a > 0.01f && tmp.text.Length > 0;
                    float px = drawn ? tmp.fontSize * sc : 0f;
                    if (drawn && px < 16f) allReadable = false;
                    sbl.Append($"{ln} {(drawn ? px.ToString("0.0", ci) + " px" : "not drawn")}; ");
                }
                return sbl.ToString().TrimEnd(' ', ';');
            }
            IEnumerator CardOpen(string line, float waitS)
            {
                int o0 = Teach43Probe.Opens;
                Hud47Probe.ForceTeach = line;
                float t0 = Time.unscaledTime;
                while (Teach43Probe.Opens == o0 && Time.unscaledTime - t0 < waitS) { Hold(); yield return null; }
            }
            float teachT0 = 0f, h2 = -1f, h8 = -1f, px8 = -1f, hk0 = -1f, hk1 = -1f;
            string reason8 = "", reasonK = "";
            // round 9 (n52-r12: 95 px at "8 s" with no chip, why ''): the clock is the card's own, the HUD's time since this line opened
            // (Teach52Probe.Age; the HUD's clock steps 1/60 s per recorded frame, so under 60 fps 8 real seconds are less HUD time);
            // without hud52 the reel's own (unscaled) clock. The opens are counted from the 2 s frame to the 8 s one (a re-open restarts the card's
            // clock, so the 8 s frame is always 8 s after the line on screen opened).
            float CardAge() => built ? Teach52Probe.Age : Time.unscaledTime - teachT0;
            IEnumerator UntilAge(float s, float capS)
            {
                float c0 = Time.unscaledTime;
                while (CardAge() < s && Time.unscaledTime - c0 < capS) { Hold(); yield return null; }
            }
            yield return CardOpen(UnityTuning.Teach52ReelLine, 4f);
            teachT0 = Time.unscaledTime;
            bool cardUp = Teach43Probe.Shown;
            yield return UntilAge(2f, 20f);
            h2 = CardH();
            float age2 = CardAge();
            int opens2 = Teach43Probe.Opens;
            yield return Shot("teach-2s-" + sides[sides.Length - 1]);
            yield return UntilAge(8f, 40f);
            h8 = CardH();
            float age8 = CardAge(), real8 = Time.unscaledTime - teachT0;
            int reopened = Teach43Probe.Opens - opens2;
            px8 = CardHeadPx();
            string lines8 = CardLines(out bool readable8);
            reason8 = Teach52Probe.Reason;
            yield return Shot("teach-8s-" + sides[sides.Length - 1]);
            // the kill path: a new line opens the card full; a (view-only) kill 1 s later shrinks it
            yield return CardOpen(UnityTuning.Teach52ReelLineKill, 4f);
            teachT0 = Time.unscaledTime;
            yield return UntilAge(1f, 10f);
            hk0 = CardH();
            Teach52Probe.ForceKill = true;
            float tk = Time.unscaledTime;
            // the ease (Teach52ChipEaseS of HUD time) done, or 3 real s
            while ((!built || Teach52Probe.Chip < 0.999f) && Time.unscaledTime - tk < 3f) { Hold(); yield return null; }
            hk1 = CardH();
            reasonK = Teach52Probe.Reason;
            string linesK = CardLines(out bool readableK);
            yield return Shot("teach-kill-" + sides[sides.Length - 1]);
            Teach52Probe.ForceKill = false;
            Hud47Probe.ForceTeachOff = true;
            yield return HoldFor(10);
            bool chipped = built && h2 > 0 && h8 > 0 && h8 <= UnityTuning.Teach52ReelChipShare * h2 && px8 >= 16f && readable8 && age8 >= 8f; // a re-open restarts the card's clock: the 8 s frame is then 8 s after the line it opened (the count printed)
            bool killChip = built && hk0 > 0 && hk1 > 0 && hk1 <= UnityTuning.Teach52ReelChipShare * hk0 && reasonK == "kill" && readableK;
            lines.Add($"lesson card: up {cardUp} (head '{Teach43Probe.Head}'); the card's clock {age2.ToString("0.00", ci)} s / {age8.ToString("0.00", ci)} s at the two frames ({real8.ToString("0.0", ci)} real s; the line re-opened {reopened} times between); height at 2 s {h2.ToString("0", ci)} px, at 8 s {h8.ToString("0", ci)} px (want <= x{UnityTuning.Teach52ReelChipShare.ToString("0.00", ci)} with hud52: {chipped}; why '{reason8}'), its verb line at 8 s {px8.ToString("0.0", ci)} px (>= 16); the lines drawn at 8 s: {lines8} (every drawn line >= 16 px: {readable8}); the kill path: {hk0.ToString("0", ci)} -> {hk1.ToString("0", ci)} px 0.6 s after a kill ({killChip}; why '{reasonK}'; drawn: {linesK}, >= 16 px: {readableK})");
            // ---- 6. round 11: the top of the HUD (UiHud52.Top.cs): the counter, the score, the room title ----
            // a forced count (Counter43Probe.Force) and a forced room title (Hud47Probe.ForceBanner, the message slot), per side the frame top-<side>
            bool topOk = false;
            string topLine = "top: hud52 not built (frames only)";
            Counter43Probe.Force = UnityTuning.Hud52ReelCount;
            offs.Add(() => Counter43Probe.Force = int.MinValue);
            Hud47Probe.ForceBanner = UnityTuning.Hud52ReelTitle;
            foreach (string side in sides)
            {
                Hud52Probe.ForceOff = side == "off";
                yield return HoldFor(40); // the counter's pop and the slot's fade-in done
                yield return Shot($"top-{side}");
                if (side != "on") continue;
                // each plate's screen rect (an overlay canvas: world corners are screen px) and its gap to the reticle's 18 px box
                float k1080 = Screen.height / 1080f;
                var centre = new Rect(Screen.width * 0.5f - 9f * k1080, Screen.height * 0.5f - 9f * k1080, 18f * k1080, 18f * k1080);
                var corners = new Vector3[4];
                string Plate(UnityEngine.UI.Image im, out bool shown, out bool clear)
                {
                    shown = im != null && im.gameObject.activeInHierarchy && im.color.a > 0.05f;
                    clear = true;
                    if (im == null) return "not built";
                    im.rectTransform.GetWorldCorners(corners);
                    var r = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                    clear = !r.Overlaps(centre);
                    return $"{Mathf.RoundToInt(r.width / k1080)} x {Mathf.RoundToInt(r.height / k1080)} px at ({Mathf.RoundToInt(r.x / k1080)}, {Mathf.RoundToInt((Screen.height - r.yMax) / k1080)}) from the top-left, shown {shown}, clear of the reticle {clear}";
                }
                string cRect = Plate(Top52Probe.CounterPlate, out bool cShown, out bool cClear);
                string pRect = Plate(Top52Probe.PointsPlate, out bool pShown, out bool pClear);
                string sRect = Plate(Top52Probe.SlotPlate, out bool sShown, out bool sClear);
                Color pc52 = Top52Probe.PointsColor;
                float parryHue = Hud47Math.HueDeg(Skygrind.Game.Look.Hex(UnityTuning.Look49ParryHex));
                bool pointsBone = Chroma52(pc52) < 0.06f; // bone, not the parry's yellow
                bool counterDanger = HueGap52(Hud47Math.HueDeg(Top52Probe.CounterAccent), dangerHue) <= 25f && Chroma52(Top52Probe.CounterAccent) > 0.08f; // enemies: red-orange (7 left)
                bool slotBone = Chroma52(Top52Probe.SlotText) < 0.06f; // a room title: bone
                bool sizes = Top52Probe.CounterNumPx >= 16f && Top52Probe.CounterWordPx >= 16f && Top52Probe.PointsPx >= 16f && Top52Probe.SlotPx >= 16f;
                topOk = built && Top52Probe.CounterBuilt && Top52Probe.PointsBuilt && Top52Probe.SlotBuilt && cShown && pShown && sShown && cClear && pClear && sClear
                    && pointsBone && counterDanger && slotBone && sizes;
                topLine = $"top (frames top-<side>; counter forced {UnityTuning.Hud52ReelCount}, title '{UnityTuning.Hud52ReelTitle}'): counter plate {cRect}, accent {Col52(Top52Probe.CounterAccent, ci)} (red-orange {counterDanger}), drain line {Col52(Top52Probe.CounterLine, ci)}, count {Top52Probe.CounterNumPx.ToString("0", ci)} px / word {Top52Probe.CounterWordPx.ToString("0", ci)} px; "
                    + $"score plate {pRect}, the score {Col52(pc52, ci)} (bone {pointsBone}; the parry's yellow is h {parryHue.ToString("0", ci)}), {Top52Probe.PointsPx.ToString("0", ci)} px; "
                    + $"title plate {sRect}, text {Col52(Top52Probe.SlotText, ci)} (bone {slotBone}), accent {Col52(Top52Probe.SlotAccent, ci)}, {Top52Probe.SlotPx.ToString("0", ci)} px, alpha {Top52Probe.SlotAlpha.ToString("0.00", ci)}; every text >= 16 px {sizes} => {(topOk ? "GREEN" : "RED")}";
            }
            Counter43Probe.Force = int.MinValue;
            Hud52Probe.ForceOff = false;
            lines.Add(topLine);

            // ---- 7. round 14 (NIGHT52.md leftover 5): a teach35 line waits for its verb (UiTeach52.Verb.cs) ----
            // per side: teach35's own l1.fire line sent into the view's queue, the verb held back Teach52ReelVerbWaitS (HUD time: the reels' clock
            // steps 1/60 s a frame) past NIGHT51's timer, frame verb-wait-<side>; then one shot (the "fire" event) and the frames until the line is gone
            bool verbOk = !built, verbOffOk = true;
            var verbNotes = new List<string>();
            double showS = g.cfg != null && g.cfg.Has("design.n35.teach35.showS") ? g.cfg.Num("design.n35.teach35.showS") : 5;
            // round 16 (n52-r20: six taps with the THUNDERCLAP in hand, no "fire" heard): the KESTREL, whose 1-frame presses every 30 frames fire in
            // a252's kill-wash block (n52-r17: both kills made) and in verbs37 row 25; the line is the KESTREL's own (l1.fire)
            int eqStep0 = sim.simStep;
            yield return Equip27(g, "revolver");
            g.forcedCombat = IdleCombat;
            // EXT53 B (T-125): the sim fires the THUNDERCLAP on this section's own taps and takes this Equip27 headless on n48-foundry (SimHarness tap53:
            // a152's sim-side steps, 2 steps a frame, the health held: taps 1 / 3 / 5 fire, 2 / 4 / 6 wait for the gun's interval), yet at n52-freeze
            // this Equip27 left the THUNDERCLAP in hand and no tap fired. So the Player's context drops the reel's combat input here, not the game's
            // rules: these lines say which (the sim stepping, Active, flow, whose input the sim reads, the loadout's ready / target, charge35's counts).
            string Ctx53() => $"sim {(ReferenceEquals(g.sim, sim) ? "same" : "REPLACED")}, input {(sim.input == null ? "null" : sim.input.GetType().Name)}{(ReferenceEquals(sim.input, g.playerInput) ? " (the reel's)" : " (NOT the reel's forcedCombat reader)")}, active {sim.Active}, flow {sim.flow}, loadout {(ReferenceEquals(g.loadout, sim.loadout) ? "same" : "REPLACED")} ready {sim.loadout.ready} target {sim.loadout.targetSlot} in hand {sim.loadout.Weapon}";
            verbNotes.Add($"before: Equip27(revolver) over {sim.simStep - eqStep0} sim steps, {Ctx53()}");
            foreach (string side in sides)
            {
                Hud52Probe.ForceOff = side == "off";
                yield return HoldFor(30);
                int shows0 = Ui.Teach35Shows;
                g.events.Emit("teach35", new Teach35Event { id = UnityTuning.Teach52ReelVerbId, text = UnityTuning.Teach52ReelVerbLine, trigger = "reel52", seconds = showS });
                int waited = 0;
                while (Ui.Teach35Shows == shows0 && waited < 900) { Hold(); waited++; yield return null; } // the line reaches the slot (a busy slot)
                if (Ui.Teach35Shows == shows0) { verbNotes.Add($"{side}: the line never showed in 15 s"); if (side == "on") verbOk = false; continue; }
                yield return HoldFor((int)Math.Round(UnityTuning.Teach52ReelVerbWaitS * UnityTuning.ReelFps));
                bool upAtWait = Ui.Teach35IdNow == UnityTuning.Teach52ReelVerbId;
                yield return Shot($"verb-wait-{side}");
                int cleared0 = Verb52Probe.Cleared, fires0 = Verb52Probe.Fires;
                // round 15 (n52-r19: after one 1-frame press the line stayed; the likely cause, not checked: the press never fired): a tap like a thumb's (the trigger
                // held Teach52ReelTapFrames frames, then released), again every Teach52ReelTapEvery frames until a "fire" event is heard (6 tries)
                int sinceFire = -1, tries = 0, waitedF = 0;
                int tapStep0 = sim.simStep; // EXT53 B (T-125): the sim's steps over the taps, and charge35's own counts of them
                Charge35 q53 = sim.charge35;
                int qTaps0 = q53 != null ? q53.taps : 0, qShots0 = q53 != null ? q53.tapShots : 0;
                for (; waitedF < 6 * UnityTuning.Teach52ReelTapEvery && sinceFire < 0; waitedF++)
                {
                    int ph = waitedF % UnityTuning.Teach52ReelTapEvery;
                    if (ph == 0) tries++;
                    g.forcedCombat = ph < UnityTuning.Teach52ReelTapFrames ? new CombatInput { weapon = -1, fire = new Button { held = true, pressed = ph == 0 } } : IdleCombat;
                    Hold();
                    yield return null;
                    if (Verb52Probe.Fires > fires0 || (side != "on" && ph == UnityTuning.Teach52ReelTapFrames)) sinceFire = 0; // the OFF side's listener may be unwired: one tap is enough there
                }
                g.forcedCombat = IdleCombat;
                bool fired = Verb52Probe.Fires > fires0;
                verbNotes.Add($"{side} taps: {waitedF} frames, {sim.simStep - tapStep0} sim steps, charge35 {(q53 == null ? "null" : $"taps +{q53.taps - qTaps0}, tap shots +{q53.tapShots - qShots0}, holding {q53.holding}, tap queued {q53.tapQueued}")}; {Ctx53()}");
                int frames = 0;
                while (Ui.Teach35IdNow == UnityTuning.Teach52ReelVerbId && frames < 120) { Hold(); frames++; yield return null; }
                bool gone = Ui.Teach35IdNow != UnityTuning.Teach52ReelVerbId;
                yield return Shot($"verb-after-{side}");
                verbNotes.Add($"{side}: up after {UnityTuning.Teach52ReelVerbWaitS.ToString("0", ci)} s {upAtWait}; taps {tries} (in hand {g.loadout.Weapon}), a fire heard {fired}; then gone {gone} in {frames} frames (cleared by the verb {Verb52Probe.Cleared > cleared0}); the last finished line held {Ui.Teach35LastHeldS.ToString("0.00", ci)} s");
                if (side == "on") verbOk = upAtWait && fired && gone && frames <= UnityTuning.Teach52ReelVerbClearFrames && Verb52Probe.Cleared > cleared0;
                else verbOffOk = !upAtWait; // NIGHT51's timer: gone at showS
            }
            Hud52Probe.ForceOff = false;
            g.forcedCombat = IdleCombat;
            lines.Add($"verb wait (line '{UnityTuning.Teach52ReelVerbId}', NIGHT51's timer {showS.ToString("0.0", ci)} s, the verb after {UnityTuning.Teach52ReelVerbWaitS.ToString("0", ci)} s; frames verb-wait-<side>, verb-after-<side>): {string.Join("; ", verbNotes)} => {(built ? (verbOk ? "GREEN" : "RED") : "frames only")}{(built && !verbOffOk ? " (the OFF side was still up past NIGHT51's timer)" : "")}");

            lines.Add($"magenta frames {magentaFrames}");
            yield return Frames(5);
            yield return Stop();
            bool ok = built && boneOk && dangerOk && noGreen && peakShake >= 3f * Mathf.Min(1f, Mathf.Max(ss, 0.01f)) && trauma075 <= 1e-4f && peakScale >= 1.03f && swaps >= 1 && peakKick >= 6f && chipped && killChip && topOk && magentaFrames == 0; // round 16: the verb wait is judged in a252 (its KESTREL fires there), printed here
            res[0] = $"{(ok ? "GREEN" : "RED")} a152 hud52 | built {built}, bone {boneOk}, red-orange low {dangerOk}, no green {noGreen}, shake {peakShake.ToString("0.0", ci)} px, settled {trauma075 <= 1e-4f}, pop {peakScale.ToString("0.000", ci)}, kick {peakKick.ToString("0.0", ci)} px, lesson chip at 8 s {chipped} / at a kill {killChip}, top plates {topOk}, verb wait {verbOk}, magenta {magentaFrames} || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
