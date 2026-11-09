from pathlib import Path
import hashlib,re
root=Path(__file__).resolve().parents[2];base=root/'Assets/NakiPrototype'
def guid(p):return hashlib.md5(('NakiLegend/WeaponIdentityV3/'+str(p.relative_to(root))).encode()).hexdigest()
paths=[base/'NakiPrototype.WeaponIdentity.cs',base/'NakiPrototype.OriginWeapons.cs',base/'Editor/NakiWeaponGallery.cs',base/'Resources/NakiArt/WeaponIdentityV3']+list((base/'Resources/NakiArt/WeaponIdentityV3').iterdir())+list((base/'Resources/NakiCombatAudioV3').glob('weapon_*.wav'))
for p in paths:
 if p.name.endswith('.meta'):continue
 meta=Path(str(p)+'.meta')
 if meta.exists():continue
 if p.suffix=='.png':
  s=(base/'Resources/NakiArt/IslandArmory/ranged-atlas.png.meta').read_text();s=re.sub(r'guid: \w+','guid: '+guid(p),s);s=s.replace('filterMode: 1','filterMode: 0').replace('maxTextureSize: 2048','maxTextureSize: 4096')
 elif p.suffix=='.wav':
  s=(base/'Resources/NakiCombatAudioV3/hammer_0.wav.meta').read_text();s=re.sub(r'guid: \w+','guid: '+guid(p),s);s=s.replace('normalize: 1','normalize: 0')
 elif p.suffix=='.cs':s=f'fileFormatVersion: 2\nguid: {guid(p)}\nMonoImporter:\n  externalObjects: {{}}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {{instanceID: 0}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
 elif p.is_dir():s=f'fileFormatVersion: 2\nguid: {guid(p)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
 else:s=f'fileFormatVersion: 2\nguid: {guid(p)}\nTextScriptImporter:\n  externalObjects: {{}}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n'
 meta.write_text(s,encoding='utf-8')
print('Stable metadata prepared for all weapon resources, audio, scripts and editor gallery')
