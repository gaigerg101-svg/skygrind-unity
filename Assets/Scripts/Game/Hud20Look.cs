// NIGHT20 U3-34.10 (builder H): the pure rules of tonight's HUD (Hud20Tests): the shockwave chip's ring, seconds and ready
// flash; the zoom reticle's kind and size; the HP bar's segment ticks and the regen's reach; the round-end refill's promise;
// the special hint's zoom word and the pad's cycle line from the live binding. No Unity objects, no allocation per call
// (the word caches fill once per value).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The shockwave chip (UiCooldown20.cs): a ring round the alt-fire glyph on the THUNDERCLAP's card.</summary>
    public static class ShockRing20
    {
        /// <summary>The ring's fill 0..1: whole while every charge is back; else the next charge's progress (1 - left / cooldown).</summary>
        public static float Fill(int charges, int max, double left, double cooldown)
        {
            if (charges >= max || cooldown <= 0) return 1f;
            return UiTween.Clamp01(1 - left / cooldown);
        }

        /// <summary>At least one charge: LT fires the shockwave.</summary>
        public static bool Ready(int charges) => charges > 0;

        /// <summary>The whole seconds shown inside the ring (rounded up), or -1: only while no charge is ready and more than
        /// Hud20ShockSecondsAbove s are left (under it the glyph comes back: get ready).</summary>
        public static int Seconds(int charges, double left)
        {
            if (charges > 0 || !(left > UnityTuning.Hud20ShockSecondsAbove)) return -1;
            return (int)Math.Ceiling(left - 1e-9);
        }

        /// <summary>A charge came back since the last read (the ready flash). Never on the first read (`prev` &lt; 0: a run's
        /// start, a buy), never on a shot.</summary>
        public static bool ChargeBack(int prev, int now) => prev >= 0 && now > prev;

        /// <summary>The ready flash's strength `age` s in: 1 -> 0 over Hud20ShockFlash (ease-out), x FLASH INTENSITY.</summary>
        public static float Flash(double age, double intensity)
        {
            if (age < 0 || age >= UnityTuning.Hud20ShockFlash) return 0f;
            float k = 1f - UiTween.OutCubic(age / UnityTuning.Hud20ShockFlash);
            return k * UiTween.Clamp01(intensity);
        }

        /// <summary>The chip's scale `age` s after a charge came back: Hud20ShockPunch -> 1 (ease-out); 1 with REDUCED MOTION.</summary>
        public static float Punch(double age, bool reducedMotion)
        {
            if (reducedMotion || age < 0 || age >= UnityTuning.Hud20ShockPunchTime) return 1f;
            return 1f + (UnityTuning.Hud20ShockPunch - 1f) * (1f - UiTween.OutCubic(age / UnityTuning.Hud20ShockPunchTime));
        }
    }

    /// <summary>Where tonight's card parts sit (canvas px at 1080p, origin bottom left; Hud19Layout's frame).</summary>
    public static class Hud20Layout
    {
        /// <summary>The shockwave chip's tab: standing on the weapon card's top edge, centred over slot `index` of the `visible`
        /// right-aligned slots (UiHotbar.LayoutHotbar), as wide as a slot or the chip and its padding.</summary>
        public static HudRect ChipTab(int index, int visible, float width)
        {
            float step = UnityTuning.Hud17StripCellW + UnityTuning.Hud17StripGap;
            float right = width - UnityTuning.ThemeSafeInset - UnityTuning.ThemeAccentBar - UnityTuning.Hud17StripPad;
            float centre = right - (visible - 1 - index) * step - UnityTuning.Hud17StripCellW * 0.5f;
            float side = UnityTuning.Hud20ShockChipD + 2 * UnityTuning.Hud20ShockTabPad;
            float w = Math.Max(UnityTuning.Hud17StripCellW, side);
            return new HudRect(centre - w * 0.5f, Hud19Layout.CardBottom + Hud19Layout.CardHeight, w, side);
        }

        /// <summary>The threat bar's block (its name's line over the bar), bottom centre: as wide as the boss's / elite's bar,
        /// narrowed so that under HUD SCALE `hudScale` (the layer scales about the bottom centre) it keeps Hud20ThreatSideGap px
        /// from the vitals plate.</summary>
        public static HudRect ThreatBar(float width, bool boss, float hudScale)
        {
            float s = hudScale > 0 ? hudScale : 1f;
            float vitalsRight = UnityTuning.ThemeSafeInset + UnityTuning.Hud17VitalsWidth;
            float half = (width * 0.5f - (vitalsRight * s + UnityTuning.Hud20ThreatSideGap)) / s;
            float w = Math.Max(0f, Math.Min(boss ? UnityTuning.Hud20ThreatBossW : UnityTuning.Hud20ThreatEliteW, 2 * half));
            float h = UnityTuning.Hud20ThreatNameH + UnityTuning.Hud20ThreatNameGap + (boss ? UnityTuning.Hud20ThreatBossH : UnityTuning.Hud20ThreatEliteH);
            return new HudRect(width * 0.5f - w * 0.5f, UnityTuning.Hud20ThreatBottom, w, h);
        }
    }

    /// <summary>The zoom's reticle (UiZoom20.cs): none, a ring (1.25 / 1.5) or the scope (3.5).</summary>
    public static class ZoomReticle20
    {
        public const int None = 0, Ring = 1, Scope = 2;

        /// <summary>By the gun's magnification: none unzoomed, the scope from Hud20ScopeFromMag, else the ring.</summary>
        public static int Kind(double mag) => !(mag > 1.0001) ? None : mag >= UnityTuning.Hud20ScopeFromMag ? Scope : Ring;

        /// <summary>The ring's radius in px: Hud20ReticleR0 / magnification (the tighter zoom, the tighter ring).</summary>
        public static float Radius(double mag) => mag > 0 ? (float)(UnityTuning.Hud20ReticleR0 / mag) : 0f;

        /// <summary>The centre dot: the precise zoom only (the KESTREL's 1.5, not the STITCHER's 1.25).</summary>
        public static bool Dot(double mag) => mag >= UnityTuning.Hud20ReticleDotFromMag;

        /// <summary>The reticle's alpha: the zoom's eased amount (Viewmodel.AdsAmount), clamped.</summary>
        public static float Alpha(double amount) => UiTween.Clamp01(amount);

        /// <summary>The ring's scale: Hud20ReticleFrom at the zoom's start, closing to 1 (ease-out on the amount); 1 with REDUCED MOTION.</summary>
        public static float Scale(double amount, bool reducedMotion) =>
            reducedMotion ? 1f : 1f + (UnityTuning.Hud20ReticleFrom - 1f) * (1f - UiTween.OutCubic(amount));

        /// <summary>The lens' scale: Hud20ScopeFrom at the scope's start, settling to 1; 1 with REDUCED MOTION.</summary>
        public static float ScopeScale(double amount, bool reducedMotion) =>
            reducedMotion ? 1f : 1f + (UnityTuning.Hud20ScopeFrom - 1f) * (1f - UiTween.OutCubic(amount));

        /// <summary>The lens' radius in canvas px (a share of the canvas height).</summary>
        public static float ScopeRadius(float canvasHeight) => canvasHeight * UnityTuning.Hud20ScopeRadiusShare;

        /// <summary>The mask's outer radius: past every corner of the canvas (with the entrance's scale in hand).</summary>
        public static float MaskOuter(float canvasWidth, float canvasHeight) =>
            (float)Math.Sqrt(canvasWidth * canvasWidth + canvasHeight * canvasHeight) * 0.5f * UnityTuning.Hud20ScopeFrom + 8f;
    }

    /// <summary>The HP bar's segments (UiVitals20.cs): a tick every segment's HP, the regen's reach.</summary>
    public static class HpSegments20
    {
        /// <summary>Ticks inside the bar: one at every segment's end under `max` (at most Hud20MaxTicks); 0 without segments.</summary>
        public static int Ticks(double max, double segment)
        {
            if (!(segment > 0) || !(max > segment)) return 0;
            int n = (int)Math.Ceiling(max / segment - 1e-9) - 1;
            return Math.Max(0, Math.Min(UnityTuning.Hud20MaxTicks, n));
        }

        /// <summary>Tick i's place along the bar (0..1).</summary>
        public static float TickAt(int i, double max, double segment) => max > 0 ? (float)((i + 1) * segment / max) : 0f;

        /// <summary>Where the in-round regen stops: the top of the segment `hp` is in (a tick when on one), never past `cap`
        /// (ULTRA HEALTH); 0 when down; the cap without segments.</summary>
        public static double Top(double hp, double cap, double segment)
        {
            if (hp <= 0) return 0;
            if (!(segment > 0)) return cap;
            double top = Math.Ceiling(hp / segment - 1e-9) * segment;
            return Math.Min(cap, top);
        }

        /// <summary>The highest tick the fill reached or crossed going up from `before` to `after`, or -1 (a hit: none).</summary>
        public static int Crossed(float before, float after, int ticks, double max, double segment)
        {
            if (!(after > before)) return -1;
            int hit = -1;
            for (int i = 0; i < ticks; i++)
            {
                float at = TickAt(i, max, segment);
                if (at > before + 1e-6f && at <= after + 1e-6f) hit = i;
            }
            return hit;
        }
    }

    /// <summary>The round-end refill's promise on the HP bar (UiVitals20.cs): it enters with the event, holds while the bar
    /// climbs, lingers Hud20RefillLinger once the bar arrived (or the refill's seconds ran out), then fades.</summary>
    public sealed class Refill20
    {
        double age = -1, seconds, arrivedAt = -1;
        float target;

        public bool Active => age >= 0;
        public float Target => target;

        public void Start(double toFrac, double seconds)
        {
            target = UiTween.Clamp01(toFrac);
            this.seconds = Math.Max(0, seconds);
            age = 0;
            arrivedAt = -1;
        }

        /// <summary>Frame time; `fracNow` the bar's fill this frame. The first step after Start is drawn as its first frame.</summary>
        public void Step(double frameDt, double fracNow)
        {
            if (age < 0) return;
            age = HudEntrance18.Step(age, frameDt);
            if (arrivedAt < 0 && (fracNow >= target - 0.002 || age >= seconds)) arrivedAt = age;
            if (arrivedAt >= 0 && age - arrivedAt >= UnityTuning.Hud20RefillLinger + UnityTuning.Hud20RefillFade) age = -1;
        }

        /// <summary>The promise's alpha this frame.</summary>
        public float Alpha
        {
            get
            {
                if (age < 0) return 0f;
                float a = UiTween.OutCubic(age / UnityTuning.Hud20RefillIn);
                if (arrivedAt >= 0)
                {
                    double t = age - arrivedAt - UnityTuning.Hud20RefillLinger;
                    if (t > 0) a *= 1f - UiTween.InCubic(t / UnityTuning.Hud20RefillFade);
                }
                return a;
            }
        }

        public void Reset()
        {
            age = -1;
            arrivedAt = -1;
        }
    }

    /// <summary>
    /// F1: the run beats' slabs (UiAnnouncer20.cs). The table of run events -> slabs (the events keep their names: the web's or
    /// NIGHT20's port-only ones, for GG's audio map): `specialRound` by name and phase (E's special rounds: the boss, THE
    /// HUNT, THE ELITE, the twists), `swarmRound`, `exfilCall`, `exfilStart`, `powerOn`, and LAST ONE from the director's
    /// count. Priorities: the boss first (it cuts in), then the special rounds, exfil, LAST ONE, the rest.
    /// </summary>
    public static class Announcer20
    {
        public const int Boss = 0, Special = 1, Exfil = 2, Last = 3, Info = 4;

        public const string BossInbound = "BOSS INBOUND", BossDown = "BOSS DOWN", TheHunt = "THE HUNT", EliteInbound = "ELITE INBOUND";
        public const string LastOne = "LAST ONE", ExfilOpen = "EXFIL OPEN", ExfilCalled = "EXFIL CALLED", TheSwarm = "THE SWARM";
        public const string PowerOn = "POWER ON", FogRound = "FOG ROUND", Swarmageddon = "SWARMAGEDDON", LethalRound = "LETHAL ROUND", GoldenRound = "GOLDEN ROUND";

        /// <summary>Every fixed slab (the strings test).</summary>
        public static readonly List<string> AllSlabs = new List<string>
        {
            BossInbound, BossDown, TheHunt, EliteInbound, LastOne, ExfilOpen, ExfilCalled, TheSwarm, PowerOn, FogRound, Swarmageddon, LethalRound, GoldenRound,
        };

        static readonly Dictionary<string, string> named = new Dictionary<string, string>();
        static readonly Dictionary<string, string> twistIds = new Dictionary<string, string>();

        /// <summary>A5's twist rounds' names: "twist:<id>".</summary>
        public static bool IsTwist(string name) => name != null && name.StartsWith("twist:", StringComparison.Ordinal);

        /// <summary>A twist's id ("twist:fog" -> "fog"), cached; the name itself when it isn't a twist's.</summary>
        public static string TwistId(string name)
        {
            if (!IsTwist(name)) return name;
            if (!twistIds.TryGetValue(name, out string id)) twistIds[name] = id = name.Substring(6);
            return id;
        }

        /// <summary>A threat's slab wears DANGER's accent (enemies and danger only: CLAUDE.md rule 8); the player's beats CYAN.</summary>
        public static bool Danger(string slab) =>
            slab == BossInbound || slab == TheHunt || slab == EliteInbound || slab == LastOne || slab == TheSwarm
            || slab == FogRound || slab == Swarmageddon || slab == LethalRound;

        /// <summary>A special round's slab and its priority, or null (no slab for that phase). Phases: "announce" names the round
        /// (in the break, or as an elite appears: "start" too for the elite), "last" is LAST ONE, "end" names a boss's fall.</summary>
        public static string ForSpecial(SpecialRoundEvent e, out int priority)
        {
            priority = Info;
            if (e == null || string.IsNullOrEmpty(e.name)) return null;
            string n = e.name, ph = e.phase;
            // A5's twists arrive as twist:<id> (FOG, SWARMAGEDDON, LETHAL, GOLDEN): named at their announce or start
            bool twist = IsTwist(n);
            if (twist)
            {
                n = TwistId(n);
                if (ph == "start") ph = "announce";
            }
            bool boss = n == "boss" || n == "bulwark";
            if (ph == "last")
            {
                priority = Last;
                return LastOne;
            }
            if (ph == "end")
            {
                if (!boss) return null;
                priority = Special;
                return BossDown;
            }
            bool announce = ph == "announce" || ph == null;
            if (boss && (announce || ph == "start"))
            {
                priority = Boss;
                return BossInbound;
            }
            if (n == "elite" && (announce || ph == "start"))
            {
                priority = Special;
                return EliteInbound;
            }
            if (!announce) return null;
            priority = Special;
            switch (n)
            {
                case "hunt": return TheHunt;
                case "fog": return FogRound;
                case "swarmageddon": return Swarmageddon;
                case "lethal": return LethalRound;
                case "golden": return GoldenRound;
            }
            // a name nobody tabled: itself in caps when it's short (cached once per name), else no slab
            if (named.TryGetValue(n, out string s)) return s;
            string up = n.ToUpperInvariant();
            s = up.Length <= 16 && up.Split(' ').Length <= 3 ? up : null;
            named[n] = s;
            if (s == null) priority = Info;
            return s;
        }
    }

    /// <summary>F1: the slabs' priority queue (one at a time, one start per Hud20SlabGap, a boss cuts in, stale beats dropped
    /// after Hud20SlabMaxWait, twins merged). Fixed arrays: no allocation per push.</summary>
    public sealed class AnnouncerQueue20
    {
        readonly string[] texts = new string[UnityTuning.Hud20SlabQueue];
        readonly int[] prios = new int[UnityTuning.Hud20SlabQueue];
        readonly double[] times = new double[UnityTuning.Hud20SlabQueue];
        int count;
        string showing;
        int showPrio = Announcer20.Info;
        double startedAt = double.NegativeInfinity;

        /// <summary>The slab on screen (null: none).</summary>
        public string Showing => showing;
        public int ShowingPriority => showPrio;
        public int Queued => count;
        /// <summary>When the slab on screen (or the last one) started.</summary>
        public double StartedAt => startedAt;

        public void Push(string text, int priority, double now)
        {
            if (string.IsNullOrEmpty(text) || text == showing) return;
            for (int i = 0; i < count; i++)
            {
                if (texts[i] != text) continue;
                if (priority < prios[i]) prios[i] = priority;
                return;
            }
            int at = count;
            if (count == texts.Length)
            {
                // full: the worst beat (the lowest priority, the newest of those) gives way to a better one
                int worst = 0;
                for (int i = 1; i < count; i++)
                    if (prios[i] > prios[worst] || (prios[i] == prios[worst] && times[i] >= times[worst])) worst = i;
                if (priority >= prios[worst]) return;
                at = worst;
            }
            else count++;
            texts[at] = text;
            prios[at] = priority;
            times[at] = now;
        }

        /// <summary>Advance to `now`: end the slab past its life, drop stale beats, start the next one; true when one started.</summary>
        public bool Step(double now)
        {
            if (showing != null && now - startedAt >= UnityTuning.Hud20SlabLife) showing = null;
            for (int i = count - 1; i >= 0; i--)
                if (prios[i] != Announcer20.Boss && now - times[i] > UnityTuning.Hud20SlabMaxWait) RemoveAt(i);
            if (count == 0) return false;
            int best = 0;
            for (int i = 1; i < count; i++)
                if (prios[i] < prios[best] || (prios[i] == prios[best] && times[i] < times[best])) best = i;
            bool cutIn = prios[best] == Announcer20.Boss && (showing == null || showPrio != Announcer20.Boss);
            if (!cutIn && (showing != null || now - startedAt < UnityTuning.Hud20SlabGap)) return false;
            showing = texts[best];
            showPrio = prios[best];
            startedAt = now;
            RemoveAt(best);
            return true;
        }

        void RemoveAt(int i)
        {
            count--;
            for (int k = i; k < count; k++)
            {
                texts[k] = texts[k + 1];
                prios[k] = prios[k + 1];
                times[k] = times[k + 1];
            }
            texts[count] = null;
        }

        public void Clear()
        {
            for (int i = 0; i < texts.Length; i++) texts[i] = null;
            count = 0;
            showing = null;
            showPrio = Announcer20.Info;
            startedAt = double.NegativeInfinity;
        }
    }

    /// <summary>F1: a slab's envelope: a SLAM (HudSlam18: from 1.6x at full alpha, ease-out), held, an ease-in fade from
    /// Hud20SlabFadeAt to Hud20SlabLife; WHITE-hot for its first Hud20SlabWhite s.</summary>
    public static class Slab20
    {
        public static float Scale(double age) => HudSlam18.Scale(Math.Max(0, age));

        public static float Alpha(double age)
        {
            if (age >= UnityTuning.Hud20SlabLife) return 0f;
            if (age <= UnityTuning.Hud20SlabFadeAt) return 1f;
            return 1f - UiTween.InCubic((age - UnityTuning.Hud20SlabFadeAt) / (UnityTuning.Hud20SlabLife - UnityTuning.Hud20SlabFadeAt));
        }

        public static bool White(double age) => age >= 0 && age < UnityTuning.Hud20SlabWhite;
    }

    /// <summary>F1: LAST ONE: once a round, when the director's count reaches 1 in a round that had Hud20LastOneFrom or more
    /// left at once (a round that starts with one isn't a "last one").</summary>
    public sealed class LastOne20
    {
        int round = -1, maxSeen;
        bool fired;

        public bool Step(int round, bool fight, int remaining)
        {
            if (round != this.round)
            {
                this.round = round;
                maxSeen = 0;
                fired = false;
            }
            if (!fight) return false;
            if (remaining > maxSeen) maxSeen = remaining;
            if (fired || remaining != 1 || maxSeen < UnityTuning.Hud20LastOneFrom) return false;
            fired = true;
            return true;
        }

        public void Reset()
        {
            round = -1;
            maxSeen = 0;
            fired = false;
        }
    }

    /// <summary>A8: the NEXT line in the break (UiNext20.cs): the nearest special round in words and five pips for the next
    /// rounds' kinds (Hades' doors, the backlog's A8).</summary>
    public static class NextLine20
    {
        public const int Pips = 5;

        /// <summary>A round's kind without E's read: the director's own RoundType (by the round number only: a seed replays it),
        /// a 'boss' round ordinary until a boss is ported (`boss`), HOLD / GOLD only while the objectives are live.</summary>
        public static string Kind(int round, DirectorMath.D d, bool objectives, bool boss)
        {
            string k = DirectorMath.RoundType(round, d);
            if (k == "boss" && !boss) return "normal";
            if ((k == "hold" || k == "gold") && !objectives) return "normal";
            return k;
        }

        /// <summary>A kind's word on the line (null: an ordinary round).</summary>
        public static string Word(string kind)
        {
            switch (Announcer20.TwistId(kind)) // A5: "twist:fog" reads as fog
            {
                case "hold": return "HOLD";
                case "gold": return "GOLD";
                case "swarm": return "SWARM";
                case "hunt": return "HUNT";
                case "boss": return "BOSS";
                case "elite": return "ELITE";
                case "exfil": return "EXFIL";
                case "fog": return "FOG";
                case "lethal": return "LETHAL";
                case "golden": return "GOLDEN";
                case "swarmageddon": return "SWARMAGEDDON";
                default: return null;
            }
        }

        static readonly Dictionary<string, string>[] lines = new Dictionary<string, string>[Pips + 1];
        public const string ExfilOpen = "EXFIL OPEN";

        /// <summary>The line: EXFIL OPEN while the radio can be called; else the nearest special in the next rounds ("NEXT: W"
        /// for the next one, "W IN k" further on); null when none is coming (the pips alone). Cached strings.</summary>
        public static string Line(IList<string> kinds, bool exfilOpen)
        {
            if (exfilOpen) return ExfilOpen;
            if (kinds == null) return null;
            for (int i = 0; i < kinds.Count && i < Pips; i++)
            {
                string w = Word(kinds[i]);
                if (w == null) continue;
                int k = i + 1;
                Dictionary<string, string> cache = lines[k] ??= new Dictionary<string, string>();
                if (!cache.TryGetValue(w, out string s)) cache[w] = s = k == 1 ? "NEXT: " + w : w + " IN " + k;
                return s;
            }
            return null;
        }

        /// <summary>A pip's tint: 0 ordinary (INK-SOFT), 1 an objective or the way out (CYAN), 2 a threat (DANGER), 3 the boss
        /// (DANGER, a taller pip).</summary>
        public static int Tint(string kind)
        {
            switch (Announcer20.TwistId(kind)) // A5: "twist:<id>" by its id
            {
                case "hold":
                case "gold":
                case "exfil":
                case "golden": return 1;
                case "swarm":
                case "hunt":
                case "elite":
                case "fog":
                case "lethal":
                case "swarmageddon": return 2;
                case "boss": return 3;
                default: return 0;
            }
        }
    }

    /// <summary>C1: the kinds' debut cards (UiDebut20.cs): the web's kinds with one counter each, from their own design notes
    /// (skygrind/src/enemies/*.js headers): once per profile, at a calm moment.</summary>
    public static class Debut20
    {
        static readonly Dictionary<string, (string name, string counter)> cards = new Dictionary<string, (string, string)>
        {
            ["swarmer"] = ("SWARMER", "KEEP MOVING"), // swarmer.js: "it punishes standing still"
            ["spitter"] = ("SPITTER", "PUNCH THE GLOBS BACK"), // the port's globReflect (Spitter.cs): a punch returns a glob
            ["charger"] = ("CHARGER", "SIDESTEP INTO WALLS"), // charger.js: "if it hits a wall it is stunned ... its back weak point exposed"
            ["drone"] = ("DRONE", "PUNCH THE ORBS BACK"), // drone.js: "a punch reflects them (a parry)"
            ["lancer"] = ("LANCER", "OUTRUN THE LASER"), // lancer.js: "its laser sight tracks you WITH LAG (... fast movement escapes it)"
            ["warden"] = ("WARDEN", "FLANK OR PUNCH IT"), // warden.js: "break it by flanking ... or by punching the shield"
            ["bulwark"] = ("BULWARK", "RAM IT INTO WALLS"), // bulwark.js: "RAM ... into a wall: stunned 3 s, a plate drops"
        };

        static readonly Dictionary<string, string> flags = new Dictionary<string, string>();

        public static bool Has(string kind) => kind != null && cards.ContainsKey(kind);

        /// <summary>Every card is seen in this profile (no more scans).</summary>
        public static bool AllSeen(Profile p)
        {
            if (p == null) return false;
            foreach (string k in cards.Keys)
                if (!p.Flag(Flag(k))) return false;
            return true;
        }
        public static string Name(string kind) => kind != null && cards.TryGetValue(kind, out var c) ? c.name : null;
        public static string Counter(string kind) => kind != null && cards.TryGetValue(kind, out var c) ? c.counter : null;

        /// <summary>The profile flag that marks the card seen (Profile.flags; cached per kind).</summary>
        public static string Flag(string kind)
        {
            if (kind == null) return null;
            if (!flags.TryGetValue(kind, out string f)) flags[kind] = f = "debut20." + kind;
            return f;
        }

        /// <summary>A card may show: unseen in this profile, nothing within Hud20DebutClear m, no boss intro, playing.</summary>
        public static bool CanShow(bool seen, double nearestEnemy, bool bossIntro, bool playing) =>
            !seen && playing && !bossIntro && nearestEnemy >= UnityTuning.Hud20DebutClear;

        /// <summary>The card's alpha: whole from its slam, an ease-in fade from Hud20DebutFadeAt to Hud20DebutLife.</summary>
        public static float Alpha(double age)
        {
            if (age >= UnityTuning.Hud20DebutLife) return 0f;
            if (age <= UnityTuning.Hud20DebutFadeAt) return 1f;
            return 1f - UiTween.InCubic((age - UnityTuning.Hud20DebutFadeAt) / (UnityTuning.Hud20DebutLife - UnityTuning.Hud20DebutFadeAt));
        }
    }

    /// <summary>The climb's off-screen arrow: a point pinned to the inset screen edge (UiClimbArrow20.cs). The usual
    /// off-screen indicator: a point behind the camera is flipped (it lies the other way), then the direction from the
    /// centre is scaled onto the inset rectangle's border; the chevron points along it (0 deg = up, counter-clockwise +).</summary>
    public static class EdgeArrow20
    {
        /// <summary>`p` the point from the screen's centre (canvas px, y up), `behind` it is behind the camera, `half` the
        /// screen's half size; false (no arrow) when it's on screen in front.</summary>
        public static bool Place(UnityEngine.Vector2 p, bool behind, UnityEngine.Vector2 half, float inset, out UnityEngine.Vector2 at, out float angle)
        {
            at = default;
            angle = 0;
            if (!behind && Math.Abs(p.x) <= half.x && Math.Abs(p.y) <= half.y) return false;
            UnityEngine.Vector2 d = behind ? -p : p;
            if (d.sqrMagnitude < 1e-6f) d = new UnityEngine.Vector2(0, -1); // straight behind: below
            float hx = Math.Max(1f, half.x - inset), hy = Math.Max(1f, half.y - inset);
            float kx = Math.Abs(d.x) > 1e-6f ? hx / Math.Abs(d.x) : float.PositiveInfinity;
            float ky = Math.Abs(d.y) > 1e-6f ? hy / Math.Abs(d.y) : float.PositiveInfinity;
            at = d * Math.Min(kx, ky);
            angle = (float)(Math.Atan2(-d.x, d.y) * 180 / Math.PI);
            return true;
        }
    }

    /// <summary>The climbers the arrows follow (N's `climb` events): from "start" / "up" until "top" / "knocked", or
    /// Hud20ClimbArrowMax s after its last word; at most Hud20ClimbArrows. Fixed arrays.</summary>
    public sealed class ClimbTrack20
    {
        readonly int[] ids = new int[UnityTuning.Hud20ClimbArrows];
        readonly Vec3[] lips = new Vec3[UnityTuning.Hud20ClimbArrows];
        readonly double[] times = new double[UnityTuning.Hud20ClimbArrows];
        int count;

        public int Count => count;
        public int Id(int i) => ids[i];
        /// <summary>The face's lip (where it will come over): the arrow's target when the climber isn't found.</summary>
        public Vec3 Lip(int i) => lips[i];

        public void On(ClimbEvent e, double now)
        {
            if (e == null) return;
            int at = -1;
            for (int i = 0; i < count; i++) if (ids[i] == e.enemy) { at = i; break; }
            if (e.phase == "top" || e.phase == "knocked")
            {
                if (at >= 0) RemoveAt(at);
                return;
            }
            if (e.phase != "start" && e.phase != "up") return;
            if (at < 0)
            {
                if (count >= ids.Length) return;
                at = count++;
                ids[at] = e.enemy;
            }
            lips[at] = e.lip;
            times[at] = now;
        }

        public void Step(double now)
        {
            for (int i = count - 1; i >= 0; i--)
                if (now - times[i] > UnityTuning.Hud20ClimbArrowMax) RemoveAt(i);
        }

        void RemoveAt(int i)
        {
            count--;
            for (int k = i; k < count; k++)
            {
                ids[k] = ids[k + 1];
                lips[k] = lips[k + 1];
                times[k] = times[k + 1];
            }
        }

        public void Clear() => count = 0;
    }

    /// <summary>The earned heal's "+N" by the HP numeral (UiVitals20.cs): heals within Hud20HealMerge s sum into one pop
    /// that punches in, holds, drifts up and fades over Hud20HealLife s.</summary>
    public sealed class HealPop20
    {
        double sum, lastAt = double.NegativeInfinity, age = -1;

        public bool Shown => age >= 0 && age < UnityTuning.Hud20HealLife;
        public int Amount => (int)Math.Round(sum, MidpointRounding.AwayFromZero);
        public double Age => age;

        public void Add(double hp, double now)
        {
            if (!(hp > 0)) return;
            if (!Shown || now - lastAt > UnityTuning.Hud20HealMerge) sum = 0;
            sum += hp;
            lastAt = now;
            age = 0;
        }

        public void Step(double frameDt)
        {
            if (age < 0) return;
            age += frameDt;
            if (age >= UnityTuning.Hud20HealLife) age = -1;
        }

        public void Reset()
        {
            age = -1;
            sum = 0;
            lastAt = double.NegativeInfinity;
        }

        public static float Scale(double age) =>
            age < 0 || age >= UnityTuning.Hud20HealPunchTime ? 1f : 1f + (UnityTuning.Hud20HealPunch - 1f) * (1f - UiTween.OutCubic(age / UnityTuning.Hud20HealPunchTime));

        public static float Alpha(double age)
        {
            if (age < 0 || age >= UnityTuning.Hud20HealLife) return 0f;
            double fadeAt = UnityTuning.Hud20HealLife - UnityTuning.Hud20HealFade;
            return age <= fadeAt ? 1f : 1f - UiTween.InCubic((age - fadeAt) / UnityTuning.Hud20HealFade);
        }

        public static float Rise(double age) => UnityTuning.Hud20HealRise * UiTween.OutCubic(Math.Max(0, age) / UnityTuning.Hud20HealLife);
    }

    /// <summary>The boss bar and the elite's plate (UiThreat20.cs): the WHITE chunk a hit takes holds Hud20ThreatTrailHold s,
    /// then drains to the fill at Hud20ThreatTrailRate (never under it; a heal raises it with the fill).</summary>
    public sealed class ThreatBar20
    {
        float trail = -1, fill;
        double hold;

        public float Trail => trail;
        public bool Chunk => trail > fill + 1e-6f && hold > 0;

        public void Step(float frac, double frameDt)
        {
            if (trail < 0)
            {
                trail = fill = frac;
                return;
            }
            if (frac < fill - 1e-6f) hold = UnityTuning.Hud20ThreatTrailHold; // a hit: the chunk holds (again)
            fill = frac;
            if (trail < fill) trail = fill;
            if (hold > 0) hold -= frameDt;
            else trail = Math.Max(fill, trail - (float)(UnityTuning.Hud20ThreatTrailRate * frameDt));
        }

        public void Reset()
        {
            trail = -1;
            hold = 0;
        }

        /// <summary>The bar's fill share as it enters (0 -> 1 over Hud20ThreatSweep, ease-out).</summary>
        public static float Sweep(double age) => UiTween.OutCubic(age / UnityTuning.Hud20ThreatSweep);

        static readonly Dictionary<(string, string, string), string> lines = new Dictionary<(string, string, string), string>();

        /// <summary>The bar's line: the name, then each affix in caps ("CHARGER · VOLATILE · SWIFT"); cached; null without a name.</summary>
        public static string Line(string name, string affixA, string affixB)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var key = (name, affixA, affixB);
            if (lines.TryGetValue(key, out string s)) return s;
            s = name;
            if (!string.IsNullOrEmpty(affixA)) s += " · " + affixA.ToUpperInvariant();
            if (!string.IsNullOrEmpty(affixB)) s += " · " + affixB.ToUpperInvariant();
            lines[key] = s;
            return s;
        }
    }

    /// <summary>F3: one of the reveal's three offers.</summary>
    public struct RevealOffer
    {
        public int kind; // Reveal20.PowerUp, Mod, Points
        public string type; // the power-up's type
        public int points;
    }

    /// <summary>F3: the reward reveal's rule (UiReveal20.cs): three offers, one of each kind (a power-up, a MODS pick, the
    /// round's points; two different power-ups with the mods off), shuffled, from a stream seeded by the run's seed and the
    /// round (its own Mulberry32: the run's streams are never drawn, so a run plays the same with the reveal unseen).</summary>
    /// <summary>NIGHT20 row 4 (builder HV), banned 10: the reveal's key prompts in the menus' language: a key's name on a
    /// drawn key chip (UiMenus.KeyChip) beside its verb on the mouse and keyboard; on a pad, the button's glyph sprite in the
    /// verb (PromptText), never its letters. Strings cached (no allocation per opening after the first).</summary>
    public static class RevealPrompt20
    {
        static readonly string[] slotKeys = { "1", "2", "3" };
        static readonly string[] padSkips = new string[4];
        const string Skip = "SKIP", Esc = "ESC";

        /// <summary>Slot i's key on its chip (1..3), or null.</summary>
        public static string SlotKey(int i) => i >= 0 && i < slotKeys.Length ? slotKeys[i] : null;

        /// <summary>The skip's chip: ESC on the mouse and keyboard; none on a pad (its glyph is in the verb).</summary>
        public static string SkipChip(bool pad) => pad ? null : Esc;

        /// <summary>The skip's verb: SKIP; on a pad the B glyph sprite, two spaces, SKIP.</summary>
        public static string SkipVerb(bool pad, GlyphSet set)
        {
            if (!pad) return Skip;
            int k = Math.Max(0, Math.Min(padSkips.Length - 1, (int)set));
            return padSkips[k] ??= PromptText.Button(PadButtonName.B, set) + "  " + Skip;
        }
    }

    public static class Reveal20
    {
        public const int PowerUp = 0, Mod = 1, Points = 2;

        /// <summary>The offers for `round` of the run seeded `runSeed` into `into` (3 slots).</summary>
        public static void Offers(uint runSeed, int round, double salt, double pointsBase, double pointsPerRound, string[] types, bool modsOn, RevealOffer[] into)
        {
            var rng = new Mulberry32(unchecked(runSeed ^ (uint)(long)salt ^ (uint)round * 2654435761u));
            into[0].kind = PowerUp;
            into[1].kind = modsOn ? Mod : PowerUp;
            into[2].kind = Points;
            for (int i = 2; i > 0; i--) // Fisher-Yates
            {
                int j = (int)Math.Floor(rng.Next() * (i + 1));
                RevealOffer t = into[i];
                into[i] = into[j];
                into[j] = t;
            }
            int points = PointsFor(round, pointsBase, pointsPerRound);
            int firstType = -1;
            for (int i = 0; i < 3; i++)
            {
                into[i].type = null;
                into[i].points = 0;
                if (into[i].kind == Points) into[i].points = points;
                if (into[i].kind != PowerUp || types == null || types.Length == 0) continue;
                int k = (int)Math.Floor(rng.Next() * types.Length);
                if (k == firstType && types.Length > 1) k = (k + 1 + (int)Math.Floor(rng.Next() * (types.Length - 1))) % types.Length; // a second power-up differs
                if (firstType < 0) firstType = k;
                into[i].type = types[k];
            }
        }

        /// <summary>The points slot: base + perRound x round, to the nearest ten (half up).</summary>
        public static int PointsFor(int round, double pointsBase, double pointsPerRound) =>
            (int)(Math.Round((pointsBase + pointsPerRound * round) / 10, MidpointRounding.AwayFromZero) * 10);

        /// <summary>Slot i's turn-over 0..1 `age` s into the reveal (in turn, Hud20RevealStagger apart, ease-out over Hud20RevealSlotIn).</summary>
        public static float SlotIn(double age, int i)
        {
            double t = age - i * UnityTuning.Hud20RevealStagger;
            if (t <= 0) return i == 0 ? UiTween.OutCubic(HudEntrance18.FirstAge(1.0 / 60) / UnityTuning.Hud20RevealSlotIn) : 0f;
            return UiTween.OutCubic(t / UnityTuning.Hud20RevealSlotIn);
        }

        /// <summary>A slot may be taken: the reveal has ended (a jump's A or a held key never takes one mid-reveal).</summary>
        public static bool Ready(double age) => age >= UnityTuning.Hud20RevealTime;

        /// <summary>The special rounds whose end pays a reveal: a hunt, an elite, a boss.</summary>
        public static bool Triggers(string name, string phase) =>
            phase == "end" && (name == "hunt" || name == "elite" || name == "boss" || name == "bulwark");
    }

    /// <summary>E3: the CODEX tab's rules (UiCodex20.cs; Research/2026-10-06-additions-backlog.md E3: RoR2's logbook, Hades'
    /// and DOOM's codex): the web's kinds a run meets (each entry its debut card) and builder G's named combos (each with how
    /// it's made), recorded once per profile in Profile.flags; the locked ones show as silhouettes.</summary>
    public static class Codex20
    {
        /// <summary>The web's roster in the order a run meets them (the backlog's debut rounds: swarmer 3, drone 4, ...).</summary>
        public static readonly string[] Enemies = { "husk", "swarmer", "drone", "spitter", "charger", "lancer", "warden", "bulwark" };

        /// <summary>Builder G's named combos (Combos19, Combos20).</summary>
        public static readonly string[] Combos = { Combos19.Pop, Combos19.LaunchSnipe, Combos19.Conductor, Combos19.Nailbomb, Combos19.SwapCancel, Combos20.Sent };

        // How each combo is made, from G's own notes (Core/Guns/Sandbox19.cs :10-20, Combos20.cs): <= 6 words, the static charset.
        static readonly Dictionary<string, string> how = new Dictionary<string, string>
        {
            [Combos19.Pop] = "KESTREL YOUR ROCKET IN FLIGHT",
            [Combos19.LaunchSnipe] = "COMET LAUNCH, THEN HIT AIRBORNE",
            [Combos19.Conductor] = "NAILS IN, THEN ZAP THEM",
            [Combos19.Nailbomb] = "NAILS IN, THEN A BLAST",
            [Combos19.SwapCancel] = "SHOOT, SWAP, HIT AT ONCE",
            [Combos20.Sent] = "PUSH IT INTO A TRAP",
        };

        static readonly Dictionary<string, string> enemyFlags = new Dictionary<string, string>(), comboFlags = new Dictionary<string, string>();
        static readonly Dictionary<string, string> heads = new Dictionary<string, string>();

        /// <summary>The kind's name in caps (the debut card's; the husk's own).</summary>
        public static string EnemyName(string kind) => kind == "husk" ? "HUSK" : Debut20.Name(kind);

        /// <summary>The kind's entry: its debut card's counter (the husk: the parry, its first lesson).</summary>
        public static string EnemyLine(string kind) => kind == "husk" ? "PUNCH ITS SWING BACK" : Debut20.Counter(kind);

        public static string ComboHow(string combo) => combo != null && how.TryGetValue(combo, out string s) ? s : null;

        public static string EnemyFlag(string kind)
        {
            if (kind == null) return null;
            if (!enemyFlags.TryGetValue(kind, out string f)) enemyFlags[kind] = f = "codex20.enemy." + kind;
            return f;
        }

        public static string ComboFlag(string combo)
        {
            if (combo == null) return null;
            if (!comboFlags.TryGetValue(combo, out string f)) comboFlags[combo] = f = "codex20.combo." + combo;
            return f;
        }

        /// <summary>Met in this profile: recorded, or its debut card shown, or (the husk) a run finished.</summary>
        public static bool EnemyMet(Profile p, string kind) =>
            p != null && (p.Flag(EnemyFlag(kind)) || p.Flag(Debut20.Flag(kind)) || (kind == "husk" && p.runs > 0));

        public static bool ComboFound(Profile p, string combo) => p != null && p.Flag(ComboFlag(combo));

        /// <summary>Record a kind seen on the field; true the first time (a codex kind only).</summary>
        public static bool RecordEnemy(Profile p, string kind) => p != null && Array.IndexOf(Enemies, kind) >= 0 && p.SetFlag(EnemyFlag(kind));

        /// <summary>Record a combo made; true the first time (one of G's names only).</summary>
        public static bool RecordCombo(Profile p, string combo) => p != null && how.ContainsKey(combo ?? string.Empty) && p.SetFlag(ComboFlag(combo));

        public static int EnemiesMet(Profile p)
        {
            int n = 0;
            foreach (string k in Enemies) if (EnemyMet(p, k)) n++;
            return n;
        }

        public static int CombosFound(Profile p)
        {
            int n = 0;
            foreach (string c in Combos) if (ComboFound(p, c)) n++;
            return n;
        }

        // --- C3's affixes (builder E's port of affixes.js): each with the counter its own header names --------------------
        public static readonly string[] Affixes = AffixMath.Names;

        static readonly Dictionary<string, (string name, string line)> affixes = new Dictionary<string, (string, string)>
        {
            ["shielded"] = ("SHIELDED", "HEADSHOT OR PUNCH THE PLATE"), // affixes.js: "stopped until a headshot or a melee hit breaks it"
            ["volatile"] = ("VOLATILE", "STAY OUT OF ITS BURST"), // "it bursts 0.4 s after it dies ... stay out of it"
            ["blinker"] = ("BLINKER", "WATCH FOR THE WHITE FLASH"), // "it blinks a short way (a white flash where it goes)"
            ["regenerating"] = ("REGENERATING", "KEEP DAMAGE ON IT"), // "out of damage for a moment, it heals back"
            ["swift"] = ("SWIFT", "LEAD YOUR SHOTS"), // "it moves 35% faster"
        };
        static readonly Dictionary<string, string> affixFlags = new Dictionary<string, string>();

        public static string AffixName(string a) => a != null && affixes.TryGetValue(a, out var v) ? v.name : null;
        public static string AffixLine(string a) => a != null && affixes.TryGetValue(a, out var v) ? v.line : null;

        public static string AffixFlag(string a)
        {
            if (a == null) return null;
            if (!affixFlags.TryGetValue(a, out string f)) affixFlags[a] = f = "codex20.affix." + a;
            return f;
        }

        public static bool AffixMet(Profile p, string a) => p != null && p.Flag(AffixFlag(a));

        /// <summary>Record an affix seen on the field; true the first time (one of affixes.js' five only).</summary>
        public static bool RecordAffix(Profile p, string a) => p != null && affixes.ContainsKey(a ?? string.Empty) && p.SetFlag(AffixFlag(a));

        public static int AffixesMet(Profile p)
        {
            int n = 0;
            foreach (string a in Affixes) if (AffixMet(p, a)) n++;
            return n;
        }

        public static string AffixesHead(int met) => Head("AFFIXES ", met, Affixes.Length);

        public static string EnemiesHead(int met) => Head("ENEMIES ", met, Enemies.Length);

        public static string CombosHead(int found) => Head("COMBOS ", found, Combos.Length);

        static string Head(string what, int n, int of)
        {
            string key = what + n;
            if (!heads.TryGetValue(key, out string s)) heads[key] = s = what + n + " / " + of;
            return s;
        }
    }

    /// <summary>
    /// F4's player-facing half (UiDownTell20.cs): WHAT GOT YOU's line, the killer's kind and its tell. The killer arrives as
    /// DeathRecap's display name (config.ui.recap.names, by the hurt's source type); each name is turned back into the kind
    /// that dealt it (the glob and the acid are the SPITTER's, the orb the DRONE's, VOLATILE's blast the affix's) and that
    /// kind's tell: its debut card's counter (Debut20), the husk's its CODEX line, and three a card doesn't name (the acid
    /// pool, VOLATILE's burst, a boss's). None for THE FALL, the RIVAL (magenta's, not a lesson), DEBUG or SOMETHING. Built
    /// once from the dictionaries; Line returns the same string each call (no allocation per frame).
    /// </summary>
    public sealed class DownTell20
    {
        // The tells no debut card carries (<= 4 words each, the static charset):
        const string AcidTell = "STAY OFF THE ACID"; // spitter.js: the glob that lands leaves an acid pool that burns while you stand in it
        const string VolatileTell = "BACK OFF ITS BURST"; // affixes.js :180-197: a VOLATILE enemy bursts where it dies after its fuse
        const string BossTell = "DODGE THE RED MARKS"; // bossKit.js: every boss attack is a telegraph (fx/telegraph.js) in the enemies' colour first

        readonly Dictionary<string, string> lines = new Dictionary<string, string>();
        readonly Dictionary<string, string> kinds = new Dictionary<string, string>();
        readonly Dictionary<string, string> tells = new Dictionary<string, string>();

        /// <summary>`recapNames` = config.ui.recap.names (source key -> display name); `bossNames` = config.bossKit.names, for a
        /// recap that names the boss (DeathRecap.SourceName with the boss names), optional.</summary>
        public DownTell20(IDictionary<string, string> recapNames, IDictionary<string, string> bossNames = null)
        {
            if (recapNames != null)
                foreach (KeyValuePair<string, string> kv in recapNames) Add(kv.Value, KindOf(kv.Key), TellOf(kv.Key));
            if (bossNames != null)
                foreach (KeyValuePair<string, string> kv in bossNames)
                    if (Debut20.Has(kv.Key)) Add(kv.Value, Debut20.Name(kv.Key), Debut20.Counter(kv.Key)); // the BULWARK's card
                    else Add(kv.Value, kv.Value, BossTell);
        }

        /// <summary>"KIND · TELL" for the killer's display name, or null (no tell: not an enemy, or unknown).</summary>
        public string Line(string killerName) => killerName != null && lines.TryGetValue(killerName, out string s) ? s : null;

        /// <summary>The killer's kind in caps (SPITTER for the glob and the acid), or null.</summary>
        public string Kind(string killerName) => killerName != null && kinds.TryGetValue(killerName, out string s) ? s : null;

        /// <summary>The kind's tell alone (<= 4 words), or null.</summary>
        public string Tell(string killerName) => killerName != null && tells.TryGetValue(killerName, out string s) ? s : null;

        void Add(string name, string kind, string tell)
        {
            if (name == null || kind == null || tell == null || lines.ContainsKey(name)) return;
            lines[name] = kind + " · " + tell;
            kinds[name] = kind;
            tells[name] = tell;
        }

        static string KindOf(string key)
        {
            switch (key)
            {
                case "husk": return Codex20.EnemyName("husk");
                case "acid": return Debut20.Name("spitter");
                // The port's VOLATILE blast reaches the recap as "affix" (Affixes.cs :404 keeps the type, not the attack), and
                // VOLATILE is affixes.js' only affix that hurts the player: AN AFFIX is VOLATILE's blast too.
                case "affix:volatile":
                case "affix": return "VOLATILE";
                case "boss": return "BOSS";
                default: return Debut20.Has(key) ? Debut20.Name(key) : null;
            }
        }

        static string TellOf(string key)
        {
            switch (key)
            {
                case "husk": return Codex20.EnemyLine("husk");
                case "acid": return AcidTell;
                case "affix:volatile":
                case "affix": return VolatileTell;
                case "boss": return BossTell;
                default: return Debut20.Has(key) ? Debut20.Counter(key) : null;
            }
        }
    }

    /// <summary>NIGHT20 A1 (builder HV): the boss bar's read of a boss's BossBar (bossKit.js bar(): {name, hp, max, phase,
    /// segments, pips}; BossKit.cs): valid only while it fights; its phase mark and its plates as pips.</summary>
    public static class BossThreat20
    {
        public static void From(BossBar bar, ref HudReads20.Threat t)
        {
            if (!bar.valid || !(bar.max > 0)) return;
            t.valid = true;
            t.enemy = bar.id;
            t.name = bar.name;
            t.affixA = t.affixB = null;
            t.hp = bar.hp;
            t.maxHp = bar.max;
            t.phaseAt = bar.segment;
            t.pips = bar.pips;
            t.pipsOn = bar.pipsOn;
        }
    }

    /// <summary>The LANCER's off-screen tell (UiLancer20.cs): by the aim's progress 0..1 (Lancer.AimProgress; &lt; 0: not aiming).</summary>
    public static class LancerTell20
    {
        /// <summary>Hud20LancerFrom on the aim's first frame, rising (ease-in: the shot is what matters) to whole at the shot.</summary>
        public static float Alpha(double progress)
        {
            if (progress < 0) return 0f;
            float p = UiTween.Clamp01(progress);
            return UnityTuning.Hud20LancerFrom + (1f - UnityTuning.Hud20LancerFrom) * UiTween.Clamp01(p * p * 1.0005f);
        }

        /// <summary>1 until Hud20LancerSwellFrom of the aim, then swelling to Hud20LancerSwell at the shot; 1 with REDUCED MOTION.</summary>
        public static float Scale(double progress, bool reducedMotion = false)
        {
            if (reducedMotion || progress <= UnityTuning.Hud20LancerSwellFrom) return 1f;
            double t = (Math.Min(1, progress) - UnityTuning.Hud20LancerSwellFrom) / (1 - UnityTuning.Hud20LancerSwellFrom);
            return 1f + (UnityTuning.Hud20LancerSwell - 1f) * UiTween.Clamp01(t);
        }
    }

    /// <summary>The special hint's words and the pad's cycle line, from the live numbers and binding (UiHotbar.cs).</summary>
    public static class SpecialHint20
    {
        static readonly Dictionary<double, string> zoomWords = new Dictionary<double, string>();
        static readonly Dictionary<long, string> cycleLines = new Dictionary<long, string>();

        /// <summary>"ZOOM ×1.25" for the gun's real magnification (HudReads20.ZoomOf), cached per value; null unzoomed.</summary>
        public static string ZoomWord(double mag)
        {
            if (!(mag > 1.0001)) return null;
            if (zoomWords.TryGetValue(mag, out string s)) return s;
            s = "ZOOM ×" + JsNumber.ToString(mag);
            zoomWords[mag] = s;
            return s;
        }

        /// <summary>The hotbar's cycle line in the pad's words from the live layout (NEXT WEAPON's button, PREVIOUS's): the
        /// fixed "Y NEXT · D-PAD LEFT BACK" was the v2 default's; layout v3 moves NEXT to RB. Cached per buttons and set.</summary>
        public static string CycleLine(PadLayout layout, GlyphSet set)
        {
            layout ??= GamepadMath.DefaultLayoutV2;
            PadButtonName? next = GamepadMath.ButtonFor(PadAction.NextWeapon, layout), prev = GamepadMath.ButtonFor(PadAction.PrevWeapon, layout);
            long key = ((long)set << 16) | ((long)(next.HasValue ? (int)next.Value + 1 : 0) << 8) | (long)(prev.HasValue ? (int)prev.Value + 1 : 0);
            if (cycleLines.TryGetValue(key, out string s)) return s;
            s = next.HasValue ? Glyphs.Button(next.Value, set) + UnityTuning.PromptCycleNext : "";
            if (prev.HasValue) s += (s.Length > 0 ? UnityTuning.PromptCycleSep : "") + Glyphs.Button(prev.Value, set) + UnityTuning.PromptCycleBack;
            cycleLines[key] = s;
            return s;
        }
    }
}
