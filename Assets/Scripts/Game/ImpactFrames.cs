// NIGHT16 U3-29.5 (builder I): the impact director. Core's ImpactRules (Core/ImpactRules.cs) listens to the bus (hit, kill,
// parry, roundClear, the sim's own hitstop) and decides each step's freeze and slow-mo; this class owns it for the Game's
// loop (SurvivalGame.Impact.cs): the settings into it every frame (HIT FREEZE; SLOW-MO off with REDUCED MOTION), the frame's
// time share out of it, the events (`impactFreeze`, `slowMo`: FeelKeys, reused payloads), and what a freeze shows:
//   - the world stopped: the loop's WorldDt is 0 while FreezeLeft > 0 (every world view, the enemies' hit flash included:
//     EnemyViews count it down on the dt they're given, SurvivalGame.Render.cs:50-52);
//   - the hit marker on the freeze's first frame: Ui.cs's `hit` handler sets it inside the step (Ui.cs:463-481) and the HUD
//     draws it on frame time in the same frame (Ui.cs:1240-1247), before the first frozen frame;
//   - a render-only FOV punch-in on the web's FOV kick stack (below), for the non-melee moments;
//   - a low-alpha WHITE flash on a headshot kill (the parry's and the perfect parry's are the HUD's own, UiFeel.cs:24-37);
//     every flash x settings.flashIntensity, and every flash asks the game's one gate (Game/FlashGate.cs: at most three in any
//     second, WCAG 2.3.1; the HUD's parry flash asks it too);
//   - a pad pulse (Rumble.Play: x RUMBLE, only with VIBRATION on and the pad in use).
// The look stays live (the loop polls the devices every frame, frozen or not); a press waits in the readers' latches for
// the next step (Tests/EditMode/Game/ImpactLatchTests.cs). Time and pictures only: never Core state (the composed
// fingerprint stays EQUAL); the sim's own hitstop is drained by SurvivalGame.Impact.cs (pacing, see there).
//
// The web's FOV kick stack (skygrind/src/player/player.js :166-212, :229-231, :490; not ported before tonight: CameraFeel.cs's
// header), ported here line for line on the lead's request so the camera's FOV kicks have one owner: a multikill widens the
// FOV a touch, a melee hit narrows it (the lunge), a rank-up widens it; frame time; applied after the viewmodel's FOV.
//
// NIGHT18 U3-31.3 (builder X): the freeze ladder (NIGHT18.md M4) is design.impact.freeze; NIGHT17's table is kept as
// design.impact.n17.freeze. The rules are built from the FeelAB side's table and swap it in place when FeelAB.Version moves
// (BeginFrame: F6 / the LAB row in a developer build; -skygrindFeelAB off pins the A side for a reel).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed class ImpactFrames
    {
        readonly SurvivalGame game;
        readonly GameConfig cfg;
        readonly EventBus events;

        /// <summary>The rules (Core): the freeze, the slow-mo, the budget, the ledger.</summary>
        public readonly ImpactRules rules;

        readonly ImpactFreezeEvent freezeEvent = new ImpactFreezeEvent { owner = "player" };
        readonly SlowMoEvent slowMoEvent = new SlowMoEvent { owner = "player" };

        // --- the web's FOV kick stack (player.js) --------------------------------------------------------------------------
        // NIGHT5 F4: a multikill widens the FOV a touch; a melee hit narrows it (the lunge); the DISCUS catch shakes a little.
        readonly KickStack fovKick = new KickStack(4); // deg, + widens (player.js :167)
        double lastKillAt = double.NegativeInfinity; // :168
        int lastLungeShot = -1; // :169
        double feelClock; // :145 s of frame time
        readonly double multikillWindow, fovPunchDeg, fovPunchAttack, fovPunchRecover, lungeDeg, lungeAttack, lungeRecover;
        int lastRank; // the port's rankChange carries the rank only (StyleMeter.cs:61): up = higher than the last one

        // --- the flash -----------------------------------------------------------------------------------------------------
        Canvas flashCanvas;
        Image flashImage;
        double flashLeft;
        float shownFlash = -1;

        public ImpactFrames(SurvivalGame game, GameConfig cfg, EventBus events)
        {
            this.game = game;
            this.cfg = cfg;
            this.events = events;
            rules = new ImpactRules(cfg, FreezeTableFor(FeelAB.On)); // NIGHT18 U3-31.3: tonight's ladder, or NIGHT17's table (FeelAB off)
            feelAbSeen = FeelAB.Version;
            rules.Watch(events);
            multikillWindow = cfg.Num("style.multikillWindow");
            fovPunchDeg = cfg.Num("feel.fovPunch.deg");
            fovPunchAttack = cfg.Num("feel.fovPunch.attack");
            fovPunchRecover = cfg.Num("feel.fovPunch.recover");
            lungeDeg = cfg.Num("feel.meleeLunge.deg");
            lungeAttack = cfg.Num("feel.meleeLunge.attack");
            lungeRecover = cfg.Num("feel.meleeLunge.recover");
            events.On<KillEvent>("kill", OnKill);
            events.On<HitEvent>("hit", OnHit);
            events.On("rankChange", OnRankChange);
            game.OnNewRun += NewRun;
            lastRank = game.style != null ? game.style.rank : 0;
            killFeel42 = new KillFeel42(game, cfg, events); // NIGHT42 F1 (design.n42.kills42): the gauntlet's ring and kick, the kill's burst; nothing with the row off
            feel43 = new Feel43(game, cfg, events); // NIGHT43 F1 (design.n43.feel43): the FUSION, gun hits, the slug, the TORCH, rumble words; nothing with the row off
            events.On("wick47CorePunch", o => { if (Fx47View.RowOn(cfg)) rules.RequestSlowMo(SlowMoCause.CorePunch); }); // NIGHT47 F fx47 (G7, W's hand-off): the core punch's short slow motion
        }

        /// <summary>NIGHT43 F1: the night's feel (Game/Feel43.cs).</summary>
        public readonly Feel43 feel43;

        /// <summary>NIGHT42 F1: the hit / kill picture of impact42's events (Game/KillFeel42.cs).</summary>
        public readonly KillFeel42 killFeel42;

        /// <summary>NIGHT18 U3-31.3: NIGHT17's freeze table (FeelAB off: the reel's A side).</summary>
        public const string FreezeTableN17 = "design.impact.n17.freeze.";

        /// <summary>NIGHT18: the freeze table for a FeelAB side (on: design.impact.freeze, off: design.impact.n17.freeze).</summary>
        public static string FreezeTableFor(bool on) => on ? ImpactRules.FreezeTable : FreezeTableN17;

        int feelAbSeen; // NIGHT18: FeelAB.Version the rules' table was read for (a flip swaps it in place)

        /// <summary>The freeze left (real s): the render half freezes on it.</summary>
        public double FreezeLeft => rules.FreezeLeft;

        // player.js :170 emphasis: none with reduced motion or effects at 0.
        bool Emphasis() => !cfg.movement.settings.reducedMotion && cfg.movement.settings.effectsIntensity > 0;

        // player.js :171-176: a kill within style.multikillWindow of the last widens the FOV (the trauma is the port's Trauma).
        void OnKill(KillEvent e)
        {
            if (e.owner != "player") return;
            if (feelClock - lastKillAt <= multikillWindow && Emphasis()) fovKick.Add(fovPunchDeg, fovPunchAttack, fovPunchRecover);
            // NIGHT51 crunch51: a plain gun kill punches in too (a headshot / punch kill has its own via its freeze tier). On the kill, not its
            // freeze's tier: the same step's body hit outranks the Kill tier, so Show never saw a plain kill (n51-r2 kill12: fov stayed 74).
            else if (Night51.On && !e.headshot && !e.melee && Emphasis())
                fovKick.Add(-UnityTuning.N51KillPunchInDeg, UnityTuning.ImpactPunchAttack, UnityTuning.N51KillPunchHoldS + UnityTuning.ImpactPunchRelease);
            lastKillAt = feelClock;
        }

        // player.js :177-182: a melee hit narrows it, once per swing (the shot id).
        void OnHit(HitEvent e)
        {
            if (e.owner != "player") return;
            if (!e.melee || e.shot == lastLungeShot || !Emphasis()) return;
            lastLungeShot = e.shot;
            fovKick.Add(-lungeDeg, lungeAttack, lungeRecover);
        }

        // player.js :208-213: a rank-up widens it (the pitch kick is CameraFeel's).
        void OnRankChange(object o)
        {
            int rank = o is int r ? r : lastRank;
            bool up = rank > lastRank;
            lastRank = rank;
            if (up && Emphasis()) fovKick.Add(fovPunchDeg, fovPunchAttack, fovPunchRecover); // NIGHT5 F4
        }

        /// <summary>A new run (SurvivalGame.OnNewRun): the rules' clock, budget and slow-mo, the kicks, the flash.</summary>
        void NewRun()
        {
            rules.NewRun();
            fovKick.Clear(); // player.js :490 (respawn)
            lastKillAt = double.NegativeInfinity;
            lastLungeShot = -1;
            lastRank = game.style != null ? game.style.rank : 0;
            flashLeft = 0;
        }

        /// <summary>Once a frame while the sim runs: the settings in, the frame's share of time out (0 frozen).</summary>
        public double BeginFrame(double frameDt)
        {
            SettingsView v = game.settings.View;
            if (feelAbSeen != FeelAB.Version)
            {
                // NIGHT18 U3-31.3: F6 / the LAB row flipped FeelAB: the other freeze table, in place (the budget and cooldowns stay)
                feelAbSeen = FeelAB.Version;
                rules.UseFreezeTable(cfg, FreezeTableFor(FeelAB.On));
            }
            // NIGHT16 review (finding 2): automation (noLab: the smoke, the AUTOPILOT, the farm, the strips; not the attract,
            // which keeps its viewer's settings) plays design.impact.settings' defaults whatever the player saved.
            rules.ApplySettings(game.noLab && !game.attractOn, v.hitFreeze, v.slowMo, v.reducedMotion);
            rules.crunch36 = game.sim != null && game.sim.n36.crunch36; // NIGHT36 F crunch36: the run's bit (body-hit freezes, the 0.1 s punch)
            rules.impact42 = game.sim != null && game.sim.n42.impact42; // NIGHT42 F1 impact42: the run's bit (the gauntlet's tier, the longer freezes)
            rules.ClearPending(); // a moment between frames (outside a step) earns nothing
            return rules.BeginFrame(frameDt, game.flow != Flow.Playing);
        }

        /// <summary>After each sim step: the step's moments resolved; true when a freeze begins (the loop stops stepping).</summary>
        public bool StepEnded(double simHitstop)
        {
            bool froze = rules.EndStep(game.flow != Flow.Playing, simHitstop);
            if (rules.skippedMask != 0)
            {
                for (int t = 1; t < ImpactRules.TierCount; t++)
                {
                    if ((rules.skippedMask & (1 << t)) == 0) continue;
                    freezeEvent.cause = ImpactRules.TierNames[t];
                    freezeEvent.seconds = 0;
                    freezeEvent.skipped = true;
                    events.Emit(FeelKeys.ImpactFreeze, freezeEvent);
                }
            }
            if (froze)
            {
                ImpactTier t = rules.granted;
                freezeEvent.cause = ImpactRules.TierNames[(int)t];
                freezeEvent.seconds = rules.grantedSeconds;
                freezeEvent.skipped = false;
                events.Emit(FeelKeys.ImpactFreeze, freezeEvent);
                Show(t, rules.grantedSeconds);
            }
            if (rules.slowMoStarted != SlowMoCause.None)
            {
                slowMoEvent.cause = ImpactRules.SlowMoNames[(int)rules.slowMoStarted];
                slowMoEvent.scale = rules.slowMoScale;
                slowMoEvent.seconds = rules.slowMoHold;
                events.Emit(FeelKeys.SlowMo, slowMoEvent);
            }
            return froze;
        }

        /// <summary>What a granted freeze shows beyond the stopped world: the punch-in, the flash, the pulse.</summary>
        void Show(ImpactTier t, double seconds)
        {
            // NIGHT43 F1 (W1's hand-off): W1's FUSION tier (ImpactTier.Fusion43, cause "fusion43") takes the strongest path: the full punch-in and the
            // perfect-parry rumble; with feel43 on the rumble is Feel43's CRACK (asked on the fusion43 event, this would cut its first step). By its name, so
            // this line builds before and after W1's enum value lands.
            bool fusion43 = (int)t < ImpactRules.TierNames.Length && ImpactRules.TierNames[(int)t] == N43Events.Fusion;
            double deg = t == ImpactTier.HeadshotKill || t == ImpactTier.Parry || t == ImpactTier.PerfectParry || t == ImpactTier.Gauntlet42 || fusion43 ? UnityTuning.ImpactPunchInDeg // NIGHT42 F1: the gauntlet's full punch-in
                : t == ImpactTier.Headshot || t == ImpactTier.HeavyHit ? UnityTuning.ImpactPunchInLightDeg : 0;
            if (deg > 0 && Emphasis()) fovKick.Add(-deg, UnityTuning.ImpactPunchAttack, seconds + UnityTuning.ImpactPunchRelease);
            // NIGHT17 round 3: no white flash on a headshot kill any more: X's crush frame (PostFx: the world 2 EV down, the
            // victim's blocks white-hot) owns that moment, and round 2's reviewer asked that nothing lighten the whole screen.
            // Flash() stays for any later caller.
            Rumble rumble = game.rumble;
            if (rumble == null) return;
            double ms = Math.Max(UnityTuning.ImpactRumbleMinMs, seconds * 1000);
            if (fusion43 && feel43 != null && feel43.On) { } // NIGHT43 F1: Feel43's CRACK owns the FUSION's rumble
            else if (t == ImpactTier.PerfectParry || t == ImpactTier.Gauntlet42 || fusion43) rumble.Play(UnityTuning.ImpactRumblePerfectStrong, UnityTuning.ImpactRumblePerfectWeak, ms); // NIGHT42 F1: the gauntlet's hit rumbles as the perfect parry
            else if (t == ImpactTier.Headshot || t == ImpactTier.HeadshotKill || t == ImpactTier.HeavyHit || t == ImpactTier.PunchHit || t == ImpactTier.PunchKill)
                rumble.Play(UnityTuning.ImpactRumbleStrong, UnityTuning.ImpactRumbleWeak, ms);
            // a kill's and a plain parry's pulse stay the web's (SurvivalGame.Init.cs rumble.Kill / rumble.Parry)
        }

        double FlashIntensity()
        {
            double f = game.settings.View.flashIntensity;
            return f < 0 ? 0 : f > 1 ? 1 : f;
        }

        void Flash()
        {
            if (FlashIntensity() <= 0) return; // FLASH INTENSITY 0: no flash (the freeze stays)
            // NIGHT16 review (finding 7): the game's one flash gate (Game/FlashGate.cs: three in any second, every flash of the
            // night, the HUD's parry flash included); refused: the luminance pop only is dropped, the freeze stays.
            if (!game.flashGate.TryFlash(SurvivalGame.RealNow)) return;
            flashLeft = UnityTuning.ImpactFlashTime;
        }

        /// <summary>The render half (after the camera and the viewmodel): the FOV kicks and the flash, on frame time.</summary>
        public void Render(double frameDt)
        {
            feelClock += frameDt; // player.js :411
            fovKick.Advance(frameDt); // :230
            double kick = fovKick.Value();
            if (kick != 0 && game.cam != null)
            {
                // :231 `+ fovKick.value() * intensity` (intensity: settings.effectsIntensity within feel.effectsIntensityMax, :391)
                double intensity = Math.Max(0, Math.Min(cfg.movement.feel.effectsIntensityMax, cfg.movement.settings.effectsIntensity));
                game.cam.fieldOfView += (float)(kick * intensity * CameraFeel.FovKickScale(game.settings)); // NIGHT18: FOV KICK % (M's composition point)
            }
            killFeel42?.Render(frameDt); // NIGHT42 F1 (kills42): the gauntlet's camera kick and the rings, after the camera
            feel43?.Render(frameDt); // NIGHT43 F1 (feel43): the FUSION, the gun reads, the TORCH, the rumble words' next step
            flashLeft = Math.Max(0, flashLeft - frameDt);
            float a = flashLeft > 0 ? (float)(UnityTuning.ImpactFlashAlpha * (flashLeft / UnityTuning.ImpactFlashTime) * FlashIntensity()) : 0;
            // NIGHT17: the flash's canvas is built on the first render frame (at load), not on the first headshot kill (X's
            // review of round 2's first-kill hitch: a canvas made on the kill frame costs that frame).
            if (flashCanvas == null) BuildFlash();
            if (a == shownFlash) return;
            shownFlash = a;
            flashImage.color = UiTheme.A(Color.white, a);
            flashCanvas.enabled = a > 0;
        }

        void BuildFlash()
        {
            var go = new GameObject("Impact flash", typeof(Canvas));
            flashCanvas = go.GetComponent<Canvas>();
            flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flashCanvas.sortingOrder = UnityTuning.ImpactFlashSortingOrder;
            var img = new GameObject("Flash", typeof(RectTransform), typeof(Image));
            img.transform.SetParent(go.transform, false);
            var rt = (RectTransform)img.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            flashImage = img.GetComponent<Image>();
            flashImage.raycastTarget = false;
            flashImage.color = new Color(1, 1, 1, 0);
        }
    }
}
