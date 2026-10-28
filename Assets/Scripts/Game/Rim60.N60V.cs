// NIGHT60 builder V (B7 rim56's trims; design.n56.rim56, OFF in the repo). View code only: nothing here reaches the sim.
// THE CAUSE (Tools/night/n60-v-rimpath.py on NIGHT58's r1 a357 pairs, Logs/n60/V/rimpath-n58-r1.txt): decor55's string course (the "Decor55 band",
// 2.70-3.02 m up every wall face) is coloured by Decor55.Recolour, which never calls Colour56.Clamp, so NIGHT58's accent dim (Rim58.AccentMax)
// reached decor54's pieces but never the band: 85-97 % of the accent-hue pixels the same on and off, the env p90 V unchanged (0.729 / 0.729).
// Planted (the unreached accent x0.45 on the ON frames): bodies over the env p90 75 / 98 -> 87 / 98.
// THE FIX (behind rim56): the band's largest channel x UnityTuning.Rim60BandScale while rim56 is on (its hue and saturation kept).
// Off: -skygrindNoRim56 (the row), -skygrindOff58 (NIGHT57's rim56) and -skygrindOff60 (NIGHT59's rim56) leave the band as it was.
// B7 attempt 2 (the same switches): the lit side's gain Rim56 writes is UnityTuning.Rim60LitGain in place of NIGHT57's 2.205, which washed the
// brightest bodies under the environment's saturation (Tools/night/n60-v-rimgain.py; Rim56.LateUpdate's one hook line).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Rim60
    {
        static int off60 = -1;

        /// <summary>-skygrindOff60 or lower (or the process switch): NIGHT59's rim56 exactly.</summary>
        public static bool Off60()
        {
            if (NightRows.ForceOffFrom <= 60) return true;
            if (off60 < 0)
            {
                try { off60 = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 60 ? 1 : 0; }
                catch (System.Exception) { off60 = 0; }
            }
            return off60 == 1;
        }

        /// <summary>The band takes rim56's accent dim: rim56 on (its row, not -skygrindNoRim56) and neither -skygrindOff58 nor -skygrindOff60.</summary>
        public static bool On(GameConfig cfg) => LitOn(Rim56.On(cfg));

        /// <summary>NIGHT63 V (leftover 3, T-146; the w60 review's S2): the rule given rim56's own read of this frame (Rim56.LateUpdate's `on`),
        /// so the row is read once a frame, not again through Rim56.On: rim56 on and neither -skygrindOff60 nor -skygrindOff58.</summary>
        public static bool LitOn(bool rim56On) => rim56On && !Off60() && !Rim58.Off58();

        /// <summary>For the [decor55] line: the band's colour was scaled this map.</summary>
        public static bool BandScaled;

        /// <summary>The string course's colour: while `on`, its largest channel x UnityTuning.Rim60BandScale (hue, saturation and alpha kept);
        /// else as it came.</summary>
        public static Color Band(Color c, bool on)
        {
            if (!on) return c;
            float a = c.a;
            c *= UnityTuning.Rim60BandScale;
            c.a = a;
            return c;
        }

        /// <summary>NIGHT60 V (B7 attempt 2): the lit side's gain Rim56 writes: while `on`, UnityTuning.Rim60LitGain in place of NIGHT57's
        /// (Rim56LitGain x Rim57Gain, 2.205: it washed the brightest bodies under the environment's saturation, Tools/night/n60-v-rimgain.py);
        /// else `night59` as it came.</summary>
        public static float LitGain(float night59, bool on) => on ? UnityTuning.Rim60LitGain : night59;
    }
}
