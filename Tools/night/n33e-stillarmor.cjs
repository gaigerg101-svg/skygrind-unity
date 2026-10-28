// NIGHT33 builder E -> the lead: wire StillArmor33View (STILL ARMOR's sign: the grey deflect burst and the shell) into SurvivalGame next to the spawn tell, and the
// message slot's danger line "MOVE" while the armor is up (builder U's UiHud33.Slot33: EXPOSED keeps first).
// Usage, from the repo root: node Tools/night/n33e-stillarmor.cjs [root]. Each anchor must occur exactly once; an edit already applied is skipped.
const fs = require('fs');
const path = require('path');
const root = process.argv[2] || '.';
function read(file) { return fs.readFileSync(path.join(root, file), 'utf8'); }
function after(file, anchor, add) {
  const p = path.join(root, file);
  let s = fs.readFileSync(p, 'utf8');
  if (s.includes(add.trim())) { console.log('already: ' + file); return; }
  const n = s.split(anchor).length - 1;
  if (n !== 1) throw new Error(file + ': anchor found ' + n + ' times: ' + anchor);
  const eol = s.includes('\r\n') ? '\r\n' : '\n';
  const at = s.indexOf(anchor);
  const lineStart = s.lastIndexOf('\n', at) + 1;
  const indent = s.slice(lineStart, at).match(/^\s*/)[0];
  const lineEnd = s.indexOf('\n', at + anchor.length);
  s = s.slice(0, lineEnd + 1) + indent + add + eol + s.slice(lineEnd + 1);
  fs.writeFileSync(p, s);
  console.log('patched: ' + file);
}
function replace(file, from, to) {
  const p = path.join(root, file);
  let s = fs.readFileSync(p, 'utf8');
  if (s.includes(to)) { console.log('already: ' + file); return; }
  const n = s.split(from).length - 1;
  if (n !== 1) throw new Error(file + ': anchor found ' + n + ' times: ' + from);
  s = s.replace(from, () => to);
  fs.writeFileSync(p, s);
  console.log('patched: ' + file);
}
after('Assets/Scripts/Game/SurvivalGame.cs', 'SpawnTell33View spawnTell33View;', 'StillArmor33View stillArmor33View; // NIGHT33 E: STILL ARMOR\'s sign (StillArmor33View.cs)');
after('Assets/Scripts/Game/SurvivalGame.Init.cs', 'spawnTell33View = new SpawnTell33View(this);', 'stillArmor33View = new StillArmor33View(this, particles); // NIGHT33 E: the deflect burst and the shell (nothing shows with bosses33 off)');
after('Assets/Scripts/Game/SurvivalGame.Render.cs', 'spawnTell33View?.Render(worldDt);', 'stillArmor33View?.Render(worldDt); // NIGHT33 E');
replace('Assets/Scripts/Game/UiHud33.cs',
  'h33Msg.Want(MessageSlot33.Danger, game.director != null && game.director.Exposed ? UnityTuning.Hud33DangerExposed : null);',
  'h33Msg.Want(MessageSlot33.Danger, game.director != null && game.director.Exposed ? UnityTuning.Hud33DangerExposed : game.sim != null && game.sim.bosses33Armor ? UnityTuning.StillArmor33Line : null); // NIGHT33 E: "MOVE" while a boss\'s STILL ARMOR is up');
