"""Runs `campaign <plan> --seeds 10` for levels 6-10, reads the median clear times and the first-try rates, writes Tools/night/levels610-pars.json
(room id -> par seconds, the bot's median over 10 seeds) and prints the table. Then `python Tools/night/gen-levels610.py --pars Tools/night/levels610-pars.json`
writes the pars into the plans (never hand-typed)."""
import json, os, re, subprocess, sys
ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
DLL = os.path.join(ROOT, 'Temp', 'SimHarness', 'bin', 'Debug', 'net10.0', 'SimHarness.dll')
pars, table = {}, []
for lid in ['level06', 'level07', 'level08', 'level09', 'level10']:
    plan = json.load(open(os.path.join(ROOT, 'Design', 'levels', lid + '.plan.json')))
    names = {r['name']: r['id'] for r in plan['rooms']}
    out = subprocess.run(['dotnet', DLL, 'campaign', os.path.join('Design', 'levels', lid + '.plan.json'), '--seeds', '10', '--rooms', str(len(plan['rooms']))],
                         cwd=ROOT, capture_output=True, text=True).stdout
    for line in out.splitlines():
        m = re.match(r'clear time (.+?) \(normal, 10 seeds\): median ([\d.]+) s', line)
        if m and m.group(1) in names:
            pars[names[m.group(1)]] = round(float(m.group(2)), 1)
        m = re.match(r'clear rate room \d+ (.+?) \(normal, 10 seeds\): first-try (\d+)/10', line)
        if m:
            table.append((lid, m.group(1), int(m.group(2)) * 10))
json.dump(pars, open(os.path.join(ROOT, 'Tools', 'night', 'levels610-pars.json'), 'w'), indent=1)
for t in table: print(t)
print(pars)
