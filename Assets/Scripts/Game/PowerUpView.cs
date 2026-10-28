// NIGHT8 U2-12: the power-up obelisks as powerUps.js draws them: an octahedron crystal (radius 0.5 scaled 0.7 x 1.6 x
// 0.7) in the type's colour x2.2 hovering powerUps.hover m up (bobbing 0.15 m at 2.4 rad/s) and turning (1.8 rad/s), a
// floor ring (0.7..0.85 m, pulsing x(1 +- 0.12) at 4 rad/s) and a thin beam (powerUps.beamHeight) in the colour x1.6;
// it blinks out over its last powerUps.blinkFrom s. Not ported: the copy drawn through walls (GreaterDepth, wallGlow).
// NIGHT17 U3-30.10 (beyond the ask, view only): within UnityTuning.PickupPullRange m outside the sim's grabRadius the
// crystal leans toward your chest (ease-in, up to PickupPullShare of the way), and the frame the sim takes it the crystal
// flashes at you (scale x PickupFlashScale, x PickupFlashGain bright, gone over PickupFlashTime s). The grab itself stays
// the sim's (powerUps.grabRadius).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class PowerUpView
    {
        readonly SurvivalGame game;
        readonly Transform[] crystals, rings, beams;
        readonly Renderer[][] rends;
        readonly string[] shownType;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly float hover;
        readonly int blinkSteps;
        readonly double grabRadius; // the sim's (powerUps.grabRadius): the pull starts PickupPullRange outside it
        readonly double[] lastDist, flashLeft; // NIGHT17: the distance last frame (a take vs an expiry), the flash's time left
        readonly bool[] wasOn;
        readonly Color[] typeColor; // the drop's colour, cached at its spawn's tint
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public PowerUpView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            hover = (float)cfg.Num("powerUps.hover");
            blinkSteps = cfg.Steps("powerUps.blinkFrom");
            grabRadius = cfg.Num("powerUps.grabRadius");
            float beamH = (float)cfg.Num("powerUps.beamHeight");
            int n = game.powerUps.drops.Length;
            crystals = new Transform[n];
            rings = new Transform[n];
            beams = new Transform[n];
            rends = new Renderer[n][];
            shownType = new string[n];
            lastDist = new double[n];
            flashLeft = new double[n];
            wasOn = new bool[n];
            typeColor = new Color[n];
            var parent = new GameObject("Power-ups").transform;
            Mesh oct = Octahedron();
            var mat = new Material(game.mats.neon);
            for (int i = 0; i < n; i++)
            {
                var c = new GameObject("crystal", typeof(MeshFilter), typeof(MeshRenderer));
                c.transform.SetParent(parent, false);
                c.GetComponent<MeshFilter>().sharedMesh = oct;
                var cr = c.GetComponent<MeshRenderer>();
                cr.sharedMaterial = mat;
                crystals[i] = c.transform;
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.Destroy(ring.GetComponent<Collider>());
                ring.transform.SetParent(parent, false);
                ring.transform.localScale = new Vector3(1.7f, 0.004f, 1.7f); // 0.85 m radius, a thin disc
                var rr = ring.GetComponent<Renderer>();
                rr.sharedMaterial = mat;
                rr.shadowCastingMode = ShadowCastingMode.Off;
                rings[i] = ring.transform;
                var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.Destroy(beam.GetComponent<Collider>());
                beam.transform.SetParent(parent, false);
                beam.transform.localScale = new Vector3(0.16f, beamH / 2, 0.16f);
                var br = beam.GetComponent<Renderer>();
                br.sharedMaterial = mat;
                br.shadowCastingMode = ShadowCastingMode.Off;
                beams[i] = beam.transform;
                rends[i] = new[] { cr, rr, br };
                c.SetActive(false);
                ring.SetActive(false);
                beam.SetActive(false);
            }
        }

        static Mesh Octahedron()
        {
            var v = new[]
            {
                new Vector3(0, 0.8f, 0), new Vector3(0, -0.8f, 0), new Vector3(0.35f, 0, 0), new Vector3(-0.35f, 0, 0), new Vector3(0, 0, 0.35f), new Vector3(0, 0, -0.35f),
            };
            int[] faces = { 0, 4, 2, 0, 2, 5, 0, 5, 3, 0, 3, 4, 1, 2, 4, 1, 5, 2, 1, 3, 5, 1, 4, 3 };
            var verts = new Vector3[faces.Length];
            var tris = new int[faces.Length];
            for (int i = 0; i < faces.Length; i++)
            {
                verts[i] = v[faces[i]];
                tris[i] = i;
            }
            var m = new Mesh { name = "Octahedron", vertices = verts, triangles = tris };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        /// <summary>NIGHT17: where a pickup is pulled to and flashes: in front of the camera, a little low (in view).</summary>
        Vector3 CatchPoint()
        {
            Transform c = game.cam != null ? game.cam.transform : null;
            if (c == null) return WebSpace.ToUnity(game.Feet) + new Vector3(0, 1.1f, 0);
            return c.position + c.forward * UnityTuning.PickupCatchAhead - c.up * UnityTuning.PickupCatchDrop;
        }

        void Tint(Renderer r, Color c)
        {
            c.a = 1;
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        public void Render(double frameDt)
        {
            PowerUps.Drop[] drops = game.powerUps.drops;
            Vec3 feet = game.Feet;
            for (int i = 0; i < drops.Length; i++)
            {
                PowerUps.Drop d = drops[i];
                // NIGHT17: the frame the sim takes it (it went inactive while you stood in reach): the collection flash.
                if (wasOn[i] && !d.active && lastDist[i] <= grabRadius + UnityTuning.PickupTakeSlack) flashLeft[i] = UnityTuning.PickupFlashTime;
                wasOn[i] = d.active;
                if (flashLeft[i] > 0)
                {
                    flashLeft[i] -= frameDt;
                    float k = Mathf.Clamp01((float)(flashLeft[i] / UnityTuning.PickupFlashTime)); // 1 -> 0
                    bool show = flashLeft[i] > 0;
                    if (crystals[i].gameObject.activeSelf != show) crystals[i].gameObject.SetActive(show);
                    if (rings[i].gameObject.activeSelf) rings[i].gameObject.SetActive(false);
                    if (beams[i].gameObject.activeSelf) beams[i].gameObject.SetActive(false);
                    if (show)
                    {
                        crystals[i].position = CatchPoint();
                        float sc = UnityTuning.PickupFlashScale * k; // shrinks into you as it fades
                        crystals[i].localScale = new Vector3(sc, sc, sc);
                        if (shownType[i] != null) Tint(rends[i][0], typeColor[i] * (float)(2.2 * (1 + (UnityTuning.PickupFlashGain - 1) * k)));
                    }
                    else
                    {
                        crystals[i].localScale = Vector3.one;
                        shownType[i] = null; // re-tinted at its next spawn
                    }
                    continue;
                }
                bool on = d.active && (d.steps > blinkSteps || Mathf.Sin((float)d.age * 20) > 0);
                if (crystals[i].gameObject.activeSelf != on)
                {
                    crystals[i].gameObject.SetActive(on);
                    rings[i].gameObject.SetActive(on);
                    beams[i].gameObject.SetActive(on);
                }
                if (on && !ReferenceEquals(d.type, shownType[i]))
                {
                    shownType[i] = d.type;
                    Color c = Look.C(game.cfg, $"powerUps.types.{d.type}.color");
                    typeColor[i] = c;
                    Tint(rends[i][0], c * 2.2f);
                    Tint(rends[i][1], c * 1.6f);
                    Tint(rends[i][2], c * 1.6f);
                }
                if (!d.active) continue;
                d.age += frameDt;
                float a = (float)d.age;
                Vector3 p = WebSpace.ToUnity(d.position);
                double dist = JsMath.Hypot(feet.x - d.position.x, feet.z - d.position.z);
                lastDist[i] = dist;
                // NIGHT17: the last metre's pull toward the chest (ease-in), view only
                float pull = (float)(1 - (dist - grabRadius) / UnityTuning.PickupPullRange);
                pull = Mathf.Clamp01(pull);
                pull = pull * pull * UnityTuning.PickupPullShare;
                crystals[i].position = Vector3.Lerp(p + new Vector3(0, hover + Mathf.Sin(a * 2.4f) * 0.15f, 0), CatchPoint(), pull);
                crystals[i].rotation = Quaternion.Euler(0, -a * 1.8f * Mathf.Rad2Deg, 0);
                rings[i].position = p + new Vector3(0, 0.05f, 0);
                float s = 1 + 0.12f * Mathf.Sin(a * 4);
                rings[i].localScale = new Vector3(1.7f * s, 0.004f, 1.7f * s);
                beams[i].position = p + new Vector3(0, beams[i].localScale.y, 0);
            }
        }
    }
}
