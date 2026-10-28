// NIGHT40 builder E3 (models40): adds the Enemy40Parts hooks to the six enemy views (husk + tank, swarmer, spitter, charger, warden, drone). Per view class:
// a Body field, the view's row flag and two colours (set in its constructor, never a field initializer: L-035), the parts made at the end of Make (row on
// only), and the pose after the body's own tint in Render. Run once from the repo root: node Tools/night/n40e3-models40-hooks.cjs (it refuses a second run).
const fs = require('fs');
const views = [
  {
    file: 'Assets/Scripts/Game/EnemyViews.cs', cls: 'HuskView', kind: 'husk', height: 'height',
    make: '            meshes ??= new[] { body.GetComponent<MeshFilter>().sharedMesh, head.GetComponent<MeshFilter>().sharedMesh };\n',
    render: '                FeelProbe.Enemy(husk.id, b.rends[0].bounds, b.rends[1].bounds, flash); // NIGHT18: the reel\'s victim rect (M5)\n',
    pose: '                if (b.m40 != null) { b.m40.ShowGroup(husk.tank29 ? 2 : 1); b.m40.Pose(look, block, c, flash, windingUp ? 0.5f + 0.5f * wave : 0, staggered, danger40, parry40, (float)husk.windupAge); } // NIGHT40 E3 models40\n',
  },
  {
    file: 'Assets/Scripts/Game/EnemyViews.cs', cls: 'SwarmerView', kind: 'swarmer', height: 'height',
    make: '                meshes = new[] { cube, cube, cube };\n            }\n',
    render: '                    look.Tint(block, b.rends[i], pc, flash, Rims[i]);\n                }\n',
    pose: '                b.m40?.Pose(look, block, c, flash, crouch ? 1 : 0, false, danger40, parry40, (float)s.drawAge); // NIGHT40 E3 models40\n',
  },
  {
    file: 'Assets/Scripts/Game/EnemyViews.cs', cls: 'SpitterView', kind: 'spitter', height: 'height',
    make: '                for (int i = 0; i < 3; i++) meshes[i] = b.parts[i].GetComponent<MeshFilter>().sharedMesh;\n            }\n',
    render: '                for (int i = 0; i < 3; i++) look.Tint(block, b.rends[i], b.colors[i], flash, Rims[i]);\n',
    pose: '                b.m40?.Pose(look, block, body, flash, tell ? (swell - 1) / 0.6f : 0, false, danger40, parry40, (float)s.drawAge); // NIGHT40 E3 models40\n',
  },
  {
    file: 'Assets/Scripts/Game/EnemyViews.Charger.cs', cls: 'ChargerView', kind: 'charger', height: 'height',
    make: '            line.SetActive(false);\n',
    render: '                for (int i = 0; i < 3; i++) look.Tint(block, b.rends[i], b.colors[i], flash, Rims[i]);\n',
    pose: '                b.m40?.Pose(look, block, bc, flash, telling ? 1 : charging ? 0.6f : 0, stunned, danger40, parry40, age); // NIGHT40 E3 models40\n',
  },
  {
    file: 'Assets/Scripts/Game/EnemyViews.N20.cs', cls: 'WardenView', kind: 'warden', height: 'height',
    make: '            look.Attach(b.rends[2], false, false);\n',
    render: '                look.Tint(block, b.rends[2], armColor, flash, Rims[2]);\n',
    pose: '                b.m40?.Pose(look, block, body, flash, w.state == "tell" ? 1 : 0, down, danger40, parry40, age); // NIGHT40 E3 models40\n',
  },
  {
    file: 'Assets/Scripts/Game/EnemyViews.Drone.cs', cls: 'DroneView', kind: 'drone', height: '1f',
    make: '            look.Attach(b.rends[1], false, false);\n',
    render: '                look.Tint(block, b.rends[1], cc, flash, Rims[1]);\n',
    pose: '                b.m40?.Pose(look, block, sc, flash, charging ? 1 : 0, false, danger40, parry40, (float)d.pulse); // NIGHT40 E3 models40\n',
  },
];
const texts = {};
function block0(t, cls) {
  const s = t.indexOf('public sealed class ' + cls);
  if (s < 0) return '';
  const e = t.indexOf('public sealed class ', s + 10);
  return t.slice(s, e < 0 ? t.length : e);
}
for (const v of views) {
  let t = texts[v.file] ?? (texts[v.file] = fs.readFileSync(v.file, 'utf8').replace(/\r\n/g, '\n'));
  if (block0(t, v.cls).includes("Enemy40Parts m40;")) throw new Error("already patched: " + v.cls);
  const start = t.indexOf('public sealed class ' + v.cls);
  if (start < 0) throw new Error('no class ' + v.cls);
  let end = t.indexOf('public sealed class ', start + 10);
  if (end < 0) end = t.length;
  let block = t.slice(start, end);
  function once(anchor, insert, after = true) {
    const i = block.indexOf(anchor);
    if (i < 0) throw new Error(v.cls + ': no anchor ' + anchor.slice(0, 70));
    if (block.indexOf(anchor, i + 1) >= 0) throw new Error(v.cls + ': anchor twice ' + anchor.slice(0, 70));
    block = after ? block.slice(0, i + anchor.length) + insert + block.slice(i + anchor.length) : block.slice(0, i) + insert + block.slice(i);
  }
  once('            public Enemy drawn', '            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)\n', false);
  once('        readonly EnemyLook look;\n', '        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made\n        readonly Color danger40, parry40; // NIGHT40 E3: the parts\' red-orange and yellow\n');
  const lookLine = block.indexOf('look = EnemyLook.For(game);');
  if (lookLine < 0) throw new Error(v.cls + ': no look line');
  const eol = block.indexOf('\n', lookLine) + 1;
  block = block.slice(0, eol) + '            models40 = Enemy40Parts.On(game.cfg);\n            danger40 = Look.Hex(UnityTuning.Tell36RedHex);\n            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);\n' + block.slice(eol);
  once(v.make, '            if (models40) b.m40 = Enemy40Parts.Make("' + v.kind + '", b.root, look, ' + v.height + '); // NIGHT40 E3 models40\n');
  once(v.render, v.pose);
  t = t.slice(0, start) + block + t.slice(end);
  texts[v.file] = t;
}
for (const f of Object.keys(texts)) {
  const crlf = fs.readFileSync(f, 'utf8').includes('\r\n');
  fs.writeFileSync(f, crlf ? texts[f].replace(/\n/g, '\r\n') : texts[f]);
  console.log('patched ' + f);
}
