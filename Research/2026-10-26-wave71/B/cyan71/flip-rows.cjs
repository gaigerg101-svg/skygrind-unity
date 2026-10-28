// Scratch tool (builder B, NIGHT71): turn the given design.n71 rows ON in the given config files, in place, by their one-line opening
// (`"<row>": {"enabled": false`), so the file's bytes change only there. For a fingerprint check with the rows ON; the caller restores the
// files with `git checkout --` (its own uncommitted change). Usage: node flip-rows.cjs <row,row,...> <file> [<file> ...]
const fs = require('fs');
const rows = process.argv[2].split(',');
for (const f of process.argv.slice(3)) {
  let s = fs.readFileSync(f, 'utf8');
  for (const r of rows) {
    const from = '"' + r + '": {"enabled": false', to = '"' + r + '": {"enabled": true';
    const n = s.split(from).length - 1;
    if (n !== 1) { console.error('flip-rows: ' + f + ': ' + r + ' found ' + n + ' times (want 1)'); process.exit(1); }
    s = s.replace(from, to);
  }
  fs.writeFileSync(f, s);
  console.log('flip-rows: ' + f + ': ' + rows.join(', ') + ' ON');
}
