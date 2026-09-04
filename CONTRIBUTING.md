# Contributing

## Development

1. Open an issue before substantial behavior or security changes.
2. Create a focused branch and include tests for observable behavior.
3. Run `dotnet format --verify-no-changes` and `dotnet test`.
4. Do not commit generated installers, credentials, API keys, runtime state, or
   third-party archives.
5. Submit a pull request. At least one maintainer review is required before
   merge.

## Dependency and download changes

Changes to allow-listed origins, pinned versions, hashes, process termination,
privilege elevation, or signing workflows require security-focused review.
Never replace a pinned download with an unverified `latest` URL.

## Licensing

Contributions are provided under the repository's MIT License. Do not copy
third-party code unless its license is compatible and the required attribution
is added to `THIRD_PARTY_NOTICES.md`.
