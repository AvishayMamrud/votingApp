$services = @("Users", "Surveys", "Votes", "Results")

Write-Host "Running Liquibase migrations for all services..."

foreach ($service in $services) {
    $path = "..\Services\${service}Service\db"
    Write-Host "Migrating ${service}Service..."
    Push-Location $path
    liquibase update
    Pop-Location
}

Write-Host "`n✅ All migrations complete."