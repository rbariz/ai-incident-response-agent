import { createContext, useContext, useEffect, useRef, useState, useCallback, type ReactNode } from "react";
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { API_BASE_URL, getStoredToken } from "@/config/api";
import { toast } from "sonner";

export type HubEvent =
  | "AgentEventCreated"
  | "AgentExecutionStarted"
  | "AgentExecutionCompleted"
  | "AgentExecutionApprovalChanged"
  | "IncidentChanged";

export type ConnStatus = "connected" | "reconnecting" | "disconnected";

type Handler = (payload: any) => void;

function resolveHubUrl(): string {
  const base = API_BASE_URL ?? "";
  // Absolute URL
  if (/^https?:\/\//i.test(base)) {
    return base.replace(/\/$/, "") + "/hubs/agent";
  }
  // "/api" or relative — derive from window origin
  if (typeof window !== "undefined") {
    return window.location.origin.replace(/\/$/, "") + "/hubs/agent";
  }
  return "/hubs/agent";
}

type Ctx = {
  status: ConnStatus;
  on: (event: HubEvent, handler: Handler) => () => void;
};

const RealtimeContext = createContext<Ctx>({ status: "disconnected", on: () => () => {} });

export function RealtimeProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<ConnStatus>("disconnected");
  const handlersRef = useRef<Map<HubEvent, Set<Handler>>>(new Map());
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
    const url = resolveHubUrl();
    const conn = new HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => getStoredToken() ?? "" })
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
    conn.onreconnected(() => setStatus("connected"));
    conn.onclose(() => setStatus("disconnected"));

    setStatus("reconnecting");
    conn
      .start()
      .then(() => setStatus("connected"))
      .catch((err) => {
        console.warn("[realtime] connect failed:", err?.message ?? err);
        setStatus("disconnected");
      });

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
    return () => {
      set!.delete(handler);
    };
  }, []);

  return <RealtimeContext.Provider value={{ status, on }}>{children}</RealtimeContext.Provider>;
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

/** Subtle toast helper used by route pages */
export function rtToast(message: string, description?: string) {
  toast(message, { description, duration: 2800 });
}
