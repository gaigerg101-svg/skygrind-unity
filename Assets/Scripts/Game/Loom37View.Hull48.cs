// NIGHT48 builder L (loomHull48, a VIEW row: NightRows.ViewOn(cfg, "loomHull48"); NIGHT48.md G36 / G37): THE LOOM's shuttle as it must read
// from where the player stands. Drawn on top of loomLook47 (Loom37View.Look47.cs: it needs that row on; off, nothing here is built or drawn).
// What the NIGHT47 hull got wrong, read from its own code and the r2 frames (Logs/feel/n47-r2/a447/a447/shot-N29-L47-shuttle-10 / -20):
//  - it was turned to face the player (LookRotation(toward you)): the long pointed body was always seen END-ON, a hexagon with two eyes;
//    the hull now lies ACROSS the well (a loom's shuttle flies across the warp: its long axis along world x, the legs' own axis), turned
//    at most Hull48YawMaxDeg toward the player, so its length and both points read broadside from the door and every post;
//  - the legs, the spinneret and the noses were the primitive cube / sphere on SKYGRIND/Stone, which draws VERTEX colours: a primitive has
//    none, so they drew WHITE-grey (the "grey box"); here they get their own coloured meshes: blackened-iron lower legs, dark bronze upper
//    legs, brass claws at the feet, a dark bronze spinneret;
//  - the drum sat on the body's back, which the player (always below it) never sees; here it is the brass HUB through the body's middle,
//    its face toward the player between the two amber lamp EYES, under angular iron brows (the visor language of G13), its amber thread
//    band showing under the belly;
//  - the under-light was the pale knee colour; here it is the lamps' amber, so the belly the player looks at reads warm bronze;
//  - MOTION (G37): the anticipation now REARS the body back (a rotation about its long axis, readable at 20 m where a 0.8 m sink is ~2 deg),
//    the strike snaps it forward past rest and settles; the hit FLINCH is a fast spring (Hull48FlinchHz, ~70 ms to its peak) that tips the
//    body away from the hit by up to Hull48FlinchMaxDeg, scaled by the hit's damage, plus an 80 ms additive flash shell (the hit flash);
//    a planted foot dips the body (weight); a stepping foot reaches AHEAD of its moving goal (the procedural-gait lead).
// View only: the sim's knees, spinneret and hit spheres are the truth; nothing here moves a hit box. Numbers: UnityTuning.L48.cs (Hull48*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        /// <summary>Reel-only switches (FeelReel.N48.Loom.cs, the blind pair's frame sets): -1 the config's row, 0 off, 1 on.</summary>
        public static int Force47 = -1, Force48 = -1;

        /// <summary>For the reels / frames (loomHull48): drawn now; the flinch's tilt now (deg); hits that flinched; the anticipation's rear (deg);
        /// the tell share spent rearing (0..1) of the last tell; a stepping leg's progress (-1: none lifting).</summary>
        public bool Hull48Shown;
        public float Hull48FlinchDeg, Hull48TiltDeg;
        public int Hull48Flinches, Hull48Hits;
        public float Hull48StepPhase = -1f;
        /// <summary>For the reels: the drawn hull's length / width (round 4: >= 2.5, the lead's G36 bar).</summary>
        public float Hull48Aspect;

        Transform hull48Root, flashT, hubT, hubBandT, browLT, browRT;
        Renderer flashR, hubBandR;
        readonly Transform[] clawT = new Transform[6], tipT = new Transform[2];
        readonly Renderer[] tipR = new Renderer[2];
        Transform[] edgeT;
        Renderer[] edgeR;
        Mesh spindleMesh, spindleHotMesh, flashMesh, hubMesh, cylMesh, legUpMesh, legLowMesh, clawMesh, spinMesh, browMesh;
        Material bronze48;
        bool hull48Hooked, hull48Swapped;
        Color underWas;
        float yaw48, yawVel48, tilt48, tiltVel48, flinch48, flinchVel48, flash48;
        Boss37State lastState48;
        int lastSteps48;
        // the gait's lead (Look47's step loop asks Lead48): each foot goal's smoothed velocity, the last goal, a reel-forced goal offset
        readonly Vector3[] goalLast48 = new Vector3[6], goalVel48 = new Vector3[6], forcedGoal48 = new Vector3[6], goalNow48 = new Vector3[6];
        readonly bool[] goalSeen48 = new bool[6];
        readonly float[] stepDur48 = { 0.35f, 0.35f, 0.35f, 0.35f, 0.35f, 0.35f };

        /// <summary>Look47's step loop, a step starting on leg i (loomHull48 on): its time, as quick as its goal is fast (StepS x PaceMs /
        /// the goal's speed, between StepMinS and StepS; Spider Walker's "minimum and maximum step times": Research/2026-10-19-wave48/L.md).</summary>
        void Step48Begin(int i)
        {
            float v = goalVel48[i].magnitude;
            stepDur48[i] = Mathf.Clamp(UnityTuning.Look47StepS * UnityTuning.Hull48PaceMs / Mathf.Max(1e-4f, v), UnityTuning.Hull48StepMinS, UnityTuning.Look47StepS);
        }

        /// <summary>Look47's step loop (loomHull48 on): the foot's goal plus the reel's forced offset plus its velocity x the step's time x
        /// Hull48Lead (a stepping foot lands where its goal will be, not where it was: the procedural-gait lead).</summary>
        Vector3 Lead48(int i, Vector3 goal, float dt)
        {
            if (!goalSeen48[i] || dt <= 1e-5f) { goalSeen48[i] = true; goalLast48[i] = goal; goalVel48[i] = Vector3.zero; }
            else
            {
                Vector3 raw = (goal - goalLast48[i]) / dt;
                goalVel48[i] = Vector3.Lerp(goalVel48[i], raw, UnityTuning.Hull48LeadSmooth);
                goalLast48[i] = goal;
            }
            return goal + goalVel48[i] * (UnityTuning.Look47StepS * UnityTuning.Hull48Lead);
        }

        /// <summary>Look47's step trigger (loomHull48 on): NIGHT47's StepDist, or the shorter idle distance while the goal is nearly still.
        /// NIGHT47's idle shuffle sways each goal +-Look47ShuffleM (1.2 m) and steps past StepDist (1.5 m): a foot that plants within 0.3 m
        /// of the sway's middle can never be 1.5 m from it again, so a resting machine stops stepping for good (a448 on n48-r1: "n47: no
        /// step in 10 s"); an idle distance under the sway's amplitude always comes round again.</summary>
        float StepDist48(int i) => goalVel48[i].magnitude < UnityTuning.Hull48IdleSpeed ? UnityTuning.Hull48IdleStepM : UnityTuning.Look47StepDist;

        /// <summary>For the reel's strip: the time (s) leg i's current step takes (NIGHT47's StepS with loomHull48 off).</summary>
        public float StepDurOf(int i) => i >= 0 && i < 6 && Hull48Shown ? stepDur48[i] : UnityTuning.Look47StepS;

        bool Hull48On() => Force48 >= 0 ? Force48 == 1 : NightRows.ViewOn(game.cfg, "loomHull48");

        /// <summary>The spindle's radius share at u in -1..1 along its length: flat over the middle, then a long taper to a point.</summary>
        static float Profile48(float u)
        {
            float a = Mathf.Abs(u);
            if (a <= UnityTuning.Hull48FlatShare) return 1f;
            float s = Mathf.Clamp01((a - UnityTuning.Hull48FlatShare) / (1f - UnityTuning.Hull48FlatShare));
            return Mathf.Max(0f, 1f - Mathf.Pow(s, UnityTuning.Hull48TaperPow));
        }

        /// <summary>A six-sided spindle along z (unit: x, y in -0.5..0.5, z in -0.5..0.5), its rings at Hull48Rings, pointed at both ends;
        /// flat-shaded, both windings (two-sided whatever the material culls), every vertex coloured: the tips' last share brass, the rest body.</summary>
        static Mesh Spindle48(Color body, Color brass)
        {
            var v = new System.Collections.Generic.List<Vector3>();
            var cs = new System.Collections.Generic.List<Color>();
            var t = new System.Collections.Generic.List<int>();
            float[] rings = UnityTuning.Hull48Rings;
            Vector3 R(int k, float u) { float r = Profile48(u) * 0.5f, a = k * Mathf.PI / 3f + Mathf.PI / 6f; return new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, u * 0.5f); }
            void Tri(Vector3 a, Vector3 b, Vector3 c, Color col)
            {
                int i = v.Count; v.Add(a); v.Add(b); v.Add(c); t.Add(i); t.Add(i + 1); t.Add(i + 2);
                i = v.Count; v.Add(a); v.Add(c); v.Add(b); t.Add(i); t.Add(i + 1); t.Add(i + 2);
                for (int n = 0; n < 6; n++) cs.Add(col);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int r = 0; r + 1 < rings.Length; r++)
                {
                    float u0 = side * rings[r], u1 = side * rings[r + 1];
                    Color col = Mathf.Abs(u1) > UnityTuning.Hull48CapShare + 1e-4f ? brass : body;
                    for (int k = 0; k < 6; k++)
                    {
                        Vector3 a0 = R(k, u0), a1 = R(k + 1, u0), b0 = R(k, u1), b1 = R(k + 1, u1);
                        if (Profile48(u1) > 1e-4f) { Tri(a0, b0, b1, col); Tri(a0, b1, a1, col); }
                        else Tri(a0, b0, a1, col); // the point
                    }
                }
            var m = new Mesh { name = "loom48 spindle" };
            m.SetVertices(v);
            m.SetColors(cs);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        void Hull48Build()
        {
            hull48Root = new GameObject("loomHull48").transform;
            hull48Root.SetParent(root, false);
            Color bronze = Look.Hex(UnityTuning.Hull48BronzeHex), brassC = Look.Hex(UnityTuning.Hull48BrassHex);
            spindleMesh = Spindle48(bronze, brassC);
            spindleHotMesh = Spindle48(Look.Hex(UnityTuning.Hull48BronzeHotHex), Look.Hex(UnityTuning.Look47BrassHotHex));
            flashMesh = Spindle48(Color.white, Color.white);
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylMesh = cyl.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cyl);
            hubMesh = Colored(Object.Instantiate(cylMesh), brassC);
            legUpMesh = Colored(Object.Instantiate(boxMesh), Look.Hex(UnityTuning.Hull48LegUpHex));
            legLowMesh = Colored(Object.Instantiate(boxMesh), Look.Hex(UnityTuning.Hull48LegLowHex));
            clawMesh = Colored(Object.Instantiate(boxMesh), brassC);
            spinMesh = Colored(Object.Instantiate(ballMesh), Look.Hex(UnityTuning.Hull48SpinHex));
            browMesh = Colored(Object.Instantiate(boxMesh), Look.Hex(UnityTuning.Hull48LegLowHex));
            bronze48 = Opaque("loom48 bronze", UnityTuning.Hull48BronzeHex);
            hubT = Solid(hull48Root, "loom48 hub", hubMesh, bronze48).transform;
            hubBandR = Glow(hull48Root, "loom48 hub thread", cylMesh, out hubBandT);
            browLT = Solid(hull48Root, "loom48 brow L", browMesh, bronze48).transform;
            browRT = Solid(hull48Root, "loom48 brow R", browMesh, bronze48).transform;
            for (int i = 0; i < 6; i++) clawT[i] = Solid(hull48Root, "loom48 claw " + i, clawMesh, bronze48).transform;
            flashR = Glow(hull48Root, "loom48 hit flash", flashMesh, out flashT);
            int edges = 4 * 2 * (UnityTuning.Hull48Rings.Length - 1);
            edgeT = new Transform[edges];
            edgeR = new Renderer[edges];
            for (int i = 0; i < edges; i++) edgeR[i] = Glow(hull48Root, "loom48 outline " + i, boxMesh, out edgeT[i]);
            for (int i = 0; i < 2; i++) tipR[i] = Glow(hull48Root, "loom48 point lamp " + i, ballMesh, out tipT[i]);
            if (!hull48Hooked && game.events != null)
            {
                hull48Hooked = true;
                game.events.On<BossEvent>("bossHit", e =>
                {
                    if (e == null || e.boss != "loom37" || hull48Root == null || !Hull48Shown) return;
                    Hull48Hits++;
                    // one kick per hit, never summed (a THUNDERCLAP's 8 pellets are one flinch): the velocity is set to at least the kick
                    float k = UnityTuning.Hull48FlinchKickDegS * Mathf.Clamp((float)e.damage / UnityTuning.Hull48FlinchRefDamage, UnityTuning.Hull48FlinchMin, UnityTuning.Hull48FlinchMaxScale);
                    if (flinchVel48 < k * 0.5f) Hull48Flinches++;
                    flinchVel48 = Mathf.Max(flinchVel48, k);
                    flash48 = 1f;
                });
            }
        }

        void Hull48Swap(bool on)
        {
            if (hull48Swapped == on) return;
            hull48Swapped = on;
            // the lead's velocity starts fresh whenever the row comes on (the reel's look switches left it seconds stale: a448 on n48-r1, the
            // n48 strip's step ran short with a 0.58 m lift)
            for (int i = 0; i < 6; i++) { goalSeen48[i] = false; goalVel48[i] = Vector3.zero; }
            for (int i = 0; i < 6; i++)
            {
                upper[i].GetComponent<MeshFilter>().sharedMesh = on ? legUpMesh : boxMesh;
                lower[i].GetComponent<MeshFilter>().sharedMesh = on ? legLowMesh : boxMesh;
            }
            spinT.GetComponent<MeshFilter>().sharedMesh = on ? spinMesh : ballMesh;
            if (on) { underWas = underLight.color; underLight.color = Look.Hex(UnityTuning.Look47AmberHex); underLight.intensity = UnityTuning.Hull48UnderIntensity * UnityTuning.LightScale; }
            else { underLight.color = underWas; underLight.intensity = UnityTuning.Loom37LightIntensity * UnityTuning.LightScale; }
        }

        /// <summary>The row off (or loomLook47 off: Look47Render calls it): nothing of the hull drawn, the NIGHT47 meshes and light back.</summary>
        void Hull48Off()
        {
            Hull48Shown = false;
            Hull48StepPhase = -1f;
            if (hull48Root == null) return;
            if (hull48Root.gameObject.activeSelf) hull48Root.gameObject.SetActive(false);
            Hull48Swap(false);
            if (drumT != null && !drumT.gameObject.activeSelf) drumT.gameObject.SetActive(true); // loomLook47's own drum back
            if (drumBandT != null && !drumBandT.gameObject.activeSelf) drumBandT.gameObject.SetActive(true);
        }

        /// <summary>The end of Look47Render (loomLook47 on): the shuttle laid across the well, its hub, eyes, brows, claws, the motion.
        /// bc: the drawn body centre (Look47's springs applied); c: the sim's centre; fwd: toward the player (flat).</summary>
        void Hull48Render(SurvivalSim sim, Loom37 L, float dt, Vector3 bc, Vector3 c, Vector3 fwd, int leg, Color brassGlow)
        {
            bool on = Hull48On();
            if (!on) { Hull48Off(); return; }
            Hull48Shown = true;
            if (hull48Root == null) Hull48Build();
            if (!hull48Root.gameObject.activeSelf) hull48Root.gameObject.SetActive(true);
            Hull48Swap(true);
            bool calm = game.cfg.movement.settings.reducedMotion;
            Boss37Brain br = L.brain;
            float prog = (float)br.Progress;

            // the long axis: across the well (world x, the legs' axis), turned toward the player by a share of the angle, capped, sprung
            Vector3 side = Vector3.Cross(Vector3.up, fwd);
            if (side.x < 0) side = -side;
            float yawGoal = Mathf.Clamp(Vector3.SignedAngle(Vector3.right, side, Vector3.up) * UnityTuning.Hull48YawFollow, -UnityTuning.Hull48YawMaxDeg, UnityTuning.Hull48YawMaxDeg);
            if (calm) { yaw48 = yawGoal; yawVel48 = 0; } else Spring(ref yaw48, ref yawVel48, yawGoal, UnityTuning.Hull48YawHz, 1f, dt);
            Vector3 longAx = Quaternion.AngleAxis(yaw48, Vector3.up) * Vector3.right;
            Vector3 flank = Vector3.Cross(longAx, Vector3.up).normalized;
            if (Vector3.Dot(flank, fwd) < 0) flank = -flank; // the flank that faces the player

            // MOTION: the rear (anticipation) and the strike, the flinch, the hit flash, the plant's dip
            float tiltGoal = 0f;
            if (br.state == Boss37State.Tell) tiltGoal = -UnityTuning.Hull48RearDeg * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(prog / UnityTuning.Hull48RearShare));
            else if (br.state == Boss37State.Active) tiltGoal = UnityTuning.Hull48StrikeDeg;
            if (br.state == Boss37State.Active && lastState48 == Boss37State.Tell) tiltVel48 += UnityTuning.Hull48StrikeKickDegS;
            lastState48 = br.state;
            if (Steps47 > lastSteps48) offVel += Vector3.down * (UnityTuning.Hull48PlantKick * (Steps47 - lastSteps48));
            lastSteps48 = Steps47;
            if (calm) { tilt48 = 0; tiltVel48 = 0; flinch48 = 0; flinchVel48 = 0; }
            else
            {
                Spring(ref tilt48, ref tiltVel48, tiltGoal, UnityTuning.Hull48TiltHz, UnityTuning.Hull48TiltZeta, dt);
                Spring(ref flinch48, ref flinchVel48, 0f, UnityTuning.Hull48FlinchHz, UnityTuning.Hull48FlinchZeta, dt);
                flinch48 = Mathf.Clamp(flinch48, -UnityTuning.Hull48FlinchMaxDeg, UnityTuning.Hull48FlinchMaxDeg);
            }
            Hull48TiltDeg = tilt48;
            Hull48FlinchDeg = flinch48;
            // a positive tilt brings the top toward the player (the strike); the rear and the flinch take it away
            float tiltRad = (tilt48 - flinch48) * Mathf.Deg2Rad;
            Vector3 up1 = Vector3.up * Mathf.Cos(tiltRad) + flank * Mathf.Sin(tiltRad);
            Quaternion rot = Quaternion.AngleAxis(roll, flank) * Quaternion.LookRotation(longAx, up1);
            if (m63.on) rot = m63.tilt * rot; // NIGHT63 V bossMotion63 (Loom37View.N63V.cs): the layer's tilt about the hull's centre (off: as NIGHT48)

            // round 4 (the G36 blind pair's judges: "a flat saucer, not a long shuttle"): longer, narrower, lower (Hull48LenMul / WidthMul / HeightMul)
            float len = UnityTuning.Loom37BodyL * UnityTuning.Hull48LenMul, w = UnityTuning.Loom37BodyW * UnityTuning.Hull48WidthMul, h = UnityTuning.Loom37BodyH * UnityTuning.Hull48HeightMul;
            if (m63.on) { w *= m63.squash.x; h *= m63.squash.y; len *= m63.squash.z; } // NIGHT63 V bossMotion63: the layer's volume-keeping squash
            hullT.position = bc;
            hullT.rotation = rot;
            hullT.localScale = new Vector3(w, h, len);
            Hull48Aspect = len / Mathf.Max(1e-4f, w);
            hullT.GetComponent<MeshFilter>().sharedMesh = leg >= 2 ? spindleHotMesh : spindleMesh;
            flash48 = Mathf.Max(0f, flash48 - dt / UnityTuning.Hull48FlashS);
            bool flashOn = flash48 > 0f && !calm;
            if (flashT.gameObject.activeSelf != flashOn) flashT.gameObject.SetActive(flashOn);
            if (flashOn)
            {
                flashT.position = bc;
                flashT.rotation = rot;
                flashT.localScale = new Vector3(w, h, len) * UnityTuning.Hull48FlashGrow;
                Tint(flashR, Look.Hex(UnityTuning.Hull48FlashHex, UnityTuning.Hull48FlashGain * flash48));
            }
            Vector3 lAx = rot * Vector3.forward, uAx = rot * Vector3.up, fAx = rot * Vector3.right;
            if (Vector3.Dot(fAx, flank) < 0) fAx = -fAx;

            // round 3 (the lead's n48-r2 frames: "from the front at 10 m and 20 m the hull reads as a brass sphere with radiating legs": the
            // dark bronze is lost against the dark roof from below, the lit hub reads as a ball): THE OUTLINE, amber glow lines along the
            // spindle's top ridge, its keel and its two near edges from point to point, plus a lamp on each point, so its length and both
            // points read as a lit lens shape from any side at any distance
            int segs = UnityTuning.Hull48Rings.Length - 1;
            Color edge = Look.Hex(leg >= 2 ? UnityTuning.Look47BrassHotHex : UnityTuning.Look47AmberHex, UnityTuning.Hull48EdgeGain);
            for (int line = 0; line < 4; line++)
            {
                // the hexagon's vertices in the hull's unit cross-section: the top (0, 0.5), the bottom (0, -0.5), the near edges (0.433, +-0.25)
                float cx = line < 2 ? 0f : 0.433f, cy = line == 0 ? 0.5f : line == 1 ? -0.5f : line == 2 ? 0.25f : -0.25f;
                for (int sgn = -1, k = 0; sgn <= 1; sgn += 2)
                    for (int r = 0; r < segs; r++, k++)
                    {
                        float u0 = sgn * UnityTuning.Hull48Rings[r], u1 = sgn * UnityTuning.Hull48Rings[r + 1];
                        Vector3 p0 = bc + lAx * (u0 * len * 0.5f) + (fAx * (cx * w) + uAx * (cy * h)) * Profile48(u0) * UnityTuning.Hull48EdgeOut;
                        Vector3 p1 = bc + lAx * (u1 * len * 0.5f) + (fAx * (cx * w) + uAx * (cy * h)) * Profile48(u1) * UnityTuning.Hull48EdgeOut;
                        Transform et = edgeT[line * 2 * segs + k];
                        Span(et, p0, p1, UnityTuning.Hull48EdgeThick);
                        Tint(edgeR[line * 2 * segs + k], edge);
                    }
            }
            for (int s = 0; s < 2; s++)
            {
                tipT[s].position = bc + lAx * ((s == 0 ? -0.5f : 0.5f) * len);
                tipT[s].localScale = Vector3.one * UnityTuning.Hull48TipLampM;
                Tint(tipR[s], edge * UnityTuning.Hull48TipGain);
            }

            // the brass bands round the spindle, sized by its profile where they sit
            for (int i = 0; i < bandT.Length; i++)
            {
                float u = bandT.Length > 1 ? (i / (float)(bandT.Length - 1) - 0.5f) * 2f * UnityTuning.Hull48BandAt : 0f;
                float pr = Profile48(u) * UnityTuning.Hull48BandGrow;
                bandT[i].position = bc + lAx * (u * len * 0.5f);
                bandT[i].rotation = rot;
                bandT[i].localScale = new Vector3(w * pr, h * pr, UnityTuning.Look47BandM);
            }
            // the two brass strips along the flanks' upper edges (the middle third, where the spindle is straight)
            for (int i = 0; i < 2; i++)
            {
                stripT[i].position = bc + fAx * ((i == 0 ? -1f : 1f) * w * 0.43f) + uAx * (h * 0.25f);
                stripT[i].rotation = rot;
                stripT[i].localScale = new Vector3(UnityTuning.Look47StripM, UnityTuning.Look47StripM, len * UnityTuning.Hull48FlatShare);
            }
            // THE HUB: a brass drum through the middle, its face toward the player; its amber thread band under the belly and over the back
            float hubR = h * UnityTuning.Hull48HubR, hubL = w * UnityTuning.Hull48HubL;
            hubT.position = bc;
            hubT.rotation = Quaternion.LookRotation(lAx, fAx) ; // the cylinder's length (its local y) along the flank axis
            hubT.localScale = new Vector3(hubR * 2f, hubL * 0.5f, hubR * 2f);
            hubBandT.position = bc;
            hubBandT.rotation = hubT.rotation;
            hubBandT.localScale = new Vector3(hubR * 2f * UnityTuning.Hull48ThreadGrow, hubL * 0.5f * UnityTuning.Hull48ThreadShare, hubR * 2f * UnityTuning.Hull48ThreadGrow);
            Tint(hubBandR, brassGlow);
            drumT.gameObject.SetActive(false);
            drumBandT.gameObject.SetActive(false);
            // THE EYES: two amber lamps on the player's flank either side of the hub, each under an angular iron brow (inner end low)
            bool tell = br.state == Boss37State.Tell && br.Current != null;
            for (int s = -1; s <= 1; s += 2)
            {
                float u = s * UnityTuning.Hull48EyeAt;
                Vector3 eye = bc + lAx * (u * len * 0.5f) + fAx * (w * 0.5f * Profile48(u) + UnityTuning.Hull48EyeOut) + uAx * (h * UnityTuning.Hull48EyeUp);
                Transform lt = s < 0 ? lampLT : lampRT;
                lt.position = eye;
                lt.localScale = Vector3.one * UnityTuning.Hull48EyeM * (tell ? 1f + 0.5f * prog : 1f);
                Transform bt = s < 0 ? browLT : browRT;
                Vector3 outer = eye + lAx * (s * UnityTuning.Hull48BrowHalf) + uAx * UnityTuning.Hull48BrowOuterUp + fAx * 0.1f;
                Vector3 inner = eye - lAx * (s * UnityTuning.Hull48BrowHalf) + uAx * UnityTuning.Hull48BrowInnerUp + fAx * 0.1f;
                Span(bt, outer, inner, UnityTuning.Hull48BrowThick);
            }
            // the hips on the hull (the sim's hips are 3 m out and 2.8 m fore / aft, off a 3.8 m wide body): the drawn hip keeps its along
            // place and sits inside the flank, a little under the middle; the upper leg is re-spanned to the sim's knee; the collars follow
            for (int i = 0; i < 6; i++)
            {
                Loom37Knee k = L.knees[i];
                Vector3 hd = U(k.hip) - c;
                float along = Vector3.Dot(hd, lAx), across = Mathf.Clamp(Vector3.Dot(hd, fAx), -w * UnityTuning.Hull48HipIn, w * UnityTuning.Hull48HipIn);
                Vector3 hip = bc + lAx * along + fAx * across - uAx * (h * UnityTuning.Hull48HipDown);
                Span(upper[i], hip, U(k.at), UnityTuning.Loom37LegThick);
                collarT[i].position = hip;
                // the brass claw: the last ClawM of the lower leg (its foot end), a little thicker
                Transform lo = lower[i];
                if (!k.torn && lo.gameObject.activeSelf)
                {
                    Vector3 foot = lo.position + lo.forward * (lo.localScale.z * 0.5f);
                    Vector3 back = foot - lo.forward * Mathf.Min(UnityTuning.Hull48ClawM, lo.localScale.z * 0.5f);
                    Span(clawT[i], back, foot + lo.forward * 0.2f, UnityTuning.Loom37LegThick * UnityTuning.Hull48ClawThick);
                }
                else if (clawT[i].gameObject.activeSelf) clawT[i].gameObject.SetActive(false);
            }
            // the steps' phase for the strip (the leg lifting furthest along)
            float ph = -1f;
            for (int i = 0; i < 6; i++) if (stepT[i] >= 0) ph = Mathf.Max(ph, stepT[i]);
            Hull48StepPhase = ph;
        }

        /// <summary>For the reel's motion strip: leg i's step progress (0..1 while it lifts, -1 planted) and where its foot is drawn now.</summary>
        public float StepPhaseOf(int i) => i >= 0 && i < 6 ? stepT[i] : -1f;
        public Vector3 FootDrawn(int i) => i >= 0 && i < 6 ? lower[i].position + lower[i].forward * (lower[i].localScale.z * 0.5f) : Vector3.zero;

        /// <summary>Reel-only: start a step on one leg now (its goal moved by Hull48ForceStepM along the wall): the motion strip's subject.</summary>
        public bool Hull48ForceStep(int i)
        {
            if (i < 0 || i >= 6 || stepT[i] >= 0) return false;
            // the goal jumps along the wall (or back to zero on the next call): Look47's own step rule then lifts the foot and plants it there;
            // the jump goes the way the goal already leans from the plant, so the shuffle never cancels it (|goal - plant| >= ForceStepM)
            if (forcedGoal48[i].sqrMagnitude > 1e-4f) { forcedGoal48[i] = Vector3.zero; return true; }
            float sign = (goalNow48[i] - footPlant[i]).z >= 0f ? 1f : -1f;
            forcedGoal48[i] = Vector3.forward * (sign * UnityTuning.Hull48ForceStepM);
            return true;
        }
    }
}
