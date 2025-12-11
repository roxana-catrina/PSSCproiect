# Script pentru configurarea cozilor și topic-urilor în namespace-ul existent
# Rulează cu: powershell -ExecutionPolicy Bypass -File setup-azure-existing.ps1

Write-Host "=== Setup Azure Service Bus - Folosind resurse existente ===" -ForegroundColor Cyan

# Verifică dacă Azure CLI este instalat
try {
    $azVersion = az --version 2>&1
    Write-Host "✓ Azure CLI este deja instalat" -ForegroundColor Green
} catch {
    Write-Host "× Azure CLI nu este instalat" -ForegroundColor Red
    exit 1
}

# Login la Azure
Write-Host "`n=== Step 1: Login la Azure ===" -ForegroundColor Cyan
Write-Host "Se va deschide browser-ul pentru autentificare..." -ForegroundColor Yellow
az login

if ($LASTEXITCODE -ne 0) {
    Write-Host "× Eroare la login" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Login reușit" -ForegroundColor Green

# Setează variabilele pentru resursele existente
$resourceGroup = "PSSCproiect"
$namespace = "PSSCproiect"

Write-Host "`n=== Step 2: Verificare resurse existente ===" -ForegroundColor Cyan

# Verifică resource group
Write-Host "Verificare Resource Group: $resourceGroup" -ForegroundColor White
$rgInfo = az group show --name $resourceGroup --query "{Name:name,Location:location}" -o json 2>$null | ConvertFrom-Json

if (-not $rgInfo) {
    Write-Host "× Resource Group '$resourceGroup' nu există!" -ForegroundColor Red
    Write-Host "  Creează-l din Azure Portal sau modifică numele în script." -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Resource Group găsit în locația: $($rgInfo.Location)" -ForegroundColor Green

# Verifică namespace
Write-Host "Verificare Service Bus Namespace: $namespace" -ForegroundColor White
$nsInfo = az servicebus namespace show --resource-group $resourceGroup --name $namespace --query "{Name:name,Location:location,Sku:sku.name}" -o json 2>$null | ConvertFrom-Json

if (-not $nsInfo) {
    Write-Host "× Service Bus Namespace '$namespace' nu există!" -ForegroundColor Red
    Write-Host "  Creează-l din Azure Portal sau modifică numele în script." -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Service Bus Namespace găsit" -ForegroundColor Green
Write-Host "  Location: $($nsInfo.Location)" -ForegroundColor White
Write-Host "  SKU: $($nsInfo.Sku)" -ForegroundColor White

if ($nsInfo.Sku -ne "Standard" -and $nsInfo.Sku -ne "Premium") {
    Write-Host "⚠ Atenție: SKU-ul este '$($nsInfo.Sku)'. Pentru Topics ai nevoie de SKU Standard sau Premium!" -ForegroundColor Yellow
    Write-Host "  Schimbă SKU-ul din Azure Portal în Standard." -ForegroundColor Yellow
    exit 1
}

# Creează Queue pentru comenzi
Write-Host "`n=== Step 3: Creare Queue pentru comenzi ===" -ForegroundColor Cyan
Write-Host "Creez queue: order-commands..." -ForegroundColor White

$existingQueue = az servicebus queue show --resource-group $resourceGroup --namespace-name $namespace --name order-commands 2>$null

if ($existingQueue) {
    Write-Host "✓ Queue 'order-commands' există deja" -ForegroundColor Green
} else {
    az servicebus queue create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --name order-commands
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Queue 'order-commands' creat" -ForegroundColor Green
    } else {
        Write-Host "× Eroare la crearea queue-ului" -ForegroundColor Red
    }
}

# Creează Topics și Subscriptions
Write-Host "`n=== Step 4: Creare Topics și Subscriptions ===" -ForegroundColor Cyan

# Topic 1: order-events
Write-Host "Creez topic: order-events..." -ForegroundColor White
$existingTopic = az servicebus topic show --resource-group $resourceGroup --namespace-name $namespace --name order-events 2>$null

if ($existingTopic) {
    Write-Host "✓ Topic 'order-events' există deja" -ForegroundColor Green
} else {
    az servicebus topic create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --name order-events
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Topic 'order-events' creat" -ForegroundColor Green
    }
}

# Subscription pentru order-events
$existingSub = az servicebus topic subscription show --resource-group $resourceGroup --namespace-name $namespace --topic-name order-events --name shipping-subscription 2>$null

if ($existingSub) {
    Write-Host "✓ Subscription 'shipping-subscription' există deja" -ForegroundColor Green
} else {
    az servicebus topic subscription create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --topic-name order-events `
        --name shipping-subscription
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Subscription 'shipping-subscription' creată" -ForegroundColor Green
    }
}

# Topic 2: package-events
Write-Host "`nCreez topic: package-events..." -ForegroundColor White
$existingTopic = az servicebus topic show --resource-group $resourceGroup --namespace-name $namespace --name package-events 2>$null

if ($existingTopic) {
    Write-Host "✓ Topic 'package-events' există deja" -ForegroundColor Green
} else {
    az servicebus topic create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --name package-events
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Topic 'package-events' creat" -ForegroundColor Green
    }
}

# Subscription pentru package-events
$existingSub = az servicebus topic subscription show --resource-group $resourceGroup --namespace-name $namespace --topic-name package-events --name billing-subscription 2>$null

if ($existingSub) {
    Write-Host "✓ Subscription 'billing-subscription' există deja" -ForegroundColor Green
} else {
    az servicebus topic subscription create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --topic-name package-events `
        --name billing-subscription
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Subscription 'billing-subscription' creată" -ForegroundColor Green
    }
}

# Topic 3: invoice-events
Write-Host "`nCreez topic: invoice-events..." -ForegroundColor White
$existingTopic = az servicebus topic show --resource-group $resourceGroup --namespace-name $namespace --name invoice-events 2>$null

if ($existingTopic) {
    Write-Host "✓ Topic 'invoice-events' există deja" -ForegroundColor Green
} else {
    az servicebus topic create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --name invoice-events
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Topic 'invoice-events' creat" -ForegroundColor Green
    }
}

# Subscription pentru invoice-events
$existingSub = az servicebus topic subscription show --resource-group $resourceGroup --namespace-name $namespace --topic-name invoice-events --name notification-subscription 2>$null

if ($existingSub) {
    Write-Host "✓ Subscription 'notification-subscription' există deja" -ForegroundColor Green
} else {
    az servicebus topic subscription create `
        --resource-group $resourceGroup `
        --namespace-name $namespace `
        --topic-name invoice-events `
        --name notification-subscription
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Subscription 'notification-subscription' creată" -ForegroundColor Green
    }
}

# Obține Connection String
Write-Host "`n=== Step 5: Obținere Connection String ===" -ForegroundColor Cyan
$connectionString = az servicebus namespace authorization-rule keys list `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --name RootManageSharedAccessKey `
    --query primaryConnectionString `
    --output tsv

if ($connectionString) {
    Write-Host "✓ Connection String obținut" -ForegroundColor Green
} else {
    Write-Host "× Eroare la obținerea Connection String" -ForegroundColor Red
    exit 1
}

# Actualizează appsettings.json
Write-Host "`n=== Step 6: Actualizare appsettings.json ===" -ForegroundColor Cyan

$appsettingsDevPath = ".\Proiect\appsettings.Development.json"

if (Test-Path $appsettingsDevPath) {
    $appsettings = Get-Content $appsettingsDevPath -Raw | ConvertFrom-Json
    
    if (-not $appsettings.ConnectionStrings) {
        $appsettings | Add-Member -NotePropertyName "ConnectionStrings" -NotePropertyValue @{} -Force
    }
    
    $appsettings.ConnectionStrings | Add-Member -NotePropertyName "ServiceBus" -NotePropertyValue $connectionString -Force
    
    if (-not $appsettings.ServiceBus) {
        $appsettings | Add-Member -NotePropertyName "ServiceBus" -NotePropertyValue @{} -Force
    }
    
    $appsettings.ServiceBus | Add-Member -NotePropertyName "Queues" -NotePropertyValue @{
        OrderCommands = "order-commands"
    } -Force
    
    $appsettings.ServiceBus | Add-Member -NotePropertyName "Topics" -NotePropertyValue @{
        OrderEvents = "order-events"
        PackageEvents = "package-events"
        InvoiceEvents = "invoice-events"
    } -Force
    
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsDevPath
    
    Write-Host "✓ appsettings.Development.json actualizat" -ForegroundColor Green
} else {
    Write-Host "⚠ Fișierul appsettings.Development.json nu a fost găsit" -ForegroundColor Yellow
}

# Salvează informațiile într-un fișier
$infoPath = ".\azure-config.txt"
@"
=== Configurare Azure Service Bus ===
Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Resource Group: $resourceGroup
Namespace: $namespace
Location: $($rgInfo.Location)

Queue:
  - order-commands

Topics și Subscriptions:
  - order-events
    * shipping-subscription
  - package-events
    * billing-subscription
  - invoice-events
    * notification-subscription

Connection String:
$connectionString

Pentru a vedea resursele în Azure Portal:
https://portal.azure.com/#@/resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$resourceGroup/overview
"@ | Out-File $infoPath -Encoding UTF8

Write-Host "`n=== ✓ Setup Complet ===" -ForegroundColor Green
Write-Host "Informațiile au fost salvate în: $infoPath" -ForegroundColor Yellow
Write-Host "`nPoți testa conexiunea rulând aplicația cu: dotnet run --project .\Proiect" -ForegroundColor Cyan
Write-Host "`nResurse create/verificate:" -ForegroundColor Cyan
Write-Host "  • 1 Queue: order-commands" -ForegroundColor White
Write-Host "  • 3 Topics: order-events, package-events, invoice-events" -ForegroundColor White
Write-Host "  • 3 Subscriptions: shipping-subscription, billing-subscription, notification-subscription" -ForegroundColor White

