# 公开仓库配置

[English](REPOSITORY-SETUP.md) | [简体中文](REPOSITORY-SETUP.zh-CN.md)

首次公开发布前完成以下项目。

## 仓库

1. 创建名为 `dsh-launcher` 的公开仓库。
2. 推送源码，但不包含 `bin`、`obj`、`artifacts`、安装包、运行状态或凭据。
3. 每次发布前确认 `installer/DshLauncher.iss` 中的仓库链接。
4. 启用 Issues 和私有漏洞报告。
5. 确认公开账号或组织为 `aowen233` 提供 `CODE_SIGNING_POLICY.zh-CN.md` 记录的所有者、维护者、审核者和签名批准者角色。

## 分支保护

保护 `main`，要求 Pull Request、至少一名审核者批准、`build-and-test` 状态检查通过、解决对话，并禁止强制推送和删除分支。可行时要求签名提交，新提交后撤销旧批准。不得让来自 Fork 的不可信 Pull Request 使用发布或签名机密。

## 发布环境

创建需要人工批准的受保护 `release` 环境。签名集成凭据只能放在该环境中。当前工作流生成未签名预览产物，不能创建稳定 GitHub Release。

## 免费开源签名

公开可用的预发布版上线后可申请 SignPath Foundation。申请材料应链接许可证、隐私与安全政策、代码签名策略、可复现发布工作流，以及带 GitHub 来源证明的公开未签名预览版。
