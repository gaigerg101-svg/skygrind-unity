// NIGHT71 builder B (leftover L1, design.n71.room71; the rule: Room71.cs; the run's half: Room71Runner.cs). The level select's half, two hooks in
// UiLevelSelect26.cs: RenderLevelSelect26 calls Room71Select() each frame the select is open (left / right on the focused level's row pick
// where its run starts; the select's own hint label names the pick), and PressLevel26 calls Room71Press(id) as a level starts (the pick
// becomes Room71.PendingRoom, which outlives the scene reload). Left / right: the pad's menu side (PlayerDevices.Menu, its repeat edge) or the
// keyboard's arrows / A / D (this frame's press). Row off: the hint label's own words, nothing picked, nothing pending.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        string r71HintBase;
        bool r71WasOn;
        string r71ShownFor;
        int r71ShownPick = -2;
        readonly Dictionary<string, int> r71Pick = new Dictionary<string, int>();
        readonly Dictionary<string, List<KeyValuePair<string, string>>> r71Rooms = new Dictionary<string, List<KeyValuePair<string, string>>>();

        /// <summary>The level's rooms a run may start in (read once a level from its file; empty when it has none or the file cannot be read).</summary>
        List<KeyValuePair<string, string>> Rooms71(string id)
        {
            if (id == null) return new List<KeyValuePair<string, string>>();
            if (r71Rooms.TryGetValue(id, out var list)) return list;
            list = new List<KeyValuePair<string, string>>();
            try
            {
                if (TryMap26(id, out SurvivalGame.MapEntry me) && !string.IsNullOrEmpty(me.file))
                    list = Room71.RoomsOf(Skygrind.Core.PortData.LoadJson(me.file) as Dictionary<string, object>);
            }
            catch (System.Exception e) { Debug.Log("[room71] the rooms of " + id + " could not be read: " + e.Message); }
            r71Rooms[id] = list;
            return list;
        }

        /// <summary>From RenderLevelSelect26 while the select is open: the pick for the focused level, and the hint label's words.</summary>
        void Room71Select()
        {
            if (levelHint26 == null) return;
            if (r71HintBase == null) r71HintBase = levelHint26.text;
            if (!Room71.On(cfg))
            {
                if (r71WasOn) { levelHint26.text = r71HintBase; r71WasOn = false; r71ShownFor = null; r71ShownPick = -2; }
                return;
            }
            r71WasOn = true;
            int c = CursorRow26();
            if (levelSlots26 == null || c < 0 || c >= levelSlots26.Count || !levelSlots26[c].playable)
            {
                if (r71ShownFor != null) { levelHint26.text = r71HintBase; r71ShownFor = null; r71ShownPick = -2; }
                return;
            }
            string id = levelSlots26[c].id;
            List<KeyValuePair<string, string>> rooms = Rooms71(id);
            int pick = r71Pick.TryGetValue(id, out int p) ? p : -1;
            Keyboard kb = Keyboard.current;
            MenuInput mi = PlayerDevices.Menu;
            bool left = mi.left || (kb != null && (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame));
            bool right = mi.right || (kb != null && (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame));
            if (left) pick = Room71.Step(pick, -1, rooms.Count);
            if (right) pick = Room71.Step(pick, 1, rooms.Count);
            if (pick >= rooms.Count) pick = -1;
            r71Pick[id] = pick;
            if (id == r71ShownFor && pick == r71ShownPick) return;
            r71ShownFor = id;
            r71ShownPick = pick;
            levelHint26.text = Room71.Hint(r71HintBase, pick < 0 ? null : rooms[pick].Key, pick < 0 ? null : rooms[pick].Value);
        }

        /// <summary>From PressLevel26 as level `id` starts: the run starts in its picked room (row on), else at the level's start.</summary>
        void Room71Press(string id)
        {
            Room71.PendingRoom = null;
            if (id == null || !Room71.On(cfg) || !r71Pick.TryGetValue(id, out int pick) || pick < 0) return;
            List<KeyValuePair<string, string>> rooms = Rooms71(id);
            if (pick >= rooms.Count) return;
            Room71.PendingRoom = rooms[pick].Key;
            Debug.Log("[room71] level select: " + id + " starts at room " + rooms[pick].Key + " '" + rooms[pick].Value + "' (DOORFREE on: unranked)");
        }
    }
}
