using System.Collections.Generic;
using UnityEngine;
namespace NakiLegend.Prototype {
 public sealed partial class NakiPrototype {
  private AudioSource[] scorePressure;
  private float[] voiceGains,voiceStarted;
  private string[] voiceKeys;
  private float soundDuckUntil;
  private string soundLetterState;private bool soundWasNarrative;
  private sealed class ActorSound {public int attack,phase;public Vector2 at;public float stepAt,volleyAt,beamAt;public bool resting;}
  private readonly Dictionary<Enemy,ActorSound> soundActors=new Dictionary<Enemy,ActorSound>();
  private bool AudioReading {get{return capitalMode&&capitalLetter>=0||manorCourtyardMode&&manorLetter>=0;}}
  private bool AudioNarrative {get{return StoryActive||AudioReading||capitalMode&&(capitalCg>=0||capitalConfirm>0||CapitalTerminal)||manorCourtyardMode&&(manorIntro||manorVictoryCg)||showRainCG||chapterCg;}}
  private string SoundRegion {get{return capitalMode?"capital":manorCourtyardMode?"manor":woodlandMode?"woodland":fireChapterMode?"fire":iceSampleMode?"ice":ThunderActive?"thunder":"desert";}}
  private Enemy SoundBoss {get{foreach(Enemy e in enemies)if(e.boss&&e.hp>0)return e;return null;}}
  private string WantedScore {
   get {
    string region=SoundRegion;
    if(health<=0)return "defeat";
    if(capitalMode&&capitalCg>=0)return capitalCg==0?"capital_return":capitalCg==3?"capital_victory":capitalCg==4?"ending_keep":capitalCg==5?"ending_rebirth":capitalCg==6?"defeat":"capital_story";
    if(capitalMode&&chapterEnding)return capitalEnding==2?"ending_rebirth":"ending_keep";
    if(AudioReading||manorCourtyardMode&&(manorIntro||manorVictoryCg))return region+"_story";
    if(showRainCG||chapterCg||chapterEnding)return "ending";
    if(SoundBoss!=null)return region+"_boss";
    if(pendingBossClear&&musicKey!=null)return musicKey;
    bool combat=false;foreach(Enemy e in enemies)if(e.hp>0&&!CombatFixture(e)){combat=true;break;}
    if(!combat&&currentRoom>=0&&currentRoom<journeyRooms.Count)combat=IsCombatRoom(journeyRooms[currentRoom])&&!journeyRooms[currentRoom].cleared&&(pendingSpawns.Count>0||currentWave>0);
    return region+(combat?"_combat":"_explore");
   }
  }
  private bool PlaySoundCue(string key,int lane,float gain,bool variants,Vector2? at=null){
   if(voices==null||AudioSuspended||openingActive)return false;
   float now=Time.unscaledTime,ready;if(audioCooldowns.TryGetValue(key,out ready)&&now<ready)return true;
   AudioClip clip=AudioAsset(variants?key+"_"+(audioVariant++%3):key);if(clip==null)return false;
   int[] starts={0,8,14,22,26,30},ends={8,14,22,26,30,32};int first=starts[lane],last=ends[lane],index=-1,duplicates=0;float oldest=float.PositiveInfinity;
   if(lane==4){if(IsPlayerDamageCue(key))last=29;else first=29;}
   for(int i=first;i<last;i++){if(voices[i].isPlaying&&voiceKeys[i]==key)duplicates++;if(!voices[i].isPlaying&&index<0)index=i;}
   if(duplicates>=2&&lane<3)return true;
   if(index<0){if(lane<2)return true;for(int i=first;i<last;i++)if(voiceStarted[i]<oldest){oldest=voiceStarted[i];index=i;}voices[index].Stop();}
   float cooldown=key=="skill_starfall_hit"?.09f:key=="skill_solar_pulse"?.12f:lane==2?.085f:lane==3?.16f:lane==4?.08f:lane==1?.07f:.05f;
   audioCooldowns[key]=now+cooldown;voiceKeys[index]=key;voiceStarted[index]=now;voiceGains[index]=Mathf.Clamp01(gain);
   AudioSource bus=voices[index];bus.clip=clip;bus.pitch=lane<=1?1+(audioVariant%3-1)*.018f:1;
   bus.panStereo=at.HasValue?Mathf.Clamp((at.Value.x-position.x)/12,-.6f,.6f):0;bus.volume=audioMaster*audioEffects*voiceGains[index];bus.Play();
   if(IsPlayerDamageCue(key)){float duck=key=="break"?.38f:key=="hurt"?.5f:.7f;damageFeedbackDuck=now<damageFeedbackUntil?Mathf.Min(damageFeedbackDuck,duck):duck;damageFeedbackUntil=Mathf.Max(damageFeedbackUntil,now+(key=="break"?.42f:key=="hurt"?.24f:.16f));}
   if(lane==4||lane==3&&gain>.55f)soundDuckUntil=now+.3f;return true;
  }
  private void SwitchSoundScore(string key){
   AudioClip clip=AudioAsset(key);if(clip==null)return;
   AudioSource old=score[scoreSide];if(musicKey!=null&&old.clip!=null)scorePositions[musicKey]=old.time;
   scoreSide=1-scoreSide;AudioSource next=score[scoreSide],pulse=scorePressure[scoreSide];next.Stop();pulse.Stop();next.clip=clip;next.volume=0;pulse.volume=0;
   float resume;if(scorePositions.TryGetValue(key,out resume))next.time=Mathf.Repeat(resume,clip.length);
   double when=AudioSettings.dspTime+.06;next.PlayScheduled(when);
   pulse.clip=key.EndsWith("_boss")?AudioAsset(SoundRegion+"_pressure"):null;
   if(pulse.clip!=null){pulse.time=Mathf.Repeat(next.time,pulse.clip.length);pulse.PlayScheduled(when);}musicKey=key;
  }
  private void TickSoundV25(){
   if(!enableDesertJourney)return;SetupSoundscape();bool suspended=AudioSuspended;
   if(suspended!=audioWasSuspended){foreach(AudioSource bus in voices){if(suspended)bus.Pause();else bus.UnPause();}foreach(AudioSource bus in score){if(suspended)bus.Pause();else bus.UnPause();}foreach(AudioSource bus in scorePressure){if(suspended)bus.Pause();else bus.UnPause();}if(suspended)ambience.Pause();else ambience.UnPause();audioWasSuspended=suspended;}
   if(!suspended&&!openingActive&&WantedScore!=musicKey)SwitchSoundScore(WantedScore);
   bool narrative=AudioNarrative;if(narrative&&!soundWasNarrative)for(int i=0;i<26;i++)voices[i].Stop();soundWasNarrative=narrative;
   UpdateOriginAudio();float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);Enemy boss=SoundBoss;bool rest=boss!=null&&RecoveryWindow(boss);
   float duck=AudioNarrative?.42f:Time.unscaledTime<soundDuckUntil?.64f:rest?.77f:1;
   for(int i=0;i<2;i++){
    float target=i==scoreSide&&!openingActive&&!suspended?audioMaster*audioMusic*duck:0;
    score[i].volume=Mathf.MoveTowards(score[i].volume,target,dt*.4f);
    float pressure=boss==null||rest||AudioNarrative?0:boss.difficultyPhase==2?.7f:boss.difficultyPhase==1?.4f:.12f;
    scorePressure[i].volume=Mathf.MoveTowards(scorePressure[i].volume,target*pressure,dt*.35f);
   }
   string env=AudioReading?"study":SoundRegion=="desert"&&!ThunderActive&&canalEpilogueComplete?"water":SoundRegion;
   if(!suspended&&env!=ambientKey){ambience.Stop();ambience.clip=AudioAsset("ambient_"+env);if(ambience.clip!=null)ambience.Play();ambientKey=env;}
   ambience.volume=Mathf.MoveTowards(ambience.volume,openingActive||suspended?0:audioMaster*audioEnvironment*(AudioNarrative?.55f:musicKey!=null&&(musicKey.EndsWith("_combat")||musicKey.EndsWith("_boss"))?.48f:1),dt*.4f);
   float load=0;for(int i=0;i<22;i++)if(voices[i].isPlaying)load+=voiceGains[i];float crowd=Mathf.Min(1,3/Mathf.Max(3,load));
   for(int i=0;i<voices.Length;i++)voices[i].volume=audioMaster*audioEffects*voiceGains[i]*(i<22?crowd*(Time.unscaledTime<damageFeedbackUntil?damageFeedbackDuck:1):1);
   if(openingAmbience!=null)openingAmbience.volume=openingAudioVolume*audioMaster*audioEnvironment;if(openingAccent!=null)openingAccent.volume=openingAudioVolume*audioMaster*audioEffects;
   string letter=capitalMode&&capitalLetter>=0?"c"+capitalLetter+":"+capitalPage:manorCourtyardMode&&manorLetter>=0?"m"+manorLetter+":"+manorLetterPage:null;
   if(letter!=soundLetterState){if(!suspended&&(letter!=null||soundLetterState!=null))PlaySoundCue("paper_turn",5,.45f,true);soundLetterState=letter;}
   if(soundActors.Count>enemies.Count+12){var dead=new List<Enemy>();foreach(var pair in soundActors)if(pair.Key.hp<=0||!enemies.Contains(pair.Key))dead.Add(pair.Key);foreach(Enemy e in dead)soundActors.Remove(e);}
  }
  private ActorSound SoundState(Enemy e){ActorSound s;if(!soundActors.TryGetValue(e,out s)){s=new ActorSound{at=e.position,attack=e.attackNumber,phase=e.difficultyPhase};soundActors[e]=s;}return s;}
  private string BossSoundFamily(Enemy e){if(e.kind==EnemyKind.CapitalKing)return "royal";if(IsCapitalEnemy(e.kind)||manorCourtyardMode)return "armor";return woodlandMode?"insect":fireChapterMode?"forge":iceSampleMode?"ice":ThunderActive?"storm":"stone";}
  private void TickEnemySound(Enemy e){
   if(!e.boss||voices==null||AudioSuspended||AudioNarrative||openingActive)return;ActorSound s=SoundState(e);float now=Time.unscaledTime;
   if(e.attackNumber!=s.attack){s.attack=e.attackNumber;if(e.warning!=null||e.chargeUntil>clock)PlaySoundCue("boss_"+BossSoundFamily(e)+"_tell",3,.64f,true,e.position);}
   if(e.difficultyPhase>s.phase){s.phase=e.difficultyPhase;PlaySoundCue("boss_"+BossSoundFamily(e)+"_phase",3,.75f,true,e.position);}
   bool rest=RecoveryWindow(e);if(rest&&!s.resting)PlaySoundCue("boss_recover",3,.38f,true,e.position);s.resting=rest;
   if((e.position-s.at).sqrMagnitude>.00002f&&e.chargeUntil<=clock&&now>=s.stepAt){s.stepAt=now+.45f;PlaySoundCue("boss_"+BossSoundFamily(e)+"_step",3,.26f,true,e.position);}s.at=e.position;
  }
  private void HostileVolleyAudio(Enemy e,float speed,Vector2 at){
   if(e==null||voices==null||AudioSuspended||AudioNarrative||!EnemyOnScreen(at,.3f))return;ActorSound s=SoundState(e);float now=Time.unscaledTime;if(now<s.volleyAt)return;s.volleyAt=now+(e.boss?.3f:.7f);
   PlaySoundCue(speed>=5.3f?"enemy_fast":"enemy_slow",e.boss?3:0,e.boss?.28f:.18f,true,at);
  }
  private void BeamContactAudio(SignatureZone z,Vector2 end,bool blocked){
   if(AudioSuspended||AudioNarrative||clock<z.active+.16f||clock>=z.end)return;ActorSound s=SoundState(z.owner);if(Time.unscaledTime<s.beamAt)return;s.beamAt=Time.unscaledTime+.48f;
   PlaySoundCue(blocked?"beam_stone":"enemy_beam",3,blocked?.38f:.3f,true,blocked?end:z.at);
  }
 }
}
