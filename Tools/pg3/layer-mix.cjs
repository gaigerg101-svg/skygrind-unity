// PG3: layered effects from Tools/pg3/layers.json. Each layer is one ElevenLabs sound-generation request (kept under
// Research/2026-10-28-pg3-audio/layers/<id>/L<n>.mp3); ffmpeg mixes them (gain dB, delay ms, no auto-normalize), then a -1 dBTP
// limiter, into Assets/Resources/Audio/PG3/<id>.mp3. Before the first overwrite the existing clip is kept once in raw/sfx/ (normalize.cjs
// reads raw/ as the original, so after a layered mix the lead copies the mix into raw/ too: this script does it).
// Usage: node Tools/pg3/layer-mix.cjs [--only id,id]   -> Tools/pg3/layer-mix-log.json
'use strict';
const fs = require('fs');
const path = require('path');
const { spawnSync, execFileSync } = require('child_process');

const root = path.resolve(__dirname, '..', '..');
const spec = JSON.parse(fs.readFileSync(path.join(__dirname, 'layers.json'), 'utf8')).clips;
const outDir = path.join(root, 'Assets', 'Resources', 'Audio', 'PG3');
const layerRoot = path.join(root, 'Research', '2026-10-28-pg3-audio', 'layers');
const rawDir = path.join(root, 'Research', '2026-10-28-pg3-audio', 'raw', 'sfx');
const logPath = path.join(__dirname, 'layer-mix-log.json');

function key() {
  if (process.env.ELEVENLABS_API_KEY) return process.env.ELEVENLABS_API_KEY.trim();
  try { return execFileSync('powershell', ['-NoProfile', '-Command', '[Environment]::GetEnvironmentVariable("ELEVENLABS_API_KEY","User")'], { encoding: 'utf8' }).trim(); } catch (e) { return ''; }
}

async function gen(k, layer, file) {
  if (fs.existsSync(file)) return { status: 'kept' };
  const body = { text: layer.prompt, duration_seconds: Math.min(30, Math.max(0.5, layer.seconds)), prompt_influence: 0.6 };
  if (layer.loop) body.loop = true;
  let res;
  for (let tries = 0; ; tries++) {
    res = await fetch('https://api.elevenlabs.io/v1/sound-generation?output_format=mp3_44100_192', {
      method: 'POST', headers: { 'xi-api-key': k, 'Content-Type': 'application/json', Accept: 'audio/mpeg' }, body: JSON.stringify(body) });
    if (res.status !== 429 || tries >= 40) break;
    if (!(await res.clone().text()).includes('concurrent')) break;
    await new Promise(r => setTimeout(r, 4000));
  }
  if (!res.ok) return { status: res.status, error: (await res.text()).slice(0, 200) };
  fs.writeFileSync(file, Buffer.from(await res.arrayBuffer()));
  return { status: 200 };
}

function mix(clip, files, out) {
  const args = ['-hide_banner', '-y'];
  for (const f of files) args.push('-i', f);
  const parts = [];
  clip.layers.forEach((l, i) => {
    const d = l.delay ? `adelay=${l.delay}|${l.delay},` : '';
    parts.push(`[${i}:a]${d}volume=${(l.gain || 0)}dB[l${i}]`);
  });
  const ins = clip.layers.map((_, i) => `[l${i}]`).join('');
  // duration=first keeps a loop's length; a one-shot takes the longest layer (its tail)
  const dur = clip.loop ? 'first' : 'longest';
  parts.push(`${ins}amix=inputs=${clip.layers.length}:duration=${dur}:normalize=0,alimiter=limit=0.89:level=false[out]`);
  args.push('-filter_complex', parts.join(';'), '-map', '[out]', '-c:a', 'libmp3lame', '-b:a', '192k', out);
  const r = spawnSync('ffmpeg', args, { encoding: 'utf8' });
  return r.status === 0 ? null : (r.stderr || '').slice(-300);
}

async function main() {
  const args = process.argv.slice(2);
  const oi = args.indexOf('--only');
  const only = oi >= 0 ? new Set(args[oi + 1].split(',')) : null;
  const k = key();
  if (!k.startsWith('sk_')) { console.error('layer-mix: no sk_ key'); process.exit(2); }
  fs.mkdirSync(rawDir, { recursive: true });
  const log = fs.existsSync(logPath) ? JSON.parse(fs.readFileSync(logPath, 'utf8')) : [];
  const todo = spec.filter(c => !only || only.has(c.id));
  // the layers: two requests at a time (the plan's limit)
  const jobs = [];
  for (const c of todo) c.layers.forEach((l, i) => jobs.push({ c, l, i }));
  let j = 0;
  const results = new Map();
  async function worker() {
    while (j < jobs.length) {
      const { c, l, i } = jobs[j++];
      const dir = path.join(layerRoot, c.id);
      fs.mkdirSync(dir, { recursive: true });
      const r = await gen(k, l, path.join(dir, 'L' + (i + 1) + '.mp3'));
      results.set(c.id + ':' + i, r);
      log.push({ id: c.id, layer: i + 1, prompt: l.prompt, seconds: l.seconds, gain: l.gain || 0, delay: l.delay || 0, ...r, at: new Date().toISOString() });
      console.log((r.status === 200 || r.status === 'kept' ? 'ok   ' : 'FAIL ') + c.id + ' L' + (i + 1) + ' ' + r.status + (r.error ? ' ' + r.error : ''));
    }
  }
  await Promise.all([worker(), worker()]);
  let made = 0;
  for (const c of todo) {
    const files = c.layers.map((_, i) => path.join(layerRoot, c.id, 'L' + (i + 1) + '.mp3'));
    if (files.some(f => !fs.existsSync(f))) { console.log('skip ' + c.id + ': a layer is missing'); continue; }
    const out = path.join(outDir, c.id + '.mp3');
    const keep = path.join(rawDir, c.id + '.single.mp3');
    if (fs.existsSync(out) && !fs.existsSync(keep)) fs.copyFileSync(out, keep); // the single-take original, once
    const err = mix(c, files, out);
    if (err) { console.log('MIX FAIL ' + c.id + ' ' + err); continue; }
    fs.copyFileSync(out, path.join(rawDir, c.id + '.mp3')); // normalize.cjs's original is now the mix
    made++;
    console.log('mixed ' + c.id + ' (' + c.layers.length + ' layers)');
  }
  fs.writeFileSync(logPath, JSON.stringify(log, null, 2));
  console.log('layer-mix: ' + made + ' mixed of ' + todo.length + '; log Tools/pg3/layer-mix-log.json');
}
main();
