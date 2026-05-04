import { createContext, useContext, useEffect, useRef, useState, useCallback, type ReactNode } from "react";
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { API_BASE_URL, ensureFreshToken, getAccessToken, clearSession } from "@/config/api";
import { toast } from "sonner";

export type HubEvent =
  | "AgentEventCreated"
  | "AgentExecutionStarted"
  | "AgentExecutionCompleted"
  | "AgentExecutionApprovalChanged"
  | "IncidentChanged";

export type ConnStatus = "connected" | "reconnecting" | "disconnected" | "unauthorized";

type Handler = (payload: any) => void;

function resolveHubUrl(): string {
  const base = API_BASE_URL ?? "";
  if (/^https?:\/\//i.test(base)) {
    return base.replace(/\/$/, "") + "/hubs/agent";
  }
  if (typeof window !== "undefined") {
    return window.location.origin.replace(/\/$/, "") + "/hubs/agent";
  }
  return "/hubs/agent";
}

type Ctx = {
  status: ConnStatus;
  on: (event: HubEvent, handler: Handler) => () => void;
  onReconnected: (cb: () => void) => () => void;
};

const RealtimeContext = createContext<Ctx>({
  status: "disconnected",
  on: () => () => {},
  onReconnected: () => () => {},
});

export function RealtimeProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<ConnStatus>("disconnected");
  const handlersRef = useRef<Map<HubEvent, Set<Handler>>>(new Map());
  const reconnectedRef = useRef<Set<() => void>>(new Set());
  const connRef = useRef<HubConnection | null>(null);

  const emit = useCallback((event: HubEvent, payload: any) => {
    const set = handlersRef.current.get(event);
    if (!set) return;
    set.forEach((h) => {
      try { h(payload); } catch (e) { console.error("[realtime handler]", e); }
    });
  }, []);

  useEffect(() => {
    if (typeof window === "undefined") return;
    // Only connect if user is logged in
    if (!getAccessToken()) {
      setStatus("disconnected");
      return;
    }
    const url = resolveHubUrl();
    const conn = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: async () => (await ensureFreshToken()) ?? getAccessToken() ?? "",
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (ctx) => Math.min(30000, 1000 * Math.pow(2, ctx.previousRetryCount)),
      })
      .configureLogging(LogLevel.Warning)
      .build();

    connRef.current = conn;

    const events: HubEvent[] = [
      "AgentEventCreated",
      "AgentExecutionStarted",
      "AgentExecutionCompleted",
      "AgentExecutionApprovalChanged",
      "IncidentChanged",
    ];
    events.forEach((evt) => conn.on(evt, (payload: any) => emit(evt, payload)));

    conn.onreconnecting(() => setStatus("reconnecting"));
    conn.onreconnected(() => {
      setStatus("connected");
      reconnectedRef.current.forEach((cb) => { try { cb(); } catch {} });
    });
    conn.onclose((err) => {
      const msg = String(err?.message ?? "");
      if (/401|unauthorized/i.test(msg)) {
        setStatus("unauthorized");
        toast.error("Realtime unauthorized");
        clearSession();
        if (!window.location.pathname.startsWith("/login")) {
          const redirect = encodeURIComponent(window.location.pathname + window.location.search);
          window.location.replace(`/login?redirect=${redirect}&reason=expired`);
        }
      } else {
        setStatus("disconnected");
      }
    });

    setStatus("reconnecting");
    (async () => {
      // Pre-refresh if needed
      await ensureFreshToken();
      try {
        await conn.start();
        setStatus("connected");
      } catch (err: any) {
        const msg = String(err?.message ?? err);
        if (/401|unauthorized/i.test(msg)) {
          setStatus("unauthorized");
          clearSession();
          if (!window.location.pathname.startsWith("/login")) {
            const redirect = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.replace(`/login?redirect=${redirect}&reason=expired`);
          }
        } else {
          console.warn("[realtime] connect failed:", msg);
          setStatus("disconnected");
        }
      }
    })();

    return () => {
      try {
        if (conn.state !== HubConnectionState.Disconnected) conn.stop();
      } catch {}
      connRef.current = null;
    };
  }, [emit]);

  const on = useCallback((event: HubEvent, handler: Handler) => {
    let set = handlersRef.current.get(event);
    if (!set) {
      set = new Set();
      handlersRef.current.set(event, set);
    }
    set.add(handler);
    return () => { set!.delete(handler); };
  }, []);

  const onReconnected = useCallback((cb: () => void) => {
    reconnectedRef.current.add(cb);
    return () => { reconnectedRef.current.delete(cb); };
  }, []);

  return <RealtimeContext.Provider value={{ status, on, onReconnected }}>{children}</RealtimeContext.Provider>;
}

export function useRealtime() {
  return useContext(RealtimeContext);
}

export function useHubEvent(event: HubEvent, handler: Handler) {
  const { on } = useRealtime();
  const ref = useRef(handler);
  ref.current = handler;
  useEffect(() => on(event, (p) => ref.current(p)), [event, on]);
}

/** Subscribe to "reconnected" so pages can refetch their data. */
export function useHubReconnected(cb: () => void) {
  const { onReconnected } = useRealtime();
  const ref = useRef(cb);
  ref.current = cb;
  useEffect(() => onReconnected(() => ref.current()), [onReconnected]);
}

/** Subtle toast helper used by route pages */
export function rtToast(message: string, description?: string) {
  toast(message, { description, duration: 2800 });
}
