// NIGHT35 V: the SimHarness compiles UnityTuning.Look35.cs (Unity-free consts) beside UnityTuning.Look33.cs, because LightPlan33.cs (compiled there for
// light33 / light35) reads the lights35 numbers from it. Idempotent. Usage: node Tools/night/n35v-csproj.cjs
const fs = require('fs');
const path = require('path');
const f = path.join(__dirname, '..', 'SimHarness', 'SimHarness.csproj');
let s = fs.readFileSync(f, 'utf8');
if (s.includes('UnityTuning.Look35.cs')) { console.log('already there'); process.exit(0); }
const anchor = '    <Compile Include="../../Assets/Scripts/Game/UnityTuning.Look33.cs" />';
if (!s.includes(anchor)) throw new Error('anchor missing');
const eol = s.includes('\r\n') ? '\r\n' : '\n';
s = s.replace(anchor, anchor + eol + '    <!-- NIGHT35 V: lights35\'s numbers (LightPlan33.SkyFixtures / Green with lights35; light35 in Harness.N35.Look.cs) -->' + eol + '    <Compile Include="../../Assets/Scripts/Game/UnityTuning.Look35.cs" />');
fs.writeFileSync(f, s);
console.log('added UnityTuning.Look35.cs');
