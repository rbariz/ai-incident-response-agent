import { useRealtime } from "@/realtime/hub";
import { useI18n } from "@/i18n";
import { cn } from "@/lib/utils";

export function ConnectionStatus() {
  const { status } = useRealtime();
  const { t } = useI18n();

  const map = {
    connected: { dot: "bg-success", text: "text-foreground", pulse: false, label: t("rt.connected") },
    reconnecting: { dot: "bg-warning", text: "text-foreground", pulse: true, label: t("rt.reconnecting") },
    disconnected: { dot: "bg-destructive", text: "text-muted-foreground", pulse: false, label: t("rt.disconnected") },
    unauthorized: { dot: "bg-destructive", text: "text-muted-foreground", pulse: false, label: t("rt.unauthorized") },
  } as const;

  const s = map[status];
  return (
    <div className="inline-flex items-center gap-2 rounded-lg border border-border bg-muted px-2.5 py-1 text-xs font-medium">
      <span className={cn("h-2 w-2 rounded-full", s.dot, s.pulse && "animate-pulse")} />
      <span className={s.text}>{s.label}</span>
    </div>
  );
}
