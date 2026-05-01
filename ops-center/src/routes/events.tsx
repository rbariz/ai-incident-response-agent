import { createFileRoute } from "@tanstack/react-router";
import { useApi } from "@/hooks/use-api";
import { ENDPOINTS } from "@/config/api";
import { PageHeader } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { DataTable, formatDate, Mono } from "@/components/data-table";
import { StatusBadge } from "@/components/status-badge";
import { useI18n } from "@/i18n";

export const Route = createFileRoute("/events")({
  component: EventsPage,
  head: () => ({ meta: [{ title: "Events — AI Incident Response" }] }),
});

function EventsPage() {
  const { t } = useI18n();
  const { data, loading, error, refetch } = useApi<any>(ENDPOINTS.events);
  const rows = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);

  return (
    <>
      <PageHeader
        title={t("page.events.title")}
        description={t("page.events.desc")}
        actions={<RefreshButton onClick={refetch} loading={loading} />}
      />
      <DataTable
        loading={loading}
        error={error}
        onRetry={refetch}
        rows={rows}
        columns={[
          { key: "type", header: t("col.type"), render: (r) => <span className="font-medium text-foreground">{r.type ?? "—"}</span> },
          { key: "source", header: t("col.source"), render: (r) => <span className="text-muted-foreground">{r.source ?? "—"}</span> },
          { key: "correlationId", header: t("col.correlationId"), render: (r) => <Mono>{r.correlationId}</Mono> },
          {
            key: "processed",
            header: t("col.processed"),
            render: (r) => (
              <StatusBadge tone={r.processed ? "success" : "warning"}>
                {r.processed ? t("badge.yes") : t("badge.no")}
              </StatusBadge>
            ),
          },
          { key: "createdAtUtc", header: t("col.createdAt"), render: (r) => <span className="text-muted-foreground">{formatDate(r.createdAtUtc)}</span> },
        ]}
      />
    </>
  );
}
