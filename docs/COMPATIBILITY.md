# Compatibility

[English](COMPATIBILITY.md) | [简体中文](COMPATIBILITY.zh-CN.md)

## Windows

- Minimum launcher target: Windows 10 version 1809 (build 17763).
- Recommended: a supported Windows 10 or Windows 11 release.
- Architectures: x64 (primary) and arm64.
- 32-bit x86 is not supported because current Node.js releases no longer ship
  an official Windows x86 installer.

The application targets `net10.0-windows10.0.17763.0`, treats warnings as errors,
and is published self-contained. Users do not need to install .NET.
Dialogs use the operating system's native task-dialog surface and WinForms
`PerMonitorV2` scaling so moving between monitors with different scaling does
not leave the interface blurry.

## Node.js and DSH

The launcher accepts Node.js `^22.19.0` or `>=24.0.0` and prefers the pinned
Node.js 24 LTS prerequisite. The official Node.js support table lists Windows
10/Server 2016 or later for x64 and Windows 10 or later for arm64.

DeepSeek Harness is in developer preview and may introduce breaking changes.
DSH updates therefore require user confirmation.
