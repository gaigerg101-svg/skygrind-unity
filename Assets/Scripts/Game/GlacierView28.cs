// NIGHT28 builder B2: THE GLACIER lap fight as the view draws it (Core/Enemies/Glacier28.cs; the rink itself is builder A2's RinkView28). Shown only while sim.glacier28 is alive. It draws:
//   - the skater (the NIGHT27 blocky model, Design/bosses/glacier27.json, boxes in its own frame) placed on the rink's centre line and turned along the way it runs (it brakes and turns in the skid),
//     with the ice GLAZE as a cyan shell while glazed (dimmer as the crack fills; gone while cracked) and the PULSE-READY shell while a window is open (cyan = player-usable);
//   - a cyan MARKER: a thin beam and a square ring on the boss, so it is never a speck across the island;
//   - the attacks as pooled boxes in the danger colour (red-orange): the decal flat on the floor during the tell, then the standing object (shard, column, roller, low bar, chasm strip, melt strip);
//   - the GAP TAPE (TextMeshPro, top centre, 30 px): the signed gap in metres both ways (>> the boss is counter-clockwise of you, << clockwise), the glaze state and the glove.
// The lead adds two lines to SurvivalGame.Boss26.cs (`public GlacierView28 glacierView28;` + `glacierView28 = new GlacierView28(this);` in Boss26InitViews + `glacierView28?.Render(alpha);` in Boss26RenderViews).
// What A2's RinkView28 must expose: nothing (the rink is only triangles; this view reads Rink28Math from the boss). Not compiled in the Editor by its author (dn-check only).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class GlacierView28
    {
        readonly SurvivalGame game;
        readonly Transform root;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        readonly Mesh box;
        readonly Material solid, add;
        readonly Color body, visor, blade, cyan, danger;
        readonly Transform[] partT = new Transform[UnityTuning.SkaterPartMax];
        readonly Renderer[] partR = new Renderer[UnityTuning.SkaterPartMax];
        readonly Transform[] hazT = new Transform[UnityTuning.Glacier28HazardMax], decT = new Transform[UnityTuning.Glacier28HazardMax];
        readonly Renderer[] hazR = new Renderer[UnityTuning.Glacier28HazardMax], decR = new Renderer[UnityTuning.Glacier28HazardMax];
        Transform glazeT, readyT, beamT;
        readonly Transform[] ringT = new Transform[4];
        Renderer glazeR, readyR, beamR;
        readonly Renderer[] ringR = new Renderer[4];
        readonly List<(string part, double a, double b, double c, double hx, double hy, double hz)> model = new List<(string, double, double, double, double, double, double)>();
        TMPro.TextMeshProUGUI tape;
        GameObject tapeRoot;

        public bool Shown => root != null && root.gameObject.activeSelf;
        /// <summary>The tape's text this frame (the reel line and the HUD read it).</summary>
        public string TapeText { get; private set; } = "";

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        public GlacierView28(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            body = Look.Hex(UnityTuning.SkaterBodyHex);
            visor = Look.Hex(UnityTuning.SkaterVisorHex, UnityTuning.SkaterVisorGain);
            blade = Look.Hex(UnityTuning.SkaterBladeHex, UnityTuning.SkaterBladeGain);
            cyan = Look.Hex(UnityTuning.SkaterReadyHex);
            danger = Look.Hex(UnityTuning.GlacierAttackHex);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            solid = FxDraw.Opaque(game.mats.neon, "Glacier28 body", Color.white);
            add = FxDraw.Additive(game.mats.neon, "Glacier28 glow");
            root = new GameObject("THE GLACIER (lap fight)").transform;
            LoadModel(cfg);
            for (int i = 0; i < partT.Length; i++) partR[i] = Part("skater " + i, solid, out partT[i]);
            for (int i = 0; i < hazT.Length; i++)
            {
                hazR[i] = Part("attack " + i, add, out hazT[i]);
                decR[i] = Part("decal " + i, add, out decT[i]);
            }
            glazeR = Part("glaze", add, out glazeT);
            readyR = Part("ready", add, out readyT);
            beamR = Part("marker beam", add, out beamT);
            for (int i = 0; i < 4; i++) ringR[i] = Part("marker ring " + i, add, out ringT[i]);
            root.gameObject.SetActive(false);
        }

        void LoadModel(GameConfig cfg)
        {
            string dir = SurvivalSim.Wyrm24Dir;
            if (string.IsNullOrEmpty(dir) || !cfg.Has("design.n27.boss2.body.model")) return;
            string path = System.IO.Path.Combine(dir, cfg.Str("design.n27.boss2.body.model"));
            if (!System.IO.File.Exists(path)) return;
            foreach (object o in PortData.Arr(PortData.Obj(PortData.LoadJson(path))["parts"]))
            {
                var p = PortData.Obj(o);
                var h = PortData.Arr(p["half"]);
                if (model.Count < partT.Length) model.Add(((string)p["part"], PortData.Num(p["a"]), PortData.Num(p["b"]), PortData.Num(p["c"]), PortData.Num(h[0]), PortData.Num(h[1]), PortData.Num(h[2])));
            }
        }

        Renderer Part(string name, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = box;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            g.SetActive(false);
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static void Set(Transform t, bool on)
        {
            if (t.gameObject.activeSelf != on) t.gameObject.SetActive(on);
        }

        void Place(Transform t, Vector3 centre, Quaternion rot, Vector3 size)
        {
            t.SetPositionAndRotation(centre, rot);
            t.localScale = size;
        }

        void EnsureTape()
        {
            if (tapeRoot != null) return;
            tapeRoot = new GameObject("Glacier28 gap tape");
            var canvas = tapeRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var txt = new GameObject("tape text");
            txt.transform.SetParent(tapeRoot.transform, false);
            tape = txt.AddComponent<TMPro.TextMeshProUGUI>();
            tape.alignment = TMPro.TextAlignmentOptions.Center;
            tape.fontSize = UnityTuning.G28TapeFont;
            tape.raycastTarget = false;
            var rt = tape.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -UnityTuning.G28TapeTop);
            rt.sizeDelta = new Vector2(900, 80);
        }

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            Glacier28 g = sim?.glacier28;
            bool on = g != null && g.Alive && g.phase != "dead";
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            bool tapeOn = on && !(sim.n29.boss2 && N29Fixes.BossViewOn); // NIGHT29: the Hud29 gap bar replaces the old tape (its GLAZED / glove text is the pulse-only rule's)
            if (tapeRoot != null && tapeRoot.activeSelf != tapeOn) tapeRoot.SetActive(tapeOn);
            if (!on) return;
            float a = (float)alpha;
            Vec3 f = new Vec3(Mathf.Lerp((float)g.prevCenter.x, (float)g.box.c.x, a), g.origin.y + 0.2, Mathf.Lerp((float)g.prevCenter.z, (float)g.box.c.z, a));
            // the way it runs: the box's forward, turned by the skid (the frame is the box's own, set by the sim)
            Vec3 fw = g.box.f;
            Vector3 fwU = new Vector3((float)fw.x, 0, (float)-fw.z);
            if (fwU.sqrMagnitude < 1e-6f) fwU = Vector3.forward;
            Quaternion rot = Quaternion.LookRotation(fwU.normalized, Vector3.up);
            double leap = g.BossOverChasm ? UnityTuning.G28LeapHeight : 0;
            Vector3 origin = new Vector3((float)f.x, (float)(f.y + leap), (float)-f.z);
            int n = System.Math.Min(model.Count, partT.Length);
            for (int i = 0; i < n; i++)
            {
                var m = model[i];
                Set(partT[i], true);
                // the model's frame: a = right, b = up, c = forward
                Vector3 centre = origin + rot * new Vector3((float)m.a, (float)m.b, (float)m.c);
                Place(partT[i], centre, rot, new Vector3((float)m.hx * 2, (float)m.hy * 2, (float)m.hz * 2));
                Tint(partR[i], m.part == "blade" ? blade : m.part == "visor" ? visor : m.part == "plate" ? cyan * 1.5f : body);
            }
            for (int i = n; i < partT.Length; i++) Set(partT[i], false);
            Glacier28Params p = g.p;
            Vector3 mid = origin + Vector3.up * (float)(p.boxH * 0.5);
            // the glaze: a cyan shell while glazed (dimmer as the crack fills), nothing while cracked; the PULSE-READY shell while a window is open
            bool glazed = g.glazed;
            Set(glazeT, glazed);
            if (glazed)
            {
                Place(glazeT, mid, rot, new Vector3((float)p.boxW, (float)p.boxH, (float)p.boxL) * UnityTuning.G28GlazeGrow);
                Tint(glazeR, cyan * (UnityTuning.G28GlazeGain * (1f - 0.7f * (float)g.CrackFrac)));
            }
            bool ready = g.WindowOpen;
            Set(readyT, ready);
            if (ready)
            {
                float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 18f);
                Place(readyT, mid, rot, new Vector3((float)p.boxW, (float)p.boxH, (float)p.boxL) * UnityTuning.G28ReadyGrow);
                Tint(readyR, cyan * (pulse * 2f));
            }
            // the marker: a thin beam and a square ring (rotating slowly) so the boss is never a speck
            Set(beamT, true);
            Place(beamT, origin + Vector3.up * (UnityTuning.G28BeamHeight * 0.5f), Quaternion.identity, new Vector3(UnityTuning.G28BeamWidth, UnityTuning.G28BeamHeight, UnityTuning.G28BeamWidth));
            Tint(beamR, cyan * UnityTuning.G28BeamGain);
            Quaternion spin = Quaternion.Euler(0, Time.time * 40f, 0);
            float half = UnityTuning.G28RingSize * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Set(ringT[i], true);
                bool alongX = i < 2;
                Vector3 off = alongX ? new Vector3(0, 0, (i == 0 ? -half : half)) : new Vector3((i == 2 ? -half : half), 0, 0);
                Vector3 size = alongX ? new Vector3(UnityTuning.G28RingSize, UnityTuning.G28RingBar, UnityTuning.G28RingBar) : new Vector3(UnityTuning.G28RingBar, UnityTuning.G28RingBar, UnityTuning.G28RingSize);
                Place(ringT[i], origin + Vector3.up * UnityTuning.G28RingLift + spin * off, spin, size);
                Tint(ringR[i], cyan * UnityTuning.G28RingGain);
            }
            // the attacks: decal flat during the tell (growing in), the object standing after it
            int k = 0;
            Rink28Math rk = g.rink;
            foreach (Glacier28Hazard h in g.hazards)
            {
                if (k >= hazT.Length) break;
                double lc = (h.lat0 + h.lat1) * 0.5, wl = h.lat1 - h.lat0;
                rk.PointAt(h.s, lc, out double hx, out double hz, out double tx, out double tz);
                Vector3 pos = new Vector3((float)hx, (float)g.origin.y, (float)-hz);
                Quaternion hr = Quaternion.LookRotation(new Vector3((float)tx, 0, (float)-tz), Vector3.up);
                Vector3 size = new Vector3((float)wl, 0, (float)(h.hl * 2));
                if (h.tellLeft > 0)
                {
                    Set(hazT[k], false);
                    Set(decT[k], true);
                    float fill = 1f - (float)h.tellLeft / System.Math.Max(1, h.total);
                    Place(decT[k], pos + Vector3.up * 0.06f, hr, new Vector3(size.x, UnityTuning.G28DecalThick, size.z));
                    Tint(decR[k], danger * (UnityTuning.G28DecalGain * (0.25f + 0.75f * fill)));
                }
                else
                {
                    Set(decT[k], h.kind == "melt" || h.kind == "chasm");
                    float y0 = (float)h.y0, y1 = (float)(h.kind == "melt" || h.kind == "chasm" ? 0.05 : h.y1);
                    if (h.kind == "melt" || h.kind == "chasm")
                    {
                        Place(decT[k], pos + Vector3.up * 0.06f, hr, new Vector3(size.x, UnityTuning.G28DecalThick * 2, size.z));
                        Tint(decR[k], danger * (UnityTuning.G28DecalGain * (h.safe ? 0.5f : 1f)));
                        Set(hazT[k], false);
                    }
                    else
                    {
                        Set(hazT[k], true);
                        float hgt = System.Math.Max(0.05f, y1 - y0);
                        Place(hazT[k], pos + Vector3.up * (y0 + hgt * 0.5f), hr, new Vector3(size.x, hgt, size.z));
                        Tint(hazR[k], danger * UnityTuning.G28ObjectGain);
                    }
                }
                k++;
            }
            for (int i = k; i < hazT.Length; i++) { Set(hazT[i], false); Set(decT[i], false); }
            // the gap tape
            EnsureTape();
            double sg = g.gapSigned;
            string dirArrows = sg >= 0 ? ">> " : "";
            string dirArrowsR = sg >= 0 ? "" : " <<";
            string glaze = g.CutOpen ? "CUT: PULSE LANDS" : g.glazed ? $"GLAZED {(int)(g.CrackFrac * 100)}%" : $"CRACKED {g.OpenFrac * p.glazeOpen:0.0}s";
            string win = g.WindowOpen ? "  PULSE-READY" : g.Ready ? "  glove ready" : "  glove charging";
            TapeText = $"{dirArrows}{System.Math.Abs(sg):0} m{dirArrowsR}   {glaze}{win}";
            tape.text = TapeText;
            tape.color = g.WindowOpen ? new Color(cyan.r, cyan.g, cyan.b, 1f) : new Color(0.81f, 0.78f, 1f, 1f);
        }
    }
}
