// NIGHT38 builder H (design.n38.juice38, a view row: no sim bit): the render side of the boss entrance, the big boss moves and the damage static
// (Juice38.cs, CameraFeel's hook, Crt29Pass's hook, the card on hud33's layer). CLAUDE.md rule 5: Unity-only numbers, each with a comment. The
// gameplay-facing amounts (shake sizes in ULTRAKILL's units, the hit-stop, the static's level and time) are config: design.n38.juice38.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the shake: ULTRAKILL's CameraShake on a first-person camera that only turns (CameraFeel: rotation only, never a translation) ---------
        public const double Juice38PitchDeg = 1.4; // deg the view pitches at a shake of 1 (ULTRAKILL moves the camera up to 1 m; at a 10 m look that is ~5.7 deg: we turn less, the aim must stay readable)
        public const double Juice38YawDeg = 1.4; // deg of yaw at a shake of 1
        public const double Juice38RollDeg = 2.2; // deg of roll at a shake of 1 (roll reads as impact without moving the aim point much)
        public const double Juice38NoiseHz = 22; // the shake's noise rate (Cosmetic.ValueNoise: smooth, no Random); ULTRAKILL re-rolls its offset every frame (~60 Hz): ours is smoother
        public const int Juice38NoiseSalt = 3801; // the shake's own noise salt (the web trauma uses 11 / 23 / 37, the crunch 101 / 211 / 307)

        // --- the flicker: Crt29's pass spiked (a boss entrance, a big move) ---------------------------------------------------------------------------
        public const int Juice38EntrancePulses = 2; // brightness pulses in an entrance's flicker (two in 0.3 s: under the 3-flashes-a-second rule, and FlashGate on top)
        public const int Juice38MovePulses = 1; // pulses in a big move's flicker
        public const float Juice38FlickerGain = 0.22f; // the brightness pulse's peak gain over the whole screen (the sweep band made screen-tall); x FLASH INTENSITY
        public const float Juice38SweepHalf = 2f; // uv: the sweep band's half height while it is the flicker (covers the whole screen: >= 0.75 of the gain everywhere)
        public const float Juice38FlickerScan = 0.35f; // extra scanline depth at the flicker's start (falls to 0 over the flicker; on top of the CRT's)
        public const float Juice38FlickerTear = 0.8f; // the band tear at the flicker's start (Crt29's ridelook35 tear, x 0.03 uv per torn band)
        public const float Juice38FlickerSplitUv = 0.004f; // the flat red / blue split at the flicker's start (uv x: ~8 px at 1920)
        public const float Juice38FlickerGrain = 0.06f; // extra grain at the flicker's start

        // --- the damage static: a full-screen TV burst at the hit's level (0.5 .. 0.8), falling 1 per second -------------------------------------
        public const float Juice38StaticGrain = 0.22f; // grain amplitude at level 1 (Look35's at-rest grain is 0.012: this is TV static, on purpose and brief)
        public const float Juice38StaticSplitUv = 0.007f; // the red / blue split at level 1 (uv x: ~13 px at 1920: the RGB-split read)
        public const float Juice38StaticTear = 0.9f; // the band tear at level 1 (none with REDUCED MOTION: it moves the picture)
        public const float Juice38StaticScan = 0.25f; // extra scanline depth at level 1
        public const double Juice38TearHz = 20; // Hz: the torn bands and the grain change this often during a burst (a fixed hash of the step: no Random)
        // NIGHT39 V (the lead's n39-r1 frame f0900: the static refired on every 0.25 s floor tick, a constant buzz): the full burst is for a hit; a
        // damage-over-time tick gets a small cue (Juice39Hurt.cs decides which).
        public static readonly string[] Juice38TickSources = { "hazard", "acid", "vent", "deadline" }; // hurt sources that tick (Health.Damage's source): hazard27 floors, THE LANTERN's floor and sweep (0.25 s ticks), spitter acid, device vents, the DEADLINE's bleed
        public const double Juice38BurstCooldown = 1.0; // s: a hurt this soon after a full burst is a tick (the burst, 0.5-0.8 falling 1 / s, is still on screen)
        public const double Juice38RepeatWindow = 0.75; // s: the same source hitting again this soon is a burn's cadence (THE WICK's hot wall and Lava30's grace: 0.5 s), a tick
        public const float Juice38TickStatic = 0.12f; // a tick's static level (a burst is 0.5-0.8): grain 0.026, split ~1.6 px at 1920, gone in 0.12 s at staticDecay 1 / s
        public const double Juice38TickShake = 0; // a tick's shake (none: the HUD's own damage cues still show the hit)
        public const string Juice38TickEvent = "hurt38Tick"; // a damage tick's small cue (names only: GG's sounds; hurt38Static stays the burst's)
        public const float Juice38LiveEpsilon = 0.002f; // under this every CRT add counts as 0 (the pass is not forced on for nothing)

        // --- the entrance card (the boss's name on hud33's layer) ---------------------------------------------------------------------------------
        public const float Juice38CardPx = 56f; // the name's size (px at 1080p; hud33's biggest is the 64 px health number)
        public const float Juice38CardSubPx = 20f; // the subtitle's size (rule 8: >= 16 px)
        public const float Juice38CardY = 170f; // px above the screen's centre (clear of the reticle, under the top-centre boss bar and slot)
        public const float Juice38CardW = 900f; // px: the card's text box width (one line, centred)
        public const double Juice38CardHold = 1.6; // s on screen (after the flicker-in), then out
        public const double Juice38CardIn = 0.12; // s: the flicker-in (three frames of on / off at 60 fps, the slot's TV language)
        public const double Juice38CardOut = 0.3; // s: the fade-out
        public const float Juice38CardTrackFrom = 28f; // TMP character spacing at the card's start, closing to Juice38CardTrackTo (the name lands)
        public const float Juice38CardTrackTo = 8f;

        // --- events (names only: GG's sounds; CLAUDE.md rule 7) -------------------------------------------------------------------------------
        public const string Juice38EntranceEvent = "boss38Entrance"; // a boss entered (shake + flicker + hit-stop + card); also listened to on the bus for an explicit call
        public const string Juice38MoveEvent = "boss38Move"; // a big boss move landed (shake + flicker)
        public const string Juice38HurtEvent = "hurt38Static"; // the damage static started (its level in Juice38Probe.Static)
    }
}
