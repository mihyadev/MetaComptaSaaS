$ErrorActionPreference = 'Stop'
$labels = @('Janvier','Fevrier','Mars','Avril','Mai','Juin','Juillet','Aout','Septembre','Octobre','Novembre','Decembre')
$ends = @(31,29,31,30,31,30,31,31,30,31,30,31)
$periodObjs = @()
for ($m = 1; $m -le 12; $m++) {
    $periodObjs += [ordered]@{
        label = $labels[$m - 1]
        start = ('{0:D2}-01' -f $m)
        end = ('{0:D2}-{1:D2}' -f $m, $ends[$m - 1])
        startMonth = $m
        startDay = 1
        endMonth = $m
        endDay = $ends[$m - 1]
    }
}
$cfgObj = [ordered]@{
    initTrigger = 'both'
    allowManualInit = $true
    periodTemplate = 'Mensuelle'
    periodFrequency = 'Mensuelle'
    periods = $periodObjs
}
$cfg = ($cfgObj | ConvertTo-Json -Compress -Depth 6)
# Use French accents in labels for UI parity
$cfg = $cfg.Replace('"Fevrier"','"Février"').Replace('"Aout"','"Août"')

# Prefer direct parameterized update via .NET SqlClient
Add-Type -AssemblyName System.Data
$cs = 'Server=localhost,58080;Database=MetaComptaSASDb_Dev;User Id=sa;Password=RS2iRS2i;TrustServerCertificate=True;'
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
UPDATE wst
SET ActionConfigJson = @cfg,
    Description = N'Ouverture période — Fin/échéance = Modèle de périodes Mensuel (dernier jour du mois), Action à la fin = clôture',
    CronDescription = CASE
        WHEN CronExpression IS NOT NULL AND LTRIM(RTRIM(CronExpression)) <> ''
        THEN N'Le 1er de chaque mois — ouvre période précédente ; clôture à Fin/échéance du modèle'
        ELSE CronDescription END
FROM WorkflowStepTemplates wst
INNER JOIN WorkflowTemplates wt ON wt.Id = wst.WorkflowTemplateId
WHERE wst.IsDeleted = 0
  AND wst.StepType = 0
  AND wt.DeclarationType IN (1, 2, 4); -- TVA, IS, CNSS

SELECT wt.Id AS TemplateId, wt.DeclarationType, LEN(wst.ActionConfigJson) AS CfgLen,
       CASE WHEN wst.ActionConfigJson LIKE '%periods%' THEN 1 ELSE 0 END AS HasPeriods,
       CASE WHEN wst.ActionConfigJson LIKE '%windowEndDay%' THEN 1 ELSE 0 END AS HasWindowEnd
FROM WorkflowStepTemplates wst
INNER JOIN WorkflowTemplates wt ON wt.Id = wst.WorkflowTemplateId
WHERE wst.IsDeleted = 0 AND wst.StepType = 0
  AND wt.DeclarationType IN (1, 2, 4);
"@
$p = $cmd.Parameters.Add('@cfg', [System.Data.SqlDbType]::NVarChar, -1)
$p.Value = $cfg
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host ("Template {0} {1}: len={2} periods={3} windowEnd={4}" -f $reader[0], $reader[1], $reader[2], $reader[3], $reader[4])
}
$reader.Close()
$conn.Close()
Write-Host 'OK patched Init period configs.'
