# 火岛第一层 Unity 原生验证

这是第一阶段历史结果。当前两层实现与完整原生检查见 `Validation/FireWorkshop/README.md`；当前构建脚本已适配 30 房。

`final-unity.log` 与 `runtime/smoke-result.txt`：**PASS 1183**。本轮新增 Fire 专项用例，连同沙漠、雷岛、森林、冰岛与弓弩既有回归一起运行。日志与实际截图位于忽略提交的 runtime 目录。

检查覆盖 15 房完整图连通、门口安全落点与港口出口全宽换房、每房所有波次、避难所战斗后绞盘、首领前救援门锁、冰击和普通武器破壳、半血 9→13 发扇射、延迟煤包伤害与拆除、清房消除危险、火种消耗/CD/被墙阻挡不扣能量、有限燃烧、学习条件、重复交互保留技能槽、独立存档与 .bak、反复读档不叠护盾、光暗资源、冰火两岛实际往返与装备继承、练习不覆盖正式进度。

`fire-regression.log`：后续聚焦 Fire 的 PASS 116。最终完整运行重新确认 1183 项通过，运行时未捕获 Error / Exception / Assert。

构建：`python Validation/FireCoast/build.py`。它从已存在的 IceChapter harness 增量派生用例并准备隔离 QA 工程，不修改用户的游玩场景。Unity 入口 `NakiFireCoastSmoke.Run`；聚焦入口 `RunFireRegression`。不要用 `-quit` 抢先退出；harness 完成后主动退出。

跨岛存档测试启用 batch 专用目录 `runtime/save/cross-island`，不读取或写入玩家自己的实际存档。场景由 Unity 自身序列化生成，保留主脚本 GUID。

`finalize_assets.py` 回收 Unity 导入与序列化产生的元数据、新场景；`audit_sync.py` 核对实际工程与分发目录的差异；`sync_runtime.py` 只同步核对过的脚本、新素材和新 Fire 场景，先备份旧文件，逐字节验证。用户原有场景不覆盖。

已目视检查：`fire-harbor.png`、`fire-encounter.png`、`red-spine-lizard.png`、`training-forge.png`、`fire-skills.png`。敌人无相邻主体串图，文字和上下状态栏保持在战斗视口之外。
