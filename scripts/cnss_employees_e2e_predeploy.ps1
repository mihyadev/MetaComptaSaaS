# CNSS client employees (entrants/sortants + CSV) smoke test
# Requires API http://localhost:7100
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"

function ConvertTo-JsonSafe($obj) {
    if ($null -eq $obj) { return "null" }
    if ($obj -is [bool]) { if ($obj) { return "true" } else { return "false" } }
    if ($obj -is [int] -or $obj -is [long] -or $obj -is [decimal] -or $obj -is [double]) { return "$obj" }
    if ($obj -is [string]) {
        $escaped = $obj.Replace('\','\\').Replace('"','\"').Replace("`r",'\r').Replace("`n",'\n').Replace("`t",'\t')
        return '"' + $escaped + '"'
    }
    if ($obj -is [System.Collections.IDictionary]) {
        $parts = @()
        foreach ($k in $obj.Keys) { $parts += ('"' + $k + '":' + (ConvertTo-JsonSafe $obj[$k])) }
        return '{' + ($parts -join ',') + '}'
    }
    if ($obj -is [System.Collections.IEnumerable]) {
        $parts = @()
        foreach ($item in $obj) { $parts += (ConvertTo-JsonSafe $item) }
        return '[' + ($parts -join ',') + ']'
    }
    return (ConvertTo-JsonSafe ([string]$obj))
}

function Api($method, $path, $token, $body = $null) {
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{ Uri = "$base$path"; Method = $method; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $body) {
        $params.ContentType = "application/json; charset=utf-8"
        $params.Body = ConvertTo-JsonSafe $body
    }
    try { return Invoke-RestMethod @params }
    catch {
        $resp = $_.Exception.Response
        if ($null -ne $resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $raw = $reader.ReadToEnd()
            try { return ($raw | ConvertFrom-Json) } catch { throw "HTTP $($resp.StatusCode): $raw" }
        }
        throw
    }
}

function Login($email, $password) {
    $login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" `
        -Body (@{ email = $email; password = $password } | ConvertTo-Json)
    if (-not $login.data.accessToken) { throw "Login failed for $email" }
    return $login.data.accessToken
}

Write-Host "=== Health ==="
$null = Invoke-RestMethod -Uri "$base/health" -UseBasicParsing
$token = Login "gerant@cabinet-demo.ma" "Gerant@2026!"

$sfx = Get-Random -Minimum 30000 -Maximum 89999
$utc = (Get-Date).ToUniversalTime()
$dStart = $utc.AddDays(-15).ToString("yyyy-MM-dd")
$dEnd = $utc.AddYears(1).ToString("yyyy-MM-dd")
$periodStart = (Get-Date).AddMonths(-1)
$periodStart = Get-Date -Year $periodStart.Year -Month $periodStart.Month -Day 1
$hireInPeriod = $periodStart.AddDays(4).ToString("yyyy-MM-dd")
$exitInPeriod = $periodStart.AddDays(18).ToString("yyyy-MM-dd")
$hireOld = $periodStart.AddMonths(-6).ToString("yyyy-MM-dd")

Write-Host "=== Client + contrat CNSS ==="
$client = Api Post "/api/clients" $token @{
    raisonSociale = "E2E CNSS Emp $sfx SARL"; ice = "005$sfx"; if = "IFC$sfx"; rc = "RCC$sfx"; numeroCNSS = "CNSSC$sfx"
    formeJuridique = "SARL"; villeSiegeSocial = "Casablanca"; adresseSiegeSocial = "Bd CNSS $sfx"
    telephonePrincipal = "+212522333444"; emailPrincipal = "e2ecnss$sfx@demo.ma"; typeTVA = "Mensuelle"; moisClotureExercice = 12
}
if (-not $client.success) { throw "Client: $($client.message)" }
$clientId = $client.data.id

$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $token
$idManager = ($emps.data.items | Where-Object { $_.email -eq "manager1@cabinet-demo.ma" }).id
if (-not $idManager) { $idManager = $emps.data.items[0].id }

$contrat = Api Post "/api/contrats" $token @{
    clientId = $clientId; objet = "Mission CNSS Emp"; devise = "MAD"; dateDebutContrat = $dStart; dateFinContrat = $dEnd
    modeleFacturation = "Forfait"; frequenceFacturation = "Mensuelle"; notes = "CNSS employees e2e"
    membres = @(@{ userId = $idManager; roleLabel = "Superviseur" })
    servicesActives = @(@{ typeService = "CNSS"; autoGenererDeclarations = $true })
    lignes = @(@{ typeLigne = "Honoraire"; description = "CNSS"; quantite = 1; prixUnitaireHT = 2000; tauxTVA = 20; modeFacturation = "Forfait" })
}
if (-not $contrat.success) { throw "Contrat: $($contrat.message)" }
$cid = $contrat.data.id
$null = Api Post "/api/contrats/$cid/signer" $token @{ signataireNom = "Demo"; signataireFonction = "Gerant" }
$val = Api Post "/api/contrats/$cid/valider" $token @{}
if (-not $val.success) { throw "Validate: $($val.message)" }

Write-Host "=== Create CNSS declaration ==="
$decl = Api Post "/api/declarations/cnss/$clientId" $token @{}
if (-not $decl.success) { $decl = Api Post "/api/declarations" $token @{ clientId = $clientId; type = "CNSS" } }
if (-not $decl.success) { throw "Decl: $($decl.message)" }
$declId = $decl.data.id
Write-Host "Decl=$declId Client=$clientId"

Write-Host "=== Manual add entrant ==="
$add = Api Post "/api/declarations/$declId/cnss/employees" $token @{
    cin = "CIN$sfx"; nom = "Entrant"; prenom = "Test"; numeroCNSS = "CNSS$sfx"
    matriculeEmployeur = "M$sfx"; dateEmbauche = $hireInPeriod; salaireBase = 8000; sexe = "M"
}
if (-not $add.success) { throw "Add: $($add.message)" }
if ($add.data.entrantCount -lt 1) { throw "Expected entrantCount >= 1, got $($add.data.entrantCount)" }
Write-Host "  Entrants=$($add.data.entrantCount) Present=$($add.data.presentCount)"

Write-Host "=== CSV import present + sortant ==="
$importRows = @(
    @{
        matricule = "MOLD$sfx"; cin = "COLD$sfx"; cnss = "CNSSOLD$sfx"
        nom = "Present"; prenom = "Old"; dateEmbauche = $hireOld; salaireBrut = 9000; sexe = "F"
    },
    @{
        matricule = "MOUT$sfx"; cin = "COUT$sfx"; cnss = "CNSSOUT$sfx"
        nom = "Sortant"; prenom = "Out"; dateEmbauche = $hireOld; dateSortie = $exitInPeriod
        motifSortie = "Démission"; salaireBrut = 7500; sexe = "M"
    }
)
$imp = Api Post "/api/declarations/$declId/cnss/employees/import" $token $importRows
if (-not $imp.success) { throw "Import: $($imp.message)" }
$roster = $imp.data.roster
if (-not $roster) { $roster = (Api Get "/api/declarations/$declId/cnss/employees" $token).data }
if ($roster.sortantCount -lt 1) { throw "Expected sortantCount >= 1" }
if ($roster.presentCount -lt 2) { throw "Expected presentCount >= 2, got $($roster.presentCount)" }
Write-Host "  After import: Present=$($roster.presentCount) Entrant=$($roster.entrantCount) Sortant=$($roster.sortantCount)"

Write-Host "=== Calculate CNSS ==="
$calc = Api Post "/api/declarations/cnss/$declId/calculate" $token @{}
if (-not $calc.success) { throw "Calc: $($calc.message)" }
if ($calc.data.nombreEmployes -lt 2) { throw "NombreEmployes too low: $($calc.data.nombreEmployes)" }
if ([string]::IsNullOrWhiteSpace($calc.data.detailEmployesJson)) { throw "DetailEmployesJson empty" }
Write-Host "  NombreEmployes=$($calc.data.nombreEmployes) TotalBruts=$($calc.data.totalSalairesBruts)"

$saveBody = @{
    declarationId = $declId
    totalSalairesBruts = [decimal]$calc.data.totalSalairesBruts
    totalCotisationSalariale = [decimal]$calc.data.totalCotisationSalariale
    totalCotisationPatronale = [decimal]$calc.data.totalCotisationPatronale
    totalAMOSalariale = [decimal]$calc.data.totalAMOSalariale
    totalAMOPatronale = [decimal]$calc.data.totalAMOPatronale
    totalFormationProfessionnelle = [decimal]$calc.data.totalFormationProfessionnelle
    montantTotal = [decimal]$calc.data.montantTotal
    nombreEmployes = [int]$calc.data.nombreEmployes
    detailEmployesJson = [string]$calc.data.detailEmployesJson
}
$save = Api Post "/api/declarations/cnss/$declId/data" $token $saveBody
if (-not $save.success) { throw "Save: $($save.message)" }

Write-Host ""
Write-Host "=== CNSS EMPLOYEES E2E PASSED ===" -ForegroundColor Green
Write-Host "Client=$clientId Decl=$declId Present=$($roster.presentCount) Entrant=$($roster.entrantCount) Sortant=$($roster.sortantCount)"
