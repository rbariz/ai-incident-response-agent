import { cn } from "@/lib/utils";
import type { ReactNode } from "react";

type Tone = "primary" | "success" | "warning" | "error" | "info" | "muted" | "accent";

const toneClass: Record<Tone, string> = {
  primary: "bg-primary/10 text-primary ring-1 ring-inset ring-primary/20",
  success: "bg-success/10 text-success ring-1 ring-inset ring-success/20",
  warning: "bg-warning/15 text-warning-foreground ring-1 ring-inset ring-warning/30",
  error: "bg-destructive/10 text-destructive ring-1 ring-inset ring-destructive/20",
  info: "bg-info/10 text-info ring-1 ring-inset ring-info/20",
  muted: "bg-muted text-muted-foreground ring-1 ring-inset ring-border",
  accent: "bg-accent/15 text-accent ring-1 ring-inset ring-accent/30",
};

export function StatusBadge({
  tone = "muted",
  children,
  dot = true,
  className,
}: {
  tone?: Tone;
  children: ReactNode;
  dot?: boolean;
  className?: string;
}) {
  const dotColor: Record<Tone, string> = {
    primary: "bg-primary",
    success: "bg-success",
    warning: "bg-warning",
    error: "bg-destructive",
    info: "bg-info",
    muted: "bg-muted-foreground",
    accent: "bg-accent",
  };
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium whitespace-nowrap",
        toneClass[tone],
        className
      )}
    >
      {dot && <span className={cn("h-1.5 w-1.5 rounded-full", dotColor[tone])} />}
      {children}
    </span>
  );
}

export function toneForStatus(s?: string | null): Tone {
  const v = (s ?? "").toLowerCase();
  if (["success", "succeeded", "ok", "completed", "resolved", "done"].includes(v)) return "success";
  if (["failed", "error", "critical"].includes(v)) return "error";
  if (["skipped", "ignored", "cancelled", "canceled"].includes(v)) return "muted";
  if (["pending", "running", "in_progress", "open", "active"].includes(v)) return "info";
  if (["warning", "warn", "degraded"].includes(v)) return "warning";
  return "primary";
}

export function toneForSeverity(s?: string | null): Tone {
  const v = (s ?? "").toLowerCase();
  if (["critical", "high"].includes(v)) return "error";
  if (["medium", "moderate"].includes(v)) return "warning";
  if (["low", "info"].includes(v)) return "info";
  return "muted";
}
