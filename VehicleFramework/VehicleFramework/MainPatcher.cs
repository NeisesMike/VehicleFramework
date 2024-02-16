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
using Nautilus.Options.Attributes;
using Nautilus.Options;
using Nautilus.Json;
using Nautilus.Handlers;
using Nautilus.Utility;
using Nautilus.Json.Attributes;
using VehicleFramework.UpgradeModules;
using Nautilus.Assets;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using UnityEngine.SceneManagement;

using techtype = System.String;
using upgrades = System.Collections.Generic.Dictionary<string, System.String>;
using batteries = System.Collections.Generic.List<System.Tuple<System.String, float>>;
using innateStorages = System.Collections.Generic.List<System.Tuple<UnityEngine.Vector3, System.Collections.Generic.List<System.Tuple<System.String, float>>>>;
using modularStorages = System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<System.Tuple<System.String, float>>>>;
using color = System.Tuple<float, float, float, float>;

namespace VehicleFramework
{
    public static class Logger
    {
        internal static ManualLogSource MyLog { get; set; }
        public static void Log(string message)
        {
            MyLog.LogInfo(message);
        }
        public static void Warn(string message)
        {
            MyLog.LogWarning(message);
        }
        public static void Error(string message)
        {
            MyLog.LogError(message);
        }
        public static void DebugLog(string message)
        {
            if (MainPatcher.VFConfig.isDebugLogging)
            {
                MyLog.LogInfo("[VehicleFramework] " + message);
            }
        }
        public static void Output(string msg, int x=500, int y=0)
        {
            BasicText message = new BasicText(x, y);
            message.ShowMessage(msg, 4);
        }
        public static void Output(string msg, float time, int x = 500, int y = 0)
        {
            BasicText message = new BasicText(x, y);
            message.ShowMessage(msg, time);
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

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, Nautilus.PluginInfo.PLUGIN_VERSION)]
    public class MainPatcher : BaseUnityPlugin
    {

        internal static VehicleFrameworkConfig VFConfig { get; private set; }
        internal static SaveData VehicleSaveData { get; private set; }
        internal static Atlas.Sprite ModVehicleIcon { get; private set; }

        internal static List<AutoPilotVoice> voices = new List<AutoPilotVoice>();

        public void Awake()
        {
            VehicleFramework.Logger.MyLog = base.Logger;
            GetAssets();
            SetupDefaultAssets();
            BuildableDroneStation.Register();
            PrePatch();
        }

        public void Start()
        {
            Patch();
            PostPatch();
        }
        public void PrePatch()
        {
            VFConfig = OptionsPanelHandler.RegisterModOptions<VehicleFrameworkConfig>();
            IEnumerator CollectPrefabsForBuilderReference()
            {
                CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.BaseUpgradeConsole, true);
                yield return request;
                VehicleBuilder.upgradeconsole = request.GetResult();

                yield break;
            }
            StartCoroutine(CollectPrefabsForBuilderReference());

            // grab the icon image
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            byte[] spriteBytes = System.IO.File.ReadAllBytes(Path.Combine(modPath, "ModVehicleIcon.png"));
            Texture2D SpriteTexture = new Texture2D(128, 128);
            SpriteTexture.LoadImage(spriteBytes);
            Sprite mySprite = Sprite.Create(SpriteTexture, new Rect(0.0f, 0.0f, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            ModVehicleIcon = new Atlas.Sprite(mySprite);

            // patch in the crafting node for the vehicle upgrade menu
            string[] stepsToDepthTab = { "MVUM" };
            CraftTreeHandler.AddTabNode(CraftTree.Type.SeamothUpgrades, stepsToDepthTab[0], LocalizationManager.GetString(EnglishString.MVModules), MainPatcher.ModVehicleIcon);
            CraftTreeHandler.AddTabNode(CraftTree.Type.SeamothUpgrades, "MVDM", LocalizationManager.GetString(EnglishString.MVDepthModules), MainPatcher.ModVehicleIcon, stepsToDepthTab);
            CraftTreeHandler.AddTabNode(CraftTree.Type.SeamothUpgrades, "MVCM", "Mod Vehicle Common Modules", MainPatcher.ModVehicleIcon, stepsToDepthTab);


            //string[] stepsToMVTab = { "SeamothMenu" };
            //CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "ModVehicle", "ModVehicle Modules", ModVehicleIcon, stepsToMVTab);

            // patch in the depth module upgrades
            ModulePrepper.RegisterModVehicleDepthModules();
        }

        public void Patch()
        {
            SaveData saveData = SaveDataHandler.RegisterSaveDataCache<SaveData>();

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

            void SetWorldNotLoaded()
            {
                VehicleManager.isWorldLoaded = false;
            }
            void SetWorldLoaded()
            {
                VehicleManager.isWorldLoaded = true;
                VoiceManager.voices.ForEach(x => x.NotifyReadyToSpeak());
            }
            Nautilus.Utility.SaveUtils.RegisterOnQuitEvent(SetWorldNotLoaded);
            Nautilus.Utility.SaveUtils.RegisterOnFinishLoadingEvent(SetWorldLoaded);

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
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
            var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
            if (type2 != null)
            {
                var awakePreOriginal = AccessTools.Method(type2, "Prefix");
                var awakePrefix = new HarmonyMethod(AccessTools.Method(typeof(Patches.CompatibilityPatches.SlotExtenderPatcher), "PrePrefix"));
                harmony.Patch(awakePreOriginal, awakePrefix);

                var awakePostOriginal = AccessTools.Method(type2, "Postfix");
                var awakePostfix = new HarmonyMethod(AccessTools.Method(typeof(Patches.CompatibilityPatches.SlotExtenderPatcher), "PrePostfix"));
                harmony.Patch(awakePostOriginal, awakePostfix);
            }

            /*
            var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
            if (type2 != null)
            {
                // Example of assigning a static field in another mod
                var type3 = Type.GetType("SlotExtender.Main, SlotExtender", false, false);
                var but = AccessTools.StaticFieldRefAccess<bool>(type3, "uGUI_PostfixComplete");
                Logger.Log("but was " + but.ToString());
                but = false;
                // example of calling another mod's function
                var awakeOriginal = AccessTools.Method(type2, "Prefix");
                object dummyInstance = null;
                awakeOriginal.Invoke(dummyInstance, new object[] { equipment });
                //Patches.CompatibilityPatches.SlotExtenderPatcher.hasGreenLight = false;
            }
            */

            // do this here because it happens only once
            SceneManager.sceneUnloaded += Admin.GameStateWatcher.OnResetScene;
        }

        public void PostPatch()
        {
            //VehicleBuilder.ScatterDataBoxes(craftables);
        }

        public static void GetAssets()
        {
            // load the asset bundle
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modPath, "modvehiclepingsprite"));
            if (myLoadedAssetBundle == null)
            {
                VehicleFramework.Logger.Error("Failed to load AssetBundle.");
                return;
            }
            System.Object[] arr = myLoadedAssetBundle.LoadAllAssets();
            foreach (System.Object obj in arr)
            {
                if (obj.ToString().Contains("ModVehicleSpriteAtlas"))
                {
                    UnityEngine.U2D.SpriteAtlas thisAtlas = (UnityEngine.U2D.SpriteAtlas)obj;
                    Sprite ping = thisAtlas.GetSprite("ModVehiclePingSprite");
                    VehicleManager.defaultPingSprite = new Atlas.Sprite(ping);
                    return;
                }
                else
                {
                    //Logger.Log(obj.ToString());
                }
            }
            VehicleFramework.Logger.Error("Failed to retrieve PingSprite from asset bundle.");
        }
        public static void SetupDefaultAssets()
        {
            VehicleManager.defaultRecipe.Add(TechType.PlasteelIngot, 1);
            VehicleManager.defaultRecipe.Add(TechType.Lubricant, 1);
            VehicleManager.defaultRecipe.Add(TechType.ComputerChip, 1);
            VehicleManager.defaultRecipe.Add(TechType.AdvancedWiringKit, 1);
            VehicleManager.defaultRecipe.Add(TechType.Lead, 2);
            VehicleManager.defaultRecipe.Add(TechType.EnameledGlass, 2);

            VehicleManager.defaultEngine = new Engines.AtramaEngine();
        }

        public static List<Action<Player>> VFPlayerStartActions = new List<Action<Player>>();
    }


    [Menu("Vehicle Framework Options")]
    public class VehicleFrameworkConfig : ConfigFile
    {
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
                foreach (var tmp in MainPatcher.voices)
                {
                    tmp.TryGetAllAudioClips(voiceChoice);
                }
            }
        }

        [Slider("Engine Volume", Step = 1f, DefaultValue = 100, Min = 0, Max = 100)]
        public float engineVolume = 50f;
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
        
        public List<Tuple<Vector3, string, color, color, color, color, bool>> AllVehiclesAesthetics { get; set; }
    }
}
