// NIGHT26 builder B: the bosses' views' calls into the game. The lead puts two lines in SurvivalGame.Wyrm24.cs (a lead-only file):
//   Wyrm24InitView:   Boss26InitViews();          Wyrm24RenderView(worldDt, alpha):   Boss26RenderViews(alpha);
// NIGHT27 builder B: THE LANTERN's view joins the same two calls (LanternView, level 1 with design.n27.boss1 on); the glacier view draws both NIGHT26's hull and NIGHT27's skater.
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        public HulkView hulkView; // THE SLAG HULK's body (level 1, boss1 off)
        public LanternView lanternView; // THE LANTERN (level 1, boss1 on)
        public GlacierView glacierView; // THE GLACIER's course and body (level 2)
        public GlacierView28 glacierView28; // NIGHT28: THE GLACIER on the banked rink (design.n28.glacier)
        public Boss29View boss29View; // NIGHT29 builder B: lane lines, frost fields, the Lantern's lava glow
        public RinkView28 rinkView28; // NIGHT28 A2: THE GLACIER's curved rink (design.n28.rink; shown only while the sim has a rink)

        public void Boss26InitViews()
        {
            hulkView = new HulkView(this);
            Boss36View.Current = new Boss36View(this); // NIGHT36 B: THE LANTERN on Boss36 (bossmodel36, the hall)
            Stag37View.Current = new Stag37View(this); // NIGHT37 S: THE STAG's canal and body
            Wick37View.Current = new Wick37View(this); // NIGHT37 W: THE WICK's stack, slag and body
            Loom37View.Current = new Loom37View(this); // NIGHT37 O: THE LOOM's canyon threads and body
            Brawl40View.Current = new Brawl40View(this); // NIGHT40 B2: THE ICEBREAKER (Brawl40View.cs)
            lanternView = new LanternView(this);
            glacierView = new GlacierView(this);
            glacierView28 = new GlacierView28(this);
            rinkView28 = new RinkView28(this);
            boss29View = new Boss29View(this);
            Boss30View.Current = new Boss30View(this); // NIGHT30 builder B: THE LANTERN RING and the Wyrm stagger view (view only)
            Boss31View.Current = new Boss31View(this); // NIGHT31 builder B: THE STAG and the NIGHT31 boss tells (view only)
        }

        public void Boss26RenderViews(double alpha)
        {
            hulkView?.Render(alpha);
            Boss36View.Current?.Render(alpha); // NIGHT36 B
            Stag37View.Current?.Render(alpha); // NIGHT37 S
            Wick37View.Current?.Render(alpha); // NIGHT37 W
            Loom37View.Current?.Render(alpha); // NIGHT37 O
            Brawl40View.Current?.Render(alpha); // NIGHT40 B2
            lanternView?.Render(alpha);
            glacierView?.Render(alpha);
            glacierView28?.Render(alpha);
            rinkView28?.Render(alpha);
            boss29View?.Render(alpha);
            Boss30View.Current?.Render(alpha); // NIGHT30 builder B
            Boss31View.Current?.Render(alpha); // NIGHT31 builder B
        }
    }
}
