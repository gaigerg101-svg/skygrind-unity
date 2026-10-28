// NIGHT74 builder L, slice L3 (view row design.n74.crown74; the shader half: the _Crown74 block of Assets/Shaders/Resources/Ceiling27.shader AND, since the retake, of Assets/Shaders/Stone.shader: n74-t2's Player pair showed the ceiling alone
// lifting the region a third of the model's number, because stagger / tell have only 35-65 % ceiling in the region and the rest is the arena's stone). GG 2026-10-26 "a lot of the room especially the boss rooms are very dark So we need to make sure to fix that";
// gg-proxy-close #9: THE CROWN's ceiling centre is near black at 900-1300 x 150-450 (crown37-idle, -flinch, -tell, -stagger of the n73 freeze build).
// What was found (read from the n73-freeze boss63 frames, Logs/feel/n73-freeze/bp/boss63/shot-N30-B63-crown37-*.png, a dark-pixel map of each): the dark pixels are the CEILING (RoofView27's coffered ceiling, SKYGRIND/Ceiling27: an UNLIT shader,
// a flat colour with fog), darkest over the arena's far middle where the fog is deepest; the walls, the floor and the sky are not. So a point light cannot lift it (the shader reads no light), and lit72's display lift cannot take a pixel at luma 1-4 over
// 30 (its gain cap, kmax 9). The fix is where the dark is made: the ceiling's own pixel, in display space, a cool fill added to the darkest, none to a lit beam (Ceiling27.shader), faded in over 0.8 s when THE CROWN's fight is on and out when it is not.
// The fill is cool by construction (red never over green, blue leading: no danger hue, CLAUDE.md rule 8; not cyan, which is the player's): UnityTuning.N74CrownFill*.
// View only: it sets two shader globals from the sim's public state (the Crown's alive flag) and writes nothing to the sim; no tape bit, no audio. The weight is computed every frame from the room whether the row is on or not (the row only gates what the shader
// is handed), so the frame the row turns ON is the frame it would have drawn (D1's contract; NightRows.ViewOn every frame). Self-hooked (Goal72View's way: a hidden object made at scene load hooks the live SurvivalGame's OnRender / OnNewRun).
// Off: the row off (-skygrindRows72 crown74:0): the shader's block is skipped (_Crown74.a = 0) and the ceiling is as it was. Tuning without a rebuild (the lead's retake): -skygrindCrown74Tune=<k> scales the fill (default 1).
// Probe: Crown74Probe (the Player log's `[crown74]` lines).
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Crown74Probe
    {
        public static bool On, Crown;
        public static float Weight, Scale = 1f;
        public static int Ups, Downs;

        public static string Line() => $"crown74 on {On}, the Crown's fight {Crown}, weight {Weight:0.00}, fill x{Scale:0.##} ({UnityTuning.N74CrownFillR * Scale:0.000}, {UnityTuning.N74CrownFillG * Scale:0.000}, {UnityTuning.N74CrownFillB * Scale:0.000}), lifts {Ups}, fades {Downs}";
    }

    public sealed class Crown74View : MonoBehaviour
    {
        public const string Row = "crown74";
        public const string TuneSwitch = "-skygrindCrown74Tune";

        static readonly int FillId = Shader.PropertyToID("_Crown74"), ShapeId = Shader.PropertyToID("_Crown74B");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Crown74View>() != null) return;
            var go = new GameObject("Crown74");
            DontDestroyOnLoad(go);
            go.AddComponent<Crown74View>();
        }

        SurvivalGame game;
        float weight, scale = 1f;
        bool lastLive, fullLogged;
        Vector4 sentFill = new Vector4(-1, -1, -1, -1);

        void Awake()
        {
            scale = ReadScale(Environment.GetCommandLineArgs());
            Crown74Probe.Scale = scale;
            Shader.SetGlobalVector(ShapeId, new Vector4(UnityTuning.N74CrownL0, UnityTuning.N74CrownL1, 0f, 0f));
            Shader.SetGlobalVector(FillId, Vector4.zero);
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
            g.OnNewRun += () => { weight = 0f; lastLive = false; Send(Vector4.zero); };
        }

        /// <summary>-skygrindCrown74Tune=k (or the number as the next argument): the fill's scale, 0.2 .. 3 (default 1).</summary>
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
                    Debug.Log("[crown74] fill scale x" + Mathf.Clamp(k, 0.2f, 3f).ToString("0.##", CultureInfo.InvariantCulture) + " (" + TuneSwitch + ")");
                    return Mathf.Clamp(k, 0.2f, 3f);
                }
            }
            return 1f;
        }

        void Send(Vector4 fill)
        {
            if (fill == sentFill) return;
            sentFill = fill;
            Shader.SetGlobalVector(FillId, fill);
        }

        void Render(double frameDt, double frameAlpha)
        {
            SurvivalGame g = game;
            if (g == null || g.sim == null || g.cfg == null) return;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1)
            Crown37 c = g.sim.crown37;
            bool fight = g.flow == Flow.Playing && c != null && c.Alive; // THE CROWN's fight: its arena is the room whose ceiling is dark
            weight = Lit72Math.Step(weight, fight ? 1f : 0f, (float)frameDt, UnityTuning.N74CrownFadeInS, UnityTuning.N74CrownFadeOutS);
            bool live = on && weight > 0.0005f;
            Crown74Probe.On = on;
            Crown74Probe.Crown = fight;
            Crown74Probe.Weight = weight;
            Send(live ? new Vector4(UnityTuning.N74CrownFillR * scale, UnityTuning.N74CrownFillG * scale, UnityTuning.N74CrownFillB * scale, weight) : Vector4.zero);
            if (live != lastLive)
            {
                lastLive = live;
                fullLogged = false;
                if (live) Crown74Probe.Ups++; else Crown74Probe.Downs++;
                Debug.Log("[crown74] " + (live ? "ceiling lift ON" : "ceiling lift off") + ": " + Crown74Probe.Line());
            }
            else if (live && !fullLogged && weight >= 0.999f)
            {
                fullLogged = true; // the retake's evidence: the weight did reach 1 (n74-t2's lift was a third of the model's; a fade still running at the shot would have been one reason)
                Debug.Log("[crown74] weight 1.00 reached: " + Crown74Probe.Line());
            }
        }
    }
}
