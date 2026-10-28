// NIGHT30 (builder S owns this file): the sim's half of the NIGHT30 systems rows (Core/N30Blocks.cs): span (THE ARCLINE in a campaign level),
// rules (Core/Guns/Rules30.cs), lava (Core/Lava30.cs), glide (Core/Glide30.cs), teach (the lesson line's clearing). The hooks are the
// declaration-only ones in SurvivalSim.N30.cs (N30SystemsStartRun / N30SystemsStep, the lead's call sites). Every effect is gated on its n30 bit:
// with the bits off nothing here runs and NIGHT29's game plays bit for bit.
//
// span: THE ARCLINE (Core/Arcline21.cs, wired by SurvivalSim.Arcline21.cs whenever design.n21.arcline is on) is OWNED from the level's start in a
// CAMPAIGN run whose level carries grapple nodes (the level JSON's `grappleNodes`, from the plan's `grappleNodes` rows). No terminal, no price.
// The swing keeps its 0.4 s (arcline.cooldown, the Mover's rope cooldown) and the yank its 15 s (design.n21.arcline.yankCooldown). campaignOn is
// known only after N24StartRun (which runs after N30StartRun), so the ownership is taken at the run's first fixed step.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT30 span: the ARCLINE was handed over at this level's start (false until the first step of a span level).</summary>
        public bool arclineOwned30;

        bool systems30First;

        // ---- teach (item 12 + P0 b / d) ----------------------------------------------------------------------------------------------------------

        /// <summary>NIGHT30 teach: the boss lines' budget (null with the block off).</summary>
        public Teach30 teach30;
        readonly LessonEvent bossLineEvent = new LessonEvent { id = "boss", kind = "boss" };
        bool teach30Hooked;
        string teach30BossRoom;

        /// <summary>NIGHT30 teach (for builder B's bosses): show the boss line `prompt` (key `key`, e.g. "lantern.lava") through the lesson widget, at most
        /// design.n30.teach.maxShows times per fight. False: the block is off, or the line has had its shows this fight.</summary>
        public bool BossLine30(string key, string prompt, string promptPad = null)
        {
            if (!n30.teach || teach30 == null || !teach30.Allow(key)) return false;
            bossLineEvent.room = campaign != null ? campaign.room : -1;
            bossLineEvent.prompt = prompt;
            bossLineEvent.promptPad = promptPad ?? prompt;
            bossLineEvent.time = SimTime;
            events.Emit("lessonBegin", bossLineEvent);
            return true;
        }

        /// <summary>NIGHT30 teach: the boss line goes (the tell ended).</summary>
        public void BossLineClear30()
        {
            if (!n30.teach) return;
            events.Emit("lessonClear", bossLineEvent);
        }

        /// <summary>NIGHT30 P0 d (the view's gate, behind N30Fixes): in a campaign run a boss bar shows only while the player is in a BOSS room (GG's frame:
        /// the Kiln Wyrm's bar in room k4; the worm exists from the level's start).</summary>
        public bool BossBarHidden30 => campaignOn && (campaign == null || !campaign.InBossRoom);

        // ---- rules (item 1) -------------------------------------------------------------------------------------------------------------------

        /// <summary>NIGHT30 rules: the object the loadout reads (created at the first run with the block on; the loadout's field is null with it off).</summary>
        public Rules30 rules30;
        bool rules30Hooked;

        void Rules30StartRun()
        {
            if (n30.rules)
            {
                rules30 ??= Rules30.From(cfg);
                rules30.Reset();
                loadout.rules30 = rules30;
                if (!rules30Hooked)
                {
                    rules30Hooked = true;
                    events.On("pulse27Blast", _ => loadout.rules30?.OnBlast()); // the PULSE's LT blast is an action (rule 1)
                }
            }
            else
            {
                rules30?.Reset();
                loadout.rules30 = null;
            }
        }

        // ---- lava (item 3) --------------------------------------------------------------------------------------------------------------------

        /// <summary>NIGHT30 lava: the map's lava surfaces and the burn rule (null with the block off).</summary>
        public Lava30 lava30;
        Func<double, bool> lavaHurt30;
        Action<double> lavaKnock30;

        void Lava30StartRun()
        {
            if (!n30.lava)
            {
                lava30 = null;
                return;
            }
            lava30 ??= new Lava30(Lava30Params.From(cfg), step);
            lava30.Load(map.json);
            lava30.Reset();
            lavaHurt30 ??= amount => health.Damage(amount, "lava");
            lavaKnock30 ??= vy => mover.Launch(vy); // the jump pad's path: ends a dash / slide / ride, airborne; no Mover edit
        }

        void Lava30Step()
        {
            if (lava30 == null || lava30.zones.Count == 0) return;
            lava30.holdEnemies34 = n34HoldWorld; // NIGHT34 B: the split bend's held step holds the enemies' burns (Lava30.holdEnemies34)
            lava30.Step(mover.position, health.max, flow == Flow.Playing, lavaHurt30, lavaKnock30, enemies.all, events);
        }

        // ---- glide (item 6a) ------------------------------------------------------------------------------------------------------------------

        /// <summary>NIGHT30 glide: the ice rule (null with the block off).</summary>
        public Glide30 glide30;

        /// <summary>NIGHT30 glide (for builder B's Glacier30 arena): an extra ice test at the feet (null: only the level's ICE surfaces, design.n26.surfaces).</summary>
        public Func<Vec3, bool> glideIceAt30;

        bool glide30Hooked;

        /// <summary>Are the feet on an ice surface (a level ICE rect, or B's arena test)?</summary>
        public bool OnIce30(Vec3 feet)
        {
            if (glideIceAt30 != null && glideIceAt30(feet)) return true;
            if (surfaces26 == null) return false;
            foreach (SurfaceZone z in surfaces26.zones)
                if (z.kind == SurfaceKind.Ice && z.Contains(feet.x, feet.y, feet.z)) return true;
            return false;
        }

        void Glide30StartRun()
        {
            if (glacier29 != null) glacier29.lane.off30 = n30.glide;
            if (!n30.glide)
            {
                glide30 = null;
                return;
            }
            glide30 ??= new Glide30(Glide30Params.From(cfg), cfg.movement);
            glide30.Reset();
            if (!glide30Hooked)
            {
                glide30Hooked = true;
                mover.Events += e =>
                {
                    if (e.type == MoverEventType.Dash && glide30 != null && n30.glide)
                        glide30.OnDash(e.entrySpeed, (e.from == MoveState.Ground || e.from == MoveState.Slide) && OnIce30(mover.position));
                };
            }
        }

        void Glide30Step()
        {
            if (glacier29 != null) glacier29.lane.off30 = n30.glide; // the snap lanes stay off while glide is on (B's Glacier29 steps before this hook)
            if (glide30 == null) return;
            // the move input in world x / z (yaw 0 looks toward -z: forward (-sin, -cos), right (cos, -sin)), from this step's frame (N24LimitFrame)
            InputFrame f = moveFrame30;
            double sy = JsMath.Sin(f.yaw), cy = JsMath.Cos(f.yaw);
            double wx = -sy * f.moveZ + cy * f.moveX, wz = -cy * f.moveZ - sy * f.moveX;
            glide30.Step(mover, OnIce30(mover.position), wx, wz);
        }

        partial void N30SystemsStartRun()
        {
            Glide30StartRun();
            arclineOwned30 = false;
            systems30First = true;
            bossLineUp30 = false;
            roomLineUp30 = null;
            if (!roomLineHooked30) // at the run's start: the first room's line begins inside the first live step, before this hook's step runs
            {
                roomLineHooked30 = true;
                events.On<LessonEvent>("lessonBegin", e => roomLineUp30 = e.id);
                events.On<LessonEvent>("lessonClear", e => roomLineUp30 = null);
            }
            Rules30StartRun();
            Lava30StartRun();
            if (n30.teach)
            {
                teach30 ??= Teach30.From(cfg);
                teach30.ResetFight();
                teach30BossRoom = null; // a run's start (a level load): every boss teaches again
                if (!teach30Hooked)
                {
                    teach30Hooked = true;
                    // a DIFFERENT boss room entered: a new fight's budget (a restart of the same room after a death keeps the count: the round-1 reel showed the
                    // Lantern's lines 6 times over three attempts; the lines teach once, then stay quiet)
                    events.On<RoomEvent>("roomEnter", e =>
                    {
                        if (!n30.teach || teach30 == null || e.kind != "boss" || e.id == teach30BossRoom) return;
                        teach30BossRoom = e.id;
                        teach30.ResetFight();
                    });
                }
            }
            else teach30 = null;
        }

        partial void N30SystemsStep(double dt)
        {
            if (systems30First)
            {
                systems30First = false;
                if (n30.span && campaignOn && arcline21 != null && n21.arcline && (map.grappleNodes.Count > 0 || n35.grapple35) && !arcline21.Owned && Grapple35AutoOwn) // NIGHT35: grapple35 owns it from design.n35.grapple35.fromLevel only, on every level from there (it pulls to walls: a level without nodes keeps it, PRINCIPLES 1) (SurvivalSim.N35.cs)
                {
                    arcline21.Own(true); // the level's verb: owned from the start (no terminal in a campaign level)
                    arclineOwned30 = true;
                }
            }
            Lava30Step();
            Glide30Step();
            BossLines30Step();
            RoomLine30Step();
        }

        string roomLineUp30; // the room id whose lesson line is up (null: none; "boss": a boss line)
        bool roomLineHooked30;
        readonly LessonEvent staleClear30 = new LessonEvent { kind = "" };

        /// <summary>NIGHT30 teach (final fix, the r6 frame): a room's lesson line goes the moment the player's feet are in ANOTHER room's zone, whatever the path (a
        /// respawn, DOORFREE, a boss start that moves the player): Campaign24's Enter / Clear cover only the normal door. The current room's lesson stops too.</summary>
        void RoomLine30Step()
        {
            if (!n30.teach || !campaignOn || campaignLevel == null) return;
            if (roomLineUp30 == null || roomLineUp30 == "boss") return;
            Vec3 f = mover.position;
            string here = zones.ZoneAt(new Vec3(f.x, f.y + 1, f.z));
            if (here == null || here == roomLineUp30) return;
            int hereIdx = -1, lineIdx = -1;
            for (int i = 0; i < campaignLevel.rooms.Count; i++)
            {
                if (campaignLevel.rooms[i].id == here) hereIdx = i;
                if (campaignLevel.rooms[i].id == roomLineUp30) lineIdx = i;
            }
            if (hereIdx < 0) return; // a door's zone between two rooms: not yet another room
            // a room AHEAD of the line's room, or any boss room (a step back into an earlier room keeps the current room's lesson: it still has to be done)
            if (!(hereIdx > lineIdx || campaignLevel.rooms[hereIdx].kind == "boss")) return;
            string gone = roomLineUp30;
            if (campaign != null && campaign.DropLessonFor30(gone)) return; // its lessonClear set roomLineUp30 = null
            staleClear30.id = gone; // the line is up but its lesson is not the campaign's current one: clear the line alone
            staleClear30.room = campaign != null ? campaign.room : -1;
            events.Emit("lessonClear", staleClear30);
        }

        Func<string, string, bool> lanternSay30;
        bool bossLineUp30;

        /// <summary>NIGHT30 teach: B's Lantern30 asks the ONE budget (BossLine30) before it says a line, so the line shows through the lesson widget (lessonBegin) at
        /// most maxShows times per fight, and goes (lessonClear) when the Lantern's line time runs out. Set before N30BossStep (the lead's order: systems, then bosses).</summary>
        void BossLines30Step()
        {
            if (lantern30 == null) return;
            if (!n30.teach)
            {
                lantern30.sayLine30 = null;
                return;
            }
            lanternSay30 ??= (k, t) => BossLine30("lantern." + k, t);
            lantern30.sayLine30 = lanternSay30;
            if (lantern30.LineNow != null) bossLineUp30 = true;
            else if (bossLineUp30)
            {
                bossLineUp30 = false;
                BossLineClear30();
            }
        }
    }
}
