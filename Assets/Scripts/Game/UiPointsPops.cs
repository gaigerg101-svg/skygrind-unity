// NIGHT16 U3-29.8 polish #30 (a web port, hud.js :96-97 / :309 / :414-415 / :524-535 / :918-932; SURVIVAL S1): the "+60"
// points pops near the crosshair, the points' own cue. A pool of config.points.popPool labels in the HUD's sight layer
// (hidden with the sight). The web's x jitter is Math.random; here a Mulberry32 seeded per run (the economy's `points`
// "start", at every run's start) with UnityTuning.PointsPopSeedSalt, as MovementFx does.
// NIGHT17 U3-30.7 (builder H): rebuilt for energy (Research/2026-10-05-overhaul.md section 2; round 1's reviewer):
// - GOLD (points only), the display face, a hard INK shadow, tabular digits, its first frame white (FLASH INTENSITY 0: none);
// - round 2's reviewer: a 4-5 px INK stroke (TMP outline) so they read over anything; up-right of the crosshair
//   (Hud17PopX / Y), never on the body under it; sized by what the gain pays: S32 a hit, S56 a kill, S75 a headshot or punch
//   kill (PopMath.TierOf by the reason, raised by the same shot's hit event: killed + headshot / melee);
// - ONE GROWING NUMBER per burst: a gain within 500 ms of a pop's last gain joins it ("+285"), restarting its hold with a
//   punch, taking the burst's biggest tier;
// - the envelope: in at 1.8x -> 1.0 (ease-out-back, 120 ms), hold 350 ms, then a 250 ms ease-in FLIGHT INTO THE COUNTER
//   (shrinking to 0.6x): decoration only (round 2's reviewer: the bank itself ticks on the gain's frame, UiHudMotion's
//   RenderPoints17); the landing gives the counter a second, small punch (Hud17PopArrivePunch);
// - the round's bonus pops under the round's CLEAR ("+100 ROUND", S56), holds 0.45 s and flies in the same way, so it
//   tallies into the counter inside the CLEAR's 1.2 s (UiCeremony.cs);
// - older live pops step up Hud17PopStack px each (eased) so separate bursts read as a column, not a pile.
// No allocation per frame (TMP SetText with numbers; colours from UiTheme; the counter's place read from its transform).
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        sealed class PointsPop
        {
            public TextMeshProUGUI t;
            public double age = double.PositiveInfinity, sinceGain, x, y, lift, liftTo, white, hold, pending;
            public bool round;
            public PopStack stack;
            public UiPunch punch;
            public float size;
            public double fade18 = double.PositiveInfinity; // NIGHT18 round 2: s since a newer burst took its lane (it fades as it leaves)
        }

        PointsPop[] popPool;
        int popNext, popNewest = -1;
        Mulberry32 popRng;
        double popSpread;
        bool popArrived; // a pop landed in the counter this frame (RenderPoints17 punches it)
        bool popArrivedRound; // round 3's reviewer: the round's bonus landed (the score rolls then: the ceremony's exception)

        /// <summary>Round 3's reviewer: the round's bonus still flying to the score (the score holds it back until it lands).</summary>
        double PointsHeldBack()
        {
            if (popPool == null) return 0;
            double s = 0;
            for (int i = 0; i < popPool.Length; i++) if (popPool[i].round) s += popPool[i].pending;
            return s;
        }
        double popRoundBest; // U3-30.10 (c): the round's biggest burst (the CLEAR's tally; UiCeremony resets it per round)

        void BuildPointsPops()
        {
            popSpread = cfg.Num("points.popSpread");
            popRng = new Mulberry32(game.runSeed ^ UnityTuning.PointsPopSeedSalt);
            popPool = new PointsPop[cfg.Int("points.popPool")];
            float big = UiTheme.Px(UiTheme.Size.S56);
            for (int i = 0; i < popPool.Length; i++)
            {
                TextMeshProUGUI t = Hud17Text(sight, $"Points pop {i}", "", big, UiTheme.Gold, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.PointsPopBoxWidth, UiTheme.Px(UiTheme.Size.S75) * 1.4f), true);
                t.outlineWidth = UnityTuning.Hud17PopOutline; // the INK stroke (a material instance per pop, made once)
                t.outlineColor = UiTheme.InkSolid;
                t.gameObject.SetActive(false);
                popPool[i] = new PointsPop { t = t };
            }
            game.events.On<PointsEvent>("points", e =>
            {
                if (e.reason == "start") // a run starts: a fresh stream and no pop left over
                {
                    popRng = new Mulberry32(game.runSeed ^ UnityTuning.PointsPopSeedSalt);
                    popNext = 0;
                    popNewest = -1;
                    foreach (PointsPop q in popPool)
                    {
                        q.age = double.PositiveInfinity;
                        q.pending = 0;
                        if (q.t.gameObject.activeSelf) q.t.gameObject.SetActive(false);
                    }
                    return;
                }
                if (e.delta <= 0 || !sight.gameObject.activeInHierarchy) return; // hud.js: `!visible`
                bool round = e.reason == "round";

                int tier = PopMath.TierOf(e.reason);
                // Round 5: merge into the live pop whose last gain is the youngest (any of them, not only the newest: two
                // kills ~17 ms apart once made two pops that overprinted "+12025"), when that gain is within 150 ms.
                PointsPop last = null;
                if (!round)
                    foreach (PointsPop q in popPool)
                        if (!double.IsPositiveInfinity(q.age) && !q.round && PopMath.Fly(q.age, q.hold) <= 0 && PopMath.Merges(true, q.sinceGain) && (last == null || q.sinceGain < last.sinceGain))
                            last = q;
                if (last != null)
                {
                    // the burst's one running sum: the hold starts again, a punch, a white frame
                    last.stack.Add(e.delta, tier);
                    last.pending += e.delta;
                    last.sinceGain = 0;
                    if (last.age > UnityTuning.Hud17PopIn) last.age = UnityTuning.Hud17PopIn;
                    last.punch.Kick(UnityTuning.Hud17PopMergePunch, UnityTuning.Hud17PopMergePunchTime, force: true); // round 3: 1.0 -> 1.35 -> 1.0
                    PopLabel(last);
                    return;
                }
                // Round 6 (the reel: an older pop, still holding from the burst before, began its flight from the very place the
                // new one appeared, so "+125" and "+120" overprinted for ~100 ms): the older pops leave for the counter from
                // where they are, and a new burst's pop is stacked a line above any of them still near the crosshair.
                float above = 0;
                // NIGHT18 round 2 (build 6's roundend, tonight's side f109-f112: "+71" printed over "+75"): NIGHT17's lift is
                // capped at one line, so a third burst landed on a lifted one. Two lanes (0 and one line up): a new burst takes
                // a free one; with both taken, the one whose pop is further on its way, and that pop fades out as it leaves.
                PointsPop lane0 = null, lane1 = null;
                double fly0 = -1, fly1 = -1;
                foreach (PointsPop q in popPool)
                {
                    if (double.IsPositiveInfinity(q.age) || q.round) continue;
                    double qf = PopMath.Fly(q.age, q.hold);
                    if (qf < 0.5)
                    {
                        above = Math.Max(above, (float)Math.Abs(q.lift) + UnityTuning.Hud17PopStack);
                        if (Math.Abs(q.liftTo) < UnityTuning.Hud17PopStack * 0.5) { if (qf > fly0) { fly0 = qf; lane0 = q; } } // NIGHT19: a mirrored lane's lift is negative
                        else if (qf > fly1) { fly1 = qf; lane1 = q; }
                    }
                    q.age = PopMath.FlyNow(q.age, q.hold); // round 3: and leave for the counter now
                }
                float lift18 = 0;
                if (lane0 != null)
                {
                    if (lane1 == null) lift18 = UnityTuning.Hud17PopStack;
                    else if (fly1 > fly0)
                    {
                        lift18 = UnityTuning.Hud17PopStack;
                        if (FeelAB.On) lane1.fade18 = 0;
                    }
                    else if (FeelAB.On) lane0.fade18 = 0;
                }
                PointsPop p = popPool[popNext];
                if (p.pending > 0)
                {
                    // the ring came round to a pop still in flight: it lands now (its points reach the counter)
                    if (p.round) popArrivedRound = true;
                    p.pending = 0;
                    popArrived = true;
                }
                popNewest = popNext;
                popNext = (popNext + 1) % popPool.Length;
                p.age = 0;
                p.sinceGain = 0;
                p.round = round;
                p.hold = round ? UnityTuning.Hud17ClearBonusHold : UnityTuning.Hud17PopHold;
                p.lift = p.liftTo = round ? 0 : FeelAB.On ? lift18 : Math.Min(UnityTuning.Hud17PopLiftMax, above); // round 6: a line above the older pops (NIGHT18: a free lane)
                p.fade18 = double.PositiveInfinity;
                p.stack = default;
                p.stack.Add(e.delta, tier);
                p.pending = e.delta;
                // NIGHT18 round 1's picks ("+96 overlaps +100 ROUND", FeelAB on): the round's bonus pops left of the CLEAR's
                // middle, clear of the kill pops' lane up-right of the crosshair (the jitter's draw is unchanged: the stream
                // stays the same on both sides)
                double popX = UnityTuning.Hud17PopX + (round ? 0 : (popRng.Next() * 2 - 1) * UnityTuning.Hud17PopJitter);
                p.x = round ? (FeelAB.On ? -UnityTuning.Hud18RoundPopX : 0) : popX;
                p.y = round ? UnityTuning.Hud17ClearY - UnityTuning.Hud17ClearBonusDrop : UnityTuning.Hud17PopY;
                if (!round && PopsUnderClear19)
                {
                    // NIGHT19 U3-33.8 (NIGHT18's "too much": roundend's pops over the CLEAR's band): while ROUND N CLEAR shows,
                    // a kill's pop takes the lanes BELOW the crosshair (mirrored), never the band's
                    p.y = -UnityTuning.Hud17PopY;
                    p.lift = p.liftTo = -p.liftTo;
                }
                p.white = 0; // round 5: no white frame on a pop (it read as a big white number turning into a small gold one)
                PopLabel(p);
            });
            // The same shot's hit event (it follows the economy's points in the same emit): a headshot or punch kill raises
            // the pop it just made to the top tier. The handler only writes the pop's text (no sim state).
            game.events.On<HitEvent>("hit", e =>
            {
                if (e.owner != "player" || !e.killed || !(e.headshot || e.melee) || popNewest < 0) return;
                PointsPop q = popPool[popNewest];
                if (q.round || double.IsPositiveInfinity(q.age) || q.sinceGain > 0) return;
                q.stack.Raise(2);
                PopLabel(q);
            });
        }

        /// <summary>The pop's text and size from its stack (on a gain only).</summary>
        void PopLabel(PointsPop p)
        {
            float px = p.round ? UiTheme.Px(UiTheme.Size.S56) : PopMath.SizeOfTier(p.stack.tier);
            if (px != p.size)
            {
                p.size = px;
                p.t.fontSize = px;
            }
            if (p.round) p.t.SetText(UnityTuning.Hud17Tabular + "+{0:0}</mspace> ROUND", (float)p.stack.Shown);
            else
            {
                p.t.SetText(UnityTuning.Hud17Tabular + "+{0:0}", (float)p.stack.Shown);
                if (p.stack.sum > popRoundBest) popRoundBest = p.stack.sum;
            }
        }

        /// <summary>Frame time: each live pop's entrance, hold and flight into the counter; its landing releases its points.</summary>
        void RenderPointsPops(double frameDt)
        {
            if (popPool == null) return;
            float ease = 1f - Mathf.Exp(-(float)frameDt * 25f); // the step up eases in ~0.1 s
            Vector2 counter = PopCounterPoint();
            // NIGHT19 U3-33.8 (NIGHT18's "too much": roundend's pops over the CLEAR's band): the frame the CLEAR starts, a kill
            // pop still in the upper lanes leaves at once, fading as it flies (its points still land in the counter)
            bool clear19 = PopsUnderClear19;
            bool clearStarts19 = clear19 && !popClearShown19;
            popClearShown19 = clear19;
            foreach (PointsPop p in popPool)
            {
                if (double.IsPositiveInfinity(p.age)) continue;
                if (clearStarts19 && !p.round && p.y > 0)
                {
                    p.age = PopMath.FlyNow(p.age, p.hold);
                    if (double.IsPositiveInfinity(p.fade18)) p.fade18 = 0;
                }
                // NIGHT18 U3-31.6 (FeelAB on): a new pop's first frame at FirstAge (the same first frame at any fps)
                p.age = FeelAB.On ? HudEntrance18.Step(p.age, frameDt) : p.age + frameDt;
                p.sinceGain += frameDt;
                if (PopMath.Arrived(p.age, p.hold))
                {
                    p.age = double.PositiveInfinity;
                    p.t.gameObject.SetActive(false);
                    if (p.pending > 0)
                    {
                        if (p.round) popArrivedRound = true; // the round's bonus lands: the score rolls now
                        p.pending = 0;
                        popArrived = true; // RenderPoints17: a second, small punch (the round's: the roll and its big punch)
                    }
                    continue;
                }
                if (!p.t.gameObject.activeSelf) p.t.gameObject.SetActive(true);
                p.lift += (p.liftTo - p.lift) * ease;
                var from = new Vector2((float)Math.Round(p.x), (float)Math.Round(p.y + p.lift));
                float fly = (float)PopMath.Fly(p.age, p.hold);
                // NIGHT18 U3-31.6 (FeelAB on; NIGHT17's reviewer: "big-white to small-gold in one frame"): the pop ENTERS
                // (HudEntrance18: grows from 0.6x with its overshoot and fades in, GOLD from its first frame), then NIGHT17's
                // merge punch, hold and flight
                float enter = FeelAB.On ? HudEntrance18.Scale(p.age) : (float)PopMath.Scale(p.age);
                float s = enter * p.punch.Step(frameDt) * Mathf.Lerp(1f, UnityTuning.Hud17PopFlyScale, fly);
                p.t.rectTransform.anchoredPosition = fly > 0 ? Vector2.LerpUnclamped(from, counter, fly) : from;
                p.t.rectTransform.localScale = new Vector3(s, s, 1);
                p.white = Math.Max(0, p.white - frameDt);
                Color pc = p.white > 0 ? UiTheme.White : UiTheme.Gold;
                if (FeelAB.On)
                {
                    pc.a = HudEntrance18.Alpha(p.age);
                }
                if (!double.IsPositiveInfinity(p.fade18) && (FeelAB.On || clear19))
                {
                    p.fade18 += frameDt; // a newer burst took its lane, or the CLEAR began: out over Hud18ExitTime as it leaves (its points still land)
                    pc.a *= HudEntrance18.ExitAlpha(p.fade18);
                }
                p.t.color = pc;
            }
        }

        bool popClearShown19;

        /// <summary>NIGHT19 U3-33.8: ROUND N CLEAR is on screen or about to be (UiCeremony): the kill pops keep off its band.</summary>
        bool PopsUnderClear19 => ceremonyRound && (cerClearPending || !RoundClearMath.Done(cerClearSlamT));

        /// <summary>The points counter's middle in the sight layer's space (the pops fly there): its gold bar's middle, a box higher.</summary>
        Vector2 PopCounterPoint()
        {
            if (pointsBar17 == null) return new Vector2(0, -UnityTuning.Hud17PopY);
            RectTransform bar = pointsBar17.rectTransform;
            Vector3 w = bar.TransformPoint(new Vector3(-bar.sizeDelta.x * 0.5f, UnityTuning.Hud17PointsBox * 0.5f, 0));
            Vector3 l = sight.InverseTransformPoint(w);
            return new Vector2(l.x, l.y);
        }
    }

    /// <summary>NIGHT17 U3-30.7: a burst's gains as ONE running sum ("+285": round 1's reviewer; Hud17Tests).</summary>
    public struct PopStack
    {
        public double first, sum;
        public int count;
        public bool equal;

        public void Add(double delta)
        {
            if (count == 0)
            {
                first = sum = delta;
                count = 1;
                equal = true;
                return;
            }
            equal &= delta == first;
            sum += delta;
            count++;
        }

        /// <summary>The value after the "+": the burst's running sum.</summary>
        public double Shown => sum;

        /// <summary>The burst's size tier: its biggest gain's (0 a hit, 1 a kill, 2 a headshot or punch kill).</summary>
        public int tier;

        public void Add(double delta, int gainTier)
        {
            bool first = count == 0;
            Add(delta);
            tier = first ? gainTier : Math.Max(tier, gainTier);
        }

        /// <summary>Raise the tier (the same shot turned out a headshot or punch kill).</summary>
        public void Raise(int gainTier) => tier = Math.Max(tier, gainTier);
    }

    /// <summary>NIGHT17 U3-30.7: the pops' rules (Research/2026-10-05-overhaul.md section 2; round 1's reviewer; Hud17Tests).</summary>
    public static class PopMath
    {
        /// <summary>s from the gain to the counter: in, hold, flight.</summary>
        public static double Life => UnityTuning.Hud17PopIn + UnityTuning.Hud17PopHold + UnityTuning.Hud17PopFly;

        /// <summary>Round 2's reviewer: 36 px a hit, 56 a kill, 80 a headshot or punch kill, on the type scale: S32 / S56 / S75.</summary>
        public static float SizeOfTier(int tier) =>
            UiTheme.Px(tier <= 0 ? UiTheme.Size.S32 : tier == 1 ? UiTheme.Size.S56 : UiTheme.Size.S75);

        /// <summary>A gain's tier from the economy's reason: "kill" 1, anything else (a hit, a parry, DEADEYE) 0.</summary>
        public static int TierOf(string reason) => reason == "kill" ? 1 : 0;

        /// <summary>The flight into the counter, 0 through the entrance and the hold, then 0 -> 1 (ease-in) over Hud17PopFly.</summary>
        public static double Fly(double age) => Fly(age, UnityTuning.Hud17PopHold);

        public static double Fly(double age, double hold)
        {
            double k = (age - UnityTuning.Hud17PopIn - hold) / UnityTuning.Hud17PopFly;
            return k <= 0 ? 0 : UiTween.InCubic(k);
        }

        /// <summary>The pop has reached the counter.</summary>
        public static bool Arrived(double age) => Arrived(age, UnityTuning.Hud17PopHold);

        public static bool Arrived(double age, double hold) => age >= UnityTuning.Hud17PopIn + hold + UnityTuning.Hud17PopFly;

        /// <summary>Round 3's reviewer: the age that starts a holding pop's flight now (a new burst began); a flying pop keeps its age.</summary>
        public static double FlyNow(double age, double hold) => Math.Max(age, UnityTuning.Hud17PopIn + hold);

        /// <summary>A live pop whose last gain is younger than the merge window takes the next gain.</summary>
        public static bool Merges(bool live, double age) => live && age < UnityTuning.Hud17PopMerge;

        /// <summary>Round 5: a punch in: Hud17PopFrom (0.7x) -> Hud17PopOver (1.3x) at half of Hud17PopIn -> 1.0 at its end (ease-out each way), then 1.</summary>
        public static double Scale(double age)
        {
            double k = age / UnityTuning.Hud17PopIn;
            if (k >= 1) return 1;
            if (k < 0.5) return UnityTuning.Hud17PopFrom + (UnityTuning.Hud17PopOver - UnityTuning.Hud17PopFrom) * UiTween.OutCubic(k / 0.5);
            return UnityTuning.Hud17PopOver + (1 - UnityTuning.Hud17PopOver) * UiTween.OutCubic((k - 0.5) / 0.5);
        }
    }
}
