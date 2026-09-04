# 贡献指南

[English](CONTRIBUTING.md) | [简体中文](CONTRIBUTING.zh-CN.md)

## 开发流程

1. 大幅修改功能或安全行为前，请先创建 Issue。
2. 使用目的明确的分支，并为可观察行为补充测试。
3. 运行格式检查、Release 构建和 `DshLauncher.Tests` 检查程序。
4. 不得提交生成的安装包、凭据、API 密钥、运行状态或第三方压缩包。
5. 提交 Pull Request；合并前至少需要一名维护者审核。

## 依赖和下载变更

修改允许的下载来源、固定版本、哈希、进程终止、权限提升或签名工作流时，必须进行安全专项审核。不得使用未经验证的 `latest` 地址替换固定下载项。

## 许可证

代码贡献采用本仓库 MIT License。除非第三方代码许可证兼容且已在 `THIRD_PARTY_NOTICES.zh-CN.md` 中补齐归属，否则不得复制第三方代码。
