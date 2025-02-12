
using BepInEx;
using BepInEx.Bootstrap;
using System.Linq;
using System.Collections;
using UnityEngine;

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
            while (ErrorMessage.main == null)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1);
            while (ErrorMessage.main == null)
            {
                yield return null;
            }
            CheckForFlareDurationIndicator();
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
    }
}
