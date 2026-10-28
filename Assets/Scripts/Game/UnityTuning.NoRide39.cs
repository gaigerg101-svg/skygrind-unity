// NIGHT39 builder R1 (NIGHT39.md R1; GG 2026-10-13: "a new wall material with a clear visual texture that indicates you cannot wall ride on it"):
// Unity-only numbers for the no-ride surface (NoRide39View.cs; CLAUDE.md rule 5: each with a comment). View only: which walls are no-ride is the
// level's (campaign boxes tagged `noride: true`, Ride38.NoRideFrom: the Mover refuses the same boxes). Research (Research/2026-10-14-wave39/
// research/arena-boss-web.md section 3): every game marks the USABLE surface (Mirror's Edge red, Ghostrunner / Dying Light 2 yellow, Doom Eternal
// creased white) and none marks the unusable one; so the cyan ride trim stays the primary cue and the no-ride surface is its clear opposite:
// dark, matte, VERTICAL corrugation (a ride panel is smooth, lit, with horizontal cyan trim). Colours are colors.pillarSide (the level's dusk
// slate violet) darkened: neither cyan (usable) nor red-orange (enemies and danger only), CLAUDE.md rule 8.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float NoRide39PlateShade = 0.32f; // the backing plate over each no-ride face: colors.pillarSide x 0.32 (a dark, dull face: reads "dead" next to a lit ride panel)
        public const float NoRide39FinShade = 0.7f; // the vertical fins: colors.pillarSide x 0.7 (the stripes' contrast against the plate: about 2:1 in value, so the pattern holds at 30 m in dusk light)
        public const float NoRide39PlateProud = 0.02f; // m: the plate stands this far off the box's faces (view only, no collider; covers the wall's own colour)
        public const float NoRide39FinPitch = 0.8f; // m: fin spacing along a face. At 30 m a 0.8 m pitch is ~1.5 deg, ~24 px at 1080p (a 70 deg vertical FOV): a clear stripe pattern, not noise
        public const float NoRide39FinWidth = 0.22f; // m: a fin's width (~6 px at 30 m at 1080p: still a stripe)
        public const float NoRide39FinProud = 0.12f; // m: how far a fin stands off the face (its side faces catch the light: ridged, not painted)
        public const float NoRide39MinFace = 1.0f; // m: a face narrower than this gets the plate only (no fins: a 0.2 m liner's end)
        public const int NoRide39RibsHideFrames = 30; // frames between looks for Doors38View's NIGHT38 ribs ("No-ride ribs 38") to hide while this look is on (one pattern, not two)
    }
}
