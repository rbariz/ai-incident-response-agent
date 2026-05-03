import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { apiGet, apiPost, clearSession, ENDPOINTS, STORAGE_KEYS, type Role } from "@/config/api";

export type AuthUser = {
  username: string;
  role: Role;
  expiresAtUtc?: string;
};

type LoginResponse = {
  accessToken: string;
  username: string;
  role: Role;
  expiresAtUtc: string;
};

type AuthCtx = {
  user: AuthUser | null;
  ready: boolean;
  loading: boolean;
  login: (username: string, password: string) => Promise<AuthUser>;
  logout: () => void;
  hasRole: (...roles: Role[]) => boolean;
  canApprove: boolean;
  canManageTickets: boolean;
  canEditIncident: boolean;
  canDeleteIncident: boolean;
};

const Ctx = createContext<AuthCtx>({
  user: null,
  ready: false,
  loading: false,
  login: async () => { throw new Error("AuthProvider missing"); },
  logout: () => {},
  hasRole: () => false,
  canApprove: false,
  canManageTickets: false,
  canEditIncident: false,
  canDeleteIncident: false,
});

function readStored(): AuthUser | null {
  if (typeof window === "undefined") return null;
  const token = localStorage.getItem(STORAGE_KEYS.token);
  const username = localStorage.getItem(STORAGE_KEYS.username);
  const role = localStorage.getItem(STORAGE_KEYS.role) as Role | null;
  const expiresAtUtc = localStorage.getItem(STORAGE_KEYS.expiresAt) ?? undefined;
  if (!token || !username || !role) return null;
  if (expiresAtUtc) {
    const t = new Date(expiresAtUtc).getTime();
    if (!isNaN(t) && t < Date.now()) {
      clearSession();
      return null;
    }
  }
  return { username, role, expiresAtUtc };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [ready, setReady] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const stored = readStored();
    if (!stored) {
      setReady(true);
      return;
    }
    setUser(stored);
    // Validate token via /me, but don't block UI
    apiGet<{ username: string; role: Role }>(ENDPOINTS.authMe)
      .then((me) => {
        if (me?.username && me?.role) {
          setUser((u) => ({ ...(u ?? { username: me.username, role: me.role }), username: me.username, role: me.role }));
          localStorage.setItem(STORAGE_KEYS.username, me.username);
          localStorage.setItem(STORAGE_KEYS.role, me.role);
        }
      })
      .catch(() => {
        // 401 already handled globally
      })
      .finally(() => setReady(true));
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    setLoading(true);
    try {
      const res = await apiPost<LoginResponse>(ENDPOINTS.authLogin, { username, password }, { skipAuth: true });
      localStorage.setItem(STORAGE_KEYS.token, res.accessToken);
      localStorage.setItem(STORAGE_KEYS.username, res.username);
      localStorage.setItem(STORAGE_KEYS.role, res.role);
      localStorage.setItem(STORAGE_KEYS.expiresAt, res.expiresAtUtc);
      const u: AuthUser = { username: res.username, role: res.role, expiresAtUtc: res.expiresAtUtc };
      setUser(u);
      return u;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setUser(null);
    if (typeof window !== "undefined") window.location.replace("/login");
  }, []);

  const hasRole = useCallback((...roles: Role[]) => !!user && roles.includes(user.role), [user]);

  const canApprove = hasRole("Operator", "Admin");
  const canManageTickets = hasRole("Operator", "Admin");
  const canEditIncident = hasRole("Operator", "Admin");
  const canDeleteIncident = hasRole("Admin");

  return (
    <Ctx.Provider value={{ user, ready, loading, login, logout, hasRole, canApprove, canManageTickets, canEditIncident, canDeleteIncident }}>
      {children}
    </Ctx.Provider>
  );
}

export const useAuth = () => useContext(Ctx);
