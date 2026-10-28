// NIGHT56 builder L (L2): the a156 room meter's probe numbers (FeelReel.N56.Room.cs). Unity-only: a reel reads them, the sim never does (golden
// vectors, replays and fingerprints untouched). The waits are a452's (FeelReel.N52.Rooms.cs), so a156's entry frame is a452's entry frame.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const double L56BodyH = 1.0; // m above an enemy's feet: the body point the meter's frustum test and ray aim at (n56-room.py's +1.0 m)
        public const double L56HeadH = 1.6; // m above an enemy's feet: the head point, tried when the body is hidden (n56-room.py's +1.6 m)
        public const double L56RaySlack = 0.3; // m: a ray hit this close before the target counts as reaching it (the target point sits inside the enemy, not on a face)
        public const float L56RoomWaitS = 6f; // s of real time for the campaign to enter the room after the camera is placed (a452's 6 s)
        public const double L56WaveWaitS = 8; // s of sim time for the room's first wave to come up (a452's 8 s)
        public const double L56SettleS = 0.5; // s of sim time after the first live enemy before the frame (a452's 0.5 s: the rest of the wave out of its spawn tell)
    }
}
