// NIGHT32 builder G (design.n32.grapplePips, a view row: no sim bit): THE GRAPPLE'S CHARGES beside the reticle. Two cyan diamonds (rule 8: cyan is
// player-usable), one per charge (design.n32.grapple32.charges): full = solid cyan; spent = a dim backing whose cyan fill rises from the bottom as the
// charge comes back (SurvivalSim.grappleRechargeLeft32 of grappleRechargeSteps32; one at a time, so only the next one fills); a charge that comes
// back flashes once. Shown while a run is playing with design.n32.grapple32 on and the ARCLINE owned. No text (nothing under 16 px).
// View only; installs itself (no edit in the lead's SurvivalGame files); Unity objects are made on first use (never in field initialisers, L-035).
// Off: the row off, or -skygrindOff32 (N32Fixes.ViewRowOn).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed class GrapplePips32 : MonoBehaviour
    {
        /// <summary>For the reels: the pips drawn this frame, how many are full, and the next one's fill (0..1).</summary>
        public static bool Shown;
        public static int FullShown;
        public static float NextFill;

        SurvivalGame game;
        Canvas canvas;
        RectTransform root;
        RectTransform[] fills;
        Image[] backs, fillImgs;
        float[] flash;
        int lastFull = -1;
        bool rowOn, rowRead;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("GrapplePips32") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<GrapplePips32>();
        }

        static Color Cyan => UiTheme.Ready ? UiTheme.Cyan : Color.cyan;

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.cfg == null) { game = null; return false; }
            if (canvas != null) Destroy(canvas.gameObject);
            var go = new GameObject("Grapple pips 32", typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Grapple32PipSort;
            var cs = go.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(UnityTuning.Grapple32PipRefW, UnityTuning.Grapple32PipRefH);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;
            root = new GameObject("pips", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(go.transform, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(UnityTuning.Grapple32PipX, UnityTuning.Grapple32PipY);
            root.sizeDelta = Vector2.zero;
            rowRead = false;
            lastFull = -1;
            fills = null;
            return true;
        }

        void Build(int n)
        {
            for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
            fills = new RectTransform[n];
            backs = new Image[n];
            fillImgs = new Image[n];
            flash = new float[n];
            float s = UnityTuning.Grapple32PipSize;
            for (int i = 0; i < n; i++)
            {
                // a diamond: a square turned 45 degrees, with a mask so the fill rises inside the diamond's shape
                var pip = new GameObject("pip " + i, typeof(RectTransform), typeof(Image), typeof(Mask)).GetComponent<RectTransform>();
                pip.SetParent(root, false);
                pip.anchorMin = pip.anchorMax = new Vector2(0.5f, 0.5f);
                pip.sizeDelta = new Vector2(s, s);
                pip.anchoredPosition = new Vector2(i * UnityTuning.Grapple32PipGap, 0);
                pip.localRotation = Quaternion.Euler(0, 0, 45);
                backs[i] = pip.GetComponent<Image>();
                backs[i].raycastTarget = false;
                pip.GetComponent<Mask>().showMaskGraphic = true;
                // the fill: an upright rect under the turned diamond (counter-rotated), its height the charge's share, from the diamond's bottom tip
                var f = new GameObject("fill", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                f.SetParent(pip, false);
                f.localRotation = Quaternion.Euler(0, 0, -45);
                f.anchorMin = f.anchorMax = new Vector2(0.5f, 0.5f);
                f.pivot = new Vector2(0.5f, 0f);
                float diag = s * 1.4142135f;
                f.sizeDelta = new Vector2(diag, diag);
                // the fill's pivot (its bottom centre) on the diamond's bottom tip: in the pip's turned space that tip is the corner (-s/2, -s/2)
                f.localPosition = new Vector3(-s * 0.5f, -s * 0.5f, 0);
                fills[i] = f;
                fillImgs[i] = f.GetComponent<Image>();
                fillImgs[i].raycastTarget = false;
            }
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (!rowRead)
            {
                rowOn = N32Fixes.ViewRowOn(game.cfg, "grapplePips");
                rowRead = true;
            }
            SurvivalSim sim = game.sim;
            // NIGHT33 U (design.n33.hud33): with HUD33 drawn the charges are its two slash ticks under the reticle (UiHud33.cs): no diamonds
            bool live = rowOn && !Ui.Hud33Live && sim != null && sim.Grapple32On && sim.flow == Flow.Playing && sim.arcline21 != null && sim.arcline21.Owned;
            if (root.gameObject.activeSelf != live) root.gameObject.SetActive(live);
            Shown = live;
            if (!live) return;
            int n = sim.grapple32Rules.charges;
            if (fills == null || fills.Length != n) Build(n);
            int full = Mathf.Clamp(sim.grappleCharges32, 0, n);
            float next = sim.grappleRechargeSteps32 > 0 && full < n ? 1f - (float)sim.grappleRechargeLeft32 / sim.grappleRechargeSteps32 : 0f;
            if (lastFull >= 0 && full > lastFull)
                for (int i = lastFull; i < full; i++) flash[i] = UnityTuning.Grapple32PipFlashSeconds;
            lastFull = full;
            FullShown = full;
            NextFill = next;
            Color c = Cyan;
            float diag = UnityTuning.Grapple32PipSize * 1.4142135f;
            for (int i = 0; i < n; i++)
            {
                flash[i] = Mathf.Max(0, flash[i] - FeelClock.UnscaledDelta); // NIGHT63 D1: 0 in a same-run held frame
                float share = i < full ? 1f : i == full ? next : 0f;
                backs[i].color = new Color(c.r, c.g, c.b, UnityTuning.Grapple32PipEmptyAlpha);
                Color fc = i < full ? c : new Color(c.r, c.g, c.b, UnityTuning.Grapple32PipFillAlpha);
                if (flash[i] > 0) fc = Color.Lerp(fc, Color.white, flash[i] / UnityTuning.Grapple32PipFlashSeconds);
                fillImgs[i].color = fc;
                fills[i].sizeDelta = new Vector2(diag, diag * share);
            }
        }
    }
}
