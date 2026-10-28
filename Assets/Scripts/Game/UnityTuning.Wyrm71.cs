// NIGHT71 builder B (slate B3 wyrm71; the boss63 reel's stand, FeelReel.Stand71.cs / Stand71Rig.cs). Unity-only: a reel's camera for the
// measurement frames, no gameplay number (the row's own numbers are design.n71.wyrm71.numbers).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The boss63 reel's `-skygrindStand71 &lt;m&gt;` range (m from the boss's centre): outside it the option is refused (said) and the
        /// reel frames from the player's own eye as before. 5 m keeps the camera out of THE KILN WYRM's 1.5 m half-width head; 40 m is past the
        /// 29.4 m the kiln boss room's spawn gave (Logs/n63/verdict-B1.md:73), so any stand the slate names (12-15 m) is inside.</summary>
        public const float Stand71MinM = 5f, Stand71MaxM = 40f;
    }
}
