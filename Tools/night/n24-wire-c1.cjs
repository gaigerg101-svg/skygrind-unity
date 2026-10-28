// NIGHT24 lead wiring, round C1 (F's three lines). Run once: node Tools/night/n24-wire-c1.cjs
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
edit('Assets/Scripts/Game/SurvivalSim.Campaign24.cs', 'if (campaign.finished && !campaignEnded)\n', 'if (campaign.finished && !campaignEnded && !CampaignBigPickHold())\n');
edit('Assets/Scripts/Game/SurvivalGame.cs', 'if (flow == Flow.Playing && run.state == SurvivalRun.State.Break && mods.Pending && hitstopLeft <= 0',
  'if (flow == Flow.Playing && (run.state == SurvivalRun.State.Break || mods.LayerPick) && mods.Pending && hitstopLeft <= 0');
edit('Assets/Scripts/Game/FeelReel.N24.cs', '"wyrm", "hitch" };', '"wyrm", "hitch", "cue" };');
edit('Assets/Scripts/Game/FeelReel.N24.cs', '                case "hitch": N24HitchBody(g, dir, res, offs, pre, ref body); break;\n',
  '                case "hitch": N24HitchBody(g, dir, res, offs, pre, ref body); break;\n                case "cue": body = N24CueScenario(g, dir, res, offs, pre); break;\n');
