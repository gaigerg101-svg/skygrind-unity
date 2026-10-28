// NIGHT42 builder K1: patch ONLY design.n42.arena42 in Design/unity-config.json (text-level: the row's lines after its enabled_why), then copy the
// file byte for byte to Assets/StreamingAssets/Design/unity-config.json. Idempotent: the row's numbers are replaced if present.
// Run from the worktree root: node Tools/night/k1-arena42-row.cjs
const fs = require('fs');
const src = 'Design/unity-config.json', dst = 'Assets/StreamingAssets/Design/unity-config.json';
let text = fs.readFileSync(src, 'utf8');
const start = text.indexOf('"arena42": {');
if (start < 0) throw new Error('no arena42 row');
const anchor = text.indexOf('"enabled_why"', start);
const lineEnd = text.indexOf('\n', anchor);
const eol = text[lineEnd - 1] === '\r' ? '\r\n' : '\n';
const close = text.indexOf('}', lineEnd); // the row's own closing brace (no nested object before our insert)
// drop anything this script inserted before (between the enabled_why line and the row's close)
let head = text.slice(0, lineEnd + 1), tail = text.slice(lineEnd + 1);
const mark = '        "grinder": {';
if (tail.startsWith(mark)) {
  const endMark = '        "cover": {';
  const ci = tail.indexOf(endMark);
  const ce = tail.indexOf('        }', ci);
  const after = tail.indexOf('\n', ce);
  tail = tail.slice(after + 1);
}
// the enabled_why line must end with a comma now
head = head.replace(/("enabled_why": "[^"]*")(\r?\n)$/, '$1,$2');
const ind = '        ', ind2 = '          ';
const row = {
  grinder: [
    ['_why', 'THE GRINDER (level JSON `grinders42`: [{rect: [x0, z0, x1, z1], y, room}], Core/Arena42.cs): a pit floor of blades. Any enemy whose feet touch it is ground at once, the player\'s kill (the traps\' path, weapon trap), event arena42Ground; went in knocked or in the air -> style GROUND. ULTRAKILL pays for using the arena as a weapon (https://ultrakill.wiki.gg/wiki/Style: an enemy into a pit, Fall 80). Walkers never walk in (nav mask + edge guard). The web has no grinder.'],
    ['touchMargin', 0.05], ['touchMargin_why', 'The feet count as touching at or below the grinder floor y + this (Lava30\'s touchMargin, design.n30.lava: a body standing on the floor always reads as in it).'],
    ['playerDamageFraction', 0.2], ['playerDamageFraction_why', 'The player who falls in loses this share of max health per touch (lava: 0.25; the grinder sits in a pit you climb out of, so a touch costs a little less).'],
    ['knockUp', 9], ['knockUp_why', 'The player is knocked up this many m/s on a touch (lava\'s knockUp 9: the same hop out, Mover.Launch).'],
    ['graceSeconds', 0.5], ['graceSeconds_why', 'No second hurt for this long after a touch (lava\'s graceSeconds).'],
    ['groundStyle', 80], ['groundStyle_why', 'The style bonus GROUND for a body that went in knocked or in the air: ULTRAKILL\'s Fall / Minced / Shredded are 80 each (https://ultrakill.wiki.gg/wiki/Style, arenas-web.md).'],
    ['maskAbove', 1.5], ['maskAbove_why', 'A nav node whose column is in a grinder rect and stands at most this many m over its floor is closed to walkers (a walkway higher up stays open; ai33\'s lava mask works the same).'],
    ['guardMargin', 0.6], ['guardMargin_why', 'The edge guard\'s band round the rect, m: a walker the player did not knock that enters it is put back (a husk\'s radius is 0.4, so it never hangs over the rim).'],
    ['guardAbove', 6], ['guardAbove_why', 'The edge guard acts up to this many m over the grinder floor (a 4 m pit\'s rim and a step over it); a walkway higher up is not guarded.'],
  ],
  cover: [
    ['_why', 'BREAKABLE COVER (level JSON `cover42`: [{min: [x, y, z], max: [x, y, z], hits, room}], Core/Arena42.cs): a solid box in the world (the sim layer arena42Cover). Each player shot that ends on it takes a hit; at 0 it shatters (event arena42CoverBreak): the shards hit every enemy within burstRadius and push them away; shards that hit a body pay SHATTER. ULTRAKILL 0-1 / 4-2 glass floors and walkways that break (arenas-web.md). The web has no breakable cover.'],
    ['hits', 6], ['hits_why', 'Shots a box takes when its level row gives no `hits` (about 1.5 s of KESTREL fire: a fight\'s cover, gone if you camp behind it shooting).'],
    ['burstRadius', 4], ['burstRadius_why', 'The shatter hits every enemy whose aim point is within this many m of the box centre (a crowd at the cover, not the room).'],
    ['burstDamage', 60], ['burstDamage_why', 'The shards\' damage per body: more than a husk\'s 40 hp (PortExport husk.hp), so a shatter kills the husks at it and hurts the heavy kinds.'],
    ['burstPush', 9], ['burstPush_why', 'The shards push a surviving body away from the box at this many m/s (Enemy.Knockback): toward a grinder when the level puts one behind the cover.'],
    ['burstPushSeconds', 0.3], ['burstPushSeconds_why', 'The push lasts this long (whole steps): about 2.7 m.'],
    ['shatterStyle', 60], ['shatterStyle_why', 'The style bonus SHATTER when the shards hit at least one body (under GROUND\'s 80: the arena as a weapon, the smaller trick).'],
  ],
};
function block(name, pairs, last) {
  let s = ind + JSON.stringify(name) + ': {' + eol;
  pairs.forEach(([k, v], i) => { s += ind2 + JSON.stringify(k) + ': ' + JSON.stringify(v) + (i < pairs.length - 1 ? ',' : '') + eol; });
  return s + ind + '}' + (last ? '' : ',') + eol;
}
const insert = block('grinder', row.grinder, false) + block('cover', row.cover, true);
text = head + insert + tail;
JSON.parse(text); // still valid
fs.writeFileSync(src, text);
fs.copyFileSync(src, dst);
const c = JSON.parse(text).design.n42.arena42;
console.log('arena42 row:', Object.keys(c).join(', '), '| grinder keys', Object.keys(c.grinder).length, '| cover keys', Object.keys(c.cover).length);
