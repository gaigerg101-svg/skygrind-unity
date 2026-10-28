# NIGHT49 blind pair (the lead's): puts two frames side by side as A and B in a random order, for three fresh judges.
#   python -I Tools/night/n49-pair.py <label> <old.png> <new.png>
# Writes Logs/n49/judge/<label>/A.png and B.png (the only files a judge is shown), and the key at Logs/n49/judge-keys/<label>.json
# (the lead reads it after the votes; a judge is never pointed at it). Prints the two judge paths and nothing else.
import json
import os
import random
import shutil
import sys

if len(sys.argv) != 4:
    sys.exit("usage: python -I Tools/night/n49-pair.py <label> <old.png> <new.png>")
label, old, new = sys.argv[1], sys.argv[2], sys.argv[3]
for p in (old, new):
    if not os.path.isfile(p):
        sys.exit("missing frame: " + p)
root = os.path.join("Logs", "n49")
judge_dir = os.path.join(root, "judge", label)
key_dir = os.path.join(root, "judge-keys")
os.makedirs(judge_dir, exist_ok=True)
os.makedirs(key_dir, exist_ok=True)
rng = random.Random(os.urandom(16))
flip = rng.random() < 0.5
a_src, b_src = (new, old) if flip else (old, new)
shutil.copyfile(a_src, os.path.join(judge_dir, "A.png"))
shutil.copyfile(b_src, os.path.join(judge_dir, "B.png"))
with open(os.path.join(key_dir, label + ".json"), "w", encoding="utf8") as f:
    json.dump({"A": "new" if flip else "old", "B": "old" if flip else "new", "old": old, "new": new}, f, indent=1)
print(os.path.join(judge_dir, "A.png"))
print(os.path.join(judge_dir, "B.png"))
