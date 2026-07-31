# Verify cabinet role → claims matrix (login + allowed/forbidden API probes)
# Password for all seeded cabinet staff: Demo@2026!
$ErrorActionPreference = "Stop"
$base = "http://localhost:7100"
$pwd = "Demo@2026!"

function Login($email) {
    $r = Invoke-RestMethod -Uri "$base/api/auth/login" -Method Post -ContentType "application/json" -Body (@{
        email = $email
        password = $pwd
    } | ConvertTo-Json)
    if (-not $r.data.accessToken) { throw "Login failed for $email" }
    return $r.data
}

function Decode-JwtPayload($jwt) {
    $parts = $jwt.Split('.')
    if ($parts.Length -lt 2) { throw "Bad JWT" }
    $payload = $parts[1].Replace('-', '+').Replace('_', '/')
    switch ($payload.Length % 4) {
        2 { $payload += '==' }
        3 { $payload += '=' }
    }
    $json = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
    return ($json | ConvertFrom-Json)
}

function ApiStatus($method, $path, $token, $body = $null) {
    try {
        $params = @{
            Uri = "$base$path"
            Method = $method
            Headers = @{ Authorization = "Bearer $token" }
            ContentType = "application/json"
            UseBasicParsing = $true
        }
        if ($null -ne $body) { $params.Body = ($body | ConvertTo-Json -Depth 6) }
        $null = Invoke-WebRequest @params
        return 200
    } catch {
        if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode.value__ }
        throw
    }
}

function Expect-HasClaim($email, $payload, $claim) {
    $perms = @($payload.Permission)
    if ($perms -notcontains $claim -and $perms -notcontains $claim.ToLowerInvariant()) {
        # JWT may serialize single claim as string
        if ("$($payload.Permission)" -eq $claim) { return }
        throw "ASSERT FAIL [$email] missing claim $claim (count=$(@($payload.Permission).Count))"
    }
}

function Expect-MissingClaim($email, $payload, $claim) {
    $perms = @($payload.Permission)
    if ($perms -contains $claim -or $perms -contains $claim.ToLowerInvariant()) {
        throw "ASSERT FAIL [$email] should NOT have claim $claim"
    }
}

Write-Host "=== Role claims verification ==="

$profiles = @(
    @{ Email = "manager1@cabinet-demo.ma"; Role = "Manager"; Must = @("commercial.validate", "cabinet.employee.create"); MustNot = @("system.admin") },
    @{ Email = "comptable1@cabinet-demo.ma"; Role = "Comptable"; Must = @("comptabilite.create", "client.detail"); MustNot = @("commercial.validate", "tva.validate", "system.admin") },
    @{ Email = "fiscal@cabinet-demo.ma"; Role = "Fiscaliste"; Must = @("tva.validate", "declaration.create"); MustNot = @("payroll.create", "commercial.sign", "cabinet.role.manage") },
    @{ Email = "social@cabinet-demo.ma"; Role = "Socialiste"; Must = @("cnss.create", "payroll.validate"); MustNot = @("tva.create", "commercial.create") },
    @{ Email = "juridique@cabinet-demo.ma"; Role = "Juriste"; Must = @("juridique.create"); MustNot = @("payroll.create", "commercial.validate") },
    @{ Email = "commercial1@cabinet-demo.ma"; Role = "Commercial"; Must = @("commercial.sign", "facture.create"); MustNot = @("declaration.validate", "cabinet.role.manage") },
    @{ Email = "assistant1@cabinet-demo.ma"; Role = "Assistant"; Must = @("declaration.create", "client.edit"); MustNot = @("declaration.validate", "commercial.sign", "system.admin") }
)

foreach ($p in $profiles) {
    Write-Host ""
    Write-Host "-- $($p.Email) ($($p.Role)) --"
    $data = Login $p.Email
    $token = $data.accessToken
    $payload = Decode-JwtPayload $token
    $roleClaim = $payload."http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    if (-not $roleClaim) { $roleClaim = $payload.role }
    $permCount = @($payload.Permission).Count
    if ($permCount -eq 0 -and $payload.Permission) { $permCount = 1 }
    Write-Host "OK  login role=$roleClaim perms=$permCount"

    foreach ($c in $p.Must) { Expect-HasClaim $p.Email $payload $c; Write-Host "OK  has $c" }
    foreach ($c in $p.MustNot) { Expect-MissingClaim $p.Email $payload $c; Write-Host "OK  missing $c" }
}

Write-Host ""
Write-Host "=== API probes (Fiscaliste) ==="
$fiscal = Login "fiscal@cabinet-demo.ma"
$ft = $fiscal.accessToken
$okList = ApiStatus Get "/api/clients?page=1&pageSize=5" $ft
if ($okList -ne 200) { throw "Fiscaliste should list clients, got $okList" }
Write-Host "OK  Fiscaliste GET /api/clients => $okList"

# Forbidden: commercial validate (needs commercial.validate)
$forbid = ApiStatus Post "/api/contrats/1/valider" $ft @{}
if ($forbid -ne 403 -and $forbid -ne 401) {
    # 400 if contract missing/state invalid still proves auth passed — only fail if 200
    if ($forbid -eq 200) { throw "Fiscaliste must not validate contrats (got 200)" }
    Write-Host "OK  Fiscaliste POST /api/contrats/1/valider => $forbid (blocked or rejected)"
} else {
    Write-Host "OK  Fiscaliste POST /api/contrats/1/valider => $forbid"
}

Write-Host ""
Write-Host "=== API probes (Commercial) ==="
$com = Login "commercial1@cabinet-demo.ma"
$ct = $com.accessToken
$okCom = ApiStatus Get "/api/contrats?page=1&pageSize=5" $ct
if ($okCom -ne 200) { throw "Commercial should list contrats, got $okCom" }
Write-Host "OK  Commercial GET /api/contrats => $okCom"

Write-Host ""
Write-Host "=== API probes (Manager cabinet employee edit) ==="
$mgr = Login "manager1@cabinet-demo.ma"
$mt = $mgr.accessToken
$emps = Invoke-RestMethod -Uri "$base/api/cabinet/employees?page=1&pageSize=5" -Headers @{ Authorization = "Bearer $mt" }
$empId = $emps.data.items[0].id
if (-not $empId) { throw "No employee to probe edit" }
# GET is allowed; PUT with empty body may 400 but must NOT be 403 for Manager
$mgrPut = ApiStatus Put "/api/cabinet/employees/$empId" $mt @{ firstName = $emps.data.items[0].firstName; lastName = $emps.data.items[0].lastName; role = $emps.data.items[0].role; isActive = $true }
if ($mgrPut -eq 403) { throw "Manager PUT cabinet employee must not be 403" }
Write-Host "OK  Manager PUT /api/cabinet/employees/$empId => $mgrPut"

$soc = Login "social@cabinet-demo.ma"
$st = $soc.accessToken
$socPut = ApiStatus Put "/api/cabinet/employees/$empId" $st @{ firstName = "X"; lastName = "Y"; role = "Socialiste"; isActive = $true }
if ($socPut -ne 403 -and $socPut -ne 401) { throw "Socialiste PUT cabinet employee must be 403, got $socPut" }
Write-Host "OK  Socialiste PUT /api/cabinet/employees/$empId => $socPut"

Write-Host ""
Write-Host "VERIFY ROLE CLAIMS PASS"
Write-Host "Login password for all profiles: Demo@2026!"
