// NIGHT73 builder K (kiln73, Game/Kiln73.cs): the Unity-only numbers of THE KILN WYRM's arena colours, each with its reason (CLAUDE.md rule 5). View only:
// nothing here touches the sim (the web has no boss grade at all). Every colour is a (r, g, b) triple in 0..1 as design.n54.grade54 / design.n55.grade55 write
// theirs; the ledger Research/2026-10-27-wave73/K/ledger.md has the measurements these answer (the n54 a254 pair: blue 57 -> 1 under grade54's wyrm palette).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- grade54, THE WYRM's three phases (design.n54.grade54.numbers.wyrm.p1..p3) as kiln73 ON writes them ----

        /// <summary>kiln73: the grade's colour filter, before BossGrade54.Push (x common.intensity 1.8 away from white): (0.98, 0.97, 1.0) pushes to (0.964, 0.946, 1.0),
        /// a 4-5 % dimming and no blue cut. The config's (0.95, 0.96, 0.78) pushed to (0.91, 0.93, 0.60): linear blue x 0.6 on every pixel of the arena.</summary>
        public static readonly float[] Kiln73Filter = { 0.98f, 0.97f, 1.0f };

        /// <summary>kiln73: grade54's split-toning shadows per phase, before the push. The push must not clamp a channel: the config's P1 (0.36, 0.34, 0.20) pushed to blue -0.04 -> 0
        /// and URP's SoftLight with a blend of 0 squares the channel (n54 a254: wall blue 57 -> 1). These push to about (0.42, 0.38, 0.62), (0.40, 0.35, 0.66), (0.39, 0.33, 0.68):
        /// an indigo-plum shadow (hue about 250, S 0.35-0.5; palette72's kiln walls are hue 272), all channels well inside 0..1.</summary>
        public static readonly float[] Kiln73Shadows =
        {
            0.456f, 0.433f, 0.567f, // P1
            0.444f, 0.417f, 0.589f, // P2
            0.439f, 0.406f, 0.600f, // P3
        };

        /// <summary>kiln73: grade54's split-toning highlights, before the push: neutral grey (SoftLight with 0.5 is the identity), so a lit tell keeps its hue. The config's sulphur
        /// highlights (0.54, 0.76, 0.30) pushed to blue 0.14 and then 0.0 in P3.</summary>
        public static readonly float[] Kiln73Highlights = { 0.5f, 0.5f, 0.5f };

        /// <summary>kiln73: the turn's wash (the colour filter swings to it at a turn's peak, x washMix), before the push: a pale violet-white, so a turn only dims the frame a little
        /// (THE CROWN's turn is common's darkening beat only). The config's (0.85, 0.95, 0.45) pushed to blue 0.0: a blue-less yellow flash.</summary>
        public static readonly float[] Kiln73Wash = { 0.92f, 0.92f, 1.0f };

        /// <summary>kiln73: grade54's saturation / contrast deltas (points, before x intensity 1.8). Saturation 0: a saturation lift pushes a sand floor (hue 36-42, S 0.25-0.27) over
        /// the danger meter's S 0.35 (the config's +4 / +10 / +12). Contrast 6 (the config's +16 / +24 / +32 became +29 .. +58 points: dark walls darker, a lit floor hard).</summary>
        public const float Kiln73Sat = 0f, Kiln73Contrast = 6f;

        /// <summary>kiln73: the per-turn saturation of THE WYRM's grade54 palette (Grade54Boss.turnSat; the common default is 35): 0, so a turn darkens and does not saturate.</summary>
        public const float Kiln73TurnSat = 0f;

        // ---- the light layers (both grades write them over RenderSettings: fog, ambient, key) ----

        /// <summary>kiln73: the ambient / key tints (both grades), a pale violet-white as THE CROWN's (0.88, 0.93, 1) / (0.9, 0.95, 1): the sun's blue is no longer x 0.52
        /// ((0.95, 0.95, 0.70) x (1, 0.90, 0.75)). Stone.shader lights with the theme's probe, so the key tint is the part that reaches the stone.</summary>
        public static readonly float[] Kiln73Ambient = { 0.92f, 0.91f, 1.0f };
        public static readonly float[] Kiln73Key = { 0.96f, 0.95f, 1.0f };

        /// <summary>kiln73: the fog colour a grade writes (a dark plum, not the config's umber (0.16, 0.14, 0.06) / (0.16, 0.09, 0.05)) and how far it mixes (the config's 0.45-0.66).</summary>
        public static readonly float[] Kiln73Fog = { 0.10f, 0.08f, 0.16f };
        public const float Kiln73FogMix = 0.25f;

        // ---- grade55, THE WYRM's three phases (design.n55.grade55.numbers.wyrm.p1..p3) ----

        /// <summary>kiln73: grade55's shadow tint (applied as it stands, no push): a mild plum, (0.46, 0.42, 0.58) (the config's amber-umber (0.62, 0.48, 0.34) was chosen
        /// "the yellow-red-orange theme kept through every phase": rule 8 says red-orange is the enemy's). With the balance below the tint's weight is about 0.4 on a dark pixel.</summary>
        public static readonly float[] Kiln73Split55 = { 0.46f, 0.42f, 0.58f };

        /// <summary>kiln73: grade55's turn colour (the shadows swing to it at a turn's peak): neutral grey, THE CROWN's (a turn is the darkening beat, not a colour swing over the tells).</summary>
        public static readonly float[] Kiln73Turn55 = { 0.5f, 0.5f, 0.5f };

        /// <summary>kiln73: grade55's split balance (the config's 30 / 28 / 24: the tint's weight is 1 - luma - balance, so a higher balance spares lit pixels): 40.</summary>
        public const float Kiln73Balance55 = 40f;

        /// <summary>kiln73: grade55's contrast delta (the config's +12 / +16 / +22) and saturation delta: small, none.</summary>
        public const float Kiln73Contrast55 = 4f, Kiln73Sat55 = 0f;

        // ---- the edge probe (read only; K2 was retired: kiln73 never touches the HUD's edges) ----

        /// <summary>kiln73: "[kiln73] edge probe" lines per session (one per change of the director's EXPOSED state while the arena is drawn): a flapping state cannot flood the log.</summary>
        public const int Kiln73EdgeProbeMax = 20;

        // ---- calm72 reworked (NIGHT73 K2, Game/Calm72.cs): the three red edges draw at any health on the thin frame; EXPOSED's pulse is calmer ----

        /// <summary>calm72 (K2): the share of the EXPOSED edge's pulse amplitude that is kept (Calm72Math.ExposedAlpha). Ui's is 0.35 + 0.35 sin(7 t): 0 .. 0.70, a full-alpha-to-nothing
        /// flicker at 1.1 Hz (GG 2026-10-26 "flickers"). 0.45 gives 0.35 +- 0.16 (0.19 .. 0.51): the mean is Ui's, the peak-to-trough range 0.32 (the lead's bar: at most half of 0.70 = 0.35;
        /// Ui rounds its alpha to hundredths, so a margin under one half).</summary>
        public const float Calm73ExposedSwingScale = 0.45f;

        /// <summary>calm72 (K2): the hurt flash's alpha rises by at least this in one frame at a hit (0 -> about 0.3 -> 0.6 over Hud18FlashAttack's three frames; its decay and the low-HP breath move
        /// by hundredths a frame), which is how a hit is told from a breath for the `[calm72] hit` line.</summary>
        public const float Calm73HitJump = 0.03f;

        /// <summary>calm72 (K3): the HUD search: at most this many searches per HUD (a lost HUD, Calm72.HudLost, starts the count again), this many unscaled seconds apart (the first build's
        /// literals, 40 and 0.5: 20 s of looking for an Image a HUD does not have, then none).</summary>
        public const int Calm73FindMax = 40;
        public const float Calm73FindEveryS = 0.5f;
    }
}
