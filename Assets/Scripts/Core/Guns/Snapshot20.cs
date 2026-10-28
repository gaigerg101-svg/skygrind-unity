// NIGHT20 fix list G2 (builder G2; RunRestoreTests.Restore_AtALaterBreak_EqualsTheUninterruptedRun: seed 37's restored run
// left the uninterrupted one at step 9801, the gauntlet's first swing after the break paid SWAP-CANCEL on one side only):
// the run snapshot's part for the NIGHT19 sandbox and tonight's guns, the loadout state that crosses a break:
// - Sandbox19: the THUNDERCLAP's compress and its special cooldown, the COMET's launched bodies (LAUNCH + SNIPE), the
//   SWAP-CANCEL clock (the last pull's gun and time, the gun a swap left), the combo count;
// - the ZAPPER's heat and alt cooldown, its stats; the BOUNCER's airbursts;
// - the THUNDERCLAP's shockwave (charges, the clock) and the KESTREL's gust (its cooldown, the air charge);
// - the zoom's descope; D1's pushed bodies (SENT's window);
// - the TORCH's heat, its tick and alt clocks, the flame group, its fire walls;
// - the VORTEX's press, repel, dash clock and stats.
// Written in the loadout's section as "guns20" (Combat.cs Save / Load); an older save without it keeps StartRun's state.
// Bodies are registry indices (StateIO.EnemyIndex / EnemyAt), as the projectiles' are.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed partial class Loadout
    {
        void SaveGuns20(StateWriter w)
        {
            // Sandbox19
            w.D("compress", compress);
            w.I("shotgunSpecialCooldown", shotgunSpecialCooldown);
            var li = new List<int>(launched.Count);
            for (int i = 0; i < launched.Count; i++) li.Add(StateIO.EnemyIndex(enemies, launched[i]));
            w.Ints("launched", li);
            w.Bools("sniped", sniped);
            w.S("lastShotWeapon", lastShotWeapon);
            w.S("swapFromWeapon", swapFromWeapon);
            w.D("lastShotAt", lastShotAt);
            w.D("swapFromShotAt", swapFromShotAt);
            w.I("combos19", combos19);
            // the ZAPPER, the BOUNCER
            w.D("zapHeat", zapHeat != null ? zapHeat.heat : 0);
            w.D("zapLock", zapHeat != null ? zapHeat.@lock : 0);
            w.I("zapAltCooldown", zapAltCooldown);
            w.Ints("zapStats", new[] { zapDischarges, zapStunned, bouncerAirbursts });
            // the shockwave, the gust, the zoom
            w.Begin("shockwave");
            shockwave.Save(w);
            w.End();
            w.Begin("gust");
            gust.Save(w);
            w.End();
            w.B("descoped", descoped);
            // D1
            var pi = new List<int>(pushed20.Count);
            for (int i = 0; i < pushed20.Count; i++) pi.Add(StateIO.EnemyIndex(enemies, pushed20[i]));
            w.Ints("pushed20", pi);
            w.Doubles("pushedAt20", pushedAt20);
            // the TORCH
            w.D("torchHeat", torchHeat != null ? torchHeat.heat : 0);
            w.D("torchLock", torchHeat != null ? torchHeat.@lock : 0);
            w.Ints("torchClocks", new[] { torchSteps, torchAltCooldown, torchGroupId, torchWalls });
            w.D("torchGroupAt", torchGroupAt);
            w.B("torchGroup", torchGroup);
            w.Begin("walls");
            for (int i = 0; i < walls.Length; i++)
            {
                FireWall wl = walls[i];
                w.Begin(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                w.B("live", wl.live);
                w.V("a", wl.a);
                w.V("b", wl.b);
                w.Ints("steps", new[] { wl.left, wl.tick, wl.shot });
                w.End();
            }
            w.End();
            // the VORTEX
            w.D("vortexPressAt", vortexPressAt);
            w.B("vortexRepel", vortexRepel);
            w.D("sinceDash", sinceDash);
            w.Ints("vortexStats", new[] { vortexCollapses, vortexRepels });
        }

        void LoadGuns20(StateReader r)
        {
            List<int> li = r.Ints("launched");
            List<bool> sn = r.Bools("sniped");
            List<int> zs = r.Ints("zapStats"), tc = r.Ints("torchClocks"), vs = r.Ints("vortexStats"), pi = r.Ints("pushed20");
            List<double> pa = r.Doubles("pushedAt20");
            if (li.Count != sn.Count || zs.Count != 3 || tc.Count != 4 || vs.Count != 2 || pi.Count != pa.Count) throw new StateException("snapshot: loadout.guns20 lists");
            compress = r.D("compress");
            shotgunSpecialCooldown = r.Int("shotgunSpecialCooldown");
            launched.Clear();
            sniped.Clear();
            for (int i = 0; i < li.Count; i++)
            {
                launched.Add(StateIO.EnemyAt(enemies, li[i]));
                sniped.Add(sn[i]);
            }
            lastShotWeapon = r.S("lastShotWeapon");
            swapFromWeapon = r.S("swapFromWeapon");
            lastShotAt = r.D("lastShotAt");
            swapFromShotAt = r.D("swapFromShotAt");
            combos19 = r.Int("combos19");
            if (zapHeat != null)
            {
                zapHeat.heat = r.D("zapHeat");
                zapHeat.@lock = r.D("zapLock");
            }
            zapAltCooldown = r.Int("zapAltCooldown");
            zapDischarges = zs[0];
            zapStunned = zs[1];
            bouncerAirbursts = zs[2];
            shockwave.Load(r.Section("shockwave"));
            gust.Load(r.Section("gust"));
            descoped = r.B("descoped");
            pushed20.Clear();
            pushedAt20.Clear();
            for (int i = 0; i < pi.Count; i++)
            {
                pushed20.Add(StateIO.EnemyAt(enemies, pi[i]));
                pushedAt20.Add(pa[i]);
            }
            if (torchHeat != null)
            {
                torchHeat.heat = r.D("torchHeat");
                torchHeat.@lock = r.D("torchLock");
            }
            torchSteps = tc[0];
            torchAltCooldown = tc[1];
            torchGroupId = tc[2];
            torchWalls = tc[3];
            torchGroupAt = r.D("torchGroupAt");
            torchGroup = r.B("torchGroup");
            StateReader ws = r.Section("walls");
            for (int i = 0; i < walls.Length; i++)
            {
                StateReader e = ws.Section(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                FireWall wl = walls[i];
                List<int> st = e.Ints("steps");
                if (st.Count != 3) throw new StateException("snapshot: loadout.guns20.walls steps");
                wl.live = e.B("live");
                wl.a = e.V("a");
                wl.b = e.V("b");
                wl.left = st[0];
                wl.tick = st[1];
                wl.shot = st[2];
            }
            vortexPressAt = r.D("vortexPressAt");
            vortexRepel = r.B("vortexRepel");
            sinceDash = r.D("sinceDash");
            vortexCollapses = vs[0];
            vortexRepels = vs[1];
        }
    }

    public sealed partial class Shockwave
    {
        /// <summary>NIGHT20 fix list G2: the run snapshot's part (the charges, the full count, the clock, the count fired).</summary>
        public void Save(StateWriter w) => w.Ints("state", new[] { charges, max, cooldownSteps, fired });

        public void Load(StateReader r)
        {
            List<int> s = r.Ints("state");
            if (s.Count != 4) throw new StateException("snapshot: shockwave state");
            charges = s[0];
            max = s[1];
            cooldownSteps = s[2];
            fired = s[3];
        }
    }

    public sealed partial class KestrelGust
    {
        /// <summary>NIGHT20 fix list G2: the run snapshot's part (the cooldown, the air charge, the count fired).</summary>
        public void Save(StateWriter w)
        {
            w.Ints("state", new[] { cooldownSteps, fired });
            w.B("airUsed", airUsed);
        }

        public void Load(StateReader r)
        {
            List<int> s = r.Ints("state");
            if (s.Count != 2) throw new StateException("snapshot: gust state");
            cooldownSteps = s[0];
            fired = s[1];
            airUsed = r.B("airUsed");
        }
    }
}
