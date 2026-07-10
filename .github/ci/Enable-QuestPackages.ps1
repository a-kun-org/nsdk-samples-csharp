# Injects the Meta Quest packages into Packages/manifest.json for the Quest
# CI flavor only. The committed manifest stays identical to upstream so the
# phone AR flavor never loads Meta XR SDKs (their build hooks fail Android
# builds that do not use a Meta XR loader — see OVRGradleGeneration).
param(
    [string]$ManifestPath = "NsdkSamples/Packages/manifest.json"
)

$ErrorActionPreference = 'Stop'

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json

$registry = [pscustomobject]@{
    name   = 'Meta XR'
    url    = 'https://npm.developer.oculus.com'
    scopes = @('com.meta.xr')
}
if (-not $manifest.PSObject.Properties['scopedRegistries']) {
    $manifest | Add-Member -MemberType NoteProperty -Name scopedRegistries -Value @($registry)
} elseif (-not ($manifest.scopedRegistries | Where-Object url -eq $registry.url)) {
    $manifest.scopedRegistries = @($manifest.scopedRegistries) + $registry
}

$deps = @{
    'com.nianticspatial.nsdk.metaquest' = 'https://github.com/nianticspatial/nsdk-library-upm-quest3.git#4.1.0-26051913'
    'com.unity.xr.interaction.toolkit'  = '3.5.1'
}
foreach ($name in $deps.Keys) {
    if ($manifest.dependencies.PSObject.Properties[$name]) {
        $manifest.dependencies.$name = $deps[$name]
    } else {
        $manifest.dependencies | Add-Member -MemberType NoteProperty -Name $name -Value $deps[$name]
    }
}

$manifest | ConvertTo-Json -Depth 10 | Set-Content $ManifestPath -Encoding utf8
Write-Host "Quest packages injected into $ManifestPath"
Get-Content $ManifestPath | Select-Object -First 20
