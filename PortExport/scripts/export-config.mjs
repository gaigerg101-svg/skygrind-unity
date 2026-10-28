// A. config.json (the live config object, JSON.stringify of src/config.js's export) and config.notes.md (every
// comment the source puts next to a value, verbatim, with the value's dotted path). Units are what the comments say;
// nothing is inferred. Run: node port-export/scripts/export-config.mjs
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { REPO_DIR, config, writeJson, writeText } from './lib.mjs';

const SOURCE = 'src/config.js';

export function exportConfig() {
  const json = writeJson('config.json', config, { pretty: true });
  const roundTrip = JSON.stringify(JSON.parse(JSON.stringify(config))) === JSON.stringify(config);

  const lines = readFileSync(join(REPO_DIR, SOURCE), 'utf8').split(/\r?\n/);
  const notes = parseComments(lines);
  const header = lines.slice(0, 4).map((l) => l.replace(/^\/\/ ?/, '')).join(' ');

  const resolvePath = (path) => path.split('.').reduce((o, k) => (o === undefined || o === null ? undefined : o[k]), config);
  let unresolved = 0;
  const rows = notes.map((n) => {
    const value = resolvePath(n.path);
    if (value === undefined) unresolved++;
    return { ...n, value };
  });

  const esc = (s) => String(s).replace(/\|/g, '\\|');
  const show = (v) => {
    if (v === undefined) return '(unresolved)';
    if (v && typeof v === 'object') {
      const s = JSON.stringify(v);
      return s.length <= 60 ? `\`${esc(s)}\`` : Array.isArray(v) ? `array[${v.length}]` : '{…}';
    }
    return `\`${esc(JSON.stringify(v))}\``;
  };
  const out = [
    '# config.notes.md',
    '',
    `Generated from \`${SOURCE}\` by \`port-export/scripts/export-config.mjs\`. Do not edit by hand.`,
    '',
    `The file's own header states the unit system: "${header}"`,
    '',
    'Each row is a comment the source code puts next to a value, copied verbatim, with the dotted path of the key it',
    'sits on (resolve it in `config.json`). A comment on its own line is listed under the first key that follows it,',
    'marked "(above)". Several keys on one line share that line\'s comment. Keys with no comment in the source are not',
    'listed. For those, only `config.json` and the header above apply. `(S)` in a comment is the source\'s marker for',
    '"our own starting guess, tune in playtesting".',
    '',
    `Rows: ${rows.length}. Paths that did not resolve in config.json: ${unresolved}.`,
    '',
    '| Path | Value | Line | Comment (verbatim) |',
    '|---|---|---|---|',
    ...rows.map((r) => `| \`${r.path}\` | ${show(r.value)} | ${r.line} | ${r.above ? '(above) ' : ''}${esc(r.comment)} |`),
    '',
  ].join('\n');
  const notesFile = writeText('config.notes.md', out);
  return { json, notesFile, rows: rows.length, unresolved, roundTrip };
}

/**
 * Walk the object literal line by line (outside string literals): `key:` names, `{` / `[` push, `}` / `]` pop,
 * commas count array elements. Every `// comment` is attached to the keys (or array elements) on its line; a
 * comment-only line is held for the next key.
 */
function parseComments(lines) {
  const stack = []; // {type: 'obj' | 'arr', key, index}
  const notes = [];
  let pending = []; // comment-only lines waiting for the next key
  let started = false;
  const pathOf = (key) => {
    const parts = [];
    for (const f of stack) {
      if (f.key !== null) parts.push(f.key);
    }
    if (key !== null) parts.push(key);
    return parts.join('.');
  };
  lines.forEach((raw, i) => {
    const lineNo = i + 1;
    if (!started) {
      if (/export const config = \{/.test(raw)) {
        started = true;
        stack.push({ type: 'obj', key: null, index: 0 });
      }
      return;
    }
    if (stack.length === 0) return;
    const { code, comment } = splitComment(raw);
    const keysHere = [];
    const elementsHere = [];
    let lastKey = null;
    // Tokens outside strings.
    let j = 0;
    let inStr = null;
    while (j < code.length) {
      const ch = code[j];
      if (inStr) {
        if (ch === '\\') j++;
        else if (ch === inStr) inStr = null;
        j++;
        continue;
      }
      if (ch === "'" || ch === '"' || ch === '`') {
        inStr = ch;
        j++;
        continue;
      }
      const m = /^([A-Za-z_$][\w$]*)\s*:/.exec(code.slice(j));
      if (m && (j === 0 || /[\s{,]/.test(code[j - 1]))) {
        lastKey = m[1];
        const top = stack[stack.length - 1];
        if (top.type === 'obj') keysHere.push({ path: pathOf(lastKey) });
        j += m[0].length;
        continue;
      }
      const top = stack[stack.length - 1];
      if (ch === '{' || ch === '[') {
        let key = null;
        if (top.type === 'obj') key = lastKey;
        else {
          key = String(top.index);
          elementsHere.push({ path: pathOf(key) });
        }
        stack.push({ type: ch === '{' ? 'obj' : 'arr', key, index: 0 });
        lastKey = null;
      } else if (ch === '}' || ch === ']') {
        stack.pop();
        if (stack.length === 0) return;
      } else if (ch === ',') {
        if (top.type === 'arr') top.index++;
        lastKey = null;
      } else if (top.type === 'arr' && /[-\d'"]/.test(ch) && elementsHere.length === 0) {
        elementsHere.push({ path: pathOf(String(top.index)) });
      }
      j++;
    }
    const targets = keysHere.length > 0 ? keysHere : elementsHere;
    if (comment !== null) {
      if (code.trim() === '') pending.push({ comment, line: lineNo });
      else if (targets.length > 0) for (const t of targets) notes.push({ path: t.path, line: lineNo, comment, above: false });
      else if (stack.length > 0 && code.trim().length > 0) {
        // A continuation line (e.g. the tail of a one-line object): attach to the innermost open container.
        notes.push({ path: pathOf(null), line: lineNo, comment, above: false });
      }
    }
    if (targets.length > 0 && pending.length > 0) {
      for (const p of pending) notes.push({ path: targets[0].path, line: p.line, comment: p.comment, above: true });
      pending = [];
    }
  });
  return notes.sort((a, b) => a.line - b.line);
}

/** Split a line into code and its `//` comment (null when none), ignoring `//` inside string literals. */
function splitComment(line) {
  let inStr = null;
  for (let i = 0; i < line.length; i++) {
    const ch = line[i];
    if (inStr) {
      if (ch === '\\') i++;
      else if (ch === inStr) inStr = null;
      continue;
    }
    if (ch === "'" || ch === '"' || ch === '`') inStr = ch;
    else if (ch === '/' && line[i + 1] === '/') return { code: line.slice(0, i), comment: line.slice(i + 2).trim() };
  }
  return { code: line, comment: null };
}

if (import.meta.url === `file:///${process.argv[1].replace(/\\/g, '/')}` || process.argv[1]?.endsWith('export-config.mjs')) {
  const r = exportConfig();
  console.log(`config.json ${r.json.bytes} B (JSON round trip exact: ${r.roundTrip}); config.notes.md ${r.rows} rows, ${r.unresolved} unresolved`);
}
