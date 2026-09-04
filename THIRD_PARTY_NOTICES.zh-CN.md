# 第三方声明

[English](THIRD_PARTY_NOTICES.md) | [简体中文](THIRD_PARTY_NOTICES.zh-CN.md)

## 应用图像素材

用户提供的应用图像及其编辑后图标受 `ASSET_LICENSE.zh-CN.md` 中的单独说明约束，不属于 MIT 软件许可证范围。

本仓库只包含面向下列独立第三方项目的集成代码和元数据，不主张拥有其名称、图像或源代码。

## DeepSeek Harness

- 项目：DeepSeek Harness（`@deepseek-ai/dsh`）
- 版权：Copyright (c) 2026 DeepSeek
- 仓库：https://github.com/deepseek-ai/deepseek-harness
- 许可证：MIT
- 关系：通过 npm 全局安装的外部运行时

安装器从 npm 注册表下载 DeepSeek Harness。DSH Launcher 并非 DeepSeek Harness 的作者，双方不存在隶属关系。

## Node.js

- 项目：Node.js
- 网站：https://nodejs.org/
- 许可证信息：https://github.com/nodejs/node/blob/main/LICENSE
- 关系：按需从 Node.js 官方分发服务下载的前置组件

## DeepSeek Balance Whale Widget

- 项目/包：DeepSeek Balance Whale Widget（`dsh-whale-widget`）
- 作者及版权所有者：MeteorNOX
- 版权：Copyright (c) 2026 MeteorNOX
- 仓库：https://github.com/MeteorNOX/DeepSeek-Balance-Whale-Widget
- 许可证：MIT
- 关系：仅在用户明确选择后下载的可选第三方 DSH 插件

该组件不打包在本仓库中。内置依赖清单将其固定到已审核的上游提交，DSH 仅在用户明确同意后安装。启动器保留归属和上游许可证说明。

## Inno Setup 简体中文翻译

- 项目：Inno Setup Chinese Simplified Translation
- 作者：kira-96 / kirakira
- 仓库：https://github.com/kira-96/Inno-Setup-Chinese-Simplified-Translation
- 固定提交：`1ff90acc4ed4aee82b1cda43253243deee3daed4`
- 许可证：MIT
- 关系：仅用于安装器本地化的翻译文件

所需版权与许可证原文保存在 `installer/translations/LICENSE-kira-96.txt`。

## System.Management

- 项目/包：`System.Management`
- 版权：.NET Foundation and Contributors
- 包所有者：Microsoft
- 版本：10.0.11
- 许可证：MIT
- 关系：用于查询准确 Windows 进程命令行的运行时库
