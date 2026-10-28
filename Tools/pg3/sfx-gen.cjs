// PG3: generate the game's sounds with the ElevenLabs sound-generation API from Tools/pg3/sounds.json.
// Usage: node Tools/pg3/sfx-gen.cjs [--only id,id] [--force]
// Key: the ELEVENLABS_API_KEY environment variable (user scope on Windows; read from the registry if the shell lacks it).
// Output: Assets/Resources/Audio/PG3/<id>.mp3 (Pg3Audio.cs loads them by id) and Tools/pg3/sfx-gen-log.json
// (prompt, seconds, HTTP status, bytes, time: the prompt log's audio section reads it). Never writes the key anywhere.
'use strict';
const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const root = path.resolve(__dirname, '..', '..');
// sounds.json (the first 28) plus sounds2.json (the sound pass: variants, charge, per kind, per boss, UI), one id space
const list = ['sounds.json', 'sounds2.json'].filter(f => fs.existsSync(path.join(__dirname, f)))
  .flatMap(f => JSON.parse(fs.readFileSync(path.join(__dirname, f), 'utf8')).sounds);
const outDir = path.join(root, 'Assets', 'Resources', 'Audio', 'PG3');
const logPath = path.join(__dirname, 'sfx-gen-log.json');

function key() {
  if (process.env.ELEVENLABS_API_KEY) return process.env.ELEVENLABS_API_KEY.trim();
  try {
    return execFileSync('powershell', ['-NoProfile', '-Command', '[Environment]::GetEnvironmentVariable("ELEVENLABS_API_KEY","User")'], { encoding: 'utf8' }).trim();
  } catch (e) { return ''; }
}

async function main() {
  const args = process.argv.slice(2);
  const force = args.includes('--force');
  const oi = args.indexOf('--only');
  const only = oi >= 0 ? new Set(args[oi + 1].split(',')) : null;
  const k = key();
  if (!k.startsWith('sk_')) { console.error('sfx-gen: ELEVENLABS_API_KEY is missing or is not an sk_ key'); process.exit(2); }
  fs.mkdirSync(outDir, { recursive: true });
  const log = fs.existsSync(logPath) ? JSON.parse(fs.readFileSync(logPath, 'utf8')) : [];
  let made = 0, failed = 0;
  for (const s of list) {
    if (only && !only.has(s.id)) continue;
    const file = path.join(outDir, s.id + '.mp3');
    if (fs.existsSync(file) && !force) { console.log('skip ' + s.id + ' (exists)'); continue; }
    // the API takes 0.5-30 s (its 400 on a 0.3 s request, 2026-10-28); a shorter wish is clamped up, the clip's tail is silence
    const body = { text: s.prompt, duration_seconds: Math.min(30, Math.max(0.5, s.seconds)), prompt_influence: 0.5 };
    if (s.loop) body.loop = true;
    const t0 = Date.now();
    let res;
    // 2 requests at once on this plan (429 "concurrent_limit_exceeded" when music runs beside): wait and retry that one only
    for (let tries = 0; ; tries++) {
      res = await fetch('https://api.elevenlabs.io/v1/sound-generation?output_format=mp3_44100_128', {
        method: 'POST',
        headers: { 'xi-api-key': k, 'Content-Type': 'application/json', Accept: 'audio/mpeg' },
        body: JSON.stringify(body),
      });
      if (res.status !== 429 || tries >= 40) break;
      if (!(await res.clone().text()).includes('concurrent')) break;
      await new Promise(r => setTimeout(r, 4000));
    }
    const entry = { id: s.id, event: s.event, triad: s.triad, prompt: s.prompt, seconds: s.seconds, loop: !!s.loop, status: res.status, at: new Date().toISOString(), ms: Date.now() - t0 };
    if (!res.ok) {
      entry.error = (await res.text()).slice(0, 300);
      failed++;
      console.log('FAIL ' + s.id + ' ' + res.status + ' ' + entry.error);
    } else {
      const buf = Buffer.from(await res.arrayBuffer());
      fs.writeFileSync(file, buf);
      entry.bytes = buf.length;
      made++;
      console.log('ok   ' + s.id + ' ' + buf.length + ' bytes');
    }
    log.push(entry);
    fs.writeFileSync(logPath, JSON.stringify(log, null, 2));
  }
  console.log('sfx-gen: ' + made + ' made, ' + failed + ' failed; log ' + path.relative(root, logPath));
  process.exit(failed ? 1 : 0);
}
main();
