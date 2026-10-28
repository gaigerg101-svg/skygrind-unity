// The oracle's checkers for the pure modules ported on NIGHT9 U3-07 (every exported vector set gets a consumer):
// gunMath, spawnMath, weaponMath, arsenalMath and momentumMath (PortExport/vectors/<set>.json). Each walks EVERY record
// of its file in order (calls, sequences, mulberry32's first16), calls the C# port and compares bit for bit, the
// mutated arguments (argsAfter) and the rng draws included.
// - Config arguments are recorded as {"$config": "path"}: the params are built from that path of config.json.
// - An rng argument {"$fn": {"$rng": "mulberry32", seed, draws}} is a fresh Mulberry32 that counts its draws; the
//   count after the call is compared with argsAfter's.
// - chainTargets' injected callback is replayed by its recorded rule ("canReach(i) = i !== 0"), as CombatGoldenTests
//   does for pickRicochetTarget; an unknown rule is a mismatch.
// - A sequence's inputs that its note states (stuckStep's timeout, smoothVelocity's v and start) are read from the note,
//   its config path too; a sequence stops at its first differing row (the state has diverged after it).
// - sweepTargets runs through CombatMath.SweepTargets, perkEffects / perkPrice through PerkMath (Perks.cs).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Skygrind.Core
{
    public static partial class Golden
    {
        static partial void AddPorted(Dictionary<string, GoldenCheck> d)
        {
            d["gunMath"] = PortedChecks.CheckGunMath;
            d["spawnMath"] = PortedChecks.CheckSpawnMath;
            d["weaponMath"] = PortedChecks.CheckWeaponMath;
            d["arsenalMath"] = PortedChecks.CheckArsenalMath;
            d["momentumMath"] = PortedChecks.CheckMomentumMath;
        }
    }

    static class PortedChecks
    {
        // --- decoding --------------------------------------------------------------------------------------------

        static double N(object o) => PortData.Num(o);

        static int I(object o) => (int)PortData.Num(o);

        static Vec3 V(object o) => PortData.V3(o);

        static List<object> A(object o) => PortData.Arr(o);

        static Dictionary<string, object> O(object o) => PortData.Obj(o);

        static List<object> Args(Dictionary<string, object> c) => A(c["args"]);

        /// <summary>The arguments after the call (the same as before when the record has no argsAfter).</summary>
        static List<object> After(Dictionary<string, object> c) => c.TryGetValue("argsAfter", out object a) ? A(a) : Args(c);

        static string ConfigPath(object o)
        {
            if (o is Dictionary<string, object> d && d.TryGetValue("$config", out object p)) return (string)p;
            throw new FormatException($"expected a {{\"$config\": path}} argument, got {Golden.Describe(o)}");
        }

        /// <summary>Label each record "fn[k]" (k counts that function's records) and count it.</summary>
        static string Label(Dictionary<string, object> c, Dictionary<string, int> seen)
        {
            string fn = c.TryGetValue("fn", out object f) ? (string)f : c.TryGetValue("sequence", out object s) ? "sequence " + (string)s : "?";
            seen.TryGetValue(fn, out int k);
            seen[fn] = k + 1;
            return $"{fn}[{k}]";
        }

        static string Fn(Dictionary<string, object> c) => c.TryGetValue("fn", out object f) ? (string)f : null;

        static string Sequence(Dictionary<string, object> c) => c.TryGetValue("sequence", out object s) ? (string)s : null;

        /// <summary>A recorded mulberry32 argument, replayed with a draw counter.</summary>
        sealed class CountingRng
        {
            readonly Mulberry32 m;
            double draws;
            public readonly Func<double> Next;

            public CountingRng(object arg)
            {
                var f = O(O(arg)["$fn"]);
                if (!(f.TryGetValue("$rng", out object kind) && (string)kind == "mulberry32")) throw new FormatException($"not a recorded mulberry32: {Golden.Describe(arg)}");
                m = new Mulberry32(N(f["seed"]));
                draws = N(f["draws"]);
                for (int i = 0; i < draws; i++) m.Next();
                Next = Draw;
            }

            double Draw()
            {
                draws++;
                return m.Next();
            }

            public void CheckDraws(GoldenResult r, object after, string what) => r.Same(O(O(after)["$fn"])["draws"], draws, what + " draws");
        }

        /// <summary>The config path a sequence's note names ("config.weapons.nailgun" -> "weapons.nailgun").</summary>
        static string NoteConfigPath(Dictionary<string, object> c)
        {
            Match m = Regex.Match((string)c["note"], @"config\.([A-Za-z0-9_]+(?:\.[A-Za-z0-9_]+)*)");
            if (!m.Success) throw new FormatException($"no config path in the note: {c["note"]}");
            return m.Groups[1].Value;
        }

        /// <summary>A number the note states as `name=value`.</summary>
        static double NoteNumber(Dictionary<string, object> c, string name)
        {
            Match m = Regex.Match((string)c["note"], Regex.Escape(name) + @"=(-?[0-9.]+(?:e-?[0-9]+)?)");
            if (!m.Success) throw new FormatException($"no {name}= in the note: {c["note"]}");
            return double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        /// <summary>A vector the note states after `prefix`, as "(x, y, z)".</summary>
        static Vec3 NoteVector(Dictionary<string, object> c, string prefix)
        {
            Match m = Regex.Match((string)c["note"], Regex.Escape(prefix) + @"\s*\(([^)]*)\)");
            if (!m.Success) throw new FormatException($"no '{prefix} (x, y, z)' in the note: {c["note"]}");
            string[] p = m.Groups[1].Value.Split(',');
            if (p.Length != 3) throw new FormatException($"'{prefix}' is not a 3-vector in the note: {c["note"]}");
            double P(int i) => double.Parse(p[i].Trim(), CultureInfo.InvariantCulture);
            return new Vec3(P(0), P(1), P(2));
        }

        static Dictionary<string, object> Obj(params (string key, object value)[] kv)
        {
            var d = new Dictionary<string, object>();
            foreach (var (k, v) in kv) d[k] = v;
            return d;
        }

        static void NoRule(GoldenResult r, Dictionary<string, object> c) => r.Fail($"no rule for this record: {Golden.Describe(c)}");

        // --- gunMath -----------------------------------------------------------------------------------------------

        public static void CheckGunMath(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            var seen = new Dictionary<string, int>();
            foreach (var c in data.Cases("gunMath"))
            {
                r.where = Label(c, seen);
                r.cases++;
                var a = Args(c);
                switch (Fn(c))
                {
                    case "sweepTargets":
                    {
                        var bodies = A(a[2]).ConvertAll(t =>
                        {
                            var d = O(t);
                            return new BodyTarget { enemy = d, basePos = V(d["base"]), height = N(d["height"]), radius = N(d["radius"]) };
                        });
                        List<TargetPick> got = CombatMath.SweepTargets(V(a[0]), V(a[1]), bodies, N(a[3]), N(a[4]));
                        var list = new List<object>();
                        foreach (TargetPick p in got)
                        {
                            var rec = (Dictionary<string, object>)p.target.enemy;
                            list.Add(Obj(("target", Obj(("id", rec["id"]), ("base", p.target.basePos), ("height", p.target.height), ("radius", p.target.radius))), ("distance", p.distance)));
                        }
                        r.SameJson(c["result"], list, "result", true);
                        break;
                    }
                    case "chainTargets":
                    {
                        Func<int, Vec3, bool> canReach = null;
                        if (a.Count > 5)
                        {
                            string rule = (string)O(O(a[5])["$fn"])["$callback"];
                            if (rule == "canReach(i) = i !== 0") canReach = (i, from) => i != 0;
                            else
                            {
                                r.Fail($"unknown recorded callback rule: {rule}");
                                break;
                            }
                        }
                        var got = GunMath.ChainTargets(V(a[0]), A(a[1]).ConvertAll(V), N(a[2]), N(a[3]), N(a[4]), canReach);
                        r.SameJson(c["result"], got.ConvertAll(l => (object)Obj(("index", (double)l.index), ("mult", l.mult))), "result", true);
                        break;
                    }
                    case "bounceVelocity":
                    {
                        Vec3 v = V(a[0]);
                        Vec3 ret = GunMath.BounceVelocity(ref v, V(a[1]), N(a[2]), N(a[3]));
                        r.SameVec(c["result"], ret, "result");
                        r.SameVec(After(c)[0], v, "v after");
                        break;
                    }
                    case "reflectDir":
                    {
                        Vec3 d = V(a[0]);
                        Vec3 ret = GunMath.ReflectDir(ref d, V(a[1]));
                        r.SameVec(c["result"], ret, "result");
                        r.SameVec(After(c)[0], d, "d after");
                        break;
                    }
                    case "discDamage":
                        r.Same(c["result"], GunMath.DiscDamage(N(a[0]), N(a[1]), N(a[2])), "result");
                        break;
                    case "pullSpeed":
                        if (a.Count < 4) r.Fail("pullSpeed recorded without `core` (the port takes it from config.weapons.vortex.pullCore)");
                        else r.Same(c["result"], GunMath.PullSpeed(N(a[0]), N(a[1]), N(a[2]), N(a[3])), "result");
                        break;
                    case "nextCombo":
                        r.Same(c["result"], GunMath.NextCombo(I(a[0]), N(a[1]), N(a[2]), I(a[3])), "result");
                        break;
                    case "burstRoundAt":
                        r.Same(c["result"], GunMath.BurstRoundAt(I(a[0]), I(a[1]), I(a[2])), "result");
                        break;
                    case "thumperWave":
                    {
                        ThumperWaveResult w = GunMath.ThumperWave((bool)a[0], N(a[1]), ThumperParams.From(cfg, ConfigPath(a[2])));
                        r.SameJson(c["result"], Obj(("radius", w.radius), ("damage", w.damage), ("big", w.big)), "result", true);
                        break;
                    }
                    default:
                        NoRule(r, c);
                        break;
                }
            }
        }

        // --- spawnMath ---------------------------------------------------------------------------------------------

        static StuckTrack Track(object o)
        {
            var d = O(o);
            return new StuckTrack { x = N(d["x"]), y = N(d["y"]), z = N(d["z"]), time = N(d["time"]) };
        }

        public static void CheckSpawnMath(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            var seen = new Dictionary<string, int>();
            foreach (var c in data.Cases("spawnMath"))
            {
                r.where = Label(c, seen);
                r.cases++;
                if (Sequence(c) == "stuckStep")
                {
                    // stuckStep(track, pos, dealtDamage=false, dt, config.spawner.stuckMove, timeout=5) each step;
                    // columns [step, pos.x, track.x, track.time, returned]; pos = (pos.x, 0, 0) (vectors.mjs).
                    double dt = 1 / cfg.Num("loop.simHz");
                    double moveDist = cfg.Num(NoteConfigPath(c));
                    double timeout = NoteNumber(c, "timeout");
                    var track = new StuckTrack();
                    foreach (object row in A(c["rows"]))
                    {
                        var w = A(row);
                        bool ret = SpawnMath.StuckStep(track, new Vec3(N(w[1]), 0, 0), false, dt, moveDist, timeout);
                        int before = r.mismatches.Count;
                        string at = $"step {Golden.Describe(w[0])}";
                        r.Same(w[2], track.x, at + " track.x");
                        r.Same(w[3], track.time, at + " track.time");
                        r.SameBool(w[4], ret, at + " returned");
                        if (r.mismatches.Count > before) break;
                    }
                    continue;
                }
                if (Sequence(c) != null)
                {
                    NoRule(r, c);
                    continue;
                }
                var a = c.TryGetValue("args", out object ao) ? A(ao) : null;
                switch (Fn(c))
                {
                    case "mulberry32":
                    {
                        var rng = new Mulberry32(N(c["seed"]));
                        var first = A(c["first16"]);
                        for (int i = 0; i < first.Count; i++) r.Same(first[i], rng.Next(), $"draw {i + 1}");
                        break;
                    }
                    case "waveBudget":
                        r.Same(c["result"], SpawnMath.WaveBudget(N(a[0]), SpawnParams.From(cfg, ConfigPath(a[1]))), "result");
                        break;
                    case "waveComposition":
                    {
                        var rng = new CountingRng(a[1]);
                        WaveMix m = SpawnMath.WaveComposition(N(a[0]), rng.Next, SpawnParams.From(cfg, ConfigPath(a[2])));
                        r.SameJson(c["result"], Obj(("budget", m.budget), ("husks", m.husks), ("drones", m.drones)), "result", true);
                        rng.CheckDraws(r, After(c)[1], "rng");
                        break;
                    }
                    case "groupSizes":
                    {
                        var rng = new CountingRng(a[1]);
                        List<int> sizes = SpawnMath.GroupSizes(I(a[0]), rng.Next, SpawnParams.From(cfg, ConfigPath(a[2])));
                        r.SameJson(c["result"], sizes, "result");
                        rng.CheckDraws(r, After(c)[1], "rng");
                        break;
                    }
                    case "shuffled":
                    {
                        var rng = new CountingRng(a[1]);
                        List<object> list = A(a[0]);
                        List<object> got = SpawnMath.Shuffled(list, rng.Next);
                        r.SameJson(c["result"], got, "result");
                        r.SameJson(After(c)[0], list, "list after");
                        rng.CheckDraws(r, After(c)[1], "rng");
                        break;
                    }
                    case "waveGroups":
                        r.SameJson(c["result"], SpawnMath.WaveGroups(N(a[0]), N(a[1]), SpawnParams.From(cfg, ConfigPath(a[2]))), "result");
                        break;
                    case "pickSpawnPoint":
                    {
                        var rng = new CountingRng(a[3]);
                        List<Vec3> points = A(a[0]).ConvertAll(V);
                        int i = SpawnMath.PickSpawnPoint(points, V(a[1]), N(a[2]), rng.Next);
                        if (c["result"] == null)
                        {
                            if (i != -1) r.Fail($"result: expected null got point {i}");
                        }
                        else if (i < 0) r.Fail($"result: expected {Golden.Describe(c["result"])} got null");
                        else r.SameVec(c["result"], points[i], $"result (point {i})");
                        rng.CheckDraws(r, After(c)[3], "rng");
                        break;
                    }
                    case "stuckStep":
                    {
                        StuckTrack track = Track(a[0]);
                        bool ret = SpawnMath.StuckStep(track, V(a[1]), (bool)a[2], N(a[3]), N(a[4]), N(a[5]));
                        r.SameBool(c["result"], ret, "result");
                        r.SameJson(After(c)[0], Obj(("x", track.x), ("y", track.y), ("z", track.z), ("time", track.time)), "track after", true);
                        break;
                    }
                    default:
                        NoRule(r, c);
                        break;
                }
            }
        }

        // --- weaponMath --------------------------------------------------------------------------------------------

        static WeaponMods Mods(object o)
        {
            var d = O(o);
            return new WeaponMods { fireRate = N(d["fireRate"]), swap = N(d["swap"]), charge = N(d["charge"]) };
        }

        /// <summary>PerkMath.Effects' flattened result rebuilt in perkEffects' shape and key order.</summary>
        static Dictionary<string, object> PerkJson(PerkEffects fx) => Obj(
            ("bonusPips", fx.bonusPips),
            ("regen", fx.regen.HasValue ? Obj(("delay", fx.regen.Value.delay), ("rate", fx.regen.Value.rate)) : null),
            ("maxHp", fx.maxHp.HasValue ? (object)fx.maxHp.Value : null),
            ("weapons", Obj(("fireRate", fx.fireRate), ("swap", fx.swap), ("charge", fx.charge))),
            ("bonusRideTime", fx.bonusRideTime),
            ("bonusWallJumps", fx.bonusWallJumps),
            ("assist", fx.assistCone.HasValue || fx.assistLockRange.HasValue ? Obj(("cone", Nullable(fx.assistCone)), ("lockRange", Nullable(fx.assistLockRange))) : null),
            ("recoil", fx.recoil),
            ("arsenal", fx.arsenalRecover.HasValue || fx.arsenalPrimedBonus.HasValue ? Obj(("recover", Nullable(fx.arsenalRecover)), ("primedBonus", Nullable(fx.arsenalPrimedBonus))) : null),
            ("points", fx.points),
            ("slide", fx.slideSpeed.HasValue || fx.slideFriction.HasValue || fx.slideJumpCap.HasValue
                ? Obj(("speed", Nullable(fx.slideSpeed)), ("friction", Nullable(fx.slideFriction)), ("jumpCap", Nullable(fx.slideJumpCap)))
                : null),
            ("slam", Obj(("damage", fx.slamDamage), ("radius", fx.slamRadius))),
            ("headshot", fx.headshotMult.HasValue || fx.headshotPoints.HasValue ? Obj(("mult", Nullable(fx.headshotMult)), ("points", Nullable(fx.headshotPoints))) : null),
            ("style", Obj(("drain", fx.styleDrain), ("gain", fx.styleGain))),
            ("staticAura", fx.staticAura),
            ("phoenix", fx.phoenix));

        static object Nullable(double? v) => v.HasValue ? (object)v.Value : null;

        public static void CheckWeaponMath(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            double fileDt = N(data.Vectors("weaponMath")["dt"]);
            var seen = new Dictionary<string, int>();
            foreach (var c in data.Cases("weaponMath"))
            {
                r.where = Label(c, seen);
                r.cases++;
                if (Sequence(c) == "heatStep")
                {
                    // heatStep(h, firing, dt, config.weapons.nailgun) each step from {heat 0, lock 0};
                    // columns [step, firing, heat, lock, overheated].
                    NailgunParams n = NailgunParams.From(cfg, NoteConfigPath(c));
                    var h = new HeatState();
                    foreach (object row in A(c["rows"]))
                    {
                        var w = A(row);
                        bool over = WeaponMath.HeatStep(h, (bool)w[1], fileDt, n);
                        int before = r.mismatches.Count;
                        string at = $"step {Golden.Describe(w[0])}";
                        r.Same(w[2], h.heat, at + " heat");
                        r.Same(w[3], h.@lock, at + " lock");
                        r.SameBool(w[4], over, at + " overheated");
                        if (r.mismatches.Count > before) break;
                    }
                    continue;
                }
                if (Sequence(c) != null)
                {
                    NoRule(r, c);
                    continue;
                }
                var a = Args(c);
                switch (Fn(c))
                {
                    case "splashDamage":
                        r.Same(c["result"], WeaponMath.SplashDamage(N(a[0]), N(a[1]), N(a[2]), N(a[3])), "result");
                        break;
                    case "blastKnockback":
                    {
                        double speed = WeaponMath.BlastKnockback(V(a[0]), V(a[1]), N(a[2]), N(a[3]), N(a[4]), out Vec3 push);
                        r.Same(c["result"], speed, "result");
                        r.SameVec(After(c)[5], push, "out after");
                        break;
                    }
                    case "fireInterval":
                        r.Same(c["result"], WeaponMath.FireInterval(N(a[0]), Mods(a[1])), "result");
                        break;
                    case "swapTime":
                        r.Same(c["result"], WeaponMath.SwapTime(N(a[0]), Mods(a[1])), "result");
                        break;
                    case "chargeTime":
                        r.Same(c["result"], WeaponMath.ChargeTime(N(a[0]), Mods(a[1])), "result");
                        break;
                    case "perkEffects":
                    {
                        // PerkMath.Effects reads config.perks and config.weapons.swapTime itself: the recorded arguments
                        // must be those (else the port can't replay the call).
                        if (ConfigPath(a[1]) != "perks")
                        {
                            r.Fail($"perkEffects recorded with {Golden.Describe(a[1])}; PerkMath.Effects reads config.perks only");
                            break;
                        }
                        if (a.Count < 3 || !GoldenResult.Bits(N(a[2]), cfg.Num("weapons.swapTime")))
                        {
                            r.Fail($"perkEffects recorded with baseSwapTime {(a.Count < 3 ? "(default)" : Golden.Describe(a[2]))}; PerkMath.Effects reads config.weapons.swapTime only");
                            break;
                        }
                        List<string> owned = A(a[0]).ConvertAll(o => (string)o);
                        r.SameJson(c["result"], PerkJson(PerkMath.Effects(owned, cfg)), "result", true);
                        break;
                    }
                    case "perkPrice":
                        r.Same(c["result"], PerkMath.PerkPrice(N(a[0]), I(a[1]), N(a[2]), a.Count > 3 && (bool)a[3]), "result");
                        break;
                    default:
                        NoRule(r, c);
                        break;
                }
            }
        }

        // --- arsenalMath -------------------------------------------------------------------------------------------

        static PrimedSwap Primed(object o)
        {
            if (o == null) return null;
            var d = O(o);
            return new PrimedSwap { weapon = (string)d["weapon"], at = N(d["at"]), used = (bool)d["used"] };
        }

        static ArsenalRecord Record(object o)
        {
            if (o == null) return null;
            var d = O(o);
            var rec = new ArsenalRecord { family = d["family"] as string, at = N(d["at"]) };
            if (d.TryGetValue("statuses", out object st) && st != null)
            {
                foreach (var kv in O(st)) rec.statuses.Set(kv.Key, N(kv.Value));
            }
            else rec.statuses = null;
            return rec;
        }

        public static void CheckArsenalMath(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            var seen = new Dictionary<string, int>();
            foreach (var c in data.Cases("arsenalMath"))
            {
                r.where = Label(c, seen);
                r.cases++;
                if (Sequence(c) != null)
                {
                    NoRule(r, c);
                    continue;
                }
                var a = Args(c);
                switch (Fn(c))
                {
                    case "weaponOf":
                        r.SameStr(c["result"], ArsenalMath.WeaponOf((string)a[0]), "result");
                        break;
                    case "familyOf":
                        r.SameStr(c["result"], ArsenalMath.FamilyOf((string)a[0], WeaponCatalog.From(cfg, ConfigPath(a[1]))), "result");
                        break;
                    case "isMelee":
                        r.SameBool(c["result"], ArsenalMath.IsMelee((string)a[0], WeaponCatalog.From(cfg, ConfigPath(a[1]))), "result");
                        break;
                    case "freshMult":
                        r.Same(c["result"], ArsenalMath.FreshMult(N(a[0]), ArsenalParams.From(cfg, ConfigPath(a[1]))), "result");
                        break;
                    case "spendFresh":
                        r.Same(c["result"], ArsenalMath.SpendFresh(N(a[0]), N(a[1])), "result");
                        break;
                    case "recoverFresh":
                        r.Same(c["result"], a.Count > 3 ? ArsenalMath.RecoverFresh(N(a[0]), N(a[1]), N(a[2]), N(a[3])) : ArsenalMath.RecoverFresh(N(a[0]), N(a[1]), N(a[2])), "result");
                        break;
                    case "primedMult":
                        r.Same(c["result"], ArsenalMath.PrimedMult(Primed(a[0]), (string)a[1], N(a[2]), N(a[3]), N(a[4])), "result");
                        break;
                    case "crossResult":
                    {
                        CrossOutcome x = ArsenalMath.CrossResult(Record(a[0]), a[1] as string, (string)a[2], N(a[3]), ArsenalParams.From(cfg, ConfigPath(a[4])), WeaponCatalog.From(cfg, ConfigPath(a[5])));
                        r.SameJson(c["result"], Obj(("mult", x.mult), ("cross", x.cross), ("combo", x.combo)), "result", true);
                        break;
                    }
                    case "slotForDigit":
                        r.Same(c["result"], ArsenalMath.SlotForDigit((string)a[0], I(a[1])), "result");
                        break;
                    case "cycleSlot":
                        r.Same(c["result"], ArsenalMath.CycleSlot(I(a[0]), I(a[1]), N(a[2])), "result");
                        break;
                    case "radialSector":
                        r.Same(c["result"], ArsenalMath.RadialSector(N(a[0]), N(a[1]), I(a[2]), N(a[3])), "result");
                        break;
                    default:
                        NoRule(r, c);
                        break;
                }
            }
        }

        // --- momentumMath ------------------------------------------------------------------------------------------

        public static void CheckMomentumMath(GoldenData data, GoldenResult r)
        {
            GameConfig cfg = data.Config;
            double fileDt = N(data.Vectors("momentumMath")["dt"]);
            var seen = new Dictionary<string, int>();
            foreach (var c in data.Cases("momentumMath"))
            {
                r.where = Label(c, seen);
                r.cases++;
                if (Sequence(c) == "smoothVelocity")
                {
                    // smoothVelocity(smooth, v = (24, 0, 0), dt, config.momentum.smoothTime) each step from (0, 0, 0);
                    // columns [step, x, y, z].
                    double tau = cfg.Num(NoteConfigPath(c));
                    Vec3 v = NoteVector(c, "v =");
                    Vec3 smooth = NoteVector(c, "from");
                    foreach (object row in A(c["rows"]))
                    {
                        var w = A(row);
                        MomentumMath.SmoothVelocity(ref smooth, v, fileDt, tau);
                        int before = r.mismatches.Count;
                        string at = $"step {Golden.Describe(w[0])}";
                        r.Same(w[1], smooth.x, at + " x");
                        r.Same(w[2], smooth.y, at + " y");
                        r.Same(w[3], smooth.z, at + " z");
                        if (r.mismatches.Count > before) break;
                    }
                    continue;
                }
                if (Sequence(c) != null)
                {
                    NoRule(r, c);
                    continue;
                }
                var a = Args(c);
                switch (Fn(c))
                {
                    case "smoothVelocity":
                    {
                        Vec3 smooth = V(a[0]);
                        Vec3 ret = MomentumMath.SmoothVelocity(ref smooth, V(a[1]), N(a[2]), N(a[3]));
                        r.SameVec(c["result"], ret, "result");
                        r.SameVec(After(c)[0], smooth, "smooth after");
                        break;
                    }
                    case "closingSpeed":
                        r.Same(c["result"], MomentumMath.ClosingSpeed(V(a[0]), V(a[1]), V(a[2])), "result");
                        break;
                    case "surgeOf":
                    {
                        double[] ring = A(a[0]).ConvertAll(N).ToArray();
                        r.Same(c["result"], MomentumMath.SurgeOf(ring, N(a[1])), "result");
                        break;
                    }
                    case "momentumOf":
                        r.Same(c["result"], MomentumMath.MomentumOf(N(a[0]), N(a[1]), MomentumParams.From(cfg, ConfigPath(a[2]))), "result");
                        break;
                    case "momentumDamage":
                        r.Same(c["result"], MomentumMath.MomentumDamage(N(a[0]), (bool)a[1], MomentumParams.From(cfg, ConfigPath(a[2]))), "result");
                        break;
                    case "momentumRadius":
                        r.Same(c["result"], MomentumMath.MomentumRadius(N(a[0]), MomentumParams.From(cfg, ConfigPath(a[1]))), "result");
                        break;
                    case "impactWave":
                    {
                        ImpactWaveResult w = MomentumMath.ImpactWave(N(a[0]), N(a[1]), ImpactWaveParams.From(cfg, ConfigPath(a[2])));
                        r.SameJson(c["result"], Obj(("fires", w.fires), ("share", w.share), ("knockback", w.knockback)), "result", true);
                        break;
                    }
                    default:
                        NoRule(r, c);
                        break;
                }
            }
        }
    }
}
