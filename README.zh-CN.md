# DSH Launcher

[English](README.md) | [简体中文](README.zh-CN.md)

一个面向 Windows 的非官方开源 DSH 启动器，只管理 npm **全局安装**的
`@deepseek-ai/dsh`。

> [!IMPORTANT]
> 本项目是独立社区项目，与 DeepSeek 无隶属、合作、赞助或官方背书关系。

## 当前状态

原生启动器与安装包已经达到可测试候选版状态。在接入可信 Authenticode
签名之前，生成的安装包会明确标记为 `unsigned-preview`，不能作为已签名稳定版发布。

## 目标功能

- Windows 10/11 一键安装与使用。
- 使用 Windows 原生现代任务对话框，并按显示器自动适配高 DPI 缩放。
- 检测 Node.js；缺失或版本不兼容时，提示并安装经过校验的官方 LTS 版本。
- 只安装、定位、启动、停止和更新 npm 全局版 DSH，不使用隔离副本。
- 单实例管理，重复启动只打开网页，不重复启动服务器。
- 仅绑定 `127.0.0.1`；优先使用 3080，被占用时选择避开常用服务的安全高位端口。
- 网页关闭且没有任务时自动关闭服务器；有任务时等待任务结束。
- 等待关闭期间重新打开网页，会取消该轮自动关闭计划。
- 只有检测到新版本时才弹窗提醒更新。
- 可选安装第三方鲸鱼娘插件，固定到已审核的上游提交版本。

## 安全模型

启动器不会把 DSH 暴露到局域网。Node.js 前置组件只能从 HTTPS 允许清单中的
官方来源下载，并且必须通过 SHA-256 清单校验。可选插件通过 DSH 记录的插件
机制安装，并固定到不可变的已审核提交。机密不得写入日志。详情参见
[SECURITY.zh-CN.md](SECURITY.zh-CN.md)、[PRIVACY.zh-CN.md](PRIVACY.zh-CN.md)
和 [docs/THREAT-MODEL.zh-CN.md](docs/THREAT-MODEL.zh-CN.md)。

## 构建

项目面向 Windows 使用 .NET 10。唯一直接运行时 NuGet 包是 Microsoft 以 MIT
许可证发布的 `System.Management`，用于准确核验 DSH 进程身份。

```powershell
dotnet restore
dotnet build DshLauncher.sln --configuration Release --no-restore
dotnet run --project tests/DshLauncher.Tests/DshLauncher.Tests.csproj --configuration Release --no-build
dotnet publish src/DshLauncher/DshLauncher.csproj --configuration Release --runtime win-x64 --self-contained true
```

发布产物只能由仓库内的 GitHub Actions 工作流生成。每个候选版都包含 SHA-256
校验和与 GitHub 构建来源证明；Authenticode 签名是单独且可审计的环节。

## 参与贡献

请阅读 [CONTRIBUTING.zh-CN.md](CONTRIBUTING.zh-CN.md)。参与本项目即表示同意
遵守 [CODE_OF_CONDUCT.zh-CN.md](CODE_OF_CONDUCT.zh-CN.md)。

## 项目关系与许可证

本启动器源代码使用 [MIT License](LICENSE)；应用图像素材采用单独的
[再使用说明](ASSET_LICENSE.zh-CN.md)。DeepSeek Harness 与鲸鱼娘插件是独立
项目，本仓库不复制它们的源码。详细归属见
[THIRD_PARTY_NOTICES.zh-CN.md](THIRD_PARTY_NOTICES.zh-CN.md)。

项目所有者与维护者：**aowen233**。
