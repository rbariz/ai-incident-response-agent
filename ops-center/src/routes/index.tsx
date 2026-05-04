import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useState, type ReactNode } from "react";
import { Card, PageHeader } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { useI18n } from "@/i18n";
import { useHubEvent, useHubReconnected } from "@/realtime/hub";
import { Activity, Cpu, AlertTriangle, Clock, CheckCircle2, XCircle, Ticket, ShieldAlert, TrendingUp, Timer, RotateCcw } from "lucide-react";
import { useApi } from "@/hooks/use-api";
import { ENDPOINTS } from "@/config/api";

export const Route = createFileRoute("/")({
  component: Dashboard,
  head: () => ({ meta: [{ title: "Dashboard — AI Incident Response" }] }),
});

type Overview = {
  totalEvents?: number; pendingEvents?: number; processedEvents?: number;
  totalExecutions?: number; succeededExecutions?: number; failedExecutions?: number; skippedExecutions?: number; pendingApprovalExecutions?: number;
  totalIncidents?: number; openIncidents?: number; resolvedIncidents?: number;
  totalTickets?: number; activeTickets?: number; blockedTickets?: number;
  successRate?: number; failureRate?: number;
};

type Technical = {
  averageExecutionDurationMs?: number;
  maxExecutionDurationMs?: number;
  totalRetries?: number;
  retryScheduledExecutions?: number;
  aiProviderUsage?: { name: string; count: number }[];
  actionUsage?: { name: string; count: number }[];
  statusDistribution?: { name: string; count: number }[];
};

function KpiCard({
  label, value, icon, tone = "primary", loading, hint,
}: {
  label: string; value: ReactNode; icon: ReactNode;
  tone?: "primary" | "info" | "warning" | "error" | "accent" | "success";
  loading?: boolean; hint?: string;
}) {
  const toneBg: Record<string, string> = {
    primary: "bg-primary/10 text-primary",
    info: "bg-info/10 text-info",
    warning: "bg-warning/15 text-warning-foreground",
    error: "bg-destructive/10 text-destructive",
    accent: "bg-accent/15 text-accent",
    success: "bg-success/10 text-success",
  };
  return (
    <Card className="p-5">
      <div className="flex items-start justify-between">
        <div className="space-y-1.5 min-w-0">
          <div className="text-xs font-medium uppercase tracking-wider text-muted-foreground">{label}</div>
          {loading ? <div className="skeleton h-8 w-20" /> : (
            <div className="text-3xl font-semibold tracking-tight text-foreground">{value}</div>
          )}
          {hint && <div className="text-xs text-muted-foreground">{hint}</div>}
        </div>
        <div className={`h-10 w-10 rounded-lg flex items-center justify-center ${toneBg[tone]}`}>{icon}</div>
      </div>
    </Card>
  );
}

function ProgressRing({ value }: { value: number }) {
  const v = Math.max(0, Math.min(100, value));
  const r = 28;
  const c = 2 * Math.PI * r;
  const dash = (v / 100) * c;
  const tone = v >= 80 ? "stroke-success" : v >= 50 ? "stroke-warning" : "stroke-destructive";
  return (
    <svg viewBox="0 0 72 72" className="h-16 w-16">
      <circle cx="36" cy="36" r={r} className="stroke-muted" strokeWidth="6" fill="none" />
      <circle cx="36" cy="36" r={r} className={tone} strokeWidth="6" fill="none"
        strokeDasharray={`${dash} ${c}`} strokeLinecap="round" transform="rotate(-90 36 36)" />
      <text x="36" y="40" textAnchor="middle" className="fill-foreground text-[12px] font-semibold">{v.toFixed(0)}%</text>
    </svg>
  );
}

function Bars({ items }: { items?: { name: string; count: number }[] }) {
  const list = items ?? [];
  const max = Math.max(1, ...list.map((i) => i.count));
  if (list.length === 0) return <div className="text-xs text-muted-foreground py-2">—</div>;
  return (
    <ul className="space-y-2">
      {list.map((it) => (
        <li key={it.name} className="text-xs">
          <div className="flex items-center justify-between mb-1">
            <span className="text-foreground font-medium truncate">{it.name}</span>
            <span className="text-muted-foreground tabular-nums">{it.count}</span>
          </div>
          <div className="h-1.5 rounded-full bg-muted overflow-hidden">
            <div className="h-full bg-primary" style={{ width: `${(it.count / max) * 100}%` }} />
          </div>
        </li>
      ))}
    </ul>
  );
}

function fmtDuration(ms?: number) {
  if (ms == null) return "—";
  if (ms < 1000) return `${ms.toFixed(0)} ms`;
  return `${(ms / 1000).toFixed(2)} s`;
}

function Dashboard() {
  const { t } = useI18n();
  const overview = useApi<Overview>(ENDPOINTS.metricsOverview);
  const technical = useApi<Technical>(ENDPOINTS.metricsTechnical);

  const refreshAll = () => { overview.refetch(); technical.refetch(); };

  useHubEvent("AgentEventCreated", refreshAll);
  useHubEvent("AgentExecutionStarted", refreshAll);
  useHubEvent("AgentExecutionCompleted", refreshAll);
  useHubEvent("AgentExecutionApprovalChanged", refreshAll);
  useHubEvent("IncidentChanged", refreshAll);
  useHubReconnected(refreshAll);

  const o = overview.data ?? {};
  const tk = technical.data ?? {};
  const successRate = (o.successRate ?? 0) <= 1 ? (o.successRate ?? 0) * 100 : (o.successRate ?? 0);
  const failureRate = (o.failureRate ?? 0) <= 1 ? (o.failureRate ?? 0) * 100 : (o.failureRate ?? 0);

  return (
    <>
      <PageHeader
        title={t("page.dashboard.title")}
        description={t("page.dashboard.desc")}
        actions={<RefreshButton onClick={refreshAll} loading={overview.loading || technical.loading} />}
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard label={t("kpi.totalEvents")} value={o.totalEvents ?? 0} icon={<Activity className="h-5 w-5" />} tone="primary" loading={overview.loading} />
        <KpiCard label={t("kpi.pendingEvents")} value={o.pendingEvents ?? 0} icon={<Clock className="h-5 w-5" />} tone="warning" loading={overview.loading} />
        <KpiCard label={t("kpi.executions")} value={o.totalExecutions ?? 0} icon={<Cpu className="h-5 w-5" />} tone="accent" loading={overview.loading} />
        <Card className="p-5">
          <div className="flex items-center justify-between gap-3">
            <div className="space-y-1 min-w-0">
              <div className="text-xs font-medium uppercase tracking-wider text-muted-foreground">{t("kpi.successRate")}</div>
              <div className="text-xs text-muted-foreground">{(o.succeededExecutions ?? 0)} / {(o.totalExecutions ?? 0)}</div>
            </div>
            <ProgressRing value={successRate} />
          </div>
        </Card>
        <KpiCard label={t("kpi.failedExecutions")} value={o.failedExecutions ?? 0} icon={<XCircle className="h-5 w-5" />} tone="error" loading={overview.loading} hint={`${failureRate.toFixed(1)}% ${t("kpi.failureRate").toLowerCase()}`} />
        <KpiCard label={t("kpi.pendingApproval")} value={o.pendingApprovalExecutions ?? 0} icon={<ShieldAlert className="h-5 w-5" />} tone="warning" loading={overview.loading} />
        <KpiCard label={t("kpi.openIncidents")} value={o.openIncidents ?? 0} icon={<AlertTriangle className="h-5 w-5" />} tone="error" loading={overview.loading} hint={`${o.resolvedIncidents ?? 0} ${t("kpi.resolvedIncidents").toLowerCase()}`} />
        <KpiCard label={t("kpi.blockedTickets")} value={o.blockedTickets ?? 0} icon={<Ticket className="h-5 w-5" />} tone="error" loading={overview.loading} hint={`${o.activeTickets ?? 0} ${t("kpi.activeTickets").toLowerCase()}`} />
      </div>

      <div className="mt-8">
        <h2 className="text-sm font-semibold text-foreground mb-3">{t("section.technicalMetrics")}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <KpiCard label={t("kpi.avgDuration")} value={fmtDuration(tk.averageExecutionDurationMs)} icon={<Timer className="h-5 w-5" />} tone="info" loading={technical.loading} />
          <KpiCard label={t("kpi.maxDuration")} value={fmtDuration(tk.maxExecutionDurationMs)} icon={<TrendingUp className="h-5 w-5" />} tone="info" loading={technical.loading} />
          <KpiCard label={t("kpi.totalRetries")} value={tk.totalRetries ?? 0} icon={<RotateCcw className="h-5 w-5" />} tone="warning" loading={technical.loading} />
          <KpiCard label={t("kpi.retryScheduled")} value={tk.retryScheduledExecutions ?? 0} icon={<Clock className="h-5 w-5" />} tone="warning" loading={technical.loading} />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
          <Card className="p-5">
            <h3 className="text-sm font-semibold text-foreground mb-3">{t("section.providerUsage")}</h3>
            <Bars items={tk.aiProviderUsage} />
          </Card>
          <Card className="p-5">
            <h3 className="text-sm font-semibold text-foreground mb-3">{t("section.actionUsage")}</h3>
            <Bars items={tk.actionUsage} />
          </Card>
          <Card className="p-5">
            <h3 className="text-sm font-semibold text-foreground mb-3">{t("section.statusDistribution")}</h3>
            <Bars items={tk.statusDistribution} />
          </Card>
        </div>
      </div>
    </>
  );
}
