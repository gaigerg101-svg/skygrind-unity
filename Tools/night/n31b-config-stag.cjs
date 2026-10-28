// NIGHT31 builder B: THE STAG's numbers under design.n31.stag31 (DESIGN31 C2 + R1 + R7). Every number is Unity-only (the web has no Stag) and carries its _why.
// `enabled` stays false (the lead flips it). Idempotent: the block is replaced from its "stag31": { line to its closing "      },".
// Run: node Tools/night/n31b-config-stag.cjs
const fs = require('fs');
const path = require('path');
const file = path.join(__dirname, '..', '..', 'Design', 'unity-config.json');
let text = fs.readFileSync(file, 'utf8');
const start = text.indexOf('      "stag31": {');
if (start < 0) throw new Error('no design.n31.stag31');
const end = text.indexOf('\n      },', start);
if (end < 0) throw new Error('no end of stag31');

const rows = [
  ['_why', "Item 6b (DESIGN31 C2, D-2, the reviewer's R1 / R7): THE STAG replaces the Glacier as level 2's boss: a blocky ice ram on the glide-ice rink. Still 2 s or farther than farMetres: a CHARGE (a tellSeconds red-orange line on the ice) that does NOT stop at walls: a wall slam stuns it wallStunSeconds with its back open; a stompRadius stomp where it stops; a missed charge skids and turns its OPEN BACK to the player for openSeconds (a gauntlet punch there lands in full, everything else gunScale); two cyan ANTLER CRYSTALS: a charged KESTREL shot shatters one (a crystalStunSeconds stun, regrows after crystalRegrowSeconds: the 15 s crowd-control rule); ice shards lead a far, still or off-ice player; at half health the banks melt to open water (design.n30.lava's rules) and charges come in pairs. Unity-only numbers (the web has no Stag); each has its _why. Builder B."],
  ['enabled', false],
  ['name', '"THE STAG"', "the boss bar's name"],
  ['pool', 1800, 'measured: with the 1.5 x body the pilot killed 1800 in 76.6 s and 2100 in 164.8 s (one back punch more or less swings it: the kill time is set by punch windows), so 1800 stays; (camp31 stag): a PULSE back punch lands 192 (the web gauntlet 32 x design.n27.pulse27.damageMult 6) and the human-lag pilot lands about one per charge cycle; 1400 died in 71 s, so 1800 aims at the 80-130 s band (DESIGN31 D-5)'],
  ['introHold', 2.0, 'seconds of the rise before it can be hurt (the boss bar fills; the other bosses hold 2-2.5 s)'],
  ['dieTime', 1.5, 'seconds of the death fall before the room clears'],
  ['halfW', 1.6, 'half width of the body box (m): a 3.2 m wide ram; the n31-r2 frame the lead read: at 2.2 m it read as a fridge at 25 m, so 1.5 x'],
  ['halfL', 3.0, 'half length of the body box (m): 6 m nose to tail (1.5 x the first pass), the back a clear target 3 m behind the centre'],
  ['halfH', 1.6, 'half height of the body box (m): the shoulders at 3.2 m (1.33 x); a punch from the ice still reaches the back (eye 1.6 m + gauntlet range 2.4)'],
  ['walkSpeed', 3.5, 'm/s it walks toward the player between charges (slower than a walking player, 6.5: it never catches you without a charge)'],
  ['turnRate', 2.5, 'rad/s it turns while walking or in the tell (a sidestep during the tell is not followed: the line locks at the tell\'s start)'],
  ['stillSpeed', 1.0, 'm/s under which the player counts as still (C2.1: a camper)'],
  ['stillSeconds', 2.0, 'seconds of stillness that trigger a charge (C2.1, guess)'],
  ['farMetres', 22, 'm beyond which the player triggers a charge (C2.1, guess: the far plinker)'],
  ['chargeFirstSeconds', 3.0, 'seconds after the intro before the first charge may start'],
  ['chargeEverySeconds', 6.0, 'seconds after which it charges a moving, near player anyway (a fight with no camper still sees a charge every 6 s)'],
  ['chargeGapSeconds', 1.5, 'seconds after a charge recovers before the next may start'],
  ['tellSeconds', 0.9, 'the charge tell (C2.1): head down, the red-orange line on the ice, locked at the tell\'s start'],
  ['chargeSpeed', 22, 'm/s of the charge (a sprint is about 9 m/s: a dodge must go sideways, not away)'],
  ['chargeMaxMetres', 30, 'm of a charge that meets no wall; then it skids (a miss)'],
  ['chargeHalfWidth', 1.5, 'm either side of the line the charge hits (C2.2: 3 m wide)'],
  ['chargeDamageFrac', 0.3, 'fraction of the player\'s max health a charge hit costs (C2.1, guess)'],
  ['chargeKnock', 14, 'm/s the charge throws the player along its line'],
  ['chargeKnockUp', 6, 'm/s up with the charge hit'],
  ['skidSeconds', 0.5, 'seconds of the skid after a missed charge (it slows to a stop)'],
  ['openSeconds', 2.5, 'seconds its OPEN BACK faces the player after a missed charge (C2.3, guess)'],
  ['wallStunSeconds', 2.0, 'seconds of the stun after a wall slam, back open (R1)'],
  ['stompRadius', 4, 'm of the stomp where the charge stops, wall or not (R1: the corner camper is hit by the charge or the stomp)'],
  ['stompDamageFrac', 0.2, 'fraction of max health the stomp costs'],
  ['stompKnock', 10, 'm/s the stomp throws the player outward'],
  ['stompKnockUp', 7, 'm/s up with the stomp'],
  ['gunScale', 0.05, 'what lands from any hit but a gauntlet punch on the open back. DESIGN31 C2.3 guessed 15 %; measured: at 15 % a camper plinking the KESTREL dealt 1.28 %/s against the pilot\'s 1.41 %/s (R2 needs under a third), so guns are a 5 % chip and the back punch is the fight'],
  ['crystalRadius', 0.7, 'm of each antler crystal hit sphere (the n31-r2 frame: larger, so a charged shot at 20 m has a target)'],
  ['crystalUp', 0.9, 'm above the head top the crystals sit (the antlers)'],
  ['crystalSide', 0.9, 'm either side of the centre line'],
  ['headForward', 3.4, 'm from the centre to the head (in front of the body box: head and neck lower than the shoulders, the charging ram the n31-r2 frame asked for); the crystals and the shards sit on it'],
  ['headHeight', 1.5, 'm above the ice of the head centre (the shoulders are at 2 x halfH = 3.2 m: the head hangs lower)'],
  ['headHalf', 0.6, 'm half size of the head box (the view draws it; the crystals sit headHalf + crystalUp above its centre)'],
  ['crystalMinDamage', 60, 'a crystal shatters only to a hit of at least this much (a charged KESTREL shot is 4 x the web revolver\'s 25 = 100; a plain shot 25 only sparks), or a weapon id that names the charge'],
  ['crystalStunSeconds', 1.5, 'the stun when a crystal shatters (C2.3, guess); back open'],
  ['crystalRegrowSeconds', 15, 'a shattered crystal regrows after this (R7: crowd control 15 s or longer)'],
  ['shardFarMetres', 16, 'm beyond which shards fly at the player (also at a player off the floor it can reach, or still)'],
  ['shardEverySeconds', 2.5, 'seconds between shard volleys while the rule holds'],
  ['shardCount', 3, 'shards per volley (one aimed with the lead, two spread)'],
  ['shardSpread', 1.6, 'm the side shards are aimed off the led point'],
  ['shardSpeed', 30, 'm/s of a shard'],
  ['shardDamage', 9, 'damage per shard hit (a still player takes one of three: about 9 per 2.5 s)'],
  ['shardRadius', 0.7, 'm a shard\'s hit radius around the player\'s chest'],
  ['shardLife', 2.0, 'seconds a shard flies before it melts'],
  ['halfAt', 0.5, 'the bar fraction where the banks melt and charges come in pairs (C2.5)'],
  ['pairGapSeconds', 0.35, 'seconds between the two charges of a pair'],
  ['rink', null, null],
];

const rink = [
  ['_why', "THE STAG's arena: the NIGHT30 Glacier rink (design.n30.glacier30.rink) with the banks lowered to low ice ramps (bankDeg), so every spot is ice the Stag reaches or in shard range (C2). Unity-only; read by Rink28Params.From(cfg, \"design.n31.stag31.rink.\")"],
  ['straight', 26], ['islandRadius', 6], ['islandHeight', 3.5], ['laneWidth', 11], ['centreRadius', 11.5], ['bankRun', 4.6], ['doorWidth', 5], ['rimHeight', 8],
  ['bankDeg', 12, 'the banks as low ramps (the Glacier\'s 28 degrees made a safe slope: GG\'s camp spot): 4.6 m of run rises about 1 m'],
];

const I = '        ';
const lines = ['      "stag31": {'];
for (const [k, v, why] of rows) {
  if (k === 'rink') {
    const rl = [I + '"rink": {'];
    for (const [rk, rv, rwhy] of rink) {
      if (rwhy) rl.push(I + '  "' + rk + '_why": ' + JSON.stringify(rwhy) + ',');
      rl.push(I + '  "' + rk + '": ' + (typeof rv === 'string' ? JSON.stringify(rv) : rv) + ',');
    }
    rl[rl.length - 1] = rl[rl.length - 1].replace(/,$/, '');
    rl.push(I + '}');
    lines.push(...rl);
    continue;
  }
  if (why) lines.push(I + '"' + k + '_why": ' + JSON.stringify(why) + ',');
  const val = k === '_why' ? JSON.stringify(v) : (typeof v === 'string' ? v : String(v));
  lines.push(I + '"' + k + '": ' + val + ',');
}
lines[lines.length - 1] = lines[lines.length - 1].replace(/,$/, '');
text = text.slice(0, start) + lines.join('\n') + text.slice(end);
JSON.parse(text);
fs.writeFileSync(file, text);
console.log('design.n31.stag31: ' + rows.length + ' rows written (enabled false)');
