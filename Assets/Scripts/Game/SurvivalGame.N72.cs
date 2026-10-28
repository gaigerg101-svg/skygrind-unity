// GG 2026-10-26: FingerprintBaseline.Ultra71 (the ULTRAKILL profile's baseline, set at boot by MovementTuning71) is static. Leaving Play mode
// does not reload the domain, so the flag is cleared when the game goes: EditMode tests after a Play session judge the web's movement again.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        void OnDestroy()
        {
            FingerprintBaseline.Ultra71 = false;
        }
    }
}
