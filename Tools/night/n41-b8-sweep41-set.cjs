// NIGHT41 builder B8: sets one design.n41.sweep41 number (text-level, CRLF kept, both copies byte-identical), optionally its _why; adds the key
// (and its _why) at the row's end if missing. Usage: node Tools/night/n41-b8-sweep41-set.cjs <key> <json value> [why]
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
const [key, value, why] = process.argv.slice(2);
if (!key || value === undefined) throw new Error('usage: <key> <json value> [why]');
JSON.parse(value);
for (const rel of ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json']) {
  const p = path.join(root, rel);
  let t = fs.readFileSync(p, 'utf8');
  const start = t.indexOf('"sweep41": {');
  const end = t.indexOf('\r\n      }', start);
  let block = t.slice(start, end);
  const keyRe = new RegExp(`("${key}": )([^\\r\\n]*?)(,?)(\\r\\n)`);
  if (keyRe.test(block)) block = block.replace(keyRe, (m, a, v, comma, nl) => a + value + comma + nl);
  else if (block.match(new RegExp(`"${key}": [^\\r\\n]*$`))) block = block.replace(new RegExp(`("${key}": )[^\\r\\n]*$`), `$1${value}`);
  else block += `,\r\n        "${key}": ${value}` + (why ? `,\r\n        "${key}_why": ${JSON.stringify(why)}` : '');
  if (why && !block.includes(`"${key}_why"`))
    block = block.replace(new RegExp(`("${key}": [^\\r\\n]*?)(,?)(\\r\\n|$)`), (m, a, comma, nl) => `${a},\r\n        "${key}_why": ${JSON.stringify(why)}${comma}${nl}`);
  else if (why && block.includes(`"${key}_why"`)) block = block.replace(new RegExp(`("${key}_why": )"(?:[^"\\\\]|\\\\.)*"`), `$1${JSON.stringify(why).replace(/\$/g, '$$$$')}`);
  t = t.slice(0, start) + block + t.slice(end);
  JSON.parse(t);
  fs.writeFileSync(p, t);
}
console.log(`sweep41.${key} = ${value}`);
