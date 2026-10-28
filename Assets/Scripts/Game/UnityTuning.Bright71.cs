// GG 2026-10-26 (play, screenshots of the foundry corridors and the boss arenas): "a lot of the room especially the boss rooms are very dark".
// The causes, read from the code: the look33 theme ambient is 0.03-0.12 (UnityTuning.Look33ThemeAmbient*: unlit faces read black), the boss
// grades multiply that ambient down again and cut exposure by 0.1-0.45 EV (grade54 / grade55 per-boss `ev`), and the fog colour is near black.
// Bright71 (Game/Bright71.cs) is the floor: the last writer each frame, it lifts each ambient colour, the fog colour and the fog density to these
// floors, keeping each colour's hue (so a theme or a boss grade still tints the room). Render-only numbers (CLAUDE.md rule 5): view only, the sim
// never reads them. Off: -skygrindNoBright71.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Bright71AmbientSky = 0.42f; // the trilight top's brightest channel at least this (was 0.06-0.12): ceilings and upper walls read
        public const float Bright71AmbientEquator = 0.32f; // the trilight middle (walls facing sideways) at least this (was 0.04-0.08)
        public const float Bright71AmbientGround = 0.22f; // the trilight bottom (floors' undersides, faces pointing down) at least this (was 0.02-0.05)
        public const float Bright71BossAmbientSky = 0.62f; // the floors during a boss fight (x the boss grades' weight): the arenas' walls read (gamma)
        public const float Bright71BossAmbientEquator = 0.56f; // walls facing sideways in a boss arena (THE ICEBREAKER's pillars read black at 0.32)
        public const float Bright71BossAmbientGround = 0.40f; // faces pointing down in a boss arena
        public const float Bright71AmbientFlat = 0.34f;
        public const float Bright71AlbedoFloor = 0.34f; // a boss-arena stone colour's brightest channel at least this (gamma; THE ICEBREAKER's pillars 0x241f2e = 0.18 read black) // a Flat ambient (survival maps, Look.cs) at least this
        public const float Bright71FogColour = 0.18f; // the fog colour's brightest channel at least this: far walls fade to dusk violet, not black
        public const float Bright71FogDensityMax = 0.0035f; // fog density at most this (exp and exp2): far rooms stay readable
        public const float Bright71MaxGain = 8f; // a colour is never scaled up more than this (a black colour stays dark rather than turning grey)
        public const double Bright71GrowStep = 0.002; // m: a drawn stone box grows by this x (1 + its index mod GrowSteps): two faces on one plane end 2-6 mm apart (no z-fight)
        public const int Bright71GrowSteps = 3; // the growth's distinct steps (2, 4, 6 mm); every decor lift (20-80 mm) stays clear of the largest
        public const float Bright71SkyZenith = 0.36f; // the sky's top band, brightest channel at least this (gamma: 0.16 read ~0.02 linear, still black on n71-polish2)
        public const float Bright71SkyMid = 0.46f; // the sky's middle band (gamma)
        public const float Bright71SkyHorizon = 0.56f; // the sky's horizon band (gamma)
        public const float Bright71ExposureLift = 0.35f; // EV added to the base grade's post-exposure (PostFx): the ambient floor alone moved the frame's mean 2-8 % (n71-polish1)
        public const float Bright71BossEvScale = 0f; // the boss grades' per-boss exposure cut (`ev`) x this while Bright71 is on: their colour stays, the darkening goes
    }
}
