# ⚡ Quick Start - Slottet

Du har lige klonet projektet. Her er hvad du skal gøre:

---

## 📋 Forudsætninger

✅ .NET 9.0 SDK - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)  
✅ XAMPP (MySQL) - [Download](https://www.apachefriends.org/)

---

## 🚀 3 Steps til at Køre

### 1️⃣ Start MySQL

Åbn **XAMPP Control Panel** → Klik "Start" ved MySQL

### 2️⃣ Opret Database

```powershell
cd Slottet
dotnet restore
dotnet ef database update
```

Hvis du ikke har EF tools:
```powershell
dotnet tool install --global dotnet-ef
```

### 3️⃣ Kør App

```powershell
dotnet run
```

→ Åbn http://localhost:5000

---

## ✅ Det Virker Hvis...

Du ser dette i terminalen:
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

I browseren ser du:
- ✅ Bootstrap styling
- ✅ Navigation menu
- ✅ Ingen 404 fejl i DevTools (F12)

---

## 🆘 Fejl?

### Connection String Problem?

Rediger `Slottet/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=SlottetDb;User=root;Password=DitPassword;SslMode=None;"
  }
}
```

Standard XAMPP bruger `User=root` og `Password=` (tomt).

### Port 3306 Optaget?

XAMPP MySQL kører ikke. Start den i XAMPP Control Panel.

### Build Fejl?

```powershell
dotnet clean
dotnet restore
dotnet build
```

---

## 📖 Mere Info

- **Full dokumentation**: [README.md](README.md)
- **Implementer features**: [IMPLEMENTER_SELV.md](IMPLEMENTER_SELV.md)
- **Kode eksempler**: [EXAMPLES.md](EXAMPLES.md)

---

**🎉 Du er i gang! God arbejdslyst!**
