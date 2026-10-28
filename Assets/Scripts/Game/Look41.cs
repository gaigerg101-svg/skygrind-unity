// NIGHT41 K2 (design.n41.look41; INVENTORY A3; DECIDE41 D-4): which colour a room light takes. The theme of a level is known at light-build time from
// its map JSON's name (LightPlan33.Theme: "n40-iceline" -> 1, "n41-kiln" -> 2; an n41 level id holds its levelFrom's theme word). With look41 off
// (the row, -skygrindOff41 or any older off switch: N41View.RowOn) the theme is -1 and every colour is NIGHT40's (Look33PanelWarmHex,
// Look35FillHex). Numbers: UnityTuning.Look41.cs. View only.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Look41
    {
        /// <summary>The look41 theme of a map (LightPlan33.Themes index) while the row is on; -1 when it is off or the map matches no theme.</summary>
        public static int ThemeFor(GameConfig cfg, Dictionary<string, object> json) => N41View.RowOn(cfg, "look41") ? LightPlan33.Theme(json) : -1;

        /// <summary>The room key ("warm") colour for a look41 theme; NIGHT33's warm white for -1.</summary>
        public static uint KeyHex(int theme) => theme >= 0 && theme < UnityTuning.Look41KeyHex.Length ? UnityTuning.Look41KeyHex[theme] : UnityTuning.Look33PanelWarmHex;

        /// <summary>The lights35 fill colour for a look41 theme; NIGHT35's blue-violet for -1.</summary>
        public static uint FillHex(int theme) => theme >= 0 && theme < UnityTuning.Look41FillHex.Length ? UnityTuning.Look41FillHex[theme] : UnityTuning.Look35FillHex;

        /// <summary>Round 2b: the dusk sun's colour for a look41 theme; 0 = keep colors.lightSun (foundry, and -1).</summary>
        public static uint SunHex(int theme) => theme >= 0 && theme < UnityTuning.Look41SunHex.Length ? UnityTuning.Look41SunHex[theme] : 0u;

        /// <summary>Hue (degrees 0-360) and HSV saturation (0-1) of a 0xRRGGBB colour (Look41Tests' reserved-colour check).</summary>
        public static void HueSat(uint hex, out double hue, out double sat)
        {
            double r = ((hex >> 16) & 255) / 255.0, g = ((hex >> 8) & 255) / 255.0, b = (hex & 255) / 255.0;
            double mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b)), d = mx - mn;
            hue = 0;
            if (d > 0)
            {
                if (mx == r) hue = 60 * (((g - b) / d) % 6);
                else if (mx == g) hue = 60 * ((b - r) / d + 2);
                else hue = 60 * ((r - g) / d + 4);
            }
            if (hue < 0) hue += 360;
            sat = mx > 0 ? d / mx : 0;
        }

        /// <summary>The shorter way round the hue circle between two hues (degrees).</summary>
        public static double HueGap(double a, double b)
        {
            double d = Math.Abs(a - b) % 360;
            return d > 180 ? 360 - d : d;
        }

        /// <summary>Is a room light clear of a reserved colour (UnityTuning.Look41MinHueGap / Look41PaleSat)?</summary>
        public static bool ClearOf(uint light, uint reserved)
        {
            HueSat(light, out double h, out double s);
            HueSat(reserved, out double rh, out _);
            return s < UnityTuning.Look41PaleSat || HueGap(h, rh) >= UnityTuning.Look41MinHueGap;
        }
    }
}
