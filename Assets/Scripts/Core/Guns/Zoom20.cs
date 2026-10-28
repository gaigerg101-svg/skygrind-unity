// NIGHT20 U3-34.3 (builder G, design.zoom): zoom on most guns. GG 2026-10-06 evening: "Most weapons should, besides a few
// that have unique left clicks, zoom: simple zoom." The ZOOM switch MOST, the web's ads.js rule (player/ads.js: right mouse
// on a zoom weapon zooms while held; on every other weapon it is the special; the SPECIAL key is always the special) with
// more guns in the zoom list:
// - KESTREL 1.5 (ads.weapons.revolver; once design.shockwave turns the gust off it zooms again, its hip spread back to
//   design.zoom.kestrelHipSpreadDeg, the web's 0.8), STITCHER design.zoom.nailgunMag 1.25 (the web never zoomed the
//   nailgun; zoomed, its nails spread x nailgunSpreadMult; its detonate stays on SPECIAL), LONGWAVE the web's 3.5 scope
//   (walk x design.zoom.railWalkMult zoomed; holding fire past a full charge folds into the overcharge, POLISH V2), the
//   ZAPPER 1.3 when its port fires (zoomed, its chain reach x the web's ads.weapons.zapper.chainRange);
// - unique LTs: the THUNDERCLAP (the shockwave, Shockwave.cs), the COMET (steer), the BOUNCER (its airburst: the web never
//   zoomed it), the gauntlet (nothing). ZoomGun is 0 for them;
// - the web's suspend (ads.suspendStates: a dash or a swing suspends the zoom, it resumes while LT is held) and, port-only,
//   Halo's split: only a scope (ads.weapons.<gun>.scope, the LONGWAVE) descopes on a hit, until LT is pressed again.
// With design.zoom off the loadout keeps NIGHT19's rule (the KESTREL alone, and not with the gust: Combat.cs StepAds).
// The zoom's FOV and the look sensitivity stay the views' (Viewmodel: CombatMath.ZoomedFov(base, ZoomGun(held)) eased).
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.zoom, read once (null without its numbers).</summary>
    public sealed class ZoomParams
    {
        public double nailgunMag, railWalkMult, kestrelHipSpreadDeg, nailgunSpreadMult;

        public static ZoomParams From(GameConfig cfg)
        {
            if (!cfg.Has("design.zoom.nailgunMag")) return null;
            return new ZoomParams
            {
                nailgunMag = cfg.Num("design.zoom.nailgunMag"),
                railWalkMult = cfg.Num("design.zoom.railWalkMult"),
                kestrelHipSpreadDeg = cfg.Num("design.zoom.kestrelHipSpreadDeg"),
                nailgunSpreadMult = cfg.Num("design.zoom.nailgunSpreadMult"),
            };
        }
    }

    public sealed partial class Loadout
    {
        ZoomParams zoomP; // design.zoom (Zoom20Init)
        readonly Dictionary<string, double> adsZoom = new Dictionary<string, double>(); // ads.weapons.<id>.zoom (0: none), per id
        readonly Dictionary<string, bool> adsScope = new Dictionary<string, bool>(); // ads.weapons.<id>.scope, per id
        bool descoped; // a scope knocked out by a hit: LT must be let go and pressed again
        double zapChainZoom = 1; // ads.weapons.zapper.chainRange (the web's zoomed x1.15)

        /// <summary>The zoom is this run's (design.zoom on, its numbers there).</summary>
        public bool ZoomOn => n20.zoom && zoomP != null;

        void Zoom20Init()
        {
            zoomP = ZoomParams.From(cfg);
            zapChainZoom = cfg.Has("ads.weapons.zapper.chainRange") ? cfg.Num("ads.weapons.zapper.chainRange") : 1;
            // Only a scope descopes on a hit (the player's own hurt; read only with design.zoom on).
            events.On<HurtEvent>("hurt", h =>
            {
                if (h != null && h.owner == owner && ZoomOn && adsActive && AdsScope(Weapon)) descoped = true;
            });
        }

        double AdsZoom(string id)
        {
            if (!adsZoom.TryGetValue(id, out double z)) adsZoom[id] = z = cfg.Has($"ads.weapons.{id}.zoom") ? cfg.Num($"ads.weapons.{id}.zoom") : 0;
            return z;
        }

        bool AdsScope(string id)
        {
            if (!adsScope.TryGetValue(id, out bool s)) adsScope[id] = s = cfg.Has($"ads.weapons.{id}.scope") && cfg.Bool($"ads.weapons.{id}.scope");
            return s;
        }

        /// <summary>A gun's zoom magnification (0 when its LT is a unique alt or nothing): the HUD's "ZOOM x" and the views' FOV.</summary>
        public double ZoomGun(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            double z31 = ZoomGun31(id); // NIGHT31 ltAlways (Core/Guns/LtAlways31.cs): the KESTREL / THUNDERCLAP zoom; the NAILGUN zooms until a nail is stuck
            if (z31 >= 0) return z31;
            if (id26 != null && id26.zoom.TryGetValue(id, out double z26)) return z26; // NIGHT26 weaponIdentity: a gun whose LT is its own alt (design.n26.weapons.<w>.zoom 0)
            if (id == "revolver") return GustOn ? 0 : AdsZoom(id); // NIGHT19: the gust took the KESTREL's LT
            if (!ZoomOn) return 0; // NIGHT19: only the KESTREL zoomed
            if (shockwaveOff25 && id == "shotgun") return shotgunZoom25; // NIGHT25 shockwaveOff: the THUNDERCLAP's LT zooms (design.n25.shockwaveOff.zoom)
            if (id == "nailgun") return zoomP.nailgunMag;
            if (IsPort19(id) && !Arsenal19On) return 0;
            return AdsZoom(id); // rail 3.5, zapper 1.3 (and the web's rattler / prism when they're ported); the rest 0
        }

        /// <summary>The active zoom's magnification (1 when not zoomed).</summary>
        public double ZoomMag => adsActive ? JsMath.Max(1, ZoomGun(Weapon)) : 1;

        /// <summary>LT zooms this gun (ads.js routeSpecial: anywhere else LT is the special). Off: NIGHT19's routing.</summary>
        bool LtZooms(string w) => ZoomOn ? ZoomGun(w) > 1 : ZoomWeapon(w) && !AltIsSpecial(w);

        /// <summary>The zoom with design.zoom on (ads.js step): want = LT held on a zoom gun, not descoped; on = want, ready, not suspended.</summary>
        void StepAds20(CombatInput input)
        {
            string w = Weapon;
            if (descoped && (!input.altFire.held || input.altFire.pressed)) descoped = false; // let go (or pressed again): the scope may return
            bool want = ZoomGun(w) > 1 && input.altFire.held && !descoped;
            bool suspended = suspend.Contains(PortData.Name(mover.state));
            bool on = want && ready && !suspended;
            if (on == adsActive) return;
            adsActive = on;
            mover.walkMult = on ? (w == "rail" ? zoomP.railWalkMult : walkMult) : 1;
            events.Emit(on ? "adsStart" : "adsEnd", w);
        }

        /// <summary>The STITCHER's nail spread (deg) now: x nailgunSpreadMult zoomed with design.zoom.</summary>
        double NailSpreadNow => ZoomOn && adsActive ? nailSpread * zoomP.nailgunSpreadMult : nailSpread;

        /// <summary>The ZAPPER's chain reach multiplier now (the web's zoomed x1.15 with design.zoom).</summary>
        double ZapChainZoom => ZoomOn && adsActive ? zapChainZoom : 1;
    }
}
