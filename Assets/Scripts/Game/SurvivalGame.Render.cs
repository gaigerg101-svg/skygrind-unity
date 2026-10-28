// NIGHT13 U3-26.5: SurvivalGame split by concern (audit-perf section 5 move 2): the render half of the frame (the guide light, the render markers; RenderFrame).
// The same partial type, the statements unchanged and in the same order (SurvivalGame.cs has the rest).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>The render half of the frame (main.js frame after the steps): the camera, the views, the HUD, the FX, at alpha.</summary>
        void RenderFrame(double frameDt, double alpha, bool simulating)
        {
            // NIGHT14 U3-27.9: WHAT GOT YOU draws its own frame while it plays (the UI still renders over it).
            bool recapDrawn = false;
            RecapRender(frameDt, alpha, ref recapDrawn);
            if (recapDrawn)
            {
                using (rUi.Auto()) ui?.Render(frameDt);
                return;
            }
            Vec3 drawn = previous + (current - previous) * alpha;
            // NIGHT11 U3-24.2: one profiler marker per render system too (the Development profile found 3.2 KB a frame
            // allocated in Update outside the sim's markers).
            using (rFeel.Auto())
            {
                trauma.Step(frameDt, traumaP.decay);
                double calm = cfg.movement.settings.reducedMotion ? 0 : 1;
                double shakeScale = Math.Max(0, Math.Min(cfg.movement.feel.effectsIntensityMax, cfg.movement.settings.effectsIntensity)) * calm;
                // NIGHT54 M move54 (SurvivalGame.Move54.cs): the camera's height after a grounded step, and the last step's own look turn at alpha (`drawn`, input.yaw / pitch when off)
                feel.Apply(cam, Move54Drawn(drawn, frameDt, simulating), frameDt,
                    Move54ViewYaw(alpha, simulating) + viewmodel.PunchYawRad, Move54ViewPitch(alpha, simulating) + viewmodel.PunchPitchRad, trauma.Sample(traumaP, shakeScale));
                glacierView?.BendCamera(cam, sim); // NIGHT27 B: the Glacier's course bends in the view (camera onto the bend, yaw + the course heading); no-op unless the chase runs
                if (flow == Flow.Title) titleView?.Apply(cam, frameDt); // NIGHT12: the slow camera path over the map at dusk
                bool kioskCamera = false;
                CameraKiosk(frameDt, ref kioskCamera); // NIGHT14 U3-27.7: the attract's flythrough
            }
            using (rViewmodel.Auto()) cam.fieldOfView = viewmodel.Render(frameDt, cam.fieldOfView);
            ImpactRender(frameDt); // NIGHT16 U3-29.5: what a freeze shows (the FOV punch-in, the flash; SurvivalGame.Impact.cs, builder I)
            double wAlpha = sim.BendWorldAlpha(alpha), wMul = sim.Bend34WorldAnimMul; // NIGHT34 B (split bend): the world's interpolation and pace (alpha and 1 outside a split bend)
            using (rWorld.Auto())
            {
                coreMesh.Render(wAlpha);
                zonesView.Render();
                rankWorld?.Render(frameDt); // NIGHT17 U3-30.10: the world answers the rank
                foreach (PadMarker r in padRings) r.gameObject.SetActive(zones.padEnabled[r.index]);
                if (flow == Flow.Playing) zones.Track(current);
            }
            using (rEnemies.Auto())
            {
                huskView.Render(wAlpha, simulating && hitstopLeft <= 0 ? frameDt * WorldScale * wMul : 0);
                swarmerView.Render(wAlpha, simulating && hitstopLeft <= 0 ? frameDt * WorldScale * wMul : 0);
                spitterView.Render(wAlpha, simulating && hitstopLeft <= 0 ? frameDt * WorldScale * wMul : 0);
                N19EnemyViewsRender(wAlpha, simulating && hitstopLeft <= 0 ? frameDt * WorldScale * wMul : 0); // NIGHT19 (SurvivalGame.N19.cs)
                N20EnemyViewsRender(wAlpha, simulating && hitstopLeft <= 0 ? frameDt * WorldScale * wMul : 0); // NIGHT20 (SurvivalGame.N20.cs)
            }
            using (rProjectiles.Auto())
            {
                projectileViews.Render(wAlpha, hitstopLeft > 0 ? 0 : frameDt * WorldScale * wMul);
                N19GunFxRender(wAlpha, hitstopLeft > 0 ? 0 : frameDt * WorldScale * wMul); // NIGHT19 (SurvivalGame.N19.cs)
                N20EffectsRender(wAlpha, hitstopLeft > 0 ? 0 : frameDt * WorldScale * wMul); // NIGHT20 (SurvivalGame.N20.cs)
            }
            using (rProps.Auto())
            {
                telegraphView.Render(frameDt);
                wallBuys.Render(frameDt);
                boxView?.Render(hitstopLeft > 0 ? 0 : frameDt * WorldScale);
                perkMachines.Render(frameDt);
                powerUpView.Render(simulating && hitstopLeft <= 0 ? frameDt * WorldScale : 0);
                // NIGHT11: the run shape's views (main.js render: protocol, objectives and traps update on worldDt)
                double worldDt = hitstopLeft > 0 ? 0 : frameDt * WorldScale; // NIGHT16: x the slow-mo
                ringView.Render(worldDt);
                relayViews.Render(worldDt);
                trapViews.Render(worldDt);
                powerView.Render(worldDt);
                radioView.Render(worldDt);
                siteView.Render(worldDt);
                beaconView?.Render(worldDt); // NIGHT20 A3 (builder P2)
                spawnTell33View?.Render(worldDt); // NIGHT33 E
                stillArmor33View?.Render(worldDt); // NIGHT33 E
                arclineView?.Render(worldDt, alpha); // NIGHT21 U3-35.3 (builder M)
                N22RenderViews(worldDt * wMul, wAlpha); // NIGHT22 (the shared commit)
                N26RenderViews(worldDt * wMul, wAlpha); // NIGHT26 (the shared commit)
                N25RenderViews(worldDt * wMul, wAlpha); // NIGHT25 (the shared commit)
                N24RenderViews(worldDt * wMul, wAlpha); // NIGHT24 (the shared commit)
                RenderGhost(frameDt, alpha); // NIGHT14 U3-27.11: the ghost runner (SurvivalGame.Route.cs)
            }
            TipForCandidate(); // NIGHT12: tips.js's buy-prompt moments
            XrayHint();
            using (rUi.Auto())
            {
                ui?.Render(frameDt);
                ui?.RenderDamageNumbers(cam, WorldDt, flow != Flow.Title); // damageNumbers.js (settings.damageNumbers, off by default)
            }
            using (rSigns.Auto()) signs?.Render(cam);
            using (rFx.Auto())
            {
                particles.Render(hitstopLeft > 0 ? 0 : frameDt * WorldScale);
                killConfirm.Render(frameDt); // killConfirm.update(feelDt): the ring blooms through the kill's hitstop
                impactFx.Render(hitstopLeft > 0 ? 0 : frameDt * WorldScale);
                weaponFx.Render(frameDt, flow != Flow.Title && flow != Flow.Results);
                movementFx.Render(); // reads FrameDt / WorldDt
            }
            lightAndLens?.Render(FrameDt, WorldDt); // its own marker: SKYGRIND.R.Light
            UpdateGuide();
            using (rOther.Auto()) OnRender?.Invoke(frameDt, alpha);
            FeelProbe.EndFrame(this); // NIGHT18: the reel's telemetry (a no-op outside a reel)
        }
        string guideKind;
        Vec3 guideAt;
        bool guideOn;

        /// <summary>NIGHT16 (the shared commit): builder I's render hook (SurvivalGame.Impact.cs), after the camera and the viewmodel.</summary>
        partial void ImpactRender(double frameDt);

        /// <summary>
        /// NIGHT12 (design.onboarding): the first-run lesson's guide target (Ui.GuideTarget) as a light pool over the next
        /// thing to learn: a wall gun (the ARMORY gate while it is shut), the nearest open perk machine, the power switch, the
        /// next unlit relay, the nearest gate for sale. Recomputed when the target changes (a relay or gate when it is used).
        /// </summary>
        void UpdateGuide()
        {
            string kind = flow == Flow.Playing ? ui?.GuideTarget : null;
            bool on = false;
            Vec3 at = default;
            if (kind != null)
            {
                Vec3 me = mover.position;
                switch (kind)
                {
                    case "wallBuy":
                        if (zones.IsOpen("armory") && map.armoryWallZs.Count > 0)
                        {
                            at = new Vec3(map.armoryWallX + map.armoryWallFacing * wallBuyOut, map.armoryWallY, map.armoryWallZs[0]);
                            on = true;
                        }
                        else
                        {
                            foreach (BuySpot gate in zones.spots) // a loop, not Find: no closure per frame
                                if (gate.zoneId == "armory" && zones.SpotBuyable(gate)) { at = gate.at; on = true; break; }
                        }
                        break;
                    case "perk":
                    {
                        double best = double.PositiveInfinity;
                        foreach (PerkSpot ps in map.perkSpots)
                        {
                            int zi = ZoneMath.ZoneIndexAt(map.zoneParts, ps.pos.x, ps.pos.y + 1, ps.pos.z);
                            if (zi >= 0 && zones.LockedTag(zi)) continue;
                            double d = Vec3.DistanceTo(ps.pos, me);
                            if (d < best) { best = d; at = ps.pos; on = true; }
                        }
                        break;
                    }
                    case "power":
                        if (power.enabled && !power.On) { at = power.spot.pos; on = true; }
                        else goto case "relay";
                        break;
                    case "relay":
                        foreach (Protocol.Relay r in protocol.relays)
                            if (!r.lit && !r.secret) { at = r.spot; on = true; break; }
                        break;
                    case "gate":
                    {
                        double best = double.PositiveInfinity;
                        foreach (BuySpot s in zones.spots)
                        {
                            if (!zones.SpotBuyable(s)) continue;
                            double d = Vec3.DistanceTo(s.at, me);
                            if (d < best) { best = d; at = s.at; on = true; }
                        }
                        break;
                    }
                }
            }
            if (kind == guideKind && on == guideOn && (!on || (at.x == guideAt.x && at.y == guideAt.y && at.z == guideAt.z))) return;
            guideKind = kind;
            guideOn = on;
            guideAt = at;
            lightAndLens?.Guide(on ? WebSpace.ToUnity(at) : (Vector3?)null);
        }

        static readonly ProfilerMarker rFeel = new ProfilerMarker("SKYGRIND.R.Feel"), rViewmodel = new ProfilerMarker("SKYGRIND.R.Viewmodel"),
            rWorld = new ProfilerMarker("SKYGRIND.R.World"), rEnemies = new ProfilerMarker("SKYGRIND.R.Enemies"), rProjectiles = new ProfilerMarker("SKYGRIND.R.Projectiles"),
            rProps = new ProfilerMarker("SKYGRIND.R.Props"), rUi = new ProfilerMarker("SKYGRIND.R.Ui"), rSigns = new ProfilerMarker("SKYGRIND.R.Signs"),
            rFx = new ProfilerMarker("SKYGRIND.R.Fx"), rOther = new ProfilerMarker("SKYGRIND.R.Other"), rInput = new ProfilerMarker("SKYGRIND.R.Input");
    }
}
