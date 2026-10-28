// NIGHT71 builder B (slate B2, design.n71.words71; the rule, the table and the counter: Words71.cs). The HUD's half, two hooks in UiHud33.cs:
//   1. Slot33 calls Words71Slot(ref lesson, ref teach) right after PinHint63Slot: a lesson or teaching line that names a weapon not held waits
//      (the slot is handed null this frame, so text36's slot and teach43's card show it once the weapon is in hand); the line the game without
//      the row would hand the slot is kept for the counter.
//   2. RenderHud33 calls Words71Late() right after FightLine64Late (the frame's last HUD write: Bar43Draw's Set, Fight47Late's queue, order64,
//      bossRule61 and fightLine64 have drawn): the boss bar's order, its fight-line copy and bossRule61's rule line are shown as their variant
//      IN PLACE (Text33.Set on the same holder, its active state, the queue's age, hold and priority untouched, so order64 still sees one order in
//      two places and hides the bar's copy); restored to the owner's text the first frame the row reads off or the weapon is in hand (D1's
//      held off frame shows the off look). An order with no variant waits: the bar's copy at alpha 0 (its own multiplier, restored), the fight
//      line at alpha 0 this frame only (Fight47Tick / BossRule61Tick write its alpha every frame). Then the counter.
// The HUD's look is untouched (GG 2026-10-20: "the UI looks good as it is"): no font, size, colour, place or fade change; only which words a
// holder carries and whether a line waits. The variants' words are config (design.n71.words71.numbers.lines). Allocation-free per frame
// except a variant's resolve (on a change of order, hand, device or row) and the counter's new-line notes.
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly Words71.Count w71 = new Words71.Count();
        bool w71Wired;
        // the slot's lines this frame (the counter's would-be half): the lesson and the teaching line as the game without the row hands them on
        string w71SlotLesson, w71SlotTeach;
        // the bar's order: the owner's raw and text (Bar43Draw's last Set), the variant shown in its place, and whether its alpha is ours
        string w71BarRawSeen, w71BarOwner, w71BarVarFor, w71BarVar;
        int w71BarVarStamp = int.MinValue;
        bool w71BarAlphaMine;
        // the fight line: the owner's text we replaced and the variant we put there
        string w71FightFrom, w71FightTo;
        // the rule line's variant cache
        string w71RuleVarFor, w71RuleVar;
        int w71RuleVarStamp = int.MinValue;

        string W71Held() => game != null && game.sim != null && game.loadout != null ? game.loadout.Weapon : null;

        /// <summary>From Slot33 after PinHint63Slot: the lesson and the teaching line, each held back while it names a weapon not held (row on).</summary>
        void Words71Slot(ref string lesson, ref string teach)
        {
            w71SlotLesson = lesson;
            w71SlotTeach = teach;
            if (!Words71.On(cfg)) return; // every frame (D1's same-run toggle)
            Words71.Table t = Words71.Of(cfg);
            string held = W71Held();
            if (lesson != null && Words71.LessonFor(t, lesson, held) == null) { lesson = null; w71.Waits++; }
            if (teach != null && Words71.LessonFor(t, teach, held) == null) { teach = null; w71.Waits++; }
        }

        /// <summary>A variant's text as the last device reads it (cached: re-resolved when the order, the device stamp or the variant changes).</summary>
        string W71Resolve(string variant, ref string cacheFor, ref string cache, ref int cacheStamp)
        {
            int stamp = Lesson25StampNow();
            if (!ReferenceEquals(variant, cacheFor) || stamp != cacheStamp)
            {
                cacheFor = variant;
                cacheStamp = stamp;
                cache = ResolveLesson25(variant, null) ?? variant;
            }
            return cache;
        }

        /// <summary>Set a holder's words without touching its shown / hidden state (Text33.Set shows a holder on new text).</summary>
        static void W71Set(UiStyle33.Text33 t, string s)
        {
            bool was = t.rt.gameObject.activeSelf;
            t.Set(s);
            if (t.rt.gameObject.activeSelf != was) t.rt.gameObject.SetActive(was);
        }

        /// <summary>From RenderHud33 after FightLine64Late: the boss orders in place, then the counter.</summary>
        void Words71Late()
        {
            Words71Wire();
            bool rowOn = Words71.On(cfg); // every frame (D1's same-run toggle)
            Words71.Table t = Words71.Of(cfg);
            string held = W71Held();
            bool barUp = b43Root != null && b43Hint != null && b43Root.gameObject.activeSelf;

            // ---- the bar's order: its owner's text (a new raw: Bar43Draw just set its resolve) ----
            if (barUp && !ReferenceEquals(b43ShownHintRaw, w71BarRawSeen))
            {
                w71BarRawSeen = b43ShownHintRaw;
                w71BarOwner = b43Hint.Shown;
            }
            bool b72On = Btext72.On(cfg); // NIGHT72 B btext72: its lines (at most 6 words, the verb in capitals, the input's glyph) take the place of words71's variants and of the long raw orders
            string barOrder = barUp && !string.IsNullOrEmpty(w71BarRawSeen)
                ? (b72On ? Btext72.Order(w71BarRawSeen, held) : null) ?? Words71.OrderFor(t, w71BarRawSeen, w71BarOwner, held) : null;
            bool barWait = rowOn && barOrder != null && barOrder.Length == 0;
            string barWant = barUp ? (rowOn && !string.IsNullOrEmpty(barOrder) ? W71Resolve(barOrder, ref w71BarVarFor, ref w71BarVar, ref w71BarVarStamp) : w71BarOwner ?? "") : null;
            if (barUp && barWant != null && b43Hint.Shown != barWant && (barWant.Length > 0 || b43Hint.Shown.Length > 0))
            {
                W71Set(b43Hint, barWant);
                if (!ReferenceEquals(barWant, w71BarOwner)) w71.Variants++;
            }
            if (barUp && barWait != w71BarAlphaMine)
            {
                b43Hint.Alpha = barWait ? 0f : 1f;
                w71BarAlphaMine = barWait;
            }
            else if (!barUp && w71BarAlphaMine && b43Hint != null) { b43Hint.Alpha = 1f; w71BarAlphaMine = false; }
            if (barUp && barWait) w71.Waits++;

            // ---- the fight line: the bar's order there, or bossRule61's rule line ----
            bool fightDrawnOwner = false; // the fight line drawn as the game without the row draws it (for the counter)
            string fightOwnerText = null;
            if (fight47 != null)
            {
                bool drawn = fight47.rt.gameObject.activeInHierarchy && fight47.Alpha > 0f;
                // whose text is up: ours (a variant we put there) or the owner's
                bool ours = w71FightTo != null && fight47Shown == w71FightTo;
                if (!ours) { w71FightFrom = w71FightTo = null; }
                string owner = ours ? w71FightFrom : fight47Shown;
                fightOwnerText = owner;
                fightDrawnOwner = drawn && fight47Prio != 1; // a banner (prio 1: a room's name, a leg line) is not an order
                string want = owner;
                bool wait = false;
                if (rowOn && !string.IsNullOrEmpty(owner) && fight47Prio != 1)
                {
                    string order = null;
                    if (br61.Showing && !string.IsNullOrEmpty(br61Raw)) order = (b72On ? Btext72.Order(br61Raw, held) : null) ?? Words71.OrderFor(t, br61Raw, owner, held);
                    else if (barUp && owner == w71BarOwner && !string.IsNullOrEmpty(w71BarRawSeen)) order = barOrder;
                    else if (Words71.NotHeld(t, owner, held)) order = "";
                    if (order != null && order.Length == 0) wait = true;
                    else if (order != null) want = br61.Showing ? W71Resolve(order, ref w71RuleVarFor, ref w71RuleVar, ref w71RuleVarStamp) : barWant;
                }
                if (!string.IsNullOrEmpty(want) && !string.IsNullOrEmpty(owner) && fight47Shown != want)
                {
                    bool toOwner = want == owner;
                    fight47Shown = want;
                    W71Set(fight47, want);
                    Case47(fight47, want); // copy47's case, as the owner set it
                    if (toOwner) w71FightFrom = w71FightTo = null;
                    else { w71FightFrom = owner; w71FightTo = want; }
                }
                if (wait && fight47.Alpha > 0f) { fight47.Alpha = 0f; w71.Waits++; } // this frame only: its owner writes the alpha every frame
            }

            // ---- the counter: what is drawn now, and what the game without the row would draw ----
            bool held71 = FeelClock.Holding;
            bool any = false, anyWould = false;
            // the bar
            bool barDrawn = barUp && b43Hint.rt.gameObject.activeInHierarchy && b43Hint.Shown.Length > 0;
            bool barNow = barDrawn && b43Hint.Alpha > 0f && Words71.NotHeld(t, b43Hint.Shown, held);
            bool barWould = barDrawn && Words71.NotHeld(t, w71BarOwner, held);
            Count71(Words71.Surface.Bar, barNow, barWould, barNow ? b43Hint.Shown : w71BarOwner, held, held71, ref any, ref anyWould);
            // the fight line
            bool fightNow = fight47 != null && fight47.rt.gameObject.activeInHierarchy && fight47.Alpha > 0f && fight47Prio != 1 && Words71.NotHeld(t, fight47Shown, held);
            bool fightWould = fightDrawnOwner && Words71.NotHeld(t, fightOwnerText, held);
            Count71(Words71.Surface.Fight, fightNow, fightWould, fightNow ? fight47Shown : fightOwnerText, held, held71, ref any, ref anyWould);
            // the teach card (teach43) and the teaching slot (text36): the line on it; would = the slot's lines before the rule
            string teachNow = null;
            if (t43Root != null) { if (t43Root.gameObject.activeSelf && t43CloseAge < 0) teachNow = t43Text; }
            else if (h36Teach != null && h36Teach.Alpha > 0f) teachNow = h36Text;
            bool teachIs = Words71.NotHeld(t, teachNow, held);
            bool slotWould = Words71.NotHeld(t, w71SlotLesson, held) || Words71.NotHeld(t, w71SlotTeach, held);
            Count71(Words71.Surface.Teach, teachIs, slotWould || (!rowOn && teachIs), teachIs ? teachNow : Words71.NotHeld(t, w71SlotLesson, held) ? w71SlotLesson : w71SlotTeach, held, held71, ref any, ref anyWould);
            // the lesson widget (the meta layer; with hud33 on it runs under hud33Hidden's alpha-0 group and its text rides the slot: not drawn)
            bool lessonOnScreen = lesson25Rt != null && lesson25Rt.gameObject.activeInHierarchy && lesson25Group != null && lesson25Group.alpha > 0f
                && (hud33Hidden == null || !lesson25Rt.IsChildOf(hud33Hidden));
            bool lessonNow = lessonOnScreen && Words71.NotHeld(t, lesson25Text != null ? lesson25Text.text : null, held);
            Count71(Words71.Surface.Lesson, lessonNow, lessonNow, lessonNow ? lesson25Text.text : null, held, held71, ref any, ref anyWould);
            w71.Frame(held71, rowOn, any, anyWould);
            Btext72Late(); // NIGHT72 B btext72 (UiBtext72.cs): the frame's last write to the boss text, after words71's: one order at a time, the bar's order 22 px, a plate; its counter, the row on or off
        }

        void Count71(Words71.Surface s, bool now, bool would, string line, string held, bool heldFrame, ref bool any, ref bool anyWould)
        {
            if (now) any = true;
            if (would) anyWould = true;
            if (heldFrame) return;
            w71.SurfaceFrame(s, now, would);
            if (now || would) w71.Note(s, line, held, now);
        }

        /// <summary>Once a session: a new run prints the totals and starts clean; the quit prints what is left.</summary>
        void Words71Wire()
        {
            if (w71Wired || game == null) return;
            w71Wired = true;
            game.OnNewRun += () => { if (w71.Dirty) Words71Say("a new run"); w71.Reset(); };
            Application.quitting += () => { if (w71.Dirty) Words71Say("the quit"); };
        }

        void Words71Say(string why)
        {
            Debug.Log(w71.Line(why, Words71.On(cfg), FeelClock.Now));
            w71.Dirty = false;
        }
    }
}
