import { createFileRoute } from "@tanstack/react-router";
import { useApi } from "@/hooks/use-api";
import { ENDPOINTS } from "@/config/api";
import { Card, PageHeader } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { StatusBadge } from "@/components/status-badge";
import { useI18n } from "@/i18n";
import { Activity, Cpu, AlertTriangle, Clock, Sparkles, ArrowUpRight } from "lucide-react";
import type { ReactNode } from "react";

export const Route = createFileRoute("/")({
  component: Dashboard,
  head: () => ({ meta: [{ title: "Dashboard — AI Incident Response" }] }),
});

type AnyRecord = Record<string, any>;

function asArray(v: any): AnyRecord[] {
  if (Array.isArray(v)) return v;
  if (v && Array.isArray(v.items)) return v.items;
  if (v && Array.isArray(v.data)) return v.data;
  return [];
}

function KpiCard({
  label,
  value,
  icon,
  tone = "primary",
  loading,
  hint,
}: {
  label: string;
  value: ReactNode;
  icon: ReactNode;
  tone?: "primary" | "info" | "warning" | "error" | "accent";
  loading?: boolean;
  hint?: string;
}) {
  const toneBg: Record<string, string> = {
    primary: "bg-primary/10 text-primary",
    info: "bg-info/10 text-info",
    warning: "bg-warning/15 text-warning-foreground",
    error: "bg-destructive/10 text-destructive",
    accent: "bg-accent/15 text-accent",
  };
  return (
    <Card className="p-5">
      <div className="flex items-start justify-between">
        <div className="space-y-1.5">
          <div className="text-xs font-medium uppercase tracking-wider text-muted-foreground">{label}</div>
          {loading ? (
            <div className="skeleton h-8 w-20" />
          ) : (
            <div className="text-3xl font-semibold tracking-tight text-foreground">{value}</div>
          )}
          {hint && <div className="text-xs text-muted-foreground">{hint}</div>}
        </div>
        <div className={`h-10 w-10 rounded-lg flex items-center justify-center ${toneBg[tone]}`}>{icon}</div>
      </div>
    </Card>
  );
}

function Dashboard() {
  const { t } = useI18n();
  const events = useApi<any>(ENDPOINTS.events);
  const executions = useApi<any>(ENDPOINTS.executions);
  const incidents = useApi<any>(ENDPOINTS.incidents);

  const eventsList = asArray(events.data);
  const execList = asArray(executions.data);
  const incList = asArray(incidents.data);

  const pendingEvents = eventsList.filter((e) => e.processed === false).length;
  const lastProvider = execList[0]?.analysisProvider ?? execList[0]?.provider ?? "—";

  const refreshAll = () => {
    events.refetch();
    executions.refetch();
    incidents.refetch();
  };
  const anyLoading = events.loading || executions.loading || incidents.loading;

  return (
    <>
      <PageHeader
        title={t("page.dashboard.title")}
        description={t("page.dashboard.desc")}
        actions={<RefreshButton onClick={refreshAll} loading={anyLoading} />}
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard
          label={t("kpi.totalEvents")}
          value={eventsList.length}
          icon={<Activity className="h-5 w-5" />}
          tone="primary"
          loading={events.loading}
        />
        <KpiCard
          label={t("kpi.pendingEvents")}
          value={pendingEvents}
          icon={<Clock className="h-5 w-5" />}
          tone="warning"
          loading={events.loading}
        />
        <KpiCard
          label={t("kpi.executions")}
          value={execList.length}
          icon={<Cpu className="h-5 w-5" />}
          tone="accent"
          loading={executions.loading}
        />
        <KpiCard
          label={t("kpi.incidents")}
          value={incList.length}
          icon={<AlertTriangle className="h-5 w-5" />}
          tone="error"
          loading={incidents.loading}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 mt-6">
        <Card className="p-5 lg:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-sm font-semibold text-foreground">Recent executions</h3>
              <p className="text-xs text-muted-foreground">Latest AI agent activity</p>
            </div>
            <ArrowUpRight className="h-4 w-4 text-muted-foreground" />
          </div>
          {executions.loading ? (
            <div className="space-y-3">
              {[...Array(4)].map((_, i) => <div key={i} className="skeleton h-10" />)}
            </div>
          ) : execList.length === 0 ? (
            <p className="text-sm text-muted-foreground py-6 text-center">{t("state.empty")}</p>
          ) : (
            <ul className="divide-y divide-border">
              {execList.slice(0, 5).map((e, i) => (
                <li key={i} className="py-3 flex items-center justify-between gap-3">
                  <div className="min-w-0">
                    <div className="text-sm font-medium text-foreground truncate">{e.action ?? e.decision ?? "—"}</div>
                    <div className="text-xs text-muted-foreground truncate">{e.correlationId ?? ""}</div>
                  </div>
                  <StatusBadge tone={(e.status ?? "").toLowerCase() === "failed" ? "error" : (e.status ?? "").toLowerCase() === "skipped" ? "muted" : "success"}>
                    {e.status ?? "—"}
                  </StatusBadge>
                </li>
              ))}
            </ul>
          )}
        </Card>

        <Card className="p-5">
          <div className="flex items-center gap-2 mb-3">
            <Sparkles className="h-4 w-4 text-accent" />
            <h3 className="text-sm font-semibold text-foreground">{t("kpi.lastProvider")}</h3>
          </div>
          <div className="text-2xl font-semibold text-foreground capitalize">{String(lastProvider)}</div>
          <p className="text-xs text-muted-foreground mt-1">Detected from latest execution</p>
          <div className="mt-4">
            <StatusBadge tone={String(lastProvider).toLowerCase() === "ollama" ? "accent" : "muted"}>
              {String(lastProvider)}
            </StatusBadge>
          </div>
        </Card>
      </div>
    </>
  );
}
