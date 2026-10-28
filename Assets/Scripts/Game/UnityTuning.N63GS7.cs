// NIGHT63 builder G (leftover 6, S7: kiln's presses as machines; Press63Math.cs, Press63.cs; design.n63.press63): Unity-only view geometry and
// colours (CLAUDE.md rule 5), one comment each. The sim never reads them: the presses' boxes, their colliders and the nav stay the level's.
// S7b (after S7's GATE FAIL, Logs/n63/verdict-S7.md): a press's silhouette (bed, gap, ram head, crown) at the player's eye instead of a frame
// round a column; every part drawn UNLIT in steel tones so the hall's warm light can never turn it tan or lavender; no stripes.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>A campaign box whose job starts with this is a drop press (n57-kiln's THE PRESS HALL: "hero prop: a drop press (the
        /// charger's stun post)", Design/levels/n57/kiln.plan.json): the level's own word, not a shape guess.</summary>
        public const string Press63Job = "hero prop: a drop press";

        /// <summary>m: the bed (the heavy base the ram stamps onto): its height, and how far it stands out from the press's box (every part stands
        /// out less than config's body.capsuleRadius, 0.35 m: the eye never enters a view part of a box it collides with).</summary>
        public const double Press63BedH = 1.1, Press63BedOutM = 0.28;

        /// <summary>m: the throat's stand-off from the box: the dark back of the gap between the bed and the crown (it hides the old pillar's bands).</summary>
        public const double Press63ThroatOutM = 0.04;

        /// <summary>m: the four guide columns at the box's corners (bed to crown): their section and how far each stands out past its corner.</summary>
        public const double Press63UprightM = 0.3, Press63UprightOutM = 0.2;

        /// <summary>m: the crown (the press's heavy top): its bottom above the floor, its height, its stand-out (the widest part). At 4.4-5.8 m the
        /// whole machine sits in the view from THE PRESS HALL's entry (the nearest press about 9 m away).</summary>
        public const double Press63CrownAtM = 4.4, Press63CrownH = 1.4, Press63CrownOutM = 0.3;

        /// <summary>m: the stack (the cylinder housing from the crown to the roof): its stand-out, over the rods' so a rod's top hides behind it.</summary>
        public const double Press63StackOutM = 0.15;

        /// <summary>m: the ram head: its height and stand-out; the die (its bright working edge) at its bottom, its height and stand-out.</summary>
        public const double Press63HeadH = 1.0, Press63HeadOutM = 0.26, Press63DieH = 0.14, Press63DieOutM = 0.27;

        /// <summary>m: the head's lowest bottom above the bed's top (the stamp) and its highest top under the crown's bottom.</summary>
        public const double Press63HeadLowGapM = 0.03, Press63HeadHighGapM = 0.15;

        /// <summary>The rod's share of each face's width and its stand-out (m): in front of the throat, behind the head, the crown and the stack.</summary>
        public const double Press63RodShare = 0.4, Press63RodOutM = 0.12;

        /// <summary>s: the head's cycle on the FeelClock: a slow lift, a hold up top (the lamp lit: it is about to drop), a hard drop, a dwell
        /// on the bed; each press offset by Press63PhaseS so the hall stamps in turn.</summary>
        public const double Press63LiftS = 1.7, Press63HoldS = 0.8, Press63DropS = 0.22, Press63DwellS = 0.5, Press63PhaseS = 0.83;

        /// <summary>m: the lamp on the crown's face toward the hall's middle: its housing (width, height, depth past the crown) and its lens
        /// (width, height, depth past the housing): big enough to read from the entry (S7's 0.42 x 0.26 m lens was a blob).</summary>
        public const double Press63LampW = 0.9, Press63LampH = 0.7, Press63LampD = 0.03, Press63LensW = 0.72, Press63LensH = 0.5, Press63LensD = 0.015;

        /// <summary>Colours (sRGB hex, each drawn unlit: what shows is the value set here, in steel blue-greys): per part the faces along z
        /// (front), along x (side), the top and the bottom. Values for the read at 15-25 m: the throat near-black, the rod and the guide columns
        /// bright metal, the head mid with a bright die, the bed dark with a lit top line, the crown dark-mid, the stack dark (the machine ends at
        /// the crown). No warm hue, no danger red-orange (a press does not hurt the player: CLAUDE.md rule 8), no parry yellow (DECIDE63 D-1),
        /// no cyan, no magenta: N63PressTests checks each.</summary>
        public const uint Press63BedFrontHex = 0x353B46, Press63BedSideHex = 0x272C35, Press63BedTopHex = 0x7C8592;
        public const uint Press63ThroatHex = 0x090A0D;
        public const uint Press63UprightFrontHex = 0xAEB6C2, Press63UprightSideHex = 0x6E7684;
        public const uint Press63CrownFrontHex = 0x434A57, Press63CrownSideHex = 0x323843, Press63CrownBottomHex = 0x5A6270;
        public const uint Press63StackHex = 0x16181E;
        public const uint Press63HeadFrontHex = 0x6E7787, Press63HeadSideHex = 0x515968, Press63HeadTopHex = 0x8A93A1;
        public const uint Press63DieFrontHex = 0xB4BCC8, Press63DieSideHex = 0x868F9D;
        public const uint Press63RodFrontHex = 0xCDD4DE, Press63RodSideHex = 0x939DAB;
        public const uint Press63HousingHex = 0x0E1014, Press63LensOnHex = 0xEAF2FF, Press63LensOffHex = 0x4A5260;
    }
}
