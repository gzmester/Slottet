# ☁️ Cloud-Ready & Distribueret Arkitektur

Dette dokument beskriver hvordan Slottet er designet som et **distribueret system** der kan køre i forskellige deployment-scenarier.

---

## 🎯 Deployment Scenarier

Systemet understøtter **tre deployment-modeller** uden kode-ændringer:

### 1. 🖥️ Standalone Installation (Én PC)

```
┌──────────────────────────────────┐
│     Windows PC / Mac             │
│  ┌────────────────────────────┐  │
│  │   ASP.NET Core App         │  │
│  │   (Kestrel Server)         │  │
│  └────────────┬───────────────┘  │
│               │                  │
│  ┌────────────▼───────────────┐  │
│  │   MySQL Database           │  │
│  │   (localhost:3306)         │  │
│  └────────────────────────────┘  │
└──────────────────────────────────┘

✅ Perfekt til: små steder, demo, development
✅ Setup: XAMPP + dotnet run
✅ Ingen netværkskonfiguration nødvendig
```

**Connection String:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=SlottetDb;User=root;Password=;"
  }
}
```

---

### 2. 🏢 Client/Server Løsning (Lokalt Netværk)

```
┌─────────────────────────────────────────────┐
│           Lokalt Netværk (LAN)              │
│                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Client 1 │  │ Client 2 │  │ Client 3 │  │
│  │ Browser  │  │ Browser  │  │ Browser  │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  │
│       │             │             │         │
│       └─────────────┼─────────────┘         │
│                     │                       │
│            ┌────────▼────────┐              │
│            │  Server PC       │              │
│            │  192.168.1.100  │              │
│            │                 │              │
│            │  ASP.NET Core   │              │
│            │  (Port 5000)    │              │
│            └────────┬────────┘              │
│                     │                       │
│            ┌────────▼────────┐              │
│            │  MySQL Server   │              │
│            │  (Port 3306)    │              │
│            └─────────────────┘              │
└─────────────────────────────────────────────┘

✅ Perfekt til: Mellemstore institutioner, flere arbejdsstationer
✅ Setup: En server + flere klienter på LAN
✅ Delt database, central data
```

**appsettings.json på server:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=SlottetDb;User=slottet;Password=SecurePass123!"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
```

**Klienter browser:**
```
http://192.168.1.100:5000
```

---

### 3. ☁️ Cloud-Baseret Løsning (Azure/AWS)

```
                    ┌──────────────┐
                    │   Internet   │
                    └───────┬──────┘
                            │
                    ┌───────▼──────┐
                    │ Load Balancer│
                    │ (Azure LB)   │
                    └───┬──────┬───┘
                        │      │
            ┌───────────┘      └──────────┐
            │                             │
    ┌───────▼────────┐          ┌────────▼───────┐
    │  App Instance 1│          │ App Instance 2 │
    │  (Container)   │          │  (Container)   │
    └───────┬────────┘          └────────┬───────┘
            │                            │
            └────────────┬───────────────┘
                         │
              ┌──────────▼───────────┐
              │  Azure Database      │
              │  for MySQL           │
              │  (Managed Service)   │
              └──────────────────────┘
                         │
              ┌──────────▼───────────┐
              │  Azure Key Vault     │
              │  (Secrets Storage)   │
              └──────────────────────┘

✅ Perfekt til: Store organisationer, multi-site, høj tilgængelighed
✅ Setup: Azure App Service + Managed MySQL
✅ Auto-scaling, backup, disaster recovery
```

**Azure Configuration (Miljøvariabler):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<FROM_KEYVAULT>
ApplicationInsights__ConnectionString=<FROM_KEYVAULT>
CORS__AllowedOrigins=https://slottet.dk,https://app.slottet.dk
```

---

## 🔧 Cloud-Ready Principper

### ✅ 1. Stateless Design

**ALDRIG gemme state på serveren:**
```csharp
// ❌ DÅRLIGT - Bruger server-side session
HttpContext.Session.SetString("UserId", user.Id);

// ✅ GODT - Brug JWT tokens eller cookies
var claims = new[] { new Claim("UserId", user.Id) };
var token = GenerateJwtToken(claims);
```

**Hvorfor?** Med load balancing kan næste request gå til en anden server-instans.

---

### ✅ 2. Miljøbaseret Konfiguration

**ALDRIG hardcode værdier:**
```csharp
// ❌ DÅRLIGT
var apiKey = "abc123xyz";
var connectionString = "Server=localhost;...";

// ✅ GODT
var apiKey = Configuration["ExternalApi:ApiKey"]; // Fra miljøvariabel
var connectionString = Configuration.GetConnectionString("DefaultConnection");
```

**Miljøvariabler i forskellige miljøer:**

| Miljø | Configuration Source |
|-------|---------------------|
| Development | appsettings.Development.json |
| Staging | Azure App Configuration |
| Production | Azure Key Vault + Environment Variables |

---

### ✅ 3. Health Checks

**Implementer health endpoints:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddMySql(Configuration.GetConnectionString("DefaultConnection"));

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready"); // Readiness probe
app.MapHealthChecks("/health/live");  // Liveness probe
```

**Kubernetes kan bruge disse til:**
- Auto-restart ved fejl
- Load balancer routing
- Rolling updates

---

### ✅ 4. Horisontal Skalering

**Design for multiple instanser:**

```yaml
# docker-compose.yml - Flere app instanser
services:
  slottet-app-1:
    image: slottet:latest
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
    deploy:
      replicas: 3  # 3 instances

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=${DB_ROOT_PASSWORD}
```

**Hvad skal du tænke på:**
- 🔄 Database connections pool (ikke hold connections åbne)
- 📦 Ingen local file storage (brug Blob Storage)
- 🔔 Distributed caching (Redis, ikke in-memory cache)
- 📨 Message queues for async tasks (Azure Service Bus)

---

### ✅ 5. Secrets Management

**ALDRIG commit passwords eller API keys!**

**Development:**
```bash
# User Secrets (development only)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..."
```

**Production (Azure):**
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Docker Secrets:**
```yaml
# docker-compose.yml
services:
  slottet-app:
    secrets:
      - db_password
      
secrets:
  db_password:
    file: ./secrets/db_password.txt
```

---

### ✅ 6. Distributed Logging

**Struktureret logging til centralt system:**

```csharp
// Installer Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// I din controller
_logger.LogInformation(
    "Resident {ResidentId} medication logged by {UserId}", 
    residentId, 
    userId);

_logger.LogError(
    exception, 
    "Failed to save medication log for {ResidentId}", 
    residentId);
```

**Log aggregation:**
- 📊 Azure Application Insights
- 📈 ELK Stack (Elasticsearch, Logstash, Kibana)
- 🔍 Splunk

---

### ✅ 7. CORS Configuration

**Tillad kun specifikke origins:** 
## Fx specifikke IP addresser der kan tilgå applikationen

```csharp
// Program.cs
var allowedOrigins = Configuration
    .GetSection("CORS:AllowedOrigins")
    .Get<string[]>() ?? new[] { "*" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CloudPolicy", policy =>
    {
        if (allowedOrigins[0] == "*")
        {
            policy.AllowAnyOrigin(); // Kun development!
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});
```

**appsettings.json:**
```json
{
  "CORS": {
    "AllowedOrigins": [
      "https://slottet.dk",
      "https://app.slottet.dk"
    ]
  }
}
```

---

## 🐳 Container-Based Deployment

### Dockerfile Best Practices

```dockerfile
# Multi-stage build for smaller image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Slottet/Slottet.csproj", "Slottet/"]
RUN dotnet restore "Slottet/Slottet.csproj"
COPY . .
WORKDIR "/src/Slottet"
RUN dotnet build "Slottet.csproj" -c Release -o /app/build
RUN dotnet publish "Slottet.csproj" -c Release -o /app/publish

# Runtime image (smaller)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .

# Non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Slottet.dll"]
```

### Kubernetes Deployment

```yaml
# kubernetes/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: slottet-app
spec:
  replicas: 3  # Horisontal skalering
  selector:
    matchLabels:
      app: slottet
  template:
    metadata:
      labels:
        app: slottet
    spec:
      containers:
      - name: slottet
        image: slottet:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connectionstring
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: slottet-service
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 80
  selector:
    app: slottet
```

---

## 📊 Monitoring & Observability

### Application Insights Integration

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = Configuration["ApplicationInsights:ConnectionString"];
});

// Custom metrics
public class ResidentService : IResidentService
{
    private readonly TelemetryClient _telemetry;
    
    public async Task<Resident> CreateResidentAsync(Resident resident)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // ... save resident
            _telemetry.TrackEvent("ResidentCreated", new Dictionary<string, string>
            {
                ["ResidentId"] = resident.Id.ToString(),
                ["Room"] = resident.RoomNumber
            });
            return resident;
        }
        finally
        {
            sw.Stop();
            _telemetry.TrackMetric("ResidentCreation.Duration", sw.ElapsedMilliseconds);
        }
    }
}
```

### Structured Logging

```csharp
// Installer Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.ApplicationInsights(
            context.Configuration["ApplicationInsights:ConnectionString"],
            TelemetryConverter.Traces);
});
```

---

## 🔐 Security Best Practices

### 1. Always Use HTTPS in Production
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

### 2. Implement Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### 3. SQL Injection Protection
✅ EF Core bruger parameterized queries automatisk  
✅ Brug aldrig string concatenation i queries  
✅ Valider altid user input

---

## 🚀 CI/CD Pipeline Example

```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Build Docker image
      run: docker build -t slottet:${{ github.sha }} .
    
    - name: Push to ACR
      run: |
        docker tag slottet:${{ github.sha }} slottetacr.azurecr.io/slottet:latest
        docker push slottetacr.azurecr.io/slottet:latest
    
    - name: Deploy to Azure App Service
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'slottet-prod'
        images: 'slottetacr.azurecr.io/slottet:latest'
```

---

## 📋 Deployment Checklist

### før Production:
- [ ] Environment variables konfigureret
- [ ] Secrets i Key Vault
- [ ] HTTPS certificat installeret
- [ ] Database backup konfigureret
- [ ] Health checks implementeret
- [ ] Logging til Application Insights
- [ ] CORS origins sat korrekt
- [ ] Rate limiting aktiveret
- [ ] Error handling implementeret
- [ ] Load testing gennemført

---

## 🎓 Lær Mere

- [Azure App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)
- [Kubernetes Documentation](https://kubernetes.io/docs/home/)
- [ASP.NET Core Deployment](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

---

**Husk**: Design for distribution fra dag 1 - det er meget sværere at refaktorere senere! 🚀
