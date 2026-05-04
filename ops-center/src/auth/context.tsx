import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { apiGet, apiPost, clearSession, ENDPOINTS, getRefreshToken, persistSession, type Role } from "@/config/api";

export type AuthUser = {
  username: string;
  role: Role;
  expiresAtUtc?: string;
};

type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  username: string;
  role: Role;
  expiresAtUtc: string;
};

type AuthCtx = {
  user: AuthUser | null;
  ready: boolean;
  loading: boolean;
  login: (username: string, password: string) => Promise<AuthUser>;
  logout: () => Promise<void>;
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
  logout: async () => {},
  hasRole: () => false,
  canApprove: false,
  canManageTickets: false,
  canEditIncident: false,
  canDeleteIncident: false,
});

function readStored(): AuthUser | null {
  if (typeof window === "undefined") return null;
  const username = localStorage.getItem("ops.username");
  const role = localStorage.getItem("ops.role") as Role | null;
  const expiresAtUtc = localStorage.getItem("ops.expiresAtUtc") ?? undefined;
  const access = localStorage.getItem("ops.accessToken");
  const refresh = localStorage.getItem("ops.refreshToken");
  if (!username || !role) return null;
  // Even if access is expired, keep session if refresh token still present — refresh will run.
  if (!access && !refresh) return null;
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
    apiGet<{ username: string; role: Role }>(ENDPOINTS.authMe)
      .then((me) => {
        if (me?.username && me?.role) {
          setUser((u) => ({ ...(u ?? { username: me.username, role: me.role }), username: me.username, role: me.role }));
          localStorage.setItem("ops.username", me.username);
          localStorage.setItem("ops.role", me.role);
        }
      })
      .catch(() => {
        // 401 is handled globally — will redirect to login
      })
      .finally(() => setReady(true));
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    setLoading(true);
    try {
      const res = await apiPost<LoginResponse>(ENDPOINTS.authLogin, { username, password }, { skipAuth: true });
      persistSession(res);
      const u: AuthUser = { username: res.username, role: res.role, expiresAtUtc: res.expiresAtUtc };
      setUser(u);
      return u;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = getRefreshToken();
    try {
      if (refreshToken) {
        await apiPost(ENDPOINTS.authLogout, { refreshToken }, { skipRefresh: true }).catch(() => {});
      }
    } finally {
      clearSession();
      setUser(null);
      if (typeof window !== "undefined") window.location.replace("/login");
    }
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
