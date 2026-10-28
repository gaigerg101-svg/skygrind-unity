// Self-check: the exported files are enough to reproduce the game's results.
// - vectors: every recorded call is decoded from JSON alone, re-run on the real function and compared exactly
//   (result, and the mutated arguments when recorded). Calls with an injected callback are skipped (only a
//   description of the callback is stored).
// - trajectories: each run is replayed from its recorded per-step input only (no script), on a fresh Mover, and
//   every step's position, velocity, state and events are compared exactly.
// Run: node port-export/scripts/verify.mjs
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import { BoxGeometry, Group, Mesh, Scene } from 'three';
import { Octree } from 'three/addons/math/Octree.js';
import { EXPORT_DIR, config, enc, seededRng } from './lib.mjs';
import { Mover, createInputFrame } from '../../src/movement/mover.js';
import { createWorldCollider } from '../../src/world/collider.js';
import { createArena } from '../../src/world/arena.js';

const MODULES = {
  movementMath: '../../src/movement/movementMath.js', swingMath: '../../src/movement/swingMath.js', styleMath: '../../src/style/styleMath.js',
  styleSim: '../../src/style/styleSim.js', directorMath: '../../src/ai/directorMath.js', spawnMath: '../../src/enemies/spawnMath.js',
  combatMath: '../../src/combat/combatMath.js', weaponMath: '../../src/combat/weaponMath.js', gunMath: '../../src/combat/gunMath.js',
  arsenalMath: '../../src/combat/arsenalMath.js', momentumMath: '../../src/combat/momentumMath.js', assistMath: '../../src/combat/assistMath.js',
};

class Skip extends Error {}

/** Inverse of lib.mjs enc(). */
function dec(v) {
  if (Array.isArray(v)) return v.map(dec);
  if (!v || typeof v !== 'object') return v;
  if ('$num' in v) return v.$num === '-0' ? -0 : Number(v.$num);
  if ('$undefined' in v) return undefined;
  if ('$config' in v) return v.$config === '' ? config : v.$config.split('.').reduce((o, k) => o[k], config);
  if ('$fn' in v) {
    if (v.$fn?.$rng === 'mulberry32') {
      if (v.$fn.draws !== 0) throw new Skip('rng recorded mid-stream');
      return seededRng(v.$fn.seed);
    }
    throw new Skip('injected callback');
  }
  if ('$map' in v) return new Map(v.$map.map(([k, x]) => [dec(k), dec(x)]));
  if ('$set' in v) return new Set(v.$set.map(dec));
  const out = {};
  for (const k of Object.keys(v)) out[k] = dec(v[k]);
  return out;
}

async function verifyVectors() {
  let pass = 0;
  let skip = 0;
  const fails = [];
  for (const [name, path] of Object.entries(MODULES)) {
    const mod = await import(path);
    const file = JSON.parse(readFileSync(join(EXPORT_DIR, 'vectors', `${name}.json`), 'utf8'));
    for (const c of file.cases) {
      if (!c.fn || !c.args) continue; // constants and sequences are checked by regeneration, not here
      if (c.fn === 'mulberry32') continue;
      let args;
      try {
        args = dec(c.args);
      } catch (e) {
        if (e instanceof Skip) {
          skip++;
          continue;
        }
        throw e;
      }
      const result = enc(mod[c.fn](...args));
      const same = JSON.stringify(result) === JSON.stringify(c.result);
      const afterSame = !c.argsAfter || JSON.stringify(args.map(enc).map((a) => stripDraws(a))) === JSON.stringify(c.argsAfter.map((a) => stripDraws(a)));
      if (same && afterSame) pass++;
      else fails.push(`${name}.${c.fn}`);
    }
  }
  return { pass, skip, fails };
}
const stripDraws = (a) => JSON.parse(JSON.stringify(a, (k, v) => (k === 'draws' ? undefined : v)));

function colliderFor(scene) {
  if (scene.kind === 'map') {
    const arena = createArena(new Scene(), config);
    arena.load(scene.map);
    return arena.octree;
  }
  const group = new Group();
  for (const b of scene.boxes) {
    const w = b.max[0] - b.min[0];
    const h = b.max[1] - b.min[1];
    const d = b.max[2] - b.min[2];
    group.add(new Mesh(new BoxGeometry(w, h, d).translate(b.min[0] + w / 2, b.min[1] + h / 2, b.min[2] + d / 2)));
  }
  group.updateMatrixWorld(true);
  const collider = createWorldCollider();
  collider.set(new Octree().fromGraphNode(group), null);
  return collider;
}

function verifyTrajectories() {
  const results = [];
  for (const f of readdirSync(join(EXPORT_DIR, 'trajectories')).sort()) {
    const t = JSON.parse(readFileSync(join(EXPORT_DIR, 'trajectories', f), 'utf8'));
    const events = [];
    const mover = new Mover(config, colliderFor(t.scene), { events: { emit: (type, payload) => events.push({ type, ...payload }) }, owner: 'player' });
    mover.ropeEnabled = t.ropeEnabled;
    mover.reset(...t.start.position);
    const frame = createInputFrame();
    let firstBad = -1;
    for (const row of t.rows) {
      const inp = dec(row.input);
      frame.moveX = inp.moveX;
      frame.moveZ = inp.moveZ;
      frame.yaw = inp.yaw;
      frame.pitch = inp.pitch;
      for (const b of ['jump', 'dash', 'slide', 'fire', 'altFire', 'special', 'punch', 'interact', 'grapple', 'reel']) {
        frame[b].held = inp.held.includes(b);
        frame[b].pressed = inp.pressed.includes(b);
      }
      frame.grappleTarget = inp.held.includes('grapple') ? t.anchor : null; // the documented rule
      events.length = 0;
      mover.step(frame, t.dt);
      const got = enc({
        position: [mover.position.x, mover.position.y, mover.position.z],
        velocity: [mover.velocity.x, mover.velocity.y, mover.velocity.z],
        state: mover.state,
        events,
      });
      const want = { position: row.position, velocity: row.velocity, state: row.state, events: row.events };
      if (firstBad < 0 && JSON.stringify(got) !== JSON.stringify(want)) firstBad = row.step;
    }
    results.push({ name: t.name, steps: t.rows.length, firstMismatchStep: firstBad });
  }
  return results;
}

const v = await verifyVectors();
console.log(`vectors: ${v.pass} calls reproduced exactly, ${v.fails.length} mismatched, ${v.skip} skipped (injected callbacks)`);
if (v.fails.length) console.log('  mismatches:', v.fails.join(', '));
const tr = verifyTrajectories();
for (const r of tr) console.log(`trajectory ${r.name}: ${r.firstMismatchStep < 0 ? `all ${r.steps} steps reproduced exactly` : `MISMATCH from step ${r.firstMismatchStep}`}`);
process.exitCode = v.fails.length || tr.some((r) => r.firstMismatchStep >= 0) ? 1 : 0;
