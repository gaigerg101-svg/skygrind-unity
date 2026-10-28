// NIGHT20 A3 (builder P2): THE BEACON's look (Game/Beacon20View.cs; the rule is Core/Beacon20.cs, its numbers
// design.adds.beacon). Render-only numbers with no web counterpart (the web has no beacon).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The obelisk at the ring spot (m): its plinth, its column, the lamp on top.
        public const float BeaconPlinth = 1.0f, BeaconPlinthHeight = 0.15f;
        public const float BeaconWidth = 0.4f, BeaconHeight = 1.8f;
        public const float BeaconLamp = 0.3f;
        // The column of light over it (m): tall enough to find from across the map (the PROTOCOL's beam reads at 30 m), thin
        // while it waits, wider once lit (the elite is coming).
        public const float BeaconBeamHeight = 30f, BeaconBeamRadius = 0.3f, BeaconLitBeamRadius = 0.7f;
        // The trim's pulse while it waits to be lit (Hz), and its floor (x the colour).
        public const float BeaconPulseRate = 1.2f, BeaconPulseFloor = 0.45f;
        // The banners (the Ui's banner line) when it stands (`beaconStand`) and when it is lit (`beaconTake`; {0}: the round).
        public const string BeaconStandBanner = "A BEACON STANDS · LIGHT IT: AN ELITE COMES, A REWARD IF YOU WIN";
        public const string BeaconLitBanner = "BEACON LIT · ROUND {0} BRINGS AN ELITE";
    }
}
