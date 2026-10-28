// NIGHT33 builder U (design.n33.hud33, a view row: no sim bit): the HUD rebuilt in one language (U1 spec section 6; DESIGN33 card U; DECIDE33 D-1).
// GG (2026-10-11): the HUD "looks so AI generated", "way too much space". With the row on:
//   - The NIGHT32 HUD layers (frame / values / sight) are hidden by a CanvasGroup at alpha 0. They keep running, so every reel hook they write
//     (Weapon30Ids, Lesson25TextNow, Hud32TopLineNow, weaponBlock's widgets, the hit marker) still reads true values. The parts that are not HUD
//     readouts move into this layer as they are: the hurt and damage vignettes, the damage-direction arcs, the aim-assist brackets, the buy prompt and
//     its plate, the ADS / LONGWAVE scopes. GrapplePips32's diamonds hide (Hud33Live): the charges are two slash ticks under the reticle here.
//   - No panel fills. Text sits on the world with a 2 px shadow (UiStyle33). The one ornament is the 20-degree slash tick.
//   - Bottom-left: the health number (64 px) over ten slash ticks (red-orange only on a hit's 150 ms flash and at or under 25 %), the stamina as thin
//     slash bars under them (never mid-screen).
//   - Bottom-right, off the gun model (it never follows the sway): the held weapon's name (24 px caps), its LT action under it (16 px, the only LT
//     label on screen), a pip per owned weapon (the held one cyan and wider); on a switch the names list above it for 700 ms and the name slides in.
//   - Top-right: the points in bone (32 px), the style rank's letter and its 160 x 3 line only while the meter is above 0.
//   - Top-left: the round line (20 px) when the run has one.
//   - Top-centre: THE MESSAGE SLOT (MessageSlot33): one line, each held >= 1.2 s, danger (EXPOSED) > a boss's teaching line > room titles (every
//     Ui.Banner line and the ROOM n banner, posted) > the door hint (DOOR OPEN) > the lesson. The lesson widget and the old banners / door cue still run,
//     hidden. The door's distance is a world marker on the door (a cyan chevron and "7 m" over the room's exit while it is on screen).
//   - The boss bar: top-centre, the name in 16 px over a 640 x 6 red-orange bar (every boss through bossKit / HudReads20.ThreatNow, THE CROWN included);
//     the slot drops under it (and under the Glacier chase's gap bar, Hud29, while that shows). A 4:3 screen narrows the column clear of the corners.
//   - The reticle: a 4 px dot, four 8 x 2 ticks at 10 px; a hit flashes and pushes the ticks (80 ms), a kill turns them 45 degrees (an X, 120 ms in),
//     the KESTREL / THUNDERCLAP charge closes two arcs on the dot (cyan when full), the ticks go cyan while the momentum is at IMPACT (the momentum
//     ring folded in) and red-orange while the aim-assist holds an enemy; the grapple's charges are two slash ticks under it.
// Off: the row off or -skygrindOff33 (N33Fixes.ViewRowOn): nothing here is built and the HUD is NIGHT32's.
// Every number: UnityTuning.Hud33.cs; every box: Hud33Layout (the reel and the EditMode tests check the same boxes).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool hud33On, hud33Read;

        /// <summary>For the reels and the self-installing overlays: the HUD33 layer is drawn this frame (row on, a run on screen).</summary>
        public static bool Hud33Live;
        /// <summary>For the reels: the HUD33 root (its live text rects are what the hud33 reel checks), and what it shows now.</summary>
        public static RectTransform Hud33Root;
        public static string Hud33WeaponNow = "", Hud33LtNow = "", Hud33SlotNow = "";
        public static int Hud33HitFlashes, Hud33KillTurns, Hud33Switches;
        public static bool Hud33RankShown, Hud33SwitchListShown;
        public static float Hud33ChargeNow, Hud33KillAngleNow;
        public static int Hud33GrappleFull = -1;

        bool Hud33On
        {
            get
            {
                if (!hud33Read && cfg != null)
                {
                    hud33On = N33Fixes.ViewRowOn(cfg, "hud33");
                    hud33Read = true;
                }
                return hud33On;
            }
        }

        RectTransform hud33, hud33Kept;
        UiStyle33.Text33 h33Health, h33Weapon, h33Lt, h33Points, h33Rank, h33Round, h33Slot;
        readonly List<UiStyle33.Text33> h33List = new List<UiStyle33.Text33>();
        Image[] h33HpTicks, h33StamFill, h33StamTrack, h33Pips, h33Arms, h33GrFill, h33GrTrack;
        Image h33Dot, h33ArcA, h33ArcB, h33RankLine;
        RectTransform h33ArmHolder;
        int h33ShownHp = int.MinValue, h33ShownLit = -1, h33ShownStamN = -1, h33ShownPipN = -1, h33ShownHeldIdx = -1;
        long h33ShownPoints = long.MinValue;
        int h33ShownRank = -1;
        string h33ShownHeld;
        bool h33HpRed;
        double h33HitLeft, h33HurtLeft, h33KillAge = double.MaxValue, h33SwitchAge = double.MaxValue;
        Color h33TickColor;
        double h33ArcAt = double.NaN;
        StaminaHud.Info h33Stam;
        readonly Dictionary<string, string> h33LtWords = new Dictionary<string, string>();
        string h33LtLastWord, h33LtLast = "";
        bool h33LtLastPad;
        object h33LtLayout;
        int h33LtGlyphs = -1;
        long h33RoundKey = long.MinValue;

        // round 2: the message slot, the boss bar, the door marker
        readonly MessageSlot33 h33Msg = new MessageSlot33 { MinHold = UnityTuning.Hud33SlotMinHold, DropAfter = UnityTuning.Hud33SlotDropAfter }; // pure C#: safe in an initialiser
        RectTransform hud33Hidden, h33BossRt, h33DoorRt;
        Image h33SlotScrim;
        UiStyle33.Text33 h33BossName, h33DoorText;
        Image h33BossFill, h33BossTrack;
        HudReads20.Threat h33Threat;
        double h33BossAge = -1, h33SlotFade;
        float h33ShownBossFill = -1, h33ShownCw = -1;
        string h33SlotText = "", h33BossRaw, h33ShownBoss;
        int h33SlotPrio = -1, h33ShownDoorM = -1, h33SlotPlace = -1;
        /// <summary>For the reels: the slot's shows / dropped posts, the boss bar up and its name, the door marker drawn and its distance.</summary>
        public static int Hud33SlotShows, Hud33SlotDropped, Hud33SlotPrioNow = -1, Hud33DoorMetres = -1;
        public static bool Hud33BossBarShown, Hud33DoorMarkShown;
        public static string Hud33BossNameNow = "";
        public static double Hud33SlotMinHeldSeen = double.MaxValue; // the shortest time a line stayed up before another replaced it (s; MaxValue: none replaced yet)

        /// <summary>A banner (Ui.Banner, the ROOM n banner) into the slot as a room-title post; any time, built or not (the row on).</summary>
        void Post33(string text, double seconds, int prio = MessageSlot33.Title)
        {
            if (!Hud33On || string.IsNullOrEmpty(text)) return;
            h33Msg.Post(prio, text, h33Clock, seconds);
        }

        // ================================================================== build ==================================================================

        void BuildHud33Now()
        {
            UiTheme.Init(cfg);
            hud33 = UiStyle33.Layer(root, "HUD 33");
            hud33.SetSiblingIndex(menus.GetSiblingIndex()); // under the menus (PAUSED, SETTINGS draw over it), over the hidden NIGHT32 layers
            Hud33Root = hud33;
            hud33Kept = UiStyle33.Rect(hud33, "Kept", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            hud33Kept.anchorMin = Vector2.zero;
            hud33Kept.anchorMax = Vector2.one;
            hud33Kept.offsetMin = hud33Kept.offsetMax = Vector2.zero;
            // the NIGHT32 layers: hidden, still running (their values feed the reels)
            foreach (RectTransform layer in new[] { frame, values, sight })
            {
                CanvasGroup g = layer.GetComponent<CanvasGroup>();
                if (g == null) g = layer.gameObject.AddComponent<CanvasGroup>(); // never `??` on a Unity object (the Editor's fake null)
                g.alpha = UnityTuning.Hud33HiddenAlpha;
                g.blocksRaycasts = false;
                g.interactable = false;
            }
            // what is not a readout keeps working, moved here unchanged (the layers are full-screen rects alike, so their places hold)
            Keep33(damageVignette != null ? damageVignette.transform : null);
            Keep33(vignette != null ? vignette.transform : null);
            foreach (Image hi in hitImgs) Keep33(hi.transform);
            Keep33(lockBox);
            Keep33(adsScopeRoot);
            Keep33(zoomScope20);
            Keep33(buyPlate19 != null ? buyPlate19.transform : null);
            Keep33(prompt != null ? prompt.transform : null);
            // the lesson line (meta layer) keeps running hidden: its text goes into the message slot (Lesson25TextNow / Lesson25Visible keep their values)
            hud33Hidden = UiStyle33.Rect(hud33, "Hidden", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            CanvasGroup hg = hud33Hidden.gameObject.AddComponent<CanvasGroup>();
            hg.alpha = 0f;
            hg.blocksRaycasts = hg.interactable = false;
            if (lesson25Rt != null) lesson25Rt.SetParent(hud33Hidden, false);
            // the break's verb prompts ("Q  LAST GUN", bottom centre, UiOnboarding): no dark plate (the lead's n33-r1 frames), the text on the world with the
            // theme's hard shadow; the key cap stays (it is the key's glyph)
            foreach (VerbPrompt vp in prompts)
            {
                if (vp.plate != null) vp.plate.enabled = false;
                Material sm = vp.text != null ? UiTheme.ShadowFor(vp.text.font) : null;
                if (sm != null) vp.text.fontSharedMaterial = sm;
            }

            Color bone = UiStyle33.Bone, faint = UiTheme.A(bone, UnityTuning.Hud33GrappleSpentAlpha);
            Vector2 bl = Vector2.zero, br = new Vector2(1, 0), tr = Vector2.one, tl = new Vector2(0, 1), mid = new Vector2(0.5f, 0.5f);

            // --- health and stamina, bottom-left ---
            Hud33Layout.Box hn = Hud33Layout.HealthNumber;
            h33Health = UiStyle33.Label(hud33, "Health", (float)hn.px, bone, UiStyle33.FaceNumber, TextAlignmentOptions.BottomLeft, bl, bl, P(hn), S(hn), false);
            Hud33Layout.Box ht = Hud33Layout.HealthTicks;
            RectTransform ticks = UiStyle33.Rect(hud33, "Health ticks", bl, bl, P(ht), S(ht));
            h33HpTicks = new Image[UnityTuning.Hud33HealthTicks];
            for (int i = 0; i < h33HpTicks.Length; i++)
                h33HpTicks[i] = UiStyle33.Slash(ticks, "Tick " + i, bone, bl, bl, new Vector2(i * UnityTuning.Hud33HealthTickPitch, 0), new Vector2(UnityTuning.Hud33TickW, UnityTuning.Hud33TickH));
            Hud33Layout.Box st = Hud33Layout.Stamina;
            RectTransform stam = UiStyle33.Rect(hud33, "Stamina", bl, bl, P(st), S(st));
            const int maxBars = 4;
            h33StamFill = new Image[maxBars];
            h33StamTrack = new Image[maxBars];
            for (int i = 0; i < maxBars; i++)
            {
                h33StamFill[i] = UiStyle33.SlashBar(stam, "Bar " + i, UiStyle33.Cyan, bl, bl, Vector2.zero, new Vector2(UnityTuning.Hud33StaminaW, UnityTuning.Hud33StaminaH), out h33StamTrack[i], faint);
                h33StamFill[i].gameObject.SetActive(false);
                h33StamTrack[i].gameObject.SetActive(false);
            }

            // --- the weapon, bottom-right ---
            Hud33Layout.Box wn = Hud33Layout.WeaponName, lt = Hud33Layout.LtLabel;
            h33Weapon = UiStyle33.Label(hud33, "Weapon", (float)wn.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.BottomRight, br, br, P(wn), S(wn));
            h33Lt = UiStyle33.Label(hud33, "LT label", (float)lt.px, UiStyle33.Cyan, UiStyle33.FaceLabel, TextAlignmentOptions.BottomRight, br, br, P(lt), S(lt));
            Hud33Layout.Box wp = Hud33Layout.WeaponPips(UnityTuning.Hud30MaxSlots);
            RectTransform pips = UiStyle33.Rect(hud33, "Weapon pips", br, br, P(wp), S(wp));
            h33Pips = new Image[UnityTuning.Hud30MaxSlots];
            for (int i = 0; i < h33Pips.Length; i++)
            {
                h33Pips[i] = UiStyle33.Slash(pips, "Pip " + i, bone, br, br, Vector2.zero, new Vector2(UnityTuning.Hud33PipW, UnityTuning.Hud33PipH));
                h33Pips[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < UnityTuning.Hud30MaxSlots; i++)
            {
                Hud33Layout.Box sl = Hud33Layout.SwitchLine(i);
                UiStyle33.Text33 line = UiStyle33.Label(hud33, sl.name, (float)sl.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.BottomRight, br, br, P(sl), S(sl));
                line.Alpha = 0f; // shown only for Hud33SwitchExpand s after a switch
                h33List.Add(line);
            }

            // --- points and rank, top-right ---
            Hud33Layout.Box pt = Hud33Layout.Points, rk = Hud33Layout.RankLetter, rl = Hud33Layout.RankLine;
            h33Points = UiStyle33.Label(hud33, "Points", (float)pt.px, bone, UiStyle33.FaceNumber, TextAlignmentOptions.TopRight, tr, tr, P(pt), S(pt), false);
            h33Rank = UiStyle33.Label(hud33, "Rank", (float)rk.px, bone, UiStyle33.FaceRank, TextAlignmentOptions.TopRight, tr, tr, P(rk), S(rk), false);
            RectTransform rlRt = UiStyle33.Rect(hud33, "Rank line", tr, tr, P(rl), S(rl));
            UiStyle33.Line(rlRt, "Track", faint, Vector2.zero, Vector2.zero, Vector2.zero, S(rl));
            h33RankLine = UiStyle33.Line(rlRt, "Fill", UiStyle33.Cyan, Vector2.zero, Vector2.zero, Vector2.zero, S(rl));
            h33RankLine.sprite = UiStyle33.WhiteSprite();
            h33RankLine.type = Image.Type.Filled;
            h33RankLine.fillMethod = Image.FillMethod.Horizontal;
            h33RankLine.fillOrigin = (int)Image.OriginHorizontal.Right; // it drains toward the letter
            rlRt.gameObject.SetActive(false);
            h33RankLineRt = rlRt;
            if (Health42On) BuildHealth42(); // NIGHT42 U1 (design.n42.health42, UiHud33.Health42.cs): the damage chunk, the heal glow, the punch
            if (Style42On) BuildStyle42(); // NIGHT42 U1 (design.n42.style42, UiHud33.Style42.cs): the letter's motion, the combo, the name, the award
            if (Hud43On) BuildHud43(); // NIGHT43 U1 (design.n43.hud43, UiHud33.Hud43.cs): the health bar rebuilt (blade cells, after-image, bracket)
            if (Counter43On) BuildCounter43(); // NIGHT43 U1 (design.n43.counter43, UiHud33.Counter43.cs): the locked room's "N LEFT"

            // --- the round line, top-left; the message slot, top-centre ---
            Hud33Layout.Box rd = Hud33Layout.Round, slot = Hud33Layout.Slot(false);
            h33Round = UiStyle33.Label(hud33, "Round", (float)rd.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, P(rd), S(rd));
            h33RunLines = new UiStyle33.Text33[UnityTuning.Hud33RunLines];
            for (int i = 0; i < h33RunLines.Length; i++)
            {
                Hud33Layout.Box rl2 = Hud33Layout.RunLine(i);
                h33RunLines[i] = UiStyle33.Label(hud33, rl2.name, (float)rl2.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.TopLeft, tl, tl, P(rl2), S(rl2));
                h33RunLines[i].main.overflowMode = h33RunLines[i].back.overflowMode = TextOverflowModes.Ellipsis; // inside its 340 px
            }
            h33Slot = UiStyle33.Label(hud33, Hud33Layout.SlotName, (float)slot.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0.5f, 1), P(slot), S(slot));
            h33Slot.main.richText = h33Slot.back.richText = true; // a lesson line carries the pad's glyph sprites ({jump} -> <sprite>)
            h33Slot.main.overflowMode = h33Slot.back.overflowMode = TextOverflowModes.Ellipsis; // one line: never wider than the column
            h33SlotScrim = UiStyle33.Scrim(h33Slot.rt, "Scrim", new Vector2(200, UnityTuning.Hud33ScrimH));
            // the lead's n33-r5 camp-door: "1.1 INTAKE" over a bright cyan ceiling panel, unreadable with the wash alone: a solid dark outline on the glyphs too
            // (TMP's SDF outline: a per-text material instance, faded with the text's vertex alpha)
            Color so = UiStyle33.Shadow;
            h33Slot.main.outlineColor = new Color32((byte)(so.r * 255), (byte)(so.g * 255), (byte)(so.b * 255), 255);
            h33Slot.main.outlineWidth = UnityTuning.Hud33SlotOutline;
            BuildText36(hud33); // NIGHT36 M (text36, UiHud36Text.cs): the teaching slot in the lower middle (nothing with the row off)

            // --- the boss bar, top-centre: the name (16 px) over a 640 x 6 bar ---
            Hud33Layout.Box bn = Hud33Layout.BossName(), bb = Hud33Layout.BossBar();
            var top = new Vector2(0.5f, 1);
            h33BossRt = UiStyle33.Rect(hud33, "Boss", top, top, Vector2.zero, new Vector2((float)bn.w, (float)(bn.h - bb.y)));
            h33BossName = UiStyle33.Label(h33BossRt, Hud33Layout.BossNameName, (float)bn.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.Top, top, top, P(bn), S(bn));
            RectTransform barRt = UiStyle33.Rect(h33BossRt, Hud33Layout.BossBarName, top, top, P(bb), S(bb));
            h33BossTrack = UiStyle33.Line(barRt, "Track", UiTheme.A(bone, UnityTuning.Hud33BossTrackAlpha), Vector2.zero, Vector2.zero, Vector2.zero, S(bb));
            h33BossFill = UiStyle33.Line(barRt, "Fill", UiStyle33.Danger, Vector2.zero, Vector2.zero, Vector2.zero, S(bb));
            h33BossFill.sprite = UiStyle33.WhiteSprite();
            h33BossFill.type = Image.Type.Filled;
            h33BossFill.fillMethod = Image.FillMethod.Horizontal;
            h33BossFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            h33BossRt.gameObject.SetActive(false);

            // --- the door marker: a chevron and "7 m" on the open door (the world, not the top centre) ---
            h33DoorRt = UiStyle33.Rect(hud33, "Door marker", Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(120, UnityTuning.Hud33DoorChevronH + UnityTuning.Hud33DoorMarkPx + 8));
            Image ca = UiStyle33.Line(h33DoorRt, "Chevron L", UiStyle33.Cyan, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-UnityTuning.Hud33DoorChevronH * 0.3f, 0), new Vector2(UnityTuning.Hud33DoorChevronW, UnityTuning.Hud33DoorChevronH));
            ca.rectTransform.pivot = new Vector2(0.5f, 0);
            ca.rectTransform.localRotation = Quaternion.Euler(0, 0, 35);
            ca.rectTransform.anchoredPosition = Vector2.zero;
            Image cb = UiStyle33.Line(h33DoorRt, "Chevron R", UiStyle33.Cyan, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(UnityTuning.Hud33DoorChevronW, UnityTuning.Hud33DoorChevronH));
            cb.rectTransform.pivot = new Vector2(0.5f, 0);
            cb.rectTransform.localRotation = Quaternion.Euler(0, 0, -35);
            h33DoorText = UiStyle33.Label(h33DoorRt, "Door marker text", UnityTuning.Hud33DoorMarkPx, UiStyle33.Cyan, UiStyle33.FaceLabel, TextAlignmentOptions.Bottom, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, UnityTuning.Hud33DoorChevronH + 4), new Vector2(120, UnityTuning.Hud33DoorMarkPx + 4), false);
            h33DoorRt.gameObject.SetActive(false);

            // --- the reticle ---
            RectTransform ret = UiStyle33.Rect(hud33, "Reticle", mid, mid, Vector2.zero, Vector2.zero);
            h33Dot = UiStyle33.Line(ret, "Dot", bone, mid, mid, Vector2.zero, new Vector2(UnityTuning.Hud33Dot, UnityTuning.Hud33Dot));
            h33ArcA = UiStyle33.Arc(ret, "Charge arc right", UiStyle33.Cyan, UnityTuning.Hud33ArcR, UnityTuning.Hud33ArcW, true);
            h33ArcB = UiStyle33.Arc(ret, "Charge arc left", UiStyle33.Cyan, UnityTuning.Hud33ArcR, UnityTuning.Hud33ArcW, false);
            h33ArmHolder = UiStyle33.Rect(ret, "Ticks", mid, mid, Vector2.zero, Vector2.zero);
            h33Arms = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                bool horizontal = i < 2;
                h33Arms[i] = UiStyle33.Line(h33ArmHolder, "Tick " + i, bone, mid, mid, Vector2.zero, horizontal ? new Vector2(UnityTuning.Hud33ArmLen, UnityTuning.Hud33ArmW) : new Vector2(UnityTuning.Hud33ArmW, UnityTuning.Hud33ArmLen));
            }
            PlaceArms33(0);
            h33TickColor = bone;
            h33GrFill = new Image[2];
            h33GrTrack = new Image[2];
            for (int i = 0; i < 2; i++)
            {
                var at = new Vector2((i - 0.5f) * UnityTuning.Hud33GrapplePitch, UnityTuning.Hud33GrappleY);
                h33GrFill[i] = UiStyle33.SlashFill(ret, "Grapple " + i, UiStyle33.Cyan, mid, mid, at, new Vector2(UnityTuning.Hud33TickW, UnityTuning.Hud33TickH), out h33GrTrack[i], UiTheme.A(UiStyle33.Cyan, UnityTuning.Hud33GrappleSpentAlpha));
                h33GrFill[i].gameObject.SetActive(false);
                h33GrTrack[i].gameObject.SetActive(false);
            }
            if (Hud38On) BuildHud38(ret); // NIGHT38 H (design.n38.hud38, UiHud38.cs): the charge bar, the grapple's hook row (its two slash ticks above hide)

            // --- events (the reticle's grammar, the health flash) ---
            game.events.On<HitEvent>("hit", e =>
            {
                if (e.owner != "player") return;
                h33HitLeft = UnityTuning.Hud33HitFlashTime;
                Hud33HitFlashes++;
            });
            game.events.On<KillEvent>("kill", e =>
            {
                if (e.owner != "player") return;
                h33KillAge = 0;
                Hud33KillTurns++;
            });
            game.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner == "player") h33HurtLeft = UnityTuning.Hud33HitFlash;
            });
            // a boss's teaching line (lessonBegin id "boss") rides the slot's boss channel, any other lesson its lesson channel
            game.events.On<LessonEvent>("lessonBegin", e => h33BossRaw = e.id == "boss" && !string.IsNullOrEmpty(e.prompt) ? e.prompt : null);
            game.OnNewRun += () =>
            {
                h33Msg.Clear();
                h33BossRaw = null;
                h33ShownHeld = null;
                h33SwitchAge = double.MaxValue;
                h33KillAge = double.MaxValue;
                h33HitLeft = h33HurtLeft = 0;
                Hud33HitFlashes = Hud33KillTurns = Hud33Switches = 0;
            };
            if (Rail34On) BuildRail34(); // NIGHT34 H (design.n34.rail34): the arsenal rail in place of the pip row (UiRail34.cs)
            if (N34Fixes.ViewRowOn(cfg, "bendLook")) BuildBendMeter34(); // NIGHT34 H: the bend meter's slash beside the grapple ticks (UiRail34.cs)
            if (N34Fixes.ViewRowOn(cfg, "bendLook") || Rail34On) WireCombo34(); // NIGHT34 H: B's bend34Combo name in the message slot (UiRail34.cs)
            if (Hud47On) BuildHud47(); // NIGHT47 F (design.n47.hud47, UiHud47.cs): the continuous bar, the counter, the reticle alone, the text sizes
            if (Hud52On) BuildHud52(); // NIGHT52 U (design.n52.hud52, UiHud52.cs): the health and weapon blocks as hard-edged ink panels, the hit shake, the heal pulse
        }

        RectTransform h33RankLineRt;

        static Vector2 P(Hud33Layout.Box b) => new Vector2((float)b.x, (float)b.y);
        static Vector2 S(Hud33Layout.Box b) => new Vector2((float)b.w, (float)b.h);

        void Keep33(Transform t)
        {
            if (t == null) return;
            t.SetParent(hud33Kept, false);
        }

        void PlaceArms33(float push)
        {
            float d = UnityTuning.Hud33ArmGap + UnityTuning.Hud33ArmLen * 0.5f + push;
            h33Arms[0].rectTransform.anchoredPosition = new Vector2(-d, 0);
            h33Arms[1].rectTransform.anchoredPosition = new Vector2(d, 0);
            h33Arms[2].rectTransform.anchoredPosition = new Vector2(0, d);
            h33Arms[3].rectTransform.anchoredPosition = new Vector2(0, -d);
        }

        // ================================================================== render ==================================================================

        /// <summary>Every frame, every flow (from Render, after RenderHud30): builds on first use with the row on; shows while a run is on screen.</summary>
        void RenderHud33(Flow f, double frameDt)
        {
            if (!Hud33On) { Hud33Live = false; return; }
            h33Clock += frameDt; // the slot's clock: the HUD's frame time (the reels' FeelClock steps it as the frames are recorded)
            if (hud33 == null) BuildHud33Now();
            bool on = f == Flow.Playing || f == Flow.Down;
            if (hud33.gameObject.activeSelf != on) hud33.gameObject.SetActive(on);
            Hud33Live = on;
            if (!on) { Rail34Live = false; return; }
            Health33(frameDt);
            Weapon33(frameDt);
            Points33(frameDt);
            Round33();
            if (c43Root != null) Counter43(frameDt); // NIGHT43 U1 counter43 (UiHud33.Counter43.cs)
            Reticle33(frameDt);
            RenderBendMeter34(); // NIGHT34 H: nothing without bendLook (never built)
            float cw = hud33.rect.width;
            if (cw != h33ShownCw) Width33(cw);
            Boss33(frameDt);
            Door33();
            Slot33(frameDt);
            RenderCard38(frameDt); // NIGHT38 H (juice38, UiJuice38.cs): a boss entrance's name card; nothing until the first entrance with the row on
            if (hud47Built) Fight47Late(frameDt); // NIGHT47 F hud47 (UiHud47.Boss.cs): one fight line at a time while a boss bar is up
            if (hud47Built) FightLine64Late(); // NIGHT64 G fightLine64 (UiFightLine64.N64G.cs): no room title drawn as the fight line mid-fight (held for after it); its counter, the row on or off
            Words71Late(); // NIGHT71 B words71 (UiWords71.cs): a boss order naming a weapon not held shown as its variant in place (as before with the row off); its counter, the row on or off
            if (h52Built) Top52(); // NIGHT52 U hud52 round 11 (UiHud52.Top.cs): the counter, the score and the room title on their plates
        }

        /// <summary>The top-centre column's width follows the canvas (a 4:3 screen narrows it clear of the round line and the points).</summary>
        void Width33(float cw)
        {
            h33ShownCw = cw;
            float w = (float)Hud33Layout.TopCentreW(cw);
            h33Slot.rt.sizeDelta = new Vector2(w, h33Slot.rt.sizeDelta.y);
            h33BossName.rt.sizeDelta = new Vector2(w, h33BossName.rt.sizeDelta.y);
            RectTransform bar = h33BossFill.rectTransform.parent as RectTransform;
            float bw = (float)Hud33Layout.BossBar(cw).w;
            bar.sizeDelta = new Vector2(bw, bar.sizeDelta.y);
            h33BossTrack.rectTransform.sizeDelta = h33BossFill.rectTransform.sizeDelta = new Vector2(bw, UnityTuning.Hud33BossBarH);
        }

        void Boss33(double frameDt)
        {
            HudReads20.ThreatNow(game, ref h33Threat);
            bool on = h33Threat.valid && h33Threat.boss && h33Threat.maxHp > 0;
            bool drawn = on && !N41View.BossBar41Live; // NIGHT41 (the lead's hook): bars41's bar draws instead while it runs (the slot keeps its place: Hud33BossBarShown = on)
            if (h33BossRt.gameObject.activeSelf != drawn)
            {
                h33BossRt.gameObject.SetActive(drawn);
                h33BossAge = drawn ? 0 : -1;
                h33ShownBossFill = -1;
                h33ShownBoss = null; // the lead's n33-r5: the same boss again (the 720 pass's STAG) kept its old name reference, so Hud33BossNameNow stayed ""
            }
            Hud33BossBarShown = on;
            if (!on) { Hud33BossNameNow = ""; return; }
            if (!ReferenceEquals(h33Threat.name, h33ShownBoss))
            {
                h33ShownBoss = h33Threat.name;
                h33BossName.Set(h33Threat.name ?? "");
                Hud33BossNameNow = h33Threat.name ?? "";
            }
            float fill = Mathf.Round(Mathf.Clamp01((float)(h33Threat.hp / h33Threat.maxHp)) * 500f) / 500f;
            if (fill != h33ShownBossFill)
            {
                h33ShownBossFill = fill;
                h33BossFill.fillAmount = fill;
            }
            if (h33BossAge >= 0)
            {
                // slides in from 12 px up and fades in over 200 ms, then rests (motion only on the event)
                h33BossAge += frameDt;
                float k = h33BossAge >= UnityTuning.Hud33BossSlideIn ? 1f : UiTween.OutCubic(h33BossAge / UnityTuning.Hud33BossSlideIn);
                h33BossRt.anchoredPosition = new Vector2(0, (1 - k) * UnityTuning.Hud33BossSlide);
                h33BossName.Alpha = k;
                h33BossTrack.color = UiTheme.A(UiStyle33.Bone, UnityTuning.Hud33BossTrackAlpha * k);
                h33BossFill.color = UiTheme.A(UiStyle33.Danger, k);
                if (k >= 1f) h33BossAge = -1;
            }
        }

        /// <summary>The door hint: the slot says DOOR OPEN (its door channel) while the cleared room's door stands open and the player is away from it; the
        /// distance stands on the door itself (a chevron and "7 m" over the room's exit) while that point is on screen.</summary>
        void Door33()
        {
            SurvivalSim sim = game.sim;
            Campaign24 c = sim != null && sim.campaignOn ? sim.campaign : null;
            bool open = false;
            double dist = 0;
            Vec3 exit = default;
            if (c != null && game.flow == Flow.Playing && c.phase == Campaign24.Phase.Cleared && !c.finished && c.Room != null)
            {
                exit = c.Room.exit;
                Vec3 feet = game.Feet;
                dist = Math.Sqrt((exit.x - feet.x) * (exit.x - feet.x) + (exit.z - feet.z) * (exit.z - feet.z));
                open = dist > cue24Hide;
            }
            // NIGHT72 N3 (goal72, Goal72View.cs): while goal72's line is up it IS the door hint (the line at the exit, its marker, the edge arrow): this slot line and the chevron over the door
            // give way, so one line and one marker are on screen (the row off: Goal72Probe.Visible is false and everything below is as it was)
            if (Goal72Probe.Visible) open = false;
            h33Msg.Want(MessageSlot33.Door, open ? UnityTuning.Hud33DoorLine : null);
            bool mark = false;
            Camera cam = game.cam;
            if (open && cam != null)
            {
                Vector3 p = cam.WorldToScreenPoint(WebSpace.ToUnity(new Vec3(exit.x, exit.y + UnityTuning.Hud33DoorMarkUp, exit.z)));
                float edge = UnityTuning.Hud33DoorEdge * (UnityEngine.Screen.height / 1080f);
                mark = p.z > 0 && p.x > edge && p.x < UnityEngine.Screen.width - edge && p.y > edge && p.y < UnityEngine.Screen.height - edge;
                if (mark)
                {
                    float sf = canvas.scaleFactor > 0 ? canvas.scaleFactor : 1f;
                    h33DoorRt.anchoredPosition = new Vector2(Mathf.Round(p.x / sf), Mathf.Round(p.y / sf));
                    int m = (int)dist;
                    if (m != h33ShownDoorM)
                    {
                        h33ShownDoorM = m;
                        h33DoorText.Set(m.ToString(CultureInfo.InvariantCulture) + " m"); // on a metre's change only
                    }
                }
            }
            if (h33DoorRt.gameObject.activeSelf != mark) h33DoorRt.gameObject.SetActive(mark);
            Hud33DoorMarkShown = mark;
            Hud33DoorMetres = mark ? h33ShownDoorM : -1;
        }

        void Health33(double frameDt)
        {
            Health h = game.health;
            if (hp43Built) Health43Draw(h, frameDt); // NIGHT43 U1 hud43 (UiHud33.Hud43.cs): the blade bar (health42 does not draw under it)
            else if (hp42Built) Health42Draw(h, frameDt); // NIGHT42 U1 health42 (UiHud33.Health42.cs): the number and the ticks in motion
            else Health33Ticks(h, frameDt);
            Stamina33();
        }

        /// <summary>hud33's number and ticks (health42 off).</summary>
        void Health33Ticks(Health h, double frameDt)
        {
            float frac = (float)(h.hp / Math.Max(1, h.max));
            int hp = (int)Math.Ceiling(h.hp);
            if (hp != h33ShownHp)
            {
                h33ShownHp = hp;
                h33Health.Set(hp.ToString(CultureInfo.InvariantCulture));
            }
            h33HurtLeft = Math.Max(0, h33HurtLeft - frameDt);
            int lit = Mathf.Clamp(Mathf.CeilToInt(frac * UnityTuning.Hud33HealthTicks - 1e-4f), 0, UnityTuning.Hud33HealthTicks);
            bool red = h33HurtLeft > 0 || frac <= UnityTuning.Hud33LowFrac;
            if (lit != h33ShownLit || red != h33HpRed)
            {
                h33ShownLit = lit;
                h33HpRed = red;
                Color on = red ? UiStyle33.Danger : UiStyle33.Bone, off = UiTheme.A(UiStyle33.Bone, UnityTuning.Hud33GrappleSpentAlpha);
                for (int i = 0; i < h33HpTicks.Length; i++) h33HpTicks[i].color = i < lit ? on : off;
                h33Health.Color = red ? UiStyle33.Danger : UiStyle33.Bone;
            }
        }

        void Stamina33()
        {
            // the stamina: one thin slash bar per full boost (two with the rules' bar), each filling in turn
            StaminaHud.Read(game, ref h33Stam);
            if (!h33Stam.valid) return;
            double cost = Math.Max(1e-6, h33Stam.boostCost);
            int n = Mathf.Clamp((int)Math.Round(h33Stam.max / cost), 1, h33StamFill.Length);
            if (n != h33ShownStamN)
            {
                h33ShownStamN = n;
                float total = 2 * UnityTuning.Hud33StaminaW + UnityTuning.Hud33StaminaGap;
                float w = (total - (n - 1) * UnityTuning.Hud33StaminaGap) / n;
                for (int i = 0; i < h33StamFill.Length; i++)
                {
                    bool show = i < n;
                    h33StamFill[i].gameObject.SetActive(show);
                    h33StamTrack[i].gameObject.SetActive(show);
                    var pos = new Vector2(i * (w + UnityTuning.Hud33StaminaGap), 0);
                    var size = new Vector2(w, UnityTuning.Hud33StaminaH);
                    h33StamFill[i].rectTransform.anchoredPosition = h33StamTrack[i].rectTransform.anchoredPosition = pos;
                    h33StamFill[i].rectTransform.sizeDelta = h33StamTrack[i].rectTransform.sizeDelta = size;
                }
            }
            for (int i = 0; i < n; i++)
            {
                float share = Mathf.Clamp01((float)((h33Stam.fill - i * cost) / cost));
                share = Mathf.Round(share * 100f) / 100f;
                if (h33StamFill[i].fillAmount != share) h33StamFill[i].fillAmount = share;
                Color c = share >= 1f ? UiStyle33.Cyan : UiStyle33.CyanDeep;
                if (h33StamFill[i].color != c) h33StamFill[i].color = c;
            }
        }

        string LtWord33(string id)
        {
            Loadout lo = game.loadout;
            if (lo == null || lo.lt31 == null || !lo.lt31.active) return "";
            string w = lo.LtLabel31(id);
            if (string.IsNullOrEmpty(w)) return "";
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            int glyphs = (int)PlayerDevices.Glyphs;
            object layout = PlayerDevices.Layout;
            if (!ReferenceEquals(layout, h33LtLayout) || glyphs != h33LtGlyphs)
            {
                h33LtLayout = layout; // a remap or another pad: the button words again
                h33LtGlyphs = glyphs;
                h33LtWords.Clear();
                h33LtLastWord = null;
            }
            if (ReferenceEquals(w, h33LtLastWord) && pad == h33LtLastPad) return h33LtLast; // no string work on a frame where nothing changed
            string key = (pad ? "p:" : "k:") + w;
            if (!h33LtWords.TryGetValue(key, out string s))
            {
                // the button that carries the LT action, read from the bindings: on a pad, PadAction.AltFire's button in the live layout and glyph set
                // (Gamepad.cs: LT in both default layouts; a remap follows it; "LT" / "L2" / "ZL" by the set); on mouse and keyboard the alt fire is the
                // right button, hard-wired in CombatReader.cs (:131, :176: mouse.rightButton -> altFire; not rebindable), so "RMB"
                string btn = pad ? Glyphs.Label("altFire", InputDevice.Pad, PlayerDevices.Glyphs, null, PlayerDevices.Layout ?? GamepadMath.DefaultLayoutV2) : UnityTuning.Hud33LtKey;
                if (string.IsNullOrEmpty(btn)) btn = UnityTuning.Hud33LtPad;
                h33LtWords[key] = s = btn + " " + w;
            }
            h33LtLastWord = w;
            h33LtLastPad = pad;
            h33LtLast = s;
            return s;
        }

        void Weapon33(double frameDt)
        {
            Loadout lo = game.loadout;
            if (lo == null) return;
            string held = lo.Weapon;
            // NIGHT34 rail34: with the rail, the name and the LT label follow the press (Loadout.targetSlot) as the rail's highlight does, so they never disagree
            if (r34Root != null && lo.targetSlot >= 0 && lo.targetSlot < lo.owned.Count) held = lo.owned[lo.targetSlot];
            int n = Math.Min(lo.owned.Count, h33Pips.Length);
            int heldIdx = -1;
            for (int i = 0; i < n; i++) if (lo.owned[i] == held) { heldIdx = i; break; }
            if (held != h33ShownHeld)
            {
                if (h33ShownHeld != null && !string.IsNullOrEmpty(held)) { h33SwitchAge = 0; Hud33Switches++; }
                h33ShownHeld = held;
                Hud33WeaponNow = string.IsNullOrEmpty(held) ? "" : WeaponName(held);
                h33Weapon.Set(Hud33WeaponNow);
                h33ShownPipN = -1; // relayout the pips and the list
            }
            string ltNow = string.IsNullOrEmpty(held) ? "" : LtWord33(held);
            if (!ReferenceEquals(ltNow, Hud33LtNow))
            {
                Hud33LtNow = ltNow;
                h33Lt.Set(ltNow);
            }
            if (n != h33ShownPipN || heldIdx != h33ShownHeldIdx)
            {
                h33ShownPipN = n;
                h33ShownHeldIdx = heldIdx;
                for (int i = 0; i < h33Pips.Length; i++)
                {
                    bool show = i < n && n > 1; // one weapon: no strip (nothing to switch to)
                    if (h33Pips[i].gameObject.activeSelf != show) h33Pips[i].gameObject.SetActive(show);
                    if (!show) continue;
                    bool isHeld = i == heldIdx;
                    h33Pips[i].color = isHeld ? UiStyle33.Cyan : UiStyle33.Bone;
                    h33Pips[i].rectTransform.sizeDelta = new Vector2(isHeld ? UnityTuning.Hud33PipHeldW : UnityTuning.Hud33PipW, UnityTuning.Hud33PipH);
                    h33Pips[i].rectTransform.anchoredPosition = new Vector2(-(n - 1 - i) * UnityTuning.Hud33PipPitch, 0);
                }
                for (int i = 0; i < h33List.Count; i++)
                {
                    // line 0 is the bottom: the owned order reads top to bottom
                    int w = n - 1 - i;
                    h33List[i].Set(w >= 0 && w < n ? WeaponName(lo.owned[w]) : "");
                    h33List[i].Color = w == heldIdx ? UiStyle33.Cyan : UiStyle33.Bone;
                }
            }
            // the switch: the list for Hud33SwitchExpand s, the name sliding in
            bool rail = r34Root != null; // NIGHT34 rail34: the rail is the list, and the swap's read (no list, no name slide)
            bool listOn = !rail && h33SwitchAge < UnityTuning.Hud33SwitchExpand && n > 1;
            if (h33SwitchAge < double.MaxValue) h33SwitchAge += frameDt;
            if (listOn != Hud33SwitchListShown)
            {
                Hud33SwitchListShown = listOn;
                foreach (UiStyle33.Text33 l in h33List) l.Alpha = listOn ? 1f : 0f;
                h33Weapon.Alpha = listOn ? 0f : 1f; // the lead's n33-r1 switch frame: the held name twice (the list's cyan line and the big name): one at a time
            }
            // the big name slides in as the list closes (it was hidden under the list)
            double sinceList = h33SwitchAge - UnityTuning.Hud33SwitchExpand;
            float slide = !rail && n > 1 && sinceList >= 0 && sinceList < UnityTuning.Hud33SwitchSlideTime ? (float)(1 - UiTween.OutCubic(sinceList / UnityTuning.Hud33SwitchSlideTime)) * UnityTuning.Hud33SwitchSlide : 0f;
            Vector2 home = P(rail ? Hud33Layout.RailWeaponName : Hud33Layout.WeaponName) + new Vector2(slide, 0);
            if (h33Weapon.rt.anchoredPosition != home) h33Weapon.rt.anchoredPosition = home;
            if (rail) RenderRail34(frameDt);
            if (rail && hud47Built) Rail47(); // NIGHT47 F hud47 (UiHud47.cs): the PULSE pips, the BLAST cooldown, the STITCHER heat on the rail
            else Rail34Live = false;
            if (h52Built) Weapon52(frameDt); // NIGHT52 U hud52 (UiHud52.cs): the weapon panel fitted to its contents, the swap's kick
        }

        void Points33(double frameDt)
        {
            long pts = (long)Math.Max(0, Math.Round(game.economy.total));
            if (pts != h33ShownPoints)
            {
                h33ShownPoints = pts;
                h33Points.Set(pts.ToString("N0", CultureInfo.InvariantCulture));
            }
            StyleMeter st = game.style;
            if (s42Rank != null) { Style42Draw(st, frameDt); return; } // NIGHT42 U1 style42 (UiHud33.Style42.cs): the animated meter draws instead
            bool show = st != null && st.meter > 0;
            int rank = show ? st.rank : -1;
            if (show)
            {
                StyleMath.S sp = st.Params;
                if (rank != h33ShownRank)
                {
                    h33ShownRank = rank;
                    h33Rank.Set(rank >= 0 && rank < sp.letters.Count ? sp.letters[rank] : "");
                }
                double lo = sp.thresholds != null && rank < sp.thresholds.Length ? sp.thresholds[rank] : 0;
                double hi = sp.thresholds != null && rank + 1 < sp.thresholds.Length ? sp.thresholds[rank + 1] : sp.max;
                float fill = hi > lo ? Mathf.Clamp01((float)((st.meter - lo) / (hi - lo))) : 1f;
                fill = Mathf.Round(fill * 100f) / 100f;
                if (h33RankLine.fillAmount != fill) h33RankLine.fillAmount = fill;
            }
            else if (h33ShownRank != -1)
            {
                h33ShownRank = -1;
                h33Rank.Set("");
            }
            if (h33RankLineRt.gameObject.activeSelf != show) h33RankLineRt.gameObject.SetActive(show);
            Hud33RankShown = show;
        }

        void Round33()
        {
            // From the NIGHT32 top line's state (its values a frame late: they are written after this in Render). The lead's n33-r1: reading the TMP
            // line's .text gave nothing (a SetText(format, args) line), so the lines are built here, on a state change only:
            //   the fight's line top-left ("ROUND 3 · 7 LEFT"); the break's countdown ("ROUND 2 IN 6", with the READY key when it can be skipped) in the
            //   message slot as a live room-title line (it ticks in place). A campaign's break (rb -2) and a boss fight (rb -1) have neither.
            long key = (((((long)shownRoundA * 1000003L + shownRoundB) * 31L + (int)shownRunState) * 2L + (shownReady ? 1L : 0L)) * 2L + (breakReadyPad ? 1L : 0L)) * 1000003L + shownShapeKey;
            if (key != h33RoundKey)
            {
                h33RoundKey = key;
                // the survival extras that rode the NIGHT32 top line (UiRunShape: the objective, the PROTOCOL's step, exfil), one small line each under the round
                // line (the lead's round-3 note: nothing the player can use is lost)
                h33Sb.Clear();
                if (shownShapeKey != 0) AppendRunShape(h33Sb, shownRunState == SurvivalRun.State.Fight);
                string[] parts = h33Sb.Length > 0 ? h33Sb.ToString().Split(new[] { " · " }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
                for (int i = 0; i < h33RunLines.Length; i++) h33RunLines[i].Set(i < parts.Length ? parts[i].Trim() : "");
                string word = game.setup.mode == RunSetup.Grind ? "WAVE " : "ROUND ";
                bool fight = shownRunState == SurvivalRun.State.Fight && shownRoundB >= 0 && shownRoundA > 0;
                bool brk = shownRunState == SurvivalRun.State.Break && shownRoundB >= 0 && shownRoundA > 0;
                h33Round.Set(fight ? word + shownRoundA + " · " + shownRoundB + " LEFT" : "");
                h33Countdown = brk ? word + shownRoundA + " IN " + shownRoundB + (shownReady ? breakReadySuffix : "") : null;
            }
            h33Msg.WantLive(MessageSlot33.Title, h33Countdown);
        }

        string h33Countdown;
        readonly System.Text.StringBuilder h33Sb = new System.Text.StringBuilder(128); // pure C#: safe in an initialiser
        UiStyle33.Text33[] h33RunLines;

        void Reticle33(double frameDt)
        {
            SurvivalSim sim = game.sim;
            Loadout lo = game.loadout;
            if (h38Root != null) Hud38Step(frameDt); // NIGHT38 H (UiHud38.cs): the bar, the grapple row; sets the reload spin (h38Spin) and the snap (h38Push, h38White)
            // the hit's flash and push
            h33HitLeft = Math.Max(0, h33HitLeft - frameDt);
            float push = Math.Max(h33HitLeft > 0 ? UnityTuning.Hud33HitGrow : 0f, h38Push);
            PlaceArms33(push);
            // the kill's turn (in, hold, out)
            float ang = 0f;
            if (h33KillAge < double.MaxValue)
            {
                double a = h33KillAge, tin = UnityTuning.Hud33KillTurnIn, hold = UnityTuning.Hud33KillHold, tout = UnityTuning.Hud33KillTurnOut;
                double k = a < tin ? UiTween.OutCubic(a / tin) : a < tin + hold ? 1 : a < tin + hold + tout ? 1 - UiTween.OutCubic((a - tin - hold) / tout) : 0;
                ang = 45f * (float)k;
                h33KillAge += frameDt;
                if (a >= tin + hold + tout) h33KillAge = double.MaxValue;
            }
            Hud33KillAngleNow = ang;
            h33ArmHolder.localRotation = Quaternion.Euler(0, 0, ang - h38Spin); // NIGHT38: minus the reload spin (clockwise on screen; 0 with hud38 off)
            // the charge: two arcs close on the dot
            float charge = 0f;
            ChargeShots27 cs = sim != null && lo != null && (sim.n27.chargeShots27 || sim.n31.charge) && lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
            if (cs != null && cs.charging)
            {
                int gi = ChargeShots27.Index(lo.Weapon);
                if (gi >= 0) charge = (float)cs.Level(gi);
            }
            charge = Mathf.Round(charge * 50f) / 50f;
            Hud33ChargeNow = charge;
            float half = hud47Built || h49El != null ? 0f : charge * 0.5f; // NIGHT49 H charge49 (UiHud38.Arc40.cs): the one element carries the charge, the dot's arcs stay empty (never two at once). NIGHT47 F hud47: the dot's two charge arcs go (GG: "the charged attack indicator"); the charge reads on the gun and the rail (the ticks' cyan at full stays)
            if (h33ArcA.fillAmount != half) { h33ArcA.fillAmount = half; h33ArcB.fillAmount = half; }
            // the ticks' colour: a hit's white flash > the aim-assist on an enemy (red-orange) > ready (a full charge, the momentum at IMPACT: cyan) > bone
            if (double.IsNaN(h33ArcAt)) h33ArcAt = cfg.Has("momentum.arcAt") ? cfg.Num("momentum.arcAt") : double.PositiveInfinity;
            bool impact = lo != null && lo.momentum.enabled && lo.AimMomentum() > h33ArcAt;
            bool locked = game.Active && game.assist != null && game.assist.Info() != null;
            Color tc = h33HitLeft > 0 || h38White ? Color.white : locked ? UiStyle33.Danger : charge >= 1f || impact ? UiStyle33.Cyan : UiStyle33.Bone;
            if (tc != h33TickColor)
            {
                h33TickColor = tc;
                for (int i = 0; i < 4; i++) h33Arms[i].color = tc;
            }
            // the grapple's charges: two slash ticks under the reticle (GrapplePips32 folded in)
            bool gr = sim != null && sim.Grapple32On && sim.flow == Flow.Playing && sim.arcline21 != null && sim.arcline21.Owned;
            int full = -1;
            for (int i = 0; i < 2; i++)
            {
                bool show = gr && i < sim.grapple32Rules.charges && h38Root == null; // NIGHT38 hud38: the hook row (UiHud38.cs) shows the charges instead
                if (h33GrFill[i].gameObject.activeSelf != show) { h33GrFill[i].gameObject.SetActive(show); h33GrTrack[i].gameObject.SetActive(show); }
            }
            if (gr)
            {
                int n = Math.Min(2, sim.grapple32Rules.charges);
                full = Mathf.Clamp(sim.grappleCharges32, 0, n);
                float next = sim.grappleRechargeSteps32 > 0 && full < n ? 1f - (float)sim.grappleRechargeLeft32 / sim.grappleRechargeSteps32 : 0f;
                for (int i = 0; i < n; i++)
                {
                    float share = i < full ? 1f : i == full ? Mathf.Round(next * 50f) / 50f : 0f;
                    if (h33GrFill[i].fillAmount != share) h33GrFill[i].fillAmount = share;
                    Color c = i < full ? UiStyle33.Cyan : UiStyle33.CyanDeep;
                    if (h33GrFill[i].color != c) h33GrFill[i].color = c;
                }
            }
            Hud33GrappleFull = full;
        }

        double h33SlotShownAt, h33Clock;
        int h33SlotShowsSeen;

        /// <summary>THE MESSAGE SLOT (MessageSlot33's rule): the posts (Ui.Banner's lines, the ROOM n banner) are queued as they come; the wants are read here each
        /// frame: EXPOSED (danger), the lesson line (a boss's teaching line on the boss channel), the door hint (Door33). One line, bone (red-orange for
        /// danger), a 3-frame flicker-on, 120 ms out; under the boss bar while a boss fights, under the chase gap bar while it shows.</summary>
        void Slot33(double frameDt)
        {
            double now = h33Clock;
            h33Msg.Want(MessageSlot33.Danger, game.director != null && game.director.Exposed ? UnityTuning.Hud33DangerExposed : game.sim != null && game.sim.bosses33Armor ? UnityTuning.StillArmor33Line : null); // NIGHT33 E: "MOVE" while a boss's STILL ARMOR is up
            string lesson = lesson25Rt != null && lesson25Rt.gameObject.activeSelf && lesson25Want && lesson25Text != null ? lesson25Text.text : null;
            bool bossLine = lesson != null && h33BossRaw != null && lesson25Raw == h33BossRaw;
            string teach = Teach35Want(frameDt, Teach35SlotNow); // NIGHT35 T (teach35, UiTeach35.cs): the teaching line rides the teaching channel after a boss's own
            PinHint63Slot(ref lesson, ref bossLine, ref teach); // NIGHT63 G (UiPinHint63.cs): a reel's -skygrindPinHint63 feeds the pinned line instead (nothing without it)
            Words71Slot(ref lesson, ref teach); // NIGHT71 B words71 (UiWords71.cs): a lesson or teaching line naming a weapon not held waits (nothing with the row off)
            if (t36On) // NIGHT36 M (text36, UiHud36Text.cs): the boss's line, the teaching line and the lesson go to the teaching slot in the lower middle
            {
                Slot36(frameDt, now, lesson, bossLine, teach);
                h33Msg.Want(MessageSlot33.Boss, null);
                h33Msg.Want(MessageSlot33.Lesson, null);
            }
            else
            {
                if (bossLine || teach == null) h33Msg.Want(MessageSlot33.Boss, bossLine ? lesson : null);
                else h33Msg.WantLive(MessageSlot33.Boss, teach); // live: a device change re-reads its button in place
                h33Msg.Want(MessageSlot33.Lesson, bossLine ? null : lesson);
            }
            string s = h33Msg.Step(now);
            Hud33SlotShows = h33Msg.Shows;
            Hud33SlotDropped = h33Msg.Dropped;
            if (s != null && (s != h33SlotText || h33Msg.Prio != h33SlotPrio))
            {
                bool inPlace = h33Msg.Shows == h33SlotShowsSeen && h33Msg.Prio == h33SlotPrio && h33SlotFade > 0; // a live line ticking ("ROUND 2 IN 6"): same message
                if (!inPlace)
                {
                    if (!string.IsNullOrEmpty(h33SlotText) && h33SlotFade >= 1) Hud33SlotMinHeldSeen = Math.Min(Hud33SlotMinHeldSeen, now - h33SlotShownAt); // a line replaced: how long it stood
                    h33SlotShownAt = now;
                    h33SlotFade = 0;
                }
                h33SlotText = s;
                h33SlotPrio = h33Msg.Prio;
                if (!t36On) Teach35Case(s); // NIGHT35 T (with text36 on, the teaching slot sets its own case: UiHud36Text.cs): a teaching line keeps its case (the verb in capitals); every other line is caps as before
                h33Slot.Set(s);
                h33Slot.Color = h33SlotPrio == MessageSlot33.Danger ? UiStyle33.Danger : h34ComboText != null && s == h34ComboText ? UiStyle33.Cyan : UiStyle33.Bone; // NIGHT34: a bend combo's name in cyan (UiRail34.cs)
                float tw = Mathf.Min(h33Slot.main.GetPreferredValues(s).x, h33Slot.rt.sizeDelta.x); // on a change only
                h33SlotScrim.rectTransform.sizeDelta = new Vector2(tw + 2 * UnityTuning.Hud33ScrimPadX, UnityTuning.Hud33ScrimH);
            }
            h33SlotShowsSeen = h33Msg.Shows;
            Hud33SlotPrioNow = s != null ? h33SlotPrio : -1;
            if (s != null) h33SlotFade = Math.Min(1, h33SlotFade + frameDt / UnityTuning.Hud33SlotFadeIn);
            else if (h33SlotFade > 0)
            {
                h33SlotFade = Math.Max(0, h33SlotFade - frameDt / UnityTuning.Hud33SlotFadeOut);
                if (h33SlotFade <= 0)
                {
                    h33SlotText = "";
                    h33SlotPrio = -1;
                    h33Slot.Set("");
                }
            }
            Hud33SlotNow = h33SlotFade > 0 ? h33SlotText : "";
            float sa = Mathf.Round((float)h33SlotFade * 50f) / 50f;
            if (sa != h33Slot.Alpha)
            {
                h33Slot.Alpha = sa;
                Color sc = UiStyle33.Shadow;
                h33SlotScrim.color = new Color(sc.r, sc.g, sc.b, UnityTuning.Hud33ScrimAlpha * sa);
            }
            int place = Hud29.BossGapMetres >= 0 ? 2 : Bar43Live ? 3 : Hud33BossBarShown ? 1 : 0; // NIGHT43 U1 bossbar43: under its taller block
            if (place != h33SlotPlace)
            {
                h33SlotPlace = place;
                h33Slot.rt.anchoredPosition = place == 3 ? new Vector2(0, -UnityTuning.Bar43SlotY) : P(Hud33Layout.Slot(place == 1, place == 2, h33ShownCw > 0 ? h33ShownCw : 1920));
            }
        }
    }
}
