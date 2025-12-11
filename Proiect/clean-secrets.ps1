# Script pentru curățarea completă a secretelor din Git
Write-Host "=== Curățare Completă Secrete din Git ===" -ForegroundColor Cyan

$projectRoot = "C:\Users\Ionela\Desktop\Semestrul 1\PSSC\PSSCproiect\Proiect"
cd $projectRoot

Write-Host "`n1. Verificare branch curent..." -ForegroundColor Yellow
git branch --show-current

Write-Host "`n2. Ștergere fișiere cu secrete din Git tracking..." -ForegroundColor Yellow

# Șterge appsettings.Local.json din tracking (dar păstrează-l local)
git rm --cached Proiect/appsettings.Local.json -ErrorAction SilentlyContinue
git rm --cached Proiect.EventProcessor/appsettings.Local.json -ErrorAction SilentlyContinue

# Șterge și din bin/Debug dacă există
git rm --cached -r Proiect/bin -ErrorAction SilentlyContinue
git rm --cached -r Proiect.EventProcessor/bin -ErrorAction SilentlyContinue
git rm --cached -r Proiect/obj -ErrorAction SilentlyContinue
git rm --cached -r Proiect.EventProcessor/obj -ErrorAction SilentlyContinue

Write-Host "`n3. Verificare ce fișiere cu secrete mai sunt tracked..." -ForegroundColor Yellow
git ls-files | Select-String "appsettings"

Write-Host "`n4. Adăugare toate modificările..." -ForegroundColor Yellow
git add .

Write-Host "`n5. Status înainte de commit..." -ForegroundColor Yellow
git status

Write-Host "`n6. Commit modificări..." -ForegroundColor Yellow
git commit -m "Remove appsettings.Local.json from Git tracking - keep only placeholders"

Write-Host "`n7. Push pe branch..." -ForegroundColor Yellow
$currentBranch = git branch --show-current
Write-Host "Pushing to branch: $currentBranch" -ForegroundColor Green

git push origin $currentBranch

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ SUCCESS! Push completed without errors!" -ForegroundColor Green
} else {
    Write-Host "`n❌ Push failed! Checking for remaining secrets..." -ForegroundColor Red
    Write-Host "`nFișiere care pot conține secrete:" -ForegroundColor Yellow
    git ls-files | Select-String "appsettings\|launchSettings\|azure-config"
}

Write-Host "`n=== Script Completed ===" -ForegroundColor Cyan

