# Privacy policy

[English](PRIVACY.md) | [简体中文](PRIVACY.zh-CN.md)

DSH Launcher does not collect analytics, telemetry, advertising identifiers, or
personal files.

Network access occurs only when requested by a launcher function:

- checking the npm registry for a DSH version;
- installing or updating the npm global DSH package;
- downloading an official Node.js prerequisite;
- checking or installing the optional Whale Widget from its documented GitHub
  repository; and
- opening the local DSH web interface.

The launcher stores operational state and redacted logs under the current
user's local application-data directory. It must never log API keys, browser
cookies, or authorization headers. When crash recovery needs to retain a local
DSH access URL, it is encrypted with Windows DPAPI for the current user; the
plaintext URL and its token are never written to logs. The uninstaller deletes
the launcher's local state and diagnostic logs.

DSH and optional plugins have their own behavior and privacy policies. Users
must review those projects before enabling them.
