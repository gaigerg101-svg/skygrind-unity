// NIGHT43 builder E1: adds E1's numbers to ITS OWN rows only (design.n43.rooms43, design.n43.iceline43), as text inserted before each row's
// closing brace (L-049: no reformat of the file), then copies the config to Assets/StreamingAssets/Design/ (the two stay byte-identical).
// Idempotent: a key already in its row is left alone. Never touches "enabled".
// Run from the worktree root: node Tools/night/n43e1-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..") + "/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rows = {
  rooms43: [
    ["endS", 2, "G5 (NIGHT43.md E1: 'an enemy out of its room's bounds, under its floor or in a dry pit for > 2 s ends'): a room's own body that stays outside its room's box or under its floor this long is ended (alive false, no kill event, no points), so a husk knocked into THE GANTRY's or PIERCE's dry 4 m pit, or one that fell through geometry, never holds the door (CODE-FACTS-A B9.3: only lava and the -30 m kill plane killed them). 2 s lets a body that is only passing a pit's lip or hopping a gap come back first (the husk's hop is ~0.6 s)."],
    ["pitDropM", 1.5, "Under its room's floor = the feet more than 1.5 m below the lowest of the room's spawn and nodes (the places a body of this room is meant to stand). Levels 1-3's dry pits are 4 m deep (CODE-FACTS-A B5), a step or a ramp's foot is under 1 m, so 1.5 m is a pit and never a floor detail."],
    ["outMarginM", 2, "Out of its room = the feet more than 2 m outside the room's rect and its nodes' box (x / z). 2 m is a door's depth beyond the rect's line (the doors are 4 m wide, a body stepping into the doorway is still in), and a drone's orbit stays inside its room."],
    ["heavyKind", "tank", "G22 (NIGHT43.md E1: \"'heavy' maps to a real heavier kind (name it)\"): levels 3 and 5 list 'heavy' (kb3 x2, v2, v6) and Husks.Make made a plain 40 HP husk of it (CODE-FACTS-B Enemies). The TANK (design.n29.tank: a husk body with 600 hp, the gauntlet x10, drawn 1.4x and darker) is the game's heavy and the validator already prices 'heavy' and 'tank' the same (LevelValidate29: costHeavy)."],
    ["maxTopUps", 2, "G5: with rooms43 on a room re-spawns its last wave for an enemy-needing lesson at most twice (design.n25.lesson.maxTopUps: 12, ~3 minutes of re-spawns holding the door: CODE-FACTS-A B9.4), then the enemy lessons are passed over (lessonTopUpCap) and the door opens on the room's own count. Two top-ups give a player who killed the husks before the lesson two more tries."],
    ["lessonCapS", 8, "G5 (CODE-FACTS-B: 'a lesson that needs a verb (not an enemy) can block the clear forever'): once a fight room's enemies are all dead, a lesson still not done is marked done after 8 s (the event lessonCap43) and the door opens: the line has shown its verb for its 5 s (teach35's showS) plus a breath, and the room never holds a player who cannot or will not do it."],
    ["onScreenRelocateS", 2, "G22 ('fail to move'; NIGHT43.md E1: 'the watchdog relocates a stuck enemy ON screen too ... after ~2 s'): the web's watchdog (ai/watchdog.js) hops, side-steps and re-paths at 0.5 / 1 / 1.5 s and relocates only OFF screen after ai.watchdog.relocateAt (CODE-FACTS-B: HuskBrain.cs:1141), so an enemy stuck in view stood there for the fight. With rooms43 on, in a campaign run, an enemy stuck 2 s in view (the ladder's three rungs tried) gets a tell and a short step."],
    ["onScreenTellS", 0.35, "The on-screen relocation's tell (event navTell43 with the enemy's id: the view's flash / shimmer) before the step: ULTRAKILL-style readability, a body never pops without a warning. 0.35 s is the spawn tell's order of time (spawn33) cut to a blink, short enough that the stuck spell ends at ~2.4 s."],
    ["onScreenStepMinM", 1.5, "The step's target: the nearest free nav node at least 1.5 m from the stuck enemy (a capsule's diameter plus a margin, so it leaves the corner it was stuck in) that still reaches the player (NavField.Relocate's field test, a full circle round the enemy)."],
    ["stillNudgeS", 2, "G22 (NIGHT43.md E1: 'no enemy stands still > 3 s while the player is in reach'): an enemy not in an attack that has moved under stillMoveM in 2 s near the player takes a new wait spot (its strafe flips and its ring angle jumps half a strafe cycle; a swarmer's flank side flips). Measured by `n43rooms still` (every enemy on screen, alive 20 s): the ring waiters (swarmers 4.8 s, a husk 4.3 s) and a dazed charger (6.3 s) stood longest. 2 s keeps the longest still spell under 3 s with a think's slack."],
    ["stillMoveM", 0.5, "Still = the feet moved less than 0.5 m (x / z) since the spell began: a body holding its ring spot (the strafe's own drift is under that within a think or two) or pressed into a wall."],
    ["stillRangeM", 16, "The nudge only near the player: within 16 m (the ring's 10 m wait radius plus a spitter's hold), where a still body reads as 'failing to move' (GG G22)."],
    ["unstickS", 2.4, "G22: the sim's own check on a room's body (SurvivalSim.N43.Rooms.cs), for what the brain's watchdog cannot see (it counts only while the enemy wants to move and makes no progress): `n43rooms still` found swarmers hung just off the ground at their spawn nodes (grounded false, y 0.1, 'direct' mode, the watchdog's stuck 0) and a charger standing 4.5 m from the player, still 4.8-6.3 s. A body still (under stillMoveM) for 2.4 s within stillRangeM, not in an attack, gets the tell and the step, so no spell reaches 3 s (2.4 + the 0.35 s tell = 2.75 s)."],
    ["standInHoldS", 20, "Nothing lost (PRINCIPLES 1): in NIGHT42 a room's stand-ins (a lesson's example targets: the KESTREL room's, PIERCE's) counted as alive, so they held the room's first wave until shot (the KESTREL room's order: the stand-ins, then the husks) and also its door. With rooms43 they still hold the next wave, for at most 20 s after the entry (a player who cannot land the shot still gets the fight), and never the door (the lesson gates it, and lessonCapS caps the lesson)."],
    ["commitM", 3, "G4 / T-4 round 2 ('teach, confirm, then spawn'; CODE-FACTS-A B7: in every arena36 room spawns33's trigger lines are bypassed, Level36.cs:161, so a room's waves started at its entry): a room's FIRST wave waits until the player's feet are 3 m from the room's spawn (the doorway side), i.e. the player has stepped into the room; enemies never stand in view of a player still in the doorway."],
    ["commitMaxS", 3, "...or 3 s in the room (the lead's round-2 card): a player who stands in the doorway still gets the fight after a breath (the arena36 switch-on bubble and spawn33's tells still play)."],
    ["afterLessonMaxS", 20, "G4 / afterLesson43: a wave that waits for a lesson comes in anyway 20 s after the room's entry (or retry), so a player who does not perform the taught verb is never left in an empty locked room (PIERCE: 'HOLD {jump} ON A WALL', then the enemies arrive). 20 s is four times teach35's 5 s line."],
  ],
};

for (const [row, keys] of Object.entries(rows)) {
  const start = raw.indexOf('"' + row + '": {');
  if (start < 0) throw new Error("row not found: " + row);
  // the row's own closing brace (strings skipped: a _why may hold "{jump}")
  let end = -1;
  for (let i = raw.indexOf("{", start), depth = 0, inStr = false; i < raw.length; i++) {
    const ch = raw[i];
    if (inStr) { if (ch === "\\") i++; else if (ch === '"') inStr = false; continue; }
    if (ch === '"') inStr = true;
    else if (ch === "{") depth++;
    else if (ch === "}" && --depth === 0) { end = i; break; }
  }
  if (end < 0) throw new Error("no closing brace for " + row);
  let body = raw.slice(start, end);
  const ew = body.indexOf('"enabled_why":');
  if (ew < 0) throw new Error("no enabled_why in " + row);
  const lineStart = raw.lastIndexOf(eol, start + ew) + eol.length;
  const indent = raw.slice(lineStart, start + ew);
  const closeLine = raw.lastIndexOf(eol, end);
  let insert = "";
  for (const [k, v, why] of keys) {
    if (body.includes('"' + k + '":')) continue;
    insert += "," + eol + indent + JSON.stringify(k) + ": " + JSON.stringify(v) + "," + eol + indent + JSON.stringify(k + "_why") + ": " + JSON.stringify(why);
  }
  if (!insert) continue;
  raw = raw.slice(0, closeLine) + insert + raw.slice(closeLine);
}
JSON.parse(raw); // still valid
fs.writeFileSync(file, raw);
fs.writeFileSync(copy, raw);
const d = JSON.parse(raw).design.n43;
for (const r of Object.keys(rows)) console.log(r, JSON.stringify(Object.fromEntries(Object.entries(d[r]).filter(([k]) => !k.endsWith("_why")))));
