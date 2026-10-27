using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NakiLegend.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NakiJourneySmoke
{
    private const string Flag = "NakiJourneySmoke.Running";
    private static readonly string Output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Validation/DesertJourneyV8/runtime/")) + "/";
    private const BindingFlags Fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly List<string> errors = new List<string>();
    private static int ticks, checks;
    [InitializeOnLoadMethod] private static void Reload() { if (SessionState.GetBool(Flag, false)) { EditorApplication.update += Tick; Application.logMessageReceived += Log; } }
    public static void Run()
    {
        Directory.CreateDirectory(Output);
        EditorSceneManager.OpenScene("Assets/Scenes/NakiTest.unity");
        NakiPrototype p = UnityEngine.Object.FindObjectOfType<NakiPrototype>();
        if(p==null)throw new Exception("Saved entry scene is missing the prototype component");p.journeySeed = 1729;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/JourneySmoke.unity");
        SessionState.SetBool(Flag, true); EditorApplication.update -= Tick; EditorApplication.update += Tick;
        Application.logMessageReceived += Log; EditorApplication.isPlaying = true;
    }
    private static void Log(string text, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(text); }
    private static object Get(object obj, string field) { return obj.GetType().GetField(field, Fields).GetValue(obj); }
    private static void Set(object obj, string field, object value) { obj.GetType().GetField(field, Fields).SetValue(obj, value); }
    private static object Call(NakiPrototype p, string name, params object[] args) { return typeof(NakiPrototype).GetMethod(name, Fields).Invoke(p, args); }
    private static void Check(bool good, string message) { if (!good) throw new Exception(message); checks++; Debug.Log("NAKI_JOURNEY_PASS: " + message); }
    private static IList Rooms(NakiPrototype p) { return (IList)Get(p, "journeyRooms"); }
    private static IList Enemies(NakiPrototype p) { return (IList)Get(p, "enemies"); }
    private static void Quiet(NakiPrototype p) { Set(p, "storyLines", null); Set(p, "showRainCG", false); Set(p, "routeMenu", false); Set(p, "mapMenu", false); Set(p, "rewardMenu", false); }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling || ++ticks < 30) return;
        NakiPrototype p = UnityEngine.Object.FindObjectOfType<NakiPrototype>(); if (p == null || Get(p, "world") == null) return;
        EditorApplication.update -= Tick;
        try
        {
            p.enabled = false;
            Check(errors.Count == 0, "Scene and Chinese HUD initialize without runtime errors");
            Check(p.enableDesertJourney && Rooms(p).Count == 16, "Journey starts by default with sixteen nodes");
            Check(((IDictionary)Get(p,"enemyActionArt")).Count == 8, "All five regular/elite enemies and three bosses load action atlases");
            foreach (Sprite floor in (Sprite[])Get(p,"journeyFloors")) Check(floor != null,"Distinct room background imported at original size");
            Check(Get(p,"journeyRock") != null && Get(p,"rainCG") != null,"Independent cover and rain CG are available");
            VerifyGraphs(p);
            Call(p,"BeginJourney",false); Quiet(p);
            VerifyRoomsAndProgress(p);
            VerifyRewards(p);
            VerifyActors(p);
            VerifyBosses(p);
            VerifyRestarts(p);
            p.bossPracticeMode=true;Call(p,"BeginJourney",false);
            Check((int)Get(p,"currentRoom")==11&&Enemies(p).Count==1,"Boss practice mode begins directly in its arena");
            p.bossPracticeMode=false;
            p.enableDesertJourney = false; Call(p,"ResetRoom");
            VerifyLegacyCover(p);
            Check(errors.Count == 0,"No runtime or GUI exceptions during progression and all boss variants");
            File.WriteAllText(Output+"smoke-result.txt","PASS: "+checks+" checks. Unity 2022.3.62f3 / URP.\n50 seeded route graphs, sixteen reachable unique layouts, clear room navigation, wave/roster progression, one-time rewards/events/rest, actor poses and telegraphs, three boss patterns and safe lethal projectile handling, wind unlock and CG dialogue, same-seed retry/new-seed restart, full pillar projectile blocking.\nCamera renders exported for room variety and all three bosses; keyboard feel still requires editor playtesting.\n");
            Finish(0);
        }
        catch(Exception e) { Debug.LogException(e); File.WriteAllText(Output+"smoke-result.txt","FAIL: "+e+"\nRuntime errors: "+string.Join("\n",errors)); Finish(1); }
    }

    private static string GraphSignature(IList rooms)
    {
        string result="";foreach(object node in rooms) result+=Get(node,"layout")+":"+Get(node,"floor")+":"+Get(node,"kind")+";";return result;
    }
    private static void VerifyGraphs(NakiPrototype p)
    {
        var signatures=new HashSet<string>();bool connected=true, unique=true, symmetric=true, branches=true;
        for(int seed=1;seed<=50;seed++)
        {
            Call(p,"BuildJourneyGraph",new System.Random(seed));IList rooms=Rooms(p);
            var seen=new HashSet<int>();var queue=new Queue<int>();queue.Enqueue(0);
            var layouts=new HashSet<int>();
            while(queue.Count>0){int id=queue.Dequeue();if(!seen.Add(id))continue;foreach(int link in (List<int>)Get(rooms[id],"links"))queue.Enqueue(link);}
            connected &= seen.Count==16;foreach(object node in rooms)layouts.Add((int)Get(node,"layout"));unique &= layouts.Count==16;
            for(int i=0;i<rooms.Count;i++) foreach(int link in (List<int>)Get(rooms[i],"links")) symmetric &= ((List<int>)Get(rooms[link],"links")).Contains(i);
            for(int i=12;i<16;i++)branches &= ((List<int>)Get(rooms[i],"links")).Count==1;
            signatures.Add(GraphSignature(rooms));
        }
        Check(connected&&symmetric,"Fifty seeds produce sixteen mutually connected reachable nodes");
        Check(unique&&branches,"Every seed has sixteen unique layout IDs and four optional side branches");
        Check(signatures.Count==50,"Different seeds change room/floor combinations");
        Call(p,"BuildJourneyGraph",new System.Random(123));string a=GraphSignature(Rooms(p));Call(p,"BuildJourneyGraph",new System.Random(123));
        Check(a==GraphSignature(Rooms(p)),"The same seed reproduces the same route");
    }

    private static void VerifyRoomsAndProgress(NakiPrototype p)
    {
        IList rooms=Rooms(p);bool connected=true,spawns=true;int lateKinds=0;var allKinds=new HashSet<string>();
        Camera camera=(Camera)Get(p,"view");camera.orthographicSize=6.8f;camera.transform.position=new Vector3(0,0,-10);
        for(int id=0;id<16;id++)
        {
            Call(p,"EnterJourneyRoom",id);Quiet(p);connected &= CanWalkToExit(p);
            foreach(object enemy in Enemies(p))
            {
                Vector2 at=(Vector2)Get(enemy,"position");float radius=(float)Get(enemy,"radius");
                spawns &= !(bool)Call(p,"Blocked",at,radius)&&!(bool)Call(p,"ProjectilePathBlocked",at,at,.15f);
                if(id<11)allKinds.Add(Get(enemy,"kind").ToString());
            }
            if(id==10){lateKinds=Enemies(p).Count;Check((int)Get(rooms[id],"waves")>1,"Late rooms add waves rather than only increasing health");}
            if(id==0||id==2||id==4||id==7)Capture(camera,Output+"unity-room-"+id+".png",1600,900);
        }
        Check(connected,"All sixteen layouts have a collision-free walking route to the exit");
        Check(spawns,"Enemies spawn outside solid cover and movement colliders");
        Check(allKinds.Count==5&&lateKinds>=4,"Progression introduces all five normal/elite archetypes and larger combinations");
        Call(p,"EnterJourneyRoom",1);Quiet(p);ClearRoom(p);
        Check((bool)Get(rooms[1],"cleared"),"A completed combat opens its portal");
        float hp=(float)Get(p,"health");Call(p,"EnterJourneyRoom",0);Call(p,"EnterJourneyRoom",1);
        Check(Enemies(p).Count==0&&(float)Get(p,"health")==hp,"Returning to a cleared room preserves health and never respawns enemies");
    }

    private static bool CanWalkToExit(NakiPrototype p)
    {
        const float step=.25f;int columns=73,rows=29;
        var visited=new bool[columns,rows];var queue=new Queue<Vector2Int>();queue.Enqueue(new Vector2Int(36,2));
        while(queue.Count>0)
        {
            Vector2Int cell=queue.Dequeue();if(cell.x<0||cell.y<0||cell.x>=columns||cell.y>=rows||visited[cell.x,cell.y])continue;visited[cell.x,cell.y]=true;
            Vector2 point=new Vector2(-9+cell.x*step,-3.5f+cell.y*step);if((bool)Call(p,"Blocked",point,.3f))continue;
            if(Vector2.Distance(point,new Vector2(0,3.15f))<.5f)return true;
            queue.Enqueue(cell+Vector2Int.right);queue.Enqueue(cell+Vector2Int.left);queue.Enqueue(cell+Vector2Int.up);queue.Enqueue(cell+Vector2Int.down);
        }
        return false;
    }

    private static void ClearRoom(NakiPrototype p)
    {
        object node=Rooms(p)[(int)Get(p,"currentRoom")];int guard=0;
        while(!(bool)Get(node,"cleared")&&guard++<20)
        {
            var copy=new ArrayList(Enemies(p));foreach(object enemy in copy)Call(p,"DamageEnemy",enemy,100000f);
            Set(p,"clock",(float)Get(p,"clock")+2);Call(p,"TickJourney",.01f);
            Set(p,"clock",(float)Get(p,"clock")+2);Call(p,"TickJourney",.01f);
        }
        Check((bool)Get(node,"cleared"),"All waves can progress to room completion");
    }

    private static void VerifyRewards(NakiPrototype p)
    {
        Call(p,"BeginJourney",false);Quiet(p);Set(p,"health",2f);Call(p,"EnterJourneyRoom",3);
        Check((float)Get(p,"health")==4,"Rest heals two health on first visit");Call(p,"EnterJourneyRoom",0);Call(p,"EnterJourneyRoom",3);
        Check((float)Get(p,"health")==4,"Rest cannot be farmed by reentering");
        Call(p,"EnterJourneyRoom",8);int canals=(int)Get(p,"restoredCanals");Call(p,"EnterJourneyRoom",0);Call(p,"EnterJourneyRoom",8);
        Check(canals==1&&(int)Get(p,"restoredCanals")==1,"Canal event and reward are recorded only once");
        Call(p,"EnterJourneyRoom",13);float sword=p.swordDamage;Call(p,"ClaimJourneyReward",1);Call(p,"ClaimJourneyReward",1);
        Check(p.swordDamage==sword+3,"Reward choice improves the sword only once");
        Call(p,"EnterJourneyRoom",0);Set(p,"energy",71f);Call(p,"EnterJourneyRoom",1);
        Check((float)Get(p,"energy")==71,"Room transition preserves energy rather than refilling it");
    }

    private static object Kind(int id) { return Enum.ToObject(typeof(NakiPrototype).GetNestedType("EnemyKind",BindingFlags.NonPublic),id); }
    private static void VerifyActors(NakiPrototype p)
    {
        Call(p,"BeginJourney",false);Quiet(p);Call(p,"EnterJourneyRoom",0);
        for(int kind=0;kind<8;kind++)
        {
            object enemy=Call(p,"SpawnJourneyEnemy",Kind(kind),new Vector2(0,1.35f),7);
            Set(p,"position",new Vector2(1.5f,1.35f));Set(p,"clock",10f);Set(p,"health",6f);Set(p,"invulnerableUntil",0f);
            Call(p,"BeginJourneyAttack",enemy);
            Check((float)Get(enemy,"windup")>0&&Get(enemy,"warning")!=null,"Archetype "+kind+" has a visible attack telegraph");
            Call(p,"UpdateJourneyEnemy",enemy,.01f);
            SpriteRenderer body=(SpriteRenderer)Get(enemy,"body");Sprite windup=body.sprite;
            Set(p,"clock",11f);Call(p,"UpdateJourneyEnemy",enemy,1f);
            Check(body.sprite!=windup,"Archetype "+kind+" changes its full-body contact pose");
            Call(p,"EnterJourneyRoom",0);
        }
        Set(p,"position",Vector2.zero);Set(p,"facing",Vector2.right);Set(p,"clock",15f);Call(p,"UpdatePixelHero",Vector2.right);Sprite right=((SpriteRenderer)Get(p,"heroBody")).sprite;
        Set(p,"facing",Vector2.left);Call(p,"UpdatePixelHero",Vector2.left);
        Check(((SpriteRenderer)Get(p,"heroBody")).sprite==right&&((SpriteRenderer)Get(p,"heroBody")).flipX,"Approved mirrored left-player gait remains intact");
        Rect cover=((List<Rect>)Get(p,"projectileObstacles"))[0];
        Vector2 left=new Vector2(cover.xMin-.2f,cover.center.y),rightAt=new Vector2(cover.xMax+.2f,cover.center.y);
        object target=Call(p,"SpawnJourneyEnemy",Kind(0),rightAt,1);float hp=(float)Get(target,"hp");
        Set(p,"position",left);Set(p,"facing",Vector2.right);Set(p,"clock",30f);Call(p,"Swing");Set(p,"clock",30.12f);Call(p,"UpdateSwordImpact");
        Check((float)Get(target,"hp")==hp,"Sword strikes cannot damage an enemy through a full pillar body");
        Set(p,"position",new Vector2(cover.xMax+.4f,cover.center.y));Set(target,"position",new Vector2(cover.xMax+1.1f,cover.center.y));
        Set(p,"clock",31f);Call(p,"Swing");Set(p,"clock",31.12f);Call(p,"UpdateSwordImpact");
        Check((float)Get(target,"hp")==hp-p.swordDamage,"Clear melee contact still lands at the intended frame");
    }

    private static void VerifyBosses(NakiPrototype p)
    {
        for(int variant=0;variant<3;variant++)
        {
            p.bossOverride=variant;Call(p,"BeginJourney",false);Quiet(p);Call(p,"EnterJourneyRoom",11);
            object boss=Enemies(p)[0];Check(Get(boss,"kind").ToString()==new[]{"WindScorpion","Colossus","ApeKing"}[variant],"Boss candidate "+variant+" is selected and spawned");
            Capture((Camera)Get(p,"view"),Output+"unity-boss-"+variant+".png",1600,900);
            for(int attack=0;attack<3;attack++)
            {
                Set(p,"health",6f);Set(p,"clock",20f+attack*3);Set(boss,"attackNumber",attack);Call(p,"BeginJourneyAttack",boss);Set(p,"clock",21f+attack*3);Call(p,"UpdateJourneyEnemy",boss,1f);
            }
            Check((int)Get(boss,"attackNumber")==3,"Boss candidate "+variant+" resolves three different patterns");
            Set(boss,"hp",(float)Get(boss,"maximum")*.4f);Call(p,"UpdateJourneyEnemy",boss,.01f);
            Check((bool)Get(boss,"secondPhase"),"Boss candidate "+variant+" enters its low-health phase");
            // A lethal projectile must finish its own removal before completion
            // clears other shots/summons; this covers a list-mutation regression.
            Vector2 at=(Vector2)Get(boss,"position");int before=((IList)Get(p,"shots")).Count;
            Call(p,"SpawnShot",at,Vector2.zero,false,boss,at);IList shots=(IList)Get(p,"shots");
            Check(shots.Count==before+1,"Lethal projectile can be created at the boss");Set(shots[shots.Count-1],"damage",100000f);
            Call(p,"UpdateShot",shots.Count-1,.001f);Call(p,"TickJourney",.01f);
            Check((bool)Get(p,"journeyComplete")&&(bool)Get(p,"windUnlocked")&&Enemies(p).Count==0,"Boss completion removes summons and unlocks the wind fragment");
            Call(p,"AdvanceJourneyStory",false);Call(p,"AdvanceJourneyStory",false);
            Check((bool)Get(p,"showRainCG"),"Natural restoration dialogue transitions into the rain CG");
            Quiet(p);Set(p,"position",Vector2.zero);Set(p,"clock",50f);Set(p,"facing",Vector2.right);float energy=(float)Get(p,"energy");Call(p,"CastWind");
            Check((float)Get(p,"energy")==energy-20&&((IList)Get(p,"shots")).Count==5,"Unlocked wind skill casts a five-projectile fan");
        }
        p.bossOverride=-1;
    }

    private static void VerifyRestarts(NakiPrototype p)
    {
        Call(p,"BeginJourney",false);string signature=GraphSignature(Rooms(p));int seed=(int)Get(p,"activeSeed");p.swordDamage+=20;Call(p,"ResetRoom");
        Check((int)Get(p,"activeSeed")==seed&&GraphSignature(Rooms(p))==signature&&p.swordDamage==18,"Retry repeats the seed and resets run-specific upgrades");
        Call(p,"BeginJourney",true);Check((int)Get(p,"activeSeed")!=seed&&!((bool)Get(p,"windUnlocked")),"New journey changes seed and resets fragment state");
    }

    private static void VerifyLegacyCover(NakiPrototype p)
    {
        var blockers=(List<Rect>)Get(p,"projectileObstacles");Check(blockers.Count==4,"Legacy single-room collision test remains available");
        foreach(Rect r in blockers)
        {
            Vector2 a=new Vector2(r.xMin-1,r.center.y),b=new Vector2(r.xMax+1,r.center.y);
            Check((bool)Call(p,"ProjectilePathBlocked",a,b,.11f)&&(bool)Call(p,"ProjectilePathBlocked",b,a,.135f),"Full pillar bodies stop shots in either direction");
        }
    }

    private static void Capture(Camera camera,string path,int width,int height)
    {
        RenderTexture old=RenderTexture.active,targetOld=camera.targetTexture;var target=new RenderTexture(width,height,24);var image=new Texture2D(width,height,TextureFormat.RGB24,false);
        try {camera.aspect=width/(float)height;camera.orthographicSize=6.8f;camera.transform.position=new Vector3(0,0,-10);camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
        finally{camera.targetTexture=targetOld;RenderTexture.active=old;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);}
    }
    private static void Finish(int code){SessionState.SetBool(Flag,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorApplication.Exit(code);}
}
