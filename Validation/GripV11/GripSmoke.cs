using System;
using System.IO;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NakiLegend.Prototype;
public static class NakiGripSmoke
{
 const BindingFlags B=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
 const string Flag="NakiGripV11.Run";static readonly string Out="D:/NakiLegend/Repository/Validation/GripV11/runtime/";
 static NakiPrototype p;static int ticks,poses;static System.Collections.Generic.List<string> errors=new System.Collections.Generic.List<string>();
 [InitializeOnLoadMethod]static void Reload(){if(SessionState.GetBool(Flag,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
 public static void Run(){Directory.CreateDirectory(Out);EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");var proto=UnityEngine.Object.FindObjectOfType<NakiPrototype>();proto.showOpeningOnStart=false;SessionState.SetBool(Flag,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;EditorApplication.isPlaying=true;}
 static void Log(string text,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(text);}
 static object Get(object o,string f){return o.GetType().GetField(f,B).GetValue(o);}static object Get(string f){return Get(p,f);}static void Set(object o,string f,object v){o.GetType().GetField(f,B).SetValue(o,v);}static void Set(string f,object v){Set(p,f,v);}static object Call(string f,params object[] a){return typeof(NakiPrototype).GetMethod(f,B).Invoke(p,a);}static object Action(string s){return Enum.Parse(typeof(NakiPrototype).GetNestedType("HeroAction",BindingFlags.NonPublic),s);}static void Check(bool condition,string text){if(!condition)throw new Exception(text);}
 static object Frame(){var map=(IDictionary)Get("heroFrames");var dirs=(Array)map[Get("currentHeroAction")];var frames=(Array)dirs.GetValue((int)Get("pixelDirection"));return frames.GetValue((int)Get("currentHeroFrame"));}
 static Vector2 Direction(int d){return d==0?Vector2.down:d==1?Vector2.right:d==2?Vector2.up:Vector2.left;}
 static void Pose(string action,int direction,int frame,int count)
 {
  float clock=100,phase=(frame+.2f)/count;Vector2 aim=Direction(direction);
  Set("clock",clock);Set("health",6f);Set("position",Vector2.zero);Set("facing",aim);Set("castDirection",aim);Set("swingDirection",aim);Set("dashDirection",aim);
  Set("pixelHurtUntil",0f);Set("dashUntil",0f);Set("swordUntil",0f);Set("castPoseUntil",0f);Set("currentHeroAction",Action(action));
  if(action=="Walk")Set("polishedWalkStarted",clock-phase/1.6f);
  if(action=="Attack"){Set("swingStarted",clock-.32f*phase);Set("meleeActionDuration",.32f);Set("swordUntil",clock+.32f*(1-phase));}
  if(action=="Cast")Set("castPoseUntil",clock+.32f*(1-phase));
  if(action=="Dodge")Set("dashUntil",clock+.17f*(1-phase));
  if(action=="Hurt")Set("pixelHurtUntil",clock+.2f*(1-phase));
  if(action=="Defeated")Set("health",0f);
  Call("UpdatePixelHero",action=="Walk"?aim:Vector2.zero);
 }
 static void Verify(string action,int direction,int weapon)
 {
  GameObject sword=(GameObject)Get("sword");var cover=(SpriteRenderer)Get("heldHandCover");
  if(action=="Defeated"){Check(!sword.activeSelf&&!cover.gameObject.activeSelf,"Defeated body retains a floating weapon");return;}
  object frame=Frame();Vector2 grip=(Vector2)Get(frame,"grip");float scale=p.pixelHeroScale;
  Check(Vector2.Distance(sword.transform.position,grip*scale)<.00001f,"Weapon pivot detached from authored hand");
  Check(Vector2.Distance(cover.transform.position,sword.transform.position)<.00001f,"Finger cover detached from grip");
  Check(cover.sprite!=null&&cover.flipX==(bool)Get(frame,"flipX"),"Missing or incorrectly mirrored finger cover");
  Check(cover.sortingOrder>sword.GetComponent<SpriteRenderer>().sortingOrder,"Weapon renders over all fingers");
  if(action=="Attack"&&((int)Get("currentHeroFrame")==2||(int)Get("currentHeroFrame")==3))Check(Vector2.Dot(sword.transform.right,Direction(direction))>.65f,"Attack points away from aimed direction");
  poses++;
 }
 static void Render(string name,bool handOnly=false)
 {
  var c=(Camera)Get("view");var sword=(GameObject)Get("sword");var old=c.targetTexture;var active=RenderTexture.active;
  var rt=new RenderTexture(handOnly?256:480,handOnly?256:320,24);var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
  foreach(SpriteRenderer sr in ((Transform)Get("world")).GetComponentsInChildren<SpriteRenderer>(true))sr.gameObject.layer=30;
  foreach(string field in new[]{"hero","sword","heroShadow","weaponTrail"})((GameObject)Get(field)).layer=0;
  ((SpriteRenderer)Get("heldHandCover")).gameObject.layer=0;
  try{c.cullingMask=1;c.backgroundColor=new Color(.72f,.68f,.59f);c.clearFlags=CameraClearFlags.SolidColor;c.aspect=rt.width/(float)rt.height;c.orthographicSize=handOnly?.27f:1.4f;Vector3 at=handOnly?sword.transform.position:new Vector3(0,.55f,0);c.transform.position=new Vector3(at.x,at.y,-10);c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(Out+name+".png",tex.EncodeToPNG());}finally{c.targetTexture=old;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
 static void Tick()
 {
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<35)return;p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;EditorApplication.update-=Tick;
  try{
   p.enabled=false;Call("StopOpeningCinematic");Set("storyLines",null);Set("paused",false);
   Check((bool)Get("layeredHeroReady"),"Layered body unavailable");
   var inventory=(int[])Get("carriedWeapons");Set("equippedSlot",0);
   string[] actions={"Idle","Walk","Attack","Cast","Dodge","Hurt","Defeated"};int[] counts={2,6,6,4,4,2,1};
   foreach(float scale in new[]{.75f,1f,1.25f}){p.pixelHeroScale=scale;
    for(int weapon=0;weapon<8;weapon++){inventory[0]=weapon;
     for(int d=0;d<4;d++)for(int a=0;a<actions.Length;a++)for(int f=0;f<counts[a];f++){
      Pose(actions[a],d,f,counts[a]);Verify(actions[a],d,weapon);
      if(scale==1 && f==0 && actions[a]=="Idle")Render("weapon-"+weapon+"-dir-"+d);
      if(scale==1 && weapon==0 && (actions[a]=="Walk"||actions[a]=="Attack"))Render(actions[a]+"-dir-"+d+"-frame-"+f);
      if(scale==1 && weapon==0 && f==0 && (actions[a]=="Cast"||actions[a]=="Dodge"||actions[a]=="Hurt"))Render(actions[a]+"-dir-"+d);
      if(scale==1 && (weapon==0||weapon==3) && actions[a]=="Idle" && f==0)Render("hand-weapon-"+weapon+"-dir-"+d,true);
     }
    }
   }
   // Walking left must reuse both the right sprite and its mirrored grip.
   inventory[0]=0;p.pixelHeroScale=1;
   for(int f=0;f<6;f++){Pose("Walk",1,f,6);object r=Frame();Pose("Walk",3,f,6);object l=Frame();Vector2 rg=(Vector2)Get(r,"grip"),lg=(Vector2)Get(l,"grip");Check(Get(r,"sprite")==Get(l,"sprite")&&Mathf.Abs(rg.x+lg.x)<.0001f&&Mathf.Abs(rg.y-lg.y)<.0001f,"Left gait and hand socket lost mirror alignment");}
   // New room replaces its finger overlay instead of keeping a stale renderer.
   Call("EnterJourneyRoom",1);Pose("Idle",3,0,2);Verify("Idle",3,0);Check(((SpriteRenderer)Get("heldHandCover")).transform.parent==(Transform)Get("world"),"Room transition retains previous hand overlay");
   Check(errors.Count==0,"Runtime/GUI error: "+string.Join("\n",errors));File.WriteAllText(Out+"smoke-result.txt","PASS: "+poses+" live pose combinations. Unity 2022.3.62f3 / URP.\nEight weapons, four directions, idle/walk/attack/cast/dodge/hurt/defeated across 0.75/1/1.25 scales; same hand socket for weapon and finger cover; left attacks aim left; mirrored gait/sockets; room transition overlay cleanup. Actual isolated camera renders exported for visual verification.\n");Finish(0);
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+poses+" poses\n"+e+"\n"+string.Join("\n",errors));Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Flag,false);Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
