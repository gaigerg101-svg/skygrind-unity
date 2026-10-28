// NIGHT55 builder R (P2-clear; design.n55.clear55, a view row): the HUD's room-clear card. On the bus's `roomClear` for a combat room (Clear55.Counts)
// two lines under the reticle, in HUD33's own type (UiStyle33.Label: the 2 px shadow, no panel fill): the head "ROOM CLEAR · UNDER PAR" in the
// 24 px label face (bone), and "31.2 S · PAR 50 S" in the 32 px number face, cyan when within par (the player's own result: player-usable cyan),
// bone when over (never red: red-orange is danger only). The par is the event's (the level file's campaign.rooms[].par). It enters with the HUD's
// entrance (HudEntrance18: no pop-in; REDUCED MOTION: no scale), stays UnityTuning.Clear55LifeS of HUD frame time (it plays through the world's
// hold: the HUD runs on frame time) and fades over its last Clear55FadeS. Built on the NIGHT32 sight layer and kept in HUD33's "Kept" rect when
// HUD33 is on (as RIDE KILL: the NIGHT32 layers are hidden at alpha 0 there). Row off: nothing is built (NIGHT54's HUD). The strings are built
// once per clear; nothing allocates per frame. Numbers: UnityTuning.N55R.cs.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform c55Root;
        CanvasGroup c55Group;
        UiStyle33.Text33 c55Head, c55Line;
        double c55Age = -1, c55Enter = -1; // s of HUD time since the card's clear (-1: idle); the entrance's age (-1: done)
        float c55ShownAlpha = -1f, c55ShownScale = -1f;

        /// <summary>The reel's probe (a455): cards shown this session, the last card's two lines, its on-screen alpha now, its smallest font px.</summary>
        public static int Clear55Cards;
        public static string Clear55TextNow = "";
        public static float Clear55Alpha, Clear55MinPx;
        /// <summary>The card's box in screen px (bottom-left origin) at its last drawn frame.</summary>
        public static Rect Clear55ScreenRect;

        /// <summary>After BuildRideKill53: the card, hidden, and the bus's roomClear (row on only).</summary>
        void BuildClear55()
        {
            if (!Clear55.On(cfg)) return;
            float headPx = UnityTuning.Hud33Name, linePx = UnityTuning.Hud33Mid;
            float h = UnityTuning.Clear55HeadH + UnityTuning.Clear55Gap + UnityTuning.Clear55LineH;
            Vector2 mid = new Vector2(0.5f, 0.5f), top = new Vector2(0.5f, 1f), bottom = new Vector2(0.5f, 0f);
            c55Root = UiStyle33.Rect(sight, "Room clear card 55", mid, mid, new Vector2(0, UnityTuning.Clear55Y), new Vector2(UnityTuning.Clear55W, h));
            c55Group = c55Root.gameObject.AddComponent<CanvasGroup>();
            c55Group.blocksRaycasts = c55Group.interactable = false;
            c55Group.alpha = 0f;
            c55Head = UiStyle33.Label(c55Root, "Head", headPx, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.Center, top, top, Vector2.zero,
                new Vector2(UnityTuning.Clear55W, UnityTuning.Clear55HeadH));
            c55Line = UiStyle33.Label(c55Root, "Time vs par", linePx, UiStyle33.Bone, UiStyle33.FaceNumber, TextAlignmentOptions.Center, bottom, bottom, Vector2.zero,
                new Vector2(UnityTuning.Clear55W, UnityTuning.Clear55LineH), false);
            Clear55MinPx = Mathf.Min(c55Head.FontPx, c55Line.FontPx);
            c55Root.gameObject.SetActive(false);
            game.events.On<RoomEvent>("roomClear", e =>
            {
                // NIGHT56 L (clear55's r2 fix, gate-r2 FAIL 2 / 3: "the off side still draws the card at alpha 1.00"): the card obeys the off
                // switch the hold obeys. The Ui is built before a reel's -skygrindOff55 reaches NightRows.ForceOffFrom, so Clear55.On(cfg) at build
                // time can read ON; the hold (SurvivalGame.Clear55Beat) is built at the first frame, after it. No hold, no card (Cards stays 0).
                if (!Clear55.Counts(e) || game.Clear55Beat == null) return;
                c55Head.Set(Clear55.Head(e.time, e.par)); // built once per clear
                c55Line.Set(Clear55.Line(e.time, e.par));
                c55Line.Color = Clear55.Within(e.time, e.par) ? UiStyle33.Cyan : UiStyle33.Bone;
                Clear55TextNow = c55Head.Shown + " | " + c55Line.Shown;
                c55Age = 0;
                c55Enter = 0;
                Clear55Cards++;
            });
            game.OnNewRun += () =>
            {
                c55Age = -1;
                if (c55Root != null) c55Root.gameObject.SetActive(false);
            };
        }

        /// <summary>Each frame (after RenderRideKill53): the card's entrance, life and fade while it lives.</summary>
        void RenderClear55(double frameDt, bool playing)
        {
            if (c55Root == null) return;
            bool live = playing && c55Age >= 0 && c55Age < UnityTuning.Clear55LifeS && game.Clear55Beat != null; // NIGHT56 L: the hold's off switch hides the card too
            if (!live)
            {
                if (c55Root.gameObject.activeSelf) c55Root.gameObject.SetActive(false);
                if (!playing || c55Age >= UnityTuning.Clear55LifeS) c55Age = -1;
                Clear55Alpha = 0f;
                return;
            }
            if (hud33Kept != null && c55Root.parent != hud33Kept)
            {
                Keep33(c55Root); // HUD33 on: the NIGHT32 sight layer is hidden; the card draws in the kept rect (full-screen rects alike: its place holds)
                c55Root.SetAsLastSibling();
            }
            if (!c55Root.gameObject.activeSelf) c55Root.gameObject.SetActive(true);
            float s = 1f, enterA = 1f;
            if (c55Enter >= 0)
            {
                c55Enter = HudEntrance18.Step(c55Enter, frameDt);
                bool done = HudEntrance18.Done(c55Enter);
                s = done || game.settings.View.reducedMotion ? 1f : HudEntrance18.Scale(c55Enter);
                enterA = done ? 1f : HudEntrance18.Alpha(c55Enter);
                if (done) c55Enter = -1;
            }
            double fadeFrom = UnityTuning.Clear55LifeS - UnityTuning.Clear55FadeS;
            float a = enterA * (c55Age <= fadeFrom ? 1f : Mathf.Clamp01((float)(1.0 - (c55Age - fadeFrom) / UnityTuning.Clear55FadeS)));
            if (s != c55ShownScale)
            {
                c55ShownScale = s;
                c55Root.localScale = new Vector3(s, s, 1f);
            }
            if (a != c55ShownAlpha)
            {
                c55ShownAlpha = a;
                c55Group.alpha = a;
            }
            // the probe reads what reaches the screen: the alpha through every parent CanvasGroup, the card's own included (0 under a hidden
            // layer; the canvas's last rebuild), and the card's box
            Clear55Alpha = c55Root.gameObject.activeInHierarchy ? c55Line.main.canvasRenderer.GetInheritedAlpha() : 0f;
            Vector3 ls = c55Root.lossyScale, c = c55Root.position;
            float w = Mathf.Abs(c55Root.rect.width * ls.x), hh = Mathf.Abs(c55Root.rect.height * ls.y);
            Clear55ScreenRect = new Rect(c.x - w * 0.5f, c.y - hh * 0.5f, w, hh);
            c55Age += frameDt;
        }
    }
}
