// PG3: level the ElevenLabs clips so the mix is even (a kill at -21.6 LUFS sat 13 dB under a shotgun at -8.7; 2026-10-28).
// Measures each clip's integrated loudness (ffmpeg ebur128) and applies one gain to reach its group's target, clamped to +-12 dB,
// then a -1 dBTP limiter. Originals are kept once in Research/2026-10-28-pg3-audio/raw/ (re-running reads the originals, so it
// never stacks gains). Targets (LUFS, integrated, short clips): research section 4 cites whole-mix -18 (handheld) / -24 (console);
// these are per-clip levels under the game's own volumes (UnityTuning.Pg3SfxVolume, MusicVolume).
// Usage: node Tools/pg3/normalize.cjs   -> Tools/pg3/normalize-log.json
'use strict';
const fs = require('fs');
const path = require('path');
const { spawnSync } = require('child_process');

const root = path.resolve(__dirname, '..', '..');
const sfxDir = path.join(root, 'Assets', 'Resources', 'Audio', 'PG3');
const musicDir = path.join(sfxDir, 'Music');
const raw = path.join(root, 'Research', '2026-10-28-pg3-audio', 'raw');

function target(id, music) {
  if (music) return /-calm$|^select$|^results$/.test(id) ? -14.5 : /^sting-/.test(id) ? -12 : -12;
  if (/^ui-/.test(id)) return -20;
  if (/^amb-/.test(id)) return -26; // the level ambience: a bed under the music, never in front of it
  if (/^wind-loop$/.test(id)) return -20;
  if (/^step(-\d)?$/.test(id)) return -20; // footsteps: many per second, felt more than heard
  if (id === 'charge-loop') return -15; // GG 2026-10-28: the charge "more powerful": the loudest loop
  if (/loop$/.test(id)) return -18;
  if (/^(parry(-2)?|charge-ready)$/.test(id)) return -12;
  if (/^(jump|land|dash|double-jump|wall-jump|wallride|slide-start|grapple|stamina|ads-in|weapon-swap|pump)/.test(id)) return -17;
  if (/(intro|down|slam|-charged|slug|pulse|win|lose|boss-phase|weapon-earned)/.test(id)) return -11;
  if (/^fire-/.test(id)) return -12;
  return -14; // hits, kills, deaths, tells, rewards
}

function measure(file) {
  const txt = spawnSync('ffmpeg', ['-hide_banner', '-nostats', '-i', file, '-af', 'ebur128', '-f', 'null', '-'], { encoding: 'utf8' }).stderr || '';
  const m = [...txt.matchAll(/^\s+I:\s+(-?[\d.]+) LUFS/gm)];
  return m.length ? parseFloat(m[m.length - 1][1]) : null;
}

function run(dir, music, log) {
  const rawDir = path.join(raw, music ? 'music' : 'sfx');
  fs.mkdirSync(rawDir, { recursive: true });
  for (const f of fs.readdirSync(dir).filter(n => n.endsWith('.mp3'))) {
    const id = f.slice(0, -4);
    const file = path.join(dir, f);
    const orig = path.join(rawDir, f);
    if (!fs.existsSync(orig)) fs.copyFileSync(file, orig);
    const before = measure(orig);
    if (before == null || !isFinite(before)) { log.push({ id, music, skipped: 'no loudness' }); continue; }
    const tgt = target(id, music);
    const gain = Math.max(-12, Math.min(12, tgt - before));
    const tmp = file + '.tmp.mp3';
    spawnSync('ffmpeg', ['-hide_banner', '-y', '-i', orig, '-af', `volume=${gain.toFixed(2)}dB,alimiter=limit=0.89:level=false`, '-c:a', 'libmp3lame', '-b:a', music ? '192k' : '160k', tmp], { stdio: 'ignore' });
    fs.renameSync(tmp, file);
    const after = measure(file);
    log.push({ id, music, target: tgt, before, gain: +gain.toFixed(2), after });
    console.log((music ? 'music ' : 'sfx   ') + id.padEnd(22) + ' ' + before.toFixed(1) + ' -> ' + (after == null ? '?' : after.toFixed(1)) + ' (target ' + tgt + ')');
  }
}

const log = [];
run(sfxDir, false, log);
run(musicDir, true, log);
fs.writeFileSync(path.join(__dirname, 'normalize-log.json'), JSON.stringify(log, null, 2));
console.log('normalize: ' + log.length + ' clips; log Tools/pg3/normalize-log.json');
