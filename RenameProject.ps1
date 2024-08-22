param (
    [Parameter(Mandatory = $true)]
    [string]$FolderPath,

    [Parameter(Mandatory = $true)]
    [string]$NewName
)

# Check if the folder exists
if (-not (Test-Path -Path $FolderPath)) {
    Write-Error "The folder path '$FolderPath' does not exist."
    exit
}

# Step 1: Rename folders
$folders = Get-ChildItem -Path $FolderPath -Recurse -Directory | Sort-Object -Property FullName -Descending

foreach ($folder in $folders) {
    if ($folder.Name -like "*Vault*") {
        $newFolderName = $folder.Name -replace "Vault", $NewName
        $newFolderPath = Join-Path -Path $folder.Parent.FullName -ChildPath $newFolderName

        Rename-Item -Path $folder.FullName -NewName $newFolderName
        Write-Host "Renamed folder '$($folder.FullName)' to '$newFolderPath'"
    }
}

# Step 2: Rename files
$files = Get-ChildItem -Path $FolderPath -Recurse -File -Filter "*Vault*"

foreach ($file in $files) {
    $newFileName = $file.Name -replace "Vault", $NewName
    $newFilePath = Join-Path -Path $file.DirectoryName -ChildPath $newFileName

    Rename-Item -Path $file.FullName -NewName $newFileName
    Write-Host "Renamed file '$($file.FullName)' to '$newFilePath'"
}

# Step 3: Update content inside files
$allFiles = Get-ChildItem -Path $FolderPath -Recurse -File

foreach ($file in $allFiles) {
    $content = Get-Content -Path $file.FullName -Raw

    if ($content -match "Vault") {
        $newContent = $content -replace "Vault", $NewName
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated content in '$($file.FullName)'"
    }
}

Write-Host "**********************************************"
Write-Host "Done!"
Write-Host "You can delete this script file if you want."





