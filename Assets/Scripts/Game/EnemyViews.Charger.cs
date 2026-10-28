// NIGHT19 U3-33.7 (builder E owns this file): the CHARGER as charger.js draws it (GG's design, rebuilt as it is; CLAUDE.md
// rule 7), a new partial of SurvivalGame through the shared commit's hooks (N19ChargerViewInit / N19ChargerViewRender):
// - the body: a 1.35 x (height x 0.8) x 1.2 box at height x 0.5 in colors.enemy x charger.shade; the tell stomps it
//   (|sin(age x 18)| x 0.12 up) and pulses it toward husk.windupColor (0.5 + 0.5 sin(age x 30)); the charge pitches it
//   0.18 rad; a stun darkens it x0.6; blocked (it met a jump it can't make) shakes its yaw (sin(age x 26) x 0.18) and
//   pulses it toward a dazed grey 0x9a9aa8 (0.45 + 0.25 sin(age x 12));
// - the horn: a 1.0 x 0.3 x 0.5 box at (0, height x 0.72, 0.62 forward) in colors.enemy x 0.7;
// - the back weak point: a 0.8 x 0.8 x 0.1 plate at (0, height x 0.62, 0.66 back), no rim, 0x5a2a20, glowing 0xffe14a x 2.5
//   while stunned;
// - the charge line painted on the ground during the tell only: charger.lineWidth wide, charger.chargeDistance long from
//   its feet along the locked chargeYaw, 0.03 m up, in colors.enemy (the DANGER role) x (1.2 + 0.5 sin(age x 24)).
// A hit flashes it white (EnemyLook), a kill breaks it into its blocks (EnemyLook.Die); its stun throws colors.slamDust
// cubes (charger.js: particles.burst at +1.2 m, 16, 6, 0.5, 1.4). FeelProbe.Enemy reports it as the husk view does.
// Web space (forward -z) -> Unity (+z): the offsets' z and the yaw / pitch are negated, as HuskView does.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        ChargerView chargerView;

        partial void N19ChargerViewInit()
        {
            chargerView = new ChargerView(this);
            burrowerView = new BurrowerView(this); // NIGHT43 A1 burrow43: the BURROWER's view (Enemy43View.Burrow.cs; draws nothing until one spawns)
            ring43View = new WyrmRing43View(this); // NIGHT43 A1 round 3: THE RING dressed as the kiln's heart (WyrmRing43View.cs; look43, built on the worm room's first fight)
            Color dust = Look.C(cfg, "colors.slamDust");
            events.On<ChargerEvent>("chargerStunned", e =>
            {
                // charger.js: particles.burst(position + 1.2 up, colors.slamDust, 16, 6, 0.5, 1.4)
                foreach (Enemy en in enemies.all)
                {
                    if (en.id != e.id || !en.alive) continue;
                    particles?.Burst(new Vec3(en.position.x, en.position.y + 1.2, en.position.z), dust, 16, 6, 0.5f, 1.4f);
                    break;
                }
            });
        }

        partial void N19ChargerViewRender(double alpha, double frameDt)
        {
            chargerView?.Render(alpha, frameDt);
            burrowerView?.Render(alpha, frameDt); // NIGHT43 A1 burrow43
            ring43View?.Render(); // NIGHT43 A1 round 3: THE RING
        }
    }

    public sealed class ChargerView
    {
        sealed class Body
        {
            public Transform root;
            public readonly Transform[] parts = new Transform[3];
            public readonly Renderer[] rends = new Renderer[3];
            public readonly Color[] colors = new Color[3];
            public Transform line;
            public Renderer lineRend;
            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)
            public Look43 l43; // NIGHT43 A1 models43: the rebuilt model (Enemy43View.cs)
            public bool low; // NIGHT43 A1: the low long ram body (models40's; models43 builds on it too)
            public Enemy drawn;
            public Transform[] p47; // NIGHT47 B chargerLook47: the prow, two horn blades, three back vents (null: the view row off)
            public Renderer[] r47;
        }

        static readonly float[] Rims = { 1, 1, 0 }; // charger.js: the back plate has rim: false
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made
        readonly Color danger40, parry40; // NIGHT40 E3: the parts' red-orange and yellow
        readonly List<Body> pool = new List<Body>();
        int drawnCount;
        Mesh[] meshes;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock(), lineBlock = new MaterialPropertyBlock();
        readonly Material lineMat;
        readonly Transform parent;
        readonly Color body, horn, enemy, tell, dazed, backCold, backHot;
        readonly float height, lineWidth, chargeDistance;
        // charger.js's boxes (web metres): the body (x, height x 0.8, z), the horn, the back plate
        static readonly Vector3 HornSize = new Vector3(1.0f, 0.3f, 0.5f), BackSize = new Vector3(0.8f, 0.8f, 0.1f);
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public ChargerView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game); // SKYGRIND/Enemy: unlit, as the web's MeshBasicMaterial parts
            models40 = Enemy40Parts.On(game.cfg);
            danger40 = Look.Hex(UnityTuning.Tell36RedHex);
            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);
            parent = new GameObject("Chargers").transform;
            enemy = Look.C(cfg, "colors.enemy");
            body = enemy * (float)cfg.Num("charger.shade");
            body.a = 1;
            horn = enemy * 0.7f; // charger.js: colors.enemy x 0.7
            horn.a = 1;
            tell = Look.C(cfg, "husk.windupColor");
            dazed = Look.Hex(0x9a9aa8); // charger.js: the blocked daze's grey
            backCold = Look.Hex(0x5a2a20); // charger.js: the back plate
            backHot = Look.Hex(0xffe14a, 2.5f); // charger.js: the stunned back plate's glow
            height = (float)cfg.Num("charger.height");
            lineWidth = (float)cfg.Num("charger.lineWidth");
            chargeDistance = (float)cfg.Num("charger.chargeDistance");
            lineMat = new Material(game.mats.neon) { name = "Charger line" };
            game.events.On<KillEvent>("kill", OnKill);
        }

        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 3, -1, b.l43 != null ? b.l43.DyingParts(b.m40) : b.m40); // no head part; NIGHT41 E6 models41: + the parts break (NIGHT43 A1: models43's while it draws)
                return;
            }
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("charger").transform;
            b.root.SetParent(parent, false);
            var sizes = new[] { new Vector3(1.35f, height * 0.8f, 1.2f), HornSize, BackSize };
            var offsets = new[] { new Vector3(0, height * 0.5f, 0), new Vector3(0, height * 0.72f, 0.62f), new Vector3(0, height * 0.62f, -0.66f) }; // web z -> Unity -z
            b.low = models40 || Models43.On(game.cfg); // NIGHT43 A1 models43: the rebuilt charger is the low long ram too
            if (b.low)
            {
                // NIGHT40 E3 models40 (round 4: the reader paired the TANK and the CHARGER as "both heavy squares"): the body low and long (a battering
                // ram), the head plate down at its nose, the back plate (the weak point) on its rump; the sim's hit shape is unchanged (view only)
                sizes[0] = new Vector3(UnityTuning.Models40ChargerBody[0], height * UnityTuning.Models40ChargerBody[1], UnityTuning.Models40ChargerBody[2]);
                offsets[1] = new Vector3(0, height * UnityTuning.Models40ChargerHead[0], UnityTuning.Models40ChargerHead[1]);
                offsets[2] = new Vector3(0, height * UnityTuning.Models40ChargerBack[0], UnityTuning.Models40ChargerBack[1]);
            }
            for (int i = 0; i < 3; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(b.root, false);
                go.transform.localPosition = offsets[i];
                go.transform.localScale = sizes[i];
                b.parts[i] = go.transform;
                b.rends[i] = go.GetComponent<Renderer>();
                look.Attach(b.rends[i], i == 0, true); // enemyKind.js xrayMesh: the body; the heavy reads as an elite
            }
            if (meshes == null)
            {
                Mesh cube = b.parts[0].GetComponent<MeshFilter>().sharedMesh;
                meshes = new[] { cube, cube, cube };
            }
            if (NightRows.ViewOn(game.cfg, "chargerLook47"))
            {
                // NIGHT47 B chargerLook47 (the CHARGER card: one dominant shape, a wedge PROW over the head, two swept horn blades, a bank of three
                // VENT slats on its back that glow red-orange in the wind-up and open YELLOW when it is stunned; UnityTuning.Brawl47.cs)
                b.p47 = new Transform[6];
                b.r47 = new Renderer[6];
                for (int i = 0; i < 6; i++)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Object.Destroy(go.GetComponent<Collider>());
                    go.transform.SetParent(b.root, false);
                    b.p47[i] = go.transform;
                    b.r47[i] = go.GetComponent<Renderer>();
                    look.Attach(b.r47[i], false, true);
                }
                float h = height;
                b.p47[0].localPosition = new Vector3(0, h * UnityTuning.Charger47ProwY, UnityTuning.Charger47ProwZ);
                b.p47[0].localScale = UnityTuning.Charger47ProwSize;
                b.p47[0].localRotation = Quaternion.Euler(UnityTuning.Charger47ProwPitch, 0, 0);
                for (int s = 0; s < 2; s++)
                {
                    float side = s == 0 ? -1 : 1;
                    b.p47[1 + s].localPosition = new Vector3(side * UnityTuning.Charger47HornX, h * UnityTuning.Charger47HornY, UnityTuning.Charger47HornZ);
                    b.p47[1 + s].localScale = UnityTuning.Charger47HornSize;
                    b.p47[1 + s].localRotation = Quaternion.Euler(UnityTuning.Charger47HornPitch, side * UnityTuning.Charger47HornYaw, 0);
                }
                for (int k = 0; k < 3; k++)
                {
                    b.p47[3 + k].localPosition = new Vector3(0, h * (UnityTuning.Charger47VentY0 + k * UnityTuning.Charger47VentStep), UnityTuning.Charger47VentZ);
                    b.p47[3 + k].localScale = UnityTuning.Charger47VentSize;
                }
            }
            // The charge line: a flat box from the feet forward (its own transform: it keeps the locked yaw, not the body's).
            var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(line.GetComponent<Collider>());
            line.transform.SetParent(parent, false);
            b.line = line.transform;
            b.lineRend = line.GetComponent<Renderer>();
            b.lineRend.sharedMaterial = lineMat;
            b.lineRend.shadowCastingMode = ShadowCastingMode.Off;
            b.lineRend.receiveShadows = false;
            line.SetActive(false);
            if (models40) b.m40 = Enemy40Parts.Make("charger", b.root, look, height); // NIGHT40 E3 models40
            b.l43 = new Look43("charger", b.root, b.rends, UnityTuning.Models43ChargerBase, null, look, height); // NIGHT43 A1 models43
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            // NIGHT47 B chargerLook47 (view row): the line's follow / lock states, only while the sim's charger47 gives a line that follows
            bool look47 = game.sim != null && game.sim.Charger47On && NightRows.ViewOn(game.cfg, "chargerLook47");
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Charger c) || !c.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                b.drawn = c;
                b.root.gameObject.SetActive(true);
                c.flash = System.Math.Max(0, c.flash - frameDt);
                c.drawAge += frameDt;
                float age = (float)c.drawAge;
                bool telling = c.state == "tell", charging = c.state == "charge", stunned = c.state == "stunned", blocked = c.state == "blocked";
                Color bc = body;
                if (telling) bc = Color.Lerp(bc, tell, 0.5f + 0.5f * Mathf.Sin(age * 30));
                if (stunned) bc *= 0.6f;
                if (blocked) bc = Color.Lerp(bc, dazed, 0.45f + 0.25f * Mathf.Sin(age * 12));
                bc.a = 1;
                float stomp = telling ? Mathf.Abs(Mathf.Sin(age * 18)) * 0.12f : 0;
                float shake = blocked ? Mathf.Sin(age * 26) * 0.18f : 0;
                float pitch = charging ? 0.18f : 0;
                Vec3 p = c.previous + (c.position - c.previous) * alpha;
                Quaternion yawQ = Quaternion.Euler(0, -(float)(c.yaw * Mathf.Rad2Deg), 0);
                float flash = look.React(c.id, yawQ, c.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt); // NIGHT17
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * yawQ;
                b.root.localScale = squash;
                // The body's own yaw shake and pitch (composeAt(yaw + shake, pitch)): about the root's axes.
                b.parts[0].localRotation = Quaternion.Euler(0, -shake * Mathf.Rad2Deg, 0) * Quaternion.Euler(-pitch * Mathf.Rad2Deg, 0, 0);
                b.parts[0].localPosition = b.parts[0].localRotation * new Vector3(0, height * (b.low ? UnityTuning.Models40ChargerBodyY : 0.5f) + stomp, 0); // NIGHT40 E3 models40: the low body's centre
                b.l43.Apply(Models43.On(game.cfg), b.m40); // NIGHT43 A1 models43: the body and the brow darkened, the back plate keeps its weak-point colours
                Color bc43 = b.l43.on ? Models43.Body(bc) : bc;
                b.colors[0] = bc43;
                b.colors[1] = b.l43.on ? Models43.Body(horn) : horn;
                b.colors[2] = stunned ? backHot : backCold;
                for (int i = 0; i < 3; i++) look.Tint(block, b.rends[i], b.colors[i], flash, Rims[i]);
                if (b.p47 != null)
                {
                    // NIGHT47 B chargerLook47: the prow dark steel (rimmed), the horns red-orange, the vents by state: dark idle, a red-orange ramp
                    // through the wind-up, YELLOW open while stunned (punch it: the parry language); the vents open (stretch) when stunned
                    double tellFull = System.Math.Max(1, MovementMath.StepsFor(c.Attack.tellTime, game.cfg.Dt));
                    float ramp = telling ? Mathf.Clamp01(1f - (float)(c.stateSteps / tellFull)) : charging ? 1f : 0f;
                    Color vent = stunned ? parry40 * UnityTuning.Charger47VentOpenGain : Color.Lerp(Look.Hex(UnityTuning.Charger47VentIdleHex), danger40 * UnityTuning.Charger47VentHotGain, ramp);
                    look.Tint(block, b.r47[0], Look.Hex(UnityTuning.Charger47ProwHex), flash, 1);
                    look.Tint(block, b.r47[1], danger40 * UnityTuning.Charger47HornGain, flash, 0);
                    look.Tint(block, b.r47[2], danger40 * UnityTuning.Charger47HornGain, flash, 0);
                    for (int k = 0; k < 3; k++)
                    {
                        look.Tint(block, b.r47[3 + k], vent, flash, 0);
                        Vector3 vs = UnityTuning.Charger47VentSize;
                        b.p47[3 + k].localScale = stunned ? new Vector3(vs.x, vs.y * UnityTuning.Charger47VentOpen, vs.z) : vs;
                    }
                }
                b.m40?.Pose(look, block, bc, flash, telling ? 1 : charging ? 0.6f : 0, stunned, danger40, parry40, age); // NIGHT40 E3 models40
                if (b.l43.on) b.l43.extras.Pose(look, block, bc43, flash, telling ? 1 : charging ? 0.6f : 0, stunned, danger40, parry40, age); // NIGHT43 A1 models43
                look.Notch(c.id, b.root);
                FeelProbe.Enemy(c.id, b.rends[0].bounds, b.rends[1].bounds, flash); // NIGHT18: the reel's victim rect (M5)
                // The painted charge line (the tell only), along the locked yaw from the feet.
                if (telling != b.line.gameObject.activeSelf) b.line.gameObject.SetActive(telling);
                if (telling)
                {
                    Quaternion lineQ = Quaternion.Euler(0, -(float)(c.chargeYaw * Mathf.Rad2Deg), 0);
                    b.line.rotation = lineQ;
                    b.line.position = WebSpace.ToUnity(new Vec3(p.x, p.y + 0.03, p.z)) + lineQ * new Vector3(0, 0, chargeDistance * 0.5f);
                    b.line.localScale = new Vector3(lineWidth, 0.02f, chargeDistance);
                    Color lc = enemy * (1.2f + Mathf.Sin(age * 24) * 0.5f);
                    if (look47) // NIGHT47 B chargerLook47: following = dim, slow pulse; locked = bright, steady, white-edged, wider (UnityTuning.Brawl47.cs)
                    {
                        if (c.locked47)
                        {
                            lc = Color.Lerp(enemy, Color.white, UnityTuning.Charger47LockWhite) * UnityTuning.Charger47LineLocked;
                            b.line.localScale = new Vector3(lineWidth * UnityTuning.Charger47LockWiden, 0.02f, chargeDistance);
                        }
                        else lc = enemy * (UnityTuning.Charger47LineFollow * (1f + 0.25f * Mathf.Sin(age * UnityTuning.Charger47PulseRate)));
                    }
                    lc.a = 1;
                    lineBlock.SetColor(BaseColor, lc);
                    b.lineRend.SetPropertyBlock(lineBlock);
                }
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                pool[i].root.gameObject.SetActive(false);
                if (pool[i].line.gameObject.activeSelf) pool[i].line.gameObject.SetActive(false);
                pool[i].drawn = null;
            }
        }
    }
}
