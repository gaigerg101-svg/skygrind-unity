# NIGHT20: decode a blind reviewer's picks with its key: N20 = the side whose folder is tonight's reel (b5n20 / final20),
# N19 = Logs/feel/A20 (NIGHT19's game). picks: "s01=A s02=B ... " (Same / Unknown allowed). Prints a markdown table and counts.
#   python decode20.py <key.json> "<picks>" [reviewer label]
import sys, json
key = json.load(open(sys.argv[1], encoding='utf-8'))
picks = dict(p.split('=') for p in sys.argv[2].split())
who = sys.argv[3] if len(sys.argv) > 3 else ''
n = {'N20': 0, 'N19': 0, 'Same': 0, 'Unknown': 0}
print('| Reviewer | Scenario | Pick | Side |')
print('| --- | --- | --- | --- |')
for sid, s in sorted(key['scenarios'].items()):
    p = picks.get(sid, 'Unknown')
    if p in ('A', 'B'):
        side = 'N19' if 'A20' in s[p] else 'N20'
    else:
        side = p
    n[side] += 1
    print(f'| {who} | {sid} {s["name"]} | {p} | {side} |')
print(f'{who}: N20 {n["N20"]}, N19 {n["N19"]}, Same {n["Same"]}, Unknown {n["Unknown"]}')
