// NIGHT72 builder N (goal72, Goal72View.cs): the lesson line's glyph resolver, public for the goal72 view (the Ui instance and ResolveLesson25 are private). A line as a FORMAT:
// {jump} {slide} ... placeholders become the live key (keyboard) or the live pad glyph (pad), with the words when the atlas has no sprite for it: the input's own glyph, never a
// keyboard word on a pad (NIGHT72 D-9).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        public string ResolveGoal72(string raw, string rawPad) => ResolveLesson25(raw, rawPad);
    }
}
