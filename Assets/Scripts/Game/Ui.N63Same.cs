// NIGHT63 builder D (slate D1, same-run SPOT pairs): bossRule61 judged OFF as "the same frame with the row's UI root hidden" (the red team's
// finding 2: the rule line is decided at the boss's rise, so flipping the row at the capture frame would change nothing). The rule line's UI root
// is hud47's fight line holder (fight47.rt: the label, its shadow twin, outline and underlay; UiBossRule61.cs draws the line on it). In the reel's
// held frame (FeelReel.SameRun63, -skygrindSameRun63 n61.bossRule61) SameRun63View hides it when the rule line holds it (BossRule61Probe: a line
// showing, alpha above 0) and the reel shows it again after the off capture. Nothing else reads it; G's BossRule61 / UiBossRule61 are untouched.
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool same63Hid;

        /// <summary>The held frame: hide the fight line's holder when the rule line holds it. True when this call hid it (the proof line says
        /// so; false: no rule line up, nothing to hide, and the pair's 0 px is the honest answer).</summary>
        internal bool SameRun63HideRule()
        {
            if (fight47 == null || fight47.rt == null || BossRule61Probe.Age < 0 || BossRule61Probe.Alpha <= 0f) return false;
            if (!fight47.rt.gameObject.activeSelf) return false;
            fight47.rt.gameObject.SetActive(false);
            same63Hid = true;
            return true;
        }

        /// <summary>After the off capture: the holder back as it was.</summary>
        internal void SameRun63RestoreRule()
        {
            if (!same63Hid) return;
            same63Hid = false;
            if (fight47 != null && fight47.rt != null) fight47.rt.gameObject.SetActive(true);
        }
    }
}
