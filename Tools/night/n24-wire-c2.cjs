// NIGHT24 lead wiring, round C2 (B's MONOLITH reel registration). Run once: node Tools/night/n24-wire-c2.cjs
const fs = require('fs');
function edit(file, from, to) {
  let s = fs.readFileSync(file, 'utf8');
  const crlf = s.includes('\r\n');
  const f = crlf ? from.replace(/\n/g, '\r\n') : from;
  const t = crlf ? to.replace(/\n/g, '\r\n') : to;
  if (s.includes(t)) { console.log('already: ' + file); return; }
  if (!s.includes(f)) { console.log('MISSING in ' + file + ': ' + from.slice(0, 60)); return; }
  fs.writeFileSync(file, s.replace(f, t));
  console.log('edited ' + file);
}
edit('Assets/Scripts/Game/FeelReel.N24.cs', '"hitch", "cue" };', '"hitch", "cue", "monolith" };');
edit('Assets/Scripts/Game/FeelReel.N24.cs', '                case "cue": body = N24CueScenario(g, dir, res, offs, pre); break;\n',
  '                case "cue": body = N24CueScenario(g, dir, res, offs, pre); break;\n                case "monolith": body = MonolithScenario(g, dir, res, pre); break;\n');
