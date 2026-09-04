# 兼容性

[English](COMPATIBILITY.md) | [简体中文](COMPATIBILITY.zh-CN.md)

## Windows

- 最低版本：Windows 10 1809（内部版本 17763）。
- 推荐：仍受支持的 Windows 10 或 Windows 11。
- 架构：x64（主要版本）和 ARM64。
- 不支持 32 位 x86，因为当前 Node.js 已不再提供官方 Windows x86 安装器。

应用目标为 `net10.0-windows10.0.17763.0`，将警告视为错误并采用自包含发布，因此用户无需另装 .NET。对话框使用系统原生任务对话框和 WinForms `PerMonitorV2` 缩放，在不同缩放比例的显示器之间移动时仍保持清晰。

## Node.js 与 DSH

启动器接受 Node.js `^22.19.0` 或 `>=24.0.0`，缺失时优先提供固定版本的 Node.js 24 LTS。Node.js 官方支持表中，x64 支持 Windows 10/Server 2016 及以上，ARM64 支持 Windows 10 及以上。

DeepSeek Harness 仍处于开发者预览阶段，可能引入不兼容变更，因此更新 DSH 必须得到用户确认。
