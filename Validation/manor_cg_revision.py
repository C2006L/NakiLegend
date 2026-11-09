from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');a=r/'Assets/NakiPrototype';folder=a/'Resources/NakiArt/ManorFinaleV3';shutil.copy2('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-7d733d21-e41f-4395-ad8d-ec8e5512f8a6.png',folder/'endings.png')
p=a/'NakiPrototype.ManorFinale.cs';s=p.read_text(encoding='utf-8-sig').replace('拒绝淬炼村民 · 以自身意志终止契约\\n少年牺牲','拒绝淬炼村民 · 不以无辜者换取力量\\n少年最终败北').replace('少年：这里还有一人份。这一次，不必再从别人身上取了。','少年：这里还有一人份。但他们不该为我而死。空甲仍然站立，少年终于倒在来时的石阶上。');p.write_text(s,encoding='utf-8')
f=r/'Validation/manor-finale-files.txt';f.write_text(f.read_text(encoding='utf-8')+'\nAssets/NakiPrototype/Editor/NakiManorScenes.cs',encoding='utf-8')
