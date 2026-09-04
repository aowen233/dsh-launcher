# Code signing policy

[English](CODE_SIGNING_POLICY.md) | [简体中文](CODE_SIGNING_POLICY.zh-CN.md)

Release binaries must be produced from this public repository by the release
workflow. Maintainers do not sign locally built, manually modified, or
unreviewed artifacts.

The preferred no-cost signing route is the SignPath Foundation open-source
program. If accepted, release pages will state:

> Free code signing provided by SignPath.io, certificate by SignPath Foundation.

Project owner, committer, reviewer, and signing approver: **aowen233**. Every
signing request requires manual approval. Signing credentials are never stored
in the repository.

When SignPath Foundation's free open-source certificate is used, Windows shows
`SignPath Foundation` as the Authenticode certificate subject because the
Foundation owns that certificate. Application metadata and installer screens
identify the project publisher as `aowen233`; neither field claims affiliation
with DeepSeek.

Until a trusted Authenticode signing service is configured, releases must be
clearly marked `unsigned-preview`. Self-signed certificates must not be
presented as public-trust signatures.
