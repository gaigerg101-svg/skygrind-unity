# NIGHT26 builder B: the worm's level-3 behaviours into Core/Enemies/Wyrm.cs (a one-time patch: every replacement must match exactly once).
#   python Tools/night/n26b-wyrm-patch.py
p = 'Assets/Scripts/Core/Enemies/Wyrm.cs'
raw = open(p, encoding='utf-8', newline='').read()
nl = '\r\n' if '\r\n' in raw else '\n'
s = raw.replace('\r\n', '\n')

def rep(a, b):
    global s
    assert s.count(a) == 1, ('not exactly once', s.count(a), a[:80])
    s = s.replace(a, b)

# --- params ---
rep('''        public bool aimOverlapP3;

        public static WyrmParams From(GameConfig c, Dictionary<string, object> boss, bool final = false)
        {''', '''        public bool aimOverlapP3;
        // NIGHT26 B (design.n26.bosses.wyrm26; used only while n26 is on: level 3's worm): body hits x bodyMult26, PROXIMITY AGGRO (within aggroRadius m of the head or any
        // piece the head breaks its cycle and dives at the player: a tell, then a fast pass; cooldown aggroCool s, the first after aggroFirst s), a second spit lane in
        // phase 2 (spit2Delay s after each volley), the phase-3 FINALE (a ring squeeze with one marked gap: its contact hurts x finaleContact)
        public bool n26;
        public double bodyMult26 = 1, aggroRadius, aggroCool, aggroFirst, aggroSpeed, aggroOvershoot, aggroPitch, aggroYMin, aggroYMax, spit2Delay, finaleContact;

        public static WyrmParams From(GameConfig c, Dictionary<string, object> boss, bool final = false, bool n26 = false)
        {''')
rep('''            foreach (var kv in c.Obj(w + "tells"))
                if (kv.Value is double d) p.tells[kv.Key] = d;
            foreach (object o in PortData.Arr(boss["cycle"])) p.cycle.Add((string)o);''', '''            foreach (var kv in c.Obj(w + "tells"))
                if (kv.Value is double d) p.tells[kv.Key] = d;
            if (n26)
            {
                const string z = "design.n26.bosses.wyrm26.";
                p.n26 = true;
                p.bodyMult26 = c.Num(z + "body.mult");
                p.pool *= c.Num(z + "hp.mult");
                p.aggroRadius = c.Num(z + "aggro.radius");
                p.aggroCool = c.Num(z + "aggro.cooldown");
                p.aggroFirst = c.Num(z + "aggro.first");
                p.aggroSpeed = c.Num(z + "aggro.speedMult");
                p.aggroOvershoot = c.Num(z + "aggro.overshoot");
                p.aggroPitch = c.Num(z + "aggro.pitch");
                p.aggroYMin = c.Num(z + "aggro.yMin");
                p.aggroYMax = c.Num(z + "aggro.yMax");
                p.spit2Delay = c.Num(z + "spit2.delay");
                p.finaleContact = c.Num(z + "finale.contactMult");
                foreach (var kv in c.Obj(z + "tells"))
                    if (kv.Value is double d2) p.tells[kv.Key] = d2;
            }
            foreach (object o in PortData.Arr(n26 && boss.ContainsKey("cycle26") ? boss["cycle26"] : boss["cycle"])) p.cycle.Add((string)o);''')

# --- stats ---
rep('"lungeAims", "lungeReaims" };', '"lungeAims", "lungeReaims", "aggros", "aggroHits", "spits2", "finales" };')

# --- fields ---
rep('''        public string hitPart;
        string lastPart;''', '''        public string hitPart;
        string lastPart;
        // NIGHT26 B: the proximity aggro (aggroTellLeft: 0 none, > 0 telling, -1 due), the second spit lane
        int aggroCool, aggroTellLeft, spit2Left;
        Vec3 aggroTarget;
        /// <summary>The locked aggro dive's direction (arena axes, unit, x z) while it tells: the harness bot leaves the line.</summary>
        public double AggroDirX, AggroDirZ;
        public bool AggroTelling => aggroTellLeft != 0;
        public bool AggroDiving => curPat != null && curPat.name == "aggro";
        public bool FinaleOn => curPat != null && curPat.name == "finale";''')

# --- start / restore ---
rep('''            spitFire = 0;
            spitLeft = S(p.spitEvery2);
            shotCount = 0;''', '''            spitFire = 0;
            spitLeft = S(p.spitEvery2);
            aggroCool = S(p.aggroFirst);
            aggroTellLeft = spit2Left = 0;
            shotCount = 0;''')
rep('''            spitLeft = SpitSteps(ph);
            if (phase != ph) SetPhase(ph); // a death inside the beat: the beat is over, the phase it was entering starts''', '''            spitLeft = SpitSteps(ph);
            aggroCool = S(p.aggroFirst);
            aggroTellLeft = spit2Left = 0;
            if (phase != ph) SetPhase(ph); // a death inside the beat: the beat is over, the phase it was entering starts''')

# --- next pattern: the finale only in phase 3 ---
rep('''            StartPattern(p.patterns[p.cycle[cycleIndex]]);
        }

        bool MarkLive''', '''            if (p.n26 && phase != "intro" && p.cycle[cycleIndex] == "finale" && EffPhase < 3)
            {
                cycleIndex++; // the finale is phase 3's: before it the cycle wraps
                if (cycleIndex >= p.cycle.Count)
                {
                    cycleIndex = 0;
                    side = rng.Next() < 0.5 ? 0 : 1;
                }
            }
            StartPattern(p.patterns[p.cycle[cycleIndex]]);
        }

        bool MarkLive''')

# --- the gap marker's floor circle ---
rep('''                    else if (m.name == "lungeCommit")
                    {
                        if (aim) AimLunge(m.wp, false);''', '''                    else if (m.name == "finaleGap")
                    {
                        kit.telegraph.Circle(spot.x, center.y, spot.z, p.crackR * 1.6, tellSec);
                        markers.Add(new WyrmMarker { kind = "diveCrack", pos = spot, left = S(tellSec) + S(p.crackHold), total = S(tellSec) });
                    }
                    else if (m.name == "lungeCommit")
                    {
                        if (aim) AimLunge(m.wp, false);''')
rep('''                    else if (m.name == "squeeze") stats["squeezes"]++;''', '''                    else if (m.name == "squeeze") { stats["squeezes"]++; if (curPat.name == "finale") stats["finales"]++; }''')

# --- volley: lane 2 ---
rep('''        void Volley()
        {
            stats["volleys"]++;''', '''        void Volley(bool lane2 = false)
        {
            stats["volleys"]++;
            if (lane2) stats["spits2"]++;''')
rep('''                int piece = 3 + k * 2;
                if (piece >= PieceCount - 1) piece = PieceCount - 2;''', '''                int piece = 3 + k * 2 + (lane2 ? 1 : 0); // NIGHT26: the second lane fires from the ports between the first's
                if (piece >= PieceCount - 1) piece = PieceCount - 2;''')
rep('''            Resolve("spit");
        }''', '''            Resolve(lane2 ? "spit2" : "spit");
        }''')
rep('''                    spitFire = 0;
                    Volley();
                    spitLeft = SpitSteps(phase);''', '''                    spitFire = 0;
                    Volley();
                    spitLeft = SpitSteps(phase);
                    if (p.n26 && EffPhase >= 2) spit2Left = S(p.spit2Delay);''')
rep('''            // the core: a roar, then it is open for a while''', '''            // NIGHT26: the second spit lane, spit2Delay after the first volley (never inside the gap of a lethal attack)
            if (spit2Left > 0 && --spit2Left == 0 && SpitOk && stepNo - lastResolve >= S(p.resolveGap) && NextAttackIn(baseSpeed * PhaseSpeedMult) > p.resolveGap + 1e-9) Volley(true);
            // the core: a roar, then it is open for a while''')

# --- contact in the finale ---
rep('''                double dmg = head ? (Lunging ? p.contactLunge : p.contactHead) : p.contactBody;''', '''                double dmg = head ? (Lunging ? p.contactLunge : p.contactHead) : p.contactBody;
                if (p.n26 && FinaleOn) dmg *= p.finaleContact; // the squeeze closes: touching the ring hurts''')

# --- filter ---
rep('''p.headMult : p.bodyMult);
        }''', '''p.headMult : p.bodyMult * (p.n26 ? p.bodyMult26 : 1));
        }''')

# --- speed in the aggro dive ---
rep('''            double baseSpeed = p.speed * scale.speed;
            double v = baseSpeed * PhaseSpeedMult * mult * dieFactor;''', '''            if (p.n26 && AggroDiving) mult = p.aggroSpeed;
            double baseSpeed = p.speed * scale.speed;
            double v = baseSpeed * PhaseSpeedMult * mult * dieFactor;''')

# --- the aggro step, after the marks ---
rep('''            if (phase != "dying") StepMarks(baseSpeed * PhaseSpeedMult);
            body.position = ToWorld(headLocal);''', '''            if (phase != "dying") StepMarks(baseSpeed * PhaseSpeedMult);
            if (p.n26) StepAggro(baseSpeed * PhaseSpeedMult);
            body.position = ToWorld(headLocal);''')

rep('''        // --- Attacks -------------------------------------------------------------------------------------------------
        void Shards(Vec3 spot)''', '''        // --- NIGHT26: proximity aggro ----------------------------------------------------------------------------------
        bool AttackInFlight()
        {
            for (int i = 0; i < curVar.marks.Count; i++)
            {
                WyrmMarkDef m = curVar.marks[i];
                if (m.attack && tellFired[i] && !resolved[i] && MarkLive(m)) return true;
            }
            return false;
        }

        bool PlayerNear()
        {
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            for (int i = 0; i < PieceCount; i++)
                if (!buried[i] && Vec3.DistanceTo(mid, boxes[i].c) <= p.aggroRadius) return true;
            return false;
        }

        /// <summary>The player within aggroRadius of the head or any piece: the head breaks its path and dives at him. A tell (design tells.aggro, a line from the head to where he
        /// stands, locked), then a pass at aggroSpeed x the phase speed through his spot and on; the cycle then restarts the pattern it interrupted. Cooldown aggroCool s. Never
        /// while an attack of the pattern is told or about to resolve, in the finale or the dive's burrow; waits (committed, not cancelled) for the resolve gap.</summary>
        void StepAggro(double speed)
        {
            if (aggroCool > 0) aggroCool--;
            if (phase != "p1" && phase != "p2" && phase != "p3")
            {
                aggroTellLeft = 0;
                return;
            }
            if (aggroTellLeft > 0)
            {
                if (--aggroTellLeft == 0) aggroTellLeft = -1;
                return;
            }
            if (aggroTellLeft == -1)
            {
                // due: it goes once nothing else resolves inside the gap, either side
                if (stepNo - lastResolve >= S(p.resolveGap) && NextAttackIn(speed) > p.resolveGap + 1e-9 && spitFire == 0 && !AttackInFlight()) AggroCommit();
                return;
            }
            if (aggroCool > 0 || AggroDiving || FinaleOn || buried[0] || AttackInFlight() || spitFire != 0) return;
            if (NextAttackIn(speed) < p.tells["aggro"] + p.resolveGap + 0.3) return;
            if (!PlayerNear()) return;
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5 - center.x, (c.start.y + c.end.y) * 0.5 - center.y, (c.start.z + c.end.z) * 0.5 - center.z);
            aggroTarget = new Vec3(mid.x, Math.Max(p.aggroYMin, Math.Min(p.aggroYMax, mid.y)), mid.z);
            double dx = aggroTarget.x - headLocal.x, dz = aggroTarget.z - headLocal.z, len = JsMath.Hypot(dx, dz);
            if (len < 3) return; // on top of the head already: the contact rules own that
            AggroDirX = dx / len;
            AggroDirZ = dz / len;
            aggroTellLeft = S(p.tells["aggro"]);
            Vec3 at = ToWorld(headLocal);
            Tell("aggro", p.tells["aggro"], at);
            kit.telegraph.Line(at.x, center.y, at.z, AggroDirX, AggroDirZ, len + p.aggroOvershoot, p.lungeWidth, p.tells["aggro"]);
            markers.Add(new WyrmMarker { kind = "lunge", pos = at, left = aggroTellLeft + S(p.lungeHold), total = aggroTellLeft });
        }

        void AggroCommit()
        {
            aggroTellLeft = 0;
            aggroCool = S(p.aggroCool);
            stats["aggros"]++;
            Resolve("aggro");
            double half = arenaHalf > 0 ? arenaHalf - p.aimClamp : 1e9;
            double ox = aggroTarget.x + AggroDirX * p.aggroOvershoot, oz = aggroTarget.z + AggroDirZ * p.aggroOvershoot;
            ox = Math.Max(-half, Math.Min(half, ox));
            oz = Math.Max(-half, Math.Min(half, oz));
            var v = new WyrmVariant();
            v.wp.Add(aggroTarget);
            v.wp.Add(new Vec3(ox, Math.Max(p.aggroYMin, aggroTarget.y), oz));
            var pat = new WyrmPattern { name = "aggro", pitch = p.aggroPitch, spit = false };
            pat.variants.Add(v);
            kit.telegraph.Flash(body.position.x, body.position.y, body.position.z, p.flash);
            cycleIndex--; // the pass ends and NextPattern runs the pattern it interrupted again from its start (the cycle is rejoined)
            StartPattern(pat);
        }

        // --- Attacks -------------------------------------------------------------------------------------------------
        void Shards(Vec3 spot)''')

open(p, 'w', encoding='utf-8', newline='').write(s.replace('\n', nl))
print('patched')
