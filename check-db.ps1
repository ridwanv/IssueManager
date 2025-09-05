# PowerShell script to check SQLite database
$dbPath = "src\Server.UI\BlazorDashboardDb.db"

if (Test-Path $dbPath) {
    Write-Host "Database file exists at: $dbPath"
    
    # Get file info
    $fileInfo = Get-Item $dbPath
    Write-Host "File size: $($fileInfo.Length) bytes"
    Write-Host "Last modified: $($fileInfo.LastWriteTime)"
} else {
    Write-Host "Database file does not exist at: $dbPath"
}

# List all .db files in project
Write-Host "`nAll .db files in project:"
Get-ChildItem -Recurse -Filter "*.db" | ForEach-Object {
    Write-Host "  $($_.FullName) - Size: $($_.Length) bytes"
}