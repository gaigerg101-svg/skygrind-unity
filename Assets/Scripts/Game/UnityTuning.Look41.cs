// NIGHT41 K2 (design.n41.look41; INVENTORY A3; DECIDE41 D-4): the room light's colour by the level's theme. Until tonight every room key, ceiling
// panel, flood lamp and sconce was look33's warm 0xffe6c8 whatever the level (LightPlan33.Green names them "warm"), so level 2's ice tint read tan
// (NIGHT40 L2: the warm light kept ~0.9 / 0.7 / 0.5 of R / G / B). With look41 on, "warm" and the room key take the theme's KEY colour and the
// lights35 fill takes the theme's FILL colour. Rules (CLAUDE.md rule 8): never cyan (player-usable, colors.usable hue 186) and never red-orange
// (danger, colors.enemy hue 12) for room light; never magenta (RIVAL). Look41Tests checks the hue distances below.
// Unity-free plain numbers. Order = LightPlan33.Themes: foundry, iceline, kiln, span, spire. View only: nothing here touches the sim.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The room key per theme (ceiling panels, the key spot, open rooms' flood lamps, sconces, pool panels: every plan light named "warm").</summary>
        public static readonly uint[] Look41KeyHex =
        {
            0xffe6c8, // foundry: warm, look33's own tungsten white (unchanged: level 1 keeps the look GG has seen)
            0xc8dcff, // iceline: blue-white (hue 218, saturation 0.22: blue, not cyan's 186): R x0.78, B x1.28 of the warm key, so the ice tint reads blue
            0xffbe78, // kiln: molten amber (hue 33, saturation 0.53; the danger red-orange is hue 12 at 0.89: the lava and braziers stay the only orange)
            0xdcc4f4, // span: dusk violet-lilac (hue 270, saturation 0.20; not the boundary's 0xd8d0ff hue 250 / 0.18, not magenta's 300)
            0xfff0c0, // spire: high pale gold (hue 46, saturation 0.25)
        };

        /// <summary>The lights35 cool fill per theme (CampaignLighting24.Key35: a point light low at the room's far end; NIGHT35's Look35FillHex 0x6474ff).</summary>
        public static readonly uint[] Look41FillHex =
        {
            0x6474ff, // foundry: NIGHT35's blue-violet fill (unchanged)
            0xa4b4ec, // iceline: a pale frost blue (hue 227, saturation 0.31; not cyan)
            0x7060b0, // kiln: a dusky violet (the cool side against the molten key)
            0x8a6cd8, // span: deeper dusk violet under the lilac key
            0x8078d8, // spire: a soft violet under the pale gold key
        };

        /// <summary>Round 2b (the lead's n41-r2 frames: the floor's mean colour did NOT move with the keys above: L2 (150.1, 129.5, 119.3) before ->
        /// (150.0, 129.4, 119.5) after, L3 / L4 / L5 within 0.2): the near floor is lit by the directional dusk SUN (Look.Apply's key light,
        /// colors.lightSun 0xffbd8a: R 1.0, G 0.74, B 0.54, exactly the "0.9 / 0.7 / 0.5" NIGHT40 L2 measured), not by the room's spot or sconces.
        /// So the theme also tints the sun while a campaign level is built (CampaignLighting24.Sun41; config's colour comes back on its destroy).
        /// 0 = keep colors.lightSun. Each is scaled to about the config sun's luminance (0.76) so the exposure holds.</summary>
        public static readonly uint[] Look41SunHex =
        {
            0,        // foundry: the config's warm dusk sun (unchanged: level 1 as GG has seen it)
            0xbcc4d8, // iceline: a pale moon-white (hue 222, saturation 0.13; luminance 0.77): B / R of a lit surface x2.1 of the warm sun's
            0xffc070, // kiln: molten amber (hue 34, saturation 0.56; 21 deg off the danger red-orange, 8 deg more amber than the config sun's 26; B / R x0.81)
            0xc8b4e6, // span: dusk violet (hue 264, saturation 0.22; luminance 0.74; 36 deg off magenta): B / R x2.1
            0xd8c298, // spire: high pale gold (hue 39, saturation 0.30; luminance 0.77): paler and less orange than the config sun (B / R x1.3)
        };

        /// <summary>Look41Tests' rule: a room light is "not cyan / not red-orange / not magenta" when its hue is at least this far (degrees) from that
        /// colour's hue or its saturation is under Look41PaleSat (a near-white light tints no surface into the reserved colour).</summary>
        public const double Look41MinHueGap = 18;
        /// <summary>The saturation under which a light counts as near-white (no reserved hue can read from it).</summary>
        public const double Look41PaleSat = 0.15;

        // --- round 2: level 2's canal (Ice41View.cs; look41 on and the iceline theme): frost and ice sheen as VIEW props, no level data, no collider ---
        /// <summary>The frost's colour: a pale blue-white on the lit stone material (hue ~210, saturation ~0.13: bluer and less violet than the boundary's
        /// 0xd8d0ff hue 250; not cyan's 186 at 0.89). It takes the room's light like the floor does.</summary>
        public const uint Ice41FrostHex = 0xd4e4f4;
        /// <summary>m: a collision box of kind "wall" at least this tall gets a frost skirt round its base (a low kerb does not).</summary>
        public const double Ice41WallMinH = 1.5;
        /// <summary>m: the skirt is drawn in pieces this long along each face, each its own height (a ragged edge, not a ruled line).</summary>
        public const double Ice41FrostSeg = 1.1;
        /// <summary>m: a piece's height over the wall's base, from min to max (hashed per piece: the same every run; no RNG).</summary>
        public const double Ice41FrostMinH = 0.18, Ice41FrostMaxH = 0.7;
        /// <summary>m: how far a piece stands out of the wall face (a crust, thin enough that no one reads it as a step; no collider).</summary>
        public const double Ice41FrostOut = 0.03;
        /// <summary>The share of pieces drawn (the rest left bare: patchy frost, not a painted band).</summary>
        public const double Ice41FrostFill = 0.8;
        /// <summary>The sheen streaks' colour on the floors (additive: it lifts the floor toward blue-white where it lies).</summary>
        public const uint Ice41SheenHex = 0xc8dcff;
        /// <summary>The streaks' additive gain (the colour x this: a faint polish, under the bloom threshold 1.0 by far).</summary>
        public const float Ice41SheenGain = 0.10f;
        /// <summary>m: the streaks lie this far over their floor's top (no z-fight with the stone).</summary>
        public const double Ice41SheenLift = 0.012;
        /// <summary>m: one streak per this much of a floor's short side (at most Ice41SheenMax per floor box).</summary>
        public const double Ice41SheenSpacing = 4;
        public const int Ice41SheenMax = 6; // streaks per floor box at most
        /// <summary>m: a streak's width, from min to max (hashed).</summary>
        public const double Ice41SheenMinW = 0.3, Ice41SheenMaxW = 1.1;
        /// <summary>A streak's length as a share of its floor's long side, from min to max (hashed); it runs along the long side (the canal's way).</summary>
        public const double Ice41SheenMinLen = 0.35, Ice41SheenMaxLen = 0.8;
        /// <summary>The canal's frozen floor (campaign.surfaces kind "ice"): one whole glaze over it at this additive gain, plus streaks at Ice41GlazeStreakGain.</summary>
        public const float Ice41GlazeGain = 0.07f, Ice41GlazeStreakGain = 0.16f;
        /// <summary>The glaze's colour (a slightly deeper blue than the sheen: the channel reads as a strip of ice).</summary>
        public const uint Ice41GlazeHex = 0xbcd4ff;
    }
}
