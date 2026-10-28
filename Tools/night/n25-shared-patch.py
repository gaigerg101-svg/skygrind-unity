#!/usr/bin/env python3
"""NIGHT25 shared commit: applies the N25 switches, tape format 9, hooks (copy of NIGHT24's shared commit). Run from the repo root once."""
import io, os, uuid, re, json

def rd(p):
    return io.open(p, encoding='utf-8', newline='').read()

def wr(p, s):
    io.open(p, 'w', encoding='utf-8', newline='').write(s)

def sub(path, old, new, count=1):
    s = rd(path)
    crlf = '\r\n' in s
    o = old.replace('\n', '\r\n') if crlf else old
    n = new.replace('\n', '\r\n') if crlf else new
    assert o in s, (path, old[:70])
    wr(path, s.replace(o, n, count))

def newfile(path, text):
    wr(path, text)
    wr(path + '.meta', 'fileFormatVersion: 2\nguid: ' + uuid.uuid4().hex + '\n')

# ---------------------------------------------------------------- Core/N25Blocks.cs
newfile('Assets/Scripts/Core/N25Blocks.cs', '''// NIGHT25 (the shared commit, the lead): the six NIGHT25 design blocks a run reads, one bit each, so every NIGHT25 feature can be
// switched off (NIGHT25.md "Switches"). With every block off the game is NIGHT24's bit for bit: the A side for every judge
// (`--off25` / `-skygrindOff25`); `--off24`, `--off22`, `--off21`, `--off20` and `--off19` also turn these off. Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n25), a tape records them
// (format 9: one byte after the campaign state; formats 1-8 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.n25.meleeFire (C: the quick melee and the gun's fire in the same frame), pulseCharge (C: the PULSE GAUNTLET's
// left click builds by its own hits and launches), shockwaveOff (C: no shockwave on the shotgun), wyrmAim (W: the worm aims at the
// player and is harder), tutorial (L: layer 1's six teaching stages), curriculum (L: levels 2-10).
namespace Skygrind.Core
{
    public struct N25Blocks
    {
        public bool meleeFire, pulseCharge, shockwaveOff, wyrmAim, tutorial, curriculum;

        /// <summary>The number of blocks (the tape's NIGHT25 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off25`, `-skygrindOff25`; also `--off24` and older): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT24's game.</summary>
        public static N25Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => meleeFire || pulseCharge || shockwaveOff || wyrmAim || tutorial || curriculum;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (meleeFire, pulseCharge,
        /// shockwaveOff change a survival run's combat; the worm, the tutorial and the levels change only their own runs.)</summary>
        public bool SimChanging => meleeFire || pulseCharge || shockwaveOff;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT24's config).</summary>
        public static N25Blocks From(GameConfig cfg) => new N25Blocks
        {
            meleeFire = cfg.Design("n25.meleeFire.enabled"),
            pulseCharge = cfg.Design("n25.pulseCharge.enabled"),
            shockwaveOff = cfg.Design("n25.shockwaveOff.enabled"),
            wyrmAim = cfg.Design("n25.wyrmAim.enabled"),
            tutorial = cfg.Design("n25.tutorial.enabled"),
            curriculum = cfg.Design("n25.curriculum.enabled"),
        };

        /// <summary>Block i (0 meleeFire, 1 pulseCharge, 2 shockwaveOff, 3 wyrmAim, 4 tutorial, 5 curriculum).</summary>
        public bool Flag(int i) => i == 0 ? meleeFire : i == 1 ? pulseCharge : i == 2 ? shockwaveOff : i == 3 ? wyrmAim : i == 4 ? tutorial : i == 5 && curriculum;

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
        public static N25Blocks FromBits(int b) => new N25Blocks
        {
            meleeFire = (b & 1) != 0, pulseCharge = (b & 2) != 0, shockwaveOff = (b & 4) != 0,
            wyrmAim = (b & 8) != 0, tutorial = (b & 16) != 0, curriculum = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "meleeFire", "pulseCharge", "shockwaveOff", "wyrmAim", "tutorial", "curriculum" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT25 block off";
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

# ---------------------------------------------------------------- SurvivalSim.N25.cs
newfile('Assets/Scripts/Game/SurvivalSim.N25.cs', '''// NIGHT25 (the shared commit, the lead owns this file): the six NIGHT25 design blocks in the sim (Core/N25Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n25Override), --off25 / -skygrindOff25 (and --off24 and older)
// take every block off (N25Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (nothing runs without a body); each is called once at the run's start or once per step from the lead's call
// sites (SurvivalSim.Run.cs).
// - C (combat) implements N25CombatStartRun / N25CombatStep (Game/SurvivalSim.Combat25.cs).
// - W (the worm) implements N25WyrmStartRun / N25WyrmStep (Game/SurvivalSim.Wyrm25.cs).
// - L (levels) implements N25LevelStartRun / N25LevelStep (Game/SurvivalSim.Level25.cs).
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT25 blocks, read once.</summary>
        N25Blocks n25Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N25Blocks? n25Override;

        /// <summary>The NIGHT25 blocks this run read (what a tape records).</summary>
        public N25Blocks n25;

        void N25Init() => n25Design = N25Blocks.From(cfg);

        void N25StartRun()
        {
            n25 = n25Override ?? (N25Blocks.ForceOff ? N25Blocks.Off : n25Design);
            N25CombatStartRun();
            N25WyrmStartRun();
            N25LevelStartRun();
        }

        partial void N25CombatStartRun();
        partial void N25CombatStep(double dt);
        partial void N25WyrmStartRun();
        partial void N25WyrmStep(double dt);
        partial void N25LevelStartRun();
        partial void N25LevelStep(double dt);
    }
}
''')

# ---------------------------------------------------------------- SurvivalGame.N25.cs / Ui.N25.cs (plain methods + declaration-only hooks)
newfile('Assets/Scripts/Game/SurvivalGame.N25.cs', '''// NIGHT25 (the shared commit, the lead owns this file): the hooks the NIGHT25 world views hang on. A builder implements the
// declaration-only partial in its own file and names the one call in its report. Nothing runs without a body.
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>Once at game init, after the NIGHT24 views.</summary>
        void N25InitViews() => N25InitViewsHook();

        /// <summary>Every render frame beside the other world views (worldDt: the slowed world frame time; alpha: the sim's render interpolation).</summary>
        void N25RenderViews(double worldDt, double alpha) => N25RenderViewsHook(worldDt, alpha);

        partial void N25InitViewsHook();
        partial void N25RenderViewsHook(double worldDt, double alpha);
    }
}
''')
newfile('Assets/Scripts/Game/Ui.N25.cs', '''// NIGHT25 (the shared commit, the lead owns this file): the HUD hooks for NIGHT25's widgets (the boss bar, the lesson prompts).
// A builder implements the declaration-only partials in its own file (Ui.Lesson25.cs, ...). Wiring: Ui.BuildHud calls BuildN25Hud()
// after BuildN24Hud(); Ui.Render calls RenderN25Hud(frameDt) after RenderN24Hud(frameDt).
namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        void BuildN25Hud() => BuildN25HudHook();
        void RenderN25Hud(double frameDt) => RenderN25HudHook(frameDt);

        partial void BuildN25HudHook();
        partial void RenderN25HudHook(double frameDt);
    }
}
''')

# ---------------------------------------------------------------- call sites
sub('Assets/Scripts/Game/SurvivalSim.cs', '            N24Init();', '            N25Init(); // NIGHT25: the six NIGHT25 design blocks, read once (SurvivalSim.N25.cs)\n            N24Init();')
sub('Assets/Scripts/Game/SurvivalSim.Run.cs', '            N24StartRun();', '            N25StartRun(); // NIGHT25: the run\'s NIGHT25 blocks (a tape\'s, or off with --off25 and older), the same way (SurvivalSim.N25.cs)\n            N24StartRun();')
sub('Assets/Scripts/Game/SurvivalSim.Run.cs', '            N24WyrmStep(step);\n', '            N24WyrmStep(step);\n            N25CombatStep(step); // NIGHT25: the lead\'s hooks for the NIGHT25 rows (SurvivalSim.N25.cs; nothing without a body)\n            N25WyrmStep(step);\n            N25LevelStep(step);\n')
sub('Assets/Scripts/Game/SurvivalSim.Run.cs', '            N24Blocks? n24Saved = n24Override;', '            N25Blocks? n25Saved = n25Override; // NIGHT25: and every NIGHT25 block off until the freeze (FingerprintBaseline.N25OnPath)\n            if (!FingerprintBaseline.N25OnPath) n25Override = N25Blocks.Off;\n            N24Blocks? n24Saved = n24Override;')
sub('Assets/Scripts/Game/SurvivalSim.Run.cs', '                n24Override = n24Saved;\n', '                n24Override = n24Saved;\n                n25Override = n25Saved;\n')
sub('Assets/Scripts/Game/Ui.cs', '            BuildN24Hud();', '            BuildN25Hud(); // NIGHT25 (the shared commit): the NIGHT25 HUD widgets (Ui.N25.cs hooks)\n            BuildN24Hud();')
sub('Assets/Scripts/Game/Ui.cs', '            RenderN24Hud(frameDt);', '            RenderN25Hud(frameDt); // NIGHT25 (the shared commit): see BuildN25Hud\n            RenderN24Hud(frameDt);')
sub('Assets/Scripts/Game/SurvivalGame.Init.cs', '            N24InitViews();', '            N25InitViews(); // NIGHT25 (the shared commit): the NIGHT25 world views (SurvivalGame.N25.cs hooks)\n            N24InitViews();')
sub('Assets/Scripts/Game/SurvivalGame.Render.cs', '                N24RenderViews(worldDt, alpha);', '                N25RenderViews(worldDt, alpha); // NIGHT25 (the shared commit)\n                N24RenderViews(worldDt, alpha);')
sub('Assets/Scripts/Game/FingerprintBaseline.cs', '        public const bool N24OnPath = false;', '        public const bool N24OnPath = false;\n\n        /// <summary>NIGHT25: false unless the freeze (U3-39.7) records a run with a sim-changing NIGHT25 default on (meleeFire, pulseCharge,\n        /// shockwaveOff); until then the composed fingerprint runs with every NIGHT25 block off.</summary>\n        public const bool N25OnPath = false;')
# tape
sub('Assets/Scripts/Game/Tape/RunTape.cs', 'public const int FormatVersion = 8;', 'public const int FormatVersion = 9;')
sub('Assets/Scripts/Game/Tape/RunTape.cs', '        public Campaign24State campaign = Campaign24State.None;\n', '        public Campaign24State campaign = Campaign24State.None;\n        /// <summary>NIGHT25: the NIGHT25 design blocks the run read (N25Blocks.Bits; formats 1-8: 0, every block off, NIGHT24\'s game).</summary>\n        public int n25;\n')
sub('Assets/Scripts/Game/Tape/RunTape.cs', '            w.VarI(campaign.limits);\n', '            w.VarI(campaign.limits);\n            w.Byte((byte)n25); // format 9: the NIGHT25 blocks\n')
sub('Assets/Scripts/Game/Tape/RunTape.cs', 'limits = r.VarI() };\n            }\n', 'limits = r.VarI() };\n            }\n            t.n25 = version >= 9 ? r.Byte() : 0; // formats 1-8: recorded on NIGHT24\'s game (or before), so every NIGHT25 block OFF (explicitly)\n')
sub('Assets/Scripts/Game/Tape/TapeRecorder.cs', '            tape.n24 = sim.n24.Bits;', '            tape.n25 = sim.n25.Bits; // NIGHT25: the NIGHT25 blocks the run read\n            tape.n24 = sim.n24.Bits;')
sub('Assets/Scripts/Game/Tape/TapeReplay.cs', '            sim.n24Override = N24Blocks.FromBits(tape.n24);', '            sim.n25Override = N25Blocks.FromBits(tape.n25); // NIGHT25: the NIGHT25 blocks the run read (formats 1-8: all off, NIGHT24\'s game)\n            sim.n24Override = N24Blocks.FromBits(tape.n24);')
sub('Assets/Scripts/Game/Tape/TapeScript.cs', '            N24Blocks? n24Saved = sim.n24Override;', '            N25Blocks? n25Saved = sim.n25Override; // NIGHT25: and every NIGHT25 block off until the freeze (N25OnPath)\n            if (!FingerprintBaseline.N25OnPath) sim.n25Override = N25Blocks.Off;\n            N24Blocks? n24Saved = sim.n24Override;')
sub('Assets/Scripts/Game/Tape/TapeScript.cs', '                sim.n24Override = n24Saved;\n', '                sim.n24Override = n24Saved;\n                sim.n25Override = n25Saved;\n')
# PlayerChecks
pc = 'Assets/Scripts/Game/PlayerChecks.cs'
s = rd(pc)
s = s.replace('N24Blocks.ForceOff = true; // NIGHT19', 'N24Blocks.ForceOff = N25Blocks.ForceOff = true; // NIGHT19')
s = s.replace('N24Blocks.ForceOff = true; // NIGHT20', 'N24Blocks.ForceOff = N25Blocks.ForceOff = true; // NIGHT20')
s = s.replace('N24Blocks.ForceOff = true; // NIGHT21', 'N24Blocks.ForceOff = N25Blocks.ForceOff = true; // NIGHT21')
s = s.replace('N24Blocks.ForceOff = true; // NIGHT22', 'N24Blocks.ForceOff = N25Blocks.ForceOff = true; // NIGHT22')
s = s.replace('if (Array.IndexOf(args, "-skygrindOff24") >= 0) N24Blocks.ForceOff = true; // NIGHT24', 'if (Array.IndexOf(args, "-skygrindOff24") >= 0) N24Blocks.ForceOff = N25Blocks.ForceOff = true; // NIGHT24')
nl = '\r\n' if '\r\n' in s else '\n'
anchor = 'N25Blocks.ForceOff = true; // NIGHT24'
i = s.index('-skygrindOff24')
j = s.index(nl, i)
s = s[:j + len(nl)] + '            if (Array.IndexOf(args, "-skygrindOff25") >= 0) N25Blocks.ForceOff = true; // NIGHT25: every NIGHT25 design block off for every run (NIGHT24\'s game, the A side)' + nl + s[j + len(nl):]
wr(pc, s)
# SimHarness Program.cs
sub('Tools/SimHarness/Program.cs', '        N24Blocks.ForceOff = off24;\n', '        N24Blocks.ForceOff = off24;\n        // NIGHT25: --off25 runs every command with every NIGHT25 design block off (NIGHT24\'s game; tonight\'s A side);\n        // --off24 and the older switches turn off these blocks as well\n        bool off25 = off24 || Array.IndexOf(args, "--off25") >= 0;\n        N25Blocks.ForceOff = off25;\n')
sub('Tools/SimHarness/Program.cs', '                if (off24) sim.n24Override = N24Blocks.Off; // NIGHT24: the same\n', '                if (off24) sim.n24Override = N24Blocks.Off; // NIGHT24: the same\n                if (off25) sim.n25Override = N25Blocks.Off; // NIGHT25: the same\n')
sub('Tools/SimHarness/Program.cs', '        N24(cmd, args, ref code);', '        N25(cmd, args, ref code); // NIGHT25: n25shared (the lead) and the rows\' commands (Harness.N25.cs)\n        N24(cmd, args, ref code);')

# AsFormat7 now goes through AsFormat8 first (format 9 tapes); Harness.N24 and N24SharedTests
sub('Tools/SimHarness/Harness.N24.cs', '        if (BitConverter.ToInt32(b8, RunTape.Magic.Length) < 8) return b8;\n', '        b8 = AsFormat8(b8); // NIGHT25: the current format is 9; its format 8 form first\n        if (BitConverter.ToInt32(b8, RunTape.Magic.Length) < 8) return b8;\n')
sub('Assets/Tests/EditMode/Game/N24SharedTests.cs', '            if (BitConverter.ToInt32(b8, RunTape.Magic.Length) < 8) return b8;\n', '            b8 = N25SharedTests.AsFormat8(b8); // NIGHT25: the current format is 9; its format 8 form first\n            if (BitConverter.ToInt32(b8, RunTape.Magic.Length) < 8) return b8;\n')
# N24SharedTests asserts version 8 on a freshly written tape: it is 9 now
sub('Assets/Tests/EditMode/Game/N24SharedTests.cs', '            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(8));\n            RunTape back = RunTape.FromBytes(b);', '            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion), "NIGHT25 premise: the current format is 9; format 8 keeps these fields");\n            RunTape back = RunTape.FromBytes(b);')

# ---------------------------------------------------------------- Harness.N25.cs
newfile('Tools/SimHarness/Harness.N25.cs', '''// NIGHT25 (the shared commit): tonight's headless instruments. Each builder implements its commands in its own file
// (Harness.N25.<Topic>.cs) by filling the declaration-only partial below; a missing body prints "NOT WRITTEN (RED)" and exits 1, so
// the proof-first rule has a RED line to show.
//   n25shared               the lead (this file): the N25 bits round trip, tape format 9 carries the NIGHT25 byte, a format 8 tape
//                           (recorded on NIGHT24's game) reads with every block off and replays EQUAL
//   meleefire [--tag t]     builder C (Harness.N25.Combat.cs, MeleeFireCommand): shots and punches held together in 3 s, before / after
//   pulse [--tag t]         builder C (Harness.N25.Combat.cs, PulseCommand): the PULSE builds by its own hits, launches, small damage
//   wyrm25 [--tag t]        builder W (Harness.N25.Wyrm.cs, Wyrm25Command): aim / hits per minute / bar / death, before and after
//   tutorial [--tag t]      builder L (Harness.N25.Level.cs, TutorialCommand): the six stages by bot, no soft-lock
//   curriculum [--tag t]    builder L (Harness.N25.Level.cs, CurriculumCommand): levels 2-10, validate report, clear rates
//   level25 <plan> ...      builder L (Harness.N25.Level.cs, Level25Command): one level plan to collision JSON and its report
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N25(string cmd, string[] args, ref int code)
    {
        if (cmd == "n25shared") N25SharedCommand(args, ref code);
        else if (cmd == "meleefire") { code = 1; Console.WriteLine("meleefire: NOT WRITTEN (RED)"); MeleeFireCommand(args, ref code); }
        else if (cmd == "pulse") { code = 1; Console.WriteLine("pulse: NOT WRITTEN (RED)"); PulseCommand(args, ref code); }
        else if (cmd == "wyrm25") { code = 1; Console.WriteLine("wyrm25: NOT WRITTEN (RED)"); Wyrm25Command(args, ref code); }
        else if (cmd == "tutorial") { code = 1; Console.WriteLine("tutorial: NOT WRITTEN (RED)"); TutorialCommand(args, ref code); }
        else if (cmd == "curriculum") { code = 1; Console.WriteLine("curriculum: NOT WRITTEN (RED)"); CurriculumCommand(args, ref code); }
        else if (cmd == "level25") { code = 1; Console.WriteLine("level25: NOT WRITTEN (RED)"); Level25Command(args, ref code); }
    }

    // A body replaces the RED line by setting `code` (0 = pass) and printing its own line.
    static partial void MeleeFireCommand(string[] args, ref int code);
    static partial void PulseCommand(string[] args, ref int code);
    static partial void Wyrm25Command(string[] args, ref int code);
    static partial void TutorialCommand(string[] args, ref int code);
    static partial void CurriculumCommand(string[] args, ref int code);
    static partial void Level25Command(string[] args, ref int code);

    /// <summary>A format 9 tape's bytes as a format 8 build wrote them: the version 8 and no NIGHT25 byte (the byte after the campaign
    /// state). A tape that is already format 8 or older passes through.</summary>
    public static byte[] AsFormat8(byte[] b9)
    {
        if (BitConverter.ToInt32(b9, RunTape.Magic.Length) < 9) return b9;
        var r = new TapeReader(b9, RunTape.Magic.Length, b9.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
        r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
        int at = r.Position;
        r.Byte(); // n25
        var b8 = new byte[b9.Length - 1];
        Buffer.BlockCopy(b9, 0, b8, 0, at);
        Buffer.BlockCopy(b9, at + 1, b8, at, b9.Length - at - 1);
        var w = new TapeWriter(8);
        w.U32(8);
        Buffer.BlockCopy(w.data, 0, b8, RunTape.Magic.Length, 4);
        return b8;
    }

    static void N25SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N25Blocks.Count; bits++)
            if (N25Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n25 bits round trip over {1 << N25Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N25Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n25Override = N25Blocks.Off; // as a NIGHT24 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n25shared", null, TapeScript.God);
        byte[] b9 = tape.ToBytes();
        int version = BitConverter.ToInt32(b9, RunTape.Magic.Length);
        bool v9 = version == 9 && RunTape.FromBytes(b9).n25 == 0;
        Console.WriteLine($"tape format {version} (want 9), n25 value read back {RunTape.FromBytes(b9).n25}: {(v9 ? "PASS" : "FAIL")}");

        var withBits = new RunTape { n24 = 0b101100101, n25 = 0b101011, mode = "campaign" };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n25 == 0b101011 && back.n24 == 0b101100101;
        Console.WriteLine($"format 9 carries n24/n25 {back.n24:b}/{back.n25:b}: {(carry ? "PASS" : "FAIL")}");

        byte[] b8 = AsFormat8(b9);
        RunTape old = RunTape.FromBytes(b8);
        TapeResult rep8 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep9 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b9));
        Console.WriteLine($"format 8 tape: n25 {old.n25} (want 0), replay {(rep8.Equal ? "EQUAL" : "DIFFERS")}; format 9 tape replay {(rep9.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v9 && carry && old.n25 == 0 && rep8.Equal && rep9.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n25shared PASS" : "n25shared FAIL");
    }
}
''')

# ---------------------------------------------------------------- FeelReel.N25.cs
ids = [('meleefire', 'C', 'shots and punches in the same 3 s'), ('shockwave', 'C', 'no shockwave on the shotgun'),
       ('pulse', 'C', 'the PULSE builds by use, launches, small damage'), ('wyrmvoid', 'W', 'no boot path shows the worm over a void'),
       ('wyrmbar', 'W', 'the worm has a boss bar'), ('wyrmdies', 'W', 'the worm dies: bossDown and the finisher'),
       ('wyrmaim', 'W', 'the worm aims at the player: hits per minute'), ('tutorial', 'L', 'the six tutorial stages'),
       ('curriculum', 'L', 'levels 2-10'), ('lesson', 'F', 'the lesson prompts'), ('hitch25', 'F', 'the first PLAY hitch')]
hooks = ''.join('        static partial void N25%sBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);\n' % i.title().replace('Hitch25', 'Hitch') for i, _, _ in ids)
cases = ''.join('                case "%s": N25%sBody(g, dir, res, offs, pre, ref body); break;\n' % (i, i.title().replace('Hitch25', 'Hitch')) for i, _, _ in ids)
table = ''.join('//   %-11s %s   %s\n' % (i, o, d) for i, o, d in ids)
phase = ', '.join('"%s"' % i for i, _, _ in ids)
newfile('Assets/Scripts/Game/FeelReel.N25.cs', '''// NIGHT25 (the lead, phase H): the reel scenarios for NIGHT25's rows, on the Player, proof-first (NIGHT25.md): a scenario is written and
// seen RED (the row's switch off or its feature absent) before the feature is built. Each scenario is in its owner's partial file
// (FeelReel.N25.<Topic>.cs) as a `static partial void N25<Topic>Body(...)` hook: the lead's switch below calls the hook, and a hook with
// no body answers "NOT WRITTEN (RED)". Every result line starts with the scenario's PRECONDITIONS (FeelReel.Preconditions / SafeSpot).
''' + table + '''using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseH = { ''' + phase + ''' };

        static bool IsPhaseH(string s) => Array.IndexOf(PhaseH, s) >= 0;

        /// <summary>The owners' scenario bodies. A hook that no builder has written leaves `res[0]` as it is (NOT WRITTEN).</summary>
''' + hooks + '''
        static IEnumerator N25Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
''' + cases + '''            }
            if (body == null) yield break;
            yield return body;
        }
    }
}
''')
sub('Assets/Scripts/Game/FeelReel.cs', '                        if (IsPhaseG(s))\n', '                        if (IsPhaseH(s))\n                        {\n                            yield return N25Scenario(g, s, dir, res, offs, yaw, pre); // NIGHT25 (FeelReel.N25.cs)\n                            yield break;\n                        }\n                        if (IsPhaseG(s))\n')

# ---------------------------------------------------------------- N25SharedTests
newfile('Assets/Tests/EditMode/Game/N25SharedTests.cs', '''// NIGHT25 (the shared commit, the lead): the contract every builder compiles against.
//   1. the six NIGHT25 blocks: the bits round trip, every block off is NIGHT24's game, each block present in the design config
//      (design.n25.<row>) with an `enabled` and a `_why`, all off in the shared commit; the Player's copy stays byte-identical
//   2. --off25 / -skygrindOff25 (N25Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 9 carries the NIGHT25 byte; a format 8 tape is read with every block off and replays EQUAL
//   4. the composed fingerprint runs with every NIGHT25 block off unless the freeze sets FingerprintBaseline.N25OnPath
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N25SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Rows = { "meleeFire", "pulseCharge", "shockwaveOff", "wyrmAim", "tutorial", "curriculum" };

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N25Blocks_Bits_RoundTrip_AndOffIsNight24()
        {
            for (int bits = 0; bits < 1 << N25Blocks.Count; bits++)
                Assert.That(N25Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N25Blocks.Off.Any, Is.False);
        }

        [Test]
        public void EveryN25Block_IsInTheDesignConfig_WithItsWhy()
        {
            GameConfig cfg = Cfg();
            string design = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "unity-config.json"));
            GameConfig full = GameConfig.Load(Path.Combine(Root, "config.json"), design);
            Assert.That(full.Has("design.n25._why"), Is.True, "n25._why");
            foreach (string b in Rows)
            {
                Assert.That(full.Has("design.n25." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(full.Has("design.n25." + b + "._why"), Is.True, b + "._why");
            }
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N25Blocks.ForceOff;
            try
            {
                N25Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n25.Any, Is.False);
            }
            finally { N25Blocks.ForceOff = was; }
            sim.n25Override = N25Blocks.FromBits(0b101001);
            sim.StartRun(1, null, null);
            Assert.That(sim.n25.Bits, Is.EqualTo(0b101001), "an override (a tape's) wins");
        }

        [Test]
        public void TheTape_Format9_CarriesTheN25Byte()
        {
            var t = new RunTape { n24 = 0b101100101, n25 = 0b101011, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(9));
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n25, Is.EqualTo(0b101011));
            Assert.That(back.n24, Is.EqualTo(0b101100101));
            RunTape old = RunTape.FromBytes(AsFormat8(b));
            Assert.That(old.n25, Is.EqualTo(0), "format 8: every NIGHT25 block off");
            Assert.That(old.n24, Is.EqualTo(0b101100101), "format 8 keeps its NIGHT24 value");
        }

        [Test]
        public void AFormat8Tape_RecordedOnNight24sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n25Override = N25Blocks.Off; // as a NIGHT24 build recorded it
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat8(tape.ToBytes()));
            Assert.That(old.n25, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_N25IsOffThePath_AndTheOffPathIsNight24()
        {
            Assert.That(FingerprintBaseline.N25OnPath, Is.False, "the freeze sets it only if a sim-changing NIGHT25 default is on");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n25Override = N25Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n25Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 9 tape's bytes as a format 8 build wrote them: the version 8, no NIGHT25 byte (after the campaign state).
        /// A tape that is already format 8 or older passes through.</summary>
        public static byte[] AsFormat8(byte[] b9)
        {
            if (BitConverter.ToInt32(b9, RunTape.Magic.Length) < 9) return b9;
            var r = new TapeReader(b9, RunTape.Magic.Length, b9.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
            int at = r.Position;
            r.Byte(); // n25
            var b8 = new byte[b9.Length - 1];
            Buffer.BlockCopy(b9, 0, b8, 0, at);
            Buffer.BlockCopy(b9, at + 1, b8, at, b9.Length - at - 1);
            var w = new TapeWriter(8);
            w.U32(8);
            Buffer.BlockCopy(w.data, 0, b8, RunTape.Magic.Length, 4);
            return b8;
        }
    }
}
''')

# ---------------------------------------------------------------- config rows (both copies are written by n24-sync.sh afterwards)
rows = [
 ('meleeFire', "The quick melee and the gun's fire in the same frame (builder C, GG 2026-10-08: 'if you're shooting a weapon and you're trying to melee you can't do both at the same time it's either one action or the other'): the web serialises them (punch.js / weapons.js); with the row on a punch neither waits for nor cancels the gun's cycle, and the reverse. Off (--off25): NIGHT24's serialised rules."),
 ('pulseCharge', "The PULSE GAUNTLET's left click builds power by its own hits and launches the player with a little damage (builder C, GG: 'the pulse is the weapon that should have the actual pulse for the left click that's built based off of how many times you use it'). Numbers (hits per tier, the launch distances, the damage) are added by builder C under this row, each with a _why. Off (--off25): NIGHT24's fist-charge rules."),
 ('shockwaveOff', "No shockwave on the shotgun (builder C, GG: 'remove that shockwave effect from the shotgun should not be like that'): the THUNDERCLAP's LT shove (Core/Guns/Shockwave.cs, NIGHT19) is removed; its LT zooms (GG: most weapons zoom). Off (--off25): NIGHT24's shotgun with the shockwave."),
 ('wyrmAim', "The worm aims at the player and fights a little harder (builder W, GG: 'make it a little more difficult and make sure it's like obviously targeted towards you'): spit leads the target, the lunge locks at its tell and re-aims once, 10-15 % less breathing room per phase, phase 3 overlaps compatible attacks; every tell stays >= 0.5 s. Numbers are added by builder W under this row, each with a _why. Off (--off25): NIGHT24's worm."),
 ('tutorial', "Layer 1 as a six-stage TUTORIAL (builder L, GG: 'it should really be a tutorial for this first map and don't be afraid to break it up into stages'): look and walk, jump and slide, wall and air, the pistol, the fist, a first fight; each stage is Exposition / Validation / Challenge (RESEARCH.md S2), a respawn at the stage's start, no fail state. Off (--off25): NIGHT24's layer 1."),
 ('curriculum', "Levels 2-10 as plan files that each teach one new thing (builder L, GG: 'I'd like this to have 10 levels and they should all build up to each other'): the table is Design/levels/CURRICULUM.md. Off (--off25): only NIGHT24's layer 1."),
]
block = ',\n      "n25": {\n        "_why": "NIGHT25 U3-39 (GG 2026-10-08: the teaching campaign, melee while firing, the PULSE, no shotgun shockwave, the worm aimed at the player): the six NIGHT25 rows, one block each (design.n25.<row>.enabled), all off in the shared commit; a row is turned on only when its contract passes. Off (--off25 / -skygrindOff25): NIGHT24\'s game bit for bit. Unity-only numbers live in each row with a _why.",\n'
import json
parts = []
for name, why in rows:
    parts.append('      "%s": {\n        "_why": %s,\n        "enabled": false\n      }' % (name, json.dumps(why)))
block += ',\n'.join(parts) + '\n      }\n    }'
p = 'Design/unity-config.json'
s = rd(p)
nl = '\r\n' if '\r\n' in s else '\n'
tail_old = '        "shaderWarmup": true\n      }\n    }\n  }\n}'
if nl == '\r\n':
    tail_old = tail_old.replace('\n', nl)
assert tail_old in s
# the n24 block ends with "      }\n    }" before "  }\n}": append n25 after the n24 block's closing brace
new_tail = '        "shaderWarmup": true\n      }\n    }' + block.replace('\n', nl if nl == '\r\n' else '\n') .replace(',\n      "n25"', ',\n      "n25"') + '\n  }\n}'
if nl == '\r\n':
    new_tail = new_tail.replace('\n', nl).replace('\r\r', '\r')
s = s.replace(tail_old, new_tail)
wr(p, s)

# ---------------------------------------------------------------- NIGHT25-FEATURES.json
feats = [
 ('bug-void', 'W', 'design.n25.wyrmAim', 'wyrmvoid', "no Player boot path (TITLE > PLAY in SURVIVAL with the worm on; MODE > CAMPAIGN; -skygrindMap layer1) shows the worm over an empty sky: the arena has floor, walls and light on every path and no SURVIVAL banner ('ROUND 1 . 0 LEFT') is up during a campaign boss; one Player line per path prints what was on screen (floor present, banner text)"),
 ('boss-bar', 'W', 'design.n25.wyrmAim', 'wyrmbar', "the worm's HUD bar shows its name, HP fraction and phase pips from the intro to the death, text 18 px or more, no pop-in (M9 0), through BossKit.BossBar and HudReads20; the result line prints the bar's fraction at 100 %, 66 %, 33 % and 0 % of the pool"),
 ('boss-dies', 'W', 'design.n25.wyrmAim', 'wyrmdies', "on a Player the worm dies: the HP pool reaches 0, the finisher plays, bossDown fires once, and the run moves on (the next door opens or RESULTS); proven from the reel AND a human-like composed-sim run; the result line prints the time to death"),
 ('boss-aimed', 'W', 'design.n25.wyrmAim', 'wyrmaim', "spit leads the player's velocity, the lunge locks at its tell and re-aims once, the safe window shrinks 10-15 % per phase, phase 3 overlaps two compatible attacks; every tell >= 0.5 s and no two lethal attacks resolve inside 0.5 s (the harness reads the table); player hits per minute printed before and after; the composed-sim human bot kills it in 3:30-4:30; attack count unchanged (47 told)"),
 ('melee-fire', 'C', 'design.n25.meleeFire', 'meleefire', "with the row on a held fire button and the quick melee in the same frame both resolve: a harness holds both for 3 s and prints shots and punches (before: one blocks the other; after: both counts rise by their own cooldowns, each within 10 % of the alone rate); --off25 restores the serialised rules; golden exact"),
 ('shockwave-off', 'C', 'design.n25.shockwaveOff', 'shockwave', "the THUNDERCLAP has no shockwave / LT shove (its LT zooms); a grep of Core/Guns and config finds no shotgun shockwave row in either config copy; a Player line shows LT on the shotgun zooms and no shockwave event fires; --off25 restores it; the tests that asserted the shockwave are changed and listed"),
 ('pulse-charge', 'C', 'design.n25.pulseCharge', 'pulse', "the PULSE GAUNTLET's left click builds charge by its own hits (about 6-8 hits per tier, printed), never by time (idle 60 s = +0) and no longer by other guns' kills; a charged release launches along the aim 0.5 / 1.0 / 2.25 of a jump distance (measured on the Mover, printed) with a little damage (printed); 3 s lockout, one air flick, clamped on the collider; the long-cooldown rule (CORE.md) is met by the charge cost; the reel prints before and after"),
 ('tutorial', 'L', 'design.n25.tutorial', 'tutorial', "layer 1 is six stages (look and walk; jump and slide; wall and air; the pistol; the fist; a first fight), each Exposition / Validation / Challenge with a lesson-clear condition that opens its door, a respawn at the stage's start, no enemy in stages 1-3, ONE enemy kind (husk) in the first fight; a bot clears all six, the validate report proves no soft-lock (every pit has a recovery path), prompts 18 px or more with the pad glyph on a pad; a Player line per stage"),
 ('curriculum', 'L', 'design.n25.curriculum', 'curriculum', "Design/levels/CURRICULUM.md lists levels 2-10 (the new thing, the world's lesson, LIMIT flags, enemies, par, pass check); each level is one plan file + a config row + a validate report with 0 errors (every tier reachable by enemies); the bot's per-room clear rate is 60-90 % (first and final numbers printed); each level's first room is a breath; levels 2-5 pass on a Player, 6-10 headless at minimum; the number of levels that play door to door is printed"),
 ('first-play-hitch', 'F', 'none (a fix)', 'hitch25', "on a Development Player on a quiet machine the first PLAY after boot costs under 100 ms at its worst frame (NIGHT24: 346.1 ms), or the dominant cause is named with its before and after and the row is SHORT; Hitch24's markers name the cause"),
 ('m9-popins', 'F', 'none (fixes)', 'lesson', "Tools/night/m9-list.py on the final reel lists the pop-ins before and after; the Buy prompt, Down menu, Markers, vignettes and prompt wheel get entrances of 0.12 s or less; the lesson prompts (UiLesson25) enter and leave without a pop-in; M9 0 on every NEW element"),
 ('fingerprint', 'lead', 'FingerprintBaseline.N25OnPath', 'none', "at the freeze: if meleeFire, pulseCharge or shockwaveOff is on by default the composed fingerprint is re-recorded ONCE in its own commit with the reason (N25OnPath true); --off25 EQUAL to the NIGHT24 baseline; --off24 / --off22 / --off21 / --off19 EQUAL to theirs"),
]
fe = {"schema": "night25-features-1", "about": "The 12 contracts of NIGHT25 (the master prompt). The lead sets passes / evidence only from this night's printed lines. owner = the builder queue in NIGHT25.md (C combat, W the worm, L levels and the campaign, F feel / HUD / prompts, lead = the lead). A number in a check is a threshold the run must print; where a number is our inference (RESEARCH.md), the run tunes the config to meet it and prints both the first and the final value.", "features": [
    {"id": i, "owner": o, "switch": sw, "scenario": sc, "check": ck, "passes": False, "evidence": None} for i, o, sw, sc, ck in feats]}
io.open('NIGHT25-FEATURES.json', 'w', encoding='utf-8', newline='').write(json.dumps(fe, indent=2, ensure_ascii=False) + '\n')
print('n25 shared patch applied')
