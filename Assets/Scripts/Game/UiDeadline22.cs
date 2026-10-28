// NIGHT22 U3-36.5 DEADLINE bar (builder S): the fuse of a DEADLINE twist round on the HUD (Core/Deadline21.cs; design.n21.deadline).
// RESEARCH R5 (Mullet MadJack): a kill-refilled fuse is ONE number that doubles as the health bar, the gain is its own element beside
// it, and the low-time pattern is a pulse. So: a slanted INK track with a DANGER (red-orange) fill that shortens with the fuse and the
// seconds left as the number on it (42 px, 18 px being M10's floor), a pulse in the last Deadline22PulseSeconds (our tuning: the fuse
// is 18 s), a separate "+N" pop at the bar's right end on every kill that refills it (what the kill paid: 2.5 / 5 slide / 6 slam, the
// event's `paid`), and a burn flash when the fuse runs out (through the flash gate and FLASH INTENSITY). DANGER is the role for a
// danger timer (CLAUDE.md rule 8); the pops are CYAN (the player's reward). REDUCED MOTION keeps the bar and the pops and drops the
// pulse's motion. A picture of the sim: it reads sim.deadline21 each frame. Wiring: Ui's BuildN22Hud calls BuildDeadline22Bar(),
// RenderN22Hud calls RenderDeadline22Bar(frameDt). UNCOMPILED when written (the builder has no Editor).
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform dl22Root;
        CanvasGroup dl22Group;
        UiShape dl22Track, dl22Fill, dl22Flash;
        TextMeshProUGUI dl22Number, dl22Label;
        TextMeshProUGUI[] dl22Pop;
        RectTransform[] dl22PopRt;
        double[] dl22PopAge;
        int dl22PopNext, dl22Shown10 = -1;
        bool dl22On, dl22Pulsing;
        double dl22InAge, dl22FlashAge = 99, dl22Phase;
        float dl22ShownFill = -1, dl22FlashOk;
        float dl22ShownScale = -1;
        Color dl22ShownFillColor;
        Vector2 dl22Home;

        /// <summary>The bar now (the Player reel reads these).</summary>
        public bool Deadline22Built => dl22Root != null;
        public bool Deadline22Visible => dl22On && dl22Group != null && dl22Group.alpha > 0;
        public float Deadline22NumberPx => dl22Number != null ? dl22Number.fontSize : 0;
        public string Deadline22NumberText => dl22Number != null ? dl22Number.text : null;
        public bool Deadline22Pulsing => dl22Pulsing;
        public Color Deadline22FillColor => dl22Fill != null ? dl22Fill.color : default;
        public int Deadline22PopsStarted { get; private set; }
        public int Deadline22Flashes { get; private set; }
        /// <summary>The "+N" pops that are on screen now.</summary>
        public int Deadline22PopsLive
        {
            get
            {
                int n = 0;
                if (dl22PopAge != null) foreach (double a in dl22PopAge) if (a < UnityTuning.Deadline22PopLife) n++;
                return n;
            }
        }

        void BuildDeadline22Bar()
        {
            float w = UnityTuning.Deadline22Width, h = UnityTuning.Deadline22Height, rim = UnityTuning.Deadline22Inset;
            var mid = new Vector2(0.5f, 0.5f);
            var topMid = new Vector2(0.5f, 1f);
            dl22Home = new Vector2(0, -UnityTuning.Deadline22Top);
            dl22Root = Hud17Rect(values, "Deadline bar", topMid, topMid, dl22Home, new Vector2(w, h));
            dl22Group = dl22Root.gameObject.AddComponent<CanvasGroup>();
            dl22Group.alpha = 0;
            dl22Group.interactable = dl22Group.blocksRaycasts = false;
            dl22Track = Hud17Shape(dl22Root, "Track", UiTheme.InkSolid, mid, mid, Vector2.zero, new Vector2(w, h));
            dl22Track.SetCuts(UnityTuning.ThemeCut, UnityTuning.ThemeCut);
            var left = new Vector2(0, 0.5f);
            dl22Fill = Hud17Shape(dl22Root, "Fill", UiTheme.Danger, left, left, new Vector2(rim, 0), new Vector2(w - 2 * rim, h - 2 * rim));
            dl22Fill.SetCuts(0, 0);
            dl22Fill.Fill = 1f;
            dl22Flash = Hud17Shape(dl22Root, "Burn flash", UiTheme.A(UiTheme.White, 0), mid, mid, Vector2.zero, new Vector2(w, h));
            dl22Flash.SetCuts(UnityTuning.ThemeCut, UnityTuning.ThemeCut);
            dl22Flash.enabled = false;
            // the label inside the bar's left end (18 px, M10's floor), the number on its middle (42 px)
            dl22Label = Hud17Text(dl22Root, "Label", "DEADLINE", UiTheme.Px(UiTheme.Size.S18), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Left, left, left, new Vector2(rim + 14, 0), new Vector2(180, 30), false);
            Hud17Stroke(dl22Label);
            dl22Number = Hud17Text(dl22Root, "Number", "", UiTheme.Px(UiTheme.Size.S42), UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, mid, mid, Vector2.zero, new Vector2(w * 0.5f, h), true);
            Hud17Stroke(dl22Number);
            // the "+N" pops at the right end, outside the bar (CYAN: the player's reward)
            int n = UnityTuning.Deadline22Pops;
            dl22Pop = new TextMeshProUGUI[n];
            dl22PopRt = new RectTransform[n];
            dl22PopAge = new double[n];
            var right = new Vector2(1, 0.5f);
            for (int i = 0; i < n; i++)
            {
                dl22Pop[i] = Hud17Text(dl22Root, "Pop " + i, "", UiTheme.Px(UiTheme.Size.S32), UiTheme.CyanHot, UiTheme.Display, TextAlignmentOptions.Left, right, left, new Vector2(18, 0), new Vector2(160, 44), true);
                Hud17Stroke(dl22Pop[i]);
                dl22PopRt[i] = dl22Pop[i].rectTransform;
                dl22PopAge[i] = UnityTuning.Deadline22PopLife;
                dl22Pop[i].gameObject.SetActive(false);
            }
            dl22Root.gameObject.SetActive(false);
            game.events.On<DeadlineEvent>(Deadline21.Tick, e => { if (e.refill && e.paid > 0) Deadline22Pop(e.paid); });
            game.events.On<DeadlineEvent>(Deadline21.Burn, e => Deadline22Burn());
        }

        /// <summary>A kill refilled the fuse: its own "+N" pop (the next free one; the oldest when all are live).</summary>
        void Deadline22Pop(double paid)
        {
            if (dl22Pop == null) return;
            int i = dl22PopNext;
            dl22PopNext = (dl22PopNext + 1) % dl22Pop.Length;
            dl22Pop[i].text = "+" + paid.ToString("0.#", CultureInfo.InvariantCulture);
            dl22PopAge[i] = 0;
            dl22Pop[i].gameObject.SetActive(true);
            Deadline22PopsStarted++;
        }

        /// <summary>The fuse burned out: one flash on the bar, if the flash gate and FLASH INTENSITY allow (the gate counts it).</summary>
        void Deadline22Burn()
        {
            if (dl22Flash == null) return;
            float intensity = Mathf.Clamp01((float)game.settings.Num("flashIntensity"));
            if (intensity <= 0 || !game.flashGate.TryFlash(SurvivalGame.RealNow)) return;
            dl22FlashOk = intensity;
            dl22FlashAge = 0;
            Deadline22Flashes++;
        }

        void RenderDeadline22Bar(double frameDt)
        {
            if (dl22Root == null) return;
            Deadline21 d = game.sim != null ? game.sim.deadline21 : null;
            bool playing = game.flow == Flow.Playing || game.flow == Flow.Paused;
            bool live = playing && d != null && d.on && d.Running;
            if (!live)
            {
                if (dl22On)
                {
                    dl22On = false;
                    dl22Root.gameObject.SetActive(false);
                    for (int i = 0; i < dl22Pop.Length; i++) dl22PopAge[i] = UnityTuning.Deadline22PopLife;
                    dl22FlashAge = 99;
                    dl22Shown10 = -1;
                    dl22ShownFill = -1;
                }
                return;
            }
            if (!dl22On)
            {
                dl22On = true;
                dl22InAge = 0;
                dl22Root.gameObject.SetActive(true);
            }
            bool still = game.settings.View.reducedMotion;
            // the entrance: alpha and a drop from above over Deadline22InTime (nothing shows whole on its first frame)
            if (game.flow == Flow.Playing && dl22InAge < UnityTuning.Deadline22InTime) dl22InAge += frameDt;
            float e = UiTween.OutCubic((float)(dl22InAge / UnityTuning.Deadline22InTime));
            dl22Group.alpha = e;
            dl22Root.anchoredPosition = dl22Home + new Vector2(0, still ? 0 : (1 - e) * UnityTuning.Deadline22InDrop);

            // the number is the bar: seconds left (one decimal, written only when it changes) and the fill's length
            double left = d.LeftSeconds;
            int tenths = (int)(left * 10);
            if (tenths != dl22Shown10)
            {
                dl22Shown10 = tenths;
                dl22Number.SetText("{0:1}", (float)left);
            }
            float share = (float)d.Share;
            if (share != dl22ShownFill)
            {
                dl22ShownFill = share;
                dl22Fill.Fill = share;
            }

            // the pulse in the last seconds (DANGER toward white, the number swelling); steady DANGER otherwise
            bool pulse = left <= UnityTuning.Deadline22PulseSeconds;
            dl22Pulsing = pulse;
            float k = 0;
            if (pulse)
            {
                if (game.flow == Flow.Playing) dl22Phase = (dl22Phase + frameDt * UnityTuning.Deadline22PulseHz) % 1.0;
                k = still ? 1f : 0.5f + 0.5f * Mathf.Sin((float)dl22Phase * Mathf.PI * 2f);
            }
            Color fc = Color.Lerp(UiTheme.Danger, UiTheme.White, k * UnityTuning.Deadline22PulseMix);
            if (fc != dl22ShownFillColor)
            {
                dl22ShownFillColor = fc;
                dl22Fill.color = fc;
            }
            float sc = pulse && !still ? 1f + UnityTuning.Deadline22PulseScale * k : 1f;
            if (sc != dl22ShownScale)
            {
                dl22ShownScale = sc;
                dl22Number.rectTransform.localScale = new Vector3(sc, sc, 1);
            }

            // the "+N" pops: each rises and fades on its own clock
            for (int i = 0; i < dl22Pop.Length; i++)
            {
                if (dl22PopAge[i] >= UnityTuning.Deadline22PopLife) continue;
                dl22PopAge[i] += frameDt;
                if (dl22PopAge[i] >= UnityTuning.Deadline22PopLife)
                {
                    dl22Pop[i].gameObject.SetActive(false);
                    continue;
                }
                float u = (float)(dl22PopAge[i] / UnityTuning.Deadline22PopLife);
                dl22PopRt[i].anchoredPosition = new Vector2(18, UnityTuning.Deadline22PopRise * UiTween.OutCubic(u));
                // NIGHT23: a short fade-in (the n23 reel's M9 flagged 13 pop-ins: the pop began at full alpha)
                float fadeIn = Mathf.Clamp01((float)(dl22PopAge[i] / UnityTuning.Deadline22PopIn));
                dl22Pop[i].color = UiTheme.A(UiTheme.CyanHot, (1f - u * u) * fadeIn);
            }

            // the burn flash: one white-hot sheet over the bar, fading
            bool flash = dl22FlashAge < UnityTuning.Deadline22FlashTime;
            if (flash)
            {
                dl22FlashAge += frameDt;
                flash = dl22FlashAge < UnityTuning.Deadline22FlashTime;
            }
            if (dl22Flash.enabled != flash) dl22Flash.enabled = flash;
            if (flash) dl22Flash.color = UiTheme.A(UiTheme.White, UnityTuning.Deadline22FlashAlpha * dl22FlashOk * (1f - (float)(dl22FlashAge / UnityTuning.Deadline22FlashTime)));
        }
    }
}
