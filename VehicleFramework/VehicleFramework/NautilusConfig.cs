using Nautilus.Options;
using Nautilus.Options.Attributes;
using System.Reflection;
using UnityEngine;

namespace VehicleFramework
{
    [Menu("Vehicle Framework")]
    internal class VehicleFrameworkNautilusConfig : Nautilus.Json.ConfigFile
    {
        [Button(LabelLanguageId = "VFConfigManagerLabel", TooltipLanguageId = "VFConfigManagerTooltip")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Config will totally die if this is marked static.")]
        public void PrintConfigManagerInfo(ButtonClickedEventArgs _)
        {
            ErrorMessage.AddMessage(Language.main.Get("VFConfigManagerPrintInfo"));
        }

        #region accessibility
        [Toggle(LabelLanguageId = "VFFlashingLightsLabel", TooltipLanguageId = "VFFlashingLightsTooltip")]
        public bool IsFlashingLights = false;

        [Toggle(LabelLanguageId = "VFSubtitlesLabel", TooltipLanguageId = "VFSubtitlesTooltip")]
        public bool IsSubtitles = true;

        [Toggle(LabelLanguageId = "VFFahrenheit", TooltipLanguageId = "VFFahrenheitTooltip")]
        public bool IsFahrenheit = false;

        [Toggle(LabelLanguageId = "VFDebugLogLabel", TooltipLanguageId = "VFDebugLogTooltip")]
        public bool IsDebugLogging = false;
        #endregion

        #region cheats
        [Button(LabelLanguageId = "VFDroneStationCheatLabel", TooltipLanguageId = "VFDroneStationCheatTooltip")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Config will totally die if this is marked static.")]
        public void UnlockDroneStation(ButtonClickedEventArgs _)
        {
            if(Player.main == null)
            {
                ErrorMessage.AddWarning(Language.main.Get("VFDroneStationError"));
            }
            else
            {
                DevConsole.SendConsoleCommand("unlock dronestation");
            }
        }
        #endregion
    }
}
