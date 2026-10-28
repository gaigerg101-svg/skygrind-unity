// NIGHT30 builder B (round 5): THE LOOM's SWEEP as a moving bar (it hits where it is, so a jump timed over it clears it): sweepLiveSeconds 0.6 -> 1.5, sweepHeight 1.2 -> 1.0 and a
// new sweepBarHalf, in design.n30.span.loom of Design/unity-config.json (text replacement; keeps the line endings; idempotent). Run: node Tools/night/n30b-config-sweep2.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
if (text.includes('"sweepBarHalf"')) { console.log('already present'); process.exit(0); }
const live = /"sweepLiveSeconds": 0\.6,(\r?\n)(\s*)"sweepLiveSeconds_why": "[^"]*",/;
const m = live.exec(text);
if (!m) throw new Error('sweepLiveSeconds row not found');
const ind = m[2];
text = text.replace(live, `"sweepLiveSeconds": 1.5,${eol}${ind}"sweepLiveSeconds_why": "The sweeping leg crosses the deck (grip to the far corner, about 25 m) in this long: about 17 m/s, slow enough to see it come and jump it.",${eol}${ind}"sweepBarHalf": 0.8,${eol}${ind}"sweepBarHalf_why": "The bar's half width (m) along its travel: it hits a low player only where it is now, so a jump timed over it clears it (harness: a jumper at 4 m is not hit).",`);
text = text.replace(/"sweepHeight": 1\.2,(\r?\n)(\s*)"sweepHeight_why": "[^"]*",/, (s, a, i) => `"sweepHeight": 1,${eol}${i}"sweepHeight_why": "The bar's height over the deck (m): a jump clears it, a grapple off the deck too.",`);
fs.writeFileSync(path, text);
console.log('span.loom sweep: bar');
