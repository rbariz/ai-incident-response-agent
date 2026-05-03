import { useEffect, useState } from "react";
import { cn } from "@/lib/utils";
import { X, Check, Ban, Loader2 } from "lucide-react";
import { useI18n } from "@/i18n";

export type ApprovalMode = "approve" | "reject";

export function ApprovalModal({
  open,
  mode,
  execution,
  loading,
  onClose,
  onConfirm,
}: {
  open: boolean;
  mode: ApprovalMode;
  execution: { decision?: string | null; action?: string | null } | null;
  loading: boolean;
  onClose: () => void;
  onConfirm: (reason: string) => void;
}) {
  const { t } = useI18n();
  const [reason, setReason] = useState("");
  const [touched, setTouched] = useState(false);

  useEffect(() => {
    if (open) {
      setReason("");
      setTouched(false);
    }
  }, [open, mode]);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !loading) onClose();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, loading, onClose]);

  if (!open) return null;

  const isReject = mode === "reject";
  const reasonRequired = isReject;
  const trimmed = reason.trim();
  const reasonError = reasonRequired && touched && !trimmed;
  const canSubmit = !loading && (!reasonRequired || !!trimmed);

  const submit = () => {
    if (!canSubmit) {
      setTouched(true);
      return;
    }
    onConfirm(trimmed);
  };

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center p-4" role="dialog" aria-modal="true">
      <div
        className="absolute inset-0 bg-background/70 backdrop-blur-sm"
        onClick={() => !loading && onClose()}
      />
      <div className="relative w-full max-w-md rounded-xl border border-border bg-card shadow-elevated">
        <header className="flex items-start justify-between gap-3 px-5 py-4 border-b border-border">
          <div className="flex items-center gap-3 min-w-0">
            <div
              className={cn(
                "flex h-8 w-8 items-center justify-center rounded-lg ring-1 ring-inset",
                isReject
                  ? "bg-destructive/10 text-destructive ring-destructive/20"
                  : "bg-info/10 text-info ring-info/20"
              )}
            >
              {isReject ? <Ban className="h-4 w-4" /> : <Check className="h-4 w-4" />}
            </div>
            <div className="min-w-0">
              <div className="text-sm font-semibold text-foreground">
                {isReject ? t("approval.confirmReject") : t("approval.confirmApprove")}
              </div>
              <div className="text-xs text-muted-foreground mt-0.5">
                {isReject ? t("approval.rejectDescription") : t("approval.approveDescription")}
              </div>
            </div>
          </div>
          <button
            onClick={onClose}
            disabled={loading}
            className="rounded-md p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors disabled:opacity-50"
            aria-label={t("action.close")}
          >
            <X className="h-4 w-4" />
          </button>
        </header>

        <div className="px-5 py-4 space-y-4">
          <div className="rounded-lg border border-border bg-background/40 p-3 text-xs">
            <div className="grid grid-cols-[80px_1fr] gap-2">
              <span className="text-muted-foreground uppercase tracking-wider">{t("col.decision")}</span>
              <span className="font-medium text-foreground">{execution?.decision ?? "—"}</span>
              <span className="text-muted-foreground uppercase tracking-wider">{t("col.action")}</span>
              <span className="font-medium text-foreground">{execution?.action ?? "—"}</span>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1.5">
              {reasonRequired ? t("approval.reason") : t("approval.reasonOptional")}
              {reasonRequired && <span className="text-destructive ml-0.5">*</span>}
            </label>
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              onBlur={() => setTouched(true)}
              placeholder={t("approval.reasonPlaceholder")}
              rows={4}
              disabled={loading}
              className={cn(
                "w-full rounded-md border bg-background px-3 py-2 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 transition-shadow resize-none",
                reasonError
                  ? "border-destructive/50 focus:ring-destructive/30"
                  : "border-border focus:ring-ring/40"
              )}
            />
            {reasonError && (
              <p className="mt-1 text-xs text-destructive">{t("approval.reasonRequired")}</p>
            )}
          </div>
        </div>

        <footer className="flex items-center justify-end gap-2 px-5 py-3 border-t border-border bg-muted/20 rounded-b-xl">
          <button
            onClick={onClose}
            disabled={loading}
            className="inline-flex items-center rounded-md px-3 py-1.5 text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted transition-colors disabled:opacity-50"
          >
            {t("action.cancel")}
          </button>
          <button
            onClick={submit}
            disabled={!canSubmit}
            className={cn(
              "inline-flex items-center gap-1.5 rounded-md px-3.5 py-1.5 text-sm font-semibold text-white shadow-sm transition-colors disabled:opacity-60 disabled:cursor-not-allowed",
              isReject
                ? "bg-destructive hover:bg-destructive/90"
                : "bg-primary hover:bg-primary/90"
            )}
          >
            {loading ? (
              <>
                <Loader2 className="h-3.5 w-3.5 animate-spin" />
                {t("action.processing")}
              </>
            ) : isReject ? (
              <>
                <Ban className="h-3.5 w-3.5" />
                {t("approval.reject")}
              </>
            ) : (
              <>
                <Check className="h-3.5 w-3.5" />
                {t("approval.approve")}
              </>
            )}
          </button>
        </footer>
      </div>
    </div>
  );
}
