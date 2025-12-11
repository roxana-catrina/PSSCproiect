# Script pentru crearea topic-urilor și subscription-urilor în Azure Service Bus
# Rulează acest script pentru a configura infrastructura necesară

param(
    [string]$ResourceGroupName = "PSSCProiectRG",
    [string]$NamespaceName = "psscproiect"
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Crearea topic-urilor Azure Service Bus" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Verifică dacă utilizatorul este autentificat
Write-Host "`nVerificare autentificare Azure..." -ForegroundColor Yellow
$context = Get-AzContext
if (!$context) {
    Write-Host "Nu ești autentificat. Rulează: Connect-AzAccount" -ForegroundColor Red
    exit 1
}

Write-Host "Autentificat ca: $($context.Account.Id)" -ForegroundColor Green

# Topic-urile și subscription-urile de creat
$topics = @(
    @{
        Name = "order-events"
        Subscription = "order-subscription"
        Description = "Topic pentru evenimente de comenzi"
    },
    @{
        Name = "invoice-events"
        Subscription = "invoice-subscription"
        Description = "Topic pentru evenimente de facturare"
    },
    @{
        Name = "package-events"
        Subscription = "package-subscription"
        Description = "Topic pentru evenimente de expediere"
    }
)

foreach ($topic in $topics) {
    Write-Host "`n------------------------------------------" -ForegroundColor Cyan
    Write-Host "Procesare: $($topic.Name)" -ForegroundColor Cyan
    Write-Host "------------------------------------------" -ForegroundColor Cyan
    
    # Verifică dacă topic-ul există
    $existingTopic = Get-AzServiceBusTopic -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -Name $topic.Name -ErrorAction SilentlyContinue
    
    if (!$existingTopic) {
        Write-Host "Creare topic: $($topic.Name)..." -ForegroundColor Yellow
        New-AzServiceBusTopic -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -Name $topic.Name
        Write-Host "✓ Topic creat: $($topic.Name)" -ForegroundColor Green
    } else {
        Write-Host "✓ Topic-ul există deja: $($topic.Name)" -ForegroundColor Green
    }
    
    # Verifică dacă subscription-ul există
    $existingSubscription = Get-AzServiceBusSubscription -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -TopicName $topic.Name -Name $topic.Subscription -ErrorAction SilentlyContinue
    
    if (!$existingSubscription) {
        Write-Host "Creare subscription: $($topic.Subscription)..." -ForegroundColor Yellow
        New-AzServiceBusSubscription -ResourceGroupName $ResourceGroupName -NamespaceName $NamespaceName -TopicName $topic.Name -Name $topic.Subscription
        Write-Host "✓ Subscription creat: $($topic.Subscription)" -ForegroundColor Green
    } else {
        Write-Host "✓ Subscription-ul există deja: $($topic.Subscription)" -ForegroundColor Green
    }
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "✅ Configurare completă!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan

Write-Host "`nTopic-uri create:" -ForegroundColor Yellow
Write-Host "  1. order-events -> order-subscription" -ForegroundColor White
Write-Host "  2. invoice-events -> invoice-subscription" -ForegroundColor White
Write-Host "  3. package-events -> package-subscription" -ForegroundColor White

Write-Host "`nPoți rula acum EventProcessor-ul!" -ForegroundColor Green

