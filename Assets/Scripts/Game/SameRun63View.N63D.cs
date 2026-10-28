// NIGHT63 builder D (slate D1, same-run SPOT pairs; FeelReel.N63Same.cs is the reel): what the held frame needs beyond FeelClock's hold. Installed
// only with -skygrindSameRun63 (nothing exists without it).
// - UnityEngine.Random replayed: the held frame starts its Update with the shot frame's Random state (SameRun63Early, execution order -32000) and
//   its render with the shot frame's (SameRun63View.LateUpdate, order 32000), so per-frame view jitter (Boss31View's shake, Boss36View's death
//   shake, SkateFx27's dust) and URP's own per-frame Random (FilmGrain's offset) draw what they drew in the shot frame.
// - bossRule61 (kind UiRoot): in the held frame with the row off, the rule line's UI root is hidden (Ui.SameRun63HideRule); AfterHold shows it.
// - plant63 (the planted 10 x 10 px case): an opaque UnityTuning.Same63PlantPx square at Same63PlantAt px from the top-left on its own overlay
//   canvas (sorting order 32767, pixel perfect, scale 1: exactly px * px pixels), drawn while NightRows.ViewOn(cfg, "plant63") reads on, re-read
//   EVERY frame (the contract each claimed row keeps; the reel forces the row on for its run and off in the held frame).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT63 D1: the held frame's Update starts from the shot frame's Random state (before every other script's Update).</summary>
    [DefaultExecutionOrder(-32000)]
    public sealed class SameRun63Early : MonoBehaviour
    {
        void Update() => SameRun63View.EarlyUpdate();
    }

    /// <summary>NIGHT63 D1: the held frame's late work (after every other script's LateUpdate, before the render).</summary>
    [DefaultExecutionOrder(32000)]
    public sealed class SameRun63View : MonoBehaviour
    {
        static SameRun63View instance;
        static string row;
        static SameRun63Math.Kind kind;
        static Random.State early, late;
        static bool haveEarly, haveLate;

        /// <summary>The held frame hid the row's UI root (the proof line reads it; AfterHold clears it).</summary>
        public static bool HidThisHold { get; private set; }

        /// <summary>Frames the planted square was drawn / held frames whose Random state was replayed (the reel's telemetry).</summary>
        public static int PlantFrames, Replays;

        RawImage plant;

        /// <summary>FeelReel.SameRun63Boot, once, with the option's row.</summary>
        public static void Install(string r)
        {
            if (instance != null) return;
            row = r;
            kind = SameRun63Math.KindOf(r);
            var go = new GameObject("SameRun63") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<SameRun63Early>();
            instance = go.AddComponent<SameRun63View>();
            if (r == SameRun63Math.PlantRow) instance.BuildPlant();
        }

        void BuildPlant()
        {
            var cgo = new GameObject("SameRun63 plant", typeof(RectTransform));
            DontDestroyOnLoad(cgo);
            var canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvas.pixelPerfect = true;
            var igo = new GameObject("planted square", typeof(RectTransform));
            igo.transform.SetParent(cgo.transform, false);
            plant = igo.AddComponent<RawImage>();
            plant.color = new Color(UnityTuning.Same63PlantR, UnityTuning.Same63PlantG, UnityTuning.Same63PlantB, 1f);
            plant.raycastTarget = false;
            RectTransform rt = plant.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            Place();
        }

        void Place()
        {
            int[] r = SameRun63Math.PlantRect(Screen.width, Screen.height, UnityTuning.Same63PlantAt, UnityTuning.Same63PlantPx);
            RectTransform rt = plant.rectTransform;
            var pos = new Vector2(r[0], -r[1]);
            var size = new Vector2(r[2], r[3]);
            if (rt.anchoredPosition != pos) rt.anchoredPosition = pos;
            if (rt.sizeDelta != size) rt.sizeDelta = size;
        }

        /// <summary>SameRun63Early: a held frame starts from the shot frame's state; any other frame saves its own.</summary>
        internal static void EarlyUpdate()
        {
            if (FeelClock.Holding)
            {
                if (haveEarly) Random.state = early;
                return;
            }
            early = Random.state;
            haveEarly = true;
        }

        void LateUpdate()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (plant != null)
            {
                bool on = NightRows.ViewOn(g != null ? g.cfg : null, SameRun63Math.PlantRow); // re-read every frame (the claimed rows' contract)
                if (plant.enabled != on) plant.enabled = on;
                if (on)
                {
                    Place(); // boss61 switches 1920x1080 -> 1280x720 mid-reel
                    PlantFrames++;
                }
            }
            if (kind == SameRun63Math.Kind.UiRoot && FeelClock.Holding && !HidThisHold && !NightRows.Live(true, row) && g != null && g.UiView != null)
                HidThisHold = g.UiView.SameRun63HideRule();
            if (FeelClock.Holding)
            {
                if (haveLate)
                {
                    Random.state = late;
                    Replays++;
                }
                return;
            }
            late = Random.state;
            haveLate = true;
        }

        /// <summary>The reel, after the held frame's off capture: the UI root shown again.</summary>
        public static void AfterHold(SurvivalGame g)
        {
            if (HidThisHold && g != null && g.UiView != null) g.UiView.SameRun63RestoreRule();
            HidThisHold = false;
        }
    }
}
