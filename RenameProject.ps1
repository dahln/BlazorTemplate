param (
    [Parameter(Mandatory = $true)]
    [string]$FolderPath,

    [Parameter(Mandatory = $true)]
    [string]$OldName,

    [Parameter(Mandatory = $true)]
    [string]$NewName
)

# Check if the folder exists
if (-not (Test-Path -Path $FolderPath)) {
    Write-Error "The folder path '$FolderPath' does not exist."
    exit
}

# Define the old name segment to replace
$ScriptFileName = "RenameProject.ps1"

# Step 1: Rename folders
$folders = Get-ChildItem -Path $FolderPath -Recurse -Directory | Sort-Object -Property FullName -Descending

foreach ($folder in $folders) {
    if ($folder.Name -like "*$OldName*" -and $folder.Name -ne $ScriptFileName) {
        $newFolderName = $folder.Name -replace [regex]::Escape($OldName), $NewName
        $newFolderPath = Join-Path -Path $folder.Parent.FullName -ChildPath $newFolderName

        Rename-Item -Path $folder.FullName -NewName $newFolderName
        Write-Host "Renamed folder '$($folder.FullName)' to '$newFolderPath'"
    }
}

# Step 2: Rename files
$files = Get-ChildItem -Path $FolderPath -Recurse -File -Filter "*$OldName*"

foreach ($file in $files) {
    if ($file.Name -ne $ScriptFileName) {
        $newFileName = $file.Name -replace [regex]::Escape($OldName), $NewName
        $newFilePath = Join-Path -Path $file.DirectoryName -ChildPath $newFileName

        Rename-Item -Path $file.FullName -NewName $newFileName
        Write-Host "Renamed file '$($file.FullName)' to '$newFilePath'"
    }
}

# Step 3: Update content inside files
$allFiles = Get-ChildItem -Path $FolderPath -Recurse -File

foreach ($file in $allFiles) {
    if ($file.Name -ne $ScriptFileName) {
        $content = Get-Content -Path $file.FullName -Raw

        if ($content -match [regex]::Escape($OldName)) {
            $newContent = $content -replace [regex]::Escape($OldName), $NewName
            Set-Content -Path $file.FullName -Value $newContent
            Write-Host "Updated content in '$($file.FullName)'"
        }
    }
}
