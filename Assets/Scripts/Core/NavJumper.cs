// enemies/navJump.js (POWER P1): a husk on a jump / gap link crouches, then flies a ballistic arc to the link's landing.
// S6 fills this in; until then nothing jumps (the walk + drop field only).
namespace Skygrind.Core
{
    public class NavJumper
    {
        /// <summary>True while a nav jump owns the husk's body this step (it then only moves under gravity).</summary>
        public virtual bool Step(Husk husk, double dt, bool busy) => false;
    }
}
