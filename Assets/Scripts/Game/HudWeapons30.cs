// NIGHT30 builder V (item 2; design.n30.hud30; DESIGN30 A5 + F + reviewer I.2): THE WEAPON-STATE WIDGET. GG: "players need a clearer, more readable
// indicator for when a weapon is charging, cooling down, or ready to fire again". One slot per owned weapon in the weapon card (bottom right), in
// place of NIGHT17's silhouette strip; the PULSE gauntlet (with design.n27.pulse27 live) has TWO slots, PUNCH (its heavy swing's cooldown) and BLAST
// (its five kill pips, then the blast's cooldown). Each slot says its state three ways (colour-blind safe: the word and the ring's shape carry it alone):
//   READY     a full ring, the icon bright cyan; the word READY for 1 s after it turns ready, a 0.15 s flash on the disc (game.flashGate: at most 3 a
//             second, x FLASH INTENSITY) and a one-frame glow ring; FeelProbe event + Hud30.WeaponReady "weaponReady" for GG's sound.
//   COOLING   a DRAINING ring (dim cyan: never red, red-orange is danger), the icon at 40 %, the seconds left while over 1 s ("1.8").
//   CHARGING  a FILLING ring with ticks: a charge shot's level ("CHARGING"), or the BLAST slot's pips ("2/5", five ticks).
//   SWITCHING on the slot a switch waits for (NIGHT29's lockout queues it while an action runs: DESIGN30 A2 made visible).
// The slot in hand stands 8 px higher with a cyan bar under it. A cooldown shorter than Hud30MinCoolSeconds (the pistol's shot cycle) never shows
// COOLING nor a READY turn: no flicker per shot. A charge shot's own cooldown (design.n27.chargeShots27) drains the ring with the icon at full
// strength, because the normal shot still fires.
// The reads (view only, Hud30Reads: one place): builder S's Loadout.CooldownStepsOf (every gun, holstered too), Rules30.FullOf (the ring's whole),
// Loadout.Switching30 (Rules30's queued switch or the swap in progress; NIGHT29's Lockout29.queuedSwitch with the rules row off), PulseCharge27 (pips,
// cooldownSteps), ChargeShots27 (charging, Level, cooldown).
// The pure rules are Slot30Rules (EditMode tests: Hud30Tests). Written on change only; the strings are cached constants or cached seconds.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public enum Slot30 { Ready, Cooling, Charging }

    /// <summary>One slot's look, from the rules (pure).</summary>
    public struct Slot30Look
    {
        public Slot30 state;
        public float ring; // 0..1: the ring's sweep (READY 1, COOLING the share left, CHARGING the level)
        public float icon; // the icon's alpha
        public string word; // the state's word (null: none; the view adds READY / SWITCHING / the rest label)
        public int ticks; // CHARGING ticks on the ring (0: none)
        public bool waiting; // the slot cannot act yet (COOLING, or the BLAST slot's pips): a later READY is a "turn"
        public bool refill; // NIGHT36 F (tap36): the BLAST slot only refilling its pips: the word REFILL and the state's name REFILL (not CHARGING)
    }

    /// <summary>NIGHT30 item 2's rules: what each slot shows from the sim's numbers (pure: no Unity types).</summary>
    public static class Slot30Rules
    {
        /// <summary>The seconds word while over Hud30SecondsOver ("1.8"), else null.</summary>
        public static string Seconds(double s) => s > UnityTuning.Hud30SecondsOver ? s.ToString("0.0", CultureInfo.InvariantCulture) : null;

        /// <summary>A cooldown long enough to show (its full length at least Hud30MinCoolSeconds).</summary>
        public static bool Shows(int fullSteps, double dt) => fullSteps * dt >= UnityTuning.Hud30MinCoolSeconds - 1e-9;

        static Slot30Look Ready() => new Slot30Look { state = Slot30.Ready, ring = 1f, icon = 1f };

        /// <summary>A gun: charging (the charge shot held), else its cooldown, else its charge shot's cooldown, else READY.</summary>
        public static Slot30Look Gun(int left, int full, double dt, bool charging, double level, int chargeLeft, int chargeFull)
        {
            if (charging) return new Slot30Look { state = Slot30.Charging, ring = Clamp01((float)level), icon = 1f, word = UnityTuning.Hud30WordCharging, ticks = UnityTuning.Hud30ChargeTicks };
            if (left > 0 && Shows(Math.Max(full, left), dt))
                return new Slot30Look { state = Slot30.Cooling, ring = Share(left, full), icon = UnityTuning.Hud30CoolIconAlpha, word = Seconds(left * dt), waiting = true };
            if (chargeLeft > 0 && Shows(Math.Max(chargeFull, chargeLeft), dt))
                return new Slot30Look { state = Slot30.Cooling, ring = Share(chargeLeft, chargeFull), icon = 1f, word = Seconds(chargeLeft * dt), waiting = true };
            return Ready();
        }

        /// <summary>The gauntlet's PUNCH slot: the heavy swing's cooldown (it runs in the holster too: DESIGN30 A4).</summary>
        public static Slot30Look Punch(int left, int full, double dt) => Gun(left, full, dt, false, 0, 0, 0);

        /// <summary>The gauntlet's BLAST slot: after a blast its cooldown (COOLING with the seconds; the lead's round-1 finding: a blast empties the glove, so
        /// the cooldown comes first, as S's Loadout.Gauntlet30Now reads it), else the pips fill (CHARGING "2/5", five ticks), else READY.</summary>
        public static Slot30Look Blast(int pips, int maxPips, int cdLeft, int cdFull, double dt)
        {
            if (cdLeft > 0)
                return new Slot30Look { state = Slot30.Cooling, ring = Share(cdLeft, cdFull), icon = UnityTuning.Hud30CoolIconAlpha, word = Seconds(cdLeft * dt) ?? PipWord(pips, maxPips), waiting = true };
            if (maxPips > 0 && pips < maxPips)
                return new Slot30Look { state = Slot30.Charging, ring = Clamp01(pips / (float)maxPips), icon = UnityTuning.Hud30PipsIconAlpha, word = PipWord(pips, maxPips), ticks = maxPips, waiting = true };
            return Ready();
        }

        static readonly Dictionary<int, string> PipWords = new Dictionary<int, string>();

        /// <summary>"2/5" (cached per pair).</summary>
        public static string PipWord(int pips, int max)
        {
            int key = pips * 64 + max;
            if (!PipWords.TryGetValue(key, out string s)) PipWords[key] = s = pips + "/" + max;
            return s;
        }

        /// <summary>A READY turn: the slot was waiting (COOLING, or the BLAST pips) and is READY now. A charge shot's release is not a turn.</summary>
        public static bool TurnedReady(bool wasWaiting, Slot30 now) => wasWaiting && now == Slot30.Ready;

        /// <summary>The word a slot shows: SWITCHING (the queued slot) over READY (for Hud30ReadyWordSeconds after a turn) over the state's word over the rest label.</summary>
        public static string Word(bool switching, double sinceReady, Slot30Look look, string restLabel)
        {
            if (switching) return UnityTuning.Hud30WordSwitching;
            if (look.state == Slot30.Ready && sinceReady >= 0 && sinceReady < UnityTuning.Hud30ReadyWordSeconds) return UnityTuning.Hud30WordReady;
            return look.word ?? restLabel;
        }

        /// <summary>The state's name for the reels and the report ("READY", "COOLING", "CHARGING").</summary>
        public static string Name(Slot30 s) => s == Slot30.Ready ? "READY" : s == Slot30.Cooling ? "COOLING" : "CHARGING";

        static float Share(int left, int full) => full > 0 ? Clamp01(left / (float)full) : 1f;
        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }

    /// <summary>The reads the widget needs, in one place: builder S's public Loadout reads (Core/Guns/Rules30.cs, item 1).</summary>
    public static class Hud30Reads
    {
        /// <summary>Steps of `w`'s cooldown left (its own attack interval; it ticks in the holster): Loadout.CooldownStepsOf.</summary>
        public static int CooldownSteps(Loadout lo, string w) => lo == null || string.IsNullOrEmpty(w) ? 0 : lo.CooldownStepsOf(w);

        /// <summary>The full cooldown `w` last set (Rules30.FullOf; 0 with design.n30.rules off or none yet: the view learns it then).</summary>
        public static int FullSteps(Loadout lo, string w) => lo != null && lo.rules30 != null ? lo.rules30.FullOf(w) : 0;

        /// <summary>The owned slot a switch is going to: Rules30's queued ask or the swap in progress (Loadout.Switching30), else NIGHT29's lockout queue; -1 none.</summary>
        public static int SwitchingSlot(Loadout lo)
        {
            if (lo == null) return -1;
            int s = lo.Switching30;
            if (s < 0 && lo.lockout29 != null) s = lo.lockout29.queuedSwitch;
            return s == lo.slot ? -1 : s;
        }
    }

    public sealed partial class Ui
    {
        enum Slot30Kind { Gun, Punch, Blast }

        sealed class Slot30View
        {
            public RectTransform rt, body;
            public UiRing20 glow, disc, track, fill, ticks;
            public UnityEngine.UI.Image icon, handBar;
            public TextMeshProUGUI word;
            public Slot30Kind kind;
            public string id; // the gun id (the gauntlet's for PUNCH / BLAST)
            public int owned = -1; // index in loadout.owned
            // shown state (written on change only)
            public int state = -1, tickCount = -1, glowLeft;
            public float ring = -1, iconAlpha = -1, flashShown = -1;
            public string wordShown;
            public bool hand, handShown, wasWaiting;
            public double sinceReady = -1, flashAge = double.MaxValue;
            public Color fillColor;
        }

        RectTransform w30Row;
        readonly List<Slot30View> w30Slots = new List<Slot30View>();
        readonly List<(Slot30Kind kind, string id, int owned)> w30Defs = new List<(Slot30Kind, string, int)>();
        readonly Dictionary<string, int> w30Full = new Dictionary<string, int>(); // the longest cooldown seen since it last reached 0 (the ring's whole), per gun
        readonly Dictionary<string, int> w30Prev = new Dictionary<string, int>();
        float w30RowWidth;
        int w30First = -1, w30Count = -1;

        /// <summary>What the widget draws now (the reels read these). Per shown slot: its id ("revolver", "punch", "blast"...), state and word.</summary>
        public static readonly List<string> Weapon30Ids = new List<string>(), Weapon30States = new List<string>(), Weapon30Words = new List<string>();
        public static int Weapon30ReadyTurns;
        public static string Weapon30BlastState = "", Weapon30BlastWord = "", Weapon30PunchState = "";

        static float Weapons30RowHeight => UnityTuning.Hud30WordH + UnityTuning.Hud30WordGap + UnityTuning.Hud30Raise + 2 * UnityTuning.Hud30RingOuter + UnityTuning.Hud30RowPad;

        /// <summary>The widget's state as one line ("revolver READY | shotgun COOLING 1.8 | punch READY | blast CHARGING 2/5").</summary>
        public static string Weapon30Describe()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Weapon30Ids.Count; i++)
            {
                if (i > 0) sb.Append(" | ");
                sb.Append(Weapon30Ids[i]).Append(' ').Append(i < Weapon30States.Count ? Weapon30States[i] : "?");
                if (i < Weapon30Words.Count && !string.IsNullOrEmpty(Weapon30Words[i])) sb.Append(" '").Append(Weapon30Words[i]).Append('\'');
            }
            return sb.ToString();
        }

        void BuildWeapons30()
        {
            if (weaponBlock == null) return;
            w30Row = Hud17Rect(weaponBlock, "Weapon state 30", new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, Weapons30RowHeight));
            for (int i = 0; i < UnityTuning.Hud30MaxSlots; i++) w30Slots.Add(BuildSlot30(i));
            game.OnNewRun += () =>
            {
                w30Full.Clear();
                w30Prev.Clear();
                w30First = w30Count = -1;
                foreach (Slot30View v in w30Slots) { v.state = -1; v.sinceReady = -1; v.wasWaiting = false; v.flashAge = double.MaxValue; v.glowLeft = 0; v.id = null; }
                Weapon30ReadyTurns = 0;
            };
        }

        Slot30View BuildSlot30(int i)
        {
            float rOut = UnityTuning.Hud30RingOuter, rIn = rOut - UnityTuning.Hud30RingBand;
            var v = new Slot30View();
            v.rt = Hud17Rect(w30Row, "Slot " + i, new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(UnityTuning.Hud30SlotPitch, Weapons30RowHeight));
            v.body = Hud17Rect(v.rt, "Body", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, UnityTuning.Hud30WordH + UnityTuning.Hud30WordGap), new Vector2(2 * rOut, 2 * rOut));
            int steps = UnityTuning.Hud30RingSteps;
            v.glow = Ring30(v.body, "Glow", UiTheme.A(UiTheme.CyanHot, 0.85f), rOut, rOut + UnityTuning.Hud30GlowGrow, steps);
            v.glow.enabled = false;
            v.disc = Ring30(v.body, "Disc", UiTheme.InkSolid, 0, rIn, steps);
            v.track = Ring30(v.body, "Track", UiTheme.InkSoft, rIn, rOut, steps);
            v.fill = Ring30(v.body, "Fill", UiTheme.Cyan, rIn, rOut, steps);
            v.ticks = Ring30(v.body, "Ticks", UiTheme.InkSolid, rIn - 1, rOut + 1, steps);
            v.ticks.enabled = false;
            v.icon = Img(v.body, "Icon", UiTheme.Cyan, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud30IconW, UnityTuning.Hud30IconH));
            v.icon.preserveAspect = true;
            v.icon.raycastTarget = false;
            v.handBar = Img(v.rt, "Hand bar", UiTheme.Cyan, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, UnityTuning.Hud30WordH + UnityTuning.Hud30WordGap + 2f), new Vector2(UnityTuning.Hud30HandBarW, UnityTuning.Hud30HandBarH));
            v.handBar.rectTransform.pivot = new Vector2(0.5f, 0);
            v.handBar.raycastTarget = false;
            v.handBar.enabled = false;
            v.word = Hud17Text(v.rt, "Word", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(UnityTuning.Hud30WordBox, UnityTuning.Hud30WordH), true);
            v.word.enableAutoSizing = true;
            v.word.fontSizeMax = UiTheme.Px(UiTheme.Size.S18);
            v.word.fontSizeMin = 16f; // CLAUDE.md rule 8: UI text at least 16 px at 1080p
            v.word.overflowMode = TextOverflowModes.Truncate;
            FitWord32(v.word); // NIGHT32 G (hud32): "CHARGIN" -> the box at the slot's pitch, overflow instead of a cut (UiHud32.cs)
            v.rt.gameObject.SetActive(false);
            return v;
        }

        static UiRing20 Ring30(Transform parent, string name, Color c, float rIn, float rOut, int steps)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiRing20));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(2 * rOut, 2 * rOut);
            var ring = go.GetComponent<UiRing20>();
            ring.color = c;
            ring.raycastTarget = false;
            ring.SetRadii(rIn, rOut);
            ring.Steps = steps;
            return ring;
        }

        /// <summary>The slots this frame: each owned gun, the gauntlet as PUNCH + BLAST while the glove's pips are live.</summary>
        void Weapons30Defs(Loadout lo, bool pulseLive)
        {
            w30Defs.Clear();
            for (int i = 0; i < lo.owned.Count; i++)
            {
                string id = lo.owned[i];
                if (id == "gauntlet" && pulseLive)
                {
                    w30Defs.Add((Slot30Kind.Punch, id, i));
                    w30Defs.Add((Slot30Kind.Blast, id, i));
                }
                else w30Defs.Add((Slot30Kind.Gun, id, i));
            }
        }

        /// <summary>A gun's cooldown left, and the longest seen since it last reached 0 (the ring's whole: the view learns it, no number typed).</summary>
        int Cool30(Loadout lo, string id, out int full)
        {
            int left = Hud30Reads.CooldownSteps(lo, id);
            w30Prev.TryGetValue(id, out int prev);
            w30Full.TryGetValue(id, out full);
            if (left > prev || left > full) full = left; // a new cooldown started (it only ever counts down otherwise)
            if (left <= 0) full = 0;
            int ruled = Hud30Reads.FullSteps(lo, id); // Rules30's own record of the full length, when the rules row is on
            if (left > 0 && ruled >= left) full = ruled;
            w30Full[id] = full;
            w30Prev[id] = left;
            return left;
        }

        void RenderWeapons30(double frameDt)
        {
            if (w30Row == null) return;
            Weapon30Ids.Clear();
            Weapon30States.Clear();
            Weapon30Words.Clear();
            Weapon30BlastState = Weapon30BlastWord = Weapon30PunchState = "";
            if (!values.gameObject.activeSelf || game.loadout == null || game.sim == null) return;
            Loadout lo = game.loadout;
            SurvivalSim sim = game.sim;
            double dt = sim.step;
            PulseCharge27 pc = sim.pulse27;
            bool pulseLive = sim.n27.pulse27 && pc != null && pc.active;
            ChargeShots27 cs = (sim.n27.chargeShots27 || sim.n31.charge) && lo.charge27 != null && lo.charge27.active ? lo.charge27 : null; // NIGHT31 W: the loadout's charge (n27's or n31.charge's)
            Weapons30Defs(lo, pulseLive);
            string inHand = lo.Weapon;
            int queued = Hud30Reads.SwitchingSlot(lo);

            // the window: at most Hud30MaxSlots, round the slot in hand (as the hotbar's)
            int n = w30Defs.Count, hand = 0;
            for (int i = 0; i < n; i++) if (w30Defs[i].id == inHand) { hand = i; break; }
            int shown = Math.Min(n, w30Slots.Count);
            int first = n <= shown ? 0 : Math.Max(0, Math.Min(n - shown, hand - shown / 2));
            bool layout = first != w30First || shown != w30Count;
            for (int i = 0; i < shown && !layout; i++)
            {
                var d = w30Defs[first + i];
                if (w30Slots[i].id != d.id || w30Slots[i].kind != d.kind) layout = true;
            }
            if (layout) LayoutWeapons30(first, shown);

            // the guns' cooldowns are read once per gun per frame (Cool30 learns each ring's whole)
            for (int i = 0; i < shown; i++)
            {
                Slot30View v = w30Slots[i];
                Slot30Look look;
                string rest = null;
                if (v.kind == Slot30Kind.Blast)
                {
                    look = Slot30Rules.Blast(pc.pips, pc.p.pips, pc.cooldownSteps, pc.BlastCooldownSteps, dt);
                    rest = UnityTuning.Hud30WordBlast;
                    if (sim.n36.tap36 && look.state == Slot30.Charging) { look.refill = true; look.word = UnityTuning.Hud36WordRefill; } // NIGHT36 F: pips refilling are not a charge
                }
                else
                {
                    int left = Cool30(lo, v.id, out int full);
                    if (v.kind == Slot30Kind.Punch)
                    {
                        look = Slot30Rules.Punch(left, Math.Max(full, pc.SwingSteps), dt);
                        rest = UnityTuning.Hud30WordPunch;
                    }
                    else
                    {
                        int gi = cs != null ? ChargeShots27.Index(v.id) : -1;
                        bool charging = gi >= 0 && cs.charging && v.id == inHand;
                        look = Slot30Rules.Gun(left, full, dt, charging, charging ? cs.Level(gi) : 0, gi >= 0 ? cs.cooldown[gi] : 0, gi >= 0 ? cs.CooldownSteps(gi) : 0);
                    }
                }
                // NIGHT31 W ltAlways (design.n31.ltAlways, R6): the slot names its LT (ZOOM / DETONATE; the gauntlet BLAST / PARRY) as its rest label; the
                // gauntlet's parry cooldown is COOLING with seconds; before full pips the PARRY word replaces the pip count (the five ticks keep it)
                if (lo.lt31 != null && lo.lt31.active && v.kind != Slot30Kind.Punch)
                {
                    string ltWord = lo.LtLabel31(v.kind == Slot30Kind.Blast ? "gauntlet" : v.id);
                    if (ltWord.Length > 0)
                    {
                        if (v.id == "gauntlet" && lo.lt31.parryCooldown > 0 && !(look.state == Slot30.Cooling)) look = Slot30Rules.Punch(lo.lt31.parryCooldown, lo.lt31.CooldownSteps, dt);
                        else if (v.kind == Slot30Kind.Blast && ltWord == "PARRY" && !look.refill) look.word = null; // NIGHT36 tap36: REFILL stays while refilling
                        rest = ltWord;
                    }
                }
                bool inHandNow = v.id == inHand; // the gauntlet's two slots both stand raised while it is in hand
                bool switching = queued >= 0 && queued == v.owned && v.kind != Slot30Kind.Blast;
                RenderSlot30(v, look, rest, inHandNow, switching, frameDt);
                string id = v.kind == Slot30Kind.Punch ? "punch" : v.kind == Slot30Kind.Blast ? "blast" : v.id;
                Weapon30Ids.Add(id);
                Weapon30States.Add(look.refill ? UnityTuning.Hud36WordRefill : Slot30Rules.Name(look.state));
                Weapon30Words.Add(v.wordShown ?? "");
                if (v.kind == Slot30Kind.Blast) { Weapon30BlastState = look.refill ? UnityTuning.Hud36WordRefill : Slot30Rules.Name(look.state); Weapon30BlastWord = v.wordShown ?? ""; }
                if (v.kind == Slot30Kind.Punch) Weapon30PunchState = Slot30Rules.Name(look.state);
            }
        }

        void LayoutWeapons30(int first, int shown)
        {
            w30First = first;
            w30Count = shown;
            float pitch = UnityTuning.Hud30SlotPitch;
            for (int i = 0; i < w30Slots.Count; i++)
            {
                Slot30View v = w30Slots[i];
                bool on = i < shown;
                if (v.rt.gameObject.activeSelf != on) v.rt.gameObject.SetActive(on);
                if (!on) { v.id = null; continue; }
                var d = w30Defs[first + i];
                bool same = v.id == d.id && v.kind == d.kind;
                v.id = d.id;
                v.kind = d.kind;
                v.owned = d.owned;
                v.rt.anchoredPosition = new Vector2(-((shown - 1 - i) * pitch) - pitch * 0.5f, 0);
                if (same) continue;
                v.icon.sprite = WeaponSilhouette(d.id); // made once per gun (UiHotbar.cs)
                v.state = -1;
                v.tickCount = -1;
                v.ring = v.iconAlpha = -1;
                v.wordShown = null;
                v.word.SetText(string.Empty);
                v.sinceReady = -1;
                v.wasWaiting = false;
            }
            w30RowWidth = shown * pitch;
            if (w30Row != null) w30Row.sizeDelta = new Vector2(w30RowWidth, Weapons30RowHeight);
            weaponBlockDirty = true; // the card refits (FitWeaponBlock30, later this frame)
        }

        void RenderSlot30(Slot30View v, Slot30Look look, string rest, bool hand, bool switching, double frameDt)
        {
            // a READY turn: the word for 1 s, the gated flash, the one-frame glow, the event
            if (Slot30Rules.TurnedReady(v.wasWaiting, look.state) && v.state >= 0)
            {
                v.sinceReady = 0;
                Weapon30ReadyTurns++;
                if (game.settings.Num("flashIntensity") > 0 && game.flashGate.TryFlash(SurvivalGame.RealNow)) v.flashAge = 0;
                v.glowLeft = UnityTuning.Hud30GlowFrames;
                string evId = v.kind == Slot30Kind.Punch ? "punch" : v.kind == Slot30Kind.Blast ? "blast" : v.id;
                FeelProbe.Event(UnityTuning.Hud30ReadyEvent);
                Hud30.RaiseReady(evId);
            }
            else if (look.state != Slot30.Ready) v.sinceReady = -1;
            v.wasWaiting = look.waiting;

            int st = (int)look.state;
            if (st != v.state)
            {
                v.state = st;
                Color fc = look.state == Slot30.Cooling ? UiTheme.CyanDeep : UiTheme.Cyan;
                v.fill.color = fc;
                v.fillColor = fc;
            }
            if (look.ring != v.ring)
            {
                v.ring = look.ring;
                v.fill.Fill = look.ring;
            }
            if (look.ticks != v.tickCount)
            {
                v.tickCount = look.ticks;
                v.ticks.enabled = look.ticks > 0;
                if (look.ticks > 0)
                {
                    float span = 360f / look.ticks;
                    v.ticks.SetGaps(look.ticks, span - UnityTuning.Hud30TickDeg, -span * 0.5f);
                }
            }
            if (look.icon != v.iconAlpha)
            {
                v.iconAlpha = look.icon;
                v.icon.color = UiTheme.A(look.state == Slot30.Ready ? UiTheme.CyanHot : UiTheme.Cyan, look.icon);
            }

            string word = Slot30Rules.Word(switching, v.sinceReady, look, rest);
            if (word != v.wordShown)
            {
                v.wordShown = word;
                v.word.SetText(word ?? string.Empty);
                v.word.color = switching ? UiTheme.CyanHot
                    : ReferenceEquals(word, UnityTuning.Hud30WordReady) ? UiTheme.CyanHot
                    : ReferenceEquals(word, rest) ? UiTheme.Muted
                    : look.state == Slot30.Cooling ? UiTheme.CyanPale : UiTheme.White;
            }
            if (v.sinceReady >= 0)
            {
                v.sinceReady += frameDt;
                if (v.sinceReady >= UnityTuning.Hud30ReadyWordSeconds + 0.05) v.sinceReady = -1;
            }

            // in hand: raised, the bar under it
            if (hand != v.handShown)
            {
                v.handShown = hand;
                v.body.anchoredPosition = new Vector2(0, UnityTuning.Hud30WordH + UnityTuning.Hud30WordGap + (hand ? UnityTuning.Hud30Raise : 0f));
                v.handBar.enabled = hand;
            }

            // the flash (0.15 s on the disc, x FLASH INTENSITY) and the one-frame glow
            float flash = 0f;
            if (v.flashAge < UnityTuning.Hud30ReadyFlash)
            {
                flash = (float)(1.0 - v.flashAge / UnityTuning.Hud30ReadyFlash) * Mathf.Clamp01((float)game.settings.Num("flashIntensity"));
                v.flashAge += frameDt;
            }
            if (flash != v.flashShown)
            {
                v.flashShown = flash;
                v.disc.color = Color.Lerp(UiTheme.InkSolid, UiTheme.CyanHot, flash);
            }
            bool glow = v.glowLeft > 0;
            if (v.glow.enabled != glow) v.glow.enabled = glow;
            if (glow) v.glowLeft--;
        }
    }
}
