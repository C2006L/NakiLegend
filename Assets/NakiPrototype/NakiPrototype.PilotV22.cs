using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private Sprite clearFastBullet,clearSlowBullet;
  private void SpawnHeavyMother(Enemy owner){
   Vector2 aim=(IntentLead(.3f)-owner.position).normalized;int before=shots.Count;PressureFan(owner.position+aim*(owner.radius+.3f),aim,1,0,2.6f,owner.attackElement,1.5f);
   if(shots.Count==before)return;Shot s=shots[shots.Count-1];s.emitter=owner;s.heavyParent=true;s.steeringUntil=clock+.55f;s.life=4.8f;
  }
  private void SplitHeavyMother(Shot s){
   if(!s.heavyParent||s.emitter==null||s.emitter.hp<=0||!enemies.Contains(s.emitter))return;s.heavyParent=false;
   Vector2 at=s.position-s.direction*.28f;Enemy previous=attackEmitter;attackEmitter=s.emitter;
   try{for(int i=0;i<4;i++){Vector2 ray=Quaternion.Euler(0,0,45+i*90)*Vector2.right;PressureFan(at,ray,1,0,3.1f,s.element,.8f);}}finally{attackEmitter=previous;}
  }
  private Sprite ReadableEnemyBullet(bool fast){
   Sprite cached=fast?clearFastBullet:clearSlowBullet;if(cached!=null)return cached;
   int w=fast?24:24,h=fast?16:24;var t=new Texture2D(w,h,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Point;
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){
    float px=(x+.5f)/w,py=(y+.5f)/h;Color c=Color.clear;
    if(fast){float edge=(1-px)*.46f;if(Mathf.Abs(py-.5f)<edge&&px>.12f)c=C("4c1425");if(Mathf.Abs(py-.5f)<edge-.07f&&px>.22f)c=C("ff5369");if(Mathf.Abs(py-.5f)<edge-.16f&&px>.32f)c=C("fff1d4");}
    else{float radius=Vector2.Distance(new Vector2(px,py),Vector2.one*.5f)*2;if(radius<.94f)c=C("4b2032");if(radius<.8f)c=C("ff7483");if(radius<.63f)c=new Color(.7f,.10f,.2f,.25f);if(Vector2.Distance(new Vector2(px,py),new Vector2(.32f,.73f))<.09f)c=C("fff5eb");}
    t.SetPixel(x,y,c);
   }t.Apply();cached=Sprite.Create(t,new Rect(0,0,w,h),Vector2.one*.5f,w,0,SpriteMeshType.FullRect);ownedAssets.Add(t);ownedAssets.Add(cached);if(fast)clearFastBullet=cached;else clearSlowBullet=cached;return cached;
  }
  private bool MotherEncounter {get{if(!woodlandMode)return false;foreach(Enemy e in enemies)if(e.boss&&e.hp>0&&e.kind==WoodKinds[9])return true;return false;}}
  private Sprite CapitalMechanismPose(Enemy e){return CapitalProp(e.iceOwner!=null&&e.iceOwner.kind==EnemyKind.CapitalCommander?1:7);}
  private Rect CapitalPoseBounds(Texture2D t,int x,int y,int w,int h){
   Color[] pixels=t.GetPixels(x,y,w,h);bool[] seen=new bool[pixels.Length];int[] queue=new int[pixels.Length];int largest=0;Rect best=new Rect(0,0,w,h);
   for(int start=0;start<pixels.Length;start++){
    if(seen[start]||pixels[start].a<.65f)continue;int head=0,tail=0,x0=w,y0=h,x1=0,y1=0;seen[start]=true;queue[tail++]=start;
    while(head<tail){int at=queue[head++],xx=at%w,yy=at/w;x0=Mathf.Min(x0,xx);y0=Mathf.Min(y0,yy);x1=Mathf.Max(x1,xx+1);y1=Mathf.Max(y1,yy+1);
     for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=xx+dx,ny=yy+dy;if(nx<0||nx>=w||ny<0||ny>=h)continue;int n=ny*w+nx;if(!seen[n]&&pixels[n].a>=.65f){seen[n]=true;queue[tail++]=n;}}
    }if(tail>largest){largest=tail;best=new Rect(x+x0,y+y0,x1-x0,y1-y0);}
   }return best;
  }
  private Sprite CapitalAnimation(int boss,int frame){
   string key="actions/"+boss+"/"+frame;Sprite result;if(capitalArt.TryGetValue(key,out result)&&result!=null)return result;
   string name=new[]{"commander-actions","magistrate-actions","king-actions"}[boss];var t=Resources.Load<Texture2D>("NakiArt/CapitalV1/"+name);
   if(t==null)throw new System.InvalidOperationException("Missing capital action atlas "+name);
   t.filterMode=FilterMode.Point;int cw=t.width/4,ch=t.height/3;float maxHeight=1,anchorX=0;Rect selected=default(Rect);
   for(int i=0;i<12;i++){
    int x=i%4*cw,y=(2-i/4)*ch,pad=Mathf.CeilToInt(cw*.08f),left=Mathf.Max(0,x-pad),right=Mathf.Min(t.width,x+cw+pad);
    Rect bounds=CapitalPoseBounds(t,left,y,right-left,ch);maxHeight=Mathf.Max(maxHeight,bounds.height);if(i==frame){selected=bounds;anchorX=(x+cw*.5f-bounds.x)/bounds.width;}
   }
   result=Sprite.Create(t,selected,new Vector2(anchorX,0),maxHeight/2.65f,0,SpriteMeshType.FullRect);result.name=name+" / "+frame;ownedAssets.Add(result);capitalArt[key]=result;return result;
  }
  private void CapitalActionPose(Enemy e,bool moving){
   if(!IsCapitalBoss(e.kind))return;Sprite[] art;if(!enemyActionArt.TryGetValue(e.kind,out art)||art.Length<12)return;
   CapitalFight s=CapitalState(e);
   // Ground travel must retain its gait during volleys, guard and moving recovery.
   // Hit feedback is still shown by the body tint; it must not freeze a walking actor.
   bool walking=moving&&e.windup<=0&&e.chargeUntil<=clock;
   int frame=walking?Mathf.FloorToInt(clock*7)%4:clock<e.hurtUntil?6:e.windup>0?4:e.chargeUntil>clock?5:clock<s.shield?8:RecoveryWindow(e)?10:s.salvo>0?9:11;e.body.sprite=art[frame];
  }
 }
}
