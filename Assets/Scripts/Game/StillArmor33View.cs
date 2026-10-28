// NIGHT33 builder E (design.n33.bosses33 STILL ARMOR; the lead 11:47): the visible sign that a boss shrugs off a camper's hits. While the sim's still armor is up
// (event bossStillArmor33 {on}; SurvivalSim.Bosses33ArmorBody):
//   - every player hit on that boss throws a grey-white DEFLECT burst at the hit point (the "blocked" read: grey, not the damage flash's colour; CLAUDE.md rule 8:
//     red-orange stays the enemy's, cyan the player's);
//   - a pale grey shell stands round the boss body (a field column the body's size), dimming its glow and weak spots behind it, pulsing slowly.
// When the player moves (the armor drops) the shell goes at once. Nothing shows with bosses33 off (no event, no armored body). A plain class: no Unity API in field
// initializers (L-035); made once by SurvivalGame (the lead's line: Tools/night/n33e-stillarmor.cjs), Render(worldDt) once a frame; nothing allocates per frame.
// The slot line "MOVE" (danger priority) is builder U's slot: the same patch script adds it to UiHud33.Slot33. Numbers: UnityTuning.Spawns33.cs (StillArmor33*).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class StillArmor33View
    {
        readonly SurvivalGame game;
        readonly Particles particles;
        readonly Transform shell;
        readonly Material shellMat;
        readonly Color grey;
        bool up, shown;
        float time;

        public StillArmor33View(SurvivalGame game, Particles particles)
        {
            this.game = game;
            this.particles = particles;
            grey = UnityTuning.StillArmor33Grey;
            shellMat = DesignKit.Field(game, grey * UnityTuning.StillArmor33ShellAlpha, 0, 1);
            shell = DesignKit.Beam("still armor shell", null, 0.5f, 1, shellMat);
            shell.gameObject.SetActive(false);
            game.events.On("bossStillArmor33", o => up = o is bool b && b);
            game.events.On<HitEvent>("hit", e =>
            {
                if (!up || e.owner != "player" || game.sim == null) return;
                Enemy body = game.sim.Bosses33ArmorBody;
                if (body == null || body.id != e.enemy) return;
                particles?.Burst(e.position, grey, UnityTuning.StillArmor33SparkCount, UnityTuning.StillArmor33SparkSpeed, UnityTuning.StillArmor33SparkLife, UnityTuning.StillArmor33SparkScale);
            });
        }

        public void Render(double worldDt)
        {
            time += (float)worldDt;
            Enemy body = up && game.sim != null ? game.sim.Bosses33ArmorBody : null;
            bool show = body != null;
            if (show != shown)
            {
                shown = show;
                shell.gameObject.SetActive(show);
            }
            if (!show) return;
            float r = Mathf.Max(UnityTuning.StillArmor33MinRadius, (float)body.Radius * UnityTuning.StillArmor33Grow);
            float h = Mathf.Max(UnityTuning.StillArmor33MinHeight, (float)body.Height * UnityTuning.StillArmor33Grow);
            Vector3 at = WebSpace.ToUnity(body.position);
            shell.position = at + new Vector3(0, h / 2, 0);
            shell.localScale = new Vector3(r * 2, h / 2, r * 2);
            shellMat.SetFloat("_FadeTop", at.y + h);
            shellMat.SetFloat("_FadeLength", h);
            float pulse = 1 - UnityTuning.StillArmor33Pulse * (0.5f + 0.5f * Mathf.Sin(time * Mathf.PI * 2 * UnityTuning.StillArmor33PulseRate));
            shellMat.SetColor(DesignKit.FieldColor, grey * UnityTuning.StillArmor33ShellAlpha * pulse);
        }
    }
}
