from pathlib import Path
import shutil
root=Path(__file__).resolve().parents[2];out=root/'Validation/ManorCourtyardV2';qa=root.parent/'Validation/DesertV2/UnitySmokeProject'
p=out/'tests.txt';s=p.read_text(encoding='utf-8-sig').replace('ManorGateLocked",9,3','ManorGateLocked",9,2');p.write_text(s,encoding='utf-8')
# Retain obsolete native harnesses outside Assets; they are fixtures, never player project files.
for src in (qa/'Assets/NakiPrototype/Editor').glob('*Smoke*.cs'):
 if src.name=='ManorCourtyardV2Smoke.cs':continue
 assert src.resolve().is_relative_to(qa.resolve())
 dst=out/'archived-qa-harnesses'/src.name;dst.parent.mkdir(exist_ok=True)
 if dst.exists():assert dst.read_bytes()==src.read_bytes()
 else:shutil.copy2(src,dst)
 src.rename(Path(str(src)+'.disabled'))
print('Kept old harness backups; isolated QA compiles only the current self-contained runner')
