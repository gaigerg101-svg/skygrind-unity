#!/usr/bin/env python3
"""NIGHT26 shared commit: the design.n26 block (seven rows, all enabled false) appended after design.n25 in Design/unity-config.json."""
import io, json
p = 'Design/unity-config.json'
s = io.open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in s else '\n'
rows = [
 ('pulseLT', "The PULSE GAUNTLET's blast is the LEFT TRIGGER (builder W, GG 2026-10-08: 'the gauntlet the second weapon it doesn't do the blast attack at all with the left trigger ... it's supposed to charge and build up'): holding LT charges by use (its hits), releasing launches the player along the aim with a little damage, a 3 s lockout; the left click is a plain swing; R3 punches. NIGHT25 put this on the click: wrong. Off (--off26): NIGHT25's click-charge."),
 ('weaponIdentity', "Every weapon has an LT and a cost, enemies have vulnerabilities, the rocket and the BOUNCER are nerfed, weapons unlock one per level or two (builder W, GG: 'almost all the weapons should really have a left trigger that either zooms in or does something else ... you really don't feel incentivized at all to switch weapons ... the grenade and rocket launcher are a little too broken'). Numbers are added by builder W under this row, each with a _why. Off (--off26): NIGHT25's arsenal and loadout."),
 ('surfaces', "Ice, mud and boost strips from a level plan's `surfaces` list through the one named Mover change: a public groundFrictionMult (default 1) multiplied into the ground friction call (builder M, GG: 'different surfaces ... speed skating'). Golden vectors and trajectory replays stay exact (default 1). Off (--off26): every surface reads as normal ground."),
 ('levelSelect', "The ULTRAKILL-style level select, three ranks (time, kills, style), the aggregate and P rank, the best per level in the profile, CAMPAIGN as the default and first mode (builder L, GG: 'when you click into the campaign which should be the first game mode by the way that should be the default one ... it should be like ULTRAKILL where it kind of shows all the levels what rank you got what time'). Off (--off26): NIGHT25's TITLE and results."),
 ('level1', "Level 1 THE FOUNDRY: the tutorial shortened, every room at most 28 m wide, a platforming run and a wall-ride hall, ending in THE SLAG HULK (builder L and B, GG: 'for that first map the arena is just way too wide ... platforming sections'). Off (--off26): NIGHT25's tutorial."),
 ('level2', "Level 2 THE ICE LINE: the speed-skating chase on ice, pistol only, THE GLACIER ahead with telegraphed lane attacks and a gap meter (builder B and M, GG: 'the second one to feel like you're speed skating to get this boss'). Off (--off26): level 2 is NIGHT25's THE KILN."),
 ('level3', "Level 3 THE KILN: today's layer 1 rooms trimmed and the worm with proximity aggro, body x0.5 / head x2, a final attack and a wall-riding arena (builder B and L, GG: 'it should almost feel a little like Terraria'). Off (--off26): NIGHT25's worm."),
]
why = "NIGHT26 U3-40 (GG 2026-10-08, after his first playtest: the first three levels, platforming, a chase boss, weapon identity, a level select with ranks, the PULSE on the left trigger): the seven NIGHT26 rows, one block each (design.n26.<row>.enabled), all off in the shared commit; a row is turned on only when its contract passes. Off (--off26 / -skygrindOff26): NIGHT25's game bit for bit. Unity-only numbers live in each row with a _why."
parts = ['      "%s": {\n        "_why": %s,\n        "enabled": false\n      }' % (n, json.dumps(w)) for n, w in rows]
block = ',\n    "n26": {\n      "_why": ' + json.dumps(why) + ',\n' + ',\n'.join(parts) + '\n    }'
old = '    }\n  }\n}'.replace('\n', nl)
assert s.rstrip().endswith(old)
i = s.rstrip().rindex(old)
new = s[:i] + '    }' + block.replace('\n', nl) + nl + '  }' + nl + '}'
io.open(p, 'w', encoding='utf-8', newline='').write(new)
d = json.load(io.open(p, encoding='utf-8'))
print('n26 rows:', [k for k in d['design']['n26'] if k != '_why'], 'n25 rows kept:', len(d['design']['n25']))
