// NIGHT37 builder O (loom37; NIGHT37.md O1, BOSSES.md "L4 THE LOOM"): what the player SEES of THE LOOM's fight (the canyon itself is the level's
// boxes: the towers' ride faces carry RideTrim36's cyan).
// The lead adds two lines in SurvivalGame.Boss26.cs:  Boss26InitViews:   `Loom37View.Current = new Loom37View(this);`
//                                                      Boss26RenderViews: `Loom37View.Current?.Render(alpha);`
//  - THE LOOM (while sim.loom37 owns the fight): a dark iron body slung between six long legs (hip -> knee -> a foot on the canyon wall);
//    each knee a glowing sphere: CYAN for the leg's live pair (a grapple node and the weak point), pale for the others, warmer with each nail
//    in it; a torn knee is gone and its lower leg hangs limp. The spinneret under the body is shut iron until the CYAN window (Boss37Glow).
//    Pale threads run from the body to the live thread anchors; an anchor about to be cut pulses red-orange (then its ring is gone: the sim
//    parks the node).
//  - The attacks: the THREAD LASH's line red-orange through its wind-up, bright when it strikes; the SPOOL a yellow ball (cyan once parried);
//    the WEB WALL a red-orange sheet between the towers (faint while it winds up, solid while it stands).
//  - The stagger bar under the boss bar (Boss37StaggerBar.Show).
//  - NIGHT38 O2 loom38 (UnityTuning.Loom38; off with -skygrindOff38): the live knees breathe, each stuck nail is a warm pip circling its knee,
//    a knee ready to tear flashes white, two red-orange slits on the body's sides.
// Colours and sizes: UnityTuning.Loom37. Nothing is built or shown with design.n37.loom37 off (no fight, no level row).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        public static Loom37View Current;

        readonly SurvivalGame game;
        readonly Transform root, bodyT, spinT;
        readonly Mesh boxMesh, ballMesh;
        readonly Material add, iron, ironDark;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        readonly Transform[] upper = new Transform[6], lower = new Transform[6], kneeT = new Transform[6];
        readonly Renderer[] kneeR = new Renderer[6];
        readonly List<Transform> threadT = new List<Transform>(), cutT = new List<Transform>(), spoolT = new List<Transform>();
        readonly List<Renderer> threadR = new List<Renderer>(), cutR = new List<Renderer>(), spoolR = new List<Renderer>();
        readonly Transform lashT, webT;
        readonly Renderer lashR, webR;
        // NIGHT38 O2 loom38 (UnityTuning.Loom38): the nail pips, the body's red-orange slits
        readonly List<Transform> pipT = new List<Transform>();
        readonly List<Renderer> pipR = new List<Renderer>();
        readonly Transform[] slitT = new Transform[2];
        readonly Renderer[] slitR = new Renderer[2];
        /// <summary>For the reels (loom38): nail pips on screen, knees flashing ready to tear.</summary>
        public int PipsShown, KneesReady;
        readonly Light underLight;
        Boss37Glow glow;
        float t;

        /// <summary>For the reels: the body drawn, knees lit live, nails shown, the lash / web / spools on screen, anchors pulsing.</summary>
        public bool BodyShown => root.gameObject.activeSelf;
        public int KneesLive, NailsShown, SpoolsShown, CutsPulsing;
        public bool LashShown, WebShown;
        public float GlowLevel => glow != null ? glow.Level : 0;

        public Loom37View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballMesh = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
            add = FxDraw.Additive(game.mats.neon, "Loom37 glow");
            iron = Opaque("loom37 iron", UnityTuning.Loom37IronHex);
            ironDark = Opaque("loom37 iron dark", UnityTuning.Loom37IronDarkHex);
            root = new GameObject("NIGHT37 THE LOOM").transform;
            bodyT = Solid(root, "body", boxMesh, iron).transform;
            bodyT.localScale = new Vector3(UnityTuning.Loom37BodyW, UnityTuning.Loom37BodyH, UnityTuning.Loom37BodyL);
            spinT = Solid(root, "spinneret", ballMesh, ironDark).transform;
            spinT.localScale = Vector3.one * (2 * UnityTuning.Loom37SpinR);
            for (int i = 0; i < 6; i++)
            {
                upper[i] = Solid(root, "leg upper " + i, boxMesh, iron).transform;
                lower[i] = Solid(root, "leg lower " + i, boxMesh, ironDark).transform;
                kneeR[i] = Glow(root, "knee " + i, ballMesh, out kneeT[i]);
            }
            for (int i = 0; i < 2; i++) slitR[i] = Glow(bodyT, "loom38 slit " + i, boxMesh, out slitT[i]);
            lashR = Glow(root, "lash", boxMesh, out lashT);
            webR = Glow(root, "web wall", boxMesh, out webT);
            var lg = new GameObject("loom37 light");
            lg.transform.SetParent(root, false);
            underLight = lg.AddComponent<Light>();
            underLight.type = LightType.Point;
            underLight.shadows = LightShadows.None;
            underLight.range = UnityTuning.Loom37LightRange;
            underLight.color = Look.Hex(UnityTuning.Loom37KneeIdleHex);
            underLight.intensity = UnityTuning.Loom37LightIntensity * UnityTuning.LightScale;
            glow = new Boss37Glow(game, spinT, UnityTuning.Loom37GlowR);
            for (int i = 0; i < 2; i++) // loom38: on each long side of the body (its local unit cube), a thin red-orange slit
            {
                slitT[i].localPosition = new Vector3(i == 0 ? -0.505f : 0.505f, 0.15f, 0f);
                slitT[i].localScale = new Vector3(0.02f, UnityTuning.Loom38SlitH / UnityTuning.Loom37BodyH, UnityTuning.Loom38SlitW / UnityTuning.Loom37BodyL);
                Tint(slitR[i], Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Loom38SlitGain));
            }
            Init40(); // NIGHT40 B4 (loom40): the tell glow, the practice knee (Loom37View.N40.cs)
            Init41(); // NIGHT41 B8 (sweep41): the floor sweep's decal, top bar and line (Loom37View.N41.cs)
            Init47(); // NIGHT47 L (loom47): the dive / cage tells, the seam (Loom37View.N47.cs)
            root.gameObject.SetActive(false);
        }

        Material Opaque(string name, uint hex)
        {
            var mat = new Material(game.mats.stone) { name = name };
            Color c = Look.Hex(hex);
            if (mat.HasProperty(BaseColor)) mat.SetColor(BaseColor, c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
            return Light54.StoneOrLit(game, mat, "Loom37View", name, hex); // NIGHT54 light54: Stone.shader reads the vertex colour, not _BaseColor (white): the intended hex on URP Lit with the row on
        }

        GameObject Solid(Transform parent, string name, Mesh mesh, Material mat)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            return g;
        }

        Renderer Glow(Transform parent, string name, Mesh mesh, out Transform tr)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            tr = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        /// <summary>A stretched box from a to b (a leg segment, a thread, the lash).</summary>
        static void Span(Transform tr, Vector3 a, Vector3 b, float thick)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f) { tr.gameObject.SetActive(false); return; }
            if (!tr.gameObject.activeSelf) tr.gameObject.SetActive(true);
            tr.position = (a + b) * 0.5f;
            tr.rotation = Quaternion.LookRotation(d / len, Mathf.Abs(d.y / len) > 0.99f ? Vector3.right : Vector3.up);
            tr.localScale = new Vector3(thick, thick, len);
        }

        void Pool(List<Transform> ts, List<Renderer> rs, int n, string name, Mesh mesh)
        {
            while (ts.Count < n)
            {
                rs.Add(Glow(root, name + " " + ts.Count, mesh, out Transform tr));
                ts.Add(tr);
            }
            for (int i = n; i < ts.Count; i++) if (ts[i].gameObject.activeSelf) ts[i].gameObject.SetActive(false);
        }

        public void Render(double alpha)
        {
            BossTell63.Draw(game, root, BossAttackClock63.Loom); // NIGHT63 G bossTell63 (BossTell63.cs): the danger column through a non-parry wind-up (nothing with the row off)
            float dt = Time.deltaTime;
            t += dt;
            SurvivalSim sim = game.sim;
            Loom37 L = sim?.loom37;
            Render40Always(sim); // NIGHT40 B4: o2's practice knee (loom40 only)
            bool on = L != null && sim.loom37Owns && L.Alive;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on)
            {
                glow?.Set(false, 1, dt);
                return;
            }
            Boss37StaggerBar.Show(L.brain);
            DrawMotion63(sim, L); // NIGHT63 V bossMotion63 (Loom37View.N63V.cs): this frame's springs, wind-up, flinch, slump, breath for the shuttle (off: Rest)
            Vector3 c = U(L.centre);
            bodyT.position = c;
            spinT.position = U(L.Spinneret);
            underLight.transform.position = c + Vector3.down * 3f;
            glow.Render(L.brain, dt);
            Render40(sim, L, c, dt); // NIGHT40 B4: the pose, the tell glow, the pull-away anchors (loom40 only; off: nothing)
            Render41(sim, L); // NIGHT41 B8 (sweep41): the floor sweep's decal (only while it tells or passes; off: it never does)
            // the legs and knees
            KneesLive = NailsShown = KneesReady = 0;
            bool loom38 = !N38Blocks.ForceOff; // NIGHT38 O2 loom38 (UnityTuning.Loom38); -skygrindOff38: NIGHT37's look
            for (int i = 0; i < 2; i++) if (slitT[i].gameObject.activeSelf != loom38) slitT[i].gameObject.SetActive(loom38);
            int pips = 0;
            if (loom38) for (int i = 0; i < 6; i++) if (!L.knees[i].torn) pips += Mathf.Min(L.knees[i].nails, L.p.nailsToTear);
            Pool(pipT, pipR, pips, "loom38 nail pip", ballMesh);
            int pi = 0;
            float breath = 1f + UnityTuning.Loom38KneeBreathAmp * Mathf.Sin(t * UnityTuning.Loom38KneeBreathHz * 2f * Mathf.PI);
            float ready = 0.5f + 0.5f * Mathf.Sin(t * UnityTuning.Loom38ReadyHz * 2f * Mathf.PI);
            for (int i = 0; i < 6; i++)
            {
                Loom37Knee k = L.knees[i];
                Vector3 hip = U(k.hip), knee = U(k.at), foot = U(k.foot);
                Span(upper[i], hip, knee, UnityTuning.Loom37LegThick);
                if (k.torn)
                {
                    Span(lower[i], knee, knee + Vector3.down * 9f, UnityTuning.Loom37LegThick * 0.8f); // limp
                    if (kneeT[i].gameObject.activeSelf) kneeT[i].gameObject.SetActive(false);
                    continue;
                }
                Span(lower[i], knee, foot, UnityTuning.Loom37LegThick);
                if (!kneeT[i].gameObject.activeSelf) kneeT[i].gameObject.SetActive(true);
                kneeT[i].position = knee;
                kneeT[i].localScale = Vector3.one * (2f * (float)L.p.kneeRadius * UnityTuning.Loom37KneeScale);
                bool live = L.ActiveKnee(i) && !L.Open;
                if (live) KneesLive++;
                if (k.nails > 0) NailsShown++;
                Color col = live ? Look.Hex(UnityTuning.Loom37KneeLiveHex, UnityTuning.Loom37KneeLiveGain) : BossColours63.PartRim(game.cfg, "loom.knees", Look.Hex(UnityTuning.Loom37KneeIdleHex, UnityTuning.Loom37KneeIdleGain)); // NIGHT63 B3: the weak point's cyan, dim, with bossColours63; B3c: through the fight's grade rim (its filter turned the cyan blue)
                if (k.nails > 0)
                {
                    float share = Mathf.Clamp01(k.nails / (float)Mathf.Max(1, L.p.nailsToTear));
                    col = Color.Lerp(col, BossColours63.PartRim(game.cfg, "loom.nails", Look.Hex(UnityTuning.Loom37NailHex, UnityTuning.Loom37KneeLiveGain)), share * 0.6f) * (1f + UnityTuning.Loom37NailGainPer * Mathf.Min(k.nails, L.p.nailsToTear)); // NIGHT63 B3
                }
                if (loom38)
                {
                    if (live) kneeT[i].localScale *= breath; // the pair to nail breathes
                    if (k.nails >= L.p.nailsToTear) // ready to tear: pull away now
                    {
                        KneesReady++;
                        col = Color.Lerp(col, Look.Hex(UnityTuning.Loom38ReadyHex, UnityTuning.Loom38ReadyGain), ready);
                    }
                    int n = Mathf.Min(k.nails, L.p.nailsToTear);
                    float orbit = (float)L.p.kneeRadius * UnityTuning.Loom38PipOrbit;
                    for (int j = 0; j < n && pi < pipT.Count; j++, pi++) // the stuck nails as warm pips circling the knee: the count to a tear
                    {
                        float a = (t * UnityTuning.Loom38PipTurns + j / (float)Mathf.Max(1, L.p.nailsToTear)) * 2f * Mathf.PI;
                        if (!pipT[pi].gameObject.activeSelf) pipT[pi].gameObject.SetActive(true);
                        pipT[pi].position = knee + new Vector3(Mathf.Cos(a) * orbit, 0.35f * orbit, Mathf.Sin(a) * orbit);
                        pipT[pi].localScale = Vector3.one * (2f * UnityTuning.Loom38PipR);
                        Tint(pipR[pi], BossColours63.PartRim(game.cfg, "loom.nails", Look.Hex(UnityTuning.Loom37NailHex, UnityTuning.Loom38PipGain))); // NIGHT63 B3 (B3c: through the fight's grade rim)
                    }
                }
                Tint(kneeR[i], col);
            }
            PipsShown = pi;
            // the threads to the live anchors, and the anchors about to be cut
            int nt = 0, nc = 0;
            foreach (var a in sim.loom37Anchors) { if (!a.dark) nt++; if (sim.Loom37Pulse(a) > 0) nc++; }
            Pool(threadT, threadR, nt, "thread", boxMesh);
            Pool(cutT, cutR, nc, "cut", ballMesh);
            int ti = 0, ci = 0;
            Color thread = Look.Hex(UnityTuning.Loom37ThreadHex, UnityTuning.Loom37ThreadGain);
            foreach (var a in sim.loom37Anchors)
            {
                if (a.dark) continue;
                Span(threadT[ti], c, U(a.at), UnityTuning.Loom37ThreadThick);
                Tint(threadR[ti], thread);
                ti++;
                double pulse = sim.Loom37Pulse(a);
                if (pulse <= 0) continue;
                if (!cutT[ci].gameObject.activeSelf) cutT[ci].gameObject.SetActive(true);
                cutT[ci].position = U(a.at);
                cutT[ci].localScale = Vector3.one * (2f * UnityTuning.Loom37CutR * (1f + 0.4f * (float)pulse));
                float blink = 0.5f + 0.5f * Mathf.Sin(t * UnityTuning.Loom37CutHz * 2f * Mathf.PI);
                Tint(cutR[ci], Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Loom37TellGain + (UnityTuning.Loom37LiveGain - UnityTuning.Loom37TellGain) * blink));
                ci++;
            }
            CutsPulsing = nc;
            // the lash's line
            LashShown = L.LashLive;
            if (L.LashLive)
            {
                Vector3 a = U(L.lashFrom), b = U(L.lashTo);
                Vector3 dir = (b - a).normalized;
                Span(lashT, a, a + dir * (float)L.p.lashLength, UnityTuning.Loom37LashThick * (L.brain.state == Boss37State.Active ? 2f : 1f));
                float g = L.brain.state == Boss37State.Active ? UnityTuning.Loom37LiveGain : UnityTuning.Loom37TellGain * (0.4f + 0.6f * (float)L.brain.Progress);
                Tint(lashR, Look.Hex(UnityTuning.Loom37DangerHex, g));
            }
            else if (lashT.gameObject.activeSelf) lashT.gameObject.SetActive(false);
            // the web wall
            WebShown = L.webPair >= 0 && L.webPair < sim.loom37Webs.Count;
            if (WebShown)
            {
                var w = sim.loom37Webs[L.webPair];
                Vector3 mn = U(w.min), mx = U(w.max);
                if (!webT.gameObject.activeSelf) webT.gameObject.SetActive(true);
                webT.position = (mn + mx) * 0.5f;
                webT.rotation = Quaternion.identity;
                webT.localScale = new Vector3(Mathf.Abs(mx.x - mn.x), Mathf.Abs(mx.y - mn.y), Mathf.Max(0.3f, Mathf.Abs(mx.z - mn.z)));
                float g = L.WebSolid ? UnityTuning.Loom37LiveGain * 0.5f : UnityTuning.Loom37TellGain * (0.25f + 0.75f * (float)L.brain.Progress) * (0.6f + 0.4f * Mathf.Sin(t * 10f));
                Tint(webR, Look.Hex(UnityTuning.Loom37DangerHex, g));
            }
            else if (webT.gameObject.activeSelf) webT.gameObject.SetActive(false);
            // the spools
            Pool(spoolT, spoolR, L.spools.Count, "spool", ballMesh);
            for (int i = 0; i < L.spools.Count; i++)
            {
                Loom37Spool s = L.spools[i];
                if (!spoolT[i].gameObject.activeSelf) spoolT[i].gameObject.SetActive(true);
                spoolT[i].position = U(s.pos);
                spoolT[i].localScale = Vector3.one * (2f * UnityTuning.Loom37SpoolR);
                Tint(spoolR[i], SpoolColor47(s)); // NIGHT47 L: a RED bobbin red-orange; else NIGHT46's spool colours (Loom37View.N47.cs)
            }
            SpoolsShown = L.spools.Count;
            Render47(sim, L); // NIGHT47 L (loom47): the dive / cage tells, the seam's yellow (off: nothing shows)
            Look47Render(sim, L, dt); // NIGHT47 L (loomLook47, a view row): the shuttle, the warp, the springs (off: nothing built)
            Weak74Loom(L); // NIGHT74 B weak74 (Loom37View.Weak74.cs): the probe's boxes for the boss63 reel (the idle knees' colour is BossColours63's)
            TripoLoom(); // PG3 tripo74: THE LOOM's Tripo model over the body (Loom37View.Tripo.cs; the legs and knees stay; nothing when its file is missing or the row is off)
        }

        partial void Weak74Loom(Loom37 L);
    }
}
