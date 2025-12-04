# Integrare Bază de Date SQL - Proiect PSSC

## Descriere

Am implementat integrarea completă cu o bază de date SQL Server pentru cele 3 workflow-uri:
1. **OrderProcessingWorkflow** - Procesare comenzi
2. **BillingWorkflow** - Generare facturi
3. **ShippingWorkflow** - Expediere colete

## Structura Implementării

### 1. Baza de Date

Am creat următoarele tabele în baza de date `PSSCProiectDb`:

- **Products** - Stocul de produse disponibile
- **Orders** - Comenzile clienților
- **OrderItems** - Articolele din fiecare comandă
- **Invoices** - Facturile generate
- **Packages** - Coletele expediate

### 2. Servicii de Stare (State Services)

Am creat 3 servicii pentru gestionarea stării în baza de date:

#### OrderStateService
- `LoadOrderAsync()` - Încarcă o comandă din baza de date
- `SaveOrderAsync()` - Salvează o comandă în baza de date
- `CheckProductExistsAsync()` - Verifică existența unui produs
- `CheckStockAvailabilityAsync()` - Verifică stocul disponibil (implementare cu date din DB)
- `UpdateStockAsync()` - Actualizează stocul după plasarea comenzii

#### InvoiceStateService
- `LoadInvoiceAsync()` - Încarcă o factură din baza de date
- `SaveInvoiceAsync()` - Salvează o factură în baza de date

#### PackageStateService
- `LoadPackageAsync()` - Încarcă un colet din baza de date
- `SavePackageAsync()` - Salvează un colet în baza de date

### 3. Serviciu de Orchestrare

**WorkflowOrchestrationService** orchestrează executarea workflow-urilor cu integrarea în DB:

Pentru fiecare workflow:
1. **Înainte de execuție**: Încarcă starea existentă din baza de date
2. **În timpul execuției**: Folosește funcții de verificare care interogează baza de date
3. **După execuție**: Salvează rezultatul în baza de date

## Configurare și Rulare

### Pasul 1: Instalare Pachete

Pachetele Entity Framework Core au fost deja adăugate în `Proiect.csproj`:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
```

Rulați:
```bash
dotnet restore
```

### Pasul 2: Configurare Connection String

Connection string-ul este deja configurat în `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PSSCProiectDb;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

**Notă**: Acesta folosește LocalDB. Dacă aveți SQL Server instalat, puteți modifica connection string-ul.

### Pasul 3: Creare Bază de Date

#### Opțiunea A: Creare Automată (Recomandată)
Baza de date se va crea automat la prima rulare a aplicației datorită codului din `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}
```

#### Opțiunea B: Creare Manuală cu Script SQL
Dacă preferați să creați manual baza de date, folosiți scriptul `Database/CreateDatabase.sql`:
```bash
sqlcmd -S (localdb)\mssqllocaldb -i Database/CreateDatabase.sql
```

### Pasul 4: Rulare Aplicație

```bash
dotnet build
dotnet run
```

Aplicația va porni pe `https://localhost:5001` și va crea automat baza de date cu datele inițiale.

## Testare API-urilor

### 1. Plasare Comandă (Order Processing)

**POST** `/api/orders`

```json
{
  "customerName": "Ion Popescu",
  "customerEmail": "ion.popescu@email.com",
  "deliveryStreet": "Strada Victoriei 123",
  "deliveryCity": "București",
  "deliveryPostalCode": "010101",
  "deliveryCountry": "România",
  "items": [
    {
      "productName": "Laptop",
      "quantity": "2",
      "unitPrice": "999.99"
    },
    {
      "productName": "Mouse",
      "quantity": "3",
      "unitPrice": "29.99"
    }
  ]
}
```

**Funcționalități Implementate**:
- ✅ Verifică existența produselor în baza de date
- ✅ Verifică stocul disponibil în timp real din DB
- ✅ Salvează comanda în baza de date
- ✅ Actualizează stocul produselor după confirmare

### 2. Generare Factură (Billing)

**POST** `/api/invoices`

```json
{
  "orderNumber": "ORD-20241204-A1B2C3D4",
  "customerName": "Ion Popescu",
  "totalAmount": "2089.95"
}
```

**Funcționalități Implementate**:
- ✅ Încarcă comanda din baza de date
- ✅ Generează număr de factură
- ✅ Calculează TVA (19%)
- ✅ Salvează factura în baza de date

### 3. Expediere Colet (Shipping)

**POST** `/api/packages/pickup`

```json
{
  "orderNumber": "ORD-20241204-A1B2C3D4",
  "deliveryStreet": "Strada Victoriei 123",
  "deliveryCity": "București",
  "deliveryPostalCode": "010101",
  "deliveryCountry": "România"
}
```

**Funcționalități Implementate**:
- ✅ Încarcă comanda din baza de date
- ✅ Generează AWB (număr de tracking)
- ✅ Notifică curierul
- ✅ Salvează coletul în baza de date

## Arhitectura Soluției

```
┌─────────────────────────────────────────────────────────────┐
│                      Controllers Layer                       │
│  (OrdersController, InvoicesController, PackagesController) │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              WorkflowOrchestrationService                    │
│  • Încarcă starea din DB înainte de workflow                │
│  • Execută workflow-ul cu verificări în DB                  │
│  • Salvează rezultatul în DB după workflow                  │
└─────────────┬───────────────────────────┬───────────────────┘
              │                           │
              ▼                           ▼
┌─────────────────────────┐   ┌──────────────────────────────┐
│    Domain Workflows     │   │     State Services           │
│  • OrderProcessing      │   │  • OrderStateService         │
│  • Billing              │   │  • InvoiceStateService       │
│  • Shipping             │   │  • PackageStateService       │
└─────────────────────────┘   └──────────┬───────────────────┘
                                         │
                                         ▼
                              ┌──────────────────────────────┐
                              │   ApplicationDbContext       │
                              │   (Entity Framework Core)    │
                              └──────────┬───────────────────┘
                                         │
                                         ▼
                              ┌──────────────────────────────┐
                              │   SQL Server Database        │
                              │   (PSSCProiectDb)            │
                              └──────────────────────────────┘
```

## Caracteristici Implementate

### ✅ Cerința 1: Crearea bazei de date SQL
- Tabele pentru Products, Orders, OrderItems, Invoices, Packages
- Relații între tabele (Foreign Keys)
- Indexuri pentru performanță
- Date inițiale (seed data) pentru produse

### ✅ Cerința 2: Încărcare stare din DB înainte de workflow
- `LoadOrderAsync()` - pentru comenzi
- `LoadInvoiceAsync()` - pentru facturi
- `LoadPackageAsync()` - pentru colete

### ✅ Cerința 3: Verificări folosind informații din DB
- `CheckProductExistsAsync()` - verifică existența produsului în DB
- `CheckStockAvailabilityAsync()` - verifică stocul disponibil în DB
- Date în timp real din baza de date

### ✅ Cerința 4: Salvare rezultat în DB după workflow
- `SaveOrderAsync()` - salvează comenzile
- `SaveInvoiceAsync()` - salvează facturile
- `SavePackageAsync()` - salvează coletele
- `UpdateStockAsync()` - actualizează stocul

## Date Inițiale în Baza de Date

La prima rulare, baza de date este populată cu următoarele produse:

| ID | Name       | StockQuantity | UnitPrice |
|----|------------|---------------|-----------|
| 1  | Laptop     | 10            | 999.99    |
| 2  | Mouse      | 50            | 29.99     |
| 3  | Keyboard   | 30            | 79.99     |
| 4  | Monitor    | 15            | 299.99    |
| 5  | Headphones | 25            | 149.99    |

## Verificare Funcționalitate

Pentru a verifica că totul funcționează corect:

1. **Porniți aplicația**:
   ```bash
   dotnet run
   ```

2. **Accesați Swagger UI**: `https://localhost:5001/swagger`

3. **Testați API-urile în ordine**:
   - Mai întâi plasați o comandă (POST /api/orders)
   - Notați `orderNumber` din răspuns
   - Generați o factură cu acel orderNumber (POST /api/invoices)
   - Expediați coletul cu același orderNumber (POST /api/packages/pickup)

4. **Verificați baza de date**:
   ```sql
   USE PSSCProiectDb;
   SELECT * FROM Orders;
   SELECT * FROM OrderItems;
   SELECT * FROM Invoices;
   SELECT * FROM Packages;
   SELECT * FROM Products; -- Verificați că stocul s-a actualizat
   ```

## Troubleshooting

### Eroare: "Cannot open database"
- Asigurați-vă că SQL Server LocalDB este instalat
- Sau modificați connection string-ul pentru a folosi o instanță SQL Server diferită

### Eroare: "Login failed"
- Verificați că folosiți `Trusted_Connection=true` pentru Windows Authentication
- Sau adăugați `User Id` și `Password` în connection string

### Stocul nu se actualizează
- Verificați că produsele din comandă există în tabela Products cu exact același nume
- Verificați logs-urile pentru erori

## Concluzie

Implementarea este completă și funcțională. Toate cele 3 workflow-uri sunt integrate cu baza de date SQL și îndeplinesc toate cerințele:
- ✅ Bază de date SQL creată cu toate tabelele necesare
- ✅ Încărcare stare din DB înainte de workflow
- ✅ Verificări produse/stoc folosind date din DB
- ✅ Salvare rezultate în DB după workflow

