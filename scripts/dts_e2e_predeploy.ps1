# DTS / État 9421 pre-deploy E2E:
# - seed default DTS workflow, start with CreateDeclaration
# - duplicate active guard, shell save, client Extrane list
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
        foreach ($k in $obj.Keys) {
            $parts += ('"' + $k + '":' + (ConvertTo-JsonSafe $obj[$k]))
        }
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
    try {
        return Invoke-RestMethod @params
    } catch {
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
    $login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" -Body (@{ email = $email; password = $password } | ConvertTo-Json)
    if (-not $login.data.accessToken) { throw "Login failed for $email" }
    return @{ token = $login.data.accessToken; userId = $login.data.user.id; fullName = $login.data.user.fullName; role = $login.data.user.role }
}

function Assert-Fail($method, $path, $token, $body, $mustContain) {
    try {
        $r = Api $method $path $token $body
        if ($r.success) { throw "Expected failure but succeeded on $path" }
        if ($mustContain -and ($r.message -notmatch [regex]::Escape($mustContain))) {
            throw "Expected '$mustContain' in: $($r.message)"
        }
        return $r.message
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) {
            if ($_.Exception.Message -match "Expected") { throw }
            throw
        }
        $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $j = ($reader.ReadToEnd() | ConvertFrom-Json)
        if ($j.success) { throw "Expected failure on $path" }
        if ($mustContain -and ($j.message -notmatch [regex]::Escape($mustContain))) {
            throw "Expected '$mustContain' in: $($j.message)"
        }
        return [string]$j.message
    }
}

Write-Host "=== Health ==="
$null = Invoke-RestMethod -Uri "$base/health" -UseBasicParsing
Write-Host "API OK"

$gerant = Login "gerant@cabinet-demo.ma" "Gerant@2026!"
$manager = Login "manager1@cabinet-demo.ma" "Demo@2026!"
$tokenG = $gerant.token
$tokenM = $manager.token

$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $tokenG
$idManager = ($emps.data.items | Where-Object { $_.email -eq "manager1@cabinet-demo.ma" }).id
if (-not $idManager) { throw "Missing demo manager employee" }

Write-Host "=== Seed default templates (ensure DTS) ==="
$seed = Api Post "/api/workflow/templates/seed-all-defaults" $tokenG @{}
Write-Host "  Seed: $($seed.message)"

$tpls = Api Get "/api/workflow/templates?declarationType=DTS&page=1&pageSize=20" $tokenG
if (-not $tpls.success) { throw "List DTS templates failed: $($tpls.message)" }
$dtsTpl = @($tpls.data.items | Where-Object { $_.isDefault -eq $true } | Select-Object -First 1)
if (-not $dtsTpl) { $dtsTpl = @($tpls.data.items | Select-Object -First 1) }
if (-not $dtsTpl) { throw "No DTS workflow template after seed" }
$tplId = $dtsTpl.id
Write-Host "DTS template #$tplId ($($dtsTpl.name), $($dtsTpl.stepCount) steps)"

$sfx = Get-Random -Minimum 20000 -Maximum 89999
$utc = (Get-Date).ToUniversalTime()
$dStart = $utc.AddDays(-15).ToString("yyyy-MM-dd")
$dEnd = $utc.AddYears(2).ToString("yyyy-MM-dd")
$annee = (Get-Date).Year

Write-Host "=== Client + contrat DTS only ==="
$client = Api Post "/api/clients" $tokenG @{
    raisonSociale = "E2E DTS $sfx SARL"; ice = "004$sfx"; if = "IFD$sfx"; rc = "RCD$sfx"; numeroCNSS = "CNSSD$sfx"
    formeJuridique = "SARL"; villeSiegeSocial = "Rabat"; adresseSiegeSocial = "Av DTS $sfx"
    telephonePrincipal = "+212537111222"; emailPrincipal = "e2edts$sfx@demo.ma"; typeTVA = "Mensuelle"; moisClotureExercice = 12
}
if (-not $client.success) { throw "Client create failed: $($client.message)" }
$clientId = $client.data.id

$contrat = Api Post "/api/contrats" $tokenG @{
    clientId = $clientId; objet = "Mission DTS E2E"; devise = "MAD"; dateDebutContrat = $dStart; dateFinContrat = $dEnd
    modeleFacturation = "Forfait"; frequenceFacturation = "Annuelle"; notes = "DTS predeploy E2E"
    membres = @(@{ userId = $idManager; roleLabel = "Superviseur" })
    servicesActives = @(@{ typeService = "DTS"; autoGenererDeclarations = $true })
    lignes = @(@{ typeLigne = "Honoraire"; description = "Honoraires DTS"; quantite = 1; prixUnitaireHT = 2500; tauxTVA = 20; modeFacturation = "Forfait" })
}
if (-not $contrat.success) { throw "Contrat create failed: $($contrat.message)" }
$cid = $contrat.data.id
$null = Api Post "/api/contrats/$cid/signer" $tokenG @{ signataireNom = "Demo"; signataireFonction = "Gerant" }
$val = Api Post "/api/contrats/$cid/valider" $tokenG @{}
if (-not $val.success) { throw "Contrat validate failed: $($val.message)" }

$clientEmail = "client.dts$sfx@demo.ma"; $clientPwd = "Client@2026!"
$cu = Api Post "/api/clients/$clientId/users" $tokenG @{ email = $clientEmail; password = $clientPwd; firstName = "Client"; lastName = "DTS$sfx" }
if (-not $cu.success) { throw "Client user create failed: $($cu.message)" }

Write-Host "=== Start DTS workflow (auto CreateDeclaration) ==="
$wf = Api Post "/api/workflow/start" $tokenG @{
    clientId = $clientId
    declarationType = "DTS"
    workflowTemplateId = $tplId
}
if (-not $wf.success) { throw "Start DTS workflow failed: $($wf.message)" }
$wfId = $wf.data.id
$declId = $wf.data.declarationId
if (-not $declId) { throw "Expected DeclarationId after DTS start, got null" }
Write-Host "WF=$wfId Decl=$declId"

$decl = Api Get "/api/declarations/$declId" $tokenG
if (-not $decl.success) { throw "Get declaration failed: $($decl.message)" }
if ($decl.data.type -ne "DTS") { throw "Expected type DTS, got $($decl.data.type)" }
$limiteRaw = $decl.data.dateLimite
$limite = [datetime]$limiteRaw
$expectedYear = $annee + 1
if ($limite.Year -ne $expectedYear -or $limite.Month -ne 3) {
    throw "Expected dateLimite ~ 31/03/$expectedYear, got $limiteRaw"
}
Write-Host "  Type=DTS Periode=$($decl.data.periode) Limite=$($limite.ToString('yyyy-MM-dd'))"

Write-Host "=== Duplicate active DTS start must fail ==="
$dupMsg = Assert-Fail Post "/api/workflow/start" $tokenG @{
    clientId = $clientId
    declarationType = "DTS"
    workflowTemplateId = $tplId
} "actif"
Write-Host "  Rejected: $dupMsg"

Write-Host "=== Shell save montants ==="
$shell = Api Post "/api/declarations/$declId/shell" $tokenG @{
    montantDu = 1500.50
    montantCredit = 0
    notes = "E2E DTS shell save $sfx"
}
if (-not $shell.success) { throw "Shell save failed: $($shell.message)" }
Write-Host "  Shell OK montantDu=$($shell.data.montantDu)"

Write-Host "=== Advance past AutoSystem Init if needed ==="
$inst = (Api Get "/api/workflow/instances/$wfId" $tokenG).data
$cur = @($inst.stepInstances | Where-Object { $_.status -in @("InProgress","WaitingForClient","WaitingForValidation") } | Select-Object -First 1)
if ($cur -and $cur.stepName -match "Ouverture|Init") {
    $adv = Api Post "/api/workflow/instances/$wfId/advance" $tokenG @{ notes = "Init DTS E2E"; comment = "Init" }
    if (-not $adv.success) { Write-Host "  Init advance skipped/failed: $($adv.message)" }
    $inst = (Api Get "/api/workflow/instances/$wfId" $tokenG).data
}

# Reach client collecte if still on demande pieces — advance with manager
for ($i = 0; $i -lt 3; $i++) {
    $inst = (Api Get "/api/workflow/instances/$wfId" $tokenG).data
    $cur = @($inst.stepInstances | Where-Object { $_.status -in @("InProgress","WaitingForClient","WaitingForValidation") } | Select-Object -First 1)
    if (-not $cur) { break }
    Write-Host "  Current step: $($cur.stepName) [$($cur.status)] assign=$($cur.assignmentType)"
    if ($cur.assignmentType -eq "Client" -or $cur.status -eq "WaitingForClient") { break }
    if ($cur.assignmentType -eq "AutoSystem") { Start-Sleep -Seconds 1; continue }
    $adv = Api Post "/api/workflow/instances/$wfId/advance" $tokenM @{ notes = "Advance DTS E2E"; comment = "Advance"; forceSkip = $true }
    if (-not $adv.success) {
        $adv = Api Post "/api/workflow/instances/$wfId/advance" $tokenG @{ notes = "Advance DTS E2E"; comment = "Advance"; forceSkip = $true }
    }
    if (-not $adv.success) { Write-Host "  Advance stop: $($adv.message)"; break }
}

Write-Host "=== Extranet client sees DTS declaration ==="
$clientLogin = Login $clientEmail $clientPwd
$tokenCl = $clientLogin.token
$clientDecls = Api Get "/api/client-declarations" $tokenCl
if (-not $clientDecls.success) { throw "Client declarations failed: $($clientDecls.message)" }
$found = @($clientDecls.data.items | Where-Object { $_.id -eq $declId -and $_.type -eq "DTS" })
if (-not $found -or $found.Count -eq 0) {
    throw "Client Extrane list missing DTS declaration #$declId"
}
Write-Host "  Client sees Decl #$declId type=DTS statut=$($found[0].statut)"

$tasks = Api Get "/api/client-workflow/my-tasks" $tokenCl
if ($tasks.success) {
    $pending = @($tasks.data | Where-Object { $_.workflowInstanceId -eq $wfId })
    Write-Host "  Client pending steps for WF: $($pending.Count)"
}

Write-Host "=== Cleanup: cancel WF ==="
$cancel = Api Post "/api/workflow/instances/$wfId/cancel" $tokenG "E2E DTS cleanup"
if (-not $cancel.success) {
    # Some APIs expect JSON string body
    $headers = @{ Authorization = "Bearer $tokenG" }
    try {
        $cancel = Invoke-RestMethod -Uri "$base/api/workflow/instances/$wfId/cancel" -Method Post -Headers $headers `
            -ContentType "application/json" -Body '"E2E DTS cleanup"' -UseBasicParsing
    } catch {
        Write-Host "  Cancel warn: $($_.Exception.Message)"
    }
}
Write-Host "  Cancel: success=$($cancel.success) msg=$($cancel.message)"

Write-Host ""
Write-Host "=== DTS E2E PASSED ===" -ForegroundColor Green
Write-Host "Client=$clientId WF=$wfId Decl=$declId Template=$tplId"
