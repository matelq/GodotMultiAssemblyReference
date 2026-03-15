# Godot Multi-Assembly C# Reference Project

Example project demonstrating multi-assembly C# script support in Godot 4.7-dev. This requires the engine patch from [godotengine/godot PR #117452](https://github.com/godotengine/godot/pull/117452).

## What this demonstrates

Godot normally only discovers C# scripts from the main project assembly. This example shows three ways to use scripts from **additional assemblies**, with `[Export]` properties, `[GlobalClass]`, and lifecycle methods all working correctly.

## Project structure

```
GodotMultiAssemblyReference/
  GodotProject/                          <- project.godot here (res:// root)
    project.godot
    Directory.Build.props
    nuget.config
    MultiAssemblyExample.slnx
    node_3d.tscn                         <- test scene with all node types
    icon.svg
    MainProject/                         <- main Godot C# project
      MultiAssemblyExample.csproj          (Godot.NET.Sdk)
      MainScene.cs
    InTreeModule/                        <- in-tree module (Microsoft.NET.Sdk + NuGet)
      InTreeModule.csproj
      EnemyController.cs                   res://InTreeModule/EnemyController.cs
    InTreeGodotSdkModule/                <- in-tree module (Godot.NET.Sdk)
      InTreeGodotSdkModule.csproj
      HealthComponent.cs                   res://InTreeGodotSdkModule/HealthComponent.cs
  ExternalNuGetModule/                   <- OUTSIDE res://, distributed as NuGet package
    ExternalNuGetModule.csproj             (Microsoft.NET.Sdk + NuGet)
    InventorySystem.cs                     csharp://ExternalNuGetModule/...
```

## Three scenarios covered

### 1. In-tree ProjectReference (Microsoft.NET.Sdk)
**`InTreeModule/`** — Uses `Microsoft.NET.Sdk` with manual `GodotSharp` and `Godot.SourceGenerators` NuGet references. Source files are inside the Godot project tree, so they get `res://` paths.

- Appears in FileSystem panel, Add Node dialog, and Select Script dialog
- Opens in external editor (Rider, VS, etc.)
- `[Export]` properties visible in Inspector

### 2. In-tree ProjectReference (Godot.NET.Sdk)
**`InTreeGodotSdkModule/`** — Uses `Godot.NET.Sdk` directly. Same behavior as scenario 1. `Directory.Build.props` redirects `obj/bin` output outside `res://` to keep the FileSystem panel clean.

### 3. External NuGet PackageReference
**`ExternalNuGetModule/`** — Lives **outside** the Godot project directory. Compiled and distributed as a NuGet package. Gets `csharp://` paths since source files are not under `res://`.

- Appears in Add Node dialog (via `[GlobalClass]`)
- `[Export]` properties visible in Inspector
- Cannot be opened in external editor (source not on disk) — shows warning
- Does not appear in FileSystem panel (no file under `res://`)

## How it works

### Key setting: `dotnet/project/project_directory`

In `project.godot`:
```ini
[dotnet]
project/project_directory="MainProject"
```

This tells Godot that the `.csproj` is in `MainProject/` subdirectory, not next to `project.godot`. This enables the recommended layout where `project.godot` is at the solution root and all module directories are siblings.

### Directory.Build.props

Sets `GodotProjectDir` for all projects in the solution, ensuring source generators compute `res://` paths relative to the Godot project root (where `project.godot` lives):

```xml
<PropertyGroup>
  <GodotProjectDir>$(MSBuildThisFileDirectory)</GodotProjectDir>
</PropertyGroup>
```

### nuget.config

References a local NuGet source for the dev-version Godot packages (`4.7.0-dev`):

```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="LocalGodotDev" value="C:\Users\user\MyLocalNugetSource" />
  </packageSources>
</configuration>
```

## Prerequisites

- Custom Godot 4.7-dev engine build with the multi-assembly patch
- .NET SDK 8.0+
- Local NuGet source with `Godot.NET.Sdk` 4.7.0-dev packages (built from patched engine)

## Building

```bash
# Build the NuGet package (from repo root)
cd ExternalNuGetModule
dotnet pack -o <your-local-nuget-source>

# Build the Godot project
cd GodotProject
dotnet build MainProject/MultiAssemblyExample.csproj

# Open in Godot editor
godot --editor --path GodotProject
```

## Related

- [godotengine/godot#75352](https://github.com/godotengine/godot/issues/75352) — Main tracking issue
- [godotengine/godot#95036](https://github.com/godotengine/godot/issues/95036) — GlobalClass regression
- [godotengine/godot-proposals#7895](https://github.com/godotengine/godot-proposals/issues/7895) — C# GDExtension Roadmap
