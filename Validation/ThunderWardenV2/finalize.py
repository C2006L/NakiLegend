from pathlib import Path
import shutil,json,hashlib
root=Path(__file__).resolve().parents[2];qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
rels=['Assets/NakiPrototype/Editor/NakiManorBlueprint.cs','Assets/NakiPrototype/Resources/NakiArt/ManorV1']
rels += ['Assets/NakiPrototype/Resources/NakiArt/ManorV1/'+name for name in ['blueprint.json','opening.png','enemies-concept.png','bosses-concept.png']]
for rel in rels:
 src=qa/(rel+'.meta');dst=root/(rel+'.meta');assert src.exists(),src
 if dst.exists():assert src.read_bytes()==dst.read_bytes(),rel
 else:shutil.copy2(src,dst)
manifest=[]
for name in ['opening.png','enemies-concept.png','bosses-concept.png']:
 p=root/'Assets/NakiPrototype/Resources/NakiArt/ManorV1'/name
 manifest.append(dict(file=str(p.relative_to(root)),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),source='Original image_gen output',purpose='opening CG' if name=='opening.png' else 'concept sheet, not runtime animation'))
out=root/'Art/ManorV1';out.mkdir(parents=True,exist_ok=True);(out/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print('Preserved six imported GUIDs and recorded three original image assets')
