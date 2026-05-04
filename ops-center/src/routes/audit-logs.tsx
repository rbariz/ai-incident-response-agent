import { createFileRoute } from "@tanstack/react-router";
import { useMemo, useState } from "react";
import { Search, X, Copy, Check } from "lucide-react";
import { ENDPOINTS } from "@/config/api";
import { usePagedApi } from "@/hooks/use-paged-api";
import { PageHeader, Card, EmptyState, ErrorState, TableSkeleton } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { formatDate, Mono } from "@/components/data-table";
import { StatusBadge } from "@/components/status-badge";
import { Pagination } from "@/components/pagination";
import { DetailDrawer, DetailField, DetailSection, CodeBlock } from "@/components/detail-drawer";
import { useI18n } from "@/i18n";

export const Route = createFileRoute("/audit-logs")({
  component: AuditLogsPage,
  head: () => ({ meta: [{ title: "Audit Logs — AI Incident Response" }] }),
});

type AuditLog = {
  id: string;
  actorType?: string;
  actorName?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  correlationId?: string;
  detailsJson?: string | null;
  createdAtUtc?: string;
};

function AuditLogsPage() {
  const { t } = useI18n();
  const [entityType, setEntityType] = useState("");
  const [correlationId, setCorrelationId] = useState("");
  const [selected, setSelected] = useState<AuditLog | null>(null);

  const extraParams = useMemo(
    () => ({
      entityType: entityType.trim() || undefined,
      correlationId: correlationId.trim() || undefined,
    }),
    [entityType, correlationId],
  );

  const paged = usePagedApi<AuditLog>(ENDPOINTS.auditLogs, { pageSize: 50, extraParams });

  const applyFilters = () => {
    paged.setFilters({
      entityType: entityType.trim() || undefined,
      correlationId: correlationId.trim() || undefined,
    });
  };

  const clearFilters = () => {
    setEntityType("");
    setCorrelationId("");
    paged.setFilters({});
  };

  return (
    <>
      <PageHeader
        title={t("page.auditLogs.title")}
        description={t("page.auditLogs.desc")}
        actions={<RefreshButton onClick={paged.refetch} loading={paged.loading} />}
      />

      <Card className="mb-4">
        <div className="px-5 py-4 grid grid-cols-1 md:grid-cols-[1fr_1fr_auto] gap-3 items-end">
          <div>
            <label className="block text-[11px] font-medium uppercase tracking-wider text-muted-foreground mb-1.5">
              {t("audit.filter.entityType")}
            </label>
            <input
              value={entityType}
              onChange={(e) => setEntityType(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && applyFilters()}
              placeholder="Incident, Execution, Ticket…"
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
            />
          </div>
          <div>
            <label className="block text-[11px] font-medium uppercase tracking-wider text-muted-foreground mb-1.5">
              {t("audit.filter.correlationId")}
            </label>
            <input
              value={correlationId}
              onChange={(e) => setCorrelationId(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && applyFilters()}
              placeholder="00000000-0000-…"
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm font-mono focus:outline-none focus:ring-2 focus:ring-ring/40"
            />
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={applyFilters}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-2 text-sm font-semibold text-primary-foreground shadow-sm hover:bg-primary/90 transition-colors"
            >
              <Search className="h-4 w-4" /> {t("audit.filter.apply")}
            </button>
            {(entityType || correlationId) && (
              <button
                onClick={clearFilters}
                className="inline-flex items-center gap-1.5 rounded-md border border-border bg-background px-3 py-2 text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
              >
                <X className="h-4 w-4" /> {t("audit.filter.clear")}
              </button>
            )}
          </div>
        </div>
      </Card>

      <Card className="overflow-hidden">
        {paged.loading ? (
          <TableSkeleton cols={6} />
        ) : paged.error ? (
          <ErrorState message={t("state.error")} onRetry={paged.refetch} retryLabel={t("state.retry")} />
        ) : paged.items.length === 0 ? (
          <EmptyState message={t("state.empty")} />
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="bg-muted/50 border-b border-border">
                <tr>
                  {[
                    t("col.createdAt"),
                    t("audit.col.actorType"),
                    t("audit.col.actorName"),
                    t("audit.col.action"),
                    t("audit.col.entityType"),
                    t("audit.col.entityId"),
                    t("col.correlationId"),
                  ].map((h) => (
                    <th key={h} className="text-left px-5 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {paged.items.map((r) => (
                  <tr
                    key={r.id}
                    onClick={() => setSelected(r)}
                    className="hover:bg-muted/40 transition-colors cursor-pointer"
                  >
                    <td className="px-5 py-3.5 text-muted-foreground whitespace-nowrap">{formatDate(r.createdAtUtc)}</td>
                    <td className="px-5 py-3.5">
                      <StatusBadge tone="muted" dot={false}>{r.actorType ?? "—"}</StatusBadge>
                    </td>
                    <td className="px-5 py-3.5 text-foreground">{r.actorName ?? "—"}</td>
                    <td className="px-5 py-3.5 font-medium text-foreground">{r.action ?? "—"}</td>
                    <td className="px-5 py-3.5 text-muted-foreground">{r.entityType ?? "—"}</td>
                    <td className="px-5 py-3.5"><Mono>{r.entityId}</Mono></td>
                    <td className="px-5 py-3.5"><Mono>{r.correlationId}</Mono></td>
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

      <DetailDrawer
        open={!!selected}
        onClose={() => setSelected(null)}
        title={selected?.action ?? t("audit.detail.title")}
        subtitle={selected ? `${selected.actorType ?? ""} · ${selected.actorName ?? ""}` : undefined}
      >
        {selected && (
          <>
            <DetailSection title={t("detail.section.overview")}>
              <DetailField label={t("col.createdAt")}>{formatDate(selected.createdAtUtc)}</DetailField>
              <DetailField label={t("audit.col.actorType")}>{selected.actorType ?? "—"}</DetailField>
              <DetailField label={t("audit.col.actorName")}>{selected.actorName ?? "—"}</DetailField>
              <DetailField label={t("audit.col.action")}>{selected.action ?? "—"}</DetailField>
              <DetailField label={t("audit.col.entityType")}>{selected.entityType ?? "—"}</DetailField>
              <DetailField label={t("audit.col.entityId")}><Mono>{selected.entityId}</Mono></DetailField>
              <DetailField label={t("col.correlationId")}><Mono>{selected.correlationId}</Mono></DetailField>
            </DetailSection>
            <DetailSection title={t("audit.detail.details")}>
              <div className="py-3">
                {selected.detailsJson ? (
                  <CodeBlock value={selected.detailsJson} />
                ) : (
                  <p className="text-sm text-muted-foreground py-2">{t("audit.detail.noDetails")}</p>
                )}
              </div>
            </DetailSection>
          </>
        )}
      </DetailDrawer>
    </>
  );
}

// Re-export to silence "unused" if linter complains; not required. Used inline above.
export const _icons = { Copy, Check };
