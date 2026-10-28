# NIGHT26 builder B: writes design.n26.bosses (hulk, wyrm26, glacier) into Design/unity-config.json as TEXT (the file is CRLF and hand-formatted: a json round
# trip would rewrite all of it). Idempotent: an existing design.n26.bosses block is replaced, a missing design.n26 is created after the last design block.
#   python Tools/night/n26b-config.py            (run from the repo root; the rows live in Tools/night/n26b-rows.json)
import json, re, sys

PATH = 'Design/unity-config.json'
ROWS = json.load(open('Tools/night/n26b-rows.json', encoding='utf-8'))
raw = open(PATH, 'rb').read().decode('utf-8')
nl = '\r\n' if '\r\n' in raw else '\n'
text = raw.replace('\r\n', '\n')

def scalar(v):
    return json.dumps(v, ensure_ascii=False)

def emit(d, ind):
    """A dict as JSON text: _why on its own line, scalar leaves on one line, nested dicts as blocks."""
    pad = ' ' * ind
    lines = []
    leaves = []
    nested = []
    for k, v in d.items():
        if k == '_why':
            continue
        (nested if isinstance(v, dict) else leaves).append((k, v))
    if '_why' in d:
        lines.append(pad + '  "_why": ' + scalar(d['_why']))
    if leaves:
        lines.append(pad + '  ' + ', '.join('"%s": %s' % (k, scalar(v)) for k, v in leaves))
    for k, v in nested:
        lines.append(pad + '  "%s": {' % k + '\n' + emit(v, ind + 2) + '\n' + pad + '  }')
    return ',\n'.join(lines)

block = '"bosses": {\n' + emit(ROWS, 6) + '\n      }'

def match_end(s, start):
    """Index just past the brace that closes the '{' at s[start]."""
    depth = 0
    i = start
    instr = False
    while i < len(s):
        ch = s[i]
        if instr:
            if ch == '\\':
                i += 1
            elif ch == '"':
                instr = False
        else:
            if ch == '"':
                instr = True
            elif ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth == 0:
                    return i + 1
        i += 1
    raise SystemExit('unbalanced braces')

m26 = re.search(r'^    "n26": \{', text, re.M)
if m26:
    open_at = m26.end() - 1
    end = match_end(text, open_at)
    inner = text[open_at:end]
    mb = re.search(r'^      "bosses": \{', inner, re.M)
    if mb:
        b0 = open_at + mb.start()
        b1 = match_end(text, open_at + mb.end() - 1)
        text = text[:b0] + '      ' + block + text[b1:]
        print('replaced design.n26.bosses')
    else:
        ins = open_at + 1
        text = text[:ins] + '\n      ' + block + ',' + text[ins:]
        print('added design.n26.bosses to the existing design.n26')
else:
    tail = '\n  }\n}'
    stripped = text.rstrip('\n')
    assert stripped.endswith(tail), 'unexpected end of file'
    cut = len(stripped) - len(tail)
    text = text[:cut] + ',\n    "n26": {\n      ' + block + '\n    }' + tail + '\n'
    print('created design.n26 with bosses')
json.loads(text)  # must still parse
open(PATH, 'wb').write(text.replace('\n', nl).encode('utf-8'))
