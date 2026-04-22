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
      EnemyController.cs                   res://InTreeModule/EnemyController.cs   [Icon]
    InTreeGodotSdkModule/                <- in-tree module (Godot.NET.Sdk)
      InTreeGodotSdkModule.csproj
      HealthComponent.cs                   res://InTreeGodotSdkModule/HealthComponent.cs
    TransitiveDependencyModule/          <- transitive via InTreeGodotSdkModule
      TransitiveDependencyModule.csproj
      TransitivePlayer.cs                  res://TransitiveDependencyModule/...
  ExternalNuGetModule/                   <- OUTSIDE res://, distributed as NuGet package
    ExternalNuGetModule.csproj             (Microsoft.NET.Sdk + NuGet)
    InventorySystem.cs                     csharp://ExternalNuGetModule/...       [Icon]
    InventoryItem.cs                       csharp://ExternalNuGetModule/...       (Resource)
  TransitiveNuGetFromNuGet/              <- transitive NuGet -> NuGet
    TransitiveNuGetFromNuGet.csproj
    QuestTracker.cs                        csharp://TransitiveNuGetFromNuGet/...
  TransitiveNuGetFromProject/            <- transitive Project -> NuGet
    TransitiveNuGetFromProject.csproj
    BuffSystem.cs                          csharp://TransitiveNuGetFromProject/...
```

## Three scenarios covered

### 1. In-tree ProjectReference (Microsoft.NET.Sdk)
**`InTreeModule/`** — Uses `Microsoft.NET.Sdk` with manual `GodotSharp` and `Godot.SourceGenerators` NuGet references. Source files are inside the Godot project tree, so they get `res://` paths.

- Appears in FileSystem panel, Add Node dialog, and Select Script dialog
- Opens in external editor (Rider, VS, etc.)
- `[Export]` properties visible and editable in Inspector

### 2. In-tree ProjectReference (Godot.NET.Sdk)
**`InTreeGodotSdkModule/`** — Uses `Godot.NET.Sdk` directly. Same behavior as scenario 1. `Directory.Build.props` redirects `obj/bin` output outside `res://` to keep the FileSystem panel clean.

### 3. External NuGet PackageReference
**`ExternalNuGetModule/`** — Lives **outside** the Godot project directory. Compiled and distributed as a NuGet package. Gets `csharp://` paths since source files are not under `res://`.

- `InventorySystem` (Node) appears in Add Node dialog via `[GlobalClass]`
- `InventoryItem` (Resource) appears in FileSystem -> Create New -> Resource dialog via `[GlobalClass]`
- `[Export]` properties visible and editable in Inspector for both
- `[Icon]` attributes render correctly (custom icon next to the class name)
- Cannot be opened in external editor (source not on disk) — shows warning
- Does not appear in FileSystem panel (no file under `res://`)

### 4. Transitive references
**`TransitiveNuGetFromNuGet/`** (NuGet -> NuGet) and **`TransitiveNuGetFromProject/`** (in-tree ProjectReference -> NuGet) confirm that `[GlobalClass]` types reached via a chain of references are discovered and registered the same way as direct references. Their scripts (`QuestTracker`, `BuffSystem`) also get `csharp://` paths.

## Important: `TOOLS` and `GODOT` defines

Projects using `Microsoft.NET.Sdk` (not `Godot.NET.Sdk`) **must** define `GODOT` and `TOOLS` for `[Export]` properties to work correctly in the editor. Without these defines, the source generators' editor-only code (like `GetGodotPropertyDefaultValues`) is compiled out via `#if TOOLS`, causing properties to show default C# values (0, null) and be non-editable.

`Godot.NET.Sdk` sets these automatically. For `Microsoft.NET.Sdk` projects, add to your `.csproj`:

```xml
<DefineConstants>GODOT;TOOLS;$(DefineConstants)</DefineConstants>
```

Or set it in `Directory.Build.props` to apply to all projects (as this example does):

```xml
<DefineConstants Condition="!$(DefineConstants.Contains('GODOT'))">GODOT;TOOLS;$(DefineConstants)</DefineConstants>
```

This also applies to **NuGet packages** — they must be built with `TOOLS` defined, otherwise `[Export]` won't work for consumers.

## How it works

### Key setting: `dotnet/project/project_directory`

In `project.godot`:
```ini
[dotnet]
project/project_directory="MainProject"
```

This tells Godot that the `.csproj` is in `MainProject/` subdirectory, not next to `project.godot`. This enables the recommended layout where `project.godot` is at the solution root and all module directories are siblings.

### Directory.Build.props

Sets `GodotProjectDir` for all projects in the solution, ensuring source generators compute `res://` paths relative to the Godot project root (where `project.godot` lives). Also redirects `bin/obj` output outside `res://` and defines `GODOT`/`TOOLS` for non-SDK projects:

```xml
<PropertyGroup>
  <GodotProjectDir>$(MSBuildThisFileDirectory)</GodotProjectDir>

  <BaseOutputPath>$(GodotProjectDir).godot\mono\temp\bin\</BaseOutputPath>
  <BaseIntermediateOutputPath>$(GodotProjectDir).godot\mono\temp\obj\$(MSBuildProjectName)\</BaseIntermediateOutputPath>
  <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>

  <DefineConstants Condition="!$(DefineConstants.Contains('GODOT'))">GODOT;TOOLS;$(DefineConstants)</DefineConstants>
</PropertyGroup>
```

### nuget.config

References a local NuGet source for the dev-version Godot packages (`4.7.0-dev`). Update the path to match your local setup:

```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="LocalGodotDev" value="C:\Users\user\MyLocalNugetSource" />
  </packageSources>
</configuration>
```

## Prerequisites

- Custom Godot 4.7-dev engine build with the multi-assembly patch ([PR #117452](https://github.com/godotengine/godot/pull/117452))
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

- [godotengine/godot PR #117452](https://github.com/godotengine/godot/pull/117452) — Engine patch (proof of concept)
- [godotengine/godot#100963](https://github.com/godotengine/godot/issues/100963) — Cannot organize code into multiple assemblies
- [godotengine/godot#95036](https://github.com/godotengine/godot/issues/95036) — GlobalClass regression with external assemblies
- [godotengine/godot#75352](https://github.com/godotengine/godot/issues/75352) — Loading scripts from external assemblies
- [godotengine/godot-proposals#7895](https://github.com/godotengine/godot-proposals/issues/7895) — C# GDExtension Roadmap
