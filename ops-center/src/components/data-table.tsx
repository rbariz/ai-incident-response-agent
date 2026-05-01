import type { ReactNode } from "react";
import { Card, EmptyState, ErrorState, TableSkeleton } from "./ui-bits";
import { useI18n } from "@/i18n";

export interface Column<T> {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
  className?: string;
}

export function DataTable<T extends Record<string, any>>({
  columns,
  rows,
  loading,
  error,
  onRetry,
  onRowClick,
}: {
  columns: Column<T>[];
  rows: T[];
  loading?: boolean;
  error?: Error | null;
  onRetry?: () => void;
  onRowClick?: (row: T) => void;
}) {
  const { t } = useI18n();
  return (
    <Card className="overflow-hidden">
      {loading ? (
        <TableSkeleton cols={columns.length} />
      ) : error ? (
        <ErrorState message={t("state.error")} onRetry={onRetry} retryLabel={t("state.retry")} />
      ) : rows.length === 0 ? (
        <EmptyState message={t("state.empty")} />
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full text-sm">
            <thead className="bg-muted/50 border-b border-border">
              <tr>
                {columns.map((c) => (
                  <th
                    key={c.key}
                    className={`text-left px-5 py-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground ${c.className ?? ""}`}
                  >
                    {c.header}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {rows.map((r, i) => (
                <tr
                  key={i}
                  onClick={onRowClick ? () => onRowClick(r) : undefined}
                  className={`hover:bg-muted/40 transition-colors ${onRowClick ? "cursor-pointer" : ""}`}
                >
                  {columns.map((c) => (
                    <td key={c.key} className={`px-5 py-3.5 align-middle ${c.className ?? ""}`}>
                      {c.render(r)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}

export function formatDate(v?: string | null) {
  if (!v) return "—";
  try {
    const d = new Date(v);
    return d.toLocaleString();
  } catch {
    return v;
  }
}

export function Mono({ children }: { children: ReactNode }) {
  return <span className="font-mono text-xs text-muted-foreground">{children ?? "—"}</span>;
}
