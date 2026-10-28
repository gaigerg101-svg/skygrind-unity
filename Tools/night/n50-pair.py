# NIGHT50 blind pair (the lead's). Each judge sees the pair TWICE, in both orders (A/B, then B/A), as two separate calls.
# A judge's verdict counts only if both orders pick the same frame; otherwise it is a TIE (position-bias fix; NIGHT50 PROMPT-RESEARCH.md B).
#   python -I Tools/night/n50-pair.py stage <label> <old.png> <new.png>   -> writes Logs/n50/judge/<label>/{o1,o2}/{A,B}.png, key (seeded, not shown)
#   python -I Tools/night/n50-pair.py tally <label> <vote1> <vote2> <vote3>  -> each vote: "A", "B" or "TIE" in the FIRST order; the second
#       order's answer is read from <label>/second.txt as the same form (judge1,judge2,judge3 in order). Prints the verdict per judge and the total.
import json
import os
import random
import shutil
import sys

ROOT = os.path.join("Logs", "n50")
KEYS = os.path.join(ROOT, "judge-keys")


def stage(label, old, new):
    for p in (old, new):
        if not os.path.isfile(p):
            sys.exit("missing frame: " + p)
    rng = random.Random(os.urandom(16))
    flip = rng.random() < 0.5
    first_new = flip
    base = os.path.join(ROOT, "judge", label)
    for order, (a_src, b_src) in (("o1", (new, old) if flip else (old, new)), ("o2", (old, new) if flip else (new, old))):
        d = os.path.join(base, order)
        os.makedirs(d, exist_ok=True)
        shutil.copyfile(a_src, os.path.join(d, "A.png"))
        shutil.copyfile(b_src, os.path.join(d, "B.png"))
    os.makedirs(KEYS, exist_ok=True)
    with open(os.path.join(KEYS, label + ".json"), "w", encoding="utf8") as f:
        # o1: A is new when flip; o2 swaps the two positions.
        json.dump({"o1": {"A": "new" if flip else "old", "B": "old" if flip else "new"},
                   "o2": {"A": "old" if flip else "new", "B": "new" if flip else "old"},
                   "old": old, "new": new}, f, indent=1)
    print(os.path.join(base, "o1", "A.png"))
    print(os.path.join(base, "o1", "B.png"))
    print(os.path.join(base, "o2", "A.png"))
    print(os.path.join(base, "o2", "B.png"))


def tally(label, first_votes, second_votes):
    # first_votes / second_votes: lists of "A" or "B" per judge, as the judge saw them in o1 and o2.
    with open(os.path.join(KEYS, label + ".json"), encoding="utf8") as f:
        key = json.load(f)
    verdicts = []
    for v1, v2 in zip(first_votes, second_votes):
        if v1 == "TIE" or v2 == "TIE":
            verdicts.append("TIE")
            continue
        n1 = key["o1"][v1]
        n2 = key["o2"][v2]
        verdicts.append(n1 if n1 == n2 else "TIE")
    new_wins = verdicts.count("new")
    old_wins = verdicts.count("old")
    result = "new" if new_wins > old_wins and new_wins >= 2 else ("old" if old_wins > new_wins and old_wins >= 2 else "TIE")
    print(json.dumps({"verdicts": verdicts, "result": result}))
    return result


if __name__ == "__main__":
    if len(sys.argv) >= 5 and sys.argv[1] == "stage":
        stage(sys.argv[2], sys.argv[3], sys.argv[4])
    elif len(sys.argv) == 3 + 6 and sys.argv[1] == "tally":
        # tally <label> o1a o1b o1c o2a o2b o2c
        tally(sys.argv[2], sys.argv[3:6], sys.argv[6:9])
    else:
        sys.exit("usage: stage <label> <old.png> <new.png> | tally <label> <o1 judge1-3> <o2 judge1-3>")
