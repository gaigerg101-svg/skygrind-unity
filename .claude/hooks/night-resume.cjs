// FORGE68 (2026-10-24; DECIDE68 Q5 E1, the forge's call at GG's "you make the calls"): a SessionStart hook for matcher compact|resume.
// After a context compaction or a resume, Claude Code adds a SessionStart hook's stdout to the context (prompt-methods.md [A12]), so the
// lead gets back what compaction summarises away: the running night's state file and its prompt's rules and stop section.
// Prints nothing when no night clock is running (Logs/clock.json missing, stopped, or older than its staleMin), so ordinary sessions see
// nothing. Never writes; never fails the session (every error prints nothing and exits 0).
//   node .claude/hooks/night-resume.cjs            (the hook)
//   SKYGRIND_RESUME_TEST_NOW=<unix s> node ...     (a dry run at a given time)
'use strict';
const fs = require('fs');
const path = require('path');
try {
  const root = process.env.CLAUDE_PROJECT_DIR || path.join(__dirname, '..', '..');
  const rd = (p) => { try { return fs.readFileSync(path.join(root, p), 'utf8').replace(/\r\n/g, '\n'); } catch (e) { return null; } };
  const clock = JSON.parse(rd('Logs/clock.json') || 'null');
  if (!clock || clock.stopped || !/^n\d+$/.test(clock.night || '')) process.exit(0);
  const now = Number(process.env.SKYGRIND_RESUME_TEST_NOW) || Math.floor(Date.now() / 1000);
  const min = (now - clock.t0) / 60;
  if (min < 0 || min > (clock.staleMin || 720)) process.exit(0);
  const N = clock.night.slice(1), NIGHT = 'NIGHT' + N;
  const h = Math.floor(min / 60), m = Math.floor(min % 60);
  const out = [];
  out.push('## ' + NIGHT + ' resume card (.claude/hooks/night-resume.cjs, after a compaction or resume)');
  out.push('The night clock reads ' + h + ':' + String(m).padStart(2, '0') + ' of ' + Math.floor((clock.capMin || 480) / 60) + ':' + String((clock.capMin || 480) % 60).padStart(2, '0') + ' (t0 ' + (clock.t0Text || clock.t0) + '). Run `node Tools/night/' + clock.night + '-clock.cjs` now.');
  out.push('Read ' + NIGHT + '.md in full before the next step. Resume agents by the ids in the state file below (names given before a compaction need the id).');
  const state = rd('Logs/' + clock.night + '/STATE.md');
  out.push('', '### Logs/' + clock.night + '/STATE.md', state ? state.slice(0, 6000) : '(missing: write it now, NIGHT' + N + '.md rule 12)');
  const prompt = rd(NIGHT + '.md');
  if (prompt) {
    const sec = (re) => { const i = prompt.search(re); if (i < 0) return null; const rest = prompt.slice(i); const j = rest.slice(3).search(/\n## /); return j < 0 ? rest : rest.slice(0, j + 3); };
    for (const re of [/^## 0\. /m, /^## 10\. /m]) { const s = sec(re); if (s) out.push('', s.trim().slice(0, 9000)); }
  }
  process.stdout.write(out.join('\n') + '\n');
} catch (e) { /* never fail the session */ }
process.exit(0);
