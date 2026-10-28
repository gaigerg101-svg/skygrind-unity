// NIGHT12 (committed in NIGHT13 U3-26.3): make the main tree's Unity-generated csproj files usable in a git worktree,
// so an agent can compile Unity code with `dotnet build` (no Editor): Library\ hint paths become absolute (a worktree
// has no Library), and each project's explicit Compile list becomes one `dir\*.cs` glob per source folder it had (new
// files in those folders are compiled; a NEW folder needs its glob added by hand, e.g. Game\Input\*.cs).
// Usage: node Tools/wt-csproj.cjs <worktreeRoot>     The copies are gitignored (*.csproj): never commit them.
// Then: dotnet build <worktreeRoot>/Skygrind.Game.csproj (or Skygrind.Core.Tests.csproj, Skygrind.Game.Tests.csproj).
const fs = require('fs');
const path = require('path');
const main = 'C:\\Users\\gaige\\dev\\SKYGRIND-unity';
const dest = process.argv[2];
if (!dest || !fs.existsSync(dest)) throw new Error('usage: node wt-csproj.cjs <worktreeRoot>');
const projects = ['Skygrind.Core.csproj', 'Skygrind.Game.csproj', 'Skygrind.Core.Tests.csproj', 'Skygrind.Game.Tests.csproj', 'Skygrind.Bench.csproj', 'Assembly-CSharp-Editor.csproj', 'Assembly-CSharp.csproj'];
for (const p of projects) {
  const src = path.join(main, p);
  if (!fs.existsSync(src)) continue;
  let text = fs.readFileSync(src, 'utf8');
  text = text.replace(/<HintPath>Library\\/g, `<HintPath>${main}\\Library\\`);
  const dirs = new Set();
  text = text.replace(/^\s*<Compile Include="([^"]+)" \/>\r?\n/gm, (_, f) => {
    dirs.add(path.win32.dirname(f));
    return '';
  });
  const globs = [...dirs].map((d) => `    <Compile Include="${d}\\*.cs" />`).join('\n');
  text = text.replace(/<ItemGroup>/, `<ItemGroup>\n${globs}`);
  fs.writeFileSync(path.join(dest, p), text);
  console.log(`${p}: ${dirs.size} source folder(s)`);
}
