// NIGHT47 builder B, G35 (row wyrm47's sim side, on level 3 as n47-kiln only; Research/2026-10-18-wave47/B-cards.md section 4.3): THE KILN WYRM
// foreshadowed in three beats before its fight. GG 2026-10-18: "It also needs to appear earlier in the stage to help build anticipation and
// teach mechanics before the fight." Called from SurvivalSim.N47.Brawl.cs's step hook. The room ids are level 3's (the plan's), not numbers.
//   1 THE GLIMPSE (kb1 3.1 THE STEPS): the first time the player's feet are glimpseY m over the room's spawn (the top of the boost steps), the
//     worm's molten back breaks out of the far wall, arcs and goes back into the rock (glimpseS s; no collider, no damage). Event wyrm47Glimpse.
//   2 THE PASS (kb2 3.2 POUR HALL): passAfterS s after entering, the burrow's glowing trail runs across the hall under the player (west wall to
//     east, through the player's z, passS s), the floor shakes (the view's). No damage. Event wyrm47Pass.
//   3 THE CAMEO (kb3 3.3 CASTING FLOOR): cameoAfterS s after entering, cameoCount times cameoGapS apart: the breach's tell (a cameoRadius m
//     crack circle under the player that follows him for cameoTrack of cameoTellS, then locks), then the head erupts through it: cameoDamage
//     and a cameoKnockUp kick to a player whose feet are within the circle and under cameoMaxUp m over the room's floor (jump, boost or step
//     off it). Events wyrm47BreachTell {at, seconds}, wyrm47Cameo {at, hit}. The fight's burrow (the DIVE) is the same tell (G31 / G34).
// Numbers in design.n47.wyrm47.numbers. Off (the row off, or another level): nothing. No allocation per step. No UnityEngine.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The beats' event payload (one reused object): which beat, where, how long its tell, whether the cameo hit the player.</summary>
    public sealed class Wyrm47BeatEvent
    {
        public string beat;
        public Vec3 at, to;
        public double seconds;
        public bool hit;
    }

    public sealed partial class SurvivalSim
    {
        bool k47On, k47Glimpsed, k47Passed;
        int k47Cameos, k47CameoWait, k47TellLeft, k47TellTotal, k47LockAt;
        readonly Wyrm47BeatEvent k47Said = new Wyrm47BeatEvent();

        /// <summary>The view's read of the beats: steps since each began (-1: not running), where, and the cameo's tell.</summary>
        public int Glimpse47Steps = -1, Pass47Steps = -1, Cameo47BreachSteps = -1;
        public Vec3 Glimpse47At, Pass47From, Pass47To, Cameo47At;
        public bool Cameo47Telling => k47TellLeft > 0;
        public double Cameo47Progress => k47TellTotal > 0 ? 1.0 - (double)k47TellLeft / k47TellTotal : 0;
        public double Cameo47Radius => cfg.Num(Wyrm47Root + "cameoRadius");
        /// <summary>The harness's counts this sim's life: beats run, cameo hits on the player.</summary>
        public int beats47Glimpse, beats47Pass, beats47Cameo, beats47CameoHits;

        void Kiln47StartRun()
        {
            k47Glimpsed = k47Passed = false;
            k47Cameos = k47CameoWait = k47TellLeft = k47TellTotal = 0;
            Glimpse47Steps = Pass47Steps = Cameo47BreachSteps = -1;
        }

        void Kiln47Step()
        {
            k47On = w47On && campaignLevel != null && (campaignLevel.id == "n47-kiln" || N52LevelFrom(campaignLevel.id) == "n47-kiln" || N52LevelFrom(N57LevelFrom(campaignLevel.id)) == "n47-kiln") && campaign != null; // NIGHT52 (n52-shared.cjs): n52-kiln keeps the beats; NIGHT57 (n57-shared.cjs): n57-kiln too
            if (Glimpse47Steps >= 0) Glimpse47Steps++;
            if (Pass47Steps >= 0) Pass47Steps++;
            if (Cameo47BreachSteps >= 0) Cameo47BreachSteps++;
            if (!k47On) return;
            RoomDef r = campaign.Room;
            if (r == null) return;
            double dt = cfg.Dt;
            Vec3 me = mover.position;
            if (!k47Glimpsed && r.id == "kb1" && me.y - r.spawn.y >= cfg.Num(Wyrm47Root + "glimpseY"))
            {
                k47Glimpsed = true;
                beats47Glimpse++;
                Glimpse47Steps = 0;
                Glimpse47At = new Vec3((r.x0 + r.x1) * 0.5, me.y, System.Math.Min(r.z0, r.z1)); // the far wall (rooms run toward -z)
                Say47("glimpse", Glimpse47At, Glimpse47At, cfg.Num(Wyrm47Root + "glimpseS"), false);
            }
            if (!k47Passed && r.id == "kb2" && campaign.roomSteps >= MovementMath.StepsFor(cfg.Num(Wyrm47Root + "passAfterS"), dt))
            {
                k47Passed = true;
                beats47Pass++;
                Pass47Steps = 0;
                Pass47From = new Vec3(System.Math.Min(r.x0, r.x1), r.spawn.y, me.z);
                Pass47To = new Vec3(System.Math.Max(r.x0, r.x1), r.spawn.y, me.z);
                Say47("pass", Pass47From, Pass47To, cfg.Num(Wyrm47Root + "passS"), false);
            }
            if (r.id != "kb3") return;
            if (k47TellLeft > 0)
            {
                if (k47TellLeft > k47LockAt) Cameo47At = new Vec3(me.x, r.spawn.y, me.z); // it follows you, then locks
                if (--k47TellLeft == 0)
                {
                    double rad = cfg.Num(Wyrm47Root + "cameoRadius");
                    double dx = me.x - Cameo47At.x, dz = me.z - Cameo47At.z;
                    bool hit = dx * dx + dz * dz <= rad * rad && me.y - r.spawn.y <= cfg.Num(Wyrm47Root + "cameoMaxUp");
                    if (hit && health.Damage(cfg.Num(Wyrm47Root + "cameoDamage"), "wyrm47", Cameo47At))
                    {
                        mover.Impulse(0, cfg.Num(Wyrm47Root + "cameoKnockUp"), 0);
                        beats47CameoHits++;
                    }
                    Cameo47BreachSteps = 0;
                    beats47Cameo++;
                    k47Cameos++;
                    k47CameoWait = MovementMath.StepsFor(cfg.Num(Wyrm47Root + "cameoGapS"), dt);
                    Say47("cameo", Cameo47At, Cameo47At, 0, hit);
                }
                return;
            }
            if (k47Cameos >= (int)cfg.Num(Wyrm47Root + "cameoCount")) return;
            if (k47CameoWait > 0) { k47CameoWait--; return; }
            if (k47Cameos == 0 && campaign.roomSteps < MovementMath.StepsFor(cfg.Num(Wyrm47Root + "cameoAfterS"), dt)) return;
            double tell = cfg.Num(Wyrm47Root + "cameoTellS");
            k47TellTotal = k47TellLeft = MovementMath.StepsFor(tell, dt);
            k47LockAt = k47TellTotal - MovementMath.StepsFor(tell * cfg.Num(Wyrm47Root + "cameoTrack"), dt);
            Cameo47At = new Vec3(me.x, r.spawn.y, me.z);
            Say47("breachTell", Cameo47At, Cameo47At, tell, false);
        }

        void Say47(string beat, Vec3 at, Vec3 to, double seconds, bool hit)
        {
            k47Said.beat = beat;
            k47Said.at = at;
            k47Said.to = to;
            k47Said.seconds = seconds;
            k47Said.hit = hit;
            events.Emit(beat == "glimpse" ? "wyrm47Glimpse" : beat == "pass" ? "wyrm47Pass" : beat == "breachTell" ? "wyrm47BreachTell" : "wyrm47Cameo", k47Said);
        }
    }
}
