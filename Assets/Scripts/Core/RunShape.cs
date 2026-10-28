// NIGHT11 (the run's shape): payloads shared by the run-shape systems (objectives.js, protocol.js, traps.js, and the
// port-only power switch, swarm drop and exfil). The web's systems emit `banner {text}` and the HUD shows it
// (main.js / hud.js); the port keeps that event name and field.
namespace Skygrind.Core
{
    /// <summary>The web's `banner` event: {text} (objectives.js, protocol.js, main.js).</summary>
    public sealed class BannerEvent
    {
        public string text;
    }
}
