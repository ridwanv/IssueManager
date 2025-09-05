# PowerShell script to take screenshot
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$screenBounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$screenshot = New-Object Drawing.Bitmap $screenBounds.Width, $screenBounds.Height
$graphics = [Drawing.Graphics]::FromImage($screenshot)
$graphics.CopyFromScreen(0, 0, 0, 0, $screenBounds.Size)

# Save screenshot
$filePath = Join-Path $PSScriptRoot "dashboard-screenshot.png"
$screenshot.Save($filePath)

$graphics.Dispose()
$screenshot.Dispose()

Write-Host "Screenshot saved to: $filePath"