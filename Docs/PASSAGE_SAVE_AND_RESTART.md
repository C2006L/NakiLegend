# 港口通路、旅行存档与从头试玩

## 本轮修复

- 新章节清理冰、火、雷等技能遗留的时间戳，解决 `Invalid saved cooldown` 引起的存档失败与航图无法登岛。保留冷却值校验。
- 主冒险记录最后成功保存的岛屿；重新运行主场景，恢复该岛自己的安全点和六格背包。明确打开单独章节场景时，继续那个章节。
- 鸣沙洲存档增加可选的外岛技能记录，保留回访时携带的雷、冰、火能力，旧档仍可读取。跨岛继续携带装备、消耗品、天赋和资源。
- 港口默认落点放到岸上；横向开口、接路与碰撞统一加宽。栈桥同步调整画面和可走宽度，可以偏离中线走门；海面保持阻挡。
- 观雨台及沿海港口换用新的完整底图。火岛保留黑砂港场景，并使用相同通路修正。
- 五岛栅栏改为砂岩铜门、雷纹铜门、冰钢、熔铁、木藤门禁。开门保留石座，封闭时阻挡角色与弹体。
- 雷雨高原旧方块路标换为刻纹石灯标，另加入铜手柄、避雨棚、巡路记录台。

## 从鸣沙洲重新开始

本次重置先停止 Play，将全部匹配的旅程文件备份、校验后再清空。备份位于正在使用的 Unity 工程根目录 `ProgressBackups` 中。五岛进度、背包与天赋从头开始，音量偏好保留。

1. 等待 Unity 导入和编译结束。
2. 选择 `Tools → Naki Legend → Play → 1 Desert Adventure`，点击 Play。
3. 观看或跳过开场，再选择职业和光／暗方向。
4. 推荐路线：鸣沙洲 → 雷雨高原 → 霜潮群岛 → 赤炉群岛 → 青栖林海。码头 F 或 G 打开航图，选择岛屿与继续点，再确认出发。
5. 安全房自动保存，F5 手动存档、F9 读取；B 整理行囊，V 阅读手记。下次继续仍运行主冒险场景，将回到最后成功保存的岛屿。

以后再次从头测试，可停止 Play 后使用 `Tools → Naki Legend → Saves → Backup and Reset Five Islands`，同样先备份再清理。

## 新美术资源

以下由内置 imagegen 制作／编辑。图集保留透明通道，由 Unity 运行时切片；没有使用 Python 修改图像。

- [五岛门禁图集](../Assets/NakiPrototype/Resources/NakiArt/PassageV2/island-gates.png)
- [雷雨交互物图集](../Assets/NakiPrototype/Resources/NakiArt/PassageV2/storm-props.png)
- [雨后港口](../Assets/NakiPrototype/Resources/NakiArt/PassageV2/rain-harbor.png)
- [沿海港口](../Assets/NakiPrototype/Resources/NakiArt/PassageV2/coastal-harbor.png)
- [完整生成提示词与参考源](../Art/PassageV2/prompts.json)
