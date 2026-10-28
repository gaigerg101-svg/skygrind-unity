// NIGHT63 builder D (slate C1, THE COLOUR TABLE; view row design.n63.colours63, OFF; the table: Colours63Math.N63D.cs). With the row on, the
// world's off-palette colours read the table, every frame:
//   - THE WORLD TIER: each material colour (_BaseColor) and each Light's colour under the world's roots (UnityTuning.Colours63WorldRoots: the
//     map and its decor, the light kits, the props, the roofs, the lava; never the enemies' "Husks" / "Swarmers" / "Spitters", a boss, the
//     viewmodel, the HUD or an effect) whose hue is off every role band at or over the route floor keeps its hue and its luminance at the table's
//     saturation: span's lime accent trims, kiln's blue trims, iceline's green accent, spire's green and amber fixtures and lights, the kit lamps'
//     amber edge lines (gg-proxy's "lime floor stripes" and "yellow edge lines");
//   - THE ROUTES: the lava's gold (LavaView30's _Gold) to danger; THE CROWN's goal column to the boundary (Route47View asks Route each frame:
//     gg-proxy's "danger-red line through the exit").
// The materials are found by two scans of each map (Colours63FirstScanFrames after it is seen: the build and decor54's first pass; then
// Colours63SecondScanS: after decor54's second pass at 3 s, on decor54's own clock, FeelClock.UnscaledDelta: NIGHT63 review S2), never in a
// same-run held frame; each keeps its own colour and its routed one.
// READ EVERY FRAME (D1's contract): NightRows.Live(decided, "colours63"); when it changes (D1's held off frame) every routed colour goes back to
// its own and returns after, nothing rebuilt or lost. A colour someone else changed since the scan is left alone from then on (counted "lost").
// Off (the row off, -skygrindOff63 or older, -skygrindNoColours63): nothing is scanned or touched. The HUD is never touched (GG 2026-10-20:
// "the UI looks good as it is"): the magenta dashes under the weapon name stay (the audit lists them). The proof (L-089): one Player log line per
// scan, "[colours63] <level> scan <n>: routed <k> materials (<per root>) and <j> lights, lava gold <m>; e.g. #from -> #to ...; off-palette left
// in other roots: ...", and Colours63.Line / Routed / LightsRouted / Toggles for a reel. No sim read, no tape bit, no audio.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.Colours63Math;

namespace Skygrind.Game
{
    public sealed class Colours63 : MonoBehaviour
    {
        static Colours63 instance;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"), GoldId = Shader.PropertyToID("_Gold");

        sealed class MatRoute
        {
            public Material m;
            public int prop;
            public Color orig, routed;
            public bool lost;
        }

        sealed class LightRoute
        {
            public Light l;
            public Color orig, routed;
            public bool lost;
        }

        /// <summary>For a reel: the state on this map, the colours routed, the lights routed, the times the live value changed.</summary>
        public static string Line = "colours63: not attached";
        public static int Routed, LightsRouted, Toggles, Lost;

        SurvivalGame game;
        SurvivalMap builtFor;
        GameConfig cfgFor;
        M.Table table;
        bool decided, applied;
        double scanSince; // NIGHT63 review S2: decor54's clock (FeelClock.UnscaledDelta) since the map was seen
        int scanFrames, scans;
        readonly List<MatRoute> mats = new List<MatRoute>();
        readonly List<LightRoute> lights = new List<LightRoute>();
        readonly HashSet<int> seenMats = new HashSet<int>(), seenLights = new HashSet<int>();
        HashSet<string> roots;

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying || instance != null) return;
            var go = new GameObject("Colours63") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            instance = go.AddComponent<Colours63>();
        }

        /// <summary>A per-frame caller's colour through its route: moved to the route's role (at its own luminance) while the row is on THIS frame,
        /// else unchanged. Allocation-free; read every frame (D1's contract).</summary>
        public static Color Route(string route, Color original)
        {
            Colours63 c = instance;
            if (c == null || !c.decided || c.table == null || !NightRows.Live(true, M.Row)) return original;
            string role = c.table.Route(route);
            if (role == null) return original;
            M.ToRole(c.table, role, original.r, original.g, original.b, out double r, out double g, out double b);
            return new Color((float)r, (float)g, (float)b, original.a);
        }

        void LateUpdate()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.cfg == null || g.map == null) return;
            bool fresh = false;
            if (!ReferenceEquals(g, game) || !ReferenceEquals(g.map, builtFor) || !ReferenceEquals(g.cfg, cfgFor)) { NewMap(g); fresh = true; }
            if (!decided || table == null) return;
            // NIGHT63 review S2: the scans on decor54's own clock (Decor54.Update: since += FeelClock.UnscaledDelta), counted from the frame after
            // the map is seen; a held frame neither counts nor scans. Never the wall clock (it ran ahead of the reel's fixed clock on a slow Player).
            if (!FeelClock.Holding)
            {
                if (!fresh)
                {
                    scanFrames++;
                    scanSince += FeelClock.UnscaledDelta;
                }
                if (Colours63Math.ScanDue(scans, scanFrames, scanSince) > 0) Scan();
            }
            bool live = NightRows.Live(true, M.Row); // every frame (D1's held off frame turns it off for one frame)
            if (live != applied) Apply(live);
        }

        void NewMap(SurvivalGame g)
        {
            if (applied) Apply(false); // the old map's shared materials back to their own colours
            mats.Clear();
            lights.Clear();
            seenMats.Clear();
            seenLights.Clear();
            game = g;
            builtFor = g.map;
            cfgFor = g.cfg;
            applied = false;
            scans = 0;
            scanFrames = 0; // NIGHT63 review S2: decor54's clock from here
            scanSince = 0;
            Routed = LightsRouted = Lost = 0;
            bool rowOn = NightRows.ForceOffFrom > M.Night && g.cfg.Design("n63." + M.Row + ".enabled"); // the config's own answer (D1's override is per frame, below)
            decided = M.Decide(rowOn, Args());
            table = decided ? M.From(g.cfg) : null;
            roots ??= new HashSet<string>(UnityTuning.Colours63WorldRoots, StringComparer.Ordinal);
            Line = !decided ? "colours63 OFF (" + (rowOn ? "a switch: -skygrindOff63 or older / " + M.NoSwitch : "design.n63." + M.Row + " off") + ")"
                : table == null ? "colours63 ON but design.n63.colours63.numbers does not read as a table: nothing routed" : "colours63 ON: waiting for the map's colours";
            if (decided) Debug.Log("[colours63] " + Line);
        }

        void Scan()
        {
            scans++;
            CultureInfo ci = CultureInfo.InvariantCulture;
            var perRoot = new Dictionary<string, int>(StringComparer.Ordinal);
            var leftRoots = new Dictionary<string, int>(StringComparer.Ordinal);
            var examples = new List<string>();
            int newMats = 0, newLights = 0, gold = 0, accentMoved = 0;
            // NIGHT72 C1 (slate C1, gg-proxy #2 "dark-red stripes ... red is supposed to mean danger"): a level whose accent wears danger (its hue inside the danger band
            // widened by UnityTuning.Colours72DangerMarginDeg) has every accent piece moved to Colours72ExitHueDeg at the world tier's saturation: the world tier alone kept
            // the rose's hue, so the stripes stayed red on the dark wall
            bool accentDanger = false;
            double accentHue = 0, accentSat = 0;
            if (Decor54.ScriptFor(game.cfg, game.map, out Decor54.Script accentScript))
            {
                M.Hsv(accentScript.accent.r, accentScript.accent.g, accentScript.accent.b, out accentHue, out accentSat, out _);
                accentDanger = M.InDangerReach(table, accentHue, UnityTuning.Colours72DangerMarginDeg);
            }
            foreach (MeshRenderer r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string rootName = r.transform.root.name;
                bool world = roots.Contains(rootName);
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    if (!world)
                    {
                        if (m.HasProperty(BaseColorId) && scans == 1) { Color c0 = m.GetColor(BaseColorId); if (M.Routes(table, c0.r, c0.g, c0.b)) leftRoots[rootName] = (leftRoots.TryGetValue(rootName, out int k0) ? k0 : 0) + 1; }
                        continue;
                    }
                    if (!seenMats.Add(m.GetInstanceID())) continue;
                    if (m.HasProperty(BaseColorId))
                    {
                        Color c = m.GetColor(BaseColorId);
                        if (accentDanger && IsLevelAccent(c, accentHue, accentSat))
                        {
                            M.WorldAt(table, c.r, c.g, c.b, UnityTuning.Colours72ExitHueDeg, out double ar, out double ag, out double ab);
                            AddMat(m, BaseColorId, c, new Color((float)ar, (float)ag, (float)ab, c.a));
                            newMats++;
                            accentMoved++;
                            perRoot[rootName] = (perRoot.TryGetValue(rootName, out int ka) ? ka : 0) + 1;
                            if (examples.Count < 4) examples.Add("accent " + Hex(c) + " -> " + Hex(mats[mats.Count - 1].routed));
                        }
                        else if (M.Routes(table, c.r, c.g, c.b))
                        {
                            M.World(table, c.r, c.g, c.b, out double wr, out double wg, out double wb);
                            AddMat(m, BaseColorId, c, new Color((float)wr, (float)wg, (float)wb, c.a));
                            newMats++;
                            perRoot[rootName] = (perRoot.TryGetValue(rootName, out int k) ? k : 0) + 1;
                            if (examples.Count < 4) examples.Add(Hex(c) + " -> " + Hex(mats[mats.Count - 1].routed));
                        }
                    }
                    string goldRole = table.Route("lavaGold");
                    if (goldRole != null && m.HasProperty(GoldId))
                    {
                        Color c = m.GetColor(GoldId);
                        M.ToRole(table, goldRole, c.r, c.g, c.b, out double gr, out double gg, out double gb);
                        AddMat(m, GoldId, c, new Color((float)gr, (float)gg, (float)gb, c.a));
                        gold++;
                        if (examples.Count < 6) examples.Add("lava gold " + Hex(c) + " -> " + Hex(mats[mats.Count - 1].routed));
                    }
                }
            }
            foreach (Light l in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!roots.Contains(l.transform.root.name) || !seenLights.Add(l.GetInstanceID())) continue;
                Color c = l.color;
                if (!M.Routes(table, c.r, c.g, c.b)) continue;
                M.World(table, c.r, c.g, c.b, out double lr, out double lg, out double lb);
                var e = new LightRoute { l = l, orig = c, routed = new Color((float)lr, (float)lg, (float)lb, c.a) };
                lights.Add(e);
                if (applied) l.color = e.routed;
                newLights++;
            }
            Routed = mats.Count;
            LightsRouted = lights.Count;
            var sb = new StringBuilder();
            foreach (KeyValuePair<string, int> kv in perRoot) sb.Append(sb.Length > 0 ? ", " : "").Append(kv.Key).Append(' ').Append(kv.Value.ToString(ci));
            var lb2 = new StringBuilder();
            foreach (KeyValuePair<string, int> kv in leftRoots) lb2.Append(lb2.Length > 0 ? ", " : "").Append(kv.Key).Append(' ').Append(kv.Value.ToString(ci));
            string level = game.map.json != null ? PostFx.LevelIdOf(game.map.json) : "?";
            Line = $"colours63 ON on {level}: routed {Routed} materials and {LightsRouted} lights after scan {scans}";
            Debug.Log($"[colours63] {level} scan {scans}: routed {newMats} materials ({(sb.Length > 0 ? sb.ToString() : "none")}; {accentMoved} of them the level's accent out of the danger reach: {(accentDanger ? "accent hue " + accentHue.ToString("0", ci) + " -> hue " + UnityTuning.Colours72ExitHueDeg.ToString("0", ci) : "the accent does not wear danger")}) and {newLights} lights, lava gold {gold} (total {Routed} materials, {LightsRouted} lights){(examples.Count > 0 ? "; e.g. " + string.Join(", ", examples.ToArray()) : "")}{(scans == 1 ? "; off-palette left in other roots (not the world's: enemies, bosses, effects, the HUD): " + (lb2.Length > 0 ? lb2.ToString() : "none") : "")}; live {(applied ? "on" : "off")}");
        }

        /// <summary>A material colour that is the level's accent: hue within UnityTuning.Colours72AccentHueDeg of it (HDR allowed: hue and saturation are its own at any
        /// intensity), saturation at least Colours72AccentSatShare of it, not near-black.</summary>
        static bool IsLevelAccent(Color c, double accentHue, double accentSat)
        {
            M.Hsv(c.r, c.g, c.b, out double h, out double s, out double v);
            double d = Math.Abs(h - accentHue) % 360;
            if (d > 180) d = 360 - d;
            return v >= UnityTuning.Colours72AccentMinValue && d <= UnityTuning.Colours72AccentHueDeg && s >= accentSat * UnityTuning.Colours72AccentSatShare;
        }

        void AddMat(Material m, int prop, Color orig, Color routed)
        {
            var e = new MatRoute { m = m, prop = prop, orig = orig, routed = routed };
            mats.Add(e);
            if (applied) m.SetColor(prop, routed);
        }

        /// <summary>Every routed colour to its routed value (on) or back to its own (off); a colour changed by someone else since is left alone.</summary>
        void Apply(bool on)
        {
            applied = on;
            Toggles++;
            foreach (MatRoute e in mats)
            {
                if (e.lost || e.m == null) continue;
                Color cur = e.m.GetColor(e.prop), from = on ? e.orig : e.routed;
                if (cur != from) { e.lost = true; Lost++; continue; }
                e.m.SetColor(e.prop, on ? e.routed : e.orig);
            }
            foreach (LightRoute e in lights)
            {
                if (e.lost || e.l == null) continue;
                Color from = on ? e.orig : e.routed;
                if (e.l.color != from) { e.lost = true; Lost++; continue; }
                e.l.color = on ? e.routed : e.orig;
            }
        }

        static string Hex(Color c)
        {
            int r = Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255), g = Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255), b = Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
            return "#" + r.ToString("X2", CultureInfo.InvariantCulture) + g.ToString("X2", CultureInfo.InvariantCulture) + b.ToString("X2", CultureInfo.InvariantCulture) + (c.maxColorComponent > 1f ? " (HDR x" + c.maxColorComponent.ToString("0.0", CultureInfo.InvariantCulture) + ")" : "");
        }

        void OnDestroy()
        {
            if (applied) Apply(false);
        }
    }
}
