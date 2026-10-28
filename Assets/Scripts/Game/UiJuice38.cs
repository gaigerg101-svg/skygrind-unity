// NIGHT38 builder H (design.n38.juice38): the boss entrance's CARD on hud33's layer: the boss's name (56 px, red-orange: the boss is danger) above the
// reticle, flickering on with the screen's flicker (three frames on / off, the message slot's TV language), its tracking closing as it lands, held
// Juice38CardHold s, then out; the kit's subtitle under it (20 px, bone) when its bossIntro follows. Juice38 (CameraFeel's) starts it through
// Juice38Probe.CardSerial; this file only draws. Built on the first card (nothing with the row off). Numbers: UnityTuning.Juice38.cs.
// NIGHT41 builder J2:
//   - bars41 (UiBossBar41.cs): RenderBar41 runs from here once a frame (this is hud33's per-frame hook J2 owns); nothing with the row off.
//   - entrance41 (design.n41.entrance41, a view row; INVENTORY C4): the entrance is ONE beat of UnityTuning.Entrance41Beat (1.5 s), the same order
//     for every boss: at 0 the name flickers in while juice38's shake + flicker play (Juice38.Entrance, the same frame), at Entrance41LineAt the
//     boss's RULE line comes in under the name, at the beat's end both fade out. The rule line: design.n41.entrance41.lines.<boss id of the
//     bossRise> (the five bosses' BossLine40 entry rules, written once in the row), else a bossLine40 line said inside the beat, else the kit's
//     bossIntro subtitle. Its {action} names resolve to the bound buttons through the lesson widget's resolver (ResolveLesson25). With the row
//     off the card is NIGHT38's exactly. Numbers: UnityTuning.Bars41.cs (Entrance41*).
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT41 J2: what the entrance beat showed (the reels print it).</summary>
    public static class Entrance41Probe
    {
        public static int Beats;
        /// <summary>n41-r3: beats whose card line yielded to a BossLine40 line under the reticle (the rule shown once), and this beat's.</summary>
        public static int Yielded;
        public static bool LineYielded;
        public static string Boss = "", Line = "";
        /// <summary>The beat's age now (s; &lt; 0: no beat) and the rule line's alpha now.</summary>
        public static double Age = -1;
        public static float LineAlpha;
    }

    public sealed partial class Ui
    {
        UiStyle33.Text33 j38Name, j38Sub;
        RectTransform j38Card;
        int j38Serial;
        double j38Age = double.MaxValue;
        string j38SubShown;

        // NIGHT41 J2 (entrance41)
        bool e41Wired, e41RowRead, e41RowOn, e41Beat, e41BossLineUp, e41LineYielded;
        GameConfig e41Cfg;
        HudReads20.Threat e41Threat;
        string e41RiseBoss, e41SaidLine, e41Line;
        double e41Clock, e41SaidAt = double.NegativeInfinity;

        /// <summary>For the reels: the card on screen this frame and its name.</summary>
        public static bool Card38Shown;
        public static string Card38Name = "";

        bool Entrance41On()
        {
            if (!e41RowRead || !ReferenceEquals(e41Cfg, game.cfg))
            {
                e41RowRead = true;
                e41Cfg = game.cfg;
                e41RowOn = N41View.RowOn(game.cfg, "entrance41");
            }
            return e41RowOn && !N41View.Off;
        }

        /// <summary>The boss id of the last bossRise and the last bossLine40 said (subscribed once, on hud33's first frame).</summary>
        void Wire41()
        {
            if (e41Wired || game.events == null) return;
            e41Wired = true;
            game.events.On<BossEvent>("bossRise", e => e41RiseBoss = e != null ? e.boss : null);
            game.events.On<BossLine30>("bossLine40", l =>
            {
                if (l == null || string.IsNullOrEmpty(l.text)) return;
                e41SaidLine = l.text;
                e41SaidAt = e41Clock;
            });
            // n41-r3 (the lead): THE ICEBREAKER / THE LOOM / THE CROWN showed their rule twice (under the card and BossLine40's line under the
            // reticle): while a BossLine40 line (LessonEvent kind "boss40") is up, the card's line yields to it
            game.events.On<LessonEvent>("lessonBegin", e => { if (e != null && e.kind == "boss40") e41BossLineUp = true; });
            game.events.On<LessonEvent>("lessonClear", e => { if (e != null && e.kind == "boss40") e41BossLineUp = false; });
        }

        /// <summary>entrance41: the card's name is the boss bar's name when the bar is up (n41-r3: THE KILN WYRM's rise said "WYRM", its bar
        /// "KILN WYRM"); else juice38's.</summary>
        string Beat41Name()
        {
            HudReads20.ThreatNow(game, ref e41Threat);
            return e41Threat.valid && e41Threat.boss && !string.IsNullOrEmpty(e41Threat.name) ? e41Threat.name : Juice38Probe.CardName;
        }

        /// <summary>Every frame with hud33 on screen (RenderHud33): a new card from Juice38, its flicker-in, hold and fade.</summary>
        void RenderCard38(double frameDt)
        {
            RenderBar41(frameDt); // NIGHT41 J2 (bars41, UiBossBar41.cs): nothing with the row off
            Wire41();
            e41Clock += frameDt;
            if (Juice38Probe.CardSerial != j38Serial)
            {
                j38Serial = Juice38Probe.CardSerial;
                if (!string.IsNullOrEmpty(Juice38Probe.CardName))
                {
                    if (j38Card == null) BuildCard38();
                    j38Name.Set(Juice38Probe.CardName);
                    j38Sub.Set("");
                    j38SubShown = "";
                    j38Age = 0;
                    Card38Name = Juice38Probe.CardName;
                    e41Beat = Entrance41On();
                    if (e41Beat) StartBeat41();
                }
            }
            if (j38Card == null) { Card38Shown = false; return; }
            double cardIn = e41Beat ? UnityTuning.Entrance41CardIn : UnityTuning.Juice38CardIn;
            double cardOut = e41Beat ? UnityTuning.Entrance41CardOut : UnityTuning.Juice38CardOut;
            double hold = e41Beat ? System.Math.Max(0, UnityTuning.Entrance41Beat - cardIn - cardOut) : UnityTuning.Juice38CardHold;
            bool on = j38Age < cardIn + hold + cardOut;
            if (j38Card.gameObject.activeSelf != on) j38Card.gameObject.SetActive(on);
            Card38Shown = on;
            if (!on)
            {
                j38Age = double.MaxValue;
                Card38Name = "";
                if (e41Beat) { e41Beat = false; Entrance41Probe.Age = -1; Entrance41Probe.LineAlpha = 0; }
                return;
            }
            string wantName = e41Beat ? Beat41Name() : Juice38Probe.CardName;
            if (wantName != Card38Name && !string.IsNullOrEmpty(wantName))
            {
                Card38Name = wantName; // the kit's intro named the boss whole (THE KILN WYRM's rise says "WYRM"); entrance41: the bar's name
                j38Name.Set(Card38Name);
            }
            double a = j38Age;
            float alpha;
            if (a < cardIn) alpha = ((int)(a / (cardIn / 3.0)) % 2) == 0 ? 1f : 0.25f; // on, dim, on
            else if (a < cardIn + hold) alpha = 1f;
            else alpha = 1f - UiTween.Clamp01((a - cardIn - hold) / cardOut);
            alpha = Mathf.Round(alpha * 50f) / 50f;
            float subAlpha = alpha;
            if (e41Beat) subAlpha = Beat41Line(a, alpha);
            else if (Juice38Probe.CardSub != j38SubShown)
            {
                j38SubShown = Juice38Probe.CardSub;
                j38Sub.Set(j38SubShown ?? "");
            }
            if (j38Name.Alpha != alpha) j38Name.Alpha = alpha;
            if (j38Sub.Alpha != subAlpha) j38Sub.Alpha = subAlpha;
            float k = UiTween.OutCubic(UiTween.Clamp01(a / (cardIn + 0.3)));
            float track = Mathf.Round(Mathf.Lerp(UnityTuning.Juice38CardTrackFrom, UnityTuning.Juice38CardTrackTo, k));
            if (j38Name.main.characterSpacing != track) j38Name.main.characterSpacing = j38Name.back.characterSpacing = track;
            j38Age += frameDt;
        }

        /// <summary>entrance41: a new beat (the card's first frame): the rule line chosen now when the row names one for this boss.</summary>
        void StartBeat41()
        {
            Entrance41Probe.Beats++;
            Entrance41Probe.Boss = e41RiseBoss ?? "";
            Entrance41Probe.Age = 0;
            e41Line = null;
            e41LineYielded = false;
            string key = "design.n41.entrance41.lines." + (e41RiseBoss ?? "");
            if (!string.IsNullOrEmpty(e41RiseBoss) && game.cfg.Has(key)) e41Line = game.cfg.Str(key);
            float px = UnityTuning.Entrance41LinePx;
            if (j38Sub.main.fontSize != px) j38Sub.main.fontSize = j38Sub.back.fontSize = px;
        }

        /// <summary>entrance41: the rule line under the name (the row's line, else a bossLine40 said inside the beat, else the kit's subtitle),
        /// in at Entrance41LineAt, out with the card. Returns its alpha.</summary>
        float Beat41Line(double age, float cardAlpha)
        {
            Entrance41Probe.Age = age;
            string raw = e41Line;
            if (string.IsNullOrEmpty(raw) && e41Clock - e41SaidAt <= age + 1e-9) raw = e41SaidLine;
            if (string.IsNullOrEmpty(raw)) raw = Juice38Probe.CardSub;
            if (raw != j38SubShown)
            {
                j38SubShown = raw;
                string shown = string.IsNullOrEmpty(raw) ? "" : ResolveLesson25(raw, null);
                j38Sub.Set(shown ?? "");
                Entrance41Probe.Line = shown ?? "";
            }
            // the rule shows ONCE: a BossLine40 line up now (or once during this beat) keeps the card's copy hidden for the rest of the beat
            if (e41BossLineUp && !e41LineYielded) { e41LineYielded = true; Entrance41Probe.Yielded++; }
            float la = age < UnityTuning.Entrance41LineAt || e41LineYielded ? 0f : cardAlpha;
            Entrance41Probe.LineAlpha = la;
            Entrance41Probe.LineYielded = e41LineYielded;
            return la;
        }

        void BuildCard38()
        {
            var mid = new Vector2(0.5f, 0.5f);
            j38Card = UiStyle33.Rect(hud33, "Boss card 38", mid, mid, new Vector2(0, UnityTuning.Juice38CardY), new Vector2(UnityTuning.Juice38CardW, UnityTuning.Juice38CardPx + UnityTuning.Juice38CardSubPx + 12));
            j38Name = UiStyle33.Label(j38Card, "Name", UnityTuning.Juice38CardPx, UiStyle33.Danger, UiStyle33.FaceNumber, TextAlignmentOptions.Bottom, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, UnityTuning.Juice38CardSubPx + 8), new Vector2(UnityTuning.Juice38CardW, UnityTuning.Juice38CardPx + 4));
            j38Sub = UiStyle33.Label(j38Card, "Subtitle", UnityTuning.Juice38CardSubPx, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.Bottom, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(UnityTuning.Juice38CardW, UnityTuning.Juice38CardSubPx + 4));
            j38Card.gameObject.SetActive(false);
        }
    }
}
