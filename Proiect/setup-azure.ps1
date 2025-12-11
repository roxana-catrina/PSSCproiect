# Script pentru configurarea Azure Service Bus pentru proiect
# Rulează cu: powershell -ExecutionPolicy Bypass -File setup-azure.ps1
# Sau cu o locație specifică: powershell -ExecutionPolicy Bypass -File setup-azure.ps1 -Location "italynorth"

param(
    [string]$Location = "westeurope"
)

Write-Host "=== Setup Azure Service Bus pentru Proiect ===" -ForegroundColor Cyan
Write-Host "Locație dorită: $Location" -ForegroundColor Yellow
Write-Host "(Alte regiuni disponibile: italynorth, germanywestcentral, francecentral, uksouth, swedencentral, northeurope)" -ForegroundColor Yellow

# Verifică dacă Azure CLI este instalat
try {
    $azVersion = az --version 2>&1
    Write-Host "✓ Azure CLI este deja instalat" -ForegroundColor Green
} catch {
    Write-Host "× Azure CLI nu este instalat" -ForegroundColor Red
    Write-Host "Descarcă și instalează Azure CLI de la: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    Write-Host "Sau rulează în PowerShell ca Administrator:" -ForegroundColor Yellow
    Write-Host 'Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi; Start-Process msiexec.exe -Wait -ArgumentList "/I AzureCLI.msi /quiet"; Remove-Item .\AzureCLI.msi' -ForegroundColor White
    Write-Host "`nDupă instalare, închide și redeschide PowerShell, apoi rulează din nou acest script." -ForegroundColor Yellow
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

# Setează variabilele
$resourceGroup = "proiect-pssc-rg"
$namespace = "proiect-pssc-servicebus-" + (Get-Random -Minimum 1000 -Maximum 9999)

Write-Host "`n=== Step 2: Verificare/Creare Resource Group ===" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroup" -ForegroundColor White

# Verifică dacă resource group-ul există deja
$existingRg = az group show --name $resourceGroup --query "{Location:location}" -o json 2>$null | ConvertFrom-Json

if ($existingRg) {
    $location = $existingRg.Location
    Write-Host "✓ Resource Group există deja în locația: $location" -ForegroundColor Green
    if ($location -ne $Location) {
        Write-Host "  (Notă: Resource group-ul există în '$location', nu în '$Location' cum ai specificat)" -ForegroundColor Yellow
    }
} else {
    $location = $Location
    Write-Host "Creez Resource Group în locația: $location" -ForegroundColor White
    
    az group create --name $resourceGroup --location $location
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "× Eroare la crearea resource group" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✓ Resource Group creat" -ForegroundColor Green
}

# Creează Service Bus Namespace
Write-Host "`n=== Step 3: Creare Service Bus Namespace ===" -ForegroundColor Cyan
Write-Host "Namespace: $namespace" -ForegroundColor White
Write-Host "Location: $location" -ForegroundColor White
Write-Host "Sku: Standard (necesar pentru Topics)" -ForegroundColor White

az servicebus namespace create `
    --resource-group $resourceGroup `
    --name $namespace `
    --location $location `
    --sku Standard

if ($LASTEXITCODE -ne 0) {
    Write-Host "× Eroare la crearea namespace" -ForegroundColor Red
    Write-Host "  Încearcă cu o altă locație: powershell -ExecutionPolicy Bypass -File setup-azure.ps1 -Location 'northeurope'" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Service Bus Namespace creat" -ForegroundColor Green

# Creează Queue pentru comenzi
Write-Host "`n=== Step 4: Creare Queue pentru comenzi ===" -ForegroundColor Cyan
az servicebus queue create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --name order-commands

Write-Host "✓ Queue 'order-commands' creat" -ForegroundColor Green

# Creează Topics și Subscriptions
Write-Host "`n=== Step 5: Creare Topics și Subscriptions ===" -ForegroundColor Cyan

# Topic 1: order-events
Write-Host "Creez topic: order-events..." -ForegroundColor White
az servicebus topic create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --name order-events

az servicebus topic subscription create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --topic-name order-events `
    --name shipping-subscription

Write-Host "✓ Topic 'order-events' și subscription 'shipping-subscription' create" -ForegroundColor Green

# Topic 2: package-events
Write-Host "Creez topic: package-events..." -ForegroundColor White
az servicebus topic create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --name package-events

az servicebus topic subscription create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --topic-name package-events `
    --name billing-subscription

Write-Host "✓ Topic 'package-events' și subscription 'billing-subscription' create" -ForegroundColor Green

# Topic 3: invoice-events
Write-Host "Creez topic: invoice-events..." -ForegroundColor White
az servicebus topic create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --name invoice-events

az servicebus topic subscription create `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --topic-name invoice-events `
    --name notification-subscription

Write-Host "✓ Topic 'invoice-events' și subscription 'notification-subscription' create" -ForegroundColor Green

# Obține Connection String
Write-Host "`n=== Step 6: Obținere Connection String ===" -ForegroundColor Cyan
$connectionString = az servicebus namespace authorization-rule keys list `
    --resource-group $resourceGroup `
    --namespace-name $namespace `
    --name RootManageSharedAccessKey `
    --query primaryConnectionString `
    --output tsv

Write-Host "✓ Connection String obținut" -ForegroundColor Green

# Actualizează appsettings.json
Write-Host "`n=== Step 7: Actualizare appsettings.json ===" -ForegroundColor Cyan

$appsettingsPath = ".\Proiect\appsettings.json"
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
}

# Salvează informațiile într-un fișier
$infoPath = ".\azure-config.txt"
@"
=== Configurare Azure Service Bus ===
Data: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

Resource Group: $resourceGroup
Location: $location
Namespace: $namespace

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

Pentru a șterge resursele când nu mai sunt necesare:
az group delete --name $resourceGroup --yes --no-wait
"@ | Out-File $infoPath -Encoding UTF8

Write-Host "`n=== ✓ Setup Complet ===" -ForegroundColor Green
Write-Host "Informațiile au fost salvate în: $infoPath" -ForegroundColor Yellow
Write-Host "`nPoți testa conexiunea rulând aplicația cu: dotnet run --project .\Proiect" -ForegroundColor Cyan
Write-Host "`nPentru a vedea resursele în Azure Portal:" -ForegroundColor Cyan
Write-Host "https://portal.azure.com/#@/resource/subscriptions/your-subscription-id/resourceGroups/$resourceGroup/overview" -ForegroundColor White
