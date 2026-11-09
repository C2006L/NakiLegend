from pathlib import Path
root=Path(__file__).resolve().parents[1]/'Assets/NakiPrototype'
def edit(file,old,new):
 p=root/file;s=p.read_text(encoding='utf-8-sig');assert old in s,(file,old[:90]);p.write_text(s.replace(old,new),encoding='utf-8')
edit('NakiPrototype.Audio.cs','private bool skillImpactActive;','private bool skillImpactActive;private float lightImpactSoundReady;')
edit('NakiPrototype.Audio.cs','private void SkillSound(string skill,string phase){float gain=','private void SkillSound(string skill,string phase){if(skill=="lightbeam"&&phase=="hit"){if(clock<lightImpactSoundReady)return;lightImpactSoundReady=clock+.18f;}float gain=skill=="lightbeam"?(phase=="hit"?.32f:.55f):')
edit('NakiPrototype.PixelHero.cs','else if (clock < castPoseUntil) { action = HeroAction.Cast;','else if(OriginActionActive&&movement.sqrMagnitude>.01f){action=HeroAction.Walk;if(currentHeroAction!=HeroAction.Walk)polishedWalkStarted=clock;phase=polishedHeroReady?Mathf.Repeat((clock-polishedWalkStarted)*1.6f,1):Mathf.Repeat(clock*2,1);aim=castDirection;}\n            else if (clock < castPoseUntil) { action = HeroAction.Cast;')
edit('NakiPrototype.Equipment.cs','if(action==HeroAction.Cast || action==HeroAction.Dodge || bowCharging)','if(action==HeroAction.Cast || action==HeroAction.Dodge || bowCharging || OriginActionActive)')
edit('NakiPrototype.cs','UpdateIllustratedHero(input);if(enableDesertJourney)AudioFootsteps','UpdateIllustratedHero(input);if(lightChannel)RenderLightBeam(false,0);if(enableDesertJourney)AudioFootsteps')
edit('NakiPrototype.Journey.cs','nextWaveAt=-1,journeyEnergyRegen=6;','nextWaveAt=-1;')
edit('NakiPrototype.Elements.cs','hintReadyAt = 0; journeyEnergyRegen = 6;','hintReadyAt = 0;')
print('Attached light to the hand, retained walking animation while channeling and softened repeated impact audio')
