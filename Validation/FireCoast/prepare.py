from pathlib import Path
import shutil

root=Path(__file__).resolve().parents[2]
generated=Path('C:/Users/15853/.codex/generated_images/01a1764d-2b16-7840-a400-e14f3ffe5ffd')
art=root/'Assets/NakiPrototype/Resources/NakiArt/FireCoast'
art.mkdir(parents=True,exist_ok=True)
for name,source in [('creatures','exec-3d6060c4-9664-4d8a-a7c9-5ddc81f94874.png'),('harbor','exec-f1c22a9d-e92c-45bd-9c59-05ae0e452385.png'),('environment','exec-021668e8-cff3-48a4-b333-42dabba3ccb2.png')]:
    shutil.copy2(generated/source,art/(name+'.png'))

path=root/'Assets/NakiPrototype/NakiPrototype.FireCoast.cs'
s=path.read_text(encoding='utf-8')
s=s.replace('var rect=new Rect(index%2*cw,(1-index/2)*ch,cw,ch);s=Sprite.Create', 'var rect=index==0?new Rect(t.width*44f/1254,t.height*(1254-590f)/1254,t.width*550f/1254,t.height*550f/1254):new Rect(index%2*cw,(1-index/2)*ch,cw,ch);s=Sprite.Create')
s=s.replace('ch/(index==0?4:index==1?1.2f:1.6f)', 'rect.height/(index==0?4:index==1?1.2f:1.6f)')
old='for(int i=0;i<5;i++){var s=Sprite.Create(t,new Rect(i*t.width/5f,0,t.width/5f,t.height),new Vector2(.5f,.1f),t.height/(i==4?3:i==2?1.3f:1.6f),0,SpriteMeshType.FullRect);'
new='Rect[] regions={new Rect(0,205,330,405),new Rect(330,245,420,365),new Rect(750,230,470,380),new Rect(1220,220,330,390),new Rect(1550,145,622,465)};for(int i=0;i<5;i++){Rect r=regions[i];r=new Rect(r.x*t.width/2172,(724-r.yMax)*t.height/724,r.width*t.width/2172,r.height*t.height/724);var s=Sprite.Create(t,r,new Vector2(.5f,.04f),r.height/(i==4?2.5f:i==2?1.05f:1.25f),0,SpriteMeshType.FullRect);'
assert old in s
s=s.replace(old,new)
path.write_text(s,encoding='utf-8')
path=root/'Assets/NakiPrototype/NakiPrototype.ThunderChapterSave.cs'
s=path.read_text(encoding='utf-8').replace('s.fireBolt&&!s.stone||s.ending', 's.fireBolt&&!s.stone||s.stone&&!s.ranger||s.ending')
path.write_text(s,encoding='utf-8')
