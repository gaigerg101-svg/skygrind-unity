// NIGHT64 builder G (leftover 2, design.n64.fightLine64; the rules and the counter: FightLine64.N64G.cs). The HUD's half, one hook: UiHud33.cs
// RenderHud33 calls FightLine64Late() right after Fight47Late(frameDt) (hud47's queue, bossRule61's line and order64 have drawn this frame) and
// before Top52(). Row on: a room title the fight line draws while the boss bar is up gets alpha 0 this frame (the queue keeps its state), and
// is held; when the bar goes down (the fight over) each held title is posted once to the message slot (Post33, the zone banner's own time:
// Ui.cs zoneEnter's Math.Min(zoneBannerTime, Hud17ZoneBannerTime)). Row off: nothing hidden, nothing posted. The counter runs the row on or off.
// Room titles: every zoneEnter's name (subscribed once) and the campaign level's rooms' names, each as posted and without its number.
// Proof lines (Player.log; t on FeelClock, the reel's fixed clock):
//   [fight64] <boss> title "<text>" in the fight line mid-fight <n> frames: would draw <w>, drawn <d> | held for after the fight | shown as
//             before | row fightLine64 on|off | t <s> s (FeelClock)                          when a title's run in the fight line ends
//   [fight64] <boss> | row fightLine64 on <n> of <m> frames | room-title frames in the fight line: would draw <W>, drawn <D> | held <h> frames:
//             ... | titles mid-fight <k>: hidden <x>, held for after the fight <y>, posted after <p>, shown after <s> | <why>, t <s> s (FeelClock)
//                                        the totals: after each title line, at the bar's going down, a new boss, a new run and the quit
//   [fight64] title "<text>" posted to the message slot after the fight / shown after the fight (the message slot)
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        readonly FightLine64.Count f64 = new FightLine64.Count();
        readonly FightLine64.Later f64Later = new FightLine64.Later();
        readonly FightLine64.Titles f64Titles = new FightLine64.Titles();
        bool f64Wired, f64BarWas;
        object f64Level; // the campaign level whose rooms' names are in f64Titles

        /// <summary>From RenderHud33 after Fight47Late (hud47 built): the fight line's room title mid-fight, the titles held for after, the counter.</summary>
        void FightLine64Late()
        {
            FightLine64Wire();
            FightLine64Rooms();
            bool barUp = b43Root != null && b43Root.gameObject.activeSelf;
            bool rowOn = FightLine64.On(cfg); // every frame (D1's same-run toggle)
            if (!barUp)
            {
                if (f64BarWas) FightLine64FightOver(rowOn);
                f64BarWas = false;
                if (h33Slot != null && h33Slot.Alpha > 0f && f64Later.Seen(h33Slot.Shown))
                    Debug.Log("[fight64] title \"" + h33Slot.Shown + "\" shown after the fight (the message slot) | t " + FeelClock.Now.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " s (FeelClock)");
                return;
            }
            f64BarWas = true;
            string boss = string.IsNullOrEmpty(b43ShownName) ? "?" : b43ShownName;
            if (!string.Equals(boss, f64.Boss, System.StringComparison.Ordinal))
            {
                if (f64.Dirty) FightLine64Say("a new boss");
                f64.Reset(boss);
            }
            if (fight47 == null) return;
            string line = fight47Shown;
            bool queueDraws = fight47.rt.gameObject.activeInHierarchy && fight47.Alpha > 0f;
            bool isTitle = queueDraws && f64Titles.Has(line);
            bool hide = FightLine64.Hide(rowOn, true, queueDraws, isTitle);
            if (hide)
            {
                fight47.Alpha = 0f; // this frame only: Fight47Tick sets the queue's alpha again next frame (its age and turn untouched)
                Hud47Probe.FightLine = "";
                f64Later.Hidden(line);
            }
            bool would = queueDraws && isTitle;
            if (f64.Frame(FeelClock.Holding, rowOn, would, would && !hide, line))
            {
                Debug.Log(f64.TitleLine(rowOn, FeelClock.Now));
                FightLine64Say("a title's end");
            }
        }

        /// <summary>The bar went down (the fight over): a title run still open ends; each held title is posted once to the message slot.</summary>
        void FightLine64FightOver(bool rowOn)
        {
            if (f64.EndRun()) Debug.Log(f64.TitleLine(rowOn, FeelClock.Now));
            foreach (string t in f64Later.TakeForPost())
            {
                Post33(t, System.Math.Min(zoneBannerTime, UnityTuning.Hud17ZoneBannerTime)); // the zone banner's own time (Ui.cs zoneEnter)
                Debug.Log("[fight64] title \"" + t + "\" posted to the message slot after the fight (" + f64.Boss + ")");
            }
            if (f64.Dirty) FightLine64Say("the boss bar went down");
        }

        /// <summary>Once a session: the zones' names as they are entered; a new run starts clean; the quit prints what is left.</summary>
        void FightLine64Wire()
        {
            if (f64Wired || game == null || game.events == null) return;
            f64Wired = true;
            game.events.On<Skygrind.Core.ZoneEnterEvent>("zoneEnter", e => { if (e != null) f64Titles.Add(e.name); });
            game.OnNewRun += () => { if (f64.Dirty) FightLine64Say("a new run"); f64.Reset(""); f64Later.Clear(); f64BarWas = false; };
            Application.quitting += () => { if (f64.Dirty) FightLine64Say("the quit"); };
        }

        /// <summary>The campaign level's rooms' names, once per level (a reference check every frame).</summary>
        void FightLine64Rooms()
        {
            var lv = game != null && game.sim != null ? game.sim.campaignLevel : null;
            if (lv == null || ReferenceEquals(lv, f64Level)) return;
            f64Level = lv;
            if (lv.rooms != null) foreach (var r in lv.rooms) if (r != null) f64Titles.Add(r.name);
        }

        void FightLine64Say(string why)
        {
            Debug.Log(f64.Line(why, FightLine64.On(cfg), f64Later, FeelClock.Now));
            f64.Dirty = false;
        }
    }
}
