# Intermediate HandIn - Projektprotokoll

## Technische Architektur
Das Projekt ist aufgrund der Anforderungen auf Moodle nach einer **Layered Architecture** aufgebaut, bestehend aus folgenden Schichten:

### API
- Implementiert den HTTP-Server (`HttpListener`) und die Routing-Logik  
- Verarbeitet HTTP-Requests, deserialisiert JSON und ruft Services aus der Application-Layer auf  

### Application Layer
- Vermittelt zwischen API und Domain und verarbeitet Daten für die Geschäftslogik  
- **DTOs:** `CreateMediaDto`, `UpdateMediaDto`, `RegisterDto`, `LoginDto` – dienen zum strukturierten Datentransfer zwischen API und Domain  
- **Services:** `JwtService` – erstellt und validiert JWT-Tokens, kapselt Authentifizierungslogik  

### Domain Layer
- Definiert die **Entities und Interfaces:** `MediaEntry`, `UserAccount`, `UserRating`, `IMediaEntry`  
- Enthält **Enums:** `Genres`, `MediaType`  
- Beinhaltet die Domain-Logik (z. B. Berechnung der durchschnittlichen Bewertung)  

### Infrastructure Layer
- Verantwortlich für den Datenzugriff (`UserRepository`) und die Datenbankverbindung (`Database`)  
- Implementiert konkrete Interaktionen mit PostgreSQL über `Npgsql`  
- Enthält Security-Utilities wie `PasswordHasher`  

### Test Layer
- Enthält Unit-Tests für Domain-Logik und Service-Funktionen  
- Verwendet **xUnit** zum Testen von Kernfunktionen wie:  
  - `UserRating` (bewerten, bearbeiten, löschen, liken)  
  - `UserAccount` (Erstellen, Media-Management, Favoriten, Ratings)  

---

## SOLID-Prinzipien
Bei der Umsetzung wurde darauf geachtet, dass die Architektur den **SOLID-Prinzipien** folgt, um wartbaren und erweiterbaren Code zu gewährleisten:

### Single Responsibility Principle (SRP)
Jede Klasse hat genau eine klar definierte Verantwortung:
- `UserAccount` verwaltet Benutzeraktionen (z. B. Medien hinzufügen, Bewertungen liken).  
- `MediaManager`, `FavoritesManager` und `RatingManager` trennen die Verantwortlichkeiten für jeweilige Datenoperationen.  
- `JwtService` ist ausschließlich für die Token-Erstellung und -Validierung zuständig.  

### Open/Closed Principle (OCP)
Das System ist **erweiterbar, aber nicht veränderbar**:
- Neue Medientypen (z. B. *Podcast*) können durch Vererbung von `MediaEntry` hinzugefügt werden, ohne bestehende Klassen anzupassen.  
- Neue Genres können über das `Genres`-Enum ergänzt werden, ohne Logik zu brechen.  

### Liskov Substitution Principle (LSP)
Abgeleitete Klassen (`Movie`, `Series`, `Game`) können überall dort verwendet werden, wo ein `IMediaEntry` erwartet wird, ohne dass das Verhalten beeinträchtigt wird.  

### Interface Segregation Principle (ISP)
Das Interface `IMediaEntry` enthält nur Eigenschaften, die für alle Medientypen relevant sind.  
So werden Klassen nicht gezwungen, ungenutzte Methoden zu implementieren.  

### Dependency Inversion Principle (DIP)
Höhere Schichten (z. B. API oder Application Layer) hängen von **Abstraktionen** statt von konkreten Implementierungen ab:
- Die API ruft Services über Interfaces auf.  

---

## Unit Tests

### User Rating
- **ConfirmRatingSuccessTest:** prüft, dass ein Rating nach Bestätigung korrekt als bestätigt markiert wird.  
- **EditRatingSuccessTest / EditRatingFailTest:** stellt sicher, dass ein Benutzer seine Bewertung ändern kann, andere Benutzer dies jedoch nicht dürfen.  
- **DeleteRatingSuccessTest / DeleteRatingByOtherUserFailTest:** prüft das Löschen von Ratings unter Berücksichtigung von Berechtigungen.  
- **AddLikeSuccessTest / AddLikeTwiceFailTest / RemoveLikeSuccessTest:** testet die Like-/Unlike-Funktionalität, um doppelte Likes zu verhindern und Likes korrekt zu entfernen.  

### User Account
- **CreateUserSuccessTest:** prüft, dass Benutzer korrekt erstellt werden.  
- **AddMediaEntryTest / RemoveMediaEntrySuccessTest:** testet das Hinzufügen und Entfernen von MediaEntries im MediaManager.  
- **AddFavoriteSuccessTest / AddSameFavoriteTwiceFailTest / RemoveFavoriteSuccessTest:** prüft die Favoritenverwaltung und verhindert Duplikate.  
- **AddRatingSuccessTest / RemoveRatingSuccessTest:** stellt sicher, dass Ratings korrekt zum Benutzer hinzugefügt und entfernt werden.  
- **LikeRatingSuccessTest / LikeOwnRatingFailTest / LikeSameRatingTwiceFailTest:** prüft die Like-Logik für Ratings inkl. Berechtigungen.  
- **GetMediaByIdSuccessTest:** validiert, dass MediaEntries korrekt anhand der ID abgerufen werden.  

---

## Probleme & Lösungen
Das größte Problem, das auch viel Zeit beim Lösen in Anspruch genommen hat, war, dass beim Bearbeiten eines bereits erstellten Media Entrys die Änderungen **nicht übernommen wurden**.  
Dabei wurde keine Fehlermeldung ausgegeben, aber stattdessen ein leeres Media Entry zurückgegeben.  

Die Ursache war, dass keine `JsonSerializerOptions` bei der `UpdateMediaDto` angegeben wurden, wodurch **Case-Sensitive Matching** galt.  
Das JSON im vorgegebenen Postman-Skript benutzt klein geschriebene Feldnamen (`"title"`, `"description"`, …), während `UpdateMediaDto` groß geschriebene Properties verwendet (`Title`, `Description`, …).  
Da `System.Text.Json` standardmäßig case-sensitive ist, wurden Werte zu `null`, `0` oder leeren Listen gesetzt, wenn die Namen nicht exakt passten.

**Lösung:**  
Durch das Hinzufügen folgender Zeilen wurde Case-Insensitive-Deserialisierung aktiviert:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};
