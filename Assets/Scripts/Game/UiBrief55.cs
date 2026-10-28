// NIGHT55 K brief55 (Brief55.cs; slate P1-teachText): the level text's time and place, in the existing styles (no restyle: NIGHT54 D-4). Hooks
// (one line each): UiTeach43.cs Teach43Draw's first line `s = Teach55Filter(s, frameDt);`; UiMeta.cs LayoutLeft `y = Brief55LeftTop(y);` and its
// caller `if (leftDirty || Brief55LeftMoved()) LayoutLeft();`. Off (the row off, -skygrindOff55): every hook returns its input.
//   - Teach55Filter: the line the centre card is asked to show (text36's slot) is passed through until it has stood Brief55.TeachMaxS on the card,
//     then withheld (the card closes as it does when the slot empties) while the slot keeps that same line; a new line opens the card again. A
//     basic-jump line ("B to jump") is withheld outside the campaign's first level.
//   - Brief55LeftTop: the top-left column (the lesson card, the hint, the toasts: UiMeta.LayoutLeft) starts under the room counter's plate
//     (UiHud33.Counter43) while that counter shows, so no toast is ever drawn over "N LEFT"; Brief55LeftMoved re-lays the column when the counter
//     opens or closes.
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The level text's probe (the a355 reel and the telemetry check print it).</summary>
    public static class Teach55Probe
    {
        public static int Withheld, JumpWithheld, ColumnMoves;
        /// <summary>NIGHT57 V: the column checks that counted the counter as shown before its root went active (the open frame: the fix's input).</summary>
        public static int OpenFrameMoves;
        public static string LastWithheld = "";
        public static float LastStoodS, ColumnTop;
    }

    public sealed partial class Ui
    {
        string t55Line, t55Done;
        double t55Age;
        bool t55CounterWas;
        readonly Vector3[] t55Corners = new Vector3[4];

        /// <summary>UiTeach43.Teach43Draw's first line: the line to draw (null: none) after the row's time cap and its level-1-only basic jump.</summary>
        string Teach55Filter(string s, double frameDt)
        {
            double cap = Brief55.TeachMaxS(cfg);
            if (cap <= 0) return s;
            if (string.IsNullOrEmpty(s))
            {
                t55Line = null;
                t55Done = null;
                return s;
            }
            if (s == t55Done) return null; // stood its time: closed while the slot keeps it
            if (s != t55Line)
            {
                t55Line = s;
                t55Age = 0;
                if (!Brief55.FirstLevel(cfg, game.MapId) && Brief55.IsBasicJump(Brief55.Plain(Teach43Rich(s))))
                {
                    t55Done = s;
                    Teach55Probe.JumpWithheld++;
                    Teach55Probe.LastWithheld = s;
                    return null;
                }
            }
            t55Age += frameDt;
            if (t55Age >= cap)
            {
                t55Done = s;
                Teach55Probe.Withheld++;
                Teach55Probe.LastWithheld = s;
                Teach55Probe.LastStoodS = (float)t55Age;
                return null;
            }
            return s;
        }

        /// <summary>UiMeta.LayoutLeft's column top (`y`: px below the meta layer's top, negative): under the counter's plate while it shows.</summary>
        float Brief55LeftTop(float y)
        {
            if (!Brief55.On(cfg) || c43Root == null || !Brief55CounterShows() || metaLayer == null) return y;
            // the plate's lowest drawn edge: the root and every active child (hud52's plate and shadow, the line track sit past the root's rect);
            // an overlay canvas: world = screen pixels. Only on a layout (a card shown or gone, the counter opened or closed): never per frame.
            c43Root.GetWorldCorners(t55Corners);
            float bottom = Mathf.Min(t55Corners[0].y, t55Corners[3].y), top = Mathf.Max(t55Corners[1].y, t55Corners[2].y);
            foreach (RectTransform rt in c43Root.GetComponentsInChildren<RectTransform>(false))
            {
                rt.GetWorldCorners(t55Corners);
                bottom = Mathf.Min(bottom, Mathf.Min(t55Corners[0].y, t55Corners[3].y));
                top = Mathf.Max(top, Mathf.Max(t55Corners[1].y, t55Corners[2].y));
            }
            Canvas c = metaLayer.GetComponentInParent<Canvas>();
            // NIGHT56 G (P1-teachText, the r1 critic: "a card over the counter in 94 frames (worst 42.1 px)" on kiln and spire): the column is laid
            // when the counter OPENS, while its open animation still draws the plate 4.2 px tall (n56-r1 verbs37 telemetry, frames 702-711: 4.2 ->
            // 83.6 px), so the hint sat at 70.5 px under a plate that grew to 136. The column now clears the plate's height AT REST
            // (UnityTuning.Brief55CounterRestPx at the 1080p reference, x the canvas scale) from the plate's top, whichever is lower.
            float scale = c != null ? c.scaleFactor : 1f;
            bottom = Mathf.Min(bottom, top - UnityTuning.Brief55CounterRestPx * scale);
            Camera cam = c != null && c.renderMode != RenderMode.ScreenSpaceOverlay ? c.worldCamera : null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(metaLayer, new Vector2(t55Corners[0].x, bottom), cam, out Vector2 local)) return y;
            float below = local.y - metaLayer.rect.yMax - UnityTuning.Brief55CounterGap; // px from the layer's top (negative: down)
            Teach55Probe.ColumnTop = Mathf.Min(y, below);
            return Mathf.Min(y, below);
        }

        /// <summary>UiMeta's caller of LayoutLeft: true once when the counter opened or closed (the column moves under it, or back).</summary>
        bool Brief55LeftMoved()
        {
            if (!Brief55.On(cfg)) return false;
            bool now = Brief55CounterShows();
            if (now == t55CounterWas) return false;
            t55CounterWas = now;
            Teach55Probe.ColumnMoves++;
            return true;
        }

        /// <summary>The counter shows this frame. NIGHT56's rule (and with -skygrindOff57): its root is active. NIGHT57 V (slate V2-brief55): Ui's
        /// render lays the meta column (RenderMeta) BEFORE the HUD opens the counter (RenderHud33 -> Counter43, the same frame), so on the frame
        /// it opened the column still sat over the plate (n56-r5 verbs37 kiln / spire f702: the hint at y 56, the plate drawn at 53.9-58.4: 2.4 px;
        /// laid under it on f703). Now the counter's own open test (Counter43: the room's left count >= 0 while Playing) counts too, so the column
        /// moves on the frame the counter opens; a closing counter stays counted until its root goes inactive (its close animation still draws).</summary>
        bool Brief55CounterShows()
        {
            if (c43Root == null) return false;
            if (c43Root.gameObject.activeInHierarchy) return true;
            if (!Brief55.OpenFix57(cfg)) return false;
            Transform p = c43Root.parent;
            if (p == null || !p.gameObject.activeInHierarchy || game == null) return false;
            SurvivalSim sim = game.sim;
            if (sim == null || game.flow != Flow.Playing) return false;
            int n = Counter43Probe.Force != int.MinValue ? Counter43Probe.Force : sim.N43RoomLeft;
            if (n >= 0) Teach55Probe.OpenFrameMoves++;
            return n >= 0;
        }
    }
}
