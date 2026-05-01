import { createContext, useContext, useEffect, useState, type ReactNode } from "react";

type Lang = "fr" | "en";

const en = {
  "nav.dashboard": "Dashboard",
  "nav.events": "Events",
  "nav.executions": "Executions",
  "nav.incidents": "Incidents",
  "nav.timeline": "Timeline",
  "app.title": "AI Incident Response",
  "app.subtitle": "Operations Center",
  "action.refresh": "Refresh",
  "action.copy": "Copy",
  "action.copied": "Copied",
  "action.close": "Close",
  "kpi.totalEvents": "Total events",
  "kpi.pendingEvents": "Pending events",
  "kpi.executions": "Executions",
  "kpi.incidents": "Incidents",
  "kpi.lastProvider": "Last AI provider",
  "state.empty": "No data available",
  "state.error": "Unable to load data",
  "state.retry": "Retry",
  "col.type": "Type",
  "col.source": "Source",
  "col.correlationId": "Correlation",
  "col.processed": "Processed",
  "col.createdAt": "Created at",
  "col.status": "Status",
  "col.decision": "Decision",
  "col.action": "Action",
  "col.provider": "Provider",
  "col.confidence": "Confidence",
  "col.title": "Title",
  "col.severity": "Severity",
  "col.resolvedAt": "Resolved at",
  "badge.yes": "Yes",
  "badge.no": "No",
  "page.dashboard.title": "Dashboard",
  "page.dashboard.desc": "Overview of your AI incident response agent",
  "page.events.title": "Agent Events",
  "page.events.desc": "All events ingested by the agent",
  "page.executions.title": "Agent Executions",
  "page.executions.desc": "AI decisions and actions performed",
  "page.incidents.title": "Incidents",
  "page.incidents.desc": "Detected incidents and their status",
  "page.timeline.title": "Timeline",
  "page.timeline.desc": "Unified real-time activity",

  // Detail drawers
  "detail.section.overview": "Overview",
  "detail.section.timing": "Timing",
  "detail.section.timeline": "Timeline",
  "detail.section.description": "Description",
  "detail.section.errorMessage": "Error message",
  "detail.section.resultJson": "Result JSON",
  "detail.section.aiSummary": "AI Summary",
  "detail.aiSummary.fr": "AI Summary (FR)",
  "detail.aiSummary.en": "AI Summary (EN)",
  "detail.aiSummary.originalLang": "original language",
  "detail.aiSummary.unavailable": "No summary available in this language",

  "field.status": "Status",
  "field.decision": "Decision",
  "field.action": "Action",
  "field.provider": "Analysis Provider",
  "field.confidence": "Confidence Score",
  "field.retryCount": "Retry Count",
  "field.analysisLanguage": "Analysis Language",
  "field.createdAt": "Created At",
  "field.startedAt": "Started At",
  "field.completedAt": "Completed At",
  "field.resolvedAt": "Resolved At",
  "field.title": "Title",
  "field.severity": "Severity",
  "field.description": "Description",

  "entity.execution": "Execution",
  "entity.incident": "Incident",
  "entity.event": "Event",

  "timeline.empty": "No timeline events for this execution",
  "timeline.error": "Unable to load timeline",
} as const;

const fr: Record<keyof typeof en, string> = {
  "nav.dashboard": "Tableau de bord",
  "nav.events": "Événements",
  "nav.executions": "Exécutions",
  "nav.incidents": "Incidents",
  "nav.timeline": "Chronologie",
  "app.title": "AI Incident Response",
  "app.subtitle": "Centre des opérations",
  "action.refresh": "Actualiser",
  "action.copy": "Copier",
  "action.copied": "Copié",
  "action.close": "Fermer",
  "kpi.totalEvents": "Événements totaux",
  "kpi.pendingEvents": "Événements en attente",
  "kpi.executions": "Exécutions",
  "kpi.incidents": "Incidents",
  "kpi.lastProvider": "Dernier fournisseur IA",
  "state.empty": "Aucune donnée disponible",
  "state.error": "Impossible de charger les données",
  "state.retry": "Réessayer",
  "col.type": "Type",
  "col.source": "Source",
  "col.correlationId": "Corrélation",
  "col.processed": "Traité",
  "col.createdAt": "Créé le",
  "col.status": "Statut",
  "col.decision": "Décision",
  "col.action": "Action",
  "col.provider": "Fournisseur",
  "col.confidence": "Confiance",
  "col.title": "Titre",
  "col.severity": "Sévérité",
  "col.resolvedAt": "Résolu le",
  "badge.yes": "Oui",
  "badge.no": "Non",
  "page.dashboard.title": "Tableau de bord",
  "page.dashboard.desc": "Vue d'ensemble de votre agent de réponse aux incidents",
  "page.events.title": "Événements de l'agent",
  "page.events.desc": "Tous les événements ingérés par l'agent",
  "page.executions.title": "Exécutions de l'agent",
  "page.executions.desc": "Décisions et actions effectuées par l'IA",
  "page.incidents.title": "Incidents",
  "page.incidents.desc": "Incidents détectés et leur statut",
  "page.timeline.title": "Chronologie",
  "page.timeline.desc": "Activité unifiée en temps réel",

  "detail.section.overview": "Vue d'ensemble",
  "detail.section.timing": "Horodatage",
  "detail.section.timeline": "Chronologie",
  "detail.section.description": "Description",
  "detail.section.errorMessage": "Message d'erreur",
  "detail.section.resultJson": "Résultat JSON",
  "detail.section.aiSummary": "Résumé IA",
  "detail.aiSummary.fr": "Résumé IA (FR)",
  "detail.aiSummary.en": "Résumé IA (EN)",
  "detail.aiSummary.originalLang": "langue d'origine",
  "detail.aiSummary.unavailable": "Aucun résumé disponible dans cette langue",

  "field.status": "Statut",
  "field.decision": "Décision",
  "field.action": "Action",
  "field.provider": "Fournisseur d'analyse",
  "field.confidence": "Score de confiance",
  "field.retryCount": "Nombre de tentatives",
  "field.analysisLanguage": "Langue d'analyse",
  "field.createdAt": "Créé le",
  "field.startedAt": "Démarré le",
  "field.completedAt": "Terminé le",
  "field.resolvedAt": "Résolu le",
  "field.title": "Titre",
  "field.severity": "Sévérité",
  "field.description": "Description",

  "entity.execution": "Exécution",
  "entity.incident": "Incident",
  "entity.event": "Événement",

  "timeline.empty": "Aucun événement de chronologie pour cette exécution",
  "timeline.error": "Impossible de charger la chronologie",
};

const dict = { en, fr } as const;

type Key = keyof typeof en;

const I18nContext = createContext<{ lang: Lang; setLang: (l: Lang) => void; t: (k: Key) => string }>({
  lang: "fr",
  setLang: () => {},
  t: (k) => k,
});

export function I18nProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Lang>("fr");

  useEffect(() => {
    if (typeof window === "undefined") return;
    const saved = localStorage.getItem("lang") as Lang | null;
    if (saved === "fr" || saved === "en") setLangState(saved);
  }, []);

  const setLang = (l: Lang) => {
    setLangState(l);
    if (typeof window !== "undefined") localStorage.setItem("lang", l);
  };

  // Fallback to English if a key is missing in current language
  const t = (k: Key) => (dict[lang] as Record<string, string>)[k] ?? (en as Record<string, string>)[k] ?? k;
  return <I18nContext.Provider value={{ lang, setLang, t }}>{children}</I18nContext.Provider>;
}

export const useI18n = () => useContext(I18nContext);
