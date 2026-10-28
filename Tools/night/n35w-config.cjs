// NIGHT35 builder W: patch ONLY W's rows (design.n35.charge35, design.n35.pickup35) inside "n35": { ... } of Design/unity-config.json (L-049: the
// text of each row's object is replaced; the file is never re-serialised). Usage: node Tools/night/n35w-config.cjs [row ...]  (default: both rows)
// Then copy the file to Assets/StreamingAssets/Design/unity-config.json (bash Tools/night/n24-sync.sh).
const fs = require('fs');
const path = require('path');
const file = path.join(__dirname, '..', '..', 'Design', 'unity-config.json');
let text = fs.readFileSync(file, 'utf8');

const rows = {
  charge35: `{
        "_why": "NIGHT35 W1 P0 (DECIDE35 D-8; audit.md item 1): the KESTREL's and THE THUNDERCLAP's charge as its own trigger: a press-and-hold charges at once with NO normal shots first, a tap fires a normal shot at the release, the release of a FULL charge fires the charged shot (pierce33's beam / the slug, as NIGHT33 left them), a full charge held holdMaxS more fires itself, then a short fixed cooldown gates the next charge (a press or hold while it runs is today's normal fire). ULTRAKILL's revolver charges on its own input with no shots first (Research/2026-10-11-wave34/research/rea-ultrakill.md). Needs design.n31.charge on (its object and its fullSeconds: KESTREL 0.5 s, THUNDERCLAP 0.7 s, counted from the press). Code: Core/Guns/Charge35.cs. Off: NIGHT31's rule (0.4 s of auto-fire, then the charge), bit-exact.",
        "enabled": false,
        "tapS": 0.1,
        "tapS_why": "A hold shorter than this (whole steps: 12 at 120 Hz) is a TAP: no charge cue starts (charge35Start fires only past it). A press released before FULL always fires one normal shot, so this only decides when the charge reads as begun. 0.1 s: a deliberate tap on a pad trigger or a mouse button is 50-100 ms (guess, logged).",
        "holdMaxS": 1.0,
        "holdMaxS_why": "A FULL charge still held this long fires itself (n31's autoReleaseSeconds was 0.5 s): long enough to line up a pierce through a crowd, short enough that a held trigger (the smoke's virtual pad, the walk / attract bots) never stalls the gun more than full + 1 s. ULTRAKILL holds a full charge until release; the bots need it to end (guess, logged).",
        "revolver": {
          "cooldownS": 2.5,
          "cooldownS_why": "After a charged KESTREL shot, no new charge for this long (a press or hold in it is the normal fire). design.n31.charge's _why R3: ULTRAKILL's piercer about 0.5 s charge, 2.5 s cooldown (n31 used 4 s and read as 'lost'). With pierce33's x9 (225 per body) the KESTREL's sustained output rises about a quarter over n34's hold pattern; the boss band is re-measured by the lead (N-11)."
        },
        "shotgun": {
          "cooldownS": 3.0,
          "cooldownS_why": "After a charged THUNDERCLAP slug, no new charge for this long. The slug is a whole volley's damage x damageMult on one line, so it keeps a longer gap than the KESTREL (n31: 6 s vs 4 s, the same 1.5 ratio kept loosely): 3 s (guess, logged)."
        }
      }`,
  pickup35: `{
        "_why": "NIGHT35 W1 pickups (PRINCIPLES 3; audit.md items 2 and 4; DECIDE35 D-5): a VIEW row (no sim bit). A weapon pickup shows that weapon's own kit model (BlockyKit.ForWeapon, as SURVIVAL's wall buys), turning and bobbing, with a cyan rim and its name on approach; a pickup in a room the player has not reached yet shows the gun's dark silhouette (CoD's chalk outline); a wall buy the player cannot afford shows the same silhouette. The health pickup is a cyan vial (never a red cross: a protected emblem, research/teach.md section 2) with a pulse and 'MEND +N' on approach. Code: Game/PropsView29.cs, Game/Pickup35View.cs, Game/WallBuys.cs; numbers in UnityTuning.Weapons35.cs. Off (or -skygrindOff35): NIGHT34's cubes.",
        "enabled": false
      }`,
};

function matchBrace(s, open) {
  let depth = 0, inStr = false;
  for (let i = open; i < s.length; i++) {
    const ch = s[i];
    if (inStr) {
      if (ch === '\\') { i++; continue; }
      if (ch === '"') inStr = false;
      continue;
    }
    if (ch === '"') inStr = true;
    else if (ch === '{') depth++;
    else if (ch === '}') { depth--; if (depth === 0) return i; }
  }
  throw new Error('unbalanced');
}

const n35At = text.indexOf('"n35": {');
if (n35At < 0) throw new Error('"n35": { not found (the shared commit has not landed)');
const n35Open = text.indexOf('{', n35At);
const n35Close = matchBrace(text, n35Open);
const want = process.argv.slice(2).length ? process.argv.slice(2) : Object.keys(rows);
for (const row of want) {
  const key = `"${row}": {`;
  const at = text.indexOf(key, n35Open);
  if (at < 0 || at > n35Close) throw new Error(`${key} not found inside n35`);
  const open = text.indexOf('{', at);
  const close = matchBrace(text, open);
  text = text.slice(0, open) + rows[row] + text.slice(close + 1);
  console.log(`patched design.n35.${row}`);
}
JSON.parse(text); // still valid JSON
fs.writeFileSync(file, text);
