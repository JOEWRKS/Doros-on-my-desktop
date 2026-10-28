"""Read-only bounded observation summary. No forced collection or process control."""
from pathlib import Path
import sys,json,statistics as st,datetime as dt
root=Path(sys.argv[1])
def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def stamp(s):return dt.datetime.fromisoformat(s.replace('Z','+00:00'))
def stats(v):return {'min':min(v),'median':st.median(v),'max':max(v)} if v else None
def summarize(folder,probe):
 run=read(folder/'run.json');rows=[json.loads(s) for s in (folder/'samples.jsonl').read_text(encoding='utf-8-sig').splitlines() if s.strip()]
 completion=read(folder/'completion.json') if (folder/'completion.json').exists() else None
 for r in rows:r['elapsed']=r['seconds'] if probe else r['ms']/1000
 times=[r['elapsed'] for r in rows];gaps=[b-a for a,b in zip(times,times[1:])]
 discrepancy=[abs((stamp(b['utc'])-stamp(a['utc'])).total_seconds()-(b['elapsed']-a['elapsed'])) for a,b in zip(rows,rows[1:])]
 expected=run['startTicks'];pid=run['pid']
 windows={}
 for name,lo,hi in [('warm',0,180),('first',180,360),('middle',360,540),('lastRepeat',540,720),('recovery',720,901)]:
  rr=[r for r in rows if lo<=r['elapsed']<hi];measure={}
  for key in ['privateBytes','managedEstimate','heapBytes','committedBytes','fragmentedBytes','gdi','user','handles','threads']:
   vals=[r[key]/(1024*1024) if key.endswith('Bytes') or key=='managedEstimate' else r[key] for r in rr if key in r]
   if vals:measure[key+('MiB' if key.endswith('Bytes') or key=='managedEstimate' else '')]=stats(vals)
  measure['writeCombineMiB']=stats([sum(v['bytes'] for v in r.get('regions',[]))/(1024*1024) for r in rr]);windows[name]={'samples':len(rr),**measure}
 capacities={}
 if probe:
  for key in rows[-1]['capacity']:capacities[key]=stats([r['capacity'][key] for r in rows if r['elapsed']>=180])
 required=['tap','shake','groom','stretch','beg','angry','chuckle','eating','sleep']
 observed=(completion or rows[-1]).get('observed',{})
 region_sets=[{v['allocationBase'] for v in r.get('regions',[])} for r in rows]
 generated=set();released=set()
 for a,b in zip(region_sets,region_sets[1:]):generated|=b-a;released|=a-b
 result={'run':run,'completion':completion,'sampleCount':len(rows),'lastElapsed':times[-1],
 'identityMatches':all(r['pid']==pid and r['startTicks']==expected for r in rows),'maxSampleGapSeconds':max(gaps,default=0),'maxUtcMonotonicDifferenceSeconds':max(discrepancy,default=0),
 'windows':windows,'capacitiesAfterWarmup':capacities,'lastSample':rows[-1],
 'regions':{'firstAllocationBases':len(region_sets[0]),'lastAllocationBases':len(region_sets[-1]),'createdBases':len(generated),'releasedBases':len(released)},
 'missingObservedPaths':[k for k in required if not observed.get(k)] if probe else None,
 'scope':'component replay, not real mouse/full PetLoop' if probe else 'read-only live observation, input unmeasured',
 'status':'IN_PROGRESS' if not completion else ('COMPLETED_REVIEW_REQUIRED' if completion['reason']=='duration_completed' else 'INCOMPLETE')}
 if probe:
  cycleRows={}
  for r in rows:
   if r['phase'] in ['recovery','end']:continue
   cycleRows.setdefault(r['ticks']//1980,[]).append(r)
  result['cycleWindows']=[{'cycle':cycle,'samples':len(rr),'firstSeconds':rr[0]['elapsed'],'lastSeconds':rr[-1]['elapsed'],'privateMiB':stats([r['privateBytes']/1048576 for r in rr]),'managedEstimateMiB':stats([r['managedEstimate']/1048576 for r in rr]),'idlePrivateMiB':stats([r['privateBytes']/1048576 for r in rr if r['phase']=='idle']),'heapAtNaturalGcMiB':stats([r['heapBytes']/1048576 for r in rr])} for cycle,rr in cycleRows.items()]
 return result
result={name:summarize(root/name,name=='actual') for name in ['actual','live']}
(root/'summary.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
for name,r in result.items():
 print(name,json.dumps({k:r[k] for k in ['status','sampleCount','lastElapsed','identityMatches','maxSampleGapSeconds','maxUtcMonotonicDifferenceSeconds','missingObservedPaths']}))
 for key,window in r['windows'].items():
  if window['samples']:print(key,json.dumps({k:window[k] for k in ['samples','privateBytesMiB','writeCombineMiB','gdi','handles'] if k in window}))
