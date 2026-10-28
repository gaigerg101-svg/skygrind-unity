// NIGHT42 builder U1 (view rows design.n42.meter42 / style42 / health42; NIGHT42.md U1; GG's G7 / G13): the numbers of the meter, the style meter and
// the health bar. CLAUDE.md rule 5: Unity-only numbers (render / UI layout / motion), each with a comment; px at 1080p (the HUD canvas scales with the
// height). No gameplay number lives here: the style ranks, thresholds and points stay PortExport/config.json's `style` (StyleMath), the health is the sim's.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- meter42: the arc's one language (UiHud38.Meter42.cs) -----------------------------------------------------------------------------------
        // GG G7: "needs to be polished and more accurate". CODE-FACTS G7: arc40's eased fill (level^1.6) showed 33 % at a half charge, the arc read FULL
        // (cyan) at rest and flashed on every tap's release, and the PULSE's swing cooldown was not drawn. meter42: the fill IS the charge (linear,
        // unrounded); cyan only at a real FULL; every cooldown REFILLS left to right in a dim bone; the empty track means "ready to charge".
        public const float Meter42ChargeAlpha = 0.95f; // the charge's fill (bone, bright: you are building it)
        public const float Meter42CoolAlpha = 0.42f; // a cooldown's refill (bone, dim: waiting, not yours yet); quieter than arc40's 0.6 drain so the two never read alike
        public const float Meter42TrackAlpha = 0.26f; // the empty track (ready to charge / the part not yet refilled); hud38's 0.28 track, a touch quieter under the dim refill
        public const float Meter42HeadDeg = 2.2f; // deg: the bright head at the fill's front while it moves (charging or refilling): the eye follows the edge, not the area
        public const float Meter42HeadGrow = 2f; // px: the head stands this far outside the band (it reads as a moving tick on the arc)
        public const float Meter42HeadAlpha = 1f; // the head's alpha (white on a charge, bone on a refill)
        public const double Meter42PingTime = 0.30; // s: a cooldown's end: the track flashes cyan and fades back to Meter42TrackAlpha (the "you can charge again" ping)
        public const float Meter42PingGrow = 2f; // px: the band's outward grow at the ping's first frame (shrinks with it)
        public const double Meter42BreatheHz = 3.0; // Hz: a FULL charge held breathes (alpha 0.75 .. 1) until it is released: the arc never sits still while a shot waits
        public const float Meter42BreatheLow = 0.75f; // the breathe's lowest alpha (cyan stays cyan)
        public const float Meter42FlashGrow = 4f; // px: the FULL flash's outward grow on its first frame (arc40's 3 px; one px more: FULL is the arc's loudest moment)
        public const float Meter42Line2Alpha = 0.7f; // the second line (the gun's NORMAL attack recovering: the PULSE's swing, the THUNDERCLAP's pump), bone
        public const double Meter42LineDoneTime = 0.18; // s: a line that completes flashes white this long, then disappears
        public const float Meter42MatchTol = 0.005f; // the shown fill vs the telemetry's `charge`: a frame is a MISMATCH past this (the reel's count; float noise is ~1e-7)

        // --- style42: the style meter, top-right (UiHud33.Style42.cs) --------------------------------------------------------------------------------
        // GG G13: "styule meter ... needs to feel aweosme to combo ... alot more movement and expression"; DECIDE42 D-6 (ULTRAKILL-like letters that punch
        // in, shake, decay; cyan / bone, red-orange on a hit). hud33 drew the letter and a 160 x 3 line that snapped (CODE-FACTS G13). Ranks stay the sim's six.
        public const float Style42LetterScale = 1.25f; // the letter's resting scale over hud33's 40 px (50 px: the rank is the corner's loudest glyph); pivot top-right
        public const double Style42PunchTime = 0.28; // s: a rank UP: the letter slams in from Style42PunchScale x to rest with an overshoot (OutBack)
        public const float Style42PunchScale = 1.9f; // x rest at the punch's first frame
        public const double Style42DropTime = 0.22; // s: a rank DOWN: the letter deflates in from Style42DropScale and falls Style42DropPx back to its place
        public const float Style42DropScale = 0.7f; // x rest at the drop's first frame
        public const float Style42DropPx = 10f; // px it falls from (above) on a drop
        public const double Style42ShakeTime = 0.32; // s: the shake after a rank up or a hit (decays as (1 - k)^2)
        public const float Style42ShakeUpPx = 5f; // px: the rank-up shake's amplitude
        public const float Style42ShakeHitPx = 8f; // px: the hit's shake (bigger: losing points should sting)
        public const double Style42ShakeHz = 23.0; // Hz: the shake's wobble (two incommensurate sines, x at this, y at 1.37x)
        public const double Style42FlashTime = 0.14; // s: a rank up flashes the letter white, then its rank colour
        public const double Style42HitTime = 0.40; // s: a hit taken turns the letter and the line red-orange (danger), back to their colour over this
        public const int Style42CyanFromRank = 2; // ranks from this index (B) draw cyan; D and C bone (the meter "lights up" as you climb)
        public const double Style42TopHz = 1.6; // Hz: the top rank (SS SKYGRIND) breathes (letter alpha 0.8 .. 1, the name's scale 1 .. 1.06)
        public const float Style42DecayDim = 0.45f; // the letter's alpha at an empty band (it dims as the line drains: the decay you can see before the drop)
        public const float Style42DecayFrom = 0.35f; // the band's share under which the letter starts to dim
        public const float Style42LineEase = 18f; // 1/s: the line eases toward the meter (a gain slides up in ~0.1 s; the drain follows the sim's linear decay)
        public const double Style42GainFlash = 0.12; // s: a gain flashes the line white
        public const double Style42FadeOut = 0.30; // s: the meter reaching 0: the letter shrinks and fades instead of vanishing
        public const float Style42ComboPx = 24f; // px: the combo count "x7" over the line's right end
        public const int Style42ComboMin = 2; // the count shows from this many awards in a chain
        public const int Style42ComboCyan = 10; // the count turns cyan from this many
        public const double Style42ComboPop = 0.16; // s: each new award pops the count from 1.45 x
        public const float Style42ComboPopScale = 1.45f; // x rest at the pop's first frame
        public const float Style42NamePx = 18f; // px: the rank's name under the line (DULL .. SKYGRIND; >= 16 px at 1080p)
        public const float Style42AwardPx = 16f; // px: the last award ("+30 AIRSHOT") under the name
        public const double Style42AwardSlide = 0.12; // s: the award slides in from Style42AwardSlidePx to the LEFT (n42-r2: from the right it crossed the column's edge)
        public const float Style42AwardSlidePx = 28f;
        public const double Style42AwardHold = 1.1; // s: the award stays, then fades over Style42AwardFade
        public const double Style42AwardFade = 0.3;
        public const float Style42RowGap = 4f; // px between the rows under the letter

        // --- health42: the health number and its ten ticks, bottom-left (UiHud33.Health42.cs) ------------------------------------------------------------
        // GG G13: "heathbar need to look better ... animated". hud33's number and ticks snapped (CODE-FACTS G13: red only on a 150 ms flash or <= 25 %).
        public const double Health42ChunkHold = 0.40; // s: a hit's lost ticks stay red-orange this long (the damage chunk: what you just lost reads before it goes)
        public const float Health42ChunkDrain = 0.9f; // share of the max per s: the chunk then drains down to the health (a 30 % hit's chunk empties in ~0.33 s)
        public const float Health42NumEase = 14f; // 1/s: the number counts toward the health (a hit counts down, a heal counts up; ~0.2 s to land)
        public const double Health42HitPunch = 0.20; // s: a hit punches the number from Health42HitScale x back to rest
        public const float Health42HitScale = 1.18f;
        public const double Health42ShakeTime = 0.26; // s: a hit shakes the number and the ticks (decays as (1 - k)^2)
        public const float Health42ShakePx = 6f; // px: the shake's amplitude
        public const double Health42ShakeHz = 27.0; // Hz: the shake's wobble
        public const double Health42HealTime = 0.55; // s: a heal: the ticks it filled glow cyan (the player's colour) and fade to bone; the number flashes cyan
        public const float Health42HealScale = 1.10f; // x rest at a heal's first frame (a softer pop than a hit's)
        public const double Health42LowHz = 2.0; // Hz: at or under hud33's 25 % the red-orange number and ticks pulse (alpha Health42LowAlpha .. 1)
        public const float Health42LowAlpha = 0.6f;
        public const float Health42Eps = 1e-4f; // a drop smaller than this share of the max is no hit
        public const float Health42HealMin = 0.02f; // a rise of at least this share of the max in one frame is a heal (glow); a slow regen only counts the number up
    }
}
