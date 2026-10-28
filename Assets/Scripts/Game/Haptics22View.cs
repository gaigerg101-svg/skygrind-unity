// NIGHT22 U3-36.8 (builder M): HAPTICS (design.n22.haptics), the Unity half. UNCOMPILED when written (no Editor in the builder's
// worktree); the rule and its tests are Core/Haptics22.cs and Assets/Tests/EditMode/Arcline22Tests.cs, which ran headless.
// Plays Core's patterns on the existing Rumble (Game/Input/Rumble.cs: VIBRATION on, RUMBLE strength, the pad in use) for the events
// the web names: "shockwave" (the THUNDERCLAP's), "ropeAttach" and "arclinePull" (the ARCLINE's), "kill" and "death" (the player's DOWN).
// The web's hurt / parry pulses and ImpactFrames' tier pulses are unchanged; the web's kill pulse is replaced by the kill pattern while
// the row is live (Rumble.distinctKill). Render side only: nothing here reads or writes the sim; no audio.
// Wiring (the lead): SurvivalGame.N22InitViews: `haptics22 = new Haptics22View(this);` and N22RenderViews: `haptics22?.Render();`
// (a field `Haptics22View haptics22;` on SurvivalGame). The log: Haptics22View.Log (see FeelReel.N22.Haptics.cs).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Haptics22View : IHapticsSink
    {
        readonly SurvivalGame game;
        readonly Rumble rumble;
        public readonly Haptics22 haptics;

        /// <summary>The live view (the reel's haptics scenario reads its log; SurvivalGame's field for it is the lead's).</summary>
        public static Haptics22View Instance;

        public Haptics22View(SurvivalGame game)
        {
            Instance = this;
            this.game = game;
            rumble = game.rumble;
            haptics = new Haptics22(game.cfg, this);
            game.events.On<ShockwaveEvent>(N20Events.Shockwave, e => Fire("shockwave", e.owner));
            game.events.On<Arcline21RopeEvent>("ropeAttach", e => Fire("ropeAttach", e.owner));
            game.events.On("arclinePull", _ => Fire("arclinePull", null));
            game.events.On<KillEvent>("kill", e => Fire("kill", e.owner));
            game.events.On<DeathEvent>("death", e => Fire("death", e.owner));
        }

        /// <summary>The row is live for this run (design.n22.haptics.enabled; --off22 / -skygrindOff22: off).</summary>
        public bool Live => game.sim != null && game.sim.n22.haptics;

        void Fire(string eventName, string owner)
        {
            if (!Live) return;
            string pattern = Haptics22.ForEvent(eventName, owner);
            if (pattern != null) haptics.Fire(pattern);
        }

        /// <summary>Once a rendered frame: the next step of a running pattern, on the real frame time (hitstop and slow-mo do not stretch a pattern).</summary>
        public void Render()
        {
            if (rumble == null) return;
            rumble.distinctKill = Live;
            haptics.Tick(Time.unscaledDeltaTime);
        }

        // --- the sink: Rumble ---------------------------------------------------------------------------------------------------

        public bool CanPlay => rumble != null && rumble.CanPlay;

        public double Strength => rumble != null ? rumble.strength : 0;

        public void Play(double strong, double weak, double ms)
        {
            if (rumble == null) return;
            if (strong == 0 && weak == 0) rumble.Stop(); // a pause
            else rumble.Play(strong, weak, ms);
        }
    }
}
