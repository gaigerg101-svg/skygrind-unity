// PG3 (AME 294 Portfolio Game 3, GG 2026-10-28: "Lift it" — CLAUDE.md rule 7's no-audio rule lifted for the assignment):
// the game's sound effects. A VIEW row, design.n74.sound74 (NightRows.ViewOn every frame; off: -skygrindRows72 sound74:0).
// It only listens: the EventBus names (PORT-STATUS.md "Events the port emits (for the audio map)") and the mover's events,
// and plays clips from Resources/Audio/PG3/<id> (Tools/pg3/sfx-gen.cjs from sounds.json + sounds2.json, ElevenLabs sound
// generation). Nothing here touches the sim, so the golden vectors and the replays cannot move. A missing clip is silent.
// The sound pass (docs/pg3/PG3-SOUND-PROMPT.md section 3): round-robin variants (<id>, <id>-2, <id>-3), the charged shots and
// their charge loop, every enemy kind's tell and death, each boss's intro / tell / slam / down, UI, the low-health heartbeat,
// the music duck under big hits (MusicDirector reads DuckUntil) and the slow-motion pitch-down.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Pg3AudioProbe
    {
        public static int Played, Missing, Skipped;
        public static readonly Dictionary<string, int> ByClip = new Dictionary<string, int>();

        public static void Reset() { Played = Missing = Skipped = 0; ByClip.Clear(); }

        public static string Line()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kv in ByClip) sb.Append(kv.Key).Append('=').Append(kv.Value).Append(' ');
            return $"[sound74] played {Played}, missing {Missing}, rate-skipped {Skipped}, kinds {ByClip.Count}; {sb}";
        }
    }

    public sealed class Pg3Audio : MonoBehaviour
    {
        public const string Row = "sound74";
        static Pg3Audio inst;

        /// <summary>Real time until which the music sits ducked under a big hit (MusicDirector reads it).</summary>
        public static float DuckUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Pg3Audio>() != null) return;
            var go = new GameObject("Pg3Audio");
            DontDestroyOnLoad(go);
            go.AddComponent<Pg3Audio>();
        }

        /// <summary>A UI sound (UiNav / UiPress call it); silent with the row off or before the game hooks.</summary>
        public static void Ui(string id) { if (inst != null) inst.Play(id, false); }

        SurvivalGame game;
        Mover hookedMover;
        AudioSource[] voices; // a small round-robin pool: each clip keeps the pitch it started with
        AudioSource music, chargeLoop, slideLoop, heart, windLoop, ambLoop;
        double stepPhase; // footsteps: 0..1 between two steps, advanced by the ground speed
        float gainNext = 1f, pitchNext = 1f; // the next Play's own volume / pitch scale (the hit's damage); reset after each play
        AudioListener listener;
        int voice, jitter;
        bool on;
        string pendingFire; // a plain shot waits to the end of the frame: a charged release replaces it
        float torchAt = -99f;
        AudioSource torchLoop;
        string pendingDeath; // a kill's kind death, waiting for its kill confirm's position
        AudioSource[] voices3d; // the positioned voices (enemy deaths, tells), each on its own child object
        AudioSource playOn; // Play's voice when set (Play3D), else the next flat voice
        int voice3d;
        Campaign24.Phase lastPhase = Campaign24.Phase.None;
        int lastRank;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, int> variants = new Dictionary<string, int>();
        readonly Dictionary<string, int> nextVariant = new Dictionary<string, int>();
        readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();

        void Awake()
        {
            inst = this;
            Pg3Volume.ApplyMaster(); // PG3 A1: the player's MASTER volume on the listener from the first frame (the SFX and MUSIC levels are read where each voice plays)
            voices = new AudioSource[UnityTuning.Pg3Voices];
            for (int i = 0; i < voices.Length; i++) voices[i] = Source(false);
            music = Source(true);
            music.volume = 0f;
            chargeLoop = Source(true);
            slideLoop = Source(true);
            heart = Source(true);
            torchLoop = Source(true);
            windLoop = Source(true);
            ambLoop = Source(true);
            voices3d = new AudioSource[UnityTuning.Pg3Voices3D];
            for (int i = 0; i < voices3d.Length; i++)
            {
                var child = new GameObject("Pg3Voice3D");
                child.transform.SetParent(transform, false);
                AudioSource s = child.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = UnityTuning.Pg3SpatialBlend; // mostly 3D, a little flat so a far tell is still heard
                s.rolloffMode = AudioRolloffMode.Linear;
                s.minDistance = UnityTuning.Pg3Near3D;
                s.maxDistance = UnityTuning.Pg3Far3D;
                s.dopplerLevel = 0f;
                voices3d[i] = s;
            }
        }

        AudioSource Source(bool loop)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.loop = loop;
            return s;
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
            if (Pg3Volume.Muted) AudioListener.volume = 0f; // -skygrindMute: silent run (held every frame)
            if (game == null || game.cfg == null) return;

            on = NightRows.ViewOn(game.cfg, Row); // read every frame, like every view row
            if (game.sim != null && game.mover != hookedMover) HookMover(game.mover);
            EnsureListener();
            bool playing = on && game.flow == Flow.Playing && !game.attractOn;

            // the old single bed: only while the music director's row is off (MusicDirector owns the music when it is on)
            bool director = NightRows.ViewOn(game.cfg, MusicDirector.Row);
            AudioClip bed = director ? null : Clip("music-loop");
            if (bed != null && music.clip != bed) { music.clip = bed; music.Play(); }
            float want = director || !on || game.attractOn ? 0f : game.flow == Flow.Playing ? UnityTuning.Pg3MusicVolume : UnityTuning.Pg3MusicVolume * UnityTuning.Pg3MusicMenuDuck;
            music.volume = Mathf.MoveTowards(music.volume, want * Pg3Volume.Music, Time.unscaledDeltaTime / UnityTuning.Pg3MusicFadeS); // PG3 A1: the player's MUSIC level

            // the loops: the charge (KESTREL / THUNDERCLAP / gauntlet charge, 0..1 polled), the slide, the low-health heartbeat
            Loadout lo = game.sim?.loadout;
            double charge = lo != null ? lo.Charge35Level : 0;
            // the charge builds: louder and higher as it fills (GG 2026-10-28: the charge "too basic ... more powerful")
            Loop(chargeLoop, "charge-loop", playing && charge > 0.02 && charge < 0.999, UnityTuning.Pg3LoopVolume * (UnityTuning.Pg3ChargeVolFrom + (1f - UnityTuning.Pg3ChargeVolFrom) * (float)charge), 0.75f + 0.6f * (float)charge);
            Loop(slideLoop, "slide-loop", playing && game.mover != null && game.mover.state == MoveState.Slide, UnityTuning.Pg3LoopVolume * 0.6f, 1f);
            Loop(torchLoop, "torch-loop", playing && Time.unscaledTime - torchAt < UnityTuning.Pg3TorchGapS, UnityTuning.Pg3LoopVolume, 1f);

            // the body: footsteps paced by the ground speed, the wind rush that grows with speed, the level's ambience bed
            Mover mv = game.sim != null ? game.mover : null;
            float speed = mv != null ? (float)mv.Speed : 0f;
            bool walking = playing && mv != null && mv.grounded && mv.state == MoveState.Ground && speed > UnityTuning.Pg3StepMinSpeed;
            if (walking)
            {
                stepPhase += Time.deltaTime * speed / UnityTuning.Pg3StepStride;
                if (stepPhase >= 1) { stepPhase -= 1; Play("step", false); }
            }
            else stepPhase = 0.6; // the first step lands soon after starting to move
            float wind = Mathf.Clamp01((speed - UnityTuning.Pg3WindFrom) / (UnityTuning.Pg3WindFull - UnityTuning.Pg3WindFrom));
            Loop(windLoop, "wind-loop", playing && wind > 0.01f, UnityTuning.Pg3WindVolume * wind, 0.9f + 0.3f * wind);
            Loop(ambLoop, "amb-" + MusicDirector.LevelKeyOf(game), on && !game.attractOn && (game.flow == Flow.Playing || game.flow == Flow.Paused), UnityTuning.Pg3AmbVolume, 1f);
            double hpShare = game.health != null && game.health.max > 0 ? game.health.hp / game.health.max : 1;
            Loop(heart, "heartbeat-loop", playing && hpShare < UnityTuning.Pg3LowHpShare && hpShare > 0, UnityTuning.Pg3LoopVolume, 1f + 0.25f * (float)(1 - hpShare / UnityTuning.Pg3LowHpShare));

            // a campaign fight room locking (phase None/Cleared -> Fight): the doors slam
            if (game.sim != null && game.sim.campaignOn && game.sim.campaign != null)
            {
                Campaign24.Phase ph = game.sim.campaign.phase;
                if (ph == Campaign24.Phase.Fight && lastPhase != Campaign24.Phase.Fight) Play("room-enter", true);
                lastPhase = ph;
            }
            // the style rank going up
            if (game.sim != null && game.style != null)
            {
                int rank = game.style.rank;
                if (rank > lastRank && rank >= 2) Play("rank-up", false);
                lastRank = rank;
            }
        }

        void LateUpdate()
        {
            if (pendingFire != null) { Play(pendingFire, false); pendingFire = null; }
            if (pendingDeath != null) { Play(pendingDeath, false); pendingDeath = null; }
        }

        /// <summary>An enemy-side clip at its world position (a 3D voice: you hear where the threat is), through Play's rules.</summary>
        void Play3D(string id, Vec3 at)
        {
            AudioSource s = voices3d[voice3d];
            voice3d = (voice3d + 1) % voices3d.Length;
            s.transform.position = WebSpace.ToUnity(at);
            playOn = s;
            Play(id, false);
            playOn = null;
        }

        void Loop(AudioSource s, string id, bool want, float vol, float pitch)
        {
            if (want)
            {
                AudioClip c = Clip(id);
                if (c == null) return;
                if (s.clip != c) s.clip = c;
                s.pitch = pitch * SlowPitch();
                s.volume = Mathf.MoveTowards(s.volume, vol * Pg3Volume.Sfx, Time.unscaledDeltaTime * 6f); // PG3 A1: the player's SFX level
                if (!s.isPlaying) s.Play();
            }
            else if (s.isPlaying)
            {
                s.volume = Mathf.MoveTowards(s.volume, 0f, Time.unscaledDeltaTime * 8f);
                if (s.volume <= 0.001f) s.Stop();
            }
        }

        /// <summary>The slow-motion pitch-down: the impact freeze / slow-mo world scale and the ride-aim slow (GG: "extremely slow").</summary>
        float SlowPitch()
        {
            if (game == null) return 1f;
            float world = (float)game.WorldScale;
            float aim = (float)game.SlowAim01;
            float p = Mathf.Lerp(1f, UnityTuning.Pg3SlowPitch, Mathf.Clamp01(Mathf.Max(1f - world, aim)));
            return Mathf.Clamp(p, UnityTuning.Pg3SlowPitch, 1f);
        }

        // the player proof: what played, printed to the Player log at quit (and every new run)
        void OnApplicationQuit() => Debug.Log(Pg3AudioProbe.Line());

        void Hook(SurvivalGame g)
        {
            game = g;
            hookedMover = null;
            EventBus ev = g.events;
            // weapons
            ev.On("fire", o =>
            {
                // the TORCH fires every tick: one burst at the start, then the roaring loop while the ticks keep coming
                if (o as string == "torch") { if (Time.unscaledTime - torchAt > UnityTuning.Pg3TorchGapS) pendingFire = "fire-torch"; torchAt = Time.unscaledTime; return; }
                pendingFire = "fire-" + FireClip(o as string);
            });
            ev.On<ChargeShot27Event>("charge35Fire", e => { pendingFire = null; Play(ChargedClip(e?.gun), true, 0.5f); });
            ev.On("charge35Full", _ => Play("charge-ready", false));
            ev.On("slug43", _ => { pendingFire = null; Play("fire-slug", true, 0.4f); });
            ev.On("pulse27Blast", _ => Play("gauntlet-pulse", true, 0.4f));
            ev.On("pulse47Blast", _ => Play("gauntlet-pulse", true, 0.4f));
            ev.On("punch", _ => Play("fire-gauntlet", false));
            ev.On("shotgunPump", _ => Play("pump", false));
            ev.On("ricochet", _ => Play("ricochet", false));
            ev.On("weaponSwap", _ => Play("weapon-swap", false));
            ev.On("adsStart", _ => Play("ads-in", false));
            ev.On("parry", _ => { Play("parry", true, 0.6f); });
            // reward
            // a hit sized by its damage: a heavy hit (a slug, a charged shot, a punch) louder and a little lower than a chip
            ev.On<HitEvent>("hit", e =>
            {
                float heavy = e != null ? Mathf.Clamp01((float)(e.damage / UnityTuning.Pg3HitHeavyDamage)) : 0f;
                gainNext = UnityTuning.Pg3HitGainFrom + (1f - UnityTuning.Pg3HitGainFrom) * heavy;
                pitchNext = 1.06f - 0.14f * heavy;
                Play("hit", false);
            });
            ev.On<KillEvent>("kill", e =>
            {
                Play("kill", false);
                // the kind's death waits for the kill confirm, which carries the position (3D); unconfirmed, it plays flat at the frame's end
                if (e != null && e.kind != null && e.kind != "boss") pendingDeath = e.kind + "-death";
            });
            // the kill confirm's tier (killConfirm.js: 0 plain, 1 head / melee, 2 multikill): the head crack, the combo arpeggio
            ev.On<KillConfirmEvent>(MetaEvents.KillConfirm, e =>
            {
                if (e == null || e.owner != "player") return;
                if (pendingDeath != null) { Play3D(pendingDeath, e.position); pendingDeath = null; }
                if (e.tier >= 2) Play("multikill", false);
                else if (e.tier == 1) Play("headshot", false);
            });
            ev.On("points", _ => Play("points", false));
            ev.On("buy", _ => Play("buy", false));
            ev.On("powerUpGrab", _ => Play("powerup", false));
            ev.On("pickup29", _ => Play("pickup", false));
            ev.On("weaponEarned", _ => Play("weapon-earned", true, 1.2f));
            ev.On("roomClear", _ => Play("room-clear", false));
            ev.On("checkpoint", _ => Play("checkpoint", false));
            ev.On("regenStart", _ => Play("regen", false));
            ev.On("boxOpen", _ => Play("box-open", false));
            ev.On("gateOpen", _ => Play("gate-open", false));
            ev.On("exfilCall", _ => Play("exfil-call", false));
            ev.On("roundBegin", _ => Play("round-begin", false));
            // damage
            // taking damage: the music ducks for a beat so the hit lands
            ev.On<HurtEvent>("hurt", e => { if (e != null && e.owner == "player") Play("hurt", true, UnityTuning.Pg3HurtDuckS); });
            ev.On<DeathEvent>("death", e => { if (e != null && e.owner == "player") Play("lose", true, 1.5f); });
            ev.On("huskWindup", _ => Play("enemy-windup", false));
            ev.On("huskAttack", _ => Play("husk-attack", false));
            ev.On("spitterTell", _ => Play("spitter-tell", false));
            ev.On("spitterVolley", _ => Play("spitter-volley", false));
            ev.On("swarmerPounce", _ => Play("swarmer-pounce", false));
            ev.On("chargerCharge", _ => Play("charger-charge", false));
            ev.On("enemyBlocked", _ => Play("warden-block", false));
            ev.On("enemyShot", _ => Play("drone-shot", false));
            ev.On("burrowEmerge43", _ => Play("burrow-emerge", false));
            ev.On("spawnTelegraph", _ => Play("spawn-telegraph", false));
            // the shared wind-up (Tell36: every enemy and boss attack with a tell): a bright parry cue for a Yellow (parryable) tell,
            // a hard warning for a Red one, so the ear knows "parry it" from "get out" (the colour's sound)
            ev.On<Tell36>(Tells36.Event, t => { if (t != null) Play3D(t.Parryable ? "tell-parry" : "tell-danger", t.at); });
            // bosses (BossEvent.boss names the boss: wick, brawl40 = THE ICEBREAKER, wyrm, loom37, crown37)
            ev.On<BossEvent>("bossIntro", e => Play(BossClip(e, "intro", "boss-intro"), true, 2.5f));
            ev.On<BossEvent>("bossRise", e => { if (e != null && e.boss == "wick") Play(BossClip(e, "intro", "boss-intro"), true, 2.5f); }); // THE WICK skips BossKit.Start: no bossIntro
            ev.On<BossEvent>("bossTell", e => Play(BossClip(e, "tell", null), false));
            ev.On<BossEvent>("bossSlam", e => Play(BossClip(e, "slam", null), true, 0.6f));
            ev.On<BossEvent>("bossHit", _ => Play("boss-hit", false));
            ev.On<BossEvent>("bossPhase", _ => Play("boss-phase", true, 1f));
            ev.On<BossEvent>("bossStun", _ => Play("boss-stun", true, 0.5f));
            ev.On<BossEvent>("bossWindow", _ => Play("boss-window", false));
            ev.On<BossEvent>("bossRoar", _ => Play("boss-roar", true, 1f));
            ev.On<BossEvent>("bossVolley", _ => Play("boss-volley", false));
            ev.On<BossEvent>("bossShield", _ => Play("boss-shield", false));
            ev.On<BossEvent>("bossDown", e => Play(BossClip(e, "down", "boss-down"), true, 3f));
            // end
            ev.On("exfilDone", _ => Play("win", true, 2f));
            ev.On("roomRetry", _ => Play("retry", false));
            g.OnNewRun += () => { if (Pg3AudioProbe.Played + Pg3AudioProbe.Missing > 0) Debug.Log(Pg3AudioProbe.Line()); lastPlayed.Clear(); lastPhase = Campaign24.Phase.None; lastRank = 0; };
        }

        void HookMover(Mover m)
        {
            if (hookedMover != null) hookedMover.Events -= OnMover;
            hookedMover = m;
            if (m != null) m.Events += OnMover;
        }

        void OnMover(MoverEvent e)
        {
            switch (e.type)
            {
                case MoverEventType.Jump: Play("jump", false); break;
                case MoverEventType.DoubleJump: Play("double-jump", false); break;
                case MoverEventType.WallJump: Play("wall-jump", false); break;
                case MoverEventType.Dash:
                case MoverEventType.DashJump:
                case MoverEventType.BoostJump: Play("dash", false); break;
                case MoverEventType.DashFail: Play("dash-fail", false); break;
                case MoverEventType.Land: if (e.fallSpeed > UnityTuning.Pg3HardLandSpeed) Play("land-hard", true, 0.3f); else Play("land", false); break;
                case MoverEventType.Slam: Play("slam", true, 0.5f); break;
                case MoverEventType.SlideStart: Play("slide-start", false); break;
                case MoverEventType.WallRideStart: Play("wallride", false); break;
                case MoverEventType.RopeFire: Play("grapple", false); break;
                case MoverEventType.RopeAttach: Play("grapple-attach", false); break;
                case MoverEventType.RopeRelease: Play("grapple-release", false); break;
                case MoverEventType.JumpPad: Play("jump-pad", false); break;
                case MoverEventType.StaminaFull: Play("stamina-full", false); break;
            }
        }

        static string FireClip(string weapon)
        {
            switch (weapon)
            {
                case "shotgun": return "thunderclap";
                case "torch": return "torch";
                default: return "kestrel"; // the revolver, and the sandbox guns until they get their own
            }
        }

        static string ChargedClip(string gun)
        {
            if (gun == "shotgun") return "fire-slug";
            if (gun == "gauntlet") return "gauntlet-pulse";
            return "fire-kestrel-charged";
        }

        /// <summary>The boss's own clip (wick-slam ...) when it exists, else the shared fallback (null: silent).</summary>
        string BossClip(BossEvent e, string beat, string fallback)
        {
            string key = BossKey(e?.boss);
            if (key != null && Clip(key + "-" + beat) != null) return key + "-" + beat;
            return fallback;
        }

        public static string BossKey(string boss)
        {
            switch (boss)
            {
                case "wick": return "wick";
                case "brawl40": case "glacier": case "hulk": return "icebreaker";
                case "wyrm": return "wyrm";
                case "loom": case "loom37": return "loom";
                case "crown": case "crown37": return "crown";
                default: return null;
            }
        }

        void Play(string id, bool duck, float duckSeconds = 0f)
        {
            float gainScale = gainNext, pitchScale = pitchNext;
            gainNext = 1f; pitchNext = 1f; // a one-shot scale (the hit's damage) applies to this play only, even when it is skipped
            if (id == null || !on || game == null || game.attractOn) return;
            string pick = PickVariant(id);
            if (pick == null) { Pg3AudioProbe.Missing++; return; }
            float now = Time.unscaledTime;
            if (lastPlayed.TryGetValue(id, out float t) && now - t < UnityTuning.Pg3MinRepeatS) { Pg3AudioProbe.Skipped++; return; }
            lastPlayed[id] = now;
            if (duck) DuckUntil = Mathf.Max(DuckUntil, now + duckSeconds);
            // a small fixed pitch walk so a run of the same clip doesn't machine-gun (view only: no gameplay randomness)
            jitter = (jitter + 1) % 5;
            AudioSource s = playOn;
            if (s == null) { s = voices[voice]; voice = (voice + 1) % voices.Length; }
            s.Stop();
            s.pitch = (1f + (jitter - 2) * UnityTuning.Pg3PitchStep) * SlowPitch();
            s.pitch *= pitchScale;
            s.PlayOneShot(clips[pick], UnityTuning.Pg3SfxVolume * Pg3Volume.Sfx * gainScale); // PG3 A1: x the player's SFX level (flat and 3D voices both come through here)
            Pg3AudioProbe.Played++;
            Pg3AudioProbe.ByClip[id] = Pg3AudioProbe.ByClip.TryGetValue(id, out int n) ? n + 1 : 1;
        }

        /// <summary>Round robin over <id>, <id>-2, <id>-3 (whichever exist); null when none does.</summary>
        string PickVariant(string id)
        {
            if (!variants.TryGetValue(id, out int count))
            {
                count = Clip(id) != null ? 1 : 0;
                if (count == 1) for (int v = 2; v <= 4 && Clip(id + "-" + v) != null; v++) count = v;
                variants[id] = count;
            }
            if (count == 0) return null;
            int k = nextVariant.TryGetValue(id, out int nv) ? nv : 0;
            nextVariant[id] = (k + 1) % count;
            return k == 0 ? id : id + "-" + (k + 1);
        }

        AudioClip Clip(string id)
        {
            if (clips.TryGetValue(id, out AudioClip c)) return c;
            c = Resources.Load<AudioClip>("Audio/PG3/" + id);
            clips[id] = c; // a missing clip is cached as null: one lookup, then silence
            return c;
        }

        void EnsureListener()
        {
            if (listener != null && listener.isActiveAndEnabled) return;
            listener = FindAnyObjectByType<AudioListener>();
            if (listener != null) return;
            Camera c = game.cam != null ? game.cam : Camera.main;
            if (c != null) listener = c.gameObject.AddComponent<AudioListener>();
        }
    }
}
