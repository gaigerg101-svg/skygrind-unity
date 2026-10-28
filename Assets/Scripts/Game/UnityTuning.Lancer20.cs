// NIGHT20 U3-34.9 C2 (builder E owns this file): the LANCER's look, lancer.js's render numbers as it draws them (GG's design,
// rebuilt as it is; CLAUDE.md rule 7). Visual only: the sim never reads them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>lancer.js: the barrel box's colour (0x3a3a40, a gunmetal grey: not a palette role, the web's literal).</summary>
        public const uint LancerBarrelHex = 0x3a3a40;
        /// <summary>lancer.js: the fired beam's colour (0xffe0c0, white-hot; DANGER's warm end) and its brightness x4.</summary>
        public const uint LancerViewBeamHex = 0xffe0c0;
        public const float LancerBeamGain = 4f;
        /// <summary>lancer.js: the aiming laser in colors.enemy (DANGER) x2.5; the visor x2.5 aiming, x1.2 otherwise.</summary>
        public const float LancerLaserGain = 2.5f, LancerVisorAim = 2.5f, LancerVisorIdle = 1.2f;
        /// <summary>lancer.js: the laser's radius 0.012 m, growing by 0.02 m over the aim; the beam 0.08 m, shrinking over 0.2 s.</summary>
        public const float LancerLaserRadius = 0.012f, LancerLaserGrow = 0.02f, LancerViewBeamRadius = 0.08f, LancerBeamTime = 0.2f;
        /// <summary>lancer.js: a blink lerps the body 0.7 toward white and stretches it (0.4, 1.3, 0.4).</summary>
        public const float LancerBlinkWhite = 0.7f, LancerBlinkXZ = 0.4f, LancerBlinkY = 1.3f;
    }
}
