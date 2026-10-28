// NIGHT32 builder B (view row design.n32.bossMarks; DESIGN32 B; the lead's frame review, Research/2026-10-10-wave32/REVIEW-frames.md: "Bosses read as dark boxes ... Nothing says
// 'shoot here' except the Lantern's core cube"; n32-r2 stag-back frame: a black box on dark ice, mid-frame luminance 0.074, the open back a thin cyan line). View only: reads the sim,
// writes nothing. Drawn additive (cyan-white: rule 8's player-usable colour), so it lifts a dark boss off a dark floor:
//  - every boss's HURTBOX gets a faint cyan-white outline (12 edge beams per box; the Wyrm's pieces a faint shell each, the buried ones skipped) so the body reads at 20 m;
//  - its DAMAGE SPOT glows: the Lantern's core box, the Stag's rear (where a hit from behind lands in full), the Wyrm's head, the Loom's eight joints;
//  - an OPEN WINDOW is unmistakable: the spot swells and pulses at full brightness (the Lantern's core open, the Stag's back open, the Wyrm staggered, a Loom joint in full reach).
// Built and drawn from Boss31View.Render (one hook line). Numbers: UnityTuning (view-only constants below, each with a comment). No sim effect: no tape or fingerprint change.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class BossMarks32
    {
        // view-only numbers (render, not gameplay; CLAUDE.md rule 5 puts render numbers in Unity-side code with a comment)
        const float EdgeThick = 0.09f;                       // m: an outline beam's thickness (about 2 px at 20 m on a 1080p frame at the game's FOV)
        const float OutlineGlow = 0.35f;                     // the outline's brightness (additive): reads on the dark ice / deck, under the spot's
        const float SpotGlow = 0.55f;                        // the damage spot's brightness with the window shut
        const float OpenGlow = 1.6f;                         // ... and with the window open (pulsing between OpenGlow * 0.7 and OpenGlow)
        const float OpenPulseHz = 3f;                        // the open window's pulse
        const float SpotSwell = 1.15f, OpenSwell = 1.45f;    // the spot shell's size over the hurtbox (shut / open): at least 1.3x the drawn part when open
        const float JointShell = 0.8f, JointOpenSwell = 1.15f;   // a Loom joint's shell: half-size 0.8 x the hit radius (Boss30View's own 0.8), 1.15x open = 0.92 x the hit sphere
        static readonly Color Cyan = new Color(0.72f, 1f, 1f, 1f); // cyan-white (rule 8: player-usable)

        public static BossMarks32 Current;
        readonly SurvivalGame game;
        readonly Transform root;
        readonly Mesh boxMesh;
        readonly Material add;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        readonly List<Transform> pool = new List<Transform>();
        readonly List<Renderer> poolR = new List<Renderer>();
        int used;
        // NIGHT73 core73 (Core73.cs): the open weak point's glow spheres (a second pool: the shells are boxes), the state of the window's opening, the layers' arrays (made once)
        readonly Mesh sphereMesh;
        readonly List<Transform> poolS = new List<Transform>();
        readonly List<Renderer> poolSR = new List<Renderer>();
        int usedS;
        bool wasOpen73;
        float openAt73;
        readonly float[] core73Radius = new float[UnityTuning.Core73Layers], core73Glow = new float[UnityTuning.Core73Layers];

        /// <summary>What was drawn this frame (the reel and the frame check read it): shells / beams, and whether an open window was shown.</summary>
        public int Drawn => used;
        public bool OpenShown { get; private set; }
        public string BossShown { get; private set; } = "";

        BossMarks32(SurvivalGame game, Mesh boxMesh, Material add)
        {
            this.game = game;
            this.boxMesh = boxMesh;
            this.add = add;
            root = new GameObject("NIGHT32 BOSS MARKS").transform;
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere); // NIGHT73 core73: radius 0.5, the glow's discs
            sphereMesh = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
        }

        /// <summary>Boss31View.Render's hook: make the view the first time the row is on, then draw (or hide everything with the row off).</summary>
        public static void Draw(SurvivalGame game, Mesh boxMesh, Material add)
        {
            Tells32.Draw(game, add); // the anti-camp tells: drawn whenever bosses32 is on (a hit with no warning is unfair), whatever the bossMarks row says
            bool on = game != null && game.sim != null && N32Fixes.ViewRowOn(game.sim.cfg, "bossMarks");
            if (!on) { Current?.Hide(); return; }
            if (Current == null || Current.game != game) Current = new BossMarks32(game, boxMesh, add);
            Current.Render(game.sim);
        }

        void Hide()
        {
            for (int i = 0; i < pool.Count; i++) if (pool[i].gameObject.activeSelf) pool[i].gameObject.SetActive(false);
            for (int i = 0; i < poolS.Count; i++) if (poolS[i].gameObject.activeSelf) poolS[i].gameObject.SetActive(false);
            used = 0;
            usedS = 0;
            wasOpen73 = false;
            OpenShown = false;
        }

        /// <summary>NIGHT73 core73: one glow sphere (additive, the shells' own material), its colour set per draw.</summary>
        Transform NextSphere(Color c)
        {
            if (usedS == poolS.Count)
            {
                var g = new GameObject("core73");
                g.transform.SetParent(root, false);
                g.AddComponent<MeshFilter>().sharedMesh = sphereMesh;
                var r = g.AddComponent<MeshRenderer>();
                r.sharedMaterial = add;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                poolS.Add(g.transform);
                poolSR.Add(r);
            }
            Transform t = poolS[usedS];
            block.SetColor(BaseColor, c);
            poolSR[usedS].SetPropertyBlock(block);
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            usedS++;
            return t;
        }

        /// <summary>ROUND 2: where the core glow sits and how big it is: on the face of the head's box that the camera sees (Core73Math.FrontPoint: where the line from the head's centre to the camera leaves the box),
        /// its radius a share of the box's smallest half size (Core73Math.FaceRadius). The camera's position in web space (Unity z is web -z), the sim's eye when there is no camera.</summary>
        Vec3 CorePoint73(Wyrm w, out float outer)
        {
            WyrmBox h = w.boxes[0];
            Vec3 eye = game.Eye;
            if (game.cam != null) { Vector3 p = game.cam.transform.position; eye = new Vec3(p.x, p.y, -p.z); }
            outer = (float)Core73Math.FaceRadius(h.hr, h.hu, h.hf, UnityTuning.Core73FaceShare);
            return Core73Math.FrontPoint(h.c, h.r, h.u, h.f, h.hr, h.hu, h.hf, eye, out int _);
        }

        /// <summary>NIGHT73 core73: the open weak point as a lit core (Core73.cs): UnityTuning.Core73Layers concentric additive spheres ON the head's front face (round 2; the head stays lit cyan, as today), the cyan
        /// role colour (the open shell's own routed colour: ShellRim) x each layer's glow x the slow pulse x the fade-in; no shell box. Returns the outer radius (m).</summary>
        float DrawCore73(Wyrm w)
        {
            Vec3 cc = CorePoint73(w, out float outer);
            Core73Math.Layers(UnityTuning.Core73Layers, UnityTuning.Core73Power, UnityTuning.Core73Peak, core73Radius, core73Glow);
            float k = Core73Math.Pulse(Time.time, UnityTuning.Core73PulseHz, UnityTuning.Core73PulseDepth) * Core73Math.Fade(Time.time - openAt73, UnityTuning.Core73FadeS);
            Vector3 at = U(cc);
            for (int i = 0; i < UnityTuning.Core73Layers; i++)
            {
                Transform t = NextSphere(BossColours63.ShellRim(game.cfg, "wyrm.mark", Cyan, core73Glow[i] * k));
                t.SetPositionAndRotation(at, Quaternion.identity);
                t.localScale = Vector3.one * (2f * outer * core73Radius[i]); // the primitive sphere's radius is 0.5
            }
            return outer;
        }

        /// <summary>NIGHT73 core73: the proof line, once on each open (the row on or off): what was drawn and how big the core and today's box are on this frame's screen (the real camera; "off screen" when the head is
        /// not in front of it).</summary>
        void SayCore73(Wyrm w, bool core73On)
        {
            WyrmBox h = w.boxes[0];
            string box = "off screen", head = "off screen", core = "off screen", inside = "n/a";
            Camera cam = game.cam;
            if (cam != null)
            {
                // the screen rect of the head's box at a swell (the 8 corners through the real camera): today's shell is OpenSwell x it, the head itself 1
                bool Rect(float swell, out float x0, out float y0, out float x1, out float y1)
                {
                    x0 = y0 = float.MaxValue;
                    x1 = y1 = float.MinValue;
                    bool seen = false;
                    for (int sx = -1; sx <= 1; sx += 2)
                        for (int sy = -1; sy <= 1; sy += 2)
                            for (int sz = -1; sz <= 1; sz += 2)
                            {
                                Vec3 corner = h.c + h.r * (sx * h.hr * swell) + h.u * (sy * h.hu * swell) + h.f * (sz * h.hf * swell);
                                Vector3 sp = cam.WorldToScreenPoint(U(corner));
                                if (sp.z <= 0.1f) continue;
                                seen = true;
                                x0 = Mathf.Min(x0, sp.x); x1 = Mathf.Max(x1, sp.x); y0 = Mathf.Min(y0, sp.y); y1 = Mathf.Max(y1, sp.y);
                            }
                    return seen;
                }
                if (Rect(OpenSwell, out float bx0, out float by0, out float bx1, out float by1)) box = Mathf.RoundToInt(bx1 - bx0) + " x " + Mathf.RoundToInt(by1 - by0);
                bool headSeen = Rect(1f, out float hx0, out float hy0, out float hx1, out float hy1);
                if (headSeen) head = Mathf.RoundToInt(hx1 - hx0) + " x " + Mathf.RoundToInt(hy1 - hy0);
                Vec3 cc = CorePoint73(w, out float r);
                Vector3 c0 = cam.WorldToScreenPoint(U(cc));
                if (c0.z > 0.1f)
                {
                    float rpx = Mathf.Abs(cam.WorldToScreenPoint(U(cc) + cam.transform.up * r).y - c0.y);
                    core = Mathf.RoundToInt(2f * rpx).ToString();
                    if (headSeen) inside = c0.x - rpx >= hx0 && c0.x + rpx <= hx1 && c0.y - rpx >= hy0 && c0.y + rpx <= hy1 ? "yes" : "NO";
                }
            }
            float maxStep = 0f;
            Core73Math.Layers(UnityTuning.Core73Layers, UnityTuning.Core73Power, UnityTuning.Core73Peak, core73Radius, core73Glow);
            for (int i = 0; i < UnityTuning.Core73Layers; i++) maxStep = Mathf.Max(maxStep, core73Glow[i] / UnityTuning.Core73Peak);
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            float outerM = (float)Core73Math.FaceRadius(h.hr, h.hu, h.hf, UnityTuning.Core73FaceShare);
            Debug.Log("[core73] wyrm weak point: row " + (core73On ? "ON" : "off") + ", drawn as " + (core73On ? "core" : "box") + " | core on the head's front face " + outerM.ToString("0.00", ci) + " m radius ("
                + UnityTuning.Core73Layers + " layers, largest step " + (maxStep * 100f).ToString("0", ci) + " % of the peak) = " + core + " px across, inside the head's box on screen: " + inside + "; the head's box = " + head
                + " px, today's shell (" + OpenSwell.ToString("0.00", ci) + " x the head) = " + box + " px | pulse " + (core73On ? UnityTuning.Core73PulseHz : OpenPulseHz).ToString("0.0", ci) + " Hz | head lit cyan"
                + (core73On ? " (kept)" : " as today (bossColours63 / wyrmLook47)") + " | frame " + Time.frameCount);
        }

        Transform Next(Color c)
        {
            if (used == pool.Count)
            {
                var g = new GameObject("mark");
                g.transform.SetParent(root, false);
                g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
                var r = g.AddComponent<MeshRenderer>();
                r.sharedMaterial = add;
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
                pool.Add(g.transform);
                poolR.Add(r);
            }
            Transform t = pool[used];
            block.SetColor(BaseColor, c);
            poolR[used].SetPropertyBlock(block);
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            used++;
            return t;
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        static Color Glow(float k) => new Color(Cyan.r * k, Cyan.g * k, Cyan.b * k, 1f);

        /// <summary>A solid shell over an oriented box, `swell` times its size.</summary>
        void Shell(WyrmBox b, float swell, float glow) => Shell(b, swell, Glow(glow));

        /// <summary>NIGHT63 B3c: a shell in a given colour (bossColours63's routed open-window shell).</summary>
        void Shell(WyrmBox b, float swell, Color colour)
        {
            Transform t = Next(colour);
            Vector3 f = U(b.f), u = U(b.u);
            if (f.sqrMagnitude < 1e-8f) f = Vector3.forward;
            if (u.sqrMagnitude < 1e-8f) u = Vector3.up;
            t.SetPositionAndRotation(U(b.c), Quaternion.LookRotation(f, u));
            t.localScale = new Vector3((float)(2 * b.hr) * swell, (float)(2 * b.hu) * swell, (float)(2 * b.hf) * swell);
        }

        /// <summary>The 12 edges of an oriented box as thin beams.</summary>
        void Outline(WyrmBox b, float glow)
        {
            Vec3 c = b.c;
            for (int axis = 0; axis < 3; axis++)
            {
                Vec3 a = axis == 0 ? b.r * b.hr : axis == 1 ? b.u * b.hu : b.f * b.hf;
                Vec3 p = axis == 0 ? b.u * b.hu : axis == 1 ? b.f * b.hf : b.r * b.hr;
                Vec3 q = axis == 0 ? b.f * b.hf : axis == 1 ? b.r * b.hr : b.u * b.hu;
                for (int s = 0; s < 4; s++)
                {
                    Vec3 o = c + p * (s % 2 == 0 ? 1 : -1) + q * (s < 2 ? 1 : -1);
                    Beam(Next(Glow(glow)), U(o - a), U(o + a), EdgeThick);
                }
            }
        }

        static void Beam(Transform t, Vector3 a, Vector3 b, float thick)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            t.SetPositionAndRotation((a + b) * 0.5f, len > 1e-5f ? Quaternion.LookRotation(d / len, Mathf.Abs(d.y) > 0.99f * len ? Vector3.forward : Vector3.up) : Quaternion.identity);
            t.localScale = new Vector3(thick, thick, Mathf.Max(len, thick));
        }

        static WyrmBox Cube(Vec3 c, double half)
        {
            var b = default(WyrmBox);
            b.Orient(new Vec3(0, 0, 1));
            b.c = c;
            b.hr = b.hu = b.hf = half;
            return b;
        }

        float Pulse() => OpenGlow * (0.85f + 0.15f * Mathf.Sin(Time.time * OpenPulseHz * 2 * Mathf.PI));

        void Render(SurvivalSim sim)
        {
            used = 0;
            usedS = 0;
            bool open73 = false; // NIGHT73 core73: the wyrm's window is open this frame
            OpenShown = false;
            BossShown = "";
            Stag31 st = sim.stag31;
            Wyrm w = sim.wyrm24;
            Loom30 lm = sim.loom30;
            Lantern27 ln = sim.lantern27;
            if (st != null && st.Alive && sim.n31.stag31)
            {
                BossShown = "stag";
                Outline(st.box, OutlineGlow);
                // the rear plate: where a hit from behind lands in full (Stag31.Behind: past half the body's length behind its centre)
                bool open = st.BackOpen;
                OpenShown |= open;
                WyrmBox plate = st.box;
                plate.c = st.box.c - st.box.f * (st.box.hf + 0.08);
                plate.hf = 0.08;
                Shell(plate, open ? OpenSwell : 1.0f, open ? Pulse() : SpotGlow * 0.6f);
            }
            if (w != null && w.Alive && sim.n24.wyrm)
            {
                BossShown += (BossShown.Length > 0 ? "+" : "") + "wyrm";
                bool open = sim.wyrm31 != null && sim.wyrm31.Staggered;
                OpenShown |= open;
                for (int i = 1; i < w.PieceCount && i < w.boxes.Length; i++)
                    if (!w.buried[i]) Shell(w.boxes[i], 1.04f, OutlineGlow * 0.5f); // a lift on the body pieces (the n32-r2 frames: the body was a dark band)
                if (!w.buried[0])
                {
                    Outline(w.boxes[0], OutlineGlow);
                    // the head: x2 always, the stagger's open window x2.5 on top. NIGHT63 B3c (bossColours63, builder D): the open window's shell is the
                    // weak cyan through the fight's grade rim with the row on (its cyan-white read lime under THE WYRM's grade); off: exactly as before
                    // NIGHT73 core73 (Core73.cs; the row read every frame): the open window drawn as a lit core (soft cyan glow on the head's back, no shell box); off: the box above, exactly as before
                    bool core73 = Core73.On(game.cfg);
                    if (open)
                    {
                        open73 = true;
                        if (!wasOpen73)
                        {
                            openAt73 = Time.time;
                            SayCore73(w, core73);
                        }
                        if (core73) DrawCore73(w);
                        else Shell(w.boxes[0], OpenSwell, BossColours63.ShellRim(game.cfg, "wyrm.mark", Cyan, Pulse()));
                    }
                    else Shell(w.boxes[0], SpotSwell, SpotGlow);
                }
            }
            wasOpen73 = open73;
            if (lm != null && lm.Alive)
            {
                BossShown += (BossShown.Length > 0 ? "+" : "") + "loom";
                for (int j = 0; j < lm.joints.Length; j++)
                {
                    bool full = lm.FullOn(j);
                    OpenShown |= full;
                    Shell(Cube(lm.joints[j], lm.p.jointRadius * JointShell), full ? JointOpenSwell : 1.0f, full ? Pulse() : SpotGlow); // inside the hit sphere: the mark never promises a hit that misses
                }
                Outline(lm.box, OutlineGlow * 0.6f);
            }
            if (ln != null && ln.Alive && sim.Lantern30On)
            {
                BossShown += (BossShown.Length > 0 ? "+" : "") + "lantern";
                bool open = sim.lantern31 != null && sim.lantern31.CoreOpen;
                OpenShown |= open;
                Outline(ln.boxes[0], OutlineGlow);
                if (open) Shell(ln.boxes[0], OpenSwell, Pulse());
            }
            for (int i = used; i < pool.Count; i++) if (pool[i].gameObject.activeSelf) pool[i].gameObject.SetActive(false);
            for (int i = usedS; i < poolS.Count; i++) if (poolS[i].gameObject.activeSelf) poolS[i].gameObject.SetActive(false);
        }
    }

    /// <summary>NIGHT32 B (design.n32.bosses32; the lead 20:22: "a hit with no on-screen warning is unfair"): the anti-camp tells the other views do not draw.
    ///  - THE LANTERN's camp strike (Lantern31.Camp32): a red-orange ring on the floor at the strike's spot through its tell, closing in and brightening as it nears;
    ///  - THE KILN WYRM's spit globs (Wyrm31.spits: never drawn since NIGHT31): red-orange cubes in flight.
    /// The Loom's flyer-drop mark (Boss31View, while Loom31.DropTelling) and the Stag's shards (Boss31View, pool 24; at most about 6 alive now) draw already.
    /// View only; red-orange is danger (rule 8).</summary>
    public static class Tells32
    {
        const float RingLift = 0.06f;      // m over the floor (no z-fight)
        const float RingThick = 0.04f;     // m: the flat disc's height
        const float RingGain = 1.3f;       // additive brightness at the strike (from 0.5 at the tell's start)
        const float GlobSize = 0.45f;      // m: a spit glob's drawn cube (the sim's hit radius is design.n31.wyrm31.spitRadius 0.7)
        const int GlobPool = 24;           // spit globs drawn at most (3 a volley, 3 s life, a volley every 1.5 s: about 6-9 alive)
        static readonly Color Danger = new Color(1f, 0.353f, 0.122f, 1f); // 0xff5a1f, the Stag's / the Crown's danger hex

        static Transform root, ringT;
        static Renderer ringR;
        static readonly List<Transform> globT = new List<Transform>();
        static readonly List<Renderer> globR = new List<Renderer>();
        static MaterialPropertyBlock block; // made in Build (L-035: no Unity API in field initializers)
        static int BaseColor;
        static SurvivalGame owner;

        /// <summary>What is drawn now (the reel counts frames).</summary>
        public static bool CampRingShown => ringT != null && ringT.gameObject.activeSelf;
        public static int GlobsShown { get; private set; }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        static void Tint(Renderer r, float k)
        {
            block.SetColor(BaseColor, new Color(Danger.r * k, Danger.g * k, Danger.b * k, 1f));
            r.SetPropertyBlock(block);
        }

        static void Build(SurvivalGame game, Material add)
        {
            owner = game;
            block = new MaterialPropertyBlock();
            BaseColor = Shader.PropertyToID("_BaseColor");
            root = new GameObject("NIGHT32 ANTI-CAMP TELLS").transform;
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh disc = cyl.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cyl);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            ringT = Make("lantern camp ring", disc, add, out ringR);
            globT.Clear();
            globR.Clear();
            for (int i = 0; i < GlobPool; i++) { globT.Add(Make("wyrm spit " + i, box, add, out Renderer r)); globR.Add(r); }
        }

        static Transform Make(string name, Mesh mesh, Material add, out Renderer r)
        {
            var g = new GameObject(name);
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            g.SetActive(false);
            return g.transform;
        }

        public static void Draw(SurvivalGame game, Material add)
        {
            SurvivalSim sim = game?.sim;
            bool on = sim != null && sim.n32.bosses32 && N32Fixes.On;
            if (!on && root == null) return;
            if (root == null || owner != game) Build(game, add);
            // the Lantern's camp strike
            Lantern31 l31 = on ? sim.lantern31 : null;
            bool ring = l31 != null && l31.CampTelling32;
            if (ringT.gameObject.activeSelf != ring) ringT.gameObject.SetActive(ring);
            if (ring)
            {
                double prog = l31.CampTellProgress32;
                float d = (float)(2 * l31.campRadius32 * (1.25 - 0.25 * prog));
                ringT.SetPositionAndRotation(U(new Vec3(l31.campAt32.x, l31.campAt32.y + RingLift, l31.campAt32.z)), Quaternion.identity);
                ringT.localScale = new Vector3(d, RingThick, d);
                Tint(ringR, Mathf.Lerp(0.5f, RingGain, (float)prog));
            }
            // the Wyrm's spit globs
            Wyrm31 w31 = on && sim.wyrm24 != null && sim.wyrm24.Alive ? sim.wyrm31 : null;
            int n = w31 != null ? Mathf.Min(w31.spits.Count, GlobPool) : 0;
            for (int i = 0; i < GlobPool; i++)
            {
                bool show = i < n && w31.spits[i].alive;
                if (globT[i].gameObject.activeSelf != show) globT[i].gameObject.SetActive(show);
                if (!show) continue;
                globT[i].SetPositionAndRotation(U(w31.spits[i].pos), Quaternion.identity);
                globT[i].localScale = new Vector3(GlobSize, GlobSize, GlobSize);
                Tint(globR[i], RingGain);
            }
            GlobsShown = n;
        }
    }
}
