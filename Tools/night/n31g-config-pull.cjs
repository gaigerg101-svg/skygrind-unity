// NIGHT31 builder G: design.n31.grapplePull's numbers (Core/GrappleTap31.cs, Game/SurvivalSim.Grapple31.cs), each with its _why, inserted
// into the block by text (layout and line endings kept; `enabled` untouched). Re-runnable: an existing value is replaced in place.
// Run from the repo root: node Tools/night/n31g-config-pull.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const rows = [
  ['speed', 44, 'm/s, the pull-in speed written on the Mover\'s config.arcline.zip.reelSpeed for a run with the block on (the way Arcline21Rules.Apply writes the zip numbers; golden vectors never hook). GUESS (DESIGN31 B): the web\'s enemy zip is 34; R2 cites ULTRAKILL\'s whiplash at about 60 u/s; MEASURED (harness grapple31 B, the press to 30 m of path): 34 m/s 0.942 s, 44 m/s 0.742 s, 60 m/s 0.558 s; 44 and stays under the swing\'s horizontal clamp (arcline.softCap 42 x 1.2 = 50.4), so the speed kept at the release is the pull\'s.'],
  ['tapWindow', 0.2, 's: the grapple button up within this time after the press that fired the hook = a TAP (the pull-in); still held = the NIGHT30 swing. GUESS (DESIGN31 B); the hook itself flies 0.06-0.1 s (arcline.hookMin / hookMax), so a tap usually comes up while the hook flies or just after it bites. To check against real pad trigger taps (DESIGN31 R9).'],
  ['arrivePop', 5, 'm/s added upward to the velocity when a pull lets go at the anchor (the Mover\'s "zip" release at arcline.zip.arrive 2.6 m; the speed is kept), so a pull to a ledge or a node clears it. GUESS (R2: 4-6).'],
];
const n31At = text.indexOf('"n31": {');
const blockAt = text.indexOf('"grapplePull": {', n31At);
if (n31At < 0 || blockAt < 0) { console.error('n31g-config-pull: design.n31.grapplePull not found'); process.exit(1); }
const ind = '        ';
for (const [key, val, why] of rows) {
  const end = text.indexOf('}', blockAt);
  const block = text.slice(blockAt, end);
  const re = new RegExp('"' + key + '": [-0-9.]+');
  if (re.test(block)) {
    text = text.slice(0, blockAt) + block.replace(re, '"' + key + '": ' + val) + text.slice(end);
    continue;
  }
  // after the block's last row (before the newline that precedes its closing brace)
  const insertAt = text.lastIndexOf(eol, end);
  const add = ',' + eol + ind + '"' + key + '_why": ' + JSON.stringify(why) + ',' + eol + ind + '"' + key + '": ' + val;
  text = text.slice(0, insertAt) + add + text.slice(insertAt);
}
fs.writeFileSync(path, text);
console.log('n31g-config-pull: grapplePull = ' + JSON.stringify(JSON.parse(text).design.n31.grapplePull, (k, v) => (k.endsWith('_why') ? undefined : v)));
