// NIGHT42 builder U1 (design.n42.health42, a view row: no sim bit; NIGHT42.md U1, GG's G13: "heathbar need to look better ... UI needs to be animated").
// hud33 drew the health as a 64 px number over ten slash ticks that snapped, red-orange on a 150 ms flash or at <= 25 % (CODE-FACTS G13). With the row
// on, the same number and ticks (no new box, no fill), in motion:
//   - A HIT: the ticks just lost stay red-orange (the damage CHUNK) for 0.4 s, then drain tick by tick down to the health; the number counts down,
//     punches (1.18 x) and turns red-orange; the number and the ticks shake.
//   - A HEAL (a rise of 2 % of the max or more in a frame: a kill's heal, a pickup, a parry): the ticks it filled glow cyan and fade to bone, the
//     number counts up and pops cyan. A slow regen only counts the number up.
//   - LOW (hud33's 25 %): the red-orange number and ticks pulse.
// The stamina bars under the ticks stay hud33's. Off (health42 off, -skygrindOff42 or any older switch): hud33's Health33 exactly. View only.
// Numbers: UnityTuning.Ui42.cs.
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT42 health42's statics for the reels.</summary>
    public static class Health42
    {
        public static bool Live;
        public static int Hits, Heals, ChunkTicks, GlowTicks, LitTicks, NumberShown;
        public static float Chunk = -1f, Frac = -1f;

        public static void ResetCounters()
        {
            Hits = Heals = ChunkTicks = GlowTicks = LitTicks = 0;
            NumberShown = 0;
            Chunk = Frac = -1f;
        }
    }

    /// <summary>health42's rules (pure; EditMode tests: Health42Tests).</summary>
    public static class Health42Math
    {
        /// <summary>The chunk's level after a frame: a drop raises it to the old health and restarts the hold; the hold runs out, then it drains at
        /// ChunkDrain to the health; it never sits under the health.</summary>
        public static float Chunk(float chunk, float last, float frac, ref double hold, double dt)
        {
            if (frac < last - UnityTuning.Health42Eps)
            {
                chunk = Math.Max(chunk, last);
                hold = UnityTuning.Health42ChunkHold;
                return chunk;
            }
            if (hold > 0) hold -= dt;
            else chunk -= (float)(UnityTuning.Health42ChunkDrain * dt);
            return Math.Max(chunk, frac);
        }

        /// <summary>Ticks lit for a share (hud33's rule: one per started tenth).</summary>
        public static int Ticks(float share, int n) => Mathf.Clamp(Mathf.CeilToInt(share * n - 1e-4f), 0, n);

        /// <summary>The number's eased value toward the health (snaps within 0.5).</summary>
        public static double EaseNumber(double shown, double hp, double dt)
        {
            if (double.IsNaN(shown)) return hp;
            double v = shown + (hp - shown) * Math.Min(1.0, dt * UnityTuning.Health42NumEase);
            return Math.Abs(hp - v) < 0.5 ? hp : v;
        }
    }

    public sealed partial class Ui
    {
        bool health42On, health42Read, hp42Built;
        RectTransform hp42Ticks;
        Vector2 hp42TicksHome, hp42NumHome;
        float hp42Last = -1f, hp42Chunk = -1f, hp42HealLo, hp42HealHi;
        double hp42Hold, hp42Num = double.NaN, hp42HitAge = double.MaxValue, hp42HealAge = double.MaxValue, hp42Clock;
        int hp42ShownNum = int.MinValue;

        bool Health42On
        {
            get
            {
                if (!health42Read && cfg != null)
                {
                    health42On = N42View.RowOn(cfg, "health42");
                    health42Read = true;
                }
                return health42On;
            }
        }

        /// <summary>From BuildHud33Now with health42 on (after the ticks are built).</summary>
        void BuildHealth42()
        {
            hp42Built = true;
            hp42Ticks = h33HpTicks[0].rectTransform.parent as RectTransform;
            hp42TicksHome = hp42Ticks.anchoredPosition;
            hp42NumHome = h33Health.rt.anchoredPosition;
            game.OnNewRun += () =>
            {
                hp42Last = hp42Chunk = -1f;
                hp42Hold = 0;
                hp42Num = double.NaN;
                hp42HitAge = hp42HealAge = double.MaxValue;
                hp42ShownNum = int.MinValue;
                Health42.ResetCounters();
            };
        }

        /// <summary>Health33's health42 branch: the number and the ticks in motion (the stamina stays Health33's).</summary>
        void Health42Draw(Health h, double frameDt)
        {
            hp42Clock += frameDt;
            float frac = Mathf.Clamp01((float)(h.hp / Math.Max(1, h.max)));
            if (hp42Last < 0f) { hp42Last = frac; hp42Chunk = frac; }
            // the events: a drop is a hit, a big enough rise a heal
            if (frac < hp42Last - UnityTuning.Health42Eps) { hp42HitAge = 0; Health42.Hits++; }
            if (frac >= hp42Last + UnityTuning.Health42HealMin)
            {
                hp42HealLo = hp42HealAge < UnityTuning.Health42HealTime ? Math.Min(hp42HealLo, hp42Last) : hp42Last;
                hp42HealHi = frac;
                hp42HealAge = 0;
                Health42.Heals++;
            }
            hp42Chunk = Health42Math.Chunk(hp42Chunk, hp42Last, frac, ref hp42Hold, frameDt);
            hp42Last = frac;
            // the number counts toward the health
            hp42Num = Health42Math.EaseNumber(hp42Num, h.hp, frameDt);
            int num = (int)Math.Ceiling(hp42Num);
            if (num != hp42ShownNum)
            {
                hp42ShownNum = num;
                h33Health.Set(num.ToString(CultureInfo.InvariantCulture));
            }
            h33HurtLeft = Math.Max(0, h33HurtLeft - frameDt); // hud33's hit flash clock (its hurt listener sets it)
            bool low = frac <= UnityTuning.Hud33LowFrac;
            float pulse = low ? Mathf.Lerp(UnityTuning.Health42LowAlpha, 1f, 0.5f + 0.5f * Mathf.Cos((float)(hp42Clock * UnityTuning.Health42LowHz * 2 * Math.PI))) : 1f;
            Color bone = UiStyle33.Bone, cyan = UiStyle33.Cyan, danger = UiStyle33.Danger;
            float hitK = hp42HitAge < UnityTuning.Health42ShakeTime ? (float)(1 - hp42HitAge / UnityTuning.Health42ShakeTime) : 0f;
            float healK = hp42HealAge < UnityTuning.Health42HealTime ? (float)(1 - hp42HealAge / UnityTuning.Health42HealTime) : 0f;
            // the number: red-orange on a hit or low, cyan on a heal; a punch / a pop
            Color nc = h33HurtLeft > 0 || low ? danger : bone;
            if (healK > 0f && !low) nc = Color.Lerp(bone, cyan, healK);
            h33Health.Color = nc;
            h33Health.Alpha = Mathf.Round(pulse * 50f) / 50f;
            float sc = 1f;
            if (hp42HitAge < UnityTuning.Health42HitPunch) sc = Mathf.Lerp(UnityTuning.Health42HitScale, 1f, UiTween.OutCubic(hp42HitAge / UnityTuning.Health42HitPunch));
            else if (hp42HealAge < UnityTuning.Health42HitPunch) sc = Mathf.Lerp(UnityTuning.Health42HealScale, 1f, UiTween.OutCubic(hp42HealAge / UnityTuning.Health42HitPunch));
            var scv = new Vector3(sc, sc, 1f);
            if (h33Health.rt.localScale != scv) h33Health.rt.localScale = scv;
            Vector2 shake = Vector2.zero;
            if (hitK > 0f)
            {
                double w = hp42HitAge * UnityTuning.Health42ShakeHz * 2 * Math.PI;
                float a = UnityTuning.Health42ShakePx * hitK * hitK;
                shake = new Vector2(Mathf.Round((float)Math.Sin(w) * a), Mathf.Round((float)Math.Sin(w * 1.41 + 0.7) * a));
            }
            Vector2 np = hp42NumHome + shake, tp = hp42TicksHome + shake;
            if (h33Health.rt.anchoredPosition != np) h33Health.rt.anchoredPosition = np;
            if (hp42Ticks.anchoredPosition != tp) hp42Ticks.anchoredPosition = tp;
            // the ticks: lit (bone / red-orange low, pulsing), the chunk (red-orange), the heal's glow (cyan fading), spent (faint)
            int n = h33HpTicks.Length;
            int lit = Health42Math.Ticks(frac, n), chunk = Health42Math.Ticks(hp42Chunk, n);
            int glowLo = Health42Math.Ticks(hp42HealLo, n), glowHi = Health42Math.Ticks(hp42HealHi, n);
            Color on = low ? UiTheme.A(danger, pulse) : (h33HurtLeft > 0 ? danger : bone), off = UiTheme.A(bone, UnityTuning.Hud33GrappleSpentAlpha);
            int chunkN = 0, glowN = 0;
            for (int i = 0; i < n; i++)
            {
                Color c;
                if (i < lit)
                {
                    c = on;
                    if (healK > 0f && i >= glowLo && i < glowHi) { c = Color.Lerp(on, cyan, healK); glowN++; }
                }
                else if (i < chunk) { c = danger; chunkN++; }
                else c = off;
                if (h33HpTicks[i].color != c) h33HpTicks[i].color = c;
            }
            h33ShownLit = -1; // hud33's change tracker: redraw from scratch if the row is ever turned off mid-session
            if (hp42HitAge < double.MaxValue) hp42HitAge = hp42HitAge > Math.Max(UnityTuning.Health42ShakeTime, UnityTuning.Health42HitPunch) ? double.MaxValue : hp42HitAge + frameDt;
            if (hp42HealAge < double.MaxValue) hp42HealAge = hp42HealAge > UnityTuning.Health42HealTime ? double.MaxValue : hp42HealAge + frameDt;
            Health42.Live = true;
            Health42.Frac = frac;
            Health42.Chunk = hp42Chunk;
            Health42.LitTicks = lit;
            Health42.ChunkTicks = chunkN;
            Health42.GlowTicks = glowN;
            Health42.NumberShown = num;
        }
    }
}
