// NIGHT39 builder P1: writes design.n39.p39's numbers (the KESTREL's hold rule, its feel) into Design/unity-config.json by text, keeping the line endings
// and the layout, then copies the file to Assets/StreamingAssets/Design/unity-config.json (the two stay byte-identical). Patches ONLY the p39 block: every
// line after its "enabled" line up to the block's close is replaced by the rows below (re-runnable; `enabled` is left as it is unless --enable / --disable).
// Run from the repo root: node Tools/night/n39p-config-p39.cjs [--enable|--disable]
const fs = require('fs');
const path = 'Design/unity-config.json';
const copy = 'Assets/StreamingAssets/Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const n39 = text.indexOf('"n39": {');
if (n39 < 0) throw new Error('no design.n39 block');
const at = text.indexOf('"p39": {', n39);
if (at < 0) throw new Error('no design.n39.p39 block');
const close = text.indexOf(eol + '      }', at); // the p39 block's closing brace (6 spaces: a row of design.n39)
const m = /"enabled": (true|false)/.exec(text.slice(at));
if (!m || at + m.index > close) throw new Error('no enabled line in p39');
let enabled = m[1];
if (process.argv.includes('--enable')) enabled = 'true';
if (process.argv.includes('--disable')) enabled = 'false';
const I = '        ';
const rows = [
  `"tapS": 0.15`,
  `"tapS_why": "The KESTREL's tap / hold split (whole steps: 18 at 120 Hz): a press released inside this fires the tap ON THE RELEASE; held past it, NO tap fires and the hold is a charge (charge35Start, the HUD's CHARGING; FULL at design.n31.charge.revolver.fullSeconds 0.5 s from the press, as before). In research/feel-web.md section 3's range: Unity's Input System tap 0.2 s, casual players need up to ~0.2 s to choose hold over tap, an average finger is off the button in ~0.1 s; 0.15 s sits above a deliberate click (50-100 ms, design.n35.charge35.tapS_why) and under tap36's holdStartS 0.18 s, so a tap's added latency (the press's own length) is at most 0.15 s and the charge reads 0.35 s of filling."`,
  `"partialFires": true`,
  `"partialFires_why": "A hold released past tapS but before FULL fires the tap on the release (true: the trigger always answers, NIGHT35 charge35's rule) or drains with no shot (false: ULTRAKILL's Piercer winds down without firing, feel-web.md section 3). true: a slow pad trigger that just passes tapS still shoots; the tap after a partial gets no aim correction past correctFadeS."`,
  `"correctFullS": 0.11`,
  `"correctFullS_why": "A release tap fired up to this long after its press leaves from the PRESS's eye along the PRESS's aim, in full (INVERSUS, Ryan Juckett, gamedeveloper.com 'Reducing perceived attack delay in INVERSUS': full correction 0-0.11 s; feel-web.md section 3): the shot goes where the crosshair was when the finger went down, so the release's delay never moves the hit."`,
  `"correctFadeS": 0.19`,
  `"correctFadeS_why": "The correction fades linearly from correctFullS to none at this age (INVERSUS: none by 0.19 s); tapS 0.15 s is inside it, so every quick tap is corrected at least 50 %."`,
  `"intervalMult": 1.2`,
  `"intervalMult_why": "x the KESTREL's normal fire interval while p39 is on (GG: the pistol 'should feel heavier, slower, and more impactful'): the web's 0.4 s (48 steps) becomes 58 steps (0.483 s), taps and the held auto-fire alike; the charged shot and its 2.5 s cooldown unchanged. Tap DPS -17 %: the boss bands (B1) are measured with p39 on. 1.2 is a guess, logged; the heavier look (Pistol39View) carries the rest."`,
  `"hitS": 0.05`,
  `"hitS_why": "A KESTREL tap's hit on a body freezes the frame this long (sim.Hitstop, the web's freeze: the longest request wins, so a kill's own freeze still rules). crunch36's body hit is 15-20 ms (design.n36.crunch36.bodyHitS); ULTRAKILL's revolver beam 0.05 s and punch 0.1 s (REVIEW.md root cause 3). Pistol39View requests it on the hit event (loop pacing: the sim and the tape never see it)."`,
  `"chargedHitS": 0.1`,
  `"chargedHitS_why": "The charged KESTREL shot's (the cannon's) hit freezes this long, once per shot however many bodies it pierces: ULTRAKILL's heavy tier (punch 0.1 s; fighting games' heavy 16 frames ~0.27 s at 60 fps is too long for a gun that pierces three)."`,
];
const body = [`"enabled": ${enabled}`].concat(rows).map(r => I + r).join(',' + eol);
const start = at + m.index; // the enabled line's text
text = text.slice(0, start) + body.slice(I.length) + text.slice(close);
JSON.parse(text); // still valid JSON
fs.writeFileSync(path, text);
fs.copyFileSync(path, copy);
console.log(`design.n39.p39: ${rows.length / 2} numbers written, enabled ${enabled}; ${copy} copied`);
