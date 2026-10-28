// NIGHT42 builder F1 (design.n42.kills42, a view row: no sim bit; G9 "hits and kills need to feel more satifying", G15 "a greater or mor
// epowerful effect when hitting it or any other enemy iwth gaughtlent"). The picture of the sim's NIGHT42 impact events (emitted only with
// the sim row impact42 on: Game/SurvivalSim.N42.Impact.cs), so the two rows go on together:
//   - `gauntletHit42`: a camera-facing shockwave ring at the struck body (two rings, the gauntlet's kinetic cyan, additive, growing on frame time
//     so it moves while the world is frozen), a camera kick (pitch up + a roll that springs back, after CameraFeel), trauma into the web shake,
//     a spark burst; every size by the body's class (a husk, a big kind, a boss: THE ICEBREAKER's the biggest). The freeze is ImpactRules' tier
//     Gauntlet42; the FOV punch-in and the rumble are ImpactFrames.Show's.
//   - `killTier42`: a bigger debris burst in the enemy colour and a trauma kick by tier; the chunks' throw x Kills42ThrowMult (EnemyViews.Death.cs
//     reads ThrowMult: a crunchier break).
// REDUCED MOTION drops the kick and the trauma (the ring and the bursts stay); EFFECTS INTENSITY scales the kick. View only: nothing here is
// read by the sim. Probe: KillFeel42Probe (the hits42 reel's telemetry).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>NIGHT42 F1: what KillFeel42 did (the reels read it; all 0 with the row off).</summary>
    public static class KillFeel42Probe
    {
        public static int Rings, Kicks, KillBursts, LastClass = -1, LastTier;
        public static float RingRadius, KickDeg; // the newest ring's radius now (m), the kick's pitch now (deg)

        public static void Clear()
        {
            Rings = Kicks = KillBursts = LastTier = 0;
            LastClass = -1;
            RingRadius = KickDeg = 0;
        }
    }

    public sealed class KillFeel42
    {
        struct Ring
        {
            public Vector3 at;
            public double age;
            public float radius;
            public bool live;
        }

        readonly SurvivalGame game;
        bool on;
        readonly Ring[] rings = new Ring[UnityTuning.Kills42RingPool * 2];
        readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[UnityTuning.Kills42RingPool * 2];
        Mesh ringMesh, ringThin;
        RenderParams rp;
        bool built;
        Color cyan, enemy;
        Vector4 cyanLinear;
        int nextRing;
        double kickAge = double.MaxValue;
        float kickDeg, kickSide = 1;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>The row's state for the static readers (the death view's throw): set at construction.</summary>
        static bool live;

        public bool On => on;

        public KillFeel42(SurvivalGame game, GameConfig cfg, EventBus events)
        {
            this.game = game;
            this.events = events;
            live = false;
            if (N42View.RowOn(cfg, "kills42")) Force(true);
        }

        readonly EventBus events;
        bool hooked;

        /// <summary>The row on / off now (the constructor: the design row; the hits42 reel: its off / on passes). Hooks the bus once.</summary>
        public void Force(bool v)
        {
            if (v && !hooked)
            {
                hooked = true;
                GameConfig cfg = game.cfg;
                cyan = WeaponLook.FamilyColor(cfg, "gauntlet", Look.C(cfg, "colors.spark"));
                if (Fx47View.RowOn(cfg)) cyan = Look.Hex(UnityTuning.Fx47BodyHex); // NIGHT47 F fx47 (G21): the gauntlet's rings and sparks in its own pink (the hits were cyan: the colour lied)
                enemy = Look.C(cfg, "colors.enemy");
                cyanLinear = cyan.linear;
                for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
                events.On<GauntletHit42Event>(GauntletHit42Event.Name, OnGauntlet);
                events.On<KillTier42Event>(KillTier42Event.Name, OnKill);
                game.OnNewRun += Clear;
            }
            on = v;
            live = v;
            if (!v) Clear();
        }

        /// <summary>The chunks' throw multiplier for a kill (1 with the row off): a crunchier break by the kill's tier, read from the kill itself
        /// (the death view breaks the body inside the `kill` emit, before the sim's killTier42).</summary>
        public static float ThrowMult(KillEvent e, float throwK)
        {
            if (!live || e == null || e.owner != "player") return throwK;
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.sim == null || !g.sim.n42.impact42) return throwK;
            int tier = e.kind == "boss" ? KillTier42Event.Big
                : e.melee || e.weapon == "gauntlet" || e.weapon == "punch" ? KillTier42Event.Melee
                : e.headshot || e.weapon == "rail" || e.weapon == "rocket" || e.weapon == "shotgun" ? KillTier42Event.Heavy
                : KillTier42Event.Plain;
            return Mathf.Min(throwK * UnityTuning.Kills42ThrowMult[tier], Mathf.Max(throwK, UnityTuning.Kills42ThrowMax));
        }

        /// <summary>NIGHT42 F1: the hit flinch's multiplier for a hit of the player's by weapon (1 with kills42 or impact42 off, or for another
        /// owner / a wave's share): EnemyViews.Hit.cs scales the tilt share and the knock-back with it.</summary>
        public static float FlinchMult(HitEvent e)
        {
            if (!live || e == null || e.owner != "player" || e.shock || e.killed) return 1;
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.sim == null || !g.sim.n42.impact42) return 1;
            switch (e.weapon)
            {
                case "revolver": return UnityTuning.Kills42FlinchRevolver;
                case "shotgun": return UnityTuning.Kills42FlinchShotgun;
                case "nailgun": return UnityTuning.Kills42FlinchNail;
                case "gauntlet": return UnityTuning.Kills42FlinchGauntlet;
                default: return 1;
            }
        }

        /// <summary>NIGHT42 F1: hit flinches scaled since boot (the reels).</summary>
        public static int Flinches;

        bool Calm() => game.cfg.movement.settings.reducedMotion;

        void OnGauntlet(GauntletHit42Event e)
        {
            if (!on || e.owner != "player") return;
            int cls = Mathf.Clamp(e.cls, 0, 2);
            Vector3 at = WebSpace.ToUnity(e.position);
            // NIGHT49 X fx49 round 3 (D-17; n49-r2 a249: the PULSE punch +0.31 ON in the wash box): the gauntlet's rings and spark burst were a
            // second ring and spark set over the kit's own (Fx49Kit: the gauntlet's three-tone ring and sparks): with fx49 the kit's only. The
            // kick and the trauma stay.
            if (!Fx49.On(game.cfg))
            {
                AddRing(at, UnityTuning.Kills42RingRadius[cls], 0);
                AddRing(at, UnityTuning.Kills42RingRadius[cls] * UnityTuning.Kills42SecondRing, -1.0 / 60); // a frame behind
                game.ParticleBurst(e.position, cyan, UnityTuning.Kills42SparkCount[cls], UnityTuning.Kills42SparkSpeed, UnityTuning.Kills42SparkLife, UnityTuning.Kills42SparkScale);
            }
            if (!Calm())
            {
                kickDeg = UnityTuning.Kills42KickDeg[cls];
                kickAge = 0;
                kickSide = -kickSide;
                game.trauma.Add(UnityTuning.Kills42Trauma[cls]);
                KillFeel42Probe.Kicks++;
            }
            KillFeel42Probe.LastClass = cls;
        }

        void OnKill(KillTier42Event e)
        {
            if (!on || e.owner != "player") return;
            int t = Mathf.Clamp(e.tier, 0, 4);
            KillFeel42Probe.LastTier = t;
            if (UnityTuning.Kills42KillCount[t] > 0)
            {
                // NIGHT49 X fx49 round 3 (the kill +0.32 ON): fewer extra debris bits (their colour, the enemy's, unchanged)
                int count = Fx49.On(game.cfg) ? Mathf.Max(1, Mathf.RoundToInt(UnityTuning.Kills42KillCount[t] * UnityTuning.Fx49KillDebrisShare)) : UnityTuning.Kills42KillCount[t];
                game.ParticleBurst(e.position, enemy, count, UnityTuning.Kills42KillSpeed, UnityTuning.Kills42KillLife, 1);
                KillFeel42Probe.KillBursts++;
            }
            if (UnityTuning.Kills42KillTrauma[t] > 0 && !Calm()) game.trauma.Add(UnityTuning.Kills42KillTrauma[t]);
        }

        void AddRing(Vector3 at, float radius, double age)
        {
            rings[nextRing] = new Ring { at = at, radius = radius, age = age, live = true };
            nextRing = (nextRing + 1) % rings.Length;
            KillFeel42Probe.Rings++;
        }

        void Clear()
        {
            for (int i = 0; i < rings.Length; i++) rings[i].live = false;
            kickAge = double.MaxValue;
            KillFeel42Probe.Clear();
        }

        void Build()
        {
            built = true;
            ringMesh = FxDraw.Ring(1 - UnityTuning.Kills42RingWidth, 1, UnityTuning.Kills42RingSegments, "Gauntlet ring 42");
            ringThin = FxDraw.Ring(1 - UnityTuning.Kills42RingWidth * 0.5f, 1, UnityTuning.Kills42RingSegments, "Gauntlet ring 42 thin");
            Material m = FxDraw.Additive(game.mats.neon, "Gauntlet ring 42");
            m.SetFloat("_Cull", 0);
            rp = new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
        }

        /// <summary>Once a frame after the camera (ImpactFrames.Render): the kick on the camera, the rings drawn and grown on frame time.</summary>
        public void Render(double frameDt)
        {
            if (!on) return;
            Camera cam = game.cam;
            if (cam == null) return;
            if (!built) Build();
            // the kick: a pitch up and a roll, springing back (ease-out cubic) on frame time, x EFFECTS INTENSITY
            KillFeel42Probe.KickDeg = 0;
            if (kickAge < UnityTuning.Kills42KickTime)
            {
                kickAge += frameDt;
                double u = Math.Min(1, kickAge / UnityTuning.Kills42KickTime);
                double w = 1 - u;
                float k = (float)(kickDeg * w * w * w * Math.Max(0, Math.Min(1, game.cfg.movement.settings.effectsIntensity)));
                cam.transform.rotation = cam.transform.rotation * Quaternion.Euler(-k, 0, k * UnityTuning.Kills42KickRoll * kickSide);
                KillFeel42Probe.KickDeg = k;
            }
            Quaternion face = Quaternion.FromToRotation(Vector3.up, -cam.transform.forward);
            // NIGHT49 X fx49 (Fx49.cs): the gauntlet's rings on the kit: its heavy tier's life and glow, its pink (the hue fx47 already gave it)
            bool k49 = Fx49.On(game.cfg);
            double ringTime = k49 ? Fx49.LifeS(Fx49Tier.Heavy) : UnityTuning.Kills42RingTime;
            Vector4 ringLin = cyanLinear;
            float ringHdr = UnityTuning.Kills42RingHdr;
            if (k49) { Color h = Fx49.Hue(Fx49Owner.Gauntlet).linear; ringLin = new Vector4(h.r, h.g, h.b, 1); ringHdr = Fx49.Cell(Fx49Owner.Gauntlet, Fx49Tier.Heavy).glow; }
            for (int i = 0; i < rings.Length; i++)
            {
                if (!rings[i].live) continue;
                rings[i].age += frameDt;
                double t = rings[i].age / ringTime;
                if (t >= 1)
                {
                    rings[i].live = false;
                    continue;
                }
                if (t < 0) continue; // the second ring's frame behind
                float grow = 1 - (float)((1 - t) * (1 - t) * (1 - t)); // ease-out cubic
                float maxR = Vector3.Distance(cam.transform.position, rings[i].at) * Mathf.Tan(UnityTuning.Kills42RingMaxDeg * Mathf.Deg2Rad); // n42-r2: capped on screen
                float r = Mathf.Min(rings[i].radius, maxR) * (UnityTuning.Kills42RingStart + (1 - UnityTuning.Kills42RingStart) * grow);
                float hdr = ringHdr * (float)((1 - t) * (1 - t));
                blocks[i].SetVector(BaseColor, new Vector4(ringLin.x * hdr, ringLin.y * hdr, ringLin.z * hdr, 1));
                rp.matProps = blocks[i];
                rp.worldBounds = new Bounds(rings[i].at, Vector3.one * (2 * r + 0.1f));
                Graphics.RenderMesh(rp, (i & 1) == 0 ? ringMesh : ringThin, 0, Matrix4x4.TRS(rings[i].at, face, new Vector3(r, 1, r)));
                KillFeel42Probe.RingRadius = r;
            }
        }
    }
}
