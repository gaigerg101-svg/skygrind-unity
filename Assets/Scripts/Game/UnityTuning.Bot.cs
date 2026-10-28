// NIGHT14 U3-27 (the shared types): agent A's Unity-only numbers, U3-27.5 / U3-27.6 (the AUTOPILOT's adapter and shopper, the farm): render / UI / file numbers only; gameplay numbers go in design.autopilot / design.farm. Each with a comment saying why
// the web has no value for it (CLAUDE.md rule 5).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The folder under Application.persistentDataPath that holds the bot's nav graph cache (BotNav: one binary file per
        // content hash). The web built its duel graph in a worker at every page load and kept nothing.
        public const string BotNavCacheFolder = "botnav";
    }
}
