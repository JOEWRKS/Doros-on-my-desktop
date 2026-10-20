'use strict';
const http=require('node:http'),fs=require('node:fs'),path=require('node:path');
const {dir}=require('./grab-release.cjs');
const server=http.createServer((req,res)=>{
 if(req.method!=='GET'){res.writeHead(405);return res.end();}
 let name;try{name=decodeURIComponent(new URL(req.url,'http://localhost').pathname).slice(1)||'index.html';}catch{res.writeHead(400);return res.end();}
 if(name.includes('/')||name.includes('\\')||!(/^[a-zA-Z0-9.-]+\.(html|png)$/.test(name))){res.writeHead(404);return res.end();}
 fs.readFile(path.join(dir,name),(err,data)=>{if(err){res.writeHead(404);return res.end();}res.writeHead(200,{'Content-Type':name.endsWith('.html')?'text/html; charset=utf-8':'image/png','Cache-Control':'no-store','X-Content-Type-Options':'nosniff'});res.end(data);});
});server.listen(0,'127.0.0.1',()=>console.log(`http://127.0.0.1:${server.address().port}/`));
