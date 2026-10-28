// NIGHT13 U3-26.12: the local run log (NIGHT13-RESEARCH section 6 "now": testers attach evidence; nothing leaves the
// machine). One JSONL file per run under Application.persistentDataPath/runs (UnityTuning.RunLogFolder), named
// run-<UTC start yyyyMMdd-HHmmss>-<seed>.jsonl; the newest UnityTuning.RunLogKeep files are kept.
//   line 1, the header: {"t":0,"e":"header","version":"SKYGRIND 0.1.0 (abc1234)","schema":1,"utc":...,"seed":...,
//     "seedCode":...,"mode":"survival"|"grind","heat":N,"mutators":[...],"lab":null|"<names>","assist":...,"settings":...}
//   then one line per gameplay event, with the web's event names (PORT-STATUS.md's list): {"t":<run sim time s>,"e":"kill",
//     "weapon":"kestrel","kind":"husk","head":true,"melee":false}
//   last, the footer: {"t":...,"e":"footer","outcome":"results"|"abandoned"|"quit","round":N,"kills":N,"headshots":N,
//     "earned":N,"time":s,"grade":"B"|null,"killedBy":...,"exfil":...}
// The events logged are the run's milestones only (rounds, kills, a death, every buy, gates, perks, THE FORGE, wall guns,
// the box, power-ups, mods, the run shape's objectives / relays / power / swarm / exfil, the meta's contracts, unlocks,
// mastery and HEAT): none fires every step. Per-hit and per-shot events (hit, fire, hurt, shotResult) are left out.
// PORT-RULES section 6: a line is built per gameplay event (a kill allocates its line), never per step or frame; the file is
// written off the sim: lines are buffered, handed to a worker thread at a round's clear (at the frame's end, never inside a
// sim step), and at the run's end; a quit, COPY REPORT and the smoke write synchronously.
// Off when the setting UnityTuning.RunLogSetting is false (its row is the lead's; the default is on when the row is
// missing), for automation (a Player started with a PlayerChecks flag, or noRecords / profileReadOnly at the run's start;
// `automationFolder` lets the smoke log into a folder of its own instead), and on a Web build (no file system).
// It only reads: every handler runs after the systems' own (EventBus order: registered last), changes nothing, and a
// failure inside it stops this run's log with one warning, never the game.
// Wiring: RunLog.Attach(game) once, after the run exists (StartGame); it hooks game.OnNewRun (a run starts), the bus's
// runEnd (the footer is written at that frame's end through game.OnRender, once EndRun has filled the grade) and
// Application.quitting.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class RunLog
    {
        /// <summary>The attached log (null before Attach).</summary>
        public static RunLog Instance { get; private set; }

        /// <summary>Attach the run log to the game (once, after its run exists); a second call for the same game returns it.</summary>
        public static RunLog Attach(SurvivalGame game)
        {
            if (Instance != null)
            {
                if (ReferenceEquals(Instance.game, game)) return Instance;
                Instance.Detach(); // a new Play session with domain reload off: the old game is gone
            }
            Instance = new RunLog(game);
            return Instance;
        }

        /// <summary>persistentDataPath/runs: where the player's runs are logged.</summary>
        public static string DefaultFolder => Path.Combine(Application.persistentDataPath, UnityTuning.RunLogFolder);

        /// <summary>Automation (the smoke): when set, an automation run logs into this folder instead of not at all.</summary>
        public string automationFolder;

        /// <summary>The file of the run being logged, or of the last one written this session (null: none yet).</summary>
        public string LastPath { get; private set; }

        /// <summary>Lines handed to that file so far (header and footer included).</summary>
        public int LinesWritten { get; private set; }

        /// <summary>The running run is being logged.</summary>
        public bool Logging => path != null;

        /// <summary>The last write error (null: none).</summary>
        public string LastError { get; private set; }

        static readonly Encoding Utf8 = new UTF8Encoding(false);
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        readonly SurvivalGame game;
        readonly List<Action> offs = new List<Action>();
        readonly List<string> buffer = new List<string>(64);
        readonly JsonLine line = new JsonLine();
        readonly Action onNewRun, onQuit;
        readonly Action<double, double> onRender;
        readonly bool automationProcess;
        string path; // the running run's file (null: this run isn't logged)
        bool automationRun, headerDone, fileStarted, flushDue;
        RunStats runStats; // the logged run's stats (taken at its first roundBreak, inside run.Start)
        RunStats ended; // runEnd's stats: the footer waits for the frame's end (EndRun fills the grade after run.End)
        DateTime startUtc;
        Task chain = Task.CompletedTask; // the worker's writes, in order

        RunLog(SurvivalGame game)
        {
            this.game = game;
            automationProcess = RunLogMath.IsAutomation(Environment.GetCommandLineArgs());
            onNewRun = OnNewRun;
            onRender = OnFrame;
            onQuit = OnQuit;
            game.OnNewRun += onNewRun;
            game.OnRender += onRender;
            Application.quitting += onQuit;
            HookEvents();
        }

        void Detach()
        {
            foreach (Action off in offs) off();
            offs.Clear();
            game.OnNewRun -= onNewRun;
            game.OnRender -= onRender;
            Application.quitting -= onQuit;
            path = null;
        }

        // --- the run's start and end ---------------------------------------------------------------------------------

        void OnNewRun()
        {
            if (path != null) Finish(false, false); // the last run's footer first (an abandoned run, or RESTART's judged one)
            ended = null;
            runStats = null;
            buffer.Clear();
            headerDone = fileStarted = flushDue = false;
            LinesWritten = 0;
            path = null;
            automationRun = automationProcess || game.noRecords || game.profileReadOnly || game.sim.metaSilent; // NIGHT14 rule 7: a bot's run
            string folder = automationRun ? automationFolder : SettingOn() ? DefaultFolder : null;
#if UNITY_WEBGL && !UNITY_EDITOR
            folder = null; // a Web build's persistentDataPath is IndexedDB: no run log there
#endif
            if (string.IsNullOrEmpty(folder)) return;
            startUtc = DateTime.UtcNow;
            path = UniquePath(folder, RunLogMath.FileName(startUtc, game.runSeed));
            LastPath = path;
        }

        bool SettingOn()
        {
            try
            {
                return game.settings.Bool(UnityTuning.RunLogSetting);
            }
            catch (Exception)
            {
                return UnityTuning.RunLogDefault; // the settings row isn't there (yet)
            }
        }

        string UniquePath(string folder, string name)
        {
            string p = Path.Combine(folder, name);
            string stem = name.Substring(0, name.Length - ".jsonl".Length);
            for (int n = 2; (p == LastPath || File.Exists(p)) && n < 100; n++) p = Path.Combine(folder, $"{stem}-{n}.jsonl"); // two runs in one second on one seed
            return p;
        }

        /// <summary>Frame end (game.OnRender): a pending footer, else a write a round's clear asked for. Two field reads otherwise.</summary>
        void OnFrame(double frameDt, double alpha)
        {
            if (ended != null) Finish(false, false);
            else if (flushDue) Flush(false);
        }

        void OnQuit()
        {
            if (path != null) Finish(true, true);
            else Wait();
        }

        /// <summary>The footer, the last write, and the log closed for this run.</summary>
        void Finish(bool sync, bool quitting)
        {
            RunStats s = ended;
            ended = null;
            if (path == null) return;
            RunStats st = s ?? runStats ?? game.run.stats;
            bool judged = s != null && ReferenceEquals(game.lastStats, s); // EndRun ran: RESULTS (a death, an exfil, PAUSED's RESTART)
            string outcome = judged ? "results" : s == null && quitting ? "quit" : "abandoned";
            buffer.Add(line.Begin(RunTime, "footer")
                .Add("outcome", outcome)
                .Add("round", s != null ? s.round : game.run.round)
                .Add("kills", st.kills)
                .Add("headshots", st.headshots)
                .Add("earned", s != null ? s.earned : game.economy.earned)
                .Add("time", s != null ? s.time : game.run.Time)
                .Add("grade", judged ? s.grade : null)
                .Add("killedBy", st.killedBy)
                .Add("exfil", st.exfil)
                .End());
            Flush(sync);
            path = null;
        }

        /// <summary>
        /// Write what's buffered now and wait for the writes (COPY REPORT, the smoke): a pending footer is written first.
        /// The run keeps logging when it's still going.
        /// </summary>
        public void FlushNow()
        {
            if (ended != null) Finish(true, false);
            else Flush(true);
            Wait();
        }

        /// <summary>The file COPY REPORT names: this session's last run log, else the newest one on disk (null: none).</summary>
        public string NewestFile()
        {
            if (LastPath != null && File.Exists(LastPath)) return LastPath;
            try
            {
                string dir = DefaultFolder;
                if (!Directory.Exists(dir)) return null;
                string newest = null;
                foreach (string f in Directory.GetFiles(dir, "run-*.jsonl"))
                    if (newest == null || string.CompareOrdinal(Path.GetFileName(f), Path.GetFileName(newest)) > 0) newest = f;
                return newest;
            }
            catch (Exception)
            {
                return null;
            }
        }

        double RunTime => game.run != null ? game.run.Time : 0;

        // --- writing -------------------------------------------------------------------------------------------------

        void Flush(bool sync)
        {
            flushDue = false;
            if (path == null)
            {
                if (sync) Wait();
                return;
            }
            if (!headerDone)
            {
                buffer.Insert(0, HeaderLine());
                headerDone = true;
            }
            if (buffer.Count == 0)
            {
                if (sync) Wait();
                return;
            }
            var text = new StringBuilder(buffer.Count * 96);
            foreach (string l in buffer) text.Append(l).Append('\n');
            LinesWritten += buffer.Count;
            buffer.Clear();
            string p = path, t = text.ToString();
            bool first = !fileStarted;
            fileStarted = true;
            if (sync)
            {
                Wait();
                Append(p, t, first);
            }
            else chain = chain.ContinueWith(_ => Append(p, t, first), TaskScheduler.Default);
        }

        void Wait()
        {
            try
            {
                chain.Wait(UnityTuning.RunLogWaitMs);
            }
            catch (Exception)
            {
                // a write's own error is in LastError
            }
        }

        /// <summary>Append to the run's file (the first write makes the folder, then keeps the newest RunLogKeep files).</summary>
        void Append(string p, string text, bool first)
        {
            try
            {
                string dir = Path.GetDirectoryName(p);
                if (first && !string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(p, text, Utf8);
                if (first && !string.IsNullOrEmpty(dir))
                    foreach (string old in RunLogMath.ToPrune(Directory.GetFiles(dir, "run-*.jsonl"), UnityTuning.RunLogKeep)) File.Delete(old);
            }
            catch (Exception e)
            {
                LastError = e.Message;
                Debug.LogWarning($"[runlog] could not write {p}: {e.Message}"); // thread-safe in Unity
            }
        }

        string HeaderLine()
        {
            RunStats s = runStats ?? game.run.stats;
            JsonLine h = line.Begin(0, "header")
                .Add("version", BuildStamp.Line)
                .Add("schema", 1)
                .Add("utc", startUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", Invariant))
                .Add("seed", (double)game.runSeed)
                .Add("seedCode", s.seedCode)
                .Add("mode", game.setup.mode == RunSetup.Grind ? RunSetup.Grind : RunSetup.Survival)
                .Add("heat", s.heat)
                .Add("mutators", s.mutators)
                .Add("lab", game.lab != null && game.lab.Any ? game.lab.Names() : null)
                .Add("assist", game.AssistMode)
                .Add("settings", SettingsSummary(game.settings));
            if (automationRun) h.Add("automation", true);
            return h.End();
        }

        /// <summary>The non-default settings, "key=value, ..." ("all default"); "unavailable" if the store can't be read.</summary>
        public static string SettingsSummary(GameSettings settings)
        {
            try
            {
                SettingsStore store = settings.store;
                return RunLogMath.SettingsSummary(store.items, store.values, store.defaults);
            }
            catch (Exception)
            {
                return "unavailable";
            }
        }

        // --- the events (registered last: they run after every system's own handler) ------------------------------------

        JsonLine Ev(string e) => line.Begin(RunTime, e);

        void Push()
        {
            buffer.Add(line.End());
            if (buffer.Count >= UnityTuning.RunLogFlushLines) flushDue = true;
        }

        void Hook(string name, Action<object> fn)
        {
            offs.Add(game.events.On(name, o =>
            {
                if (path == null) return; // not logging: one field read
                try
                {
                    fn(o);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[runlog] '{name}' could not be logged ({e.GetType().Name}: {e.Message}): this run's log stops here");
                    buffer.Clear();
                    path = null;
                }
            }));
        }

        void HookEvents()
        {
            // the run (survival.js)
            Hook("roundBreak", o =>
            {
                if (runStats == null) runStats = game.run.stats; // run.Start's first break: this run's stats object
                JsonLine j = Ev("roundBreak");
                if (o is RoundEvent r) j.Add("round", r.number).Add("time", r.time);
                Push();
            });
            Hook("roundBegin", o =>
            {
                JsonLine j = Ev("roundBegin");
                if (o is RoundEvent r) j.Add("round", r.number);
                Push();
            });
            Hook("roundClear", o =>
            {
                JsonLine j = Ev("roundClear");
                if (o is RoundEvent r) j.Add("round", r.number).Add("points", r.points);
                Push();
                flushDue = true; // the round's lines go to the file at this frame's end
            });
            Hook("ready", o =>
            {
                JsonLine j = Ev("ready");
                if (o is RoundEvent r) j.Add("round", r.number);
                Push();
            });
            Hook("runEnd", o =>
            {
                ended = o as RunStats ?? runStats ?? game.run.stats; // the footer at this frame's end (EndRun fills the grade first)
            });
            // the player
            Hook("kill", o =>
            {
                JsonLine j = Ev("kill");
                if (o is KillEvent k)
                {
                    j.Add("weapon", k.weapon).Add("kind", k.kind).Add("head", k.headshot).Add("melee", k.melee);
                    if (k.owner != "player") j.Add("owner", k.owner);
                }
                Push();
            });
            Hook("death", o =>
            {
                if (!(o is DeathEvent d) || d.owner != "player") return;
                Vec3 f = game.Feet;
                Ev("death").Add("source", d.source).Add("round", game.run.round).Add("zone", game.zones.ZoneAt(new Vec3(f.x, f.y + 1, f.z)));
                Push();
            });
            // the economy and its buys
            Hook("buy", o =>
            {
                JsonLine j = Ev("buy");
                if (o is SpentEvent s) j.Add("what", s.what).Add("price", s.price);
                Push();
            });
            Hook("gateOpen", o =>
            {
                JsonLine j = Ev("gateOpen");
                if (o is GateOpenEvent g) j.Add("zone", g.zone).Add("cost", g.cost).Add("free", g.free);
                Push();
            });
            Hook("perk", o =>
            {
                JsonLine j = Ev("perk");
                if (o is PerkEvent p) j.Add("id", p.id).Add("price", p.price).Add("free", p.free);
                Push();
            });
            Hook("forge", o =>
            {
                JsonLine j = Ev("forge");
                if (o is ForgeEvent fe) j.Add("weapon", fe.weapon).Add("price", fe.price).Add("free", fe.free);
                Push();
            });
            Hook("weaponBought", o =>
            {
                Ev("weaponBought").Add("weapon", o as string);
                Push();
            });
            Hook("boxRoll", o =>
            {
                JsonLine j = Ev("boxRoll");
                if (o is BoxRollEvent b) j.Add("weapon", b.weapon).Add("pull", b.pull);
                Push();
            });
            Hook("powerUpGrab", o =>
            {
                JsonLine j = Ev("powerUpGrab");
                if (o is PowerUpEvent p) j.Add("type", p.type);
                Push();
            });
            Hook("modPick", o =>
            {
                JsonLine j = Ev("modPick");
                if (o is ModEvent m) j.Add("id", m.id).Add("stacks", m.stacks).Add("free", m.free);
                Push();
            });
            // the run's shape (objectives.js, protocol.js; the port-only power, swarm drop and exfil)
            Hook("objectiveStart", o =>
            {
                JsonLine j = Ev("objectiveStart");
                if (o is ObjectiveStartEvent s) j.Add("type", s.type).Add("round", s.round);
                Push();
            });
            Hook("objectiveDone", o =>
            {
                JsonLine j = Ev("objectiveDone");
                if (o is ObjectiveDoneEvent d) j.Add("type", d.type).Add("round", d.round).Add("bonus", d.bonus);
                Push();
            });
            Hook("relayLit", o =>
            {
                JsonLine j = Ev("relayLit");
                if (o is RelayLitEvent r) j.Add("id", r.id).Add("count", r.count).Add("total", r.total);
                Push();
            });
            Hook("protocolComplete", o =>
            {
                JsonLine j = Ev("protocolComplete");
                if (o is ProtocolCompleteEvent p) j.Add("newFinish", p.newFinish);
                Push();
            });
            Hook("secretRelay", o =>
            {
                JsonLine j = Ev("secretRelay");
                if (o is SecretRelayEvent s) j.Add("id", s.id).Add("newFinish", s.newFinish);
                Push();
            });
            Hook("powerOn", o =>
            {
                Ev("powerOn");
                Push();
            });
            Hook("swarmRound", o =>
            {
                JsonLine j = Ev("swarmRound");
                if (o is SwarmRoundEvent s) j.Add("round", s.round);
                Push();
            });
            foreach (string name in new[] { "exfilCall", "exfilStart", "exfilReach", "exfilDone", "exfilFail" })
            {
                string n = name;
                Hook(n, o =>
                {
                    JsonLine j = Ev(n);
                    if (o is ExfilEvent x)
                    {
                        j.Add("round", x.round).Add("zone", x.zone).Add("kills", x.kills).Add("needed", x.needed);
                        if (x.reason != null) j.Add("reason", x.reason);
                    }
                    Push();
                });
            }
            // the meta (contracts, unlocks, mastery, HEAT, the DAILY)
            Hook(MetaEvents.ContractDone, o =>
            {
                JsonLine j = Ev(MetaEvents.ContractDone);
                if (o is ContractEvent c) j.Add("id", c.id).Add("unlock", c.unlock);
                Push();
            });
            Hook(MetaEvents.UnlockEarned, o =>
            {
                JsonLine j = Ev(MetaEvents.UnlockEarned);
                if (o is UnlockEvent u) j.Add("flag", u.flag);
                Push();
            });
            Hook(MetaEvents.MasteryTier, o =>
            {
                JsonLine j = Ev(MetaEvents.MasteryTier);
                if (o is MasteryEvent m) j.Add("weapon", m.weapon).Add("tier", m.tier).Add("finish", m.finish);
                Push();
            });
            Hook(MetaEvents.HeatUnlock, o =>
            {
                JsonLine j = Ev(MetaEvents.HeatUnlock);
                if (o is HeatEvent h) j.Add("rung", h.rung);
                Push();
            });
            Hook(MetaEvents.MutatorUnlock, o =>
            {
                JsonLine j = Ev(MetaEvents.MutatorUnlock);
                if (o is MutatorEvent m) j.Add("id", m.id);
                Push();
            });
        }
    }

    /// <summary>The run log's pure parts (file names, pruning, automation, JSON values, the settings summary).</summary>
    public static class RunLogMath
    {
        /// <summary>"run-&lt;UTC yyyyMMdd-HHmmss&gt;-&lt;seed&gt;.jsonl": the names sort by start time.</summary>
        public static string FileName(DateTime utc, uint seed) =>
            "run-" + utc.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + seed.ToString(CultureInfo.InvariantCulture) + ".jsonl";

        /// <summary>
        /// Which run files go so the newest `keep` stay: the run-*.jsonl names (paths or bare names) sorted by name (their UTC
        /// start), all but the last `keep`. Other files are never touched.
        /// </summary>
        public static List<string> ToPrune(IEnumerable<string> files, int keep)
        {
            var runs = new List<string>();
            foreach (string f in files)
            {
                if (f == null) continue;
                string n = Path.GetFileName(f);
                if (n.StartsWith("run-", StringComparison.Ordinal) && n.EndsWith(".jsonl", StringComparison.Ordinal)) runs.Add(f);
            }
            runs.Sort((a, b) => string.CompareOrdinal(Path.GetFileName(a), Path.GetFileName(b)));
            int drop = runs.Count - Math.Max(0, keep);
            return drop > 0 ? runs.GetRange(0, drop) : new List<string>();
        }

        /// <summary>A command line holding one of PlayerChecks' flags (UnityTuning.AutomationFlags): an automation run.</summary>
        public static bool IsAutomation(string[] args)
        {
            if (args == null) return false;
            foreach (string a in args)
                if (a != null && Array.IndexOf(UnityTuning.AutomationFlags, a) >= 0) return true;
            return false;
        }

        /// <summary>A JSON string (quotes, backslashes and control characters escaped), or null.</summary>
        public static void AppendString(StringBuilder sb, string s)
        {
            if (s == null)
            {
                sb.Append("null");
                return;
            }
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        /// <summary>A JSON number in the invariant culture to 3 decimals (null for NaN and the infinities).</summary>
        public static void AppendNumber(StringBuilder sb, double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) sb.Append("null");
            else sb.Append(v.ToString("0.###", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The settings that differ from their defaults, in the rows' order: "key=value, ..." (values as the web's keys:
        /// SettingsMath.Key); "all default" when none does. A row with no value or default is skipped.
        /// </summary>
        public static string SettingsSummary(IList<SettingItem> items, IDictionary<string, object> values, IDictionary<string, object> defaults)
        {
            var sb = new StringBuilder();
            foreach (SettingItem item in items)
            {
                if (item?.key == null || !values.TryGetValue(item.key, out object v) || !defaults.TryGetValue(item.key, out object d)) continue;
                string vs = SettingsMath.Key(v), ds = SettingsMath.Key(d);
                if (vs == ds) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(item.key).Append('=').Append(vs);
            }
            return sb.Length > 0 ? sb.ToString() : "all default";
        }
    }

    /// <summary>One JSON object on one line, built field by field (reused: Begin starts a fresh line).</summary>
    public sealed class JsonLine
    {
        readonly StringBuilder sb = new StringBuilder(256);

        /// <summary>{"t":&lt;t&gt;,"e":"&lt;e&gt;" ...</summary>
        public JsonLine Begin(double t, string e)
        {
            sb.Clear();
            sb.Append("{\"t\":");
            RunLogMath.AppendNumber(sb, t);
            sb.Append(",\"e\":");
            RunLogMath.AppendString(sb, e);
            return this;
        }

        JsonLine Key(string key)
        {
            sb.Append(",\"").Append(key).Append("\":");
            return this;
        }

        public JsonLine Add(string key, string value)
        {
            Key(key);
            RunLogMath.AppendString(sb, value);
            return this;
        }

        public JsonLine Add(string key, double value)
        {
            Key(key);
            RunLogMath.AppendNumber(sb, value);
            return this;
        }

        public JsonLine Add(string key, int value)
        {
            Key(key);
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        public JsonLine Add(string key, bool value)
        {
            Key(key);
            sb.Append(value ? "true" : "false");
            return this;
        }

        /// <summary>A list of strings as a JSON array (null: []).</summary>
        public JsonLine Add(string key, IList<string> values)
        {
            Key(key);
            sb.Append('[');
            if (values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    RunLogMath.AppendString(sb, values[i]);
                }
            }
            sb.Append(']');
            return this;
        }

        /// <summary>The line, closed.</summary>
        public string End()
        {
            sb.Append('}');
            return sb.ToString();
        }
    }
}
