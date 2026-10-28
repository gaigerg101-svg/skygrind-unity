// NIGHT61 builder G (S3, design.n61.bossRule61; BossRule61.cs holds the rules): the boss's rule line, drawn by hud47's ONE fight line
// (UiHud47.Boss.cs's label: 40 px em at 1080p x TEXT SIZE, its outline and underlay, the glyph sprite, sentence case; one text object). No new
// label, font, colour or place: the theme's label face (Assets/Resources/Fonts) and the fight line's slot under the boss's name card.
// Hooks (one line each):
//   - UiHud47.cs BuildHud47: BossRule61Wire(): a "bossRise" of one of the five campaign bosses in a campaign run, with the row on, arms its line
//     (and builds hud47's fight line if no boss bar has built it yet: UiHud47.Boss.cs Fight47Build);
//   - UiHud47.Boss.cs Fight47Tick, before its bar-down branch: `if (BossRule61Tick(frameDt, barUp)) return;`. From the rise the line holds the
//     fight line's place (the queue below waits, as it waits for the entrance card); BossRule61DelayS after the rise it comes in under the name
//     card (the name above, the rule under it: one block; the card does not hide it), stands its hold and fades; then the queue resumes
//     (lines posted meanwhile show after it). A new rise replaces it.
// NIGHT63 G (P2, Logs/n63/slate.json P2: NIGHT62 measured THE KILN WYRM's line in 2.43 s after its rise, alpha 0.00 at the reel's 1.0 s frame):
// the timing is BossRule61.Clock / Tick (pure, N63BossRuleTimeTests): in BossRule61DelayS after the rise whether or not the boss bar is up (the
// wyrm's bar waits 2.0 s for its emerge, and on a session's first boss that bar was also what built the fight line); the bar only ends it, when
// it goes down after it was up (the fight over). While the line holds the place the name card's own subtitle (entrance41's line, else the
// kit's) is hidden, as hud47 hides it once the bar is up (Fight47Late): one rule on screen, under the name. P2b (the P2 critic's problem 5): so
// is the boss bar's own hint ("PARRY THE BLOCK. PUNISH STUCK FISTS" on THE ICEBREAKER's bar), shown again when the line ends ("[boss61] bar hint
// held" / "bar hint back" in the Player log).
// Proof (L-089): one Player.log line when the line shows, "[boss61] line: <boss> ...", with its words, its time on screen, its em in screen px
// at this resolution, the glyph and the raw-tag check, and (NIGHT63 G P2) how long after the rise it came in with the boss bar's and the name
// card's state then; one line when an armed line is refused or cut (none is dropped since NIGHT63: BossRule61Probe.Dropped stays 0).
// Off (the row off, -skygrindOff61 or older, -skygrindNoBossRule61): nothing is armed and both hooks return at once (NIGHT60's fight line).
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT61 G (bossRule61): what the rule line did (the boss61 reel prints it).</summary>
    public static class BossRule61Probe
    {
        public static int Armed, Shown, Dropped, Refused, Cut;
        /// <summary>The boss id and the line as resolved (the bound buttons filled in) of the line shown last.</summary>
        public static string Boss = "", Line = "";
        /// <summary>s since the shown line came in (-1: none showing now), its alpha now, its planned time on screen (fades included).</summary>
        public static double Age = -1, OnScreenS;
        public static float Alpha;
    }

    public sealed partial class Ui
    {
        bool br61Wired;
        string br61Boss, br61Raw;      // the armed or shown line: the rise's boss id and its line as written ({action} tokens)
        BossRule61.Clock br61 = BossRule61.Clock.Idle; // NIGHT63 G P2: from the rise (armed) to the line's end (showing); BossRule61.Tick runs it

        /// <summary>From BuildHud47 (once a session; game.events lives as long as the game): the rises arm the line; a new run clears it.</summary>
        void BossRule61Wire()
        {
            if (br61Wired || game.events == null) return;
            br61Wired = true;
            game.events.On<BossEvent>("bossRise", e => BossRule61Arm(e != null ? e.boss : null));
            game.OnNewRun += () =>
            {
                br61Raw = br61Boss = null;
                br61 = BossRule61.Clock.Idle;
                BossRule61HintRelease(); // NIGHT63 G P2b: a held hint is not left hidden into the next run (Text33.Set skips an unchanged text)
                BossRule61Probe.Age = -1;
                BossRule61Probe.Alpha = 0f;
            };
        }

        /// <summary>A boss rose: one of the five, in a campaign run, the row on: its line is armed (Fight47Tick shows it BossRule61DelayS later,
        /// the boss bar up or not) and hud47's fight line built if no boss bar has built it yet.</summary>
        void BossRule61Arm(string boss)
        {
            if (!BossRule61.IsBoss(boss) || game.sim == null || game.sim.campaign == null || !BossRule61.On(game.cfg)) return;
            string raw = BossRule61.Line(game.cfg, boss);
            int words = BossRule61.Words(raw);
            if (raw == null || words > UnityTuning.BossRule61MaxWords || !BossRule61.HasGlyph(raw))
            {
                BossRule61Probe.Refused++;
                Debug.Log($"[boss61] line: {boss} refused ({(raw == null ? "no line in design.n61.bossRule61.numbers.lines" : words > UnityTuning.BossRule61MaxWords ? words + " words, over " + UnityTuning.BossRule61MaxWords : "no {button} glyph")}): \"{raw}\"");
                return;
            }
            br61Boss = boss;
            br61Raw = raw;
            BossRule61.Arm(ref br61);
            Fight47Build(); // NIGHT63 G P2: the line from the rise (a session's first boss: no bar has built it yet)
            BossRule61Probe.Armed++;
        }

        /// <summary>From Fight47Tick (the fight line built, no finale), before its bar-down branch: true while the rule line holds the fight line
        /// (the queue below waits this frame); false when nothing is armed or showing (always, with the row off).</summary>
        bool BossRule61Tick(double frameDt, bool barUp)
        {
            if (!br61.Armed && !br61.Showing) return false;
            if (br61.Armed && br61.Showing) BossRule61Cut("a new rise", br61.age);
            double was = br61.age, waited = br61.wait;
            switch (BossRule61.Tick(ref br61, frameDt, barUp, BossRule61.Words(br61Raw)))
            {
                case BossRule61.Beat.Waiting:
                    // the place is held from the rise: nothing else shows in the fight line while the boss's name lands
                    fight47.Alpha = 0f;
                    Hud47Probe.FightLine = "";
                    BossRule61Sub();
                    return true;
                case BossRule61.Beat.Began:
                    BossRule61Begin(waited + frameDt, barUp);
                    break;
                case BossRule61.Beat.Showing:
                    break;
                case BossRule61.Beat.Cut:
                    BossRule61Cut("the boss bar went down", was);
                    return false;
                case BossRule61.Beat.Ended:
                    BossRule61End();
                    return false;
                default:
                    return false;
            }
            float a = BossRule61.Alpha(br61.age, br61.hold), s = BossRule61.Scale(br61.age);
            fight47.Alpha = a;
            var sc = new Vector3(s, s, 1f);
            if (fight47.rt.localScale != sc) fight47.rt.localScale = sc;
            Hud47Probe.FightLine = a > 0f ? fight47Shown : "";
            BossRule61Probe.Age = br61.age;
            BossRule61Probe.Alpha = a;
            BossRule61Sub();
            return true;
        }

        /// <summary>NIGHT63 G P2: while the rule line holds the place, the name card's own subtitle (entrance41's line, else the kit's) is hidden
        /// (after RenderCard38 set it this frame): the name above, the rule under it, one rule on screen. NIGHT62's b61k frame at 1.0 s showed the
        /// card's "SHOOT ITS HEAD. THE OPEN CORE HITS X3" where the rule line now stands; hud47 hides it the same way once the bar is up.</summary>
        void BossRule61Sub()
        {
            if (j38Sub != null && j38Sub.Alpha > 0f) j38Sub.Alpha = 0f;
            // NIGHT63 G P2b (the P2 critic's problem 5): the boss bar's hint too ("PARRY THE BLOCK. PUNISH STUCK FISTS" stood above the rule line on
            // THE ICEBREAKER's 1.0 s frame). hud47's Bar47Build hides it, but Text33.Set shows its holder again on each new text (UiStyle33.cs:78),
            // so it is checked every frame the line holds (after Bar43Draw set it this frame) and shown again at the line's end (BossRule61HintRelease)
            if (b43Hint == null) return;
            bool want = BossRule61.HintShown(true, b43Hint.Shown);
            if (b43Hint.rt.gameObject.activeSelf == want) return;
            b43Hint.rt.gameObject.SetActive(want);
            if (!br61HintHeld)
            {
                br61HintHeld = true;
                Debug.Log($"[boss61] bar hint held: \"{b43Hint.Shown}\" (the rule line holds the place)");
            }
        }

        bool br61HintHeld; // NIGHT63 G P2b: the rule line hid the boss bar's hint (shown again at the line's end, a cut or a new run)

        /// <summary>NIGHT63 G P2b: the line no longer holds the place: the bar's hint as Text33.Set would show it (drawn when it has text).</summary>
        void BossRule61HintRelease()
        {
            if (!br61HintHeld) return;
            br61HintHeld = false;
            if (b43Hint == null) return;
            bool on = BossRule61.HintShown(false, b43Hint.Shown);
            if (b43Hint.rt.gameObject.activeSelf != on) b43Hint.rt.gameObject.SetActive(on);
            Debug.Log($"[boss61] bar hint back: {(on ? "\"" + b43Hint.Shown + "\"" : "none (no text)")}");
        }

        /// <summary>The armed line comes in: the fight line takes it (the queue's own line, if one was up, is dropped: it was the room's banner or a
        /// hint from before the fight), with hud47's checks (one text object, no raw tag left) and the proof line (`sinceRise`: s since the rise
        /// on the HUD's frame clock; `barUp`: the boss bar this frame).</summary>
        void BossRule61Begin(double sinceRise, bool barUp)
        {
            string shown = ResolveLesson25(br61Raw, null) ?? br61Raw; // {action} -> the bound key or pad glyph (re-read at each rise)
            fight47Shown = shown;
            fight47Age = -1; // the queue has no live line of its own: it resumes when this one ends
            fight47Prio = -1;
            fight47.Set(shown);
            Case47(fight47, shown); // copy47: a sentence in sentence case
            fight47.main.ForceMeshUpdate();
            Hud47Probe.FightObjects = (fight47.main.enabled ? 1 : 0) + (fight47.back.enabled ? 1 : 0);
            string parsed = fight47.main.GetParsedText() ?? "";
            Hud47Probe.FightRawTag = parsed.IndexOf('<') >= 0 || parsed.IndexOf("sprite", System.StringComparison.OrdinalIgnoreCase) >= 0;
            Hud47Probe.FightLines++;
            int words = BossRule61.Words(br61Raw);
            fight47.Alpha = 0f;
            float k = fight47.main.rectTransform.lossyScale.y, em = fight47.main.fontSize * k;
            BossRule61Probe.Shown++;
            BossRule61Probe.Boss = br61Boss;
            BossRule61Probe.Line = shown;
            BossRule61Probe.Age = 0;
            BossRule61Probe.Alpha = 0f;
            BossRule61Probe.OnScreenS = BossRule61.OnScreenS(words);
            CultureInfo ci = CultureInfo.InvariantCulture;
            Debug.Log($"[boss61] line: {br61Boss} \"{br61Raw}\" -> \"{shown}\" | {words} words, on screen {BossRule61Probe.OnScreenS.ToString("0.00", ci)} s (max {UnityTuning.BossRule61MaxS.ToString("0.0", ci)}), em {em.ToString("0.0", ci)} px at {UnityEngine.Screen.width}x{UnityEngine.Screen.height} (canvas x{k.ToString("0.000", ci)}), glyph {(BossRule61.HasGlyph(br61Raw) ? "yes" : "NO")}, text objects {Hud47Probe.FightObjects}, raw tag {(Hud47Probe.FightRawTag ? "YES" : "no")} | in {sinceRise.ToString("0.00", ci)} s after the rise, boss bar {(barUp ? "up" : "not up yet")}, name card {(j38Card != null && j38Card.gameObject.activeSelf ? "up" : "down")}");
        }

        /// <summary>The line ran its time: the fight line is free (the queue shows what waited, if anything).</summary>
        void BossRule61End()
        {
            br61.age = -1;
            br61Raw = null;
            fight47Shown = null;
            fight47Age = -1;
            fight47Prio = -1;
            fight47.Alpha = 0f;
            if (fight47.rt.localScale != Vector3.one) fight47.rt.localScale = Vector3.one;
            Hud47Probe.FightLine = "";
            BossRule61Probe.Age = -1;
            BossRule61Probe.Alpha = 0f;
            BossRule61HintRelease(); // NIGHT63 G P2b: the bar's hint as before the line
        }

        /// <summary>The line stops before its time (the bar went down, a new rise): the same as its end, with a proof line (`age`: s it stood).</summary>
        void BossRule61Cut(string why, double age)
        {
            BossRule61Probe.Cut++;
            Debug.Log($"[boss61] cut after {age.ToString("0.00", CultureInfo.InvariantCulture)} s: {why}");
            string keep = br61Raw;
            BossRule61End();
            if (br61.Armed) br61Raw = keep; // a new rise's line stays armed
        }
    }
}
