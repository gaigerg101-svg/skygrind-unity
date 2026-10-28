// NIGHT31 builder W: adds the numbers of design.n31.unlocks (item 3; the row stays off) to Design/unity-config.json. The web has no campaign
// and no unlock order, so the table is a Unity-side config row (CLAUDE.md rule 5). Run from the repo (or worktree) root:
//   node Tools/night/n31w-config-unlocks.cjs     (refuses to run twice; edits only the unlocks block's text; the lead syncs StreamingAssets)
const fs = require("fs");
const path = require("path");
const file = path.resolve(__dirname, "..", "..", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d = JSON.parse(raw);
const u = d.design && d.design.n31 && d.design.n31.unlocks;
if (!u) throw new Error("design.n31.unlocks missing (the shared commit adds it)");
if (u.start || u.levels) throw new Error("design.n31.unlocks already has its numbers");
const anchor = '      "unlocks": {' + eol + '        "_why": ' + JSON.stringify(u._why) + "," + eol + '        "enabled": false' + eol + "      }";
const at = raw.indexOf(anchor);
if (at < 0 || raw.indexOf(anchor, at + 1) >= 0) throw new Error("the unlocks block's text is not where it was expected (once)");
const row = (n, id, earns, room, note) => {
  const r = { n, id };
  if (earns) r.earns = earns;
  if (room) r.room = room;
  r.note = note;
  return r;
};
const block = {
  _why: u._why,
  enabled: false,
  start: {
    _why: "The weapons every campaign level starts with before the earlier levels' awards (Core/Unlocks26.cs Unlocks31.StartFor): the KESTREL only. The fist melee is not a weapon slot and is always there. Off: unlocks.json's start (KESTREL + gauntlet) plus the profile's cleared levels.",
    weapons: ["revolver"],
  },
  levels: {
    _why: "One row per level id (the NIGHT30 plan ids first; the NIGHT29 and old ids carry the same order so either curriculum works): level n starts with start.weapons plus the `earns` of every level with a smaller n. A row with `room` awards its weapon when that room is entered (Campaign24.Award31: weaponEarned once; a pickup of the same weapon in that room is then taken without a second award). A row without `room` earns as NIGHT30 did (the plan's room award or pickup, else the level's last room through unlocks.json).",
    rows: [
      row(1, "n30-foundry", null, null, "LEVEL 1 THE FOUNDRY: the KESTREL only (the Lantern's shield needs its charged shot)."),
      row(1, "n29-foundry", null, null, "level 1 under its NIGHT29 id."),
      row(1, "tutorial", null, null, "level 1 under its old id."),
      row(2, "n30-iceline", "gauntlet", "i1a", "LEVEL 2 THE ICELINE: the PULSE gauntlet in i1a (2.1 TANK ALONE: the tank dies to one gauntlet hit), DESIGN31 A."),
      row(2, "n29-iceline", "gauntlet", "i1", "level 2 under its NIGHT29 id (i1 TANK CORRIDOR)."),
      row(2, "level2", "gauntlet", "i1", "level 2 under its old id (i1 THE PULSE HALL)."),
      row(3, "n30-kiln", "shotgun", null, "LEVEL 3 THE KILN: the THUNDERCLAP as NIGHT30 (k1's award and pickup)."),
      row(3, "n29-kiln", "shotgun", null, "level 3 under its NIGHT29 id."),
      row(3, "layer1", "shotgun", null, "level 3 under its old id."),
      row(4, "n30-span", "nailgun", null, "LEVEL 4 THE SPAN: the NAILGUN as NIGHT30 (s1's award and pickup)."),
      row(4, "level3", "nailgun", null, "level 4 under its old id."),
    ],
  },
};
const text = JSON.stringify(block, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, at) + '      "unlocks": ' + text + raw.slice(at + anchor.length);
fs.writeFileSync(file, out);
const back = JSON.parse(fs.readFileSync(file, "utf8")).design.n31.unlocks;
console.log("design.n31.unlocks: start " + back.start.weapons.join(", ") + "; " + back.levels.rows.length + " level rows; enabled " + back.enabled);
