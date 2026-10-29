// PG3 pass 3 (GG 2026-10-28: "each level should have 3+ breakcore style tracks that fit the stage"): a fourth combat track per level (<level>-combat-d),
// written to its stage, so each level rotates four breakcore fight themes. Idempotent: an id already in music.json is left alone.
// Usage: node Tools/pg3/add-pass3.cjs
'use strict';
const fs = require('fs');
const path = require('path');

const music = [
  { id: 'foundry-combat-d', state: 'combat', level: 'foundry', seconds: 120, prompt: 'Instrumental breakcore, D minor, 180 bpm, a fourth fight theme for a molten foundry at dusk: a clanking conveyor-belt rhythm of hammered steel and hissing pistons that drops into furious chopped amen breaks with rapid glitch stutters, a grinding distorted reese bass like molten metal, sparking bit-crushed snare rolls, a heroic detuned supersaw hook over sad piano chords, a sudden half-time drop where the beat slams like a forge press. Hot, industrial, emotional, loud crisp mix, no vocals, loopable.' },
  { id: 'iceline-combat-d', state: 'combat', level: 'iceline', seconds: 120, prompt: 'Instrumental breakcore, F sharp minor, 174 bpm, a fourth fight theme on a frozen rail line at night: cracking ice and a lonely celesta melody open, then crisp fast chopped amen breaks with stuttering glitch edits, a cold deep sub bass, shimmering frozen bell arpeggios, a train-horn-like synth swell, a bright melancholic supersaw lead that soars over the drop, freeze-frame silences before each slam. Icy, fast, bittersweet, high-fidelity, no vocals, loopable.' },
  { id: 'kiln-combat-d', state: 'combat', level: 'kiln', seconds: 120, prompt: 'Instrumental breakcore, C minor, 184 bpm, a fourth fight theme in scorching kiln halls: roaring fire and deep war drums build into relentless chopped amen breaks with violent glitch stutters, a snarling overdriven reese bass, low brass and taiko hits, a burning distorted guitar riff doubled by a screaming supersaw, a heat-haze filter sweep into a crushing half-time drop. Aggressive, hot, relentless, loud crisp mix, no vocals, loopable.' },
  { id: 'span-combat-d', state: 'combat', level: 'span', seconds: 120, prompt: 'Instrumental breakcore, A minor, 176 bpm, a fourth fight theme on a vast bridge in the wind at dusk: wind gusts and a soaring string section open, then intricate chopped amen breaks with glitch stutters and rapid ghost-note rolls, a clockwork harpsichord riff, a wobbling reese bass, a high flying supersaw melody, choir swells into a huge drop with the strings returning. Epic, vertiginous, emotional, crisp mix, no vocals, loopable.' },
  { id: 'spire-combat-d', state: 'combat', level: 'spire', seconds: 120, prompt: 'Instrumental breakcore, E minor, 182 bpm, a fourth fight theme for the final bell tower at dusk: a cathedral pipe organ and tolling bells, then the fastest most intense chopped amen breaks with glitch stutters and machine-gun snare fills, a massive reese bass, a desperate choir, a soaring emotional supersaw lead, a piano breakdown that explodes into a final climactic drop. Grand, desperate, triumphant, loud crisp mix, no vocals, loopable.' },
];

const p = path.join(__dirname, 'music.json');
const json = JSON.parse(fs.readFileSync(p, 'utf8'));
let added = 0;
for (const t of music) if (!json.tracks.some(x => x.id === t.id)) { json.tracks.push(t); added++; }
fs.writeFileSync(p, JSON.stringify(json, null, 2) + '\n');
console.log('music.json: added ' + added + ' of ' + music.length);
