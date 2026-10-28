// NIGHT63 builder G (slate B2's measurement: "the boss63 reel lists every attack of the five bosses with its cue lead and cue hue, before and
// after"): the Unity half of the cue measure, filling V's boss63 partials (FeelReel.N63V.cs declares them; nothing there changes). A reel's
// measure only: no view, no sim and no row is touched (bossTell63 stays OFF; this file never reads it). BossCue63.cs holds the analysis.
//   Boss63EachG (every frame of boss63's hold): every Boss63CueRescanFrames frames it lists the renderers (meshes, skinned meshes, lines; no
//     particles or trails) and lights within Boss63CueRadiusM of the boss's centre and farther than Boss63CueNearCamM from the eye (inactive
//     ones too: a pooled telegraph is known before it is switched on); every frame it records each one's drawn state (enabled, active, rendered
//     by a camera; a light: enabled with intensity) and the colour the view draws it with (a renderer's MaterialPropertyBlock _BaseColor /
//     _EmissionColor / _Color, the brightest, else its material's; a line's start colour; a light's colour x intensity), keeping
//     Boss63CueBufferS. It follows BossAttackClock63 as V's lines do: a wind-up's first step (ms - (windupSteps - stepsToHit) steps), the
//     strike's step (ms - phaseSteps steps); at each strike it measures that attack's cue (BossCue63.Analyse) and prints
//       [boss63] cue <boss> <attack> start <ms|none> hit <ms> lead <ms> hue <deg|-> sat <s|-> parry <yes|no> danger <yes|no> kind <yellow|red|?>
//                objects <n> obj <the strongest riser's name>
//     (kind: the sim's tell colour: a Boss37Brain attack's kind; THE ICEBREAKER's block yellow, its other attacks red; the wyrm "?").
//   Boss63EndG (once after the hold): "[boss63] cues <boss> attacks <n> found <f> | under 300 ms <k> (ids) | off the danger hue <m> (ids) |
//     parry drawn <p> of <y> yellow | method ...": B2's fix list (NIGHT63 G B2b: BossCue63.Verdict, parryable by the sim's kind only; the
//     reference reaches back to the previous hit). A parryable attack (kind yellow) is not counted off
//     the danger hue: its yellow is the parry tell (DECIDE63 D-1). The values read are one frame late at most (the views draw in the game's
//     frame; this runs in the reel's coroutine): said in the notes.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static readonly HashSet<Transform> g63Roots = new HashSet<Transform>();
        static readonly HashSet<string> g63TellShot = new HashSet<string>(); // NIGHT63 G B2b: the bosses whose tell frame was taken this run
        static readonly List<Renderer> g63Rends = new List<Renderer>();
        static readonly List<Light> g63Lights = new List<Light>();
        static readonly Dictionary<int, string> g63Names = new Dictionary<int, string>();
        static readonly List<double> g63Ms = new List<double>();
        static readonly List<Lit63[]> g63Frames = new List<Lit63[]>();
        static readonly List<string> g63Short = new List<string>(), g63OffHue = new List<string>();
        static readonly MaterialPropertyBlock g63Block = new MaterialPropertyBlock();
        static readonly int G63Base = Shader.PropertyToID("_BaseColor"), G63Emission = Shader.PropertyToID("_EmissionColor"), G63Color = Shader.PropertyToID("_Color");
        static BossAttack63 g63Prev;
        static int g63ScanAt = -1;
        static double g63LastMs = -1, g63WindupMs = -1, g63LastHitMs = double.NegativeInfinity;
        static string g63WindupOf, g63Kind;
        static int g63Frame, g63Attacks, g63Found, g63ParryDrawn, g63ParryKind, g63NoWindup;

        static void Boss63ResetG()
        {
            g63Roots.Clear();
            g63TellShot.Clear();
            g63ScanAt = -1;
            g63Rends.Clear();
            g63Lights.Clear();
            g63Names.Clear();
            g63Ms.Clear();
            g63Frames.Clear();
            g63Short.Clear();
            g63OffHue.Clear();
            g63Prev = default;
            g63WindupMs = -1;
            g63LastHitMs = double.NegativeInfinity;
            g63WindupOf = g63Kind = null;
            g63Frame =g63Attacks = g63Found = g63ParryDrawn = g63ParryKind = g63NoWindup = 0;
        }

        static Vector3 U63(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        static partial void Boss63EachG(SurvivalGame g, string dir, BossAttack63 a, double ms, List<string> lines)
        {
            if (g == null || g.sim == null) return;
            if (g63LastMs < 0 || ms < g63LastMs - 1) Boss63ResetG(); // a new boss63 run
            g63LastMs = ms;
            g63Frame++;
            if (a.Live)
            {
                if (g63ScanAt < 0 || g63Frame - g63ScanAt >= Math.Max(1, UnityTuning.Boss63CueRescanFrames))
                {
                    g63ScanAt = g63Frame;
                    if (g63Roots.Count == 0) Boss63RootsG(lines, a, ms); // n63-b2: the wyrm and the crown found none at the reveal: tried again each scan
                    Boss63ScanG(U63(a.centre), U63(g.Eye));
                }
                Boss63SnapG(ms);
            }
            double stepMs = g.sim.step * 1000;
            // the wind-up's first step (V's reckoning: the read may come a step or two after it began)
            if (a.phase == BossPhase63.Windup && (g63Prev.phase != BossPhase63.Windup || g63Prev.attack != a.attack))
            {
                g63WindupMs = ms - (a.windupSteps - a.stepsToHit) * stepMs;
                g63WindupOf = a.attack;
                g63Kind = Boss63KindG(g.sim, a.boss, a.attack);
            }
            // NIGHT63 G B2b: B2's SPOT frame, 70 % into the first wind-up of an attack on the fix list (on or off: the list is read either way, so
            // both sides frame the same pose; Shot30 adds D1's same-run off half)
            if (a.phase == BossPhase63.Windup && a.Windup01 >= 0.7 && !string.IsNullOrEmpty(a.boss) && !g63TellShot.Contains(a.boss)
                && BossTell63.Wants(a.boss, a.attack, Boss63KindG(g.sim, a.boss, a.attack), BossTell63.FixList(g.cfg)))
            {
                g63TellShot.Add(a.boss);
                Weak74Reel.Mark(g, a.boss, "tell", s => Boss63SayG(lines, s)); // NIGHT74 B weak74 (FeelReel.N74.B.cs): the open weak point's box and the parts still cyan, at this frame's end
                g.StartCoroutine(Shot30(dir, "B63-" + a.boss + "-tell"));
                Boss63SayG(lines, "[boss63] tell frame " + a.boss + " " + a.attack + " at wind-up " + a.Windup01.ToString("0.00", CultureInfo.InvariantCulture)
                    + ": shot-N30-B63-" + a.boss + "-tell.png | bossTell63 " + (BossTell63.On(g.cfg) ? "on" : "off") + ", recoloured " + BossTell63Probe.Recoloured + ", column " + (BossTell63Probe.Frame >= Time.frameCount - 1 ? "drawn" : "not drawn"));
            }
            if (a.phase == BossPhase63.Strike && !(g63Prev.phase == BossPhase63.Strike && g63Prev.attack == a.attack))
            {
                double hitMs = ms - a.phaseSteps * stepMs;
                if (g63WindupOf != a.attack || g63WindupMs < 0)
                {
                    g63NoWindup++;
                    Boss63SayG(lines, "[boss63] cue " + (a.boss ?? "?") + " " + (a.attack ?? "?") + " not measured: its wind-up began before the hold");
                }
                else
                {
                    // NIGHT63 G B2b: the reference reaches back to the previous hit (an ambient glow's cycle inside it); the verdict by the sim's kind
                    BossCue63.Use(g.cfg); // review S3: the colour table's bands
                    Cue63 c = BossCue63.Analyse(g63Ms, g63Frames, g63WindupMs, hitMs, g63LastHitMs);
                    g63Attacks++;
                    if (c.found) g63Found++;
                    string kind = g63Kind ?? Boss63KindG(g.sim, a.boss, a.attack);
                    if (c.parry) g63ParryDrawn++;
                    if (kind == "yellow") g63ParryKind++;
                    BossCue63.Verdict63 v = BossCue63.Verdict(c, kind);
                    if (v == BossCue63.Verdict63.Short) g63Short.Add(a.attack ?? "?");
                    else if (v == BossCue63.Verdict63.OffHue) g63OffHue.Add(a.attack ?? "?");
                    string what = c.found && g63Names.TryGetValue(c.strongest, out string nm) ? nm : null; // ids of run-time objects are negative
                    string dangerWhat = c.dangerCueFound && g63Names.TryGetValue(c.dangerCueId, out string dn) ? dn : null; // NIGHT63 G B2e
                    Boss63SayG(lines, BossCue63.CueLine(a.boss, a.attack, c, kind, what, dangerWhat));
                }
                g63WindupMs = -1;
                g63WindupOf = g63Kind = null;
                g63LastHitMs = hitMs;
            }
            g63Prev = a;
        }

        static partial void Boss63EndG(SurvivalGame g, string dir, string boss, List<string> lines)
        {
            BossCue63.Use(g != null ? g.cfg : null); // the end line names the bands it judged by
            Boss63SayG(lines, BossCue63.EndLine(boss, g63Attacks, g63Found, g63Short, g63OffHue, g63ParryDrawn, g63ParryKind)
                + (g63NoWindup > 0 ? " | not measured (wind-up before the hold) " + g63NoWindup : "")
                + (g63Roots.Count == 0 ? " | NO ROOTS: nothing of the boss was found, every cue reads none (not a measurement)" : ""));
            g63LastMs = -1; // the next run starts clean
        }

        static void Boss63SayG(List<string> lines, string line)
        {
            lines?.Add(line);
            Debug.Log(line);
        }

        /// <summary>The sim's tell colour for the attack: a Boss37Brain attack's kind; THE ICEBREAKER's block yellow, else red; the wyrm "?".</summary>
        static string Boss63KindG(SurvivalSim sim, string boss, string attack) => BossTell63.Kind(sim, boss, attack); // one rule for the measure and the fix

        /// <summary>The boss's own transform roots: the roots of the drawn renderers within Boss63CueCoreM of its centre or weak point on the
        /// hold's first live frame (the reveal: the reel's taps have not begun, so no player FX is there). Only things under these roots are
        /// measured: a pooled player FX (a tracer, a hit spark) near the boss is not a boss's cue. Said in a line.</summary>
        static void Boss63RootsG(List<string> lines, BossAttack63 a, double ms)
        {
            // n63-b2's red: the wyrm (emerging) and the crown (a ring, its centre empty) had nothing drawn within 3 m at the reveal, so their
            // cues read none. The boss views' own roots by name first (UnityTuning.Boss63CueRootNames: each view's root GameObject), then the
            // roots of what is drawn near its centre or weak point; tried again at each scan until one is found
            string[] byName = UnityTuning.Boss63CueRootNames.TryGetValue(a.boss ?? "", out string[] nm) ? nm : new string[0];
            Vector3 c = U63(a.centre), w = U63(a.weak);
            float core2 = UnityTuning.Boss63CueCoreM * UnityTuning.Boss63CueCoreM;
            foreach (Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (r == null || r is ParticleSystemRenderer || r is TrailRenderer) continue;
                Transform root = r.transform.root;
                if (Array.IndexOf(byName, root.name) >= 0) { g63Roots.Add(root); continue; }
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                Vector3 p = r.bounds.center;
                if ((p - c).sqrMagnitude <= core2 || (p - w).sqrMagnitude <= core2) g63Roots.Add(root);
            }
            if (g63Roots.Count == 0) return; // said once at the end (Boss63EndG) if no scan ever found one
            var names = new List<string>();
            foreach (Transform t in g63Roots) names.Add(t.name);
            names.Sort(StringComparer.Ordinal);
            Boss63SayG(lines, "[boss63] cue roots " + (a.boss ?? "?") + ": " + string.Join(", ", names) + " | at " + ms.ToString("0", CultureInfo.InvariantCulture) + " ms, phase " + a.phase);
        }

        /// <summary>The renderers and lights near the boss under its roots (inactive included), the player's own near the eye left out.</summary>
        static void Boss63ScanG(Vector3 centre, Vector3 eye)
        {
            g63Rends.Clear();
            g63Lights.Clear();
            float r2 = UnityTuning.Boss63CueRadiusM * UnityTuning.Boss63CueRadiusM, near2 = UnityTuning.Boss63CueNearCamM * UnityTuning.Boss63CueNearCamM;
            foreach (Renderer r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (r == null || r is ParticleSystemRenderer || r is TrailRenderer || !g63Roots.Contains(r.transform.root)) continue;
                Vector3 p = r.transform.position;
                if ((p - centre).sqrMagnitude > r2 || (p - eye).sqrMagnitude < near2) continue;
                g63Rends.Add(r);
                int id = r.GetInstanceID();
                if (!g63Names.ContainsKey(id)) g63Names[id] = (r.transform.parent != null ? r.transform.parent.name + "/" : "") + r.name;
            }
            foreach (Light l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (l == null || !g63Roots.Contains(l.transform.root)) continue;
                Vector3 p = l.transform.position;
                if ((p - centre).sqrMagnitude > r2 || (p - eye).sqrMagnitude < near2) continue;
                g63Lights.Add(l);
                int id = l.GetInstanceID();
                if (!g63Names.ContainsKey(id)) g63Names[id] = "light " + (l.transform.parent != null ? l.transform.parent.name + "/" : "") + l.name;
            }
        }

        /// <summary>This frame's drawn state and colour of every listed thing; the buffer keeps Boss63CueBufferS.</summary>
        static void Boss63SnapG(double ms)
        {
            var arr = new Lit63[g63Rends.Count + g63Lights.Count];
            int n = 0;
            foreach (Renderer r in g63Rends)
            {
                var x = new Lit63 { id = r != null ? r.GetInstanceID() : 0 };
                if (r != null && r.enabled && r.gameObject.activeInHierarchy && r.isVisible)
                {
                    Color c = Boss63DrawnG(r);
                    x.drawn = true;
                    x.r = c.r;
                    x.g = c.g;
                    x.b = c.b;
                }
                arr[n++] = x;
            }
            foreach (Light l in g63Lights)
            {
                var x = new Lit63 { id = l != null ? l.GetInstanceID() : 0 };
                if (l != null && l.enabled && l.gameObject.activeInHierarchy && l.intensity > 0f)
                {
                    Color c = l.color * l.intensity;
                    x.drawn = true;
                    x.r = c.r;
                    x.g = c.g;
                    x.b = c.b;
                }
                arr[n++] = x;
            }
            g63Ms.Add(ms);
            g63Frames.Add(arr);
            double keep = ms - UnityTuning.Boss63CueBufferS * 1000;
            int drop = 0;
            while (drop < g63Ms.Count && g63Ms[drop] < keep) drop++;
            if (drop > 0) { g63Ms.RemoveRange(0, drop); g63Frames.RemoveRange(0, drop); }
        }

        /// <summary>The colour a renderer is drawn with as the view set it: the brightest of its block's _BaseColor / _EmissionColor / _Color, else its
        /// material's; a line's start colour too.</summary>
        static Color Boss63DrawnG(Renderer r)
        {
            Color best = Color.clear;
            float bv = 0f;
            void Pick(Color c)
            {
                float v = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
                if (v > bv) { bv = v; best = c; }
            }
            if (r.HasPropertyBlock())
            {
                r.GetPropertyBlock(g63Block);
                Pick(g63Block.GetColor(G63Base));
                Pick(g63Block.GetColor(G63Emission));
                Pick(g63Block.GetColor(G63Color));
            }
            if (bv <= 0f)
            {
                Material m = r.sharedMaterial;
                if (m != null)
                {
                    if (m.HasProperty(G63Base)) Pick(m.GetColor(G63Base));
                    else if (m.HasProperty(G63Color)) Pick(m.GetColor(G63Color));
                    if (m.HasProperty(G63Emission)) Pick(m.GetColor(G63Emission));
                }
            }
            if (r is LineRenderer lr) Pick(lr.startColor);
            return best;
        }
    }
}
