# 代码签名策略

[English](CODE_SIGNING_POLICY.md) | [简体中文](CODE_SIGNING_POLICY.zh-CN.md)

发布二进制文件必须由本公开仓库的发布工作流生成。维护者不得签署本地构建、手工修改或未经审核的产物。

首选的免费签名方案是 SignPath Foundation 开源计划。获批后，发布页面将注明：

> Free code signing provided by SignPath.io, certificate by SignPath Foundation.

项目所有者、提交者、审核者和签名批准者为 **aowen233**。每次签名请求都必须人工批准，签名凭据不得存入仓库。

使用 SignPath Foundation 免费开源证书时，Windows 显示的 Authenticode 证书主体是 `SignPath Foundation`，因为证书归基金会所有；应用元数据和安装界面仍将发布者标为 `aowen233`。两者均不表示与 DeepSeek 存在关联。

在可信 Authenticode 服务配置完成前，发布文件必须明确标为 `unsigned-preview`。不得把自签名证书宣传为受公众信任的签名。
