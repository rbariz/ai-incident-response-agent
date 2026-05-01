import { useEffect, useState, type ReactNode } from "react";
import { cn } from "@/lib/utils";
import { X, Copy, Check } from "lucide-react";
import { useI18n } from "@/i18n";

export function DetailDrawer({
  open,
  onClose,
  title,
  subtitle,
  children,
}: {
  open: boolean;
  onClose: () => void;
  title: ReactNode;
  subtitle?: ReactNode;
  children: ReactNode;
}) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", onKey);
    document.body.style.overflow = "hidden";
    return () => {
      window.removeEventListener("keydown", onKey);
      document.body.style.overflow = "";
    };
  }, [open, onClose]);

  return (
    <div
      className={cn(
        "fixed inset-0 z-50 transition-opacity",
        open ? "opacity-100 pointer-events-auto" : "opacity-0 pointer-events-none"
      )}
      aria-hidden={!open}
    >
      <div
        className="absolute inset-0 bg-background/60 backdrop-blur-sm"
        onClick={onClose}
      />
      <aside
        className={cn(
          "absolute right-0 top-0 h-full w-full sm:max-w-xl md:max-w-2xl bg-card border-l border-border shadow-elevated flex flex-col transition-transform duration-300 ease-out",
          open ? "translate-x-0" : "translate-x-full"
        )}
        role="dialog"
        aria-modal="true"
      >
        <header className="flex items-start justify-between gap-4 px-6 py-5 border-b border-border">
          <div className="min-w-0 flex-1">
            <div className="text-base font-semibold text-foreground truncate">{title}</div>
            {subtitle && <div className="mt-0.5 text-xs text-muted-foreground truncate">{subtitle}</div>}
          </div>
          <CloseButton onClose={onClose} />
        </header>
        <div className="flex-1 overflow-y-auto px-6 py-5">{children}</div>
      </aside>
    </div>
  );
}

function CloseButton({ onClose }: { onClose: () => void }) {
  const { t } = useI18n();
  return (
    <button
      onClick={onClose}
      className="rounded-md p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
      aria-label={t("action.close")}
    >
      <X className="h-4 w-4" />
    </button>
  );
}

export function DetailField({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-[140px_1fr] gap-4 py-2.5 border-b border-border/60 last:border-0">
      <div className="text-xs font-medium uppercase tracking-wider text-muted-foreground pt-0.5">{label}</div>
      <div className="text-sm text-foreground min-w-0 break-words">{children ?? <span className="text-muted-foreground">—</span>}</div>
    </div>
  );
}

export function DetailSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="mt-6 first:mt-0">
      <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground mb-3">{title}</h3>
      <div className="rounded-lg border border-border bg-background/40">
        <div className="px-4">{children}</div>
      </div>
    </section>
  );
}

export function CodeBlock({ value, language = "json" }: { value: unknown; language?: string }) {
  const { t } = useI18n();
  const [copied, setCopied] = useState(false);
  let text: string;
  if (typeof value === "string") {
    try {
      text = JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      text = value;
    }
  } else {
    try {
      text = JSON.stringify(value, null, 2);
    } catch {
      text = String(value);
    }
  }

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {}
  };

  return (
    <div className="relative rounded-lg border border-border bg-muted/40 overflow-hidden">
      <div className="flex items-center justify-between px-3 py-1.5 border-b border-border bg-muted/60">
        <span className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground">{language}</span>
        <button
          onClick={copy}
          className="inline-flex items-center gap-1.5 rounded-md px-2 py-1 text-xs text-muted-foreground hover:text-foreground hover:bg-background transition-colors"
        >
          {copied ? <Check className="h-3.5 w-3.5 text-success" /> : <Copy className="h-3.5 w-3.5" />}
          {copied ? t("action.copied") : t("action.copy")}
        </button>
      </div>
      <pre className="max-h-[420px] overflow-auto p-4 text-xs leading-relaxed font-mono text-foreground">
        <code>{text}</code>
      </pre>
    </div>
  );
}
