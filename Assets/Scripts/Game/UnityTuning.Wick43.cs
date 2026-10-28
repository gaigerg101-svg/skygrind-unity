// NIGHT43 builder B1 (wick43; NIGHT43.md card B1 G15 / G10 / G16, DECIDE43 D-7): Unity-only numbers for how THE WICK's reveal, THE STACK's door
// and the leg beats are DRAWN (Wick37View.N43.cs, Juice38's wick43 entrance, Doors38View's boss door). The sim never reads these (its numbers are
// design.n43.wick43). Rule 8: the door open is cyan (usable) and locked red-orange like every door (the violet-white 0xCFC6FF read pink: G10);
// the furnace's light beyond it and THE STACK's lights are a warm white (a light, not a surface colour).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int Wick43LetterboxSort = 4; // the letterbox's canvas order: under the dash tint (5), the impact flash (6) and the HUD (10): the name card, the bar and the reticle draw over it
        public const float Wick43LetterboxFirst = 0.105f; // each bar's share of the screen's height on the first reveal (two bars: 2.39:1 framing from 16:9 is 0.128; a little less, so the HUD's top and bottom rows stay clear)
        public const float Wick43LetterboxRetry = 0.06f; // each bar's share on a retry's 1 s reveal (a beat, not a scene)
        public const float Wick43LetterboxIn = 0.22f; // the reveal's share over which the bars slide in (eased out)
        public const float Wick43LetterboxOut = 0.3f; // the reveal's last share over which they slide away (eased in), so the frame ends on the player's own view
        public const float Wick43LetterboxAlpha = 0.92f; // the bars' opacity (near black: the world still faintly reads behind them)
        public const float Wick43LightsDip = 0.3f; // THE STACK's lights drop to this share at the reveal's start (bosses-web C.11: "lights drop to 40 % for 0.3 s")
        public const float Wick43LightsFrom = 0.12f, Wick43LightsSpan = 0.55f; // then come back one by one, bottom to top: light i of n lights fully on at reveal share From + Span x i / (n - 1)
        public const float Wick43LightsRamp = 0.12f; // the reveal share each light takes to come up
        public const uint Wick43StackLightHex = 0xffd2a8; // THE STACK's lights with wick43: a warm furnace white (NIGHT37's 0xd8d0ff violet-white read as the pink of G10)
        public const uint Wick43DoorBeyondHex = 0xffb070; // the light just inside THE STACK behind its open door: the furnace's warm glow seen through the curtain (the boss's language)
        public const uint Wick43SignHex = 0xfff0e0; // the door's sign (THE STACK's name): warm white text (was the violet-white)
        public const float Wick43DoorFrameMul = 1.6f; // the boss door's frame (posts, top bar) this much wider than another door's: a heavier door
        public const float Wick43EntranceShakeMul = 0.6f; // the WICK's entrance shake x this with wick43 (the 0.12 s entrance hit-stop is dropped: G10's "brick wall"; the reveal frames it)
        public const float Wick43RecoilShakeM = 0.35f; // m: the cage's shudder at a leg beat (Wick37.recoil43), at its start, falling to 0
        public const float Wick43RecoilShakeHz = 23f; // the shudder's rate
        public const float Wick43CapFlareInner = 9.1f, Wick43CapFlareOuter = 12.9f; // m from the axis: leg 3's cap flare drawn on the catwalk (THE STACK's rim 9 to its ring wall 13)
        public const float Wick43CapFlareLift = 0.04f; // m over the catwalk
        public const float Wick43CapFlareGain = 1.3f; // its red-orange gain at full (the tell brightens toward it; live x 1.5)
        public const int Wick43CapFlareSegments = 20; // the arc's segments
        public const float Wick43FlinchS = 0.22f; // s: the cage's flinch at a hit that lands while OPEN (round 2, G16 "boss hits smoother and crisper"): a short kick, eased back
        public const float Wick43FlinchM = 0.55f; // m: how far the cage is kicked away from the camera at the flinch's start (falls as the square of what is left)
        public const float Wick43FlinchUpM = 0.18f; // m: and up
        public const float Wick43FlinchTiltDeg = 9f; // degrees the cage tilts back (and shivers) at the flinch's start
    }
}
