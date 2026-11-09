"""Author the Manor's reviewed room graph and story encounter specification."""
from pathlib import Path
import json
root=Path(__file__).resolve().parents[2]
out=root/'Assets/NakiPrototype/Resources/NakiArt/ManorV1'
names=[
 ['暮钟渡口','封岸石径','旧营地','断旗岗哨','巡卫武库','纪念庭入口','誓盾庭','旧委任书','马车窄道','钟楼阶下','粮道门前','战前整备庭','誓庭卫长','解锁粮道','归功旧阶'],
 ['旧村施粥屋','扣押粮道','封存粮仓','链锁廊道','药物转运间','旧卫巡房','掌印间','封印档案室','工坊侧廊','冷却备料间','淬炼回路','工坊整备台','掌印机关','解除村门','内庭阶梯'],
 ['受封回廊','废弃宴会厅','武备夹道','未寄出的家书','旧航海室','残誓长廊','契约柱廊','暮钟对质','无昼侧庭','安息室','钟声断廊','最后整备间','暗黑大骑士','契约的代价','归航与终局']]
roles=[
 ('巡卫','褪色蓝披肩、旧制短剑','两段斩击，第二斩前停顿','生物，可获得残响'),
 ('誓盾卫','旧方盾与封蜡纹','正面减伤，转身慢，侧后可打','生物，可获得残响'),
 ('钟楼弩手','半面盔与机械弩','锁定瞄准线、三连矢、装填换位','生物，可获得残响'),
 ('缚誓猎犬','断链、旧军犬护具','低伏预警、锁向直扑、扑空收招','生物，可获得残响'),
 ('掌印执役','长袍与印器','为同伴施护印，可被打断','生物，可获得残响'),
 ('锁链狱卒','铁钩与钥匙','钩地封路，保留侧向出口','生物，可获得残响'),
 ('淬炼傀儡','空甲、人工紫石容器','慢行与环弹，击中胸核可打断','构造物，不提供灵魂'),
 ('残誓回声','断续人影与细碎光纹','预警后显形攻击，显形时清晰可伤','残响，不可刷灵魂')]
safe={0,4,7,11,14};rooms=[]
for floor in range(3):
 for local,title in enumerate(names[floor]):
  ident=floor*15+local;kind='Boss' if local==12 else 'Reward' if local in {4,9} else 'Rest' if local in {0,11,14} else 'Event' if local in {7,13} else 'Elite' if local in {6,10} else 'Combat'
  if floor==1 and local in {2,4}:kind='Event'
  if floor==2 and local in {3,4,9}:kind='Event'
  combat=kind in {'Boss','Combat','Elite'}
  pool=['巡卫','誓盾卫','钟楼弩手','缚誓猎犬'] if floor==0 else ['巡卫','钟楼弩手','掌印执役','锁链狱卒','淬炼傀儡'] if floor==1 else ['誓盾卫','钟楼弩手','掌印执役','锁链狱卒','淬炼傀儡','残誓回声']
  groups=[]
  if combat and kind!='Boss':
   for wave in range(4 if kind=='Combat' else 5):
    group=[pool[(local+wave)%len(pool)],pool[(local+wave+1)%len(pool)],pool[(local+wave+2)%len(pool)]]
    groups.append('、'.join(group)+f'；合计 {6+floor+(wave%2)} 名，分批到场，远程同时不超过 3 名')
  rooms.append(dict(id=ident,stage=floor,x=local%5,y=2-local//5,title=title,kind=kind,waves=groups,links=[],stairsTo=ident+15 if local==14 and floor<2 else ident-15 if local==14 and floor>0 else -1,objective='清理道路，沿岸上门廊继续探索' if combat else '整理物资，调查环境后继续前进',requires='',grants='',story=[],npc='',boss=''))
# Adjacent loops with a single boss access. No main-route gate is dependent on a side branch.
for n in rooms:
 local=n['id']%15
 for other in rooms[n['stage']*15:n['stage']*15+15]:
  if abs(n['x']-other['x'])+abs(n['y']-other['y'])!=1:continue
  a,b=sorted([local,other['id']%15])
  if b==12 and a!=7 or a==12 and b!=13:continue
  if b==14 and a!=13:continue
  n['links'].append(other['id'])
 # Explicit stairs are paired independently of floor coordinates.
 n['stairsTo']=15 if n['id']==14 else 14 if n['id']==15 else 30 if n['id']==29 else 29 if n['id']==30 else -1
def at(i,**changes):rooms[i].update(changes)
at(0,objective='沿石岸登岛，找到纪念庭的旧委任',story=['暮钟响过三次，岸上却没有迎船的人。','船夫：这座岛以前收留过很多人。现在，船只准进，不准出。'])
at(7,objective='核对纪念像、佩剑编号与委任书',grants='commission',story=['石碑上的勇者，佩着与你相似的短剑。','旧委任书末尾写着：等最后一块碎片取回，我就回去。'])
at(12,boss='誓庭卫长',requires='commission',grants='outer_guard',objective='破盾、躲避突刺，打开粮道')
at(13,requires='outer_guard',objective='打开粮道铁门，沿旧阶进入村庄')
at(15,npc='暮钟医师',objective='在安全屋了解药粮缺口',story=['医师：他以前把自己的口粮留给这里。后来，他开始要求我们证明，自己值得被留下。'])
at(17,npc='守粮人',objective='清除粮仓外封锁后，将粮车送回施粥屋',grants='grain',story=['守粮人：粮食就在这里。是门不让我们把它送出去。'])
at(19,npc='药物转运员',objective='检查与霜潮群岛药单相符的药箱',grants='medicine',story=['药箱还带着霜潮港的船签，接驳纹上压着王室封蜡。'])
at(22,requires='grain',grants='identity',objective='核对出海记录、受封画像与人工容器图',story=['画像右眼被反复涂黑，佩剑编号却没有改过。','淬炼图注：战斗残响并非完整人格。完整淬炼将夺走原主的意志。','少年：原来住在这里的，一直是上一位出海的人。'])
at(27,boss='掌印机关·缄令者',requires='identity',grants='workshop',objective='破坏刻印臂，解除村庄封锁')
at(28,requires='workshop',npc='暮钟医师',objective='确认居民能离开村庄，再进入内庭')
at(33,grants='letter',objective='读未寄出的家书',story=['家书：他们说只要再强一些，就不会再有人死在我面前。'])
at(34,objective='查看前五岛留下的遗物与求援信',story=['修渠拓印、旧巡路簿、没有返乡票的船单、北方药粮单与森林旧信，落款出自同一人。'])
at(37,requires='identity',grants='confrontation',objective='与大骑士对质，再进入最后整备间',story=['大骑士：他这一次，还是让一个孩子出海。','少年：你说的是国王？','大骑士：我也曾以为，只要把碎片带回去，一切就会结束。','大骑士：我也曾是屠魔的少年。后来，我披上了魔的铠甲，他们却仍叫我救世主。'])
at(41,objective='保存独立战前恢复点，整理装备与技能；可回访旧村')
at(42,boss='暗黑大骑士',requires='confrontation',grants='knight_defeated',objective='击破誓甲与人工契约外壳，保留全部既有储能')
at(43,requires='knight_defeated',objective='光系拆解契约；暗系明确选择自己承担或将代价转嫁居民',story=['一人份指完整意志，不是把暗能余额改成一点。','少年：这里还有一人份。','大骑士：你知道那意味着什么。','少年：这一次，不必再从别人身上取了。'])
at(44,objective='播放对应结尾，光线带证据返乡；暗线保留独立战前恢复点')
data=dict(version=1,status='blueprint_not_playable',title='暮钟岛 大骑士府邸',floors=['归功外庭','断粮旧村与淬炼工坊','无昼内庭'],opening='NakiArt/ManorV1/opening',rooms=rooms,enemies=[dict(name=a,appearance=b,combat=c,soul=d) for a,b,c,d in roles],endings=[dict(name='归途尚未结束',route='光',description='取回真正暗碎片与契约原令，村民点亮渡口，少年带证据返乡'),dict(name='一人份的贤者之石',route='暗 承担',description='以自己的完整意志阻断接替；紫光熄灭，家书留在手中'),dict(name='新任大骑士',route='暗 转嫁',description='居民避难屋失去回应，新铠甲封住少年，新的王室委任送达')])
out.mkdir(parents=True,exist_ok=True);(out/'blueprint.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
print('Authored 45 rooms, adjacent loops, paired stairs, 8 enemy specifications, story and 3 ending branches')
