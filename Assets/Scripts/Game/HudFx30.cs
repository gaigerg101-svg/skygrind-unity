// NIGHT30 builder V (item 8, design.n30.look30; DESIGN30 E; D-4): THE HUD THAT BUZZES LIKE AN OLD TV, on the HUD only (GG: "UI elements that buzz, glow
// briefly, or flicker like an old TV ... without affecting the entire screen"). Three panels: the vitals (HP), the weapon card, the points.
//  - each panel: a SCANLINE overlay (a 2 px period texture, the dark rows at 8 %) and a faint cyan EDGE GLOW, both inside the panel (inset 3 px: the
//    theme's cut corners stay clean), drawn over the plate and under its text;
//  - FLICKER (2-4 frames: the scanlines go bright / clear / bright) when the panel's value changes: HP, the gun in hand, the points;
//  - GLOW (0.2 s: the edge to 55 % and back) on a state change: a hit taken (vitals), a READY turn (the weapon card), a kill (the points);
//  - BUZZ (a 1-2 px horizontal jitter at 12 Hz, never a brightness change) of the vitals under 25 % health.
// Safety (R2): one panel flickers at a time and a flicker starts at most every 0.34 s (FlickerGate30: 3 a second); nothing flashes over 5 s (a flicker
// is 3 frames, a glow 0.2 s); the glow and the flicker scale with FLASH INTENSITY (0: none); REDUCED MOTION stills the buzz; HUD FLICKER (ACCESS, the
// accessibility switch) turns all of it off. Off (-skygrindOff30 or the row off): nothing built.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class FxPanel30
        {
            public RectTransform plate, twin; // twin: the panel's values-layer copy (jittered with it by the buzz), may be null
            public UnityEngine.UI.Image scan, edge;
            public double glowAge = double.MaxValue;
            public float scanShown = -1, edgeShown = -1, jitterShown;
        }

        bool fx30On;
        FxPanel30[] fx30;
        readonly FlickerGate30 fx30Gate = new FlickerGate30();
        static Sprite fx30ScanSprite;
        double fx30LastHp = double.NaN;
        long fx30LastPoints = long.MinValue;
        string fx30LastWeapon;
        int fx30LastTurns = -1;

        /// <summary>For the reel `look30`: the HUD fx are live (row on, HUD FLICKER on), flickers started, glows started, the vitals buzzing now.</summary>
        public static bool Fx30Live;
        public static int Fx30Flickers, Fx30Glows;
        public static bool Fx30Buzzing;

        const int Fx30Vitals = 0, Fx30Card = 1, Fx30Points = 2;

        void BuildFx30()
        {
            fx30On = N30Fixes.ViewRowOn(cfg, "look30");
            if (!fx30On) return;
            fx30ScanSprite ??= ScanSprite30();
            Transform hp = frame.Find("HP panel");
            fx30 = new FxPanel30[3];
            fx30[Fx30Vitals] = FxPanel30Of(hp as RectTransform, hpText != null ? hpText.rectTransform.parent as RectTransform : null);
            fx30[Fx30Card] = FxPanel30Of(weaponPlate != null ? weaponPlate.rectTransform : null, null);
            fx30[Fx30Points] = FxPanel30Of(pointsPlate17 != null ? pointsPlate17.rectTransform : null, null);
            game.events.On<HurtEvent>("hurt", e => { if (e.owner == "player") Glow30(Fx30Vitals); });
            game.events.On<KillEvent>("kill", e => { if (e.owner == "player") Glow30(Fx30Points); });
            game.OnNewRun += () => { fx30LastHp = double.NaN; fx30LastPoints = long.MinValue; fx30LastWeapon = null; fx30LastTurns = -1; };
        }

        FxPanel30 FxPanel30Of(RectTransform plate, RectTransform twin)
        {
            if (plate == null) return null;
            var p = new FxPanel30 { plate = plate, twin = twin };
            float inset = UnityTuning.Look30PanelInset;
            p.scan = Img(plate, "Scanlines 30", new Color(0, 0, 0, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            p.scan.sprite = fx30ScanSprite;
            p.scan.type = UnityEngine.UI.Image.Type.Tiled;
            p.edge = Img(plate, "Edge glow 30", UiTheme.A(UiTheme.Cyan, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            p.edge.sprite = InsetShadowSprite(UnityTuning.ChargeEdgeBlur, UnityTuning.ChargeEdgeSpread, 1);
            p.edge.type = UnityEngine.UI.Image.Type.Sliced;
            foreach (UnityEngine.UI.Image im in new[] { p.scan, p.edge })
            {
                im.raycastTarget = false;
                RectTransform rt = im.rectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(inset, inset);
                rt.offsetMax = new Vector2(-inset, -inset);
                im.enabled = false;
            }
            return p;
        }

        /// <summary>The scanline tile: Look30ScanPeriod rows, the first dark, one canvas px each (point filter, repeat).</summary>
        static Sprite ScanSprite30()
        {
            int n = Mathf.Max(2, UnityTuning.Look30ScanPeriod);
            var tex = new Texture2D(1, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, name = "Scanlines 30" };
            var px = new Color32[n];
            for (int i = 0; i < n; i++) px[i] = i == 0 ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 1, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }

        void Glow30(int id)
        {
            if (fx30 == null || id < 0 || id >= fx30.Length || fx30[id] == null || !Fx30Live) return;
            fx30[id].glowAge = 0;
            Fx30Glows++;
        }

        bool Fx30SettingOn()
        {
            GameSettings st = game.settings;
            if (st == null) return UnityTuning.HudFx30Default;
            return st.store.Item(UnityTuning.HudFx30Setting) != null ? st.Bool(UnityTuning.HudFx30Setting) : UnityTuning.HudFx30Default;
        }

        void RenderFx30(double frameDt)
        {
            if (!fx30On || fx30 == null) return;
            bool live = values.gameObject.activeSelf && Fx30SettingOn();
            Fx30Live = live;
            SettingsView sv = game.settings.View;
            float flashK = Mathf.Clamp01((float)sv.flashIntensity);
            double now = SurvivalGame.RealNow;
            if (live)
            {
                // value changes -> a flicker (the gate: one panel at a time, 3 a second at most)
                double hp = game.health != null ? System.Math.Ceiling(game.health.hp) : 0;
                if (!double.IsNaN(fx30LastHp) && hp != fx30LastHp && flashK > 0 && fx30Gate.TryStart(Fx30Vitals, now)) Fx30Flickers++;
                fx30LastHp = hp;
                string w = game.loadout != null ? game.loadout.Weapon : null;
                if (fx30LastWeapon != null && w != fx30LastWeapon && flashK > 0 && fx30Gate.TryStart(Fx30Card, now)) Fx30Flickers++;
                fx30LastWeapon = w;
                long pts = game.economy != null ? (long)System.Math.Round(game.economy.total) : 0;
                if (fx30LastPoints != long.MinValue && pts != fx30LastPoints && flashK > 0 && fx30Gate.TryStart(Fx30Points, now)) Fx30Flickers++;
                fx30LastPoints = pts;
                // a READY turn (the weapon-state widget) -> the card glows
                if (fx30LastTurns >= 0 && Weapon30ReadyTurns != fx30LastTurns) Glow30(Fx30Card);
                fx30LastTurns = Weapon30ReadyTurns;
            }
            // the buzz: the vitals under 25 % health (REDUCED MOTION: still)
            bool buzz = live && !sv.reducedMotion && game.health != null && game.health.hp > 0 && game.health.hp / System.Math.Max(1, game.health.max) < UnityTuning.Look30BuzzBelow;
            Fx30Buzzing = buzz;
            for (int i = 0; i < fx30.Length; i++)
            {
                FxPanel30 p = fx30[i];
                if (p == null) continue;
                if (p.scan.enabled != live) { p.scan.enabled = live; p.edge.enabled = live; p.scanShown = p.edgeShown = -1; }
                float jit = i == Fx30Vitals && buzz ? Look30Math.BuzzOffset(now) : 0f;
                if (jit != p.jitterShown)
                {
                    float d = jit - p.jitterShown;
                    p.plate.anchoredPosition += new Vector2(d, 0);
                    if (p.twin != null) p.twin.anchoredPosition += new Vector2(d, 0);
                    p.jitterShown = jit;
                }
                if (!live) continue;
                int f = fx30Gate.Frame(i);
                float scanA = f >= 0 ? Look30Math.FlickerAlpha(f, UnityTuning.Look30FlickerFrames, UnityTuning.Look30ScanAlpha) * flashK + (1 - flashK) * UnityTuning.Look30ScanAlpha : UnityTuning.Look30ScanAlpha;
                if (scanA != p.scanShown) { p.scanShown = scanA; p.scan.color = new Color(0, 0, 0, scanA); }
                float edgeA = UnityTuning.Look30EdgeAlpha + (Look30Math.GlowAlpha(p.glowAge, UnityTuning.Look30EdgeAlpha) - UnityTuning.Look30EdgeAlpha) * flashK;
                if (p.glowAge < UnityTuning.Look30GlowSeconds + 1) p.glowAge += frameDt;
                edgeA = Mathf.Round(edgeA * 100f) / 100f;
                if (edgeA != p.edgeShown) { p.edgeShown = edgeA; p.edge.color = UiTheme.A(UiTheme.Cyan, edgeA); }
            }
        }
    }
}
