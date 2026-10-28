// NIGHT45 builder A3 (look45, the view row design.n45.look45; DESIGN-RULES A-6, A-9; the audit: "every room's landmark is look43's same generic
// exit pylons"): a room whose level data gives it its OWN landmark (a campaign prop with job "landmark" or "hero": NIGHT45's identity cards, e.g.
// A1's b1, A3's k5) keeps that landmark alone: Look43View.BuildLandmarks skips its two generic cyan pylons by the exit. The room's own landmark is
// drawn where it always was (the props' view); this file only names the rooms. Row off (or -skygrindOff45): nothing is skipped, look43 as it was.
// Pure (no UnityEngine).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Look45
    {
        /// <summary>The prop jobs that make a room's own landmark (A-9's card fields "Landmark" and "Hero prop").</summary>
        public static readonly string[] Jobs = { "landmark", "hero" };

        /// <summary>The ids of the rooms with a landmark of their own in the level (campaign.props rows with a job in Jobs); empty when none.</summary>
        public static HashSet<string> OwnLandmarkRooms(Dictionary<string, object> mapJson)
        {
            var rooms = new HashSet<string>();
            if (mapJson == null || !mapJson.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return rooms;
            if (!camp.TryGetValue("props", out object po) || !(po is List<object> props)) return rooms;
            foreach (object o in props)
            {
                if (!(o is Dictionary<string, object> p) || !p.TryGetValue("job", out object jo) || !(jo is string job)) continue;
                if (System.Array.IndexOf(Jobs, job) < 0 || !p.TryGetValue("room", out object ro) || !(ro is string room)) continue;
                rooms.Add(room);
            }
            return rooms;
        }

        /// <summary>The rooms whose generic pylons look45 skips: null when the row is off (look43 unchanged).</summary>
        public static HashSet<string> Skip(GameConfig cfg, Dictionary<string, object> mapJson) =>
            N45View.RowOn(cfg, "look45") ? OwnLandmarkRooms(mapJson) : null;
    }
}
