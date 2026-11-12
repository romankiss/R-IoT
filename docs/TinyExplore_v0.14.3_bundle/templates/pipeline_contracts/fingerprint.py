"""CodeCraft module fingerprint reference. Usage: python fingerprint.py module.json at.json
Implements the supplied pair's declared UTF-8/sorted-key/compact JSON profile.
No file is modified. Requires fingerprint.algorithm SHA-256 and the documented profile.
"""
import copy,hashlib,json,sys
from pathlib import Path
def unique(pairs):
 d={}
 for k,v in pairs:
  if k in d:raise ValueError('Duplicate JSON key: '+k)
  d[k]=v
 return d
def fingerprint(module,at):
 m=copy.deepcopy(module);a=copy.deepcopy(at)
 m['Design']['artifact_identity']['fingerprint']['value']=''
 m['Design']['declaration'].pop('version',None);a['Design']['declaration'].pop('version',None)
 def canonical(v):return json.dumps(v,ensure_ascii=False,sort_keys=True,separators=(',',':'),allow_nan=False).encode('utf-8')
 return hashlib.sha256(canonical(m)+b'\n'+canonical(a)).hexdigest()
if __name__=='__main__':
 if len(sys.argv)!=3:raise SystemExit('Usage: python fingerprint.py module.json at.json')
 m,a=[json.loads(Path(p).read_text(encoding='utf-8'),object_pairs_hook=unique) for p in sys.argv[1:]]
 actual=fingerprint(m,a);expected=m['Design']['artifact_identity']['fingerprint']['value']
 print(json.dumps({'computed':actual,'declared':expected,'match':actual==expected}))
 raise SystemExit(0 if actual==expected else 1)
