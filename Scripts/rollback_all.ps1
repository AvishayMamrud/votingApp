$services = @("Users", "Surveys", "Votes", "Results")

Write-Host "Running Liquibase rollback for all services..."

foreach ($service in $services) {
    $path = "..\${service}Service"
    Write-Host "Rolling back last changeset for ${service}Service..."
    Push-Location $path
    liquibase rollbackCount 1
    Pop-Location
}

Write-Host "`n✅ All rollbacks complete."