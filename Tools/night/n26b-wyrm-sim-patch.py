# NIGHT26 builder B: the sim side of level 3's worm (SurvivalSim.Wyrm24.cs): a one-time patch, every replacement must match exactly once.
p = 'Assets/Scripts/Game/SurvivalSim.Wyrm24.cs'
raw = open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in raw else '\n'
s = raw.replace('\r\n', '\n')

def rep(a, b):
    global s
    assert s.count(a) == 1, a[:60]
    s = s.replace(a, b)

rep('''        public int wyrm24Retries;
''', '''        public int wyrm24Retries;
        /// <summary>NIGHT26 B: wyrm24 was built as level 3's worm (design.n26.bosses.wyrm26: its own paths and wall-ride arena, proximity aggro, body x0.5, the finale).</summary>
        public bool wyrm24Is26;
        /// <summary>Level 3's worm is wanted now: the n26 level3 block is on this run and its files are configured (the harness forces n26Override).</summary>
        public bool Wyrm26On => !wyrmFinal && n26.level3 && cfg.Has("design.n26.bosses.wyrm26.paths");
''')
rep('''            string paths = Path.Combine(Wyrm24Dir, cfg.Str(pfx + ".paths")), arena = Path.Combine(Wyrm24Dir, cfg.Str(pfx + ".arena"));
            if (!File.Exists(paths) || !File.Exists(arena)) return null;
            double[] o = cfg.Nums(pfx + ".room.origin");''', '''            bool w26 = Wyrm26On; // NIGHT26 B: level 3's worm reads its own paths and arena (the room's origin and salt stay the worm's)
            string paths = Path.Combine(Wyrm24Dir, cfg.Str(w26 ? "design.n26.bosses.wyrm26.paths" : pfx + ".paths")), arena = Path.Combine(Wyrm24Dir, cfg.Str(w26 ? "design.n26.bosses.wyrm26.arena" : pfx + ".arena"));
            if (!File.Exists(paths) || !File.Exists(arena)) return null;
            wyrm24Is26 = w26;
            double[] o = cfg.Nums(pfx + ".room.origin");''')
rep('WyrmParams.From(cfg, Wyrm.LoadBoss(paths), wyrmFinal)', 'WyrmParams.From(cfg, Wyrm.LoadBoss(paths), wyrmFinal, w26)')
rep('''            if ((!final && !n24.wyrm) || bossKit == null || Wyrm24Ensure() == null) return false;''', '''            if (wyrm24 != null && !wyrmFinal && !wyrm24.Alive && wyrm24Is26 != Wyrm26On)
            {
                // NIGHT26 B: the n26 level3 switch changed since the worm was built: build the other one
                enemies.all.Remove(wyrm24.body);
                world.SetLayer("wyrm", null);
                wyrm24 = null;
                wyrm24Arena = null;
            }
            if ((!final && !n24.wyrm) || bossKit == null || Wyrm24Ensure() == null) return false;''')
open(p, 'w', encoding='utf-8', newline='').write(s.replace('\n', nl))
print('patched')
