# SRMonitor – Server-Room-Monitoring

## 🌐 Überblick

SRMonitor ist eine verteilte Anwendung zur Überwachung von Serverräumen, bestehend aus einem lokal installierten Agent (z. B. Raspberry Pi), einem zentralen Core-Service und einer Weboberfläche. Das System erfasst Temperatur- und Türstatusdaten über ein IoT-Gerät (Shelly) und überprüft die Erreichbarkeit von Netzwerkgeräten durch regelmäßige Pings. Zusätzlich können kritische Ereignisse automatisch an ein Ticketsystem (Redmine) weitergeleitet werden.

## 🔧 Features

- **Agent (Raspberry Pi)**  
  Installiert beim Kunden vor Ort. Erfasst Temperatur und Türstatus über einen Shelly Door/Window2 Sensor und sendet die Daten an den zentralen Core. Führt regelmäßig Pings auf vom Kunden konfigurierbare IP-Adressen durch.

- **Core-Service**  
  Das Herzstück der Anwendung. Verwaltet die gesamte Business-Logik und persistiert Daten in einer relationalen Datenbank.

- **Token-Service**  
  Vergibt JWT-Tokens für eine sichere interne Kommunikation auf Basis von Token-based Security.

- **Weboberfläche (Frontend)**  
  - Login-Portal für Kunden und Admins  
  - Kunden sehen ihre eigenen Logs und Ping-Daten  
  - Admins können IP-Listen bearbeiten und Logs einsehen

- **Redmine Integration**  
  Kritische Ereignisse (z. B. Türöffnung außerhalb von Wartungszeiten) erzeugen automatisch Tickets im Redmine-System.

## 🏗️ Architektur
```text
+------------+      HTTPS      +------------+      HTTPS      +-------------+
|   Agent    | <-------------> |   Core     | <-------------> | TokenService|
| (Raspberry)|                 |            |                 |   (JWT DB)  |
+------------+                 +------------+                 +-------------+
      |                              |
      | Shelly API                   | PostgreSQL
      v                              v
+-------------+               +-----------------+
|  IoT Device |               |     Database    |
|  (Shelly)   |               +-----------------+
+-------------+

+---------------------------------------------+
|               Web Frontend                  |
|  - Login                                     |
|  - Ping-IP Verwaltung (/admin)              |
|  - Shelly-Loganzeige (/shelly-admin)        |
+---------------------------------------------+

+------------------+        Internet        +------------------+
|     Kunde        | <------------------->  |    Redmine       |
+------------------+                        +------------------+
```

### Ports

| Dienst            | Port  |
|-------------------|-------|
| Web-Frontend      | 8080  |
| Core-Service      | 5000  |
| Token-Service     | 6000  |
| Redmine           | 3000  |
| PostgreSQL        | 5432  |
| MongoDB (Token)   | 27017 |

## 📡 API-Beschreibung

### Frontend-Endpunkte

| Endpoint        | Beschreibung                                      |
|-----------------|---------------------------------------------------|
| `/login`        | Login-Seite für Kunden und Admins                 |
| `/admin`        | Verwaltung der IP-Tabelle für Server-Pings        |
| `/shelly-admin` | Einsicht der Shelly-Logs durch Admins             |

### Backend-Endpunkte (Core-Service)

| Endpoint           | Beschreibung                                  |
|--------------------|-----------------------------------------------|
| `/api/ip-table`    | Auslesen und Verwalten der IP-Tabelle         |
| `/api/shelly-logs` | Auslesen der gesammelten Shelly-Daten         |
| `/api/ip-logs`     | Protokoll der Ping-Antworten                  |

## 🧩 ER-Diagramme

[ER-Diagramm](./images/ER_Diagramm.png)

## 📦 Klassendiagramme

[UML-Diagramm](./images/UML_Diagramm.png)


## 👥 Teammitglieder

- Victoria Potok  
- Thomas Bacher  
- Ibrahim Farghali  
- Alexander Zimmermann  
- Maximilian Luegger
- Fabian Strohmeier  

## 🚀 Anwendung starten

```bash
git clone https://github.com/dein-repo/srmonitor.git
cd srmonitor
docker compose up --build
