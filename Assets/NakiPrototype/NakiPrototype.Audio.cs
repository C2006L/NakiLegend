using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype
{
 public sealed partial class NakiPrototype
 {
  private AudioSource[] score,voices;
  private AudioSource ambience;
  private int scoreSide,audioVariant;
  private string musicKey,ambientKey;
  private readonly Dictionary<string,AudioClip> audioClips=new Dictionary<string,AudioClip>();
  private readonly Dictionary<string,float> audioCooldowns=new Dictionary<string,float>();
  private float audioMaster=.8f,audioMusic=.45f,audioEnvironment=.4f,audioEffects=.75f,nextFootstep;
  private bool audioSettingsMenu,audioWasSuspended,shieldWasRecovering;
  private AudioClip AudioAsset(string key)
  {
   AudioClip clip;if(!audioClips.TryGetValue(key,out clip)){clip=Resources.Load<AudioClip>("NakiAudio/"+key);audioClips[key]=clip;}return clip;
  }
  private AudioSource AudioBus(string name,bool loop)
  {
   GameObject obj=new GameObject(name);obj.transform.SetParent(transform,false);var source=obj.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;source.spatialBlend=0;source.volume=0;return source;
  }
  private void SetupSoundscape()
  {
   if(score!=null&&score.Length==2&&score[0]!=null)return;
   // Dedicated child buses are replaced together after an assembly reload.
   foreach(Transform child in transform)if(child.name.StartsWith("Naki audio "))Destroy(child.gameObject);
   audioClips.Clear();audioCooldowns.Clear();musicKey=ambientKey=null;
   score=new[]{AudioBus("Naki audio score A",true),AudioBus("Naki audio score B",true)};
   ambience=AudioBus("Naki audio environment",true);voices=new AudioSource[10];
   for(int i=0;i<voices.Length;i++){voices[i]=AudioBus("Naki audio voice "+i,false);voices[i].priority=i>=8?32:160;}
   audioMaster=PlayerPrefs.GetFloat("Naki.Audio.Master",.8f);audioMusic=PlayerPrefs.GetFloat("Naki.Audio.Music",.45f);audioEnvironment=PlayerPrefs.GetFloat("Naki.Audio.Environment",.4f);audioEffects=PlayerPrefs.GetFloat("Naki.Audio.Effects",.75f);
   audioWasSuspended=false;
  }
  private bool AudioSuspended {get{return paused||audioSettingsMenu||inventoryMenu||mapMenu||rewardMenu||chapterSkillsMenu||chapterRewardMenu||chapterSetup||thunderSetupMenu||ProfessionReady||OriginChoiceReady;}}
  private void TickSoundscape()
  {
   if(!enableDesertJourney)return;SetupSoundscape();
   bool suspended=AudioSuspended;
   if(suspended!=audioWasSuspended){foreach(AudioSource voice in voices){if(suspended)voice.Pause();else voice.UnPause();}audioWasSuspended=suspended;}
   string island=woodlandMode?"woodland":ThunderActive?"thunder":"desert";
   bool boss=false;foreach(Enemy enemy in enemies)if(enemy.boss&&enemy.hp>0)boss=true;
   bool combat=journeyRooms.Count>currentRoom&&currentRoom>=0&&!journeyRooms[currentRoom].cleared;
   bool ending=showRainCG||chapterCg||woodlandMode&&chapterEnding;
   string wanted=ending?"ending":island+(boss?"_boss":combat?"_combat":"_explore");
   if(!openingActive&&wanted!=musicKey)
   {
    AudioSource old=score[scoreSide];scoreSide=1-scoreSide;AudioSource next=score[scoreSide];next.Stop();next.clip=AudioAsset(wanted);next.volume=0;
    if(next.clip!=null){if(old.isPlaying&&old.clip!=null&&old.clip.length==next.clip.length)next.time=old.time%next.clip.length;next.Play();}musicKey=wanted;
   }
   float dt=Mathf.Min(Time.unscaledDeltaTime,.1f),duck=StoryActive?.35f:1;
   for(int i=0;i<2;i++){float target=i==scoreSide&&!openingActive&&!suspended?audioMaster*audioMusic*duck:0;score[i].volume=Mathf.MoveTowards(score[i].volume,target,dt*.35f);}
   string env=(!ThunderActive&&canalEpilogueComplete)?"water":island;
   if(env!=ambientKey){ambience.Stop();ambience.clip=AudioAsset("ambient_"+env);if(ambience.clip!=null)ambience.Play();ambientKey=env;}
   ambience.volume=Mathf.MoveTowards(ambience.volume,openingActive||suspended?0:audioMaster*audioEnvironment*(StoryActive?.5f:1),dt*.4f);
   // Opening sources use the same master/environment controls.
   if(openingAmbience!=null)openingAmbience.volume=openingAudioVolume*audioMaster*audioEnvironment;
   if(openingAccent!=null)openingAccent.volume=openingAudioVolume*audioMaster*audioEffects;
   foreach(AudioSource voice in voices)voice.volume=audioMaster*audioEffects;
  }
  private void Sfx(string key,bool priority=false,float gain=1)
  {
   if(!enableDesertJourney||voices==null||AudioSuspended||openingActive)return;
   float now=Time.unscaledTime,ready;if(audioCooldowns.TryGetValue(key,out ready)&&now<ready)return;
   int start=priority?8:0,end=priority?10:8,index=-1;
   for(int i=start;i<end;i++)if(!voices[i].isPlaying){index=i;break;}
   if(index<0){if(!priority)return;index=8;voices[index].Stop();}
   AudioClip clip=AudioAsset(key+"_"+(audioVariant++%3));if(clip==null)return;
   audioCooldowns[key]=now+(priority?.07f:.045f);var bus=voices[index];bus.volume=audioMaster*audioEffects;bus.pitch=1;bus.PlayOneShot(clip,Mathf.Clamp01(gain));
  }
  private void WeaponAudio()
  {
   WeaponDef w=CurrentWeapon;Sfx(w.type==WeaponType.Spear?"spear":w.type==WeaponType.Bow?w.name.Contains("弩")?"crossbow":"bow":w.type==WeaponType.Staff?"staff":w.damage>=26?"heavy":"sword",false,.75f);
  }
  private void HitAudio(Enemy e)
  {
   string material=woodlandMode?(e.kind==EnemyKind.CopperRoller||e.kind==EnemyKind.CopperWarden?"shell":"wood"):e.kind==EnemyKind.StoneSentinel||e.kind==EnemyKind.Colossus||e.kind==EnemyKind.ThunderStone?"stone":IsChapterBoss(e.kind)||e.kind==EnemyKind.CopperRoller?"shell":"flesh";
   Sfx(material,false,.65f);
  }
  private void AudioFootsteps(Vector2 input)
  {
   if(input.sqrMagnitude>.000001f&&clock>=dashUntil&&Time.unscaledTime>=nextFootstep){nextFootstep=Time.unscaledTime+.34f;Sfx(woodlandMode?"wood":"stone",false,.12f);}
  }
  private bool HandleAudioSettings()
  {
   if(PressedFunction10()){audioSettingsMenu=!audioSettingsMenu;SaveAudioSettings();}
   if(!audioSettingsMenu)return false;
   if(Pressed('e')){audioSettingsMenu=false;SaveAudioSettings();}return true;
  }
  private bool PressedFunction10()
  {
#if ENABLE_INPUT_SYSTEM
   return UnityEngine.InputSystem.Keyboard.current!=null&&UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
   return Input.GetKeyDown(KeyCode.F10);
#else
   return false;
#endif
  }
  private void SaveAudioSettings(){PlayerPrefs.SetFloat("Naki.Audio.Master",audioMaster);PlayerPrefs.SetFloat("Naki.Audio.Music",audioMusic);PlayerPrefs.SetFloat("Naki.Audio.Environment",audioEnvironment);PlayerPrefs.SetFloat("Naki.Audio.Effects",audioEffects);PlayerPrefs.Save();}
  private float VolumeSlider(Rect row,string title,float value)
  {
   FittedLabel(new Rect(row.x,row.y,200,30),title+" "+Mathf.RoundToInt(value*100)+"%",journeyTextStyle);return GUI.HorizontalSlider(new Rect(row.x+215,row.y+10,280,24),value,0,1);
  }
  private void DrawAudioSettings(float w,float h)
  {
   HudRect(new Rect(0,0,w,h),new Color(.015f,.025f,.04f,.94f));Rect box=new Rect(w/2-300,h/2-190,600,380);
   CenterLabel(new Rect(box.x,box.y,600,45),"声音设置",journeyTitleStyle);
   audioMaster=VolumeSlider(new Rect(box.x+40,box.y+65,520,35),"总音量",audioMaster);audioMusic=VolumeSlider(new Rect(box.x+40,box.y+120,520,35),"配乐",audioMusic);
   audioEnvironment=VolumeSlider(new Rect(box.x+40,box.y+175,520,35),"环境",audioEnvironment);audioEffects=VolumeSlider(new Rect(box.x+40,box.y+230,520,35),"武器与战斗",audioEffects);
   if(FlatButton(new Rect(w/2-120,box.y+295,240,50),"保存并返回 · F10 / ESC")){audioSettingsMenu=false;SaveAudioSettings();}
  }
 }
}
