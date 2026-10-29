# 配乐与挥剑重做

V2 替换此前的简易合成音乐，采用作者发布的完整 RPG 乐曲。参考方向是明快、可循环的地牢动作游戏节奏，配乐使用独立授权素材。

| 岛屿 | 探索 | 战斗 | Boss |
| --- | --- | --- | --- |
| 沙漠 | Heat Adventure | Heat Battle | Heat Boss Battle |
| 雷雨高原 | Heavy Dungeon | Heavy Battle 1 | Heavy Boss Battle 1 |
| 青栖林海 | Fairy Adventure | Fairy Battles | Fairy Boss Battles |

上述九首作品由 **MintoDog** 创作，以 CC0 发布，来源为 [作者 RPG 系列](https://opengameart.org/content/for-rpg-mintodogs-music)。完整乐曲约 81–144 秒，保留编排与立体声，进行响度匹配，不再使用此前的 32 秒波形循环。探索与战斗切换时淡入淡出，返回同一曲目从上次进度继续，避免不断重复前奏。

收尾曲使用 **Cleyton Kauffman — Forest Whisper Theme**，CC0。[作者发布页](https://opengameart.org/content/forest-whisper-theme)。

挥剑改用 **StarNinjas** 录制的刀剑攻击与碰撞素材，CC0。[作者发布页](https://opengameart.org/content/20-sword-sound-effects-attacks-and-clashes)。短剑挥空使用约 0.18 秒的短攻击声；命中甲壳或金属时叠加独立碰撞声。长矛、重剑采用该录音的速度变化版本。脚步响度降低，避免与挥剑声音混淆。

## 来源与授权记录

每首曲目、刀剑素材的作者、发布页、下载地址和原始文件 SHA-256 记录在 `Assets/NakiPrototype/Resources/NakiAudioV2/credits.json`。全部选用素材的发布页列出 CC0。保留作者署名以便追溯。声音来源不是项目原创；旧版 AUDIO_V1.md 中的“原创合成”描述只适用于 V1 资源。

导入脚本为 Tools/import_audio_v2.py。下载缓存与授权页面保存在工作区 References/AudioV2，缓存不提交 Git；游戏需要的音频和来源记录已随项目保存。解码使用工作区内的 FFmpeg，未修改系统 Python。

## 试听与测试

- Art/AudioV2/music-v2-audition.wav：三岛战斗音乐各六秒的拼接试听。
- Art/AudioV2/sword-v2-audition.wav：三次“挥空 + 命中”的独立试听。
- 上述为离线素材演示，游戏内按 F10 调节音量；原存档可直接继续。
- Unity 导入时音乐保留立体声并流式播放；短音效为单声道解压播放。
- 验证文件：Validation/AudioV2，覆盖资源时长、声道、播放状态选择、设置静音，以及三岛原有流程回归。主观听感仍需实玩反馈，数值检查不等于音质验收。
