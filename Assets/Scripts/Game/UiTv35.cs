// NIGHT35 V (design.n35.tv35; GG 2026-10-11: the UI "moving around a little bit ... bouncier ... glitching out like a TV", "I've explained this before"):
// the TV HUD, one self-installing component (as Bend34View) on the HUD's layer (Ui.Hud33Root); no edit inside UiHud33's build code (builder T owns
// the text path). Three things, all view only, no sim path, no random stream (fixed hashes of a frame counter):
//   JIGGLE   a damped spring (Ryan Juckett's form, as Coily ports it: x'' = -w^2 x - 2 z w x') on the whole HUD layer's position and scale: a hit taken
//            kicks it hardest, a weapon swap lightly. The reticle is held still (counter-offset): the aim point never moves.
//   BOUNCE   a HUD text whose string changes pops (scale 1 + Tv35BouncePop, a damped wobble back over Tv35BounceS); a text that changes every frame
//            (a timer) is left alone. Text only grows, so nothing drops under 16 px.
//   GLITCH   a short TV burst over the HUD: thin horizontal noise bars in two channels (cyan + deep blue, offset like a split signal), a bright
//            scanline band sweeping down and a sideways tear of the HUD layer: on a bend (bend34Start) and a level start (a new run); RED-ORANGE bars
//            instead on damage (rule 8: red only for danger). Flicker safety (look30's rules): at most one burst per Tv35GlitchGap s, under 25 % of the
//            screen, each Tv35GlitchS long.
// UIEffect is Editor-only in a Player (NIGHT21: its TMP variant shader is not in the build: magenta) and LitMotion needs the HudFx20 bridge assembly,
// so the motion is this file's own spring maths and the glitch plain uGUI Images. Off with HUD FLICKER off (look30's ACCESS switch) or REDUCED MOTION
// (the jiggle and the tear; the bars stay as a still flash), -skygrindOff35 or the row off. Numbers: UnityTuning.Look35.cs (Tv35*).
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>What the TV HUD did, for the look35 reel (all 0 with tv35 off).</summary>
    public static class UiTv35Probe
    {
        public static int Kicks; // spring kicks started (hits taken, swaps)
        public static int Glitches; // glitch bursts started (a bend, a level start, damage)
        public static int Bounces; // new-text bounces started
        public static float OffsetPx; // the HUD layer's offset from rest this frame (px at 1080p, length)
        public static float MinTextPx = -1; // the smallest live HUD text size this frame (px at 1080p, its font size x its scale; -1 unknown)
    }

    /// <summary>NIGHT35 V: the spring, the bounce wobble and the glitch's bars as pure rules (UiTv35 uses them; testable).</summary>
    public static class Tv35Math
    {
        /// <summary>One semi-implicit step of a damped spring toward 0 (angular frequency w rad/s, damping ratio z).</summary>
        public static void Spring(ref float x, ref float v, float w, float z, float dt)
        {
            v += (-w * w * x - 2f * z * w * v) * dt;
            x += v * dt;
        }

        /// <summary>The bounce's scale at age t (s) of `len`: 1 + pop x a damped cosine (2.5 wobbles), 1 after.</summary>
        public static float Bounce(float t, float len, float pop)
        {
            if (t < 0 || t >= len) return 1f;
            float u = t / len;
            return 1f + pop * Mathf.Cos(u * Mathf.PI * 5f) * (1f - u) * (1f - u);
        }

        /// <summary>A fixed hash to 0..1 (the CRT grain's).</summary>
        public static float Hash01(uint a, uint b) => Crt29Math.Noise(a, b, 35u);
    }

    public sealed class UiTv35 : MonoBehaviour
    {
        /// <summary>A reel that wants the HUD's motion on screen (look35) sets this; otherwise the HUD holds still while FeelReel.Running.</summary>
        public static bool ReelMotion;

        SurvivalGame game;
        bool rowOn;
        RectTransform layer, reticle;
        Vector2 layerBase, reticleBase;
        Vector3 layerScaleBase = Vector3.one;
        float px, py, pvx, pvy, ps, pvs; // the spring: offset (px), scale delta
        double glitchAge = double.MaxValue, lastGlitch = double.NegativeInfinity;
        Color glitchA, glitchB;
        uint frame;
        RectTransform overlay;
        readonly List<Image> bars = new List<Image>();
        Image sweep;
        readonly List<Watch> watched = new List<Watch>();
        float rescan;
        double now;

        sealed class Watch
        {
            public TMP_Text t;
            public string last;
            public Vector3 baseScale;
            public float age = float.MaxValue;
            public int changesInRow;
            public float quietUntil;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("UiTv35") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<UiTv35>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.cfg == null) { game = null; return false; }
            rowOn = N35Fixes.ViewRowOn(game.cfg, "tv35");
            if (rowOn)
            {
                game.events.On("hurt", _ => { Kick(UnityTuning.Tv35HitKickPx, UnityTuning.Tv35HitKickScale); Glitch(true); });
                game.events.On("weaponSwap", _ => Kick(UnityTuning.Tv35SwapKickPx, UnityTuning.Tv35SwapKickScale));
                game.events.On("bend34Start", _ => Glitch(false));
                game.OnNewRun += () => { glitchAge = double.MaxValue; lastGlitch = double.NegativeInfinity; pendingStart = true; };
            }
            return true;
        }

        bool pendingStart;

        bool Allowed()
        {
            if (game == null || game.settings == null) return true;
            return game.settings.store.Item(UnityTuning.HudFx30Setting) == null || game.settings.Bool(UnityTuning.HudFx30Setting);
        }

        bool Calm => game != null && game.cfg.movement.settings.reducedMotion;

        void Kick(float pxAmount, float scale)
        {
            if (!Allowed() || Calm) return;
            frame++;
            // a fixed walk of directions (no Random): up-left, right, down-right, left ...
            float a = Tv35Math.Hash01(frame, 7u) * Mathf.PI * 2f;
            float w = UnityTuning.Tv35SpringHz * Mathf.PI * 2f;
            pvx += Mathf.Cos(a) * pxAmount * w;
            pvy += Mathf.Sin(a) * pxAmount * w;
            pvs += scale * w;
            UiTv35Probe.Kicks++;
        }

        void Glitch(bool danger)
        {
            if (!Allowed()) return;
            if (now - lastGlitch < UnityTuning.Tv35GlitchGap) return;
            lastGlitch = now;
            glitchAge = 0;
            if (danger) { glitchA = Look.C(game.cfg, "colors.enemy"); glitchB = glitchA * 0.6f; }
            else { glitchA = Look.C(game.cfg, "colors.usable"); glitchB = Look.Hex(UnityTuning.Tv35BlueHex); }
            UiTv35Probe.Glitches++;
        }

        void Build()
        {
            layer = Ui.Hud33Root;
            layerBase = layer.anchoredPosition;
            layerScaleBase = layer.localScale;
            reticle = layer.Find("Reticle") as RectTransform;
            reticleBase = reticle != null ? reticle.anchoredPosition : Vector2.zero;
            // the glitch overlay: a full-screen rect over the HUD layer (a sibling right after it), raycasts off
            overlay = new GameObject("TV 35", typeof(RectTransform)).GetComponent<RectTransform>();
            overlay.SetParent(layer.parent, false);
            overlay.SetSiblingIndex(layer.GetSiblingIndex() + 1);
            overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            bars.Clear();
            for (int i = 0; i < UnityTuning.Tv35Bars; i++)
            {
                var img = new GameObject("Bar " + i, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                img.rectTransform.SetParent(overlay, false);
                img.raycastTarget = false;
                img.rectTransform.anchorMin = new Vector2(0, 0); img.rectTransform.anchorMax = new Vector2(1, 0);
                img.gameObject.SetActive(false);
                bars.Add(img);
            }
            sweep = new GameObject("Sweep", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            sweep.rectTransform.SetParent(overlay, false);
            sweep.raycastTarget = false;
            sweep.rectTransform.anchorMin = new Vector2(0, 0); sweep.rectTransform.anchorMax = new Vector2(1, 0);
            sweep.gameObject.SetActive(false);
            watched.Clear();
            rescan = 0;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (!rowOn) { UiTv35Probe.OffsetPx = 0; return; }
            if (Ui.Hud33Root == null) return;
            if (layer != Ui.Hud33Root) Build();
            float dt = Mathf.Min(FeelClock.UnscaledDelta, 1f / 30f); // NIGHT63 D1: 0 in a same-run held frame
            now += dt;
            const float k = 1f; // the canvas counts px at 1080 (Ui.cs: CanvasScaler by height, reference 1080): no screen scale here
            if (pendingStart && game.flow == Flow.Playing) { pendingStart = false; Glitch(false); } // the level start: the first playing frame of a run

            // --- jiggle ---
            float w = UnityTuning.Tv35SpringHz * Mathf.PI * 2f, z = UnityTuning.Tv35SpringDamping;
            int sub = Mathf.Max(1, Mathf.CeilToInt(dt * 240f));
            for (int i = 0; i < sub; i++)
            {
                Tv35Math.Spring(ref px, ref pvx, w, z, dt / sub);
                Tv35Math.Spring(ref py, ref pvy, w, z, dt / sub);
                Tv35Math.Spring(ref ps, ref pvs, w, z, dt / sub);
            }
            px = Mathf.Clamp(px, -UnityTuning.Tv35MaxPx, UnityTuning.Tv35MaxPx);
            py = Mathf.Clamp(py, -UnityTuning.Tv35MaxPx, UnityTuning.Tv35MaxPx);
            ps = Mathf.Clamp(ps, -UnityTuning.Tv35MaxScale, UnityTuning.Tv35MaxScale);
            float tear = 0f;
            bool glitching = glitchAge < UnityTuning.Tv35GlitchS;
            if (glitching && !Calm)
            {
                // the tear: the layer slides sideways on alternate frames (a fixed hash), fading over the burst
                float u = (float)(glitchAge / UnityTuning.Tv35GlitchS);
                tear = (Tv35Math.Hash01((uint)FeelClock.FrameCount, 3u) - 0.5f) * 2f * UnityTuning.Tv35TearPx * (1f - u);
            }
            // r2 (the lead): a reel's stills are rule-checked (hud33 / hud34): while a FeelReel runs the HUD holds still unless the reel asks for the motion
            // (ReelMotion: look35's hud-hit frames). The counters still count.
            bool still = FeelReel.Running && !ReelMotion;
            if (still) { px = py = pvx = pvy = ps = pvs = 0; tear = 0; }
            Vector2 off = new Vector2(px + tear, py);
            layer.anchoredPosition = layerBase + off * k;
            layer.localScale = layerScaleBase * (1f + ps);
            if (reticle != null) reticle.anchoredPosition = reticleBase - off * k / Mathf.Max(0.5f, 1f + ps); // the aim point stays put
            UiTv35Probe.OffsetPx = off.magnitude;

            // --- glitch bars + sweep ---
            if (glitching && !still)
            {
                float u = (float)(glitchAge / UnityTuning.Tv35GlitchS);
                float h = overlay.rect.height > 0 ? overlay.rect.height : 1080f;
                uint f = (uint)FeelClock.FrameCount;
                for (int i = 0; i < bars.Count; i++)
                {
                    Image b = bars[i];
                    bool on = Tv35Math.Hash01(f, (uint)(i * 13 + 1)) < UnityTuning.Tv35BarOdds;
                    b.gameObject.SetActive(on);
                    if (!on) continue;
                    float y = Tv35Math.Hash01(f, (uint)(i * 13 + 5)) * h;
                    float bh = Mathf.Lerp(UnityTuning.Tv35BarMinPx, UnityTuning.Tv35BarMaxPx, Tv35Math.Hash01(f, (uint)(i * 13 + 9))) * k;
                    float split = UnityTuning.Tv35SplitPx * k * (i % 2 == 0 ? 1f : -1f);
                    b.rectTransform.offsetMin = new Vector2(split, y);
                    b.rectTransform.offsetMax = new Vector2(split, y + bh);
                    Color c = i % 2 == 0 ? glitchA : glitchB;
                    c.a = UnityTuning.Tv35BarAlpha * (1f - u * 0.6f);
                    b.color = c;
                }
                sweep.gameObject.SetActive(true);
                float sy = Calm ? h * 0.66f : h * (1f - u);
                float sh = UnityTuning.Tv35SweepPx * k;
                sweep.rectTransform.offsetMin = new Vector2(0, sy - sh * 0.5f);
                sweep.rectTransform.offsetMax = new Vector2(0, sy + sh * 0.5f);
                Color sc = Color.Lerp(glitchA, Color.white, 0.5f);
                sc.a = UnityTuning.Tv35SweepAlpha * (1f - u);
                sweep.color = sc;
                glitchAge += dt;
            }
            else if (sweep != null && sweep.gameObject.activeSelf)
            {
                sweep.gameObject.SetActive(false);
                foreach (Image b in bars) b.gameObject.SetActive(false);
            }
            if (glitching && still) glitchAge += dt; // a reel's still: the burst runs out unseen

            // --- bounce on new text ---
            rescan -= dt;
            if (rescan <= 0)
            {
                rescan = UnityTuning.Tv35RescanS;
                var live = layer.GetComponentsInChildren<TMP_Text>(false);
                var keep = new List<Watch>(live.Length);
                foreach (TMP_Text t in live)
                {
                    Watch found = watched.Find(x => x.t == t);
                    keep.Add(found ?? new Watch { t = t, last = t.text, baseScale = t.rectTransform.localScale });
                }
                foreach (Watch old in watched) if (!keep.Contains(old) && old.t != null && old.age < UnityTuning.Tv35BounceS) old.t.rectTransform.localScale = old.baseScale;
                watched.Clear();
                watched.AddRange(keep);
            }
            float minPx = float.MaxValue;
            bool allowed = Allowed() && !Calm;
            foreach (Watch wt in watched)
            {
                if (wt.t == null) continue;
                string s = wt.t.text;
                if (!ReferenceEquals(s, wt.last) && s != wt.last)
                {
                    wt.last = s;
                    wt.changesInRow++;
                    if (wt.changesInRow >= 3) wt.quietUntil = (float)now + 2f; // a timer: left alone
                    if (allowed && now >= wt.quietUntil && !string.IsNullOrEmpty(s) && wt.age >= UnityTuning.Tv35BounceGap) { wt.age = 0; UiTv35Probe.Bounces++; }
                }
                else wt.changesInRow = 0;
                if (wt.age < UnityTuning.Tv35BounceS)
                {
                    wt.age += dt;
                    float sc = still ? 1f : Tv35Math.Bounce(wt.age, UnityTuning.Tv35BounceS, UnityTuning.Tv35BouncePop);
                    wt.t.rectTransform.localScale = wt.baseScale * Mathf.Max(1f, sc); // only ever grows: text stays >= its size (ends exactly at its base)
                }
                if (wt.t.isActiveAndEnabled && wt.t.color.a > 0.3f && !string.IsNullOrEmpty(s))
                    minPx = Mathf.Min(minPx, wt.t.fontSize * wt.t.rectTransform.localScale.y);
            }
            UiTv35Probe.MinTextPx = minPx < float.MaxValue ? minPx : -1;
        }
    }
}
