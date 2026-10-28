// NIGHT8 U2-06: two HUD feel pieces.
// - Speed lines (player.js): opacity eases (feel.speedLinesFadeTime) toward min(1, (dashing ? 1 : the speed's share
//   between feel.speedLinesMinSpeed and speedLinesFullSpeed) x speedLinesMaxOpacity x effects intensity), 0 with reduced
//   motion; the streaks jitter by speedLinesJitterDeg (cosmetic noise, no random stream). A generated radial-streak sprite
//   over the whole screen (the web draws its own streak quad).
//   NIGHT18 U3-31.5 (builder M, M8; FeelAB off: the above): the alpha from the speed's share of dash.speed, over 0.3 only
//   above 70 % of it (whatever the fade: a dash's end drops it in that frame), faint edge lines while sprinting, whole on a
//   boost's first frame or two; the streaks start outside the screen's central 60 % (UnityTuning.CameraMove.cs).
// - The weapon's freshness tier by its name (FRESH / USED / STALE, styleMath freshnessTierName, coloured as hud.js
//   .hud-fresh: FRESH usable, USED #d8d4cc at 0.65, STALE danger at 0.5); none for a weapon without freshness.
//   NIGHT13 U3-26.9: the line names the requested gun (loadout.targetSlot, as hud.js's .hud-wname) and is the name row of
//   the weapon block's plate (UiHotbar.cs: the hotbar and the slot label over it, the meter and the special hint under
//   it); the tier stays beside the name (the web shows the tiers in the style block's .hud-fresh icons instead).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image speedLines;
        double lines, linesClock;
        float shownLines = -1;
        string shownTier;
        string freshHex, usedHex, staleHex;

        // The parry's white flash (hud.js flashScale: punch.parryFlashOpacity x settings.flashIntensity, over
        // punch.parryFlashTime). NIGHT17 U3-30.7 (round 1's reviewer: a PERFECT parry whited out the frame, HUD and all, for
        // its whole ~74 ms freeze): always punch.parryFlashTime, its alpha capped at Hud17ParryFlashMax, drawn in the HUD's
        // frame layer under every plate and number; none on a PERFECT parry (builder X draws its impact frame and ring).
        UnityEngine.UI.Image whiteFlash;
        double flashLeft, flashTime;
        float shownFlash = -1;
        float linesShownX = float.NaN; // NIGHT17: the speed lines' centre, eased toward the direction of travel
        // NIGHT18 U3-31.5 (builder M, M8): the lines' alpha from the speed's share of dash.speed (CameraMove.LinesTarget / LinesStep:
        // over 0.3 only above 70 % of it; the sprint's faint edge lines), a sprite whose streaks start outside the screen's
        // central 60 % (CameraMove.StreakAlpha), a smaller sideways shift; the probe hears the lines (FeelFx.SpeedLines).
        // FeelAB off: NIGHT17's (the web's alpha, the radial sprite, Hud17LinesShift).
        Sprite linesSprite17, linesSprite18, linesSpriteBoost18;
        UnityEngine.UI.Image speedLinesBoost; // NIGHT18 round 2: the boost's layer (reaching in to the central box's edge)
        double linesBoost;
        float shownLinesBoost = -1;
        int linesAbVersion = -1;
        double linesTopSpeed;
        StaminaHud.Info linesStamina;

        void BuildFeel()
        {
            whiteFlash = Img(frame, "Parry flash", UiTheme.A(UiTheme.White, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            whiteFlash.transform.SetAsFirstSibling(); // under the plates (the speed lines go under it next)
            game.events.On<HitEvent>("parry", e =>
            {
                if (e.owner != "player" || e.perfect) return; // a PERFECT parry: X's impact frame instead
                // NIGHT16 review (finding 7): the game's one flash gate (FlashGate.cs, three in any second); refused, or FLASH
                // INTENSITY 0: no flash (the parry's freeze stays).
                if (!(game.settings.Num("flashIntensity") > 0) || !game.flashGate.TryFlash(SurvivalGame.RealNow)) return;
                flashTime = cfg.Num("punch.parryFlashTime");
                flashLeft = flashTime;
            });
            // NIGHT17 U3-30.7 (round 1's reviewer: the boost's lines crossed the HP plate): the lines live in the HUD's frame
            // layer as its first child, so every plate and number draws over them.
            speedLines = Img(frame, "Speed lines", UiTheme.A(UiTheme.White, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            speedLines.sprite = linesSprite17 = StreakSprite();
            speedLines.preserveAspect = false;
            linesTopSpeed = cfg.Num("slide.floorSpeed"); // NIGHT18 round 1: M8's top speed, the fastest sustained ground move (the boost is a 0.25 s burst)
            speedLines.transform.SetAsFirstSibling();
            speedLinesBoost = Img(frame, "Speed lines boost", UiTheme.A(UiTheme.White, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            speedLinesBoost.preserveAspect = false;
            speedLinesBoost.transform.SetAsFirstSibling(); // NIGHT18 round 2: under the plates too
            // NIGHT17 U3-30.7: the tier on the cyan value ladder (FRESH cyan, USED pale, STALE muted): red-orange is danger only.
            freshHex = UiTheme.CyanHex;
            usedHex = UiTheme.CyanPaleHex;
            staleHex = UiTheme.MutedHex;
        }

        static Sprite StreakSprite()
        {
            const int n = 512;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Speed lines" };
            var px = new Color32[n * n];
            const int streaks = 90;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = 0;
                    if (r > 0.45f)
                    {
                        float ang = (Mathf.Atan2(dy, dx) / (2 * Mathf.PI) + 0.5f) * streaks;
                        int i = Mathf.FloorToInt(ang);
                        float f = ang - i;
                        float center = 0.5f + 0.35f * (float)Cosmetic.Lattice(i, 5); // each streak at its own angle in its bin
                        float width = 0.06f + 0.05f * (float)(Cosmetic.Lattice(i, 9) * 0.5 + 0.5);
                        float inner = 0.5f + 0.25f * (float)(Cosmetic.Lattice(i, 13) * 0.5 + 0.5); // where it starts
                        float line = Mathf.Clamp01(1 - Mathf.Abs(f - center) / width);
                        a = line * Mathf.SmoothStep(inner, inner + 0.25f, r);
                    }
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>NIGHT18 U3-31.5: the streaks start outside a box (CameraMove.StreakAlpha), so none crosses the central 60 %;
        /// round 2: a layer's sprite (the boost's, or the run's off the HUD's bottom corners).</summary>
        static Sprite StreakSprite18(bool boost)
        {
            const int n = 512;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Speed lines N18" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float a = CameraMove.StreakAlpha((x + 0.5f) / n * 2 - 1, (y + 0.5f) / n * 2 - 1, boost);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
        }

        /// <summary>NIGHT18 round 3 (final): the boost layer's sprite, LinesBoostStreaks thin radial lines from the central box's
        /// edge to the screen's (CameraMove.BoostStreakAlpha) on a texture whose texels are near square on screen.</summary>
        static Sprite BoostStreakSprite18()
        {
            int w = UnityTuning.LinesBoostTexW, h = UnityTuning.LinesBoostTexH;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Speed lines boost N18" };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(CameraMove.BoostStreakAlpha(x, y, w, h) * 255));
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        }

        void RenderFeel(double frameDt)
        {
            if (speedLines == null) BuildFeel();
            flashLeft = System.Math.Max(0, flashLeft - frameDt);
            double peak = System.Math.Min(UnityTuning.Hud17ParryFlashMax, cfg.Num("punch.parryFlashOpacity"));
            float fa = flashTime > 0 ? (float)(flashLeft / flashTime * peak * System.Math.Max(0, System.Math.Min(1, game.settings.Num("flashIntensity")))) : 0;
            if (fa != shownFlash)
            {
                shownFlash = fa;
                whiteFlash.color = UiTheme.A(UiTheme.White, fa);
            }
            MovementConfig mc = cfg.movement;
            double intensity = System.Math.Max(0, System.Math.Min(mc.feel.effectsIntensityMax, mc.settings.effectsIntensity));
            double calm = mc.settings.reducedMotion ? 0 : 1;
            if (linesAbVersion != FeelAB.Version)
            {
                linesAbVersion = FeelAB.Version;
                if (FeelAB.On && linesSprite18 == null)
                {
                    linesSprite18 = StreakSprite18(false); // made once, on the first N18 frame
                    linesSpriteBoost18 = BoostStreakSprite18(); // round 3: thin, long radial streaks
                    speedLinesBoost.sprite = linesSpriteBoost18;
                }
                speedLines.sprite = FeelAB.On ? linesSprite18 : linesSprite17;
            }
            if (FeelAB.On)
            {
                // NIGHT18 U3-31.5 (M8): over 0.3 only above 70 % of dash.speed, whatever the fade; the sprint's faint edge lines
                StaminaHud.Read(game, ref linesStamina);
                double share = linesTopSpeed > 0 ? game.mover.Speed / linesTopSpeed : 0;
                bool sprinting = linesStamina.valid && linesStamina.sprinting;
                double want = game.Active ? System.Math.Min(1, CameraMove.LinesTarget(share, sprinting) * intensity) * calm : 0;
                lines = CameraMove.LinesStep(lines, want, share, frameDt, UnityTuning.LinesFadeOutN18);
                // round 2: the boost's layer, whole on its first frame, the quiet tail after
                double wantBoost = game.Active ? System.Math.Min(1, CameraMove.LinesBoostTarget(share) * intensity) * calm : 0;
                linesBoost = CameraMove.LinesStep(linesBoost, wantBoost, share, frameDt, UnityTuning.LinesBoostFadeIn, UnityTuning.LinesFadeOutN18);
            }
            else
            {
                double min = cfg.Num("feel.speedLinesMinSpeed"), full = cfg.Num("feel.speedLinesFullSpeed");
                double fromSpeed = System.Math.Max(0, System.Math.Min(1, (game.mover.Speed - min) / (full - min)));
                bool dashing = game.mover.state == MoveState.Dash;
                double target = game.Active ? System.Math.Min(1, (dashing ? 1 : fromSpeed) * cfg.Num("feel.speedLinesMaxOpacity") * intensity) * calm : 0;
                double step = frameDt / cfg.Num("feel.speedLinesFadeTime");
                lines = lines < target ? System.Math.Min(target, lines + step) : System.Math.Max(target, lines - step);
                linesBoost = 0;
            }
            // NIGHT20 U3-34.7 (builder V): the two screen layers retire behind the AIR streaks (SpeedLines20.cs: world space,
            // by speed, pointing away from the travel); with -skygrindOff20 (the A side) they draw as NIGHT19's.
            if (SpeedLines20.Live) lines = linesBoost = 0;
            float a = (float)lines, ab = (float)linesBoost;
            if (ab != shownLinesBoost)
            {
                shownLinesBoost = ab;
                speedLinesBoost.color = UiTheme.A(UiTheme.White, ab);
            }
            float shift = UnityTuning.Pick(FeelAB.On, UnityTuning.LinesShiftN18, UnityTuning.LinesShiftN17);
            if (a != shownLines)
            {
                shownLines = a;
                speedLines.color = UiTheme.A(UiTheme.White, a);
            }
            if (a > 0 || ab > 0)
            {
                linesClock += frameDt;
                // NIGHT18 round 2: no jitter turn (LinesJitterN18 0: the boost's streaks reach the central box's edge)
                float jitter = (float)(cfg.Num("feel.speedLinesJitterDeg") * UnityTuning.Pick(FeelAB.On, UnityTuning.LinesJitterN18, UnityTuning.LinesJitterN17) * Cosmetic.ValueNoise(linesClock * 30, 17));
                speedLines.rectTransform.localRotation = Quaternion.Euler(0, 0, jitter);
                speedLinesBoost.rectTransform.localRotation = Quaternion.Euler(0, 0, jitter);
                // NIGHT17 (round 1's reviewer: a strafe boost burst from the centre): the lines' centre moves toward the
                // direction of travel, the sideways share of the velocity in the view x Hud17LinesShift of half the canvas,
                // eased (Hud17LinesEase per s) so it never jumps. The image is wider than the canvas by the most it can move.
                float half = root.rect.width * 0.5f;
                float want = 0;
                if (game.cam != null && game.mover != null)
                {
                    Vector3 travel = game.cam.transform.InverseTransformDirection(WebSpace.ToUnity(game.mover.velocity));
                    want = Mathf.Clamp(travel.x / Mathf.Max(0.1f, Mathf.Abs(travel.x) + Mathf.Abs(travel.z)), -1f, 1f) * shift * half;
                }
                if (float.IsNaN(linesShownX)) linesShownX = want;
                linesShownX += (want - linesShownX) * (1f - Mathf.Exp(-(float)frameDt * UnityTuning.Hud17LinesEase));
                float x = Mathf.Round(linesShownX);
                RectTransform lr = speedLines.rectTransform;
                if (lr.anchoredPosition.x != x) lr.anchoredPosition = new Vector2(x, 0);
                float grow = 2f * shift * half;
                if (lr.sizeDelta.x != grow) lr.sizeDelta = new Vector2(grow, 0);
                RectTransform br = speedLinesBoost.rectTransform; // the boost's layer on the same rect
                if (br.anchoredPosition.x != x) br.anchoredPosition = new Vector2(x, 0);
                if (br.sizeDelta.x != grow) br.sizeDelta = new Vector2(grow, 0);
                // NIGHT18: the reel hears the lines: the image's rect on screen (the union of its streaks), their alpha
                if (FeelProbe.Recording && FeelProbe.UiScreenRect(lr, out Rect r))
                {
                    // NIGHT18 (M8): the box the streaks leave clear in the middle, in Unity screen pixels: from the image's own
                    // screen rect (FeelProbe.UiScreenRect, EffectUi's way: overlay or camera canvas, any canvas scale), before
                    // the clip. Round 3 (the Editor at 2560 x 1440 read the old one in canvas units, off-centre): the rect is the
                    // turned image's bound, so its half-sizes are un-turned first (ClearBox at 1), then the clear box is turned
                    // by this frame's jitter (none in N18); both sides, each with its sprite's clear box (NIGHT17's: inside its
                    // r 0.5 circle), the drawn layers' smallest.
                    Vector2 mid = r.center;
                    CameraMove.ClearBox(r.width * 0.5, r.height * 0.5, 1, jitter, out double halfW, out double halfH);
                    float clearInner = !FeelAB.On ? UnityTuning.LinesClearN17 : ab > 0 ? UnityTuning.LinesBoostInner : UnityTuning.LinesRunInner;
                    CameraMove.ClearBox(halfW, halfH, clearInner, jitter, out double cw, out double ch);
                    r = Rect.MinMaxRect(Mathf.Max(0, r.xMin), Mathf.Max(0, r.yMin), Mathf.Min(UnityEngine.Screen.width, r.xMax), Mathf.Min(UnityEngine.Screen.height, r.yMax));
                    FeelProbe.Effect(FeelFx.SpeedLines, r, Mathf.Max(a, ab), FeelAB.On ? UnityTuning.LinesStreaksN18 : UnityTuning.LinesStreaks);
                    FeelProbe.Effect(FeelFx.SpeedLinesClear, new Rect(mid.x - (float)cw, mid.y - (float)ch, 2 * (float)cw, 2 * (float)ch), Mathf.Max(a, ab));
                }
            }

            // The freshness tier by the weapon name. NIGHT9 U3-08: the arsenal's FRESH while it runs (hud.js reads the
            // arsenal; 0..1 shown on the meter's tiers, the scale the style meter pays it on), else the meter's own.
            // NIGHT13 U3-26.9: the requested gun shows at once, as the hotbar's cell in hand (player.js hudState: s.weapon =
            // owned[targetSlot]); the line sits in the weapon block's plate (UiHotbar.cs FitWeaponBlock), refitted on change.
            Loadout lo = game.loadout;
            string w = lo.targetSlot >= 0 && lo.targetSlot < lo.owned.Count ? lo.owned[lo.targetSlot] : lo.Weapon;
            Arsenal ars = lo.arsenal;
            double v = ars.enabled && ars.Catalog.Has(ArsenalMath.WeaponOf(w)) && game.style != null
                ? ars.Fresh(w) * game.style.Params.max
                : game.style != null ? game.style.Freshness(w) : -1;
            string tier = v < 0 ? null : game.style.TierName(v); // DULL too with the LAB's FRESHNESS TEETH (NIGHT8 U2-16)
            string name = game.forge != null && game.forge.IsForged(w) ? ForgedName(w) : WeaponName(w); // hud.js: a forged gun's "+" (NIGHT8 U2-17)
            if (!ReferenceEquals(tier, shownTier) || !ReferenceEquals(name, shownWeapon))
            {
                shownTier = tier;
                shownWeapon = name;
                sb.Clear();
                sb.Append(name);
                if (tier != null) sb.Append("  <size=80%><color=#").Append(tier == "FRESH" ? freshHex : tier == "USED" ? usedHex : staleHex).Append('>').Append(tier).Append("</color></size>");
                weapon.SetText(sb);
                weaponBlockDirty = true;
            }
            WeaponCardSet(w, name); // NIGHT17 round 2: the name under the crosshair on a swap (UiHotbar.cs; a no-op unless the gun changed)
            if (weaponBlockDirty) FitWeaponBlock(); // after every row's text this frame (RenderHotbar ran before)
        }
    }
}
