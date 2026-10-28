// NIGHT37 builder W (wick37): what the player SEES of THE STACK and THE WICK.
// The lead adds two lines in SurvivalGame.Boss26.cs:  Boss26InitViews:   `Wick37View.Current = new Wick37View(this);`
//                                                      Boss26RenderViews: `Wick37View.Current?.Render(alpha);`
//  - THE STACK (whenever the level has the wick37 row): the SAME triangles the sim collides with (Wick37Stack): the shaft's dark iron with hot
//    red-orange seams (danger: it throws a rider off), the ride panels with cyan trim on their ride face (top, bottom, both ends), the ledge rings
//    with a cyan lip, the cap's catwalk and its ring wall with a cyan band (ride the full lap), the entry tunnel; pale lights up the shaft.
//  - THE SLAG: a red-orange disc filling the shaft at the slag's height, with its own glow light (it rises while THE WICK attacks).
//  - THE WICK: a cage of plates round a flame core. The plates turn with the cage; a cracked plate tilts open and glows (a gap: pierce through it).
//    The core: warm at rest, the tell's yellow / red through a wind-up (brightening), CYAN in the window and in the dive's stall.
//  - THE FIGHT FX: the embers (YELLOW; a returned one white), the flare's half of the wall (red, brightening through its tell, full while it
//    burns), the slam's decal on the ledge, the dive.
// Rule 8: red-orange danger, cyan usable / the window, yellow parry. No gameplay number here (UnityTuning.Wick37.cs).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Wick37View // NIGHT43 B1: partial (wick43's view in Wick37View.N43.cs)
    {
        public static Wick37View Current;

        readonly SurvivalGame game;
        readonly Transform root, stackRoot, fxRoot, bodyRoot;
        readonly Mesh boxMesh, ballMesh;
        readonly Material add;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        Vec3 builtAt;
        Wick37Stack builtFor;
        bool built;
        Renderer slagR, flareR, slamR, coreR, hotAR, hotBR, washR, curtainR;
        Transform curtainT;
        float burnFlash;
        int burnsSeen;
        Transform slagT, flareT, slamT, coreT, cageT;
        Light slagLight, coreLight;
        readonly List<Transform> plateT = new List<Transform>();
        readonly List<Renderer> plateGlowR = new List<Renderer>();
        readonly List<Transform> emberTs = new List<Transform>();
        readonly List<Renderer> emberRs = new List<Renderer>();
        Material iron, ironDark;
        float t;

        /// <summary>For the reel: the core's colour this frame, the embers drawn, the slag drawn, the flare drawn, the panels trimmed.</summary>
        public Color CoreColour;
        public int EmbersShown, PanelsTrimmed;
        public bool SlagShown => slagT != null && slagT.gameObject.activeSelf;
        public bool FlareShown => flareT != null && flareT.gameObject.activeSelf;

        public Wick37View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballMesh = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
            add = FxDraw.Additive(game.mats.neon, "Wick37 glow");
            root = new GameObject("NIGHT37 THE WICK").transform;
            stackRoot = new GameObject("stack").transform;
            stackRoot.SetParent(root, false);
            stackRoot.gameObject.SetActive(false);
            fxRoot = new GameObject("fight fx").transform;
            fxRoot.SetParent(root, false);
            bodyRoot = new GameObject("the wick").transform;
            bodyRoot.SetParent(root, false);
            bodyRoot.gameObject.SetActive(false);
        }

        Material Opaque(string name, uint hex)
        {
            var mat = new Material(game.mats.stone) { name = name };
            Color c = Look.Hex(hex);
            if (mat.HasProperty(BaseColor)) mat.SetColor(BaseColor, c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
            return mat;
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z
        static Vector3 N(Vec3 n) => new Vector3((float)n.x, (float)n.y, (float)-n.z);

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        Renderer Glow(Transform parent, string name, Mesh mesh, out Transform tr)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            tr = g.transform;
            return r;
        }

        Light PointLight(Transform parent, string name, Vector3 at, float range, float intensity, uint hex)
        {
            var lg = new GameObject(name);
            lg.transform.SetParent(parent, false);
            lg.transform.position = at;
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.range = range;
            l.intensity = intensity * UnityTuning.LightScale;
            l.color = Look.Hex(hex);
            return l;
        }

        // --- the stack ------------------------------------------------------------------------------------------------------------------------

        void BuildStack(Wick37Stack s, Vec3 o)
        {
            foreach (Transform ch in stackRoot) Object.Destroy(ch.gameObject);
            foreach (Transform ch in fxRoot) Object.Destroy(ch.gameObject);
            emberTs.Clear(); emberRs.Clear(); curtainT = null;
            Begin54Build(); // NIGHT54 light54 (Wick37View.Light54.cs): the albedo rule of this build
            var kits = new Dictionary<Wick37Part, MeshKit>();
            foreach (Wick37Tri tr in s.tris)
            {
                if (!kits.TryGetValue(tr.part, out MeshKit k)) kits[tr.part] = k = new MeshKit();
                Vec3 n = TriangleWorld.TriangleNormal(tr.a, tr.b, tr.c);
                k.Triangle(U(tr.a + o), U(tr.b + o), U(tr.c + o), N(n), Albedo54(tr.part)); // NIGHT54 light54: the Stone shader's albedo is the vertex colour (white off, as NIGHT53)
            }
            foreach (var kv in kits)
            {
                uint hex = kv.Key == Wick37Part.Shaft ? UnityTuning.Stack37ShaftHex : kv.Key == Wick37Part.Panel ? UnityTuning.Stack37PanelHex
                    : kv.Key == Wick37Part.Ledge || kv.Key == Wick37Part.Step ? UnityTuning.Stack37LedgeHex : kv.Key == Wick37Part.Tunnel ? UnityTuning.Stack37TunnelHex : UnityTuning.Stack37CapHex;
                string nm = "stack " + kv.Key.ToString().ToLowerInvariant();
                MeshKit.Object(nm, stackRoot, kv.Value.Build("wick37 " + nm), Opaque(nm, hex), shadows: false);
            }
            // the hot seams on the shaft (red-orange: danger) and the cyan trim (panels, ledge lips, the cap's ring)
            // two interleaved sets of seams (even / odd rows) whose glow pulses in counter-phase: the heat ripples up the wall (Render), and a
            // dim red-orange wash over the whole wall face so the iron itself reads hot, never cyan
            var hotA = new MeshKit();
            var hotB = new MeshKit();
            var wash = new MeshKit();
            var trim = new MeshKit();
            var lip = new MeshKit();
            double fa = s.FacetAngle, rw = s.CornerRadius - UnityTuning.Stack37SeamOff;
            for (int j = 0; j < s.p.facets; j++)
            {
                double a0 = j * fa, a1 = (j + 1) * fa, am = (j + 0.5) * fa;
                Vec3 nrm = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                int row = 0;
                for (double y = UnityTuning.Stack37SeamStep; y < s.p.capY - 0.5; y += UnityTuning.Stack37SeamStep, row++)
                {
                    if (s.InGap(j) && y < s.p.gapHeight) continue;
                    Vec3 p0 = Polar(a0, rw, y) + o, p1 = Polar(a1, rw, y) + o, h = new Vec3(0, UnityTuning.Stack37SeamH, 0);
                    (row % 2 == 0 ? hotA : hotB).Quad(U(p0), U(p1), U(p1 + h), U(p0 + h), N(nrm), Color.white);
                }
                double wy0 = s.InGap(j) ? s.p.gapHeight : 0, rwash = rw - 0.01;
                wash.Quad(U(Polar(a0, rwash, wy0) + o), U(Polar(a1, rwash, wy0) + o), U(Polar(a1, rwash, s.p.capY) + o), U(Polar(a0, rwash, s.p.capY) + o), N(nrm), Color.white);
                // the cap's ring wall: a cyan band at ride height (the full lap is a ride)
                double crw = s.p.capRadius / System.Math.Cos(fa / 2) - UnityTuning.Stack37TrimOff;
                foreach (double y in new[] { s.p.capY + UnityTuning.Stack37CapTrimY, s.p.capY + s.p.capWallHeight - 0.4 })
                {
                    Vec3 q0 = Polar(a0, crw, y) + o, q1 = Polar(a1, crw, y) + o, hh = new Vec3(0, UnityTuning.Stack37TrimH, 0);
                    trim.Quad(U(q0), U(q1), U(q1 + hh), U(q0 + hh), N(nrm), Color.white);
                }
            }
            PanelsTrimmed = 0;
            foreach (Wick37Panel pn in s.panels)
            {
                Vec3 off = pn.normal * UnityTuning.Stack37TrimOff, along = pn.end - pn.start, hh = new Vec3(0, PanelTrimH55(), 0); // NIGHT56 V: grade55's ride-cue strip height (Stack37TrimH with the row off)
                double len = along.Length;
                Vec3 dir = along * (1 / System.Math.Max(1e-6, len));
                Vec3 a = pn.start + off + o, b = pn.end + off + o;
                foreach (double y in new[] { 0.25, pn.top - pn.bottom - 0.3 })
                {
                    Vec3 lift = new Vec3(0, y, 0);
                    trim.Quad(U(a + lift), U(b + lift), U(b + lift + hh), U(a + lift + hh), N(pn.normal), Color.white);
                }
                double hgt = pn.top - pn.bottom;
                Vec3 w = dir * UnityTuning.Stack37TrimH;
                trim.Quad(U(a), U(a + w), U(a + w + new Vec3(0, hgt, 0)), U(a + new Vec3(0, hgt, 0)), N(pn.normal), Color.white);
                trim.Quad(U(b - w), U(b), U(b + new Vec3(0, hgt, 0)), U(b - w + new Vec3(0, hgt, 0)), N(pn.normal), Color.white);
                PanelsTrimmed++;
            }
            int ls = System.Math.Max(2, s.p.ledgeSegments);
            double arc = s.p.ledgeArcDeg * System.Math.PI / 180, sa = arc / ls;
            for (int li = 0; li < s.p.ledges.Length; li++)
            {
                if (s.p.stepCount > 0 && li == 0) continue; // NIGHT47 W (wick47): ledge 1 is the last step (its lip below, the step's arc)
                double ly = s.p.ledges[li], a00 = s.ledgeAngle[li] - arc / 2;
                double rIn = (s.p.radius - s.p.ledgeWidth) / System.Math.Cos(sa / 2) - UnityTuning.Stack37TrimOff;
                for (int k = 0; k < ls; k++)
                {
                    double a0 = a00 + k * sa, a1 = a00 + (k + 1) * sa, am = a00 + (k + 0.5) * sa;
                    Vec3 nrm = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                    Vec3 p0 = Polar(a0, rIn, ly - UnityTuning.Stack37TrimH) + o, p1 = Polar(a1, rIn, ly - UnityTuning.Stack37TrimH) + o, hh = new Vec3(0, UnityTuning.Stack37TrimH, 0);
                    lip.Quad(U(p0), U(p1), U(p1 + hh), U(p0 + hh), N(nrm), Color.white);
                }
            }
            var stepLip = new MeshKit();
            // NIGHT47 W (wick47): THE STEPS' cyan lips (a floor you stand on is player-usable: V-2), each step's arc and depth
            for (int si = 0; si < s.stepTops.Count; si++)
            {
                double arcS = s.StepArc(si), saS = arcS / ls, ly = s.stepTops[si], a00 = s.stepAngles[si] - arcS / 2;
                double rIn = (s.p.radius - s.StepDepth(si)) / System.Math.Cos(saS / 2) - UnityTuning.Stack37TrimOff;
                for (int k = 0; k < ls; k++)
                {
                    double a0 = a00 + k * saS, a1 = a00 + (k + 1) * saS, am = a00 + (k + 0.5) * saS;
                    Vec3 nrm = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                    Vec3 p0 = Polar(a0, rIn, ly - UnityTuning.Stack37TrimH) + o, p1 = Polar(a1, rIn, ly - UnityTuning.Stack37TrimH) + o, hh = new Vec3(0, UnityTuning.Stack37TrimH, 0);
                    stepLip.Quad(U(p0), U(p1), U(p1 + hh), U(p0 + hh), N(nrm), Color.white);
                }
            }
            Lips48Trim(s, o, ls, stepLip); // NIGHT48 W (climbTop48; Wick37View.N48.cs): the top-third lips' cyan edges in the steps' lip mesh (nothing without lips)
            if (s.stepTops.Count > 0) Tint(MeshKit.Object("stack step lips", stackRoot, stepLip.Build("wick47 step lips"), add, shadows: false).GetComponent<Renderer>(), Look.Hex(UnityTuning.Stack37TrimHex, UnityTuning.Stack37LipGain));
            hotAR = MeshKit.Object("stack hot seams A", stackRoot, hotA.Build("wick37 hot A"), add, shadows: false).GetComponent<Renderer>();
            hotBR = MeshKit.Object("stack hot seams B", stackRoot, hotB.Build("wick37 hot B"), add, shadows: false).GetComponent<Renderer>();
            washR = MeshKit.Object("stack hot wash", stackRoot, wash.Build("wick37 wash"), add, shadows: false).GetComponent<Renderer>();
            Tint(MeshKit.Object("stack ride trim", stackRoot, trim.Build("wick37 trim"), add, shadows: false).GetComponent<Renderer>(), Look.Hex(UnityTuning.Stack37TrimHex, UnityTuning.Stack37TrimGain));
            Tint(MeshKit.Object("stack ledge lips", stackRoot, lip.Build("wick37 lips"), add, shadows: false).GetComponent<Renderer>(), Look.Hex(UnityTuning.Stack37TrimHex, UnityTuning.Stack37LipGain));
            foreach (float y in UnityTuning.Stack37LightYs)
                PointLight(stackRoot, "stack light", U(o) + new Vector3(0, y, 0), UnityTuning.Stack37LightRange, UnityTuning.Stack37LightIntensity, UnityTuning.Stack37LightHex);
            // the slag (a disc filling the shaft) and its light
            slagR = Glow(stackRoot, "slag", Disc(), out slagT);
            slagT.localScale = new Vector3((float)s.CornerRadius, 1, (float)s.CornerRadius);
            slagLight = PointLight(slagT, "slag light", Vector3.zero, UnityTuning.Stack37SlagLightRange, UnityTuning.Stack37SlagLightIntensity, UnityTuning.Stack37SlagHex);
            slagLight.transform.localPosition = new Vector3(0, 1.5f, 0);
            // the fight fx
            flareR = Glow(fxRoot, "flare half", HalfRing(s), out flareT);
            flareT.gameObject.SetActive(false);
            slamR = Glow(fxRoot, "slam decal", Disc(), out slamT);
            slamT.gameObject.SetActive(false);
            BuildBody();
            builtAt = o;
            builtFor = s;
            built = true;
        }

        static Vec3 Polar(double a, double r, double y) => new Vec3(System.Math.Cos(a) * r, y, System.Math.Sin(a) * r);

        Mesh disc;
        Mesh Disc()
        {
            if (disc != null) return disc;
            var k = new MeshKit();
            int n = UnityTuning.Stack37DiscSegments;
            for (int i = 0; i < n; i++)
            {
                float a0 = i * Mathf.PI * 2 / n, a1 = (i + 1) * Mathf.PI * 2 / n;
                k.Triangle(Vector3.zero, new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)), new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), Vector3.up, Color.white);
            }
            disc = k.Build("wick37 disc");
            return disc;
        }

        /// <summary>A half ring (Wick37FlareSpanDeg) of the shaft's wall, 1 m tall, centred on local +x (rotated to the flare's angle each frame).</summary>
        Mesh HalfRing(Wick37Stack s)
        {
            var k = new MeshKit();
            int n = UnityTuning.Wick37FlareSegments;
            float span = UnityTuning.Wick37FlareSpanDeg * Mathf.Deg2Rad, r = (float)s.p.radius - 0.08f;
            for (int i = 0; i < n; i++)
            {
                float a0 = -span / 2 + span * i / n, a1 = -span / 2 + span * (i + 1) / n;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * r, 0, Mathf.Sin(a0) * r), p1 = new Vector3(Mathf.Cos(a1) * r, 0, Mathf.Sin(a1) * r);
                Vector3 nrm = -new Vector3(Mathf.Cos((a0 + a1) / 2), 0, Mathf.Sin((a0 + a1) / 2));
                k.Quad(p0, p1, p1 + Vector3.up, p0 + Vector3.up, nrm, Color.white);
                k.Quad(p1, p0, p0 + Vector3.up, p1 + Vector3.up, -nrm, Color.white);
            }
            return k.Build("wick37 flare");
        }

        // --- the body ------------------------------------------------------------------------------------------------------------------------

        void BuildBody()
        {
            foreach (Transform old in bodyRoot) Object.Destroy(old.gameObject);
            plateT.Clear(); plateGlowR.Clear();
            iron ??= Opaque("wick37 iron", UnityTuning.Wick37IronHex);
            ironDark ??= Opaque("wick37 iron dark", UnityTuning.Wick37IronDarkHex);
            Body54Materials(); // NIGHT54 light54: the plates' grey Lit pair with the row on (the Stone copy drew them white); NIGHT37's pair off
            var p = game.cfg; // the plate count and the cage's size are the sim's (design.n37.wick37.boss)
            int plates = p.Int("design.n37.wick37.boss.plates");
            float cr = (float)p.Num("design.n37.wick37.boss.cageR"), ch = (float)p.Num("design.n37.wick37.boss.cageHeight"), core = (float)p.Num("design.n37.wick37.boss.coreR");
            cageT = new GameObject("cage").transform;
            cageT.SetParent(bodyRoot, false);
            bool m43 = Models43.On(game.cfg); // NIGHT43 A1 models43: the cage built from Mesh43's smooth shapes (read when the body is built)
            for (int i = 0; i < plates; i++)
            {
                float a = (i + 0.5f) * Mathf.PI * 2 / plates;
                var hinge = new GameObject("plate hinge " + i).transform;
                hinge.SetParent(cageT, false);
                hinge.localPosition = new Vector3(Mathf.Cos(a) * cr, -ch / 2, -Mathf.Sin(a) * cr);
                hinge.localRotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(a), 0, -Mathf.Sin(a)), Vector3.up);
                var plate = Solid(hinge, "plate", new Vector3(0, ch / 2, 0), new Vector3(UnityTuning.Wick37PlateW, ch * 0.92f, UnityTuning.Wick37PlateT), iron, m43 ? Mesh43.Get(Mesh43.ShieldSlab) : null); // NIGHT43 A1 models43: a bowed rounded plate
                Renderer gr = Glow(hinge, "plate seam", boxMesh, out Transform gt);
                gt.localPosition = new Vector3(0, ch / 2, -UnityTuning.Wick37PlateT);
                gt.localScale = new Vector3(UnityTuning.Wick37PlateW * 0.8f, ch * 0.8f, 0.02f);
                plateT.Add(hinge);
                plateGlowR.Add(gr);
            }
            if (!m43)
            {
                Solid(cageT, "top ring", new Vector3(0, ch / 2 + 0.1f, 0), new Vector3(cr * 2.1f, 0.25f, cr * 2.1f), ironDark, ballMesh);
                Solid(cageT, "bottom ring", new Vector3(0, -ch / 2 - 0.1f, 0), new Vector3(cr * 1.6f, 0.3f, cr * 1.6f), ironDark, ballMesh);
                Solid(cageT, "spire", new Vector3(0, ch / 2 + 0.7f, 0), new Vector3(0.25f, 1.2f, 0.25f), ironDark);
                Solid(cageT, "keel", new Vector3(0, -ch / 2 - 0.8f, 0), new Vector3(0.3f, 1.2f, 0.3f), ironDark);
            }
            else
            {
                // NIGHT43 A1 models43 (G21: "standard enemies, elite enemies, and bosses" in one language; the lead's round-2 ask, mesh building only): the
                // cage's caps a domed crown and a domed foot (Mesh43.Pauldron, the lip rolled), a band round the crown's rim (Mesh43.Ring), the spire and
                // the keel lathed horns (Mesh43.Spike, the keel turned down): the same smooth shapes as the rebuilt enemies; the same sizes and places
                Solid(cageT, "top crown", new Vector3(0, ch / 2 + 0.05f, 0), new Vector3(cr * 2.1f, 0.7f, cr * 2.1f), ironDark, Mesh43.Get(Mesh43.Pauldron));
                Solid(cageT, "crown band", new Vector3(0, ch / 2 + 0.02f, 0), new Vector3(cr * 2.2f, 0.5f, cr * 2.2f), iron, Mesh43.Get(Mesh43.Ring));
                Transform foot = Solid(cageT, "bottom foot", new Vector3(0, -ch / 2 - 0.05f, 0), new Vector3(cr * 1.6f, 0.6f, cr * 1.6f), ironDark, Mesh43.Get(Mesh43.Pauldron));
                foot.localRotation = Quaternion.Euler(180, 0, 0);
                Solid(cageT, "spire", new Vector3(0, ch / 2 + 0.85f, 0), new Vector3(0.4f, 1.5f, 0.4f), ironDark, Mesh43.Get(Mesh43.Spike));
                Transform keel = Solid(cageT, "keel", new Vector3(0, -ch / 2 - 0.85f, 0), new Vector3(0.45f, 1.4f, 0.45f), ironDark, Mesh43.Get(Mesh43.Spike));
                keel.localRotation = Quaternion.Euler(180, 0, 0);
            }
            coreR = Glow(bodyRoot, "flame core", ballMesh, out coreT);
            coreT.localScale = Vector3.one * cr * UnityTuning.Wick37CoreDrawFrac * 2; // the flame core fills most of the cage: it shows through the gaps between the plates
            coreLight = PointLight(bodyRoot, "core light", Vector3.zero, UnityTuning.Wick37CoreLightRange, UnityTuning.Wick37CoreLightIntensity, UnityTuning.Wick37WarmHex);
            coreLight.transform.localPosition = Vector3.zero;
        }

        Transform Solid(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, Mesh mesh = null)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = size;
            g.AddComponent<MeshFilter>().sharedMesh = mesh ?? boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.On;
            return g.transform;
        }

        static float FlashGain(double progress)
        {
            float g = Mathf.Lerp(UnityTuning.Tell36FlashStart, UnityTuning.Tell36FlashPeak, (float)progress);
            if (progress > 0.7) g *= calm54 ? Ramp54(progress) : 0.75f + 0.25f * Mathf.Sin((float)(Time.time * UnityTuning.Tell36PulseHz * Mathf.PI * 2)); // NIGHT54 light54: one brighten, not the 7 Hz pulse
            return g;
        }

        // --- the frame -----------------------------------------------------------------------------------------------------------------------

        public void Render(double alpha)
        {
            BossTell63.Draw(game, root, BossAttackClock63.Wick); // NIGHT63 G bossTell63 (BossTell63.cs): the danger column through a non-parry wind-up (nothing with the row off)
            SurvivalSim sim = game.sim;
            t += Time.deltaTime;
            if (sim == null || sim.wick37Centre == null || sim.wick37Stack == null)
            {
                stackRoot.gameObject.SetActive(false);
                fxRoot.gameObject.SetActive(false);
                bodyRoot.gameObject.SetActive(false);
                Letterbox(-1f, false); // NIGHT43 B1 (wick43): no stack, no frame
                Light54Frame(sim); // NIGHT54 light54 (Wick37View.Light54.cs): no stack, no bloom trim
                return;
            }
            Vec3 o = sim.wick37Centre.Value;
            if (!built || builtAt != o || builtFor != sim.wick37Stack) BuildStack(sim.wick37Stack, o);
            stackRoot.gameObject.SetActive(true);
            // the hot wall: the seams ripple (two sets in counter-phase), the wash glows; a burn flashes them all
            if (sim.wick37HotBurns != burnsSeen) { if (sim.wick37HotBurns > burnsSeen) burnFlash = UnityTuning.Stack37BurnFlashS; burnsSeen = sim.wick37HotBurns; }
            burnFlash = Mathf.Max(0, burnFlash - Time.deltaTime);
            float fl = calm54 ? Burn54(burnFlash) : 1 + UnityTuning.Stack37BurnFlashGain * burnFlash / UnityTuning.Stack37BurnFlashS; // NIGHT54 light54: a softer, eased burn flash
            float ph = t * UnityTuning.Stack37ShimmerHz * Mathf.PI * 2;
            float amp = Shimmer54(); // NIGHT54 light54: the seams' ripple shallower (Stack37ShimmerAmp off)
            Tint(hotAR, Look.Hex(UnityTuning.Stack37HotHex, UnityTuning.Stack37SeamGain * fl * (1 + amp * Mathf.Sin(ph))));
            Tint(hotBR, Look.Hex(UnityTuning.Stack37HotHex, UnityTuning.Stack37SeamGain * fl * (1 - amp * Mathf.Sin(ph))));
            Tint(washR, Look.Hex(UnityTuning.Stack37HotHex, UnityTuning.Stack37WashGain * fl * (1 + 0.25f * Mathf.Sin(ph * 0.37f))));
            // the slag
            Wick37Slag slag = sim.wick37Slag;
            bool slagOn = slag != null;
            slagT.gameObject.SetActive(slagOn);
            if (slagOn)
            {
                slagT.position = U(o) + new Vector3(0, (float)slag.y + UnityTuning.Stack37SlagLift, 0);
                float pulse = 1 + 0.12f * Mathf.Sin(t * 2.3f);
                Tint(slagR, Look.Hex(UnityTuning.Stack37SlagHex, UnityTuning.Stack37SlagGain * pulse * (slag.rising ? 1.25f : 1f)));
            }
            Wick37 w = sim.wick37;
            bool fight = w != null && w.Alive && w.brain != null;
            fxRoot.gameObject.SetActive(fight);
            bodyRoot.gameObject.SetActive(fight);
            if (!fight) { EmbersShown = 0; Render43(sim, w, o, false); Render47(sim, w, o, false); Light54Frame(sim); return; } // NIGHT43 B1 (wick43): the lights and the frame outside a fight too
            Boss37Brain b = w.brain;
            string atk = w.AttackId;
            double prog = b.Progress;
            // the body
            bodyRoot.position = U(w.Centre) + new Vector3(0, Mathf.Sin(t * UnityTuning.Wick37BreatheHz * Mathf.PI * 2) * UnityTuning.Wick37BreatheAmp, 0);
            cageT.localRotation = Quaternion.Euler(0, -(float)(w.cageTurn * 180 / System.Math.PI), 0);
            for (int i = 0; i < plateT.Count; i++)
            {
                bool cracked = i < w.crackLeft.Length && w.crackLeft[i] > 0;
                float open = b.Open || w.stalled ? UnityTuning.Wick37OpenTiltDeg : cracked ? UnityTuning.Wick37CrackTiltDeg : 0; // the window: every plate flung wide
                plateT[i].localRotation = Quaternion.LookRotation(Dir((i + 0.5f) * Mathf.PI * 2 / plateT.Count), Vector3.up) * Quaternion.Euler(-open, 0, 0);
                Tint(plateGlowR[i], cracked || b.Open ? Look.Hex(UnityTuning.Wick37OpenHex, UnityTuning.Wick37CrackGain) : BossColours63.Part(game.cfg, "wick.edges", Look.Hex(UnityTuning.Wick37WarmHex, UnityTuning.Wick37EdgeGain))); // ember-lit edges; cyan when open (NIGHT63 B3: danger with bossColours63)
            }
            Color core;
            if (b.Open || w.stalled) core = Look.Hex(UnityTuning.Wick37OpenHex, UnityTuning.Wick37CoreGainOpen * (0.85f + 0.15f * Mathf.Sin(t * 12)));
            else if (b.state == Boss37State.Tell && b.Current != null)
                core = Look.Hex(b.Current.kind == Tell36Kind.Yellow ? UnityTuning.Tell36YellowHex : UnityTuning.Tell36RedHex, UnityTuning.Wick37CoreGainTell * FlashGain(prog));
            else if (b.state == Boss37State.Active && b.Current != null && b.Current.kind == Tell36Kind.Red) core = Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Wick37CoreGainTell);
            else core = BossColours63.Part(game.cfg, "wick.core", Look.Hex(UnityTuning.Wick37WarmHex, UnityTuning.Wick37CoreGainIdle)); // NIGHT63 B3: the weak point's cyan, dim, with bossColours63
            CoreColour = core;
            Tint(coreR, core);
            coreLight.color = core.maxColorComponent > 0 ? core / core.maxColorComponent : core;
            Weak74Wick(b.Open || w.stalled); // NIGHT74 B weak74 (Wick37View.Weak74.cs): the probe's boxes for the boss63 reel (the idle core's colour is BossColours63's)
            // the embers
            int shown = 0;
            foreach (Wick37Ember e in w.embers)
            {
                if (!e.alive) continue;
                if (shown >= emberTs.Count && emberTs.Count < UnityTuning.Wick37EmberPool) { emberRs.Add(Glow(fxRoot, "ember", ballMesh, out Transform et)); emberTs.Add(et); }
                if (shown >= emberTs.Count) break;
                Vec3 pos = e.prev + (e.pos - e.prev) * alpha;
                emberTs[shown].gameObject.SetActive(true);
                emberTs[shown].position = U(pos);
                emberTs[shown].localScale = Vector3.one * UnityTuning.Wick37EmberDraw * 2;
                Tint(emberRs[shown], e.back ? Look.Hex(0xffffff, 1.6f) : Look.Hex(UnityTuning.Tell36YellowHex, 1.8f));
                shown++;
            }
            for (int i = shown; i < emberTs.Count; i++) emberTs[i].gameObject.SetActive(false);
            EmbersShown = shown;
            // the flare: the half of the wall it burns, red through the tell and full while live
            bool flareTell = atk == Wick37.Flare && b.state == Boss37State.Tell;
            flareT.gameObject.SetActive(flareTell || w.flareLive);
            if (flareTell || w.flareLive)
            {
                float y = (float)(w.Centre.y - o.y - w.p.flareBelow - 2);
                flareT.position = U(o) + new Vector3(0, y, 0);
                flareT.localRotation = Quaternion.Euler(0, (float)(w.flareAngle * 180 / System.Math.PI), 0);
                flareT.localScale = new Vector3(1, UnityTuning.Wick37FlareH, 1);
                // the burning half lights at once (round 3: it rose from a dim sliver) and brightens through the tell
                Tint(flareR, Look.Hex(UnityTuning.Tell36RedHex, w.flareLive ? UnityTuning.Wick37FlareGain * 1.6f : UnityTuning.Wick37FlareGain * Mathf.Max(UnityTuning.Wick37FlareTellMin, FlashGain(prog))));
            }
            // the CURTAIN: a red sheet through the axis on the line between the burning half and the safe half (in view whenever you look at the
            // core): leap across it
            if (curtainT == null) { curtainR = Glow(fxRoot, "flare curtain", boxMesh, out curtainT); }
            curtainT.gameObject.SetActive(flareTell || w.flareLive);
            if (flareTell || w.flareLive)
            {
                float cy = (float)(w.Centre.y - o.y - w.p.flareBelow - 2) + UnityTuning.Wick37CurtainH * 0.5f;
                curtainT.position = U(o) + new Vector3(0, cy, 0);
                curtainT.localRotation = Quaternion.Euler(0, (float)(w.flareAngle * 180 / System.Math.PI), 0);
                curtainT.localScale = new Vector3(0.06f, UnityTuning.Wick37CurtainH, (float)w.stack.p.radius * 2);
                Tint(curtainR, Look.Hex(UnityTuning.Tell36RedHex, w.flareLive ? UnityTuning.Wick37CurtainLive : Mathf.Max(UnityTuning.Wick37CurtainTellMin, FlashGain(prog) * 0.5f)));
            }
            // the slam's decal on the camp spot
            bool slamTell = atk == Wick37.Slam && b.state == Boss37State.Tell;
            slamT.gameObject.SetActive(slamTell);
            if (slamTell)
            {
                slamT.position = U(b.campAnchor) + new Vector3(0, 0.05f, 0);
                float r = (float)w.p.slamRadius;
                slamT.localScale = new Vector3(r, 1, r);
                Tint(slamR, Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Wick37SlamGain * FlashGain(prog)));
            }
            Render43(sim, w, o, true); // NIGHT43 B1 (wick43; Wick37View.N43.cs): the reveal's frame, the lights, the leg beat, leg 3's cap flare
            Render47(sim, w, o, true); // NIGHT47 W (Wick37View.N47.cs): the panels folded in (P1), the glass (P3), wickLook47's indicators
            DrawMotion63(sim, w); // NIGHT63 V bossMotion63 (Wick37View.N63V.cs): the springs, the wind-up, the flinch, the slump, the breath on the cage (off: the rigid pose)
            TripoWick(); // PG3 tripo74: THE WICK's Tripo model over the cage, the core and the seams kept (Wick37View.Tripo.cs; nothing when its file is missing or the row is off)
            Light54Frame(sim); // NIGHT54 light54 (Wick37View.Light54.cs): the arena's exposure, the bloom trim, the core light's colour eased
        }

        /// <summary>A web angle (atan2(z, x), radians) as a Unity direction on the ground.</summary>
        static Vector3 Dir(double a) => new Vector3((float)System.Math.Cos(a), 0, -(float)System.Math.Sin(a));

        partial void Weak74Wick(bool open);
    }
}
