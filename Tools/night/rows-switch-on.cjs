// NIGHT45 builder S1 (rows as data; docs/NIGHT-ROWS.md): NIGHT46's ONE-TIME switch-on of the rows registry (Core/NightRows.cs,
// Game/SurvivalSim.Rows.cs, Game/Tape/TapeRows.cs), the lead's to run at NIGHT46's G0, once:
//   node Tools/night/rows-switch-on.cjs --dry     every anchor found exactly once? prints the plan, writes nothing (exit 1 on a miss)
//   node Tools/night/rows-switch-on.cjs           applies it (refuses a file whose anchor is missing or ambiguous; skips an edit already in)
// 30 edits in 19 files, one or two lines each (none in Design/: no config change): the sim's Init / StartRun / Step / fingerprint lines, the tape's format 27 (once; no later night bumps
// it for a row), the off switches (-skygrindOffNN / --offNN), the level rows' swap and base id, and the test chain (the format pins
// 26 -> 27, AsFormat25 through NightRowsTapeTests.AsFormat26, the old-order tests with every NIGHT46+ row off). No config change.
// Dry-run by S1 on NIGHT45 (Research/2026-10-17-wave45/S1.md): applied in the S1 worktree, the gates measured, then reverted.
const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..', '..');
const dry = process.argv.includes('--dry');
const MARK = 'NIGHT46 rows'; // every inserted line carries it (the idempotence check)

// kind 'after': insert `text` (one or more lines, without indentation) after the one line containing `anchor`, at that line's indentation
// (plus `indent` extra spaces). kind 'replace': replace the one occurrence of `anchor` with `text`.
const edits = [
  { file: 'Assets/Scripts/Game/SurvivalSim.cs', kind: 'after', anchor: 'N43Init(); // NIGHT43:',
    text: 'RowsInit(); // NIGHT46 rows (docs/NIGHT-ROWS.md): every NIGHT46+ sim row, read once (SurvivalSim.Rows.cs, Core/NightRows.cs)' },
  { file: 'Assets/Scripts/Game/SurvivalSim.Run.cs', kind: 'after', anchor: 'N43StartRun(); // NIGHT43:',
    text: 'RowsStartRun(); // NIGHT46 rows: the run\'s NIGHT46+ rows (a tape\'s, or off with -skygrindOffNN / --offNN from their night), then each night\'s StartRun hook' },
  { file: 'Assets/Scripts/Game/SurvivalSim.Run.cs', kind: 'after', anchor: 'N43Step(step); // NIGHT43',
    text: 'RowsStep(step); // NIGHT46 rows: each NIGHT46+ night\'s step hook in order (SurvivalSim.NNN.cs; nothing until a night fills its hook)' },
  { file: 'Assets/Scripts/Game/SurvivalSim.Run.cs', kind: 'after', anchor: 'if (!FingerprintBaseline.N43OnPath) n43Override = N43Blocks.Off;',
    text: 'RowsFingerprintEnter(); // NIGHT46 rows: rows of nights after RowsBaseline.OnPathThrough off until their freeze' },
  { file: 'Assets/Scripts/Game/SurvivalSim.Run.cs', kind: 'after', anchor: 'n43Override = n43Saved;',
    text: 'RowsFingerprintExit(); // NIGHT46 rows' },
  { file: 'Assets/Scripts/Game/Tape/TapeScript.cs', kind: 'after', anchor: 'if (!FingerprintBaseline.N43OnPath) sim.n43Override = N43Blocks.Off;',
    text: 'sim.RowsFingerprintEnter(); // NIGHT46 rows: rows of nights after RowsBaseline.OnPathThrough off until their freeze' },
  { file: 'Assets/Scripts/Game/Tape/TapeScript.cs', kind: 'after', anchor: 'sim.n43Override = n43Saved;',
    text: 'sim.RowsFingerprintExit(); // NIGHT46 rows' },
  { file: 'Assets/Scripts/Game/Tape/RunTape.cs', kind: 'replace', anchor: 'public const int FormatVersion = 26; // NIGHT43:',
    text: 'public const int FormatVersion = 27; // NIGHT46 rows: 27 adds the rows record (Core/NightRows.cs, TapeRows; formats 1-26: every NIGHT46+ row off; no later night bumps the tape for a row); NIGHT43:' },
  { file: 'Assets/Scripts/Game/Tape/RunTape.cs', kind: 'after', anchor: 'public int n43;',
    text: ['', '/// <summary>NIGHT46 rows: the NIGHT46+ sim rows the run read, as NightRowsTape\'s record (name -> bit; formats 1-26: null, every row off).</summary>', 'public byte[] rows;'] },
  { file: 'Assets/Scripts/Game/Tape/RunTape.cs', kind: 'after', anchor: 'w.VarI(n43); // format 26: the NIGHT43 blocks',
    text: 'TapeRows.Write(w, rows); // format 27: the NIGHT46 rows record (NIGHT46 rows)' },
  { file: 'Assets/Scripts/Game/Tape/RunTape.cs', kind: 'after', anchor: 't.n43 = version >= 26 ? r.VarI() : 0;',
    text: 't.rows = version >= NightRowsTape.FirstTapeFormat ? TapeRows.Read(bytes, r) : null; // NIGHT46 rows: formats 1-26 recorded before any NIGHT46 row, so every row OFF (explicitly)' },
  { file: 'Assets/Scripts/Game/Tape/TapeRecorder.cs', kind: 'after', anchor: 'tape.n43 = sim.n43.Bits;',
    text: 'tape.rows = sim.RowsTapeRecord(); // NIGHT46 rows: the NIGHT46+ rows the run read' },
  { file: 'Assets/Scripts/Game/Tape/TapeReplay.cs', kind: 'after', anchor: 'sim.n43Override = N43Blocks.FromBits(tape.n43);',
    text: 'sim.rowsOverride = TapeRows.Replay(sim.rowsRegistry, tape.rows); // NIGHT46 rows: the rows the run read (formats 1-26: all off; a row ON this build lacks is refused)' },
  { file: 'Assets/Scripts/Game/PlayerChecks.cs', kind: 'after', anchor: 'for (int o43 = 19; o43 <= 43; o43++)',
    text: 'NightRows.ForceOffFrom = NightRows.OffFrom(args, "-skygrindOff"); // NIGHT46 rows: -skygrindOffNN turns off every row of night NN and later (every older switch: all of them)' },
  { file: 'Tools/SimHarness/Program.cs', kind: 'after', anchor: 'N45Levels.ForceOff = off43 ||',
    text: 'NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off"); // NIGHT46 rows: --offNN turns off every row of night NN and later (every older switch: all of them)' },
  { file: 'Assets/Scripts/Game/N29Fixes.cs', kind: 'replace', anchor: 'a != "-skygrindOff45" && ',
    text: 'a != "-skygrindOff45" && Skygrind.Core.NightRows.OffSwitchNight(a, "-skygrindOff") < Skygrind.Core.NightRows.FirstNight /* NIGHT46 rows: -skygrindOff46 and later boot like a player\'s game */ && ' },
  { file: 'Assets/Scripts/Core/Curriculum25.cs', kind: 'after', anchor: 'N45Levels.Swap(cfg, res);',
    text: 'NightRows.SwapLevels(cfg, res); // NIGHT46 rows: the NIGHT46+ level rows (design.nNN.<row>.levelFrom / levelTo), nights in order' },
  { file: 'Assets/Scripts/Core/N45Levels.cs', kind: 'after', anchor: 'public static string BaseId(GameConfig cfg, string levelId)', indent: 4,
    text: '', // placeholder: the insertion goes after the method's opening brace (see below)
    afterBrace: 'levelId = NightRows.BaseId(cfg, levelId); // NIGHT46 rows: a NIGHT46+ level is its row\'s levelFrom first (n46-foundry -> n45-foundry), allocation-free' },
  { file: 'Assets/Tests/EditMode/Game/N43SharedTests.cs', kind: 'after', anchor: 'public static byte[] AsFormat25(byte[] b26)', indent: 4,
    text: '', afterBrace: 'b26 = NightRowsTapeTests.AsFormat26(b26); // NIGHT46 rows: a format 27 tape first loses its rows record (the chain\'s last link)' },
  { file: 'Assets/Tests/EditMode/Game/N37SharedTests.cs', kind: 'replace', anchor: 'bool was, was38, was39, was40, was41, was42, was43;',
    text: 'bool was, was38, was39, was40, was41, was42, was43; int wasRows; // NIGHT46 rows' },
  { file: 'Assets/Tests/EditMode/Game/N37SharedTests.cs', kind: 'replace', anchor: 'was43 = N43Blocks.ForceOff; N43Blocks.ForceOff = true; }',
    text: 'was43 = N43Blocks.ForceOff; N43Blocks.ForceOff = true; wasRows = NightRows.ForceOffFrom; NightRows.ForceOffFrom = NightRows.FirstNight; /* NIGHT46 rows: and every NIGHT46+ row off */ }' },
  { file: 'Assets/Tests/EditMode/Game/N37SharedTests.cs', kind: 'replace', anchor: 'N43Blocks.ForceOff = was43; }',
    text: 'N43Blocks.ForceOff = was43; NightRows.ForceOffFrom = wasRows; /* NIGHT46 rows */ }' },
  { file: 'Assets/Tests/EditMode/Lesson25Tests.cs', kind: 'after', anchor: 'N45Levels.ForceOff = true; // NIGHT45 (L-027)',
    text: 'NightRows.ForceOffFrom = NightRows.FirstNight; // NIGHT46 rows (L-027): the NIGHT46+ level rows pin nothing older' },
];
// the format pins: the one current-format assert in each of N37-N43SharedTests
for (const n of [37, 38, 39, 40, 41, 42, 43])
  edits.push({ file: `Assets/Tests/EditMode/Game/N${n}SharedTests.cs`, kind: 'replace', anchor: 'BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(26)',
    text: 'BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */' });

function count(hay, needle) {
  let n = 0, i = 0;
  while ((i = hay.indexOf(needle, i)) >= 0) { n++; i += needle.length; }
  return n;
}

const files = {};
let misses = 0, applied = 0, skipped = 0;
for (const e of edits) {
  const p = path.join(root, e.file);
  if (!files[e.file]) {
    if (!fs.existsSync(p)) { console.log(`MISS ${e.file}: no such file`); misses++; continue; }
    const raw = fs.readFileSync(p, 'utf8');
    files[e.file] = { raw, text: raw, eol: raw.includes('\r\n') ? '\r\n' : '\n' };
  }
  const f = files[e.file];
  const insert = e.afterBrace || e.text;
  const probe = Array.isArray(insert) ? insert.filter(Boolean).pop() : insert;
  if (probe && f.text.includes(probe)) { console.log(`skip ${e.file}: already in (${probe.slice(0, 60)}...)`); skipped++; continue; }
  const n = count(f.text, e.anchor);
  if (n !== 1) { console.log(`MISS ${e.file}: anchor found ${n} times: ${e.anchor}`); misses++; continue; }
  const at = f.text.indexOf(e.anchor);
  if (e.kind === 'replace') {
    f.text = f.text.slice(0, at) + e.text + f.text.slice(at + e.anchor.length);
  } else {
    const ls = f.text.lastIndexOf('\n', at) + 1;
    const indent = /^[ \t]*/.exec(f.text.slice(ls))[0];
    let le = f.text.indexOf('\n', at);
    if (e.afterBrace) {
      // the method's opening brace is on the next line: insert after it, one level deeper
      const braceLine = f.text.indexOf('\n', le + 1);
      if (!/^\s*\{\s*$/.test(f.text.slice(le + 1, braceLine).replace(/\r$/, ''))) { console.log(`MISS ${e.file}: no '{' line after ${e.anchor}`); misses++; continue; }
      le = braceLine;
      f.text = f.text.slice(0, le + 1) + indent + ' '.repeat(e.indent || 4) + e.afterBrace + f.eol + f.text.slice(le + 1);
    } else {
      const lines = (Array.isArray(e.text) ? e.text : [e.text]).map(t => (t ? indent + ' '.repeat(e.indent || 0) + t : '') + f.eol).join('');
      f.text = f.text.slice(0, le + 1) + lines + f.text.slice(le + 1);
    }
  }
  console.log(`${dry ? 'plan' : 'edit'} ${e.file}: ${e.kind} at "${e.anchor.slice(0, 50)}"`);
  applied++;
}
if (misses > 0) { console.log(`rows-switch-on: ${misses} miss(es): nothing written`); process.exit(1); }
if (!dry) for (const [rel, f] of Object.entries(files)) if (f.text !== f.raw) fs.writeFileSync(path.join(root, rel), f.text);
console.log(`rows-switch-on: ${applied} edit(s) ${dry ? 'planned (dry run, nothing written)' : 'applied'}, ${skipped} already in, ${Object.keys(files).length} file(s)`);
