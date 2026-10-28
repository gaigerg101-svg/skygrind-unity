// NIGHT35 builder T (design.n35.teach35, NIGHT35.md T1): THE TEACHING TEXT on screen. The sim emits "teach35" (Core Teach35Event: a FORMAT with {action}
// tokens, the pad's wording, the seconds) when a line's trigger fires (SurvivalSim.Teach35.cs); here each line waits its turn in a small queue and rides the
// hud33 MESSAGE SLOT (MessageSlot33) on its teaching channel (Boss: under danger only, over room titles, the door hint and the old lesson line; a boss's
// own teaching line keeps the channel first). It is a LIVE want: the tokens are resolved again when the device, the bindings or the pad layout change while it
// shows (ULTRAKILL's HudMessage reads the live binding; XAG 107: "Remapped bindings update hints"), and it stays `seconds` (5 s) counted from the moment the
// slot shows it, then the next queued line follows. While a teach line shows, the slot drops its all-capitals style so the ULTRAKILL format reads: the verb
// and the weapon in capitals, the rest in sentence case ("HOLD RT to CHARGE the KESTREL. Release to fire."). The slot's text is plain (no rich text), so a
// token resolves to WORDS: the pad's button in its glyph set (Glyphs.Label: RT / R2 / ZR), the mouse's LEFT CLICK / RIGHT CLICK (UnityTuning.Lesson25Aliases),
// the bound key's name. The slot's label is 24 px at 1080p (>= 16). With hud33 off nothing is drawn (the lines are still counted for the reel).
// Off (the row off, --off35 / -skygrindOff35): the sim emits nothing, so nothing here runs.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly List<Teach35Event> t35Queue = new List<Teach35Event>(8);
        Teach35Event t35Now;
        string t35Text;
        int t35Stamp = -1;
        double t35ShownFor, t35Waited;
        bool t35Wired, t35Lower;

        /// <summary>For the reels: the teaching line on screen now ("" none), its id, how many shown / dropped, the ids shown in order, the slot's px while one
        /// showed, and how long the last finished line stood (s).</summary>
        public static string Teach35TextNow = "", Teach35IdNow = "";
        public static int Teach35Shows, Teach35Dropped;
        public static readonly List<string> Teach35ShownIds = new List<string>();
        public static float Teach35PxNow;
        public static double Teach35LastHeldS = -1;

        /// <summary>The longest a queued line may wait for the slot before it is dropped (s): a line about a room long gone teaches nothing.</summary>
        const double Teach35MaxWait = UnityTuning.Teach35MaxWaitS;

        void WireTeach35()
        {
            if (t35Wired || game == null || game.events == null) return;
            t35Wired = true;
            game.events.On<Teach35Event>("teach35", e => { if (e != null && !string.IsNullOrEmpty(e.text)) t35Queue.Add(e); });
            game.OnNewRun += () =>
            {
                t35Queue.Clear();
                t35Now = null;
                t35Text = null;
                Teach35TextNow = Teach35IdNow = "";
            };
        }

        /// <summary>A token's words for the device used last.</summary>
        string Teach35Glyph(string action)
        {
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            if (UnityTuning.Lesson25Aliases.TryGetValue(action, out var pair))
            {
                string a = pad ? pair.pad : pair.keys;
                if (a != null) return a;
            }
            PadLayout layout = game != null && game.padReader != null && game.padReader.layout != null ? game.padReader.layout : PlayerDevices.Layout ?? GamepadMath.DefaultLayoutV2;
            return Glyphs.Label(action, PlayerDevices.Last, PlayerDevices.Glyphs, LiveBindings, layout);
        }

        string Teach35Resolve(Teach35Event e)
        {
            bool pad = PlayerDevices.Last == InputDevice.Pad;
            return Teach35Text.Resolve(Copy47.Map(cfg, pad && !string.IsNullOrEmpty(e.textPad) ? e.textPad : e.text), Teach35Glyph); // NIGHT47 F copy47 (Copy47.cs): the rewrite, off = the line as it is
        }

        /// <summary>The teaching line the slot should want this frame (null: none). `slotNow` is what the slot showed last frame.</summary>
        string Teach35Want(double frameDt, string slotNow)
        {
            WireTeach35();
            if (t35Now == null)
            {
                if (t35Queue.Count == 0) return null;
                t35Now = t35Queue[0];
                t35Queue.RemoveAt(0);
                t35ShownFor = t35Waited = 0;
                t35Stamp = Lesson25StampNow();
                t35Text = Teach35Resolve(t35Now);
            }
            int stamp = Lesson25StampNow();
            if (stamp != t35Stamp)
            {
                t35Stamp = stamp; // the device, the bindings or the layout changed: the same line, read again (a live want ticks in place)
                t35Text = Teach35Resolve(t35Now);
            }
            bool up = slotNow == t35Text && !string.IsNullOrEmpty(t35Text);
            if (up)
            {
                if (t35ShownFor == 0)
                {
                    Teach35Shows++;
                    Teach35ShownIds.Add(t35Now.id);
                    Teach52VerbMark(t35Now); // NIGHT52 U hud52 round 14 (UiTeach52.Verb.cs): the verb this line waits for (nothing with the row off)
                }
                t35ShownFor += frameDt;
                Teach35TextNow = t35Text;
                Teach35IdNow = t35Now.id;
            }
            else if (t35ShownFor == 0) t35Waited += frameDt;
            if (Teach35Over(t35Now, t35ShownFor) || (t35ShownFor == 0 && t35Waited > Teach35MaxWait) || Teach35Kiln40Done()) // NIGHT52 U: Teach35Over = NIGHT51's timer with hud52 off
            {
                if (t35ShownFor > 0) Teach35LastHeldS = t35ShownFor;
                else Teach35Dropped++;
                v52Verb = null; // NIGHT52 U hud52 round 14: the next line marks its own verb
                t35Now = null;
                t35Text = null;
                Teach35TextNow = Teach35IdNow = "";
                return Teach35Want(0, slotNow); // the next one, if any, from this frame
            }
            return t35Text;
        }

        /// <summary>NIGHT40 S1 (kiln40): a line of the boost chain stands until its step is done; the sim then queues the chain's next line (trigger
        /// "kiln40"), which ends the one showing at once. Only kiln40 emits that trigger, so with the row off nothing changes here.</summary>
        bool Teach35Kiln40Done()
        {
            if (t35Now == null || t35Now.trigger != "kiln40") return false;
            for (int i = 0; i < t35Queue.Count; i++) if (t35Queue[i].trigger == "kiln40") return true;
            return false;
        }

        /// <summary>The slot drops its capitals while it shows a teaching line (the format's case reads), and takes them back after.</summary>
        void Teach35Case(string shown)
        {
            bool lower = shown != null && shown == t35Text;
            UiStyle33.Text33 label = Teach35Label; // NIGHT36 M (text36): the teaching slot in the lower middle when the row is on (UiHud36Text.cs)
            if (lower == t35Lower || label == null) return;
            t35Lower = lower;
            FontStyles st = lower ? FontStyles.Normal : FontStyles.UpperCase;
            label.main.fontStyle = st;
            label.back.fontStyle = st;
            if (lower) Teach35PxNow = label.main.fontSize;
        }
    }
}
