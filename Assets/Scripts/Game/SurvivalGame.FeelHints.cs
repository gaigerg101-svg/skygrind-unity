// NIGHT16 U3-29.8 polish #17 (the lead's; the audit's "a limit you can't see feels like a bug"): the slide's charge
// (design.move.slideCharge) is a new limit, so the first time in a session that a slide starts with its burst spent, the
// hint toast says why and where to look (the long tick under the crosshair). Read from the mover's events only (no frame
// cost): the charge counts as ready at a run's start and on `slideCharged`; a slide starting while it isn't is the moment;
// after a slide starts, it is ready again only if that slide used none (a slide entered at or above the floor). Not for the
// attract or a bot's run (metaSilent), nor with the SLIDE CHARGE row off.
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        bool feelHintChargeReady = true, feelHintChargeShown;
        string feelHintChargeText;

        /// <summary>Wired once at the game's start (StartGame); the text carries design.move's recharge time.</summary>
        void FeelHintsInit()
        {
            if (!cfg.Has("design.move.slideCharge.rechargeTime")) return; // no design.move: the web's slide, nothing to teach
            feelHintChargeText = UnityTuning.SlideChargeHint17; // NIGHT17 U3-30.8: 3 words (was a 90-character sentence)
            mover.Events += FeelHintsOnMover;
            OnNewRun += () => feelHintChargeReady = true;
        }

        void FeelHintsOnMover(Skygrind.Core.MoverEvent e)
        {
            if (e.type == Skygrind.Core.MoverEventType.SlideCharged)
            {
                feelHintChargeReady = true;
                return;
            }
            if (e.type != Skygrind.Core.MoverEventType.SlideStart) return;
            bool wasReady = feelHintChargeReady;
            feelHintChargeReady = !mover.moveBalance.spent;
            if (wasReady || feelHintChargeShown || !mover.moveBalance.rules.slideCharge) return;
            if (flow != Flow.Playing || attractOn || sim.metaSilent) return;
            feelHintChargeShown = true;
            ui?.ShowHint(feelHintChargeText);
        }
    }
}
