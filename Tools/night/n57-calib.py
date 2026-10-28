# NIGHT57 P0-calib (the lead's; NIGHT57.md section 5 P0): set up a BLIND judge-calibration folder OUTSIDE the repo.
# A critic spawned in the repo reads CLAUDE.md -> CORE.md and the auto-memory, which quote GG's verdicts; so the frames are copied, renamed,
# into <out> with only a rubric, and the critic runs there with `claude -p` (no access to the repo). The key stays in the repo
# (Logs/n57/calib-key.json). Part A: two sets of six level-walk frames, n35 (GG w35: "the gameplay just feels awful") vs n37 (GG w37:
# "Gameplay feels a little bit better"), shown as SET 1 / SET 2 in one order, then (a second folder) the other. Part B: n41-r7's level 1
# frames (GG: the wall ride section "awful and relaly difficult") and n42-r4's UI frames (GG: the UI "too generic and cheap"), each set
# asked for its biggest complaint before any of GG's words are seen. The limits (printed in the rubric's log, not to the critic): the
# frames are stills, not the same rooms, and GG judged by playing.
#   python -I Tools/night/n57-calib.py <out dir>
import json, os, random, shutil, sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FEEL = os.path.join(ROOT, "Logs", "feel")
out = os.path.abspath(sys.argv[1])
if out.startswith(ROOT):
    sys.exit("the calibration folder must be OUTSIDE the repo")

N35 = ["n35-r2/levels6/levels35/shot-N29-N35-levels35-x2-a.png", "n35-r2/levels6/levels35/shot-N29-N35-levels35-x2-c.png",
       "n35-r2/levels6/levels35/shot-N29-N35-levels35-x3-a.png", "n35-r2/levels6/levels35/shot-N29-N35-levels35-x3-c.png",
       "n35-r2/levels6/levels35/shot-N29-N35-levels35-x4-a.png", "n35-r2/levels6/levels35/shot-N29-N35-levels35-x4-c.png"]
N37 = ["n37-r1/arena2/level36/shot-N29-N36-level36-s1-a.png", "n37-r1/arena2/level36/shot-N29-N36-level36-s2-a.png",
       "n37-r1/arena2/level36/shot-N29-N36-level36-s3-a.png", "n37-r1/arena2/level36/shot-N29-N36-level36-o1-a.png",
       "n37-r1/arena2/level36/shot-N29-N36-level36-o2-a.png", "n37-r1/arena2/level36/shot-N29-N36-level36-i3-a.png"]
N41 = ["n41-r7/b-lv1/level36/shot-N29-N36-level36-a4-a.png", "n41-r7/b-lv1/level36/shot-N29-N36-level36-a5-a.png",
       "n41-r7/b-lv1/level36/shot-N29-N36-level36-a6-a.png", "n41-r7/b-lv1/level36/shot-N29-N36-level36-a7-a.png"]
N42 = ["n42-r4/b43ui/ui42/shot-N30-U42-h-hit.png", "n42-r4/b43ui/ui42/shot-N30-U42-k-full.png",
       "n42-r4/b43ui/ui42/shot-N30-U42-s-combo.png", "n42-r4/b43ui/ui42/shot-N30-U42-t-pump.png"]

def find(rel):
    p = os.path.join(FEEL, rel)
    if os.path.exists(p):
        return p
    # n41-r7's frames sit one folder deeper than the name says on some runs: search by basename under the build folder
    base, name = rel.split("/")[0], os.path.basename(rel)
    for d, _, fs in os.walk(os.path.join(FEEL, base)):
        if name in fs:
            return os.path.join(d, name)
    sys.exit("missing frame: " + rel)

RUBRIC_A = """You are judging two sets of screenshots from two builds of a first-person movement shooter (fast movement, wall riding,
small combat arenas, at dusk). You have not played it. Look at every image in this folder (SET1_*.png and SET2_*.png; use the Read tool).
Question: a demanding player who loves ULTRAKILL and DOOM plays both builds. Which build would they call the better game, judging what
the frames show of the arenas, the enemies, the readability and how much the space invites movement? Answer in this exact form:
BETTER: SET1 or SET2 or TIE
WHY: <two sentences>
Do not guess from file names or metadata; judge the images."""
RUBRIC_B = """You are reviewing two sets of screenshots from a first-person movement shooter (fast movement, wall riding, small combat
arenas, at dusk). You have not played it. Look at every image in this folder (P_*.png and Q_*.png; use the Read tool).
For EACH set, name the single biggest thing its owner would complain about after playing it, as the owner would say it in one blunt
sentence. Answer in this exact form:
P: <one sentence>
Q: <one sentence>
Do not guess from file names or metadata; judge the images."""

rng = random.Random(57)
key = {"why": "NIGHT57 P0-calib key (n57-calib.py); the critic folders hold only renamed frames + a rubric", "parts": {}}
os.makedirs(out, exist_ok=True)
for part, sets in (("a1", (N35, N37)), ("a2", (N37, N35))):
    d = os.path.join(out, part)
    os.makedirs(d, exist_ok=True)
    for label, frames in zip(("SET1", "SET2"), sets):
        order = frames[:]
        rng.shuffle(order)
        for i, rel in enumerate(order, 1):
            shutil.copyfile(find(rel), os.path.join(d, "%s_%d.png" % (label, i)))
    open(os.path.join(d, "RUBRIC.txt"), "w").write(RUBRIC_A)
    key["parts"][part] = {"SET1": "n35" if sets[0] is N35 else "n37", "SET2": "n37" if sets[0] is N35 else "n35", "truth": "n37 better (GG w35 'awful' -> w37 'a little bit better')"}
d = os.path.join(out, "b")
os.makedirs(d, exist_ok=True)
for label, frames in (("P", N41), ("Q", N42)):
    for i, rel in enumerate(frames, 1):
        shutil.copyfile(find(rel), os.path.join(d, "%s_%d.png" % (label, i)))
open(os.path.join(d, "RUBRIC.txt"), "w").write(RUBRIC_B)
key["parts"]["b"] = {"P": "n41-r7 level 1 (GG: the wall ride section 'awful and relaly difficult'; match words: wall, ride, platform, jump, gap)",
                     "Q": "n42-r4 UI (GG: 'too generic and cheap, almost like basic placeholder or default AI-generated UI'; match words: generic, cheap, placeholder, default, template, bland, plain)"}
key["limits"] = "stills, not the same rooms; GG judged by playing; n35 has 9 level frames only"
kp = os.path.join(ROOT, "Logs", "n57", "calib-key.json")
open(kp, "w").write(json.dumps(key, indent=2))
print("calib folders:", out, "(a1, a2, b); key:", kp)
