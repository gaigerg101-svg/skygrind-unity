// Shared helpers for the port-export scripts. Nothing here changes game code: every script imports the real modules
// from src/ and records what they return.
import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { config } from '../../src/config.js';
import { mulberry32 } from '../../src/enemies/spawnMath.js';

export const EXPORT_DIR = resolve(dirname(fileURLToPath(import.meta.url)), '..');
export const REPO_DIR = resolve(EXPORT_DIR, '..');
export { config };

/** Every object inside config, keyed by identity -> its dotted path (so args that ARE config are written as refs). */
const CONFIG_PATHS = new Map();
(function walk(o, path) {
  if (!o || typeof o !== 'object' || CONFIG_PATHS.has(o)) return;
  CONFIG_PATHS.set(o, path);
  for (const k of Object.keys(o)) walk(o[k], path ? `${path}.${k}` : k);
})(config, '');

/**
 * JSON-safe snapshot of any value, lossless for doubles:
 * - numbers are written by JSON.stringify (shortest round-trip form: parses back to the identical double);
 *   -0, NaN, Infinity, -Infinity become {"$num": "-0" | "NaN" | "Infinity" | "-Infinity"};
 * - an object that is part of config.js becomes {"$config": "dotted.path"} (resolve it in config.json);
 * - a function becomes {"$fn": ...} (its descriptor, e.g. a seeded mulberry32 and how many draws it has made);
 * - typed arrays become plain arrays; Map / Set become {"$map": [[k, v], ...]} / {"$set": [...]};
 * - undefined becomes {"$undefined": true}.
 */
export function enc(v) {
  if (typeof v === 'number') {
    if (Object.is(v, -0)) return { $num: '-0' };
    if (!Number.isFinite(v)) return { $num: String(v) };
    return v;
  }
  if (v === undefined) return { $undefined: true };
  if (typeof v === 'function') return { $fn: v.desc ? { ...v.desc } : v.name || 'anonymous' };
  if (v === null || typeof v !== 'object') return v;
  if (CONFIG_PATHS.has(v)) return { $config: CONFIG_PATHS.get(v) };
  if (ArrayBuffer.isView(v)) return Array.from(v, enc);
  if (v instanceof Map) return { $map: [...v].map(([k, x]) => [enc(k), enc(x)]) };
  if (v instanceof Set) return { $set: [...v].map(enc) };
  if (Array.isArray(v)) return v.map(enc);
  const out = {};
  for (const k of Object.keys(v)) out[k] = enc(v[k]);
  return out;
}

/** A seeded mulberry32 (the game's PRNG, enemies/spawnMath.js) that describes itself and counts its draws. */
export function seededRng(seed) {
  const next = mulberry32(seed);
  const desc = { $rng: 'mulberry32', seed, draws: 0 };
  const fn = () => {
    desc.draws++;
    return next();
  };
  fn.desc = desc;
  return fn;
}

/** A named stand-in for an injected callback (line of sight tests and the like). */
export function namedFn(description, fn) {
  fn.desc = { $callback: description };
  return fn;
}

/**
 * Records calls to a real function: the arguments as they were before the call, the return value, and the arguments
 * after the call when the function mutated any of them.
 */
export function createRecorder(moduleName, extra = {}) {
  const cases = [];
  return {
    cases,
    call(name, fn, args, note) {
      const before = args.map(enc);
      const result = fn(...args);
      const after = args.map(enc);
      const entry = { fn: name, args: before, result: enc(result) };
      if (after.some((a, i) => JSON.stringify(a) !== JSON.stringify(before[i]))) entry.argsAfter = after;
      if (note) entry.note = note;
      cases.push(entry);
      return result;
    },
    /** A free-form record (sequences, simulations): stored as given, after enc(). */
    add(entry) {
      cases.push(enc(entry));
    },
    save(file) {
      writeJson(file, {
        module: moduleName,
        generatedBy: 'port-export/scripts/vectors.mjs',
        encoding: 'see port-export/README.md "Value encoding"',
        ...extra,
        cases,
      });
      return cases.length;
    },
  };
}

export function writeJson(relPath, data, { pretty = false } = {}) {
  const path = join(EXPORT_DIR, relPath);
  mkdirSync(dirname(path), { recursive: true });
  const text = pretty ? JSON.stringify(data, null, 2) : JSON.stringify(data);
  writeFileSync(path, `${text}\n`);
  return { path, bytes: text.length + 1 };
}

export function writeText(relPath, text) {
  const path = join(EXPORT_DIR, relPath);
  mkdirSync(dirname(path), { recursive: true });
  writeFileSync(path, text);
  return { path, bytes: text.length };
}
