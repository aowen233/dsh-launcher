# 更新日志

[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

本文件记录重要变更，格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本遵循[语义化版本](https://semver.org/lang/zh-CN/)。

## [未发布]

## [0.1.0] - 2026-09-04

### 新增

- 原生 Windows 启动器仓库及安全文档。
- npm 全局版 DSH 的发现、启动、更新和经过验证的进程管理。
- 仅回环地址的端口选择及任务感知的浏览器生命周期管理。
- 经过校验的 Node.js 24 LTS x64/ARM64 前置安装信息。
- 固定上游提交并注明 MeteorNOX 归属的可选鲸鱼娘插件。
- 带校验和与 GitHub 来源证明的可复现未签名候选版流程。
- Windows 原生任务对话框和 PerMonitorV2 高 DPI 缩放。
- 固定版本并保留 MIT 许可证的简体中文安装器翻译。

### 修复

- 恢复已由启动器管理的全局 DSH 服务器时不再重复启动。
- 收到停止命令后，主启动器能够完整退出。
- 正常启动时不再重复解析 npm 全局目录。
