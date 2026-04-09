
$ErrorActionPreference = "Continue"
$API = "http://localhost:7100"
$pass = 0; $fail = 0; $total = 0

function Test($label, $scriptBlock) {
    $script:total++
    try {
        $result = & $scriptBlock
        if ($result) {
            $script:pass++
            Write-Host "  [PASS] $label" -ForegroundColor Green
        } else {
            $script:fail++
            Write-Host "  [FAIL] $label -- returned falsy" -ForegroundColor Red
        }
    } catch {
        $script:fail++
        Write-Host "  [FAIL] $label -- $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Login($email, $password) {
    $body = @{email=$email; password=$password} | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/auth/login" -Method POST -Body $body -ContentType "application/json"
    return @{
        "Authorization" = "Bearer $($resp.data.accessToken)"
        "Content-Type"  = "application/json"
    }
}

function AdvanceStep($instanceId, $headers, $notes, $forceSkip) {
    $body = @{ notes = $notes }
    if ($forceSkip) { $body.forceSkip = $true }
    $body = $body | ConvertTo-Json
    return Invoke-RestMethod -Uri "$API/api/workflow/instances/$instanceId/advance" -Method POST -Headers $headers -Body $body
}

function GetInstance($instanceId, $headers) {
    return Invoke-RestMethod -Uri "$API/api/workflow/instances/$instanceId" -Headers $headers
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " METACOMPTASAS -- WORKFLOW E2E TEST v3" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ============ PHASE 1: Authentication ============
Write-Host "--- PHASE 1: Authentication ---" -ForegroundColor Yellow

$hGerant=$null; $hFatima=$null; $hYoussef=$null; $hAmina=$null; $hKarim=$null; $hSara=$null

Test "Login Gerant (SuperAdmin)" { $script:hGerant = Login "gerant@cabinet-demo.ma" "Gerant@2026!"; $script:hGerant -ne $null }
Test "Login Fatima (Comptable)" { $script:hFatima = Login "fatima.benali@cabinet-demo.ma" "Test@2026!"; $script:hFatima -ne $null }
Test "Login Youssef (Fiscaliste)" { $script:hYoussef = Login "youssef.tazi@cabinet-demo.ma" "Test@2026!"; $script:hYoussef -ne $null }
Test "Login Amina (Manager)" { $script:hAmina = Login "amina.rachidi@cabinet-demo.ma" "Test@2026!"; $script:hAmina -ne $null }
Test "Login Karim (Assistant)" { $script:hKarim = Login "karim.ouazzani@cabinet-demo.ma" "Test@2026!"; $script:hKarim -ne $null }
Test "Login Sara (Comptable)" { $script:hSara = Login "sara.elidrissi@cabinet-demo.ma" "Test@2026!"; $script:hSara -ne $null }

# ============ PHASE 2: TVA Full Workflow ============
Write-Host "`n--- PHASE 2: TVA Full Workflow (Client 1, ATLAS TEXTILE) ---" -ForegroundColor Yellow

$tvaDecl = $null; $tvaWf = $null

Test "Create TVA Declaration (2026-05)" {
    $body = @{ clientId = 1; type = "TVA"; periode = "2026-05" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:tvaDecl = $r.data.id
    Write-Host "    -> Declaration ID=$tvaDecl" -ForegroundColor DarkGray
    $r.success -eq $true -and $tvaDecl -gt 0
}

Test "Start TVA Workflow" {
    $body = @{ declarationId = $tvaDecl; clientId = 1; declarationType = "TVA"; workflowTemplateId = 1 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:tvaWf = $r.data.id
    Write-Host "    -> WF ID=$tvaWf" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "TVA WF started (Active, step 0 auto-advanced)" {
    $inst = GetInstance $tvaWf $hGerant
    Write-Host "    -> Status=$($inst.data.status) Step=$($inst.data.currentStepOrder)" -ForegroundColor DarkGray
    $inst.data.status -eq "Active" -and $inst.data.currentStepOrder -ge 1
}

# TVA has 9 steps (0-8). Step 0 auto-advances. Last step (8) auto-completes.
# Manual advances needed: steps 1,2,3,4,5,6,7 = 7 advances
# Step 6 is non-required client approval
$tvaAdvUsers = @($hYoussef, $hGerant, $hFatima, $hAmina, $hAmina, $hGerant, $hYoussef)
$tvaAdvNames = @("Youssef(Fiscaliste)","Gerant(ForClient)","Fatima(Comptable)","Amina(Manager)","Amina(Manager)","Gerant(ForClient)","Youssef(Fiscaliste)")
$tvaStepNames = @("Demande factures","Collecte docs","Saisie TVA","Controle TVA","Validation","Approbation client","Teledeclaration")

for ($i = 0; $i -lt 7; $i++) {
    Test "TVA Advance: $($tvaStepNames[$i]) by $($tvaAdvNames[$i])" {
        $r = AdvanceStep $tvaWf $tvaAdvUsers[$i] "TVA step $($i+1) by $($tvaAdvNames[$i])" $false
        if (-not $r.success) { Write-Host "    -> FAILED: $($r.message)" -ForegroundColor Red }
        else { Write-Host "    -> OK" -ForegroundColor DarkGray }
        $r.success -eq $true
    }
}

Test "TVA Workflow COMPLETED" {
    $inst = GetInstance $tvaWf $hGerant
    Write-Host "    -> Status=$($inst.data.status) Step=$($inst.data.currentStepOrder)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

Test "TVA Declaration status = Soumise" {
    $d = Invoke-RestMethod -Uri "$API/api/declarations/$tvaDecl" -Headers $hGerant
    Write-Host "    -> Declaration status: $($d.data.statut)" -ForegroundColor DarkGray
    $d.data.statut -eq "Soumise"
}

# ============ PHASE 3: IS Full Workflow (Client 10) ============
Write-Host "`n--- PHASE 3: IS Workflow (Client 10) ---" -ForegroundColor Yellow

$isDecl = $null; $isWf = $null

Test "Create IS Declaration (2026)" {
    $body = @{ clientId = 10; type = "IS"; periode = "2026"; anneeFiscale = 2026 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:isDecl = $r.data.id
    Write-Host "    -> IS Declaration ID=$isDecl" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Start IS Workflow" {
    $body = @{ declarationId = $isDecl; clientId = 10; declarationType = "IS"; workflowTemplateId = 2 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:isWf = $r.data.id
    Write-Host "    -> IS WF ID=$isWf" -ForegroundColor DarkGray
    $r.success -eq $true
}

# IS: 9 steps (0-8), auto steps at 0 and 8 => 7 manual advances
$isAdvUsers = @($hFatima, $hGerant, $hYoussef, $hAmina, $hAmina, $hGerant, $hYoussef)
$isAdvNames = @("Fatima","Gerant","Youssef","Amina","Amina","Gerant","Youssef")
$isStepNames = @("Demande docs","Collecte","Retraitements","Controle liasse","Validation expert","Validation client","Depot DGI")

for ($i = 0; $i -lt 7; $i++) {
    Test "IS Advance: $($isStepNames[$i]) by $($isAdvNames[$i])" {
        $r = AdvanceStep $isWf $isAdvUsers[$i] "IS step by $($isAdvNames[$i])" $false
        if (-not $r.success) { Write-Host "    -> FAILED: $($r.message)" -ForegroundColor Red }
        else { Write-Host "    -> OK" -ForegroundColor DarkGray }
        $r.success -eq $true
    }
}

Test "IS Workflow COMPLETED" {
    $inst = GetInstance $isWf $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ============ PHASE 4: CNSS Full Workflow (Client 9 - BOUTIQUE LUXE) ============
Write-Host "`n--- PHASE 4: CNSS Workflow (Client 9) ---" -ForegroundColor Yellow

$cnssDecl = $null; $cnssWf = $null

Test "Create CNSS Declaration (2026-04)" {
    $body = @{ clientId = 9; type = "CNSS"; periode = "2026-04" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:cnssDecl = $r.data.id
    Write-Host "    -> CNSS Declaration ID=$cnssDecl" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Start CNSS Workflow" {
    $body = @{ declarationId = $cnssDecl; clientId = 9; declarationType = "CNSS"; workflowTemplateId = 4 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:cnssWf = $r.data.id
    Write-Host "    -> CNSS WF ID=$cnssWf" -ForegroundColor DarkGray
    $r.success -eq $true
}

# CNSS: 8 steps (0-7), auto at 0 and 7 => 6 manual advances (no client approval step)
$cnssAdvUsers = @($hSara, $hGerant, $hSara, $hAmina, $hAmina, $hSara)
$cnssAdvNames = @("Sara","Gerant","Sara","Amina","Amina","Sara")
$cnssStepNames = @("Demande personnel","Collecte paie","Calcul cotisations","Controle bordereau","Validation","Teledeclaration Damancom")

for ($i = 0; $i -lt 6; $i++) {
    Test "CNSS Advance: $($cnssStepNames[$i]) by $($cnssAdvNames[$i])" {
        $r = AdvanceStep $cnssWf $cnssAdvUsers[$i] "CNSS by $($cnssAdvNames[$i])" $false
        if (-not $r.success) { Write-Host "    -> FAILED: $($r.message)" -ForegroundColor Red }
        else { Write-Host "    -> OK" -ForegroundColor DarkGray }
        $r.success -eq $true
    }
}

Test "CNSS Workflow COMPLETED" {
    $inst = GetInstance $cnssWf $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ============ PHASE 5: IR Workflow (Client 17 - BRASSERIE) ============
Write-Host "`n--- PHASE 5: IR Workflow (Client 17) ---" -ForegroundColor Yellow

$irDecl = $null; $irWf = $null

Test "Create IR Declaration (2026)" {
    $body = @{ clientId = 17; type = "IR"; periode = "2026"; anneeFiscale = 2026 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:irDecl = $r.data.id
    Write-Host "    -> IR Declaration ID=$irDecl" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Start IR Workflow" {
    $body = @{ declarationId = $irDecl; clientId = 17; declarationType = "IR"; workflowTemplateId = 3 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:irWf = $r.data.id
    Write-Host "    -> IR WF ID=$irWf" -ForegroundColor DarkGray
    $r.success -eq $true
}

# IR: check how many manual steps needed
Test "IR WF active" {
    $inst = GetInstance $irWf $hGerant
    Write-Host "    -> Status=$($inst.data.status) Step=$($inst.data.currentStepOrder)" -ForegroundColor DarkGray
    $inst.data.status -eq "Active"
}

# Advance until completed (max 10 attempts)
$irStepCount = 0
for ($i = 0; $i -lt 10; $i++) {
    $inst = GetInstance $irWf $hGerant
    if ($inst.data.status -eq "Completed") { break }
    $user = if ($i -in @(3,4)) { $hAmina } elseif ($i % 2 -eq 0) { $hKarim } else { $hGerant }
    $uName = if ($i -in @(3,4)) { "Amina" } elseif ($i % 2 -eq 0) { "Karim" } else { "Gerant" }
    Test "IR Advance step $($inst.data.currentStepOrder) by $uName" {
        $r = AdvanceStep $irWf $user "IR advance by $uName" $false
        if (-not $r.success) { Write-Host "    -> FAILED: $($r.message)" -ForegroundColor Red }
        else { Write-Host "    -> OK" -ForegroundColor DarkGray }
        $r.success -eq $true
    }
    $irStepCount++
}

Test "IR Workflow COMPLETED" {
    $inst = GetInstance $irWf $hGerant
    Write-Host "    -> Status=$($inst.data.status) (after $irStepCount advances)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ============ PHASE 6: Authorization Tests ============
Write-Host "`n--- PHASE 6: Authorization Tests ---" -ForegroundColor Yellow

Test "All employees can list workflows" {
    $ok = $true
    foreach ($h in @($hFatima,$hYoussef,$hAmina,$hKarim,$hSara)) {
        $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?page=1&pageSize=1" -Headers $h
        if ($r.success -ne $true) { $ok = $false }
    }
    Write-Host "    -> All 5 employees can list" -ForegroundColor DarkGray
    $ok
}

Test "Unauthenticated = 401" {
    try {
        $null = Invoke-WebRequest -Uri "$API/api/workflow/instances" -Method GET -ErrorAction Stop
        $false
    } catch {
        $sc = $_.Exception.Response.StatusCode.value__
        Write-Host "    -> HTTP $sc" -ForegroundColor DarkGray
        $sc -eq 401
    }
}

# ============ PHASE 7: ForceSkip on optional step ============
Write-Host "`n--- PHASE 7: ForceSkip Test ---" -ForegroundColor Yellow

$skipDecl = $null; $skipWf = $null

Test "Create TVA for ForceSkip test (2026-06)" {
    $body = @{ clientId = 1; type = "TVA"; periode = "2026-06" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:skipDecl = $r.data.id
    $r.success -eq $true
}

Test "Start ForceSkip TVA workflow" {
    $body = @{ declarationId = $skipDecl; clientId = 1; declarationType = "TVA"; workflowTemplateId = 1 } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:skipWf = $r.data.id
    $r.success -eq $true
}

# Advance to step 6 (5 advances: steps 1,2,3,4,5)
for ($i = 0; $i -lt 5; $i++) {
    $null = AdvanceStep $skipWf $hGerant "Skip-test fast-forward $i" $false
}

Test "At step 6 (non-required client approval)" {
    $inst = GetInstance $skipWf $hGerant
    Write-Host "    -> Step=$($inst.data.currentStepOrder) Name=$($inst.data.currentStepName)" -ForegroundColor DarkGray
    $inst.data.currentStepOrder -eq 6
}

Test "ForceSkip step 6" {
    $r = AdvanceStep $skipWf $hGerant "Skipping client approval" $true
    Write-Host "    -> success=$($r.success)" -ForegroundColor DarkGray
    $r.success -eq $true
}

# Advance remaining (step 7 teledeclaration, step 8 auto)
$null = AdvanceStep $skipWf $hGerant "Step 7 teledeclaration" $false

Test "ForceSkip workflow completed" {
    $inst = GetInstance $skipWf $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ============ PHASE 8: Query & Filter Tests ============
Write-Host "`n--- PHASE 8: Query & Filter Tests ---" -ForegroundColor Yellow

Test "Filter instances by client=1" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?clientId=1" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) instances" -ForegroundColor DarkGray
    $r.success -eq $true -and $r.data.totalCount -gt 0
}

Test "Filter instances by status=Completed" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?status=Completed" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) completed" -ForegroundColor DarkGray
    $r.success -eq $true -and $r.data.totalCount -ge 4
}

Test "List all workflow templates" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/templates?page=1&pageSize=50" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) templates" -ForegroundColor DarkGray
    $r.data.totalCount -ge 9
}

Test "Get my pending steps" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/my-steps" -Headers $hFatima
    Write-Host "    -> $($r.data.Count) pending for Fatima" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Declaration locked after completion" {
    $d = Invoke-RestMethod -Uri "$API/api/declarations/$tvaDecl" -Headers $hGerant
    Write-Host "    -> TVA status: $($d.data.statut)" -ForegroundColor DarkGray
    $d.data.statut -in @("Soumise","Validee","Archivee")
}

# ============ SUMMARY ============
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " TEST RESULTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Total:  $total" -ForegroundColor White
Write-Host "  Passed: $pass" -ForegroundColor Green
if ($fail -gt 0) {
    Write-Host "  Failed: $fail" -ForegroundColor Red
} else {
    Write-Host "  Failed: $fail" -ForegroundColor Green
}
$pct = [math]::Round(($pass / $total) * 100, 1)
Write-Host "  Rate:   $pct%" -ForegroundColor $(if($pct -ge 90){"Green"}elseif($pct -ge 70){"Yellow"}else{"Red"})
Write-Host "========================================`n" -ForegroundColor Cyan
