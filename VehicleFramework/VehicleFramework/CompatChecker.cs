﻿
using BepInEx;
using BepInEx.Bootstrap;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;

namespace VehicleFramework
{
    internal static class CompatChecker
    {
        private static void ShowError(string message)
        {
            string result = $"<color=#FF0000>Vehicle Framework Error: </color><color=#FFFF00>{message}</color>";
            ErrorMessage.AddMessage(result);
            Logger.Error(message);
        }
        private static void ShowWarning(string message)
        {
            string result = $"<color=#FF0000>Vehicle Framework Warning: </color><color=#FFFF00>{message}</color>";
            ErrorMessage.AddMessage(result);
            Logger.Error(message);
        }
        internal static void CheckAll()
        {
            UWE.CoroutineHost.StartCoroutine(CheckAllSoon());
        }
        private static IEnumerator CheckAllSoon()
        {
            yield return new WaitUntil(() => ErrorMessage.main != null);
            yield return new WaitForSeconds(1);
            yield return new WaitUntil(() => ErrorMessage.main != null);
            CheckForNautilusUpdate();
            CheckForBepInExPackUpdate();
            CheckForFlareDurationIndicator();
            CheckForBuildingTweaks();
        }

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

        internal static void CheckForFlareDurationIndicator()
        {
            if (Chainloader.PluginInfos.ContainsKey("com.ramune.FlareDurationIndicator"))
            {
                if (Chainloader.PluginInfos["com.ramune.FlareDurationIndicator"].Metadata.Version.ToString() == "1.0.1")
                {
                    ShowError("not compatible with the Flare Duration Indicator mod version 1.0.1\nPlease remove or downgrade the plugin.");
                }
            }
        }
        private static void CheckForBuildingTweaks()
        {
            const string buildingTweaksGUID = "BuildingTweaks";
            if (Chainloader.PluginInfos.ContainsKey(buildingTweaksGUID))
            {
                ShowWarning("Do not use BuildingTweaks to build things inside/on Vehicle Framework submarines!");
            }
        }
    }
}
