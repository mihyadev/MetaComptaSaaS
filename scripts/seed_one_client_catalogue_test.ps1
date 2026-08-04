# Keep ONE Active client (Id=7 by default) with ONE Valide contract + full declaration catalogue.
# Smoke-tests Create for every DeclarationServiceCatalog type.
# Prerequisites: API http://localhost:7100, SQL MetaComptaSASDb_Dev on localhost,58080
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"
$sqlServer = "localhost,58080"
$sqlDb = "MetaComptaSASDb_Dev"
$sqlUser = "sa"
$sqlPass = "RS2iRS2i"
$keepClientId = 7
$annee = (Get-Date).Year

$catalogueServices = @(
    "TVA", "IS", "ACOMPTES", "REG_IS", "CM", "DRF", "LIASSEFISCALE",
    "IR", "IR_SAL", "RAS_FONC", "RAS_DIV",
    "CNSS", "DTS",
    "COMPTABILITE", "PAIE", "BILAN", "ETATSFISCAUX"
)

$declarationTypes = @(
    @{ Type = "TVA"; Annee = $null },
    @{ Type = "CNSS"; Annee = $null },
    @{ Type = "IS"; Annee = $annee },
    @{ Type = "ACOMPTES"; Annee = $null },
    @{ Type = "REG_IS"; Annee = $annee },
    @{ Type = "CM"; Annee = $annee },
    @{ Type = "DRF"; Annee = $annee },
    @{ Type = "IR"; Annee = $annee },
    @{ Type = "IR_SAL"; Annee = $null },
    @{ Type = "RAS_FONC"; Annee = $null },
    @{ Type = "RAS_DIV"; Annee = $null },
    @{ Type = "DTS"; Annee = $annee }
)

function Api($method, $path, $token, $body = $null) {
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{
        Uri         = "$base$path"
        Method      = $method
        Headers     = $headers
        ContentType = "application/json"
        UseBasicParsing = $true
    }
    if ($null -ne $body) {
        $params.Body = ($body | ConvertTo-Json -Depth 12)
    }
    try {
        return Invoke-RestMethod @params
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $raw = $reader.ReadToEnd()
        try { return ($raw | ConvertFrom-Json) } catch { throw "HTTP $($resp.StatusCode): $raw" }
    }
}

function Invoke-Sql($query) {
    $tmp = [System.IO.Path]::GetTempFileName() + ".sql"
    Set-Content -Path $tmp -Value $query -Encoding UTF8
    $out = & sqlcmd -S $sqlServer -U $sqlUser -P $sqlPass -d $sqlDb -I -i $tmp -W -h -1 2>&1
    Remove-Item $tmp -Force -ErrorAction SilentlyContinue
    return $out
}

Write-Host "=== Health ==="
$null = Invoke-RestMethod -Uri "$base/health" -UseBasicParsing
Write-Host "API OK"

Write-Host "=== Login manager ==="
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" -Body (@{
    email = "manager1@cabinet-demo.ma"
    password = "Demo@2026!"
} | ConvertTo-Json)
$token = $login.data.accessToken
if (-not $token) { throw "No accessToken" }
Write-Host "OK $($login.data.user.fullName)"

Write-Host "=== Soft-delete every client except Id=$keepClientId ==="
$wipeOthers = @"
UPDATE Clients
SET IsDeleted = 1, DeletedAt = SYSUTCDATETIME(), UpdatedAt = SYSUTCDATETIME()
WHERE IsDeleted = 0 AND Id <> $keepClientId;

UPDATE ContratsCommerciaux
SET IsDeleted = 1, DeletedAt = SYSUTCDATETIME(), UpdatedAt = SYSUTCDATETIME()
WHERE IsDeleted = 0 AND ClientId <> $keepClientId;

SELECT COUNT(*) AS ActiveClients FROM Clients WHERE IsDeleted = 0;
"@
$wipeOut = Invoke-Sql $wipeOthers
Write-Host ($wipeOut | Out-String)

Write-Host "=== Keep ONE Valide contract: terminate extras / soft-delete non-Valide ==="
# Prefer the lowest Valide/Actif contract id; soft-delete other contracts for this client.
$contratInfo = Api Get "/api/contrats?clientId=$keepClientId&page=1&pageSize=50" $token
$items = @($contratInfo.data.items)
if ($items.Count -lt 1) { throw "No contracts for client $keepClientId" }

$valide = @($items | Where-Object { $_.statut -eq "Valide" -or $_.statut -eq "Actif" } | Sort-Object id)
if ($valide.Count -lt 1) {
    # Try validate a signed one
    $signe = @($items | Where-Object { $_.statut -eq "Signe" } | Sort-Object id | Select-Object -First 1)
    if ($null -eq $signe) { throw "No Valide/Signe contract to keep" }
    $v = Api Post "/api/contrats/$($signe.id)/valider" $token @{}
    if (-not $v.success) { throw "Valider failed: $($v.message)" }
    $keepContratId = $signe.id
} else {
    $keepContratId = $valide[0].id
    # Terminate extra Valide/Actif contracts so only one manage window remains
    foreach ($extra in ($valide | Select-Object -Skip 1)) {
        Write-Host "Terminer contrat extra $($extra.id) ($($extra.reference))"
        $t = Api Post "/api/contrats/$($extra.id)/terminer" $token @{}
        if (-not $t.success) { Write-Host "WARN terminer $($extra.id): $($t.message)" }
    }
}

# Soft-delete non-Valide/non-Actif contracts for this client (Signe drafts, Termine noise)
$softExtra = @"
UPDATE ContratsCommerciaux
SET IsDeleted = 1, DeletedAt = SYSUTCDATETIME(), UpdatedAt = SYSUTCDATETIME()
WHERE ClientId = $keepClientId AND IsDeleted = 0 AND Id <> $keepContratId
  AND Statut NOT IN (N'Valide', N'Actif');
"@
$null = Invoke-Sql $softExtra
Write-Host "OK keepContratId=$keepContratId"

Write-Host "=== Patch ServicesActivesJson on Valide contrat + sync ServiceContrats ==="
$svcJsonLines = @()
foreach ($code in $catalogueServices) {
    $svcJsonLines += "{`"TypeService`":`"$code`",`"LibelleService`":`"$code`",`"AutoGenererDeclarations`":true}"
}
$svcJson = "[" + ($svcJsonLines -join ",") + "]"
$svcJsonEsc = $svcJson.Replace("'", "''")
$patchSql = @"
UPDATE ContratsCommerciaux
SET ServicesActivesJson = N'$svcJsonEsc', UpdatedAt = SYSUTCDATETIME(), Notes = N'Seed catalogue test - one client'
WHERE Id = $keepContratId AND ClientId = $keepClientId;

UPDATE Clients
SET RaisonSociale = N'TEST Catalogue Declarations SARL',
    Statut = N'Active',
    UpdatedAt = SYSUTCDATETIME()
WHERE Id = $keepClientId;
"@
$null = Invoke-Sql $patchSql

$sync = Api Post "/api/contrats/$keepContratId/sync-services" $token @{}
if (-not $sync.success) { throw "sync-services failed: $($sync.message)" }
Write-Host "OK sync: $($sync.message)"

$access = Api Get "/api/clients/$keepClientId/contract-access" $token
$svcList = @($access.data.activeServices) | Sort-Object
Write-Host ("OK mode={0} services=[{1}]" -f $access.data.mode, ($svcList -join ", "))
if ($access.data.mode -ne "Manage") { throw "Expected Manage mode" }
foreach ($need in @("TVA","CNSS","IS","IR","ACOMPTES","DTS")) {
    if ($svcList -notcontains $need) { throw "Missing active service: $need" }
}

Write-Host "=== Ensure équipe on Valide contrat ==="
$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $token
$staff = @($emps.data.items | Where-Object { $_.estActif -or $_.isActive })
$equipe = @()
$roles = @("Chef de mission", "Collaborateur", "Superviseur")
for ($i = 0; $i -lt [Math]::Min(3, $staff.Count); $i++) {
    $equipe += @{ userId = $staff[$i].id; roleLabel = $roles[$i] }
}
$eq = Api Put "/api/contrats/$keepContratId/equipe" $token @{ membres = $equipe }
if (-not $eq.success) { Write-Host "WARN equipe: $($eq.message)" } else { Write-Host "OK equipe=$($equipe.Count)" }

Write-Host "=== Smoke-create one declaration per catalogue type ==="
$created = @()
$failed = @()
foreach ($d in $declarationTypes) {
    $body = @{ clientId = $keepClientId; type = $d.Type }
    if ($null -ne $d.Annee) { $body.anneeFiscale = $d.Annee }
    $r = Api Post "/api/declarations" $token $body
    if ($r.success) {
        $created += [pscustomobject]@{ Type = $d.Type; Id = $r.data.id; Statut = $r.data.statut; Periode = $r.data.periode }
        Write-Host ("  OK  {0,-10} id={1} statut={2} periode={3}" -f $d.Type, $r.data.id, $r.data.statut, $r.data.periode)
    } else {
        $failed += [pscustomobject]@{ Type = $d.Type; Message = $r.message }
        Write-Host ("  FAIL {0,-10} {1}" -f $d.Type, $r.message)
    }
}

Write-Host ""
Write-Host "=== SUMMARY ==="
Write-Host "ClientId=$keepClientId  Name=TEST Catalogue Declarations SARL  Statut=Active"
Write-Host "ContratId=$keepContratId  Mode=$($access.data.mode)"
Write-Host "ActiveServices=[$($svcList -join ', ')]"
Write-Host "Created=$($created.Count)/$($declarationTypes.Count)  Failed=$($failed.Count)"
Write-Host "BackOffice: http://localhost:7101  (manager1@cabinet-demo.ma / Demo@2026!)"
Write-Host "Extranet:   http://localhost:7102  (client.tva.manual@demo.ma / Client@2026!)"
Write-Host "Filter Declarations by client Id=$keepClientId - create/edit buttons for all catalogue types."

if ($failed.Count -gt 0) {
    Write-Host "FAILURES:"
    $failed | ForEach-Object { Write-Host ("  {0}: {1}" -f $_.Type, $_.Message) }
    exit 1
}
Write-Host "ALL CATALOGUE CREATE PATHS OK"
