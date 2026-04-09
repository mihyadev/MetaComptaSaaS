# ============================================================
# MetaComptaSAS — FULL WORKFLOW E2E TEST
# Tests: Create declaration → Start Workflow → Advance all 9 steps
# Tests with different users: gerant, fiscaliste, comptable, manager
# ============================================================

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
        }
        else {
            $script:fail++
            Write-Host "  [FAIL] $label — returned falsy" -ForegroundColor Red
        }
    }
    catch {
        $script:fail++
        Write-Host "  [FAIL] $label — $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Login($email, $password) {
    $body = @{email = $email; password = $password } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/auth/login" -Method POST -Body $body -ContentType "application/json"
    return @{
        "Authorization" = "Bearer $($resp.data.accessToken)"
        "Content-Type"  = "application/json"
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " METACOMPTASAS — WORKFLOW E2E TEST" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ──────────────────────────────────────────────────────────
# PHASE 1: Login with ALL users
# ──────────────────────────────────────────────────────────
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
Test "Login as Sara El Idrissi (Comptable Juridique)" {
    $script:hSara = Login "sara.elidrissi@cabinet-demo.ma" "Test@2026!"
    $script:hSara -ne $null
}

# ──────────────────────────────────────────────────────────
# PHASE 2: Create a NEW TVA Declaration for Client 1
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 2: Create TVA Declaration ---" -ForegroundColor Yellow

$declId = $null
Test "Create TVA Declaration (March 2026, Client 1)" {
    $body = @{
        clientId = 1
        type     = "TVA"
        periode  = "2026-03"
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:declId = $resp.data.id
    Write-Host "    -> Created declaration ID=$script:declId" -ForegroundColor DarkGray
    $resp.success -eq $true -and $script:declId -gt 0
}

$declDetail = $null
Test "Read back declaration — status should be Brouillon" {
    $script:declDetail = Invoke-RestMethod -Uri "$API/api/declarations/$script:declId" -Headers $hGerant
    $script:declDetail.data.statut -eq "Brouillon"
}

# ──────────────────────────────────────────────────────────
# PHASE 3: Start Workflow on the Declaration
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 3: Start TVA Workflow ---" -ForegroundColor Yellow

$wfInstanceId = $null
Test "Start TVA Workflow (template=1) on declaration" {
    $body = @{
        declarationId      = $declId
        clientId           = 1
        declarationType    = "TVA"
        workflowTemplateId = 1
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:wfInstanceId = $resp.data.id
    Write-Host "    -> Workflow instance ID=$script:wfInstanceId" -ForegroundColor DarkGray
    $resp.success -eq $true -and $script:wfInstanceId -gt 0
}

Test "Workflow instance is Active and at step 0" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId" -Headers $hGerant
    $inst.data.status -eq "Active" -and $inst.data.currentStepOrder -eq 0
}

# ──────────────────────────────────────────────────────────
# PHASE 4: Advance through ALL 9 steps
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 4: Advance Workflow (9 steps) ---" -ForegroundColor Yellow

# Helper to advance and verify
function AdvanceAndVerify($stepLabel, $headers, $userName, $expectedNextStep) {
    Test "Step: $stepLabel (by $userName)" {
        $body = @{ notes = "Test auto: $stepLabel"; comment = "Processed by $userName" } | ConvertTo-Json
        $resp = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId/advance" -Method POST -Headers $headers -Body $body
        if ($resp.success -ne $true) {
            Write-Host "    -> ADVANCE FAILED: $($resp.message)" -ForegroundColor Red
            return $false
        }
        # Check state after advance
        $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId" -Headers $hGerant
        $currentStep = $inst.data.currentStepOrder
        $status = $inst.data.status
        Write-Host "    -> Now at step $currentStep ($($inst.data.currentStepName)) | Status=$status" -ForegroundColor DarkGray

        if ($expectedNextStep -eq "Completed") {
            return $status -eq "Completed"
        }
        return $currentStep -eq $expectedNextStep
    }
}

# Step 0 → 1: Ouverture période TVA (AutoSystem → Employee should advance)
AdvanceAndVerify "0→1 Ouverture période TVA" $hGerant "Gerant(SuperAdmin)" 1

# Step 1 → 2: Demande factures (Employee → Client)
AdvanceAndVerify "1→2 Demande factures & pièces" $hYoussef "Youssef(Fiscaliste)" 2

# Step 2 → 3: Collecte & numérisation (Client → Department)
AdvanceAndVerify "2→3 Collecte & numérisation" $hGerant "Gerant(SuperAdmin)" 3

# Step 3 → 4: Saisie & rapprochement TVA (Department → Role:Manager)
AdvanceAndVerify "3→4 Saisie & rapprochement TVA" $hFatima "Fatima(Comptable)" 4

# Step 4 → 5: Contrôle TVA (Role:Manager → Role:Manager)
AdvanceAndVerify "4→5 Contrôle TVA" $hAmina "Amina(Manager)" 5

# Step 5 → 6: Validation chef comptable (Role:Manager → Client)
AdvanceAndVerify "5→6 Validation chef comptable" $hAmina "Amina(Manager)" 6

# Step 6 → 7: Approbation client (Client, non-required → Employee)
AdvanceAndVerify "6→7 Approbation client" $hGerant "Gerant(SuperAdmin)" 7

# Step 7 → 8: Télédéclaration DGI (Employee → AutoSystem)
AdvanceAndVerify "7→8 Télédéclaration DGI" $hYoussef "Youssef(Fiscaliste)" 8

# Step 8 → Completed: Clôture & archivage (AutoSystem → end)
AdvanceAndVerify "8→END Clôture & archivage" $hGerant "Gerant(SuperAdmin)" "Completed"

# ──────────────────────────────────────────────────────────
# PHASE 5: Verify final state
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 5: Final Verification ---" -ForegroundColor Yellow

Test "Workflow instance is Completed" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId" -Headers $hGerant
    Write-Host "    -> Final status: $($inst.data.status) | StepOrder: $($inst.data.currentStepOrder)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

Test "Declaration status after full workflow" {
    $decl = Invoke-RestMethod -Uri "$API/api/declarations/$declId" -Headers $hGerant
    Write-Host "    -> Declaration status: $($decl.data.statut)" -ForegroundColor DarkGray
    # After full workflow, should be Soumise or beyond
    $decl.data.statut -in @("Soumise", "Validee", "Completee", "Archivee")
}

# ──────────────────────────────────────────────────────────
# PHASE 6: Test IS Workflow with different client
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 6: IS Workflow (Client 10) ---" -ForegroundColor Yellow

$isDeclId = $null
$isWfId = $null

Test "Create IS Declaration for Client 10" {
    $body = @{
        clientId     = 10
        type         = "IS"
        periode      = "2026"
        anneeFiscale = 2026
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:isDeclId = $resp.data.id
    Write-Host "    -> IS declaration ID=$isDeclId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Start IS Workflow (template=2)" {
    $body = @{
        declarationId      = $isDeclId
        clientId           = 10
        declarationType    = "IS"
        workflowTemplateId = 2
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:isWfId = $resp.data.id
    Write-Host "    -> IS workflow instance ID=$isWfId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

# Advance all IS steps (9 steps)
Test "Advance IS Workflow: Step 0 (Init)" {
    $body = @{notes = "IS auto-test step 0" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hGerant -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 1 (Demande docs)" {
    $body = @{notes = "IS step 1" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hFatima -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 2 (Collecte)" {
    $body = @{notes = "IS step 2" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hGerant -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 3 (Analyse)" {
    $body = @{notes = "IS step 3" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hYoussef -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 4 (Controle)" {
    $body = @{notes = "IS step 4" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hAmina -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 5 (Validation)" {
    $body = @{notes = "IS step 5" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hAmina -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 6 (Client approval)" {
    $body = @{notes = "IS step 6" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hGerant -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 7 (Soumission)" {
    $body = @{notes = "IS step 7" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hYoussef -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "Advance IS Workflow: Step 8 (Cloture)" {
    $body = @{notes = "IS step 8" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId/advance" -Method POST -Headers $hGerant -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}
Test "IS Workflow final state" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$isWfId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status) Step=$($inst.data.currentStepOrder)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ──────────────────────────────────────────────────────────
# PHASE 7: Test CNSS Workflow with Client 9, advanced by Sara
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 7: CNSS Workflow (Client 9, by Sara) ---" -ForegroundColor Yellow

$cnssDeclId = $null
$cnssWfId = $null

Test "Create CNSS Declaration for Client 9" {
    $body = @{
        clientId = 9
        type     = "CNSS"
        periode  = "2026-02"
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:cnssDeclId = $resp.data.id
    Write-Host "    -> CNSS declaration ID=$cnssDeclId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Start CNSS Workflow (template=4)" {
    $body = @{
        declarationId      = $cnssDeclId
        clientId           = 9
        declarationType    = "CNSS"
        workflowTemplateId = 4
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:cnssWfId = $resp.data.id
    Write-Host "    -> CNSS workflow ID=$cnssWfId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

# Advance all 8 CNSS steps (0-7) using Sara
for ($i = 0; $i -lt 8; $i++) {
    $user = switch ($i) {
        0 { $hGerant }    # Init - AutoSystem
        1 { $hSara }      # Demande état personnel - Sara(Comptable)
        2 { $hGerant }    # Collecte données paie - Client
        3 { $hSara }      # Calcul cotisations CNSS - Department
        4 { $hAmina }     # Contrôle bordereau - Manager
        5 { $hAmina }     # Validation & signature - Manager
        6 { $hSara }      # Télédéclaration Damancom - Employee
        7 { $hGerant }    # Clôture & archivage - AutoSystem
    }
    $uName = switch ($i) { 0 { "Gerant" } 1 { "Sara" } 2 { "Gerant" } 3 { "Sara" } 4 { "Amina" } 5 { "Amina" } 6 { "Sara" } 7 { "Gerant" } }
    Test "CNSS Step $i by $uName" {
        $body = @{notes = "CNSS step $i by $uName" } | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$cnssWfId/advance" -Method POST -Headers $user -Body $body
        Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
        $r.success -eq $true
    }
}

Test "CNSS Workflow completed" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$cnssWfId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ──────────────────────────────────────────────────────────
# PHASE 8: Test Authorization — All employees CAN advance, unauthenticated CANNOT
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 8: Authorization Tests ---" -ForegroundColor Yellow

Test "Karim (Assistant) CAN read workflow instances" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?page=1&pageSize=5" -Headers $hKarim
    $r.success -eq $true
}

$authTestDeclId = $null
$authTestWfId = $null
Test "Create test declaration for auth test" {
    $body = @{
        clientId = 17
        type     = "TVA"
        periode  = "2026-02"
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/declarations" -Method POST -Headers $hGerant -Body $body
    $script:authTestDeclId = $resp.data.id
    Write-Host "    -> Declaration ID=$authTestDeclId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Start workflow for auth test" {
    $body = @{
        declarationId      = $authTestDeclId
        clientId           = 17
        declarationType    = "TVA"
        workflowTemplateId = 1
    } | ConvertTo-Json
    $resp = Invoke-RestMethod -Uri "$API/api/workflow/start" -Method POST -Headers $hGerant -Body $body
    $script:authTestWfId = $resp.data.id
    Write-Host "    -> Workflow ID=$authTestWfId" -ForegroundColor DarkGray
    $resp.success -eq $true
}

Test "Karim (Assistant) CAN advance workflow" {
    $body = @{notes = "Karim advances step 0" } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hKarim -Body $body
    Write-Host "    -> success=$($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Unauthenticated request CANNOT advance workflow (401)" {
    try {
        $badHeaders = @{"Content-Type" = "application/json" }
        $body = @{notes = "anon" } | ConvertTo-Json
        $r = Invoke-WebRequest -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $badHeaders -Body $body -ErrorAction Stop
        $false  # Should not succeed
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "    -> Got HTTP $statusCode (expected 401)" -ForegroundColor DarkGray
        $statusCode -eq 401
    }
}

# ──────────────────────────────────────────────────────────
# PHASE 9: Test ForceSkip on non-required steps
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 9: ForceSkip Test ---" -ForegroundColor Yellow

# Advance auth test workflow to step 6 (Client approval - non-required)
# Step 0 was already advanced by Karim in Phase 8, so we start from step 1
for ($i = 1; $i -lt 6; $i++) {
    $body = @{notes = "Fast-forward step $i" } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hGerant -Body $body
}

Test "Workflow at step 6 (Client approval, non-required)" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId" -Headers $hGerant
    Write-Host "    -> Step=$($inst.data.currentStepOrder) | StepName=$($inst.data.currentStepName)" -ForegroundColor DarkGray
    $inst.data.currentStepOrder -eq 6
}

Test "ForceSkip step 6 (non-required)" {
    $body = @{notes = "Skipping client approval"; forceSkip = $true } | ConvertTo-Json
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hGerant -Body $body
    Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
    $r.success -eq $true
}

# Finish remaining steps
for ($i = 7; $i -lt 9; $i++) {
    Test "Finish step $i" {
        $body = @{notes = "Finishing step $i" } | ConvertTo-Json
        $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId/advance" -Method POST -Headers $hGerant -Body $body
        Write-Host "    -> $($r.success) | $($r.message)" -ForegroundColor DarkGray
        $r.success -eq $true
    }
}

Test "Auth-test workflow completed after ForceSkip" {
    $inst = Invoke-RestMethod -Uri "$API/api/workflow/instances/$authTestWfId" -Headers $hGerant
    Write-Host "    -> Status=$($inst.data.status)" -ForegroundColor DarkGray
    $inst.data.status -eq "Completed"
}

# ──────────────────────────────────────────────────────────
# PHASE 10: Test Edit Lock on completed declaration
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 10: Declaration Edit Lock ---" -ForegroundColor Yellow

Test "Cannot edit TVA of Soumise/completed declaration" {
    try {
        $r = Invoke-WebRequest -Uri "$API/api/declarations/$declId" -Method PUT -Headers $hGerant -Body '{"statut":"Brouillon"}' -ErrorAction Stop
        # Check if declaration is in a locked status
        $decl = Invoke-RestMethod -Uri "$API/api/declarations/$declId" -Headers $hGerant
        Write-Host "    -> Declaration status: $($decl.data.statut)" -ForegroundColor DarkGray
        $decl.data.statut -in @("Soumise", "Validee", "Archivee")
    }
    catch {
        Write-Host "    -> Edit blocked (expected)" -ForegroundColor DarkGray
        $true
    }
}

# ──────────────────────────────────────────────────────────
# PHASE 11: Test Workflow Instance Query Filters
# ──────────────────────────────────────────────────────────
Write-Host "`n--- PHASE 11: Query & Filter Tests ---" -ForegroundColor Yellow

Test "Get workflow instances filtered by client=1" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?clientId=1" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) instances for client 1" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Get workflow instances filtered by status=Completed" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances?status=Completed" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) completed instances" -ForegroundColor DarkGray
    $r.success -eq $true -and $r.data.totalCount -ge 3
}

Test "Get current step details" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/instances/$wfInstanceId/current-step" -Headers $hGerant
    Write-Host "    -> Current step: $($r.data)" -ForegroundColor DarkGray
    $r.success -eq $true
}

Test "Get workflow templates list" {
    $r = Invoke-RestMethod -Uri "$API/api/workflow/templates?page=1&pageSize=50" -Headers $hGerant
    Write-Host "    -> $($r.data.totalCount) templates" -ForegroundColor DarkGray
    $r.data.totalCount -ge 9
}

# ──────────────────────────────────────────────────────────
# SUMMARY
# ──────────────────────────────────────────────────────────
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " TEST RESULTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Total:  $total" -ForegroundColor White
Write-Host "  Passed: $pass" -ForegroundColor Green
Write-Host "  Failed: $fail" -ForegroundColor $(if ($fail -gt 0) { "Red" }else { "Green" })
Write-Host "========================================`n" -ForegroundColor Cyan
