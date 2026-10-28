// NIGHT33 builder P (design.n33.shop33): the SHOP screen's layout and look numbers (CLAUDE.md rule 5: Unity-only, each with a comment). Every px is at
// 1080p (the shop's canvas scales with the height from 1920 x 1080). The language is builder U's (UiStyle33 / UnityTuning.Hud33.cs: no panel fills, the
// 20-degree slash, a 2 px shadow, 64 / 24 / 16 px type, bone-white numbers); only the shop's own placements live here, so a restyle is one file.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int Shop33SortOrder = 120; // over the menus and the RESULTS (their canvases sort lower), under nothing the shop needs
        public const float Shop33ScrimAlpha = 0.86f; // the full-screen dim behind the shop (UiStyle33.Shadow's colour): the RESULTS under it stay faintly visible, text reads on it
        public const float Shop33Side = 72f; // U1's side margin
        public const float Shop33Top = 54f; // U1's edge margin
        public const float Shop33HeadW = 900f; // the "SHOP" head's box (64 px)
        public const float Shop33SubY = 132f; // the sub line under the head (16 px), from the top
        public const float Shop33WalletW = 360f; // the wallet block's width (U1 rule 2: <= 360 px)
        public const float Shop33WalletLabelH = 22f; // "WALLET" (16 px)
        public const float Shop33WalletNumY = 80f; // the wallet number's top, from the top (64 px)
        public const float Shop33WalletNumH = 72f;
        public const float Shop33BankedY = 158f; // "+N BANKED" under the number (24 px), from the top
        public const float Shop33ColTop = 250f; // the slot columns' head, from the top
        public const float Shop33ColPitch = 600f; // one column's step to the right
        public const float Shop33ColW = 520f; // a column's width (name + its state on one line)
        public const float Shop33RowTop = 314f; // the first item row's top, from the top
        public const float Shop33RowPitch = 104f; // an item row's step down (name 24 px + line 16 px + air)
        public const float Shop33LineGap = 34f; // the 16 px effect line under the 24 px name
        public const float Shop33StateW = 200f; // the price / OWNED / EQUIPPED box, right-aligned in the row (24 px condensed numerals: "10,600" / "EQUIPPED")
        public const float Shop33StatePx = 24f; // the state's size: U1 tier 2 (round 2's 16 px in the condensed number face read under 16 px at 1080p: the lead, n33-r5)
        public const float Shop33RackTickW = 10f; // a rack slot's slash tick (filled cyan = an equipped upgrade there)
        public const float Shop33RackTickH = 24f;
        public const float Shop33RackTickPitch = 18f;
        public const float Shop33FocusTickW = 8f; // the focus marker: a cyan slash left of the focused row
        public const float Shop33FocusTickH = 58f;
        public const float Shop33FocusTickX = 26f; // the marker's left offset from the row
        public const float Shop33DimAlpha = 0.42f; // an item the wallet cannot buy yet (bone, faded: never red-orange, which is danger only)
        public const float Shop33EmptyTickAlpha = 0.3f; // an empty rack slot's tick
        public const float Shop33BottomRowY = 54f; // CONTINUE and the hint line, from the bottom
        public const float Shop33MessageY = 100f; // the message slot (24 px) above the hint line, from the bottom
        public const float Shop33ContinueW = 560f; // CONTINUE's box (24 px), bottom-right
        public const float Shop33ContinueH = 40f;
        public const float Shop33Fade = 0.18f; // s: the open fade (no pop-in, M9)
        public const float Shop33OpenGuard = 0.25f; // s: input ignored after opening (the A that pressed CONTINUE on the RESULTS must not buy)
        public const float Shop33RepeatDelay = 0.35f; // s: a held direction repeats after this ...
        public const float Shop33RepeatEvery = 0.15f; // s: ... every this
        public const float Shop33StickDead = 0.5f; // the stick's travel that counts as a direction
        public const float Shop33MessageSeconds = 2.5f; // s: a message (BOUGHT / NEED n MORE) stays this long
    }
}
