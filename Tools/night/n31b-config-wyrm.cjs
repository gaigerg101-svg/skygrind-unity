// NIGHT31 builder B: THE KILN WYRM's NIGHT31 numbers under design.n31.wyrm31 (DESIGN31 C3 + R5). Every number is Unity-only (the web's worm has no feint, weapon rule or still-spit)
// and carries its _why. `enabled` stays false (the lead flips it). Idempotent: the block is replaced from its "wyrm31": { line to its closing "      },".
// Run: node Tools/night/n31b-config-wyrm.cjs
const fs = require('fs');
const path = require('path');
const file = path.join(__dirname, '..', '..', 'Design', 'unity-config.json');
let text = fs.readFileSync(file, 'utf8');
const start = text.indexOf('      "wyrm31": {');
if (start < 0) throw new Error('no design.n31.wyrm31');
const end = text.indexOf('\n      },', start);
if (end < 0) throw new Error('no end of wyrm31');
// the lead's switch is kept as the file has it (it flips rows on for builds)
const cur = /"enabled": (true|false)/.exec(text.slice(start, end));
const keepEnabled = cur ? cur[1] === 'true' : false;

const rows = [
  ['_why', "Item 6c (DESIGN31 C3, the reviewer's R5): THE KILN WYRM cannot be camped. One aggro charge in feintEvery is a FEINT (stops feintShort m short, no cyan head, then a tail sweep: jump it); guns land gunScale unless the head is STAGGERED (staggerSeconds after a head punch), and then only the THUNDERCLAP lands in full (its slug slugMult x); a player still for stillSeconds meets a led spit volley at any range. Unity-only numbers (the web's worm has none of these); each has its _why. Builder B. Cut: the platforms crumbling at half health."],
  ['enabled', keepEnabled],
  ['gunScale', 0.03, 'what a gun lands (after the part multiplier) unless the head is staggered and the gun is the THUNDERCLAP; the PULSE punch lands in full only while staggered (the punch that staggers lands its bonus through the stagger, design.n30.wyrm30.bonusMult). DESIGN31 C3.3 guessed 20 %; measured (camp31 wyrm31): at 20 % the centre camper plinking the head dealt 0.95 %/s against the pilot\'s 0.65; at 5 % still 0.24 against 0.49 (the head is x2), so a 3 % chip'],
  ['staggerSeconds', 2.5, 'seconds after a head punch (wyrmStagger) in which the THUNDERCLAP lands in full (C3.3, guess)'],
  ['slugMult', 2, 'the THUNDERCLAP slug (builder W\'s charged shot: a weapon id naming "slug") on a staggered head: 2 x (C3.3 / R5: "the slug double")'],
  ['feintEvery', 3, 'every third aggro charge is a feint (C3.6: one in three)'],
  ['feintShort', 7, 'm short of the player\'s spot the feint stops (a held punch at the head misses; the sweep follows)'],
  ['sweepTellSeconds', 0.6, 'the tail sweep\'s tell after a feint stops (a jump in it clears the sweep)'],
  ['sweepRadius', 9, 'm around where the feint stopped that the sweep hits a grounded player (feintShort + 2: the player it feinted at is inside)'],
  ['sweepDamage', 20, 'damage of the sweep (the Loom\'s sweep is 20)'],
  ['sweepKnock', 10, 'm/s outward knock of the sweep'],
  ['sweepUp', 6, 'm/s up with the sweep'],
  ['stillSpeed', 1.0, 'm/s under which the player counts as not moving'],
  ['stillSeconds', 3, 'seconds of not moving before the spit (R5: at any range)'],
  ['spitEverySeconds', 3, 'seconds between spit volleys while the player stays still'],
  ['spitCount', 3, 'spits per volley (one led, two spread)'],
  ['spitSpread', 1.6, 'm the side spits are aimed off the led point'],
  ['spitSpeed', 28, 'm/s of a spit'],
  ['spitDamage', 10, 'damage per spit hit (a still camper takes about one in three: 10 per 3 s)'],
  ['spitRadius', 0.7, 'm a spit\'s hit radius around the chest'],
  ['spitLife', 3.0, 'seconds a spit flies (84 m at 28 m/s: the whole arena)'],
  ['aggroCooldown', 6, 'seconds between aggro charges with the row on (design.n26 has 14: GG asked for 6 s, DECIDE26 D-7, eased then for bot deaths); measured: at 14 the punch-and-slug pilot met about one real charge per 25 s, so the stagger window came too rarely for the moving player to out-deal a camper (R2)'],
  ['crumbleAt', 0.5, 'the bar fraction where the platforms crumble (C3.5: half health)'],
  ['crumbleTellSeconds', 1.0, 'the crumble tell (red-orange cracks) before the platforms fall away (C3.5 / builder L k5.crumble31: 1 s)'],
  ['crumbleBoxes', [23, 24], 'indices into the worm arena file collision.boxes that crumble when the level row k5.crumble31 is not there (the survival map, the harness): builder L row values (the two higher platforms, tops 9 and 12; 22 stays)'],
];

const I = '        ';
const lines = ['      "wyrm31": {'];
for (const [k, v, why] of rows) {
  if (why) lines.push(I + '"' + k + '_why": ' + JSON.stringify(why) + ',');
  const val = k === '_why' ? JSON.stringify(v) : (Array.isArray(v) ? JSON.stringify(v) : String(v));
  lines.push(I + '"' + k + '": ' + val + ',');
}
lines[lines.length - 1] = lines[lines.length - 1].replace(/,$/, '');
text = text.slice(0, start) + lines.join('\n') + text.slice(end);
JSON.parse(text);
fs.writeFileSync(file, text);
console.log('design.n31.wyrm31: ' + rows.length + ' rows written (enabled false)');
