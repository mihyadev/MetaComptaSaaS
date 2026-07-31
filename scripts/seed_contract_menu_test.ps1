# Seed test clients + contracts: one-valid-per-period, team assignment, menu filtering
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"

function Api($method, $path, $token, $body = $null) {
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{
        Uri = "$base$path"
        Method = $method
        Headers = $headers
        ContentType = "application/json"
        UseBasicParsing = $true
    }
    if ($null -ne $body) {
        $params.Body = ($body | ConvertTo-Json -Depth 10)
    }
    return Invoke-RestMethod @params
}

function ApiExpectFail($method, $path, $token, $body = $null) {
    try {
        $null = Api $method $path $token $body
        return @{ success = $true; message = "UNEXPECTED SUCCESS" }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { return @{ success = $false; message = $_.Exception.Message } }
        $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $raw = $reader.ReadToEnd()
        try {
            $j = $raw | ConvertFrom-Json
            return @{ success = [bool]$j.success; message = [string]$j.message }
        } catch {
            return @{ success = $false; message = $raw }
        }
    }
}

Write-Host "=== Login (Manager) ==="
$login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" -Body (@{
    email = "manager1@cabinet-demo.ma"
    password = "Demo@2026!"
} | ConvertTo-Json)
$token = $login.data.accessToken
Write-Host "OK as $($login.data.user.fullName) / $($login.data.user.role)"

Write-Host "=== Load cabinet employees ==="
$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $token
$staff = @($emps.data.items | Where-Object { $_.estActif -or $_.isActive })
if ($staff.Count -lt 1) { throw "No active cabinet employees found for assignment." }
$equipe = @()
$roles = @("Chef de mission", "Collaborateur", "Superviseur")
$i = 0
foreach ($s in ($staff | Select-Object -First 3)) {
    $equipe += @{ userId = $s.id; roleLabel = $roles[$i % $roles.Count] }
    $i++
}
Write-Host ("Équipe dossier={0}" -f (($equipe | ForEach-Object { $_.roleLabel }) -join ", "))

function New-Client($name, $iceSuffix) {
    $dto = @{
        raisonSociale = $name
        ice = "002$iceSuffix"
        if = "IF$iceSuffix"
        rc = "RC$iceSuffix"
        numeroCNSS = "CNSS$iceSuffix"
        formeJuridique = "SARL"
        villeSiegeSocial = "Casablanca"
        adresseSiegeSocial = "Bd Test $iceSuffix"
        telephonePrincipal = "+212522000000"
        emailPrincipal = "contact@$($iceSuffix.ToLower()).ma"
        typeTVA = "Mensuelle"
        moisClotureExercice = 12
    }
    $r = Api Post "/api/clients" $token $dto
    if (-not $r.success) { throw "Client create failed: $($r.message)" }
    return $r.data
}

function New-Contrat($clientId, $objet, $services, $debut, $fin, $membres) {
    $svc = @()
    foreach ($s in $services) {
        $svc += @{ typeService = $s; autoGenererDeclarations = $true }
    }
    $dto = @{
        clientId = $clientId
        objet = $objet
        devise = "MAD"
        dateDebutContrat = $debut
        dateFinContrat = $fin
        modeleFacturation = "Forfait"
        frequenceFacturation = "Mensuelle"
        notes = "Seed test contract with team"
        membres = @($membres)
        servicesActives = $svc
        lignes = @(
            @{
                typeLigne = "Honoraire"
                description = "Honoraires mission fiduciaire"
                quantite = 1
                prixUnitaireHT = 5000
                tauxTVA = 20
                modeFacturation = "Forfait"
            }
        )
    }
    $r = Api Post "/api/contrats" $token $dto
    if (-not $r.success) { throw "Contrat create failed: $($r.message)" }
    return $r.data
}

function Sign-Only($contratId) {
    $sign = Api Post "/api/contrats/$contratId/signer" $token @{
        signataireNom = "Demo Signataire"
        signataireFonction = "Gerant"
    }
    if (-not $sign.success) { throw "Sign failed: $($sign.message)" }
    return $sign.data
}

function Sign-And-Validate($contratId) {
    $null = Sign-Only $contratId
    $val = Api Post "/api/contrats/$contratId/valider" $token @{}
    if (-not $val.success) { throw "Validate failed: $($val.message)" }
    return $val.data
}

function Show-Access($clientId, $label) {
    $a = Api Get "/api/clients/$clientId/contract-access" $token
    $svc = ($a.data.activeServices -join ", ")
    Write-Host ("  [{0}] Mode={1} Reason={2} Services=[{3}]" -f $label, $a.data.mode, $a.data.reason, $svc)
}

$utc = (Get-Date).ToUniversalTime()
$dStart = $utc.AddDays(-30).ToString("yyyy-MM-dd")
$dEnd = $utc.AddYears(1).ToString("yyyy-MM-dd")
$dPrevStart = $utc.AddYears(-1).ToString("yyyy-MM-dd")
$dPrevEnd = $utc.AddDays(-60).ToString("yyyy-MM-dd")
$sfx = Get-Random -Minimum 91000 -Maximum 98999

Write-Host ""
Write-Host "=== A: TVA + Compta + CNSS (Manage) ==="
$cA = New-Client "Seed Atlas Fiscal SARL" "$sfx"
$ctA = New-Contrat $cA.id "Mission TVA+Compta+CNSS" @("TVA","COMPTABILITE","TRESORERIE","CNSS") $dStart $dEnd $equipe
$ctA = Sign-And-Validate $ctA.id
Write-Host "  ClientId=$($cA.id) Contrat=$($ctA.reference) Statut=$($ctA.statut) Membres=$($ctA.membres.Count)"
Show-Access $cA.id "A"

Write-Host ""
Write-Host "=== B: Paie + IS (Manage, no TVA) ==="
$cB = New-Client "Seed Paie IS SARL" "$($sfx+1)"
$ctB = New-Contrat $cB.id "Mission Paie+IS" @("PAIE","ACOMPTES","REG_IS","DTS") $dStart $dEnd $equipe
$ctB = Sign-And-Validate $ctB.id
Write-Host "  ClientId=$($cB.id) Contrat=$($ctB.reference) Statut=$($ctB.statut)"
Show-Access $cB.id "B"

Write-Host ""
Write-Host "=== C: Signed only (Consult) ==="
$cC = New-Client "Seed Consult Only SARL" "$($sfx+2)"
$ctC = New-Contrat $cC.id "Mission signed not validated" @("TVA","IR_SAL","PV_AGO") $dStart $dEnd $equipe
$ctC = Sign-Only $ctC.id
Write-Host "  ClientId=$($cC.id) Contrat=$($ctC.reference) Statut=$($ctC.statut)"
Show-Access $cC.id "C"

Write-Host ""
Write-Host "=== D: Juridique + Liasse (Manage) ==="
$cD = New-Client "Seed Juridique Liasse SARL" "$($sfx+3)"
$ctD = New-Contrat $cD.id "Mission Juridique+Liasse" @("PV_AGO","DEPOT_RC","LIASSEFISCALE","BILAN") $dStart $dEnd $equipe
$ctD = Sign-And-Validate $ctD.id
Write-Host "  ClientId=$($cD.id) Contrat=$($ctD.reference) Statut=$($ctD.statut)"
Show-Access $cD.id "D"

Write-Host ""
Write-Host "=== E: Full fiscal pack (Manage) ==="
$cE = New-Client "Seed Full Fiscal SARL" "$($sfx+4)"
$ctE = New-Contrat $cE.id "Pack fiscal complet" @("TVA","IR_SAL","ACOMPTES","REG_IS","DRF","CM","COMPTABILITE") $dStart $dEnd $equipe
$ctE = Sign-And-Validate $ctE.id
Write-Host "  ClientId=$($cE.id) Contrat=$($ctE.reference) Statut=$($ctE.statut)"
Show-Access $cE.id "E"

Write-Host ""
Write-Host "=== F: Sequential periods (old Termine + current Valide) ==="
$cF = New-Client "Seed Sequential Periods SARL" "$($sfx+5)"
$ctFold = New-Contrat $cF.id "Exercice precedent" @("TVA","COMPTABILITE") $dPrevStart $dPrevEnd $equipe
$ctFold = Sign-And-Validate $ctFold.id
$term = Api Post "/api/contrats/$($ctFold.id)/terminer" $token @{}
Write-Host "  Old Contrat=$($ctFold.reference) now=$($term.data.statut)"
$ctFnew = New-Contrat $cF.id "Exercice courant" @("TVA","CNSS","PAIE") $dStart $dEnd $equipe
$ctFnew = Sign-And-Validate $ctFnew.id
Write-Host "  New Contrat=$($ctFnew.reference) Statut=$($ctFnew.statut)"
Show-Access $cF.id "F"

Write-Host ""
Write-Host "=== G: Overlap must FAIL validation ==="
$cG = New-Client "Seed Overlap Guard SARL" "$($sfx+6)"
$ctG1 = New-Contrat $cG.id "First active period" @("TVA","COMPTABILITE") $dStart $dEnd $equipe
$ctG1 = Sign-And-Validate $ctG1.id
Write-Host "  First OK: $($ctG1.reference) Statut=$($ctG1.statut)"
$ctG2 = New-Contrat $cG.id "Overlapping attempt" @("PAIE","CNSS") $dStart $dEnd $equipe
$ctG2 = Sign-Only $ctG2.id
$fail = ApiExpectFail Post "/api/contrats/$($ctG2.id)/valider" $token @{}
Write-Host "  Overlap Valider blocked: success=$($fail.success) msg=$($fail.message)"
Show-Access $cG.id "G"

Write-Host ""
Write-Host "=== SUMMARY ==="
Write-Host "BackOffice: http://localhost:7101"
Write-Host "Login: manager1@cabinet-demo.ma / Demo@2026!"
Write-Host "  A Id=$($cA.id) => TVA + Compta + CNSS"
Write-Host "  B Id=$($cB.id) => IS + Paie (no TVA)"
Write-Host "  C Id=$($cC.id) => Consultation only"
Write-Host "  D Id=$($cD.id) => Juridique + Liasse"
Write-Host "  E Id=$($cE.id) => Full fiscal"
Write-Host "  F Id=$($cF.id) => Sequential (old Termine + current Valide)"
Write-Host "  G Id=$($cG.id) => Overlap blocked on 2nd Valider"
Write-Host "All contracts assigned equipe du dossier (membres + roles)."
