// NIGHT40 builder B5: writes design.n40.crown40's numbers (each with its _why) into Design/unity-config.json and copies the file to
// Assets/StreamingAssets/Design/unity-config.json (the two stay byte-identical). Patches ONLY the crown40 row's text (L-049): the row's
// keys the shared commit wrote (_why, enabled, enabled_why, levelFrom, levelTo) are kept as they are; this script's keys are set.
// Re-runnable (it sets, never appends). Run from the worktree root: node Tools/night/n40-crown40-config.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const file = path.join(ROOT, "Design/unity-config.json");
const copy = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

const ADD = {
  ring: {
    _why: "B5 round 2 (Research/2026-10-14-wave40/B5.md D3 / D4 / D7 / D8; boss_rules 4): where THE CROWN's ring may be (Core/Enemies/Crown37.N40.cs). The web has no CROWN.",
    hoverDistance: 8,
    hoverDistance_why: "m (flat) from the player (NIGHT37: 5). At 5 m the ring (3 m) and its blades (1.6 m) spanned ~85 deg of the view and filled the screen (frames f0700, f1000); at 8 m ~60 deg: the whole ring and the space through it read at once. A jump-dash reaches 21 m (Harness crown37 jumps), so 8 m stays in one pass. A hypothesis, judged on the Player's frames.",
    coreMargin: 0.5,
    coreMargin_why: "m kept between the ring's rim and the spire's core (the core's half width 4 m, the ring's radius 3 m): the ring never cuts into the core's wall (D3: 9.9 % of fight steps did, frame f0500).",
    roomMargin: 1,
    roomMargin_why: "m kept between the ring's rim and the boss room's walls (boss_rules 4: every position clamped to the arena rect minus a margin).",
    repositionSpeed: 30,
    repositionSpeed_why: "m/s of the anti-hide's flight onto the player (NIGHT37 teleported, Crown37.cs:407): 2.7x its 11 m/s hover, so it closes 20 m in under a second and is seen doing it (boss_rules 1: tell every move).",
  },
  fallDamage: 5,
  fallDamage_why: "hp a fall costs in the descent (NIGHT37 crown.fallDamage 20). Falls are part of the descent: the skilled pilot fell 4.7 times a fight and died once in 9 of 10 fights with no attack hit taken; the casual pilot fell ~10 times and died twice (n40crown defects, round 1). The leg's checkpoint is still the cost (the climb and the time); 5 keeps a sting.",
  line: {
    _why: "B5 (boss_rules 5: 'the damage rule is taught on entry, one line with the bound buttons'; GG 2026-10-13: 'players have no idea what to do, and even I don't know how to damage it'). The entry line THE CROWN shows when its fight begins (Core/BossLine40.cs through the lesson widget: {dash} and {jump} become the bound buttons), shown again after the first fall or death (maxShows 2 per fight). The web has no boss lines.",
    entry: "FLY THROUGH THE RING: {dash} DASH + {jump} JUMP",
    entryPad: "FLY THROUGH THE RING: {dash} DASH + {jump} JUMP",
    seconds: 6,
    seconds_why: "Long enough to read the line twice and look at the ring (the widget's 18 px line; the Lantern's lineSeconds was 3 for a 4-word line, this one is 7 words with two buttons). A hypothesis: no published number for a boss line's time was found (research/boss-web.md section 6).",
    maxShows: 2,
    maxShows_why: "Once on entry, once more after the first fall or death (the moment a person who missed it needs it); never more (Teach30's budget was 2 per fight).",
    ringPrompt: "{dash} + {jump}: THROUGH THE RING",
    ringPrompt_why: "n40-spire's v10 THE RING lesson (empty since NIGHT37), written into the level by Tools/night/gen-n40-spire.cjs: the fight's pass rehearsed with the same two buttons. Six words: a room lesson's limit (levelgen's lesson rule, Design/levels/CURRICULUM.md); the boss line above has no such limit.",
  },
};

const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const d = JSON.parse(raw);
const row = d.design && d.design.n40 && d.design.n40.crown40;
if (!row) throw new Error("design.n40.crown40 is missing (the shared commit writes it)");
const next = {};
for (const k of ["_why", "enabled", "enabled_why", "levelFrom", "levelTo"]) if (k in row) next[k] = row[k];
for (const k of Object.keys(row)) if (!(k in next) && !(k in ADD)) next[k] = row[k];
Object.assign(next, ADD);

// the row's text span: from `"crown40": {` inside design.n40 to its matching brace
const n40At = raw.indexOf('"n40": {');
const start = raw.indexOf('"crown40": {', n40At);
if (n40At < 0 || start < 0) throw new Error("crown40 row not found");
let i = raw.indexOf("{", start), depth = 0, inStr = false;
for (; i < raw.length; i++) {
  const ch = raw[i];
  if (inStr) { if (ch === "\\") i++; else if (ch === '"') inStr = false; continue; }
  if (ch === '"') inStr = true;
  else if (ch === "{") depth++;
  else if (ch === "}") { depth--; if (depth === 0) break; }
}
const lineStart = raw.lastIndexOf("\n", start) + 1;
const indent = raw.slice(lineStart, start);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const body = JSON.stringify(next, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, start) + '"crown40": ' + body + raw.slice(i + 1);
const check = JSON.parse(out);
if (JSON.stringify(check.design.n40.crown40) !== JSON.stringify(next)) throw new Error("the patched row does not read back");
for (const k of Object.keys(d.design.n40)) if (k !== "crown40" && JSON.stringify(check.design.n40[k]) !== JSON.stringify(d.design.n40[k])) throw new Error("another n40 row changed: " + k);
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n40.crown40: " + Object.keys(next).join(", "));
