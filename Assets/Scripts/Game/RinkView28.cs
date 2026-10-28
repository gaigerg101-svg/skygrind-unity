// NIGHT28 (builder A2, design.n28.rink): THE GLACIER's rink as the view draws it. It draws the SAME triangles the sim collides with (Rink28Arena.tris, built from the same config numbers), so the picture
// and the physics cannot disagree: ONE ice material (SKYGRIND/RinkIce28, world-space, no UVs) over the whole floor: lane, banks, lip, island top, door floor; the rim wall, the island's sides and the door walls
// in stone; a cyan strip along the rim on the two straights (the wall-ride lane: player-usable); pale violet-white boundary lines at the bank's foot and round the island's top edge; and a ROOF at the rim's
// top (roof27's dusk-dark violet, an unlit colour) with a skylight slot over the island (design.n27.roof27.skylightShare of the floor area; the collision lid has no hole). One point light over the centre.
// The skate streak lines live in the ice shader and are driven by SkateFx27's globals (_Rink28Dir / _Rink28Scroll / _Rink28Strength), so the streaks cover the whole floor, not a patch round the feet.
// Shown only while the sim has a rink (SurvivalSim.rink28: the glacier room with design.n28.rink on); with the row off nothing here is created. The boss and its attacks are the boss builder's views.
// Not compiled in the Editor by its author (dn-check only).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class RinkView28
    {
        readonly SurvivalGame game;
        readonly Transform root;
        readonly Light light;
        Rink28Arena built;
        Mesh iceMesh, wallMesh, roofMesh, cyanMesh, lineMesh;
        GameObject iceGo, wallGo, roofGo, cyanGo, lineGo;
        Material iceMat, wallMat, roofMat, cyanMat, glowMat, lineMat, duskWallMat, duskRoofMat;
        Mesh glowMesh;
        GameObject glowGo;
        readonly Color wall;

        public bool Shown => root != null && root.gameObject.activeSelf;

        /// <summary>The triangle / strip / line counts of the last build, for the reel line.</summary>
        public int iceTris, wallTris, roofQuads, stripQuads, lineQuads, postCount;

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        public RinkView28(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            root = new GameObject("THE GLACIER rink").transform;
            wall = Look.Hex(UnityTuning.RinkWallHex);
            var lg = new GameObject("Rink light");
            lg.transform.SetParent(root, false);
            light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = (float)cfg.Num("design.n27.boss2.room.lightRange");
            light.intensity = (float)cfg.Num("design.n27.boss2.room.lightIntensity") * UnityTuning.LightScale;
            light.color = Look.Hex((uint)cfg.Num("design.n27.boss2.room.lightColor"));
            root.gameObject.SetActive(false);
        }

        void Rebuild(Rink28Arena a)
        {
            built = a;
            GameConfig cfg = game.cfg;
            Color white = Color.white;
            bool dusk29 = game.sim != null && N29Fixes.BossViewOn && game.sim.n29.boss2; // NIGHT29: dark dusk walls and roof (GG: pale lavender / white)
            Color wallCol = dusk29 ? Look.Hex(UnityTuning.Rink29WallHex) : wall;
            // --- the meshes ---
            var ice = new MeshKit();
            var stone = new MeshKit();
            iceTris = wallTris = 0;
            foreach (Rink28Tri t in a.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
                var un = new Vector3((float)n.x, (float)n.y, (float)-n.z);
                switch (t.part)
                {
                    case Rink28Part.Floor: case Rink28Part.Bank: case Rink28Part.Lip: case Rink28Part.IslandTop: case Rink28Part.DoorFloor:
                        float tint = t.part == Rink28Part.Bank ? UnityTuning.RinkBankTint : t.part == Rink28Part.IslandTop ? UnityTuning.RinkIslandTint : UnityTuning.RinkLaneTint;
                        ice.Triangle(U(t.a), U(t.b), U(t.c), un, new Color(tint, tint, tint, 1));
                        iceTris++;
                        break;
                    case Rink28Part.Lid:
                        break; // the roof below draws the ceiling (with its skylight)
                    default:
                        stone.Triangle(U(t.a), U(t.b), U(t.c), un, wallCol);
                        wallTris++;
                        break;
                }
            }
            // --- the roof: a ring from the rim to an opening over the island, at the rim's top, facing down ---
            var roof = new MeshKit();
            double share = cfg.Num("design.n27.roof27.skylightShare");
            double R = a.p.centreRadius + a.RimLat, L = a.p.straight;
            double floorArea = 2 * R * L + System.Math.PI * R * R;
            double open = (-2 * L + System.Math.Sqrt(4 * L * L + 4 * System.Math.PI * share * floorArea)) / (2 * System.Math.PI); // the opening's cap radius: stadium area L * 2r + pi r^2 = share x floor area
            double latOpen = open - a.p.centreRadius;
            Color roofCol = dusk29 ? Look.Hex(UnityTuning.Rink29RoofHex) : Look.Hex((uint)cfg.Num("design.n27.roof27.ceilingHex"));
            roofQuads = 0;
            int n0 = a.stations.Count;
            for (int i = 0; i < n0; i++)
            {
                double s0 = a.stations[i], s1 = i + 1 < n0 ? a.stations[i + 1] : a.Lap;
                Vec3 o0 = Pt(a, s0, a.RimLat, a.rimTopY), o1 = Pt(a, s1, a.RimLat, a.rimTopY), i0 = Pt(a, s0, latOpen, a.rimTopY), i1 = Pt(a, s1, latOpen, a.rimTopY);
                roof.Quad(U(o0), U(o1), U(i1), U(i0), Vector3.down, roofCol);
                roofQuads++;
            }
            // --- the cyan wall-ride strip on the two straights, and the boundary lines ---
            var cyan = new MeshKit();
            var glow = new MeshKit();
            var line = new MeshKit();
            Color usable = Look.C(cfg, "colors.usable", UnityTuning.RinkStripGain), boundary = Look.C(cfg, "barrier.boundaryColor", UnityTuning.RinkLineGain);
            stripQuads = lineQuads = postCount = 0;
            double y0 = a.bankTopY + UnityTuning.RinkStripHeight, y1 = y0 + UnityTuning.RinkStripThick;
            double gy0 = y0 + UnityTuning.RinkStripThick * 0.5 - UnityTuning.RinkGlowHeight * 0.5, gy1 = gy0 + UnityTuning.RinkGlowHeight;
            double off = a.RimLat - UnityTuning.RinkStripOff;
            double hw = UnityTuning.RinkLineWidth * 0.5, lift = UnityTuning.RinkLineLift, cap = a.math.CapLength;
            for (int i = 0; i < n0; i++)
            {
                double s0 = a.stations[i], s1 = i + 1 < n0 ? a.stations[i + 1] : a.Lap;
                double mid = (s0 + s1) * 0.5;
                bool straight = mid < L || (mid > L + cap && mid < 2 * L + cap);
                if (straight)
                {
                    cyan.Quad(U(Pt(a, s0, off, y0)), U(Pt(a, s1, off, y0)), U(Pt(a, s1, off, y1)), U(Pt(a, s0, off, y1)), Vector3.up, white);
                    glow.Quad(U(Pt(a, s0, off - 0.01, gy0)), U(Pt(a, s1, off - 0.01, gy0)), U(Pt(a, s1, off - 0.01, gy1)), U(Pt(a, s0, off - 0.01, gy1)), Vector3.up, white);
                    stripQuads++;
                }
                // the bank's foot (flat lane side) and the island's top edge (not across a door)
                line.Quad(U(Pt(a, s0, a.LaneOutLat - hw * 2, a.floorY + lift)), U(Pt(a, s1, a.LaneOutLat - hw * 2, a.floorY + lift)), U(Pt(a, s1, a.LaneOutLat, a.floorY + lift)), U(Pt(a, s0, a.LaneOutLat, a.floorY + lift)), Vector3.up, white);
                lineQuads++;
                bool door = System.Math.Abs(mid - L / 2) < a.p.doorWidth / 2 + 1e-6 || System.Math.Abs(mid - (L + cap + L / 2)) < a.p.doorWidth / 2 + 1e-6;
                if (!door)
                {
                    line.Quad(U(Pt(a, s0, a.latIsland - hw * 2, a.islandTopY + lift)), U(Pt(a, s1, a.latIsland - hw * 2, a.islandTopY + lift)), U(Pt(a, s1, a.latIsland, a.islandTopY + lift)), U(Pt(a, s0, a.latIsland, a.islandTopY + lift)), Vector3.up, white);
                    lineQuads++;
                }
            }
            // the door posts: a pale violet-white post at each of the four island corners of each door

            double pz = UnityTuning.RinkPostSize;
            foreach (double sd in new[] { L / 2 - a.p.doorWidth / 2, L / 2 + a.p.doorWidth / 2, L + cap + L / 2 - a.p.doorWidth / 2, L + cap + L / 2 + a.p.doorWidth / 2 })
            {
                Vec3 pt = Pt(a, sd, a.latIsland, a.islandTopY + UnityTuning.RinkPostHeight * 0.5);
                line.Cube(U(pt), new Vector3((float)pz, UnityTuning.RinkPostHeight, (float)pz), white);
                postCount++;
            }
            DestroyOld();
            iceMesh = ice.Build("rink ice"); wallMesh = stone.Build("rink walls"); glowMesh = glow.Build("rink strip glow"); roofMesh = roof.Build("rink roof"); cyanMesh = cyan.Build("rink strip"); lineMesh = line.Build("rink lines");
            // --- the materials ---
            Shader iceShader = Shader.Find("SKYGRIND/RinkIce28");
            if (iceMat == null)
            {
                iceMat = iceShader != null ? new Material(iceShader) : FxDraw.Opaque(game.mats.neon, "Rink ice (shader missing)", Look.Hex(UnityTuning.RinkIceLitHex));
                if (iceShader != null)
                {
                    iceMat.SetColor("_DeepColor", Look.Hex(UnityTuning.RinkIceDeepHex));
                    iceMat.SetColor("_BaseColor", Look.Hex(UnityTuning.RinkIceLitHex));
                    iceMat.SetColor("_Sheen", Look.C(cfg, "barrier.boundaryColor"));
                    iceMat.SetFloat("_LinesPerM", (float)cfg.Num("design.n28.rink.streakLinesPerM"));
                    iceMat.SetFloat("_Width", (float)cfg.Num("design.n27.skate27.streakWidth"));
                    iceMat.SetFloat("_Gain", (float)cfg.Num("design.n27.skate27.streakGain"));
                    iceMat.SetFloat("_ScratchGain", (float)cfg.Num("design.n28.rink.scratchGain"));
                }
                Shader wallShader = Shader.Find("SKYGRIND/RinkWall28");
                if (wallShader != null)
                {
                    wallMat = new Material(wallShader);
                    wallMat.SetColor("_GridColor", Look.C(cfg, "barrier.boundaryColor"));
                }

                else wallMat = game.mats.stone;
                roofMat = FxDraw.Opaque(game.mats.neon, "Rink roof", Color.white);
                cyanMat = FxDraw.Additive(game.mats.neon, "Rink strip");
                cyanMat.SetColor("_BaseColor", usable);
                glowMat = FxDraw.Additive(game.mats.neon, "Rink strip glow");
                glowMat.SetColor("_BaseColor", Look.C(cfg, "colors.usable", UnityTuning.RinkGlowGain));
                lineMat = FxDraw.Additive(game.mats.neon, "Rink lines");
                lineMat.SetColor("_BaseColor", boundary);
            }
            if (wallMat != null && wallMat.HasProperty("_GridGain")) wallMat.SetFloat("_GridGain", dusk29 ? UnityTuning.Rink29WallGrid : 0.28f); // RinkWall28's default gain
            // NIGHT29 round 5 (Player frames n29-5): the walls were the STONE shader's lit lavender (RinkWall28 was not what the frame drew) and the roof a pure WHITE slab (URP Unlit ignores the vertex colour, so
            // "Rink roof" with _BaseColor white drew white whatever roofCol said). With the reworked Glacier on, the walls and roof are flat unlit dusk-dark violets set through _BaseColor, and the ice a deeper blue.
            if (dusk29)
            {
                if (duskWallMat == null) duskWallMat = FxDraw.Opaque(game.mats.neon, "Rink wall dusk29", Look.Hex(UnityTuning.Rink29WallHex));
                if (duskRoofMat == null) duskRoofMat = FxDraw.Opaque(game.mats.neon, "Rink roof dusk29", Look.Hex(UnityTuning.Rink29RoofHex));
                if (iceMat != null && iceMat.HasProperty("_DeepColor")) { iceMat.SetColor("_DeepColor", Look.Hex(UnityTuning.Rink29IceDeepHex)); iceMat.SetColor("_BaseColor", Look.Hex(UnityTuning.Rink29IceLitHex)); iceMat.SetColor("_Sheen", Look.Hex(UnityTuning.Rink29IceSheenHex)); }
            }
            else if (iceMat != null && iceMat.HasProperty("_DeepColor")) { iceMat.SetColor("_DeepColor", Look.Hex(UnityTuning.RinkIceDeepHex)); iceMat.SetColor("_BaseColor", Look.Hex(UnityTuning.RinkIceLitHex)); iceMat.SetColor("_Sheen", Look.C(cfg, "barrier.boundaryColor")); }
            iceGo = MeshKit.Object("ice", root, iceMesh, iceMat, shadows: false);
            wallGo = MeshKit.Object("walls", root, wallMesh, dusk29 ? duskWallMat : wallMat, shadows: false);
            glowGo = MeshKit.Object("wall-ride strip glow", root, glowMesh, glowMat, shadows: false);
            roofGo = MeshKit.Object("roof", root, roofMesh, dusk29 ? duskRoofMat : roofMat, shadows: false);
            cyanGo = MeshKit.Object("wall-ride strip", root, cyanMesh, cyanMat, shadows: false);
            lineGo = MeshKit.Object("boundary lines", root, lineMesh, lineMat, shadows: false);
            Vec3 c = new Vec3(a.p.cx, a.rimTopY * UnityTuning.RinkLightHeightShare, a.p.cz);
            light.transform.position = U(c);
        }

        static Vec3 Pt(Rink28Arena a, double s, double lat, double y)
        {
            a.math.PointAt(s, lat, out double x, out double z, out _, out _);
            return new Vec3(x, y, z);
        }

        void DestroyOld()
        {
            foreach (GameObject g in new[] { iceGo, wallGo, roofGo, cyanGo, glowGo, lineGo }) if (g != null) Object.Destroy(g);
            foreach (Mesh m in new[] { iceMesh, wallMesh, roofMesh, cyanMesh, glowMesh, lineMesh }) if (m != null) Object.Destroy(m);
        }

        public void Render(double alpha)
        {
            Rink28Arena a = game.sim?.rink28 ?? (game.sim?.glacier28 != null && game.sim.glacier28.Alive ? game.sim.glacier28Rink : null) ?? (game.sim?.stag31 != null && game.sim.stag31.Alive ? game.sim.stag31Rink : null); // NIGHT31 B: the Stag's rink too; NIGHT28 B2: the lap fight skates on glacier28Rink
            bool on = a != null;
            if (on && a != built) Rebuild(a);
            if (!on) built = null;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
        }
    }
}
