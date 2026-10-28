// NIGHT25 (the shared commit, the lead owns this file): the hooks the NIGHT25 world views hang on. A builder implements the
// declaration-only partial in its own file and names the one call in its report. Nothing runs without a body.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>Once at game init, after the NIGHT24 views.</summary>
        void N25InitViews()
        {
            N25WireLessons(); // NIGHT25 (the lead): the lesson prompts and the campaign's progress
            N25InitViewsHook();
        }

        /// <summary>NIGHT25: lessonBegin shows the stage's prompt line (UiLesson25: the pad's variant on a pad), lessonClear hides it; layerClear saves the
        /// next level of the curriculum as where the CAMPAIGN row resumes (GoCampaign).</summary>
        void N25WireLessons()
        {
            events.On<LessonEvent>("lessonBegin", e =>
            {
                string text = PlayerDevices.Last == InputDevice.Pad && !string.IsNullOrEmpty(e.promptPad) ? e.promptPad : e.prompt;
                if (!string.IsNullOrEmpty(text)) ui?.ShowLesson25(e.prompt, e.promptPad);
            });
            events.On<LessonEvent>("lessonClear", _ => ui?.HideLesson25());
            events.On<RoomEvent>("layerClear", e =>
            {
                N25Blocks n25 = N25Blocks.ForceOff ? N25Blocks.Off : N25Blocks.From(cfg);
                string next = e.id; // the next level of the order that has a map file (levels 4-10 land through the night)
                do next = Curriculum25.Next(cfg, n25, next); while (next != null && !mapList.Exists(m => m.id == next));
                PlayerPrefs.SetString(CampaignNextPref, next ?? string.Empty);
                Debug.Log($"[skygrind] campaign: {e.id} cleared, next level '{next ?? "(end)"}'");
            });
        }

        /// <summary>The curriculum level the CAMPAIGN row starts on: the saved next level when it is still in the order, else the first.</summary>
        public string CampaignStartLevel()
        {
            N25Blocks n25 = N25Blocks.ForceOff ? N25Blocks.Off : N25Blocks.From(cfg);
            string saved = PlayerPrefs.GetString(CampaignNextPref, string.Empty);
            var order = Curriculum25.Order(cfg, n25);
            return !string.IsNullOrEmpty(saved) && order.Contains(saved) && mapList.Exists(m => m.id == saved) ? saved : Curriculum25.Start(cfg, n25);
        }

        const string CampaignNextPref = "skygrind.campaign.next"; // NIGHT25: a Unity-only pref key (CLAUDE.md rule 5: prefs keys)

        /// <summary>Every render frame beside the other world views (worldDt: the slowed world frame time; alpha: the sim's render interpolation).</summary>
        void N25RenderViews(double worldDt, double alpha) => N25RenderViewsHook(worldDt, alpha);

        partial void N25InitViewsHook();
        partial void N25RenderViewsHook(double worldDt, double alpha);
    }
}
