from pathlib import Path
import shutil
r=Path('D:/NakiLegend/Repository');shutil.copy2('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd/exec-136f6ed5-1f05-480d-80b3-0a07c7d861ef.png',r/'Assets/NakiPrototype/Resources/NakiArt/ManorFinaleV3/endings.png')
p=r/'Validation/prepare_manor_full.py';s=p.read_text(encoding='utf-8-sig').replace('Call("ManorAction",722);Quiet();','Call("ManorAction",722);').replace("p.with_suffix('.cs.v11-start-disabled')","p.with_suffix('.cs.v11-light-disabled')");p.write_text(s,encoding='utf-8')
