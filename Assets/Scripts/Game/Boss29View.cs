// NIGHT29 builder B: what the player SEES of the reworked bosses (design.n29.boss1 / boss2). A new view next to LanternView / RinkView28 / GlacierView28 (they are untouched):
//  - THE GLACIER: the three SKATE LANE lines drawn as thin glowing cyan lines on the ice (the player-usable colour; the line the player is locked to burns brighter); a FROST TRAIL as a visible slow field on the
//    ice (a red-orange pulse while it tells, a dusk-blue field while it stands: it slows you);
//  - THE LANTERN: while the floor is live the whole floor glows red-orange (danger) with a red-orange light; in the tell the glow pulses up so the floor warning is seen before it hurts. No sound.
// Shown only with the run's n29.boss1 / n29.boss2 block on and N29Fixes' switch (-skygrindOff29 turns it off). One call from SurvivalGame.Boss26.cs. Not compiled in the Editor by its author (dn-check only).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Boss29View
    {
        readonly SurvivalGame game;
        readonly Transform root;
        readonly Material add;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        readonly Mesh cube;
        // the lane lines
        Rink28Arena builtFor;
        GameObject[] laneGo;
        MeshRenderer[] laneR;
        Mesh[] laneMesh;
        int builtCount;
        // frost fields (pooled cubes)
        readonly Transform[] frostT = new Transform[UnityTuning.Frost29Max];
        readonly Renderer[] frostR = new Renderer[UnityTuning.Frost29Max];
        // the lantern's lava floor
        readonly Transform lavaT;
        readonly Renderer lavaR;
        readonly Light lavaLight;
        readonly Color lane, frost, lava;

        /// <summary>For the reel lines: the lava floor is drawn now and its gain, the lane lines built, the frost fields drawn.</summary>
        public bool LavaShown { get; private set; }
        public float LavaGain { get; private set; }
        public int LaneLinesBuilt => laneGo == null ? 0 : laneGo.Length;
        public int FrostShown { get; private set; }

        public Boss29View(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            root = new GameObject("NIGHT29 boss view").transform;
            add = FxDraw.Additive(game.mats.neon, "Boss29 glow");
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube = c.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(c);
            lane = Look.C(cfg, "colors.usable");
            frost = Look.Hex(UnityTuning.Frost29Hex);
            lava = Look.Hex(UnityTuning.Lava29Hex);
            for (int i = 0; i < frostT.Length; i++) frostR[i] = Part("frost " + i, out frostT[i]);
            lavaR = Part("lava floor", out lavaT);
            var lg = new GameObject("Lava light");
            lg.transform.SetParent(root, false);
            lavaLight = lg.AddComponent<Light>();
            lavaLight.type = LightType.Point;
            lavaLight.shadows = LightShadows.None;
            lavaLight.color = lava;
            lavaLight.range = UnityTuning.Lava29LightRange;
            lavaLight.enabled = false;
            root.gameObject.SetActive(true);
        }

        MeshRenderer Part(string name, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = cube;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            g.SetActive(false);
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        static Vec3 Pt(Rink28Arena a, double s, double lat, double y)
        {
            a.math.PointAt(s, lat, out double x, out double z, out _, out _);
            return new Vec3(x, y, z);
        }

        void DestroyLanes()
        {
            if (laneGo != null) foreach (GameObject g in laneGo) if (g != null) Object.Destroy(g);
            if (laneMesh != null) foreach (Mesh m in laneMesh) if (m != null) Object.Destroy(m);
            laneGo = null;
            laneMesh = null;
            laneR = null;
        }

        /// <summary>One ribbon per lane line along the whole lap (the flat lane), 1 m segments.</summary>
        void BuildLanes(Rink28Arena a, SkateLane29 sl)
        {
            DestroyLanes();
            builtFor = a;
            builtCount = sl.count;
            laneGo = new GameObject[sl.count];
            laneMesh = new Mesh[sl.count];
            laneR = new MeshRenderer[sl.count];
            double hw = UnityTuning.Lane29Width * 0.5, y = a.floorY + UnityTuning.Lane29Lift;
            int n = (int)System.Math.Ceiling(a.Lap / UnityTuning.Lane29Step);
            for (int k = 0; k < sl.count; k++)
            {
                var kit = new MeshKit();
                double lat = sl.LineLat(k);
                for (int i = 0; i < n; i++)
                {
                    double s0 = i * a.Lap / n, s1 = (i + 1) * a.Lap / n;
                    kit.Quad(U(Pt(a, s0, lat - hw, y)), U(Pt(a, s1, lat - hw, y)), U(Pt(a, s1, lat + hw, y)), U(Pt(a, s0, lat + hw, y)), Vector3.up, Color.white);
                }
                laneMesh[k] = kit.Build("lane line " + k);
                laneGo[k] = MeshKit.Object("lane line " + k, root, laneMesh[k], add, shadows: false);
                laneR[k] = laneGo[k].GetComponent<MeshRenderer>();
            }
        }

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            bool lanternOn = sim != null && N29Fixes.BossViewOn && sim.n29.boss1 && sim.lantern29 != null && sim.lantern27 != null && sim.lantern27.Alive;
            bool glacierOn = sim != null && N29Fixes.BossViewOn && sim.n29.boss2 && sim.glacier29 != null && sim.glacier28 != null && sim.glacier28.Alive && sim.glacier28Rink != null;
            RenderLantern(sim, lanternOn);
            RenderGlacier(sim, glacierOn);
        }

        void RenderLantern(SurvivalSim sim, bool on)
        {
            float gain = 0;
            if (on)
            {
                Lantern29 l29 = sim.lantern29;
                if (l29.Stage == Lantern29Stage.Tell && Light54.On(game.cfg))
                {
                    // NIGHT54 light54 (fix 10): the 22 rad/s (3.5 Hz) +-35 % pulse on the floor and its 30 m light -> Light54.Calm's swell
                    double hz = UnityTuning.Lava29TellPulse / (2 * System.Math.PI), d = 0.35;
                    Light54.Calm(game.cfg, ref hz, ref d);
                    gain = UnityTuning.Lava29TellGain * (0.25f + 0.75f * (float)l29.TellProgress) * (1f - (float)d + (float)d * Mathf.Sin(Time.time * (float)(hz * 2 * System.Math.PI)));
                }
                else if (l29.Stage == Lantern29Stage.Tell) gain = UnityTuning.Lava29TellGain * (0.25f + 0.75f * (float)l29.TellProgress) * (0.65f + 0.35f * Mathf.Sin(Time.time * UnityTuning.Lava29TellPulse));
                else if (l29.Stage == Lantern29Stage.Live) gain = UnityTuning.Lava29LiveGain * (0.9f + 0.1f * Mathf.Sin(Time.time * 7f));
            }
            bool show = on && gain > 0.001f;
            LavaShown = show;
            LavaGain = show ? gain : 0;
            // NIGHT30 (builder V; design.n30.look30 or the run's lantern30): the full-floor additive quad and its 30 m red-orange light washed walls, roof and the
            // HUD's backings salmon (shot 170227). LavaView30 draws the live floor as a lava surface over the floor only, with no light: this one stands down.
            if (show && LavaView30.LanternLookOn(game.cfg, sim)) show = false;
            if (lavaT.gameObject.activeSelf != show) lavaT.gameObject.SetActive(show);
            lavaLight.enabled = show;
            if (!show) return;
            Vec3 o = sim.lantern27Origin;
            float half = (float)sim.lantern27.p.arenaHalf * 2f;
            lavaT.position = U(new Vec3(o.x, o.y + UnityTuning.Lava29Lift, o.z));
            lavaT.localScale = new Vector3(half, 0.02f, half);
            Tint(lavaR, lava * gain);
            lavaLight.transform.position = U(new Vec3(o.x, o.y + 3, o.z));
            lavaLight.intensity = UnityTuning.Lava29LightIntensity * UnityTuning.LightScale * Mathf.Clamp01(gain);
        }

        // the dusk fog / ambient of the rink while the fight runs (the reel frame showed the whole view a flat lavender: the stone shader's sky-fog globals and RenderSettings.fogColor, both lavender, swallowed the walls)
        bool fogged;
        Color prevFogColor, prevAmbSky, prevAmbEq, prevAmbGround, prevFogZ, prevFogM, prevFogH;
        UnityEngine.Rendering.AmbientMode prevAmbMode;
        static readonly int FogZ = Shader.PropertyToID("_SkyFogZenith"), FogM = Shader.PropertyToID("_SkyFogMid"), FogH = Shader.PropertyToID("_SkyFogHorizon");

        void FogDusk(bool on)
        {
            if (on)
            {
                if (!fogged)
                {
                    fogged = true;
                    prevFogColor = RenderSettings.fogColor; prevAmbMode = RenderSettings.ambientMode;
                    prevAmbSky = RenderSettings.ambientSkyColor; prevAmbEq = RenderSettings.ambientEquatorColor; prevAmbGround = RenderSettings.ambientGroundColor;
                    prevFogZ = Shader.GetGlobalColor(FogZ); prevFogM = Shader.GetGlobalColor(FogM); prevFogH = Shader.GetGlobalColor(FogH);
                }
                Color f = Look.Hex(UnityTuning.Fog29Hex);
                RenderSettings.fogColor = f;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = Look.Hex(UnityTuning.Fog29AmbSkyHex);
                RenderSettings.ambientEquatorColor = Look.Hex(UnityTuning.Fog29AmbEqHex);
                RenderSettings.ambientGroundColor = Look.Hex(UnityTuning.Fog29AmbGroundHex);
                Shader.SetGlobalColor(FogZ, f); Shader.SetGlobalColor(FogM, f); Shader.SetGlobalColor(FogH, f);
            }
            else if (fogged)
            {
                fogged = false;
                RenderSettings.fogColor = prevFogColor; RenderSettings.ambientMode = prevAmbMode;
                RenderSettings.ambientSkyColor = prevAmbSky; RenderSettings.ambientEquatorColor = prevAmbEq; RenderSettings.ambientGroundColor = prevAmbGround;
                Shader.SetGlobalColor(FogZ, prevFogZ); Shader.SetGlobalColor(FogM, prevFogM); Shader.SetGlobalColor(FogH, prevFogH);
            }
        }

        void RenderGlacier(SurvivalSim sim, bool on)
        {
            FogDusk(on);
            if (!on)
            {
                FrostShown = 0;
                if (laneGo != null) { DestroyLanes(); builtFor = null; }
                for (int i = 0; i < frostT.Length; i++) if (frostT[i].gameObject.activeSelf) frostT[i].gameObject.SetActive(false);
                return;
            }
            Glacier29 g29 = sim.glacier29;
            Rink28Arena a = sim.glacier28Rink;
            if (builtFor != a || builtCount != g29.lane.count || laneGo == null) BuildLanes(a, g29.lane);
            int mine = g29.lane.Locked ? g29.lane.LaneIndex : -1;
            for (int k = 0; k < laneR.Length; k++) Tint(laneR[k], lane * (k == mine ? UnityTuning.Lane29ActiveGain : UnityTuning.Lane29Gain));
            // the frost fields
            int n = 0;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 14f);
            Color danger = Look.Hex(UnityTuning.LanternOrbHex);
            foreach (Glacier29Frost f in g29.frost)
            {
                if (n >= frostT.Length) break;
                if (!frostT[n].gameObject.activeSelf) frostT[n].gameObject.SetActive(true);
                var dir = new Vector3((float)f.tx, 0, (float)-f.tz);
                if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;
                frostT[n].SetPositionAndRotation(U(new Vec3(f.x, a.floorY + UnityTuning.Frost29Lift, f.z)), Quaternion.LookRotation(dir, Vector3.up));
                frostT[n].localScale = new Vector3((float)(f.lat1 - f.lat0), 0.03f, (float)(f.hl * 2));
                bool telling = f.tellLeft > 0;
                float fade = telling ? 0.35f + 0.65f * pulse : Mathf.Clamp01(f.lifeLeft / 120f);
                Tint(frostR[n], (telling ? danger * 1.1f : frost * UnityTuning.Frost29Gain) * fade);
                n++;
            }
            for (int i = n; i < frostT.Length; i++) if (frostT[i].gameObject.activeSelf) frostT[i].gameObject.SetActive(false);
            FrostShown = n;
        }
    }
}
