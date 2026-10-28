// NIGHT12 U3-25.10: finishes and MASTERY. Finishes is a port of skygrind/src/modes/finishes.js (a finish recolours a gun's
// neutral body tones: bone, gunmetal, dark, grip; never the family trim or the core gem; config.weapons.finishes) with
// the medal thresholds of modes/trialMath.js it reads. The port has no trials, so its medal counts are 0: a finish the web
// unlocks by medals (GRAPHITE, ARCTIC, DUSK BRASS, MIRROR) also opens with the port-only flag "finish.<id>" (a contract
// grants it for every gun) or, for one gun, with that gun's MASTERY tier. PROTOCOL and SKYLIGHT keep their web flags.
// Mastery is port-only (design.mastery; Research/retention P4): a track per gun the port fires, 3 tiers tonight, counts in
// the tens that teach the verbs (kills with the gun; FRESH or air kills; kills at style rank S or higher), each tier
// unlocking a finish for that gun. Every counter counts from the start; a gun's tier is the number of leading tracks done
// (a later track done first waits for the ones before it). Counters live in the profile (Profile.SetMastery), capped at
// their targets; nothing else is cached (Reload re-reads the profile, StartRun does).
// The Game sets `fresh` (StyleMeter.getFresh: the arsenal's FRESH 0..1 of a hit source), `airborne` and `styleRank`.
// The kill handler allocates nothing unless a tier is reached (its payload).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace Skygrind.Core
{
    public static class Finishes
    {
        /// <summary>A finish's palette (config.weapons.finishes.list.&lt;id&gt;.palette, 0xRRGGBB); FACTORY has none.</summary>
        public sealed class Palette
        {
            public uint bone, gunmetal, dark, grip;
        }

        /// <summary>finishes.js medalCounts: how many trials have each medal or better, plus the earned flags.</summary>
        public sealed class Counts
        {
            public int bronze, silver, gold, ace;
            public Func<string, bool> flags; // NIGHT6: the earned flags (modes/unlocks.js) for flag unlocks; null: none

            public int Of(string medal) => medal switch { "bronze" => bronze, "silver" => silver, "gold" => gold, "ace" => ace, _ => 0 };

            public bool Flag(string f) => flags != null && f != null && flags(f);
        }

        /// <summary>trialMath.js medalTimes: the medal thresholds (s) of a trial.</summary>
        public struct MedalTimes
        {
            public double ace, gold, silver, bronze;

            public double Of(string medal) => medal switch { "ace" => ace, "gold" => gold, "silver" => silver, "bronze" => bronze, _ => double.NaN };
        }

        /// <summary>trialMath.js MEDALS: best first.</summary>
        public static readonly string[] Medals = { "ace", "gold", "silver", "bronze" };

        static readonly string[] MedalOrder = { "bronze", "silver", "gold", "ace" }; // finishes.js ORDER

        /// <summary>The port-only flag that opens a medal finish for every gun ("finish.graphite").</summary>
        public const string FlagPrefix = "finish.";

        sealed class Def
        {
            public string id, name, flag, label, medal, portFlag;
            public double count;
            public bool hasUnlock;
            public Palette palette;
        }

        sealed class Table
        {
            public readonly List<string> order = new List<string>();
            public readonly Dictionary<string, Def> defs = new Dictionary<string, Def>();
        }

        static readonly ConditionalWeakTable<GameConfig, Table> tables = new ConditionalWeakTable<GameConfig, Table>();

        // NIGHT12 (the lead, after a review): the callback made once; `GetValue(cfg, Build)` allocated a delegate on every call
        // (C# 9 doesn't cache method-group conversions), and FinishFor reaches this every frame from the viewmodel and the wall.
        static readonly ConditionalWeakTable<GameConfig, Table>.CreateValueCallback BuildCallback = Build;

        static Table T(GameConfig cfg) => tables.GetValue(cfg, BuildCallback);

        static Table Build(GameConfig cfg)
        {
            var t = new Table();
            foreach (string id in cfg.Strs("weapons.finishes.order")) t.order.Add(id);
            foreach (var kv in cfg.Obj("weapons.finishes.list"))
            {
                var f = PortData.Obj(kv.Value);
                var d = new Def { id = kv.Key, name = f.TryGetValue("name", out object n) ? n as string : kv.Key, portFlag = FlagPrefix + kv.Key };
                if (f.TryGetValue("unlock", out object u) && u is Dictionary<string, object> un)
                {
                    d.hasUnlock = true;
                    d.flag = un.TryGetValue("flag", out object fl) ? fl as string : null;
                    d.label = un.TryGetValue("label", out object lb) ? lb as string : null;
                    d.medal = un.TryGetValue("medal", out object md) ? md as string : null;
                    d.count = un.TryGetValue("count", out object ct) ? PortData.Num(ct) : 0;
                }
                if (f.TryGetValue("palette", out object p) && p is Dictionary<string, object> pal)
                {
                    d.palette = new Palette
                    {
                        bone = (uint)PortData.Num(pal["bone"]), gunmetal = (uint)PortData.Num(pal["gunmetal"]),
                        dark = (uint)PortData.Num(pal["dark"]), grip = (uint)PortData.Num(pal["grip"]),
                    };
                }
                t.defs[kv.Key] = d;
            }
            return t;
        }

        /// <summary>String(number), as a template literal prints it.</summary>
        internal static string Js(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        /// <summary>config.weapons.finishes.order.</summary>
        public static List<string> Order(GameConfig cfg) => new List<string>(T(cfg).order);

        /// <summary>config.weapons.finishes.order, the shared list (read only: no copy).</summary>
        internal static List<string> OrderView(GameConfig cfg) => T(cfg).order;

        /// <summary>The first finish (FACTORY): the default, always unlocked.</summary>
        public static string Factory(GameConfig cfg) => T(cfg).order.Count > 0 ? T(cfg).order[0] : null;

        /// <summary>The finish's name ("DUSK BRASS").</summary>
        public static string Name(GameConfig cfg, string id) => id != null && T(cfg).defs.TryGetValue(id, out Def d) ? d.name : id;

        /// <summary>The palette to draw, or null for FACTORY / an unknown id (the guns' own tones).</summary>
        public static Palette PaletteOf(GameConfig cfg, string id) => id != null && T(cfg).defs.TryGetValue(id, out Def d) ? d.palette : null;

        /// <summary>Is `id` a finish config knows?</summary>
        public static bool Known(GameConfig cfg, string id) => id != null && T(cfg).defs.ContainsKey(id);

        /// <summary>The port-only flag of a medal finish ("finish.graphite"), or null for a flag finish / FACTORY / unknown.</summary>
        public static string PortFlag(GameConfig cfg, string id) =>
            id != null && T(cfg).defs.TryGetValue(id, out Def d) && d.hasUnlock && d.flag == null ? d.portFlag : null;

        /// <summary>Does earning `flag` open finish `id` (its web flag, or its port-only flag)?</summary>
        public static bool OpenedBy(GameConfig cfg, string id, string flag)
        {
            if (flag == null || id == null || !T(cfg).defs.TryGetValue(id, out Def d) || !d.hasUnlock) return false;
            return d.flag != null ? d.flag == flag : d.portFlag == flag;
        }

        // --- trialMath.js ------------------------------------------------------------------------------------------------

        /// <summary>trialMath.js medalTimes: par x each ratio, rounded up to the tenth (config.trials.medals as `ratios`).</summary>
        public static MedalTimes MedalTimesOf(double par, MedalTimes ratios)
        {
            static double Up(double s) => Math.Ceiling(s * 10 - 1e-9) / 10; // the web's epsilon: a time shown as 12.3 never misses 12.3
            return new MedalTimes { ace = Up(par * ratios.ace), gold = Up(par * ratios.gold), silver = Up(par * ratios.silver), bronze = Up(par * ratios.bronze) };
        }

        /// <summary>trialMath.js medalFor: the best medal a time earns ('ace' .. 'bronze'), or null.</summary>
        public static string MedalFor(double time, MedalTimes times)
        {
            foreach (string m in Medals) if (time <= times.Of(m) + 1e-9) return m;
            return null;
        }

        /// <summary>config.trials.medals as ratios.</summary>
        public static MedalTimes Ratios(GameConfig cfg) => new MedalTimes
        {
            ace = cfg.Num("trials.medals.ace"), gold = cfg.Num("trials.medals.gold"), silver = cfg.Num("trials.medals.silver"), bronze = cfg.Num("trials.medals.bronze"),
        };

        // --- finishes.js -------------------------------------------------------------------------------------------------

        /// <summary>
        /// finishes.js medalCounts(trials, getBest, flags): how many of config.trials.list have each medal or better.
        /// `bestTime`: a trial's best time (s), or null. The port has no trials: pass null (every count 0).
        /// </summary>
        public static Counts MedalCounts(GameConfig cfg, Func<string, double?> bestTime, Func<string, bool> flags = null)
        {
            var counts = new Counts { flags = flags };
            if (bestTime == null) return counts;
            MedalTimes ratios = Ratios(cfg);
            foreach (object o in cfg.Arr("trials.list"))
            {
                var def = PortData.Obj(o);
                double? best = bestTime((string)def["id"]);
                string medal = best.HasValue ? MedalFor(best.Value, MedalTimesOf(PortData.Num(def["par"]), ratios)) : null;
                if (medal == null) continue;
                int upTo = Array.IndexOf(MedalOrder, medal);
                for (int i = 0; i <= upTo; i++)
                {
                    switch (MedalOrder[i])
                    {
                        case "bronze": counts.bronze++; break;
                        case "silver": counts.silver++; break;
                        case "gold": counts.gold++; break;
                        case "ace": counts.ace++; break;
                    }
                }
            }
            return counts;
        }

        /// <summary>
        /// finishes.js finishUnlocked: is a finish unlocked with these counts? (No `unlock`: always.) Port-only: a medal
        /// finish also opens with its flag "finish.&lt;id&gt;" (the port has no trials; a contract grants it).
        /// </summary>
        public static bool FinishUnlocked(GameConfig cfg, string id, Counts counts)
        {
            if (id == null || !T(cfg).defs.TryGetValue(id, out Def f)) return false;
            if (f.flag != null) return counts != null && counts.Flag(f.flag); // NIGHT6: earned once (the PROTOCOL)
            if (!f.hasUnlock) return true;
            return (counts != null ? counts.Of(f.medal) : 0) >= f.count || (counts != null && counts.Flag(f.portFlag));
        }

        /// <summary>finishes.js activeFinish: the chosen one if it is unlocked, else the first (the factory finish).</summary>
        public static string ActiveFinish(GameConfig cfg, string chosen, Counts counts) => FinishUnlocked(cfg, chosen, counts) ? chosen : Factory(cfg);

        /// <summary>finishes.js finishNeeds: "3 SILVER" (what a locked finish needs), or "" for an unlocked one.</summary>
        public static string FinishNeeds(GameConfig cfg, string id, Counts counts)
        {
            if (id == null || !T(cfg).defs.TryGetValue(id, out Def f) || !f.hasUnlock || FinishUnlocked(cfg, id, counts)) return "";
            if (f.flag != null) return f.label;
            return $"{Js(f.count)} {f.medal.ToUpperInvariant()}";
        }
    }

    public sealed class Mastery : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: recording, the profile's dirty flag, this run's XP per gun and its tier-ups (the
        // PAYOFF strip). The counters themselves live in the profile; Load re-reads them (Reload), as StartRun does.
        public void Save(StateWriter w)
        {
            w.B("recording", recording);
            w.B("dirty", Dirty);
            var weapons = new List<string>(guns.Count);
            var xp = new List<double>(guns.Count);
            foreach (Gun g in guns)
            {
                weapons.Add(g.weapon);
                xp.Add(g.runXp);
            }
            w.Strings("guns", weapons);
            w.Doubles("runXp", xp);
            var tw = new List<string>(tierUpsThisRun.Count);
            var tt = new List<int>(tierUpsThisRun.Count);
            foreach (var (weapon, tier) in tierUpsThisRun)
            {
                tw.Add(weapon);
                tt.Add(tier);
            }
            w.Strings("tierUpWeapons", tw);
            w.Ints("tierUpTiers", tt);
        }

        public void Load(StateReader r)
        {
            List<string> weapons = r.Strings("guns"), tw = r.Strings("tierUpWeapons");
            List<double> xp = r.Doubles("runXp");
            List<int> tt = r.Ints("tierUpTiers");
            bool same = weapons.Count == guns.Count && xp.Count == weapons.Count && tt.Count == tw.Count;
            for (int i = 0; same && i < guns.Count; i++) same = weapons[i] == guns[i].weapon;
            if (!same) throw new StateException("snapshot: mastery: the guns differ");
            recording = r.B("recording");
            Dirty = r.B("dirty");
            for (int i = 0; i < guns.Count; i++) guns[i].runXp = xp[i];
            tierUpsThisRun.Clear();
            for (int i = 0; i < tw.Count; i++) tierUpsThisRun.Add((tw[i], tt[i]));
            Reload();
        }

        public sealed class Track
        {
            public string id, name; // "kills" / "KILLS"
            public double value, target; // toward the next tier
            public string desc; // "Kills with this gun"
            public string finish; // the finish its tier unlocks for the gun
            public bool Done => value >= target;
        }

        public sealed class Gun
        {
            public string weapon, name; // catalog id, display name
            public int tier; // 0..3
            public readonly List<Track> tracks = new List<Track>();
            public readonly List<string> finishes = new List<string>(); // the finishes this gun may wear (unlocked), FACTORY first
            public string finish; // the one chosen (drawn), FACTORY by default
            public double runXp; // progress gained this run (the PAYOFF strip)
        }

        /// <summary>The tracks design.mastery.tiers may name.</summary>
        public static readonly string[] TrackKinds = { "kills", "fresh", "srank" };

        /// <summary>False for a LAB run or an automation run (noRecords): nothing counts.</summary>
        public bool recording;

        /// <summary>The profile changed since the Game last saved it.</summary>
        public bool Dirty;

        /// <summary>design.mastery.enabled: false keeps the guns (finishes by flags only) with no tracks.</summary>
        public readonly bool enabled;

        /// <summary>The arsenal's FRESH (0..1) of a hit source, or null when it doesn't track it (StyleMeter.getFresh).</summary>
        public Func<string, double?> fresh;

        /// <summary>The player is in the air (a "fresh" track kill).</summary>
        public Func<bool> airborne;

        /// <summary>The style rank index now (StyleMeter.rank): an "srank" kill at rank >= SRank.</summary>
        public Func<int> styleRank;

        /// <summary>The style rank index an "srank" kill needs (design.mastery.sRank's letter in config.style.ranks).</summary>
        public readonly int SRank;

        sealed class Tier
        {
            public string track, name, desc, finish;
            public double target;
        }

        readonly GameConfig cfg;
        readonly EventBus events;
        readonly Profile profile;
        readonly StyleMath.S style;
        readonly List<Tier> tiers = new List<Tier>();
        readonly List<Gun> guns = new List<Gun>();
        readonly Dictionary<string, Gun> byWeapon = new Dictionary<string, Gun>();
        readonly Finishes.Counts counts;
        readonly string factory;

        /// <summary>`fires`: the guns the port can fire (Loadout.Fires); only those get a track.</summary>
        public Mastery(GameConfig cfg, EventBus events, Profile profile, Func<string, bool> fires)
        {
            this.cfg = cfg;
            this.events = events;
            this.profile = profile;
            style = StyleMath.From(cfg);
            counts = Finishes.MedalCounts(cfg, null, profile.Flag); // no trials in the port: flags only
            factory = Finishes.Factory(cfg);
            enabled = cfg.Design("mastery.enabled");
            if (enabled && cfg.Has("design.mastery.tiers"))
            {
                foreach (object o in cfg.Arr("design.mastery.tiers"))
                {
                    var t = PortData.Obj(o);
                    var tier = new Tier
                    {
                        track = (string)t["track"], name = (string)t["name"], desc = t.TryGetValue("desc", out object d) ? d as string : null,
                        finish = (string)t["finish"], target = PortData.Num(t["target"]),
                    };
                    if (Array.IndexOf(TrackKinds, tier.track) < 0) throw new InvalidDataException($"design.mastery.tiers: unknown track '{tier.track}'");
                    if (!Finishes.Known(cfg, tier.finish)) throw new InvalidDataException($"design.mastery.tiers: unknown finish '{tier.finish}'");
                    tiers.Add(tier);
                }
            }
            // The rank an S-rank kill needs: the letter's index in config.style.ranks (the top rank when it isn't there).
            string letter = enabled && cfg.Has("design.mastery.sRank") ? cfg.Str("design.mastery.sRank") : null;
            List<object> ranks = cfg.Arr("style.ranks");
            SRank = ranks.Count - 1;
            for (int i = 0; i < ranks.Count; i++)
                if (PortData.Obj(ranks[i]).TryGetValue("letter", out object l) && l as string == letter)
                {
                    SRank = i;
                    break;
                }
            foreach (var kv in cfg.Obj("weapons.catalog")) // catalog order
            {
                if (fires == null || !fires(kv.Key)) continue;
                var w = PortData.Obj(kv.Value);
                var gun = new Gun { weapon = kv.Key, name = w.TryGetValue("name", out object n) ? n as string : kv.Key };
                foreach (Tier t in tiers) gun.tracks.Add(new Track { id = t.track, name = t.name, desc = t.desc, target = t.target, finish = t.finish });
                guns.Add(gun);
                byWeapon[gun.weapon] = gun;
            }
            Reload();
            events.On<KillEvent>("kill", OnKill);
        }

        /// <summary>A new run: the run's XP and tier-ups start over, the counters re-read from the profile.</summary>
        public void StartRun(bool recording)
        {
            this.recording = recording;
            tierUpsThisRun.Clear();
            foreach (Gun g in guns) g.runXp = 0;
            Reload();
        }

        /// <summary>Re-read every counter, tier and finish from the profile (it may have been restored in place). No events.</summary>
        public void Reload()
        {
            foreach (Gun g in guns)
            {
                foreach (Track t in g.tracks) t.value = profile.Mastery(g.weapon, t.id);
                g.tier = TierOf(g);
                RefreshFinishes(g);
            }
        }

        static int TierOf(Gun g)
        {
            int tier = 0;
            while (tier < g.tracks.Count && g.tracks[tier].Done) tier++;
            return tier;
        }

        void RefreshFinishes(Gun g)
        {
            g.finishes.Clear();
            foreach (string id in Finishes.OrderView(cfg)) if (UnlockedFor(g.weapon, id)) g.finishes.Add(id);
            g.finish = FinishFor(g.weapon);
        }

        void OnKill(KillEvent e)
        {
            if (!recording || tiers.Count == 0 || !(e.owner == null || e.owner == "player")) return;
            string w = ArsenalMath.WeaponOf(e.weapon); // a punch / orb is the GAUNTLET's, a ricochet the KESTREL's
            if (w == null || !byWeapon.TryGetValue(w, out Gun g)) return;
            for (int i = 0; i < g.tracks.Count; i++)
            {
                Track t = g.tracks[i];
                bool hit = t.id switch
                {
                    "kills" => true,
                    "fresh" => IsFresh(e.weapon) || (airborne != null && airborne()),
                    "srank" => styleRank != null && styleRank() >= SRank,
                    _ => false,
                };
                if (hit) Bump(g, t);
            }
            int tier = TierOf(g);
            if (tier <= g.tier) return;
            while (g.tier < tier)
            {
                g.tier++;
                string finish = g.tracks[g.tier - 1].finish;
                tierUpsThisRun.Add((g.weapon, g.tier));
                events.Emit(MetaEvents.MasteryTier, new MasteryEvent { weapon = g.weapon, tier = g.tier, finish = finish });
            }
            RefreshFinishes(g);
        }

        /// <summary>FRESH by the style meter's tiers (style.freshness.freshAt on the arsenal's FRESH x max).</summary>
        bool IsFresh(string source)
        {
            double? f = fresh?.Invoke(source);
            return f.HasValue && StyleMath.FreshnessTierName(f.Value * style.max, style) == "FRESH";
        }

        void Bump(Gun g, Track t)
        {
            double v = profile.Mastery(g.weapon, t.id);
            if (v >= t.target) return;
            double next = JsMath.Min(v + 1, t.target);
            profile.SetMastery(g.weapon, t.id, next);
            t.value = next;
            g.runXp += next - v;
            Dirty = true;
        }

        /// <summary>Every gun with a track, in catalog order (the WEAPONS screen). The live list: don't modify it.</summary>
        public List<Gun> Guns()
        {
            foreach (Gun g in guns) RefreshFinishes(g); // a contract may have opened a finish since
            return guns;
        }

        /// <summary>One gun's state (null: no track).</summary>
        public Gun Of(string weapon)
        {
            if (weapon == null || !byWeapon.TryGetValue(weapon, out Gun g)) return null;
            RefreshFinishes(g);
            return g;
        }

        /// <summary>
        /// May `weapon` wear `finish`? FACTORY always; a finish unlocked for every gun (its web flag, or the port's
        /// "finish.&lt;id&gt;"); or a finish one of this gun's reached tiers unlocks.
        /// </summary>
        public bool UnlockedFor(string weapon, string finish)
        {
            if (finish == null || !Finishes.Known(cfg, finish)) return false;
            if (finish == factory || Finishes.FinishUnlocked(cfg, finish, counts)) return true;
            if (weapon == null || !byWeapon.TryGetValue(weapon, out Gun g)) return false;
            for (int i = 0; i < g.tier && i < g.tracks.Count; i++) if (g.tracks[i].finish == finish) return true;
            return false;
        }

        /// <summary>What a locked finish needs on this gun ("TIER 2 · 30 FRESH OR AIR KILLS", "COMPLETE THE PROTOCOL"), "" when unlocked.</summary>
        public string Needs(string weapon, string finish)
        {
            if (UnlockedFor(weapon, finish)) return "";
            if (weapon != null && byWeapon.TryGetValue(weapon, out Gun g))
                for (int i = 0; i < g.tracks.Count; i++)
                    if (g.tracks[i].finish == finish) return $"TIER {Finishes.Js(i + 1)} · {Finishes.Js(g.tracks[i].target)} {g.tracks[i].name}";
            return Finishes.FinishNeeds(cfg, finish, counts);
        }

        /// <summary>The finish to draw on a gun: the chosen one if unlocked, else FACTORY ("factory").</summary>
        public string FinishFor(string weapon)
        {
            string chosen = weapon != null ? profile.FinishOf(weapon) : null;
            return chosen != null && UnlockedFor(weapon, chosen) ? chosen : factory;
        }

        /// <summary>Choose a gun's finish (false if it isn't unlocked for that gun). A menu choice: works outside a run.</summary>
        public bool Choose(string weapon, string finish)
        {
            if (weapon == null || !UnlockedFor(weapon, finish)) return false;
            profile.finishes[weapon] = finish;
            Dirty = true;
            if (byWeapon.TryGetValue(weapon, out Gun g)) g.finish = finish;
            return true;
        }

        /// <summary>Tiers reached this run: (weapon, tier), in order (RESULTS, the PAYOFF strip).</summary>
        public readonly List<(string weapon, int tier)> tierUpsThisRun = new List<(string, int)>();
    }
}
