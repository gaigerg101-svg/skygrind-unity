// NIGHT33 builder E -> the lead: wire SpawnTell33View (the spawns33 spawn tell's red-orange column) into SurvivalGame, next to the beacon view.
// Usage, from the repo root: node Tools/night/n33e-spawntell.cjs [root]. Each anchor must occur exactly once; the edit is skipped when already applied.
const fs = require('fs');
const path = require('path');
const root = process.argv[2] || '.';
function patch(file, anchor, add) {
  const p = path.join(root, file);
  let s = fs.readFileSync(p, 'utf8');
  if (s.includes(add.trim())) { console.log('already: ' + file); return; }
  const n = s.split(anchor).length - 1;
  if (n !== 1) throw new Error(file + ': anchor found ' + n + ' times: ' + anchor);
  const eol = s.includes('\r\n') ? '\r\n' : '\n';
  const i = s.indexOf(anchor) + anchor.length;
  const lineEnd = s.indexOf('\n', i);
  const indent = s.slice(s.lastIndexOf('\n', s.indexOf(anchor)) + 1, s.indexOf(anchor)).match(/^\s*/)[0];
  s = s.slice(0, lineEnd + 1) + indent + add + eol + s.slice(lineEnd + 1);
  fs.writeFileSync(p, s);
  console.log('patched: ' + file);
}
patch('Assets/Scripts/Game/SurvivalGame.cs', 'Beacon20View beaconView; // NIGHT20 A3 (builder P2)', 'SpawnTell33View spawnTell33View; // NIGHT33 E: the spawns33 spawn tell (SpawnTell33View.cs)');
patch('Assets/Scripts/Game/SurvivalGame.Init.cs', 'beaconView = new Beacon20View(this, sim.beacon20); // NIGHT20 A3 (builder P2)', 'spawnTell33View = new SpawnTell33View(this); // NIGHT33 E: draws enemySpawnTell33 (nothing shows with spawns33 off: no event)');
patch('Assets/Scripts/Game/SurvivalGame.Render.cs', 'beaconView?.Render(worldDt); // NIGHT20 A3 (builder P2)', 'spawnTell33View?.Render(worldDt); // NIGHT33 E');
