Write-Host "=== PREFABS (Turret/Laser/Cannon/Robot/Character/Defence) ==="
Get-ChildItem -Recurse -Path "Assets" -Filter "*.prefab" | Where-Object { $_.FullName -match "Turret|Laser|Cannon|Robot|Character|ithappy|Firadzo|Defence" } | ForEach-Object { Write-Host "PREFAB: $($_.Name) | $($_.FullName)" }

Write-Host ""
Write-Host "=== FBX (Turret/Laser/Cannon/Robot/Character) ==="
Get-ChildItem -Recurse -Path "Assets" -Filter "*.fbx" | Where-Object { $_.FullName -match "Turret|Laser|Cannon|Robot|Character" } | ForEach-Object { Write-Host "FBX: $($_.Name) | $($_.FullName)" }

Write-Host ""
Write-Host "=== ALL ASSET FOLDERS (top 3 levels) ==="
Get-ChildItem -Recurse -Path "Assets" -Directory | Where-Object { ($_.FullName.Split('\').Count - "C:\Users\helde\IronBastion_GDD\Assets".Split('\').Count) -le 3 } | ForEach-Object { Write-Host "DIR: $($_.FullName)" }

Write-Host ""
Write-Host "=== MATERIALS (Defence/Robot/ithappy/Turret/Character) ==="
Get-ChildItem -Recurse -Path "Assets" -Filter "*.mat" | Where-Object { $_.FullName -match "Firadzo|Defence|Robot|ithappy|Turret|Character" } | ForEach-Object { Write-Host "MAT: $($_.Name) | $($_.FullName)" }
