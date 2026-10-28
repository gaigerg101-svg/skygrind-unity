// NIGHT16 U3-29.4 (builder M owns this file): the slide made a skill (Core/MoveBalance.cs) in the sim. design.move is read
// once (MoveBalanceInit, in the constructor); each run sets the Mover's rules from the LAB tab's movement rows
// (FeelKeys.MoveSlideChargeSetting / MoveSlideDecaySetting / MoveHopFalloffSetting, on by default) or, for automation
// (noLab: the smoke, the AUTOPILOT, the farm, the fingerprint), design.move's defaults; a tape's replay forces the flags it
// recorded (moveBalanceOverride). The rules and their clocks live in the Mover (Mover.moveBalance), so the run snapshot
// saves them with the mover (MoverState.Fields) and a resumed run keeps the flags it started with. The mover's port-only
// SlideCharged reaches the bus as `slideCharged` (payload: the owner id "player"). No allocation per step.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>design.move, read once (Params.Neutral when the design config has no move block: the web's mover).</summary>
        MoveBalance.Params moveBalanceDesign;

        /// <summary>A tape's replay: the three flags its run read (slideCharge, slideDecay, hopFalloff); null: the settings or the defaults.</summary>
        public bool[] moveBalanceOverride;

        /// <summary>The flags this run read (slideCharge, slideDecay, hopFalloff): what a tape records.</summary>
        public bool MoveBalanceFlag(int i) => i == 0 ? mover.moveBalance.rules.slideCharge : i == 1 ? mover.moveBalance.rules.slideDecay : mover.moveBalance.rules.hopFalloff;

        partial void MoveBalanceInit()
        {
            moveBalanceDesign = MoveBalance.Params.From(cfg);
            mover.Events += MoveBalanceOnMover;
        }

        void MoveBalanceOnMover(MoverEvent e)
        {
            if (e.type == MoverEventType.SlideCharged) events.Emit(FeelKeys.SlideCharged, "player");
        }

        partial void MoveBalanceStartRun(bool fromSettings)
        {
            MoveBalance.Params d = moveBalanceDesign;
            if (d.rechargeSteps == 0 && d.sustainSteps == 0 && moveBalanceOverride == null)
            {
                mover.SetMoveBalance(MoveBalance.Params.Neutral); // no design.move block: the web's mover whatever the rows say
                return;
            }
            bool charge = d.slideCharge, decay = d.slideDecay, hop = d.hopFalloff;
            if (moveBalanceOverride != null)
            {
                charge = moveBalanceOverride[0];
                decay = moveBalanceOverride[1];
                hop = moveBalanceOverride[2];
            }
            else if (fromSettings)
            {
                charge = SettingOr(FeelKeys.MoveSlideChargeSetting, charge);
                decay = SettingOr(FeelKeys.MoveSlideDecaySetting, decay);
                hop = SettingOr(FeelKeys.MoveHopFalloffSetting, hop);
            }
            mover.SetMoveBalance(d.With(charge, decay, hop));
        }
    }
}
