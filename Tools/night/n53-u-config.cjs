// NIGHT53 builder U: fill (or refill) ONE of U's items in design.n53.polish53.numbers, in both config copies (Design/unity-config.json and
// Assets/StreamingAssets/Design/unity-config.json, kept byte-identical). The item's object is read from a JSON file; it replaces the item's
// current object (the shared commit's placeholder {} or U's earlier fill). Asserts: the copies are identical before; the item's key appears
// exactly once inside design.n53.polish53.numbers; the result parses and holds the object; nothing outside the item's object changes.
//   node Tools/night/n53-u-config.cjs <item> <object.json>      (item: firstShot | killWash | scorchCore | icelineGrade | killPop)
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '..', '..');
const files = ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json'].map((f) => path.join(root, f));
const items = ['firstShot', 'killWash', 'scorchCore', 'icelineGrade', 'killPop'];
const [item, objFile] = process.argv.slice(2);
if (!items.includes(item) || !objFile) throw new Error('usage: node n53-u-config.cjs <item> <object.json>');
const obj = JSON.parse(fs.readFileSync(objFile, 'utf8'));
const texts = files.map((f) => fs.readFileSync(f, 'utf8'));
if (texts[0] !== texts[1]) throw new Error('the two config copies differ before the patch');
let text = texts[0];
const blockAt = text.indexOf('"polish53": {');
if (blockAt < 0 || text.indexOf('"polish53": {', blockAt + 1) >= 0) throw new Error('"polish53": { not found exactly once');
const numbersAt = text.indexOf('"numbers": {', blockAt);
const endAt = text.indexOf('"killPop_why"', blockAt);
if (numbersAt < 0 || endAt < 0 || endAt < numbersAt) throw new Error('polish53.numbers or killPop_why not found');
const indent = '          '; // the numbers' keys (10 spaces)
const key = `\n${indent}"${item}": `;
const at = text.indexOf(key, numbersAt);
const next = text.indexOf(key, at + 1);
if (at < 0 || at > endAt || (next >= 0 && next < endAt)) throw new Error(`${item} not found exactly once in polish53.numbers`);
// the item's object: from its opening brace to the matching close (strings in it carry no braces we must skip: track quotes anyway)
let i = at + key.length;
if (text[i] !== '{') throw new Error(`${item}'s value is not an object`);
let depth = 0, inStr = false, j = i;
for (; j < text.length; j++) {
  const c = text[j];
  if (inStr) { if (c === '\\') j++; else if (c === '"') inStr = false; continue; }
  if (c === '"') inStr = true;
  else if (c === '{') depth++;
  else if (c === '}') { depth--; if (depth === 0) break; }
}
const body = JSON.stringify(obj, null, 2).split('\n').map((l, n) => (n === 0 ? l : indent + l)).join('\n');
const out = text.slice(0, i) + body + text.slice(j + 1);
const parsed = JSON.parse(out);
const got = parsed.design.n53.polish53.numbers[item];
if (JSON.stringify(got) !== JSON.stringify(obj)) throw new Error('the patched item does not read back');
const before = JSON.parse(text);
before.design.n53.polish53.numbers[item] = obj;
if (JSON.stringify(before) !== JSON.stringify(parsed)) throw new Error('something outside the item changed');
for (const f of files) fs.writeFileSync(f, out);
console.log(`${item}: written to both copies (${out.length} bytes)`);
