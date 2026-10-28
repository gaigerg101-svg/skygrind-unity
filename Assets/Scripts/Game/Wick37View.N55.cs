// NIGHT55 builder B, grade55 (design.n55.grade55, a view row): THE WICK's hot walls read as a STATE, not wallpaper (critic-veteran #6; OPEN
// D-16; slate P0-hotWall), and THE WICK's stack lights take the boss palette's cool light (BossGrade55.LightsTint / LightsMix).
// NIGHT37 drew every seam of the shaft red-orange (Stack37HotHex) and washed the whole wall face red-orange, all the time: 30-80 px stripes in every
// shaft frame, the danger hue used as wallpaper (V-1, V-2), and no frame said which wall was hot. The sim's rule is unchanged (the shaft wall burns
// whoever rides or brushes it; SurvivalSim.Wick37.cs). With the row on:
//   - AT REST the seams are cold iron: numbers.hot.seam at numbers.hot.seamGain (a value step on the dark iron, low chroma, far from the danger
//     hue), the red-orange wash off;
//   - THE HEAT is where the player is: as the player's capsule comes within numbers.hot.nearM of the shaft wall, the seams and the iron round the
//     nearest point light up red-orange (Stack37HotHex), up to numbers.hot.contact at the wall, over +-hot.spanM along the wall and +-hot.rows seam
//     rows, the rows' heat rippling upward at hot.rippleHz (a pattern as well as a hue: XAG 103's second channel; +-hot.rippleAmp, well under a
//     flash);
//   - A BURN (sim.wick37HotBurns rises) lights the patch where it happened at hot.burn (the brightest, most saturated thing on the wall), easing out
//     over hot.burnS. NIGHT56 V (V4, round 3): an ADDITIVE red-orange over the graded indigo iron sums to a pink (n56-r1 a155: the patch S 0.22-0.48
//     vs the walls' 0.66-0.74): while the burn is at least hot.burnOpaqueMin of its peak, the patch's iron quads are drawn OPAQUE in the danger
//     colour (Stack37HotHex x hot.burnIron, the edges at 45 % of it), just behind the seams, so the burning patch carries the danger token's own
//     saturation (~0.88); below that share (and for the approach heat) the additive glow as round 2. No number: round 2's look.
// A pool of numbers-free quads (Glow: the same additive material as every Wick37View glow) is laid over the seams near the heat each frame; nothing
// is drawn with the row off, and NIGHT54's seams, wash and burn flash come back as they were.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        bool on55, washHidden55;
        Mesh quad55;
        readonly System.Collections.Generic.List<Transform> heatT55 = new System.Collections.Generic.List<Transform>();
        readonly System.Collections.Generic.List<Renderer> heatR55 = new System.Collections.Generic.List<Renderer>();
        int burnsSeen55 = -1;
        Material burnMat55; // NIGHT56 V: the burning patch's opaque iron (a copy of the neon, tinted per quad by the property block)
        float burnLeft55;
        double burnAngle55, burnY55;

        // --- for the a155 reel ---
        /// <summary>The heat drawn this frame (0..1: the patch's peak) and the quads it used.</summary>
        public float Heat55 { get; private set; }
        public int HeatQuads55 { get; private set; }
        /// <summary>The patch's centre this frame (web arena-local: the angle round the axis, radians, and the height) and its half span (m).</summary>
        public double HeatAngle55 { get; private set; }
        public double HeatY55 { get; private set; }
        public float HeatSpanM55 { get; private set; }
        public float HeatHalfH55 { get; private set; }
        /// <summary>The cold seams' colour with the row on (a155 prints its hue against the danger hue).</summary>
        public Color Seam55 { get; private set; }
        public bool On55 => on55;

        static float H(GameConfig cfg, string k, float missing) => BossGrade55.N(cfg, "hot." + k, missing);

        /// <summary>NIGHT56 V (V4 attempt 2, the r2 critic: "give THE WICK's ride panels a cyan strip"; n56-r2 a155: the panels' cyan share 0-8 %): the
        /// ride panels' top and bottom cyan strips (BuildStack's panel trim) at Stack37TrimH x hot.panelTrimScale while grade55 is on (0.16 m: ~16 px at
        /// 8 m, under a 20 px sample cell); Stack37TrimH with the row off or no number. P1 folds the panels into the wall (wick47): no strip to see there.</summary>
        float PanelTrimH55()
        {
            GameConfig cfg = game != null ? game.cfg : null;
            if (cfg == null || !BossGrade55.On(cfg)) return UnityTuning.Stack37TrimH;
            return UnityTuning.Stack37TrimH * Mathf.Clamp(H(cfg, "panelTrimScale", 1f), 1f, 4f);
        }

        /// <summary>From Light54Frame (every Render path, after Render43 / Render47): the cold seams, the heat patch, the stack lights' palette.</summary>
        void Frame55(SurvivalSim sim)
        {
            GameConfig cfg = game.cfg;
            bool on = cfg != null && BossGrade55.On(cfg) && built && sim != null && sim.wick37Centre != null && sim.wick37Stack != null;
            on55 = on;
            if (!on)
            {
                if (washHidden55 && washR != null) washR.enabled = true;
                washHidden55 = false;
                HidePool55(0);
                Heat55 = 0;
                HeatQuads55 = 0;
                return;
            }
            // the stack lights: the palette's cool light (grade55's light layer; 0 outside a graded fight)
            float mix = BossGrade55.LightsMix;
            if (mix > 0)
                foreach (Light l in stackLights43)
                    if (l != null) l.color = Color.Lerp(l.color, BossGrade55.LightsTint, mix);

            // at rest: cold seams, no wash
            Color seamC = new Color(0.3f, 0.32f, 0.4f, 1f);
            BossGrade55.C(cfg, "hot.seam", ref seamC);
            Seam55 = seamC;
            float seamGain = H(cfg, "seamGain", 0.35f);
            Color seamT = new Color(seamC.r * seamGain, seamC.g * seamGain, seamC.b * seamGain, 1f);
            if (hotAR != null) Tint(hotAR, seamT);
            if (hotBR != null) Tint(hotBR, seamT);
            if (washR != null && washR.enabled) { washR.enabled = false; washHidden55 = true; }

            // the heat: the player's approach, a burn
            Wick37Stack s = sim.wick37Stack;
            Vec3 o = sim.wick37Centre.Value, l0 = sim.mover.position - o;
            double r = JsMath.Hypot(l0.x, l0.z), capR = cfg.movement.body.capsuleRadius;
            double ang = System.Math.Atan2(l0.z, l0.x);
            float nearM = Mathf.Max(0.1f, H(cfg, "nearM", 2.5f)), contact = H(cfg, "contact", 0.7f);
            double dWall = s.p.radius - r - capR;
            bool inShaft = l0.y > -1 && l0.y < s.p.capY && r < s.p.radius + 0.5;
            float hp = inShaft ? contact * (1f - Smooth55(Mathf.Clamp01((float)dWall / nearM))) : 0f;
            if (burnsSeen55 < 0 || sim.wick37HotBurns < burnsSeen55) burnsSeen55 = sim.wick37HotBurns;
            if (sim.wick37HotBurns > burnsSeen55)
            {
                burnsSeen55 = sim.wick37HotBurns;
                burnLeft55 = Mathf.Max(0.05f, H(cfg, "burnS", 0.8f));
                burnAngle55 = ang;
                burnY55 = l0.y;
            }
            float burnS = Mathf.Max(0.05f, H(cfg, "burnS", 0.8f));
            burnLeft55 = Mathf.Max(0, burnLeft55 - Time.deltaTime);
            float u = burnLeft55 / burnS, hb = burnLeft55 > 0 ? H(cfg, "burn", 1f) * u * u * (3f - 2f * u) : 0f;
            float spanM = Mathf.Max(0.5f, H(cfg, "spanM", 3f));
            int rows = Mathf.Clamp(Mathf.RoundToInt(H(cfg, "rows", 3f)), 0, 8);
            double ca = ang, cy = l0.y;
            if (hb > 0 && (hp <= 0 || AlongM55(ang, burnAngle55, s.p.radius) > spanM || System.Math.Abs(l0.y - burnY55) > (rows + 1) * UnityTuning.Stack37SeamStep)) { ca = burnAngle55; cy = burnY55; }
            float heat = Mathf.Max(hp, hb);
            Heat55 = heat;
            HeatAngle55 = ca;
            HeatY55 = cy;
            HeatSpanM55 = spanM;
            HeatHalfH55 = (rows + 1) * UnityTuning.Stack37SeamStep;
            if (heat <= 0.001f) { HidePool55(0); HeatQuads55 = 0; BurnOpaque55 = false; return; }
            // NIGHT56 V: a missing hot.burnOpaqueMin (2: above any share) never draws opaque (round 2's look)
            bool burnLit = hb > 0 && hb >= hp && hb >= H(cfg, "burn", 1f) * H(cfg, "burnOpaqueMin", 2f);
            BurnOpaque55 = burnLit;
            HeatQuads55 = DrawHeat55(cfg, s, o, ca, cy, heat, spanM, rows, burnLit);
        }

        /// <summary>For the a155 reel (NIGHT56 V): the burning patch was drawn opaque in the danger colour this frame.</summary>
        public bool BurnOpaque55 { get; private set; }

        int DrawHeat55(GameConfig cfg, Wick37Stack s, Vec3 o, double ca, double cy, float heat, float spanM, int rows, bool burnLit = false)
        {
            if (burnLit && burnMat55 == null) burnMat55 = FxDraw.Opaque(game.mats.neon, "wick55 burn iron", Color.white);
            float burnIron = H(cfg, "burnIron", 1f);
            if (quad55 == null) quad55 = Quad55();
            double fa = s.FacetAngle, rw = s.CornerRadius - UnityTuning.Stack37SeamOff, chord = 2 * rw * System.Math.Sin(fa / 2), inR = rw * System.Math.Cos(fa / 2) - 0.012;
            int facets = s.p.facets, k = Mathf.CeilToInt((float)(spanM / System.Math.Max(0.05, chord)));
            int jc = (int)System.Math.Floor(((ca % (2 * System.Math.PI)) + 2 * System.Math.PI) % (2 * System.Math.PI) / fa);
            float seamGain = H(cfg, "seamGainHot", 2.4f), washGain = H(cfg, "washGainHot", 0.55f), hz = H(cfg, "rippleHz", 1.2f), amp = Mathf.Clamp01(H(cfg, "rippleAmp", 0.25f));
            double step = UnityTuning.Stack37SeamStep, halfH = (rows + 1) * step;
            int used = 0;
            for (int dj = -k; dj <= k; dj++)
            {
                int j = ((jc + dj) % facets + facets) % facets;
                double am = (j + 0.5) * fa;
                double along = AlongM55(am, ca, s.p.radius);
                float fx = 1f - (float)(along * along / (spanM * spanM));
                if (fx <= 0) continue;
                fx *= fx;
                Vec3 nrm = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                Quaternion rot = Quaternion.LookRotation(N(nrm), Vector3.up);
                // the iron's glow behind the seams (one quad over the patch's height on this facet)
                double wy0 = System.Math.Max(s.InGap(j) ? s.p.gapHeight : 0, cy - halfH), wy1 = System.Math.Min(s.p.capY, cy + halfH);
                if (wy1 > wy0 + 0.05)
                {
                    if (burnLit)
                    {
                        // NIGHT56 V: the burning iron, opaque, 6 mm behind the seams (inR) and 6 mm in front of the wall's face (inR + 0.012)
                        Vec3 cb = Polar(am, inR + 0.006, (wy0 + wy1) * 0.5) + o;
                        used = Place55(used, U(cb), rot, new Vector3((float)chord, (float)(wy1 - wy0), 1f), Look.Hex(UnityTuning.Stack37HotHex, burnIron * (0.45f + 0.55f * fx)), burnMat55);
                    }
                    else
                    {
                        Vec3 c = Polar(am, inR - 0.01, (wy0 + wy1) * 0.5) + o;
                        used = Place55(used, U(c), rot, new Vector3((float)chord, (float)(wy1 - wy0), 1f), Look.Hex(UnityTuning.Stack37HotHex, washGain * heat * fx));
                    }
                }
                // the seams in the patch
                int row = 0;
                for (double y = step; y < s.p.capY - 0.5; y += step, row++)
                {
                    if (s.InGap(j) && y < s.p.gapHeight) continue;
                    double dy = y + UnityTuning.Stack37SeamH * 0.5 - cy;
                    if (System.Math.Abs(dy) > halfH) continue;
                    float fy = 1f - (float)(dy * dy / (halfH * halfH));
                    fy *= fy;
                    float ripple = 1f + amp * Mathf.Sin((t * hz - row * 0.25f) * Mathf.PI * 2);
                    Vec3 c = Polar(am, inR, y + UnityTuning.Stack37SeamH * 0.5) + o;
                    used = Place55(used, U(c), rot, new Vector3((float)chord, UnityTuning.Stack37SeamH, 1f), Look.Hex(UnityTuning.Stack37HotHex, seamGain * heat * fx * fy * ripple));
                }
            }
            HidePool55(used);
            return used;
        }

        int Place55(int i, Vector3 pos, Quaternion rot, Vector3 scale, Color c, Material m = null)
        {
            if (i >= heatT55.Count)
            {
                if (heatT55.Count >= UnityTuning.Grade55HeatPool) return i;
                Renderer r = Glow(stackRoot, "heat55", quad55, out Transform tr);
                heatT55.Add(tr);
                heatR55.Add(r);
            }
            Transform t5 = heatT55[i];
            if (t5 == null) { heatT55.Clear(); heatR55.Clear(); return i; } // a rebuilt stack destroyed the pool (stackRoot's children): rebuilt next frame
            t5.gameObject.SetActive(true);
            t5.SetPositionAndRotation(pos, rot);
            t5.localScale = scale;
            Material want = m != null ? m : add; // NIGHT56 V: the pool's quads are additive unless a burning iron quad asks for the opaque one
            if (heatR55[i].sharedMaterial != want) heatR55[i].sharedMaterial = want;
            Tint(heatR55[i], c);
            return i + 1;
        }

        void HidePool55(int from)
        {
            for (int i = from; i < heatT55.Count; i++)
            {
                if (heatT55[i] == null) { heatT55.Clear(); heatR55.Clear(); return; }
                if (heatT55[i].gameObject.activeSelf) heatT55[i].gameObject.SetActive(false);
            }
        }

        /// <summary>The distance along the shaft's wall (m) between two angles round the axis.</summary>
        public static double AlongM55(double a, double b, double radius)
        {
            double d = System.Math.Abs(a - b) % (2 * System.Math.PI);
            if (d > System.Math.PI) d = 2 * System.Math.PI - d;
            return d * radius;
        }

        static float Smooth55(float x) => x * x * (3f - 2f * x);

        /// <summary>A unit quad in local XY, both faces (the additive material culls back faces).</summary>
        static Mesh Quad55()
        {
            var k = new MeshKit();
            Vector3 a = new Vector3(-0.5f, -0.5f, 0), b = new Vector3(0.5f, -0.5f, 0), c = new Vector3(0.5f, 0.5f, 0), d = new Vector3(-0.5f, 0.5f, 0);
            k.Quad(a, b, c, d, Vector3.back, Color.white);
            k.Quad(b, a, d, c, Vector3.forward, Color.white);
            return k.Build("wick55 heat quad");
        }
    }
}
