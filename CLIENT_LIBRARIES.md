# Client-Side Libraries Setup

Dette projekt bruger **Bootstrap 5.3.2** og **jQuery 3.7.1** til frontend.

---

## ✅ Installer LibMan CLI (Én Gang)

```powershell
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
```

---

## 📦 Download Libraries (Bootstrap + jQuery)

```powershell
cd c:\xampp\htdocs\Slottet\Slottet
libman restore
```

Dette downloader:
- ✅ Bootstrap 5.3.2 (CSS + JS)
- ✅ jQuery 3.7.1

**Output:**
```
Restoring library bootstrap@5.3.2...
Downloading file https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css...
2 libraries restored in 0,49 seconds
```

---

## 📂 Filstruktur Efter Installation

```
wwwroot/lib/
├── bootstrap/
│   └── dist/
│       ├── css/
│       │   ├── bootstrap.min.css
│       │   └── bootstrap.min.css.map
│       └── js/
│           ├── bootstrap.bundle.min.js
│           └── bootstrap.bundle.min.js.map
└── jquery/
    └── dist/
        ├── jquery.min.js
        └── jquery.min.map
```

---

## 🔧 libman.json Konfiguration

```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": [
    {
      "library": "bootstrap@5.3.2",
      "destination": "wwwroot/lib/bootstrap/dist/",
      "files": [
        "css/bootstrap.min.css",
        "css/bootstrap.min.css.map",
        "js/bootstrap.bundle.min.js",
        "js/bootstrap.bundle.min.js.map"
      ]
    },
    {
      "library": "jquery@3.7.1",
      "destination": "wwwroot/lib/jquery/dist/",
      "files": [
        "jquery.min.js",
        "jquery.min.map"
      ]
    }
  ]
}
```

---

## ⚠️ Troubleshooting

### LibMan Command Not Found?
```powershell
# Installer LibMan CLI
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Genstart terminal
```

### Library Not Found Error?
Sørg for at bruge **korrekte filstier** fra CDNJS:
- Bootstrap har `css/` og `js/` mapper (IKKE `dist/css/`)  
- jQuery har filer direkte i roden (IKKE `dist/jquery.min.js`)

### Validation Libraries?
jQuery Validation er **ikke inkluderet** i libman.json da versionerne på CDNJS er inkompatible.

**Alternativ**: Brug CDN links direkte i `_Layout.cshtml`:
```html
<script src="https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/jquery-validation-unobtrusive@4.0.0/dist/jquery.validate.unobtrusive.min.js"></script>
```

---

## 🌐 Alternativ: Brug CDN (Ingen Download)

Rediger `Views/Shared/_Layout.cshtml` til at bruge CDN:

```html
<head>
    <!-- Bootstrap CSS fra CDN -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body>
    <!-- jQuery fra CDN -->
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.7.1/dist/jquery.min.js"></script>
    
    <!-- Bootstrap JS fra CDN -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
</body>
```

**Fordele:**
- ✅ Ingen download nødvendig
- ✅ Caching på tværs af websites
- ✅ Nemt at opdatere versioner

**Ulemper:**
- ❌ Kræver internet forbindelse
- ❌ Kan ikke køre offline

---

## ✅ Verificer Installation

Start applikationen:
```powershell
dotnet run
```

Åbn browser DevTools (F12) → Network tab.

Du skulle **IKKE** se 404 fejl for:
- `/lib/bootstrap/dist/css/bootstrap.min.css`
- `/lib/bootstrap/dist/js/bootstrap.bundle.min.js`
- `/lib/jquery/dist/jquery.min.js`

---

## 📚 Libraries Information

| Library | Version | Purpose |
|---------|---------|---------|
| **Bootstrap** | 5.3.2 | UI framework (navigation, forms, buttons) |
| **jQuery** | 3.7.1 | DOM manipulation, AJAX |

---

**🎉 Bootstrap og jQuery er nu installeret!**

Dette projekt bruger LibMan (Library Manager) til at håndtere client-side libraries som Bootstrap og jQuery.

## 🔧 Installation af Libraries

### Option 1: Via Visual Studio
1. Open Solution i Visual Studio
2. Højreklik på projektet → "Manage Client-Side Libraries"
3. Klik "Restore" for at downloade alle libraries

### Option 2: Via CLI

```bash
# Install LibMan tool (kun én gang)
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Restore libraries
cd Slottet
libman restore
```

### Option 3: Manuel Download

Hvis LibMan ikke virker, kan du manuelt downloade:

1. **Bootstrap 5.3.2**
   - Download fra: https://getbootstrap.com/
   - Placer i: `wwwroot/lib/bootstrap/`

2. **jQuery 3.7.1**
   - Download fra: https://jquery.com/download/
   - Placer i: `wwwroot/lib/jquery/`

3. **jQuery Validation**
   - Download fra: https://github.com/jquery-validation/jquery-validation
   - Placer i: `wwwroot/lib/jquery-validation/`

4. **jQuery Validation Unobtrusive**
   - Download fra: https://github.com/aspnet/jquery-validation-unobtrusive
   - Placer i: `wwwroot/lib/jquery-validation-unobtrusive/`

## 📦 Inkluderede Libraries

- **Bootstrap 5.3.2** - CSS Framework
- **jQuery 3.7.1** - JavaScript library
- **jQuery Validation** - Client-side validation
- **jQuery Validation Unobtrusive** - ASP.NET Core integration

## 🎨 Alternative: CDN

Du kan også bruge CDN ved at opdatere `_Layout.cshtml`:

```cshtml
<!-- Bootstrap CSS -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">

<!-- jQuery -->
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>

<!-- Bootstrap JS -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
```

## ✅ Verificer Installation

Efter restore, tjek at følgende filer findes:

```
wwwroot/lib/
├── bootstrap/
│   └── dist/
│       ├── css/bootstrap.min.css
│       └── js/bootstrap.bundle.min.js
├── jquery/
│   └── dist/jquery.min.js
├── jquery-validation/
│   └── dist/jquery.validate.min.js
└── jquery-validation-unobtrusive/
    └── jquery.validate.unobtrusive.min.js
```

## 🚫 Troubleshooting

Hvis LibMan ikke virker:
1. Brug CDN links i stedet
2. Eller download manuelt og placer i wwwroot/lib/
3. Check at alle stier i `_Layout.cshtml` matcher dine filer
