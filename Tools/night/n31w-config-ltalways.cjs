// NIGHT31 builder W: adds the numbers of design.n31.ltAlways (item 2b; the row stays off) to Design/unity-config.json. The web's LT is the zoom
// or the special only, so these are Unity-side numbers (CLAUDE.md rule 5). Run from the repo (or worktree) root:
//   node Tools/night/n31w-config-ltalways.cjs     (refuses to run twice; edits only the ltAlways block's text; the lead syncs StreamingAssets)
const fs = require("fs");
const path = require("path");
const file = path.resolve(__dirname, "..", "..", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const c = d.design && d.design.n31 && d.design.n31.ltAlways;
if (!c) throw new Error("design.n31.ltAlways missing (the shared commit adds it)");
if (c.parryWindowSeconds !== undefined) throw new Error("design.n31.ltAlways already has its numbers");
const anchor = '      "ltAlways": {' + eol + '        "_why": ' + JSON.stringify(c._why) + "," + eol + '        "enabled": false' + eol + "      }";
const at = raw.indexOf(anchor);
if (at < 0 || raw.indexOf(anchor, at + 1) >= 0) throw new Error("the ltAlways block's text is not where it was expected (once)");
const block = {
  _why: c._why,
  enabled: false,
  parryWindowSeconds: 0.25,
  parryWindowSeconds_why: "The gauntlet's LT parry before its five pips (DESIGN31 A, R6): for this long a spitter glob or a drone orb within the punch's reflect reach (the web's orb.reflectRange 2 m) goes back at its shooter at the web's orb.reflectDamageMult (x2). Guess, logged.",
  parryCooldownSeconds: 1.0,
  parryCooldownSeconds_why: "After a parry press the gauntlet's LT shows COOLING with seconds for this long (R6). A parry moves no enemy, so it is not crowd control (no 15 s rule). Guess, logged.",
  nailgunZoom: 1.25,
  nailgunZoom_why: "R6: the NAILGUN's LT zooms this much while no nail is stuck (design.zoom.nailgunMag's 1.25) and detonates once one is (the slot label ZOOM / DETONATE). design.n26.weapons.nailgun.zoom 0 had left LT without a zoom.",
  shotgunZoom: 1.25,
  shotgunZoom_why: "The THUNDERCLAP's LT zoom (design.n25.shockwaveOff.zoom's 1.25; no shockwave: GG removed it in NIGHT25).",
  wordSeconds: 0.8,
  wordSeconds_why: "How long the slot shows an LT word that answers a press (PARRY, COOLING): long enough to read at a glance. View timing for the HUD, kept here beside the rule it reports.",
};
const text = JSON.stringify(block, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, at) + '      "ltAlways": ' + text + raw.slice(at + anchor.length);
fs.writeFileSync(file, out);
const back = JSON.parse(fs.readFileSync(file, "utf8")).design.n31.ltAlways;
console.log("design.n31.ltAlways: parry " + back.parryWindowSeconds + " s / cooldown " + back.parryCooldownSeconds + " s, NAILGUN zoom " + back.nailgunZoom + ", THUNDERCLAP zoom " + back.shotgunZoom + "; enabled " + back.enabled);
