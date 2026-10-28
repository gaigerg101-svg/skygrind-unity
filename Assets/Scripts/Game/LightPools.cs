// Dynamic lights (NIGHT12 U3-25.2), after skygrind/src/fx/lightPool.js and combat/effects.js's light (POLISH V4):
// - The pool (lightPool.js): a fixed lightPool.max point lights for blasts (lightPool.blast on rocketBlast / grenadeBlast,
//   main.js) and the steady glows. They always exist (off while dark), shadows off. A flash (a colour, an intensity, a
//   reach, a life) takes the free or the weakest slot and fades out ((1 - t)^2); the slots left over each frame go to the
//   steady sources nearest the camera (sorted by xz distance), faded by distance between lightPool.steadyNear and
//   steadyFar. The quality preset caps the slots used (quality.presets.*.lights: Low 0, Medium 1, High 3).
//   Steady sources: the perk machines (perks.js glowSpots: perks.<id>.color, lightPool.machine intensity / distance, its
//   height over the cabinet's feet; MEND rides its pillar); port-only, the same machine numbers in cyan (colors.usable:
//   things to use) or the relay's colour: THE FORGE (forge.color), the Mystery Box (game.box), the PROTOCOL's relays
//   (protocol.color, protocol.litColor once lit; the secret relay stays dark), the exfil radio (design.exfil) and the power
//   switch until it is thrown (design.power).
// - The player's own light (effects.js): the muzzle flash (fx.flashLightIntensity / flashLightDistance on "fire", the
//   gun family's colour (weapons.families), else colors.muzzleFlash; fx.flashFrames render frames, the second at
//   fx.flashSecondFrame) about the muzzle (the camera + its forward x UnityTuning.FxMuzzleDepth + fx.flashForward), and
//   the pops that borrow it while no flash needs it (fx.popTime / popIntensity / popDistance, fading k^2): every kill
//   (colors.enemy), a rocket blast (lightPool.blast.color: main.js pops 0xffa040, the same hex) and, port-only
//   (UnityTuning.ImpactPops), a world impact in the gun's family colour.
// - The first-run guide (port-only, NIGHT12 onboarding): one cyan pool pulsing at a point (Guide).
// three.js lights divide by pi where URP doesn't: every intensity x UnityTuning.LightScale. Their falloff is the same
// curve (decay 2 with a cutoff distance = URP's 1 / d^2 with its smooth range window). World time (the world's dt: hitstop
// and pause freeze the flashes); nothing is allocated per frame.
// NIGHT13 U3-26.4 (PerfSwitches lights, on by default): each pool slot's and the flash light's final intensity is picked
// first and applied once a frame, so no Light is disabled and re-enabled within a frame (the steady lights were turned
// off and back on every frame: a Behaviour disable / enable each). The lights end each frame in the same state as before.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class LightPools // NIGHT52 guns52: partial (LightPools.Guns52.cs, the muzzle light in real time)
    {
        sealed class Slot
        {
            public Light light;
            public Transform t;
            public double age = double.PositiveInfinity, life = 1, peak;
            public bool steady;
        }

        struct Source
        {
            public Vector3 at;
            public Color color;
            public float key; // xz distance squared to the camera (the sort)
        }

        struct Machine
        {
            public Vec3 pos;
            public bool core;
            public Color color;
        }

        readonly SurvivalGame game;
        readonly Slot[] slots;
        int enabled;
        readonly Machine[] machines;
        readonly Source[] steady;
        int steadyCount;
        readonly double steadyNear, steadyFar;
        readonly float machineIntensity, machineDistance, machineHeight;
        readonly Color blastColor, enemyColor, usable, muzzleColor, relayColor, relayLit;
        readonly double blastIntensity, blastDistance, blastLife;

        // effects.js's light: the muzzle flash, then the pops.
        readonly Light flash;
        readonly Transform flashT;
        readonly double flashTimeMax, flashSecond, flashIntensity, flashDistance, flashForward, popTimeMax, popIntensity, popDistance, surfaceOffset;
        readonly int flashFramesMax;
        double flashTime, popTime;
        int flashFrames;
        bool flashOn;
        Color flashColor, popColor;
        Vector3 popAt;

        // The guide (port-only).
        readonly Light guide;
        bool guideOn;
        double guideClock;

        // NIGHT13 U3-26.4: the final state applied once a frame (PerfSwitches lights; off: every Set as before).
        readonly bool once;
        readonly double[] target; // each slot's intensity this frame
        bool flashTouched; // the flash light was set this frame; flashTarget is its last value
        double flashTarget;

        public LightPools(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            var parent = new GameObject("Light pool (fx/lightPool.js)").transform;
            slots = new Slot[cfg.Int("lightPool.max")];
            for (int i = 0; i < slots.Length; i++)
            {
                Light l = MakeLight(parent, $"Pool light {i}");
                slots[i] = new Slot { light = l, t = l.transform };
            }
            enabled = slots.Length;
            once = PerfSwitches.On(PerfFix.Lights);
            target = new double[slots.Length];
            steadyNear = cfg.Num("lightPool.steadyNear");
            steadyFar = cfg.Num("lightPool.steadyFar");
            machineIntensity = (float)cfg.Num("lightPool.machine.intensity");
            machineDistance = (float)cfg.Num("lightPool.machine.distance");
            machineHeight = (float)cfg.Num("lightPool.machine.height");
            blastColor = Look.Hex((uint)cfg.Num("lightPool.blast.color"));
            blastIntensity = cfg.Num("lightPool.blast.intensity");
            blastDistance = cfg.Num("lightPool.blast.distance");
            blastLife = cfg.Num("lightPool.blast.life");
            enemyColor = Look.C(cfg, "colors.enemy");
            usable = Look.C(cfg, "colors.usable");
            muzzleColor = Look.C(cfg, "colors.muzzleFlash");
            relayColor = Look.Hex((uint)cfg.Num("protocol.color"));
            relayLit = Look.Hex((uint)cfg.Num("protocol.litColor"));

            // The steady sources' cabinets (perks.js machines: the ported perks' spots; THE FORGE's).
            var list = new List<Machine>();
            foreach (PerkSpot s in game.map.perkSpots)
            {
                if (!Perks.Ported.Contains(s.perk)) continue; // PerkMachines builds only these
                list.Add(new Machine { pos = s.pos, core = s.core, color = Look.C(cfg, $"perks.{s.perk}.color") });
            }
            if (game.map.forgeSpot.HasValue && game.forge != null && game.forge.p.enabled)
                list.Add(new Machine { pos = game.map.forgeSpot.Value.pos, core = game.map.forgeSpot.Value.core, color = Look.C(cfg, "forge.color") });
            machines = list.ToArray();
            steady = new Source[machines.Length + game.map.relaySpots.Count + 4]; // + the box, the radio, the power switch, the secret's room

            flashTimeMax = cfg.Num("fx.flashTime");
            flashFramesMax = cfg.Int("fx.flashFrames");
            flashSecond = cfg.Num("fx.flashSecondFrame");
            flashIntensity = cfg.Num("fx.flashLightIntensity");
            flashDistance = cfg.Num("fx.flashLightDistance");
            flashForward = cfg.Num("fx.flashForward");
            popTimeMax = cfg.Num("fx.popTime");
            popIntensity = cfg.Num("fx.popIntensity");
            popDistance = cfg.Num("fx.popDistance");
            surfaceOffset = cfg.Num("fx.surfaceOffset");
            flash = MakeLight(parent, "Flash light (combat/effects.js)");
            flashT = flash.transform;
            guide = MakeLight(parent, "Guide light (first run)");
            guide.color = usable;
            guide.range = machineDistance;

            EventBus events = game.events;
            events.On("fire", o => OnFire((string)o));
            events.On<KillEvent>("kill", e => { popKill71 = true; Pop(WebSpace.ToUnity(e.position), enemyColor); }); // main.js: every kill pops (NIGHT71 L4: a kill's pop, for wash71's clock)
            events.On<BlastEvent>("rocketBlast", OnBlast);
            events.On<BlastEvent>("grenadeBlast", OnBlast); // none in the port yet (the web's grenade)
            if (UnityTuning.ImpactPops)
            {
                double shotgunRange = cfg.Num("weapons.shotgun.range"); // polish #44: a THUNDERCLAP that hit nothing ends here
                events.On<ShotEvent>("shotResult", e =>
                {
                    if (e.surface == "world" && !FxGates.ShotgunMissedAll(e, shotgunRange)) // polish #44: no light pop in mid-air
                        PopImpact(WebSpace.ToUnity(e.end + e.normal * surfaceOffset), Family(e.weapon)); // NIGHT54 light54: the impact glow (off: Pop as before)
                });
            }
            Register54(); // NIGHT54 light54 (LightPools.Light54.cs): the transient lights the landmark ignores
        }

        static Light MakeLight(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.intensity = 0;
            l.enabled = false;
            return l;
        }

        /// <summary>main.js familyHex: the gun's family colour (weapons.families), else colors.muzzleFlash (cached per gun).</summary>
        Color Family(string weapon)
        {
            return WeaponLook.FamilyColor(game.cfg, weapon, muzzleColor); // NIGHT13 U3-26.5: the one family colour lookup (cached per gun)
        }

        void OnFire(string weapon)
        {
            flashColor = Family(weapon);
            flashTime = flashTimeMax;
            flashFrames = flashFramesMax; // NIGHT5 F4 (styled): at most this many render frames
            Light52Fire(); // NIGHT52 U guns52 (LightPools.Guns52.cs): the shot's real time (and the reel's lifetime probe)
        }

        void OnBlast(BlastEvent e)
        {
            popKill71 = false; // NIGHT71 L4: a blast's pop keeps the world clock (wash71 is the kill's)
            Vector3 at = WebSpace.ToUnity(e.position);
            Flash(at, blastColor, blastIntensity, blastDistance, blastLife);
            Pop(at, blastColor); // main.js: effects.pop(e.position, 0xffa040) on rocketBlast
        }

        /// <summary>effects.js pop: a brief light at a kill (or a blast); it borrows the flash light while no flash needs it.</summary>
        public void Pop(Vector3 at, Color color)
        {
            if (Pop54(at, color)) return; // NIGHT54 light54: the pop's own light (never the muzzle flash's)
            popAt = at;
            popColor = color;
            popTime = popTimeMax;
        }

        /// <summary>lightPool.js pick: the free flash slot, else the one closest to done (never past the quality cap).</summary>
        int Pick()
        {
            int best = -1;
            double bestLeft = double.PositiveInfinity;
            for (int i = 0; i < enabled; i++)
            {
                Slot s = slots[i];
                if (s.steady || s.age >= s.life) return i;
                double left = s.peak * (1 - s.age / s.life);
                if (left < bestLeft)
                {
                    best = i;
                    bestLeft = left;
                }
            }
            return best;
        }

        /// <summary>lightPool.js flash: a short light at `at` (Unity space), colour, peak intensity, reach (m), life (s).</summary>
        public void Flash(Vector3 at, Color color, double intensity, double distance, double life)
        {
            int i = Pick();
            if (i < 0) return;
            Slot s = slots[i];
            s.t.position = at;
            s.light.color = color;
            s.light.range = (float)distance;
            s.peak = intensity;
            s.life = life;
            s.age = 0;
            s.steady = false;
        }

        /// <summary>lightPool.js setEnabled: how many slots the quality preset allows (the rest stay dark).</summary>
        public void SetEnabled(int n)
        {
            enabled = System.Math.Max(0, System.Math.Min(slots.Length, n));
            for (int i = enabled; i < slots.Length; i++) Set(slots[i].light, 0);
        }

        /// <summary>The first run's guide: one cyan pool pulsing at a Unity-space point (its feet; the light hangs
        /// lightPool.machine.height over it), or none (null).</summary>
        public void Guide(Vector3? at)
        {
            guideOn = at.HasValue;
            if (guideOn) guide.transform.position = at.Value + Vector3.up * machineHeight;
            else Set(guide, 0);
        }

        /// <summary>A new run (effects.js clear): no flash, no pop, no flash left in the pool.</summary>
        public void NewRun()
        {
            flashTime = 0;
            flashFrames = 0;
            popTime = 0;
            flashOn = false;
            Set(flash, 0);
            Clear54(); // NIGHT54 light54: the pop light dark
            foreach (Slot s in slots) s.age = double.PositiveInfinity;
        }

        static void Set(Light l, double intensity)
        {
            float v = (float)intensity * UnityTuning.LightScale;
            bool on = v > 0;
            if (l.enabled != on) l.enabled = on;
            if (on) l.intensity = v;
        }

        /// <summary>Frame: the pool (world dt), the flash light (world dt) and the guide's pulse (frame dt).</summary>
        public void Render(double worldDt, double frameDt, Camera cam)
        {
            if (cam == null) return;
            Transform ct = cam.transform;
            Vector3 camPos = ct.position;
            UpdatePool(worldDt, camPos);
            UpdateFlash(worldDt, ct);
            Step54(worldDt, frameDt); // NIGHT54 light54 (LightPools.Light54.cs): the pops' own light (nothing with the row off); NIGHT71 L4: frameDt for wash71's kill pop
            if (guideOn)
            {
                guideClock += frameDt;
                double wave = 0.5 + 0.5 * System.Math.Sin(guideClock * UnityTuning.GuidePulseRate * 2 * System.Math.PI);
                Set(guide, machineIntensity * (UnityTuning.GuidePulseMin + (1 - UnityTuning.GuidePulseMin) * wave));
            }
        }

        void UpdatePool(double dt, Vector3 cam)
        {
            int free = 0;
            for (int i = 0; i < enabled; i++)
            {
                Slot s = slots[i];
                if (!s.steady && s.age < s.life)
                {
                    s.age += dt;
                    double t = System.Math.Min(1, s.age / s.life);
                    Put(i, s.peak * (1 - t) * (1 - t));
                }
                else
                {
                    s.steady = false;
                    Put(i, 0);
                    free++;
                }
            }
            if (free > 0) TakeSteady(cam);
            if (once) for (int i = 0; i < enabled; i++) Set(slots[i].light, target[i]); // each slot once, in its final state
        }

        /// <summary>The nearest steady sources within their fade range take the free slots.</summary>
        void TakeSteady(Vector3 cam)
        {
            GatherSteady(cam);
            if (steadyCount == 0) return;
            int k = 0;
            for (int i = 0; i < enabled && k < steadyCount; i++)
            {
                Slot s = slots[i];
                if (s.age < s.life && !s.steady) continue;
                Source src = steady[k++];
                double d = (src.at - cam).magnitude;
                double fade = 1 - System.Math.Min(1, System.Math.Max(0, (d - steadyNear) / (steadyFar - steadyNear)));
                if (fade <= 0) break;
                s.steady = true;
                s.t.position = src.at;
                s.light.color = src.color;
                s.light.range = machineDistance;
                Put(i, machineIntensity * fade);
            }
        }

        /// <summary>Slot i's intensity this frame: kept for the once-a-frame apply, or set now (the switch off: as before).</summary>
        void Put(int i, double intensity)
        {
            if (once) target[i] = intensity;
            else Set(slots[i].light, intensity);
        }

        /// <summary>The flash light's intensity: its last value this frame applied at the end of UpdateFlash, or set now.</summary>
        void PutFlash(double intensity)
        {
            if (!once)
            {
                Set(flash, intensity);
                return;
            }
            flashTouched = true;
            flashTarget = intensity;
        }

        /// <summary>The steady sources where they stand now, sorted by xz distance to the camera (a stable insertion sort).</summary>
        void GatherSteady(Vector3 cam)
        {
            steadyCount = 0;
            for (int i = 0; i < machines.Length; i++)
            {
                Machine m = machines[i];
                double y = m.pos.y;
                if (m.core && game.core != null)
                {
                    double? top = game.core.TopAt(m.pos.x, m.pos.z); // MEND (and THE FORGE) ride their pillar
                    if (top.HasValue) y = top.Value;
                }
                Add(new Vec3(m.pos.x, y, m.pos.z), m.color, cam);
            }
            if (game.box != null) Add(game.box.Current.pos, usable, cam);
            Protocol protocol = game.protocol;
            if (protocol != null)
            {
                for (int i = 0; i < protocol.relays.Count; i++)
                {
                    Protocol.Relay r = protocol.relays[i];
                    if (!r.secret) Add(r.spot, r.lit ? relayLit : relayColor, cam);
                }
            }
            if (game.exfil != null && game.exfil.enabled) Add(game.exfil.radio.pos, usable, cam);
            if (game.power != null && game.power.enabled && !game.power.On) Add(game.power.spot.pos, usable, cam);
            for (int i = 1; i < steadyCount; i++)
            {
                Source x = steady[i];
                int j = i - 1;
                while (j >= 0 && steady[j].key > x.key)
                {
                    steady[j + 1] = steady[j];
                    j--;
                }
                steady[j + 1] = x;
            }
        }

        void Add(Vec3 feet, Color color, Vector3 cam)
        {
            if (steadyCount >= steady.Length) return;
            Vector3 at = WebSpace.ToUnity(feet);
            at.y += machineHeight;
            float dx = at.x - cam.x, dz = at.z - cam.z;
            steady[steadyCount++] = new Source { at = at, color = color, key = dx * dx + dz * dz };
        }

        /// <summary>effects.js update's light: the muzzle flash for its frames, then a pop while no flash needs the light.</summary>
        void UpdateFlash(double dt, Transform cam)
        {
            flashTouched = false;
            bool rt52 = Light52On; // NIGHT52 U guns52 (LightPools.Guns52.cs): the light by real time since the shot (as star51), not by render frames
            double k52 = 0;
            if (rt52 ? Light52Lit(out k52) : flashTime > 0 && flashFrames > 0)
            {
                double k = rt52 ? k52 : flashFrames == flashFramesMax ? 1 : flashSecond;
                if (!rt52 && --flashFrames == 0) flashTime = dt;
                // effects.js :245-246 / :254: at the muzzle (viewmodel.muzzleWorld) + the camera's forward x fx.flashForward
                // (polish #59); no rig in hand: the camera's forward at FxMuzzleDepth, as before
                Viewmodel vm = game.viewmodel;
                flashT.position = vm != null && vm.MuzzleWorld(out Vector3 muzzle)
                    ? muzzle + cam.forward * (float)flashForward
                    : cam.position + cam.forward * (float)(UnityTuning.FxMuzzleDepth + flashForward);
                flash.color = flashColor;
                flash.range = (float)flashDistance;
                PutFlash(flashIntensity * k);
                flashOn = true;
                Light52Frames++;
                if (!rt52) flashTime = System.Math.Max(0, flashTime - dt);
            }
            else if (flashOn)
            {
                flashOn = false;
                PutFlash(0);
                Light52Ended(); // NIGHT52 U guns52: the reel's lifetime (either mode)
                FlashEnded54(); // NIGHT54 light54: a short tail instead of the cut (nothing with the row off)
            }
            else
            {
                double tl = Tail54(dt); // NIGHT54 light54: the muzzle tail (negative: none, always with the row off)
                if (tl >= 0) PutFlash(tl);
            }
            // A pop borrows the light while no muzzle flash needs it.
            if (popTime > 0 && flashTime <= 0)
            {
                FlashPops54++; // the a154 reel: a pop took the flash light (0 with light54 on: the pops have their own)
                double k = popTime / popTimeMax;
                flashT.position = popAt;
                flash.color = popColor;
                flash.range = (float)popDistance;
                PutFlash(popIntensity * k * k);
                popTime = System.Math.Max(0, popTime - dt);
                if (popTime == 0) PutFlash(0);
            }
            if (flashTouched) Set(flash, flashTarget); // once, in its final state (the last value set this frame)
        }
    }
}
