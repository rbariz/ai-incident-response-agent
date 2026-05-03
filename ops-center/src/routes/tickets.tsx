import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { Plus, Loader2, X } from "lucide-react";
import { toast } from "sonner";
import { ENDPOINTS, apiPost } from "@/config/api";
import { usePagedApi } from "@/hooks/use-paged-api";
import { PageHeader, Card, EmptyState, ErrorState, TableSkeleton } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { formatDate } from "@/components/data-table";
import { StatusBadge, toneForStatus } from "@/components/status-badge";
import { Pagination } from "@/components/pagination";
import { useI18n } from "@/i18n";
import { useAuth } from "@/auth/context";
import { useHubEvent } from "@/realtime/hub";

export const Route = createFileRoute("/tickets")({
  component: TicketsPage,
  head: () => ({ meta: [{ title: "Tickets — AI Incident Response" }] }),
});

function TicketsPage() {
  const { t } = useI18n();
  const { canManageTickets } = useAuth();
  const paged = usePagedApi<any>(ENDPOINTS.tickets);
  const [createOpen, setCreateOpen] = useState(false);

  useHubEvent("AgentExecutionCompleted", (p: any) => {
    if (String(p?.action ?? "").toLowerCase() === "blockticket") paged.refetch();
  });

  return (
    <>
      <PageHeader
        title={t("page.tickets.title")}
        description={t("page.tickets.desc")}
        actions={
          <div className="flex items-center gap-2">
            <RefreshButton onClick={paged.refetch} loading={paged.loading} />
            {canManageTickets && (
              <button
                onClick={() => setCreateOpen(true)}
                className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-semibold text-primary-foreground shadow-sm hover:bg-primary/90 transition-colors"
              >
                <Plus className="h-4 w-4" /> {t("tickets.create")}
              </button>
            )}
          </div>
        }
      />
      <Card className="overflow-hidden">
        {paged.loading ? (
          <TableSkeleton cols={5} />
        ) : paged.error ? (
          <ErrorState message={t("state.error")} onRetry={paged.refetch} retryLabel={t("state.retry")} />
        ) : paged.items.length === 0 ? (
          <EmptyState message={t("state.empty")} />
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="bg-muted/50 border-b border-border">
                <tr>
                  {[t("col.ticketCode"), t("col.status"), t("col.blockedReason"), t("col.blockedAt"), t("col.createdAt")].map((h) => (
                    <th key={h} className="text-left px-5 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {paged.items.map((r: any) => (
                  <tr key={r.id} className="hover:bg-muted/40 transition-colors">
                    <td className="px-5 py-3.5 font-medium text-foreground">{r.ticketCode ?? "—"}</td>
                    <td className="px-5 py-3.5"><StatusBadge tone={toneForStatus(r.status)}>{r.status ?? "—"}</StatusBadge></td>
                    <td className="px-5 py-3.5 text-muted-foreground max-w-md truncate">{r.blockedReason ?? "—"}</td>
                    <td className="px-5 py-3.5 text-muted-foreground">{formatDate(r.blockedAtUtc)}</td>
                    <td className="px-5 py-3.5 text-muted-foreground">{formatDate(r.createdAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination
          page={paged.page}
          pageSize={paged.pageSize}
          totalPages={paged.totalPages}
          totalCount={paged.totalCount}
          onPageChange={paged.setPage}
          onPageSizeChange={paged.setPageSize}
        />
      </Card>

      <CreateTicketModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={() => { setCreateOpen(false); paged.refetch(); }}
      />
    </>
  );
}

function CreateTicketModal({ open, onClose, onCreated }: { open: boolean; onClose: () => void; onCreated: () => void }) {
  const { t } = useI18n();
  const [code, setCode] = useState("");
  const [loading, setLoading] = useState(false);

  if (!open) return null;

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!code.trim()) return;
    setLoading(true);
    try {
      await apiPost(ENDPOINTS.tickets, { ticketCode: code.trim() });
      toast.success(t("tickets.toast.created"));
      setCode("");
      onCreated();
    } catch (err: any) {
      toast.error(t("tickets.toast.createFailed"), { description: err?.message });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center p-4" role="dialog" aria-modal="true">
      <div className="absolute inset-0 bg-background/70 backdrop-blur-sm" onClick={() => !loading && onClose()} />
      <form onSubmit={submit} className="relative w-full max-w-md rounded-xl border border-border bg-card shadow-elevated">
        <header className="flex items-start justify-between gap-3 px-5 py-4 border-b border-border">
          <div className="text-sm font-semibold text-foreground">{t("tickets.create.title")}</div>
          <button type="button" onClick={onClose} disabled={loading} className="rounded-md p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted disabled:opacity-50"><X className="h-4 w-4" /></button>
        </header>
        <div className="px-5 py-4">
          <label className="block text-xs font-medium text-muted-foreground mb-1.5">{t("tickets.code")}</label>
          <input
            autoFocus
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder={t("tickets.codePlaceholder")}
            className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
          />
        </div>
        <footer className="flex items-center justify-end gap-2 px-5 py-3 border-t border-border bg-muted/20 rounded-b-xl">
          <button type="button" onClick={onClose} disabled={loading} className="inline-flex items-center rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted">{t("action.cancel")}</button>
          <button type="submit" disabled={loading || !code.trim()} className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-primary-foreground shadow-sm hover:bg-primary/90 disabled:opacity-60">
            {loading && <Loader2 className="h-3.5 w-3.5 animate-spin" />}
            {t("action.create")}
          </button>
        </footer>
      </form>
    </div>
  );
}
