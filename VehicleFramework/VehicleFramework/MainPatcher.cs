using BepInEx;
using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VehicleFramework.Admin;
using VehicleFramework.Patches.CompatibilityPatches;
using VehicleFramework.VehicleBuilding;
using static HandReticle;

namespace VehicleFramework
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, "1.0.0.32")]
    public class MainPatcher : BaseUnityPlugin
    {
        public static MainPatcher Instance { get; private set; } = null!;
        internal static VFConfig VFConfig { get; private set; } = null!;
        internal static VehicleFrameworkNautilusConfig NautilusConfig { get; private set; } = null!;

        internal Coroutine? GetVoices = null;
        internal Coroutine? GetEngineSounds = null;

        public void Awake()
        {
            Nautilus.Handlers.LanguageHandler.RegisterLocalizationFolder();
            SetupInstance();
            VehicleFramework.Logger.MyLog = base.Logger;
            if (VehicleFramework.Logger.MyLog == null)
            {
                throw Admin.SessionManager.Fatal("VehicleFramework.Logger.MyLog == null in Awake!");
            }
            if (Instance == null)
            {
                throw Admin.SessionManager.Fatal("Instance == null in Awake!");
            }
            VFConfig = new();
            if (VFConfig == null)
            {
                throw Admin.SessionManager.Fatal("VFConfig == null in Awake!");
            }
            NautilusConfig = Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions<VehicleFrameworkNautilusConfig>();
            if(NautilusConfig == null)
            {
                throw Admin.SessionManager.Fatal("NautilusConfig == null in Awake!");
            }
            PrePatch();
        }
        public void Start()
        {
            Patch();
            PostPatch();
            CompatChecker.CheckAll();
            Admin.SessionManager.StartCoroutine(VehicleFramework.Logger.MakeAlerts());
        }
        public void PrePatch()
        {
            Admin.EnumHelper.Setup();
            Assets.StaticAssets.SetupDefaultAssets();
            Assets.StaticAssets.GetSprites();
            Assets.VFFabricator.CreateAndRegister();
            Admin.CraftTreeHandler.AddFabricatorMenus();
            Admin.Utils.RegisterDepthModules();
            GetVoices = Admin.SessionManager.StartCoroutine(Admin.VoiceManager.LoadAllVoices());
            GetEngineSounds = Admin.SessionManager.StartCoroutine(Admin.EngineSoundsManager.LoadAllVoices());
        }
        public void Patch()
        {
            Nautilus.Utility.SaveUtils.RegisterOnSaveEvent(() => 
            {
                try
                {
                    VehicleRootComponents.MagnetBoots.DetachAll();
                }
                catch (Exception ex)
                {
                    VehicleFramework.Logger.LogException("Failed to detach all magnet boots!", ex);
                }
            });

            void SetWorldNotLoaded()
            {
                Admin.GameStateWatcher.IsWorldLoaded = false;
                ModuleBuilder.Reset();
            }
            void SetWorldLoaded()
            {
                Admin.GameStateWatcher.IsWorldLoaded = true;

                foreach (var vehicleTechType in VehicleManager.vehicleTypes.Where(x => x.mv != null).Select(x => x.techType).Where(x => !PDAScanner.ContainsCompleteEntry(x)))
                {
                    if (!PDAScanner.GetPartialEntryByKey(vehicleTechType, out PDAScanner.Entry entry))
                    {
                        entry = PDAScanner.Add(vehicleTechType, 0);
                    }
                }
            }
            void OnLoadOnce()
            {

            }
            Nautilus.Utility.SaveUtils.RegisterOnQuitEvent(SetWorldNotLoaded);
            WaitScreenHandler.RegisterLateLoadTask(PluginInfo.PLUGIN_NAME, x=>SetWorldLoaded());
            Nautilus.Utility.SaveUtils.RegisterOneTimeUseOnLoadEvent(OnLoadOnce);

            Harmony harmony = new(PluginInfo.PLUGIN_GUID);
            var assembly = typeof(MainPatcher).Assembly;
            var patches =
                AccessTools
                    .GetTypesFromAssembly(assembly)
                    .Select(x => (Type: x, Processor: harmony.CreateClassProcessor(x)))
                    .ToList();
            //VehicleFramework.Logger.Log($"Identified {patches.Count} types to potentially patch. Patching...");
            foreach (var patch in patches)
                //log.Write($"Executing patch {patch.Type}...");
                try
                {
                    patch.Processor.Patch();
                }
                catch (Exception e)
                {
                    VehicleFramework.Logger.LogException($"Failed to patch {patch.Type}.", e);
                }

            // Patch SubnauticaMap with appropriate ping sprites, lest it crash.
            try
            {
                var type = Type.GetType("SubnauticaMap.PingMapIcon, SubnauticaMap", false, false);
                if (type != null)
                {
                    var pingOriginal = AccessTools.Method(type, "Refresh");
                    HarmonyMethod pingPrefix = new(AccessTools.Method(typeof(MapModPatcher), nameof(MapModPatcher.Prefix)));
                    harmony.Patch(pingOriginal, pingPrefix);
                }
            }
            catch(Exception e)
            {
                VehicleFramework.Logger.LogException($"Failed to patch SubnauticaMap.Refresh", e);
            }

            // Patch SlotExtender, lest it break or break us
            try
            {
                var type2 = Type.GetType("SlotExtender.Patches.uGUI_Equipment_Awake_Patch, SlotExtender", false, false);
                if (type2 != null)
                {
                    var awakePreOriginal = AccessTools.Method(type2, "Prefix");
                    HarmonyMethod awakePrefix = new(AccessTools.Method(typeof(SlotExtenderPatcher), nameof(SlotExtenderPatcher.PrePrefix)));
                    harmony.Patch(awakePreOriginal, awakePrefix);

                    var awakePostOriginal = AccessTools.Method(type2, "Postfix");
                    HarmonyMethod awakePostfix = new(AccessTools.Method(typeof(SlotExtenderPatcher), nameof(SlotExtenderPatcher.PrePostfix)));
                    harmony.Patch(awakePostOriginal, awakePostfix);
                }
            }
            catch (Exception e)
            {
                VehicleFramework.Logger.LogException($"Failed to patch SlotExtender.uGUI_Equipment_Awake_Patch", e);
            }

            // Patch BetterVehicleStorage to add ModVehicle compat
            try
            {
                var type3 = Type.GetType("BetterVehicleStorage.Managers.StorageModuleMgr, BetterVehicleStorage", false, false);
                if (type3 != null)
                {
                    var AllowedToAddOriginal = AccessTools.Method(type3, "AllowedToAdd");
                    HarmonyMethod AllowedToAddPrefix = new(AccessTools.Method(typeof(BetterVehicleStoragePatcher), nameof(BetterVehicleStoragePatcher.Prefix)));
                    harmony.Patch(AllowedToAddOriginal, AllowedToAddPrefix);
                }
            }
            catch (Exception e)
            {
                VehicleFramework.Logger.LogException($"Failed to patch BetterVehicleStorage.AllowedToAdd", e);
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
        public static void PostPatch()
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
