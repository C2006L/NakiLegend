using System;
using System.IO;
using System.Reflection;
using NakiLegend.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class NakiEntrySmoke {
 const string Flag="Naki.EntrySmoke";
 static int ticks;
 [InitializeOnLoadMethod] static void Reload(){ if(SessionState.GetBool(Flag,false)) EditorApplication.update+=Tick; }
 public static void Run(){ EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity"); if(UnityEngine.Object.FindObjectsOfType<NakiPrototype>().Length!=1)throw new Exception("Entry scene must have one saved prototype");SessionState.SetBool(Flag,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true; }
 static void Tick(){if(!EditorApplication.isPlaying||++ticks<20)return;EditorApplication.update-=Tick;int code=0;string result;
 try{var p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();var flags=BindingFlags.Instance|BindingFlags.NonPublic;if(p==null||typeof(NakiPrototype).GetField("world",flags).GetValue(p)==null||!(bool)typeof(NakiPrototype).GetField("polishedHeroReady",flags).GetValue(p))throw new Exception("Entry scene failed to start gameplay");result="PASS: saved NakiTest scene loads and starts the polished playable prototype automatically.";}catch(Exception e){result="FAIL: "+e;code=1;}
 File.WriteAllText("D:/NakiLegend/Validation/LeftWalkV6/entry-scene-result.txt",result);SessionState.SetBool(Flag,false);EditorApplication.Exit(code); }
}
