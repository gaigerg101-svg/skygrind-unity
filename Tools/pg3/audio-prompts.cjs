// PG3: writes docs/pg3/AUDIO_PROMPTS.md (the prompt log's audio section) from the generation logs: every ElevenLabs prompt sent,
// its endpoint / model, length, HTTP status, the game event it plays on, and the levelling it got. Re-run after any generation.
// Usage: node Tools/pg3/audio-prompts.cjs
'use strict';
const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..', '..');
const read = f => (fs.existsSync(path.join(__dirname, f)) ? JSON.parse(fs.readFileSync(path.join(__dirname, f), 'utf8')) : []);
const sfx = read('sfx-gen-log.json');
const music = read('music-gen-log.json');
const norm = read('normalize-log.json');
const lvl = {};
for (const n of norm) lvl[(n.music ? 'm:' : 's:') + n.id] = n;
const lists = ['sounds.json', 'sounds2.json'].flatMap(f => (fs.existsSync(path.join(__dirname, f)) ? JSON.parse(fs.readFileSync(path.join(__dirname, f), 'utf8')).sounds : []));
const triad = {};
for (const s of lists) triad[s.id] = s.triad || '';

const cell = s => String(s == null ? '' : s).replace(/\|/g, '/').replace(/\n/g, ' ');
// the last successful request per id is the shipped clip; failed ones are listed as error recovery
const lastOk = log => { const m = new Map(); for (const e of log) if (e.status === 200) m.set(e.id, e); return [...m.values()]; };
const fails = log => log.filter(e => e.status !== 200);

let md = '# Audio prompts (ElevenLabs): the prompt log\'s section 5\n\n';
md += 'Written by `node Tools/pg3/audio-prompts.cjs` from `Tools/pg3/sfx-gen-log.json`, `music-gen-log.json` and `normalize-log.json` ';
md += '(' + new Date().toISOString().slice(0, 16).replace('T', ' ') + ' UTC). Route: the ElevenLabs HTTP API called from Node scripts the ';
md += 'coding agent wrote (`Tools/pg3/sfx-gen.cjs`, `music-gen.cjs`); the clips land in `Assets/Resources/Audio/PG3/` and are played by ';
md += '`Assets/Scripts/Game/Pg3Audio.cs` (effects) and `MusicDirector.cs` (music). No prompt names an artist or a game (ElevenLabs\' rule ';
md += 'and GG\'s "obviously it needs to be origional").\n\n';

const m = lastOk(music);
md += '## Music (' + m.length + ' tracks, POST /v1/music, force_instrumental)\n\n| id | model | state | level | length | LUFS before -> after | prompt |\n|---|---|---|---|---|---|---|\n';
for (const e of m) {
  const n = lvl['m:' + e.id];
  md += `| ${e.id} | ${e.model || 'music_v1'} | ${e.state || ''} | ${e.level || ''} | ${e.seconds} s | ${n ? n.before + ' -> ' + n.after : ''} | ${cell(e.prompt)} |\n`;
}
const s = lastOk(sfx);
md += '\n## Sound effects (' + s.length + ' clips, POST /v1/sound-generation, prompt_influence 0.5)\n\n| id | triad | plays on | length | LUFS before -> after | prompt |\n|---|---|---|---|---|---|\n';
for (const e of s) {
  const n = lvl['s:' + e.id];
  md += `| ${e.id} | ${cell(triad[e.id] || e.triad)} | ${cell(e.event)} | ${e.seconds} s${e.loop ? ' loop' : ''} | ${n ? n.before + ' -> ' + n.after : ''} | ${cell(e.prompt)} |\n`;
}
const f = [...fails(sfx), ...fails(music)];
md += '\n## Failed requests (error recovery)\n\n';
if (!f.length) md += 'None.\n';
else {
  md += '| id | status | error | what was done |\n|---|---|---|---|\n';
  for (const e of f) {
    const err = cell((e.error || '').slice(0, 160));
    const fix = /duration_seconds/.test(e.error || '') ? 'sfx-gen clamps the length to 0.5-30 s; re-run made it'
      : /concurrent/.test(e.error || '') ? 'the scripts wait 4-6 s and retry on concurrent_limit_exceeded; music runs one or two at a time'
      : 'see the session log';
    md += `| ${e.id} | ${e.status} | ${err} | ${fix} |\n`;
  }
}
fs.writeFileSync(path.join(root, 'docs', 'pg3', 'AUDIO_PROMPTS.md'), md);
console.log('audio-prompts: ' + m.length + ' tracks, ' + s.length + ' effects, ' + f.length + ' failed requests -> docs/pg3/AUDIO_PROMPTS.md');
