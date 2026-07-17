# CSVEditor.Core

Core library for the CSVEditor application. It contains shared models, services, constants, extensions, and configuration/app-state handling used by `CSVEditor.View` and `CSVEditor.ViewModel`.

## Framework

- **Target Framework:** `.NET 10` (`net10.0-windows10.0.22000.0`)
- **UseWPF:** `true`

## NuGet dependencies

- **CredentialManagement** (1.0.2)
- **LibGit2Sharp** (0.30.0)
- **Newtonsoft.Json** (13.0.3)
- **Octokit** (14.0.0)

## UI styling (app project)

UI styling packages are defined in `CSVEditor.View`:

- **FontAwesome5** (2.1.11)
- **MahApps.Metro.IconPacks** (5.1.0)

## Deployment (standalone executable)

Build from repository root:

```powershell
dotnet publish CSVEditor.View\CSVEditor.View.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output location:

`CSVEditor.View\bin\Release\net10.0-windows10.0.22000.0\win-x64\publish\CSVEditor.exe`

## AppOptions notes

`AppOptions` stores persisted editor/session settings, including:

- `LastRootPath`
- `LastOpenedDirectoryPath`
- `LastSavedDirectoryPath`
- `LastSelectedFilePath`
- visual, save, git and CSV-related options

`LastOpenedDirectoryPath` defaults to `MyDocuments` when empty.  
`LastSavedDirectoryPath` defaults to `LastRootPath` (or `MyDocuments` if `LastRootPath` is empty).
