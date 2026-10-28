// NIGHT31 builder B: THE LANTERN's NIGHT31 numbers under design.n31.lantern31 (DESIGN31 C1). Every number is Unity-only (the web's Lantern is NIGHT27's, no shield charge rule)
// and carries its _why. The lead's `enabled` switch is kept as the file has it. Idempotent: the block is replaced from its "lantern31": { line to its closing "      },".
// Run: node Tools/night/n31b-config-lantern.cjs
const fs = require('fs');
const path = require('path');
const file = path.join(__dirname, '..', '..', 'Design', 'unity-config.json');
let text = fs.readFileSync(file, 'utf8');
const start = text.indexOf('      "lantern31": {');
if (start < 0) throw new Error('no design.n31.lantern31');
const end = text.indexOf('\n      },', start);
if (end < 0) throw new Error('no end of lantern31');
const cur = /"enabled": (true|false)/.exec(text.slice(start, end));
const keepEnabled = cur ? cur[1] === 'true' : false;

const rows = [
  ['_why', "Item 6a (DESIGN31 C1): THE LANTERN's shield breaks only to a CHARGED KESTREL shot (weapon revolver, at least chargedMinDamage) fired while wall riding; then the core is open for coreOpenSeconds and every hit lands in full (none from a platform). A plain KESTREL shot from the wall sparks. At sinkAt of the bar the three platforms (builder L's f12.sink31: panels 2, 7, 13) tell for sinkTellSeconds and sink to sinkTo over sinkSeconds. Unity-only numbers; each has its _why. Builder B. The NIGHT30 platform lob (design.n30.lantern30.platformOrbSeconds 2.5) is the C1 'orbs at a player on a platform 3 s' rule already."],
  ['enabled', keepEnabled],
  ['coreOpenSeconds', 4, 'seconds the core stays open after the shield breaks (C1.3, guess)'],
  ['chargedMinDamage', 60, 'a revolver hit of at least this breaks the shield: builder W\'s charged shot is 4 x 25 = 100, a plain shot 25 (C1.3)'],
  ['lightMult', 0.6, 'the light share (NIGHT30 light and floor scale) times this while the shield is up. Measured (camp31 lantern31): at 1 the floor campers dealt 0.37 %/s against a third of the charge-and-ride pilot rate 1.02 = 0.34 (they die in the lava at 20 s, but R2 needs both)'],
  ['rideGraceSeconds', 0.6, 'a shot fired up to this long after a wall ride ends still counts as from the wall (the charge takes 1.0 s to fill and a ride ends at a seam or a jump: the pilot broke the shield 2 times in 240 s without it)'],
  ['sinkAt', 0.5, 'the bar fraction where the platforms sink (C1.5: half health)'],
  ['sinkTellSeconds', 1.0, 'the sink tell (a red-orange edge on the platforms, builder L f12.sink31: 1 s)'],
  ['sinkSeconds', 1.5, 'seconds the platforms take from their top to sinkTo (a rider sees them go; a stander rides down into the lava rules)'],
  ['sinkTo', 0, 'the platforms\' top when sunk, arena-local m (builder L f12.sink31 topTo: flush with the live floor)'],
  ['sinkStepSeconds', 0.25, 'how often the ring\'s collision layer is rebuilt while the platforms sink (6 rebuilds over 1.5 s: the Mover rides each step down)'],
];

const I = '        ';
const lines = ['      "lantern31": {'];
for (const [k, v, why] of rows) {
  if (why) lines.push(I + '"' + k + '_why": ' + JSON.stringify(why) + ',');
  const val = k === '_why' ? JSON.stringify(v) : (Array.isArray(v) ? JSON.stringify(v) : String(v));
  lines.push(I + '"' + k + '": ' + val + ',');
}
lines[lines.length - 1] = lines[lines.length - 1].replace(/,$/, '');
text = text.slice(0, start) + lines.join('\n') + text.slice(end);
JSON.parse(text);
fs.writeFileSync(file, text);
console.log('design.n31.lantern31: ' + rows.length + ' rows written (enabled ' + keepEnabled + ', kept)');
