// NIGHT19 (the lead owns this file): the reel's doors into H's screens (FeelReel's `ui-weapons`), so the reel never edits a
// builder's file: the WEAPONS screen opened and closed as TITLE's PROGRESS button and its BACK do.
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>FeelReel `ui-weapons`: open WEAPONS (UiWeapons.cs's OpenWeapons).</summary>
        public void ReelOpenWeapons() => OpenWeapons();

        /// <summary>FeelReel `ui-weapons`: its BACK (CloseMeta).</summary>
        public void ReelCloseMeta() => CloseMeta();
    }
}
