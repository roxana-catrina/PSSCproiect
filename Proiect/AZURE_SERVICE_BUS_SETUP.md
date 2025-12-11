# Instrucțiuni pentru Provisionare Azure Service Bus

Acest document conține comenzile Azure CLI necesare pentru a crea resursele Azure Service Bus pentru proiect.

## Prerequisite

1. Instalează Azure CLI de la: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli
2. Autentifică-te: `az login`

## Pasul 1: Creează Resource Group (dacă nu există deja)

```bash
az group create --name PSSCProiectResourceGroup --location eastus
```

## Pasul 2: Creează Service Bus Namespace

```bash
az servicebus namespace create ^
  --resource-group PSSCProiectResourceGroup ^
  --name pssc-proiect-servicebus ^
  --sku Standard ^
  --location eastus
```

**Notă**: Numele namespace-ului trebuie să fie unic global. Dacă `pssc-proiect-servicebus` este deja folosit, încearcă un alt nume (ex: `pssc-proiect-sb-yourinitials`).

## Pasul 3: Creează Cozile (Queues)

```bash
# Coadă pentru comenzi de plasare ordine
az servicebus queue create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name orders-commands

# Coadă pentru comenzi de facturare (opțional - momentan nu e folosită)
az servicebus queue create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name invoices-commands

# Coadă pentru comenzi de shipping (opțional - momentan nu e folosită)
az servicebus queue create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name shipping-commands
```

## Pasul 4: Creează Topic-urile (Topics)

```bash
# Topic pentru evenimente de ordine
az servicebus topic create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name order-events

# Topic pentru evenimente de facturare
az servicebus topic create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name invoice-events

# Topic pentru evenimente de pachete
az servicebus topic create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name package-events
```

## Pasul 5: Creează Subscriptions pentru Topic-uri

```bash
# Subscriptions pentru order-events topic
az servicebus topic subscription create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --topic-name order-events ^
  --name billing-subscription

az servicebus topic subscription create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --topic-name order-events ^
  --name shipping-subscription

# Subscriptions pentru invoice-events (opțional - pentru viitoare extensii)
az servicebus topic subscription create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --topic-name invoice-events ^
  --name accounting-subscription

# Subscriptions pentru package-events (opțional - pentru viitoare extensii)
az servicebus topic subscription create ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --topic-name package-events ^
  --name tracking-subscription
```

## Pasul 6: Obține Connection String

```bash
az servicebus namespace authorization-rule keys list ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name RootManageSharedAccessKey ^
  --query primaryConnectionString ^
  --output tsv
```

**IMPORTANT**: Copiază connection string-ul returnat și actualizează fișierul `appsettings.json`:

```json
"ConnectionStrings": {
  "ServiceBus": "COPIAZA_CONNECTION_STRING_UL_AICI"
}
```

## Verificare

Pentru a verifica că resursele au fost create corect:

```bash
# Listează cozile
az servicebus queue list ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --output table

# Listează topic-urile
az servicebus topic list ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --output table

# Listează subscriptions pentru un topic
az servicebus topic subscription list ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --topic-name order-events ^
  --output table
```

## Configurare pentru Producție (Recomandări)

### 1. Configurare Dead Letter Queue (DLQ)
Dead letter queue este activat automat pentru fiecare coadă/subscription. Mesajele care nu pot fi procesate după un număr de retry-uri vor fi mutate în DLQ.

### 2. Configurare Time-To-Live (TTL)
```bash
# Setează TTL pentru o coadă (ex: 14 zile = 1209600 secunde)
az servicebus queue update ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name orders-commands ^
  --default-message-time-to-live P14D
```

### 3. Configurare Max Delivery Count
```bash
# Setează numărul maxim de încercări înainte de a muta în DLQ
az servicebus queue update ^
  --resource-group PSSCProiectResourceGroup ^
  --namespace-name pssc-proiect-servicebus ^
  --name orders-commands ^
  --max-delivery-count 10
```

### 4. Managed Identity (Recomandată pentru Producție)
În loc de connection string, folosește Managed Identity pentru securitate sporită:

```bash
# Activează System Assigned Managed Identity pentru aplicație (în Azure App Service)
az webapp identity assign ^
  --name your-app-name ^
  --resource-group PSSCProiectResourceGroup

# Obține Object ID-ul
$objectId = az webapp identity show ^
  --name your-app-name ^
  --resource-group PSSCProiectResourceGroup ^
  --query principalId ^
  --output tsv

# Acordă permisiuni la Service Bus
az role assignment create ^
  --role "Azure Service Bus Data Owner" ^
  --assignee $objectId ^
  --scope /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/PSSCProiectResourceGroup/providers/Microsoft.ServiceBus/namespaces/pssc-proiect-servicebus
```

## Ștergere Resurse (Cleanup)

Dacă vrei să ștergi toate resursele create:

```bash
# Șterge tot resource group-ul (ATENȚIE: șterge TOATE resursele din grup!)
az group delete --name PSSCProiectResourceGroup --yes --no-wait
```

## Rezumat Arhitectură

### Cozi (Point-to-Point):
- `orders-commands` → procesată de `OrderCommandProcessor`

### Topic-uri și Subscriptions (Pub/Sub):
- `order-events`:
  - `billing-subscription` → procesată de `BillingEventSubscriber`
  - `shipping-subscription` → procesată de `ShippingEventSubscriber`
- `invoice-events`:
  - `accounting-subscription` (pentru extensii viitoare)
- `package-events`:
  - `tracking-subscription` (pentru extensii viitoare)

### Flux de Date:
1. **API** → `orders-commands` queue
2. **OrderCommandProcessor** consumă → procesează → publică pe `order-events` topic
3. **BillingEventSubscriber** consumă `order-events` → generează factură → publică pe `invoice-events`
4. **ShippingEventSubscriber** consumă `order-events` → procesează shipping → publică pe `package-events`

