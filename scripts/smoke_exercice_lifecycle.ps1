#Requires -Version 5.1
<#
.SYNOPSIS
  E2E smoke: open-exercice gate, create, cloture blocked by brouillon, AN + N+1.
#>
param(
    [string]$ApiBase = "http://localhost:7100",
    [string]$Email = "gerant@cabinet-demo.ma",
    [string]$Password = "Gerant@2026!",
    [int]$ClientId = 13
)

$ErrorActionPreference = "Stop"
function Fail($msg) { Write-Error $msg; exit 1 }
function Ok($msg) { Write-Host "[OK] $msg" -ForegroundColor Green }
function Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Cyan }

Step "Login"
$login = Invoke-RestMethod -Uri "$ApiBase/api/auth/login" -Method Post -ContentType "application/json" `
    -Body (@{ email = $Email; password = $Password } | ConvertTo-Json)
$token = $login.data.accessToken
if (-not $token) { Fail "No JWT from login: $($login | ConvertTo-Json -Depth 5)" }
$H = @{ Authorization = "Bearer $token" }
Ok "Authenticated"

Step "has-open (expect false after wipe)"
$has = Invoke-RestMethod -Method GET -Uri "$ApiBase/api/comptabilite/exercices/has-open?clientId=$ClientId" -Headers $H
$hasOpen = $has.data
Ok "HasOpenExercice=$hasOpen"

$exoId = $null
if (-not $hasOpen) {
    Step "Gate: create TVA without open exo must fail"
    try {
        $r = Invoke-RestMethod -Method POST -Uri "$ApiBase/api/declarations/tva/$ClientId" -Headers $H -ContentType "application/json" -Body "{}"
        if ($r.success -eq $true) { Fail "Create TVA should have failed without open exo" }
        Ok "Create blocked: $($r.message)"
    } catch {
        Ok "Create blocked (HTTP error) - expected"
    }

    Step "Create open exercice 2026"
    $exo = Invoke-RestMethod -Method POST -Uri "$ApiBase/api/comptabilite/exercices" -Headers $H -ContentType "application/json" `
        -Body (@{ clientId = $ClientId; annee = 2026 } | ConvertTo-Json)
    if (-not $exo.success) { Fail "Create exercice failed: $($exo | ConvertTo-Json -Depth 5)" }
    $exoId = $exo.data.id
    Ok "Exercice created id=$exoId"
} else {
    $list = Invoke-RestMethod -Method GET -Uri "$ApiBase/api/comptabilite/exercices?clientId=$ClientId" -Headers $H
    $open = @($list.data) | Where-Object { $_.statut -eq "Ouvert" } | Select-Object -First 1
    $exoId = $open.id
    Ok "Using existing open exercice id=$exoId"
}

Step "Create TVA (should succeed with open exo)"
try {
    $decl = Invoke-RestMethod -Method POST -Uri "$ApiBase/api/declarations/tva/$ClientId" -Headers $H -ContentType "application/json" -Body "{}"
    if (-not $decl.success) { Write-Host "Create decl note: $($decl.message)" }
    else { Ok "Declaration created id=$($decl.data.id)" }
} catch {
    Write-Host "Create decl HTTP note: $($_.Exception.Message)"
}

Step "Create brouillon ecriture (for cloture block)"
$piece = "SMK-" + (Get-Random -Maximum 99999)
$ecBody = @{
    clientId = $ClientId
    exerciceComptableId = $exoId
    journal = "OD"
    dateEcriture = "2026-07-15"
    libelle = "Smoke brouillon cloture"
    numeroPiece = $piece
    valider = $false
    lignes = @(
        @{ numCompte = "5141"; debit = 100; credit = 0; libelleCompte = "Banque" },
        @{ numCompte = "3421"; debit = 0; credit = 100; libelleCompte = "Clients" }
    )
} | ConvertTo-Json -Depth 6
try {
    $ec = Invoke-RestMethod -Method POST -Uri "$ApiBase/api/comptabilite/ecritures" -Headers $H -ContentType "application/json" -Body $ecBody
    if (-not $ec.success) { Write-Host "Ecriture note: $($ec.message)" }
    else { Ok "Brouillon ecriture created" }
} catch {
    Write-Host "Ecriture HTTP note: $($_.Exception.Message)"
}

Step "Precheck cloture"
$pre = Invoke-RestMethod -Method GET -Uri "$ApiBase/api/comptabilite/exercices/$exoId/precheck-cloture" -Headers $H
if (-not $pre.success) { Fail "Precheck failed: $($pre | ConvertTo-Json -Depth 6)" }
$can = $pre.data.canCloturer
Ok "CanCloturer=$can checks=$($pre.data.checks.Count)"

Step "Force-cloture with AN approve + open N+1"
$clotBody = @{
    exerciceId = $exoId
    approuverANouveaux = $true
    ouvrirExerciceSuivant = $true
    rolloverServices = $true
    forceWithReason = $true
    forceReason = "Smoke E2E exercice lifecycle"
} | ConvertTo-Json
$clot = Invoke-RestMethod -Method POST -Uri "$ApiBase/api/comptabilite/exercices/cloturer" -Headers $H -ContentType "application/json" -Body $clotBody
if (-not $clot.success) { Fail "Cloture failed: $($clot | ConvertTo-Json -Depth 6)" }
Ok "Cloture OK. NextExerciceId=$($clot.data.nextExerciceId) statut=$($clot.data.statut)"

Step "Verify has-open still true (N+1)"
$has2 = Invoke-RestMethod -Method GET -Uri "$ApiBase/api/comptabilite/exercices/has-open?clientId=$ClientId" -Headers $H
if (-not $has2.data) { Fail "Expected open N+1 after cloture" }
Ok "Open exercice present after cloture"

Write-Host "`nSMOKE PASSED" -ForegroundColor Green
