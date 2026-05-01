export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

export const ENDPOINTS = {
  events: "/api/agent-events?take=200",
  executions: "/api/agent-executions?take=200",
  incidents: "/api/incidents?take=200",
  timeline: "/api/agent-timeline?take=300",
} as const;

export async function apiGet<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json" },
  });
  if (!res.ok) throw new Error(`Request failed (${res.status})`);
  return res.json() as Promise<T>;
}
