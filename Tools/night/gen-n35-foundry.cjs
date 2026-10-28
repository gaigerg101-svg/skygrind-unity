// NIGHT35 builder A (NIGHT35.md A1, design.n35.campaign6 swaps n34-foundry for n35-foundry): LEVEL 1 THE FOUNDRY, polished. It runs
// Tools/night/gen-n34-foundry.cjs UNCHANGED (which runs gen-n33 -> gen-n30) with two hooks placed BEFORE it loads:
//   - Level.room: the RIDE HALL f10r (Tools/night/n35a-lib.cjs rideRoom: two staggered ride panels over a lava strip, the deck, a pillar to circle) goes in
//     right before f11 REST, after f10 CRUCIBLE: the last fight before THE LANTERN's ring of ride panels;
//   - Level.write: after NIGHT34's lines34 pass, the n35 pass below (D-3 and the teach35 triggers), the id n35-foundry, Design/levels/n35/foundry.plan.json.
// Then dress33 and levelgen: bash Tools/night/n35-regen.sh foundry. Checks: lines35 n35-foundry, lines34 walk n35-foundry (Harness.N35.Levels.cs).
// D-3 (DECIDE35, decided per room by A): level 1 is the tutorial level (PRINCIPLES 2 / teach.md rule 10: teaching rooms are bare), the grapple is owned
// from level 4 (design.n35.grapple35.fromLevel), so NIGHT34's nodes in level 1 are REMOVED (f5, f7, f8, f9, f9b, f10: their wall lanes and ride walls are
// the line; a ring nobody can use in the first level is a lying cue, PRINCIPLES 3), except f5b's two rings over the lava run: the one TEASE (dim, locked).
// teach35 (builder T's lines by id; the rects override T's fallback rooms): l1.dash f3, l1.slide f7, l1.fire f5 (the first enemies: the stand-ins die
// to shots; T's fallback f8), l1.charge f8, l1.zoom f9 (one line per room, each at the room's door: ULTRAKILL 0-1's one verb per room).
const lib = require("./n30-lib.cjs");
const { Level } = lib;
const A35 = require("./n35a-lib.cjs");

const OUT = "Design/levels/n35/foundry.plan.json";
const P = Level.prototype;

A35.insertBefore(P, "f11", (L, base) => A35.rideRoom(L, base, {
  id: "f10r", name: "1.10r RIDE HALL", par: 25,
  waves: [["husk", "husk"], ["husk", "spitter"]],
  prompt: "RIDE THE PANELS OVER THE LAVA.",
}));

function n35(L) {
  const R = (id) => { const r = L.rooms.find((q) => q.id === id); if (!r) throw new Error("n35 foundry: no room " + id); return r; };
  for (const id of ["f5", "f7", "f8", "f9", "f9b", "f10"])
    A35.noNodes(R(id), "level 1 teaches the base verbs; the grapple is owned from level 4 (its line here is the wall rides)");
  // f5b LAVA RUN keeps its two rings over the lava as the level's one TEASE (teach.md rule 8: show a later verb's spot before it unlocks); walk round 1
  // with them removed: 9 / 10 (seed 7 stuck in f5b's fight, as NIGHT34 round 3 measured without the span pilot)
  A35.dimNodes(R("f5b"), "the lava run's two rings: the level's one tease of the grapple");
  // f5b round 2 (the lead: "a ride line over lava needs a floor to fall to"; the walk with grapple35 + wallride35 on: 9 / 10, seed 7 burned 171 times in the
  // lava run's fight): a 1.8 m DRY WALKWAY at floor height along the east wall over the lava (lx 5.2..7, lz 5.5..16) up to the catch steps: the long way
  // round for walkers and a floor to fall to under the east ride; the west wall ride over the lava stays the fast line.
  // f9 LAVA LANE round 2 (the walk with wallride35 + grapple35 on: seeds 1, 6, 10 stuck crossing the 10 m trench, 90-175 lava deaths): a 1.4 m DRY LEDGE along
  // the west wall under the wall lane (lx -8..-6.6, across the trench lz 8..18): the floor to fall to under the ride; the ride (and the ferry) stay the quick ways
  { const r = R("f9"); r.box("deck", -8, 8, -6.6, 18, -1.0, 0, "NIGHT35 A the west ledge across the lava trench (floor height, 1.4 m): a floor under the west wall lane"); }
  // and its RAMP over the catch steps' east end up onto the 2 m deck (lz 16 -> 21: 5 m for 2 m, as the deck's north ramp): the spitters' deck is reachable
  // on foot (seed 7 hopped on the entry floor for 900 s with the deck's two spitters alive and no way up but the rides)
  { const r = R("f5b"); r.box("deck", 5.2, 5.5, 7, 16, -0.8, 0, "NIGHT35 A the east walkway over the lava (floor height): the long way round, a floor under the east ride");
    r.ramp(6.1, 16, 21, 1.8, 2.0, "NIGHT35 A up from the east walkway onto the deck (walkers: 5 m for 2 m), over the catch steps' east end"); }
  // teach35 triggers: the rect is where the player stands when the verb is first needed
  { const r = R("f3"); A35.teach(r, "l1.dash", -5, 0, 5, 5.5); }          // the dash gap room's entry floor
  { const r = R("f5"); A35.teach(r, "l1.fire", -7, 0, 7, 5.5); }          // the first enemies (the stand-ins on the east ledge): fire
  { const r = R("f7"); A35.teach(r, "l1.slide", -4, 0, 4, 4); }           // the slide tunnel's mouth
  { const r = R("f8"); A35.teach(r, "l1.charge", -7, 0, 7, 4); }          // the pistol room's door: the charge (its stand-in line pierces)
  { const r = R("f9"); A35.teach(r, "l1.zoom", -8, 0, 8, 4); }            // the lava lane's door: zoom (aim on the west wall lane slows time)
}

P.write = (function (base) {
  return function () {
    n35(this);
    this.plan.id = "n35-foundry";
    this.plan._why = "NIGHT35 builder A (design.n35.campaign6 swaps n34-foundry for this file): Tools/night/gen-n35-foundry.cjs = gen-n34-foundry.cjs + the ride hall f10r (Lucio semi-platforming: two staggered ride panels over lava, a deck, a pillar) + D-3 (level 1's grapple nodes removed: owned from level 4) + the teach35 trigger rects (l1.dash f3, l1.fire f5, l1.slide f7, l1.charge f8, l1.zoom f9). " + this.plan._why;
    return base.call(this, OUT);
  };
})(P.write);

require("./gen-n34-foundry.cjs");
