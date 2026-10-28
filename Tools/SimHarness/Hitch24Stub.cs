// NIGHT25 (lead): the sim's StartRun carries Hitch24.Mark tags (Assets/Scripts/Game/Hitch24.cs uses UnityEngine's profiler, so it is
// not in this project): a no-op stand-in so SurvivalSim.Run.cs compiles here.
namespace Skygrind.Game
{
    public static class Hitch24
    {
        public static void Mark(string t) { }
    }
}
