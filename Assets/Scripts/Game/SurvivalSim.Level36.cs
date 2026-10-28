// NIGHT36 builder L (NIGHT36.md L1; PLAN.md's ULTRAKILL table "arena flow"; rea-ultrakill-combat.md section 4): the ARENA FLOW of the n36 levels.
// It runs only in a campaign room whose level-file row carries `arena36: true` (Tools/night/gen-n36-foundry.cjs marks its fight rooms); every older
// level has no such room and is untouched (no bit: the level file is the switch; -skygrindOff36 / --off36 turns it off too, N36Blocks.ForceOff).
// The rules (numbers: design.n36.level36, each with its ULTRAKILL source):
//   - entering an arena room: the event arena36Lock {room, id} (the view turns its doors red; Campaign24 already locks the door behind and keeps the
//     door ahead shut until the room clears);
//   - a wave's enemies switch on one by one: the first switchOnDelayS (1 s) after the room's entry, then one every staggerS (0.1 s); each one with
//     the event arena36SpawnIn {kind, x, y, z, seconds = bubbleS} (the view's spawn bubble and light);
//   - the next wave: nextWaveS (1 s) after the last of a wave dies (Campaign24's own waveGap runs first; the rest is added before the switch-on);
//   - the room's last kill: the event arena36LastKill {seconds = lastKillSlowS, scale = lastKillSlowScale} (the Game's time layer slows the world),
//     then arena36Unlock {seconds = unlockStaggerS} as Campaign24 opens the doors.
// It rides on Campaign24's NIGHT33 hooks (waveReady33 / waveEarly33 / waveNodes33 / spawn33 / pending33): in an arena room the queue below answers;
// in any other room the hooks spawns33 set (or none) answer as before. Queued enemies count as alive (pending33): a room never clears under them.
// Timers are whole steps (MovementMath.StepsFor). No randomness (the nodes are Campaign24's own draw).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>arena36Lock / arena36SpawnIn / arena36LastKill / arena36Unlock's payload.</summary>
    public sealed class Arena36Event
    {
        public int room, index;
        public string id = "", kind = "";
        public double x, y, z, seconds, scale;
    }

    public sealed partial class SurvivalSim
    {
        const string Level36Root = "design.n36.level36.";

        struct Queue36
        {
            public string kind;
            public Vec3 at;
            public int left;
        }

        bool a36On, a36Subscribed;
        Campaign24 a36Campaign;
        LevelDef a36FlagsFor;
        bool[] a36Rooms;
        int a36SwitchSteps, a36StaggerSteps, a36RestSteps, a36GroupStep = -1, a36GroupK, a36Groups, a36Steps, a36RoomStartStep;
        double a36Bubble, a36SlowS, a36SlowScale, a36UnlockStagger;
        readonly List<Queue36> a36Queue = new List<Queue36>();
        readonly Arena36Event a36Ev = new Arena36Event();
        Vec3 a36LastSpawnAt;
        bool a36LastSpawnSeen;
        System.Func<RoomDef, int, bool> a36PrevReady, a36PrevEarly;
        System.Func<RoomDef, int, List<Vec3>> a36PrevNodes;
        System.Func<string, Vec3, bool> a36PrevSpawn;
        System.Func<int> a36PrevPending;

        /// <summary>The harness's counts this run: arena rooms entered, enemies switched on, last kills.</summary>
        public int arena36Locks, arena36SpawnIns, arena36LastKills;

        /// <summary>Is this run's level an n36 arena level (any room with arena36: true)? Valid once the campaign has started.</summary>
        public bool Arena36Level => a36On && A36Flags() != null;

        partial void N36LevelStartRun()
        {
            a36Queue.Clear();
            a36Campaign = null;
            a36FlagsFor = null;
            a36Rooms = null;
            a36GroupStep = -1;
            a36GroupK = a36Groups = a36Steps = 0;
            arena36Locks = arena36SpawnIns = arena36LastKills = 0;
            a36On = !N36Blocks.ForceOff && cfg.Has(Level36Root + "switchOnDelayS");
            if (!a36On) return;
            a36SwitchSteps = MovementMath.StepsFor(cfg.Num(Level36Root + "switchOnDelayS"), step);
            a36StaggerSteps = MovementMath.StepsFor(cfg.Num(Level36Root + "staggerS"), step);
            double gap = cfg.Has("design.n24.campaign.waveGap") ? cfg.Num("design.n24.campaign.waveGap") : 0;
            a36RestSteps = System.Math.Max(0, MovementMath.StepsFor(cfg.Num(Level36Root + "nextWaveS"), step) - MovementMath.StepsFor(gap, step));
            a36Bubble = cfg.Num(Level36Root + "bubbleS");
            a36SlowS = cfg.Num(Level36Root + "lastKillSlowS");
            a36SlowScale = cfg.Num(Level36Root + "lastKillSlowScale");
            a36UnlockStagger = cfg.Num(Level36Root + "unlockStaggerS");
            if (!a36Subscribed)
            {
                a36Subscribed = true; // once per sim; every handler checks a36On and the room's flag
                events.On<RoomEvent>("roomEnter", e => A36RoomStart(e, true));
                events.On<RoomEvent>("roomRetry", e => A36RoomStart(e, false));
                events.On<RoomEvent>("roomClear", A36RoomClear);
                events.On<SpawnEvent>("enemySpawn", e => { a36LastSpawnAt = e.position; a36LastSpawnSeen = true; });
            }
        }

        /// <summary>The level's arena36 flags by room index (null: not a campaign run, or no arena36 room: an older level).</summary>
        bool[] A36Flags()
        {
            if (campaignLevel == null) return null;
            if (a36FlagsFor == campaignLevel) return a36Rooms;
            a36FlagsFor = campaignLevel;
            a36Rooms = null;
            if (map == null || map.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is List<object> rows)) return null;
            var flags = new bool[campaignLevel.rooms.Count];
            bool any = false;
            for (int i = 0; i < rows.Count && i < flags.Length; i++)
                if (rows[i] is Dictionary<string, object> d && d.TryGetValue("arena36", out object a) && a is bool b && b) flags[i] = any = true;
            a36Rooms = any ? flags : null;
            return a36Rooms;
        }

        bool A36Room(int i)
        {
            bool[] f = a36On ? A36Flags() : null;
            return f != null && i >= 0 && i < f.Length && f[i];
        }

        bool A36Room(RoomDef r) => campaignLevel != null && A36Room(campaignLevel.rooms.IndexOf(r));

        bool A36Here => campaign != null && A36Room(campaign.room);

        void A36RoomStart(RoomEvent e, bool entered)
        {
            a36Queue.Clear();
            a36GroupStep = -1;
            a36Groups = 0;
            a36RoomStartStep = a36Steps;
            if (!A36Room(e.room)) return;
            if (entered) arena36Locks++;
            Emit36("arena36Lock", e.room, e.id, "", default, 0, 0, 0);
        }

        void A36RoomClear(RoomEvent e)
        {
            if (!A36Room(e.room) || campaignLevel == null || campaignLevel.rooms[e.room].WaveCount == 0) return;
            arena36LastKills++;
            Emit36("arena36LastKill", e.room, e.id, "", default, a36SlowS, a36SlowScale, 0);
            Emit36("arena36Unlock", e.room, e.id, "", default, a36UnlockStagger, 0, 0);
        }

        void Emit36(string name, int room, string id, string kind, Vec3 at, double seconds, double scale, int index)
        {
            a36Ev.room = room;
            a36Ev.id = id ?? "";
            a36Ev.kind = kind ?? "";
            a36Ev.x = at.x;
            a36Ev.y = at.y;
            a36Ev.z = at.z;
            a36Ev.seconds = seconds;
            a36Ev.scale = scale;
            a36Ev.index = index;
            events.Emit(name, a36Ev);
        }

        /// <summary>Hook the run's campaign (once it exists): in an arena room the queue answers, elsewhere the hooks found (spawns33's or none).</summary>
        void A36Hook()
        {
            if (campaign == a36Campaign) return;
            a36Campaign = campaign;
            if (campaign == null || A36Flags() == null) return; // an older level: the campaign's hooks stay as they are
            a36PrevReady = campaign.waveReady33;
            a36PrevEarly = campaign.waveEarly33;
            a36PrevNodes = campaign.waveNodes33;
            a36PrevSpawn = campaign.spawn33;
            a36PrevPending = campaign.pending33;
            Campaign24 c = campaign;
            c.waveReady33 = (r, w) => A36Room(r) || a36PrevReady == null || a36PrevReady(r, w);
            c.waveEarly33 = (r, w) => !A36Room(r) && a36PrevEarly != null && a36PrevEarly(r, w);
            c.waveNodes33 = (r, w) => A36Room(r) ? null : a36PrevNodes?.Invoke(r, w);
            c.spawn33 = (kind, at) => A36Here ? A36Queue(kind, at) : a36PrevSpawn != null ? a36PrevSpawn(kind, at) : c.SpawnNow33(kind, at);
            c.pending33 = () => (A36Here ? a36Queue.Count : 0) + (a36PrevPending != null ? a36PrevPending() : 0);
        }

        /// <summary>Campaign24 places a wave's enemies in one step: each call of that step is one more of the group, staggerS after the one before.</summary>
        bool A36Queue(string kind, Vec3 at)
        {
            if (a36GroupStep != a36Steps)
            {
                a36GroupStep = a36Steps;
                a36GroupK = 0;
                a36Groups++;
            }
            // the room's first wave, placed at its entry: switchOnDelayS after it; any later wave (or a first one held back by the room's stand-ins, which count
            // as alive: the KESTREL room's line) comes after Campaign24's waveGap, so it waits the rest of nextWaveS
            int baseSteps = a36Groups == 1 && a36Steps - a36RoomStartStep <= 2 ? a36SwitchSteps : a36RestSteps;
            a36Queue.Add(new Queue36 { kind = kind, at = at, left = 1 + baseSteps + a36GroupK * a36StaggerSteps }); // +1: this step's N36LevelStep (after the campaign's) counts too
            a36GroupK++;
            return true;
        }

        partial void N36LevelStep(double dt)
        {
            if (!a36On) return;
            a36Steps++;
            A36Hook();
            if (campaign == null || a36Queue.Count == 0) return;
            for (int i = 0; i < a36Queue.Count; i++)
            {
                Queue36 q = a36Queue[i];
                if (--q.left > 0) { a36Queue[i] = q; continue; }
                a36Queue.RemoveAt(i--);
                a36LastSpawnSeen = false;
                if (!campaign.SpawnNow33(q.kind, q.at)) continue;
                arena36SpawnIns++;
                Emit36("arena36SpawnIn", campaign.room, campaign.room >= 0 && campaignLevel != null ? campaignLevel.rooms[campaign.room].id : "", q.kind, a36LastSpawnSeen ? a36LastSpawnAt : q.at, a36Bubble, 0, arena36SpawnIns);
            }
        }
    }
}
