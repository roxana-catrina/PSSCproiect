# Interfață Web pentru Procesarea Comenzilor

## Descriere

Am creat o interfață web modernă și intuitivă care permite plasarea comenzilor și urmărirea în timp real a întregului flux de procesare prin cele 3 workflow-uri:
1. **Order Processing** - Plasare și validare comandă
2. **Billing** - Generare factură
3. **Shipping** - Expediere colet

## Caracteristici

### 🎨 Interfața Conține:

- **Formular Intuitiv**: Introducere date client și produse
- **Log-uri în Timp Real**: Vizualizare pas cu pas a procesării
- **Afișare Rezultate**: Detalii complete pentru comandă, factură și colet
- **Design Responsiv**: Funcționează pe desktop și mobile
- **Animații**: Feedback vizual pentru fiecare acțiune

### 🔄 Fluxul de Procesare:

Când apăsați butonul "Procesează Comanda", interfața:

1. **Colectează datele** din formular
2. **Trimite cerere** POST către `/api/orders`
3. **Afișează log-uri** pentru fiecare pas:
   - ✅ Comandă plasată → Event "OrderPlaced"
   - ✅ Factură generată → Event "InvoiceGenerated"  
   - ✅ Colet expediat → Event "PackageShipped"
4. **Afișează rezultatele** pentru fiecare workflow

## Cum să Utilizați

### Pasul 1: Pornire Aplicație

```powershell
cd "C:\Users\Ionela\Desktop\Semestrul 1\PSSC\PSSCproiect\Proiect\Proiect"
dotnet run
```

Aplicația va porni pe: `https://localhost:5001`

### Pasul 2: Accesare Interfață

Deschideți browserul la:
```
https://localhost:5001
```

**Notă**: Dacă primiți un avertisment de securitate (certificat self-signed), apăsați "Advanced" și "Proceed to localhost".

### Pasul 3: Completare Formular

Formularul vine pre-completat cu date de test:
- **Client**: Ion Popescu
- **Email**: ion.popescu@email.com
- **Adresă**: Strada Victoriei 123, București
- **Produse**: 
  - Laptop (2 buc x 999.99 RON)
  - Mouse (3 buc x 29.99 RON)

Puteți modifica orice câmp sau adăuga/șterge produse.

### Pasul 4: Procesare Comandă

1. Verificați datele introduse
2. Apăsați butonul **"🚀 Procesează Comanda"**
3. Urmăriți în secțiunea de log-uri procesarea în timp real
4. Vedeți rezultatele pentru fiecare workflow în cardurile din dreapta

## Structura Interfeței

### Partea Stângă - Formular
```
📝 Plasare Comandă Nouă
├── Date Client (Nume, Email)
├── Adresă Livrare (Strada, Oraș, Cod Poștal, Țară)
├── Produse (Nume, Cantitate, Preț)
│   ├── ➕ Adaugă Produs
│   └── ❌ Șterge Produs
└── 🚀 Procesează Comanda
```

### Partea Dreaptă - Log-uri și Rezultate
```
📊 Status Procesare
├── 📋 Log-uri Procesare (în timp real)
├── ✅ Comandă Plasată (detalii complete)
├── 🧾 Factură Generată (cu TVA)
└── 📦 Colet Expediat (cu AWB)
```

## Log-uri Afișate

Interfața afișează log-uri detaliate pentru fiecare pas:

### 1. Plasare Comandă
```
📋 Date comandă colectate din formular
👤 Client: Ion Popescu
📧 Email: ion.popescu@email.com
📍 Livrare: București, România
📦 Număr produse: 2

🚀 PASUL 1: Începe procesarea comenzii...
📤 Trimit cerere către API /api/orders...
✅ Răspuns primit de la server
📝 Comandă plasată cu succes! Număr: ORD-20241211-XXXXXXXX
💰 Total comandă: 2089.95 RON
📊 Status: Confirmed
🔔 Event publicat: OrderPlaced
```

### 2. Generare Factură
```
🧾 PASUL 2: Începe generarea facturii...
⏳ Aștept 2 secunde pentru procesarea event-ului...
📤 Trimit cerere către API /api/invoices...
✅ Răspuns primit de la server
🧾 Factură generată cu succes! Număr: INV-20241211-XXXXXXXX
💰 Total fără TVA: 2089.95 RON
💰 TVA (19%): 397.09 RON
💰 Total cu TVA: 2487.04 RON
🔔 Event publicat: InvoiceGenerated
```

### 3. Expediere Colet
```
📦 PASUL 3: Începe procesul de expediere...
⏳ Aștept 2 secunde pentru procesarea event-ului...
📤 Trimit cerere către API /api/packages/pickup...
✅ Răspuns primit de la server
📦 Colet expediat cu succes! AWB: AWB-20241211-XXXXXXXX
🚚 Status curier: Notified
🔔 Event publicat: PackageShipped

🎉 PROCESARE COMPLETĂ! Toate workflow-urile au fost executate cu succes!
```

## Fișiere Create

Am creat următoarele fișiere în folder-ul `wwwroot`:

1. **index.html** - Structura paginii web
2. **styles.css** - Stilizare modernă cu gradient și animații
3. **app.js** - Logica JavaScript pentru comunicare cu API

## Funcționalități JavaScript

### Funcții Principale:

- `addLog(message, type)` - Adaugă log-uri colorate în timp real
- `processOrder(orderData)` - Plasează comanda
- `generateInvoice(order)` - Generează factura
- `shipPackage(order)` - Expediază coletul
- `displayOrderDetails(order)` - Afișează detalii comandă
- `displayInvoiceDetails(invoice)` - Afișează detalii factură
- `displayPackageDetails(pkg)` - Afișează detalii colet
- `addItem()` - Adaugă produs nou în formular
- `removeItem(button)` - Șterge produs din formular
- `clearLogs()` - Curăță log-urile și rezultatele

### Tipuri de Log-uri:

- **info** (albastru) - Informații generale
- **success** (verde) - Acțiuni reușite
- **warning** (galben) - Avertismente
- **error** (roșu) - Erori

## Design și Culori

Interfața folosește o paletă de culori modernă:
- **Gradient Principal**: Purple-Blue (#667eea → #764ba2)
- **Succes**: Verde (#28a745)
- **Eroare**: Roșu (#dc3545)
- **Avertisment**: Galben (#ffc107)
- **Info**: Albastru (#17a2b8)

## Testare

### Test Rapid:

1. Porniți aplicația: `dotnet run`
2. Accesați: `https://localhost:5001`
3. Apăsați direct **"Procesează Comanda"** (datele sunt pre-completate)
4. Urmăriți log-urile în timp real

### Test Personalizat:

1. Modificați datele clientului
2. Adăugați sau ștergeți produse
3. Procesați comanda
4. Verificați rezultatele în interfață

### Verificare Bază de Date:

După procesare, verificați în baza de date:
```sql
USE PSSCProiectDb;
SELECT * FROM Orders ORDER BY CreatedAt DESC;
SELECT * FROM Invoices ORDER BY IssueDate DESC;
SELECT * FROM Packages ORDER BY PickupDate DESC;
```

## Alternative de Accesare

### 1. Interfața Web (Recomandată)
```
https://localhost:5001
```

### 2. Swagger UI
```
https://localhost:5001/swagger
```

## Diferențe față de Swagger

| Caracteristică | Interfața Web | Swagger |
|----------------|---------------|---------|
| Formular intuitiv | ✅ | ❌ |
| Log-uri în timp real | ✅ | ❌ |
| Flux complet automat | ✅ | ❌ |
| Afișare evenimente | ✅ | ❌ |
| Design modern | ✅ | ⚠️ |
| API Documentation | ❌ | ✅ |

## Troubleshooting

### Eroare: "Cannot connect to API"
- Verificați că aplicația rulează
- Verificați URL-ul: `https://localhost:5001`

### Log-urile nu apar
- Deschideți Console în browser (F12)
- Verificați erori JavaScript

### Pagina nu se încarcă
- Verificați că folder-ul `wwwroot` există
- Verificați că fișierele HTML/CSS/JS sunt în `wwwroot`

### Eroare CORS
- Nu ar trebui să apară (API și interfață pe același server)
- Dacă apare, verificați `Program.cs`

## Funcționalități Viitoare (Opțional)

Puteți extinde interfața cu:
- ✅ Validare formulare în timp real
- ✅ Istoric comenzi plasate
- ✅ Filtrare și căutare comenzi
- ✅ Export rezultate (PDF/JSON)
- ✅ Notificări desktop
- ✅ WebSocket pentru evenimente în timp real
- ✅ Dark mode

## Concluzie

Aveți acum o interfață web completă și funcțională care:
- ✅ Înlocuiește Swagger pentru utilizare practică
- ✅ Afișează întreg fluxul de procesare
- ✅ Oferă feedback vizual în timp real
- ✅ Are un design modern și profesional
- ✅ Este ușor de folosit

**Bucurați-vă de noua interfață! 🎉**

