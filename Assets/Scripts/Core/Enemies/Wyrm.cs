// NIGHT24 (builder B): THE KILN WYRM, layer 1's boss (GG 2026-10-08: "a crazy worm boss ... slithers really easy though and is
// really fast"). DESIGN24-bosses.md section 1 is the design; Design/bosses/wyrm.json holds its paths and design.n24.wyrm in
// Design/unity-config.json every number (each group with a `_why`). A new module: the web has no worm. Fixed 120 Hz step, timers in whole
// steps (stepsFor), the run's seeded Mulberry32 (NewRun), no PhysX, no audio.
//
// WHAT IT IS: 26 oriented boxes (head 3 x 3 x 4 m, 24 body pieces 2.5 m tapering to 1.2 m, a tail) on a ring buffer of the head's positions,
// one per step: piece i sits at the buffer entry i x spacingSteps back, so the body follows the head's exact trail (deterministic, no
// physics). One shared HP pool: a body hit x1, the head x2, the exposed core x3 (nothing is wasted). The pieces are NOT solid to the
// player (BoxLayer is axis-aligned; no collider change): a shot is a ray tested against each oriented box here (Raycast / TakeHit), and
// the player takes contact damage plus a push from a piece's box (body hurts little, the head hurts, the lunge's head hurts most).
//
// THE CYCLE (a fixed learnable loop of four patterns, waypoints in Design/bosses/wyrm.json, Catmull-Rom smoothed, pitch clamped):
//   SPIRAL COIL around the room, rising 1 m a lap (phase 3: the squeeze closes the radius)  ->  DIVE (a floor crack, then it burrows and
//   erupts from a marked wall spot 1 s after the mark shows, 5 shards)  ->  LUNGE (a line shows 0.8 s ahead; a straight fast pass at
//   slide height)  ->  LOOP (a vertical loop through the room's middle). Phase 2 (66 %) +25 % speed, the spit barrage and the core's
//   exposure after a roar; phase 3 (33 %) +25 % again, a faster barrage, the squeeze. Every attack has a tell of at least 0.5 s and no two
//   attacks resolve inside tells.resolveGap (0.5 s). The BossKit gives the intro card, the phase beats, the finisher and the bar.
//
// ENTRY API (layer 1's builder L integrates it; Bulwark.cs is the template):
//   var wyrm = new Wyrm(cfg, WyrmParams.From(cfg, bossJson), events, world, target, nextId);   // bossJson: Wyrm.LoadBoss(path)
//   kit.bosses["wyrm"] = wyrm;  kit.Start("wyrm") or wyrm.StartFight(new BossFight { center = <arena centre, world>, kit = kit, scale = BossScale.One });
//   wyrm.NewRun(seed);                    // once per run, before StartFight: the seeded stream (side per cycle, the barrage's angle)
//   wyrm.Step(dt) every fixed step        // after the player moved (reads target.position(), target.health, target.impulse)
//   wyrm.TakeHit(origin, dir, amount, weapon, out dist)   // a hitscan shot (the caller stops the ray at the room's walls first)
//   wyrm.Raycast(origin, dir)             // the nearest piece's distance (-1: none); wyrm.body is the registry's Enemy (WyrmBody)
//   wyrm.playerBody = () => mover.Body;   // optional: the player's capsule (a slide is shorter); default: target.position() + body.capsuleHeight
//   View reads: boxes[i] / buried[i] / PieceCount, shots, markers, phase, CoreOpen, CoreTelling, Bar().
// Arena coordinates in the files are ARENA-LOCAL; `center` (the fight's ctx.center) is where the room's origin sits in the world.
// Events (the web's names, BossEvent): bossRise, bossPhase, bossTell {name, seconds}, bossHit {damage, part, core, hp}, bossPart {part
// core, reason exposed | closed}; the kit's bossIntro, bossBeat, bossDying, bossDown.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class WyrmMarkDef
    {
        public int wp, phase;
        public string name, tell;
        public bool attack, shards;
    }

    public sealed class WyrmZoneDef
    {
        public int from, to;
        public bool lunge;
    }

    public sealed class WyrmVariant
    {
        public readonly List<Vec3> wp = new List<Vec3>(), wp3 = new List<Vec3>();
        public readonly List<WyrmMarkDef> marks = new List<WyrmMarkDef>();
        public readonly List<WyrmZoneDef> zones = new List<WyrmZoneDef>();
    }

    public sealed class WyrmPattern
    {
        public string name;
        public double pitch;
        public bool spit;
        public readonly List<WyrmVariant> variants = new List<WyrmVariant>();
    }

    /// <summary>design.n24.wyrm and Design/bosses/wyrm.json, read once.</summary>
    public sealed class WyrmParams
    {
        public int pieces, spacingSteps;
        public double headW, headH, headLen, bodyW0, bodyW1, overlap, buriedY;
        public double pool, bodyMult, headMult, coreMult, phase2At, phase3At;
        /// <summary>NIGHT25 W: the ELDER WYRM (design.n25.bosses.final) has a fourth phase at this HP share (0: three phases); its name on the bar.</summary>
        public double phase4At;
        public string name = "KILN WYRM";
        /// <summary>The intro card's name when it differs from the kit's name for the key (the ELDER WYRM: design.n25.bosses.final.introName); null: the kit's.</summary>
        public string introName;
        public double speed, phaseMult, lungeMult, dieSlow;
        public double contactBody, contactHead, contactLunge, contactCooldown, pushBody, pushHead, pushUp;
        public readonly Dictionary<string, double> tells = new Dictionary<string, double>();
        public double resolveGap;
        public int spitCount, spitAimedEvery;
        public double spitEvery2, spitEvery3, spitSpeed, spitDamage, spitLife, spitRadius, spitGolden;
        public int shardCount;
        public double shardSpeed, shardDamage, shardSpread, shardLife;
        public double coreR, coreUp, coreBack, coreEvery, coreEvery3, coreOpen, coreOpen3;
        public double spawnClear;
        public bool tailShed;
        public int shedKeep;
        public double tailW, sinkTaper, dieTime, dieEase, dieSink, shardUp, shardDown;
        public double crackR, exitR, exitOffset, lungeWidth, crackHold, lungeHold, flash, volleyFlash;
        public int shedPieces;
        public double shedHp, shedBonus, shedRegrow;
        public readonly List<string> cycle = new List<string>();
        public readonly Dictionary<string, WyrmPattern> patterns = new Dictionary<string, WyrmPattern>();
        public WyrmPattern emerge;
        public readonly List<Vec3> exits = new List<Vec3>(), holes = new List<Vec3>();
        public double capsuleHeight, capsuleRadius;
        // NIGHT25 W (design.n25.wyrmAim; used only while Wyrm.aim is on)
        public int aimSpitEvery;
        public double aimLeadMax, aimVelMax, aimReaimDeg, aimClamp, aimHpMult = 1;
        public double[] aimGapCut = new double[3];
        public bool aimOverlapP3;
        // NIGHT26 B (design.n26.bosses.wyrm26; used only while n26 is on: level 3's worm): body hits x bodyMult26, PROXIMITY AGGRO (within aggroRadius m of the head or any
        // piece the head breaks its cycle and dives at the player: a tell, then a fast pass; cooldown aggroCool s, the first after aggroFirst s), a second spit lane in
        // phase 2 (spit2Delay s after each volley), the phase-3 FINALE (a ring squeeze with one marked gap: its contact hurts x finaleContact)
        public bool n26;
        public double bodyMult26 = 1, aggroRadius, aggroCool, aggroFirst, aggroSpeed, aggroOvershoot, aggroPitch, aggroYMin, aggroYMax, spit2Delay, finaleContact;

        public static WyrmParams From(GameConfig c, Dictionary<string, object> boss, bool final = false, bool n26 = false)
        {
            const string w = "design.n24.wyrm.";
            var p = new WyrmParams
            {
                pieces = c.Int(w + "body.pieces"), spacingSteps = c.Int(w + "body.spacingSteps"), headW = c.Num(w + "body.headW"), headH = c.Num(w + "body.headH"),
                headLen = c.Num(w + "body.headLen"), bodyW0 = c.Num(w + "body.bodyW0"), bodyW1 = c.Num(w + "body.bodyW1"), overlap = c.Num(w + "body.overlap"),
                buriedY = c.Num(w + "body.buriedY"),
                pool = c.Num(w + "hp.pool"), bodyMult = c.Num(w + "hp.bodyMult"), headMult = c.Num(w + "hp.headMult"), coreMult = c.Num(w + "hp.coreMult"),
                phase2At = c.Num(w + "hp.phase2At"), phase3At = c.Num(w + "hp.phase3At"),
                speed = c.Num(w + "speed.base"), phaseMult = c.Num(w + "speed.phaseMult"), lungeMult = c.Num(w + "speed.lungeMult"), dieSlow = c.Num(w + "speed.dieSlow"),
                contactBody = c.Num(w + "contact.body"), contactHead = c.Num(w + "contact.head"), contactLunge = c.Num(w + "contact.lunge"),
                contactCooldown = c.Num(w + "contact.cooldown"), pushBody = c.Num(w + "contact.pushBody"), pushHead = c.Num(w + "contact.pushHead"), pushUp = c.Num(w + "contact.pushUp"),
                resolveGap = c.Num(w + "tells.resolveGap"),
                spitCount = c.Int(w + "spit.count"), spitAimedEvery = c.Int(w + "spit.aimedEvery"), spitEvery2 = c.Num(w + "spit.every2"), spitEvery3 = c.Num(w + "spit.every3"),
                spitSpeed = c.Num(w + "spit.speed"), spitDamage = c.Num(w + "spit.damage"), spitLife = c.Num(w + "spit.life"), spitRadius = c.Num(w + "spit.radius"),
                spitGolden = c.Num(w + "spit.golden"),
                shardCount = c.Int(w + "shards.count"), shardSpeed = c.Num(w + "shards.speed"), shardDamage = c.Num(w + "shards.damage"), shardSpread = c.Num(w + "shards.spreadDeg"),
                shardLife = c.Num(w + "shards.life"),
                coreR = c.Num(w + "core.r"), coreUp = c.Num(w + "core.up"), coreBack = c.Num(w + "core.back"), coreEvery = c.Num(w + "core.every"), coreEvery3 = c.Num(w + "core.every3"),
                coreOpen = c.Num(w + "core.open"), coreOpen3 = c.Num(w + "core.open3"),
                spawnClear = c.Num(w + "intro.spawnClear"),
                tailW = c.Num(w + "body.tailW"), sinkTaper = c.Num(w + "body.sinkTaper"), dieTime = c.Num(w + "speed.dieTime"), dieEase = c.Num(w + "speed.dieEase"),
                dieSink = c.Num(w + "speed.dieSink"), shardUp = c.Num(w + "shards.up"), shardDown = c.Num(w + "shards.down"), shedKeep = c.Int(w + "tailShed.keep"),
                crackR = c.Num(w + "marks.crackR"), exitR = c.Num(w + "marks.exitR"), exitOffset = c.Num(w + "marks.exitOffset"), lungeWidth = c.Num(w + "marks.lungeWidth"),
                crackHold = c.Num(w + "marks.crackHold"), lungeHold = c.Num(w + "marks.lungeHold"), flash = c.Num(w + "marks.flash"), volleyFlash = c.Num(w + "marks.volleyFlash"),
                tailShed = !N24Blocks.ForceOff && c.Design("n24.wyrm.tailShed.enabled"), shedPieces = c.Int(w + "tailShed.pieces"), shedHp = c.Num(w + "tailShed.hp"),
                shedBonus = c.Num(w + "tailShed.bonus"), shedRegrow = c.Num(w + "tailShed.regrow"),
                capsuleHeight = c.Num("body.capsuleHeight"), capsuleRadius = c.Num("body.capsuleRadius"),
            };
            const string a = "design.n25.wyrmAim.";
            if (c.Has(a + "spit.aimedEvery"))
            {
                p.aimSpitEvery = c.Int(a + "spit.aimedEvery");
                p.aimLeadMax = c.Num(a + "spit.leadMax");
                p.aimVelMax = c.Num(a + "spit.velMax");
                p.aimReaimDeg = c.Num(a + "lunge.reaimMaxDeg");
                p.aimClamp = c.Num(a + "lunge.clamp");
                p.aimGapCut = new[] { c.Num(a + "gapCut.p1"), c.Num(a + "gapCut.p2"), c.Num(a + "gapCut.p3") };
                p.aimOverlapP3 = c.Design("n25.wyrmAim.overlap.p3");
                p.aimHpMult = c.Num(a + "hp.mult");
            }
            if (final)
            {
                // NIGHT25 W: THE ELDER KILN WYRM: the same worm, harder (every number from design.n25.bosses.final)
                const string fz = "design.n25.bosses.final.";
                p.pool *= c.Num(fz + "hpMult");
                p.speed *= c.Num(fz + "speedMult");
                p.spitDamage *= c.Num(fz + "spitDamageMult");
                p.phaseMult = c.Num(fz + "phaseMult"); // each phase is this much faster than the last (the worm: 1.25); four phases, so a gentler step
                p.tailShed = true;
                List<object> ph = c.Arr(fz + "phases");
                p.phase2At = PortData.Num(ph[0]);
                p.phase3At = PortData.Num(ph[1]);
                p.phase4At = ph.Count > 2 ? PortData.Num(ph[2]) : 0;
                p.name = c.Str(fz + "name");
                p.introName = c.Str(fz + "introName");
            }
            foreach (var kv in c.Obj(w + "tells"))
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
            foreach (object o in PortData.Arr(n26 && boss.ContainsKey("cycle26") ? boss["cycle26"] : boss["cycle"])) p.cycle.Add((string)o);
            p.emerge = Pattern("emerge", PortData.Obj(boss["emerge"]));
            foreach (var kv in PortData.Obj(boss["patterns"])) p.patterns[kv.Key] = Pattern(kv.Key, PortData.Obj(kv.Value));
            foreach (object o in PortData.Arr(boss["exits"])) p.exits.Add(PortData.V3(o));
            foreach (object o in PortData.Arr(boss["holes"])) p.holes.Add(PortData.V3(o));
            return p;
        }

        static WyrmPattern Pattern(string name, Dictionary<string, object> j)
        {
            var pat = new WyrmPattern { name = name, pitch = PortData.Num(j["pitch"]), spit = j["spit"] is bool b && b };
            foreach (object vo in PortData.Arr(j["variants"]))
            {
                var vj = PortData.Obj(vo);
                var v = new WyrmVariant();
                foreach (object o in PortData.Arr(vj["wp"])) v.wp.Add(PortData.V3(o));
                if (vj.TryGetValue("wp3", out object w3))
                    foreach (object o in PortData.Arr(w3)) v.wp3.Add(PortData.V3(o));
                foreach (object o in PortData.Arr(vj["marks"]))
                {
                    var m = PortData.Obj(o);
                    v.marks.Add(new WyrmMarkDef
                    {
                        wp = (int)PortData.Num(m["wp"]), name = (string)m["name"], tell = (string)m["tell"], attack = m["attack"] is bool a && a,
                        shards = m.TryGetValue("shards", out object sh) && sh is bool s && s, phase = m.TryGetValue("phase", out object ph) ? (int)PortData.Num(ph) : 0,
                    });
                }
                foreach (object o in PortData.Arr(vj["zones"]))
                {
                    var z = PortData.Obj(o);
                    v.zones.Add(new WyrmZoneDef { from = (int)PortData.Num(z["from"]), to = (int)PortData.Num(z["to"]), lunge = z["lunge"] is bool l && l });
                }
                pat.variants.Add(v);
            }
            return pat;
        }
    }

    /// <summary>The boss as the enemies' registry sees it: the shots' target (RayHit finds the nearest piece; Damage takes the HP).</summary>
    public sealed class WyrmBody : Enemy
    {
        public readonly Wyrm system;

        public WyrmBody(Wyrm system)
        {
            this.system = system;
            kind = "wyrm";
        }

        public override double Height => 3;
        public override double Radius => 3;
        public override double AimHeight => 1.5;

        public override double RayHit(Vec3 o, Vec3 d, out bool headshot)
        {
            double t = system.Raycast(o, d);
            headshot = t >= 0 && (system.hitPart == "head" || system.hitPart == "core");
            return t;
        }

        public override bool Damage(double amount, double flashTime)
        {
            system.Apply(amount);
            return false;
        }

        public override void Knockback(double dirX, double dirZ, double speed, double time, double dt) { }
        public override void Stagger(double time, double dt) { }
        public override void Stun(double time, double dt) { }
    }

    /// <summary>A spit or a shard in flight.</summary>
    public sealed class WyrmShot
    {
        public Vec3 pos, vel;
        public int life;
        public double damage;
        public bool aimed, shard;
    }

    /// <summary>A telegraph marker for the view: the floor crack, the wall spot an eruption comes from, the lunge's start.</summary>
    public sealed class WyrmMarker
    {
        public string kind;
        public Vec3 pos;
        public int left, total;
    }

    /// <summary>One line of the fight's attack log: a tell (seconds) or an attack's resolve, at a step.</summary>
    public struct WyrmLog
    {
        public int step;
        public string name;
        public double seconds;
        public bool resolve;
    }

    public sealed class Wyrm : IBossSystem
    {
        public readonly WyrmParams p;
        public readonly WyrmBody body;
        readonly EventBus events;
        readonly ICollisionWorld world;
        readonly BossTarget target;
        readonly Func<int> nextId;
        readonly double dt0;
        readonly BossEvent said = new BossEvent();
        Mulberry32 rng = new Mulberry32(1);
        BossKit kit;
        BossScale scale = BossScale.One;
        Vec3 center;

        // the body: a ring buffer of the head's (arena-local) positions
        readonly Vec3[] buf;
        readonly int mask;
        int bufHead;
        Vec3 headLocal, headDir = new Vec3(0, 1, 0);
        public readonly WyrmBox[] boxes;
        public readonly bool[] buried;
        /// <summary>Each piece's centre one step ago (the view's render interpolation).</summary>
        public readonly Vec3[] prevCenters;
        /// <summary>Where the room's origin sits in the world, when it is not the fight context's centre (layer 1 places the room).</summary>
        public Vec3? arenaCenter;

        // the path
        WyrmPath path;
        WyrmPattern curPat;
        WyrmVariant curVar;
        double s;
        int cycleIndex, side;
        bool[] tellFired, resolved;
        int[] tellAt;
        const double PathDs = 0.5;
        int builtPhase = 1;

        public string phase = "dead";
        string nextPhase;
        public int stepNo, phaseSteps, contactLeft;
        int beatSteps, dieSteps, spitLeft, spitFire, coreLeft, coreTell, coreOpenLeft, shotCount, lastResolve = -100000;
        double spitAngle;
        public bool Lunging;
        /// <summary>NIGHT25 W: design.n25.wyrmAim is on for this run (the sim sets it before the fight): spit leads, the lunge locks and re-aims, less breathing room, phase-3 overlap.</summary>
        public bool aim;
        /// <summary>Half the room's width (m, arena-local): the lunge's aimed end is cut short of a wall; 0: no clamp.</summary>
        public double arenaHalf;
        IList<Vec3> liveWps;
        int wpOff;
        bool[] lungeReaimed;
        Vec3 playerVel, lastPlayerPos;
        bool havePlayerPos;
        public string hitPart;
        string lastPart;
        // NIGHT26 B: the proximity aggro (aggroTellLeft: 0 none, > 0 telling, -1 due), the second spit lane
        int aggroCool, aggroTellLeft, spit2Left;
        Vec3 aggroTarget;
        /// <summary>The locked aggro dive's direction (arena axes, unit, x z) while it tells: the harness bot leaves the line.</summary>
        public double AggroDirX, AggroDirZ;
        public bool AggroTelling => aggroTellLeft != 0;
        public bool AggroDiving => curPat != null && curPat.name == "aggro";
        public bool FinaleOn => curPat != null && curPat.name == "finale";
        /// <summary>The finale's marked gap (arena-local, the tell showed it): set when its tell fires, cleared with the pattern. The harness bot leaves the ring through it.</summary>
        public Vec3? FinaleGapLocal;
        public readonly List<WyrmShot> shots = new List<WyrmShot>();
        public readonly List<WyrmMarker> markers = new List<WyrmMarker>();
        public readonly List<WyrmLog> log = new List<WyrmLog>();
        public readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        static readonly string[] StatNames = { "contactsBody", "contactsHead", "contactsLunge", "shotsHit", "volleys", "shards", "lunges", "dives", "squeezes", "sheds", "shedKills", "regrows", "coreOpens", "coreHits", "headHits", "bodyHits", "lungeAims", "lungeReaims", "aggros", "aggroHits", "spits2", "finales" };
        /// <summary>The player's capsule when the caller knows it better than a standing one at the target's feet (a slide is shorter).</summary>
        public Func<Capsule> playerBody;

        public Wyrm(GameConfig cfg, WyrmParams p, EventBus events, ICollisionWorld world, BossTarget target, Func<int> nextId)
        {
            this.p = p;
            this.events = events;
            this.world = world;
            this.target = target;
            this.nextId = nextId;
            dt0 = cfg.Dt;
            body = new WyrmBody(this);
            int need = (p.pieces + 2) * p.spacingSteps + 8, size = 1;
            while (size < need) size <<= 1;
            buf = new Vec3[size];
            mask = size - 1;
            boxes = new WyrmBox[p.pieces + 2];
            buried = new bool[p.pieces + 2];
            prevCenters = new Vec3[p.pieces + 2];
            goneFrom = p.pieces + 2;
            foreach (string n in StatNames) stats[n] = 0;
        }

        /// <summary>Read Design/bosses/wyrm.json.</summary>
        public static Dictionary<string, object> LoadBoss(string path) => PortData.Obj(PortData.LoadJson(path));

        int S(double sec) => MovementMath.StepsFor(sec, dt0);

        public bool Alive => body.alive;
        public bool Frozen { get; set; }
        public int Id => body.id;
        public Vec3 Center => center;
        public int PieceCount => p.pieces + 2;
        public Vec3 HeadPosition => body.position;
        public double HpFrac => body.maxHp > 0 ? body.hp / body.maxHp : 0;
        public bool CoreOpen => coreOpenLeft > 0;
        public bool CoreTelling => coreTell > 0;
        public bool SpitTelling => spitFire > 0;
        public string PatternName => curPat?.name;
        public int Side => side;
        public double PathS => s;
        /// <summary>NIGHT26 V (view only): the current pattern's path length (m), so the view can tell how near the coil's release is: (PathLength - PathS) / the head's speed.</summary>
        public double PathLength => path != null ? path.length : 0;
        static int PhaseNo(string ph) => ph == "p2" ? 2 : ph == "p3" ? 3 : ph == "p4" ? 4 : 1;

        /// <summary>The phase number the worm acts in (1-4; a beat counts as the phase it enters; the intro and the finisher read 1).</summary>
        int EffPhase => phase == "beat" ? PhaseNo(nextPhase) : PhaseNo(phase);

        public double PhaseSpeedMult
        {
            get
            {
                int n = EffPhase;
                double m = n >= 4 ? p.phaseMult * p.phaseMult * p.phaseMult : n == 3 ? p.phaseMult * p.phaseMult : n == 2 ? p.phaseMult : 1;
                return m * GapSpeed;
            }
        }

        /// <summary>NIGHT25 W: the speed that cuts the time between attacks by the phase's gapCut (1 with aim off).</summary>
        double GapSpeed => !aim ? 1 : 1 / (1 - p.aimGapCut[Math.Min(EffPhase, 3) - 1]);

        /// <summary>The spit's interval in phase ph ("p2" / "p3"), steps; with aim on, cut by the phase's gapCut.</summary>
        int SpitSteps(string ph) => S((PhaseNo(ph) >= 3 ? p.spitEvery3 : p.spitEvery2) * (aim ? 1 - p.aimGapCut[PhaseNo(ph) >= 3 ? 2 : 1] : 1));

        /// <summary>The spit barrage runs in this pattern: coil and loop; with aim on, in phase 3 also during the dive and the lunge (the overlap).</summary>
        bool SpitOk => curPat.spit || (aim && p.aimOverlapP3 && (phase == "p3" || phase == "p4"));

        public void NewRun(uint seed) => rng = new Mulberry32(seed);

        /// <summary>The phase the worm's HP is restored to on a player death (NIGHT25 W, the lead's decision: a death in the boss room does not restart the
        /// worm at full HP): "p2" or "p3", or null in the intro and phase 1 (a full restart IS the start of phase 1). A beat counts as the phase it enters.</summary>
        public string RetryPhase => !body.alive ? null : phase == "beat" ? nextPhase : phase == "p2" || phase == "p3" || phase == "p4" ? phase : null;

        /// <summary>NIGHT25 W (design.n25.wyrmAim, DESIGN24 "a checkpoint at each phase start"): a player death inside the boss room. The worm goes back to the START
        /// of its current phase: its HP at that phase's threshold, its spit, core and contact clocks as at the beat's end, the shots and marks cleared; the seed
        /// stream is NOT restarted (same stream continued) and no new intro runs. false (nothing done): the worm is not in phase 2 or 3 (the caller starts the fight).</summary>
        public bool RestorePhaseStart()
        {
            string ph = RetryPhase;
            if (ph == null) return false;
            body.hp = body.maxHp * (ph == "p4" ? p.phase4At : ph == "p3" ? p.phase3At : p.phase2At);
            shots.Clear();
            markers.Clear();
            coreOpenLeft = coreTell = 0;
            spitFire = 0;
            contactLeft = S(p.contactCooldown); // the player respawns clear of the worm: no contact in the first cooldown
            coreLeft = S(PhaseNo(ph) >= 3 ? p.coreEvery3 : p.coreEvery) / 2; // as EnterBeat
            spitLeft = SpitSteps(ph);
            aggroCool = S(aggroFirst31 >= 0 ? aggroFirst31 : p.aggroFirst);
            aggroTellLeft = spit2Left = leanLeft47 = 0;
            if (phase != ph) SetPhase(ph); // a death inside the beat: the beat is over, the phase it was entering starts
            return true;
        }

        public uint RngState
        {
            get => rng.State;
            set => rng.State = value;
        }

        Vec3 ToWorld(Vec3 v) => new Vec3(v.x + center.x, v.y + center.y, v.z + center.z);

        void SetPhase(string ph)
        {
            phase = ph;
            phaseSteps = 0;
            said.Reset("wyrm");
            said.phase = ph;
            said.hp = body.hp;
            said.maxHp = body.maxHp;
            events.Emit("bossPhase", said);
        }

        void Tell(string name, double seconds, Vec3 at)
        {
            said.Reset("wyrm");
            said.name = name;
            said.seconds = seconds;
            events.Emit("bossTell", said);
            log.Add(new WyrmLog { step = stepNo, name = name, seconds = seconds, resolve = false });
            kit.telegraph.Threat(at.x, at.y, at.z);
        }

        void Resolve(string name)
        {
            lastResolve = stepNo;
            log.Add(new WyrmLog { step = stepNo, name = name, seconds = 0, resolve = true });
        }

        /// <summary>The kit starts the fight: the worm rises out of the floor hole (no damage either way until it is out).</summary>
        public void StartFight(BossFight ctx)
        {
            kit = ctx.kit;
            scale = ctx.scale;
            center = arenaCenter ?? ctx.center;
            body.id = nextId();
            body.alive = true;
            body.maxHp = JsMath.Round(p.pool * ctx.scale.hp * (aim ? p.aimHpMult : 1));
            body.hp = body.maxHp;
            body.flash = 0;
            stepNo = 0;
            shots.Clear();
            markers.Clear();
            log.Clear();
            foreach (string n in StatNames) stats[n] = 0;
            contactLeft = 0;
            goneFrom = PieceCount;
            shedActive = shedPending = false;
            lastResolve = -100000;
            coreOpenLeft = coreTell = 0;
            coreLeft = S(p.coreEvery);
            spitFire = 0;
            spitLeft = S(p.spitEvery2);
            aggroCool = S(aggroFirst31 >= 0 ? aggroFirst31 : p.aggroFirst);
            aggroTellLeft = spit2Left = leanLeft47 = 0;
            shotCount = 0;
            spitAngle = rng.Next() * 2 * Math.PI;
            side = rng.Next() < 0.5 ? 0 : 1;
            cycleIndex = 0;
            Lunging = false;
            WyrmVariant first = p.emerge.variants[0];
            headLocal = first.wp[0];
            headDir = new Vec3(0, 1, 0);
            for (int i = 0; i < buf.Length; i++) buf[i] = headLocal;
            bufHead = 0;
            curPat = null;
            StartPattern(p.emerge);
            body.position = ToWorld(headLocal);
            body.previous = body.position;
            body.velocity = default;
            Pose();
            for (int i = 0; i < PieceCount; i++) prevCenters[i] = boxes[i].c;
            SetPhase("intro");
            said.Reset("wyrm");
            said.name = "WYRM";
            said.hp = body.maxHp;
            said.round = ctx.round;
            events.Emit("bossRise", said);
        }

        public void Clear()
        {
            body.alive = false;
            phase = "dead";
            shots.Clear();
            markers.Clear();
        }

        // --- Paths ---------------------------------------------------------------------------------------------------
        void StartPattern(WyrmPattern pat)
        {
            curPat = pat;
            curVar = pat.variants[Math.Min(side, pat.variants.Count - 1)];
            FinaleGapLocal = null;
            // phase 3: the coil's squeeze is its own waypoint list (wp3): the radius closes through the gap between two pillars
            bool p3 = EffPhase >= 3;
            builtPhase = p3 ? 3 : 1;
            List<Vec3> wps = p3 && curVar.wp3.Count > 0 ? curVar.wp3 : curVar.wp;
            path = WyrmMath.BuildPath(headLocal, headDir, wps, pat.pitch, PathDs);
            s = 0;
            liveWps = wps;
            wpOff = 0;
            int mc = curVar.marks.Count;
            lungeReaimed = new bool[mc];
            tellFired = new bool[mc];
            resolved = new bool[mc];
            tellAt = new int[mc];
        }

        void NextPattern()
        {
            if (phase == "intro")
            {
                SetPhase("p1");
                cycleIndex = 0;
            }
            else
            {
                cycleIndex++;
                if (cycleIndex >= p.cycle.Count)
                {
                    cycleIndex = 0;
                    side = rng.Next() < 0.5 ? 0 : 1;
                }
            }
            if (p.n26 && phase != "intro" && p.cycle[cycleIndex] == "finale" && EffPhase < 3)
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

        bool MarkLive(WyrmMarkDef m) => m.phase == 0 || builtPhase >= m.phase; // a phase-3 mark exists only on a pattern built (its path too) in phase 3

        /// <summary>Seconds until the pattern's next attack resolves (a huge number: none).</summary>
        double NextAttackIn(double speed)
        {
            double best = 1e9;
            for (int i = 0; i < curVar.marks.Count; i++)
            {
                WyrmMarkDef m = curVar.marks[i];
                if (!m.attack || resolved[i] || !MarkLive(m)) continue;
                double t = (WpS(m.wp) - s) / Math.Max(1e-6, speed);
                if (t < best) best = t;
            }
            return best;
        }

        void StepMarks(double baseSpeed)
        {
            for (int i = 0; i < curVar.marks.Count; i++)
            {
                WyrmMarkDef m = curVar.marks[i];
                if (resolved[i] || !MarkLive(m)) continue;
                double tellSec = p.tells[m.tell];
                double at = WpS(m.wp);
                Vec3 spot = ToWorld(curVar.wp[m.wp]);
                if (!tellFired[i] && (at - s) <= baseSpeed * tellSec)
                {
                    tellFired[i] = true;
                    tellAt[i] = stepNo;
                    Tell(m.tell, tellSec, spot);
                    if (m.name == "diveExit" || m.name == "diveCrack")
                    {
                        // the crack on the floor where it dives; for the eruption, the floor in front of the wall spot it comes out of
                        double inward = spot.x < center.x ? 1 : -1;
                        if (m.name == "diveExit") kit.telegraph.Circle(spot.x + inward * p.exitOffset, center.y, spot.z, p.exitR, tellSec);
                        else kit.telegraph.Circle(spot.x, center.y, spot.z, p.crackR, tellSec);
                        markers.Add(new WyrmMarker { kind = m.name, pos = spot, left = S(tellSec) + S(p.crackHold), total = S(tellSec) });
                    }
                    else if (m.name == "finaleGap")
                    {
                        FinaleGapLocal = curVar.wp[m.wp];
                        kit.telegraph.Circle(spot.x, center.y, spot.z, p.crackR * 1.6, tellSec);
                        markers.Add(new WyrmMarker { kind = "diveCrack", pos = spot, left = S(tellSec) + S(p.crackHold), total = S(tellSec) });
                    }
                    else if (m.name == "lungeCommit")
                    {
                        if (aim) AimLunge(m.wp, false);
                        Vec3 end = ToWorld(aim ? liveWps[m.wp + 1 - wpOff] : curVar.wp[m.wp + 1]);
                        double dx = end.x - spot.x, dz = end.z - spot.z, len = JsMath.Hypot(dx, dz);
                        kit.telegraph.Line(spot.x, center.y, spot.z, dx / len, dz / len, len, p.lungeWidth, tellSec);
                        markers.Add(new WyrmMarker { kind = "lunge", pos = spot, left = S(tellSec) + S(p.lungeHold), total = S(tellSec) });
                    }
                }
                if (s >= at)
                {
                    resolved[i] = true;
                    if (m.attack) Resolve(m.name);
                    if (m.name == "diveExit")
                    {
                        stats["dives"]++;
                        kit.telegraph.Flash(spot.x, spot.y, spot.z, p.flash);
                        if (m.shards) Shards(spot);
                    }
                    else if (m.name == "lungeCommit")
                    {
                        stats["lunges"]++;
                        kit.telegraph.Flash(spot.x, spot.y, spot.z, p.flash);
                    }
                    else if (m.name == "squeeze") { stats["squeezes"]++; if (curPat.name == "finale") stats["finales"]++; }
                }
            }
            if (aim)
                for (int i = 0; i < curVar.marks.Count; i++)
                {
                    WyrmMarkDef m = curVar.marks[i];
                    if (m.name != "lungeCommit" || !resolved[i] || lungeReaimed[i] || !MarkLive(m) || m.wp < wpOff || m.wp + 1 - wpOff >= path.wpS.Length) continue;
                    // the midpoint of the lunge: halfway along the arc between the commit waypoint and the line's end
                    if (s < (WpS(m.wp) + WpS(m.wp + 1)) * 0.5) continue;
                    lungeReaimed[i] = true;
                    AimLunge(m.wp, true);
                }
        }

        /// <summary>The arc length at waypoint i of the live path (the path is rebuilt from the head when a lunge is aimed: wpOff waypoints are behind it).</summary>
        double WpS(int i) => path.wpS[i - wpOff];

        /// <summary>NIGHT25 W: aim the lunge whose start waypoint is k. At the tell's start (reaim false) its line points at where the player stands, the NIGHT24 length kept;
        /// at the midpoint (reaim true) the remaining line turns at most aimReaimDeg toward the player, once. The path is rebuilt from the head through the new end.</summary>
        void AimLunge(int k, bool reaim)
        {
            Vec3 from = reaim ? headLocal : liveWps[k - wpOff];
            Vec3 end0 = liveWps[k + 1 - wpOff];
            Vec3 pw = target.position();
            Vec3 pl = new Vec3(pw.x - center.x, 0, pw.z - center.z);
            double tx = pl.x - from.x, tz = pl.z - from.z, tl = JsMath.Hypot(tx, tz);
            double ex = end0.x - from.x, ez = end0.z - from.z, len = JsMath.Hypot(ex, ez);
            if (tl < 1e-6 || len < 1e-6) return;
            tx /= tl;
            tz /= tl;
            if (reaim)
            {
                double ux = ex / len, uz = ez / len;
                double dot = ux * tx + uz * tz, cross = ux * tz - uz * tx, max = p.aimReaimDeg * Math.PI / 180;
                if (dot < JsMath.Cos(max))
                {
                    double sn = JsMath.Sin(max), cs = JsMath.Cos(max), sg = cross < 0 ? -1 : 1;
                    tx = ux * cs - uz * sn * sg;
                    tz = ux * sn * sg + uz * cs;
                }
            }
            double ax = from.x + tx * len, az = from.z + tz * len;
            if (arenaHalf > 0)
            {
                double lim = arenaHalf - p.aimClamp, k2 = 1;
                if (Math.Abs(ax) > lim) k2 = Math.Min(k2, Math.Abs(lim - Math.Abs(from.x)) / Math.Max(1e-6, Math.Abs(ax - from.x)));
                if (Math.Abs(az) > lim) k2 = Math.Min(k2, Math.Abs(lim - Math.Abs(from.z)) / Math.Max(1e-6, Math.Abs(az - from.z)));
                ax = from.x + (ax - from.x) * k2;
                az = from.z + (az - from.z) * k2;
            }
            var aimed = new Vec3(ax, end0.y, az);
            // the waypoints still ahead of the head, the lunge's end replaced
            int j = wpOff;
            while (j - wpOff < path.wpS.Length && path.wpS[j - wpOff] <= s) j++;
            var rest = new List<Vec3>();
            for (int idx = j; idx - wpOff < liveWps.Count; idx++) rest.Add(idx == k + 1 ? aimed : liveWps[idx - wpOff]);
            stats[reaim ? "lungeReaims" : "lungeAims"]++;
            path = WyrmMath.BuildPath(headLocal, headDir, rest, curPat.pitch, PathDs);
            s = 0;
            liveWps = rest;
            wpOff = j;
        }

        // --- NIGHT26: proximity aggro ----------------------------------------------------------------------------------
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
        /// <summary>NIGHT47 B wyrm47 (SurvivalSim.N47.Wyrm.cs sets it while the row is on; false = NIGHT46's rule): no proximity charge starts
        /// while the DIVE pattern runs, so the burrow (the crack, the dive, the travel under the floor, the eruption) is never cut and replayed.</summary>
        public bool noAggroInDive47;
        /// <summary>NIGHT47 B wyrm47 (false = NIGHT46's rule): after a proximity charge that cut the COIL the cycle goes on to the NEXT pattern
        /// instead of replaying the coil (measured: with the charges every 5 s the coil restarted 226 times in 10 fights and held 72 % of the fight;
        /// advancing after every pattern instead cut the bot's staggers 18 -> 3-13 in 10 fights).</summary>
        public bool advanceAfterAggro47;
        /// <summary>NIGHT47 B wyrm47 (0 = NIGHT46's rule: the charge's aim is fixed at its tell's start): the aim follows the player until this many
        /// seconds of the tell are left. A longer tell with a fixed aim charges where you were, not where you are.</summary>
        public double aggroLock47;
        /// <summary>NIGHT47 B wyrm47 (G33, "a weak spot reachable from the air"; 0 = NIGHT46: the core's x coreMult from anywhere): a core hit from a
        /// ray whose origin is under the core's centre + coreAboveShare47 x its radius lands x coreBelowMult47 instead (from above: coreMult).</summary>
        public double coreBelowMult47, coreAboveShare47;
        /// <summary>NIGHT47 B wyrm47 (0 = NIGHT46): THE LEAN, seconds the head rears and tracks you before the charge's own tell (the lead's round-3
        /// idea: the 0.5 s tell keeps the punch timing, the warning grows to lean + tell); Leaning47 for the view.</summary>
        public double aggroLean47;
        /// <summary>NIGHT47 B wyrm47 (G34, the lead's round-3 line "shorten the coil's length instead of skipping it"; 0 = NIGHT46): the COIL ends
        /// at this share of its path in phases 1-2, so the cycle reaches the DIVE sooner.</summary>
        public double coilCut47;
        int leanLeft47;
        public bool Leaning47 => leanLeft47 > 0;
        bool coreFromAbove47;
        /// <summary>NIGHT48 B kilnBoost48 (G33, the lead's line "the core's x3 only from an island or mid-boost"; null = NIGHT47's rule): the sim's
        /// test of the shooter right now; false -> an open-core hit lands x coreElse48 instead of x coreMult.</summary>
        public Func<bool> coreFull48;
        public double coreElse48 = 1;
        /// <summary>NIGHT48 B wyrmDive48 (round 3, the worm's min with wyrmTell48 on too; 0 = NIGHT47): seconds added to each phase beat (the 66 % /
        /// 33 % transitions, no damage lands), the same length added to every fight, the pool unchanged.</summary>
        public double beatExtra48;

        /// <summary>NIGHT48 B wyrmDive48 (G34, the lead's line "the burrow as the punish: after a stagger the worm always dives"; nothing calls it with the
        /// row off): the DIVE pattern starts now from where the head is (its crack tell, the dive, the travel under the floor, the eruption); the cycle
        /// then goes on from where it was. false (nothing started; the sim asks again next step): not in a fighting phase, the finale, or the head
        /// already diving, charging (an aggro pass or its tell) or recoiling.</summary>
        public bool Dive48()
        {
            if (!body.alive || !p.patterns.ContainsKey("dive")) return false;
            if (phase != "p1" && phase != "p2" && phase != "p3") return false;
            if (FinaleOn || AggroTelling || leanLeft47 > 0) return false;
            if (curPat != null && (curPat.name == "dive" || curPat.name == "aggro" || curPat.name == "feint31" || curPat.name == "recoil30")) return false;
            Lunging = false;
            StartPattern(p.patterns["dive"]);
            return true;
        }

        void StepAggro(double speed)
        {
            if (aggroCool > 0) aggroCool--;
            if (phase != "p1" && phase != "p2" && phase != "p3")
            {
                aggroTellLeft = leanLeft47 = 0;
                return;
            }
            if (leanLeft47 > 0)
            {
                // NIGHT47 B wyrm47: THE LEAN (aggroLean47 s before the charge's own tell): the head rears and glows, tracking you; then the
                // NIGHT46 tell (0.5 s, locked) runs as before, so the punch timing is today's and the whole warning is lean + tell
                Retarget47();
                if (--leanLeft47 > 0) return;
                // the lean always runs into the tell (its conditions were met when it began; a lean dropped for an attack that started under it
                // cost the human pilot its punch windows: 15 / 40 killed)
                StartAggroTell(Math.Max(3, JsMath.Hypot(aggroTarget.x - headLocal.x, aggroTarget.z - headLocal.z)));
                return;
            }
            if (aggroTellLeft > 0)
            {
                if (aggroLock47 > 0 && aggroTellLeft > S(aggroLock47)) Retarget47(); // NIGHT47 B wyrm47: the aim follows you until aggroLock47 s are left
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
            if (noAggroInDive47 && curPat != null && curPat.name == "dive") return; // NIGHT47 B wyrm47: the burrow is never cut by a charge (G34)
            if (NextAttackIn(speed) < p.tells["aggro"] + aggroLean47 + p.resolveGap + 0.3) return; // (NIGHT47: + the lean, 0 = NIGHT46)
            if (!PlayerNear()) return;
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5 - center.x, (c.start.y + c.end.y) * 0.5 - center.y, (c.start.z + c.end.z) * 0.5 - center.z);
            aggroTarget = new Vec3(mid.x, Math.Max(p.aggroYMin, Math.Min(p.aggroYMax, mid.y)), mid.z);
            double dx = aggroTarget.x - headLocal.x, dz = aggroTarget.z - headLocal.z, len = JsMath.Hypot(dx, dz);
            if (len < 3) return; // on top of the head already: the contact rules own that
            AggroDirX = dx / len;
            AggroDirZ = dz / len;
            if (aggroLean47 > 0)
            {
                leanLeft47 = Math.Max(1, S(aggroLean47)); // NIGHT47 B wyrm47: the lean first
                Tell("aggroLean", aggroLean47, ToWorld(headLocal));
                return;
            }
            StartAggroTell(len);
        }

        /// <summary>The proximity charge's tell (NIGHT46's lines, moved here so NIGHT47's lean can start it when it ends).</summary>
        void StartAggroTell(double len)
        {
            aggroTellLeft = S(p.tells["aggro"]);
            Vec3 at = ToWorld(headLocal);
            Tell("aggro", p.tells["aggro"], at);
            kit.telegraph.Line(at.x, center.y, at.z, AggroDirX, AggroDirZ, len + p.aggroOvershoot, p.lungeWidth, p.tells["aggro"]);
            markers.Add(new WyrmMarker { kind = "lunge", pos = at, left = aggroTellLeft + S(p.lungeHold), total = aggroTellLeft });
        }

        /// <summary>NIGHT47 B wyrm47: the charge's aim set on the player now (the lean, and aggroLock47's tracking).</summary>
        void Retarget47()
        {
            Capsule pc = PlayerCapsule();
            Vec3 pm = new Vec3((pc.start.x + pc.end.x) * 0.5 - center.x, (pc.start.y + pc.end.y) * 0.5 - center.y, (pc.start.z + pc.end.z) * 0.5 - center.z);
            Vec3 t = new Vec3(pm.x, Math.Max(p.aggroYMin, Math.Min(p.aggroYMax, pm.y)), pm.z);
            double tx = t.x - headLocal.x, tz = t.z - headLocal.z, tl = JsMath.Hypot(tx, tz);
            if (tl < 3) return;
            aggroTarget = t;
            AggroDirX = tx / tl;
            AggroDirZ = tz / tl;
        }

        void AggroCommit()
        {
            aggroTellLeft = 0;
            aggroCool = S(aggroCool31 >= 0 ? aggroCool31 : p.aggroCool);
            stats["aggros"]++;
            Resolve("aggro");
            double half = arenaHalf > 0 ? arenaHalf - p.aimClamp : 1e9;
            double ox = aggroTarget.x + AggroDirX * p.aggroOvershoot, oz = aggroTarget.z + AggroDirZ * p.aggroOvershoot;
            ox = Math.Max(-half, Math.Min(half, ox));
            oz = Math.Max(-half, Math.Min(half, oz));
            var v = new WyrmVariant();
            // NIGHT31 B (design.n31.wyrm31): a FEINT stops feintShort31 m short of the player's spot with no overshoot, under its own pattern name (not "aggro": no cyan head, no punch window)
            bool feint = feintNext31 != null && feintNext31();
            if (feint)
            {
                double fl = Math.Max(0, JsMath.Hypot(aggroTarget.x - headLocal.x, aggroTarget.z - headLocal.z) - feintShort31);
                v.wp.Add(new Vec3(headLocal.x + AggroDirX * fl, aggroTarget.y, headLocal.z + AggroDirZ * fl));
            }
            else
            {
                v.wp.Add(aggroTarget);
                v.wp.Add(new Vec3(ox, Math.Max(p.aggroYMin, aggroTarget.y), oz));
            }
            var pat = new WyrmPattern { name = feint ? "feint31" : "aggro", pitch = p.aggroPitch, spit = false };
            pat.variants.Add(v);
            kit.telegraph.Flash(body.position.x, body.position.y, body.position.z, p.flash);
            if (!(advanceAfterAggro47 && curPat != null && curPat.name == "coil")) cycleIndex--; // the pass ends and NextPattern runs the pattern it interrupted again from its start (the cycle is rejoined)
            // NIGHT47 B wyrm47 (advanceAfterAggro47): an interrupted COIL is not replayed; the cycle goes on to the next pattern (the 297 m coil no longer restarts after every charge)
            StartPattern(pat);
        }

        // --- NIGHT30 B (design.n30.wyrm30): the head-punch stagger's hooks (nothing calls them with the row off) ------------------------
        double recoil30Speed;

        /// <summary>The head comes at the player now: the proximity aggro's dive or a lunge zone (the charges a head punch can stop).</summary>
        public bool Charging30 => body.alive && (AggroDiving || Lunging) && phase != "dying" && phase != "beat" && phase != "intro";
        /// <summary>The head recoils from a stagger (the "recoil30" pass).</summary>
        public bool Recoiling30 => curPat != null && curPat.name == "recoil30";

        /// <summary>The stagger (Game/SurvivalSim.Boss30.cs): the head leaves its path on a recoil arc, back from its heading by `back` m (`awayX`, `awayZ`: the
        /// direction away from the player) and up by `up` m, which lasts `seconds`; the aggro goes on its cooldown and the contact rests for the recoil; when the arc
        /// ends the cycle moves on to its NEXT pattern (never the interrupted charge again). false: nothing to stagger (intro, beat, dying).</summary>
        public bool Stagger30(double seconds, double back, double up, double awayX, double awayZ)
        {
            if (!body.alive || phase == "intro" || phase == "beat" || phase == "dying" || phase == "dead" || FinaleOn) return false;
            double l = JsMath.Hypot(awayX, awayZ);
            if (l < 1e-6) { awayX = -headDir.x; awayZ = -headDir.z; l = JsMath.Hypot(awayX, awayZ); }
            if (l < 1e-6) { awayX = 1; awayZ = 0; l = 1; }
            awayX /= l;
            awayZ /= l;
            double half = arenaHalf > 0 ? arenaHalf - p.aimClamp : 1e9;
            double ax = Math.Max(-half, Math.Min(half, headLocal.x + awayX * back * 0.5)), az = Math.Max(-half, Math.Min(half, headLocal.z + awayZ * back * 0.5));
            double bx = Math.Max(-half, Math.Min(half, headLocal.x + awayX * back)), bz = Math.Max(-half, Math.Min(half, headLocal.z + awayZ * back));
            var v = new WyrmVariant();
            v.wp.Add(new Vec3(ax, headLocal.y + up, az));
            v.wp.Add(new Vec3(bx, headLocal.y + up * 0.4, bz));
            var pat = new WyrmPattern { name = "recoil30", pitch = curPat != null ? curPat.pitch : 0, spit = false };
            pat.variants.Add(v);
            // (an aggro dive already stepped the cycle back one, so the recoil's end reruns the pattern the dive interrupted; a lunge's pattern is followed by the next one)
            aggroTellLeft = leanLeft47 = 0;
            aggroCool = Math.Max(aggroCool, S(aggroCool31 >= 0 ? aggroCool31 : p.aggroCool));
            Lunging = false;
            StartPattern(pat);
            recoil30Speed = path.length / Math.Max(1e-6, seconds);
            contactLeft = Math.Max(contactLeft, S(seconds));
            return true;
        }

        // --- Attacks -------------------------------------------------------------------------------------------------
        void Shards(Vec3 spot)
        {
            // from the wall spot, inward (the wall's side decides), fanned over +-spreadDeg
            double inward = spot.x < center.x ? 1 : -1;
            double spread = p.shardSpread * Math.PI / 180;
            for (int k = 0; k < p.shardCount; k++)
            {
                double f = p.shardCount == 1 ? 0 : (double)k / (p.shardCount - 1) * 2 - 1;
                double a = f * spread;
                double ca = JsMath.Cos(a), sa = JsMath.Sin(a);
                var shot = new WyrmShot
                {
                    pos = spot, vel = new Vec3(inward * ca * p.shardSpeed, 0, sa * p.shardSpeed), life = S(p.shardLife), damage = p.shardDamage * scale.damage, shard = true,
                };
                shot.vel.y = (k % 2 == 0 ? p.shardUp : -p.shardDown) * p.shardSpeed;
                shots.Add(shot);
                stats["shards"]++;
            }
        }

        void Volley(bool lane2 = false)
        {
            stats["volleys"]++;
            if (lane2) stats["spits2"]++;
            double golden = p.spitGolden * Math.PI / 180;
            Vec3 chest = ChestPoint();
            for (int k = 0; k < p.spitCount; k++)
            {
                int piece = 3 + k * 2 + (lane2 ? 1 : 0); // NIGHT26: the second lane fires from the ports between the first's
                if (piece >= PieceCount - 1) piece = PieceCount - 2;
                if (buried[piece] || (shedActive && piece >= shedFirst && piece < shedEnd)) continue;
                WyrmBox b = boxes[piece];
                double a = spitAngle + k * golden;
                double ca = JsMath.Cos(a), sa = JsMath.Sin(a);
                Vec3 dir = new Vec3(b.r.x * ca + b.u.x * sa, b.r.y * ca + b.u.y * sa, b.r.z * ca + b.u.z * sa);
                shotCount++;
                int every = aim ? p.aimSpitEvery : p.spitAimedEvery;
                bool aimed = every > 0 && shotCount % every == 0;
                if (aimed)
                {
                    Vec3 at = chest;
                    if (aim)
                    {
                        // where the player will be when this spit arrives: his ground velocity over the flight time (capped); height is not predicted
                        double flight = Math.Min(p.aimLeadMax, Vec3.DistanceTo(chest, b.c) / Math.Max(1e-6, p.spitSpeed));
                        at = new Vec3(chest.x + playerVel.x * flight, chest.y, chest.z + playerVel.z * flight);
                    }
                    dir = Vec3.Normalize(at - b.c);
                }
                shots.Add(new WyrmShot { pos = b.c, vel = dir * p.spitSpeed, life = S(p.spitLife), damage = p.spitDamage * scale.damage, aimed = aimed });
            }
            spitAngle += golden * p.spitCount;
            kit.telegraph.Flash(boxes[p.pieces / 2].c.x, boxes[p.pieces / 2].c.y, boxes[p.pieces / 2].c.z, p.volleyFlash);
            Resolve(lane2 ? "spit2" : "spit");
        }

        Capsule PlayerCapsule()
        {
            if (playerBody != null) return playerBody();
            Vec3 f = target.position();
            return new Capsule { start = new Vec3(f.x, f.y + p.capsuleRadius, f.z), end = new Vec3(f.x, f.y + p.capsuleHeight - p.capsuleRadius, f.z), radius = p.capsuleRadius };
        }

        Vec3 ChestPoint()
        {
            Capsule c = PlayerCapsule();
            return new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
        }

        bool HurtPlayer(double amount)
        {
            Health h = target.health;
            if (h == null || h.Dead) return false;
            return h.Damage(amount * scale.damage, "boss");
        }

        void StepContact()
        {
            if (contactLeft > 0) contactLeft--;
            if (contactLeft > 0 || phase == "intro" || phase == "dying") return;
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            for (int i = 0; i < PieceCount; i++)
            {
                if (buried[i]) continue;
                WyrmBox b = boxes[i];
                if (!(b.Touches(c.start, c.radius) || b.Touches(mid, c.radius) || b.Touches(c.end, c.radius))) continue;
                bool head = i == 0;
                double dmg = head ? (Lunging ? p.contactLunge : p.contactHead) : p.contactBody;
                if (p.n26 && FinaleOn) dmg *= p.finaleContact; // the squeeze closes: touching the ring hurts
                if (!HurtPlayer(dmg)) return; // dead or shielded: nothing lands
                contactLeft = S(p.contactCooldown);
                stats[head ? (Lunging ? "contactsLunge" : "contactsHead") : "contactsBody"]++;
                double dx = mid.x - b.c.x, dz = mid.z - b.c.z, len = JsMath.Hypot(dx, dz);
                if (len < 1e-6)
                {
                    dx = b.r.x;
                    dz = b.r.z;
                    len = JsMath.Hypot(dx, dz);
                    if (len < 1e-6)
                    {
                        dx = 1;
                        dz = 0;
                        len = 1;
                    }
                }
                double push = head ? p.pushHead : p.pushBody;
                target.impulse?.Invoke(dx / len * push, p.pushUp, dz / len * push);
                return;
            }
        }

        void StepShots(double dt)
        {
            Capsule c = PlayerCapsule();
            Vec3 mid = new Vec3((c.start.x + c.end.x) * 0.5, (c.start.y + c.end.y) * 0.5, (c.start.z + c.end.z) * 0.5);
            for (int i = shots.Count - 1; i >= 0; i--)
            {
                WyrmShot sh = shots[i];
                Vec3 prev = sh.pos;
                sh.pos = new Vec3(prev.x + sh.vel.x * dt, prev.y + sh.vel.y * dt, prev.z + sh.vel.z * dt);
                bool dead = --sh.life <= 0;
                if (!dead && world != null)
                {
                    double len = Vec3.DistanceTo(prev, sh.pos);
                    if (len > 1e-9 && world.RayIntersect(prev, Vec3.Normalize(sh.pos - prev), out RayHit hit) && hit.distance <= len) dead = true;
                }
                if (!dead && phase != "intro")
                {
                    double rad = (sh.shard ? 0.5 : p.spitRadius) + c.radius;
                    if (Vec3.DistanceTo(sh.pos, c.start) <= rad || Vec3.DistanceTo(sh.pos, mid) <= rad || Vec3.DistanceTo(sh.pos, c.end) <= rad)
                    {
                        if (HurtPlayer(sh.damage)) stats["shotsHit"]++;
                        dead = true;
                    }
                }
                if (dead)
                {
                    shots[i] = shots[shots.Count - 1];
                    shots.RemoveAt(shots.Count - 1);
                }
            }
            for (int i = markers.Count - 1; i >= 0; i--)
                if (--markers[i].left <= 0) markers.RemoveAt(i);
        }

        void StepSpitAndCore(double baseSpeed)
        {
            bool live = phase == "p2" || phase == "p3" || phase == "p4";
            if (!live)
            {
                spitFire = 0;
                coreTell = 0;
                return;
            }
            // the spit barrage: while the worm coils or loops
            if (spitFire > 0)
            {
                if (!SpitOk) spitFire = 0; // the pattern ended under the tell: no volley
                else if (--spitFire == 0) spitFire = -1; // due: fires once nothing else resolves inside the gap
            }
            if (spitFire == -1)
            {
                int gap = S(p.resolveGap);
                if (stepNo - lastResolve >= gap && NextAttackIn(baseSpeed * PhaseSpeedMult) > p.resolveGap + 1e-9)
                {
                    spitFire = 0;
                    Volley();
                    spitLeft = SpitSteps(phase);
                    if (p.n26 && EffPhase >= 2) spit2Left = S(p.spit2Delay);
                }
                else if (!SpitOk) spitFire = 0;
            }
            else if (spitFire == 0 && SpitOk)
            {
                if (--spitLeft <= 0)
                {
                    spitFire = S(p.tells["spit"]);
                    Tell("spit", p.tells["spit"], body.position);
                }
            }
            // NIGHT26: the second spit lane, spit2Delay after the first volley (never inside the gap of a lethal attack)
            if (spit2Left > 0 && --spit2Left == 0 && SpitOk && stepNo - lastResolve >= S(p.resolveGap) && NextAttackIn(baseSpeed * PhaseSpeedMult) > p.resolveGap + 1e-9) Volley(true);
            // the core: a roar, then it is open for a while
            if (coreOpenLeft > 0)
            {
                if (--coreOpenLeft == 0)
                {
                    said.Reset("wyrm");
                    said.part = "core";
                    said.reason = "closed";
                    events.Emit("bossPart", said);
                    coreLeft = S(phase == "p3" || phase == "p4" ? p.coreEvery3 : p.coreEvery);
                }
            }
            else if (coreTell > 0)
            {
                if (--coreTell == 0)
                {
                    coreOpenLeft = S(phase == "p3" || phase == "p4" ? p.coreOpen3 : p.coreOpen);
                    stats["coreOpens"]++;
                    said.Reset("wyrm");
                    said.part = "core";
                    said.reason = "exposed";
                    said.position = body.position;
                    events.Emit("bossPart", said);
                }
            }
            else if (--coreLeft <= 0)
            {
                coreTell = S(p.tells["core"]);
                Tell("roar", p.tells["core"], body.position);
            }
        }

        // --- The body ------------------------------------------------------------------------------------------------
        double sink;

        // --- the tail-shed sub-boss (design.n24.wyrm.tailShed) ----------------------------------------------------------
        int goneFrom, shedFirst, shedEnd, shedAt, shedRegrowLeft;
        double shedHpNow;
        bool shedActive, shedPending;
        WyrmBox[] shedBoxes;
        public bool ShedActive => shedActive;
        public int ShedFirst => shedFirst;
        public int ShedEnd => shedEnd;
        public double ShedHp => shedHpNow;

        void DoShed()
        {
            shedPending = false;
            shedEnd = goneFrom;
            shedFirst = goneFrom - p.shedPieces;
            shedBoxes = shedBoxes ?? new WyrmBox[PieceCount];
            for (int i = shedFirst; i < shedEnd; i++) shedBoxes[i] = boxes[i];
            shedActive = true;
            shedHpNow = JsMath.Round(p.shedHp * scale.hp);
            shedRegrowLeft = S(p.shedRegrow);
            stats["sheds"]++;
            Resolve("shed");
            said.Reset("wyrm");
            said.part = "tail";
            said.reason = "shed";
            said.position = boxes[shedFirst].c;
            events.Emit("bossPart", said);
            kit.telegraph.Flash(boxes[shedFirst].c.x, boxes[shedFirst].c.y, boxes[shedFirst].c.z, 2);
        }

        void StepShed()
        {
            // the cut waits (its tell already showed) until no other attack resolves within the gap, either side of it
            if (shedPending && --shedAt <= 0 && stepNo - lastResolve >= S(p.resolveGap) && NextAttackIn(p.speed * scale.speed * PhaseSpeedMult) > p.resolveGap + 1e-9) DoShed();
            if (shedActive && --shedRegrowLeft <= 0)
            {
                shedActive = false;
                stats["regrows"]++;
                said.Reset("wyrm");
                said.part = "tail";
                said.reason = "regrown";
                said.position = boxes[shedFirst].c;
                events.Emit("bossPart", said);
            }
        }

        /// <summary>A hit on the cut-loose tail: its own HP; at 0 it is gone for good and the worm takes the bonus.</summary>
        bool HitShed(double dmg)
        {
            if (!shedActive) return false;
            shedHpNow -= dmg;
            said.Reset("wyrm");
            said.damage = JsMath.Round(dmg);
            said.part = "tail";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (shedHpNow <= 0)
            {
                shedActive = false;
                goneFrom = shedFirst;
                stats["shedKills"]++;
                said.Reset("wyrm");
                said.part = "tail";
                said.reason = "destroyed";
                said.position = boxes[shedFirst].c;
                events.Emit("bossPart", said);
                lastPart = "body";
                Apply(body.maxHp * p.shedBonus);
            }
            return true;
        }

        void Pose()
        {
            int n = PieceCount;
            for (int i = 0; i < n; i++) prevCenters[i] = boxes[i].c;
            // head
            WyrmBox h = default;
            h.c = ToWorld(headLocal);
            h.c.y -= sink;
            h.Orient(headDir);
            h.hr = p.headW / 2;
            h.hu = p.headH / 2;
            h.hf = p.headLen / 2;
            boxes[0] = h;
            buried[0] = h.c.y - center.y < p.buriedY;
            for (int i = 1; i < n; i++)
            {
                Vec3 ci = ToWorld(buf[(bufHead - i * p.spacingSteps) & mask]);
                ci.y -= sink * (1 + i * p.sinkTaper);
                WyrmBox b = default;
                b.c = ci;
                boxes[i] = b;
            }
            for (int i = 1; i < n; i++)
            {
                WyrmBox b = boxes[i];
                Vec3 prev = boxes[i - 1].c;
                Vec3 next = i + 1 < n ? boxes[i + 1].c : b.c;
                Vec3 fwd = i + 1 < n ? prev - next : prev - b.c;
                double fl = fwd.Length;
                b.Orient(fl > 1e-6 ? fwd / fl : boxes[i - 1].f);
                double dPrev = Vec3.DistanceTo(prev, b.c), dNext = i + 1 < n ? Vec3.DistanceTo(b.c, next) : 0;
                double len = Math.Max(dPrev, dNext) + p.overlap;
                double t = (double)(i - 1) / Math.Max(1, p.pieces - 1);
                double wd = p.bodyW0 + (p.bodyW1 - p.bodyW0) * Math.Min(1, t);
                if (i == n - 1) wd = p.bodyW1 * p.tailW;
                b.hr = wd / 2;
                b.hu = wd / 2;
                b.hf = len / 2;
                boxes[i] = b;
                buried[i] = b.c.y - center.y < p.buriedY;
            }
            if (shedActive)
                for (int i = shedFirst; i < shedEnd; i++)
                {
                    boxes[i] = shedBoxes[i];
                    buried[i] = false;
                }
            for (int i = goneFrom; i < n; i++) buried[i] = true;
        }

        /// <summary>Where piece `i`'s centre will be in `seconds` (the bot's lookahead; the world frame): the trail for what the head
        /// already passed, the current path for what is still ahead of it.</summary>
        public Vec3 PredictCenter(int piece, double seconds)
        {
            int ahead = (int)(seconds / dt0);
            int back = piece * p.spacingSteps - ahead;
            if (back >= 0 && back < buf.Length - 1) return ToWorld(buf[(bufHead - back) & mask]);
            double dist = (-back) * dt0 * p.speed * PhaseSpeedMult * scale.speed;
            Vec3 pt = path.At(Math.Min(path.length, s + dist), out Vec3 d);
            return ToWorld(pt);
        }

        // --- Hits ----------------------------------------------------------------------------------------------------
        /// <summary>The nearest piece along a unit ray (the exposed core too): the distance or -1; hitPart names it (head, core, body, tail).</summary>
        public double Raycast(Vec3 o, Vec3 d)
        {
            if (!body.alive || phase == "dying" || phase == "dead") return -1;
            double best = -1;
            string name = null;
            for (int i = 0; i < PieceCount; i++)
            {
                if (buried[i]) continue;
                double t = boxes[i].Ray(o, d);
                if (t >= 0 && (best < 0 || t < best))
                {
                    best = t;
                    name = i == 0 ? "head" : i == PieceCount - 1 || (shedActive && i >= shedFirst && i < shedEnd) ? "tail" : "body";
                }
            }
            if (CoreOpen && !buried[0])
            {
                WyrmBox h = boxes[0];
                Vec3 cc = h.c + h.u * p.coreUp - h.f * p.coreBack;
                double t = WyrmMath.RaySphere(o, d, cc, p.coreR);
                if (t >= 0 && (best < 0 || t < best))
                {
                    best = t;
                    name = "core";
                    coreFromAbove47 = o.y >= cc.y + p.coreR * coreAboveShare47; // NIGHT47 B wyrm47: the shot came from above the core (the islands, a boost)
                }
            }
            if (best < 0) return -1;
            hitPart = name;
            return best;
        }

        /// <summary>The part's multiplier: body and tail x1, the head x2, the exposed core x3. Blocked (nothing lands) in the intro, the beat and
        /// the finisher.</summary>
        public double Filter(double amount, out bool blocked)
        {
            blocked = false;
            if (!body.alive || phase == "intro" || phase == "beat" || phase == "dying")
            {
                blocked = true;
                return 0;
            }
            string part = hitPart ?? "body";
            hitPart = null;
            lastPart = part;
            if (part == "core" && coreFull48 != null && !coreFull48()) return amount * coreElse48; // NIGHT48 B kilnBoost48: the core's full x only from an island or mid-boost (G33)
            if (part == "core" && coreBelowMult47 > 0 && !coreFromAbove47) return amount * coreBelowMult47; // NIGHT47 B wyrm47: the core from below lands less (G33)
            return amount * (part == "core" ? p.coreMult : part == "head" ? p.headMult : p.bodyMult * (p.n26 ? p.bodyMult26 : 1));
        }

        /// <summary>NIGHT31 B (design.n31.wyrm31): the hooks the sim sets with the row on (null: NIGHT30's worm). feintNext31: is the aggro charge about to commit a feint;
        /// feintShort31: how far short it stops; filter31(damage, weapon, part): the weapon rule over the part's multiplier (guns 20 % unless staggered).</summary>
        public Func<bool> feintNext31;
        public double feintShort31;
        public Func<double, string, string, double> filter31;
        /// <summary>NIGHT31 B (design.n31.wyrm31): the aggro charge's cooldown and first delay with the row on (-1: design.n26's aggro.cooldown / first).</summary>
        public double aggroCool31 = -1, aggroFirst31 = -1;

        /// <summary>Filter with the hit's weapon (EnemyHits.Filter's call): the part rule, then filter31 when the sim set one.</summary>
        public double Filter(double amount, string weapon, out bool blocked)
        {
            double d = Filter(amount, out blocked);
            return filter31 == null || blocked ? d : filter31(d, weapon, lastPart);
        }

        /// <summary>One hitscan shot: the ray, the part, the HP; the damage dealt (-1: a miss, 0: blocked).</summary>
        public double TakeHit(Vec3 origin, Vec3 dir, double amount, string weapon, out double distance)
        {
            distance = Raycast(origin, dir);
            if (distance < 0) return -1;
            double dmg = Filter(amount, out bool blocked);
            if (blocked) return 0;
            Apply(dmg);
            return dmg;
        }

        /// <summary>The HP, bossHit, the phases' beats at 66 % and 33 %, the finisher at 0.</summary>
        public void Apply(double dmg)
        {
            if (!body.alive) return;
            if (lastPart == "tail" && HitShed(dmg)) return;
            body.hp -= dmg;
            body.flash = 0.06;
            string part = lastPart ?? "body";
            if (part == "core") stats["coreHits"]++;
            else if (part == "head") stats["headHits"]++;
            else stats["bodyHits"]++;
            said.Reset("wyrm");
            said.damage = JsMath.Round(dmg);
            said.part = part;
            said.core = part == "core";
            said.hp = Math.Max(0, body.hp);
            events.Emit("bossHit", said);
            if (phase == "p1" && HpFrac <= p.phase2At) EnterBeat("p2", p.phase2At, "PHASE 2");
            else if (phase == "p2" && HpFrac <= p.phase3At) EnterBeat("p3", p.phase3At, "PHASE 3");
            else if (p.phase4At > 0 && phase == "p3" && HpFrac <= p.phase4At) EnterBeat("p4", p.phase4At, "PHASE 4"); // NIGHT25 W: the ELDER WYRM's fourth phase
            else if (body.hp <= 0)
            {
                body.hp = 0;
                SetPhase("dying");
                dieSteps = S(p.dieTime);
                coreOpenLeft = coreTell = 0;
                kit.Dying("wyrm", body.position);
            }
        }

        void EnterBeat(string to, double at, string label)
        {
            body.hp = body.maxHp * at;
            nextPhase = to;
            SetPhase("beat");
            beatSteps = kit.Beat("wyrm", label, new Vec3(body.position.x, body.position.y + 3, body.position.z));
            if (beatExtra48 > 0) beatSteps += S(beatExtra48); // NIGHT48 B wyrmDive48 (the worm's min): a longer phase beat (0 = NIGHT47)
            coreOpenLeft = coreTell = 0;
            spitFire = 0;
            if (p.tailShed && goneFrom - p.shedPieces >= p.shedKeep && !shedActive)
            {
                shedPending = true;
                shedAt = S(p.tells["shed"]);
                Tell("shed", p.tells["shed"], body.position);
            }
            coreLeft = S(PhaseNo(to) >= 3 ? p.coreEvery3 : p.coreEvery) / 2;
            spitLeft = SpitSteps(to);
        }

        public BossBar Bar() => new BossBar
        {
            valid = body.alive, id = body.id, name = p.name, phase = phase, hp = Math.Max(0, body.hp), max = body.maxHp, segment = p.phase2At,
            // NIGHT25 W (boss-bar): with the tail-shed off the pips are the PHASES still to come (two lit in phase 1, one in phase 2, none in phase 3); the bar's pips are view only
            pips = p.phase4At > 0 ? 3 : p.tailShed ? 1 : 2,
            pipsOn = p.phase4At > 0 ? (phase == "dying" ? 0 : Math.Max(0, 4 - EffPhase)) // the ELDER WYRM: one pip per phase still to come
                : p.tailShed ? (shedActive ? 1 : 0) : (phase == "p3" || phase == "dying" ? 0 : phase == "p2" || (phase == "beat" && nextPhase == "p3") ? 1 : 2),
        };

        // --- The step ------------------------------------------------------------------------------------------------
        public void Step(double dt)
        {
            if (!body.alive) return;
            body.previous = body.position;
            if (Frozen) return;
            if (aim)
            {
                Vec3 pp = target.position();
                playerVel = havePlayerPos ? new Vec3((pp.x - lastPlayerPos.x) / dt0, 0, (pp.z - lastPlayerPos.z) / dt0) : default;
                if (JsMath.Hypot(playerVel.x, playerVel.z) > p.aimVelMax) playerVel = default; // a teleport (a respawn), not a run
                lastPlayerPos = pp;
                havePlayerPos = true;
            }
            stepNo++;
            phaseSteps++;
            // phase machine
            if (phase == "beat" && phaseSteps >= beatSteps) SetPhase(nextPhase);
            double dieFactor = 1;
            if (phase == "dying")
            {
                double k = 1 - (double)(dieSteps - 1) / Math.Max(1, S(p.dieTime));
                dieFactor = Math.Max(0, 1 - k * p.dieSlow * p.dieEase);
                sink = k * k * p.dieSink;
            }
            else sink = 0;
            // the head moves along the path
            double mult = 1;
            Lunging = false;
            foreach (WyrmZoneDef z in curVar.zones)
            {
                if (z.lunge && z.to >= wpOff && s >= (z.from >= wpOff ? WpS(z.from) : 0) && s <= WpS(z.to))
                {
                    mult = p.lungeMult;
                    Lunging = true;
                }
            }
            if (p.n26 && AggroDiving) mult = p.aggroSpeed;
            double baseSpeed = p.speed * scale.speed;
            double v = baseSpeed * PhaseSpeedMult * mult * dieFactor;
            if (recoil30Speed > 0 && Recoiling30) v = recoil30Speed; // NIGHT30 B (wyrm30): the recoil arc lasts design.n30.wyrm30.recoilSeconds whatever the phase (never set with the row off)
            s += v * dt;
            // NIGHT47 B wyrm47 (coilCut47; 0 = NIGHT46): the COIL ends at this share of its path in phases 1-2 (its squeeze, phase 3's, is kept whole)
            if (coilCut47 > 0 && coilCut47 < 1 && curPat != null && curPat.name == "coil" && EffPhase < 3 && s >= path.length * coilCut47 && s < path.length) s = path.length;
            while (s >= path.length && phase != "dying")
            {
                double left = s - path.length;
                headLocal = path.At(path.length, out headDir);
                if (phase == "intro")
                {
                    NextPattern();
                    s = left;
                }
                else
                {
                    NextPattern();
                    s = left;
                }
            }
            if (s > path.length) s = path.length;
            headLocal = path.At(s, out headDir);
            bufHead = (bufHead + 1) & mask;
            buf[bufHead] = headLocal;
            if (phase != "dying") StepMarks(baseSpeed * PhaseSpeedMult);
            if (p.n26) StepAggro(baseSpeed * PhaseSpeedMult);
            body.position = ToWorld(headLocal);
            body.velocity = new Vec3((body.position.x - body.previous.x) / dt0, (body.position.y - body.previous.y) / dt0, (body.position.z - body.previous.z) / dt0);
            Pose();
            StepSpitAndCore(baseSpeed);
            StepShed();
            StepShots(dt);
            if (phase != "dying") StepContact();
            if (phase == "dying" && --dieSteps <= 0)
            {
                body.alive = false;
                phase = "dead";
                kit.Down("wyrm", body.id, body.position);
            }
        }
    }
}
