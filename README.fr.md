🧠 AI Incident Response Agent

Plateforme IA opérationnelle autonome pour la détection, la décision, l’approbation, l’exécution d’actions et l’observabilité en temps réel.

---

## 🚀 Vision

Construire un agent IA autonome contrôlé capable de :

- Détecter des événements critiques
- Comprendre le contexte via IA + règles métier
- Prendre des décisions déterministes
- Exécuter des actions de manière sécurisée
- Demander une validation humaine si nécessaire
- Observer les résultats
- Relancer les actions échouées avec backoff
- Fournir une traçabilité complète

👉 Objectif : **automatiser les workflows opérationnels de manière fiable, explicable et contrôlée**

---

## 🧩 Architecture

```text
Event → Detection → Analysis → Decision → Policy → Action → Feedback → Memory → Realtime → Metrics
Principes clés
L’IA n’est pas le décideur final
Les règles métier contrôlent l’exécution
Les actions critiques nécessitent une validation humaine
Traçabilité complète
Idempotence + action locks
Observabilité intégrée : logs, métriques, health checks

🏗️ Structure du projet
src/
├─ Api              → API HTTP, Swagger, Auth, SignalR, Health Checks
├─ Worker           → Traitement asynchrone
├─ Domain           → Entités métier
├─ Application      → Orchestrateur + logique
├─ Infrastructure   → EF Core, PostgreSQL, Ollama
├─ Contracts        → DTOs

ops-center/         → UI React (dashboard Ops)

⚙️ Stack technique
.NET 8
ASP.NET Core Web API
PostgreSQL
Entity Framework Core
Worker background
SignalR (temps réel)
JWT Auth + RBAC
Ollama (LLM local)
Swagger
React + Tailwind
Health Checks
Logging structuré

🧠 Composants principaux
Composant	Rôle
Event Ingestion	Capture des événements
Worker	Traitement asynchrone
Orchestrator	Pilotage complet
AI Analyzer	Analyse IA
Decision Engine	Décisions métier
Policy Engine	Sécurité
Manual Approval	Validation humaine
Action Executor	Exécution
Ticketing	Module local
Memory	Contexte
Realtime	SignalR
Metrics	Monitoring

🔐 Sécurité & contrôle
IA toujours encadrée
Règles métier obligatoires
agent_action_locks évitent les doublons
Validation humaine supportée
Retry avec backoff
Logs complets

🌍 Capacités IA
Ollama local
JSON strict
Guardrails
Retry/fallback
Résumés bilingues :
FR
EN
lang propagé de bout en bout

🖥️ Ops Center

Dashboard SaaS :

KPI
Metrics techniques
Events
Executions
Incidents CRUD
Tickets
Approval manuel
Timeline
Temps réel
Auth / RBAC

🔑 Auth / RBAC
User	Password	Role
admin	Admin123!	Admin
operator	Operator123!	Operator
viewer	Viewer123!	Viewer

🚀 Lancement
dotnet run --project src/AiIncidentResponseAgent.Api
dotnet run --project src/AiIncidentResponseAgent.Worker
cd ops-center
npm run dev

🤖 Ollama
ollama pull llama3
ollama run llama3

📊 Metrics
GET /api/metrics/overview
GET /api/metrics/technical

❤️ Health Checks
GET /health
GET /health/live
GET /health/ready

📊 État actuel

✅ Implémenté
Architecture propre
Orchestrateur complet
IA Ollama
Retry avec backoff
Approval workflow
Ticket module réel
CRUD incidents
Auth + RBAC
SignalR realtime
Metrics
Logs structurés
Health checks

🚧 Limites actuelles
Pas encore d’intégration externe réelle (BlockTicket)
Observabilité avancée à compléter (OpenTelemetry)
Audit trail détaillé manquant
Tests incomplets
Docker non finalisé
Hardening sécurité à compléter

🎯 Objectif

Construire une plateforme IA autonome :

Fiable
Contrôlée
Explicable
Scalable


## 🚧 Éléments en cours de développement

Le projet évolue activement vers une plateforme d’agent IA autonome prête pour la production.

Les améliorations suivantes sont prévues ou en cours :

- 🔌 Intégration d’une API externe réelle pour BlockTicket (remplacement ou extension du module local)
- 📡 Intégration OpenTelemetry (traces distribuées, observabilité avancée)
- 🐳 Finalisation du Docker Compose et configuration proche production
- 🧪 Extension des tests automatisés (retries, approvals, RBAC, audit logs)
- 📜 Interface Audit Logs dans l’Ops Center
- 📸 Screenshots finaux et scénarios de démonstration avec dataset propre

Ces améliorations renforceront la fiabilité, l’observabilité et la maturité du produit.

👨 Author

Rachid Bariz