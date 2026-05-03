import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { Check, Ban } from "lucide-react";
import { toast } from "sonner";
import { ENDPOINTS, apiPost } from "@/config/api";
import { usePagedApi } from "@/hooks/use-paged-api";
import { PageHeader, Card, EmptyState, ErrorState, TableSkeleton } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { formatDate, Mono } from "@/components/data-table";
import { StatusBadge, toneForStatus } from "@/components/status-badge";
import { Pagination } from "@/components/pagination";
import { DetailDrawer, DetailField, DetailSection, CodeBlock } from "@/components/detail-drawer";
import { ExecutionTimeline } from "@/components/execution-timeline";
import { ApprovalModal, type ApprovalMode } from "@/components/approval-modal";
import { useI18n } from "@/i18n";
import { useHubEvent, rtToast } from "@/realtime/hub";
import { useAuth } from "@/auth/context";

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
      <div className="h-1.5 flex-1 rounded-full bg-muted overflow-hidden"><div className={`h-full ${tone}`} style={{ width: `${pct}%` }} /></div>
      <span className="text-xs font-medium text-foreground tabular-nums w-10 text-right">{pct.toFixed(0)}%</span>
    </div>
  );
}

function ExecutionsPage() {
  const { t } = useI18n();
  const { canApprove } = useAuth();
  const paged = usePagedApi<any>(ENDPOINTS.executions);
  const [selected, setSelected] = useState<any | null>(null);
  const [approvalMode, setApprovalMode] = useState<ApprovalMode | null>(null);
  const [approvalLoading, setApprovalLoading] = useState(false);

  useEffect(() => {
    if (!selected?.id) return;
    const fresh = paged.items.find((r: any) => r.id === selected.id);
    if (fresh && JSON.stringify(fresh) !== JSON.stringify(selected)) setSelected(fresh);
  }, [paged.items, selected]);

  useHubEvent("AgentExecutionStarted", () => { rtToast(t("rt.execution.started")); paged.refetch(); });
  useHubEvent("AgentExecutionCompleted", (p: any) => {
    const desc = [p?.action, p?.status].filter(Boolean).join(" · ");
    rtToast(t("rt.execution.completed"), desc || undefined);
    paged.refetch();
  });
  useHubEvent("AgentExecutionApprovalChanged", (p: any) => {
    const desc = [p?.action, p?.status].filter(Boolean).join(" · ");
    rtToast(t("rt.execution.approvalChanged"), desc || undefined);
    paged.refetch();
  });

  const isPending = (s?: string | null) => (s ?? "").toLowerCase().replace(/[_-]/g, "") === "pendingapproval";
  const isRetry = (s?: string | null) => (s ?? "").toLowerCase().replace(/[_-]/g, "") === "retryscheduled";

  const submitApproval = async (reason: string) => {
    if (!selected || !approvalMode) return;
    const isApprove = approvalMode === "approve";
    const path = isApprove ? `/api/agent-executions/${selected.id}/approve` : `/api/agent-executions/${selected.id}/reject`;
    setApprovalLoading(true);
    try {
      await apiPost(path, { reason: reason || (isApprove ? "Approved by operator." : "") });
      toast.success(isApprove ? t("approval.toast.approved") : t("approval.toast.rejected"));
      setApprovalMode(null);
      await paged.refetch();
    } catch (e: any) {
      toast.error(isApprove ? t("approval.toast.approveFailed") : t("approval.toast.rejectFailed"), { description: e?.message });
    } finally {
      setApprovalLoading(false);
    }
  };

  const headers = [t("col.status"), t("col.decision"), t("col.action"), t("col.provider"), t("col.confidence"), t("col.correlationId"), t("col.createdAt")];

  return (
    <>
      <PageHeader title={t("page.executions.title")} description={t("page.executions.desc")} actions={<RefreshButton onClick={paged.refetch} loading={paged.loading} />} />
      <Card className="overflow-hidden">
        {paged.loading ? <TableSkeleton cols={7} /> :
          paged.error ? <ErrorState message={t("state.error")} onRetry={paged.refetch} retryLabel={t("state.retry")} /> :
          paged.items.length === 0 ? <EmptyState message={t("state.empty")} /> : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="bg-muted/50 border-b border-border"><tr>{headers.map((h) => <th key={h} className="text-left px-5 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">{h}</th>)}</tr></thead>
              <tbody className="divide-y divide-border">
                {paged.items.map((r: any, i: number) => (
                  <tr key={r.id ?? i} onClick={() => setSelected(r)} className="hover:bg-muted/40 transition-colors cursor-pointer">
                    <td className="px-5 py-3.5"><StatusBadge tone={toneForStatus(r.status)}>{r.status ?? "—"}</StatusBadge></td>
                    <td className="px-5 py-3.5 font-medium text-foreground">{r.decision ?? "—"}</td>
                    <td className="px-5 py-3.5 text-muted-foreground">{r.action ?? "—"}</td>
                    <td className="px-5 py-3.5">{(() => { const p = r.analysisProvider ?? r.provider; return <StatusBadge tone={String(p).toLowerCase() === "ollama" ? "accent" : "muted"}>{p ?? "—"}</StatusBadge>; })()}</td>
                    <td className="px-5 py-3.5"><Confidence value={r.confidenceScore} /></td>
                    <td className="px-5 py-3.5"><Mono>{r.correlationId}</Mono></td>
                    <td className="px-5 py-3.5 text-muted-foreground">{formatDate(r.createdAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={paged.page} pageSize={paged.pageSize} totalPages={paged.totalPages} totalCount={paged.totalCount} onPageChange={paged.setPage} onPageSizeChange={paged.setPageSize} />
      </Card>

      <DetailDrawer
        open={!!selected}
        onClose={() => setSelected(null)}
        title={selected?.decision ?? "Execution"}
        subtitle={selected?.correlationId ? `Correlation: ${selected.correlationId}` : undefined}
      >
        {selected && (
          <>
            {isPending(selected.status) && canApprove && (
              <div className="mb-5 rounded-lg border border-warning/30 bg-warning/5 p-4">
                <div className="flex items-start justify-between gap-4 flex-wrap">
                  <div className="min-w-0">
                    <StatusBadge tone="warning">{t("approval.pending")}</StatusBadge>
                    <p className="mt-1.5 text-xs text-muted-foreground">
                      {selected.action && <span className="font-medium text-foreground">{selected.action}</span>}
                      {selected.action && selected.decision ? " · " : null}
                      {selected.decision ?? null}
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <button onClick={() => setApprovalMode("reject")} className="inline-flex items-center gap-1.5 rounded-md border border-destructive/30 bg-background px-3 py-1.5 text-xs font-semibold text-destructive hover:bg-destructive/10 transition-colors"><Ban className="h-3.5 w-3.5" />{t("approval.reject")}</button>
                    <button onClick={() => setApprovalMode("approve")} className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-primary-foreground shadow-sm hover:bg-primary/90 transition-colors"><Check className="h-3.5 w-3.5" />{t("approval.approve")}</button>
                  </div>
                </div>
              </div>
            )}

            {isRetry(selected.status) && (
              <div className="mb-5 rounded-lg border border-warning/30 bg-warning/5 p-3 text-xs text-foreground">
                <span className="font-medium">{t("retry.scheduled")}</span>
                {selected.nextRetryAtUtc && <> · {t("retry.in")} <span className="font-mono">{formatDate(selected.nextRetryAtUtc)}</span></>}
              </div>
            )}

            <DetailSection title={t("detail.section.overview")}>
              <DetailField label={t("field.status")}><StatusBadge tone={toneForStatus(selected.status)}>{selected.status ?? "—"}</StatusBadge></DetailField>
              <DetailField label={t("field.decision")}>{selected.decision}</DetailField>
              <DetailField label={t("field.action")}>{selected.action}</DetailField>
              <DetailField label={t("field.provider")}>{(() => { const p = selected.analysisProvider ?? selected.provider; return <StatusBadge tone={String(p).toLowerCase() === "ollama" ? "accent" : "muted"}>{p ?? "—"}</StatusBadge>; })()}</DetailField>
              <DetailField label={t("field.confidence")}><Confidence value={selected.confidenceScore} /></DetailField>
              <DetailField label={t("field.analysisLanguage")}>{selected.analysisLanguage ? <StatusBadge tone="muted">{String(selected.analysisLanguage).toUpperCase()}</StatusBadge> : null}</DetailField>
            </DetailSection>

            <DetailSection title={t("detail.section.retry")}>
              <DetailField label={t("field.retryCount")}><span className="font-mono tabular-nums">{selected.retryCount ?? 0}</span></DetailField>
              <DetailField label={t("field.nextRetryAt")}>{formatDate(selected.nextRetryAtUtc)}</DetailField>
              <DetailField label={t("field.lastRetryAt")}>{formatDate(selected.lastRetryAtUtc)}</DetailField>
            </DetailSection>

            <DetailSection title={t("detail.section.timing")}>
              <DetailField label={t("field.createdAt")}>{formatDate(selected.createdAtUtc)}</DetailField>
              <DetailField label={t("field.startedAt")}>{formatDate(selected.startedAtUtc)}</DetailField>
              <DetailField label={t("field.completedAt")}>{formatDate(selected.completedAtUtc)}</DetailField>
              {(selected.approvedAtUtc || selected.approvedAt) && <DetailField label={t("approval.field.approvedAt")}>{formatDate(selected.approvedAtUtc ?? selected.approvedAt)}</DetailField>}
              {(selected.rejectedAtUtc || selected.rejectedAt) && <DetailField label={t("approval.field.rejectedAt")}>{formatDate(selected.rejectedAtUtc ?? selected.rejectedAt)}</DetailField>}
              {(selected.approvalReason || selected.rejectionReason) && <DetailField label={t("approval.field.reason")}><span className="whitespace-pre-wrap">{selected.approvalReason ?? selected.rejectionReason}</span></DetailField>}
            </DetailSection>

            <AiSummary execution={selected} />

            {selected.errorMessage && (
              <section className="mt-6">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-destructive mb-3">{t("detail.section.errorMessage")}</h3>
                <div className="rounded-lg border border-destructive/30 bg-destructive/5 p-4 text-sm font-mono text-destructive whitespace-pre-wrap break-words">{selected.errorMessage}</div>
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

      <ApprovalModal
        open={!!approvalMode}
        mode={approvalMode ?? "approve"}
        execution={selected}
        loading={approvalLoading}
        onClose={() => !approvalLoading && setApprovalMode(null)}
        onConfirm={submitApproval}
      />
    </>
  );
}
