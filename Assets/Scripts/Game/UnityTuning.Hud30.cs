// NIGHT30 builder V (design.n30.hud30; DESIGN30 F + I): the weapon-state widget's and the HUD layout pass's Unity-only numbers (CLAUDE.md
// rule 5: each with a comment). Px are at 1080p (the HUD canvas scales from 1920 x 1080). No gameplay number lives here: every cooldown, pip
// count and charge level the widget draws is read from the sim (Loadout, PulseCharge27, ChargeShots27, Lockout29) at render time.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the weapon-state widget (HudWeapons30.cs): one slot per owned weapon, the gauntlet as two (PUNCH, BLAST) -------------------------
        public const float Hud30RingOuter = 28f; // px: the ring's outer radius (DESIGN30 F: a 56 px ring)
        public const float Hud30RingBand = 4f; // px: the ring's band width (inner diameter 48 px: the icon fits inside it)
        public const float Hud30IconW = 42f, Hud30IconH = 21f; // px: the gun's silhouette inside the ring (2:1 like the hotbar's sprite; its diagonal 47 px fits the 48 px hole)
        public const float Hud30SlotPitch = 90f; // px between slot centres: the longest word ("SWITCHING") fits its own slot at 16 px, so two words never touch
        public const float Hud30WordBox = 86f; // px: the word's box under the ring (auto-sized 18 -> 16 px inside it; rule 8: never under 16 px)
        public const float Hud30WordGap = 2f, Hud30WordH = 22f; // px: from the ring's bottom to the word's box, and the box's height
        public const float Hud30Raise = 8f; // px: the slot in hand stands this much higher (DESIGN30 F)
        public const float Hud30HandBarW = 26f, Hud30HandBarH = 3f; // px: the cyan bar under the slot in hand (a shape cue beside the raise)
        public const float Hud30RowPad = 4f; // px: above the raised slot inside the card
        public const int Hud30MaxSlots = 6; // slots shown at most (a window round the slot in hand, like the hotbar's), the gauntlet's two included
        public const int Hud30RingSteps = 64; // the ring mesh's steps round a whole turn
        public const float Hud30TickDeg = 5f; // degrees: each CHARGING tick (the BLAST slot's five pip separators, the charge shot's quarters)
        public const int Hud30ChargeTicks = 4; // ticks on a charge shot's filling ring (quarters: a shape cue, not a number)

        // States (shape + word + colour; colour-blind safe: the word and the ring's shape carry the state alone)
        public const float Hud30CoolIconAlpha = 0.4f; // COOLING: the icon at 40 % (DESIGN30 F)
        public const float Hud30PipsIconAlpha = 0.4f; // the BLAST slot while its pips fill (it cannot act yet): dim like COOLING
        public const double Hud30MinCoolSeconds = 0.6; // s: a cooldown shorter than this (a pistol's 0.2 s shot cycle) never shows COOLING or a READY turn (no flicker per shot)
        public const double Hud30SecondsOver = 1.0; // s: COOLING shows its seconds left while more than this remains (DESIGN30 F: "seconds left when over 1 s")
        public const double Hud30ReadyWordSeconds = 1.0; // s: the word READY shows this long after a slot turns ready
        public const double Hud30ReadyFlash = 0.15; // s: the READY flash on the slot's disc (through game.flashGate: at most 3 a second, x FLASH INTENSITY)
        public const int Hud30GlowFrames = 1; // drawn frames: the one-frame glow ring on a READY turn
        public const float Hud30GlowGrow = 6f; // px: the glow ring stands this far outside the ring
        public const string Hud30ReadyEvent = "weaponReady"; // the FeelProbe / Hud30.WeaponReady event name for GG's sound (PORT-STATUS event list)
        public const string Hud30WordReady = "READY", Hud30WordCharging = "CHARGING", Hud30WordSwitching = "SWITCHING", Hud30WordPunch = "PUNCH", Hud30WordBlast = "BLAST";

        // --- P0 e: the style meter's multiplier chip ("x1.2 AIR") clear of the rank plate --------------------------------------------------------
        // The rank plate reaches Hud17StylePlatePad (14 px) left of the letter box and leans ThemeSlantDeg (12 deg) over its ~100 px height (~21 px);
        // NIGHT29's chip stood 10 px from the box, so it lay over the plate's slanted edge (shot 170227). With hud30 it stands this far left of the box.
        public const float Hud30ChipClearX = 48f;
        public const int LevelSel30SpanPlayable = 4; // round 4: the level select's playable slots while design.n30.span is on (LEVEL 4 THE SPAN joins levels 1-3; NIGHT26's config says 3)
        public const int LevelSel32Level5Playable = 5; // NIGHT32 F: the level select's playable slots while design.n32.level5 is on (LEVEL 5 THE SPIRE joins levels 1-4); off or -skygrindOff32: the 4 above
        public const float Hud30ChipDockGap = 4f; // px: round 4: the chip docks as a tab this far under the rank plate's bottom edge, right-aligned with it (nothing floats)

        // --- THE GLACIER's patience gauge under Hud29's gap bar (design.n30.glacier30; GG: "target you in a meaningful way") ------------------------------
        public const float Hud30GaugeGap = 6f; // px between the gap bar's plate and the gauge's plate
        public const float Hud30GaugeH = 46f, Hud30GaugeBarH = 10f; // px: the gauge's plate and its bar
        public const string Hud30GaugeRest = "SHARDS: STAY CLOSE"; // the gauge drains: you are in its reach
        public const string Hud30GaugeFar = "TOO FAR: SHARDS COMING"; // filling: past its reach (Glacier30.Far)
        public const string Hud30GaugeHidden = "OUT OF SIGHT: SHARDS COMING"; // filling: out of its sight (Glacier30.Hidden)
        public const string Hud30GaugeVolley = "SHARDS INCOMING: DASH SIDEWAYS"; // the volley's tell (DESIGN30 C2: dash sideways to dodge)
        public const string Hud30GaugeSweep = "SWEEP: CHANGE LANE"; // the frost sweep's tell (B's world band marks the lane)

        // --- P0 e: the layout check (Hud30Layout.cs) ------------------------------------------------------------------------------------------
        public const float Hud30LayoutMinArea = 24f; // px squared (Unity screen px): a smaller shared area is a touching edge, not an overlap (HudOverlap25's number)
        public const float Hud30LayoutMinAlpha = 0.3f; // a text or icon fainter than this is not counted (fading in or out)
    }
}
