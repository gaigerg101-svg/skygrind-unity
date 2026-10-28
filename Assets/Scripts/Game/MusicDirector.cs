// PG3 (docs/pg3/PG3-SOUND-PROMPT.md section 1): music that follows the fight. A VIEW row, design.n74.music74 (off:
// -skygrindRows72 music74:0; the old single bed in Pg3Audio comes back). Tracks are Resources/Audio/PG3/Music/<id> (Tools/pg3/
// music-gen.cjs from music.json, ElevenLabs Music; streamed, Assets/Editor/Pg3AudioImport.cs):
//   title, select, results, campaign-complete, <level>-calm, <level>-combat, boss-<boss>, sting-clear, sting-death.
// State: the flow (Title / Playing / Paused / Down / Results), the level select, the campaign room (Fight / None / Cleared), the
// enemies alive (survival), the boss alive. Two sources crossfade; each track resumes where it was left (calm and combat trade
// places without restarting). The music ducks under the big hits (Pg3Audio.DuckUntil), goes muffled under low health (a low-pass)
// and pitches down in slow motion. Reads the game only; nothing here touches the sim.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class MusicDirectorProbe
    {
        public static readonly Dictionary<string, float> Seconds = new Dictionary<string, float>();
        public static int Switches, Stings, Missing;
        public static string Current = "-";

        public static string Line()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kv in Seconds) sb.Append(kv.Key).Append('=').Append(kv.Value.ToString("0")).Append("s ");
            return $"[music74] now {Current}, switches {Switches}, stings {Stings}, missing {Missing}; {sb}";
        }
    }

    public sealed class MusicDirector : MonoBehaviour
    {
        public const string Row = "music74";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<MusicDirector>() != null) return;
            var go = new GameObject("MusicDirector");
            DontDestroyOnLoad(go);
            go.AddComponent<MusicDirector>();
        }

        SurvivalGame game;
        AudioSource a, b, sting; // a / b crossfade; the active one is `cur`
        AudioSource cur, old;
        AudioLowPassFilter lowPass;
        string want, playing;
        bool bossPhase2, wasFight;
        int fights;
        float fade = 1f, fadeTime = 1.5f, calmSince = -1f, duck = 1f;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, float> resumeAt = new Dictionary<string, float>();

        void Awake()
        {
            a = Source(true);
            b = Source(true);
            sting = Source(false);
            lowPass = gameObject.AddComponent<AudioLowPassFilter>();
            lowPass.cutoffFrequency = 22000f;
            cur = a;
            old = b;
        }

        AudioSource Source(bool loop)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0f;
            s.volume = 0f;
            return s;
        }

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g != game && g != null) Hook(g);
            if (game == null || game.cfg == null) return;
            bool on = NightRows.ViewOn(game.cfg, Row) && NightRows.ViewOn(game.cfg, Pg3Audio.Row);
            float dt = Time.unscaledDeltaTime;

            string target = on ? Pick() : null;
            if (target != want) { want = target; fadeTime = target != null && target.StartsWith("boss-") ? UnityTuning.MusicFadeBossS : UnityTuning.MusicFadeS; }
            if (want != playing) Switch(want);
            // the loop seam: an MP3 loop has a gap (RESEARCH.md 4), so near a track's end it crossfades into itself from the top
            else if (playing != null && fade >= 1f && cur.isPlaying && cur.clip != null && cur.clip.length > 3f * UnityTuning.MusicLoopXfadeS
                     && cur.time > cur.clip.length - UnityTuning.MusicLoopXfadeS)
            {
                string id = playing;
                resumeAt[id] = 0f;
                playing = null; // so Switch doesn't store the tail as the resume point
                fadeTime = UnityTuning.MusicLoopXfadeS;
                Switch(id);
            }

            // the crossfade, the duck, the low-pass, the slow-motion pitch
            fade = Mathf.MoveTowards(fade, 1f, dt / Mathf.Max(0.05f, fadeTime));
            bool ducked = Time.unscaledTime < Pg3Audio.DuckUntil;
            duck = Mathf.MoveTowards(duck, ducked ? UnityTuning.MusicDuck : 1f, dt / (ducked ? 0.06f : 0.5f));
            float menu = game.flow == Flow.Playing ? 1f : UnityTuning.Pg3MusicMenuDuck;
            float player = Pg3Volume.Music; // PG3 A1: the player's MUSIC level (tracks and stings)
            float vol = UnityTuning.MusicVolume * duck * (game.flow == Flow.Title || game.flow == Flow.Results ? 1f : menu) * player;
            cur.volume = vol * fade;
            old.volume = vol * (1f - fade);
            if (fade >= 1f && old.isPlaying) old.Stop();
            sting.volume = UnityTuning.MusicVolume * 1.2f * player;

            double hpShare = game.health != null && game.health.max > 0 ? game.health.hp / game.health.max : 1;
            bool low = game.flow == Flow.Playing && hpShare > 0 && hpShare < UnityTuning.Pg3LowHpShare;
            float cutoff = low ? UnityTuning.MusicLowHpCutoff : 22000f;
            lowPass.cutoffFrequency = Mathf.MoveTowards(lowPass.cutoffFrequency, cutoff, dt * 30000f);

            float slow = Mathf.Clamp01(Mathf.Max(1f - (float)game.WorldScale, (float)game.SlowAim01));
            float pitch = Mathf.Lerp(1f, UnityTuning.MusicSlowPitch, slow);
            cur.pitch = old.pitch = pitch;

            if (playing != null) MusicDirectorProbe.Seconds[playing] = (MusicDirectorProbe.Seconds.TryGetValue(playing, out float s) ? s : 0f) + dt;
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            g.events.On("roomClear", _ => Sting("sting-clear"));
            g.events.On("exfilDone", _ => Sting("sting-clear"));
            g.events.On<DeathEvent>("death", e => { if (e != null && e.owner == "player") Sting("sting-death"); });
            // the boss's second theme: a hit leaving it under the share of its health (bossPhase also fires at a fight's start: the
            // pg3-b boss63 reel played the second theme from the first second on it, so the phase event is not used)
            g.events.On<BossEvent>("bossHit", e => { if (e != null && e.maxHp > 0 && e.hp / e.maxHp < UnityTuning.MusicBoss2Share) bossPhase2 = true; });
            g.OnNewRun += () => { bossPhase2 = false; fights = 0; wasFight = false; calmSince = -1f; };
        }

        /// <summary>The track the game wants now (null: silence).</summary>
        string Pick()
        {
            if (game.attractOn) return Have("title");
            if (Ui.CampRes25Open) return Have("results");
            switch (game.flow)
            {
                case Flow.Title:
                    Ui ui = game.UiView;
                    return ui != null && ui.LevelSelectOpen26 ? Have("select") ?? Have("title") : Have("title");
                case Flow.Results: return Have("results");
                case Flow.Down: return playing; // keep the last track under the death sting
            }
            // playing or paused: the level's calm / combat, or its boss
            SurvivalSim sim = game.sim;
            if (sim == null) return Have("title");
            string level = LevelKey();
            if (sim.bossKit != null && sim.bossKit.AnyAlive()) // each campaign level has one boss: the level names its theme
            {
                string theme = "boss-" + BossOfLevel(level);
                return (bossPhase2 ? Have(theme + "-2") : null) ?? Have(theme) ?? Have("boss-wick") ?? Have(level + "-combat");
            }
            bossPhase2 = false;
            // the fight: a campaign room locked; in survival, a round in progress (the alive count drops to 0 between spawns: the
            // first pg3-b smoke switched 56 times in 227 s on it)
            bool fight = sim.campaignOn && sim.campaign != null ? sim.campaign.phase == Campaign24.Phase.Fight
                : game.run != null ? game.run.state == SurvivalRun.State.Fight : game.enemies != null && game.enemies.AliveCount > 0;
            float now = Time.unscaledTime;
            if (fight) { if (calmSince != -1f || !wasFight) fights++; calmSince = -1f; }
            else if (calmSince < 0f) calmSince = now;
            wasFight = fight;
            bool combat = fight || (calmSince >= 0f && now - calmSince < UnityTuning.MusicCalmAfterS && playing != null && playing.Contains("-combat"));
            // two combat themes per level, alternating fight by fight, so a long level doesn't loop one track
            string combatId = level + (fights % 2 == 0 ? "-combat-b" : "-combat");
            if (combat) return Have(combatId) ?? Have(level + "-combat") ?? Have("foundry-combat");
            return Have(level + "-calm") ?? Have("foundry-calm") ?? Have(level + "-combat");
        }

        string LevelKey() => LevelKeyOf(game);

        /// <summary>The campaign level's sound key (foundry / iceline / kiln / span / spire); foundry outside the campaign.</summary>
        public static string LevelKeyOf(SurvivalGame game)
        {
            string id = game?.sim?.campaignLevel?.id ?? game?.sim?.campaign?.Level?.id ?? "";
            if (id.Contains("iceline")) return "iceline";
            if (id.Contains("kiln")) return "kiln";
            if (id.Contains("span")) return "span";
            if (id.Contains("spire")) return "spire";
            return "foundry";
        }

        static string BossOfLevel(string level)
        {
            switch (level)
            {
                case "iceline": return "icebreaker";
                case "kiln": return "wyrm";
                case "span": return "loom";
                case "spire": return "crown";
                default: return "wick";
            }
        }

        string Have(string id)
        {
            if (id == null) return null;
            if (!clips.TryGetValue(id, out AudioClip c))
            {
                c = Resources.Load<AudioClip>("Audio/PG3/Music/" + id);
                clips[id] = c;
                if (c == null) MusicDirectorProbe.Missing++;
            }
            return c != null ? id : null;
        }

        void Switch(string id)
        {
            if (playing != null && cur.clip != null) resumeAt[playing] = cur.time;
            AudioSource t = old; old = cur; cur = t;
            fade = 0f;
            playing = id;
            MusicDirectorProbe.Current = id ?? "-";
            MusicDirectorProbe.Switches++;
            if (id == null) { cur.Stop(); cur.clip = null; return; }
            AudioClip c = clips[id];
            cur.clip = c;
            cur.time = resumeAt.TryGetValue(id, out float at) && at < c.length - 1f ? at : 0f;
            cur.Play();
        }

        void Sting(string id)
        {
            if (game == null || !NightRows.ViewOn(game.cfg, Row) || game.attractOn) return;
            AudioClip c = Have(id) != null ? clips[id] : null;
            if (c == null) return;
            sting.PlayOneShot(c);
            Pg3Audio.DuckUntil = Mathf.Max(Pg3Audio.DuckUntil, Time.unscaledTime + Mathf.Min(c.length, 3f));
            MusicDirectorProbe.Stings++;
        }

        void OnApplicationQuit() => Debug.Log(MusicDirectorProbe.Line());
    }
}
