#!/usr/bin/env python3
"""NIGHT24 T-12 helper: the M9 pop-in list in one command.

    python Tools/night/m9-list.py <reel folder> [--alpha 0.95] [--all] [--json]

Reads every scenario's telemetry.jsonl in a reel folder (Logs/feel/<tag>/on, or its parent) with Tools/feel-metrics.py's own parsing
(Reel / Scenario / _ui_runs / ui_list, imported, not copied), and lists every ui[] path whose FIRST frame (after frame 0: an element that is
there from the start is not an entrance) is drawn at alpha >= --alpha (default 0.95, M9's POP_ALPHA). Per line: scenario, first frame, the path,
first-frame alpha and scale, the text if it has one. Re-entries (an element that left and came back) are marked. --all lists every first appearance with
its alpha, not just the alpha-1 ones. Exit 0 always (a list, not a gate); the last line counts unique paths.
"""
import importlib.util
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
FM = os.path.join(HERE, '..', 'feel-metrics.py')


def load_fm():
    spec = importlib.util.spec_from_file_location('feel_metrics', FM)
    mod = importlib.util.module_from_spec(spec)
    sys.modules['feel_metrics'] = mod
    spec.loader.exec_module(mod)  # its main() sits behind __name__ == '__main__'
    return mod


def main(argv):
    args = [a for a in argv if not a.startswith('--')]
    flags = [a for a in argv if a.startswith('--')]
    alpha_min = 0.95
    for i, a in enumerate(argv):
        if a == '--alpha' and i + 1 < len(argv):
            alpha_min = float(argv[i + 1])
            if argv[i + 1] in args:
                args.remove(argv[i + 1])
    if not args:
        print(__doc__)
        return 2
    fm = load_fm()
    reel = fm.Reel(args[0])
    for e in reel.errors:
        print('error:', e)
    rows, unique = [], {}
    for name in reel.order:
        sc = reel.scenarios[name]
        runs = fm._ui_runs(sc)
        for eid, rl in runs.items():
            for k, run in enumerate(rl):
                idx, f, e, r = run[0]
                if idx == 0:
                    continue  # there from the first frame: not an entrance
                a0 = fm.num(e.get('alpha'), 1.0)
                s0 = fm.num(e.get('scale'), 1.0)
                if a0 < alpha_min and '--all' not in flags:
                    continue
                path = e.get('path') or ('id %s' % eid)
                text = e.get('text') if isinstance(e.get('text'), str) else ''
                rows.append({'scenario': name, 'frame': f, 'path': path, 'alpha': round(a0, 3), 'scale': round(s0, 3),
                             'reentry': k > 0, 'text': text.strip()[:40]})
                if a0 >= alpha_min:
                    unique.setdefault(path, []).append(name)
    if '--json' in flags:
        print(json.dumps({'popins': rows, 'unique': {p: sorted(set(v)) for p, v in unique.items()}}, indent=1))
        return 0
    for r in rows:
        print('%-18s f%-5d a=%.2f s=%.2f %s%s%s' % (r['scenario'], r['frame'], r['alpha'], r['scale'], r['path'],
                                                     ' [re-entry]' if r['reentry'] else '',
                                                     (' "%s"' % r['text']) if r['text'] else ''))
    print('m9-list: %d first appearance line(s) at alpha >= %.2f in %d scenario(s); %d unique path(s)' % (
        sum(1 for r in rows if r['alpha'] >= alpha_min), alpha_min, len(reel.order), len(unique)))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
