using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using Nautilus.Json;
using Nautilus.Handlers;
using BepInEx;
using UnityEngine.SceneManagement;

namespace VehicleFramework
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, "1.0.0.32")]
    public class MainPatcher : BaseUnityPlugin
    {
        internal static VehicleFrameworkConfig VFConfig { get; private set; }
        internal static SaveData VehicleSaveData { get; private set; }
        public static List<Action<Player>> VFPlayerStartActions = new List<Action<Player>>();
        public void Awake()
        {
            VehicleFramework.Logger.MyLog = base.Logger;
            PrePatch();
        }
        public void Start()
        {
            Patch();
            PostPatch();
        }
        public void PrePatch()
        {
            Assets.StaticAssets.SetupDefaultAssets();
            VFConfig = OptionsPanelHandler.RegisterModOptions<VehicleFrameworkConfig>();
            IEnumerator CollectPrefabsForBuilderReference()
            {
                CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.BaseUpgradeConsole, true);
                yield return request;
                VehicleBuilder.upgradeconsole = request.GetResult();
                yield break;
            }
            UWE.CoroutineHost.StartCoroutine(CollectPrefabsForBuilderReference());
            Assets.StaticAssets.GetSprites();
            Admin.CraftTreeHandler.AddFabricatorMenus();
            Admin.Utils.RegisterDepthModules();
            UWE.CoroutineHost.StartCoroutine(VoiceManager.LoadAllVoices());
            UWE.CoroutineHost.StartCoroutine(EngineSoundsManager.LoadAllVoices());
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
                ModuleBuilder.haveWeCalledBuildAllSlots = false;
                ModuleBuilder.slotExtenderIsPatched = false;
                ModuleBuilder.slotExtenderHasGreenLight = false;
            }
            void SetWorldLoaded()
            {
                VehicleManager.isWorldLoaded = true;
            }
            void OnLoadOnce()
            {

            }
            Nautilus.Utility.SaveUtils.RegisterOnQuitEvent(SetWorldNotLoaded);
            Nautilus.Utility.SaveUtils.RegisterOnFinishLoadingEvent(SetWorldLoaded);
            Nautilus.Utility.SaveUtils.RegisterOneTimeUseOnLoadEvent(OnLoadOnce);

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

            // Patch BetterVehicleStorage to add ModVehicle compat
            var type3 = Type.GetType("BetterVehicleStorage.Managers.StorageModuleMgr, BetterVehicleStorage", false, false);
            if (type3 != null)
            {
                var AllowedToAddOriginal = AccessTools.Method(type3, "AllowedToAdd");
                var AllowedToAddPrefix = new HarmonyMethod(AccessTools.Method(typeof(Patches.CompatibilityPatches.BetterVehicleStoragePatcher), "Prefix"));
                harmony.Patch(AllowedToAddOriginal, AllowedToAddPrefix);
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
    }
}
