using UnityEngine;
using Nautilus.Options;
using Nautilus.Options.Attributes;

namespace VehicleFramework
{
    [Menu("Vehicle Framework")]
    internal class VehicleFrameworkNautilusConfig : Nautilus.Json.ConfigFile
    {
        [Button(LabelLanguageId = "VFConfigManagerLabel", TooltipLanguageId = "VFConfigManagerTooltip")]
        public static void PrintConfigManagerInfo(ButtonClickedEventArgs _)
        {
            ErrorMessage.AddMessage(Language.main.Get("VFConfigManagerPrintInfo"));
        }

        #region camera
        [Keybind(LabelLanguageId = "VFNextCameraLabel", TooltipLanguageId = "VFNextCameraTooltip")]
        public KeyCode NextCamera = KeyCode.F;

        [Keybind(LabelLanguageId = "VFPrevCameraLabel", TooltipLanguageId = "VFPrevCameraTooltip")]
        public KeyCode PreviousCamera = KeyCode.T;

        [Keybind(LabelLanguageId = "VFExitCameraLabel", TooltipLanguageId = "VFExitCameraTooltip")]
        public KeyCode ExitCamera = KeyCode.V;
        #endregion

        #region headlights
        [Keybind(LabelLanguageId = "VFHeadlightsButtonLabel", TooltipLanguageId = "VFHeadlightsButtonTooltip")]
        public KeyCode HeadlightsButton = KeyCode.Mouse1;

        [Toggle(LabelLanguageId = "VFLeftClickHeadlightsLabel", TooltipLanguageId = "VFLeftClickHeadlightsTooltip")]
        public bool LeftClickHeadlights = false;

        [Toggle(LabelLanguageId = "VFRightClickHeadlightsLabel", TooltipLanguageId = "VFRightClickHeadlightsTooltip")]
        public bool RightClickHeadlights = false;
        #endregion

        [Keybind(LabelLanguageId = "VFMagBootsButtonLabel", TooltipLanguageId = "VFMagBootsButtonTooltip")]
        public KeyCode MagnetBootsButton = KeyCode.G;

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
        public static void UnlockDroneStation(ButtonClickedEventArgs _)
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
