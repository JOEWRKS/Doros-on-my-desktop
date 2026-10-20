'use strict';
const http=require('node:http'),fs=require('node:fs'),path=require('node:path');
const dir=path.resolve(__dirname,'../../artifacts/runtime-grab-20261018-r52');
http.createServer((req,res)=>{
 if(req.method!=='GET'){res.writeHead(405);return res.end();}
 let name;try{name=decodeURIComponent(new URL(req.url,'http://localhost').pathname).slice(1)||'index.html';}catch{res.writeHead(400);return res.end();}
 if(!(name==='index.html'||name==='manifest.json'||/^(?:(?:final-v4|host-final-v4)\/)?[a-zA-Z0-9.-]+\.png$/.test(name))){res.writeHead(404);return res.end();}
 fs.readFile(path.join(dir,name),(err,data)=>{if(err){res.writeHead(404);return res.end();}res.writeHead(200,{'Content-Type':name.endsWith('.html')?'text/html; charset=utf-8':name.endsWith('.json')?'application/json':'image/png','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'});res.end(data);});
}).listen(0,'127.0.0.1',function(){console.log(`http://127.0.0.1:${this.address().port}/`);});
