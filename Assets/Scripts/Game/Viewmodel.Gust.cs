// NIGHT19 U3-33.5 (builder V): the KESTREL's gust in the hands (port-only: the web's KESTREL zooms on its alt; the gust is
// NIGHT19's, design.gust, fired by the sim: builder W's N19Events.Gust). The gun first thrusts forward (pushing the air
// out, GustThrustZ over GustThrustTime), then the blowback shoves it back toward the eye, flips the muzzle up and rolls
// the wrist open (Spring-It-On's critically damped decay spring, Viewmodel.Springs.cs: a peak at ~54 ms, settled without
// a wobble); a gust that launches the body (aimed down) also drops the gun in view (its inertia). The gem flashes through
// the game's one FlashGate x FLASH INTENSITY. REDUCED MOTION stills the kick entirely (the gem still flashes, gated).
// The kick plays on max(world, frame) time (a freeze never holds it at its peak), drawn after the gun's own kick.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The gust's pose in the hands, pure (HandsN19Tests): the thrust, the blowback, the launch's drop.</summary>
    public sealed class HandsGust
    {
        public const int Channels = 6; // 0 pitch deg, 1 back m, 2 drop m, 3 roll deg, 4 yaw deg, 5 side m
        readonly HalfLifeKick kick = new HalfLifeKick(Channels, UnityTuning.GustKickHalflife);
        static readonly double[] Peaks =
        {
            UnityTuning.GustKickPitchDeg, UnityTuning.GustKickBackZ, UnityTuning.GustKickDropY, UnityTuning.GustKickRollDeg, 0, UnityTuning.GustKickSideX,
        };
        double age = double.PositiveInfinity, motion;
        bool lift, pending;

        /// <summary>A gust: `motion` the effects' share (0 with REDUCED MOTION: nothing moves), `launchesUp` a gust aimed down.
        /// The thrust plays first; the blowback's kick lands GustKickDelay s later.</summary>
        public void Fire(double motion, bool launchesUp) => Fire(motion, launchesUp, 1);

        /// <summary>NIGHT20: the same at `scale` x the gust's size (the THUNDERCLAP's shockwave: UnityTuning.ShockKickMult).</summary>
        public void Fire(double motion, bool launchesUp, double scale)
        {
            if (!(motion > 0) || !(scale > 0)) return;
            this.motion = Math.Min(1, motion) * scale;
            lift = launchesUp;
            age = 0;
            pending = true;
        }

        public void Step(double dt)
        {
            if (!(dt > 0)) return;
            if (age < double.PositiveInfinity)
            {
                age += dt;
                if (pending && age >= UnityTuning.GustKickDelay)
                {
                    pending = false;
                    kick.Kick(Peaks, motion);
                }
                if (!pending && age > Math.Max(UnityTuning.GustThrustTime, UnityTuning.GustAirLiftTime)) age = double.PositiveInfinity;
            }
            kick.Step(dt);
        }

        /// <summary>The offsets now (web camera space, added to the viewmodel's).</summary>
        public HandsDrawPose Sample()
        {
            var p = new HandsDrawPose
            {
                pitchDeg = kick.Value(0),
                z = kick.Value(1),
                y = -kick.Value(2),
                rollDeg = kick.Value(3),
                yawDeg = kick.Value(4),
                x = kick.Value(5),
            };
            if (age < UnityTuning.GustThrustTime) p.z -= UnityTuning.GustThrustZ * motion * Math.Sin(Math.PI * age / UnityTuning.GustThrustTime);
            if (lift && age < UnityTuning.GustAirLiftTime) p.y -= UnityTuning.GustAirLiftY * motion * Math.Sin(Math.PI * age / UnityTuning.GustAirLiftTime);
            return p;
        }

        public bool Active => kick.Active || age < double.PositiveInfinity;

        public void Clear()
        {
            kick.Clear();
            age = double.PositiveInfinity;
        }
    }

    public sealed partial class Viewmodel
    {
        readonly HandsGust gustHands = new HandsGust();

        void GustBuild()
        {
            game.events.On<GustEvent>(N19Events.Gust, OnGust);
            game.events.On(N20Events.Shockwave, OnShockwaveHands); // NIGHT20 U3-34.3: the THUNDERCLAP's shockwave in the hands
        }

        int shockHandsFrame = -1;

        /// <summary>NIGHT20 U3-34.3 (builder V): the shockwave's shove on the THUNDERCLAP's pose: the gust's blowback x
        /// ShockKickMult (the double blast is a bigger push than NIGHT19's gust), once a frame.</summary>
        void OnShockwaveHands(object payload)
        {
            if (!(payload is ShockwaveEvent e) || e.owner != "player" || shockHandsFrame == UnityEngine.Time.frameCount) return;
            shockHandsFrame = UnityEngine.Time.frameCount;
            Blowback(e.dir.y, UnityTuning.ShockKickMult);
        }

        /// <summary>The bus's gust: the hands' blowback; the draw's flourish and the movement poses yield to it.</summary>
        void OnGust(GustEvent e)
        {
            if (e == null || e.owner != "player") return;
            Blowback(e.dir.y, 1);
        }

        /// <summary>The air's blowback in the hands (a gust's, or x `scale` a shockwave's), the gem's gated flash.</summary>
        void Blowback(double aimY, double scale)
        {
            DrawCancel();
            feelMove.Shot();
            feelFlourish.Cancel();
            MovementConfig mc = cfg.movement;
            double motion = mc.settings.reducedMotion ? 0 : Clamp(mc.settings.effectsIntensity, 0, mc.feel.effectsIntensityMax);
            gustHands.Fire(motion, -aimY > UnityTuning.GustPuffMinUp, scale);
            double fi = FlashIntensity();
            if (fi > 0 && rigs.TryGetValue(game.loadout != null ? game.loadout.Weapon : "revolver", out Rig rig) && game.flashGate.TryFlash(SurvivalGame.RealNow))
            {
                rig.flash = v.gemFlashTime;
                rig.flashScale = fi * UnityTuning.GustGemFlash;
            }
        }

        /// <summary>The gust's offsets this frame (max(world, frame) time), added to the pose sums.</summary>
        void GustFrame(double animDt, double frameDt, ref double px, ref double py, ref double pz, ref double rx, ref double ry, ref double rz)
        {
            gustHands.Step(HandsKick.ClockDt(animDt, frameDt));
            if (!gustHands.Active) return;
            HandsDrawPose p = gustHands.Sample();
            px += p.x;
            py += p.y;
            pz += p.z;
            rx += p.pitchDeg * Deg;
            ry += p.yawDeg * Deg;
            rz += p.rollDeg * Deg;
        }
    }
}
