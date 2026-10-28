// NIGHT26 builder B (contract hulk): THE SLAG HULK as the view draws it: the sim's oriented boxes as they are (Core/Enemies/Hulk26.cs: a shot finds what you see),
// interpolated on the sim's render alpha; the head and the back plate carry a pale violet-white glow plate (the weak points, rule 8: the boundary tone), brighter
// while it is stunned; the slam's shockwave rings are rings of pooled red-orange boxes (danger) at the ring's radius, 1 m tall: the jump height reads on the
// screen. The tells' circle and line are the sim's kit.telegraph calls (Telegraph20), wired here as WyrmView does. Hooks (the lead's SurvivalGame.Wyrm24.cs):
//   Wyrm24InitView:   hulkView = new HulkView(this);      Wyrm24RenderView: hulkView?.Render(alpha);     field: public HulkView hulkView;
// Numbers: UnityTuning.Hulk26.cs. Not compiled in the Editor by its author (dn-check only).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class HulkView
    {
        readonly SurvivalGame game;
        readonly Transform root;
        readonly Transform[] baseT, glowT, ringT;
        readonly Renderer[] baseR, glowR, ringR;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color body, head, weak, ring, white;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool telegraphWired;

        /// <summary>The hulk is drawn now.</summary>
        public bool Shown => root != null && root.gameObject.activeSelf;

        public HulkView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            body = Look.Hex(UnityTuning.HulkBodyHex);
            head = Look.Hex(UnityTuning.HulkHeadHex);
            weak = Look.C(cfg, "barrier.boundaryColor", UnityTuning.HulkWeakGain);
            ring = Look.Hex(UnityTuning.HulkRingHex, UnityTuning.HulkRingGain);
            white = Look.Hex(0xffffff);
            int parts = HulkPartCount(cfg);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            Material solid = FxDraw.Opaque(game.mats.neon, "Hulk body", Color.white);
            Material add = FxDraw.Additive(game.mats.neon, "Hulk glow");
            root = new GameObject("SLAG HULK").transform;
            baseT = new Transform[parts];
            baseR = new Renderer[parts];
            glowT = new Transform[parts];
            glowR = new Renderer[parts];
            for (int i = 0; i < parts; i++)
            {
                baseR[i] = Part(root, "part " + i, box, solid, out baseT[i]);
                glowR[i] = Part(root, "glow " + i, box, add, out glowT[i]);
            }
            int rb = UnityTuning.HulkRingBoxes * UnityTuning.HulkRingMax;
            ringT = new Transform[rb];
            ringR = new Renderer[rb];
            for (int i = 0; i < rb; i++) ringR[i] = Part(root, "ring " + i, box, add, out ringT[i]);
            root.gameObject.SetActive(false);
            game.events.On<BossEvent>("bossDying", _ => game.BossDyingSeen());
            game.events.On<BossEvent>("bossDown", e =>
            {
                if (e.boss == "hulk") game.ParticleBurst(e.position, white, UnityTuning.HulkDeathBits, UnityTuning.HulkDeathSpeed, UnityTuning.HulkDeathLife, UnityTuning.HulkDeathSize);
            });
        }

        /// <summary>The number of boxes in the hulk's body file (read once; the file is beside PortExport).</summary>
        static int HulkPartCount(GameConfig cfg)
        {
            string dir = SurvivalSim.Wyrm24Dir;
            if (string.IsNullOrEmpty(dir)) return 8;
            string path = System.IO.Path.Combine(dir, cfg.Str("design.n26.bosses.hulk.paths"));
            return System.IO.File.Exists(path) ? PortData.Arr(Hulk.LoadBoss(path)["parts"]).Count : 8;
        }

        static Renderer Part(Transform parent, string name, Mesh mesh, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            Hulk h = sim?.hulk26;
            if (!telegraphWired && sim?.bossKit != null && (object)game.Telegraphs is ITelegraph tg)
            {
                sim.bossKit.telegraph = tg;
                telegraphWired = true;
            }
            bool on = h != null && h.Alive;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on) return;
            float a = Mathf.Clamp01((float)alpha);
            int n = Mathf.Min(h.PartCount, baseT.Length);
            for (int i = 0; i < n; i++)
            {
                WyrmBox b = h.boxes[i];
                string part = h.partOf[i];
                Vector3 c = Vector3.Lerp(U(h.prevCenters[i]), U(b.c), a);
                Quaternion q = Quaternion.LookRotation(U(b.f), U(b.u));
                baseT[i].SetPositionAndRotation(c, q);
                baseT[i].localScale = new Vector3((float)b.hr * 2, (float)b.hu * 2, (float)b.hf * 2);
                Tint(baseR[i], part == "head" ? head : body);
                bool weakPart = part == "head" || part == "back";
                if (glowT[i].gameObject.activeSelf != weakPart) glowT[i].gameObject.SetActive(weakPart);
                if (!weakPart) continue;
                // a thin plate on the weak point's outward face: the head's top, the back plate's rear
                Vector3 off = part == "head" ? U(b.u) * ((float)b.hu + UnityTuning.HulkWeakThick * 0.5f) : -U(b.f) * ((float)b.hf + UnityTuning.HulkWeakThick * 0.5f);
                glowT[i].SetPositionAndRotation(c + off, q);
                glowT[i].localScale = part == "head"
                    ? new Vector3((float)b.hr * 2 * 0.7f, UnityTuning.HulkWeakThick, (float)b.hf * 2 * 0.7f)
                    : new Vector3((float)b.hr * 2 * 0.8f, (float)b.hu * 2 * 0.8f, UnityTuning.HulkWeakThick);
                Tint(glowR[i], weak * (h.Stunned ? UnityTuning.HulkStunGain / UnityTuning.HulkWeakGain : 1f));
            }
            // the shockwave rings: pooled boxes around the circle, 1 m tall
            Vec3 feet = h.FeetWorld;
            for (int rI = 0; rI < UnityTuning.HulkRingMax; rI++)
            {
                bool live = rI < h.rings.Count;
                for (int k = 0; k < UnityTuning.HulkRingBoxes; k++)
                {
                    int idx = rI * UnityTuning.HulkRingBoxes + k;
                    if (ringT[idx].gameObject.activeSelf != live) ringT[idx].gameObject.SetActive(live);
                    if (!live) continue;
                    double r = h.rings[rI].r, ang = k * (2 * System.Math.PI / UnityTuning.HulkRingBoxes);
                    Vec3 p = new Vec3(feet.x + System.Math.Sin(ang) * r, feet.y + UnityTuning.HulkRingHeight * 0.5, feet.z + System.Math.Cos(ang) * r);
                    ringT[idx].SetPositionAndRotation(U(p), Quaternion.Euler(0, -(float)(ang * Mathf.Rad2Deg), 0));
                    float arc = (float)(2 * System.Math.PI * r / UnityTuning.HulkRingBoxes) * 1.15f;
                    ringT[idx].localScale = new Vector3(arc, UnityTuning.HulkRingHeight, UnityTuning.HulkRingWidth);
                    Tint(ringR[idx], ring);
                }
            }
        }
    }
}
