// NIGHT20 U3-34.10 beyond (builder H): the TITLE and RESULTS passes with the package effects (HudFx20: LitMotion's per-
// character reveal), never on everything (banned 3): two moments only.
// - TITLE: each time it opens, the SKYGRIND wordmark (and its CYAN-DEEP offset copy, in step) reveals letter by letter.
// - RESULTS: the grade's letter reveals as its group slams in (the tally's grade stage: P's UiResults, read here, not edited).
// (NIGHT24: the WEEKLY badge's plate went with the WEEKLY.) Read from the screens' own state each frame they show; written on change only.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool menusFxTitleShown20;
        float menusFxGradeAlpha20;

        /// <summary>From RenderMenus, every frame: the TITLE wordmark's reveal on open, the grade's reveal.</summary>
        void RenderMenusFx20()
        {
            bool still = game.settings.View.reducedMotion;
            bool titleOn = title != null && title.activeSelf;
            if (titleOn && !menusFxTitleShown20)
            {
                HudFx20.DoReveal(wordmark, still);
                HudFx20.DoReveal(wordmarkBack, still);
            }
            menusFxTitleShown20 = titleOn;

            bool resultsOn = results != null && results.activeSelf;
            float ga = resultsOn && tallyGradeG != null ? tallyGradeG.alpha : 0f;
            if (ga > 0 && menusFxGradeAlpha20 <= 0) HudFx20.DoReveal(tallyGrade, still);
            menusFxGradeAlpha20 = ga;

        }
    }
}
