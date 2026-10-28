#!/usr/bin/env python3
"""NIGHT26 shared commit generator (the lead's fork). Run once from the repo root: python Tools/night/n26-shared-patch.py
Creates N26Blocks / SurvivalSim.N26 / SurvivalGame.N26 / Ui.N26 / FeelReel.N26 / Harness.N26 / N26SharedTests and patches the call sites."""
import io, os, re, subprocess

def rd(p):
    return io.open(p, encoding='utf-8', newline='').read()

def wr(p, s):
    io.open(p, 'w', encoding='utf-8', newline='').write(s)

def nlof(s):
    return '\r\n' if '\r\n' in s else '\n'

def edit(p, old, new, count=1):
    s = rd(p)
    nl = nlof(s)
    o = old.replace('\n', nl)
    n = new.replace('\n', nl)
    assert o in s, (p, old[:70])
    wr(p, s.replace(o, n, count))

def new(p, text):
    wr(p, text.replace('\n', '\r\n'))

G = 'Assets/Scripts/Game/'

# ---- Core/N26Blocks.cs
new('Assets/Scripts/Core/N26Blocks.cs', '''// NIGHT26 (the shared commit, the lead): the seven NIGHT26 design blocks a run reads, one bit each, so every NIGHT26 feature can be
// switched off (NIGHT26.md "Switches"). With every block off the game is NIGHT25's bit for bit: the A side for every judge
// (`--off26` / `-skygrindOff26`); `--off25`, `--off24`, `--off22`, `--off21`, `--off20` and `--off19` also turn these off. Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n26), a tape records them
// (format 10: one byte after the NIGHT25 byte; formats 1-9 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.n26.pulseLT (W: the PULSE GAUNTLET's blast is the LEFT TRIGGER), weaponIdentity (W: every weapon's LT, costs,
// enemy vulnerabilities, the rocket and BOUNCER nerfs, the unlock order), surfaces (M: ice and boost strips through Mover.groundFrictionMult),
// levelSelect (L: the ULTRAKILL-style level select, ranks, CAMPAIGN as the default mode), level1 / level2 / level3 (L, B: the first three
// levels and their bosses).
namespace Skygrind.Core
{
    public struct N26Blocks
    {
        public bool pulseLT, weaponIdentity, surfaces, levelSelect, level1, level2, level3;

        /// <summary>The number of blocks (the tape's NIGHT26 byte, bit i = block i).</summary>
        public const int Count = 7;

        /// <summary>The harnesses' and the Player checks' switch (`--off26`, `-skygrindOff26`; also `--off25` and older): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT25's game.</summary>
        public static N26Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => pulseLT || weaponIdentity || surfaces || levelSelect || level1 || level2 || level3;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (pulseLT and weaponIdentity
        /// change a survival run's combat; surfaces only act where a level plan places them; the level blocks change only campaign runs.)</summary>
        public bool SimChanging => pulseLT || weaponIdentity;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT25's config).</summary>
        public static N26Blocks From(GameConfig cfg) => new N26Blocks
        {
            pulseLT = cfg.Design("n26.pulseLT.enabled"),
            weaponIdentity = cfg.Design("n26.weaponIdentity.enabled"),
            surfaces = cfg.Design("n26.surfaces.enabled"),
            levelSelect = cfg.Design("n26.levelSelect.enabled"),
            level1 = cfg.Design("n26.level1.enabled"),
            level2 = cfg.Design("n26.level2.enabled"),
            level3 = cfg.Design("n26.level3.enabled"),
        };

        /// <summary>Block i (0 pulseLT, 1 weaponIdentity, 2 surfaces, 3 levelSelect, 4 level1, 5 level2, 6 level3).</summary>
        public bool Flag(int i) => i == 0 ? pulseLT : i == 1 ? weaponIdentity : i == 2 ? surfaces : i == 3 ? levelSelect : i == 4 ? level1 : i == 5 ? level2 : i == 6 && level3;

        /// <summary>The blocks as a value (bit i = block i): what a tape records.</summary>
        public int Bits
        {
            get
            {
                int b = 0;
                for (int i = 0; i < Count; i++)
                    if (Flag(i)) b |= 1 << i;
                return b;
            }
        }

        /// <summary>The blocks from a tape's value.</summary>
        public static N26Blocks FromBits(int b) => new N26Blocks
        {
            pulseLT = (b & 1) != 0, weaponIdentity = (b & 2) != 0, surfaces = (b & 4) != 0, levelSelect = (b & 8) != 0,
            level1 = (b & 16) != 0, level2 = (b & 32) != 0, level3 = (b & 64) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "pulseLT", "weaponIdentity", "surfaces", "levelSelect", "level1", "level2", "level3" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT26 block off";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (!Flag(i)) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Names[i]);
            }
            return sb.Append(" on").ToString();
        }
    }
}
''')
wr('Assets/Scripts/Core/N26Blocks.cs.meta', rd('Assets/Scripts/Core/N25Blocks.cs.meta').replace('\r\n', '\n').replace(
    [l for l in rd('Assets/Scripts/Core/N25Blocks.cs.meta').splitlines() if l.startswith('guid:')][0], 'guid: 26a1c0de26a1c0de26a1c0de26a10001'))

# ---- Game/SurvivalSim.N26.cs
new(G + 'SurvivalSim.N26.cs', '''// NIGHT26 (the shared commit, the lead owns this file): the seven NIGHT26 design blocks in the sim (Core/N26Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n26Override), --off26 / -skygrindOff26 (and --off25 and older)
// take every block off (N26Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (nothing runs without a body); each is called once at the run's start or once per step from the lead's call
// sites (SurvivalSim.Run.cs).
// - W (weapons) implements N26WeaponsStartRun / N26WeaponsStep (Game/SurvivalSim.Weapons26.cs).
// - M (movement) implements N26SurfaceStartRun / N26SurfaceStep (Game/SurvivalSim.Surface26.cs).
// - B (bosses) implements N26BossStartRun / N26BossStep (Game/SurvivalSim.Boss26.cs).
// - L (levels) implements N26LevelStartRun / N26LevelStep (Game/SurvivalSim.Level26.cs).
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT26 blocks, read once.</summary>
        N26Blocks n26Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N26Blocks? n26Override;

        /// <summary>The NIGHT26 blocks this run read (what a tape records).</summary>
        public N26Blocks n26;

        void N26Init() => n26Design = N26Blocks.From(cfg);

        void N26StartRun()
        {
            n26 = n26Override ?? (N26Blocks.ForceOff ? N26Blocks.Off : n26Design);
            N26WeaponsStartRun();
            N26SurfaceStartRun();
            N26BossStartRun();
            N26LevelStartRun();
        }

        partial void N26WeaponsStartRun();
        partial void N26WeaponsStep(double dt);
        partial void N26SurfaceStartRun();
        partial void N26SurfaceStep(double dt);
        partial void N26BossStartRun();
        partial void N26BossStep(double dt);
        partial void N26LevelStartRun();
        partial void N26LevelStep(double dt);
    }
}
''')

new(G + 'SurvivalGame.N26.cs', '''// NIGHT26 (the shared commit, the lead owns this file): the hooks the NIGHT26 world views hang on. A builder implements the
// declaration-only partial in its own file and names the one call in its report. Nothing runs without a body.
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>Once at game init, after the NIGHT25 views.</summary>
        void N26InitViews() => N26InitViewsHook();

        /// <summary>Every render frame beside the other world views (worldDt: the slowed world frame time; alpha: the sim's render interpolation).</summary>
        void N26RenderViews(double worldDt, double alpha) => N26RenderViewsHook(worldDt, alpha);

        partial void N26InitViewsHook();
        partial void N26RenderViewsHook(double worldDt, double alpha);
    }
}
''')

new(G + 'Ui.N26.cs', '''// NIGHT26 (the shared commit, the lead owns this file): the HUD hooks for NIGHT26's widgets (the level select, the ranks, the gap meter).
// A builder implements the declaration-only partials in its own file (UiLevelSelect26.cs, ...). Wiring: Ui.BuildHud calls BuildN26Hud()
// after BuildN25Hud(); Ui.Render calls RenderN26Hud(frameDt) after RenderN25Hud(frameDt).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        void BuildN26Hud() => BuildN26HudHook();
        void RenderN26Hud(double frameDt) => RenderN26HudHook(frameDt);

        partial void BuildN26HudHook();
        partial void RenderN26HudHook(double frameDt);
    }
}
''')

# ---- FeelReel.N26.cs
ids = ['pulselt', 'weapons', 'ice', 'hulk', 'glacier', 'wyrm26', 'level1', 'level3', 'levelselect', 'ranks']
owners = {'pulselt': 'W', 'weapons': 'W', 'ice': 'M', 'hulk': 'B', 'glacier': 'B', 'wyrm26': 'B', 'level1': 'L', 'level3': 'L', 'levelselect': 'L', 'ranks': 'L'}
desc = {'pulselt': 'the PULSE blast on the left trigger (charge by use, release to launch)', 'weapons': 'every weapon has an LT and a cost; the weapon-by-enemy table',
        'ice': 'ice and boost strips: friction multiplier, speed, stopping distance', 'hulk': 'THE SLAG HULK, the easy level-1 boss', 'glacier': 'THE GLACIER, the ice chase boss (level 2)',
        'wyrm26': 'the worm: proximity aggro, body x0.5 / head x2, the final attack (level 3)', 'level1': 'level 1: narrower, platforming run, wall-ride hall',
        'level3': 'level 3: THE KILN rooms and the worm arena', 'levelselect': 'the level select screen', 'ranks': 'time / kills / style ranks, aggregate, P rank, best per level'}
def cap(i):
    return ''.join(w.capitalize() for w in re.split(r'(?<=[a-z])(?=\d)|_', i)) if False else i[0].upper() + i[1:]
hooks = '\n'.join('        static partial void N26%sBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);' % cap(i) for i in ids)
cases = '\n'.join('                case "%s": N26%sBody(g, dir, res, offs, pre, ref body); break;' % (i, cap(i)) for i in ids)
table = '\n'.join('//   %-12s %s   %s' % (i, owners[i], desc[i]) for i in ids)
phase = ', '.join('"%s"' % i for i in ids)
new(G + 'FeelReel.N26.cs', '''// NIGHT26 (the lead, phase I): the reel scenarios for NIGHT26's rows, on the Player, proof-first (NIGHT26.md): a scenario is written and
// seen RED (the row's switch off or its feature absent) before the feature is built. Each scenario is in its owner's partial file
// (FeelReel.N26.<Topic>.cs) as a `static partial void N26<Topic>Body(...)` hook: the lead's switch below calls the hook, and a hook with
// no body answers "NOT WRITTEN (RED)". Every result line starts with the scenario's PRECONDITIONS (FeelReel.Preconditions / SafeSpot).
%s
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseI = { %s };

        static bool IsPhaseI(string s) => Array.IndexOf(PhaseI, s) >= 0;

        /// <summary>The owners' scenario bodies. A hook that no builder has written leaves `res[0]` as it is (NOT WRITTEN).</summary>
%s

        static IEnumerator N26Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
%s
            }
            if (body == null) yield break;
            yield return body;
        }
    }
}
''' % (table, phase, hooks, cases))

# ---- Harness.N26.cs
new('Tools/SimHarness/Harness.N26.cs', '''// NIGHT26 (the shared commit): tonight's headless instruments. Each builder implements its commands in its own file
// (Harness.N26.<Topic>.cs) by filling the declaration-only partial below; a missing body prints "NOT WRITTEN (RED)" and exits 1, so
// the proof-first rule has a RED line to show.
//   n26shared               the lead (this file): the N26 bits round trip, tape format 10 carries the NIGHT26 byte, a format 9 tape
//                           (recorded on NIGHT25's game) reads with every block off and replays EQUAL
//   weapons [--tag t]       builder W (Harness.N26.Weapons.cs, WeaponsCommand): every weapon's LT, costs, the weapon-by-enemy table
//   surfaces [--tag t]      builder M (Harness.N26.Surface.cs, SurfacesCommand): ice / boost strips, friction multiplier, golden exact when absent
//   hulk [--tag t]          builder B (Harness.N26.Boss.cs, HulkCommand): THE SLAG HULK killed by a bot, tells, tape EQUAL
//   glacier [--tag t]       builder B (Harness.N26.Boss.cs, GlacierCommand): THE GLACIER chase: the gap meter, tells, a bot run
//   wyrm26 [--tag t]        builder B (Harness.N26.Boss.cs, Wyrm26Command): proximity aggro, body / head ratio, the final attack
//   level26 <plan> ...      builder L (Harness.N26.Level.cs, Level26Command): one level plan to collision JSON and its report, ranks
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N26(string cmd, string[] args, ref int code)
    {
        if (cmd == "n26shared") N26SharedCommand(args, ref code);
        else if (cmd == "weapons") { code = 1; Console.WriteLine("weapons: NOT WRITTEN (RED)"); WeaponsCommand(args, ref code); }
        else if (cmd == "surfaces") { code = 1; Console.WriteLine("surfaces: NOT WRITTEN (RED)"); SurfacesCommand(args, ref code); }
        else if (cmd == "hulk") { code = 1; Console.WriteLine("hulk: NOT WRITTEN (RED)"); HulkCommand(args, ref code); }
        else if (cmd == "glacier") { code = 1; Console.WriteLine("glacier: NOT WRITTEN (RED)"); GlacierCommand(args, ref code); }
        else if (cmd == "wyrm26") { code = 1; Console.WriteLine("wyrm26: NOT WRITTEN (RED)"); Wyrm26Command(args, ref code); }
        else if (cmd == "level26") { code = 1; Console.WriteLine("level26: NOT WRITTEN (RED)"); Level26Command(args, ref code); }
    }

    // A body replaces the RED line by setting `code` (0 = pass) and printing its own line.
    static partial void WeaponsCommand(string[] args, ref int code);
    static partial void SurfacesCommand(string[] args, ref int code);
    static partial void HulkCommand(string[] args, ref int code);
    static partial void GlacierCommand(string[] args, ref int code);
    static partial void Wyrm26Command(string[] args, ref int code);
    static partial void Level26Command(string[] args, ref int code);

    /// <summary>A format 10 tape's bytes as a format 9 build wrote them: the version 9 and no NIGHT26 byte (the byte after the NIGHT25
    /// byte). A tape that is already format 9 or older passes through.</summary>
    public static byte[] AsFormat9(byte[] b10)
    {
        if (BitConverter.ToInt32(b10, RunTape.Magic.Length) < 10) return b10;
        var r = new TapeReader(b10, RunTape.Magic.Length, b10.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
        r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
        r.Byte(); // n25
        int at = r.Position;
        r.Byte(); // n26
        var b9 = new byte[b10.Length - 1];
        Buffer.BlockCopy(b10, 0, b9, 0, at);
        Buffer.BlockCopy(b10, at + 1, b9, at, b10.Length - at - 1);
        var w = new TapeWriter(9);
        w.U32(9);
        Buffer.BlockCopy(w.data, 0, b9, RunTape.Magic.Length, 4);
        return b9;
    }

    static void N26SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N26Blocks.Count; bits++)
            if (N26Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n26 bits round trip over {1 << N26Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N26Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n26Override = N26Blocks.Off; // as a NIGHT25 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n26shared", null, TapeScript.God);
        byte[] b10 = tape.ToBytes();
        int version = BitConverter.ToInt32(b10, RunTape.Magic.Length);
        bool v10 = version == RunTape.FormatVersion && version == 10 && RunTape.FromBytes(b10).n26 == 0;
        Console.WriteLine($"tape format {version} (want 10), n26 value read back {RunTape.FromBytes(b10).n26}: {(v10 ? "PASS" : "FAIL")}");

        var withBits = new RunTape { n24 = 0b101100101, n25 = 0b101011, n26 = 0b1010011, mode = "campaign" };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n26 == 0b1010011 && back.n25 == 0b101011 && back.n24 == 0b101100101;
        Console.WriteLine($"format 10 carries n24/n25/n26 {back.n24:b}/{back.n25:b}/{back.n26:b}: {(carry ? "PASS" : "FAIL")}");

        byte[] b9 = AsFormat9(b10);
        RunTape old = RunTape.FromBytes(b9);
        TapeResult rep9 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep10 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b10));
        Console.WriteLine($"format 9 tape: n26 {old.n26} (want 0), replay {(rep9.Equal ? "EQUAL" : "DIFFERS")}; format 10 tape replay {(rep10.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v10 && carry && old.n26 == 0 && rep9.Equal && rep10.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n26shared PASS" : "n26shared FAIL");
    }
}
''')

# ---- N26SharedTests (from N25SharedTests with the night numbers moved on)
t = rd('Assets/Tests/EditMode/Game/N25SharedTests.cs')
def bump(m):
    return m.group(0).replace('25', '26').replace('24', '25')
t = t.replace('NIGHT25', '\x01').replace('N25', '\x02').replace('n25', '\x03').replace('Off25', '\x04')
t = t.replace('NIGHT24', 'NIGHT25').replace('N24', 'N25').replace('n24', 'n25').replace('Off24', 'Off25')
t = t.replace('\x01', 'NIGHT26').replace('\x02', 'N26').replace('\x03', 'n26').replace('\x04', 'Off26')
t = t.replace('AsFormat8', 'AsFormat9').replace('TheTape_Format9_CarriesTheN26Byte', 'TheTape_Format10_CarriesTheN26Byte')
t = t.replace('Is.EqualTo(9)', 'Is.EqualTo(10)').replace('< 9) return b9', '< 10) return b9')
t = t.replace('format 9 tape', 'format 10 tape').replace('format 8 tape', 'format 9 tape').replace('Format 9', 'Format 10')
t = t.replace('new RunTape { n25 = 0b101100101, n26 = 0b101011, mode = "campaign" }', 'new RunTape { n24 = 0b101100101, n25 = 0b101011, n26 = 0b1010011, mode = "campaign" }')
t = t.replace('"meleeFire", "pulseCharge", "shockwaveOff", "wyrmAim", "tutorial", "curriculum"', '"pulseLT", "weaponIdentity", "surfaces", "levelSelect", "level1", "level2", "level3"')
t = t.replace('0b101001', '0b1010011')
wr('Assets/Tests/EditMode/Game/N26SharedTests.cs', t.replace('\r\n', '\n').replace('\n', '\r\n'))
print('N26SharedTests generated; check by eye:')
for l in t.splitlines():
    if 'EqualTo(10)' in l or 'RunTape {' in l or 'AsFormat9(' in l or 'n25 ' in l.lower() and 'back' in l:
        print('  ', l.strip()[:150])
wr('Assets/Tests/EditMode/Game/N26SharedTests.cs.meta', rd('Assets/Tests/EditMode/Game/N25SharedTests.cs.meta').replace(
    [l for l in rd('Assets/Tests/EditMode/Game/N25SharedTests.cs.meta').splitlines() if l.startswith('guid:')][0], 'guid: 26a1c0de26a1c0de26a1c0de26a10002'))
for p, g in [(G + 'SurvivalSim.N26.cs', '3'), (G + 'SurvivalGame.N26.cs', '4'), (G + 'Ui.N26.cs', '5'), (G + 'FeelReel.N26.cs', '6')]:
    src = p.replace('N26', 'N25') + '.meta'
    wr(p + '.meta', rd(src).replace([l for l in rd(src).splitlines() if l.startswith('guid:')][0], 'guid: 26a1c0de26a1c0de26a1c0de26a1000' + g))

# ---- call-site patches
edit(G + 'FeelReel.cs', '''                        if (IsPhaseH(s))
''', '''                        if (IsPhaseI(s))
                        {
                            yield return N26Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT26 (FeelReel.N26.cs)
                            yield break;
                        }
                        if (IsPhaseH(s))
''')
edit(G + 'FingerprintBaseline.cs', '        public const bool N25OnPath = false;\n', '''        public const bool N25OnPath = false;

        /// <summary>NIGHT26: false unless the freeze (U3-40.4) records a run with a sim-changing NIGHT26 default on (pulseLT, weaponIdentity);
        /// until then the composed fingerprint runs with every NIGHT26 block off.</summary>
        public const bool N26OnPath = false;
''')
pc = G + 'PlayerChecks.cs'
s = rd(pc)
for n in ['19', '20', '21', '22', '24']:
    s = re.sub(r'(-skygrindOff%s"\) >= 0\) [^;]*?N25Blocks\.ForceOff = )true;' % n, r'\1N26Blocks.ForceOff = true;', s)
s = re.sub(r'(-skygrindOff25"\) >= 0\) N25Blocks\.ForceOff = )true;', r'\1N26Blocks.ForceOff = true;', s)
nl = nlof(s)
s = s.replace('            if (Array.IndexOf(args, "-skygrindOff25") >= 0)',
              '            if (Array.IndexOf(args, "-skygrindOff26") >= 0) N26Blocks.ForceOff = true; // NIGHT26: every NIGHT26 design block off for every run (NIGHT25\'s game, the A side)' + nl +
              '            if (Array.IndexOf(args, "-skygrindOff25") >= 0)', 1)
wr(pc, s)
edit(G + 'SurvivalGame.Init.cs', '            N25InitViews();', '            N26InitViews(); // NIGHT26 (the shared commit): the NIGHT26 world views (SurvivalGame.N26.cs hooks)\n            N25InitViews();')
edit(G + 'SurvivalGame.Render.cs', '                N25RenderViews(worldDt, alpha);', '                N26RenderViews(worldDt, alpha); // NIGHT26 (the shared commit)\n                N25RenderViews(worldDt, alpha);')
edit(G + 'SurvivalSim.cs', '            N25Init();', '            N26Init(); // NIGHT26: the seven NIGHT26 design blocks, read once (SurvivalSim.N26.cs)\n            N25Init();')
run = G + 'SurvivalSim.Run.cs'
edit(run, '            N25StartRun();', '            N26StartRun(); // NIGHT26: the run\'s NIGHT26 blocks (a tape\'s, or off with --off26 and older), the same way (SurvivalSim.N26.cs)\n            N25StartRun();')
edit(run, '            N25CombatStep(step);', '            N26WeaponsStep(step); // NIGHT26: the lead\'s hooks for the NIGHT26 rows (SurvivalSim.N26.cs; nothing without a body)\n            N26SurfaceStep(step);\n            N26BossStep(step);\n            N26LevelStep(step);\n            N25CombatStep(step);')
edit(run, '            N25Blocks? n25Saved = n25Override;', '            N26Blocks? n26Saved = n26Override; // NIGHT26: and every NIGHT26 block off until the freeze (FingerprintBaseline.N26OnPath)\n            if (!FingerprintBaseline.N26OnPath) n26Override = N26Blocks.Off;\n            N25Blocks? n25Saved = n25Override;')
edit(run, '                n25Override = n25Saved;', '                n26Override = n26Saved;\n                n25Override = n25Saved;')
edit(G + 'Ui.cs', '            BuildN25Hud();', '            BuildN26Hud(); // NIGHT26 (the shared commit): the NIGHT26 HUD widgets (Ui.N26.cs hooks)\n            BuildN25Hud();')
edit(G + 'Ui.cs', '            RenderN25Hud(frameDt);', '            RenderN26Hud(frameDt); // NIGHT26 (the shared commit): see BuildN26Hud\n            RenderN25Hud(frameDt);')
ts = G + 'Tape/TapeScript.cs'
edit(ts, '            N25Blocks? n25Saved = sim.n25Override;', '            N26Blocks? n26Saved = sim.n26Override; // NIGHT26: and every NIGHT26 block off until the freeze (N26OnPath)\n            if (!FingerprintBaseline.N26OnPath) sim.n26Override = N26Blocks.Off;\n            N25Blocks? n25Saved = sim.n25Override;')
edit(ts, '                sim.n25Override = n25Saved;', '                sim.n26Override = n26Saved;\n                sim.n25Override = n25Saved;')
edit(G + 'Tape/TapeRecorder.cs', '            tape.n25 = sim.n25.Bits;', '            tape.n26 = sim.n26.Bits; // NIGHT26: the NIGHT26 blocks the run read\n            tape.n25 = sim.n25.Bits;')
edit(G + 'Tape/TapeReplay.cs', '            sim.n25Override = N25Blocks.FromBits(tape.n25);', '            sim.n26Override = N26Blocks.FromBits(tape.n26); // NIGHT26: the NIGHT26 blocks the run read (formats 1-9: all off, NIGHT25\'s game)\n            sim.n25Override = N25Blocks.FromBits(tape.n25);')
rt = G + 'Tape/RunTape.cs'
s = rd(rt)
s = s.replace('public const int FormatVersion = 9;', 'public const int FormatVersion = 10;', 1)
s = s.replace('NIGHT19: 4 adds the NIGHT19 blocks', 'NIGHT26: 10 adds the NIGHT26 blocks (formats 1-9: off); NIGHT19: 4 adds the NIGHT19 blocks', 1)
nl = nlof(s)
s = s.replace('        public int n25;', '        public int n25;' + nl + nl + '        /// <summary>NIGHT26: the NIGHT26 design blocks the run read (N26Blocks.Bits; formats 1-9: 0, every block off, NIGHT25\'s game).</summary>' + nl + '        public int n26;', 1)
s = s.replace('            w.Byte((byte)n25); // format 9: the NIGHT25 blocks', '            w.Byte((byte)n25); // format 9: the NIGHT25 blocks' + nl + '            w.Byte((byte)n26); // format 10: the NIGHT26 blocks', 1)
m = re.search(r'( +)t\.n25 = version >= 9 \? r\.Byte\(\) : 0;[^\r\n]*', s)
s = s[:m.end()] + nl + m.group(1) + 't.n26 = version >= 10 ? r.Byte() : 0; // formats 1-9: recorded on NIGHT25\'s game (or before), so every NIGHT26 block OFF (explicitly)' + s[m.end():]
wr(rt, s)
pg = 'Tools/SimHarness/Program.cs'
edit(pg, '        N25Blocks.ForceOff = off25;\n', '''        N25Blocks.ForceOff = off25;
        // NIGHT26: --off26 runs every command with every NIGHT26 design block off (NIGHT25's game; tonight's A side);
        // --off25 and the older switches turn off these blocks as well
        bool off26 = off25 || Array.IndexOf(args, "--off26") >= 0;
        N26Blocks.ForceOff = off26;
''')
edit(pg, '                if (off25) sim.n25Override = N25Blocks.Off; // NIGHT25: the same\n', '                if (off25) sim.n25Override = N25Blocks.Off; // NIGHT25: the same\n                if (off26) sim.n26Override = N26Blocks.Off; // NIGHT26: the same\n')
edit(pg, '        N25(cmd, args, ref code);', '        N26(cmd, args, ref code); // NIGHT26: n26shared (the lead) and the rows\' commands (Harness.N26.cs)\n        N25(cmd, args, ref code);')
# premises: the format-8 helpers go through the format-9 one first
edit('Tools/SimHarness/Harness.N25.cs', '    public static byte[] AsFormat8(byte[] b9)\n    {\n', '    public static byte[] AsFormat8(byte[] b9)\n    {\n        b9 = AsFormat9(b9); // NIGHT26: the current format is 10; its format 9 form first\n')
edit('Tools/SimHarness/Harness.N25.cs', 'bool v9 = version == 9 &&', 'bool v9 = version == RunTape.FormatVersion &&')
edit('Tools/SimHarness/Harness.N25.cs', 'tape format {version} (want 9)', 'tape format {version} (want {RunTape.FormatVersion}; NIGHT26 premise: the current format is 10)')
edit('Assets/Tests/EditMode/Game/N25SharedTests.cs', '        public static byte[] AsFormat8(byte[] b9)\n        {\n', '        public static byte[] AsFormat8(byte[] b9)\n        {\n            b9 = N26SharedTests.AsFormat9(b9); // NIGHT26: the current format is 10; its format 9 form first\n')
edit('Assets/Tests/EditMode/Game/N25SharedTests.cs', 'Is.EqualTo(9));', 'Is.EqualTo(RunTape.FormatVersion), "NIGHT26 premise: the current format is 10; format 9 keeps these fields");')
print('call sites patched')
