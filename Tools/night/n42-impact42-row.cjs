// NIGHT42 builder F1: writes design.n42.impact42's numbers (each with a _why) into Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json (the two stay byte-identical). Patches only the impact42 row: the lines after its
// enabled_why, by text (the file is not JSON.stringify's layout, so it is never re-serialised). Idempotent: a second run replaces the
// numbers it wrote. Run from the repo root: node Tools/night/n42-impact42-row.cjs
const fs = require('fs');
const path = require('path');

const src = path.join('Design', 'unity-config.json');
const dst = path.join('Assets', 'StreamingAssets', 'Design', 'unity-config.json');

const rows = [
  ['gauntletHitS', 0.16, "G15 ('a greater or mor epowerful effect when hitting it or any other enemy iwth gaughtlent'): a gauntlet hit on any body freezes 0.16 s, above heavy41's 0.14 s big-kind freeze and crunch36's 0.1 s punch (ULTRAKILL's punch). hitfeel-web.md number 4: 0.05-0.08 s on a strong or melee hit, 0.10-0.15 s on a finisher; the PULSE swing is the game's finisher-weight blow (one swing per 1.8-2.5 s), so it sits just above that band and under ULTRAKILL's major hitstop 0.25 s (number 1), which stays the parry's."],
  ['gauntletBigS', 0.2, "A gauntlet hit on a big kind (design.n41.heavy41.kinds: the TANK, the WARDEN, the CHARGER) freezes 0.2 s: the weight GG asked the gauntlet to have against large enemies (w40). hitfeel-web.md section 1: fighting games stop 6-10 frames (0.10-0.17 s) on a heavy attack and 12-18 frames (0.20-0.30 s) on a boss super; this is the bottom of the super band."],
  ['gauntletBossS', 0.24, "A gauntlet hit on a boss body (THE ICEBREAKER and every Loadout.IsBoss34 body) freezes 0.24 s, the biggest hit in the game (NIGHT42.md F1: 'THE ICEBREAKER's hit the biggest'), just under ULTRAKILL's major hitstop 0.25 s (hitfeel-web.md number 1) and the perfect parry's 0.3 s, so a parry still reads as the top moment."],
  ['gauntletKillS', 0.22, "A gauntlet kill freezes at least 0.22 s (a boss hit's 0.24 still wins): longer than a plain gauntlet hit, the finisher (hitfeel-web.md number 4: 0.10-0.15 s on a finisher, raised for the gauntlet's slow cycle; number 10: DOOM keeps its finishers to hundreds of ms)."],
  ['gauntletCooldownS', 0.4, "The shortest real time between two gauntlet freezes: the web's gauntlet cycle 0.42 s (PortExport/config.json weapons.gauntlet), so a faster swing rule (a mod, a shorter pulse27) can never chain freezes into a stall."],
  ['gauntletPerMinute', 0.08, "The gauntlet's freezes take at most this share of any rolling minute (4.8 s), their own budget beside design.impact.budget.perMinute 0.06 (the shared one: kills, headshots, punches); over it a freeze is cut to what is left. A boss fight landing every 1.8 s swing on the boss would ask 33 x 0.24 = 7.9 s a minute; the share holds it to 8 % of real time (SimHarness n42hit measures it)."],
  ['heavyHitS', 0.06, "A heavy gun hit (a rocket's direct hit, a rail hit, a THUNDERCLAP shot landing design.impact.freeze.heavyPellets pellets in one body) freezes 0.06 s (was 0.03): hitfeel-web.md number 4's strong-hit band 0.05-0.08 s."],
  ['killS', 0.085, "A gun kill freezes 0.085 s (was 0.058): GG's G9 'hits and kills need to feel more satifying'; hitfeel-web.md number 4: a finisher 0.10-0.15 s; the body kill is the most common moment, so it sits under that band, 10 sim steps, and two still fit design.impact.budget.perSecond 0.2."],
  ['headshotKillS', 0.12, "A headshot kill freezes 0.12 s (was 0.096): the finisher band 0.10-0.15 s (hitfeel-web.md number 4)."],
  ['punchKillS', 0.13, "A punch kill freezes 0.13 s (was 0.108): a melee finisher, the band's middle (hitfeel-web.md number 4); the gauntlet's kill is gauntletKillS."],
  ['killCapS', 0.18, "A step with N kills freezes killS + design.impact.freeze.killPerExtra x (N - 1), at most 0.18 s (was 0.155): a THUNDERCLAP blast through 5 husks freezes 0.18 s; under design.impact.budget.perSecond 0.2, so it is never cut alone."],
];

const byWeapon = [
  ['revolver', 0.024, "The KESTREL's body hit freezes 0.024 s (crunch36: 0.018): hitfeel-web.md number 3, Nuclear Throne's 10-20 ms on an ordinary bullet hit, a step above for the slow, heavy pistol (one shot per 0.4 s): about 3 sim steps."],
  ['shotgun', 0.036, "A THUNDERCLAP volley's body hit (under heavyPellets pellets) freezes 0.036 s: between the ordinary bullet (number 3) and the strong-hit band 0.05-0.08 s (number 4); a full volley in one body is heavyHitS."],
  ['nailgun', 0.012, "A STITCHER nail's body hit freezes 0.012 s: the bottom of Nuclear Throne's 10-20 ms (number 3), because it fires about 10 nails a second (crunch36's bodyHitCooldownS 0.15 and bodyBudgetPerSecond 0.1 still cap the stream)."],
];

const ind = '        ';
const esc = (s) => JSON.stringify(s);
const lines = [];
for (const [k, v, why] of rows) {
  lines.push(`${ind}${esc(k)}: ${v},`);
  lines.push(`${ind}${esc(k + '_why')}: ${esc(why)},`);
}
lines.push(`${ind}"bodyHitByWeapon": {`);
lines.push(`${ind}  "_why": ${esc("A gun's non-kill body hit freezes by weapon (the weapon id of the hit event), inside crunch36's body budget (bodyHitCooldownS, bodyBudgetPerSecond); a weapon not listed keeps design.n36.crunch36.bodyHitS. Each value's reason is its _why key.")},`);
byWeapon.forEach(([k, v, why], i) => {
  lines.push(`${ind}  ${esc(k)}: ${v},`);
  lines.push(`${ind}  ${esc(k + '_why')}: ${esc(why)}${i === byWeapon.length - 1 ? '' : ','}`);
});
lines.push(`${ind}}`);

const text = fs.readFileSync(src, 'utf8');
const nl = text.includes('\r\n') ? '\r\n' : '\n';
const all = text.split(nl);
const start = all.findIndex((l) => l === '      "impact42": {');
if (start < 0) throw new Error('impact42 row not found');
let end = start + 1;
while (all[end] !== '      },' && all[end] !== '      }') end++;
// keep the row's own first lines (_why, enabled, enabled_why), drop anything this script wrote before
let keep = start + 1;
const own = [];
while (keep < end) {
  const l = all[keep];
  if (/^ {8}"(_why|enabled|enabled_why)":/.test(l)) own.push(l.replace(/,$/, ''));
  keep++;
}
const body = own.map((l) => l + ',').concat(lines);
const out = all.slice(0, start + 1).concat(body, all.slice(end));
fs.writeFileSync(src, out.join(nl));
JSON.parse(fs.readFileSync(src, 'utf8'));
fs.copyFileSync(src, dst);
console.log(`impact42: ${rows.length} numbers + ${byWeapon.length} weapons written; copied to ${dst}`);
