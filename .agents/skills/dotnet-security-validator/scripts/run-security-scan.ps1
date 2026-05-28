Write-Host "Running .NET security validation..."

dotnet build

Write-Host "Checking vulnerable packages..."
dotnet list package --vulnerable --include-transitive

Write-Host "Searching for exposed secrets..."

Get-ChildItem -Recurse -Include *.cs,*.json |
Select-String -Pattern "password|secret|apikey|token|connectionstring"

Write-Host "Checking insecure configuration..."

Get-ChildItem -Recurse -Include *.json |
Select-String -Pattern "AllowAnyOrigin|DeveloperExceptionPage"

Write-Host "Security validation completed."
