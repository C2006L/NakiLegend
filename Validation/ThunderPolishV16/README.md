# 第二岛倒地与波次回归

`ThunderPolishSmoke.cs` 复制到独立 QA 工程的 Editor 目录，执行 `NakiThunderPolishV16Smoke.Run`。不放入正式玩家工程。

检查真实击杀与淡出、缩放和镜像、全岛普通 / 精英房的完整波次，以及保存后的清房状态。截图为同一房间三个敌人活着与倒地时的 Unity 原生画面；时间冻结只用于截图，战斗检查使用正常时间步进。

`smoke-result.txt` 为本轮结果；`chapter-regression-result.txt` 为本轮重新运行 V15 完整章节检查的结果。`deliver.py` 校验后只同步两个改动文件，记录于 `delivery.json`。
