// NIGHT41 builder J2 (design.n41.bars41 and design.n41.entrance41, view rows: no sim bit). CLAUDE.md rule 5: Unity-only numbers (render / UI
// layout / the entrance beat's on-screen timing), each with a comment. The bar's thresholds and the stagger meter are the bosses' own numbers
// (design.n37.boss37.legAt / a boss row's kit.legAt, design.n40.brawl40.phase2At, a kit boss's Bar().segment): nothing gameplay is typed here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- bars41: UiBossBar41 (the boss bar with its phase ticks and the stagger meter under it), top-centre where the HUD33 bar sits ---
        public const float Bars41TickW = 3f; // px: a phase tick's width (at 1080p; thick enough to read at a glance over the red-orange fill)
        public const float Bars41TickOver = 5f; // px a tick stands above and below the bar (the bar is Hud33BossBarH = 6 px: the tick reads as a notch)
        public const float Bars41TickAlpha = 0.95f; // a tick still ahead (the phase to come): bone, nearly opaque
        public const float Bars41TickPassedAlpha = 0.3f; // a tick the HP has passed (that phase is done): dimmed, kept so the bar still shows the shape of the fight
        public const float Bars41PipPx = 8f; // px: a leg pip's side (THE LOOM / THE CROWN: their phases are legs, not HP marks)
        public const float Bars41PipGap = 6f; // px between leg pips
        public const float Bars41PipLift = 6f; // px the pips stand above the bar's top (level with the name's line, at the bar's right end)
        public const float Bars41StaggerGap = 10f; // px between the HP bar's bottom and the stagger meter's top (n41-r1 frames: 6 px read as one bar with an outline)
        public const float Bars41LabelPx = 16f; // px: the meter's name left of it (rule 8's floor; a second read, smaller than the boss's name)
        public const float Bars41LabelAlpha = 0.7f; // the STAGGER label's bone (OPEN is full cyan)
        public const float Bars41LabelGap = 10f; // px between the label's right edge and the meter's left end
        public const float Bars41LabelW = 160f; // px: the label's box (one word, right-aligned)
        public const string Bars41StaggerText = "STAGGER"; // the meter left to empty (bone)
        public const string Bars41OpenText = "OPEN"; // the window / THE ICEBREAKER's stuck fist counting down (cyan): hit it now
        public const float Bars41StaggerH = 5f; // px: the stagger meter's height (thinner than the HP bar: it is the second read)
        public const float Bars41StaggerTrackAlpha = 0.22f; // the meter's empty track (bone)
        public const float Bars41StaggerFillAlpha = 0.85f; // the meter's fill (bone; CYAN while the window is open and counts down)
        public const float Bars41DrainFlashS = 0.18f; // s: a stagger drain flashes the meter white this long (the hit landed on the right thing)
        public const float Bars41SlideIn = 0.2f; // s: the bar slides / fades in when a fight starts (the HUD33 bar's 200 ms)
        public const float Bars41Slide = 12f; // px it slides down from (the HUD33 bar's 12 px)
        public const double Bars41FillQuant = 500; // the fill is rounded to 1/500 so the Image is only touched on a visible change

        // --- entrance41: the boss entrance beat (the name card, juice38's shake + flicker, the rule line), one 1.5 s beat in the same order ---
        public const double Entrance41Beat = 1.5; // s: the whole beat, from the rise to the card gone (NIGHT41.md J2 / INVENTORY C4)
        public const double Entrance41CardIn = 0.12; // s: the name flickers in (juice38's card language, three frames at 60 fps)
        public const double Entrance41CardOut = 0.25; // s: the card and the rule line fade out at the end of the beat
        public const double Entrance41LineAt = 0.45; // s: the rule line comes in under the name (after the 0.3 s flicker, while the shake fades)
        public const float Entrance41LinePx = 24f; // px: the rule line's size (rule 8: >= 16 px at 1080p; under the 56 px name)
    }
}
