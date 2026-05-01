import { useI18n } from "@/i18n";
import { RefreshCw } from "lucide-react";
import { cn } from "@/lib/utils";

export function RefreshButton({ onClick, loading }: { onClick: () => void; loading?: boolean }) {
  const { t } = useI18n();
  return (
    <button
      onClick={onClick}
      disabled={loading}
      className="inline-flex items-center gap-2 rounded-md border border-border bg-card px-3 py-1.5 text-sm font-medium text-foreground shadow-card hover:bg-muted transition-colors disabled:opacity-60"
    >
      <RefreshCw className={cn("h-4 w-4", loading && "animate-spin")} />
      {t("action.refresh")}
    </button>
  );
}
