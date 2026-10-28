# NIGHT53 the building share (NIGHT53.md section 7: "log each block as BUILD or CHECK; the report prints the share (target >= 70 % BUILD)").
# Reads NIGHT53-LOG.md's timeline lines "- HH:MM-HH:MM BUILD." / "CHECK." / "BUILD + CHECK." (a single "- HH:MM TAG" line counts 1 minute);
# overlapping blocks are counted per minute once: a minute that has any BUILD block is BUILD, else CHECK. Prints minutes and the share.
#   python -I Tools/night/n53-share.py [NIGHT53-LOG.md]
import re
import sys

path = sys.argv[1] if len(sys.argv) > 1 else "NIGHT53-LOG.md"
pat = re.compile(r"^- (\d\d):(\d\d)(?:-(\d\d):(\d\d))? (BUILD \+ CHECK|CHECK \+ BUILD|BUILD|CHECK)\b")
minutes = {}


def m(h, mi):
    t = int(h) * 60 + int(mi)
    return t + 24 * 60 if t < 12 * 60 else t  # the night crosses midnight: hours before noon are the next day


for line in open(path, encoding="utf-8"):
    g = pat.match(line)
    if not g:
        continue
    a = m(g.group(1), g.group(2))
    b = m(g.group(3), g.group(4)) if g.group(3) else a + 1
    tag = g.group(5)
    build = "BUILD" in tag
    for t in range(a, max(b, a + 1)):
        minutes[t] = minutes.get(t, False) or build
total = len(minutes)
built = sum(1 for v in minutes.values() if v)
print("logged minutes %d: BUILD %d, CHECK only %d -> building share %.0f %% (target >= 75 %%, NIGHT53 forge)" % (total, built, total - built, 100.0 * built / max(total, 1)))
