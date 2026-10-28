// C. Movement trajectories: the real Mover (src/movement/mover.js) against the real world collider (src/world/collider.js
// over a three.js Octree built the way src/world/arena.js builds it), stepped at the real fixed rate with scripted
// InputFrames. Every step records the input fed in, then the Mover's position, velocity, state and the events it
// emitted. Run: node port-export/scripts/trajectories.mjs
import { BoxGeometry, Group, Mesh, Scene } from 'three';
import { Octree } from 'three/addons/math/Octree.js';
import { Mover, createInputFrame } from '../../src/movement/mover.js';
import { createWorldCollider } from '../../src/world/collider.js';
import { createArena } from '../../src/world/arena.js';
import { config, enc, writeJson } from './lib.mjs';

const DT = 1 / config.loop.simHz;
const SETTLE = 0.25; // s standing still after the reset before any input (config.debug.settleTime does the same)
const BUTTONS = ['jump', 'dash', 'slide', 'fire', 'altFire', 'special', 'punch', 'interact', 'grapple', 'reel'];

/** The same box -> geometry mapping as arena.js boxGeometry(): BoxGeometry(w, h, d) translated to the box centre. */
function boxMesh(b) {
  const w = b.max[0] - b.min[0];
  const h = b.max[1] - b.min[1];
  const d = b.max[2] - b.min[2];
  return new Mesh(new BoxGeometry(w, h, d).translate(b.min[0] + w / 2, b.min[1] + h / 2, b.min[2] + d / 2));
}

function boxScene(boxes) {
  const group = new Group();
  for (const b of boxes) group.add(boxMesh(b));
  group.updateMatrixWorld(true);
  const collider = createWorldCollider();
  collider.set(new Octree().fromGraphNode(group), null);
  return collider;
}

const FLOOR = { min: [-100, -config.core.floorThickness, -100], max: [100, 0, 100], kind: 'floor' };
const WALL = { min: [-1, 0, -60], max: [0, 10, 10], kind: 'wall' };
const SCENES = {
  flat: { description: 'one floor box, top at y = 0', boxes: [FLOOR] },
  wall: { description: 'the flat floor plus one wall box (its +x face at x = 0, 10 m tall, z -60..10)', boxes: [FLOOR, WALL] },
  pit: {
    description: 'two platforms (tops at y = 0) with a 40 m gap between them and nothing below (a swing needs room under the anchor; the kill plane is not simulated)',
    boxes: [{ min: [-10, -2, -10], max: [10, 0, 10], kind: 'floor' }, { min: [-10, -2, -90], max: [10, 0, -50], kind: 'floor' }],
  },
};

let spireArena = null;
function spire() {
  if (!spireArena) {
    spireArena = createArena(new Scene(), config);
    spireArena.load('spire'); // the core is left at its load preset ('flat': no pillar collides)
  }
  return spireArena;
}
/** The Spire's 20° ramp whose low end is at (-5, -26.5) (world/maps/spire.js, rotation k = 0). */
function spireRamp() {
  const map = spire().map;
  const w = map.wedges.reduce((a, b) => (Math.hypot(b.x + 5, b.z + 26.5) < Math.hypot(a.x + 5, a.z + 26.5) ? b : a));
  const ax = -Math.sin(w.yaw);
  const az = -Math.cos(w.yaw);
  return { wedge: w, ax, az, index: map.wedges.indexOf(w) };
}

/** Input helpers: a script returns {moveX, moveZ, yaw, pitch, held: [button...], grappleTarget}. */
const between = (t, a, b) => t >= a - 1e-9 && t < b - 1e-9;

const SCENARIOS = [
  {
    name: 'walk', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 3.5,
    description: 'stand, hold forward for 2 s (Quake ground accel to walk speed), release (friction to a stop)',
    script: ({ t }) => ({ moveZ: between(t, SETTLE, SETTLE + 2) ? 1 : 0 }),
  },
  {
    name: 'walk_diagonal', scene: 'flat', start: [0, 0, 0], yaw: 0.3, seconds: 2.5,
    description: 'forward + right held (the wish direction is normalized), yaw 0.3 rad',
    script: ({ t }) => (t >= SETTLE ? { moveX: 1, moveZ: 1 } : {}),
  },
  {
    name: 'run_jump_double', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 4,
    description: 'there is no sprint in SKYGRIND (walk speed is the ground cap): run 1 s, jump (held 0.4 s), double jump in the air (held 0.3 s), land and keep running',
    script: ({ t }) => ({
      moveZ: t >= SETTLE ? 1 : 0,
      held: [...(between(t, SETTLE + 1, SETTLE + 1.4) || between(t, SETTLE + 1.6, SETTLE + 1.9) ? ['jump'] : [])],
    }),
  },
  {
    name: 'short_hop', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 2.5,
    description: 'run, tap jump for one step (the release cut: vy x releaseMult while rising)',
    script: ({ t }) => ({ moveZ: t >= SETTLE ? 1 : 0, held: between(t, SETTLE + 1, SETTLE + 1 + DT) ? ['jump'] : [] }),
  },
  {
    name: 'bunny_hop', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 5,
    description: 'forward + slight right strafe, jump re-pressed on every landing step (held 0.25 s each); yaw turns at 0.4 rad/s while airborne',
    script: ({ t, mover, mem }) => {
      if (t < SETTLE) return {};
      mem.yaw ??= 0;
      if (!mover.grounded) mem.yaw += 0.4 * DT;
      if (t >= SETTLE + 0.5 && mover.grounded && !(mem.until > t)) mem.until = t + 0.25;
      return { moveZ: 1, moveX: 0.3, yaw: mem.yaw, held: mem.until > t ? ['jump'] : [] };
    },
  },
  {
    name: 'slide', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 4,
    description: 'run 1 s, hold slide 1.5 s (floor speed, then slide friction), release, keep running',
    script: ({ t }) => ({ moveZ: t >= SETTLE ? 1 : 0, held: between(t, SETTLE + 1, SETTLE + 2.5) ? ['slide'] : [] }),
  },
  {
    name: 'slide_jump', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 3.5,
    description: 'run, slide at 1 s, jump out of the slide at 1.5 s (x jumpHorizontalMult below the cap)',
    script: ({ t }) => ({
      moveZ: t >= SETTLE ? 1 : 0,
      held: [...(between(t, SETTLE + 1, SETTLE + 1.5) ? ['slide'] : []), ...(between(t, SETTLE + 1.5, SETTLE + 1.8) ? ['jump'] : [])],
    }),
  },
  {
    name: 'dash', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 5,
    description: 'a standing dash (no input: along the facing), then run and dash, then press dash four times 0.3 s apart (pips run out: dashFail), stamina regen',
    script: ({ t }) => {
      const presses = [SETTLE + 0.1, SETTLE + 1.6, SETTLE + 2.2, SETTLE + 2.5, SETTLE + 2.8, SETTLE + 3.1];
      return { moveZ: t >= SETTLE + 1 ? 1 : 0, held: presses.some((p) => between(t, p, p + 0.05)) ? ['dash'] : [] };
    },
  },
  {
    name: 'dash_jump', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 3,
    description: 'run, dash at 0.5 s, jump 0.1 s into the dash (dash jump: v0 x 0.8, lighter gravity for 0.5 s)',
    script: ({ t }) => ({
      moveZ: t >= SETTLE ? 1 : 0,
      held: [...(between(t, SETTLE + 0.5, SETTLE + 0.55) ? ['dash'] : []), ...(between(t, SETTLE + 0.6, SETTLE + 1.1) ? ['jump'] : [])],
    }),
  },
  {
    name: 'slam_boost', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 3.5,
    description: 'jump, slam (slide pressed in the air near the apex), then jump on the step after the slam landing (boost jump)',
    script: ({ t, mem, lastEvents }) => {
      if (lastEvents.some((e) => e.type === 'slam')) mem.boostFrom = t;
      const boost = mem.boostFrom !== undefined && between(t, mem.boostFrom, mem.boostFrom + 0.5);
      return {
        moveZ: between(t, SETTLE, SETTLE + 0.6) ? 1 : 0,
        held: [...(between(t, SETTLE + 0.5, SETTLE + 0.9) || boost ? ['jump'] : []), ...(between(t, SETTLE + 0.9, SETTLE + 0.95) ? ['slide'] : [])],
      };
    },
  },
  {
    name: 'wall_ride', scene: 'wall', start: [1.2, 0, 8], yaw: 0.25, seconds: 4,
    description: 'run along the wall angled 0.25 rad into it, jump at 0.6 s, keep holding forward: the ride starts on contact and ends on its timer',
    script: ({ t }) => ({ moveZ: t >= SETTLE ? 1 : 0, held: between(t, SETTLE + 0.6, SETTLE + 0.9) ? ['jump'] : [] }),
  },
  {
    name: 'wall_jump', scene: 'wall', start: [1.2, 0, 8], yaw: 0.25, seconds: 4,
    description: 'as wall_ride, then a wall jump 0.3 s after each ride starts (holding forward brings you back to the wall)',
    script: ({ t, mem, lastEvents }) => {
      if (lastEvents.some((e) => e.type === 'wallRideStart')) mem.jumpAt = t + 0.3;
      const wallJump = mem.jumpAt !== undefined && between(t, mem.jumpAt, mem.jumpAt + 0.1);
      return { moveZ: t >= SETTLE ? 1 : 0, held: between(t, SETTLE + 0.6, SETTLE + 0.9) || wallJump ? ['jump'] : [] };
    },
  },
  {
    name: 'swing', scene: 'pit', start: [0, 0, 8], yaw: 0, seconds: 8, rope: true,
    description: 'ARCLINE on (mover.ropeEnabled): run off platform A (jump at its edge), fire the hook one step later at an anchor 15 m up over the middle of the gap, hold grapple + forward (the pump) through the bottom of the arc, let go on the rising side once the feet are 0.5 m above the platform height (z < -40), fly and land on platform B',
    anchor: { x: 0, y: 15, z: -30, node: null },
    script: ({ t, mover, mem }, sc) => {
      if (t < SETTLE) return {};
      const p = mover.position;
      if (mem.jumpAt === undefined && p.z < -9) mem.jumpAt = t;
      if (mem.jumpAt !== undefined && mem.hookAt === undefined && !mover.grounded && p.z < -10.5) mem.hookAt = t;
      if (mem.hookAt !== undefined && mem.letGoAt === undefined && mover.rope.phase === 'attached' && mover.velocity.y > 0 && p.y > 0.5 && p.z < -40) mem.letGoAt = t;
      const grapple = mem.hookAt !== undefined && mem.letGoAt === undefined;
      const jump = mem.jumpAt !== undefined && between(t, mem.jumpAt, mem.jumpAt + 0.3);
      return { moveZ: 1, held: [...(jump ? ['jump'] : []), ...(grapple ? ['grapple'] : [])], grappleTarget: grapple ? sc.anchor : null };
    },
  },
  {
    name: 'swing_from_ground', scene: 'flat', start: [0, 0, 0], yaw: 0, seconds: 4.5, rope: true,
    description: 'ARCLINE on flat ground: run, jump, hook an anchor 16 m up and 18 m ahead at 0.6 s. The arc\'s low point is under the floor, so you land and the rope pays out on the ground (mover.js _stepRope) until you let go at 2.6 s',
    anchor: { x: 0, y: 16, z: -18, node: null },
    script: ({ t }, sc) => {
      const grapple = between(t, SETTLE + 0.6, SETTLE + 2.6);
      return { moveZ: t >= SETTLE ? 1 : 0, held: [...(between(t, SETTLE + 0.4, SETTLE + 0.7) ? ['jump'] : []), ...(grapple ? ['grapple'] : [])], grappleTarget: grapple ? sc.anchor : null };
    },
  },
  {
    name: 'ramp_walk_up', scene: 'spireRamp', seconds: 2.5,
    description: 'the Spire map (real arena), its 20° ramp at (-5, -26.5): start 3 m before the low end, walk straight up it',
    place: () => {
      const { wedge: w, ax, az } = spireRamp();
      return { start: [w.x - ax * 3, 0, w.z - az * 3], yaw: Math.atan2(-ax, -az) };
    },
    script: ({ t }) => ({ moveZ: t >= SETTLE ? 1 : 0 }),
  },
  {
    name: 'ramp_slide_down', scene: 'spireRamp', seconds: 2,
    description: 'the same ramp: start on the slope 7 m up it, facing downhill, hold forward + slide (slope boost). Pads are not simulated here (arena.checkPads is main.js\'s job), so the core-edge pad at (0, 0, -26.5) does not launch',
    place: () => {
      const { wedge: w, ax, az } = spireRamp();
      const along = 7;
      return { start: [w.x + ax * along, (w.rise * along) / w.length + 0.02, w.z + az * along], yaw: Math.atan2(ax, az) };
    },
    script: ({ t }) => (t >= SETTLE ? { moveZ: 1, held: ['slide'] } : {}),
  },
];

function run(sc) {
  let collider;
  let sceneInfo;
  if (sc.scene === 'spireRamp') {
    collider = spire().octree;
    const { wedge, index } = spireRamp();
    sceneInfo = { kind: 'map', map: 'spire', corePreset: 'flat (the load preset)', wedgeIndex: index, wedge, note: 'the whole Spire collider (static Octree + core grid); see maps/spire.json' };
  } else {
    collider = boxScene(SCENES[sc.scene].boxes);
    sceneInfo = { kind: 'boxes', description: SCENES[sc.scene].description, boxes: SCENES[sc.scene].boxes };
  }
  const placed = sc.place ? sc.place() : { start: sc.start, yaw: sc.yaw };
  const events = [];
  const mover = new Mover(config, collider, { events: { emit: (type, payload) => events.push({ type, ...payload }) }, owner: 'player' });
  mover.ropeEnabled = !!sc.rope;
  mover.reset(...placed.start);
  const frame = createInputFrame();
  const prevHeld = new Set();
  const mem = {};
  let lastEvents = [];
  const rows = [];
  const steps = Math.round(sc.seconds * config.loop.simHz);
  for (let i = 0; i < steps; i++) {
    const t = i * DT;
    const want = sc.script({ t, step: i, mover, mem, lastEvents }, sc);
    frame.moveX = want.moveX ?? 0;
    frame.moveZ = want.moveZ ?? 0;
    frame.yaw = placed.yaw + (want.yaw ?? 0);
    frame.pitch = want.pitch ?? 0;
    const held = new Set(want.held ?? []);
    const pressed = [];
    for (const b of BUTTONS) {
      frame[b].held = held.has(b);
      frame[b].pressed = held.has(b) && !prevHeld.has(b);
      if (frame[b].pressed) pressed.push(b);
    }
    frame.grappleTarget = want.grappleTarget ?? null;
    prevHeld.clear();
    for (const b of held) prevHeld.add(b);
    events.length = 0;
    mover.step(frame, DT);
    lastEvents = events.map((e) => ({ ...e }));
    const row = {
      step: i + 1,
      t: (i + 1) * DT,
      input: { moveX: frame.moveX, moveZ: frame.moveZ, yaw: frame.yaw, pitch: frame.pitch, held: [...held], pressed },
      position: [mover.position.x, mover.position.y, mover.position.z],
      velocity: [mover.velocity.x, mover.velocity.y, mover.velocity.z],
      state: mover.state,
      grounded: mover.grounded,
      crouched: mover.crouched,
      pips: mover.pips,
      touchingWall: mover.touchingWall,
      gravityMultiplier: mover.gravityMultiplier,
      events: lastEvents,
    };
    if (sc.rope) row.rope = { phase: mover.rope.phase, L: mover.rope.L, taut: mover.rope.taut };
    rows.push(enc(row));
  }
  const states = [...new Set(rows.map((r) => r.state))];
  const eventTypes = [...new Set(rows.flatMap((r) => r.events.map((e) => e.type)))];
  const file = writeJson(`trajectories/${sc.name}.json`, {
    name: sc.name,
    description: sc.description,
    source: 'src/movement/mover.js (Mover.step), src/world/collider.js, three/addons/math/Octree.js',
    generatedBy: 'port-export/scripts/trajectories.mjs',
    dt: DT,
    simHz: config.loop.simHz,
    configNote: 'every number comes from port-export/config.json (the unmodified game config)',
    scene: sceneInfo,
    start: { position: placed.start, yaw: placed.yaw, how: 'mover.reset(x, y, z): state AIR, zero velocity, full pips; then one Mover.step per row', settleSeconds: SETTLE },
    ropeEnabled: !!sc.rope,
    anchor: sc.anchor ?? null,
    statesSeen: states,
    eventsSeen: eventTypes,
    steps: rows.length,
    rows,
  });
  return { name: sc.name, steps: rows.length, states, events: eventTypes, bytes: file.bytes };
}

export function exportTrajectories() {
  return SCENARIOS.map(run);
}

if (process.argv[1]?.endsWith('trajectories.mjs')) {
  for (const r of exportTrajectories()) console.log(`trajectories/${r.name}.json: ${r.steps} steps, states ${r.states.join('/')}, events ${r.events.join(', ')}`);
}
