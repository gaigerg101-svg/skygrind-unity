// NIGHT35 builder T (design.n35.teach35): the harness check `teach35` (NIGHT35.md T1; the Player proof is the teach35 reel).
//   teach35 [--level <map id>] [--off]
// 1. FORMAT: every line of design.n35.teach35.lines (text, textPad, textOff) passes Teach35Text.Problem (an {action} token, a verb in capitals, at most two
//    sentences, at most maxChars once resolved with the keyboard's words), and every token names an action a key or the pad carries (resolves).
// 2. LEVEL 1: a CAMPAIGN run of the level (default n34-foundry; DOORFREE, god, a respawn into every room) with teach35 on and a FRESH profile shows every
//    `foundry` line once (by its rect, or its fallback room) and marks each on the profile; a weapon line on a weaponEarned.
// 3. ONCE PER PROFILE: the same run again on that profile shows none; the profile written and read back (Profile.ToJson / Read) still holds the ids; a
//    fresh profile shows them all again (D-7).
// 4. OFF (--off, or the --off35 switch): the block off: nothing is emitted (RED for the check: no line teaches).
// GREEN: exit 0. Prints `teach35 GREEN` / `teach35 FAIL` / `teach35 RED (off)`.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N35Teach(string cmd, string[] args, ref int code)
    {
        if (cmd != "teach35") return;
        code = Teach35Command(args);
    }

    /// <summary>The keyboard's words for a token (the view's Lesson25Aliases for the mouse buttons, else the bound key's code made readable); null: unbound.</summary>
    static string Teach35KeyWords(GameConfig cfg, string action)
    {
        if (action == "fire") return "LEFT CLICK";
        if (action == "altFire") return "RIGHT CLICK";
        if (action == "move") return "WASD";
        if (action == "look") return "MOUSE";
        if (!cfg.Has("input.bindings." + action)) return null;
        string code = cfg.Str("input.bindings." + action);
        if (code.StartsWith("Key")) return code.Substring(3);
        if (code.StartsWith("Digit")) return code.Substring(5);
        if (code == "ShiftLeft") return "L-SHIFT";
        if (code == "ControlLeft") return "L-CTRL";
        return code.ToUpperInvariant();
    }

    static string Teach35MapFile(GameConfig cfg, string id)
    {
        foreach (object o in cfg.Arr("design.maps.list"))
            if (o is Dictionary<string, object> m && m.TryGetValue("id", out object mi) && (mi as string) == id && m.TryGetValue("file", out object f)) return f as string;
        return null;
    }

    /// <summary>One campaign walk of `mapId` (DOORFREE, god, a respawn into every room, then a weaponEarned of each weapon line): the emitted lines.</summary>
    static List<Teach35Event> Teach35Walk(GameConfig cfg, string mapId, Profile profile, bool on, out string note)
    {
        note = "";
        string file = Teach35MapFile(cfg, mapId);
        if (file == null) { note = $"no design.maps.list entry for {mapId}"; return null; }
        SurvivalMap map = CampaignMapFrom(file, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true };
        sim.n25Override = new N25Blocks { tutorial = true };
        sim.setup.mode = Campaign24.Mode;
        N35Blocks b = N35Blocks.ForceOff ? N35Blocks.Off : N35Blocks.From(cfg);
        b.teach35 = on && !N35Blocks.ForceOff;
        sim.n35Override = b;
        // the sim's profile is the one it was built with: copy the test profile into it (and back out after)
        sim.profile.CopyFrom(profile);
        sim.StartRun(3535, null, null);
        for (int s = 0; s < 360; s++) { sim.doorFreeHeld = true; sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
        sim.doorFreeHeld = false;
        foreach (RoomDef r in level.rooms)
        {
            sim.Respawn(r.spawn, r.spawnYawDeg);
            for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
        }
        foreach (Teach35Line l in Teach35Table.From(cfg).lines.Where(x => x.weapon != ""))
        {
            sim.events.Emit("weaponEarned", new WeaponEarnedEvent { weapon = l.weapon, name = l.weapon, room = 0 });
            sim.Step();
        }
        note = $"level {level.id} ({level.rooms.Count} rooms), campaign {sim.campaignOn}, teach35 {sim.n35.teach35}";
        profile.CopyFrom(sim.profile);
        return new List<Teach35Event>(sim.teach35Emitted);
    }

    static int Teach35Command(string[] args)
    {
        GameConfig cfg = Config();
        bool off = Flag(args, "--off") || N35Blocks.ForceOff;
        string mapId = Opt30(args, "--level", "n34-foundry");
        Teach35Table table = Teach35Table.From(cfg);
        bool ok = true;
        // 1. FORMAT
        int formats = 0, bad = 0;
        var known = new HashSet<string>(GamepadMath.ActionNames) { "move", "look" };
        foreach (Teach35Line l in table.lines)
        {
            foreach (var (which, t) in new[] { ("text", l.text), ("textPad", l.textPad), ("textOff", l.textOff) })
            {
                if (string.IsNullOrEmpty(t)) continue;
                formats++;
                string why = Teach35Text.Problem(t, a => Teach35KeyWords(cfg, a), table.maxChars);
                foreach (string tok in Teach35Text.Tokens(t))
                {
                    bool keyOk = Teach35KeyWords(cfg, tok) != null, padOk = known.Contains(tok);
                    if (!keyOk || !padOk) why = (why == null ? "" : why + "; ") + "{" + tok + "}" + (keyOk ? "" : " has no key") + (padOk ? "" : " has no pad action");
                }
                string shown = Teach35Text.Resolve(t, a => Teach35KeyWords(cfg, a));
                Console.WriteLine($"  {(why == null ? "ok " : "BAD")} {l.id,-12} {which,-7} \"{shown}\" ({shown.Length} chars){(why == null ? "" : " <- " + why)}");
                if (why != null) bad++;
            }
        }
        if (table.lines.Count == 0) { Console.WriteLine("  no lines in design.n35.teach35.lines"); bad++; }
        Console.WriteLine($"teach35 format: {formats} texts, {bad} bad (maxChars {table.maxChars}, showS {table.showS})");
        ok &= bad == 0;
        // the trigger ids per level (builder A's rect list)
        foreach (var g in table.lines.Where(x => x.weapon == "").GroupBy(x => x.level))
            Console.WriteLine($"  ids {g.Key}: {string.Join(", ", g.Select(x => x.id + " (room " + x.room + ")"))}");
        Console.WriteLine($"  ids weapons: {string.Join(", ", table.lines.Where(x => x.weapon != "").Select(x => x.id + " (" + x.weapon + ")"))}");

        // 2. LEVEL 1 on a fresh profile
        var fresh = new Profile();
        List<Teach35Event> first = Teach35Walk(cfg, mapId, fresh, !off, out string note1);
        if (first == null) { Console.WriteLine("teach35 FAIL: " + note1); return 1; }
        string key = Teach35Table.LevelKey(mapId);
        var want = table.lines.Where(x => x.weapon == "" && x.level == key).Select(x => x.id).ToList();
        var wantW = table.lines.Where(x => x.weapon != "").Select(x => x.id).ToList();
        var got = first.Select(e => e.id).ToList();
        Console.WriteLine($"teach35 walk 1 (fresh profile): {note1}; emitted {got.Count}: {string.Join(", ", first.Select(e => e.id + "/" + e.trigger + " " + e.seconds + " s"))}");
        if (off)
        {
            bool red = got.Count == 0;
            Console.WriteLine(red ? "teach35 RED (off: no line teaches, NIGHT34's lessons only)" : "teach35 NOT RED: off and lines were emitted (the switch is blind)");
            return red ? 1 : 2;
        }
        var missing = want.Concat(wantW).Where(id => !got.Contains(id)).ToList();
        int dup = got.Count - got.Distinct().Count();
        int marked = want.Concat(wantW).Count(id => fresh.Teach35Seen(id));
        bool walk1 = missing.Count == 0 && dup == 0 && marked == want.Count + wantW.Count && want.Count > 0;
        Console.WriteLine($"  level lines {want.Count}, weapon lines {wantW.Count}: missing [{string.Join(", ", missing)}], twice {dup}, marked on the profile {marked} -> {(walk1 ? "PASS" : "FAIL")}");
        ok &= walk1;

        // 3. ONCE PER PROFILE: the same profile again, through its JSON
        Profile again = Profile.Read(fresh.ToJson());
        int kept = want.Concat(wantW).Count(id => again.Teach35Seen(id));
        List<Teach35Event> second = Teach35Walk(cfg, mapId, again, true, out _);
        bool once = second != null && second.Count == 0 && kept == marked;
        Console.WriteLine($"teach35 walk 2 (the same profile, read back from its JSON: {kept} ids kept): emitted {second?.Count ?? -1} -> {(once ? "PASS" : "FAIL")}");
        ok &= once;
        List<Teach35Event> third = Teach35Walk(cfg, mapId, new Profile(), true, out _);
        bool freshAgain = third != null && third.Count == first.Count;
        Console.WriteLine($"teach35 walk 3 (a fresh profile, D-7): emitted {third?.Count ?? -1} of {first.Count} -> {(freshAgain ? "PASS" : "FAIL")}");
        ok &= freshAgain;
        bool empty = new Profile().ToJson().IndexOf("teach35", StringComparison.Ordinal) < 0;
        Console.WriteLine($"teach35 profile bytes: a profile that saw no line writes no teach35 key: {(empty ? "PASS" : "FAIL")}");
        ok &= empty;

        Console.WriteLine(ok ? "teach35 GREEN" : "teach35 FAIL");
        return ok ? 0 : 1;
    }
}
