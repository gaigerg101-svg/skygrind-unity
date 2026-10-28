// NIGHT41 builder B8 (brawlLook41): THE ICEBREAKER's ice armour look (Brawl40View.N41.cs). Render numbers only.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The ice plates' vertex colour (SKYGRIND/Stone reads vertex colours only: Brawl40View.Opaque's _BaseColor never reached it,
        /// so the body drew white and the warm room light made it the walls' tan, n41-r1 frames). A strong blue: the room light keeps about
        /// 0.9 / 0.7 / 0.5 of R / G / B (NIGHT40 L2's measure), so a pale blue-white albedo lands grey; this one lands pale blue. Not cyan.</summary>
        public const uint Brawl41PlateHex = 0x5f95ff;
        /// <summary>The dark stone core (legs, skull, fists' stone): near-black violet, under the plates.</summary>
        public const uint Brawl41CoreHex = 0x2a2833;
        /// <summary>The frost: faint additive pale-blue edges on the plates (unlit, so they read blue under any room light).</summary>
        public const uint Brawl41FrostHex = 0x9fc4ff;
        public const float Brawl41FrostGain = 0.55f;
        /// <summary>The fist shell: an additive cube over each fist (x the fist's size) that glows red-orange through its tell (gain from TellMin
        /// to this at the strike) and cyan while stuck (the punish), so the attacking fist reads, not a thin knuckle strip.</summary>
        public const float Brawl41ShellScale = 1.18f, Brawl41ShellGain = 1.8f, Brawl41ShellStuckGain = 1.2f;
        /// <summary>The poses (added to Brawl40's): the slam wind-up crouches (m) and arches back (deg), the rush wind-up drops lower (m).</summary>
        public const float Brawl41SlamCrouch = 0.45f, Brawl41SlamArch = -14f, Brawl41RushCrouch = 0.5f;
        /// <summary>The shatter: the death's blocks (crunch's Chunks.Break) are thrown at this overkill share (1 = a plain kill).</summary>
        public const float Brawl41ShatterThrow = 1.6f;
    }
}
