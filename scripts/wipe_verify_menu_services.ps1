# Wipe-verify: create one contract (TVA/COMPTABILITE/CNSS) + équipe, assert menu ActiveServices
# Prerequisites: clean DB + API healthy with demo cabinet employees
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"
$expected = @("CNSS", "COMPTABILITE", "TVA")

function Api($method, $path, $token, $body = $null) {
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{
        Uri = "$base$path"
        Method = $method
        Headers = $headers
        ContentType = "application/json"
        UseBasicParsing = $true
    }
    if ($null -ne $body) { $params.Body = ($body | ConvertTo-Json -Depth 10) }
    return Invoke-RestMethod @params
}

function Assert-Eq($label, $actual, $expected) {
    if ("$actual" -ne "$expected") { throw "ASSERT FAIL [$label]: actual='$actual' expected='$expected'" }
    Write-Host "OK  $label = $actual"
}

function Assert-Services($label, $actualList, $expectedList) {
    $a = @($actualList | ForEach-Object { "$_" } | Sort-Object)
    $e = @($expectedList | ForEach-Object { "$_" } | Sort-Object)
    if (($a -join ",") -ne ($e -join ",")) {
        throw "ASSERT FAIL [$label]: actual=[$($a -join ',')] expected=[$($e -join ',')]"
    }
    Write-Host "OK  $label = [$($a -join ', ')]"
}

Write-Host "=== Login ==="
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" -Body (@{
    email = "manager1@cabinet-demo.ma"
    password = "Demo@2026!"
} | ConvertTo-Json)
$token = $login.data.accessToken
if (-not $token) { throw "No accessToken" }
Write-Host "OK  $($login.data.user.fullName)"

Write-Host "=== Load employees for équipe ==="
$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $token
$staff = @($emps.data.items | Where-Object { $_.estActif -or $_.isActive })
if ($staff.Count -lt 1) { throw "No active employees after wipe/seed" }
$equipe = @()
$roles = @("Chef de mission", "Collaborateur", "Superviseur")
for ($i = 0; $i -lt [Math]::Min(3, $staff.Count); $i++) {
    $equipe += @{ userId = $staff[$i].id; roleLabel = $roles[$i] }
}
Write-Host ("OK  équipe={0}" -f (($equipe | ForEach-Object { $_.roleLabel }) -join ", "))

Write-Host "=== Create client ==="
$sfx = Get-Random -Minimum 10000 -Maximum 99999
$client = Api Post "/api/clients" $token @{
    raisonSociale = "Verify Menu SARL"
    ice = "001$sfx"
    if = "IF$sfx"
    rc = "RC$sfx"
    numeroCNSS = "CNSS$sfx"
    formeJuridique = "SARL"
    villeSiegeSocial = "Casablanca"
    adresseSiegeSocial = "Bd Verification $sfx"
    telephonePrincipal = "+212522000000"
    emailPrincipal = "verify@$sfx.ma"
    typeTVA = "Mensuelle"
    moisClotureExercice = 12
}
if (-not $client.success) { throw "Client create failed: $($client.message)" }
$clientId = $client.data.id
Write-Host "OK  ClientId=$clientId"

Write-Host "=== Create contract TVA+COMPTABILITE+CNSS + équipe ==="
$utc = (Get-Date).ToUniversalTime()
$debut = $utc.AddDays(-7).ToString("yyyy-MM-dd")
$fin = $utc.AddYears(1).ToString("yyyy-MM-dd")
$svc = @($expected | ForEach-Object { @{ typeService = $_; autoGenererDeclarations = $true } })
$ct = Api Post "/api/contrats" $token @{
    clientId = $clientId
    objet = "Mission verify menu = ActiveServices"
    devise = "MAD"
    dateDebutContrat = $debut
    dateFinContrat = $fin
    modeleFacturation = "Forfait"
    frequenceFacturation = "Mensuelle"
    notes = "Wipe+verify fixture"
    membres = $equipe
    servicesActives = $svc
    lignes = @(
        @{
            typeLigne = "Honoraire"
            description = "Honoraires mission"
            quantite = 1
            prixUnitaireHT = 3000
            tauxTVA = 20
        }
    )
}
if (-not $ct.success) { throw "Contrat create failed: $($ct.message)" }
$contratId = $ct.data.id
$contratRef = $ct.data.reference
Write-Host "OK  Contrat=$contratRef Id=$contratId Membres=$($ct.data.membres.Count)"
if ($ct.data.membres.Count -lt 1) { throw "Équipe not assigned on create" }

Write-Host "=== AFTER SIGN (expect Consult / empty ActiveServices) ==="
$sign = Api Post "/api/contrats/$contratId/signer" $token @{
    signataireNom = "Verify Signataire"
    signataireFonction = "Gérant"
}
if (-not $sign.success) { throw "Sign failed: $($sign.message)" }
$accessSigned = Api Get "/api/clients/$clientId/contract-access" $token
Assert-Eq "Signed.Mode" $accessSigned.data.mode "Consult"
Assert-Eq "Signed.Reason" $accessSigned.data.reason "AwaitingValidation"
$signedSvcs = @($accessSigned.data.activeServices)
if ($signedSvcs.Count -gt 0) {
    throw "ASSERT FAIL [Signed.ActiveServices]: expected empty, got [$($signedSvcs -join ', ')]"
}
Write-Host "OK  Signed.ActiveServices = []"

Write-Host "=== AFTER VALIDATE (expect Manage / exact services) ==="
$val = Api Post "/api/contrats/$contratId/valider" $token @{}
if (-not $val.success) { throw "Validate failed: $($val.message)" }
Assert-Eq "Validated.Statut" $val.data.statut "Valide"
$accessValide = Api Get "/api/clients/$clientId/contract-access" $token
Assert-Eq "Valide.Mode" $accessValide.data.mode "Manage"
Assert-Eq "Valide.Reason" $accessValide.data.reason "Ok"
Assert-Services "Valide.ActiveServices" $accessValide.data.activeServices $expected

# Contract ServicesActives codes must match menu ActiveServices
$contractCodes = @($val.data.servicesActives | ForEach-Object { $_.typeService } | Sort-Object -Unique)
Assert-Services "Contract.ServicesActives vs menu" $accessValide.data.activeServices $contractCodes

Write-Host ""
Write-Host "=== SUMMARY ==="
Write-Host "BackOffice: http://localhost:7101"
Write-Host "Login: manager1@cabinet-demo.ma / Demo@2026!"
Write-Host "ClientId=$clientId Contrat=$contratRef ($contratId)"
Write-Host "Signed => Consult + empty menu services"
Write-Host "Valide => Manage + menu [$($expected -join ', ')]"
Write-Host "VERIFY PASS"
