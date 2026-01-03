# MediaRatings Platform – Projektprotokoll

## Technische Architektur

Das Projekt **MediaRatings Platform** wurde als serverseitige REST-API umgesetzt und folgt einer **Layered Architecture**, um Wartbarkeit, Testbarkeit und Erweiterbarkeit sicherzustellen.  
Die Anwendung verwendet **PostgreSQL als persistente Datenbank** und kann sowohl **lokal** als auch **containerisiert über Docker** betrieben werden.

Die Architektur ist in folgende Schichten unterteilt:

---

### API Layer

Der API-Layer ist verantwortlich für die Kommunikation über HTTP.

**Aufgaben:**
- Starten eines HTTP-Servers mittels `HttpListener`
- Routing von Requests über eine zentrale `Router`-Klasse
- Authentifizierung über JWT-Tokens
- Serialisierung und Deserialisierung von JSON
- Rückgabe strukturierter HTTP-Antworten

**Zentrale Klassen:**
- `Program.cs` – Initialisiert Abhängigkeiten und startet den Server
- `Router` – Ordnet HTTP-Methoden und Pfade den Controllern zu
- Controller-Klassen:
  - `AuthController`
  - `FavoritesController`
  - `LeaderboardController`
  - `MediaController`
  - `RatingController`
  - `UserController`

Der API-Layer übernimmt nur die Verarbeitung von HTTP-Anfragen und gibt die eigentliche Logik an andere Schichten weiter.

---

### Application Layer

Diese Schicht kapselt **anwendungsnahe Logik**, die nicht direkt zur Domain gehört.

**Aufgaben:**
- Authentifizierung und Autorisierung
- Erzeugung und Validierung von JWT-Tokens
- Datenübertragung zwischen API und Domain mittels DTOs

**Wichtige Komponenten:**
- `JwtService` – erstellt und validiert JWT-Tokens
- DTOs:
  - `CreateMediaDto`
  - `EditRatingDto`
  - `LoginDto`
  - `RateMediaDto`
  - `RegisterDto`
  - `UpdateMediaDto`
  - `UpdateProfileDto`

---

### Domain Layer

Der Domain Layer bildet den **fachlichen Kern** der Anwendung.

**Inhalte:**
- Modelklassen:
  - `Game`
  - `MediaEntry`
  - `Movie`
  - `Series`
  - `UserAccount`
  - `UserRating`
- Interfaces:
  - `IFavoritesManager`
  - `IMediaEntry`
  - `IMediaManager`
  - `IMediaRepository`
  - `IRatingManager`
  - Repository-Interfaces
- Enums:
  - `MediaType`
  - `Genres`

**Beispiele für Domain-Logik:**
- Berechnung der durchschnittlichen Bewertung eines Mediums
- Überprüfung, ob ein Benutzer ein Rating bearbeiten oder bestätigen darf
- Verhindern von doppelten Likes oder Favoriten

---

### Infrastructure Layer

Der Infrastructure Layer implementiert technische Details.

**Aufgaben:**
- Datenbankzugriff über PostgreSQL (`Npgsql`)
- Umsetzung der Repository-Interfaces
- Passwort-Hashing
- Initialisierung der Datenbank

**Zentrale Klassen:**
- `FavoritesRepository`
- `MediaRepository`
- `RatingRepository`
- `UserRepository`
- `DatabaseInitializer`
- `PasswordHasher`

Die Daten werden **persistent in PostgreSQL gespeichert**, wobei der Betrieb über **Docker-Container** erfolgt.

---

### Test Layer

Der Test Layer enthält **Unit-Tests**, die unabhängig von Datenbank und API ausgeführt werden können.

**Framework:** xUnit

Getestet wird hauptsächlich die **Domain-Logik**, da diese kritisch für die korrekte Funktion der Anwendung ist.

---

## SOLID-Prinzipien

Im Projekt wurden mehrere **SOLID-Prinzipien** bewusst angewendet. Zwei davon werden im Folgenden anhand konkreter Beispiele erklärt.

---

### Single Responsibility Principle (SRP)

> Eine Klasse sollte genau eine Verantwortung haben.

**Beispiele:**
- `JwtService` ist ausschließlich für das Erstellen und Validieren von JWT-Tokens zuständig.
- `UserRepository` kümmert sich nur um Datenbankzugriffe für Benutzer.
- `RatingManager` verarbeitet ausschließlich Logik rund um Bewertungen.

➡️ Änderungen an der Authentifizierung beeinflussen somit keine andere Funktionalität.

---

### Dependency Inversion Principle (DIP)

> High-Level-Module sollen nicht von Low-Level-Modulen abhängen, sondern von Abstraktionen.

**Beispiele:**
- Controller arbeiten mit Repositories über Interfaces.
- Die Domain kennt keine konkrete Datenbankimplementierung.
- PostgreSQL kann theoretisch durch eine andere Datenquelle ersetzt werden.

➡️ Die Anwendung bleibt flexibel und testbar.

---

## Unit Testing Strategie & Coverage

Die Unit-Tests konzentrieren sich auf **Geschäftslogik**, da diese unabhängig von der Infrastruktur getestet werden kann.

### Getestete Bereiche:

#### User Ratings
- Bestätigen von Ratings
- Bearbeiten und Löschen von Ratings
- Like- und Unlike-Logik
- Verhindern von mehrfachen Likes
- Berechtigungsprüfungen

#### User Account
- Erstellen von Benutzern
- Favoriten hinzufügen und entfernen
- Verhindern von doppelten Favoriten
- Verknüpfung von Ratings mit Benutzern

Die Tests decken **kritische Kernfunktionen** ab und stellen sicher, dass Regeln auch bei zukünftigen Änderungen eingehalten werden.

---

## Lessons Learned

Während der Umsetzung wurden mehrere wichtige Erkenntnisse gewonnen:

- Eine saubere Schichtenarchitektur spart langfristig Zeit, auch wenn sie anfangs aufwendiger ist.
- Kleine Controller und klare Verantwortlichkeiten erleichtern Debugging erheblich.

---

## Probleme & Lösungen

### Docker & Datenbank-Initialisierung

Beim erstmaligen Einsatz von Docker traten Probleme auf, obwohl alle Container korrekt liefen. Beim Testen über Postman wurde folgender Fehler zurückgegeben:\
`Server error: 42P01: relation "users" does not exist`

**Ursache:**
Die API startete schneller als der PostgreSQL-Container, wodurch die Tabellen noch nicht existierten.

**Lösung:**
Die Initialisierung der Datenbank wurde verzögert, bis eine erfolgreiche Verbindung zur Datenbank hergestellt wurde:
```csharp
var connected = false;
while (!connected)
{
    try
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        connected = true;
    }
    catch
    {
        Console.WriteLine("Warte auf Datenbank...");
        await Task.Delay(1000);
    }
}

dbInitializer.Initialize();
```

---

## Zeitaufwand (Tracked Time)

**Hinweis zur Zeitaufzeichnung**:
Die angegebenen Zeiten stellen nur Näherungen dar. Eine exakte Zeitaufzeichnung während der Projektumsetzung wurde
nicht durchgeführt und die Zeiten wurden im Nachhinein realistisch geschätzt.

| Aufgabe                                   | Zeitaufwand |
|-------------------------------------------|-------------|
| Projektstruktur & Architektur             | 3 h         |
| Datenbank & PostgreSQL                    | 3 h         |
| JWT Authentifizierung                     | 4 h         |
| Controller & Routing                      | 6 h         |
| Domain-Logik                              | 3 h         |
| Unit Tests                                | 2 h         |
| Docker & Deployment                       | 3 h         |
| Debugging & Fehlerbehebung                | 5 h         |
| **Gesamt**                                | **29 h**    |
