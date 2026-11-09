#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace NakiLegend.Prototype.Editor
{
 public sealed class NakiWeaponGallery:EditorWindow
 {
  // These fields are assigned by JsonUtility.
#pragma warning disable 0649
  [Serializable] private sealed class Manifest {public Entry[] entries;}
  [Serializable] private sealed class Entry {public int id,x,y,width,height;public string displayName,sheet,form,cast,hit;}
#pragma warning restore 0649
  private Manifest data;private Vector2 scroll;private string search="";private int filter;
  [MenuItem("Tools/Naki Legend/Design/Weapon Gallery")]
  public static void Open(){var w=GetWindow<NakiWeaponGallery>("Naki 武器图鉴");w.minSize=new Vector2(740,540);w.Show();}
  private void OnGUI()
  {
   if(data==null){var asset=Resources.Load<TextAsset>("NakiArt/WeaponIdentityV3/manifest");if(asset==null){EditorGUILayout.HelpBox("等待武器资源导入完成。",MessageType.Info);return;}data=JsonUtility.FromJson<Manifest>(asset.text);}
   EditorGUILayout.LabelField("NAKI · 武器图鉴 · "+data.entries.Length+" 件",EditorStyles.boldLabel);
   EditorGUILayout.LabelField("查看独立外观与发射/命中音色；不会改动旅程和背包。",EditorStyles.miniLabel);
   search=EditorGUILayout.TextField("名称 / 编号",search);filter=GUILayout.Toolbar(filter,new[]{"全部","光之力与贤者之石","首领专属"});
   scroll=EditorGUILayout.BeginScrollView(scroll);int cols=Mathf.Max(2,Mathf.FloorToInt((position.width-28)/235));int index=0;
   foreach(var e in data.entries){if(filter==1&&e.id<123||filter==2&&(e.id<72||e.id>=123))continue;if(search.Length>0&&!e.displayName.Contains(search)&&e.id.ToString()!=search)continue;
    if(index%cols==0)EditorGUILayout.BeginHorizontal();EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width((position.width-35)/cols),GUILayout.Height(176));
    GUILayout.Label(e.displayName,EditorStyles.boldLabel);var texture=Resources.Load<Texture2D>(e.sheet);Rect r=GUILayoutUtility.GetRect(205,88,GUILayout.ExpandWidth(true));EditorGUI.DrawRect(r,new Color(.055f,.095f,.12f));
    if(texture!=null){float scale=Mathf.Min((r.width-18)/e.width,(r.height-14)/e.height);Rect at=new Rect(r.center.x-e.width*scale/2,r.center.y-e.height*scale/2,e.width*scale,e.height*scale);GUI.DrawTextureWithTexCoords(at,texture,new Rect(e.x/(float)texture.width,e.y/(float)texture.height,e.width/(float)texture.width,e.height/(float)texture.height));}
    GUILayout.Label("#"+e.id+" · "+e.form,EditorStyles.miniLabel);EditorGUILayout.BeginHorizontal();if(GUILayout.Button("听发射"))Preview(e.cast);if(GUILayout.Button("听命中"))Preview(e.hit);EditorGUILayout.EndHorizontal();EditorGUILayout.EndVertical();
    index++;if(index%cols==0)EditorGUILayout.EndHorizontal();
   }
   if(index%cols!=0)EditorGUILayout.EndHorizontal();EditorGUILayout.EndScrollView();
  }
  private static void Preview(string key)
  {
   var clip=Resources.Load<AudioClip>("NakiCombatAudioV3/"+key);if(clip==null)return;
   var util=typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");if(util==null)return;
   var flags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
   var stop=util.GetMethod("StopAllPreviewClips",flags);if(stop!=null)stop.Invoke(null,null);
   var play=util.GetMethod("PlayPreviewClip",flags,null,new[]{typeof(AudioClip),typeof(int),typeof(bool)},null);if(play!=null)play.Invoke(null,new object[]{clip,0,false});
  }
 }
}
#endif
