// NIGHT47 builder F (design.n47.grapple47, a view row; NIGHT47.md G23; GG 2026-10-18: "The right-bumper grapple should replace the dotted line with a
// single long glowing aura-like line that feels powerful and stylish"). The bind is RB on the live pad layout v4 (F-cards 1e: SPECIAL + ARCLINE),
// G on the keyboard; GG's name is right. The "dotted line" was Grapple31View's five pull beads over the rope (F-cards 1e). With the row on (and
// view33's rope drawing), after Rope33Render has placed the rope's points:
//   - the rope is one continuous ribbon: an HDR white-cyan CORE (bloom picks it up) tapering from the hand to the hook, inside a soft additive
//     GLOW shell Grapple47ShellMul x its width in cyan; no dashes; Grapple31View's beads are not drawn;
//   - fire: the line flies out with a decaying wave (Grapple47WaveAmp m, Grapple47Waves waves along it, straight in Grapple47WaveS);
//   - pull: a bright PULSE runs hook -> hand every Grapple47PulseS (it says "you are being pulled", which the beads said);
//   - the BEND (VERBS 22): the line brightens Grapple47BendGain x and its pulse slows with the bend's time (sim.Bend34WorldAnimMul);
//   - release: Rope33's snap keeps its whip; the shell fades with it.
// Off (the row off, -skygrindOff47): view33's rope and the beads as NIGHT46. Numbers: UnityTuning.F47.cs (Grapple47*).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>grapple47's switch and probes (the a747 reel; Grapple31View reads On to hide its beads).</summary>
    public static class Grapple47
    {
        public static bool? Force;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "grapple47"); }
            return on;
        }

        /// <summary>For the reel: the ribbon drawn this frame, its phase ("fly" / "pull" / "hold" / "snap"), the wave's amplitude, the pulse's place (0 hook .. 1 hand).</summary>
        public static bool Drawn;
        public static string Phase = "";
        public static float WaveNow, PulseAt = -1f;
    }

    public sealed partial class Arcline21View
    {
        LineRenderer shell47, pulse47;
        Material shellMat47, pulseMat47;
        Texture2D glowTex47;
        Vector3[] pts47;
        float flyAge47 = -1f, pulseClock47;
        bool built47, tapered47;
        AnimationCurve taper47, flat47;

        void Build47()
        {
            built47 = true;
            pts47 = new Vector3[Mathf.Max(2, rope.positionCount)];
            taper47 = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, UnityTuning.Grapple47TipShare));
            flat47 = AnimationCurve.Constant(0f, 1f, 1f);
            Transform root = rope.transform.parent;
            shellMat47 = FxDraw.Additive(game.mats.neon, "Grapple 47 shell");
            pulseMat47 = FxDraw.Additive(game.mats.neon, "Grapple 47 pulse");
            // n47-r1b ("thin and without a visible glow"): the shell was a flat band; it gets a soft falloff across its width (a made texture: a
            // Gaussian in v, white on black for the additive blend), so it reads as a glow round the core, not a wider stripe
            glowTex47 = new Texture2D(4, UnityTuning.Grapple47GlowTexH, TextureFormat.RGBA32, false) { name = "Grapple 47 glow", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[4 * UnityTuning.Grapple47GlowTexH];
            for (int y = 0; y < UnityTuning.Grapple47GlowTexH; y++)
            {
                float v = (y + 0.5f) / UnityTuning.Grapple47GlowTexH * 2f - 1f; // -1 .. 1 across the width
                byte b = (byte)Mathf.RoundToInt(255f * Mathf.Exp(-v * v * UnityTuning.Grapple47GlowFalloff));
                for (int x = 0; x < 4; x++) px[y * 4 + x] = new Color32(b, b, b, 255);
            }
            glowTex47.SetPixels32(px);
            glowTex47.Apply(false, true);
            shellMat47.SetTexture("_BaseMap", glowTex47);
            pulseMat47.SetTexture("_BaseMap", glowTex47);
            shell47 = Line47("Grapple 47 shell", root, shellMat47);
            pulse47 = Line47("Grapple 47 pulse", root, pulseMat47);
            pulse47.positionCount = 2;
        }

        static LineRenderer Line47(string name, Transform root, Material m)
        {
            var l = new GameObject(name).AddComponent<LineRenderer>();
            l.transform.SetParent(root, false);
            l.useWorldSpace = true;
            l.alignment = LineAlignment.View;
            l.sharedMaterial = m;
            l.shadowCastingMode = ShadowCastingMode.Off;
            l.receiveShadows = false;
            l.enabled = false;
            return l;
        }

        /// <summary>From Arcline21View.Render right after Rope33Render (the rope's points placed this frame). Off: the core's width and curve back.</summary>
        void Grapple47Render(SurvivalSim sim, Rope r, bool roped, double worldDt)
        {
            bool on = view33 && Grapple47.On(game.cfg);
            if (!on)
            {
                if (built47 && tapered47)
                {
                    tapered47 = false;
                    shell47.enabled = pulse47.enabled = false;
                    rope.widthCurve = flat47;
                    rope.widthMultiplier = (float)game.cfg.Num("arcline.ropeWidth") * UnityTuning.Arcline21RopeWidthMul;
                }
                Grapple47.Drawn = false;
                return;
            }
            if (!built47) Build47();
            float dt = (float)worldDt;
            if (!rope.enabled)
            {
                shell47.enabled = pulse47.enabled = false;
                flyAge47 = -1f;
                Grapple47.Drawn = false;
                Grapple47.Phase = "";
                return;
            }
            int n = rope.positionCount;
            if (pts47.Length != n) pts47 = new Vector3[n]; // the rope's point count is fixed (view33: ropeSegments + 1): made once
            rope.GetPositions(pts47);
            bool flying = roped && r.phase == RopePhase.Flying;
            bool attached = roped && r.phase == RopePhase.Attached;
            // the fire's wave: perpendicular to the line in the view, dying over Grapple47WaveS from the first flying frame
            if (flying) flyAge47 = flyAge47 < 0 ? 0 : flyAge47 + dt;
            else if (!roped) flyAge47 = -1f;
            float wave = flying && flyAge47 < UnityTuning.Grapple47WaveS ? UnityTuning.Grapple47WaveAmp * (1f - flyAge47 / UnityTuning.Grapple47WaveS) : 0f;
            if (wave > 0f)
            {
                Vector3 side = game.cam.transform.right;
                for (int i = 0; i < n; i++)
                {
                    float t = i / (float)(n - 1);
                    pts47[i] += side * (wave * Mathf.Sin(t * Mathf.PI * 2f * UnityTuning.Grapple47Waves - flyAge47 * 40f) * Mathf.Sin(t * Mathf.PI));
                }
                rope.SetPositions(pts47);
            }
            bool bend = sim.Bend34Inside;
            float gain = bend ? UnityTuning.Grapple47BendGain : 1f;
            Color cyan = ropeColor;
            Color core = Color.Lerp(cyan, Color.white, UnityTuning.Grapple47CoreWhite) * (UnityTuning.Grapple47CoreGlow * gain);
            // the core: thin, tapering hand -> hook; the shell: the same points, wider, softer
            if (!tapered47) { rope.widthCurve = taper47; shell47.widthCurve = taper47; tapered47 = true; }
            rope.widthMultiplier = UnityTuning.Grapple47CoreW;
            ropeMat.SetColor(DesignKit.BaseColor, core);
            shell47.positionCount = n;
            shell47.SetPositions(pts47);
            shell47.widthMultiplier = UnityTuning.Grapple47CoreW * UnityTuning.Grapple47ShellMul;
            shellMat47.SetColor(DesignKit.BaseColor, cyan * (UnityTuning.Grapple47ShellGlow * gain));
            shell47.enabled = true;
            // the pull's pulse: hook -> hand every Grapple47PulseS (slowed with the bend's time)
            bool pull = attached;
            if (pull)
            {
                float timeMul = bend ? (float)sim.Bend34WorldAnimMul : 1f;
                pulseClock47 = (pulseClock47 + dt * timeMul / UnityTuning.Grapple47PulseS) % 1f;
                float u = 1f - pulseClock47; // 1 at the hook (the rope's last point) -> 0 at the hand
                float len = 0f;
                for (int i = 1; i < n; i++) len += Vector3.Distance(pts47[i - 1], pts47[i]);
                float du = len > 0.01f ? UnityTuning.Grapple47PulseLen / len : 1f;
                pulse47.SetPosition(0, Along47(n, Mathf.Clamp01(u)));
                pulse47.SetPosition(1, Along47(n, Mathf.Clamp01(u - du)));
                pulse47.widthMultiplier = UnityTuning.Grapple47CoreW * UnityTuning.Grapple47PulseWidthMul;
                pulseMat47.SetColor(DesignKit.BaseColor, Color.white * (UnityTuning.Grapple47PulseGlow * gain));
                pulse47.enabled = true;
                Grapple47.PulseAt = u;
            }
            else { pulse47.enabled = false; Grapple47.PulseAt = -1f; }
            Grapple47.Drawn = true;
            Grapple47.Phase = flying ? "fly" : attached ? "pull" : roped ? "hold" : "snap";
            Grapple47.WaveNow = wave;
        }

        /// <summary>A point at `u` (0 the hand .. 1 the hook) along this frame's rope points.</summary>
        Vector3 Along47(int n, float u)
        {
            float f = u * (n - 1);
            int i = Mathf.Min(n - 2, Mathf.FloorToInt(f));
            return Vector3.Lerp(pts47[i], pts47[i + 1], f - i);
        }
    }
}
