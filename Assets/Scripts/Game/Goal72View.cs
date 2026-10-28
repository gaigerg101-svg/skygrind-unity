// NIGHT72 builder N, slice N3 (view row design.n72.goal72; the pure half and the numbers' meaning: Goal72Math.cs; GG 2026-10-26: "there needs to be more text on the screen that explains like
// OK now you have to go up or whatever, obviously make it gamified and easy for the user to understand"). THE ROOM'S ONE LINE AT THE THING IT NAMES: when a campaign room is cleared and its exit
// is open, one line hangs over the exit (verb in capitals in the player's cyan, at most 6 words, UiTheme S24 = 24 px at 1080p, the input's own glyph) with an arrowhead marker under it; an exit off
// screen keeps the marker on the screen's edge (never on the bottom, where the gun and the HP plate sit) and the line beside it. After 10 s with no progress toward the exit the line says WHERE and
// the marker breathes; after 20 s it says HOW and a pointer rides a ring round the crosshair toward the exit. Progress or arriving retires it; a verb the player already used is not taught again.
// Self-hooked (Feel46View's way: a hidden object made at scene load finds the live SurvivalGame and hooks its OnRender / OnNewRun; no shared file changes), its own screen-space canvas UNDER the
// HUD's (sorting 9 < 10). While the row is on it hides the DOOR OPEN cue (UiCampaignCue24's "Door cue 24" group: its distance and side words are this line's job now); off: the cue as before.
// View only: no sim read that is not the sim's own public state, no tape bit, no audio. Read EVERY frame (NightRows.ViewOn: D1's contract; the state machine keeps running while the row reads
// off, so the next frame on is the frame it would have drawn). No allocation per frame beyond the text's change. Calm: the breath is a slow scale (never a flicker), still with REDUCED MOTION.
// Probe: Goal72Probe (the Player log's `[goal72]` lines and the stuck72 reel read it). Off: the row off, -skygrindNoGoal72 (Rows72), -skygrindOff72 if the shared switch covers the night.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT72 N3: what goal72 shows now and what it counted since the run started (the reel's / the log's line).</summary>
    public static class Goal72Probe
    {
        public static bool On, Visible;
        public static int Stage;
        public static string TextNow = "", RawNow = "";
        public static float PxNow, ScreenX, ScreenY, Alpha;
        public static double StuckS;
        public static int Lines, Hint10, Hint20, Retired, Words;
        public static bool PointerVisible, CueHidden;

        public static string Line() =>
            $"goal72 on {On}, visible {Visible}, stage {Stage}, text '{TextNow}' ({Words} words, {PxNow:0} px), stuck {StuckS:0.0} s, lines {Lines}, hints at 10 s {Hint10}, at 20 s {Hint20}, hints retired {Retired}, pointer {PointerVisible}, door cue hidden {CueHidden}";
    }

    public sealed class Goal72View : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Goal72View>() != null) return;
            var go = new GameObject("Goal72");
            DontDestroyOnLoad(go);
            go.AddComponent<Goal72View>();
        }

        /// <summary>The planted stuck run (REEL ONLY, as -skygrindPinHint63): a scripted player counts for the stuck clock.</summary>
        static bool? switchPlanted;
        public static bool PlantedStuck
        {
            get
            {
                if (!switchPlanted.HasValue)
                    switchPlanted = Array.Exists(Environment.GetCommandLineArgs(), a => string.Equals(a, Goal72Math.StuckSwitch, StringComparison.OrdinalIgnoreCase))
                        && Array.Exists(Environment.GetCommandLineArgs(), a => string.Equals(a, PinHint63.ReelSwitch, StringComparison.OrdinalIgnoreCase));
                return switchPlanted.Value;
            }
        }

        /// <summary>The stuck72 reel's own switch for one pass (null: the command line's -skygrindStuck72).</summary>
        public static bool? PlantedOverride;

        SurvivalGame game;
        readonly Goal72Math.State st = new Goal72Math.State();
        bool plantedStuck;
        Canvas canvas;
        RectTransform canvasRect, lineRt, markerRt, pointerRt;
        CanvasGroup lineGroup;
        Image plate, marker, pointer;
        TextMeshProUGUI lineText;
        string shownRaw = "", shownResolved = "";
        float alpha, realClock, slideAge = float.MaxValue / 2;
        int lastStage;
        int stampDevice = -1;
        CanvasGroup doorCue;
        double hideWithin = 6;
        int lastLoggedStage = -1, lastLoggedRoom = -2, cueFindNext;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            doorCue = null;
            st.NewRun();
            ClearProbe();
            hideWithin = g.cfg != null && g.cfg.Has("design.n24.campaign.directed.hideWithin") ? g.cfg.Num("design.n24.campaign.directed.hideWithin") : 6;
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                st.NewRun();
                ClearProbe();
                lastLoggedStage = -1;
                lastLoggedRoom = -2;
            };
        }

        void ClearProbe()
        {
            Goal72Probe.Visible = false;
            Goal72Probe.Stage = 0;
            Goal72Probe.TextNow = Goal72Probe.RawNow = "";
            Goal72Probe.Lines = Goal72Probe.Hint10 = Goal72Probe.Hint20 = Goal72Probe.Retired = Goal72Probe.Words = 0;
            Goal72Probe.PointerVisible = Goal72Probe.CueHidden = false;
        }

        /// <summary>The verbs the player has used this run, read from the mover's state (a ride, a slide, a double jump spent): the hints do not teach them again.</summary>
        void Learn(SurvivalGame g)
        {
            Mover m = g.mover;
            if (m == null) return;
            if (m.state == MoveState.WallRide) st.Learn(Goal72Math.Verb.Ride);
            else if (m.state == MoveState.Slide) st.Learn(Goal72Math.Verb.Slide);
            if (!m.grounded && m.doubleJumpsLeft < m.Config.jump.doubleJumps) st.Learn(Goal72Math.Verb.DoubleJump);
        }

        void Render(double frameDt, double frameAlpha)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null) return;
            realClock += (float)frameDt;
            bool on = NightRows.ViewOn(g.cfg, Goal72Math.Row); // read every frame (D1)
            Goal72Probe.On = on;
            SurvivalSim sim = g.sim;
            Campaign24 c = sim.campaignOn ? sim.campaign : null;
            Learn(g);
            bool playing = c != null && g.flow == Flow.Playing && !g.attractOn && !sim.metaSilent;
            bool cleared = playing && c.phase == Campaign24.Phase.Cleared && !c.finished && c.Room != null;
            Vec3 exit = cleared ? c.Room.exit : default;
            Vec3 feet = g.Feet;
            double dx = exit.x - feet.x, dz = exit.z - feet.z;
            double dist2 = Math.Sqrt(dx * dx + dz * dz), dist3 = Math.Sqrt(dx * dx + dz * dz + (exit.y - feet.y) * (exit.y - feet.y));
            bool active = cleared && dist2 > hideWithin && c.steps * sim.step >= UnityTuning.Goal72FirstLevelS;
            bool scripted = g.forcedMove.HasValue;
            plantedStuck = PlantedOverride ?? PlantedStuck;
            double bearing = cleared ? Directed22.BearingDeg(feet, g.input.yaw, exit) : 0;
            st.Step(sim.simStep * sim.step, active, c != null ? c.room : -1, dist3, scripted, plantedStuck, exit.y - feet.y, bearing);
            int stage = on ? st.Stage : 0;
            Goal72Probe.Stage = stage;
            Goal72Probe.StuckS = st.StuckS;
            if (on)
            {
                Goal72Probe.Lines = st.Lines;
                Goal72Probe.Hint10 = st.Hint10;
                Goal72Probe.Hint20 = st.Hint20;
                Goal72Probe.Retired = st.Retired;
            }
            if (stage == 0 && alpha <= 0.001f && canvas == null) { HideCue(false); return; } // nothing was ever drawn: nothing to build
            if (!UiTheme.Ready) return;
            EnsureCanvas();
            float target = stage > 0 ? 1f : 0f;
            alpha = Mathf.MoveTowards(alpha, target, (float)frameDt / (target > alpha ? UnityTuning.Goal72FadeInS : UnityTuning.Goal72FadeOutS));
            if (stage > 0) lastStage = stage;
            if (alpha <= 0.001f)
            {
                if (lineGroup.gameObject.activeSelf) lineGroup.gameObject.SetActive(false);
                if (marker.enabled) { marker.enabled = false; pointer.enabled = false; }
                Goal72Probe.Visible = false;
                Goal72Probe.PointerVisible = false;
                HideCue(false);
                return;
            }
            if (!lineGroup.gameObject.activeSelf) { lineGroup.gameObject.SetActive(true); slideAge = 0; }
            HideCue(true);

            // the text: resolved per device, rewritten only when it changes
            string raw = st.LastRaw;
            int dev = (int)PlayerDevices.Last * 31 + (int)PlayerDevices.Glyphs;
            if (raw != shownRaw || dev != stampDevice)
            {
                shownRaw = raw;
                stampDevice = dev;
                string resolved = g.Goal72Resolve(raw, st.LastRawPad);
                if (resolved != shownResolved)
                {
                    shownResolved = resolved;
                    slideAge = 0; // a new line slides in (UI motion: GG 2026-10-14 "UI needs to be animated")
                    lineText.text = Goal72Math.Rich(resolved, ColorUtility.ToHtmlStringRGB(UiTheme.Cyan));
                    float w = Mathf.Clamp(lineText.GetPreferredValues(lineText.text).x + 56f, 240f, UnityTuning.Goal72LineWidthPx + 120f);
                    lineRt.sizeDelta = new Vector2(w, UnityTuning.Goal72LineHeightPx);
                    Goal72Probe.TextNow = resolved.Replace("[", "").Replace("]", "");
                    Goal72Probe.RawNow = raw;
                    Goal72Probe.Words = Goal72Math.Words(raw);
                    Goal72Probe.PxNow = lineText.fontSize;
                }
            }

            // where: the exit's label point on the screen, or the screen's edge toward it
            Camera cam = g.cam;
            Vector3 wp = new Vector3((float)exit.x, (float)exit.y + (float)UnityTuning.Goal72LabelRiseM, (float)exit.z);
            Vector3 sp = cam.WorldToScreenPoint(wp);
            float scale = Mathf.Max(0.01f, canvas.scaleFactor);
            float cx = (sp.x - Screen.width * 0.5f) / scale, cy = (sp.y - Screen.height * 0.5f) / scale;
            if (sp.z < 0) { cx = -cx; cy = -cy; } // behind the camera: the point mirrored, so the edge toward it is the edge behind the player
            float w2 = canvasRect.rect.width, h2 = canvasRect.rect.height;
            float hx = w2 * 0.5f - UnityTuning.Goal72EdgeInsetPx, hy = h2 * 0.5f - UnityTuning.Goal72EdgeInsetPx;
            bool inView = sp.z > 0 && Mathf.Abs(cx) <= hx && Mathf.Abs(cy) <= hy;
            float ang = Mathf.Atan2(cy, cx);
            float mx = cx, my = cy, rot = 180f; // the arrowhead points up: turned 180 it points down at the exit
            Vector2 linePos;
            if (inView)
            {
                my = Mathf.Max(my, -hy + UnityTuning.Goal72LowestPx);
                linePos = new Vector2(mx, my + UnityTuning.Goal72MarkerPx * 0.5f + UnityTuning.Goal72LineHeightPx * 0.5f + 6f);
            }
            else
            {
                float c0 = Mathf.Cos(ang), s0 = Mathf.Sin(ang);
                float t = Mathf.Min(Mathf.Abs(c0) > 1e-5f ? hx / Mathf.Abs(c0) : float.MaxValue, Mathf.Abs(s0) > 1e-5f ? hy / Mathf.Abs(s0) : float.MaxValue);
                mx = c0 * t;
                my = s0 * t;
                if (s0 < 0f) // never on the bottom edge: the side edge, at most Goal72LowestPx above the bottom inset
                {
                    float side = Mathf.Abs(c0) > 1e-4f ? Mathf.Sign(c0) : 1f;
                    mx = side * hx;
                    my = Mathf.Max(-hy + UnityTuning.Goal72LowestPx, Mathf.Abs(c0) > 1e-4f ? s0 * hx / Mathf.Abs(c0) : -hy);
                }
                rot = ang * Mathf.Rad2Deg - 90f; // outward
                float halfW = lineRt.sizeDelta.x * 0.5f;
                float lx = Mathf.Clamp(mx - Mathf.Sign(mx == 0 ? 1 : mx) * (halfW + UnityTuning.Goal72MarkerPx * 0.6f), -(w2 * 0.5f - halfW - 24f), w2 * 0.5f - halfW - 24f);
                linePos = new Vector2(lx, my);
            }
            float breath = stage >= 2 && !g.cfg.movement.settings.reducedMotion ? 1f + UnityTuning.Goal72PulseDepth * 0.5f * (1f + Mathf.Sin(realClock * UnityTuning.Goal72PulseHz * 2f * Mathf.PI)) : 1f;
            double dist = dist3;
            float dAlpha = dist > UnityTuning.Goal72FarM ? UnityTuning.Goal72FarAlpha : 1f;
            lineGroup.alpha = alpha * dAlpha;
            // the line slides in: UnityTuning.Goal72SlideInPx above its place, eased out over Goal72SlideS (none with REDUCED MOTION); a changed line slides in again
            slideAge += (float)frameDt;
            float slideT = Mathf.Clamp01(slideAge / UnityTuning.Goal72SlideS);
            float slide = g.cfg.movement.settings.reducedMotion ? 0f : (1f - slideT) * (1f - slideT) * (1f - slideT) * UnityTuning.Goal72SlideInPx;
            linePos.y += slide;
            lineRt.anchoredPosition = linePos;
            lineRt.localScale = new Vector3(breath, breath, 1f);
            marker.enabled = true;
            markerRt.anchoredPosition = new Vector2(mx, my);
            markerRt.localEulerAngles = new Vector3(0, 0, rot);
            markerRt.localScale = new Vector3(breath, breath, 1f);
            marker.color = new Color(1, 1, 1, alpha * dAlpha);
            // stage 3: a pointer on a ring round the crosshair, toward the exit
            bool ptr = stage >= 3;
            pointer.enabled = ptr;
            if (ptr)
            {
                pointerRt.anchoredPosition = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * UnityTuning.Goal72PointerRadiusPx;
                pointerRt.localEulerAngles = new Vector3(0, 0, ang * Mathf.Rad2Deg - 90f);
                pointer.color = new Color(1, 1, 1, alpha * (0.75f + 0.25f * (breath - 1f) / Mathf.Max(0.01f, UnityTuning.Goal72PulseDepth)));
            }
            Goal72Probe.Visible = true;
            Goal72Probe.PointerVisible = ptr;
            Goal72Probe.ScreenX = mx;
            Goal72Probe.ScreenY = my;
            Goal72Probe.Alpha = lineGroup.alpha;
            if (stage != lastLoggedStage || st.Room != lastLoggedRoom)
            {
                lastLoggedStage = stage;
                lastLoggedRoom = st.Room;
                Debug.Log($"[goal72] stage {stage} room {c?.Room?.id} line '{Goal72Probe.TextNow}' ({Goal72Probe.Words} words, {Goal72Probe.PxNow:0} px), exit {dist:0} m away, up {exit.y - feet.y:0.0} m, stuck {st.StuckS:0.0} s | lines {st.Lines}, hints 10 s {st.Hint10}, 20 s {st.Hint20}, retired {st.Retired}");
            }
        }

        /// <summary>The DOOR OPEN cue's group (UiCampaignCue24 "Door cue 24") held at alpha 0 while goal72 draws its line (its words are the line's now).</summary>
        void HideCue(bool hide)
        {
            Goal72Probe.CueHidden = hide;
            if (doorCue == null)
            {
                if (!hide || Time.frameCount < cueFindNext) return;
                cueFindNext = Time.frameCount + 30; // a look-up at most every 30 frames until the cue's group exists (a scan of every CanvasGroup)
                foreach (CanvasGroup cg in Resources.FindObjectsOfTypeAll<CanvasGroup>())
                    if (cg != null && cg.gameObject.name == "Door cue 24" && cg.gameObject.scene.IsValid()) { doorCue = cg; break; }
                if (doorCue == null) return;
            }
            if (hide && doorCue.alpha != 0f) doorCue.alpha = 0f; // the cue writes its own alpha each frame; this runs after it (OnRender)
        }

        // --- building (once) ---------------------------------------------------------------------------------------------------------------------

        void EnsureCanvas()
        {
            if (canvas != null) return;
            var go = new GameObject("Goal72 canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Goal72SortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f; // px at 1080p
            canvasRect = (RectTransform)go.transform;
            Color ink = new Color32(0x0B, 0x10, 0x18, 255);
            Sprite arrow = Sprite.Create(ArrowTexture(), new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));

            var lg = new GameObject("Goal72 line", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            lg.transform.SetParent(go.transform, false);
            lineRt = (RectTransform)lg.transform;
            lineRt.anchorMin = lineRt.anchorMax = lineRt.pivot = new Vector2(0.5f, 0.5f);
            lineRt.sizeDelta = new Vector2(UnityTuning.Goal72LineWidthPx, UnityTuning.Goal72LineHeightPx);
            lineGroup = lg.GetComponent<CanvasGroup>();
            lineGroup.interactable = lineGroup.blocksRaycasts = false;
            plate = lg.GetComponent<Image>();
            plate.raycastTarget = false;
            plate.color = new Color(ink.r, ink.g, ink.b, 0.62f);
            var tg = new GameObject("Goal72 text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tg.transform.SetParent(lg.transform, false);
            lineText = tg.GetComponent<TextMeshProUGUI>();
            lineText.raycastTarget = false;
            lineText.richText = true;
            lineText.alignment = TextAlignmentOptions.Center;
            lineText.textWrappingMode = TextWrappingModes.NoWrap;
            lineText.overflowMode = TextOverflowModes.Overflow;
            UiTheme.Style(lineText, UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, true);
            var trt = lineText.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            lg.SetActive(false);

            marker = MakeArrow(go.transform, "Goal72 marker", arrow, UnityTuning.Goal72MarkerPx, out markerRt);
            pointer = MakeArrow(go.transform, "Goal72 pointer", arrow, UnityTuning.Goal72PointerPx, out pointerRt);
            marker.enabled = pointer.enabled = false;
        }

        static Image MakeArrow(Transform parent, string name, Sprite s, float px, out RectTransform rt)
        {
            var g = new GameObject(name, typeof(RectTransform), typeof(Image));
            g.transform.SetParent(parent, false);
            Image im = g.GetComponent<Image>();
            im.sprite = s;
            im.raycastTarget = false;
            im.preserveAspect = true;
            rt = im.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(px, px);
            return im;
        }

        /// <summary>An arrowhead pointing up: the player's cyan with a dark ink rim (it reads on level 2's white ice and on the dark rooms).</summary>
        static Texture2D ArrowTexture()
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Goal72 arrow" };
            var px = new Color32[n * n];
            Color cy = UiTheme.Cyan;
            Color ink = new Color32(0x0B, 0x10, 0x18, 255);
            // a notched arrowhead: the triangle A-B-C (wound counter-clockwise: apex, bottom left, bottom right) minus the notch B-C-D under its middle
            Vector2 A = new Vector2(0.5f, 0.94f), B = new Vector2(0.08f, 0.1f), C = new Vector2(0.92f, 0.1f), D = new Vector2(0.5f, 0.34f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    var p = new Vector2((x + 0.5f) / n, (y + 0.5f) / n);
                    float dOuter = Mathf.Min(Edge(A, B, p), Mathf.Min(Edge(B, C, p), Edge(C, A, p)));
                    float dNotch = Mathf.Min(Edge(B, C, p), Mathf.Min(Edge(C, D, p), Edge(D, B, p)));
                    float d = Mathf.Min(dOuter, -dNotch); // inside the arrowhead: d >= 0
                    float fill = Mathf.Clamp01(d / 0.025f);
                    float rim = Mathf.Clamp01(1f - (-d) / 0.07f) * (d < 0.025f ? 1f : 0f);
                    Color col = Color.Lerp(ink, cy, fill);
                    float a = Mathf.Max(fill, rim * 0.95f);
                    px[y * n + x] = new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)(Mathf.Clamp01(a) * 255));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        /// <summary>The signed distance of p from the edge a-b of a counter-clockwise polygon (positive on the inner, left side).</summary>
        static float Edge(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 e = b - a;
            return (e.x * (p.y - a.y) - e.y * (p.x - a.x)) / e.magnitude;
        }
    }
}
