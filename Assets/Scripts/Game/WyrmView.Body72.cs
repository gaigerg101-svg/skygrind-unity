// NIGHT72 builder B (slate B1, body72, a VIEW row: NightRows.ViewOn(cfg, "body72"); gg-proxy #4: "the bosses look like boxes", THE KILN WYRM "blends into the brown
// wall" and nothing shows it is charging; GG 2026-10-22 "boss animations are too rigid to start"; D-11: the pill language, no new creature shape). THE KILN WYRM, drawn
// inside the pill language and the hitbox guard. View only: WyrmView draws the sim's own oriented boxes (the hitboxes: "a shot finds what you see"); nothing here moves
// where they are, how big they are, the AI, a timing or a hit sphere. The silhouette at rest is the same segments.
// THE ROW ON (read EVERY frame, D1's contract: the springs below are stepped the same with the row off, so the next frame on is the frame it would have drawn):
//  - PRESENCE: the body pieces are drawn with SKYGRIND/Enemy, the shader every husk, swarmer and spitter wears (EnemyLook: the rim light that lifts a silhouette off its
//    wall, the form shade, the NIGHT51 ink outline, the NIGHT52 ramp), in a RED-WEIGHTED ash a step lighter than the kiln wall (UnityTuning.Body72Hex; t1 round 1's
//    neutral ash rendered yellow, hue 46-49 deg: the kiln's grade zeroes blue, so R = G reads as the parry tell's colour); the seams go through BossColours63.Hot (clip-safe) with the row
//    on; the head's punch window and the open head's weak role are NIGHT47 / NIGHT63's own.
//  - FOLLOW-THROUGH along its segments: each body piece's drawn centre trails its rigid centre by a damped spring (UnityTuning.Body72Lag*: the head end quick, the tail
//    slow), held to Body72LagGuardShare of the piece's own guard (BossMotion63's limitShare x its half width) and then, with the ripple, sag and breath already drawn,
//    to the whole guard by DrawMotion63's own clamp. It reads in the [boss63] rms lines (the parts' offsets from their rigid pose).
//  - THE TELL GLOW: through every wind-up but the roar (the aggro charge, the spit, the dive, the lunge) the seams and trim plates go to the danger red-orange at
//    UnityTuning.Body72TellGain x BossTell63.Level (0.45 to 1 over the wind-up), and the body pieces move toward it (the head most, the tail least); the head's punch window
//    keeps its yellow (the parry tell, DECIDE63 D-1: BossTell63's own column skips the aggro charge). Idle: no glow.
// OFF: NIGHT71's worm exactly (the neon opaque material put back the frame the row reads false).
// Proof (L-014 / L-115; Player.log):
//   [body72] wyrm row body72 on|off: material SKYGRIND/Enemy|neon, base 0x<hex>, lag <hz> Hz ... 
//   [body72] wyrm tell <attack>: level <l>, trim gain <g> | row body72 on        (once per danger wind-up; OFF prints no tell line)
//   [body72] wyrm lag: <n> pieces, largest <m> m of the guard <m> m (share <s>) | row body72 on|off     (every Body72SayEveryS s of a drawn fight)
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class WyrmView
    {
        public const string Body72Row = "body72";

        /// <summary>For the reel: the row drew this frame, the tell's level now (0 outside a danger wind-up), the danger wind-ups lit since boot, the lag's largest offset (m) and its share of the guard.</summary>
        public static bool Body72Shown;
        public static float Body72Tell, Body72LagMax, Body72LagShare;
        public static int Body72Tells;

        Material solid72, add72, enemy72;
        static readonly int RimColor72 = Shader.PropertyToID("_RimColor");
        Vector4 rim72;
        bool body72Applied;
        readonly MaterialPropertyBlock block72 = new MaterialPropertyBlock();
        Vector3[] lagX72, lagV72;
        bool[] lagLive72;
        string tellSaid72 = "";
        float tell72, sayClock72;
        static readonly int Flash72 = Shader.PropertyToID("_Flash"), RimOn72 = Shader.PropertyToID("_RimOn"), Form72 = Shader.PropertyToID("_Form");

        /// <summary>Once a frame, in Render after the sim says the worm is up: the row's state, the material swap, the tell's level. True: the row draws now.</summary>
        bool Body72Begin(SurvivalSim sim)
        {
            bool want = visual == null && NightRows.ViewOn(game.cfg, Body72Row);
            EnemyLook look = want ? EnemyLook.For(game) : null;
            if (want && (look == null || !look.ok)) want = false; // a Player without SKYGRIND/Enemy keeps the neon boxes
            if (want && enemy72 == null) enemy72 = look.body;
            if (want != body72Applied)
            {
                body72Applied = want;
                Material m = want ? enemy72 : solid72, mt = want ? solid72 : add72; // the body: the enemies' shader; the seams: opaque (additive plates stack: ten of them over one another clip R and G to yellow)
                for (int i = 0; i < baseR.Length; i++) { baseR[i].sharedMaterial = m; trimR[i].sharedMaterial = mt; }
                if (want)
                {
                    Color red = Look.Hex(UnityTuning.Tell36RedHex).linear * UnityTuning.Body72RimGain; // the rim in the danger red (linear, as EnemyLook sets it): the config's own rim is (255, 154, 82), whose G clips a lit orange body to yellow
                    rim72 = new Vector4(red.r, red.g, red.b, 1f);
                }
                Debug.Log("[body72] wyrm row body72 " + (want ? "on" : "off") + ": material " + (want ? "SKYGRIND/Enemy (rim, form, ink)" : "neon (flat)") + ", base 0x"
                    + (want ? UnityTuning.Body72Hex.ToString("x6") : UnityTuning.Wyrm47AshHex.ToString("x6")) + ", lag " + (want ? UnityTuning.Body72LagHzHead.ToString("0.#", CultureInfo.InvariantCulture) + "-"
                    + UnityTuning.Body72LagHzTail.ToString("0.#", CultureInfo.InvariantCulture) + " Hz z " + UnityTuning.Body72LagZeta.ToString("0.##", CultureInfo.InvariantCulture) : "none"));
            }
            Body72Shown = want;
            tell72 = 0f;
            if (want)
            {
                BossAttack63 a = BossAttackClock63.Read(sim, BossAttackClock63.Wyrm);
                bool windup = a.phase == BossPhase63.Windup && !string.IsNullOrEmpty(a.attack);
                // every wind-up but the roar (the core opening is no attack): the body goes danger; the head keeps its punch window's yellow (the parry tell, DECIDE63 D-1), so
                // the aggro charge reads "body: danger, head: punch it" (BossTell63's own column skips aggro; this glow is the body's)
                if (windup && a.attack != "roar")
                {
                    tell72 = BossTell63.Level(a.Windup01);
                    if (tellSaid72 != a.attack)
                    {
                        tellSaid72 = a.attack;
                        Body72Tells++;
                        Debug.Log("[body72] wyrm tell " + a.attack + ": level " + tell72.ToString("0.00", CultureInfo.InvariantCulture) + ", trim gain "
                            + (UnityTuning.Body72TellGain * tell72).ToString("0.0", CultureInfo.InvariantCulture) + " | row body72 on");
                    }
                }
                else if (!windup) tellSaid72 = "";
            }
            Body72Tell = tell72;
            return want;
        }

        /// <summary>The body piece's drawn colour with the row on: the warm ash lifted, NIGHT47's molten heat on it, the tell's danger over it. The head's punch window and open
        /// head (`special`) keep NIGHT47 / NIGHT63's own colour.</summary>
        Color Body72Colour(int i, int n, float heat, Color current, bool special)
        {
            if (special) return current;
            bool head = i == 0;
            Color c = Color.Lerp(Look.Hex(head ? UnityTuning.Body72HeadHex : UnityTuning.Body72Hex), Look.Hex(head ? UnityTuning.Wyrm47HotHex : UnityTuning.Wyrm47GlowHex),
                (head ? UnityTuning.Wyrm47JawHeat : UnityTuning.Wyrm47BodyHeat) * heat);
            if (tell72 > 0f)
            {
                float fall = Mathf.Lerp(1f, UnityTuning.Body72TellTail, i / (float)Mathf.Max(1, n - 1));
                c = Color.Lerp(c, Look.Hex(UnityTuning.Tell36RedHex), Mathf.Clamp01(UnityTuning.Body72TellBody * tell72 * fall));
            }
            return c;
        }

        /// <summary>The seams' and the trim plates' colour with the row on and a danger wind-up: the danger red-orange (routed as the wyrm's seams), HDR.</summary>
        Color Body72Trim(Color current)
        {
            if (tell72 <= 0f) return current;
            return BossColours63.Hot(game.cfg, "wyrm.seams", Look.Hex(UnityTuning.Tell36RedHex), UnityTuning.Body72TellGain * tell72);
        }

        /// <summary>A body piece's block for SKYGRIND/Enemy (what EnemyLook.Tint sets, without its cache: the renderer may have worn the neon block a frame ago).</summary>
        void Tint72(Renderer r, Color c)
        {
            if (Skin52.Built) Skin52.Bake(r); // the ink's gaps at a box's corners: the mesh swapped once for its smoothed-normal copy
            Color v = c * UnityTuning.CrunchEnemyValue;
            v.a = 1f;
            block72.Clear();
            block72.SetColor(BaseColor, v);
            block72.SetFloat(Flash72, 0f);
            block72.SetFloat(RimOn72, 1f);
            block72.SetVector(RimColor72, rim72);
            block72.SetFloat(Form72, EnemyLook.Form);
            block72.SetFloat(NearFade63.NearFadeId, 1f);
            r.SetPropertyBlock(block72);
        }

        /// <summary>DrawMotion63's first act each frame (the pieces still on their rigid pose): the springs follow the rigid centres, row on or off.</summary>
        void Lag72Step(int n)
        {
            if (lagX72 == null || lagX72.Length < n)
            {
                lagX72 = new Vector3[n];
                lagV72 = new Vector3[n];
                lagLive72 = new bool[n];
            }
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            for (int i = 1; i < n; i++)
            {
                if (!baseT[i].gameObject.activeSelf) { lagLive72[i] = false; continue; }
                Vector3 target = baseT[i].position;
                if (!lagLive72[i] || (lagX72[i] - target).sqrMagnitude > UnityTuning.Body72LagResetM * UnityTuning.Body72LagResetM)
                {
                    lagX72[i] = target;
                    lagV72[i] = Vector3.zero;
                    lagLive72[i] = true;
                    continue;
                }
                float hz = Mathf.Lerp(UnityTuning.Body72LagHzHead, UnityTuning.Body72LagHzTail, i / (float)Mathf.Max(1, n - 1));
                float x = lagX72[i].x, vx = lagV72[i].x, y = lagX72[i].y, vy = lagV72[i].y, z = lagX72[i].z, vz = lagV72[i].z;
                Spring72Math.Step(ref x, ref vx, target.x, hz, UnityTuning.Body72LagZeta, dt);
                Spring72Math.Step(ref y, ref vy, target.y, hz, UnityTuning.Body72LagZeta, dt);
                Spring72Math.Step(ref z, ref vz, target.z, hz, UnityTuning.Body72LagZeta, dt);
                lagX72[i] = new Vector3(x, y, z);
                lagV72[i] = new Vector3(vx, vy, vz);
            }
        }

        /// <summary>Piece i's lag offset (the spring's centre minus the rigid one), held to Body72LagGuardShare of `guard` (its own limitShare x half width); zero off a live spring.</summary>
        Vector3 Lag72Offset(int i, Vector3 rigid, float guard)
        {
            if (lagX72 == null || i >= lagX72.Length || !lagLive72[i]) return Vector3.zero;
            Vector3 d = lagX72[i] - rigid;
            float lim = guard * UnityTuning.Body72LagGuardShare, m = d.magnitude;
            if (m > lim) d *= lim / Mathf.Max(1e-6f, m);
            if (d.magnitude > Body72LagMax) { Body72LagMax = d.magnitude; Body72LagShare = guard > 1e-6f ? d.magnitude / guard : 0f; }
            return d;
        }

        /// <summary>The lag's proof line, every Body72SayEveryS s of a drawn fight (the row's state in it: OFF prints the springs' state, never applied).</summary>
        void Lag72Say(int n)
        {
            sayClock72 += Time.deltaTime;
            if (sayClock72 < UnityTuning.Body72SayEveryS) return;
            sayClock72 = 0f;
            CultureInfo ci = CultureInfo.InvariantCulture;
            Debug.Log("[body72] wyrm lag: " + (n - 1) + " pieces, largest " + Body72LagMax.ToString("0.000", ci) + " m (share " + Body72LagShare.ToString("0.00", ci) + " of the guard) | row body72 "
                + (body72Applied ? "on" : "off"));
            Body72LagMax = Body72LagShare = 0f;
        }
    }
}
