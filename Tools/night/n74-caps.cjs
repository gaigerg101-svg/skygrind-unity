#!/usr/bin/env node
// NIGHT74 (FORGE74, n74-tools.cjs): a copy of n72-caps.cjs with the n74 names and NIGHT74.md section 5's caps.
// NIGHT72 (FORGE72 red team 9: the caps were unmetered): prints the night's spend against its caps, one line, for every clock check.
//   node Tools/night/n74-caps.cjs
// builds = Builds/n74-* folders (n74-play is a hard-link copy and is not counted); reel sides = "== <side> HH:MM:SS" lines in
// Logs/feel/n74-*/summary47.txt (SKIPPED lines not counted); smokes = Logs/player/n74-*-smoke.log + Logs/player/n56-smoke-steal-n74-*.log;
// spotter runs = Logs/n74/spot/*/spot.md (the lead saves each SPOT reply there). Caps: NIGHT72.md section 5.
'use strict';
const fs = require('fs');
const path = require('path');
const ROOT = path.resolve(__dirname, '..', '..');
const CAPS = { builds: 12, sides: 260, smokes: 12, spotter: 60 };
// the 2026-10-26 evening session's n74-fix build, reels and smoke predate the night and are not counted
const ls = (d) => { try { return fs.readdirSync(path.join(ROOT, d)).filter((n) => !/^n74-fix\b/.test(n)); } catch (e) { return []; } };
const builds = ls('Builds').filter((n) => /^n74-/.test(n) && n !== 'n74-play').length;
let sides = 0;
for (const t of ls('Logs/feel').filter((n) => /^n74-/.test(n))) {
  try { sides += (fs.readFileSync(path.join(ROOT, 'Logs/feel', t, 'summary47.txt'), 'utf8').match(/^== [A-Za-z0-9-]+ \d\d:\d\d:\d\d/gm) || []).length; } catch (e) { /* none */ }
}
const smokes = ls('Logs/player').filter((n) => /^n74-.*-smoke\.log$/.test(n) || /^n56-smoke-steal-n74-.*\.log$/.test(n)).length;
const spotter = ls('Logs/n74/spot').filter((r) => fs.existsSync(path.join(ROOT, 'Logs/n74/spot', r, 'spot.md'))).length;
let free = '?';
try { const s = fs.statfsSync(ROOT); free = (s.bavail * s.bsize / 2 ** 30).toFixed(1); } catch (e) { /* old node */ }
const over = Object.entries({ builds, sides, smokes, spotter }).filter(([k, v]) => v > CAPS[k]).map(([k]) => k);
console.log('caps n74: builds ' + builds + '/' + CAPS.builds + ', reel sides ' + sides + '/' + CAPS.sides + ', smokes ' + smokes + '/' + CAPS.smokes + ', spotter runs ' + spotter + '/' + CAPS.spotter + '; C: ' + free + ' GB free' + (over.length ? '; OVER: ' + over.join(', ') : ''));
