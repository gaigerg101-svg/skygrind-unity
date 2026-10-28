// NIGHT63 builder G (leftover 2: the pinned hint; PinHint63.cs holds the switch and the feed): the HUD's half. One hook line in UiHud33.cs Slot33,
// after the teaching and lesson lines are chosen: `PinHint63Slot(ref lesson, ref bossLine, ref teach);`. Without -skygrindPinHint63 and
// -skygrindReelOnly (normal play, and every reel without the switch) it returns at once and changes nothing. With them: the teaching slot is fed
// the pinned line (the config's text through copy47 / brief55's teachMap and the lesson widget's resolver, as a lesson's line is) from each room's
// entry, after its gap, and no teach35 or boss line; the sim's own lines are hidden from the HUD for the whole reel (counted). Proof (L-089):
//   [pin63] hint pinned: <n> line(s) "<raw>" -> "<shown>" | the sim's lesson and teach lines are not drawn this reel (map <id>)   once, at the first frame
//   [pin63] room <id> entered: "<shown>" after <gap> empty frame(s) | sim lines hidden so far <k> frames, fed <f> frames             each room entry
//   [pin63] text: blocks <n> | title "<the top slot>" | card "<the teach card's head>" | clear "<the ROOM CLEAR card's head>"         each change
// The last is the meter's input (Tools/night/n63-p3-blocks.cjs): whenever the centre text's blocks change (the top slot's line on or off, the teach
// card's line, the ROOM CLEAR card on or off), read as last HUD frame drew them (this runs before the slot and the card draw this frame); the
// Player log's order puts each a157 "entry" line after the state its frame showed (one frame's slack: a change on the shot's own frame is
// logged after it).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT63 G (the pinned hint): what the pin did (a reel may print it).</summary>
    public static class PinHint63Probe
    {
        public static bool On;
        public static int Rooms, Fed, SimHidden;
        public static string Shown = "";
    }

    public sealed partial class Ui
    {
        PinHint63Feed p63Feed;
        bool p63Read;

        /// <summary>From UiHud33.Slot33 (every HUD frame): with the reel's pin, the teaching slot's lesson line is the pinned one (null in the gaps
        /// and before the first room), its teach line none, no boss line. Without it: nothing (the arguments are read once).</summary>
        void PinHint63Slot(ref string lesson, ref bool bossLine, ref string teach)
        {
            if (!p63Read)
            {
                p63Read = true;
                string[] args;
                try { args = System.Environment.GetCommandLineArgs(); }
                catch (System.Exception) { args = null; }
                string[] lines = PinHint63.Parse(args);
                if (lines == null) return;
                p63Feed = new PinHint63Feed(lines);
                PinHint63Probe.On = true;
                if (game.events != null)
                    game.events.On<RoomEvent>("roomEnter", e =>
                    {
                        p63Feed.RoomEnter();
                        PinHint63Probe.Rooms = p63Feed.Rooms;
                        Debug.Log($"[pin63] room {(e != null ? e.id : "?")} entered: \"{PinHint63Shown(p63Feed.Current)}\" after {Mathf.Max(1, UnityTuning.PinHint63GapFrames)} empty frame(s) | sim lines hidden so far {PinHint63Probe.SimHidden} frames, fed {p63Feed.Fed} frames");
                    });
                Debug.Log($"[pin63] hint pinned: {lines.Length} line(s) \"{string.Join(" | ", lines)}\" -> \"{PinHint63Shown(lines[0])}\" | the sim's lesson and teach lines are not drawn this reel (map {game.MapId})");
            }
            if (p63Feed == null) return;
            PinHint63Text();
            if (!string.IsNullOrEmpty(lesson) || !string.IsNullOrEmpty(teach)) PinHint63Probe.SimHidden++;
            string raw = p63Feed.Line();
            lesson = raw != null ? PinHint63Shown(raw) : null;
            teach = null;
            bossLine = false;
            PinHint63Probe.Fed = p63Feed.Fed;
            PinHint63Probe.Shown = lesson ?? "";
        }

        string p63TextKey;

        /// <summary>The meter's line: the centre text's blocks as last frame drew them, logged on each change (the top slot on / off, the card's
        /// line, the ROOM CLEAR card on / off; the title's own letters typing in are not a change).</summary>
        void PinHint63Text()
        {
            string title = Hud33SlotNow ?? "";
            string card = Teach43Probe.Shown ? Teach43Probe.Head ?? "" : "";
            bool clearUp = c55Root != null && c55Root.gameObject.activeSelf && (c55Group == null || c55Group.alpha > 0f);
            string clear = clearUp && c55Head != null && c55Head.main != null ? c55Head.main.text ?? "" : "";
            string key = (title.Length > 0 ? "T" : "-") + "\u0001" + card + "\u0001" + (clearUp ? "C" : "-");
            if (key == p63TextKey) return;
            p63TextKey = key;
            int blocks = (title.Length > 0 ? 1 : 0) + (card.Length > 0 ? 1 : 0) + (clearUp ? 1 : 0);
            Debug.Log($"[pin63] text: blocks {blocks} | title \"{title}\" | card \"{card}\" | clear \"{clear}\"");
        }

        /// <summary>A pinned line as the lesson widget would hold it: copy47's map (brief55's teachMap first), then the bound buttons.</summary>
        string PinHint63Shown(string raw) => string.IsNullOrEmpty(raw) ? "" : ResolveLesson25(Copy47.Map(cfg, raw), null) ?? raw;
    }
}
