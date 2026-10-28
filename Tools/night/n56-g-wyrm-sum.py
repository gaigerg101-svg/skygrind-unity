# NIGHT56 builder G (G4): the combined line of n56-g-wyrm.sh's four n47human chunks, from their per-seed lines
# ("wyrm seed N: killed T s, deaths D, taken X, windows ..., dealt in windows P %, ..." or "wyrm seed N: alive at the cap ..."): killed / seeds,
# kill time median / p90 / min / max (a fight alive at the cap counts as the cap, as n47human), deaths total, taken median, dealt in windows median.
import re
import sys

text = open(sys.argv[1], encoding='utf8', errors='replace').read()
rows = []
for m in re.finditer(r'^\s*wyrm seed (\d+): (.*)$', text, re.M):
    seed, rest = int(m.group(1)), m.group(2)
    km = re.search(r'killed ([0-9.]+) s', rest)
    t = float(km.group(1)) if km else None
    dm = re.search(r'deaths (\d+)', rest)
    tm = re.search(r'taken (\d+)', rest)
    wm = re.search(r'dealt in windows (\d+) %', rest)
    rows.append((seed, t, int(dm.group(1)) if dm else 0, int(tm.group(1)) if tm else 0, int(wm.group(1)) if wm else 0))
cap = 300.0
cm = re.search(r'cap (\d+) s', text)
if cm:
    cap = float(cm.group(1))
if not rows:
    print('n56-g-wyrm: no per-seed lines')
    sys.exit(1)
times = sorted(t if t is not None else cap for (_, t, _, _, _) in rows)
n = len(times)
med = times[n // 2]
p90 = times[int(0.9 * (n - 1))]  # n47human's p90 (index 35 of 40)
killed = sum(1 for r in rows if r[1] is not None)
deaths = sum(r[2] for r in rows)
taken = sorted(r[3] for r in rows)[n // 2]
win = sorted(r[4] for r in rows)[n // 2]
band = killed == n and 60 <= med <= 120 and p90 <= 180 and deaths <= 10
print(f'n56-g-wyrm COMBINED: seeds {n}, killed {killed}/{n}, kill time median {med:.1f} s, p90 {p90:.1f} s, min {times[0]:.1f} s, max {times[-1]:.1f} s, '
      f'deaths {deaths}, taken median {taken}, dealt in windows median {win} %; G4 doneWhen (40/40, median 60-120, p90 <= 180, deaths <= 10): {"MET" if band else "NOT MET"}')
worst = sorted(rows, key=lambda r: -(r[1] if r[1] is not None else cap))[:8]
print('  the tail: ' + ', '.join(f's{r[0]} {("%.1f s" % r[1]) if r[1] is not None else "cap"} ({r[2]} deaths)' for r in worst))
