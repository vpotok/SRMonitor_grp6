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

+------------+      HTTP      +------------+      HTTP        +-------------+
|   Agent    | <-------------> |   Core     | <-------------> | TokenService|
| (Raspberry)|                 |            |                 |             |
+------------+                 +------------+                 +-------------+
      |                              |                               |
      | Shelly API                   | MySQL                         |
      v                              v                               v
+-------------+               +-----------------+             +-------------+     
|  IoT Device |               |     Database    |             |    Redis    |
|  (Shelly)   |               +-----------------+             +-------------+
+-------------+

+---------------------------------------------+
|               Web Frontend                  |
|  - Login                                    |
|  - Ping-IP Verwaltung (/admin)              |
|  - Shelly-Loganzeige (/shelly-admin)        |
+---------------------------------------------+

+------------------+        Internet        +------------------+
|     Kunde        | <------------------->  |    Redmine       |
+------------------+                        +------------------+



```

## 🧩 ER-Diagramme
![5856972861070231318 (1)](https://github.com/user-attachments/assets/dfe87b9c-c339-4112-815d-45b61404432b)



## 📦 Klassendiagramme


![include](https://github.com/user-attachments/assets/f995079b-2ca0-481b-892d-bc8d8cbccc49)



## 👥 Teammitglieder

- Victoria Potok  
- Thomas Bacher  
- Ibrahim Farghali  
- Alexander Zimmermann  
- Fabian Strohmeier  

## 🚀 Anwendung starten

```bash
# Web App - Backend - TokenService
git clone https://github.com/vpotok/SRMonitor_grp6
/SRMonitor_grp6/ContainerServices$ 
docker compose up --build

# Agent
The Agnent needs to be installed on a Raspberry Pi, with WLAN - Hotspot capabilities.

