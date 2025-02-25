using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace VehicleFramework
{
    internal class VehicleConfig
    {
        internal static Dictionary<string, VehicleConfig> main = new Dictionary<string, VehicleConfig>();
        internal static VehicleConfig GetConfig(ModVehicle mv)
        {
            if (!main.ContainsKey(mv.GetType().ToString()))
            {
                AddNew(mv);
            }
            return main[mv.GetType().ToString()];
        }
        private static VehicleConfig AddNew(ModVehicle mv)
        {
            var thisConf = new VehicleConfig();
            main.Add(mv.GetType().ToString(), thisConf);
            return thisConf;
        }
        internal ConfigEntry<bool> IsEnabled { get; set; }
        internal ConfigEntry<float> AutopilotVolume { get; set; }
        internal ConfigEntry<float> EngineVolume { get; set; }
        internal ConfigEntry<string> AutopilotVoice { get; set; }
        internal ConfigEntry<string> EngineSounds { get; set; }
        internal ConfigEntry<int> NumUpgrades { get; set; }
        internal ConfigEntry<bool> IsArms { get; set; }
        internal ConfigEntry<uGUI_VehicleHUD.HUDChoice> HUDChoice { get; set; }
        internal List<ConfigEntry<bool>> ExternalToggles = new List<ConfigEntry<bool>>();
        internal List<ConfigEntry<float>> ExternalSliders = new List<ConfigEntry<float>>();
        internal List<ConfigEntry<KeyboardShortcut>> ExternalKeybinds = new List<ConfigEntry<KeyboardShortcut>>();
    }
    internal class VFConfig
    {
        internal static ConfigFile config;
        internal static void GrabNewVoiceLines(object sender, EventArgs e)
        {
            if (Player.main != null)
            {
                foreach (var tmp in VoiceManager.voices.Where(x => x != null && x.mv != null & x.mv.GetComponent<TechTag>() != null))
                {
                    string voiceName = VehicleConfig.main[tmp.mv.GetType().ToString()].AutopilotVoice.Value;
                    VoiceManager.UpdateDefaultVoice(tmp.mv, voiceName);
                    tmp.SetVoice(VoiceManager.GetVoice(voiceName));
                }
            }
        }
        internal static void GrabNewEngineSounds(object sender, EventArgs e)
        {
            if (Player.main != null)
            {
                foreach (var tmp in EngineSoundsManager.engines.Where(x => x != null && x.mv != null & x.mv.GetComponent<TechTag>() != null))
                {
                    string soundsName = VehicleConfig.main[tmp.mv.GetType().ToString()].EngineSounds.Value;
                    EngineSoundsManager.UpdateDefaultVoice(tmp.mv, soundsName);
                    tmp.SetEngineSounds(EngineSoundsManager.GetVoice(soundsName));
                }
            }
        }
        internal static void Setup(ModVehicle mv)
        {
            config = MainPatcher.Instance.Config;
            var vConf = VehicleConfig.GetConfig(mv);
            string vehicleName = mv.GetType().ToString();
            vConf.AutopilotVolume = config.Bind<float>(vehicleName, "Autopilot Volume", 0.5f, new ConfigDescription("How loud is the autopilot voice", new AcceptableValueRange<float>(0, 1)));
            vConf.EngineVolume = config.Bind<float>(vehicleName, "Engine Volume", 0.5f, new ConfigDescription("How loud are the engine sounds", new AcceptableValueRange<float>(0, 1)));

            vConf.AutopilotVoice = config.Bind<string>(vehicleName, "Autopilot Voice", VoiceManager.vehicleVoices.Select(x => x.Key).FirstOrDefault(), new ConfigDescription("Choose an autopilot voice for this vehicle", new AcceptableValueList<string>(VoiceManager.vehicleVoices.Select(x => x.Key).ToArray())));
            vConf.AutopilotVoice.SettingChanged += GrabNewVoiceLines;

            vConf.EngineSounds = config.Bind<string>(vehicleName, "Engine Sounds", EngineSoundsManager.EngineSoundss.Select(x => x.Key).FirstOrDefault(), new ConfigDescription("Choose engine sounds for this vehicle", new AcceptableValueList<string>(EngineSoundsManager.EngineSoundss.Select(x => x.Key).ToArray())));
            vConf.EngineSounds.SettingChanged += GrabNewEngineSounds;

            vConf.HUDChoice = config.Bind<uGUI_VehicleHUD.HUDChoice>(vehicleName, "HUD Choice", uGUI_VehicleHUD.HUDChoice.Storage, "Choose a HUD option for this vehicle");

            vConf.NumUpgrades= config.Bind<int>(vehicleName, "Number of Upgrade Slots", mv.NumModules, new ConfigDescription("How many upgrades can this vehicle use? (restart required)", new AcceptableValueRange<int>(0, ModuleBuilder.MaxNumModules)));
            vConf.IsArms = config.Bind<bool>(vehicleName, "Enable Arm Slots", mv.HasArms, new ConfigDescription("Can this vehicle use arm upgrades? (restart required)"));

            Admin.ExternalVehicleConfig<bool>.GetModVehicleConfig(mv.name);
            Admin.ExternalVehicleConfig<float>.GetModVehicleConfig(mv.name);
            Admin.ExternalVehicleConfig<KeyboardShortcut>.GetModVehicleConfig(mv.name);
        }

        internal static void SetupGeneral(ConfigFile config)
        {
            SetupAccessibilityOptions(config);
            SetupGeneralOptions(config);
            SetupGeneralKeybinds(config);
            SetupCameraKeys(config);
            SetupCheats(config);
        }

        private static void SetupCheats(ConfigFile config)
        {
            void UnlockDroneStation(object sender, EventArgs e)
            {
                DevConsole.SendConsoleCommand("unlock dronestation");
            }
            MainPatcher.VFConfig.UnlockDroneStation = config.Bind<bool>("!Cheats", "Unlock Drone Station", false, "Toggle this (on or off) to unlock the drone station for the current game.");
            MainPatcher.VFConfig.UnlockDroneStation.SettingChanged += UnlockDroneStation;
        }

        private static void SetupGeneralKeybinds(ConfigFile config)
        {
            MainPatcher.VFConfig.MagnetBootsButton = config.Bind<KeyboardShortcut>("!Keybinds", "Magnet Boots Toggle", new KeyboardShortcut(KeyCode.G), "Press this button to activate or deactivate magnet boots.");
            MainPatcher.VFConfig.HeadlightsButton = config.Bind<KeyboardShortcut>("!Keybinds", "Headlights Toggle", new KeyboardShortcut(KeyCode.Mouse1), "Press this button to toggle vehicle headlights while piloting.");
            MainPatcher.VFConfig.LeftClickHeadlights = config.Bind<bool>("!Keybinds", "Headlights Left-click", false, "Enable using left-click(or left gamepad trigger) to activate headlights.");
            MainPatcher.VFConfig.RightClickHeadlights = config.Bind<bool>("!Keybinds", "Headlights Right-click", false, "Enable using right-click (or right gamepad trigger) to activate headlights.");
        }

        private static void SetupGeneralOptions(ConfigFile config)
        {
            MainPatcher.VFConfig.IsDebugLogging = config.Bind<bool>("!General", "Debug Logs", false, "Enable extra logging statements for debugging purposes");
            MainPatcher.VFConfig.ForceArmsCompat = config.Bind<bool>("!General", "Force Arms Slots", false, "Create Arm Upgrade Slots for vehicles that aren't setup for it.");
        }

        private static void SetupAccessibilityOptions(ConfigFile config)
        {
            MainPatcher.VFConfig.IsFlashingLights = config.Bind<bool>("!Accessibility", "Flashing Lights", false, "Enable blinking lights on vehicle exteriors (navigation lights)");
            MainPatcher.VFConfig.IsSubtitles = config.Bind<bool>("!Accessibility", "Autopilot Subtitles", true, "Enable subtitles for the autopilot voice lines");
        }
        private static void SetupCameraKeys(ConfigFile config)
        {
            MainPatcher.VFConfig.NextCamera = config.Bind<KeyboardShortcut>("!Keybinds", "Camera Next", new KeyboardShortcut(KeyCode.F), "Press this button to switch to the next vehicle camera.");
            MainPatcher.VFConfig.PreviousCamera = config.Bind<KeyboardShortcut>("!Keybinds", "Camera Previous", new KeyboardShortcut(KeyCode.T), "Press this button to switch to the previous vehicle camera.");
            MainPatcher.VFConfig.ExitCamera = config.Bind<KeyboardShortcut>("!Keybinds", "Camera Exit", new KeyboardShortcut(KeyCode.V), "Press this button to exit vehicle camera view.");
        }
        internal ConfigEntry<KeyboardShortcut> NextCamera { get; set; }
        internal ConfigEntry<KeyboardShortcut> PreviousCamera { get; set; }
        internal ConfigEntry<KeyboardShortcut> ExitCamera { get; set; }
        internal ConfigEntry<KeyboardShortcut> MagnetBootsButton { get; set; }
        internal ConfigEntry<KeyboardShortcut> HeadlightsButton { get; set; }
        internal ConfigEntry<bool> LeftClickHeadlights { get; set; }
        internal ConfigEntry<bool> RightClickHeadlights { get; set; }
        internal ConfigEntry<bool> IsFlashingLights { get; set; }
        internal ConfigEntry<bool> IsSubtitles { get; set; }
        internal ConfigEntry<bool> IsDebugLogging { get; set; }
        internal ConfigEntry<bool> ForceArmsCompat { get; set; }
        internal ConfigEntry<bool> UnlockDroneStation { get; set; }
    }
}
