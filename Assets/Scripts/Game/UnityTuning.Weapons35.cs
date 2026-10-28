// NIGHT35 builder W: Unity-only view numbers of the W rows (charge35's reads in the hands and on the rail, pickup35's models). CLAUDE.md rule 5: render /
// layout numbers live in UnityTuning, each with a comment. First values are picks from the n34 frames' scale (the gun ~0.3 m, the rail's 16 px labels);
// the lead's round-2 frames tune them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- charge35 in the hands (Viewmodel.Charge35.cs) ----
        public const float Charge35TipR = 0.022f; // m (rig space): the cyan tip's radius at a full charge (it grows from 0 with the level)
        public const float Charge35TipFullPulse = 0.25f; // x the tip's radius: the full charge's pulse depth
        public const float Charge35PulseHz = 9f; // Hz: the full charge's pulse (tip and sleeve), fast enough to read as "ready"
        public const float Charge35SleeveShare = 0.55f; // share of the gun's length (from the muzzle back) the barrel glow covers
        public const float Charge35SleeveW = 0.034f; // m (rig space): the barrel glow's width / height (a shell a little wider than the barrel)
        public const float Charge35SleeveGain = 1.6f; // x colors.usable: the barrel glow's brightness at a full charge (additive)
        public const float Charge35TipGain = 3.2f; // x colors.usable: the tip's brightness at a full charge (blooms)
        public const float Charge35GemWhite = 0.7f; // the gem's lerp to white at a full charge
        public const float Charge35ShakeM = 0.0022f; // m: the gun's charge shake at a full charge (x / y), none with reduced motion
        public const float Charge35ShakeHz = 31f; // Hz: the shake's base frequency (two incommensurate sines: no Random in view code)
        public const float Charge35BackM = 0.012f; // m: the gun draws back toward the eye as it charges (the "wind-up" pose)
        public const float Charge35PitchDeg = 2.0f; // deg: and tips its muzzle up a little
        public const float Charge35FireFlash = 0.12f; // s: the tip's white flash after the charged shot

        // ---- charge35 on the ARSENAL RAIL (UiRail34.Charge35.cs) ----
        public const string Charge35RailCharging = "CHARGING"; // the LT label's text while a charge builds (the label over the rail; 16 px, cyan)
        public const string Charge35RailFull = "CHARGED"; // the LT label's text at a full charge (white: release to fire)
        public const float Charge35RailPulseScale = 0.12f; // x the hand icon's size: the full charge's pulse on the rail

        // ---- pickup35 (Pickup35View.cs, PropsView29, WallBuys) ----
        public const float Pickup35GunLength = 1.3f; // m: a weapon pickup's model is fit to this length (n35-r2 frames: 0.85 m read small and dark at 3 m; x1.5)
        public const float Pickup35RimM = 0.02f; // m: the cyan rim's shell outside the model (an inverted hull, additive; n35-r2: 12 mm read as a thin outline only)
        public const float Pickup35RimGain = 3.0f; // x colors.usable: the rim's brightness (n35-r2: 1.4 was lost against a dark wall)
        public const float Pickup35LockedRimGain = 0.45f; // x colors.pale (the boundary's violet-white is not used): a locked silhouette's dim outline
        public const float Pickup35LockedBody = 0.03f; // the locked silhouette's body colour (near black: the shape, not the gun)
        public const float Pickup35NameNear = 14f; // m: the label shows within this distance (fully from NameNear - NameFade = 12 m: the lead's "16 px at 12 m")
        public const float Pickup35NameFade = 2f; // m: the label's fade band
        public const float Pickup35NameH = 0.95f; // m: the label's height over the pickup's centre (over the 1.3 m gun as it turns: never crossed)
        public const float Pickup35NameSize = 1.45f; // TextMeshPro world font size of the label (n35-r2: 2.4 was too big; 60 %: about 27 px tall at 3.2 m on 1080p)
        public const float Pickup35VialR = 0.17f; // m: the health vial's radius (n35-r2: 0.11 m sat inside the glow pillar and was not seen)
        public const float Pickup35VialH = 0.62f; // m: the health vial's glass height
        public const float Pickup35VialPulseHz = 1.1f; // Hz: the vial's slow pulse (a heartbeat-ish rate)
        public const float Pickup35VialPulse = 0.35f; // the pulse's brightness depth (0..1)
        public const float Pickup35NameScaleFrom = 5f; // m: past this the label grows with the distance (scale d / this), so it stays about 17 px tall out to 12 m
        public const float Pickup35RingR = 0.6f; // m: the floor ring under a pickup (in place of NIGHT29's glow pillar, which crossed the model and the label)
        public const float Pickup35RingW = 0.07f; // m: the ring's width
        public const float Pickup35RingGain = 1.6f; // x colors.usable: the ring's brightness
        public const float Pickup35LightRange = 2.2f; // m: the pickup's own small light (it lights the gun's body tones in a dark room)
        public const float Pickup35LightIntensity = 2.5f; // URP point light intensity
        public const float Pickup35LightUp = 0.7f; // m: the light sits this far over the model's centre
        public const float Pickup35VialGain = 3.2f; // x colors.usable: the vial's core brightness (blooms: the vial is the thing you see)
    }
}
