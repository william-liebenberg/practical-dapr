function Join-Objects($source, $extend) {
    if ($source.GetType().Name -eq "PSCustomObject" -and $extend.GetType().Name -eq "PSCustomObject") {
        foreach ($Property in $source | Get-Member -type NoteProperty, Property) {
            if ($null -eq $extend.$($Property.Name)) {
                continue;
            }
            $source.$($Property.Name) = Join-Objects $source.$($Property.Name) $extend.$($Property.Name)
        }
    }
    else {
        $source = $extend;
    }
    return $source
}

function Add-Property($source, $toExtend) {
    if ($source.GetType().Name -eq "PSCustomObject") {
        foreach ($Property in $source | Get-Member -type NoteProperty, Property) {
            if ($null -eq $toExtend.$($Property.Name)) {
                $toExtend | Add-Member -MemberType NoteProperty -Value $source.$($Property.Name) -Name $Property.Name
            }
            else {
                $toExtend.$($Property.Name) = Add-Property $source.$($Property.Name) $toExtend.$($Property.Name)
            }
        }
    }
    return $toExtend
}

function Merge-Objects($source, $extend) {
    $merged = Join-Objects $source $extend
    $extended = Add-Property $merged $extend
    return $extended
}

# Find all the spec files to merge
$specFiles = Get-ChildItem -Filter specification.json -Recurse -File -Name

# Loop through all the spec files and merge them one by one
$merged = [PSCustomObject]@{}
foreach($specFile in $specFiles) {
    Write-Host "Merging spec: $($specFile)" -ForegroundColor Green -NoNewline
    try {
        $spec = Get-Content -Path $specFile -Raw | ConvertFrom-Json
        $merged = Merge-Objects $spec $merged
        Write-Host "...✅ Done!"
    }
    catch {
        Write-Host "... ❌ Failed! $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Add some customizations
$merged.info.title = "My DaprShop API"
$merged.info.description = "My DaprShop API"

# Save the merged Spec
$destination="./DaprShop.Gateway.API/wwwroot/api/v1/daprshop.json"
New-Item -ItemType File -Path $destination -Force | Out-Null
$merged | ConvertTo-Json -Depth 100 | Out-File -FilePath $destination