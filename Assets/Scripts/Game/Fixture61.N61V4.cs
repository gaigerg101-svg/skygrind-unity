// NIGHT61 builder V (slate S4; view row design.n61.fixture61, OFF in the repo; Tools/night/n61-v4-rows.cjs): LIGHTS HANG FROM REAL FIXTURES.
// GG 2026-10-11: lights look "placed and floating" and "crappy"; the N55 critic: "bare white rectangles hang in the ceilings" (a flat white quad, no
// housing, cable or rod, under a black ceiling), fixed looks like "a dark frame >= 4 px at 1080p and a hanger to the beam"; gg-proxy at NIGHT61's start:
// iceline's verbs37 f0300, a bright ceiling panel under the beams, no mount. PRINCIPLES 7: lights hang from real fixtures. View geometry only.
// On every campaign level with the light plan (look33 on: LightPlan33.GreenFor, the plan CampaignLighting24 / Fixtures33 / LightKit29 draw), each
// ceiling light the plan draws gets its mounts (Fixture61Math.Mounts): a dark housing round the panel and hanger rails to the nearest joists
// (lights35's beams), else to the nearest wall. One stone mesh in colors.pillarSide x UnityTuning.Fixture61Shade, no collider, no shadow.
// Nothing else changes: no light moves, dims or flickers (the plan's lights and fixtures are only read; the mounts cast no shadow on the key
// spot's hard shadows), and every part stays within Rods40WallClear under its ceiling, so rods40's play volume never meets one.
// A panel Fixtures33 does not draw (a ceiling socket's panel straight over a grapple node: LightKit29.OverNode33) gets no mounts either.
// Off: the row off, -skygrindOff61 (or older) or -skygrindNoFixture61 (this row alone). The proof (L-089): one Player log line per map built,
// "[fixture61] mounts on <level>: ceiling lights <n>, housed <n>, held by a beam <n>, by a wall <n>, unmounted <n>" (ExitFrame61's pattern);
// Fixture61.Line for a reel. Numbers: UnityTuning.N61V4.cs (Unity-only). No audio, no gameplay number, no sim row.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.Fixture61Math;

namespace Skygrind.Game
{
    public sealed class Fixture61 : MonoBehaviour
    {
        /// <summary>For a reel: the mounts' state on the current map.</summary>
        public static string Line = "fixture mounts: not attached";
        /// <summary>For a reel: the meter on the current map (the mounts drawn).</summary>
        public static M.Report Now;

        static string[] args;

        /// <summary>This process's command line, read once (none readable: no switch).</summary>
        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Off for this config and this process's command line (Fixture61Math.OffFor).</summary>
        public static bool Off(GameConfig cfg) => M.OffFor(cfg, Args());

        SurvivalGame game;
        SurvivalMap builtFor;
        Transform root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Fixture61") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Fixture61>();
        }

        bool Attach()
        {
            game = SurvivalGame.Instance;
            if (game == null || game.mats == null || game.mats.stone == null || game.cfg == null) { game = null; return false; }
            builtFor = null;
            return true;
        }

        void Build()
        {
            builtFor = game.map;
            if (root != null) Destroy(root.gameObject);
            root = null;
            Now = default;
            GameConfig cfg = game.cfg;
            string why = M.OffReason(cfg, Args());
            if (why != null) { Line = "fixture mounts OFF (" + why + ")"; return; }
            Dictionary<string, object> json = game.map != null ? game.map.json : null;
            string name = json != null && json.TryGetValue("name", out object no) ? no as string : null;
            if (json == null || !json.ContainsKey("campaign")) { Line = $"fixture mounts: not a campaign level ({name ?? "no map"})"; return; }
            if (!N33Fixes.ViewRowOn(cfg, "look33")) { Line = "fixture mounts: no light plan (look33 off)"; return; }
            LightPlan33.Plan plan = LightPlan33.GreenFor(json, cfg, N30Fixes.ViewRowOn(cfg, "look30"), out LightPlan33.Level lv); // the plan the views draw (cached per map)
            if (plan == null) { Line = $"fixture mounts: {name} has no light plan"; return; }
            SurvivalGame g = game;
            Func<LightPlan33.Fix, bool> drawn = f => !(f.kind == "lens" && f.type == "panel" && LightKit29.OverNode33(g, f)); // Fixtures33's own skip
            List<M.Mount> mounts = M.Mounts(lv, plan, M.Dims.Tuning, drawn);
            Now = M.Count(plan, mounts, UnityTuning.Fixture61MinRimM, UnityTuning.Look33FrameWidth, drawn);
            if (mounts.Count > 0)
            {
                var kit = new MeshKit();
                Color shade = Look.C(cfg, "colors.pillarSide") * UnityTuning.Fixture61Shade;
                foreach (M.Mount m in mounts)
                {
                    LightPlan33.Box b = m.box;
                    if (b.x1 - b.x0 < 1e-3 || b.y1 - b.y0 < 1e-3 || b.z1 - b.z0 < 1e-3) continue; // a hanger with no length (the housing meets the beam)
                    kit.Box(new Vec3(b.x0, b.y0, b.z0), new Vec3(b.x1, b.y1, b.z1), shade, shade);
                }
                root = new GameObject("Fixture mounts 61").transform;
                DontDestroyOnLoad(root.gameObject);
                MeshKit.Object("Fixture mounts 61", root, kit.Build("fixture61 mounts"), game.mats.stone, shadows: false);
            }
            Line = M.LogLine(name ?? "?", Now);
            Debug.Log(Line);
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (SurvivalGame.Instance != game) { game = null; return; }
            if (builtFor != game.map) Build();
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
        }
    }
}
