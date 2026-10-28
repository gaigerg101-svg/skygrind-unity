// NIGHT27 skate27 (builder V; design.n27.skate27): the ice skate feel. VIEW ONLY: it reads the mover and the level's ice zones (Surface26's data,
// SurvivalSim.surfaces26) and changes the camera after the camera feel wrote it (LateUpdate: the feel assigns the FOV and the rotation absolutely every
// frame, so these adds never accumulate) and draws extras; nothing reaches the sim, the tape or the fingerprint. NO screen-border streaks (GG banned them).
// On an ice rect (the player grounded on it) it gives: a speed FOV (0 at run speed .. +fovMaxDeg at fovFullMult x, in over fovInS, out over fovOutS, capped);
// a camera roll from the sideways speed (rollDeg, rollSlideDeg in a slide, smoothed rollSmoothS); the eye lower in a slide; a tiny shake; the FLOOR STREAK
// quad (SKYGRIND/SkateFloor27: world-space lines along the velocity, two parallax layers); ice dust motes; a carve spray on a heading change; two
// additive blade trails. REDUCED MOTION turns off the FOV, roll, shake and eye drop (the draws stay, they are not camera motion).
// Created by CampaignLighting24.Create (a campaign map: the ice is level data); numbers are design.n27.skate27.*, each with a _why.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class SkateFx27 : MonoBehaviour
    {
        SurvivalGame game;
        Camera cam;
        // numbers (design.n27.skate27)
        double fovMax, fovCap, fovFull, fovIn, fovOut, roll, rollSlide, rollSmooth, eyeDrop, linesPerM, width, scrollK, loShare, hiShare, gain;
        double carveAngle, trailLife, shakeRad, shakeLo, shakeHi, dustRadius;
        int dustCount, carveCount;
        double walk, top;
        // state
        double fov, rollNow, drop, scroll, shakeT, refTime;
        double refX, refZ;
        float carveCool, dustAcc;
        Material floorMat;
        GameObject floorGo;
        ParticleSystem dust;
        TrailRenderer trailL, trailR;
        MaterialPropertyBlock block;
        static readonly int PDir = Shader.PropertyToID("_Dir"), PScroll = Shader.PropertyToID("_Scroll"), PStrength = Shader.PropertyToID("_Strength"),
            PCenter = Shader.PropertyToID("_Center"), PLines = Shader.PropertyToID("_LinesPerM"), PWidth = Shader.PropertyToID("_Width"), PGain = Shader.PropertyToID("_Gain");
        // NIGHT28: the rink's ice material (SKYGRIND/RinkIce28) draws the streaks over the WHOLE floor from these globals
        static readonly int GRinkDir = Shader.PropertyToID("_Rink28Dir"), GRinkScroll = Shader.PropertyToID("_Rink28Scroll"), GRinkStrength = Shader.PropertyToID("_Rink28Strength");

        /// <summary>The last frame's numbers, for the reel line: onIce, strength 0..1, the FOV add (deg), the roll (deg).</summary>
        public bool onIce;
        public double strength, fovAdd, rollDeg;
        public static SkateFx27 Active;

        public static SkateFx27 Create(SurvivalGame g, Transform parent)
        {
            if (N27Blocks.ForceOff || !g.cfg.Design("n27.skate27.enabled") || g.cam == null) return null;
            var go = new GameObject("Skate FX 27");
            go.transform.SetParent(parent, false);
            var s = go.AddComponent<SkateFx27>();
            s.Build(g);
            return s;
        }

        void OnDestroy() { if (Active == this) Active = null; }

        static Material Additive(Color c)
        {
            Shader sh = Shader.Find("SKYGRIND/Particle");
            if (sh == null) return null;
            var m = new Material(sh);
            m.SetFloat("_SrcBlend", 1); m.SetFloat("_DstBlend", 1); m.SetFloat("_ZWrite", 0); m.SetFloat("_Cull", 0);
            m.SetColor("_BaseColor", c);
            m.renderQueue = 3000;
            return m;
        }

        void Build(SurvivalGame g)
        {
            game = g; cam = g.cam; Active = this;
            GameConfig c = g.cfg;
            double N(string k) => c.Num("design.n27.skate27." + k);
            fovMax = N("fovMaxDeg"); fovCap = N("fovCapDeg"); fovFull = N("fovFullMult"); fovIn = N("fovInS"); fovOut = N("fovOutS");
            roll = N("rollDeg"); rollSlide = N("rollSlideDeg"); rollSmooth = N("rollSmoothS"); eyeDrop = N("slideEyeDrop");
            linesPerM = N("streakLinesPerM"); width = N("streakWidth"); scrollK = N("streakScrollK"); loShare = N("streakLoShare"); hiShare = N("streakHiShare"); gain = N("streakGain");
            dustCount = (int)N("dustCount"); dustRadius = N("dustRadius"); carveAngle = N("carveAngleDeg"); carveCount = (int)N("carveCount"); trailLife = N("trailLifeS");
            shakeRad = N("shakeRad"); shakeLo = N("shakeHzLo"); shakeHi = N("shakeHzHi");
            walk = c.movement.ground.walkSpeed; top = walk * fovFull; // "top speed" = the speed the FOV add is full at (1.5 x the run speed, 12 m/s): the streaks are full there too. (The slide-jump cap, 24, is not reachable by skating.)
            Color pale = new Color(0.85f, 0.82f, 1f); // the pale violet-white boundary tone; never red-orange

            Shader fs = Shader.Find("SKYGRIND/SkateFloor27");
            if (fs != null)
            {
                floorMat = new Material(fs);
                floorMat.SetColor("_Color", pale);
                floorGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                floorGo.name = "Skate floor streaks";
                Destroy(floorGo.GetComponent<Collider>());
                floorGo.transform.SetParent(transform, false);
                floorGo.transform.rotation = Quaternion.Euler(90, 0, 0);
                floorGo.transform.localScale = new Vector3(20, 20, 1);
                var r = floorGo.GetComponent<MeshRenderer>();
                r.sharedMaterial = floorMat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                floorGo.SetActive(false);
            }
            Material add = Additive(pale * 0.7f);
            if (add != null)
            {
                var pg = new GameObject("Ice dust");
                pg.transform.SetParent(transform, false);
                dust = pg.AddComponent<ParticleSystem>();
                var main = dust.main;
                main.loop = false; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = Mathf.Max(dustCount, carveCount * 4) + carveCount; main.startLifetime = 1.1f; main.startSize = 0.05f;
                var em = dust.emission; em.enabled = false;
                var sh = dust.shape; sh.enabled = false;
                var rend = pg.GetComponent<ParticleSystemRenderer>();
                rend.sharedMaterial = add; rend.renderMode = ParticleSystemRenderMode.Billboard;
                dust.Play();
                trailL = MakeTrail("Blade trail L", add);
                trailR = MakeTrail("Blade trail R", add);
            }
        }

        TrailRenderer MakeTrail(string name, Material m)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TrailRenderer>();
            t.time = (float)trailLife; t.widthMultiplier = 0.04f; t.minVertexDistance = 0.1f; t.sharedMaterial = m;
            t.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; t.emitting = false;
            return t;
        }

        bool OnIce(Mover m)
        {
            if (!m.grounded) return false;
            Vec3 p = m.position;
            // NIGHT30 item 6a's view (builder V): on a GLIDE floor (S's SurvivalSim.OnIce30: the level's ICE rects and B's Glacier30 arena) the same skate
            // trail, floor streak and dust show speed (R3: "a skate trail and wind cue show speed"); no screen-border lines (GG's never-again)
            if (N30Fixes.On && game.sim.glide30 != null && game.sim.OnIce30(p)) return true;
            // the level's zones, and (NIGHT28) the glacier boss room's own: the rink's ONE ice zone (SurvivalSim.Glacier27Surf)
            Surfaces26 s = game.sim.surfaces26, r = game.sim.Glacier27Surf ?? game.sim.Glacier28Surf ?? game.sim.Stag31Surf; // NIGHT31 B: the Stag's rink ice too
            if (s != null) foreach (SurfaceZone z in s.zones) if (z.kind == SurfaceKind.Ice && z.Contains(p.x, p.y, p.z)) return true;
            if (r != null) foreach (SurfaceZone z in r.zones) if (z.kind == SurfaceKind.Ice && z.Contains(p.x, p.y, p.z)) return true;
            return false;
        }

        void LateUpdate()
        {
            if (game == null || game.sim == null || cam == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.1f);
            bool playing = game.flow == Flow.Playing;
            Mover m = game.sim.mover;
            onIce = playing && m != null && OnIce(m);
            double speed = onIce ? m.Speed : 0;
            strength = onIce ? SkateMath27.Strength(speed, top, loShare, hiShare) : 0;
            bool calm = game.cfg.movement.settings.reducedMotion;
            Vector3 vel = m != null ? new Vector3((float)m.velocity.x, 0, -(float)m.velocity.z) : Vector3.zero; // web -> Unity (z flips)
            Vector3 feet = m != null ? WebSpace.ToUnity(game.Move54Steps != null ? game.Move54DrawnFeet : m.position) : Vector3.zero; // NIGHT54 M move54: the drawn feet (the latest step's with the row off)

            // --- the camera (after the feel) ---
            double fovTarget = onIce && !calm ? SkateMath27.FovAdd(speed, walk, fovFull, fovMax, fovCap) : 0;
            fov = SkateMath27.SmoothUpDown(fov, fovTarget, dt, fovIn, fovOut);
            fovAdd = System.Math.Min(fov, fovCap);
            if (fovAdd > 1e-4) cam.fieldOfView += (float)fovAdd;
            bool sliding = onIce && m.state == MoveState.Slide;
            double lateral = onIce ? Vector3.Dot(vel, cam.transform.right) : 0;
            double rollTarget = onIce && !calm ? SkateMath27.Roll(lateral, top, sliding ? rollSlide : roll) : 0;
            rollNow = SkateMath27.Smooth(rollNow, rollTarget, dt, rollSmooth);
            rollDeg = rollNow;
            drop = SkateMath27.Smooth(drop, sliding && !calm ? eyeDrop : 0, dt, 0.1);
            shakeT += dt * Mathf.Lerp((float)shakeLo, (float)shakeHi, (float)strength);
            double shake = calm ? 0 : shakeRad * strength * Mathf.Rad2Deg;
            if (System.Math.Abs(rollNow) > 1e-4 || shake > 0)
                cam.transform.rotation = cam.transform.rotation * Quaternion.Euler((float)(shake * Mathf.Sin((float)(shakeT * 6.2831853))), (float)(shake * Mathf.Sin((float)(shakeT * 4.1 + 1.3))), (float)-rollNow);
            if (drop > 1e-4) cam.transform.position += Vector3.down * (float)drop;

            // --- the floor streaks ---
            bool rinkOn = game.rinkView28 != null && game.rinkView28.Shown; // the rink's own ice material draws the streaks (no patch round the feet)
            if (strength > 0.001) scroll += speed * scrollK * dt;
            {
                Vector3 rdir = vel.sqrMagnitude > 1e-4f ? vel.normalized : cam.transform.forward;
                Shader.SetGlobalVector(GRinkDir, new Vector4(rdir.x, 0, rdir.z, 0));
                Shader.SetGlobalFloat(GRinkScroll, (float)(scroll % 1000.0));
                Shader.SetGlobalFloat(GRinkStrength, rinkOn ? (float)strength : 0f);
            }
            if (floorGo != null)
            {
                bool show = strength > 0.001 && !rinkOn;
                if (floorGo.activeSelf != show) floorGo.SetActive(show);
                if (show)
                {
                    Vector3 dir = vel.sqrMagnitude > 1e-4f ? vel.normalized : cam.transform.forward;
                    floorGo.transform.position = new Vector3(feet.x, feet.y + 0.025f, feet.z);
                    floorMat.SetVector(PDir, new Vector4(dir.x, 0, dir.z, 0));
                    floorMat.SetFloat(PScroll, (float)(scroll % 1000.0));
                    floorMat.SetFloat(PStrength, (float)strength);
                    floorMat.SetVector(PCenter, new Vector4(feet.x, 0, feet.z, 0));
                    floorMat.SetFloat(PLines, (float)linesPerM);
                    floorMat.SetFloat(PWidth, (float)width);
                    floorMat.SetFloat(PGain, (float)gain);
                }
            }

            // --- dust motes, carve spray, blade trails ---
            if (dust != null)
            {
                Vector3 dir = vel.sqrMagnitude > 1e-4f ? vel.normalized : Vector3.forward;
                // dust: a steady count near the feet drifting opposite to travel (lifetime 1.1 s: rate = count / life at full strength)
                dustAcc += (float)(dustCount / 1.1 * strength) * dt;
                int n = (int)dustAcc;
                dustAcc -= n;
                for (int i = 0; i < n && onIce; i++)
                {
                    Vector2 d2 = Random.insideUnitCircle * (float)dustRadius; // view-side randomness (never the sim's)
                    var ep = new ParticleSystem.EmitParams
                    {
                        position = feet + new Vector3(d2.x, Random.Range(0.05f, 0.6f), d2.y),
                        velocity = -dir * (float)(speed * 0.15) + Vector3.up * 0.05f,
                        startSize = Random.Range(0.03f, 0.07f), startLifetime = 1.1f,
                    };
                    dust.Emit(ep, 1);
                }
                // carve: the heading now against the heading 0.25 s ago
                carveCool -= dt;
                refTime += dt;
                if (!onIce || speed < loShare * top) { refX = vel.x; refZ = vel.z; refTime = 0; }
                else if (refTime >= 0.25)
                {
                    if (carveCool <= 0 && SkateMath27.HeadingChangeDeg(refX, refZ, vel.x, vel.z) > carveAngle)
                    {
                        carveCool = 0.4f;
                        Vector3 side = Vector3.Cross(Vector3.up, dir);
                        float sgn = Mathf.Sign(Vector3.Dot(side, new Vector3((float)refX, 0, (float)refZ) - vel) + 1e-6f);
                        for (int i = 0; i < carveCount; i++)
                        {
                            var ep = new ParticleSystem.EmitParams
                            {
                                position = feet + Vector3.up * 0.05f,
                                velocity = side * sgn * Random.Range(1.5f, 3.5f) + Vector3.up * Random.Range(0.4f, 1.2f) - dir * 0.5f,
                                startSize = Random.Range(0.04f, 0.09f), startLifetime = 0.5f,
                            };
                            dust.Emit(ep, 1);
                        }
                    }
                    refX = vel.x; refZ = vel.z; refTime = 0;
                }
            }
            if (trailL != null)
            {
                bool on = strength > 0.1;
                Vector3 right = cam.transform.right; right.y = 0; right = right.sqrMagnitude > 1e-4f ? right.normalized : Vector3.right;
                trailL.transform.position = feet - right * 0.18f + Vector3.up * 0.04f;
                trailR.transform.position = feet + right * 0.18f + Vector3.up * 0.04f;
                if (trailL.emitting != on) { trailL.emitting = on; trailR.emitting = on; }
            }
        }
    }
}
