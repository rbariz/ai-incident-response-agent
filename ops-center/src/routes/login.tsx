import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { ShieldCheck, Loader2 } from "lucide-react";
import { useAuth } from "@/auth/context";
import { useI18n } from "@/i18n";
import { z } from "zod";
import { fallback, zodValidator } from "@tanstack/zod-adapter";

const searchSchema = z.object({
  redirect: fallback(z.string(), "/").default("/"),
  reason: fallback(z.string(), "").default(""),
});

export const Route = createFileRoute("/login")({
  validateSearch: zodValidator(searchSchema),
  component: LoginPage,
  head: () => ({ meta: [{ title: "Sign in — Ops Center" }] }),
});

function LoginPage() {
  const { t } = useI18n();
  const { user, login, loading, ready } = useAuth();
  const navigate = useNavigate();
  const search = Route.useSearch();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (ready && user) navigate({ to: search.redirect || "/" });
  }, [ready, user, navigate, search.redirect]);

  useEffect(() => {
    if (search.reason === "expired") setError(t("auth.sessionExpired"));
  }, [search.reason, t]);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await login(username.trim(), password);
      navigate({ to: search.redirect || "/" });
    } catch (err: any) {
      setError(err?.message?.includes("401") || err?.message?.toLowerCase().includes("invalid")
        ? t("auth.invalidCredentials")
        : err?.message ?? t("auth.invalidCredentials"));
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm">
        <div className="flex flex-col items-center mb-8">
          <div className="h-12 w-12 rounded-xl bg-primary text-primary-foreground flex items-center justify-center shadow-card mb-4">
            <ShieldCheck className="h-6 w-6" />
          </div>
          <h1 className="text-xl font-semibold text-foreground">{t("app.title")}</h1>
          <p className="text-xs text-muted-foreground mt-1">{t("app.subtitle")}</p>
        </div>
        <form onSubmit={submit} className="rounded-xl border border-border bg-card shadow-elevated p-6 space-y-4">
          <h2 className="text-base font-semibold text-foreground">{t("auth.login")}</h2>
          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1.5">{t("auth.username")}</label>
            <input
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
            />
          </div>
          <div>
            <label className="block text-xs font-medium text-muted-foreground mb-1.5">{t("auth.password")}</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring/40"
            />
          </div>
          {error && (
            <div className="rounded-md border border-destructive/30 bg-destructive/5 px-3 py-2 text-xs text-destructive">{error}</div>
          )}
          <button
            type="submit"
            disabled={loading}
            className="w-full inline-flex items-center justify-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground hover:bg-primary/90 transition-colors disabled:opacity-60"
          >
            {loading && <Loader2 className="h-4 w-4 animate-spin" />}
            {t("auth.signIn")}
          </button>
        </form>
      </div>
    </div>
  );
}
