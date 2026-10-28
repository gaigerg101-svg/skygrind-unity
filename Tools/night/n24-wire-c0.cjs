// NIGHT24 lead wiring after C0 (the builders named these lines in their reports). Run once: node Tools/night/n24-wire-c0.cjs
const fs = require('fs');
function edit(file, from, to) {
  let s = fs.readFileSync(file, 'utf8');
  const crlf = s.includes('\r\n');
  const f = crlf ? from.replace(/\n/g, '\r\n') : from;
  const t = crlf ? to.replace(/\n/g, '\r\n') : to;
  if (!s.includes(f)) { console.log('MISSING in ' + file + ': ' + from.slice(0, 60)); return; }
  if (s.includes(t) && t.length > f.length && s.split(t).length > 1) { console.log('already: ' + file); return; }
  fs.writeFileSync(file, s.replace(f, t));
  console.log('edited ' + file);
}
edit('Assets/Scripts/Game/SurvivalSim.cs', '            spawner = director;\n',
  '            spawner = campaignSpawner = new Campaign24Spawner(director); // NIGHT24 (campaign-core): a wrapper that passes through unless a campaign run is on\n');
edit('Assets/Scripts/Game/SurvivalSim.Run.cs', '            InputFrame frame = input.SampleMove(Active);\n',
  '            InputFrame frame = input.SampleMove(Active);\n            N24LimitFrame(ref frame); // NIGHT24 (weapons-limits): a LIMIT room strips a verb for its room only\n');
edit('Assets/Scripts/Game/SurvivalGame.Init.cs', '            playerInput = new ReaderInput(input, combatReader, padReader, rumble);\n',
  '            playerInput = new ReaderInput(input, combatReader, padReader, rumble);\n            playerInput.events = events; // NIGHT24 (pad-y-weapons): the weaponSwitch event\n');
edit('Assets/Scripts/Game/SurvivalGame.N24.cs', '        void N24InitViews() => N24InitViewsHook();',
  '        void N24InitViews()\n        {\n            Wyrm24InitView(); // NIGHT24 builder B\n            Prewarm24.Start(this); // NIGHT24 switch-lag (builder F; off unless design.n24.prewarm.enabled)\n            N24InitViewsHook();\n        }');
edit('Assets/Scripts/Game/SurvivalGame.N24.cs', '        void N24RenderViews(double worldDt, double alpha) => N24RenderViewsHook(worldDt, alpha);',
  '        void N24RenderViews(double worldDt, double alpha)\n        {\n            Wyrm24RenderView(worldDt, alpha); // NIGHT24 builder B\n            N24RenderViewsHook(worldDt, alpha);\n        }');
edit('Assets/Scripts/Game/Ui.N24.cs', '        partial void BuildN24Hud();\n        partial void RenderN24Hud(double frameDt);',
  '        partial void BuildN24Hud() => BuildReticleArc24(); // NIGHT24 builder F\n        partial void RenderN24Hud(double frameDt) => RenderReticleArc24(frameDt);');
edit('Design/unity-config.json', '        { "id": "drydock", "label": "DRYDOCK", "file": "Design/maps/drydock.json" }\n',
  '        { "id": "drydock", "label": "DRYDOCK", "file": "Design/maps/drydock.json" },\n        { "id": "layer1", "label": "THE KILN", "file": "Design/levels/layer1.json" }\n');
