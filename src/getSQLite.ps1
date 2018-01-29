try {
    Push-Location $PSScriptRoot
    $year = "2018"
    $version = "3220000"

    &{ #<# Win32
        function Get-Win32 ($win, $arch) {
            Invoke-WebRequest https://sqlite.org/$year/sqlite-dll-$win-$arch-$version.zip -OutFile sqlite.zip
            Expand-Archive .\sqlite.zip -Force
            $dest = New-Item "runtimes\win-$arch\lib\netstandard1.4" -ItemType Directory -Force
            Move-Item "sqlite\sqlite3.dll" -Destination $dest -Force            
        }
        Get-Win32 win32 x86
        Get-Win32 win64 x64
    <##>}

    &{ #<#  UAP 10.0
        Invoke-WebRequest https://sqlite.org/$year/sqlite-uwp-$version.vsix -OutFile sqlite.zip
        Expand-Archive .\sqlite.zip -Force
        Get-ChildItem sqlite\Redist\Retail | ForEach-Object {
            $dest = New-Item "runtimes\win-$($_.Name.ToLowerInvariant())\lib\uap10.0" -ItemType Directory -Force
            Move-Item (Join-Path $_.FullName sqlite3.dll) -Destination $dest -Force
        }
    <##>}

    Remove-Item sqlite -Recurse
    Remove-Item sqlite.zip
} finally { Pop-Location }
