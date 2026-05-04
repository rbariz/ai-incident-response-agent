import { createFileRoute } from "@tanstack/react-router";
import { useApi } from "@/hooks/use-api";
import { ENDPOINTS } from "@/config/api";
import { Card, EmptyState, ErrorState, PageHeader } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { useI18n } from "@/i18n";
import { useHubEvent, useHubReconnected } from "@/realtime/hub";
import { Activity, Cpu, AlertTriangle } from "lucide-react";
import { formatDate } from "@/components/data-table";

export const Route = createFileRoute("/timeline")({
  component: TimelinePage,
  head: () => ({ meta: [{ title: "Timeline — AI Incident Response" }] }),
});

type TimelineItem = {
  itemType?: string;
  type?: string;
  kind?: string;
  category?: string;
  title?: string;
  message?: string;
  description?: string;
  eventType?: string;
  action?: string;
  decision?: string;
  createdAtUtc?: string;
  timestamp?: string;
  correlationId?: string;
};

function classifyType(t?: string) {
  const v = (t ?? "").toLowerCase();
  if (v.includes("incident")) return { tone: "incident", color: "bg-destructive", ring: "ring-destructive/30", text: "text-destructive", Icon: AlertTriangle, labelKey: "entity.incident" as const };
  if (v.includes("execution") || v.includes("action") || v.includes("decision")) return { tone: "execution", color: "bg-accent", ring: "ring-accent/30", text: "text-accent", Icon: Cpu, labelKey: "entity.execution" as const };
  return { tone: "event", color: "bg-info", ring: "ring-info/30", text: "text-info", Icon: Activity, labelKey: "entity.event" as const };
}

function titleFor(it: TimelineItem): string {
  const kind = (it.itemType ?? it.type ?? it.kind ?? it.category ?? "").toLowerCase();
  if (kind === "event") return it.eventType ?? it.type ?? it.title ?? it.message ?? "—";
  if (kind === "execution") return it.action ?? it.decision ?? it.title ?? "—";
  if (kind === "incident") return it.title ?? it.message ?? "—";
  return it.title ?? it.message ?? it.description ?? it.eventType ?? it.action ?? "—";
}

function TimelinePage() {
  const { t } = useI18n();
  const { data, loading, error, refetch } = useApi<any>(ENDPOINTS.timeline);
  const items: TimelineItem[] = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);

  useHubEvent("AgentEventCreated", () => refetch());
  useHubEvent("AgentExecutionStarted", () => refetch());
  useHubEvent("AgentExecutionCompleted", () => refetch());
  useHubEvent("IncidentChanged", () => refetch());
  useHubEvent("AgentExecutionApprovalChanged", () => refetch());
  useHubReconnected(() => refetch());

  return (
    <>
      <PageHeader
        title={t("page.timeline.title")}
        description={t("page.timeline.desc")}
        actions={<RefreshButton onClick={refetch} loading={loading} />}
      />

      <Card className="p-6">
        {loading ? (
          <div className="space-y-4">
            {[...Array(6)].map((_, i) => (
              <div key={i} className="flex gap-4">
                <div className="skeleton h-9 w-9 rounded-full" />
                <div className="flex-1 space-y-2">
                  <div className="skeleton h-3 w-1/3" />
                  <div className="skeleton h-3 w-2/3" />
                </div>
              </div>
            ))}
          </div>
        ) : error ? (
          <ErrorState message={t("state.error")} onRetry={refetch} retryLabel={t("state.retry")} />
        ) : items.length === 0 ? (
          <EmptyState message={t("state.empty")} />
        ) : (
          <ol className="relative">
            <span className="absolute left-[18px] top-2 bottom-2 w-px bg-border" aria-hidden />
            {items.map((it, i) => {
              const meta = classifyType(it.itemType ?? it.type ?? it.kind ?? it.category);
              const when = it.createdAtUtc ?? it.timestamp;
              const Icon = meta.Icon;
              return (
                <li key={i} className="relative pl-12 pb-6 last:pb-0">
                  <span className={`absolute left-0 top-1 h-9 w-9 rounded-full bg-card ring-4 ${meta.ring} flex items-center justify-center`}>
                    <span className={`h-2.5 w-2.5 rounded-full ${meta.color}`} />
                  </span>
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <Icon className={`h-3.5 w-3.5 ${meta.text}`} />
                        <span className={`text-[11px] font-semibold uppercase tracking-wider ${meta.text}`}>{t(meta.labelKey)}</span>
                      </div>
                      <div className="mt-1 text-sm font-medium text-foreground">
                        {titleFor(it)}
                      </div>
                      {it.correlationId && (
                        <div className="mt-0.5 font-mono text-xs text-muted-foreground truncate">{it.correlationId}</div>
                      )}
                    </div>
                    <span className="text-xs text-muted-foreground whitespace-nowrap shrink-0">{formatDate(when)}</span>
                  </div>
                </li>
              );
            })}
          </ol>
        )}
      </Card>
    </>
  );
}
