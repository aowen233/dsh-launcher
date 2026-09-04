# Architecture

## Components

- `DshLauncher.Core`: platform-neutral version, port, and redaction policies.
- `DshLauncher`: native Windows tray application and lifecycle controller.
- `launcher-status-plugin.mjs`: minimal DSH plugin that reports task state to a
  user-private local file.
- `dependencies.json`: reviewed prerequisite and optional-plugin manifest.
- `installer`: per-user Inno Setup package. The launcher separately requests UAC
  only when the user approves installation of the system-wide Node.js MSI.

## Startup flow

1. Acquire a per-user mutex. Secondary launches send a command through a
   current-user-only named pipe.
2. Locate a supported Node.js installation without changing the system.
3. If needed, download the pinned official Node.js LTS MSI, verify SHA-256, and
   request explicit UAC consent.
4. Resolve `npm root --global` exactly once and validate the
   `@deepseek-ai/dsh/package.json` name, version, entry point, and containment.
5. Reuse an already-running process only when its command line contains that
   exact global DSH entry point.
6. Otherwise select a loopback port, generate the task-status patch, and start
   DSH directly with `node.exe`; no command shell is used.
7. Verify that the listener is loopback-only before opening the browser.

## Shutdown generation

The launcher increments a generation counter whenever the page is opened. Once
the browser disconnects, it waits twelve seconds. A fresh task snapshot decides
whether shutdown is safe. Unknown or stale state keeps the server alive.

If tasks are running, the current generation becomes the pending-shutdown
generation. Reopening the page clears that pending generation. Therefore a task
that finishes after a reopen cannot shut down the newly opened session.

## Updates

Background checks never interrupt startup and never display a dialog when the
installed DSH version is current. An approved update stops only processes whose
command lines are revalidated against the npm global DSH entry point, then runs
npm through `node.exe` and npm's JavaScript entry point. Other Node.js programs
are not terminated.

Unsigned candidate builds cannot create a public GitHub Release. Signing and
release publication will be implemented through a protected environment after
the open-source signing provider approves the project.
