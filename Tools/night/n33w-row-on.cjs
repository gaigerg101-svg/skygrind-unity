// NIGHT33 W: set design.n33.<row>.enabled = true in a COPY of Design/unity-config.json (a scratch root's; never the repo's).
// Usage: node Tools/night/n33w-row-on.cjs <path to the copy's unity-config.json> <row> [<row> ...]
const fs = require('fs');
const [file, ...rows] = process.argv.slice(2);
if (!file || rows.length === 0) throw new Error('usage: n33w-row-on.cjs <unity-config.json copy> <row> ...');
if (/[\\/]Design[\\/]unity-config\.json$/.test(file) && fs.realpathSync(file).startsWith(fs.realpathSync(process.cwd()) + require('path').sep + 'Design')) throw new Error('refusing the repo\'s own config');
const j = JSON.parse(fs.readFileSync(file, 'utf8'));
for (const r of rows) {
  if (!j.design || !j.design.n33 || !j.design.n33[r]) throw new Error('no design.n33.' + r);
  j.design.n33[r].enabled = true;
}
fs.writeFileSync(file, JSON.stringify(j, null, 2) + '\n');
console.log('design.n33.' + rows.join(', ') + ' on in ' + file);
