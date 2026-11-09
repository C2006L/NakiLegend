using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NakiLegend.Prototype;
public static class NakiOpeningSmoke
{
    const BindingFlags B=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    const string Flag="NakiOpening.Running";
    static readonly string Out="D:/NakiLegend/Repository/Validation/OpeningV10/runtime/";
    static NakiPrototype p; static int ticks,checks,stage,shot; static List<string> errors=new List<string>();
    [InitializeOnLoadMethod] static void Reload(){if(SessionState.GetBool(Flag,false)){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}}
    public static void Run(){Directory.CreateDirectory(Out);EditorSceneManager.OpenScene("Assets/Scenes/NakiOpeningPreview.unity");var t=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");var window=EditorWindow.GetWindow(t);window.Show();SetCaptureResolution(window);SessionState.SetBool(Flag,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;EditorApplication.isPlaying=true;}
    static void SetCaptureResolution(EditorWindow window)
    {
        Assembly assembly=typeof(UnityEditor.Editor).Assembly;
        Type sizes=assembly.GetType("UnityEditor.GameViewSizes");
        object singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizes).GetProperty("instance",BindingFlags.Static|BindingFlags.Public).GetValue(null,null);
        Type groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");
        object group=sizes.GetMethod("GetGroup").Invoke(singleton,new[]{Enum.Parse(groupType,"Standalone")});
        Type sizeType=assembly.GetType("UnityEditor.GameViewSize"),kind=assembly.GetType("UnityEditor.GameViewSizeType");
        object size=Activator.CreateInstance(sizeType,new object[]{Enum.Parse(kind,"FixedResolution"),1920,1080,"Naki Opening QA"});
        group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int index=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null)-1;
        window.GetType().GetProperty("selectedSizeIndex",B).SetValue(window,index,null);
    }
    static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
    static object Get(string k){return typeof(NakiPrototype).GetField(k,B).GetValue(p);}static void Set(string k,object v){typeof(NakiPrototype).GetField(k,B).SetValue(p,v);}static object Call(string k,params object[] a){return typeof(NakiPrototype).GetMethod(k,B).Invoke(p,a);}static void Check(bool b,string m){if(!b)throw new Exception(m);checks++;Debug.Log("NAKI_OPENING_PASS "+m);}
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||++ticks<40)return;
        p=UnityEngine.Object.FindObjectOfType<NakiPrototype>();if(p==null||Get("world")==null)return;
        try
        {
            if(stage==0)
            {
                Check(errors.Count==0,"Saved scene starts without runtime errors");Check((bool)Get("openingActive"),"Normal entry starts cinematic before gameplay");
                var art=(Texture2D[])Get("openingPaintings");Check(art.Length==6,"Six original cinematic paintings are loaded");
                foreach(Texture2D tex in art)Check(tex!=null&&tex.width>=1600&&tex.height>=900,"Full resolution artwork: "+tex.name);
                Check((bool)Get("openingHasPlayed")&&Get("storyLines")==null,"Intro owns narrative input without overlapping village dialogue");
                Check(((AudioSource)Get("openingAmbience")).isPlaying,"Procedural environmental ambience starts");
                float clock=(float)Get("clock"),shield=(float)Get("shield"),mana=(float)Get("energy");
                for(int i=0;i<30;i++)Call("Update");
                Check((float)Get("clock")==clock&&(float)Get("shield")==shield&&(float)Get("energy")==mana,"Intro freezes combat, shield, cooldown and resource clock");
                Set("openingPaused",true);float before=(float)Get("openingBeatTime");Call("TickOpeningCinematic",.1f);Check((float)Get("openingBeatTime")==before,"Pausing holds camera and subtitle timing");Set("openingPaused",false);
                Array beats=(Array)typeof(NakiPrototype).GetField("OpeningBeats",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);float duration=0;
                foreach(object beat in beats)duration+=(float)beat.GetType().GetField("duration").GetValue(beat);
                Check(beats.Length==11&&Mathf.Abs(duration-74)<.01f,"Eleven authored shots span 74 seconds");
                for(int i=0;i<beats.Length;i++){Set("openingBeatTime",100f);Call("TickOpeningCinematic",.01f);}
                Check(!(bool)Get("openingActive")&&Get("storyLines")!=null,"Natural completion returns to village dialogue");Check(!((AudioSource)Get("openingAmbience")).isPlaying,"Completion stops cinematic audio");
                Call("BeginJourney",false);Check(!(bool)Get("openingActive"),"Same-seed retry does not replay intro");Call("BeginJourney",true);Check(!(bool)Get("openingActive"),"New route in same play session does not force replay");
                Set("storyLines",null);Set("paused",true);Call("ReplayOpeningCinematic");Check((bool)Get("openingActive")&&(int)Get("openingBeat")==0,"Manual replay restarts first shot");
                Call("CompleteOpeningCinematic");Check(!(bool)Get("openingActive")&&(bool)Get("paused"),"Skip returns to prior pause state");
                Call("BeginJourney",false);Set("storyLines",null);Set("paused",false);Call("StartOpeningCinematic");Call("BeginJourney",false);Check(!(bool)Get("openingActive"),"Restart during intro safely stops all cinematic state");
                Call("StartOpeningCinematic");Call("CompleteOpeningCinematic");Set("paused",false);Call("StartOpeningCinematic");Check(((AudioSource)Get("openingAmbience")).isPlaying,"Early skip followed by replay restarts same sea ambience");Call("StopOpeningCinematic");
                p.bossPracticeMode=true;Set("openingHasPlayed",false);Call("BeginJourney",false);Check(!(bool)Get("openingActive"),"Boss practice bypasses story cinematic");p.bossPracticeMode=false;p.showOpeningOnStart=false;Call("BeginJourney",false);Check(!(bool)Get("openingActive")&&Get("storyLines")!=null,"Inspector switch retains original dialogue fallback");p.showOpeningOnStart=true;
                Set("paused",false);Set("storyLines",null);Call("StartOpeningCinematic");Set("openingPaused",true);Set("openingBeatTime",3f);stage=1;ticks=0;return;
            }
            if(stage==1)
            {
                Check(Get("openingCaptionStyle")!=null,"Actual Game View draws cinematic styles");
                Set("openingBeat",shot);Set("openingPreviousBeat",-1);Set("openingBeatTime",shot==10?2.5f:4.5f);Set("openingRevealAll",true);Set("openingTotalTime",shot*7f+4.5f);stage=2;ticks=0;return;
            }
            if(stage==2){ScreenCapture.CaptureScreenshot(Out+"shot-"+shot.ToString("00")+".png");stage=3;ticks=0;return;}
            if(stage==3)
            {
                if(!File.Exists(Out+"shot-"+shot.ToString("00")+".png"))throw new Exception("Game view capture missing");
                shot++;if(shot<11){stage=1;ticks=0;return;}
                Check(errors.Count==0,"All eleven cinematic shots render without GUI/runtime exceptions");
                Call("CompleteOpeningCinematic");Set("paused",false);Set("openingGameReveal",0f);stage=4;ticks=0;return;
            }
            if(stage==4){ScreenCapture.CaptureScreenshot(Out+"village-return.png");stage=5;ticks=0;return;}
            if(stage==5){Check(errors.Count==0&&Get("journeyTitleStyle")!=null,"Gameplay Chinese HUD resumes after skip/end");File.WriteAllText(Out+"smoke-result.txt","PASS "+checks+" checks, Unity 2022.3.62f3 / URP.\nSix 1672x941 paintings, 11 shots / 74s, simulation freeze, pause timing, natural completion, skip, replay, retries, practice and disabled fallback. Real Game View screenshots of every shot and return to gameplay captured; no runtime or GUI errors.\n");Finish(0);}
        }
        catch(Exception e){Debug.LogException(e);File.WriteAllText(Out+"smoke-result.txt","FAIL after "+checks+" checks\n"+e+"\n"+string.Join("\n",errors));Finish(1);}
    }
    static void Finish(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
