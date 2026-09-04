# 发布指南

[English](RELEASING.md) | [简体中文](RELEASING.zh-CN.md)

只有仓库所有者本人负责登录 GitHub、推送提交或标签以及发布 Release。整理构建内容不需要访问所有者的 GitHub 账号。

## 未签名预发布版

1. 确认 `main` 工作区干净且 CI 已通过。
2. 在经过审核的发布提交上创建带说明的版本标签。
3. 将提交和标签推送到 GitHub。
4. 为该标签创建 GitHub Release，并勾选 **Set as a pre-release**。
5. 标题中明确写出 `unsigned preview`。
6. 上传两个安装包、独立源码 ZIP 和 `SHA256SUMS.txt`。
7. 粘贴经过审核的中英文双语发布说明并发布。
8. 发布后逐个下载文件，重新验证 SHA-256。

GitHub 还会自动为标签生成源码 ZIP 和 TAR.GZ。

## 稳定版

受保护发布工作流生成可信 Authenticode 签名前，不得取消预发布标记或宣传为稳定版。应遵循 `CODE_SIGNING_POLICY.zh-CN.md`，并在双语发布说明中记录签名服务商和证书主体。
