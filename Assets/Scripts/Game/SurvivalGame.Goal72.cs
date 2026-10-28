// NIGHT72 builder N (goal72, Goal72View.cs): the one member the self-hooked view needs from the game: a line's glyph placeholders resolved by the Ui's own resolver (the Ui instance is
// private to SurvivalGame). Before the Ui exists the line is returned with its placeholders as words.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        public string Goal72Resolve(string raw, string rawPad) => ui != null ? ui.ResolveGoal72(raw, rawPad) : Lesson25Text.Resolve(raw, false, a => a.ToUpperInvariant());
    }
}
