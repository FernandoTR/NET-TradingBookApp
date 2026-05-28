Write-Host "Analyzing dependencies..."

$packages = dotnet list package --vulnerable --include-transitive

if ($packages -match "Critical") {
    Write-Host "Critical vulnerabilities detected!"
}

if ($packages -match "High") {
    Write-Host "High severity vulnerabilities detected!"
}

Write-Host "Dependency analysis completed."
