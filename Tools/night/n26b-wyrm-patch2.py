# NIGHT26 builder B: the finale gap's public read for the harness bot (one-time patch on Core/Enemies/Wyrm.cs).
p = 'Assets/Scripts/Core/Enemies/Wyrm.cs'
s = open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in s else '\n'
s = s.replace('\r\n', '\n')
def rep(a, b):
    global s
    assert s.count(a) == 1, a[:60]
    s = s.replace(a, b)
a = '        public bool FinaleOn => curPat != null && curPat.name == "finale";'
rep(a, a + '''
        /// <summary>The finale's marked gap (arena-local, the tell showed it): set when its tell fires, cleared with the pattern. The harness bot leaves the ring through it.</summary>
        public Vec3? FinaleGapLocal;''')
b = '''                    else if (m.name == "finaleGap")
                    {'''
rep(b, b + '''
                        FinaleGapLocal = curVar.wp[m.wp];''')
c = '''            curPat = pat;
            curVar = pat.variants[Math.Min(side, pat.variants.Count - 1)];'''
rep(c, c + '''
            FinaleGapLocal = null;''')
open(p, 'w', encoding='utf-8', newline='').write(s.replace('\n', nl))
print('patched')
