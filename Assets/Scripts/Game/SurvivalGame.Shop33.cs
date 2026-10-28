// NIGHT33 P (DESIGN33 card P, D-6, row design.n33.shop33): the game half of the shop.
// - Before every run (SurvivalGame.StartRun calls N33ShopBeforeRun: the lead's line from Tools/night/n33p-startrun.cjs) a CAMPAIGN run with the row on
//   takes the owned / equipped upgrades (sim.shop33Owned, which the tape records); every other run takes none. bank33 stays 0: the run's own points are
//   NIGHT32's (the lead, 11:04: the WALLET is a separate campaign currency).
// - At the level's end (layerClear) its earned points are banked into the wallet BEST-OF per level (Shop33.Bank: only what beats the level's best earn, so
//   a replay never earns twice), saved at once (quitting from RESULTS keeps it). Shop33.LastBanked / LastEarned / Wallet are the RESULTS view's numbers.
// - CONTINUE on the RESULTS (ContinueCampaign -> N33ShopBeforeContinue) opens the SHOP (UiShop33.cs) once per cleared level when there is a next level;
//   the shop's CONTINUE calls ContinueCampaign again, which then goes on.
// - AUTOMATION never writes the wallet or the upgrades (the lead, 11:04): a process with a -skygrind argument (reels, the smoke, LevelSelect29, the
//   batch tools; only the -skygrindOffNN switches are not automation here) or SurvivalGame.Shop33NoSave (a reel in the Editor) keeps them in an
//   in-memory copy of the profile's shop fields that never reaches disk; the real profile is left as it was.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        bool shop33GameWired;

        /// <summary>A reel or a test sets this: the shop's wallet and upgrades live in memory only (never saved), whatever the command line says.</summary>
        public static bool Shop33NoSave;

        static bool? shop33AutoArg;
        Profile shop33Mem;

        /// <summary>The SHOP screen (made on first use).</summary>
        public UiShop33 Shop33Ui { get; private set; }

        /// <summary>Has this run's cleared level had its SHOP visit (its CONTINUE pressed)? Reset at every run's start.</summary>
        public bool Shop33Visited { get; private set; }

        /// <summary>The reel `shop33` turns the shop on whatever the design row says (it also hands the sim shop33 through n33Override); never with -skygrindOff33.</summary>
        public static bool Shop33ReelForce;

        /// <summary>Is the shop on for this process (the design row or the reel's force, never with -skygrindOff33)?</summary>
        public bool Shop33On => !N33Blocks.ForceOff && (Shop33ReelForce || N33Blocks.From(cfg).shop33);

        /// <summary>Is this automation (the wallet never saved)?</summary>
        public static bool Shop33Automation
        {
            get
            {
                if (Shop33NoSave) return true;
                if (shop33AutoArg == null)
                {
                    bool auto = false;
                    foreach (string a in Environment.GetCommandLineArgs())
                        if (a.StartsWith("-skygrind", StringComparison.Ordinal) && !a.StartsWith("-skygrindOff", StringComparison.Ordinal)) auto = true;
                    shop33AutoArg = auto;
                }
                return shop33AutoArg.Value;
            }
        }

        /// <summary>Where the wallet, the upgrades and the per-level bests live: the profile, or (automation) an in-memory copy of its shop fields.</summary>
        public Profile Shop33Store
        {
            get
            {
                if (!Shop33Automation) return profile;
                if (shop33Mem == null)
                {
                    shop33Mem = new Profile();
                    if (profile != null)
                    {
                        shop33Mem.bank33 = profile.bank33;
                        shop33Mem.shop33 = profile.shop33;
                        foreach (var kv in profile.banked33) shop33Mem.banked33[kv.Key] = kv.Value;
                    }
                }
                return shop33Mem;
            }
        }

        /// <summary>The wallet now.</summary>
        public int Shop33Wallet => Shop33Store != null ? Shop33Store.bank33 : 0;

        /// <summary>The owned / equipped upgrades now (Shop33Catalogue's encoding).</summary>
        public int Shop33Code => Shop33Store != null ? Shop33Store.shop33 : 0;

        /// <summary>The catalogue (the sim's, read once).</summary>
        public Shop33Catalogue Shop33Cat => sim.shop33Catalogue ??= Shop33Catalogue.From(cfg);

        void Shop33Save()
        {
            if (Shop33Automation) return; // automation: the in-memory copy only
            SaveProfile();
        }

        /// <summary>Buy item i from the wallet (owned and equipped). False: nothing changed.</summary>
        public bool Shop33Buy(int i)
        {
            Profile s = Shop33Store;
            if (s == null) return false;
            int code = s.shop33, wallet = s.bank33;
            if (!Shop33Cat.TryBuy(ref code, ref wallet, i)) return false;
            s.shop33 = code;
            s.bank33 = wallet;
            Shop33Save();
            Debug.Log($"[skygrind] shop33: bought {Shop33Cat.items[i].id} for {Shop33Cat.items[i].price} (wallet {wallet}, code {code})");
            return true;
        }

        /// <summary>Equip an owned item (its slot's oldest comes off when full), or take an equipped one off.</summary>
        public bool Shop33Toggle(int i)
        {
            Profile s = Shop33Store;
            if (s == null || !Shop33Catalogue.Owns(s.shop33, i)) return false;
            int code = Shop33Cat.Toggle(s.shop33, i);
            if (code == s.shop33) return false;
            s.shop33 = code;
            Shop33Save();
            return true;
        }

        /// <summary>Called right before sim.StartRun (SurvivalGame.StartRun): the run's shop inputs. Only a campaign run with the row on carries the
        /// upgrades; anything else starts with none (so a survival run, the attract or a reel's survival run is unchanged). bank33 stays 0.</summary>
        void N33ShopBeforeRun()
        {
            if (!shop33GameWired)
            {
                shop33GameWired = true;
                sim.events.On<RoomEvent>("layerClear", Shop33GameOnLayerClear);
            }
            Shop33Visited = false;
            if (Shop33Ui != null) Shop33Ui.Close();
            bool camp = Shop33On && setup.mode == RunSetup.Campaign && Shop33Store != null;
            sim.shop33Owned = camp ? Shop33Cat.Sanitize(Shop33Code) : 0;
            sim.bank33 = 0;
        }

        /// <summary>The level is cleared: bank its earned points into the wallet (best-of per level) and save (not under automation).</summary>
        void Shop33GameOnLayerClear(RoomEvent e)
        {
            if (!sim.n33.shop33 || !sim.campaignOn) return;
            Profile s = Shop33Store;
            if (s == null) return;
            string level = sim.campaignLevel != null ? sim.campaignLevel.id : MapId;
            int before = s.bank33;
            Shop33.LastEarned = Shop33.Whole(sim.economy.earned);
            Shop33.LastBanked = Shop33.Bank(s, level, sim.economy.earned);
            Shop33.Wallet = s.bank33;
            Shop33Save();
            Debug.Log($"[skygrind] shop33: {level} earned {Shop33.LastEarned}, banked {Shop33.LastBanked} (wallet {before} -> {s.bank33}){(Shop33Automation ? " [automation: not saved]" : "")}");
        }

        /// <summary>NIGHT33 P: CONTINUE on the RESULTS opens the SHOP once per cleared level when a next level exists (the campaign's last level has no shop:
        /// its points are still banked). The shop's CONTINUE marks the visit and calls ContinueCampaign again.</summary>
        partial void N33ShopBeforeContinue(string next, ref bool opened)
        {
            if (!Shop33On || next == null || Shop33Visited) return;
            if (Shop33Ui == null)
            {
                Shop33Ui = new GameObject("UiShop33").AddComponent<UiShop33>();
                Shop33Ui.Init(this);
            }
            if (!Shop33Ui.IsOpen) Shop33Ui.Open(next);
            opened = true;
        }

        /// <summary>The shop's CONTINUE: the visit is done; the campaign goes on to `next` (the scene reloads, or a reel reads PendingMapId).</summary>
        public string Shop33Continue()
        {
            Shop33Visited = true;
            if (Shop33Ui != null) Shop33Ui.Close();
            return ContinueCampaign();
        }
    }
}
