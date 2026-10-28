// EXT53 builder N (row N, RIDE KILL; design.n53.rideKill53): the HUD callout's layout and motion (UiRideKill53.cs) and the a253 reel's
// placement (FeelReel.N53.RideKill.cs). Unity-only numbers: the sim never reads them (golden vectors, replays and fingerprints untouched).
// Px at 1080p (the canvas scales with the height). The callout's colour is the theme's cyan (player-usable, CLAUDE.md rule 8).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- the callout (UiRideKill53.cs) ----
        public const string RideKill53Word = "RIDE KILL"; // the callout's word when the bar was already full (nothing paid back)
        // with a boost paid back: the payoff named on the same line (21 px at 1080p, over the 16 px floor), so it reads as the boost refund, not as
        // the web's style award "+20 WALLKILL" (Core/Style.cs: style points for any kill in the WALLRIDE state, shown in the style column)
        public const string RideKill53WordPaid = "RIDE KILL  <size=66%>+1 BOOST</size>";
        public const float RideKill53Px = 32f; // px: the type scale's S32 (DISPLAY: the theme's display face), over the 16 px floor
        public const float RideKill53Y = 150f; // px over the reticle: above the points pops (34 / 70 px) and the kill pop's shards (out to 110 px), under the power banner (250 px)
        public const float RideKill53BoxW = 420f, RideKill53BoxH = 48f; // px: the text's box (one line, no wrap)
        public const float RideKill53Punch = 1.45f; // x the scale on the kill's frame, back to x1 over RideKill53PunchS (an overshoot: the HUD hit, KillPop53's grammar)
        public const float RideKill53PunchS = 0.16f; // s of HUD frame time (~10 frames at 60 fps)
        public const float RideKill53LifeS = 0.9f; // s the callout stays (a kill chain on the wall restarts it)
        public const float RideKill53FadeS = 0.25f; // s of the life's end it fades over
        public const float RideKill53HotS = 0.1f; // s it starts white-hot (the theme's CyanHot) before it cools to cyan

        // ---- the a253 reel (FeelReel.N53.RideKill.cs) ----
        public static readonly string[] RideKill53ReelRooms = { "g4", "a3" }; // the rooms tried first (n52-iceline's THE REACH: the harness's `n53ridekill ride` default; n52-foundry's a3 runner)
        public const double RideKill53ReelReach = 24; // m: the ride wall's search around the room's spawn (RideKill53Wall.Find)
        public const double RideKill53ReelLength = 14; // m of wall the run line needs (floor under it, the face hit every 2 m)
        public const double RideKill53ReelStandOff = 0.45; // m from the face to the feet at the start (capsule radius 0.4 + 0.05: touching on the first step)
        public const double RideKill53ReelUp = 1.2; // m over the floor the player is put (airborne: the ride's attach)
        public const double RideKill53ReelSpeed = 11; // m/s along the wall at the start (the hold ride's floor is 10.4)
        public const double RideKill53ReelLean = 0.5; // the stick's lean into the wall (never away: the ride's away rule)
        public const double RideKill53ReelAhead = 10; // m ahead along the wall the target husk stands (held there): ~9 deg down from the rider's eye, inside ride38's 15 deg pitch band (a steeper look sinks the hold ride; the harness's --shoot at 8 m lost the wall)
        public const double RideKill53ReelOut = 1.5; // m off the wall the target stands (clear of the rider's line)
        public const int RideKill53ReelMinPx = 150; // the a253 GREEN floor: the word's cyan pixels in its screen box over the frame before the shot (a 32 px "RIDE KILL" draws ~1000+; n53-r3's invisible word drew 0)
        public const int RideKill53ReelCalloutFrames = 6; // frames after the kill by which the callout must be up (alpha >= 0.5)
    }
}
