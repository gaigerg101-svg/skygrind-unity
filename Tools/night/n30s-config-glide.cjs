// NIGHT30 builder S: design.n30.glide's numbers (Core/Glide30.cs) inserted after its "enabled", by text (layout and line endings kept). Re-runnable:
// an existing value is replaced in place (the numbers are tuned against harness glide30). Run from the repo root: node Tools/night/n30s-config-glide.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const rows = [
  ['decayPerSecond', 0.12, 'The share of the speed an ice glide loses per second with no input, in place of the Mover\'s ground friction (R3: ULTRAKILL gasoline removes friction, a slide decays slowly; Quake friction 4 halves speed in 0.17 s). 0.12: from 16 m/s about 11 m/s after 3 s, so a glide carries across a room but does not run forever. Measured by glide30.'],
  ['dashAdd', 7, 'm/s a dash begun on ice ADDS to its entry speed when it ends (scaled by 1 - entry / softCap, so it pays most when slow and nothing at the cap); the Mover alone ends a dash at max(walk, entry): no gain once fast. R3: pumping pays. Target (NIGHT30.md item 6): one dash on ice gains at least 20 %. Measured by glide30.'],
  ['softCap', 30, 'm/s: above it the glide speed loses capDrag of the excess per second, and a dash adds nothing (R3: caps are soft and layered; ULTRAKILL slide caps at 3x base). 30 m/s is 3.75x walk, above the 24 m/s dash.'],
  ['capDrag', 1.5, 'Per second, the share of the speed above softCap that a glide loses (a drag that bites gradually, not a wall: R3 pattern 4).'],
  ['turnRateDeg', 110, 'Degrees per second the glide turns the heading toward the move input at speed, the speed kept (R3 pattern 5: a TF-style slide steers at 1.8 rad/s = 103 deg/s; at speed the Mover\'s Quake acceleration cannot turn you at all). Measured by glide30.'],
  ['slopeGain', 1, 'x gravity along the ice floor\'s slope (the horizontal part of the floor normal points downhill): downhill adds speed, uphill bleeds it (R3: gravity is the accelerator; Tribes / Apex). 1 = the full gravity component.'],
];
const blockAt = text.indexOf('"glide": {', text.indexOf('"n30": {'));
if (blockAt < 0) { console.error('n30s-config-glide: glide block not found'); process.exit(1); }
const ind = '        ';
for (const [key, val, why] of rows) {
  const end = text.indexOf('}', blockAt);
  const block = text.slice(blockAt, end);
  const re = new RegExp('"' + key + '": [-0-9.]+');
  if (re.test(block)) {
    const nb = block.replace(re, '"' + key + '": ' + val);
    text = text.slice(0, blockAt) + nb + text.slice(end);
    continue;
  }
  const enAt = text.indexOf('"enabled": false', blockAt);
  const enAt2 = text.indexOf('"enabled": true', blockAt);
  const at = enAt >= 0 && enAt < end ? enAt + '"enabled": false'.length : enAt2 + '"enabled": true'.length;
  const add = ',' + eol + ind + '"' + key + '_why": ' + JSON.stringify(why) + ',' + eol + ind + '"' + key + '": ' + val;
  // append after the last row of the block (keep the order: after enabled, then each new row after the previous)
  const lastRow = text.lastIndexOf(eol, text.indexOf('}', blockAt));
  const insertAt = Math.max(at, lastRow);
  text = text.slice(0, insertAt) + add + text.slice(insertAt);
}
fs.writeFileSync(path, text);
console.log('n30s-config-glide: glide = ' + JSON.stringify(JSON.parse(text).design.n30.glide));
