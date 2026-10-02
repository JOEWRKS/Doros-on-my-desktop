const {test}=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const {createRequire}=require('node:module');
const http=require('node:http'),{Duplex}=require('node:stream');

const root=path.resolve(__dirname,'..');
const previews=[
  'tools/PreviewLocomotion/server.cjs',
  'tools/PreviewLocomotion/serve-pounce.cjs',
  'tools/CheekSpringPreview/server.cjs'
];

function loadPreview(file,readError=false){
  const filename=path.join(root,file),localRequire=createRequire(filename);
  let handler;
  // Historical proof textures need not exist to exercise request handling.
  const assets={readFileSync:()=>Buffer.from('texture'),accessSync:()=>{},
    readFile:(_file,callback)=>callback(readError?Error('asset unavailable'):null,Buffer.from('<!-- preview-images -->fixture'))};
  const listener={listen(_port,host){assert.equal(host,'127.0.0.1');return this;}};
  vm.runInNewContext(fs.readFileSync(filename,'utf8'),{
    require:name=>name==='node:fs'?assets:name==='node:http'?{
      createServer(callback){handler=callback;return listener;}
    }:localRequire(name),
    __dirname:path.dirname(filename),URL,process:{argv:['node',filename]},console:{log(){}}
  },{filename});
  return handler;
}

function request(handler,target){
  // Real Node parser/ServerResponse on an in-memory transport, not a listener.
  return new Promise((resolve,reject)=>{
    let wire='';
    class MemorySocket extends Duplex{
      _read(){}
      _write(chunk,_encoding,callback){wire+=chunk.toString();callback();}
      setTimeout(){return this;} setNoDelay(){return this;} setKeepAlive(){return this;}
    }
    const socket=new MemorySocket();
    const timer=setTimeout(()=>{socket.destroy();reject(Error('Response timeout'));},1000);
    const server=http.createServer((req,res)=>{
      try{handler(req,res);}catch(error){reject(error);socket.destroy();}
    });
    server.on('clientError',error=>{reject(error);socket.destroy();});
    socket.on('error',reject);
    socket.on('finish',()=>{clearTimeout(timer);resolve(wire);socket.destroy();});
    socket.on('close',()=>clearTimeout(timer));
    server.emit('connection',socket);
    socket.push(`GET ${target} HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n`);
  });
}

for(const file of previews){
  for(const target of ['http://[','http://[::1','http://localhost:bad/']){
    test(`${file}: malformed target ${target} returns 400 and a subsequent request works`,async()=>{
      const handler=loadPreview(file);
      assert.match(await request(handler,target),/^HTTP\/1\.1 400 /);
      const valid=await request(handler,'/?frame=1');
      assert.match(valid,/^HTTP\/1\.1 200 /);
      assert.match(valid,/Content-Type: text\/html; charset=utf-8/i);
      assert.match(valid,/Cache-Control: no-store/i);
      assert.match(valid,/fixture/);
    });
  }
  for(const target of ['/missing','/constructor','/__proto__','/../not-allowed']){
    test(`${file}: unlisted path ${target} remains 404`,async()=>{
      assert.match(await request(loadPreview(file),target),/^HTTP\/1\.1 404 /);
    });
  }
  test(`${file}: legitimate absolute HTTP target remains supported`,async()=>{
    assert.match(await request(loadPreview(file),'http://localhost/?frame=1'),/^HTTP\/1\.1 200 /);
  });
  test(`${file}: listed script retains its MIME type`,async()=>{
    const target=file.includes('CheekSpringPreview')?'/motion.mjs':file.includes('serve-pounce')?'/pounce-motion.js':'/rig.js';
    const response=await request(loadPreview(file),target);
    assert.match(response,/^HTTP\/1\.1 200 /);
    assert.match(response,/Content-Type: text\/javascript/i);
  });
  test(`${file}: asset-read failure remains 500, not an invalid-URL response`,async()=>{
    assert.match(await request(loadPreview(file,true),'/'),/^HTTP\/1\.1 500 /);
  });
}
