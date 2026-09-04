# Public repository setup

Complete these steps before the first public release.

## Repository

1. Create a public repository named `dsh-launcher`.
2. Push this source tree without `bin`, `obj`, `artifacts`, installers, runtime
   state, or credentials.
3. Replace the generic GitHub URLs in `installer/DshLauncher.iss` with the final
   repository URLs.
4. Enable Issues and private vulnerability reporting.
5. Add the private advisory URL to `.github/ISSUE_TEMPLATE/config.yml`.
6. Confirm that the public account or organization grants `aowen233` the owner,
   maintainer, reviewer, and signing-approver roles documented in
   `CODE_SIGNING_POLICY.md`.

## Branch protection

Protect `main` and require:

- pull requests before merging;
- at least one approving review;
- the `build-and-test` status check;
- conversation resolution;
- signed commits where practical;
- dismissal of stale approvals after new commits; and
- no force pushes or branch deletion.

Do not allow release or signing workflows to run with secrets on pull requests
from forks.

## Release environments

Create a protected `release` environment with a required human approver. Add
signing integration credentials only to that environment. The current workflow
produces unsigned preview artifacts and cannot create a public GitHub Release.

## Free open-source signing

After the project has a public, functioning prerelease, apply to SignPath
Foundation. The application should link to:

- `LICENSE`;
- `PRIVACY.md`;
- `SECURITY.md`;
- `CODE_SIGNING_POLICY.md`;
- the reproducible release workflow; and
- the published unsigned preview and its GitHub provenance.
