// NIGHT47 builder F (design.n47.lava47, a view row; NIGHT47.md G11; GG 2026-10-18: "the lava lighting is inconsistent and messy"). The diagnosis
// (Research/2026-10-18-wave47/F-cards.md 6b, read in code): the lava surface lights nothing; two ROAMING real lights did it: LightKit29's last slot,
// which every 0.25 s jumped to the lava fixture nearest the camera in the current room with no fade, and Look43View's lava light, which every frame
// teleported to the pool under the feet with its range rescaled; four rhythms (0.8 Hz +-15 % surface, 0.35 Hz +-25 % underglow and light, a steady
// slot) on one source. The rule with the row on, one sentence: A LAVA FIELD'S LIGHT IS FIXED, MADE ONCE, FADED BY DISTANCE, AND BREATHES WITH ITS
// SURFACE:
//   - one real point light per lava light the plan placed (LightPlan33: one per 100 m2 of pit, >= 1), created once at the level's build at the
//     plan's position (1.2 m over the surface), its range and colour the plan's, never moved, never created later;
//   - not filtered by the room (lava seen through a door keeps its light), faded by camera distance (Lava47FadeNear -> Lava47FadeFar m) and kept
//     to the nearest Lava47MaxLit (the rest fade out, never cut); every intensity change is eased (Lava47EaseS), so nothing pops;
//   - all of them, the surface (LavaView30) and the underglow (Look43View) breathe on ONE curve (Lava47.Breath: 0.35 Hz, +-10 %, in phase);
//   - LightKit29's lava slot no longer takes lava fixtures (it keeps an ordinary fixture) and Look43View's roaming lava light stays dark.
// Off (the row off, -skygrindOff47 or any older switch): NIGHT46's lights bit for bit. Numbers: UnityTuning.F47.cs (Lava47*).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>lava47's switch and its one breath (LightKit29, LavaView30, Look43View read it).</summary>
    public static class Lava47
    {
        /// <summary>The a847 reel's off / on passes (null: design.n47.lava47).</summary>
        public static bool? Force;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "lava47"); }
            return on;
        }

        /// <summary>The one breath at time t (1 with reduced motion): the surface, the underglow and the fixed lights share it.</summary>
        public static float Breath(double t, bool reducedMotion) =>
            reducedMotion ? 1f : 1f + UnityTuning.Lava47BreathDepth * (float)Math.Sin(2 * Math.PI * UnityTuning.Lava47BreathHz * t);

        /// <summary>The distance fade: 1 inside near, 0 past far, smooth between.</summary>
        public static float Fade(float d) => 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(UnityTuning.Lava47FadeNear, UnityTuning.Lava47FadeFar, d));

        /// <summary>For the reels: the fixed lava lights built this level, lit now (intensity > 0), and lights created after the build (want 0).</summary>
        public static int Built, LitNow, CreatedLate;
    }

    public sealed partial class LightKit29
    {
        bool lava47;
        Light[] lava47Lights;
        float[] lava47Base, lava47Dist;
        int lava47Count;

        /// <summary>From the end of Build: one fixed light per lava fixture (row on).</summary>
        void BuildLava47(SurvivalGame g)
        {
            lava47 = Lava47.On(g.cfg);
            if (!lava47) return;
            if (lava47Lights != null) Lava47.CreatedLate++; // a second build in one level (never, by design: the reel's line reads it)
            var list = new List<Fixture>();
            foreach (Fixture f in fixtures) if (f.kind == "lava") list.Add(f);
            lava47Count = list.Count;
            lava47Lights = new Light[lava47Count];
            lava47Base = new float[lava47Count];
            lava47Dist = new float[lava47Count];
            for (int i = 0; i < lava47Count; i++)
            {
                Fixture f = list[i];
                var lg = new GameObject("Lava light 47 " + i);
                lg.transform.SetParent(transform, false);
                lg.transform.position = f.at;
                var l = lg.AddComponent<Light>();
                l.type = LightType.Point;
                l.shadows = LightShadows.None;
                l.color = f.color;
                l.range = f.range;
                l.intensity = 0f; // eased in by StepLava47 (no pop on the level's first frame either)
                l.renderMode = LightRenderMode.Auto;
                lava47Lights[i] = l;
                lava47Base[i] = f.intensity * lightScale;
            }
            Lava47.Built = lava47Count;
        }

        // ---- the room's roaming slots under lava47 (the lead's n47-r0c meter: with the lava lights fixed, field 0's floor light still jumped
        // 82 % at a walk: the other slots' 0.25 s re-pick moved a light to another fixture in one frame). With the row on a slot never jumps: it
        // fades out where it is (Lava47SlotFadeS), moves to its new fixture, fades in. ----
        int[] fix47, want47;
        float[] wantI47, curI47, fade47;

        void Want47(int li, int fixture, float intensity)
        {
            int n = lights.Length;
            if (fix47 == null || fix47.Length != n)
            {
                fix47 = new int[n]; want47 = new int[n]; wantI47 = new float[n]; curI47 = new float[n]; fade47 = new float[n];
                for (int i = 0; i < n; i++) fix47[i] = want47[i] = -1;
            }
            want47[li] = fixture;
            wantI47[li] = intensity;
        }

        /// <summary>Every frame with lava47 on: each slot's fade toward its wanted fixture (a move happens only at zero).</summary>
        void StepSlots47(float dt)
        {
            if (fix47 == null) return;
            float rate = dt / (l54 ? Mathf.Max(0.01f, Light54.F(game.cfg, "slotFadeS", 0.45)) : UnityTuning.Lava47SlotFadeS); // NIGHT54 light54: the cross-fade's time
            if (baseI33 == null || baseI33.Length != lights.Length) baseI33 = new float[lights.Length];
            for (int li = 0; li < lights.Length; li++)
            {
                Light l = lights[li];
                if (fix47[li] != want47[li])
                {
                    if (l54) Twin54(li, l); // NIGHT54 light54 (LightKit29.Light54.cs): the old light's twin fades out where it stands; the slot moves now
                    fade47[li] = l54 ? 0f : Mathf.Max(0f, fade47[li] - rate);
                    if (fade47[li] <= 0f)
                    {
                        fix47[li] = want47[li];
                        curI47[li] = wantI47[li];
                        if (fix47[li] >= 0 && fix47[li] < fixtures.Count)
                        {
                            Fixture bf = fixtures[fix47[li]];
                            l.transform.position = bf.at;
                            if (lavaSlot33)
                            {
                                LightType want = bf.down ? LightType.Spot : LightType.Point;
                                if (l.type != want) l.type = want;
                                if (bf.down) { l.transform.rotation = Quaternion.Euler(90f, 0f, 0f); l.spotAngle = UnityTuning.Look33PanelSpotAngle; l.innerSpotAngle = UnityTuning.Look33PanelSpotAngle * 0.5f; }
                            }
                            l.color = bf.color;
                            l.range = bf.range;
                        }
                    }
                }
                else
                {
                    curI47[li] = Mathf.MoveTowards(curI47[li], wantI47[li], rate * Mathf.Max(1e-4f, wantI47[li]));
                    fade47[li] = Mathf.Min(1f, fade47[li] + rate);
                }
                float i47 = fix47[li] >= 0 ? curI47[li] * (l54 ? Mathf.SmoothStep(0f, 1f, fade47[li]) : fade47[li]) : 0f; // NIGHT54 light54: an eased fade-in
                l.intensity = i47;
                baseI33[li] = i47; // Vfx33's brazier flicker multiplies this
            }
        }

        /// <summary>Every frame (Update, before the slot re-pick's clock): the fade, the cap, the breath, eased.</summary>
        void StepLava47()
        {
            if (game == null) return;
            bool rowOn = Lava47.On(game.cfg); // the a847 reel flips Lava47.Force at runtime: the lights are made on the first on, darkened on an off
            if (rowOn != lava47)
            {
                if (rowOn && lava47Lights == null) BuildLava47(game);
                lava47 = rowOn;
                if (!rowOn && lava47Lights != null) foreach (Light l in lava47Lights) if (l != null) { l.intensity = 0f; l.enabled = false; }
            }
            if (lava47) StepSlots47(FeelClock.UnscaledDelta); // NIGHT63 D1: 0 in a same-run held frame
            if (!lava47 || lava47Lights == null || game.cam == null) return;
            Vector3 p = game.cam.transform.position;
            for (int i = 0; i < lava47Count; i++) lava47Dist[i] = lava47Lights[i] != null ? Vector3.Distance(p, lava47Lights[i].transform.position) : float.MaxValue;
            bool reduced = game.settings != null && game.settings.View.reducedMotion;
            float breath = Lava47.Breath(Time.time, reduced);
            float step = FeelClock.UnscaledDelta / UnityTuning.Lava47EaseS;
            int lit = 0;
            for (int i = 0; i < lava47Count; i++)
            {
                Light l = lava47Lights[i];
                if (l == null) continue;
                int closer = 0;
                for (int j = 0; j < lava47Count; j++) if (lava47Dist[j] < lava47Dist[i] || (lava47Dist[j] == lava47Dist[i] && j < i)) closer++;
                float want = closer < UnityTuning.Lava47MaxLit ? lava47Base[i] * Lava47.Fade(lava47Dist[i]) * breath : 0f;
                float maxStep = lava47Base[i] * step;
                l.intensity = Mathf.MoveTowards(l.intensity, want, Mathf.Max(maxStep, 1e-4f));
                bool en = l.intensity > 1e-4f;
                if (l.enabled != en) l.enabled = en;
                if (en) lit++;
            }
            Lava47.LitNow = lit;
        }
    }
}
