// NIGHT41 builder J2 (design.n41.bars41, a view row: no sim bit; NIGHT41.md J2, INVENTORY C3; GG w35 "hard to understand what to do", w40 THE
// CROWN "no idea"): ONE boss bar for every boss, drawn on hud33's layer where the HUD33 boss bar sits (top centre), with
//   - the boss's name (bone, the HUD33 bar's size) over the HP bar (red-orange: the boss is danger),
//   - a TICK at each phase threshold: Boss37 bosses (THE WICK, THE STAG, THE LOOM, THE CROWN) at their brain's legAt (design.n37.boss37.legAt or
//     the boss row's kit.legAt, when autoLegs); THE ICEBREAKER at design.n40.brawl40.phase2At; a kit boss (the BULWARK, THE KILN WYRM, ...) at its
//     Bar().segment. A tick the HP has passed dims (that phase is done) and stays,
//   - the STAGGER METER under the bar (Boss37 brains and THE ICEBREAKER): bone = the meter left (a drain flashes it); in the CYAN window the
//     meter turns cyan and counts the window down (Boss37StaggerBar's language, now for all five bosses).
// While it draws, N41View.BossBar41Live = true: the lead's hooks then hide the HUD33 bar (UiHud33.Boss33) and the Boss37 stagger bar
// (Boss37StaggerBar.Show), so one bar shows, never two. Called once a frame from RenderCard38 (UiJuice38.cs, inside RenderHud33): with hud33 off
// nothing here runs and nothing is built. Reads only (HudReads20.ThreatNow and the bosses' public state); no allocation per frame after the build.
// Numbers: UnityTuning.Bars41.cs. Not checked in the Player (the lead takes the frames).
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>What UiBossBar41 drew this frame (the reels print it; all false / 0 with the row off).</summary>
    public static class BossBar41Probe
    {
        public static bool Shown, StaggerShown, Cyan;
        public static int Ticks, TicksPassed;
        /// <summary>Leg pips drawn (THE LOOM / THE CROWN: 3; 0 for a boss with HP ticks) and how many are lit (legs passed).</summary>
        public static int Legs, LegsLit;
        public static float Fill, Stagger;
        public static string Name = "", Boss = "";
    }

    /// <summary>The bar's read of the live boss (pure reads of public state; BossRead41.Read fills it).</summary>
    public struct Bar41
    {
        public bool valid, hasStagger, open;
        public string name, boss;
        public double hpFrac, staggerFrac;
        /// <summary>The phase thresholds as a share of max HP (ticks of them used; the array is the reader's, never reallocated).</summary>
        public int ticks;
        public double[] at;
        /// <summary>A boss whose phases are legs, not HP marks (THE LOOM, THE CROWN: Boss37 autoLegs false): its legs (0: none) and the leg now.</summary>
        public int legs, leg;
    }

    /// <summary>The live boss for bars41: the HUD's threat read (name, HP) plus the boss's brain (thresholds, stagger, window).</summary>
    public static class BossRead41
    {
        public const int MaxTicks = 4;
        /// <summary>A Boss37 fight's legs (Boss37Brain.leg is 1..3; NextLeg never passes 3).</summary>
        public const int LegCount = 3;
        static HudReads20.Threat threat;

        public static void Read(SurvivalGame g, ref Bar41 b)
        {
            if (b.at == null) b.at = new double[MaxTicks];
            b.valid = b.hasStagger = b.open = false;
            b.ticks = 0;
            b.legs = b.leg = 0;
            b.staggerFrac = 0;
            b.name = b.boss = null;
            b.hpFrac = 0;
            if (g == null || g.sim == null) return;
            HudReads20.ThreatNow(g, ref threat);
            if (!threat.valid || !threat.boss || !(threat.maxHp > 0)) return;
            b.valid = true;
            b.name = threat.name;
            b.hpFrac = Mathf.Clamp01((float)(threat.hp / threat.maxHp));
            SurvivalSim s = g.sim;
            Boss37Brain brain = null;
            if (s.wick37 != null && s.wick37.Alive && Live(s.wick37.brain)) { brain = s.wick37.brain; b.boss = "wick"; }
            else if (s.stag37 != null && s.stag37.Alive && Live(s.stag37.brain)) { brain = s.stag37.brain; b.boss = "stag37"; }
            else if (s.loom37 != null && s.loom37.Alive && Live(s.loom37.brain)) { brain = s.loom37.brain; b.boss = "loom37"; }
            else if (s.crown37 != null && s.crown37.Alive && Live(s.crown37.brain)) { brain = s.crown37.brain; b.boss = "crown37"; }
            if (brain != null)
            {
                if (brain.p.autoLegs) for (int i = 0; i < brain.p.legAt.Length && b.ticks < MaxTicks; i++) Tick(ref b, brain.p.legAt[i]);
                // n41-r3 probes: THE WICK (autoLegs false: its legs are places) showed no tick; its HP is locked per leg at design.n37.wick37's
                // legLock (0.6, 0.3): those are its phase marks. THE LOOM / THE CROWN (autoLegs false, legs by knees / rings, no HP mark) stay without.
                else if (b.boss == "wick" && s.wick37.p.legLock != null) for (int i = 0; i < s.wick37.p.legLock.Length && b.ticks < MaxTicks; i++) Tick(ref b, s.wick37.p.legLock[i]);
                // the lead (12:07): THE LOOM / THE CROWN's phases are legs, not HP: leg pips instead of ticks (a boss with HP ticks gets none).
                // Boss37's legs are 1..3 (Boss37Brain.NextLeg stops at 3).
                if (b.ticks == 0) { b.legs = LegCount; b.leg = System.Math.Max(1, System.Math.Min(LegCount, brain.leg)); }
                b.hasStagger = true;
                b.open = brain.Open;
                b.staggerFrac = b.open ? 1 - brain.Progress : brain.StaggerFrac;
                return;
            }
            Brawl40 br = s.brawl40;
            if (br != null && br.Alive && br.state != Brawl40State.Off && br.state != Brawl40State.Dead)
            {
                b.boss = "brawl40";
                Tick(ref b, br.p.phase2At);
                b.hasStagger = true;
                // the lead's n41-r1 frames (brawl40 'stuck'): its punish is the STUCK fist (every hit x stuckMult), not only the stagger window:
                // both count down in cyan (Progress is the state's own whole-step clock)
                b.open = br.Open || br.StuckNow;
                double full = br.StaggerFull;
                b.staggerFrac = b.open ? 1 - br.Progress : full > 0 ? System.Math.Max(0, System.Math.Min(1, br.stagger / full)) : 0;
                return;
            }
            Tick(ref b, threat.phaseAt); // a kit boss: its bar's own phase mark (0: none)
        }

        static bool Live(Boss37Brain br) => br != null && br.state != Boss37State.Off && br.state != Boss37State.Dead;

        static void Tick(ref Bar41 b, double at)
        {
            if (at > 0 && at < 1 && b.ticks < MaxTicks) b.at[b.ticks++] = at;
        }
    }

    public sealed partial class Ui
    {
        RectTransform b41Root, b41BarRt, b41StagRt;
        UiStyle33.Text33 b41Name, b41StagLabel;
        int b41LabelShown = -1; // 0 STAGGER (bone), 1 OPEN (cyan)
        Image b41Track, b41Fill, b41StagTrack, b41StagFill;
        readonly Image[] b41Ticks = new Image[BossRead41.MaxTicks];
        readonly Image[] b41Pips = new Image[BossRead41.LegCount];
        Bar41 b41Read;
        float b41ShownFill = -1, b41ShownW = -1, b41Flash;
        double b41Age = -1, b41LastStagger = -1;
        string b41ShownName;
        bool b41RowRead, b41RowOn;
        GameConfig b41Cfg;

        /// <summary>bars41 on in this session (read once per config: the row and the off switches).</summary>
        bool Bars41On()
        {
            if (!b41RowRead || !ReferenceEquals(b41Cfg, game.cfg))
            {
                b41RowRead = true;
                b41Cfg = game.cfg;
                b41RowOn = N41View.RowOn(game.cfg, "bars41");
            }
            return b41RowOn && !N41View.Off;
        }

        /// <summary>Every frame with hud33 on screen (from RenderCard38): the bar while a boss fights; BossBar41Live while it draws.</summary>
        void RenderBar41(double frameDt)
        {
            if (Bar43Frame(frameDt)) // NIGHT43 U1 bossbar43 (UiBossBar43.cs): a valid N43BossState draws there; this bar hides (one bar, never two)
            {
                N41View.BossBar41Live = true;
                BossBar41Probe.Shown = BossBar41Probe.StaggerShown = false;
                if (b41Root != null && b41Root.gameObject.activeSelf) b41Root.gameObject.SetActive(false);
                return;
            }
            if (!Bars41On())
            {
                N41View.BossBar41Live = false;
                BossBar41Probe.Shown = BossBar41Probe.StaggerShown = false;
                if (b41Root != null && b41Root.gameObject.activeSelf) b41Root.gameObject.SetActive(false);
                return;
            }
            BossRead41.Read(game, ref b41Read);
            bool on = b41Read.valid;
            N41View.BossBar41Live = on;
            BossBar41Probe.Shown = on;
            if (on && b41Root == null) BuildBar41();
            if (b41Root == null) return;
            if (b41Root.gameObject.activeSelf != on)
            {
                b41Root.gameObject.SetActive(on);
                b41Age = on ? 0 : -1;
                b41ShownFill = -1;
                b41ShownName = null;
                b41LastStagger = -1;
            }
            if (!on) { BossBar41Probe.StaggerShown = false; BossBar41Probe.Name = BossBar41Probe.Boss = ""; return; }
            // the lead's round-2 note: Boss33 ran earlier this frame with last frame's BossBar41Live (false on a fight's first frame) and turned the
            // HUD33 bar on; turn it off again now, before the canvas draws, so the two bars never share a frame (the Boss37 stagger bar is its child).
            // Next frame Boss33 sees BossBar41Live and keeps it off (its own SetActive path, unchanged).
            if (h33BossRt != null && h33BossRt.gameObject.activeSelf) h33BossRt.gameObject.SetActive(false);

            float cw = hud33.rect.width;
            float w = (float)Hud33Layout.BossBar(cw).w;
            if (w != b41ShownW)
            {
                b41ShownW = w;
                b41BarRt.sizeDelta = new Vector2(w, b41BarRt.sizeDelta.y);
                b41StagRt.sizeDelta = new Vector2(w, b41StagRt.sizeDelta.y);
                b41Name.rt.sizeDelta = new Vector2((float)Hud33Layout.TopCentreW(cw), b41Name.rt.sizeDelta.y);
            }
            if (!ReferenceEquals(b41Read.name, b41ShownName))
            {
                b41ShownName = b41Read.name;
                b41Name.Set(b41Read.name ?? "");
            }
            BossBar41Probe.Name = b41Read.name ?? "";
            BossBar41Probe.Boss = b41Read.boss ?? "";

            // the HP fill
            float fill = Mathf.Round((float)b41Read.hpFrac * (float)UnityTuning.Bars41FillQuant) / (float)UnityTuning.Bars41FillQuant;
            if (fill != b41ShownFill) { b41ShownFill = fill; b41Fill.fillAmount = fill; }
            BossBar41Probe.Fill = fill;

            // the ticks: one per threshold, dimmed once the HP is under it
            Color bone = UiStyle33.Bone;
            int passed = 0;
            for (int i = 0; i < b41Ticks.Length; i++)
            {
                bool want = i < b41Read.ticks;
                Image t = b41Ticks[i];
                if (t.gameObject.activeSelf != want) t.gameObject.SetActive(want);
                if (!want) continue;
                bool done = b41Read.hpFrac <= b41Read.at[i];
                if (done) passed++;
                float x = Mathf.Round((float)b41Read.at[i] * w);
                if (t.rectTransform.anchoredPosition.x != x) t.rectTransform.anchoredPosition = new Vector2(x, 0);
                Color c = UiTheme.A(bone, done ? UnityTuning.Bars41TickPassedAlpha : UnityTuning.Bars41TickAlpha);
                if (t.color != c) t.color = c;
            }
            BossBar41Probe.Ticks = b41Read.ticks;
            BossBar41Probe.TicksPassed = passed;

            // the leg pips (THE LOOM, THE CROWN: phases are legs, not HP): one per leg above the bar's right end, lit per leg passed
            int lit = 0;
            for (int i = 0; i < b41Pips.Length; i++)
            {
                bool want = i < b41Read.legs;
                Image p = b41Pips[i];
                if (p.gameObject.activeSelf != want) p.gameObject.SetActive(want);
                if (!want) continue;
                bool done = i < b41Read.leg - 1;
                if (done) lit++;
                Color c = UiTheme.A(bone, done ? UnityTuning.Bars41TickAlpha : UnityTuning.Bars41TickPassedAlpha);
                if (p.color != c) p.color = c;
            }
            BossBar41Probe.Legs = b41Read.legs;
            BossBar41Probe.LegsLit = lit;

            // the stagger meter: bone = the meter left (a drain flashes it), CYAN = the window counting down
            bool stag = b41Read.hasStagger;
            if (b41StagRt.gameObject.activeSelf != stag) b41StagRt.gameObject.SetActive(stag);
            BossBar41Probe.StaggerShown = stag;
            BossBar41Probe.Cyan = stag && b41Read.open;
            if (stag)
            {
                double sf = b41Read.staggerFrac;
                if (!b41Read.open && b41LastStagger >= 0 && sf < b41LastStagger - 1e-9) b41Flash = UnityTuning.Bars41DrainFlashS;
                b41LastStagger = b41Read.open ? -1 : sf;
                b41Flash = Mathf.Max(0, b41Flash - (float)frameDt);
                Color fc;
                if (b41Read.open) fc = UiStyle33.Cyan;
                else
                {
                    Color c = UiTheme.A(bone, UnityTuning.Bars41StaggerFillAlpha);
                    fc = b41Flash > 0 ? Color.Lerp(c, Color.white, b41Flash / UnityTuning.Bars41DrainFlashS) : c;
                }
                if (b41StagFill.color != fc) b41StagFill.color = fc;
                int lab = b41Read.open ? 1 : 0;
                if (lab != b41LabelShown)
                {
                    // the lead's n41-r1 frames: a full bone meter tight under the bar read as part of the bar: it is named now
                    b41LabelShown = lab;
                    b41StagLabel.Set(lab == 1 ? UnityTuning.Bars41OpenText : UnityTuning.Bars41StaggerText);
                    b41StagLabel.Color = lab == 1 ? UiStyle33.Cyan : UiTheme.A(bone, UnityTuning.Bars41LabelAlpha);
                }
                float f = Mathf.Clamp01((float)sf);
                if (b41StagFill.fillAmount != f) b41StagFill.fillAmount = f;
                BossBar41Probe.Stagger = f;
            }
            else BossBar41Probe.Stagger = 0;

            // the slide-in (motion only when a fight starts)
            if (b41Age >= 0)
            {
                b41Age += frameDt;
                float k = b41Age >= UnityTuning.Bars41SlideIn ? 1f : UiTween.OutCubic((float)(b41Age / UnityTuning.Bars41SlideIn));
                b41Root.anchoredPosition = new Vector2(0, (1 - k) * UnityTuning.Bars41Slide);
                b41Name.Alpha = k;
                if (k >= 1f) b41Age = -1;
            }
        }

        void BuildBar41()
        {
            Hud33Layout.Box bn = Hud33Layout.BossName(), bb = Hud33Layout.BossBar();
            var top = new Vector2(0.5f, 1);
            var left = new Vector2(0, 0.5f);
            Color bone = UiStyle33.Bone;
            float h = (float)(bn.h - bb.y) + UnityTuning.Bars41StaggerGap + UnityTuning.Bars41StaggerH + UnityTuning.Bars41TickOver;
            b41Root = UiStyle33.Rect(hud33, "Boss bar 41", top, top, Vector2.zero, new Vector2((float)bn.w, h));
            b41Name = UiStyle33.Label(b41Root, "Boss name 41", (float)bn.px, bone, UiStyle33.FaceLabel, TextAlignmentOptions.Top, top, top, new Vector2((float)bn.x, (float)bn.y), new Vector2((float)bn.w, (float)bn.h));
            b41BarRt = UiStyle33.Rect(b41Root, "HP 41", top, top, new Vector2((float)bb.x, (float)bb.y), new Vector2((float)bb.w, (float)bb.h));
            b41Track = UiStyle33.Line(b41BarRt, "Track", UiTheme.A(bone, UnityTuning.Hud33BossTrackAlpha), Vector2.zero, Vector2.zero, Vector2.zero, new Vector2((float)bb.w, (float)bb.h));
            Stretch41(b41Track.rectTransform);
            b41Fill = UiStyle33.Line(b41BarRt, "Fill", UiStyle33.Danger, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2((float)bb.w, (float)bb.h));
            Stretch41(b41Fill.rectTransform);
            Filled41(b41Fill);
            for (int i = 0; i < b41Ticks.Length; i++)
            {
                b41Ticks[i] = UiStyle33.Line(b41BarRt, "Tick " + i, UiTheme.A(bone, UnityTuning.Bars41TickAlpha), left, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Bars41TickW, (float)bb.h + 2 * UnityTuning.Bars41TickOver));
                b41Ticks[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < b41Pips.Length; i++)
            {
                // right-aligned at the bar's right end, standing above it (beside the name's line); leg 1 leftmost
                float x = -(b41Pips.Length - 1 - i) * (UnityTuning.Bars41PipPx + UnityTuning.Bars41PipGap);
                b41Pips[i] = UiStyle33.Line(b41BarRt, "Leg pip " + i, UiTheme.A(bone, UnityTuning.Bars41TickPassedAlpha), new Vector2(1, 1), new Vector2(1, 0), new Vector2(x, UnityTuning.Bars41PipLift), new Vector2(UnityTuning.Bars41PipPx, UnityTuning.Bars41PipPx));
                b41Pips[i].gameObject.SetActive(false);
            }
            b41StagRt = UiStyle33.Rect(b41Root, "Stagger 41", top, top, new Vector2((float)bb.x, (float)bb.y - (float)bb.h - UnityTuning.Bars41StaggerGap), new Vector2((float)bb.w, UnityTuning.Bars41StaggerH));
            b41StagTrack = UiStyle33.Line(b41StagRt, "Track", UiTheme.A(bone, UnityTuning.Bars41StaggerTrackAlpha), Vector2.zero, Vector2.zero, Vector2.zero, new Vector2((float)bb.w, UnityTuning.Bars41StaggerH));
            Stretch41(b41StagTrack.rectTransform);
            b41StagFill = UiStyle33.Line(b41StagRt, "Fill", UiTheme.A(bone, UnityTuning.Bars41StaggerFillAlpha), Vector2.zero, Vector2.zero, Vector2.zero, new Vector2((float)bb.w, UnityTuning.Bars41StaggerH));
            Stretch41(b41StagFill.rectTransform);
            Filled41(b41StagFill);
            b41StagLabel = UiStyle33.Label(b41StagRt, "Stagger label 41", UnityTuning.Bars41LabelPx, UiTheme.A(bone, UnityTuning.Bars41LabelAlpha), UiStyle33.FaceLabel, TextAlignmentOptions.Right, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(-UnityTuning.Bars41LabelGap, 0), new Vector2(UnityTuning.Bars41LabelW, UnityTuning.Bars41LabelPx + 4));
            b41LabelShown = -1;
            b41Root.gameObject.SetActive(false);
        }

        static void Stretch41(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void Filled41(Image img)
        {
            img.sprite = UiStyle33.WhiteSprite();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }
}
