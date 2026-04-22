# Testing guide

Repros for the two issues reported by @pcloves on [godotengine/godot#117452](https://github.com/godotengine/godot/pull/117452), and regression checks for the fixes.

## Preconditions

- Patched Godot editor built from branch [matelq/godot:multi-assembly-fix-comprehensive](https://github.com/matelq/godot/tree/multi-assembly-fix-comprehensive) at `bin/godot.windows.editor.x86_64.mono.exe`.
- `.NET SDK 10` installed.
- Local NuGet source at `C:\Users\user\MyLocalNugetSource` populated with the 4 Godot dev nupkgs produced by `build_assemblies.py` (`Godot.NET.Sdk.4.7.0-dev.nupkg`, `Godot.SourceGenerators.4.7.0-dev.nupkg`, `GodotSharp.4.7.0-dev.nupkg`, `GodotSharpEditor.4.7.0-dev.nupkg`) and with the example's own 4 packages (`ExternalNuGetModule.1.0.0`, `NuGetModule.1.0.0`, `TransitiveNuGetFromNuGet.1.0.0`, `TransitiveNuGetFromProject.1.0.0`).

## Build the example project

```powershell
Set-Location "C:\Users\user\GodotProjects\LearningGameDev\MultiAssemblyExample-new"
dotnet build GodotProject\MainProject\MultiAssemblyExample.csproj
```

Confirm the main DLL:

```powershell
Test-Path "GodotProject\.godot\mono\temp\bin\Debug\MultiAssemblyExample.dll"   # should be True
```

## Launch the editor

**Do not wipe `.godot/`** — the compiled assemblies live there and the engine's first-startup assembly loader reads them from `.godot/mono/temp/bin/Debug/`.

```powershell
Set-Location "C:\Users\user\GodotProjects\LearningGameDev\godot"
bin\godot.windows.editor.x86_64.mono.exe -v -e --path "C:\Users\user\GodotProjects\LearningGameDev\MultiAssemblyExample-new\GodotProject" 2>&1 | Tee-Object -FilePath "$env:TEMP\godot-repro.log"
```

Wait for the "Project initialization" progress bar to finish before running any check.

## Test A — Bug 1 (Node dialog): csharp:// classes survive first scan

Open any scene (e.g. `node_3d.tscn`). Click **+** (Add Child Node).

| Search | Expected | Path scheme |
|---|---|---|
| `inven` | `InventorySystem` appears, blue Godot logo icon | csharp:// (was missing pre-fix) |
| `quest` | `QuestTracker` appears, generic Node icon | csharp:// (was missing pre-fix) |
| `buff` | `BuffSystem` appears, generic Node icon | csharp:// (was missing pre-fix) |
| `enemy` | `EnemyController` appears, blue Godot logo icon | res:// (always worked) |
| `health` | `HealthComponent` appears, generic Node icon | res:// (always worked) |
| `transitiveplayer` | `TransitivePlayer` appears, generic Node icon | res:// (always worked) |

All 6 must appear **without pressing Build in the MSBuild panel first**. Pre-fix, the 3 csharp:// entries would be missing until an MSBuild rebuild re-registered them.

## Test B — Bug 1 (Resource dialog): pcloves' canonical case

In the **FileSystem** panel (left side), right-click `res://` → **Create New → Resource...**

| Search | Expected |
|---|---|
| `inventoryitem` | `InventoryItem` appears under the `Resource` branch |

Must appear **without pressing Build**. This is the exact scenario pcloves reported.

## Test C — Internal state probe

Creates a throwaway `EditorScript` that queries `ScriptServer::get_global_class_list()` directly. Proves that all 7 assembly-backed `[GlobalClass]` types are registered and haven't been purged.

1. Click **Script** in the top menu bar (opens Script Editor).
2. **File → New Script...**:
   - Language: `GDScript`
   - Inherits: `EditorScript`
   - Path: `res://probe.gd`
   - Template: `Empty`
3. Paste this content, save (Ctrl+S):

```gdscript
@tool
extends EditorScript

func _run() -> void:
    var classes := ["InventorySystem", "InventoryItem", "QuestTracker", "BuffSystem",
                    "EnemyController", "HealthComponent", "TransitivePlayer"]
    var global := ProjectSettings.get_global_class_list()
    print("=== ScriptServer global classes (", global.size(), " total) ===")
    for cls in classes:
        var found := false
        for entry in global:
            if str(entry["class"]) == cls:
                print("  ", cls, "  path=", entry["path"], "  base=", entry["base"])
                found = true
                break
        if not found:
            print("  ", cls, "  -- MISSING from ScriptServer --")
```

4. With `probe.gd` focused in the Script Editor, **File → Run** (or Ctrl+Shift+X).
5. Check the **Output** panel at the bottom.

Expected output — 7 classes, 4 csharp:// + 3 res://:

```
=== ScriptServer global classes (N total) ===
  InventorySystem  path=csharp://ExternalNuGetModule/ExternalNuGetModule.InventorySystem.cs  base=Node
  InventoryItem  path=csharp://ExternalNuGetModule/ExternalNuGetModule.InventoryItem.cs  base=Resource
  QuestTracker  path=csharp://TransitiveNuGetFromNuGet/TransitiveNuGetFromNuGet.QuestTracker.cs  base=Node
  BuffSystem  path=csharp://TransitiveNuGetFromProject/TransitiveNuGetFromProject.BuffSystem.cs  base=Node
  EnemyController  path=res://InTreeModule/EnemyController.cs  base=Node
  HealthComponent  path=res://InTreeGodotSdkModule/HealthComponent.cs  base=Node
  TransitivePlayer  path=res://TransitiveDependencyModule/TransitivePlayer.cs  base=Node
```

Delete `res://probe.gd` after the check (don't commit it).

## Test D — Log spot-check for errors

After closing the editor:

```powershell
Select-String -Path "$env:TEMP\godot-repro.log" -Pattern "Failed to load|error|assertion|crash" -CaseSensitive:$false | Select-Object -First 20
```

Expected: only the pre-existing Vulkan loader warning (`WARNING: GENERAL - Message Id Number: 0 | Message Id Name: Loader Message`). No `.NET: Failed to load project assembly`, no C# stack traces.

## Pass criteria

- Test A: all 6 rows present.
- Test B: `InventoryItem` present.
- Test C: all 7 classes printed with correct paths.
- Test D: no new errors.

Any deviation means something regressed — capture the log and investigate before declaring the fix good.

## Iterating after code changes

- **Engine C++ changes only** → rebuild editor via `scons ...` (see [godot fork CLAUDE.md](https://github.com/matelq/godot/blob/fork/4.6/README.md) or memory), then relaunch Godot. Do NOT wipe `.godot/`.
- **Changes to `ExternalNuGetModule` / `TransitiveNuGetFromNuGet` / `TransitiveNuGetFromProject`** → repack the changed project (`dotnet pack -c Release -o "C:\Users\user\MyLocalNugetSource"`), clear cached `1.0.0` under `$env:USERPROFILE\.nuget\packages\<pkgname>\1.0.0`, then `dotnet build GodotProject\MainProject\MultiAssemblyExample.csproj`.
- **Changes to `MainProject` / `InTreeModule` / `InTreeGodotSdkModule` / `TransitiveDependencyModule`** → just `dotnet build GodotProject\MainProject\MultiAssemblyExample.csproj`, then relaunch Godot.
