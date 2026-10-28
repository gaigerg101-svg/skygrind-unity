// NIGHT55 K (builder K): the loop's lines for kill55 (and, later, finish55 / speed55 / brief55). Three one-line hooks in SurvivalGame.Impact.cs:
//   ImpactBeginFrame  share = N55KFrame(frameDt, share)        (last: the ladder's hold, after every slow source)
//   ImpactStepEnded   return N55KStepEnded(<the rules' answer>) (a kill55 hold beginning stops the stepping, as a freeze does)
//   ImpactToTitle     N55KHalt()
// Everything else hangs on the game's own events (the bus, OnRender, OnNewRun), hooked once on the first frame. With the row off nothing is
// built and every hook returns its input: NIGHT54's game exactly (pacing and pictures only; no Core state: golden and fingerprints untouched).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        bool n55kHooked, n55kOn;
        Kill55Hold n55kHold;
        Kill55Debris n55kDebris;
        Kill55Damage n55kDamage;
        int n55kKillTag;
        Mesh n55kCube;

        /// <summary>The a355 reel's probes (null with the row off).</summary>
        public Kill55Hold N55KHolds => n55kHold;
        public Kill55Debris N55KDebris => n55kDebris;
        public Kill55Damage N55KDamage => n55kDamage;
        /// <summary>The last kill's debris tag (the reel counts its shards by it).</summary>
        public int N55KLastKillTag => n55kKillTag;

        void N55KHook()
        {
            n55kHooked = true;
            n55kOn = Kill55.Live(cfg);
            if (!n55kOn) return;
            GameConfig icfg = Move54.ImpactConfig(cfg); // the impact director's own view (move54's kill cap when that row is on), as ImpactBeginFrame built it
            double cap = sim != null && sim.n42.impact42 && icfg.Has("design.n42.impact42.killCapS") ? icfg.Num("design.n42.impact42.killCapS") : icfg.Num("design.impact.freeze.killCap");
            n55kHold = Kill55Hold.From(cfg, cap);
            var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            n55kCube = probe.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.Destroy(probe);
            n55kDebris = new Kill55Debris(this, n55kCube);
            n55kDamage = new Kill55Damage(this);
            events.On<KillEvent>("kill", N55KOnKill);
            events.On<HitEvent>("hit", e => n55kDamage.OnHit(e, n55kDebris));
            OnRender += N55KRender;
            OnNewRun += N55KNewRun;
        }

        void N55KOnKill(KillEvent e)
        {
            if (e == null || e.owner != "player") return;
            n55kHold.Kill(sim.SimTime);
            n55kDamage.OnKill(e.enemy);
            // the shards: from the kill point, away from the gun; more inside a chain
            Vector3 at = WebSpace.ToUnity(e.position), eye = WebSpace.ToUnity(Eye);
            float feetY = at.y - 1;
            Color col = Look.C(cfg, "colors.enemy");
            foreach (Enemy en in enemies.all)
                if (en.id == e.enemy)
                {
                    feetY = (float)en.position.y;
                    if (en.tank29 && cfg.Has("design.n29.tank.color")) col = Look.C(cfg, "design.n29.tank.color");
                    break;
                }
            int extra = Math.Min(UnityTuning.Kill55ShardsChainMax, UnityTuning.Kill55ShardsPerChain * Math.Max(0, n55kHold.Chain - 1));
            n55kKillTag++;
            n55kDebris.Burst(at, (at - eye).normalized, feetY, col, UnityTuning.Kill55ShardsPerKill + extra, 1, n55kKillTag);
        }

        void N55KNewRun()
        {
            n55kHold?.Reset();
            n55kDebris?.Clear();
            n55kDamage?.Clear();
        }

        void N55KRender(double frameDt, double alpha)
        {
            float wdt = (float)WorldDt;
            n55kDebris?.Render(wdt);
            n55kDamage?.Render(wdt);
        }

        /// <summary>ImpactBeginFrame's last line: the ladder's hold (the share unchanged with the row off).</summary>
        double N55KFrame(double frameDt, double share)
        {
            if (!n55kHooked) N55KHook();
            if (!n55kOn || impactFrames == null) return share;
            return n55kHold.Frame(frameDt, impactFrames.FreezeLeft, share);
        }

        /// <summary>ImpactStepEnded's answer through the ladder: true when the rules froze or a kill55 hold begins now.</summary>
        bool N55KStepEnded(bool froze)
        {
            if (!n55kOn || impactFrames == null || n55kHold == null) return froze;
            ImpactRules r = impactFrames.rules;
            double scale = r.hitFreeze == HitFreezeMode.Off ? 0 : r.hitFreeze == HitFreezeMode.Light ? cfg.Num("design.impact.settings.lightScale") : 1;
            string cause = froze ? ImpactRules.TierNames[(int)r.granted] : null;
            bool running = Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z) >= n55kHold.runSpeed; // NIGHT57 G (G3)
            bool held = n55kHold.StepEnded(r.Clock, sim.SimTime, froze, froze ? r.grantedSeconds : 0, cause, r.FrozenBetween(r.Clock - 1, r.Clock), scale, running);
            return froze || held;
        }

        /// <summary>The way to TITLE: no hold left over.</summary>
        void N55KHalt() => n55kHold?.Reset();
    }
}
