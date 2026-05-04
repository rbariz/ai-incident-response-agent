export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

export const STORAGE_KEYS = {
  token: "ops.accessToken",
  refresh: "ops.refreshToken",
  username: "ops.username",
  role: "ops.role",
  expiresAt: "ops.expiresAtUtc",
} as const;

export type Role = "Viewer" | "Operator" | "Admin";

/** Threshold (ms) before expiry where we consider the token "about to expire" */
const REFRESH_LEEWAY_MS = 60_000;

export function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(STORAGE_KEYS.token);
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(STORAGE_KEYS.refresh);
}

export function getExpiry(): number | null {
  if (typeof window === "undefined") return null;
  const exp = localStorage.getItem(STORAGE_KEYS.expiresAt);
  if (!exp) return null;
  const t = new Date(exp).getTime();
  return isNaN(t) ? null : t;
}

export function isAccessExpired(leewayMs = 0): boolean {
  const t = getExpiry();
  if (t == null) return false;
  return t - leewayMs <= Date.now();
}

/** Returns a non-expired token if available, otherwise null. */
export function getStoredToken(): string | null {
  const tok = getAccessToken();
  if (!tok) return null;
  if (isAccessExpired()) return null;
  return tok;
}

export function clearSession() {
  if (typeof window === "undefined") return;
  Object.values(STORAGE_KEYS).forEach((k) => localStorage.removeItem(k));
}

export function persistSession(s: { accessToken: string; refreshToken: string; username: string; role: Role; expiresAtUtc: string; }) {
  localStorage.setItem(STORAGE_KEYS.token, s.accessToken);
  localStorage.setItem(STORAGE_KEYS.refresh, s.refreshToken);
  localStorage.setItem(STORAGE_KEYS.username, s.username);
  localStorage.setItem(STORAGE_KEYS.role, s.role);
  localStorage.setItem(STORAGE_KEYS.expiresAt, s.expiresAtUtc);
}

type ApiOpts = { skipAuth?: boolean; skipRefresh?: boolean };

function buildHeaders(token: string | null, opts?: ApiOpts) {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (!opts?.skipAuth && token) headers["Authorization"] = `Bearer ${token}`;
  return headers;
}

function handleAuthFailure() {
  clearSession();
  if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
    const redirect = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.replace(`/login?redirect=${redirect}&reason=expired`);
  }
}

// ---------- Refresh token plumbing ----------

type RefreshResponse = {
  accessToken: string;
  refreshToken: string;
  username: string;
  role: Role;
  expiresAtUtc: string;
};

let refreshInFlight: Promise<string | null> | null = null;

async function doRefresh(): Promise<string | null> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return null;
  try {
    const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });
    if (!res.ok) return null;
    const data = (await res.json()) as RefreshResponse;
    if (!data?.accessToken) return null;
    persistSession(data);
    return data.accessToken;
  } catch {
    return null;
  }
}

export async function refreshAccessToken(): Promise<string | null> {
  if (refreshInFlight) return refreshInFlight;
  refreshInFlight = doRefresh().finally(() => { refreshInFlight = null; });
  return refreshInFlight;
}

/** Ensure we have a valid (non-near-expiry) access token. */
export async function ensureFreshToken(): Promise<string | null> {
  const tok = getAccessToken();
  if (!tok) return null;
  if (!isAccessExpired(REFRESH_LEEWAY_MS)) return tok;
  return refreshAccessToken();
}

// ---------- HTTP wrapper ----------

async function parse<T>(res: Response): Promise<T> {
  if (res.status === 204) return undefined as T;
  const ct = res.headers.get("content-type") ?? "";
  if (!ct.includes("application/json")) return undefined as T;
  return res.json() as Promise<T>;
}

async function request<T>(path: string, init: RequestInit, opts?: ApiOpts): Promise<T> {
  let token: string | null = null;
  if (!opts?.skipAuth) {
    token = opts?.skipRefresh ? getAccessToken() : await ensureFreshToken();
  }
  const doFetch = (tok: string | null) =>
    fetch(`${API_BASE_URL}${path}`, { ...init, headers: { ...buildHeaders(tok, opts), ...(init.headers || {}) } });

  let res = await doFetch(token);

  // Reactive refresh on 401
  if (res.status === 401 && !opts?.skipAuth && !opts?.skipRefresh) {
    const fresh = await refreshAccessToken();
    if (fresh) {
      res = await doFetch(fresh);
    } else {
      handleAuthFailure();
      throw new Error("Session expired");
    }
  }

  if (res.status === 401) {
    handleAuthFailure();
    throw new Error("Session expired");
  }
  if (res.status === 403) throw new Error("Access denied");
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
  authRefresh: "/api/auth/refresh",
  authLogout: "/api/auth/logout",
  authMe: "/api/auth/me",
  auditLogs: "/api/audit-logs",
} as const;
