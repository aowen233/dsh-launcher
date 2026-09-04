# Release guide

[English](RELEASING.md) | [简体中文](RELEASING.zh-CN.md)

Only the repository owner performs GitHub authentication, pushes commits or
tags, and publishes releases. Build preparation does not require access to the
owner's GitHub account.

## Unsigned preview release

1. Confirm `main` is clean and CI has passed.
2. Create an annotated version tag on the reviewed release commit.
3. Push the commit and tag to GitHub.
4. Create a GitHub Release for that tag and mark it as a **pre-release**.
5. Include `unsigned preview` in the title.
6. Upload both installers, the explicit source ZIP, and `SHA256SUMS.txt`.
7. Paste the reviewed bilingual release notes and publish.
8. Download every asset once and verify its SHA-256 value.

GitHub also generates source-code ZIP and TAR.GZ archives for the tag.

## Stable release

Do not clear the pre-release flag or describe a build as stable until the
protected release workflow produces a trusted Authenticode signature. Follow
`CODE_SIGNING_POLICY.md` and record the signing provider and certificate subject
in the bilingual release notes.
