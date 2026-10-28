// NIGHT40 builder L2 round 3: one-shot patch of Tools/night/gen-n40-iceline.cjs (the lead's n40-r2 / n40-r3 frames): every lesson line names the button
// that does the room's job with the gauntlet in hand ({fire} is its swing; {punch} = F does nothing while it is held), and g5 / g7's middle ride panels move
// to the sides (the frames: a corridor of 5 m walls hid the decks and perches from the door). Each replacement must match exactly once.
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "gen-n40-iceline.cjs");
let s = fs.readFileSync(file, "utf8");
const R = [
  ['prompt: "BIG ONES FALL TO {punch}"', 'prompt: "{fire}: ONE SWING FELLS A TANK"'],
  ['prompt: "{punch} THE SHIELD, OR FLANK IT"', 'prompt: "{fire} THE SHIELD, OR FLANK IT"'],
  ['prompt: "{punch} THE ORB BACK AT IT"', 'prompt: "{fire} AS THE YELLOW ORB ARRIVES"'],
  ['prompt: "THE BLAST NEEDS FIFTEEN SECONDS"', 'prompt: "SAVE {altFire} FOR THE SECOND PACK"'],
  ['prompt: "THE KESTREL IS BACK: {fire}"', 'prompt: "KESTREL BACK: {fire} THE FAR ONES"'],
  ['prompt: "PISTOL FAR. PUNCH CLOSE. SWITCH."', 'prompt: "{quickSwap}: KESTREL FAR, GLOVE CLOSE"'],
  ['prompt: "IT CHARGES. DODGE. PUNCH ITS BACK."', 'prompt: "{dash} ASIDE, THEN {fire} ITS BACK"'],
  // g5: the panels from the middle (lx -5 / 5) to lx -9 / 9, facing in, ending at the decks' ramps (lz 15): the line from the door to the decks
  ['r.panel("W", -5, 6, 16, 0, 5, "ride panel (face west, 10 m): the line out of the bait\'s clump toward the west deck");',
    'r.panel("E", -9, 4, 13, 0, 5, "ride panel W (face east, 9 m; round 3: moved from the middle, whose 5 m walls hid the decks from the door): the line from the door to the west deck\'s ramp");'],
  ['r.panel("E", 5, 6, 16, 0, 5, "ride panel (face east): the line toward the east deck");',
    'r.panel("W", 9, 4, 13, 0, 5, "ride panel E (face west): the line from the door to the east deck\'s ramp");'],
  // g7: the panels from the middle (lx -5 / 5) to the side walls (lx -13 / 13, facing in): ride the wall north to the perch; the middle open (the spitters seen)
  ['r.panel("W", -5, 6, 20, 0, 5, "ride panel (face west, 14 m): the line up the west side toward the NW perch, out of the tank\'s reach");',
    'r.panel("E", -13, 4, 17, 0, 5, "ride panel W (face east, 13 m, by the west wall; round 3: moved from the middle, whose walls hid the perches from the door): ride north to the NW perch\'s ramp, out of the tank\'s reach");'],
  ['r.panel("E", 5, 6, 20, 0, 5, "ride panel (face east): the line up the east side toward the NE perch");',
    'r.panel("W", 13, 4, 17, 0, 5, "ride panel E (face west, by the east wall): ride north to the NE perch\'s ramp");'],
];
for (const [a, b] of R) {
  const n = s.split(a).length - 1;
  if (n !== 1) throw new Error(`expected 1 match, found ${n}: ${a}`);
  s = s.replace(a, () => b);
}
fs.writeFileSync(file, s);
console.log("patched", R.length, "lines");
