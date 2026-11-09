# 锻链工坊原生验证

最终运行 `final-unity.log`、`runtime/smoke-result.txt`：**PASS 1296**，捕获的 Error / Exception / Assert 为零。独立聚焦运行 `forge-regression.log`：PASS 229。

完整运行覆盖既有沙漠、雷岛、森林、冰岛、职业、技能、资源、声音、UI、旧档升级与弓弩回归，另验证火岛两层全部 30 房、所有波次、玄武岩阶梯前置条件与实际 F 换层、门口安全点、工匠营门实体碰撞、绞盘救援、放桥前置条件和碰撞消除、展开动画在剧情暂停时继续、巨匠 12 发／16+8 发弹幕、破坏铸模取消其延迟折射且无刷取奖励、链钩不拉玩家、炉芯反击窗口、烬环费用/CD/不抵挡地面熔爆、8 护盾反复读档不叠加、15 房旧档无损扩展和精确原文件备份。

重跑：`python Validation/FireWorkshop/build.py`，再以 Unity 2022.3.62f3 对隔离 QA 工程执行 `NakiFireWorkshopSmoke.Run`。聚焦入口为 `RunFireRegression`。harness 自行退出，不加 `-quit`。存档只写 workspace 下的 validation 目录；测试不写玩家的实际存档。

已目视检查实际截图：`workshop-battle.png`、`forge-master.png`、`evacuation-bridge.png`、`ember-skills.png`，连同火岛港口和第一位首领截图。桥面中心与灰槽一致，完整展开后覆盖可通行区域；怪物切片不夹入邻近主体，首领预警方向已按实际锁定方向显示。

新场景通过 Unity 序列化生成；保留已有主脚本 GUID。`finalize_assets.py` 只回收新素材元数据及新的工坊／巨匠场景，不覆盖用户旧场景。`sync_runtime.py` 预检实际工程与分发目录一致性，先备份旧脚本，再同步核对过的内容并逐字节验证。

已开放两层，不表示地心赤炉和最终 CG 已完成。试玩入口与当前范围见 `Docs/FIRE_WORKSHOP_PLAYABLE_V2.md`。
