$terms = @('Firadzo','Tower Defence','3D Defence','Defence Laser','Defence Cannon','Robot','Juanpmh','Creative Characters','ithappy','Top-Down Scifi','TopDown','Asset Maiden','Lowpoly Scifi','Black Rose','Sci-Fi MESS','PolyNest','Modular')
Write-Host "=== ASSET FOLDER AUDIT ==="
foreach ($t in $terms) {
    $r = Get-ChildItem -Recurse -Path "Assets" -ErrorAction SilentlyContinue | Where-Object { $_.FullName -match $t } | Select-Object -First 1
    if ($r) { Write-Host "FOUND: $t -> $($r.FullName)" }
    else { Write-Host "NOT FOUND: $t" }
}
Write-Host ""
Write-Host "=== RELEVANT PREFABS ==="
Get-ChildItem -Recurse -Path "Assets" -Filter "*.prefab" | Where-Object { $_.Name -match "Turret|Tower|Robot|Character|Scifi|Modular|Prop|Enemy|Laser|Cannon|Tank|Guard|Soldier|Drone" } | ForEach-Object { Write-Host "PREFAB: $($_.Name) -> $($_.FullName)" }
Write-Host ""
Write-Host "=== ALL TOP-LEVEL ASSET FOLDERS ==="
Get-ChildItem -Path "Assets" -Directory | ForEach-Object { Write-Host "DIR: $($_.Name)" }
