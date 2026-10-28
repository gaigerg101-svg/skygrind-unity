// NIGHT40 builder S1 (cut40; GG 2026-10-13: "The sixth level should be deleted entirely, along with any mention of levels beyond it"; DECIDE40 D-4):
// the level select's Unity-only numbers while design.n40.cut40 is on (CLAUDE.md rule 5: the web has no level select).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int LevelSel40Slots = 6; // NIGHT40 S1 cut40: six slots (levels 1-5 and one LOCKED, unnamed slot: level 6 comes back rebuilt later); slots 7-10 gone. Off: design.n26.levelSelect.slots (10)
        public const int LevelSel40Playable = 5; // NIGHT40 S1 cut40: levels 1-5 playable (LEVEL 6 out of the campaign order, Curriculum25.N40Levels). Off: LevelSel35Playable (6)
    }
}
