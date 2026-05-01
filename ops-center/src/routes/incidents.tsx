import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { useApi } from "@/hooks/use-api";
import { ENDPOINTS } from "@/config/api";
import { PageHeader } from "@/components/ui-bits";
import { RefreshButton } from "@/components/refresh-button";
import { DataTable, formatDate } from "@/components/data-table";
import { StatusBadge, toneForSeverity, toneForStatus } from "@/components/status-badge";
import { DetailDrawer, DetailField, DetailSection } from "@/components/detail-drawer";
import { useI18n } from "@/i18n";

export const Route = createFileRoute("/incidents")({
  component: IncidentsPage,
  head: () => ({ meta: [{ title: "Incidents — AI Incident Response" }] }),
});

function IncidentsPage() {
  const { t } = useI18n();
  const { data, loading, error, refetch } = useApi<any>(ENDPOINTS.incidents);
  const rows = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);
  const [selected, setSelected] = useState<any | null>(null);

  return (
    <>
      <PageHeader
        title={t("page.incidents.title")}
        description={t("page.incidents.desc")}
        actions={<RefreshButton onClick={refetch} loading={loading} />}
      />
      <DataTable
        loading={loading}
        error={error}
        onRetry={refetch}
        rows={rows}
        onRowClick={(r) => setSelected(r)}
        columns={[
          { key: "title", header: t("col.title"), render: (r) => <span className="font-medium text-foreground">{r.title ?? "—"}</span> },
          { key: "severity", header: t("col.severity"), render: (r) => <StatusBadge tone={toneForSeverity(r.severity)}>{r.severity ?? "—"}</StatusBadge> },
          { key: "status", header: t("col.status"), render: (r) => <StatusBadge tone={toneForStatus(r.status)}>{r.status ?? "—"}</StatusBadge> },
          { key: "createdAtUtc", header: t("col.createdAt"), render: (r) => <span className="text-muted-foreground">{formatDate(r.createdAtUtc)}</span> },
          { key: "resolvedAtUtc", header: t("col.resolvedAt"), render: (r) => <span className="text-muted-foreground">{formatDate(r.resolvedAtUtc)}</span> },
        ]}
      />

      <DetailDrawer
        open={!!selected}
        onClose={() => setSelected(null)}
        title={selected?.title ?? "Incident"}
        subtitle={selected?.id ? `ID: ${selected.id}` : undefined}
      >
        {selected && (
          <>
            <DetailSection title={t("detail.section.overview")}>
              <DetailField label={t("field.title")}>
                <span className="font-medium">{selected.title}</span>
              </DetailField>
              <DetailField label={t("field.severity")}>
                <StatusBadge tone={toneForSeverity(selected.severity)}>{selected.severity ?? "—"}</StatusBadge>
              </DetailField>
              <DetailField label={t("field.status")}>
                <StatusBadge tone={toneForStatus(selected.status)}>{selected.status ?? "—"}</StatusBadge>
              </DetailField>
            </DetailSection>

            <DetailSection title={t("detail.section.timing")}>
              <DetailField label={t("field.createdAt")}>{formatDate(selected.createdAtUtc)}</DetailField>
              <DetailField label={t("field.resolvedAt")}>{formatDate(selected.resolvedAtUtc)}</DetailField>
            </DetailSection>

            {selected.description && (
              <section className="mt-6">
                <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground mb-3">{t("detail.section.description")}</h3>
                <div className="rounded-lg border border-border bg-background/40 p-4 text-sm leading-relaxed text-foreground whitespace-pre-wrap">
                  {selected.description}
                </div>
              </section>
            )}
          </>
        )}
      </DetailDrawer>
    </>
  );
}
