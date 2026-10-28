// NIGHT33 builder W (row view33, DESIGN33 W, D-5; G1 S1 / S3): Unity-only view numbers for the zoom pose and the piercer's kick (CLAUDE.md rule 5).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // D-5 / G1 S1: with view33 on, LT zoom moves the WORLD FOV only; the gun stays at its hold pose (no slide to the centre, no ADS yaw / pitch,
        // no roll) and only lowers by this many metres (the 4-6 cm band; 5 cm the middle) at full zoom, out of the sight line
        public const double ZoomLower33 = 0.05;
        // D-5: the zoom's ease in and out with view33 on, in s (120 ms; the web's ads.classEase is 0.13 / 0.18 / 0.25 s by class)
        public const double ZoomEase33 = 0.12;
        // D-4 / G1 S3 (pierce33): the charged KESTREL's extra camera kick on its kestrelPierce33 beam, x the KESTREL's own camera punch
        // (viewmodel.weapons.revolver punch pitch); the normal shot's punch comes on top from the `fire` event
        public const double PierceKickMult33 = 2.5;
        // the camera trauma the piercing beam adds (0..1; ShakeFireHeavy is 0.42, the heaviest shot class)
        public const double PierceTrauma33 = 0.35;

        // The whip's rope (view33; Arcline21View.Rope33.cs), the web's fx/rope.js literals (it keeps them in its call sites, rule 5):
        // :84 the slack in flight (x the hand-to-hook distance) and the extra slack of an attached rope that is not taut (m)
        public const float Rope33FlySlack = 0.08f, Rope33LooseSlack = 0.3f;
        // :86 the sag: mid.y -= min(SagMax, slack x arcline.sag x SagScale)
        public const float Rope33SagMax = 6f, Rope33SagScale = 2f;
        // :87 the rope's colour x these: taut / slack (additive, so it glows)
        public const float Rope33TautGlow = 2.2f, Rope33SlackGlow = 1.5f;
        // :33 the hook head (an OctahedronGeometry of radius 0.12 there; here a cube on its corner of this edge, m) and its colour x 3
        public const float Rope33HookEdge = 0.17f, Rope33HookGlow = 3f;
        // :107 the hook head's spin in flight (rad/s)
        public const float Rope33HookSpin = 12f;
        // :80 the web's left-hand point when there is no WeaponFx muzzle: left, down, ahead of the eye (m)
        public const float Rope33HandLeft = 0.22f, Rope33HandDown = 0.2f, Rope33HandAhead = 0.45f;
        // Unity-only (D-3 "a snap on release"): the rope whips back to the muzzle in this many s, with a sideways wave of this amplitude (m) dying out
        public const float Rope33SnapTime = 0.12f, Rope33SnapWave = 0.25f;

        // The gun feel with view33 on (DESIGN33 W "Viewmodel"; G1 1 and S1; Viewmodel.View33.cs GunFeel33). The web's layers stay (breath, bob, look sway,
        // landing dip, the class / per-gun recoil); these reshape them.
        // The viewmodel camera's FOV (deg; the web's viewmodel.fovDeg is 54; D-5 / G1: 54-60, CS2's default 60, TF2's 54): a little wider so the gun reads
        // smaller and out of the sight line
        public const float ViewmodelFov33 = 58f;
        // Idle sway (G1 convention: a slow 0.2-0.5 Hz figure-eight of ~1-3 mm): x and roll at IdleSwayHz33, y at twice it (the figure-eight); faded out
        // by the movement bob (only a still gun sways)
        public const double IdleSwayHz33 = 0.3, IdleSwayX33 = 0.0025, IdleSwayY33 = 0.0012, IdleSwayRollDeg33 = 0.35;
        // The speed-scaled bob: the web caps the bob's scale at viewmodel.bobMaxScale (1.5 = 12 m/s at walkSpeed 8); with view33 it keeps growing with
        // speed up to this (2.0 = 16 m/s), so a skate reads faster than a run
        public const double BobMaxScale33 = 2.0;
        // The landing dip (the web's: down only, min(landingDipMax, landingDipPerSpeed x fall speed) x landDipScale): with view33 the gun also comes back
        // toward the eye by LandDipBack33 x the dip and tips down LandDipPitch33 deg per m of dip (a damped down-and-back, G1)
        public const double LandDipBack33 = 0.6, LandDipPitch33 = 60;
        // The recoil spring's clock rate per weight class with view33 (G1: a spring kick, fast attack, 80-150 ms recovery; the web's class springs
        // settle in about 156 / 230 / 329 ms to 2 %: light 8 Hz z 0.5, medium 6 Hz z 0.45, heavy 4.5 Hz z 0.42): the same peak, settled in
        // ~111 / 121 / 137 ms. Applies to the class recoil and the per-gun kick bank alike.
        public const double RecoilRateLight33 = 1.4, RecoilRateMedium33 = 1.9, RecoilRateHeavy33 = 2.4;

        // The player's muzzle flashes and impact sparks with view33 (WeaponFx; builder L's ask, CLAUDE.md rule 8: red-orange is enemies and danger only):
        // a player colour whose hue is red-orange-amber (at most WarmHueMax33 deg, or at least WarmHueWrap33) and saturation over WarmSat33 becomes
        // white-hot (the THUNDERCLAP's edge #ffb000, colors.muzzleFlash #ffe7b8, colors.spark #ffd27a, the fire family); cyan and the cool families stay
        public const float WarmHueMax33 = 50f, WarmHueWrap33 = 330f, WarmSat33 = 0.15f;
        public const uint WhiteHotHex33 = 0xfff6ec;
        // The muzzle star with view33: brighter and a touch tighter (short, bright, at the muzzle); its frame count stays fx.flashFrames
        public const float StarBright33 = 1.45f, StarSize33 = 0.85f;
        // The impact spark on every player gun hit on an enemy (view33; the web drew one only for melee, gunFx.js :120-125): count, speed (m/s), life (s),
        // cube scale; white-hot, the gun's cool family colour on the second half
        public const int HitSparkCount33 = 5;
        public const float HitSparkSpeed33 = 7f, HitSparkLife33 = 0.12f, HitSparkScale33 = 0.7f;
    }
}
