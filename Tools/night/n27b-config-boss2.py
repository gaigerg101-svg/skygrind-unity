#!/usr/bin/env python3
# NIGHT27 builder B: fills design.n27.boss2 (THE GLACIER, pulse-only chase) in Design/unity-config.json by text replacement (the hand formatting stays), then copies it to
# Assets/StreamingAssets/Design/unity-config.json (byte-identical). Run from the repo root. `enabled` stays false.
import shutil, json

P = "Design/unity-config.json"
s = open(P, encoding="utf-8").read()
start = s.index('      "boss2": {')
end = s.index('      "slowAim27": {')
new = '''      "boss2": {
        "_why": "Level 2's boss is THE GLACIER as a pulse-only chase on a curving ice course with the original body (GG: 'you can only attack him if you are able to catch up to him and hit him with the pulse'). Off (--off27): NIGHT26's glacier. Core/Enemies/Glacier27.cs. Every number below is Unity-only (the web had no glacier); each group has its _why.",
        "enabled": false,
        "name": "THE GLACIER",
        "course": {
          "_why": "The same 40 m ice tile as NIGHT26 (Design/maps/glacier-course.json: three lanes at x -7 / 0 / 7, low rails, wall lines), a window of windowBack tiles behind and windowAhead ahead of the player's, a checkpoint every 4 tiles. Straight in the SIM; the curve is view data (curve below).",
          "paths": "Design/maps/glacier-course.json", "tileLen": 40, "halfWidth": 12, "railX": 3.5, "laneX": [-7, 0, 7], "windowBack": 2, "windowAhead": 5, "checkpointTiles": 4
        },
        "room": {
          "_why": "Where tile 0 sits in the world (GLACIER 2000, LANTERN 2400), the run-seed salt, and the one light (pale violet-white boundary tone, rule 8) that follows the player along the course.",
          "origin": [2800, 0, 0], "salt": 1347120027, "lightHeight": 8, "lightRange": 60, "lightIntensity": 9, "lightColor": 13616895
        },
        "hits": {
          "_why": "THREE PULSE hits kill it (GG: only the pulse): the pool is split in thirds (the HP bar's two marks), whatever the swing's damage; no gun, and no hit outside the window, ever lands.",
          "pool": 3000, "toKill": 3
        },
        "speed": {
          "_why": "THE GAP CONTROLLER (R2 rules 11-17): the boss's speed = baseFrac x top + clamp(k x (gap - targetGap), +-clampFrac x top). top m/s is the player's top speed on the ice (the sprint is about 9 and a boost strip adds 6); clampFrac 0.15: the controller never moves the boss more than 15 % of the player's top speed off its base, so it is catchable and never a timer. targetGap m is where it settles when you hold the line (targetStep m further each phase: it runs further ahead as the fight goes on; a player who runs PAST it meets its fastest speed); k m/s per metre of gap error. laneMove / laneEvery: how fast and how often it slides between lanes; introHold s: no attacks while it pulls away.",
          "top": 8.5, "baseFrac": 0.9, "k": 0.5, "clampFrac": 0.15, "targetGap": 22, "targetStep": 4, "laneMove": 6, "laneEvery": 3, "introHold": 3
        },
        "gap": {
          "_why": "The chase starts at gap.start m; a PULSE-READY window of windowTime s opens when the gap falls under windowAt m (GG's brief: 12 m, 1.5 s); a window that runs out costs missCost m of gap (the boss skates on) and NEVER health; armTime s before a window may open again; a hit surges the boss hitBack m ahead; at failAt m you go back to the last checkpoint (the boss keeps its hits).",
          "start": 45, "windowAt": 12, "windowTime": 1.5, "missCost": 20, "armTime": 2, "hitBack": 45, "failAt": 90
        },
        "launch": {
          "_why": "THE PULSE LAUNCH's lock-on: the launch itself reaches about 5 m (Fist24: 2.25 jump distances) while the window opens at a 12 m gap, so a launch released while the window is open, with the boss within reach m and the throw within coneDeg degrees of the line to it, LANDS (GG: 'a PULSE launch that lands on the boss counts'). refill: the kill pips a hit gives back (the pips fill only from kills and there are none on the ice: without it only one launch exists), so three launches win the chase.",
          "reach": 13, "coneDeg": 16, "refill": 5
        },
        "tells": {
          "_why": "Every lane attack tells >= 0.6 s (a floor decal 0.75-0.8 s, GG's brief 0.7-0.8): the low bar (slide) and the sweeping bar (jump) 0.75 s, the roller 0.8 s (jump), the icicle fall 0.8 s (be in a wall column). Never two lethal attacks inside resolveGap.",
          "lane": 0.75, "roller": 0.8, "icicle": 0.8, "resolveGap": 0.5
        },
        "attack": {
          "_why": "A lane attack is placed `ahead` m in front of the player (never beyond the boss) and lethal for `life` s after its tell; damage 12 of the player's 100 and a push back; `thick` m thick. Heights (m): the sweeping bar 0.45 (any jump), the low bar from 1.15 to 2.8 (slide). Phase 2 adds the ROLLER (a log rolling across the lanes at rollerSpeed m/s, rollerH m tall: jump it); phase 3 adds the ICICLE FALL (ceiling spikes icicleTop m tall over all but icicleSafe m of each wall: ride or hug the wall). every1-3 s between attacks per phase.",
          "life": 1.4, "ahead": 18, "damage": 12, "push": 8, "thick": 1.2, "sweepTop": 0.45, "lowBottom": 1.15, "lowTop": 2.8,
          "rollerSpeed": 7, "rollerH": 1.1, "rollerLife": 2.6, "icicleTop": 4.5, "icicleSafe": 1.2, "icicleLife": 1, "every1": 4, "every2": 3.4, "every3": 3
        },
        "body": {
          "_why": "The skater's hit box: one large blocky body on one blade (GG: 'just switch back to the original [model] because it doesn't look good'; DECIDE27 D-3: the web's blocky enemy style, rebuilt as the web's models are), boxW x boxH x boxL m; dieTime s of finisher.",
          "boxW": 2.4, "boxH": 3.4, "boxL": 1.8, "dieTime": 2.5, "contactDamage": 0, "model": "Design/bosses/glacier27.json"
        },
        "curve": {
          "_why": "The course bends in the VIEW only (Glacier27Curve; the sim and collider stay straight so golden and replays are untouched): each 40 m tile turns the heading by up to maxTurnDeg degrees (~22, from (run seed ^ salt, tile index)); tiles is the ring the view recycles. viewOn false until the camera follows the bend (the lead's hook: the tiles, the boss and the attacks bend, the player camera must too, or the player would fly off a bent road).",
          "maxTurnDeg": 22, "tiles": 6, "salt": 1818181, "viewOn": true
        }
      },
'''
s = s[:start] + new + s[end:]
open(P, "w", encoding="utf-8").write(s)
json.loads(s)
shutil.copyfile(P, "Assets/StreamingAssets/Design/unity-config.json")
print("boss2 written; config parses")
