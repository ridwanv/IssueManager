@echo offStart-Process chrome.exe "https://localhost:44375" -Wait
timeout 5
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
 = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
 = New-Object Drawing.Bitmap .Width, .Height
 = [Drawing.Graphics]::FromImage()
.CopyFromScreen(0, 0, 0, 0, .Size)
.Save("dashboard-screenshot.png")
.Dispose()
.Dispose()
