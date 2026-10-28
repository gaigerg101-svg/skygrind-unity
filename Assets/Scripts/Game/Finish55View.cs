// NIGHT72 builder B (slate B5, design.n55.finish55, a VIEW row: NightRows.ViewOn(cfg, "finish55"); NIGHT55 K's row, never written there: its config numbers are empty). THE BOSS KILL'S BEAT.
// What the code showed: a boss's death is a KillEvent of kind "boss" (BossKit.Down) after a bossDying (the finisher's slow-mo, SurvivalGame.BossDyingSeen); Feel46View's kill ladder
// (kicks71) reads it as a plain BODY kill (e.melee false, not a headshot): 2.2 deg and a 1.2 deg FOV punch-in, the smallest tier, on the biggest kill in the game. Nothing else on the screen
// marks it but the wyrm's white particle burst. With the row ON (read EVERY frame; the beat's state runs with it off, D1) a boss kill gets:
//   - a camera kick of Finish55KickDeg (above the ladder's biggest, the punch kill's 3.6 deg) with a 0.55 s settle, and a Finish55FovDeg FOV punch-in: both frame time, x EFFECTS INTENSITY
//     and the CAMERA SHAKE / FOV KICK settings, none under REDUCED MOTION;
//   - the comic panel: two bars (a letterbox, the HUD's shadow ink, a bone hairline on the inner edge) slide in over Finish55BarInS from bossDying (or the kill, whichever comes first),
//     hold through the finisher and Finish55HoldAfterDownS after bossDown, then slide out over Finish55BarOutS. One slide in, one out: no loop, no flicker, no noise. They sit UNDER the HUD
//     canvas (UnityTuning.Finish55CanvasOrder 9 < 10).
//   Not here: a JoJo "colour turn". GG's 2026-10-20 words (docs/memory/OWNER.md:115) ask that each boss have its own palette and lighting identity "similar to how JoJo's Bizarre Adventure uses
//   dramatic color changes": that is each boss room's grade (BossGrade54 / BossGrade55, L's), not a kill beat; a full-screen colour flash at the kill would break "no new full-screen flicker".
// RENDER ONLY: no sim read for writing, no timer, no collider; the view hooks the bus (bossDying, kill, bossDown) once the game exists. Installs itself after the first scene load.
// Proof (L-014 / L-115; Player.log): "[finish55] boss kill (<boss>, <event>) : kick <deg> deg, fov <deg> deg, bars in | row finish55 on|off | t <s> s (FeelClock)" at the beat's start;
// "[finish55] bars out after <s> s | row ..." at its end; the reel (bosskill72) prints both rows' states.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed class Finish55View : MonoBehaviour
    {
        public const string Row = "finish55";

        /// <summary>For the reel: the row's state on the last frame, beats begun since boot, the peak kick (deg) and FOV punch (deg) drawn on a frame, the bars' share now (0 .. 1 of full).</summary>
        public static bool RowOn;
        public static int Beats;
        public static double KickPeak, FovPeak;
        public static float BarK;

        SurvivalGame game;
        readonly KickStack kicks = new KickStack(UnityTuning.Kick46Slots), fovs = new KickStack(UnityTuning.Kick46Slots);
        Canvas canvas;
        RectTransform top, bottom;
        Image topBar, bottomBar, topHair, bottomHair;
        double clock, beatAt = double.NegativeInfinity, downAt = double.NegativeInfinity, punchAt;
        bool beat, out_, punched;
        float k; // the bars' slide, 0 .. 1

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Finish55View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Finish55View>();
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            Clear();
            g.OnRender += Render;
            g.OnNewRun += Clear;
            g.events.On<BossEvent>("bossDying", e => Begin(e != null ? e.boss : "?", "bossDying"));
            g.events.On<KillEvent>("kill", e => { if (e != null && e.owner == "player" && e.kind == "boss") Begin("boss", "kill"); });
            g.events.On<BossEvent>("bossDown", e => { if (beat) downAt = clock; });
        }

        void Clear()
        {
            kicks.Clear();
            fovs.Clear();
            beat = out_ = false;
            beatAt = downAt = double.NegativeInfinity;
            k = 0f;
            BarK = 0f;
        }

        /// <summary>A boss's death begins (bossDying, or the kill when nothing came before it): one beat, once.</summary>
        void Begin(string boss, string why)
        {
            if (beat && !out_) return; // the bossDying and the kill of one death are one beat
            beat = true;
            out_ = false;
            beatAt = clock;
            downAt = double.NegativeInfinity;
            Beats++;
            // NIGHT74 B kill74 (Kill74.cs): with the row on and THE KILN WYRM dying, the punch-in (kick, FOV punch, letterbox) waits the flash and the hold (UnityTuning.N74KillHoldS, frame time from this
            // event), so it lands after them and not on them; 0 with the row off or any other boss: the kick and the bars at once, as NIGHT72 drew them
            double delay = Kill74Math.PunchDelayS(game != null && Kill74.On(game.cfg), Kill74.Supports(boss), UnityTuning.N74KillHoldS);
            punchAt = clock + delay;
            punched = false;
            if (delay <= 0) Punch();
            bool on = game != null && NightRows.ViewOn(game.cfg, Row);
            Debug.Log("[finish55] boss kill (" + (boss ?? "?") + ", " + why + "): kick " + UnityTuning.Finish55KickDeg.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " deg, fov "
                + UnityTuning.Finish55FovDeg.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " deg, bars in" + (delay > 0 ? " after " + delay.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s (kill74's flash and hold)" : "")
                + " | row finish55 " + (on ? "on" : "off (the beat's state runs, nothing drawn)")
                + " | t " + FeelClock.Now.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s (FeelClock)");
        }

        /// <summary>The punch-in's kick and FOV punch go on the stacks (at the beat's start, or after kill74's hold).</summary>
        void Punch()
        {
            if (punched) return;
            punched = true;
            kicks.Add(UnityTuning.Finish55KickDeg, UnityTuning.Finish55KickAttackS, UnityTuning.Finish55KickRecoverS);
            fovs.Add(UnityTuning.Finish55FovDeg, UnityTuning.Finish55FovAttackS, UnityTuning.Finish55FovRecoverS);
        }

        void Render(double frameDt, double alpha)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null) return;
            clock += frameDt;
            if (beat && !punched && clock >= punchAt) Punch(); // NIGHT74 B kill74: the punch-in after the flash and the hold (at once without the row)
            kicks.Advance(frameDt);
            fovs.Advance(frameDt);
            bool on = NightRows.ViewOn(g.cfg, Row);
            RowOn = on;
            // the beat's bars: in while it runs, out after bossDown + the hold (or the safety)
            if (beat)
            {
                bool release = out_ || (downAt > double.NegativeInfinity && clock - downAt >= UnityTuning.Finish55HoldAfterDownS) || clock - beatAt >= UnityTuning.Finish55MaxS;
                if (release && !out_) { out_ = true; }
                k = out_ ? Mathf.Max(0f, k - (float)frameDt / UnityTuning.Finish55BarOutS) : punched ? Mathf.Min(1f, k + (float)frameDt / UnityTuning.Finish55BarInS) : k; // the bars wait for the punch-in (kill74)
                if (out_ && k <= 0f)
                {
                    beat = false;
                    out_ = false;
                    Debug.Log("[finish55] bars out after " + (clock - beatAt).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s | row finish55 " + (on ? "on" : "off")
                        + " | t " + FeelClock.Now.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s (FeelClock)");
                }
            }
            BarK = on ? k : 0f;
            DrawBars(on && k > 0f ? k : 0f);
            if (!on) return;
            MovementConfig mc = g.cfg.movement;
            if (mc.settings.reducedMotion) return;
            double intensity = Mathf.Max(0f, Mathf.Min((float)mc.feel.effectsIntensityMax, (float)mc.settings.effectsIntensity));
            double kick = kicks.Value() * intensity * CameraFeel.ShakeScale(g.settings);
            if (kick != 0) g.cam.transform.rotation = g.cam.transform.rotation * Quaternion.Euler((float)-kick, 0f, 0f);
            double fov = fovs.Value() * intensity * CameraFeel.FovKickScale(g.settings);
            if (fov != 0) g.cam.fieldOfView = Mathf.Max(1f, g.cam.fieldOfView - (float)fov);
            if (System.Math.Abs(kick) > KickPeak) KickPeak = System.Math.Abs(kick);
            if (System.Math.Abs(fov) > FovPeak) FovPeak = System.Math.Abs(fov);
        }

        void DrawBars(float share)
        {
            if (share <= 0f)
            {
                if (canvas != null && canvas.gameObject.activeSelf) canvas.gameObject.SetActive(false);
                return;
            }
            if (canvas == null) Build();
            if (!canvas.gameObject.activeSelf) canvas.gameObject.SetActive(true);
            float h = UnityTuning.Finish55BarShare * 1080f * Mathf.SmoothStep(0f, 1f, share); // px at 1080p (the scaler matches the height)
            top.sizeDelta = new Vector2(0f, h);
            bottom.sizeDelta = new Vector2(0f, h);
        }

        void Build()
        {
            var go = new GameObject("Finish55 canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Finish55CanvasOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            Color ink = UiStyle33.Shadow;
            ink.a = UnityTuning.Finish55BarAlpha;
            top = Bar(go.transform, "top", true, ink, out topBar, out topHair);
            bottom = Bar(go.transform, "bottom", false, ink, out bottomBar, out bottomHair);
        }

        static RectTransform Bar(Transform parent, string name, bool atTop, Color ink, out Image bar, out Image hair)
        {
            var g = new GameObject("Finish55 " + name, typeof(RectTransform), typeof(Image));
            g.transform.SetParent(parent, false);
            var rt = (RectTransform)g.transform;
            rt.anchorMin = new Vector2(0f, atTop ? 1f : 0f);
            rt.anchorMax = new Vector2(1f, atTop ? 1f : 0f);
            rt.pivot = new Vector2(0.5f, atTop ? 1f : 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 0f);
            bar = g.GetComponent<Image>();
            bar.color = ink;
            bar.raycastTarget = false;
            var hg = new GameObject("hair", typeof(RectTransform), typeof(Image));
            hg.transform.SetParent(rt, false);
            var hr = (RectTransform)hg.transform;
            hr.anchorMin = new Vector2(0f, atTop ? 0f : 1f);
            hr.anchorMax = new Vector2(1f, atTop ? 0f : 1f);
            hr.pivot = new Vector2(0.5f, atTop ? 0f : 1f);
            hr.anchoredPosition = Vector2.zero;
            hr.sizeDelta = new Vector2(0f, UnityTuning.Finish55HairPx);
            hair = hg.GetComponent<Image>();
            hair.color = UiStyle33.Bone;
            hair.raycastTarget = false;
            return rt;
        }

        void OnDestroy()
        {
            if (game != null) game.OnRender -= Render;
        }
    }
}
