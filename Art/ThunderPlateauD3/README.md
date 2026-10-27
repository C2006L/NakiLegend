# 雷雨高原 D3 连接蓝图

这是房间关系的设计图，不是游戏中的选关界面，也不是最终地图美术。

- [PNG 预览](room-connections.png)：三层 12 / 13 / 14 房、普通通道与捷径。
- [可编辑 SVG](room-connections.svg)：用于放大查看和后续标注。
- [对应连接数据](../../Docs/DesignData/ThunderPlateauD3/room-graph.json)：39 房、41 条层内通道、2 条跨层石阶，未被 Unity 运行代码加载。
- [地图说明](../../Docs/THUNDER_MAP_D3.md)：门位、机关条件、六类场地模块、随机变化与保存规则。

蓝图由连接数据绘制，已检查房间数量、格位、相邻双向连接和关闭捷径后的主路可达性。实际通行、掩体与门碰撞需在后续 Unity 原型验证。环境和角色的概念美术仍见 [D2 四张原稿](../ThunderPlateauD2/README.md)。
