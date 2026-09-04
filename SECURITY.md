# Security policy

## Supported versions

Until the first stable release, only the newest tagged prerelease is supported.

## Reporting a vulnerability

Do not disclose a suspected vulnerability in a public issue. Use GitHub's
private vulnerability reporting feature for this repository. Include the
affected version, reproduction steps, impact, and any suggested mitigation.

Maintainers should acknowledge a report within seven days. Release timing
depends on severity and whether an upstream DSH, Node.js, or plugin fix is
required.

## Release security

- Unsigned candidates are built by GitHub Actions whose actions are pinned to
  immutable commit SHAs.
- The candidate workflow creates SHA-256 checksums and build provenance but is
  not permitted to publish a stable GitHub Release.
- Public releases must be built from a protected tag and Authenticode-signed
  after compilation and before publication.
- Release workflows never run untrusted pull-request code with signing secrets.
- Downloaded Node.js packages must match an approved SHA-256 manifest digest.
  Optional plugins must be pinned to an approved immutable upstream commit.

Signing does not prove that software is free of vulnerabilities. Users should
also verify the release provenance and checksum.
