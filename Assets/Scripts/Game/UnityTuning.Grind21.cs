// NIGHT21 U3-35.7 THE GRIND (builder W): the mode's texts (Core/Endless21.cs is the rule; its numbers are config.json endless and
// design.n21.grind). The web's endless mode had a banner and a results line but no TITLE row; these are the port's.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The run setup's MODE value ("THE GRIND") and the info line's one-line pitch (at most 8 words).
        public const string GrindLabel = "THE GRIND", GrindInfo = "ENDLESS WAVES · THE CORE RESHAPES · DIE ONCE";
        // The WAVE banner the break raises (waveBreak): "WAVE 3".
        public const string GrindBannerFormat = "WAVE {0}";
        // RESULTS' line under the tally for a GRIND run: the wave reached, the records line, the best wave of this HEAT.
        public const string GrindResultsWave = "THE GRIND · WAVE ", GrindResultsBest = "BEST WAVE ";
    }
}
