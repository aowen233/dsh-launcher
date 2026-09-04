# Changelog

[English](CHANGELOG.md) | [简体中文](CHANGELOG.zh-CN.md)

All notable changes will be documented in this file. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and versions follow
[Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0] - 2026-09-04

### Added

- Native Windows launcher repository and security documentation.
- npm global DSH discovery, launch, update, and verified process handling.
- Loopback-only port selection and task-aware browser lifecycle management.
- Verified Node.js 24 LTS prerequisite installer metadata for x64 and arm64.
- Optional, pinned Whale Widget integration attributed to MeteorNOX.
- Reproducible unsigned-candidate workflow with checksums and GitHub provenance.
- Native task-dialog UI and per-monitor V2 high-DPI scaling.
- Simplified Chinese installer translation pinned with its MIT license.

### Fixed

- Recover an already-managed global DSH server without starting a duplicate.
- Exit the primary launcher cleanly after a stop command.
- Avoid repeated npm global-root discovery during normal startup.
