// PG3 pass 2 (GG 2026-10-28 13:45: "make sound effects better like I asked before, some more breakcore songs and extra polish"):
// adds layered mixes for the plain gun shots and the big movement / death sounds to layers.json, and a third combat track per
// level (<level>-combat-c) to music.json. Idempotent: an id already present is left alone.
// Usage: node Tools/pg3/add-pass2.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const dir = __dirname;

const sub = (s, g, p) => ({ seconds: s, gain: g, prompt: p });
const L = (s, g, p, delay) => (delay ? { seconds: s, gain: g, delay, prompt: p } : sub(s, g, p));

function kestrel(flavor) {
  return [
    L(0.8, 0, 'Punchy deep sub bass gunshot thump, a tight low kick-drum-like impact, no high frequencies'),
    L(0.6, -1, 'Sharp powerful heavy revolver shot crack, ' + flavor + ', close mic, very bright transient'),
    L(0.5, -6, 'Revolver hammer click and cylinder turn, a crisp metallic mechanism clack'),
    L(1.5, -10, 'Short gunshot echo slapping off concrete walls in a large hall, fading quickly', 40),
  ];
}
function thunderclap(flavor) {
  return [
    L(1.2, 0, 'Massive sub bass shotgun blast boom, a chest-hitting low thump with a short rumble'),
    L(0.8, -1, 'Huge pump shotgun blast, ' + flavor + ', wide and violent, close mic'),
    L(0.7, -5, 'Shotgun pump action rack, a heavy metallic shuck-shuck, close mic'),
    L(2, -9, 'Thunderous shotgun echo rolling across a vast stone arena, fading', 60),
  ];
}

const layers = [
  { id: 'fire-kestrel', layers: kestrel('a snappy explosive crack') },
  { id: 'fire-kestrel-2', layers: kestrel('a slightly darker metallic bang') },
  { id: 'fire-kestrel-3', layers: kestrel('a brighter ringing blast') },
  { id: 'fire-thunderclap', layers: thunderclap('a deep explosive roar') },
  { id: 'fire-thunderclap-2', layers: thunderclap('a crunchy distorted roar') },
  { id: 'fire-thunderclap-3', layers: thunderclap('a booming roar with debris crackle') },
  { id: 'fire-gauntlet', layers: [
    L(0.8, 0, 'Heavy sub bass punch impact, a deep body-blow thump'),
    L(0.6, -2, 'Armored metal gauntlet punch smashing into metal, a hard crunchy clang'),
    L(0.8, -6, 'Hydraulic piston hiss and mechanical servo whir, short burst'),
  ] },
  { id: 'fire-gauntlet-2', layers: [
    L(0.8, 0, 'Heavy sub bass punch impact, a deep body-blow thump, slightly lower'),
    L(0.6, -2, 'Armored metal fist strike with a crackling electric discharge, a hard crunchy hit'),
    L(0.8, -6, 'Pneumatic piston release hiss, short and sharp'),
  ] },
  { id: 'fire-torch', layers: [
    L(1.2, 0, 'Roaring flamethrower burst, a violent rush of fire whoosh'),
    L(0.8, -4, 'Low sub bass ignition thump, a deep whump'),
    L(1, -8, 'Crackling sizzling embers and hot sparks, close mic'),
  ] },
  { id: 'slam', layers: [
    L(1.5, 0, 'Colossal sub bass ground pound impact, a deep earth-shaking boom'),
    L(0.8, -2, 'Heavy metal boots crashing into a steel floor, a huge crunchy smash'),
    L(1.5, -7, 'Concrete debris and gravel scattering after an impact, short shockwave whoosh', 30),
  ] },
  { id: 'slam-2', layers: [
    L(1.5, 0, 'Colossal sub bass ground slam boom, deep and short'),
    L(0.8, -2, 'Armored body smashing into a concrete floor, a crunching cracking smash'),
    L(1.5, -7, 'Shockwave air blast whoosh with rubble rattling', 30),
  ] },
  { id: 'land-hard', layers: [
    L(1, 0, 'Deep sub bass landing thud, a heavy low impact'),
    L(0.6, -3, 'Heavy boots landing hard on a metal grate, a crunchy clank'),
    L(0.8, -9, 'Short dust puff and grit scatter'),
  ] },
  { id: 'dash', layers: [
    L(0.7, 0, 'Fast powerful air dash whoosh, a sharp rushing swish'),
    L(0.5, -4, 'Short low sub bass thump kick at the start of a burst'),
    L(0.6, -8, 'Sci-fi jet thruster burst, a crisp short hiss'),
  ] },
  { id: 'dash-2', layers: [
    L(0.7, 0, 'Fast powerful air dash whoosh, a tight swish that pans quickly'),
    L(0.5, -4, 'Short low sub bass thump kick'),
    L(0.6, -8, 'Electric energy zap burst, short and crisp'),
  ] },
  { id: 'dash-3', layers: [
    L(0.7, 0, 'Fast heavy air dash whoosh, a rushing wind swipe'),
    L(0.5, -4, 'Short low sub bass thump kick'),
    L(0.6, -8, 'Compressed air thruster blast, short hiss'),
  ] },
  { id: 'husk-death', layers: [
    L(1, 0, 'Wet heavy body explosion, a gory meaty splatter burst with a deep thump'),
    L(1.2, -3, 'Monster death screech cut short, a raspy distorted shriek'),
    L(1, -7, 'Bones and armor plates clattering onto a metal floor', 50),
  ] },
];

const music = [
  { id: 'foundry-combat-c', state: 'combat', level: 'foundry', seconds: 120, prompt: 'Instrumental breakcore, D minor, 178 bpm, a third fight theme for a molten foundry: a distorted industrial intro of hammering metal and hissing steam, then explodes into frantic chopped amen breaks with glitch stutters and reversed snares, a fat growling reese bass, a screaming detuned supersaw hook, emotional piano chords under the drops, a half-time breakdown that slams back to full speed. Furious, emotional, loud crisp mix, no vocals, loopable.' },
  { id: 'iceline-combat-c', state: 'combat', level: 'iceline', seconds: 120, prompt: 'Instrumental breakcore, F sharp minor, 172 bpm, a third fight theme in a frozen rail yard: icy granular pads and a fast music-box melody, then rapid chopped amen breaks with stutter edits and drill snare rolls, a deep rolling sub bass, glassy bell arpeggios, a bright melancholic supersaw lead, a glitch freeze before each drop. Cold, fast, emotional, crisp high-fidelity mix, no vocals, loopable.' },
  { id: 'kiln-combat-c', state: 'combat', level: 'kiln', seconds: 120, prompt: 'Instrumental breakcore, C minor, 182 bpm, a third fight theme in scorching kiln halls: thundering taiko and distorted 808 kicks, furious chopped amen breaks with glitch edits, a snarling reese bass, low brass stabs, a wailing distorted guitar lead, a half-time drop that crushes. Hot, aggressive, relentless, loud crisp mix, no vocals, loopable.' },
  { id: 'span-combat-c', state: 'combat', level: 'span', seconds: 120, prompt: 'Instrumental breakcore, A minor, 172 bpm, a third fight theme on a vast bridge at dusk: a fast harpsichord and violin ostinato, then intricate chopped amen breaks with stutter edits, ticking clockwork percussion, a wobbling reese bass, a soaring supersaw counter-melody, choir swells into each drop. Elegant, frantic, emotional, no vocals, loopable.' },
  { id: 'spire-combat-c', state: 'combat', level: 'spire', seconds: 120, prompt: 'Instrumental breakcore, E minor, 178 bpm, a third fight theme for the final bell tower: tolling church bells and a pipe organ riff, then the most intense chopped amen breaks with glitch stutters and rapid fills, a massive reese bass, a desperate soaring supersaw lead with choir stabs, a piano interlude that rebuilds into a huge final drop. Grand, desperate, loud crisp mix, no vocals, loopable.' },
];

function addTo(file, items, key) {
  const p = path.join(dir, file);
  const json = JSON.parse(fs.readFileSync(p, 'utf8'));
  const arr = Array.isArray(json) ? json : json[key];
  let added = 0;
  for (const it of items) {
    const at = arr.findIndex(x => x.id === it.id);
    if (at >= 0) continue;
    arr.push(it); added++;
  }
  fs.writeFileSync(p, JSON.stringify(json, null, 2) + '\n');
  console.log(file + ': added ' + added + ' of ' + items.length);
}

addTo('layers.json', layers, 'clips');
addTo('music.json', music, 'tracks');
