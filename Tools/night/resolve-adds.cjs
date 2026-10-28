// NIGHT20: resolve a design.adds conflict between two builders' sub-blocks (both sides keep their sub-blocks; the block's
// own "enabled" stays true, the lead's). Works on both config copies; the result must parse and the copies match.
const fs = require('fs');
const R = 'C:/Users/gaige/dev/SKYGRIND-unity/';
for (const f of ['Design/unity-config.json', 'Assets/StreamingAssets/Design/unity-config.json']) {
  const p = R + f;
  let t = fs.readFileSync(p, 'utf8');
  const crlf = t.includes('\r\n');
  const nl = crlf ? '\r\n' : '\n';
  const lines = t.split(/\r?\n/);
  const out = [];
  for (let i = 0; i < lines.length; i++) {
    if (!lines[i].startsWith('<<<<<<<')) { out.push(lines[i]); continue; }
    const ours = [], theirs = [];
    let j = i + 1;
    for (; !lines[j].startsWith('======='); j++) ours.push(lines[j]);
    for (j++; !lines[j].startsWith('>>>>>>>'); j++) theirs.push(lines[j]);
    i = j;
    const isEnabled = l => /^      "enabled": (true|false),?$/.test(l);
    const a = ours.filter(l => !isEnabled(l));
    const b = theirs.filter(l => !isEnabled(l));
    const body = a.concat(b);
    // commas: every sub-block's closing "      }" gets a comma; then the block's own enabled last
    for (let k = 0; k < body.length; k++) if (body[k] === '      }') body[k] = '      },';
    for (let k = 0; k < body.length; k++) if (body[k] === '      },,') body[k] = '      },';
    out.push(...body, '      "enabled": true');
  }
  t = out.join(nl);
  JSON.parse(t);
  fs.writeFileSync(p, t);
  console.log('resolved ' + f + ': adds = ' + Object.keys(JSON.parse(t).design.adds).join(', '));
}
const a = fs.readFileSync(R + 'Design/unity-config.json'), b = fs.readFileSync(R + 'Assets/StreamingAssets/Design/unity-config.json');
console.log(a.equals(b) ? 'copies identical' : 'COPIES DIFFER');
