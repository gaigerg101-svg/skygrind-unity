#!/usr/bin/env python3
"""NIGHT25 shared commit: the design.n25 block (six rows, all enabled false) appended after design.n24 in Design/unity-config.json."""
import io, json
p = 'Design/unity-config.json'
s = io.open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in s else '\n'
rows = [
 ('meleeFire', "The quick melee and the gun's fire in the same frame (builder C, GG 2026-10-08: 'if you're shooting a weapon and you're trying to melee you can't do both at the same time it's either one action or the other'): the web serialises them (punch.js / weapons.js); with the row on a punch neither waits for nor cancels the gun's cycle, and the reverse. Off (--off25): NIGHT24's serialised rules."),
 ('pulseCharge', "The PULSE GAUNTLET's left click builds power by its own hits and launches the player with a little damage (builder C, GG: 'the pulse is the weapon that should have the actual pulse for the left click that's built based off of how many times you use it'). Numbers (hits per tier, the launch distances, the damage) are added by builder C under this row, each with a _why. Off (--off25): NIGHT24's fist-charge rules."),
 ('shockwaveOff', "No shockwave on the shotgun (builder C, GG: 'remove that shockwave effect from the shotgun should not be like that'): the THUNDERCLAP's LT shove (Core/Guns/Shockwave.cs, NIGHT19) is removed; its LT zooms (GG: most weapons zoom). Off (--off25): NIGHT24's shotgun with the shockwave."),
 ('wyrmAim', "The worm aims at the player and fights a little harder (builder W, GG: 'make it a little more difficult and make sure it's like obviously targeted towards you'): spit leads the target, the lunge locks at its tell and re-aims once, 10-15 % less breathing room per phase, phase 3 overlaps compatible attacks; every tell stays >= 0.5 s. Numbers are added by builder W under this row, each with a _why. Off (--off25): NIGHT24's worm."),
 ('tutorial', "Layer 1 as a six-stage TUTORIAL (builder L, GG: 'it should really be a tutorial for this first map and don't be afraid to break it up into stages'): look and walk, jump and slide, wall and air, the pistol, the fist, a first fight; each stage is Exposition / Validation / Challenge (RESEARCH.md S2), a respawn at the stage's start, no fail state. Off (--off25): NIGHT24's layer 1."),
 ('curriculum', "Levels 2-10 as plan files that each teach one new thing (builder L, GG: 'I'd like this to have 10 levels and they should all build up to each other'): the table is Design/levels/CURRICULUM.md. Off (--off25): only NIGHT24's layer 1."),
]
why = "NIGHT25 U3-39 (GG 2026-10-08: the teaching campaign, melee while firing, the PULSE, no shotgun shockwave, the worm aimed at the player): the six NIGHT25 rows, one block each (design.n25.<row>.enabled), all off in the shared commit; a row is turned on only when its contract passes. Off (--off25 / -skygrindOff25): NIGHT24's game bit for bit. Unity-only numbers live in each row with a _why."
parts = ['      "%s": {\n        "_why": %s,\n        "enabled": false\n      }' % (n, json.dumps(w)) for n, w in rows]
block = ',\n    "n25": {\n      "_why": ' + json.dumps(why) + ',\n' + ',\n'.join(parts) + '\n    }'
old = '        "shaderWarmup": true\n      }\n    }\n  }\n}'.replace('\n', nl)
assert old in s
new = ('        "shaderWarmup": true\n      }\n    }' + block + '\n  }\n}').replace('\n', nl)
io.open(p, 'w', encoding='utf-8', newline='').write(s.replace(old, new))
d = json.load(io.open(p, encoding='utf-8'))
print('n25 rows:', [k for k in d['design']['n25'] if k != '_why'])
