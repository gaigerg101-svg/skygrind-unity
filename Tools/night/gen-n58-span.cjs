// NIGHT58 builder L (L1; row design.n58.span58: n52-span -> n58-span, OFF tonight). LEVEL 4 as n58-span, built by NIGHT57's slots idea
// (Tools/night/gen-n57-kiln.cjs) from span's own plan: Design/levels/n52/span.plan.json is READ, never written (Tools/night/gen-n52-span.cjs
// writes it), so the row off loads n52-span bit for bit.
// THE SLOTS (the card: docs/ROOMS.md, "Level 4 as n58-span"): span's combat rooms are p1 (the teach room) / p3 / p5; its links p2, p4, o1, o2.
// The new combat room goes in slot P2 (n52's link p2 4.2 FIRST GAP, x -18..18, z 37.5..87.5, between p1 and p3); p4 4.4 PULL LINE is the
// pull's TEST link (job36 test: "two rising gates, one charge each") and stays whole (PRINCIPLES rule 1: nothing the player had is lost).
// A slot holds its NEW room once that room passes its checks (n58-l-room.py, identity, the contract, walk 10/10); until then it holds n52's
// room bit for bit, so the level always plays end to end. NIGHT58 EXT: NEW = ["p2"] (THE GAP NEST): levelgen 21 errors (n52's 22 less
// p2's east pier pit; no new one), completable p2 PASS (3 proofs), enemies reach every tier p2 PASS, n46flow 10/10; n58-l-room.py / the
// contract / identity: not built or not run (not checked). span58 stays OFF.
// NIGHT59 L: the checkers exist (python -I Tools/night/n59-l-room.py n58-span table; bash Tools/night/n59-l-contract.sh n58-span p2).
// Cut 1: p2's opening vista -> held (Design/levels/n58/openings58.json; I7). Cut 2: THE SPLICE LANDING (5 m, the middle tier, a w1
// node, the room's health) + the near deck's lip rails (the void-room falls guard). Cut 3: THE SPLICE SPRING (the upward line). Contract
// 7/14 -> 10/14 -> 11/14. p2 stays out of NEW: size / density need the slot split (a <= 32 m room + an approach link, as gen-n57-kiln.cjs's
// slot DE), not built; the ride wall is against the card (n52's noride liners keep the pull the one fast line): a lead / GG call.
// NIGHT60 L (B4): the slot split (slotP2 below: the link p1b + the door d1b + the room, 28 x 32 m under n52's 22 m roof). Contract 11/14 ->
// 12/14 (1 size on the height alone: a 16 m roof held the drones against it; 5 ride wall, the card's), walk 10/10 / 10/10; p2 stays out of
// NEW (Research/2026-10-21-wave60/L.md).
// Run (from the worktree root):
//   node Tools/night/gen-n58-span.cjs [--slots=p2]     (default: the slots in NEW; --slots= with no ids = every slot n52's)
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n58/span.plan.json Design/levels/n58/span.json --walk
//   node Tools/night/gen-n58-span.cjs --finish          (writes Design/levels/n58-span.json + the StreamingAssets copy)
//   node Tools/night/gen-n58-span.cjs --check           (the generated Design/levels/n58/span.json against n52-span.json beyond the names)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n52/span.plan.json", PLAN_OUT = "Design/levels/n58/span.plan.json";
const GEN_OUT = "Design/levels/n58/span.json";
const OUTS = ["Design/levels/n58-span.json", "Assets/StreamingAssets/Design/levels/n58-span.json"];
const GENERATED_BY = "Tools/night/gen-n58-span.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish)";
// NIGHT58 EXT (the lead): p2 is built (slotP2) but out of NEW until it passes the header's checks (n58-l-room, identity, the contract;
// not built / not run yet) and its 61 bot falls into the void are read: build it with --slots=p2.
const NEW = [];
const SLOTS = { p2: slotP2 }; // slot id -> the builder of its new room (the card written first, A-9)

// the pieces (as gen-n57-kiln.cjs): every one names the card field it serves (`job`, "<field>: <what>") and why it is there (`why`)
const piece = (Z) => ({
  box: (kind, min, max, job, why, extra) => Object.assign({ zone: Z, kind, min, max, job, why, note: why }, extra || {}),
  ramp: (from, to, width, rise, job, why) => ({ zone: Z, from, to, width, rise, job, why, note: why }),
  pit: (rect, job, why) => ({ zone: Z, rect, floorY: -14, job, why, note: why }),
  pool: (id, x, y, z, w, d, color, range, intensity) => ({ id: Z + "-" + id, room: Z, kind: "pool", at: [x, y + 0.025, z], normal: [0, 1, 0], size: [w, d, 0.05], color, range, intensity }),
});
// a zone's rows in one list replaced at the place its first old row had (every other row keeps its place)
function swapRows(plan, key, isMine, fresh) {
  const arr = plan[key] || [];
  const at = arr.findIndex(isMine);
  const rest = arr.filter((x) => !isMine(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  plan[key] = rest;
}

// ================================================================ slot P2: p1b 4.1b THE RIGGERS' WALK + p2 4.2 THE GAP NEST (card: docs/ROOMS.md, Level 4 as n58-span)
// NIGHT60 L (B4, the split; the model: Tools/night/gen-n57-kiln.cjs slot DE): n52's link p2 (x -18..18, z 37.5..87.5, 50 m door to door) is
// two zones with the door d1b (x -2..2, z 69.5..73.5) between them: the approach link p1b (x -4..4, z 73.5..87.5; d1 where n52's is) and the
// combat room p2 (x -15..13, z 37.5..69.5: 28 x 32 m, THE ROOM CONTRACT's long side; d2 where n52's is). Its roof is n52's 22 m, over the
// contract's 16 m band: cut 1 / 2's 16 m roof held the drones against it (hoverMax 8 m over the 10 m decks; 32 of 80 seed runs capped). Depth in the room d = 69.5 - z.
// The room: the near deck (the door's, y 0, z 63.5..69.5) and the far deck (n52's gate A landing, y 10, z 41.5..49.5, kept whole with its
// gate: p2's way in) over THE GAP (14 m of void, floorY -14). The side gantry on the east wall (a ramp 10 m up over 17.5 m from the near deck
// to the far deck's east arm) is the walkers' long way round; the pull is the one-way fast line (no ride wall: the card, DECIDE59 D-10).
// The winch on the near deck is the foothold's cover (R5); the work lamp on the far deck's east arm. Drones over the gap, fodder walks.
function slotP2(plan) {
  const Z = "p2", P = piece(Z);
  const r = plan.rooms.find((x) => x.id === Z);
  if (!r) throw new Error("no room p2");
  if (JSON.stringify(r.rect) !== JSON.stringify([-18, 37.5, 18, 87.5])) throw new Error("p2 is not n52's link");
  const was = { kind: r.kind, name: r.name, par: r.par, limit: r.limit, height: r.height };
  r.name = "4.2 THE GAP NEST";
  r.kind = "room";
  r.card58 = "THE GAP NEST";
  r.arena36 = true;
  r.rect = [-15, 37.5, 13, 69.5];
  r.height = 22;
  r.split60_why = "NIGHT60 L (B4, the split; gen-n57-kiln.cjs slot DE's way): the room is the slot's south 32 m, 28 m wide, n52's 22 m tall (cut 1 / 2 had 16 m, the contract's band top: 8 / 40 and 24 / 40 seed runs capped at 240 s in p2's wave 2 with drones left, 37 of the 40 drones left at y 15.4, against the roof (16 m less the drone's 0.6 m radius) over the 10 m decks: Drone.cs's targetY is the ground below + hover 4-8 m, and a drone whose climb probe meets the roof stops (Director20's roof check is a NIGHT20 block); n52's link: x -18..18, z 37.5..87.5, height " + was.height + "): THE ROOM CONTRACT's 1 size (long side 20-32 m, height 10-16 m) and 2 density (>= 2.5 pieces / 100 m2) could not pass in the 50 m slot as one room (NIGHT59 L: 36 x 50 x 22 m, 0.72 / 100 m2); the north 14 m is the approach link p1b and the door d1b";
  r.par = 30;
  r.par_why = "NIGHT58 L EXT (card p2): a two-wave room between p1 (par 20, two waves) and p3 (par 35, three waves); n52's link had par " + was.par;
  delete r.limit;
  delete r.limitReason;
  r.limit_why = "NIGHT58 L EXT (card p2): n52's link was LIMIT " + JSON.stringify(was.limit) + " (PULL ACROSS: no boost over the gap); the room is a fight now and the gantry is the walkers' (and a player's) long way round, so a limit guards nothing: the full kit; the pull stays the fast line (NIGHT60 L: 17 m straight against ~27 m of gantry)";
  r.verb36 = "the pull as a fighting move: from the near deck's lip pull up across THE GAP to the far deck under the drones and shoot them out of the air; the husks walk the side gantry, the long way round";
  r.waves = [["drone", "drone", "husk", "husk", "husk"], ["drone", "drone", "drone", "swarmer", "swarmer", "swarmer", "swarmer"]];
  r.waves_why = "NIGHT58 L EXT (card p2, the table): w1 flyer + fodder (drone 2, husk 3), w2 flyer + fodder (drone 3, swarmer 4); the drone is level 2's flyer (as kb3c's in n57-kiln), the level's one new role (R-2, R-3)";
  r.roles43 = ["flyer + fodder (the drones over the gap, the husks down the gantry)", "flyer + fodder (the drones over the far deck, the swarmers down the gantry)"];
  r.roles43_why = "NIGHT58 L EXT (card p2; R-2 / R-3): kinds on level 4 go 3 -> 4 (husk, swarmer, spitter + drone)";
  r.spawn = { at: [0, 0, 67], yawDeg: 0 };
  r.spawn_why = "NIGHT60 L (B4): 2.5 m in from the door d1b on the near deck, the take-off lane ahead (n52's spawn was 2 m in from d1)";
  // the pull's gate: n52's ring and landing kept; the lip is the near deck's south edge, 16 m nearer (THE GAP 30 -> 14 m)
  if (!r.pull38 || r.pull38.gates.length !== 1 || JSON.stringify(r.pull38.gates[0].ring) !== JSON.stringify([0, 15, 46.5])) throw new Error("p2: n52's pull38 gate changed");
  r.pull38 = { _why: r.pull38._why, gates: [Object.assign({}, r.pull38.gates[0], { lip: [0, 0, 63.5] })] };
  r.pull38_why60 = "NIGHT60 L (B4): gate A's lip moved from z 79.5 to the split room's near deck edge z 63.5 (the ring and the landing are n52's)";
  r.grapple = "ONE ring 5 m over the landing, 3 m past its lip: from the near deck's lip, 17 m short of it, look up and PULL (NIGHT60 L: n52's ring; the gap 30 -> 14 m)";
  r.routeVia = [[[0, 0, 64.5], [0, 10, 46.5]]];
  r.fallReturn43 = Object.assign({}, r.fallReturn43, {
    to: [0, 0, 65],
    by: [{ rect: [-15, 49.5, 13, 69.5], to: [0, 0, 65], yawDeg: 0 }, { rect: [-15, 37.5, 13, 49.5], to: [0, 10, 46.5], yawDeg: 0 }],
    why60: "NIGHT60 L (B4): the rects re-cut to the split room (x -15..13); a fall into THE GAP is back 1.5 m before the near deck's lip (z 65), as n52's was before its lip",
  });
  // NIGHT60 L cut 2: on the decks' front edges (z 48-48.5): seen from the near deck over the far deck's edge (n59-l-room.py IN VIEW: 3.3 of 5 with the nodes 1-6 m deep)
  const far = [[-4, 10, 48], [4, 10, 48], [0, 10, 48.5]];
  const head = [[8, 10, 48], [10.5, 10, 48.5]];
  const landing = [[7.5, 5, 56]]; // NIGHT59 L cut 2: THE SPLICE LANDING (5 m): the spawns on two heights (the contract's 6 spawn tiers); 12.2 m from the spawn (spawn31)
  r.nodes = [];
  r.highNodes = far.concat(head).concat(landing);
  r.nodes_why = "NIGHT60 L (card p2, Molecule): every spawn on the far deck and its east arm (10 m up, 21-25 m from the spawn: a vista across THE GAP) and one on THE SPLICE LANDING (5 m, 12.2 m from the spawn); none on the near deck (spawn31: no node within 12 m in the spawn's cone)";
  r.waveNodes43 = [far.concat(head).concat(landing), far];
  r.waveNodes43_why = "NIGHT58 L EXT (card p2, State change): wave 1 on the far deck and the gantry head (the husks walk the gantry down at you, the drones lift over THE GAP); wave 2 on the far deck only: the deck you crossed to is now under fire";
  if (r.proofs.length !== 2 || JSON.stringify(r.proofs[1].points) !== JSON.stringify([[0, 10, 46.5], [0, 0, 39.5]])) throw new Error("p2: n52's proofs changed");
  r.proofs = [{ kind: "walk", points: [[0, 0, 67], [0, 0, 64.5]] }, r.proofs[1], { kind: "walk", points: [[4, 0, 66.5], [10.5, 0, 68.5], [10.5, 10, 50], [8.5, 10, 46], [3, 10, 46]] }];
  r.proofs_why58 = "NIGHT60 L (B4): n52's two proofs (the door to the lip, re-cut to the split room's near deck; the landing to the exit, kept); the third walks the side gantry from the near deck to the far deck (the walkers' long way)";
  r.frames36 = { mid: [0, 0, 65.5, 0, 0.45], exit: r.frames36.exit };
  r.span58_why = "NIGHT58 L EXT (Tools/night/gen-n58-span.cjs slotP2; docs/ROOMS.md, Level 4 as n58-span, p2): n52's link p2 4.2 FIRST GAP (kind " + was.kind + ") rebuilt as a combat room in its slot: the gate A landing and its gate kept whole (the far deck), the side gantry, the winch, the work lamp; NIGHT60 L split the slot (the room's south 32 m, the approach link p1b)";
  // ---------------------------------------------------------------- the approach link p1b (the slot's north 14 m; d1 where n52's is)
  const link = {
    id: "p1b", name: "4.1b THE RIGGERS' WALK", kind: "link", rect: [-4, 73.5, 4, 87.5], height: 12, roof: true, par: 3, job36: "rest",
    verb36: "walk to the gap: the door ahead opens on THE GAP NEST's near deck",
    spawn: { at: [0, 0, 86], yawDeg: 0 },
    exit: [0, 0, 75.5],
    proofs: [{ kind: "walk", points: [[0, 0, 86], [0, 0, 75.5]] }],
    frames36: { mid: [0, 0, 86, 0, 0.05], exit: [0, 0, 75.5, 0, 0.05] },
    split60_why: "NIGHT60 L (B4, the split; gen-n57-kiln.cjs slot DE's way): the slot's north 14 m (n52's link p2 ran x -18..18, z 37.5..87.5) as an 8 m wide walk from p1's door d1 to THE GAP NEST's door d1b: no fight, no pieces; the room south of it is THE ROOM CONTRACT's size",
  };
  plan.rooms.splice(plan.rooms.indexOf(r), 0, link);
  const d1 = plan.doors.find((d) => d.id === "d1");
  if (!d1 || JSON.stringify(d1.between) !== JSON.stringify(["p1", "p2"])) throw new Error("d1 is not n52's");
  d1.between = ["p1", "p1b"];
  plan.doors.splice(plan.doors.indexOf(d1) + 1, 0, { id: "d1b", between: ["p1b", "p2"], rect: [-2, 69.5, 2, 73.5], height: 4.5, checkpoint: true });
  // ---------------------------------------------------------------- the room's pieces
  const boxes = plan.boxes.filter((b) => b.zone === Z); // n52's: [0] gate A's LANDING (the far deck, kept whole with its gate), [1] / [2] the slot's noride liners (re-cut below)
  if (boxes.length !== 3 || !boxes[0].gate) throw new Error("p2: n52's boxes changed (expected the landing + two liners)");
  const landingWhy = "gate A's LANDING (12 m wide, 10 m above the take-off, 14 m of void before it (NIGHT60 L: n52's 30 m, the room split); its faces noride): only the grapple's pull reaches it fast (the gate); the gantry is the long way";
  const farDeck = Object.assign({}, boxes[0], {
    why: landingWhy, note: landingWhy,
    card58: "spatial law: THE FAR DECK (p3's door's deck; n52's gate A landing, kept whole: the pull's way in)",
    gate: Object.assign({}, boxes[0].gate, { from: [0, 0, 64], why: "NIGHT60 L (B4): 14 m of void and 10 m up from the near deck's lip (n52's from z 80, 30 m): one grapple pull to the ring over the landing (22 m, under 0.9 x the range); the walkers' long way is the side gantry" }),
  });
  const fresh = [
    farDeck,
    P.box("hull", [-15, -14, 37.5], [-14.8, 22, 69.5], "spatial law: the west wall's noride liner (the pull the one fast line)", "THE WEST LINER (noride, dull, ridged, no cyan, over the void wall to wall: n52's liner re-cut to the split room's west wall): a ride along the wall would skip the gate (D-6); only the grapple crosses fast, the gantry is the long way"),
    P.box("hull", [12.8, -14, 37.5], [13, 22, 69.5], "spatial law: the east wall's noride liner (the pull the one fast line)", "THE EAST LINER (noride, behind the side gantry and the gantry head: n52's liner re-cut to the split room's east wall): no ride along the gantry's wall (D-6); the walkers climb the gantry, the player pulls with the grapple"),
    P.box("hull", [-15, 0, 69.2], [-2, 22, 69.5], "molecule: the entrance gated (the door's west liner)", "the DOOR's west LINER (noride, on the entry wall): no ride across the doorway: the fight goes over THE GAP (the grapple) or down the gantry's lane"),
    P.box("hull", [2, 0, 69.2], [12.8, 22, 69.5], "molecule: the entrance gated (the door's east liner)", "the DOOR's east LINER (noride, on the entry wall to the gantry's foot): as the west's: the room's fast line is the grapple pull"),
    P.box("hull", [-7, 0, 65], [-3.5, 2.2, 68], "hero prop: THE RIGGER'S WINCH (the foothold's cover, R5)", "THE RIGGER'S WINCH (3.5 x 3 m, 2.2 m, 1.5-4.5 m in, west of the take-off lane): the drum the first line across was wound on: the entry's foothold of cover (R5) from the drones over THE GAP; off the pull's line (the lip at x 0 to the ring)"),
    P.box("hull", [-14.8, 0, 66], [-13, 1.2, 68.5], "every-solid: THE SPLICE CRATES (the near deck's second cover)", "THE SPLICE CRATES (1.8 x 2.5 m, 1.2 m, the near deck's west corner): the second cover on the near deck: crouch behind them for the grapple's charge while wave 1's drones come over THE GAP, then jump the lane"),
    // NIGHT60 L cut 2: the near deck's third cover and the far deck's landing bollards
    P.box("hull", [-11, 0, 65.5], [-9.5, 1.4, 67], "every-solid: THE SHEAVE BLOCK (the near deck's third cover)", "THE SHEAVE BLOCK (1.5 x 1.5 m, 1.4 m, between the splice crates and the winch): the pulley the line ran through: cover on the near deck's west half and the third stop of the loop round the winch while the grapple's charge comes back"),
    P.box("hull", [-2.6, 10, 49.2], [-2, 11.4, 49.5], "landmark: the landing lane's west bollard (the far side)", "THE FAR WEST BOLLARD (0.6 m post, 1.4 m, on the far deck's edge west of the landing lane): where the first line was made fast on the far side: with its pair it frames the grapple pull's landing lane; cover on the deck's edge from wave 1's husks on the gantry"),
    P.box("hull", [2, 10, 49.2], [2.6, 11.4, 49.5], "landmark: the landing lane's east bollard (the far side)", "THE FAR EAST BOLLARD (as the west's, east of the landing lane): the 4 m between the far bollards is the grapple pull's landing lane, lined up with the take-off lane's bollards across THE GAP"),
    P.box("hull", [-15, 0, 63.5], [-2.6, 1.1, 63.8], "hazard: the near deck's lip rail west (the gap's catch)", "THE LIP RAIL WEST (12.4 m, 1.1 m tall, from the west liner to the take-off lane): the near deck's edge over THE GAP fenced so a fight on the deck does not end in the void (the void-room falls guard); cover from the drones below the deck's edge"),
    P.box("hull", [-2.6, 0, 63.5], [-2, 1.4, 64.1], "landmark: the take-off lane's west bollard", "THE WEST BOLLARD (0.6 m post, 1.4 m, at the take-off lane's west side): where the first line was tied off: it marks the one lane to jump off for the grapple pull (the lip rails stop at the bollards)"),
    P.box("hull", [2, 0, 63.5], [2.6, 1.4, 64.1], "landmark: the take-off lane's east bollard", "THE EAST BOLLARD (as the west's, at the lane's east side): the 4 m lane between the bollards is the one jump off the lip for the grapple pull"),
    P.box("hull", [2.6, 0, 63.5], [8.5, 1.1, 63.8], "hazard: the near deck's lip rail east (the gap's catch)", "THE LIP RAIL EAST (5.9 m, 1.1 m tall, from the take-off lane to the gantry's foot): as the west rail; the gantry's lane starts beside it"),
    P.box("deck", [6, -14, 41.5], [12.8, 10, 49.5], "spatial law: the far deck's east arm (the gantry's head)", "THE GANTRY HEAD (10 m, 6.8 x 8 m, the far deck's east arm to the east liner): where the side gantry's climb comes up onto the far deck; the husks' spawn and their way down; the work lamp stands on it"),
    P.box("hull", [11.3, 10, 42], [12.8, 13, 44], "landmark: the work lamp's post (cover on the gantry head)", "THE LAMP POST (1.5 x 2 m, 3 m, the head's south-east corner): the far deck's white work lamp's stand: the landmark from the door and cover on the head when wave 2's drones come over it"),
    P.box("hull", [6, 10, 41.5], [11.3, 11.1, 41.8], "hazard: the gantry head's south rail (the void's catch)", "THE HEAD RAIL (5.3 m, 1.1 m tall, along the head's south face over the void under it): the husks spawned on the head do not walk off it, a fight on the head does not end in the void; cover from the drones over the far deck"),
    P.box("hull", [-5, 10, 42.5], [-3, 11.6, 44.5], "30 s problem: THE CABLE SPOOL (cover on the far deck under wave 2)", "THE CABLE SPOOL (2 x 2 m, 1.6 m, the far deck's west side): the line's spare spool: cover on the deck you pulled to when wave 2's drones come over it (State change); circle it to shoot them out of the air"),
    P.box("hull", [3, 10, 42.5], [5, 11.2, 44], "30 s problem: THE RIGGERS' CRATE (cover on the far deck's east side)", "THE RIGGERS' CRATE (2 x 1.5 m, 1.2 m, the far deck's east side): the second cover on the far deck, between the landing and the head: the loop round the spool and the crate keeps the drones' line of sight broken"),
    P.box("hull", [-6, 10, 44.5], [-5.7, 11.1, 49.5], "hazard: the far deck's west rail (the side void's catch)", "THE FAR WEST RAIL (5 m, 1.1 m tall, the far deck's west edge north of the spool): a fight on the far deck under wave 2 does not end in the west void (the void-room falls guard); the landing's lane and the drop to the exit stay open"),
    P.box("deck", [5.5, -14, 55.5], [8.5, 5, 60.5], "spatial law: THE SPLICE LANDING (the gantry's mid tier, 5 m)", "THE SPLICE LANDING (3 x 5 m at 5 m, off the side gantry's west edge where the gantry passes 5 m, over THE GAP, 3 m south of the near deck's lip): the room's middle tier (the contract's 3 planes: the near deck 0, the landing 5, the far deck 10); a husk spawn half way down the walkers' long way, the room's health and the splice spring's pad"),
  ];
  const ramps = [
    P.ramp([10.5, 67], [10.5, 49.5], 4, 10, "spatial law: THE SIDE GANTRY (the walkers' long way round)", "THE SIDE GANTRY (10 m over 17.5 m along the east wall, 4 m wide, its foot 2.5 m in from the entry wall, on the floor strip beside THE GAP): the walkers' climb between the decks (GG: a route enemies can climb; no safe ledge: the drones hang over it); a player's long way round, the grapple pull the fast one"),
  ];
  const pits = [
    P.pit([-15, 49.5, 8.5, 63.5], "hazard", "THE GAP: 14 m of void from the west wall to the gantry, no floor (a fall is back on the near deck's lip, fallReturn43); the drones hang over it, a drone knocked down over it is gone"),
    P.pit([-15, 37.5, -6, 49.5], "hazard", "the void beside the far deck (west, 9 m: NIGHT60 L cut 2, the exit strip's sight line up to the drones' ring over it; 4 m in cut 1: 3 of 20 seeds capped with a drone hidden behind the far deck): the far deck is a pier"),
    P.pit([6, 37.5, 13, 41.5], "hazard", "the void under the gantry head's south face (east of the exit strip)"),
  ];
  swapRows(plan, "boxes", (b) => b.zone === Z, fresh);
  swapRows(plan, "ramps", (b) => b.zone === Z, ramps);
  swapRows(plan, "pits", (b) => b.zone === Z, pits);
  // the lights: n52's four re-cut to the split (the ceiling panels at the 16 m roof), the work lamp; the link's two panels and its door pool
  const L = piece("p1b");
  const ceil = (Q, id, x, h, z, w, d, range, intensity) => ({ id: Q + "-" + id, room: Q, kind: "ceiling", at: [x, h - 0.05, z], normal: [0, -1, 0], size: [w, d, 0.1], color: "white", range, intensity });
  const lights = [
    ceil("p1b", "L1", 0, 12, 84, 3, 2, 10, 1),
    ceil("p1b", "L2", 0, 12, 77, 3, 2, 10, 1),
    L.pool("L3", 0, 0, 75.5, 2.5, 2.5, "cyan", 7, 1.6),
    ceil(Z, "L1", 0, 22, 66.5, 3, 3, 8, 1),
    ceil(Z, "L2", 0, 22, 45.5, 4, 4, 14, 1),
    P.pool("L3", 0, 0, 65.5, 2, 2, "cyan", 5, 1),
    P.pool("L4", 0, 10, 45.5, 3, 3, "cyan", 9, 1.8),
    P.pool("L5", 10.5, 10, 43.5, 2.5, 2.5, "white", 12, 1.4),
  ];
  lights[2].why = "NIGHT60 L (B4): the link's door pool at d1b (the way into THE GAP NEST), as the links' exit pools";
  lights[7].why = "NIGHT58 L EXT (card p2, Landmark): the far deck's white WORK LAMP on its post (the near deck under one panel, the void dark, the gate's cyan ring)";
  swapRows(plan, "lights", (l) => l.room === Z, lights);
  // NIGHT59 L cut 3: the upward line (the contract's 5 upward line): a pad on THE SPLICE LANDING up onto the gantry head
  plan.pads = (plan.pads || []).concat([{ zone: Z, at: [6.5, 5, 59], target: [9, 10, 45.5], apexAbove: 2, job: "molecule: the upward line, THE SPLICE SPRING",
    why: "NIGHT59 L cut 3: THE SPLICE SPRING (a jump pad, the upward line, A5): from THE SPLICE LANDING (5 m) up onto THE GANTRY HEAD (10 m) behind the husks' spawn: the walkers' long way pays a short cut at its middle (off the gantry's walk line at x 10.5; not on the pull's line)",
    note: "the splice spring" }]);
  // NIGHT59 L cut 2: the room's health (the contract's 6 health pickup) on THE SPLICE LANDING
  plan.props = (plan.props || []).concat([{ id: "p2-P59", room: Z, kind: "healthPickup", at: [7, 5.9, 57.5], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
    why: "NIGHT59 L cut 2: the room's health on THE SPLICE LANDING, half way down the side gantry over THE GAP under the drones (A20: the best at the riskiest spot; the walkers' long way pays)" }]);
  // the contract's computed size and density (Tools/night/n52-room-check.py's counting: every box, ramp, pad and grapple row of the zone,
  // the room row's grappleNodes and pads, solid props) and every piece's card field and why: asserted here, as gen-n57-kiln.cjs asserts the field
  const [x0, z0, x1, z1] = r.rect;
  const mine = (k) => (plan[k] || []).filter((o) => o.zone === Z);
  const pieces = [...mine("boxes"), ...mine("ramps"), ...mine("pads"), ...mine("grapple"), ...(r.pads || [])];
  const anchors = (r.grappleNodes || []).length + (plan.props || []).filter((q) => q.room === Z && q.collider).length;
  const dens = (pieces.length + anchors) * 100 / ((x1 - x0) * (z1 - z0));
  if (Math.max(x1 - x0, z1 - z0) > 32) throw new Error(`p2: ${x1 - x0} x ${z1 - z0} m: the long side is outside the contract's 20-32 m`);
  if (r.height < 19.4) throw new Error(`p2: a ${r.height} m roof is under the drones' hover over the lamp post (13 + hoverMax 8 + radius 0.6 m)`);
  if (dens < 2.5) throw new Error(`p2: density ${dens.toFixed(2)} pieces / 100 m2 (< 2.5)`);
  for (const o of pieces) if (o !== farDeck && !(String(o.why || "").trim() && (String(o.job || "").includes(":") || o.job === "hazard"))) throw new Error("p2: a piece without its card field or its why: " + JSON.stringify(o).slice(0, 160));
  for (const o of plan.pits.filter((q) => q.zone === Z)) if (!String(o.why || "").trim()) throw new Error("p2: a pit without its why");
  console.log(`gen-n58-span: p2 ${x1 - x0} x ${z1 - z0} x ${r.height} m, ${pieces.length + anchors} pieces, ${dens.toFixed(2)} / 100 m2`);
}

const skip = new Set(["name", "generatedBy", "_why", "source", "id"]);
const diffKeys = (ja, jb) => [...new Set([...Object.keys(ja), ...Object.keys(jb)])].filter((k) => !skip.has(k) && JSON.stringify(ja[k]) !== JSON.stringify(jb[k]));

if (process.argv[2] === "--finish") {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const text = fs.readFileSync(src, "utf8");
  const level = JSON.parse(text);
  if (level.name !== "n58-span") throw new Error(`${src}: name ${level.name}, expected n58-span`);
  if (level.data) level.data.name = "n58-span"; // the placeholder's (the shared commit's byte copy of n52-span: its two name keys n58-span)
  level.generatedBy = GENERATED_BY;
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
  for (const f of OUTS) fs.writeFileSync(path.join(ROOT, f), out);
  console.log(`n58-span: finished ${path.relative(ROOT, src)} -> ${OUTS.join(", ")} (${out.length} bytes)`);
  process.exit(0);
}

if (process.argv[2] === "--check") {
  const a = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/n52-span.json"), "utf8"));
  const g = JSON.parse(fs.readFileSync(path.join(ROOT, process.argv[3] || GEN_OUT), "utf8"));
  const shipped = JSON.parse(fs.readFileSync(path.join(ROOT, OUTS[0]), "utf8"));
  const d1 = diffKeys(a, g).filter((k) => k !== "data"), d2 = diffKeys(a, shipped).filter((k) => k !== "data"); // data is compared key by key below (its name differs)
  const dd1 = a.data && g.data ? diffKeys(a.data, g.data) : [], dd2 = a.data && shipped.data ? diffKeys(a.data, shipped.data) : [];
  console.log(`n58-span --check: generated ${GEN_OUT} vs n52-span.json beyond the names: ${d1.concat(dd1.map((k) => "data." + k)).join(", ") || "none"}`);
  console.log(`n58-span --check: shipped ${OUTS[0]} vs n52-span.json beyond the names: ${d2.concat(dd2.map((k) => "data." + k)).join(", ") || "none"}`);
  process.exit(d1.length + dd1.length + d2.length + dd2.length ? 1 : 0);
}

const text = fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n52-span") throw new Error("unexpected base plan " + plan.id);
const arg = process.argv.find((a) => a.startsWith("--slots="));
const want = arg ? arg.slice(8).split(",").filter(Boolean) : NEW;
for (const s of want) {
  if (!(s in SLOTS)) throw new Error("no slot " + s);
  if (!SLOTS[s]) throw new Error(`slot ${s}: its new room is not built yet (the card: docs/ROOMS.md, Level 4 as n58-span)`);
  SLOTS[s](plan);
}
plan.id = "n58-span";
plan._why = "NIGHT58 L (design.n58.span58 swaps n52-span for n58-span while the row is on): n52's plan with the slots' new combat rooms (" + (want.join(", ") || "none yet") + "). Before it: " + before._why;
// the guard: with no slot built, every key but the names is n52's bit for bit
if (!want.length) for (const k of Object.keys(before)) if (!["id", "_why"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(k + " changed with no slot built");
fs.mkdirSync(path.join(ROOT, "Design/levels/n58"), { recursive: true });
fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1) + "\n");
console.log(`gen-n58-span: wrote ${PLAN_OUT} (slots built: ${want.join(", ") || "none: every room n52's"})`);
