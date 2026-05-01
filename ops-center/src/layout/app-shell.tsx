import { Link, Outlet, useRouterState } from "@tanstack/react-router";
import { LayoutDashboard, Activity, Cpu, AlertTriangle, GitBranch, ShieldCheck } from "lucide-react";
import { useI18n } from "@/i18n";
import { cn } from "@/lib/utils";

const NAV = [
  { to: "/", labelKey: "nav.dashboard", icon: LayoutDashboard, exact: true },
  { to: "/events", labelKey: "nav.events", icon: Activity, exact: false },
  { to: "/executions", labelKey: "nav.executions", icon: Cpu, exact: false },
  { to: "/incidents", labelKey: "nav.incidents", icon: AlertTriangle, exact: false },
  { to: "/timeline", labelKey: "nav.timeline", icon: GitBranch, exact: false },
] as const;

const PAGE_TITLE: Record<string, string> = {
  "/": "page.dashboard.title",
  "/events": "page.events.title",
  "/executions": "page.executions.title",
  "/incidents": "page.incidents.title",
  "/timeline": "page.timeline.title",
};

export function AppShell() {
  const { t, lang, setLang } = useI18n();
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const titleKey = PAGE_TITLE[pathname] ?? "app.title";

  return (
    <div className="min-h-screen bg-background flex">
      {/* Sidebar */}
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

        <div className="px-5 py-4 border-t border-sidebar-border">
          <div className="text-[11px] uppercase tracking-wider text-muted-foreground mb-1">Status</div>
          <div className="flex items-center gap-2 text-xs text-foreground">
            <span className="h-2 w-2 rounded-full bg-success animate-pulse" />
            All systems operational
          </div>
        </div>
      </aside>

      {/* Main */}
      <div className="flex-1 flex flex-col min-w-0">
        <header className="h-16 sticky top-0 z-10 bg-card/80 backdrop-blur border-b border-border shadow-card flex items-center justify-between px-6">
          <h2 className="text-base font-semibold text-foreground">{t(titleKey as any)}</h2>
          <div className="flex items-center gap-3">
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
