// NIGHT33 builder R: the overhaul helpers shared by Tools/night/gen-n33-*.cjs (DESIGN33 card R). An overhaul RESHAPES an existing room in place, after the
// NIGHT30 / NIGHT32 code built it: its id, door line, spawn, exit, waves, lessons (<= 6 words), spawn nodes and pickups stay theirs (the bots, builder E's
// trigger spawns and the unlock rows key on them); the overhaul adds tiers, cover, a landmark footprint for builder K and the devices
// (design.n33.devices33, Core/Devices33.cs), and may move or drop the room's own props. Room-local coordinates as n30-lib: lx = metres east of the room's
// centre line, lz = metres NORTH of its south edge; web coordinates in the rows (x east, y up, z south).
//   const R33 = require("./n33r-lib.cjs");
//   R33.overhaul(level, "f9", (r, o) => { r.box(...); o.fan(...); o.vent(...); o.platform(...); o.landmark(...); o.drop((p) => ...); });
// Every overhauled room row gets `arenas33: { landmark: [x0, z0, x1, z1], note }` (the arenas33 harness checks these rooms) and `devices33: [...]`.
const { r3 } = require("./n30-lib.cjs");

function rectW(r, lx0, lz0, lx1, lz1) {
  return [r.X(Math.min(lx0, lx1)), r.Z(Math.max(lz0, lz1)), r.X(Math.max(lx0, lx1)), r.Z(Math.min(lz0, lz1))];
}

function ops(r) {
  const devs = (r.row.devices33 = r.row.devices33 || []);
  const block = (r.row.arenas33 = r.row.arenas33 || { note: "" });
  return {
    /// an UPDRAFT FAN: a grate lx0..lx1 x lz0..lz1 on the floor at y, a column `height` m tall; refuge = a standing point within 6 m [lx, y, lz]
    fan(lx0, lz0, lx1, lz1, y, height, refuge, note) {
      devs.push({ kind: "fan", rect: rectW(r, lx0, lz0, lx1, lz1), y: r3(y), height: r3(height), refuge: r.pt(refuge[0], refuge[1], refuge[2]), note: note || "" });
    },
    /// a HAZARD VENT: a grate on the floor at y, its cycle offset `phase` (0..1); refuge = a standing point within 6 m
    vent(lx0, lz0, lx1, lz1, y, phase, refuge, note) {
      devs.push({ kind: "vent", rect: rectW(r, lx0, lz0, lx1, lz1), y: r3(y), phase: r3(phase), refuge: r.pt(refuge[0], refuge[1], refuge[2]), note: note || "" });
    },
    /// a MOVING PLATFORM: its top centre goes from [lx, y, lz] to [lx, y, lz]; size [w, thick, d]; phase 0..1; refuge = a standing point within 6 m of an end
    platform(from, to, size, phase, refuge, note) {
      devs.push({ kind: "platform", from: r.pt(from[0], from[1], from[2]), to: r.pt(to[0], to[1], to[2]), size: size.map(r3), phase: r3(phase), refuge: r.pt(refuge[0], refuge[1], refuge[2]), note: note || "" });
    },
    /// the hero landmark's footprint for builder K (outside the run: on a plinth, a wall alcove or above 3 m), with what it should be
    landmark(lx0, lz0, lx1, lz1, note) {
      block.landmark = rectW(r, lx0, lz0, lx1, lz1);
      block.landmarkNote = note || "";
    },
    /// drop this room's own props for which pred(prop) is true (a crate that stood where a new tier goes)
    drop(pred) {
      const before = r.propsOut.length;
      r.propsOut = r.propsOut.filter((p) => !pred(p));
      return before - r.propsOut.length;
    },
    /// drop this room's spawn nodes within `rad` m of local (lx, lz) (a node a new piece would cover); returns how many
    dropNodes(lx, lz, rad) {
      const x = r.X(lx), z = r.Z(lz), before = r.nodes.length;
      r.nodes = r.nodes.filter((n) => Math.hypot(n[0] - x, n[2] - z) > rad);
      return before - r.nodes.length;
    },
    /// a solid cover piece standing on the floor (or a deck top at baseY), with an id of its own (`<room>-R33-<n>`: a drop() before it would make
    /// n30-lib's next `-P<n>` id a duplicate)
    solid(kind, lx, lz, w, hgt, d, baseY = 0) {
      r.solid(kind, lx, lz, w, hgt, d, baseY);
      block.pieces = (block.pieces || 0) + 1;
      r.propsOut[r.propsOut.length - 1].id = `${r.id}-R33-${block.pieces}`;
    },
    note(s) { block.note = s; },
  };
}

/// reshape room `id` of `level` (an n30-lib Level) with fn(room, ops); throws when the room is missing (a renamed id would silently skip an overhaul)
function overhaul(level, id, fn) {
  const r = level.rooms.find((q) => q.id === id);
  if (!r) throw new Error(`n33r-lib: no room ${id} to overhaul`);
  fn(r, ops(r));
  return r;
}

module.exports = { overhaul, rectW };
