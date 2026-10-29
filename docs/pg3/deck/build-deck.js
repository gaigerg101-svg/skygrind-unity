// PG3 Demo Day deck (GG 2026-10-28: "a detailed 5-7 slide presentation about this game, the process of building it ... Human First, AI in the Middle,
// and Human Last ... process, screenshots, inspiration ... make it powerpoint"). Every number is counted from the repo on 2026-10-28; every screenshot is a
// frame from the muted Player reels (Logs/feel/...) or an Editor render, copied by make-images.py.
// Usage: NODE_PATH=<dir with pptxgenjs, react, react-dom, react-icons, sharp> node docs/pg3/deck/build-deck.js <apply_theme.js path>
'use strict';
const path = require('path');
const pptxgen = require('pptxgenjs');
const React = require('react');
const ReactDOMServer = require('react-dom/server');
const sharp = require('sharp');
const fa = require('react-icons/fa');
const { applyTheme } = require(process.argv[2]);

const IMG = path.join(__dirname, 'img');
const OUT = path.join(__dirname, '..', 'SKYGRIND-PG3-DemoDay.pptx');

const THEME = {
  name: 'SKYGRIND Dusk',
  headFontFace: 'Arial',
  bodyFontFace: 'Calibri',
  colors: {
    dk1: 'F3F0FF', lt1: '120E22', dk2: 'B8AFD8', lt2: '1E1834',
    accent1: '19D3E6', accent2: 'FF5A2C', accent3: 'C9BCFF', accent4: 'FFB347', accent5: '2C2448', accent6: 'FF2E88',
    hlink: '19D3E6', folHlink: 'C9BCFF',
  },
};
const HEX = THEME.colors;

async function icon(Comp, hex, size = 256) {
  const svg = ReactDOMServer.renderToStaticMarkup(React.createElement(Comp, { color: '#' + hex, size }));
  const png = await sharp(Buffer.from(svg)).png().toBuffer();
  return 'image/png;base64,' + png.toString('base64');
}

(async () => {
  const pres = new pptxgen();
  pres.layout = 'LAYOUT_WIDE'; // 13.333 x 7.5 in
  pres.author = 'Gaige Geraci';
  pres.title = 'SKYGRIND: Human First, AI in the Middle, Human Last';
  pres.theme = { headFontFace: THEME.headFontFace, bodyFontFace: THEME.bodyFontFace };
  const C = pres.SchemeColor;

  pres.defineSlideMaster({
    title: 'SKY_TITLE',
    background: { color: C.background1 },
    objects: [
      { placeholder: { options: { name: 'title', type: 'title', x: 0.7, y: 1.55, w: 7.0, h: 1.4, fontSize: 64, bold: true, color: C.text1, align: 'left', valign: 'bottom', margin: 0 }, text: '' } },
      { placeholder: { options: { name: 'body', type: 'body', x: 0.7, y: 3.05, w: 6.6, h: 1.3, fontSize: 22, color: C.text2, valign: 'top', margin: 0 }, text: '' } },
    ],
  });
  pres.defineSlideMaster({
    title: 'SKY_CONTENT',
    background: { color: C.background1 },
    margin: [0.5, 0.6, 0.6, 0.6],
    objects: [
      { placeholder: { options: { name: 'title', type: 'title', x: 0.6, y: 0.35, w: 12.1, h: 0.85, fontSize: 32, bold: true, color: C.text1, valign: 'middle', margin: 0 }, text: '' } },
      { text: { text: 'SKYGRIND  ·  ASU AME 294  ·  Portfolio Game 3', options: { x: 0.6, y: 7.0, w: 6.0, h: 0.3, fontSize: 10, color: C.text2, margin: 0 } } },
    ],
    slideNumber: { x: 12.1, y: 7.0, w: 0.6, h: 0.3, fontSize: 10, color: HEX.dk2, align: 'right' },
  });

  const I = {
    user: await icon(fa.FaUser, HEX.lt1), robot: await icon(fa.FaRobot, HEX.lt1), flag: await icon(fa.FaFlagCheckered, HEX.lt1),
    run: await icon(fa.FaRunning, HEX.lt1), skull: await icon(fa.FaSkull, HEX.lt1), palette: await icon(fa.FaPalette, HEX.lt1),
    quote: await icon(fa.FaQuoteLeft, HEX.accent1), arrow: await icon(fa.FaArrowRight, HEX.accent3),
    code: await icon(fa.FaCodeBranch, HEX.accent1), vial: await icon(fa.FaVial, HEX.accent1), lines: await icon(fa.FaCode, HEX.accent1),
    sfx: await icon(fa.FaVolumeUp, HEX.accent2), music: await icon(fa.FaMusic, HEX.accent2), cube: await icon(fa.FaCube, HEX.accent2),
    term: await icon(fa.FaTerminal, HEX.lt1), img: await icon(fa.FaImage, HEX.accent2), globe: await icon(fa.FaGlobe, HEX.accent2),
    cal: await icon(fa.FaCalendarCheck, HEX.lt1), board: await icon(fa.FaChalkboardTeacher, HEX.lt1), pad: await icon(fa.FaGamepad, HEX.lt1),
  };
  const circle = (s, x, y, d, fill, ic, name) => {
    s.addShape(pres.shapes.OVAL, { x, y, w: d, h: d, fill: { color: fill }, line: { color: fill }, objectName: name + ' circle' });
    s.addImage({ data: ic, x: x + d * 0.24, y: y + d * 0.24, w: d * 0.52, h: d * 0.52, objectName: name + ' icon' });
  };
  const chip = (s, x, y, w, text, fill, name) => {
    s.addText(text, { x, y, w, h: 0.5, shape: pres.shapes.ROUNDED_RECTANGLE, rectRadius: 0.25, fill: { color: fill }, color: C.background1, bold: true, fontSize: 14, align: 'center', valign: 'middle', margin: 0, isTextBox: true, objectName: name });
  };

  // 1. Title ------------------------------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'SKYGRIND' });
  let s = pres.addSlide({ masterName: 'SKY_TITLE', sectionTitle: 'SKYGRIND' });
  s.addImage({ path: path.join(IMG, 'hero.jpg'), x: 0, y: 0, w: 13.333, h: 7.5, objectName: 'hero screenshot' });
  s.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 8.6, h: 7.5, fill: { color: C.background1, transparency: 12 }, line: { color: C.background1, transparency: 100 }, objectName: 'title scrim' });
  s.addText('SKYGRIND', { placeholder: 'title' });
  s.addText('A first-person movement shooter at dusk, built Human First, AI in the Middle, Human Last', { placeholder: 'body' });
  chip(s, 0.7, 4.75, 2.05, 'HUMAN FIRST', C.accent1, 'chip human first');
  s.addImage({ data: I.arrow, x: 2.88, y: 4.87, w: 0.26, h: 0.26, objectName: 'arrow 1' });
  chip(s, 3.27, 4.75, 2.45, 'AI IN THE MIDDLE', C.accent2, 'chip ai');
  s.addImage({ data: I.arrow, x: 5.85, y: 4.87, w: 0.26, h: 0.26, objectName: 'arrow 2' });
  chip(s, 6.24, 4.75, 1.85, 'HUMAN LAST', C.accent1, 'chip human last');
  s.addText('Gaige Geraci  ·  ASU AME 294, Portfolio Game 3  ·  Demo Day, 28 October 2026', { x: 0.7, y: 6.4, w: 7.0, h: 0.4, fontSize: 14, color: C.text2, margin: 0, isTextBox: true, objectName: 'byline' });
  s.addNotes('SKYGRIND is a first-person movement shooter at dusk: Call of Duty Zombies rounds with ULTRAKILL movement. This talk is about the game and about how it was made: I lead (human first), AI tools do most of the building and generating in the middle, and I come back at the end to play, judge and decide (human last). The screenshot is a real frame from the game: a husk enemy model and the KESTREL revolver.');

  // 2. Inspiration ------------------------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'Inspiration' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'Inspiration' });
  s.addText('Inspiration: Zombies rounds × ULTRAKILL movement', { placeholder: 'title' });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.6, y: 1.45, w: 6.3, h: 1.55, rectRadius: 0.12, fill: { color: C.background2 }, line: { color: C.background2 }, objectName: 'quote card' });
  s.addImage({ data: I.quote, x: 0.85, y: 1.7, w: 0.4, h: 0.4, objectName: 'quote mark' });
  s.addText([
    { text: '"cod zombies meets ultrakill as the main, but other games should be inspired"', options: { italic: true, fontSize: 18, color: C.text1, breakLine: true } },
    { text: 'My one-line brief, 3 October 2026', options: { fontSize: 13, color: C.text2 } },
  ], { x: 1.45, y: 1.6, w: 5.25, h: 1.25, valign: 'middle', margin: 0, isTextBox: true, objectName: 'quote text' });
  const pillars = [
    ['run', C.accent1, 'Movement', 'Slide, dash, double jump, wall-ride and grapple, on a 120 Hz sim ported line for line from my web prototype'],
    ['skull', C.accent2, 'Rounds', 'Waves of husks, swarmers, spitters, chargers, wardens and drones; points, buys, and a boss at the end of every level'],
    ['palette', C.accent3, 'Dusk', 'One colour language: cyan is yours, red-orange is danger, magenta is the rival, violet-white is the edge of the world'],
  ];
  pillars.forEach((p, i) => {
    const y = 3.3 + i * 1.18;
    circle(s, 0.6, y, 0.72, p[1], I[p[0]], p[2]);
    s.addText([
      { text: p[2], options: { bold: true, fontSize: 18, color: C.text1, breakLine: true } },
      { text: p[3], options: { fontSize: 14, color: C.text2 } },
    ], { x: 1.55, y: y - 0.08, w: 5.35, h: 1.05, valign: 'top', margin: 0, isTextBox: true, objectName: p[2] + ' text' });
  });
  s.addImage({ path: path.join(IMG, 'horde.jpg'), x: 7.3, y: 1.45, w: 5.4, h: 3.04, objectName: 'horde screenshot' });
  const swatches = [['19D3E6', 'Cyan: yours'], ['FF4A1C', 'Red-orange: danger'], ['FF2E88', 'Magenta: rival'], ['C9BCFF', 'Violet-white: edge']];
  swatches.forEach((w, i) => {
    const x = 7.3 + i * 1.383;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 4.8, w: 1.25, h: 0.75, rectRadius: 0.08, fill: { color: w[0] }, line: { color: w[0] }, objectName: 'swatch ' + w[1] });
    s.addText(w[1], { x, y: 5.62, w: 1.25, h: 0.6, fontSize: 12, color: C.text2, align: 'left', valign: 'top', margin: 0, isTextBox: true, objectName: 'swatch label ' + i });
  });
  s.addNotes('The brief was one line: Zombies meets ULTRAKILL. That gave three pillars. Movement: a fast ULTRAKILL-style kit (slide, dash, double jump, wall-ride, grapple), first built as a web prototype and then ported to Unity line for line so it plays exactly the same. Rounds: a Zombies-style loop of waves, points and buys, with a boss closing each level. Dusk: one strict colour language so the player can read a fight at a glance: cyan is anything you can use, red-orange is danger, magenta is the rival, violet-white is the boundary. The screenshot shows the husk models in the kiln level.');

  // 3. The process --------------------------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'Process' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'Process' });
  s.addText('Human First → AI in the Middle → Human Last', { placeholder: 'title' });
  const stages = [
    ['user', C.accent1, '1  ·  HUMAN FIRST', 'I set the vision and the rules', [
      'The brief, the pillars and the palette',
      'A written prompt for every build night',
      'Taste in my own words: "too basic ... more powerful", "jump height needs to be lower"',
      'Hard rules: movement stays exact; nothing the player had is lost',
    ]],
    ['robot', C.accent2, '2  ·  AI IN THE MIDDLE', 'AI builds and checks it', [
      'Claude Code ported the web game to Unity and built it night by night',
      'Every change checked by tests, play bots and blind A/B frame pairs',
      'ElevenLabs: sound effects and breakcore music',
      'ChatGPT reference images → Tripo 3D models',
    ]],
    ['flag', C.accent1, '3  ·  HUMAN LAST', 'I play it and decide', [
      'A play card for each build: keep or drop every change',
      'I judge the feel; numbers never replace playing it',
      'I approve every push, release and public page',
      'And I present it, today',
    ]],
  ];
  stages.forEach((st, i) => {
    const x = 0.6 + i * 4.18;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 1.45, w: 3.75, h: 5.3, rectRadius: 0.12, fill: { color: C.background2 }, line: { color: st[1], width: 1.25 }, objectName: 'stage card ' + (i + 1) });
    circle(s, x + 0.3, 1.75, 0.75, st[1], I[st[0]], 'stage ' + (i + 1));
    s.addText([
      { text: st[2], options: { bold: true, fontSize: 13, color: st[1], charSpacing: 1, breakLine: true } },
      { text: st[3], options: { bold: true, fontSize: 19, color: C.text1 } },
    ], { x: x + 0.3, y: 2.65, w: 3.2, h: 1.05, valign: 'top', margin: 0, isTextBox: true, objectName: 'stage head ' + (i + 1) });
    s.addText(st[4].map((b, k) => ({ text: b, options: { bullet: true, fontSize: 14, color: C.text2, paraSpaceAfter: 6, breakLine: k < st[4].length - 1 } })),
      { x: x + 0.3, y: 3.75, w: 3.2, h: 2.85, valign: 'top', margin: 0, isTextBox: true, objectName: 'stage bullets ' + (i + 1) });
    if (i < 2) s.addImage({ data: I.arrow, x: x + 3.85, y: 3.95, w: 0.24, h: 0.24, objectName: 'stage arrow ' + (i + 1) });
  });
  s.addNotes('This is the whole method. Human first: I decide what the game is, write the prompt for each build, and say what feels wrong in plain words (two real examples are on the slide). I also set rules the AI may not break, like keeping the movement exact. AI in the middle: Claude Code does most of the engineering, and every change is checked by automated tests, play bots and blind A/B comparisons of frames before it ships. ElevenLabs makes the sound and music; ChatGPT makes reference images that Tripo turns into 3D models. Human last: I play every build and decide what stays, and nothing goes public without my approval.');

  // 3b. The tools (GG: "add in presentation what tools used, especially claude code and unity cli, and why i chose them, concise") ----------
  pres.addSection({ title: 'Tools' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'Tools' });
  s.addText('The tools, and why I chose them', { placeholder: 'title' });
  const big = [
    ['robot', C.accent2, 'Claude Code', 'The AI engineer. It works across the whole project from the terminal, runs builds and tests itself, and follows my written rules (CLAUDE.md) through long build nights.'],
    ['term', C.accent1, 'Unity 6 + Unity CLI', 'Unity for a Steam / Switch game. The Unity CLI lets Claude drive my open Editor: compile, run tests, build players, run checks, with no clicking from me.'],
  ];
  big.forEach((b, i) => {
    const x = 0.6 + i * 6.15;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 1.45, w: 5.95, h: 2.4, rectRadius: 0.12, fill: { color: C.background2 }, line: { color: b[1], width: 1.5 }, objectName: 'tool card ' + b[2] });
    circle(s, x + 0.3, 1.75, 0.8, b[1], I[b[0]], b[2]);
    s.addText([
      { text: b[2], options: { bold: true, fontSize: 22, color: C.text1, breakLine: true } },
      { text: b[3], options: { fontSize: 15, color: C.text2 } },
    ], { x: x + 1.35, y: 1.7, w: 4.35, h: 2.0, valign: 'top', margin: 0, isTextBox: true, objectName: 'tool text ' + b[2] });
  });
  const small = [
    ['music', 'ElevenLabs', 'Sound effects and breakcore music from text, fast and in one style'],
    ['img', 'ChatGPT images', 'Reference art, so I set the look before anything is 3D'],
    ['cube', 'Tripo', 'Image or text to a textured 3D model, exported straight to Unity'],
    ['globe', 'GitHub · itch.io', 'Public code and builds; Claude in Chrome and butler did the uploads'],
  ];
  small.forEach((t, i) => {
    const x = 0.6 + i * 3.075;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 4.15, w: 2.9, h: 2.55, rectRadius: 0.12, fill: { color: C.background2 }, line: { color: C.background2 }, objectName: 'tool card ' + t[1] });
    s.addImage({ data: I[t[0]], x: x + 0.25, y: 4.4, w: 0.45, h: 0.45, objectName: 'tool icon ' + t[1] });
    s.addText([
      { text: t[1], options: { bold: true, fontSize: 17, color: C.text1, breakLine: true } },
      { text: t[2], options: { fontSize: 14, color: C.text2 } },
    ], { x: x + 0.25, y: 5.0, w: 2.45, h: 1.6, valign: 'top', margin: 0, isTextBox: true, objectName: 'tool text ' + t[1] });
  });
  s.addNotes('Two tools carry the method. Claude Code is the engineer in the middle: it reads and edits the whole project from the terminal, builds and tests the game itself, and follows a rules file I wrote, so I can hand it a long build night and trust the guard rails. Unity is the engine because the target is Steam and hopefully Switch, and the Unity CLI is what made the loop fast: Claude drives my open Unity Editor directly, compiling, running tests, building the game and running checks, without me clicking through menus. Around them: ElevenLabs for all the audio, ChatGPT for reference images, Tripo for 3D models, and GitHub plus itch.io to publish; Claude in Chrome and the butler tool did the uploads.');

  // 4. By the numbers -----------------------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'Numbers' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'Numbers' });
  s.addText('Four weeks, by the numbers', { placeholder: 'title' });
  const stats = [
    ['4,298', 'commits since 30 September', 'code', C.accent1], ['2,551', 'automated tests', 'vial', C.accent1], ['317k', 'lines of C#', 'lines', C.accent1],
    ['138', 'ElevenLabs sound effects', 'sfx', C.accent2], ['41', 'ElevenLabs breakcore tracks', 'music', C.accent2], ['19', 'Tripo 3D models', 'cube', C.accent2],
  ];
  stats.forEach((st, i) => {
    const x = 0.6 + (i % 3) * 4.18, y = 1.45 + Math.floor(i / 3) * 2.6;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y, w: 3.75, h: 2.3, rectRadius: 0.12, fill: { color: C.background2 }, line: { color: C.background2 }, objectName: 'stat card ' + i });
    s.addImage({ data: I[st[2]], x: x + 3.05, y: y + 0.3, w: 0.42, h: 0.42, objectName: 'stat icon ' + i });
    s.addText(st[0], { x: x + 0.3, y: y + 0.3, w: 2.7, h: 1.1, fontSize: 56, bold: true, color: st[3], valign: 'middle', margin: 0, isTextBox: true, objectName: 'stat number ' + i });
    s.addText(st[1], { x: x + 0.3, y: y + 1.5, w: 3.2, h: 0.55, fontSize: 16, color: C.text1, valign: 'top', margin: 0, isTextBox: true, objectName: 'stat label ' + i });
  });
  s.addText('Cyan: engineering by Claude Code. Orange: generated assets. Counted from the repository on 28 October 2026.', { x: 0.6, y: 6.55, w: 12.1, h: 0.35, fontSize: 12, italic: true, color: C.text2, margin: 0, isTextBox: true, objectName: 'stats caption' });
  s.addNotes('Real numbers, counted from the repository today. 4,298 commits in about four weeks; 2,551 automated tests that guard the movement and the rules; about 317 thousand lines of C#. On the asset side: 138 sound effects and 41 music tracks from ElevenLabs, and 19 3D models from Tripo. The point is not the size: it is that a human-led loop can steer this much AI output and still keep the game feeling like one thing.');

  // 5. Reference -> model -> game -----------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'Assets' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'Assets' });
  s.addText('AI in the middle, one asset at a time', { placeholder: 'title' });
  const chain = [['ref-husk.png', '1. ChatGPT reference image'], ['tripo-husk.png', '2. Tripo 3D model'], ['game-husk.jpg', '3. In the game, cut into limbs that walk']];
  chain.forEach((c, i) => {
    const x = 0.6 + i * 4.18;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 1.45, w: 3.75, h: 3.3, rectRadius: 0.12, fill: { color: i === 0 ? 'D8D6E0' : C.background2 }, line: { color: C.background2 }, objectName: 'asset frame ' + i });
    s.addImage({ path: path.join(IMG, c[0]), x: x + 0.38, y: 1.48, w: 3.0, h: 3.0, objectName: 'asset image ' + i });
    s.addText(c[1], { x, y: 4.85, w: 3.75, h: 0.5, fontSize: 16, bold: true, color: C.text1, valign: 'top', margin: 0, isTextBox: true, objectName: 'asset caption ' + i });
    if (i < 2) s.addImage({ data: I.arrow, x: x + 3.85, y: 2.98, w: 0.24, h: 0.24, objectName: 'asset arrow ' + i });
  });
  const roles = [
    [C.accent1, 'Human', 'I wrote the prompt, chose the image, and judged the model in the game. When a gun came in backwards or grey, I called it and it was fixed.'],
    [C.accent2, 'AI', 'Generated the image, the mesh and the textures, then cut the model into legs and arms and wrote the walk, wind-up and strike motion.'],
  ];
  roles.forEach((r, i) => {
    const x = 0.6 + i * 6.25;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 5.45, w: 5.85, h: 1.35, rectRadius: 0.12, fill: { color: C.background2 }, line: { color: r[0], width: 1.25 }, objectName: 'role card ' + r[1] });
    s.addText([
      { text: r[1], options: { bold: true, fontSize: 16, color: r[0], breakLine: true } },
      { text: r[2], options: { fontSize: 14, color: C.text2 } },
    ], { x: x + 0.25, y: 5.55, w: 5.4, h: 1.15, valign: 'top', margin: 0, isTextBox: true, objectName: 'role text ' + r[1] });
  });
  s.addNotes('Here is the method on a single asset, the husk enemy. I wrote a prompt and picked the ChatGPT reference image. Tripo turned the image into a textured 3D model. Then Claude cut the static model into body, legs and arms and wrote procedural animation, so it walks, rears back in a wind-up and lunges on the strike. The human part matters at both ends: the first exports had a gun pointing backwards and grey, desaturated textures, and I called both out before they were fixed.');

  // 6. Screenshots ----------------------------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'The game' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'The game' });
  s.addText('The game today', { placeholder: 'title' });
  const gallery = [
    ['icebreaker.jpg', 'THE ICEBREAKER: its model rides its own punch moves'],
    ['crown.jpg', 'THE CROWN spins above its weak-point gems'],
    ['charge.jpg', 'The KESTREL charge fills the gun\'s own cyan glow'],
    ['longwave.jpg', 'The LONGWAVE rail rifle, from a text prompt'],
    ['punch.jpg', 'The gauntlet punch against a husk'],
    ['thunderclap.jpg', 'THE THUNDERCLAP against the KILN WYRM'],
  ];
  gallery.forEach((g, i) => {
    const x = 0.6 + (i % 3) * 4.18, y = 1.4 + Math.floor(i / 3) * 2.78;
    s.addImage({ path: path.join(IMG, g[0]), x, y, w: 3.75, h: 2.11, objectName: 'shot ' + g[0] });
    s.addText(g[1], { x, y: y + 2.16, w: 3.75, h: 0.48, fontSize: 12, color: C.text2, valign: 'top', margin: 0, isTextBox: true, objectName: 'shot caption ' + i });
  });
  s.addNotes('Every image here is a frame from the real game, captured by the automated check runs (sound muted). Top row: THE ICEBREAKER boss, whose 3D model is attached to its own animation bones so its punches still read; THE CROWN, a spinning boss whose gems are the weak points; and the KESTREL charge, which now fills the cyan glow on the gun itself. Bottom row: the LONGWAVE, made in Tripo from a text prompt; the gauntlet punch; and the THUNDERCLAP shotgun during the KILN WYRM fight.');

  // 7. Just showing up ---------------------------------------------------------------------------------------------------------------------
  pres.addSection({ title: 'Showing up' });
  s = pres.addSlide({ masterName: 'SKY_CONTENT', sectionTitle: 'Showing up' });
  s.addText('Just showing up: build, play, present', { placeholder: 'title' });
  const rows = [
    ['cal', C.accent1, 'Every day', 'Show up, write the prompt, play the build, say what is wrong. The 67 logged build nights came out of that loop.'],
    ['board', C.accent2, 'This deck', 'Made with AI too: I asked for the story; Claude pulled the real numbers and screenshots from the project and built this PowerPoint; I check it and present it.'],
    ['pad', C.accent3, 'Play it', ['itch.io: gaiger101.itch.io/skygrind', 'GitHub: github.com/gaigerg101-svg/skygrind-unity']],
  ];
  rows.forEach((r, i) => {
    const y = 1.5 + i * 1.6;
    circle(s, 0.6, y, 0.8, r[1], I[r[0]], r[2]);
    const lines = Array.isArray(r[3]) ? r[3] : [r[3]];
    s.addText([
      { text: r[2], options: { bold: true, fontSize: 20, color: C.text1, breakLine: true } },
      ...lines.map((t, k) => ({ text: t, options: { fontSize: 15, color: C.text2, breakLine: k < lines.length - 1 } })),
    ], { x: 1.65, y: y - 0.05, w: 5.45, h: 1.4, valign: 'top', margin: 0, isTextBox: true, objectName: r[2] + ' row' });
  });
  s.addImage({ path: path.join(IMG, 'punch.jpg'), x: 7.4, y: 1.5, w: 5.3, h: 2.98, objectName: 'closing screenshot' });
  chip(s, 7.4, 4.85, 1.65, 'HUMAN FIRST', C.accent1, 'closing chip 1');
  chip(s, 9.2, 4.85, 2.0, 'AI IN THE MIDDLE', C.accent2, 'closing chip 2');
  chip(s, 11.35, 4.85, 1.35, 'HUMAN LAST', C.accent1, 'closing chip 3');
  s.addText('The AI does the volume. The human decides what it is for, and whether it is good.', { x: 7.4, y: 5.6, w: 5.3, h: 0.9, fontSize: 16, italic: true, color: C.text1, valign: 'top', margin: 0, isTextBox: true, objectName: 'closing line' });
  s.addNotes('The habit that made this work was simply showing up: every day I wrote what I wanted, played what came back, and said what was wrong. This presentation follows the same pattern: I asked for the story and the structure, Claude gathered the real numbers and screenshots and built the PowerPoint, and I am the last step: checking it and presenting it. You can play the game on itch.io, and the code and builds are on GitHub.');

  await pres.writeFile({ fileName: OUT });
  await applyTheme(OUT, THEME);
  console.log('wrote ' + OUT);
})().catch(e => { console.error(e); process.exit(1); });
