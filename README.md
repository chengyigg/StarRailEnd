# 星轨尽头 · Star Rail's End

> 一个用 Unity 一周完成的 2D 文字冒险 AVG —— 数据驱动的对话系统 + 多结局分支 + 完整存档体验。

🎬 **[观看 1 分半完整演示视频（B 站）](https://www.bilibili.com/video/BV1nHpA6NEJA)**
---

## 项目简介

- **引擎**：Unity 2022.3.62f3c1
- **类型**：2D 文字冒险 AVG
- **内容量**：24 个对话序列 / 3 个结局 / 一周目约 30 分钟
- **开发周期**：约 10 天（个人独立开发）
- **目标平台**：Windows PC

---

## 技术亮点

### 1. 数据驱动的对话系统

剧情数据储存在 `DialogueSequence` ScriptableObject 中，支持分支选项、好感度变动、变量设置、条件跳转、结局判定。

剧本通过自研的 **CSV Importer** 批量导入生成 `.asset`，编剧只需写文本，不用碰 Unity。

### 2. 组件化的对话系统架构

`DialogueManager` 只负责核心流程（推进、跳转、存档），其他职责全部拆为独立组件：

| 组件 | 职责 |
|---|---|
| `TypewriterEffect` | 打字机效果（含标点停顿） |
| `DialogueChoiceView` | 选项按钮生成与点击分发 |
| `DialogueHistoryPanel` | 对话历史记录 |
| `DialoguePortraitView` | 立绘淡入 / 移动 / 淡出 |
| `DialogueBackgroundView` | 背景交叉淡入淡出 + 内心独白滤镜 |
| `DialogueKeyboard` | Ctrl 快进、Esc 快捷操作 |

各组件通过事件与公开方法通信，互不依赖。

### 3. 完整的存档系统

- 9 个手动存档槽 + 1 个自动存档
- 截图缩略图（存档时截取当前画面，缩放到 480×270 保存）
- 双栏 UI：左侧大预览 + 右侧 9 宫格缩略图
- 覆盖确认、单项删除、全部删除
- 跨场景读档（保存进度 + 恢复状态）

### 4. 好感度系统与多结局联动

- 好感度 -100 ~ 100，通过选项与剧情节点增减
- 好感度归零触发特殊剧情
- 结局分支：好结局 / 普通结局 / 坏结局，由最终好感度判定

### 5. 自研 Editor 工具链（8 个）

| 工具 | 功能 |
|---|---|
| `DialogueCSVImporter` | 批量导入 CSV 剧本，自动生成 ScriptableObject |
| `DialogueFlowViewer` | 生成剧情流程图，检测死胡同 / 断链 / 孤立序列 |
| `SequenceReferenceFinder` | 追踪任意资源被哪些序列引用 |
| `AutoFillSequences` | 一键填充对话管理器序列列表 |
| `MissingReferenceChecker` | 扫描场景中所有空引用 |
| `SceneExporter` | 导出场景 Hierarchy 结构为文本 |
| `ScriptExporter` | 导出项目全部脚本 |
| `AssetTreeExporter` | 导出 Assets 目录树 |

### 6. 性能与工程实践

- **PlayerPrefs 缓存 + 脏标记**：设置项读写在内存完成，退出时统一落盘
- **ReadTracker 延迟落盘**：已读记录攒批写入，避免频繁磁盘 I/O
- **资源缓存**：心形图标、光环纹理、角色数据全部静态缓存
- **事件驱动**：`GameSettings.OnBgmVolumeChanged`、`AffectionManager.OnAffectionChanged`、`SaveManager.OnAutoSaveTriggered` 等事件解耦系统
- **分层落盘器**：`PlayerPrefsSaver` / `GameSettingsSaver` / `ReadTrackerSaver` 三个自动创建的常驻对象，分别管理各自的持久化

---

## 项目结构

```
Assets/
├── Scripts/
│   ├── Core/          引擎基础设施（AudioManager、GameSettings、SceneFader…）
│   ├── Data/          纯数据类（DialogueSequence、SaveData…）
│   ├── Dialogue/      对话系统核心 + 6 个组件
│   ├── Systems/       逻辑系统（SaveManager、AffectionManager…）
│   ├── UI/
│   │   ├── Panels/    大面板
│   │   └── Widgets/   小部件
│   └── Visual/
│       ├── Animators/ 动画组件
│       ├── Stylers/   样式组件
│       └── Effects/   特效与程序化纹理
├── Editor/            8 个 Editor 工具
├── Dialogues/         24 个对话序列 .asset
├── Prefabs/           UI 预制体
├── Resources/         运行时加载资源
└── Scenes/            SampleScene（主菜单） / Game（游戏）
```

---

## 素材来源

| 部分 | 来源 |
|---|---|
| **代码 / 系统设计 / Editor 工具** | 本人独立完成 |
| **美术（背景 / 立绘 / 头像）** | AI 生成（Gemini） |
| **音乐 / 音效** | AI 生成 + 免费音效库 |
| **剧本** | AI 辅助生成，本人整理与打磨 |

> 本项目重点验证 **Unity 数据驱动架构 + AI 工具链的效率**。美术、音乐、剧本通过 AI 辅助生成，代码与系统设计为独立完成。

---

## 如何运行

1. 克隆仓库：
   ```
   git clone https://github.com/chengyigg/StarRailEnd.git
   ```
2. 用 Unity Hub 以 **Unity 2022.3.62f3c1** 打开项目
3. 打开 `Assets/Scenes/SampleScene.unity`
4. 点击 Play

---

## 联系方式

- GitHub: [@chengyigg](https://github.com/chengyigg)
- 邮箱：3201663235@qq.com
