// NIGHT24 (builder R; design.n24.padY): the port-only event "weaponSwitch": the pad's NEXT WEAPON press (a Y tap on the v4 layout)
// moved the slot in hand. Emitted by ReaderInput.ApplyPad on the game's EventBus once the lead wires playerInput.events = events
// (SurvivalGame.Init.cs, beside new ReaderInput). It is a picture of an input, not a rule: the loadout's own "weaponSwap" still
// fires one step later when the sim equips the gun. Port-only: the web has no pad layout with Y on weapons and no such event.
namespace Skygrind.Core
{
    public sealed class WeaponSwitchEvent
    {
        public const string Name = "weaponSwitch";

        public int fromSlot, toSlot;
    }
}
