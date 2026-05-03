export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

export const STORAGE_KEYS = {
  token: "ops.accessToken",
  username: "ops.username",
  role: "ops.role",
  expiresAt: "ops.expiresAtUtc",
} as const;

export type Role = "Viewer" | "Operator" | "Admin";

export function getStoredToken(): string | null {
  if (typeof window === "undefined") return null;
  const token = localStorage.getItem(STORAGE_KEYS.token);
  if (!token) return null;
  const exp = localStorage.getItem(STORAGE_KEYS.expiresAt);
  if (exp) {
    const t = new Date(exp).getTime();
    if (!isNaN(t) && t < Date.now()) return null;
  }
  return token;
}

export function clearSession() {
  if (typeof window === "undefined") return;
  Object.values(STORAGE_KEYS).forEach((k) => localStorage.removeItem(k));
}

type ApiOpts = { skipAuth?: boolean };

function buildHeaders(opts?: ApiOpts) {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (!opts?.skipAuth) {
    const token = getStoredToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;
  }
  return headers;
}

function handleAuthFailure() {
  clearSession();
  if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
    const redirect = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.replace(`/login?redirect=${redirect}&reason=expired`);
  }
}

async function parse<T>(res: Response): Promise<T> {
  if (res.status === 204) return undefined as T;
  const ct = res.headers.get("content-type") ?? "";
  if (!ct.includes("application/json")) return undefined as T;
  return res.json() as Promise<T>;
}

async function request<T>(path: string, init: RequestInit, opts?: ApiOpts): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, { ...init, headers: { ...buildHeaders(opts), ...(init.headers || {}) } });
  if (res.status === 401) {
    handleAuthFailure();
    throw new Error("Session expired");
  }
  if (res.status === 403) {
    throw new Error("Access denied");
  }
  if (!res.ok) {
    let detail = "";
    try { detail = await res.text(); } catch {}
    throw new Error(detail || `Request failed (${res.status})`);
  }
  return parse<T>(res);
}

export const apiGet = <T,>(path: string, opts?: ApiOpts) => request<T>(path, { method: "GET" }, opts);
export const apiPost = <T = unknown,>(path: string, body?: unknown, opts?: ApiOpts) =>
  request<T>(path, { method: "POST", body: body !== undefined ? JSON.stringify(body) : undefined }, opts);
export const apiPut = <T = unknown,>(path: string, body?: unknown, opts?: ApiOpts) =>
  request<T>(path, { method: "PUT", body: body !== undefined ? JSON.stringify(body) : undefined }, opts);
export const apiDelete = <T = unknown,>(path: string, opts?: ApiOpts) =>
  request<T>(path, { method: "DELETE" }, opts);

export interface Paged<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export const buildPagedPath = (base: string, page: number, pageSize: number, extra?: Record<string, string | number | undefined>) => {
  const params = new URLSearchParams();
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));
  if (extra) {
    for (const [k, v] of Object.entries(extra)) {
      if (v !== undefined && v !== null && String(v).length > 0) params.set(k, String(v));
    }
  }
  return `${base}?${params.toString()}`;
};

export const ENDPOINTS = {
  events: "/api/agent-events",
  executions: "/api/agent-executions",
  incidents: "/api/incidents",
  tickets: "/api/tickets",
  timeline: "/api/agent-timeline?take=300",
  metricsOverview: "/api/metrics/overview",
  metricsTechnical: "/api/metrics/technical",
  authLogin: "/api/auth/login",
  authMe: "/api/auth/me",
} as const;
