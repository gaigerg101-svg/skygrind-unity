// NIGHT41 builder P3 (aim41; INVENTORY C2 / C10): the lock brackets on a boss's weak point (Game/Ui.cs Brackets). Unity-only (UI layout);
// aim41's gameplay numbers are in design.n41.aim41 (reachM, acquireScale).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The brackets' half-height (m, projected like the chest's 1.1 m) round a live weak point the pad's assist pulls to (the
        /// knee, the fist, the core, the ring's opening): a weak point is a small thing, so the box is too, framing the spot the crosshair sits
        /// on instead of the whole body (the chest's box at a boss's centre would enclose the wrong part).</summary>
        public const double Aim41BracketHalfM = 0.5;

        // --- C10: the brackets' corners stay off arc40's band (UnityTuning.Hud40: outer radius 60 px, 10 px thick, +-52 deg off straight down) ---
        // A bracket corner sits at (+-0.35 h, -0.5 h) from the box's centre (Ui.cs: corners at +-43..50 of a 100-unit box, x 0.7 wide), 35 deg off
        // straight down (inside the arc's +-52 deg) at 0.61 h from the centre: with the crosshair on the target, a box h = 70-115 px tall puts both
        // lower corners on the arc (a husk's chest box at about 16-25 m, a weak point's 0.5 m box at about 7-11 m: THE LOOM's knees at 7-17 m).
        /// <summary>A lower corner's distance from the box's centre as a share of the box's height: sqrt(0.35^2 + 0.5^2).</summary>
        public const float Aim41CornerShare = 0.61f;
        /// <summary>The ring (px at 1080p from the reticle's centre) a lower corner (its 14 px arms and 1.5 px glow included) must not touch: arc40's
        /// inner edge (50 px) less 10 px of arm and gap, its outer edge (60 px) plus the 1.5 px stroke, the 3 px ready-flash grow and 8 px of arm.</summary>
        public const float Aim41CornerKeepIn = 40f, Aim41CornerKeepOut = 72f;
    }
}
