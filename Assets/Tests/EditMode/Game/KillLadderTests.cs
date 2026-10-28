// NIGHT18 U3-31.3 (builder X): the kill at every range, its pure rules against NIGHT18.md's metrics (FeelAB on; the off
// side is NIGHT17's, kept by named numbers and the design.impact.n17 table):
// - M4, the freeze ladder: ImpactRules built from design.impact (on) or design.impact.n17 (off) and run through the loop's
//   own pacing (SurvivalGame.Update: 60 fps frames, the 120 Hz accumulator, a freeze cutting the frame's steps), counted as
//   the reel's telemetry counts HELD steps (FeelProbe: heldSteps = the frame's whole steps minus the steps run, on a frozen
//   frame or the frame a freeze cut). hit 0-2, a body kill 6-8, a headshot kill 10-13, a punch kill 11-14, a 5-kill blast
//   >= 18 and at most 160 ms. The kill may land on either step of its frame: both phases are checked.
// - M3, the chunks thrown: the throw's axis (along the shot, across the screen, up) carries the cloud's middle at least 1.5
//   body heights across the screen 250 ms after its release, at 6, 12 and 20 m (NIGHT17's along-the-shot throw: well under).
// - M2, the burst's screen floor: at 20 m its visible part is at least 8 % of the screen's height; at a punch's reach it
//   stays under 40 % (too much is a failure too).
// - The burst's frames (2 full, 2 fading; NIGHT17: 1 full, 1 at half) and the horde's scale.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class KillLadderTests
    {
        static GameConfig cfg;
        const double FrameDt = 1.0 / 60; // the reel's capture rate (UnityTuning.ReelFps)
        const double Eps = 1e-9;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Root(), "config.json"));

        /// <summary>The PortExport folder: beside Assets in the Editor; the working folder's under a plain .NET runner.</summary>
        static string Root()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (Exception) { return Path.GetFullPath("PortExport"); }
        }

        // --- M4: the freeze ladder ------------------------------------------------------------------------------------------

        static HitEvent Hit(string weapon, bool head = false, bool killed = false, bool melee = false, int shot = 1, double damage = 30) =>
            new HitEvent { owner = "player", weapon = weapon, kind = "husk", headshot = head, killed = killed, melee = melee, shot = shot, damage = damage, distance = 12 };

        /// <summary>The bus's order for one moment: hit, then kill (and the sim's own kill hitstop) for a killing hit.</summary>
        static void Land(ImpactRules r, HitEvent h)
        {
            r.OnHit(h);
            if (!h.killed) return;
            r.OnKill(new KillEvent { owner = "player", weapon = h.weapon, kind = "husk", headshot = h.headshot, melee = h.melee });
            r.OnSimHitstop("kill");
        }

        /// <summary>
        /// SurvivalGame.Update's pacing at 60 fps for 3 s of run-in, then the moment on the `phase`-th step (0 or 1) of a
        /// frame: the HELD steps from that frame to the first frame not held after it (FeelProbe's count, M4's sum).
        /// </summary>
        static int HeldSteps(ImpactRules r, int phase, params HitEvent[] moments)
        {
            const double step = 1.0 / 120;
            double acc = 0;
            int runIn = 180, frame = 0, held = 0;
            bool landed = false;
            for (; frame < runIn + 120; frame++)
            {
                r.ClearPending(); // ImpactFrames.BeginFrame: a moment outside a step earns nothing
                double share = r.BeginFrame(FrameDt, false);
                bool heldFrame = share <= 0, cut = false;
                int run = 0;
                if (share > 0)
                {
                    acc += FrameDt * share;
                    while (acc >= step)
                    {
                        if (frame == runIn && run == phase && !landed)
                        {
                            landed = true;
                            foreach (HitEvent h in moments) Land(r, h);
                        }
                        run++;
                        acc -= step;
                        if (r.EndStep(false))
                        {
                            acc %= step;
                            cut = true;
                            break;
                        }
                    }
                }
                int heldSteps = heldFrame || cut ? Math.Max(0, (int)Math.Round(FrameDt / step) - run) : 0;
                if (frame < runIn) continue;
                if (frame > runIn && !heldFrame) break; // the first frame not held after the moment
                held += heldSteps;
            }
            Assert.That(landed, Is.True, "the moment landed on its step");
            return held;
        }

        static ImpactRules Rules(bool feelOn) => new ImpactRules(cfg, ImpactFrames.FreezeTableFor(feelOn));

        static void Ladder(string what, int min, int max, params HitEvent[] moments)
        {
            for (int phase = 0; phase < 2; phase++)
            {
                int held = HeldSteps(Rules(true), phase, moments);
                Assert.That(held, Is.InRange(min, max), $"{what} (the moment on step {phase + 1} of its frame): HELD steps at 120 Hz");
            }
        }

        [Test]
        public void TheLadder_HitBodyKillHeadshotKillPunchKill_InTheirHeldStepRanges()
        {
            Ladder("hit12: a revolver body hit", 0, 2, Hit("revolver"));
            Ladder("kill12: a body kill", 6, 8, Hit("revolver", killed: true));
            Ladder("headshot12: a headshot kill", 10, 13, Hit("revolver", head: true, killed: true));
            Ladder("punchkill: a punch kill", 11, 14, Hit("punch", killed: true, melee: true));
        }

        [Test]
        public void TheLadder_AFiveKillBlastHoldsAtLeast18Steps_AtMost160Ms()
        {
            var five = new HitEvent[5];
            for (int i = 0; i < five.Length; i++) five[i] = Hit("shotgun", killed: true, damage: 5);
            Ladder("horde5: one THUNDERCLAP blast killing five", 18, 19, five);
            ImpactRules r = Rules(true);
            for (int i = 0; i < 300; i++) r.BeginFrame(FrameDt, false);
            foreach (HitEvent h in five) Land(r, h);
            Assert.That(r.EndStep(false), Is.True, "a freeze");
            Assert.That(r.grantedSeconds, Is.LessThanOrEqualTo(0.16 + Eps), "the 160 ms cap (NIGHT18.md M4)");
            Assert.That(r.grantedSeconds, Is.GreaterThan(cfg.Num(ImpactFrames.FreezeTableFor(true) + "headshotKill")), "a horde freezes longer than one headshot kill");
        }

        [Test]
        public void TheOffSideIsNight17sTable_AndTheTwoTablesDiffer()
        {
            Assert.That(ImpactFrames.FreezeTableFor(true), Is.EqualTo(ImpactRules.FreezeTable), "on: design.impact.freeze");
            Assert.That(ImpactFrames.FreezeTableFor(false), Is.EqualTo(ImpactFrames.FreezeTableN17), "off: design.impact.n17.freeze");
            foreach (string tier in new[] { "kill", "headshotKill", "punchKill" })
                Assert.That(cfg.Num(ImpactFrames.FreezeTableN17 + tier), Is.Not.EqualTo(cfg.Num(ImpactRules.FreezeTable + tier)), tier + ": the A / B differ");
            foreach (string tier in new[] { "heavyHit", "headshot", "punchHit", "parry", "perfectParry", "heavyPellets" })
                Assert.That(cfg.Num(ImpactFrames.FreezeTableN17 + tier), Is.EqualTo(cfg.Num(ImpactRules.FreezeTable + tier)), tier + ": unchanged tonight");
            // the off side's body kill freezes NIGHT17's kill tier
            ImpactRules off = Rules(false);
            for (int i = 0; i < 300; i++) off.BeginFrame(FrameDt, false);
            Land(off, Hit("revolver", killed: true));
            Assert.That(off.EndStep(false), Is.True);
            Assert.That(off.grantedSeconds, Is.EqualTo(cfg.Num(ImpactFrames.FreezeTableN17 + "kill")).Within(Eps));
        }

        [Test]
        public void AFeelAbFlip_SwapsTheTableInPlace_TheCooldownsKept()
        {
            ImpactRules r = Rules(true);
            for (int i = 0; i < 300; i++) r.BeginFrame(FrameDt, false);
            Land(r, Hit("revolver", killed: true));
            Assert.That(r.EndStep(false), Is.True);
            Assert.That(r.grantedSeconds, Is.EqualTo(cfg.Num(ImpactRules.FreezeTable + "kill")).Within(Eps), "on: tonight's kill tier");
            r.UseFreezeTable(cfg, ImpactFrames.FreezeTableFor(false));
            Land(r, Hit("revolver", killed: true, shot: 2));
            Assert.That(r.EndStep(false), Is.False, "the swap keeps the cooldowns and the gap (no freeze inside them)");
            for (int i = 0; i < 300; i++) r.BeginFrame(FrameDt, false);
            Land(r, Hit("revolver", killed: true, shot: 3));
            Assert.That(r.EndStep(false), Is.True);
            Assert.That(r.grantedSeconds, Is.EqualTo(cfg.Num(ImpactFrames.FreezeTableN17 + "kill")).Within(Eps), "off: NIGHT17's kill tier");
        }

        // --- M3: the chunks thrown across the screen ------------------------------------------------------------------------

        /// <summary>
        /// The cloud's middle `t` s after its release, in body heights across the screen from the body's middle (`slide`: the
        /// body's held slide back along the shot first, as the THUNDERCLAP's shove; `drag`: tonight's drag, /s at release).
        /// </summary>
        static float ScreenTravel(float distance, Vector3 vel, float t, float drag = 0, float slide = 0)
        {
            var eye = new Vector3(0, 1.6f, 0);
            var body = new Vector3(0, 0.9f, distance);
            float gravity = (float)cfg.Num("fx.gravity"), height = (float)cfg.Num("husk.height");
            Vector3 at = body + new Vector3(0, 0, slide) + CrunchMath.ThrowOffset(vel, t, gravity, UnityTuning.ChunkGravityDelay, drag, UnityTuning.KillThrowDragTime);
            Vector2 Proj(Vector3 p) => new Vector2((p.x - eye.x) / (p.z - eye.z), (p.y - eye.y) / (p.z - eye.z));
            float bodyOnScreen = height / (body.z - eye.z);
            return (Proj(at) - Proj(body)).magnitude / bodyOnScreen;
        }

        /// <summary>Round 3: the cloud's middle's rise alone on screen (body heights), whatever its sideways travel.</summary>
        static float ScreenRise(float distance, Vector3 vel, float t, float drag, float slide)
        {
            var eye = new Vector3(0, 1.6f, 0);
            var body = new Vector3(0, 0.9f, distance);
            float gravity = (float)cfg.Num("fx.gravity"), height = (float)cfg.Num("husk.height");
            Vector3 at = body + new Vector3(0, 0, slide) + CrunchMath.ThrowOffset(vel, t, gravity, UnityTuning.ChunkGravityDelay, drag, UnityTuning.KillThrowDragTime);
            float Py(Vector3 p) => (p.y - eye.y) / (p.z - eye.z);
            return (Py(at) - Py(body)) / (height / (body.z - eye.z));
        }

        static Vector3 Shot(float distance) => (new Vector3(0, 0.9f, distance) - new Vector3(0, 1.6f, 0)).normalized;

        static Vector3 Axis(float d, float side) =>
            CrunchMath.KillThrowAxis(Shot(d), Vector3.right, side, UnityTuning.KillThrowAlong, UnityTuning.KillThrowAcross, UnityTuning.KillThrowLift);

        static readonly float UpMean = (UnityTuning.ChunkUpMin + UnityTuning.ChunkUpMax) * 0.5f;

        [Test]
        public void TheThrow_CarriesTheCloudAcrossTheScreen_250MsAfterItsRelease()
        {
            const float t = 0.25f; // M3: 250 ms after the freeze ends (the blocks' release)
            foreach (float d in new[] { 6f, 12f, 20f })
            {
                foreach (float side in new[] { 1f, -1f })
                {
                    Vector3 axis = Axis(d, side);
                    Assert.That(axis.magnitude, Is.EqualTo(1f).Within(1e-4f), "a unit axis");
                    Assert.That(axis.y, Is.GreaterThan(0.3f), "it rises");
                    Assert.That(Mathf.Abs(axis.x), Is.GreaterThan(0.5f), "it crosses the screen");
                    Assert.That(Mathf.Sign(axis.x), Is.EqualTo(side), "to its side");
                    Assert.That(axis.z, Is.GreaterThan(0f), "and away from the shooter (along the shot)");
                    // the mean block (speed x 1, the cone's mean shortening ~0.97 at 40 deg); the slowest (x 0.8): M3 reads the
                    // cloud's centroid, so the slowest only has to leave the body behind
                    Vector3 mean = axis * UnityTuning.KillThrowSpeed * 0.97f + Vector3.up * UpMean;
                    Vector3 slow = axis * UnityTuning.KillThrowSpeed * 0.8f + Vector3.up * UnityTuning.ChunkUpMin;
                    // round 2: a 7 % margin (the cloud kept nearer, inside the reel's crop: the b3 reviewers)
                    Assert.That(ScreenTravel(d, mean, t, UnityTuning.KillThrowDrag), Is.GreaterThanOrEqualTo(1.6f), $"{d} m: the mean block at least 1.5 body heights across, with a 7 % margin");
                    Assert.That(ScreenTravel(d, slow, t, UnityTuning.KillThrowDrag), Is.GreaterThanOrEqualTo(1.3f), $"{d} m: even the slowest block well clear of the body");
                }
                // NIGHT17's throw (along the shot, ChunkUp only): the swell in place the reviewers saw
                Vector3 n17 = Shot(d) * UnityTuning.ChunkThrowSpeed + Vector3.up * UpMean;
                Assert.That(ScreenTravel(d, n17, t), Is.LessThan(1.5f), $"{d} m: NIGHT17's throw stays near the body (the symptom)");
            }
        }

        [Test]
        public void TheThrow_PopsThenHangs_InViewAt400Ms()
        {
            // round 1 (b3 kill12): without drag the cloud left the reel's 480 px crop by +250 ms; the crop's half at 12 m is
            // 240 px = 2.24 body heights (a 1.8 m body is 107 px there): the drag holds the mean block inside it at +400 ms
            Vector3 mean = Axis(12, 1) * UnityTuning.KillThrowSpeed * 0.97f + Vector3.up * UpMean;
            float at250 = ScreenTravel(12, mean, 0.25f, UnityTuning.KillThrowDrag), at400 = ScreenTravel(12, mean, 0.4f, UnityTuning.KillThrowDrag);
            Assert.That(at400, Is.LessThanOrEqualTo(1.9f), "kill12: the cloud's middle well inside the crop at +400 ms (round 2: the cloud stays around the target)");
            Assert.That(at400 - at250, Is.LessThan(0.4f), "it hangs: little further between +250 and +400 ms");
            Assert.That(ScreenTravel(12, mean, 0.4f), Is.GreaterThan(2.24f), "without the drag it would have left the crop (the round 1 symptom)");
        }

        [Test]
        public void ANearKill_ThrowsSlower_InsideTheCropAt400Ms_Kill12Unchanged()
        {
            // round 4 (r3b, kill6: "only sparks": at 6 m the 480 px crop's half is 1.12 body heights): the near throw's share
            Assert.That(CrunchMath.NearThrowShare(12, UnityTuning.KillThrowFullFrom, UnityTuning.KillThrowNearShare), Is.EqualTo(1f), "kill12: the full throw (M3)");
            Assert.That(CrunchMath.NearThrowShare(20, UnityTuning.KillThrowFullFrom, UnityTuning.KillThrowNearShare), Is.EqualTo(1f), "kill20");
            Assert.That(CrunchMath.NearThrowShare(1, UnityTuning.KillThrowFullFrom, UnityTuning.KillThrowNearShare), Is.EqualTo(UnityTuning.KillThrowNearShare), "point blank: the floor");
            float k6 = CrunchMath.NearThrowShare(6, UnityTuning.KillThrowFullFrom, UnityTuning.KillThrowNearShare);
            Vector3 mean = Axis(6, -1) * UnityTuning.KillThrowSpeed * k6 * 0.97f + Vector3.up * UpMean;
            Assert.That(ScreenTravel(6, mean, 0.4f, UnityTuning.KillThrowDrag), Is.LessThanOrEqualTo(1.0f), "kill6: the cloud's middle inside the crop (1.12 body heights) at +400 ms");
            Assert.That(ScreenTravel(6, mean, 0.25f, UnityTuning.KillThrowDrag), Is.GreaterThanOrEqualTo(0.6f), "and still thrown clear of the body");
        }

        [Test]
        public void TheHordesThrow_ClearsItsBody_TheShoveShort()
        {
            // horde5: the THUNDERCLAP; its held body slides back KillShoveSlide (NIGHT17 1.5 m: depth shrank the cloud on screen
            // to 1.22 body heights, b3) and its blocks fly x KillShoveSpeed (round 2; NIGHT17 SigShoveSpeed). Round 3 (b6 1.06:
            // the sideways travel cancelled against the pack's offset from the target husk): the RISE alone clears 1.5 body
            // heights (KillShoveAcross / KillShoveLift), at the N17 reel's 4 m and b6's 6.2 m
            foreach (float d in new[] { 4f, 6.2f })
            {
                Vector3 axis = CrunchMath.KillThrowAxis(Shot(d), Vector3.right, -1, UnityTuning.KillThrowAlong, UnityTuning.KillShoveAcross, UnityTuning.KillShoveLift);
                Vector3 mean = axis * UnityTuning.KillThrowSpeed * UnityTuning.KillShoveSpeed * 0.97f + Vector3.up * UpMean;
                Assert.That(ScreenRise(d, mean, 0.25f, UnityTuning.KillThrowDrag, UnityTuning.KillShoveSlide), Is.GreaterThanOrEqualTo(1.5f * 1.15f), $"horde5 at {d} m: the rise alone at least 1.5 body heights, with a 15 % margin");
            }
            // round 3b: the reel's horde5 dies to the COMET (DeathSig.Blast: a pack within 1 m of the blast, 7 m out): its blocks
            // erupt up from the blast too (outward x KillShoveAcross, up x KillShoveLift, at KillThrowSpeed x SigBlastSpeed
            // against the drag; the lift x SigBlastUp as before): the rise alone clears 1.5 body heights at 7 m
            foreach (float d in new[] { 4f, 7f })
            {
                Vector3 erupt = CrunchMath.EruptAxis(Vector3.right, Vector3.right, UnityTuning.KillShoveAcross, UnityTuning.KillShoveLift);
                Assert.That(erupt.magnitude, Is.EqualTo(1f).Within(1e-4f));
                Assert.That(erupt.y, Is.GreaterThan(0.9f), "mostly up");
                Vector3 mean = erupt * UnityTuning.KillThrowSpeed * UnityTuning.KillBlastSpeed * 0.97f + Vector3.up * UpMean * UnityTuning.SigBlastUp;
                Assert.That(ScreenRise(d, mean, 0.25f, UnityTuning.KillThrowDrag, 0), Is.GreaterThanOrEqualTo(1.5f * 1.15f), $"horde5 (the COMET) at {d} m: the rise alone at least 1.5 body heights, with a 15 % margin");
            }
            Assert.That(CrunchMath.EruptAxis(Vector3.zero, Vector3.right, UnityTuning.KillShoveAcross, UnityTuning.KillShoveLift).y, Is.GreaterThan(0.9f), "a block at the blast's point: up (the fallback side)");
            Assert.That(UnityTuning.KillShoveSlide, Is.LessThan(UnityTuning.KillShoveSlideN17), "a shorter shove than NIGHT17's");
            Assert.That(UnityTuning.KillShoveSpeed, Is.GreaterThan(UnityTuning.KillShoveSpeedN17), "a harder shove throw than NIGHT17's");
        }

        [Test]
        public void TheThrowsSide_IsTheOpenOne_ElseItAlternates()
        {
            float need = UnityTuning.KillSideRoom;
            Assert.That(CrunchMath.ThrowSide(need * 2, need * 0.5f, -1), Is.EqualTo(1f), "a wall close on the left: right");
            Assert.That(CrunchMath.ThrowSide(need * 0.5f, need * 2, 1), Is.EqualTo(-1f), "a wall close on the right: left");
            Assert.That(CrunchMath.ThrowSide(need * 2, need * 2, -1), Is.EqualTo(-1f), "both open: the preferred side");
            Assert.That(CrunchMath.ThrowSide(need * 2, need * 2, 1), Is.EqualTo(1f));
            Assert.That(UnityTuning.KillSidePreferred, Is.EqualTo(-1f), "round 2: the left, away from the points pop (up and right of a kill)");
            Assert.That(CrunchMath.ThrowSide(need * 0.3f, need * 0.6f, 1), Is.EqualTo(-1f), "both close: the roomier side");
            Assert.That(CrunchMath.ThrowSide(need * 0.6f, need * 0.3f, -1), Is.EqualTo(1f));
        }

        // --- M2: the burst's screen floor, and its cap at a punch's reach ----------------------------------------------------

        static float ViewTan(float fovDeg) => 2 * Mathf.Tan(fovDeg * 0.5f * Mathf.Deg2Rad);

        [Test]
        public void TheBurst_ReadsAt20m_AndNeverFloodsAPunchKill()
        {
            float fov = (float)cfg.Num("settings.fovDeg"), height = (float)cfg.Num("husk.height"), vt = ViewTan(fov);
            float Share(float d) => CrunchMath.KillBurstSize(height, d, vt, UnityTuning.KillBurstBodies, UnityTuning.KillBurstMaxAngle, UnityTuning.KillBurstMinScreen)
                                    * UnityTuning.KillBurstVisibleShare / (d * vt);
            Assert.That(Share(20), Is.GreaterThanOrEqualTo(0.08f), "kill20: the burst's visible part at least 8 % of the screen's height (M2)");
            Assert.That(Share(12), Is.GreaterThanOrEqualTo(0.08f), "kill12");
            Assert.That(Share(6), Is.GreaterThanOrEqualTo(0.08f), "kill6");
            Assert.That(Share(1.4f), Is.LessThanOrEqualTo(0.4f), "a punch kill at 1.4 m: under 40 % of the screen's height");
            Assert.That(Share(4), Is.LessThanOrEqualTo(0.4f), "a horde at 4 m: each burst under 40 %");
            // NIGHT17's numbers through the same rule give NIGHT17's size (the off side unchanged)
            float n17 = Mathf.Max(Mathf.Min(height * UnityTuning.BurstBodies, UnityTuning.BurstMaxAngle * 12), 12 * vt * UnityTuning.BurstMinScreen);
            Assert.That(CrunchMath.KillBurstSize(height, 12, vt, UnityTuning.KillBurstBodiesN17, UnityTuning.KillBurstMaxAngleN17, UnityTuning.KillBurstMinScreenN17), Is.EqualTo(n17).Within(1e-5f));
        }

        // --- the burst's frames and the horde's scale ----------------------------------------------------------------------

        [Test]
        public void TheBurst_HoldsTwoFullFramesThenFades_NIGHT17sWasOneAndAHalf()
        {
            float S(int f, double fi = 1) => CrunchMath.BurstStrength(f, UnityTuning.KillBurstFullFrames, UnityTuning.KillBurstFadeFrames, fi);
            Assert.That(S(0), Is.EqualTo(1f), "the impact frame: full");
            Assert.That(S(1), Is.EqualTo(1f), "the next: full (M1 reads the first frame after the shot)");
            Assert.That(S(2), Is.InRange(0.01f, 0.99f), "then fading");
            Assert.That(S(3), Is.LessThan(S(2)).And.GreaterThan(0f));
            Assert.That(S(4), Is.EqualTo(0f), "gone by the 5th frame");
            Assert.That(S(-1), Is.EqualTo(0f));
            Assert.That(S(0, 0.5), Is.EqualTo(0.5f).Within(1e-6f), "x FLASH INTENSITY");
            Assert.That(S(0, 0), Is.EqualTo(0f), "FLASH INTENSITY 0: none");
            for (int f = 0; f < 4; f++)
                Assert.That(CrunchMath.BurstStrength(f, UnityTuning.KillBurstFullFramesN17, UnityTuning.KillBurstFadeFramesN17, 1), Is.EqualTo(CrunchMath.StarStrength(f, 2, 0.5, 1)).Within(1e-6f),
                    "off: NIGHT17's two frames (full, then half)");
        }

        // --- NIGHT18 U3-31.9 backlog 3: the spawn ring in DANGER (a role fix) ---------------------------------------------

        [Test]
        public void TheSpawnRingAndCore_HoldTheirDangerHue_UnderTheBloomThreshold()
        {
            var (r, g, b) = cfg.Rgb("colors.enemy");
            double bloom = cfg.Num("post.bloomThreshold");
            // additive DANGER whose brightest linear channel stays at or under the bloom threshold: no clipped channel, no
            // white bloom halo, so the tone map keeps its red-orange (NIGHT17-RETRO: "the spawn ring reads near white")
            Assert.That(CrunchMath.AdditivePeak(r, g, b, UnityTuning.DangerRingHdr), Is.LessThanOrEqualTo(bloom), "tonight's ring");
            Assert.That(CrunchMath.AdditivePeak(r, g, b, UnityTuning.DangerCoreHdr), Is.LessThanOrEqualTo(bloom), "tonight's core");
            Assert.That(CrunchMath.AdditivePeak(r, g, b, UnityTuning.DangerRingHdrN17), Is.GreaterThan(bloom), "NIGHT17's ring blew out (the symptom)");
            Assert.That(CrunchMath.AdditivePeak(r, g, b, UnityTuning.DangerCoreHdrN17), Is.GreaterThan(bloom), "NIGHT17's core blew out");
            Assert.That(CrunchMath.AdditivePeak(1, 1, 1, 2), Is.EqualTo(2).Within(1e-9), "white x 2: 2");
            Assert.That(CrunchMath.AdditivePeak(0.5, 0, 0, 1), Is.EqualTo(0.214).Within(1e-3), "sRGB 0.5 is linear 0.214");
            Assert.That(UnityTuning.DangerRingWidth, Is.GreaterThan(UnityTuning.DangerRingWidthN17), "a wider band for the dimmer ring");
        }

        // --- NIGHT18 U3-31.9 backlog 4: the low-HP vignette pulse (PostFx) ---------------------------------------------------

        [Test]
        public void TheLowHpPulse_ABeatThatQuickensAsHpFalls_UnderThreeBumpsASecond()
        {
            double rest = UnityTuning.LowHpPulseRestHz, low = UnityTuning.LowHpPulseLowHz;
            Assert.That(CrunchMath.HeartRate(0, rest, low), Is.EqualTo(rest).Within(1e-9), "at the low-HP line: the resting beat");
            Assert.That(CrunchMath.HeartRate(1, rest, low), Is.EqualTo(low).Within(1e-9), "at 0 HP: the quick beat");
            Assert.That(CrunchMath.HeartRate(0.5, rest, low), Is.InRange(rest, low));
            Assert.That(CrunchMath.HeartRate(2, rest, low), Is.EqualTo(low).Within(1e-9), "clamped");
            Assert.That(low, Is.GreaterThan(rest), "it quickens");
            Assert.That(low * 2, Is.LessThanOrEqualTo(3.0), "two bumps a beat: at most 3 a second (WCAG 2.3.1's three flashes)");
            double peak = 0, second = 0, tail = 0;
            for (int i = 0; i < 1000; i++)
            {
                double ph = i / 1000.0, v = CrunchMath.HeartPulse(ph);
                Assert.That(v, Is.InRange(0.0, 1.0), "0..1");
                if (ph < 0.2) peak = Math.Max(peak, v);
                else if (ph < 0.45) second = Math.Max(second, v);
                else if (ph >= 0.55) tail = Math.Max(tail, v);
            }
            Assert.That(peak, Is.GreaterThan(0.95), "the beat");
            Assert.That(second, Is.InRange(0.4, 0.8), "its smaller echo");
            Assert.That(tail, Is.LessThan(0.05), "then quiet");
            Assert.That(Math.Abs(CrunchMath.HeartPulse(0) - CrunchMath.HeartPulse(0.9999)), Is.LessThan(0.02), "no jump at the wrap");
            Assert.That(UnityTuning.LowHpPulseAmpN17, Is.EqualTo(0f), "off: NIGHT17's steady vignette");
            Assert.That(UnityTuning.LowHpPulseTintN17, Is.EqualTo(0f), "off: NIGHT17's black vignette");
        }

        [Test]
        public void AHordeFrame_BurstsBigger_UpToItsCap()
        {
            Assert.That(CrunchMath.HordeScale(1, UnityTuning.HordeBurstPerKill, UnityTuning.HordeBurstMax), Is.EqualTo(1f), "one kill: as is");
            Assert.That(CrunchMath.HordeScale(5, UnityTuning.HordeBurstPerKill, UnityTuning.HordeBurstMax), Is.GreaterThan(1.2f), "five: bigger");
            Assert.That(CrunchMath.HordeScale(50, UnityTuning.HordeBurstPerKill, UnityTuning.HordeBurstMax), Is.EqualTo(UnityTuning.HordeBurstMax), "capped");
            Assert.That(CrunchMath.HordeScale(0, UnityTuning.HordeBurstPerKill, UnityTuning.HordeBurstMax), Is.EqualTo(1f));
        }
    }
}
