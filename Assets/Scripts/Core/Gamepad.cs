// Port of skygrind/src/input/gamepad.js (OVERNIGHT3 P3, docs/GAMEPAD.md): the gamepad -> the same InputFrame fields the
// keyboard and mouse fill. Pure: every function takes a Gamepad-like object ({buttons: [{pressed, value}], axes: [..],
// mapping}) and plain settings, and returns numbers and action names; input/keyboardMouse.js polls
// navigator.getGamepads() once a frame and merges the result into its frame, so the Mover, the bot and every system
// downstream can't tell a pad from a keyboard.
// Standard mapping only (https://w3c.github.io/gamepad/#remapping): buttons 0-16, axes 0-3. A pad that reports another
// mapping gets a note and still reads through the same table (often close enough to rebind around).
// NIGHT13 U3-26.7 (Unity): the Gamepad-like object is a PadState (17 {pressed, value} buttons, 4 axes) that
// Game/Input/PadReader.cs fills from the Input System once a frame (raw sticks, y negated: up is -1 here as on the web).
// The web's Sets of action names are bitmasks over PadAction (no Set built per poll, PORT-RULES section 6); where the
// web's Set order matters (the press edges, run in the layout's order), an ordered buffer the caller owns carries it.
// A layout is a PadLayout: the web object's key order kept (heldActions' order, buttonFor's first match, padRows' rows).
// The web's InputFrame holds the combat buttons too; Unity splits them (InputFrame + CombatInput), so mergePad writes
// both (MergeMove / MergeCombat: one per sample). The web's module constants (PAD_BUTTONS, DEFAULT_PAD_LAYOUT,
// FRAME_BUTTONS, PAD_GLYPHS, TRIGGER_DOWN, PAD_ACTION_LABELS, REQUIRED) are the cited constants below.
using System;
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>Standard-mapping button indices by name (gamepad.js PAD_BUTTONS :9-11). Index 16 (home) has no name.</summary>
    public enum PadButtonName
    {
        A = 0, B = 1, X = 2, Y = 3, LB = 4, RB = 5, LT = 6, RT = 7, Select = 8, Start = 9, LS = 10, RS = 11, Up = 12, Down = 13, Left = 14, Right = 15,
    }

    /// <summary>
    /// The game actions a pad button carries (gamepad.js :13-18): the InputFrame buttons (FRAME_BUTTONS :39, in its order)
    /// then the commands (nextWeapon, prevWeapon, quickSwap, lockOn, xray, perkList, pause). One bit each in a mask.
    /// </summary>
    public enum PadAction
    {
        Jump, Dash, Slide, Fire, AltFire, Special, Punch, Interact, Grapple, Reel,
        NextWeapon, PrevWeapon, QuickSwap, LockOn, Xray, PerkList, Pause,
        Wheel, // NIGHT15 U3-28.8 (Unity-only): the radial weapon wheel on the pad (held). Not in the web's default layout.
        Sprint, // NIGHT19 (Unity-only, the shared commit): the sprint's own key (InputFrame.sprint; L3 in the v2 layout). A frame button outside the web's first 10.
    }

    /// <summary>One standard button ({pressed, value}: the triggers report 0..1).</summary>
    public struct PadButton
    {
        public bool pressed;
        public double value;
    }

    /// <summary>A Gamepad-like snapshot (gamepad.js :1-6): buttons 0-16 and axes 0-3 (lx, ly, rx, ry; up is -1). Refilled every frame.</summary>
    public sealed class PadState
    {
        public const int ButtonCount = 17, AxisCount = 4;
        public readonly PadButton[] buttons = new PadButton[ButtonCount];
        public readonly double[] axes = new double[AxisCount];

        public void Clear()
        {
            Array.Clear(buttons, 0, ButtonCount);
            Array.Clear(axes, 0, AxisCount);
        }

        public void Set(PadButtonName b, bool pressed, double value) => buttons[(int)b] = new PadButton { pressed = pressed, value = value };
    }

    /// <summary>readSticks' settings: rates in rad/s at full tilt and x1 sensitivity.</summary>
    public struct StickSettings
    {
        public double inner, outer, sensX, sensY, yawRate, pitchRate;
        public bool precise, invertY; // precise: the 'precise' curve (else 'linear')
    }

    /// <summary>readSticks' result: the pad's move axes and look rates for one poll.</summary>
    public struct Sticks
    {
        public double moveX, moveZ, yawRate, pitchRate;
    }

    /// <summary>menuNav's result: move -1 up, +1 down; side -1 left, +1 right; the press edges of A, B and START.</summary>
    public struct PadMenu
    {
        public int move, side;
        public bool confirm, back, start;

        /// <summary>NIGHT19 U3-33.3 (Unity-only): LB's (-1) or RB's (+1) press edge: the previous / next tab (0: neither, or both).</summary>
        public int tab;
    }

    /// <summary>createMenuNavState: the held direction, its time, the last buttons (bits: 1 A, 2 B, 4 START; NIGHT19: 8 LB, 16 RB).</summary>
    public sealed class MenuNavState
    {
        public int dir;
        public double hold;
        public int prev;
    }

    /// <summary>
    /// mergePad's pad (keyboardMouse.js's pad {moveX, moveZ, held, latched}): the move axes, the held actions and the
    /// latched presses as PadAction masks. The reader writes it once a frame; each step's sample merges it and clears the
    /// latches it consumed.
    /// </summary>
    public sealed class PadFrame
    {
        public double moveX, moveZ;
        public uint held, latched;
    }

    /// <summary>rebindPad's result: the layout (a copy, or the input when refused), ok, and the button the old actions moved to.</summary>
    public struct PadRebind
    {
        public PadLayout layout;
        public bool ok;
        public PadButtonName? swapped;
    }

    /// <summary>
    /// A pad layout: pad button -> game action(s) (a gamepad.js layout object), in the object's key order. Immutable: the
    /// web's `{ ...layout, b: [...] }` is With, rebindPad builds a new one.
    /// </summary>
    public sealed class PadLayout
    {
        readonly PadButtonName[] order;
        readonly PadAction[][] actions = new PadAction[16][]; // null: the button is not in the table
        readonly uint[] masks = new uint[16];

        public PadLayout(IList<(PadButtonName button, PadAction[] actions)> entries)
        {
            var keys = new List<PadButtonName>(entries.Count);
            foreach (var (button, acts) in entries)
            {
                int b = (int)button;
                if (actions[b] == null) keys.Add(button); // a repeated key keeps its first position (a JS object)
                actions[b] = (PadAction[])acts.Clone();
                uint m = 0;
                foreach (PadAction a in acts) m |= GamepadMath.Bit(a);
                masks[b] = m;
            }
            order = keys.ToArray();
        }

        /// <summary>The number of buttons in the table.</summary>
        public int Count => order.Length;

        /// <summary>The i-th button in key order.</summary>
        public PadButtonName this[int i] => order[i];

        public bool Has(PadButtonName b) => actions[(int)b] != null;

        /// <summary>The button's actions in order (empty when the button is not in the table).</summary>
        public IReadOnlyList<PadAction> ActionsOf(PadButtonName b) => actions[(int)b] ?? Array.Empty<PadAction>();

        internal PadAction[] Raw(PadButtonName b) => actions[(int)b];

        /// <summary>The button's actions as a mask (0 when it is not in the table).</summary>
        public uint MaskOf(PadButtonName b) => masks[(int)b];

        /// <summary>`{ ...layout, [b]: acts }`: a copy with button b's actions replaced (a new button goes last).</summary>
        public PadLayout With(PadButtonName b, params PadAction[] acts)
        {
            var entries = Entries();
            int i = entries.FindIndex(e => e.button == b);
            if (i >= 0) entries[i] = (b, acts);
            else entries.Add((b, acts));
            return new PadLayout(entries);
        }

        /// <summary>The table's entries in key order (copies).</summary>
        public List<(PadButtonName button, PadAction[] actions)> Entries()
        {
            var list = new List<(PadButtonName, PadAction[])>(order.Length);
            foreach (PadButtonName b in order) list.Add((b, (PadAction[])actions[(int)b].Clone()));
            return list;
        }

        /// <summary>vitest toEqual: the same buttons (in any order) with the same action lists (in order).</summary>
        public bool SameAs(PadLayout other)
        {
            if (other == null || other.Count != Count) return false;
            for (int b = 0; b < 16; b++)
            {
                PadAction[] x = actions[b], y = other.actions[b];
                if ((x == null) != (y == null)) return false;
                if (x == null) continue;
                if (x.Length != y.Length) return false;
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }
            }
            return true;
        }

        /// <summary>JSON.stringify(layout): {"rt":["fire"],...} in key order (the stored pad layout).</summary>
        public string ToJson()
        {
            var sb = new StringBuilder("{");
            for (int i = 0; i < order.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('"').Append(GamepadMath.ButtonNames[(int)order[i]]).Append("\":[");
                PadAction[] acts = actions[(int)order[i]];
                for (int j = 0; j < acts.Length; j++)
                {
                    if (j > 0) sb.Append(',');
                    sb.Append('"').Append(GamepadMath.ActionNames[(int)acts[j]]).Append('"');
                }
                sb.Append(']');
            }
            return sb.Append('}').ToString();
        }
    }

    public static class GamepadMath
    {
        /// <summary>PAD_BUTTONS' names by index (gamepad.js :9-11; the stored layout's keys).</summary>
        public static readonly string[] ButtonNames =
        {
            "a", "b", "x", "y", "lb", "rb", "lt", "rt", "select", "start", "ls", "rs", "up", "down", "left", "right",
        };

        /// <summary>The web's action names by PadAction (the stored layout's values; PAD_ACTION_LABELS' keys).</summary>
        public static readonly string[] ActionNames =
        {
            "jump", "dash", "slide", "fire", "altFire", "special", "punch", "interact", "grapple", "reel",
            "nextWeapon", "prevWeapon", "quickSwap", "lockOn", "xray", "perkList", "pause",
            "weaponWheel", // NIGHT15 (Unity-only; a saved layout may bind it)
            "sprint", // NIGHT19 (Unity-only; the v2 layout binds it to L3)
        };

        public const int ActionCount = 19; // NIGHT15: the web's 17 + Wheel; NIGHT19: + Sprint

        public static uint Bit(PadAction a) => 1u << (int)a;

        /// <summary>The web's frame buttons, jump .. reel (FRAME_BUTTONS :38-39): every one must stay bound (Required).</summary>
        public const uint FrameButtonsWeb = (1u << 10) - 1; // jump .. reel

        /// <summary>Pad buttons that are InputFrame buttons (held + pressed); the rest are commands (pressed edges only, perkList holds). FRAME_BUTTONS :38-39, + NIGHT19's Sprint (not required: a v1 layout may leave it unbound).</summary>
        public const uint FrameButtons = FrameButtonsWeb | (1u << (int)PadAction.Sprint);

        /// <summary>The frame buttons Unity's InputFrame holds (the movement ones) and the ones its CombatInput holds.</summary>
        public static readonly uint MoveButtons = Bit(PadAction.Jump) | Bit(PadAction.Dash) | Bit(PadAction.Slide) | Bit(PadAction.Grapple) | Bit(PadAction.Reel) | Bit(PadAction.Sprint);

        public static readonly uint CombatButtons = Bit(PadAction.Fire) | Bit(PadAction.AltFire) | Bit(PadAction.Special) | Bit(PadAction.Punch) | Bit(PadAction.Interact);

        /// <summary>
        /// The default layout: pad button -> game action(s). EDIT HERE (docs/GAMEPAD.md mirrors it). Actions are the InputFrame
        /// buttons (jump, dash, slide, fire, altFire, special, punch, interact, grapple, reel) or the commands below
        /// (nextWeapon, prevWeapon, quickSwap, lockOn, xray, perkList, pause). One button may carry two actions that never
        /// matter at the same time (X: buy at a machine, reel in on a swing). (DEFAULT_PAD_LAYOUT :19-36)
        /// </summary>
        public static readonly PadLayout DefaultLayout = new PadLayout(new List<(PadButtonName, PadAction[])>
        {
            (PadButtonName.RT, new[] { PadAction.Fire }),
            (PadButtonName.LT, new[] { PadAction.AltFire }), // zoom on the zoom guns, the special on the rest (right mouse)
            (PadButtonName.A, new[] { PadAction.Jump }),
            (PadButtonName.B, new[] { PadAction.Slide }), // slide on the ground, slam in the air
            (PadButtonName.X, new[] { PadAction.Interact, PadAction.Reel }),
            (PadButtonName.Y, new[] { PadAction.NextWeapon }),
            (PadButtonName.LB, new[] { PadAction.Dash }),
            (PadButtonName.RB, new[] { PadAction.Punch }), // punch / parry
            (PadButtonName.LS, new[] { PadAction.Grapple }), // ARCLINE (hold; SETTINGS > ARCLINE: TOGGLE makes it a click)
            (PadButtonName.RS, new[] { PadAction.LockOn }),
            (PadButtonName.Up, new[] { PadAction.Special }),
            (PadButtonName.Down, new[] { PadAction.Xray }),
            (PadButtonName.Left, new[] { PadAction.PrevWeapon }),
            (PadButtonName.Right, new[] { PadAction.QuickSwap }),
            (PadButtonName.Select, new[] { PadAction.PerkList }),
            (PadButtonName.Start, new[] { PadAction.Pause }),
        });

        // --- NIGHT19 U3-33.3 (builder P; Unity-only, design.pad.n19 = GG's PAD LAYOUT switch "shooter") -----------------
        // The web has one layout and no sprint key. v2 moves four buttons and keeps the rest (NIGHT19.md section 2):
        // L3 SPRINT (the shooter's stick click; L3 SPRINT TOGGLE / HOLD is the input layer's: SprintLatch below), R3 PUNCH /
        // PARRY (the quick melee: Combat's punch, unchanged), RB SPECIAL + ARCLINE (the grapple must stay bound: REQUIRED; the
        // rope is off in Survival, as X carries INTERACT + REEL), d-pad up LOCK-ON; LB stays the boost (DASH). The v1 key
        // order is kept (CONTROLS' and padRows' rows). The store is skygrind.padLayout.v2 ({"version":2,"layout":{..}}), the
        // v1 string kept for a rollback (MigrateV1).

        /// <summary>NIGHT19: the v2 default (design.pad.n19's "shooter" layout).</summary>
        public static readonly PadLayout DefaultLayoutV2 = new PadLayout(new List<(PadButtonName, PadAction[])>
        {
            (PadButtonName.RT, new[] { PadAction.Fire }),
            (PadButtonName.LT, new[] { PadAction.AltFire }), // the gust on the KESTREL (design.gust), the alt-fire on the rest
            (PadButtonName.A, new[] { PadAction.Jump }),
            (PadButtonName.B, new[] { PadAction.Slide }),
            (PadButtonName.X, new[] { PadAction.Interact, PadAction.Reel }),
            (PadButtonName.Y, new[] { PadAction.NextWeapon }),
            (PadButtonName.LB, new[] { PadAction.Dash }), // the boost (a hold after it sprints, as NIGHT18)
            (PadButtonName.RB, new[] { PadAction.Special, PadAction.Grapple }),
            (PadButtonName.LS, new[] { PadAction.Sprint }),
            (PadButtonName.RS, new[] { PadAction.Punch }), // punch / parry
            (PadButtonName.Up, new[] { PadAction.LockOn }),
            (PadButtonName.Down, new[] { PadAction.Xray }),
            (PadButtonName.Left, new[] { PadAction.PrevWeapon }),
            (PadButtonName.Right, new[] { PadAction.QuickSwap }),
            (PadButtonName.Select, new[] { PadAction.PerkList }),
            (PadButtonName.Start, new[] { PadAction.Pause }),
        });

        /// <summary>NIGHT19: the version field of the v2 store.</summary>
        public const int PadLayoutVersion = 2;

        /// <summary>NIGHT19: the default for the block: v2 with design.pad.n19 on, the web's (v1) with it off.</summary>
        public static PadLayout DefaultFor(bool v2) => v2 ? DefaultLayoutV2 : DefaultLayout;

        /// <summary>NIGHT19: the v2 store's JSON, {"version":2,"layout":{..}} (the layout as ToJson writes it).</summary>
        public static string PadLayoutV2Json(PadLayout layout) => "{\"version\":" + PadLayoutVersion + ",\"layout\":" + layout.ToJson() + "}";

        /// <summary>
        /// NIGHT19: the pad layout with design.pad.n19 on, from the two stored strings: a v2 store with its version wins; a
        /// missing, corrupt or other-version v2 reads as missing, and then a stored v1 is migrated (MigrateV1; `migrated`:
        /// the caller writes the result as v2 and keeps the v1 string); with neither, the v2 default (nothing to write).
        /// </summary>
        public static PadLayout LoadPadLayoutV2(string v2Raw, string v1Raw, out bool migrated)
        {
            migrated = false;
            PadLayout v2 = ReadPadLayoutV2(v2Raw);
            if (v2 != null) return v2;
            if (v1Raw == null) return DefaultLayoutV2;
            migrated = true;
            return MigrateV1(ReadPadLayout(v1Raw));
        }

        /// <summary>The v2 store's layout, or null when it's missing, not an object, lacks version 2 or its layout; a layout
        /// that leaves a REQUIRED action unbound is the v2 default (ReadPadLayout's rule).</summary>
        static PadLayout ReadPadLayoutV2(string raw)
        {
            if (raw == null) return null;
            try
            {
                if (!(JsJson.Parse(raw) is Dictionary<string, object> obj)) return null;
                if (!obj.TryGetValue("version", out object ver) || !(ver is double d) || d != PadLayoutVersion) return null;
                if (!obj.TryGetValue("layout", out object layout) || !(layout is Dictionary<string, object> l)) return null;
                return FromObject(l, DefaultLayoutV2);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// NIGHT19: a v1 layout (the player's NIGHT18 remap, or the v1 default) as v2. Equal to the v1 default: the v2
        /// default. Customised: its buttons kept; R3 still holding the v1 default's LOCK-ON takes PUNCH (PUNCH and LOCK-ON
        /// trade places: the button that carried PUNCH carries LOCK-ON); L3 still holding the v1 default's ARCLINE takes
        /// SPRINT (the ARCLINE joins the button that carries SPECIAL, as the v2 default's RB). Otherwise SPRINT stays unbound
        /// (CONTROLS: UNBOUND) and PUNCH stays where the player put it. REQUIRED stays bound either way.
        /// </summary>
        public static PadLayout MigrateV1(PadLayout v1)
        {
            if (v1.SameAs(DefaultLayout)) return DefaultLayoutV2;
            List<(PadButtonName button, PadAction[] actions)> e = v1.Entries();
            int rs = e.FindIndex(x => x.button == PadButtonName.RS);
            if (rs >= 0 && Only(e[rs].actions, PadAction.LockOn))
            {
                for (int i = 0; i < e.Count; i++)
                {
                    PadAction[] acts = e[i].actions;
                    for (int j = 0; j < acts.Length; j++)
                        if (acts[j] == PadAction.Punch) acts[j] = PadAction.LockOn;
                }
                e[rs] = (PadButtonName.RS, new[] { PadAction.Punch });
            }
            int ls = e.FindIndex(x => x.button == PadButtonName.LS);
            if (ls >= 0 && Only(e[ls].actions, PadAction.Grapple) && ButtonFor(PadAction.Sprint, v1) == null)
            {
                e[ls] = (PadButtonName.LS, new[] { PadAction.Sprint });
                int sp = e.FindIndex(x => Array.IndexOf(x.actions, PadAction.Special) >= 0);
                if (sp >= 0)
                {
                    var acts = new PadAction[e[sp].actions.Length + 1];
                    e[sp].actions.CopyTo(acts, 0);
                    acts[acts.Length - 1] = PadAction.Grapple;
                    e[sp] = (e[sp].button, acts);
                }
                else e[ls] = (PadButtonName.LS, new[] { PadAction.Sprint, PadAction.Grapple }); // (SPECIAL is REQUIRED: never reached)
            }
            return new PadLayout(e);
        }

        static bool Only(PadAction[] acts, PadAction a) => acts.Length == 1 && acts[0] == a;

        // --- NIGHT20 U3-34.4 (builder P; Unity-only, design.pad.n20 = GG's PAD switch "RB = NEXT WEAPON") -----------------
        // GG 2026-10-06: "the right bumper could be used to switch weapons as well, just scroll through weapons". The web has
        // no RB cycle (gamepad.js: RB punches). v3 is v2 with two rows swapped: RB NEXT WEAPON (a tap scrolls; held
        // design.pad.n20.wheelHold it opens the wheel: PadReader's NEXT WEAPON hold, whatever button carries it), Y SPECIAL +
        // ARCLINE (the grapple must stay bound: REQUIRED; the rope is off in Survival). The key order is v2's (v1's). The
        // store is skygrind.padLayout.v3 ({"version":3,"layout":{..}}); with no v3, the v2 store (itself a migrated v1 when
        // there's no v2) is migrated once (MigrateV2) and written as v3, the v2 and v1 strings kept for a rollback.

        /// <summary>NIGHT20: the v3 default (design.pad.n20): v2 with RB and Y's rows swapped.</summary>
        public static readonly PadLayout DefaultLayoutV3 = SwapRbY(DefaultLayoutV2);

        /// <summary>NIGHT20: the version field of the v3 store.</summary>
        public const int PadLayoutVersion3 = 3;

        /// <summary>NIGHT20: the default for a layout version (1 the web's, 2 NIGHT19's v2, 3 NIGHT20's v3).</summary>
        public static PadLayout DefaultForVersion(int version) => version >= 5 ? DefaultLayoutV5 : version == 4 ? DefaultLayoutV4 : version == 3 ? DefaultLayoutV3 : version == 2 ? DefaultLayoutV2 : DefaultLayout;

        /// <summary>NIGHT20: the v3 store's JSON, {"version":3,"layout":{..}} (the layout as ToJson writes it).</summary>
        public static string PadLayoutV3Json(PadLayout layout) => "{\"version\":" + PadLayoutVersion3 + ",\"layout\":" + layout.ToJson() + "}";

        /// <summary>
        /// NIGHT20: the pad layout with design.pad.n20 on, from the three stored strings: a v3 store with its version wins; a
        /// missing, corrupt or other-version v3 reads as missing, and then the v2 store (LoadPadLayoutV2: a stored v1 migrated
        /// when there's no v2) is migrated (MigrateV2; `migrated`: something was stored, so the caller writes the result as v3
        /// and keeps the older strings); with nothing stored, the v3 default (nothing to write).
        /// </summary>
        public static PadLayout LoadPadLayoutV3(string v3Raw, string v2Raw, string v1Raw, out bool migrated)
        {
            migrated = false;
            PadLayout v3 = ReadPadLayoutVersion(v3Raw, PadLayoutVersion3, DefaultLayoutV3);
            if (v3 != null) return v3;
            if (v2Raw == null && v1Raw == null) return DefaultLayoutV3;
            migrated = true;
            return MigrateV2(LoadPadLayoutV2(v2Raw, v1Raw, out _));
        }

        /// <summary>
        /// NIGHT20: a v2 layout (the player's NIGHT19 remap, or the v2 default) as v3, by the v1 -> v2 rule (MigrateV1). Equal
        /// to the v2 default: the v3 default. Customised: its buttons kept, except that RB and Y trade their actions when both
        /// still hold the v2 default's (RB SPECIAL + ARCLINE, Y NEXT WEAPON: the player never touched them). A v2 whose RB or Y
        /// holds the player's own pick stays as it is (NEXT WEAPON where the player put it).
        /// </summary>
        public static PadLayout MigrateV2(PadLayout v2)
        {
            if (v2.SameAs(DefaultLayoutV2)) return DefaultLayoutV3;
            PadAction[] rb = v2.Raw(PadButtonName.RB), y = v2.Raw(PadButtonName.Y);
            if (!SameActs(rb, DefaultLayoutV2.Raw(PadButtonName.RB)) || !SameActs(y, DefaultLayoutV2.Raw(PadButtonName.Y))) return v2;
            return SwapRbY(v2);
        }

        // --- NIGHT24 (builder R; Unity-only, design.n24.padY = GG 2026-10-08: "the Y ... should also be able to switch weapons") -----
        // v4 puts the two rows back: Y NEXT WEAPON (a tap scrolls; held design.pad.n20.wheelHold it opens the wheel: PadReader's NEXT
        // WEAPON hold, whatever button carries it), RB SPECIAL + ARCLINE (the grapple stays bound: REQUIRED), LB DASH as in every
        // default layout, so the old RB scroll is dropped. The store is skygrind.padLayout.v4 ({"version":4,"layout":{..}}); with no
        // v4, the v3 store (and, under it, v2 / v1) is migrated once (MigrateV3) and written as v4, the older strings kept for a
        // rollback (--off24 reads v3 again).

        /// <summary>NIGHT24: the v4 default (design.n24.padY): v3 with RB and Y's rows swapped back.</summary>
        public static readonly PadLayout DefaultLayoutV4 = SwapRbY(DefaultLayoutV3);

        /// <summary>NIGHT24: the version field of the v4 store.</summary>
        public const int PadLayoutVersion4 = 4;

        /// <summary>NIGHT24: the v4 store's JSON, {"version":4,"layout":{..}}.</summary>
        public static string PadLayoutV4Json(PadLayout layout) => "{\"version\":" + PadLayoutVersion4 + ",\"layout\":" + layout.ToJson() + "}";

        /// <summary>
        /// NIGHT24: the pad layout with design.n24.padY on, from the four stored strings: a v4 store with its version wins; a
        /// missing, corrupt or other-version v4 reads as missing, and then the v3 store (LoadPadLayoutV3: v2 / v1 under it) is
        /// migrated (MigrateV3; `migrated`: something was stored, so the caller writes the result as v4 and keeps the older
        /// strings); with nothing stored, the v4 default (nothing to write).
        /// </summary>
        public static PadLayout LoadPadLayoutV4(string v4Raw, string v3Raw, string v2Raw, string v1Raw, out bool migrated)
        {
            migrated = false;
            PadLayout v4 = ReadPadLayoutVersion(v4Raw, PadLayoutVersion4, DefaultLayoutV4);
            if (v4 != null) return v4;
            if (v3Raw == null && v2Raw == null && v1Raw == null) return DefaultLayoutV4;
            migrated = true;
            return MigrateV3(LoadPadLayoutV3(v3Raw, v2Raw, v1Raw, out _));
        }

        /// <summary>
        /// NIGHT24: a v3 layout as v4. Equal to the v3 default: the v4 default. Customised: its buttons kept, except that RB and Y
        /// trade their actions when both still hold the v3 default's (RB NEXT WEAPON, Y SPECIAL + ARCLINE: the player never touched
        /// them). A v3 whose RB or Y holds the player's own pick stays as it is.
        /// </summary>
        public static PadLayout MigrateV3(PadLayout v3)
        {
            if (v3.SameAs(DefaultLayoutV3)) return DefaultLayoutV4;
            PadAction[] rb = v3.Raw(PadButtonName.RB), y = v3.Raw(PadButtonName.Y);
            if (!SameActs(rb, DefaultLayoutV3.Raw(PadButtonName.RB)) || !SameActs(y, DefaultLayoutV3.Raw(PadButtonName.Y))) return v3;
            return SwapRbY(v3);
        }

        /// <summary>The layout with RB and Y's action lists traded (both in the table), the key order kept.</summary>
        static PadLayout SwapRbY(PadLayout l)
        {
            List<(PadButtonName button, PadAction[] actions)> e = l.Entries();
            int rb = e.FindIndex(x => x.button == PadButtonName.RB), y = e.FindIndex(x => x.button == PadButtonName.Y);
            PadAction[] rbActs = e[rb].actions;
            e[rb] = (PadButtonName.RB, e[y].actions);
            e[y] = (PadButtonName.Y, rbActs);
            return new PadLayout(e);
        }

        // --- NIGHT25 (builder C; Unity-only, design.n25.meleeFire = GG 2026-10-08: "if you're shooting a weapon and you're trying to melee you can't do both") ---
        // The sim already runs the trigger and the quick melee in one step (harness `meleefire`); what stopped GG was the pad: PUNCH sat on R3, and the
        // right thumb that aims cannot also click its stick, so a pad player could fire (RT) or melee, not both. v5 puts PUNCH on RB (the right index
        // finger rolls from RT to RB while both thumbs stay on the sticks) and SPECIAL + ARCLINE on R3 (SPECIAL is a tap; ARCLINE has its TOGGLE).
        // v5 = v4 with the RB and RS rows traded. The store is skygrind.padLayout.v5; with no v5 the v4 store (and under it v3 / v2 / v1) is
        // migrated once (MigrateV4) and written as v5, the older strings kept for a rollback (--off25 reads v4 again).

        /// <summary>NIGHT25: the v5 default (design.n25.meleeFire): v4 with RB and RS's rows traded.</summary>
        public static readonly PadLayout DefaultLayoutV5 = SwapRbRs(DefaultLayoutV4);

        /// <summary>NIGHT25: the version field of the v5 store.</summary>
        public const int PadLayoutVersion5 = 5;

        /// <summary>NIGHT25: the v5 store's JSON, {"version":5,"layout":{..}}.</summary>
        public static string PadLayoutV5Json(PadLayout layout) => "{\"version\":" + PadLayoutVersion5 + ",\"layout\":" + layout.ToJson() + "}";

        /// <summary>NIGHT25: the layout with design.n25.meleeFire on, from the five stored strings: a v5 store with its version wins; missing, corrupt or
        /// another version reads as missing, then the v4 store (LoadPadLayoutV4) is migrated (MigrateV4; `migrated`: the caller writes it as v5).</summary>
        public static PadLayout LoadPadLayoutV5(string v5Raw, string v4Raw, string v3Raw, string v2Raw, string v1Raw, out bool migrated)
        {
            migrated = false;
            PadLayout v5 = ReadPadLayoutVersion(v5Raw, PadLayoutVersion5, DefaultLayoutV5);
            if (v5 != null) return v5;
            if (v4Raw == null && v3Raw == null && v2Raw == null && v1Raw == null) return DefaultLayoutV5;
            migrated = true;
            return MigrateV4(LoadPadLayoutV4(v4Raw, v3Raw, v2Raw, v1Raw, out _));
        }

        /// <summary>NIGHT25: a v4 layout as v5. The v4 default: the v5 default. Customised: kept, except that RB and RS trade when both still hold the
        /// v4 default's actions (the player never touched them); a v4 whose RB or RS holds the player's own pick stays as it is.</summary>
        public static PadLayout MigrateV4(PadLayout v4)
        {
            if (v4.SameAs(DefaultLayoutV4)) return DefaultLayoutV5;
            PadAction[] rb = v4.Raw(PadButtonName.RB), rs = v4.Raw(PadButtonName.RS);
            if (!SameActs(rb, DefaultLayoutV4.Raw(PadButtonName.RB)) || !SameActs(rs, DefaultLayoutV4.Raw(PadButtonName.RS))) return v4;
            return SwapRbRs(v4);
        }

        static PadLayout SwapRbRs(PadLayout l)
        {
            List<(PadButtonName button, PadAction[] actions)> e = l.Entries();
            int rb = e.FindIndex(x => x.button == PadButtonName.RB), rs = e.FindIndex(x => x.button == PadButtonName.RS);
            PadAction[] rbActs = e[rb].actions;
            e[rb] = (PadButtonName.RB, e[rs].actions);
            e[rs] = (PadButtonName.RS, rbActs);
            return new PadLayout(e);
        }

        static bool SameActs(PadAction[] a, PadAction[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        /// <summary>A versioned store's layout (ReadPadLayoutV2's rule for any version): null when it's missing, not an object,
        /// lacks that version or its layout; a layout that leaves a REQUIRED action unbound is `fallback`.</summary>
        static PadLayout ReadPadLayoutVersion(string raw, int version, PadLayout fallback)
        {
            if (raw == null) return null;
            try
            {
                if (!(JsJson.Parse(raw) is Dictionary<string, object> obj)) return null;
                if (!obj.TryGetValue("version", out object ver) || !(ver is double d) || d != version) return null;
                if (!obj.TryGetValue("layout", out object layout) || !(layout is Dictionary<string, object> l)) return null;
                return FromObject(l, fallback);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// NIGHT19 (GG's L3 SPRINT switch, TOGGLE: CoD's): the sprint key's latch for one step. A click flips it; it lets go
        /// when the move stick is back at the centre (`moving` false) or the bar is empty, so a click at rest or on an empty
        /// bar doesn't latch, and a bar that refills never restarts a sprint by itself.
        /// </summary>
        public static bool SprintLatch(bool on, bool clicked, bool moving, bool empty)
        {
            if (clicked) on = !on;
            return on && moving && !empty;
        }

        /// <summary>
        /// NIGHT19: L3 TOGGLE on a merged frame (ReaderInput, each step, after MergeMove): the frame's sprint press is the
        /// click, its move axes the stick; the frame's sprint becomes the latch (held while on, pressed on the step it latches).
        /// </summary>
        public static void ApplySprintToggle(ref InputFrame frame, ref bool on, bool empty)
        {
            bool was = on;
            on = SprintLatch(on, frame.sprint.pressed, frame.moveX != 0 || frame.moveZ != 0, empty);
            frame.sprint = new Button { held = on, pressed = on && !was };
        }

        /// <summary>NIGHT19: the bar is empty (the Mover's stamina rules on and the bar at 0: a sprint emptied it, or a boost).
        /// With the rules off there's no bar: never empty.</summary>
        public static bool SprintBarEmpty(in Stamina.State s) => s.rules.Any && s.bar <= 0;

        /// <summary>Glyph text per button (one neutral set: the standard positions' letters; docs/GAMEPAD.md has the Nintendo note). PAD_GLYPHS :42-45.</summary>
        public static readonly string[] Glyphs =
        {
            "A", "B", "X", "Y", "LB", "RB", "LT", "RT", "VIEW", "MENU", "L3", "R3", "D-PAD UP", "D-PAD DOWN", "D-PAD LEFT", "D-PAD RIGHT",
        };

        /// <summary>A button counts as down past this analog value (the triggers report 0..1). TRIGGER_DOWN :47-48.</summary>
        public const double TriggerDown = 0.35;

        /// <summary>NIGHT27 (builder W, the pulse diagnosis): a trigger that is down stays down until it falls BELOW this (hysteresis): a worn or half-held
        /// real trigger that chatters around TriggerDown no longer reads as press-release-press (a held LT chopped into releases spends or cancels the PULSE).
        /// Unity-only; the web had a single threshold. The PadReader applies it to the live device; the pure ButtonDown is unchanged.</summary>
        public const double TriggerUp = 0.25;

        /// <summary>
        /// The hot-swap's mouse rule (keyboardMouse.js :220): a move of |dx| + |dy| over this many px is "a real move, not a
        /// desk bump" and puts the device back to the keyboard and mouse. (A keyboardMouse.js literal, kept here with the
        /// pad's own device rule, padActivity.)
        /// </summary>
        public const double MouseDeviceMove = 2;

        /// <summary>What each pad action does, for the pad's key list (pause, CONTROLS) and docs/GAMEPAD.md. PAD_ACTION_LABELS :191-196 (by PadAction).</summary>
        public static readonly string[] ActionLabels =
        {
            "JUMP", "DASH", "SLIDE / SLAM", "FIRE", "ZOOM / SPECIAL", "SPECIAL", "PUNCH / PARRY", "BUY (HOLD)", "ARCLINE", "REEL IN",
            "NEXT WEAPON", "PREVIOUS WEAPON", "LAST WEAPON", "LOCK-ON", "X-RAY", "PERK LIST (HOLD)", "PAUSE",
            "WEAPON WHEEL (HOLD)", // NIGHT15
            "SPRINT", // NIGHT19 (LabelOrderV2: CONTROLS' row with the v2 layout)
        };

        /// <summary>PAD_ACTION_LABELS' key order (main.js padBinds.rows lists the actions in it).</summary>
        public static readonly PadAction[] LabelOrder =
        {
            PadAction.Fire, PadAction.AltFire, PadAction.Jump, PadAction.Slide, PadAction.Interact, PadAction.Reel, PadAction.Dash,
            PadAction.Punch, PadAction.Grapple, PadAction.LockOn, PadAction.Special, PadAction.Xray, PadAction.NextWeapon,
            PadAction.PrevWeapon, PadAction.QuickSwap, PadAction.PerkList, PadAction.Pause,
        };

        /// <summary>NIGHT19 U3-33.3: CONTROLS' rows with the v2 layout: LabelOrder with SPRINT after DASH (the boost it pairs with).</summary>
        public static readonly PadAction[] LabelOrderV2 =
        {
            PadAction.Fire, PadAction.AltFire, PadAction.Jump, PadAction.Slide, PadAction.Interact, PadAction.Reel, PadAction.Dash,
            PadAction.Sprint, PadAction.Punch, PadAction.Grapple, PadAction.LockOn, PadAction.Special, PadAction.Xray, PadAction.NextWeapon,
            PadAction.PrevWeapon, PadAction.QuickSwap, PadAction.PerkList, PadAction.Pause,
        };

        /// <summary>Every action the layout must keep on some button (the frame buttons, pause, the weapon cycle). REQUIRED :205-206.</summary>
        static readonly uint Required = FrameButtonsWeb | Bit(PadAction.Pause) | Bit(PadAction.NextWeapon);

        /// <summary>NIGHT19: must `a` stay bound (REQUIRED)? CONTROLS' rebind of an UNBOUND action checks what it would drop.</summary>
        public static bool IsRequired(PadAction a) => (Required & Bit(a)) != 0;

        /// <summary>A standard button's index by its web name ('a' .. 'right'), or -1 (`name in PAD_BUTTONS`).</summary>
        public static int ButtonFromName(string name)
        {
            if (name == null) return -1;
            for (int i = 0; i < ButtonNames.Length; i++)
            {
                if (ButtonNames[i] == name) return i;
            }
            return -1;
        }

        /// <summary>An action by its web name ('jump' .. 'pause'), or -1.</summary>
        public static int ActionFromName(string name)
        {
            if (name == null) return -1;
            for (int i = 0; i < ActionNames.Length; i++)
            {
                if (ActionNames[i] == name) return i;
            }
            return -1;
        }

        /// <summary>three.js MathUtils.degToRad (DEG2RAD = Math.PI / 180).</summary>
        public static double DegToRad(double degrees) => degrees * (Math.PI / 180);

        /// <summary>Pure: is standard-mapping button `i` down?</summary>
        public static bool ButtonDown(PadState pad, int i)
        {
            if (pad == null || i < 0 || i >= PadState.ButtonCount) return false;
            PadButton b = pad.buttons[i];
            return b.pressed || b.value > TriggerDown;
        }

        /// <summary>
        /// Pure: a radial dead zone with rescale. Inside `inner` the stick reads 0; past `outer` it reads full; between, the
        /// length is remapped to 0..1 so the first move past the dead zone starts from zero (no jump).
        /// </summary>
        public static (double x, double y) RadialDeadzone(double x, double y, double inner, double outer)
        {
            double len = JsMath.Hypot(x, y);
            if (!(len > inner)) return (0, 0);
            double span = JsMath.Max(1e-6, outer - inner);
            double scaled = JsMath.Min(1, (len - inner) / span);
            return ((x / len) * scaled, (y / len) * scaled);
        }

        /// <summary>Pure: the response curve on a -1..1 value: 'linear', or 'precise' (squared: finer near the centre).</summary>
        public static double ResponseCurve(double v, bool precise)
        {
            if (precise) return JsSign(v) * v * v;
            return v;
        }

        /// <summary>responseCurve(v, curve = 'linear') with the setting's text.</summary>
        public static double ResponseCurve(double v, string curve = "linear") => ResponseCurve(v, curve == "precise");

        /// <summary>Math.sign: -1, +1, or v itself for +-0 and NaN.</summary>
        static double JsSign(double v) => v > 0 ? 1 : v < 0 ? -1 : v;

        /// <summary>Pure: the pad's move axes and look rates for one poll (s: rates in rad/s at full tilt and x1 sensitivity).</summary>
        public static Sticks ReadSticks(PadState pad, StickSettings s)
        {
            double a0 = pad != null ? pad.axes[0] : 0, a1 = pad != null ? pad.axes[1] : 0;
            double a2 = pad != null ? pad.axes[2] : 0, a3 = pad != null ? pad.axes[3] : 0;
            var (mx, my) = RadialDeadzone(a0, a1, s.inner, s.outer);
            var (lx, ly) = RadialDeadzone(a2, a3, s.inner, s.outer);
            // Stick up is -1; forward is +moveZ. Look: right turns right (yaw decreases), up looks up (pitch increases).
            double lookX = ResponseCurve(lx, s.precise);
            double lookY = ResponseCurve(ly, s.precise);
            return new Sticks
            {
                moveX = mx,
                moveZ = -my,
                yawRate = -lookX * s.yawRate * s.sensX,
                pitchRate = -lookY * s.pitchRate * s.sensY * (s.invertY ? -1 : 1),
            };
        }

        /// <summary>Pure: which actions the pad holds now, from the layout (a mask).</summary>
        public static uint HeldActions(PadState pad, PadLayout layout = null) => HeldActions(pad, layout, null, out _);

        /// <summary>
        /// heldActions with the web Set's insertion order: `order` (null, or at least ActionCount long) receives the held
        /// actions in the order the Set would iterate them (the layout's key order, each button's actions in order).
        /// </summary>
        public static uint HeldActions(PadState pad, PadLayout layout, PadAction[] order, out int count)
        {
            layout ??= DefaultLayout;
            uint held = 0;
            count = 0;
            for (int i = 0; i < layout.Count; i++)
            {
                PadButtonName name = layout[i];
                if (!ButtonDown(pad, (int)name)) continue;
                PadAction[] acts = layout.Raw(name);
                for (int j = 0; j < acts.Length; j++)
                {
                    uint bit = Bit(acts[j]);
                    if ((held & bit) != 0) continue;
                    held |= bit;
                    if (order != null) order[count] = acts[j];
                    count++;
                }
            }
            return held;
        }

        /// <summary>Pure: the actions newly down since `prev` (press edges), as a mask.</summary>
        public static uint PressedSince(uint held, uint prev) => held & ~prev;

        /// <summary>pressedSince in the held Set's order: copies the actions of `heldOrder` not in `prev` into `output`; returns their count.</summary>
        public static int PressedSince(PadAction[] heldOrder, int heldCount, uint prev, PadAction[] output)
        {
            int n = 0;
            for (int i = 0; i < heldCount; i++)
            {
                if ((prev & Bit(heldOrder[i])) == 0) output[n++] = heldOrder[i];
            }
            return n;
        }

        /// <summary>Pure: the first button of the layout that carries `action` (for the prompts), or null.</summary>
        public static PadButtonName? ButtonFor(PadAction action, PadLayout layout = null)
        {
            layout ??= DefaultLayout;
            uint bit = Bit(action);
            for (int i = 0; i < layout.Count; i++)
            {
                if ((layout.MaskOf(layout[i]) & bit) != 0) return layout[i];
            }
            return null;
        }

        /// <summary>Pure: does the pad show any input (a button, or a stick past `threshold`)? The hot-swap's "the pad was used".</summary>
        public static bool PadActivity(PadState pad, double threshold = 0.5)
        {
            if (pad == null) return false;
            for (int i = 0; i < PadState.ButtonCount; i++)
            {
                if (i != 16 && ButtonDown(pad, i)) return true;
            }
            for (int i = 0; i < PadState.AxisCount; i++)
            {
                if (Math.Abs(pad.axes[i]) > threshold) return true;
            }
            return false;
        }

        /// <summary>
        /// Pure: menu navigation from a pad. The d-pad or the left stick steps the focus (repeating while held: first after
        /// `delay`, then every `every` s); A confirms, B backs out, START is pause / resume. `state` is mutated: the held
        /// direction, its time, last buttons; `dt` s since the last poll. (The defaults are menuNav's; the game passes
        /// config.gamepad's menuDelay / menuEvery / menuStick.)
        /// </summary>
        public static PadMenu MenuNav(PadState pad, MenuNavState state, double dt, double delay = 0.35, double every = 0.12, double stick = 0.6)
        {
            double ax0 = pad != null ? pad.axes[0] : 0, ax1 = pad != null ? pad.axes[1] : 0;
            int v = ButtonDown(pad, (int)PadButtonName.Up) || ax1 < -stick ? -1 : ButtonDown(pad, (int)PadButtonName.Down) || ax1 > stick ? 1 : 0;
            int h = ButtonDown(pad, (int)PadButtonName.Left) || ax0 < -stick ? -1 : ButtonDown(pad, (int)PadButtonName.Right) || ax0 > stick ? 1 : 0;
            int dir = v != 0 ? v * 2 : h; // one direction at a time (vertical wins)
            bool fire = false;
            if (dir != state.dir)
            {
                state.dir = dir;
                state.hold = 0;
                fire = dir != 0;
            }
            else if (dir != 0)
            {
                double before = state.hold;
                state.hold += dt;
                if (before < delay && state.hold >= delay) fire = true;
                else if (state.hold > delay && Math.Floor((state.hold - delay) / every) > Math.Floor((before - delay) / every)) fire = true;
            }
            int now = (ButtonDown(pad, (int)PadButtonName.A) ? 1 : 0) | (ButtonDown(pad, (int)PadButtonName.B) ? 2 : 0) | (ButtonDown(pad, (int)PadButtonName.Start) ? 4 : 0)
                | (ButtonDown(pad, (int)PadButtonName.LB) ? 8 : 0) | (ButtonDown(pad, (int)PadButtonName.RB) ? 16 : 0); // NIGHT19: the bumpers
            bool lb = (now & 8) != 0 && (state.prev & 8) == 0, rb = (now & 16) != 0 && (state.prev & 16) == 0;
            var output = new PadMenu
            {
                move = fire && Math.Abs(dir) == 2 ? dir / 2 : 0,
                side = fire && Math.Abs(dir) == 1 ? dir : 0,
                confirm = (now & 1) != 0 && (state.prev & 1) == 0,
                back = (now & 2) != 0 && (state.prev & 2) == 0,
                start = (now & 4) != 0 && (state.prev & 4) == 0,
                tab = lb == rb ? 0 : lb ? -1 : 1, // NIGHT19 U3-33.3: one tab a press (both at once: neither)
            };
            state.prev = now;
            return output;
        }

        /// <summary>
        /// Pure: the pad on top of the frame the keyboard + mouse just filled, in place. Move axes add (clamped to -1..1; the
        /// Mover normalizes the wish direction, so any tilt past the dead zone is a full step, like a key), buttons OR in, press
        /// edges latch. `pad` null (no pad, or not in game) or a pad at rest leaves the frame exactly as the keyboard left it.
        /// (Unity: the web's one frame is InputFrame + CombatInput, so both are merged.)
        /// </summary>
        public static void MergePad(ref InputFrame frame, ref CombatInput combat, PadFrame pad)
        {
            MergeMove(ref frame, pad);
            MergeCombat(ref combat, pad);
        }

        /// <summary>mergePad's InputFrame half: the move axes and jump, dash, slide, grapple, reel.</summary>
        public static void MergeMove(ref InputFrame frame, PadFrame pad)
        {
            if (pad == null) return;
            if (pad.moveX != 0) frame.moveX = JsMath.Max(-1, JsMath.Min(1, frame.moveX + pad.moveX));
            if (pad.moveZ != 0) frame.moveZ = JsMath.Max(-1, JsMath.Min(1, frame.moveZ + pad.moveZ));
            Merge(ref frame.jump, pad, PadAction.Jump);
            Merge(ref frame.dash, pad, PadAction.Dash);
            Merge(ref frame.slide, pad, PadAction.Slide);
            Merge(ref frame.grapple, pad, PadAction.Grapple);
            Merge(ref frame.reel, pad, PadAction.Reel);
            Merge(ref frame.sprint, pad, PadAction.Sprint); // NIGHT19: the held bit (ReaderInput turns L3 TOGGLE into a held latch)
        }

        /// <summary>mergePad's CombatInput half: fire, altFire, special, punch, interact.</summary>
        public static void MergeCombat(ref CombatInput combat, PadFrame pad)
        {
            if (pad == null) return;
            Merge(ref combat.fire, pad, PadAction.Fire);
            Merge(ref combat.altFire, pad, PadAction.AltFire);
            Merge(ref combat.special, pad, PadAction.Special);
            Merge(ref combat.punch, pad, PadAction.Punch);
            Merge(ref combat.interact, pad, PadAction.Interact);
        }

        static void Merge(ref Button b, PadFrame pad, PadAction a)
        {
            uint bit = Bit(a);
            if ((pad.held & bit) != 0) b.held = true;
            if ((pad.latched & bit) != 0) b.pressed = true;
        }

        /// <summary>Pure: the pad's controls as [glyph, what] rows: the sticks, then every button of the layout.</summary>
        public static List<(string glyph, string what)> PadRows(PadLayout layout = null)
        {
            layout ??= DefaultLayout;
            var rows = new List<(string, string)> { ("LEFT STICK", "MOVE"), ("RIGHT STICK", "LOOK") };
            for (int i = 0; i < layout.Count; i++)
            {
                PadButtonName b = layout[i];
                PadAction[] acts = layout.Raw(b);
                var what = new StringBuilder();
                for (int j = 0; j < acts.Length; j++)
                {
                    if (j > 0) what.Append(" · ");
                    what.Append(ActionLabels[(int)acts[j]]);
                }
                rows.Add((Glyphs[(int)b], what.ToString()));
            }
            return rows;
        }

        /// <summary>
        /// Pure: give `action` to pad button `button` (its web name). The two buttons swap their whole action lists (like a
        /// key rebind swaps two actions), so nothing is ever left unbound. A copy comes back.
        /// </summary>
        public static PadRebind RebindPad(PadLayout layout, PadAction action, string button)
        {
            int b = ButtonFromName(button);
            if (b < 0) return new PadRebind { layout = layout, ok = false, swapped = null };
            return RebindPad(layout, action, (PadButtonName)b);
        }

        /// <summary>rebindPad with a standard button.</summary>
        public static PadRebind RebindPad(PadLayout layout, PadAction action, PadButtonName button)
        {
            PadButtonName? from = null;
            uint bit = Bit(action);
            for (int i = 0; i < layout.Count; i++)
            {
                if ((layout.MaskOf(layout[i]) & bit) == 0) continue;
                from = layout[i];
                break;
            }
            if (from == null) return new PadRebind { layout = layout, ok = false, swapped = null };
            if (from.Value == button) return new PadRebind { layout = layout, ok = true, swapped = null };
            var next = layout.Entries();
            PadAction[] there = layout.Has(button) ? layout.Raw(button) : Array.Empty<PadAction>();
            int at = next.FindIndex(e => e.button == button);
            if (at >= 0) next[at] = (button, layout.Raw(from.Value));
            else next.Add((button, layout.Raw(from.Value)));
            int fromAt = next.FindIndex(e => e.button == from.Value);
            if (there.Length > 0) next[fromAt] = (from.Value, there);
            else next.RemoveAt(fromAt);
            return new PadRebind { layout = new PadLayout(next), ok = true, swapped = there.Length > 0 ? from : null };
        }

        /// <summary>Pure: a stored pad layout, or the default when it is missing, corrupt, or leaves a needed action unbound.</summary>
        public static PadLayout ReadPadLayout(string raw)
        {
            try
            {
                object v = JsJson.Parse(raw ?? "null");
                // !v || typeof v !== 'object': not an object. (An array is one in JS, but its keys "0", "1", .. are no
                // button names and an empty one binds nothing: the default either way.)
                if (!(v is Dictionary<string, object> obj)) return DefaultLayout;
                return FromObject(obj, DefaultLayout);
            }
            catch (Exception)
            {
                return DefaultLayout;
            }
        }

        /// <summary>readPadLayout's body on a parsed object (NIGHT19: the v2 store's "layout" too, with its own fallback).</summary>
        static PadLayout FromObject(Dictionary<string, object> obj, PadLayout fallback)
        {
            var output = new List<(PadButtonName, PadAction[])>();
            foreach (var kv in obj)
            {
                int b = ButtonFromName(kv.Key);
                if (b < 0 || !(kv.Value is List<object> list)) return fallback;
                var acts = new PadAction[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    int a = ActionFromName(list[i] as string);
                    if (a < 0) return fallback;
                    acts[i] = (PadAction)a;
                }
                output.Add(((PadButtonName)b, acts));
            }
            uint bound = 0;
            foreach (var (_, acts) in output)
            {
                foreach (PadAction a in acts) bound |= Bit(a);
            }
            return (bound & Required) == Required ? new PadLayout(output) : fallback;
        }

        /// <summary>
        /// Pure: which standard buttons went down since `prev` (raw, for "press a pad button"): a mask of bits by
        /// PadButtonName in PAD_BUTTONS' order; `now` gets the buttons down now (the next call's prev).
        /// </summary>
        public static int RawPresses(PadState pad, int prev, out int now)
        {
            now = 0;
            for (int i = 0; i < ButtonNames.Length; i++)
            {
                if (ButtonDown(pad, i)) now |= 1 << i;
            }
            return now & ~prev;
        }

        /// <summary>
        /// keyboardMouse.js pollPad :391-396: the right stick turns the view at frame rate (like the mouse), x `k` (the look
        /// scale: the lock's friction x the zoom), the pitch clamped to +-`limit` rad. Nothing when both rates are 0. (The
        /// caller skips it while the radial is open.)
        /// </summary>
        public static void ApplyLook(ref double yaw, ref double pitch, Sticks s, double k, double dt, double limit)
        {
            if (s.yawRate == 0 && s.pitchRate == 0) return;
            yaw += s.yawRate * k * dt;
            pitch = JsMath.Max(-limit, JsMath.Min(limit, pitch + s.pitchRate * k * dt));
        }
    }
}
