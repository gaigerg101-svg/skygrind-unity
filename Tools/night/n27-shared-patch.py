#!/usr/bin/env python3
"""NIGHT27 shared commit generator (the lead). Run once from the repo root: python Tools/night/n27-shared-patch.py
Creates Core/N27Blocks.cs and Game/SurvivalSim.N27.cs, patches the tape (format 11), the sim's fingerprint run, the Player's and
the harness's --off27 switch, FingerprintBaseline.N27OnPath, and adds design.n27 (all rows enabled false) to Design/unity-config.json."""
import io, json

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

new('Assets/Scripts/Core/N27Blocks.cs', '''// NIGHT27 (the shared commit, the lead): the six sim-changing NIGHT27 design blocks a run reads, one bit each, so every sim-changing
// NIGHT27 feature can be switched off (NIGHT27.md "Switches"). With every block off the game is NIGHT26's bit for bit
// (`--off27` / `-skygrindOff27`; `--off26` and older also turn these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.n27), a tape records them (format 11: one byte after the NIGHT26 byte;
// formats 1-10 replay with every block off). The view rows (slowAim27, skate27, roof27) and the level data rows (tutorial27)
// are read directly from design.n27.<row>.enabled and need no bit.
// The blocks: arenas27 (the tight rooms, the cost budget, wall lanes as level data and a validator rule), hazardFloor27 (a floor strip
// that hurts for a limited time), pulse27 (the 2.5 s heavy swing, five kill pips, the LT blast gate), chargeShots27 (RT hold on the
// pistol and the shotgun), boss1 (THE LANTERN), boss2 (THE GLACIER, pulse-only chase).
namespace Skygrind.Core
{
    public struct N27Blocks
    {
        public bool arenas27, hazardFloor27, pulse27, chargeShots27, boss1, boss2;

        /// <summary>The number of blocks (the tape's NIGHT27 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off27`, `-skygrindOff27`; also `--off26` and older): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT26's game.</summary>
        public static N27Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => arenas27 || hazardFloor27 || pulse27 || chargeShots27 || boss1 || boss2;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (pulse27 and chargeShots27
        /// change a survival run's combat; the others act only in campaign levels.)</summary>
        public bool SimChanging => pulse27 || chargeShots27;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT26's config).</summary>
        public static N27Blocks From(GameConfig cfg) => new N27Blocks
        {
            arenas27 = cfg.Design("n27.arenas27.enabled"),
            hazardFloor27 = cfg.Design("n27.hazardFloor27.enabled"),
            pulse27 = cfg.Design("n27.pulse27.enabled"),
            chargeShots27 = cfg.Design("n27.chargeShots27.enabled"),
            boss1 = cfg.Design("n27.boss1.enabled"),
            boss2 = cfg.Design("n27.boss2.enabled"),
        };

        /// <summary>Block i (0 arenas27, 1 hazardFloor27, 2 pulse27, 3 chargeShots27, 4 boss1, 5 boss2).</summary>
        public bool Flag(int i) => i == 0 ? arenas27 : i == 1 ? hazardFloor27 : i == 2 ? pulse27 : i == 3 ? chargeShots27 : i == 4 ? boss1 : i == 5 && boss2;

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
        public static N27Blocks FromBits(int b) => new N27Blocks
        {
            arenas27 = (b & 1) != 0, hazardFloor27 = (b & 2) != 0, pulse27 = (b & 4) != 0, chargeShots27 = (b & 8) != 0,
            boss1 = (b & 16) != 0, boss2 = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "arenas27", "hazardFloor27", "pulse27", "chargeShots27", "boss1", "boss2" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT27 block off";
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

new(G + 'SurvivalSim.N27.cs', '''// NIGHT27 (the shared commit, the lead owns this file): the six sim-changing NIGHT27 design blocks in the sim (Core/N27Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n27Override), --off27 / -skygrindOff27 (and --off26 and older)
// take every block off (N27Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's, one each):
// - W (weapons) implements N27WeaponsStartRun / N27WeaponsStep (Game/SurvivalSim.Weapons27.cs): pulse27, chargeShots27.
// - B (bosses) implements N27BossStartRun / N27BossStep (Game/SurvivalSim.Boss27.cs): boss1 (THE LANTERN), boss2 (THE GLACIER).
// - A (levels) implements N27LevelStartRun / N27LevelStep (Game/SurvivalSim.Level27.cs): arenas27, hazardFloor27.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N27Blocks n27Design;

        /// <summary>A replay's or a harness's NIGHT27 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N27Blocks? n27Override;

        /// <summary>The NIGHT27 blocks the current run reads (set at StartRun).</summary>
        public N27Blocks n27;

        void N27Init() => n27Design = N27Blocks.From(cfg);

        void N27StartRun()
        {
            n27 = n27Override ?? (N27Blocks.ForceOff ? N27Blocks.Off : n27Design);
            N27WeaponsStartRun();
            N27BossStartRun();
            N27LevelStartRun();
        }

        partial void N27WeaponsStartRun();
        partial void N27WeaponsStep(double dt);
        partial void N27BossStartRun();
        partial void N27BossStep(double dt);
        partial void N27LevelStartRun();
        partial void N27LevelStep(double dt);
    }
}
''')

# ---- the call sites
SR = G + 'SurvivalSim.Run.cs'
edit(SR, '            N26StartRun(); // NIGHT26: the run\'s NIGHT26 blocks (a tape\'s, or off with --off26 and older), the same way (SurvivalSim.N26.cs)\n',
         '            N26StartRun(); // NIGHT26: the run\'s NIGHT26 blocks (a tape\'s, or off with --off26 and older), the same way (SurvivalSim.N26.cs)\n            N27StartRun(); // NIGHT27: the run\'s NIGHT27 blocks (a tape\'s, or off with --off27 and older), the same way (SurvivalSim.N27.cs)\n')
edit(SR, '            N26LevelStep(step);\n', '            N26LevelStep(step);\n            N27WeaponsStep(step); // NIGHT27: the lead\'s hooks for the NIGHT27 rows (SurvivalSim.N27.cs; nothing without a body)\n            N27BossStep(step);\n            N27LevelStep(step);\n')
edit(SR, '            N26Blocks? n26Saved = n26Override; // NIGHT26: and every NIGHT26 block off until the freeze (FingerprintBaseline.N26OnPath)\n',
         '            N27Blocks? n27Saved = n27Override; // NIGHT27: and every NIGHT27 block off until the freeze (FingerprintBaseline.N27OnPath)\n            if (!FingerprintBaseline.N27OnPath) n27Override = N27Blocks.Off;\n            N26Blocks? n26Saved = n26Override; // NIGHT26: and every NIGHT26 block off until the freeze (FingerprintBaseline.N26OnPath)\n')
edit(SR, '                n26Override = n26Saved;\n', '                n26Override = n26Saved;\n                n27Override = n27Saved;\n')

edit(G + 'Tape/TapeScript.cs', '            N26Blocks? n26Saved = sim.n26Override;',
     '            N27Blocks? n27Saved = sim.n27Override; // NIGHT27: and every NIGHT27 block off until the freeze (N27OnPath)\n            if (!FingerprintBaseline.N27OnPath) sim.n27Override = N27Blocks.Off;\n            N26Blocks? n26Saved = sim.n26Override;')
edit(G + 'Tape/TapeScript.cs', '                sim.n26Override = n26Saved;\n', '                sim.n26Override = n26Saved;\n                sim.n27Override = n27Saved;\n')

edit(G + 'Tape/TapeRecorder.cs', '            tape.n26 = sim.n26.Bits; // NIGHT26: the NIGHT26 blocks the run read\n',
     '            tape.n26 = sim.n26.Bits; // NIGHT26: the NIGHT26 blocks the run read\n            tape.n27 = sim.n27.Bits; // NIGHT27: the NIGHT27 blocks the run read\n')
edit(G + 'Tape/TapeReplay.cs', '            sim.n26Override = N26Blocks.FromBits(tape.n26);',
     '            sim.n27Override = N27Blocks.FromBits(tape.n27); // NIGHT27: the NIGHT27 blocks the run read (formats 1-10: all off, NIGHT26\'s game)\n            sim.n26Override = N26Blocks.FromBits(tape.n26);')

RT = G + 'Tape/RunTape.cs'
edit(RT, '        public int n26;\n', '        public int n26;\n\n        /// <summary>NIGHT27: the NIGHT27 design blocks the run read (N27Blocks.Bits; formats 1-10: 0, every block off, NIGHT26\'s game).</summary>\n        public int n27;\n')
edit(RT, 'FormatVersion = 10;', 'FormatVersion = 11;')
edit(RT, 'NIGHT26: 10 adds the NIGHT26 blocks (formats 1-9: off);', 'NIGHT27: 11 adds the NIGHT27 blocks (formats 1-10: off); NIGHT26: 10 adds the NIGHT26 blocks (formats 1-9: off);')
edit(RT, '            w.Byte((byte)n26); // format 10: the NIGHT26 blocks\n', '            w.Byte((byte)n26); // format 10: the NIGHT26 blocks\n            w.Byte((byte)n27); // format 11: the NIGHT27 blocks\n')
edit(RT, '            t.n26 = version >= 10 ? r.Byte() : 0; // formats 1-9: recorded on NIGHT25\'s game (or before), so every NIGHT26 block OFF (explicitly)\n',
     '            t.n26 = version >= 10 ? r.Byte() : 0; // formats 1-9: recorded on NIGHT25\'s game (or before), so every NIGHT26 block OFF (explicitly)\n            t.n27 = version >= 11 ? r.Byte() : 0; // formats 1-10: recorded on NIGHT26\'s game (or before), so every NIGHT27 block OFF (explicitly)\n')

edit(G + 'FingerprintBaseline.cs', '        public const bool N26OnPath = false;\n',
     '        public const bool N26OnPath = false;\n\n        /// <summary>NIGHT27: false unless the freeze records a run with a sim-changing NIGHT27 default on (pulse27, chargeShots27);\n        /// until then the composed fingerprint runs with every NIGHT27 block off.</summary>\n        public const bool N27OnPath = false;\n')

# ---- the sim's Init call
import re
init_hits = [G + 'SurvivalSim.cs']
for p in init_hits:
    edit(p, 'N26Init();', 'N26Init();\n            N27Init(); // NIGHT27 (the shared commit): the NIGHT27 design blocks', 1)
print('N27Init call sites patched in:', init_hits)

# ---- --off27
PC = G + 'PlayerChecks.cs'
s = rd(PC)
nl = nlof(s)
s2 = s.replace('N26Blocks.ForceOff = true;', 'N26Blocks.ForceOff = N27Blocks.ForceOff = true;')
# the lines that set N26Blocks.ForceOff chained after other ForceOffs end with N26Blocks.ForceOff = true; the replace above covers them all
s2 = s2.replace('            if (Array.IndexOf(args, "-skygrindOff26") >= 0)', '            if (Array.IndexOf(args, "-skygrindOff27") >= 0) N27Blocks.ForceOff = true; // NIGHT27: every NIGHT27 design block off for every run (NIGHT26\'s game, the A side)\n            if (Array.IndexOf(args, "-skygrindOff26") >= 0)', 1)
assert s2 != s
wr(PC, s2)

PG = 'Tools/SimHarness/Program.cs'
edit(PG, '        N26Blocks.ForceOff = off26;\n',
     '        N26Blocks.ForceOff = off26;\n        // NIGHT27: --off27 runs every command with every NIGHT27 design block off (NIGHT26\'s game; tonight\'s A side);\n        // --off26 and the older switches turn off these blocks as well\n        bool off27 = off26 || Array.IndexOf(args, "--off27") >= 0;\n        N27Blocks.ForceOff = off27;\n')
edit(PG, '                if (off26) sim.n26Override = N26Blocks.Off; // NIGHT26: the same\n',
     '                if (off26) sim.n26Override = N26Blocks.Off; // NIGHT26: the same\n                if (off27) sim.n27Override = N27Blocks.Off; // NIGHT27: the same\n')

# ---- design.n27 (every row off)
rows = [
 ('arenas27', "The first three levels rebuilt tight (GG 2026-10-09: 'the arenas are still way too wide ... a complete overhaul on the first three stages'): rooms at most 24 m wide x 40 m long, enemy counts from a cost budget, two wall-ride lanes per room, the three random LIMIT rooms removed. Mostly level data + a validator rule; the bit gates the campaign's use of the new plans. Off (--off27): NIGHT26's rooms."),
 ('hazardFloor27', "A floor strip that damages for a limited time (telegraph, live, spacing) so the wall lane is the safe route (GG: 'areas where you're forced to wall ride because the terrain might damage you temporarily'). Numbers under this row, each with a _why. Off (--off27): the strip is plain floor."),
 ('pulse27', "The PULSE GAUNTLET: a heavy swing every 2.5 s, five KILLS fill five pips (cap one stored charge), only a full charge enables the LT blast + a short self-boost (GG: 'the pulse needs to attack much slower ... after you charge it, after say killing five enemies, then you unlock the left trigger'). Off (--off27): NIGHT26's pulseLT (7 landed hits per notch)."),
 ('chargeShots27', "RT held on the PISTOL and the SHOTGUN: a tap fires as today, a hold past a threshold charges and fires on release, with a long cooldown after a charged shot (GG: 'the pistol, when you hold the right trigger, it'll do a charge attack but it's on a longer cool down. Same thing with the shotgun'). Off (--off27): the normal shot only."),
 ('boss1', "Level 1's boss is THE LANTERN: a hovering shielded box whose shield is down only on the face that looks at a wall the player is riding (GG: 'the first boss fight is way too much like the Cerberus ... original, requires actual uses of our current gameplay mechanics'). Off (--off27): THE SLAG HULK."),
 ('boss2', "Level 2's boss is THE GLACIER as a pulse-only chase on a curving ice course with the original body (GG: 'you can only attack him if you are able to catch up to him and hit him with the pulse'). Off (--off27): NIGHT26's glacier."),
 ('slowAim27', "View row: world time eases to a fraction while wall-riding AND aiming, with a screen cue (GG: 'if you aim a weapon while riding the game kind of temporarily slows down a little bit'). Read directly (no tape bit): the loop driver's accumulator dt only, never the sim."),
 ('skate27', "View row: the ice skate feel (speed FOV, roll, floor streaks, dust, blade trails) with no border streaks (GG: 'there's also no skate effect: I want it to feel like you're really dashing from right to left')."),
 ('roof27', "View row: a ceiling over every room and link of levels 1-3, vertex AO, emissive strips, gradient ambient + fog, a skylight slot over boss arenas (GG: 'there needs to be a roof ... almost like indoors')."),
 ('tutorial27', "Level data row: the tutorial rebuilt so no stage repeats a lesson (GG: 'the tutorial section is fine but kind of repeats similar things')."),
]
p = 'Design/unity-config.json'
s = rd(p)
nl = nlof(s)
why = "NIGHT27 (GG 2026-10-09, after his playtest of NIGHT26: tight arenas with wall lanes, an original wall-ride boss, a pulse-only ice chase, a slower PULSE, charge shots, slow-mo on a wall-ride aim, roofs): the ten NIGHT27 rows, all off in the shared commit; a row is turned on only when its contract passes. Off (--off27 / -skygrindOff27): NIGHT26's game bit for bit. Unity-only numbers live under each row with a _why (a new gameplay number the web never had: its file is design.n27 in this config)."
parts = ['      "%s": {\n        "_why": %s,\n        "enabled": false\n      }' % (n, json.dumps(w)) for n, w in rows]
block = ',\n    "n27": {\n      "_why": ' + json.dumps(why) + ',\n' + ',\n'.join(parts) + '\n    }'
old = '    }\n  }\n}'.replace('\n', nl)
assert s.rstrip().endswith(old)
i = s.rstrip().rindex(old)
out = s[:i] + '    }' + block.replace('\n', nl) + nl + '  }' + nl + '}'
wr(p, out)
d = json.loads(out)
print('n27 rows:', [k for k in d['design']['n27'] if k != '_why'], 'n26 rows kept:', len(d['design']['n26']))
