using BepInEx.Bootstrap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VehicleFramework
{
    internal static class CompatChecker
    {
        internal static void CheckAll()
        {
            CheckForNautilusUpdate();
            CheckForBepInExPackUpdate();
            CheckForFlareDurationIndicator();
            CheckForBuildingTweaks();
            CheckForVanillaExpanded();
        }

        #region private_utilities
        private static void ShowError(string message)
        {
            Logger.LoopMainMenuError(message, "Vehicle Framework");
        }
        private static void ShowWarning(string message)
        {
            Logger.LoopMainMenuWarning(message, "Vehicle Framework");
        }
        #endregion

        #region checks
        private static void CheckForBepInExPackUpdate()
        {
            Version target = new Version("1.0.2");
            if (Chainloader.PluginInfos["Tobey.Subnautica.ConfigHandler"].Metadata.Version.CompareTo(target) < 0)
            {
                ShowWarning("There is a BepInEx Pack update available!");
            }
        }
        private static void CheckForNautilusUpdate()
        {
            Version target = new Version(Nautilus.PluginInfo.PLUGIN_VERSION);
            if (Chainloader.PluginInfos[Nautilus.PluginInfo.PLUGIN_GUID].Metadata.Version.CompareTo(target) < 0)
            {
                ShowWarning("There is a Nautilus update available!");
            }
        }
        private static void CheckForFlareDurationIndicator()
        {
            if (Chainloader.PluginInfos.ContainsKey("com.ramune.FlareDurationIndicator"))
            {
                if (Chainloader.PluginInfos["com.ramune.FlareDurationIndicator"].Metadata.Version.ToString() == "1.0.1")
                {
                    ShowError("Not compatible with the Flare Duration Indicator mod version 1.0.1\nPlease remove or downgrade the plugin.");
                    Logger.Log("Flare Duration Indicator 1.0.1 has a bad patch that must be fixed.");
                }
            }
        }
        private static void CheckForBuildingTweaks()
        {
            const string buildingTweaksGUID = "BuildingTweaks";
            if (Chainloader.PluginInfos.ContainsKey(buildingTweaksGUID))
            {
                ShowWarning("Do not use BuildingTweaks to build things inside/on Vehicle Framework submarines!");
                Logger.Log("Using some BuildingTweaks options to build things inside submarines can prevent those buildables from correctly anchoring to the submarine. Be careful.");
            }
        }
        private static void CheckForVanillaExpanded()
        {
            const string vanillaExpandedGUID = "VanillaExpanded";
            if (Chainloader.PluginInfos.ContainsKey(vanillaExpandedGUID))
            {
                ShowError("Some vehicles not compatible with Vanilla Expanded!");
                Logger.Log("Vanilla Expanded has a patch on UniqueIdentifier.Awake that throws an error (dereferences null) during many Vehicle Framework setup methods. If you choose to continue, some vehicles, buildables, and fragments may simply not appear.");
            }
        }
        #endregion
    }
}
