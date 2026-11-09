# 归功外庭美术 V2

使用内置 **image_gen** 生成，保留原始图。提示词与约束保存于 [prompts.json](prompts.json)。前五岛与主角素材没有被替换。

| 文件 | 用途 |
|---|---|
| actors-source.png | 巡卫、盾卫、弩手、猎犬、誓录官、卫长，各 8 个动作 |
| extra-source.png | 斧骑士、锤骑士、暗黑祭师、弓箭手、暗黑法师，各 8 个动作 |
| environment-source.png | 灰石地面、墙体、粮车、石像、名册、门等道具 |
| harbor-source.png | 暮钟渡口底图；海、陆地、栈桥的实际碰撞另行设置 |
| spear-source.png | 誓庭卫枪，新武器 ID 132 |

生产切片在 `Assets/NakiPrototype/Resources/NakiArt/ManorCourtyardV2`。切片只裁取生成内容，保留 alpha；同一角色各动作使用相同格子、PPU 和脚部锚点。提取脚本为 `Validation/ManorCourtyardV2/prepare_assets.py`。

卫枪使用独立出枪与金属命中音频，来源和处理方式记录在 [audio-credits.json](audio-credits.json)。由项目已有 CC0 Foley 衍生，没有复制其他游戏声音。

游戏流程、操作和当前施工范围见 [MANOR_COURTYARD_V2.md](../../Docs/MANOR_COURTYARD_V2.md)。
