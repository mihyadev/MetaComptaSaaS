import os
path = r"c:\Users\delli\OneDrive\Bureau\MetaComptaSAS\_workflow_test2.ps1"

content = r'''
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

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " METACOMPTASAS -- WORKFLOW E2E TEST" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# PHASE 1: Authentication
Write-Host "--- PHASE 1: Authentication ---" -ForegroundColor Yellow

$hGerant = $null; $hFatima = $null; $hYoussef = $null; $hAmina = $null; $hKarim = $null; $hSara = $null

Test "Login as Gerant (SuperAdmin)" {
    $script:hGerant = Login "gerant@cabinet-demo.ma" "Gerant@2026!"
    $script:hGerant -ne $null
}
Test "Login as Fatima Benali (Comptable)" {
    $script:hFatima = Login "fatima.benali@cabinet-demo.ma" "Test@2026!"
    $script:hFatima -ne $null
}
Test "Login as Youssef Tazi (Fiscaliste)" {
    $script:hYoussef = Login "youssef.tazi@cabinet-demo.ma" "Test@2026!"
    $script:hYoussef -ne $null
}
Test "Login as Amina Rachidi (Manager)" {
    $script:hAmina = Login "amina.rachidi@cabinet-demo.ma" "Test@2026!"
    $script:hAmina -ne $null
}
Test "Login as Karim Ouazzani (Assistant)" {
    $script:hKarim = Login "karim.ouazzani@cabinet-demo.ma" "Test@2026!"
    $script:hKarim -ne $null
}
Test "Login as Sara El Idrissi (Comptable)" {
    $script:hSara = Login "sara.elidrissi@cabinet-demo.ma" "Test@2026!"
    $script:hSara -ne $null
}

# PHASE 2: Create TVA Declaration
Write-Host "`n--- PHASE 2: Create TVA Declaration ---" -ForegroundColor Yellow

$declId = $null
Test "Create TVA Declaration (March 2026, Client 1)" {
    $body = @{ clientId = 1; type = "TVA"; periode = "2026-03" } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:declId = $resp.data.id
    Write-Host "    -> Created declaration ID=$script:declId" -ForegroundColor DarkGray
    $resp.success -eq $true -and $script:declId -gt 0
}

Test "Read back declaration -- status should be Brouillon" {
    $d = Invoke-RestMethod -Uri "$API/api/declarations/$script:declId" -Headers $hGerant
    Write-Host "    -> Status: $($d.data.statut)" -ForegroundColor DarkGray
    $d.data.statut -eq "Brouillon"
}

# PHASE 3: Start TVA Workflow
Write-Host "`n--- PHASE 3: Start TVA Workflow ---" -ForegroundColor Yellow

$wfInstanceId = $null
Test "Start TVA Workflow (template=1) on declaration" {
    $body = @{ declarationId = $declId; clientId = 1; declarationType = "TVA"; workflowTemplateId = 1 } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:wfInstanceId = $resp.data.id
    Write-Host "    -> Workflow instance ID=$script:wfInstanceId" -ForegroundColor DarkGray
    $resp.success -eq $true -and $script:wfInstanceId -gt 0
}

Test "Workflow instance is Active" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status) Step=$($inst.data.currentStepOrder)" -ForegroundColor DarkGray
    $inst.data.status -eq "Active"
}

# PHASE 4: Advance through ALL 9 TVA steps
Write-Host "`n--- PHASE 4: Advance TVA Workflow (9 steps) ---" -ForegroundColor Yellow

function AdvanceAndVerify($stepLabel, $headers, $userName, $expectedNextStep) {
    Test "Step: $stepLabel (by $userName)" {
        $body = @{ notes = "Test: $stepLabel"; comment = "By $userName" } | ConvertTo-Json
        $resp = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId/advance" -Method POST -Headers $headers -Body $body
        if ($resp.success -ne $true) {
            Write-Host "    -> ADVANCE FAILED: $($resp.message)" -ForegroundColor Red
            return $false
        }
        $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId" -Headers $hGerant
        $currentStep = $inst.data.currentStepOrder
        $status = $inst.data.status
        Write-Host "    -> Now at step $currentStep | Status=$status" -ForegroundColor DarkGray
        if ($expectedNextStep -eq "Completed") { return $status -eq "Completed" }
        return $currentStep -eq $expectedNextStep
    }
}

AdvanceAndVerify "0->1 Ouverture" $hGerant "Gerant(SuperAdmin)" 1
AdvanceAndVerify "1->2 Demande factures" $hYoussef "Youssef(Fiscaliste)" 2
AdvanceAndVerify "2->3 Collecte" $hGerant "Gerant(ForClient)" 3
AdvanceAndVerify "3->4 Saisie TVA" $hFatima "Fatima(Comptable)" 4
AdvanceAndVerify "4->5 Controle TVA" $hAmina "Amina(Manager)" 5
AdvanceAndVerify "5->6 Validation" $hAmina "Amina(Manager)" 6
AdvanceAndVerify "6->7 Approbation client" $hGerant "Gerant(ForClient)" 7
AdvanceAndVerify "7->8 Teledeclaration" $hYoussef "Youssef(Fiscaliste)" 8
AdvanceAndVerify "8->END Cloture" $hGerant "Gerant(System)" "Completed"

# PHASE 5: Final Verification
Write-Host "`n--- PHASE 5: Final Verification ---" -ForegroundColor Yellow

Test "Workflow instance is Completed" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId" -Headers $hGerant
    Write-Host "    -> Final status: $($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

Test "Declaration status after workflow" {
    $decl = Invoke-RestMethod -Uri "$API/api/declarations/$declId" -Headers $hGerant
    Write-Host "    -> Declaration status: $($decl.data.statut)" -ForegroundColor DarkGray
    $decl.data.statut -in @("Soumise", "Validee", "Completee", "Archivee")
}

# PHASE 6: IS Workflow with different client
Write-Host "`n--- PHASE 6: IS Workflow (Client 10) ---" -ForegroundColor Yellow

$isDeclId = $null; $isWfId = $null

Test "Create IS Declaration for Client 10" {
    $body = @{ clientId = 10; type = "IS"; periode = "2026"; anneeFiscale = 2026 } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:isDeclId = $resp.data.id
    Write-Host "    -> IS declaration ID=$isDeclId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Start IS Workflow (template=2)" {
    $body = @{ declarationId = $isDeclId; clientId = 10; declarationType = "IS"; workflowTemplateId = 2 } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:isWfId = $resp.data.id
    Write-Host "    -> IS workflow instance ID=$isWfId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

# Advance all 9 IS steps
$isUsers = @($hGerant, $hFatima, $hGerant, $hYoussef, $hAmina, $hAmina, $hGerant, $hYoussef, $hGerant)
$isNames = @("Gerant","Fatima","Gerant","Youssef","Amina","Amina","Gerant","Youssef","Gerant")
for ($i = 0; $i -lt 9; $i++) {
    Test "IS Step $i by $($isNames[$i])" {
        $body = @{notes="IS step $i"} | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $isUsers[$i] -Body $body
        Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
        $r.success -eq $true
    }
}

Test "IS Workflow completed" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# PHASE 7: CNSS Workflow (Client 9, by Sara)
Write-Host "`n--- PHASE 7: CNSS Workflow (Client 9) ---" -ForegroundColor Yellow

$cnssDeclId = $null; $cnssWfId = $null

Test "Create CNSS Declaration for Client 9" {
    $body = @{ clientId = 9; type = "CNSS"; periode = "2026-02" } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:cnssDeclId = $resp.data.id
    Write-Host "    -> CNSS declaration ID=$cnssDeclId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Start CNSS Workflow (template=4)" {
    $body = @{ declarationId = $cnssDeclId; clientId = 9; declarationType = "CNSS"; workflowTemplateId = 4 } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:cnssWfId = $resp.data.id
    Write-Host "    -> CNSS workflow ID=$cnssWfId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

# Advance all 8 CNSS steps (0-7)
$cnssUsers = @($hGerant, $hSara, $hGerant, $hSara, $hAmina, $hAmina, $hSara, $hGerant)
$cnssNames = @("Gerant","Sara","Gerant","Sara","Amina","Amina","Sara","Gerant")
for ($i = 0; $i -lt 8; $i++) {
    Test "CNSS Step $i by $($cnssNames[$i])" {
        $body = @{notes="CNSS step $i"} | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$cnssWfId/advance" -Method POST -Headers $cnssUsers[$i] -Body $body
        Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
        $r.success -eq $true
    }
}

Test "CNSS Workflow completed" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$cnssWfId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# PHASE 8: Authorization Tests
Write-Host "`n--- PHASE 8: Authorization Tests ---" -ForegroundColor Yellow

Test "Karim (Assistant) CAN read workflow instances" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?page=1&pageSize=5" -Headers $hKarim
    $r.success -eq $true
}

$authTestDeclId = $null; $authTestWfId = $null
Test "Create declaration for auth test" {
    $body = @{ clientId = 17; type = "TVA"; periode = "2026-04" } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:authTestDeclId = $resp.data.id
    Write-Host "    -> Declaration ID=$authTestDeclId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Start workflow for auth test" {
    $body = @{ declarationId = $authTestDeclId; clientId = 17; declarationType = "TVA"; workflowTemplateId = 1 } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:authTestWfId = $resp.data.id
    Write-Host "    -> Workflow ID=$authTestWfId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Karim (Assistant) CAN advance workflow" {
    $body = @{notes="Karim advances"} | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hKarim -Body $body
    Write-Host "    -> success=$($r.success)" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Unauthenticated request returns 401" {
    try {
        $badH = @{"Content-Type"="application/json"}
        $body = @{notes="anon"} | ConvertTo-Json
        $null = Invoke-WebRequest -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $badH -Body $body -ErrorAction Stop
        $false
    } catch {
        $sc = $_.Exception.Response.StatusCode.value__
        Write-Host "    -> HTTP $sc (expected 401)" -ForegroundColor DarkGray
        $sc -eq 401
    }
}

# PHASE 9: ForceSkip Test
Write-Host "`n--- PHASE 9: ForceSkip Test ---" -ForegroundColor Yellow

# Advance auth test workflow from step 1 to step 6 (step 0 was done by Karim)
for ($i = 1; $i -lt 6; $i++) {
    $body = @{notes="Fast-forward step $i"} | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hGerant -Body $body
}

Test "Workflow at step 6 (Client approval)" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId" -Headers $hGerant
    Write-Host "    -> Step=$($inst.data.currentStepOrder) Name=$($inst.data.currentStepName)" -ForegroundColor DarkGray
    $inst.data.currentStepOrder -eq 6
}

Test "ForceSkip step 6 (non-required)" {
    $body = @{notes="Skip client approval"; forceSkip=$true} | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hGerant -Body $body
    Write-Host "    -> $($r.success)" -ForegroundColor DarkGray
    $r.success -eq $true
}

# Finish remaining steps 7 and 8
for ($i = 7; $i -lt 9; $i++) {
    Test "Finish step $i" {
        $body = @{notes="Finishing step $i"} | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hGerant -Body $body
        Write-Host "    -> $($r.success)" -ForegroundColor DarkGray
        $r.success -eq $true
    }
}

Test "Auth-test workflow completed" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# PHASE 10: Declaration Edit Lock
Write-Host "`n--- PHASE 10: Declaration Edit Lock ---" -ForegroundColor Yellow

Test "Declaration is in locked status after workflow" {
    $decl = Invoke-RestMethod -Uri "$API/api/declarations/$declId" -Headers $hGerant
    Write-Host "    -> Status: $($decl.data.statut)" -ForegroundColor DarkGray
    $decl.data.statut -in @("Soumise", "Validee", "Archivee")
}

# PHASE 11: Query & Filter Tests
Write-Host "`n--- PHASE 11: Query & Filter Tests ---" -ForegroundColor Yellow

Test "Filter workflow instances by client=1" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?clientId=1" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) instances for client 1" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Filter by status=Completed" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?status=Completed" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) completed" -ForegroundColor DarkGray
    $r.success -eq $true -and $r.data.totalCount -ge 3
}

Test "Get current step details" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId/current-step" -Headers $hGerant
    Write-Host "    -> $($r.data)" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "List workflow templates" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/templates?page=1&pageSize=50" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) templates" -ForegroundColor DarkGray
    $r.data.totalCount -ge 9
}

# SUMMARY
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
Write-Host "========================================`n" -ForegroundColor Cyan
'''

with open(path, 'w', encoding='ascii', errors='replace') as f:
    f.write(content)

print(f"Written {os.path.getsize(path)} bytes to {path}")
