using Nautilus.Options.Attributes;
using Nautilus.Json;
using UnityEngine;

namespace VehicleFramework
{
    [Menu("Vehicle Framework Options")]
    public class VehicleFrameworkConfig : ConfigFile
    {
        [Toggle("Crafting Menu Fix", Tooltip = "Nautilus-33 allows some crafting tabs to be completely obscured by others. If you're having trouble, enable this.")]
        public bool isCraftingMenuFix = false;

        [Toggle("Flashing Lights")]
        public bool isFlashingLightsEnabled = false;

        [Slider("AI Voice Volume", Step = 1f, DefaultValue = 100, Min = 0, Max = 100)]
        public float aiVoiceVolume = 50f;

        [Toggle("Enable Debug Logs")]
        public bool isDebugLogging = false;

        [Choice("Autopilot Voice", Options = new[] { "ShirubaFoxy", "Chels-E", "Mikjaw", "Turtle", "Salli" }), OnChange(nameof(GrabNewVoiceLines))]
        public string voiceChoice = "ShirubaFoxy";
        public void GrabNewVoiceLines()
        {
            if (Player.main != null)
            {
                foreach (var tmp in VoiceManager.voices)
                {
                    tmp.SetVoice(VoiceManager.GetVoice(voiceChoice));
                }
            }
        }

        [Choice("Engine Sounds", Options = new[] { "ShirubaFoxy" }), OnChange(nameof(GrabNewEngineSounds))]
        public string engineSounds = "ShirubaFoxy";
        public void GrabNewEngineSounds()
        {
            if (Player.main != null)
            {
                foreach (var tmp in EngineSoundsManager.engines)
                {
                    tmp.SetVoice(EngineSoundsManager.GetVoice(engineSounds));
                }
            }
        }

        [Slider("Engine Volume", Step = 1f, DefaultValue = 100, Min = 0, Max = 100)]
        public float engineVolume = 50f;

        [Keybind("Next Camera")]
        public KeyCode nextCamera = KeyCode.F;

        [Keybind("Previous Camera")]
        public KeyCode previousCamera = KeyCode.T;

        [Keybind("Exit Camera")]
        public KeyCode exitCamera = KeyCode.V;

        [Keybind("Magnet Boots", Tooltip = "Certain vehicles can cling to exterior surfaces, such as bases or other Submarines.")]
        public KeyCode magnetBoots = KeyCode.G;

        [Keybind("Toggle Headlights")]
        public KeyCode headlightsButton = KeyCode.Mouse1;

        [Toggle("Fragment Experience", Tooltip = "Enable scannable fragments. Leave unchecked if adding this mod to an existing world. Requires Subnautica reboot when changed.")]
        public bool isFragmentExperience = true;

        [Toggle("Force Arms Compat", Tooltip = "Force arm slots onto every vehicle. Otherwise, only certain vehicles will accept arm upgrades. Requires reload to take effect. Disabling this option while a 'forced arms' vehicle has an arm will cause that upgrade to be inaccessible until you re-enable this option.")]
        public bool forceArmsCompat = false;

        public const string StorageNone = "None";
        public const string StorageDrone = "Drones";
        public const string StorageAll = "All";
        [Choice("Storage HUD", Options = new[] { StorageAll, StorageDrone, StorageNone }, Tooltip = "Display storage HUD for vehicles with storage, only for drones, or never.")]
        public string storageHudChoice = StorageAll;

    }
}
