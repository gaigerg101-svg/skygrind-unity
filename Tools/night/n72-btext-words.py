"""NIGHT72 B3 (btext72): the as-displayed word count of every boss line in UnityTuning.N72B.cs' Btext72Lines, a mirror of Btext72.DisplayWords.
Reads the C# table (no engine), counts each held / swap line the way the keyboard shows it ({fire} = LEFT CLICK, {altFire} = RIGHT CLICK: two words; any other token one),
and the same line with the pad's glyph (one word). Bar (the lead's rule, t3 verdict): at most 6 displayed words, a verb in CAPITALS, a glyph token where the raw has one.
Usage: python -I Tools/night/n72-btext-words.py [path to UnityTuning.N72B.cs]   (prints one row per line and PASS / FAIL; exit 1 on FAIL)
--planted: checks the counter on the t3 ON line (must read 7) and the cut line (must read 6); PASS or FAIL."""
import re
import sys

MAX_WORDS = 6
TWO_WORD = {"fire": "LEFT CLICK", "altFire": "RIGHT CLICK"}
PUNCT = ",.:;!?"


def displayed(line, keyboard=True):
    n = 0
    for w in line.split():
        t = w.strip(PUNCT)
        m = re.fullmatch(r"\{(\w+)\}", t)
        if m and keyboard and m.group(1) in TWO_WORD:
            n += len(TWO_WORD[m.group(1)].split())
        else:
            n += 1
    return n


def caps_verb(line):
    for w in line.split():
        t = w.strip(PUNCT)
        if len(t) < 2 or t[0] == "{":
            continue
        if all((not ch.isalpha()) or ch.isupper() for ch in t):
            return True
    return False


def rows(path):
    src = open(path, encoding="utf-8").read()
    out = []
    for m in re.finditer(r'^\s*\("((?:[^"\\]|\\.)*)",\s*(null|"[^"]*"),\s*"((?:[^"\\]|\\.)*)",\s*"((?:[^"\\]|\\.)*)"\),\s*$', src, re.M):
        out.append((m.group(1), m.group(2).strip('"'), m.group(3), m.group(4)))
    return out


def main(argv):
    if "--planted" in argv:
        a = displayed("SWAP to gauntlet, then SWING {fire}")
        b = displayed("SWAP gauntlet, then SWING {fire}")
        ok = a == 7 and b == 6
        print("planted: the t3 ON line reads %d (must be 7), the cut line %d (must be 6): %s" % (a, b, "PASS" if ok else "FAIL"))
        return 0 if ok else 1
    path = argv[1] if len(argv) > 1 and not argv[1].startswith("--") else "Assets/Scripts/Game/UnityTuning.N72B.cs"
    table = rows(path)
    bad = 0
    print("%d table rows from %s (bar: at most %d displayed words on a keyboard, a verb in capitals, the glyph token where the raw has one)" % (len(table), path, MAX_WORDS))
    for raw, weapon, held, swap in table:
        for kind, line in (("held", held), ("swap", swap)):
            kb, pad = displayed(line, True), displayed(line, False)
            caps = caps_verb(line)
            tok = ("{" in line) or ("{" not in raw)
            ok = kb <= MAX_WORDS and caps and tok
            if not ok:
                bad += 1
            print("%-4s keyboard %d pad %d caps %s %s | %s | raw: %s" % (kind, kb, pad, "yes" if caps else "NO", "ok" if ok else "FAIL", line, raw))
    print("RESULT: %s (%d lines over the bar)" % ("PASS" if bad == 0 and table else "FAIL", bad))
    return 0 if bad == 0 and table else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
