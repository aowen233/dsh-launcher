# Threat model

## Protected assets

- DSH and DeepSeek credentials.
- Integrity of the global npm DSH installation.
- Integrity of downloaded prerequisites and plugins.
- Availability of unrelated local applications and their ports/processes.
- The current user's local files and browser session.

## Trust boundaries

The launcher trusts its signed application files, the operating system, and
explicitly allow-listed upstream services. npm packages, GitHub archives,
browser content, log text, environment variables, and existing local processes
are treated as untrusted input.

## Required controls

- Bind managed servers only to IPv4 loopback (`127.0.0.1`).
- Never use shell interpolation for paths or arguments.
- Resolve the npm global root once per operation and verify the DSH package name
  and manifest before starting or terminating a process.
- Terminate only a recorded process tree whose executable and command line match
  the global DSH installation. Never terminate every `node.exe` process.
- Keep mutable state in a per-user directory with an ACL limited to the user,
  SYSTEM, and Administrators.
- Redact URL credentials, tokens, authorization headers, and secret-like query
  parameters before writing logs.
- Encrypt the crash-recovery access URL with Windows DPAPI scoped to the current
  user, and revalidate the PID, global install path, command line, and port before
  adopting a previous server.
- Use a named mutex and authenticated same-user named pipe for single-instance
  commands.
- Use HTTPS allow-lists, pinned versions, and SHA-256 verification for downloads.
- Embed the approved dependency manifest and status plugin in signed application
  files; do not load a mutable manifest from beside the executable.
- Treat failed or stale task-state information conservatively: keep the server
  running rather than interrupting an unknown task.
- Avoid silent elevation. Explain each system-level prerequisite change before
  requesting UAC consent.

## Out of scope

The launcher cannot guarantee the security of upstream DSH tasks, user-installed
plugins, the npm registry, GitHub, Node.js, or the user's browser. Compromise of
the Windows account or an administrator account is also outside its boundary.
