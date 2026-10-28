// NIGHT13 U3-26.9 (agent H: the HUD hotbar, the radial wheel, the weapon bar): the Unity-only layout numbers, each with
// a comment saying why the web has no value for it (hud.js's CSS sizes are cited where they are the web's). px at 1080p
// (the web's UI scale 1). The gameplay numbers (weapons.hotbarShown, weapons.radial, weapons.arsenal.staleAt, the
// bindings) come from config.json; the colours from config where config has them (colors.usable, ui.theme.*, the
// families), else the hud.js CSS literal cited here. TMP letter spacing is in 1/100 em (CSS 0.1em -> 10).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the weapon block's plate (hud.js .hud-plate / .hud-plate.w :25-28) -------------------------------------------
        // CSS padding 9px 18px 11px 16px, gap 7px between the rows (hotbar, name, meter, special); the cyan edge on the
        // right is ui.theme.edgeWidth, the plate colour hud.panelOpacity over ui.theme.panel (hud.js --plate).
        public const float HudWeaponPadRight = 18, HudWeaponPadBottom = 11, HudWeaponPadLeft = 16;
        // Unity-only: TMP boxes need a line a little taller than the CSS line-height (the descender room). The name row
        // (hud.js .hud-wrow, 40 px: the slot label over the name) is the name's 30 px box (Ui.cs) with the slot label's
        // box overlapping its empty top by NameToSlot.
        public const float HudWeaponSlotLine = 18, HudWeaponRowHeight = 44;
        public const int HudWeaponSlotSpacing = 12; // .hud-wslot letter-spacing 0.12em
        public const float HudSpecialLine = 20; // .hud-special: t-small on an 18 px line (.sk-key line-height 18px)
        public const float HudFaintAlpha = 0.16f; // theme.js --faint: rgba(ui.theme.text, 0.16) (the hint's divider)

        // --- the hotbar (hud.js .hud-hotbar :145-156) ---------------------------------------------------------------------
        public const float HudHotbarCellWidth = 46, HudHotbarCellHeight = 40, HudHotbarCellGap = 3; // .it 46 x 40, gap 3px
        public const float HudHotbarOnLine = 2; // .it.on box-shadow inset 0 -2px 0 var(--usable)
        public const float HudHotbarKeyX = 3, HudHotbarKeyLine = 17; // .k left 3px, top 0, line-height 17px
        public const float HudHotbarGlyphBottom = 6; // .it padding-bottom 6px (the glyph sits on it)
        public const float HudHotbarGlyphLine = 18; // Unity-only: the glyph's TMP box (t-small, 16 px, and its descender room)
        public const int HudHotbarGlyphSpacing = 2; // .ph letter-spacing 0.02em
        public const float HudHotbarFreshInset = 3, HudHotbarFreshBottom = 2, HudHotbarFreshHeight = 2; // .fr left/right 3px, bottom 2px, 2px tall
        public const float HudHotbarFreshBackAlpha = 0.15f; // .fr background rgba(255,255,255,0.15)

        // --- the weapon meter (hud.js .hud-wbar :163-166; heat, charge or reload under the name) --------------------------
        public const float HudMeterWidth = 160, HudMeterHeight = 4; // .hud-wbar 160 x 4
        public const uint HudMeterHotColor = 0xFF5A1C; // .hud-wbar.hot i #ff5a1c (red-orange: overheating is danger)
        public const uint HudMeterLockedColor = 0xFF2A1C; // .hud-wbar.locked i #ff2a1c
        public const double HudMeterDenyStep = 0.06; // hud-deny 0.12s steps(2): 4 px right every other 0.06 s while locked
        public const float HudMeterDenyShift = 4; // @keyframes hud-deny { 50% { translate: 4px 0 } }

        // --- the special hint (hud.js .hud-special :167-169) --------------------------------------------------------------
        public const int HudSpecialSpacing = 10; // letter-spacing 0.1em
        public const string HudSpecialRmb = "RMB"; // hud.js setInfo: the right mouse's label (a literal on the web)

        // --- the pad's labels (Unity-only: the web's hotbar and hint always show the keyboard; NIGHT13.md U3-26.9 asks for
        // the pad's). gamepad.js DEFAULT_PAD_LAYOUT (y nextWeapon, left prevWeapon, lt altFire, up special) in PAD_GLYPHS'
        // neutral letters; the PlayStation set in words (the atlases lack the shapes). U3-26.7's Glyphs replaces these
        // once it lands (a rebound pad button then shows its own glyph).
        public const string HudPadCycleNeutral = "Y NEXT · D-PAD LEFT BACK";
        public const string HudPadCyclePlayStation = "TRIANGLE NEXT · D-PAD LEFT BACK";
        public const string HudPadAltNeutral = "LT", HudPadAltPlayStation = "L2";

        // --- the radial (hud.js .hud-radial :88-95) -----------------------------------------------------------------------
        public const int HudRadialMax = 24; // hud.js radialMax: options drawn at most
        // .ring: a (radius x 2 + 80) px disc, radial-gradient(circle, transparent 30%, rgba(10,11,16,0.55) 31% .. 70%,
        // transparent 71%) over the farthest-corner radius (box / 2 x sqrt 2), clipped to the disc; the band's edges at the
        // middle of each 1 % ramp.
        public const float HudRadialRingPad = 40;
        public const float HudRadialRingInnerStop = 0.305f, HudRadialRingOuterStop = 0.705f;
        public const uint HudRadialRingColor = 0x0A0B10; // rgba(10,11,16, ...)
        public const float HudRadialRingAlpha = 0.55f;
        public const float HudRadialOptPadX = 8, HudRadialOptPadY = 3, HudRadialOptBorder = 2; // .opt padding 3px 8px, border-bottom 2px
        public const float HudRadialOptLine = 20; // Unity-only: the TMP line for t-body (17 px) in the option's box
        public const int HudRadialOptSpacing = 8; // .opt letter-spacing 0.08em
        public const float HudRadialHiScale = 1.15f; // .opt.hi transform scale(1.15)
        public const uint HudRadialHiText = 0x0C0D10; // .opt.hi color #0c0d10 on var(--usable)
        public const int HudRadialMidSpacing = 18; // .mid letter-spacing 0.18em
        public const float HudRadialMidWidth = 640, HudRadialMidLine = 32; // Unity-only: the middle's TMP box (t-h3, centred, never wrapped)
        // Unity-only (the web draws no pointer: NIGHT13.md U3-26.9 asks for one): the dot at the pointer's offset.
        public const float HudRadialDotSize = 10;
    }
}
