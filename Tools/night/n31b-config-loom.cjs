// NIGHT31 builder B: THE LOOM's NIGHT31 numbers under design.n31.loom31 (DESIGN31 C4 + R4). Every number is Unity-only (the web has no Loom) and carries its _why.
// The lead's `enabled` switch is kept as the file has it. Idempotent: the block is replaced from its "loom31": { line to its closing "      },".
// Run: node Tools/night/n31b-config-loom.cjs
const fs = require('fs');
const path = require('path');
const file = path.join(__dirname, '..', '..', 'Design', 'unity-config.json');
let text = fs.readFileSync(file, 'utf8');
const start = text.indexOf('      "loom31": {');
if (start < 0) throw new Error('no design.n31.loom31');
const end = text.indexOf('\n      },', start);
if (end < 0) throw new Error('no end of loom31');
const cur = /"enabled": (true|false)/.exec(text.slice(start, end));
const keepEnabled = cur ? cur[1] === 'true' : false;

const rows = [
  ['_why', "Item 6d (DESIGN31 C4, the reviewer's R4): THE LOOM rebuilt. Nails land in full on a joint only within fullReach or roped to that leg (20 % from the deck: design.n30.span.loom.lightScale); the hips sit hipUp above the deck and hipOut past the corner (pull-in targets in sight: G's pick31 found the hook took the deck surface under them); a player hanging longer than hangSeconds is thrown off by a joint shake; a player on the deck deckSeconds calls a flyer drop; at breakAt the deck's middle (builder L's s8.deckBreak31) cracks and opens to lava. Unity-only numbers; each has its _why. Builder B."],
  ['enabled', keepEnabled],
  ['fullReach', 6, 'm from a joint within which a nail lands in full (R4: 6 m; NIGHT30 had 3)'],
  ['hipOut', 6, 'm past the deck corner the hip joint sits (NIGHT30: 2 m, under the deck): a camper 1 m inside the corner is 7 m off, past fullReach'],
  ['hipUp', 1.5, 'm above the deck the hip joint sits (NIGHT30: 2.5 m under it, where the deck hid it from the hook)'],
  ['hangSeconds', 3, 'seconds of hanging on the Loom before a joint shakes (R4: longer than 3 s)'],
  ['shakeTellSeconds', 0.5, 'the shake tell (the joint rattles red-orange; let go or zip off in it)'],
  ['shakeDropSpeed', 8, 'm/s down when the shake throws the player off (the CUT\'s drop is design.n30.span.loom.cutDropSpeed)'],
  ['deckSeconds', 3, 'seconds standing on the deck (or any ground in the room: camp31 found the entry ledge safe) before a flyer drop is called (C4, R4: in, not cut)'],
  ['dropTellSeconds', 1.0, 'the drop tell (a red-orange mark at the spot)'],
  ['dropRadius', 2.5, 'm around the mark the drop hits'],
  ['dropDamage', 25, 'damage of the drop (a still deck camper takes 25 per drop)'],
  ['dropKnock', 8, 'm/s outward knock of the drop'],
  ['dropUp', 6, 'm/s up with the drop'],
  ['dropEverySeconds', 2.0, 'seconds after a drop before the deck timer can call the next'],
  ['otherScale', 0.01, 'what any gun but the NAILGUN (and the PULSE punch) lands on the Loom with the row on. Measured (camp31 loom31): a THUNDERCLAP corner camper dealt 3.15 %/s at the 20 % light share and 0.80 %/s at 5 % (the joint counts as a headshot: 5504 raw in 14.6 s) against the lag bot 1.04; at 2 % still 0.33 against a third of the bot median 0.32, so a 1 % chip (the THUNDERCLAP pellets read the joints as headshots)'],
  ['nailLightScale', 0.12, 'what a nail lands on a joint outside fullReach and off its line, and on the body, with the row on. R4 said 20 % from the deck; measured (camp31 loom31, lag bot median about 105 s = 0.95 %/s): at 20 % a deck or entry camper streaming nails dealt 0.33-0.36 %/s, a third of the bot, so 12 % (R2 needs both: it dies AND deals under a third)'],
  ['breakAt', 0.5, 'the bar fraction where the deck\'s middle breaks (C4: half health)'],
  ['breakTellSeconds', 1.0, 'the crack tell before the middle opens (red-orange cracks across it)'],
  ['breakRect', [-5, -223, 5, -213], 'the deck\'s breakable middle (world x0, z0, x1, z1) when the level row s8.deckBreak31 is not read: builder L\'s rect'],
];

const I = '        ';
const lines = ['      "loom31": {'];
for (const [k, v, why] of rows) {
  if (why) lines.push(I + '"' + k + '_why": ' + JSON.stringify(why) + ',');
  const val = k === '_why' ? JSON.stringify(v) : (Array.isArray(v) ? JSON.stringify(v) : String(v));
  lines.push(I + '"' + k + '": ' + val + ',');
}
lines[lines.length - 1] = lines[lines.length - 1].replace(/,$/, '');
text = text.slice(0, start) + lines.join('\n') + text.slice(end);
JSON.parse(text);
fs.writeFileSync(file, text);
console.log('design.n31.loom31: ' + rows.length + ' rows written (enabled ' + keepEnabled + ', kept)');
