// NIGHT72 builder B (BOSSES & BODIES; FORGE72 D-1): the Unity-only render numbers of this night's four rows (CLAUDE.md rule 5: never typed in code, one
// comment each). Rows: bossfx72 (Crown37View.Crown72.cs, BossTell63.cs), body72 (WyrmView.Body72.cs), btext72 (Words72.cs), spring72 (Spring72.cs). The
// sim never reads any of them; no gameplay number is here (hit spheres, timings and the AI stay the web's and the config's). The web has no such
// numbers: it has no campaign bosses, no boss look and no boss text.
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- B2 bossfx72 (gg-proxy #3: THE CROWN is in none of its five frames; a 700-900 px burst sits on the crosshair) ----

        /// <summary>The Crown's alive gem as a share of the gap between two neighbours on its ring (2 pi r / gems): the gems read one by one. NIGHT38's
        /// 1.12 m gem on the 20-gem 3 m ring (a 0.94 m gap) overlapped its neighbours into one lit band. t1 round 1 (0.72 of the gap, 0.68 m) still
        /// left a flat cyan blotch of 4-5 touching gems across the crosshair (the lead's t1 verdict, bfalone-sp idle: 27.3 % of the 120 px aim disc
        /// glowing); 0.5 of the gap is 0.47 m: a small crisp diamond with a dark gap of 0.47 m to its neighbour. Derived from the ring (c.p.ringRadius,
        /// c.p.gems), so a ring of another size keeps the rule.</summary>
        public const float Fx72GemOfGap = 0.5f;

        /// <summary>The Crown's gem halo (the dim additive square behind each gem) as a multiple of its gem: NIGHT38's 2.2 x a 1.12 m gem = 2.46 m,
        /// 20 of them on the ring overlapped into one 700-900 px annulus (cyan idle, red-orange tell) that hid the ring and its blades. 1.2 x the
        /// 0.47 m gem is 0.56 m, well inside the 0.94 m gap: a soft backing on each gem, never a ring of its own.</summary>
        public const float Fx72HaloOfGem = 1.2f;

        /// <summary>The halo's colour share after NIGHT38's / NIGHT40's own tint (a read-back of what they set, times this): the smaller square needs
        /// less to read as a glow behind the gem and not as a second gem.</summary>
        public const float Fx72HaloGainShare = 0.5f;

        /// <summary>The gem's own colour share (and its cross's) after NIGHT38's / NIGHT40's tint, read back: the gem's gain 1.4 x its pulse clipped every gem to a flat
        /// 255 cyan (or a flat white-pink in a tell) blotch; 0.7 leaves a coloured shape with an edge. A start value to A / B.</summary>
        public const float Fx72GemGainShare = 0.7f;

        /// <summary>THE AIM WINDOW (the t1 verdict: "the bursts must come off the crosshair"): the gems, their crosses and their halos within Fx72AimInnerPx of the screen's centre (px at
        /// 1080p; the crosshair) are drawn at Fx72AimFloor of their colour, easing to full by Fx72AimOuterPx: the aim point keeps its target (a dim cyan core) and loses the burst. The
        /// ring and the blades (thin lines, the Crown's body) are not touched. The precedent is the torch's spray rule: banned 16 dimmed every sprite near the crosshair. Numbers:
        /// inner 100 px (the aim disc the meter reads is 120), outer 300 px (the meter's wider disc), floor 0.12 (a gem's glow under the 0.75 value the meter calls glow).</summary>
        public const float Fx72AimInnerPx = 100f, Fx72AimOuterPx = 300f, Fx72AimFloor = 0.12f;

        /// <summary>The Crown's blade ring, the inner rim and the faint outer halo ring while it may be passed (cyan, player-usable, CLAUDE.md rule 8):
        /// the gain. NIGHT40's idle gain 0.55 left the ring dim on the ceiling's dark grid; 0.9 stays under the bloom's wash-to-white (the lit gain
        /// Crown40LitGain 1.9 is the speed-at-the-pass glow and is kept as it is).</summary>
        public const float Fx72RingGain = 0.9f;

        /// <summary>THE CROWN's danger column (BossTell63, a red tell's wind-up): its radius and height shares of UnityTuning.BossTell63Size
        /// (crown37: 5 m x 10 m). At 6.6-8 m from the ring (the boss63 reel's flinch and stagger distances) the full tube covered about a third of
        /// the frame (x 450-1600, y 0-1080 in bp/boss63's tell frame). The projected area goes with radius x height: 0.62 x 0.6 = 0.37, about 12 %
        /// of the frame. A boss not listed keeps its column.</summary>
        public static readonly Dictionary<string, float[]> Fx72ColumnShare = new Dictionary<string, float[]>
        {
            { "crown37", new[] { 0.62f, 0.6f } },
        };

        // ---- B1 body72 (gg-proxy #4: THE KILN WYRM blends into its wall; nothing shows its charge; GG 2026-10-22 "boss animations are too rigid") ----

        /// <summary>THE KILN WYRM's body pieces' base colour with body72 on: NIGHT47's ash 0x1a1210 was darker than the brown kiln wall behind it, so the worm read as a hole in
        /// the wall; drawn through SKYGRIND/Enemy (the enemies' shader: x CrunchEnemyValue 1.35, the rim, the form shade, the ink outline). t1 round 1 used a NEUTRAL ash (0x5c4c46):
        /// the kiln's grade zeroes blue (the t1 frame's wall, floor and column read blue 3-13 of 255 everywhere), so a neutral body rendered R = G = yellow, hue 46-49 deg, the parry
        /// tell's colour (the lead's t1 verdict; the column under the centre platform is the worm's own pieces and seams). A red-weighted base survives the grade: 0x7a3626
        /// (122, 54, 38) x 1.35 renders about (165, 73, 3), hue 25 deg, inside the danger band (345-35), luma about 87 against the wall's 40-60 (the slate's bar: at least 25 apart;
        /// read off the Player frames, not here). Red-orange is the enemy's colour (rule 8).</summary>
        public const uint Body72Hex = 0x7a3626;

        /// <summary>The wyrm's rim with body72 on: the danger red (Tell36RedHex, linear) x this, per piece through its property block. SKYGRIND/Enemy's own rim is the config's
        /// fx.enemyRimColor (255, 154, 82) x 1.6 x CrunchRimBoost 1.5, x up to 1.6 with distance: about (3.8, 1.2, 0.3) HDR at 30 m, which on an orange body clips G to 255 = yellow
        /// (the t2 frame: a golden box on top of the column, hue 49). A red rim (R about 2 x 1.6, G 0.08 x 1.6) lifts the silhouette and leaves the hue in the danger band.</summary>
        public const float Body72RimGain = 2.0f;

        /// <summary>The head's base colour with body72 on: a step lighter than the body, the same red-weighted hue (the punch window's yellow and the open head's weak role are
        /// its own, untouched).</summary>
        public const uint Body72HeadHex = 0x8a4430;

        /// <summary>The follow-through spring along the body pieces (frame time, render only): the drawn piece trails its rigid centre by a damped spring, f in Hz
        /// (the head end follows fast, the tail slower, so the tail lags the head: scout-animation.md 1: 1-3 Hz body follow, z 0.3-0.5 jelly) and damping ratio.
        /// At 4 Hz and z 0.35 a piece on a 10 m/s path trails 0.28 m (Spring72Math.SteadyLag); the guard below holds it.</summary>
        public const float Body72LagHzHead = 4f, Body72LagHzTail = 2.2f, Body72LagZeta = 0.35f;

        /// <summary>The lag's share of a body piece's own guard (BossMotion63's limitShare x its half width: the drawn boxes are the hitboxes, "a shot finds what you
        /// see"): the rest of the guard is left for the ripple, the sag and the breath already drawn; the sum is still held to the whole guard by the layer's own clamp.</summary>
        public const float Body72LagGuardShare = 0.6f;

        /// <summary>A piece farther than this from where its spring is (m: a piece surfacing from the floor, the worm's dive) starts at rest on its rigid centre
        /// instead of streaking across the arena.</summary>
        public const float Body72LagResetM = 5f;

        /// <summary>The tell glow with body72 on, through a wind-up of THE KILN WYRM (every attack but the roar; the head's punch window keeps its yellow): the
        /// seams' and the trim plates' HDR gain in the danger red-orange (Tell36RedHex) at level 1, BossTell63.Level rising 0.45 to 1 through the wind-up.</summary>
        public const float Body72TellGain = 2.2f;

        /// <summary>The tell's share of the body piece's own colour moved toward the danger red-orange at level 1 (the head fully; Body72TellTail of it at the tail).</summary>
        public const float Body72TellBody = 0.55f, Body72TellTail = 0.3f;

        /// <summary>The wyrm's proof line (a lag line) prints at most this often (s of frame time).</summary>
        public const float Body72SayEveryS = 4f;

        // ---- B3 btext72 (gg-proxy #5: the bar's order is grey and about 16 px; a beam cuts the fight line; STAGGER stands under an unfilled meter) ----

        /// <summary>The boss bar's own order (b43Hint) at least this many px at 1080p with btext72 on, in the bone colour: NIGHT72's floor for new lines is 22 px (D-9); it was 18 px
        /// (Bar43HintPx) at a grey 55 % alpha. The fight line is already 40 px (Hud47FightPx). x the TEXT SIZE setting, as every hud47 label.</summary>
        public const float Btext72HintPx = 22f;

        /// <summary>The plate behind a boss line (the fight line, the bar's order): the HUD's shadow ink 0x0A0E14 (UiStyle33.Shadow's hue) at this alpha: dark enough that an orange beam or a
        /// bright cyan burst behind the line never cuts it (the n71-polish8 crown-windup frame's "throu|gh"), light enough that the room still shows through.</summary>
        public const float Btext72PlateAlpha = 0.62f;

        /// <summary>The plate's padding round the text (px at 1080p): left and right, top and bottom.</summary>
        public const float Btext72PlatePadX = 18f, Btext72PlatePadY = 6f;

        /// <summary>The counter's line prints at most this often while a boss bar is up (s of frame time), besides the bar going down, a new boss, a new run and the quit.</summary>
        public const float Btext72SayEveryS = 6f;

        /// <summary>THE BOSS LINES with btext72 on (the t1 verdict: "Swap to the gauntlet, then swing at its charging head." is nine words in lower case; the rule is at most 6 words, the verb in
        /// CAPITALS, the input's glyph). Each row: the order's raw text as the game and words71 carry it (the key; {action} tokens as written), the weapon id the order names (null: none), the
        /// line when that weapon is in hand (or none is named), the line when it is not (names the hand to take: SWAP). A glyph token ({fire}, {punch}, {jump}, {dash}, {grapple}) resolves to the player's bound key or pad glyph (UiLesson25's resolver), never a
        /// keyboard word on a pad. The rule counts the words AS DISPLAYED (the t3 verdict: "SWAP to gauntlet, then SWING LEFT CLICK" is seven, not six): a pad glyph or a key name is one word, but {fire} and
        /// {altFire} read "LEFT CLICK" / "RIGHT CLICK" on a keyboard (UnityTuning.Lesson25Aliases), two words; Btext72.DisplayWords counts them so, and the test holds every line to the limit. The sim never reads
        /// these; the web has none. Text only.</summary>
        public static readonly (string raw, string weapon, string held, string swap)[] Btext72Lines =
        {
            ("Swing the gauntlet {fire} at its charging head.", "gauntlet", "SWING {fire} at the head", "SWAP gauntlet, then SWING {fire}"),
            ("PUNCH THE HEAD TO STAGGER IT", "gauntlet", "SWING {fire} at the head", "SWAP gauntlet, then SWING {fire}"),
            ("PUNCH THE GLOWING HEAD.", "gauntlet", "SWING {fire} at glowing head", "SWAP gauntlet, then SWING {fire}"),
            ("Get close and swing the gauntlet {fire}.", "gauntlet", "GET CLOSE, SWING gauntlet {fire}", "SWAP gauntlet, then SWING {fire}"),
            ("GET IN CLOSE: {fire} THE GAUNTLET HITS FULL", "gauntlet", "GET CLOSE, SWING gauntlet {fire}", "SWAP gauntlet, then SWING {fire}"),
            ("Parry {punch} the yellow embers, then shoot.", null, "PARRY {punch} yellow embers, then SHOOT", "PARRY {punch} yellow embers, then SHOOT"),
            ("Hook a lit knee {grapple}, then grapple away.", null, "HOOK lit knee {grapple}, GRAPPLE away", "HOOK lit knee {grapple}, GRAPPLE away"),
            ("Jump {jump} and dash {dash} through the ring.", null, "JUMP {jump}, DASH {dash} through ring", "JUMP {jump}, DASH {dash} through ring"),
            // the bars' own hints (design.n43.wick43.hints / .bosses) that break the rule: seven words, no verb in capitals, or the wrong button (the wyrm's charge hint names {punch}; only the gauntlet staggers it)
            ("{punch} ITS HEAD AS IT COMES", "gauntlet", "SWING {fire} as it comes", "SWAP gauntlet, then SWING {fire}"),
            ("HOLD {jump}: RIDE. LET GO: LEAP UP", null, "HOLD {jump} RIDE, RELEASE to LEAP", "HOLD {jump} RIDE, RELEASE to LEAP"),
            ("{dash} + {jump} through the ring.", null, "JUMP {jump}, DASH {dash} through ring", "JUMP {jump}, DASH {dash} through ring"),
            ("Hook a knee {grapple}, pull away.", null, "HOOK knee {grapple}, PULL away", "HOOK knee {grapple}, PULL away"),
        };

        /// <summary>The longest a boss line may be with btext72 on (words AS DISPLAYED: a pad glyph or a key name is one, "LEFT CLICK" is two): the lead's rule for new boss text.</summary>
        public const int Btext72MaxWords = 6;

        // ---- B4 spring72 (EnemyViews.Spring72.cs: every enemy body in game.enemies; frame time, render only, volume-preserving) ----

        /// <summary>Idle breathing: the body's height swells by this share at this rate (Hz). scout-animation.md 1: "cheap, always on, fixes rigid"; 2.2 % of a 1.8 m husk is 4 cm up and down; 0.7 Hz
        /// is a slow breath, far under rule 8's 3 Hz flicker ceiling. A start value to A / B (no source gives one): the pair, not this comment, says whether it reads.</summary>
        public const float Spring72BreathAmp = 0.022f, Spring72BreathHz = 0.7f;

        /// <summary>The wind-up squash: the body's height gathers down by this share while the sim's wind-up runs (anticipation); the spring's rate (Hz) and damping ratio. At z 0.35 the gather
        /// itself dips to 15 % (it overshoots its 12 % target) and the release overshoots 31 % of the held squash (Spring72Math.Overshoot) into a 3.5 % stretch (the Python mirror): the strike's
        /// snap. f 6 Hz: a 0.17 s period, inside every wind-up of the config (0.5-1 s).</summary>
        public const float Spring72WindSquash = 0.12f, Spring72WindHz = 6f, Spring72WindZeta = 0.35f;

        /// <summary>The flinch follow-through: a hit kicks the squash spring's speed by this (share of the height per s; negative: squash first), then it rings at this rate and damping ratio.
        /// NIGHT72 had -4.5 at 5 Hz and z 0.3: an 8 % squash then a 3 % stretch, quiet by 0.18 s (the Python mirror of Spring72Math.Step at 60 fps, Tools/night/n73-spring-mirror.py scan): about
        /// 22 px on a husk 5 m away and 9 px at 12 m, and the hit reaction already in the game (crunch36's 15 % pop, peak 32 ms, back in 4 frames) hides it, so the n73-a reel's pair read nothing
        /// (the lead: "no player can see it"). NIGHT73 (the lead's job: "a hit squashes clearly and rings back, ULTRAKILL's enemies snap on a hit; start from 15-18 % and a faster settle"): -15.5 at
        /// 7 Hz and z 0.35 is a 17.0 % squash at 33 ms (2 frames; 30 fps 15.4 %, 120 fps 17.7 %, 144 fps 18.5 %), a 5.0 % stretch at 100 ms, under 1.5 % from 133 ms (the old one: 183 ms), 44 px
        /// on that husk at 5 m and 18 px at 12 m; with breath on top (0.022) the sum stays under Spring72MaxSquash. -15.5 is solved, not computed: a frame's coarse step reads about 15 % under the
        /// continuous peak (mirror: solve_kick), so the textbook peak per speed is not the number. A rapid fire does not pump it (the mirror's stream mode: a hit every 4 frames holds a 17 % squash,
        /// every 7 frames peaks at 17.7 % and 5.1 %, never over 18 %): no cap needed. z 0.35 and not 0.3 so the rebound is one stretch and one faint squash, not a jelly's three.</summary>
        public const float Spring72FlinchKick = -15.5f, Spring72FlinchHz = 7f, Spring72FlinchZeta = 0.35f;

        /// <summary>What a flinch must do for Spring72Tests' tuning pin (the tests read these, so a later retune cannot quietly drop the read): the squash's peak at 60 fps over this (the tuned 17 %
        /// with a margin: the same kick reads 15.4 % at 30 fps), the first stretch over this share, the spring under Spring72SettledShare, and staying there, by Spring72SettledS s after the hit (0.15 s: the last frame over 1.5 % is 117 ms in the mirror; NIGHT72's was 167 ms).</summary>
        public const float Spring72PeakMin = 0.15f, Spring72StretchMin = 0.03f, Spring72SettledShare = 0.015f, Spring72SettledS = 0.15f;

        // ---- spring73 (FeelReel.N73.B.cs: the reel scenario that gives the spring something to show; the reel's own clock, never the game's) ----

        /// <summary>How far the husk stands (m; the brief's 4-6 m: a 1.8 m husk is about 258 px high at 5 m on a 74 deg view at 1080p, so a 17 % squash is 44 px) and how many non-lethal hits land on it while
        /// it stands still (the hit spring's flinch, one every Spring73HitEveryMs; a fifth lands in its wind-up). The cadence is 0.5 s: the flinch is quiet by 0.15 s, so each hit is read alone, and five
        /// hits fit a strip of five frames evenly spaced over two seconds (n73-strip.py strip ... 5), each frame a hit's peak.</summary>
        public const double Spring73DistanceM = 5.0;
        public const int Spring73StillHits = 4, Spring73HitEveryMs = 500;

        /// <summary>After the recording starts and before the first hit (ms, on top of the reel's own pre-roll): the idle breath alone. Then the pause after the last still hit before the husk is let go
        /// (ms: the ring settles and the husk is seen standing), and the longest wait for its wind-up to start once it walks (ms; it covers the 2.8 m to its melee range at 6 m/s in 0.47 s).</summary>
        public const int Spring73BreathMs = 200, Spring73LetGoAfterMs = 700, Spring73WalkMaxMs = 1500;

        /// <summary>Where in its own wind-up (shares of husk.windupTime, read from the config, never typed) the fifth non-lethal hit and the kill land: 0.3 and 0.65 leave the gather's start, the flinch
        /// inside the gather and the strike's release unseen by the kill (the husk dies before its attack resolves at 1.0). The n73-d build's husk wind-up is 1.00 s (the reel prints it), so these are frames 18 and 39 of 60.</summary>
        public const double Spring73WindHitShare = 0.3, Spring73KillShare = 0.65;

        /// <summary>A shot is one TAP (the KESTREL fires on the RELEASE of a tap and charges while it is held: charge35's p39; the n73-d first run held the trigger for the whole wait, so every shot came at
        /// the release 40 frames later: the gap was 69 frames, not 30, and the reel's own counter, which looked for the hit inside the hold, read 0): the trigger is held UnityTuning.T125TapFrames reel
        /// frames (n52-r20's 3-frame taps), then let go, then the reel waits this many frames (0.33 s: a gun still cooling down queues the tap) for the hit event, and taps again, at most Spring73TapTries
        /// taps for one shot.</summary>
        public const int Spring73HitWaitFrames = 20, Spring73TapTries = 3;

        /// <summary>GREEN needs every non-lethal hit's flinch to squash at least this much (the springs' own, row on or off: the tuned 17 % less a margin for a slow frame) and the idle breath to have
        /// swung at least this much (Spring72BreathAmp is 0.022: the breath was seen working).</summary>
        public const float Spring73FlinchGreenMin = 0.12f, Spring73BreathGreenMin = 0.015f;

        /// <summary>The sum of breath, wind-up and flinch squash is held to this share of the height either way (the slate's squash bar: volume kept, never a pancake).</summary>
        public const float Spring72MaxSquash = 0.2f;

        /// <summary>The proof line prints at most this often while bodies are drawn (s of frame time), and always on the first body-frame of a state (on or off) and on each of the first
        /// Spring72KickLines hits that kick a spring. The t1 round's 5 s clock never fired in kill12's 2.5 s (150 frames): no line, no measurement.</summary>
        public const float Spring72SayEveryS = 1f;
        public const int Spring72KickLines = 6;

        // ---- B5 finish55 (Finish55View.cs: the boss kill's beat; view only, frame time) ----

        /// <summary>The camera kick of a boss kill (deg of view pitch-up) and its attack and recover (s): above the biggest of kicks71's ladder (the punch kill's 3.6 deg, design.n71.kicks71) because the
        /// boss is the biggest kill in the game (it got a body kill's 2.2 deg: the boss kill is a KillEvent of kind "boss"), and a longer settle (0.55 s against the ladder's 0.22) so the frame
        /// holds its tilt for the beat. A start value to A / B; the pair says whether it reads.</summary>
        public const float Finish55KickDeg = 6f, Finish55KickAttackS = 0.03f, Finish55KickRecoverS = 0.55f;

        /// <summary>The FOV punch-in of a boss kill (deg the field of view closes) and its attack and recover (s): above the body kill's 1.2 deg (kicks71) and the headshot kill's 1.5.</summary>
        // NIGHT73: 3.5 deg / 0.5 s did not read on a motion strip (Logs/n73/spot/finish55/spot.md: NOTHING OBVIOUS; the punch was gone by +0.4 s), so 8 deg over 0.8 s:
        // the boss kill closes the view like a camera push-in, still recovering at the strip's second frame.
        public const float Finish55FovDeg = 8f, Finish55FovAttackS = 0.03f, Finish55FovRecoverS = 0.8f;

        /// <summary>The comic panel's two bars (a letterbox): each bar's share of the screen's height, the slide in (s), how long they hold after bossDown (s) and the slide out (s); the
        /// bars are the HUD's shadow ink at this alpha with a bone hairline on their inner edge (px at 1080p). They slide in once and out once: no loop, no flicker (GG 2026-10-26).</summary>
        public const float Finish55BarShare = 0.085f, Finish55BarInS = 0.12f, Finish55HoldAfterDownS = 0.6f, Finish55BarOutS = 0.35f, Finish55BarAlpha = 0.92f, Finish55HairPx = 3f;

        /// <summary>A beat with no bossDown (a boss that never reaches it) lets go after this many s.</summary>
        public const float Finish55MaxS = 12f;

        /// <summary>The bars' canvas: over the impact flash (6), the crush frame (7) and feel46's warning markers (8), UNDER the HUD (Ui.cs's 10), so the boss bar, the health and the weapon panel stay on top of them.</summary>
        public const int Finish55CanvasOrder = 9;

        // ---- B5 bosskill72 (FeelReel.N72.B.cs: the reel phase that kills a boss and frames the kill; the reel's own clock, never the game's) ----

        /// <summary>s of the fight (from the rise) the reel fires at the weak point before it lowers the boss's health to the kill's share; the share of its pool left (the reel's action, as boss61's
        /// BossRoomBegin: the sim's own public fields); s the reel waits for the death before it says RED; s it films after bossDown.</summary>
        public const double BossKill72LeadS = 6.0, BossKill72HpShare = 0.02, BossKill72MaxS = 60.0, BossKill72AfterS = 1.6;

        /// <summary>The t3 round's lesson (the lead's B5 SHORT: the kill-timed sides killed a HUSK): the wyrm resets its health to a phase threshold when a hit lands under it (Wyrm.Apply: p1 under
        /// phase2At, p2 under phase3At, p3 under phase4At where one exists), so a health set to the kill share in phase 1 comes back at 66 %. The reel steps the phases down the way FeelReel.N24.Wyrm's
        /// wyrmdies does (hp set to this share of the next threshold, Apply(0) for the beat), then sets the kill share in the last phase. 0.9: just under the threshold.</summary>
        public const double BossKill72StageBelow = 0.9;

        /// <summary>s after the kill share is set that the reel waits for the taps to kill the boss; then it calls the boss's own public damage method for the last points (the same call wyrmdies makes),
        /// said in the line, so a death by the taps and a death by the reel are told apart. Needed because a boss's own rules (a part, an armour, a closed window) can refuse a tap; the kill's feel is
        /// the same either way (finish55 and kicks71 start at bossDying).</summary>
        public const double BossKill72ApplyAfterS = 6.0;

        // ---- bosskill73: framing THE KILN WYRM's kill so finish55 can be judged (FeelReel.N73.Frame.cs; the reel's own player and aim, never the game's; the other bosses are not framed) ----

        /// <summary>The pieces framed: the head and the next two (the aim is the second box's centre, the first body piece), and the share of the VERTICAL field of view their bounding sphere should fill (the lead:
        /// "the body fills about a third of the frame"). With the head 3 x 3 x 4 m and the pieces 2.5 m at 2.4 m spacing the sphere is about 4 m, so the distance is about 19 m on the 74 deg view.</summary>
        public const int Fin73Pieces = 3;
        public const double Fin73FillShare = 1.0 / 3.0;

        /// <summary>The gate opens when the eye-to-aim distance is within these multiples of the distance that fills Fin73FillShare (0.8 to 1.25: a quarter to two fifths of the frame).</summary>
        public const double Fin73BandLo = 0.8, Fin73BandHi = 1.25;

        /// <summary>Where the player stands: this far (m) from the arena's centre toward the room's entry, and how far (m) a shove may move the player before the gate puts him back (at most every Fin73DriftFrames
        /// frames, and only while the gate is shut: a respawn cuts the camera's smoothing, which would eat the dying beat's kick). The wyrm's coil is a ring of radius 19 m round the centre at 2.5-5 m (wyrm.json) and
        /// the arena has four 4 m pillars at (+-10, +-10), which cut a sight line from a spot 12 m out through the middle of the band: Tools/night/n73-frame-geometry.py (the gate's terms on that ring, the arena's
        /// boxes) found 12 m leaves only 4 m of coil gate-ok (7 frames at the coil's 34 m/s: the 10-frame hold would never be met), 10 m gives 17 frames, 8 m the most on the entry axis: 46 degrees of the ring on
        /// each side, 16 m of coil, 28 frames at 34 m/s; 6 m gives 5. (n73-frame-search.py over every spot within 15 m: the best spots are about 8.5 m out, 20 degrees off the entry axis, 62 degrees of arc; the
        /// axis spot is within a few degrees of that and needs no per-arena angle.)</summary>
        public const double Fin73VantageM = 8.0, Fin73DriftM = 3.0;
        public const int Fin73DriftFrames = 30;

        /// <summary>The gate's other terms: the sight line from the eye to the aim misses the arena's centre by at least this (m; the n48-r3 frames were filled by "the core beam in the middle"), the three pieces
        /// are at least this far (m) over the floor (not half in it: the dive's burrow, the dying beat's sink), and all of it holds this many frames in a row before the taps are let go. From the 8 m vantage the head is gate-ok for 46 degrees of
        /// the ring twice a lap (66-112 and 248-294 degrees from the entry axis; n73-frame-geometry.py): 28 frames at the coil's 34 m/s in phase 3 (22 m/s x 1.25 x 1.25: the wyrm's PhaseSpeedMult; its aim row's
        /// gap speed only adds to it), 36 at 27 m/s. 10 frames (0.17 s) leave room for that; the camera is on the body all the while, and the head is still in the band at the beat's start a few frames after
        /// the arming. After Fin73GateMaxS s from the forced health the kill is armed anyway (said in the line).</summary>
        public const double Fin73ClearM = 6.0, Fin73AboveM = 1.2, Fin73GateMaxS = 30.0;
        public const int Fin73HoldFrames = 10;

        /// <summary>The frames the line tells the lead to cut round the dying beat: this many before it and after it (the lead: "the dying beat - 6 through + 60").</summary>
        public const int Fin73CutBefore = 6, Fin73CutAfter = 60;

        /// <summary>The loop frames after the dying beat at which the second measurement is taken (30 = half a second at ReelFps 60: the head has slowed to a stop by about a second).</summary>
        public const int Fin73HalfFrames = 30;

        /// <summary>The line's FRAMED verdict: the body's larger side (the box of the three pieces: width over the frame's width, height over its height) at the dying beat is at least this share of the frame.
        /// The sphere that subtends a third of the vertical view is about 8 m across; the three pieces in a row are about 9 m long and 3 m high, so a third of the vertical view is about a quarter of the width.
        /// 0.2 leaves room for a body seen at an angle; the lead's own look and the spotter decide whether it reads.</summary>
        public const double Fin73FramedMinShare = 0.2;

        // ---- core73: THE KILN WYRM's open weak point as a lit core (Core73.cs, BossMarks32.cs, WyrmView.cs; view only; the sim's core numbers stay in design.n24.wyrm.core) ----

        /// <summary>The glow: this many concentric additive spheres (a disc each on screen), their glows summing to a radial profile Core73Peak x (1 - rho^2)^Core73Power (Core73Math.Layers). The python mirror (a
        /// scratch core_layers.py, the same arithmetic): 6 layers at power 1.6 step 25 % of the peak between neighbouring rings (a ring you can count), 10 layers 15 %, 12 layers 12 % with the edge ring 1.8 % of the
        /// peak (a soft edge) and the centre at 1.596 of 1.6: 12 draws of a 768-triangle sphere, nothing. The peak 1.6 is today's open shell's own brightness (BossMarks32.OpenGlow). ROUND 2: it is drawn ON the head,
        /// additive over the head's own lit cyan, so everywhere it is at least as bright as the head and at its middle 1.6 of the routed cyan more.</summary>
        public const int Core73Layers = 12;
        public const float Core73Power = 1.6f, Core73Peak = 1.6f;

        /// <summary>ROUND 2 (the lead's look at n73-g: the round-1 core sat above the head as a faint haze and the "shoot here" mark was gone): the glow's outer radius as a share of the head box's smallest half size
        /// (design.n24.wyrm.body headW 3, headH 3, headLen 4: 1.5 m, so 1.2 m radius, 2.4 m across), centred where the line from the head's centre toward the camera leaves the box (Core73Math.FrontPoint: the face
        /// the camera sees, at the head's centre on screen), so the dome stays inside the face's outline seen from the front (a sphere centred on the face plane stands out 1.2 m toward the camera: from 10 m it
        /// projects about 14 % wider than the face-on disc, so 1.2 m of 1.5 m stays inside). Round 1's was 1.5 x the sim's core radius, 0.7 m above the head.</summary>
        public const float Core73FaceShare = 0.8f;

        /// <summary>The pulse (breathing, not a flicker; GG 2026-10-26 "flickers"; the lead's bar: slower than 2 Hz): 1.2 Hz, 12 % deep (today's shell: 3 Hz, 15 %; the exposed core sphere in WyrmView: 2.2 Hz, 23 %,
        /// slowed to this rate with the row on), and the fade-in over 0.25 s after the window opens (a smooth step: today's shell appears at once at full brightness).</summary>
        public const float Core73PulseHz = 1.2f, Core73PulseDepth = 0.12f, Core73FadeS = 0.25f;

        /// <summary>Core73Tests' softness bar: the largest brightness step between neighbouring rings, as a share of the peak (the tuned 12 layers give 12 %); and the pulse bar the lead set.</summary>
        public const float Core73MaxStepShare = 0.15f, Core73PulseHzMax = 2f;
    }
}
