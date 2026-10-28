// NIGHT54 builder L (light54): the a154 reel's Unity-only numbers (FeelReel.N54.Light.cs; CLAUDE.md rule 5: each with a comment). Measuring
// tools, not the game: the game's light54 numbers are design.n54.light54.numbers.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int L54ReelW = 240, L54ReelH = 135; // px: the downsampled frame the reel reads each frame (1920x1080 halved three times: a box average, not a point sample)
        public const int L54ReelFrames = 180; // frames per pass (3 s at the reel's 60 fps clock)
        public const int L54ReelSettle = 45; // frames after a pass's switch flip before it is read (the eased lights land: landmarkEaseS 0.6 s is a time constant, slotFadeS 0.45 s)
        public const int L54ReelFireEvery = 6; // frames between the reel's fire presses (10 a second at 60 fps; the gun's own cooldown limits it further)
        public const float L54ReelU0 = 0.04f, L54ReelU1 = 0.40f, L54ReelV0 = 0.20f, L54ReelV1 = 0.80f; // the read region (share of the frame): the left middle, clear of the viewmodel (right), the reticle and the shot's line (centre), the boss bar (top) and the health (bottom left)
        public const float L54ReelVmU = 0.55f, L54ReelVmV = 0.55f; // the WIDE exposure read (u 0.04-0.96, v 0.15-0.85: clear of the HUD's bands) leaves out the viewmodel's box: right of u 0.55, below v 0.55 of the screen
        public const float L54ReelJump = 0.03f; // a FLICKER EVENT: the region's mean luminance moves more than 3 % from one frame to the next (a still camera)
        public const float L54ReelPixelDelta = 0.05f; // a pixel "changed": its luminance (0..1) moved more than this between two frames (13 of 255)
        public const float L54ReelWhite = 0.92f; // a pixel is near white over this luminance (the exposure read: the lead's a147 "flat cream near white")
        public const float L54ReelPassShare = 0.5f; // GREEN: the ON pass's flicker events at most this share of the OFF pass's at every spot (or both 0)
        public const float L54ReelWhiteShare = 0.6f; // GREEN at THE STACK: the ON pass's near-white share at most this share of the OFF pass's (or under L54ReelWhiteFloor)
        public const float L54ReelStackLumMax = 0.50f; // GREEN at THE STACK: the ON pass's WIDE mean luminance at most this (the lead, 15:12: n54-r1 read 0.668 OFF -> 0.645 ON; the un-graded base wanted ~0.40-0.45, G's grade adds EV -0.35 on top)
        public const float L54ReelWhiteFloor = 0.02f; // a near-white share under this counts as exposed (2 % of the region)
        public const int L54ReelMinShots = 4; // the input happened: at least this many shots per side (n54-r1: the KESTREL's cadence gave 8 in a side's 180 frames; its impacts are the pops' input, no kill or blast needed)
        public const int L54ReelEventsListed = 12; // flicker events listed per side with what was live on their frame (THE WICK's state, embers alive, a burn, a shot)
        public const double L54ReelWallMin = 4, L54ReelWallMax = 14; // m: the lit room's spot faces a wall this far away (impacts and their pops in the read region's light)
        public const double L54ReelWickWallUp = 3; // m over the shaft's floor where the STACK spot's aim meets the wall (beside the boss: its tells in frame, the shots on the wall)
        public const double L54ReelWickSideDeg = 35; // deg off the line to the core the STACK spot aims (the core near the frame's centre, the impacts on the wall)
    }
}
