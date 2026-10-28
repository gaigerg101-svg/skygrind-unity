// NIGHT47 builder W (SurvivalSim.N47.cs's W hooks; rows wick47 / climb47 / wickTop47, design.n47; Research/2026-10-18-wave47/W-cards.md):
//   climb47   the Mover's climb rules (Core/Climb47.cs) for the run, and the CLIMB WALL test it asks (Mover.climbWall): THE STACK's ride panels
//             (the face at panelApothem from the shaft's axis, inside a panel's angle span and height band). Off: Climb47.Params.Off and no test
//             (NIGHT46's Mover bit for bit).
//   wick47    THE WICK in three phases (Core/Enemies/Wick37.N47.cs): the stack rebuilt with design.n47.wick47.numbers.stack (the rise per
//             section; NIGHT46's stack when off), the fight armed (Wick47Arm, one line in SurvivalSim.Wick37.cs), P1 with the panels folded into
//             the wall, P2 with them out (wick47Phase 2), THE NET (a fall more than netDrop under the last ledge stood on returns the player to
//             it: G2, "a failed jump never drops the player more than one segment"; wick47Updraft), the player launch for the core punch.
//   wickTop47 the glass over the shaft when P3 starts (Wick37Stack.GlassTris): solid to the player and every enemy (the stack's own layer).
// Per-run fields reset in N47WStartRun (rule 6); no allocation per step (the layer is refilled only at a phase change).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Func<Vec3, Vec3, double> climb47Test, climb47NextTest;
        Wick37Stack wick47Stack, wick37StackN46;
        Wick47Params wick47P;
        bool wick47Hooked, wick47PanelsOut = true, wick47Glass;
        /// <summary>NIGHT47 wick47 / wickTop47 (the view reads them): the panels out of the wall (P1: folded in), the glass over the shaft (P3).</summary>
        public bool Wick47PanelsOut => wick47PanelsOut;
        public bool Wick47GlassOn => wick47Glass;
        public int wick47Nets, wick47Phase2At = -1, wick47Phase3At = -1, wick47Steps, wick47Hitstops, wick47LedgeChips;
        double wick47NetY;
        readonly Wick47Event w47e = new Wick47Event();

        partial void N47WStartRun()
        {
            idRideParry47 = rowsRegistry.IndexOf("wickRideParry47"); // NIGHT47 W G9 (round 4): the two G9 sim rows (-1: this config lacks them)
            idIntro47 = rowsRegistry.IndexOf("wickIntro47");
            wick47IntroDue = false;
            bool climb = rows.On(idClimb47);
            mover.SetClimb47(Climb47.Params.From(cfg, climb));
            climb47Test ??= Climb47Wall;
            mover.climbWall = climb ? climb47Test : null;
            climb47NextTest ??= Climb47Next;
            mover.climbNext = climb ? climb47NextTest : null;
            wick47Nets = 0;
            wick47Hitstops = 0;
            wick47LedgeChips = 0;
            wick47NetY = 0;
            wick47Phase2At = wick47Phase3At = -1;
            wick47Steps = 0;
            wick47Glass = false;
            wick47PanelsOut = true;
            if (!wick47Hooked)
            {
                wick47Hooked = true;
                events.On<Wick47Event>("wick47Phase", OnWick47Phase);
                events.On<Wick37Fx>("wick37Crack", e => { if (e.stage == "crack" && wick37 != null && wick37.w47) Wick47Say("crack"); });
                events.On<Wick47Event>("wick47CrackOpen", e => Wick47Say("open"));
                events.On<Wick47Event>("wick47Updraft", e => { if (e.stage == "net" && wick37 != null && wick37.Alive) Wick47Say("net"); });
                events.On<Wick47Event>("wick47CorePunch", e => { if (rows.On(idWick47)) { Hitstop(cfg.Num("design.n47.wick47.numbers.charge.hitstop"), "wick47CorePunch"); wick47Hitstops++; } }); // G7: the sim's hit-stop on the core punch (the view adds the slow motion and the camera hit)
            }
            // the stack: n47's (the rise per section) with wick47 on, NIGHT46's otherwise
            bool on = rows.On(idWick47);
            if (wick37Stack != null && wick37Stack != wick47Stack && wick37Stack != wick48Stack) wick37StackN46 = wick37Stack; // NIGHT48 climbTop48: n48's lipped stack is never the NIGHT46 one (null with the row off: NIGHT47)
            if (on && wick37Centre != null)
            {
                wick47P ??= Wick47Params.From(cfg);
                if (wick47Stack == null)
                {
                    Wick37StackParams sp = Wick37StackParams.From(cfg);
                    const string g = "design.n47.wick47.numbers.stack.";
                    sp.riseBySection = cfg.Nums(g + "riseBySection");
                    sp.ledgeAfter = cfg.Nums(g + "ledgeAfter");
                    sp.panelCount = cfg.Int(g + "panelCount");
                    sp.heightBySection = cfg.Nums(g + "heightBySection");
                    sp.lengthBySection = cfg.Nums(g + "lengthBySection");
                    sp.stepCount = cfg.Int(g + "stepCount");
                    sp.stepRise = cfg.Num(g + "stepRise");
                    sp.stepDeg = cfg.Num(g + "stepDeg");
                    sp.stepArcDeg = cfg.Num(g + "stepArcDeg");
                    sp.stepArcs = cfg.Nums(g + "stepArcs");
                    sp.stepGapDeg = cfg.Num(g + "stepGapDeg");
                    sp.stepDepths = cfg.Nums(g + "stepDepths");
                    sp.stepDepth = cfg.Num(g + "stepDepth");
                    sp.stepToPanelDeg = cfg.Num(g + "stepToPanelDeg");
                    wick47Stack = new Wick37Stack(sp);
                }
                if (wick37Stack != wick47Stack) { wick37Stack = wick47Stack; Wick37Build(); }
            }
            else if ((wick37Stack == wick47Stack || wick48Stack != null && wick37Stack == wick48Stack) && wick47Stack != null && wick37StackN46 != null)
            {
                wick37Stack = wick37StackN46;
                if (wick37Centre != null) Wick37Build();
            }
        }

        /// <summary>One line in Wick37Begin, before StartFight: the fight takes wick47's bit, numbers and vents; P1's layer (the panels folded in).</summary>
        void Wick47Arm()
        {
            if (wick37 == null) return;
            bool on = rows.On(idWick47);
            wick37.w47 = on;
            if (!on) return;
            wick47P ??= Wick47Params.From(cfg);
            wick37.p47 = wick47P;
            wick37.rideParry47 = rows.On(idRideParry47); // G9 idea 1
            if (wick37.rideParry47) { wick37.rideMult47 = cfg.Num("design.n47.wickRideParry47.numbers.speedMult"); wick37.rideCracks47 = cfg.Int("design.n47.wickRideParry47.numbers.cracks"); }
            wick37.vents47 = Wick47Vents("design.n47.wick47.numbers.vents", 0);
            wick37.capVents47 = Wick47Vents("design.n47.wick47.numbers.capVents", wick37Stack.p.capY);
            wick37.launchPlayer ??= (vy, vx, vz) => mover.Launch(vy, double.IsNaN(vx) ? (double?)null : vx, double.IsNaN(vz) ? (double?)null : vz);
            Wick47Layer(false, false);
            // P1 is fought on the floor: the checkpoint (and so the slag's ceiling, belowLedge under it) is the floor, not ledge 1
            if (wick37Slag != null && wick37CheckpointY > 0) { wick37CheckpointY = 0; wick37Slag.SetCheckpoint(0); }
            Wick47LineBegin();
        }

        /// <summary>G5: THE WICK's fight lines (design.n47.wick47.numbers.lines), once each per fight, through BossLine40 (the lesson widget).</summary>
        public BossLine40 wick47Line;
        bool wick47Rode, wick47IntroDue;
        internal int idRideParry47 = -1, idIntro47 = -1;
        public int wick47Intros;

        void Wick47LineBegin()
        {
            wick47Line ??= new BossLine40(events, step);
            string roomId = null;
            int room = -1;
            if (campaign != null && campaignLevel != null && campaign.room >= 0 && campaign.room < campaignLevel.rooms.Count) { room = campaign.room; roomId = campaignLevel.rooms[room].id; }
            wick47Line.Begin("wick47", roomId, room);
            wick47Rode = false;
            if (rows.On(idIntro47)) // G9 idea 2: the name card first, the reveal hint when it ends (N47WStep)
            {
                wick47Line.Say("intro", cfg.Str("design.n47.wickIntro47.numbers.line"), cfg.Num("design.n47.wickIntro47.numbers.seconds"));
                wick47Intros++;
                wick47IntroDue = true;
            }
            else Wick47Say("reveal");
        }

        void Wick47Say(string key)
        {
            if (wick47Line == null || !rows.On(idWick47)) return;
            const string g = "design.n47.wick47.numbers.lines.";
            string text = cfg.Str(g + key);
            int words = 1;
            for (int i = 0; i < text.Length; i++) if (text[i] == ' ') words++;
            wick47Line.Say(key, text, cfg.Num(g + "secondsBase") + cfg.Num(g + "secondsPerWord") * words);
        }

        Vec3[] Wick47Vents(string path, double y)
        {
            double[] xz = cfg.Nums(path);
            var v = new Vec3[xz.Length / 2];
            for (int i = 0; i < v.Length; i++) v[i] = new Vec3(xz[2 * i], y, xz[2 * i + 1]);
            return v;
        }

        void Wick47Layer(bool panels, bool glass)
        {
            if (wick37Stack == null || wick37Centre == null) return;
            if (panels == wick47PanelsOut && glass == wick47Glass) return;
            wick47PanelsOut = panels;
            wick47Glass = glass;
            wick37Stack.FillLayer(wick37Layer, wick37Centre.Value, panels, glass);
            world.SetLayer("wick37Stack", wick37Layer, true);
        }

        void OnWick47Phase(Wick47Event e)
        {
            if (!rows.On(idWick47) || e.stage != "phase") return;
            if (e.phase == 2) { wick47Phase2At = wick47Steps; Wick47Layer(true, false); Wick47Say("phase2"); }
            if (e.phase == 3) { wick47Phase3At = wick47Steps; Wick47Layer(true, rows.On(idWickTop47)); Wick47Say("phase3"); }
        }

        partial void N47WStep(double dt)
        {
            wick47Steps++;
            if (!rows.On(idWick47) || !Wick37On || wick47P == null) return;
            // THE NET (P2, or the stack with no fight): a fall more than netDrop under the last ledge stood on returns the player to it
            bool fight = wick37 != null && wick37Owns && wick37.Alive;
            if (wick47Line != null) { if (fight) wick47Line.Step(); else if (wick47Line.Now != null) wick47Line.Clear(); }
            if (wick47IntroDue && fight && wick47Line != null && wick47Line.Now == null) { wick47IntroDue = false; Wick47Say("reveal"); } // G9 idea 2: the hint after the name card
            if (fight && wick37.Phase47 == 2 && !wick47Rode && mover.state == MoveState.WallRide) { wick47Rode = true; Wick47Say("ride"); }
            if (fight && wick37.Phase47 != 2) return;
            if (fight && wick37Slag != null) { wick37Slag.SetCheckpoint(0); if (wick37Slag.y > wick37Slag.p.startY) wick37Slag.y = wick37Slag.p.startY; } // round 3: no slag in P2 (the climb is forgiving; THE NET catches the falls): with the slag at ledge 1 - 4 m the steps flooded and a fall to them looped slag hits (37 deaths in a capped seed)
            Vec3 o = wick37Centre.Value, l = mover.position - o;
            Wick37Stack s = wick37Stack;
            if (mover.grounded)
            {
                // the last ledge stood on (the floor at 0 counts as none)
                double f = s.FloorUnder(l.y);
                if (f > wick47NetY + 0.5 && Math.Abs(l.y - f) < 0.3 && f < s.p.capY - 0.1)
                {
                    wick47NetY = f;
                    if (fight && wick37.Phase47 == 2) { wick37.Apply(wick37.body.maxHp * cfg.Num("design.n47.wick47.numbers.ledgeChip")); wick47LedgeChips++; } // round 3: a ledge reached in P2 lands a free chip (the climb is the fight, never a pure wait)
                }
                return;
            }
            if (wick47NetY <= 0 || JsMath.Hypot(l.x, l.z) > s.p.radius || l.y > wick47NetY - cfg.Num("design.n47.wick47.numbers.netDrop")) return;
            int li = -1;
            for (int k = 0; k < s.p.ledges.Length; k++) if (Math.Abs(s.p.ledges[k] - wick47NetY) < 0.05) li = k;
            if (li < 0) return;
            double ca = s.ledgeAngle.Length > li ? s.ledgeAngle[li] : Math.PI / 2, rl = s.p.radius - s.p.ledgeWidth * 0.5;
            Vec3 at = o + new Vec3(JsMath.Cos(ca) * rl, wick47NetY, JsMath.Sin(ca) * rl);
            Respawn(new Vec3(at.x, at.y + 0.05, at.z), 0);
            if (fight) health.Damage(cfg.Num("design.n47.wick47.numbers.netCost"), "boss");
            wick47Nets++;
            w47e.stage = "net";
            w47e.phase = 2;
            w47e.at = mover.position;
            w47e.value = wick47NetY;
            events.Emit("wick47Updraft", w47e);
        }

        partial void N47WCampaign() { }

        /// <summary>NIGHT47 climb47: the climb panel's bottom (world y) when the wall with normal n at the feet p is one of THE STACK's ride panels,
        /// else NaN: the stack on this level (and its panels out), the feet within 1.5 m of the panels' face circle, the normal facing the axis,
        /// inside a panel's angle span (its half length over the apothem), the feet between its bottom - 2.5 m and its top.</summary>
        /// <summary>NIGHT47 climb47: the next hold's height (world y) from the climb panel with normal n at the feet p: the next panel's bottom in
        /// its section, else the section's ledge top (the last section: the cap); NaN when p is on no climb panel.</summary>
        double Climb47Next(Vec3 p, Vec3 n)
        {
            return Climb47NextOf(p, n);
        }

        /// <summary>NIGHT47 wick47 (round 3): arena-local feet l over one of THE STACK's steps (inside its arc, within 3 m over its top, its depth
        /// from the wall): the hot wall does not throw a player off a step.</summary>
        bool Wick47OverStep(Vec3 l)
        {
            Wick37Stack s = wick37Stack;
            if (s == null || s.stepTops.Count == 0) return false;
            double r = JsMath.Hypot(l.x, l.z), a = JsMath.Atan2(l.z, l.x), half0 = 0.05;

            for (int i = 0; i < s.stepTops.Count; i++)
            {
                double d = a - s.stepAngles[i];
                while (d > Math.PI) d -= 2 * Math.PI;
                while (d < -Math.PI) d += 2 * Math.PI;
                if (Math.Abs(d) <= s.StepArc(i) / 2 + half0 && r >= s.p.radius - s.StepDepth(i) - 0.4 && l.y > s.stepTops[i] - 0.6 && l.y < s.stepTops[i] + 3) return true;
            }
            return false;
        }

        double Climb47NextOf(Vec3 p, Vec3 n)
        {
            int i = Climb47Panel(p, n);
            if (i < 0) return double.NaN;
            Vec3 c = wick37Centre.Value;
            var list = wick37Stack.panels;
            Wick37Panel q = list[i];
            if (i + 1 < list.Count && list[i + 1].section == q.section) return c.y + list[i + 1].bottom;
            double[] ledges = wick37Stack.p.ledges;
            return c.y + (q.section < ledges.Length ? ledges[q.section] : wick37Stack.p.capY);
        }

        double Climb47Wall(Vec3 p, Vec3 n)
        {
            int i = Climb47Panel(p, n);
            return i < 0 ? double.NaN : wick37Centre.Value.y + wick37Stack.panels[i].bottom;
        }

        int Climb47Panel(Vec3 p, Vec3 n)
        {
            if (wick37Stack == null || wick37Centre == null || !wick47PanelsOut) return -1;
            Vec3 c = wick37Centre.Value;
            double lx = p.x - c.x, lz = p.z - c.z, ly = p.y - c.y;
            double r = JsMath.Hypot(lx, lz);
            Wick37StackParams sp = wick37Stack.p;
            if (r < 1e-6 || Math.Abs(r - sp.panelApothem) > 1.5) return -1;
            if (-(n.x * lx + n.z * lz) / r < 0.8) return -1; // the face looks at the axis
            double a = JsMath.Atan2(lz, lx);
            var list = wick37Stack.panels;
            for (int i = 0; i < list.Count; i++)
            {
                Wick37Panel q = list[i];
                double d = a - q.angle;
                while (d > Math.PI) d -= 2 * Math.PI;
                while (d < -Math.PI) d += 2 * Math.PI;
                if (Math.Abs(d) > JsMath.Atan2((q.end - q.start).Length * 0.5 + 0.5, sp.panelApothem)) continue; // the panel's own length (per section)
                if (ly < q.bottom - 2.5 || ly > q.top) continue;
                return i;
            }
            return -1;
        }
    }
}
