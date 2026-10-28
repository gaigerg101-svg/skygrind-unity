// NIGHT29 round 2 (builder V): the authored props' and the hazard strips' view numbers (Game/PropsView29.cs). CLAUDE.md rule 5: each
// with a comment. Props' positions, sizes and yaw are the plan's rows (campaign.props); the hazard's timings are design.n27.hazardFloor27
// (the sim's own). Only look plumbing lives here. View only: no collider (colliders come from the plan's boxes).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float PropTopLift = 0.18f; // mix toward white on a stone prop's top: a lighter lid so a crate reads against the dark floor
        public const float PropCollar = 0.03f; // m: a pillar's neon collar stands this far proud of the shaft
        public const float PropCollarH = 0.12f; // m: its height
        public const float PropCollarDrop = 0.35f; // m: its top edge sits this far under the pillar's top
        public const float PropPanelH = 0.02f; // m: a console's lit panel's thickness on its top
        public const float PropPanelShare = 0.7f; // the panel's share of the console's top (each way)
        public const float PropNeonScale = 0.8f; // x the strip brightness for a prop's neon (props are dimmer than the trim)
        public const float PropFlatBrightness = 0.7f; // x the strip brightness for a flat prop (floor strip, pad marker, frost)
        public const float PickupSpinDegPerS = 90f; // deg/s: the pickup box turns (frame time)
        public const float PickupBob = 0.12f; // m: the pickup box bobs this far up and down ...
        public const float PickupBobHz = 0.8f; // ... at this rate
        public const float PickupFloat = 1.0f; // m: the plans centre a pickup about this far over its floor (k1 1.0, f6 0.9): the glow pillar stands from there
        public const float PickupGlowW = 0.28f; // m: the glow pillar's width over a weapon pickup
        public const float PickupGlowH = 3.2f; // m: and its height (readable from across the room)
        public const float PickupGlowHealthShare = 0.5f; // a health pickup's glow pillar is this share of the weapon's height
        public const float PickupBoxBrightness = 1.4f; // x the strip brightness: the rotating box is the brightest thing on the prop list
        public const float HazardLift = 0.03f; // m: the hazard strip's quad floats over the floor
        public const float HazardOff = 0.18f; // x the danger colour: the strip at rest (a faint warning mark, never off the map)
        public const float HazardTeleLow = 0.25f, HazardTeleHigh = 1.0f; // x the danger colour: the telegraph pulses between these
        public const float HazardPulseHzFrom = 2f, HazardPulseHzTo = 6f; // Hz: the telegraph's pulse quickens from the first to the last of its seconds
        public const float HazardLive = 1.8f; // x the danger colour (HDR, blooms): the strip is live and hurts
        public const float HazardReduced = 0.6f; // x the danger colour: the telegraph is steady with REDUCED MOTION
    }
}
