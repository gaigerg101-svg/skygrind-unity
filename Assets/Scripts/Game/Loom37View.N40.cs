// NIGHT40 builder B4 (NIGHT40.md B4 THE LOOM; boss_rules 1 "a red-orange glow on the attacking part"; "a pose per state"): loom40's look on
// Loom37View (view only; the rules are the sim's). Shown only while loom40 is on this run (sim.n40.loom40); off: Loom37View draws as NIGHT39.
//  - THE TELL GLOW: through each wind-up a ball on the part that attacks swells with the wind-up's progress: the LASH on the body's front
//    toward you (red-orange), the SPOOL on the spinneret (YELLOW: parryable), the WEB on the body's top (red-orange). Bright on the strike.
//  - THE POSES (an offset and a tilt on the body; legs and knees stay the sim's): WIND-UP rears up and back from you, STRIKE lunges at you,
//    RECOVER sags (the punish beat), STAGGER (a tear just now) shudders, the CYAN WINDOW rolls open (the spinneret shown), REPOSITION leans
//    into its glide.
//  - THE CUES: a latched knee lights the thread anchors far enough to tear it from (cyan rings: "pull away to THESE"); o2's practice knee is a
//    cyan ball (white while hooked, gone while torn).
// Numbers: UnityTuning.Loom40.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Loom37View
    {
        Transform tellT, practiceT;
        Renderer tellR, practiceR;
        readonly System.Collections.Generic.List<Transform> pullT = new System.Collections.Generic.List<Transform>();
        readonly System.Collections.Generic.List<Renderer> pullR = new System.Collections.Generic.List<Renderer>();
        Transform practiceRoot;
        int lastTears;
        float staggerLeft;

        /// <summary>For the reels / frames (loom40): the pose drawn now, the tell glow shown, pull-away anchors lit, the practice knee drawn.</summary>
        public string Pose40 = "";
        public bool TellGlow40, PracticeShown40;
        public int PullLit40;

        void Init40()
        {
            tellR = Glow(root, "loom40 tell glow", ballMesh, out tellT);
            tellT.gameObject.SetActive(false);
            practiceRoot = new GameObject("NIGHT40 practice knee").transform;
            practiceR = Glow(practiceRoot, "loom40 practice knee", ballMesh, out practiceT);
            practiceRoot.gameObject.SetActive(false);
        }

        /// <summary>Every frame, fight or not: o2's practice knee.</summary>
        void Render40Always(SurvivalSim sim)
        {
            bool show = sim != null && sim.n40.loom40 && sim.loom40PracticeNode >= 0 && !sim.loom40PracticeTorn;
            PracticeShown40 = show;
            if (practiceRoot.gameObject.activeSelf != show) practiceRoot.gameObject.SetActive(show);
            if (!show) return;
            practiceT.position = U(sim.loom40PracticeAt);
            float pulse = 1f + UnityTuning.Loom40PracticePulse * Mathf.Sin(t * UnityTuning.Loom38KneeBreathHz * 2f * Mathf.PI);
            practiceT.localScale = Vector3.one * (UnityTuning.Loom40PracticeR * 2f * pulse);
            Tint(practiceR, sim.loom40PracticeLatched ? Look.Hex(UnityTuning.Loom38ReadyHex, UnityTuning.Loom38ReadyGain) : Look.Hex(UnityTuning.Loom37KneeLiveHex, UnityTuning.Loom37KneeLiveGain));
        }

        /// <summary>The fight's loom40 look (after the body's place is set): the pose, the tell glow, the pull-away anchors.</summary>
        void Render40(SurvivalSim sim, Loom37 L, Vector3 c, float dt)
        {
            bool on = sim.n40.loom40;
            if (!on)
            {
                if (tellT.gameObject.activeSelf) tellT.gameObject.SetActive(false);
                Pool(pullT, pullR, 0, "loom40 pull-away", ballMesh);
                Pose40 = "";
                TellGlow40 = false;
                PullLit40 = 0;
                return;
            }
            Vector3 me = U(sim.mover.position) + Vector3.up * 1.2f;
            Vector3 toMe = me - c;
            toMe.y = 0;
            Vector3 fwd = toMe.sqrMagnitude > 1e-4f ? toMe.normalized : Vector3.forward;
            Boss37Brain b = L.brain;
            float prog = (float)b.Progress;
            if (L.tears != lastTears) { lastTears = L.tears; staggerLeft = UnityTuning.Loom40StaggerS; }
            staggerLeft = Mathf.Max(0f, staggerLeft - dt);
            // the pose
            Vector3 off = Vector3.zero;
            float pitch = 0f, roll = 0f;
            string pose = "idle";
            if (staggerLeft > 0f)
            {
                pose = "stagger";
                float k = staggerLeft / UnityTuning.Loom40StaggerS;
                off = new Vector3(Mathf.Sin(t * 47f), Mathf.Sin(t * 31f), Mathf.Sin(t * 39f)) * (UnityTuning.Loom40ShudderM * k);
            }
            else if (b.state == Boss37State.Window) { pose = "window"; roll = UnityTuning.Loom40WindowRollDeg; off = Vector3.down * UnityTuning.Loom40SagM; }
            else if (b.state == Boss37State.Tell) { pose = "wind-up"; off = (-fwd * UnityTuning.Loom40RearM + Vector3.up * UnityTuning.Loom40RearUpM) * prog; pitch = -UnityTuning.Loom40RearDeg * prog; }
            else if (b.state == Boss37State.Active) { pose = "strike"; off = fwd * UnityTuning.Loom40LungeM; pitch = UnityTuning.Loom40LungeDeg; }
            else if (b.state == Boss37State.Recover) { pose = "recover"; off = Vector3.down * UnityTuning.Loom40SagM; pitch = UnityTuning.Loom40LungeDeg * 0.5f; }
            else if (L.GlideSpeed > 1)
            {
                pose = "reposition";
                Vector3 g = U(L.stations[System.Math.Max(0, System.Math.Min(2, b.leg - 1))]) - c;
                g.y = 0;
                if (g.sqrMagnitude > 1e-4f) fwd = g.normalized;
                pitch = UnityTuning.Loom40GlideLeanDeg * Mathf.Clamp01((float)(L.GlideSpeed / L.Glide40Max));
            }
            Pose40 = pose;
            bodyT.position = c + off;
            bodyT.rotation = Quaternion.AngleAxis(pitch, Vector3.Cross(Vector3.up, fwd)) * Quaternion.AngleAxis(roll, Vector3.forward); // a tilt toward / away from you and a roll; no yaw (the legs are the sim's, along world x)
            // the tell glow on the attacking part
            Boss37Attack a = b.Current;
            bool tell = a != null && (b.state == Boss37State.Tell || b.state == Boss37State.Active);
            TellGlow40 = tell;
            if (tellT.gameObject.activeSelf != tell) tellT.gameObject.SetActive(tell);
            if (tell)
            {
                bool strike = b.state == Boss37State.Active;
                Vector3 at = a.id == Loom37.Spool ? U(L.Spinneret) : a.id == Loom37.Web ? c + off + Vector3.up * (UnityTuning.Loom37BodyH * 0.6f) : c + off + fwd * (UnityTuning.Loom37BodyL * 0.55f);
                tellT.position = at;
                float s = UnityTuning.Loom40TellR * (0.35f + 0.65f * (strike ? 1f : prog));
                tellT.localScale = Vector3.one * (2f * s);
                uint hex = a.id == Loom37.Spool ? UnityTuning.Loom37SpoolHex : UnityTuning.Loom37DangerHex;
                Tint(tellR, Look.Hex(hex, strike ? UnityTuning.Loom37LiveGain : UnityTuning.Loom37TellGain * (0.5f + prog)));
            }
            // the cue: a latched knee lights every live thread anchor far enough to tear it from (the pull-away targets)
            int lit = 0, latched = -1;
            for (int i = 0; i < 6; i++) if (L.Latched40(i)) { latched = i; break; }
            if (latched >= 0)
                foreach (var an in sim.loom37Anchors)
                    if (!an.dark && Vec3.DistanceTo(an.at, L.knees[latched].at) >= L.p.tearDistM + 2) lit++;
            Pool(pullT, pullR, lit, "loom40 pull-away", ballMesh);
            PullLit40 = lit;
            if (lit == 0) return;
            int pi = 0;
            float beat = 0.6f + 0.4f * Mathf.Sin(t * UnityTuning.Loom38ReadyHz * 2f * Mathf.PI);
            foreach (var an in sim.loom37Anchors)
            {
                if (an.dark || Vec3.DistanceTo(an.at, L.knees[latched].at) < L.p.tearDistM + 2) continue;
                if (!pullT[pi].gameObject.activeSelf) pullT[pi].gameObject.SetActive(true);
                pullT[pi].position = U(an.at);
                pullT[pi].localScale = Vector3.one * (2f * UnityTuning.Loom40PullR);
                Tint(pullR[pi], Look.Hex(UnityTuning.Loom37KneeLiveHex, UnityTuning.Loom37KneeLiveGain * beat));
                pi++;
            }
        }
    }
}
