# Database Setup Guide

Denne guide hjælper dig med at sætte MySQL databasen op til Slottet projektet.

## 🎯 Vælg dit Setup

| Option | Bedst til | Svært |
|--------|-----------|-------|
| **XAMPP MySQL** | Lokal development | ⭐ Nemt |
| **Docker Compose** | Containerized | ⭐⭐ Middel |
| **Standalone MySQL** | Server deployment | ⭐⭐⭐ Svært |

---

## Option 1: XAMPP MySQL (Anbefalet til Development)

### 1️⃣ Start XAMPP
```bash
# Windows: Åbn XAMPP Control Panel og klik "Start" ved MySQL
# Eller via command line:
xampp-control.exe
```

### 2️⃣ Installer EF Core Tools (hvis ikke allerede installeret)
```bash
dotnet tool install --global dotnet-ef

# Verificer installation
dotnet ef --version
# Output: Entity Framework Core .NET Command-line Tools 9.0.0
```

### 3️⃣ Tjek Connection String
Rediger `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=SlottetDb;User=root;Password=;SslMode=None;"
  }
}
```

> 💡 **Note**: Standard XAMPP MySQL bruger `root` med tomt password.  
> Hvis du har ændret password, opdater connection string!

### 4️⃣ Opret Database med Migrations
```bash
# Naviger til projekt folder
cd c:\xampp\htdocs\Slottet\Slottet

# Opret migration
dotnet ef migrations add InitialCreate

# Anvend til database
dotnet ef database update
```

Dette opretter MySQL databasen `SlottetDb` med tabeller:
- ✅ Residents
- ✅ Medications  
- ✅ MedicationLogs
- ✅ Staff

### 5️⃣ Verificer Database
Åbn phpMyAdmin: http://localhost/phpmyadmin

Du skulle nu se `SlottetDb` i venstre menu.

---

## Option 2: Docker Compose (Recommended til Produktion-lignende Setup)

### 1️⃣ Start Docker Stack
```bash
cd c:\xampp\htdocs\Slottet
docker-compose up -d
```

Dette starter:
- MySQL 8.0 container (port 3306)
- Slottet app container (port 5000)

### 2️⃣ Database Oprettes Automatisk
Docker compose er konfigureret til at køre migrations automatisk.

### 3️⃣ Tilgå Database
**Via Command Line:**
```bash
docker exec -it slottet-db mysql -u slottet -pSlottet123! SlottetDb
```

**Via MySQL Workbench:**
- Host: localhost
- Port: 3306
- User: slottet
- Password: Slottet123!
- Database: SlottetDb

---

## Option 3: Standalone MySQL Server

Dette opretter en migration i `Migrations/` mappen.

### 2. Anvend Migration til Database

```bash
dotnet ef database update
```

Dette opretter databasen og alle tabeller.

### 3. Verificer Database

Åbn SQL Server Management Studio og tjek for:
- Database: `SlottetDb`
- Tabeller: `Residents`, `Medications`, `MedicationLogs`, `Staff`

## Connection Strings

### Lokal SQL Server (Windows Authentication)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SlottetDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
}
```

### SQL Server LocalDB

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SlottetDb;Trusted_Connection=True;MultipleActiveResultSets=true;"
}
```

### SQL Server med SQL Authentication

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SlottetDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

### Docker SQL Server

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=SlottetDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
}
```

## Almindelige Kommandoer

### Se alle migrations

```bash
dotnet ef migrations list
```

### Tilføj ny migration

```bash
dotnet ef migrations add <MigrationName>
```

### Fjern sidste migration (hvis ikke applied)

```bash
dotnet ef migrations remove
```

### Opdater til specifik migration

```bash
dotnet ef database update <MigrationName>
```

### Generer SQL script

```bash
dotnet ef migrations script
```

### Drop database

```bash
dotnet ef database drop
```

## Troubleshooting

### Problem: "No database provider has been configured"

**Løsning**: Sørg for at connection string er korrekt sat i `appsettings.json`

### Problem: "A network-related or instance-specific error"

**Løsning**: 
1. Tjek at SQL Server kører
2. Verificer connection string
3. Tjek firewall indstillinger

### Problem: "Login failed for user"

**Løsning**:
1. Verificer brugernavn og password
2. Tjek at SQL Server er sat til Mixed Mode authentication
3. For Docker: brug det password du satte i docker-compose.yml

### Problem: "Build failed" ved migration

**Løsning**:
```bash
# Byg projektet først
dotnet build

# Prøv derefter migration igen
dotnet ef migrations add InitialCreate
```

## Seed Data

Initial seed data inkluderer:
- 1 Admin bruger

Du kan tilføje mere seed data i `ApplicationDbContext.cs` under `OnModelCreating` metoden.

## Azure SQL Database

For Azure deployment, ændre connection string til:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=SlottetDb;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
```

## MySQL Support (Alternativ)

Hvis du vil bruge MySQL i stedet:

### 1. Tilføj MySQL pakke

```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### 2. Opdater Program.cs

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
```

### 3. MySQL Connection String

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=slottetdb;User=root;Password=yourpassword;"
}
```
