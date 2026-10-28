// NIGHT63 builder G (slate B2): Unity-only numbers (CLAUDE.md rule 5): the boss63 reel's cue measure (BossCue63.cs, FeelReel.N63G.cs), one
// comment each. The sim never reads them.
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The boss views' own root GameObjects by bossRise id (each view's `new GameObject(...)` root: Wick37View, Brawl40View,
        /// WyrmView + WyrmRing43View + Wyrm47BeatsView, Loom37View, Crown37View): the cue measure's roots (n63-b2: the 3 m sphere found none
        /// for the wyrm and the crown at the reveal). The sphere stays as the fallback.</summary>
        public static readonly Dictionary<string, string[]> Boss63CueRootNames = new Dictionary<string, string[]>
        {
            { "wick", new[] { "NIGHT37 THE WICK" } },
            { "brawl40", new[] { "NIGHT40 THE ICEBREAKER" } },
            { "wyrm", new[] { "KILN WYRM", "NIGHT43 THE RING", "WYRM47 BEATS" } },
            { "loom37", new[] { "NIGHT37 THE LOOM" } },
            { "crown37", new[] { "NIGHT37 CROWN" } },
        };

        // --- B2's fix: the danger column (BossTell63.cs; design.n63.bossTell63) ---

        /// <summary>The danger column's level at the wind-up's first step (it rises to 1 at the hit): bright enough to read at once (the meter's
        /// Boss63CueRiseAbs at BossTell63Gain), so the cue's lead is the whole wind-up (config's tells 0.55-1.5 s, over the slate's 300 ms).</summary>
        public const float BossTell63From = 0.45f;

        /// <summary>The column's colour at level 1: Tell36RedHex (the danger red-orange, 0xFF3A14) times this, drawn additively (the views'
        /// own glow path: FxDraw.Additive on the neon material, a MaterialPropertyBlock _BaseColor).</summary>
        public const float BossTell63Gain = 0.6f;

        /// <summary>B2d: a pale glow the late pull takes on a listed attack must be HDR-hot (its drawn level, the max channel, at least this: a lit
        /// surface's colour stays under 1; the crown's clapper at its fire is Crown47WhiteHex x 2.4) and tinted warm at least this saturation (a
        /// pure white has no hue to call warm).</summary>
        public const float BossTell63HotLevel = 1.0f, BossTell63WarmMinSat = 0.05f;

        /// <summary>Attacks never given the column, as "boss:attack": the wyrm's punch window (aggro, aggroLean: its head goes yellow,
        /// WyrmView.cs:125; the rule line says to swing the gauntlet at its charging head) and its roar (the core opening, not an attack).
        /// A parryable attack (the sim's tell kind yellow) is skipped by its kind.</summary>
        public static readonly string[] BossTell63Skip = { "wyrm:aggro", "wyrm:aggroLean", "wyrm:roar" };

        /// <summary>m: the column's radius and height per boss (its body and reach on screen from the boss room's spawn: a 2 % frame change at
        /// 25 m needs a column about 8 m across; NIGHT63.md rule 8).</summary>
        public static readonly Dictionary<string, float[]> BossTell63Size = new Dictionary<string, float[]>
        {
            { "wick", new[] { 3.5f, 8f } },
            { "brawl40", new[] { 3.5f, 7f } },
            { "wyrm", new[] { 3f, 6f } },
            { "loom37", new[] { 5f, 10f } },
            { "crown37", new[] { 5f, 10f } },
        };

        /// <summary>The column's sides (an open double-sided tube).</summary>
        public const int BossTell63Segments = 32;

        /// <summary>ms: the slate's B2 floor: an attack whose visible cue starts less than this before its hit is short (doneWhen 2).</summary>
        public const double Boss63CueLeadMinMs = 300;

        /// <summary>m: renderers and lights within this of the boss's centre are measured (the boss's body and its arena telegraphs: lanes,
        /// geysers, rings, marked spots).</summary>
        public const float Boss63CueRadiusM = 30f;

        /// <summary>m: on the hold's first live frame, the transform roots of the drawn renderers this near the boss's centre or weak point are
        /// the boss's own; only things under them are measured (no player FX near the boss).</summary>
        public const float Boss63CueCoreM = 3f;

        /// <summary>m: renderers and lights this near the camera are the player's (the viewmodel, its flashes): not measured.</summary>
        public const float Boss63CueNearCamM = 3f;

        /// <summary>Frames between two scans for the boss's renderers and lights (inactive ones included, so a pooled telegraph is known before
        /// it is switched on).</summary>
        public const int Boss63CueRescanFrames = 30;

        /// <summary>s of frames kept for the analysis (the longest wind-up, 1.5 s in config, plus the reference window, with slack).</summary>
        public const double Boss63CueBufferS = 4.5; // NIGHT63 G B2b: was 3.0; the reference now reaches Boss63CueRefLongMs (2.2 s) before a wind-up of up to 1.6 s

        /// <summary>ms before the wind-up's first step that make an object's reference (its maximum drawn level per hue class there).</summary>
        public const double Boss63CueRefMs = 250;

        /// <summary>ms: NIGHT63 G B2b: the reference reaches back to the previous attack's hit, at most this far (and never less than
        /// Boss63CueRefMs): long enough to hold one cycle of a boss's ambient glow (the wyrm's molten seams: Wyrm47WaveRate 3 rad/s, 2.1 s), so
        /// a wave's peak inside a wind-up is not read as the cue (n63-b4's spit: "obj KILN WYRM/trim 9").</summary>
        public const double Boss63CueRefLongMs = 2200;

        /// <summary>ms of slack before the wind-up's first step where the search starts and the reference ends (one frame at 60 fps).</summary>
        public const double Boss63CueEarlyMs = 17;

        /// <summary>The smallest rise in drawn level (max channel of the drawn colour) that counts as a cue.</summary>
        public const float Boss63CueRiseAbs = 0.15f;

        /// <summary>The rise must also be at least this share of the object's reference level (a bright lamp must brighten visibly).</summary>
        public const float Boss63CueRiseShare = 0.3f;

        /// <summary>A drawn colour below this level (max channel) is too dark to read: no hue class.</summary>
        public const float Boss63CueMinLevel = 0.08f;

        /// <summary>Saturation under this reads as pale (white, the boundary's violet-white), not as a hue.</summary>
        public const float Boss63CueMinSat = 0.5f;

        // The danger and parry hue bands are NOT typed here (review S3, Logs/n63/review-n63.md): the meter, the tell's pull and the fix-list tool
        // read the colour table's design.n63.colours63.numbers.bands (BossCue63.Use / Bands; Tools/night/n63-b2-fixlist.cjs), one set with C1.
    }
}
