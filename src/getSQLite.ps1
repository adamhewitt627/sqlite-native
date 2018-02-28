try {
    # Based on https://stackoverflow.com/questions/40104838/automatic-native-and-managed-dlls-extracting-from-nuget-package
    Push-Location $PSScriptRoot
    $buildVersion = [version]([regex]"version: (\d+.\d+.\d+)").Match((Get-Content ..\appveyor.yml -Raw)).Groups[1].Value
    $version = @($buildVersion.Major.ToString(), $buildVersion.Minor.ToString().PadRight(3,'0'), $buildVersion.Build.ToString().PadRight(3,'0')) -join ""

    $year = (Get-Date).Year + 1
    do { try {
        Invoke-WebRequest "https://sqlite.org/$year/sqlite-amalgamation-$version.zip" -Method Head | Out-Null
        break
    } catch { $year-- } } while ($true)

    &{ #<# Win32
        function Get-Win32 ($win, $arch) {
            Invoke-WebRequest https://sqlite.org/$year/sqlite-dll-$win-$arch-$version.zip -OutFile sqlite.zip
            Expand-Archive .\sqlite.zip -Force
            $dest = New-Item "runtimes\win-$arch\netcoreapp2.0" -ItemType Directory -Force
            Move-Item "sqlite\sqlite3.dll" -Destination $dest -Force
        }
        Get-Win32 win32 x86
        Get-Win32 win64 x64
    <##>}

    &{ #<#  UAP 10.0
        Invoke-WebRequest https://sqlite.org/$year/sqlite-uwp-$version.vsix -OutFile sqlite.zip
        Expand-Archive .\sqlite.zip -Force
        Get-ChildItem sqlite\Redist\Retail | ForEach-Object {
            $dest = New-Item "runtimes\win10-$($_.Name.ToLowerInvariant())\native" -ItemType Directory -Force
            Move-Item (Join-Path $_.FullName sqlite3.dll) -Destination $dest -Force
        }
    <##>}

    Remove-Item sqlite -Recurse
    Remove-Item sqlite.zip
} finally { Pop-Location }
