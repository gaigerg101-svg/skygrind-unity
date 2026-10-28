// NIGHT33 builder U (design.n33.hud33): the HUD33 layout as data, and its checks, in pure C# (no UnityEngine), so EditMode tests can run them and the
// hud33 reel can run the same checks on the live text rects at 1080p and 720p (U1 section 7; DESIGN33 U "Checks"):
//   - no two text rects overlap (a label and its shadow twin are one label);
//   - every text is at least 16 px at 1080p (CLAUDE.md rule 8);
//   - every element keeps the 54 px margin from each screen edge;
//   - no element wider than 360 px except the boss bar (the message slot may span the boss bar's 640 px column: its line is one at a time) and none taller
//     than 80 px;
//   - no label repeated on screen (the LT word shows once: for the held weapon).
// Units: the HUD canvas's (1920 x 1080 reference, scaled with the height), so a 16:9 screen of any size has the same numbers; rects are top-left
// origin, y down. Box.Anchor is also the pivot (an element hangs off its own corner of the screen).
using System;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static class Hud33Layout
    {
        public const double RefH = 1080;

        /// <summary>One element: anchored at (ax, ay) of the screen (0 left / bottom .. 1 right / top) with the same pivot, offset (x, y) like a
        /// RectTransform's anchoredPosition (y up), size (w, h); px is its font size (0: not text).</summary>
        public struct Box
        {
            public string name;
            public double ax, ay, x, y, w, h, px;
            public bool Text => px > 0;

            public Box(string name, double ax, double ay, double x, double y, double w, double h, double px = 0)
            {
                this.name = name; this.ax = ax; this.ay = ay; this.x = x; this.y = y; this.w = w; this.h = h; this.px = px;
            }
        }

        /// <summary>A checked rect (top-left origin, y down, canvas units).</summary>
        public struct Item
        {
            public string name, text;
            public double x, y, w, h, px;
            public bool bossBar, slot;
            /// <summary>A world marker (the door's): it moves with the world, so only its text size is checked (no margin, no overlap).</summary>
            public bool world;
        }

        /// <summary>The canvas width for a screen (the canvas is RefH tall).</summary>
        public static double CanvasW(double screenW, double screenH) => screenH > 0 ? screenW * RefH / screenH : 1920;

        /// <summary>A box's rect on a canvas of (cw, ch).</summary>
        public static Item ToItem(Box b, double cw, double ch, string text = null)
        {
            double left = b.ax * cw + b.x - b.ax * b.w;
            double bottom = b.ay * ch + b.y - b.ay * b.h;
            return new Item { name = b.name, text = text ?? (b.Text ? b.name : null), x = left, y = ch - (bottom + b.h), w = b.w, h = b.h, px = b.px, bossBar = b.name == BossBarName || b.name == BossNameName, slot = b.name == SlotName || b.name == TeachSlotName };
        }

        public const string BossBarName = "Boss bar", SlotName = "Message slot";

        /// <summary>NIGHT36 M (text36): the teaching slot's name (the one other line allowed the 640 px column: one teaching line at a time).</summary>
        public const string TeachSlotName = "Teach slot";

        /// <summary>NIGHT36 M (text36): the teaching slot in the lower middle: its centre Hud36TeachCentre (0.62) of the canvas down from the top, the
        /// top-centre column's width, two lines of Hud36TeachPx (32 px) at most (UnityTuning.Move36.cs).</summary>
        public static Box TeachSlot(double cw = 1920) =>
            new Box(TeachSlotName, 0.5, 1, 0, -(UnityTuning.Hud36TeachCentre * RefH - UnityTuning.Hud36TeachH / 2), Math.Min(TopCentreW(cw), UnityTuning.Hud36TeachTextW), UnityTuning.Hud36TeachH, UnityTuning.Hud36TeachPx);

        // ---- the layout (numbers from UnityTuning.Hud33.cs; the view builds from these same boxes) ----

        static double Side => UnityTuning.Hud33SideMargin;
        static double Edge => UnityTuning.Hud33EdgeMargin;

        public static Box Stamina => new Box("Stamina", 0, 0, Side, Edge, 2 * UnityTuning.Hud33StaminaW + UnityTuning.Hud33StaminaGap, UnityTuning.Hud33StaminaH);
        public static Box HealthTicks => new Box("Health ticks", 0, 0, Side, Edge + UnityTuning.Hud33StaminaH + UnityTuning.Hud33RowGap, UnityTuning.Hud33HealthTicks * UnityTuning.Hud33HealthTickPitch, UnityTuning.Hud33TickH);
        public static Box HealthNumber => new Box("Health", 0, 0, Side, HealthTicks.y + UnityTuning.Hud33TickH + UnityTuning.Hud33RowGap, UnityTuning.Hud33HealthNumW, UnityTuning.Hud33Big, UnityTuning.Hud33Big);

        public static Box WeaponPips(int owned) => new Box("Weapon pips", 1, 0, -Side, Edge, Math.Max(1, owned) * UnityTuning.Hud33PipPitch, UnityTuning.Hud33PipH);
        public static Box LtLabel => new Box("LT label", 1, 0, -Side, Edge + UnityTuning.Hud33PipH + UnityTuning.Hud33WeaponGap, UnityTuning.Hud33WeaponBoxW, UnityTuning.Hud33Small + 4, UnityTuning.Hud33Small);
        public static Box WeaponName => new Box("Weapon", 1, 0, -Side, LtLabel.y + LtLabel.h + UnityTuning.Hud33WeaponGap, UnityTuning.Hud33WeaponBoxW, UnityTuning.Hud33Name + 6, UnityTuning.Hud33Name);
        /// <summary>The switch list's line i (0 = the bottom line), shown for Hud33SwitchExpand s over the name.</summary>
        public static Box SwitchLine(int i) => new Box("Switch " + i, 1, 0, -Side, WeaponName.y + WeaponName.h + UnityTuning.Hud33WeaponGap + i * UnityTuning.Hud33ListPitch, UnityTuning.Hud33WeaponBoxW, UnityTuning.Hud33ListPitch, UnityTuning.Hud33Small);

        // NIGHT34 H (design.n34.rail34): THE ARSENAL RAIL replaces the pip row (and the switch list: the rail always shows every slot); the LT label and
        // the held name stand over it, right-aligned as before. Numbers: UnityTuning.Hud34.cs.
        public const string RailName = "Rail 34";
        public static double RailH => UnityTuning.Rail34IconH + UnityTuning.Rail34LineGap + UnityTuning.Rail34HandBarH;
        public static Box Rail(int owned) => new Box(RailName, 1, 0, -Side, Edge, Math.Max(1, Math.Min(owned, UnityTuning.Rail34MaxSlots)) * UnityTuning.Rail34SlotPitch, RailH);
        public static Box RailLt => new Box("LT label", 1, 0, -Side, Edge + RailH + UnityTuning.Rail34NameGap, UnityTuning.Hud33WeaponBoxW, UnityTuning.Hud33Small + 4, UnityTuning.Hud33Small);
        public static Box RailWeaponName => new Box("Weapon", 1, 0, -Side, RailLt.y + RailLt.h + UnityTuning.Hud33WeaponGap, UnityTuning.Hud33WeaponBoxW, UnityTuning.Hud33Name + 6, UnityTuning.Hud33Name);

        /// <summary>Every box of the at-rest HUD with the rail on (rail34): All's boxes with the pip row, the LT label and the name swapped for the rail's,
        /// and no switch list (the rail is the list).</summary>
        public static List<Box> AllRail34(int owned, bool boss, double cw = 1920)
        {
            var l = All(owned, false, boss, cw);
            l.RemoveAll(b => b.name == "Weapon pips" || b.name == "LT label" || b.name == "Weapon");
            l.Add(Rail(owned));
            l.Add(RailLt);
            l.Add(RailWeaponName);
            return l;
        }

        public static Box Points => new Box("Points", 1, 1, -Side, -Edge, UnityTuning.Hud33PointsBoxW, UnityTuning.Hud33Mid + 8, UnityTuning.Hud33Mid);
        public static Box RankLetter => new Box("Rank", 1, 1, -Side, Points.y - Points.h - UnityTuning.Hud33RankGap, UnityTuning.Hud33RankPx * UnityTuning.Hud33RankBoxEm, UnityTuning.Hud33RankPx + 8, UnityTuning.Hud33RankPx);
        public static Box RankLine => new Box("Rank line", 1, 1, -Side - RankLetter.w - UnityTuning.Hud33RankGap, RankLetter.y - RankLetter.h + UnityTuning.Hud33RankGap + UnityTuning.Hud33RankLineH, UnityTuning.Hud33RankLineW, UnityTuning.Hud33RankLineH);

        public static Box Round => new Box("Round", 0, 1, Side, -Edge, UnityTuning.Hud33RoundBoxW, UnityTuning.Hud33RoundPx + 6, UnityTuning.Hud33RoundPx);
        /// <summary>The survival extras (the objective, the PROTOCOL, exfil), one 16 px line each under the round line.</summary>
        public static Box RunLine(int i) => new Box("Run line " + i, 0, 1, Side, Round.y - Round.h - UnityTuning.Hud33RunLineGap - i * UnityTuning.Hud33RunLinePitch, UnityTuning.Hud33RoundBoxW, UnityTuning.Hud33RunLinePitch - 2, UnityTuning.Hud33Small);
        /// <summary>Hud29's DOORS OPEN tag (its own canvas): inside the top-left margin, under the run lines.</summary>
        public static Box DoorsOpenTag => new Box("DOORS OPEN tag", 0, 1, Side, RunLine(UnityTuning.Hud33RunLines).y - UnityTuning.Hud33RunLineGap, UnityTuning.Hud33RoundBoxW, 32, UnityTuning.Hud29TagTextPx);

        /// <summary>The top-centre column's width on a canvas `cw` wide: Hud33SlotW (the boss bar's 640), narrowed on a narrow screen (4:3) so it keeps clear of
        /// the round line on the left and the points on the right.</summary>
        public static double TopCentreW(double cw) =>
            Math.Max(UnityTuning.Hud33MaxWidth, Math.Min(UnityTuning.Hud33SlotW, cw - 2 * (Side + Math.Max(UnityTuning.Hud33RoundBoxW, UnityTuning.Hud33PointsBoxW) + UnityTuning.Hud33RankGap)));

        /// <summary>The message slot: at the top margin; under the boss bar while a boss fights; under the Glacier chase's gap bar (Hud29) while it shows.</summary>
        public static Box Slot(bool boss, bool gapBar = false, double cw = 1920) =>
            new Box(SlotName, 0.5, 1, 0, -(gapBar ? UnityTuning.Hud33SlotGapY : boss ? UnityTuning.Hud33SlotBossY : UnityTuning.Hud33SlotY), TopCentreW(cw), UnityTuning.Hud33SlotH, UnityTuning.Hud33Name);

        public const string BossNameName = "Boss name";
        /// <summary>The boss's name, 16 px over the bar.</summary>
        public static Box BossName(double cw = 1920) => new Box(BossNameName, 0.5, 1, 0, -Edge, TopCentreW(cw), UnityTuning.Hud33Small + 4, UnityTuning.Hud33Small);
        /// <summary>The boss bar: 640 x 6 under its name (narrowed with the column on a narrow screen).</summary>
        public static Box BossBar(double cw = 1920) => new Box(BossBarName, 0.5, 1, 0, -Edge - (UnityTuning.Hud33Small + 4) - UnityTuning.Hud33BossNameGap, Math.Min(UnityTuning.Hud33BossBarW, TopCentreW(cw)), UnityTuning.Hud33BossBarH);

        /// <summary>Every box of the at-rest HUD with `owned` weapons, the rank up, the switch list open (switching) and a boss (boss), for the tests.</summary>
        public static List<Box> All(int owned, bool switching, bool boss, double cw = 1920)
        {
            var l = new List<Box> { Stamina, HealthTicks, HealthNumber, WeaponPips(owned), LtLabel, WeaponName, Points, RankLetter, RankLine, Round, Slot(boss, false, cw) };
            if (switching) for (int i = 0; i < owned; i++) l.Add(SwitchLine(i));
            for (int i = 0; i < UnityTuning.Hud33RunLines; i++) l.Add(RunLine(i));
            l.Add(DoorsOpenTag);
            if (boss) { l.Add(BossName(cw)); l.Add(BossBar(cw)); }
            return l;
        }

        // ---- the checks ----

        public const double MinOverlapPx2 = 4;

        static bool Twin(Item a, Item b) => TwinName(a.name, b.name);

        /// <summary>A label and its own shadow twin / its holder: one name is the other's, or its prefix with a '/'.</summary>
        static bool TwinName(string a, string b) =>
            a == b || a.StartsWith(b + "/", StringComparison.Ordinal) || b.StartsWith(a + "/", StringComparison.Ordinal);

        static double Area(Item a, Item b)
        {
            double w = Math.Min(a.x + a.w, b.x + b.w) - Math.Max(a.x, b.x), h = Math.Min(a.y + a.h, b.y + b.h) - Math.Max(a.y, b.y);
            return w > 0 && h > 0 ? w * h : 0;
        }

        /// <summary>Every rule broken by one frame's items on a canvas of (cw, ch), one line each (empty: PASS).</summary>
        public static List<string> Check(List<Item> items, double cw, double ch)
        {
            var bad = new List<string>();
            double edge = UnityTuning.Hud33EdgeMargin;
            var seen = new Dictionary<string, string>();
            for (int i = 0; i < items.Count; i++)
            {
                Item a = items[i];
                bool text = !string.IsNullOrEmpty(a.text);
                if (text && a.px > 0 && a.px < UnityTuning.Hud33TextMin - 0.01) bad.Add($"small text: {a.name} '{a.text}' {a.px:0.#} px < {UnityTuning.Hud33TextMin:0}");
                if (a.world) continue; // a world marker: its size only
                if (a.x < edge - 0.5 || a.y < edge - 0.5 || a.x + a.w > cw - edge + 0.5 || a.y + a.h > ch - edge + 0.5)
                    bad.Add($"margin: {a.name} at ({a.x:0},{a.y:0}) {a.w:0}x{a.h:0} inside the {edge:0} px edge of {cw:0}x{ch:0}");
                double maxW = a.bossBar || a.slot ? UnityTuning.Hud33BossBarW : UnityTuning.Hud33MaxWidth;
                if (a.w > maxW + 0.5) bad.Add($"wide: {a.name} {a.w:0} px > {maxW:0}");
                if (a.h > UnityTuning.Hud33MaxHeight + 0.5) bad.Add($"tall: {a.name} {a.h:0} px > {UnityTuning.Hud33MaxHeight:0}");
                if (text)
                {
                    string key = a.text.Trim().ToUpperInvariant();
                    if (seen.TryGetValue(key, out string other) && !TwinName(other, a.name)) bad.Add($"repeated label: '{a.text}' in {other} and {a.name}");
                    else seen[key] = a.name;
                }
                if (!text) continue;
                for (int j = i + 1; j < items.Count; j++)
                {
                    Item b = items[j];
                    if (string.IsNullOrEmpty(b.text) || b.world || Twin(a, b)) continue;
                    double ar = Area(a, b);
                    if (ar > MinOverlapPx2) bad.Add($"overlap: {a.name} '{a.text}' x {b.name} '{b.text}' ({ar:0} px2)");
                }
            }
            return bad;
        }

        /// <summary>The static layout checked on a screen: the boxes as items (text items carry their names as text).</summary>
        public static List<string> CheckLayout(double screenW, double screenH, int owned, bool switching, bool boss, bool teach36 = false)
        {
            double cw = CanvasW(screenW, screenH);
            var items = new List<Item>();
            foreach (Box b in All(owned, switching, boss, cw)) items.Add(ToItem(b, cw, RefH));
            if (teach36) items.Add(ToItem(TeachSlot(cw), cw, RefH)); // NIGHT36 M (text36)
            return Check(items, cw, RefH);
        }
    }
}
