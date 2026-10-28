// NIGHT43 builder F1 (design.n43.feel43, a view row: no sim bit; G19 the FUSION's picture, G20 gun hits, the charged slug, the TORCH, rumble):
// the night's feel, one owner. Built by ImpactFrames (one line there) and rendered after the camera from ImpactFrames.Render, so its kicks
// sit on top of CameraFeel's and KillFeel42's. The parts:
//   Fusion43View (Fusion43View.cs)  the FUSION burst, the word, the crush, the chroma, the camera kick + trauma (fusion43, W1's event);
//   GunFeel43 (GunFx.Hits43.cs)      gun hit stars and sparks by weapon, the THUNDERCLAP's charged slug, a boss hit while OPEN;
//   TorchFeel43 (GunFx.Torch43.cs)   the TORCH's core, flame, haze and light;
//   Feel43Haptics (Feel43Math.cs)    the rumble words on Rumble: a light tick per gun shot, the THUD for the THUNDERCLAP, the BUZZ held by
//                                    the TORCH, a heavy release for a charged shot, a dash tick, the CRACK for a FUSION / an OPEN boss hit.
// With the row off nothing is hooked and nothing is drawn (Force(false) for the reel's off pass). View only: never Core state.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 F1: a payload's field by name (cached FieldInfo), so the views read W1's slug43 / burn43 payloads (Slug43Event,
    /// Burn43Event) without naming their types: this builds on a branch with or without them. One boxing read per event (an impact, an
    /// ignition), never per step.</summary>
    public static class Ev43
    {
        static readonly System.Collections.Generic.Dictionary<(System.Type, string), System.Reflection.FieldInfo> cache =
            new System.Collections.Generic.Dictionary<(System.Type, string), System.Reflection.FieldInfo>();

        public static T Get<T>(object o, string name, T fallback)
        {
            if (o == null) return fallback;
            var key = (o.GetType(), name);
            if (!cache.TryGetValue(key, out System.Reflection.FieldInfo f))
                cache[key] = f = key.Item1.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (f == null) return fallback;
            object v = f.GetValue(o);
            return v is T t ? t : fallback;
        }
    }

    public sealed class Feel43 : IHapticsSink
    {
        readonly SurvivalGame game;
        readonly EventBus events;
        bool on, hooked;

        public readonly Fusion43View fusion;
        public readonly GunFeel43 guns;
        public readonly TorchFeel43 torch;
        public readonly Fx47View fx47; // NIGHT47 F

        /// <summary>The rumble words (their log: the feel43 reel).</summary>
        public readonly Feel43Haptics Haptics;

        /// <summary>The live Feel43 (the reel; set at construction).</summary>
        public static Feel43 Instance;

        public bool On => on;

        /// <summary>The lens (the crush frame and the chroma kick), null without a Volume.</summary>
        public PostFx Post => game.lightAndLens != null ? game.lightAndLens.post : null;

        public Feel43(SurvivalGame game, GameConfig cfg, EventBus events)
        {
            Instance = this;
            this.game = game;
            this.events = events;
            Haptics = new Feel43Haptics(this);
            fusion = new Fusion43View(game, this);
            guns = new GunFeel43(game, this);
            torch = new TorchFeel43(game);
            fx47 = new Fx47View(game); // NIGHT47 F fx47 (Fx47View.cs): reads its own row (draws nothing with it off)
            if (N43View.RowOn(cfg, "feel43")) Force(true);
        }

        /// <summary>The row on / off now (the constructor: the design row; the feel43 reel: its off / on passes). Hooks the bus once.</summary>
        public void Force(bool v)
        {
            if (v && !hooked)
            {
                hooked = true;
                events.On<Fusion43Event>(N43Events.Fusion, e => { if (on) fusion.OnFusion(e); });
                events.On<HitEvent>("hit", e => { if (on) guns.OnHit(e); });
                events.On<KillEvent>("kill", e => { if (on) guns.OnKill(e); });
                events.On<ShotEvent>("shotResult", e => { if (on) guns.OnShot(e); });
                events.On<BossEvent>("bossHit", e => { if (on) guns.OnBossHit(e); });
                events.On<FlameEvent>(G20Events.Flame, e => { if (on) torch.OnFlame(e); });
                events.On("fire", o =>
                {
                    if (!on) return;
                    string w = o as string;
                    guns.OnFire(w);
                    string word = Feel43Math.FireWord(w);
                    if (word != null) Haptics.Fire(word);
                });
                events.On<ChargeShot27Event>("chargeShot27Fire", e =>
                {
                    if (!on) return;
                    guns.OnChargeFire(e);
                    Haptics.Fire(Feel43Haptics.Release);
                });
                events.On("slug43", o => { if (on) guns.OnSlug43(o); }); // W1's slug43 (Core/Slug43.cs): the blast at the slug's end
                events.On("burn43", o => { if (on) guns.OnBurn43(o); }); // W1's torch43 (Core/Torch43.cs): a body catches fire
                HookMover();
                game.OnNewRun += NewRun;
            }
            on = v;
            if (!v) NewRun();
        }

        bool moverHooked;

        /// <summary>The dash's tick (the mover may not exist yet when ImpactFrames is built: asked again each frame until it does).</summary>
        void HookMover()
        {
            if (moverHooked || game.mover == null) return;
            moverHooked = true;
            game.mover.Events += e =>
            {
                if (on && e.type == MoverEventType.Dash) Haptics.Fire(Feel43Haptics.Dash);
            };
        }

        void NewRun()
        {
            fusion.NewRun();
            guns.NewRun();
            torch.NewRun();
            Feel43Probe.Clear();
        }

        /// <summary>Once a frame after the camera (ImpactFrames.Render): every part drawn on frame time, the rumble's next step.</summary>
        public void Render(double frameDt)
        {
            Haptics.Tick(frameDt);
            fx47.Render(frameDt); // NIGHT47 F fx47: the LT blast's look (its own row, whatever feel43's)
            if (!on) return;
            HookMover();
            double freezeLeft = game.ImpactDirector != null ? game.ImpactDirector.FreezeLeft : 0;
            fusion.Render(frameDt, freezeLeft);
            guns.Render(frameDt);
            torch.Render(frameDt);
        }

        // --- the sink: Rumble (Haptics22View's way) ---------------------------------------------------------------------------------------------

        public bool CanPlay => game.rumble != null && game.rumble.CanPlay;

        public double Strength => game.rumble != null ? game.rumble.strength : 0;

        public void Play(double strong, double weak, double ms)
        {
            Rumble r = game.rumble;
            if (r == null) return;
            if (strong == 0 && weak == 0) r.Stop();
            else r.Play(strong, weak, ms);
        }
    }
}
