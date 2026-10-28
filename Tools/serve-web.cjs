// A small static server for the Unity Web build (NIGHT8 U2-14): serves Builds/Web on http://127.0.0.1:<port>/ with the
// MIME types Unity's loader needs. The build is gzip-compressed with the decompression fallback, so the .gz files are
// sent as they are (Content-Type of the inner file, no Content-Encoding) and the loader decompresses them in JS.
// usage: node Tools/serve-web.cjs [port=8090] [dir=Builds/Web]
const http = require('http');
const fs = require('fs');
const path = require('path');

const port = Number(process.argv[2] || 8090);
const root = path.resolve(process.argv[3] || path.join(__dirname, '..', 'Builds', 'Web'));
const types = {
  '.html': 'text/html; charset=utf-8', '.js': 'application/javascript', '.wasm': 'application/wasm', '.json': 'application/json',
  '.data': 'application/octet-stream', '.css': 'text/css', '.png': 'image/png', '.ico': 'image/x-icon', '.svg': 'image/svg+xml',
};

http.createServer((req, res) => {
  const url = decodeURIComponent((req.url || '/').split('?')[0]);
  let file = path.join(root, url === '/' ? 'index.html' : url);
  if (!file.startsWith(root)) {
    res.writeHead(403);
    res.end();
    return;
  }
  fs.stat(file, (err, st) => {
    if (err || !st.isFile()) {
      res.writeHead(404);
      res.end('not found');
      return;
    }
    const inner = file.endsWith('.gz') ? file.slice(0, -3) : file;
    res.writeHead(200, { 'Content-Type': types[path.extname(inner)] || 'application/octet-stream', 'Content-Length': st.size, 'Cache-Control': 'no-store' });
    fs.createReadStream(file).pipe(res);
  });
}).listen(port, () => console.log(`serving ${root} on http://127.0.0.1:${port}/ (the Playwright browser reaches 127.0.0.1, not localhost)`));
