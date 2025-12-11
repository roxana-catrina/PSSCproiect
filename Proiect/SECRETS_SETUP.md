# 🔐 Configurare Secrete pentru Dezvoltare Locală

## ⚠️ IMPORTANT: Securitatea Secretelor

Fișierele `appsettings.Local.json` conțin **secrete** (connection strings) și **NU sunt commituite în Git**.

## 📝 Setup pentru Dezvoltare Locală

### 1. Creează fișierele locale cu secretele tale

**Pentru Proiect (API):**
Creează `Proiect/Proiect/appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PSSCProiectDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true",
    "ServiceBus": "Endpoint=sb://psscproiect.servicebus.windows.net/;SharedAccessKeyName=ProiectPSSCPolicy;SharedAccessKey=YOUR_ACTUAL_KEY_HERE"
  }
}
```

**Pentru EventProcessor:**
Creează `Proiect/Proiect.EventProcessor/appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://psscproiect.servicebus.windows.net/;SharedAccessKeyName=ProiectPSSCPolicy;SharedAccessKey=YOUR_ACTUAL_KEY_HERE",
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PSSCProiectDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### 2. Înlocuiește `YOUR_ACTUAL_KEY_HERE`

Găsește connection string-ul real în:
- Azure Portal → Service Bus Namespace → Shared access policies → RootManageSharedAccessKey
- SAU în fișierul `azure-config.local.txt` (dacă îl ai salvat local)

### 3. Verifică că fișierele sunt în `.gitignore`

Fișierul `.gitignore` ar trebui să conțină:
```
appsettings.Local.json
appsettings.Development.json
azure-config.local.txt
```

## 🔄 Cum Funcționează

1. **appsettings.json** - Commituit în Git, conține placeholders
2. **appsettings.Local.json** - NU e commituit, conține secrete reale
3. **Program.cs** - Încarcă ambele fișiere, Local suprascrie valorile din json

## 🚀 Rularea Aplicației

După ce ai creat fișierele locale cu secretele reale:

```bash
# API
cd Proiect/Proiect
dotnet run

# EventProcessor
cd Proiect/Proiect.EventProcessor
dotnet run
```

## 👥 Pentru Alți Dezvoltatori din Echipă

Când clonează repository-ul, trebuie să:
1. Creeze propriile fișiere `appsettings.Local.json`
2. Obțină connection string-urile din Azure Portal sau de la tine
3. Ruleze aplicația

## 🛡️ NICIODATĂ să nu commitezi:
- ❌ Connection strings
- ❌ API keys
- ❌ Passwords
- ❌ Fișiere `*.Local.json`
- ❌ Fișiere `*secret*`

