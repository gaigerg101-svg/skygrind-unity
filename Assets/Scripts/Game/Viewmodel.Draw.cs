// NIGHT16 U3-29.6 (builder V): a draw for every gun (port-only: the web has one drop-and-raise curve for all of them).
// The sim's swap timer gates everything (Combat.StepSwap: total = StepsFor(swapTime / swapMult), the gun changes at
// `lower`, fire opens from swapFireFraction of the raise); a draw lives inside it and never gates it:
// - the holster (the lowering half) drops the gun as before (viewmodel.swapDropY / swapPitchDeg) and tips it toward the
//   right hip;
// - the raise is remapped so the gun is at its hold pose on the step fire opens (the web's curve left it ~6 deg low
//   there): the drawn lowered amount runs from 1 at the swap's bottom to 0 at the raise share `openU` where the sim's
//   SwapCanFire turns true; any raise signature rides on that amount, so it is 0 there too;
// - the signature lives in the SETTLE after fire opens (the web's NIGHT5 settle on the class spring, now closed-form here,
//   plus each gun's flourish); the settle's clock is the sim's own (the raise share) while the swap runs, then world
//   time; a shot cancels it on the same frame (Viewmodel.OnFire -> HandsDrawState.Cancel), so the recoil takes over;
// - the respawn / run-start raise (viewmodel.respawnRaiseTime) is the same draw, up at its end.
// Signatures (UnityTuning.Hands.cs): the KESTREL flicks up and spins its cylinder a whole turn; the THUNDERCLAP racks its
// pump; the COMET comes up off the shoulder and seats on its heavy spring; the LONGWAVE's charge glow sweeps along it; the
// PULSE GAUNTLET rises as it clenches, its gem flaring; the rest by their weight class (the class spring's settle).
// Pure and allocation-free (no UnityEngine call): HandsTests runs it without a scene; HandsDraw.WriteTrace writes the
// pose per frame (Logs/n16-vm-trace.csv), the web's scripts/feelTrace.mjs in spirit.
// NIGHT17 U3-30.6: the holster yanks (HandsEase.OutSine from the press: the gun moves on the first frame); the raise is an
// ease-out-back (HandsEase.OutBack, the weight class's overshoot: the gun is up within ~35 ms of the change, goes past
// hold and lands exactly at hold on the fire step); a shot in the respawn raise speeds its clock (HandsRaiseYieldSpeed);
// HandsDrawState.fromSwap / QuickdrawAge give QUICKDRAW's window (Viewmodel.Feel.cs).
// NIGHT17 round 1 (build 2's strips: ~100 ms of empty hands per swap): the view is fed the loadout's TARGET from the press;
// the gun on screen flicks out (linear, 55 ms, to its off-screen line) and the wanted gun rises from its own off-screen
// line on the sim's step clock to hold on the fire step (HandsDrawState: the swap's turns re-plan on the sim's mirror).
using System;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The view's half of the draw (the pure maths are HandsDraw / HandsDrawState below).</summary>
    public sealed partial class Viewmodel
    {
        readonly HandsDrawState drawState = new HandsDrawState();
        HandsSwapTimeline drawTimeline;
        bool bendHands34, bendHandsRead34; // NIGHT34 H (design.n34.bendLook): the hands snap inside a bend
        double bendSnapAge34 = double.MaxValue; // a plain double: safe in an initialiser (no Unity API, L-035)
        /// <summary>For the reels (hud34 / bend34): the hands' bend snaps finished since the start.</summary>
        public static int BendSnaps34;
        double drawTimelineMult = double.NaN;
        double drawSwapTime, drawLowerFraction, drawFireFraction; // weapons.swapTime / swapLowerFraction / swapFireFraction, read once
        HandsDrawPose drawPose;
        readonly HandsGlowGate drawGlowGate = new HandsGlowGate();
        double drawGlow; // the draw's glow as drawn: x flashIntensity, gated (the reviewer's finding 7)
        Transform drawBand; // the LONGWAVE's sweep band (port-only), on the rail rig
        Material drawBandMat;
        double drawGlintMin, drawGlintTime, drawGlintGain; // viewmodel.states.glintMinCooldown / glintTime / glintGain

        void DrawBuild()
        {
            drawSwapTime = cfg.Num("weapons.swapTime");
            drawLowerFraction = cfg.Num("weapons.swapLowerFraction");
            drawFireFraction = cfg.Num("weapons.swapFireFraction");
            drawGlintMin = cfg.Num("viewmodel.states.glintMinCooldown");
            drawGlintTime = cfg.Num("viewmodel.states.glintTime");
            drawGlintGain = cfg.Num("viewmodel.states.glintGain");
            foreach (Rig r in rigList)
            {
                // viewmodel.js :367 fullCooldown = weapons[name].cooldown ?? weapons[name].fireInterval ?? 0
                r.glintFull = cfg.Has($"weapons.{r.name}.cooldown") ? cfg.Num($"weapons.{r.name}.cooldown")
                    : cfg.Has($"weapons.{r.name}.fireInterval") ? cfg.Num($"weapons.{r.name}.fireInterval") : 0;
            }
            if (rigs.TryGetValue("rail", out Rig rail))
            {
                var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(band.GetComponent<Collider>());
                band.name = "draw sweep";
                band.layer = Layer;
                band.transform.SetParent(rail.t, false);
                band.transform.localScale = new Vector3(UnityTuning.HandsLongwaveBandW, UnityTuning.HandsLongwaveBandH, UnityTuning.HandsLongwaveBandD);
                drawBandMat = FxDraw.Additive(game.mats.neon, "draw sweep"); // round 5: additive: it fades to nothing (an opaque band went dark as its glow fell)
                var rend = band.GetComponent<Renderer>();
                rend.sharedMaterial = drawBandMat;
                rend.shadowCastingMode = ShadowCastingMode.Off;
                band.SetActive(false);
                drawBand = band.transform;
            }
        }

        bool drawQuick; // this draw's gun fired within UnityTuning.HandsQuickDrawWindow before it started
        double drawSlamW; // 0..1 the slam's wind-up

        /// <summary>The frame's draw: the sim's lowered amount (interpolated) through the gun's draw; weaponDraw on a start.</summary>
        // round 2: the swap's overlap: the outgoing gun (drawState.outHeld) on its own root, re-parented there for the overlap
        Transform drawOutRoot;
        Rig drawOutRig;

        void DrawOutBuild()
        {
            drawOutRoot = new GameObject("Viewmodel (outgoing)").transform;
            drawOutRoot.SetParent(vmCam.transform, false);
        }

        /// <summary>The rig still flicking out (null: none), for the rigs' visibility this frame.</summary>
        string DrawOutGun(string held) => drawState.outHeld != null && drawState.outHeld != held ? drawState.outHeld : null;

        /// <summary>After the root's pose (x .. rollDeg, the ADS damp): the outgoing rig on its own root, the root's pose with
        /// the outgoing draw pose in place of the shown gun's.</summary>
        void DrawOutFrame(string held, bool show, double x, double y, double z, double pitchDeg, double yawDeg, double rollDeg, double damp)
        {
            string og = show ? DrawOutGun(held) : null;
            Rig orig = null;
            if (og != null) rigs.TryGetValue(og, out orig);
            if (orig != drawOutRig)
            {
                if (drawOutRig != null)
                {
                    drawOutRig.t.SetParent(root, false); // back under the shown root (hidden unless it is the gun shown)
                    if (drawOutRig.name != held && drawOutRig.t.gameObject.activeSelf) drawOutRig.t.gameObject.SetActive(false);
                }
                drawOutRig = orig;
                if (orig != null) orig.t.SetParent(drawOutRoot, false);
            }
            bool on = orig != null;
            if (drawOutRoot.gameObject.activeSelf != on) drawOutRoot.gameObject.SetActive(on);
            if (!on) return;
            if (!orig.t.gameObject.activeSelf) orig.t.gameObject.SetActive(true);
            Gun g = GunOf(og);
            orig.t.localPosition = g.kitPos;
            orig.t.localRotation = g.kitRot;
            orig.t.localScale = Vector3.one * g.kitScale;
            HandsDrawPose o = drawState.outPose;
            double ox = x + (o.x - drawPose.x) * damp, oy = y + (o.y - drawPose.y) * damp, oz = z + (o.z - drawPose.z) * damp;
            double op = pitchDeg + (o.pitchDeg - drawPose.pitchDeg) * damp, oyaw = yawDeg + (o.yawDeg - drawPose.yawDeg) * damp;
            double or = rollDeg + (o.rollDeg - drawPose.rollDeg) * damp;
            drawOutRoot.localPosition = new Vector3((float)ox, (float)oy, (float)-oz);
            drawOutRoot.localRotation = Quaternion.Euler((float)-op, (float)-oyaw, (float)or);
        }

        /// <summary>NIGHT17 round 1: the gun the view wants: the loadout's target slot (its slot when no swap runs).</summary>
        static string WantGun(Loadout lo) =>
            lo.owned.Count > 0 && lo.targetSlot >= 0 && lo.targetSlot < lo.owned.Count ? lo.owned[lo.targetSlot] : lo.Weapon;

        /// <summary>A swap in progress: the target differs from the slot (until the sim's gun change), or the lowered amount is up.</summary>
        static bool SwapActive(Loadout lo) => lo.targetSlot != lo.slot || lo.swapLoweredNow > 0 || lo.prevSwapLowered > 0;

        void DrawFrame(Loadout lo, string want, Gun wantGun, double animDt, double frameDt, double motion)
        {
            if (lo.swapMult != drawTimelineMult)
            {
                drawTimelineMult = lo.swapMult; // the QUICK perk changes the sim's timeline
                drawTimeline = HandsSwapTimeline.Of(drawSwapTime, lo.swapMult, drawLowerFraction, drawFireFraction, cfg.Dt);
            }
            double quick = drawQuick ? UnityTuning.HandsQuickDrawScale : 1;
            // NIGHT34 H (design.n34.bendLook): inside a bend the sim's swap is instant (builder B's StepSwap), so the hands do not play the 0.25 s
            // timeline: the new gun is shown at once (the state's no-swap path: its settle starts) and rises from Bend34HandsDip over Bend34HandsSlideS
            if (!bendHandsRead34)
            {
                bendHands34 = N34Fixes.ViewRowOn(cfg, "bendLook");
                bendHandsRead34 = true;
            }
            if (bendHands34 && game.sim != null && game.sim.Bend34Depth01 > 0)
            {
                if (drawState.held != null && want != drawState.held) bendSnapAge34 = 0;
                drawState.Frame(wantGun.draw, drawTimeline, want, 0, 0, game.Alpha, animDt, v.respawnRaiseTime, motion * quick, false);
            }
            else drawState.Frame(wantGun.draw, drawTimeline, want, lo.prevSwapLowered, lo.swapLoweredNow, game.Alpha, animDt, v.respawnRaiseTime, motion * quick, SwapActive(lo));
            string held = drawState.held;
            if (drawState.started && held != null)
            {
                // FEEL-DESIGN 2.4: a gun fired in the last 2 s draws briskly (from the next frame; the first is at the bottom)
                drawQuick = clock - GunOf(held).drawLastFired < UnityTuning.HandsQuickDrawWindow;
                if (held != "") game.events.Emit(FeelKeys.WeaponDraw, held); // port-only (payload: the weapon id; NIGHT17: as it shows)
            }
            drawPose = drawState.pose;
            if (bendSnapAge34 < UnityTuning.Bend34HandsSlideS)
            {
                // NIGHT34 H: the bend's snap: up from the dip over SlideS (frame time; REDUCED MOTION's `motion` scales it like every other draw move)
                drawPose.y -= UnityTuning.Bend34HandsDip * (1 - UiTween.OutCubic(bendSnapAge34 / UnityTuning.Bend34HandsSlideS)) * motion;
                bendSnapAge34 += frameDt;
                BendSnaps34 += bendSnapAge34 >= UnityTuning.Bend34HandsSlideS ? 1 : 0;
            }
            drawGlow = drawGlowGate.Frame(drawState.started, drawPose.glow, FlashIntensity(), game.flashGate, SurvivalGame.RealNow);
            DrawIdleFrame(drawState.spec ?? wantGun.draw, animDt, frameDt, motion);
            FeelFlourishFrame(lo, animDt, motion); // U3-30.10: the kill flourish (Viewmodel.Feel.cs)
        }

        // FEEL-DESIGN 5 #14 the idle flourish: the clock is the kiosk's (Kiosk.idle: any device input pokes it, in FrameKiosk
        // before this render), with the view's own same-frame resets on top (the look moving, the mover moving, a shot, a
        // swing, a punch, a swap: so scripted input and the AUTOPILOT count as input too)
        readonly HandsIdle drawIdle = new HandsIdle();
        double drawIdleOwn, drawIdleYaw = double.NaN, drawIdlePitch;

        /// <summary>The idle flourish this frame, folded into drawPose (its offsets added, its parts on the same channels).</summary>
        void DrawIdleFrame(HandsDrawSpec spec, double animDt, double frameDt, double motion)
        {
            double yaw = game.input.yaw, pitch = game.input.pitch;
            bool moved = yaw != drawIdleYaw || pitch != drawIdlePitch || game.mover.Speed > 0.01 || drawState.started || drawPose.lowered > 0;
            drawIdleYaw = yaw;
            drawIdlePitch = pitch;
            drawIdleOwn = moved ? 0 : drawIdleOwn + frameDt;
            double idleS = game.kiosk != null ? Math.Min(game.kiosk.idle, drawIdleOwn) : drawIdleOwn;
            SurvivalRun run = game.run;
            bool allowed = game.Active && run != null && run.state == SurvivalRun.State.Break && game.sim.flow == Flow.Playing;
            drawIdle.Frame(idleS, allowed, animDt);
            if (!(drawIdle.age < double.PositiveInfinity)) return;
            HandsDrawPose p = HandsIdle.Sample(spec, drawIdle.age, motion);
            drawPose.x += p.x;
            drawPose.y += p.y;
            drawPose.z += p.z;
            drawPose.pitchDeg += p.pitchDeg;
            drawPose.yawDeg += p.yawDeg;
            drawPose.rollDeg += p.rollDeg;
            drawPose.spinRad += p.spinRad;
            drawPose.pump = Math.Max(drawPose.pump, p.pump);
        }

        /// <summary>A shot, a swing, a punch: the draw's flourish and the idle stop on this frame.</summary>
        void DrawCancel()
        {
            drawState.Cancel();
            drawIdle.Cancel();
            feelFlourish.Cancel(); // U3-30.10: a shot or a swing stops the kill flourish on its frame
        }

        /// <summary>The signature's motion scale now: x effects intensity, x HandsReducedMotionScale with REDUCED MOTION.</summary>
        double DrawMotion()
        {
            MovementConfig mc = cfg.movement;
            return Clamp(mc.settings.effectsIntensity, 0, mc.feel.effectsIntensityMax) * (mc.settings.reducedMotion ? UnityTuning.HandsReducedMotionScale : 1);
        }

        /// <summary>A movement kick on the held gun's class spring (the recoil bank: pitch deg, back m, drop m, roll deg).</summary>
        void DrawKick(double pitchDeg, double dropY, double rollDeg)
        {
            double m = DrawMotion();
            if (m <= 0) return;
            peaks4[0] = pitchDeg * m;
            peaks4[1] = 0;
            peaks4[2] = dropY * m;
            peaks4[3] = rollDeg * m;
            SpringShape sp = GunOf(game.loadout.Weapon).spring;
            recoil.Add(sp, peaks4);
            recoilClock.Kick(sp.tPeak);
        }

        /// <summary>Movement on the hands (port-only): a lift on every take-off, a landing weighted by fall speed, the slam's
        /// impact. Called from OnMover (the sim's step; the kicks then ring on world time).</summary>
        void DrawOnMover(MoverEvent e)
        {
            switch (e.type)
            {
                case MoverEventType.Jump:
                case MoverEventType.DoubleJump:
                case MoverEventType.DashJump:
                case MoverEventType.BoostJump:
                case MoverEventType.WallJump:
                    DrawKick(UnityTuning.HandsJumpLiftDeg, -UnityTuning.HandsJumpLiftY, 0);
                    break;
                case MoverEventType.Land:
                {
                    if (FeelAB.On)
                    {
                        // NIGHT18 round 1 ("land: Same"): the gun takes the landing (nearly full weight at the reel's 13.5 m/s)
                        double w18 = HandsMove.LandWeightN18(e.fallSpeed);
                        if (w18 > 0) DrawKick(-UnityTuning.HandsLandPitchDegN18 * w18, UnityTuning.HandsLandDropYN18 * w18, UnityTuning.HandsLandRollDegN18 * w18);
                        break;
                    }
                    double w = HandsMove.LandWeight(e.fallSpeed);
                    if (w > 0) DrawKick(-UnityTuning.HandsLandPitchDeg * w, UnityTuning.HandsLandDropY * w, UnityTuning.HandsLandRollDeg * w);
                    break;
                }
                case MoverEventType.Slam:
                    DrawKick(-UnityTuning.HandsSlamImpactDeg, UnityTuning.HandsSlamImpactY, 0);
                    break;
            }
        }

        /// <summary>The slam's wind-up while the mover slams: the gun up and back (world time; x the motion scale).</summary>
        void DrawSlamWind(bool slamming, double animDt, ref double py, ref double pz, ref double rx)
        {
            drawSlamW += ((slamming ? 1 : 0) - drawSlamW) * EaseFactor(UnityTuning.HandsSlamWindRate, animDt);
            if (drawSlamW < 1e-4) return;
            double w = drawSlamW * DrawMotion();
            py += UnityTuning.HandsSlamY * w;
            pz += UnityTuning.HandsSlamBackZ * w;
            rx += UnityTuning.HandsSlamPitchDeg * Deg * w;
        }

        /// <summary>The LONGWAVE's sweep band: from the receiver to the muzzle while the draw's glow lives (rail in hand).</summary>
        void DrawBand(Rig rig, bool show)
        {
            if (drawBand == null) return;
            bool on = show && rig != null && rig.name == "rail" && drawGlow > 0;
            if (drawBand.gameObject.activeSelf != on) drawBand.gameObject.SetActive(on);
            if (!on) return;
            float z = Mathf.Lerp(UnityTuning.HandsLongwaveBandStartZ, UnityTuning.HandsLongwaveBandEndZ, (float)drawPose.sweep);
            drawBand.localPosition = new Vector3(0, UnityTuning.HandsLongwaveBandY, -z); // web rig space, z flipped
            drawBandMat.SetColor(BaseColor, rig.gemColor * (float)(UnityTuning.HandsLongwaveBandHdr * drawGlow));
        }

        /// <summary>viewmodel.js :845-852 the glint for the gun in hand; weaponReady on the bus when it starts.</summary>
        double DrawGlint(Rig rig, Loadout lo, double animDt)
        {
            if (rig.glint.Step(lo.CooldownFractionOf(rig.name), rig.glintFull, drawGlintMin, drawGlintTime, animDt))
            {
                game.events.Emit(FeelKeys.WeaponReady, rig.name); // the web's audio hook (docs/AUDIO-HOOKS.md)
                // the game's one gate (the reviewer's finding 7); FLASH INTENSITY 0 spends no slot; refused: the event, no flash
                if (!(FlashIntensity() > 0) || !game.flashGate.TryFlash(SurvivalGame.RealNow)) rig.glint.glint = 0;
            }
            return rig.glint.Value(drawGlintTime, FlashIntensity());
        }
    }

    /// <summary>
    /// viewmodel.js :942-947 muzzleWorld (polish #59): a point in the viewmodel camera's space lined up on screen in the world
    /// camera's: x / y x tan(worldFov / 2) / tan(vmFov / 2), the depth kept (both cameras share the eye).
    /// </summary>
    public static class HandsMuzzle
    {
        public static UnityEngine.Vector3 ToWorldCameraSpace(UnityEngine.Vector3 vmCameraSpace, double worldFovDeg, double vmFovDeg)
        {
            // const k = Math.tan((worldCamera.fov * DEG) / 2) / Math.tan((camera.fov * DEG) / 2); out.set(tmp.x * k, tmp.y * k, tmp.z)
            float k = (float)(Math.Tan(worldFovDeg * Math.PI / 360) / Math.Tan(vmFovDeg * Math.PI / 360));
            return new UnityEngine.Vector3(vmCameraSpace.x * k, vmCameraSpace.y * k, vmCameraSpace.z);
        }
    }

    public sealed partial class Viewmodel
    {
        /// <summary>
        /// viewmodel.js :942-947 muzzleWorld (polish #59, read-only): the held rig's muzzle in WORLD space, lined up on screen with
        /// the viewmodel's muzzle (the vm camera is the main camera's child at its eye). False with no rig in hand (the caller
        /// keeps its own point; the web falls back to the KESTREL's rig).
        /// </summary>
        public bool MuzzleWorld(out Vector3 at)
        {
            at = default;
            if (game.loadout == null || !rigs.TryGetValue(game.loadout.Weapon, out Rig rig) || !rig.t.gameObject.activeInHierarchy) return false;
            Vector3 local = vmCam.transform.InverseTransformPoint(rig.t.TransformPoint(rig.muzzle)); // vm camera space
            Camera main = game.cam;
            at = main.transform.TransformPoint(HandsMuzzle.ToWorldCameraSpace(local, main.fieldOfView, vmCam.fieldOfView));
            return true;
        }
    }

    /// <summary>The draw's phase (the trace's column).</summary>
    public enum HandsDrawPhase { Idle = 0, Holster = 1, Raise = 2, Settle = 3 }

    /// <summary>The draw signatures: five named guns, the rest by their weight class (Generic on the class spring).</summary>
    public enum HandsDrawKind { Generic, Kestrel, Thunderclap, Comet, Longwave, Gauntlet }

    /// <summary>The draw layer's offsets (web camera space: m, deg, added to the viewmodel's) and its parts' signals.</summary>
    public struct HandsDrawPose
    {
        public double x, y, z, pitchDeg, yawDeg, rollDeg;
        public double lowered; // the drawn lowered amount after the remap (0: up)
        public double spinRad; // the KESTREL's cylinder, on top of its chamber angle
        public double pump; // 0..1 the THUNDERCLAP's rack (the pump part)
        public double sweep; // 0..1 the LONGWAVE's band from the receiver to the muzzle
        public double glow; // 0..1 the draw's glow (the LONGWAVE's sweep, the GAUNTLET's gem)
        public double clench; // 0..1 the GAUNTLET's fist
        public HandsDrawPhase phase;
    }

    /// <summary>A gun's draw numbers, read from config once (HandsDraw.SpecFor).</summary>
    public sealed class HandsDrawSpec
    {
        public string weapon;
        public HandsDrawKind kind;
        public string cls; // the weight class (viewmodel.js classOf): light | medium | heavy
        public SpringShape spring; // the weight class's spring (viewmodel.recoilSprings.<class>)
        public double settleDeg, settleY; // the web's settle (recoilSprings.<class>.settleDeg / settleY)
        public double dropY, dropPitchDeg; // viewmodel.swapDropY / swapPitchDeg
        public double pumpTiltDeg; // viewmodel.pumpTiltDeg (the rack reuses it)
        public double chamberRad; // viewmodel.cylinderStepDeg (the KESTREL's spin ends on a chamber)
        public double life; // s after fire opens until every flourish channel is back at rest
        public double inToBack = UnityTuning.HandsInToBackMedium; // NIGHT17 U3-30.6: the in-to's ease-out-back (its class's overshoot)
    }

    /// <summary>The sim's swap timeline (Combat.StepSwap's arithmetic), for a swapMult.</summary>
    public struct HandsSwapTimeline
    {
        public int total, lower, fireStep; // steps: the whole swap, the bottom (the gun changes), the first step fire is open
        public double openU; // the raise's share done when fire opens: (fireStep - lower) / (total - lower)
        public double raiseSeconds; // (total - lower) x dt
        public double swapMult;

        public static HandsSwapTimeline Of(double swapTime, double swapMult, double lowerFraction, double fireFraction, double dt)
        {
            // Combat.StepSwap :899-900 and CombatMath.SwapCanFire, step by step (at most `total` steps; no allocation)
            int total = MovementMath.StepsFor(swapTime / swapMult, dt);
            int lower = Math.Min(total - 1, Math.Max(1, (int)JsMath.Round(total * lowerFraction)));
            int fire = lower;
            while (fire < total && !CombatMath.SwapCanFire(fire, lower, total, fireFraction)) fire++;
            return new HandsSwapTimeline
            {
                total = total, lower = lower, fireStep = fire, swapMult = swapMult,
                openU = total > lower ? (double)(fire - lower) / (total - lower) : 0,
                raiseSeconds = (total - lower) * dt,
            };
        }
    }

    public static class HandsDraw
    {
        const double Deg = Math.PI / 180;

        /// <summary>A gun's draw: its signature (five named guns) and its weight class's spring (viewmodel.js classOf).</summary>
        public static HandsDrawSpec SpecFor(GameConfig cfg, string weapon)
        {
            // viewmodel.js classOf: feel.punch.weapons[name].cls ?? viewmodel.meleeClass[name] ?? 'medium' (Viewmodel.GunOf)
            string cls = cfg.Has($"feel.punch.weapons.{weapon}.cls") ? cfg.Str($"feel.punch.weapons.{weapon}.cls")
                : cfg.Has($"viewmodel.meleeClass.{weapon}") ? cfg.Str($"viewmodel.meleeClass.{weapon}") : "medium";
            string sp = cfg.Has($"viewmodel.recoilSprings.{cls}") ? $"viewmodel.recoilSprings.{cls}" : "viewmodel.recoilSprings.medium";
            var s = new HandsDrawSpec
            {
                weapon = weapon,
                cls = cls,
                kind = weapon == "revolver" ? HandsDrawKind.Kestrel : weapon == "shotgun" ? HandsDrawKind.Thunderclap : weapon == "rocket" ? HandsDrawKind.Comet
                    : weapon == "rail" ? HandsDrawKind.Longwave : weapon == "gauntlet" ? HandsDrawKind.Gauntlet : HandsDrawKind.Generic,
                spring = SpringShape.Of(cfg.Num(sp + ".freqHz"), cfg.Num(sp + ".zeta")),
                settleDeg = cfg.Num(sp + ".settleDeg"),
                settleY = cfg.Num(sp + ".settleY"),
                dropY = cfg.Num("viewmodel.swapDropY"),
                dropPitchDeg = cfg.Num("viewmodel.swapPitchDeg"),
                pumpTiltDeg = cfg.Num("viewmodel.pumpTiltDeg"),
                chamberRad = cfg.Num("viewmodel.cylinderStepDeg") * Deg,
                inToBack = HandsEase.InToBack(cls),
            };
            // the springs' unit kicks are under 1 % of their peak by SettleTime(1, s, 0.01); the timed flourishes by their times
            double life = SpringShape.SettleTime(1, s.spring, 0.01);
            switch (s.kind)
            {
                case HandsDrawKind.Kestrel: life = Math.Max(life, UnityTuning.HandsKestrelSpinTime); break;
                case HandsDrawKind.Thunderclap: life = Math.Max(life, UnityTuning.HandsThunderclapRackTime); break;
                case HandsDrawKind.Longwave: life = Math.Max(life, UnityTuning.HandsLongwaveSweepTime); break;
                case HandsDrawKind.Gauntlet: life = Math.Max(life, UnityTuning.HandsGauntletClenchTotal); break;
            }
            s.life = life;
            return s;
        }

        /// <summary>
        /// The pure pose: (gun, lowered, raising, where fire opens, s since fire opened, cancelled, motion scale). `lowered`
        /// is the sim's (interpolated); `raising` its direction; `openU` the raise share where fire opens (1: the respawn
        /// raise, no gate); `settleAge` s since the gun reached fire-ready (negative before; +inf: none); `motion` the
        /// signature's scale (effects intensity x REDUCED MOTION's damping; the web's settle is not scaled).
        /// </summary>
        public static HandsDrawPose Sample(HandsDrawSpec s, double lowered, bool raising, double openU, double settleAge, bool cancelled, double motion)
        {
            if (lowered > 0 && !raising)
            {
                double h = HandsEase.OutSine(Math.Min(1, lowered)); // NIGHT17 U3-30.6: the yank (the web's was a smoothstep)
                return SampleW(s, HandsDrawPhase.Holster, h, h, 0, 0, settleAge, cancelled, motion);
            }
            if (lowered > 0)
            {
                double b = RaiseB(lowered, openU);
                // NIGHT17 U3-30.6: the in-to's ease-out-back (past hold, then exactly at hold where b reaches 0)
                return SampleW(s, HandsDrawPhase.Raise, 1 - HandsEase.OutBack(1 - b, s.inToBack), 0, b, 1, settleAge, cancelled, motion);
            }
            return SampleW(s, HandsDrawPhase.Idle, 0, 0, 0, 0, settleAge, cancelled, motion);
        }

        /// <summary>NIGHT17 round 1: the drawn drop where the swap's guns switch (just off-screen for this rig; the hip tip x
        /// `motion` lowers it: UnityTuning.HandsFlickDepth*).</summary>
        public static double FlickDepth(HandsDrawSpec s, double motion)
        {
            bool g = s.weapon == "gauntlet", sg = s.weapon == "shotgun";
            double still = g ? UnityTuning.HandsFlickDepthGauntletStill : sg ? UnityTuning.HandsFlickDepthThunderclapStill : UnityTuning.HandsFlickDepthStill;
            double tip = g ? UnityTuning.HandsFlickDepthGauntlet : sg ? UnityTuning.HandsFlickDepthThunderclap : UnityTuning.HandsFlickDepth;
            return still + (tip - still) * Clamp(motion, 0, 1);
        }

        /// <summary>Round 3 ("small, in the same corner"): a kit gun's presence scale on its rig (x viewmodel.kitOffsets.<w>.scale).</summary>
        public static double Presence(string weapon) => weapon == "rocket" ? UnityTuning.HandsPresenceScaleComet : UnityTuning.HandsPresenceScale;

        /// <summary>Round 6 (n17-r5e horde: the THUNDERCLAP's barrel lay across the cluster at rest): a gun's Unity-side turn of
        /// its kit about the rig's origin (deg, three's convention: + pitch muzzle up, + yaw muzzle left), added to
        /// viewmodel.kitOffsets.<w>; the THUNDERCLAP's barrel turned down and right out of the centre, its body where it was.</summary>
        public static void HoldTurn(string weapon, out double pitchDeg, out double yawDeg)
        {
            bool sg = weapon == "shotgun";
            pitchDeg = sg ? UnityTuning.HandsThunderclapTurnPitchDeg : 0;
            yawDeg = sg ? UnityTuning.HandsThunderclapTurnYawDeg : 0;
        }

        /// <summary>NIGHT17 U3-30.6 (D-69): a gun's Unity-side nudge of its hold pose (web camera space m, added to
        /// viewmodel.kitOffsets.<w>): the PULSE GAUNTLET sat under the weapon card; 0 for the rest.</summary>
        public static void HoldNudge(string weapon, out double dx, out double dy)
        {
            bool g = weapon == "gauntlet";
            dx = g ? UnityTuning.HandsGauntletHoldX : 0;
            dy = g ? UnityTuning.HandsGauntletHoldY : 0;
        }

        /// <summary>The KESTREL's spin for `turns` x motion, rounded to whole chambers (so where it stops looks like rest).</summary>
        public static double SpinRad(HandsDrawSpec s, double turns, double motion)
        {
            double full = 2 * Math.PI * turns * Clamp(motion, 0, 1);
            return s.chamberRad > 0 ? Math.Round(full / s.chamberRad) * s.chamberRad : full;
        }

        /// <summary>The raise still to go (1 at the swap's bottom, 0 where fire opens): 1 - clamp(u / openU), u = 1 - lowered.</summary>
        public static double RaiseB(double lowered, double openU) => openU > 0 ? 1 - Clamp((1 - Math.Min(1, lowered)) / openU, 0, 1) : 0;

        /// <summary>
        /// The pose from the drawn amounts (HandsDrawState keeps them continuous across a swap's turns): `w` the drop
        /// (x swapDropY / swapPitchDeg), `tip` the holster's tip toward the hip, `b` this raise's share still to go (1 at its
        /// start, 0 where fire opens), `sig` the raise signature's weight (1: a new gun from the bottom; 0: a swap called
        /// back mid-way, the same gun coming back up).
        /// </summary>
        public static HandsDrawPose SampleW(HandsDrawSpec s, HandsDrawPhase phase, double w, double tip, double b, double sig, double settleAge, bool cancelled, double motion)
        {
            var p = new HandsDrawPose();
            if (tip != 0)
            {
                // the holster's tip toward the right hip (the raise carries what is left of it up and out)
                p.x += UnityTuning.HandsHolsterX * tip * motion;
                p.rollDeg -= UnityTuning.HandsHolsterRollDeg * tip * motion;
                p.yawDeg -= UnityTuning.HandsHolsterYawDeg * tip * motion;
            }
            if (phase == HandsDrawPhase.Holster) p.phase = HandsDrawPhase.Holster;
            else if (phase == HandsDrawPhase.Raise)
            {
                // the raise, remapped to be up on the step fire opens
                switch (s.kind)
                {
                    case HandsDrawKind.Kestrel:
                        p.pitchDeg += UnityTuning.HandsKestrelRaisePitchDeg * Math.Sin(Math.PI * b) * sig * motion; // the muzzle leads
                        break;
                    case HandsDrawKind.Comet:
                    {
                        // up off the shoulder; eased in over the raise's first quarter so the turn at the bottom never pops
                        double ramp = Smooth(Clamp((1 - b) / 0.25, 0, 1));
                        p.x += UnityTuning.HandsCometRaiseX * w * sig * ramp * motion;
                        p.rollDeg += UnityTuning.HandsCometRaiseRollDeg * w * sig * ramp * motion;
                        break;
                    }
                }
                p.phase = b > 0 ? HandsDrawPhase.Raise : HandsDrawPhase.Settle;
            }
            p.lowered = w;
            p.y -= s.dropY * w;
            p.pitchDeg -= s.dropPitchDeg * w;
            if (cancelled || !(settleAge > 0) || settleAge >= s.life) return p;
            if (p.phase == HandsDrawPhase.Idle) p.phase = HandsDrawPhase.Settle;

            // the settle: the web's (NIGHT5 F2: the gun lands in the hands on its class spring), a unit kick peaking at 1
            double k = s.spring.Value(1 / s.spring.peakPerA, settleAge);
            p.pitchDeg -= s.settleDeg * k;
            p.y -= s.settleY * k;
            switch (s.kind)
            {
                case HandsDrawKind.Kestrel:
                {
                    // a flick up with a wrist twist on the class spring; the cylinder spins a whole turn (ease-out)
                    p.pitchDeg += UnityTuning.HandsKestrelFlickDeg * k * motion;
                    p.rollDeg += UnityTuning.HandsKestrelFlickRollDeg * k * motion;
                    double a = Clamp(settleAge / UnityTuning.HandsKestrelSpinTime, 0, 1);
                    p.spinRad = HandsDraw.SpinRad(s, UnityTuning.HandsKestrelSpinTurns, motion) * (1 - (1 - a) * (1 - a) * (1 - a)); // whole chambers x motion
                    if (a >= 1) p.spinRad = 0; // whole chambers: it looks the same as at rest
                    break;
                }
                case HandsDrawKind.Thunderclap:
                {
                    // the pump racked on arrival (the pump part out and home), the gun tilting and lifting with it
                    double a = settleAge / UnityTuning.HandsThunderclapRackTime;
                    double r = a < 1 ? Math.Sin(Math.PI * a) : 0;
                    p.pump = r;
                    p.rollDeg += s.pumpTiltDeg * r * motion; // the reviewer: damped like the other signature motions
                    p.pitchDeg += UnityTuning.HandsThunderclapRackLiftDeg * r * motion;
                    break;
                }
                case HandsDrawKind.Comet:
                    // seated on the shoulder: an extra drop and a roll back on the heavy spring
                    p.y -= UnityTuning.HandsCometShoulderY * k * motion;
                    p.rollDeg += UnityTuning.HandsCometShoulderRollDeg * k * motion;
                    break;
                case HandsDrawKind.Longwave:
                {
                    // the charge glow runs from the receiver to the muzzle, brightest mid-barrel
                    double a = settleAge / UnityTuning.HandsLongwaveSweepTime;
                    if (a < 1)
                    {
                        p.sweep = a;
                        p.glow = Math.Sin(Math.PI * a);
                    }
                    break;
                }
                case HandsDrawKind.Gauntlet:
                {
                    // the fist rises as it clenches; the gem flares with the clench
                    double c = Envelope(settleAge, UnityTuning.HandsGauntletClenchIn, UnityTuning.HandsGauntletClenchHold, UnityTuning.HandsGauntletClenchTotal);
                    p.clench = c;
                    p.glow = c;
                    p.y += UnityTuning.HandsGauntletRiseY * k * motion;
                    p.pitchDeg += UnityTuning.HandsGauntletRisePitchDeg * k * motion;
                    break;
                }
            }
            return p;
        }

        /// <summary>
        /// The viewmodel trace (the lead runs it in the Editor: HandsTests.WritesTheViewmodelTrace): for each kit gun, a swap
        /// into it from the previous one, the sim at 120 Hz under 144 fps frames with the loop's alpha, the frames traced
        /// through the holster, the raise and UnityTuning.HandsTraceAfterSeconds of settle; then each gun's respawn raise.
        /// Columns: frame, ms (since the swap press), weapon (in hand), lowered (the drawn amount), phase, the root's
        /// offsets px / py / pz (m) and rx / ry / rz (pitch / yaw / roll, deg), the parts, and fireOpen (the sim's gate).
        /// Returns the rows written.
        /// </summary>
        public static int WriteTrace(GameConfig cfg, string path)
        {
            string[] guns = { "revolver", "shotgun", "nailgun", "rocket", "rail", "gauntlet" };
            double dt = cfg.Dt, frameDt = 1 / UnityTuning.HandsTraceFps;
            double rr = cfg.Num("viewmodel.respawnRaiseTime");
            HandsSwapTimeline tl = HandsSwapTimeline.Of(cfg.Num("weapons.swapTime"), 1, cfg.Num("weapons.swapLowerFraction"), cfg.Num("weapons.swapFireFraction"), dt);
            var sb = new StringBuilder("frame,ms,weapon,lowered,phase,px,py,pz,rx,ry,rz,spin,pump,sweep,glow,clench,fireOpen\n");
            int rows = 0;
            CultureInfo inv = CultureInfo.InvariantCulture;
            for (int g = 0; g < guns.Length; g++)
            {
                string from = guns[(g + guns.Length - 1) % guns.Length], to = guns[g];
                HandsDrawSpec a = SpecFor(cfg, from), b = SpecFor(cfg, to);
                var st = new HandsDrawState();
                st.Frame(a, tl, from, 0, 0, 1, frameDt, rr, 1);
                double acc = 0, prev = 0, now = 0;
                int t = 0;
                int frames = (int)((tl.total * dt + UnityTuning.HandsTraceAfterSeconds) * UnityTuning.HandsTraceFps);
                for (int f = 0; f < frames; f++)
                {
                    acc += frameDt;
                    while (acc >= dt)
                    {
                        acc -= dt;
                        prev = now;
                        t++;
                        now = CombatMath.SwapLowered(t, tl.lower, tl.total);
                    }
                    bool changed = t >= 1; // NIGHT17 round 1: the view wants the target from the press
                    string want = changed ? to : from;
                    st.Frame(changed ? b : a, tl, want, prev, now, acc / dt, frameDt, rr, 1, t >= 1 && t < tl.total);
                    Row(sb, inv, f, f * frameDt * 1000, st.held, st.pose, t == 0 || t >= tl.fireStep);
                    rows++;
                }
                var rs = new HandsDrawState();
                rs.Respawn();
                int rframes = (int)((rr + UnityTuning.HandsTraceAfterSeconds) * UnityTuning.HandsTraceFps);
                for (int f = 0; f < rframes; f++)
                {
                    rs.Frame(b, tl, to, 0, 0, 1, f == 0 ? 0 : frameDt, rr, 1);
                    Row(sb, inv, f, f * frameDt * 1000, to + " (respawn)", rs.pose, true);
                    rows++;
                }
            }
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            return rows;
        }

        static void Row(StringBuilder sb, CultureInfo inv, int frame, double ms, string weapon, HandsDrawPose p, bool fireOpen)
        {
            sb.Append(frame).Append(',').Append(ms.ToString("0.00", inv)).Append(',').Append(weapon).Append(',')
                .Append(p.lowered.ToString("0.0000", inv)).Append(',').Append(p.phase).Append(',')
                .Append(p.x.ToString("0.00000", inv)).Append(',').Append(p.y.ToString("0.00000", inv)).Append(',').Append(p.z.ToString("0.00000", inv)).Append(',')
                .Append(p.pitchDeg.ToString("0.000", inv)).Append(',').Append(p.yawDeg.ToString("0.000", inv)).Append(',').Append(p.rollDeg.ToString("0.000", inv)).Append(',')
                .Append(p.spinRad.ToString("0.000", inv)).Append(',').Append(p.pump.ToString("0.000", inv)).Append(',').Append(p.sweep.ToString("0.000", inv)).Append(',')
                .Append(p.glow.ToString("0.000", inv)).Append(',').Append(p.clench.ToString("0.000", inv)).Append(',').Append(fireOpen ? 1 : 0).Append('\n');
        }

        /// <summary>viewmodel.js envelope (Viewmodel.Envelope): a sine rise, a hold, a smoothstep fall until `total`.</summary>
        internal static double Envelope(double t, double rise, double hold, double total)
        {
            if (t < 0 || t >= total) return 0;
            if (t < rise) return Math.Sin(Math.PI / 2 * (t / rise));
            if (t < rise + hold) return 1;
            return 1 - Smooth((t - rise - hold) / (total - rise - hold));
        }

        internal static double Smooth(double u) => u * u * (3 - 2 * u);

        internal static double Clamp(double x, double lo, double hi) => Math.Max(lo, Math.Min(hi, x));
    }

    /// <summary>
    /// The idle flourish (FEEL-DESIGN 5 #14, port-only): after UnityTuning.HandsIdleAfter s without input in a BREAK (the
    /// caller passes `allowed`: SurvivalRun.State.Break and Flow.Playing), the gun in hand fidgets, at most once per
    /// HandsIdleAfter s of idle. Any input (the idle clock dropping), the round starting (`allowed` false) or a shot
    /// (Cancel) stops it on the same frame.
    /// </summary>
    public sealed class HandsIdle
    {
        public double age = double.PositiveInfinity; // s since the flourish started (+inf: none)
        public bool started; // it started this frame

        double nextAt = UnityTuning.HandsIdleAfter, lastIdle;

        /// <summary>One frame: `idleSeconds` s without input (the caller's clock), `allowed` a BREAK while playing.</summary>
        public void Frame(double idleSeconds, bool allowed, double animDt)
        {
            started = false;
            if (idleSeconds < lastIdle)
            {
                // input: the idle clock went back; stop now, and the next flourish is a full HandsIdleAfter away
                age = double.PositiveInfinity;
                nextAt = UnityTuning.HandsIdleAfter;
            }
            lastIdle = idleSeconds;
            if (!allowed)
            {
                age = double.PositiveInfinity; // the round started (or a menu, DOWN): stop now
                return;
            }
            if (age < double.PositiveInfinity) age += animDt;
            if (idleSeconds >= nextAt)
            {
                age = 0;
                started = true;
                nextAt = (Math.Floor(idleSeconds / UnityTuning.HandsIdleAfter) + 1) * UnityTuning.HandsIdleAfter; // at most once per 8 s
            }
        }

        /// <summary>A shot, a swing, a swap: stop on this frame.</summary>
        public void Cancel() => age = double.PositiveInfinity;

        /// <summary>The idle flourish's pose at `age` (zero outside it): the KESTREL's spin and flick back, the THUNDERCLAP's
        /// half rack, the rest a weight-class shift.</summary>
        public static HandsDrawPose Sample(HandsDrawSpec s, double age, double motion)
        {
            var p = new HandsDrawPose();
            if (!(age >= 0) || age == double.PositiveInfinity) return p;
            switch (s.kind)
            {
                case HandsDrawKind.Kestrel:
                {
                    // rolled out to show the cylinder, one whole turn, then a flick back
                    double r = HandsDraw.Envelope(age, UnityTuning.HandsIdleKestrelRollIn, UnityTuning.HandsIdleKestrelRollHold, UnityTuning.HandsIdleKestrelRollTotal);
                    p.rollDeg += UnityTuning.HandsIdleKestrelRollDeg * r * motion;
                    double a = (age - UnityTuning.HandsIdleKestrelSpinFrom) / UnityTuning.HandsIdleKestrelSpinTime;
                    if (a > 0 && a < 1) p.spinRad = HandsDraw.SpinRad(s, 1, motion) * HandsDraw.Smooth(a); // whole chambers x motion: back on a chamber
                    double f = (age - UnityTuning.HandsIdleKestrelFlickFrom) / UnityTuning.HandsIdleKestrelFlickTime;
                    if (f > 0 && f < 1) p.pitchDeg += UnityTuning.HandsIdleKestrelFlickDeg * Math.Sin(Math.PI * f) * motion;
                    break;
                }
                case HandsDrawKind.Thunderclap:
                {
                    // the pump checked: half a rack, the gun tipped to look at it
                    p.pump = UnityTuning.HandsIdlePumpTravel * HandsDraw.Envelope(age, UnityTuning.HandsIdlePumpIn, UnityTuning.HandsIdlePumpHold, UnityTuning.HandsIdlePumpTotal);
                    double t = HandsDraw.Envelope(age, UnityTuning.HandsIdlePumpTipIn, UnityTuning.HandsIdlePumpTipHold, UnityTuning.HandsIdlePumpTipTotal);
                    p.rollDeg += UnityTuning.HandsIdlePumpRollDeg * t * motion;
                    p.pitchDeg += UnityTuning.HandsIdlePumpPitchDeg * t * motion;
                    break;
                }
                default:
                {
                    // a weight-class shift: a re-grip, longer and bigger for heavier guns
                    double total = s.cls == "heavy" ? UnityTuning.HandsIdleShiftHeavyTotal : s.cls == "light" ? UnityTuning.HandsIdleShiftLightTotal : UnityTuning.HandsIdleShiftMediumTotal;
                    double amp = s.cls == "heavy" ? UnityTuning.HandsIdleShiftHeavy : s.cls == "light" ? UnityTuning.HandsIdleShiftLight : UnityTuning.HandsIdleShiftMedium;
                    double e = HandsDraw.Envelope(age, UnityTuning.HandsIdleShiftIn, UnityTuning.HandsIdleShiftHold, total) * amp * motion;
                    p.x += UnityTuning.HandsIdleShiftX * e;
                    p.y += UnityTuning.HandsIdleShiftY * e;
                    p.rollDeg += UnityTuning.HandsIdleShiftRollDeg * e;
                    p.pitchDeg += UnityTuning.HandsIdleShiftPitchDeg * e;
                    break;
                }
            }
            return p;
        }
    }

    /// <summary>
    /// A draw's glow (the LONGWAVE's sweep, the GAUNTLET's gem flare) as a flash (the reviewer's finding 7): x
    /// flashIntensity, and its peak asks the game's one FlashGate once per draw (refused or intensity 0: no glow this draw,
    /// no slot spent at 0). The motion stays.
    /// </summary>
    public sealed class HandsGlowGate
    {
        /// <summary>One frame: `started` a draw started, `glow` the draw's glow (0..1); returns the glow to draw.</summary>
        bool asked, on;

        public double Frame(bool started, double glow, double flashIntensity, FlashGate gate, double now)
        {
            if (started)
            {
                asked = false;
                on = false;
            }
            if (!(glow > 0)) return 0;
            double fi = HandsDraw.Clamp(flashIntensity, 0, 1);
            if (!asked)
            {
                asked = true;
                on = fi > 0 && (gate == null || gate.TryFlash(now)); // its peak is a flash: one slot per draw, none at 0
            }
            return on ? glow * fi : 0;
        }
    }

    /// <summary>Movement on the hands (port-only): the landing's weight by fall speed.</summary>
    public static class HandsMove
    {
        /// <summary>0 under UnityTuning.HandsLandMinFall, 1 from HandsLandMaxFall, smooth between.</summary>
        public static double LandWeight(double fallSpeed) =>
            HandsDraw.Smooth(HandsDraw.Clamp((fallSpeed - UnityTuning.HandsLandMinFall) / (UnityTuning.HandsLandMaxFall - UnityTuning.HandsLandMinFall), 0, 1));

        /// <summary>NIGHT18 round 1 (builder M; "land: Same"): the landing's weight on the gun, linear from HandsLandMinFallN18
        /// to full at HandsLandFullFallN18 (a 3 m drop's 13.5 m/s is nearly full; NIGHT17's: 0.24 of it).</summary>
        public static double LandWeightN18(double fallSpeed) =>
            HandsDraw.Clamp((fallSpeed - UnityTuning.HandsLandMinFallN18) / (UnityTuning.HandsLandFullFallN18 - UnityTuning.HandsLandMinFallN18), 0, 1);
    }

    /// <summary>
    /// The draw's per-frame state (no Unity, no allocation). NIGHT17 round 1: the view feeds it the gun it WANTS (the
    /// loadout's target from the press) and whether a swap is active; it decides the gun SHOWN (`held`):
    /// - a new want while swapping: the shown gun flicks out toward the hip from wherever it is (HandsFlickTime of world
    ///   time, an ease-out sine to HandsFlickDepth, where every rig is off-screen), then the wanted gun is shown there and
    ///   rises on the sim's own step clock (the swap's step from its lowered amount and direction), an ease-out-back to
    ///   hold exactly on the fire step (HandsSwapTimeline.fireStep); with under HandsRiseMinSteps left, on world time;
    /// - want back to the gun being flicked: it rises again from where it is (no signature);
    /// - a new want with no swap (a pickup, a new run's loadout): shown at once, its settle starts;
    /// - the respawn / run-start raise: as before (viewmodel.respawnRaiseTime), a shot speeds it up.
    /// The settle (the web's class spring + each gun's signature) starts on the fire step; a shot cancels it.
    /// </summary>
    public sealed class HandsDrawState
    {
        public string held; // the gun shown
        public HandsDrawSpec spec; // its draw spec
        public bool raising, cancelled;
        public bool started; // a draw started this frame (the view emits weaponDraw): the shown gun changed
        public double settleAge = double.PositiveInfinity, respawnAge = double.PositiveInfinity;
        public HandsDrawPose pose;
        bool respawnPending, raiseYield;

        /// <summary>NIGHT17 U3-30.6: this draw came from a swap (not a respawn / run-start raise): QUICKDRAW's draw.</summary>
        public bool fromSwap;

        /// <summary>NIGHT17 U3-30.6: s since fire opened on this swap's draw (+inf: none, a respawn, or a shot since).</summary>
        public double QuickdrawAge => fromSwap && !cancelled && settleAge >= 0 && settleAge < double.PositiveInfinity ? settleAge : double.PositiveInfinity;

        /// <summary>Round 2: the outgoing gun still flicking out while the new one rises (null: none), and its pose (the view
        /// draws it on its own root).</summary>
        public string outHeld;
        public HandsDrawPose outPose;
        HandsDrawSpec outSpec;
        double outAge, outW0, outTip0, outDepth, outTime, outLastW, outLastTip;

        static double FlickTimeOf(in HandsSwapTimeline tl, double stepDt) =>
            Math.Min(UnityTuning.HandsFlickTime, UnityTuning.HandsFlickShare * tl.fireStep * stepDt);

        /// <summary>`gun` flicks out on its own clock from (w0, tip0), `age` s into a flick of `time` s, to its off-screen line.</summary>
        void StartOut(string gun, HandsDrawSpec gunSpec, double w0, double tip0, double motion, double time, double age)
        {
            outHeld = gun;
            outSpec = gunSpec;
            outAge = age;
            outW0 = w0;
            outTip0 = tip0;
            outDepth = Math.Max(HandsDraw.FlickDepth(gunSpec, motion), w0);
            outTime = time;
            outLastW = w0;
            outLastTip = tip0;
            outPose = HandsDraw.SampleW(gunSpec, HandsDrawPhase.Holster, w0, tip0, 0, 0, double.PositiveInfinity, true, motion);
        }

        // the turns: Holster = the flick out, Raise = the rise, Idle = at hold (the settle, the respawn raise)
        HandsDrawPhase segment;
        double lastW, lastTip;
        double flickAge, flickW0, flickTip0, flickDepth, flickTime;
        double riseT0, riseW0, riseTip0, riseSig, riseAge, lastT = double.NaN;
        bool riseSim;

        /// <summary>viewmodel.js respawn(): the gun rises in (a new run, a respawn); the draw starts on the next frame.</summary>
        public void Respawn()
        {
            respawnAge = 0;
            respawnPending = true;
            cancelled = false;
            raising = true;
            settleAge = double.NegativeInfinity;
            raiseYield = false;
            fromSwap = false;
            segment = HandsDrawPhase.Idle;
            lastW = lastTip = 0;
            outHeld = null;
        }

        /// <summary>A shot (the sim's `fire` / a melee swing): the flourish stops on this frame; the recoil takes over;
        /// QUICKDRAW's glint ends; a raise still running (the respawn's, or a world-time rise after fire opened) yields
        /// (its clock x HandsRaiseYieldSpeed).</summary>
        public void Cancel()
        {
            cancelled = true;
            raiseYield = true;
        }

        /// <summary>The same frame with the swap's activity read from the lowered amounts (a test's sim without slots).</summary>
        public void Frame(HandsDrawSpec spec, in HandsSwapTimeline tl, string want, double prevLowered, double nowLowered, double alpha, double animDt, double respawnRaiseTime, double motion)
            => Frame(spec, tl, want, prevLowered, nowLowered, alpha, animDt, respawnRaiseTime, motion, prevLowered > 0 || nowLowered > 0);

        /// <summary>
        /// One rendered frame: `want` the gun to end up in hand (the loadout's target slot) and `wantSpec` its spec;
        /// `prevLowered` / `nowLowered` the sim's last two steps (Loadout.prevSwapLowered / swapLoweredNow), `alpha` the
        /// loop's interpolation, `animDt` the world's frame time (0 in a freeze), `swapActive` a swap in progress (its
        /// target differs from the slot, or the lowered amount is above 0).
        /// </summary>
        public void Frame(HandsDrawSpec wantSpec, in HandsSwapTimeline tl, string want, double prevLowered, double nowLowered, double alpha, double animDt, double respawnRaiseTime, double motion, bool swapActive)
        {
            started = false;
            bool swapping = swapActive || prevLowered > 0 || nowLowered > 0;
            if (nowLowered > prevLowered) raising = false;
            else if (nowLowered < prevLowered) raising = true;
            double l = prevLowered + (nowLowered - prevLowered) * alpha;
            // the sim's swap step (continuous): t = l x lower going down, total - l x (total - lower) coming up; 0 on the press
            double t = !swapping ? double.NaN : nowLowered <= 0 && prevLowered <= 0 ? 0 : raising ? tl.total - l * (tl.total - tl.lower) : l * tl.lower;
            double stepDt = tl.total > tl.lower ? tl.raiseSeconds / (tl.total - tl.lower) : 0;
            // the sim's mirror (a swap called back: Combat's SwapMirror) jumps the step at the same lowered amount, and on that
            // step the direction reads wrong; a jump re-plans a running rise from the drawn pose (below)
            bool jumped = !double.IsNaN(t) && !double.IsNaN(lastT) && Math.Abs(t - lastT) > UnityTuning.HandsSimJumpSteps;
            lastT = t;

            // round 2: the outgoing gun goes on flicking out on its own clock while the new one rises (the overlap)
            if (outHeld != null)
            {
                outAge += animDt;
                double ou = outTime > 0 ? HandsDraw.Clamp(outAge / outTime, 0, 1) : 1;
                if (ou >= 1 || outHeld == held) outHeld = null;
                else
                {
                    outLastW = outW0 + (outDepth - outW0) * ou;
                    outLastTip = outTip0 + (1 - outTip0) * ou;
                    outPose = HandsDraw.SampleW(outSpec, HandsDrawPhase.Holster, outLastW, outLastTip, 0, 0, double.PositiveInfinity, true, motion);
                }
            }

            if (held == null || respawnPending)
            {
                // the first frame, or a respawn: the wanted gun at once (the respawn raise brings it up)
                outHeld = null;
                held = want;
                spec = wantSpec;
                cancelled = false;
                segment = HandsDrawPhase.Idle;
                lastW = lastTip = 0;
                if (respawnPending)
                {
                    started = true;
                    respawnPending = false;
                    fromSwap = false;
                }
            }
            else if (!ReferenceEquals(want, held) && want != held)
            {
                if (!swapping)
                {
                    Show(want, wantSpec, false); // arrived without a swap: at once, its settle starts
                    outHeld = null;
                    segment = HandsDrawPhase.Idle;
                    settleAge = 0;
                    lastW = lastTip = 0;
                }
                else if (outHeld != null && want == outHeld)
                {
                    // called back to the gun still on its way out: it rises back from where it is, the shown one flicks out
                    string g = held;
                    HandsDrawSpec gs = spec;
                    double gw = lastW, gtip = lastTip;
                    Show(outHeld, outSpec, true);
                    StartRise(t, tl, outLastW, outLastTip, 0);
                    StartOut(g, gs, gw, gtip, motion, FlickTimeOf(tl, stepDt), 0);
                }
                else if (segment != HandsDrawPhase.Holster)
                {
                    // flick the shown gun out from wherever it is (a flourish under way decays on through it)
                    segment = HandsDrawPhase.Holster;
                    flickAge = 0;
                    flickW0 = lastW;
                    flickTip0 = lastTip;
                    flickDepth = Math.Max(HandsDraw.FlickDepth(spec, motion), lastW);
                    flickTime = FlickTimeOf(tl, stepDt);
                }
            }
            else if (segment == HandsDrawPhase.Holster) StartRise(t, tl, lastW, lastTip, 0); // called back: up again from here

            if (respawnAge < double.PositiveInfinity)
                respawnAge += animDt * (raiseYield && respawnAge < respawnRaiseTime ? UnityTuning.HandsRaiseYieldSpeed : 1);

            if (segment == HandsDrawPhase.Holster)
            {
                flickAge += animDt;
                // linear: the gun leaves on the first frame at full speed and doesn't linger past its off-screen line
                double u = flickTime > 0 ? HandsDraw.Clamp(flickAge / flickTime, 0, 1) : 1, e = u;
                double w = flickW0 + (flickDepth - flickW0) * e, tip = flickTip0 + (1 - flickTip0) * e;
                settleAge = settleAge > 0 && settleAge < double.PositiveInfinity ? settleAge + animDt : double.PositiveInfinity;
                pose = HandsDraw.SampleW(spec, HandsDrawPhase.Holster, w, tip, 0, 0, settleAge, cancelled, motion);
                lastW = w;
                lastTip = tip;
                // round 2: the switch comes HandsSwapOverlap before the flick ends: the shown gun goes on flicking out as
                // `out` (still on screen), the wanted gun shows from its own off-screen line and rises on the sim's clock
                double switchU = flickTime > 0 ? Math.Max(UnityTuning.HandsSwapSwitchMinShare, 1 - UnityTuning.HandsSwapOverlap / flickTime) : 0;
                if (u < switchU) return;
                StartOut(held, spec, flickW0, flickTip0, motion, flickTime, flickAge);
                outDepth = flickDepth;
                outLastW = w;
                outLastTip = tip;
                outPose = pose;
                Show(want, wantSpec, true);
                StartRise(t, tl, HandsDraw.FlickDepth(wantSpec, motion), 1, 1);
            }

            if (segment == HandsDrawPhase.Raise)
            {
                if (jumped && riseSim) StartRise(t, tl, lastW, lastTip, riseSig);
                riseAge += animDt * (raiseYield ? UnityTuning.HandsRaiseYieldSpeed : 1);
                double u;
                if (riseSim) u = double.IsNaN(t) ? 1 : HandsDraw.Clamp((t - riseT0) / (tl.fireStep - riseT0), 0, 1);
                else u = HandsDraw.Clamp(riseAge / UnityTuning.HandsRiseMinTime, 0, 1);
                double e = 1 - HandsEase.OutBack(u, spec.inToBack);
                double w = riseW0 * e, tip = riseTip0 * e;
                settleAge = riseSim ? (double.IsNaN(t) ? 0 : (t - tl.fireStep) * stepDt) : riseAge - UnityTuning.HandsRiseMinTime;
                if (u >= 1)
                {
                    segment = HandsDrawPhase.Idle;
                    settleAge = Math.Max(0, settleAge);
                }
                pose = HandsDraw.SampleW(spec, HandsDrawPhase.Raise, w, tip, 1 - u, riseSig, settleAge, cancelled, motion);
                lastW = w;
                lastTip = tip;
                return;
            }

            lastTip = 0;
            double respL = respawnRaiseTime > 0 ? Math.Max(0, 1 - respawnAge / respawnRaiseTime) : 0;
            if (respawnAge <= respawnRaiseTime)
            {
                settleAge = respawnAge - respawnRaiseTime; // the respawn raise: no fire gate, up at its end
                pose = HandsDraw.Sample(spec, respL, true, 1, settleAge, cancelled, motion);
                lastW = pose.lowered;
                return;
            }
            if (settleAge < double.PositiveInfinity) settleAge = settleAge == double.NegativeInfinity ? 0 : settleAge + animDt;
            pose = HandsDraw.Sample(spec, 0, true, tl.openU, settleAge, cancelled, motion);
            lastW = 0;
            if (pose.phase == HandsDrawPhase.Settle && settleAge >= spec.life) pose.phase = HandsDrawPhase.Idle;
        }

        void Show(string gun, HandsDrawSpec gunSpec, bool swap)
        {
            held = gun;
            spec = gunSpec;
            started = true;
            cancelled = false;
            fromSwap = swap;
        }

        void StartRise(double t, in HandsSwapTimeline tl, double w0, double tip0, double sig)
        {
            segment = HandsDrawPhase.Raise;
            riseAge = 0;
            riseW0 = w0;
            riseTip0 = tip0;
            riseSig = sig;
            riseT0 = t;
            riseSim = !double.IsNaN(t) && tl.fireStep - t >= UnityTuning.HandsRiseMinSteps;
            raiseYield = false;
        }
    }

    /// <summary>viewmodel.js :845-855 the cooldown-ready glint (per rig): starts when a slow gun's cooldown runs out.</summary>
    public struct HandsGlint
    {
        public double glint, lastCooldown;

        /// <summary>One frame (world time); true when the glint starts (the view emits weaponReady).</summary>
        public bool Step(double cooldownFraction, double fullCooldown, double minCooldown, double glintTime, double dt)
        {
            // :848-852 if (rig.lastCooldown > 0 && cd <= 0 && fullCooldown(rig.name) >= st.glintMinCooldown) rig.glint = st.glintTime
            bool start = lastCooldown > 0 && cooldownFraction <= 0 && fullCooldown >= minCooldown;
            if (start) glint = glintTime;
            lastCooldown = cooldownFraction;
            glint = Math.Max(0, glint - dt); // :854
            return start;
        }

        /// <summary>:855 (rig.glint / st.glintTime) x clamp(settings.flashIntensity, 0, 1).</summary>
        public double Value(double glintTime, double flashIntensity) => glintTime > 0 ? glint / glintTime * HandsDraw.Clamp(flashIntensity, 0, 1) : 0;
    }

    /// <summary>At most UnityTuning.HandsFlashesPerSecond hand flashes in any second (WCAG 2.3.1; NIGHT16 rule 8).</summary>
    public sealed class HandsFlashGate
    {
        readonly double[] at = new double[UnityTuning.HandsFlashesPerSecond];
        int count, head;

        /// <summary>A flash at `now` (s, real time) is allowed when fewer than the cap were allowed in the second before it.</summary>
        public bool Allow(double now)
        {
            int live = 0;
            for (int i = 0; i < count; i++) if (now - at[i] < 1) live++;
            if (live >= at.Length) return false;
            at[head] = now;
            head = (head + 1) % at.Length;
            count = Math.Min(at.Length, count + 1);
            return true;
        }
    }
}
