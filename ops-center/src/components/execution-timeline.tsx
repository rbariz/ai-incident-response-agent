import { useEffect, useState } from "react";
import { apiGet } from "@/config/api";
import { ENDPOINTS } from "@/config/api";
import { formatDate } from "@/components/data-table";
import { Activity, Cpu, AlertTriangle } from "lucide-react";
import { EmptyState, ErrorState } from "@/components/ui-bits";
import { useI18n } from "@/i18n";

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
  if (v.includes("incident")) return { color: "bg-destructive", ring: "ring-destructive/30", text: "text-destructive", Icon: AlertTriangle, labelKey: "entity.incident" as const };
  if (v.includes("execution") || v.includes("action") || v.includes("decision")) return { color: "bg-accent", ring: "ring-accent/30", text: "text-accent", Icon: Cpu, labelKey: "entity.execution" as const };
  return { color: "bg-info", ring: "ring-info/30", text: "text-info", Icon: Activity, labelKey: "entity.event" as const };
}

function titleFor(it: TimelineItem): string {
  const kind = (it.itemType ?? it.type ?? it.kind ?? it.category ?? "").toLowerCase();
  if (kind === "event") return it.eventType ?? it.type ?? it.title ?? it.message ?? "—";
  if (kind === "execution") return it.action ?? it.decision ?? it.title ?? "—";
  if (kind === "incident") return it.title ?? it.message ?? "—";
  return it.title ?? it.message ?? it.description ?? it.eventType ?? it.action ?? "—";
}

export function ExecutionTimeline({ correlationId }: { correlationId?: string | null }) {
  const { t } = useI18n();
  const [items, setItems] = useState<TimelineItem[] | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    setItems(null);
    apiGet<any>(ENDPOINTS.timeline)
      .then((data) => {
        if (cancelled) return;
        const all: TimelineItem[] = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);
        const filtered = correlationId
          ? all.filter((x) => (x.correlationId ?? "") === correlationId)
          : all;
        setItems(filtered);
      })
      .catch((e) => !cancelled && setError(e as Error))
      .finally(() => !cancelled && setLoading(false));
    return () => {
      cancelled = true;
    };
  }, [correlationId]);

  if (loading) {
    return (
      <div className="space-y-3 p-4">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="flex gap-3">
            <div className="skeleton h-7 w-7 rounded-full" />
            <div className="flex-1 space-y-2">
              <div className="skeleton h-3 w-1/3" />
              <div className="skeleton h-3 w-2/3" />
            </div>
          </div>
        ))}
      </div>
    );
  }
  if (error) return <ErrorState message={t("timeline.error")} />;
  if (!items || items.length === 0) return <EmptyState message={t("timeline.empty")} />;

  return (
    <ol className="relative p-4">
      <span className="absolute left-[27px] top-6 bottom-6 w-px bg-border" aria-hidden />
      {items.map((it, i) => {
        const meta = classifyType(it.itemType ?? it.type ?? it.kind ?? it.category);
        const when = it.createdAtUtc ?? it.timestamp;
        const Icon = meta.Icon;
        return (
          <li key={i} className="relative pl-11 pb-5 last:pb-0">
            <span className={`absolute left-0 top-0.5 h-7 w-7 rounded-full bg-card ring-4 ${meta.ring} flex items-center justify-center`}>
              <Icon className={`h-3 w-3 ${meta.text}`} />
            </span>
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <div className={`text-[10px] font-semibold uppercase tracking-wider ${meta.text}`}>{t(meta.labelKey)}</div>
                <div className="mt-0.5 text-sm font-medium text-foreground break-words">
                  {titleFor(it)}
                </div>
              </div>
              <span className="text-xs text-muted-foreground whitespace-nowrap shrink-0">{formatDate(when)}</span>
            </div>
          </li>
        );
      })}
    </ol>
  );
}
