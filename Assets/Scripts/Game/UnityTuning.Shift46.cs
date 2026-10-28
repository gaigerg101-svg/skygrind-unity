// NIGHT46 builder V1, the leftover row `shift46` (design.n46.shift46, a VIEW row; DECIDE46 N-2; DESIGN-RULES A-9 "State change" and "Light /
// colour story", A-6 "the next door is lit when the room clears"): a room's state change shows in its named fixture's light. The render numbers
// of Shift46View.cs and, at the end, the pure rule Shift46Rule (Unity-free: the room's state from the campaign the sim already keeps).
// CLAUDE.md rule 5: Unity-only numbers, each with a comment. Times are real (frame-time) seconds.
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Each room's named fixture (the A-9 cards' "light / colour story" lamp; campaign.lights ids in the n46 levels): room id, fixture id.
        /// A fixture or a room the booted level lacks is skipped. k5 (THE KILN'S HEART) has no row here: its KILN MOUTH is WyrmRing43View's own
        /// light ("ring43 kiln mouth light", with "ring43 core light"), turned by the crumble (Shift46CrumbleLights).</summary>
        public static readonly string[,] Shift46Fixtures =
        {
            { "b1", "b1-lamp" }, // THE SPILL: the ladle's amber lamp over the bridges (NIGHT45's card)
            { "g7", "g7-L3" }, // THE THAW LOCK: THE BOILER's gauge lamp on its door-side face (NIGHT45's card)
            { "p3", "p3-knot" }, // THE TIE-OFF: the KNOT's amber lamp (A1's n46-span)
            { "v2", "v2-bell-lamp" }, // THE NAVE: the BELL's warm lamp at its skirt (A2's card names it shift46's fixture)
            { "v6", "v6-organ-lamp" }, // THE CHOIR: the organ's lamp (A2's card)
        };
        /// <summary>k5's lights that the crumble turns (WyrmRing43View's GameObject names).</summary>
        public static readonly string[] Shift46CrumbleLights = { "ring43 kiln mouth light", "ring43 core light" };

        /// <summary>The last wave's colour: the danger red-orange (colors.enemy #FF4A1C; CLAUDE.md rule 8: red-orange is danger only).</summary>
        public const uint Shift46HotHex = 0xFF4A1C;
        /// <summary>The cleared room's colour is LightKit29's "cyan" (player-usable: the room is yours, A-6's "lit when the room clears").</summary>
        public const string Shift46ClearName = "cyan";
        /// <summary>The added light's intensity (x UnityTuning.LightScale), against the fixtures' own 0.8-2.2: the changed lamp is the room's
        /// brightest accent while it lasts, so the change reads from anywhere in the room.</summary>
        public const float Shift46LightIntensity = 3.2f;
        /// <summary>The added light's range: the fixture row's range x this (the colour reaches the floor the fixture lights).</summary>
        public const float Shift46RangeMul = 1.3f;
        /// <summary>The overlay's glow on the fixture's own slab (HDR x the colour: bloom catches it, so the lamp itself reads changed).</summary>
        public const float Shift46Glow = 2.6f;
        /// <summary>The overlay slab's size over the fixture's (it covers the amber slab beneath) and its depth (m) out of the surface.</summary>
        public const float Shift46OverlayScale = 1.06f, Shift46OverlayDepth = 0.06f;
        /// <summary>s: the change eases in over this (no pop: U-5's "nothing pops statically"), and back out when the state ends.</summary>
        public const float Shift46EaseIn = 0.25f, Shift46EaseOut = 0.6f;
        /// <summary>Hz and depth of the last wave's pulse (intensity x (1 - depth .. 1)): "hot", not steady; the cleared cyan holds steady.</summary>
        public const float Shift46PulseHz = 1.6f, Shift46PulseDepth = 0.35f;
        /// <summary>The crumble's multiplier on the KILN MOUTH's and the core's own light (its tell pulses at Shift46CrumbleHz; fallen: steady).</summary>
        public const float Shift46CrumbleMult = 3.0f, Shift46CrumbleHz = 3.0f;
    }

    /// <summary>NIGHT46 V1 shift46's rule, pure: the state of room `roomId` now from the campaign (Core's Campaign24, the sim's own; read only).</summary>
    public static class Shift46Rule
    {
        public const int None = 0, LastWave = 1, Cleared = 2, Crumble = 3;

        /// <summary>LastWave while the player is in the room, it fights, and its last wave is out (Campaign24.wave = the waves spawned); Cleared once
        /// the room is cleared (until the player leaves it); None otherwise. `lv` is the campaign's own level (sim.campaignLevel: waves31 applied).</summary>
        public static int RoomState(Campaign24 c, LevelDef lv, string roomId)
        {
            if (c == null || lv == null || c.room < 0 || c.room >= lv.rooms.Count) return None;
            RoomDef r = lv.rooms[c.room];
            if (r.id != roomId) return None;
            if (c.phase == Campaign24.Phase.Cleared) return Cleared;
            if (c.phase == Campaign24.Phase.Fight && r.WaveCount > 0 && c.wave >= r.WaveCount) return LastWave;
            return None;
        }

        /// <summary>k5's crumble (SurvivalSim.wyrm31CrumbleState: 1 the tell, 2 fallen): Crumble from the tell on; None before.</summary>
        public static int CrumbleState(int wyrm31CrumbleState) => wyrm31CrumbleState > 0 ? Crumble : None;
    }
}
