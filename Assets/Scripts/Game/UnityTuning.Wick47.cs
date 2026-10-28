// NIGHT47 builder W (wick47 / wickTop47 / view row wickLook47; W-cards.md section 1, 4): Unity-only numbers for how THE WICK's three phases are
// DRAWN (Wick37View.N47.cs). The sim never reads these (its numbers are design.n47.wick47 / climb47). Rule 8 + NIGHT47's palette: yellow = punch
// it / parry (the open core, its ring clock, the chevrons), red-orange = danger (the charge's lane, the geysers, the plate ring), cyan =
// player-usable (the glass's rim and its faint grid: you stand on it).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const uint Wick47GlassHex = 0x36e0ff; // the glass floor's tint: the ride trim's cyan (Stack37TrimHex), player-usable (V-2)
        public const float Wick47GlassGain = 0.22f; // the glass pane's additive brightness: faint (the slag's red-orange reads through it), never a solid sheet
        public const float Wick47GlassPulseGain = 0.6f; // the pulse that runs across the glass when it seals (it reads as a floor before you stand on it)
        public const float Wick47GlassPulseS = 0.8f; // seconds of that pulse after the seal
        public const float Wick47GlassRimGain = 1.4f; // the glass's cyan rim strip (a floor edge you can see from across the cap)
        public const float Wick47GlassRimW = 0.25f; // m: the rim strip's width
        public const float Wick47CoreOpenGain = 2.6f; // the open core's yellow (the brightest thing on the cap: "punch it")
        public const float Wick47CorePulseHz = 2f; // the open core pulses 2 Hz (W-cards.md: "pulses 2 Hz for 3.0 s")
        public const float Wick47RingClockMax = 2.2f; // the ring clock's radius over the cage radius at the open's start (it shrinks to 1.0 at the end: a readable countdown)
        public const float Wick47RingClockGain = 2.4f; // its yellow brightness
        public const int Wick47Chevrons = 3; // chevrons on the glass between the player and the open core (pointing at it)
        public const float Wick47ChevronGain = 2.6f; // their yellow brightness
        public const float Wick47ChevronSize = 1.4f; // m: each chevron's size (round 3: 0.7 m boxes did not read at 6 m)
        public const float Wick47ChevronArm = 0.3f; // the chevron's arm width as a share of its size (thick: reads at 6 m)
        public const float Wick47RingClockInner = 0.8f; // the ring clock's inner radius as a share of its outer (a thick band: 0.7 m at the clock's start)
        public const float Wick47LaneW = 1.6f; // m: the charge's lane stripe's width on the glass (the WICK's cage is 3.4 m wide; the stripe is its centre line, brightening)
        public const float Wick47LaneGain = 1.3f; // the lane's red-orange brightness at the tell's end
        public const float Wick47GeyserGain = 1.6f; // a vent's red-orange through its tell (brightening), flashing on the eruption
        public const float Wick47RingGain = 1.5f; // the plate ring's red-orange band
        public const float Wick47RingH = 0.5f; // m: the band's height (the sim's ring band is 0.9-1.4 m over the floor)
        public const int Wick47FinaleCracks = 10; // G9 (wickFinale47): the glass's crack lines radiating from under THE WICK at the finisher
        public const float Wick47FinaleS = 1.6f; // s (real time) the finale's cracks and the slag's flare last
        public const float Wick47FinaleCrackLen = 6f; // m: the longest crack line (they grow in the first third, then fade)
        public const float Wick47FinaleCrackW = 0.12f; // m: a crack line's width (reads at 10 m)
        public const uint Wick47FinaleCrackHex = 0xFFF2C0; // a yellow-white (the core's heat through the glass; not cyan: the glass is breaking)
        public const float Wick47FinaleCrackGain = 2.4f; // the cracks' additive brightness at the start
        public const float Wick47FinaleSlagFlare = 3f; // the slag light's extra x at the finale's start (decays with it)
        public const int Wick47Segments = 48; // the ring / rim meshes' segment count
    }
}
