"""Targeted semantic review helper, not a full JSON Schema validator.
Usage: python review_event_actions.py event_actions.json selected_level0_ddr.json
Reads the actual selected delivered_shape; no network or files are modified.
"""
import json,sys
from pathlib import Path
def review(actions,level0):
 findings=[]
 shape=level0['Design']['topics']['delivered_shape']
 if shape!='AtGroup + localTopic':
  return [{'blocking':True,'ref':'/Design/topics/delivered_shape','reason':'Unsupported helper profile; review the selected contract explicitly, do not assume a topic format.'}]
 seen=set()
 for i,a in enumerate(actions['actions']):
  prefix='/actions/'+str(i);t=a['trigger'];expected=t['producer']+t['topic']
  if t['delivered_topic']!=expected:findings.append({'blocking':True,'ref':prefix+'/trigger/delivered_topic','observed':t['delivered_topic'],'expected':expected})
  if a['id'] in seen:findings.append({'blocking':True,'ref':prefix+'/id','reason':'Duplicate action ID'})
  seen.add(a['id']);steps=set()
  for j,x in enumerate(a['steps']):
   if x['id'] in steps or (x['when'] is not None and x['when'] not in steps):findings.append({'blocking':True,'ref':prefix+'/steps/'+str(j),'reason':'Duplicate ID or unresolved/forward prerequisite'})
   steps.add(x['id'])
  f=a['finally'];enter=f['enter_after']
  if (f['steps'] and enter not in steps) or (not f['steps'] and enter is not None):findings.append({'blocking':True,'ref':prefix+'/finally','reason':'Cleanup scope must reference a preceding successful main step; no empty entered cleanup'})
  for j,x in enumerate(f['steps']):
   if x['id'] in steps or x['required'] or x['on_failure']!='continue':findings.append({'blocking':True,'ref':prefix+'/finally/steps/'+str(j),'reason':'Cleanup must be unique and best effort'})
   if x['when'] is not None and x['when'] not in steps:findings.append({'blocking':True,'ref':prefix+'/finally/steps/'+str(j)+'/when','reason':'Unresolved prerequisite'})
   steps.add(x['id'])
 return findings
if __name__=='__main__':
 if len(sys.argv)!=3:raise SystemExit('Usage: python review_event_actions.py event_actions.json selected_level0_ddr.json')
 try:
  actions,level0=[json.loads(Path(p).read_text(encoding='utf-8')) for p in sys.argv[1:]]
  findings=review(actions,level0);print(json.dumps({'status':'blocked' if findings else 'passed','scope':'Delivered-topic, action/step references and cleanup checks only; not complete semantic validation or runtime evidence.','findings':findings},indent=2));raise SystemExit(1 if findings else 0)
 except (KeyError,TypeError) as ex:raise SystemExit('Malformed/unsupported input: '+str(ex))
