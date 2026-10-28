// NIGHT10: the railcannon's and the zoomed KESTREL's visuals, after skygrind/src/combat/weaponFx.js (158 lines), for the
// player (owner "player"), frame time, visual only:
// - the LONGWAVE's laser sight while it charges (:119-133): a thin stretched open cylinder (CylinderGeometry(1, 1, 1, 8,
//   1, true) on its base, :21) from the muzzle to where the camera ray first hits the world (at most weapons.rail.range),
//   radius 0.018 + 0.03 charge, colors.usable x (1 + 3 charge^2), opacity 0.45 + 0.55 charge, and the dot where it lands
//   (SphereGeometry(1, 10, 8) on the laser's material, :26), radius 0.08 + 0.12 charge; shown while loadout.railCharge > 0
//   with the rail in hand;
// - the beam flash on railFire (:110-115, :148-155): muzzle -> e.to, BEAM_TIME 0.28 s, radius 0.09 (1 - t) + 0.02,
//   0xd6fbff x 4 (1 - t), opacity 1 - t;
// - the ricochet bounce line on ricochet (:72-80, :142-147): muzzle -> the bounce point -> the target (the web's path
//   [eye, impact, target]; in the port's ShotEvent `origin` is the bounce point and `end` the target's aim point),
//   colors.tracer x 3, opacity 1 - age / ads.ricochet.lineTime.
// The web's materials are additive (AdditiveBlending, transparent, no depth write): here copies of the neon material (URP
// Unlit) set to Blend One One, ZWrite off, in the transparent queue, with the opacity folded into the colour (three's
// additive is src x alpha + dst). The muzzle is the viewmodel's (main.js getMuzzle = viewmodel.muzzleWorld): the held
// rig's muzzle (BlockyKit's, in rig space) seen through the viewmodel camera and put at UnityTuning.FxMuzzleDepth in the world camera's
// view, as Viewmodel.OnShot starts its tracer. The rigs are found by the names Viewmodel gives them ("Viewmodel camera" /
// "Viewmodel" / "LONGWAVE", "KESTREL"); without them, the camera-space hold point (viewmodel.holdX / holdY / holdZ).
// Not ported: the zoomed Prism's beam preview (:82-108, :135-141; there is no PRISM in the port yet) and other owners
// (RIVAL).
// NIGHT12 U3-25.4 (combat/effects.js :172-189, the styled player's shotResult; the Viewmodel's single tracer moved here):
// - a tracer per ray of a hitscan shot (rayCount > 0: the KESTREL, the LONGWAVE, every THUNDERCLAP pellet), muzzle -> the
//   ray's end, in the gun's family colour (main.js familyHex), fading over fx.tracerTime; the port's shotResult carries the
//   THUNDERCLAP's first pellet only, so pellets 2..weapons.shotgun.pellets are cosmetic: drawn in the gun's cone
//   (weapons.shotgun.coneHalfAngleDeg, CombatMath.SampleCone) from this view's own Mulberry32 and cast against the world and
//   the enemies (weapons.shotgun.range) for their ends; they never touch the sim's dice or damage;
// - the impact flare where a ray ends on the world (fx.impactFlare: one cube of fx.size x scale x sizes[weapon] in the
//   family colour, life fx.impactFlare.life, fx.surfaceOffset off the surface); the cosmetic pellets also burst
//   fx.sparkCount - 1 sparks there (fx.sparkSpeed, sparkLife, x fx.sparkSize; the first pellet's sparks are
//   SurvivalGame's);
// - the THUNDERCLAP's shells (port-only: the web ejects none; Research/2026-10-03-polish.md A4): one per shot, thrown up
//   and right from the receiver when the pump works (weapons.shotgun.pumpDelay after the shot, the shotgun still in hand),
//   tumbling, bouncing on the floor under where it was thrown, kept UnityTuning.ShellKeep s (UnityTuning.ShellPool at most).
// The tracers, flares and shells animate on SurvivalGame.WorldDt (frozen in a hitstop); the laser, beam and bounce line keep
// frame time as before.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class WeaponFx
    {
        const double BeamTime = 0.28; // weaponFx.js:40 BEAM_TIME

        readonly SurvivalGame game;
        readonly Transform laser, dot, beam;
        readonly Renderer laserR, dotR, beamR;
        readonly LineRenderer bounce;
        readonly Material laserMat, beamMat, bounceMat;
        readonly Color usable, tracer, beamColor;
        readonly double railRange, lineTime;
        readonly Vector3 railMuzzle, kestrelMuzzle, shotgunMuzzle, hold, shellPort;
        Camera vmCam;
        Transform railRig, kestrelRig, shotgunRig;
        Vector3 beamFrom, beamTo;
        double beamAge = double.PositiveInfinity, bounceAge = double.PositiveInfinity;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        // NIGHT12: the tracers, the flares and sparks, the shells
        readonly FxLines lines;
        readonly FxCubes cubes;
        Mulberry32 rng;
        readonly float tracerTime, flareLife, flareScale, sparkSpeed, sparkLife, sparkSize;
        readonly int sparkCount, pellets;
        readonly double surfaceOffset, sgHalf, sgRange, pumpDelay;
        readonly Color revolverColor, shotgunColor, railColor;
        readonly float revolverFlare, shotgunFlare, railFlare;
        double shellIn;
        struct Shell
        {
            public Vector3 pos, vel, axis;
            public Quaternion rot;
            public float age, spin, floorY;
            public bool live, resting;
        }
        readonly Shell[] shells = new Shell[UnityTuning.ShellPool];
        int nextShell;
        readonly Matrix4x4[] shellMatrices = new Matrix4x4[UnityTuning.ShellPool];
        readonly Mesh shellMesh;
        readonly RenderParams shellParams;
        CubeBatch shellBatch; // NIGHT17 round 1: the shells as one dynamic mesh (made on the first shell)
        readonly float gravity;

        public WeaponFx(SurvivalGame game)
        {
            this.game = game;
            Current = this; // NIGHT33 W (view33)
            GameConfig cfg = game.cfg;
            fx33 = N33Fixes.ViewRowOn(cfg, "view33"); // NIGHT33 W round 3: the player's flash / spark colours and the hit sparks
            m36 = N36Fixes.ViewRowOn(cfg, "muzzle36"); // NIGHT36 F: tracers from the post-kick muzzle, the slug one line
            usable = Look.C(cfg, "colors.usable");
            tracer = Look.C(cfg, "colors.tracer");
            beamColor = Look.Hex(0xd6fbff); // weaponFx.js:153
            railRange = cfg.Num("weapons.rail.range");
            lineTime = cfg.Num("ads.ricochet.lineTime");
            // The rigs' muzzles in rig space (what Viewmodel keeps per rig; the throwaway kits are load-time only).
            railMuzzle = BlockyKit.Longwave(new MeshKit(), Color.white);
            kestrelMuzzle = BlockyKit.Kestrel(new MeshKit(), new MeshKit(), Color.white);
            shotgunMuzzle = BlockyKit.Thunderclap(new MeshKit(), Color.white, new MeshKit());
            hold = new Vector3((float)cfg.Num("viewmodel.holdX"), (float)cfg.Num("viewmodel.holdY"), (float)-cfg.Num("viewmodel.holdZ"));
            shellPort = new Vector3(UnityTuning.ShellPortX, UnityTuning.ShellPortY, -UnityTuning.ShellPortZ); // web rig space -> Unity

            var root = new GameObject("Weapon FX").transform;
            Mesh cylinder = Cylinder(8);
            Mesh sphere = Sphere(10, 8);
            laserMat = Additive(game.mats.neon, "Rail laser");
            beamMat = Additive(game.mats.neon, "Rail beam");
            bounceMat = Additive(game.mats.neon, "Ricochet line");
            laser = Part(root, "Rail laser", cylinder, laserMat, out laserR);
            dot = Part(root, "Rail laser dot", sphere, laserMat, out dotR);
            beam = Part(root, "Rail beam", cylinder, beamMat, out beamR);

            bounce = new GameObject("Ricochet line").AddComponent<LineRenderer>();
            bounce.transform.SetParent(root, false);
            bounce.useWorldSpace = true;
            bounce.positionCount = 3;
            bounce.widthMultiplier = UnityTuning.FxBounceWidth;
            bounce.sharedMaterial = bounceMat;
            bounce.shadowCastingMode = ShadowCastingMode.Off;
            bounce.receiveShadows = false;
            bounce.enabled = false;

            // NIGHT12: the shot effects' numbers, read once
            lines = new FxLines(game.mats.neon, cfg.Int("fx.lineCapacity"));
            cubes = new FxCubes(cfg, game.mats.neon, UnityTuning.FxCubePool);
            rng = new Mulberry32(game.runSeed ^ UnityTuning.WeaponFxSeedSalt);
            tracerTime = (float)cfg.Num("fx.tracerTime");
            flareLife = (float)cfg.Num("fx.impactFlare.life");
            flareScale = (float)cfg.Num("fx.impactFlare.scale");
            sparkCount = cfg.Int("fx.sparkCount");
            sparkSpeed = (float)cfg.Num("fx.sparkSpeed");
            sparkLife = (float)cfg.Num("fx.sparkLife");
            sparkSize = (float)cfg.Num("fx.sparkSize");
            surfaceOffset = cfg.Num("fx.surfaceOffset");
            pellets = cfg.Int("weapons.shotgun.pellets");
            sgHalf = cfg.Num("weapons.shotgun.coneHalfAngleDeg") * System.Math.PI / 180;
            sgRange = cfg.Num("weapons.shotgun.range");
            pumpDelay = cfg.Num("weapons.shotgun.pumpDelay");
            revolverColor = Family(cfg, "revolver");
            shotgunColor = Family(cfg, "shotgun");
            railColor = Family(cfg, "rail");
            if (fx33)
            {
                revolverColor = PlayerFx33(revolverColor);
                shotgunColor = PlayerFx33(shotgunColor);
                railColor = PlayerFx33(railColor);
            }
            revolverFlare = FlareSize(cfg, "revolver");
            shotgunFlare = FlareSize(cfg, "shotgun");
            railFlare = FlareSize(cfg, "rail");
            gravity = (float)cfg.Num("fx.gravity");
            var shellKit = new MeshKit();
            shellKit.Cube(Vector3.zero, new Vector3(UnityTuning.ShellSizeXY, UnityTuning.ShellSizeXY, UnityTuning.ShellSizeZ), Color.white);
            shellMesh = shellKit.Build("shell");
            shellParams = FxDraw.Params(FxDraw.Opaque(game.mats.neon, "Shells", Look.Hex(UnityTuning.ShellHex)));

            game.events.On<RailFireEvent>("railFire", e =>
            {
                if (e.owner != "player") return;
                Vec3 to = e.to; // the payload is reused: keep a copy
                beamFrom = Muzzle("rail");
                beamTo = WebSpace.ToUnity(to);
                beamAge = 0;
            });
            // NIGHT33 W (design.n33.pierce33, D-4): the charged KESTREL's piercing shot draws the LONGWAVE's beam (BEAM_TIME, its radius and
            // colour) from the KESTREL's muzzle to the wall, under its own event name (kestrelPierce33; railFire stays the LONGWAVE's)
            game.events.On<RailFireEvent>(Loadout.KestrelPierce33, e =>
            {
                if (e.owner != "player") return;
                Vec3 to = e.to;
                beamFrom = Muzzle("revolver");
                beamTo = WebSpace.ToUnity(to);
                beamAge = 0;
                beamPending36 = m36; // NIGHT36 muzzle36: the beam's start moves to the post-kick muzzle at this frame's render
            });
            if (m36)
            {
                // NIGHT36 muzzle36: the charged THUNDERCLAP's slug emits its shotResult, then chargeShot27Fire "shotgun" in the same call (FireCharged27)
                game.events.On<ChargeShot27Event>("chargeShot27Fire", e =>
                {
                    if (e == null || e.gun != "shotgun" || pendingN36 == 0) return;
                    ChargeShots27 cs = game.loadout != null ? game.loadout.charge27 : null;
                    if (cs == null || !cs.p.slug) return; // a charged pellet volley (slug off) keeps its pellets
                    if (pending36[pendingN36 - 1].weapon == "shotgun") pending36[pendingN36 - 1].slug = true;
                });
            }
            game.events.On<ShotEvent>("ricochet", e =>
            {
                if (e.owner != "player") return;
                Vec3 impact = e.origin, target = e.end;
                bounce.SetPosition(0, Muzzle("revolver"));
                bounce.SetPosition(1, WebSpace.ToUnity(impact));
                bounce.SetPosition(2, WebSpace.ToUnity(target));
                bounceAge = 0;
            });
            game.events.On<ShotEvent>("shotResult", OnShot);
            if (fx33) game.events.On<HitEvent>("hit", OnHitSpark33); // NIGHT33 W round 3: a spark on every gun's hit
            game.events.On("fire", o =>
            {
                if ((string)o == "shotgun") shellIn = pumpDelay; // weapons.js :601 the pump's delay
            });
            game.events.On("fire", OnFireStar); // NIGHT17 U3-30.5: the muzzle star
            // NIGHT17 round 2 (the first kill's hitch): what the first shots make, made at load: a star material (its texture
            // baked) for every fx.flashShapes entry, the star quad, the shells' batch (both faces, as the other batches),
            // the guns' flare / spark / tracer colours
            foreach (System.Collections.Generic.KeyValuePair<string, object> s in cfg.Obj("fx.flashShapes")) StarMaterial(s.Key, out _, out _);
            StarSetup();
            shellParams.material.SetFloat("_Cull", 0);
            shellBatch = new CubeBatch(UnityTuning.ShellPool, "Shells", shellMesh);
            Color sparkWarm = Look.C(cfg, "colors.spark");
            if (fx33)
            {
                sparkWarm = PlayerFx33(sparkWarm);
                cubes.Warm(Look.Hex(UnityTuning.WhiteHotHex33));
            }
            cubes.Warm(revolverColor);
            cubes.Warm(shotgunColor);
            cubes.Warm(railColor);
            cubes.Warm(sparkWarm);
            lines.Warm(revolverColor);
            lines.Warm(shotgunColor);
            lines.Warm(railColor);
            lines.Warm(tracer);
            game.events.On("shotgunPump", _ => // weapons.js :211 (if the loadout ever emits it, the shell goes now)
            {
                shellIn = 0;
                EjectShell();
            });
            game.OnNewRun += NewRun;
        }

        static Color Family(GameConfig cfg, string weapon) => WeaponLook.FamilyColor(cfg, weapon, Look.C(cfg, "colors.tracer")); // NIGHT13: the one lookup

        static float FlareSize(GameConfig cfg, string weapon) =>
            cfg.Has($"fx.impactFlare.sizes.{weapon}") ? (float)cfg.Num($"fx.impactFlare.sizes.{weapon}") : 1;

        /// <summary>A new run (effects.js clear): no tracers, flares or shells left; the cosmetic dice from the run's seed.</summary>
        void NewRun()
        {
            rng = new Mulberry32(game.runSeed ^ UnityTuning.WeaponFxSeedSalt);
            lines.Clear();
            cubes.Clear();
            for (int i = 0; i < shells.Length; i++) shells[i].live = false;
            shellIn = 0;
            pendingN36 = 0;
            beamPending36 = false;
        }

        /// <summary>effects.js :172-189 (styled): a tracer per ray, the flare (and the cosmetic pellets' sparks) on the world.</summary>
        // NIGHT36 F muzzle36 (view only; REVIEW.md root cause 2): a shot's tracers are drawn at the frame's render, after the viewmodel's kick moved the
        // muzzle (SurvivalGame.Render.cs: viewmodel.Render runs before weaponFx.Render, the sim's shotResult before both), from the same point as the
        // muzzle star (Viewmodel.MuzzleWorld + fx.flashForward); the charged THUNDERCLAP's slug (chargeShot27Fire "shotgun" right after its shotResult)
        // draws its ONE line, not the cosmetic pellets; the charged KESTREL's beam starts there too. Off: as before (the muzzle at the sim step).
        struct Pending36
        {
            public string weapon, surface;
            public Vec3 origin, dir, end, normal;
            public bool slug;
        }
        readonly bool m36;
        readonly Pending36[] pending36 = new Pending36[UnityTuning.Muzzle36Pending];
        int pendingN36;
        bool beamPending36;

        /// <summary>NIGHT36 (the reels): the last flushed tracer's start (Unity space) and the muzzle star's point that frame; the slug lines drawn.</summary>
        public static Vector3 Tracer36From, Star36At;
        public static int Tracers36, Slugs36;

        void OnShot(ShotEvent e)
        {
            if (e.owner != "player" || e.rayCount == 0) return;
            if (m36)
            {
                if (e.weapon != "revolver" && e.weapon != "shotgun" && e.weapon != "rail") return;
                if (pendingN36 >= pending36.Length) FlushPending36(); // more shots in one frame than the buffer: drawn now
                pending36[pendingN36++] = new Pending36 { weapon = e.weapon, surface = e.surface, origin = e.origin, dir = e.dir, end = e.end, normal = e.normal };
                return;
            }
            DrawShot(e.weapon, e.origin, e.dir, e.end, e.normal, e.surface == "world", Muzzle(e.weapon), false);
        }

        /// <summary>NIGHT36 muzzle36: the frame's pending shots drawn from the post-kick muzzle (the star's point), after Viewmodel.Render.</summary>
        void FlushPending36()
        {
            if (pendingN36 == 0) return;
            Camera main = game.cam;
            for (int i = 0; i < pendingN36; i++)
            {
                Pending36 p = pending36[i];
                Vector3 from = game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 mw) && game.loadout.Weapon == p.weapon ? mw : Muzzle(p.weapon);
                if (main != null) from += main.transform.forward * starForward; // the star's own point (RenderStar)
                DrawShot(p.weapon, p.origin, p.dir, p.end, p.normal, p.surface == "world", from, p.slug);
                Tracer36From = from;
                Tracers36++;
                if (p.slug) Slugs36++;
            }
            pendingN36 = 0;
        }

        /// <summary>The shot's lines and flares (effects.js :172-189 styled): the tracer from `muzzle`, the flare on the world, the THUNDERCLAP's cosmetic
        /// pellets (none for the slug: NIGHT36 muzzle36).</summary>
        void DrawShot(string weapon, Vec3 origin, Vec3 dir, Vec3 end, Vec3 normal, bool world, Vector3 muzzle, bool slug)
        {
            Color c;
            float flare;
            switch (weapon)
            {
                case "revolver": c = revolverColor; flare = revolverFlare; break;
                case "shotgun": c = shotgunColor; flare = shotgunFlare; break;
                case "rail": c = railColor; flare = railFlare; break;
                default: return;
            }
            bool fx47 = Fx47View.RowOn(game.cfg); // NIGHT47 F fx47 (G24): a tracer with a hot core and a glow, then a fading line (the KESTREL's was 1.4 cm for 60 ms)
            float tw = slug ? UnityTuning.TracerWidth * UnityTuning.Slug36Width : fx47 ? (weapon == "revolver" ? UnityTuning.Fx47TracerCoreW : UnityTuning.Fx47PelletW) : UnityTuning.TracerWidth;
            lines.Add(muzzle, WebSpace.ToUnity(end), c, tracerTime, tw);
            if (fx47 && weapon == "revolver" && !slug)
            {
                lines.Add(muzzle, WebSpace.ToUnity(end), c * UnityTuning.Fx47TracerGlowK, tracerTime * 2f, UnityTuning.Fx47TracerGlowW);
                lines.Add(muzzle, WebSpace.ToUnity(end), c * UnityTuning.Fx47TracerSmokeK, UnityTuning.Fx47TracerSmokeS, UnityTuning.Fx47TracerSmokeW);
            }
            if (world && SurfaceAt(origin, end, ref normal)) Flare(end, normal, c, flare, false);
            if (weapon != "shotgun" || slug) return;
            // The THUNDERCLAP's other pellets: cosmetic rays in its cone (this view's dice), cast for their ends.
            for (int i = 1; i < pellets; i++)
            {
                Vec3 d = CombatMath.SampleCone(dir, sgHalf, rng.Next(), rng.Next());
                bool wallHit = game.world.RayIntersect(origin, d, out RayHit wall) && wall.distance <= sgRange;
                EnemyRayHit? hit = game.enemies.Raycast(origin, d, sgRange);
                Vec3 to;
                bool onWorld = false;
                if (hit.HasValue && (!wallHit || hit.Value.distance < wall.distance)) to = origin + d * hit.Value.distance;
                else if (wallHit)
                {
                    to = origin + d * wall.distance;
                    onWorld = true;
                }
                else to = origin + d * sgRange;
                lines.Add(muzzle, WebSpace.ToUnity(to), c, tracerTime, fx47 ? UnityTuning.Fx47PelletW : UnityTuning.TracerWidth); // NIGHT47 F fx47: pellet tracers 2.2 cm
                if (Shot74View.RowLive) Shot74View.Pellet(WebSpace.ToUnity(to), onWorld ? WebSpace.ToUnity(Vec3.Dot(wall.normal, d) > 0 ? -wall.normal : wall.normal) : Vector3.zero, onWorld); // PG3 V1 shot74: the pellet's end for its comet and impact (a read of the end above; nothing drawn here)
                if (onWorld) Flare(to, Vec3.Dot(wall.normal, d) > 0 ? -wall.normal : wall.normal, c, flare, true);
            }
        }

        /// <summary>The surface under a recorded end: the event's normal, else re-cast from the shot's origin (Decals' way).</summary>
        bool SurfaceAt(Vec3 origin, Vec3 end, ref Vec3 normal)
        {
            if (normal.x != 0 || normal.y != 0 || normal.z != 0) return true;
            Vec3 d = end - origin;
            double len = d.Length;
            if (!(len > 0) || !game.world.RayIntersect(origin, d / len, out RayHit hit) || System.Math.Abs(hit.distance - len) > UnityTuning.DecalOnSurface) return false;
            normal = Vec3.Dot(hit.normal, d) > 0 ? -hit.normal : hit.normal;
            return true;
        }

        /// <summary>effects.js :183-187: one flare cube (and, for a cosmetic pellet, sparkCount - 1 sparks) off the surface.</summary>
        void Flare(Vec3 end, Vec3 normal, Color c, float size, bool sparks)
        {
            Vector3 at = WebSpace.ToUnity(end + normal * surfaceOffset);
            if (sparks) cubes.Burst(at, c, sparkCount - 1, sparkSpeed, sparkLife, sparkSize, rng);
            cubes.Burst(at, c, 1, 0, flareLife, flareScale * size, rng);
        }

        /// <summary>A shell from the THUNDERCLAP's receiver: thrown up and right (camera space) on top of the player's velocity.</summary>
        void EjectShell()
        {
            if (game.loadout.Weapon != "shotgun" || !game.Active) return;
            // NIGHT16 polish #43: the shells are a port-only motion flourish (thrown across the view, tumbling): none with
            // REDUCED MOTION or at EFFECTS INTENSITY 0, read as CameraFeel does
            MovementConfig mc = game.cfg.movement;
            if (!FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity)) return;
            Transform cam = game.cam.transform;
            Vector3 at = ViewPoint(ShotgunRig(), shellPort);
            ref Shell s = ref shells[nextShell];
            nextShell = (nextShell + 1) % shells.Length; // the oldest goes
            s.live = true;
            s.resting = false;
            s.age = 0;
            s.pos = at;
            float speed = Mathf.Lerp(UnityTuning.ShellSpeedMin, UnityTuning.ShellSpeedMax, (float)rng.Next());
            s.vel = cam.right * speed + cam.up * UnityTuning.ShellUp - cam.forward * UnityTuning.ShellBack + WebSpace.ToUnity(game.mover.velocity);
            float u = (float)(rng.Next() * 2 - 1), phi = (float)(rng.Next() * System.Math.PI * 2), r = Mathf.Sqrt(1 - u * u);
            s.axis = new Vector3(r * Mathf.Cos(phi), u, r * Mathf.Sin(phi));
            s.spin = UnityTuning.ShellSpin * (0.5f + (float)rng.Next());
            s.rot = cam.rotation;
            // the floor under the throw (one ray now; the shell lands there)
            Vec3 from = WebSpace.FromUnity(at);
            s.floorY = game.world.RayIntersect(from, new Vec3(0, -1, 0), out RayHit floor) ? (float)(from.y - floor.distance) : float.NegativeInfinity;
        }

        /// <summary>The shells fly, tumble, bounce and lie still; one instanced draw.</summary>
        void RenderShells(float dt, bool visible)
        {
            int n = 0;
            float half = UnityTuning.ShellSizeXY * 0.5f;
            for (int i = 0; i < shells.Length; i++)
            {
                ref Shell s = ref shells[i];
                if (!s.live) continue;
                s.age += dt;
                if (s.age >= UnityTuning.ShellKeep || s.pos.y < s.floorY - 1)
                {
                    s.live = false;
                    continue;
                }
                if (!s.resting)
                {
                    s.vel.y -= gravity * dt;
                    s.pos += s.vel * dt;
                    s.rot = Quaternion.AngleAxis(s.spin * dt, s.axis) * s.rot;
                    if (s.pos.y - half < s.floorY && s.vel.y < 0)
                    {
                        s.pos.y = s.floorY + half;
                        s.vel = new Vector3(s.vel.x * UnityTuning.ShellFriction, -s.vel.y * UnityTuning.ShellBounce, s.vel.z * UnityTuning.ShellFriction);
                        s.spin *= UnityTuning.ShellFriction;
                        if (s.vel.magnitude < UnityTuning.ShellRestSpeed)
                        {
                            s.resting = true;
                            Vector3 f = s.rot * Vector3.forward; // it lies on its side
                            f.y = 0;
                            s.rot = f.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(f) : Quaternion.identity;
                        }
                    }
                }
                float k = Mathf.Clamp01((UnityTuning.ShellKeep - s.age) / UnityTuning.ShellFadeTime);
                shellMatrices[n++] = Matrix4x4.TRS(s.pos, s.rot, new Vector3(k, k, k));
            }
            // NIGHT17 round 1 (builder X): one RenderMesh of the shells' batch (build 2's Player drew no RenderMeshInstanced)
            if (visible && n > 0)
            {
                shellBatch ??= new CubeBatch(UnityTuning.ShellPool, "Shells", shellMesh);
                shellBatch.Begin();
                for (int i = 0; i < n; i++) shellBatch.Add(shellMatrices[i]);
                shellBatch.Draw(shellParams);
            }
        }

        /// <summary>update(frameDt, visible): frame time; `visible` false hides everything (the title, the menus).</summary>
        // --- NIGHT17 U3-30.5 (builder X): the muzzle star ------------------------------------------------------------------
        // combat/effects.js :27-50 / :165-170 / :243-253 (styled, the player's): on `fire`, a camera-facing star at the muzzle in
        // the gun's flash shape (fx.flashShapes[weapon]: spikes, sharpness, ring, scale; the web's fragment formula baked into a
        // texture per shape at the first shot) and its family's colour (weapons.families, else colors.muzzleFlash), rolled at
        // random, sized fx.flashSize x (0.75 + 0.25 k) x the shape's scale x UnityTuning.StarScale, for fx.flashFrames render
        // frames (k 1, then fx.flashSecondFrame), x FLASH INTENSITY (0: none). Additive, the world camera, at the viewmodel's
        // muzzle lined up on screen (Viewmodel.MuzzleWorld) plus fx.flashForward. Not counted by the flash gate (a muzzle-sized
        // flash on every shot would spend the gate's three a second and refuse the kills' and the parry's): decided tonight.
        readonly System.Collections.Generic.Dictionary<string, Material> starMats = new System.Collections.Generic.Dictionary<string, Material>();
        readonly System.Collections.Generic.Dictionary<string, Vector4> starShapes = new System.Collections.Generic.Dictionary<string, Vector4>();
        readonly System.Collections.Generic.Dictionary<string, Color> starColors = new System.Collections.Generic.Dictionary<string, Color>();
        readonly MaterialPropertyBlock starBlock = new MaterialPropertyBlock();
        Mesh starQuad;
        string starWeapon;
        int starFrame0 = int.MinValue / 2;
        double starTime0 = double.MinValue / 2; // NIGHT51 star51: the shot's real time
        float starRoll;
        uint starHash = 0x6a09e667u;
        int starFrames = -1;
        double starSecond;
        float starSize, starForward;

        /// <summary>Muzzle stars drawn since boot (a Play check reads it).</summary>
        public int Stars { get; private set; }

        void OnFireStar(object o)
        {
            if (!(o is string w) || w == "gauntlet" || w == "punch") return;
            starWeapon = w;
            starFrame0 = FeelClock.FrameCount; // the shot's step runs in this frame's Update: its render is frame 0 (NIGHT63 D1: held frames skipped)
            starTime0 = FeelClock.UnscaledNow; // NIGHT51 star51
            starHash ^= starHash << 13;
            starHash ^= starHash >> 17;
            starHash ^= starHash << 5;
            starRoll = (starHash & 0xffff) / 65536f * 360f;
        }

        Material StarMaterial(string w, out Vector4 shape, out Color color)
        {
            if (starMats.TryGetValue(w, out Material m))
            {
                shape = starShapes[w];
                color = starColors[w];
                return m;
            }
            GameConfig cfg = game.cfg;
            double[] s = cfg.Has($"fx.flashShapes.{w}") ? cfg.Nums($"fx.flashShapes.{w}") : cfg.Nums("fx.flashShapes.default");
            shape = new Vector4((float)s[0], (float)s[1], (float)s[2], (float)s[3]);
            color = WeaponLook.FamilyColor(cfg, w, Look.C(cfg, "colors.muzzleFlash"));
            if (fx33) color = PlayerFx33(color); // NIGHT33: never red-orange on the player's flash
            int n = UnityTuning.StarTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = "Muzzle star " + w, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                // effects.js FLASH_FRAGMENT: p in [-1, 1]; spikes, glow, core, ring (the value, up to ~2.4, stored / 2.5)
                double pxv = (x + 0.5) / n * 2 - 1, pyv = (y + 0.5) / n * 2 - 1;
                double r = System.Math.Sqrt(pxv * pxv + pyv * pyv);
                double spikes = shape.x > 0.5 ? 0.45 + 0.55 * System.Math.Pow(System.Math.Abs(System.Math.Cos(System.Math.Atan2(pyv, pxv) * shape.x * 0.5)), shape.y) : 0.7;
                double glow = System.Math.Max(0, System.Math.Min(1, 1 - r / spikes));
                double core = System.Math.Max(0, System.Math.Min(1, 1 - r * 2.5));
                double ring = shape.z * System.Math.Max(0, System.Math.Min(1, 1 - System.Math.Abs(r - 0.72) * 9));
                byte v = (byte)System.Math.Round(System.Math.Min(1, (glow * glow + core + ring) / 2.5) * 255);
                px[y * n + x] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            m = Additive(game.mats.neon, "Muzzle star " + w);
            m.SetTexture("_BaseMap", tex);
            starMats[w] = m;
            starShapes[w] = shape;
            starColors[w] = color;
            return m;
        }

        /// <summary>The star's numbers and quad (NIGHT17 round 2: made at load).</summary>
        void StarSetup()
        {
            if (starFrames >= 0) return;
            starFrames = game.cfg.Int("fx.flashFrames");
            starSecond = game.cfg.Num("fx.flashSecondFrame");
            starSize = (float)game.cfg.Num("fx.flashSize");
            starForward = (float)game.cfg.Num("fx.flashForward");
            var probe = GameObject.CreatePrimitive(PrimitiveType.Quad);
            starQuad = probe.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(probe);
        }

        bool warmed; // NIGHT17 round 2: one tiny star and line on the first frame with a camera

        void WarmDraws()
        {
            Camera main = game.cam;
            if (warmed || main == null) return;
            warmed = true;
            StarSetup();
            Material m = StarMaterial("default", out _, out _);
            starBlock.SetColor(BaseColor, Color.black);
            var rp = new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = starBlock, worldBounds = CrunchWarm.Bounds(main) };
            Graphics.RenderMesh(rp, starQuad, 0, CrunchWarm.Matrix(main));
            lines.WarmDraw(CrunchWarm.Point(main), tracer);
        }

        /// <summary>NIGHT51 star51: the star's frame by real time since the shot (0 full, 1 its second share, past the end: none).</summary>
        static int StarFrame51(double seconds) => seconds < UnityTuning.N51StarFullS ? 0 : seconds < UnityTuning.N51StarS ? 1 : int.MaxValue / 2;

        void RenderStar(bool visible)
        {
            WarmDraws();
            if (starWeapon == null) return;
            StarSetup();
            int frame = FeelClock.FrameCount - starFrame0;
            // NIGHT51 star51: fx.flashFrames counts RENDER frames (14 ms at 144 Hz: GG plays at 104-172 FPS); on, the star lasts real time
            // (full N51StarFullS, its second share to N51StarS) and never less than the shot's frame
            int vf = !Night51.On || frame <= 0 ? frame : StarFrame51(FeelClock.UnscaledNow - starTime0);
            float strength = CrunchMath.StarStrength(vf, starFrames, starSecond, game.settings.View.flashIntensity);
            if (!visible || strength <= 0 || game.flow != Flow.Playing) return;
            Material m = StarMaterial(starWeapon, out Vector4 shape, out Color color);
            Camera main = game.cam;
            Transform ct = main.transform;
            Vector3 at = game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 mw) ? mw : Muzzle(starWeapon);
            at += ct.forward * starForward;
            if (frame == 0) Star36At = at; // NIGHT36 (the reels): the star's point on the shot's frame
            float k = vf == 0 ? 1 : (float)starSecond;
            float size = starSize * (0.75f + 0.25f * k) * shape.w * UnityTuning.StarScale * (fx33 ? UnityTuning.StarSize33 : 1f);
            starBlock.SetColor(BaseColor, color * (UnityTuning.StarHdr * 2.5f * strength * (fx33 ? UnityTuning.StarBright33 : 1f)));
            var rp = new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false, matProps = starBlock, worldBounds = new Bounds(at, Vector3.one * (size + 1)) };
            Graphics.RenderMesh(rp, starQuad, 0, Matrix4x4.TRS(at, ct.rotation * Quaternion.Euler(0, 0, starRoll), Vector3.one * size));
            if (frame == 0) Stars++;
        }

        public void Render(double frameDt, bool visible)
        {
            RenderStar(visible); // NIGHT17 U3-30.5
            if (m36)
            {
                FlushPending36(); // NIGHT36 muzzle36: after the viewmodel's kick (this frame's Viewmodel.Render ran first)
                if (beamPending36)
                {
                    beamPending36 = false;
                    if (game.viewmodel != null && game.viewmodel.MuzzleWorld(out Vector3 bm) && game.loadout.Weapon == "revolver") beamFrom = bm + game.cam.transform.forward * starForward;
                }
            }
            Loadout lo = game.loadout;
            double charge = lo.RailCharge;
            bool laserOn = visible && charge > 0 && lo.Weapon == "rail";
            laserR.enabled = laserOn;
            dotR.enabled = laserOn;
            if (laserOn)
            {
                // :123-131 the camera ray to the first wall (capped at the rail's range), the laser from the muzzle to it
                var (eye, dir) = game.Aim();
                double length = game.world.RayIntersect(eye, dir, out RayHit hit) ? System.Math.Min(hit.distance, railRange) : railRange;
                Vector3 to = WebSpace.ToUnity(eye + dir * length);
                Stretch(laser, Muzzle("rail"), to, (float)(0.018 + 0.03 * charge));
                float opacity = (float)(0.45 + 0.55 * charge);
                laserMat.SetColor(BaseColor, usable * (float)(1 + 3 * charge * charge) * opacity * BossGrade54.Core()); // NIGHT54 G grade54: a brighter core in a graded fight (1 otherwise)
                dot.position = to;
                dot.localScale = Vector3.one * (float)(0.08 + 0.12 * charge); // the unit sphere's radius
            }

            // :142-147 the ricochet bounce line
            bounceAge += frameDt;
            bool bounceOn = visible && bounceAge < lineTime;
            bounce.enabled = bounceOn;
            if (bounceOn) bounceMat.SetColor(BaseColor, tracer * (float)(3 * (1 - bounceAge / lineTime)) * BossGrade54.Core()); // NIGHT54 G grade54: a brighter core in a graded fight (1 otherwise)

            // :148-155 the beam flash
            beamAge += frameDt;
            bool beamOn = visible && beamAge < BeamTime;
            beamR.enabled = beamOn;
            if (beamOn)
            {
                double t = beamAge / BeamTime;
                Stretch(beam, beamFrom, beamTo, (float)(0.09 * (1 - t) + 0.02));
                beamMat.SetColor(BaseColor, beamColor * (float)(4 * (1 - t) * (1 - t))); // x 4 (1 - t), opacity 1 - t
            }

            // NIGHT12: the tracers, flares and shells on the world's time
            float worldDt = (float)game.WorldDt;
            if (shellIn > 0)
            {
                shellIn -= worldDt;
                if (shellIn <= 0) EjectShell(); // weapons.js :210: only with the shotgun still in hand
            }
            if (visible)
            {
                lines.Render(worldDt, game.cam);
                cubes.Render(worldDt);
            }
            RenderShells(worldDt, visible);
        }

        Transform ShotgunRig()
        {
            Muzzle("shotgun"); // finds the rigs once
            return shotgunRig;
        }

        /// <summary>main.js getMuzzle (viewmodel.muzzleWorld): the weapon's muzzle in the world, Unity space.</summary>
        Vector3 Muzzle(string weapon)
        {
            Camera main = game.cam;
            if (vmCam == null)
            {
                Transform vm = main.transform.Find("Viewmodel camera");
                if (vm != null)
                {
                    vmCam = vm.GetComponent<Camera>();
                    railRig = vm.Find("Viewmodel/LONGWAVE");
                    kestrelRig = vm.Find("Viewmodel/KESTREL");
                    shotgunRig = vm.Find("Viewmodel/THUNDERCLAP");
                }
            }
            Transform rig = weapon == "rail" ? railRig : weapon == "shotgun" ? shotgunRig : kestrelRig;
            return ViewPoint(rig, weapon == "rail" ? railMuzzle : weapon == "shotgun" ? shotgunMuzzle : kestrelMuzzle);
        }

        /// <summary>NIGHT33 W round 3 (view33): the player's flash / spark colour pass and the hit sparks are on.</summary>
        readonly bool fx33;

        /// <summary>NIGHT33 (view33, CLAUDE.md rule 8): a player effect's colour with red-orange-amber hues made white-hot; cyan and cool colours kept.</summary>
        public static Color PlayerFx33(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out _);
            float deg = h * 360f;
            bool warm = (deg <= UnityTuning.WarmHueMax33 || deg >= UnityTuning.WarmHueWrap33) && s > UnityTuning.WarmSat33;
            return warm ? Look.Hex(UnityTuning.WhiteHotHex33) : c;
        }

        /// <summary>NIGHT33 (view33): every player gun's hit on an enemy (not melee: SurvivalGame sparks those) bursts white-hot, then the gun's cool colour.</summary>
        void OnHitSpark33(HitEvent e)
        {
            if (e == null || e.owner != "player" || e.melee || e.weapon == SurvivalSim.Whip33Weapon) return;
            if (Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the hit's picture is the kit's one burst
            Vector3 at = WebSpace.ToUnity(e.position);
            int half = UnityTuning.HitSparkCount33 / 2 + 1;
            cubes.Burst(at, Look.Hex(UnityTuning.WhiteHotHex33), half, UnityTuning.HitSparkSpeed33, UnityTuning.HitSparkLife33, UnityTuning.HitSparkScale33, rng, false);
            Color fam = PlayerFx33(WeaponLook.FamilyColor(game.cfg, e.weapon, Look.Hex(UnityTuning.WhiteHotHex33)));
            cubes.Burst(at, fam, UnityTuning.HitSparkCount33 - half, UnityTuning.HitSparkSpeed33, UnityTuning.HitSparkLife33, UnityTuning.HitSparkScale33, rng, false);
            HitSparks33++;
        }

        /// <summary>NIGHT33 (the feel33 reel): hit sparks drawn since boot.</summary>
        public int HitSparks33 { get; private set; }

        /// <summary>NIGHT33 W (view33): the live WeaponFx (the rope view starts the whip's rope at the held gun's muzzle).</summary>
        public static WeaponFx Current { get; private set; }

        /// <summary>NIGHT33 W (view33): the held gun's muzzle in the world (the KESTREL / THUNDERCLAP / LONGWAVE rigs' own muzzles; any other gun: its
        /// viewmodel root), at FxMuzzleDepth in the world camera's view.</summary>
        public Vector3 HeldMuzzle(string weapon)
        {
            if (weapon == "rail" || weapon == "shotgun" || weapon == "revolver") return Muzzle(weapon);
            Muzzle("revolver"); // finds the viewmodel camera once
            Transform root = vmCam != null ? vmCam.transform.Find("Viewmodel") : null;
            return ViewPoint(root, Vector3.zero);
        }

        /// <summary>A rig-space point of the viewmodel, seen through its camera and put at FxMuzzleDepth in the world camera's view.</summary>
        Vector3 ViewPoint(Transform rig, Vector3 rigPoint)
        {
            Camera main = game.cam;
            if (vmCam == null || rig == null) return main.transform.TransformPoint(hold);
            Vector3 vp = vmCam.WorldToViewportPoint(rig.TransformPoint(rigPoint));
            return main.ViewportToWorldPoint(new Vector3(vp.x, vp.y, UnityTuning.FxMuzzleDepth));
        }

        /// <summary>:61-69 stretch(): the unit cylinder (base at 0, top at +y 1) from a to b at `radius`.</summary>
        static void Stretch(Transform t, Vector3 a, Vector3 b, float radius)
        {
            Vector3 d = b - a;
            float length = d.magnitude;
            if (length < 1e-4f) return; // :64
            t.SetPositionAndRotation(a, Quaternion.FromToRotation(Vector3.up, d / length));
            t.localScale = new Vector3(radius, length, radius);
        }

        /// <summary>A copy of the neon (URP Unlit) material drawn additively: Blend One One, no depth write, transparent queue.</summary>
        static Material Additive(Material neon, string name)
        {
            var m = new Material(neon) { name = name, renderQueue = (int)RenderQueue.Transparent };
            m.SetFloat("_SrcBlend", (float)BlendMode.One);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0);
            return m;
        }

        static Transform Part(Transform root, string name, Mesh mesh, Material mat, out Renderer r)
        {
            GameObject go = MeshKit.Object(name, root, mesh, mat, shadows: false);
            r = go.GetComponent<Renderer>();
            r.enabled = false;
            return go.transform;
        }

        /// <summary>:21 CylinderGeometry(1, 1, 1, sides, 1, true).translate(0, 0.5, 0): an open tube of radius 1 from y 0 to 1.</summary>
        static Mesh Cylinder(int sides)
        {
            var kit = new MeshKit();
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                var p0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0));
                var p1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                kit.Quad(p0, p1, p1 + Vector3.up, p0 + Vector3.up, (p0 + p1).normalized, Color.white);
            }
            return kit.Build("weapon fx cylinder");
        }

        /// <summary>:26 SphereGeometry(1, width, height): a unit sphere (flat faces; the material is unlit).</summary>
        static Mesh Sphere(int width, int height)
        {
            var kit = new MeshKit();
            Vector3 P(int i, int j)
            {
                float th = i * Mathf.PI / height, ph = j * Mathf.PI * 2 / width;
                return new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph));
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Vector3 a = P(i, j), b = P(i, j + 1), c = P(i + 1, j + 1), d = P(i + 1, j);
                    Vector3 n = (a + b + c + d).normalized;
                    if (i == 0) kit.Triangle(a, c, d, n, Color.white); // a == b: the top pole
                    else if (i == height - 1) kit.Triangle(a, b, c, n, Color.white); // c == d: the bottom pole
                    else kit.Quad(a, b, c, d, n, Color.white);
                }
            }
            return kit.Build("weapon fx sphere");
        }
    }
}
