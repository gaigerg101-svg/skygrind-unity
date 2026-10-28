// NIGHT61 builder V (slate C3, no yellow on the guns: Game/GunYellow61.N61V3.cs, view row design.n61.gunYellow61): Unity-only numbers (the
// viewmodel's colour rule), each with its reason. No gameplay number here: the colour a gun takes is the config's own colors.usable (the web's).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>gunYellow61: a gun family colour at or above this hue (deg) and at or under GunYellow61HueHi is in the yellow arc: yellow56's
        /// gunHueLo (weapons.families.fire #ff8c1a, hue 30, the TORCH: its plate renders ~45 under a level's key, inside the tells' 45-70 band).</summary>
        public const float GunYellow61HueLo = 28f;

        /// <summary>gunYellow61: the arc's high end (deg), yellow56's gunHueHi: it holds blast #ffe14a (50) and edge #ffb000 (41, the THUNDERCLAP);
        /// volt #b8ff2c (80, lime) stays outside it.</summary>
        public const float GunYellow61HueHi = 75f;

        /// <summary>gunYellow61: a family colour over this saturation in the arc is a yellow (yellow56's gunMinS; the tells band is S > 0.5).</summary>
        public const float GunYellow61MinS = 0.5f;
    }
}
