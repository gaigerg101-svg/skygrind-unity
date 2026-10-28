// NIGHT74 builder L, leftover 4 (NIGHT74.md section 7: "THE ICEBREAKER's arena at 19.8 % near-black (lit72 0.18 reached 14.6 % unseen): a local fill, not a global lift"; view row design.n74.ice74).
// GG 2026-10-26 "a lot of the room especially the boss rooms are very dark"; gg-proxy-close #9: THE ICEBREAKER's side walls are near black. What was found (the n73-d bi side, then the n74-t4 bi0 side, lit72 ON: Logs/feel/n74-t4/bi0/boss63/shot-N30-B63-brawl40-*.png):
// the dark pixels are the arena's near rim walls and pillars (20.1 % of the idle frame under luma 30, 19.0 % windup, 13.8 % stagger, 8.7 % flinch), and their pixels sit at display luma 0.010 BEFORE lit72's lift (2, 2, 6 of 255), so lit72's gain cap (x9) leaves them at 23.
// FIRST TRY, WRONG (n74-t4: no change at all, and the doubled fill identical to the single one): an ambient fill added to RenderSettings.ambientProbe. The arena's walls, pillars and ice are URP Lit parts (Light54.StoneOrLit: "[light54] stone colour: Brawl40View brawl40 wall ... drawn lit"), and the
// probe write never reached them (not found why: the probe is the Stone.shader's light; nothing measured shows the Lit parts take a runtime write). That code is gone.
// THE FIX, in the place whose effect the Player has measured (lit72's own: bi0 -> bi, 48 % -> 20 % dark): the same display-space lift in Crt29.shader's Lit72 block, with the arena's numbers while the player stands in THE ICEBREAKER's arena: a gain cap of 20 instead of 9 (a pixel at luma
// 0.010 takes (0.010 + A) / 0.010 = 13-15, not 9) and A 0.14 instead of 0.12. Crt29Pass.cs asks Ice74View.PostVector for the vector it hands the shader (the shader is not touched: the lift is lit72's block with other numbers). Local to the arena (a weight from where the player stands,
// faded over 0.8 s), hue-keeping (the colour is scaled, never replaced: the walls stay navy), and a no-op on lit pixels (w = 0 from l1 0.42). Tools/night/n74-ice-model.py re-runs lit72's curve with these numbers on the t4 frames and is exact in its mechanism: dark share 0 on all four.
// View only: it writes shader numbers from the sim's public state (the arena's centre and size, the player's feet); no tape bit, no audio. The weight is computed every frame from where the player is whether the row is on or not (the row only gates what the pass is handed), so the frame
// the row turns ON is the frame it would have drawn (D1's contract; NightRows.ViewOn every frame). Self-hooked (Goal72View's way). Off: the row off (-skygrindRows72 ice74:0). Tuning without a rebuild (the lead's retake): -skygrindIce74Tune=k scales A and the gain cap (default 1).
// Probe: Ice74Probe (the Player log's `[ice74]` lines: the transition, and one when the weight reaches 1).
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Ice74Probe
    {
        public static bool On, InArena;
        public static float Weight, Scale = 1f;
        public static int Ups, Downs;

        public static string Line() => $"ice74 on {On}, player in the arena {InArena}, weight {Weight:0.00}, pass numbers A {UnityTuning.N74IceA * Scale:0.000}, l1 {UnityTuning.N74IceL1:0.00}, gain cap {UnityTuning.N74IceCap * Scale:0.#} (x{Scale:0.##}), lifts {Ups}, fades {Downs}";
    }

    public sealed class Ice74View : MonoBehaviour
    {
        public const string Row = "ice74";
        public const string TuneSwitch = "-skygrindIce74Tune";

        /// <summary>The row is ON and the weight is above 0: the CRT pass is handed the arena's lift numbers (Crt29Pass reads it).</summary>
        public static bool PostLive { get; private set; }
        public static float PostWeight { get; private set; }
        static float postScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Ice74View>() != null) return;
            var go = new GameObject("Ice74");
            DontDestroyOnLoad(go);
            go.AddComponent<Ice74View>();
        }

        /// <summary>Whether a point is inside the arena's rim (the floor's half size plus a margin), above its floor by less than its height: the arena is a closed box.</summary>
        public static bool Inside(double px, double py, double pz, double cx, double cy, double cz, double half, double height, double margin)
            => Math.Abs(px - cx) <= half + margin && Math.Abs(pz - cz) <= half + margin && py >= cy - 2.0 && py <= cy + height + 2.0;

        /// <summary>The numbers handed to Crt29.shader's Lit72 block (x A x weight, y l0, z l1, w gain cap): lit72's own while this is not live, else the larger of lit72's and the arena's (the shader's
        /// block is skipped at x = 0, so a live arena lift runs it even when lit72 reads off). Pure: the test pins it.</summary>
        public static Vector4 PostVectorOf(Vector4 lit72, bool live, float weight, float scale)
        {
            if (!live) return lit72;
            float a = Mathf.Max(lit72.x, UnityTuning.N74IceA * scale * weight);
            float l0 = lit72.x > 0f ? lit72.y : UnityTuning.N74IceL0;
            float l1 = Mathf.Max(lit72.x > 0f ? lit72.z : 0f, UnityTuning.N74IceL1);
            float cap = Mathf.Max(lit72.x > 0f ? lit72.w : 1f, UnityTuning.N74IceCap * scale);
            return new Vector4(a, l0, Mathf.Max(l1, l0 + 0.01f), cap);
        }

        /// <summary>Crt29Pass: lit72's vector with the arena's numbers in it while the player is in THE ICEBREAKER's arena and the row is ON.</summary>
        public static Vector4 PostVector(Vector4 lit72) => PostVectorOf(lit72, PostLive, PostWeight, postScale);

        SurvivalGame game;
        float weight, scale = 1f;
        bool lastLive, fullLogged;

        void Awake()
        {
            scale = ReadScale(Environment.GetCommandLineArgs());
            postScale = scale;
            Ice74Probe.Scale = scale;
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            weight = 0f;
            lastLive = false;
            g.OnRender += Render;
            g.OnNewRun += () => { weight = 0f; lastLive = false; PostLive = false; PostWeight = 0f; };
        }

        /// <summary>-skygrindIce74Tune=k (or the number as the next argument): scales A and the gain cap, 0.2 .. 4 (default 1).</summary>
        public static float ReadScale(string[] args)
        {
            if (args == null) return 1f;
            for (int i = 0; i < args.Length; i++)
            {
                string v = null;
                if (args[i] != null && args[i].StartsWith(TuneSwitch + "=", StringComparison.OrdinalIgnoreCase)) v = args[i].Substring(TuneSwitch.Length + 1);
                else if (string.Equals(args[i], TuneSwitch, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) v = args[i + 1];
                if (v != null && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out float k))
                {
                    float c = Mathf.Clamp(k, 0.2f, 4f);
                    Debug.Log("[ice74] scale x" + c.ToString("0.##", CultureInfo.InvariantCulture) + " (" + TuneSwitch + ")");
                    return c;
                }
            }
            return 1f;
        }

        void Render(double frameDt, double frameAlpha)
        {
            SurvivalGame g = game;
            if (g == null || g.sim == null || g.cfg == null) return;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1)
            SurvivalSim sim = g.sim;
            bool inArena = false;
            if (g.flow == Flow.Playing && sim.brawl40Centre.HasValue && sim.brawl40Arena != null)
            {
                Vec3 c = sim.brawl40Centre.Value, f = g.Feet;
                inArena = Inside(f.x, f.y, f.z, c.x, c.y, c.z, sim.brawl40Arena.half, sim.brawl40Arena.height, UnityTuning.N74IceMarginM);
            }
            weight = Lit72Math.Step(weight, inArena ? 1f : 0f, (float)frameDt, UnityTuning.N74IceFadeInS, UnityTuning.N74IceFadeOutS);
            bool live = on && weight > 0.0005f;
            PostLive = live;
            PostWeight = weight;
            Ice74Probe.On = on;
            Ice74Probe.InArena = inArena;
            Ice74Probe.Weight = weight;
            if (live != lastLive)
            {
                lastLive = live;
                fullLogged = false;
                if (live) Ice74Probe.Ups++; else Ice74Probe.Downs++;
                Debug.Log("[ice74] arena lift " + (live ? "ON" : "off") + ": " + Ice74Probe.Line());
            }
            else if (live && !fullLogged && weight >= 0.999f)
            {
                fullLogged = true; // the retake's evidence: the weight did reach 1 (n74-t4's log had only the first frame's 0.02)
                Debug.Log("[ice74] weight 1.00 reached: " + Ice74Probe.Line());
            }
        }
    }
}
