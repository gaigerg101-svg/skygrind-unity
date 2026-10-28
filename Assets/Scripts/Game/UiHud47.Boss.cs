// NIGHT47 builder F (design.n47.hud47; NIGHT47.md G5's system; the lead's n47-r0b..r0e frames: on every boss the bar's sub-lines ("PUNCH THE HEAD TO
// STAGGER IT", "LEG 1/3", "STAGGER") read ~12 px at 1080p, and the boss entrance card ("THE LOOM") drew on top of the teach card: GG's "the text that
// appears during the fight ... tiny, unclear"). The boss bar is built lazily (UiBossBar43: the first boss), so hud47's build-time sizing never
// reached it. With the row on, when the bar is built:
//   - the boss name stays on the bar (36 px); the LEG / STAGGER / OPEN labels go to Hud47BossLabelPx (they are labels, not lines);
//   - the HINT leaves the bar and becomes a FIGHT LINE: its own label in the upper third (Hud47FightY, centred), Hud47FightPx with the outline and
//     the shadow twin, one line at a time, held Hud47LineHoldS + Hud47LineHoldPerWord a word (then gone until the hint changes), a 0.12 s
//     scale-in from 1.08 and a 0.25 s fade, never over the reticle;
//   - the slots never share a place: the boss entrance card moves up to Hud47CardY (it sat on the teach card's +170), and a fight line waits
//     while the card is up (one at a time);
// The bosses' words are W's / B's / L's: this sizes and places them. Off: the bar as NIGHT46. Numbers: UnityTuning.F47.cs.
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UiStyle33.Text33 fight47;
        string fight47Pending, fight47Shown;
        double fight47Age = -1, fight47Hold;

        /// <summary>From UiBossBar43's lazy build (hud47 on): the labels sized, the hint hidden from the bar, the fight line built.</summary>
        void Bar47Build()
        {
            Size47(b43Name, UnityTuning.Hud47BossNamePx);
            Size47(b43Leg, UnityTuning.Hud47BossLabelPx);
            Size47(b43Tag, UnityTuning.Hud47BossLabelPx);
            foreach (UiStyle33.Text33 t in new[] { b43Name, b43Leg, b43Tag }) Outline47(t);
            if (b43Hint != null) b43Hint.rt.gameObject.SetActive(false); // the hint is the fight line now
            Fight47Build();
        }

        /// <summary>The fight line itself (once): from Bar47Build, and since NIGHT63 G P2 also from a boss rule line's arm (UiBossRule61.cs), so a
        /// session's first boss has the line from its rise: THE KILN WYRM's bar, which used to build it, comes up 2.0 s after the rise.</summary>
        void Fight47Build()
        {
            if (fight47 != null) return;
            var top = new Vector2(0.5f, 1);
            fight47 = UiStyle33.Label(hud33, "Fight line 47", UnityTuning.Hud47FightPx, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.Center, top, top,
                new Vector2(0, -UnityTuning.Hud47FightY), new Vector2(UnityTuning.Hud47FightW, UnityTuning.Hud47FightPx + 12));
            fight47.main.richText = true; // a glyph sprite (the hint's {action})
            // n47-r1e (and r0f's WICK ghost): the shadow twin was plain text, so it printed the RAW `<sprite="PadGlyph...">` markup, wider than
            // the line, as faint copies left and right of it. The twin is not drawn for the fight line (a sprite would double in it); the
            // line's shadow is the main text's own underlay instead (one object, one string)
            fight47.back.enabled = false;
            fight47.main.textWrappingMode = TextWrappingModes.NoWrap;
            fight47.main.overflowMode = TextOverflowModes.Overflow;
            Outline47(fight47);
            Underlay47(fight47.main);
            sized47.Add((fight47, UnityTuning.Hud47FightPx));
            fight47.Alpha = 0f;
        }

        /// <summary>From Bar43Draw when the hint changes (hud47 on): the line is queued (shown when no other text holds its place).</summary>
        void Fight47Post(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            Fight47Post(line, 2); // a boss hint: the highest
        }

        // the queue's priorities (the lead's n47-r0f: up to three texts at once while a boss fought): 2 a boss line (the bar's hint, a boss line through
        // the lesson path), 1 a banner (the message slot: a leg line, a room's name), 0 a teach card. Newest replaces at once when it ranks the same or
        // higher; a lower one waits for the current line to end. While a boss bar is up nothing else draws text in the upper third.
        int fight47Prio = -1, fight47PendingPrio = -1;
        string seenTeach47, seenSlot47;

        void Fight47Post(string line, int prio)
        {
            if (string.IsNullOrEmpty(line) || line == fight47Shown && fight47Age >= 0) return;
            if (fight47Pending != null && prio < fight47PendingPrio) return;
            fight47Pending = line;
            fight47PendingPrio = prio;
        }

        /// <summary>From the end of RenderHud33 (after the slot, the teach card and the entrance card drew): with a boss bar up, the teach card's and the
        /// slot's lines are taken into the one queue and their own slots hidden; the entrance card keeps its name, never its rule line.</summary>
        void Fight47Late(double frameDt)
        {
            bool barUp = b43Root != null && b43Root.gameObject.activeSelf;
            if (barUp && fight47 != null)
            {
                if (t43Root != null && t43Root.gameObject.activeSelf)
                {
                    if (t43Text != null && t43Text != seenTeach47) { seenTeach47 = t43Text; Fight47Post(t43Text, lesson25Raw != null && lesson25Raw == h33BossRaw ? 2 : 0); }
                    t43Root.gameObject.SetActive(false);
                }
                if (h33Slot != null && h33Slot.Alpha > 0f)
                {
                    string s = h33Slot.Shown;
                    if (!string.IsNullOrEmpty(s) && s != seenSlot47) { seenSlot47 = s; Fight47Post(Copy47.Map(cfg, s), 1); } // copy47: "MOVE" -> "Keep moving!"
                    h33Slot.Alpha = 0f;
                    if (h33SlotScrim != null) h33SlotScrim.gameObject.SetActive(false);
                }
                if (j38Sub != null && j38Sub.Alpha > 0f) j38Sub.Alpha = 0f; // the entrance card's rule line: the n47-r0f ghost ("RIDE, HOLD ...: SHOOT THE CRACK" again)
            }
            else { seenTeach47 = seenSlot47 = null; if (h33SlotScrim != null && !h33SlotScrim.gameObject.activeSelf) h33SlotScrim.gameObject.SetActive(true); }
            Fight47Tick(frameDt);
            Order64Late(); // NIGHT64 G order64 (UiOrder64.N64G.cs): the frame's last write on the bar's hint (not drawn while the fight line draws the same order); its counter, the row on or off
        }

        /// <summary>R4 (NIGHT47.md section 6's list: "boss names and one line of intro at the walk-in, big and readable"): the entrance card is built
        /// on the first boss (after hud47 sized the HUD, as the bar was), at 56 px with a 20-24 px line (entrance41 sets it each beat): under hud47
        /// its name stands Hud47CardNamePx and its line Hud47CardLinePx (x TEXT SIZE), the name lifted over the larger line. Checked each frame
        /// (cheap compares; entrance41's own per-beat size is overridden after it).</summary>
        void Card47Size()
        {
            if (j38Name == null || j38Sub == null) return;
            float np = UnityTuning.Hud47CardNamePx * textScale47, lp = UnityTuning.Hud47CardLinePx * textScale47;
            if (j38Name.main.fontSize != np)
            {
                j38Name.main.fontSize = j38Name.back.fontSize = np;
                RectTransform nr = j38Name.rt;
                nr.anchoredPosition = new Vector2(nr.anchoredPosition.x, lp + 10);
                nr.sizeDelta = new Vector2(nr.sizeDelta.x, np + 6);
                Outline47(j38Name);
            }
            if (j38Sub.main.fontSize != lp)
            {
                j38Sub.main.fontSize = j38Sub.back.fontSize = lp;
                j38Sub.rt.sizeDelta = new Vector2(j38Sub.rt.sizeDelta.x, lp + 6);
                Outline47(j38Sub);
            }
        }

        /// <summary>Each HUD frame (from Fight47Late): the fight line's queue, hold and fade; the entrance card's place; the reel's forces.</summary>
        void Fight47Tick(double frameDt)
        {
            Force47Tick();
            if (j38Card != null) { var at = new Vector2(0, UnityTuning.Hud47CardY); if (j38Card.anchoredPosition != at) j38Card.anchoredPosition = at; }
            Card47Size();
            if (fight47 == null) return;
            if (finale47Age >= 0) { fight47.Alpha = 0f; return; } // R4 finale47: the boss's name DOWN is the one line while it shows
            bool cardUp = j38Card != null && j38Card.gameObject.activeSelf;
            bool barUp = b43Root != null && b43Root.gameObject.activeSelf;
            if (BossRule61Tick(frameDt, barUp)) return; // NIGHT61 G bossRule61 (UiBossRule61.cs): the boss's rule line holds the fight line at its fight's start (false with the row off: the queue below as NIGHT60)
            if (!barUp) { fight47.Alpha = 0f; fight47Age = -1; fight47Shown = null; fight47Pending = null; fight47Prio = fight47PendingPrio = -1; Hud47Probe.FightLine = ""; return; }
            bool live = fight47Age >= 0;
            bool replace = fight47Pending != null && !cardUp && (!live || fight47PendingPrio >= fight47Prio && fight47Age > UnityTuning.Hud47FightMinS);
            if (replace)
            {
                fight47Shown = fight47Pending;
                fight47Prio = fight47PendingPrio;
                fight47Pending = null;
                fight47PendingPrio = -1;
                fight47.Set(fight47Shown);
                Case47(fight47, fight47Shown); // copy47: a sentence in sentence case
                // the a647 reel's guard (n47-r1e: the shadow twin printed the raw markup): how many text objects draw the line, and whether the
                // text as drawn still holds a tag (a sprite or markup TMP did not parse)
                fight47.main.ForceMeshUpdate();
                Hud47Probe.FightObjects = (fight47.main.enabled ? 1 : 0) + (fight47.back.enabled ? 1 : 0);
                string parsed = fight47.main.GetParsedText() ?? "";
                Hud47Probe.FightRawTag = parsed.IndexOf('<') >= 0 || parsed.IndexOf("sprite", System.StringComparison.OrdinalIgnoreCase) >= 0;
                int words = fight47Shown.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                fight47Hold = UnityTuning.Hud47LineHoldS + UnityTuning.Hud47LineHoldPerWord * words;
                fight47Age = 0;
                Hud47Probe.FightLines++;
            }
            if (fight47Age < 0) { fight47.Alpha = 0f; return; }
            fight47Age += frameDt;
            float a, s;
            if (fight47Age < 0.12) { float k = (float)(fight47Age / 0.12); a = k; s = Mathf.Lerp(1.08f, 1f, k); }
            else if (fight47Age < fight47Hold) { a = 1f; s = 1f; }
            else if (fight47Age < fight47Hold + 0.25) { a = 1f - (float)((fight47Age - fight47Hold) / 0.25); s = 1f; }
            else { a = 0f; s = 1f; fight47Age = -1; fight47Prio = -1; }
            if (cardUp) a = 0f; // the card holds the place: the line waits (it was posted before the card rose)
            fight47.Alpha = a;
            var sc = new Vector3(s, s, 1f);
            if (fight47.rt.localScale != sc) fight47.rt.localScale = sc;
            Hud47Probe.FightLine = a > 0f ? fight47Shown : "";
        }

        /// <summary>The a647 reel's forces: a teach card and a banner at once with a boss bar (Bar43Probe.ForceOn) and its hint.</summary>
        void Force47Tick()
        {
            if (Hud47Probe.ForceTeach != null) { ShowLesson25(Hud47Probe.ForceTeach); Hud47Probe.ForceTeach = null; }
            if (Hud47Probe.ForceBanner != null) { Banner(Hud47Probe.ForceBanner, UiStyle33.Danger, 4.0); Hud47Probe.ForceBanner = null; }
            if (Hud47Probe.ForceTeachOff) { HideLesson25(); Hud47Probe.ForceTeachOff = false; }
        }
    }
}
