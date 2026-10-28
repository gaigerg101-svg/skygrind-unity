// NIGHT36 builder P: writes ONLY design.n36.tells36 / parry36 / launch36 / heal36's numbers (keeps each row's `enabled` and the lead's _why
// head), then copies the file to Assets/StreamingAssets/Design/unity-config.json. Run from the repo root: node Tools/night/n36-core-config.cjs
const fs = require('fs');
const path = require('path');
const src = path.join('Design', 'unity-config.json');
const dst = path.join('Assets', 'StreamingAssets', 'Design', 'unity-config.json');
const raw = fs.readFileSync(src, 'utf8');
const cfg = JSON.parse(raw);
const n36 = cfg.design.n36;
const rows = {
  tells36: {
    huskWindupS: 1.0,
    spitterTellS: 1.0,
    swarmerTellS: 0.6,
    droneChargeS: 1.0,
    chargerTellS: 1.2,
    _whyNumbers: 'P (NIGHT36 tells36): huskWindupS 1.0 (the web 0.5) and spitterTellS 1.0 (the web 0.6): Birkhead "beyond 2 seconds I start counting", design-web.md rule 5, 1-1.5 s for melee and big shots (the low end: these are the commonest attacks); a husk melee and a spitter volley are YELLOW (parryable: the punch in the wind-up, the glob punched back). swarmerTellS 0.6 (the web 0.3): a 10 HP flanker whose pounce is RED (dodge); doubled so the red tell is seen, kept under 1 s because a slow swarmer stops being a threat at all. droneChargeS 1.0 (the web 0.4; its fireInterval 2 s is kept, so one orb every 2 s): a big shot, YELLOW (the orb is parried back at the drone). chargerTellS 1.2 (the web 0.8): the middle of the 1-1.5 s band for the biggest body (150 hp at 22 m/s), RED (dodge). The LANCER keeps the web aimTime 1.2 s (already in the band) and is RED: a hitscan beam from a far perch no punch reaches. The web numbers come back with the row off.',
  },
  parry36: {
    bufferS: 0.12,
    freezeS: 0.25,
    returnBurstDamage: 40,
    returnBurstRadius: 3,
    _whyNumbers: 'P (NIGHT36 parry36): bufferS 0.12 = ULTRAKILL parryFramesLeft 6 x 0.02 s (Enemy.cs:2698-2842, Punch.cs:524; rea-ultrakill-combat.md section 3). freezeS 0.25 = ULTRAKILL parry TrueStop 0.25 (PLAN.md table; design.ultra.parryHitstop is the same 0.25). returnBurstDamage 40 = one husk (husk.hp 40) or two thirds of a spitter (spitter.hp 60): the returned glob (orb.reflectDamageMult x globDamage = 24 on the shooter) plus the burst kills the spitter that threw it; returnBurstRadius 3 = the spitter puddleRadius (3 m), the size the player already reads as the acid splash.',
  },
  launch36: {
    speed: 16,
    lift: 5,
    flightS: 0.8,
    graceS: 0.15,
    armS: 0.05,
    burstDamage: 60,
    burstRadius: 3.5,
    hitPad: 0.3,
    wallProbe: 0.35,
    wallMaxNormalY: 0.5,
    wallProbeLift: 0.25,
    _whyNumbers: 'P (NIGHT36 launch36): speed 16 m/s x flightS 0.8 s = a ~12 m throw, the length of one n36 room (12-24 m, PLAN.md section 3), so a launch reaches a wall or another body in the same room; lift 5 m/s keeps it off the floor for ~0.5 s at the sim gravity (a visible arc, not a slide). graceS 0.15: a punch just after a dash / slide ends still counts (the dash is short; ULTRAKILL keeps slide momentum). armS 0.05: the flier ignores walls for its first 6 steps (it starts next to the player, maybe on a wall-ride wall). burstDamage 60 = the spitter hp (60) and over the husk hp (40): "a launched husk into a husk = two kills" (NIGHT36.md P1 check); burstRadius 3.5 m = a crowd of 2-3 at husk.separationRadius spacing. hitPad 0.3 m: bodies touching or about to (one step at 16 m/s is 0.13 m). wallProbe 0.35 m ahead and |n.y| < 0.5 = a face steeper than 60 degrees is a wall; wallProbeLift 0.25 m: the probe capsule is raised so a body sliding on the floor reads the wall, not the floor (the floor contact bent the aggregated normal up).',
  },
  heal36: {
    radius: 4,
    amount: 10,
    _whyNumbers: 'P (NIGHT36 heal36; DECIDE36 D-5): radius 4 m and amount 10 HP = PLAN.md section 1 / NIGHT36.md P1 starting numbers ("a kill within ~4 m spills health ... start 10 HP, radius 4 m"); ULTRAKILL heals by touching the blood for 0.25 s (Bloodsplatter.cs:92-176) with per-gore amounts not checked; 10 of 100 is a husk hit (husk.meleeDamage) won back by one close kill.',
  },
};
// Text patch: each row's block (6-space indent inside design.n36) is replaced in place; the rest of the file keeps its bytes.
const eol = raw.includes('\r\n') ? '\r\n' : '\n';
let out = raw;
for (const [k, v] of Object.entries(rows)) {
  if (!n36[k]) throw new Error('no design.n36.' + k);
  const row = Object.assign({ _why: n36[k]._why, enabled: n36[k].enabled }, v);
  const open = '      "' + k + '": {' + eol;
  const at = out.indexOf(open);
  if (at < 0) throw new Error('row text not found: ' + k);
  const close = out.indexOf(eol + '      }', at);
  if (close < 0) throw new Error('row end not found: ' + k);
  const end = close + (eol + '      }').length;
  const text = '      "' + k + '": ' + JSON.stringify(row, null, 2).split('\n').map((l, i) => (i === 0 ? l : '      ' + l)).join(eol);
  out = out.slice(0, at) + text + out.slice(end);
}
JSON.parse(out);
fs.writeFileSync(src, out);
fs.copyFileSync(src, dst);
console.log('patched design.n36:', Object.keys(rows).join(', '));
