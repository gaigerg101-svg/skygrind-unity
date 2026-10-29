// PG3 (GG 2026-10-28: "please show controls at the start and display them longer or make it more visible and also remind you at times"): the controls card.
// At the start of every run (the flow turning to Playing) the full card shows in the top-left column, above the lesson card, for UnityTuning.ControlsCardSeconds:
// a CONTROLS head and five lines in the device's own keys (the live bindings through FillGlyphs; the mouse's fire / zoom as LMB / RMB, a pad's as RT / LT).
// Then every UnityTuning.ControlsRemindEvery seconds of play a two-line reminder shows for UnityTuning.ControlsRemindSeconds. Both follow SETTINGS'
// FIRST-RUN GUIDE (UnityTuning.OnboardingSetting); never in the attract run or a silent (bot / check) run. Display only: no rule changes.
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform controlsRt;
        CanvasGroup controlsGroup;
        TextMeshProUGUI controlsText;
        float controlsH, controlsAlpha;
        double controlsLeft, controlsPlayed, controlsNextRemind;
        bool controlsWasPlaying;

        void BuildControlsCard()
        {
            var card = Card("Controls card", out controlsGroup);
            controlsRt = card.rectTransform;
            float w = UnityTuning.ToastWidth - 2 * UnityTuning.ToastPadX;
            controlsText = Text(controlsRt, "Text", "", tBody, text, TextAlignmentOptions.TopLeft, new Vector2(UnityTuning.ToastPadX, -UnityTuning.ToastPadY), new Vector2(w, UnityTuning.ToastHeadLine), new Vector2(0, 1));
            UiTheme.Style(controlsText, UiTheme.Px(UiTheme.Size.S24), UiTheme.CyanPale, UiTheme.BodyBold); // the teaching cards' look, at the largest card size
            controlsText.textWrappingMode = TextWrappingModes.NoWrap;
            controlsText.richText = true;
            controlsRt.gameObject.SetActive(false);
        }

        string ControlsLines(bool full)
        {
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            string fire = pad ? "RT" : "LMB", zoom = pad ? "LT" : "RMB", move = pad ? "L-STICK" : FillGlyphs("{forward}{left}{back}{right}");
            if (!full)
                return FillGlyphs($"{move} MOVE   {{jump}} JUMP   {{dash}} DASH   {{slide}} SLIDE\n{fire} FIRE (HOLD: CHARGE)   {zoom} ZOOM   {{punch}} PUNCH");
            return FillGlyphs($"<size=130%>CONTROLS</size>\n{move}  MOVE      {{jump}}  JUMP (AGAIN IN THE AIR)\n{{dash}}  DASH      {{slide}}  SLIDE (SLAM IN THE AIR)\n"
                + $"{fire}  FIRE (HOLD TO CHARGE)      {zoom}  ZOOM\n{{punch}}  PUNCH / PARRY      {{grapple}}  GRAPPLE\n{{quickSwap}}  SWAP GUN      {{interact}}  BUY / USE");
        }

        void ShowControls(bool full)
        {
            controlsText.text = ControlsLines(full);
            Vector2 size = controlsText.GetPreferredValues(controlsText.text);
            controlsText.rectTransform.sizeDelta = new Vector2(Mathf.Ceil(size.x) + 2f, size.y);
            controlsH = size.y + 2 * UnityTuning.ToastPadY;
            controlsRt.sizeDelta = new Vector2(Mathf.Ceil(size.x) + 2f + 2 * UnityTuning.ToastPadX, controlsH);
            controlsLeft = full ? UnityTuning.ControlsCardSeconds : UnityTuning.ControlsRemindSeconds;
            controlsAlpha = 0;
            controlsGroup.alpha = 0; // fades in once the layout placed it
            if (!controlsRt.gameObject.activeSelf) controlsRt.gameObject.SetActive(true);
            leftDirty = true;
        }

        void RenderControlsCard(double frameDt)
        {
            if (controlsRt == null) return;
            bool quiet = game.attractOn || (game.sim != null && game.sim.metaSilent) || !game.settings.Bool(UnityTuning.OnboardingSetting);
            bool playing = game.flow == Flow.Playing && !quiet;
            if (playing && !controlsWasPlaying && controlsPlayed <= 0) // a run's first playing frame: the full card
            {
                ShowControls(true);
                controlsNextRemind = UnityTuning.ControlsRemindEvery;
            }
            if (game.flow != Flow.Playing && game.flow != Flow.Paused) controlsPlayed = 0; // back at a menu: the next run starts fresh
            controlsWasPlaying = playing;
            if (playing)
            {
                controlsPlayed += frameDt;
                if (controlsPlayed >= controlsNextRemind && !controlsRt.gameObject.activeSelf)
                {
                    ShowControls(false);
                    controlsNextRemind = controlsPlayed + UnityTuning.ControlsRemindEvery;
                }
            }
            if (!controlsRt.gameObject.activeSelf) return;
            if (playing) controlsLeft -= frameDt; // a pause holds it
            if (quiet) controlsLeft = 0;
            float fade = (float)(frameDt / UnityTuning.CardFade);
            float a = Mathf.Clamp01(controlsAlpha + (controlsLeft > 0 ? fade : -fade));
            if (a != controlsAlpha)
            {
                controlsAlpha = a;
                controlsGroup.alpha = a;
            }
            if (controlsLeft <= 0 && a <= 0)
            {
                controlsRt.gameObject.SetActive(false);
                leftDirty = true;
            }
        }
    }
}
