# 雷雨高原章节美术

2026-10-28。使用内置 imagegen 生成的原始 PNG 保存在 Assets/NakiPrototype/Resources/NakiArt/ThunderChapter，不重复存储同一份大图。对应完整提示见 generation-prompts.json。

三位 Boss 和铜壳滚兽各有八个真实姿态：四步走动、蓄力、攻击、受击、倒地。art.json 逐帧记录透明轮廓的实际范围与落脚点，避免相邻帧串入披风、剑或头发。武器源图分别保存，握点按真实手柄设置；弓弩箭头朝局部正 X。

两幅新地面分别用于遗址和圣所，草坡与环境机关继续使用 ThunderS1 素材。灯匠、巡路员和“灯下有归人”尾声为本次新增。NPC 当前为静态立姿，敌人采用左右镜像朝向。

Unity 原生 1920×1080 画面见本目录的 chapter-setup.png、floor1-lampkeeper.png、floor2-nodes.png、floor3-stag.png、ending-cg.png。战斗画面来自实际游戏运行。
