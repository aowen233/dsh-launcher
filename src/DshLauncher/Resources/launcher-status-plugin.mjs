import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';

export const name = 'dsh-launcher-status';
export const inject = ['agents'];

export function apply(ctx, config = {}) {
  const statusPath = config.path || process.env.DSH_LAUNCHER_STATUS_FILE;
  if (!statusPath) throw new Error('DSH_LAUNCHER_STATUS_FILE is required');

  mkdirSync(dirname(statusPath), { recursive: true });

  const writeSnapshot = () => {
    const agents = ctx.agents.list().map((agent) => ({
      id: String(agent.id),
      status: agent.status,
    }));
    const payload = {
      serverPid: process.pid,
      running: agents.filter((agent) => agent.status === 'running').length,
      agentCount: agents.length,
      agents,
      updatedAt: new Date().toISOString(),
    };
    writeFileSync(statusPath, JSON.stringify(payload), 'utf8');
  };

  ctx.on('agent/created', writeSnapshot);
  ctx.on('agent/status', writeSnapshot);
  ctx.on('agent/disposed', writeSnapshot);

  const timer = setInterval(writeSnapshot, 1000);
  timer.unref();
  ctx.effect(function* launcherStatusLifetime() {
    writeSnapshot();
    yield () => clearInterval(timer);
  });
}
