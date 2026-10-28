// NIGHT14 U3-27.9 (agent V): WHAT GOT YOU's stage. While the replay plays (SurvivalGame.Recap.cs) the live views are not
// rendered (RenderFrame returns after the UI) and their objects are hidden; this draws one recorded moment of the flight
// recorder instead, interpolated between its two nearest frames:
// - the enemies as proxies with the live look: the parts EnemyViews builds (husk.js: a capsule body and a tilted head box;
//   swarmer.js: three boxes; spitter.js: a leaned capsule, the acid sac, the box), their sizes and colours from config
//   where EnemyViews reads config and its own literals where it has them (swarmer.js / spitter.js parts), drawn with
//   EnemyLook's SKYGRIND/Enemy body material (the rim, the hit flash) through Graphics.RenderMesh; the wind-up / crouch /
//   tell pulses and the stagger's pale lean as the views draw them, on the replay's own clock;
// - the killer outlined: an inverted hull (the same parts grown by RecapOutlineWidth, front faces culled: URP Unlit's
//   _Cull) in colors.enemy x RecapOutlineBoost, and its globs too;
// - the globs (spitter.globRadius spheres, acid x 2 as SpitterView's), puddles (discs of spitter.puddleRadius, acid x 1.3
//   at 0.55 alpha on the acid material), the player's rockets and nails and the kill drops (small cyan cubes), the
//   player's hits and kills (white cubes for RecapHitShow of replay time);
// - where you were: a cyan (colors.usable) capsule of the movement body's size with a nub at eye height toward your look;
// - the camera: a slow orbit (RecapOrbitSpeedDeg) a little above (RecapOrbitPitchDeg) round the point between you and the
//   killer, its radius growing with your distance to it, starting behind your last look; a wall between pulls it in (the
//   C# collider's ray). The killer's head point in the viewport is kept for the name tag (UiRecap.cs).
// Why proxies and not the live views: the views draw the sim's live enemies (they read enemies.all and advance render
// timers on the sim objects), so they can't show a recorded frame without changing them (another owner's file); proxies
// draw any frame, need no GameObjects and leave the views exactly as they were. Nothing is allocated per frame (pooled
// property blocks, struct matrices, cached meshes and materials).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Skygrind.Game
{
    public sealed class RecapStage
    {
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly Mesh capsule, cube, sphere, cylinder;
        RenderParams bodyRp, hullRp, playerRp, globRp, puddleRp, usableRp, popRp;
        readonly MaterialPropertyBlock[] blocks;
        readonly MaterialPropertyBlock puddleBlock = new MaterialPropertyBlock();
        int blockNext;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor"), Flash = Shader.PropertyToID("_Flash"), RimOn = Shader.PropertyToID("_RimOn"),
            Cull = Shader.PropertyToID("_Cull");

        // The looks, read once (EnemyViews' numbers).
        readonly Color enemy, tell, swarmerBase, spitterBody, spitterBox, acid, hot;
        readonly float huskH, huskR, headShade, pulseScale, pulseRate, staggerPale, leanDeg, headTilt, headHeight, headForward;
        readonly Vector3 headBox;
        readonly float swarmerH, spitterH, spitterR, globR, puddleR, bodyR, bodyH, eyeH;
        // swarmer.js parts (SwarmerView: sizes, shades, the crouch's squash) and spitter.js's (SpitterView: the 0.25 rad
        // lean, the sac's 0.84 m and 0x fff0a0 tell, the box), as EnemyViews has them.
        static readonly Vector3[] SwarmerSizes = { new Vector3(0.46f, 0.32f, 0.8f), new Vector3(0.3f, 0.26f, 0.3f), new Vector3(0.62f, 0.3f, 0.62f) };
        static readonly float[] SwarmerShades = { 1, 0.8f, 0.62f };
        static readonly Vector3 SwarmerSquash = new Vector3(1.15f, 0.65f, 1.1f);
        const float SpitterLeanRad = 0.25f, SacSize = 0.84f, BoxShade = 0.7f;
        static readonly Vector3 SpitterBoxSize = new Vector3(0.4f, 0.3f, 0.4f);
        readonly Vector3 capsuleGrow, cubeGrow;

        // The live objects hidden while it plays (the views' roots: their pooled bodies keep their last pose), and the
        // viewmodel's overlay camera.
        static readonly string[] LiveRoots = { "Husks", "Swarmers", "Spitters", "Projectiles", "Power-ups", "Impact rings", "Weapon FX", "Dash tint" };
        readonly List<GameObject> live = new List<GameObject>();
        readonly List<bool> liveWas = new List<bool>();
        bool vmWas, shown;

        // The orbit
        float orbit, radius, clock;
        Vector3 lookAt;
        bool eased;

        /// <summary>The killer's head point in the main camera's viewport this frame (x, y 0..1; z its depth), and whether it is on screen.</summary>
        public Vector3 KillerViewport { get; private set; }
        public bool KillerShown { get; private set; }

        /// <summary>The killer is in the drawn frame (alive then, inside the ring).</summary>
        public bool KillerInFrame { get; private set; }

        public RecapStage(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            capsule = PrimitiveMesh(PrimitiveType.Capsule);
            cube = PrimitiveMesh(PrimitiveType.Cube);
            sphere = PrimitiveMesh(PrimitiveType.Sphere);
            cylinder = PrimitiveMesh(PrimitiveType.Cylinder);
            enemy = Look.C(cfg, "colors.enemy");
            tell = Look.C(cfg, "husk.windupColor");
            swarmerBase = enemy * (float)cfg.Num("swarmer.shade");
            swarmerBase.a = 1;
            spitterBody = enemy * (float)cfg.Num("spitter.shade");
            spitterBody.a = 1;
            spitterBox = enemy * BoxShade;
            spitterBox.a = 1;
            acid = Look.C(cfg, "spitter.acidColor");
            hot = Look.Hex(0xfff0a0);
            huskH = (float)cfg.Num("husk.height");
            huskR = (float)cfg.Num("husk.radius");
            headShade = (float)cfg.Num("husk.headShade");
            pulseScale = (float)cfg.Num("husk.windupPulseScale");
            pulseRate = (float)cfg.Num("husk.windupPulseRate");
            staggerPale = (float)cfg.Num("husk.staggerPale");
            leanDeg = (float)cfg.Num("husk.staggerLeanDeg");
            headTilt = (float)cfg.Num("husk.headTiltDeg");
            headHeight = (float)cfg.Num("husk.headHeight");
            headForward = (float)cfg.Num("husk.headForward");
            double[] hb = cfg.Nums("husk.headBox");
            headBox = new Vector3((float)hb[0], (float)hb[1], (float)hb[2]);
            swarmerH = (float)cfg.Num("swarmer.height");
            spitterH = (float)cfg.Num("spitter.height");
            spitterR = (float)cfg.Num("spitter.radius");
            globR = (float)cfg.Num("spitter.globRadius");
            puddleR = (float)cfg.Num("spitter.puddleRadius");
            bodyR = (float)cfg.movement.body.capsuleRadius;
            bodyH = (float)cfg.movement.body.capsuleHeight;
            eyeH = (float)cfg.movement.body.eyeHeight;
            float w = UnityTuning.RecapOutlineWidth;
            capsuleGrow = new Vector3(2 * w, w, 2 * w); // the capsule primitive is 2 m tall: its y scale is half its height
            cubeGrow = new Vector3(2 * w, 2 * w, 2 * w);

            Material neon = game.mats.neon;
            bodyRp = Params(look.body);
            var hull = new Material(neon) { name = "RecapOutline" };
            hull.SetFloat(Cull, (float)CullMode.Front); // the inverted hull: only its far side draws, round the body
            Color edge = enemy * UnityTuning.RecapOutlineBoost;
            edge.a = 1;
            hull.SetColor(BaseColor, edge);
            hullRp = Params(hull);
            Color usable = Look.C(cfg, "colors.usable");
            playerRp = Params(Tinted(neon, "RecapPlayer", usable));
            usableRp = Params(Tinted(neon, "RecapUsable", usable));
            globRp = Params(Tinted(neon, "RecapGlob", acid * 2)); // SpitterView's glob: acid x 2
            popRp = Params(Tinted(neon, "RecapPop", Color.white));
            puddleRp = Params(game.mats.acid);
            Color pc = acid * 1.3f; // SpitterView's puddle: acid x 1.3 at 0.55 alpha
            pc.a = 0.55f;
            puddleBlock.SetColor(BaseColor, pc);
            puddleRp.matProps = puddleBlock;
            blocks = new MaterialPropertyBlock[UnityTuning.RecapMaxEnemies * EnemyLook.MaxParts];
            for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (System.Array.IndexOf(LiveRoots, go.name) < 0) continue;
                live.Add(go);
                liveWas.Add(true);
            }
        }

        static Mesh PrimitiveMesh(PrimitiveType t)
        {
            var go = GameObject.CreatePrimitive(t);
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(go);
            return m;
        }

        static Material Tinted(Material from, string name, Color c)
        {
            var m = new Material(from) { name = name };
            c.a = 1;
            m.SetColor(BaseColor, c);
            return m;
        }

        static RenderParams Params(Material m) => new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };

        /// <summary>The replay starts: the live objects and the viewmodel hidden, the orbit behind your last look.</summary>
        public void Begin(FlightRecorder r)
        {
            for (int i = 0; i < live.Count; i++)
            {
                if (live[i] == null) continue;
                liveWas[i] = live[i].activeSelf;
                live[i].SetActive(false);
            }
            Camera vm = game.viewmodel?.Camera;
            if (vm != null)
            {
                vmWas = vm.enabled;
                vm.enabled = false;
            }
            shown = true;
            orbit = (float)r.Frame(r.Count - 1).yaw; // web: behind a look at yaw is (sin yaw, cos yaw)
            eased = false;
            clock = 0;
            KillerShown = KillerInFrame = false;
        }

        /// <summary>The replay ends: everything as it was (the frame loop places the camera again).</summary>
        public void End()
        {
            if (!shown) return;
            shown = false;
            for (int i = 0; i < live.Count; i++)
                if (live[i] != null) live[i].SetActive(liveWas[i]);
            Camera vm = game.viewmodel?.Camera;
            if (vm != null) vm.enabled = vmWas;
            KillerShown = KillerInFrame = false;
        }

        /// <summary>Draw the recorded moment at sim time `t` and place the camera (frame time `frameDt` drives the orbit and the pulses).</summary>
        public void Draw(FlightRecorder r, double t, double frameDt, int killerId)
        {
            float dt = (float)frameDt;
            clock += dt;
            blockNext = 0;
            int i = r.IndexAt(t);
            if (i < 0) return;
            int j = Mathf.Min(i + 1, r.Count - 1);
            ref readonly RecapFrame a = ref r.Frame(i);
            ref readonly RecapFrame b = ref r.Frame(j);
            double span = b.time - a.time;
            float u = span > 0 ? Mathf.Clamp01((float)((t - a.time) / span)) : 0;

            bool hasKiller = false;
            Vector3 killerAt = default;
            int killerKind = RecapKind.Husk;
            for (int k = 0; k < a.enemyCount; k++)
            {
                ref readonly RecapEnemy ea = ref r.Enemy(a, k);
                Vec3 p = ea.position;
                double yaw = ea.yaw;
                int kb = r.FindEnemy(b, ea.id, k);
                if (kb >= 0)
                {
                    ref readonly RecapEnemy eb = ref r.Enemy(b, kb);
                    p = Lerp(ea.position, eb.position, u);
                    yaw = LerpAngle(ea.yaw, eb.yaw, u);
                }
                Vector3 root = WebSpace.ToUnity(p);
                bool killer = ea.id == killerId;
                DrawEnemy(ea.kind, root, (float)(yaw * Mathf.Rad2Deg), ea.state, ea.phase, killer);
                if (!killer) continue;
                hasKiller = true;
                killerAt = root;
                killerKind = ea.kind;
            }
            DrawMarks(r, a, killerId);
            DrawPops(r, i, t);
            Vector3 feet = WebSpace.ToUnity(Lerp(a.feet, b.feet, u));
            DrawPlayer(feet, LerpAngle(a.yaw, b.yaw, u));
            PlaceCamera(feet, hasKiller, killerAt, dt);
            KillerInFrame = hasKiller;
            if (hasKiller)
            {
                Vector3 head = killerAt + Vector3.up * (HeightOf(killerKind) + UnityTuning.RecapTagAbove);
                Vector3 v = game.cam.WorldToViewportPoint(head);
                KillerViewport = v;
                KillerShown = v.z > 0 && v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1;
            }
            else KillerShown = false;
        }

        float HeightOf(int kind) => kind == RecapKind.Swarmer ? swarmerH : kind == RecapKind.Spitter ? spitterH : huskH;

        static Vec3 Lerp(Vec3 a, Vec3 b, float u) => new Vec3(a.x + (b.x - a.x) * u, a.y + (b.y - a.y) * u, a.z + (b.z - a.z) * u);

        static double LerpAngle(double a, double b, float u) => a + System.Math.IEEERemainder(b - a, 2 * System.Math.PI) * u;

        void DrawEnemy(int kind, Vector3 root, float yawDeg, int state, double phase, bool killer)
        {
            Quaternion yawQ = Quaternion.Euler(0, -yawDeg, 0); // web yaw (0 faces -z) -> Unity, as the views turn it
            bool wind = (state & RecapState.Windup) != 0, stagger = (state & RecapState.Stagger) != 0, flash = (state & RecapState.Flash) != 0;
            switch (kind)
            {
                case RecapKind.Swarmer:
                {
                    Color c = swarmerBase;
                    if (wind) c = Color.Lerp(c, tell, 0.5f + 0.5f * Mathf.Sin(clock * 40));
                    Part(cube, root + yawQ * new Vector3(0, swarmerH * 0.45f, 0), yawQ, wind ? Vector3.Scale(SwarmerSizes[0], SwarmerSquash) : SwarmerSizes[0], c * SwarmerShades[0], flash, 1, killer, cubeGrow);
                    Part(cube, root + yawQ * new Vector3(0, swarmerH * 0.62f, 0.42f), yawQ, wind ? Vector3.Scale(SwarmerSizes[1], SwarmerSquash) : SwarmerSizes[1], c * SwarmerShades[1], flash, 1, killer, cubeGrow);
                    Part(cube, root + yawQ * new Vector3(0, swarmerH * 0.18f, 0), yawQ, wind ? Vector3.Scale(SwarmerSizes[2], SwarmerSquash) : SwarmerSizes[2], c * SwarmerShades[2], flash, 1, killer, cubeGrow);
                    break;
                }
                case RecapKind.Spitter:
                {
                    Quaternion lean = Quaternion.Euler(-SpitterLeanRad * Mathf.Rad2Deg, 0, 0); // about the world x after the yaw (composeAt)
                    Part(capsule, root + lean * new Vector3(0, spitterH * 0.5f, 0), lean * yawQ, new Vector3(spitterR * 2, spitterH / 2, spitterR * 2), spitterBody, flash, 1, killer, capsuleGrow);
                    float swell = wind ? 1 + 0.6f * Mathf.Min(1, (float)phase) : 1;
                    Color sac = wind ? Color.Lerp(acid, hot, 0.5f + 0.5f * Mathf.Sin(clock * 30)) * 2.2f : acid;
                    Part(sphere, root + yawQ * new Vector3(0, spitterH * 0.72f, -0.32f), yawQ, Vector3.one * (SacSize * swell), sac, flash, 0, killer, cubeGrow);
                    Part(cube, root + yawQ * new Vector3(0, spitterH * 0.9f, 0.3f), yawQ, SpitterBoxSize, spitterBox, flash, 1, killer, cubeGrow);
                    break;
                }
                default: // the husk (and a kind the port has no body for yet)
                {
                    float wave = wind ? 0.5f + 0.5f * Mathf.Sin(clock * pulseRate) : 0;
                    float pulse = 1 + (pulseScale - 1) * wave;
                    Color c = enemy;
                    if (wind) c = Color.Lerp(c, tell, 0.5f + 0.5f * wave);
                    if (stagger) c = Color.Lerp(c, Color.white, staggerPale);
                    Quaternion rot = stagger ? yawQ * Quaternion.Euler(-leanDeg, 0, 0) : yawQ; // the stagger leans it back
                    Part(capsule, root + rot * new Vector3(0, huskH / 2, 0) * pulse, rot, new Vector3(huskR * 2, huskH / 2, huskR * 2) * pulse, c, flash, 1, killer, capsuleGrow);
                    Part(cube, root + rot * new Vector3(0, headHeight, headForward) * pulse, rot * Quaternion.Euler(headTilt, 0, 0), headBox * pulse, c * headShade, flash, 1, killer, cubeGrow);
                    break;
                }
            }
        }

        /// <summary>One body part with the enemy look (and the killer's hull round it).</summary>
        void Part(Mesh mesh, Vector3 pos, Quaternion rot, Vector3 scale, Color c, bool flash, float rim, bool killer, Vector3 grow)
        {
            MaterialPropertyBlock block = blocks[blockNext];
            blockNext = blockNext + 1 == blocks.Length ? 0 : blockNext + 1;
            c.a = 1;
            block.SetColor(BaseColor, look.ok || !flash ? c : Color.white); // EnemyLook.Tint's rule (the neon fallback flashes by colour)
            block.SetFloat(Flash, flash ? 1 : 0);
            block.SetFloat(RimOn, rim);
            bodyRp.matProps = block;
            bodyRp.worldBounds = new Bounds(pos, Vector3.one * 4);
            Graphics.RenderMesh(bodyRp, mesh, 0, Matrix4x4.TRS(pos, rot, scale));
            if (!killer) return;
            hullRp.worldBounds = bodyRp.worldBounds;
            Graphics.RenderMesh(hullRp, mesh, 0, Matrix4x4.TRS(pos, rot, scale + grow));
        }

        void Draw(ref RenderParams rp, Mesh mesh, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            rp.worldBounds = new Bounds(pos, Vector3.one * 4);
            Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.TRS(pos, rot, scale));
        }

        /// <summary>What flies and lies in the frame: globs (the killer's outlined), puddles, drops, rockets, nails.</summary>
        void DrawMarks(FlightRecorder r, in RecapFrame f, int killerId)
        {
            for (int k = 0; k < f.markCount; k++)
            {
                ref readonly RecapMark mk = ref r.Mark(f, k);
                Vector3 p = WebSpace.ToUnity(mk.position);
                switch (mk.kind)
                {
                    case RecapMarkKind.Glob:
                    case RecapMarkKind.ReflectedGlob:
                        Draw(ref globRp, sphere, p, Quaternion.identity, Vector3.one * (globR * 2));
                        if (mk.kind == RecapMarkKind.Glob && mk.enemyId == killerId && killerId >= 0)
                            Draw(ref hullRp, sphere, p, Quaternion.identity, Vector3.one * (globR * 2) + cubeGrow);
                        break;
                    case RecapMarkKind.Puddle:
                        Draw(ref puddleRp, cylinder, p + Vector3.up * UnityTuning.RecapPuddleLift, Quaternion.identity, new Vector3(puddleR * 2, UnityTuning.RecapPuddleThickness, puddleR * 2));
                        break;
                    case RecapMarkKind.Drop:
                        Draw(ref usableRp, cube, p + Vector3.up * UnityTuning.RecapDropSize, Quaternion.Euler(45, clock * 90, 45), Vector3.one * UnityTuning.RecapDropSize);
                        break;
                    case RecapMarkKind.Rocket:
                        Draw(ref usableRp, cube, p, Quaternion.identity, Vector3.one * UnityTuning.RecapRocketSize);
                        break;
                    case RecapMarkKind.Nail:
                        Draw(ref usableRp, cube, p, Quaternion.identity, Vector3.one * UnityTuning.RecapNailSize);
                        break;
                }
            }
        }

        /// <summary>The player's hits and kills of the last RecapHitShow of replay time, as white cubes.</summary>
        void DrawPops(FlightRecorder r, int i, double t)
        {
            for (int j = i; j >= 0; j--)
            {
                ref readonly RecapFrame f = ref r.Frame(j);
                if (t - f.time > UnityTuning.RecapHitShow) break;
                for (int k = 0; k < f.markCount; k++)
                {
                    ref readonly RecapMark mk = ref r.Mark(f, k);
                    if (mk.kind != RecapMarkKind.Hit && mk.kind != RecapMarkKind.Kill) continue;
                    float s = mk.kind == RecapMarkKind.Kill ? UnityTuning.RecapKillSize : UnityTuning.RecapHitSize;
                    Draw(ref popRp, cube, WebSpace.ToUnity(mk.position), Quaternion.Euler(45, 45, 0), Vector3.one * s);
                }
            }
        }

        /// <summary>Where you were: a cyan capsule of the movement body and a nub at eye height toward your look.</summary>
        void DrawPlayer(Vector3 feet, double yaw)
        {
            Draw(ref playerRp, capsule, feet + Vector3.up * (bodyH / 2), Quaternion.identity, new Vector3(bodyR * 2, bodyH / 2, bodyR * 2));
            var lookDir = new Vector3(-(float)System.Math.Sin(yaw), 0, (float)System.Math.Cos(yaw)); // web (-sin, -cos) -> Unity z flipped
            Draw(ref playerRp, cube, feet + Vector3.up * eyeH + lookDir * UnityTuning.RecapNoseForward, Quaternion.LookRotation(lookDir), Vector3.one * UnityTuning.RecapNoseSize);
        }

        /// <summary>The orbit: round the point between you and the killer, a little above, a wall pulling it in.</summary>
        void PlaceCamera(Vector3 feet, bool hasKiller, Vector3 killerAt, float dt)
        {
            Vector3 target = feet + Vector3.up * UnityTuning.RecapLookHeight;
            float dist = 0;
            if (hasKiller)
            {
                Vector3 k = killerAt + Vector3.up * UnityTuning.RecapLookHeight;
                dist = Vector3.Distance(target, k);
                target = (target + k) * 0.5f;
            }
            float want = Mathf.Clamp(UnityTuning.RecapOrbitRadiusMin + dist * UnityTuning.RecapOrbitRadiusPerMeter, UnityTuning.RecapOrbitRadiusMin, UnityTuning.RecapOrbitRadiusMax);
            float ease = eased ? 1 - Mathf.Exp(-UnityTuning.RecapCameraEase * dt) : 1;
            lookAt = Vector3.Lerp(lookAt, target, ease);
            radius = Mathf.Lerp(radius, want, ease);
            eased = true;
            orbit += dt * UnityTuning.RecapOrbitSpeedDeg * Mathf.Deg2Rad;
            float pitch = UnityTuning.RecapOrbitPitchDeg * Mathf.Deg2Rad;
            // web offset (sin orbit, ., cos orbit) -> Unity (x, y, -z)
            var dir = new Vector3(Mathf.Sin(orbit) * Mathf.Cos(pitch), Mathf.Sin(pitch), -Mathf.Cos(orbit) * Mathf.Cos(pitch));
            float rr = radius;
            if (game.world.RayIntersect(WebSpace.FromUnity(lookAt), WebSpace.FromUnity(dir), out RayHit hit) && hit.distance < rr + UnityTuning.RecapCameraClearance)
                rr = Mathf.Max(UnityTuning.RecapCameraMinRadius, (float)hit.distance - UnityTuning.RecapCameraClearance);
            Transform ct = game.cam.transform;
            ct.position = lookAt + dir * rr;
            ct.rotation = Quaternion.LookRotation(-dir, Vector3.up);
            game.cam.fieldOfView = UnityTuning.RecapFovDeg;
        }
    }
}
