// NIGHT16 U3-29.6 (builder V): the left arm, a PORT of viewmodel.js's `arm` (line for line where the web has it):
// - :157-161 the arm: a group in the viewmodel's camera space (rotation order YXZ), hidden at rest, its meshes from
//   buildParts(v.arm) (BlockyKit.Arm: config.viewmodel.arm's rows in the role colours vmBody / vmTrim / usable, :1011-1013);
// - :678 events.on('punch', () => punchAge = 0);
// - :925-928 punchAge += animDt; punchW = envelope(punchAge, v.punchOutTime, v.punchHoldTime, config.punch.cycleTime);
//   poseArm(punchW, sway.x + bobX, sway.y + bobY) (the quick punch's fist from the lower left, sharing the sway and bob);
// - :682-689 poseArm: rest + (punch - rest) x jab; visible while punchW > 0;
// - :961 respawn(): punchAge = Infinity.
// Port-only: the knuckles flash on a parry (the family cyan, `colors.usable`, a little toward white) and flash white on a
// PERFECT PARRY (the bus `parry` HitEvent's `perfect`), x flashIntensity, at most 3 hand flashes in any second
// (HandsFlashGate); a thin plate on the fist's front face (BlockyKit.ArmKnuckles), drawn only while it flashes.
// The arm runs on the world's time (frozen in a hitstop: a parry's freeze holds the fist out, its knuckles lit); its
// sway share follows the look on frame time (the Viewmodel's swayX / swayY), as the web's does.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The arm's pose (web camera space: m, deg) and whether it shows.</summary>
    public struct HandsArmPose
    {
        public double x, y, z, pitchDeg, yawDeg, rollDeg, w;
        public bool visible;
    }

    /// <summary>config.viewmodel.armRest / armPunch / punchOutTime / punchHoldTime and config.punch.cycleTime, read once.</summary>
    public sealed class HandsArmParams
    {
        public double restX, restY, restZ, restPitch, restYaw, restRoll;
        public double punchX, punchY, punchZ, punchPitch, punchYaw, punchRoll;
        public double outTime, holdTime, cycleTime;
        public BlockyKit.ArmPart[] parts; // config.viewmodel.arm
        public int segments; // config.viewmodel.cylinderSegments

        public static HandsArmParams From(GameConfig cfg)
        {
            const string r = "viewmodel.armRest.", p = "viewmodel.armPunch.";
            var a = new HandsArmParams
            {
                restX = cfg.Num(r + "x"), restY = cfg.Num(r + "y"), restZ = cfg.Num(r + "z"),
                restPitch = cfg.Num(r + "pitchDeg"), restYaw = cfg.Num(r + "yawDeg"), restRoll = cfg.Num(r + "rollDeg"),
                punchX = cfg.Num(p + "x"), punchY = cfg.Num(p + "y"), punchZ = cfg.Num(p + "z"),
                punchPitch = cfg.Num(p + "pitchDeg"), punchYaw = cfg.Num(p + "yawDeg"), punchRoll = cfg.Num(p + "rollDeg"),
                outTime = cfg.Num("viewmodel.punchOutTime"), holdTime = cfg.Num("viewmodel.punchHoldTime"), cycleTime = cfg.Num("punch.cycleTime"),
                segments = cfg.Int("viewmodel.cylinderSegments"),
            };
            var rows = cfg.Arr("viewmodel.arm");
            a.parts = new BlockyKit.ArmPart[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                var row = PortData.Arr(rows[i]); // [role, shape, sx, sy, sz, x, y, z]
                a.parts[i] = new BlockyKit.ArmPart
                {
                    role = (string)row[0], shape = (string)row[1],
                    sx = (float)PortData.Num(row[2]), sy = (float)PortData.Num(row[3]), sz = (float)PortData.Num(row[4]),
                    x = (float)PortData.Num(row[5]), y = (float)PortData.Num(row[6]), z = (float)PortData.Num(row[7]),
                };
            }
            return a;
        }
    }

    /// <summary>
    /// The knuckles' parry flash (port-only), pure: a parry arms it; it lights on the frame the fist is out and asks the
    /// game's one FlashGate then; a parry with no fist punching (a gauntlet parry) drops it without spending a slot.
    /// </summary>
    public sealed class HandsKnuckles
    {
        public bool pending, perfect;
        public double age = double.PositiveInfinity, scale;

        /// <summary>The bus `parry` (its `perfect`): armed for the fist's arrival; nothing at FLASH INTENSITY 0.</summary>
        public void Parry(bool perfect, double flashIntensity)
        {
            if (!(flashIntensity > 0)) return;
            pending = true;
            this.perfect = perfect;
            scale = Math.Min(1, flashIntensity);
            age = double.PositiveInfinity;
        }

        /// <summary>One frame, after the arm's clock: light when the fist is out (asking the gate), drop when no fist is
        /// punching; the flash decays on world time (held through a freeze).</summary>
        public void Frame(double punchAge, double outTime, double cycleTime, double animDt, FlashGate gate, double now)
        {
            if (pending)
            {
                if (!(punchAge < cycleTime)) pending = false; // no fist out there (a gauntlet parry): no flash, no slot
                else if (punchAge >= outTime)
                {
                    pending = false;
                    if (gate == null || gate.TryFlash(now)) age = 0;
                }
                return;
            }
            if (age < double.PositiveInfinity) age += animDt;
        }

        /// <summary>0..flashIntensity: the flash now (fading over `flashTime`).</summary>
        public double Value(double flashTime) => age < flashTime ? (1 - age / flashTime) * scale : 0;

        public void Clear()
        {
            pending = false;
            age = double.PositiveInfinity;
        }
    }

    /// <summary>
    /// The recoil's clock (the reviewer's finding: a shot's own freeze held the gun still, then it kicked): the newest kick's
    /// ATTACK (to its spring's first peak) runs on max(worldDt, frameDt), the hold and the return on world time. Returns
    /// the dt to advance the spring bank by; worldDt >= frameDt: worldDt exactly.
    /// </summary>
    public sealed class HandsRecoilClock
    {
        double age = double.PositiveInfinity, peak;

        /// <summary>A kick was added to the bank: its spring's first peak (SpringShape.tPeak).</summary>
        public void Kick(double tPeak)
        {
            age = 0;
            peak = tPeak;
        }

        public double Step(double worldDt, double frameDt)
        {
            if (!(age < peak)) return worldDt; // no kick rising: world time (the web's)
            double next = HandsArm.AdvanceOut(age, worldDt, frameDt, peak);
            double d = next - age;
            age = next;
            return d;
        }

        public void Clear() => age = double.PositiveInfinity;
    }

    /// <summary>
    /// viewmodel.js :654-656, :665-670, :740, :875-878, :956: NIGHT5 F4's momentum hit: a momentum melee hit drives the gun
    /// in and holds (viewmodel.impactHold on its own spring bank), on frame time so it plays through the hitstop.
    /// </summary>
    public sealed class HandsImpactHold
    {
        // NIGHT5 F4: the DISCUS lands back in the hand (an overshoot on its spring); a momentum melee hit drives in and holds.
        readonly SpringBank impact; // const impact = createSpringBank(4, v.recoilSnap);
        readonly SpringShape impactShape; // const impactShape = springShape(v.impactHold.freqHz, v.impactHold.zeta);
        readonly double[] peaks; // v.impactHold.peaks
        readonly double momentumHitstopAt; // config.feel.momentumHitstopAt
        int lastImpactShot = -1; // let lastImpactShot = -1;

        public HandsImpactHold(GameConfig cfg)
        {
            impact = new SpringBank(4, cfg.Nums("viewmodel.recoilSnap"));
            impactShape = SpringShape.Of(cfg.Num("viewmodel.impactHold.freqHz"), cfg.Num("viewmodel.impactHold.zeta"));
            peaks = cfg.Nums("viewmodel.impactHold.peaks");
            momentumHitstopAt = cfg.Num("feel.momentumHitstopAt");
        }

        /// <summary>:666-670 events.on('hit', mine(...)): true when the drive was added.</summary>
        public bool OnHit(HitEvent e)
        {
            if (e.owner != "player") return false; // mine
            if (!e.melee || e.shot == lastImpactShot || !(e.momentum > momentumHitstopAt)) return false;
            lastImpactShot = e.shot;
            impact.Add(impactShape, peaks);
            return true;
        }

        /// <summary>:740 impact.advance(frameDt); // NIGHT5 F4: frame time, so the momentum hit's drive plays through the hitstop</summary>
        public void Advance(double frameDt) => impact.Update(frameDt);

        /// <summary>impact.active.</summary>
        public bool Active => impact.Active > 0;

        /// <summary>impact.value(c): 0 pitch deg, 1 back m, 2 drop m, 3 roll deg.</summary>
        public double Value(int channel) => impact.Sample(channel);

        /// <summary>:956 impact.clear() (respawn).</summary>
        public void Clear() => impact.Clear();
    }

    /// <summary>
    /// viewmodel.js :658-663 (NIGHT6 B1: the gun comes out of the forge with a flourish: a kick on its spring, a gem flash).
    /// The web keeps a recoil bank per rig; the port draws one bank, the gun in hand's, so a forge of another gun shows
    /// nothing now (the web's kick on a hidden rig is spent before it is drawn). The gem flash asks the game's one FlashGate.
    /// </summary>
    public sealed class HandsForgeFlourish
    {
        readonly double[] flourish; // config.forge.flourish (pitch deg, back m, drop m, roll deg)

        public HandsForgeFlourish(GameConfig cfg) => flourish = cfg.Nums("forge.flourish");

        /// <summary>The `forge` event: true (the kick added to `recoil` on `spring`) when it is mine and the gun in hand.</summary>
        public bool OnForge(ForgeEvent e, string held, SpringBank recoil, SpringShape spring, HandsRecoilClock clock)
        {
            if (e.owner != "player") return false; // mine
            if (e.weapon != held) return false; // const rig = rigs[e.weapon]; (the port draws the gun in hand's bank only)
            recoil.Add(spring, flourish); // rig.recoil.add(springFor(e.weapon), config.forge.flourish);
            clock?.Kick(spring.tPeak); // its attack lands even in a freeze (the reviewer's recoil clock)
            return true;
        }

        /// <summary>The flourish's gem flash: x flashIntensity (0: none, no slot), one slot of the game's gate.</summary>
        public static bool GemFlash(double flashIntensity, FlashGate gate, double now) =>
            flashIntensity > 0 && (gate == null || gate.TryFlash(now));
    }

    /// <summary>
    /// STEADY (POWER P6, player.js :124 / :160-161, viewmodel.js :151 / :571-574): recoilMult x the gun's kick peaks and the
    /// camera punch's. perks.fx.recoil (PerkMath.Effects: perks.steady.recoil with STEADY owned, else 1).
    /// </summary>
    public static class HandsSteady
    {
        public static double Mult(PerkEffects fx) => fx != null ? fx.recoil : 1;
    }

    public static class HandsArm
    {
        /// <summary>The slide brace's weight the view draws: off (round 5: the bracing forearm filled the left quarter of the
        /// screen; UnityTuning.HandsBraceOn), else the slide pose's weight.</summary>
        public static double BraceWeight(double slideW) => UnityTuning.HandsBraceOn ? Math.Max(0, slideW) : 0;

        /// <summary>viewmodel.js :925-928 + poseArm :682-689: the punch envelope and the arm's pose (sway + bob added).</summary>
        public static HandsArmPose Pose(HandsArmParams a, double punchAge, double px, double py)
        {
            double w = HandsDraw.Envelope(punchAge, a.outTime, a.holdTime, a.cycleTime);
            // const mix = (key) => a[key] + (b[key] - a[key]) * punchW;
            return new HandsArmPose
            {
                w = w,
                visible = w > 0, // arm.visible = punchW > 0
                x = a.restX + (a.punchX - a.restX) * w + px,
                y = a.restY + (a.punchY - a.restY) * w + py,
                z = a.restZ + (a.punchZ - a.restZ) * w,
                pitchDeg = a.restPitch + (a.punchPitch - a.restPitch) * w,
                yawDeg = a.restYaw + (a.punchYaw - a.restYaw) * w,
                rollDeg = a.restRoll + (a.punchRoll - a.restRoll) * w,
            };
        }

        /// <summary>
        /// Port-only (NIGHT16 build 1's strips): a punch's clock. The OUT travel (age 0 .. outTime) advances on
        /// max(worldDt, frameDt), so in a freeze that starts on the punch's own step the fist still reaches full extension
        /// and holds there; the hold and the return stay on world time. worldDt >= frameDt: world time exactly (the web's).
        /// </summary>
        public static double AdvanceOut(double age, double worldDt, double frameDt, double outTime)
        {
            if (worldDt >= frameDt || !(age < outTime)) return age + worldDt; // the web's clock (punchAge += animDt)
            double fast = frameDt; // the world slowed or stopped: the out travel at frame speed
            if (age + fast <= outTime) return age + fast;
            return outTime + worldDt * (1 - (outTime - age) / fast); // out, then the rest of the frame on world time
        }

        /// <summary>
        /// Port-only: the same, with the free hand on a wall (wallW 0..1: rest eases to the wall pose; the punch still wins
        /// over it) and the wall-jump's push-off (pushAge s since the push; +inf: none). wallW 0 and no push is the web's.
        /// </summary>
        public static HandsArmPose Pose(HandsArmParams a, double punchAge, double px, double py, double wallW, double pushAge)
        {
            HandsArmPose p = Pose(a, punchAge, px, py);
            if (!(wallW > 0) && !(pushAge < UnityTuning.HandsWallPushTotal)) return p; // the web's arm
            // the base (armRest) eases to the wall pose; the punch blends over it by its envelope (as poseArm over rest)
            double w = p.w, k = HandsDraw.Clamp(wallW, 0, 1), push = HandsDraw.Envelope(pushAge, UnityTuning.HandsWallPushOut, UnityTuning.HandsWallPushHold, UnityTuning.HandsWallPushTotal);
            double bx = a.restX + (UnityTuning.HandsWallHandX + UnityTuning.HandsWallPushX * push - a.restX) * k;
            double by = a.restY + (UnityTuning.HandsWallHandY - a.restY) * k;
            double bz = a.restZ + (UnityTuning.HandsWallHandZ - a.restZ) * k;
            double bp = a.restPitch + (UnityTuning.HandsWallHandPitchDeg - a.restPitch) * k;
            double byaw = a.restYaw + (UnityTuning.HandsWallHandYawDeg - a.restYaw) * k;
            double br = a.restRoll + (UnityTuning.HandsWallHandRollDeg - a.restRoll) * k;
            p.x = bx + (a.punchX - bx) * w + px;
            p.y = by + (a.punchY - by) * w + py;
            p.z = bz + (a.punchZ - bz) * w;
            p.pitchDeg = bp + (a.punchPitch - bp) * w;
            p.yawDeg = byaw + (a.punchYaw - byaw) * w;
            p.rollDeg = br + (a.punchRoll - br) * w;
            p.visible = w > 0 || k > 0.001;
            return p;
        }
    }

    public sealed partial class Viewmodel
    {
        HandsArmParams armP;
        Transform armRoot, armKnuckles;
        Material armKnuckleMat;
        Color armUsable;
        double armPunchAge = double.PositiveInfinity; // viewmodel.js :548 let punchAge = Infinity
        double armBobX, armBobY; // this frame's bob (Render's step 2), the arm's share of it
        readonly HandsKnuckles armKnuckle = new HandsKnuckles(); // the parry flash (asks the game's one FlashGate)
        double armWallW; // 0..1 the free hand on a wall on the left (port-only)
        double armWallSide; // this frame's wall side (Render's: the wall normal . the view's right; > 0: the wall on the left)
        double armPushAge = double.PositiveInfinity; // s since a wall-jump's push-off

        /// <summary>:157-161: the arm group, hidden, its meshes from buildParts(v.arm); the knuckle plate (port-only).</summary>
        void ArmBuild()
        {
            armP = HandsArmParams.From(cfg);
            armUsable = Look.Hex((uint)cfg.Num("colors.usable"));
            var parts = new MeshKit();
            // NIGHT17 round 1: the port's fist (bigger than the forearm, knuckle ridges, finger folds), from the web's rows
            Color sleeve = Color.Lerp(Look.Hex((uint)cfg.Num("ui.theme.panel")), BlockyKit.Dark, UnityTuning.HandsSleeveInkMix); // round 5: the sleeve in the hands' ink (not the floor's brown), a cyan edge (ArmFist)
            BlockyKit.ArmFist(parts, armP.parts, sleeve, Look.Hex((uint)cfg.Num("colors.vmTrim")), armUsable, armP.segments);
            armRoot = new GameObject("Left arm").transform;
            armRoot.SetParent(vmCam.transform, false);
            SetLayer(MeshKit.Object("arm", armRoot, parts.Build("left arm"), kit, shadows: false));
            var knuckles = new MeshKit();
            BlockyKit.ArmKnuckles(knuckles, BlockyKit.FistOf(armP.parts), Color.white);
            armKnuckleMat = new Material(game.mats.neon);
            GameObject k = MeshKit.Object("knuckles", armRoot, knuckles.Build("left arm knuckles"), armKnuckleMat, shadows: false);
            SetLayer(k);
            k.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            armKnuckles = k.transform;
            k.SetActive(false);
            armRoot.gameObject.SetActive(false); // arm.visible = false
            game.events.On<MeleeEvent>("punch", ArmOnPunch); // :678
            game.events.On<HitEvent>("parry", ArmOnParry);
            armImpact = new HandsImpactHold(cfg);
            game.events.On<HitEvent>("hit", e => armImpact.OnHit(e)); // :666-670 (the payload read inside the handler)
            armForge = new HandsForgeFlourish(cfg);
            game.events.On<ForgeEvent>("forge", ArmOnForge); // :658-663
        }

        HandsImpactHold armImpact; // NIGHT16 U3-29.8: the momentum hit's hold spring (frame time)
        HandsForgeFlourish armForge; // NIGHT16 U3-29.8: the forge flourish

        /// <summary>:658-663 NIGHT6 B1: the gun comes out of the forge with a flourish (a kick on its spring, a gem flash).</summary>
        void ArmOnForge(ForgeEvent e)
        {
            string held = game.loadout.Weapon;
            if (!armForge.OnForge(e, held, recoil, GunOf(held).spring, recoilClock)) return;
            if (!rigs.TryGetValue(e.weapon, out Rig rig)) return;
            // rig.flash = v.gemFlashTime; (a flash of the night's: x flashIntensity, the game's one gate)
            double fi = FlashIntensity();
            if (!HandsForgeFlourish.GemFlash(fi, game.flashGate, SurvivalGame.RealNow)) return;
            rig.flash = v.gemFlashTime;
            rig.flashScale = fi;
        }

        /// <summary>:678 events.on('punch', () => punchAge = 0); the idle flourish stops on the same frame (FEEL-DESIGN 5 #14).</summary>
        void ArmOnPunch(MeleeEvent e)
        {
            // NIGHT17 round 1: a punch that connects is on its impact frame now (the sim's hit step is the kill's and the
            // freeze's first frame); a whiff winds up and strikes (HandsPunch.StartAge)
            armPunchAge = HandsPunch.StartAge(armP, e.hits > 0);
            drawIdle.Cancel();
            FeelOnPunch(e); // NIGHT17 U3-30.6: a hit rebounds off the target (HandsPunch); the gun takes the body's torque
        }

        /// <summary>Port-only: a parry lights the knuckles (cyan; white on a PERFECT PARRY), x flashIntensity, capped.</summary>
        void ArmOnParry(HitEvent e)
        {
            if (e.owner != "player") return;
            // armed: lit when the fist arrives (ArmRender), asking game.flashGate then; a gauntlet parry (no fist out) drops it
            // without spending a slot (the reviewer's finding 7)
            armKnuckle.Parry(e.perfect, FlashIntensity());
        }

        double FlashIntensity()
        {
            GameSettings s = game.settings;
            return s != null ? Clamp(s.View.flashIntensity, 0, 1) : 1;
        }

        /// <summary>:925-928, after the gun's pose: the punch envelope and poseArm (sharing the sway and the bob).</summary>
        void ArmRender(double animDt, double frameDt, bool show)
        {
            // NIGHT16 build 1: :926 punchAge += animDt, except the OUT travel, which runs on frame time while the world is
            // frozen or slowed (a punch kill / parry freezes on the punch's own step: the fist must be out in the freeze)
            armPunchAge = HandsArm.AdvanceOut(armPunchAge, animDt, frameDt, armP.outTime);
            // the knuckles light on the frame the fist arrives (then decay on world time: lit through the freeze)
            armKnuckle.Frame(armPunchAge, armP.outTime, armP.cycleTime, animDt, game.flashGate, SurvivalGame.RealNow);
            armPushAge += animDt;
            // port-only: the free hand rests on a wall-ride's wall when it is on the left (Titanfall); pushes off on a wall-jump
            bool onWall = game.mover.state == MoveState.WallRide && armWallSide > UnityTuning.HandsWallHandMinSide;
            armWallW += ((onWall ? 1 : 0) - armWallW) * EaseFactor(UnityTuning.HandsWallHandRate, animDt);
            if (armWallW < 1e-3 && !onWall) armWallW = 0;
            // NIGHT17 U3-30.6: the punch's wind-up, accelerating strike, impact frame past the target, snap and rebound
            // (HandsPunch; the web's sine envelope stays as HandsArm.Pose, the reference)
            // round 1: the free hand braces toward the floor while sliding (the slide pose's weight)
            // NIGHT19 U3-33.8: tonight's look (FeelAB on) jabs straight (HandsJab); FeelAB off keeps NIGHT18's hook
            HandsArmPose a = HandsPunch.Pose(armP, armPunchAge, feelPunchHit, swayX + armBobX, swayY + armBobY, armWallW, armPushAge, HandsArm.BraceWeight(feelMove.slideW), FeelAB.On); // round 5: the brace is off
            Fist36GuardArm(ref a); // NIGHT36 F fist36: the parry36 guard on the punch's fist
            bool visible = show && a.visible;
            if (armRoot.gameObject.activeSelf != visible) armRoot.gameObject.SetActive(visible);
            if (!visible) return;
            // the viewmodel camera's space with the z flip (as the root): yaw and pitch negate, roll stays
            armRoot.localPosition = new Vector3((float)a.x, (float)a.y, (float)-a.z);
            armRoot.localRotation = Quaternion.Euler((float)-a.pitchDeg, (float)-a.yawDeg, (float)a.rollDeg);
            double f = armKnuckle.Value(UnityTuning.HandsKnuckleFlashTime); // x flashIntensity already
            bool lit = f > 0;
            if (armKnuckles.gameObject.activeSelf != lit) armKnuckles.gameObject.SetActive(lit);
            if (lit)
            {
                Color c = Color.Lerp(armUsable, Color.white, armKnuckle.perfect ? 1 : (float)UnityTuning.HandsKnuckleParryWhite);
                armKnuckleMat.SetColor(BaseColor, c * (float)(UnityTuning.HandsKnuckleHdr * f));
            }
        }

        /// <summary>:961 respawn(): punchAge = Infinity (and the knuckles dark).</summary>
        void ArmRespawn()
        {
            armPunchAge = double.PositiveInfinity;
            armKnuckle.Clear();
            armPushAge = double.PositiveInfinity;
            armWallW = 0;
        }
    }
}
