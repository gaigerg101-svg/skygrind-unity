// PG3 (AME 294 Portfolio Game 3): the Unity-only numbers of the sound rows sound74 (Pg3Audio.cs) and music74 (MusicDirector.cs).
// The web game had no audio of the port's own, so none of these comes from PortExport/config.json; none is a gameplay number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // One-shot volume (0-1) for every effect clip; the ElevenLabs clips are already loud, so a little headroom.
        public const float Pg3SfxVolume = 0.8f;
        // The old single bed's volume during a run (used only with music74 off), its share on menus / pause, and its fade time (s).
        public const float Pg3MusicVolume = 0.32f, Pg3MusicMenuDuck = 0.5f, Pg3MusicFadeS = 0.8f;
        // The same clip never restarts sooner than this (s, real time): a shotgun's pellets fire `hit` many times in one step.
        public const float Pg3MinRepeatS = 0.045f;
        // The fixed pitch walk per repeat (five steps: -2..+2 x this), so a run of kills doesn't sound copy-pasted.
        public const float Pg3PitchStep = 0.035f;
        // One-shot voices in the round-robin pool (each keeps its pitch to the end of its clip).
        public const int Pg3Voices = 14;
        // The loops (the charge whine, the slide scrape, the heartbeat): their volume.
        public const float Pg3LoopVolume = 0.55f;
        // Below this share of max hp the heartbeat loops and the music goes muffled (the HUD's low-health feel; a view number).
        public const double Pg3LowHpShare = 0.3;
        // A landing faster than this (m/s down) plays the hard landing instead of the soft one.
        public const double Pg3HardLandSpeed = 14.0;
        // The effects' pitch at full slow motion (impact freeze, slow-mo, the ride-aim slow): GG wants the slow "extremely slow".
        public const float Pg3SlowPitch = 0.72f;
        // The TORCH's ticks closer than this (s) are one continuous stream (the loop); a longer gap starts a new burst.
        public const float Pg3TorchGapS = 0.2f;
        // The positioned voices (enemy deaths and attack tells): how many, how 3D (0 flat .. 1 fully 3D), full volume within Near (m),
        // silent past Far (m, linear: a room is 20-60 m across).
        // The body's sound: a footstep every Pg3StepStride metres of ground travel (a running stride, m) above a walking speed (m/s);
        // the wind rush from WindFrom to full at WindFull (m/s: running is ~9, a dash or slide boost well past 15) at WindVolume;
        // the level's ambience bed (amb-<level>) at AmbVolume.
        public const float Pg3StepStride = 2.6f, Pg3StepMinSpeed = 2f;
        public const float Pg3WindFrom = 11f, Pg3WindFull = 26f, Pg3WindVolume = 0.6f, Pg3AmbVolume = 0.5f;
        public const int Pg3Voices3D = 8;
        // The charge whine's volume at the start of a charge (share of full; it grows to full at a full charge).
        public const float Pg3ChargeVolFrom = 0.45f;
        // A hit's sound by its damage: full volume (and ~8 % lower) at Pg3HitHeavyDamage or more, Pg3HitGainFrom of it for a chip.
        // 60 is a view number (about a charged KESTREL hit or a close THUNDERCLAP blast), not a gameplay one.
        public const double Pg3HitHeavyDamage = 60.0;
        public const float Pg3HitGainFrom = 0.6f;
        // The music's duck when the player is hurt (s).
        public const float Pg3HurtDuckS = 0.35f;
        public const float Pg3SpatialBlend = 0.75f, Pg3Near3D = 5f, Pg3Far3D = 70f;

        // music74: the music's volume, the calm <-> combat crossfade (s), the crossfade into a boss theme (s), how long the combat
        // track holds after the last fight ends before it fades to calm (s), the duck under a big hit (share), the low-health
        // low-pass cutoff (Hz) and the music's pitch at full slow motion.
        public const float MusicVolume = 0.42f, MusicFadeS = 1.5f, MusicFadeBossS = 0.8f, MusicCalmAfterS = 4f, MusicDuck = 0.55f;
        public const float MusicLowHpCutoff = 1100f, MusicSlowPitch = 0.8f;
        // The loop seam's crossfade (s): a track crossfades into its own start this long before its end.
        public const float MusicLoopXfadeS = 2.5f;
        // A boss hit leaving it under this share of its health switches to its second ("enraged") theme.
        public const double MusicBoss2Share = 0.5;

        // PG3 A1: the player's volume rows (SETTINGS > AUDIO; Pg3Volume.cs). Each is a whole step 0..Pg3VolumeMaxStep (the level is step / max):
        // MASTER (AudioListener.volume: everything), MUSIC (the director's tracks and stings, the old bed) and SFX (every one-shot, loop and 3D voice).
        public const int Pg3VolumeMaxStep = 10;
        // The defaults: MASTER full, the music under the effects (the ElevenLabs effect clips are loud, the tracks are a bed), SFX a little under full.
        public const int Pg3MasterDefault = 10, Pg3MusicDefault = 7, Pg3SfxDefault = 9;
        // The PlayerPrefs keys the three steps are saved under (Pg3Volume; they last across sessions and builds of the same product).
        public const string Pg3MasterPref = "skygrind.audio.master", Pg3MusicPref = "skygrind.audio.music", Pg3SfxPref = "skygrind.audio.sfx";
        // The settings tab the rows sit in (every other tab already holds its 8 front rows: MenuRowsBeforeFold) and the rows' words
        // (label at most 3 words, footer at most 8 and never the label's text: MenusTests).
        public const string Pg3AudioTabId = "audio";
        public const string Pg3MasterLabel = "MASTER VOLUME", Pg3MusicLabel = "MUSIC VOLUME", Pg3SfxLabel = "SFX VOLUME";
        public const string Pg3MasterFoot = "HOW LOUD THE WHOLE GAME IS";
        public const string Pg3MusicFoot = "THE SOUNDTRACK, UNDER THE FIGHT";
        public const string Pg3SfxFoot = "SHOTS, STEPS, HITS AND MENU CLICKS";
        // The settings items' descriptions (the store's own text; the footer above is what the screen shows).
        public const string Pg3VolumeDesc = "0 is silent, 10 is full";
    }
}
