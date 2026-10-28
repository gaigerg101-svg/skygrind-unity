// NIGHT43 builder F1 (design.n43.feel43, a view row: no sim bit; G19 the FUSION's picture, G20 gun hits, the slug, the TORCH, rumble):
// the render side's numbers (Game/Feel43.cs, Fusion43View.cs, GunFx.Torch43.cs). CLAUDE.md rule 5: Unity-only numbers, each with a comment.
// The FUSION's freeze, damage and stagger are W1's (design.n43.fusion43, the sim). Sources: Research/2026-10-16-wave43/research/combat-web.md
// (B6 trauma^2, B7 layer timing, B8 the three haptic words, C12 the flame, C13 the charged release) and wave 42's hitfeel-web.md.
// Times are real (frame-time) seconds: they play through a freeze.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the FUSION's colour (DECIDE43 D-3: a hot, whitish pink only in the burst; RIVAL keeps magenta 0xff2bd6 for bodies and trails) ------
        public const uint Fusion43PinkHex = 0xff7ac8; // the burst's pink: hotter and whiter than RIVAL's 0xff2bd6 (more red and green, less blue)
        public const uint Fusion43CoreHex = 0xffe8f4; // the core flash: near-white with a pink cast
        public const float Fusion43Glow = 2.4f; // HDR gain on the burst's sprites (bloom catches it; the additive band stays a colour, not a white-out)
        public const int Fusion43Pool = 160; // sprites alive at once across bursts (two bursts overlapping at 28 motes + 24 sparks + rings fit)
        public const int Fusion43SeedSalt = 0x46553433; // "FU43": the view's cosmetic dice (Mulberry32 from the run seed; render side)

        // --- the FUSION's timeline (s after the event; W1's freeze is ~0.22 s, a boss 0.28 s) ---------------------------------------------------
        public const double Fusion43ConvergeTime = 0.09; // the two forces meet: inside the freeze, ~5 frames at 60 fps (combat-web B7: flash +20 ms, particles +50 ms)
        public const double Fusion43CoreTime = 0.26; // the core flash fades over this after the meeting ((1 - u)^2)
        public const double Fusion43RingTime = 0.36; // the rings blow out over this after the meeting (ease-out cubic)
        public const double Fusion43SparkLife = 0.42; // the outward sparks' life
        public const double Fusion43Rest = 0.9; // by this every part of the burst is at rest (the juice returns to rest: game-feel rule 1)
        public const int Fusion43Motes = 14; // motes per side (cyan from the player's side, embers from the enemy's): 28 streaks pulled into one point
        public const float Fusion43MoteReach = 1.5f; // m from the hit point the motes start
        public const int Fusion43Sparks = 40; // pink / white sparks blown out at the meeting (n43-r1: 24 at 0.06 m read as nothing on ice)
        public const float Fusion43SparkSpeed = 14; // m/s
        public const float Fusion43SparkW = 0.12f, Fusion43MoteW = 0.11f; // m: the sparks' and the converging motes' streak width (n43-r1: 0.06 / 0.07 lost on ice)
        public const float Fusion43FrontOffset = 0.75f; // m the burst stands toward the camera from the hit point (W1 sends the AimPoint, inside the body: n43-r1 hid the burst behind it)
        public const float Fusion43CoreSize = 2.4f; // m the core flash grows to (from a quarter of it), a husk is 1.8 m tall
        public static readonly float[] Fusion43RingRadius = { 3.2f, 4.6f }; // m the two rings reach: a plain body, a big kind / boss
        public const float Fusion43RingMaxDeg = 14; // deg the ring may subtend from the eye (KillFeel42's 9 deg cap, larger: this is the night's biggest hit)
        public const float Fusion43CoreMaxDeg = 12; // deg half-angle the core flash may subtend (at gauntlet range, 2 m, a 2.4 m flash would fill ~60 deg of the view and hide the enemy)
        public const float Fusion43MoteReachEye = 0.4f; // the motes start at most this share of the eye's distance from the hit (the player's side never starts in the camera)
        public const float Fusion43BossScale = 1.35f; // x every size on a boss / big kind

        // --- the FUSION's camera and screen -----------------------------------------------------------------------------------------------------
        public const float Fusion43KickDeg = 3.2f; // deg the view pitches up at the hit (KillFeel42's gauntlet 1.6-3.4; a FUSION is above a gauntlet hit)
        public const float Fusion43RollDeg = 4.0f; // deg of roll at the hit, springing back with the kick (combat-web B6: in first person, rotate more than translate)
        public const double Fusion43KickTime = 0.34; // s the kick springs back over (ease-out cubic; longer than the freeze, so the frozen frames show it moving)
        public const float Fusion43Trauma = 0.6f; // trauma added on the freeze's end (combat-web B6: +0.6 for an empowered impact: 4x a 0.3 hit's shake)
        public const double Fusion43TraumaWaitMax = 0.4; // s at most the trauma waits for the freeze to end (a freeze refused by the budget: at once)
        public const double Fusion43Chroma = 1.4; // the chroma kick at the pop (post.js pulse scale; post.chromaHurt is the hurt's): the brief chromatic split

        // --- the word "FUSION" (U1's style language when it lands: one place to swap) -----------------------------------------------------------
        public const string Fusion43Word = "FUSION";
        public const float Fusion43WordPx = 75; // px at 1080p (the type scale's S75 step; >= 16 px, CLAUDE.md rule 8)
        public const float Fusion43WordFrom = 1.7f; // its scale on the first frame (punched in)
        public const double Fusion43WordPunch = 0.12; // s to settle to 1x (ease-out back)
        public const double Fusion43WordHold = 0.45; // s held at full alpha after the punch
        public const double Fusion43WordFade = 0.2; // s to fade
        public const int Fusion43WordSortingOrder = 11; // the screen layer's canvas: over the HUD's (10), the crush frame (7) and the impact flash (6)
        public const float Fusion43WordDx = 170, Fusion43WordDy = 150; // canvas px (1080 tall) from the hit to the word's left edge: above and beside the enemy, never over it
        public const float Fusion43WordOutline = 0.22f; // TMP outline width: a dark ink rim on the pink (reads on level 2's white ice)

        // --- the FUSION's screen layer (Fusion43Screen.cs; n43-r1: the world burst alone washed out on bright ice) ------------------------------------
        public const float Fusion43InkAlpha = 0.85f; // the ink twins' alpha (UiTheme.InkSolid): the value contrast behind every bright shape
        public const float Fusion43InkPx = 8; // px the ink twin is wider / longer than its bright shape
        public const int Fusion43ScreenStreams = 12; // converging streaks: half cyan (the player's, from below), half red-orange (the enemy's, from above)
        public const float Fusion43StreamFromMin = 300, Fusion43StreamFromMax = 560; // px from the hit the streams start
        public const float Fusion43StreamW = 12; // px
        public const int Fusion43ScreenShards = 20; // radial shards at the pop
        public const float Fusion43ShardLenMin = 160, Fusion43ShardLenMax = 380; // px
        public const float Fusion43ShardW = 18; // px at the pop, thinning to 30 %
        public const float Fusion43ShardTravel = 420; // px the shards' inner ends fly out
        public const double Fusion43ScreenPopTime = 0.34; // s the shards fly
        public const float Fusion43CorePx = 260; // px the core disc at the pop
        public const float Fusion43RingPx = 430; // px the ring's radius when blown out
        public const double Fusion43EdgeTime = 0.05; // s the border flash holds (~3 frames at 60)
        public const float Fusion43EdgeAlpha = 0.6f; // its alpha at the pop (x FLASH INTENSITY, through the flash gate)
        public const float Fusion43EdgePx = 90; // px deep from each screen edge

        // --- gun hits (G20): a white-hot star at the hit point and streak sparks, by weapon --------------------------------------------------
        public const int Gun43Pool = 384; // sprites alive at once (a THUNDERCLAP volley's pellets on 3 bodies + a slug + stars)
        public const double Gun43StarLife = 0.05; // s the hit's white-hot star lives (~3 frames at 60: hitfeel-web 5's 1 white frame, scaled to 120 Hz screens)
        public const double Gun43SparkLife = 0.22; // s a hit spark lives
        public const float Gun43SparkSpeed = 9; // m/s a hit spark flies out (half of it back along the shot's way, the rest a sphere)
        public const int Gun43SparksMax = 3; // hit stars per frame per weapon at most (a volley's pellets add sparks, not 8 stars)
        // per weapon: { sparks, star size m, spark width m }; unknown guns use "default"
        public static readonly float[] Gun43Revolver = { 7, 0.55f, 0.035f }; // the KESTREL: a clean crack
        public static readonly float[] Gun43Shotgun = { 4, 0.45f, 0.04f }; // the THUNDERCLAP: per pellet hit (a volley adds up)
        public static readonly float[] Gun43Nailgun = { 2, 0.25f, 0.025f }; // the nailgun: small, many
        public static readonly float[] Gun43Rail = { 12, 0.9f, 0.05f }; // the LONGWAVE: the biggest gun hit
        public static readonly float[] Gun43Torch = { 1, 0.0f, 0.03f }; // the TORCH's tick: an ember, no star (the flame is the read)
        public static readonly float[] Gun43Default = { 4, 0.4f, 0.035f };
        public const float Gun43Glow = 2.0f; // HDR gain on the gun sprites

        // --- the THUNDERCLAP's charged slug (G20) ---------------------------------------------------------------------------------------------
        public const float Slug43InkStep = 1.4f; // m between the ink puffs shadowing the round's line (multiply: value contrast on the slug's cyan wash)
        public const double Slug43HaloLife = 0.32; // s the dark halo behind the impact lives
        public const float Gun43InkDark = 0.6f; // how dark the gun reads' ink puffs go at their centre (Fx43Shade)
        public const float Slug43Width = 0.34f; // m the tracer's white-hot core is wide (WeaponFx's slug line ~0.06: a thick round)
        public const float Slug43SheathWidth = 0.9f; // m the cyan sheath around it
        public const double Slug43Life = 0.2; // s the tracer fades over ((1 - u)^2)
        public const int Slug43Sparks = 26; // sparks of the impact's burst
        public const float Slug43SparkSpeed = 14; // m/s
        public const float Slug43FlareSize = 2.6f; // m the impact's flare
        public const float Slug43RingRadius = 2.8f; // m the impact's ring
        public const double Slug43RingTime = 0.3; // s
        public const float Slug43Trauma = 0.3f; // combat-web B6 / C13: +0.3 on a charged shotgun
        public const int Slug43Chunks = 18; // the killed body's chunks thrown along the shot (enemy colour)
        public const float Slug43ChunkSpeed = 16; // m/s along the shot (+ a spread): the body thrown
        public const double Slug43ChunkLife = 0.55; // s
        public const double Slug43LightIntensity = 5.5, Slug43LightRange = 5, Slug43LightLife = 0.16; // the impact's light (LightPools.Flash: x LightScale there); n43-r1: range 8 m in cyan tinted the whole room teal, now 5 m and white-hot (it lights the target, not the room)
        public const float Slug43BlastMinR = 2f; // m: the blast's shock ring reaches at least this (W1's slug43 blastRadius when larger; the lead's "a 2 m shock ring")
        public const int Slug43Debris = 22; // debris streaks thrown up and out of the blast (the enemy colour when it killed, else stone grit)
        public const float Slug43DebrisSpeed = 9; // m/s
        public const uint Slug43GritHex = 0xb8b0c8; // the grit: a pale dusk stone (neutral: not cyan, not danger)
        public const float Burn43FlareLife = 0.25f; // s: the ignition flare (W1's burn43) lives
        public const float Burn43FlareSize = 1.4f; // m
        public const int Burn43Embers = 8; // embers rising off the body as it catches
        public const float Charge35TipMaxDeg43 = 4f; // deg: with feel43 on, the charged release's tip flash (Viewmodel.Charge35) never subtends more than this from the viewmodel camera
        public const float Charge35TipNear43 = 0.15f; // m: and is hidden when the muzzle swings closer than this to the eye (n43-r1: a full-screen cyan wash)

        // --- the TORCH (G20; round 2 after the lead's n43-r1 frames: "almost no visible flame") ----------------------------------------------------
        public const bool Torch43Warm = true; // the lead's n43-r1 ask: white-hot -> gold -> orange -> smoke (false: NIGHT20's cyan ladder; rule 8 note in GunFx.Torch43.cs)
        public const uint Torch43GoldHex = 0xffd27a, Torch43OrangeHex = 0xff8a2a, Torch43DeepHex = 0xd8401c; // the jet's colours past its white-hot heart
        public const int Torch43JetSegments = 18; // flame billboards down the jet (x2: a heart in each), redrawn every frame while it fires
        public const float Torch43JetFrom = 0.22f, Torch43JetTo = 1.9f; // m a jet puff is across at the muzzle / at the reach (the cone is 18 deg: 2.2 m wide at 7 m)
        public const double Torch43FlowHz = 2.6; // the puffs flow outward this many segment-lengths a second (the jet rolls, not a static cone)
        public const int Torch43Pool = 320; // the lick's / embers' own pool beside the shared spray's 256 (Spray20Pool)
        public const int Torch43Puffs = 7; // flame puffs bursting over the target per tick (0.1 s)
        public const float Torch43LickSpeed = 3.2f; // m/s they spread sideways and up over what the jet hits
        public const int Torch43Embers = 5; // ember streaks per tick off the target
        public const double Torch43PuffLife = 0.45; // s
        public const float Torch43PuffFrom = 0.5f, Torch43PuffTo = 1.7f; // m a lick puff grows from / to over its life
        public const int Torch43Haze = 2; // smoke puffs per tick rising off the target (multiply: they darken, Fx43Shade)
        public const double Torch43HazeLife = 1.1; // s
        public const float Torch43HazeFrom = 0.7f, Torch43HazeTo = 2.6f; // m
        public const float Torch43Rise = 1.4f; // m/s the smoke rises
        public const int Torch43SmokePool = 48; // smoke puffs alive (and the jet's smoke tail's share)
        public const uint Torch43SmokeHex = 0x2a2236; // the smoke's darkest tint (the dusk ink)
        public const float Torch43SmokeDark = 0.55f; // how far toward it the darkest puff's centre goes (value contrast on level 2's ice)
        public const double Torch43RampTime = 0.2; // s of a hold over which the flame grows from Torch43RampFrom to full (combat-web C12 d)
        public const double Torch43RampFrom = 0.5;
        public const double Torch43HoldGap = 0.18; // s without a tick ends a hold (the tick is 0.1 s): the jet stops at most ~2 ticks after the trigger
        public const double Torch43LightIntensity = 4.0, Torch43LightRange = 8, Torch43LightLife = 0.16; // the flame's light (re-lit each tick)
        public const double Torch43LightFlicker = 0.2; // +- share of the light's intensity per tick (combat-web C12 a: 0.8-1.2x)
        public const double Torch43LightAt = 0.8; // share of the reach the light sits at (on the target)

        // --- a boss hit while OPEN (sim.N43BossState.open, or the WICK's core) -----------------------------------------------------------------
        public const float BossOpen43StarSize = 2.2f; // m the white-hot star
        public static readonly float[] BossOpen43RingRadius = { 3.0f, 4.4f }; // m the double cyan ring
        public const float BossOpen43Trauma = 0.45f;
        public const double BossOpen43Chroma = 0.6;
        public const double BossOpen43Gap = 0.15; // s at least between two OPEN reads (a nailgun stream reads once, not 12 times a second)
        public const double BossOpen43HitAge = 0.05; // s a player's hit position is used for the next bossHit (the WICK's payload has no position)

        // --- rumble words (combat-web B8: [low (strong motor), high (weak motor), ms] triples; Rumble x RUMBLE; real time) ------------------
        public static readonly double[] Rumble43Crack = { 1.0, 1.0, 120, 0.7, 0.18, 70, 0.45, 0.08, 70, 0.22, 0.0, 90 }; // FUSION / OPEN boss hit: the crack, then a fading thud
        public static readonly double[] Rumble43Thud = { 0.8, 0.2, 80 }; // the THUNDERCLAP's shot
        public static readonly double[] Rumble43Buzz = { 0.15, 0.5, 140 }; // the TORCH: re-asked every 0.1 s tick, so it holds while firing
        public static readonly double[] Rumble43Release = { 1.0, 0.55, 110, 0.4, 0.1, 90 }; // a charged release (the slug, the charged KESTREL)
        public static readonly double[] Rumble43Light = { 0.12, 0.32, 45 }; // a light gun's shot (KESTREL, nailgun, others)
        public static readonly double[] Rumble43Rail = { 0.5, 0.6, 90 }; // the LONGWAVE / rocket
        public static readonly double[] Rumble43Dash = { 0.35, 0.22, 60 }; // a dash
        public const int Rumble43LogMax = 64; // log entries kept for the reel (the oldest go)

        // --- the feel43 reel ------------------------------------------------------------------------------------------------------------------
        public const double Feel43PairMin = 0.02; // the reel's GREEN: mean |on - off| luma in the screen's middle half at the moment's key frame (n43-r1: the slug's
                                                   // own cyan wash, alike in both passes, made "on changed more than off vs its baseline" a coin flip)
    }
}
