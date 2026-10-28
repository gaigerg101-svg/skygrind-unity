// NIGHT35 builder T (design.n35.teach35, NIGHT35.md T1, D-7, PRINCIPLES 2): the sim's side of THE TEACHING TEXT (the rule is Core/Lesson25.cs Teach35).
// A CAMPAIGN run with the block on (n35.teach35) takes the level's lines (design.n35.teach35.lines whose `level` is the level id's base name, and the
// first-pickup lines) and the level file's rects (campaign.teach35). Each step (N35TeachStep, after M's hook) it fires:
//   - a line whose rect holds the feet;
//   - a line with no rect in the level file on the first entry into its fallback `room` (the roomEnter event);
//   - a weapon's first-pickup line on that weapon's first weaponEarned.
// A fired line is emitted as "teach35" (Teach35Event: the format with its {action} tokens, the pad's wording, showS) and marked on the PROFILE
// (Profile.teach35: once per profile; the Game's SaveProfile writes it, guarded by profileReadOnly). A bot's run (metaSilent) still emits (the reel reads it)
// but never marks the profile. Nothing here touches the world, the player or the RNG: with the block on or off the sim's state is the same (the
// fingerprint EQUAL either way); off (the row, --off35, -skygrindOff35) nothing is emitted.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>This run's teaching triggers (null: the block off or not a campaign run).</summary>
        public Teach35 teach35;

        /// <summary>The lines' table (read once from the config).</summary>
        Teach35Table teach35Table;

        /// <summary>Everything emitted this run, in order (the harness and the reels read it).</summary>
        public readonly List<Teach35Event> teach35Emitted = new List<Teach35Event>();

        readonly List<string> teach35Rooms = new List<string>(), teach35Weapons = new List<string>();
        readonly List<Teach35Line> teach35Fired = new List<Teach35Line>();
        bool teach35Hooked;

        partial void N35TeachStartRun()
        {
            teach35 = null;
            teach35Pending = false;
            teach35Emitted.Clear();
            teach35Rooms.Clear();
            teach35Weapons.Clear();
            if (!n35.teach35) return;
            teach35Table ??= Teach35Table.From(cfg);
            teach35Pending = true; // built on the run's first step: N24StartRun (campaignOn, campaignLevel) runs AFTER N35StartRun in StartRun
            if (!teach35Hooked)
            {
                teach35Hooked = true;
                events.On<RoomEvent>("roomEnter", e => { if (n35.teach35 && e != null) teach35Rooms.Add(e.id); });
                events.On<WeaponEarnedEvent>("weaponEarned", e => { if (n35.teach35 && e != null) teach35Weapons.Add(e.weapon); });
            }
        }

        bool teach35Pending;

        /// <summary>The run's triggers, once the campaign state is known (null: not a campaign run). Reads the map's campaign block directly (no Campaign24 change).</summary>
        void Teach35Build()
        {
            teach35Pending = false;
            if (!campaignOn || map == null || map.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            string levelId = campaignLevel != null ? campaignLevel.id : map.json.TryGetValue("name", out object nm) && nm is string ns ? ns : "";
            teach35 = new Teach35(teach35Table, levelId, Teach35Table.Rects(camp), id => profile != null && profile.Teach35Seen(id), Teach35BlockOn);
        }

        /// <summary>A line's `needs`: is that NIGHT35 block on in this run?</summary>
        bool Teach35BlockOn(string block) =>
            block == "charge35" ? n35.charge35 : block == "wallride35" ? n35.wallride35 : block == "grapple35" ? n35.grapple35 : block == "teach35" && n35.teach35;

        partial void N35TeachStep(double dt)
        {
            if (teach35Pending) Teach35Build();
            if (teach35 == null)
            {
                teach35Rooms.Clear();
                teach35Weapons.Clear();
                return;
            }
            teach35Fired.Clear();
            for (int i = 0; i < teach35Rooms.Count; i++) teach35.RoomEntered(teach35Rooms[i], teach35Fired);
            teach35Rooms.Clear();
            for (int i = 0; i < teach35Weapons.Count; i++) teach35.WeaponEarned(teach35Weapons[i], teach35Fired);
            teach35Weapons.Clear();
            if (flow == Flow.Playing) teach35.Step(mover.position, teach35Fired);
            if (teach35Fired.Count == 0) return;
            bool marked = false;
            foreach (Teach35Line l in teach35Fired)
            {
                string text = teach35.Words(l, out string pad);
                if (text == null) continue;
                var e = new Teach35Event
                {
                    id = l.id, text = text, textPad = pad, seconds = teach35Table.showS,
                    trigger = !string.IsNullOrEmpty(l.weapon) ? "weapon" : teach35.HasRect(l.id) ? "rect" : "room",
                };
                teach35Emitted.Add(e);
                events.Emit("teach35", e);
                if (!metaSilent && profile != null && profile.MarkTeach35(l.id)) marked = true;
            }
            if (marked) saveProfile?.Invoke();
        }
    }
}
