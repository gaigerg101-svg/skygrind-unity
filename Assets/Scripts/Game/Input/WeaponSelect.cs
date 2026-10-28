// NIGHT13 U3-26.7 (agent I; NIGHT14-RESEARCH section 2): the weapon selection of skygrind/src/input/keyboardMouse.js, pure
// (no UnityEngine): "POWER P3: the arsenal is unlimited, in purchase order. 1-9 and 0 select the first ten (a key for a
// slot you don't own yet does nothing), the wheel cycles all of them, Q goes back to the last one, and holding Tab opens
// the radial picker (the mouse points at a weapon, releasing Tab takes it) for the rest" (:17-20); the pad's Y / d-pad
// cycle (:399-402). CombatReader drives it from the devices once a rendered frame; HUD's UiRadial draws `radial`.
// Unity: the web's frame.weapon is the slot in hand; the Loadout here takes CombatInput.weapon as a slot request (-1:
// none), so a selection also sets `requested`, consumed by the next step. A slot change from outside (a wall buy
// equips it, Combat.cs) reaches the reader through Sync, as the web's selectWeapon(slot) from code does (:266-273).
// NIGHT15 U3-28.8 (builder K): the radial on the pad (Unity-only; the web's is mouse-only): OpenRadial(pad: true) marks
// it the pad's, PointRadial points it with the right stick, CloseRadial(true) takes the pick (ReaderInput.ApplyPad).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed class WeaponSelect
    {
        /// <summary>The slot the reader has in hand (keyboardMouse.js `weapon`).</summary>
        public int weapon;

        /// <summary>Q's target; -1 until the first switch (:61).</summary>
        public int lastWeapon = -1;

        /// <summary>The slot asked for since the last step (CombatInput.weapon), -1 none.</summary>
        public int requested = -1;

        /// <summary>The hold-Tab picker (mouse offset in px while open; :64): the HUD draws it.</summary>
        public RadialState radial = new RadialState { highlighted = -1 };

        double lastWheelTime = double.NegativeInfinity; // s (the event time), for the wheel cooldown
        readonly double wheelCooldown, deadZone, maxOffset; // weapons.wheelCooldown (s), weapons.radial.deadZone / maxOffset (px)

        public WeaponSelect(double wheelCooldown, double radialDeadZone, double radialMaxOffset)
        {
            this.wheelCooldown = wheelCooldown;
            deadZone = radialDeadZone;
            maxOffset = radialMaxOffset;
        }

        public static WeaponSelect From(GameConfig cfg) =>
            new WeaponSelect(cfg.Num("weapons.wheelCooldown"), cfg.Num("weapons.radial.deadZone"), cfg.Num("weapons.radial.maxOffset"));

        /// <summary>Select a real slot (slots you don't own and the current slot are ignored). selectWeapon :80-85.</summary>
        public bool Select(int slot, int slotCount)
        {
            if (slot < 0 || slot >= slotCount || slot == weapon) return false;
            lastWeapon = weapon;
            weapon = slot;
            requested = slot;
            return true;
        }

        /// <summary>
        /// The slot in hand as the loadout has it: a change the reader didn't ask for (a wall buy equips it) is the web's
        /// selectWeapon(slot) from code (:266-273: remembered as the last weapon's successor). Skipped while a request waits
        /// for its step (the loadout hasn't seen it yet).
        /// </summary>
        public void Sync(int currentSlot, int slotCount)
        {
            if (requested >= 0) return;
            if (currentSlot < 0 || currentSlot >= slotCount || currentSlot == weapon) return;
            lastWeapon = weapon;
            weapon = currentSlot;
        }

        /// <summary>A new run puts slot `slot` in hand and forgets the last weapon: selectWeapon(slot, {resetLast: true}) :267-271.</summary>
        public void ResetRun(int slot)
        {
            weapon = slot;
            lastWeapon = -1;
            requested = -1;
        }

        /// <summary>The next step's slot request (CombatInput.weapon), consumed.</summary>
        public int Consume()
        {
            int r = requested;
            requested = -1;
            return r;
        }

        /// <summary>WEAPON_KEYS (:20, :168): Digit1..Digit9 -> slots 0-8, Digit0 -> 9.</summary>
        public bool Digit(int digit, int slotCount) => Select(digit == 0 ? 9 : digit - 1, slotCount);

        /// <summary>Quick swap (back to the last weapon, SPEC 1.5): the quickSwap binding (:169) and the pad (:401-402).</summary>
        public bool QuickSwap(int slotCount) => lastWeapon >= 0 && Select(lastWeapon, slotCount);

        /// <summary>
        /// Mouse wheel cycles the real weapon slots (:183-194): `deltaY` + is down (the next slot); one step per
        /// weapons.wheelCooldown s (`time`: the event's time in s).
        /// </summary>
        public bool Wheel(double deltaY, double time, int slotCount)
        {
            if (deltaY == 0 || double.IsNaN(deltaY)) return false;
            if (time - lastWheelTime < wheelCooldown) return false;
            lastWheelTime = time;
            int n = slotCount;
            if (n <= 0) return false;
            return Select((weapon + (deltaY > 0 ? 1 : -1) + n) % n, n);
        }

        /// <summary>The pad's nextWeapon (:399).</summary>
        public bool Next(int slotCount) => Select((weapon + 1) % System.Math.Max(1, slotCount), slotCount);

        /// <summary>The pad's prevWeapon (:400).</summary>
        public bool Prev(int slotCount) => Select((weapon - 1 + slotCount) % System.Math.Max(1, slotCount), slotCount);

        /// <summary>
        /// The radial binding's keydown (:170-176): open at the centre with nothing highlighted. False if already open.
        /// NIGHT15 U3-28.8: `pad`, the pad's wheel opens it (the right stick points: PointRadial).
        /// </summary>
        public bool OpenRadial(int slotCount, bool pad = false)
        {
            if (radial.open) return false;
            radial.open = true;
            radial.x = 0;
            radial.y = 0;
            radial.highlighted = -1;
            radial.slots = slotCount;
            radial.pad = pad;
            return true;
        }

        /// <summary>
        /// NIGHT15 U3-28.8: the pad's wheel: the right stick (web axes: x right, y down, raw) past `stickDeadzone`
        /// (design.pad.wheelStickDeadzone) points at a weapon, as the mouse does (ArsenalMath.RadialSector): the pointer is
        /// the stick's tilt x weapons.radial.maxOffset (the HUD's dot shows the stick). Back inside the dead zone (a stick
        /// let go springs back first) nothing changes, so the last pick is the one the release takes. True when the
        /// highlighted sector changed.
        /// </summary>
        public bool PointRadial(double stickX, double stickY, double stickDeadzone, int slotCount)
        {
            if (!radial.open) return false;
            radial.slots = slotCount;
            double len = JsMath.Hypot(stickX, stickY);
            if (!(len > stickDeadzone)) return false;
            double k = len > 1 ? maxOffset / len : maxOffset;
            radial.x = stickX * k;
            radial.y = stickY * k;
            int sector = ArsenalMath.RadialSector(radial.x, radial.y, slotCount, deadZone);
            if (sector < 0 || sector == radial.highlighted) return false;
            radial.highlighted = sector;
            return true;
        }

        /// <summary>
        /// The radial takes the mouse (the view holds still): point at a weapon (:221-236). `dx` / `dy` the move in px, y
        /// down (movementY); the offset is clamped to weapons.radial.maxOffset; true when the highlighted sector changed.
        /// </summary>
        public bool MoveRadial(double dx, double dy, int slotCount)
        {
            if (!radial.open) return false;
            radial.x += dx;
            radial.y += dy;
            double len = JsMath.Hypot(radial.x, radial.y);
            if (len > maxOffset)
            {
                radial.x *= maxOffset / len;
                radial.y *= maxOffset / len;
            }
            radial.slots = slotCount;
            int sector = ArsenalMath.RadialSector(radial.x, radial.y, slotCount, deadZone);
            if (sector == radial.highlighted) return false;
            radial.highlighted = sector;
            return true;
        }

        /// <summary>closeRadial (:124-129): the radial key's keyup picks the highlighted slot (`pick`); a lost focus doesn't. False if not open.</summary>
        public bool CloseRadial(bool pick, int slotCount)
        {
            if (!radial.open) return false;
            radial.open = false;
            radial.pad = false;
            if (pick && radial.highlighted >= 0) Select(radial.highlighted, slotCount);
            return true;
        }
    }
}
