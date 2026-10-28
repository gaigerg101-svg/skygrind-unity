#!/usr/bin/env bash
# NIGHT67 builder T, T1c round 2's planted cases (Logs/n67/verdict-T1c-r2.md A1, A2, A5, A6, A11), node only (--no-skip: no dotnet), on the
# recording dry round Logs/n67/dry-rec-iceline (T's tree; g8's room file re-planted with verb65 ride = the bytes the round ran, restored after):
#   P1 no level review anywhere -> MISSING 'level review', exit 3;  P2 a review of ANOTHER round naming g8 -> still MISSING;
#   P3 a review in the round's own folder with '## g8' and a BLOCKING line -> no 'level review' in MISSING, the BLOCKING line in open-findings;
#   P4 the round copied as r9-iceline, its review at r9/review-level.md -> counted; its table.md removed -> MISSING 'table'.
# Each prints the README's first line, the exit, the verb65 line (the slate it read) and open-findings' last line. Output: t1c-r2-proof.txt
cd "$(git rev-parse --show-toplevel)" || exit 1
E=Research/2026-10-24-wave67/T; f=$E/t1c-r2-proof.txt; rf=Design/levels/n67/iceline/rooms/g8.json; R=dry-rec-iceline; L=Logs/n67
trap 'git checkout -- "$rf" 2>/dev/null; rm -rf "$L/r8-other" "$L/$R/review-level.md" "$L/r9-iceline" "$L/r9"' EXIT
node $E/t1b-plant-verb65.cjs "$rf" ride > /dev/null
pk() { local tag="$1" out="$2"; shift 2; rm -rf "$L/gate/$out"; L67=iceline node Tools/night/n67-gate-pack.cjs g8 "$tag" --no-skip --out "$L/gate/$out" --key "$L/gate/keys/$out.txt" "$@" > "$L/gate/$out.run.txt" 2>&1; local rc=$?
  echo "# exit $rc"; grep -E "verb65 g8|room file" "$L/gate/$out.run.txt" | cut -c1-200; echo "# README: $(head -1 "$L/gate/$out/README.md" | cut -c1-400)"; echo "# open-findings: $(tail -2 "$L/gate/$out/open-findings.md" | tr '\n' ' ' | cut -c1-300)"; }
{
  echo "# sha $(git rev-parse --short HEAD) (+ uncommitted: $(git status --short -uno Tools | tr '\n' ' ')); $(date '+%F %T')"
  echo; echo "## P1: no level review anywhere"; pk "$R" g8-p1
  mkdir -p "$L/r8-other"; printf '# r8-other level review (PLANTED: another round)\n\n## g8\n\nBLOCKING: a finding of another round (must not count)\n' > "$L/r8-other/review-level.md"
  echo; echo "## P2: a review of ANOTHER round (Logs/n67/r8-other/review-level.md) names g8"; pk "$R" g8-p2
  printf '# %s level review (PLANTED)\n\n## g8\n\nBLOCKING: the planted finding of this round (must show in open-findings)\n\n## g7\n\nfine\n' "$R" > "$L/$R/review-level.md"
  echo; echo "## P3: the round's own review ($R/review-level.md) with '## g8' and a BLOCKING line"; pk "$R" g8-p3
  rm -f "$L/$R/review-level.md"; cp -r "$L/$R" "$L/r9-iceline"; rm -f "$L/r9-iceline/table.md"; mkdir -p "$L/r9"
  printf '# r9 level review (PLANTED: NIGHT67.md section 4 step 2 form)\n\n## g8\n\nno BLOCKING line\n' > "$L/r9/review-level.md"
  echo; echo "## P4: the round as r9-iceline, its review at Logs/n67/r9/review-level.md; its table.md removed"; pk r9-iceline g8-p4
  echo "# done $(date +%T)"
} > "$f" 2>&1
cut -c1-420 "$f"
