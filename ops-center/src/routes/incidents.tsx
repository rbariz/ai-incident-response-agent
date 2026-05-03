import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { Pencil, CheckCircle2, RotateCcw, Trash2, Loader2, X } from "lucide-react";
import { toast } from "sonner";
import { ENDPOINTS, apiPost, apiPut, apiDelete } from "@/config/api";
import { usePagedApi } from "@/hooks/use-paged-api";
import { PageHeader, Card, EmptyState, ErrorState, TableSkeleton } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { formatDate } from "@/components/data-table";
import { StatusBadge, toneForSeverity, toneForStatus } from "@/components/status-badge";
import { Pagination } from "@/components/pagination";
import { DetailDrawer, DetailField, DetailSection } from "@/components/detail-drawer";
import { useI18n } from "@/i18n";
import { useHubEvent, rtToast } from "@/realtime/hub";
import { useAuth } from "@/auth/context";

export const Route = createFileRoute("/incidents")({
  component: IncidentsPage,
  head: () => ({ meta: [{ title: "Incidents — AI Incident Response" }] }),
});

const SEVERITIES = ["Low", "Medium", "High", "Critical"] as const;

function IncidentsPage() {
  const { t } = useI18n();
  const { canEditIncident, canDeleteIncident } = useAuth();
  const paged = usePagedApi<any>(ENDPOINTS.incidents);
  const [selected, setSelected] = useState<any | null>(null);
  const [editOpen, setEditOpen] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (!selected?.id) return;
    const fresh = paged.items.find((r: any) => r.id === selected.id);
    if (fresh && JSON.stringify(fresh) !== JSON.stringify(selected)) setSelected(fresh);
  }, [paged.items, selected]);

  useHubEvent("IncidentChanged", () => { rtToast(t("rt.incident.changed")); paged.refetch(); });
  useHubEvent("AgentExecutionCompleted", () => paged.refetch());
  useHubEvent("AgentExecutionApprovalChanged", () => paged.refetch());

  const isResolved = (s?: string | null) => (s ?? "").toLowerCase() === "resolved";

  const callOp = async (op: () => Promise<any>, successKey: "incident.toast.updated" | "incident.toast.resolved" | "incident.toast.reopened" | "incident.toast.deleted") => {
    setBusy(true);
    try {
      await op();
      toast.success(t(successKey));
      await paged.refetch();
    } catch (err: any) {
      toast.error(t("incident.toast.failed"), { description: err?.message });
    } finally {
      setBusy(false);
    }
  };

  const onResolve = () => selected && callOp(() => apiPost(`${ENDPOINTS.incidents}/${selected.id}/resolve`), "incident.toast.resolved");
  const onReopen = () => selected && callOp(() => apiPost(`${ENDPOINTS.incidents}/${selected.id}/reopen`), "incident.toast.reopened");
  const onDelete = async () => {
    if (!selected) return;
    if (!confirm(t("incident.confirmDelete"))) return;
    await callOp(() => apiDelete(`${ENDPOINTS.incidents}/${selected.id}`), "incident.toast.deleted");
    setSelected(null);
  };

  return (
    <>
      <PageHeader title={t("page.incidents.title")} description={t("page.incidents.desc")} actions={<RefreshButton onClick={paged.refetch} loading={paged.loading} />} />
      <Card className="overflow-hidden">
        {paged.loading ? <TableSkeleton cols={5} /> :
          paged.error ? <ErrorState message={t("state.error")} onRetry={paged.refetch} retryLabel={t("state.retry")} /> :
          paged.items.length === 0 ? <EmptyState message={t("state.empty")} /> : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="bg-muted/50 border-b border-border"><tr>
                {[t("col.title"), t("col.severity"), t("col.status"), t("col.createdAt"), t("col.resolvedAt")].map((h) =>
                  <th key={h} className="text-left px-5 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">{h}</th>)}
              </tr></thead>
              <tbody className="divide-y divide-border">
                {paged.items.map((r: any) => (
                  <tr key={r.id} onClick={() => setSelected(r)} className="hover:bg-muted/40 transition-colors cursor-pointer">
                    <td className="px-5 py-3.5 font-medium text-foreground">{r.title ?? "—"}</td>
                    <td className="px-5 py-3.5"><StatusBadge tone={toneForSeverity(r.severity)}>{r.severity ?? "—"}</StatusBadge></td>
                    <td className="px-5 py-3.5"><StatusBadge tone={toneForStatus(r.status)}>{r.status ?? "—"}</StatusBadge></td>
                    <td className="px-5 py-3.5 text-muted-foreground">{formatDate(r.createdAtUtc)}</td>
                    <td className="px-5 py-3.5 text-muted-foreground">{formatDate(r.resolvedAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={paged.page} pageSize={paged.pageSize} totalPages={paged.totalPages} totalCount={paged.totalCount} onPageChange={paged.setPage} onPageSizeChange={paged.setPageSize} />
      </Card>

      <DetailDrawer open={!!selected} onClose={() => setSelected(null)} title={selected?.title ?? "Incident"} subtitle={selected?.id ? `ID: ${selected.id}` : undefined}>
        {selected && (
          <>
            {(canEditIncident || canDeleteIncident) && (
              <div className="mb-5 flex flex-wrap items-center gap-2">
                {canEditIncident && (
                  <button onClick={() => setEditOpen(true)} disabled={busy} className="inline-flex items-center gap-1.5 rounded-md border border-border bg-background px-3 py-1.5 text-xs font-semibold text-foreground hover:bg-muted disabled:opacity-50">
                    <Pencil className="h-3.5 w-3.5" />{t("action.edit")}
                  </button>
                )}
                {canEditIncident && !isResolved(selected.status) && (
                  <button onClick={onResolve} disabled={busy} className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-xs font-semibold text-white hover:opacity-90 disabled:opacity-50">
                    <CheckCircle2 className="h-3.5 w-3.5" />{t("action.resolve")}
                  </button>
                )}
                {canEditIncident && isResolved(selected.status) && (
                  <button onClick={onReopen} disabled={busy} className="inline-flex items-center gap-1.5 rounded-md border border-info/30 bg-info/10 px-3 py-1.5 text-xs font-semibold text-info hover:bg-info/20 disabled:opacity-50">
                    <RotateCcw className="h-3.5 w-3.5" />{t("action.reopen")}
                  </button>
                )}
                {canDeleteIncident && (
                  <button onClick={onDelete} disabled={busy} className="ml-auto inline-flex items-center gap-1.5 rounded-md border border-destructive/30 bg-background px-3 py-1.5 text-xs font-semibold text-destructive hover:bg-destructive/10 disabled:opacity-50">
                    <Trash2 className="h-3.5 w-3.5" />{t("action.delete")}
                  </button>
                )}
              </div>
            )}

            <DetailSection title={t("detail.section.overview")}>
              <DetailField label={t("field.title")}><span className="font-medium">{selected.title}</span></DetailField>
              <DetailField label={t("field.severity")}><StatusBadge tone={toneForSeverity(selected.severity)}>{selected.severity ?? "—"}</StatusBadge></DetailField>
              <DetailField label={t("field.status")}><StatusBadge tone={toneForStatus(selected.status)}>{selected.status ?? "—"}</StatusBadge></DetailField>
            </DetailSection>

            <DetailSection title={t("detail.section.timing")}>
              <DetailField label={t("field.createdAt")}>{formatDate(selected.createdAtUtc)}</DetailField>
              <DetailField label={t("field.resolvedAt")}>{formatDate(selected.resolvedAtUtc)}</DetailField>
            </DetailSection>

            {selected.description && (
              <section className="mt-6">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground mb-3">{t("detail.section.description")}</h3>
                <div className="rounded-lg border border-border bg-background/40 p-4 text-sm leading-relaxed text-foreground whitespace-pre-wrap">{selected.description}</div>
              </section>
            )}
          </>
        )}
      </DetailDrawer>

      <EditIncidentModal
        open={editOpen}
        incident={selected}
        onClose={() => setEditOpen(false)}
        onSaved={async (updated) => {
          setEditOpen(false);
          if (selected) {
            await callOp(() => apiPut(`${ENDPOINTS.incidents}/${selected.id}`, updated), "incident.toast.updated");
          }
        }}
      />
    </>
  );
}

function EditIncidentModal({
  open, incident, onClose, onSaved,
}: {
  open: boolean;
  incident: any | null;
  onClose: () => void;
  onSaved: (data: { title: string; description: string; severity: string }) => Promise<void> | void;
}) {
  const { t } = useI18n();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [severity, setSeverity] = useState<string>("Medium");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (open && incident) {
      setTitle(incident.title ?? "");
      setDescription(incident.description ?? "");
      setSeverity(incident.severity ?? "Medium");
    }
  }, [open, incident]);

  if (!open) return null;

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try { await onSaved({ title: title.trim(), description: description.trim(), severity }); }
    finally { setLoading(false); }
  };

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center p-4" role="dialog" aria-modal="true">
      <div className="absolute inset-0 bg-background/70 backdrop-blur-sm" onClick={() => !loading && onClose()} />
      <form onSubmit={submit} className="relative w-full max-w-lg rounded-xl border border-border bg-card shadow-elevated">
        <header className="flex items-start justify-between gap-3 px-5 py-4 border-b border-border">
          <div className="text-sm font-semibold text-foreground">{t("incident.edit.title")}</div>
          <button type="button" onClick={onClose} disabled={loading} className="rounded-md p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted disabled:opacity-50"><X className="h-4 w-4" /></button>
        </header>
        <div className="px-5 py-4 space-y-4">
          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1.5">{t("field.title")}</label>
            <input value={title} onChange={(e) => setTitle(e.target.value)} required className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40" />
          </div>
          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1.5">{t("field.severity")}</label>
            <select value={severity} onChange={(e) => setSeverity(e.target.value)} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40">
              {SEVERITIES.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1.5">{t("field.description")}</label>
            <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={5} className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40 resize-none" />
          </div>
        </div>
        <footer className="flex items-center justify-end gap-2 px-5 py-3 border-t border-border bg-muted/20 rounded-b-xl">
          <button type="button" onClick={onClose} disabled={loading} className="inline-flex items-center rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted">{t("action.cancel")}</button>
          <button type="submit" disabled={loading || !title.trim()} className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-primary-foreground shadow-sm hover:bg-primary/90 disabled:opacity-60">
            {loading && <Loader2 className="h-3.5 w-3.5 animate-spin" />}
            {t("action.save")}
          </button>
        </footer>
      </form>
    </div>
  );
}
