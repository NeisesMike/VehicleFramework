using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using BepInEx;
using UnityEngine.SceneManagement;
using Nautilus.Handlers;

namespace VehicleFramework
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, "1.0.0.32")]
    public class MainPatcher : BaseUnityPlugin
    {
        public static MainPatcher Instance { get; private set; }
        internal static SaveLoad.SaveData SaveFileData { get; private set; }

        internal static VFConfig VFConfig { get; private set; }
        internal static VehicleFrameworkNautilusConfig NautilusConfig { get; private set; }

        internal Coroutine GetVoices = null;
        internal Coroutine GetEngineSounds = null;

        public void Awake()
        {
            Nautilus.Handlers.LanguageHandler.RegisterLocalizationFolder();
            SetupInstance();
            VFConfig = new();
            NautilusConfig = Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions<VehicleFrameworkNautilusConfig>();
            VehicleFramework.Logger.MyLog = base.Logger;
            PrePatch();
        }
        public void Start()
        {
            Patch();
            PostPatch();
            CompatChecker.CheckAll();
            MainPatcher.Instance.StartCoroutine(VehicleFramework.Logger.MakeAlerts());
        }
        public void PrePatch()
        {
            Admin.EnumHelper.Setup();
            Assets.StaticAssets.SetupDefaultAssets();
            Assets.StaticAssets.GetSprites();
            Assets.VFFabricator.CreateAndRegister();
            Admin.CraftTreeHandler.AddFabricatorMenus();
            Admin.Utils.RegisterDepthModules();
            GetVoices = MainPatcher.Instance.StartCoroutine(VoiceManager.LoadAllVoices());
            GetEngineSounds = MainPatcher.Instance.StartCoroutine(EngineSoundsManager.LoadAllVoices());
        }
        public void Patch()
        {
            SaveLoad.SaveData saveData = Nautilus.Handlers.SaveDataHandler.RegisterSaveDataCache<SaveLoad.SaveData>();

            // Update the player position before saving it
            saveData.OnStartedSaving += (object sender, Nautilus.Json.JsonFileEventArgs e) =>
            {
                try
                {
                    VehicleComponents.MagnetBoots.DetachAll();
                }
                catch (Exception ex)
                {
                    VehicleFramework.Logger.LogException("Failed to detach all magnet boots!", ex);
                }
                try
                {
                    VehicleManager.CreateSaveFileData(sender, e);
                }
                catch(Exception ex)
                {
                    VehicleFramework.Logger.LogException("Failed to Create Save File Data!", ex);
                }
            };

            saveData.OnFinishedSaving += (object sender, Nautilus.Json.JsonFileEventArgs e) =>
            {
                //VehicleComponents.MagnetBoots.AttachAll();
            };

            saveData.OnFinishedLoading += (object sender, Nautilus.Json.JsonFileEventArgs e) =>
            {
                SaveFileData = e.Instance as SaveLoad.SaveData;
            };

            void SetWorldNotLoaded()
            {
                Admin.GameStateWatcher.isWorldLoaded = false;
                ModuleBuilder.haveWeCalledBuildAllSlots = false;
                ModuleBuilder.slotExtenderIsPatched = false;
                ModuleBuilder.slotExtenderHasGreenLight = false;
            }
            void SetWorldLoaded()
            {
                Admin.GameStateWatcher.isWorldLoaded = true;
            }
            void OnLoadOnce()
            {

            }
            Nautilus.Utility.SaveUtils.RegisterOnQuitEvent(SetWorldNotLoaded);
            WaitScreenHandler.RegisterLateLoadTask(PluginInfo.PLUGIN_NAME, x=>SetWorldLoaded());
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
        private void SetupInstance()
        {
            if (Instance == null)
            {
                Instance = this;
                return;
            }
            if (Instance != this)
            {
                UnityEngine.Object.Destroy(this);
                return;
            }
        }
    }
}
