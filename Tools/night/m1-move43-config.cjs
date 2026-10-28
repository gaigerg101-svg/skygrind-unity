// NIGHT43 builder M1 (chimney43, ride43, boost43): writes M1's numbers into design.n43.chimney43 / ride43 / boost43 in Design/unity-config.json
// and copies the file to Assets/StreamingAssets/Design/unity-config.json (the two byte-identical before and after). Patches ONLY those three
// rows' text (L-049): each row from `"<name>": {` to its closing brace at the same indent is replaced by the row as parsed (the lead's _why,
// enabled false and enabled_why kept) plus M1's keys. Re-runnable. Run from the repo (or a worktree) root: node Tools/night/m1-move43-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const cfg = JSON.parse(raw);

const add = {
  chimney43: {
    newWallDot: 0.9,
    newWallDot_why: "A wall jump off a wall whose horizontal normal is less than this dot with the last wall jump's (about 26 deg apart or more) is a NEW wall: the count (wallJump.maxPerAirtime 3) refills first (DECIDE43 D-1; research/movement-web.md A5-A6: Dave's tutorial refills on a normal change, cr-parkour's same-wall test is dot >= 0.9). A chimney's facing walls (dot -1) and THE STACK's spiral panels (90 deg apart, dot 0) are always new.",
    newWallDist: 1.5,
    newWallDist_why: "Metres along the ground from where the last wall jump left: a parallel wall (or the same plane) farther than this is a new wall too (DECIDE43 D-1). Horizontal only, so climbing one face straight up never refills (the same wall still spends: no endless climb on one face).",
    chainWindow: 0.7,
    chainWindow_why: "Round 2, the chain (GG's G2: 'the walls should be slightly closer together so that even though the player is moving quickly, the rapid bounce from wall to wall naturally carries them upward'): a pressed wall jump off a NEW wall within this many seconds of leaving the last one (a wall jump, a jump-off, a ride's end) is a chain jump (chainUp / chainOut). A chimney's crossing at speed takes ~0.45 s.",
    chainUp: 10,
    chainUp_why: "Round 2: a chain jump's up speed (m/s; the plain wall jump's 1.0 x v0 = 8.33, wallfling42): it arrives at the facing wall still rising, so each bounce gains height (n43move flue, L1's tap pilot, 4.5 m: 2/40 -> 16/40 to the deck).",
    chainOut: 6,
    chainOut_why: "Round 2: a chain jump's speed out from the wall (m/s; wallfling42's 5): a shorter flight across the gap (less height lost on the way).",
    rideRefills: true,
    rideRefills_why: "A ride that starts on a new wall (the same test) refills the count too (NIGHT43.md M1 chimney43), so a ride, a press and the next wall never run out.",
  },
  ride43: {
    tapDraw: true,
    tapDraw_why: "(a) wallride35's near-miss draw (attachPull 4 m/s into a wall beside the motion) also when jump is not held (a tap ride, with ride40's intent: the stick into the wall). Was hold-only (CODE-FACTS-A A1).",
    attachReach: 0.45,
    attachReach_why: "(a) the swept attach probe: rays ahead-diagonal of the motion reach this many metres past the capsule plus this step's move (at 16 m/s one 120 Hz step is 0.13 m: research/movement-web.md B9 'a swept probe, 0.35-0.6 m for ATTACH while keeping 0.12 for STAY'). The stay probe (wallRide.probeDistance 0.12) is unchanged.",
    keepRise: 0.5,
    keepRise_why: "(b) the snap: wallgrip36 clamps a rising attach to 1.5 m/s at once (a jump at ~8 m/s up stops dead: the 'snaps too aggressively' of G3). With ride43 the attach keeps this share of the rise (never under the clamp), then the rising entry brakes by gravity as before (8.3 m/s -> 4.2, about 0.37 m more rise).",
    lostGrace: 0.12,
    lostGrace_why: "(c) seconds a ride survives a probe that finds no wall (wallride35's lostGrace 0.05): a facet's edge or a seam between panels no longer drops you (research/movement-web.md B10: cr-parkour lostGraceMs 120).",
    slamGrace: 0.3,
    slamGrace_why: "(d) seconds after leaving a wall (any ride end, a wall jump, a jump-off) during which B (slide) in the air is a DROP that keeps the horizontal speed (a rise cut to 0), not a slam: a slam sets (0, -48, 0) and kills every m/s (CODE-FACTS-A A3; DECIDE43 D-2).",
    slamLookDeg: 45,
    slamLookDeg_why: "(d) looking down more than this many degrees below level, B inside the grace is the slam after all (an aimed slam stays possible; DECIDE43 D-2).",
    holdNoStick: false,
    holdNoStick_why: "(e) OFF since round 2 (measured): waiving ride40's intent (the stick into the wall) while jump is held caught players holding jump on walls they were passing: B1's `n43wick casual --blocks wick43,ride43` killed THE WICK 3/10 with it (the casual bot held jump past the hot shaft wall and the ledges' faces: 222 deaths), 10/10 without it (every other ride43 sub-rule on). The rule stays in the code (true: ride43's round-1 (e), `n43move attach` hold+nostick 0/30 -> 30/30); the near-miss draws never use it.",
    lockPlane: 0.5,
    lockPlane_why: "(f) ride40's 0.2 s lockout (walls facing within 30 deg of the ended ride's) only for the SAME wall: a parallel wall whose plane is more than this many metres from where the ride ended is not locked (THE FLUE's stepped panels).",
    dashAttach: true,
    dashAttach_why: "(g) a dash in the air that meets a ridable wall it heads into attaches at once, along the wall at the dash's exit speed (DOOM Eternal's climb walls: dashing into one grabs it; research/movement-web.md B7). The locks, the re-ride rule, noride, ride40's clearance and wall height still apply.",
    dashIntoDot: 0.1,
    dashIntoDot_why: "(g) the dash must head into the wall at least this much (dot of the dash direction with -normal): a dash grazing a wall it runs along does not attach.",
    bufferDeg: 10,
    bufferDeg_why: "(i) flow34's wall buffer holds a jump press for a wall ahead within the jump buffer (0.1 s) only for an approach steeper than 30 deg (wallRide.awayDot 0.5): a press 0.05 s before reaching a wall at the usual ride approach (10-30 deg) was the double jump (n43move jumpseq, before). With ride43 an approach from this many degrees off the wall line holds it too, so the press becomes the wall jump / the hold ride on contact.",
    holdReach: 1.4,
    holdReach_why: "Round 2 (a'): with jump held, the sideways near-miss draw reaches this many metres past the capsule (ride40 cut wallride35's to 0.5 m): a player holding jump in a 4-4.5 m lane is drawn onto a wall (n43move flue, L1's held pilot: 4.5 m 16/40 -> 27/40 with the chain, 4.0 m 0/40 -> 31/40; L1 found 3.5-4.0 m lanes never started a ride). Jump held is the intent (e).",
    sameWallDeg: 30,
    sameWallDeg_why: "Round 2 (the lead's card, G13 at the root), the ONE 'same wall' every NIGHT43 rule uses (Move43.SameWall): facing within this many degrees (wallride35's reRideDeg 30) ... (the next four keys). Used by the re-ride rule, ride40's 0.2 s lockout, the ride lock, the wall-jump lock (ride43) and chimney43's new wall (a wall jump off a wall that is not the same wall refills the count; chimney43's newWallDot / newWallDist are no longer read).",
    sameWallPlane: 0.5,
    sameWallPlane_why: "... and its plane within this many metres of where the player left it (a parallel wall behind is another wall: was lockPlane, (f)) ...",
    sameWallAlong: 3,
    sameWallAlong_why: "... and along the ground within this many metres (wallride35's reRideDistance 3) ...",
    sameWallRise: 1.5,
    sameWallRise_why: "... and less than this many metres higher than where the player left it: a wall face 1.5 m or more above is a NEW wall (THE STACK's panel a turn up stands 11.2 m straight above; the lead's card: the re-ride rule refused it within 3 m horizontal until a landing) ...",
    sameWallDrop: 1,
    sameWallDrop_why: "... and less than this many metres lower: falling back down the wall you just left (a missed leap) catches you again instead of a fall to the floor (n43move stack trace: a missed panel fell past the one it left with re-ride refusing it).",
    pressPushes: true,
    pressPushes_why: "(h) a jump PRESS on a ride with the wall-jump count empty is the jump-off (wallride35's push, no spend), never the double jump straight up with no push (CODE-FACTS-A A2 'cap empty: EndRide + 7.5 up, no push off'). The lead's note: GG's pad shows the bottom face button as B = JUMP, so 'pressing B' in G3 may be this.",
  },
  boost43: {
    window: 0.15,
    window_why: "(a) the combine window, seconds: a ground THUNDERCLAP hold still inside its tap split when you take off fires on the takeoff step in the air; a ground volley aimed below level up to this long before the takeoff boosts on the takeoff (research/movement-web.md C13: TF2 treats a shot just before the jump as airborne; NIGHT43.md M1 boost43 a). charge40's shotgunTapS is 0.15 too.",
    lift: 11,
    lift_why: "(c) m/s up a boost gives at fullDeg below level or steeper (boost42's shape: a fall cut to -fallFloor first, then + lift; scaled by sin(pitch) / sin(fullDeg) above fullDeg). 11 vs 12 measured in Research/2026-10-16-wave43/M1.md (n43move boost).",
    fullDeg: 40,
    fullDeg_why: "(c) degrees below level from which the boost is full (boost42's 40).",
    doubleKeeps: true,
    doubleKeeps_why: "Round 2: a double jump never lowers a rise (the Mover's vy = max(vy, the double's 7.5 m/s)): a jump press just after a boost set vy to 7.5 and erased the boost (the move43 reel on n43-r2: jump + shot pressed on one frame, the press repeated on the frame's second sim step became the double jump: the rise 1.36 m both halves).",
    fallFloor: 3,
    fallFloor_why: "(c) a fall is cut to this many m/s down before the lift (boost42's 3): a late shot still climbs.",
  },
};

for (const name of Object.keys(add)) {
  const row = Object.assign({}, cfg.design.n43[name]);
  if (row.enabled !== false) throw new Error(name + ": enabled is not false (M1 never turns a row on)");
  for (const k of Object.keys(add[name])) delete row[k];
  Object.assign(row, add[name]);
  const key = '"' + name + '": {';
  const at = raw.indexOf(key);
  if (at < 0 || raw.indexOf(key, at + 1) >= 0) throw new Error(name + ": the row key is not unique");
  const lineStart = raw.lastIndexOf("\n", at) + 1;
  const indent = raw.slice(lineStart, at);
  if (!/^ *$/.test(indent)) throw new Error(name + ": unexpected text before the row's key");
  const eol = raw.slice(at + key.length, at + key.length + 2) === "\r\n" ? "\r\n" : "\n";
  const closeRe = new RegExp("\\r?\\n" + indent + "\\}", "g");
  closeRe.lastIndex = at;
  const cm = closeRe.exec(raw);
  if (!cm) throw new Error(name + ": the row's closing brace was not found");
  const end = cm.index + cm[0].length;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  raw = raw.slice(0, at) + '"' + name + '": ' + body + raw.slice(end);
}
JSON.parse(raw); // still valid JSON
fs.writeFileSync(file, raw);
fs.writeFileSync(copy, raw);
console.log("design.n43.chimney43 / ride43 / boost43 written, copied to StreamingAssets");
