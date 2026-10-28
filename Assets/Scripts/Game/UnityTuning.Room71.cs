// NIGHT71 builder B (leftover L1 room71: Room71Runner.cs). Unity-only (a play-build tool's waits, no gameplay number; DOORFREE's own hold time
// is the sim's, design.n29).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>`-skygrindStartRoom`: seconds after boot before the campaign run starts on the booted map (DoorFree29Check's 1.5 s: the
        /// title's views built, StartGame's ToTitle done).</summary>
        public const float Room71BootWaitS = 1.5f;
        /// <summary>The longest the runner holds DOORFREE for the sim to read it on (the hold is 3 s, design.n29; DoorFree29Check holds 3.6 s):
        /// past it the player is moved anyway and the line says "NOT on".</summary>
        public const float Room71HoldCapS = 4.5f;
    }
}
