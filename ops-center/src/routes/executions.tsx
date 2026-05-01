import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { useApi } from "@/hooks/use-api";
import { ENDPOINTS } from "@/config/api";
import { PageHeader } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { DataTable, formatDate, Mono } from "@/components/data-table";
import { StatusBadge, toneForStatus } from "@/components/status-badge";
import { DetailDrawer, DetailField, DetailSection, CodeBlock } from "@/components/detail-drawer";
import { ExecutionTimeline } from "@/components/execution-timeline";
import { useI18n } from "@/i18n";

function AiSummary({ execution }: { execution: any }) {
  const { lang, t } = useI18n();
  const summary = lang === "fr" ? execution.analysisSummaryFr : execution.analysisSummaryEn;
  const original = (execution.analysisLanguage ?? "").toLowerCase();
  const isOriginal = original && original !== lang;
  const label = lang === "fr" ? t("detail.aiSummary.fr") : t("detail.aiSummary.en");

  return (
    <section className="mt-6">
      <div className="flex items-center gap-2 mb-3">
        <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">{label}</h3>
        {isOriginal && (
          <span className="inline-flex items-center rounded-full bg-accent/15 px-2 py-0.5 text-[10px] font-medium text-accent ring-1 ring-inset ring-accent/30">
            {original.toUpperCase()} · {t("detail.aiSummary.originalLang")}
          </span>
        )}
      </div>
      <div className="rounded-lg border border-border bg-background/40 p-4 text-sm leading-relaxed text-foreground whitespace-pre-wrap">
        {summary || <span className="text-muted-foreground italic">{t("detail.aiSummary.unavailable")}</span>}
      </div>
    </section>
  );
}

export const Route = createFileRoute("/executions")({
  component: ExecutionsPage,
  head: () => ({ meta: [{ title: "Executions — AI Incident Response" }] }),
});

function Confidence({ value }: { value?: number | null }) {
  if (value == null || isNaN(Number(value))) return <span className="text-muted-foreground">—</span>;
  const pct = Math.max(0, Math.min(100, Number(value) <= 1 ? Number(value) * 100 : Number(value)));
  const tone = pct >= 75 ? "bg-success" : pct >= 40 ? "bg-warning" : "bg-destructive";
  return (
    <div className="flex items-center gap-2 min-w-[120px]">
      <div className="h-1.5 flex-1 rounded-full bg-muted overflow-hidden">
        <div className={`h-full ${tone} transition-all`} style={{ width: `${pct}%` }} />
      </div>
      <span className="text-xs font-medium text-foreground tabular-nums w-10 text-right">{pct.toFixed(0)}%</span>
    </div>
  );
}

function ExecutionsPage() {
  const { t } = useI18n();
  const { data, loading, error, refetch } = useApi<any>(ENDPOINTS.executions);
  const rows = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);
  const [selected, setSelected] = useState<any | null>(null);

  return (
    <>
      <PageHeader
        title={t("page.executions.title")}
        description={t("page.executions.desc")}
        actions={<RefreshButton onClick={refetch} loading={loading} />}
      />
      <DataTable
        loading={loading}
        error={error}
        onRetry={refetch}
        rows={rows}
        onRowClick={(r) => setSelected(r)}
        columns={[
          {
            key: "status",
            header: t("col.status"),
            render: (r) => <StatusBadge tone={toneForStatus(r.status)}>{r.status ?? "—"}</StatusBadge>,
          },
          { key: "decision", header: t("col.decision"), render: (r) => <span className="font-medium text-foreground">{r.decision ?? "—"}</span> },
          { key: "action", header: t("col.action"), render: (r) => <span className="text-muted-foreground">{r.action ?? "—"}</span> },
          {
            key: "provider",
            header: t("col.provider"),
            render: (r) => {
              const p = r.analysisProvider ?? r.provider;
              return <StatusBadge tone={String(p).toLowerCase() === "ollama" ? "accent" : "muted"}>{p ?? "—"}</StatusBadge>;
            },
          },
          { key: "confidence", header: t("col.confidence"), render: (r) => <Confidence value={r.confidenceScore} /> },
          { key: "correlationId", header: t("col.correlationId"), render: (r) => <Mono>{r.correlationId}</Mono> },
          { key: "createdAtUtc", header: t("col.createdAt"), render: (r) => <span className="text-muted-foreground">{formatDate(r.createdAtUtc)}</span> },
        ]}
      />

      <DetailDrawer
        open={!!selected}
        onClose={() => setSelected(null)}
        title={selected?.decision ?? "Execution"}
        subtitle={selected?.correlationId ? `Correlation: ${selected.correlationId}` : undefined}
      >
        {selected && (
          <>
            <DetailSection title={t("detail.section.overview")}>
              <DetailField label={t("field.status")}>
                <StatusBadge tone={toneForStatus(selected.status)}>{selected.status ?? "—"}</StatusBadge>
              </DetailField>
              <DetailField label={t("field.decision")}>{selected.decision}</DetailField>
              <DetailField label={t("field.action")}>{selected.action}</DetailField>
              <DetailField label={t("field.provider")}>
                {(() => {
                  const p = selected.analysisProvider ?? selected.provider;
                  return <StatusBadge tone={String(p).toLowerCase() === "ollama" ? "accent" : "muted"}>{p ?? "—"}</StatusBadge>;
                })()}
              </DetailField>
              <DetailField label={t("field.confidence")}>
                <Confidence value={selected.confidenceScore} />
              </DetailField>
              <DetailField label={t("field.retryCount")}>
                <span className="font-mono tabular-nums">{selected.retryCount ?? 0}</span>
              </DetailField>
              <DetailField label={t("field.analysisLanguage")}>
                {selected.analysisLanguage ? (
                  <StatusBadge tone="muted">{String(selected.analysisLanguage).toUpperCase()}</StatusBadge>
                ) : null}
              </DetailField>
            </DetailSection>

            <DetailSection title={t("detail.section.timing")}>
              <DetailField label={t("field.createdAt")}>{formatDate(selected.createdAtUtc)}</DetailField>
              <DetailField label={t("field.startedAt")}>{formatDate(selected.startedAtUtc)}</DetailField>
              <DetailField label={t("field.completedAt")}>{formatDate(selected.completedAtUtc)}</DetailField>
            </DetailSection>

            <AiSummary execution={selected} />

            {selected.errorMessage && (
              <section className="mt-6">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-destructive mb-3">{t("detail.section.errorMessage")}</h3>
                <div className="rounded-lg border border-destructive/30 bg-destructive/5 p-4 text-sm font-mono text-destructive whitespace-pre-wrap break-words">
                  {selected.errorMessage}
                </div>
              </section>
            )}

            {(selected.resultJson ?? selected.result) != null && (
              <section className="mt-6">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground mb-3">{t("detail.section.resultJson")}</h3>
                <CodeBlock value={selected.resultJson ?? selected.result} />
              </section>
            )}

            <section className="mt-6">
              <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground mb-3">{t("detail.section.timeline")}</h3>
              <div className="rounded-lg border border-border bg-background/40">
                <ExecutionTimeline correlationId={selected.correlationId} />
              </div>
            </section>
          </>
        )}
      </DetailDrawer>
    </>
  );
}
