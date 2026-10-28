// NIGHT35 V: turn one of builder V's view rows on or off in Design/unity-config.json (L-049: only that row's own object text is touched,
// never a JSON.stringify of the file). Usage: node Tools/night/n35v-row.cjs <look35|lights35|tv35|ridelook35> <true|false>
// Then copy the file to Assets/StreamingAssets/Design/unity-config.json (bash Tools/night/n24-sync.sh).
const fs = require('fs');
const path = require('path');
const row = process.argv[2], val = process.argv[3];
if (!['look35', 'lights35', 'tv35', 'ridelook35'].includes(row) || !['true', 'false'].includes(val)) {
  console.error('usage: n35v-row.cjs <look35|lights35|tv35|ridelook35> <true|false>');
  process.exit(2);
}
const file = path.join(__dirname, '..', '..', 'Design', 'unity-config.json');
const text = fs.readFileSync(file, 'utf8');
const n35 = text.indexOf('"n35": {');
if (n35 < 0) throw new Error('no "n35" block');
const at = text.indexOf('"' + row + '": {', n35);
if (at < 0) throw new Error('no row ' + row + ' inside n35');
const end = text.indexOf('}', at); // V's rows hold no nested objects
const body = text.slice(at, end);
const re = /"enabled": (true|false)/;
if (!re.test(body)) throw new Error('row ' + row + ' has no enabled flag');
const next = body.replace(re, '"enabled": ' + val);
fs.writeFileSync(file, text.slice(0, at) + next + text.slice(end));
JSON.parse(fs.readFileSync(file, 'utf8')); // still valid JSON
console.log(`design.n35.${row}.enabled = ${val}`);
