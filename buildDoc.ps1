$env:SOURCE_DIR="$PSScriptRoot"
$env:DOC_DIR="$PSScriptRoot\documentation_data"
$env:DEST_DIR="$PSScriptRoot\docs"
echo "*************************************************"
echo "* SourceCode Directory: $env:SOURCE_DIR"
echo "* SourceDocumentation Directory: $env:DOC_DIR"
echo "* Destination Directory: $env:DEST_DIR"
echo "*************************************************"

echo "* Copy code to temporary src directory"
echo "*************************************************"
if(!(Test-Path -Path $env:DOC_DIR\src\ )){
    New-Item -ItemType directory -Path $env:DOC_DIR\src\
}

cp -r $env:SOURCE_DIR\HciLab.Kinect\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\HciLab.KoBeLU.InterfacesAndDataModel\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\HciLab.Utilities\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\KoBeLU.Schema\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\KoBeLUAdmin\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\libs\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\misc\ $env:DOC_DIR\src\ 
cp -r $env:SOURCE_DIR\packages\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\tools\ $env:DOC_DIR\src\
cp -r $env:SOURCE_DIR\KoBeLUAdmin.sln $env:DOC_DIR\src\

echo "* Starting documentation generation"
echo "*************************************************"
docfx $env:DOC_DIR\docfx.json

echo "*************************************************"
echo "* Clear temp src directory"
echo "*************************************************"
Remove-Item $env:DOC_DIR\src\* -Recurse

echo "*Clear destination directory"
echo "*************************************************"
Remove-Item $env:DEST_DIR\* -Recurse

echo "*************************************************"
echo "* Copy generated content to destination"
echo "*************************************************"
cp -r $env:DOC_DIR\_site\* $env:DEST_DIR\

echo "*Clear temporary generation directory"
echo "*************************************************"
Remove-Item $env:DOC_DIR\_site\* -Recurse

echo "*********************DONE************************"
echo "*************************************************"

Write-Host "Press any key to continue ....."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")