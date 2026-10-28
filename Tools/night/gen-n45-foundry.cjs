// NIGHT45 builder A1 (foundry45; NIGHT45.md A1 "THE YARD's identity"; GG 2026-10-17 J4: "if it arena related please dont just randomly plce stuff
// it needs intension and personality"; DESIGN-RULES A-9: the identity card first, every solid's reason from it; DECIDE45 D-6: arena42's pieces leave b1).
// Level 1 as n45-foundry: n43-foundry's plan (Design/levels/n43/foundry.plan.json, written by Tools/night/gen-n43-foundry.cjs, which this file does
// not run or edit) copied whole, with ONE room rebuilt on the copy: b1 THE YARD -> THE SPILL (its card: docs/ROOMS.md "NIGHT45 identity cards").
// Every other room, door, light and prop is n43-foundry's, bit for bit (the script asserts it: the non-b1 rows of the copy equal the source's).
//
// THE SPILL (the card in one paragraph): the foundry's casting yard. An overhead crane carried ladles of iron from the furnace (the entry side,
// the NEAR bank) to the moulds (the exit side, the FAR bank); it dropped its ladle, which lies tipped on the far bank, and the slag ran across
// the yard as a river wall to wall. The husks walk the two cast BRIDGES in single file; the spitters roost on the LADLE's rim. The crane's two
// RAILS stand in the slag beside the bridges: ride one and the bridge's file is beside you (the KESTREL's charge pierces it, a punch launches
// the nearest into the slag); its inner face is the take-off for the one earned shortcut, a jump-off onto the rim. Wave 3 comes from the near
// bank's furnace corners (waveNodes43): the bridges point the other way.
// Every piece written here carries a `why` that names the card field asking for it (A-9; n36-jobs.cjs checks a job and a why on every solid).
// The checks: bash Tools/night/n45-foundry-regen.sh (this file, n36-jobs, levelgen straight into Design/levels/n45-foundry.json, the copy), then
//   dotnet run --project Tools/SimHarness -c Release -- n45yard gates | walk | jobs   (Tools/SimHarness/Harness.N45.Yard.cs)
// Probes: N45Y_<KNOB> env vars override the numbers below; a probe must write to N45Y_OUT (never the shipped plan).
// Run from the repo (or worktree) root: bash Tools/night/n45-foundry-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n43/foundry.plan.json");
const OUT = path.join(ROOT, "Design/levels/n45/foundry.plan.json");
const env = (k, d) => (process.env["N45Y_" + k] !== undefined ? Number(process.env["N45Y_" + k]) : d);
const KNOBS = {
  /// the slag's surface (m under the floor): a3 / a5's 0.8 (LEVEL-FORMAT's band 0.6-1.0: a burn's knock-up clears the rim)
  LAVA_Y: env("LAVA_Y", -0.8),
  /// the river's near (south) and far (north) edges, z (the room is z -180..-140; the entry at -140, the exit door at -180)
  RIVER_S: env("RIVER_S", -154),
  RIVER_N: env("RIVER_N", -166),
  /// the bridges: their inner and outer x (mirrored east / west); 3 m: one husk wide (a file)
  BRIDGE_IN: env("BRIDGE_IN", 10),
  BRIDGE_OUT: env("BRIDGE_OUT", 13),
  /// the crane rails: the bridge-side face's |x| (0.6 m thick, inward), and their z extent (over both banks: mounted from the near bank's floor)
  RAIL_FACE: env("RAIL_FACE", 9.4),
  RAIL_S: env("RAIL_S", -151),
  RAIL_N: env("RAIL_N", -169),
  RAIL_TOP: env("RAIL_TOP", 7),
  /// the ladle's rim (a deck): |x| half width, its south edge (z) and its top (m); its north edge is the river's far edge (it rests on the far bank)
  RIM_HALF: env("RIM_HALF", 6),
  RIM_S: env("RIM_S", -157),
  RIM_Y: env("RIM_Y", 2.5),
  /// the ladle's bowl: |x| half width, its south face (z), its top (m)
  BOWL_HALF: env("BOWL_HALF", 3.5),
  BOWL_S: env("BOWL_S", -162.5),
  BOWL_Y: env("BOWL_Y", 6.5),
  /// the rim ramps (from the far bank up beside the bowl): their centre |x|, width, and the far-bank end (z)
  RAMP_X: env("RAMP_X", 5),
  RAMP_W: env("RAMP_W", 2),
  RAMP_FROM: env("RAMP_FROM", -172),
};
const OUT_FILE = process.env.N45Y_OUT ? path.resolve(ROOT, process.env.N45Y_OUT) : OUT;
const probe = Object.keys(process.env).some((k) => k.startsWith("N45Y_") && k !== "N45Y_OUT");
if (probe && OUT_FILE === OUT) throw new Error("a probe (N45Y_* knobs) must write to N45Y_OUT, not the shipped plan");

const r2 = (v) => +v.toFixed(2);
const srcText = fs.readFileSync(SRC, "utf8");
const plan = JSON.parse(srcText);
if (plan.id !== "n43-foundry") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n45-foundry";
plan._why = "NIGHT45 builder A1 (design.n45.foundry45 swaps n43-foundry for this file while the row is on; GG 2026-10-17 J4, DESIGN-RULES A-9, DECIDE45 D-6): "
  + "Tools/night/gen-n45-foundry.cjs copies n43-foundry's plan whole and rebuilds b1 THE YARD from its identity card as THE SPILL (a slag river, two bridges, "
  + "the crane's two rails, the dropped ladle; docs/ROOMS.md). n43-foundry's _why: " + plan._why;

const Z = "b1";
const b1 = plan.rooms.find((q) => q.id === Z);
if (!b1) throw new Error("room b1 missing");
const [RX0, RZ0, RX1, RZ1] = b1.rect; // -18, -180, 18, -140
if (RX0 !== -18 || RZ0 !== -180 || RX1 !== 18 || RZ1 !== -140) throw new Error(`b1's rect moved: ${b1.rect}`);
const lz = (z) => r2(RZ1 - z); // ride36's lz: metres north of the room's south edge

// ---- the cut (card: Cut list): every n43 piece of b1 goes (galleries, their ramps, the two centre walls, the eight grid lights); the health pickup moves
const cut = {
  boxes: plan.boxes.filter((b) => b.zone === Z).length,
  ramps: plan.ramps.filter((b) => b.zone === Z).length,
  pits: plan.pits.filter((b) => b.zone === Z).length,
  lights: plan.lights.filter((l) => l.room === Z).length,
};
plan.boxes = plan.boxes.filter((b) => b.zone !== Z);
plan.ramps = plan.ramps.filter((b) => b.zone !== Z);
plan.pits = plan.pits.filter((b) => b.zone !== Z);
plan.lights = plan.lights.filter((l) => l.room !== Z);
const pickup = plan.props.find((p) => p.room === Z && p.kind === "healthPickup");
if (!pickup) throw new Error("b1's health pickup missing");
plan.props = plan.props.filter((p) => p.room !== Z || p === pickup);

const box = (kind, min, max, job, why, extra) => {
  const b = Object.assign({ zone: Z, kind, min: min.map(r2), max: max.map(r2), job, why, note: why }, extra || {});
  plan.boxes.push(b);
  return b;
};
const light = (o) => { plan.lights.push(Object.assign({ room: Z, normal: [0, 1, 0] }, o)); };
const K = KNOBS;
const BI = K.BRIDGE_IN, BO = K.BRIDGE_OUT, RS = K.RIVER_S, RN = K.RIVER_N;

// ---- the slag (card: Hazard both ways, Spatial law): five lava pits wall to wall between the banks; what is left between them is the bridges
const pit = (rect, why) => {
  const p = { zone: Z, rect: rect.map(r2), floorY: K.LAVA_Y, lava: true, job: "hazard", why, note: why };
  plan.pits.push(p);
  return p;
};
const both = "to the player a burn and a knock-up out of it; to an enemy death (Lava30): a husk punched or launched off a bridge dies in it (the card's Hazard both ways)";
pit([RX0, RN, -BO, RS], `THE SPILL's west slag (outside the west bridge, x ${RX0}..${-BO}): the river wall to wall, so the bridges are the only walk over; ${both}`);
pit([-BI, RN, -K.RIM_HALF, RS], `THE SPILL's west channel (between the west bridge and the ladle's rim, x ${-BI}..${-K.RIM_HALF}): the crane rail W stands in it; the rail's inner face jumps over it onto the rim (the earned shortcut); ${both}`);
pit([-K.RIM_HALF, K.RIM_S, K.RIM_HALF, RS], `THE SPILL's mouth (in front of the tipped ladle, z ${K.RIM_S}..${RS}): where the slag poured out; the rim is never walked onto from the near bank (only the ride reaches it); ${both}`);
pit([K.RIM_HALF, RN, BI, RS], `THE SPILL's east channel (x ${K.RIM_HALF}..${BI}): the crane rail E stands in it; its inner face is the east take-off onto the rim; ${both}`);
pit([BO, RN, RX1, RS], `THE SPILL's east slag (outside the east bridge, x ${BO}..${RX1}): the river wall to wall; ${both}`);

// ---- the crane rails (card: Verb it celebrates, Molecule's one-way edges, the earned shortcut): one ride wall in each channel beside its bridge
const rail = (side) => {
  const s = side === "W" ? -1 : 1;
  const face = K.RAIL_FACE * s, inner = r2((K.RAIL_FACE - 0.6) * s);
  const x0 = Math.min(face, inner), x1 = Math.max(face, inner);
  const name = side === "W" ? "west" : "east";
  const why = `THE SPILL's ${name} CRANE RAIL (the card's Verb it celebrates): a ride wall in the ${name} channel, ${r2(K.RAIL_S - K.RAIL_N)} m from the near bank over the slag to the far bank; `
    + `its ${name} face is ${r2(BI - K.RAIL_FACE)} m off the ${name} bridge's edge, so a rider has the bridge's file beside him: the KESTREL's charge pierces the file, a punch launches the nearest `
    + `husk off the bridge into the slag; its inner face (x ${inner}) is the take-off for the jump-off onto the ladle's rim (the earned shortcut)`;
  box("wall", [x0, -1.8, K.RAIL_N], [x1, K.RAIL_TOP, K.RAIL_S], "line", why,
    { ride: true, ride36: { face: side, planeX: r2(face), lz: [lz(K.RAIL_S), lz(K.RAIL_N)] } });
  // the cyan strips (V-3: a ridable face looks ridable), 0.9 m over the floor along both faces
  const zc = r2((K.RAIL_S + K.RAIL_N) / 2), len = r2(K.RAIL_S - K.RAIL_N);
  light({ id: `b1-R${side}`, kind: "wall", at: [r2(face + 0.05 * s), 0.9, zc], normal: [s, 0, 0], size: [len, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 });
  light({ id: `b1-R${side}i`, kind: "wall", at: [r2(inner - 0.05 * s), 0.9, zc], normal: [-s, 0, 0], size: [len, 0.2, 0.1], color: "cyan", range: 5, intensity: 0.9 });
};
rail("W");
rail("E");

// ---- the dropped ladle (card: Landmark, Hero prop, the spitters' roost): its rim (a deck resting on the far bank) and its bowl (the landmark)
const rimWhy = `the LADLE's RIM (deck ${K.RIM_Y} m, x +-${K.RIM_HALF}, z ${RN}..${K.RIM_S}; card: Molecule's node L): the spitters' roost over the slag, the landing of the earned shortcut `
  + "(the rails' inner faces), reached on foot only from the far bank by the two ramps beside the bowl";
box("deck", [-K.RIM_HALF, 0, RN], [K.RIM_HALF, K.RIM_Y, K.RIM_S], "line", rimWhy);
const bowlWhy = `the dropped LADLE's BOWL (x +-${K.BOWL_HALF}, ${K.BOWL_Y} m, on the rim's back half; card: Landmark): the tallest thing in the room, dead centre on the entry's line of sight `
  + "under the room's one amber lamp; in play it is the cover that turns the rim's spitters to face the entry (no line to the far bank) and splits the two ramps";
box("hull", [-K.BOWL_HALF, 0, RN], [K.BOWL_HALF, K.BOWL_Y, K.BOWL_S], "cover", bowlWhy);
for (const s of [-1, 1]) {
  const x = r2(K.RAMP_X * s);
  const why = `the ladle's ${s < 0 ? "west" : "east"} RIM RAMP (from the far bank z ${K.RAMP_FROM} up ${K.RIM_Y} m beside the bowl; card: Every-solid rule): the walkers' route up to the rim `
    + "(GG NIGHT20: enemies reach every floor) and the player's way down from it";
  plan.ramps.push({ zone: Z, from: [x, K.RAMP_FROM], to: [x, RN], width: K.RAMP_W, rise: K.RIM_Y, job: "climb", why, note: why });
}
// the ladle's cast face (the hero relief, 0.3 m proud of the bowl over the rim) and its glowing lip: view only (decor, no collider: 0.3 m, Rods40's flush band)
const faceZ = r2(K.BOWL_S + 0.15);
plan.props.push({ id: "b1-hero", room: Z, kind: "hero", at: [0, r2(K.RIM_Y + 1.6), faceZ], size: [r2(2 * K.BOWL_HALF - 1), 3.2, 0.3], yawDeg: 0, collider: false, decor: true,
  job: "hero", why: "THE SPILL's hero prop (card: Hero prop): the dropped ladle's cast face over the rim, the one object that says what happened here (the ladle spilled the river)" });
plan.props.push({ id: "b1-lip", room: Z, kind: "lavaChannel", at: [0, r2(K.BOWL_Y - 0.35), r2(K.BOWL_S + 0.1)], size: [r2(2 * K.BOWL_HALF - 0.4), 0.7, 0.2], yawDeg: 0, collider: false, decor: true,
  job: "landmark", why: "the ladle's LIP still glowing slag-orange (card: Landmark): the only hot glow at eye level from the entry; it is the river's source (danger colour: the slag)" });
pickup.at = [0, r2(K.RIM_Y + 0.9), r2(K.RIM_S - 1.5)];
pickup.why = "the ladle's rim pays health (card: Molecule): the earned shortcut's landing, the high ground only the ride reaches from the near side";

// ---- the light story (card: Light / colour story): the near bank dim, the ladle's amber lamp the brightest, the far bank brighter, the exit cyan
light({ id: "b1-L1", kind: "ceiling", at: [0, 11.95, -147], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 11, intensity: 0.8 });
light({ id: "b1-lamp", kind: "ceiling", at: [0, 11.95, r2((RN + K.RIM_S) / 2)], normal: [0, -1, 0], size: [3, 3, 0.1], color: "amber", range: 13, intensity: 1.5 });
light({ id: "b1-L2", kind: "ceiling", at: [0, 11.95, -173], normal: [0, -1, 0], size: [5, 3, 0.1], color: "white", range: 12, intensity: 1.25 });
light({ id: "b1-L8", kind: "pool", at: [0, 0.025, -178], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.6 });

// ---- the fight (card: The 30 s problem, State change): the waves' own spots (waveNodes43, rooms43); the room's nodes stay the far bank + the rim
const bx = r2((BI + BO) / 2);
const FAR = [[-bx, 0, -172], [bx, 0, -172], [-bx, 0, -176], [bx, 0, -176], [-15, 0, -169], [15, 0, -169]];
const RIM = [[-2.5, K.RIM_Y, -159.5], [2.5, K.RIM_Y, -159.5], [0, K.RIM_Y, -158.5], [-5, K.RIM_Y, -164], [5, K.RIM_Y, -164]];
const NEAR = [[-16, 0, -145], [16, 0, -145], [-16, 0, -149], [16, 0, -149], [-13, 0, -152], [13, 0, -152]];
b1.nodes = FAR;
b1.highNodes = RIM;
b1.waves = [
  // measured (n45yard walk --waves, round 1: 4 / 5 / 5): the bot died in the rim wave on 8 of 3 seeds' tries (19 deaths in 10 seeds, median 139.9 s
  // against the control's 51.2 s): the far wave's four husks left it at 26-61 health before the rim's spitters opened. n43's b1 opened with three.
  ["husk", "husk", "husk"],
  ["spitter", "spitter", "husk", "husk"],
  ["husk", "husk", "husk", "husk"],
];
b1.waveNodes43 = [FAR, RIM, NEAR];
b1.waveNodes43_why = "NIGHT45 A1 (the card's 30 s problem and State change): wave 1 from the far bank (two files over the bridges); wave 2 on the ladle's rim (the spitters' roost; "
  + "its husks come down the ramps and over the bridges); wave 3 from the near bank's furnace corners, the side you came in: the bridges point the other way";
b1.bridge36 = { rect: [-BO, RN, -BI, RS], why: "the west bridge: the husks' file (n36-jobs rule 2's enemy route beside the slag)" };
b1.bridges45 = [[-BO, RN, -BI, RS], [BI, RN, BO, RS]];
b1.bridges45_why = "NIGHT45 A1: both bridges (bridge36 holds one rect: the west one); read by Harness.N45.Yard.cs only";
b1.proofs = [
  { kind: "walk", points: [[0, 0, -142], [-bx, 0, -152], [-bx, 0, -168], [0, 0, -178]] },
  { kind: "walk", points: [[0, 0, -142], [bx, 0, -152], [bx, 0, -168], [0, 0, -178]] },
  { kind: "walk", points: [[K.RAMP_X, 0, -174], [K.RAMP_X, 0, K.RAMP_FROM], [K.RAMP_X, K.RIM_Y, -165], [2.5, K.RIM_Y, -159]] },
];
// the frames (the lead's reel and FeelReel.N45.Yard.cs): mid = the near bank by the west rail looking at the ladle; exit as n43
const yawTo = (x, z, tx, tz) => r2(-Math.atan2(tx - x, -(tz - z)) * 180 / Math.PI);
b1.frames36 = { mid: [-bx, 0, -150, yawTo(-bx, -150, 0, -163), 0.08], exit: [0, 0.5, -174, 0, -0.181] };
b1.verb36 = "the ride beside the bridge's file (pierce it, punch it into the slag); the rail's inner face to the ladle's rim";
b1.job36 = "combine";
b1.foundry45_why = `A1 (foundry45, THE SPILL): n43's b1 cut (${cut.boxes} boxes, ${cut.ramps} ramps, ${cut.lights} lights; arena42's grinder / cover are not written: DECIDE45 D-6); `
  + `river z ${RN}..${RS} lava ${K.LAVA_Y}; bridges x +-${BI}..${BO}; rails face x +-${K.RAIL_FACE} z ${K.RAIL_N}..${K.RAIL_S}; rim x +-${K.RIM_HALF} z ${RN}..${K.RIM_S} ${K.RIM_Y} m; `
  + `bowl x +-${K.BOWL_HALF} ${K.BOWL_Y} m; ramps x +-${K.RAMP_X} from z ${K.RAMP_FROM}`;

// ---- every other room bit for bit (asserted): the copy's non-b1 rows equal the source's, in order
{
  const src = JSON.parse(srcText);
  const keep = (k, rowZone) => {
    const a = (src[k] || []).filter((r) => rowZone(r) !== Z).map((r) => JSON.stringify(r));
    const b = (plan[k] || []).filter((r) => rowZone(r) !== Z).map((r) => JSON.stringify(r));
    if (a.length !== b.length || a.some((s, i) => s !== b[i])) throw new Error(`${k}: a non-b1 row changed`);
  };
  for (const k of ["boxes", "ramps", "pits", "tunnels", "pads", "grapple"]) keep(k, (r) => r.zone);
  for (const k of ["lights", "props"]) keep(k, (r) => r.room);
  const ra = src.rooms.filter((r) => r.id !== Z).map((r) => JSON.stringify(r)), rb = plan.rooms.filter((r) => r.id !== Z).map((r) => JSON.stringify(r));
  if (ra.length !== rb.length || ra.some((s, i) => s !== rb[i])) throw new Error("rooms: a non-b1 room changed");
  for (const k of Object.keys(src)) if (!["_why", "id", "rooms", "boxes", "ramps", "pits", "tunnels", "pads", "grapple", "lights", "props"].includes(k) && JSON.stringify(src[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);
}

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT_FILE), "| b1:", b1.foundry45_why);
