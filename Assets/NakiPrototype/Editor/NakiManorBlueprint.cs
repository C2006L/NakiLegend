using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NakiLegend.Prototype.Editor
{
 public sealed class NakiManorBlueprint : EditorWindow
 {
  [Serializable] public sealed class Room {public int id,stage,x,y,stairsTo;public string title,kind,objective,requires,grants,npc,boss;public string[] waves,story;public int[] links;}
  [Serializable] public sealed class Creature {public string name,appearance,combat,soul;}
  [Serializable] public sealed class Ending {public string name,route,description;}
  [Serializable] public sealed class Blueprint {public int version;public string status,title,opening;public string[] floors;public Room[] rooms;public Creature[] enemies;public Ending[] endings;}
  Blueprint data;Texture2D opening,enemiesArt,bossesArt;int floor,selected,tab;Vector2 scroll;
  [MenuItem("Tools/Naki Legend/Design/6 Manor Blueprint")]
  public static void Open(){var w=GetWindow<NakiManorBlueprint>();w.titleContent=new GUIContent("暮钟岛施工图");w.minSize=new Vector2(1050,650);w.Show();}
  void OnEnable(){Reload();}
  void Reload(){var asset=Resources.Load<TextAsset>("NakiArt/ManorV1/blueprint");if(asset!=null){data=JsonUtility.FromJson<Blueprint>(asset.text);opening=Resources.Load<Texture2D>(data.opening);enemiesArt=Resources.Load<Texture2D>("NakiArt/ManorV1/enemies-concept");bossesArt=Resources.Load<Texture2D>("NakiArt/ManorV1/bosses-concept");}}
  void OnGUI(){
   if(data==null){EditorGUILayout.HelpBox("等待府邸施工资料导入。",MessageType.Info);if(GUILayout.Button("重新加载"))Reload();return;}
   EditorGUILayout.LabelField("暮钟岛 · 大骑士府邸",EditorStyles.boldLabel);
   EditorGUILayout.HelpBox("这是旧版三层蓝图。归功外庭已开放15房、10类小怪、卫长战和开场CG，可通过G航图或Play/6试玩。后两层与最终结局仍待制作；当前外庭实际布局见新版施工说明。",MessageType.Info);
   tab=GUILayout.Toolbar(tab,new[]{"三层房间图","敌人与结局","开场CG"});
   if(tab==2){if(opening!=null)GUI.DrawTexture(new Rect(10,100,position.width-20,position.height-120),opening,ScaleMode.ScaleToFit);return;}
   if(tab==1){scroll=EditorGUILayout.BeginScrollView(scroll);EditorGUILayout.LabelField("旧概念板供后两层参考；外庭已使用独立动作图和战斗机制",EditorStyles.boldLabel);if(enemiesArt!=null)GUI.DrawTexture(GUILayoutUtility.GetRect(100,440),enemiesArt,ScaleMode.ScaleToFit);foreach(var e in data.enemies){EditorGUILayout.LabelField(e.name,EditorStyles.boldLabel);EditorGUILayout.LabelField(e.appearance+"\n"+e.combat+"\n"+e.soul,EditorStyles.wordWrappedLabel);GUILayout.Space(14);}if(bossesArt!=null)GUI.DrawTexture(GUILayoutUtility.GetRect(100,330),bossesArt,ScaleMode.ScaleToFit);EditorGUILayout.LabelField("誓庭卫长 → 掌印机关 → 暗黑大骑士",EditorStyles.boldLabel);foreach(var e in data.endings){EditorGUILayout.LabelField(e.name+" · "+e.route,EditorStyles.boldLabel);EditorGUILayout.LabelField(e.description,EditorStyles.wordWrappedLabel);GUILayout.Space(12);}EditorGUILayout.EndScrollView();return;}
   int next=GUILayout.Toolbar(floor,data.floors);if(next!=floor){floor=next;selected=floor*15;scroll=Vector2.zero;}
   float graphWidth=Mathf.Max(610,position.width*.60f),cw=(graphWidth-30)/5,ch=110;
   Func<Room,Rect> rect=n=>new Rect(14+n.x*cw,160+(2-n.y)*ch,cw-10,85);
   Handles.BeginGUI();Handles.color=new Color(.4f,.6f,.65f);foreach(var n in data.rooms.Where(r=>r.stage==floor))foreach(int id in n.links)if(id>n.id)Handles.DrawLine(rect(n).center,rect(data.rooms[id]).center);Handles.EndGUI();
   foreach(var n in data.rooms.Where(r=>r.stage==floor)){
    Color old=GUI.backgroundColor;GUI.backgroundColor=n.id==selected?new Color(.85f,.7f,.35f):n.kind=="Boss"?new Color(.7f,.35f,.48f):n.kind=="Combat"||n.kind=="Elite"?new Color(.35f,.45f,.6f):new Color(.35f,.65f,.55f);
    if(GUI.Button(rect(n),n.id+" · "+n.title+"\n"+n.kind+(n.waves.Length>0?" · "+n.waves.Length+"波":"")+(n.stairsTo>=0?"\n楼梯 → "+n.stairsTo:"")))selected=n.id;GUI.backgroundColor=old;
   }
   GUI.Label(new Rect(18,510,graphWidth-30,60),"点击房间查看任务、波次和对白。连线为实际相邻房间；楼梯成对连接楼层。\n贤者之石路线保留全部积攒能量，“一人份”指完整意志。",EditorStyles.wordWrappedLabel);
   GUILayout.BeginArea(new Rect(graphWidth+8,135,position.width-graphWidth-20,position.height-150));scroll=GUILayout.BeginScrollView(scroll);
   var room=data.rooms[selected];GUILayout.Label(room.title,EditorStyles.boldLabel);GUILayout.Label(room.objective,EditorStyles.wordWrappedLabel);
   if(!string.IsNullOrEmpty(room.requires))GUILayout.Label("前置："+room.requires);if(!string.IsNullOrEmpty(room.grants))GUILayout.Label("取得记录："+room.grants);
   if(!string.IsNullOrEmpty(room.npc))GUILayout.Label("安全区域人物："+room.npc);if(!string.IsNullOrEmpty(room.boss))GUILayout.Label("Boss："+room.boss);
   for(int i=0;i<room.waves.Length;i++){GUILayout.Space(7);GUILayout.Label("第"+(i+1)+"波："+room.waves[i],EditorStyles.wordWrappedLabel);}
   foreach(string line in room.story){GUILayout.Space(8);GUILayout.Label(line,EditorStyles.wordWrappedLabel);}
   GUILayout.EndScrollView();GUILayout.EndArea();
  }
  public static void ValidateBatch(){
   var text=Resources.Load<TextAsset>("NakiArt/ManorV1/blueprint");if(text==null)throw new Exception("Missing manor graph");var b=JsonUtility.FromJson<Blueprint>(text.text);
   if(b.rooms.Length!=45||b.enemies.Length!=8||b.endings.Length!=3||Resources.Load<Texture2D>(b.opening)==null)throw new Exception("Incomplete manor design packet");
   foreach(var n in b.rooms){if(n.id<0||n.id>=45||n.stage!=n.id/15||b.rooms[n.id]!=n)throw new Exception("Invalid room ID");foreach(int id in n.links){var other=b.rooms[id];if(other.stage!=n.stage||!other.links.Contains(n.id)||Math.Abs(n.x-other.x)+Math.Abs(n.y-other.y)!=1)throw new Exception("Invalid adjacent connection");}if(n.stairsTo>=0&&b.rooms[n.stairsTo].stairsTo!=n.id)throw new Exception("Unpaired stairs");if(!string.IsNullOrEmpty(n.npc)&&n.waves.Length>0)throw new Exception("Civilian in combat wave");}
   var reached=new HashSet<int>{0};var grants=new HashSet<string>();bool changed=true;while(changed){changed=false;foreach(int id in reached.ToArray()){var n=b.rooms[id];if(!string.IsNullOrEmpty(n.grants)&&grants.Add(n.grants))changed=true;foreach(int next in n.links.Concat(n.stairsTo<0?Array.Empty<int>():new[]{n.stairsTo})){var r=b.rooms[next];if(string.IsNullOrEmpty(r.requires)||grants.Contains(r.requires))if(reached.Add(next))changed=true;}}}
   if(reached.Count!=45||!grants.Contains("knight_defeated"))throw new Exception("Quest dependency or map softlock");Debug.Log("MANOR_BLUEPRINT_PASS: 45 reachable rooms, paired stairs, quest dependencies, 8 enemy definitions, 3 endings, protected NPCs and opening art");
  }
 }
}
