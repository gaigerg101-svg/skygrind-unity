// NIGHT20 (builder V): the effects' view hooks (SurvivalGame.N20.cs's N20FxInit / N20FxRender): the AIR streaks
// (SpeedLines20.cs, U3-34.7 steps 1-2) and the travel blur's per-frame values (Render/TravelBlurFeature.cs, step 3: the
// pass reads them on the main camera). Made after NIGHT19's views; drawn in the projectiles' block on the world's time.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        SpeedLines20 speedLines20;
        int blurSettingsSeen = -1;
        bool blurSetting = UnityTuning.MotionBlurDefault;

        HuntSky20 huntSky; // NIGHT20 A2: THE HUNT's dusk shift (HuntSky20.cs)

        /// <summary>NIGHT20 C2: the LANCERs' lasers and beams (LancerBeam20.cs): E's view calls Aim each frame a LANCER aims and
        /// Fire on its `lancerFire`; drawn in this frame's effects block (after the enemies' views).</summary>
        public LancerBeam20 LancerBeams { get; private set; }

        /// <summary>NIGHT20 A1: the web's telegraph language (Telegraph20.cs: telegraph.js's circle / line / lane / hazard / flash /
        /// threat in web space): the BULWARK's RAM line and stomp rings, every boss's warnings.</summary>
        public Telegraph20 Telegraphs { get; private set; }

        /// <summary>NIGHT20 A1: the BULWARK's shock ring and sweep beam (BulwarkFx20.cs; its telegraphs are Telegraphs').</summary>
        public BulwarkFx20 BulwarkFx { get; private set; }

        /// <summary>NIGHT20 B5: the VORTEX's orbs, rings, motes and burst rings (VortexFx20.cs; builder G2's Projectiles.vortexes).</summary>
        public VortexFx20 VortexFx { get; private set; }

        /// <summary>NIGHT20: gunFx.js's sprays for tonight's guns (GunFx.Spray20.cs: doubleBlast, coneWave, the TORCH's flame).</summary>
        public GunSpray20 GunSprays { get; private set; }

        /// <summary>NIGHT20 C3: the affixes' looks (AffixFx20.cs).</summary>
        public AffixFx20 AffixFx { get; private set; }

        /// <summary>The run's affixes (the sim's; null before a run).</summary>
        public Affixes AffixesNow => sim != null ? sim.affixes : null;

        /// <summary>NIGHT20 A2 / A4: THE HUNT's ring and THE ELITE's aura (SpecialFx20.cs).</summary>
        public SpecialFx20 SpecialFx { get; private set; }

        /// <summary>The run's special rounds (the sim's: EliteId, HuntRound; null before a run or with the rows off).</summary>
        public SpecialRounds SpecialsNow => sim != null ? sim.specials : null;

        /// <summary>particles.js burst on the game's pool (the affixes' and the views' bursts).</summary>
        public void ParticleBurst(Vec3 at, Color color, int count, float speed, float life, float scale) => particles?.Burst(at, color, count, speed, life, scale);

        /// <summary>THE HUNT's dusk shift now (0..1).</summary>
        public double HuntSkyShare => huntSky != null ? huntSky.Share : 0;

        partial void N20FxInit()
        {
            speedLines20 = new SpeedLines20(this);
            huntSky = new HuntSky20(this);
            LancerBeams = new LancerBeam20(this);
            Telegraphs = new Telegraph20(this);
            if (sim != null) sim.TelegraphView = Telegraphs; // NIGHT20 A1 (builder HV): the sim's bosses draw through it (Core's ITelegraph)
            BulwarkFx = new BulwarkFx20(this);
            GunSprays = new GunSpray20(this);
            VortexFx = new VortexFx20(this);
            AffixFx = new AffixFx20(this);
            SpecialFx = new SpecialFx20(this);
        }

        partial void N20FxRender(double alpha, double frameDt)
        {
            speedLines20?.Render(); // world time (game.WorldDt: frozen in a hitstop and the pause)
            TravelBlurFrame();
            if (huntSky != null)
            {
                if (flow == Flow.Title && huntSky.Share > 0) huntSky.Reset(); // TITLE: the dusk at once
                huntSky.Render(RealFrameDt20()); // frame time (a slow-mo doesn't slow the sky)
            }
            if (LancerBeams != null)
            {
                LancerBeams.Watch(enemies); // the live registry (E's LANCERs read each frame)
                LancerBeams.Render(frameDt); // the enemies' frame time (lancer.js render(alpha, frameDt)): 0 in a hitstop
            }
            if (flow == Flow.Title && Telegraphs != null && Telegraphs.model.Count > 0) Telegraphs.model.Clear();
            Telegraphs?.Render(frameDt); // telegraph.js update(frameDt): the world's frame time (a freeze holds the fill)
            if (flow != Flow.Title) BulwarkFx?.Render(); // the BULWARK's shock ring and sweep beam (the sim's state, each frame)
            GunSprays?.Render(frameDt); // the guns' sprays: the world's time (frozen in a hitstop)
            if (flow != Flow.Title) VortexFx?.Render(alpha, frameDt); // the VORTEX's orbs (the sim's, interpolated) and their motes
            AffixFx?.Render(alpha, frameDt); // the affixes' looks: the world's time
            SpecialFx?.Render(alpha, RealFrameDt20(), frameDt); // the hunt's ring (frame time), the elite's aura (the world's)
        }

        /// <summary>This frame's real delta (N20FxRender's own is the world's: 0 in a hitstop, x the slow-mo).</summary>
        double RealFrameDt20() => FrameDt;

        /// <summary>
        /// The travel blur's strength and the eye's travel this frame (TravelBlur.Set): the Mover's linear velocity only,
        /// 0 under TravelBlurFrom m/s, whole at the boost, x EFFECTS INTENSITY; none with MOTION BLUR off, REDUCED MOTION, the
        /// A side (-skygrindOff20), outside play or in a freeze (a still frame stays sharp).
        /// </summary>
        void TravelBlurFrame()
        {
            if (cam == null) return;
            if (settings != null && settings.version != blurSettingsSeen)
            {
                blurSettingsSeen = settings.version;
                blurSetting = settings.store.Item(UnityTuning.MotionBlurSetting) != null ? settings.Bool(UnityTuning.MotionBlurSetting) : UnityTuning.MotionBlurDefault;
            }
            MovementConfig mc = cfg.movement;
            bool on = SpeedLines20.Live && blurSetting && !mc.settings.reducedMotion && Active && WorldDt > 0;
            Vec3 wv = mover.velocity;
            double intensity = System.Math.Max(0, System.Math.Min(mc.feel.effectsIntensityMax, mc.settings.effectsIntensity));
            float s = (float)(TravelBlurMath.Strength(SpeedLines20Math.VEff(wv.x, wv.y, wv.z), intensity, on) * (1 - SkygrindTimeLook)); // SKYGRIND TIME: still
            Transform t = cam.transform;
            Vector3 disp = s > 0 ? TravelBlurMath.ViewDisp(WebSpace.ToUnity(wv), t.right, t.up, t.forward, UnityTuning.TravelBlurShutter) : Vector3.zero;
            float tanV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            TravelBlur.Set(cam, s, disp, tanV * cam.aspect, tanV);
        }
    }
}
