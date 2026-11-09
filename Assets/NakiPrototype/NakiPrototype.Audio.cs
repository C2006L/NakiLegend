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
  private readonly Dictionary<string,float> scorePositions=new Dictionary<string,float>();
  private float audioMaster=.8f,audioMusic=.45f,audioEnvironment=.4f,audioEffects=.75f,nextFootstep;
  private bool audioSettingsMenu,audioWasSuspended,shieldWasRecovering;
  private AudioClip AudioAsset(string key)
  {
   AudioClip clip;if(!audioClips.TryGetValue(key,out clip)){clip=Resources.Load<AudioClip>("NakiSoundV25/"+key)??Resources.Load<AudioClip>("NakiCombatAudioV3/"+key)??(key.StartsWith("skill_")?Resources.Load<AudioClip>("NakiSkills/"+key):null)??Resources.Load<AudioClip>("NakiAudioV2/"+key)??Resources.Load<AudioClip>("NakiAudio/"+key);audioClips[key]=clip;}return clip;
  }
  private AudioSource AudioBus(string name,bool loop)
  {
   GameObject obj=new GameObject(name);obj.transform.SetParent(transform,false);var source=obj.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;source.spatialBlend=0;source.volume=0;return source;
  }
  private void SetupSoundscape()
  {
   if(score!=null&&score.Length==2&&score[0]!=null&&scorePressure!=null&&voices!=null&&voices.Length==32&&voiceGains!=null)return;
   // Dedicated child buses are replaced together after an assembly reload.
   foreach(Transform child in transform)if(child.name.StartsWith("Naki audio "))Destroy(child.gameObject);
   audioClips.Clear();audioCooldowns.Clear();scorePositions.Clear();musicKey=ambientKey=null;
   score=new[]{AudioBus("Naki audio score A",true),AudioBus("Naki audio score B",true)};
   scorePressure=new[]{AudioBus("Naki audio pressure A",true),AudioBus("Naki audio pressure B",true)};
   ambience=AudioBus("Naki audio environment",true);voices=new AudioSource[32];
   voiceGains=new float[32];voiceKeys=new string[32];voiceStarted=new float[32];
   for(int i=0;i<voices.Length;i++){voices[i]=AudioBus("Naki audio voice "+i,false);voices[i].priority=i>=26?16:i>=22?32:i>=14?64:i>=8?100:160;}
   soundActors.Clear();soundLetterState=null;soundWasNarrative=false;soundDuckUntil=0;originChannelAudio=null;
   audioMaster=PlayerPrefs.GetFloat("Naki.Audio.Master",.8f);audioMusic=PlayerPrefs.GetFloat("Naki.Audio.Music",.45f);audioEnvironment=PlayerPrefs.GetFloat("Naki.Audio.Environment",.4f);audioEffects=PlayerPrefs.GetFloat("Naki.Audio.Effects",.75f);
   audioWasSuspended=false;
  }
  private bool AudioSuspended {get{return voyagerJournal||worldAtlasMenu||challengeMenu||originAbilityMenu||pendingIceVoyage>=0||talentMenu||paused||audioSettingsMenu||inventoryMenu||mapMenu||rewardMenu||chapterSkillsMenu||chapterRewardMenu||chapterSetup||thunderSetupMenu||ProfessionReady||OriginChoiceReady;}}
  private void TickSoundscape(){TickSoundV25();}
  private void Sfx(string key,bool priority=false,float gain=1,int group=0)
  {
   if(!enableDesertJourney||AudioSuspended||AudioNarrative||openingActive)return;SetupSoundscape();
   int lane=key=="hurt"||key=="shield"||key=="break"||key=="death"||key=="warrior_dash"||key=="dodge"?4:key=="warn"||key.StartsWith("boss_")?3:group==1?2:0;
   PlaySoundCue(key,lane,gain,true);
  }
  private bool skillImpactActive;private float lightImpactSoundReady;
  private void SkillSound(string skill,string phase)
  {
   if(skill=="lightbeam"&&phase=="hit"){if(clock<lightImpactSoundReady)return;lightImpactSoundReady=clock+.18f;}
   float gain=skill=="lightbeam"?(phase=="hit"?.4f:.65f):skill=="meteor"?.64f:skill=="soulseal"?.78f:.78f;
   Sfx("skill_"+skill+"_"+phase,false,gain,1);
  }
  private void ApplySkillHit(Enemy target,float amount,Element element,string skill)
  {
   SkillSound(skill,"hit");bool previous=skillImpactActive;skillImpactActive=true;
   try{ApplyElementDamage(target,amount,element);if(skill=="spore"&&target.hp>0){target.woodPoisonUntil=clock+3;target.woodPoisonNext=Mathf.Max(target.woodPoisonNext,clock+.7f);}}finally{skillImpactActive=previous;}
  }
  private void WeaponAudio()
  {
   if(PlayWeaponIdentitySound(HeldWeaponIdentity,false))return;WeaponDef w=CurrentWeapon;string key=IsHammer?"hammer":w.type==WeaponType.Spear?"spear":w.type==WeaponType.Bow?w.name.Contains("弩")?"crossbow":"bow":w.type==WeaponType.Staff?w.element==Element.Lightning?"staff_electric":"staff":w.damage>=26?"heavy":"sword";Sfx(key,false,.75f);
  }
  private void HitAudio(Enemy e)
  {
   if(skillImpactActive||weaponImpactSource>=0)return;if(IsHammer){Sfx("hammer_hit",false,.7f);return;}
   string material=woodlandMode?(e.kind==EnemyKind.CopperRoller||e.kind==EnemyKind.CopperWarden?"shell":"wood"):e.kind==EnemyKind.FrostLens||e.kind==EnemyKind.StoneSentinel||e.kind==EnemyKind.Colossus||e.kind==EnemyKind.ThunderStone?"stone":IsChapterBoss(e.kind)||e.kind==EnemyKind.CopperRoller?"shell":"flesh";
   bool melee=CurrentWeapon.type==WeaponType.Sword||CurrentWeapon.type==WeaponType.Spear;
   Sfx(material=="shell"&&melee?"clash":material,false,.65f);
  }
  private void AudioFootsteps(Vector2 input)
  {
   if(input.sqrMagnitude>.000001f&&clock>=dashUntil&&Time.unscaledTime>=nextFootstep){nextFootstep=Time.unscaledTime+.34f;Sfx(woodlandMode?"wood":"stone",false,.06f);}
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
