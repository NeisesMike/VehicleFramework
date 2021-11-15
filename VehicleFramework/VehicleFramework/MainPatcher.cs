using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Collections;
using SMLHelper.V2.Options.Attributes;
using SMLHelper.V2.Options;
using SMLHelper.V2.Json;
using SMLHelper.V2.Handlers;
using QModManager.API.ModLoading;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Json.Attributes;
using VehicleFramework.UpgradeModules;
using SMLHelper.V2.Assets;

using upgrades = System.Collections.Generic.Dictionary<string, TechType>;
using innateStorages = System.Collections.Generic.List<System.Tuple<UnityEngine.Vector3, System.Collections.Generic.List<TechType>>>;
using modularStorages = System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<TechType>>>;
using batteries = System.Collections.Generic.List<System.Tuple<TechType, float>>;

namespace VehicleFramework
{
    public static class Logger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log("[VehicleFramework] " + message);
        }
        public static void DebugLog(string message)
        {
            if (MainPatcher.Config.isDebugLogging)
            {
                UnityEngine.Debug.Log("[VehicleFramework] " + message);
            }
        }
        public static void Log(string format, params object[] args)
        {
            UnityEngine.Debug.Log("[VehicleFramework] " + string.Format(format, args));
        }
        public static void Output(string msg)
        {
            BasicText message = new BasicText(500, 0);
            message.ShowMessage(msg, 5);
        }
        public static void OutputLong(string msg)
        {
            BasicText message = new BasicText(0, 100);
            message.ShowMessage(msg, 5);
        }
        public static void Narrate(string msg)
        {
            BasicText message = new BasicText(0, -100);
            message.ShowMessage(msg, 2);
        }
    }
    [QModCore]
    public static class MainPatcher
    {
        internal static VehicleFrameworkConfig Config { get; private set; }
        internal static SaveData VehicleSaveData { get; private set; }
        internal static Atlas.Sprite ModVehicleIcon { get; private set; }

        internal static ModVehicleDepthMk1 modVehicleDepthModule1 = new ModVehicleDepthMk1();
        internal static ModVehicleDepthMk2 modVehicleDepthModule2 = new ModVehicleDepthMk2();
        internal static ModVehicleDepthMk3 modVehicleDepthModule3 = new ModVehicleDepthMk3();

        internal static List<AutoPilotVoice> voices = new List<AutoPilotVoice>();

        [QModPrePatch]
        public static void PrePatch()
        {
            // patch in the crafting node for the Workbench menu (modification station)
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            byte[] spriteBytes = System.IO.File.ReadAllBytes(Path.Combine(modPath, "ModVehicleIcon.png"));
            Texture2D SpriteTexture = new Texture2D(128, 128);
            SpriteTexture.LoadImage(spriteBytes);
            Sprite mySprite = Sprite.Create(SpriteTexture, new Rect(0.0f, 0.0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            ModVehicleIcon = new Atlas.Sprite(mySprite);
            string[] stepsToMVTab = { "SeamothMenu" };
            CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "ModVehicle", "ModVehicle Modules", ModVehicleIcon, stepsToMVTab);

            // patch in the depth module upgrades
            modVehicleDepthModule1.Patch();
            modVehicleDepthModule2.Patch();
            modVehicleDepthModule3.Patch();
        }

        [QModPatch]
        public static void Patch()
        {
            SaveData saveData = SaveDataHandler.Main.RegisterSaveDataCache<SaveData>();

            // Update the player position before saving it
            saveData.OnStartedSaving += (object sender, JsonFileEventArgs e) =>
            {
                VehicleManager.SaveVehicles(sender, e);
            };

            saveData.OnFinishedSaving += (object sender, JsonFileEventArgs e) =>
            {
                //SaveData data = e.Instance as SaveData;
                //Logger.Output(VehicleManager.VehiclesInPlay.Count.ToString());
                //Logger.Output(data.UpgradeList.Keys.ToString());
            };

            saveData.OnFinishedLoading += (object sender, JsonFileEventArgs e) =>
            {
                VehicleSaveData = e.Instance as SaveData; 
            };

            Config = OptionsPanelHandler.Main.RegisterModOptions<VehicleFrameworkConfig>();
            var harmony = new Harmony("com.mikjaw.subnautica.vehicleframework.mod");
            harmony.PatchAll();

            // Patch SubnauticaMap with appropriate ping sprites, lest it crash.
            var type = Type.GetType("SubnauticaMap.PingMapIcon, SubnauticaMap", false, false);
            if (type != null)
            {
                var pingOriginal = AccessTools.Method(type, "Refresh");
                var pingPrefix = new HarmonyMethod(AccessTools.Method(typeof(Patches.CompatibilityPatches.MapModPatcher), "Prefix"));
                harmony.Patch(pingOriginal, pingPrefix);
            }

            // Patch SlotExtender, lest it break or break us
            /*
            var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
            if (type2 != null)
            {
                var awakeOriginal = AccessTools.Method(type2, "Prefix");
                var awakePrefix = new HarmonyMethod(AccessTools.Method(typeof(Patches.CompatibilityPatches.SlotExtenderPatcher), "PrePrefix"));
                harmony.Patch(awakeOriginal, awakePrefix);
            }
            */
        }

        [QModPostPatch]
        public static void PostPatch()
        {
            List<VehicleCraftable> craftables = VehicleManager.PatchCraftables();
            //VehicleBuilder.ScatterDataBoxes(craftables);
        }
    }


    [Menu("Vehicle Framework Options")]
    public class VehicleFrameworkConfig : ConfigFile
    {
        [Toggle("Flashing Lights")]
        public bool isFlashingLightsEnabled = false;

        [Slider("AI Voice Volume", Step = 1f, DefaultValue = 100, Min = 0, Max = 100)]
        public float aiVoiceVolume = 100f;

        [Toggle("Enable Debug Logs")]
        public bool isDebugLogging = false;

        [Choice("Autopilot Voice", "Mikjaw", "Salli"), OnChange(nameof(GrabNewVoiceLines))]
        public string voiceChoice = "Mikjaw";

        public void GrabNewVoiceLines()
        {
            foreach(var tmp in MainPatcher.voices)
            {
                tmp.TryGetAllAudioClips(voiceChoice);
            }
        }
    }


    [FileName("vehicle_storage")]
    internal class SaveData : SaveDataCache
    {
        public List<Tuple<Vector3, bool>> IsPlayerInside { get; set; }
        public List<Tuple<Vector3, upgrades>> UpgradeLists { get; set; }
        public List<Tuple<Vector3, innateStorages>> InnateStorages { get; set; }
        public List<Tuple<Vector3, modularStorages>> ModularStorages { get; set; }
        
        public List<Tuple<Vector3, batteries>> Batteries { get; set; }
        public List<Tuple<Vector3, batteries>> BackupBatteries { get; set; }

        // todo: maybe this?
        // save a few lines in the output json?
        public List<Tuple<Vector3, Tuple<upgrades, innateStorages, modularStorages, batteries>>> AllVehiclesStorages { get; set; }
    }
}
