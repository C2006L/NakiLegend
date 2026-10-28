# V15 验证

使用 Unity 2022.3.62f3 / URP 的独立 QA 工程执行，未在用户正在游玩的场景中写入测试数据。

- `ExperienceSmoke.cs`：新增剧情、光暗资源、修渠前置条件、章节成长和界面实际字形检查；导出 HD、QHD、4:3 实际画面。
- `ChapterRegression.cs`：第二岛完整回归，按 V15 的技能伤害与 Boss 生命校验。
- `experience-result.txt`、`chapter-result.txt`：本轮原生结果。
- `reload-result.txt`：复用真实脚本重载验证，包含暗系存档和动画缓存。
- 其余 `*-regression-result.txt`：本轮重新执行原有沙漠、修渠、两房原型与地图验证。
- `compile-result.txt`：独立 C# 编译结果。
- `deliver.py`、`delivery.json`：同步至实际 Unity 工程前校验目标文件，保留场景、美术与玩家存档。

运行时 JSON、临时截图及日志不提交；用于阅读的截图收在 `Art/ExperienceV15`。测试用 Editor 脚本不放入正式游戏工程。
