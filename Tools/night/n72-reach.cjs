// NIGHT72 builder N, slice N2 (the reach audit; GG 2026-10-26 jump height like ULTRAKILL's, "keep double jump"): the levels were built for a 1.5 m jump; the profile's is
// 5.07 m, and the double jump takes the feet 9.18 m over what they stood on (the parity lab, Research/2026-10-27-wave72/N/evidence/n5-lab-2.txt: "double jump apex above the floor").
//   node Tools/night/n72-reach.cjs [--level id[,id...]] [--jump 5.07] [--double 9.18] [--adjacent 2] [--selftest]
// Reads Design/levels/<id>.json (the level file the game loads) and prints, per room:
//   ESCAPE  a room with no roof slab (roof not true; the closed rooms have one on their ring walls): its ring's top T (the room's `height`) is under the reach of a player
//           standing on the floor (T <= double) or on a deck / ledge the double jump can reach whose footprint is within --adjacent m of a ring wall's inner face
//           (deck top + double >= T): the player clears the wall and leaves the room (into the next room, or the void under killPlaneY)
//   BONK    a room with a roof: its ceiling C is under a jump's apex plus the head (1.8 m): C < base + jump + 1.8 (single) / base + double + 1.8 (double), base = the floor (0) for the room,
//           the lane's own base (the ride wall's min y) for each ride lane (campaign.boxes with ride true): the head hits the roof and the rise is lost
// A level's SUMMARY line: rooms, roofless rooms, ESCAPE count, the rooms whose floor-level jump / double jump bonks, the ride lanes that bonk. Exit 1 when a level has an ESCAPE
// (the slice's bar: none left on the shipped and rebuilt levels, or each listed with why in the ledger). `--selftest`: a synthetic level with a planted low wall (a roofless room whose ring
// is 6 m high), a planted deck beside a 10 m ring, a roofed 7 m room with a ride lane, and a tall roofless room that must read clean: each found or not found as it should.
// Not checked: wall-jump chimneys (a wall jump off a different wall refills the count, chimney43), climbing a ring by wall rides, grapples, jump pads.
"use strict";
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const args = process.argv.slice(2);
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 && i + 1 < args.length ? args[i + 1] : d; };
const JUMP = Number(opt("--jump", 5.07)), DOUBLE = Number(opt("--double", 9.18)), ADJ = Number(opt("--adjacent", 2)), HEAD = 1.8;
const LEVELS = opt("--level", "n52-foundry,n52-iceline,n57-kiln,n52-span,n52-spire,n65-foundry,n67-iceline,n67-kiln,n67-spire").split(",");

/** The audit of one level object: {rows: [per room], escapes, summary}. */
function audit(level, id) {
  const rooms = (level.campaign && level.campaign.rooms) || [];
  const boxes = (level.data && level.data.boxes) || [];
  const rides = ((level.campaign && level.campaign.boxes) || []).filter((b) => b.ride === true);
  const rows = [];
  let escapes = 0, roofless = 0;
  const floorBonkJ = [], floorBonkD = [], laneBonks = [];
  for (const r of rooms) {
    const [x0, z0, x1, z1] = r.rect;
    const T = Number(r.height);
    const row = { room: r.id, kind: r.kind, height: T, roof: r.roof === true, escapes: [], lanes: 0, laneBonkJ: 0, laneBonkD: 0 };
    if (r.roof === true) {
      // a closed room: the head meets the ceiling
      row.floorMarginJ = (T - HEAD) - JUMP; // m of clear air above a single jump's apex from the floor (negative: bonk)
      row.floorMarginD = (T - HEAD) - DOUBLE;
      if (row.floorMarginJ < 0) floorBonkJ.push(r.id);
      if (row.floorMarginD < 0) floorBonkD.push(r.id);
      for (const b of rides) {
        if (b.zone !== r.id) continue;
        row.lanes++;
        const base = b.min[1];
        if (T < base + JUMP + HEAD) row.laneBonkJ++;
        if (T < base + DOUBLE + HEAD) row.laneBonkD++;
      }
      if (row.laneBonkJ || row.laneBonkD) laneBonks.push(`${r.id} ${row.laneBonkJ}J/${row.laneBonkD}D of ${row.lanes}`);
    } else {
      roofless++;
      // an open room: can the ring be cleared?
      if (T <= DOUBLE) row.escapes.push(`from the floor: the ring's top ${T} m is under the double jump's ${DOUBLE} m`);
      for (const b of boxes) {
        const top = b.max[1];
        if (!(top > 0.05 && top <= DOUBLE)) continue; // a surface the double jump can reach from the floor
        if (!(b.min[0] < x1 && b.max[0] > x0 && b.min[2] < z1 && b.max[2] > z0)) continue; // inside the room's footprint (the ring is outside its rect)
        const dh = Math.max(0, Math.min(b.min[0] - x0, x1 - b.max[0], b.min[2] - z0, z1 - b.max[2]));
        if (dh <= ADJ && top + DOUBLE >= T) row.escapes.push(`from the ${b.kind} at ${top} m (${b.min[0]},${b.min[2]} to ${b.max[0]},${b.max[2]}), ${dh.toFixed(1)} m off the ring: ${top} + ${DOUBLE} = ${(top + DOUBLE).toFixed(2)} >= ${T}`);
      }
      escapes += row.escapes.length > 0 ? 1 : 0;
    }
    rows.push(row);
  }
  const esc = rows.filter((x) => x.escapes.length);
  const summary = `n72-reach ${id}: ${rooms.length} rooms, ${roofless} roofless, ESCAPE ${esc.length}${esc.length ? " (" + esc.map((x) => x.room).join(", ") + ")" : ""}; ` +
    `floor jump bonks ${floorBonkJ.length}${floorBonkJ.length ? " (" + floorBonkJ.join(", ") + ")" : ""}, floor double jump bonks ${floorBonkD.length}${floorBonkD.length ? " (" + floorBonkD.join(", ") + ")" : ""}; ` +
    `ride lanes that bonk: ${laneBonks.length ? laneBonks.join(", ") : "none"} (jump ${JUMP}, double ${DOUBLE}, head ${HEAD}, adjacent ${ADJ} m)`;
  return { rows, escapes: esc.length, summary };
}

function print(level, id) {
  const a = audit(level, id);
  console.log(a.summary);
  for (const r of a.rows) {
    const bits = [`  ${r.room} ${r.kind} h ${r.height} ${r.roof ? "roof" : "ROOFLESS"}`];
    if (r.roof) bits.push(`floor margin jump ${r.floorMarginJ.toFixed(2)} m, double ${r.floorMarginD.toFixed(2)} m`);
    if (r.lanes) bits.push(`ride lanes ${r.lanes}: bonk ${r.laneBonkJ} jump / ${r.laneBonkD} double`);
    console.log(bits.join("; "));
    for (const e of r.escapes) console.log(`    ESCAPE ${e}`);
  }
  return a;
}

function selftest() {
  const room = (id, rect, height, roof, kind) => ({ id, kind: kind || "room", rect, height, roof });
  const box = (min, max, kind) => ({ min, max, kind: kind || "ledge" });
  const lv = {
    campaign: {
      rooms: [
        room("low", [0, 0, 20, 20], 6, false), // planted low wall: a roofless room whose ring is 6 m: the floor's double jump clears it
        room("deck", [100, 0, 120, 20], 10, false), // 10 m ring, a 2.5 m deck against its west wall: 2.5 + 9.18 clears it
        room("tall", [200, 0, 220, 20], 16, false), // 16 m ring, the same deck: 11.68 < 16, clean
        room("closed", [300, 0, 320, 20], 7, true), // a roofed 7 m room: a jump bonks (7 - 1.8 = 5.2 > 5.07 clear: margin 0.13) and the double jump bonks
      ],
      boxes: [{ zone: "closed", kind: "wall", ride: true, min: [300, 2, 5], max: [301, 7, 15] }],
    },
    data: {
      boxes: [
        box([100, 0, 5], [103, 2.5, 15]), // the deck, touching x0 = 100
        box([200, 0, 5], [203, 2.5, 15]),
      ],
    },
  };
  const a = audit(lv, "selftest");
  const by = Object.fromEntries(a.rows.map((r) => [r.room, r]));
  const checks = [
    ["planted low wall (ring 6 m): found from the floor", by.low.escapes.length === 1 && /from the floor/.test(by.low.escapes[0])],
    ["planted deck beside a 10 m ring: found", by.deck.escapes.length === 1 && /ledge at 2.5/.test(by.deck.escapes[0])],
    ["a 16 m ring with the same deck: not found", by.tall.escapes.length === 0],
    ["a roofed 7 m room: the double jump bonks, the single jump clears (margin +0.13)", by.closed.floorMarginD < 0 && by.closed.floorMarginJ > 0],
    ["its ride lane (base 2 m): the single jump bonks (7 < 2 + 5.07 + 1.8 = 8.87) and the double jump too", by.closed.laneBonkJ === 1 && by.closed.laneBonkD === 1],
    ["escapes counted once per room", a.escapes === 2],
  ];
  let bad = 0;
  for (const [name, ok] of checks) { console.log((ok ? "PASS " : "FAIL ") + name); if (!ok) bad++; }
  console.log("n72-reach selftest: " + (bad ? bad + " FAIL" : "all PASS"));
  process.exit(bad ? 1 : 0);
}

if (args.includes("--selftest")) selftest();
let bad = 0;
for (const id of LEVELS) {
  const p = path.join(ROOT, "Design/levels", id + ".json");
  if (!fs.existsSync(p)) { console.log(`n72-reach ${id}: no level file`); continue; }
  const a = print(JSON.parse(fs.readFileSync(p, "utf8")), id);
  if (a.escapes) bad++;
}
console.log(`n72-reach: ${bad} of ${LEVELS.length} levels with an ESCAPE`);
process.exit(bad ? 1 : 0);
