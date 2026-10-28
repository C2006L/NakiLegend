$ErrorActionPreference='Stop'
$repoRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$runtimeRoot=Join-Path $repoRoot 'Assets/NakiPrototype'
$destinations=@('D:/NakiLegend/UnityPrototype/Assets/NakiPrototype/NakiLegend/Assets/NakiPrototype','D:/NakiLegend/UnityPrototype/Assets/NakiPrototype')
$names=@('NakiPrototype.Elements.cs','NakiPrototype.Equipment.cs','NakiPrototype.JourneyHud.cs','NakiPrototype.Origin.cs','NakiPrototype.PixelHero.cs','NakiPrototype.ThunderChapterSave.cs','NakiPrototype.ThunderSkills.cs','NakiPrototype.ThunderStory.cs','NakiPrototype.cs')
function Normalize-Source([string]$value){return $value.Replace("`r`n","`n").TrimEnd("`r","`n")}
foreach($name in $names){
    $sourcePath=Join-Path $runtimeRoot $name
    $sourceText=Normalize-Source ([System.IO.File]::ReadAllText($sourcePath))
    $headLines=& git -C $repoRoot show "HEAD:Assets/NakiPrototype/$name"
    if($LASTEXITCODE -ne 0){throw "Cannot inspect baseline for $name"}
    $headText=Normalize-Source ($headLines -join "`n")
    foreach($destination in $destinations){
        $targetPath=Join-Path $destination $name
        if(Test-Path -LiteralPath $targetPath){
            $targetText=Normalize-Source ([System.IO.File]::ReadAllText($targetPath))
            if($targetText -ne $sourceText -and $targetText -ne $headText){throw "Preserved unexpected destination changes: $targetPath"}
        }
    }
}
$delivery=@()
foreach($name in $names){
    $sourcePath=Join-Path $runtimeRoot $name
    foreach($destination in $destinations){
        $targetPath=Join-Path $destination $name
        Copy-Item -LiteralPath $sourcePath -Destination $targetPath
        $hash=(Get-FileHash -LiteralPath $sourcePath).Hash
        if((Get-FileHash -LiteralPath $targetPath).Hash -ne $hash){throw "Delivery differs: $targetPath"}
        $delivery += [pscustomobject]@{File=$name;Destination=$targetPath;SHA256=$hash}
    }
}
[System.IO.File]::WriteAllText((Join-Path $PSScriptRoot 'delivery.json'),($delivery | ConvertTo-Json -Depth 4),[System.Text.UTF8Encoding]::new($false))
Write-Output "Verified and delivered $($delivery.Count) files. Player scenes and JSON saves untouched."
