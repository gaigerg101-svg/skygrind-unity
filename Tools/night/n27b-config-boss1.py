#!/usr/bin/env python3
# NIGHT27 builder B: fills design.n27.boss1 (THE LANTERN) in Design/unity-config.json by text replacement (the file's hand formatting stays), then copies it to
# Assets/StreamingAssets/Design/unity-config.json (the Player's copy must stay byte-identical). Run from the repo root. `enabled` stays false.
import re, shutil, json

P = "Design/unity-config.json"
s = open(P, encoding="utf-8").read()
start = s.index('      "boss1": {')
end = s.index('      "boss2": {')
new = '''      "boss1": {
        "_why": "Level 1's boss is THE LANTERN: a hovering shielded box whose shield is down only on the face that looks at a wall the player is riding (GG: 'the first boss fight is way too much like the Cerberus ... original, requires actual uses of our current gameplay mechanics'). Off (--off27): THE SLAG HULK. Core/Enemies/Lantern27.cs, Design/maps/lantern-arena.json (Tools/night/gen-lantern.py). Every number below is Unity-only (the web had no lantern); each group has its _why.",
        "enabled": false,
        "name": "THE LANTERN",
        "file": "Design/maps/lantern-arena.json",
        "room": {
          "_why": "Where the courtyard sits in the world (the worm 400, MONOLITH -400, BULWARK 800, ELDER WYRM 1200, HULK 1600, GLACIER 2000), the run-seed salt, and the one soft point light (pale violet-white boundary tone, rule 8) over the ring.",
          "origin": [2400, 0, 0], "salt": 1214347, "lightHeight": 9, "lightRange": 40, "lightIntensity": 8, "lightColor": 13616895
        },
        "arena": {
          "_why": "The courtyard's size as the sim reads it (Design/maps/lantern-arena.json: inner half-width 16 m = 32 m across, 12 m high): the orbs die at the inner wall.",
          "half": 16, "height": 12
        },
        "hp": {
          "_why": "One pool; phases at 66 / 33 % (the worm's marks); the last finisherAt of it is the CORE: a gun lands x finisherGunMult there (a slow finish by guns, so the fight never waits on a charge), the PULSE x finisherPulseMult (the 5-kill charge's payoff). Pool tuned so the pistol bot with wall rides kills in 75-100 s (harness lantern).",
          "pool": 1800, "phase2At": 0.66, "phase3At": 0.33, "finisherAt": 0.18, "finisherGunMult": 0.4, "finisherPulseMult": 3
        },
        "body": {
          "_why": "The box hovers hoverY m up (a player on the floor shoots up at it, a player riding is level with it); half = its half size (2.4 m cube); turn1-3 rad/s: the plate turns to you SLOWLY, so a hop to the far lane opens a window while it catches up (slower each phase); introHold s: no damage while it rises; dieTime s the finisher.",
          "hoverY": 3, "half": 1.2, "turn1": 1.6, "turn2": 1.2, "turn3": 1.0, "introHold": 2.5, "dieTime": 2.2
        },
        "shield": {
          "_why": "segments: the ring's eight wall segments (sector i = directions [i, i+1) x 45 deg from +x); grace s: the face stays lit this long after you leave the wall, for the shot already in the air (a ride lasts at most wallRide.maxTime 1.2 s).",
          "segments": 8, "grace": 0.35
        },
        "orb": {
          "_why": "The slow readable orange orbs (red-orange: danger only): speed 9 m/s (the sprint is about 9), radius, damage 14, life s; the fan (three orbs, fanDeg apart) in phase 1, the ring (ringCount radial orbs at two heights, lowY jump / highY duck) from phase 2, the rain (rainCount orbs fall on decals of radius rainR from rainHeight at rainFall m/s, spread around you) from phase 3. Telegraph: the first attack of a fight 0.8 s, later 0.6 s (GG's brief). every1-3 s of rest between attacks, recover s after.",
          "speed": 9, "radius": 0.55, "damage": 14, "life": 4.5, "lowY": 1, "highY": 2.7, "tellFirst": 0.8, "tellLater": 0.6,
          "fanCount": 3, "fanDeg": 16, "ringCount": 8, "rainCount": 5, "rainR": 1.6, "rainFall": 14, "rainHeight": 9, "rainSpread": 6,
          "recover": 0.9, "every1": 3.4, "every2": 3, "every3": 2.6
        },
        "walls": {
          "_why": "Phase 3: every retractEvery s a run of retractCount neighbouring segments warns for retractTell s (floor decal) then drops for retractTime s: you chain rides around the ring along the ones that stay.",
          "retractEvery": 5, "retractTell": 1.2, "retractTime": 4, "retractCount": 2
        },
        "sentry": {
          "_why": "Phase 3 drops five small SENTRIES that orbit the ring (orbit m, speed rad/s, low / high m): their kills fill the PULSE's five pips (the teaching of the kill-charge). hp 40: two pistol shots.",
          "count": 5, "hp": 40, "radius": 0.6, "orbit": 10, "speed": 0.5, "low": 2.2, "high": 5
        }
      },
'''
s = s[:start] + new + s[end:]
open(P, "w", encoding="utf-8").write(s)
json.loads(s)  # must still parse
shutil.copyfile(P, "Assets/StreamingAssets/Design/unity-config.json")
print("boss1 written; config parses")
