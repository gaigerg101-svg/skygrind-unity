// NIGHT22 U3-36.4 THE STAMP (builder S): the inked rank letter on screen at each round clear (the sim's "roundGrade" event,
// Core/Grades21.cs; design.n21.stamp). It slams in from fromScale to 1 over inTime with its alpha rising over the first half, holds,
// and leaves over outTime (Core StampMotion: one curve for the UI, the harness and the tests), tilted like a rubber stamp, on an
// INK plate, the letter in the grade's results.gradeColors colour (a round P: CYAN-HOT; gold is points only). D-7, the rank
// letter's lost shine (UIEffect is Editor-only): the Player-safe replacement is a pale edge light, a second copy of the letter
// 3 px up-left under it, plus the hard INK shadow copy down-right: two more TMP texts on the same material, no new shader. The
// stamp is a picture of the event: the wiring is Ui's BuildN22Hud / RenderN22Hud calling BuildStamp22 / RenderStamp22. REDUCED MOTION
// keeps the fade and drops the slam. UNCOMPILED when written (the builder has no Editor).
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform stamp22Root;
        CanvasGroup stamp22Group;
        TextMeshProUGUI stamp22Letter, stamp22Edge, stamp22Ink, stamp22Label;
        UiShape stamp22Plate;
        StampMotion.P stamp22P;
        bool stamp22On;
        double stamp22Age;
        float stamp22ShownScale = -1, stamp22ShownAlpha = -1;
        readonly System.Text.StringBuilder stamp22Sb = new System.Text.StringBuilder(24);

        /// <summary>The stamp now (the Player reel reads these): its grade ("P", "S" ... or null before the first), age in s, scale and alpha as drawn.</summary>
        public string Stamp22Grade { get; private set; }
        public double Stamp22Age => stamp22Age;
        public float Stamp22Scale => stamp22ShownScale;
        public float Stamp22Alpha => stamp22ShownAlpha;
        /// <summary>The stamp is built (the row's numbers are in the config).</summary>
        public bool Stamp22Built => stamp22Group != null;

        // --- D-7: the style meter's rank letter, its shine in the Player (UIEffect is Editor-only, HudFx20.Shine is null there) -----------
        // One slanted pale band sweeps across the letter's box once per rank-up, clipped to the box by a RectMask2D (the built-in
        // clip; no new shader or material). In the Editor, where the package's Shiny runs, the band stays off (no double shine).
        UiShape styleSheen22;
        RectTransform styleSheenRt22;
        double styleSheenAge22 = 99;
        float styleSheenSpan22;

        void BuildSheen22(RectTransform letterBox, float lw, float box)
        {
            var mid = new Vector2(0.5f, 0.5f);
            RectTransform clip = Hud17Rect(letterBox, "Letter sheen (clip)", mid, mid, Vector2.zero, new Vector2(lw, box));
            clip.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            styleSheen22 = Hud17Shape(clip, "Letter sheen", UiTheme.A(UiTheme.CyanHot, UnityTuning.Stamp22SheenAlpha), mid, mid, Vector2.zero, new Vector2(lw * UnityTuning.Hud20FxShineWidth, box * 1.3f));
            styleSheen22.SetCuts(0, 0);
            styleSheen22.enabled = false;
            styleSheenRt22 = styleSheen22.rectTransform;
            styleSheenSpan22 = lw * (0.5f + UnityTuning.Hud20FxShineWidth);
        }

        /// <summary>A rank-up: start the sweep unless the package's shine is installed (it runs its own) or REDUCED MOTION.</summary>
        void SheenStart22(bool reducedMotion)
        {
            if (styleSheen22 == null || reducedMotion || HudFx20.On && HudFx20.Shine != null) return;
            styleSheenAge22 = 0;
        }

        void RenderSheen22(double frameDt)
        {
            if (styleSheen22 == null) return;
            bool on = styleSheenAge22 < UnityTuning.Hud20FxShineTime;
            if (styleSheen22.enabled != on) styleSheen22.enabled = on;
            if (!on) return;
            styleSheenAge22 += frameDt;
            float u = Mathf.Clamp01((float)(styleSheenAge22 / UnityTuning.Hud20FxShineTime));
            styleSheenRt22.anchoredPosition = new Vector2(Mathf.Lerp(-styleSheenSpan22, styleSheenSpan22, u), 0);
        }

        void BuildStamp22()
        {
            if (!cfg.Has("design.n21.stamp.outTime") || !cfg.Has("design.n21.stamp.fromScale")) return; // an old config copy: no stamp
            stamp22P = StampMotion.P.From(cfg);
            stamp22Age = stamp22P.Total; // nothing until the first roundGrade
            float plate = UnityTuning.Stamp22Plate;
            var mid = new Vector2(0.5f, 0.5f);
            stamp22Root = Hud17Rect(values, "Round stamp", mid, mid, new Vector2(0, UnityTuning.Stamp22Y), new Vector2(plate, plate));
            stamp22Root.localRotation = Quaternion.Euler(0, 0, UnityTuning.Stamp22TiltDeg);
            stamp22Group = stamp22Root.gameObject.AddComponent<CanvasGroup>();
            stamp22Group.alpha = 0;
            stamp22Group.interactable = stamp22Group.blocksRaycasts = false;
            stamp22Plate = Hud17Shape(stamp22Root, "Plate", UiTheme.InkSolid, mid, mid, Vector2.zero, new Vector2(plate, plate));
            stamp22Plate.SetCuts(UnityTuning.ThemeCut * 2, UnityTuning.ThemeCut * 2);
            float px = UiTheme.Px(UiTheme.Size.S133);
            var box = new Vector2(plate, plate);
            // back to front: the INK shadow (down-right), the pale edge light (up-left), the letter
            stamp22Ink = Hud17Text(stamp22Root, "Letter (ink)", "", px, UiTheme.InkSolid, UiTheme.Rank, TextAlignmentOptions.Center, mid, mid, new Vector2(UnityTuning.Stamp22InkOffset, -UnityTuning.Stamp22InkOffset), box, false);
            stamp22Edge = Hud17Text(stamp22Root, "Letter (edge light)", "", px, UiTheme.A(UiTheme.CyanPale, UnityTuning.Stamp22EdgeAlpha), UiTheme.Rank, TextAlignmentOptions.Center, mid, mid, new Vector2(-UnityTuning.Stamp22EdgeOffset, UnityTuning.Stamp22EdgeOffset), box, false);
            stamp22Letter = Hud17Text(stamp22Root, "Letter", "", px, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.Center, mid, mid, Vector2.zero, box, false);
            Hud17Stroke(stamp22Letter);
            stamp22Label = Hud17Text(stamp22Root, "Round", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, mid, mid, new Vector2(0, -plate * 0.5f - UnityTuning.Stamp22LabelGap - 12), new Vector2(plate, 34), true);
            stamp22Root.gameObject.SetActive(false);
            game.events.On<RoundGradeEvent>(Grades21.EventName, Stamp22Show);
        }

        /// <summary>The sim said roundGrade: the stamp starts from its first frame (a new stamp replaces one still on screen).</summary>
        void Stamp22Show(RoundGradeEvent e)
        {
            if (stamp22Group == null) return;
            Stamp22Grade = e.grade;
            Color c = e.p ? UiTheme.CyanHot : UiTheme.White;
            if (!e.p && gradeHex != null && gradeHex.TryGetValue(e.grade, out string hex) && ColorUtility.TryParseHtmlString("#" + hex, out Color gc)) c = gc;
            stamp22Letter.text = e.grade;
            stamp22Edge.text = e.grade;
            stamp22Ink.text = e.grade;
            stamp22Letter.color = c;
            stamp22Sb.Clear().Append("ROUND ").Append(e.round);
            stamp22Label.text = stamp22Sb.ToString();
            stamp22Age = 0;
            stamp22ShownScale = stamp22ShownAlpha = -1;
        }

        void RenderStamp22(double frameDt)
        {
            if (stamp22Group == null) return;
            bool playing = game.flow == Flow.Playing || game.flow == Flow.Paused;
            if (!playing) stamp22Age = stamp22P.Total; // RESULTS / DOWN / TITLE: it never comes back on a later flow
            bool live = stamp22Age < stamp22P.Total;
            if (!live)
            {
                if (stamp22On)
                {
                    stamp22On = false;
                    stamp22Root.gameObject.SetActive(false);
                    stamp22ShownAlpha = 0;
                }
                return;
            }
            if (game.flow == Flow.Playing) stamp22Age += frameDt;
            StampMotion.At(stamp22Age, stamp22P, out double s, out double a);
            if (game.settings.View.reducedMotion) s = 1; // the fade stays, the slam goes
            bool visible = a > 0;
            if (visible != stamp22On)
            {
                stamp22On = visible;
                stamp22Root.gameObject.SetActive(visible);
            }
            if (!visible)
            {
                stamp22ShownAlpha = 0; // the reel's exit measure read the last drawn alpha (0.0x) as visible for the rest of the clip
                return;
            }
            if ((float)s != stamp22ShownScale)
            {
                stamp22ShownScale = (float)s;
                stamp22Root.localScale = new Vector3(stamp22ShownScale, stamp22ShownScale, 1);
            }
            if ((float)a != stamp22ShownAlpha)
            {
                stamp22ShownAlpha = (float)a;
                stamp22Group.alpha = stamp22ShownAlpha;
            }
        }
    }
}
