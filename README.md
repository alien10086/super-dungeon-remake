# Super Dungeon Delve Remake

![Godot Engine](https://img.shields.io/badge/Godot-4.4-blue?logo=godot-engine&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET%208.0-purple?logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![Game Genre](https://img.shields.io/badge/Genre-Roguelike%20%7C%20Dungeon%20Crawler-orange)
![Development Status](https://img.shields.io/badge/Status-In%20Development-yellow)

## 项目简介

这是一个基于 [benc-uk/super-dungeon-delve](https://github.com/benc-uk/super-dungeon-delve) 项目的学习性复刻版本。<mcreference link="https://github.com/benc-uk/super-dungeon-delve" index="0">0</mcreference> 原项目是一个使用 Godot 引擎开发的复古风格 2D 地牢探索游戏，本项目在此基础上进行了重新实现，主要用于学习现代游戏开发技术。

游戏采用经典的 Roguelike 风格，玩家需要在随机生成的地牢中探索、战斗和收集宝物。

## 游戏特色

- **随机地牢生成**: 每次游戏都会生成不同的地牢布局，提供无限的重玩价值
- **角色扮演元素**: 玩家可以控制角色在地牢中移动、攻击敌人并收集金币
- **现代化重制**: 基于经典地牢探索游戏的现代化重制版本
- **视觉效果**: 包含光照系统、动画效果和音效
- **敌人系统**: 多种敌人类型，各具特色的AI行为

## 技术栈

- **游戏引擎**: Godot 4.4
- **编程语言**: C# (.NET 8.0)
- **图形**: 2D 像素艺术风格
- **音频**: 支持背景音乐和音效

## 项目结构

```
super-dungeon-remake/
├── Assets/          # 游戏资源文件
│   ├── Sprites/     # 精灵图片
│   ├── Music/       # 背景音乐
│   ├── SFX/         # 音效文件
│   └── UI/          # 用户界面资源
├── Scenes/          # Godot场景文件
│   ├── Player.tscn  # 玩家场景
│   ├── Enemies/     # 敌人场景
│   ├── Level/       # 关卡场景
│   └── UI/          # 用户界面场景
├── Scripts/         # C# 脚本文件
│   ├── Core/        # 核心游戏逻辑
│   ├── Gameplay/    # 游戏玩法相关
│   ├── Level/       # 关卡生成逻辑
│   └── Utils/       # 工具类
└── Resources/       # Godot资源文件
```

## 核心功能

### 玩家系统
- 角色移动和动画
- 攻击系统和武器
- 生命值管理
- 光照效果

### 地牢生成
- 程序化地牢生成算法
- 房间和走廊系统
- 瓦片地图渲染

### 敌人系统
- 多种敌人类型
- AI 行为模式
- 战斗机制

### 游戏管理
- 游戏状态管理
- 数据持久化
- 用户界面系统

## 开发目标

这个项目旨在通过复刻经典地牢探索游戏来学习现代游戏开发技术。<mcreference link="https://github.com/benc-uk/super-dungeon-delve" index="0">0</mcreference> 通过使用 Godot 引擎和 C#，项目展示了如何构建一个完整的 2D 游戏，包括:

- 模块化的代码架构
- 可扩展的游戏系统
- 现代化的开发工具链
- 跨平台兼容性
- BSP树算法实现随机地牢生成
- 敌人AI行为模式设计

## 运行要求

- Godot 4.4 或更高版本
- .NET 8.0 SDK
- 支持 C# 的开发环境

## 致谢

本项目基于 [benc-uk/super-dungeon-delve](https://github.com/benc-uk/super-dungeon-delve) 进行学习性复刻。<mcreference link="https://github.com/benc-uk/super-dungeon-delve" index="0">0</mcreference> 感谢原作者提供了优秀的开源项目作为学习参考。

**原项目技术栈：**
- Godot 3.x 引擎
- 纯 GDScript 开发

**本项目技术升级：**
- Godot 4.4 引擎
- C# (.NET 8.0) 开发

**原项目特色：**
- 使用BSP树方法生成随机关卡
- 三种不同的敌人类型（哥布林、骷髅、史莱姆）
- 可收集的宝藏和生命药水
- 支持手柄和键盘控制

---

*这是一个学习项目，旨在通过复刻经典游戏来探索现代游戏开发技术和设计理念。*