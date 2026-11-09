using System;
using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NakiLegend.Prototype;

// Editor-only validation. Requests real domain reloads while dead and alive.
public static class NakiGatehouseReloadSmoke
{
    const string Key = "Naki.Gatehouse.ReloadQA";
    const string Out = "D:/NakiLegend/Repository/Validation/GatehouseV13/runtime/";
    const BindingFlags B = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    static int ticks;
    static NakiPrototype p;
    static object Get(string n) { return typeof(NakiPrototype).GetField(n, B).GetValue(p); }
    static void Set(string n, object v) { typeof(NakiPrototype).GetField(n, B).SetValue(p, v); }
    static object Call(string n, params object[] args) { return typeof(NakiPrototype).GetMethod(n, B).Invoke(p, args); }
    static void Check(bool ok, string why) { if (!ok) throw new Exception(why); SessionState.SetInt(Key+".checks", SessionState.GetInt(Key+".checks",0)+1); }
    [InitializeOnLoadMethod] static void Reloaded() { if (SessionState.GetBool(Key,false)) Attach(); }
    static void Attach() { EditorApplication.update -= Tick; EditorApplication.update += Tick; Application.logMessageReceived -= Log; Application.logMessageReceived += Log; }
    static void Log(string message, string trace, LogType type) { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert) { SessionState.SetBool(Key+".error",true); File.AppendAllText(Out+"reload-errors.txt",message+"\n"+trace+"\n"); } }
    public static void Run()
    {
        Directory.CreateDirectory(Out); File.WriteAllText(Out+"reload-errors.txt","");
        EditorSceneManager.OpenScene("Assets/Scenes/NakiGatehouseSample.unity");
        UnityEngine.Object.FindObjectOfType<NakiPrototype>().showOpeningOnStart=false;
        SessionState.SetBool(Key,true); SessionState.SetBool(Key+".error",false);
        SessionState.SetInt(Key+".step",0); SessionState.SetInt(Key+".checks",0);
        SessionState.SetString(Key+".started",DateTime.UtcNow.Ticks.ToString());
        Attach(); EditorApplication.isPlaying=true;
    }
    static void Quiet() { Call("StopOpeningCinematic");Set("storyLines",null);Set("paused",false);Set("mapMenu",false);Set("inventoryMenu",false); }
    static void Ready()
    {
        Check((float)Get("health")==6,"Recovered player has full health");
        Check(((IDictionary)Get("heroFrames")).Count==7,"All seven animation states rebuilt");
        Check(((IList)Get("journeyRooms")).Count==5,"All five sample room objects rebuilt");
        Check(Get("weaponCatalog")!=null,"Weapon definitions rebuilt");
        Check(!(bool)Call("RuntimeCachesMissing"),"Runtime reports ready after real reload");
        Check(((Transform)Get("world")).gameObject.activeSelf,"Replacement world is active");
        Quiet();Call("UseEquippedWeapon");Call("UpdatePixelHero",Vector2.zero);
        Check(!SessionState.GetBool(Key+".error",false),"No runtime errors after reload and attack");
    }
    static void Request(int next) { SessionState.SetInt(Key+".step",next);ticks=0;EditorUtility.RequestScriptReload(); }
    static void Tick()
    {
        if(DateTime.UtcNow.Ticks-long.Parse(SessionState.GetString(Key+".started",DateTime.UtcNow.Ticks.ToString()))>TimeSpan.FromMinutes(3).Ticks) { Finish("FAIL: reload test timed out",1);return; }
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<80)return;
        p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null)return;
        try
        {
            int step=SessionState.GetInt(Key+".step",0);
            if(step==0){Quiet();Set("health",0f);Call("UpdatePixelHero",Vector2.zero);Check(Get("currentHeroAction").ToString()=="Defeated","Death renders before assembly reload");Request(1);return;}
            if(step==1){Ready();Request(2);return;}
            if(step==2){Ready();Set("health",0f);Set("swordUntil",0f);Call("UpdatePixelHero",Vector2.zero);SessionState.SetInt(Key+".step",3);ticks=0;return;}
            if(step==3){for(int i=0;i<240;i++)Call("Update");Check(Get("currentHeroAction").ToString()=="Defeated","Death still renders for 240 updates after both reloads");Call("ResetRoom");Ready();Check(!SessionState.GetBool(Key+".error",false),"Death, both actual reloads and restart produced no errors");Finish("PASS "+SessionState.GetInt(Key+".checks",0)+" checks; real Play-mode assembly reload while dead and alive; defeated state held for 240 updates; restart and attack validated.",0);}
        }
        catch(Exception e){Finish("FAIL: "+e,1);}
    }
    static void Finish(string result,int code) { File.WriteAllText(Out+"reload-result.txt",result+"\n");SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code); }
}
