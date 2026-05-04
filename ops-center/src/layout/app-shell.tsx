import { Link, Outlet, useNavigate, useRouterState } from "@tanstack/react-router";
import { useEffect } from "react";
import { LayoutDashboard, Activity, Cpu, AlertTriangle, GitBranch, ShieldCheck, Ticket, LogOut, User, FileSearch } from "lucide-react";
import { useI18n } from "@/i18n";
import { cn } from "@/lib/utils";
import { ConnectionStatus } from "@/components/connection-status";
import { useAuth } from "@/auth/context";
import { StatusBadge } from "@/components/status-badge";

const NAV = [
  { to: "/", labelKey: "nav.dashboard", icon: LayoutDashboard, exact: true },
  { to: "/events", labelKey: "nav.events", icon: Activity, exact: false },
  { to: "/executions", labelKey: "nav.executions", icon: Cpu, exact: false },
  { to: "/incidents", labelKey: "nav.incidents", icon: AlertTriangle, exact: false },
  { to: "/tickets", labelKey: "nav.tickets", icon: Ticket, exact: false },
  { to: "/timeline", labelKey: "nav.timeline", icon: GitBranch, exact: false },
  { to: "/audit-logs", labelKey: "nav.auditLogs", icon: FileSearch, exact: false },
] as const;

const PAGE_TITLE: Record<string, string> = {
  "/": "page.dashboard.title",
  "/events": "page.events.title",
  "/executions": "page.executions.title",
  "/incidents": "page.incidents.title",
  "/tickets": "page.tickets.title",
  "/timeline": "page.timeline.title",
  "/audit-logs": "page.auditLogs.title",
};

export function AppShell() {
  const { t, lang, setLang } = useI18n();
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const titleKey = PAGE_TITLE[pathname] ?? "app.title";
  const { user, ready, logout } = useAuth();
  const navigate = useNavigate();
  const isLoginRoute = pathname === "/login";

  useEffect(() => {
    if (!ready) return;
    if (!user && !isLoginRoute) {
      const redirect = encodeURIComponent(pathname);
      navigate({ to: "/login", search: { redirect, reason: "" } as any });
    }
  }, [ready, user, isLoginRoute, pathname, navigate]);

  if (isLoginRoute) {
    return <Outlet />;
  }

  if (!ready || !user) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="h-8 w-8 rounded-full border-2 border-primary border-t-transparent animate-spin" />
      </div>
    );
  }

  const roleLabel =
    user.role === "Admin" ? t("auth.role.admin") : user.role === "Operator" ? t("auth.role.operator") : t("auth.role.viewer");
  const roleTone = user.role === "Admin" ? "primary" : user.role === "Operator" ? "info" : "muted";

  return (
    <div className="min-h-screen bg-background flex">
      <aside className="hidden md:flex flex-col w-64 shrink-0 border-r border-sidebar-border bg-sidebar">
        <div className="h-16 flex items-center gap-2.5 px-5 border-b border-sidebar-border">
          <div className="h-9 w-9 rounded-lg bg-primary text-primary-foreground flex items-center justify-center shadow-card">
            <ShieldCheck className="h-5 w-5" />
          </div>
          <div className="leading-tight">
            <div className="text-sm font-semibold text-foreground">{t("app.title")}</div>
            <div className="text-[11px] text-muted-foreground">{t("app.subtitle")}</div>
          </div>
        </div>

        <nav className="flex-1 px-3 py-4 space-y-1">
          {NAV.map((item) => {
            const active = item.exact ? pathname === item.to : pathname.startsWith(item.to);
            const Icon = item.icon;
            return (
              <Link
                key={item.to}
                to={item.to as string}
                className={cn(
                  "group flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all",
                  active
                    ? "bg-sidebar-accent text-sidebar-accent-foreground shadow-card"
                    : "text-sidebar-foreground hover:bg-sidebar-accent/60 hover:text-foreground"
                )}
              >
                <Icon className={cn("h-4 w-4 transition-colors", active ? "text-primary" : "text-muted-foreground group-hover:text-foreground")} />
                <span>{t(item.labelKey as any)}</span>
                {active && <span className="ml-auto h-1.5 w-1.5 rounded-full bg-primary" />}
              </Link>
            );
          })}
        </nav>

        <div className="px-4 py-4 border-t border-sidebar-border space-y-3">
          <div className="flex items-center gap-2.5">
            <div className="h-8 w-8 rounded-full bg-muted flex items-center justify-center text-muted-foreground">
              <User className="h-4 w-4" />
            </div>
            <div className="min-w-0 flex-1">
              <div className="text-xs font-medium text-foreground truncate">{user.username}</div>
              <div className="mt-0.5"><StatusBadge tone={roleTone as any} dot={false} className="px-1.5 py-0 text-[10px]">{roleLabel}</StatusBadge></div>
            </div>
            <button
              onClick={() => { void logout(); }}
              title={t("auth.logout")}
              className="rounded-md p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        </div>
      </aside>

      <div className="flex-1 flex flex-col min-w-0">
        <header className="h-16 sticky top-0 z-10 bg-card/80 backdrop-blur border-b border-border shadow-card flex items-center justify-between px-6">
          <h2 className="text-base font-semibold text-foreground">{t(titleKey as any)}</h2>
          <div className="flex items-center gap-3">
            <ConnectionStatus />
            <div className="inline-flex rounded-lg border border-border bg-muted p-0.5 text-xs font-medium">
              {(["fr", "en"] as const).map((l) => (
                <button
                  key={l}
                  onClick={() => setLang(l)}
                  className={cn(
                    "px-2.5 py-1 rounded-md transition-all",
                    lang === l ? "bg-card text-foreground shadow-card" : "text-muted-foreground hover:text-foreground"
                  )}
                >
                  {l.toUpperCase()}
                </button>
              ))}
            </div>
          </div>
        </header>

        <main className="flex-1 px-6 py-8 max-w-[1400px] w-full mx-auto">
          <div className="fade-in">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
