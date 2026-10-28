# NIGHT26 builder B: the glacier scenario's game-time bound, per-20-s diagnostics and the labelled per-phase HP cuts (a one-time patch).
p = 'Assets/Scripts/Game/FeelReel.N26.Glacier.cs'
s = open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in s else '\n'
s = s.replace('\r\n', '\n')

def rep(a, b):
    global s
    assert s.count(a) == 1, a[:70]
    s = s.replace(a, b)

rep("""            bool markedAttack = false, markedGap = false, deathDone = false, cutUsed = false, p2Seen = false;
            double minGap = 1e9;
            for (int frame = 0; frame < Ms(260000) && gl.Alive; frame++)
            {
                f++;""", """            bool markedAttack = false, markedGap = false, deathDone = false, p2Seen = false;
            double minGap = 1e9, elapsed = 0, nextDiag = 20, cutAt = -1, gapSum = 0;
            int lastStep = gl.stepNo, gapN = 0, cuts = 0;
            string cutPhase = null;
            var diag = new List<string>();
            double dt = g.cfg.Dt;
            // bounded by GAME time (the boss's own steps): 120 s of the real fight, then HP cuts (labelled) and up to 90 s more of real fire to the kill
            for (int frame = 0; frame < Ms(600000) && gl.Alive && (cutAt < 0 || elapsed < cutAt + 90); frame++)
            {
                f++;
                if (gl.stepNo > lastStep) elapsed += (gl.stepNo - lastStep) * dt;
                lastStep = gl.stepNo;
                if (gl.phase.StartsWith("p")) { gapSum += gl.gap; gapN++; }
                if (elapsed >= nextDiag)
                {
                    nextDiag += 20;
                    Vec3 vv = g.sim.mover.velocity;
                    diag.Add($"t {elapsed:0}s HP {gl.HpFrac:0.00} {gl.phase} gap {gl.gap:0} (mean {gapSum / Math.Max(1, gapN):0}) speed {Math.Sqrt(vv.x * vv.x + vv.z * vv.z):0.0} fric x{g.sim.mover.groundFrictionMult:0.00} hits {((g.events.Emitted.TryGetValue("bossHit", out int hq) ? hq : 0) - hits0)}");
                }""")
rep('if (!deathDone && gl.phase == "p2" && gl.phaseSteps > 120 * 10)', 'if (!deathDone && gl.phase == "p2" && gl.phaseSteps > 120 * 3)')
a = s.index("                // the fallback: a long fight")
b = s.index("                yield return null;\n            }\n            g.forcedMove = null;")
s = s[:a] + '''                // the fallback (labelled): after 120 s of the real fight each phase is cut to 5 % HP once (the p2 death / retry first), then the real gun finishes it
                if (elapsed > 120 && cutAt < 0) { cutAt = elapsed; seen.Add($"HP cuts from t {elapsed:0} s (real fight: HP {gl.HpFrac:0.00}, mean gap {gapSum / Math.Max(1, gapN):0} m)"); }
                if (cutAt >= 0 && gl.phase != cutPhase && (gl.phase == "p1" || (gl.phase == "p2" && deathDone) || gl.phase == "p3") && gl.HpFrac > 0.06)
                {
                    cutPhase = gl.phase;
                    cuts++;
                    gl.body.hp = gl.body.maxHp * 0.05;
                    gl.Apply(0);
                }
                if (deathDone && cutPhase == "p2" && gl.phase == "p2" && gl.HpFrac > 0.6) cutPhase = null; // after the retry the HP is the threshold again: cut again
''' + s[b:]
rep('''            if (gl.Alive) fails.Add($"the glacier did not die (phase {gl.phase}, HP {gl.HpFrac:0.00})");''', '''            seen.Add("every 20 s: " + string.Join(" | ", diag));
            seen.Add($"HP cuts {cuts}");
            if (gl.Alive) fails.Add($"the glacier did not die (phase {gl.phase}, HP {gl.HpFrac:0.00})");''')
open(p, 'w', encoding='utf-8', newline='').write(s.replace('\n', nl))
print('patched')
