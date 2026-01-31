# ParseItemFiles.ps1
# Parses text files from [CP] Custom Starter Package Config and generates items.json

$outputItems = @()

# Define category mappings based on file and ID patterns
$files = @{
    "Objects.txt" = @{ Type = "Object"; Category = "Objects" }
    "Big Craftables.txt" = @{ Type = "BigCraftable"; Category = "Craftables" }
    "Tools.txt" = @{ Type = "Tool"; Category = "Tools" }
    "Weapons.txt" = @{ Type = "Weapon"; Category = "Weapons" }
    "Boots.txt" = @{ Type = "Boots"; Category = "Boots" }
    "Hats.txt" = @{ Type = "Hat"; Category = "Hats" }
    "Trinkets.txt" = @{ Type = "Trinket"; Category = "Trinkets" }
}

$basePath = "C:\Users\HP\Documents\[CP] Custom Starter Package Config"
$outputPath = "C:\Users\HP\Documents\CustomStarterPackageBuilderGUI\Data\items.json"

foreach ($file in $files.Keys) {
    $filePath = Join-Path $basePath $file
    if (Test-Path -LiteralPath $filePath) {
        $content = Get-Content -LiteralPath $filePath -Raw
        # Split by "Item" marker and process each entry
        $entries = $content -split '(?m)^Item\s*$' | Where-Object { $_.Trim() -ne "" -and $_.Trim() -ne $file.Replace(".txt","") }
        
        $fileCount = 0
        foreach ($entry in $entries) {
            $lines = $entry.Trim() -split "`n" | Where-Object { $_.Trim() -ne "" }
            if ($lines.Count -ge 2) {
                $name = $lines[0].Trim()
                $idLine = $lines[1].Trim()
                
                # Extract qualified ID from brackets [xxx]
                if ($idLine -match '\[([^\]]+)\]') {
                    $qualifiedId = $matches[1]
                    
                    # Determine type from prefix
                    $type = $files[$file].Type
                    $category = $files[$file].Category
                    
                    # Extract the raw ID (without prefix)
                    $nameOrIndex = $qualifiedId -replace '^\([A-Z]+\)', ''
                    
                    # Determine MaxStack based on type
                    $maxStack = switch ($type) {
                        "Tool" { 1 }
                        "Weapon" { 1 }
                        "Boots" { 1 }
                        "Hat" { 1 }
                        "Trinket" { 1 }
                        "BigCraftable" { 1 }
                        default { 999 }
                    }
                    
                    $item = [PSCustomObject]@{
                        Id = $nameOrIndex
                        Name = $name
                        Type = $type
                        Category = $category
                        QualifiedItemId = $qualifiedId
                        NameOrIndex = $nameOrIndex
                        MaxStack = $maxStack
                        Description = ""
                    }
                    $outputItems += $item
                    $fileCount++
                }
            }
        }
        Write-Host "Processed $file - found $fileCount items"
    } else {
        Write-Host "File not found: $filePath" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Total items parsed: $($outputItems.Count)" -ForegroundColor Green

# Convert to JSON and save
$json = $outputItems | ConvertTo-Json -Depth 3
$json | Set-Content $outputPath -Encoding UTF8

Write-Host "Saved to: $outputPath" -ForegroundColor Cyan
