cd /D "%~dp0"
nuget pack InheritdocInliner.csproj -OutputDirectory ..\..\..\LocalNuGet -Prop Configuration=Release -Tool