// NIGHT29 shared commit: the plumbing for Core/N29Blocks.cs (tape format 12, overrides, the off switches, the config rows).
// Re-runnable: each edit is skipped when its marker is already present.
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
function edit(rel, fn) {
  const f = path.join(root, rel);
  const raw = fs.readFileSync(f, 'utf8');
  const crlf = raw.includes('\r\n');
  let s = crlf ? raw.replace(/\r\n/g, '\n') : raw;
  const out = fn(s);
  if (out === s) { console.log('unchanged ' + rel); return; }
  fs.writeFileSync(f, crlf ? out.replace(/\n/g, '\r\n') : out);
  console.log('patched ' + rel);
}
function after(s, anchor, add, marker) {
  if (s.includes(marker)) return s;
  const i = s.indexOf(anchor);
  if (i < 0) throw new Error('anchor not found: ' + anchor.slice(0, 60));
  return s.slice(0, i + anchor.length) + add + s.slice(i + anchor.length);
}
function before(s, anchor, add, marker) {
  if (s.includes(marker)) return s;
  const i = s.indexOf(anchor);
  if (i < 0) throw new Error('anchor not found: ' + anchor.slice(0, 60));
  return s.slice(0, i) + add + s.slice(i);
}
function repl(s, a, b, marker) {
  if (s.includes(marker)) return s;
  if (!s.includes(a)) throw new Error('missing: ' + a.slice(0, 60));
  return s.replace(a, b);
}

edit('Assets/Scripts/Game/SurvivalSim.cs', s =>
  after(s, 'N27Init(); // NIGHT27: the NIGHT27 design blocks, read once (SurvivalSim.N27.cs)\n', '            N29Init(); // NIGHT29: the NIGHT29 design blocks, read once (SurvivalSim.N29.cs)\n', 'N29Init();'));

edit('Assets/Scripts/Game/SurvivalSim.Run.cs', s => {
  s = after(s, 'N27StartRun(); // NIGHT27: the run\'s NIGHT27 blocks (a tape\'s, or off with --off27 and older), the same way (SurvivalSim.N27.cs)\n',
    '            N29StartRun(); // NIGHT29: the run\'s NIGHT29 blocks (a tape\'s, or off with --off29 and older), the same way (SurvivalSim.N29.cs)\n', 'N29StartRun();');
  s = after(s, 'N27LevelStep(step);\n', '            N29SystemsStep(step); // NIGHT29: the lead\'s hooks for the NIGHT29 rows (SurvivalSim.N29.cs; nothing without a body)\n            N29BossStep(step);\n', 'N29SystemsStep(step)');
  s = after(s, 'if (!FingerprintBaseline.N27OnPath) n27Override = N27Blocks.Off;\n',
    '            N29Blocks? n29Saved = n29Override; // NIGHT29: and every NIGHT29 block off until the freeze (FingerprintBaseline.N29OnPath)\n            if (!FingerprintBaseline.N29OnPath) n29Override = N29Blocks.Off;\n', 'n29Saved');
  s = after(s, '                n27Override = n27Saved;\n', '                n29Override = n29Saved;\n', 'n29Override = n29Saved');
  return s;
});

edit('Assets/Scripts/Game/Tape/RunTape.cs', s => {
  s = repl(s, 'public const int FormatVersion = 11; // ', 'public const int FormatVersion = 12; // NIGHT29: 12 adds the NIGHT29 blocks (formats 1-11: off); ', 'FormatVersion = 12');
  s = after(s, '        public int n27;\n', '\n        /// <summary>NIGHT29: the NIGHT29 design blocks the run read (N29Blocks.Bits; formats 1-11: 0, every block off, NIGHT28\'s game).</summary>\n        public int n29;\n', 'public int n29;');
  s = after(s, 'w.Byte((byte)n27); // format 11: the NIGHT27 blocks\n', '            w.Byte((byte)n29); // format 12: the NIGHT29 blocks\n', 'format 12: the NIGHT29 blocks');
  s = after(s, 't.n27 = version >= 11 ? r.Byte() : 0; // formats 1-10: recorded on NIGHT26\'s game (or before), so every NIGHT27 block OFF (explicitly)\n',
    '            t.n29 = version >= 12 ? r.Byte() : 0; // formats 1-11: recorded on NIGHT28\'s game (or before), so every NIGHT29 block OFF (explicitly)\n', 't.n29 =');
  return s;
});

edit('Assets/Scripts/Game/Tape/TapeRecorder.cs', s =>
  after(s, 'tape.n27 = sim.n27.Bits; // NIGHT27: the NIGHT27 blocks the run read\n', '            tape.n29 = sim.n29.Bits; // NIGHT29: the NIGHT29 blocks the run read\n', 'tape.n29'));

edit('Assets/Scripts/Game/Tape/TapeReplay.cs', s =>
  after(s, 'sim.n27Override = N27Blocks.FromBits(tape.n27); // NIGHT27: the NIGHT27 blocks the run read (formats 1-10: all off, NIGHT26\'s game)\n',
    '            sim.n29Override = N29Blocks.FromBits(tape.n29); // NIGHT29: the NIGHT29 blocks the run read (formats 1-11: all off, NIGHT28\'s game)\n', 'n29Override'));

edit('Assets/Scripts/Game/Tape/TapeScript.cs', s => {
  s = after(s, 'if (!FingerprintBaseline.N27OnPath) sim.n27Override = N27Blocks.Off;\n',
    '            N29Blocks? n29Saved = sim.n29Override; // NIGHT29: and every NIGHT29 block off until the freeze (N29OnPath)\n            if (!FingerprintBaseline.N29OnPath) sim.n29Override = N29Blocks.Off;\n', 'n29Saved');
  s = after(s, '                sim.n27Override = n27Saved;\n', '                sim.n29Override = n29Saved;\n', 'sim.n29Override = n29Saved');
  return s;
});

edit('Assets/Scripts/Game/FingerprintBaseline.cs', s =>
  after(s, 'public const bool N27OnPath = false;\n',
    '\n        /// <summary>NIGHT29: false unless the freeze records a run with a sim-changing NIGHT29 default on; until then the composed\n        /// fingerprint runs with every NIGHT29 block off.</summary>\n        public const bool N29OnPath = false;\n', 'N29OnPath'));

edit('Assets/Scripts/Game/PlayerChecks.cs', s => {
  // every older -skygrindOffNN switch also turns the NIGHT29 blocks off; -skygrindOff29 alone turns only them off
  s = after(s, 'if (Array.IndexOf(args, "-skygrindOff27") >= 0) N27Blocks.ForceOff = true; // NIGHT27: every NIGHT27 design block off for every run (NIGHT26\'s game, the A side)\n',
    '            for (int o29 = 19; o29 <= 29; o29++) if (Array.IndexOf(args, "-skygrindOff" + o29) >= 0) N29Blocks.ForceOff = true; // NIGHT29: -skygrindOff29 and every older switch turn the NIGHT29 blocks off\n', 'o29');
  return s;
});

edit('Tools/SimHarness/Program.cs', s => {
  s = after(s, 'N27Blocks.ForceOff = off27;\n',
    '        // NIGHT29: --off29 runs every command with every NIGHT29 design block off (NIGHT28\'s game); every older switch turns them off as well\n        bool off29 = off27 || Array.IndexOf(args, "--off29") >= 0;\n        N29Blocks.ForceOff = off29;\n', 'off29');
  s = after(s, 'if (off27) sim.n27Override = N27Blocks.Off; // NIGHT27: the same\n', '                if (off29) sim.n29Override = N29Blocks.Off; // NIGHT29: the same\n', 'sim.n29Override');
  return s;
});

// the older tapes' test helpers: a format 12 tape first loses its NIGHT29 byte
edit('Assets/Tests/EditMode/Game/N26SharedTests.cs', s => {
  s = repl(s, '            if (BitConverter.ToInt32(b11, RunTape.Magic.Length) < 11) return b11;\n',
    '            b11 = N29SharedTests.AsFormat11(b11); // NIGHT29: a format 12 tape first loses its NIGHT29 byte\n            if (BitConverter.ToInt32(b11, RunTape.Magic.Length) < 11) return b11;\n', 'N29SharedTests.AsFormat11');
  return s;
});
edit('Assets/Tests/EditMode/Game/N27SharedTests.cs', s => {
  s = repl(s, 'Is.EqualTo(11));\n            RunTape back', 'Is.EqualTo(RunTape.FormatVersion)); // NIGHT29: the current format is 12; the n27 byte is read below\n            RunTape back', 'Is.EqualTo(RunTape.FormatVersion)); // NIGHT29');
  return s;
});

// the config: the n29 rows (all enabled false until each item lands), before design.n27
edit('Design/unity-config.json', s => {
  if (s.includes('"n29": {')) return s;
  const block = fs.readFileSync(path.join(__dirname, 'n29-config-block.json'), 'utf8').replace(/\r\n/g, '\n').replace(/\n+$/, '');
  const i = s.indexOf('    "n27": {');
  if (i < 0) throw new Error('design.n27 anchor not found');
  return s.slice(0, i) + block + ',\n' + s.slice(i);
});
