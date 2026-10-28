// FORGE66 (2026-10-23, the lead; the red team's finding 1, Research/2026-10-23-forge66/redteam.md): writes NIGHT66's round tools as copies of
// NIGHT65's (n65-round.sh, n65-round-verbs.sh, n65-red.sh, n65-table.cjs, n65-gate-pack.sh / .cjs), so no NIGHT66 command reads or
// overwrites NIGHT65's tracked evidence (Logs/n65/r1 .. r5, the gate packs). Changes beyond the rename:
//   - every default folder is Logs/n66; a round refuses an existing Logs/n66/<tag> (and the verb step an existing verbs.md)
//   - the rooms come from slots65.json (b2 joins when T2 adds its slot); the skip runs 10 seeds (M1, never a literal 5)
//   - the reds are Logs/n66/red (`n66-round.sh red --shipped`: the shipped slots, n52-foundry's rooms) and Logs/n66/red65 (`n66-round.sh
//     red65` before the first pick: NIGHT65's final rooms); the GATE pack reads both, and the room's verb plan in place of NIGHT65's card
//   - a round saves its level and plan (Logs/n66/<tag>/n65-foundry.json, foundry.plan.json: finding 11), which the play build and the GATE
//     pack read; n66-red.sh adds flow-V.txt (n46flow --verbs65: the stall guard on the verb-aware pilot, finding 3) and the ride pilot in b2
// Each planned edit must apply exactly once, or the tool stops without writing that file. Refuses to overwrite an existing n66 file.
//   node Tools/night/forge66-make-n66-rounds.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const SLOT_ROOMS = `$(node -p "Object.keys(require('./Design/levels/n65/slots65.json').slots).join(' ')")`;

const EDITS = {
  'n65-round.sh': [
    ["# NIGHT65 (the lead's; branch C, DECIDE65 D-2: no Player): one sim round of every rebuilt room on the main tree, after the lead's pick of the\n# builders' room files. In order, each output under Logs/n65/<tag>/ with its command at its top (L-149):",
      "# NIGHT66 (the lead's; FORGE66's copy of n65-round.sh, its red team's findings 1 and 11): one sim round of every slot room on the main tree,\n# after the lead's pick of the builders' room files. In order, each output under Logs/n66/<tag>/ (refused when that folder exists) with its\n# command at its top (L-149):"],
    ["(every slot's newest room file: the round's level)",
      "(every slot's newest room file: the round's level;\n#      with --shipped, slots65.json's set: the red); then n65-foundry.json / foundry.plan.json, copies of that level and plan (the play build\n#      and the GATE pack read them; never `git add -f` them)"],
    ["#      PNG, a3's ride pilot) on that level", "#      PNG, the verb-aware flow V, the ride pilot in a3 and b2) on that level"],
    ["#   4. table.md: `node Tools/night/n65-table.cjs Logs/n65/<tag>` (each number against the red)",
      "#   4. table.md and table-red65.md: `node Tools/night/n66-table.cjs Logs/n66/<tag> [Logs/n66/red65]` (each number against each red)"],
    ["# Run it alone in this tree (two dotnet runs never share a tree).   bash Tools/night/n65-round.sh <tag>      e.g. r1",
      "# Run it alone in this tree (two dotnet runs never share a tree; no build, gate pack or `git add Design/levels` while it runs).\n#   bash Tools/night/n66-round.sh <tag> [--shipped]      e.g. r1; the reds: `red --shipped`, then `red65` (both before the first pick)"],
    ['tag="${1:?tag}"; out="Logs/n65/$tag"; mkdir -p "$out"; T0=$SECONDS',
      'tag="${1:?tag}"; mode="${2:-}"; out="Logs/n66/$tag"; T0=$SECONDS\n[ -e "$out" ] && { echo "n66-round $tag: $out exists: a round\'s evidence is never overwritten (FORGE66 red team 1); use a new tag"; exit 1; }\nmkdir -p "$out"\nROOMS="' + SLOT_ROOMS + '"; export ROOMS   # the slot rooms (b2 too once T2 adds its slot)'],
    ['gen "$out/gen-latest.txt" --run --slots=latest',
      'if [ "$mode" = "--shipped" ]; then gen "$out/gen-latest.txt" --run; else gen "$out/gen-latest.txt" --run --slots=latest; fi'],
    ['echo "n65-round $tag: latest assembled',
      'cp Design/levels/n65-foundry.json "$out/n65-foundry.json"; cp Design/levels/n65/foundry.plan.json "$out/foundry.plan.json"   # the round\'s level and plan (red team 11)\necho "n65-round $tag: ${mode:-latest} assembled'],
    ['grep -m1 "^n65-red:" "$out/red-run.txt"', 'grep -m1 "^n66-red:" "$out/red-run.txt"'],
    ['  for r in a2 a3 a5 b1 a8 a4; do node Tools/night/n65-iso.cjs', '  for r in $ROOMS; do node Tools/night/n65-iso.cjs'],
    ['2>/dev/null | wc -l) / 6"', '2>/dev/null | wc -l) / $(echo $ROOMS | wc -w)"'],
    ['[ -f Tools/night/n65-verbs.sh ] && bash Tools/night/n65-round-verbs.sh "$tag"', 'bash Tools/night/n66-round-verbs.sh "$tag"'],
    ['node Tools/night/n65-table.cjs "$out" > "$out/table.md"',
      'node Tools/night/n66-table.cjs "$out" > "$out/table.md"\n[ -d Logs/n66/red65 ] && node Tools/night/n66-table.cjs "$out" Logs/n66/red65 > "$out/table-red65.md"'],
  ],
  'n65-round-verbs.sh': [
    ["# NIGHT65 (the lead's; branch C): each rebuilt room's own verb pilot and the skip test (builder T's n65verb, Tools/night/n65-verbs.sh) on the\n# level in the tree, into Logs/n65/<tag>/:",
      "# NIGHT66 (the lead's; FORGE66's copy of n65-round-verbs.sh, its red team's finding 1: Logs/n66, 10 seeds, the slot rooms): each room's own\n# verb pilot and the skip test (n65verb, Tools/night/n65-verbs.sh; T1b's skip v2 takes over the skip line) on the level in the tree, into\n# Logs/n66/<tag>/:"],
    ["verbs-skip.txt (all six)", "verbs-skip.txt (every slot room)"],
    ["a3's ride pilot is n65-red.sh's ride.txt. The red's\n# twins are Research/2026-10-23-wave65/T/verb-*-red.txt (n52-foundry). Called by n65-round.sh",
      "a3's and b2's ride pilot is n66-red.sh's ride.txt. The reds'\n# twins are Logs/n66/red/verbs-*.txt and Logs/n66/red65/verbs-*.txt. Called by n66-round.sh"],
    ['tag="${1:?tag}"; out="Logs/n65/$tag"; mkdir -p "$out"; T0=$SECONDS',
      'tag="${1:?tag}"; out="Logs/n66/$tag"; mkdir -p "$out"; T0=$SECONDS\n[ -e "$out/verbs.md" ] && { echo "n66-round-verbs $tag: $out/verbs.md exists: never overwritten (FORGE66 red team 1)"; exit 1; }\nROOMS="${ROOMS:-' + SLOT_ROOMS + '}"'],
    ['run "$out/verbs-skip.txt" skip a2,a3,a5,b1,a8,a4 --verbs65 --seeds 5',
      'run "$out/verbs-skip.txt" skip "$(echo $ROOMS | tr \' \' \',\')" --verbs65 --seeds 10   # FORGE66 red team 1: 10 seeds (M1)\n#'],
  ],
  'n65-red.sh': [
    ["# NIGHT65 (the lead's; NIGHT65.md section 1 step 6): the sim red of the six rebuilt slots on n52-foundry, no Player. Per room under\n# Logs/n65/red/<id>/:",
      "# NIGHT66 (the lead's; FORGE66's copy of n65-red.sh, its red team's findings 1 and 3): the sim checks of every slot room (slots65.json, or\n# $ROOMS) on one level, no Player; n66-round.sh runs it for each round and for the reds. Per room under <OUTDIR>/<id>/:"],
    ["pads.txt (hold52 --pads), plan.png (n52-svg-png.sh); a3 also ride.txt (n53ridekill ride).",
      "pads.txt (hold52 --pads), plan.png (n52-svg-png.sh), flow-V.txt (n46flow --verbs65 seeds 1-10: the stall guard on the\n# verb-aware pilot); a3 and b2 also ride.txt (n53ridekill ride)."],
    ["#   bash Tools/night/n65-red.sh [JOBS] [LEVEL] [OUTDIR]       defaults: 6, n52-foundry, Logs/n65/red",
      "#   bash Tools/night/n66-red.sh [JOBS] [LEVEL] [OUTDIR]       defaults: 6, n65-foundry, Logs/n66/red"],
    ['JOBS="${1:-6}"; LEVEL="${2:-n52-foundry}"; OUTDIR="${3:-Logs/n65/red}"', 'JOBS="${1:-6}"; LEVEL="${2:-n65-foundry}"; OUTDIR="${3:-Logs/n66/red}"'],
    ['ROOMS="a2 a3 a5 b1 a8 a4"', 'ROOMS="${ROOMS:-' + SLOT_ROOMS + '}"'],
    ["  printf '%s\\t%s\\n' \"$d/pads.txt\" \"hold52 --level $LEVEL --room $r --pads\" >> \"$JOBFILE\"",
      "  printf '%s\\t%s\\n' \"$d/pads.txt\" \"hold52 --level $LEVEL --room $r --pads\" >> \"$JOBFILE\"\n  printf '%s\\t%s\\n' \"$d/flow-V.txt\" \"n46flow --level $LEVEL --rooms $r --seeds 10 --first 1 --verbs65\" >> \"$JOBFILE\"   # FORGE66 red team 3: stall on the verb-aware pilot"],
    ["printf '%s\\t%s\\n' \"$OUTDIR/a3/ride.txt\" \"n53ridekill ride --level Design/levels/$LEVEL.json --room a3\" >> \"$JOBFILE\"",
      "for r in a3 b2; do case \" $ROOMS \" in *\" $r \"*) printf '%s\\t%s\\n' \"$OUTDIR/$r/ride.txt\" \"n53ridekill ride --level Design/levels/$LEVEL.json --room $r\" >> \"$JOBFILE\" ;; esac; done"],
  ],
  'n65-table.cjs': [
    ["// NIGHT65 (the lead's): the per-room table", "// NIGHT66 (the lead's; FORGE66's copy of n65-table.cjs, its red team's findings 1 and 3: the reds under Logs/n66, the slot rooms from\n// slots65.json, the verb-aware flow V's stall column, the ride pilot in b2 too): the per-room table"],
    ['const red = process.argv[3] || "Logs/n65/red";', 'const red = process.argv[3] || "Logs/n66/red";'],
    ['const ROOMS = ["a2", "a3", "a5", "b1", "a8", "a4"];', 'const ROOMS = Object.keys(JSON.parse(fs.readFileSync(path.join(__dirname, "../../Design/levels/n65/slots65.json"), "utf8")).slots);'],
    ['| stall % A / B (red) |', '| stall % A / B (red) | stall % V (red) |'],
    ['console.log("|---|---|---|---|---|---|---|---|---|---|");', 'console.log("|---|---|---|---|---|---|---|---|---|---|---|");'],
    ['rA = flow(red, id, "A"), rB = flow(red, id, "B");', 'rA = flow(red, id, "A"), rB = flow(red, id, "B"), V = flow(dir, id, "V"), rV = flow(red, id, "V");'],
    ['" | not run |  |  |  |  |  |  |  |  |");', '" | not run |  |  |  |  |  |  |  |  |  |");'],
    ['" | " + ab("stall") + " | "', '" | " + ab("stall") + " | " + (V ? V.stall : "?") + " (" + (rV ? rV.stall : "?") + ") | "'],
    ['(id === "a3" ? both(ride(dir, id), ride(red, id)) : "")', '(id === "a3" || id === "b2" ? both(ride(dir, id), ride(red, id)) : "")'],
    ['(a clear short of 10 / 10 is a GUARD failure) |  |  |  |  |  |  |  |  |");', '(a clear short of 10 / 10 is a GUARD failure) |  |  |  |  |  |  |  |  |  |");'],
  ],
  'n65-gate-pack.sh': [
    ["# NIGHT65 builder T: the GATE evidence pack", "# NIGHT66 (FORGE66's copy of NIGHT65 builder T's n65-gate-pack.sh): the GATE evidence pack"],
  ],
  'n65-gate-pack.cjs': [
    ['// NIGHT65 builder T (the GATE evidence pack; NIGHT65.md section 4 step 4, rule 9 "reviewers see evidence, not the story"): one room\'s folder',
      '// NIGHT66 (FORGE66\'s copy of NIGHT65 builder T\'s n65-gate-pack.cjs; its red team\'s findings 1 and 11: Logs/n66, the reds Logs/n66/red and\n// Logs/n66/red65, the room\'s verb plan in place of its card, the skip at 10 seeds, the round\'s saved level and plan when present, the slot\n// rooms from slots65.json). The GATE evidence pack (NIGHT65.md section 4 step 4, rule 9 "reviewers see evidence, not the story"): one room\'s folder'],
    ['(Research/2026-10-23-wave65/T/verb-*-red.txt, verb-skip-verbs65-red.txt)', '(Logs/n66/red/verbs-*.txt, Logs/n66/red65/verbs-*.txt)'],
    ["//   card.md          the room's card at the sha (Research/2026-10-23-wave65/cards/<id>.md)", "//   plan.md          the room's verb plan at the sha (Research/2026-10-23-wave66/plans/<id>.md)"],
    ["//   checks.txt       the round's room-check, flow A / B (the room's rows, SUMMARY, STYLE), pads and ride against the red's",
      "//   checks.txt       the round's room-check, flow A / B / V (the room's rows, SUMMARY, STYLE), pads and ride against both reds'"],
    ['const LOGS = abs(opt("--logs", "Logs/n65")), LEVEL = opt("--level", "Design/levels/n65-foundry.json");',
      'const LOGS = abs(opt("--logs", "Logs/n66")); let LEVEL = opt("--level", "Design/levels/n65-foundry.json");'],
    ['const LEVEL_GIVEN = args.includes("--level");',
      '// FORGE66 red team 11: the round\'s own saved level and plan (n66-round.sh copies them) unless --level / --plan name others\nconst SAVED_LEVEL = path.join(RD, "n65-foundry.json"), SAVED_PLAN = path.join(RD, "foundry.plan.json");\nlet LEVEL_GIVEN = args.includes("--level");\nif (!LEVEL_GIVEN && fs.existsSync(SAVED_LEVEL)) { LEVEL = SAVED_LEVEL; LEVEL_GIVEN = true; }\nif (!PLAN && fs.existsSync(SAVED_PLAN)) { PLAN = SAVED_PLAN; planHow = "the round\'s saved plan (" + path.relative(ROOT, SAVED_PLAN).replace(/\\\\/g, "/") + ")"; }'],
    ['for (const r of ["a2", "a3", "a4", "a5", "b1", "a8"]) files.push(', 'for (const r of Object.keys(JSON.parse(git(["show", sha + ":Design/levels/n65/slots65.json"])).slots)) files.push('],
    ['if (/^n65verb [a-z]+: (GREEN|RED)/.test(l))', 'if (/^n65verb [a-z]+: (GREEN|RED|UNSURE|M1 )/.test(l))'],
    ['    const TD = abs("Research/2026-10-23-wave65/T");\n    const redFiles = fs.existsSync(TD) ? fs.readdirSync(TD).filter((f) => /^verb-.*-red\\.txt$/.test(f)).sort() : [];\n    const redLines = [];\n    for (const f of redFiles) { const p = pick(read(path.join(TD, f))); if (p.length) redLines.push(`from Research/2026-10-23-wave65/T/${f}:`, ...p.map((l) => "    " + l)); }',
      '    // FORGE66: both reds\' own verb files (n66-round.sh red --shipped / red65)\n    const redLines = [];\n    for (const rn of ["red", "red65"]) {\n      const TD = path.join(LOGS, rn);\n      for (const f of (fs.existsSync(TD) ? fs.readdirSync(TD).filter((f) => /^verbs.*\\.txt$/.test(f)).sort() : [])) { const p = pick(read(path.join(TD, f))); if (p.length) redLines.push(`from ${path.relative(ROOT, TD).replace(/\\\\/g, "/")}/${f}:`, ...p.map((l) => "    " + l)); }\n    }'],
    ['"--seeds", "5", "--verbs65"]', '"--seeds", "10", "--verbs65"]'],
    ['\\n\\nThe red (n52-foundry):\\n\\n', '\\n\\nThe reds (Logs/n66/red: the shipped slots, n52-foundry\'s rooms; Logs/n66/red65: NIGHT65\'s final rooms):\\n\\n'],
    ['and Research/2026-10-23-wave65/T/verb-*-red.txt`);', 'and Logs/n66/red / red65 verbs-*.txt`);'],
    ['# NIGHT65 GATE spot key for', '# NIGHT66 GATE spot key for'],
    ['["the red (n52-foundry)", path.join(LOGS, "red", id)]]', '["the red (the shipped slots: n52-foundry\'s rooms)", path.join(LOGS, "red", id)], ["the red65 (NIGHT65\'s final rooms)", path.join(LOGS, "red65", id)]]'],
    ['["room-check.txt", "flow-A.txt", "flow-B.txt", "pads.txt", "ride.txt"]', '["room-check.txt", "flow-A.txt", "flow-B.txt", "flow-V.txt", "pads.txt", "ride.txt"]'],
    ['${round}/${id}/ and red/${id}/: room-check.txt, flow-A / flow-B.txt', '${round}/${id}/, red/${id}/ and red65/${id}/: room-check.txt, flow-A / flow-B / flow-V.txt'],
  ],
};
// then everywhere: the own tools' names and the logs folder (the shared tools keep their n65 names: gen-n65-foundry, n65-iso, n65-verbs, ...)
const GLOBAL = [[/Logs\/n65/g, 'Logs/n66'], [/n65-round-verbs/g, 'n66-round-verbs'], [/\bn65-round\b(?!-)/g, 'n66-round'], [/\bn65-red\b/g, 'n66-red'],
  [/\bn65-table\b/g, 'n66-table'], [/\bn65-gate-pack\b/g, 'n66-gate-pack'], [/--seeds 5 --verbs65/g, '--seeds 10 --verbs65'],
  [/Research\/2026-10-23-wave65\/cards\//g, 'Research/2026-10-23-wave66/plans/'], [/"card\.md"/g, '"plan.md"']];

let bad = 0;
for (const [f, edits] of Object.entries(EDITS)) {
  const dst = path.join(DIR, f.replace('n65-', 'n66-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); bad++; continue; }
  let s = fs.readFileSync(path.join(DIR, f), 'utf8').replace(/\r\n/g, '\n');   // n65-gate-pack.cjs is CRLF; the copies are LF
  const missed = [];
  for (const [from, to] of edits) {
    const n = s.split(from).length - 1;
    if (n !== 1) { missed.push('(' + n + 'x) ' + from.slice(0, 70)); continue; }
    s = s.replace(from, () => to);
  }
  if (missed.length) { console.log('NOT WRITTEN ' + f + ': edits that did not apply exactly once: ' + missed.join(' || ')); bad++; continue; }
  for (const [re, to] of GLOBAL) s = s.replace(re, to);
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').map((l, i) => [i + 1, l]).filter(([, l]) => /Logs\/n65|wave65|"5"|--seeds 5|n65-(round|red|table|gate-pack)/.test(l));
  console.log(path.relative(path.join(DIR, '..', '..'), dst).replace(/\\/g, '/') + ': ' + s.split('\n').length + ' lines, ' + edits.length + ' edits; lines still naming NIGHT65 folders or tools: ' + (left.length ? left.map(([n, l]) => n + ' ' + l.trim().slice(0, 100)).join(' || ') : 'none'));
}
process.exit(bad ? 1 : 0);
