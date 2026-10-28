// NIGHT24 (builder L): the sim's side of Core/Campaign24.cs. N24CampaignStartRun / N24CampaignStep (the lead's call sites in
// SurvivalSim.Run.cs, simulate() order after the enemies) run only with design.n24.campaign.enabled (this run's n24.campaign) AND
// the run mode CAMPAIGN picked (setup.mode == Campaign24.Mode) on a map that carries a "campaign" block (Design/levels/<id>.json).
// Otherwise nothing here does anything and SURVIVAL / THE GRIND are untouched.
//
// How it rides on SURVIVAL's engine: the run is one endless round in its fight (the Campaign24Spawner swallows the Director's
// calls, so no squad ever streams in); the rooms spawn their own encounters (Campaign24, husks.Spawn at the room's nodes). SURVIVAL's
// records are never touched by a campaign run (noRecords is held for the run, as the GRIND does).
// The two edits the lead applies in the lead's files (C0):
//   SurvivalSim.cs line "spawner = director;"  ->  "spawner = campaignSpawner = new Campaign24Spawner(director);"
//   SurvivalSim.Run.cs Step(), beside the n20.zoom frame edit:  "N24LimitFrame(ref frame);"  (after `InputFrame frame = input.SampleMove(Active);`)
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The run's spawner wrapper (SurvivalSim.cs builds SurvivalRun with it; null in a composition that was not given it).</summary>
        public Campaign24Spawner campaignSpawner;

        /// <summary>This run is a CAMPAIGN run (set at the run's start).</summary>
        public bool campaignOn;

        public Campaign24 campaign;
        public LevelDef campaignLevel;
        bool campaignEnded, campaignHeldNoRecords, campaignPrevNoRecords;
        readonly List<string> limitSavedOwned = new List<string>();
        int limitFlags;
        bool limitPistolActive;

        /// <summary>The LIMIT flags in force now (the HUD's banner, the dash filter).</summary>
        public int CampaignLimits => limitFlags;

        // NIGHT25: the lesson rule's counters (the tutorial's stages read differences of these; they only grow, never reset in a sim's life).
        int lsWallJumps, lsDashes, lsPads, lsFires, lsHits, lsPunches, lsFlicks, lsKills;
        bool lessonHooked;

        void HookLessonCounters()
        {
            if (lessonHooked) return;
            lessonHooked = true;
            mover.Events += e =>
            {
                if (e.type == MoverEventType.WallJump) lsWallJumps++;
                else if (e.type == MoverEventType.Dash) lsDashes++;
                else if (e.type == MoverEventType.JumpPad) lsPads++;
            };
            events.On("fire", _ => lsFires++);
            events.On("shotHit", _ => lsHits++);
            events.On("punch", _ => lsPunches++);
            events.On("fistFlick", _ => lsFlicks++);
            events.On("pulse27Blast", _ => lsFlicks++); // NIGHT27: the PULSE's LT blast is the launch the flick lesson counts
            events.On("kill", o => { if (!(o is KillEvent k) || k.owner == "player") lsKills++; });
        }

        sealed class CampaignWorld : ICampaignWorld, ILessonSource
        {
            readonly SurvivalSim s;
            public CampaignWorld(SurvivalSim sim) => s = sim;

            public LessonSample Sample(int step) => new LessonSample
            {
                step = step, feet = s.mover.position, grounded = s.mover.grounded, sliding = s.mover.state == MoveState.Slide,
                wallJumps = s.lsWallJumps, dashes = s.lsDashes, pads = s.lsPads, fires = s.lsFires, hits = s.lsHits, punches = s.lsPunches, flicks = s.lsFlicks, kills = s.lsKills, lockoutGap = s.n29.lockout ? s.loadout.LockoutWindowSteps29 : 0, wallRiding = s.mover.state == MoveState.WallRide,
            };

            /// <summary>A lesson whose verb a switched-off block removes is passed over: the melee-and-fire lesson needs design.n25.meleeFire, the flick needs the NIGHT24 charged fist.</summary>
            public bool LessonAvailable(LessonKind kind) =>
                kind == LessonKind.PunchWhileFiring ? s.n25.meleeFire : kind == LessonKind.FlickCrossed ? s.n24.chargeFist : true;

            public Vec3 PlayerFeet => s.mover.position;
            public bool PlayerDead => s.health.Dead || s.flow == Flow.Down; // DOWN counts even if something refilled the HP (the room restart must not be skipped)
            public string ZoneAt(Vec3 p) => s.zones.ZoneAt(new Vec3(p.x, p.y + 1, p.z));
            public int EnemiesAlive => s.enemies.AliveCount;

            public bool Spawn(string kind, Vec3 at)
            {
                Husk h = s.husks.Spawn(s.Rooms43Kind(kind), at, s.mover.position); // NIGHT43 E1: "heavy" is the TANK with rooms43 on (else the kind as written)
                if (h == null) return false;
                if (!(h is Lancer)) s.brain.Attach(h); // as the Director's SpawnGroup: the brain has no stationary kind yet
                s.Rooms43Track(h, false); // NIGHT43 E1: the room's own body (rooms43 on only)
                return true;
            }

            public void SpawnStandIn(Vec3 at, bool strafe, double zA, double zB)
            {
                int had = s.standIns29.Count;
                s.SpawnStandIn29(at, strafe, zA, zB);
                if (s.standIns29.Count > 0 && (s.standIns29.Count > had || s.standIns29[s.standIns29.Count - 1].body.alive)) s.Rooms43Track(s.standIns29[s.standIns29.Count - 1].body, true); // NIGHT43 E1: a stand-in is its room's too
            }

            public int RoomAlive43(int room) => s.Rooms43Alive(room); // NIGHT43 E1: only the room's own bodies hold its door
            public int RoomStandIns43(int room) => s.Rooms43StandIns(room); // NIGHT43 E1: its stand-ins hold its next wave a while (standInHoldS)

            public bool Heal(double fraction)
            {
                if (s.health.hp >= s.health.max) return false;
                s.health.Heal(s.health.max * fraction);
                return true;
            }

            public void FallCost(double fraction) => s.health.hp = System.Math.Max(1, s.health.hp - s.health.max * fraction);

            public void FallDamage43(double hp) => s.health.hp = System.Math.Max(1, s.health.hp - hp); // NIGHT43 E1 (D-5): never a death

            public void KillAllEnemies()
            {
                s.husks.KillAll();
                s.brain.Clear();
            }

            public void LockZone(string id)
            {
                s.zones.Lock(id);
                s.nav.SetBlocked(s.zones.LockedTag); // a lock masks the zone's nav nodes (gateOpen unmasks)
            }

            public void OpenZone(string id) => s.zones.Open(id, free: true);

            public void Respawn(Vec3 at, double yawDeg) => s.Respawn(at, yawDeg);

            public void Revive()
            {
                s.health.Reset();
                s.mover.RestoreStamina();
                s.flow = Flow.Playing;
                s.downSteps = 0;
                s.lastDeath = null;
            }

            public void SetLimits(int flags) => s.CampaignSetLimits(flags);

            public void AwardWeapon(string weapon) => s.loadout.AddWeapon(weapon);

            public bool BeginBoss(out Vec3 spawn, out double yawDeg)
            {
                spawn = default;
                yawDeg = 0;
                if (!s.Wyrm24Begin()) return false; // the wyrm row off, or no files: no boss to fight
                spawn = s.Wyrm24Spawn;
                yawDeg = s.wyrm24Arena.spawnYawDeg;
                return true;
            }

            public bool BossDefeated => s.wyrm24 != null && !s.wyrm24.Alive;

            // NIGHT25 W: a boss room names its boss (RoomDef.boss); SurvivalSim.Boss25.cs starts it (or, with a live boss, restores it to its phase start)
            public bool BeginBoss(string boss, out Vec3 spawn, out double yawDeg) => s.BossRoomBegin(boss, out spawn, out yawDeg);
            public bool BossIsDefeated(string boss) => s.BossRoomDefeated(boss);
            public bool BossWalkIn => s.Boss36WalkIn; // NIGHT36 B (walkin36): the fight is in the level's last room, no Respawn (SurvivalSim.N36.cs)
        }

        partial void N24CampaignStartRun()
        {
            campaign = null;
            campaignLevel = null;
            campaignEnded = false;
            limitFlags = 0;
            limitPistolActive = false;
            limitSavedOwned.Clear();
            campaignOn = n24.campaign && setup.mode == Campaign24.Mode && map.json != null && map.json.ContainsKey("campaign");
            if (campaignOn) HookLessonCounters();
            if (campaignSpawner != null) campaignSpawner.Campaign = campaignOn;
            if (campaignOn)
            {
                if (!campaignHeldNoRecords)
                {
                    campaignPrevNoRecords = noRecords;
                    campaignHeldNoRecords = true;
                }
                noRecords = true; // SURVIVAL's records and profile are not a campaign run's
                campaignLevel = LevelDef.From(map.json);
                campaign24 = new Campaign24State { levelId = campaignLevel.id, room = 0, checkpoint = 0, limits = 0 }; // the tape (format 8) records the level the run starts on
            }
            else if (campaignHeldNoRecords)
            {
                noRecords = campaignPrevNoRecords;
                campaignHeldNoRecords = false;
            }
        }

        partial void N24CampaignStep(double dt)
        {
            if (!campaignOn) return;
            if (campaign == null)
            {
                if (!run.Live) return;
                campaign = new Campaign24(cfg, events, campaignLevel, new CampaignWorld(this), dt);
                campaign.Start(runSeed);
                campaign.pickups29 = n29.pickups; // NIGHT29 round 2: plan pickups collectable, the wall-ride room's stand-ins and the fall cost
                campaign.roomRules29 = n29.roomRules;
                campaign.teach30 = n30.teach; // NIGHT30: a room's lesson line clears when the room is cleared or left (Campaign24.DropLesson30)
                campaign.rooms43 = n43.rooms43; // NIGHT43 E1: afterLesson43, fallReturn43 and the door rules (SurvivalSim.N43.Rooms.cs)
                campaign.commitOff43 = rooms43CommitOff; // NIGHT43 E1: the harness's control (false in the game)
                N46Campaign(); // NIGHT46 (n46-shared.cjs): the night's builders set their Campaign24 hooks (SurvivalSim.N46.cs; rows off: none)
                N47Campaign(); // NIGHT47 (n47-shared.cjs): W / B / L / F set their Campaign24 hooks (SurvivalSim.N47.cs; rows off: none)
                N48Campaign(); // NIGHT48 (n48-shared.cjs): W / B / L / F set their Campaign24 hooks (SurvivalSim.N48.cs; rows off: none)
            }
            if (run.state == SurvivalRun.State.Break) run.phaseSteps = 1; // the first break is skipped: a campaign run starts in its fight
            if (flow == Flow.Down && downSteps > 0 && downSteps < 3) downSteps = 3; // the room's restart (retryDelay < health.downTime) comes first
            if (campaignEnded || flow == Flow.Results) return;
            campaign.Step();
            campaign24 = campaign.State;
            if (campaign.finished && !campaignEnded && !CampaignBigPickHold())
            {
                campaignEnded = true;
                EndRun(); // the layer is cleared: RESULTS
            }
        }

        /// <summary>PISTOL ONLY: the KESTREL alone for the room; MELEE ONLY: the gauntlet alone (the owned list is kept and restored at the door); NO DASH: the dash key is
        /// dropped from the frame (N24LimitFrame). The flags are the tape's limits.</summary>
        void CampaignSetLimits(int flags)
        {
            bool melee = (flags & Limit24.MeleeOnly) != 0;
            bool pistol = (flags & (Limit24.PistolOnly | Limit24.MeleeOnly)) != 0; // MELEE ONLY wins over PISTOL ONLY: the gauntlet alone
            if (pistol && !limitPistolActive)
            {
                limitSavedOwned.Clear();
                limitSavedOwned.AddRange(loadout.owned);
                loadout.owned.Clear();
                loadout.owned.Add(melee ? "gauntlet" : "revolver");
                loadout.SetSlot(0);
                limitPistolActive = true;
            }
            else if (!pistol && limitPistolActive)
            {
                loadout.owned.Clear();
                loadout.owned.AddRange(limitSavedOwned);
                loadout.SetSlot(0);
                limitPistolActive = false;
            }
            limitFlags = flags;
        }

        /// <summary>NO DASH: the lead calls it right after the frame is sampled (SurvivalSim.Run.cs Step).</summary>
        void N24LimitFrame(ref InputFrame frame)
        {
            if (limitFlags != 0) Limit24.Strip(ref frame, limitFlags); // NO DASH, NO SLIDE, NO JUMP: exactly that key, nothing else
            moveFrame30 = frame; // NIGHT30 (builder S): the step's move input as the Mover gets it (Glide30's steering reads it; a read, nothing changes)
        }

        /// <summary>NIGHT30: this step's move input (set right after it is sampled; SurvivalSim.Systems30.cs's glide steering reads it).</summary>
        public InputFrame moveFrame30;
    }
}
