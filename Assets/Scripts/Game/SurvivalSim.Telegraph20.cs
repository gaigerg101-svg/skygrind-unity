// NIGHT20 A1 (builder HV): the sim's door to the telegraph view (the shared-hook pattern of SurvivalSim.N20.cs). The Game
// hands its view (Telegraph20, Core's ITelegraph: telegraph.js's shapes in web space) to the sim once, at the views' init
// (SurvivalGame.Fx20.cs); the sim's bosses draw through it as bulwark.js draws through `kit.telegraph`. With no view (the
// harness, the tests, a run before the views) it stays null: a BossKit then keeps its NullTelegraph, and nothing a run does
// changes (the sim never reads a telegraph back except by the id it was given).
// Builder EN (block 2): build the kit with `TelegraphView` (null is fine: BossKit makes a NullTelegraph) and implement
// N20TelegraphViewSet in your SurvivalSim partial to hand a later view to the live kit (`bossKit.telegraph = view ??
// new NullTelegraph()`): the view is set after the sim is made.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        ITelegraph telegraphView20;

        /// <summary>The Game's telegraph view for the sim's bosses (null: none).</summary>
        public ITelegraph TelegraphView
        {
            get => telegraphView20;
            set
            {
                telegraphView20 = value;
                N20TelegraphViewSet(value);
            }
        }

        // EN: hand the view to the live BossKit (a partial without a body compiles to nothing).
        partial void N20TelegraphViewSet(ITelegraph view);
    }
}
