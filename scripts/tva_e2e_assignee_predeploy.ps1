# TVA pre-deploy E2E: assignee-only advance + role-split through Cloturee
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
        if ($mustContain -and ($r.message -notmatch [regex]::Escape($mustContain))) { throw "Expected '$mustContain' in: $($r.message)" }
        return $r.message
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $j = ($reader.ReadToEnd() | ConvertFrom-Json)
        if ($j.success) { throw "Expected failure on $path" }
        if ($mustContain -and ($j.message -notmatch [regex]::Escape($mustContain))) { throw "Expected '$mustContain' in: $($j.message)" }
        return [string]$j.message
    }
}

Write-Host "=== Health ==="
$null = Invoke-RestMethod -Uri "$base/health" -UseBasicParsing
Write-Host "API OK"

$gerant = Login "gerant@cabinet-demo.ma" "Gerant@2026!"
$fiscal = Login "fiscal@cabinet-demo.ma" "Demo@2026!"
$comptable = Login "comptable1@cabinet-demo.ma" "Demo@2026!"
$manager = Login "manager1@cabinet-demo.ma" "Demo@2026!"
$tokenG = $gerant.token; $tokenF = $fiscal.token; $tokenC = $comptable.token; $tokenM = $manager.token

$emps = Api Get "/api/cabinet/employees?page=1&pageSize=50" $tokenG
$idFiscal = ($emps.data.items | Where-Object { $_.email -eq "fiscal@cabinet-demo.ma" }).id
$idComptable = ($emps.data.items | Where-Object { $_.email -eq "comptable1@cabinet-demo.ma" }).id
$idManager = ($emps.data.items | Where-Object { $_.email -eq "manager1@cabinet-demo.ma" }).id
if (-not $idFiscal -or -not $idComptable -or -not $idManager) { throw "Missing demo employees" }
Write-Host "IDs Fiscal=$idFiscal Comptable=$idComptable Manager=$idManager"

Write-Host "=== Use default TVA template (assignees set in DB) ==="
$tpls = Api Get "/api/workflow/templates?declarationType=TVA&page=1&pageSize=20" $tokenG
if (-not $tpls.success) { throw "List templates failed: $($tpls.message)" }
$tvaTpl = @($tpls.data.items | Where-Object { $_.isDefault -eq $true } | Select-Object -First 1)
if (-not $tvaTpl) { $tvaTpl = @($tpls.data.items | Select-Object -First 1) }
if (-not $tvaTpl) { throw "No TVA template" }
$tplId = $tvaTpl.id
Write-Host "Template #$tplId OK (Fiscaliste/Comptable/Manager assignees applied)"

$sfx = Get-Random -Minimum 20000 -Maximum 89999
$utc = (Get-Date).ToUniversalTime()
$dStart = $utc.AddDays(-15).ToString("yyyy-MM-dd")
$dEnd = $utc.AddYears(1).ToString("yyyy-MM-dd")

Write-Host "=== Client + contrat TVA ==="
$client = Api Post "/api/clients" $tokenG @{
    raisonSociale = "E2E TVA Assignee $sfx SARL"; ice = "003$sfx"; if = "IF$sfx"; rc = "RC$sfx"; numeroCNSS = "CNSS$sfx"
    formeJuridique = "SARL"; villeSiegeSocial = "Casablanca"; adresseSiegeSocial = "Bd E2E $sfx"
    telephonePrincipal = "+212522111222"; emailPrincipal = "e2e$sfx@demo.ma"; typeTVA = "Mensuelle"; moisClotureExercice = 12
}
if (-not $client.success) { throw "Client create failed: $($client.message)" }
$clientId = $client.data.id

$contrat = Api Post "/api/contrats" $tokenG @{
    clientId = $clientId; objet = "Mission TVA E2E"; devise = "MAD"; dateDebutContrat = $dStart; dateFinContrat = $dEnd
    modeleFacturation = "Forfait"; frequenceFacturation = "Mensuelle"; notes = "TVA predeploy E2E"
    membres = @(@{ userId = $idFiscal; roleLabel = "Fiscaliste" }, @{ userId = $idComptable; roleLabel = "Comptable" }, @{ userId = $idManager; roleLabel = "Superviseur" })
    servicesActives = @(@{ typeService = "TVA"; autoGenererDeclarations = $true })
    lignes = @(@{ typeLigne = "Honoraire"; description = "Honoraires TVA"; quantite = 1; prixUnitaireHT = 3000; tauxTVA = 20; modeFacturation = "Forfait" })
}
if (-not $contrat.success) { throw "Contrat create failed: $($contrat.message)" }
$cid = $contrat.data.id
$null = Api Post "/api/contrats/$cid/signer" $tokenG @{ signataireNom = "Demo"; signataireFonction = "Gerant" }
$val = Api Post "/api/contrats/$cid/valider" $tokenG @{}
if (-not $val.success) { throw "Contrat validate failed: $($val.message)" }

$clientEmail = "client.e2e$sfx@demo.ma"; $clientPwd = "Client@2026!"
$cu = Api Post "/api/clients/$clientId/users" $tokenG @{ email = $clientEmail; password = $clientPwd; firstName = "Client"; lastName = "E2E$sfx" }
if (-not $cu.success) { throw "Client user create failed: $($cu.message)" }

Write-Host "=== Declaration + start WF ==="
$decl = Api Post "/api/declarations/tva/$clientId" $tokenG @{}
if (-not $decl.success) { $decl = Api Post "/api/declarations" $tokenG @{ clientId = $clientId; type = "TVA"; periode = (Get-Date).ToString("yyyy-MM") } }
if (-not $decl.success) { throw "Declaration create failed: $($decl.message)" }
$declId = $decl.data.id

$wf = Api Post "/api/workflow/start" $tokenG @{ declarationId = $declId; clientId = $clientId; declarationType = "TVA"; workflowTemplateId = $tplId }
if (-not $wf.success) { throw "Start workflow failed: $($wf.message)" }
$wfId = $wf.data.id
Write-Host "WF=$wfId Decl=$declId"

function Get-Instance($tok) { return (Api Get "/api/workflow/instances/$wfId" $tok).data }
function Get-CurrentStep($inst) {
    $a = @($inst.stepInstances | Where-Object { $_.status -in @("InProgress","WaitingForClient","WaitingForValidation") } | Select-Object -First 1)
    if (-not $a) { $a = @($inst.stepInstances | Where-Object { $_.status -eq "Pending" } | Sort-Object stepOrder | Select-Object -First 1) }
    return $a
}
function Advance-As($tok, $label, $notes, $forceSkip = $false) {
    $body = @{ notes = $notes; comment = $notes }
    if ($forceSkip) { $body.forceSkip = $true }
    $r = Api Post "/api/workflow/instances/$wfId/advance" $tok $body
    if (-not $r.success) { throw "[$label] Advance failed: $($r.message) $($r | ConvertTo-Json -Compress -Depth 4)" }
    Write-Host "  [$label] OK"
    return $r
}

Write-Host "=== Comptable cannot advance Fiscaliste step ==="
$msg = Assert-Fail Post "/api/workflow/instances/$wfId/advance" $tokenC @{ notes = "should fail" } "assign"
Write-Host "  Rejected: $msg"

Write-Host "=== Fiscaliste comment + advance ==="
$tasks = Api Get "/api/tasks?assignedTo=me&page=1&pageSize=20" $tokenF
$task = @($tasks.data.items | Where-Object { $_.declarationId -eq $declId } | Select-Object -First 1)
if ($task) {
    $cmt = Api Post "/api/tasks/$($task.id)/comments" $tokenF @{ Contenu = "Demande docs E2E." }
    if (-not $cmt.success) { throw "Comment failed: $($cmt.message)" }
}
Advance-As $tokenF "Fiscaliste" "Demande docs OK"

Write-Host "=== Upload doc (Fiscaliste) + client approve collecte ==="
# Client role has no document.upload claim; cabinet uploads then client completes collecte
$boundary = [guid]::NewGuid().ToString()
$LF = "`r`n"
$bodyLines = @(
    "--$boundary", "Content-Disposition: form-data; name=`"titre`"$LF", "Facture E2E $sfx",
    "--$boundary", "Content-Disposition: form-data; name=`"categorie`"$LF", "Facture",
    "--$boundary", "Content-Disposition: form-data; name=`"clientId`"$LF", "$clientId",
    "--$boundary", "Content-Disposition: form-data; name=`"declarationId`"$LF", "$declId",
    "--$boundary", "Content-Disposition: form-data; name=`"file`"; filename=`"facture-e2e.txt`"",
    "Content-Type: text/plain$LF", "Facture E2E TVA $sfx", "--$boundary--"
) -join $LF
$upload = Invoke-RestMethod -Uri "$base/api/documents/upload" -Method Post -Headers @{ Authorization = "Bearer $tokenF" } `
    -ContentType "multipart/form-data; boundary=$boundary" -Body ([Text.Encoding]::UTF8.GetBytes($bodyLines))
if (-not $upload.success) { throw "Upload failed: $($upload.message)" }
Write-Host "  Doc #$($upload.data.id) uploaded"

$clientLogin = Login $clientEmail $clientPwd
$tokenCl = $clientLogin.token
$approve = Api Post "/api/client-workflow/instances/$wfId/approve" $tokenCl @{ notes = "Docs OK"; comment = "Collecte" }
if (-not $approve.success) { throw "Client approve failed: $($approve.message)" }
Write-Host "  Collecte approved by client"

Write-Host "=== Comptable steps ==="
for ($i = 0; $i -lt 8; $i++) {
    $inst = Get-Instance $tokenG; $cur = Get-CurrentStep $inst
    if (-not $cur) { break }
    $n = $cur.stepName
    Write-Host "  Step: $n"
    if ($n -match "Revue|Validation|SIMPL|Soumission|Paiement|Rapprochement|Archivage|Cloture") { break }
    if ($cur.assignmentType -eq "AutoSystem") { Start-Sleep -Seconds 1; continue }
    if ($cur.assignedToUserId -eq $idComptable -or $n -match "Contrôle|Controle|calcul|Import|anomal|Saisie") {
        $tasksC = Api Get "/api/tasks?assignedTo=me&page=1&pageSize=20" $tokenC
        $tC = @($tasksC.data.items | Where-Object { $_.declarationId -eq $declId } | Select-Object -First 1)
        if ($tC) { $null = Api Post "/api/tasks/$($tC.id)/comments" $tokenC @{ Contenu = "OK $n" } }
        $force = ($cur.isRequired -eq $false)
        try { Advance-As $tokenC "Comptable" "OK $n" $force }
        catch { Advance-As $tokenC "Comptable" "OK $n" $false }
        continue
    }
    break
}

Write-Host "=== Manager revue ==="
$inst = Get-Instance $tokenG; $cur = Get-CurrentStep $inst
if ($cur -and ($cur.stepName -match "Revue|Validation" -or $cur.assignedToUserId -eq $idManager)) {
    Advance-As $tokenM "Manager" "Revue OK"
}

Write-Host "=== Fiscaliste stubs to Cloturee ==="
for ($i = 0; $i -lt 12; $i++) {
    $inst = Get-Instance $tokenG
    if ($inst.status -eq "Completed") { break }
    $cur = Get-CurrentStep $inst
    if (-not $cur) { break }
    Write-Host "  Step: $($cur.stepName) [$($cur.assignmentType)]"
    if ($cur.assignmentType -eq "AutoSystem") { Start-Sleep -Milliseconds 800; continue }
    $tok = $tokenF
    if ($cur.assignedToUserId -eq $idManager) { $tok = $tokenM }
    if ($cur.assignedToUserId -eq $idComptable) { $tok = $tokenC }
    $force = ($cur.isRequired -eq $false)
    try { Advance-As $tok "Assignee" "Stub $($cur.stepName)" $force }
    catch { Advance-As $tok "Assignee" "Stub" $false }
}

$inst = Get-Instance $tokenG
$declFinal = Api Get "/api/declarations/$declId" $tokenG
Write-Host "WF=$($inst.status) Decl=$($declFinal.data.statut) steps=$($inst.completedSteps)/$($inst.totalSteps)"
if ($inst.status -ne "Completed" -and $declFinal.data.statut -ne "Cloturee") {
    foreach ($s in ($inst.stepInstances | Sort-Object stepOrder)) { Write-Host ("  [{0}] {1}={2}" -f $s.stepOrder, $s.stepName, $s.status) }
    throw "E2E did not reach Cloturee"
}
Write-Host "E2E PASSED"
exit 0
