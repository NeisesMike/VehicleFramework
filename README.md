# Vehicle Framework

Vehicle Framework is a Subnautica modding framework for adding custom vehicles and related upgrades and systems. For more information about how it works in-game and the effects of the mod, see the [Nexus Mods page](https://www.nexusmods.com/subnautica/mods/859).

## Building

Requirements: the .NET SDK and the .NET Framework 4.7.2 Developer Pack/targeting pack.

From the repository root, build the solution:

```powershell
dotnet build .\VehicleFramework\VehicleFramework.sln
```

The project targets .NET Framework 4.7.2 and restores its NuGet dependencies during the build. When a Subnautica installation is found, the build also copies the plugin and localization files to its BepInEx plugins folder. To specify a different game directory:

```powershell
dotnet build .\VehicleFramework\VehicleFramework.sln /p:SubnauticaDir="D:\Games\Subnautica"
```

## Contributing

Bug reports, translation improvements, and code contributions are welcome. For substantial changes, open an issue first to discuss the proposed approach. For a contribution, create a focused branch, make the change, build the solution, and submit a pull request with a clear summary and any relevant testing details. Keep localization keys consistent across language files and use `VehicleFramework/Localization/English.json` as the semantic source for translations.

This project is licensed under the [GNU General Public License v3.0](LICENSE).
