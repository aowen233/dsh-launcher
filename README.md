# DSH Launcher

[English](README.md) | [简体中文](README.zh-CN.md)

An unofficial, open-source Windows launcher for the npm global installation of
[DeepSeek Harness](https://github.com/deepseek-ai/deepseek-harness).

> [!IMPORTANT]
> DSH Launcher is an independent community project. It is not affiliated with,
> endorsed by, or sponsored by DeepSeek.

## Status

The native launcher and installer are working release candidates. Until trusted
Authenticode signing is configured, generated packages remain explicitly named
`unsigned-preview` and must not be presented as stable signed releases.

## Goals

- One-click setup for Windows 10 and Windows 11.
- Native Windows task dialogs with per-monitor high-DPI scaling.
- Detect a compatible Node.js installation and offer the official Node.js LTS
  installer when one is required.
- Install, locate, launch, stop, and update only the npm **global**
  `@deepseek-ai/dsh` package.
- Keep one managed DSH server per user and avoid duplicate launches.
- Bind DSH to `127.0.0.1`, use port 3080 when available, and otherwise select a
  free high port that avoids common application ports.
- Open the default browser and stop the managed server after the page closes.
- If a DSH task is running, wait for it to finish. Reopening the page while a
  shutdown is pending cancels that shutdown generation.
- Notify about DSH updates only when a newer version is available.
- Offer the third-party Whale Widget as an optional plugin pinned to a reviewed,
  immutable upstream commit.

## Security model

The launcher does not expose DSH to the local network. The downloaded Node.js
prerequisite must come from an allow-listed HTTPS origin and pass a SHA-256
manifest check. Optional plugins are installed through DSH's documented plugin
mechanism at an immutable reviewed commit. Secrets must not be written to logs.
See [SECURITY.md](SECURITY.md), [PRIVACY.md](PRIVACY.md), and
[docs/THREAT-MODEL.md](docs/THREAT-MODEL.md).

## Build

The project targets .NET 10 for Windows. Its only direct runtime NuGet package is
Microsoft's MIT-licensed `System.Management`, used for exact DSH process identity
checks.

```powershell
dotnet restore
dotnet build DshLauncher.sln --configuration Release --no-restore
dotnet run --project tests/DshLauncher.Tests/DshLauncher.Tests.csproj --configuration Release --no-build
dotnet publish src/DshLauncher/DshLauncher.csproj --configuration Release --runtime win-x64 --self-contained true
```

Release artifacts are produced only by the checked-in GitHub Actions workflow.
Each candidate includes SHA-256 checksums and a GitHub artifact attestation.
Authenticode signing is intentionally a separate, auditable step.

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md). By participating, you agree to follow
[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## License and attribution

DSH Launcher source code is licensed under the [MIT License](LICENSE). The
application artwork has a separate reuse notice documented in
[ASSET_LICENSE.md](ASSET_LICENSE.md). DeepSeek Harness and
the optional Whale Widget are separate projects and are not redistributed as
launcher source code. Their notices are recorded in
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

Project owner and maintainer: **aowen233**.
