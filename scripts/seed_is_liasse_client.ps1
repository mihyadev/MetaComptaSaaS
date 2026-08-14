# Seed one client: Extrane user + Valide contract with IS only + IS declaration + workflow attached.
# Prerequisites: API http://localhost:7100
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"
$annee = (Get-Date).Year

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
    $login = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" -Body (@{
        email = $email; password = $password
    } | ConvertTo-Json)
    if (-not $login.data.accessToken) { throw "Login failed for $email" }
    return @{ token = $login.data.accessToken; userId = $login.data.user.id; fullName = $login.data.user.fullName }
}

Write-Host "=== Health ==="
$null = Invoke-RestMethod -Uri "$base/health" -UseBasicParsing
Write-Host "API OK"

$gerant = Login "gerant@cabinet-demo.ma" "Gerant@2026!"
$tokenG = $gerant.token

$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $tokenG
$idFiscal = ($emps.data.items | Where-Object { $_.email -eq "fiscal@cabinet-demo.ma" }).id
$idComptable = ($emps.data.items | Where-Object { $_.email -eq "comptable1@cabinet-demo.ma" }).id
$idManager = ($emps.data.items | Where-Object { $_.email -eq "manager1@cabinet-demo.ma" }).id
if (-not $idFiscal) { $idFiscal = ($emps.data.items | Select-Object -First 1).id }
if (-not $idComptable) { $idComptable = ($emps.data.items | Select-Object -Skip 1 -First 1).id }
if (-not $idManager) { $idManager = ($emps.data.items | Select-Object -Skip 2 -First 1).id }
if (-not $idFiscal -or -not $idManager) { throw "Missing demo employees for equipe dossier" }

Write-Host "=== Resolve IS default workflow template ==="
$tpls = Api Get "/api/workflow/templates?declarationType=IS&page=1&pageSize=20" $tokenG
if (-not $tpls.success) { throw "List IS templates failed: $($tpls.message)" }
$isTpl = @($tpls.data.items | Where-Object { $_.isDefault -eq $true -and ($_.isActive -eq $true -or $null -eq $_.isActive) } | Select-Object -First 1)
if (-not $isTpl) { $isTpl = @($tpls.data.items | Where-Object { $_.isDefault -eq $true } | Select-Object -First 1) }
if (-not $isTpl) { $isTpl = @($tpls.data.items | Select-Object -First 1) }
if (-not $isTpl) { throw "No IS workflow template found - restart API to refresh IS seed" }
$tplId = $isTpl.id
Write-Host "Template #$tplId - $($isTpl.name) (steps=$($isTpl.steps.Count))"

$sfx = Get-Random -Minimum 20000 -Maximum 89999
$utc = (Get-Date).ToUniversalTime()
$dStart = $utc.AddDays(-30).ToString("yyyy-MM-dd")
$dEnd = $utc.AddYears(1).ToString("yyyy-MM-dd")

Write-Host "=== Client + contrat IS only (Valide) ==="
$client = Api Post "/api/clients" $tokenG @{
    raisonSociale = "Liasse IS Extrane $sfx SARL"
    ice = "005$sfx"; if = "IFI$sfx"; rc = "RCI$sfx"; numeroCNSS = "CNSSI$sfx"
    formeJuridique = "SARL"; villeSiegeSocial = "Casablanca"; adresseSiegeSocial = "Bd Liasse $sfx"
    telephonePrincipal = "+212522333444"; emailPrincipal = "liasse$sfx@demo.ma"
    typeTVA = "Mensuelle"; moisClotureExercice = 12
}
if (-not $client.success) { throw "Client create failed: $($client.message)" }
$clientId = $client.data.id

$contrat = Api Post "/api/contrats" $tokenG @{
    clientId = $clientId
    objet = "Mission Declaration fiscale - Liasse annuelle"
    devise = "MAD"
    dateDebutContrat = $dStart
    dateFinContrat = $dEnd
    modeleFacturation = "Forfait"
    frequenceFacturation = "Annuelle"
    notes = "Seed IS-only Extrane client"
    membres = @(
        @{ userId = $idFiscal; roleLabel = "Collaborateur" },
        @{ userId = $idComptable; roleLabel = "Comptable" },
        @{ userId = $idManager; roleLabel = "Chef de mission" }
    )
    servicesActives = @(
        @{ typeService = "IS"; libelleService = "Declaration fiscale - Liasse annuelle"; autoGenererDeclarations = $true }
    )
    lignes = @(
        @{ typeLigne = "Honoraire"; description = "Honoraires liasse fiscale / IS"; quantite = 1; prixUnitaireHT = 12000; tauxTVA = 20; modeFacturation = "Forfait" }
    )
}
if (-not $contrat.success) { throw "Contrat create failed: $($contrat.message)" }
$cid = $contrat.data.id
$null = Api Post "/api/contrats/$cid/signer" $tokenG @{ signataireNom = "Demo"; signataireFonction = "Gerant" }
$val = Api Post "/api/contrats/$cid/valider" $tokenG @{}
if (-not $val.success) { throw "Contrat validate failed: $($val.message)" }

$access = Api Get "/api/clients/$clientId/contract-access" $tokenG
$svcList = @($access.data.activeServices) | Sort-Object
Write-Host ("Contract access mode={0} services=[{1}]" -f $access.data.mode, ($svcList -join ", "))
if ($access.data.mode -ne "Manage") { throw "Expected Manage mode after Valide contract" }
if ($svcList -notcontains "IS") { throw "Expected IS in activeServices, got [$($svcList -join ', ')]" }
$extra = @($svcList | Where-Object { $_ -ne "IS" })
if ($extra.Count -gt 0) { Write-Host "WARN unexpected extra services: [$($extra -join ', ')]" }

$clientEmail = "client.liasse$sfx@demo.ma"
$clientPwd = "Client@2026!"
$cu = Api Post "/api/clients/$clientId/users" $tokenG @{
    email = $clientEmail; password = $clientPwd; firstName = "Client"; lastName = "Liasse$sfx"
}
if (-not $cu.success) { throw "Extrane user create failed: $($cu.message)" }
Write-Host "Extrane user OK: $clientEmail"

Write-Host "=== Create IS declaration + attach workflow ==="
$decl = Api Post "/api/declarations/is/$clientId/$annee" $tokenG @{}
if (-not $decl.success) {
    $decl = Api Post "/api/declarations" $tokenG @{ clientId = $clientId; type = "IS"; anneeFiscale = $annee }
}
if (-not $decl.success) { throw "IS declaration create failed: $($decl.message)" }
$declId = $decl.data.id

$wf = Api Post "/api/workflow/start" $tokenG @{
    declarationId = $declId
    clientId = $clientId
    declarationType = "IS"
    workflowTemplateId = $tplId
}
if (-not $wf.success) {
    $wf = Api Post "/api/workflow/start" $tokenG @{
        clientId = $clientId
        declarationType = "IS"
        workflowTemplateId = $tplId
    }
}
if (-not $wf.success) { throw "Start IS workflow failed: $($wf.message)" }
$wfId = $wf.data.id
if ($wf.data.declarationId) { $declId = $wf.data.declarationId }

$inst = (Api Get "/api/workflow/instances/$wfId" $tokenG).data
$cur = @($inst.stepInstances | Where-Object { $_.status -in @("InProgress","WaitingForClient","WaitingForValidation") } | Select-Object -First 1)
if (-not $cur) {
    $cur = @($inst.stepInstances | Where-Object { $_.status -eq "Pending" } | Sort-Object stepOrder | Select-Object -First 1)
}

$clientLogin = Login $clientEmail $clientPwd
$listPath = "/api/client-declarations?page=1" + [char]38 + "pageSize=20"
$list = Api Get $listPath $clientLogin.token
$seen = @($list.data.items | Where-Object { $_.id -eq $declId -or $_.type -eq "IS" })
if ($seen.Count -lt 1) { Write-Host "WARN Extrane list has no IS declaration yet" }

Write-Host ""
Write-Host "=== SUMMARY ==="
Write-Host "ClientId=$clientId  Name=Liasse IS Extrane $sfx SARL"
Write-Host "ContratId=$cid  Mode=$($access.data.mode)  Services=[$(($svcList -join ', '))]"
Write-Host "DeclarationId=$declId  Type=IS  Periode=$annee"
Write-Host "WorkflowId=$wfId  Template=$tplId  CurrentStep=$($cur.stepName)"
Write-Host "BackOffice: http://localhost:7101  (gerant@cabinet-demo.ma / Gerant@2026!)"
Write-Host "Extranet:   http://localhost:7102  ($clientEmail / $clientPwd)"
Write-Host "DONE"
