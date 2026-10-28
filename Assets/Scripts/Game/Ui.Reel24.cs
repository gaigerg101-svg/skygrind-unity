// NIGHT24 (builder R): the reel's door into TITLE's MODE row, so FeelReel `title` reads the whole wrap of the row and not only the label it
// shows now. It does not PICK a mode (picking CAMPAIGN reloads the scene on the level): it lists, in the order the row's cycle visits them,
// the labels CycleMode20 would show, from the same predicates (CampaignOn, GrindOn) and the same label constants.
using System.Collections.Generic;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>FeelReel `title`: the MODE row's labels in cycle order (CAMPAIGN when on, SURVIVAL, THE GRIND when on), one wrap.</summary>
        public List<string> ReelModeLabels()
        {
            var labels = new List<string>();
            if (CampaignOn) labels.Add(UnityTuning.CampaignLabel);
            labels.Add("SURVIVAL");
            if (GrindOn) labels.Add(UnityTuning.GrindLabel);
            return labels;
        }

        /// <summary>FeelReel `title`: the MODE row is on screen (its GameObject active) and the label it shows now.</summary>
        public bool ReelModeRowShown(out string current)
        {
            current = setupMode != null && setupMode.value != null ? setupMode.value.text : null;
            return setupMode != null && setupMode.go.activeInHierarchy;
        }
    }
}
