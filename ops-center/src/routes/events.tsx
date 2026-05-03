import { createFileRoute } from "@tanstack/react-router";
import { ENDPOINTS } from "@/config/api";
import { usePagedApi } from "@/hooks/use-paged-api";
import { PageHeader, Card, EmptyState, ErrorState, TableSkeleton } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { formatDate, Mono } from "@/components/data-table";
import { StatusBadge } from "@/components/status-badge";
import { Pagination } from "@/components/pagination";
import { useI18n } from "@/i18n";
import { useHubEvent, rtToast } from "@/realtime/hub";

export const Route = createFileRoute("/events")({
  component: EventsPage,
  head: () => ({ meta: [{ title: "Events — AI Incident Response" }] }),
});

function EventsPage() {
  const { t } = useI18n();
  const paged = usePagedApi<any>(ENDPOINTS.events);

  useHubEvent("AgentEventCreated", () => { rtToast(t("rt.event.created")); paged.refetch(); });
  useHubEvent("AgentExecutionCompleted", () => paged.refetch());

  const headers = [t("col.type"), t("col.source"), t("col.correlationId"), t("col.processed"), t("col.createdAt")];

  return (
    <>
      <PageHeader title={t("page.events.title")} description={t("page.events.desc")} actions={<RefreshButton onClick={paged.refetch} loading={paged.loading} />} />
      <Card className="overflow-hidden">
        {paged.loading ? <TableSkeleton cols={5} /> :
          paged.error ? <ErrorState message={t("state.error")} onRetry={paged.refetch} retryLabel={t("state.retry")} /> :
          paged.items.length === 0 ? <EmptyState message={t("state.empty")} /> : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="bg-muted/50 border-b border-border"><tr>{headers.map((h) => <th key={h} className="text-left px-5 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">{h}</th>)}</tr></thead>
              <tbody className="divide-y divide-border">
                {paged.items.map((r: any, i: number) => (
                  <tr key={r.id ?? i} className="hover:bg-muted/40 transition-colors">
                    <td className="px-5 py-3.5 font-medium text-foreground">{r.type ?? "—"}</td>
                    <td className="px-5 py-3.5 text-muted-foreground">{r.source ?? "—"}</td>
                    <td className="px-5 py-3.5"><Mono>{r.correlationId}</Mono></td>
                    <td className="px-5 py-3.5"><StatusBadge tone={r.processed ? "success" : "warning"}>{r.processed ? t("badge.yes") : t("badge.no")}</StatusBadge></td>
                    <td className="px-5 py-3.5 text-muted-foreground">{formatDate(r.createdAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={paged.page} pageSize={paged.pageSize} totalPages={paged.totalPages} totalCount={paged.totalCount} onPageChange={paged.setPage} onPageSizeChange={paged.setPageSize} />
      </Card>
    </>
  );
}
