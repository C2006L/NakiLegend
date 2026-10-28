using UnityEngine;

namespace NakiLegend.Prototype
{
    public sealed partial class NakiPrototype
    {
        [Header("Opening cinematic")]
        public bool showOpeningOnStart = true;
        [Range(0, 1)] public float openingAudioVolume = .22f;
        private sealed class OpeningBeat
        {
            public int art; public float duration; public string chapter, caption;
            public Vector2 focus; public float zoomStart, zoomEnd;
            public OpeningBeat(int image, float seconds, string section, string line, Vector2 anchor, float from, float to)
            { art=image; duration=seconds; chapter=section; caption=line; focus=anchor; zoomStart=from; zoomEnd=to; }
        }
        private static readonly OpeningBeat[] OpeningBeats = {
            new OpeningBeat(0,6,"序章 · 风起之前","很久以前，海上的人们，相信风会带来归期。",new Vector2(.52f,.45f),1.02f,1.06f),
            new OpeningBeat(0,8,"Naki 群岛 · 神鸟的传说","不死鸟 NakiTong，将自己的心淬炼成石，让不同的力量在这片土地上共存。",new Vector2(.30f,.69f),1.06f,1.12f),
            new OpeningBeat(1,7,"圣所 · 破碎之夜","直到有人，想独占它的力量。",new Vector2(.57f,.56f),1.02f,1.07f),
            new OpeningBeat(1,6,"失衡 · 散落的元素","元素被抽离，散落各岛，落入强大的守关者体内。山海，从此失去平衡。",new Vector2(.48f,.59f),1.07f,1.10f),
            new OpeningBeat(2,10,"王宫 · 受命","国王：去把散落各岛的碎片带回来。我们的人民，需要它们。\n少年：我会尽力。",new Vector2(.53f,.56f),1.01f,1.04f),
            new OpeningBeat(3,7,"启程 · 去往沙漠岛","一卷航图，一柄剑。少年登上渡船，航向失去雨季的沙漠岛。",new Vector2(.60f,.55f),1.02f,1.06f),
            new OpeningBeat(3,4,"靠岸 · 沙城之外","船夫：前面便是沙城。过了浅滩，就沿旧石路上岸。",new Vector2(.35f,.55f),1.06f,1.08f),
            new OpeningBeat(4,6,"第一岛 · 井底余凉","木桶落到井底，只碰到沙。",new Vector2(.32f,.42f),1.03f,1.07f),
            new OpeningBeat(5,7,"村落 · 最后一口水","少年：你不渴吗？\n孩子：它也渴。",new Vector2(.48f,.55f),1.01f,1.025f),
            new OpeningBeat(4,7,"山口 · 不再归来的云","老人：从前，风会把海上的云带过来。如今，它只带走井里的凉气。",new Vector2(.76f,.62f),1.03f,1.07f),
            new OpeningBeat(5,6,"约定 · 让风认得回家的路","少年：我去看看，风迷路在哪里。",new Vector2(.55f,.55f),1.01f,1.025f),
            new OpeningBeat(5,4,"第一岛 · 失去雨季的沙原","愿长风携雨而来",new Vector2(.50f,.55f),1.025f,1.03f)
        };
        private readonly Texture2D[] openingPaintings = new Texture2D[6];
        private bool openingLoaded, openingActive, openingHasPlayed, openingPaused, openingRevealAll, openingSavedPaused;
        private int openingBeat, openingPreviousBeat = -1;
        private float openingBeatTime, openingTotalTime, openingGameReveal;
        private Texture2D openingShade, openingVignette;
        private Font openingFont, openingTitleFont;
        private GUIStyle openingCaptionStyle, openingSmallStyle, openingHeadingStyle, openingLogoStyle, openingChapterStyle, openingVoyageStyle;
        private AudioSource openingAmbience, openingAccent;
        private AudioClip openingSea, openingWind, openingBell;

        private bool LoadOpeningAssets()
        {
            if (!openingLoaded)
            {
                string[] names = { "phoenix", "fracture", "court", "voyage", "well", "seedling" };
                for (int i=0;i<names.Length;i++)
                {
                    openingPaintings[i]=Resources.Load<Texture2D>("NakiArt/OpeningV10/"+names[i]);
                    if(openingPaintings[i]!=null) openingPaintings[i].filterMode=FilterMode.Bilinear;
                }
                openingLoaded=true;
            }
            foreach(Texture2D painting in openingPaintings) if(painting==null) return false;
            return true;
        }
        private bool TryStartOpeningCinematic()
        {
            if (!showOpeningOnStart || openingHasPlayed || bossPracticeMode) return false;
            return StartOpeningCinematic();
        }
        [ContextMenu("Replay opening cinematic")]
        private void ReplayOpeningCinematic()
        {
            if(Application.isPlaying && enableDesertJourney && !bossPracticeMode) StartOpeningCinematic();
        }
        private bool StartOpeningCinematic()
        {
            if(!LoadOpeningAssets()) { Debug.LogWarning("Naki opening: paintings missing; keeping village dialogue."); return false; }
            openingHasPlayed=true; openingActive=true; openingPaused=openingRevealAll=false;
            openingBeat=0; openingPreviousBeat=-1; openingBeatTime=openingTotalTime=openingGameReveal=0;
            openingSavedPaused=paused; storyLines=null; mapMenu=rewardMenu=showRainCG=inventoryMenu=false;pendingWeaponPickup=null;
            SetupOpeningAudio(); ApplyOpeningSoundscape();
            return true;
        }
        private bool TickOpeningCinematic(float dt)
        {
            dt=Mathf.Clamp(dt,0,.1f);
            if(!openingActive) { openingGameReveal=Mathf.Max(0,openingGameReveal-dt*1.5f); return false; }
            if(Pressed('e')) { CompleteOpeningCinematic(); return true; }
            if(Pressed(' '))
            {
                openingPaused=!openingPaused;
                if(openingAmbience!=null) { if(openingPaused) openingAmbience.Pause(); else openingAmbience.UnPause(); }
                if(openingAccent!=null) { if(openingPaused) openingAccent.Pause(); else openingAccent.UnPause(); }
            }
            if(openingPaused) return true;
            if(Pressed('f'))
            {
                if(!openingRevealAll && openingBeatTime*24<OpeningBeats[openingBeat].caption.Length) openingRevealAll=true;
                else AdvanceOpeningBeat();
                return true;
            }
            openingBeatTime+=dt; openingTotalTime+=dt;
            if(openingBeatTime>=OpeningBeats[openingBeat].duration) AdvanceOpeningBeat();
            return true;
        }
        private void AdvanceOpeningBeat()
        {
            openingPreviousBeat=openingBeat; openingBeat++; openingBeatTime=0; openingRevealAll=false;
            if(openingBeat>=OpeningBeats.Length) { CompleteOpeningCinematic(); return; }
            ApplyOpeningSoundscape();
        }
        private void StopOpeningCinematic()
        {
            openingActive=openingPaused=false; openingGameReveal=0;
            if(openingAmbience!=null) openingAmbience.Stop();
            if(openingAccent!=null) openingAccent.Stop();
        }
        private void CompleteOpeningCinematic()
        {
            StopOpeningCinematic(); openingGameReveal=1; paused=openingSavedPaused;
            TellDesertLanding();
        }
        private void EnsureOpeningStyles()
        {
            if(openingCaptionStyle!=null) return;
            openingFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","Noto Sans CJK SC","SimHei","Arial"},24);
            openingTitleFont=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman","Arial"},56);
            if(openingFont!=null) ownedAssets.Add(openingFont);
            if(openingTitleFont!=null) ownedAssets.Add(openingTitleFont);
            openingCaptionStyle=NewLabel(23,C("f6f0e4"),FontStyle.Normal); openingCaptionStyle.wordWrap=true; openingCaptionStyle.alignment=TextAnchor.MiddleCenter;
            openingSmallStyle=NewLabel(12,C("d9d4c9"),FontStyle.Normal);
            openingHeadingStyle=NewLabel(14,C("e5c88b"),FontStyle.Normal);
            openingVoyageStyle=NewLabel(16,C("173e4b"),FontStyle.Normal);
            openingChapterStyle=NewLabel(35,C("f7e9c9"),FontStyle.Normal); openingChapterStyle.alignment=TextAnchor.MiddleCenter;
            openingLogoStyle=NewLabel(54,C("fff1d2"),FontStyle.Normal); openingLogoStyle.font=openingTitleFont;
            foreach(var style in new[]{openingCaptionStyle,openingSmallStyle,openingHeadingStyle,openingChapterStyle,openingVoyageStyle}) style.font=openingFont!=null?openingFont:journeyFont;
            openingShade=new Texture2D(1,96,TextureFormat.RGBA32,false);
            openingVignette=new Texture2D(128,72,TextureFormat.RGBA32,false);
            for(int y=0;y<96;y++) openingShade.SetPixel(0,y,new Color(0,0,0,Mathf.Pow(1-y/95f,1.5f)*.90f));
            for(int y=0;y<72;y++) for(int x=0;x<128;x++)
            {
                float r=new Vector2((x-63.5f)/76,(y-35.5f)/51).magnitude;
                openingVignette.SetPixel(x,y,new Color(.015f,.025f,.035f,Mathf.SmoothStep(0,.40f,Mathf.InverseLerp(.5f,1.13f,r))));
            }
            openingShade.wrapMode=openingVignette.wrapMode=TextureWrapMode.Clamp;
            openingShade.Apply(); openingVignette.Apply(); ownedAssets.Add(openingShade); ownedAssets.Add(openingVignette);
        }
        private Rect OpeningFrame(float width,float height)
        {
            float w=Mathf.Min(width,height*16/9); return new Rect((width-w)*.5f,(height-w*9/16)*.5f,w,w*9/16);
        }
        private void DrawOpeningPainting(Rect frame, OpeningBeat beat, float time, float opacity)
        {
            float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/beat.duration));
            float zoom=Mathf.Lerp(beat.zoomStart,beat.zoomEnd,t), extent=1/zoom;
            float x=Mathf.Clamp(beat.focus.x-extent*.5f,0,1-extent), y=Mathf.Clamp(beat.focus.y-extent*.5f,0,1-extent);
            GUI.color=new Color(1,1,1,opacity);
            GUI.DrawTextureWithTexCoords(frame,openingPaintings[beat.art],new Rect(x,y,extent,extent),true);
            GUI.color=Color.white;
        }
        private void DrawOpeningCinematic()
        {
            EnsureOpeningStyles(); Matrix4x4 previous=GUI.matrix; Color previousColor=GUI.color;
            float scale=Mathf.Max(.2f,Mathf.Min(Screen.width/1280f,Screen.height/720f));
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1)); GUI.color=Color.white;
            float w=Screen.width/scale,h=Screen.height/scale; Rect frame=OpeningFrame(w,h);
            HudRect(new Rect(0,0,w,h),Color.black);
            OpeningBeat beat=OpeningBeats[Mathf.Min(openingBeat,OpeningBeats.Length-1)];
            float dissolve=Mathf.SmoothStep(0,1,Mathf.Clamp01(openingBeatTime/.85f));
            if(openingPreviousBeat>=0) DrawOpeningPainting(frame,OpeningBeats[openingPreviousBeat],OpeningBeats[openingPreviousBeat].duration,1);
            DrawOpeningPainting(frame,beat,openingBeatTime,openingPreviousBeat<0?Mathf.Clamp01(openingBeatTime/1.2f):dissolve);
            DrawOpeningAtmosphere(frame,beat.art);
            GUI.DrawTexture(frame,openingVignette,ScaleMode.StretchToFill,true);
            GUI.DrawTexture(new Rect(frame.x,frame.yMax-210,frame.width,210),openingShade,ScaleMode.StretchToFill,true);
            HudRect(new Rect(frame.x,frame.y,frame.width,43),new Color(.015f,.025f,.035f,.38f));
            GUI.Label(new Rect(frame.x+34,frame.y+14,850,25),beat.chapter,openingHeadingStyle);
            GUI.Label(new Rect(frame.xMax-130,frame.y+15,100,25),(openingBeat+1).ToString("00")+" / "+OpeningBeats.Length.ToString("00"),openingSmallStyle);
            if(openingBeat==5)
            {
                float alpha=Mathf.Clamp01((openingBeatTime-.7f)/1.5f); GUI.color=new Color(1,1,1,alpha);
                var title=new Rect(frame.x+66,frame.y+frame.height*.27f,650,85);
                GUI.Label(new Rect(title.x+2,title.y+3,title.width,title.height),"Naki Legend",new GUIStyle(openingLogoStyle){normal={textColor=new Color(.03f,.06f,.09f,.65f)}});
                GUI.Label(title,"Naki Legend",openingLogoStyle); HudRect(new Rect(title.x,title.y+78,285,1),new Color(.94f,.81f,.55f,alpha));
                GUI.color=new Color(1,1,1,alpha); GUI.Label(new Rect(title.x,title.y+95,500,36),"风起之前，少年启程。",openingVoyageStyle); GUI.color=Color.white;
            }
            if(openingBeat==OpeningBeats.Length-1)
            {
                HudRect(frame,new Color(.02f,.04f,.06f,.43f));
                GUI.color=new Color(1,1,1,Mathf.Clamp01(openingBeatTime/.8f));
                GUI.Label(new Rect(frame.x+80,frame.center.y-42,frame.width-160,65),"第一岛 · 失去雨季的沙原",openingChapterStyle);
                GUI.Label(new Rect(frame.x+80,frame.center.y+24,frame.width-160,45),"愿长风携雨而来",openingCaptionStyle);GUI.color=Color.white;
            }
            else
            {
                int count=openingRevealAll?beat.caption.Length:Mathf.Clamp(Mathf.FloorToInt(openingBeatTime*24),0,beat.caption.Length);
                string caption=beat.caption.Substring(0,count);
                GUI.color=new Color(1,1,1,Mathf.Clamp01(openingBeatTime/.4f));
                GUI.Label(new Rect(frame.x+102,frame.yMax-158,frame.width-204,94),caption,openingCaptionStyle); GUI.color=Color.white;
            }
            GUI.Label(new Rect(frame.x+34,frame.yMax-37,750,24),openingPaused?"已暂停    空格 继续    ESC 跳过开场":"F 显示全文 / 下一幕    空格 暂停    ESC 跳过开场",openingSmallStyle);
            for(int i=0;i<OpeningBeats.Length;i++)
            {
                float unit=(frame.width-68)/OpeningBeats.Length;
                HudRect(new Rect(frame.x+34+i*unit,frame.yMax-11,unit-4,2),new Color(.75f,.72f,.66f,.25f));
                if(i<=openingBeat) HudRect(new Rect(frame.x+34+i*unit,frame.yMax-11,(unit-4)*(i<openingBeat?1:Mathf.Clamp01(openingBeatTime/beat.duration)),2),C("e9ca86"));
            }
            if(openingBeat==OpeningBeats.Length-1 && openingBeatTime>beat.duration-.75f)
                HudRect(new Rect(0,0,w,h),new Color(0,0,0,Mathf.InverseLerp(beat.duration-.75f,beat.duration,openingBeatTime)));
            GUI.matrix=previous;GUI.color=previousColor;
        }
        private void DrawOpeningAtmosphere(Rect frame,int art)
        {
            if(softDisc==null) return;
            for(int i=0;i<22;i++)
            {
                float seed=Mathf.Repeat(Mathf.Sin(i*47.17f+3)*437.73f,1);
                float x=Mathf.Repeat(seed+openingTotalTime*(art>=4?.003f:.0017f),1);
                float y=Mathf.Repeat(i*.137f-openingTotalTime*.006f,1);
                float size=art==1?2.5f:art==3?4:3.5f;
                float opacity=(.12f+.16f*Mathf.Sin(openingTotalTime+i))*(art==0?1:.6f);
                GUI.color=art==1?new Color(.55f,.76f,1,opacity):new Color(1,.85f,.6f,opacity);
                GUI.DrawTexture(new Rect(frame.x+x*frame.width,frame.y+y*frame.height,size,size),softDisc.texture);
            }
            GUI.color=Color.white;
        }
        private void DrawOpeningGameReveal()
        {
            if(openingGameReveal>0) HudRect(new Rect(0,0,Screen.width,Screen.height),new Color(0,0,0,openingGameReveal));
        }
        private void SetupOpeningAudio()
        {
            if(openingAmbience!=null) return;
            openingAmbience=gameObject.AddComponent<AudioSource>(); openingAmbience.playOnAwake=false; openingAmbience.loop=true; openingAmbience.spatialBlend=0;
            openingAccent=gameObject.AddComponent<AudioSource>(); openingAccent.playOnAwake=false; openingAccent.spatialBlend=0;
            openingSea=CreateOpeningAmbience(false); openingWind=CreateOpeningAmbience(true);
            int n=22050*3;var samples=new float[n];
            for(int i=0;i<n;i++){float t=i/22050f;samples[i]=Mathf.Sin(t*523.25f*Mathf.PI*2)*Mathf.Exp(-t*2.8f)*.13f+Mathf.Sin(t*1046.5f*Mathf.PI*2)*Mathf.Exp(-t*4.6f)*.035f;}
            openingBell=AudioClip.Create("Opening wind bell",n,1,22050,false); openingBell.SetData(samples,0);ownedAssets.Add(openingBell);
        }
        private AudioClip CreateOpeningAmbience(bool dry)
        {
            const int rate=22050;int count=rate*8;var samples=new float[count];uint seed=dry?917u:437u;float smooth=0;
            for(int i=0;i<count;i++)
            {
                seed=1664525u*seed+1013904223u;float noise=(seed/(float)uint.MaxValue)*2-1;
                smooth=Mathf.Lerp(smooth,noise,dry?.035f:.012f);float t=i/(float)rate;
                float envelope=Mathf.Pow(Mathf.Sin(Mathf.PI*i/(count-1)),2);
                float waves=.55f+.45f*Mathf.Sin(t*Mathf.PI*.5f);
                samples[i]=smooth*envelope*(dry?.44f:.8f)*waves;
            }
            AudioClip clip=AudioClip.Create(dry?"Opening dry wind":"Opening sea hush",count,1,rate,false);clip.SetData(samples,0);ownedAssets.Add(clip);return clip;
        }
        private void ApplyOpeningSoundscape()
        {
            if(openingAmbience==null) return;
            AudioClip target=OpeningBeats[openingBeat].art>=4?openingWind:openingSea;
            openingAmbience.volume=openingAudioVolume*audioMaster*audioEnvironment;openingAccent.volume=openingAudioVolume*audioMaster*audioEffects;
            if(openingAmbience.clip!=target){openingAmbience.Stop();openingAmbience.clip=target;}
            if(!openingAmbience.isPlaying)openingAmbience.Play();
            if(openingBeat==0||openingBeat==2||openingBeat==5||openingBeat==10)openingAccent.PlayOneShot(openingBell,.6f);
        }
    }
}
