# Script Final - Creează un branch complet nou fără istoric cu secrete
Write-Host "=== Creare Branch Nou Fără Secrete ===" -ForegroundColor Cyan

$projectRoot = "C:\Users\Ionela\Desktop\Semestrul 1\PSSC\PSSCproiect\Proiect"
cd $projectRoot

Write-Host "`n1. Verificare branch curent..." -ForegroundColor Yellow
$currentBranch = git branch --show-current
Write-Host "Branch curent: $currentBranch" -ForegroundColor White

Write-Host "`n2. Ștergere TOATE fișierele cu secrete din working directory..." -ForegroundColor Yellow

# Șterge appsettings.Local.json din toate locurile
Remove-Item "Proiect\appsettings.Local.json" -Force -ErrorAction SilentlyContinue
Remove-Item "Proiect.EventProcessor\appsettings.Local.json" -Force -ErrorAction SilentlyContinue
Remove-Item "Proiect\bin\Debug\net9.0\appsettings.Local.json" -Force -ErrorAction SilentlyContinue
Remove-Item "Proiect.EventProcessor\bin\Debug\net9.0\appsettings.Local.json" -Force -ErrorAction SilentlyContinue

Write-Host "✓ Fișiere cu secrete șterse din working directory" -ForegroundColor Green

Write-Host "`n3. Creez un branch complet nou (orphan) fără istoric..." -ForegroundColor Yellow
git checkout --orphan ionela-FINAL-clean

Write-Host "`n4. Adaug toate fișierele (FĂRĂ appsettings.Local.json)..." -ForegroundColor Yellow
git add .

Write-Host "`n5. Verific ce va fi commituit..." -ForegroundColor Yellow
Write-Host "Fișiere appsettings care vor fi commituite:" -ForegroundColor Cyan
git diff --cached --name-only | Select-String "appsettings"

Write-Host "`nDacă vezi appsettings.Local.json mai sus, OPREȘTE scriptul și șterge-le!" -ForegroundColor Red
Write-Host "Apasă ENTER pentru a continua sau CTRL+C pentru a opri..." -ForegroundColor Yellow
Read-Host

Write-Host "`n6. Creez commit nou fără istoric..." -ForegroundColor Yellow
git commit -m "Initial commit - clean version without any secrets in history"

Write-Host "`n7. Șterg branch-ul vechi problematic..." -ForegroundColor Yellow
git branch -D ionela-final -ErrorAction SilentlyContinue
git branch -D ionela-clean -ErrorAction SilentlyContinue

Write-Host "`n8. Push branch nou pe GitHub..." -ForegroundColor Yellow
git push origin ionela-FINAL-clean --force

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅✅✅ SUCCESS! Branch nou push-uit fără erori!" -ForegroundColor Green
    Write-Host "`nBranch-ul tău: ionela-FINAL-clean" -ForegroundColor Cyan
    Write-Host "URL: https://github.com/roxana-catrina/PSSCproiect/tree/ionela-FINAL-clean" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ Push FAILED! Mai există secrete în fișiere!" -ForegroundColor Red
    Write-Host "`nFișiere care pot conține secrete:" -ForegroundColor Yellow
    git ls-files | Select-String "appsettings\.Local|launchSettings"
    
    Write-Host "`n🔑 ULTIMA SOLUȚIE:" -ForegroundColor Yellow
    Write-Host "1. Regenerează cheia în Azure Portal" -ForegroundColor White
    Write-Host "2. SAU accesează URL-ul de mai sus pentru a permite secret-ul" -ForegroundColor White
}

Write-Host "`n=== Script Completed ===" -ForegroundColor Cyan

