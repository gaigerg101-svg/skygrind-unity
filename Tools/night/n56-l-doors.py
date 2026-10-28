# NIGHT56 builder L, seeded leftover S1-doors (NIGHT55 P1-doorDark; the meter Tools/night/n55-r-flat.py: a frame is FLAT when its world-area
# luma std < 0.03). The lead's r1 count on verbs37 x5 (every 50th kept frame): foundry 1 (vf f0500), iceline 3 (vi f0850, f0900, f1350), kiln 1
# (vk f0850), span 0, spire 2 (vp f1550, f1600). From each frame's telemetry camera (web coords; the yaw is not recorded) and its picture:
#   vi f0900 (9.8, 1.8, -22.0) and f1350 (10.7, 1.6, -9.8): verbs37's wall steps against g1's east wall (x 11): the frame is one flat blue panel
#     (g1's lights are two ceiling panels on x 0 and a pool at the door: nothing reaches the east wall);
#   vi f0850 (8.5, 3.6, -0.3): 0.3 m off g1's south wall (z 0), 2.5 m off its east wall;
#   vk f0850 (9.8, 3.5, -225.2), looking up: 0.3 m off kb3's north wall (z -225.5) at its east corner (kb3's ceiling panels stand on x 0);
#   vp f1550 / f1600 (-12.1, 1.6, 67.3), looking up: 0.3 m off THE BELL's south face (v2's 5 m deck, z 67);
#   vf f0500 (0, 0.9, -47.9), sliding (+10 SLIDE BITE), the frame's lower two thirds one dark floor: the facing is not known without the yaw:
#     NOT lit here (a guess would be a lamp in the wrong place).
# Each spot gets one white wall lamp flush to the face the camera is against (levelgen's light check: flush to a face; white is no accent,
# LevelValidate's one accent per room is kept), at the camera's height band, so the face reads as a lit surface with a falloff, not one flat
# colour. Lights have no collider: the sim, the walk bot and the contract are untouched.
#   python -I Tools/night/n56-l-doors.py [--dry]   then bash Tools/night/n56-l-regen.sh iceline kiln spire
#   python -I Tools/night/n56-l-doors.py --slice2   then bash Tools/night/n56-l-regen.sh foundry
import json, os, sys

sys.dont_write_bytecode = True
TAG = 'NIGHT56 L (n56-l-doors.py, S1-doors: the flat frames of r1 verbs37)'
ADD2 = {
    # slice 2: foundry. vf f0500's camera (0, 0.9, -47.9) slides NORTH (telemetry: Unity z 46.0 -> 47.9 -> 49.5 at 11.6 m/s forward, i.e. web
    # -z), at 0.9 m: the frame's top is d2's arch 7 m ahead (a3's door), the rest a2's dark north floor. a2 itself cannot take a lamp: foundry's
    # finish step (gen-n52-foundry.cjs --finish, not this builder's file) keeps every room but a3 / a5 / b1 bit-equal to n48-foundry's (lights
    # included). So the lamp is THE DOOR's, in a3's row: a white wall lamp on d2's east cheek (the hull x 2..5 at a3's south wall), its west
    # face 1 m inside a3, lighting the doorway's mouth that the frame sees through the door (GG w37: "you just run through it and it's like
    # dark"). First cut: a panel under d2's lintel (0, 4.45, -57): the level report's lights check, ERROR "outside room a3's floor plan".
    'foundry': [
        {'id': 'a3-L56d', 'room': 'a3', 'kind': 'wall', 'at': [1.95, 3.5, -60], 'normal': [-1, 0, 0], 'size': [1.5, 0.3, 0.1], 'color': 'white', 'range': 7, 'intensity': 1.3,
         'why': "THE DOOR LAMP on d2's east cheek, a3's side (a2 -> a3): the doorway's mouth lit, seen through the door from a2 (r1 / r2 vf f0500 at (0, 0.9, -47.9) sliding north at d2, luma std 0.0197)"},
    ],
}
ADD = {
    'iceline': [
        {'id': 'g1-L56a', 'room': 'g1', 'kind': 'wall', 'at': [10.95, 3.5, -21.5], 'normal': [-1, 0, 0], 'size': [2.5, 0.3, 0.1], 'color': 'white', 'range': 8, 'intensity': 1.2,
         'why': "the EAST WALL LAMP (south): verbs37's wall steps on g1's east wall (r1 vi f0900 at (9.8, 1.8, -22.0), luma std 0.0181: one flat panel); g1's ceiling panels stand on x 0"},
        {'id': 'g1-L56b', 'room': 'g1', 'kind': 'wall', 'at': [10.95, 3.5, -10], 'normal': [-1, 0, 0], 'size': [2.5, 0.3, 0.1], 'color': 'white', 'range': 8, 'intensity': 1.2,
         'why': "the EAST WALL LAMP (north): the slot between the east ride panel and the east wall (r1 vi f1350 at (10.7, 1.6, -9.8), luma std 0.0219)"},
        {'id': 'g1-L56c', 'room': 'g1', 'kind': 'wall', 'at': [7, 4, -0.05], 'normal': [0, 0, -1], 'size': [2.5, 0.3, 0.1], 'color': 'white', 'range': 8, 'intensity': 1.1,
         'why': "the SOUTH WALL LAMP: the start wall's east half (r1 vi f0850 at (8.5, 3.6, -0.3), luma std 0.0149)"},
    ],
    'kiln': [
        {'id': 'kb3-L56a', 'room': 'kb3', 'kind': 'wall', 'at': [8.5, 5.5, -225.45], 'normal': [0, 0, 1], 'size': [2.5, 0.3, 0.1], 'color': 'white', 'range': 9, 'intensity': 1.2,
         'why': "the NORTH-EAST WALL LAMP: kb3's north wall at its east corner (r1 vk f0850 at (9.8, 3.5, -225.2) looking up, luma std 0.0128); kb3's ceiling panels stand on x 0"},
    ],
    'spire': [
        {'id': 'v2-L56a', 'room': 'v2', 'kind': 'wall', 'at': [-12, 3.2, 67.05], 'normal': [0, 0, 1], 'size': [2.5, 0.3, 0.1], 'color': 'white', 'range': 7, 'intensity': 1.1,
         'why': "the BELL's FACE LAMP: THE BELL's 5 m south face (r1 vp f1550 / f1600 at (-12.1, 1.6, 67.3) looking up, luma std 0.0203 / 0.0250)"},
    ],
}


def main():
    dry = '--dry' in sys.argv
    todo = ADD2 if '--slice2' in sys.argv else ADD  # slice 1 is in the plans (24334c25); --slice2 adds foundry a2's floor lamp
    for lv, rows in todo.items():
        path = os.path.join('Design', 'levels', 'n52', lv + '.plan.json')
        text = open(path, encoding='utf-8', newline='').read()
        plan = json.loads(text)
        before = json.loads(text)
        ids = {l.get('id') for l in plan['lights']}
        for row in rows:
            assert row['id'] not in ids, row['id'] + ' already there'
            row = dict(row, job='light')
            row['why'] = TAG + ': ' + row['why']
            last = max(i for i, l in enumerate(plan['lights']) if l.get('room') == row['room'])
            plan['lights'].insert(last + 1, row)
        for k in before:
            if k == 'lights':
                assert [l for l in plan['lights'] if not str(l.get('id', '')).endswith(('-L56a', '-L56b', '-L56c', '-L56d'))] == before['lights'], 'an old light changed'
            else:
                assert before[k] == plan[k], k + ' changed'
        print('%s: + %s' % (lv, ', '.join(r['id'] for r in rows)))
        if dry:
            continue
        eol = '\r\n' if '\r\n' in text else '\n'
        open(path, 'w', encoding='utf-8', newline='').write(json.dumps(plan, indent=1, ensure_ascii=False).replace('\n', eol) + (eol if text.endswith('\n') else ''))
        print('wrote ' + path)


if __name__ == '__main__':
    main()
