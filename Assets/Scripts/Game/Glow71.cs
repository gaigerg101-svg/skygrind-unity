// NIGHT71 builder F (LEFTOVER L6, T-113 "PULSE's glow"; design.n71.glow71, a VIEW row: no sim read, no tape bit). gunglow50 (ON since NIGHT50)
// lights the charge on the gun in four steps, but the PULSE was left out (DECIDE50 D-8): its steps ran along the muzzle line, inside the
// gauntlet's forearm block (BlockyKit.Gauntlet: the forearm 0.086 x 0.078 x 0.15 m round y -0.004; the top plate's top at y 0.045), so the
// 0 % and 75 % frames read alike (n50-r1 a150, glow-gauntlet-0 / -75). With the row on, the gauntlet gets its own four steps ON TOP of its
// body (the body mesh's bounds' top, numbers.lift above it), from the back toward the knuckles over numbers.span of its length, in the
// player's cyan (Look49UsableHex, as the other guns' steps); they light with the PULSE's pips while its BLAST is off cooldown (gunglow50's
// own read, Viewmodel.Glow50Level), the white flash at full and the gun still as gunglow50's. Built hidden whenever gunglow50 is on; drawn
// only on a frame the row reads on (NightRows.ViewOn every frame; the row is in SameRun63Math.FrameRows). Off: the row off, -skygrindOff71 or
// older, -skygrindNoGlow71 (any case), gunglow50 off; a number missing keeps it off. Numbers: design.n71.glow71.numbers (both config copies).
// Proof: the a150 reel's gunglow50 pass (the PULSE: "NO STEPS on this rig" with the row off; four steps, the luma rising, the flash and the
// gun still with it on: "gunglow50 ok: KESTREL THUNDERCLAP PULSE").
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public static class Glow71
    {
        public const string Row = "glow71";
        public const string NoSwitch = "-skygrindNoGlow71";
        public const int Night = 71;
        const string RowOff = "design.n71." + Row + " off";
        const string Incomplete = "design.n71." + Row + ".numbers incomplete";
        const string P = "design.n71." + Row + ".numbers.";

        /// <summary>The steps' place on the gauntlet (m, rig space): their width (x), their height (y), how far above the body's top, the share of
        /// the body's length they span from its back, and each slot's gap share.</summary>
        public sealed class Numbers
        {
            public float segW, segH, lift, span, gapShare;
        }

        static readonly string[] Keys = { "segW", "segH", "lift", "span", "gapShare" };
        static GameConfig numbersCfg;
        static Numbers numbers;
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

        /// <summary>The numbers of this config (read once per config; null when one is missing, not a number or out of range).</summary>
        public static Numbers Of(GameConfig cfg)
        {
            if (cfg == null) return null;
            if (ReferenceEquals(cfg, numbersCfg)) return numbers;
            numbersCfg = cfg;
            numbers = null;
            var v = new float[Keys.Length];
            for (int i = 0; i < Keys.Length; i++)
            {
                if (!cfg.Has(P + Keys[i]) || !(cfg.Get(P + Keys[i]) is double d) || !(d >= 0)) return null;
                v[i] = (float)d;
            }
            if (v[0] <= 0 || v[1] <= 0 || v[3] <= 0 || v[3] > 1 || v[4] >= 1) return null;
            numbers = new Numbers { segW = v[0], segH = v[1], lift = v[2], span = v[3], gapShare = v[4] };
            return numbers;
        }

        /// <summary>Why the row is off (null: on): the row (NightRows.ViewOn, the reel's live override first), the off switches, its numbers.</summary>
        public static string OffReason(GameConfig cfg, string[] argv)
        {
            if (cfg == null) return "no config";
            if (!NightRows.ViewOn(cfg, Row)) return RowOff;
            if (NightRows.OffFrom(argv, "-skygrindOff") <= Night) return "-skygrindOff71 (or older)";
            if (NightRows.HasSwitch(argv, NoSwitch)) return NoSwitch;
            if (Of(cfg) == null) return Incomplete;
            return null;
        }

        /// <summary>Is the row on now? Read every frame: never cache it.</summary>
        public static bool On(GameConfig cfg) => OffReason(cfg, Args()) == null;

        /// <summary>Pure: the steps' centres along z (rig space) from the body's back (`backZ`) toward its front (`frontZ`), segment 0 the farthest
        /// back (the first quarter), over `span` of the length; and each step's length.</summary>
        public static float[] StepZs(float backZ, float frontZ, float span, float gapShare, int steps, out float len)
        {
            float run = (frontZ - backZ) * span, slot = run / steps;
            len = Mathf.Abs(slot) * (1f - gapShare);
            var z = new float[steps];
            for (int i = 0; i < steps; i++) z[i] = backZ + (i + 0.5f) * slot;
            return z;
        }
    }

    public sealed partial class Viewmodel
    {
        /// <summary>NIGHT71 L6 glow71 (Glow71.cs): the PULSE gauntlet's own steps on top of its body, built hidden with gunglow50's (a look marked
        /// row71: Glow50Frame draws it only on a frame the row reads on). Nothing when the gauntlet rig or the numbers are missing.</summary>
        void Glow71Build()
        {
            Glow71.Numbers n = Glow71.Of(cfg);
            if (n == null || !rigs.TryGetValue("gauntlet", out Rig r) || r.meshes.Count == 0) return;
            Bounds bb = r.meshes[0].bounds; // the body mesh, at the rig's origin (BuildGauntlet: MeshKit.Object at rig.t)
            // the back is the end away from the muzzle (the knuckles); the steps run from it toward the muzzle along the top
            float sign = Mathf.Sign(r.muzzle.z - bb.center.z);
            float backZ = sign > 0 ? bb.min.z : bb.max.z, frontZ = sign > 0 ? bb.max.z : bb.min.z;
            float[] zs = Glow71.StepZs(backZ, frontZ, n.span, n.gapShare, UnityTuning.G50Steps, out float len);
            float y = bb.max.y + n.lift + n.segH * 0.5f;
            var look = new Glow50Look { gun = "gauntlet", segs = new Transform[UnityTuning.G50Steps], row71 = true };
            look.mat = FxDraw.Additive(game.mats.neon, "glow71 gauntlet");
            look.mat.SetColor(BaseColor, Color.black);
            for (int i = 0; i < UnityTuning.G50Steps; i++)
            {
                var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(seg.GetComponent<Collider>());
                seg.name = "glow71 step " + (i + 1);
                seg.layer = Layer;
                seg.transform.SetParent(r.t, false);
                seg.transform.localPosition = new Vector3(bb.center.x, y, zs[i]);
                seg.transform.localScale = new Vector3(n.segW, n.segH, len);
                var sr = seg.GetComponent<Renderer>();
                sr.sharedMaterial = look.mat;
                sr.shadowCastingMode = ShadowCastingMode.Off;
                sr.receiveShadows = false;
                seg.SetActive(false);
                look.segs[i] = seg.transform;
            }
            g50Looks.Add(look);
            Gunglow50Probe.Guns += (Gunglow50Probe.Guns.Length > 0 ? " " : "") + "gauntlet(glow71)";
        }
    }
}
