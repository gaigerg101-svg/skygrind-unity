import json,io
p='tasks.json'
raw=io.open(p,encoding='utf-8',newline='').read()
d=json.loads(raw)
rows=[
("U3-36","NIGHT22: FINISH, POLISH, PROVE (4 h box, 4.5 h cap)","GG 2026-10-07: 'Prioritize building an iteration polishing accuracy all of.'"),
("U3-36.1","NIGHT22: start (Editor gate, scripts, contracts, tasks)","The Editor gate first (L-025)."),
("U3-36.2","NIGHT22: the shared commit (N22Blocks, tape format 7, scaffolding, meters)","Builders compile against it."),
("U3-36.3","NIGHT22: ARCLINE proved (chip, pad press, three looks)","NIGHT21 built the Core, no HUD."),
("U3-36.4","NIGHT22: THE STAMP and T-11","Entrance and the grade card off POINTS."),
("U3-36.5","NIGHT22: DEADLINE bar","Fuse number is the bar."),
("U3-36.6","NIGHT22: THE GRIND wired","Smoke step, persistence, tape mode."),
("U3-36.7","NIGHT22: INSTANT REPLAY view and a real error","60-minute cap."),
("U3-36.8","NIGHT22: challenge, ghost-daily, dusk-clock, haptics","Four new rows."),
("U3-36.9","NIGHT22: the accuracy sweep A-1..A-7","Each a number from a run."),
("U3-36.10","NIGHT22: the leftovers in order","Directed steps, T-12, T-10, TRICK CHAIN, OVERDRIVE, spawn beam, shine."),
("U3-36.11","NIGHT22: the loop and the close","Freeze 3:10, tag u3-n22."),
]
for id_,title,why in rows:
    d['items'].append({"id":id_,"stage":"U3","title":title,"why":why,"acceptance":["see NIGHT22.md"],"check":{"how":"NIGHT22-LOG.md close table","expect":"pass"},"status":"not_started","evidence":None,"owner":"agent","blockedBy":[],"decisions":[],"notes":""})
for it in d['items']:
    if it['id']=="U3-36.1": it['status']='passing'; it['evidence']='Editor gate poll 1 ready 3:04 PM; CPU 100 %; contracts copied; this file parses (2026-10-07)'
io.open(p,'w',encoding='utf-8',newline='').write(json.dumps(d,indent=2,ensure_ascii=False)+"\n")
json.load(open(p,encoding='utf-8')); print('ok',len(d['items']))
