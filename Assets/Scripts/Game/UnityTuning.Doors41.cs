// NIGHT41 builder J2 (design.n41.doors41On, a view row): the doors38 curtains' iteration after T2's doors41 frames (GG w37: the doors "a
// holographic barrier or a different colour": locked red-orange, open cyan, the boss door pale violet-white; those colours are already
// Doors38View's). CLAUDE.md rule 5: render numbers only, each with a comment. These scale Doors38View's own numbers (UnityTuning.Doors38.cs, never
// edited in place) ONLY while the curtains are on through doors41On; at 1 the curtain is NIGHT38's exactly (x 1f is exact). Tuned from the frames.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Doors41SheetOpen = 1f; // x Doors38OpenAlpha: the open (cyan) sheet's density
        public const float Doors41SheetLocked = 1f; // x Doors38LockedAlpha: the locked (red-orange) sheet's density (a locked door must read solid)
        public const float Doors41SheetBoss = 1f; // x Doors38BossAlpha: the open boss door's (violet-white) sheet
        public const float Doors41Lines = 1f; // x Doors38LineAlpha: the climbing scanlines (the "holographic" read)
        public const float Doors41Edges = 1f; // x Doors38EdgeAlpha: the lit frame (posts + top bar)
        public const float Doors41Light = 1f; // x Doors38LightIntensity: the doorway's point light in the curtain's colour
    }
}
