# Third-party notices

## Application artwork

The user-supplied application artwork and its edited icon are governed by the
separate terms and attribution in `ASSET_LICENSE.md`. They are not included in
the MIT software license.

This repository contains integration code and metadata for independent
third-party projects. It does not claim ownership of their names, artwork, or
source code.

## DeepSeek Harness

- Project: DeepSeek Harness (`@deepseek-ai/dsh`)
- Copyright: Copyright (c) 2026 DeepSeek
- Repository: https://github.com/deepseek-ai/deepseek-harness
- License: MIT
- Relationship: external runtime installed globally through npm

The installer downloads DeepSeek Harness from the npm registry. DeepSeek
Harness is not authored by or affiliated with DSH Launcher.

## Node.js

- Project: Node.js
- Website: https://nodejs.org/
- License information: https://github.com/nodejs/node/blob/main/LICENSE
- Relationship: optional prerequisite downloaded from the official Node.js
  distribution service

## DeepSeek Balance Whale Widget

- Project/package: DeepSeek Balance Whale Widget (`dsh-whale-widget`)
- Author and copyright holder: MeteorNOX
- Copyright: Copyright (c) 2026 MeteorNOX
- Repository: https://github.com/MeteorNOX/DeepSeek-Balance-Whale-Widget
- License: MIT
- Relationship: optional third-party DSH plugin, downloaded only after explicit
  user selection

The widget is not bundled in this repository. The embedded dependency manifest
pins an audited upstream commit, and DSH installs it only after explicit user
consent. The launcher preserves this attribution and the upstream license notice.

## Inno Setup Simplified Chinese Translation

- Project: Inno Setup Chinese Simplified Translation
- Author: kira-96 / kirakira
- Repository: https://github.com/kira-96/Inno-Setup-Chinese-Simplified-Translation
- Pinned commit: `1ff90acc4ed4aee82b1cda43253243deee3daed4`
- License: MIT
- Relationship: the translation file is included only to localize the installer

The required copyright and license text is preserved in
`installer/translations/LICENSE-kira-96.txt`.

## System.Management

- Project/package: `System.Management`
- Copyright: .NET Foundation and Contributors
- Package owner: Microsoft
- Version: 10.0.11
- License: MIT
- Relationship: runtime library used to query exact Windows process command lines
