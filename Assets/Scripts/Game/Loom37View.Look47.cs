// NIGHT47 builder L (loomLook47, a VIEW row: NightRows.ViewOn(cfg, "loomLook47"); NIGHT47.md G36 "a much better design, colors that match its theme",
// G37 "far more fluid animations"; L-cards.md section 1 Silhouette / Palette / Animation): THE LOOM's new look on Loom37View. View only: the sim's
// knees, spinneret and hit spheres are the truth; nothing here moves a hit box (the body's drawn offset moves only the drawn body and hips).
//  - THE SHUTTLE: the iron body narrowed, with a pointed nose at each end along its long axis (triangles = danger), a brass spool drum on its back
//    (the hub the threads come off), two lamps on the front nose: amber at rest, red-orange through a RED tell, yellow through a YELLOW tell (the
//    tell's colour on the boss itself, one colour = one meaning).
//  - THE WARP: WarpThreads threads from the drum up to the roof (the loom is strung in): pale amber at rest, red-orange while an attack runs; each
//    thread is a sprung line that is PLUCKED (a decaying sideways wave) on every tell's start, every tear and every hit.
//  - THE PHASES (Gabriel's recolour): phase 2 the brass runs hot amber and the lamps red-orange at rest; phase 3 the warp burns red-orange end to end.
//  - MOTION (NIGHT47 section 3's rules, frame-rate independent springs): ANTICIPATION (through a tell's first 40 % the body sinks AnticipM and
//    pulls back), the STRIKE (on the active step it snaps forward), OVERSHOOT and SETTLE (a damped spring back to rest), the HIT FLINCH (a 60-100 ms
//    recoil spring on every bossHit, away from the hit), a ROLL LAG on every glide (secondary motion: the body leans into its velocity and settles),
//    a BREATH (the idle bob stays the sim's knees' own). REDUCED MOTION stills the springs (the body at rest, the colours kept).
// Numbers and colours: UnityTuning.Loom47.cs (Look47*). Row off (or -skygrindOff47): nothing here is built or drawn.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        Transform look47Root, noseFT, noseBT, drumT, lampLT, lampRT, drumBandT;
        // round 4 (the lead's n47-r2 shuttle frames: "a box body ... iron-grey from end to end": the body box's property-block tint does not show on
        // the stone material, and every top-side part is hidden from below): THE SHUTTLE is its own mesh now, a hexagonal prism pointed at both
        // ends, in its own bronze material (a material colour, as the iron's); brass glow BANDS round it (seen from any side), brass COLLARS at the
        // six hips; the lamp eyes stand out in front of the front point
        Transform hullT;
        Renderer hullR;
        Mesh hullMesh, hullHotMesh, bandMesh, drumMesh;
        Material bronze47, bronzeHot47;
        readonly Transform[] bandT = new Transform[UnityTuning.Look47Bands], collarT = new Transform[6];
        readonly Renderer[] bandR = new Renderer[UnityTuning.Look47Bands], collarR = new Renderer[6];
        Renderer bodyRendWas;
        Renderer noseFR, noseBR, drumR, lampLR, lampRR, drumBandR;
        // round 2 (the lead's n47-r0d frame: "its body is grey: level 4's brass / amber is not visible; the legs did not step"): brass glow strips
        // along the body's top edges and a lit band round the drum; the feet PLANT on the walls and STEP in arcs (view only)
        readonly Transform[] stripT = new Transform[2];
        readonly Renderer[] stripR = new Renderer[2];
        readonly Vector3[] footPlant = new Vector3[6], footFrom = new Vector3[6], footTo = new Vector3[6];
        readonly float[] stepT = { -1, -1, -1, -1, -1, -1 };
        bool feetInit;
        /// <summary>For the reels / frames (loomLook47): steps taken since the fight began (a foot that lifted and planted again).</summary>
        public int Steps47;
        readonly Transform[] warpT = new Transform[UnityTuning.Look47WarpThreads * UnityTuning.Look47WarpSegs];
        readonly Renderer[] warpR = new Renderer[UnityTuning.Look47WarpThreads * UnityTuning.Look47WarpSegs];
        Material brass;
        bool look47Hooked;
        // springs (frame time): the body's drawn offset (sink / snap / flinch) and roll, the warp's pluck
        Vector3 offPos, offVel;
        float roll, rollVel, pluck;
        Vector3 flinchDir;
        Vector3 lastCentre;
        Boss37State lastState;
        int lastTorn = -1;

        /// <summary>For the reels / frames (loomLook47): the new look drawn now, the warp's pluck, the body's drawn offset (m).</summary>
        public bool Look47Shown;
        public float Look47Pluck, Look47Offset;

        bool Look47On() => Force47 >= 0 ? Force47 == 1 : NightRows.ViewOn(game.cfg, "loomLook47"); // NIGHT48 L: Force47, the reel's blind-pair switch (Loom37View.Hull48.cs)

        void Look47Build()
        {
            look47Root = new GameObject("loomLook47").transform;
            look47Root.SetParent(root, false);
            brass = Opaque("loom47 brass", UnityTuning.Look47BrassHex);
            // SKYGRIND/Stone draws VERTEX colours (its albedo is i.color: the material's _BaseColor is never read; MEASURED by reading
            // Assets/Shaders/Stone.shader after the r2 frames showed every tinted part grey), so the bronze and the brass are in the meshes
            hullMesh = Colored(HexMesh(UnityTuning.Look47TipShare, true), Look.Hex(UnityTuning.Look47BodyHex));
            hullHotMesh = Colored(HexMesh(UnityTuning.Look47TipShare, true), Look.Hex(UnityTuning.Look47BodyHotHex));
            drumMesh = Colored(Object.Instantiate(ballMesh), Look.Hex(UnityTuning.Look47BrassHex));
            bandMesh = HexMesh(0f, false);
            bronze47 = Opaque("loom47 bronze", UnityTuning.Look47BodyHex);
            bronzeHot47 = Opaque("loom47 bronze hot", UnityTuning.Look47BodyHotHex);
            hullT = Solid(look47Root, "loom47 shuttle hull", hullMesh, bronze47).transform;
            hullR = hullT.GetComponent<Renderer>();
            for (int i = 0; i < bandT.Length; i++) bandR[i] = Glow(look47Root, "loom47 brass band " + i, bandMesh, out bandT[i]);
            for (int i = 0; i < 6; i++) collarR[i] = Glow(look47Root, "loom47 brass collar " + i, ballMesh, out collarT[i]);
            noseFT = Solid(look47Root, "loom47 nose front", boxMesh, iron).transform;
            noseBT = Solid(look47Root, "loom47 nose back", boxMesh, iron).transform;
            noseFR = noseFT.GetComponent<Renderer>();
            noseBR = noseBT.GetComponent<Renderer>();
            drumT = Solid(look47Root, "loom47 drum", drumMesh, brass).transform; // the brass in its vertex colours (the Stone shader)
            drumR = drumT.GetComponent<Renderer>();
            lampLR = Glow(look47Root, "loom47 lamp L", ballMesh, out lampLT);
            lampRR = Glow(look47Root, "loom47 lamp R", ballMesh, out lampRT);
            for (int i = 0; i < warpT.Length; i++) warpR[i] = Glow(look47Root, "loom47 warp " + i, boxMesh, out warpT[i]);
            for (int i = 0; i < 2; i++) stripR[i] = Glow(look47Root, "loom47 brass strip " + i, boxMesh, out stripT[i]);
            drumBandR = Glow(look47Root, "loom47 drum band", ballMesh, out drumBandT);
            feetInit = false;
            if (!look47Hooked && game.events != null)
            {
                look47Hooked = true;
                game.events.On<BossEvent>("bossHit", e =>
                {
                    if (e == null || e.boss != "loom37" || look47Root == null) return;
                    Loom37 L = game.sim?.loom37;
                    if (L == null) return;
                    Vector3 eye = U(game.sim.mover.position);
                    Vector3 d = U(L.centre) - eye;
                    flinchDir = d.sqrMagnitude > 1e-4f ? d.normalized : Vector3.forward;
                    offVel += flinchDir * UnityTuning.Look47FlinchKick;
                    pluck = Mathf.Max(pluck, UnityTuning.Look47PluckHit);
                });
            }
        }

        static float Spring(ref float x, ref float v, float target, float hz, float zeta, float dt)
        {
            // a semi-implicit damped spring (stable at any frame time; NIGHT5's closed-form springs are the reference)
            float w = 2f * Mathf.PI * hz;
            v += (-2f * zeta * w * v - w * w * (x - target)) * dt;
            x += v * dt;
            return x;
        }

        /// <summary>A hexagonal prism along z (unit: x, y in -0.5..0.5, z in -0.5..0.5), pointed at both ends when tipShare > 0 (each tip takes that
        /// share of the length), open-ended when capped is false (a band). Flat-shaded (each face its own vertices).</summary>
        /// <summary>The mesh with every vertex coloured c (the Stone shader's albedo).</summary>
        static Mesh Colored(Mesh m, Color c)
        {
            var cs = new Color[m.vertexCount];
            for (int i = 0; i < cs.Length; i++) cs[i] = c;
            m.colors = cs;
            return m;
        }

        static Mesh HexMesh(float tipShare, bool capped)
        {
            var v = new System.Collections.Generic.List<Vector3>();
            var t = new System.Collections.Generic.List<int>();
            float zr = 0.5f - tipShare;
            Vector3 R(int k, float z) { float a = k * Mathf.PI / 3f + Mathf.PI / 6f; return new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f, z); }
            // both windings, each with its own vertices (two-sided whatever the material culls; flat normals each side)
            void Tri(Vector3 a, Vector3 b, Vector3 c) { int i = v.Count; v.Add(a); v.Add(b); v.Add(c); t.Add(i); t.Add(i + 1); t.Add(i + 2); i = v.Count; v.Add(a); v.Add(c); v.Add(b); t.Add(i); t.Add(i + 1); t.Add(i + 2); }
            for (int k = 0; k < 6; k++)
            {
                Vector3 a0 = R(k, -zr), a1 = R(k + 1, -zr), b0 = R(k, zr), b1 = R(k + 1, zr);
                Tri(a0, b0, b1); Tri(a0, b1, a1); // the side
                if (capped)
                {
                    Tri(b0, new Vector3(0, 0, 0.5f), b1); // the front point
                    Tri(a1, new Vector3(0, 0, -0.5f), a0); // the back point
                }
            }
            var m = new Mesh { name = capped ? "loom47 shuttle" : "loom47 band" };
            m.SetVertices(v);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static void Spring3(ref Vector3 x, ref Vector3 v, Vector3 target, float hz, float zeta, float dt)
        {
            float w = 2f * Mathf.PI * hz;
            v += (-2f * zeta * w * v - w * w * (x - target)) * dt;
            x += v * dt;
        }

        /// <summary>Every fight frame, at the end of Render (after the knees and Render47): the shuttle, the warp, the motion.</summary>
        void Look47Render(SurvivalSim sim, Loom37 L, float dt)
        {
            bool on = Look47On();
            Look47Shown = on;
            if (!on) { if (look47Root != null && look47Root.gameObject.activeSelf) look47Root.gameObject.SetActive(false); if (bodyRendWas != null) { bodyRendWas.enabled = true; bodyRendWas = null; } Hull48Off(); return; } // NIGHT48 L: loomHull48 needs this row
            if (look47Root == null) Look47Build();
            if (!look47Root.gameObject.activeSelf) look47Root.gameObject.SetActive(true);
            bool calm = game.cfg.movement.settings.reducedMotion;
            dt = Mathf.Min(dt, 0.05f);
            Boss37Brain br = L.brain;
            Boss37Attack a = br.Current;
            float prog = (float)br.Progress;
            int leg = Mathf.Clamp(br.leg, 1, 3);

            // MOTION: the target offset from the tell / strike, then the springs
            Vector3 target = Vector3.zero;
            Vector3 me = U(sim.mover.position), c = U(L.centre);
            if (m63.on) c += m63.offset; // NIGHT63 V bossMotion63 (Loom37View.N63V.cs): the shuttle's centre moved by the layer (off: the sim's)
            Vector3 toMe = me - c; toMe.y = 0;
            Vector3 fwd = toMe.sqrMagnitude > 1e-4f ? toMe.normalized : Vector3.forward;
            if (br.state == Boss37State.Tell) target = Vector3.down * (UnityTuning.Look47AnticipM * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(prog / 0.4f))) - fwd * UnityTuning.Look47PullBackM * Mathf.Clamp01(prog / 0.4f);
            if (br.state == Boss37State.Active && lastState == Boss37State.Tell) offVel += fwd * UnityTuning.Look47StrikeKick + Vector3.up * UnityTuning.Look47StrikeKick * 0.4f;
            if (br.state == Boss37State.Tell && lastState != Boss37State.Tell) pluck = Mathf.Max(pluck, UnityTuning.Look47PluckTell);
            int torn = L.Torn;
            if (lastTorn >= 0 && torn > lastTorn) { pluck = Mathf.Max(pluck, UnityTuning.Look47PluckTear); offVel += Vector3.up * UnityTuning.Look47StrikeKick; }
            lastTorn = torn;
            lastState = br.state;
            if (calm) { offPos = Vector3.zero; offVel = Vector3.zero; }
            else Spring3(ref offPos, ref offVel, target, UnityTuning.Look47SpringHz, UnityTuning.Look47SpringZeta, dt);
            // the roll lag: lean into the glide's velocity across the body
            Vector3 vel = dt > 1e-5f ? (c - lastCentre) / dt : Vector3.zero;
            lastCentre = c;
            float lean = calm ? 0f : Mathf.Clamp(-vel.x * UnityTuning.Look47RollPerMs, -UnityTuning.Look47RollMax, UnityTuning.Look47RollMax);
            if (calm) { roll = 0; rollVel = 0; } else Spring(ref roll, ref rollVel, lean, UnityTuning.Look47RollHz, UnityTuning.Look47SpringZeta, dt);
            pluck = Mathf.Max(0f, pluck - dt * UnityTuning.Look47PluckDecay);
            Look47Pluck = pluck;
            Look47Offset = offPos.magnitude;

            // the drawn body: offset and rolled; the hips follow it (the upper legs re-spanned from the moved hips to the sim's knees)
            Vector3 bc = c + offPos;
            Quaternion face = Quaternion.LookRotation(fwd, Vector3.up) * Quaternion.Euler(0, 0, roll);
            bodyT.position = bc;
            bodyT.rotation = face;
            bodyT.localScale = new Vector3(UnityTuning.Loom37BodyW * UnityTuning.Look47BodyNarrow, UnityTuning.Loom37BodyH, UnityTuning.Loom37BodyL);
            // the body itself warm (the lead's r0d / r1 frames: "grey"): a dark bronze-iron, the noses too; the strips carry the brass at 40 m
            Renderer bodyRend = bodyT.GetComponent<Renderer>();
            if (bodyRend != null && bodyRend.enabled) { bodyRend.enabled = false; bodyRendWas = bodyRend; } // the hull draws the body (its hit sphere is the sim's)
            hullT.position = bc;
            hullT.rotation = face;
            hullT.localScale = new Vector3(UnityTuning.Loom37BodyW * UnityTuning.Look47BodyNarrow, UnityTuning.Loom37BodyH, UnityTuning.Loom37BodyL * UnityTuning.Look47HullLen);
            hullT.GetComponent<MeshFilter>().sharedMesh = leg >= 2 ? hullHotMesh : hullMesh;
            noseFT.gameObject.SetActive(false);
            noseBT.gameObject.SetActive(false);
            for (int i = 0; i < 6; i++)
            {
                Loom37Knee k = L.knees[i];
                Span(upper[i], U(k.hip) + offPos, U(k.at), UnityTuning.Loom37LegThick);
            }
            // THE STEPS (the procedural-spider rule, L.md: a foot keeps its plant until its target is StepDist away, then steps in an arc over StepS
            // with a lift, an overshoot and a settle; the legs step in two diagonal groups that never both lift at once)
            if (!feetInit) { for (int i = 0; i < 6; i++) { footPlant[i] = U(L.knees[i].foot); stepT[i] = -1; } feetInit = true; Steps47 = 0; }
            bool h48 = Hull48On(); // NIGHT48 L (loomHull48): the gait's lead (Loom37View.Hull48.cs Lead48)
            for (int i = 0; i < 6; i++)
            {
                Loom37Knee k = L.knees[i];
                // the idle shuffle (round 2b, the lead's n47-r1 frame: "its feet don't step": the sim's feet move only when the body glides): each foot's
                // goal sways slowly along the wall, so a resting machine re-plants a foot every few seconds (a breathing idle, never a still prop)
                Vector3 footGoal = U(k.foot) + Vector3.forward * (calm ? 0f : Mathf.Sin(t * UnityTuning.Look47ShuffleHz * 2f * Mathf.PI + i * 1.9f) * UnityTuning.Look47ShuffleM);
                if (h48 && !calm) footGoal = Lead48(i, footGoal, dt);
                goalNow48[i] = footGoal;
                footGoal += forcedGoal48[i]; // NIGHT48 L: the reel's forced step (a448's strip; zero in a game), after the lead's velocity
                int group = ((i / 2) + (i % 2)) & 1;
                if (stepT[i] < 0)
                {
                    bool otherLifting = false;
                    for (int j = 0; j < 6; j++) if (j != i && stepT[j] >= 0 && ((((j / 2) + (j % 2)) & 1) != group)) otherLifting = true;
                    bool leash48 = h48 && (footGoal - footPlant[i]).magnitude > UnityTuning.Hull48LeashM; // NIGHT48 L: a foot far behind steps now (Hull48.cs)
                    if (!calm && (!otherLifting || leash48) && (footGoal - footPlant[i]).magnitude > (h48 ? StepDist48(i) : UnityTuning.Look47StepDist)) { footFrom[i] = footPlant[i]; footTo[i] = footGoal; stepT[i] = 0; if (h48) Step48Begin(i); }
                    if (calm) footPlant[i] = footGoal;
                }
                Vector3 foot = footPlant[i];
                if (stepT[i] >= 0)
                {
                    stepT[i] += dt / (h48 ? stepDur48[i] : UnityTuning.Look47StepS); // NIGHT48 L: a quick step for a fast goal (Hull48.cs)
                    footTo[i] = footGoal; // it reaches for where the foot should go now
                    float u = Mathf.Clamp01(stepT[i]);
                    float e = u * u * (3f - 2f * u);
                    float over = Mathf.Sin(u * Mathf.PI) * UnityTuning.Look47StepOvershoot; // reaches a little past, then settles back
                    Vector3 dir = footTo[i] - footFrom[i];
                    foot = Vector3.Lerp(footFrom[i], footTo[i], e) + dir.normalized * over + Vector3.up * (Mathf.Sin(u * Mathf.PI) * UnityTuning.Look47StepLift);
                    if (stepT[i] >= 1f) { footPlant[i] = footTo[i]; stepT[i] = -1; foot = footPlant[i]; Steps47++; pluck = Mathf.Max(pluck, UnityTuning.Look47PluckTell * 0.5f); }
                }
                if (!k.torn) Span(lower[i], U(k.at), foot, UnityTuning.Loom37LegThick);
            }
            // the noses: boxes turned 45 deg about the body's up, at each end (a diamond's half shows: the point)
            float half = UnityTuning.Loom37BodyL * 0.5f;
            float nose = UnityTuning.Loom37BodyW * UnityTuning.Look47BodyNarrow * 0.72f;
            noseFT.position = bc + face * new Vector3(0, 0, half);
            noseBT.position = bc + face * new Vector3(0, 0, -half);
            noseFT.rotation = noseBT.rotation = face * Quaternion.Euler(0, 45, 0);
            noseFT.localScale = noseBT.localScale = new Vector3(nose, UnityTuning.Loom37BodyH * 0.8f, nose);
            // the drum on its back
            drumT.position = bc + face * new Vector3(0, UnityTuning.Loom37BodyH * 0.6f, 0);
            drumT.rotation = face * Quaternion.Euler(0, 0, 90);
            drumT.localScale = new Vector3(UnityTuning.Look47DrumR * 2f, UnityTuning.Look47DrumL, UnityTuning.Look47DrumR * 2f);
            Color brassC = leg >= 2 ? Look.Hex(UnityTuning.Look47BrassHotHex, UnityTuning.Look47BrassHotGain) : Look.Hex(UnityTuning.Look47BrassHex);
            Tint(drumR, brassC);
            // the brass glow: two strips along the body's top long edges and a band round the drum (amber at rest, hot from phase 2)
            Color brassGlow = Look.Hex(leg >= 2 ? UnityTuning.Look47BrassHotHex : UnityTuning.Look47AmberHex, UnityTuning.Look47StripGain * (br.state == Boss37State.Tell ? 1f + 0.6f * prog : 1f));
            float bw = UnityTuning.Loom37BodyW * UnityTuning.Look47BodyNarrow * 0.5f;
            for (int i = 0; i < 2; i++)
            {
                stripT[i].position = bc + face * new Vector3(i == 0 ? -bw : bw, UnityTuning.Loom37BodyH * 0.5f, 0);
                stripT[i].rotation = face;
                stripT[i].localScale = new Vector3(UnityTuning.Look47StripM, UnityTuning.Look47StripM, UnityTuning.Loom37BodyL * 0.95f);
                Tint(stripR[i], brassGlow);
            }
            for (int i = 0; i < bandT.Length; i++)
            {
                float zf = bandT.Length > 1 ? (i / (float)(bandT.Length - 1) - 0.5f) * UnityTuning.Look47BandSpread : 0f;
                bandT[i].position = bc + face * new Vector3(0, 0, zf * UnityTuning.Loom37BodyL);
                bandT[i].rotation = face;
                bandT[i].localScale = new Vector3(UnityTuning.Loom37BodyW * UnityTuning.Look47BodyNarrow * 1.06f, UnityTuning.Loom37BodyH * 1.06f, UnityTuning.Look47BandM);
                Tint(bandR[i], brassGlow);
            }
            for (int i = 0; i < 6; i++)
            {
                collarT[i].position = U(L.knees[i].hip) + offPos;
                collarT[i].localScale = Vector3.one * UnityTuning.Look47CollarM;
                Tint(collarR[i], brassGlow);
            }
            drumBandT.position = drumT.position;
            drumBandT.rotation = drumT.rotation;
            drumBandT.localScale = new Vector3(UnityTuning.Look47DrumR * 2.1f, UnityTuning.Look47DrumL * 0.25f, UnityTuning.Look47DrumR * 2.1f);
            Tint(drumBandR, brassGlow);
            // the lamps: the tell's colour on the boss, amber at rest (red-orange at rest from phase 2)
            bool tell = br.state == Boss37State.Tell && a != null;
            Color lamp = tell ? (a.kind == Tell36Kind.Yellow ? Look.Hex(UnityTuning.Loom37SpoolHex, UnityTuning.Look47LampGain) : Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Look47LampGain))
                : leg >= 2 ? Look.Hex(UnityTuning.Loom37DangerHex, UnityTuning.Look47LampGain * 0.6f) : Look.Hex(UnityTuning.Look47AmberHex, UnityTuning.Look47LampGain * 0.6f);
            float lw = UnityTuning.Loom37BodyW * UnityTuning.Look47BodyNarrow * 0.3f;
            float front = UnityTuning.Loom37BodyL * UnityTuning.Look47HullLen * 0.5f * (1f - UnityTuning.Look47TipShare) + 0.35f;
            lampLT.position = bc + face * new Vector3(-lw, -UnityTuning.Loom37BodyH * 0.1f, front);
            lampRT.position = bc + face * new Vector3(lw, -UnityTuning.Loom37BodyH * 0.1f, front);
            lampLT.localScale = lampRT.localScale = Vector3.one * UnityTuning.Look47LampM * (tell ? 1f + 0.5f * prog : 1f);
            Tint(lampLR, lamp);
            Tint(lampRR, lamp);
            // the warp: from the drum up to the roof, fanned; sprung, plucked; red-orange while an attack runs, burning in phase 3
            bool hot = br.state == Boss37State.Tell || br.state == Boss37State.Active;
            Color warp = leg >= 3 || hot ? Look.Hex(UnityTuning.Loom37DangerHex, leg >= 3 ? UnityTuning.Look47WarpBurnGain : UnityTuning.Look47WarpHotGain) : Look.Hex(UnityTuning.Look47AmberHex, UnityTuning.Look47WarpGain);
            Vector3 drum = drumT.position;
            int n = UnityTuning.Look47WarpThreads, segs = UnityTuning.Look47WarpSegs;
            float t0 = t;
            for (int th = 0; th < n; th++)
            {
                float u = n > 1 ? th / (float)(n - 1) - 0.5f : 0f;
                Vector3 top = new Vector3(c.x + u * UnityTuning.Look47WarpFanM, UnityTuning.Look47WarpTopY, c.z + (th % 2 == 0 ? 1f : -1f) * UnityTuning.Look47WarpFanM * 0.25f);
                Vector3 side = Vector3.Cross((top - drum).normalized, Vector3.up).normalized;
                Vector3 prev = drum;
                for (int s = 1; s <= segs; s++)
                {
                    float f = s / (float)segs;
                    float wave = calm ? 0f : pluck * Mathf.Sin(f * Mathf.PI) * Mathf.Sin(t0 * UnityTuning.Look47PluckHz * 2f * Mathf.PI + th * 1.3f);
                    float sag = UnityTuning.Look47WarpSagM * Mathf.Sin(f * Mathf.PI);
                    Vector3 p = Vector3.Lerp(drum, top, f) + side * wave + Vector3.down * sag;
                    int idx = th * segs + s - 1;
                    Span(warpT[idx], prev, p, UnityTuning.Look47WarpThick);
                    Tint(warpR[idx], warp);
                    prev = p;
                }
            }
            Hull48Render(sim, L, dt, bc, c, fwd, leg, brassGlow); // NIGHT48 L (loomHull48, a view row; off: Hull48Off)
        }
    }
}
