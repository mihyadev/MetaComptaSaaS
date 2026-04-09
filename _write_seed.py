import pathlib

BASE = pathlib.Path(r"c:\Users\delli\OneDrive\Bureau\MetaComptaSAS")

script = r'''
# Seed 5 cabinet employees with different profiles
$loginBody = '{"email":"gerant@cabinet-demo.ma","password":"Gerant@2026!"}'
$response = Invoke-RestMethod -Uri "http://localhost:7100/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $response.data.accessToken
Write-Host "Token: $($token.Substring(0,30))..."

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

# --- Employee 1: Comptable ---
$emp1 = @{
    firstName = "Fatima"
    lastName = "Benali"
    email = "fatima.benali@cabinet-demo.ma"
    password = "Test@2026!"
    cin = "BK456789"
    telephone = "+212 661 234 567"
    dateNaissance = "1990-03-15"
    lieuNaissance = "Casablanca"
    adresse = "123 Rue Hassan II, Casablanca"
    sexe = "Femme"
    situationFamiliale = "Célibataire"
    poste = "Comptable Senior"
    departement = "Comptabilité"
    typeContrat = "CDI"
    dateEmbauche = "2022-01-10"
    numeroCNSS = "1234567890"
    role = "Comptable"
    permissions = @("client.view","client.list","client.detail","declaration.view","declaration.create","declaration.edit","tva.create","tva.edit","tva.detail","is.create","is.edit","is.detail","employee.view","employee.list","document.view","document.upload","task.view","task.edit","comptabilite.create","comptabilite.edit","comptabilite.detail")
} | ConvertTo-Json
try {
    $r = Invoke-RestMethod -Uri "http://localhost:7100/api/cabinet/employees" -Method POST -Headers $headers -Body $emp1
    Write-Host "OK: Fatima Benali (Comptable) - $($r.data.id)"
} catch { Write-Host "ERR Fatima: $($_.Exception.Message)" }

# --- Employee 2: Fiscaliste ---
$emp2 = @{
    firstName = "Youssef"
    lastName = "Tazi"
    email = "youssef.tazi@cabinet-demo.ma"
    password = "Test@2026!"
    cin = "BH789012"
    telephone = "+212 662 345 678"
    dateNaissance = "1988-07-22"
    lieuNaissance = "Rabat"
    adresse = "45 Ave Mohammed V, Rabat"
    sexe = "Homme"
    situationFamiliale = "Marié(e)"
    poste = "Expert Fiscal"
    departement = "Fiscal"
    typeContrat = "CDI"
    dateEmbauche = "2021-06-01"
    numeroCNSS = "9876543210"
    role = "Fiscaliste"
    permissions = @("client.view","client.list","client.detail","declaration.view","declaration.create","declaration.edit","declaration.validate","declaration.submit","tva.create","tva.edit","tva.detail","tva.validate","tva.submit","is.create","is.edit","is.detail","is.validate","ir.create","ir.edit","ir.detail","ir.validate","cnss.create","cnss.edit","cnss.detail","cnss.validate","document.view","document.upload","task.view","task.edit","task.create","workflow.view","workflow.advance")
} | ConvertTo-Json
try {
    $r = Invoke-RestMethod -Uri "http://localhost:7100/api/cabinet/employees" -Method POST -Headers $headers -Body $emp2
    Write-Host "OK: Youssef Tazi (Fiscaliste) - $($r.data.id)"
} catch { Write-Host "ERR Youssef: $($_.Exception.Message)" }

# --- Employee 3: Manager ---
$emp3 = @{
    firstName = "Amina"
    lastName = "Rachidi"
    email = "amina.rachidi@cabinet-demo.ma"
    password = "Test@2026!"
    cin = "BE345678"
    telephone = "+212 663 456 789"
    dateNaissance = "1985-11-30"
    lieuNaissance = "Fès"
    adresse = "78 Blvd Zerktouni, Casablanca"
    sexe = "Femme"
    situationFamiliale = "Marié(e)"
    poste = "Chef d'équipe Social"
    departement = "Social"
    typeContrat = "CDI"
    dateEmbauche = "2019-03-15"
    numeroCNSS = "5678901234"
    role = "Manager"
    permissions = @("client.view","client.create","client.edit","client.list","client.detail","declaration.view","declaration.create","declaration.edit","declaration.validate","declaration.submit","employee.view","employee.create","employee.edit","employee.list","employee.detail","payroll.view","payroll.create","payroll.edit","payroll.validate","payroll.process","payroll.detail","document.view","document.upload","document.edit","document.validate","task.view","task.create","task.edit","task.assign","task.detail","workflow.view","workflow.advance","workflow.validate","workflow.reassign","workflow.monitor","report.view","report.export","cnss.create","cnss.edit","cnss.detail","cnss.validate","ir.create","ir.edit","ir.detail","ir.validate")
} | ConvertTo-Json
try {
    $r = Invoke-RestMethod -Uri "http://localhost:7100/api/cabinet/employees" -Method POST -Headers $headers -Body $emp3
    Write-Host "OK: Amina Rachidi (Manager) - $($r.data.id)"
} catch { Write-Host "ERR Amina: $($_.Exception.Message)" }

# --- Employee 4: Assistant ---
$emp4 = @{
    firstName = "Karim"
    lastName = "Ouazzani"
    email = "karim.ouazzani@cabinet-demo.ma"
    password = "Test@2026!"
    cin = "BA901234"
    telephone = "+212 664 567 890"
    dateNaissance = "1995-05-10"
    lieuNaissance = "Meknès"
    adresse = "12 Rue Allal Ben Abdellah, Meknès"
    sexe = "Homme"
    situationFamiliale = "Célibataire"
    poste = "Assistant Administratif"
    departement = "Support"
    typeContrat = "CDD"
    dateEmbauche = "2024-01-15"
    numeroCNSS = "1122334455"
    role = "Assistant"
    permissions = @("client.view","client.list","declaration.view","document.view","document.upload","task.view","task.edit","task.detail","employee.view","employee.list")
} | ConvertTo-Json
try {
    $r = Invoke-RestMethod -Uri "http://localhost:7100/api/cabinet/employees" -Method POST -Headers $headers -Body $emp4
    Write-Host "OK: Karim Ouazzani (Assistant) - $($r.data.id)"
} catch { Write-Host "ERR Karim: $($_.Exception.Message)" }

# --- Employee 5: Comptable Juridique ---
$emp5 = @{
    firstName = "Sara"
    lastName = "El Idrissi"
    email = "sara.elidrissi@cabinet-demo.ma"
    password = "Test@2026!"
    cin = "BJ567890"
    telephone = "+212 665 678 901"
    dateNaissance = "1992-09-18"
    lieuNaissance = "Tanger"
    adresse = "56 Ave des FAR, Tanger"
    sexe = "Femme"
    situationFamiliale = "Divorcé(e)"
    poste = "Juriste Comptable"
    departement = "Juridique"
    typeContrat = "CDI"
    dateEmbauche = "2023-04-01"
    numeroCNSS = "6677889900"
    role = "Comptable"
    permissions = @("client.view","client.list","client.detail","declaration.view","declaration.create","declaration.edit","document.view","document.upload","document.edit","task.view","task.edit","task.detail","juridique.create","juridique.edit","juridique.detail","comptabilite.create","comptabilite.edit","comptabilite.detail","employee.view")
} | ConvertTo-Json
try {
    $r = Invoke-RestMethod -Uri "http://localhost:7100/api/cabinet/employees" -Method POST -Headers $headers -Body $emp5
    Write-Host "OK: Sara El Idrissi (Comptable Juridique) - $($r.data.id)"
} catch { Write-Host "ERR Sara: $($_.Exception.Message)" }

Write-Host "`n=== Seeding complete ==="
'''

(BASE / "_seed_employees.ps1").write_text(script, encoding="utf-8")
print("OK: _seed_employees.ps1 written")
