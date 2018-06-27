cd /D "%~dp0"
msbuild InheritdocInliner.csproj /t:rebuild /verbosity:quiet /p:Configuration=Release
nuget pack InheritdocInliner.csproj -OutputDirectory ..\..\..\LocalNuGet -Prop Configuration=Release -Tool