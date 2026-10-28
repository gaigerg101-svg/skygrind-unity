// NIGHT25 (builder F): when the DOOR OPEN cue may show. The cue and the room-title banner share the top centre, so the cue waits while a banner is up; but it never waits
// for ever (the Player's `cue` run: a banner timer that never ran out kept the cue off for the whole clear). Rule: a cue already showing stays; a banner (the main one or the
// ROOM n banner) with time left holds the cue back for at most UnityTuning.Cue25MaxWait seconds of waiting; after that it shows whatever the banners say. Pure: no UnityEngine.
namespace Skygrind.Game
{
    public static class CueWait25
    {
        /// <summary>`waited`: the seconds the cue has been eligible and held back so far (the caller adds the frame time while this returns false and a banner is up).</summary>
        public static bool MayShow(bool alreadyShowing, double bannerLeft, double roomBannerLeft, double waited, double maxWait)
        {
            if (alreadyShowing) return true;
            if (bannerLeft <= 0 && roomBannerLeft <= 0) return true;
            return waited >= maxWait;
        }
    }
}
