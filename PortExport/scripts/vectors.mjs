// B. Golden vectors: the real pure functions called with representative and edge-case inputs. Each file holds the
// arguments before the call, the return value and (when the function mutates an argument) the arguments after it.
// Run: node port-export/scripts/vectors.mjs
import { config, createRecorder, namedFn, seededRng } from './lib.mjs';
import * as MM from '../../src/movement/movementMath.js';
import * as SW from '../../src/movement/swingMath.js';
import * as SM from '../../src/style/styleMath.js';
import { createStyleCore } from '../../src/style/styleCore.js';
import { paceReport, simulateStyle } from '../../src/style/styleSim.js';
import * as DM from '../../src/ai/directorMath.js';
import * as SP from '../../src/enemies/spawnMath.js';
import * as CM from '../../src/combat/combatMath.js';
import * as WM from '../../src/combat/weaponMath.js';
import * as GM from '../../src/combat/gunMath.js';
import * as AM from '../../src/combat/arsenalMath.js';
import * as MO from '../../src/combat/momentumMath.js';
import * as AS from '../../src/combat/assistMath.js';

const DT = 1 / config.loop.simHz;
const G = MM.gravityFor(config.jump.height, config.jump.apexTime);
const DEG = Math.PI / 180;
const v3 = (x, y, z) => ({ x, y, z });
const norm = (x, y, z) => {
  const l = Math.hypot(x, y, z);
  return v3(x / l, y / l, z / l);
};
const extra = { dt: DT, simHz: config.loop.simHz, gravity: G, gravityNote: 'gravityFor(config.jump.height, config.jump.apexTime)' };

function movementMath() {
  const r = createRecorder('src/movement/movementMath.js', extra);
  r.add({ constant: 'MoveState', value: MM.MoveState });
  for (const [h, t] of [[config.jump.height, config.jump.apexTime], [1.3, 0.3], [1.4, 0.3], [0, 0.36], [8, 0.36]]) {
    r.call('jumpVelocity', MM.jumpVelocity, [h, t]);
    r.call('gravityFor', MM.gravityFor, [h, t]);
  }
  for (const s of [0, 0.001, 0.1, 0.15, 0.2, 0.25, 0.35, 0.5, 1.2, 3]) r.call('stepsFor', MM.stepsFor, [s, DT]);
  r.call('stepsFor', MM.stepsFor, [0.2, 1 / 60], 'another step size');
  const v0 = MM.jumpVelocity(config.jump.height, config.jump.apexTime);
  for (const [vy, g] of [[v0, G], [0, G], [0, G * config.jump.fallGravityMult], [-20, G], [5, 0], [1.9, G * config.jump.apexHangMult]]) {
    r.call('verticalStep', MM.verticalStep, [vy, g, DT]);
  }
  const flagSets = [
    { jumpHeld: false, jumpAirtime: false, windActive: false },
    { jumpHeld: true, jumpAirtime: true, windActive: false },
    { jumpHeld: true, jumpAirtime: false, windActive: false },
    { jumpHeld: false, jumpAirtime: true, windActive: true },
  ];
  for (const state of Object.values(MM.MoveState)) {
    for (const vy of [5, 1.5, 0, -1.5, -10]) {
      for (const flags of flagSets) r.call('gravityScale', MM.gravityScale, [state, vy, flags, config]);
    }
  }
  const wishes = [v3(0, 0, -1), v3(1, 0, 0), norm(1, 0, -1)];
  const vels = [v3(0, 0, 0), v3(0, 0, -4), v3(0, 0, -8), v3(0, 0, -12), v3(8, 0, 0), v3(10, 3, -5), v3(-6, -2, 6)];
  for (const wish of wishes) {
    for (const vel of vels) {
      r.call('accelerate', MM.accelerate, [vel, wish, config.ground.walkSpeed, config.ground.accel, DT], 'ground');
      r.call('accelerate', MM.accelerate, [vel, wish, config.ground.walkSpeed, config.air.accel, DT], 'air');
    }
  }
  r.call('accelerate', MM.accelerate, [v3(3, 0, 0), v3(0, 0, 0), 0, config.ground.accel, DT], 'no input');
  for (const vel of [v3(10, 0, 0), v3(0, -3, 2), v3(1, 0, 1), v3(0.01, 0, 0), v3(0, 5, 0), v3(20, 0, -20)]) {
    r.call('applyFriction', MM.applyFriction, [vel, config.ground.friction, config.ground.stopSpeed, DT], 'ground');
    r.call('applyFriction', MM.applyFriction, [vel, config.slide.friction, 0, DT], 'slide above the floor speed');
  }
  for (const [e, d] of [[0, 24], [8, 24], [30, 24]]) r.call('dashSpeed', MM.dashSpeed, [e, d]);
  for (const [w, b] of [[8, 0], [8, 5], [8, 12]]) r.call('dashExitSpeed', MM.dashExitSpeed, [w, b]);
  for (const s of [0, 8, 11.6, 19, 20, 23.9, 24, 30]) {
    r.call('slideJumpSpeed', MM.slideJumpSpeed, [s, config.slide.jumpHorizontalMult, config.slide.jumpBoostMaxSpeed]);
  }
  for (const [p, dashing] of [[0, false], [1.5, false], [2.999, false], [3, false], [1, true]]) {
    r.call('regenPips', MM.regenPips, [p, config.dash.regenPerSec, config.dash.maxPips, DT, dashing]);
  }
  for (const vy of [6, 0.5, 0, -2]) {
    for (const held of [true, false]) {
      for (const armed of [true, false]) r.call('releaseJump', MM.releaseJump, [vy, held, config.jump.releaseMult, armed]);
    }
  }
  const base = { grounded: false, coyoteSteps: 0, doubleJumpsLeft: 1, landingSoon: false, wallNear: false, wallJumpsLeft: 3, boostSteps: 0, pips: 3, dashJumpCost: config.dashJump.extraPipCost };
  const ctxs = [
    { state: 'GROUND', grounded: true }, { state: 'GROUND', grounded: true, boostSteps: 5 }, { state: 'SLIDE', grounded: true },
    { state: 'SLIDE', grounded: true, boostSteps: 5 }, { state: 'DASH' }, { state: 'DASH', pips: 0, grounded: true }, { state: 'DASH', pips: 0 },
    { state: 'AIR', coyoteSteps: 4 }, { state: 'AIR' }, { state: 'AIR', doubleJumpsLeft: 0 }, { state: 'AIR', landingSoon: true },
    { state: 'AIR', wallNear: true }, { state: 'AIR', wallNear: true, wallJumpsLeft: 0 }, { state: 'WALLRIDE' }, { state: 'WALLRIDE', wallJumpsLeft: 0 },
    { state: 'SLAM' }, { state: 'SWING' },
  ];
  for (const c of ctxs) r.call('resolveJump', MM.resolveJump, [{ ...base, ...c }]);
  const n = v3(1, 0, 0);
  for (const vel of [v3(0, 0, -10), v3(-3, 0, -10), v3(5, 2, 5), v3(-8, 0, 0)]) {
    r.call('alongWallSpeed', MM.alongWallSpeed, [vel, n]);
    r.call('wallJumpVelocity', MM.wallJumpVelocity, [vel, n, config.wallJump.outSpeed, v0 * config.wallJump.v0Mult]);
  }
  r.call('alongWallSpeed', MM.alongWallSpeed, [v3(3, 0, 4), norm(1, 0, 1)]);
  for (const [dir, has, fwd] of [[v3(-1, 0, 0), true, v3(0, 0, -1)], [v3(1, 0, 0), true, v3(0, 0, -1)], [norm(-1, 0, -1), true, v3(0, 0, -1)], [v3(0, 0, 0), false, v3(0, 0, -1)], [v3(0, 0, 0), false, norm(-1, 0, -1)], [v3(0, 0, 0), false, v3(-1, 0, 0)]]) {
    r.call('wallRideDashDir', MM.wallRideDashDir, [dir, has, fwd, n]);
  }
  for (const h of [0, 1, config.jumpPad.apexHeight]) r.call('padLaunchSpeed', MM.padLaunchSpeed, [h, G]);
  r.call('padLaunchVelocity', MM.padLaunchVelocity, [v3(0, 0, -26.5), v3(0, 9, -39.5), 2.5, G, config.jump.fallGravityMult]);
  r.call('padLaunchVelocity', MM.padLaunchVelocity, [v3(0, 6, 0), v3(10, 0, 0), 1, G, config.jump.fallGravityMult]);
  for (const [vy, t] of [[0, 0.1], [-10, 0.1], [5, 0.1], [20, 0.1], [0, 0]]) r.call('fallDistance', MM.fallDistance, [vy, G, t]);
  for (const yaw of [0, Math.PI / 2, -Math.PI / 2, Math.PI, 0.3, -2.5]) {
    for (const [mx, mz] of [[0, 1], [1, 0], [-1, 0], [0, -1], [1, 1], [-0.5, 0.25], [0, 0]]) r.call('wishDirection', MM.wishDirection, [mx, mz, yaw]);
  }
  for (const vel of [v3(3, 4, 4), v3(0, -9, 0)]) r.call('horizontalSpeed', MM.horizontalSpeed, [vel]);
  for (const vel of [v3(30, 5, 40), v3(3, 5, 4), v3(40, 0, 0), v3(0, 99, 0)]) r.call('clampHorizontal', MM.clampHorizontal, [vel, config.speed.maxHorizontal]);
  const s20 = Math.sin(20 * DEG);
  const c20 = Math.cos(20 * DEG);
  for (const [nrm, dir, label] of [[v3(s20, c20, 0), v3(1, 0, 0), 'downhill'], [v3(s20, c20, 0), v3(-1, 0, 0), 'uphill'], [v3(0, 1, 0), v3(1, 0, 0), 'flat'], [v3(s20, c20, 0), v3(0, 0, 1), 'across']]) {
    r.call('slopeSlideAccel', MM.slopeSlideAccel, [nrm, dir, G, config.slide.slopeBoost], label);
  }
  // A composite the Mover runs every airborne step: a held jump's vertical arc (gravityScale + verticalStep only).
  const arc = [];
  let vy = v0;
  let y = 0;
  const flags = { jumpHeld: true, jumpAirtime: true, windActive: false };
  for (let i = 0; i < 200 && y >= 0; i++) {
    const mult = MM.gravityScale('AIR', vy, flags, config);
    const out = MM.verticalStep(vy, G * mult, DT);
    vy = out.vy;
    y += out.dy;
    arc.push([i + 1, mult, vy, y]);
  }
  r.add({ sequence: 'heldJumpVerticalArc', note: 'AIR, Space held, jump airtime: each step mult = gravityScale(AIR, vy, flags, config), then verticalStep(vy, G*mult, dt); columns [step, mult, vy, y]', v0, rows: arc });
  return r.save('vectors/movementMath.json');
}

function swingMath() {
  const r = createRecorder('src/movement/swingMath.js', { ...extra, substepH: DT / config.arcline.substeps });
  const p = SW.swingParams(config.arcline, G);
  r.call('swingParams', SW.swingParams, [config.arcline, G]);
  r.call('ropeDistance', SW.ropeDistance, [v3(3, 4, 12), v3(0, 0, 0)]);
  r.call('swingEnergy', SW.swingEnergy, [v3(0, 10, 0), v3(3, 4, 0), p.g]);
  const h = DT / config.arcline.substeps;
  const anchor = v3(0, 20, 0);
  const one = (pos, vel, L, input, note) => r.call('swingStep', SW.swingStep, [{ pos, vel, L }, anchor, input, p, h], note);
  one(v3(0, 5, 0), v3(10, 0, 0), 15, { wish: null, reel: false, pulling: false }, 'taut at the bottom');
  one(v3(0, 10, 0), v3(10, 0, 0), 15, { wish: null, reel: false, pulling: false }, 'slack');
  one(v3(0, 5, 0), v3(10, 0, 0), 15, { wish: v3(1, 0, 0), reel: false, pulling: false }, 'pump at the bottom');
  one(v3(0, 5, 0), v3(10, 0, 0), 15, { wish: null, reel: true, pulling: false }, 'reel while taut');
  one(v3(0, 5, 0), v3(10, 0, 0), 15, { wish: null, reel: false, pulling: true }, 'pull-in');
  one(v3(0, 4, 0), v3(0, -10, 0), 15, { wish: null, reel: false, pulling: false }, 'past the stretch limit: projected');
  one(v3(15, 20, 0), v3(0, 0, 0), 15, { wish: null, reel: false, pulling: false }, 'horizontal, at rest');
  one(v3(0, 5, 0), v3(45, 0, 0), 15, { wish: v3(1, 0, 0), reel: false, pulling: false }, 'pump past the soft cap');
  one(v3(0, 16, 0), v3(5, 0, 0), config.arcline.minLen, { wish: null, reel: true, pulling: false }, 'reel at minLen');
  // A full swing: released horizontally, 2 s of sub-steps (swingStep only, no collision), then a pumped run with a reel.
  for (const [name, input, seconds, pos] of [
    ['freePendulum', { wish: null, reel: false, pulling: false }, 2, v3(15, 20, 0)],
    ['pumpAndReel', { wish: v3(1, 0, 0), reel: true, pulling: false }, 1, v3(0, 5, 0)],
  ]) {
    const s = { pos: { ...pos }, vel: v3(0, 0, 0), L: 15 };
    const rows = [];
    for (let i = 0; i < seconds * config.loop.simHz * config.arcline.substeps; i++) {
      const out = SW.swingStep(s, anchor, input, p, h);
      rows.push([i + 1, s.pos.x, s.pos.y, s.pos.z, s.vel.x, s.vel.y, s.vel.z, s.L, out.taut, out.stretch]);
    }
    r.add({ sequence: name, note: 'repeated swingStep(s, anchor, input, params, h); columns [call, pos.x, pos.y, pos.z, vel.x, vel.y, vel.z, L, taut, stretch]', start: { pos, vel: v3(0, 0, 0), L: 15 }, anchor, input, params: p, h, rows });
  }
  for (const [vel, j] of [[v3(10, 5, 0), 0], [v3(10, -5, 0), 0], [v3(10, 5, 0), config.arcline.jumpUp], [v3(10, -5, 0), config.arcline.jumpUp]]) {
    r.call('releaseVelocity', SW.releaseVelocity, [vel, config.arcline.risingBoost, j]);
  }
  for (const d of [0, 5, 36, 60, 100]) r.call('hookTime', SW.hookTime, [d, config.arcline]);
  const eye = v3(0, 1.6, 0);
  const cands = [
    { point: v3(0, 10, -30), node: false }, { point: v3(2, 12, -30), node: true }, { point: v3(0, -5, -20), node: false },
    { point: v3(40, 10, 0), node: true }, { point: v3(0, 1.6, -80), node: true },
  ];
  r.call('pickAnchor', SW.pickAnchor, [cands, eye, norm(0, 0.2, -1), config.arcline.coneDeg * DEG, config.arcline.range]);
  r.call('pickAnchor', SW.pickAnchor, [cands, eye, v3(1, 0, 0), config.arcline.coneDeg * DEG, config.arcline.range]);
  r.call('pickAnchor', SW.pickAnchor, [[], eye, v3(0, 0, -1), config.arcline.coneDeg * DEG, config.arcline.range]);
  return r.save('vectors/swingMath.json');
}

function styleMath() {
  const s = config.style;
  const r = createRecorder('src/style/styleMath.js');
  r.add({ constant: 'FRESH_WEAPONS', value: SM.FRESH_WEAPONS });
  for (const w of ['revolver', 'ricochet', 'orb', 'slam', 'rattler', 'bogus']) r.call('freshKey', SM.freshKey, [w]);
  r.call('createFreshness', SM.createFreshness, [s]);
  r.call('useFreshness', SM.useFreshness, [SM.createFreshness(s), 'revolver', s]);
  r.call('useFreshness', SM.useFreshness, [SM.createFreshness(s), 'nailgun', s, s.freshness.nailgunUseScale]);
  r.call('useFreshness', SM.useFreshness, [{ revolver: 0.3, shotgun: 9.8, slam: 5, punch: 0, nailgun: 10, rocket: 7.5, rail: 2 }, 'revolver', s]);
  r.call('regenFreshness', SM.regenFreshness, [{ revolver: 0, shotgun: 9.99, slam: 5, punch: 0, nailgun: 10, rocket: 7.5, rail: 2 }, 0.5, s]);
  for (const v of [0, 4.99, 5, 7.49, 7.5, 10]) {
    r.call('freshnessTierName', SM.freshnessTierName, [v, s]);
    r.call('freshnessTier', SM.freshnessTier, [v, s]);
  }
  for (const st of ['GROUND', 'AIR', 'DASH', 'WALLRIDE', 'SLAM', 'SLIDE', 'SWING']) r.call('stateMultiplier', SM.stateMultiplier, [st, s]);
  const kills = [
    { kind: 'husk', weapon: 'revolver', state: 'GROUND', distance: 12, recentKills: 1 },
    { kind: 'husk', weapon: 'revolver', state: 'AIR', distance: 1.5, recentKills: 2 },
    { kind: 'drone', weapon: 'punch', state: 'WALLRIDE', distance: 1, recentKills: 1 },
    { kind: 'husk', weapon: 'sunder', state: 'SLIDE', distance: 1.2, recentKills: 3, melee: true },
    { kind: 'drone', weapon: 'orb', state: 'GROUND', distance: 20, recentKills: 1 },
    { kind: 'charger', weapon: 'ricochet', state: 'DASH', distance: 8, recentKills: 1 },
    { kind: 'swarmer', weapon: 'discus', state: 'GROUND', distance: 6, recentKills: 1, bounced: true },
    { kind: 'husk', weapon: 'slam', state: 'SLAM', distance: 3, recentKills: 1 },
    { kind: 'bogus', weapon: 'rocket', state: 'SWING', distance: 2, recentKills: 1 },
  ];
  for (const k of kills) r.call('killAwards', SM.killAwards, [k, s]);
  for (const h of [{ weapon: 'revolver', headshot: true }, { weapon: 'shotgun', headshot: true }, { weapon: 'punch' }, { weapon: 'sunder', melee: true }, { weapon: 'torch', pointScale: 0.3 }]) {
    r.call('hitAwards', SM.hitAwards, [h, s]);
  }
  const last = {};
  for (const [trick, now, near] of [['wallJump', 0, true], ['wallJump', 1, true], ['wallJump', 4.5, true], ['dashJump', 5, false], ['dashJump', 9, true], ['bogus', 20, true]]) {
    r.call('trickPoints', SM.trickPoints, [trick, now, last, near, s]);
  }
  for (const [p, a, b] of [[30, 1, 1], [30, 1.2, 1.1], [5, 1.2, 0.5], [12, 1.2, 1.1], [15, 1, 0.5]]) r.call('applyMultipliers', SM.applyMultipliers, [p, a, b]);
  for (const m of [0, 89.99, 90, 239, 240, 470, 800, 1299, 1300, 1800, 5000]) r.call('rankOf', SM.rankOf, [m, s]);
  r.call('meterMax', SM.meterMax, [s]);
  for (const rank of [-1, 0, 1, 2, 3, 4, 5, 6]) {
    r.call('rankBand', SM.rankBand, [rank, s]);
    r.call('rankMid', SM.rankMid, [rank, s]);
  }
  for (const [m, rank] of [[0, 0], [45, 0], [100, 1], [1500, 5], [1800, 5], [50, 1]]) r.call('rankFill', SM.rankFill, [m, rank, s]);
  const times = [];
  for (const now of [0, 1, 2, 3, 4.5, 9, 9.1, 9.2, 9.3, 9.4, 9.5]) r.call('diminish', SM.diminish, [times, now, s.diminish]);
  r.call('diminishKey', SM.diminishKey, ['KILL', 'revolver']);
  r.call('diminishKey', SM.diminishKey, ['WALL JUMP', null]);
  const bucket = { tokens: s.hitCap.burst };
  for (const p of [5, 5, 5, 0]) r.call('takeHitIncome', SM.takeHitIncome, [bucket, p]);
  r.call('refillHitIncome', SM.refillHitIncome, [bucket, 0.25, s.hitCap]);
  r.call('refillHitIncome', SM.refillHitIncome, [bucket, 5, s.hitCap]);
  for (let rank = 0; rank < 6; rank++) {
    r.call('drainRate', SM.drainRate, [rank, s]);
    r.call('staminaRewardMult', SM.staminaRewardMult, [rank, s]);
    r.call('rankPulseRate', SM.rankPulseRate, [rank, s]);
    r.call('rankWarmth', SM.rankWarmth, [rank, s]);
    r.call('rankEnergy', SM.rankEnergy, [rank, 0.5, s]);
  }
  r.call('staminaRewardMult', SM.staminaRewardMult, [9, s]);
  r.call('rankEnergy', SM.rankEnergy, [5, 2, s]);
  const shown = { rank: 0, below: 0 };
  for (const [m, dt] of [[300, DT], [100, 0.5], [100, 0.49], [100, 0.02], [900, DT], [0, 1], [0, 1]]) r.call('stepShownRank', SM.stepShownRank, [shown, m, dt, s]);
  return r.save('vectors/styleMath.json');
}

function styleCore() {
  const s = config.style;
  const events = [];
  const core = createStyleCore(s, {
    onAward: (name, points, quiet) => events.push(['award', name, points, quiet]),
    onRank: (rank, up) => events.push(['rank', rank, up]),
  });
  const log = [];
  const snap = () => ({ meter: core.meter, rank: core.rank, total: core.total, time: core.time, fill: core.fill(), stats: { ...core.stats } });
  const op = (name, fn, args = []) => {
    events.length = 0;
    const result = fn(...args);
    log.push({ op: name, args, result: result === undefined ? null : result, events: [...events], after: snap() });
  };
  const steps = (n) => {
    events.length = 0;
    for (let i = 0; i < n; i++) core.step(DT);
    log.push({ op: 'step', args: [DT], repeat: n, events: [...events], after: snap() });
  };
  op('start', () => core.start());
  op('hit', (h) => core.hit(h), [{ weapon: 'revolver', headshot: true, melee: false, state: 'GROUND', shot: 1, assist: 1, fresh: 1 }]);
  op('kill', (k) => core.kill(k), [{ kind: 'husk', weapon: 'revolver', state: 'AIR', distance: 12, melee: false, assist: 1, fresh: 1 }]);
  steps(60);
  op('hit', (h) => core.hit(h), [{ weapon: 'revolver', headshot: false, melee: false, state: 'GROUND', shot: 1, assist: 1, fresh: 1 }]);
  op('hit', (h) => core.hit(h), [{ weapon: 'punch', melee: true, state: 'SLIDE', shot: 2, assist: 1, fresh: 0.5 }]);
  op('kill', (k) => core.kill(k), [{ kind: 'husk', weapon: 'punch', state: 'SLIDE', distance: 1.5, melee: true, assist: 1, fresh: 0.5 }]);
  op('parry', (p) => core.parry(p), [{ state: 'AIR' }]);
  op('trick', (...a) => core.trick(...a), ['wallJump', 'WALL JUMP', true, 'WALLRIDE']);
  op('trick', (...a) => core.trick(...a), ['wallJump', 'WALL JUMP', true, 'WALLRIDE']);
  op('trick', (...a) => core.trick(...a), ['dashJump', 'DASH JUMP', false, 'AIR']);
  op('cross', (c) => core.cross(c), [{ combo: null, weapon: 'shotgun', state: 'GROUND' }]);
  op('cross', (c) => core.cross(c), [{ combo: 'COLLAPSE', weapon: 'bouncer', state: 'AIR' }]);
  op('swapKill', (c) => core.swapKill(c), [{ weapon: 'rocket', state: 'AIR' }]);
  op('bonus', (...a) => core.bonus(...a), ['IMPACT', s.bonus.impact, { weapon: 'sunder', state: 'DASH', assist: 1 }]);
  for (let i = 0; i < 3; i++) op('kill', (k) => core.kill(k), [{ kind: 'drone', weapon: 'rocket', state: 'AIR', distance: 9, melee: false, assist: 0.8, fresh: 0.9 }]);
  for (let i = 0; i < 20; i++) op('hit', (h) => core.hit(h), [{ weapon: 'rattler', melee: false, state: 'GROUND', shot: 10 + i, assist: 1, fresh: 1 }]);
  op('hurt', () => core.hurt());
  op('setDrainMult', (v) => core.setDrainMult(v), [2]);
  steps(600);
  op('setPerkMods', (v) => core.setPerkMods(v), [{ drain: 0.6, gain: 1.2 }]);
  for (let i = 0; i < 4; i++) {
    op('kill', (k) => core.kill(k), [{ kind: 'charger', weapon: i % 2 ? 'shotgun' : 'zapper', state: 'WALLRIDE', distance: 4, melee: false, assist: 1, fresh: null }]);
    steps(90);
  }
  for (let i = 0; i < 50; i++) op('use', (...a) => core.use(...a), ['nailgun', s.freshness.nailgunUseScale]);
  op('useKey', (k) => core.useKey(k), ['revolver']);
  op('freshTiers', (o) => core.freshTiers(o), [{}]);
  op('debugSetRank', (v) => core.debugSetRank(v), [4]);
  steps(1);
  op('debugSetMeter', (v) => core.debugSetMeter(v), [0]);
  steps(240);
  op('summary', () => core.summary());
  const r = createRecorder('src/style/styleCore.js', { dt: DT, note: 'createStyleCore(config.style, hooks), then these operations in order. events = onAward(name, points, quiet) / onRank(rank, up) calls during the op; after = the core\'s state once the op returns.' });
  r.add({ session: log });
  return r.save('vectors/styleCore.json');
}

function styleSim() {
  const r = createRecorder('src/style/styleSim.js', { note: 'paceReport(config) and simulateStyle(config, opts): the headless 120 Hz pace simulation, recorded whole.' });
  r.call('paceReport', paceReport, [config]);
  r.call('simulateStyle', simulateStyle, [config, { profile: 'varied', seed: 42, seconds: 60 }]);
  r.call('simulateStyle', simulateStyle, [config, { profile: 'spam', weapon: 'shotgun', target: 'husks', seconds: 30 }]);
  return r.save('vectors/styleSim.json');
}

function directorMath() {
  const d = config.director;
  const r = createRecorder('src/ai/directorMath.js', { dt: DT });
  r.add({ constant: 'PHASES', value: DM.PHASES });
  r.call('createDirectorState', DM.createDirectorState, []);
  // stepDirector every fixed step for 20 s: damage at 1 s and 2 s, kills at 3 s, aggression 0.5 from 10 s.
  const st = DM.createDirectorState();
  const rng = seededRng(77);
  const rows = [];
  for (let i = 1; i <= 20 * config.loop.simHz; i++) {
    const t = i * DT;
    const e = { dt: DT, damageTaken: i === 120 ? 40 : i === 240 ? 45 : 0, nearbyKills: i === 360 ? 2 : 0, aggression: t >= 10 ? 0.5 : 0, rng };
    const change = DM.stepDirector(st, e, d);
    rows.push([i, e.damageTaken, e.nearbyKills, e.aggression, st.intensity, st.phase, st.phaseTime, st.relaxTime, change]);
  }
  r.add({ sequence: 'stepDirector', note: 'stepDirector(state, {dt, damageTaken, nearbyKills, aggression, rng}, config.director) each step from createDirectorState(); rng = mulberry32(77) shared across steps; columns [step, damageTaken, nearbyKills, aggression, intensity, phase, phaseTime, relaxTime, returned]', d: config.director, rows });
  for (const ph of DM.PHASES) r.call('phaseSpawnMult', DM.phaseSpawnMult, [ph, d]);
  // stepCamp: 30 s standing still with enemies alive and no damage dealt, then a relocation, then a kill chain.
  const camp = DM.createDirectorState();
  const crow = [];
  for (let i = 1; i <= 30 * config.loop.simHz; i++) {
    const t = i * DT;
    const e = { dt: DT, x: t >= 20 ? 20 : t >= 12 ? 4 : 0, z: 0, enemiesAlive: 5, dealtRecent: t >= 25 ? 100 : 0, killChain: i === 26 * config.loop.simHz };
    const edges = DM.stepCamp(camp, e, d.camp);
    crow.push([i, e.x, e.z, e.enemiesAlive, e.dealtRecent, e.killChain, camp.camp, camp.lingerTime, camp.exposed, camp.hunted, edges.started, edges.ended, edges.hunted]);
  }
  r.add({ sequence: 'stepCamp', note: 'stepCamp(state, {dt, x, z, enemiesAlive, dealtRecent, killChain}, config.director.camp) each step from createDirectorState(); x moves 0 -> 4 m at 12 s (inside the radius) -> 20 m at 20 s (a relocation); dealtRecent 100 from 25 s; a kill chain at 26 s. Columns [step, x, z, enemiesAlive, dealtRecent, killChain, camp, lingerTime, exposed, hunted, startedEdge, endedEdge, huntedEdge]', rows: crow });
  for (let round = 1; round <= 40; round++) {
    r.call('roundBudget', DM.roundBudget, [round, d.rounds]);
    r.call('roundHpMult', DM.roundHpMult, [round, d.rounds]);
    r.call('isBossRound', DM.isBossRound, [round, d.rounds]);
    r.call('isSwarmRound', DM.isSwarmRound, [round, d.rounds]);
    r.call('roundKinds', DM.roundKinds, [round, d.rounds]);
  }
  for (const round of [1, 2, 3, 4, 6, 7, 8, 9, 10, 12, 13, 17, 21, 30]) r.call('planSquads', DM.planSquads, [round, seededRng(1000 + round), d.rounds]);
  const noCharger = namedFn('available(kind) = kind !== "charger"', (k) => k !== 'charger');
  r.call('planSquads', DM.planSquads, [9, seededRng(9), d.rounds, noCharger]);
  const onlyDrone = namedFn('available(kind) = kind === "drone"', (k) => k === 'drone');
  r.call('planSquads', DM.planSquads, [5, seededRng(5), d.rounds, onlyDrone]);
  for (const [sx, sz, vis] of [[0, -20, true], [0, 20, true], [15, -15, true], [20, -12, true], [0, -20, false]]) {
    r.call('outOfView', DM.outOfView, [0, 0, 0, sx, sz, d.viewHalfAngle, vis]);
    r.call('behind', DM.behind, [0, 0, 0, sx, sz]);
  }
  r.call('outOfView', DM.outOfView, [5, 5, Math.PI / 2, -10, 5, d.viewHalfAngle, true]);
  r.call('behind', DM.behind, [5, 5, Math.PI / 2, 10, 5]);
  return r.save('vectors/directorMath.json');
}

function spawnMath() {
  const sp = config.spawner;
  const r = createRecorder('src/enemies/spawnMath.js');
  for (const seed of [0, 1, 2, 42, 77, 7919, 123456789, 4294967295, -1]) {
    const next = SP.mulberry32(seed);
    r.add({ fn: 'mulberry32', seed, first16: Array.from({ length: 16 }, () => next()) });
  }
  for (let wave = 0; wave <= 20; wave++) r.call('waveBudget', SP.waveBudget, [wave, sp]);
  for (let wave = 1; wave <= 15; wave++) r.call('waveComposition', SP.waveComposition, [wave, seededRng(500 + wave), sp]);
  for (const count of [0, 1, 2, 3, 4, 5, 6, 7, 11, 16, 25]) r.call('groupSizes', SP.groupSizes, [count, seededRng(count + 1), sp]);
  r.call('shuffled', SP.shuffled, [[1, 2, 3, 4, 5, 6, 7, 8], seededRng(3)]);
  r.call('shuffled', SP.shuffled, [[], seededRng(3)]);
  for (let wave = 1; wave <= 15; wave++) r.call('waveGroups', SP.waveGroups, [wave, 12345, sp]);
  const pts = [v3(0, 0, -30), v3(10, 0, 5), v3(-26, 0, 0), v3(3, 4, 40)];
  r.call('pickSpawnPoint', SP.pickSpawnPoint, [pts, v3(0, 0, 0), sp.minPlayerDistance, seededRng(8)]);
  r.call('pickSpawnPoint', SP.pickSpawnPoint, [pts, v3(0, 0, 0), 100, seededRng(8)], 'none far enough: the farthest');
  r.call('pickSpawnPoint', SP.pickSpawnPoint, [[], v3(0, 0, 0), 25, seededRng(8)]);
  const track = { x: 0, y: 0, z: 0, time: 0 };
  const rows = [];
  for (let i = 1; i <= 13 * config.loop.simHz; i++) {
    const pos = v3(i * DT < 3 ? 0.5 : 2, 0, 0); // moves > 1 m once at 3 s, then stands
    const out = SP.stuckStep(track, pos, false, DT, sp.stuckMove, 5);
    rows.push([i, pos.x, track.x, track.time, out]);
  }
  r.add({ sequence: 'stuckStep', note: 'stuckStep(track, pos, dealtDamage=false, dt, config.spawner.stuckMove, timeout=5) each step; columns [step, pos.x, track.x, track.time, returned]', rows });
  r.call('stuckStep', SP.stuckStep, [{ x: 0, y: 0, z: 0, time: 4 }, v3(0.2, 0, 0), true, DT, sp.stuckMove, 5], 'dealing damage resets');
  return r.save('vectors/spawnMath.json');
}

function combatMath() {
  const r = createRecorder('src/combat/combatMath.js');
  const half = config.weapons.shotgun.coneHalfAngleDeg * DEG;
  for (const dir of [v3(0, 0, -1), v3(0, 1, 0), norm(0.6, 0.8, 0), norm(1, -0.2, 0.3)]) {
    for (const [u1, u2] of [[0, 0], [1, 0], [0.5, 0.25], [0.999, 0.75], [0.1234, 0.9876]]) r.call('sampleCone', CM.sampleCone, [dir, half, u1, u2]);
  }
  const o = v3(0, 1.6, 0);
  for (const [c, rr] of [[v3(0, 1.6, -10), 0.5], [v3(2, 1.6, -10), 0.5], [v3(0, 1.6, 0.2), 0.5], [v3(0, 1.6, 10), 0.5]]) r.call('raySphere', CM.raySphere, [o, v3(0, 0, -1), c, rr]);
  for (const [d, base] of [[v3(0, 0, -1), v3(0, 0, -10)], [v3(0, 0, -1), v3(0.2, 1.2, -10)], [norm(0, -1, -0.01), v3(0, -3, -0.2)], [v3(0, 0, -1), v3(3, 0, -10)], [norm(0, 0.5, -1), v3(0, 0, -5)]]) {
    r.call('rayVerticalCapsule', CM.rayVerticalCapsule, [o, d, base, config.husk.height, config.husk.radius]);
  }
  const targets = [{ id: 'a', point: v3(3, 1, 0) }, { id: 'b', point: v3(-2, 1, 1) }, { id: 'c', point: v3(0, 1, 20) }];
  r.call('pickRicochetTarget', CM.pickRicochetTarget, [v3(0, 1, 0), targets, config.ads.ricochet.range, namedFn('hasLOS = always true', () => true)]);
  r.call('pickRicochetTarget', CM.pickRicochetTarget, [v3(0, 1, 0), targets, config.ads.ricochet.range, namedFn('hasLOS(from, to) = to.x > 0', (a, b) => b.x > 0)]);
  for (const [from, to, apex] of [[v3(0, 0, 0), v3(5, 3, 0), config.husk.leapApexAbove], [v3(0, 4, 0), v3(6, 0, 2), 1], [v3(0, 0, 0), v3(0, 0, 0), 1]]) r.call('leapVelocity', CM.leapVelocity, [from, to, apex, G]);
  const total = MM.stepsFor(config.weapons.swapTime, DT);
  const lower = total * config.weapons.swapLowerFraction;
  for (let t = -1; t <= total + 1; t += 3) {
    r.call('swapLowered', CM.swapLowered, [t, lower, total]);
    r.call('swapCanFire', CM.swapCanFire, [t, lower, total, config.weapons.swapFireFraction]);
    r.call('swapMirror', CM.swapMirror, [t, lower, total]);
  }
  const bodies = [
    { id: 'near', base: v3(0, 0, -1.8), height: 1.8, radius: 0.4 },
    { id: 'side', base: v3(1.2, 0, -1.2), height: 1.8, radius: 0.4 },
    { id: 'far', base: v3(0, 0, -4), height: 1.8, radius: 0.4 },
    { id: 'big', base: v3(-2, 0, -1), height: 7.6, radius: 2.1 },
  ];
  r.call('pickPunchTarget', CM.pickPunchTarget, [o, v3(0, 0, -1), bodies, config.punch.range, config.punch.coneHalfAngleDeg * DEG]);
  r.call('pickPunchTarget', CM.pickPunchTarget, [o, v3(1, 0, 0), bodies, config.punch.range, config.punch.coneHalfAngleDeg * DEG]);
  r.call('pickPunchTarget', CM.pickPunchTarget, [o, v3(0, 0, 1), bodies, config.punch.range, config.punch.coneHalfAngleDeg * DEG]);
  r.call('punchKind', CM.punchKind, [0]);
  r.call('punchKind', CM.punchKind, [5]);
  return r.save('vectors/combatMath.json');
}

function weaponMath() {
  const w = config.weapons;
  const r = createRecorder('src/combat/weaponMath.js', { dt: DT });
  const h = { heat: 0, lock: 0 };
  const rows = [];
  for (let i = 1; i <= 6 * config.loop.simHz; i++) {
    const firing = i * DT <= 4;
    const over = WM.heatStep(h, firing, DT, w.nailgun);
    rows.push([i, firing, h.heat, h.lock, over]);
  }
  r.add({ sequence: 'heatStep', note: 'heatStep(h, firing, dt, config.weapons.nailgun) each step from {heat 0, lock 0}; firing for the first 4 s; columns [step, firing, heat, lock, overheated]', rows });
  for (const d of [0, 1, 2, 3.99, 4, 4.01]) r.call('splashDamage', WM.splashDamage, [d, w.rocket.splashRadius, w.rocket.damage, w.rocket.edgeDamage]);
  for (const body of [v3(0, 0.1, 0), v3(0, 1, 0), v3(2, 0, 0), v3(1, 1, 1), v3(5, 0, 0), v3(0, -1, 0)]) {
    r.call('blastKnockback', WM.blastKnockback, [v3(0, 0, 0), body, w.rocket.splashRadius, w.rocket.knockback, w.rocket.knockbackMinUp, v3(0, 0, 0)]);
  }
  const mods = [{ fireRate: 1, swap: 1, charge: 1 }, { fireRate: 1.15, swap: 2, charge: 0.75 }];
  for (const m of mods) {
    r.call('fireInterval', WM.fireInterval, [w.revolver.fireInterval, m]);
    r.call('swapTime', WM.swapTime, [w.swapTime, m]);
    r.call('chargeTime', WM.chargeTime, [w.rail.charge, m]);
  }
  r.call('perkEffects', WM.perkEffects, [[], config.perks, w.swapTime]);
  for (const id of config.perks.order) r.call('perkEffects', WM.perkEffects, [[id], config.perks, w.swapTime]);
  r.call('perkEffects', WM.perkEffects, [['quick', 'flip'], config.perks, w.swapTime]);
  r.call('perkEffects', WM.perkEffects, [[...config.perks.order], config.perks, w.swapTime]);
  for (const id of config.perks.order) {
    for (const owned of [0, 1, 5, 13]) r.call('perkPrice', WM.perkPrice, [config.perks[id].price, owned, config.perks.stackTax]);
  }
  r.call('perkPrice', WM.perkPrice, [4000, 3, config.perks.stackTax, true]);
  return r.save('vectors/weaponMath.json');
}

function gunMath() {
  const w = config.weapons;
  const r = createRecorder('src/combat/gunMath.js');
  const o = v3(0, 1.6, 0);
  const bodies = [
    { id: 'a', base: v3(0, 0, -2), height: 1.8, radius: 0.4 }, { id: 'b', base: v3(1.5, 0, -1.5), height: 1.8, radius: 0.4 },
    { id: 'c', base: v3(-2.5, 0, -0.5), height: 0.75, radius: 0.3 }, { id: 'd', base: v3(0, 0, -6), height: 2.2, radius: 0.7 },
    { id: 'e', base: v3(0, 0, 2), height: 1.8, radius: 0.4 },
  ];
  r.call('sweepTargets', GM.sweepTargets, [o, v3(0, 0, -1), bodies, w.sunder.range, w.sunder.coneDeg[0] * DEG]);
  r.call('sweepTargets', GM.sweepTargets, [o, v3(0, 0, -1), bodies, w.torch.range, w.torch.coneDeg * DEG]);
  r.call('sweepTargets', GM.sweepTargets, [o, norm(1, 0, -1), bodies, w.gauntlet.range, w.gauntlet.coneDeg * DEG]);
  const pts = [v3(3, 1, 0), v3(6, 1, 0), v3(12, 1, 0), v3(3, 1, 5), v3(-4, 1, 0)];
  r.call('chainTargets', GM.chainTargets, [v3(0, 1, 0), pts, w.zapper.chains, w.zapper.chainRange, w.zapper.chainFalloff]);
  r.call('chainTargets', GM.chainTargets, [v3(0, 1, 0), pts, w.zapper.chains + w.zapper.airChains, w.zapper.chainRange, w.zapper.chainFalloff]);
  r.call('chainTargets', GM.chainTargets, [v3(0, 1, 0), pts, 3, 7, 0.7, namedFn('canReach(i) = i !== 0', (i) => i !== 0)]);
  for (const [v, n] of [[v3(5, -10, 0), v3(0, 1, 0)], [v3(5, 10, 0), v3(0, 1, 0)], [v3(-10, 0, 3), v3(1, 0, 0)], [v3(3, -4, 5), norm(0, 1, 1)]]) {
    r.call('bounceVelocity', GM.bounceVelocity, [v, n, w.bouncer.bounce, w.bouncer.friction]);
    r.call('reflectDir', GM.reflectDir, [norm(v.x, v.y, v.z), n]);
  }
  for (const b of [0, 1, 2, 4]) r.call('discDamage', GM.discDamage, [w.discus.damage, b, w.discus.bounceBonus]);
  for (const d of [0, 0.75, 1.5, 3, 7, 7.01]) r.call('pullSpeed', GM.pullSpeed, [d, w.vortex.pullRadius, w.vortex.pullSpeed, w.vortex.pullCore]);
  for (const [last, since] of [[-1, 0], [0, 0.3], [1, 0.3], [2, 0.3], [1, 0.71]]) r.call('nextCombo', GM.nextCombo, [last, since, w.sunder.comboWindow, w.sunder.damage.length]);
  const gap = MM.stepsFor(w.rattler.roundInterval, DT);
  for (let steps = 0; steps <= 3 * gap + 1; steps++) r.call('burstRoundAt', GM.burstRoundAt, [steps, gap, w.rattler.rounds]);
  for (const [slamming, since] of [[true, 99], [false, 0.2], [false, 0.3], [false, 0.31]]) r.call('thumperWave', GM.thumperWave, [slamming, since, w.thumper]);
  return r.save('vectors/gunMath.json');
}

function arsenalMath() {
  const w = config.weapons;
  const a = w.arsenal;
  const r = createRecorder('src/combat/arsenalMath.js');
  for (const x of ['ricochet', 'punch', 'orb', 'slam', 'rocket', 'bogus']) {
    r.call('weaponOf', AM.weaponOf, [x]);
    r.call('familyOf', AM.familyOf, [x, w.catalog]);
    r.call('isMelee', AM.isMelee, [x, w.catalog]);
  }
  for (const f of [-0.5, 0, 0.5, 1, 1.5]) r.call('freshMult', AM.freshMult, [f, a]);
  for (const [f, c] of [[1, 0.08], [0.05, 0.08]]) r.call('spendFresh', AM.spendFresh, [f, c]);
  for (const [f, m] of [[0.2, 1], [0.99, 1], [0.2, config.perks.flip.recover]]) r.call('recoverFresh', AM.recoverFresh, [f, 0.5, a.recover, m]);
  const primed = { weapon: 'shotgun', at: 10, used: false };
  for (const [wpn, now] of [['shotgun', 10.5], ['shotgun', 11.01], ['rocket', 10.5]]) r.call('primedMult', AM.primedMult, [primed, wpn, now, a.primedWindow, a.primedBonus]);
  r.call('primedMult', AM.primedMult, [{ ...primed, used: true }, 'shotgun', 10.5, a.primedWindow, a.primedBonus]);
  r.call('primedMult', AM.primedMult, [null, 'shotgun', 10.5, a.primedWindow, a.primedBonus]);
  const rec = (family, at, statuses = {}) => ({ family, at, statuses });
  for (const [record, family, weapon, now] of [
    [null, 'kinetic', 'revolver', 5], [rec('kinetic', 4, {}), 'edge', 'shotgun', 5], [rec('kinetic', 1, {}), 'edge', 'shotgun', 5],
    [rec('kinetic', 4, {}), 'kinetic', 'rattler', 5], [rec('gravity', 4, { pull: 6 }), 'blast', 'bouncer', 5],
    [rec('volt', 4, { shock: 6 }), 'kinetic', 'gauntlet', 5], [rec('volt', 4, { shock: 4 }), 'kinetic', 'gauntlet', 5], [rec('volt', 4, {}), null, 'bogus', 5],
  ]) r.call('crossResult', AM.crossResult, [record, family, weapon, now, a, w.catalog]);
  for (const [digit, owned] of [['1', 3], ['3', 3], ['4', 3], ['0', 10], ['0', 9], ['9', 12]]) r.call('slotForDigit', AM.slotForDigit, [digit, owned]);
  for (const [cur, owned, dir] of [[0, 5, 1], [4, 5, 1], [0, 5, -1], [0, 0, 1]]) r.call('cycleSlot', AM.cycleSlot, [cur, owned, dir]);
  for (const [x, y] of [[0, -100], [100, 0], [0, 100], [-100, 0], [70, -70], [10, 10], [-1, -120]]) r.call('radialSector', AM.radialSector, [x, y, 12, w.radial.deadZone]);
  r.call('radialSector', AM.radialSector, [0, -100, 0, w.radial.deadZone]);
  return r.save('vectors/arsenalMath.json');
}

function momentumMath() {
  const m = config.momentum;
  const r = createRecorder('src/combat/momentumMath.js', { dt: DT });
  const smooth = v3(0, 0, 0);
  const rows = [];
  for (let i = 1; i <= 30; i++) {
    MO.smoothVelocity(smooth, v3(24, 0, 0), DT, m.smoothTime);
    rows.push([i, smooth.x, smooth.y, smooth.z]);
  }
  r.add({ sequence: 'smoothVelocity', note: 'smoothVelocity(smooth, v = (24, 0, 0), dt, config.momentum.smoothTime) each step from (0, 0, 0); columns [step, x, y, z]', rows });
  r.call('smoothVelocity', MO.smoothVelocity, [v3(1, 2, 3), v3(5, 5, 5), DT, 0]);
  for (const [vel, from, to] of [[v3(10, 0, 0), v3(0, 0, 0), v3(5, 0, 0)], [v3(10, 0, 0), v3(0, 0, 0), v3(-5, 0, 0)], [v3(10, 0, 10), v3(0, 0, 0), v3(3, 4, 0)], [v3(3, 4, 0), v3(1, 1, 1), v3(1, 1, 1)]]) {
    r.call('closingSpeed', MO.closingSpeed, [vel, from, to]);
  }
  r.call('surgeOf', MO.surgeOf, [[8, 8.2, 12, 20], 24]);
  r.call('surgeOf', MO.surgeOf, [[], 24]);
  r.call('surgeOf', MO.surgeOf, [[30, 28], 24]);
  for (const [c, s] of [[0, 0], [8, 0], [20, 0], [32, 0], [40, 0], [20, 10.5], [31, 12], [0, 10]]) r.call('momentumOf', MO.momentumOf, [c, s, m]);
  for (const M of [-0.2, 0, 0.5, 1, 1.3]) {
    r.call('momentumDamage', MO.momentumDamage, [M, true, m]);
    r.call('momentumDamage', MO.momentumDamage, [M, false, m]);
    r.call('momentumRadius', MO.momentumRadius, [M, m]);
  }
  for (const [M, cd] of [[0.5, 0], [0.55, 0], [0.56, 0], [0.8, 0], [1, 0], [1, 0.1]]) r.call('impactWave', MO.impactWave, [M, cd, m.shock]);
  return r.save('vectors/momentumMath.json');
}

function assistMath() {
  const lk = config.aim.lock;
  const r = createRecorder('src/combat/assistMath.js', { dt: DT });
  r.call('lerp', AS.lerp, [2, 10, 0.25]);
  const eye = v3(0, 1.6, 0);
  for (const p of [v3(0, 1.6, -10), v3(1, 1.6, -10), v3(0, 1.6, 10), v3(0, 1.6, 0)]) r.call('angleTo', AS.angleTo, [eye, v3(0, 0, -1), p]);
  for (const mode of ['assist', 'snap', 'lock', 'off', 'bogus']) {
    for (const strength of [0, 30, 60, 90, 100]) {
      for (const ads of [false, true]) r.call('lockParams', AS.lockParams, [mode, strength, ads, lk]);
    }
  }
  for (const s of [0, 50, 60, 100, 150]) r.call('lockPayout', AS.lockPayout, [s, lk]);
  r.call('assistPayout', AS.assistPayout, [100, true, config.aim.payout]);
  r.call('assistPayout', AS.assistPayout, [100, false, config.aim.payout]);
  const cone = lk.modes.snap.cone * DEG;
  const cands = [
    { id: 1, angle: 0.05, distance: 10, threat: lk.threat.husk, head: true },
    { id: 2, angle: 0.02, distance: 40, threat: lk.threat.lancer, head: false },
    { id: 3, angle: 0.15, distance: 5, threat: lk.threat.boss, head: false, weak: true },
    { id: 4, angle: 0.3, distance: 5, threat: 1, head: true },
    { id: 5, angle: 0.01, distance: 95, threat: 1, head: true },
  ];
  for (const c of cands) r.call('lockScore', AS.lockScore, [c, cone, lk.range, lk.weights]);
  r.call('pickBest', AS.pickBest, [cands, cone, lk.range, lk.weights]);
  r.call('pickBest', AS.pickBest, [cands, cone, lk.range, lk.weights, namedFn('skip(c) = c.id === 1', (c) => c.id === 1)]);
  r.call('pickBest', AS.pickBest, [[], cone, lk.range, lk.weights]);
  const ch = { id: null, t: 0 };
  for (const [cur, id, score] of [[1, 7, 1.5], [1, 7, 1.5], [1, 7, 1.2], [1, 7, 1.5], ...Array.from({ length: 14 }, () => [1, 7, 1.5]), [1, 8, 2]]) {
    r.call('challenge', AS.challenge, [ch, cur, id, score, DT, lk.hysteresis.margin, lk.hysteresis.hold]);
  }
  const springRows = [];
  let sx = 10;
  let sv = 0;
  const omega = lk.modes.snap.omega;
  for (let i = 1; i <= 60; i++) {
    const out = AS.springStep(sx, sv, omega, DT);
    sx = out.x;
    sv = out.v;
    springRows.push([i, sx, sv]);
  }
  r.add({ sequence: 'springStep', note: `springStep(x, v, omega = config.aim.lock.modes.snap.omega (${omega}), dt) each step from x = 10, v = 0; columns [step, x, v]`, rows: springRows });
  for (const [x, v, om] of [[1, 0, 22], [0, 5, 50], [-3, 2, 60], [1, 0, 0]]) r.call('springStep', AS.springStep, [x, v, om, DT]);
  for (const [on, err, prev, held] of [[true, 5, 6, 0], [false, 0.5, 2, 0], [false, 7, 8, 0], [false, 3, 3, 0], [false, 3, 4, 0.02], [false, 3, 4, 0.05]]) {
    r.call('fireReady', AS.fireReady, [on, err, prev, held, lk.fire]);
  }
  for (const [turn, before, after] of [[0.05, 1, 2], [0.01, 1, 2], [0.05, 2, 1], [0.05, 0, 0]]) r.call('breaksAway', AS.breaksAway, [turn, DT, lk.modes.snap.breakaway, before, after]);
  r.call('breaksAway', AS.breaksAway, [0.05, 0, lk.modes.snap.breakaway, 1, 2]);
  for (const [vel, target, maxTurn] of [[v3(0, 0, -30), v3(10, 0, -10), 0.1], [v3(0, 0, -30), v3(0.01, 0, -10), 0.1], [v3(0, 0, 0), v3(1, 0, 0), 0.1], [v3(0, 0, -30), v3(0, 0, 20), 5]]) {
    r.call('homeVelocity', AS.homeVelocity, [vel, v3(0, 0, 0), target, maxTurn]);
  }
  const sorted = [{ id: 'a', angle: 0.01 }, { id: 'b', angle: 0.05 }, { id: 'c', angle: 0.2 }];
  for (const cur of ['a', 'c', 'zzz']) r.call('nextLock', AS.nextLock, [sorted, cur]);
  r.call('nextLock', AS.nextLock, [[{ id: 'a', angle: 0 }], 'a']);
  r.call('nextLock', AS.nextLock, [[], 'a']);
  for (const a of [0, Math.PI, -Math.PI, 3 * Math.PI, 7, -7, 2 * Math.PI]) r.call('wrapAngle', AS.wrapAngle, [a]);
  for (const p of [v3(0, 1.6, -10), v3(10, 1.6, 0), v3(-3, 5, 4), v3(0, 11.6, 0)]) r.call('yawPitchTo', AS.yawPitchTo, [eye, p]);
  for (const [yaw, pitch] of [[0, 0], [Math.PI / 2, 0], [0.3, -0.4], [-2, 1.2]]) r.call('lookDir', AS.lookDir, [yaw, pitch]);
  r.call('lookError', AS.lookError, [0, 0, 0.1, 0]);
  r.call('lookError', AS.lookError, [0.3, 0.2, -0.3, -0.1]);
  r.call('leadPoint', AS.leadPoint, [eye, v3(0, 1, -20), v3(5, 0, 0), config.weapons.rocket.speed, v3(0, 0, 0)]);
  r.call('leadPoint', AS.leadPoint, [eye, v3(0, 1, -20), v3(5, 0, 0), 0, v3(0, 0, 0)]);
  return r.save('vectors/assistMath.json');
}

export function exportVectors() {
  const out = {};
  for (const [name, fn] of Object.entries({ movementMath, swingMath, styleMath, styleCore, styleSim, directorMath, spawnMath, combatMath, weaponMath, gunMath, arsenalMath, momentumMath, assistMath })) {
    out[name] = fn();
  }
  return out;
}

if (process.argv[1]?.endsWith('vectors.mjs')) {
  for (const [name, count] of Object.entries(exportVectors())) console.log(`vectors/${name}.json: ${count} records`);
}
