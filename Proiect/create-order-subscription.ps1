# Script pentru crearea subscription-ului order-subscription
Write-Host "=== Creare order-subscription ===" -ForegroundColor Cyan

$resourceGroup = "PSSCproiect"
$namespace = "PSSCproiect"
$topicName = "order-events"
$subscriptionName = "order-subscription"

# Verifică dacă topic-ul există
Write-Host "Verificare topic: $topicName..." -ForegroundColor White
$topic = az servicebus topic show --resource-group $resourceGroup --namespace-name $namespace --name $topicName 2>$null

if (-not $topic) {
    Write-Host "× Topic '$topicName' nu există! Creez topic-ul..." -ForegroundColor Yellow
    az servicebus topic create --resource-group $resourceGroup --namespace-name $namespace --name $topicName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Topic '$topicName' creat" -ForegroundColor Green
    } else {
        Write-Host "× Eroare la crearea topic-ului" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✓ Topic '$topicName' există" -ForegroundColor Green
}

# Verifică dacă subscription-ul există
Write-Host "Verificare subscription: $subscriptionName..." -ForegroundColor White
$sub = az servicebus topic subscription show --resource-group $resourceGroup --namespace-name $namespace --topic-name $topicName --name $subscriptionName 2>$null

if (-not $sub) {
    Write-Host "× Subscription '$subscriptionName' nu există! Creez subscription-ul..." -ForegroundColor Yellow
    az servicebus topic subscription create --resource-group $resourceGroup --namespace-name $namespace --topic-name $topicName --name $subscriptionName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Subscription '$subscriptionName' creat" -ForegroundColor Green
    } else {
        Write-Host "× Eroare la crearea subscription-ului" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✓ Subscription '$subscriptionName' există deja" -ForegroundColor Green
}

Write-Host "`n=== Configurare completă ===" -ForegroundColor Green
Write-Host "Topic: $topicName" -ForegroundColor White
Write-Host "Subscription: $subscriptionName" -ForegroundColor White

