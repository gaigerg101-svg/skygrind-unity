// NIGHT32 builder C (design.n32.boss5; DESIGN32 C): THE CROWN as the view draws it. View only; installs itself (RuntimeInitializeOnLoadMethod, as GrappleNodes30: no line in the
// lead's SurvivalGame files); Unity objects are made on first use, never in field initialisers (L-035). Not compiled in the Editor by its author (dn-check only).
//  - THE RING: the SAME triangles as the collider (Crown32Arena.tris), one mesh per surface kind, each its own copy of the stone material tinted through _BaseColor (Boss30View's
//    recipe: the stone material ignores vertex colours): the wall, the inner disc, the outer sectors, the roof, the ledges; rebuilt when the drop ring changes.
//  - THE DROP RING: a sector in its tell glows red-orange on the floor (pulsing); a down sector's floor is red-orange LAVA.
//  - THE CROWN: a dark core cube with a red-orange band and a crown of red-orange prongs; three plates: cyan while open (x2.5, pulsing through the beat-3 stun), dim grey when shut.
//  - THE TELLS (rule 8, red-orange danger only): the beam's tell (a faint beam where it will start) and the beam; the orbs; the mortar's mark on the floor (it brightens to the
//    landing); the charge line to the wall. The beat-2 grapple rings are GrappleNodes30's (they are in the level's grapple nodes).
// Numbers: UnityTuning.Crown32.cs. Off: the run's boss5 block off, or no crown.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Crown32View : MonoBehaviour
    {
        /// <summary>The live view (the reel reads what it drew).</summary>
        public static Crown32View Current;

        SurvivalGame game;
        Transform root, ringRoot, crownRoot;
        Mesh boxMesh, ballMesh;
        Material add, bodyMat;
        MaterialPropertyBlock block;
        int baseColorId;
        static readonly Color White = Color.white;

        readonly MeshKit[] kits = new MeshKit[6];
        readonly Mesh[] meshes = new Mesh[6];
        readonly Material[] ringMats = new Material[6];
        Renderer glowR;
        MeshKit glowKit;
        Mesh glowMesh;
        int laidDown = -1, laidTell = -1;
        Crown32Arena laidArena;

        Transform coreT, bandT, beamT, markT, lineT, lightT;
        Renderer bandR, beamR, markR, lineR;
        readonly Transform[] plateT = new Transform[3];
        readonly Renderer[] plateR = new Renderer[3];
        readonly List<Transform> prongT = new List<Transform>();
        readonly List<Transform> orbT = new List<Transform>();
        readonly List<Renderer> orbR = new List<Renderer>();
        Light ringLight;

        /// <summary>What is drawn now (the reel counts frames of each).</summary>
        public bool CrownShown => crownRoot != null && crownRoot.gameObject.activeSelf;
        public bool BeamShown => beamT != null && beamT.gameObject.activeSelf;
        public bool MarkShown => markT != null && markT.gameObject.activeSelf;
        public bool LineShown => lineT != null && lineT.gameObject.activeSelf;
        public int PlatesLit { get; private set; }
        public int SectorsGlowing { get; private set; }
        public int OrbsShown { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Crown32View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Crown32View>();
        }

        void Awake()
        {
            Current = this;
            block = new MaterialPropertyBlock();
            baseColorId = Shader.PropertyToID("_BaseColor");
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null) { game = null; return false; }
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("NIGHT32 CROWN").transform;
            DontDestroyOnLoad(root.gameObject);
            ringRoot = new GameObject("crown ring").transform;
            ringRoot.SetParent(root, false);
            crownRoot = new GameObject("the crown").transform;
            crownRoot.SetParent(root, false);
            if (boxMesh == null)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
                Destroy(cube);
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ballMesh = ball.GetComponent<MeshFilter>().sharedMesh;
                Destroy(ball);
            }
            add = FxDraw.Additive(game.mats.neon, "Crown32 glow");
            bodyMat = FxDraw.Opaque(game.mats.neon, "Crown32 body", Look.Hex(UnityTuning.Crown32BodyHex));
            // the ring: one object per surface kind (wall, inner disc, sectors, roof, ledges, pit walls)
            uint[] hex = { UnityTuning.Crown32WallHex, UnityTuning.Crown32InnerHex, UnityTuning.Crown32SectorHex, UnityTuning.Crown32RoofHex, UnityTuning.Crown32LedgeHex, UnityTuning.Crown32SectorHex };
            string[] names = { "crown wall", "crown inner disc", "crown sectors", "crown roof", "crown ledges", "crown pit walls" };
            for (int k = 0; k < kits.Length; k++)
            {
                kits[k] = new MeshKit();
                meshes[k] = new Mesh { name = names[k] };
                var mat = new Material(game.mats.stone) { name = names[k] };
                Color c = Look.Hex(hex[k]);
                if (mat.HasProperty(baseColorId)) mat.SetColor(baseColorId, c);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
                mat = Light54.StoneOrLit(game, mat, "Crown32View", names[k], hex[k]); // NIGHT54 light54: the stone copy drew white (its kits' White vertex colour); the hex on URP Lit with the row on
                ringMats[k] = mat;
                MeshKit.Object(names[k], ringRoot, meshes[k], mat, shadows: false);
            }
            glowKit = new MeshKit();
            glowMesh = new Mesh { name = "crown drop glow" };
            glowR = MeshKit.Object("crown drop glow", ringRoot, glowMesh, add, shadows: false).GetComponent<Renderer>();
            var lg = new GameObject("crown light");
            lg.transform.SetParent(ringRoot, false);
            lightT = lg.transform;
            ringLight = lg.AddComponent<Light>();
            ringLight.type = LightType.Point;
            ringLight.shadows = LightShadows.None;
            ringLight.range = UnityTuning.Crown32LightRange;
            ringLight.intensity = UnityTuning.Crown32LightIntensity * UnityTuning.LightScale;
            ringLight.color = Look.Hex(UnityTuning.Crown32LightHex);
            // the crown
            coreT = Part(crownRoot, "core", bodyMat, out _);
            bandR = Part(crownRoot, "core band", add, out bandT).GetComponent<Renderer>();
            for (int k = 0; k < 3; k++) plateR[k] = Part(crownRoot, "plate " + k, add, out plateT[k]).GetComponent<Renderer>();
            prongT.Clear();
            for (int s = 0; s < 4; s++)
                for (int i = 0; i < UnityTuning.Crown32Prongs; i++)
                {
                    Part(crownRoot, "prong", add, out Transform t);
                    Tint(t.GetComponent<Renderer>(), Look.Hex(UnityTuning.Crown32DangerHex));
                    prongT.Add(t);
                }
            beamR = Part(root, "beam", add, out beamT).GetComponent<Renderer>();
            markR = Part(root, "mortar mark", add, out markT).GetComponent<Renderer>();
            markT.GetComponent<MeshFilter>().sharedMesh = ballMesh;
            lineR = Part(root, "charge line", add, out lineT).GetComponent<Renderer>();
            orbT.Clear(); orbR.Clear();
            laidDown = laidTell = -1;
            laidArena = null;
            root.gameObject.SetActive(false);
            return true;
        }

        Transform Part(Transform parent, string name, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return t;
        }

        void Tint(Renderer r, Color c)
        {
            block.Clear();
            block.SetColor(baseColorId, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => WebSpace.ToUnity(v);
        static Vector3 N(Vec3 n) => new Vector3((float)n.x, (float)n.y, (float)-n.z);
        static Vector3 Dir(double a) => new Vector3((float)JsMath.Cos(a), 0, (float)-JsMath.Sin(a)); // web atan2(z, x) angle -> a Unity flat direction

        /// <summary>The ring's meshes from the arena's triangles (the collider's), when the drop ring changed.</summary>
        void LayRing(Crown32Arena a, Vec3 o)
        {
            foreach (MeshKit k in kits) k.Clear();
            foreach (Crown32Tri t in a.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
                int k = t.part == Crown32Part.Wall ? 0 : t.part == Crown32Part.Inner ? 1 : t.part == Crown32Part.Sector ? 2 : t.part == Crown32Part.Roof ? 3 : t.part == Crown32Part.LedgeTop || t.part == Crown32Part.LedgeSide ? 4 : 5;
                kits[k].Triangle(U(t.a + o), U(t.b + o), U(t.c + o), N(n), White);
            }
            for (int k = 0; k < kits.Length; k++) kits[k].Build(meshes[k].name, meshes[k]);
            lightT.position = U(new Vec3(o.x, o.y + a.p.height - UnityTuning.Crown32LightBelowRoof, o.z));
            laidDown = a.DownMask;
            laidArena = a;
        }

        /// <summary>The red-orange floor glow: the tell sectors on the floor, the down sectors' lava.</summary>
        void LayGlow(Crown32Arena a, Vec3 o, int down, int tell)
        {
            glowKit.Clear();
            int segs = a.FloorSegments;
            double segA = 2 * System.Math.PI / segs, rIn = a.p.innerRadius, rOut = a.CornerRadius;
            for (int s = 0; s < a.p.sectors; s++)
            {
                bool isDown = (down & (1 << s)) != 0, isTell = (tell & (1 << s)) != 0;
                if (!isDown && !isTell) continue;
                double y = isDown ? -a.p.lavaDepth + UnityTuning.Crown32LavaLift : 0.03;
                for (int j = 0; j < 2; j++)
                {
                    double a0 = (s * 2 + j) * segA, a1 = (s * 2 + j + 1) * segA;
                    Vec3 p0 = Crown32Arena.Polar(a0, rIn, y) + o, p1 = Crown32Arena.Polar(a0, rOut, y) + o, p2 = Crown32Arena.Polar(a1, rOut, y) + o, p3 = Crown32Arena.Polar(a1, rIn, y) + o;
                    glowKit.Triangle(U(p0), U(p1), U(p2), Vector3.up, White);
                    glowKit.Triangle(U(p0), U(p2), U(p3), Vector3.up, White);
                    glowKit.Triangle(U(p0), U(p2), U(p1), Vector3.down, White);
                    glowKit.Triangle(U(p0), U(p3), U(p2), Vector3.down, White);
                }
            }
            glowKit.Build(glowMesh.name, glowMesh);
            laidTell = tell;
            SectorsGlowing = 0;
            for (int s = 0; s < a.p.sectors; s++) if (((down | tell) & (1 << s)) != 0) SectorsGlowing++;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            SurvivalSim sim = game.sim;
            Crown32 cr = sim?.crown32;
            bool on = cr != null && sim.n32.boss5 && sim.crown32Arena != null && (cr.Alive || cr.phase == "dying");
            if (!on)
            {
                if (root.gameObject.activeSelf) root.gameObject.SetActive(false);
                PlatesLit = 0; OrbsShown = 0; SectorsGlowing = 0;
                return;
            }
            if (!root.gameObject.activeSelf) root.gameObject.SetActive(true);
            Crown32Arena a = sim.crown32Arena;
            Vec3 o = sim.crown32Origin;
            if (laidArena != a || laidDown != a.DownMask) { LayRing(a, o); laidTell = -1; }
            if (laidTell != cr.dropTellMask || laidDown != a.DownMask) LayGlow(a, o, a.DownMask, cr.dropTellMask);
            Color danger = Look.Hex(UnityTuning.Crown32DangerHex), cyan = Look.C(game.cfg, "colors.usable");
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.PI * 2 * UnityTuning.Crown32TellHz);
            float glowGain = cr.dropTellMask != 0 && a.DownMask == 0 ? Mathf.Lerp(UnityTuning.Crown32TellGainLo, UnityTuning.Crown32TellGainHi, pulse) : UnityTuning.Crown32LavaGain;
            Tint(glowR, danger * glowGain);
            RenderCrown(cr, (float)game.Move54WorldAlpha, danger, cyan); // NIGHT54 M move54: the split bend's world alpha (game.Alpha with the row off)
            RenderTells(cr, a, o, danger);
            RenderOrbs(cr, danger);
        }

        void RenderCrown(Crown32 cr, float alpha, Color danger, Color cyan)
        {
            Vec3 bp = cr.body.previous + (cr.body.position - cr.body.previous) * alpha;
            Vec3 c = new Vec3(bp.x, bp.y + cr.HurtHalf, bp.z);
            Vector3 cu = U(c);
            float h = (float)cr.p.coreHalf;
            Quaternion rot = Quaternion.LookRotation(Dir(cr.yawRad), Vector3.up);
            coreT.SetPositionAndRotation(cu, rot);
            coreT.localScale = Vector3.one * h * 2;
            bandT.SetPositionAndRotation(cu, rot);
            bandT.localScale = new Vector3(h * 2.08f, h * 0.22f, h * 2.08f);
            Tint(bandR, danger * (cr.Hurtable ? 0.9f : 1.4f));
            // the crown: prongs along the top's four edges
            int n = UnityTuning.Crown32Prongs, idx = 0;
            float ps = UnityTuning.Crown32ProngSize;
            for (int s = 0; s < 4; s++)
            {
                Vector3 side = rot * Quaternion.Euler(0, 90 * s, 0) * Vector3.forward, along = Vector3.Cross(Vector3.up, side);
                for (int i = 0; i < n; i++)
                {
                    float f = n == 1 ? 0 : (i / (float)(n - 1) - 0.5f) * 2 * (h - ps * 0.5f);
                    Transform t = prongT[idx++];
                    t.SetPositionAndRotation(cu + side * (h - ps * 0.5f) + along * f + Vector3.up * (h + ps * (i % 2 == 0 ? 0.9f : 0.6f)), rot);
                    t.localScale = new Vector3(ps, ps * (i % 2 == 0 ? 1.8f : 1.2f), ps);
                }
            }
            // the plates
            PlatesLit = 0;
            bool open = cr.PlatesOpen;
            float plateGain = cr.Stunned ? UnityTuning.Crown32PlateGain * (0.75f + 0.25f * Mathf.Sin(Time.time * Mathf.PI * 2 * UnityTuning.Crown32PlateHz)) : UnityTuning.Crown32PlateGain;
            for (int k = 0; k < 3; k++)
            {
                bool shown = k < cr.p.plateCount;
                plateT[k].gameObject.SetActive(shown);
                if (!shown) continue;
                Vec3 nrm = cr.PlateNormal(k);
                Vec3 pc = c + nrm * (cr.p.coreHalf + cr.p.plateDepth);
                plateT[k].SetPositionAndRotation(U(pc), Quaternion.LookRotation(N(nrm), Vector3.up));
                plateT[k].localScale = new Vector3((float)cr.p.plateHalfW * 2, (float)cr.p.plateHalfH * 2, (float)cr.p.plateDepth * 2);
                Tint(plateR[k], open ? cyan * plateGain : Look.Hex(UnityTuning.Crown32PlateShutHex) * 0.35f);
                if (open) PlatesLit++;
            }
        }

        void RenderTells(Crown32 cr, Crown32Arena a, Vec3 o, Color danger)
        {
            // the beam (and its tell: a faint beam where it will start)
            bool beam = cr.Sweeping || cr.SweepTelling;
            beamT.gameObject.SetActive(beam);
            if (beam)
            {
                double ang = cr.Sweeping ? cr.sweepAngle : cr.sweepStartAngle, y = o.y + (cr.p.beamLow + cr.p.beamHigh) * 0.5;
                Vec3 from = new Vec3(cr.Centre.x, y, cr.Centre.z), dir = new Vec3(JsMath.Cos(ang), 0, JsMath.Sin(ang));
                double lx = cr.Centre.x - o.x, lz = cr.Centre.z - o.z, b = lx * dir.x + lz * dir.z, cc = lx * lx + lz * lz - a.p.apothem * a.p.apothem;
                double len = -b + System.Math.Sqrt(System.Math.Max(0, b * b - cc));
                Vector3 p0 = U(from), p1 = U(from + dir * len);
                float thick = cr.Sweeping ? (float)(cr.p.beamHigh - cr.p.beamLow) : UnityTuning.Crown32TellLineWidth * 0.5f;
                Vector3 d = p1 - p0;
                float l = d.magnitude;
                beamT.SetPositionAndRotation((p0 + p1) * 0.5f, l > 1e-4f ? Quaternion.LookRotation(d / l, Vector3.up) : Quaternion.identity);
                beamT.localScale = new Vector3(UnityTuning.Crown32BeamThick, thick, l);
                Tint(beamR, danger * (cr.Sweeping ? 1.2f : UnityTuning.Crown32TellLineGain * (float)(0.3 + 0.7 * cr.ActProgress)));
            }
            // the mortar's mark (a flat red-orange disc that brightens to the landing)
            markT.gameObject.SetActive(cr.mortarPending);
            if (cr.mortarPending)
            {
                double prog = 1.0 - cr.mortarLeft * game.cfg.Dt / System.Math.Max(1e-6, cr.p.mortarDelay);
                Vec3 at = cr.mortarAt;
                markT.SetPositionAndRotation(U(new Vec3(at.x, at.y + UnityTuning.Crown32MarkLift, at.z)), Quaternion.identity);
                float r = (float)cr.p.mortarRadius * 2;
                markT.localScale = new Vector3(r, UnityTuning.Crown32MarkThick, r);
                Tint(markR, danger * (float)(0.35 + 0.9 * System.Math.Max(0, System.Math.Min(1, prog))));
            }
            // the charge line to the wall (through the tell)
            bool line = cr.ChargeTelling;
            lineT.gameObject.SetActive(line);
            if (line)
            {
                Vector3 p0 = U(new Vec3(cr.Centre.x, o.y + UnityTuning.Crown32LineLift, cr.Centre.z)), p1 = U(new Vec3(cr.lineEnd.x, o.y + UnityTuning.Crown32LineLift, cr.lineEnd.z));
                Vector3 d = p1 - p0;
                float l = d.magnitude;
                lineT.SetPositionAndRotation((p0 + p1) * 0.5f, l > 1e-4f ? Quaternion.LookRotation(d / l, Vector3.up) : Quaternion.identity);
                lineT.localScale = new Vector3(UnityTuning.Crown32LineWidth, 0.02f, l);
                Tint(lineR, danger * (float)(0.4 + 0.8 * cr.ActProgress));
            }
        }

        void RenderOrbs(Crown32 cr, Color danger)
        {
            int n = 0;
            foreach (Crown32Orb ob in cr.orbs)
            {
                if (!ob.alive) continue;
                if (n >= orbT.Count)
                {
                    Part(root, "crown orb", add, out Transform t);
                    orbT.Add(t);
                    orbR.Add(t.GetComponent<Renderer>());
                }
                orbT[n].gameObject.SetActive(true);
                orbT[n].SetPositionAndRotation(U(ob.pos), Quaternion.Euler(45, Time.time * 180, 45));
                orbT[n].localScale = Vector3.one * UnityTuning.Crown32OrbSize;
                Tint(orbR[n], danger * 1.3f);
                n++;
            }
            for (int i = n; i < orbT.Count; i++) orbT[i].gameObject.SetActive(false);
            OrbsShown = n;
        }
    }
}
