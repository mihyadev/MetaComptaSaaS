# Seed one ready-to-test client: TVA + IS + CNSS, workflows, extranet user, open exercice.
# Prerequisites: API http://localhost:7100
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"
$annee = (Get-Date).Year

function Api($method, $path, $token, $body = $null) {
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{
        Uri             = "$base$path"
        Method          = $method
        Headers         = $headers
        UseBasicParsing = $true
    }
    if ($null -ne $body) {
        $params.ContentType = "application/json; charset=utf-8"
        $params.Body = ($body | ConvertTo-Json -Depth 12)
    }
    try {
        return Invoke-RestMethod @params
    } catch {
        $resp = $_.Exception.Response
        if ($null -ne $resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $raw = $reader.ReadToEnd()
            try { return ($raw | ConvertFrom-Json) } catch { throw "HTTP $($resp.StatusCode.value__): $raw" }
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

function Get-CabinetTemplate($token, $type) {
    $tpls = Api Get "/api/workflow/templates?declarationType=$type&page=1&pageSize=50" $token
    if (-not $tpls.success) { throw "List $type templates failed: $($tpls.message)" }
    $items = @($tpls.data.items)
    $pick = $items | Where-Object {
        -not $_.clientId -and $_.isDefault -eq $true -and ($_.isActive -eq $true -or $null -eq $_.isActive) -and ($_.name -notmatch '\(ancien\)')
    } | Select-Object -First 1
    if (-not $pick) {
        $pick = $items | Where-Object { -not $_.clientId -and ($_.name -notmatch '\(ancien\)') } | Select-Object -First 1
    }
    if (-not $pick) { throw "No cabinet workflow template for $type" }
    return $pick
}

Write-Host "=== Health ==="
$null = Invoke-RestMethod -Uri "$base/health" -UseBasicParsing
Write-Host "API OK"

$gerant = Login "gerant@cabinet-demo.ma" "Gerant@2026!"
$tokenG = $gerant.token

Write-Host "=== Cabinet staff ==="
$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $tokenG
$staff = @($emps.data.items)
$idFiscal = ($staff | Where-Object { $_.email -eq "fiscal@cabinet-demo.ma" }).id
$idComptable = ($staff | Where-Object { $_.email -eq "comptable1@cabinet-demo.ma" }).id
$idSocial = ($staff | Where-Object { $_.email -eq "social@cabinet-demo.ma" }).id
$idManager = ($staff | Where-Object { $_.email -eq "manager1@cabinet-demo.ma" }).id
if (-not $idFiscal) { $idFiscal = ($staff | Select-Object -First 1).id }
if (-not $idComptable) { $idComptable = ($staff | Select-Object -Skip 1 -First 1).id }
if (-not $idSocial) { $idSocial = $idComptable }
if (-not $idManager) { $idManager = ($staff | Select-Object -Skip 2 -First 1).id }
if (-not $idFiscal -or -not $idManager) { throw "Missing demo employees" }

Write-Host "=== Resolve WF templates TVA / IS / CNSS ==="
$tplTva = Get-CabinetTemplate $tokenG "TVA"
$tplIs = Get-CabinetTemplate $tokenG "IS"
$tplCnss = Get-CabinetTemplate $tokenG "CNSS"
Write-Host "TVA #$($tplTva.id) | IS #$($tplIs.id) | CNSS #$($tplCnss.id)"

$sfx = Get-Random -Minimum 30000 -Maximum 89999
$utc = (Get-Date).ToUniversalTime()
$dStart = $utc.AddDays(-30).ToString("yyyy-MM-dd")
$dEnd = $utc.AddYears(1).ToString("yyyy-MM-dd")

Write-Host "=== Create client ==="
$client = Api Post "/api/clients" $tokenG @{
    raisonSociale       = "Demo Process $sfx SARL"
    ice                 = "007$sfx"
    if                  = "IFD$sfx"
    rc                  = "RCD$sfx"
    numeroCNSS          = "CNSSD$sfx"
    formeJuridique      = "SARL"
    villeSiegeSocial    = "Casablanca"
    adresseSiegeSocial  = "Bd Test Process $sfx"
    telephonePrincipal  = "+212522111222"
    emailPrincipal      = "contact$sfx@demo.ma"
    typeTVA             = "Mensuelle"
    moisClotureExercice = 12
}
if (-not $client.success) { throw "Client create failed: $($client.message)" }
$clientId = $client.data.id
Write-Host "ClientId=$clientId"

Write-Host "=== Contrat TVA + IS + CNSS (Valide) ==="
$contrat = Api Post "/api/contrats" $tokenG @{
    clientId             = $clientId
    objet                = "Mission TVA + IS (liasse) + CNSS"
    devise               = "MAD"
    dateDebutContrat     = $dStart
    dateFinContrat       = $dEnd
    modeleFacturation    = "Forfait"
    frequenceFacturation = "Mensuelle"
    notes                = "Seed process test TVA/IS/CNSS"
    membres              = @(
        @{ userId = $idManager; roleLabel = "Chef de mission" },
        @{ userId = $idFiscal; roleLabel = "Fiscaliste" },
        @{ userId = $idComptable; roleLabel = "Comptable" },
        @{ userId = $idSocial; roleLabel = "Social" }
    )
    servicesActives      = @(
        @{ typeService = "TVA"; libelleService = "Declaration TVA"; autoGenererDeclarations = $true },
        @{ typeService = "IS"; libelleService = "Declaration fiscale - Liasse annuelle"; autoGenererDeclarations = $true },
        @{ typeService = "CNSS"; libelleService = "Declaration CNSS"; autoGenererDeclarations = $true }
    )
    lignes               = @(
        @{ typeLigne = "Honoraire"; description = "Honoraires mission TVA/IS/CNSS"; quantite = 1; prixUnitaireHT = 15000; tauxTVA = 20; modeFacturation = "Forfait" }
    )
}
if (-not $contrat.success) { throw "Contrat create failed: $($contrat.message)" }
$cid = $contrat.data.id
$null = Api Post "/api/contrats/$cid/signer" $tokenG @{ signataireNom = "Demo"; signataireFonction = "Gerant" }
$val = Api Post "/api/contrats/$cid/valider" $tokenG @{}
if (-not $val.success) { throw "Contrat validate failed: $($val.message)" }

$access = Api Get "/api/clients/$clientId/contract-access" $tokenG
$svcList = @($access.data.activeServices) | Sort-Object
Write-Host ("Access mode={0} services=[{1}]" -f $access.data.mode, ($svcList -join ", "))
foreach ($need in @("TVA", "IS", "CNSS")) {
    if ($svcList -notcontains $need) { throw "Missing service $need in activeServices" }
}

Write-Host "=== Open exercice $annee ==="
$exo = Api Post "/api/comptabilite/exercices" $tokenG @{
    clientId = $clientId
    annee    = $annee
}
if (-not $exo.success) { throw "Create exercice failed: $($exo.message)" }
$exoId = $exo.data.id
Write-Host "ExerciceId=$exoId Annee=$annee Statut=$($exo.data.statut)"

Write-Host "=== Extranet user ==="
$clientEmail = "client.process$sfx@demo.ma"
$clientPwd = "Client@2026!"
$cu = Api Post "/api/clients/$clientId/users" $tokenG @{
    email     = $clientEmail
    password  = $clientPwd
    firstName = "Client"
    lastName  = "Process$sfx"
}
if (-not $cu.success) { throw "Extranet user create failed: $($cu.message)" }
Write-Host "Extranet OK: $clientEmail / $clientPwd"

Write-Host "=== Assign client WF (SaveClientConfig = affectation + copie) ==="
foreach ($pair in @(
        @{ Type = "TVA"; Tpl = $tplTva.id },
        @{ Type = "IS"; Tpl = $tplIs.id },
        @{ Type = "CNSS"; Tpl = $tplCnss.id }
    )) {
    $ens = Api Post "/api/workflow/client-config" $tokenG @{
        clientId                = $clientId
        declarationType         = $pair.Type
        workflowTemplateId      = $pair.Tpl
        clientValidationEnabled = $true
        documentRequestEnabled  = $true
        autoStartWorkflow       = $true
        slaMultiplier           = 1
        isActive                = $true
    }
    if (-not $ens.success) { throw "Affect $($pair.Type) WF failed: $($ens.message)" }
    Write-Host ("  {0} attached -> template #{1}" -f $pair.Type, $ens.data.workflowTemplateId)
}

Write-Host "=== Skip start WF / declarations (affectation only) ==="

Write-Host ""
Write-Host "=== SUMMARY - ready to test (no declarations / no WF instances) ==="
Write-Host "ClientId=$clientId  Name=Demo Process $sfx SARL"
Write-Host "ContratId=$cid  ExerciceId=$exoId ($annee)"
Write-Host "Services=[$($svcList -join ', ')]"
Write-Host "Workflows attaches: TVA / IS / CNSS (ClientWorkflowConfigs only)"
Write-Host ""
Write-Host "BackOffice: http://localhost:7101"
Write-Host "  gerant@cabinet-demo.ma / Gerant@2026!"
Write-Host "Extranet:   http://localhost:7102"
Write-Host "  $clientEmail / $clientPwd"
Write-Host "DONE"
