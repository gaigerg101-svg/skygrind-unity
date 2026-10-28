// NIGHT16 U3-29.5 (builder I): the impact layer's render-only numbers (Game/ImpactFrames.cs), each with why the web has
// none. The freezes, their cooldowns and budget and the slow-mos are design.impact (Design/unity-config.json); the web's
// FOV kicks (the multikill punch, the melee lunge, the rank-up punch) keep config.json's feel.fovPunch / feel.meleeLunge.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the freeze's FOV punch-in (a kick on the web's fovKick stack, narrowing: player.js's + widens) -----------------
        // The web freezes only on a kill and a parry and has no punch-in for either; ULTRAKILL-style impact frames read
        // stronger with the view drawn in a touch while the world stops (FEEL-DESIGN.md section 2). Degrees narrowed, for the
        // non-melee moments only (a punch already narrows by the web's melee lunge; two narrowings would stack):
        public const double ImpactPunchInDeg = 1.5; // headshot kill, parry, PERFECT PARRY: the big moments
        public const double ImpactPunchInLightDeg = 1.0; // a headshot, a heavy body hit
        // Its shape (KickStack: an eased rise, then a smoothstep fall): in over ImpactPunchAttack s, held through the freeze,
        // back over ImpactPunchRelease s after it (the kick's recover = the freeze + this), on frame time.
        public const double ImpactPunchAttack = 0.015;
        public const double ImpactPunchRelease = 0.08;

        // --- the headshot kill's screen flash (the parry's is the HUD's own: UiFeel.cs, punch.parryFlashOpacity) --------------
        // The web flashes only on a parry. Its alpha (x settings.flashIntensity; 0 = no flash, the freeze stays), white (a red
        // flash is the enemies' colour and XAG 118's warning: FEEL-DESIGN.md section 2.7), and its fade on frame time:
        // NIGHT17 round 2: 0.18 for 0.1 s washed the whole headshot freeze grey (a flat white over ~80 ms reads as lag; both
        // reviewers saw it); now a 2-3 frame pop that's gone before the freeze ends, the victim's white blocks carry the moment.
        public const float ImpactFlashAlpha = 0.08f; // under the parry's flash: a headshot kill is frequent, a parry is rare
        public const double ImpactFlashTime = 0.035;
        // WCAG 2.3.1 (three flashes in any second): the game's one gate, Game/FlashGate.cs (NIGHT16 review finding 7).
        // Its canvas: over the dash tint (UnityTuning.DashTintSortingOrder 5), under the HUD (Ui.cs's canvas, 10), so the
        // hit marker on the freeze's first frame stays on top.
        public const int ImpactFlashSortingOrder = 6;

        // --- the pad's impact pulse (Rumble.Play, x RUMBLE) ---------------------------------------------------------------------
        // The web rumbles on a hit taken, a kill and a parry (config.gamepad.vibration) and has no impact tiers. A tier's
        // pulse lasts the freeze (at least ImpactRumbleMinMs, so a short one is felt); a kill's and a parry's keep the web's.
        public const double ImpactRumbleStrong = 0.35, ImpactRumbleWeak = 0.45; // headshots, punches, heavy hits: a sharp buzz
        public const double ImpactRumblePerfectStrong = 0.7, ImpactRumblePerfectWeak = 0.4; // PERFECT PARRY: over the parry's 0.45 / 0.2
        public const double ImpactRumbleMinMs = 50;
    }
}
