# NIGHT56 builder L, S1-doors slice 3 (the lead's r3 flat list after slice 1: iceline f0850 0.0137, f0900 0.0238, f1350 0.0220; spire f1550
# 0.0205, f1600 0.0249; foundry, kiln, span 0). From r3's telemetry (Logs/feel/n56-r3/v?/verbs37/telemetry.jsonl, main repo): the camera's
# distance to the face it looks at sets what a lamp can do:
#   vi f0900 (9.8, 1.77, -22.0), still, 1.2 m off g1's east wall: the frame covers ~0.9-2.7 m of the wall's height; slice 1's lamp g1-L56a
#     at 3.5 m lit the wall above the frame (0.0181 -> 0.0238): it comes down to 2.4 m and to z -22, inside the frame;
#   vp f1550 / f1600 (-12.1, 1.6, 67.35), looking 34 deg up, 0.35 m off THE BELL's face: the frame covers ~1.6-2.6 m of the face; slice 1's
#     v2-L56a at 3.2 m sat over it: it comes down to 2.3 m;
#   vi f0850 (8.5, 3.6, -0.35) moving 15 m/s along g1's south wall, and f1350 (10.65, 1.6, -9.8), 0.35 m off g1's east wall: the frame is
#     ~0.5 m of wall at a third of a metre: no lamp placement lights that patch except one put at the reel's own sample point (not done: that
#     would light the meter, not the room). Not changed.
# Moves only this builder's own slice-1 lamps (asserted), nothing else in the plans.   python -I Tools/night/n56-l-doors3.py
import json, os, sys

sys.dont_write_bytecode = True
TAG = 'NIGHT56 L (n56-l-doors3.py, S1-doors slice 3)'
MOVE = {
    'iceline': ('g1-L56a', [10.95, 3.5, -21.5], [10.95, 2.4, -22], "moved into r3 vi f0900's frame (1.2 m off the wall: ~0.9-2.7 m of its height in view; 0.0238 at 3.5 m)"),
    'spire': ('v2-L56a', [-12, 3.2, 67.05], [-12, 2.3, 67.05], "moved into r3 vp f1550 / f1600's frame (0.35 m off the face, 34 deg up: ~1.6-2.6 m in view; 0.0205 / 0.0249 at 3.2 m)"),
}


def main():
    for lv, (lid, was, now, why) in MOVE.items():
        path = os.path.join('Design', 'levels', 'n52', lv + '.plan.json')
        text = open(path, encoding='utf-8', newline='').read()
        plan = json.loads(text)
        before = json.loads(text)
        l = next(x for x in plan['lights'] if x.get('id') == lid)
        assert l['at'] == was, lid + ' moved already'
        l['at'] = now
        l['why'] = l['why'] + ' ' + TAG + ': ' + why
        for k in before:
            if k == 'lights':
                assert [x for x in before['lights'] if x.get('id') != lid] == [x for x in plan['lights'] if x.get('id') != lid]
            else:
                assert before[k] == plan[k], k + ' changed'
        eol = '\r\n' if '\r\n' in text else '\n'
        open(path, 'w', encoding='utf-8', newline='').write(json.dumps(plan, indent=1, ensure_ascii=False).replace('\n', eol) + (eol if text.endswith('\n') else ''))
        print('%s: %s %s -> %s' % (lv, lid, was, now))


if __name__ == '__main__':
    main()
