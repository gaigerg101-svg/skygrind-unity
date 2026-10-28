#!/usr/bin/env node
// SKYGRIND guard (FORGE61 section 4, 2026-10-22): a PreToolUse hook that blocks what CLAUDE.md forbids and the nights keep re-learning.
// Bash / PowerShell: the command text (tokenized, so a quoted commit message never trips a rule). Write / Edit / MultiEdit / NotebookEdit:
// the file path. A blocked call exits 2 with the reason on stderr (Claude reads it) and appends a line to Logs/guard-blocks.log.
// It fails open: a crash in this script allows the call and writes Logs/guard-errors.log (a bug here must never stop a night).
// Test every rule with planted cases: node .claude/hooks/guard.cjs --selftest
'use strict';
const fs = require('fs');
const path = require('path');
const cp = require('child_process');

const ROOT = (process.env.CLAUDE_PROJECT_DIR || path.resolve(__dirname, '..', '..')).replace(/\\/g, '/');
// GG's uncommitted edits (CLAUDE.md rule 1, L-047, NIGHT60.md section 9): never staged, committed, checked out, restored or edited.
const GG_FILES = [
  'ProjectSettings/UnityConnectSettings.asset', 'ProjectSettings/EditorBuildSettings.asset',
  'Assets/ProjectSettings/UIEffectProjectSettings.asset', 'Assets/Scenes/Arena.unity',
  'Assets/Resources/Fonts/Archivo-ExtraBold SDF.asset', 'Assets/Resources/Fonts/Archivo-ExtraCondensedBlackItalic SDF.asset',
  'Assets/Resources/Fonts/Archivo-SemiBold SDF.asset', 'Assets/Resources/Fonts/Tourney-BlackItalic SDF.asset',
].map(s => s.toLowerCase());
// Folders an rm / Remove-Item may never take whole (CLAUDE.md rule 10: delete only your own temporary output).
const PROTECTED_DIRS = ['', '.', '..', '/', '~', '.git', 'assets', 'projectsettings', 'packages', 'library', 'docs', 'design', 'portexport',
  'builds', 'logs', 'nights', 'tools', 'research', '.claude', '.claude/agents', '.claude/hooks', '.claude/skills'];
const DOC_CODE_EXT = /\.(md|cs|cjs|mjs|js|ts|py|sh|ps1|shader|hlsl|cginc|compute|asmdef|unity|prefab|asset|meta)$/i;
const WRITE_JS = /writeFileSync|appendFileSync|writeFile\s*\(|appendFile\s*\(|createWriteStream|renameSync|copyFileSync|unlinkSync|rmSync|rmdirSync|cpSync/;
const WRITE_PY = /open\([^)]*['"][wax]\+?b?['"]|write_text|write_bytes|shutil\.(copy|move|rmtree)|os\.(remove|unlink|rename|replace)/;
const WEB_REPO = /(^|[\\/])dev[\\/]skygrind([\\/]|$)/i;   // the frozen web repo (CLAUDE.md rule 2); skygrind-unity does not match
const minFreeGB = () => Number(process.env.SKYGRIND_GUARD_MIN_FREE_GB || 12);

// ---- a small shell tokenizer: segments split at unquoted ; && || | & newline ( ) $( `; quotes kept as one token ----
function tokenize(s, ps) {
  const segs = []; let seg = []; let cur = ''; let quoted = false; let q = null; let i = 0;
  const push = () => { if (cur !== '' || quoted) seg.push({ t: cur, q: quoted }); cur = ''; quoted = false; };
  const end = () => { push(); if (seg.length) segs.push(seg); seg = []; };
  while (i < s.length) {
    const c = s[i], n = s[i + 1];
    if (q) {
      if (c === q) { q = null; i++; continue; }
      if (q === '"' && !ps && c === '\\' && i + 1 < s.length) { cur += n; i += 2; continue; }
      cur += c; i++; continue;
    }
    if (c === "'" || c === '"') { q = c; quoted = true; i++; continue; }
    if (!ps && c === '\\' && i + 1 < s.length && n !== '\n') { cur += n; i += 2; continue; }
    if (ps && c === '`' && i + 1 < s.length) { cur += n; i += 2; continue; }
    if (c === ' ' || c === '\t' || c === '\r') { push(); i++; continue; }
    if (c === '\n' || c === ';' || c === '(' || c === ')') { end(); i++; continue; }
    if (c === '&' && n === '&') { end(); i += 2; continue; }
    if (c === '|' && n === '|') { end(); i += 2; continue; }
    if (c === '&' && n === '>') { push(); seg.push({ t: '>', op: true }); i += 2; continue; }
    if (c === '|' || c === '&') { end(); i++; continue; }
    if (c === '$' && n === '(') { end(); i += 2; continue; }
    if (!ps && c === '`') { end(); i++; continue; }
    if (c === '<' && n === '<' && s[i + 2] === '<') { cur += '<<<'; i += 3; continue; }   // a here-string feeds a string, writes nothing
    if (c === '<' && n === '<') { push(); seg.push({ t: '<<', op: true }); i += 2; continue; }
    if (c === '>') {
      push(); let op = '>'; i++;
      if (s[i] === '>') { op = '>>'; i++; }
      if (s[i] === '&') { i++; while (/\d/.test(s[i] || '')) i++; continue; }   // 2>&1: a fd copy, not a file
      seg.push({ t: op, op: true }); continue;
    }
    cur += c; i++;
  }
  end();
  return segs;
}

const base = t => t.replace(/\\/g, '/').split('/').pop().toLowerCase().replace(/\.exe$/, '');
function rel(t) {   // a path token -> repo-relative, lower case, forward slashes
  let p = t.replace(/\\/g, '/').trim();
  const roots = [ROOT, ROOT.replace(/^([A-Za-z]):/, (m, d) => '/' + d.toLowerCase())];
  for (const r of roots) if (p.toLowerCase().startsWith(r.toLowerCase() + '/')) p = p.slice(r.length + 1);
  while (p.startsWith('./')) p = p.slice(2);
  return p.replace(/\/+$/, '').toLowerCase();
}
function touchesGG(tok) {   // the token names a GG file, or a folder / glob / pathspec that holds one
  const p = rel(tok);
  if (p === '.' || p === '' || p === ':/' || p === ':/*' || p === '*') return true;
  const wild = p.search(/[*?[]/);
  const stem = wild >= 0 ? p.slice(0, wild) : null;
  return GG_FILES.some(g => g === p || g.startsWith(p + '/') || (stem !== null && g.startsWith(stem.replace(/[^/]*$/, ''))));
}
function freeGB() { try { const s = fs.statfsSync(ROOT); return s.bavail * s.bsize / 2 ** 30; } catch (e) { return Infinity; } }
// FORGE62b (GG 2026-10-22: "2 hour build prompt hard cap"): the run's clock, Logs/clock.json, written by Tools/night/n62-clock.cjs start.
// A budget in the prompt is advisory; this rule holds the cap: FREEZE (freezeMin on) lets only the close's heavy steps start (a tag in
// closeTags), CAP (capMin on) lets no heavy step, test run or agent start. A stopped clock, or one older than staleMin, is ignored.
function clockState() {
  let c = null;
  try { c = JSON.parse(process.env.SKYGRIND_GUARD_TEST_CLOCK || fs.readFileSync(path.join(ROOT, 'Logs', 'clock.json'), 'utf8')); } catch (e) { return null; }
  if (!c || c.stopped || !Number.isFinite(Number(c.t0))) return null;
  const now = Number(process.env.SKYGRIND_GUARD_TEST_NOW || Math.floor(Date.now() / 1000));
  const min = (now - Number(c.t0)) / 60;
  if (min < 0 || min >= Number(c.staleMin || 360)) return null;
  const hm = m => Math.floor(m / 60) + ':' + String(Math.floor(m % 60)).padStart(2, '0');
  return { min, at: hm(min), freezeMin: Number(c.freezeMin || 90), capMin: Number(c.capMin || 120), freezeAt: hm(Number(c.freezeMin || 90)),
    capAt: hm(Number(c.capMin || 120)), closeTags: (c.closeTags || []).map(t => String(t).toLowerCase()), night: c.night || '' };
}
function branchOf(dir) {
  if (process.env.SKYGRIND_GUARD_TEST_BRANCH) return process.env.SKYGRIND_GUARD_TEST_BRANCH;
  try { return cp.execFileSync('git', ['-C', dir, 'rev-parse', '--abbrev-ref', 'HEAD'], { encoding: 'utf8', timeout: 4000 }).trim(); }
  catch (e) { return ''; }
}
function cmdlineOfPid(pid) {   // a batch-mode Unity (-batchmode) may be stopped; the Editor never (red team 2, 2026-10-22)
  if (process.env.SKYGRIND_GUARD_TEST_CMDLINE !== undefined) return process.env.SKYGRIND_GUARD_TEST_CMDLINE;
  try { return cp.execFileSync('powershell', ['-NoProfile', '-Command', '(Get-CimInstance Win32_Process -Filter "ProcessId=' + pid + '").CommandLine'], { encoding: 'utf8', timeout: 8000 }).trim(); }
  catch (e) { return ''; }
}
function imageOfPid(pid) {
  if (process.env.SKYGRIND_GUARD_TEST_IMAGE) return process.env.SKYGRIND_GUARD_TEST_IMAGE;
  try { return cp.execFileSync('tasklist', ['/FI', 'PID eq ' + pid, '/FO', 'CSV', '/NH'], { encoding: 'utf8', timeout: 4000 }).split(',')[0].replace(/"/g, ''); }
  catch (e) { return ''; }
}

// ---- the rules: return [ruleId, reason] or null ----
function checkSegment(seg, ps, cwd, depth) {
  const toks = seg.filter(x => !x.op).map(x => x.t);
  const ops = seg.filter(x => x.op).map(x => x.t);
  let i = 0;
  while (i < toks.length && /^[A-Za-z_][A-Za-z0-9_]*=/.test(toks[i])) i++;
  // FORGE70: a shell keyword before the command (`for f in x; do node -e ...`, `then`, `{`) hid the program from every rule (NIGHT69's lead
  // ran a `node -e` file write inside a for loop and the guard allowed it)
  while (i < toks.length && ['time', 'nohup', 'exec', 'command', 'env', 'sudo', 'do', 'then', 'else', 'elif', 'if', 'while', 'until', '{', '!'].includes(base(toks[i]))) i++;
  if (i < toks.length && base(toks[i]) === 'timeout') { i++; while (i < toks.length && /^-|^\d/.test(toks[i])) i++; }
  const prog = i < toks.length ? base(toks[i]) : '';
  const args = toks.slice(i + 1);
  const argsL = args.map(a => a.toLowerCase());

  // nested shells: analyze the inner command
  const inner = (idx) => (idx >= 0 && idx + 1 < args.length ? checkCommand(args.slice(idx + 1).join(' '), ['powershell', 'pwsh'].includes(prog), cwd, depth + 1) : null);
  if (['bash', 'sh', 'zsh'].includes(prog)) { const k = args.findIndex(a => /^-[a-z]*c$/.test(a)); if (k >= 0) { const r = inner(k); if (r) return r; } }
  if (['powershell', 'pwsh'].includes(prog)) { const k = argsL.findIndex(a => a === '-command' || a === '-c'); if (k >= 0) { const r = inner(k); if (r) return r; } }
  if (prog === 'cmd') { const k = argsL.findIndex(a => a === '/c' || a === '/k'); if (k >= 0) { const r = inner(k); if (r) return r; } }
  if (prog === 'eval') { const r = checkCommand(args.join(' '), ps, cwd, depth + 1); if (r) return r; }

  // CLAUDE.md rule 9 / L-114: docs and code are written with the Write / Edit tools, never a heredoc, node -e or an in-place editor
  if (ops.includes('<<') && prog !== 'git') return ['rule9-heredoc', 'a heredoc (<<). CLAUDE.md rule 9 / L-114: write docs and code with the Write / Edit tools; put multi-line code in a .cjs / .cs / .py file you write with Write, then run it.'];
  if (prog === 'node') { const k = argsL.findIndex(a => ['-e', '--eval', '-p', '--print'].includes(a)); if (k >= 0 && WRITE_JS.test(args[k + 1] || '')) return ['rule9-node-e', 'node -e that writes files. CLAUDE.md rule 9: write a .cjs file with the Write tool, then run node on it.']; }
  if (['python', 'python3', 'py'].includes(prog)) { const k = argsL.indexOf('-c'); if (k >= 0 && WRITE_PY.test(args[k + 1] || '')) return ['rule9-python-c', 'python -c that writes files. CLAUDE.md rule 9: write a .py file with the Write tool, then run it.']; }
  if (prog === 'sed' && args.some(a => /^-[a-z]*i/.test(a) || a.startsWith('--in-place'))) return ['rule9-sed-i', 'sed -i edits a file in place. CLAUDE.md rule 9: use the Edit tool.'];
  if (prog === 'perl' && args.some(a => /^-[a-z]*i/.test(a))) return ['rule9-perl-i', 'perl -i edits a file in place. CLAUDE.md rule 9: use the Edit tool.'];
  for (let k = 0; k < seg.length; k++) {
    if (seg[k].op && (seg[k].t === '>' || seg[k].t === '>>') && seg[k + 1] && DOC_CODE_EXT.test(seg[k + 1].t)) return ['rule9-redirect', 'a shell redirect into ' + seg[k + 1].t + '. CLAUDE.md rule 9: write docs and code with the Write / Edit tools.'];
  }
  if (prog === 'tee' && args.some(a => DOC_CODE_EXT.test(a))) return ['rule9-tee', 'tee into a doc / code file. CLAUDE.md rule 9: use the Write / Edit tools.'];
  if (['set-content', 'add-content', 'out-file', 'sc', 'ac'].includes(prog) && args.some(a => DOC_CODE_EXT.test(a))) return ['rule9-ps-write', prog + ' into a doc / code file. CLAUDE.md rule 9: use the Write / Edit tools.'];
  if (/\[(system\.)?io\.file\]::(writealltext|writealllines|appendalltext)/i.test(toks.join(' '))) return ['rule9-ps-write', '[IO.File] writes a file. CLAUDE.md rule 9: use the Write / Edit tools.'];

  // git (CLAUDE.md rule 1; NIGHT60 D-3; L-047)
  if (prog === 'git') {
    let j = 0;
    while (j < args.length && /^-/.test(args[j])) { if (['-C', '-c', '--git-dir', '--work-tree', '--namespace'].includes(args[j])) j++; j++; }
    const sub = (args[j] || '').toLowerCase();
    const rest = args.slice(j + 1);
    const restL = rest.map(a => a.toLowerCase());
    const cIdx = args.indexOf('-C');
    const gitDir = cIdx >= 0 && args[cIdx + 1] ? args[cIdx + 1] : cwd;
    const short = f => rest.some(a => /^-[a-zA-Z]+$/.test(a) && a.includes(f));
    if (sub === 'push') return ['git-push', 'git push. CLAUDE.md rule 1: never push unless GG says so in that session.'];
    if (sub === 'pull' || (sub === 'merge' && !restL.includes('--abort'))) return ['git-merge', 'git ' + sub + '. CLAUDE.md rule 1: never merge (builders are integrated by cherry-pick, L-066).'];
    if (sub === 'rebase' && !restL.some(a => a === '--abort' || a === '--quit')) return ['git-rebase', 'git rebase rewrites history (NIGHT60 rules: never rebase).'];
    if (['filter-branch', 'filter-repo', 'replace'].includes(sub)) return ['git-history', 'git ' + sub + ' rewrites history.'];
    if (sub === 'add') {
      if (restL.some(a => ['-a', '-A', '--all', '-u', '--update', '--no-ignore-removal'].includes(a) || a === '-a') || rest.includes('-A') || short('A') || short('u'))
        return ['git-add-all', 'git add -A / --all / -u. CLAUDE.md rule 1: stage explicit paths (plus their .meta) only.'];
      const paths = rest.filter(a => !a.startsWith('-'));
      if (paths.some(touchesGG)) return ['gg-files', 'git add of "." or a path holding GG\'s files (the four ProjectSettings / Arena.unity files and the four TMP fonts in Assets/Resources/Fonts). CLAUDE.md rule 1 / L-047: never stage them.'];
    }
    if (sub === 'commit') {
      if (restL.includes('--amend')) return ['git-amend', 'git commit --amend rewrites the last commit: make a new commit.'];
      if (restL.includes('--all') || short('a')) return ['git-commit-all', 'git commit -a stages every modified file, GG\'s included. CLAUDE.md rule 1: commit explicit paths.'];
      const valued = new Set(['-m', '-F', '-C', '-c', '--author', '--date', '--template', '--fixup', '--squash', '--cleanup', '--trailer', '--file', '--message']);
      const paths = []; let dd = false;
      for (let k = 0; k < rest.length; k++) {
        const a = rest[k];
        if (dd) { paths.push(a); continue; }
        if (a === '--') { dd = true; continue; }
        if (valued.has(a)) { k++; continue; }
        if (a.startsWith('-')) continue;
        paths.push(a);
      }
      if (paths.some(touchesGG)) return ['gg-files', 'git commit naming "." or a path holding GG\'s files. CLAUDE.md rule 1 / L-047: never commit them.'];
      const b = branchOf(gitDir);
      if (b === 'master' || b === 'main') return ['git-master', 'git commit on ' + b + '. CLAUDE.md rule 1: never commit to master (the night branch is port/stage3).'];
    }
    if (sub === 'checkout' || sub === 'restore' || sub === 'switch') {
      if (restL.some(a => a === '-f' || a === '--force' || a === '--discard-changes')) return ['git-discard', 'git ' + sub + ' --force discards local changes, GG\'s included (L-047).'];
      const nonOpt = rest.filter(a => !a.startsWith('-'));
      if ((sub === 'checkout' || sub === 'switch') && ['master', 'main'].includes((nonOpt[0] || '').toLowerCase()) && !rest.includes('--')) return ['git-master', 'git ' + sub + ' ' + nonOpt[0] + ': the night works on port/stage3 only (CLAUDE.md rule 1).'];
      if (sub !== 'switch' && nonOpt.some(touchesGG)) return ['gg-files', 'git ' + sub + ' of "." or a path holding GG\'s files would discard his uncommitted edits (L-047, NIGHT60 D-3). Restore only your own files by explicit path.'];
    }
    if (sub === 'reset' && restL.some(a => ['--hard', '--merge', '--keep'].includes(a))) return ['git-reset-hard', 'git reset --hard discards GG\'s uncommitted edits (NIGHT60 D-3). Undo your own commit with git revert.'];
    if (sub === 'clean' && rest.some(a => /^-[a-zA-Z]*f/.test(a) || a === '--force')) return ['git-clean', 'git clean deletes untracked files (GG\'s prompts and decisions are untracked).'];
    if (sub === 'stash' && !['list', 'show'].includes(restL[0] || '')) return ['git-stash', 'git stash takes GG\'s uncommitted edits out of the tree (L-047).'];
    if (sub === 'worktree' && restL[0] === 'remove') return ['worktrees', 'git worktree remove: deleting .claude/worktrees is GG\'s call.'];
    if (sub === 'branch' && rest.some(a => a === '-D' || (/^-[a-zA-Z]+$/.test(a) && a.includes('D')) || a === '--force')) return ['git-branch-D', 'git branch -D deletes unmerged work kept for GG (NIGHT60 D-13).'];
    if (sub === 'tag' && restL.some(a => ['-d', '--delete', '-f', '--force'].includes(a))) return ['git-tag', 'deleting or moving a tag changes a night\'s record (L-107).'];
  }
  // dotnet format rewrites code unless it only verifies (research-deadcode.md: without --verify-no-changes it deletes the members it flags)
  if (prog === 'dotnet' && argsL[0] === 'format' && !argsL.includes('--verify-no-changes')) return ['dotnet-format', 'dotnet format without --verify-no-changes rewrites (deletes) code. Run it with --verify-no-changes on a scratch copy, then delete by hand in a reviewed batch.'];
  if (prog === 'gh' && argsL[0] === 'auth' && argsL[1] === 'switch') return ['gh-auth-switch', 'gh auth switch. CLAUDE.md rule 1: never, unless GG says so in that session.'];
  if (prog === 'unity' && argsL[0] === 'test') return ['unity-test', 'unity test starts a second Editor. Use bash Tools/unity-loop.sh tests <filter> (CLAUDE.md, Workflow).'];
  // FORGE62 (T-147, L-126): a synchronous test run that reaches MapChoiceTests' map cases holds the live Editor past the 600 s limit; NIGHT60's
  // close left one running and NIGHT61 had no Editor all night. run_tests matches its filter by substring of the full test name, so a filter
  // reaches the slow test when it is a substring of its name (or empty: the whole suite). Async runs (--async_tests true) and batch copies pass.
  const SLOW_M = 'skygrind.game.tests.mapchoicetests.listedmap_loadsandplays600steps';
  const reachesM = f => !f || SLOW_M.includes(String(f).toLowerCase());
  const mMsg = what => 'a synchronous test run that reaches Game M\'s MapChoiceTests (' + what + '). T-147 / L-126: it wedged the live Editor for all of NIGHT61. Run Game M with `unity command run_tests --async_tests true --timeout 2400` and poll test_status, or in batch mode on a copy; name other classes explicitly.';
  if (prog === 'unity' && argsL[0] === 'command' && argsL[1] === 'run_tests') {
    const k = argsL.indexOf('--filter'); const filter = k >= 0 ? args[k + 1] : '';
    const asyncOn = argsL.some((a, idx) => a === '--async_tests' && /^true$/i.test(args[idx + 1] || ''));
    if (!asyncOn && reachesM(filter)) return ['game-m-sync', mMsg(filter ? 'filter ' + filter : 'no filter: the whole suite')];
  }
  if (['bash', 'sh'].includes(prog) && args[0] && base(args[0]) === 'unity-loop.sh' && (args[1] || '').toLowerCase() === 'tests' && reachesM(args[2])) return ['game-m-sync', mMsg(args[2] ? 'filter ' + args[2] : 'no filter: the whole suite')];
  if (['bash', 'sh'].includes(prog) && args[0] && base(args[0]) === 'n41-suite.sh') {
    const only = (args[2] || '').toLowerCase(); const letters = args[3];
    if (only !== 'core' && (!letters || letters.includes('M'))) return ['game-m-sync', mMsg('n41-suite.sh with Game and the letter M' + (letters ? '' : ' (the default letters)'))];
  }

  // deleting: worktrees are GG's call; whole project folders never (CLAUDE.md rule 10)
  if (['rm', 'rmdir', 'remove-item', 'ri', 'del', 'rd', 'erase'].includes(prog)) {
    const paths = args.filter(a => !a.startsWith('-') && !/^\/[a-z]$/i.test(a));
    for (const a of paths) {
      const p = rel(a);
      if (p.includes('.claude/worktrees')) return ['worktrees', 'deleting under .claude/worktrees is GG\'s call (FORGE61; HANDOFF).'];
      const stem = p.replace(/\/?\*.*$/, '');
      if (PROTECTED_DIRS.includes(p) || (p.includes('*') && PROTECTED_DIRS.includes(stem)) || /^[a-z]:\/?$/.test(p) || rel(a) === rel(ROOT)) return ['rm-protected', 'deleting a whole project folder (' + a + '). CLAUDE.md rule 10: delete only your own temporary output.'];
      if (WEB_REPO.test(a)) return ['web-repo', 'the web repo is read only (CLAUDE.md rule 2).'];
    }
  }
  // never kill the Unity Editor (CLAUDE.md rule 10)
  if (['taskkill', 'stop-process', 'spps', 'kill', 'pkill', 'killall'].includes(prog)) {
    if (args.some(a => /^unity(\.exe)?$/i.test(a) || /unity\.exe/i.test(a))) return ['kill-editor', 'killing the Unity Editor. CLAUDE.md rule 10: never kill the Editor; close only the Players you launched, by PID.'];
    for (let k = 0; k < args.length; k++) {
      if (['/pid', '-pid', '-id'].includes(argsL[k].replace(/^\/+/, '/')) && args[k + 1]) {   // Git Bash writes /PID as //PID
        const pid = args[k + 1].replace(/\D/g, '');
        const img = imageOfPid(pid);
        if (/^unity\.exe$/i.test(img) && !/-batchmode/i.test(cmdlineOfPid(pid))) return ['kill-editor', 'PID ' + args[k + 1] + ' is Unity.exe without -batchmode (the Editor). CLAUDE.md rule 10: never kill it; a batch-mode Unity may be stopped by its PID.'];
      }
    }
  }
  // disk: builds and reels write gigabytes (L-104, L-113)
  const script = args[0] ? base(args[0]) : '';
  const heavy = (['bash', 'sh'].includes(prog) && /^(n\d+-build|n\d+-reels|reel|n\d+-smoke[-a-z]*|n\d+-focus-probe)\.sh$/.test(script))
    || (prog === 'unity' && argsL[0] === 'command' && argsL[1] === 'build')
    || (prog === 'unity' && argsL.includes('-batchmode') && (argsL.includes('-buildtarget') || argsL.includes('-executemethod') || argsL.includes('-runtests')));   // batch builds and test runs write gigabytes too
  if (heavy) { const g = freeGB(); if (g < minFreeGB()) return ['disk', g.toFixed(1) + ' GB free on the project drive, under ' + minFreeGB() + ' GB: a build or a reel set writes ~10 GB (L-104, L-113). Prune your own reel frames (Tools/night/n62-prune.sh <tag>) first.']; }
  // the clock (FORGE62b): a heavy step or a test run started late is what pushes a short run past its cap
  const testRun = (prog === 'unity' && argsL[0] === 'command' && argsL[1] === 'run_tests')
    || (['bash', 'sh'].includes(prog) && args[0] && (base(args[0]) === 'n41-suite.sh' || (base(args[0]) === 'unity-loop.sh' && (args[1] || '').toLowerCase() === 'tests')));
  if (heavy || testRun) {
    const clk = clockState();
    if (clk) {
      const line = toks.join(' ').toLowerCase();
      const tagged = clk.closeTags.some(t => line.includes(t));
      if (clk.min >= clk.capMin) return ['clock', 'the clock reads ' + clk.at + ' since t0, past the ' + clk.capAt + ' hard cap (Logs/clock.json): no build, reel set, smoke or test run starts now. Log it under Blocked with this line, commit what is done and end.'];
      if (clk.min >= clk.freezeMin && (testRun || !tagged)) return ['clock', 'the clock reads ' + clk.at + ' since t0, past the ' + clk.freezeAt + ' freeze (Logs/clock.json): only the close\'s steps start now (a build, reel set or smoke tagged ' + clk.closeTags.join(' / ') + '; no test run). Log this one under Blocked and go to the close.'];
    }
  }
  // the web repo: no git writes there (CLAUDE.md rule 2)
  if (prog === 'git' && (WEB_REPO.test(cwd || '') || args.some(a => WEB_REPO.test(a))) && /^(commit|checkout|switch|reset|merge|rebase|push|add|stash|clean|restore|tag|branch)$/i.test(args.find(a => !a.startsWith('-') && !WEB_REPO.test(a)) || '')) return ['web-repo', 'git changes in the web repo. CLAUDE.md rule 2: it is read only.'];
  return null;
}

function checkCommand(cmd, ps, cwd, depth) {
  if (depth > 3 || !cmd) return null;
  for (const seg of tokenize(String(cmd), ps)) { const r = checkSegment(seg, ps, cwd, depth || 0); if (r) return r; }
  return null;
}

function checkFile(fp) {
  if (!fp) return null;
  if (WEB_REPO.test(fp)) return ['web-repo', 'the web repo is read only (CLAUDE.md rule 2).'];
  const p = rel(fp);
  if (GG_FILES.includes(p)) return ['gg-files', p + ' holds GG\'s uncommitted edits: never edit it (L-047).'];
  return null;
}

// FORGE62 (L-116, broken three times on 2026-10-22): a background `&` that ends an `&&` chain backgrounds the whole chain, so the chain's
// output (a cherry-pick, a log, a check) is lost and its `cd` does not apply to what follows. Scan the raw Bash text: quotes and parentheses
// tracked; a top-level `&` (not `&&`, `&>` or `>&`) after a top-level `&&` in the same statement (no `;` or newline between) is refused.
function bgChain(s) {
  let q = null, depth = 0, sawAnd = false;
  for (let i = 0; i < s.length; i++) {
    const c = s[i], n = s[i + 1], p = s[i - 1];
    if (q) { if (c === q) q = null; else if (c === '\\' && q === '"') i++; continue; }
    if (c === "'" || c === '"') { q = c; continue; }
    if (c === '\\') { i++; continue; }
    if (c === '(') { depth++; continue; }
    if (c === ')') { depth = Math.max(0, depth - 1); continue; }
    if (depth > 0) continue;
    if (c === ';' || c === '\n') { sawAnd = false; continue; }
    if (c === '&' && n === '&') { sawAnd = true; i++; continue; }
    if (c === '&' && (n === '>' || p === '>')) continue;   // &> and 2>&1 are redirects
    if (c === '&' && sawAnd) return true;
  }
  return false;
}

function evaluate(input) {
  const tool = input.tool_name || '';
  const ti = input.tool_input || {};
  if (tool === 'Bash' && bgChain(String(ti.command || ''))) return ['bg-chain', 'a background `&` at the end of an `&&` chain backgrounds the whole chain: its output is lost and its cd does not apply (L-116). Use the Bash tool\'s run_in_background, or put the job in its own statement: `( ... ) > file 2>&1 &`.'];
  if (tool === 'Agent' || tool === 'Task') {   // FORGE62b: an agent started after the freeze outlives the cap (a critic takes 17-23 min, L-109)
    const clk = clockState();
    if (clk && clk.min >= clk.freezeMin) return ['clock', 'the clock reads ' + clk.at + ' since t0, past the ' + clk.freezeAt + ' freeze (Logs/clock.json): no new agent (' + (ti.subagent_type || 'general') + ') starts now; a running one may finish. Log it under Blocked and go to the close.'];
    return null;
  }
  if (tool === 'Bash' || tool === 'PowerShell') return checkCommand(ti.command || '', tool === 'PowerShell', input.cwd || ROOT, 0);
  if (['Write', 'Edit', 'MultiEdit', 'NotebookEdit'].includes(tool)) return checkFile(ti.file_path || ti.notebook_path || '');
  return null;
}

function log(file, line) { try { fs.mkdirSync(path.join(ROOT, 'Logs'), { recursive: true }); fs.appendFileSync(path.join(ROOT, 'Logs', file), line + '\n'); } catch (e) { /* never block on logging */ } }

function selftest() {
  const B = (tool, cmd, rule, env) => ({ tool, cmd, want: rule, env });
  const A = (tool, cmd, env) => ({ tool, cmd, want: null, env });
  const clk = (min, stopped) => ({ SKYGRIND_GUARD_TEST_CLOCK: JSON.stringify({ night: 'n62', t0: 1000000, freezeMin: 90, capMin: 120, staleMin: 360,
    closeTags: ['n62-freeze'], stopped: !!stopped }), SKYGRIND_GUARD_TEST_NOW: String(1000000 + min * 60), SKYGRIND_GUARD_MIN_FREE_GB: '0' });
  const cases = [
    B('Bash', 'git push', 'git-push'), B('Bash', 'git push --dry-run origin port/stage3', 'git-push'), B('PowerShell', 'git push', 'git-push'),
    B('Bash', 'cd /c/x && git -C /c/Users/gaige/dev/SKYGRIND-unity push', 'git-push'), B('Bash', 'bash -c "git push"', 'git-push'),
    A('Bash', 'git commit -m "never git push, never git add -A" -- NIGHT61-LOG.md'),
    B('Bash', 'git add -A', 'git-add-all'), B('Bash', 'git add .', 'gg-files'), B('Bash', 'git add --all -n', 'git-add-all'), B('Bash', 'git add -u', 'git-add-all'),
    A('Bash', 'git add Tools/night/n61-gates.sh Assets/Scripts/Game/X.cs Assets/Scripts/Game/X.cs.meta'),
    B('Bash', 'git add --dry-run ProjectSettings/UnityConnectSettings.asset', 'gg-files'),
    B('Bash', 'git add "Assets/Resources/Fonts/Archivo-SemiBold SDF.asset"', 'gg-files'), B('Bash', 'git add Assets/Resources/Fonts/*.asset', 'gg-files'),
    B('Bash', 'git add ProjectSettings', 'gg-files'), B('PowerShell', 'git add Assets\\Scenes\\Arena.unity', 'gg-files'),
    A('Bash', 'git add -f Logs/n61/slate.json'),
    B('Bash', 'git commit -am "x"', 'git-commit-all'), B('Bash', 'git commit -m "x" -- ProjectSettings/EditorBuildSettings.asset', 'gg-files'),
    B('Bash', 'git commit --amend --no-edit', 'git-amend'), B('Bash', 'git commit -m "x" -- a.md', 'git-master', { SKYGRIND_GUARD_TEST_BRANCH: 'master' }),
    A('Bash', 'git commit -m "x" -- a.md', { SKYGRIND_GUARD_TEST_BRANCH: 'port/stage3' }),
    B('Bash', 'git checkout master', 'git-master'), B('Bash', 'git switch main', 'git-master'),
    B('Bash', 'git checkout -- .', 'gg-files'), B('Bash', 'git restore .', 'gg-files'), B('Bash', 'git checkout -- ProjectSettings/UnityConnectSettings.asset', 'gg-files'),
    A('Bash', 'git checkout -- "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset"'),
    A('Bash', 'git checkout -b n61-b1'), A('Bash', 'git cherry-pick 1a8df4ac'), A('Bash', 'git revert --no-edit 1a8df4ac'),
    B('Bash', 'git reset --hard HEAD~1', 'git-reset-hard'), A('Bash', 'git reset -- Design/unity-config.json'),
    B('Bash', 'git clean -fd', 'git-clean'), A('Bash', 'git clean -n'), B('Bash', 'git stash', 'git-stash'), A('Bash', 'git stash list'),
    B('Bash', 'git merge n61-b1', 'git-merge'), B('Bash', 'git rebase port/stage3', 'git-rebase'), B('Bash', 'git branch -D n60-b10', 'git-branch-D'),
    B('Bash', 'git worktree remove .claude/worktrees/agent-x', 'worktrees'), B('Bash', 'rm -rf .claude/worktrees/agent-a957c6fb543911d91', 'worktrees'),
    B('PowerShell', 'Remove-Item -Recurse -Force .claude\\worktrees', 'worktrees'), B('Bash', 'rm -rf Builds', 'rm-protected'), B('Bash', 'rm -rf Logs/*', 'rm-protected'),
    A('Bash', 'rm -rf Builds/n61-b1'), A('Bash', 'rm -f Logs/n61/scratch.txt'),
    B('Bash', 'gh auth switch', 'gh-auth-switch'), B('Bash', 'unity test --project-path .', 'unity-test'),
    B('Bash', 'unity command run_tests --caller plugin --skill unity-cli', 'game-m-sync'), A('Bash', 'bash Tools/unity-loop.sh tests N61'),
    B('Bash', 'unity command run_tests --mode EditMode --filter Skygrind.Game.Tests.MapChoiceTests. --timeout 600 --caller plugin --skill unity-cli', 'game-m-sync'),
    A('Bash', 'unity command run_tests --mode EditMode --filter Skygrind.Game.Tests.MapChoiceTests. --async_tests true --timeout 2400 --caller plugin --skill unity-cli'),
    A('Bash', 'unity command run_tests --mode EditMode --filter Skygrind.Game.Tests.M9Fade25Tests. --caller plugin --skill unity-cli'),
    B('Bash', 'bash Tools/unity-loop.sh tests Skygrind.Game.Tests.M', 'game-m-sync'), B('Bash', 'bash Tools/unity-loop.sh tests', 'game-m-sync'),
    B('Bash', 'bash Tools/night/n41-suite.sh n62', 'game-m-sync'), B('Bash', 'bash Tools/night/n41-suite.sh split Game MNPRSTUVWZ class', 'game-m-sync'),
    A('Bash', 'timeout 900 bash Tools/night/n41-suite.sh n61 Game ABCDEFGHIJKLNOPQRSTUVWXYZ'), A('Bash', 'bash Tools/night/n41-suite.sh n62 Core'),
    B('Bash', 'cd /c/x && git log -1 && ( sleep 1 ) > /dev/null 2>&1 &', 'bg-chain'), B('Bash', 'bash -n a.sh && echo ok && (bash a.sh > out.txt 2>&1) &', 'bg-chain'),
    A('Bash', '(bash Tools/night/n62-gates.sh > out.txt 2>&1; echo done >> out.txt) > /dev/null 2>&1 &'), A('Bash', 'cd /c/x; (sleep 1) > /dev/null 2>&1 &'),
    A('Bash', 'git log -1 && echo ok'), A('Bash', 'ls &> /dev/null && echo "a & b" && echo c 2>&1'),
    B('Bash', "cat > NOTES.md <<'EOF'\nhello\nEOF", 'rule9-heredoc'), B('Bash', "python - <<'EOF'\nopen('x.py','w')\nEOF", 'rule9-heredoc'),
    A('Bash', 'IFS=, read -r -a parts <<< "a,b,c"'),
    B('Bash', 'dotnet format style Skygrind.Core.csproj --diagnostics IDE0051', 'dotnet-format'),
    A('Bash', 'dotnet format style C:/scratch/Skygrind.Core.csproj --diagnostics IDE0051 --verify-no-changes'),
    B('Bash', 'node -e "require(\'fs\').writeFileSync(\'a.md\',\'x\')"', 'rule9-node-e'), B('Bash', 'for f in a.md b.md; do node -e "require(\'fs\').writeFileSync(process.argv[1],\'x\')" $f; done', 'rule9-node-e'),A('Bash', 'node -e "console.log(require(\'./Design/unity-config.json\').design.n60)"'),
    B('Bash', 'python -c "open(\'x.cs\',\'w\').write(\'y\')"', 'rule9-python-c'), B('Bash', 'sed -i s/a/b/ NIGHT61-LOG.md', 'rule9-sed-i'),
    B('Bash', 'echo hi > NIGHT61-LOG.md', 'rule9-redirect'), A('Bash', 'python -I Tools/night/n60-v-lamp.py a b > Logs/n61/lamp.txt 2>&1'),
    B('PowerShell', "Set-Content -Path HANDOFF.md -Value 'x'", 'rule9-ps-write'),
    B('Bash', 'taskkill //IM Unity.exe //F', 'kill-editor'), B('Bash', 'taskkill //PID 4242 //F', 'kill-editor', { SKYGRIND_GUARD_TEST_IMAGE: 'Unity.exe' }),
    A('Bash', 'taskkill //PID 4242 //F', { SKYGRIND_GUARD_TEST_IMAGE: 'SKYGRIND.exe' }),
    B('Bash', 'taskkill //PID 4242 //F', 'kill-editor', { SKYGRIND_GUARD_TEST_IMAGE: 'Unity.exe', SKYGRIND_GUARD_TEST_CMDLINE: '"C:\\Unity\\Editor\\Unity.exe" -projectpath C:\\Users\\gaige\\dev\\SKYGRIND-unity' }),
    A('Bash', 'taskkill //PID 4242 //F', { SKYGRIND_GUARD_TEST_IMAGE: 'Unity.exe', SKYGRIND_GUARD_TEST_CMDLINE: '"C:\\Unity\\Editor\\Unity.exe" -batchmode -projectPath C:\\Users\\gaige\\dev\\SKYGRIND-unity-batch -runTests' }),
    B('Bash', '"C:/Program Files/Unity/Editor/Unity.exe" -batchmode -projectPath C:/Users/gaige/dev/SKYGRIND-unity-batch -runTests -testPlatform EditMode', 'disk', { SKYGRIND_GUARD_MIN_FREE_GB: '100000' }),
    B('Bash', 'bash Tools/night/n41-build.sh n61-b1', 'disk', { SKYGRIND_GUARD_MIN_FREE_GB: '100000' }),
    B('Bash', 'bash Tools/night/n61-reels.sh n61-b1 vf,n52-foundry,verbs37', 'disk', { SKYGRIND_GUARD_MIN_FREE_GB: '100000' }),
    A('Bash', 'bash Tools/night/n41-build.sh n61-b1', { SKYGRIND_GUARD_MIN_FREE_GB: '0' }),
    B('Bash', 'git -C C:/Users/gaige/dev/skygrind commit -m x', 'web-repo'), A('Bash', 'git -C C:/Users/gaige/dev/skygrind log --oneline -3'),
    // the clock (FORGE62b): t0 = 1000000, FREEZE at 90 min, CAP at 120, stale at 360; the disk rule held off with a 0 GB floor
    A('Bash', 'bash Tools/night/n41-build.sh n62-p0', clk(30)), B('Bash', 'bash Tools/night/n41-build.sh n62-b3', 'clock', clk(95)),
    A('Bash', 'bash Tools/night/n41-build.sh n62-freeze', clk(95)), A('Bash', 'bash Tools/night/n62-reels.sh n62-freeze vf,n52-foundry,verbs37', clk(95)),
    B('Bash', 'bash Tools/night/n62-reels.sh n62-b2 vf,n52-foundry,verbs37', 'clock', clk(95)), A('Bash', 'bash Tools/night/n56-smoke-steal.sh n62-freeze', clk(100)),
    B('Bash', 'unity command run_tests --mode EditMode --filter Skygrind.Game.Tests.N61SharedTests. --caller plugin --skill unity-cli', 'clock', clk(95)),
    A('Bash', 'unity command run_tests --mode EditMode --filter Skygrind.Game.Tests.N61SharedTests. --caller plugin --skill unity-cli', clk(30)),
    B('Bash', 'bash Tools/unity-loop.sh tests N61', 'clock', clk(91)), B('Bash', 'bash Tools/night/n41-build.sh n62-freeze', 'clock', clk(125)),
    B('Bash', 'bash Tools/night/n56-smoke-steal.sh n62-freeze', 'clock', clk(121)),
    A('Bash', 'git commit -m "NIGHT62 close" -- NIGHT62-LOG.md', Object.assign(clk(125), { SKYGRIND_GUARD_TEST_BRANCH: 'port/stage3' })),
    A('Bash', 'bash Tools/night/n41-build.sh n62-b3', clk(400)), A('Bash', 'bash Tools/night/n41-build.sh n62-b3', clk(95, true)),
    { tool: 'Agent', agent: 'skygrind-critic', want: null, env: clk(60) }, { tool: 'Agent', agent: 'skygrind-critic', want: 'clock', env: clk(91) },
    { tool: 'Task', agent: 'gg-proxy', want: 'clock', env: clk(125) }, { tool: 'Agent', agent: 'skygrind-spotter', want: null, env: clk(95, true) },
    { tool: 'Edit', file: 'C:\\Users\\gaige\\dev\\SKYGRIND-unity\\ProjectSettings\\UnityConnectSettings.asset', want: 'gg-files' },
    { tool: 'Write', file: 'C:\\Users\\gaige\\dev\\skygrind\\src\\main.js', want: 'web-repo' },
    { tool: 'Write', file: 'C:\\Users\\gaige\\dev\\SKYGRIND-unity\\NIGHT61-LOG.md', want: null },
  ];
  let bad = 0;
  for (const c of cases) {
    const saved = {};
    for (const k of Object.keys(c.env || {})) { saved[k] = process.env[k]; process.env[k] = c.env[k]; }
    const input = c.file ? { tool_name: c.tool, tool_input: { file_path: c.file }, cwd: ROOT }
      : c.agent ? { tool_name: c.tool, tool_input: { subagent_type: c.agent, prompt: 'x' }, cwd: ROOT } : { tool_name: c.tool, tool_input: { command: c.cmd }, cwd: ROOT };
    const r = evaluate(input);
    for (const k of Object.keys(saved)) { if (saved[k] === undefined) delete process.env[k]; else process.env[k] = saved[k]; }
    const got = r ? r[0] : null;
    const ok = got === c.want;
    if (!ok) bad++;
    console.log((ok ? 'PASS ' : 'FAIL ') + (c.want ? 'block ' + c.want : 'allow') + ' <- ' + c.tool + ': ' + JSON.stringify(c.cmd || c.file || c.agent) + (ok ? '' : '  (got ' + got + ')'));
  }
  console.log('guard selftest: ' + (cases.length - bad) + ' / ' + cases.length + ' as planted' + (bad ? ', ' + bad + ' WRONG' : ''));
  process.exit(bad ? 1 : 0);
}

if (process.argv.includes('--selftest')) selftest();
else {
  let raw = '';
  process.stdin.on('data', d => { raw += d; });
  process.stdin.on('end', () => {
    try {
      const input = JSON.parse(raw || '{}');
      const r = evaluate(input);
      if (r) {
        const what = input.tool_input && (input.tool_input.command || input.tool_input.file_path) || '';
        log('guard-blocks.log', new Date().toISOString() + ' ' + (input.tool_name || '?') + ' ' + r[0] + ' ' + JSON.stringify(String(what).slice(0, 300)));
        process.stderr.write('SKYGRIND guard blocked this (' + r[0] + '): ' + r[1] + '\n');
        process.exit(2);
      }
      process.exit(0);
    } catch (e) {
      log('guard-errors.log', new Date().toISOString() + ' ' + String(e && e.stack || e).slice(0, 500));
      process.exit(0);
    }
  });
}
