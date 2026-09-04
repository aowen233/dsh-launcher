# DSH Launcher

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

安全设计参见 [SECURITY.md](SECURITY.md)、[PRIVACY.md](PRIVACY.md) 和
[docs/THREAT-MODEL.md](docs/THREAT-MODEL.md)。

## 项目关系与许可证

本启动器源代码使用 [MIT License](LICENSE)；应用图像素材采用单独的
[CC BY-NC-SA 4.0 条款](ASSET_LICENSE.md)。DeepSeek Harness 与鲸鱼娘插件是独立
项目，本仓库不复制它们的源码。详细归属见
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

项目所有者与维护者：**aowen233**。
