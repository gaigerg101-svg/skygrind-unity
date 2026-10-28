// NIGHT50 shared commit (Tools/night/n50-shared.cjs): builder H's Unity-only numbers for charge50 (the charge bar back under the reticle, G1)
// and gunglow50 (the charge on the gun, four steps, G2). Each number carries a comment; each gameplay number comes from config, never here.
// Colours come from UnityTuning.Look49* (the one palette, UnityTuning.Look49.cs); nothing here touches the sim.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- charge50 (G1): arc40 + meter42 back under hud47's reticle (UiHud38.Arc40.cs, UiHud38.cs Bar38, UiHud38.Meter42.cs) ----
        /// <summary>px at 1080p: the arc's OUTER edge from the reticle's centre with charge50 on (arc40's Hud40Radius 60 is NIGHT46's placement; 46 sits
        /// it closer under the reticle). Its deepest drawn pixel is meter42's second line with its under-stroke: 46 - 10 (band) - 2 x (3 gap + 3 line)
        /// - 1.5 (stroke) = 22.5 px, 4.5 px clear of the 18 px reticle box (hud33's ticks end 18 px out). The fallback (DECIDE50 D-5: the pair loses
        /// to the old placement) is this number set to Hud40Radius.</summary>
        public const float H50ArcRadius = 46f;
        /// <summary>The fill's alpha while a charge builds (the weapon's Look49 token; arc40's bone fill was 0.9 alpha at Hud38ChargingAlpha).</summary>
        public const float H50ChargeAlpha = 0.95f;
        /// <summary>A cooldown's refill (the weapon's token, quieter than a charge: arc40's Hud40DrainAlpha 0.6 on bone).</summary>
        public const float H50CoolAlpha = 0.55f;
        /// <summary>The BEND meter on the arc (the gun in hand has nothing to say): the token at this alpha (it is not the gun's charge).</summary>
        public const float H50BendAlpha = 0.8f;
        /// <summary>s: the BEND shows this long after its meter changes (charge49's H49BendShowS: the same rule), and throughout a bend.</summary>
        public const double H50BendShowS = 1.2;
        /// <summary>The reel's bars (design.n50.charge50 has no numbers block): nearest drawn pixel from the centre, px at 1080p (NIGHT50.md G1).</summary>
        public const float H50MinCentrePx = 18f;
        /// <summary>The reel's bars: the band's height, px at 1080p (NIGHT50.md G1).</summary>
        public const float H50MinHeightPx = 4f;

        // ---- gunglow50 (G2): the charge on the viewmodel in four steps (Viewmodel.Charge35.cs) ----
        /// <summary>Steps along the barrel: one segment lights at each quarter (25 / 50 / 75 / 100 %; ULTRAKILL's Railcannon capacitors).</summary>
        public const int G50Steps = 4;
        /// <summary>m (rig space): each segment's square cross-section (charge35's sleeve is Charge35SleeveW 0.034; a hair wider so it reads over the body).</summary>
        public const float G50SegW = 0.038f;
        /// <summary>Share of the barrel (muzzle back toward the body's middle) the four segments span.</summary>
        public const float G50Span = 0.9f;
        /// <summary>Share of each segment's slot left dark between segments (the steps read as separate lights).</summary>
        public const float G50GapShare = 0.3f;
        /// <summary>The lit segment's colour gain over the weapon's Look49 token (additive on the viewmodel layer: blooms like charge35's sleeve).</summary>
        public const float G50Gain = 2.2f;
        /// <summary>The flash's colour gain (white) on the 2 frames at full.</summary>
        public const float G50FlashGain = 3.0f;
        /// <summary>Rendered frames of the white flash when the charge reaches 100 % (NIGHT50.md G2: one 2-frame flash).</summary>
        public const int G50FlashFrames = 2;
        /// <summary>The reel's settle frames before a gunglow50 / charge50 grab.</summary>
        public const int H50ReelSettleFrames = 6;
        /// <summary>px at 1080p: the reel's pair crop round the screen centre (charge49's H49ReelCropPx, the same crop for the blind pair).</summary>
        public const int H50ReelCropPx = 240;
    }
}
