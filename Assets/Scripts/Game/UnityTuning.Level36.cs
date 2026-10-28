// NIGHT36 builder L (NIGHT36.md L1): Unity-only numbers for the arena flow's view (Arena36View.cs; CLAUDE.md rule 5: each with a comment).
// The flow's timings are gameplay (design.n36.level36); these are only how it is drawn.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const uint Arena36RedHex = 0xFF3A14; // the locked door's and the spawn bubble's red-orange (CLAUDE.md rule 8: red-orange = enemies and danger; the same hex as Tell36RedHex so "danger" is one colour)
        public const float Arena36Glow = 2.2f; // HDR multiplier on that colour (the bloom picks it up; Tell36FlashPeak's range)
        public const float Arena36BubbleR = 1.6f; // m: the spawn bubble's starting radius (a body and a half: it wraps a husk, then shrinks to nothing over the event's seconds)
        public const float Arena36BubbleAlpha = 0.55f; // the bubble's additive strength at its start (fades with its size)
        public const float Arena36LightRange = 9f; // m: the spawn light's starting range (ULTRAKILL's shrinks at 50 per second: SpawnEffect.cs, rea-ultrakill-combat.md section 4; ours over the bubble's life)
        public const float Arena36LightIntensity = 4f; // the spawn light's intensity at its start (URP point light units, as the lamps of CampaignLighting24)
        public const float Arena36DoorAlpha = 0.35f; // the locked door slab's additive strength (the doorway stays readable through it)
        public const float Arena36DoorLightRange = 6f; // m: the red light on a locked door, so the floor in front of it reads red too
        public const float Arena36DoorLightIntensity = 2.5f; // that light's intensity
    }
}
