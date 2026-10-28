// PG3: generate the game's music with the ElevenLabs Music API from Tools/pg3/music.json (docs/pg3/PG3-SOUND-PROMPT.md section 4).
// Usage: node Tools/pg3/music-gen.cjs [--only id,id] [--force] [--parallel N]
// Key: ELEVENLABS_API_KEY (user env; read from the registry when the shell lacks it). Never written anywhere.
// Output: Assets/Resources/Audio/PG3/Music/<id>.mp3 (MusicDirector loads them) and Tools/pg3/music-gen-log.json (prompt, length,
// HTTP status, bytes, song id, time: the prompt log's music section reads it). A 401 / 402 / 429 stops the run (the quota signal).
'use strict';
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = path.resolve(__dirname, '..', '..');
const list = JSON.parse(fs.readFileSync(path.join(__dirname, 'music.json'), 'utf8')).tracks;
const outDir = path.join(root, 'Assets', 'Resources', 'Audio', 'PG3', 'Music');
const logPath = path.join(__dirname, 'music-gen-log.json');

function key() {
  if (process.env.ELEVENLABS_API_KEY) return process.env.ELEVENLABS_API_KEY.trim();
  try {
    return execFileSync('powershell', ['-NoProfile', '-Command', '[Environment]::GetEnvironmentVariable("ELEVENLABS_API_KEY","User")'], { encoding: 'utf8' }).trim();
  } catch (e) { return ''; }
}

let stop = false;

async function one(k, t, log) {
  const file = path.join(outDir, t.id + '.mp3');
  // the model: music_v2_5 (the first 21 went out with no model_id = music_v1; the research found v2_5, RESEARCH.md section 1)
  const model = t.model || 'music_v2_5';
  const body = t.plan ? { composition_plan: t.plan, model_id: model } : { prompt: t.prompt, music_length_ms: Math.round(t.seconds * 1000), force_instrumental: true, model_id: model };
  const t0 = Date.now();
  let res;
  // the plan allows 2 requests at once (the 429 "concurrent_limit_exceeded", 2026-10-28): wait and retry that one, stop on any other 429
  for (let tries = 0; ; tries++) {
    res = await fetch('https://api.elevenlabs.io/v1/music?output_format=mp3_44100_192', {
      method: 'POST',
      headers: { 'xi-api-key': k, 'Content-Type': 'application/json', Accept: 'audio/mpeg' },
      body: JSON.stringify(body),
    });
    if (res.status !== 429 || tries >= 40) break;
    const txt = await res.clone().text();
    if (!txt.includes('concurrent')) break;
    await new Promise(r => setTimeout(r, 6000));
  }
  const entry = { id: t.id, model, state: t.state, level: t.level || null, seconds: t.seconds, prompt: t.prompt || null, plan: !!t.plan, status: res.status, songId: res.headers.get('song-id'), at: new Date().toISOString(), ms: Date.now() - t0 };
  if (!res.ok) {
    entry.error = (await res.text()).slice(0, 400);
    if ([401, 402, 429].includes(res.status)) stop = true;
    console.log('FAIL ' + t.id + ' ' + res.status + ' ' + entry.error);
  } else {
    const buf = Buffer.from(await res.arrayBuffer());
    fs.writeFileSync(file, buf);
    entry.bytes = buf.length;
    console.log('ok   ' + t.id + ' ' + buf.length + ' bytes in ' + entry.ms + ' ms');
  }
  log.push(entry);
  fs.writeFileSync(logPath, JSON.stringify(log, null, 2));
  return res.ok;
}

async function main() {
  const args = process.argv.slice(2);
  const force = args.includes('--force');
  const oi = args.indexOf('--only');
  const only = oi >= 0 ? new Set(args[oi + 1].split(',')) : null;
  const pi = args.indexOf('--parallel');
  const par = pi >= 0 ? Math.max(1, parseInt(args[pi + 1], 10)) : 2;
  const k = key();
  if (!k.startsWith('sk_')) { console.error('music-gen: ELEVENLABS_API_KEY is missing or is not an sk_ key'); process.exit(2); }
  fs.mkdirSync(outDir, { recursive: true });
  const log = fs.existsSync(logPath) ? JSON.parse(fs.readFileSync(logPath, 'utf8')) : [];
  const todo = list.filter(t => (!only || only.has(t.id)) && (force || !fs.existsSync(path.join(outDir, t.id + '.mp3'))));
  let made = 0, failed = 0, i = 0;
  async function worker() {
    while (!stop && i < todo.length) {
      const t = todo[i++];
      try { (await one(k, t, log)) ? made++ : failed++; } catch (e) { failed++; console.log('FAIL ' + t.id + ' ' + e.message); }
    }
  }
  await Promise.all(Array.from({ length: par }, worker));
  console.log('music-gen: ' + made + ' made, ' + failed + ' failed' + (stop ? ' (stopped: quota / auth)' : '') + '; log ' + path.relative(root, logPath));
  process.exitCode = failed ? 1 : 0;
}
main();
