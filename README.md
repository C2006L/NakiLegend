# Naki Legend

七岛冒险、弹幕战斗与剧情抉择。当前 Windows 试玩版：**v0.46.0**，包含七座岛屿、21 位主首领、暮钟岛与王都结局，以及独立存档的章节重玩和新旅程。

## 下载与游玩

在 [Releases](https://github.com/C2006L/NakiLegend/releases) 下载 Windows x64 压缩包。**完整解压后双击 `NakiLegend.exe`**，无需安装 Unity。请保留同目录的 `NakiLegend_Data`、`MonoBleedingEdge` 和 DLL，不能只单独发送 exe。

- WASD 移动，J 攻击，H 切换武器，K 职业战技。
- L / O / P 主属性技能，U / I 元素技能，T 管理技能。
- F 交互与继续剧情，B 行囊，M 地图，G 航图。
- ESC 暂停；清房后按 N 或点击暂停菜单“重玩与其他结局”。
- 1 / 2 使用补给，F5 / F9 存读安全点，F10 声音设置。
- 暂停菜单可退出，Alt+F4 也可关闭。

进度保存在本机用户目录。重玩本章或开启新旅程会创建独立存档分支；菜单可以返回上一轮进度。已达成的结局独立收录，不会因新旅程丢失。

## 本版内容

- 七座岛屿及王都终章连贯推进，首领战前需完成对应调查。
- 暮钟岛战后提供登船返航互动，直接衔接王都。
- 王都文书集中为一册连续阅读，删去重复技能桌；首领加强地面封路，保留移动收招窗口。
- 支持返回最终抉择前、重新体验当前章节、从第一岛开始新旅程、回到上一轮进度。
- 修复普通房间暂停时残留技能特效阻挡重玩入口的问题。

[完整操作与修订说明](Docs/PLAY_GUIDE_V18.md) · [150 字游戏简介](Docs/GAME_INTRO_150.txt) · [历史开发记录](Docs/DEVELOPMENT_HISTORY_PRE_V46.md)

## Unity 工程与打包

Unity **2022.3.62f3**，安装 Windows Build Support。仓库本身是 Unity 工程，使用 Unity Hub 打开根目录。运行 `Assets/Scenes/NakiTest.unity` 从头体验；`Tools → Naki Legend` 提供章节和首领练习入口。

选择 `Tools → Naki Legend → Build → Windows portable` 导出免安装 Windows x64 版本。命令行：

```powershell
Unity.exe -batchmode -projectPath <工程目录> -executeMethod NakiLegend.Prototype.Editor.NakiWindowsBuild.Build -nakiOutput <导出目录> -logFile <日志路径>
```

构建器生成干净的第一岛入口，不带开发者测试存档或首领试炼配置。运行时资源位于 `Assets/NakiPrototype/Resources`。请勿提交 Library、Temp 或游戏运行存档。

第三方素材说明见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)，字体的 OFL 原文随项目和发行包附带。
