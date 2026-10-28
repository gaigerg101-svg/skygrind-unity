// NIGHT47 builder L (NIGHT47.md G41 / G43 boss part; Research/2026-10-18-wave47/L-cards.md section 2 and 5, the card gate's PASS): THE CROWN
// rebuilt, behind the sim row crown47 (the sim hands the flag in through Crown47 at each fight's begin; off: nothing here runs, the brain's list
// and numbers are NIGHT46's, bit for bit). Numbers: design.n47.crown47.numbers (each with its _why there).
//   CHIP        guns outside the CYAN window chip the gems at chipScale (the TORCH at torchChip) instead of BLOCKED (0): "every gun counts".
//   THE WINDOW  windowSeconds -> windowS; one window breaks at most windowGems gems by gunfire (a pass still breaks its own), so the fight's
//               length comes from passes (VERBS 8, the jump-dash through the ring), never from HP.
//   THE SUMMIT  stays for all of phase 1 (NIGHT37: it shattered as the reveal ended, leaving the orbit platforms as the only floor: measured
//               103 falls in 10 casual fights); it shatters at phase 2's start.
//   CAUGHT      a fall never restarts the phase (Catch47: the leg's pieces come back, the brain keeps its state; the sim puts the player on the
//               leg's checkpoint for fallHp).
//   CROWN FIRE  (RED, the boss flame, legs fireLegs) the clapper heats over the tell, a floor line shows the sweep; then a cone (reachM, halfDeg)
//               from the ring sweeps arcDeg across the player's side over sweepS, a tick of tickDmg every tickS to a player inside the cone and
//               not above the ring's plane (the answer: be over it: a jump-dash through the ring, a ride, or out of reach); a hit sets the burn
//               (burnS, the view's state). Its recover is the VENT (ventS): the ring hangs still and flat: the easiest pass of the fight.
//   BELL TOLL   (RED, legs tollLegs) the ring stops and tolls: a ring wave grows from it along the player's floor at tollSpeed; a player on the
//               floor in its band when it passes is hit (tollDamage + a push out); a jump, a ride or the rope answers it.
// Pure: no UnityEngine, whole steps, JsMath, no randomness of its own (the brain picks).
using System;

namespace Skygrind.Core
{
    /// <summary>design.n47.crown47.numbers (each has its _why there).</summary>
    public sealed class Crown47Params
    {
        public double chipScale = 0.3, torchChip = 0.6, chipFloor = 0.1, windowS = 3.5, fallHp = 5, shardWindup = 1.2;
        public int windowGems = 2, passGems = 0, torchPassGems = 1;
        public double[] stagger = { 6, 6, 6 };
        public double bladeDamage = 15, bladeWindup = 1.4, slamSpeed = 20, volleyDeg = 14, finaleS = 0.35;
        public int slamGems = 1;
        public double spiralArcDeg = 360;
        public double fireWindup = 1.3, sweepS = 1.5, ventS = 1.5, arcDeg = 90, halfDeg = 14, reachM = 9, tickS = 0.25, tickDmg = 8, burnS = 1.5, fireWeight = 1.1, overM = 0.5;
        public double tollWindup = 1.3, tollSpeed = 14, tollMaxR = 30, tollBand = 1.0, tollDamage = 20, tollPush = 6, tollRecover = 1.5, tollWeight = 1;
        public int fireCost = 2, tollCost = 2, fireLegs = 5, tollLegs = 6;

        public static Crown47Params From(GameConfig c)
        {
            const string R = "design.n47.crown47.numbers.";
            var p = new Crown47Params();
            double N(string k, double d) => c.Has(R + k) ? c.Num(R + k) : d;
            int L(string k, int d)
            {
                if (!c.Has(R + k)) return d;
                int m = 0;
                foreach (double l in c.Nums(R + k)) m |= 1 << ((int)l - 1);
                return m;
            }
            p.chipScale = N("chipScale", p.chipScale);
            p.torchChip = N("torchChip", p.torchChip);
            p.windowS = N("windowS", p.windowS);
            p.windowGems = (int)N("windowGems", p.windowGems);
            p.passGems = (int)N("passGems", p.passGems);
            p.torchPassGems = (int)N("torchPassGems", p.torchPassGems);
            p.chipFloor = N("chipFloor", p.chipFloor);
            if (c.Has(R + "stagger")) p.stagger = c.Nums(R + "stagger");
            p.bladeDamage = N("bladeDamage", p.bladeDamage);
            p.bladeWindup = N("bladeWindup", p.bladeWindup);
            p.slamSpeed = N("slamSpeed", p.slamSpeed);
            p.volleyDeg = N("volleyDeg", p.volleyDeg);
            p.finaleS = N("finaleS", p.finaleS);
            p.slamGems = (int)N("slamGems", p.slamGems);
            p.fallHp = N("fallHp", p.fallHp);
            p.shardWindup = N("shardWindup", p.shardWindup);
            p.fireWindup = N("fireWindup", p.fireWindup);
            p.sweepS = N("sweepS", p.sweepS);
            p.ventS = N("ventS", p.ventS);
            p.arcDeg = N("arcDeg", p.arcDeg);
            p.spiralArcDeg = N("spiralArcDeg", p.spiralArcDeg);
            p.halfDeg = N("halfDeg", p.halfDeg);
            p.reachM = N("reachM", p.reachM);
            p.tickS = N("tickS", p.tickS);
            p.tickDmg = N("tickDmg", p.tickDmg);
            p.burnS = N("burnS", p.burnS);
            p.overM = N("overM", p.overM);
            p.fireWeight = N("fireWeight", p.fireWeight);
            p.fireCost = (int)N("fireCost", p.fireCost);
            p.fireLegs = L("fireLegs", p.fireLegs);
            p.tollWindup = N("tollWindup", p.tollWindup);
            p.tollSpeed = N("tollSpeed", p.tollSpeed);
            p.tollMaxR = N("tollMaxR", p.tollMaxR);
            p.tollBand = N("tollBand", p.tollBand);
            p.tollDamage = N("tollDamage", p.tollDamage);
            p.tollPush = N("tollPush", p.tollPush);
            p.tollRecover = N("tollRecover", p.tollRecover);
            p.tollWeight = N("tollWeight", p.tollWeight);
            p.tollCost = (int)N("tollCost", p.tollCost);
            p.tollLegs = L("tollLegs", p.tollLegs);
            return p;
        }
    }

    public sealed partial class Crown37
    {
        public const string CrownFire = "crownFire", BellToll = "bellToll";

        /// <summary>crown47 on this fight (the sim sets it at the fight's begin).</summary>
        public bool crown47;
        public Crown47Params p47;
        double c47WindowWas = double.NaN;
        double[] c47StaggerWas;
        double c47BladeWas = double.NaN;
        int c47ShardWas = -1, c47GemsAtOpen = -1, c47BladeWindWas = -1;
        bool c47WasOpen;

        /// <summary>The flame (the view): the cone's direction now (flat unit), its start / end directions, the burn left on the player (s).</summary>
        public Vec3 fireDir, fireFrom, fireTo;
        /// <summary>The sweep's middle (toward the player at the tell) and its arc (deg: arcDeg; spiralArcDeg in phase 2, FIRE SPIRAL: a full turn round the shaft).</summary>
        public Vec3 fireMid47;
        public double fireArc47 = 90;
        public bool Spiral47 => crown47 && brain.leg == 2;
        public double burnLeft47, tollR, tollY;
        int fireTick;
        bool tollHit;

        /// <summary>The harness's counters (this fight).</summary>
        public int fires47, fireTicks47, fireHits47, tolls47, tollHits47, catches47, chipHits47, slams47, volleys47;
        public double chip47;

        public bool FireTelling => crown47 && brain.Current?.id == CrownFire && brain.state == Boss37State.Tell;
        public bool FireLive => crown47 && brain.Current?.id == CrownFire && brain.state == Boss37State.Active;
        public bool Venting => crown47 && brain.Current?.id == CrownFire && brain.state == Boss37State.Recover;
        public bool TollLive => crown47 && brain.Current?.id == BellToll && brain.state == Boss37State.Active;
        /// <summary>The ring holds still (no hover move): the flame's sweep and vent, the toll.</summary>
        bool Hold47 => crown47 && brain.Current != null && (brain.Current.id == CrownFire && (brain.state == Boss37State.Active || brain.state == Boss37State.Recover)
            || brain.Current.id == BellToll && brain.state != Boss37State.Rest);

        /// <summary>At a fight's begin: crown47's attacks and numbers (on), or NIGHT46's (off).</summary>
        public void Crown47(bool on, Crown47Params cp)
        {
            for (int i = brain.attacks.Count - 1; i >= 0; i--) if (brain.attacks[i].id == CrownFire || brain.attacks[i].id == BellToll) brain.attacks.RemoveAt(i);
            if (!double.IsNaN(c47WindowWas)) { brain.p.windowSeconds = c47WindowWas; c47WindowWas = double.NaN; }
            if (c47StaggerWas != null) { brain.p.stagger = c47StaggerWas; c47StaggerWas = null; }
            if (!double.IsNaN(c47BladeWas)) { p.bladeDamage = c47BladeWas; c47BladeWas = double.NaN; }
            Boss37Attack shard = null, blade = null;
            foreach (Boss37Attack a in brain.attacks) { if (a.id == "gemShard") shard = a; if (a.id == "bladeRing") blade = a; }
            if (c47ShardWas >= 0 && shard != null) { shard.windupSteps = c47ShardWas; c47ShardWas = -1; }
            if (c47BladeWindWas >= 0 && blade != null) { blade.windupSteps = c47BladeWindWas; c47BladeWindWas = -1; }
            crown47 = on && cp != null;
            p47 = cp;
            fires47 = fireTicks47 = fireHits47 = tolls47 = tollHits47 = catches47 = chipHits47 = slams47 = volleys47 = 0;
            chip47 = 0;
            burnLeft47 = tollR = 0;
            c47GemsAtOpen = -1;
            c47WasOpen = false;
            if (!crown47) return;
            c47WindowWas = brain.p.windowSeconds;
            c47StaggerWas = brain.p.stagger;
            brain.p.stagger = cp.stagger;
            c47BladeWas = p.bladeDamage;
            p.bladeDamage = cp.bladeDamage;
            brain.p.windowSeconds = cp.windowS;
            if (shard != null) { c47ShardWas = shard.windupSteps; shard.windupSteps = Math.Max(1, S(cp.shardWindup)); }
            if (blade != null) { c47BladeWindWas = blade.windupSteps; blade.windupSteps = Math.Max(1, S(cp.bladeWindup)); }
            brain.attacks.Add(Boss37Attack.Of(CrownFire, Tell36Kind.Red, cp.fireWindup, cp.sweepS, cp.ventS, dt, cp.fireCost, cp.fireWeight, 0, 1e9, cp.fireLegs, Boss37Habit.Any));
            brain.attacks.Add(Boss37Attack.Of(BellToll, Tell36Kind.Red, cp.tollWindup, cp.tollMaxR / Math.Max(1, cp.tollSpeed), cp.tollRecover, dt, cp.tollCost, cp.tollWeight, 0, 1e9, cp.tollLegs, Boss37Habit.Ground));
        }

        /// <summary>Filter's hook outside the window: the chip (the TORCH's own).</summary>
        double Chip47(double amount, string weapon)
        {
            double s = weapon == "torch" ? p47.torchChip : p47.chipScale;
            // the chip lands and shows on the bar but never breaks a gem by itself: the gem it wears stops at chipFloor of its hp (a pass, a parry
            // or the window breaks it). MEASURED round 1 before this floor: chip was 74 % of the dealt and the human-model pilot's kill 45.9 s.
            double floor = Math.Max(0, (GemsLeft - 1) * p.gemHp + p.gemHp * p47.chipFloor);
            double d = Math.Max(0, Math.Min(amount * s, body.hp - floor));
            chipHits47++;
            chip47 += d;
            lastHitStep = stepNo;
            return d;
        }

        /// <summary>Filter's hook inside the window: gunfire breaks at most windowGems gems a window.</summary>
        double Window47(double amount)
        {
            if (c47GemsAtOpen < 0) return amount;
            double floor = Math.Max(0, (c47GemsAtOpen - p47.windowGems) * p.gemHp);
            return Math.Max(0, Math.Min(amount, body.hp - floor));
        }

        /// <summary>The sim's fall rule (crown47): the leg's pieces come back, the brain keeps its state (no restart), the shards stay.</summary>
        public void Catch47()
        {
            airTime = 0;
            catches47++;
            Piece37Kind mine = brain.leg == 1 ? Piece37Kind.Orbit : brain.leg == 2 ? Piece37Kind.Gallery : Piece37Kind.Plaza;
            for (int i = 0; i < orbit.pieces.Count; i++) if (orbit.pieces[i].kind == mine) orbit.Raise(i, events);
            Say("crown47Catch", "catch");
        }

        /// <summary>Each step after the brain (before the attacks): the summit's shatter at phase 2, the window's gem count, the burn.</summary>
        void Crown47Step(in Crown37Input inp)
        {
            if (!shattered && brain.leg >= 2) Shatter();
            bool open = brain.Open;
            if (open && !c47WasOpen) c47GemsAtOpen = GemsLeft;
            if (!open) c47GemsAtOpen = -1;
            c47WasOpen = open;
            if (burnLeft47 > 0) burnLeft47 = Math.Max(0, burnLeft47 - dt);
        }

        /// <summary>The attacks block's hook: the flame and the toll.</summary>
        void Crown47Attacks(Boss37Attack a, in Crown37Input inp)
        {
            if (a == null || (a.id != CrownFire && a.id != BellToll)) return;
            if (a.id == CrownFire)
            {
                if (brain.state == Boss37State.Tell && brain.entered)
                {
                    fires47++;
                    Vec3 to = Flat47(inp.feet - centre);
                    fireArc47 = brain.leg == 2 ? p47.spiralArcDeg : p47.arcDeg; // FIRE SPIRAL in phase 2 (the shaft): the cone turns all the way round
                    fireMid47 = to;
                    double h = fireArc47 * 0.5 * Math.PI / 180;
                    fireFrom = Rot47(to, -h);
                    fireTo = Rot47(to, h);
                    fireDir = fireFrom;
                    fireTick = 0;
                    Say("crown47Fire", "tell", a.id, 0, inp.feet);
                }
                else if (brain.state == Boss37State.Active)
                {
                    if (brain.entered) Say("crown47Fire", "sweep", a.id);
                    double t = brain.Progress, ang = (t - 0.5) * fireArc47 * Math.PI / 180;
                    fireDir = Rot47(fireMid47, ang);
                    if (++fireTick >= Math.Max(1, S(p47.tickS)))
                    {
                        fireTick = 0;
                        fireTicks47++;
                        Vec3 d = inp.chest - centre;
                        double flat = JsMath.Hypot(d.x, d.z);
                        double cos = flat > 1e-6 ? (d.x * fireDir.x + d.z * fireDir.z) / flat : 1;
                        bool inCone = flat <= p47.reachM && cos >= JsMath.Cos(p47.halfDeg * Math.PI / 180) && inp.feet.y <= centre.y + p47.overM;
                        if (inCone)
                        {
                            fireHits47++;
                            burnLeft47 = p47.burnS;
                            Hurt(p47.tickDmg);
                            Say("crown47Fire", "burn", a.id, 0, inp.chest);
                        }
                    }
                }
                else if (brain.state == Boss37State.Recover && brain.entered) Say("crown47Fire", "vent", a.id);
                return;
            }
            // the toll
            if (brain.state == Boss37State.Tell && brain.entered) { tolls47++; tollR = 0; tollHit = false; Say("crown47Toll", "tell", a.id); }
            else if (brain.state == Boss37State.Active)
            {
                if (brain.entered) { tollY = inp.feet.y; tollR = 0; Say("crown47Toll", "wave", a.id, 0, new Vec3(centre.x, tollY, centre.z)); }
                double prev = tollR;
                tollR += p47.tollSpeed * dt;
                double rr = JsMath.Hypot(inp.feet.x - centre.x, inp.feet.z - centre.z);
                if (!tollHit && rr >= prev - p47.tollBand && rr <= tollR + p47.tollBand && inp.grounded && !inp.riding && Math.Abs(inp.feet.y - tollY) <= 1.0)
                {
                    tollHit = true;
                    tollHits47++;
                    Hurt(p47.tollDamage);
                    Vec3 o = Flat47(inp.feet - centre);
                    target.impulse?.Invoke(o.x * p47.tollPush, 2, o.z * p47.tollPush);
                    Say("crown47Toll", "hit", a.id, 0, inp.feet);
                }
            }
            else tollR = 0;
        }

        /// <summary>SHARD VOLLEY (phase 3, the card's P3 remix of GEM SHARD): after the yellow shard, two RED shards fanned volleyDeg either side
        /// of it (dodge those, punch the yellow one back).</summary>
        void Volley47(in Crown37Input inp)
        {
            if (shards.Count == 0) return;
            Crown37Shard y = shards[shards.Count - 1];
            double sp = Math.Sqrt(y.vel.x * y.vel.x + y.vel.y * y.vel.y + y.vel.z * y.vel.z);
            if (sp < 1e-6) return;
            Vec3 u = y.vel / sp;
            double a = p47.volleyDeg * Math.PI / 180;
            foreach (double s in new[] { -a, a })
            {
                Vec3 f = Rot47(new Vec3(u.x, 0, u.z), s);
                double fl = Math.Sqrt(u.x * u.x + u.z * u.z);
                Vec3 d = new Vec3(f.x * fl, u.y, f.z * fl);
                shards.Add(new Crown37Shard { pos = centre, prev = centre, vel = d * sp, life = S(3), alive = true, red = true });
            }
            volleys47++;
            Say("crown47Volley", "fire", "gemShard");
        }

        static Vec3 Flat47(Vec3 v)
        {
            double l = JsMath.Hypot(v.x, v.z);
            return l > 1e-6 ? new Vec3(v.x / l, 0, v.z / l) : new Vec3(0, 0, 1);
        }

        static Vec3 Rot47(Vec3 v, double a)
        {
            double c = JsMath.Cos(a), s = JsMath.Sin(a);
            return new Vec3(v.x * c - v.z * s, 0, v.x * s + v.z * c);
        }
    }
}
