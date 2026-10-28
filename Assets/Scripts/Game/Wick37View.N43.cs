// NIGHT43 builder B1 (wick43; NIGHT43.md card B1 G15 / G16 / G10, DECIDE43 D-7, research/bosses-web.md C.11): what wick43 adds to THE WICK's
// view, drawn only while the sim's wick43 runs (SurvivalSim.Wick43On; --off43 / -skygrindOff43: nothing here draws, Wick37View is NIGHT42's):
//  - THE REVEAL'S FRAME: two letterbox bars slide in over the reveal's window (SurvivalSim.Wick43Reveal01: 2.5 s the first time, 1 s on a retry,
//    thinner) and slide away before it ends, so the frame ends on the player's own view (the look ease itself is the sim's: the player keeps
//    control). THE STACK's lights dip at the reveal's start and come back one by one from the bottom up (the first reveal only).
//  - THE STACK's lights in a warm furnace white (NIGHT37's violet-white read as G10's pink).
//  - THE LEG BEAT: the cage shudders while Wick37.recoil43 runs (the sim kicks it up and back; this adds the shake).
//  - LEG 3's CAP FLARE: the burning arc drawn on the catwalk (+-capFlareHalfDeg round the player's angle) instead of the walls' half ring and
//    its curtain (the sim burns the catwalk there in leg 3).
// Wick37View.Render calls Render43 once a frame (one line). Unity objects are made on first use (L-035). Numbers: UnityTuning.Wick43.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        /// <summary>For the reels: the letterbox's bar share drawn this frame (0: none), the reveal's progress, the cap flare drawn.</summary>
        public float Letterbox43;
        public double Reveal43 = -1;
        public bool CapFlare43;

        Canvas lb43;
        RectTransform lbTop43, lbBot43;
        readonly List<Light> stackLights43 = new List<Light>();
        readonly List<float> stackLightBase43 = new List<float>();
        Wick37Stack lightsFor43;
        Renderer capFlareR43;
        Transform capFlareT43;
        int revealSeen43 = -1;
        double hpSeen43 = -1;
        float flinch43;
        bool openPrev43;
        /// <summary>For the reels: flinches drawn (a hit landed while OPEN).</summary>
        public int Flinches43;

        /// <summary>Is the sim's wick43 running on this level now?</summary>
        public static bool On43(SurvivalGame g) => g != null && g.sim != null && g.sim.Wick43On;

        void Render43(SurvivalSim sim, Wick37 w, Vec3 o, bool fight)
        {
            bool on = sim.Wick43On;
            Reveal43 = on ? sim.Wick43Reveal01 : -1;
            Letterbox(on ? (float)Reveal43 : -1f, sim.wick43RevealFirst);
            StackLights(sim, on);
            CapFlare43 = false;
            if (!on || !fight || w == null || w.brain == null)
            {
                if (capFlareT43 != null) capFlareT43.gameObject.SetActive(false);
                return;
            }
            // the leg beat's shudder
            if (w.recoil43 > 0 && w.p43 != null)
            {
                float left = w.recoil43 / Mathf.Max(1f, (float)(w.p43.recoilSeconds * 120.0));
                float a = UnityTuning.Wick43RecoilShakeM * left;
                float ph = t * UnityTuning.Wick43RecoilShakeHz * Mathf.PI * 2;
                bodyRoot.position += new Vector3(Mathf.Sin(ph) * a, Mathf.Sin(ph * 1.37f) * a * 0.6f, Mathf.Cos(ph * 0.83f) * a);
            }
            // round 2 (G16 "boss hits smoother and crisper"): a hit that lands while it is OPEN (the window or a cracked plate: full damage) makes
            // the cage FLINCH: kicked away from the camera, falling back over Wick43FlinchS (a chip hit does not)
            double hp = w.body.hp;
            if (hpSeen43 >= 0 && hp < hpSeen43 - 0.5 && (w.Open43 || w.brain.Open || openPrev43)) { flinch43 = UnityTuning.Wick43FlinchS; Flinches43++; } // a pierce spends its crack: open on the frame before counts
            openPrev43 = w.Open43;
            hpSeen43 = hp;
            if (flinch43 > 0)
            {
                flinch43 = Mathf.Max(0, flinch43 - Time.deltaTime);
                float k = flinch43 / UnityTuning.Wick43FlinchS;
                Camera cam = game.cam != null ? game.cam : Camera.main;
                Vector3 away = cam != null ? bodyRoot.position - cam.transform.position : Vector3.forward;
                away.y = 0;
                if (away.sqrMagnitude > 1e-4f) away.Normalize();
                bodyRoot.position += away * (UnityTuning.Wick43FlinchM * k * k) + Vector3.up * (UnityTuning.Wick43FlinchUpM * k);
                cageT.localRotation *= Quaternion.Euler(UnityTuning.Wick43FlinchTiltDeg * k * Mathf.Sin(t * 47f), 0, UnityTuning.Wick43FlinchTiltDeg * k);
            }
            // leg 3's cap flare: the arc on the catwalk replaces the walls' half ring and its curtain
            Boss37Brain b = w.brain;
            bool capTell = b.leg == 3 && w.AttackId == Wick37.Flare && b.state == Boss37State.Tell;
            bool capLive = b.leg == 3 && w.flareLive;
            if (capTell || capLive)
            {
                if (flareT != null) flareT.gameObject.SetActive(false);
                if (curtainT != null) curtainT.gameObject.SetActive(false);
                if (capFlareT43 == null) capFlareR43 = Glow(fxRoot, "cap flare (wick43)", CapArc43(w.p43 != null ? w.p43.capFlareHalfDeg : 55), out capFlareT43);
                capFlareT43.gameObject.SetActive(true);
                capFlareT43.position = U(o) + new Vector3(0, (float)w.stack.p.capY + UnityTuning.Wick43CapFlareLift, 0);
                capFlareT43.localRotation = Quaternion.Euler(0, (float)(w.flareAngle * 180 / System.Math.PI), 0);
                Tint(capFlareR43, Look.Hex(UnityTuning.Tell36RedHex, capLive ? UnityTuning.Wick43CapFlareGain * 1.5f : UnityTuning.Wick43CapFlareGain * Mathf.Max(UnityTuning.Wick37FlareTellMin, FlashGain(b.Progress))));
                CapFlare43 = true;
            }
            else if (capFlareT43 != null) capFlareT43.gameObject.SetActive(false);
        }

        /// <summary>The cap flare's arc: a flat ring sector on the catwalk, +-halfDeg round local +x (Wick37View's flare convention).</summary>
        Mesh CapArc43(double halfDeg)
        {
            var k = new MeshKit();
            int n = UnityTuning.Wick43CapFlareSegments;
            float span = (float)(2 * halfDeg) * Mathf.Deg2Rad, r0 = UnityTuning.Wick43CapFlareInner, r1 = UnityTuning.Wick43CapFlareOuter;
            for (int i = 0; i < n; i++)
            {
                float a0 = -span / 2 + span * i / n, a1 = -span / 2 + span * (i + 1) / n;
                Vector3 i0 = new Vector3(Mathf.Cos(a0) * r0, 0, Mathf.Sin(a0) * r0), i1 = new Vector3(Mathf.Cos(a1) * r0, 0, Mathf.Sin(a1) * r0);
                Vector3 o0 = new Vector3(Mathf.Cos(a0) * r1, 0, Mathf.Sin(a0) * r1), o1 = new Vector3(Mathf.Cos(a1) * r1, 0, Mathf.Sin(a1) * r1);
                k.Quad(i0, o0, o1, i1, Vector3.up, Color.white);
                k.Quad(i1, o1, o0, i0, Vector3.down, Color.white);
            }
            return k.Build("wick43 cap flare");
        }

        void Letterbox(float reveal, bool first)
        {
            float share = 0f;
            if (reveal >= 0f)
            {
                float full = first ? UnityTuning.Wick43LetterboxFirst : UnityTuning.Wick43LetterboxRetry;
                float i = Mathf.Clamp01(reveal / UnityTuning.Wick43LetterboxIn), u = Mathf.Clamp01((1f - reveal) / UnityTuning.Wick43LetterboxOut);
                float e = Mathf.Min(1f - (1f - i) * (1f - i), u * u); // in: ease-out; away: ease-in
                share = full * e;
            }
            Letterbox43 = share;
            if (share <= 0f && lb43 == null) return;
            if (lb43 == null) BuildLetterbox();
            lb43.enabled = share > 0f;
            if (share <= 0f) return;
            lbTop43.anchorMin = new Vector2(0, 1f - share);
            lbTop43.anchorMax = new Vector2(1, 1);
            lbBot43.anchorMin = new Vector2(0, 0);
            lbBot43.anchorMax = new Vector2(1, share);
        }

        void BuildLetterbox()
        {
            var go = new GameObject("wick43 letterbox", typeof(Canvas), typeof(CanvasScaler));
            lb43 = go.GetComponent<Canvas>();
            lb43.renderMode = RenderMode.ScreenSpaceOverlay;
            lb43.sortingOrder = UnityTuning.Wick43LetterboxSort;
            lbTop43 = Bar43(go.transform, "top");
            lbBot43 = Bar43(go.transform, "bottom");
            lb43.enabled = false;
        }

        static RectTransform Bar43(Transform parent, string name)
        {
            var g = new GameObject(name, typeof(RectTransform), typeof(Image));
            g.transform.SetParent(parent, false);
            var img = g.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, UnityTuning.Wick43LetterboxAlpha);
            img.raycastTarget = false;
            var rt = (RectTransform)g.transform;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>THE STACK's lights: warm with wick43; on the first reveal they dip and come back one by one from the bottom up.</summary>
        void StackLights(SurvivalSim sim, bool on)
        {
            if (lightsFor43 != sim.wick37Stack || stackLights43.Count == 0)
            {
                stackLights43.Clear();
                stackLightBase43.Clear();
                lightsFor43 = sim.wick37Stack;
                foreach (Light l in stackRoot.GetComponentsInChildren<Light>(true))
                    if (l.gameObject.name == "stack light") stackLights43.Add(l);
                stackLights43.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
                foreach (Light l in stackLights43) stackLightBase43.Add(l.intensity);
            }
            int n = stackLights43.Count;
            bool reveal = on && sim.wick43RevealFirst && Reveal43 >= 0;
            if (reveal && revealSeen43 != sim.wick43RevealAt) revealSeen43 = sim.wick43RevealAt;
            for (int i = 0; i < n; i++)
            {
                Light l = stackLights43[i];
                if (l == null) { stackLights43.Clear(); return; } // a rebuilt stack's old lights (Destroy lands at the frame's end): collect again next frame
                l.color = Look.Hex(on ? UnityTuning.Wick43StackLightHex : UnityTuning.Stack37LightHex);
                float k = 1f;
                if (reveal)
                {
                    float at = UnityTuning.Wick43LightsFrom + UnityTuning.Wick43LightsSpan * (n > 1 ? i / (float)(n - 1) : 0f);
                    float up = Mathf.Clamp01(((float)Reveal43 - (at - UnityTuning.Wick43LightsRamp)) / UnityTuning.Wick43LightsRamp);
                    k = Mathf.Lerp(UnityTuning.Wick43LightsDip, 1f, up * up * (3f - 2f * up));
                }
                l.intensity = stackLightBase43[i] * k * Stack54Gain(); // NIGHT54 light54: x wickStackGain (1 with the row off)
            }
        }
    }
}
