# CSVEditor

A WPF-based CSV Editor application.

## Versions & Dependencies

This project is built using the following technologies and versions:

### Framework
- **Target Framework:** .NET 8.0 (`net8.0-windows10.0.22000.0`)

### Core Libraries (NuGet)
- **Newtonsoft.Json** (13.0.3) - JSON processing
- **LibGit2Sharp** (0.30.0) - Git operations
- **Prism.Core** (9.0.537) - MVVM Framework

### UI & Styling
- **FontAwesome5** (2.1.11) - Icons
- **MahApps.Metro.IconPacks** (5.1.0) - More icons

## Local Development & Testing (JetBrains Rider)

To run and test the application locally using JetBrains Rider:

1. **Open Solution:** Open `CSVEditor.sln` in JetBrains Rider.
2. **Restore NuGet Packages:** Rider should automatically restore dependencies. If not, right-click the Solution and select **Restore NuGet Packages**.
3. **Set Startup Project:** Ensure `CSVEditor.View` is selected as the Startup Project in the top toolbar.
4. **Run or Debug:**
   - Press `F5` to start with the debugger attached.
   - Press `Ctrl + F5` to run without debugging.
5. **XAML Hot Reload:** You can use Rider's XAML Hot Reload to see UI changes immediately while the app is running.

## How to Build Standalone Executable

To build a standalone `.exe` that includes all dependencies and the .NET runtime (so it can run on machines without .NET installed), use the following command:

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) installed on your system.

### Build Command
Run this command from the root directory of the project:

```powershell
dotnet publish CSVEditor.View\CSVEditor.View.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
```

#### Flag Explanation:
- `-c Release`: Builds in Release mode (optimized).
- `-r win-x64`: Targets 64-bit Windows.
- `--self-contained true`: Bundles the .NET runtime with the app.
- `-p:PublishSingleFile=true`: Packs everything into one `.exe`.
- `-p:PublishReadyToRun=true`: Improves startup time by ahead-of-time compilation.
- `-p:IncludeNativeLibrariesForSelfExtract=true`: Ensures native dependencies are correctly extracted at runtime.

### Output Location
After building, the standalone executable will be found at:
`CSVEditor.View\bin\Release\net8.0-windows10.0.22000.0\win-x64\publish\CSVEditor.View.exe`
