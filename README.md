# Slottet

En webbaseret plejestyringsplatform til en bofacilitet. Plejepersonalet kan administrere beboeroplysninger, registrere humør og risikoniveauer, registrere vagttyper, administrere medicin og tilføje statusnoter. Alle vigtige dataændringer skrives til en revisionslog for ansvarssporing og GDPR-overholdelse.

Systemet består af to uafhængigt deploybare applikationer:

- **API** - ASP.NET Core Web API. Håndterer autentificering, forretningslogik og al databaseadgang.
- **SlottetBlazor** - ASP.NET Core Blazor Server. Frontenden. Kommunikerer udelukkende med API'et over HTTP.

---

## Arkitektur

### Systemtopologi

```
Browser
  |
  | HTTP (SignalR WebSocket)
  v
SlottetBlazor  (Blazor Server, port 5050)
  |
  | HTTP - server-til-server (internt Docker-netværk i produktion)
  v
API            (ASP.NET Core Web API, port 5000)
  |
  | TCP
  v
MariaDB / MySQL
```

Da Blazor kører som en Server-app, afvikles al komponentlogik på serveren. HTTP-kald til API'et er dermed server-til-server og passerer ikke gennem brugerens browser.

### Clean Architecture

API-projektet følger Clean Architecture med tre lag.

```
┌──────────────────────────────────────────────────────┐
│  API (Præsentationslag)                              │
│  Controllers · Middleware · Program.cs               │
│  Afhænger af: Application                           │
├──────────────────────────────────────────────────────┤
│  Application (Applikationslag)                       │
│  DTOs · IRepository-interfaces                      │
│  Afhænger af: Domain                                │
├──────────────────────────────────────────────────────┤
│  Domain (Domænelag)                                  │
│  Entiteter · Enums · Domæneregler                   │
│  Afhænger af: ingenting                             │
├──────────────────────────────────────────────────────┤
│  Infrastructure (Infrastrukturlag)                   │
│  EF Core DbContext · Repository-implementeringer    │
│  Migrationer · Seed-data                            │
│  Afhænger af: Application + Domain                  │
└──────────────────────────────────────────────────────┘
```

**Repository-mønsteret** er implementeret for alle 11 domæneområder. Interfaces defineres i `Application/Interfaces/Repositories/` og implementeres i `Infrastructure/Repositories/`. Controllerne afhænger udelukkende af interfaces — aldrig af konkrete klasser eller EF Core direkte.

Dette betyder:
- Controllere kan unit-testes uden en rigtig database (in-memory EF brugt i tests)
- Infrastructure-laget kan udskiftes uden at ændre applikations- eller domænelaget
- Afhængighedspielen peger altid indad mod domænet

---

## Teknologistak

| Lag              | Teknologi                               |
|------------------|-----------------------------------------|
| Frontend         | ASP.NET Core Blazor Server (.NET 9)     |
| API              | ASP.NET Core Web API (.NET 9)           |
| Autentificering  | ASP.NET Core Identity + JWT Bearer      |
| ORM              | Entity Framework Core 9 + Pomelo MySQL  |
| Database         | MariaDB / MySQL 8                       |
| Containerisering | Docker + Docker Compose                 |

---

## Projektstruktur

```
Slottet/
  Slottet/
    API/
      Controllers/        12 controllere - afhænger kun af IRepository-interfaces
      Middleware/         JWT-validering og fejlhåndtering
      Program.cs          DI-registrering, middleware-pipeline

    Application/
      DTOs/               Request- og response-DTO'er for alle domæneområder
      Interfaces/
        Repositories/     11 IRepository-interfaces (kontrakterne)

    Domain/
      Entities/           EF Core-entitetsklasser
      Enums/              Domæneenums (RiskLevel, Mood, ShiftType m.fl.)

    Infrastructure/
      Data/               ApplicationDbContext (IdentityDbContext<Employee>)
      Migrations/         EF Core-migreringsfiler
      Repositories/       11 EF Core repository-implementeringer
      Seeders/            Initial seed-data

    SlottetBlazor/        Blazor Server frontend - kalder API over HTTP
    Slottet.Tests/        xUnit-enhedstests (27 tests, in-memory EF)

    docker-compose.yml    Orkestrerer API + Blazor som separate services
    .env                  Lokale hemmeligheder (git-ignoreret, commit aldrig)
    .env.example          Skabelon - kopier til .env og udfyld værdier
    README.md             Udviklerreference
```

### Repository-interfaces

| Interface                    | Ansvar                                      |
|------------------------------|---------------------------------------------|
| `IResidentRepository`        | Beboere inkl. medicin, statusser, lokation  |
| `IShiftRepository`           | Vagtregistrering                            |
| `IMedicinRepository`         | Fast medicin                                |
| `IPNMedicinRepository`       | PN-medicin (ved behov)                      |
| `IPhoneNumberRepository`     | Arbejdstelefoner                            |
| `ISpecialTasksRepository`    | Ansvarsroller med tilknyttede medarbejdere  |
| `IDepartmentTasksRepository` | Afdelingsopgaver                            |
| `ILocationRepository`        | Lokationer/afdelinger                       |
| `IAuthorizationRepository`   | Autorisationsroller (read-only)             |
| `IAuditLogRepository`        | Revisionslog                                |
| `IEmployeeRepository`        | Medarbejder-specifikke EF-operationer       |

---

## Forudsættninger

- .NET 9 SDK
- Docker Desktop (til containeriseret deployment)
- En korende MariaDB- eller MySQL 8-instans

---

## Lokal udvikling (uden Docker)

### 1. Konfigurer miljøvariabler

```
cp Slottet/.env.example Slottet/.env
```

Udfyld databaseoplysninger og en JWT-signeringsnoegle. API'et laeder automatisk .env-filen ved opstart via DotNetEnv. Filen resolves ved at traversere opad fra arbejdskataloget, så placering ved solution-roden daekker API-projektet.

### 2. Start API'et

```
cd Slottet/API
dotnet run
```

Starter på http://localhost:5000 (se launchSettings.json).

### 3. Start Blazor

```
cd Slottet/SlottetBlazor
dotnet run
```

Starter på http://localhost:5140. Laeder ApiBaseUrl fra appsettings.json (standard: http://localhost:5000). Tilsidesætt ved at sættte miljøvariablen ApiBaseUrl inden start.

### 4. Databasemigrationer

Migrationer kører automatisk ved API-opstart. For manuel kørrsel:

```
cd Slottet/API
dotnet ef database update
```

---

## Docker-deployment (distribueret)

API'et og Blazor kører som separate containere på et internt bridge-netvaerk. Blazor kalder API'et via det interne container-hostname slottet-api.

### 1. Konfigurer .env

```
cp Slottet/.env.example Slottet/.env
```

Minimumskraevede variabler:

| Variabel      | Beskrivelse                          |
|---------------|--------------------------------------|
| DB_HOST       | Database-servers hostname eller IP   |
| DB_PORT       | Databaseport (standard: 3306)        |
| DB_NAME       | Databasenavn                         |
| DB_USER       | Databasebrugernavn                   |
| DB_PASSWORD   | Databaseadgangskode                  |
| JWT_KEY       | JWT-signeringsnøgle, min. 32 tegn   |

### 2. Byg og start

```
cd Slottet
docker compose up --build -d
```

- API health check: http://localhost:5000/health
- Blazor frontend:  http://localhost:5050

Blazor venter på, at API'ets health check passerer, inden det starter.

### 3. Stop

```
docker compose down
```

### Netværkstopologi

```
Vaertsmaskine
  port 5000 --> slottet-api:8080     (API-container)
  port 5050 --> slottet-blazor:8080  (Blazor-container)

slottet-net (internt bridge-netværk)
  slottet-blazor --> http://slottet-api:8080
```

Portene kan tilsidesættes via API_PORT og BLAZOR_PORT i .env.

---

## Autentificering

Login sker via POST /api/auth/login med en e-mailadresse og en numerisk PIN-kode.

### Login-flow

1. Brugeren indsender e-mail og PIN i Blazors login-formular (LoginHeader.razor).
2. API'et validerer PIN-koden mod den hash, der er gemt af ASP.NET Core Identity.
3. Ved succes udsteder API'et et signeret JWT-token gyldigt i 12 timer.
4. Blazor gemmer tokenet i localStorage via JavaScript interop: localStorage.setItem("authToken", token).
5. Efterfoelgende autentificerede API-kald vedhaefter tokenet som en Authorization: Bearer header.
6. Ved sideindlaesning laeder Blazor tokenet fra localStorage, dekoder JWT-payload (base64) og gendanner sessionstilstanden uden en tur-retur til API'et.

### JWT-claims

| Claim                     | Indhold                                      |
|---------------------------|----------------------------------------------|
| ClaimTypes.NameIdentifier | Medarbejder-ID (heltal)                      |
| ClaimTypes.Name           | Fuldt navn (Fornavn + Efternavn)             |
| EmployeeId                | Duplikat af NameIdentifier                   |
| AuthorizationRole         | Rolle fra Authorization-tabellen             |
| ClaimTypes.Role           | Identity-rolle (Admin / Vagtansvarlig / ...) |
| ShiftType                 | Aktuel vagttype, hvis der er en i dag        |
| jti                       | Unikt token-ID (Guid)                        |

Tokens er ikke revocerbare server-side. Logout rydder kun tokenet fra localStorage.

### localStorage

| Noegle     | Vaerdi               | Skrevet af        |
|------------|----------------------|-------------------|
| authToken  | Signeret JWT-streng  | LoginHeader.razor |

### Roller

| Identity-rolle  | Politik          | Adgangsniveau                                         |
|-----------------|------------------|-------------------------------------------------------|
| Admin           | RequireAdmin     | Fuld adgang, herunder medarbejderadministration, læse log, slette medarbejdere / borgere hvis der kommer en GPDR forespørgsel      |
| Vagtansvarlig   | RequireScheduler | Vagtplanlægning, læsning af alle medarbejdere       |
| Plejepersonale  | RequireCareStaff | Læse/opdatere beboere, registrere vagter og statusser, humør, og risiko |

Første login returnerer requiresSetup: true, hvis medarbejderen endnu ikke har en PIN, hvilket udloser PIN-opsættningsmodalen.

---

## Miljøvariabler

### API

| Variabel      | Kraevet | Standard                   | Beskrivelse                           |
|---------------|---------|----------------------------|---------------------------------------|
| DB_HOST       | Ja      | -                          | Database-servers host                 |
| DB_PORT       | Nej     | 3306                       | Database-servers port                 |
| DB_NAME       | Ja      | -                          | Databasenavn                          |
| DB_USER       | Ja      | -                          | Databasebrugernavn                    |
| DB_PASSWORD   | Ja      | -                          | Databaseadgangskode                   |
| Jwt__Key      | Ja      | -                          | JWT-signeringsnoegle (min. 32 tegn)   |
| Jwt__Issuer   | Nej     | SlottetApi                 | JWT issuer-claim                      |
| Jwt__Audience | Nej     | SlottetBlazor              | JWT audience-claim                    |
| BLAZOR_ORIGIN | Nej     | http://localhost:5140,...  | Kommaseparerede CORS-tilladte origins |

__ (dobbelt understregning) er .NET-konfigurationssektionsseparatoren. Jwt__Key svarer til builder.Configuration["Jwt:Key"].

### Blazor

| Variabel   | Kraevet | Standard               | Beskrivelse       |
|------------|---------|------------------------|-------------------|
| ApiBaseUrl | Nej     | http://localhost:5000  | API'ets basis-URL |

I Docker sætttes ApiBaseUrl automatisk til http://slottet-api:8080 af docker-compose.yml.

---

## API-endpoints

### Autentificering

| Metode | Sti                      | Politik      | Beskrivelse                           |
|--------|--------------------------|--------------|---------------------------------------|
| POST   | /api/auth/login          | Ingen        | Login - returnerer JWT                |
| POST   | /api/auth/setup-pincode  | Ingen        | Sæt PIN for nye brugere (ved første login)             |
| POST   | /api/auth/assign-role    | RequireAdmin | Tildel Identity-rolle til medarbejder |

### Medarbejdere

| Metode | Sti                           | Politik          | Beskrivelse                                      |
|--------|-------------------------------|------------------|--------------------------------------------------|
| GET    | /api/employees                | RequireScheduler | Hent alle medarbejdere                           |
| GET    | /api/employees/{id}           | RequireCareStaff | Hent enkelt medarbejder                          |
| POST   | /api/employees                | RequireAdmin     | Opret medarbejder                                |
| PUT    | /api/employees/{id}           | RequireAdmin     | Opdater medarbejder                              |
| DELETE | /api/employees/{id}           | RequireAdmin     | GDPR-slet medarbejder og alle tilknyttede data   |
| GET    | /api/employees/job-roles      | RequireCareStaff | Hent alle jobbetegnelser                         |
| PUT    | /api/employees/{id}/job-roles | RequireAdmin     | Opdater medarbejders jobbetegnelser              |

### Beboere

| Metode | Sti                    | Politik          | Beskrivelse                                    |
|--------|------------------------|------------------|------------------------------------------------|
| GET    | /api/resident          | Ingen            | Hent alle beboere med statusser                |
| GET    | /api/resident/public   | Ingen            | Offentlig storskaemsvisning (ingen persondata) til personale skærm |
| PUT    | /api/resident/{id}     | RequireCareStaff | Opdater beboer, humor, risikoniveau, status    |

### Vagter

| Metode | Sti               | Politik          | Beskrivelse                                       |
|--------|-------------------|------------------|---------------------------------------------------|
| POST   | /api/shifts       | RequireCareStaff | Opret eller opdater dagens vagt                   |
| GET    | /api/shifts/today | RequireCareStaff | Hent den indloggede medarbejders vagt i dag       |

### Medicin

| Metode | Sti                       | Politik | Beskrivelse                         |
|--------|---------------------------|---------|-------------------------------------|
| POST   | /api/medicin              | Ingen   | Opret fast medicin for en beboer    |
| PATCH  | /api/medicin/{id}/taken   | Ingen   | Marker medicin som taget/ikke taget |
| PUT    | /api/medicin/{id}         | Ingen   | Opdater administreringstidspunkt    |

### PN-medicin

| Metode | Sti                  | Politik | Beskrivelse                          |
|--------|----------------------|---------|--------------------------------------|
| POST   | /api/pnmedicin       | Ingen   | Opret PN-medicin for en beboer       |
| PUT    | /api/pnmedicin/{id}  | Ingen   | Opdater PN-medicinens tidspunkt/type |

### Telefonnumre arbejdstelefoner

| Metode | Sti                                 | Politik | Beskrivelse                           |
|--------|-------------------------------------|---------|---------------------------------------|
| GET    | /api/phonenumber                    | Ingen   | Hent alle telefonnumre                |
| GET    | /api/phonenumber/{id}               | Ingen   | Hent enkelt telefonnummer             |
| POST   | /api/phonenumber                    | Ingen   | Opret telefonnummer                   |
| PUT    | /api/phonenumber/{id}               | Ingen   | Opdater telefonnummer                 |
| PATCH  | /api/phonenumber/{id}/assignment    | Ingen   | Tilknyt medarbejder til telefonnummer |
| DELETE | /api/phonenumber/{id}               | Ingen   | Slet telefonnummer                    |

### Særlige opgaver (ansvarsroller)

| Metode | Sti                              | Politik | Beskrivelse                              |
|--------|----------------------------------|---------|------------------------------------------|
| GET    | /api/specialtasks                | Ingen   | Hent alle ansvarsroller med medarbejdere |
| POST   | /api/specialtasks                | Ingen   | Opret ny ansvarsrolle                    |
| PUT    | /api/specialtasks/{id}/employees | Ingen   | Opdater tilknyttede medarbejdere         |
| DELETE | /api/specialtasks/{id}           | Ingen   | Slet ansvarsrolle                        |

### Afdelingsopgaver

| Metode | Sti                       | Politik | Beskrivelse                |
|--------|---------------------------|---------|----------------------------|
| GET    | /api/departmenttasks      | Ingen   | Hent alle afdelingsopgaver |
| POST   | /api/departmenttasks      | Ingen   | Opret afdelingsopgave      |
| PUT    | /api/departmenttasks/{id} | Ingen   | Opdater afdelingsopgave    |
| DELETE | /api/departmenttasks/{id} | Ingen   | Slet afdelingsopgave       |

### Lokationer

| Metode | Sti                  | Politik | Beskrivelse          |
|--------|----------------------|---------|----------------------|
| GET    | /api/locations       | Ingen   | Hent alle lokationer |
| GET    | /api/locations/{id}  | Ingen   | Hent enkelt lokation |
| POST   | /api/locations       | Ingen   | Opret lokation       |
| PUT    | /api/locations/{id}  | Ingen   | Opdater lokation     |

### Autorisationer

| Metode | Sti                   | Politik | Beskrivelse                    |
|--------|-----------------------|---------|--------------------------------|
| GET    | /api/authorizations   | Ingen   | Hent alle autorisationsroller  |

### Revisionslog

| Metode | Sti             | Politik | Beskrivelse                               |
|--------|-----------------|---------|-------------------------------------------|
| GET    | /api/auditlog   | Ingen   | Hent alle logposter, sorteret nyest først |

### System

| Metode | Sti      | Politik | Beskrivelse  |
|--------|----------|---------|--------------|
| GET    | /health  | Ingen   | Health check |

---

## Tests

Projektet har 27 unit-tests i `Slottet.Tests/` skrevet med xUnit og in-memory EF Core.

```
cd Slottet/Slottet.Tests
dotnet test
```

Testene dækker `ResidentController` og `DepartmentTasksController` samt domain-entiteter. Da controllerne afhænger af repository-interfaces, instansieres de konkrete repository-klasser direkte i testene med en in-memory database — ingen mocking-framework nødvendigt.

---

## Revisionslog

Alle dataopdaterende handlinger skriver en række til AuditLogs-tabellen med UserId, UserName, Action, Entity, EntityId og TimeStamp. Medarbejdersletning/Borgersletning er en hard delete, der fjerner alle tilknyttede personoplysninger (GDPR).

---

## Udviklingsnoter

- .env-filen er git-ignoreret. du skal selve opsætte din .env før at du kan hoste via docker.
- appsettings.json-filer indeholder ingen secrets - kun sikre strukturelle standardvaerdier.
- I Docker tilsidesætter miljøvariable fra docker-compose.yml appsettings.json via den standard .NET-konfigurationsprioritetskaede.
- Docker health check poller GET /health hvert 30. sekund. Blazor-containeren starter ikke, før API'et reagere.
- Kørsel af dotnet run fra API/ medfører, at DotNetEnv traverser opad og finder .env ved solution-root.

---

## Hosting

Projektet er hostet på en privat Linux-server. Gruppen har valgt at bruge gratis vaerktoejer til at eksponere det på internettet:

- **DuckDNS** - gratis dynamisk DNS. Serveren får et fast hostname (*.duckdns.org) selvom den offentlige IP aendrer sig.
- **Let's Encrypt** - gratis TLS-certifikater via ACME-protokollen. Certifikaterne fornyes automatisk.
- **Nginx Proxy Manager** - kører i Docker og håndterer reverse proxy, SSL-terminering og certifikatfornyelse.

### Overblik over produktionsmiljø

```
Internettet
  |
  | HTTPS (port 443)
  v
Nginx Proxy Manager  (Docker, håndterer TLS + reverse proxy)
  |
  +-- /api/*  --> slottet-api:8080
  +-- /*      --> slottet-blazor:8080

DuckDNS opdaterer DNS-posten når serverens IP aendres (cron-job eller DuckDNS-klient).
Let's Encrypt-certifikat udstedes til DuckDNS-hostname og fornyes automatisk af Nginx Proxy Manager.
```

### Hvad er kørende i Docker på serveren

| Container            | Beskrivelse                                      |
|----------------------|--------------------------------------------------|
| slottet-api          | API-containeren (bygget fra API/Dockerfile)      |
| slottet-blazor       | Blazor-containeren (bygget fra SlottetBlazor/...) |
| nginx-proxy-manager  | Reverse proxy + SSL-terminering                  |
| DuckDNS-klient       | Holder DNS-posten opdateret                      |

---

## Tests

Projektet indeholder et dedikeret test-projekt: ```Slottet.Tests```.

### kør tests

```
cd Slottet/Slottet.Tests
dotnet test
```

### Teststruktur

| Testklasse                       | Hvad testes                                                   |
|----------------------------------|---------------------------------------------------------------|
| ResidentControllerTests          | GetAll, GetById (fundet/ikke fundet), GetPublic med filter    |
| DepartmentTasksControllerTests   | Fuld CRUD: opret, opdater, slet, 404-håndtering              |
| DomainEntityTests                | Domaenobjekter: Employee.HasPincode, RiskLevel, Mood, AuditLog |

Testene bruger en in-memory database (Microsoft.EntityFrameworkCore.InMemory), så der kræves ingen databaseforbindelse for at køre dem.

### XP og test

- **Domænelaget** testes isoleret uden afhængigheder.
- **Controllerene** testes med in-memory database, så HTTP-laget og databaselaget verificeres samlet.

Områder der ikke er dækket af tests:
- Autentificeringscontrolleren (kræver mock af UserManager/SignInManager)
- Blazor-komponenterne (kraver Bunit eller Playwright)
- Integrationstests mod rigtig database
