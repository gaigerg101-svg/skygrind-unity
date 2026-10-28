// NIGHT39 builder P1 (the pistol; NIGHT39.md P1, GG 2026-10-13: the pistol "should feel heavier, slower, and more impactful when you shoot", "a cooler,
// more powerful-looking animation when you fire the special shot"): the KESTREL's heavier look. View only: installs itself (Doors38View's pattern; no edit
// in the lead's SurvivalGame files, none in CameraFeel or the Viewmodel), live only while design.n39.p39 is live in the run (SurvivalSim.N39.Pistol.cs;
// off with -skygrindOff39), and only while the KESTREL is in hand. Unity objects are made on first use (never in field initialisers: L-035).
// It runs in LateUpdate, after SurvivalGame.Update has posed the camera and the viewmodel, and adds on top of them (undone next frame if nothing
// re-posed them, so nothing accumulates on the recap or a menu frame):
// - the TAP ("fire" revolver): an extra gun kick (up, back, a wrist roll) with a long settle that dips once below rest, a small view kick, a bigger
//   cyan muzzle bloom (WeaponFx's star still draws);
// - the PRESS (p39Press: a press that may become a charge fires nothing yet): the hammer cocks on the press frame (a pull back, a dip), so the
//   press is seen at once (INVERSUS's instant press-time cue; research/feel-web.md section 3);
// - the CANNON (charge35Fire on the KESTREL: the charged shot): a ~2.5x gun kick settling over 0.6 s, a view kick, a FOV punch-out, extra trauma,
//   a muzzle blast, a shock ring along the shot, and a thick beam with a hot core from the muzzle to the shot's end (kestrelPierce33's `to`);
// - the HIT FREEZES (design.n39.p39.hitS / chargedHitS, gameplay numbers): game.Hitstop on a KESTREL body hit and on a charged shot that hit
//   (the web's freeze: the longest request wins; loop pacing the sim and the tape never see).
// Camera kicks are x CameraFeel.ShakeScale (CAMERA SHAKE) and none with REDUCED MOTION; the FOV punch is x CameraFeel.FovKickScale (FOV KICK).
// Colours: cyan (colors.usable): the KESTREL is the player's. Events (names only, GG's sounds; CLAUDE.md rule 7): p39Press (the sim's), and the
// counters below for the reels. Numbers: UnityTuning.Pistol39.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT39 P1: Pistol39View's shapes (pure).</summary>
    public static class Pistol39Math
    {
        /// <summary>A kick's weight at age t (s): 0 before, up to 1 over `attack` (smoothstep), then back to 0 over `settle` with one small dip below
        /// rest (cos(1.5 pi u) x (1 - u)^2: the dip is ~15 % at u 0.6), 0 after. A heavy gun's snap and slow, weighted return.</summary>
        public static double Settle(double t, double attack, double settle)
        {
            if (t < 0 || double.IsInfinity(t) || double.IsNaN(t)) return 0;
            if (t < attack) { double a = t / attack; return a * a * (3 - 2 * a); }
            if (settle <= 0) return 0;
            double u = (t - attack) / settle;
            if (u >= 1) return 0;
            double k = 1 - u;
            return System.Math.Cos(u * System.Math.PI * 1.5) * k * k;
        }

        /// <summary>NIGHT40 W1 (the pistol in its rhythm): a kick's weight at age t (s): up to 1 over `attack` (smoothstep), then an ease-out back to
        /// rest over `settle` ((1 - u)^2: fast off the peak, slow into rest, no dip), 0 from attack + settle on.</summary>
        public static double EaseOut(double t, double attack, double settle)
        {
            if (t < 0 || double.IsInfinity(t) || double.IsNaN(t)) return 0;
            if (t < attack) { double a = t / attack; return a * a * (3 - 2 * a); }
            if (settle <= 0) return 0;
            double u = (t - attack) / settle;
            if (u >= 1) return 0;
            return (1 - u) * (1 - u);
        }

        /// <summary>NIGHT40 W1: the crosshair turn's share 0..1 at age t of a turn lasting `spinS` (ease-out: 1 - (1 - u)^2); 1 from spinS on. For
        /// H1's re-timed KESTREL spin (UnityTuning.Pistol40TapSpinS / Pistol40CannonSpinS).</summary>
        public static double SpinShare(double t, double spinS)
        {
            if (t <= 0 || spinS <= 0) return t > 0 ? 1 : 0;
            double u = t / spinS;
            if (u >= 1) return 1;
            return 1 - (1 - u) * (1 - u);
        }

        /// <summary>A flash's weight at age t (s) over `life`: 1 at once, linear to 0.</summary>
        public static double Fade(double t, double life) => t < 0 || t >= life || life <= 0 ? 0 : 1 - t / life;
    }

    public sealed class Pistol39View : MonoBehaviour
    {
        /// <summary>For the reels: taps, presses, cannon shots and hit freezes seen while live this session; the largest gun kick (deg) drawn.</summary>
        public static int Taps, Presses, Cannons, HitFreezes, CannonFreezes;
        public static float KickDegMax;

        /// <summary>Is p39 live in the current run (the run's design block on and charge35's object in place)?</summary>
        public static bool On;

        /// <summary>NIGHT40 W1 (charge40 live in the run: Charge35.c40): the KESTREL's rhythm timings (UnityTuning.Pistol40.cs) and the slug's look.
        /// The reels' frame log: taps measured, the last tap's ends (s after the shot: the gun kick, the view kick, the hammer's re-cock), how many
        /// ended past Pistol40Share of the interval, slugs drawn.</summary>
        public static bool Rhythm40On;
        public static int Rhythm40Taps, Rhythm40Late, Slugs40;
        public static double Rhythm40KickEndS = -1, Rhythm40CamEndS = -1, Rhythm40HammerEndS = -1, Rhythm40IntervalS;

        SurvivalGame game;
        EventBus wiredTo;
        double hitS, chargedHitS;
        double tapAge = double.PositiveInfinity, cockAge = double.PositiveInfinity, cannonAge = double.PositiveInfinity;
        double muzzleAge = double.PositiveInfinity, blastAge = double.PositiveInfinity, beamAge = double.PositiveInfinity;
        Vec3 lastEnd;
        bool lastEndSet;
        // NIGHT40 W1: the slug's ages; the tap's rhythm log (age since the tap, the ends seen, whether this tap's line is still open)
        double slugAge = double.PositiveInfinity, slugBlastAge = double.PositiveInfinity;
        double logAge = double.PositiveInfinity, logKick = -1, logCam = -1, logHammer = -1;
        bool logOpen, logHammerSeen;
        Vector3 beamTo;

        // what this view added last frame (undone when nothing re-posed the camera / the gun since)
        Transform vmRoot;
        Vector3 rootWrittenPos, rootBasePos;
        Quaternion rootWrittenRot, rootBaseRot;
        bool rootWritten;
        Quaternion camWrittenRot, camBaseRot;
        float camWrittenFov, camBaseFov;
        bool camWritten;

        Transform fx;
        Renderer muzzle, blast, ring, beam, core;
        Material mat;
        MaterialPropertyBlock mpb;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Pistol39View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Pistol39View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null || game.sim == null || game.viewmodel == null) { game = null; return false; }
            hitS = game.cfg.Has("design.n39.p39.hitS") ? game.cfg.Num("design.n39.p39.hitS") : 0;
            chargedHitS = game.cfg.Has("design.n39.p39.chargedHitS") ? game.cfg.Num("design.n39.p39.chargedHitS") : 0;
            mpb ??= new MaterialPropertyBlock();
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Pistol 39");
            if (fx == null)
            {
                fx = new GameObject("Pistol39 fx").transform;
                fx.SetParent(transform, false);
                muzzle = Part("muzzle", PrimitiveType.Sphere);
                blast = Part("blast", PrimitiveType.Sphere);
                ring = Part("ring", PrimitiveType.Cylinder);
                beam = Part("beam", PrimitiveType.Cylinder);
                core = Part("core", PrimitiveType.Cylinder);
            }
            else foreach (Renderer r in new[] { muzzle, blast, ring, beam, core }) r.sharedMaterial = mat;
            wiredTo = null;
            vmRoot = null;
            rootWritten = camWritten = false;
            return true;
        }

        Renderer Part(string name, PrimitiveType shape)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.name = name;
            Collider c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
            go.transform.SetParent(fx, false);
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return r;
        }

        bool Live => game != null && game.sim != null && game.sim.charge35 != null && game.sim.charge35.p39;
        /// <summary>NIGHT40 W1: charge40 live in the run (the KESTREL's rhythm and the slug's look).</summary>
        bool Live40 => game != null && game.sim != null && game.sim.charge35 != null && game.sim.charge35.c40;

        /// <summary>A kick's weight: NIGHT40's ease-out with charge40 live (Pistol40EaseOut), NIGHT39's settle with its dip otherwise.</summary>
        static double Kick(bool r40, double t, double attack, double settle) =>
            r40 && UnityTuning.Pistol40EaseOut ? Pistol39Math.EaseOut(t, attack, settle) : Pistol39Math.Settle(t, attack, settle);

        void Wire()
        {
            if (wiredTo == game.events) return;
            wiredTo = game.events;
            EventBus ev = game.events;
            ev.On("fire", o =>
            {
                if (!(o is string w) || w != "revolver" || !Live) return;
                tapAge = 0;
                muzzleAge = 0;
                Taps++;
                if (Live40) { logAge = 0; logOpen = true; logKick = logCam = logHammer = -1; logHammerSeen = false; } // NIGHT40 W1: this tap's rhythm line
            });
            ev.On<ChargeShot27Event>("p39Press", e =>
            {
                if (!Live) return;
                cockAge = 0;
                Presses++;
            });
            ev.On<ShotEvent>("shotResult", e =>
            {
                if (e == null || e.owner != "player" || e.weapon != "revolver") return;
                lastEnd = e.end;
                lastEndSet = true;
            });
            ev.On<RailFireEvent>(Loadout.KestrelPierce33, e =>
            {
                if (e == null || e.owner != "player") return;
                lastEnd = e.to;
                lastEndSet = true;
            });
            ev.On<HitEvent>("hit", e =>
            {
                if (!Live || e == null || e.owner != "player" || e.weapon != "revolver" || e.melee || hitS <= 0) return;
                game.Hitstop(hitS, "p39Hit");
                HitFreezes++;
            });
            ev.On<ChargeShot27Event>("charge35Fire", e =>
            {
                if (e != null && e.gun == "shotgun" && Live40)
                {
                    // NIGHT40 W1: the slug's look (a heavier kick, a view kick and FOV punch, a blast, a wide ring; WeaponFx's slug line still draws)
                    slugAge = 0;
                    slugBlastAge = 0;
                    game.trauma.Add(UnityTuning.Slug40Trauma);
                    Slugs40++;
                    return;
                }
                if (!Live || e == null || e.gun != "revolver") return;
                logOpen = false; // the cannon replaces the tap's kick: no rhythm line for it
                cannonAge = 0;
                blastAge = 0;
                tapAge = double.PositiveInfinity; // the cannon's kick replaces the tap's (a charged shot may also emit "fire")
                muzzleAge = double.PositiveInfinity;
                beamAge = lastEndSet ? 0 : double.PositiveInfinity;
                if (lastEndSet) beamTo = WebSpace.ToUnity(lastEnd);
                lastEndSet = false;
                game.trauma.Add(UnityTuning.Pistol39CannonTrauma);
                Cannons++;
                if (e.hits > 0 && chargedHitS > 0)
                {
                    game.Hitstop(chargedHitS, "p39Cannon");
                    CannonFreezes++;
                }
            });
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            Wire();
            On = Live;
            bool r40 = Rhythm40On = On && Live40; // NIGHT40 W1: charge40 live: the rhythm timings (UnityTuning.Pistol40.cs)
            double dt = Mathf.Min(FeelClock.UnscaledDelta, 0.1f); // NIGHT63 D1: 0 in a same-run held frame
            tapAge += dt;
            cockAge += dt;
            cannonAge += dt;
            muzzleAge += dt;
            blastAge += dt;
            beamAge += dt;
            slugAge += dt;
            slugBlastAge += dt;
            logAge += dt;
            Camera cam = game.cam;
            UndoLast(cam);
            string inHand = game.loadout != null ? game.loadout.Weapon : null;
            bool kestrel = On && inHand == "revolver";
            if (!kestrel)
            {
                // NIGHT40 W1: the slug's look on the THUNDERCLAP (charge40 live)
                if (Live40 && inHand == "shotgun" && SlugFrame(cam)) return;
                Hide();
                return;
            }
            // --- the gun: the tap, the cock and the cannon on top of the viewmodel's own pose (NIGHT40: ease-out, the tap re-timed, with charge40 live)
            double tap = r40 ? Kick(true, tapAge, UnityTuning.Pistol40KickAttackS, UnityTuning.Pistol40KickSettleS)
                : Pistol39Math.Settle(tapAge, UnityTuning.Pistol39KickAttackS, UnityTuning.Pistol39KickSettleS);
            double cock = Kick(r40, cockAge, UnityTuning.Pistol39CockAttackS, UnityTuning.Pistol39CockSettleS);
            double can = Kick(r40, cannonAge, UnityTuning.Pistol39CannonAttackS, UnityTuning.Pistol39CannonSettleS);
            float pitch = (float)(UnityTuning.Pistol39KickPitchDeg * tap + UnityTuning.Pistol39CockPitchDeg * cock + UnityTuning.Pistol39CannonPitchDeg * can);
            float back = (float)(UnityTuning.Pistol39KickBack * tap + UnityTuning.Pistol39CockBack * cock + UnityTuning.Pistol39CannonBack * can);
            float roll = (float)(UnityTuning.Pistol39KickRollDeg * tap + UnityTuning.Pistol39CannonRollDeg * can);
            if (pitch > KickDegMax) KickDegMax = pitch;
            if (vmRoot == null && game.viewmodel.Camera != null) vmRoot = game.viewmodel.Camera.transform.Find("Viewmodel");
            if (vmRoot != null && (pitch != 0 || back != 0 || roll != 0))
            {
                rootBasePos = vmRoot.localPosition;
                rootBaseRot = vmRoot.localRotation;
                vmRoot.localPosition = rootBasePos + new Vector3(0, 0, -back);
                vmRoot.localRotation = rootBaseRot * Quaternion.Euler(-pitch, 0, roll); // Unity: -x pitches up; +z turns clockwise as the eye sees it
                rootWrittenPos = vmRoot.localPosition;
                rootWrittenRot = vmRoot.localRotation;
                rootWritten = true;
            }
            // --- the view: the kicks (x CAMERA SHAKE, none with REDUCED MOTION) and the cannon's FOV punch (x FOV KICK)
            if (cam != null)
            {
                bool still = game.cfg.movement.settings.reducedMotion;
                double shake = still ? 0 : CameraFeel.ShakeScale(game.settings);
                double camTap = r40 ? Kick(true, tapAge, UnityTuning.Pistol40CamAttackS, UnityTuning.Pistol40CamSettleS)
                    : Pistol39Math.Settle(tapAge, UnityTuning.Pistol39CamAttackS, UnityTuning.Pistol39CamSettleS);
                double camCan = Kick(r40, cannonAge, UnityTuning.Pistol39CannonCamAttackS, UnityTuning.Pistol39CannonCamSettleS);
                if (r40) Rhythm40Log(tap, camTap);
                float camPitch = (float)((UnityTuning.Pistol39CamPitchDeg * camTap + UnityTuning.Pistol39CannonCamDeg * camCan) * shake);
                float fovKick = (float)(UnityTuning.Pistol39CannonFovDeg * Pistol39Math.Fade(cannonAge, UnityTuning.Pistol39CannonFovS) * CameraFeel.FovKickScale(game.settings));
                if (camPitch != 0 || fovKick != 0)
                {
                    camBaseRot = cam.transform.rotation;
                    camBaseFov = cam.fieldOfView;
                    cam.transform.rotation = camBaseRot * Quaternion.Euler(-camPitch, 0, 0);
                    cam.fieldOfView = camBaseFov + fovKick;
                    camWrittenRot = cam.transform.rotation;
                    camWrittenFov = cam.fieldOfView;
                    camWritten = true;
                }
            }
            // --- the fx at the muzzle and along the shot
            if (!game.viewmodel.MuzzleWorld(out Vector3 at))
            {
                Hide();
                return;
            }
            Color cyan = Look.C(game.cfg, "colors.usable");
            Vector3 fwd = cam != null ? cam.transform.forward : Vector3.forward;
            // NIGHT49 X fx49 round 2 (D-17; the lead's a249 on n49-r1: kestrel-charge's WASH box +0.51 OFF / +0.52 ON): the cannon on the kit.
            // Its muzzle blast (a 0.75 m ball at glow 3 half a metre from the eye) and its shock ring (to 2.6 m) covered the middle of the screen:
            // with fx49 their span is capped by Fx49.WashCap (their edge outside the wash box), their glow the kit's critical; the beam down the aim
            // is thinner and dimmer (Fx49Beam*) and fades on the critical timing. Off: as it was.
            bool k49 = Fx49.On(game.cfg);
            float cap49 = k49 ? Fx49.WashCap(cam, at) : float.PositiveInfinity;
            float crit49 = Fx49.Cell(Fx49Owner.Player, Fx49Tier.Critical).glow;
            Ball(muzzle, at, Mathf.Min(UnityTuning.Pistol39MuzzleSize, cap49), Pistol39Math.Fade(muzzleAge, UnityTuning.Pistol39MuzzleLifeS), cyan * (k49 ? Mathf.Min(UnityTuning.Pistol39MuzzleGlow, crit49) : UnityTuning.Pistol39MuzzleGlow));
            Ball(blast, at, Mathf.Min(UnityTuning.Pistol39BlastSize, cap49), Pistol39Math.Fade(blastAge, UnityTuning.Pistol39BlastLifeS), cyan * (k49 ? crit49 : UnityTuning.Pistol39BlastGlow));
            Vector3 dir = fwd;
            float len = 0;
            bool beamOn = beamAge < UnityTuning.Pistol39BeamLifeS;
            if (beamOn)
            {
                Vector3 d = beamTo - at;
                len = Mathf.Min(d.magnitude, UnityTuning.Pistol39BeamMaxLen);
                if (len > 0.01f) dir = d / d.magnitude;
            }
            // the shock ring: a thin disc facing along the shot, growing as it fades
            double rf = Pistol39Math.Fade(blastAge, UnityTuning.Pistol39RingLifeS);
            if (rf > 0)
            {
                float u = (float)(1 - rf);
                float dia = Mathf.Lerp(UnityTuning.Pistol39RingFrom, UnityTuning.Pistol39RingTo, 1 - (1 - u) * (1 - u));
                Vector3 ringAt = at + dir * (0.4f * u);
                if (k49) dia = Mathf.Min(dia, Fx49.WashCap(cam, ringAt)); // fx49: the ring stays round the muzzle, its edge outside the wash box
                ring.transform.SetPositionAndRotation(ringAt, Quaternion.FromToRotation(Vector3.up, dir));
                ring.transform.localScale = new Vector3(dia, UnityTuning.Pistol39RingThick, dia);
                Paint(ring, cyan * ((k49 ? Fx49.Cell(Fx49Owner.Player, Fx49Tier.Heavy).glow : UnityTuning.Pistol39RingGlow) * (float)rf));
                ring.enabled = dia > 0.01f;
            }
            else ring.enabled = false;
            // the beam: a cyan sleeve and a hot core from the muzzle to the shot's end, thinning out
            double bf = beamOn ? Pistol39Math.Fade(beamAge, UnityTuning.Pistol39BeamLifeS) : 0;
            if (k49) bf = beamOn ? Fx49.Env(Fx49Tier.Critical, (float)beamAge) : 0; // fx49: the kit's critical timing (snap, hold 6 steps, fade 32)
            if (bf > 0 && len > 0.01f)
            {
                float sw = UnityTuning.Pistol39BeamWidth * (k49 ? UnityTuning.Fx49BeamSleeveWidth : 1f), cw = UnityTuning.Pistol39BeamCoreWidth * (k49 ? UnityTuning.Fx49BeamCoreWidth : 1f);
                float sg = k49 ? crit49 * UnityTuning.Fx49BeamSleeveShare : UnityTuning.Pistol39BeamGlow, cg = k49 ? crit49 : UnityTuning.Pistol39BeamCoreGlow;
                Rod(beam, at, dir, len, sw * (float)bf, cyan * (sg * (float)bf));
                Rod(core, at, dir, len, cw * (float)bf, cyan * (cg * (float)bf));
            }
            else beam.enabled = core.enabled = false;
        }

        /// <summary>NIGHT40 W1: the tap's rhythm line (the reels' frame log): the frame time from the shot to the end of the gun kick, the view kick
        /// and the hammer's re-cock (Viewmodel.Hammer36Deg back to 0 after it fell), against Pistol40Share x the KESTREL's interval. One Debug.Log
        /// line per tap ("rhythm40 tap ...") once all three ended (or 1 s passed).</summary>
        void Rhythm40Log(double tap, double camTap)
        {
            if (!logOpen) return;
            if (Rhythm40IntervalS <= 0)
            {
                double dtS = game.cfg.Dt, mult = game.cfg.Has("design.n39.p39.intervalMult") ? game.cfg.Num("design.n39.p39.intervalMult") : 1;
                Rhythm40IntervalS = MovementMath.StepsFor(game.cfg.Num("weapons.revolver.fireInterval") * mult, dtS) * dtS;
            }
            if (logKick < 0 && logAge > UnityTuning.Pistol40KickAttackS && tap <= 0) logKick = logAge;
            if (logCam < 0 && logAge > UnityTuning.Pistol40CamAttackS && camTap <= 0) logCam = logAge;
            if (Viewmodel.Hammer36Deg > 0) logHammerSeen = true;
            else if (logHammer < 0 && (logHammerSeen || Viewmodel.Hammer36Deg < 0)) logHammer = logHammerSeen ? logAge : 0; // -1: no hammer drawn
            bool done = logKick >= 0 && logCam >= 0 && logHammer >= 0;
            if (!done && logAge < 1) return;
            logOpen = false;
            double cap = UnityTuning.Pistol40Share * Rhythm40IntervalS;
            bool late = logKick < 0 || logKick > cap || logCam < 0 || logCam > cap || logHammer > cap;
            Rhythm40Taps++;
            if (late) Rhythm40Late++;
            Rhythm40KickEndS = logKick;
            Rhythm40CamEndS = logCam;
            Rhythm40HammerEndS = logHammer;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            Debug.Log($"rhythm40 tap {Rhythm40Taps}: gun kick ends +{logKick.ToString("0.000", ci)} s, view kick +{logCam.ToString("0.000", ci)} s, hammer re-cocked +{logHammer.ToString("0.000", ci)} s; interval {Rhythm40IntervalS.ToString("0.000", ci)} s, cap {cap.ToString("0.000", ci)} s ({UnityTuning.Pistol40Share:0.00}): {(late ? "LATE" : "inside")}");
        }

        /// <summary>NIGHT40 W1: the slug's look on the THUNDERCLAP while it plays (charge40 live): an extra gun kick, a view kick and FOV punch, a cyan
        /// blast at the muzzle and a wide shock ring along the look. False when nothing of it is left to draw.</summary>
        bool SlugFrame(Camera cam)
        {
            double kick = Pistol39Math.EaseOut(slugAge, UnityTuning.Slug40KickAttackS, UnityTuning.Slug40KickSettleS);
            double blastW = Pistol39Math.Fade(slugBlastAge, UnityTuning.Slug40BlastLifeS);
            double rf = Pistol39Math.Fade(slugBlastAge, UnityTuning.Slug40RingLifeS);
            double camW = Pistol39Math.EaseOut(slugAge, UnityTuning.Slug40CamAttackS, UnityTuning.Slug40CamSettleS);
            double fovW = Pistol39Math.Fade(slugAge, UnityTuning.Slug40FovS);
            if (kick <= 0 && blastW <= 0 && rf <= 0 && camW <= 0 && fovW <= 0) return false;
            float pitch = (float)(UnityTuning.Slug40KickPitchDeg * kick), back = (float)(UnityTuning.Slug40KickBack * kick), roll = (float)(UnityTuning.Slug40KickRollDeg * kick);
            if (vmRoot == null && game.viewmodel.Camera != null) vmRoot = game.viewmodel.Camera.transform.Find("Viewmodel");
            if (vmRoot != null && (pitch != 0 || back != 0 || roll != 0))
            {
                rootBasePos = vmRoot.localPosition;
                rootBaseRot = vmRoot.localRotation;
                vmRoot.localPosition = rootBasePos + new Vector3(0, 0, -back);
                vmRoot.localRotation = rootBaseRot * Quaternion.Euler(-pitch, 0, roll);
                rootWrittenPos = vmRoot.localPosition;
                rootWrittenRot = vmRoot.localRotation;
                rootWritten = true;
            }
            if (cam != null)
            {
                bool still = game.cfg.movement.settings.reducedMotion;
                double shake = still ? 0 : CameraFeel.ShakeScale(game.settings);
                float camPitch = (float)(UnityTuning.Slug40CamDeg * camW * shake);
                float fovKick = (float)(UnityTuning.Slug40FovDeg * fovW * CameraFeel.FovKickScale(game.settings));
                if (camPitch != 0 || fovKick != 0)
                {
                    camBaseRot = cam.transform.rotation;
                    camBaseFov = cam.fieldOfView;
                    cam.transform.rotation = camBaseRot * Quaternion.Euler(-camPitch, 0, 0);
                    cam.fieldOfView = camBaseFov + fovKick;
                    camWrittenRot = cam.transform.rotation;
                    camWrittenFov = cam.fieldOfView;
                    camWritten = true;
                }
            }
            muzzle.enabled = beam.enabled = core.enabled = false;
            if (!game.viewmodel.MuzzleWorld(out Vector3 at))
            {
                blast.enabled = ring.enabled = false;
                return true;
            }
            Color cyan = Look.C(game.cfg, "colors.usable");
            Vector3 dir = cam != null ? cam.transform.forward : Vector3.forward;
            Ball(blast, at, UnityTuning.Slug40BlastSize, blastW, cyan * UnityTuning.Slug40BlastGlow);
            if (rf > 0)
            {
                float u = (float)(1 - rf);
                float dia = Mathf.Lerp(UnityTuning.Slug40RingFrom, UnityTuning.Slug40RingTo, 1 - (1 - u) * (1 - u));
                ring.transform.SetPositionAndRotation(at + dir * (0.6f * u), Quaternion.FromToRotation(Vector3.up, dir));
                ring.transform.localScale = new Vector3(dia, UnityTuning.Slug40RingThick, dia);
                Paint(ring, cyan * (UnityTuning.Slug40RingGlow * (float)rf));
                ring.enabled = true;
            }
            else ring.enabled = false;
            return true;
        }

        /// <summary>Takes back last frame's additions if nothing re-posed the gun / the camera since (the recap, a menu frame).</summary>
        void UndoLast(Camera cam)
        {
            if (rootWritten && vmRoot != null && vmRoot.localPosition == rootWrittenPos && vmRoot.localRotation == rootWrittenRot)
            {
                vmRoot.localPosition = rootBasePos;
                vmRoot.localRotation = rootBaseRot;
            }
            rootWritten = false;
            if (camWritten && cam != null && cam.transform.rotation == camWrittenRot && Mathf.Approximately(cam.fieldOfView, camWrittenFov))
            {
                cam.transform.rotation = camBaseRot;
                cam.fieldOfView = camBaseFov;
            }
            camWritten = false;
        }

        void Ball(Renderer r, Vector3 at, float size, double w, Color c)
        {
            if (w <= 0) { r.enabled = false; return; }
            r.transform.SetPositionAndRotation(at, Quaternion.identity);
            float s = size * (0.6f + 0.4f * (float)w);
            r.transform.localScale = new Vector3(s, s, s);
            Paint(r, c * (float)w);
            r.enabled = true;
        }

        void Rod(Renderer r, Vector3 from, Vector3 dir, float len, float width, Color c)
        {
            r.transform.SetPositionAndRotation(from + dir * (len * 0.5f), Quaternion.FromToRotation(Vector3.up, dir));
            r.transform.localScale = new Vector3(width, len * 0.5f, width); // the primitive cylinder is 2 m tall
            Paint(r, c);
            r.enabled = true;
        }

        void Paint(Renderer r, Color c)
        {
            mpb.SetColor(BaseColor, c);
            r.SetPropertyBlock(mpb);
        }

        void Hide()
        {
            if (muzzle == null) return;
            muzzle.enabled = blast.enabled = ring.enabled = beam.enabled = core.enabled = false;
        }
    }
}
