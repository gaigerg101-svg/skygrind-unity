// NIGHT41 builder J2 (design.n41.select41, a view row; NIGHT41.md J2, INVENTORY C9, DECIDE41 D-7): the level select's preview. NIGHT26's rows
// already show each level's name and the weapon it unlocks (the UNLOCKS column); with the row on, each playable / existing slot's name is
// followed by its BOSS in red-orange (the boss is danger), from design.n41.select41.bosses keyed by the level's family (foundry, iceline,
// kiln, span, spire: the part of the level id after "nNN-"), and the locked slot design.n41.select41.rebuildSlot (6) reads "6  REBUILDING"
// (rebuildText) instead of LOCKED. SHORT: a drawn boss SILHOUETTE needs art the project has no file for; the boss's name stands in for it
// tonight. Called from RefreshLevelSelect26 after a row's own texts are set; with the row off nothing here runs. Not checked in the Player.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>What select41 wrote (the levelselect reel prints it).</summary>
    public static class Select41Probe
    {
        public static int Bosses, Rebuilding;
    }

    public sealed partial class Ui
    {
        const string Select41Row = "design.n41.select41.";

        bool Select41On() => N41View.RowOn(cfg, "select41");

        /// <summary>A row's preview (the row's texts are NIGHT26's already): the boss after the name, or the rebuilding slot's words.</summary>
        void Select41Slot(LevelRow26 r, LevelSlot26 s, bool locked)
        {
            if (r == null || s == null || r.item == null || r.item.label == null) return;
            if (locked && cfg.Has(Select41Row + "rebuildSlot") && s.number == (int)cfg.Num(Select41Row + "rebuildSlot"))
            {
                r.item.label.text = s.number.ToString(System.Globalization.CultureInfo.InvariantCulture) + "  " + cfg.Str(Select41Row + "rebuildText");
                Select41Probe.Rebuilding++;
                return;
            }
            if (!s.exists) return;
            string boss = Boss41Of(s.id);
            if (string.IsNullOrEmpty(boss)) return;
            r.item.label.text += "  <color=#" + ColorUtility.ToHtmlStringRGB(UiStyle33.Danger) + ">" + boss + "</color>";
            Select41Probe.Bosses++;
        }

        /// <summary>The boss of a level id by its family ("n41-foundry" -> "foundry"), from design.n41.select41.bosses (null: none).</summary>
        string Boss41Of(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int dash = id.IndexOf('-');
            string family = dash >= 0 ? id.Substring(dash + 1) : id;
            string key = Select41Row + "bosses." + family;
            return cfg.Has(key) ? cfg.Str(key) : null;
        }
    }
}
