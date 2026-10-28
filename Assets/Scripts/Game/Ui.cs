// The HUD and the screens (uGUI + TextMeshPro, built at load like the map), after skygrind/src/ui/hud.js, menus.js and
// theme.js (the parts ported so far). The theme's tokens come from config.ui.theme (text, gold, panel + opacity, edge
// width, the type scale; nothing under 16 px at 1080p), cyan is player-usable, red-orange only for danger (the hurt
// vignette, EXPOSED), the edge margin and panel skew from config.hud. The canvas scales with the screen from 1920 x 1080
// (the web's UI scale: 1 up to 1080 px tall, then with the height).
// NIGHT12 U3-25.6 (D-13): the theme fonts (theme.js FONT_UI / FONT_DISPLAY): Barlow Semi Condensed for everything you read,
// Chakra Petch for display (the wordmark, headings, big numbers). TMP font assets with a static atlas, made once by
// SKYGRIND/Make Theme Fonts (Assets/Editor/ThemeFonts.cs) into Resources/Fonts and loaded here; Text() and Button() apply
// them (so every partial's screens get them), TMP's default when an asset is missing. Bold / italic go to the real faces
// through each asset's weight table (Barlow 700, Chakra Petch 700 italic). Every string stays inside the static charset
// (ASCII, Latin-1, a few marks), so no glyph reaches TMP's dynamic fallback asset.
// HUD: crosshair + hit marker (hud.hitMarkerTime, headshotScale, killScale), HP bar (hud.hpBarWidth, low-HP blink),
// stamina segments (hud.js: hud.staminaSegmentWidth each, the partial regen as a fill), points, the weapon, ROUND N +
// enemies left / the break countdown, the buy prompt (HOLD E, name, price, the hold's progress, NOT ENOUGH POINTS),
// banners (the round, the zone you enter, in its accent), EXPOSED, the hurt vignette; the aim-assist lock (main.js
// updateLockBrackets + hud.js setLock: four white corners with a danger glow around the target's 2.2 m box, tightening
// from aim.lock.bracketFrom over bracketTime, x eliteScale on elites, pulsing once SNAP's error is under its pulse; the
// crosshair's ticks close in 2 px and turn white while locked, and flash white for flashTime on a new lock). Screens:
// TITLE (the run setup, PLAY, RESUME, CONTRACTS, WEAPONS, SETTINGS, CONTROLS, QUIT; NIGHT13 S10: AIM ASSIST lives in
// SETTINGS), PAUSED (RESUME, RESTART, SETTINGS, CONTROLS, AIM ASSIST, SAVE & QUIT, COPY REPORT, QUIT TO TITLE), DOWN,
// RESULTS (RESTART, TITLE, RETRY SAME SEED, COPY SCORE LINE).
// NIGHT8 U2-03 (no per-frame allocations, as hud.js's write(key, value) does): every text is set only when its value
// changed, through TMP's SetText (numbers without strings); colour hex strings are made once; the feed reuses one
// StringBuilder. Four nested canvases, so a change rebuilds only its own batch: the static frame, the changing values,
// the crosshair / markers / lock, and the menus (the only GraphicRaycaster). Every glyph is in LiberationSans SDF's
// static atlas (ASCII, Latin-1 with the middle dot, U+2022): the stamina pips are images now, so TMP never adds a glyph
// to the dynamic fallback asset (which rewrote that tracked asset in NIGHT7); NIGHT13 S1: the meta's lock is a padlock
// Image over a transparent U+00A4 (UiMeta.PlaceLocks), no longer U+25A1 (TMP's missing-glyph mark).
using System;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly SurvivalGame game;
        readonly GameConfig cfg;
        readonly RectTransform root;
        readonly Color text, muted, usable, danger, gold, panel, panelStrong;
        Color crossColor; // settings.crosshairColor (live)
        bool showPrices = true, hurtVignetteOn = true;
        int shownSettings = -1; // GameSettings.version last read
        readonly string goldHex, mutedHex, dangerHex;
        readonly float tSmall, tBody, tLabel, tH3, tH2, tH1, tNum, margin;

        // The four layers (nested canvases)
        readonly RectTransform frame, values, sight, menus;

        // HUD
        readonly RectTransform[] cross = new RectTransform[4];
        readonly RectTransform[] marker = new RectTransform[4];
        readonly UnityEngine.UI.Image[] markerImg = new UnityEngine.UI.Image[4];
        readonly UnityEngine.UI.Image[] markerTwin = new UnityEngine.UI.Image[4];
        readonly UnityEngine.UI.Image markerHeadDiamond; // NIGHT16 U3-29.7: the headshot's shape
        readonly UnityEngine.UI.Image eliteRing;
        readonly UnityEngine.UI.Image hpFill, hpHard;
        float shownHardFrac = 1;
        readonly TextMeshProUGUI hpText, points, weapon, roundText, prompt, banner, exposed;
        readonly TextMeshProUGUI pointsLabel; // NIGHT13 S5: "POINTS" beside the number (NIGHT17 U3-30.7: built hidden, no label)
        readonly UnityEngine.UI.Image promptBar;
        readonly TextMeshProUGUI rankLetter, rankName, feed; // NIGHT17 U3-30.7: built hidden (UiStyleMeter.cs replaces them)
        readonly UnityEngine.UI.Image rankFill;
        readonly StringBuilder sb = new StringBuilder(512);
        readonly UnityEngine.UI.Image vignette;
        readonly Canvas canvas;
        readonly RectTransform lockBox;
        readonly List<UnityEngine.UI.Image> lockParts = new List<UnityEngine.UI.Image>();
        readonly UnityEngine.UI.Image[] crossImg = new UnityEngine.UI.Image[4];
        double markerLeft, markerScale = 1, bannerLeft, vignetteLeft, blink, snapFlash, pulseT;
        Color markerColor;

        // config, read once
        readonly double hitMarkerTime, killScale, headshotScale, hitMarkerGap, hitMarkerSize, lowHpFraction, lowHpBlinkTime;
        readonly double feedFadeTime, hurtVignetteTime, hurtVignetteOpacity, bannerTime, zoneBannerTime;
        readonly int feedSize;

        // What is shown now (each set only when it changes)
        int shownHp = int.MinValue, shownRoundA = int.MinValue, shownRoundB = int.MinValue;
        int shownDownRound = int.MinValue, shownDownPoints = int.MinValue;
        SurvivalRun.State shownRunState = (SurvivalRun.State)(-1);
        string shownWeapon, shownMode;
        float shownHpFrac = -1, shownPromptFill = -1, shownBannerAlpha = -1, shownVignette = -1, shownLockAlpha = -1;
        bool shownHpLow, shownExposed, shownPrompt, shownMarker = true, shownLocked = true, feedDirty;
        Buyables.Item shownCandidate;
        bool shownCan;
        double shownPrice = double.NaN;
        RunStats shownStats;
        Flow shownFlow = (Flow)(-1);

        readonly string restartLabel; // the restart key's label (input.bindings.restart: KeyR -> R)
        string breakReadyFormat; // main.js hudLine (NIGHT6 B6): the break line with " · HOLD E: READY"
        string breakReadySuffix; // NIGHT23: rebuilt when the player's device changes (it was cached once, so the pad pass showed 'HOLD E')
        string breakReadyKey; // the interact key's word from the config
        bool breakReadyPad;
        long shownShapeKey;
        bool shownReady;
        string shownCandidateName;
        DeathRecap.Summary shownDeath;

        // U2-05: the damage-direction arcs (hud.js .hud-hits: config.ui.hitDirection radius, width, arcDeg)
        readonly List<UnityEngine.UI.Image> hitImgs = new List<UnityEngine.UI.Image>();
        readonly double hitLife, hitFadeFrom;

        // NIGHT12 U3-25.6: the theme faces (null: TMP's default) and the sizes from which Text() picks the display face and
        // the italic heading (theme.js: h2 and up are Chakra Petch; h1 / .big / the banner are its 700 italic).
        static TMP_FontAsset fontDisplay, fontBody;
        static float displayFrom = float.MaxValue, headingFrom = float.MaxValue;

        /// <summary>The face Text() gives a label: Auto picks by size (display from h2, the italic heading from h1).</summary>
        public enum Face { Auto, Body, Display, Heading }

        // NIGHT12 U3-25.6: the ceremony flags (design.ceremony.*; each false: the port's earlier screen).
        readonly bool ceremonyRound, resultsTally, livingTitle, hudMotion;

        // The hit marker grammar (hud.js 'hit'): a later, stronger hit in the same shot keeps the bigger marker; the kill's
        // marker in colors.enemy; the crosshair's kill flash (hud.killFlashTime / killFlashColor).
        bool markerKill, markerHead;
        double killFlash, markerTwistLeft, eliteRingLeft;
        // NIGHT16 polish #25 (a web port, hud.js :436, :511, :885-890): the crosshair's spread: open while dashing
        // (hud.crosshairDashSpread px) and after a THUNDERCLAP shot (hud.shotgunSpread px, recovering over spreadRecoverTime).
        double crossShotSpread, crossDashSpread, crossShotgunSpread, crossSpreadRecover;
        float shownCrossSpread = -1;
        // NIGHT16 polish #26 (a web port, hud.js rankUp :464-477, on rankChange up :489): a rank-up pops the letter (style.popScale -> 0.92 -> 1 over
        // style.popTime, ease-out) and slides the name in (30 px -> -4 -> 0). The name's fade and skew aren't ported (its alpha
        // is the zero-state plate's, UiFeelHud.cs; TMP has no skew transform).
        double rankPopAge = double.MaxValue, rankPopScale, rankPopTime;
        Vector2 rankNameHome;
        // NIGHT16 polish #31: the damage vignette (hud.js .hud-damage).
        UnityEngine.UI.Image damageVignette;
        Color damageColor;
        double damageShown, regenVignetteMax, regenVignetteEase;
        float shownDamage = -1;
        Color eliteRingColor = Color.white;
        readonly double killFlashTime;
        readonly Color killFlashColor;
        double lowHpBreath; // NIGHT17 U3-30.7: the low-HP vignette's breath (s into Hud17LowHpPeriod)
        string zoneBannerLast; // NIGHT17 U3-30.7: the zone whose name showed last, and when (real time)
        double zoneBannerAt = double.NegativeInfinity;

        public Ui(SurvivalGame game)
        {
            this.game = game;
            cfg = game.cfg;
            UiTheme.Init(cfg); // NIGHT17 U3-30.3: the identity's tokens (UnityTuning.Theme.cs) before anything is built
            // The theme fonts first: every Text() below uses them.
            fontDisplay = Resources.Load<TMP_FontAsset>(UnityTuning.FontDisplayAsset);
            fontBody = Resources.Load<TMP_FontAsset>(UnityTuning.FontBodyAsset);
            if (fontDisplay == null || fontBody == null) Debug.LogWarning("[ui] theme fonts missing (run SKYGRIND/Make Theme Fonts): TMP's default font for the missing face");
            displayFrom = (float)cfg.Num("ui.theme.type.h2");
            headingFrom = (float)cfg.Num("ui.theme.type.h1");
            ceremonyRound = cfg.Design("ceremony.round");
            resultsTally = cfg.Design("ceremony.resultsTally");
            livingTitle = cfg.Design("ceremony.livingTitle");
            hudMotion = cfg.Design("ceremony.hudMotion");
            killFlashTime = cfg.Num("hud.killFlashTime");
            killFlashColor = Look.C(cfg, "hud.killFlashColor");
            text = Look.C(cfg, "ui.theme.text");
            muted = text * new Color(1, 1, 1, 0.74f);
            usable = Look.C(cfg, "colors.usable");
            crossColor = CrosshairColor(game.settings.Str("crosshairColor"));
            danger = Look.C(cfg, "colors.enemy");
            gold = Look.C(cfg, "ui.theme.gold");
            goldHex = ColorUtility.ToHtmlStringRGB(gold);
            mutedHex = ColorUtility.ToHtmlStringRGB(muted);
            dangerHex = ColorUtility.ToHtmlStringRGB(danger);
            panel = Look.C(cfg, "ui.theme.panel");
            panel.a = (float)cfg.Num("ui.theme.panelOpacity");
            panelStrong = Look.C(cfg, "ui.theme.panel");
            panelStrong.a = (float)cfg.Num("ui.theme.panelStrongOpacity");
            tSmall = (float)cfg.Num("ui.theme.type.small");
            tBody = (float)cfg.Num("ui.theme.type.body");
            tLabel = (float)cfg.Num("ui.theme.type.label");
            tH3 = (float)cfg.Num("ui.theme.type.h3");
            tH2 = (float)cfg.Num("ui.theme.type.h2");
            tH1 = (float)cfg.Num("ui.theme.type.h1");
            tNum = (float)cfg.Num("ui.theme.type.num");
            margin = UnityTuning.ThemeSafeInset; // NIGHT17 U3-30.7: the safe inset (NIGHT17.md rule 8: 48-64 px), was hud.edgeMargin's 22
            hitMarkerTime = cfg.Num("hud.hitMarkerTime");
            crossDashSpread = cfg.Num("hud.crosshairDashSpread");
            crossShotgunSpread = cfg.Num("hud.shotgunSpread");
            crossSpreadRecover = cfg.Num("hud.spreadRecoverTime");
            rankPopScale = cfg.Num("style.popScale");
            rankPopTime = cfg.Num("style.popTime");
            killScale = cfg.Num("hud.killScale");
            headshotScale = cfg.Num("hud.headshotScale");
            hitMarkerGap = cfg.Num("hud.hitMarkerGap");
            hitMarkerSize = cfg.Num("hud.hitMarkerSize");
            lowHpFraction = cfg.Num("health.lowHpFraction");
            lowHpBlinkTime = cfg.Num("hud.lowHpBlinkTime");
            feedFadeTime = cfg.Num("hud.feedFadeTime");
            feedSize = cfg.Int("hud.feedSize");
            hurtVignetteTime = cfg.Num("hud.hurtVignetteTime");
            hurtVignetteOpacity = cfg.Num("hud.hurtVignetteOpacity");
            bannerTime = cfg.Num("survival.bannerTime");
            zoneBannerTime = cfg.Num("zones.banner.time");

            var canvasGo = new GameObject("UI", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, (float)cfg.Num("ui.theme.scaleBaseHeight"));
            scaler.matchWidthOrHeight = 1; // the web scales its UI with the height
            root = canvasGo.GetComponent<RectTransform>();
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            frame = Layer("HUD frame", false);
            values = Layer("HUD values", false);
            sight = Layer("HUD sight", false);
            menus = Layer("Menus", true);

            // --- HUD -----------------------------------------------------------------------------------------------
            vignette = Img(values, "Hurt vignette", danger, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            vignette.sprite = VignetteSprite();
            vignette.color = new Color(danger.r, danger.g, danger.b, 0);
            vignette.transform.SetAsFirstSibling();
            // NIGHT16 polish #31 (a web port, hud.js .hud-damage :246-247 / :916-917): the damage vignette, dark red edges
            // eased toward (1 - hp / max) x regen.vignetteMax at regen.vignetteEase, so it deepens with the HP lost and fades
            // as the HP regenerates. Under the hurt flash (the web's DOM order).
            damageColor = Look.Hex(UnityTuning.DamageVignetteHex);
            damageVignette = Img(values, "Damage vignette", damageColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            damageVignette.sprite = DamageVignetteSprite();
            damageVignette.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0);
            damageVignette.transform.SetAsFirstSibling();
            regenVignetteMax = cfg.Num("regen.vignetteMax");
            regenVignetteEase = cfg.Num("regen.vignetteEase");
            for (int i = 0; i < 4; i++)
            {
                bool horizontal = i < 2;
                crossImg[i] = Img(sight, $"Crosshair {i}", crossColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, horizontal ? new Vector2(10, 2) : new Vector2(2, 10));
                cross[i] = crossImg[i].rectTransform;
                markerImg[i] = Img(sight, $"Hit marker {i}", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2, (float)hitMarkerSize));
                marker[i] = markerImg[i].rectTransform;
                marker[i].localRotation = Quaternion.Euler(0, 0, 45 + 90 * i);
                // NIGHT12 U3-25.6 (design.ceremony.hudMotion): a headshot's second tick beside each arm (shape, not colour alone).
                markerTwin[i] = Img(sight, $"Hit marker {i} (head)", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2, (float)hitMarkerSize));
                markerTwin[i].enabled = false;
            }
            // NIGHT16 U3-29.7: a headshot's own shape, a small hollow diamond inside the arms (UiFeelHud.cs HeadDiamondSprite).
            markerHeadDiamond = Img(sight, "Hit marker (head diamond)", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            markerHeadDiamond.sprite = HeadDiamondSprite(UnityTuning.HeadDiamondRadius, UnityTuning.HeadDiamondWidth);
            markerHeadDiamond.rectTransform.sizeDelta = markerHeadDiamond.sprite.rect.size;
            markerHeadDiamond.raycastTarget = false;
            markerHeadDiamond.enabled = false;
            // An elite's ring burst round the marker (a hit on an aim.lock.elite kind; design.ceremony.hudMotion).
            eliteRing = Img(sight, "Hit marker (elite)", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            eliteRing.sprite = RingSprite(UnityTuning.EliteRingRadius, UnityTuning.EliteRingWidth);
            eliteRing.rectTransform.sizeDelta = eliteRing.sprite.rect.size;
            eliteRing.enabled = false;

            // The damage-direction arcs: ui.hitDirection.max red-orange ring segments round the crosshair, rotated toward the
            // source, full for fadeFrom s, then fading to 0 at life (main.js updateHitArcs).
            hitLife = cfg.Num("ui.hitDirection.life");
            hitFadeFrom = cfg.Num("ui.hitDirection.fadeFrom");
            Sprite arc = ArcSprite((float)cfg.Num("ui.hitDirection.radius"), (float)cfg.Num("ui.hitDirection.width"), (float)cfg.Num("ui.hitDirection.arcDeg"));
            for (int i = 0; i < cfg.Int("ui.hitDirection.max"); i++)
            {
                var hi = Img(sight, $"Hit arc {i}", new Color(danger.r, danger.g, danger.b, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, arc.rect.size);
                hi.sprite = arc;
                hi.enabled = false;
                hitImgs.Add(hi);
            }

            // The lock brackets: a 100 x 100 box (scaled to the target) of four 14 px corners, 3 px thick, white with a danger glow.
            lockBox = new GameObject("Lock brackets", typeof(RectTransform)).GetComponent<RectTransform>();
            lockBox.SetParent(sight, false);
            lockBox.anchorMin = lockBox.anchorMax = Vector2.zero;
            lockBox.pivot = new Vector2(0.5f, 0.5f);
            lockBox.sizeDelta = new Vector2(100, 100);
            for (int i = 0; i < 4; i++)
            {
                float sx = i % 2 == 0 ? -1 : 1, sy = i < 2 ? 1 : -1;
                var hBar = Img(lockBox, $"Corner {i} h", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(sx * 43, sy * 48.5f), new Vector2(14, 3));
                var vBar = Img(lockBox, $"Corner {i} v", Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(sx * 48.5f, sy * 43), new Vector2(3, 14));
                foreach (var bar in new[] { hBar, vBar })
                {
                    var glow = bar.gameObject.AddComponent<UnityEngine.UI.Outline>();
                    glow.effectColor = new Color(danger.r, danger.g, danger.b, 0.8f);
                    glow.effectDistance = new Vector2(1.5f, -1.5f);
                    lockParts.Add(bar);
                }
            }
            lockBox.gameObject.SetActive(false);
            game.events.On<AimLockEvent>("aimLock", _ => snapFlash = game.assist.lk.flashTime);

            // Bottom left: the vitals (NIGHT17 U3-30.7: the glance tier, no labels). An INK plate with the theme's cut and a
            // 4 px cyan accent bar (no translucent grey plate); HP secondary to the points (round 1's reviewer): a tabular S42
            // DISPLAY numeral over a fat slanted WHITE bar cut in quarters by INK slits (it blinks DANGER at low HP).
            // NIGHT19 U3-33.8: the stamina is back in this block, a row of chunky pips under the HP bar (UiStaminaBar.cs; the
            // bar under the crosshair is gone: S14), the slide charge's cell at the row's end (UiFeelHud.cs); the HP bar moves
            // up 4 px (Hud19HpBarY) to make room. The panel, its edge and the bar's back are the frame; the number and the
            // fills sit in the values layer at the same place.
            float slantTan = Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad);
            var hpPanel = Img(frame, "HP panel", UiTheme.Ink, Vector2.zero, Vector2.zero, new Vector2(margin, margin), new Vector2(UnityTuning.Hud17VitalsWidth, UnityTuning.Hud17VitalsHeight));
            hpPanel.rectTransform.pivot = Vector2.zero;
            hpPanel.sprite = UiTheme.Cut;
            hpPanel.type = UnityEngine.UI.Image.Type.Sliced;
            Img(hpPanel.transform, "Edge", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(UnityTuning.ThemeAccentBar, -2 * UnityTuning.ThemeCut)).rectTransform.pivot = new Vector2(0, 0.5f);
            RectTransform hpValues = Twin(hpPanel.rectTransform, values);
            float hpPx = UiTheme.Px(UiTheme.Size.S42);
            hpText = Hud17Text(hpValues, "HP", "100", hpPx, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(UnityTuning.Hud17VitalsPad, 2), new Vector2(200, hpPx * 1.05f), true);
            // NIGHT19: the empty track solid INK-SOFT, as the stamina pips' (section 6: no translucent white-on-INK grey)
            var hpBack = Img(hpPanel.transform, "HP bar", UiTheme.InkSoft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(UnityTuning.Hud17VitalsPad, -UnityTuning.Hud19HpBarY), new Vector2(UnityTuning.Hud17HpBarWidth, UnityTuning.Hud17HpBarHeight));
            hpBack.rectTransform.pivot = new Vector2(0, 1);
            UiSkew.Apply(hpBack, slantTan);
            RectTransform hpBarValues = Twin(hpBack.rectTransform, hpValues);
            hpFill = Img(hpBarValues, "Fill", UiTheme.White, Vector2.zero, new Vector2(1, 1), Vector2.zero, Vector2.zero);
            hpFill.rectTransform.pivot = new Vector2(0, 0.5f);
            UiSkew.Apply(hpFill, slantTan);
            // NIGHT11 U3-24.9 (the LAB's ULTRA HEALTH): the hard damage, a cap from max - hard to max (heals stop at it).
            hpHard = Img(hpBarValues, "Hard damage", UiTheme.Muted, new Vector2(1, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            hpHard.rectTransform.pivot = new Vector2(1, 0.5f);
            UiSkew.Apply(hpHard, slantTan);
            hpHard.enabled = false;
            // the quarters: INK slits over the fills (a segmented bar reads a quarter of HP at a glance)
            hpBarValues20 = hpBarValues; // NIGHT20 U3-34.10: the segment ticks and the refill sit in the same rect (UiVitals20.cs)
            hpSlits20 = new UnityEngine.UI.Image[Math.Max(0, UnityTuning.Hud17HpSegments - 1)];
            for (int i = 1; i < UnityTuning.Hud17HpSegments; i++)
            {
                float at = i / (float)UnityTuning.Hud17HpSegments;
                var slit = Img(hpBarValues, $"Slit {i}", UiTheme.InkSolid, new Vector2(at, 0), new Vector2(at, 1), Vector2.zero, new Vector2(UnityTuning.Hud17HpSlit, 0));
                UiSkew.Apply(slit, slantTan);
                hpSlits20[i - 1] = slit;
            }

            // Bottom right: the weapon card + the points (NIGHT17 U3-30.7: the points are the screen's ONE hero number, S75
            // DISPLAY gold (round 1's reviewer), tabular, no POINTS label; the card sits over them: UiHotbar.cs).
            float ptsPx = UiTheme.Px(UiTheme.Size.S75);
            weapon = Hud17Text(values, "Weapon", "KESTREL", UiTheme.Px(UiTheme.Size.S32), UiTheme.White, UiTheme.Display, TextAlignmentOptions.Right, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-margin, margin + UnityTuning.Hud17PointsBox + UnityTuning.Hud17PointsBarHeight + 2 * UnityTuning.Hud17PointsBarGap + UnityTuning.PointsWeaponGap), new Vector2(500, 40), true);
            points = Hud17Text(values, "Points", "500", ptsPx, UiTheme.Gold, UiTheme.Display, TextAlignmentOptions.BottomRight, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-margin, margin + UnityTuning.Hud17PointsBarHeight + UnityTuning.Hud17PointsBarGap), new Vector2(500, UnityTuning.Hud17PointsBox), true);
            pointsLabel = Text(frame, "Points label", "POINTS", tLabel, muted, TextAlignmentOptions.Right, new Vector2(-margin, margin + 28), new Vector2(UnityTuning.PointsLabelWidth, UnityTuning.PointsLabelHeight), new Vector2(1, 0));
            pointsLabel.gameObject.SetActive(false); // NIGHT17 U3-30.7: the glance numbers have no labels

            // Top: round + enemies, EXPOSED; banner; bottom centre: the buy prompt
            roundText = Text(values, "Round", "", tH3, text, TextAlignmentOptions.Center, new Vector2(0, -margin), new Vector2(1500, 32), new Vector2(0.5f, 1)); // NIGHT11: wide enough for the objective, the PROTOCOL and exfil
            UiTheme.Style(roundText, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, true); // NIGHT17 U3-30.7: secondary tier
            roundText.characterSpacing = UnityTuning.ThemeLabelTracking;
            exposed = Text(values, "Exposed", "EXPOSED", tH3, danger, TextAlignmentOptions.Center, new Vector2(0, -margin - 36), new Vector2(700, 30), new Vector2(0.5f, 1), Face.Heading); // hud.js .hud-exposed: display 700 italic
            exposed.gameObject.SetActive(false);
            banner = Text(values, "Banner", "", tH1, text, TextAlignmentOptions.Center, new Vector2(0, -170), new Vector2(1400, 70), new Vector2(0.5f, 1));
            // NIGHT17 U3-30.7: the theme's DISPLAY with its hard shadow, S42 down to S24 for the long ones
            UiTheme.Style(banner, UiTheme.Px(UiTheme.Size.S42), UiTheme.White, UiTheme.Display, true);
            banner.fontStyle = FontStyles.Normal; // the display face is a black italic already
            banner.enableAutoSizing = true; // NIGHT11: the run's long banners (THE PROTOCOL IS COMPLETE · EVERY GUN FORGED) fit the box
            banner.fontSizeMax = UiTheme.Px(UiTheme.Size.S42);
            banner.fontSizeMin = UiTheme.Px(UiTheme.Size.S24);
            // NIGHT19 U3-33.8 (section 6's banned list: flat text on nothing, a translucent grey bar): the buy prompt sits on an
            // INK slanted, cut plate with a CYAN accent bar, sized to its text (BuyPlate19), its hold bar on a solid INK-SOFT track
            buyPlate19 = Hud17Shape(values, "Buy prompt plate", UiTheme.Ink, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 170 - UnityTuning.Hud19BuyPlateBelow), new Vector2(200, 32 + UnityTuning.Hud19BuyPlateBelow + UnityTuning.Hud19BuyPlateAbove));
            buyPlateEdge19 = Hud17Shape(buyPlate19.rectTransform, "Edge", UiTheme.Cyan, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 32 + UnityTuning.Hud19BuyPlateBelow + UnityTuning.Hud19BuyPlateAbove));
            buyPlateEdge19.SetCuts(0, 0);
            buyPlate19.gameObject.SetActive(false);
            prompt = Text(values, "Buy prompt", "", tH3, text, TextAlignmentOptions.Center, new Vector2(0, 170), new Vector2(900, 32), new Vector2(0.5f, 0));
            var barBack = Img(prompt.transform, "Hold", UiTheme.InkSoft, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -10), new Vector2(240, 5));
            promptBar = Img(barBack.transform, "Fill", usable, Vector2.zero, new Vector2(0, 1), Vector2.zero, Vector2.zero);
            promptBar.rectTransform.pivot = new Vector2(0, 0.5f);
            prompt.gameObject.SetActive(false);
            BuildFeelEdges(barBack, hpValues); // NIGHT16 polish #32-#34: EXPOSED's edge, the prompt's poor / denied, regen (UiFeelEdges.cs)
            BuildFeelTags(); // NIGHT16 polish #37 / #38: the crosshair tag, the charge edge (UiFeelTags.cs)
            BuildMoveRow(); // NIGHT16 polish #35: hud.js's move row, the speed and the wall-jump ticks (UiMoveRow.cs)

            // Right: the style rank (styleMeter's HUD: the rank letter in its colour, its name, the band's fill, the feed)
            rankLetter = Text(values, "Rank letter", "D", 72, text, TextAlignmentOptions.Right, new Vector2(-margin, 120), new Vector2(200, 84), new Vector2(1, 0.5f));
            rankLetter.fontStyle = FontStyles.Bold | FontStyles.Italic;
            rankName = Text(values, "Rank name", "DULL", tLabel, muted, TextAlignmentOptions.Right, new Vector2(-margin, 70), new Vector2(300, 24), new Vector2(1, 0.5f));
            rankName.characterSpacing = 10;
            rankNameHome = rankName.rectTransform.anchoredPosition;
            var fillBack = Img(frame, "Rank bar", new Color(1, 1, 1, 0.12f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-margin, 50), new Vector2(180, 6));
            fillBack.rectTransform.pivot = new Vector2(1, 0.5f);
            rankFill = Img(Twin(fillBack.rectTransform, values), "Fill", usable, Vector2.zero, new Vector2(0, 1), Vector2.zero, Vector2.zero);
            rankFill.rectTransform.pivot = new Vector2(0, 0.5f);
            awardLines = new Dictionary<(string, double), string>();
            feed = Text(values, "Award feed", "", tBody, text, TextAlignmentOptions.TopRight, new Vector2(-margin, 30), new Vector2(420, 160), new Vector2(1, 0.5f));
            feed.rectTransform.pivot = new Vector2(1, 1);
            // NIGHT17 U3-30.7: the web's rank box (the letter in the rank's hue, its name, the 6 px bar, the "+ NAME points"
            // feed) gives way to the style meter (UiStyleMeter.cs: the cyan value ladder, the decay bar, the callouts). Its
            // parts stay built, hidden, so the older partials that write to them keep working.
            rankLetter.gameObject.SetActive(false);
            rankName.gameObject.SetActive(false);
            fillBack.gameObject.SetActive(false);
            rankFill.gameObject.SetActive(false);
            feed.gameObject.SetActive(false);

            string rk = cfg.Str("input.bindings.restart");
            restartLabel = rk.StartsWith("Key") ? rk.Substring(3) : rk.ToUpperInvariant();
            string ik = cfg.Str("input.bindings.interact");
            breakReadyKey = ik.StartsWith("Key") ? ik.Substring(3) : ik.ToUpperInvariant();
            BreakReadyRefresh();
            BuildMenus(); // NIGHT17 U3-30.3: TITLE, PAUSED, DOWN, RESULTS (UiMenus.cs, builder N)
            settingsScreen = Screen("Settings", panelStrong);
            BuildSettings();
            BuildDraft();
            BuildMeta(); // NIGHT12: the run setup, CONTRACTS, WEAPONS, the PAYOFF strip, toasts, the first-run guide (UiMeta*.cs)
            // NIGHT13 (the shared types): each night-13 screen in its own partial file; every hook is optional.
            BuildControls(); // U3-26.8: CONTROLS (UiControls.cs)
            BuildNav(); // U3-26.8: pad / keyboard navigation of the menus (UiNav.cs)
            BuildHotbar(); // U3-26.9: the HUD hotbar, the weapon bar, the special hint (UiHotbar.cs)
            BuildRadial(); // U3-26.9: the radial wheel (UiRadial.cs)
            BuildReport(); // U3-26.12: the version stamp, COPY REPORT (UiReport.cs)
            // NIGHT14 (the shared types): each night-14 screen in its own partial file; every hook is optional.
            BuildKiosk(); // U3-27.7: the kiosk's control hints (UiKiosk.cs)
            BuildRecap(); // U3-27.9: WHAT GOT YOU's host (UiRecap.cs)
            BuildRoute(); // U3-27.10: THE ROUTE on RESULTS (UiRoute.cs)
            BuildGhostSplits(); // U3-27.11: the round splits (UiGhost.cs)
            BuildN26Hud(); // NIGHT26 (the shared commit): the NIGHT26 HUD widgets (Ui.N26.cs hooks)
            BuildWeapons27Hud(); // NIGHT27 (builder W): the glove pips and the charge arc (UiWeapons.N27.cs)
            BuildN25Hud(); // NIGHT25 (the shared commit): the NIGHT25 HUD widgets (Ui.N25.cs hooks)
            BuildN24Hud(); // NIGHT24 (the shared commit): the NIGHT24 HUD widgets (Ui.N24.cs hooks)
            BuildN22Hud(); // NIGHT22 (the shared commit): the NIGHT22 HUD widgets, each builder's own partial file (the ARCLINE chip, the DEADLINE bar, the stamp, ...)
            BuildFeelHud(); // NIGHT16 (the shared commit): the movement HUD at the crosshair (UiFeelHud.cs, builder U)
            BuildPointsPops(); // NIGHT16 polish #30: the "+60" points pops at the crosshair (UiPointsPops.cs)
            BuildAds(); // NIGHT16 polish #36: the ADS ring, the LONGWAVE's scope, the rangefinder (UiAds.cs)
            if (hudMotion) BuildHudMotion(); // NIGHT12 U3-25.6: the HP ghost, the skewed plates (UiHudMotion.cs)
            BuildPoints17(); // NIGHT17 U3-30.7: the odometer, its punch, its white frame, its gold bar (UiHudMotion.cs)
            BuildStyleMeter(); // NIGHT17 U3-30.7: the style meter (UiStyleMeter.cs)
            BuildStaminaBar(); // NIGHT19 U3-33.8: the stamina pips in the vitals plate (UiStaminaBar.cs)
            BuildKillMarker17(); // NIGHT17 U3-30.7: the kill marker, distinct from the hit marker (UiMarkers.cs)
            BuildKillPop53(); // NIGHT53 U polish53 (killPop): the kill marker's pop shards (UiKillPop53.cs; off: nothing built)
            BuildRideKill53(); // EXT53 N rideKill53: the RIDE KILL callout, hidden until the sim's rideKill53 (UiRideKill53.cs; row off: never shown)
            BuildClear55(); // NIGHT55 R clear55: the room-clear card (time vs par), hidden until a combat room's roomClear (UiClear55.cs; row off: nothing built)
            BuildHitMarker18(); // NIGHT18 round 1: the hit marker's INK underlays (UiHudMotion18.cs)
            if (ceremonyRound) BuildCeremony(); // NIGHT12 U3-25.6: the round counter and its burn-in (UiCeremony.cs)
            BuildHud20(); // NIGHT20 U3-34.10: the shockwave chip, the zoom reticle and scope, the segments and the refill (builder H)
            BuildHud30(); // NIGHT30 (builder V, design.n30.hud30): the weapon-state widget, the menu-hidden run rows, the layout pass (Hud30.cs, HudWeapons30.cs)
            BuildFront33(); // NIGHT33 U (design.n33.hud33): the front screens in the HUD33 language, last (every screen built); off: nothing (UiFront33.cs)

            // Events
            // NIGHT12 U3-25.6: the hit marker grammar (hud.js 'hit'): body (white), head (x headshotScale), kill (x killScale,
            // in colors.enemy: the web's colour; the port had gold); a later, stronger hit in the same shot keeps the bigger
            // marker. design.ceremony.hudMotion adds the shape cues: a headshot's doubled ticks, the kill's twist, an elite's
            // ring (aim.lock.elite kinds).
            List<string> eliteKinds = game.assist != null ? game.assist.lk.elite : null;
            game.events.On<HitEvent>("hit", e =>
            {
                if (e.owner != "player") return;
                // NIGHT17 U3-30.7: a kill has its own marker (UiMarkers.cs: bigger, another shape, a scale punch); the hit
                // marker stays the hit's (white, the headshot's diamond).
                if (e.killed) KillMarker17Fire(e.headshot);
                if (FeelAB.On && KillMarker18Fresh)
                {
                    // NIGHT18 U3-31.6: the kill's own frame shows the kill confirm alone (one shape: UiMarkers.cs); the hit
                    // marker stands down for it, so a kill never reads as a hit
                    markerLeft = 0;
                }
                else
                {
                    double scale = e.headshot ? headshotScale : 1;
                    if (markerLeft <= 0 || scale >= markerScale)
                    {
                        markerScale = scale;
                        markerKill = false;
                        markerHead = e.headshot;
                        markerColor = UiTheme.White;
                    }
                    // NIGHT18 U3-31.6: the arms grow out from the crosshair (a hit while they show re-grows a little: a kick)
                    markerGrowFrom = markerLeft > 0 ? UnityTuning.Hud18HitMarkerRepeatFrom : UnityTuning.Hud18HitMarkerFrom;
                    markerGrowAge = 0;
                    markerLeft = hitMarkerTime;
                }
                if (hudMotion && eliteKinds != null && e.kind != null && eliteKinds.Contains(e.kind))
                {
                    eliteRingLeft = hitMarkerTime * UnityTuning.EliteRingLife;
                    eliteRingColor = e.killed ? danger : Color.white;
                }
                shownMarker = false; // redraw the marker
            });
            game.events.On<KillEvent>("kill", e =>
            {
                if (e.owner == "player") killFlash = killFlashTime; // hud.js: the crosshair's kill flash
            });
            game.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player") return;
                if (vignetteLeft <= 0) hurtFlashAge = 0; // NIGHT18 U3-31.6: a fresh flash swells in (one already showing doesn't dip)
                vignetteLeft = hurtVignetteTime;
            });
            // NIGHT12 U3-25.6: with design.ceremony.round the burn-in replaces the ROUND N banner (UiCeremony.cs; the other
            // banners keep their queue).
            // NIGHT21 U3-35.7: THE GRIND's WAVE banner (waveBreak) replaces the ROUND N banner (the waves are its rounds).
            if (!ceremonyRound) game.events.On<RoundEvent>("roundBegin", e => { if (game.setup.mode != RunSetup.Grind && !(hud30On && game.setup.mode == RunSetup.Campaign)) Banner($"ROUND {e.number}", text, bannerTime, true); }); // NIGHT30 (hud30): no ROUND banner in a campaign
            // NIGHT11: the web's `banner {text}` event (objectives.js, protocol.js and the port-only power, swarm drop and
            // exfil). hud.js shows the last one only, so on the web a round's own banners (HOLD ROUND, THE PROTOCOL WAKES)
            // were overwritten by main.js's ROUND N in the same frame; here they queue behind it (a port-only HUD change).
            game.events.On<BannerEvent>("banner", e => Banner(e.text, text, bannerTime));
            game.events.On<ZoneEnterEvent>("zoneEnter", e =>
            {
                // NIGHT17 U3-30.7 (round 1's reviewer: LANDING sat on screen through fights): a zone's name shows
                // Hud17ZoneBannerTime s on entering it, and not again for Hud17ZoneRepeat s (crossing a border back and forth).
                if (!game.run.Live || game.run.round <= 0) return;
                double now = SurvivalGame.RealNow;
                if (e.name == zoneBannerLast && now - zoneBannerAt < UnityTuning.Hud17ZoneRepeat) return;
                zoneBannerLast = e.name;
                zoneBannerAt = now;
                Banner(Copy47.Title(cfg, e.name), Look.Hex(e.color), Math.Min(zoneBannerTime, UnityTuning.Hud17ZoneBannerTime)); // NIGHT47 F copy47: the name without its "1.1 " (off: as it is)
            });
            game.OnNewRun += () => zoneBannerLast = null; // a new run names its first zone again
            game.events.On<GateOpenEvent>("gateOpen", e => Banner($"{e.name} OPEN", usable, zoneBannerTime));
            // hud.js :511: a THUNDERCLAP shot opens the crosshair (the port's `fire` carries the weapon id only).
            game.events.On("fire", o =>
            {
                if (o is string w && w == "shotgun") crossShotSpread = crossSpreadRecover;
            });
        }

        // NIGHT12 (the shared types): the meta's UI lives in its own partial files (UiMeta*.cs); each hook is optional.
        partial void BuildMeta();
        partial void RenderMeta(double frameDt);
        partial void OpenContracts();
        partial void OpenWeapons();

        // NIGHT13 (the shared types): one owner per hook (NIGHT13.md section 3): S UiControls.cs / UiNav.cs, H UiHotbar.cs /
        // UiRadial.cs, G UiReport.cs. Render hooks run every frame, every flow, before the HUD's early return.
        Dictionary<(string, double), string> awardLines; // the award feed's lines by (name, points)

        partial void BuildControls();
        partial void OpenControls();
        partial void RenderControls(double frameDt);
        partial void BuildNav();
        partial void RenderNav(double frameDt);
        partial void BuildHotbar();
        partial void RenderHotbar(double frameDt);
        partial void BuildRadial();
        partial void RenderRadial(double frameDt);
        partial void BuildReport();
        partial void RenderReport(double frameDt);
        partial void CopyReport();
        // NIGHT14 (the shared types): one owner per hook (NIGHT14.md section 0 rule 11): K UiKiosk.cs, V UiRecap.cs, V2
        // UiRoute.cs / UiGhost.cs. Render hooks run every frame, every flow, before the HUD's early return.
        partial void BuildKiosk();
        partial void RenderKiosk(double frameDt);
        partial void BuildRecap();
        partial void RenderRecap(double frameDt);
        partial void BuildRoute();
        partial void RenderRoute(double frameDt);
        partial void SaveRouteCard();
        partial void CopyRouteSeed();
        partial void BuildGhostSplits();
        partial void RenderGhostSplits(double frameDt);
        // NIGHT16 (the shared commit): builder U's hooks (UiFeelHud.cs). Render runs every frame, every flow, before the HUD's early return.
        partial void BuildFeelHud();
        partial void BuildN22Hud();

        void BreakReadyRefresh()
        {
            breakReadyPad = PlayerDevices.Last == InputDevice.Pad;
            breakReadySuffix = " · " + PromptText.For("interact", true, "HOLD " + breakReadyKey) + ": READY"; // NIGHT19: PromptText (builder P)
            breakReadyFormat = "ROUND {0:0} IN {1:0}" + breakReadySuffix;
        }
        partial void RenderN22Hud(double frameDt);
        partial void RenderFeelHud(double frameDt);

        // NIGHT19 U3-33.8: the buy prompt's plate (built before the prompt: drawn behind it), sized to its text
        UiShape buyPlate19, buyPlateEdge19;
        float buyPlateShownW19 = -1, buyPlateShownScale19 = -1, buyPlateShownAlpha19 = -1;
        Vector2 buyPlateShownPos19 = new Vector2(float.NaN, 0);

        /// <summary>NIGHT19 U3-33.8: the plate follows the prompt: shown with it, its width (on a text change), its place (the
        /// deny shake), its entrance's scale and alpha (Prompt18). Written only on change.</summary>
        void BuyPlate19()
        {
            if (buyPlate19 == null) return;
            bool on = prompt.gameObject.activeSelf;
            if (buyPlate19.gameObject.activeSelf != on) buyPlate19.gameObject.SetActive(on);
            if (!on) return;
            // TMP caches its preferred width until the text changes (set on change only), so this read is cheap per frame
            float w = Mathf.Round(Mathf.Max(prompt.preferredWidth, 240f) + 2 * UnityTuning.Hud19BuyPlatePadX);
            if (w != buyPlateShownW19)
            {
                buyPlateShownW19 = w;
                RectTransform rt = buyPlate19.rectTransform;
                rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
            }
            Vector2 pos = prompt.rectTransform.anchoredPosition - new Vector2(0, UnityTuning.Hud19BuyPlateBelow);
            if (pos != buyPlateShownPos19)
            {
                buyPlateShownPos19 = pos;
                buyPlate19.rectTransform.anchoredPosition = pos;
            }
            float s = prompt.rectTransform.localScale.x;
            if (s != buyPlateShownScale19)
            {
                buyPlateShownScale19 = s;
                buyPlate19.rectTransform.localScale = new Vector3(s, s, 1);
            }
            float a = promptGroup18 != null ? promptGroup18.alpha : 1f;
            if (a != buyPlateShownAlpha19)
            {
                buyPlateShownAlpha19 = a;
                buyPlate19.color = UiTheme.A(UiTheme.Ink, UiTheme.Ink.a * a);
                buyPlateEdge19.color = UiTheme.A(UiTheme.Cyan, a);
            }
        }

        readonly List<(string text, Color color, double time)> bannerQueue = new List<(string, Color, double)>(8);
        int bannerFrame = -1;

        /// <summary>
        /// Show a banner. NIGHT11: banners raised in the same frame as the one showing queue behind it (each for its time),
        /// a round's own banner first; a banner from a later frame replaces the one showing, as hud.js does.
        /// </summary>
        void Banner(string s, Color c, double time, bool first = false)
        {
            Post33(s, time); // NIGHT33 U (design.n33.hud33): the line also goes to the one message slot (UiHud33.cs); off: nothing
            if (Time.frameCount == bannerFrame && bannerLeft > 0)
            {
                if (first)
                {
                    bannerQueue.Insert(0, (banner.text, banner.color, bannerLeft));
                    ShowBanner(s, c, time);
                }
                else if (bannerQueue.Count < 8) bannerQueue.Add((s, c, time));
                return;
            }
            bannerQueue.Clear();
            bannerFrame = Time.frameCount;
            ShowBanner(s, c, time);
        }

        void ShowBanner(string s, Color c, double time)
        {
            banner.text = s;
            c.a = 1;
            banner.color = c;
            shownBannerAlpha = -1;
            bannerLeft = time;
            bannerEnterAge = 0; // NIGHT18 U3-31.6: each banner enters along the slant (FeelAB on; UiHudMotion18.cs)
        }

        /// <summary>A full-screen layer under the root canvas, with its own (nested) canvas; `interactive` adds the raycaster.</summary>

        /// <summary>hud.js rankUp (:464-477): WAAPI eases the whole pop with CSS ease-out, cubic-bezier(0, 0, 0.58, 1), and goes
        /// linearly between the keyframes: the letter scale(popScale) -> 0.92 at 0.65 -> 1; the name translateX(30px) -> -4px at
        /// 0.7 -> none. Writes only while a pop runs, and once more to rest.</summary>
        void RankPop(double frameDt)
        {
            if (rankPopAge > rankPopTime) return;
            rankPopAge += frameDt;
            double p = rankPopTime > 0 ? Math.Min(1, rankPopAge / rankPopTime) : 1;
            // the bezier's x(t) = 3 x2 (1 - t) t^2 + t^3, y(t) = 3 t^2 - 2 t^3: bisect x(t) = p (16 halvings: under 2e-5)
            const double x2 = UnityTuning.CssEaseOutX2;
            double lo = 0, hi = 1;
            for (int i = 0; i < 16; i++)
            {
                double t = (lo + hi) * 0.5;
                if (3 * x2 * (1 - t) * t * t + t * t * t < p) lo = t; else hi = t;
            }
            double m = (lo + hi) * 0.5, u = 3 * m * m - 2 * m * m * m;
            const double under = UnityTuning.RankPopUnder, underAt = UnityTuning.RankPopUnderAt;
            const double from = UnityTuning.RankNameFrom, over = UnityTuning.RankNameOver, overAt = UnityTuning.RankNameOverAt;
            double scale = u < underAt ? rankPopScale + (under - rankPopScale) * (u / underAt) : under + (1 - under) * ((u - underAt) / (1 - underAt));
            double dx = u < overAt ? from + (over - from) * (u / overAt) : over - over * ((u - overAt) / (1 - overAt));
            if (p >= 1) { scale = 1; dx = 0; rankPopAge = double.MaxValue; }
            rankLetter.rectTransform.localScale = new Vector3((float)scale, (float)scale, 1);
            rankName.rectTransform.anchoredPosition = rankNameHome + new Vector2((float)dx, 0);
        }

        /// <summary>main.js updateLockBrackets: the target's chest +- 1.1 m projected, x1.1 x the tightening; 0.7 wide.</summary>
        void Brackets(LockInfo lockInfo, double frameDt)
        {
            Camera cam = game.cam;
            if (lockInfo == null || cam == null)
            {
                if (lockBox.gameObject.activeSelf) lockBox.gameObject.SetActive(false);
                pulseT = 0;
                return;
            }
            AssistMath.L lk = game.assist.lk;
            Vec3 chest = lockInfo.enemy.AimPoint;
            double half = 1.1;
            // NIGHT41 P3 aim41: on a live weak point (the pad's ASSIST with aim41 on) the box frames the weak point, small; else the chest as before
            if (game.assist.LockAt41(out Vec3 weak41)) { chest = weak41; half = UnityTuning.Aim41BracketHalfM; }
            Vector3 a = cam.WorldToScreenPoint(WebSpace.ToUnity(new Vec3(chest.x, chest.y - half, chest.z)));
            Vector3 b = cam.WorldToScreenPoint(WebSpace.ToUnity(new Vec3(chest.x, chest.y + half, chest.z)));
            if (a.z < 0 || b.z < 0)
            {
                if (lockBox.gameObject.activeSelf) lockBox.gameObject.SetActive(false);
                return;
            }
            double k = Math.Min(1, lockInfo.age / lk.bracketTime);
            double tight = (lk.bracketFrom + (1 - lk.bracketFrom) * k) * (lockInfo.elite ? lk.eliteScale : 1);
            float scale = canvas.scaleFactor > 0 ? canvas.scaleFactor : 1;
            float h = Mathf.Abs(a.y - b.y) / scale * 1.1f * (float)tight;
            // NIGHT41 P3 C10 (aim41 on, a pad): the lower corners never sit on arc40's band: a box whose corners would land in it snaps to the
            // nearer clear size (inside the arc, or outside it). Off (aim41 off, -skygrindOff41, the mouse): the size as before.
            if (game.assist.weakPoint41 && PlayerDevices.Last == InputDevice.Pad)
            {
                float rc = h * UnityTuning.Aim41CornerShare, mid = (UnityTuning.Aim41CornerKeepIn + UnityTuning.Aim41CornerKeepOut) * 0.5f;
                if (rc > UnityTuning.Aim41CornerKeepIn && rc < UnityTuning.Aim41CornerKeepOut)
                    h = (rc < mid ? UnityTuning.Aim41CornerKeepIn : UnityTuning.Aim41CornerKeepOut) / UnityTuning.Aim41CornerShare;
            }
            float w = h * 0.7f;
            bool lockStarted = !lockBox.gameObject.activeSelf;
            if (lockStarted) lockBox.gameObject.SetActive(true);
            lockBox.anchoredPosition = new Vector2((a.x + b.x) * 0.5f / scale, (a.y + b.y) * 0.5f / scale);
            lockBox.localScale = new Vector3(Mathf.Max(0.3f, w / 100), Mathf.Max(0.3f, h / 100), 1);
            // SNAP's pulse: opacity 1 <-> 0.45, 0.32 s each way, eased (hud-lockpulse).
            float alpha = Lock18(frameDt, lockStarted); // NIGHT18 U3-31.6: the brackets fade in (FeelAB on; off: 1)
            if (lockInfo.pulse)
            {
                pulseT += frameDt;
                double u = pulseT / 0.32 % 2, tri = u < 1 ? u : 2 - u;
                alpha *= 1 - 0.55f * (float)(0.5 - 0.5 * Math.Cos(Math.PI * tri));
            }
            else pulseT = 0;
            if (alpha != shownLockAlpha)
            {
                shownLockAlpha = alpha;
                foreach (var part in lockParts) part.color = new Color(1, 1, 1, alpha);
            }
        }

        static Sprite VignetteSprite()
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Vignette" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.4142f;
                    byte a = (byte)(Mathf.SmoothStep(0.45f, 1f, d) * 255);
                    px[y * n + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>hud.js .hud-damage: radial-gradient(ellipse at center, transparent 38%, rgba(150,18,8,0.95) 100%): CSS's
        /// ellipse reaches the corners at 100% and the stops interpolate linearly; stretched over the screen like the hurt
        /// vignette's sprite. White with the alpha ramp; the Image's colour is the red.</summary>
        static Sprite DamageVignetteSprite()
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Damage vignette" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / 1.4142f;
                    float a = Mathf.Clamp01((d - UnityTuning.DamageVignetteClear) / (1 - UnityTuning.DamageVignetteClear)) * UnityTuning.DamageVignetteEdgeAlpha;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>hud.js .hud-hits i: a ring of `radius` px, `width` px thick, masked to `arcDeg` round straight up, with a soft glow.</summary>
        static Sprite ArcSprite(float radius, float width, float arcDeg)
        {
            int n = (int)Mathf.Ceil((radius + width + 8) * 2);
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Hit arc" };
            var px = new Color32[n * n];
            float c = n * 0.5f, half = arcDeg * 0.5f;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = x + 0.5f - c, dy = y + 0.5f - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Abs(Mathf.Atan2(dx, dy) * Mathf.Rad2Deg); // 0 = straight up
                    float mask = Mathf.Clamp01(half - ang + 0.5f);
                    float off = Mathf.Abs(d - radius) - width * 0.5f;
                    float ring = Mathf.Clamp01(0.5f - off);
                    float glow = off > 0 ? Mathf.Exp(-off * off / 18f) * 0.6f : 1; // the 6 px drop-shadow glow
                    float a = Mathf.Max(ring, glow * 0.8f) * mask;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        static readonly Dictionary<string, string> Names = new Dictionary<string, string>();

        string WeaponName(string id)
        {
            if (!Names.TryGetValue(id, out string n)) Names[id] = n = cfg.Has($"weapons.catalog.{id}.name") ? cfg.Str($"weapons.catalog.{id}.name") : id.ToUpperInvariant();
            return n;
        }

        static readonly Dictionary<string, string> ForgedNames = new Dictionary<string, string>();

        /// <summary>hud.js: a forged gun's name with its "+" (NIGHT8 U2-17), cached like WeaponName.</summary>
        string ForgedName(string id)
        {
            if (!ForgedNames.TryGetValue(id, out string n)) ForgedNames[id] = n = WeaponName(id) + "+";
            return n;
        }


        /// <summary>Frame time: the screens by the flow, the HUD's values (each written only when it changed).</summary>
        public void Render(double frameDt)
        {
            Flow f = game.UiFlow; // NIGHT14 U3-27.7: TITLE over the attract loop (else the sim's flow)
            HudFx20.DoTick(frameDt); // NIGHT20 row 5: the package effects on the HUD's frame clock, in every flow (TITLE's and RESULTS' reveals run there)
            if (settingsOpen && f != Flow.Title && f != Flow.Paused) CloseSettings(); // the run went on (or ended) under it
            if (game.DraftOpen) RenderDraft();
            if (game.settings.version != shownSettings)
            {
                shownSettings = game.settings.version;
                crossColor = CrosshairColor(game.settings.Str("crosshairColor"));
                showPrices = game.settings.Bool("showPrices");
                hurtVignetteOn = game.settings.Bool("hurtVignette");
                for (int i = 0; i < 4; i++) crossImg[i].color = crossColor;
                shownCandidate = null; // the prompt's price may come or go
                RefreshSettingRows();
            }
            if (f != shownFlow)
            {
                shownFlow = f;
                title.SetActive(f == Flow.Title && !settingsOpen);
                pause.SetActive(f == Flow.Paused && !settingsOpen && !game.DraftOpen && !Reveal20Open); // NIGHT20 F3: the reveal pauses as the draft does
                settingsScreen.SetActive(settingsOpen);
                draftScreen.SetActive(game.DraftOpen);
                down.SetActive(f == Flow.Down);
                results.SetActive(f == Flow.Results);
                if (f == Flow.Paused) RefreshPauseKeys(); // NIGHT13 S12: the live keys (each set only when it changed)
                // NIGHT17 round 3's reviewer (an orphan "1" of the top line poked out under PAUSED): the HUD's layers hide under
                // PAUSED and its menus (SETTINGS, the MODS draft); PAUSED's own header carries the run line.
                bool hudOn = f == Flow.Playing || f == Flow.Down;
                frame.gameObject.SetActive(hudOn);
                values.gameObject.SetActive(hudOn);
                sight.gameObject.SetActive(hudOn);
                if (f != Flow.Results)
                {
                    ResultsHeatBadge = null;
                    shownStats = null; // the next RESULTS stages its tally again, even for the same stats
                }
            }
            RenderMenus(f, frameDt); // NIGHT17 U3-30.3: the screens' part (UiMenus.cs, builder N)
            RenderMeta(frameDt); // NIGHT12: every flow (the title's screens, DOWN / RESULTS' strip, the HUD's toasts)
            RenderControls(frameDt);
            RenderNav(frameDt);
            RenderHotbar(frameDt);
            RenderRadial(frameDt);
            RenderReport(frameDt);
            RenderKiosk(frameDt); // NIGHT14 (the shared types)
            RenderRecap(frameDt);
            RenderDownTell20(frameDt); // NIGHT20 F4: the killer's tell under WHAT GOT YOU's block (UiDownTell20.cs, design.adds.downtell)
            RenderRoute(frameDt);
            RenderGhostSplits(frameDt);
            RenderN26Hud(frameDt); // NIGHT26 (the shared commit): see BuildN26Hud
            RenderWeapons27Hud(frameDt); // NIGHT27 (builder W): see BuildWeapons27Hud
            RenderN25Hud(frameDt); // NIGHT25 (the shared commit): see BuildN25Hud
            RenderN24Hud(frameDt); // NIGHT24 (the shared commit): see BuildN24Hud
            RenderN22Hud(frameDt); // NIGHT22 (the shared commit): see BuildN22Hud
            RenderFeelHud(frameDt); // NIGHT16 (the shared commit): the movement HUD at the crosshair (UiFeelHud.cs, builder U)
            RenderPointsPops(frameDt); // NIGHT16 polish #30 (UiPointsPops.cs)
            RenderMoveRow(frameDt); // NIGHT16 polish #35 (UiMoveRow.cs)
            RenderAds(frameDt); // NIGHT16 polish #36 (UiAds.cs)
            RenderZoom20(frameDt); // NIGHT20 U3-34.10: design.zoom's reticle and scope (UiZoom20.cs; after UiAds: it switches UiAds' off)
            RenderClimbArrow20(frameDt); // NIGHT20 U3-34.10: the climb's off-screen arrow (UiClimbArrow20.cs, design.routes)
            RenderLancer20(); // NIGHT20 C2: the LANCER's off-screen tell (UiLancer20.cs, design.adds.lancer)
            RenderReveal20(frameDt); // NIGHT20 F3: the reward reveal (UiReveal20.cs; every flow: it runs paused)
            RecordCodex20(frameDt); // NIGHT20 E3: the kinds met in a run (UiCodex20.cs)
            RenderFeelTags(frameDt); // NIGHT16 polish #37 / #38 (UiFeelTags.cs)
            Payoff18(frameDt); // NIGHT18 U3-31.6: the PAYOFF strip's entrance on DOWN / RESULTS (UiPayoff.cs)
            RenderHud30(f, frameDt); // NIGHT30 (builder V, design.n30.hud30): every flow (the run rows hide under a menu); the widget before RenderFeel's card fit
            RenderHud33(f, frameDt); // NIGHT33 U (design.n33.hud33): the rebuilt HUD over the hidden NIGHT32 layers (UiHud33.cs); off: nothing
            if (!values.gameObject.activeSelf) return;
            Health h = game.health;
            float frac = (float)(h.hp / Math.Max(1, h.max));
            SetFill(hpFill, ref shownHpFrac, frac);
            // hud.js :916-917: eased on frame time, written at two decimals
            damageShown += ((1 - Math.Max(0, frac)) * regenVignetteMax - damageShown) * (1 - Math.Exp(-regenVignetteEase * frameDt));
            float da = (float)Math.Round(damageShown, 2);
            if (da != shownDamage)
            {
                shownDamage = da;
                damageVignette.color = new Color(damageColor.r, damageColor.g, damageColor.b, da);
            }
            float capFrac = h.hard > 0 ? (float)Math.Max(0, h.Cap / Math.Max(1, h.max)) : 1; // NIGHT11: ULTRA HEALTH's grey cap
            if (capFrac != shownHardFrac)
            {
                shownHardFrac = capFrac;
                hpHard.enabled = capFrac < 1;
                hpHard.rectTransform.anchorMin = new Vector2(capFrac, 0);
            }
            bool low = frac <= lowHpFraction;
            blink += frameDt;
            bool blinkOn = low && (int)(blink / lowHpBlinkTime) % 2 == 0;
            if (blinkOn != shownHpLow)
            {
                shownHpLow = blinkOn;
                hpFill.color = blinkOn ? UiTheme.Danger : UiTheme.White; // NIGHT17 U3-30.7: the tile's white bar; low HP blinks DANGER
            }
            SetInt(hpText, ref shownHp, (int)Math.Ceiling(h.hp), UnityTuning.Hud17Tabular + "{0:0}");
            if (hudMotion) RenderHudMotion(frameDt, frac); // NIGHT12 U3-25.6: the HP ghost, the plates
            RenderPoints17(frameDt); // NIGHT17 U3-30.7: the odometer (UiHudMotion.cs)
            RenderStaminaBar(frameDt); // NIGHT19 U3-33.8: the stamina pips in the vitals plate (UiStaminaBar.cs)
            RenderVitals20(frameDt, frac); // NIGHT20 U3-34.10: the segment ticks, the regen's reach, the refill (UiVitals20.cs)
            RenderCooldown20(frameDt); // NIGHT20 U3-34.10: the shockwave chip on the card (UiCooldown20.cs; after the hotbar's layout)
            RenderHudScale17(); // NIGHT17 U3-30.7: HUD SCALE (UiHudMotion.cs)
            if (ceremonyRound) RenderCeremony(frameDt); // NIGHT12 U3-25.6: the round counter, the clear flash, the burn-in
            RenderPerks(frameDt); // NIGHT18 U3-31.6: frame time for the icons' entrance
            RenderPowerUps(frameDt); // NIGHT17 round 3: the pickup moment and the chips (UiPowerUps.cs)
            RenderMarkers(frameDt); // NIGHT18 round 1: frame time for the markers' entrance
            RenderFeel(frameDt); // the weapon name + its freshness tier, the speed lines
            RenderMomentumArc(); // NIGHT9 U3-09: the momentum ring round the crosshair
            SurvivalRun run = game.run;
            int ra = run.round, rb = run.state == SurvivalRun.State.Break ? (int)Math.Ceiling(run.BreakLeft) : run.state == SurvivalRun.State.Fight ? (game.WyrmFightLive || game.setup.mode == RunSetup.Campaign ? -1 : game.director.Remaining) : 0; // NIGHT25 (W): -1 while the worm fights: the survival counter is not a boss fight's line
            // NIGHT30 (builder V, design.n30.hud30; the lead's round 4): a campaign has no survival rounds: its break line ("ROUND 1 IN 82") never shows in a
            // campaign room, the boss rooms included (-2: no line). Survival runs keep it.
            bool noRounds30 = hud30On && game.setup.mode == RunSetup.Campaign && run.state == SurvivalRun.State.Break;
            if (noRounds30) rb = -2;
            else if (HideBreakLine32(run)) { noRounds30 = true; rb = -2; } // NIGHT32 G (hud32): no "ROUND 1 IN 83" over a live boss fight, any mode (UiHud32.cs)
            bool canReady = run.state == SurvivalRun.State.Break && ra >= run.readyFrom && !noRounds30;
            bool grindLine = game.setup.mode == RunSetup.Grind; // NIGHT22: THE GRIND's waves are its rounds: the top line says WAVE
            long shapeKey = RunShapeKey(); // NIGHT11: the objective, the PROTOCOL and exfil ride the line (main.js topText)
            if (breakReadyPad != (PlayerDevices.Last == InputDevice.Pad)) { BreakReadyRefresh(); shownReady = !canReady; } // NIGHT23: the device changed: the break line's key word follows (S12)
            if (run.state != shownRunState || ra != shownRoundA || rb != shownRoundB || canReady != shownReady || shapeKey != shownShapeKey)
            {
                shownRunState = run.state;
                shownReady = canReady;
                shownRoundA = ra;
                shownRoundB = rb;
                shownShapeKey = shapeKey;
                topLineDirty = true; // NIGHT12: its plate follows the text's width (UiHudMotion.cs)
                if (shapeKey == 0)
                {
                    if (run.state == SurvivalRun.State.Break && rb == -2) roundText.SetText(string.Empty); // NIGHT30: a campaign's break: no survival line
                    else if (run.state == SurvivalRun.State.Break) roundText.SetText(grindLine ? (canReady ? "WAVE {0:0} IN {1:0}" + breakReadySuffix : "WAVE {0:0} IN {1:0}") : canReady ? breakReadyFormat : "ROUND {0:0} IN {1:0}", ra, rb); // NIGHT22: the GRIND says WAVE
                    else if (run.state == SurvivalRun.State.Fight)
                    {
                        // NIGHT17 round 2's reviewer: the round lives here ("ROUND 1 · 3 LEFT"), not in a corner
                        if (rb < 0) roundText.SetText(string.Empty); // NIGHT25 (W): a boss fight has no round counter (GG's screenshot: ROUND 1 · 0 LEFT over the worm)
                        else roundText.SetText(grindLine ? "WAVE {0:0} · {1:0} LEFT" : "ROUND {0:0} · {1:0} LEFT", ra, rb);
                    }
                    else roundText.SetText(string.Empty);
                }
                else
                {
                    // Set only when the line's state changed (a second ticks, a kill, a relay): a string then, not per frame.
                    sb.Clear();
                    if (run.state == SurvivalRun.State.Break && rb != -2) // NIGHT30: none in a campaign
                    {
                        sb.Append(grindLine ? "WAVE " : "ROUND ").Append(ra).Append(" IN ").Append(rb);
                        if (canReady) sb.Append(breakReadySuffix);
                    }
                    else if (run.state == SurvivalRun.State.Fight) sb.Append(grindLine ? "WAVE " : "ROUND ").Append(ra).Append(" · ").Append(rb).Append(" LEFT");
                    AppendRunShape(sb, run.state == SurvivalRun.State.Fight);
                    roundText.SetText(sb);
                }
            }
            SetActive(exposed.gameObject, ref shownExposed, game.director.Exposed);
            Exposed18(frameDt, shownExposed); // NIGHT18 U3-31.6: its entrance (FeelAB on)
            // Buy prompt
            Buyables.Item c = game.Active ? game.buyables.candidate : null;
            SetActive(prompt.gameObject, ref shownPrompt, c != null);
            Prompt18(frameDt, shownPrompt); // NIGHT18 U3-31.6: its entrance (FeelAB on)
            bool promptPoor = false;
            if (c != null)
            {
                double price = game.economy.Price(c.price()); // buyables.js: the prompt shows a pending REFUND
                bool can = game.economy.CanAfford(price);
                promptPoor = !(c.owned != null && c.owned()) && !can; // hud.js: !pr.owned && !pr.affordable
                if (c != shownCandidate || can != shownCan || price != shownPrice || !ReferenceEquals(c.name, shownCandidateName))
                {
                    shownCandidate = c;
                    shownCandidateName = c.name; // the FORGE's name follows the gun in hand
                    shownCan = can;
                    shownPrice = price;
                    sb.Clear();
                    if (c.owned != null && c.owned()) sb.Append(c.name).Append("  ·  OWNED"); // hud.js buyLabel (NIGHT11: a lit relay)
                    else if (can)
                    {
                        sb.Append(PromptText.For("interact", true, "HOLD E")).Append("  ·  ").Append(c.name); // NIGHT19: PromptText (builder P)
                        if (c.kind == "box") sb.Append(' ').Append((long)Math.Round(price)); // NIGHT10: "MYSTERY BOX 950" (CoD shows the box's cost)
                        else if (c.kind == "boxTake") { } // a box gun waiting to be taken costs nothing
                        else if (price == 0) sb.Append("  ·  <b>FREE</b>"); // hud.js buyLabel: a price of 0 always says FREE (NIGHT11: relays, the power, the radio)
                        else if (showPrices) sb.Append("  ·  <color=#").Append(goldHex).Append('>').Append((long)Math.Round(price)).Append("</color>"); // hud.js buyLabel: no price unless SHOW BUY PRICES
                    }
                    else
                    {
                        sb.Append(c.name);
                        if (c.kind == "box") sb.Append(' ').Append((long)Math.Round(price));
                        else if (showPrices) sb.Append("  ·  ").Append((long)Math.Round(price));
                        sb.Append("  ·  <color=#").Append(mutedHex).Append(">NOT ENOUGH POINTS</color>");
                    }
                    prompt.SetText(sb);
                }
                SetFill(promptBar, ref shownPromptFill, (float)game.buyables.Progress);
            }
            else shownCandidate = null;
            BuyPlate19();
            RenderFeelEdges(frameDt, game.director.Exposed, c != null, promptPoor); // NIGHT16 polish #32-#34 (UiFeelEdges.cs)
            // Hit marker (hud.js: its opacity is the time left over hitMarkerTime; NIGHT12 U3-25.6 the grammar's shapes)
            markerLeft = Math.Max(0, markerLeft - frameDt);
            markerTwistLeft = Math.Max(0, markerTwistLeft - frameDt);
            bool markerOn = markerLeft > 0;
            // NIGHT18 U3-31.6 (FeelAB on): the arms grow out from the crosshair, scale and distance together, drawn at their
            // age then stepped (the hit's own frame at Hud18HitMarkerFrom: M9's pop-in rule, since a hit's alpha fades over
            // its whole life and only the scale can carry the entrance); off: 1, NIGHT17's marker.
            float grow = 1f;
            if (FeelAB.On && markerOn)
            {
                grow = UiTween.PopIn(markerGrowAge / UnityTuning.Hud18HitMarkerIn, markerGrowFrom, UnityTuning.Hud18HitMarkerPeak, 0.5);
                markerGrowAge += frameDt;
            }
            bool markerBold18 = FeelAB.On; // NIGHT18 round 1: bolder arms over an INK underlay (off: NIGHT17's 2 px arms)
            if (markerOn || shownMarker || grow != markerShownGrow || markerUnderShown18)
            {
                markerUnderShown18 = markerOn && markerBold18;
                float gap = (float)hitMarkerGap * (float)markerScale;
                float alpha = (float)(markerLeft / hitMarkerTime);
                Color mc = markerColor;
                mc.a = alpha;
                float twist = markerKill && UnityTuning.KillMarkerTwistTime > 0 ? UnityTuning.KillMarkerTwistDeg * (float)(markerTwistLeft / UnityTuning.KillMarkerTwistTime) : 0;
                bool twin = markerOn && hudMotion && markerHead;
                bool diamond = markerOn && markerHead; // NIGHT16 U3-29.7: the headshot's shape, with or without hudMotion
                if (markerHeadDiamond.enabled != diamond) markerHeadDiamond.enabled = diamond;
                if (diamond)
                {
                    markerHeadDiamond.color = mc;
                    float ds = (float)markerScale * grow;
                    markerHeadDiamond.rectTransform.localScale = new Vector3(ds, ds, 1);
                }
                bool regrow = grow != markerShownGrow;
                markerShownGrow = grow;
                var growScale = new Vector3(grow, grow, 1);
                for (int i = 0; i < 4; i++)
                {
                    markerImg[i].enabled = markerOn;
                    if (markerTwin[i].enabled != twin) markerTwin[i].enabled = twin;
                    if (regrow)
                    {
                        marker[i].localScale = growScale;
                        markerTwin[i].rectTransform.localScale = growScale;
                    }
                    if (!markerOn)
                    {
                        HitUnder18(i, false, default, default, default, default, 0);
                        continue;
                    }
                    markerImg[i].color = mc;
                    float deg = 45 + 90 * i + twist;
                    float a = deg * Mathf.Deg2Rad;
                    var dir = new Vector2(-Mathf.Sin(a), Mathf.Cos(a));
                    float armLen = (float)hitMarkerSize * (float)markerScale * (markerBold18 ? UnityTuning.Hud18HitMarkerLength : 1f);
                    Vector2 at = dir * (gap + armLen * 0.5f) * grow;
                    var size = new Vector2(markerBold18 ? UnityTuning.Hud18HitMarkerWidth : 2, armLen);
                    marker[i].anchoredPosition = at;
                    marker[i].sizeDelta = size;
                    marker[i].localRotation = Quaternion.Euler(0, 0, deg);
                    HitUnder18(i, markerBold18, at, size, marker[i].localRotation, growScale, alpha);
                    FeelProbe.EffectUi(FeelFx.HitMarker, marker[i], alpha);
                    if (!twin) continue;
                    // the headshot's second tick, beside the arm (perpendicular to it)
                    RectTransform tr = markerTwin[i].rectTransform;
                    tr.anchoredPosition = at + new Vector2(dir.y, -dir.x) * UnityTuning.HeadMarkerTwinGap * grow;
                    tr.sizeDelta = size;
                    tr.localRotation = marker[i].localRotation;
                    markerTwin[i].color = mc;
                }
                shownMarker = markerOn;
            }
            RenderKillMarker17(frameDt, f == Flow.Playing); // NIGHT17 U3-30.7 (UiMarkers.cs)
            RenderKillPop53(f == Flow.Playing); // NIGHT53 U polish53 (killPop; UiKillPop53.cs)
            RenderRideKill53(frameDt, f == Flow.Playing); // EXT53 N rideKill53 (UiRideKill53.cs)
            RenderClear55(frameDt, f == Flow.Playing); // NIGHT55 R clear55 (UiClear55.cs)
            // An elite's ring burst: grows from EliteRingFrom to 1 while it fades.
            if (eliteRingLeft > 0 || eliteRing.enabled)
            {
                eliteRingLeft = Math.Max(0, eliteRingLeft - frameDt);
                bool ringOn = eliteRingLeft > 0 && f == Flow.Playing;
                if (eliteRing.enabled != ringOn) eliteRing.enabled = ringOn;
                if (ringOn)
                {
                    float k = (float)(eliteRingLeft / (hitMarkerTime * UnityTuning.EliteRingLife)); // 1 -> 0
                    float s = Mathf.Lerp(1, UnityTuning.EliteRingFrom, k * k);
                    eliteRing.rectTransform.localScale = new Vector3(s, s, 1);
                    Color rc = eliteRingColor;
                    rc.a = k;
                    eliteRing.color = rc;
                }
            }
            // The aim-assist lock: the crosshair's state and the brackets
            LockInfo lockInfo = game.Active ? game.assist.Info() : null;
            snapFlash = Math.Max(0, snapFlash - frameDt);
            killFlash = Math.Max(0, killFlash - frameDt);
            bool locked = lockInfo != null, white = locked || snapFlash > 0;
            // NIGHT16 polish #25 (hud.js :885-890): the spread: dash (held) + the shotgun's shot (recovering), whole tenths
            // of a px so the arms move only when it changes.
            crossShotSpread = Math.Max(0, crossShotSpread - frameDt);
            bool crossDashing = game.mover != null && game.mover.state == MoveState.Dash;
            float spread = Mathf.Round((float)((crossDashing ? crossDashSpread : 0) + crossShotgunSpread * (crossShotSpread / crossSpreadRecover)) * 10) / 10;
            if (locked != shownLocked || spread != shownCrossSpread)
            {
                shownLocked = locked;
                shownCrossSpread = spread;
                float close = (locked ? 2 : 0) - spread;
                cross[0].anchoredPosition = new Vector2(-13 + close, 0);
                cross[1].anchoredPosition = new Vector2(13 - close, 0);
                cross[2].anchoredPosition = new Vector2(0, 13 - close);
                cross[3].anchoredPosition = new Vector2(0, -13 + close);
            }
            Color cc = white ? Color.white : killFlash > 0 ? killFlashColor : crossColor; // hud.js: snap white, then the kill flash
            cc.a *= adsTickAlpha; // NIGHT16 polish #36: hud.js --tick, the arms fade to 1 - 0.6 k while zoomed (UiAds.cs)
            if (crossImg[0].color != cc) for (int i = 0; i < 4; i++) crossImg[i].color = cc;
            Brackets(lockInfo, frameDt);
            // The damage-direction arcs (main.js updateHitArcs): aged on frame time, drawn while playing, following the look.
            game.hitArcs.Step(frameDt, hitLife);
            int shown = 0;
            if (f == Flow.Playing)
            {
                Vec3 feet = game.Feet;
                foreach (HitArcs.Arc a in game.hitArcs.arcs)
                {
                    if (a.age == double.PositiveInfinity || shown >= hitImgs.Count) continue;
                    UnityEngine.UI.Image hi = hitImgs[shown++];
                    hi.enabled = true;
                    hi.rectTransform.localRotation = Quaternion.Euler(0, 0, -(float)HitArcs.HitAngle(game.input.yaw, feet.x, feet.z, a.x, a.z));
                    float arcAlpha = (float)HitArcs.Alpha(a.age, hitFadeFrom, hitLife);
                    // NIGHT18 U3-31.6 (FeelAB on): the arc grows out to its ring and fades in over its first frames
                    float arcScale = FeelAB.On ? HudEntrance18.Scale(a.age) : 1f;
                    if (FeelAB.On) arcAlpha *= HudEntrance18.Alpha(a.age);
                    if (hi.rectTransform.localScale.x != arcScale) hi.rectTransform.localScale = new Vector3(arcScale, arcScale, 1);
                    hi.color = new Color(danger.r, danger.g, danger.b, arcAlpha);
                }
            }
            for (int i = shown; i < hitImgs.Count; i++) if (hitImgs[i].enabled) hitImgs[i].enabled = false;

            // The style rank: NIGHT17 U3-30.7's style meter (UiStyleMeter.cs) replaces the rank box and its feed.
            RenderStyleMeter(frameDt);
            // Banner + vignette
            RenderAnnouncer20(frameDt); // NIGHT20 F1: the announcer's slab (UiAnnouncer20.cs); the banner waits under it
            RenderNext20(frameDt); // NIGHT20 A8: the NEXT line in the break (UiNext20.cs)
            RenderDebut20(frameDt); // NIGHT20 C1: the kinds' debut cards (UiDebut20.cs; after the slab: it waits for one)
            RenderThreat20(frameDt); // NIGHT20 A1 / A4: the boss bar and the elite's plate (UiThreat20.cs)
            bool slabHold20 = Announcer20Holds;
            if (!slabHold20) bannerLeft = Math.Max(0, bannerLeft - frameDt);
            if (bannerLeft <= 0 && bannerQueue.Count > 0)
            {
                var next = bannerQueue[0];
                bannerQueue.RemoveAt(0);
                ShowBanner(next.text, next.color, next.time);
            }
            float ba = UiTween.OutCubic(bannerLeft / 0.4); // NIGHT17 U3-30.7: its last 0.4 s fade with an ease (nothing linear)
            ba = Banner18(frameDt, ba); // NIGHT18 U3-31.6: its entrance (FeelAB on; off: ba as it was)
            if (slabHold20) ba = 0; // NIGHT20 F1: hidden (and its time paused) while a slab stands at its place
            ba = Mathf.Round(ba * 100f) / 100f; // written in hundredths
            if (ba != shownBannerAlpha)
            {
                shownBannerAlpha = ba;
                Color bc = banner.color;
                bc.a = ba;
                banner.color = bc;
            }
            LowHealthEdge31Swap(); // NIGHT31 G (P0 d, design.n31.lowHealthEdge): the two red vignettes' sprites follow the row (edges only when on; UiEdge31.cs)
            vignetteLeft = Math.Max(0, vignetteLeft - frameDt);
            if (hurtFlashAge < hurtVignetteTime) hurtFlashAge += frameDt;
            float va = (float)(hurtVignetteOpacity * vignetteLeft / hurtVignetteTime) * FlashAttack18(hurtFlashAge); // NIGHT18 U3-31.6: it swells in (FeelAB on; off: x 1)
            if (low && hurtVignetteOn)
            {
                // LOW HEALTH VIGNETTE (settings.hurtVignette). NIGHT17 U3-30.7: it breathes slowly (DANGER, no new hue); steady
                // with REDUCED MOTION. Written in hundredths, so it moves only as often as it shows.
                lowHpBreath = game.settings.View.reducedMotion ? 0 : (lowHpBreath + frameDt) % UnityTuning.Hud17LowHpPeriod;
                // NIGHT18 U3-31.6 round 1 (the lead; X's backlog 4): with FeelAB on the HUD's DANGER breath follows PostFx's
                // low-HP beat (one rhythm at the screen's edges, not two); REDUCED MOTION: steady, as before. Off: NIGHT17's 1.2 s
                // cosine.
                float breath = FeelAB.On
                    ? (game.settings.View.reducedMotion ? 0f : PostFx.LowHpPulseNow)
                    : 0.5f - 0.5f * Mathf.Cos((float)(lowHpBreath / UnityTuning.Hud17LowHpPeriod) * Mathf.PI * 2f);
                va = Math.Max(va, Mathf.Round((UnityTuning.Hud17LowHpBase + UnityTuning.Hud17LowHpAmp * breath) * 100f) / 100f);
            }
            if (va != shownVignette)
            {
                shownVignette = va;
                vignette.color = new Color(danger.r, danger.g, danger.b, va);
            }
        }
    }

    /// <summary>The gates' price signs (zones.js makeLabel): name + price floating over each buy spot, shown while buyable.</summary>
    public sealed class ZoneSigns
    {
        readonly SurvivalGame game;
        readonly List<(BuySpot spot, TextMeshPro t)> signs = new List<(BuySpot, TextMeshPro)>();

        public ZoneSigns(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            Color gate = Look.C(cfg, "barrier.gateColor");
            var root = new GameObject("Zone signs").transform;
            float scale = (float)cfg.Num("zones.labelScale");
            foreach (BuySpot s in game.zones.spots)
            {
                var go = new GameObject($"Sign {s.name}", typeof(TextMeshPro));
                go.transform.SetParent(root, false);
                go.transform.position = WebSpace.ToUnity(s.label);
                var t = go.GetComponent<TextMeshPro>();
                // NIGHT17 round 3's reviewer ("+111" fused with "THE GRINDER"): the world's signs in BODY, upright, never the
                // HUD's display italic.
                if (UiTheme.BodyBold != null) t.font = UiTheme.BodyBold;
                t.text = $"<b>{s.name}</b>\n<color=#ffc7ad>{s.price:0}</color>";
                t.alignment = TextAlignmentOptions.Center;
                t.fontSize = 6;
                t.color = Color.white;
                t.outlineColor = gate;
                t.outlineWidth = 0.15f;
                t.rectTransform.sizeDelta = new Vector2(scale, scale * 176 / 512f);
                t.enableWordWrapping = false;
                signs.Add((s, t));
            }
        }

        public void Render(Camera cam)
        {
            bool prices = game.settings.Bool("showPrices"); // zones.js: the labels show only with SHOW BUY PRICES
            foreach (var (spot, t) in signs)
            {
                Flow uf = game.UiFlow; // NIGHT14: TITLE over the attract hides them too
                // NIGHT13 S8: never behind RESULTS, DOWN or TITLE. NIGHT16 (build 3's UI pass, Logs/shots/n16-b3/n16-paused.png):
                // nor behind PAUSED: a sign read through the pause menu between its buttons.
                bool show = prices && uf == Flow.Playing && game.zones.SpotBuyable(spot);
                if (t.gameObject.activeSelf != show) t.gameObject.SetActive(show);
                if (show) t.transform.rotation = Quaternion.LookRotation(t.transform.position - cam.transform.position);
            }
        }
    }
}
